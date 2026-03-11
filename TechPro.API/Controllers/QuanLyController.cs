using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechPro.API.Data;
using TechPro.API.Models;

namespace TechPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "StoreAdmin,SystemAdmin")]  // Chỉ quản lý mới xem được báo cáo
    public class QuanLyController : ControllerBase
    {
        private readonly TechProDbContext _context;

        public QuanLyController(TechProDbContext context)
        {
            _context = context;
        }

        [HttpGet("DashboardStats")]
        public async Task<ActionResult<DashboardViewModel>> GetDashboardStats()
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddDays(-1);

            // Revenue Logic (Assuming PhieuSuaChua has TongTien and TrangThai)
            // Note: In real app, check for PaymentStatus. Here simplified to Status = 'completed' or 'delivered'
            var currentMonthRevenue = await _context.PhieuSuaChuas
                .Where(p => p.NgayNhan >= startOfMonth && (p.TrangThai == PhieuSuaChua.Statuses.Done || p.TrangThai == PhieuSuaChua.Statuses.Delivered))
                .SumAsync(p => p.TongTien);

            var lastMonthRevenue = await _context.PhieuSuaChuas
                .Where(p => p.NgayNhan >= startOfLastMonth && p.NgayNhan <= endOfLastMonth && (p.TrangThai == PhieuSuaChua.Statuses.Done || p.TrangThai == PhieuSuaChua.Statuses.Delivered))
                .SumAsync(p => p.TongTien);

            var revenueChange = lastMonthRevenue > 0 
                ? (double)((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 
                : 100;

            var totalTickets = await _context.PhieuSuaChuas.CountAsync();
            var thisMonthTickets = await _context.PhieuSuaChuas.CountAsync(p => p.NgayNhan >= startOfMonth);

            // Revenue Data (Last 7 days)
            var revenueData = new List<RevenueDataPoint>();
            for (int i = 6; i >= 0; i--)
            {
                var day = now.AddDays(-i).Date;
                var nextDay = day.AddDays(1);
                var dayRevenue = await _context.PhieuSuaChuas
                    .Where(p => p.NgayNhan >= day && p.NgayNhan < nextDay && (p.TrangThai == PhieuSuaChua.Statuses.Done || p.TrangThai == PhieuSuaChua.Statuses.Delivered))
                    .SumAsync(p => p.TongTien);
                revenueData.Add(new RevenueDataPoint { Name = day.ToString("dd/MM"), Value = dayRevenue });
            }

            // Status Data
            var statusData = await _context.PhieuSuaChuas
                .GroupBy(p => p.TrangThai)
                .Select(g => new StatusDataPoint { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Top Parts
            var topParts = await _context.YeuCauLinhKiens
                .GroupBy(y => y.LinhKien.TenLinhKien)
                .Select(g => new TopPartDataPoint { PartName = g.Key, Quantity = g.Sum(y => y.SoLuong) })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .ToListAsync();

            // Tech Performance (Assuming KyThuatVien is NguoiDung)
            // Note: KyThuatVien might be null
            var techPerformance = await _context.PhieuSuaChuas
                .Where(p => p.KyThuatVien != null)
                .GroupBy(p => p.KyThuatVien!.TenDayDu)
                .Select(g => new TechPerformanceDataPoint 
                { 
                    TechName = g.Key, 
                    Total = g.Count(),
                    Completed = g.Count(x => x.TrangThai == PhieuSuaChua.Statuses.Done || x.TrangThai == PhieuSuaChua.Statuses.Delivered)
                })
                .Take(5)
                .ToListAsync();

            return new DashboardViewModel
            {
                TotalRevenue = currentMonthRevenue,
                RevenueChange = revenueChange,
                TotalTickets = totalTickets,
                ThisMonthTickets = thisMonthTickets,
                RevenueData = revenueData,
                StatusData = statusData,
                TopParts = topParts,
                TechPerformance = techPerformance
            };
        }

        [HttpGet("ReportSummary")]
        public async Task<ActionResult<ReportSummaryViewModel>> GetReportSummary(DateTime? fromDate, DateTime? toDate)
        {
            var start = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var end = toDate ?? DateTime.UtcNow;

            var tickets = await _context.PhieuSuaChuas
                .Where(p => p.NgayNhan >= start && p.NgayNhan <= end)
                .ToListAsync();

            var totalTickets = tickets.Count;
            var totalRevenue = tickets.Where(p => p.TrangThai == PhieuSuaChua.Statuses.Done || p.TrangThai == PhieuSuaChua.Statuses.Delivered).Sum(p => p.TongTien);
            var completedTickets = tickets.Count(p => p.TrangThai == PhieuSuaChua.Statuses.Done || p.TrangThai == PhieuSuaChua.Statuses.Delivered);
            var completionRate = totalTickets > 0 ? (double)completedTickets / totalTickets * 100 : 0;

            return new ReportSummaryViewModel
            {
                TotalTickets = totalTickets,
                TotalRevenue = totalRevenue,
                CompletionRate = completionRate
            };
        }
    }
}
