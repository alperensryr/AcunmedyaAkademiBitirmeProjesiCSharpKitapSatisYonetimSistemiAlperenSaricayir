using KitapApi.Entities;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace KitapApi.Data
{
    public static class DataSeeder
    {
        public static async Task SeedDataAsync(ApplicationDbContext context)
        {
            // Veritabanının oluşturulduğundan emin ol
            await context.Database.EnsureCreatedAsync();

            // Eğer zaten veri varsa, seeding yapma
            if (await context.Kategoriler.AnyAsync())
            {
                return; // Veri zaten var
            }

            // Kategoriler ekle
            var kategoriler = new List<Kategori>
            {
                new Kategori { Ad = "Roman", Aciklama = "Roman kategorisi" },
                new Kategori { Ad = "Bilim Kurgu", Aciklama = "Bilim kurgu kitapları" },
                new Kategori { Ad = "Tarih", Aciklama = "Tarih kitapları" },
                new Kategori { Ad = "Felsefe", Aciklama = "Felsefe kitapları" },
                new Kategori { Ad = "Çocuk", Aciklama = "Çocuk kitapları" },
                new Kategori { Ad = "Edebiyat", Aciklama = "Edebiyat kitapları" },
                new Kategori { Ad = "Bilim", Aciklama = "Bilim kitapları" },
                new Kategori { Ad = "Sanat", Aciklama = "Sanat kitapları" }
            };

            context.Kategoriler.AddRange(kategoriler);
            await context.SaveChangesAsync();

            // Kitaplar ekle
            var kitaplar = new List<Kitap>
            {
                new Kitap { Ad = "Suç ve Ceza", Yazar = "Fyodor Dostoyevski", Fiyat = 25.50m, Aciklama = "Klasik Rus edebiyatının başyapıtı", KategoriId = 1, ResimUrl = "/images/suc_ve_ceza.svg" },
                new Kitap { Ad = "Bilinçaltının Gücü", Yazar = "Joseph Murphy", Fiyat = 30.00m, Aciklama = "Kişisel gelişim kitabı", KategoriId = 4, ResimUrl = "/images/bilincaltinin_gucu.svg" },
                new Kitap { Ad = "Bilinmeyen Bir Kadının Mektubu", Yazar = "Stefan Zweig", Fiyat = 15.75m, Aciklama = "Duygusal bir aşk hikayesi", KategoriId = 1, ResimUrl = null },
                new Kitap { Ad = "1984", Yazar = "George Orwell", Fiyat = 22.00m, Aciklama = "Distopik roman", KategoriId = 2, ResimUrl = null },
                new Kitap { Ad = "Hayvan Çiftliği", Yazar = "George Orwell", Fiyat = 18.50m, Aciklama = "Alegorik roman", KategoriId = 1, ResimUrl = null },
                new Kitap { Ad = "Dune", Yazar = "Frank Herbert", Fiyat = 35.00m, Aciklama = "Bilim kurgu klasiği", KategoriId = 2, ResimUrl = null },
                new Kitap { Ad = "Sapiens", Yazar = "Yuval Noah Harari", Fiyat = 28.75m, Aciklama = "İnsanlık tarihi", KategoriId = 3, ResimUrl = null },
                new Kitap { Ad = "Homo Deus", Yazar = "Yuval Noah Harari", Fiyat = 32.00m, Aciklama = "Geleceğin tarihi", KategoriId = 3, ResimUrl = null },
                new Kitap { Ad = "Küçük Prens", Yazar = "Antoine de Saint-Exupéry", Fiyat = 12.50m, Aciklama = "Çocuklar için klasik", KategoriId = 5, ResimUrl = null },
                new Kitap { Ad = "Simyacı", Yazar = "Paulo Coelho", Fiyat = 20.00m, Aciklama = "Felsefi roman", KategoriId = 4, ResimUrl = null },
                new Kitap { Ad = "Kürk Mantolu Madonna", Yazar = "Sabahattin Ali", Fiyat = 16.25m, Aciklama = "Türk edebiyatı klasiği", KategoriId = 6, ResimUrl = null },
                new Kitap { Ad = "Harry Potter ve Felsefe Taşı", Yazar = "J.K. Rowling", Fiyat = 24.00m, Aciklama = "Fantastik roman", KategoriId = 5, ResimUrl = null },
                new Kitap { Ad = "Yüzüklerin Efendisi", Yazar = "J.R.R. Tolkien", Fiyat = 45.00m, Aciklama = "Fantastik epik", KategoriId = 2, ResimUrl = null },
                new Kitap { Ad = "Vadideki Zambak", Yazar = "Honoré de Balzac", Fiyat = 19.50m, Aciklama = "Fransız edebiyatı", KategoriId = 6, ResimUrl = null },
                new Kitap { Ad = "Beyaz Geceler", Yazar = "Fyodor Dostoyevski", Fiyat = 14.75m, Aciklama = "Romantik hikaye", KategoriId = 1, ResimUrl = null },
                new Kitap { Ad = "Çalıkuşu", Yazar = "Reşat Nuri Güntekin", Fiyat = 17.00m, Aciklama = "Türk edebiyatı klasiği", KategoriId = 6, ResimUrl = null },
                new Kitap { Ad = "Aşk-ı Memnu", Yazar = "Halit Ziya Uşaklıgil", Fiyat = 21.50m, Aciklama = "Türk edebiyatı", KategoriId = 6, ResimUrl = null },
                new Kitap { Ad = "Tutunamayanlar", Yazar = "Oğuz Atay", Fiyat = 26.00m, Aciklama = "Modern Türk edebiyatı", KategoriId = 6, ResimUrl = null },
                new Kitap { Ad = "Kırmızı Pazartesi", Yazar = "Gabriel García Márquez", Fiyat = 23.25m, Aciklama = "Latin Amerika edebiyatı", KategoriId = 1, ResimUrl = null },
                new Kitap { Ad = "Yüz Yıllık Yalnızlık", Yazar = "Gabriel García Márquez", Fiyat = 29.00m, Aciklama = "Büyülü gerçekçilik", KategoriId = 1, ResimUrl = null },
                new Kitap { Ad = "Fahrenheit 451", Yazar = "Ray Bradbury", Fiyat = 20.75m, Aciklama = "Distopik bilim kurgu", KategoriId = 2, ResimUrl = null },
                new Kitap { Ad = "Brave New World", Yazar = "Aldous Huxley", Fiyat = 22.50m, Aciklama = "Distopik roman", KategoriId = 2, ResimUrl = null },
                new Kitap { Ad = "Zamanın Kısa Tarihi", Yazar = "Stephen Hawking", Fiyat = 31.00m, Aciklama = "Fizik ve kozmoloji", KategoriId = 7, ResimUrl = null },
                new Kitap { Ad = "Sanat Tarihi", Yazar = "Ernst Gombrich", Fiyat = 42.00m, Aciklama = "Sanat tarihi rehberi", KategoriId = 8, ResimUrl = null },
                new Kitap { Ad = "Meditations", Yazar = "Marcus Aurelius", Fiyat = 18.00m, Aciklama = "Stoik felsefe", KategoriId = 4, ResimUrl = null },
                new Kitap { Ad = "Nicomachean Ethics", Yazar = "Aristotle", Fiyat = 25.00m, Aciklama = "Etik felsefesi", KategoriId = 4, ResimUrl = null },
                new Kitap { Ad = "The Republic", Yazar = "Plato", Fiyat = 27.50m, Aciklama = "Politik felsefe", KategoriId = 4, ResimUrl = null },
                new Kitap { Ad = "İnsan Hakları Evrensel Beyannamesi", Yazar = "Birleşmiş Milletler", Fiyat = 8.00m, Aciklama = "İnsan hakları metni", KategoriId = 3, ResimUrl = null },
                new Kitap { Ad = "Osmanlı Tarihi", Yazar = "İlber Ortaylı", Fiyat = 35.50m, Aciklama = "Osmanlı İmparatorluğu tarihi", KategoriId = 3, ResimUrl = null },
                new Kitap { Ad = "Nutuk", Yazar = "Mustafa Kemal Atatürk", Fiyat = 15.00m, Aciklama = "Kurtuluş Savaşı anıları", KategoriId = 3, ResimUrl = null },
                new Kitap { Ad = "Alice Harikalar Diyarında", Yazar = "Lewis Carroll", Fiyat = 13.75m, Aciklama = "Çocuk klasiği", KategoriId = 5, ResimUrl = null },
                new Kitap { Ad = "Pinokyo", Yazar = "Carlo Collodi", Fiyat = 11.50m, Aciklama = "Çocuk masalı", KategoriId = 5, ResimUrl = null },
                new Kitap { Ad = "Heidi", Yazar = "Johanna Spyri", Fiyat = 14.00m, Aciklama = "Çocuk romanı", KategoriId = 5, ResimUrl = null }
            };

            context.Kitaplar.AddRange(kitaplar);
            await context.SaveChangesAsync();

            // Admin kullanıcı ekle
            var adminUser = new Kullanici
            {
                AdSoyad = "Admin User",
                Email = "admin@kitap.com",
                SifreHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Rol = "Admin",
                KayitTarihi = DateTime.Now
            };

            // Test kullanıcısı ekle
            var testUser = new Kullanici
            {
                AdSoyad = "Test Kullanıcı",
                Email = "test@kitap.com",
                SifreHash = BCrypt.Net.BCrypt.HashPassword("test123"),
                Rol = "Kullanici",
                KayitTarihi = DateTime.Now
            };

            context.Kullanicilar.AddRange(new[] { adminUser, testUser });
            await context.SaveChangesAsync();
        }
    }
}