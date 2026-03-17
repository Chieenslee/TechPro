using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TechPro.API.Data;
using TechPro.API.Hubs;
using TechPro.API.Models;
using TechPro.API.Models.DTOs;
using TechPro.API.Services;

namespace TechPro.API.Controllers
{
    /// <summary>Kho – quản lý linh kiện, duyệt yêu cầu, kiểm kê tồn kho</summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly TechProDbContext _context;
        private readonly IHubContext<TicketHub> _hubContext;
        private readonly AuditLogService _audit;

        public InventoryController(TechProDbContext context, IHubContext<TicketHub> hubContext, AuditLogService audit)
        {
            _context = context;
            _hubContext = hubContext;
            _audit = audit;
        }

        // Helper: lấy caller info MVC truyền qua header (server-to-server)
        private string CallerEmail() => Request.Headers["X-Caller-Email"].FirstOrDefault() ?? "unknown";
        private string CallerRole()  => Request.Headers["X-Caller-Role"].FirstOrDefault() ?? "unknown";
        private string CallerIp()    => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // GET: api/Inventory/my-requests
        // MVC (server-to-server) forward X-Caller-Email qua header — được kiểm soát tại MVC layer
        // Technician chỉ xem yêu cầu của chính họ (email do MVC gửi, đã vậrified beff MVC [Authorize])
        [HttpGet("my-requests")]
        public async Task<IActionResult> GetMyRequests()
        {
            // Ưu tiên lấy từ X-Caller-Email header (MVC server-to-server call)
            // Fallback: lấy từ User.Claims (nếu gọi trực tiếp với JWT trong tương lai)
            var myEmail = Request.Headers["X-Caller-Email"].FirstOrDefault()
                          ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(myEmail) || myEmail == "unknown")
                return Unauthorized("Không xác định được danh tính.");

