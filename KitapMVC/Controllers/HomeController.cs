using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using KitapMVC.Models; // Bu sat�r zaten olmal�

namespace KitapMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // ---------- BURADAN SONRASINI EKLE ----------

        public IActionResult Hakkimizda()
        {
            ViewData["Title"] = "Hakk�m�zda"; // Sayfa ba�l���n� belirleyelim
            return View();
        }

        public IActionResult BizeUlasin()
        {
            ViewData["Title"] = "Bize Ula��n"; // Sayfa ba�l���n� belirleyelim
            return View();
        }

        // ---------------------------------------------
    }
}
//using System.Diagnostics;
//using Microsoft.AspNetCore.Mvc;
//using KitapMVC.Models;

//namespace KitapMVC.Controllers;

//public class HomeController : Controller
//{
//    private readonly ILogger<HomeController> _logger;

//    public HomeController(ILogger<HomeController> logger)
//    {
//        _logger = logger;
//    }

//    public IActionResult Index()
//    {
//        return View();
//    }

//    public IActionResult Privacy()
//    {
//        return View();
//    }

//    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//    public IActionResult Error()
//    {
//        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//    }
//}
