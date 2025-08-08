using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitapMVC.Models.Entities
{
    public class Kullanici
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string AdSoyad { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string SifreHash { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Rol { get; set; } = "User";

        public DateTime KayitTarihi { get; set; } = DateTime.Now;
    }
}