            // Resolve email -> userId một lần, tránh join phức tạp trên navigation KyThuatVien
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == myEmail);
            if (user == null)
                return Ok(Array.Empty<YeuCauLinhKien>());

            var myUserId = user.Id;

            var userName = user.UserName ?? myEmail;

            var requests = await _context.YeuCauLinhKiens
                .Include(y => y.PhieuSuaChua)
                .Include(y => y.LinhKien)
                .Where(y => y.PhieuSuaChua != null &&
                            (
                                // Phiếu hiện tại đã gán KyThuatVienId trùng user
                                y.PhieuSuaChua.KyThuatVienId == myUserId
                                // Hoặc yêu cầu cũ được tạo theo tên kỹ thuật viên
                                || y.TenKyThuatVien == userName
                            ))
                .OrderByDescending(y => y.NgayYeuCau)
                .ToListAsync();

            return Ok(requests);
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard(string? searchTerm = null)
        {
            // Inventory (bảng "KhoLinhKiens" trong DB)
            var inventoryQuery = _context.KhoLinhKiens.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                inventoryQuery = inventoryQuery.Where(k =>
                    k.TenLinhKien.Contains(searchTerm) ||
                    (k.DanhMuc != null && k.DanhMuc.Contains(searchTerm)));
            }
            var inventory = await inventoryQuery.OrderBy(k => k.TenLinhKien).ToListAsync();

            // Part Requests
            var partRequests = await _context.YeuCauLinhKiens
                .Include(y => y.PhieuSuaChua)
                .Include(y => y.LinhKien)
                .OrderByDescending(y => y.NgayYeuCau)
                .ToListAsync();


            // Waste Returns
            var wasteReturns = await _context.TraXacs
                .Include(t => t.PhieuSuaChua)
                .OrderByDescending(t => t.NgayTra)
                .ToListAsync();

            var dto = new InventoryDashboardDto
            {
                Inventory = inventory,
                PartRequests = partRequests,
                WasteReturns = wasteReturns,
                PendingRequestsCount = partRequests.Count(r => r.TrangThai == "pending"),
                PendingWasteCount = wasteReturns.Count(w => w.TrangThai == "pending")
            };

            return Ok(dto);
        }

        [HttpPost("approve/{id}")]
        [Authorize(Roles = "Storekeeper,StoreAdmin,SystemAdmin")]
        public async Task<IActionResult> ApproveRequest(string id)
        {
            var yeuCau = await _context.YeuCauLinhKiens
                .Include(y => y.LinhKien)
                .FirstOrDefaultAsync(y => y.Id == id);

            if (yeuCau == null)
                return NotFound(new { message = "Không tìm thấy yêu cầu." });

            var linhKien = yeuCau.LinhKien;
            if (linhKien == null)
                return BadRequest(new { message = "Không tìm thấy linh kiện." });

            // Dùng raw SQL UPDATE atomic để tránh race condition tồn kho âm:
            // Nếu 2 request duyệt cùng lúc, chỉ 1 cái UPDATE thành công (WHERE SoLuongTon >= SoLuong)
            var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                "UPDATE \"KhoLinhKiens\" SET \"SoLuongTon\" = \"SoLuongTon\" - {0} WHERE \"Id\" = {1} AND \"SoLuongTon\" >= {0}",
                yeuCau.SoLuong, linhKien.Id);

            if (rowsAffected == 0)
                return BadRequest(new { message = "Không đủ tồn kho hoặc đã được duyệt bởi người khác." });

            // Reload để lấy giá trị mới nhất sau UPDATE
            await _context.Entry(linhKien).ReloadAsync();

            yeuCau.TrangThai = "approved";
            yeuCau.GiaTaiThoiDiemYeuCau = linhKien.GiaBan;

            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("PartRequestApproved", id);

            // Audit log
            await _audit.LogAsync(
                thucHienBoi: CallerEmail(),
                role: CallerRole(),
                hanhDong: "DuyetYeuCauKho",
                doiTuongId: id,
                loaiDoiTuong: "YeuCauLinhKien",
                ghiChu: $"Duyệt yêu cầu {yeuCau.SoLuong}x {linhKien.TenLinhKien}",
                ipAddress: CallerIp()
            );

            return Ok(new { message = "Đã duyệt và xuất kho thành công!" });
        }

        [HttpPost("reject/{id}")]
        [Authorize(Roles = "Storekeeper,StoreAdmin,SystemAdmin")]
        public async Task<IActionResult> RejectRequest(string id)
        {
            var yeuCau = await _context.YeuCauLinhKiens.FindAsync(id);
            if (yeuCau == null) return NotFound(new { message = "Không tìm thấy yêu cầu." });

            yeuCau.TrangThai = "rejected";
            await _context.SaveChangesAsync();

            // Audit log
            await _audit.LogAsync(
                thucHienBoi: CallerEmail(),
                role: CallerRole(),
                hanhDong: "TuChoiYeuCauKho",
                doiTuongId: id,
                loaiDoiTuong: "YeuCauLinhKien",
                ipAddress: CallerIp()
            );

            return Ok(new { message = "Đã từ chối yêu cầu." });
        }

        [HttpPost("confirm-waste/{id}")]
        [Authorize(Roles = "Storekeeper,StoreAdmin,SystemAdmin")]
        public async Task<IActionResult> ConfirmWaste(string id)
        {
            var traXac = await _context.TraXacs.FindAsync(id);
            if (traXac == null) return NotFound(new { message = "Không tìm thấy yêu cầu trả xác." });

            traXac.TrangThai = "returned";
            await _context.SaveChangesAsync();

            // Audit log
            await _audit.LogAsync(
                thucHienBoi: CallerEmail(),
                role: CallerRole(),
                hanhDong: "XacNhanTraXac",
                doiTuongId: id,
                loaiDoiTuong: "TraXac",
                ipAddress: CallerIp()
            );

            return Ok(new { message = "Đã xác nhận nhận xác linh kiện." });
        }

        [HttpGet("low-stock")]
        [Authorize(Roles = "Storekeeper,StoreAdmin,SystemAdmin")]
        public async Task<IActionResult> GetLowStockItems(int threshold = 5, string? tenantId = null)
        {
            // Note: tenantId logic should typically come from User claims in API
            var lowStockItems = await _context.KhoLinhKiens
                .Where(k => k.SoLuongTon <= threshold && (tenantId == null || k.TenantId == tenantId))
                .Select(k => new { k.Id, k.TenLinhKien, k.SoLuongTon, k.DanhMuc })
                .ToListAsync();

            return Ok(new { success = true, data = lowStockItems });
        }

            // PUT: api/Inventory/price/{id}
        // SysAdmin dùng để đồng bộ giá chuẩn toàn hệ thống
        [HttpPut("price/{id}")]
        [Authorize(Roles = "SystemAdmin")]
        public async Task<IActionResult> UpdatePrice(string id, [FromBody] dynamic payload)
        {
            if (string.IsNullOrWhiteSpace(id)) return BadRequest(new { message = "Thiếu partId." });

            decimal giaBan;
            try
            {
                giaBan = (decimal)payload.giaBan;
            }
            catch
            {
                return BadRequest(new { message = "Thiếu hoặc sai kiểu giaBan." });
            }

            if (giaBan < 0) return BadRequest(new { message = "Giá không hợp lệ." });

            var part = await _context.KhoLinhKiens.FirstOrDefaultAsync(k => k.Id == id);
            if (part == null) return NotFound(new { message = "Không tìm thấy linh kiện." });

            part.GiaBan = giaBan;
            await _context.SaveChangesAsync();

            await _audit.LogAsync(
                thucHienBoi: CallerEmail(),
                role: CallerRole(),
                hanhDong: "CapNhatGiaChuan",
                doiTuongId: id,
                loaiDoiTuong: "KhoLinhKien",
                ghiChu: $"Giá mới: {giaBan:N0}",
                ipAddress: CallerIp()
            );

            return Ok(new { message = "Đã cập nhật giá." });
        }
    }
}
