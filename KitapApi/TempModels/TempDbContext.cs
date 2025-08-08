using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace KitapApi.TempModels;

public partial class TempDbContext : DbContext
{
    public TempDbContext()
    {
    }

    public TempDbContext(DbContextOptions<TempDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Favoriler> Favorilers { get; set; }

    public virtual DbSet<Kategoriler> Kategorilers { get; set; }

    public virtual DbSet<Kitaplar> Kitaplars { get; set; }

    public virtual DbSet<Kullanicilar> Kullanicilars { get; set; }

    public virtual DbSet<SiparisDetaylari> SiparisDetaylaris { get; set; }

    public virtual DbSet<Siparisler> Siparislers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=KitapProjesiDb;Integrated Security=true;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Favoriler>(entity =>
        {
            entity.ToTable("Favoriler");

            entity.HasIndex(e => e.KitapId, "IX_Favoriler_KitapId");

            entity.HasIndex(e => e.KullaniciId, "IX_Favoriler_KullaniciId");

            entity.HasOne(d => d.Kitap).WithMany(p => p.Favorilers).HasForeignKey(d => d.KitapId);

            entity.HasOne(d => d.Kullanici).WithMany(p => p.Favorilers).HasForeignKey(d => d.KullaniciId);
        });

        modelBuilder.Entity<Kategoriler>(entity =>
        {
            entity.ToTable("Kategoriler");
        });

        modelBuilder.Entity<Kitaplar>(entity =>
        {
            entity.ToTable("Kitaplar");

            entity.HasIndex(e => e.KategoriId, "IX_Kitaplar_KategoriId");

            entity.Property(e => e.Fiyat).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Kategori).WithMany(p => p.Kitaplars).HasForeignKey(d => d.KategoriId);
        });

        modelBuilder.Entity<Kullanicilar>(entity =>
        {
            entity.ToTable("Kullanicilar");

            entity.Property(e => e.AdSoyad)
                .HasMaxLength(100)
                .HasDefaultValue("");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasDefaultValue("");
            entity.Property(e => e.Rol).HasMaxLength(50);
            entity.Property(e => e.SifreHash)
                .HasMaxLength(255)
                .HasDefaultValue("");
        });

        modelBuilder.Entity<SiparisDetaylari>(entity =>
        {
            entity.ToTable("SiparisDetaylari");

            entity.HasIndex(e => e.KitapId, "IX_SiparisDetaylari_KitapId");

            entity.HasIndex(e => e.SiparisId, "IX_SiparisDetaylari_SiparisId");

            entity.Property(e => e.Fiyat).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Kitap).WithMany(p => p.SiparisDetaylaris).HasForeignKey(d => d.KitapId);

            entity.HasOne(d => d.Siparis).WithMany(p => p.SiparisDetaylaris).HasForeignKey(d => d.SiparisId);
        });

        modelBuilder.Entity<Siparisler>(entity =>
        {
            entity.ToTable("Siparisler");

            entity.HasIndex(e => e.KullaniciId, "IX_Siparisler_KullaniciId");

            entity.Property(e => e.ToplamTutar).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Kullanici).WithMany(p => p.Siparislers).HasForeignKey(d => d.KullaniciId);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
