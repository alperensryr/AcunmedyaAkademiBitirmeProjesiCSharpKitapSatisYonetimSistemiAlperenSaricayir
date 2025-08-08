-- Bu script mevcut veritabanındaki tüm verileri export eder
-- Diğer bilgisayarlarda bu verileri import edebilirsiniz

USE KitapProjesiDb;
GO

-- Kategoriler tablosundaki verileri export et
SELECT 'INSERT INTO Kategoriler (Ad, Aciklama) VALUES (' + 
       QUOTENAME(Ad, '''') + ', ' + 
       ISNULL(QUOTENAME(Aciklama, ''''), 'NULL') + ');'
FROM Kategoriler
ORDER BY Id;

PRINT '-- Kategori verileri yukarıda';
PRINT '';

-- Kitaplar tablosundaki verileri export et
SELECT 'INSERT INTO Kitaplar (Ad, Yazar, Fiyat, Aciklama, ResimUrl, KategoriId) VALUES (' + 
       QUOTENAME(Ad, '''') + ', ' + 
       QUOTENAME(Yazar, '''') + ', ' + 
       CAST(Fiyat AS VARCHAR(20)) + ', ' + 
       ISNULL(QUOTENAME(Aciklama, ''''), 'NULL') + ', ' + 
       ISNULL(QUOTENAME(ResimUrl, ''''), 'NULL') + ', ' + 
       CAST(KategoriId AS VARCHAR(10)) + ');'
FROM Kitaplar
ORDER BY Id;

PRINT '-- Kitap verileri yukarıda';
PRINT '';

-- Kullanıcılar tablosundaki verileri export et (şifreler hariç)
SELECT 'INSERT INTO Kullanicilar (AdSoyad, Email, SifreHash, Rol, KayitTarihi) VALUES (' + 
       QUOTENAME(AdSoyad, '''') + ', ' + 
       QUOTENAME(Email, '''') + ', ' + 
       QUOTENAME(SifreHash, '''') + ', ' + 
       QUOTENAME(Rol, '''') + ', ' + 
       QUOTENAME(CONVERT(VARCHAR, KayitTarihi, 120), '''') + ');'
FROM Kullanicilar
ORDER BY Id;

PRINT '-- Kullanıcı verileri yukarıda';