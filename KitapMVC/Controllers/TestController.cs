using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KitapMVC.Controllers
{
    public class TestController : Controller
    {
        public IActionResult SessionCheck()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var userName = User.Identity?.Name ?? "No Name";
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "No Role";
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "No ID";
            
            var sessionKullaniciId = HttpContext.Session.GetInt32("KullaniciId");
            var sessionKullaniciAd = HttpContext.Session.GetString("KullaniciAd");
            var sessionKullaniciRol = HttpContext.Session.GetString("KullaniciRol");
            var sessionJwtToken = HttpContext.Session.GetString("JwtToken");
            
            var result = $"=== AUTHENTICATION ===\n" +
                        $"Authenticated: {isAuthenticated}\n" +
                        $"Name: {userName}\n" +
                        $"Role: {userRole}\n" +
                        $"ID: {userId}\n" +
                        $"Claims Count: {User.Claims.Count()}\n\n" +
                        $"=== SESSION ===\n" +
                        $"KullaniciId: {sessionKullaniciId}\n" +
                        $"KullaniciAd: {sessionKullaniciAd}\n" +
                        $"KullaniciRol: {sessionKullaniciRol}\n" +
                        $"JwtToken: {(!string.IsNullOrEmpty(sessionJwtToken) ? "SET" : "NOT SET")}\n\n" +
                        $"=== LAYOUT CONDITION ===\n" +
                        $"User.IsInRole(\"Admin\"): {User.IsInRole("Admin")}\n" +
                        $"Session Role == Admin: {sessionKullaniciRol == "Admin"}\n" +
                        $"Combined Condition: {User.IsInRole("Admin") || sessionKullaniciRol == "Admin"}";
            
            return Content(result, "text/plain");
        }
    }
}