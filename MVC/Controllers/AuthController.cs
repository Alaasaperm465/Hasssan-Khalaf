using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MVC.Controllers.Dtos;
using InfraStructure.Context;

namespace MVC.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DBContext _db;
        public AuthController(DBContext db)
        {
            _db = db;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = _db.Users.FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password);
            if (user == null) return Unauthorized();

            return Ok(new { username = user.Username, role = user.Role });
        }
    }
}