using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitapMVC.Models;
using KitapMVC.Models.Entities;
using KitapMVC.Services;

namespace KitapMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminKullanicilarController : Controller
    {
        private readonly IKullaniciApiService _kitapApiService;

        public AdminKullanicilarController(IKullaniciApiService kitapApiService)
        {
            _kitapApiService = kitapApiService;
        }

        // Test action
        public IActionResult Test()
        {
            return Content("AdminKullanicilar Controller çalışıyor! Zaman: " + DateTime.Now.ToString());
        }

        // GET: AdminKullanicilar
        public async Task<IActionResult> Index()
        {
            try
            {
                TempData["DebugInfo"] = "AdminKullanicilar Index action başladı - " + DateTime.Now.ToString("HH:mm:ss");
                
                var kullanicilar = await _kitapApiService.GetKullanicilarAsync();
                TempData["DebugInfo"] += $"<br>API'den {kullanicilar?.Count ?? 0} kullanıcı alındı";
                
                if (kullanicilar != null && kullanicilar.Any())
                {
                    var ilkKullanici = kullanicilar.First();
                    TempData["DebugInfo"] += $"<br>İlk kullanıcı: Id={ilkKullanici.Id}, AdSoyad={ilkKullanici.AdSoyad}, Email={ilkKullanici.Email}";
                }
                
                var kullaniciViewModels = kullanicilar?.Select(k => new KullaniciViewModel
                {
                    Id = k.Id,
                    AdSoyad = k.AdSoyad,
                    Email = k.Email,
                    Rol = k.Rol,
                    KayitTarihi = k.KayitTarihi
                }).ToList() ?? new List<KullaniciViewModel>();
                
                TempData["DebugInfo"] += $"<br>ViewModel'e {kullaniciViewModels.Count} kullanıcı dönüştürüldü";
                
                if (kullaniciViewModels.Any())
                {
                    var ilkViewModel = kullaniciViewModels.First();
                    TempData["DebugInfo"] += $"<br>İlk ViewModel: Id={ilkViewModel.Id}, AdSoyad={ilkViewModel.AdSoyad}";
                }
                
                TempData["DebugInfo"] += "<br>View'a gönderiliyor...";
                return View(kullaniciViewModels);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kullanıcılar yüklenirken bir hata oluştu: " + ex.Message;
                TempData["DebugInfo"] = $"HATA: {ex.Message}<br>StackTrace: {ex.StackTrace}";
                return View(new List<KullaniciViewModel>());
            }
        }

        // GET: AdminKullanicilar/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var kullanici = await _kitapApiService.GetKullaniciByIdAsync(id);
                if (kullanici == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Index");
                }

                var kullaniciViewModel = new KullaniciViewModel
                {
                    Id = kullanici.Id,
                    AdSoyad = kullanici.AdSoyad,
                    Email = kullanici.Email,
                    Rol = kullanici.Rol,
                    KayitTarihi = kullanici.KayitTarihi
                };

                return View(kullaniciViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kullanıcı detayları yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // GET: AdminKullanicilar/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminKullanicilar/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KullaniciViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var kullanici = new Kullanici
                    {
                        AdSoyad = model.AdSoyad,
                        Email = model.Email,
                        Rol = model.Rol,
                        KayitTarihi = DateTime.Now
                    };

                    var result = await _kitapApiService.CreateKullaniciAsync(kullanici, model.Sifre);
                    if (result != null)
                    {
                        TempData["SuccessMessage"] = "Kullanıcı başarıyla eklendi.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Kullanıcı eklenirken bir hata oluştu.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Kullanıcı eklenirken bir hata oluştu: " + ex.Message;
                }
            }
            return View(model);
        }

        // GET: AdminKullanicilar/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var kullanici = await _kitapApiService.GetKullaniciByIdAsync(id);
                if (kullanici == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Index");
                }

                var kullaniciViewModel = new KullaniciViewModel
                {
                    Id = kullanici.Id,
                    AdSoyad = kullanici.AdSoyad,
                    Email = kullanici.Email,
                    Rol = kullanici.Rol,
                    KayitTarihi = kullanici.KayitTarihi
                };

                return View(kullaniciViewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Kullanıcı bilgileri yüklenirken bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        // POST: AdminKullanicilar/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, KullaniciViewModel model)
        {
            if (id != model.Id)
            {
                TempData["ErrorMessage"] = "Geçersiz kullanıcı ID'si.";
                return RedirectToAction("Index");
            }

            // Şifre alanı isteğe bağlı olduğu için ModelState'den kaldır
            if (string.IsNullOrWhiteSpace(model.Sifre))
            {
                ModelState.Remove("Sifre");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mevcut kullanıcı bilgilerini al
                    var mevcutKullanici = await _kitapApiService.GetKullaniciByIdAsync(model.Id);
                    if (mevcutKullanici == null)
                    {
                        TempData["ErrorMessage"] = "Güncellenecek kullanıcı bulunamadı.";
                        return View(model);
                    }

                    var kullanici = new Kullanici
                    {
                        Id = model.Id,
                        AdSoyad = model.AdSoyad,
                        Email = model.Email,
                        Rol = model.Rol,
                        KayitTarihi = model.KayitTarihi,
                        SifreHash = mevcutKullanici.SifreHash // Varsayılan olarak mevcut şifreyi koru
                    };

                    // Eğer yeni şifre girilmişse, şifreyi güncelle
                    if (!string.IsNullOrWhiteSpace(model.Sifre))
                    {
                        var updateResult = await _kitapApiService.UpdateKullaniciWithPasswordAsync(kullanici, model.Sifre);
                        Console.WriteLine($"Kullanıcı şifreli güncelleme sonucu: {updateResult}");
                        
                        if (updateResult)
                        {
                            TempData["SuccessMessage"] = "Kullanıcı ve şifre başarıyla güncellendi.";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Kullanıcı güncellenirken bir hata oluştu. API'den başarısız yanıt alındı.";
                            return View(model);
                        }
                    }
                    else
                    {
                        // Sadece kullanıcı bilgilerini güncelle (şifre değişmez)
                        Console.WriteLine($"Kullanıcı güncelleme başlıyor - ID: {kullanici.Id}, AdSoyad: {kullanici.AdSoyad}, Email: {kullanici.Email}");
                        var result = await _kitapApiService.UpdateKullaniciAsync(kullanici);
                        Console.WriteLine($"Kullanıcı güncelleme sonucu: {result}");
                        
                        if (result)
                        {
                            TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi.";
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Kullanıcı güncellenirken bir hata oluştu. API'den başarısız yanıt alındı.";
                            return View(model);
                        }
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    Console.WriteLine($"HTTP Request Exception: {httpEx.Message}");
                    if (httpEx.Message.Contains("Forbidden") || httpEx.Message.Contains("401"))
                    {
                        TempData["ErrorMessage"] = "Kullanıcı güncellenirken hata oluştu: Yetkiniz bulunmuyor. Lütfen tekrar giriş yapın.";
                    }
                    else if (httpEx.Message.Contains("400"))
                    {
                        TempData["ErrorMessage"] = "Kullanıcı güncellenirken hata oluştu: Geçersiz veri gönderildi. Lütfen form bilgilerini kontrol edin.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Kullanıcı güncellenirken hata oluştu: " + httpEx.Message;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"General Exception: {ex.Message}");
                    TempData["ErrorMessage"] = "Kullanıcı güncellenirken bir hata oluştu: " + ex.Message;
                }
            }
            else
            {
                // ModelState hatalarını göster
                var errors = string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = "Form doğrulama hatası: " + errors;
                Console.WriteLine($"ModelState hatası: {errors}");
            }
            return View(model);
        }

        // GET: AdminKullanicilar/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                TempData["DebugInfo"] = $"Delete action başladı - ID: {id} - Zaman: {DateTime.Now:HH:mm:ss}";
                
                var kullanici = await _kitapApiService.GetKullaniciByIdAsync(id);
                TempData["DebugInfo"] += $"<br>API'den kullanıcı alındı: {(kullanici != null ? "Başarılı" : "Null")}";
                
                if (kullanici == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }
                
                TempData["DebugInfo"] += $"<br>Kullanıcı bilgileri: Id={kullanici.Id}, AdSoyad={kullanici.AdSoyad}, Email={kullanici.Email}";
                
                var viewModel = new KullaniciViewModel
                {
                    Id = kullanici.Id,
                    AdSoyad = kullanici.AdSoyad,
                    Email = kullanici.Email,
                    Rol = kullanici.Rol,
                    KayitTarihi = kullanici.KayitTarihi
                };
                
                TempData["DebugInfo"] += $"<br>ViewModel oluşturuldu: Id={viewModel.Id}, AdSoyad={viewModel.AdSoyad}";
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kullanıcı yüklenirken hata oluştu: {ex.Message}";
                TempData["DebugInfo"] = $"Exception: {ex.Message}<br>StackTrace: {ex.StackTrace}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: AdminKullanicilar/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _kitapApiService.DeleteKullaniciAsync(id);
                if (result)
                {
                    TempData["SuccessMessage"] = "Kullanıcı başarıyla silindi.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Kullanıcı silinirken bir hata oluştu.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kullanıcı silinirken hata oluştu: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }
    }
}