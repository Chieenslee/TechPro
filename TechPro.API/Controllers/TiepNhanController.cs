using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechPro.API.Data;
using TechPro.API.Models;
using TechPro.API.Services;

namespace TechPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // Mọi endpoint yêu cầu đăng nhập
    public class TiepNhanController : ControllerBase
    {
        private readonly TechProDbContext _context;
        private readonly AuditLogService _audit;

        public TiepNhanController(TechProDbContext context, AuditLogService audit)
        {
            _context = context;
            _audit = audit;
        }

        // ════════════════════════════════════════════════════════════════
        // LIST & SEARCH (Dashboard - lễ tân / kỹ thuật dùng)
        // ════════════════════════════════════════════════════════════════

        // GET: api/TiepNhan?searchTerm=...&status=...
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhieuSuaChua>>> GetPhieuSuaChuas(string? searchTerm, string? status)
        {
            var query = _context.PhieuSuaChuas.AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                query = query.Where(p => p.TrangThai == status);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerTerm = searchTerm.Trim().ToLower();
                query = query.Where(p =>
                    p.TenKhachHang.ToLower().Contains(lowerTerm) ||
                    p.SoDienThoai.Contains(searchTerm.Trim()) ||
                    p.Id.Contains(searchTerm.Trim()));
            }

            return await query.OrderByDescending(p => p.NgayNhan).ToListAsync();
        }

        // GET: api/TiepNhan/search?query=...  (Tra cứu công khai từ trang chủ)
        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Vui lòng nhập thông tin tra cứu.");

            var raw = query.Trim();
            var normalizedQuery = new string(raw.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

            // Lấy dữ liệu ra bộ nhớ rồi mới chuẩn hóa / so khớp cho linh hoạt,
            // tránh phụ thuộc vào hạn chế translate của provider.
            var tickets = await _context.PhieuSuaChuas
                .Include(p => p.KyThuatVien)
                .Include(p => p.YeuCauLinhKiens).ThenInclude(y => y.LinhKien)
                .AsNoTracking()
                .ToListAsync();

            string Normalize(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return string.Empty;
                var filtered = new string(s.Where(char.IsLetterOrDigit).ToArray());
                return filtered.ToLowerInvariant();
            }

            var phieu = tickets.FirstOrDefault(p =>
                // Khớp chính xác theo Id hoặc số điện thoại
                string.Equals(p.Id, raw, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.SoDienThoai, raw, StringComparison.OrdinalIgnoreCase) ||
                // Cho phép gõ khác định dạng (RX-2026-X001 vs RX 2026 X001, chữ thường/hoa…)
                Normalize(p.Id) == normalizedQuery ||
                // Cho phép nhập một phần SĐT (3–4 số cuối)
                (!string.IsNullOrEmpty(p.SoDienThoai) &&
                 p.SoDienThoai.EndsWith(raw, StringComparison.Ordinal)));

            if (phieu == null) return NotFound();
            return Ok(phieu);
        }

        // GET: api/TiepNhan/debug-first
        // Debug endpoint: xem nhanh 20 Id đầu tiên mà API đang nhìn thấy trong DB hiện tại
        [HttpGet("debug-first")]
        [AllowAnonymous]
        public async Task<IActionResult> DebugFirst()
        {
            var first = await _context.PhieuSuaChuas
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Take(20)
                .Select(p => new
                {
                    p.Id,
                    p.TenKhachHang,
                    p.SoDienThoai,
                    p.TenThietBi
                })
                .ToListAsync();

            return Ok(new
            {
                count = first.Count,
                items = first
            });
        }

        // GET: api/TiepNhan/{id}  (Chi tiết đầy đủ - public + nội bộ)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPhieuSuaChua(string id)
        {
            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.KyThuatVien)
                .Include(p => p.CuaHang)
                .Include(p => p.YeuCauLinhKiens).ThenInclude(y => y.LinhKien)
                .Include(p => p.TraXacs)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phieu == null) return NotFound();
            return Ok(phieu);
        }

        // ════════════════════════════════════════════════════════════════
        // CREATE
        // ════════════════════════════════════════════════════════════════

        // POST: api/TiepNhan/TaoPhieu
        [HttpPost("TaoPhieu")]
        public async Task<ActionResult<PhieuSuaChua>> CreatePhieuSuaChua(PhieuSuaChua phieuSuaChua)
        {
            if (string.IsNullOrEmpty(phieuSuaChua.Id))
            {
                var now = DateTime.UtcNow.AddHours(7);
                // Dùng Guid suffix để tránh race condition khi 2 phiếu được tạo cùng giây
                phieuSuaChua.Id = "PSC" + now.ToString("yyMMddHHmm") + Guid.NewGuid().ToString("N")[..6].ToUpper();
            }

            phieuSuaChua.NgayNhan = DateTime.UtcNow;
            phieuSuaChua.TrangThai = PhieuSuaChua.Statuses.Pending;

            // Auto-gán TenantId từ user Support đang tạo phiếu (nếu MVC không truyền vào)
            if (string.IsNullOrEmpty(phieuSuaChua.TenantId))
            {
                var callerEmail = Request.Headers["X-Caller-Email"].FirstOrDefault()
                    ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                if (!string.IsNullOrEmpty(callerEmail))
                {
                    var callerUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == callerEmail);
                    if (callerUser != null && !string.IsNullOrEmpty(callerUser.TenantId))
                        phieuSuaChua.TenantId = callerUser.TenantId;
                }
            }

            _context.PhieuSuaChuas.Add(phieuSuaChua);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (_context.PhieuSuaChuas.Any(p => p.Id == phieuSuaChua.Id))
                    return Conflict();
                throw;
            }

            return CreatedAtAction(nameof(GetPhieuSuaChua), new { id = phieuSuaChua.Id }, phieuSuaChua);
        }


        // ════════════════════════════════════════════════════════════════
        // ACTIONS
        // ════════════════════════════════════════════════════════════════

        // POST: api/TiepNhan/{id}/Huy  (Lễ tân hủy phiếu)
        [HttpPost("{id}/Huy")]
        // Dùng [FromQuery] thay [FromBody] vì MVC caller gửi null body (PostAsync(..., null))
        // [FromBody] với null body và không có Content-Type sẽ gây HTTP 415
        public async Task<IActionResult> HuyPhieu(string id, [FromQuery] string? lyDoHuy = null)
        {
            var phieu = await _context.PhieuSuaChuas.FindAsync(id);
            if (phieu == null) return NotFound(new { message = "Không tìm thấy phiếu." });

            if (phieu.TrangThai == PhieuSuaChua.Statuses.Done || phieu.TrangThai == PhieuSuaChua.Statuses.Delivered)
                return BadRequest(new { message = "Không thể hủy phiếu đã hoàn thành." });

            phieu.TrangThai = "cancelled";

            // Ghi lịch sử hủy phiếu để traceability — ai hủy, lúc nào, lý do gì
            var callerEmail = Request.Headers["X-Caller-Email"].FirstOrDefault() ?? "unknown";
            var caller = await _context.Users.FirstOrDefaultAsync(u => u.Email == callerEmail);
            if (caller != null)
            {
                _context.LichSuHuyPhieus.Add(new LichSuHuyPhieu
                {
                    PhieuSuaChuaId = id,
                    NguoiYeuCauId  = caller.Id,
                    LyDoHuy        = lyDoHuy ?? "Không có lý do",
                    TrangThai      = "DaHuy",
                    NgayYeuCau     = DateTime.UtcNow.AddHours(7),
                    NgayDuyet      = DateTime.UtcNow.AddHours(7)
                });
            }

            await _context.SaveChangesAsync();

            // Ghi audit log
            await _audit.LogAsync(
                thucHienBoi: callerEmail,
                role: Request.Headers["X-Caller-Role"].FirstOrDefault() ?? "unknown",
                hanhDong: "HuyPhieu",
                doiTuongId: id,
                loaiDoiTuong: "PhieuSuaChua",
                ghiChu: lyDoHuy ?? "Không có lý do",
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            return Ok(new { message = "Đã hủy phiếu thành công." });
        }

        // POST: api/TiepNhan/{id}/XacNhanBaoGia  (Khách hàng xác nhận báo giá, trạng thái → repairing)
        [HttpPost("{id}/XacNhanBaoGia")]
        public async Task<IActionResult> XacNhanBaoGia(string id)
        {
            var phieu = await _context.PhieuSuaChuas.FindAsync(id);
            if (phieu == null) return NotFound(new { message = "Không tìm thấy phiếu." });

            if (phieu.TrangThai != PhieuSuaChua.Statuses.WaitingParts)
                return BadRequest(new { message = "Phiếu không ở trạng thái chờ xác nhận." });

            phieu.TrangThai = PhieuSuaChua.Statuses.Repairing;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xác nhận báo giá. Kỹ thuật sẽ tiến hành sửa ngay." });
        }

        // ════════════════════════════════════════════════════════════════
        // THANH TOÁN & GIAO MÁY
        // ════════════════════════════════════════════════════════════════

        // POST: api/TiepNhan/{id}/ThanhToan
        [HttpPost("{id}/ThanhToan")]
        [Authorize(Roles = "Support,StoreAdmin,SystemAdmin")]
        public async Task<IActionResult> ThanhToan(string id)
        {
            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.YeuCauLinhKiens)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phieu == null) return NotFound(new { message = "Không tìm thấy phiếu." });

            if (phieu.TrangThai != PhieuSuaChua.Statuses.Done)
                return BadRequest(new { message = "Phiếu chưa hoàn thiện, không thể thanh toán." });

            // Tính tổng tiền = Tiền linh kiện (đã duyệt) + Tiền công
            var tienLinhKien = phieu.YeuCauLinhKiens
                .Where(y => y.TrangThai == "approved")
                .Sum(y => (y.GiaTaiThoiDiemYeuCau ?? 0) * y.SoLuong);
            
            var tienCong = phieu.CoBaoHanh == true ? 0 : 250000m;
            phieu.TongTien = tienLinhKien + tienCong;

            // Đổi trạng thái & chốt ngày hoàn thành
            phieu.TrangThai = PhieuSuaChua.Statuses.Delivered;
            phieu.NgayHoanThanh ??= DateTime.UtcNow.AddHours(7);

            // Ghi nhận doanh thu vào RevenueDailies
            var date = phieu.NgayHoanThanh.Value.Date;
            var doanhThu = await _context.RevenueDailies
                .FirstOrDefaultAsync(r => r.Ngay == date && r.TenantId == phieu.TenantId);
            
            if (doanhThu == null)
            {
                doanhThu = new RevenueDaily { Ngay = date, DoanhThu = phieu.TongTien, TenantId = phieu.TenantId };
                _context.RevenueDailies.Add(doanhThu);
            }
            else
            {
                doanhThu.DoanhThu += phieu.TongTien;
            }

            // Giải phóng kỹ thuật viên (tùy chọn) để report không bị dính
            // Nếu không giải phóng, KTV vẫn gắn trên phiếu để quản lý biết ai làm

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = $"Thanh toán thành công. Thu khách: {phieu.TongTien:N0}đ" });
        }

        // ════════════════════════════════════════════════════════════════
        // AI QUOTE ANALYSIS
        // ════════════════════════════════════════════════════════════════

        public class AnalyzeRequest
        {
            public string MoTaLoi { get; set; } = string.Empty;
            public string TenThietBi { get; set; } = string.Empty;
        }

        // POST: api/TiepNhan/AnalyzeQuote
        [HttpPost("AnalyzeQuote")]
        public async Task<IActionResult> AnalyzeQuote([FromBody] AnalyzeRequest request, [FromServices] TechPro.API.Services.SmartDiagnosisService aiService)
        {
            var result = await aiService.AnalyzeQuoteAsync(request.MoTaLoi, request.TenThietBi);
            return Ok(result);
        }

        // ════════════════════════════════════════════════════════════════
        // WARRANTY / DEVICE LOOKUP  (từ trang chủ - tra cứu bảo hành)
        // (Đã gộp từ DevicesController - thiết bị bán ra)
        // ════════════════════════════════════════════════════════════════

        // GET: api/TiepNhan/device-warranty?serial=...
        [HttpGet("device-warranty")]
        [AllowAnonymous]
        public async Task<IActionResult> DeviceWarranty(string serial)
        {
            if (string.IsNullOrWhiteSpace(serial))
                return BadRequest("Vui lòng nhập số Serial.");

            var thietBi = await _context.ThietBiBans
                .FirstOrDefaultAsync(t => t.SerialNumber == serial.Trim());

            if (thietBi == null) return NotFound();
            return Ok(thietBi);
        }
    }
}

