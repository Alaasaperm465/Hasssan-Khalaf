using Microsoft.AspNetCore.Mvc;
using InfraStructure.Context;
using Microsoft.EntityFrameworkCore;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly DBContext _db;
        public HomeController(DBContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            // gather metrics for dashboard
            var totalProducts = await _db.Products.AsNoTracking().CountAsync();

            var utcNow = DateTime.UtcNow;
            var todayStart = utcNow.Date;
            var todayEnd = todayStart.AddDays(1);

            var todayInbounds = await _db.Inbounds.AsNoTracking()
                .Where(i => i.CreatedAt >= todayStart && i.CreatedAt < todayEnd)
                .CountAsync();

            var todayOutbounds = await _db.Outbounds.AsNoTracking()
                .Where(o => o.CreatedAt >= todayStart && o.CreatedAt < todayEnd)
                .CountAsync();

            var recentInbounds = await _db.Inbounds
                .Include(i => i.Client)
                .Include(i => i.Details)
                .OrderByDescending(i => i.CreatedAt)
                .Take(6)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.TotalProducts = totalProducts;
            ViewBag.TodayInbounds = todayInbounds;
            ViewBag.TodayOutbounds = todayOutbounds;
            ViewBag.RecentInbounds = recentInbounds;

            return View();
        }
    }
}
