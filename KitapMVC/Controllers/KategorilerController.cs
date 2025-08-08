using KitapMVC.Services;
using Microsoft.AspNetCore.Mvc;
using KitapMVC.Models;

namespace KitapMVC.Controllers
{
    public class KategorilerController : Controller
    {
        private readonly IKullaniciApiService _kitapApiService;

        public KategorilerController(IKullaniciApiService kitapApiService)
        {
            _kitapApiService = kitapApiService;
        }

        // Bu metot, /Kategoriler adresine gidildiğinde çalışacak
        public async Task<IActionResult> Index()
        {
            try
            {
                var kategoriler = await _kitapApiService.GetKategorilerAsync();
                
                // Debug için
                ViewBag.DebugMessage = $"API'den {kategoriler?.Count ?? 0} kategori geldi";
                
                return View(kategoriler);
            }
            catch (Exception ex)
            {
                ViewBag.DebugMessage = $"Hata: {ex.Message}";
                return View(new List<KitapMVC.Models.Entities.Kategori>());
            }
        }
        // GET: Kategoriler/Detail/5 (5 numaralı kategorinin detayını gösterir)
        public async Task<IActionResult> Detail(int id)
        {
            try
            {
                var kategori = await _kitapApiService.GetKategoriByIdAsync(id); // API servisinden kategoriyi çek
                if (kategori == null)
                {
                    return NotFound(); // Kategori bulunamazsa 404 sayfasına yönlendir
                }

                // Kategoriye ait kitapları getir
                var kitaplar = await _kitapApiService.GetKitaplarByKategoriAsync(id);
                
                // Kullanıcı ID'sini session'dan al
                int? kullaniciId = null;
                if (HttpContext.Session.GetInt32("KullaniciId") != null)
                    kullaniciId = HttpContext.Session.GetInt32("KullaniciId");
                ViewBag.KullaniciId = kullaniciId;

                var viewModel = new KategoriViewModel
                {
                    Id = kategori.Id,
                    Ad = kategori.Ad,
                    Aciklama = kategori.Aciklama ?? string.Empty,
                    Kitaplar = kitaplar
                };

                ViewBag.SayfaBasligi = "Kategori Detayları"; // ViewBag örneği
                return View(viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.DebugMessage = $"Hata: {ex.Message}";
                return View(new KategoriViewModel());
            }
        }
    }
}
