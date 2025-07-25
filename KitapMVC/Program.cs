using KitapMVC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient<KitapApiService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7001");
});

builder.Services.AddControllersWithViews();
// ----- BURAYA EKLENECEK KODLAR (AddHttpClient'�n alt�na ekleyebilirsin) -----
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturumun 30 dakika bo�ta kal�rsa sona ermesini sa�lar
    options.Cookie.HttpOnly = true; // Taray�c� taraf�ndaki scriptlerin cookie'ye eri�imini engeller
    options.Cookie.IsEssential = true; // GDPR uyumlulu�u i�in, session cookie'sinin gerekli oldu�unu belirtir
});
// -------------------------------------------------------------------------
var app = builder.Build();
// ----- BURAYA EKLENECEK KOD -----
app.UseSession(); // Session servisini kullan
// ---------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
//using KitapMVC.Services;

//var builder = WebApplication.CreateBuilder(args);

//// ----- AddHttpClient KODU BURADA, DO�RU PORT NUMARASIYLA -----
//builder.Services.AddHttpClient<KitapApiService>(client =>
//{
//    // API'nin �al��t���, daha �nce sabitledi�imiz adresi yaz�yoruz
//    client.BaseAddress = new Uri("https://localhost:7001");
//});
//// -------------------------------------------------------------

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles(); // Bu sat�r �nemli, statik dosyalar i�in
//app.UseRouting();

//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.Run();
////using KitapMVC.Services;
////var builder = WebApplication.CreateBuilder(args);
////// ----- BURAYA EKLENECEK KOD -----
////builder.Services.AddHttpClient<KitapApiService>(client =>
////{
////    // KitapApi'nin �al��t��� adresi buraya yaz�yoruz.
////    // Bu adresi KitapApi projesinin launchSettings.json dosyas�ndan bulabilirsin.
////    client.BaseAddress = new Uri("https://localhost:7158");
////});
////// ------------------------------------
////// Add services to the container.
////builder.Services.AddControllersWithViews();

////var app = builder.Build();

////// Configure the HTTP request pipeline.
////if (!app.Environment.IsDevelopment())
////{
////    app.UseExceptionHandler("/Home/Error");
////    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
////    app.UseHsts();
////}

////app.UseHttpsRedirection();
////app.UseRouting();

////app.UseAuthorization();

////app.MapStaticAssets();

////app.MapControllerRoute(
////    name: "default",
////    pattern: "{controller=Home}/{action=Index}/{id?}")
////    .WithStaticAssets();


////app.Run();
