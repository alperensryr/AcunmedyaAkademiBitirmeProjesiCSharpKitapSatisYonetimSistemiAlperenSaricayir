using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KitapMVC.Models;
using KitapMVC.Services;

namespace KitapMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly IRaporApiService _raporApiService;

        public AdminDashboardController(IRaporApiService raporApiService)
        {
            _raporApiService = raporApiService;
        }
        public IActionResult Index()
        {
            ViewData["Title"] = "Admin Dashboard";
            return View();
        }

        public IActionResult Ayarlar()
        {
            ViewData["Title"] = "Ayarlar";
            return View();
        }

        public async Task<IActionResult> Raporlar()
        {
            ViewData["Title"] = "Raporlar";
            
            try
            {
                // API'den verileri çek
                var kitaplar = await _raporApiService.GetKitaplarAsync();
                var kullanicilar = await _raporApiService.GetKullanicilarAsync();
                var kategoriler = await _raporApiService.GetKategorilerAsync();
                var siparisler = await _raporApiService.GetSiparislerAsync();
                
                // En çok sipariş edilen kitapları hesapla
                var enCokSiparisEdilenKitaplar = siparisler
                    .SelectMany(s => s.SiparisDetaylari)
                    .GroupBy(sd => sd.KitapId)
                    .Select(g => new
                    {
                        KitapId = g.Key,
                        ToplamAdet = g.Sum(sd => sd.Adet)
                    })
                    .OrderByDescending(x => x.ToplamAdet)
                    .Take(10)
                    .Select(x => new
                    {
                        Kitap = kitaplar.FirstOrDefault(k => k.Id == x.KitapId),
                        SiparisAdedi = x.ToplamAdet
                    })
                    .Where(x => x.Kitap != null)
                    .Select(x => (x.Kitap, x.SiparisAdedi))
                    .ToList();

                // Rapor modelini oluştur
                var raporModel = new RaporViewModel
                {
                    ToplamKitap = kitaplar.Count,
                    ToplamKullanici = kullanicilar.Count,
                    ToplamKategori = kategoriler.Count,
                    BuAySiparis = siparisler.Count(s => s.SiparisTarihi.Month == DateTime.Now.Month && s.SiparisTarihi.Year == DateTime.Now.Year),
                    SonSiparisler = siparisler.OrderByDescending(s => s.SiparisTarihi).Take(5).ToList(),
                    PopulerKitaplar = kitaplar.Take(5).ToList(),
                    EnCokSiparisEdilenKitaplar = enCokSiparisEdilenKitaplar
                };


                
                // Kategori dağılımını hesapla
                foreach (var kategori in kategoriler)
                {
                    var kategoriKitapSayisi = kitaplar.Count(k => k.KategoriId == kategori.Id);
                    raporModel.KategoriDagilimi[kategori.Ad] = kategoriKitapSayisi;
                }
                
                // Aylık sipariş verilerini hesapla (son 6 ay)
                for (int i = 5; i >= 0; i--)
                {
                    var ay = DateTime.Now.AddMonths(-i);
                    var ayAdi = ay.ToString("MMMM", new System.Globalization.CultureInfo("tr-TR"));
                    var aySiparisSayisi = siparisler.Count(s => s.SiparisTarihi.Month == ay.Month && s.SiparisTarihi.Year == ay.Year);
                    raporModel.AylikSiparisler[ayAdi] = aySiparisSayisi;
                }
                
                Console.WriteLine($"Toplam Kitap: {raporModel.ToplamKitap}");
                Console.WriteLine($"Kategori Dağılımı: {string.Join(", ", raporModel.KategoriDagilimi.Select(x => $"{x.Key}:{x.Value}"))}");
                Console.WriteLine($"Aylık Siparişler: {string.Join(", ", raporModel.AylikSiparisler.Select(x => $"{x.Key}:{x.Value}"))}");
                
                return View(raporModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Rapor verileri yüklenirken hata: {ex.Message}");
                // Hata durumunda boş model döndür
                return View(new RaporViewModel());
            }
        }

        [HttpPost]
        public IActionResult ToggleTheme(string theme)
        {
            HttpContext.Session.SetString("Theme", theme);
            return Json(new { success = true });
        }
    }
}