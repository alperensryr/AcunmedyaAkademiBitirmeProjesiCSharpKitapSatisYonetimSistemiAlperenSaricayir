using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitapMVC.Models.Entities
{
    public class Siparis
    {
        [Key]
        public int Id { get; set; }
        public int KullaniciId { get; set; }
        public DateTime SiparisTarihi { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ToplamTutar { get; set; }
        
        public string Durum { get; set; } = "Beklemede"; // Beklemede, Onaylandı, İptal Edildi

        public Kullanici Kullanici { get; set; } = null!;
        public List<SiparisDetay> SiparisDetaylari { get; set; } = new();
    }
}