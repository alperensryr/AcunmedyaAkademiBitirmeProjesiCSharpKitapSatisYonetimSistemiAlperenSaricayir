using System.ComponentModel.DataAnnotations;

namespace KitapMVC.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Mevcut şifre gereklidir.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string MevcutSifre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifre gereklidir.")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string YeniSifre { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre Tekrar")]
        [Compare("YeniSifre", ErrorMessage = "Yeni şifreler eşleşmiyor.")]
        public string YeniSifreTekrar { get; set; } = string.Empty;
    }
}