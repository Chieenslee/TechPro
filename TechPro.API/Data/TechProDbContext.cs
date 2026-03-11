using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechPro.API.Models;

namespace TechPro.API.Data
{
    public class TechProDbContext : IdentityDbContext<NguoiDung>
    {
        public TechProDbContext(DbContextOptions<TechProDbContext> options)
            : base(options)
        {
        }

        public DbSet<CuaHang> CuaHangs { get; set; }
        public DbSet<PhieuSuaChua> PhieuSuaChuas { get; set; }
        public DbSet<KhoLinhKien> KhoLinhKiens { get; set; }
        public DbSet<YeuCauLinhKien> YeuCauLinhKiens { get; set; }
        public DbSet<TraXac> TraXacs { get; set; }
        public DbSet<ThietBiBan> ThietBiBans { get; set; }
        public DbSet<LichHen> LichHens { get; set; }
        public DbSet<TicketNote> TicketNotes { get; set; }
        public DbSet<ScratchMark> ScratchMarks { get; set; }
        public DbSet<RevenueDaily> RevenueDailies { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<GiaoCa> GiaoCas { get; set; }
        public DbSet<HoaHong> HoaHongs { get; set; }
        public DbSet<PhieuDieuChuyen> PhieuDieuChuyens { get; set; }
        public DbSet<ChiTietDieuChuyen> ChiTietDieuChuyens { get; set; }
        public DbSet<LichSuHuyPhieu> LichSuHuyPhieus { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships
            builder.Entity<NguoiDung>()
                .HasOne(n => n.CuaHang)
                .WithMany(c => c.NhanViens)
                .HasForeignKey(n => n.TenantId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<PhieuSuaChua>()
                .HasOne(p => p.KyThuatVien)
                .WithMany(n => n.PhieuSuaChuas)
                .HasForeignKey(p => p.KyThuatVienId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<PhieuSuaChua>()
                .HasOne(p => p.CuaHang)
                .WithMany(c => c.PhieuSuaChuas)
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<KhoLinhKien>()
                .HasOne(k => k.CuaHang)
                .WithMany()
                .HasForeignKey(k => k.TenantId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<YeuCauLinhKien>()
                .HasOne(y => y.PhieuSuaChua)
                .WithMany(p => p.YeuCauLinhKiens)
                .HasForeignKey(y => y.PhieuSuaChuaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<YeuCauLinhKien>()
                .HasOne(y => y.LinhKien)
                .WithMany(k => k.YeuCauLinhKiens)
                .HasForeignKey(y => y.LinhKienId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TraXac>()
                .HasOne(t => t.PhieuSuaChua)
                .WithMany(p => p.TraXacs)
                .HasForeignKey(t => t.PhieuSuaChuaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ThietBiBan>()
                .HasOne(t => t.CuaHang)
                .WithMany()
                .HasForeignKey(t => t.TenantId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes for performance
            builder.Entity<PhieuSuaChua>()
                .HasIndex(p => p.SerialNumber);

            builder.Entity<PhieuSuaChua>()
                .HasIndex(p => p.TrangThai);

            builder.Entity<PhieuSuaChua>()
                .HasIndex(p => p.NgayNhan);

            builder.Entity<ThietBiBan>()
                .HasIndex(t => t.SerialNumber)
                .IsUnique();

            builder.Entity<ScratchMark>()
                .HasIndex(s => s.PhieuSuaChuaId);

            builder.Entity<TicketNote>()
                .HasIndex(n => n.PhieuSuaChuaId);

            builder.Entity<RevenueDaily>()
                .HasIndex(r => r.Ngay);

            builder.Entity<RevenueDaily>()
                .Property(r => r.DoanhThu)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Notification>()
                .HasIndex(n => n.UserId);

            builder.Entity<Notification>()
                .HasIndex(n => n.IsRead);

            builder.Entity<Notification>()
                .HasIndex(n => n.CreatedAt);

            // PhieuDieuChuyen Relationships
            builder.Entity<PhieuDieuChuyen>()
                .HasOne(p => p.TuCuaHang)
                .WithMany()
                .HasForeignKey(p => p.TuCuaHangId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<PhieuDieuChuyen>()
                .HasOne(p => p.DenCuaHang)
                .WithMany()
                .HasForeignKey(p => p.DenCuaHangId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ChiTietDieuChuyen>()
                .HasOne(c => c.PhieuDieuChuyen)
                .WithMany(p => p.ChiTietDieuChuyens)
                .HasForeignKey(c => c.PhieuDieuChuyenId)
                .OnDelete(DeleteBehavior.Cascade);

            // GiaoCa Relationships
            builder.Entity<GiaoCa>()
                .HasOne(g => g.NguoiGiao)
                .WithMany()
                .HasForeignKey(g => g.NguoiGiaoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GiaoCa>()
                .HasOne(g => g.NguoiNhan)
                .WithMany()
                .HasForeignKey(g => g.NguoiNhanId)
                .OnDelete(DeleteBehavior.Restrict);

            // LichSuHuyPhieu Relationships
            builder.Entity<LichSuHuyPhieu>()
                .HasOne(l => l.NguoiYeuCau)
                .WithMany()
                .HasForeignKey(l => l.NguoiYeuCauId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<LichSuHuyPhieu>()
                .HasOne(l => l.NguoiDuyet)
                .WithMany()
                .HasForeignKey(l => l.NguoiDuyetId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<HoaHong>()
                .HasOne(h => h.PhieuSuaChua)
                .WithMany()
                .HasForeignKey(h => h.PhieuSuaChuaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

