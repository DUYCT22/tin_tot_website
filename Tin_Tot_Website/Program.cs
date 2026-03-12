using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tin_Tot_Website.Services;
using Tin_Tot_Website.Services.Messages;
using Tin_Tot_Website.Services.Notifications;
using TinTot.Application.Interfaces.Banners;
using TinTot.Application.Interfaces.Categories;
using TinTot.Application.Interfaces.Contact;
using TinTot.Application.Interfaces.Home;
using TinTot.Application.Interfaces.Images;
using TinTot.Application.Interfaces.Listings;
using TinTot.Application.Interfaces.Messages;
using TinTot.Application.Interfaces.Notifications;
using TinTot.Application.Interfaces.Users;
using TinTot.Application.Services;
using TinTot.Application.Services.Contact;
using TinTot.Application.Services.Home;
using TinTot.Application.Services.Listings;
using TinTot.Application.Services.Messages;
using TinTot.Application.Services.Notifications;
using TinTot.Application.Services.Users;
using TinTot.Infrastructure.Data;
using TinTot.Infrastructure.Repositories;
using TinTot.Infrastructure.Repositories.Home;
using TinTot.Infrastructure.Repositories.Messages;
using TinTot.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
// Disable HTTP/3 (QUIC) because it can crash some local dev environments/drivers.
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureEndpointDefaults(listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
});

// Global exception handlers
AppDomain.CurrentDomain.UnhandledException += (_, e) =>
{
    Console.Error.WriteLine($"[FATAL][UnhandledException] {e.ExceptionObject}");
};

TaskScheduler.UnobservedTaskException += (_, e) =>
{
    Console.Error.WriteLine($"[FATAL][UnobservedTaskException] {e.Exception}");
    e.SetObserved();
};

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddSignalR();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAvatarStorageService, CloudinaryAvatarStorageService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBannerService, BannerService>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<IListingImageService, ListingImageService>();
builder.Services.AddScoped<IInteractionService, InteractionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IContactService, ContactService>();

builder.Services.AddScoped<IPublicListingReadRepository, PublicListingReadRepository>();
builder.Services.AddScoped<IPublicListingQueryService, PublicListingQueryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IBannerRepository, BannerRepository>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();
builder.Services.AddScoped<IListingImageRepository, ListingImageRepository>();
builder.Services.AddScoped<IInteractionRepository, InteractionRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationRealtimePublisher, SignalRNotificationRealtimePublisher>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();
builder.Services.AddScoped<IMessageRealtimePublisher, SignalRMessageRealtimePublisher>();
builder.Services.AddScoped<IHomeReadRepository, HomeReadRepository>();
builder.Services.AddScoped<IHomeQueryService, HomeQueryService>();
builder.Services.AddScoped<IContactEmailSender, SmtpContactEmailSender>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IEntityKeyService, EntityKeyService>();

var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Thiếu cấu hình Jwt:Key");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "TinTot";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "TinTotClient";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (string.IsNullOrEmpty(context.Token)
                    && context.Request.Cookies.TryGetValue("tin_tot_access_token", out var tokenFromCookie))
                {
                    context.Token = tokenFromCookie;
                }

                return Task.CompletedTask;
            }
        };
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnlyPolicy", policy => policy.RequireRole("1"));
    options.AddPolicy("BannerManagePolicy", policy => policy.RequireRole("1", "2"));
    options.AddPolicy("CategoryManagePolicy", policy => policy.RequireRole("1", "3"));
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:Connection"];
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<Tin_Tot_Website.Hubs.NotificationHub>("/hubs/notifications");
app.MapHub<Tin_Tot_Website.Hubs.MessageHub>("/hubs/messages");
// Friendly URL for page
app.MapControllerRoute(
    name: "FriendlyHome",
    pattern: "Trang-Chu",
    defaults: new { controller = "Home", action = "Index" });
//app.MapControllerRoute(
//    name: "FriendlyCreateListing",
//    pattern: "Dang-tin",
//    defaults: new { controller = "MemberListing", action = "Post" });
//app.MapControllerRoute(
//    name: "FriendlyFavorite",
//    pattern: "Tin-da-luu",
//    defaults: new { controller = "MemberListing", action = "Saved" });
//app.MapControllerRoute(
//    name: "FriendlyProfile",
//    pattern: "Trang-ca-nhan",
//    defaults: new { controller = "MemberListing", action = "Profile" });
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Apply pending migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.Run();
