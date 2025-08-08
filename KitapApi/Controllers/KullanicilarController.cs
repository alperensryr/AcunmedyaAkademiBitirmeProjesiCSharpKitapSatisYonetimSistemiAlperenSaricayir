using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KitapApi.Data;
using KitapApi.Entities;
using System.Security.Cryptography; // Şifre hash'leme için
using System.Text; // Encoding için
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel.DataAnnotations; // LoginModel için
using KitapApi.Attributes; // ApiAuthorize için
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace KitapApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KullanicilarController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public KullanicilarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Yardımcı Metot: Şifre Hash'leme
        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        // POST: api/Kullanicilar/Register
        // Yeni bir kullanıcı kaydı yapar
        [HttpPost("Register")]
        public async Task<ActionResult<Kullanici>> Register(Kullanici kullanici)
        {
            if (await _context.Kullanicilar.AnyAsync(u => u.Email == kullanici.Email))
            {
                return BadRequest("Bu email adresi zaten kullanılıyor.");
            }

            kullanici.SifreHash = HashPassword(kullanici.SifreHash); // Gelen şifreyi hash'le
            if (string.IsNullOrEmpty(kullanici.Rol)) // Eğer rol belirtilmediyse varsayılan "User" olsun
            {
                kullanici.Rol = "User";
            }
            kullanici.KayitTarihi = DateTime.Now;

            _context.Kullanicilar.Add(kullanici);
            await _context.SaveChangesAsync();

            // Şifre hash'ini güvenlik nedeniyle döndürme
            kullanici.SifreHash = "";
            return CreatedAtAction(nameof(GetKullanici), new { id = kullanici.Id }, kullanici);
        }

        // POST: api/Kullanicilar/Login
        // Kullanıcı girişi yapar ve geçerli ise kullanıcı nesnesini döner
        [HttpPost("Login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginModel loginModel)
        {
            try
            {
                var kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(u => u.Email == loginModel.Email);

                if (kullanici == null || !VerifyPassword(loginModel.Sifre, kullanici.SifreHash))
                {
                    return Unauthorized("Geçersiz e-posta veya şifre.");
                }

                // JWT Token üret
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = System.Text.Encoding.UTF8.GetBytes(StartupStatic.JwtKey); // Statik key
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, kullanici.Id.ToString()),
                    new Claim(ClaimTypes.Email, kullanici.Email),
                    new Claim(ClaimTypes.Role, kullanici.Rol)
                };
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(2),
                    Issuer = StartupStatic.JwtIssuer,
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwt = tokenHandler.WriteToken(token);

                // Şifre hash'ini güvenlik nedeniyle döndürme
                kullanici.SifreHash = "";
                return Ok(new { token = jwt, kullanici });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Login hatası: {ex.Message}");
            }
        }

        // GET: api/Kullanicilar
        // Tüm kullanıcıları listeler (Admin için)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Kullanici>>> GetKullanicilar()
        {
            return await _context.Kullanicilar.ToListAsync();
        }

        // GET: api/Kullanicilar/5
        // Belirli bir ID'ye sahip kullanıcıyı getirir
        [HttpGet("{id}")]
        public async Task<ActionResult<Kullanici>> GetKullanici(int id)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);

            if (kullanici == null)
            {
                return NotFound();
            }

            return kullanici;
        }

        // PUT: api/Kullanicilar/5
        // Belirli bir ID'ye sahip kullanıcıyı günceller
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateKullanici(int id, Kullanici kullanici)
        {
            if (id != kullanici.Id)
            {
                return BadRequest("Güncellenmek istenen kullanıcının ID'si ile URL'deki ID eşleşmiyor.");
            }

            // Eğer şifre güncelleniyorsa, yeni şifreyi hash'le
            if (!string.IsNullOrEmpty(kullanici.SifreHash))
            {
                kullanici.SifreHash = HashPassword(kullanici.SifreHash);
            }
            else // Şifre boş gelirse, mevcut şifre hash'ini koru
            {
                // Mevcut kullanıcıyı veritabanından çekip şifre hash'ini kopyalıyoruz
                _context.Entry(kullanici).Property(u => u.SifreHash).IsModified = false;
            }

            _context.Entry(kullanici).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KullaniciExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Kullanicilar/5
        // Belirli bir ID'ye sahip kullanıcıyı siler
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteKullanici(int id)
        {
            var kullanici = await _context.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                return NotFound();
            }

            _context.Kullanicilar.Remove(kullanici);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Kullanicilar/{id}/ChangePassword
        // Kullanıcının şifresini değiştirir
        [HttpPost("{id}/ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            try
            {
                var kullanici = await _context.Kullanicilar.FindAsync(id);
                if (kullanici == null)
                {
                    return NotFound("Kullanıcı bulunamadı.");
                }

                // Mevcut şifreyi doğrula
                if (!VerifyPassword(request.MevcutSifre, kullanici.SifreHash))
                {
                    return BadRequest("Mevcut şifre yanlış.");
                }

                // Yeni şifreyi hash'le ve kaydet
                kullanici.SifreHash = HashPassword(request.YeniSifre);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Şifre başarıyla değiştirildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Şifre değiştirme hatası: {ex.Message}");
            }
        }

        // PUT: api/Kullanicilar/{id}/UpdateWithPassword
        // Kullanıcı bilgilerini ve şifresini birlikte günceller
        [HttpPut("{id}/UpdateWithPassword")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateKullaniciWithPassword(int id, [FromBody] UpdateWithPasswordRequest request)
        {
            try
            {
                var kullanici = await _context.Kullanicilar.FindAsync(id);
                if (kullanici == null)
                {
                    return NotFound("Kullanıcı bulunamadı.");
                }

                // Kullanıcı bilgilerini güncelle
                kullanici.AdSoyad = request.AdSoyad;
                kullanici.Email = request.Email;
                kullanici.Rol = request.Rol;

                // Yeni şifreyi hash'le ve kaydet
                if (!string.IsNullOrWhiteSpace(request.YeniSifre))
                {
                    kullanici.SifreHash = HashPassword(request.YeniSifre);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Kullanıcı ve şifre başarıyla güncellendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Kullanıcı güncelleme hatası: {ex.Message}");
            }
        }

        private bool KullaniciExists(int id)
        {
            return _context.Kullanicilar.Any(e => e.Id == id);
        }
    }

    // Login işlemi için basit bir model (Bu sınıf genellikle aynı dosya içinde tanımlanır)
    public class LoginModel
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Sifre { get; set; } = string.Empty;
    }

    // Şifre değiştirme için model
    public class ChangePasswordRequest
    {
        [Required]
        public string MevcutSifre { get; set; } = string.Empty;
        [Required]
        public string YeniSifre { get; set; } = string.Empty;
    }

    // Kullanıcı bilgilerini ve şifresini birlikte güncelleme için model
    public class UpdateWithPasswordRequest
    {
        [Required]
        public string AdSoyad { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Rol { get; set; } = string.Empty;
        public string YeniSifre { get; set; } = string.Empty;
    }
}