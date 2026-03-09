using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechPro.API.Models;

namespace TechPro.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(UserManager<NguoiDung> userManager, RoleManager<IdentityRole> roleManager, TechProDbContext context)
        {
            // 1. Tạo Roles
            string[] roles = { "SystemAdmin", "StoreAdmin", "Technician", "Support" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Tạo Cửa hàng duy nhất
            if (!context.CuaHangs.Any())
            {
                context.CuaHangs.AddRange(new[]
                {
                    new CuaHang
                    {
                        Id = "STORE-001",
                        TenCuaHang = "TechPro Care Center",
                        DiaChi = "123 Đường Công Nghệ, Quận Hoàn Kiếm, Hà Nội",
                        TrangThai = "active",
                        Hotline = "1900 6868",
                        AdminEmail = "admin@techpro.com"
                    }
                });
                await context.SaveChangesAsync();
            }

            // 3. Tạo Users với mật khẩu: demo123 (cho tất cả)
            // Đảm bảo cửa hàng đã tồn tại trước khi tạo user
            var existingStores = await context.CuaHangs.Select(c => c.Id).ToListAsync();
            
            var users = new (string Email, string Name, string Role, string? TenantId)[]
            {
                ("sysadmin@techpro.com", "Admin Chuỗi", "SystemAdmin", null),
                ("admin@techpro.com", "Quản Lý Cửa Hàng", "StoreAdmin", "STORE-001"),
                ("support@techpro.com", "Lễ Tân", "Support", "STORE-001"),
                ("kho@techpro.com", "Thủ Kho", "StoreAdmin", "STORE-001"),
                ("tech@techpro.com", "Kỹ Thuật Viên 1", "Technician", "STORE-001"),
                ("tech2@techpro.com", "Kỹ Thuật Viên 2", "Technician", "STORE-001")
            };

            foreach (var userData in users)
            {
                var existingUser = await userManager.FindByEmailAsync(userData.Email);
                if (existingUser == null)
                {
                    // Kiểm tra TenantId có tồn tại hoặc null
                    if (userData.TenantId != null && !existingStores.Contains(userData.TenantId))
                    {
                        continue; // Skip user nếu cửa hàng chưa tồn tại
                    }

                    var user = new NguoiDung
                    {
                        UserName = userData.Email,
                        Email = userData.Email,
                        TenDayDu = userData.Name,
                        TenantId = userData.TenantId,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, "demo123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, userData.Role);
                    }
                }
            }

            var rand = new Random();
            var tenantList = await context.CuaHangs.Select(c => c.Id).ToListAsync();
            string[] tenants = tenantList.ToArray();

            // 4. Seed Kho linh kiện (30)
            var existingPartIds = await context.KhoLinhKiens.Select(k => k.Id).ToListAsync();
            if (existingPartIds.Count < 30)
            {
                var items = new List<KhoLinhKien>();
                for (int i = 1; i <= 30; i++)
                {
                    var partId = $"PK-{i:D3}";
                    // Chỉ thêm nếu chưa tồn tại
                    if (!existingPartIds.Contains(partId))
                    {
                        items.Add(new KhoLinhKien
                        {
                            Id = partId,
                            TenLinhKien = $"Linh kiện {i}",
                            DanhMuc = (i % 3 == 0) ? "Màn hình" : (i % 3 == 1 ? "Pin" : "Camera"),
                            DanhSachModelTuongThich = $"Model {i % 10}",
                            GiaBan = 500000 + rand.Next(500000, 4000000),
                            SoLuongTon = rand.Next(1, 20),
                            TenantId = tenants[rand.Next(tenants.Length)]
                        });
                    }
                }
                if (items.Any())
                {
                    context.KhoLinhKiens.AddRange(items);
                    await context.SaveChangesAsync();
                }
            }

            // 5. Seed PhieuSuaChua (30)
            if (context.PhieuSuaChuas.Count() < 30)
            {
                var statuses = new[] { "pending", "repairing", "waiting_parts", "done", "delivered" };
                var techUser = await userManager.FindByEmailAsync("tech@techpro.com");

                var tickets = new List<PhieuSuaChua>();
                for (int i = 1; i <= 30; i++)
                {
                    tickets.Add(new PhieuSuaChua
                    {
                        Id = $"RX-2026-{i:D3}",
                        TenKhachHang = $"Khách {i}",
                        SoDienThoai = $"09{rand.Next(10000000, 99999999)}",
                        TenThietBi = $"Thiết bị {i}",
                        SerialNumber = $"SN-{100000 + i}",
                        TrangThai = statuses[i % statuses.Length],
                        NgayNhan = DateTime.Now.AddHours(-i * 3),
                        MoTaLoi = $"Mô tả lỗi {i}",
                        KyThuatVienId = (i % 2 == 0) ? techUser?.Id : null,
                        TenantId = tenants[rand.Next(tenants.Length)],
                        KetQuaKiemTra = i % 5 == 0 ? "Đã kiểm tra sơ bộ" : null,
                        CoBaoHanh = i % 4 == 0
                    });
                }
                context.PhieuSuaChuas.AddRange(tickets);
                await context.SaveChangesAsync();
            }

            // 6. Seed TicketNotes (30)
            if (context.TicketNotes.Count() < 30)
            {
                var ticketIds = context.PhieuSuaChuas.Select(p => p.Id).Take(10).ToList();
                var notes = new List<TicketNote>();
                for (int i = 1; i <= 30; i++)
                {
                    notes.Add(new TicketNote
                    {
                        PhieuSuaChuaId = ticketIds[i % ticketIds.Count],
                        UserName = (i % 2 == 0) ? "Support" : "Technician",
                        Message = $"Ghi chú nội bộ {i}",
                        CreatedAt = DateTime.UtcNow.AddMinutes(-i * 5)
                    });
                }
                context.TicketNotes.AddRange(notes);
                await context.SaveChangesAsync();
            }

            // 7. Seed ScratchMarks (30)
            if (context.ScratchMarks.Count() < 30)
            {
                var ticketIds = context.PhieuSuaChuas.Select(p => p.Id).Take(10).ToList();
                var marks = new List<ScratchMark>();
                for (int i = 1; i <= 30; i++)
                {
                    marks.Add(new ScratchMark
                    {
                        PhieuSuaChuaId = ticketIds[i % ticketIds.Count],
                        X = rand.NextDouble() * 100,
                        Y = rand.NextDouble() * 100,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-i * 2),
                        CreatedByName = "Technician"
                    });
                }
                context.ScratchMarks.AddRange(marks);
                await context.SaveChangesAsync();
            }

            // 8. Seed RevenueDaily (30 days)
            if (context.RevenueDailies.Count() < 30)
            {
                var list = new List<RevenueDaily>();
                for (int i = 0; i < 30; i++)
                {
                    var day = DateTime.Today.AddDays(-i);
                    list.Add(new RevenueDaily
                    {
                        Ngay = day,
                        DoanhThu = 10_000_000 + rand.Next(1_000_000, 8_000_000),
                        TenantId = tenants[rand.Next(tenants.Length)]
                    });
                }
                context.RevenueDailies.AddRange(list);
                await context.SaveChangesAsync();
            }

            // 9. Seed ThietBiBan (30)
            if (context.ThietBiBans.Count() < 30)
            {
                var devices = new List<ThietBiBan>();
                for (int i = 1; i <= 30; i++)
                {
                    devices.Add(new ThietBiBan
                    {
                        SerialNumber = $"SER-{1000 + i}",
                        Model = $"Model {i}",
                        NgayMua = DateTime.Today.AddDays(-30 - i),
                        ThoiHanBaoHanhThang = 12 + (i % 6),
                        TenKhachHang = $"KH {i}",
                        TenantId = tenants[rand.Next(tenants.Length)]
                    });
                }
                context.ThietBiBans.AddRange(devices);
                await context.SaveChangesAsync();
            }

            // 10. Seed LichHen (30)
            if (context.LichHens.Count() < 30)
            {
                var bookings = new List<LichHen>();
                for (int i = 1; i <= 30; i++)
                {
                    bookings.Add(new LichHen
                    {
                        HoTen = $"Khách đặt lịch {i}",
                        SoDienThoai = $"09{rand.Next(10000000, 99999999)}",
                        ThietBi = $"Thiết bị đặt {i}",
                        ChiNhanh = "TechPro Care Center",
                        NgayHen = DateTime.Today.AddDays(i % 5),
                        GioHen = (i % 2 == 0) ? "09:00" : "14:00",
                        MoTaLoi = $"Mô tả lịch hẹn {i}",
                        TenantId = tenants[rand.Next(tenants.Length)],
                        CreatedAt = DateTime.Now.AddDays(-i)
                    });
                }
                context.LichHens.AddRange(bookings);
                await context.SaveChangesAsync();
            }
        }
    }
}

