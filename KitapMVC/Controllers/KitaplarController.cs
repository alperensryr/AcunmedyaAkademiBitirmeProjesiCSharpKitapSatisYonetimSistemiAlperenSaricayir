using KitapMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace KitapMVC.Controllers
{
    public class KitaplarController : Controller
    {
        private readonly IKullaniciApiService _kitapApiService;

        // Dependency Injection ile API servisimizi çağırıyoruz
        public KitaplarController(IKullaniciApiService kitapApiService)
        {
            _kitapApiService = kitapApiService;
        }

        // Bu metot, /Kitaplar adresine gidildiğinde çalışacak
        public async Task<IActionResult> Index()
        {
            try
            {
                var kitaplar = await _kitapApiService.GetKitaplarAsync();
                
                // Debug için
                ViewBag.DebugMessage = $"API'den {kitaplar?.Count ?? 0} kitap geldi";
                
                int? kullaniciId = null;
                if (HttpContext.Session.GetInt32("KullaniciId") != null)
                    kullaniciId = HttpContext.Session.GetInt32("KullaniciId");
                ViewBag.KullaniciId = kullaniciId;
                ViewData["Aciklama"] = "Tüm kitapları kategori, fiyat ve resimleriyle birlikte görebilirsiniz.";
                
                return View(kitaplar);
            }
            catch (Exception ex)
            {
                ViewBag.DebugMessage = $"Hata: {ex.Message}";
                return View(new List<KitapMVC.Models.Entities.Kitap>());
            }
        }

        // GET: Kitaplar/Detail/5 (5 numaralı kitabın detayını gösterir)
        public async Task<IActionResult> Detail(int id)
        {
            var kitap = await _kitapApiService.GetKitapByIdAsync(id);
            if (kitap == null)
            {
                TempData["ErrorMessage"] = "Aradığınız kitap bulunamadı.";
                return RedirectToAction("Index");
            }

            // Kullanıcı ID'sini session'dan al
            int? kullaniciId = null;
            if (HttpContext.Session.GetInt32("KullaniciId") != null)
                kullaniciId = HttpContext.Session.GetInt32("KullaniciId");
            ViewBag.KullaniciId = kullaniciId;

            // Ödevde istenen ViewBag kullanımı için örnek
            ViewBag.SayfaBasligi = "Kitap Detayları";

            return View(kitap);
        }
    }
}