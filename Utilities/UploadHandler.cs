using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SensoreAPPMVC.Models;
using SensoreAPPMVC.Data;

namespace SensoreAPPMVC.Utilities
{
    public class UploadHandler
    {
        private readonly AppDBContext _context;
        private const double HIGH_RISK_THRESHOLD = 500.0;
        private const int GRID_SIZE = 32; // 32x32 grid
        private const int VALUES_PER_FRAME = GRID_SIZE * GRID_SIZE; // 1024 values

        public UploadHandler(AppDBContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, int RecordCount)> Upload(
            IFormFile file,
            int patientId)
        {
            if (file == null || file.Length == 0)
                return (false, "No file provided.", 0);

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return (false, "Only CSV files allowed.", 0);

            try
            {
                var patient = await _context.Patients.FindAsync(patientId);
                if (patient == null)
                    return (false, "Patient not found.", 0);

                // Parse date from filename (format: patientid_yyyyMMdd.csv)
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                DateTime baseTimestamp = DateTime.UtcNow;
                
                var parts = fileName.Split('_');
                foreach (var part in parts)
                {
                    if (DateTime.TryParseExact(part, "yyyyMMdd", CultureInfo.InvariantCulture, 
                        DateTimeStyles.None, out var parsedDate))
                    {
                        baseTimestamp = parsedDate;
                        break;
                    }
                }

                int recordCount = 0;

                using (var stream = file.OpenReadStream())
                using (var reader = new StreamReader(stream))
                {
                    var allLines = await reader.ReadToEndAsync();
                    var lines = allLines.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                        .Where(l => !string.IsNullOrWhiteSpace(l))
                        .ToList();

                    if (lines.Count == 0)
                        return (false, "CSV file is empty.", 0);

                    // Parse all values from all lines
                    var allValues = new List<double>();
                    foreach (var line in lines)
                    {
                        var values = line.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(v => double.TryParse(v.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : 0.0)
                            .ToList();
                        allValues.AddRange(values);
                    }

                    if (allValues.Count == 0)
                        return (false, "No valid pressure data found.", 0);

                    // Create one record per 32x32 frame (1024 values)
                    const int FILE_DURATION_SECONDS = 60;
                    int numFrames = (int)Math.Ceiling((double)allValues.Count / VALUES_PER_FRAME);
                    double frameIntervalSeconds = (double)FILE_DURATION_SECONDS / numFrames;

                    var currentTimestamp = baseTimestamp;

                    for (int frameIndex = 0; frameIndex < numFrames; frameIndex++)
                    {
                        int startIndex = frameIndex * VALUES_PER_FRAME;
                        int endIndex = Math.Min(startIndex + VALUES_PER_FRAME, allValues.Count);
                        
                        var frameValues = allValues.Skip(startIndex).Take(endIndex - startIndex).ToList();

                        if (frameValues.Count == 0) continue;

                        var peakPressure = frameValues.Max();
                        var nonZeroCells = frameValues.Count(v => v > 0);
                        var contactAreaPercent = frameValues.Count > 0 
                            ? (double)nonZeroCells / frameValues.Count * 100.0 
                            : 0.0;

                        var pressureMap = new PressureMap
                        {
                            PatientId = patientId,
                            Timestamp = currentTimestamp,
                            PeakPressure = peakPressure,
                            ContactAreaPercent = contactAreaPercent,
                            IsHighRisk = peakPressure >= HIGH_RISK_THRESHOLD,
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.PressureMaps.Add(pressureMap);
                        currentTimestamp = currentTimestamp.AddSeconds(frameIntervalSeconds);
                        recordCount++;
                    }

                    await _context.SaveChangesAsync();
                }

                return (true, $"Imported {recordCount} frames.", recordCount);
            }
            catch (Exception ex)
            {
                return (false, $"Error: {ex.Message}", 0);
            }
        }
    }
}
