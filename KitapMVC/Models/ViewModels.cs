using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using KitapMVC.Models.Entities;

namespace KitapMVC.Models
{
    public class KategoriViewModel
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Aciklama { get; set; } = string.Empty;
        public List<Kitap> Kitaplar { get; set; } = new List<Kitap>();
    }

    public class KitapViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Kitap adı zorunludur.")]
        [Display(Name = "Kitap Adı")]
        public string Ad { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Yazar adı zorunludur.")]
        [Display(Name = "Yazar")]
        public string Yazar { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Fiyat 0'dan büyük olmalıdır.")]
        [Display(Name = "Fiyat")]
        public decimal Fiyat { get; set; }
        
        [Required(ErrorMessage = "Açıklama zorunludur.")]
        [Display(Name = "Açıklama")]
        public string Aciklama { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Kategori seçimi zorunludur.")]
        [Display(Name = "Kategori")]
        public int KategoriId { get; set; }
        
        public string KategoriAd { get; set; } = string.Empty;
        public string ResimUrl { get; set; } = string.Empty;
    }

    public class KullaniciViewModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [Display(Name = "Ad Soyad")]
        public string AdSoyad { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Email alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [Display(Name = "Şifre (İsteğe bağlı)")]
        public string Sifre { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Rol alanı zorunludur.")]
        [Display(Name = "Rol")]
        public string Rol { get; set; } = string.Empty;
        
        public DateTime KayitTarihi { get; set; }
    }

    public class SiparisViewModel
    {
        public int Id { get; set; }
        public string KullaniciAd { get; set; } = string.Empty;
        public string KullaniciEmail { get; set; } = string.Empty;
        public string SiparisTarihi { get; set; } = string.Empty;
        public decimal ToplamTutar { get; set; }
        public string Durum { get; set; } = "Beklemede";
        public List<SiparisDetayViewModel> SiparisDetaylari { get; set; } = new List<SiparisDetayViewModel>();
    }

    public class SiparisDetayViewModel
    {
        public string KitapAd { get; set; } = string.Empty;
        public int Adet { get; set; }
        public decimal Fiyat { get; set; }
    }

    public class FavoriViewModel
    {
        public int Id { get; set; }
        public string KullaniciAd { get; set; } = string.Empty;
        public string KitapAd { get; set; } = string.Empty;
    }
}