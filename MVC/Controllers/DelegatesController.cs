using InfraStructure.Context;
using Hassann_Khala.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MVC.Controllers
{
    public class DelegatesController : Controller
    {
        private readonly DBContext _db;
        public DelegatesController(DBContext db) => _db = db;

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Clients = _db.Clients.OrderBy(c => c.Name).Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
            return View(new Hassann_Khala.Domain.Delegate());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Hassann_Khala.Domain.Delegate model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Clients = _db.Clients.OrderBy(c => c.Name).Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList();
                return View(model);
            }

            model.CreatedAt = DateTime.UtcNow;
            _db.Delegates.Add(model);
            _db.SaveChanges();
            TempData["Success"] = "?? ????? ?????? ?????";
            return RedirectToAction("Index", "Delegates");
        }

        [HttpGet]
        public IActionResult Index()
        {
            var list = _db.Delegates.Include(d => d.Client).OrderBy(d => d.Name).ToList();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var del = _db.Delegates.Find(id);
            if (del == null) return NotFound();
            try
            {
                _db.Delegates.Remove(del);
                _db.SaveChanges();
                TempData["Success"] = "?? ??? ??????.";
            }
            catch
            {
                TempData["Error"] = "??? ??? ??????.";
            }
            return RedirectToAction("Index");
        }
    }
}