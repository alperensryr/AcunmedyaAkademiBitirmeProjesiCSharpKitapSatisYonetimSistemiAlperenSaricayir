using System;
using System.Collections.Generic;

namespace KitapApi.TempModels;

public partial class Siparisler
{
    public int Id { get; set; }

    public int KullaniciId { get; set; }

    public DateTime SiparisTarihi { get; set; }

    public decimal ToplamTutar { get; set; }

    public virtual Kullanicilar Kullanici { get; set; } = null!;

    public virtual ICollection<SiparisDetaylari> SiparisDetaylaris { get; set; } = new List<SiparisDetaylari>();
}
