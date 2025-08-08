using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitapMVC.Models.Entities
{
    public class Kitap
    {
        [Key]
        public int Id { get; set; }
        public string Ad { get; set; } = null!;
        public string Yazar { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Fiyat { get; set; }
        public string? Aciklama { get; set; }
        public string? ResimUrl { get; set; }

        public int KategoriId { get; set; }
        public Kategori? Kategori { get; set; }
    }
}