using Microsoft.AspNetCore.Mvc;
using KitapMVC.Models.Entities; // SepetItem ve Kitap modelini kullanmak için
using KitapMVC.Services; // KitapApiService kullanmak için
using System.Text.Json; // Session'a JSON olarak kaydetmek için

namespace KitapMVC.Controllers
{
    public class SepetController : Controller
    {
        private readonly IKullaniciApiService _kitapApiService;
        private const string SepetSessionKey = "Sepet"; // Session anahtarı

        public SepetController(IKullaniciApiService kitapApiService)
        {
            _kitapApiService = kitapApiService;
        }

        // Sepetim sayfasını gösterecek metot
        public IActionResult Index()
        {
            List<SepetItem> sepet = GetSepetFromSession();
            return View(sepet);
        }

        [HttpPost] // Sadece POST isteklerini kabul et
        public async Task<IActionResult> SepeteEkle(int kitapId, int adet = 1)
        {
            // API'den kitabı getir
            var kitap = await _kitapApiService.GetKitapByIdAsync(kitapId);
            if (kitap == null)
            {
                return NotFound(); // Kitap bulunamazsa hata döndür
            }

            // Sepeti Session'dan al
            List<SepetItem> sepet = GetSepetFromSession();

            // Kitap zaten sepette mi kontrol et
            var mevcutItem = sepet.FirstOrDefault(item => item.KitapId == kitapId);
            if (mevcutItem != null)
            {
                mevcutItem.Adet += adet; // Adetini artır
            }
            else
            {
                // Yeni SepetItem oluştur
                sepet.Add(new SepetItem
                {
                    KitapId = kitap.Id,
                    KitapAd = kitap.Ad,
                    Fiyat = kitap.Fiyat,
                    Adet = adet,
                    ResimUrl = kitap.ResimUrl
                });
            }

            // Sepeti Session'a geri kaydet
            SaveSepetToSession(sepet);

            TempData["SuccessMessage"] = $"{kitap.Ad} sepete eklendi!";
            return Redirect(Request.Headers["Referer"].ToString()); // Kullanıcıyı geldiği sayfaya geri yönlendir
        }

        // Sepetteki toplam ürün sayısını dönecek metot (AJAX çağrıları için)
        [HttpGet]
        public IActionResult GetSepetItemCount()
        {
            List<SepetItem> sepet = GetSepetFromSession();
            int totalCount = sepet.Sum(item => item.Adet);
            return Json(totalCount); // JSON olarak geri döndür
        }

        // Sepetten ürün silme metodu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SepettenSil(int kitapId)
        {
            List<SepetItem> sepet = GetSepetFromSession();
            var silinecekItem = sepet.FirstOrDefault(item => item.KitapId == kitapId);

            if (silinecekItem != null)
            {
                sepet.Remove(silinecekItem);
                SaveSepetToSession(sepet);
                TempData["InfoMessage"] = $"{silinecekItem.KitapAd} sepetten kaldırıldı.";
            }
            return RedirectToAction("Index"); // Sepetim sayfasına geri yönlendir
        }

        // Sepetteki ürün adedini güncelleme metodu
        [HttpPost]
        public IActionResult AdetGuncelle(int kitapId, int adet)
        {
            List<SepetItem> sepet = GetSepetFromSession();
            var guncellenecekItem = sepet.FirstOrDefault(item => item.KitapId == kitapId);

            if (guncellenecekItem != null)
            {
                if (adet <= 0) // Adet 0 veya daha az ise ürünü sepetten sil
                {
                    sepet.Remove(guncellenecekItem);
                    TempData["InfoMessage"] = $"{guncellenecekItem.KitapAd} sepetten kaldırıldı (adet 0 olduğu için).";
                }
                else
                {
                    guncellenecekItem.Adet = adet;
                    TempData["SuccessMessage"] = $"{guncellenecekItem.KitapAd} adedi {adet} olarak güncellendi.";
                }
                SaveSepetToSession(sepet);
            }
            return RedirectToAction("Index"); // Sepetim sayfasına geri yönlendir
        }

        // Siparişi Tamamla (GET)
        [HttpGet]
        public IActionResult SiparisiTamamla()
        {
            List<SepetItem> sepet = GetSepetFromSession();
            if (sepet == null || !sepet.Any())
            {
                TempData["ErrorMessage"] = "Sepetiniz boş!";
                return RedirectToAction("Index");
            }
            return View(sepet);
        }

        // Siparişi Tamamla (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SiparisiTamamlaOnay(string adSoyad, string email)
        {
            List<SepetItem> sepet = GetSepetFromSession();
            if (sepet == null || !sepet.Any())
            {
                TempData["ErrorMessage"] = "Sepetiniz boş!";
                return RedirectToAction("Index");
            }
            // Kullanıcıyı bul veya oluştur (örnek: email ile kontrol)
            var kullanicilar = await _kitapApiService.GetKullanicilarAsync();
            var kullanici = kullanicilar.FirstOrDefault(u => u.Email == email);
            if (kullanici == null)
            {
                // Yeni kullanıcı kaydı
                kullanici = await _kitapApiService.RegisterAsync(new KitapMVC.Models.Entities.Kullanici
                {
                    AdSoyad = adSoyad,
                    Email = email,
                    SifreHash = Guid.NewGuid().ToString(), // Rastgele şifre, gerçek uygulamada daha iyi bir yöntem gerekir
                    Rol = "User"
                });
            }
            if (kullanici == null)
            {
                TempData["ErrorMessage"] = "Kullanıcı kaydı başarısız!";
                return RedirectToAction("Index");
            }
            // Sipariş ve detaylarını hazırla
            var siparis = new KitapMVC.Models.Entities.Siparis
            {
                KullaniciId = kullanici.Id,
                SiparisTarihi = DateTime.Now,
                ToplamTutar = sepet.Sum(x => x.ToplamFiyat),
                Durum = "Beklemede",
                SiparisDetaylari = sepet.Select(item => new KitapMVC.Models.Entities.SiparisDetay
                {
                    KitapId = item.KitapId,
                    Adet = item.Adet,
                    Fiyat = item.Fiyat
                }).ToList()
            };
            var sonuc = await _kitapApiService.CreateSiparisAsync(siparis);
            if (sonuc == null)
            {
                TempData["ErrorMessage"] = "Sipariş kaydedilemedi!";
                return RedirectToAction("Index");
            }
            // Sepeti temizle
            SaveSepetToSession(new List<SepetItem>());
            TempData["SuccessMessage"] = "Siparişiniz başarıyla tamamlandı!";
            return RedirectToAction("Index");
        }

        // Sepet içeriğini Session'dan okuyan yardımcı metot
        private List<SepetItem> GetSepetFromSession()
        {
            string? sepetJson = HttpContext.Session.GetString(SepetSessionKey);
            if (string.IsNullOrEmpty(sepetJson))
            {
                return new List<SepetItem>();
            }
            // JsonSerializerOptions ile büyük/küçük harf duyarsızlığı ayarlayarak hatayı engelle
            return JsonSerializer.Deserialize<List<SepetItem>>(sepetJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<SepetItem>();
        }

        // Sepet içeriğini Session'a yazan yardımcı metot
        private void SaveSepetToSession(List<SepetItem> sepet)
        {
            string sepetJson = JsonSerializer.Serialize(sepet);
            HttpContext.Session.SetString(SepetSessionKey, sepetJson);
        }
    }
}