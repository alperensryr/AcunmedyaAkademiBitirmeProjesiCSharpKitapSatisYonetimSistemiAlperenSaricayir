//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using KitapApi.Data;
//using KitapApi.Entities;

using KitapApi.Data; // DbContext için
using KitapApi.Entities; // Kitap modeli için
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // ToListAsync vs. için
using KitapApi.Attributes; // ApiAuthorize için
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace KitapApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KitaplarController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public KitaplarController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Kitaplar
        // Tüm kitapları ve ilişkili kategori bilgilerini getirir.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Kitap>>> GetKitaplar()
        {
            return await _context.Kitaplar.Include(k => k.Kategori).ToListAsync();
        }

        // GET: api/Kitaplar/5
        // Belirli bir ID'ye sahip kitabı ve ilişkili kategori bilgisini getirir.
        [HttpGet("{id}")]
        public async Task<ActionResult<Kitap>> GetKitap(int id)
        {
            var kitap = await _context.Kitaplar.Include(k => k.Kategori).FirstOrDefaultAsync(i => i.Id == id);

            if (kitap == null)
            {
                return NotFound();
            }

            return kitap;
        }

        // GET: api/Kitaplar/kategori/5
        // Belirli bir kategoriye ait kitapları getirir.
        [HttpGet("kategori/{kategoriId}")]
        public async Task<ActionResult<IEnumerable<Kitap>>> GetKitaplarByKategori(int kategoriId)
        {
            var kitaplar = await _context.Kitaplar
                .Include(k => k.Kategori)
                .Where(k => k.KategoriId == kategoriId)
                .ToListAsync();

            return kitaplar;
        }

        // POST: api/Kitaplar
        // Yeni bir kitap ekler.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Kitap>> CreateKitap(Kitap kitap)
        {
            if (kitap.KategoriId == 0)
            {
                return BadRequest("Kitap için bir KategoriId belirtilmelidir.");
            }

            var kategoriVarMi = await _context.Kategoriler.AnyAsync(k => k.Id == kitap.KategoriId);
            if (!kategoriVarMi)
            {
                return BadRequest($"Belirtilen KategoriId ({kitap.KategoriId}) bulunamadı.");
            }

            _context.Kitaplar.Add(kitap);
            await _context.SaveChangesAsync();

            // Yeni oluşturulan kaynağın URL'sini (GetKitap metodu ile) ve nesneyi döndürür
            return CreatedAtAction(nameof(GetKitap), new { id = kitap.Id }, kitap);
        }

        // PUT: api/Kitaplar/5
        // Belirli bir ID'ye sahip kitabı günceller.
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateKitap(int id, Kitap kitap)
        {
            // Gelen ID ile kitap nesnesinin ID'si eşleşmeli
            if (id != kitap.Id)
            {
                return BadRequest("Güncellenmek istenen kitabın ID'si ile URL'deki ID eşleşmiyor.");
            }

            // KategoriId kontrolü (isteğe bağlı ama iyi bir pratik)
            if (kitap.KategoriId == 0)
            {
                return BadRequest("Kitap için bir KategoriId belirtilmelidir.");
            }

            var kategoriVarMi = await _context.Kategoriler.AnyAsync(k => k.Id == kitap.KategoriId);
            if (!kategoriVarMi)
            {
                return BadRequest($"Belirtilen KategoriId ({kitap.KategoriId}) bulunamadı.");
            }

            // Entity'nin durumunu Modified olarak işaretle
            _context.Entry(kitap).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Kitap bulunamadıysa (aynı anda silinmiş olabilir) veya başka bir çakışma olduysa
                if (!KitapExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw; // Bilinmeyen bir concurrency hatası ise yeniden fırlat
                }
            }

            return NoContent(); // Başarılı güncelleme için içerik yok döner
        }

        // DELETE: api/Kitaplar/5
        // Belirli bir ID'ye sahip kitabı siler.
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteKitap(int id)
        {
            var kitap = await _context.Kitaplar.FindAsync(id);
            if (kitap == null)
            {
                return NotFound(); // Kitap bulunamadıysa
            }

            _context.Kitaplar.Remove(kitap); // Kitabı sil
            await _context.SaveChangesAsync();

            return NoContent(); // Başarılı silme için içerik yok döner
        }

        [HttpPost("upload")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UploadImage(int kitapId, IFormFile file)
        {
            var kitap = await _context.Kitaplar.FindAsync(kitapId);
            if (kitap == null)
                return NotFound();
            if (file == null || file.Length == 0)
                return BadRequest("Dosya seçilmedi.");

            var currentDirectory = Directory.GetCurrentDirectory();
            Console.WriteLine($"Current Directory: {currentDirectory}");
            
            var uploadsFolder = Path.Combine(currentDirectory, "wwwroot", "uploads");
            Console.WriteLine($"Uploads Folder Path: {uploadsFolder}");
            
            if (!Directory.Exists(uploadsFolder))
            {
                Console.WriteLine($"Creating directory: {uploadsFolder}");
                Directory.CreateDirectory(uploadsFolder);
            }
            
            var fileName = $"kitap_{kitapId}_{Guid.NewGuid().ToString().Substring(0,8)}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            Console.WriteLine($"File Path: {filePath}");
            
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            kitap.ResimUrl = $"/uploads/{fileName}";
            await _context.SaveChangesAsync();
            return Ok(new { kitap.Id, kitap.ResimUrl });
        }

        // Kitabın veritabanında var olup olmadığını kontrol eden yardımcı metot.
        private bool KitapExists(int id)
        {
            return _context.Kitaplar.Any(e => e.Id == id);
        }
    }
}
//[Route("api/[controller]")]
//[ApiController]
//public class KitaplarController : ControllerBase
//{
//    private readonly ApplicationDbContext _context;

//    public KitaplarController(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    // GET: api/Kitaplar
//    [HttpGet]
//    public async Task<ActionResult<IEnumerable<Kitap>>> GetKitaplar()
//    {
//        return await _context.Kitaplar.Include(k => k.Kategori).ToListAsync();
//    }

//    // GET: api/Kitaplar/5
//    [HttpGet("{id}")]
//    public async Task<ActionResult<Kitap>> GetKitap(int id)
//    {
//        var kitap = await _context.Kitaplar.Include(k => k.Kategori).FirstOrDefaultAsync(k => k.Id == id);

//        if (kitap == null)
//        {
//            return NotFound();
//        }

//        return kitap;
//    }
//}