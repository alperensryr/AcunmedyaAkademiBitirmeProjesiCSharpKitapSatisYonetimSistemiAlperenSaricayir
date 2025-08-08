using Microsoft.AspNetCore.Mvc;
using KitapMVC.Services;
using KitapMVC.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace KitapMVC.Controllers
{
    public class KullaniciController : Controller
    {
        private readonly IKullaniciApiService _kullaniciApiService;

        public KullaniciController(IKullaniciApiService kullaniciApiService)
        {
            _kullaniciApiService = kullaniciApiService;
        }

        // GET: /Kullanici/Login
        [HttpGet]
        public IActionResult Login()
        {
            ViewData["Title"] = "Kullanıcı Girişi";
            return View();
        }

        // POST: /Kullanici/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var (kullanici, token) = await _kullaniciApiService.LoginAsync(model);

                if (kullanici != null)
                {
                    // Önceki session'ları temizle (çoklu giriş sorununu önlemek için)
                    HttpContext.Session.Clear();
                    
                    // Session'a kullanıcı bilgilerini kaydet
                    HttpContext.Session.SetInt32("KullaniciId", kullanici.Id);
                    HttpContext.Session.SetString("KullaniciAd", kullanici.AdSoyad);
                    HttpContext.Session.SetString("KullaniciEmail", kullanici.Email);
                    HttpContext.Session.SetString("KullaniciRol", kullanici.Rol);
                    
                    // JWT token'ı da session'a kaydet (normal kullanıcılar için de gerekebilir)
                    if (!string.IsNullOrEmpty(token))
                    {
                        HttpContext.Session.SetString("JwtToken", token);
                    }
                    
                    TempData["SuccessMessage"] = "Başarıyla giriş yaptınız!";
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");
                }
            }
            ViewData["Title"] = "Kullanıcı Girişi";
            return View(model);
        }

        // GET: /Kullanici/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Tüm session'ı temizle
            HttpContext.Session.Clear();
            
            // Eğer cookie authentication varsa onu da temizle
            try
            {
                await HttpContext.SignOutAsync("CookieAuth");
            }
            catch
            {
                // Cookie auth yoksa hata vermesin
            }
            
            TempData["InfoMessage"] = "Başarıyla çıkış yaptınız.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Kullanici/Register
        [HttpGet]
        public IActionResult Register()
        {
            ViewData["Title"] = "Kullanıcı Kaydı";
            return View();
        }

        // POST: /Kullanici/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var kullanici = new Models.Entities.Kullanici
                {
                    AdSoyad = model.AdSoyad,
                    Email = model.Email,
                    Rol = "User" // Normal kullanıcı rolü
                };

                var result = await _kullaniciApiService.CreateKullaniciAsync(kullanici, model.Sifre);
                if (result != null)
                {
                    TempData["SuccessMessage"] = "Kayıt başarılı! Şimdi giriş yapabilirsiniz.";
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Kayıt sırasında bir hata oluştu.");
                }
            }
            ViewData["Title"] = "Kullanıcı Kaydı";
            return View(model);
        }
    }
}