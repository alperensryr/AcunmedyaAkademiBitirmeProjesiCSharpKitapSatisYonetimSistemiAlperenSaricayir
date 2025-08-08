using System;
using System.Collections.Generic;

namespace KitapApi.TempModels;

public partial class Kitaplar
{
    public int Id { get; set; }

    public string Ad { get; set; } = null!;

    public string Yazar { get; set; } = null!;

    public decimal Fiyat { get; set; }

    public string? Aciklama { get; set; }

    public string? ResimUrl { get; set; }

    public int KategoriId { get; set; }

    public virtual ICollection<Favoriler> Favorilers { get; set; } = new List<Favoriler>();

    public virtual Kategoriler Kategori { get; set; } = null!;

    public virtual ICollection<SiparisDetaylari> SiparisDetaylaris { get; set; } = new List<SiparisDetaylari>();
}
