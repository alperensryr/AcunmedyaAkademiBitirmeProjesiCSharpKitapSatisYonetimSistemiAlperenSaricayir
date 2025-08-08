using KitapMVC.Models.Entities;
using KitapMVC.Models;
using Microsoft.AspNetCore.Http;

namespace KitapMVC.Services
{
    public interface IKullaniciApiService
    {
        // Kullanıcı metotları
        Task<(Kullanici? kullanici, string? token)> LoginAsync(LoginViewModel loginModel);
        Task<Kullanici?> RegisterAsync(Kullanici kullanici);
        Task<Kullanici?> CreateKullaniciAsync(Kullanici kullanici, string sifre);
        Task<List<Kullanici>> GetKullanicilarAsync();
        Task<Kullanici?> GetKullaniciByIdAsync(int id);
        Task<bool> UpdateKullaniciAsync(Kullanici kullanici);
        Task<bool> UpdateKullaniciWithPasswordAsync(Kullanici kullanici, string yeniSifre);
        Task<bool> DeleteKullaniciAsync(int id);
        Task<bool> ChangePasswordAsync(int kullaniciId, ChangePasswordViewModel model);
        
        // Favori metotları
        Task<List<Favori>> GetFavorilerAsync();
        Task<Favori?> GetFavoriByIdAsync(int id);
        Task<bool> CreateFavoriAsync(Favori favori);
        Task<bool> DeleteFavoriAsync(int id);
        
        // Kitap metotları
        Task<List<Kitap>> GetKitaplarAsync();
        Task<Kitap?> GetKitapByIdAsync(int id);
        Task<bool> UpdateKitapAsync(Kitap kitap);
        Task<string?> UploadKitapImageAsync(int kitapId, IFormFile file);
        Task<Kitap?> CreateKitapAsync(Kitap kitap);
        Task<bool> DeleteKitapAsync(int id);
        Task<List<Kitap>> GetKitaplarByKategoriAsync(int kategoriId);
        
        // Kategori metotları
        Task<List<Kategori>> GetKategorilerAsync();
        Task<Kategori?> GetKategoriByIdAsync(int id);
        Task<Kategori?> CreateKategoriAsync(Kategori kategori);
        Task<bool> UpdateKategoriAsync(Kategori kategori);
        Task<(bool success, string? errorMessage)> DeleteKategoriAsync(int id);
        
        // Sipariş metotları
        Task<bool> CreateSiparisAsync(Siparis siparis);
    }
}