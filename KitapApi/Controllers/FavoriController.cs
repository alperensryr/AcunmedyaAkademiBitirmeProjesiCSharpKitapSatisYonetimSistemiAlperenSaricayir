using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using KitapApi.Data;
using KitapApi.Entities;
using Microsoft.AspNetCore.Authorization;

namespace KitapApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FavoriController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Favori
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Favori>>> GetFavoriler()
        {
            return await _context.Favoriler
                .Include(f => f.Kullanici)
                .Include(f => f.Kitap)
                .ToListAsync();
        }

        // GET: api/Favori/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Favori>> GetFavori(int id)
        {
            var favori = await _context.Favoriler
                .Include(f => f.Kullanici)
                .Include(f => f.Kitap)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (favori == null)
            {
                return NotFound();
            }

            return favori;
        }

        // POST: api/Favori
        [HttpPost]
        public async Task<ActionResult<Favori>> CreateFavori(Favori favori)
        {
            // Validation için navigation property'leri kontrol et
            if (favori.KitapId <= 0 || favori.KullaniciId <= 0)
            {
                return BadRequest("KitapId ve KullaniciId gereklidir.");
            }
            
            // Navigation property'leri null olarak ayarla çünkü sadece ID'ler gerekli
            favori.Kullanici = null;
            favori.Kitap = null;
            
            // Aynı kullanıcı ve kitap için favori var mı kontrol et
            var existingFavori = await _context.Favoriler
                .FirstOrDefaultAsync(f => f.KitapId == favori.KitapId && f.KullaniciId == favori.KullaniciId);
            
            if (existingFavori != null)
            {
                return BadRequest("Bu kitap zaten favorilerinizde.");
            }
            
            _context.Favoriler.Add(favori);
            await _context.SaveChangesAsync();
            
            // Oluşturulan favoriyi navigation property'leri ile birlikte döndür
            var createdFavori = await _context.Favoriler
                .Include(f => f.Kullanici)
                .Include(f => f.Kitap)
                .FirstOrDefaultAsync(f => f.Id == favori.Id);
                
            return CreatedAtAction(nameof(GetFavori), new { id = favori.Id }, createdFavori);
        }

        // PUT: api/Favori/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateFavori(int id, Favori favori)
        {
            if (id != favori.Id)
            {
                return BadRequest();
            }

            _context.Entry(favori).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FavoriExists(id))
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

        // DELETE: api/Favori/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFavori(int id)
        {
            var favori = await _context.Favoriler.FindAsync(id);
            if (favori == null)
            {
                return NotFound();
            }

            _context.Favoriler.Remove(favori);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FavoriExists(int id)
        {
            return _context.Favoriler.Any(e => e.Id == id);
        }
    }
}