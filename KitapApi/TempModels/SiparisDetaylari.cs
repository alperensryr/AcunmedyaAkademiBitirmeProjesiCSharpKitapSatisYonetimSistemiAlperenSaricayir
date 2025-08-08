using System;
using System.Collections.Generic;

namespace KitapApi.TempModels;

public partial class SiparisDetaylari
{
    public int Id { get; set; }

    public int SiparisId { get; set; }

    public int KitapId { get; set; }

    public int Adet { get; set; }

    public decimal Fiyat { get; set; }

    public virtual Kitaplar Kitap { get; set; } = null!;

    public virtual Siparisler Siparis { get; set; } = null!;
}
