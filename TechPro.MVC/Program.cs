using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TechPro.Models;

namespace TechPro
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configuration for API Client
            var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] 
                             ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");

            // Needed by CallerEmailHandler to access current user
            builder.Services.AddHttpContextAccessor();

            // Handler that injects X-Caller-Email into every API request
            builder.Services.AddTransient<TechPro.Services.CallerEmailHandler>();

            // Client cho API — tự động đính email người dùng vào header
            builder.Services.AddHttpClient("TechProAPI", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            }).AddHttpMessageHandler<TechPro.Services.CallerEmailHandler>();
            
            // Authentication Authentication (Cookie) without Identity
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
            });

            // Cấu hình Cookie Authentication - Configured above in AddCookie
            // builder.Services.ConfigureApplicationCookie(options =>
            // {
            //     options.LoginPath = "/Account/Login";
            //     options.LogoutPath = "/Account/Logout";
            //     options.AccessDeniedPath = "/Account/AccessDenied";
            //     options.ExpireTimeSpan = TimeSpan.FromDays(30);
            //     options.SlidingExpiration = true;
            // });

            // Local AI Models handles requests on API server


            builder.Services.AddControllersWithViews();
            // SignalR was previously used for internal chat UI; keeping disabled for now.

            var app = builder.Build();

            // Cấu hình HTTP request pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Middleware xác thực và phân quyền
            app.UseAuthentication();
            app.UseAuthorization();

            // Map SignalR Hub - Moved to API
            // app.MapHub<Hubs.TicketHub>("/ticketHub");
            // app.MapHub<...> removed (internal chat deleted)

            // Route cho Support module (hỗ trợ URL dạng /Support/TiepNhan/ChiTiet/{id})
            app.MapControllerRoute(
                name: "support",
                pattern: "Support/{controller=TiepNhan}/{action=Index}/{id?}");

            // Route cho Technician module
            app.MapControllerRoute(
                name: "technician",
                pattern: "Technician/{controller=KyThuat}/{action=Index}/{id?}");

            // Route mặc định
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Seed database với dữ liệu mẫu - Moved to API
            /*
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var userManager = services.GetRequiredService<UserManager<NguoiDung>>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var context = services.GetRequiredService<TechProDbContext>();
                    await DbInitializer.SeedAsync(userManager, roleManager, context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Lỗi khi seed database");
                }
            }
            */

            app.Run();
        }
    }
}
