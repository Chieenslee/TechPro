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
            var tenantId = User.FindFirst("TenantId")?.Value;
            if (string.IsNullOrEmpty(tenantId) && !User.IsInRole("SystemAdmin"))
            {
                return Unauthorized();
            }

            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var startOfLastMonth = startOfMonth.AddMonths(-1);
            var endOfLastMonth = startOfMonth.AddDays(-1);

            var queryPhieu = _context.PhieuSuaChuas.AsQueryable();
            var queryYeuCau = _context.YeuCauLinhKiens.AsQueryable();
            
            if (!string.IsNullOrEmpty(tenantId) && !User.IsInRole("SystemAdmin"))
            {
                // StoreAdmin only sees their store data
                queryPhieu = queryPhieu.Where(p => p.TenantId == tenantId);
                queryYeuCau = queryYeuCau.Where(y => y.PhieuSuaChua.TenantId == tenantId); // Assuming YeuCauLinhKien is linked to PhieuSuaChua
                // SystemAdmin sees everything
            }

            // Dùng NgayHoanThanh để tính doanh thu đúng kỳ kế toán
            // Phiếu nhận tháng 2 nhưng hoàn thành tháng 3 sẽ tính vào tháng 3
            var currentMonthRevenue = await queryPhieu
                .Where(p => (p.NgayHoanThanh ?? p.NgayNhan) >= startOfMonth
                    && (p.TrangThai == PhieuSuaChua.Statuses.Done || p.TrangThai == PhieuSuaChua.Statuses.Delivered))
                .SumAsync(p => p.TongTien);

            var lastMonthRevenue = await queryPhieu
                .Where(p => (p.NgayHoanThanh ?? p.NgayNhan) >= startOfLastMonth
                    && (p.NgayHoanThanh ?? p.NgayNhan) <= endOfLastMonth
                    && (p.TrangThai == PhieuSuaChua.Statuses.Done || p.TrangThai == PhieuSuaChua.Statuses.Delivered))
                .SumAsync(p => p.TongTien);
            var revenueChange = lastMonthRevenue > 0 
                ? (double)((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 
                : 100;

            var totalTickets = await queryPhieu.CountAsync();
            var thisMonthTickets = await queryPhieu.CountAsync(p => p.NgayNhan >= startOfMonth);

            // Một query duy nhất group by date — thay vì 7 vòng lặp x 7 round trips DB
            // Lấy list trước rồi group in-memory để tránh EF Core không translate được
            // (p.NgayHoanThanh ?? p.NgayNhan).Date khi group trực tiếp trên SQL
            var sevenDaysAgo = now.AddDays(-6).Date;
            var revenueRaw = await queryPhieu
                .Where(p => (p.NgayHoanThanh ?? p.NgayNhan) >= sevenDaysAgo
                    && (p.TrangThai == PhieuSuaChua.Statuses.Done || p.TrangThai == PhieuSuaChua.Statuses.Delivered))
                .Select(p => new { NgayThucTe = p.NgayHoanThanh ?? p.NgayNhan, p.TongTien })
                .ToListAsync(); 

            // Group in-memory theo Date — tránh EF Core phức tạp khi translate nullable coalesce + .Date
            var revenueByDay = revenueRaw
                .GroupBy(p => p.NgayThucTe.Date)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.TongTien));

            // Map đủ 7 ngày, ngày không có doanh thu thì = 0
            var revenueData = Enumerable.Range(0, 7)
                .Select(i => now.AddDays(-6 + i).Date)
                .Select(day => new RevenueDataPoint
                {
                    Name  = day.ToString("dd/MM"),
                    Value = revenueByDay.TryGetValue(day, out var val) ? val : 0
                })
                .ToList();

            // Status Data
            var statusData = await queryPhieu
                .GroupBy(p => p.TrangThai)
                .Select(g => new StatusDataPoint { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // Top Parts
            var topParts = await queryYeuCau
                .GroupBy(y => y.LinhKien.TenLinhKien)
                .Select(g => new TopPartDataPoint { PartName = g.Key, Quantity = g.Sum(y => y.SoLuong) })
                .OrderByDescending(x => x.Quantity)
                .Take(5)
                .ToListAsync();

            // Tech Performance (Assuming KyThuatVien is NguoiDung)
            // Note: KyThuatVien might be null
            var techPerformance = await queryPhieu
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
