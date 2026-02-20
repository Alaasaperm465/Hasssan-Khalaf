using Microsoft.AspNetCore.Mvc;
//using Frozen_Warehouse.Application.Interfaces.IServices;
//using MVC.Controllers.Dtos;

namespace MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SectionsController : ControllerBase
    {
        // Provide a simple implementation that reads from DBContext for now
        private readonly InfraStructure.Context.DBContext _db;

        public SectionsController(InfraStructure.Context.DBContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sections = _db.Sections.ToList();
            return Ok(sections);
        }
    }
}
