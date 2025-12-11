using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SensoreAPPMVC.Models;
using SensoreAPPMVC.Data;

namespace SensoreAPPMVC.Utilities
{
    public class UploadHandler
    {
        private readonly AppDBContext _context;

        private const double HIGH_RISK_THRESHOLD = 500.0;
        private const int GRID_SIZE = 32;               // 32×32 matrix
        private const int VALUES_PER_FRAME = 1024;      // 32×32 = 1024

        public UploadHandler(AppDBContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, int RecordCount)> Upload(
            IFormFile file,
            int patientId)
        {
            if (file == null || file.Length == 0)
                return (false, "No file uploaded.", 0);

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return (false, "Only CSV files accepted.", 0);

            try
            {
                var patient = await _context.Patients.FindAsync(patientId);
                if (patient == null)
                    return (false, "Patient not found.", 0);

                // --- Auto rename uploaded file ---
                string uploadsDir = Path.Combine("wwwroot", "uploads");
                Directory.CreateDirectory(uploadsDir);

                string newFileName = $"{patientId}_{DateTime.UtcNow:yyyyMMdd}.csv";
                string savePath = Path.Combine(uploadsDir, newFileName);

                // Save the uploaded CSV to a permanent location
                using (var output = System.IO.File.Create(savePath))
                {
                    await file.CopyToAsync(output);
                }


                // Use timestamp from filename if formatted as yyyyMMdd
                DateTime baseTimestamp = DateTime.UtcNow;
                string fileName = Path.GetFileNameWithoutExtension(file.FileName);

                var parts = fileName.Split('_');
                foreach (var p in parts)
                {
                    if (DateTime.TryParseExact(
                            p,
                            "yyyyMMdd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var parsedDate))
                    {
                        baseTimestamp = parsedDate;
                        break;
                    }
                }

                int recordCount = 0;

                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream);

                string text = await reader.ReadToEndAsync();
                var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                .Where(l => !string.IsNullOrWhiteSpace(l))
                                .ToList();

                if (lines.Count == 0)
                    return (false, "CSV file is empty.", 0);

                // Parse values
                var allValues = new List<double>();
                foreach (var line in lines)
                {
                    var values = line.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                     .Select(v => double.TryParse(
                                         v.Trim(),
                                         NumberStyles.Any,
                                         CultureInfo.InvariantCulture,
                                         out var d) ? d : 0.0)
                                     .ToList();
                    allValues.AddRange(values);
                }

                if (allValues.Count == 0)
                    return (false, "No readable values in CSV.", 0);

                // Slice array into frames of 1024 values
                const int FILE_DURATION_SECONDS = 60;
                int numFrames = (int)Math.Ceiling((double)allValues.Count / VALUES_PER_FRAME);
                double frameInterval = (double)FILE_DURATION_SECONDS / numFrames;

                DateTime currentTimestamp = baseTimestamp;

                for (int frameIndex = 0; frameIndex < numFrames; frameIndex++)
                {
                    int start = frameIndex * VALUES_PER_FRAME;
                    int end = Math.Min(start + VALUES_PER_FRAME, allValues.Count);

                    var frameValues = allValues.Skip(start).Take(end - start).ToList();
                    if (frameValues.Count == 0) continue;

                    // ---- Build 32×32 matrix ----
                    // ---- Build jagged array for JSON ----
                    double[][] grid = new double[GRID_SIZE][];

                    for (int i = 0; i < GRID_SIZE; i++)
                    {
                        grid[i] = new double[GRID_SIZE];

                        for (int j = 0; j < GRID_SIZE; j++)
                        {
                            int k = i * GRID_SIZE + j;
                            double v = (k < frameValues.Count) ? frameValues[k] : 0;
                            grid[i][j] = v > 0 ? v : 0; // zero mask
                        }
                    }

                    string gridJson = JsonSerializer.Serialize(grid);


                    // ---- Metrics ----
                    var peak = frameValues.Max();
                    var nonZeroCount = frameValues.Count(v => v > 0);
                    var contactAreaPercent =
                        frameValues.Count > 0
                        ? (nonZeroCount / (double)frameValues.Count) * 100.0
                        : 0.0;

                    // ---- DB Entity ----
                    var entity = new PressureMap
                    {
                        PatientId = patientId,
                        Timestamp = currentTimestamp,
                        PeakPressure = peak,
                        ContactAreaPercent = contactAreaPercent,
                        IsHighRisk = peak >= HIGH_RISK_THRESHOLD,
                        CreatedAt = DateTime.UtcNow,
                        GridData = gridJson,
                        ClinicianComment = null
                    };

                    _context.PressureMaps.Add(entity);
                    recordCount++;

                    currentTimestamp = currentTimestamp.AddSeconds(frameInterval);
                }

                await _context.SaveChangesAsync();
                return (true, $"Imported {recordCount} frames.", recordCount);
            }
            catch (Exception ex)
            {
                return (false, $"Upload failed: {ex.Message}", 0);
            }
        }
    }
}
