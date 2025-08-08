using System.ComponentModel.DataAnnotations;

namespace KitapMVC.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad Soyad gereklidir.")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre gereklidir.")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Sifre", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string SifreTekrar { get; set; } = string.Empty;
    }
}