using System;
using System.Collections.Generic;

namespace KitapApi.TempModels;

public partial class Kategoriler
{
    public int Id { get; set; }

    public string Ad { get; set; } = null!;

    public string? Aciklama { get; set; }

    public virtual ICollection<Kitaplar> Kitaplars { get; set; } = new List<Kitaplar>();
}
