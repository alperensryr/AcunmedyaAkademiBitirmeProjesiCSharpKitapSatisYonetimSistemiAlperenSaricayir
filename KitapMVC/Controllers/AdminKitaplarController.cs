using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitapMVC.Models;
using KitapMVC.Models.Entities;
using KitapMVC.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KitapMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminKitaplarController : Controller
    {
        private readonly IKullaniciApiService _kitapApiService;

        public AdminKitaplarController(IKullaniciApiService kitapApiService)
        {
            _kitapApiService = kitapApiService;
        }

        // GET: AdminKitaplar
        public async Task<IActionResult> Index()
        {
            try
            {
                var kitaplar = await _kitapApiService.GetKitaplarAsync();
                return View(kitaplar);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kitaplar yüklenirken hata oluştu: " + ex.Message;
                return View(new List<KitapMVC.Models.Entities.Kitap>());
            }
        }

        // GET: AdminKitaplar/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var kitap = await _kitapApiService.GetKitapByIdAsync(id);
                if (kitap == null)
                {
                    return NotFound();
                }

                var model = new KitapViewModel
                {
                    Id = kitap.Id,
                    Ad = kitap.Ad ?? string.Empty,
                    Yazar = kitap.Yazar ?? string.Empty,
                    Fiyat = kitap.Fiyat,
                    KategoriId = kitap.KategoriId,
                    KategoriAd = kitap.Kategori?.Ad ?? "Kategori Bulunamadı",
                    Aciklama = kitap.Aciklama ?? string.Empty,
                    ResimUrl = kitap.ResimUrl ?? string.Empty
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kitap detayları yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: AdminKitaplar/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                var kategoriler = await _kitapApiService.GetKategorilerAsync();
                ViewBag.Kategoriler = new SelectList(kategoriler, "Id", "Ad");
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kategoriler yüklenirken hata oluştu: " + ex.Message;
                ViewBag.Kategoriler = new SelectList(new List<object>(), "Id", "Ad");
                return View();
            }
        }

        // POST: AdminKitaplar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KitapViewModel model, IFormFile ResimDosyasi)
        {
            // Resim dosyası kontrolü
            if (ResimDosyasi == null || ResimDosyasi.Length == 0)
            {
                ModelState.AddModelError("ResimDosyasi", "Kitap resmi seçilmesi zorunludur.");
            }
            else
            {
                // Dosya türü kontrolü
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(ResimDosyasi.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ResimDosyasi", "Sadece JPG, JPEG, PNG ve GIF dosyaları kabul edilir.");
                }
                
                // Dosya boyutu kontrolü (5MB)
                if (ResimDosyasi.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ResimDosyasi", "Dosya boyutu 5MB'dan küçük olmalıdır.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var kitap = new KitapMVC.Models.Entities.Kitap
                    {
                        Ad = model.Ad,
                        Yazar = model.Yazar,
                        Fiyat = model.Fiyat,
                        KategoriId = model.KategoriId,
                        Aciklama = model.Aciklama
                    };

                    var createdKitap = await _kitapApiService.CreateKitapAsync(kitap);
                    
                    if (createdKitap != null)
                    {
                        // Resim yükleme işlemi
                        var uploadResult = await _kitapApiService.UploadKitapImageAsync(createdKitap.Id, ResimDosyasi);
                        if (string.IsNullOrEmpty(uploadResult))
                        {
                            TempData["WarningMessage"] = "Kitap eklendi ancak resim yüklenirken bir sorun oluştu.";
                        }
                        else
                        {
                            TempData["SuccessMessage"] = "Kitap ve resim başarıyla eklendi.";
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Kitap oluşturulamadı. Lütfen tekrar deneyin.";
                        // Kategorileri tekrar yükle ve formu göster
                        await LoadKategorilerForView(model.KategoriId);
                        return View(model);
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Kitap eklenirken hata oluştu: " + ex.Message;
                }
            }

            // Hata durumunda kategorileri tekrar yükle
            await LoadKategorilerForView(model.KategoriId);
            return View(model);
        }

        private async Task LoadKategorilerForView(int selectedKategoriId = 0)
        {
            try
            {
                var kategoriler = await _kitapApiService.GetKategorilerAsync();
                ViewBag.Kategoriler = new SelectList(kategoriler, "Id", "Ad", selectedKategoriId);
            }
            catch
            {
                ViewBag.Kategoriler = new SelectList(new List<object>(), "Id", "Ad");
            }
        }

        // GET: AdminKitaplar/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var kitap = await _kitapApiService.GetKitapByIdAsync(id);
                if (kitap == null)
                {
                    return NotFound();
                }

                var kategoriler = await _kitapApiService.GetKategorilerAsync();
                ViewBag.Kategoriler = new SelectList(kategoriler, "Id", "Ad", kitap.KategoriId);

                var model = new KitapViewModel
                {
                    Id = kitap.Id,
                    Ad = kitap.Ad ?? string.Empty,
                    Yazar = kitap.Yazar ?? string.Empty,
                    Fiyat = kitap.Fiyat,
                    KategoriId = kitap.KategoriId,
                    Aciklama = kitap.Aciklama ?? string.Empty,
                    ResimUrl = kitap.ResimUrl ?? string.Empty
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kitap bilgileri yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: AdminKitaplar/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KitapViewModel model, IFormFile ResimDosyasi)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Geçersiz kitap ID'si.";
                return RedirectToAction(nameof(Index));
            }

            // Resim dosyası seçildiyse validasyon yap
            if (ResimDosyasi != null && ResimDosyasi.Length > 0)
            {
                // Dosya türü kontrolü
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(ResimDosyasi.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    ModelState.AddModelError("ResimDosyasi", "Sadece JPG, JPEG, PNG ve GIF dosyaları kabul edilir.");
                }
                
                // Dosya boyutu kontrolü (5MB)
                if (ResimDosyasi.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ResimDosyasi", "Dosya boyutu 5MB'dan küçük olmalıdır.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var kitap = new KitapMVC.Models.Entities.Kitap
                    {
                        Id = model.Id,
                        Ad = model.Ad,
                        Yazar = model.Yazar,
                        Fiyat = model.Fiyat,
                        KategoriId = model.KategoriId,
                        Aciklama = model.Aciklama,
                        ResimUrl = model.ResimUrl
                    };

                    Console.WriteLine($"Kitap güncelleme başlıyor - ID: {kitap.Id}, Ad: {kitap.Ad}");
                    var updateResult = await _kitapApiService.UpdateKitapAsync(kitap);
                    Console.WriteLine($"Kitap güncelleme sonucu: {updateResult}");
                    
                    if (updateResult)
                    {
                        if (ResimDosyasi != null)
                        {
                            try
                            {
                                await _kitapApiService.UploadKitapImageAsync(id, ResimDosyasi);
                                Console.WriteLine("Resim yükleme başarılı");
                            }
                            catch (Exception imgEx)
                            {
                                Console.WriteLine($"Resim yükleme hatası: {imgEx.Message}");
                                TempData["WarningMessage"] = "Kitap güncellendi ancak resim yüklenirken hata oluştu: " + imgEx.Message;
                            }
                        }

                        TempData["SuccessMessage"] = "Kitap başarıyla güncellendi.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Kitap güncellenirken bir hata oluştu. API'den başarısız yanıt alındı.";
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    Console.WriteLine($"HTTP Request Exception: {httpEx.Message}");
                    // API çağrısı ile ilgili özel hata mesajı
                    if (httpEx.Message.Contains("Forbidden") || httpEx.Message.Contains("401"))
                    {
                        TempData["ErrorMessage"] = "Kitap güncellenirken hata oluştu: Yetkiniz bulunmuyor. Lütfen tekrar giriş yapın.";
                    }
                    else if (httpEx.Message.Contains("400"))
                    {
                        TempData["ErrorMessage"] = "Kitap güncellenirken hata oluştu: Geçersiz veri gönderildi. Lütfen form bilgilerini kontrol edin.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Kitap güncellenirken hata oluştu: " + httpEx.Message;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Exception: {ex.Message}");
                    TempData["ErrorMessage"] = "Kitap güncellenirken hata oluştu: " + ex.Message;
                }
            }
            else
            {
                // ModelState hatalarını göster
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = "Form doğrulama hatası: " + errors;
                Console.WriteLine($"ModelState hatası: {errors}");
            }

            // Hata durumunda kategorileri tekrar yükle
            try
            {
                var kategoriler = await _kitapApiService.GetKategorilerAsync();
                ViewBag.Kategoriler = new SelectList(kategoriler, "Id", "Ad", model.KategoriId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kategori yükleme hatası: {ex.Message}");
                ViewBag.Kategoriler = new SelectList(new List<object>(), "Id", "Ad");
            }
            return View(model);
        }

        // GET: AdminKitaplar/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var kitap = await _kitapApiService.GetKitapByIdAsync(id);
                if (kitap == null)
                {
                    return NotFound();
                }
                
                // Kitap entity'sini KitapViewModel'e dönüştür
                var viewModel = new KitapViewModel
                {
                    Id = kitap.Id,
                    Ad = kitap.Ad,
                    Yazar = kitap.Yazar,
                    Fiyat = kitap.Fiyat,
                    KategoriId = kitap.KategoriId,
                    KategoriAd = kitap.Kategori?.Ad ?? "Bilinmiyor"
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kitap bilgileri yüklenirken hata oluştu: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: AdminKitaplar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _kitapApiService.DeleteKitapAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Kitap başarıyla silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Kitap silinirken bir hata oluştu. Kitap bulunamadı veya silme işlemi başarısız oldu.";
                }
            }
            catch (HttpRequestException httpEx)
            {
                // API çağrısı ile ilgili özel hata mesajı
                if (httpEx.Message.Contains("Forbidden"))
                {
                    TempData["ErrorMessage"] = "Kitap silinirken hata oluştu: Yetkiniz bulunmuyor. Lütfen tekrar giriş yapın.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Kitap silinirken hata oluştu: " + httpEx.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kitap silinirken hata oluştu: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}