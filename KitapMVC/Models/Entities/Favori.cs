using System.ComponentModel.DataAnnotations;

namespace KitapMVC.Models.Entities
{
    public class Favori
    {
        [Key]
        public int Id { get; set; }
        public int KullaniciId { get; set; }
        public int KitapId { get; set; }

        public Kullanici Kullanici { get; set; } = null!;
        public Kitap Kitap { get; set; } = null!;
    }
}