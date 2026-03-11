using TechPro.API.Data;
using TechPro.API.Models;

namespace TechPro.API.Services
{
    /// <summary>
    /// Service ghi Audit Log vào DB cho mọi thao tác nhạy cảm.
    /// Inject vào InventoryController, TechnicianController, TiepNhanController.
    /// </summary>
    public class AuditLogService
    {
        private readonly TechProDbContext _context;
        private readonly ILogger<AuditLogService> _logger;

        public AuditLogService(TechProDbContext context, ILogger<AuditLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Ghi một bản ghi audit log.
        /// </summary>
        public async Task LogAsync(
            string thucHienBoi,
            string role,
            string hanhDong,
            string doiTuongId,
            string loaiDoiTuong,
            string? ghiChu = null,
            string? ipAddress = null)
        {
            var log = new AuditLog
            {
                ThucHienBoi = thucHienBoi,
                Role        = role,
                HanhDong    = hanhDong,
                DoiTuongId  = doiTuongId,
                LoaiDoiTuong = loaiDoiTuong,
                GhiChu      = ghiChu,
                IpAddress   = ipAddress,
                ThoiGian    = DateTime.UtcNow.AddHours(7)
            };

            _context.AuditLogs.Add(log);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Không để lỗi audit làm vỡ luồng chính
                _logger.LogError(ex, "Lỗi khi ghi audit log: {Action} bởi {User}", hanhDong, thucHienBoi);
            }
        }
    }
}
