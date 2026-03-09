using System;
using System.Collections.Generic;

namespace TechPro.Models
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public double RevenueChange { get; set; }
        public int TotalTickets { get; set; }
        public int ThisMonthTickets { get; set; }
        public List<RevenueDataPoint> RevenueData { get; set; } = new();
        public List<StatusDataPoint> StatusData { get; set; } = new();
        public List<TopPartDataPoint> TopParts { get; set; } = new();
        public List<TechPerformanceDataPoint> TechPerformance { get; set; } = new();
    }

    public class RevenueDataPoint
    {
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class StatusDataPoint
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class TopPartDataPoint
    {
        public string PartName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class TechPerformanceDataPoint
    {
        public string TechName { get; set; } = string.Empty;
        public int Completed { get; set; }
        public int Total { get; set; }
    }

    public class ReportSummaryViewModel
    {
        public int TotalTickets { get; set; }
        public decimal TotalRevenue { get; set; }
        public double CompletionRate { get; set; }
    }
}
