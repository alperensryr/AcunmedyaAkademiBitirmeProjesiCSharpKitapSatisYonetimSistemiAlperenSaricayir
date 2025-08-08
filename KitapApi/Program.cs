using Microsoft.EntityFrameworkCore;
using KitapApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")
});

// 1. Veritabanı bağlantısı ayarları
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// 2. CORS ayarları eklendi
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowKitapMVC", policy =>
    {
        policy.WithOrigins("https://localhost:7002", "http://localhost:7003", "http://localhost:5262", "http://localhost:7000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 3. Servisleri konteynere ekleme
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "supersecretkey12345";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "KitapApi";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Swagger için servisler
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// HTTP istek hattını yapılandırma
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTP bağlantıları için HTTPS yönlendirmesini devre dışı bırakıyoruz
// app.UseHttpsRedirection();

// CORS'u authentication'dan önce ekleyin
app.UseCors("AllowKitapMVC");

// Static files middleware'i ekle
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Veri seeding işlemi
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await KitapApi.Data.DataSeeder.SeedDataAsync(context);
}

app.Run();

// Sınıf tanımları dosyanın sonunda olmalı
public static class StartupStatic
{
    public static string JwtKey = "supersecretkey12345supersecretkey12345";
    public static string JwtIssuer = "KitapApi";
}