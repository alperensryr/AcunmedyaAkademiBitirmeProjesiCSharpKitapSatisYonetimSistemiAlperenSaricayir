//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KitapApi.Entities
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

        public Kullanici? Kullanici { get; set; }
        public List<SiparisDetay> SiparisDetaylari { get; set; } = new();
    }
}

//public class Siparis
//{
//    [Key]
//    public int Id { get; set; }
//    public int KullaniciId { get; set; }
//    public DateTime SiparisTarihi { get; set; } = DateTime.Now;

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal ToplamTutar { get; set; }

//    public Kullanici Kullanici { get; set; } = null!;
//    public List<SiparisDetay> SiparisDetaylari { get; set; } = new();
//}
////public class Siparis
////{
////    [Key]
////    public int Id { get; set; }
////    public int KullaniciId { get; set; }
////    public DateTime SiparisTarihi { get; set; } = DateTime.Now;

////    [Column(TypeName = "decimal(18,2)")]
////    public decimal ToplamTutar { get; set; }

////    public Kullanici Kullanici { get; set; }
////    public List<SiparisDetay> SiparisDetaylari { get; set; }
//} /* using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//public class Siparis
//{
//    [Key]
//    public int Id { get; set; }
//    public int KullaniciId { get; set; }
//    public DateTime SiparisTarihi { get; set; } = DateTime.Now;

//    [Column(TypeName = "decimal(18,2)")]
//    public decimal ToplamTutar { get; set; }

//    // Düzeltme 1: Bu alanın null olmayacağına emin olduğumuzu belirtiyoruz.
//    public Kullanici Kullanici { get; set; } = null!;

//    // Düzeltme 2: Liste'yi boş bir liste olarak başlatıyoruz.
//    public List<SiparisDetay> SiparisDetaylari { get; set; } = new();
//} */