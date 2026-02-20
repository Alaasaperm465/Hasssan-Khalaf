using Microsoft.AspNetCore.Mvc;
using InfraStructure.Context;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly DBContext _db;
        public HomeController(DBContext db) => _db = db;

        public IActionResult Index()
        {
            return View();
        }
    }
}
