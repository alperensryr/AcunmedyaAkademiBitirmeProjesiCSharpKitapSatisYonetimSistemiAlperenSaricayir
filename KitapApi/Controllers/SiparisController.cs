using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KitapApi.Data;
using KitapApi.Entities;

namespace KitapApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiparisController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SiparisController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Siparis
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetSiparisler()
        {
            try
            {
                var siparisler = await _context.Siparisler
                    .Include(s => s.Kullanici)
                    .Include(s => s.SiparisDetaylari)
                        .ThenInclude(sd => sd.Kitap)
                    .Select(s => new
                    {
                        Id = s.Id,
                        KullaniciId = s.KullaniciId,
                        SiparisTarihi = s.SiparisTarihi,
                        ToplamTutar = s.ToplamTutar,
                        Durum = s.Durum,
                        Kullanici = s.Kullanici != null ? new
                        {
                            Id = s.Kullanici.Id,
                            AdSoyad = s.Kullanici.AdSoyad,
                            Email = s.Kullanici.Email
                        } : null,
                        SiparisDetaylari = s.SiparisDetaylari.Select(sd => new
                        {
                            Id = sd.Id,
                            Adet = sd.Adet,
                            Fiyat = sd.Fiyat,
                            KitapId = sd.KitapId,
                            SiparisId = sd.SiparisId,
                            Kitap = sd.Kitap != null ? new
                            {
                                Id = sd.Kitap.Id,
                                Ad = sd.Kitap.Ad,
                                Yazar = sd.Kitap.Yazar,
                                Fiyat = sd.Kitap.Fiyat
                            } : null
                        }).ToList()
                    })
                    .ToListAsync();
                return Ok(siparisler);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/Siparis/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Siparis>> GetSiparis(int id)
        {
            try
            {
                var siparis = await _context.Siparisler
                    .Include(s => s.Kullanici)
                    .Include(s => s.SiparisDetaylari)
                        .ThenInclude(sd => sd.Kitap)
                    .Where(s => s.Id == id)
                    .FirstOrDefaultAsync();

                if (siparis == null)
                {
                    return NotFound();
                }

                return Ok(siparis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: api/Siparis
        [HttpPost]
        public async Task<ActionResult<Siparis>> CreateSiparis(Siparis siparis)
        {
            // Sipariş detaylarını da kaydet
            if (siparis.SiparisDetaylari != null && siparis.SiparisDetaylari.Any())
            {
                foreach (var detay in siparis.SiparisDetaylari)
                {
                    detay.SiparisId = siparis.Id; // Bu otomatik olarak ayarlanacak
                }
            }
            
            _context.Siparisler.Add(siparis);
            await _context.SaveChangesAsync();
            
            // Circular reference'ı önlemek için sadece temel bilgileri döndür
            var result = new Siparis
            {
                Id = siparis.Id,
                KullaniciId = siparis.KullaniciId,
                SiparisTarihi = siparis.SiparisTarihi,
                ToplamTutar = siparis.ToplamTutar,
                Durum = siparis.Durum
            };
            
            return CreatedAtAction(nameof(GetSiparis), new { id = siparis.Id }, result);
        }

        // PUT: api/Siparis/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateSiparis(int id, Siparis siparis)
        {
            if (id != siparis.Id)
            {
                return BadRequest();
            }

            _context.Entry(siparis).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SiparisExists(id))
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

        // DELETE: api/Siparis/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteSiparis(int id)
        {
            var siparis = await _context.Siparisler.FindAsync(id);
            if (siparis == null)
            {
                return NotFound();
            }

            _context.Siparisler.Remove(siparis);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: api/Siparis/Onayla/5
        [HttpPut("Onayla/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SiparisOnayla(int id)
        {
            var siparis = await _context.Siparisler.FindAsync(id);
            if (siparis == null)
            {
                return NotFound();
            }

            siparis.Durum = "Onaylandı";
            _context.Entry(siparis).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SiparisExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { message = "Sipariş başarıyla onaylandı" });
        }

        private bool SiparisExists(int id)
        {
            return _context.Siparisler.Any(e => e.Id == id);
        }
    }
}