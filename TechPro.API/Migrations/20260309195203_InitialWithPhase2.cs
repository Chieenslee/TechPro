using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechPro.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialWithPhase2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CuaHangs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenCuaHang = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DoanhThu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LogoUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AdminEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Hotline = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MauInHoaDon = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuaHangs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RevenueDailies",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ngay = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DoanhThu = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RevenueDailies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TenDayDu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_CuaHangs_TenantId",
                        column: x => x.TenantId,
                        principalTable: "CuaHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "KhoLinhKiens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenLinhKien = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SoLuongTon = table.Column<int>(type: "int", nullable: false),
                    GiaBan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DanhSachModelTuongThich = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DanhMuc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoLinhKiens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KhoLinhKiens_CuaHangs_TenantId",
                        column: x => x.TenantId,
                        principalTable: "CuaHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LichHens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ThietBi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChiNhanh = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgayHen = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GioHen = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    MoTaLoi = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichHens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichHens_CuaHangs_TenantId",
                        column: x => x.TenantId,
                        principalTable: "CuaHangs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ThietBiBans",
                columns: table => new
                {
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NgayMua = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiHanBaoHanhThang = table.Column<int>(type: "int", nullable: false),
                    TenKhachHang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThietBiBans", x => x.SerialNumber);
                    table.ForeignKey(
                        name: "FK_ThietBiBans_CuaHangs_TenantId",
                        column: x => x.TenantId,
                        principalTable: "CuaHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GiaoCas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NguoiGiaoId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NguoiNhanId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ThoiGianGiao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianNhan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TienMatBanGiao = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThucNhanTienMat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiaoCas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GiaoCas_AspNetUsers_NguoiGiaoId",
                        column: x => x.NguoiGiaoId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiaoCas_AspNetUsers_NguoiNhanId",
                        column: x => x.NguoiNhanId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GiaoCas_CuaHangs_TenantId",
                        column: x => x.TenantId,
                        principalTable: "CuaHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Link = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PhieuDieuChuyens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TuCuaHangId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DenCuaHangId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NguoiYeuCauId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NguoiDuyetXuatId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayYeuCau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayXuat = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayNhan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuDieuChuyens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhieuDieuChuyens_AspNetUsers_NguoiDuyetXuatId",
                        column: x => x.NguoiDuyetXuatId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PhieuDieuChuyens_AspNetUsers_NguoiYeuCauId",
                        column: x => x.NguoiYeuCauId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PhieuDieuChuyens_CuaHangs_DenCuaHangId",
                        column: x => x.DenCuaHangId,
                        principalTable: "CuaHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhieuDieuChuyens_CuaHangs_TuCuaHangId",
                        column: x => x.TuCuaHangId,
                        principalTable: "CuaHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhieuSuaChuas",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenKhachHang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TenThietBi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayNhan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MoTaLoi = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    KetQuaKiemTra = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    KyThuatVienId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    MatKhauManHinh = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CoBaoHanh = table.Column<bool>(type: "bit", nullable: true),
                    TenantId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TongTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsICloudRemoved = table.Column<bool>(type: "bit", nullable: false),
                    IsFindMyOff = table.Column<bool>(type: "bit", nullable: false),
                    PhuKien = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhieuSuaChuas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhieuSuaChuas_AspNetUsers_KyThuatVienId",
                        column: x => x.KyThuatVienId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PhieuSuaChuas_CuaHangs_TenantId",
                        column: x => x.TenantId,
                        principalTable: "CuaHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDieuChuyens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhieuDieuChuyenId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LinhKienId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SoLuongYeuCau = table.Column<int>(type: "int", nullable: false),
                    SoLuongThucThuat = table.Column<int>(type: "int", nullable: true),
                    GhiChu = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDieuChuyens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietDieuChuyens_KhoLinhKiens_LinhKienId",
                        column: x => x.LinhKienId,
                        principalTable: "KhoLinhKiens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChiTietDieuChuyens_PhieuDieuChuyens_PhieuDieuChuyenId",
                        column: x => x.PhieuDieuChuyenId,
                        principalTable: "PhieuDieuChuyens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoaHongs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhieuSuaChuaId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NhanVienId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LoaiHoaHong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SoTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NgayGhiNhan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaHongs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoaHongs_AspNetUsers_NhanVienId",
                        column: x => x.NhanVienId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HoaHongs_PhieuSuaChuas_PhieuSuaChuaId",
                        column: x => x.PhieuSuaChuaId,
                        principalTable: "PhieuSuaChuas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LichSuHuyPhieus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PhieuSuaChuaId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NguoiYeuCauId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    NguoiDuyetId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    LyDoHuy = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayYeuCau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayDuyet = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichSuHuyPhieus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LichSuHuyPhieus_AspNetUsers_NguoiDuyetId",
                        column: x => x.NguoiDuyetId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuHuyPhieus_AspNetUsers_NguoiYeuCauId",
                        column: x => x.NguoiYeuCauId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LichSuHuyPhieus_PhieuSuaChuas_PhieuSuaChuaId",
                        column: x => x.PhieuSuaChuaId,
                        principalTable: "PhieuSuaChuas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScratchMarks",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhieuSuaChuaId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    X = table.Column<double>(type: "float", nullable: false),
                    Y = table.Column<double>(type: "float", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedByName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScratchMarks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScratchMarks_PhieuSuaChuas_PhieuSuaChuaId",
                        column: x => x.PhieuSuaChuaId,
                        principalTable: "PhieuSuaChuas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TicketNotes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhieuSuaChuaId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketNotes_PhieuSuaChuas_PhieuSuaChuaId",
                        column: x => x.PhieuSuaChuaId,
                        principalTable: "PhieuSuaChuas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TraXacs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhieuSuaChuaId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenKyThuatVien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TenLinhKien = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayTra = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraXacs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraXacs_PhieuSuaChuas_PhieuSuaChuaId",
                        column: x => x.PhieuSuaChuaId,
                        principalTable: "PhieuSuaChuas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YeuCauLinhKiens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PhieuSuaChuaId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenKyThuatVien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LinhKienId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NgayYeuCau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GiaTaiThoiDiemYeuCau = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauLinhKiens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YeuCauLinhKiens_KhoLinhKiens_LinhKienId",
                        column: x => x.LinhKienId,
                        principalTable: "KhoLinhKiens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_YeuCauLinhKiens_PhieuSuaChuas_PhieuSuaChuaId",
                        column: x => x.PhieuSuaChuaId,
                        principalTable: "PhieuSuaChuas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TenantId",
                table: "AspNetUsers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDieuChuyens_LinhKienId",
                table: "ChiTietDieuChuyens",
                column: "LinhKienId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDieuChuyens_PhieuDieuChuyenId",
                table: "ChiTietDieuChuyens",
                column: "PhieuDieuChuyenId");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoCas_NguoiGiaoId",
                table: "GiaoCas",
                column: "NguoiGiaoId");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoCas_NguoiNhanId",
                table: "GiaoCas",
                column: "NguoiNhanId");

            migrationBuilder.CreateIndex(
                name: "IX_GiaoCas_TenantId",
                table: "GiaoCas",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_HoaHongs_NhanVienId",
                table: "HoaHongs",
                column: "NhanVienId");

            migrationBuilder.CreateIndex(
                name: "IX_HoaHongs_PhieuSuaChuaId",
                table: "HoaHongs",
                column: "PhieuSuaChuaId");

            migrationBuilder.CreateIndex(
                name: "IX_KhoLinhKiens_TenantId",
                table: "KhoLinhKiens",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LichHens_TenantId",
                table: "LichHens",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuHuyPhieus_NguoiDuyetId",
                table: "LichSuHuyPhieus",
                column: "NguoiDuyetId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuHuyPhieus_NguoiYeuCauId",
                table: "LichSuHuyPhieus",
                column: "NguoiYeuCauId");

            migrationBuilder.CreateIndex(
                name: "IX_LichSuHuyPhieus_PhieuSuaChuaId",
                table: "LichSuHuyPhieus",
                column: "PhieuSuaChuaId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuDieuChuyens_DenCuaHangId",
                table: "PhieuDieuChuyens",
                column: "DenCuaHangId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuDieuChuyens_NguoiDuyetXuatId",
                table: "PhieuDieuChuyens",
                column: "NguoiDuyetXuatId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuDieuChuyens_NguoiYeuCauId",
                table: "PhieuDieuChuyens",
                column: "NguoiYeuCauId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuDieuChuyens_TuCuaHangId",
                table: "PhieuDieuChuyens",
                column: "TuCuaHangId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuSuaChuas_KyThuatVienId",
                table: "PhieuSuaChuas",
                column: "KyThuatVienId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuSuaChuas_NgayNhan",
                table: "PhieuSuaChuas",
                column: "NgayNhan");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuSuaChuas_SerialNumber",
                table: "PhieuSuaChuas",
                column: "SerialNumber");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuSuaChuas_TenantId",
                table: "PhieuSuaChuas",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PhieuSuaChuas_TrangThai",
                table: "PhieuSuaChuas",
                column: "TrangThai");

            migrationBuilder.CreateIndex(
                name: "IX_RevenueDailies_Ngay",
                table: "RevenueDailies",
                column: "Ngay");

            migrationBuilder.CreateIndex(
                name: "IX_ScratchMarks_PhieuSuaChuaId",
                table: "ScratchMarks",
                column: "PhieuSuaChuaId");

            migrationBuilder.CreateIndex(
                name: "IX_ThietBiBans_SerialNumber",
                table: "ThietBiBans",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ThietBiBans_TenantId",
                table: "ThietBiBans",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketNotes_PhieuSuaChuaId",
                table: "TicketNotes",
                column: "PhieuSuaChuaId");

            migrationBuilder.CreateIndex(
                name: "IX_TraXacs_PhieuSuaChuaId",
                table: "TraXacs",
                column: "PhieuSuaChuaId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauLinhKiens_LinhKienId",
                table: "YeuCauLinhKiens",
                column: "LinhKienId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauLinhKiens_PhieuSuaChuaId",
                table: "YeuCauLinhKiens",
                column: "PhieuSuaChuaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ChiTietDieuChuyens");

            migrationBuilder.DropTable(
                name: "GiaoCas");

            migrationBuilder.DropTable(
                name: "HoaHongs");

            migrationBuilder.DropTable(
                name: "LichHens");

            migrationBuilder.DropTable(
                name: "LichSuHuyPhieus");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "RevenueDailies");

            migrationBuilder.DropTable(
                name: "ScratchMarks");

            migrationBuilder.DropTable(
                name: "ThietBiBans");

            migrationBuilder.DropTable(
                name: "TicketNotes");

            migrationBuilder.DropTable(
                name: "TraXacs");

            migrationBuilder.DropTable(
                name: "YeuCauLinhKiens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "PhieuDieuChuyens");

            migrationBuilder.DropTable(
                name: "KhoLinhKiens");

            migrationBuilder.DropTable(
                name: "PhieuSuaChuas");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CuaHangs");
        }
    }
}
