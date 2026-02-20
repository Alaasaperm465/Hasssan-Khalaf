using Hassann_Khala.Domain;
using InfraStructure.Context;
using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SectionController : ControllerBase
    {
        private readonly ILogger<SectionController> _logger;
        private readonly DBContext _context;
        public SectionController(ILogger<SectionController> logger, DBContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpGet]
        public IActionResult GetAllSections()
        {
            var sections = _context.Sections.ToList();
            return Ok(sections);
        }
        [HttpPost]
        public IActionResult AddSection(Section section)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }
            _context.Sections.Add(section);
            _context.SaveChanges();
            return Ok();
        }
    }
}
