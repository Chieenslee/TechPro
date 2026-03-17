using Microsoft.EntityFrameworkCore;
using TechPro.API.Data;
using TechPro.API.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TechProDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<NguoiDung, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<TechProDbContext>()
    .AddDefaultTokenProviders();

// CORS: chỉ cho phép MVC gọi vào API — không mở public
// TODO: Cần thêm JWT Bearer Auth cho production. Hiện tại dùng Cookie Identity
// (MVC gọi server-to-server + X-Caller-Email header để định danh)
var mvcOrigin = builder.Configuration["ApiSettings:MvcOrigin"] ?? "https://localhost:7041";
builder.Services.AddCors(options =>
{
    options.AddPolicy("MvcOnly", policy =>
        policy.WithOrigins(mvcOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddSignalR();
builder.Services.AddScoped<TechPro.API.Services.SmartDiagnosisService>();
builder.Services.AddScoped<TechPro.API.Services.AuditLogService>();
builder.Services.Configure<TechPro.API.Services.SmsOptions>(builder.Configuration.GetSection("Sms"));
builder.Services.Configure<TechPro.API.Services.EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddHttpClient();
builder.Services.AddSingleton<TechPro.API.Services.ISmsSender, TechPro.API.Services.HttpSmsSender>();
builder.Services.AddSingleton<TechPro.API.Services.IEmailSender, TechPro.API.Services.SmtpEmailSender>();
builder.Services.AddHttpContextAccessor();
// Swagger (Swashbuckle — bundle local, không cần CDN)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TechPro API",
        Version = "v1",
        Description = """
            API backend cho hệ thống quản lý sửa chữa & bảo hành thiết bị **TechPro**.

            ## Xác thực
            Mọi request từ MVC phải kèm header **X-Caller-Email** chứa email người dùng đã đăng nhập.

            ## Roles
            - `SystemAdmin` – Toàn quyền hệ thống
            - `StoreAdmin` – Quản lý cửa hàng
            - `Support` – Tiếp nhận phiếu sửa chữa
            - `Technician` – Kỹ thuật viên
            - `Storekeeper` – Thủ kho
            """,
        Contact = new OpenApiContact { Name = "TechPro Team" }
    });

    options.AddSecurityDefinition("X-Caller-Email", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "X-Caller-Email",
        Description = "Email người dùng đã đăng nhập. Ví dụ: admin@techpro.vn"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "X-Caller-Email" }
            },
            []
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TechProDbContext>();
        var userManager = services.GetRequiredService<UserManager<NguoiDung>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // This will automatically apply migrations if they are not already applied
        context.Database.Migrate();
        
        await DbInitializer.SeedAsync(userManager, roleManager, context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "TechPro API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseCors("MvcOnly"); // Chỉ MVC origin mới được gọi API

app.UseAuthentication();

// Middleware: đọc X-Caller-Email header từ MVC -> Tự động tạo ClaimsPrincipal
// Giải quyết vấn đề: MVC gọi API nhưng không mang cookie Identity
app.Use(async (context, next) =>
{
    var callerEmail = context.Request.Headers["X-Caller-Email"].FirstOrDefault();
    if (!string.IsNullOrEmpty(callerEmail) && !(context.User?.Identity?.IsAuthenticated ?? false))
    {
        var userManager = context.RequestServices.GetRequiredService<UserManager<TechPro.API.Models.NguoiDung>>();
        var user = await userManager.FindByEmailAsync(callerEmail);
        if (user != null)
        {
            var roles = await userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? callerEmail),
                new Claim(ClaimTypes.Name, user.UserName ?? callerEmail)
            };
            if (!string.IsNullOrEmpty(user.TenantId))
            {
                claims.Add(new Claim("TenantId", user.TenantId));
            }
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
            var identity = new System.Security.Claims.ClaimsIdentity(claims, "XCallerEmail");
            context.User = new System.Security.Claims.ClaimsPrincipal(identity);
        }
    }
    await next();
});

app.UseAuthorization();

app.MapControllers();
app.MapHub<TechPro.API.Hubs.TicketHub>("/ticketHub");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
