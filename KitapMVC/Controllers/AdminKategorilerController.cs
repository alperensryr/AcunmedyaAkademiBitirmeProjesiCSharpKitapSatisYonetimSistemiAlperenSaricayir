using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitapMVC.Models;
using KitapMVC.Services;
using KitapMVC.Models.Entities;

namespace KitapMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminKategorilerController : Controller
    {
        private readonly IKullaniciApiService _kitapApiService;

        public AdminKategorilerController(IKullaniciApiService kitapApiService)
        {
            _kitapApiService = kitapApiService;
        }

        // GET: AdminKategoriler
        public async Task<IActionResult> Index()
        {
            try
            {
                var kategoriler = await _kitapApiService.GetKategorilerAsync();
                var viewModels = kategoriler.Select(k => new KategoriViewModel
                {
                    Id = k.Id,
                    Ad = k.Ad ?? string.Empty,
                    Aciklama = k.Aciklama ?? string.Empty
                }).ToList();
                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kategoriler yüklenirken hata oluştu: {ex.Message}";
                return View(new List<KategoriViewModel>());
            }
        }

        // GET: AdminKategoriler/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var kategori = await _kitapApiService.GetKategoriByIdAsync(id);
                if (kategori == null)
                {
                    return NotFound();
                }
                
                // Kategoriye ait kitapları getir
                var kitaplar = await _kitapApiService.GetKitaplarByKategoriAsync(id);
                
                var viewModel = new KategoriViewModel
                {
                    Id = kategori.Id,
                    Ad = kategori.Ad ?? string.Empty,
                    Aciklama = kategori.Aciklama ?? string.Empty,
                    Kitaplar = kitaplar
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kategori detayları yüklenirken hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: AdminKategoriler/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminKategoriler/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KategoriViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var kategori = new Kategori
                    {
                        Ad = model.Ad,
                        Aciklama = model.Aciklama
                    };
                    
                    var yeniKategori = await _kitapApiService.CreateKategoriAsync(kategori);
                    if (yeniKategori != null)
                    {
                        TempData["SuccessMessage"] = "Kategori başarıyla eklendi.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Kategori eklenirken bir hata oluştu.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Hata: {ex.Message}");
                }
            }
            return View(model);
        }

        // GET: AdminKategoriler/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var kategori = await _kitapApiService.GetKategoriByIdAsync(id);
                if (kategori == null)
                {
                    return NotFound();
                }
                var viewModel = new KategoriViewModel
                {
                    Id = kategori.Id,
                    Ad = kategori.Ad,
                    Aciklama = kategori.Aciklama ?? string.Empty
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kategori yüklenirken hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: AdminKategoriler/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KategoriViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Geçersiz kategori ID'si.";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var kategori = new Kategori
                    {
                        Id = model.Id,
                        Ad = model.Ad,
                        Aciklama = model.Aciklama
                    };
                    
                    Console.WriteLine($"Kategori güncelleme başlıyor - ID: {kategori.Id}, Ad: {kategori.Ad}");
                    var basarili = await _kitapApiService.UpdateKategoriAsync(kategori);
                    Console.WriteLine($"Kategori güncelleme sonucu: {basarili}");
                    
                    if (basarili)
                    {
                        TempData["SuccessMessage"] = "Kategori başarıyla güncellendi.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Kategori güncellenirken bir hata oluştu. API'den başarısız yanıt alındı.";
                        Console.WriteLine("Kategori güncelleme başarısız - API false döndü");
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    Console.WriteLine($"HTTP Hatası: {httpEx.Message}");
                    if (httpEx.Message.Contains("401") || httpEx.Message.Contains("Unauthorized"))
                    {
                        TempData["ErrorMessage"] = "Yetkilendirme hatası. Lütfen tekrar giriş yapın.";
                    }
                    else if (httpEx.Message.Contains("400") || httpEx.Message.Contains("BadRequest"))
                    {
                        TempData["ErrorMessage"] = "Geçersiz kategori verisi. Lütfen bilgileri kontrol edin.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = $"Ağ hatası: {httpEx.Message}";
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Genel hata: {ex.Message}");
                    TempData["ErrorMessage"] = $"Beklenmeyen hata: {ex.Message}";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Form verilerinde hata var. Lütfen bilgileri kontrol edin.";
                Console.WriteLine("ModelState geçersiz:");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"- {error.ErrorMessage}");
                }
            }
            
            // Hata durumunda kategorileri yeniden yükle
            try
            {
                var kategoriler = await _kitapApiService.GetKategorilerAsync();
                // Gerekirse ViewBag'e kategoriler eklenebilir
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kategoriler yüklenirken hata: {ex.Message}");
            }
            
            return View(model);
        }

        // GET: AdminKategoriler/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var kategori = await _kitapApiService.GetKategoriByIdAsync(id);
                if (kategori == null)
                {
                    return NotFound();
                }
                var viewModel = new KategoriViewModel
                {
                    Id = kategori.Id,
                    Ad = kategori.Ad ?? string.Empty,
                    Aciklama = kategori.Aciklama ?? string.Empty
                };
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kategori yüklenirken hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: AdminKategoriler/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var (success, errorMessage) = await _kitapApiService.DeleteKategoriAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Kategori başarıyla silindi.";
                }
                else
                {
                    // API'den gelen hata mesajını temizle ve kullanıcı dostu hale getir
                    var cleanErrorMessage = errorMessage.Replace("\"", "").Trim();
                    TempData["ErrorMessage"] = cleanErrorMessage;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}