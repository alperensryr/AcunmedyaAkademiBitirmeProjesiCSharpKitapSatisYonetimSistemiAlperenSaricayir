namespace KitapMVC.Models.Entities
{
    public class SepetItem
    {
        public int KitapId { get; set; }
        public string KitapAd { get; set; } = string.Empty;
        public decimal Fiyat { get; set; }
        public int Adet { get; set; }
        public string? ResimUrl { get; set; } // Sepette resim göstermek için
        public decimal ToplamFiyat => Fiyat * Adet; // Otomatik hesaplanan toplam fiyat
    }
}