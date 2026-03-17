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
            string[] roles = { "SystemAdmin", "StoreAdmin", "Technician", "Support", "Storekeeper" };
            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Tạo Cửa hàng (3 cửa hàng demo)
            var demoStores = new[]
            {
                new CuaHang { Id = "STORE-001", TenCuaHang = "TechPro Care HK", DiaChi = "123 Đường Công Nghệ, Hoàn Kiếm, HN", TrangThai = "active", Hotline = "1900 6868", AdminEmail = "admin.hk@techpro.com" },
                new CuaHang { Id = "STORE-002", TenCuaHang = "TechPro Care CG", DiaChi = "456 Xuân Thủy, Cầu Giấy, HN", TrangThai = "active", Hotline = "1900 6869", AdminEmail = "admin.cg@techpro.com" },
                new CuaHang { Id = "STORE-003", TenCuaHang = "TechPro Care SG", DiaChi = "789 Nguyễn Huệ, Quận 1, TPHCM", TrangThai = "active", Hotline = "1900 6870", AdminEmail = "admin.sg@techpro.com" }
            };

            var existingStoreIds = await context.CuaHangs.Select(c => c.Id).ToListAsync();
            foreach (var s in demoStores)
            {
                if (!existingStoreIds.Contains(s.Id))
                {
                    context.CuaHangs.Add(s);
                }
            }
            await context.SaveChangesAsync();

            // Lấy lại danh sách store để chắc chắn
            var existingStores = await context.CuaHangs.Select(c => c.Id).ToListAsync();
            
            // 3. Tạo Users với mật khẩu: demo123
            var users = new List<(string Email, string Name, string Role, string? TenantId)>
            {
                ("sysadmin@techpro.com", "Admin Toàn Hệ Thống", "SystemAdmin", null)
            };

            // Mỗi store tạo 4 nhân viên cơ bản
            int userCounter = 1;
            foreach (var store in existingStores)
            {
                string suffix = store.Replace("STORE-00", "");
                users.Add(($"admin{suffix}@techpro.com", $"Quản Lý {store}", "StoreAdmin", store));
                users.Add(($"support{suffix}@techpro.com", $"Lễ Tân {store}", "Support", store));
                users.Add(($"kho{suffix}@techpro.com", $"Thủ Kho {store}", "Storekeeper", store));
                users.Add(($"tech{suffix}@techpro.com", $"Kỹ Thuật Viên 1 {store}", "Technician", store));
                users.Add(($"tech{suffix}_2@techpro.com", $"Kỹ Thuật Viên 2 {store}", "Technician", store));
            }

            foreach (var userData in users)
            {
                var existingUser = await userManager.FindByEmailAsync(userData.Email);
                if (existingUser == null)
                {
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
                else
                {
                    var currentRoles = await userManager.GetRolesAsync(existingUser);
                    if (!currentRoles.Contains(userData.Role) || currentRoles.Count != 1)
                    {
                        if (currentRoles.Count > 0)
                            await userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                        await userManager.AddToRoleAsync(existingUser, userData.Role);
                    }

                    if (existingUser.TenantId != userData.TenantId)
                    {
                        existingUser.TenantId = userData.TenantId;
                        await userManager.UpdateAsync(existingUser);
                    }
                }
            }

            var rand = new Random();
            var tenantList = await context.CuaHangs.Select(c => c.Id).ToListAsync();
            string[] tenants = tenantList.ToArray();

            // 4. Seed Kho linh kiện (100)
            var existingPartIds = await context.KhoLinhKiens.Select(k => k.Id).ToListAsync();
            if (existingPartIds.Count < 100)
            {
                var items = new List<KhoLinhKien>();
                for (int i = 1; i <= 100; i++)
                {
                    var partId = $"PK-{i:D4}";
                    if (!existingPartIds.Contains(partId))
                    {
                        items.Add(new KhoLinhKien
                        {
                            Id = partId,
                            TenLinhKien = $"Linh kiện cao cấp {i}",
                            DanhMuc = (i % 4 == 0) ? "Màn hình" : (i % 4 == 1 ? "Pin" : (i % 4 == 2 ? "Mainboard" : "Camera")),
                            DanhSachModelTuongThich = $"iPhone {10 + (i % 6)}",
                            GiaBan = 500000 + rand.Next(50000, 4000000),
                            SoLuongTon = rand.Next(5, 50),
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

            // 5. Seed PhieuSuaChua (100)
            if (context.PhieuSuaChuas.Count() < 100)
            {
                var statuses = new[] { "pending", "repairing", "waiting_parts", "done", "delivered" };
                
                var tickets = new List<PhieuSuaChua>();
                for (int i = 1; i <= 100; i++)
                {
                    string ticketId = $"RX-2026-X{i:D3}";
                    if (!context.PhieuSuaChuas.Any(p => p.Id == ticketId))
                    {
                        tickets.Add(new PhieuSuaChua
                        {
                            Id = ticketId,
                            TenKhachHang = $"Khách hàng VIP {i}",
                            SoDienThoai = $"09{rand.Next(10000000, 99999999)}",
                            TenThietBi = $"iPhone {10 + (i % 6)} Pro Max",
                            SerialNumber = $"SN-{100000 + i * 7}",
                            TrangThai = statuses[i % statuses.Length],
                            NgayNhan = DateTime.UtcNow.AddHours(-rand.Next(1, 240)), // Rải rác trong 10 ngày
                            MoTaLoi = $"Mô tả lỗi của khách hàng {i} về màn hình hoặc pin.",
                            TenantId = tenants[rand.Next(tenants.Length)],
                            KetQuaKiemTra = i % 3 == 0 ? "Phát hiện vô nước" : "Lỗi do người dùng",
                            CoBaoHanh = i % 5 == 0,
                            TongTien = 1000000 + rand.Next(100000, 5000000)
                        });
                    }
                }
                if (tickets.Any())
                {
                    context.PhieuSuaChuas.AddRange(tickets);
                    await context.SaveChangesAsync();
                }
            }

            // Lấy lại danh sách phiếu để tạo Note & Scratch...
            var allTicketIds = await context.PhieuSuaChuas.Select(p => p.Id).ToListAsync();

            // 6. Seed TicketNotes (100)
            if (context.TicketNotes.Count() < 100 && allTicketIds.Count > 0)
            {
                var notes = new List<TicketNote>();
                for (int i = 1; i <= 100; i++)
                {
                    notes.Add(new TicketNote
                    {
                        PhieuSuaChuaId = allTicketIds[rand.Next(allTicketIds.Count)],
                        UserName = (i % 2 == 0) ? "Support" : "Technician",
                        Message = $"Ghi chú cập nhật tiến độ {i}",
                        CreatedAt = DateTime.UtcNow.AddMinutes(-rand.Next(1, 1000))
                    });
                }
                context.TicketNotes.AddRange(notes);
                await context.SaveChangesAsync();
            }

            // 7. Seed ScratchMarks (100)
            if (context.ScratchMarks.Count() < 100 && allTicketIds.Count > 0)
            {
                var marks = new List<ScratchMark>();
                for (int i = 1; i <= 100; i++)
                {
                    marks.Add(new ScratchMark
                    {
                        PhieuSuaChuaId = allTicketIds[rand.Next(allTicketIds.Count)],
                        X = rand.NextDouble() * 100,
                        Y = rand.NextDouble() * 100,
                        CreatedAt = DateTime.UtcNow.AddMinutes(-i * 5),
                        CreatedByName = "Technician"
                    });
                }
                context.ScratchMarks.AddRange(marks);
                await context.SaveChangesAsync();
            }

            // 8. Seed RevenueDaily (60 days per tenant)
            if (context.RevenueDailies.Count() < 180) // 3 stores * 60 days
            {
                var list = new List<RevenueDaily>();
                foreach (var t in tenants)
                {
                    for (int i = 0; i < 60; i++)
                    {
                        var day = DateTime.UtcNow.Date.AddDays(-i);
                        if (!context.RevenueDailies.Any(r => r.TenantId == t && r.Ngay == day))
                        {
                            list.Add(new RevenueDaily
                            {
                                Ngay = day,
                                DoanhThu = 5_000_000 + rand.Next(2_000_000, 20_000_000),
                                TenantId = t
                            });
                        }
                    }
                }
                if (list.Any())
                {
                    context.RevenueDailies.AddRange(list);
                    await context.SaveChangesAsync();
                }
            }

            // 9. Seed ThietBiBan (100)
            if (context.ThietBiBans.Count() < 100)
            {
                var devices = new List<ThietBiBan>();
                for (int i = 1; i <= 100; i++)
                {
                    string serial = $"SER-{202600 + i}";
                    if (!context.ThietBiBans.Any(d => d.SerialNumber == serial))
                    {
                        devices.Add(new ThietBiBan
                        {
                            SerialNumber = serial,
                            Model = $"MacBook Pro M{1 + (i % 3)}",
                            NgayMua = DateTime.UtcNow.Date.AddDays(-rand.Next(1, 365)),
                            ThoiHanBaoHanhThang = 12 + (i % 12),
                            TenKhachHang = $"Khách hàng mua máy {i}",
                            TenantId = tenants[rand.Next(tenants.Length)]
                        });
                    }
                }
                if (devices.Any())
                {
                    context.ThietBiBans.AddRange(devices);
                    await context.SaveChangesAsync();
                }
            }

            // 10. Seed LichHen (60)
            if (context.LichHens.Count() < 60)
            {
                var bookings = new List<LichHen>();
                for (int i = 1; i <= 60; i++)
                {
                    bookings.Add(new LichHen
                    {
                        HoTen = $"Khách đặt lịch VIP {i}",
                        SoDienThoai = $"09{rand.Next(10000000, 99999999)}",
                        ThietBi = $"iPad Pro M{1 + (i % 4)}",
                        ChiNhanh = $"TechPro Care {(i % 3 == 0 ? "HK" : (i % 3 == 1 ? "CG" : "SG"))}",
                        NgayHen = DateTime.UtcNow.Date.AddDays(rand.Next(-5, 10)),
                        GioHen = $"{8 + rand.Next(0, 8)}:00",
                        MoTaLoi = $"Mô tả lỗi cần kiểm tra {i}",
                        TenantId = tenants[rand.Next(tenants.Length)],
                        CreatedAt = DateTime.UtcNow.AddDays(-rand.Next(1, 10))
                    });
                }
                context.LichHens.AddRange(bookings);
                await context.SaveChangesAsync();
            }
        }
    }
}

