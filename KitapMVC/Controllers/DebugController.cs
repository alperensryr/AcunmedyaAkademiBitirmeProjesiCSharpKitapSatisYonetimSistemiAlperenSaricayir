using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace KitapMVC.Controllers
{
    public class DebugController : Controller
    {
        public IActionResult CheckAuth()
        {
            var isAuthenticated = User.Identity?.IsAuthenticated ?? false;
            var userName = User.Identity?.Name ?? "No Name";
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "No Role";
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "No ID";
            
            var result = $"Authenticated: {isAuthenticated}\n" +
                        $"Name: {userName}\n" +
                        $"Role: {userRole}\n" +
                        $"ID: {userId}\n" +
                        $"Claims Count: {User.Claims.Count()}";
            
            return Content(result, "text/plain");
        }
    }
}