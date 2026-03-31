using Hassann_Khala.Application.DTOs.Reports;
using Hassann_Khala.Application.Interfaces.IServices;
using InfraStructure.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MVC.Controllers
{
    public class MovementDetailsController : Controller
    {
        private readonly IMovementDetailsService _service;
        private readonly IPdfService _pdfService;
        private readonly DBContext _db;

        public MovementDetailsController(IMovementDetailsService service, IPdfService pdfService, DBContext db)
        {
            _service = service;
            _pdfService = pdfService;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Clients = await _db.Clients.OrderBy(c => c.Name).Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToListAsync();
            ViewBag.Sections = await _db.Sections.OrderBy(s => s.Name).Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToListAsync();
            ViewBag.Products = await _db.Products.OrderBy(p => p.Name).Select(p => new SelectListItem(p.Name, p.Id.ToString())).ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetData([FromForm] MovementFilterDto filter)
        {
            var result = await _service.GetMovementDetailsAsync(filter);
            return PartialView("_MovementDetailsResult", result);
        }

        [HttpPost]
        public IActionResult ExportPdf([FromForm] MovementFilterDto filter)
        {
            var report = _service.GetMovementDetailsAsync(filter).GetAwaiter().GetResult();
            var bytes = _pdfService.GenerateMovementDetailsPdf(report);
            return File(bytes, "application/pdf", "movement-details.pdf");
        }
    }
}
