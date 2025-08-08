
using KitapMVC.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
});

// KitapApiService için HttpClient yapılandırması
builder.Services.AddHttpClient<KitapApiService>(client =>
{
    var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:7011/";
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
    Console.WriteLine($"API Base Address set to: {client.BaseAddress}");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    // Development ortamında SSL sertifika doğrulamasını atla
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
    }
    return handler;
});

// IHttpContextAccessor'ı ekle
builder.Services.AddHttpContextAccessor();

// KitapApiService'i hem kendisi hem de interface olarak kaydet
builder.Services.AddScoped<KitapApiService>();
builder.Services.AddScoped<IKullaniciApiService, KitapApiService>();
builder.Services.AddScoped<IRaporApiService, KitapApiService>();

// Cookie tabanlı kimlik doğrulama ekle
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "KitapMVC.AuthCookie";
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Admin/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    });

// MVC ve Session servislerini ekle
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Uygulama oluştur ve middleware'leri yapılandır
var app = builder.Build();

// Geliştirme ortamına göre hata yönetimini yapılandır
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware pipeline'ını yapılandır
// HTTP kullanıldığı için HTTPS yönlendirmesini devre dışı bırakıyoruz
// app.UseHttpsRedirection();
Console.WriteLine($"API Base URL from config: {app.Configuration["ApiSettings:BaseUrl"]}");

// HTTP isteklerini log'la
app.Use(async (context, next) =>
{
    Console.WriteLine($"HTTP {context.Request.Method} {context.Request.Path} - {DateTime.Now:HH:mm:ss}");
    await next();
});

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// Route yapılandırması
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Uygulamayı başlat
app.Run();