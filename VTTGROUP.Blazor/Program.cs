using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using VTTGROUP.Blazor.Services.Auth;
using VTTGROUP.Blazor.Services.Hubs;
using VTTGROUP.Domain.Model.Email;
using VTTGROUP.Domain.Model.Hubs;
using VTTGROUP.Infrastructure.Database;
using VTTGROUP.Infrastructure.Services;
using VTTGROUP.Infrastructure.Services.Email;
using Serilog;
using VTTGROUP.Infrastructure.Database2;
using VTTGROUP.Domain.Model.CongTrinh;

var builder = WebApplication.CreateBuilder(args);
//builder.Host.UseSerilog((context, services, configuration) =>
//{
//    configuration
//        .WriteTo.Console()
//        .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day);
//});
//builder.Services.AddDbContext<AppDbContext>(options =>
//{
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
//});
builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddDbContextFactory<ErpBinhPhuCoDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("ErpBinhPhuCo")));

// ✅ THÊM DÒNG NÀY
builder.Services.AddRazorPages(); // BẮT BUỘC nếu dùng _Host.cshtml
//builder.Services.AddServerSideBlazor(); // Để dùng Blazor Server
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    });
var apiBaseUrl = builder.Configuration["BackendApi:BaseUrl"];
builder.Services.AddHttpClient("BackendApi", client =>
{
    //client.BaseAddress = new Uri("https://localhost:7072/");
    client.BaseAddress = new Uri(apiBaseUrl ?? string.Empty);
}).ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler
    {
        UseCookies = true,
        CookieContainer = new System.Net.CookieContainer()
    });

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("BackendApi"));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthorizationService, AuthorizationService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IBaseService, BaseService>();
builder.Services.Configure<EmailSettingsModal>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<CongTrinhSettingsModal>(
    builder.Configuration.GetSection("CongTrinhSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddScoped<CongViecService>();
builder.Services.AddScoped<DuAnService>();
builder.Services.AddScoped<BlockService>();
builder.Services.AddScoped<TangService>();
builder.Services.AddScoped<LoaiCanHoService>();
builder.Services.AddScoped<LoaiGocService>();
builder.Services.AddScoped<ViTriService>();
builder.Services.AddScoped<ViewService>();
builder.Services.AddScoped<ViewTrucService>();
builder.Services.AddScoped<MatKhoiService>();
builder.Services.AddScoped<FileUploadService>();
builder.Services.AddScoped<SanPhamService>();
builder.Services.AddScoped<CanHoService>();
builder.Services.AddScoped<PhanQuyenService>();
builder.Services.AddScoped<KhachHangTamService>();
builder.Services.AddScoped<NhanVienService>();
builder.Services.AddScoped<DMKhachHangService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<DMDotMoBanCanHoService>();
builder.Services.AddScoped<KeHoachBanHangService>();
builder.Services.AddScoped<PhieuDuyetGiaService>();
builder.Services.AddScoped<PhieuGiuChoService>();
builder.Services.AddScoped<GioHangService>();
builder.Services.AddScoped<PhieuDangKyService>();
builder.Services.AddScoped<PhieuDatCocService>();
builder.Services.AddScoped<ThanhLyDatCocService>();
builder.Services.AddScoped<HopDongMuaBanService>();
builder.Services.AddScoped<ThanhLyHopDongService>();
builder.Services.AddScoped<PhuLucHopDongMuaBanService>();
builder.Services.AddScoped<ChinhSachThanhToanService>();
builder.Services.AddScoped<QuyTrinhDuyetService>();
builder.Services.AddScoped<BuocDuyetService>();
builder.Services.AddScoped<DMThietKeService>();
builder.Services.AddScoped<PhieuGiuChoSanGDService>();
builder.Services.AddScoped<PhieuDangKySanGDService>();
builder.Services.AddScoped<INotificationSender, NotificationSender>();
builder.Services.AddScoped<MauInService>();
builder.Services.AddScoped<TemplateService>();
builder.Services.AddScoped<GioHangGroupService>();
builder.Services.AddHostedService<BaseBackgroundService>();
builder.Services.AddScoped<ThongTinCongTyService>();
builder.Services.AddScoped<HopDongChuyenNhuongService>();
builder.Services.Configure<OfficeOptions>(builder.Configuration.GetSection("Office"));
builder.Services.AddSingleton<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<ISessionKiller, SessionKiller>();
builder.Services.AddScoped<DMSanGiaoDichService>();
builder.Services.AddScoped<PhieuCongNoPhaiThuService>();
builder.Services.AddScoped<TongHopBookingService>();
builder.Services.AddScoped<PhieuDeNghiHoanTienBookingService>();
builder.Services.AddScoped<PhieuTongHopCongNoPhaiTraService>();
builder.Services.AddScoped<PhieuXacNhanThanhToanService>();
builder.Services.AddScoped<ChinhSachBanHangService>();
builder.Services.AddSingleton<LoadingService>();
builder.Services.AddSingleton<HuongService>();
builder.Services.AddSingleton<LoaiDienTichService>();
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });
builder.Services.AddAuthorization();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<NotificationHub>("/notificationhub");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();

// ✅ DÙNG ĐÚNG CẤU TRÚC Razor Pages
app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();              // 🧨 KHÔNG ĐƯỢC THIẾU
    endpoints.MapBlazorHub();               // ✅ SignalR
    endpoints.MapFallbackToPage("/_Host");  // ✅ dùng _Host.cshtml làm shell
});

app.Run();
