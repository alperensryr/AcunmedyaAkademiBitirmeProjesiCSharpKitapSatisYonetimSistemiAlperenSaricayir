using Microsoft.AspNetCore.Mvc;
using KitapMVC.Services;
using KitapMVC.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace KitapMVC.Controllers
{
    public class AdminController : Controller
    {
        private readonly IKullaniciApiService _kullaniciApiService;

        public AdminController(IKullaniciApiService kullaniciApiService)
        {
            _kullaniciApiService = kullaniciApiService;
        }

        // GET: /Admin/Login
        [HttpGet]
        public IActionResult Login()
        {
            ViewData["Title"] = "Admin Girişi";
            return View();
        }

        // POST: /Admin/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var (kullanici, token) = await _kullaniciApiService.LoginAsync(model);

                if (kullanici != null && !string.IsNullOrEmpty(token))
                {
                    // Önceki session'ları temizle (çoklu giriş sorununu önlemek için)
                    HttpContext.Session.Clear();
                    await HttpContext.SignOutAsync("CookieAuth");
                    
                    // JWT token'ı session'a kaydet
                    HttpContext.Session.SetString("JwtToken", token);
                    
                    // Session bilgilerini de set et (layout'ta kullanılıyor)
                    HttpContext.Session.SetInt32("KullaniciId", kullanici.Id);
                    HttpContext.Session.SetString("KullaniciAd", kullanici.AdSoyad);
                    HttpContext.Session.SetString("KullaniciRol", kullanici.Rol);
                    
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
                        new Claim(ClaimTypes.Name, kullanici.AdSoyad),
                        new Claim(ClaimTypes.Email, kullanici.Email),
                        new Claim(ClaimTypes.Role, kullanici.Rol)
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, "CookieAuth");

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
                    };

                    await HttpContext.SignInAsync(
                        "CookieAuth",
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    TempData["SuccessMessage"] = "Başarıyla giriş yaptınız!";
                    return RedirectToAction("Index", "AdminDashboard");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Geçersiz e-posta veya şifre.");
                }
            }
            ViewData["Title"] = "Admin Girişi";
            return View(model);
        }

        // GET: /Admin/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // Tüm session'ı temizle (çoklu giriş sorununu önlemek için)
            HttpContext.Session.Clear();
            
            // Cookie authentication'dan çıkış yap
            await HttpContext.SignOutAsync("CookieAuth");
            
            TempData["InfoMessage"] = "Admin panelinden başarıyla çıkış yaptınız.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Admin/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Admin/ChangePassword
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult ChangePassword()
        {
            ViewData["Title"] = "Şifre Değiştir";
            return View();
        }

        // POST: /Admin/ChangePassword
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var kullaniciId = HttpContext.Session.GetInt32("KullaniciId");
                    if (kullaniciId == null)
                    {
                        return RedirectToAction("Login");
                    }

                    var result = await _kullaniciApiService.ChangePasswordAsync(kullaniciId.Value, model);
                    if (result)
                    {
                        TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi.";
                        return RedirectToAction("Index", "AdminDashboard");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Şifre değiştirme işlemi başarısız. Mevcut şifrenizi kontrol edin.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Bir hata oluştu: {ex.Message}");
                }
            }
            ViewData["Title"] = "Şifre Değiştir";
            return View(model);
        }
    }
}