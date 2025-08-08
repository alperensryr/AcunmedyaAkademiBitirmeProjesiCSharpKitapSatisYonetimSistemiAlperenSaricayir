using System.Text.Json;
using KitapMVC.Models.Entities;
using KitapMVC.Models;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace KitapMVC.Services
{
    public class KitapApiService : IKullaniciApiService, IRaporApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public KitapApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetAuthorizationHeader()
        {
            // Önce session'dan token almaya çalış (admin girişi için)
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
            
            // Eğer session'da yoksa cookie'den al (normal kullanıcı girişi için)
            if (string.IsNullOrEmpty(token))
            {
                token = _httpContextAccessor.HttpContext?.Request.Cookies["AuthToken"];
            }
            
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                Console.WriteLine($"Authorization header set with token: {token.Substring(0, Math.Min(20, token.Length))}...");
            }
            else
            {
                Console.WriteLine("No JWT token found in session or cookies");
            }
        }

        // KATEGORI METOTLARI
        public async Task<List<Kategori>> GetKategorilerAsync()
        {
            try
            {
                var response = _httpClient.BaseAddress != null 
                    ? await _httpClient.GetAsync("api/Kategoriler")
                    : await _httpClient.GetAsync("http://localhost:7011/api/Kategoriler");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var kategoriler = JsonSerializer.Deserialize<List<Kategori>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return kategoriler ?? new List<Kategori>();
                }
                else
                {
                    Console.WriteLine($"API Hatası: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<Kategori>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kategori getirme hatası: {ex.Message}");
                return new List<Kategori>();
            }
        }

        // KITAP METOTLARI
        public async Task<List<Kitap>> GetKitaplarAsync()
        {
            try
            {
                var response = _httpClient.BaseAddress != null 
                    ? await _httpClient.GetAsync("api/Kitaplar")
                    : await _httpClient.GetAsync("http://localhost:7011/api/Kitaplar");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var kitaplar = JsonSerializer.Deserialize<List<Kitap>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return kitaplar ?? new List<Kitap>();
                }
                else
                {
                    Console.WriteLine($"API Hatası: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<Kitap>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kitap getirme hatası: {ex.Message}");
                return new List<Kitap>();
            }
        }

        public async Task<Kitap?> GetKitapByIdAsync(int id)
        {
            try
            {
                var response = _httpClient.BaseAddress != null 
                    ? await _httpClient.GetAsync($"api/Kitaplar/{id}")
                    : await _httpClient.GetAsync($"http://localhost:7011/api/Kitaplar/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var kitap = JsonSerializer.Deserialize<Kitap>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return kitap;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kitap getirme hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateKitapAsync(Kitap kitap)
        {
            try
            {
                SetAuthorizationHeader();
                
                if (kitap == null)
                {
                    Console.WriteLine("UpdateKitapAsync: Kitap nesnesi null");
                    throw new ArgumentNullException(nameof(kitap), "Kitap nesnesi null olamaz");
                }
                
                if (kitap.Id <= 0)
                {
                    Console.WriteLine($"UpdateKitapAsync: Geçersiz kitap ID: {kitap.Id}");
                    throw new ArgumentException("Geçersiz kitap ID", nameof(kitap));
                }
                
                if (string.IsNullOrWhiteSpace(kitap.Ad))
                {
                    Console.WriteLine("UpdateKitapAsync: Kitap adı boş");
                    throw new ArgumentException("Kitap adı boş olamaz", nameof(kitap));
                }
                
                if (kitap.KategoriId <= 0)
                {
                    Console.WriteLine($"UpdateKitapAsync: Geçersiz kategori ID: {kitap.KategoriId}");
                    throw new ArgumentException("Geçersiz kategori ID", nameof(kitap));
                }
                
                var jsonContent = JsonSerializer.Serialize(kitap, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                Console.WriteLine($"Kitap güncelleme isteği gönderiliyor - ID: {kitap.Id}, Ad: {kitap.Ad}");
                Console.WriteLine($"JSON Content: {jsonContent}");

                var response = _httpClient.BaseAddress != null 
                    ? await _httpClient.PutAsync($"api/Kitaplar/{kitap.Id}", content)
                    : await _httpClient.PutAsync($"http://localhost:7011/api/Kitaplar/{kitap.Id}", content);
                
                Console.WriteLine($"API Response Status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Kitap başarıyla güncellendi");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Kitap güncelleme hatası: {response.StatusCode} - {errorContent}");
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException($"BadRequest: {errorContent}");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        throw new HttpRequestException($"NotFound: Kitap bulunamadı");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new HttpRequestException($"Unauthorized: Yetkiniz bulunmuyor");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        throw new HttpRequestException($"Forbidden: Erişim reddedildi");
                    }
                    else
                    {
                        throw new HttpRequestException($"API Hatası: {response.StatusCode} - {errorContent}");
                    }
                }
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kitap güncelleme genel hatası: {ex.Message}");
                throw new Exception($"Kitap güncellenirken beklenmeyen bir hata oluştu: {ex.Message}", ex);
            }
        }

        public async Task<string?> UploadKitapImageAsync(int kitapId, IFormFile file)
        {
            SetAuthorizationHeader();
            
            if (file == null || file.Length == 0)
                return null;
            using var content = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            content.Add(new StreamContent(stream), "file", file.FileName);
            var response = await _httpClient.PostAsync($"api/Kitaplar/upload?kitapId={kitapId}", content);
            if (!response.IsSuccessStatusCode)
                return null;
            var responseContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseContent);
            if (doc.RootElement.TryGetProperty("resimUrl", out var urlProp))
                return urlProp.GetString();
            return null;
        }

        public async Task<Kitap?> CreateKitapAsync(Kitap kitap)
        {
            try
            {
                SetAuthorizationHeader();
                
                Console.WriteLine($"CreateKitapAsync çağrıldı - BaseAddress: {_httpClient.BaseAddress}");
                
                var jsonContent = JsonSerializer.Serialize(kitap, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                Console.WriteLine($"JSON Content: {jsonContent}");

                var url = _httpClient.BaseAddress != null 
                    ? "api/Kitaplar"
                    : "http://localhost:7011/api/Kitaplar";
                    
                Console.WriteLine($"Request URL: {url}");
                
                var response = _httpClient.BaseAddress != null 
                    ? await _httpClient.PostAsync("api/Kitaplar", content)
                    : await _httpClient.PostAsync("http://localhost:7011/api/Kitaplar", content);

                Console.WriteLine($"API Response Status: {response.StatusCode}");
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Kitap oluşturma hatası: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Content: {responseContent}");
                var yeniKitap = JsonSerializer.Deserialize<Kitap>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return yeniKitap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateKitapAsync genel hatası: {ex.Message}");
                throw new Exception($"Kitap oluşturulurken beklenmeyen bir hata oluştu: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteKitapAsync(int id)
        {
            SetAuthorizationHeader();
            
            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.DeleteAsync($"api/Kitaplar/{id}")
                : await _httpClient.DeleteAsync($"http://localhost:7011/api/Kitaplar/{id}");
            
            return response.IsSuccessStatusCode;
        }

        public async Task<List<Kitap>> GetKitaplarByKategoriAsync(int kategoriId)
        {
            try
            {
                var response = _httpClient.BaseAddress != null 
                    ? await _httpClient.GetAsync($"api/Kitaplar/kategori/{kategoriId}")
                    : await _httpClient.GetAsync($"http://localhost:7011/api/Kitaplar/kategori/{kategoriId}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var kitaplar = JsonSerializer.Deserialize<List<Kitap>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return kitaplar ?? new List<Kitap>();
                }
                else
                {
                    Console.WriteLine($"API Hatası: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<Kitap>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kategoriye göre kitap getirme hatası: {ex.Message}");
                return new List<Kitap>();
            }
        }

        // KULLANICI METOTLARI
        public async Task<(Kullanici? kullanici, string? token)> LoginAsync(LoginViewModel loginModel)
        {
            var jsonContent = JsonSerializer.Serialize(loginModel, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.PostAsync("api/Kullanicilar/Login", content)
                : await _httpClient.PostAsync("http://localhost:7011/api/Kullanicilar/Login", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Login hatası: {response.StatusCode} - {errorContent}");
                return (null, null);
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseContent);
            
            Kullanici? kullanici = null;
            string? token = null;
            
            if (doc.RootElement.TryGetProperty("kullanici", out var kullaniciElement))
            {
                kullanici = JsonSerializer.Deserialize<Kullanici>(kullaniciElement.GetRawText(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            
            if (doc.RootElement.TryGetProperty("token", out var tokenElement))
            {
                token = tokenElement.GetString();
            }
            
            return (kullanici, token);
        }

        public async Task<Kullanici?> RegisterAsync(Kullanici kullanici)
        {
            var jsonContent = JsonSerializer.Serialize(kullanici, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.PostAsync("api/Kullanicilar/Register", content)
                : await _httpClient.PostAsync("http://localhost:7011/api/Kullanicilar/Register", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Kayıt hatası: {response.StatusCode} - {errorContent}");
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var yeniKullanici = JsonSerializer.Deserialize<Kullanici>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return yeniKullanici;
        }

        public async Task<Kullanici?> CreateKullaniciAsync(Kullanici kullanici, string sifre)
        {
            var registerModel = new
            {
                AdSoyad = kullanici.AdSoyad,
                Email = kullanici.Email,
                SifreHash = sifre,
                Rol = kullanici.Rol
            };

            var jsonContent = JsonSerializer.Serialize(registerModel, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.PostAsync("api/Kullanicilar/Register", content)
                : await _httpClient.PostAsync("http://localhost:7011/api/Kullanicilar/Register", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Kullanıcı oluşturma hatası: {response.StatusCode} - {errorContent}");
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var yeniKullanici = JsonSerializer.Deserialize<Kullanici>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return yeniKullanici;
        }

        public async Task<List<Kullanici>> GetKullanicilarAsync()
        {
            try
            {
                Console.WriteLine($"GetKullanicilarAsync başladı - BaseAddress: {_httpClient.BaseAddress}");
                
                var response = _httpClient.BaseAddress != null 
                    ? await _httpClient.GetAsync("api/Kullanicilar")
                    : await _httpClient.GetAsync("http://localhost:7011/api/Kullanicilar");
                
                Console.WriteLine($"API Response Status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"API Response Content Length: {content.Length}");
                    
                    var kullanicilar = JsonSerializer.Deserialize<List<Kullanici>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    Console.WriteLine($"Deserialize edilen kullanıcı sayısı: {kullanicilar?.Count ?? 0}");
                    
                    if (kullanicilar != null && kullanicilar.Any())
                    {
                        foreach (var k in kullanicilar)
                        {
                            k.SifreHash = "";
                        }
                    }
                    
                    return kullanicilar ?? new List<Kullanici>();
                }
                else
                {
                    Console.WriteLine($"API Hatası: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<Kullanici>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kullanıcı getirme hatası: {ex.Message}");
                return new List<Kullanici>();
            }
        }

        public async Task<Kullanici?> GetKullaniciByIdAsync(int id)
        {
            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.GetAsync($"api/Kullanicilar/{id}")
                : await _httpClient.GetAsync($"http://localhost:7011/api/Kullanicilar/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var content = await response.Content.ReadAsStringAsync();
            var kullanici = JsonSerializer.Deserialize<Kullanici>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return kullanici;
        }

        public async Task<bool> UpdateKullaniciAsync(Kullanici kullanici)
        {
            try
            {
                SetAuthorizationHeader();
                
                if (kullanici == null)
                {
                    Console.WriteLine("UpdateKullaniciAsync: Kullanıcı nesnesi null");
                    throw new ArgumentNullException(nameof(kullanici), "Kullanıcı nesnesi null olamaz");
                }
                
                if (kullanici.Id <= 0)
                {
                    Console.WriteLine($"UpdateKullaniciAsync: Geçersiz kullanıcı ID: {kullanici.Id}");
                    throw new ArgumentException("Geçersiz kullanıcı ID", nameof(kullanici));
                }
                
                if (string.IsNullOrWhiteSpace(kullanici.AdSoyad))
                {
                    Console.WriteLine("UpdateKullaniciAsync: Kullanıcı adı boş");
                    throw new ArgumentException("Kullanıcı adı boş olamaz", nameof(kullanici));
                }
                
                if (string.IsNullOrWhiteSpace(kullanici.Email))
                {
                    Console.WriteLine("UpdateKullaniciAsync: Email boş");
                    throw new ArgumentException("Email boş olamaz", nameof(kullanici));
                }
                
                var jsonContent = JsonSerializer.Serialize(kullanici, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                Console.WriteLine($"Kullanıcı güncelleme isteği gönderiliyor - ID: {kullanici.Id}, AdSoyad: {kullanici.AdSoyad}, Email: {kullanici.Email}");
                Console.WriteLine($"JSON Content: {jsonContent}");

                var url = _httpClient.BaseAddress != null 
                    ? $"api/Kullanicilar/{kullanici.Id}"
                    : $"http://localhost:7011/api/Kullanicilar/{kullanici.Id}";
                    
                Console.WriteLine($"Request URL: {url}");
                
                var response = _httpClient.BaseAddress != null 
                    ? await _httpClient.PutAsync($"api/Kullanicilar/{kullanici.Id}", content)
                    : await _httpClient.PutAsync($"http://localhost:7011/api/Kullanicilar/{kullanici.Id}", content);
                
                Console.WriteLine($"API Response Status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Kullanıcı başarıyla güncellendi");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Kullanıcı güncelleme hatası: {response.StatusCode} - {errorContent}");
                    
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        throw new HttpRequestException($"BadRequest: {errorContent}");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        throw new HttpRequestException($"NotFound: Kullanıcı bulunamadı");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        throw new HttpRequestException($"Unauthorized: Yetkiniz bulunmuyor");
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        throw new HttpRequestException($"Forbidden: Erişim reddedildi");
                    }
                    else
                    {
                        throw new HttpRequestException($"API Hatası: {response.StatusCode} - {errorContent}");
                    }
                }
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kullanıcı güncelleme genel hatası: {ex.Message}");
                throw new Exception($"Kullanıcı güncellenirken beklenmeyen bir hata oluştu: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteKullaniciAsync(int id)
        {
            SetAuthorizationHeader();
            
            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.DeleteAsync($"api/Kullanicilar/{id}")
                : await _httpClient.DeleteAsync($"http://localhost:7011/api/Kullanicilar/{id}");
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ChangePasswordAsync(int kullaniciId, ChangePasswordViewModel model)
        {
            SetAuthorizationHeader();
            
            var changePasswordRequest = new
            {
                MevcutSifre = model.MevcutSifre,
                YeniSifre = model.YeniSifre
            };
            
            var jsonContent = JsonSerializer.Serialize(changePasswordRequest, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.PostAsync($"api/Kullanicilar/{kullaniciId}/ChangePassword", content)
                : await _httpClient.PostAsync($"http://localhost:7011/api/Kullanicilar/{kullaniciId}/ChangePassword", content);
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateKullaniciWithPasswordAsync(Kullanici kullanici, string yeniSifre)
        {
            try
            {
                SetAuthorizationHeader();
                
                if (kullanici == null)
                {
                    Console.WriteLine("UpdateKullaniciWithPasswordAsync: Kullanıcı nesnesi null");
                    throw new ArgumentNullException(nameof(kullanici), "Kullanıcı nesnesi null olamaz");
                }
                
                if (kullanici.Id <= 0)
                {
                    Console.WriteLine($"UpdateKullaniciWithPasswordAsync: Geçersiz kullanıcı ID: {kullanici.Id}");
                    throw new ArgumentException("Geçersiz kullanıcı ID", nameof(kullanici));
                }
                
                if (string.IsNullOrWhiteSpace(yeniSifre))
                {
                    Console.WriteLine("UpdateKullaniciWithPasswordAsync: Yeni şifre boş");
                    throw new ArgumentException("Yeni şifre boş olamaz", nameof(yeniSifre));
                }
                
                var updateRequest = new
                {
                    Id = kullanici.Id,
                    AdSoyad = kullanici.AdSoyad,
                    Email = kullanici.Email,
                    Rol = kullanici.Rol,
                    YeniSifre = yeniSifre
                };
                
                var jsonContent = JsonSerializer.Serialize(updateRequest, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                Console.WriteLine($"Kullanıcı şifreli güncelleme isteği gönderiliyor - ID: {kullanici.Id}, AdSoyad: {kullanici.AdSoyad}, Email: {kullanici.Email}");
                Console.WriteLine($"JSON Content: {jsonContent}");

                var url = _httpClient.BaseAddress != null 
                    ? $"api/Kullanicilar/{kullanici.Id}/UpdateWithPassword"
                    : $"http://localhost:7011/api/Kullanicilar/{kullanici.Id}/UpdateWithPassword";
                    
                Console.WriteLine($"Request URL: {url}");
                
                var response = _httpClient.BaseAddress != null 
                    ? await _httpClient.PutAsync($"api/Kullanicilar/{kullanici.Id}/UpdateWithPassword", content)
                    : await _httpClient.PutAsync($"http://localhost:7011/api/Kullanicilar/{kullanici.Id}/UpdateWithPassword", content);
                
                Console.WriteLine($"API Response Status: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Kullanıcı şifreli güncelleme başarılı");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Kullanıcı şifreli güncelleme hatası: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateKullaniciWithPasswordAsync Exception: {ex.Message}");
                throw;
            }
        }

        // FAVORİ METOTLARI
        public async Task<List<Favori>> GetFavorilerAsync()
        {
            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.GetAsync("api/Favori")
                : await _httpClient.GetAsync("http://localhost:7011/api/Favori");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Favori API Response: {content}");
            var favoriler = JsonSerializer.Deserialize<List<Favori>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            Console.WriteLine($"Deserialize edilen favori sayısı: {favoriler?.Count ?? 0}");
            return favoriler ?? new List<Favori>();
        }

        public async Task<Favori?> GetFavoriByIdAsync(int id)
        {
            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.GetAsync($"api/Favori/{id}")
                : await _httpClient.GetAsync($"http://localhost:7011/api/Favori/{id}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }
            var content = await response.Content.ReadAsStringAsync();
            var favori = JsonSerializer.Deserialize<Favori>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return favori;
        }

        public async Task<bool> DeleteFavoriAsync(int id)
        {
            SetAuthorizationHeader();
            
            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.DeleteAsync($"api/Favori/{id}")
                : await _httpClient.DeleteAsync($"http://localhost:7011/api/Favori/{id}");
            return response.IsSuccessStatusCode;
        }

        // SİPARİŞ METOTLARI
        public async Task<List<Siparis>> GetSiparislerAsync()
        {
            try
            {
                var response = _httpClient.BaseAddress != null 
                    ? await _httpClient.GetAsync("api/Siparis")
                    : await _httpClient.GetAsync("http://localhost:7011/api/Siparis");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var siparisler = JsonSerializer.Deserialize<List<Siparis>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return siparisler ?? new List<Siparis>();
                }
                else
                {
                    Console.WriteLine($"API Hatası: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<Siparis>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sipariş getirme hatası: {ex.Message}");
                return new List<Siparis>();
            }
        }

        public async Task<Siparis?> GetSiparisByIdAsync(int id)
        {
            try
            {
                var response = _httpClient.BaseAddress != null 
                    ? await _httpClient.GetAsync($"api/Siparis/{id}")
                    : await _httpClient.GetAsync($"http://localhost:7011/api/Siparis/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var siparis = JsonSerializer.Deserialize<Siparis>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return siparis;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Sipariş getirme hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateSiparisDurumAsync(int id, string durum)
        {
            SetAuthorizationHeader();
            
            var updateRequest = new { Durum = durum };
            var jsonContent = JsonSerializer.Serialize(updateRequest, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.PutAsync($"api/Siparis/{id}/durum", content)
                : await _httpClient.PutAsync($"http://localhost:7011/api/Siparis/{id}/durum", content);
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteSiparisAsync(int id)
        {
            SetAuthorizationHeader();
            
            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.DeleteAsync($"api/Siparis/{id}")
                : await _httpClient.DeleteAsync($"http://localhost:7011/api/Siparis/{id}");
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> SiparisOnaylaAsync(int id)
        {
            SetAuthorizationHeader();
            
            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.PutAsync($"api/Siparis/Onayla/{id}", null)
                : await _httpClient.PutAsync($"http://localhost:7011/api/Siparis/Onayla/{id}", null);
            
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CreateSiparisAsync(Siparis siparis)
        {
            SetAuthorizationHeader();
            
            var jsonContent = JsonSerializer.Serialize(siparis, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.PostAsync("api/Siparis", content)
                : await _httpClient.PostAsync("http://localhost:7011/api/Siparis", content);

            return response.IsSuccessStatusCode;
        }

        // KATEGORİ METOTLARI (Eksik olanlar)
        public async Task<Kategori?> GetKategoriByIdAsync(int id)
        {
            try
            {
                var response = _httpClient.BaseAddress != null 
                    ? await _httpClient.GetAsync($"api/Kategoriler/{id}")
                    : await _httpClient.GetAsync($"http://localhost:7011/api/Kategoriler/{id}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var kategori = JsonSerializer.Deserialize<Kategori>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return kategori;
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kategori getirme hatası: {ex.Message}");
                return null;
            }
        }

        public async Task<Kategori?> CreateKategoriAsync(Kategori kategori)
        {
            SetAuthorizationHeader();
            
            var jsonContent = JsonSerializer.Serialize(kategori, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.PostAsync("api/Kategoriler", content)
                : await _httpClient.PostAsync("http://localhost:7011/api/Kategoriler", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Kategori oluşturma hatası: {response.StatusCode} - {errorContent}");
                return null;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var yeniKategori = JsonSerializer.Deserialize<Kategori>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return yeniKategori;
        }

        public async Task<bool> UpdateKategoriAsync(Kategori kategori)
        {
            SetAuthorizationHeader();
            
            var jsonContent = JsonSerializer.Serialize(kategori, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.PutAsync($"api/Kategoriler/{kategori.Id}", content)
                : await _httpClient.PutAsync($"http://localhost:7011/api/Kategoriler/{kategori.Id}", content);
            
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool success, string? errorMessage)> DeleteKategoriAsync(int id)
        {
            SetAuthorizationHeader();
            
            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.DeleteAsync($"api/Kategoriler/{id}")
                : await _httpClient.DeleteAsync($"http://localhost:7011/api/Kategoriler/{id}");
            
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, errorContent);
            }
        }

        // FAVORİ METOTLARI (Eksik olan)
        public async Task<bool> CreateFavoriAsync(Favori favori)
        {
            SetAuthorizationHeader();
            
            Console.WriteLine($"CreateFavoriAsync çağrıldı - KullaniciId: {favori.KullaniciId}, KitapId: {favori.KitapId}");
            Console.WriteLine($"BaseAddress: {_httpClient.BaseAddress}");
            
            var jsonContent = JsonSerializer.Serialize(favori, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            Console.WriteLine($"JSON Content: {jsonContent}");

            var response = _httpClient.BaseAddress != null 
                ? await _httpClient.PostAsync("api/Favori", content)
                : await _httpClient.PostAsync("http://localhost:7011/api/Favori", content);

            Console.WriteLine($"API Response Status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Favori başarıyla oluşturuldu");
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Favori oluşturma hatası: {response.StatusCode} - {errorContent}");
                return false;
            }
        }
    }
}