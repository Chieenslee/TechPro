namespace TechPro.API.Models
{
    /// <summary>
    /// Ghi lại toàn bộ thao tác nhạy cảm: duyệt kho, từ chối, hủy phiếu, gán KTV...
    /// Mục đích: Audit trail, tránh lạm dụng quyền, truy vết khi có sự cố.
    /// </summary>
    public class AuditLog
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>Email hoặc ID của người thực hiện</summary>
        public string ThucHienBoi { get; set; } = string.Empty;

        /// <summary>Role của người thực hiện lúc đó</summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>Hành động: DuyetKho, TuChoiKho, HuyPhieu, GanKyThuat, CapNhatTrangThai...</summary>
        public string HanhDong { get; set; } = string.Empty;

        /// <summary>Đối tượng chịu tác động (mã phiếu, mã yêu cầu,...)</summary>
        public string DoiTuongId { get; set; } = string.Empty;

        /// <summary>Loại đối tượng: PhieuSuaChua, YeuCauLinhKien, TraXac...</summary>
        public string LoaiDoiTuong { get; set; } = string.Empty;

        /// <summary>Mô tả ngắn gọn thêm</summary>
        public string? GhiChu { get; set; }

        /// <summary>IP của request</summary>
        public string? IpAddress { get; set; }

        public DateTime ThoiGian { get; set; } = DateTime.UtcNow.AddHours(7);
    }
}
