using KitapMVC.Models.Entities;
using KitapMVC.Models;

namespace KitapMVC.Services
{
    public interface IRaporApiService
    {
        Task<List<Kategori>> GetKategorilerAsync();
        Task<List<Kitap>> GetKitaplarAsync();
        Task<List<Kullanici>> GetKullanicilarAsync();
        Task<List<Favori>> GetFavorilerAsync();
        Task<List<Siparis>> GetSiparislerAsync();
        Task<Siparis?> GetSiparisByIdAsync(int id);
        Task<bool> UpdateSiparisDurumAsync(int id, string durum);
        Task<bool> DeleteSiparisAsync(int id);
        Task<bool> SiparisOnaylaAsync(int id);
        Task<bool> CreateSiparisAsync(Siparis siparis);
    }
}