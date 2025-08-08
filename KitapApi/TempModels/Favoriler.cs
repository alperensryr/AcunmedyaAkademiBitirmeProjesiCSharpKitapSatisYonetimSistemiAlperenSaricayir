using System;
using System.Collections.Generic;

namespace KitapApi.TempModels;

public partial class Favoriler
{
    public int Id { get; set; }

    public int KullaniciId { get; set; }

    public int KitapId { get; set; }

    public virtual Kitaplar Kitap { get; set; } = null!;

    public virtual Kullanicilar Kullanici { get; set; } = null!;
}
