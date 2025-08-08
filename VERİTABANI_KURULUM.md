# Veritabanı Kurulum Rehberi

Bu rehber, diğer bilgisayarlarda aynı kitap ve kategori verilerini görmek için gerekli adımları açıklar.

## Sorun
Her bilgisayarda farklı LocalDB instance'ları kullanıldığı için veriler farklı görünüyor. Bu bilgisayarda 33 kitap varken, diğer bilgisayarlarda sadece 2 kitap görünüyor.

## Çözüm 1: Otomatik Veri Yükleme (Önerilen)

Projeye otomatik veri yükleme sistemi eklenmiştir. Aşağıdaki adımları takip edin:

### 1. Mevcut Veritabanını Sıfırlayın
```bash
# KitapApi klasöründe çalıştırın
dotnet ef database drop --force
dotnet ef database update
```

### 2. Uygulamayı Başlatın
```bash
# KitapApi klasöründe
dotnet run
```

Uygulama başladığında otomatik olarak:
- 8 kategori
- 33 kitap
- 1 admin kullanıcı (admin@kitap.com / admin123)

yüklenecektir.

## Çözüm 2: Manuel SQL Script Kullanma

Eğer otomatik yükleme çalışmazsa, `ExportData.sql` dosyasını kullanabilirsiniz:

1. SQL Server Management Studio veya Visual Studio'da LocalDB'ye bağlanın
2. `ExportData.sql` dosyasını çalıştırın
3. Çıkan INSERT komutlarını kopyalayın
4. Diğer bilgisayarlarda bu komutları çalıştırın

## Çözüm 3: Veritabanı Dosyasını Kopyalama

1. Bu bilgisayarda LocalDB veritabanı dosyasını bulun:
   ```
   C:\Users\[KullanıcıAdı]\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB
   ```

2. `KitapProjesiDb.mdf` ve `KitapProjesiDb.ldf` dosyalarını kopyalayın

3. Diğer bilgisayarlarda aynı konuma yapıştırın

## Doğrulama

Kurulum sonrası kontrol edin:
- Ana sayfada 33 kitap görünmeli
- Kategoriler sayfasında 8 kategori olmalı
- Admin paneline admin@kitap.com / admin123 ile giriş yapabilmelisiniz

## Notlar

- LocalDB her kullanıcı için ayrı instance oluşturur
- Proje ilk kez çalıştırıldığında otomatik veri yükleme aktif olur
- Eğer zaten veri varsa, tekrar veri yüklenmez
- Veritabanını tamamen sıfırlamak için `dotnet ef database drop --force` komutunu kullanın

## Sorun Giderme

Eğer hala sorun yaşıyorsanız:

1. Veritabanı bağlantısını kontrol edin
2. Migration'ları yeniden çalıştırın: `dotnet ef database update`
3. Uygulama loglarını kontrol edin
4. DataSeeder.cs dosyasının doğru çalıştığından emin olun

## İletişim

Sorun devam ederse, lütfen hata mesajlarını ve log dosyalarını paylaşın.