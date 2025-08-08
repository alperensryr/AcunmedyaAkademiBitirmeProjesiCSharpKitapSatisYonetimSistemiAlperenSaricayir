using System.ComponentModel.DataAnnotations;

namespace KitapMVC.Models.Entities
{
    public class Kategori
    {
        [Key]
        public int Id { get; set; }
        public string Ad { get; set; } = null!;
        public string? Aciklama { get; set; }
    }
}