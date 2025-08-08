using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KitapMVC.Models;
using KitapMVC.Services;

namespace KitapMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminFavorilerController : Controller
    {
        private readonly IKullaniciApiService _apiService;

        public AdminFavorilerController(IKullaniciApiService apiService)
        {
            _apiService = apiService;
        }

        // GET: AdminFavoriler
        public async Task<IActionResult> Index()
        {
            var favoriler = await _apiService.GetFavorilerAsync();
            var vmList = favoriler.Select(f => new FavoriViewModel
            {
                Id = f.Id,
                KullaniciAd = f.Kullanici?.AdSoyad ?? "-",
                KitapAd = f.Kitap?.Ad ?? "-"
            }).ToList();
            return View(vmList);
        }

        // GET: AdminFavoriler/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var favori = await _apiService.GetFavoriByIdAsync(id);
                if (favori == null)
                {
                    return NotFound();
                }

                var viewModel = new FavoriViewModel
                {
                    Id = favori.Id,
                    KullaniciAd = favori.Kullanici?.AdSoyad ?? "Bilinmeyen Kullanıcı",
                    KitapAd = favori.Kitap?.Ad ?? "Bilinmeyen Kitap"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Favori detayları yüklenirken hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: AdminFavoriler/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var favori = await _apiService.GetFavoriByIdAsync(id);
                if (favori == null)
                {
                    return NotFound();
                }

                var viewModel = new FavoriViewModel
                {
                    Id = favori.Id,
                    KullaniciAd = favori.Kullanici?.AdSoyad ?? "Bilinmeyen Kullanıcı",
                    KitapAd = favori.Kitap?.Ad ?? "Bilinmeyen Kitap"
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Favori bilgileri yüklenirken hata oluştu: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: AdminFavoriler/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _apiService.DeleteFavoriAsync(id);
                TempData["SuccessMessage"] = "Favori başarıyla silindi.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Favori silinirken hata oluştu: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}