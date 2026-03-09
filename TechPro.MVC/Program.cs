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

            // Client cho API
            builder.Services.AddHttpClient("TechProAPI", client =>
            {
                client.BaseAddress = new Uri(apiBaseUrl);
            });
            
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
