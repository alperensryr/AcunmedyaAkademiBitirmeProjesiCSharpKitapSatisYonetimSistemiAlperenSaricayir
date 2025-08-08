using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitapMVC.Models.Entities
{
    public class SiparisDetay
    {
        [Key]
        public int Id { get; set; }
        public int SiparisId { get; set; }
        public int KitapId { get; set; }
        public int Adet { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Fiyat { get; set; }

        public Siparis Siparis { get; set; } = null!;
        public Kitap Kitap { get; set; } = null!;
    }
}