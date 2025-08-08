using Microsoft.AspNetCore.Mvc;
using KitapMVC.Models.Entities;
using KitapMVC.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace KitapMVC.Controllers
{
    public class FavorilerController : Controller
    {
        private readonly IKullaniciApiService _kitapApiService;
        public FavorilerController(IKullaniciApiService kitapApiService)
        {
            _kitapApiService = kitapApiService;
        }

        // Favorilerim sayfası
        public async Task<IActionResult> Index()
        {
            // Sadece session'dan kullanıcı ID'sini al
            var sessionKullaniciId = HttpContext.Session.GetInt32("KullaniciId");
            
            if (!sessionKullaniciId.HasValue)
            {
                // Kullanıcı giriş yapmamışsa login sayfasına yönlendir
                TempData["ErrorMessage"] = "Favorilerinizi görmek için giriş yapmalısınız.";
                return RedirectToAction("Login", "Kullanici");
            }
            
            var favoriler = await _kitapApiService.GetFavorilerAsync();
            Console.WriteLine($"API'den gelen favori sayısı: {favoriler.Count}");
            
            // Sadece giriş yapmış kullanıcının favorilerini filtrele
            favoriler = favoriler.Where(f => f.KullaniciId == sessionKullaniciId.Value).ToList();
            Console.WriteLine($"Kullanıcı {sessionKullaniciId.Value} için filtrelenmiş favori sayısı: {favoriler.Count}");
            
            return View(favoriler);
        }

        // Favorilere ekle
        [HttpPost]
        public async Task<IActionResult> Ekle(int kitapId, int kullaniciId)
        {
            var favori = new Favori { KitapId = kitapId, KullaniciId = kullaniciId };
            var result = await _kitapApiService.CreateFavoriAsync(favori);
            
            if (result != null)
            {
                TempData["SuccessMessage"] = "Kitap favorilere eklendi!";
            }
            else
            {
                TempData["ErrorMessage"] = "Favori eklenirken bir hata oluştu.";
            }
            
            // Referer header kontrolü
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            else
            {
                return RedirectToAction("Index", "Kitaplar");
            }
        }

        // Favorilerden çıkar
        [HttpPost]
        public async Task<IActionResult> Sil(int favoriId)
        {
            await _kitapApiService.DeleteFavoriAsync(favoriId);
            TempData["InfoMessage"] = "Kitap favorilerden çıkarıldı.";
            return RedirectToAction("Index");
        }
    }
}