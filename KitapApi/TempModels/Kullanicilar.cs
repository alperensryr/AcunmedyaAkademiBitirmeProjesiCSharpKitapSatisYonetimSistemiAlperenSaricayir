using System;
using System.Collections.Generic;

namespace KitapApi.TempModels;

public partial class Kullanicilar
{
    public int Id { get; set; }

    public string Rol { get; set; } = null!;

    public string AdSoyad { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime KayitTarihi { get; set; }

    public string SifreHash { get; set; } = null!;

    public virtual ICollection<Favoriler> Favorilers { get; set; } = new List<Favoriler>();

    public virtual ICollection<Siparisler> Siparislers { get; set; } = new List<Siparisler>();
}
