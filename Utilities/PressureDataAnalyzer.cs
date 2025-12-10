using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SensoreAPPMVC.Models;
using SensoreAPPMVC.Data;

namespace SensoreAPPMVC.Utilities
{
    /// <summary>
    /// Helper for loading and analysing pressure map data.
    /// Loads from database and aggregates by date.
    /// </summary>
    public static class PressureDataAnalyzer
    {
        public const double HighPressureThreshold = 500.0;

        /// <summary>
        /// Loads and analyses all pressure map data for a given patient
        /// from the database, aggregated by date.
        /// </summary>
        public static List<PressureTimePoint> LoadPatientHistoryFromDatabase(
            AppDBContext context,
            int patientId,
            DateTime? from = null,
            DateTime? to = null)
        {
            var query = context.PressureMaps
                .Where(p => p.PatientId == patientId);

            if (from.HasValue)
                query = query.Where(p => p.Timestamp >= from.Value);

            if (to.HasValue)
                query = query.Where(p => p.Timestamp <= to.Value);

            var allRecords = query
                .OrderBy(p => p.Timestamp)
                .ToList();

            if (allRecords.Count == 0)
                return new List<PressureTimePoint>();

            // Group by 6-hour intervals and aggregate each period's data
            var results = allRecords
                .GroupBy(p => 
                {
                    // Round down to nearest 6-hour interval
                    var hour = p.Timestamp.Hour;
                    var sixHourBucket = (hour / 6) * 6;
                    return p.Timestamp.Date.AddHours(sixHourBucket);
                })
                .Select(group =>
                {
                    var periodRecords = group.ToList();
                    var peakPressure = periodRecords.Max(r => r.PeakPressure);
                    var avgContactArea = periodRecords.Average(r => r.ContactAreaPercent);
                    var isHighRisk = peakPressure >= HighPressureThreshold;

                    return new PressureTimePoint
                    {
                        PatientId = patientId,
                        Timestamp = group.Key, // Use the 6-hour bucket timestamp
                        PeakPressure = peakPressure,
                        ContactAreaPercent = avgContactArea,
                        IsHighRisk = isHighRisk
                    };
                })
                .OrderBy(p => p.Timestamp)
                .ToList();

            return results;
        }

        /// <summary>
        /// Splits a full history into current and previous windows.
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
