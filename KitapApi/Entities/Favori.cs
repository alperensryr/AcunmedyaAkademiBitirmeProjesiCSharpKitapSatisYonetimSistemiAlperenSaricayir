using System.ComponentModel.DataAnnotations;

namespace KitapApi.Entities
{
    public class Favori
    {
        [Key]
        public int Id { get; set; }
        public int KullaniciId { get; set; }
        public int KitapId { get; set; }

        public Kullanici? Kullanici { get; set; }
        public Kitap? Kitap { get; set; }
    }
}
//using System.ComponentModel.DataAnnotations;


//public class Favori
//{
//    [Key]
//    public int Id { get; set; }
//    public int KullaniciId { get; set; }
//    public int KitapId { get; set; }

//    public Kullanici Kullanici { get; set; } = null!; // Düzeltme
//    public Kitap Kitap { get; set; } = null!; // Düzeltme
//}
///*public class Favori
//{
//    [Key]
//    public int Id { get; set; }
//    public int KullaniciId { get; set; }
//    public int KitapId { get; set; }

//    public Kullanici Kullanici { get; set; }
//    public Kitap Kitap { get; set; }
//}*/