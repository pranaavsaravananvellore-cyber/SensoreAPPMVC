using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SensoreAPPMVC.Models;

namespace SensoreAPPMVC.Utilities
{
    /// <summary>
    /// Helper for loading and analysing pressure map data stored as CSV files.
    /// Each CSV file represents a chronological stack of 32x32 pressure maps.
    /// For dashboard metrics we aggregate the whole file into a single
    /// PressureTimePoint (peak and average contact area).
    /// </summary>
    public static class PressureDataAnalyzer
    {
        /// <summary>
        /// High pressure threshold for flagging risk events.
        /// This can be made configurable; for now it is a sensible default.
        /// </summary>
        public const double HighPressureThreshold = 500.0;

        /// <summary>
        /// Loads and analyses all pressure map CSV files for a given patient
        /// within an optional date range.
        /// </summary>
        /// <param name="patientId">The numeric patient identifier.</param>
        /// <param name="rootFolder">Root folder where CSV files are stored.</param>
        /// <param name="from">Inclusive start date (optional).</param>
        /// <param name="to">Inclusive end date (optional).</param>
        public static List<PressureTimePoint> LoadPatientHistory(
            int patientId,
            string rootFolder,
            DateTime? from = null,
            DateTime? to = null)
        {
            var results = new List<PressureTimePoint>();

            if (string.IsNullOrWhiteSpace(rootFolder) || !Directory.Exists(rootFolder))
            {
                return results;
            }

            // Expected pattern: {patientId}_yyyyMMdd.csv
            var pattern = patientId.ToString(CultureInfo.InvariantCulture) + "_*.csv";
            var files = Directory.GetFiles(rootFolder, pattern, SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                var date = TryParseDateFromFileName(file);
                if (from.HasValue && date.Date < from.Value.Date) continue;
                if (to.HasValue && date.Date > to.Value.Date) continue;

                var point = AnalyseFile(file, patientId, date);
                if (point != null)
                {
                    results.Add(point);
                }
            }

            return results
                .OrderBy(p => p.Timestamp)
                .ToList();
        }

        private static PressureTimePoint? AnalyseFile(string path, int patientId, DateTime date)
        {
            // Read all lines and split by comma.
            var lines = File.ReadAllLines(path);
            if (lines.Length == 0) return null;

            var values = new List<double>();
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var p in parts)
                {
                    if (double.TryParse(p, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                    {
                        values.Add(v);
                    }
                }
            }

            if (values.Count == 0) return null;

            var totalCells = values.Count;
            var nonZeroCells = values.Count(v => Math.Abs(v) > double.Epsilon);
            var peak = values.Max();

            var contactPercent = totalCells == 0
                ? 0.0
                : (double)nonZeroCells / totalCells * 100.0;

            return new PressureTimePoint
            {
                PatientId = patientId,
                Timestamp = date,
                PeakPressure = peak,
                ContactAreaPercent = contactPercent,
                IsHighRisk = peak >= HighPressureThreshold
            };
        }

        private static DateTime TryParseDateFromFileName(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            // Expect something like "123_20251011"
            var parts = fileName.Split('_', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                var datePart = parts[1];
                if (DateTime.TryParseExact(
                        datePart,
                        "yyyyMMdd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var parsed))
                {
                    return parsed;
                }
            }

            // Fallback: use file creation time.
            return File.GetCreationTime(path);
        }

        /// <summary>
        /// Splits a full history into current and previous windows so that
        /// simple comparison reports can be generated (current vs previous).
        /// </summary>
        public static (List<PressureTimePoint> current, List<PressureTimePoint> previous)
            SplitCurrentAndPrevious(IReadOnlyList<PressureTimePoint> history)
        {
            if (history == null || history.Count == 0)
            {
                return (new List<PressureTimePoint>(), new List<PressureTimePoint>());
            }

            var lastDate = history.Max(p => p.Timestamp).Date;
            var firstDate = history.Min(p => p.Timestamp).Date;
            var totalDays = (lastDate - firstDate).TotalDays + 1;

            // Use the last half of the range as the current window.
            var halfSpan = Math.Max(1, (int)Math.Ceiling(totalDays / 2.0));
            var currentStart = lastDate.AddDays(-halfSpan + 1);

            var current = history
                .Where(p => p.Timestamp.Date >= currentStart)
                .ToList();

            var previous = history
                .Where(p => p.Timestamp.Date < currentStart)
                .ToList();

            return (current, previous);
        }

        /// <summary>
        /// Calculates simple aggregate metrics for a collection of pressure points.
        /// </summary>
        public static (double avgPeak, double avgContact, int highRiskCount)
            Summarise(IEnumerable<PressureTimePoint> points)
        {
            var list = points?.ToList() ?? new List<PressureTimePoint>();
            if (list.Count == 0) return (0, 0, 0);

            var avgPeak = list.Average(p => p.PeakPressure);
            var avgContact = list.Average(p => p.ContactAreaPercent);
            var highRisk = list.Count(p => p.IsHighRisk);

            return (avgPeak, avgContact, highRisk);
        }
    }
}
