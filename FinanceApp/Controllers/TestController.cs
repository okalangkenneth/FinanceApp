using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Data; // Replace YourNamespace with your project's namespace

namespace FinanceApp.Controllers // Replace YourNamespace with your project's namespace
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("test-connection")]
        public IActionResult TestConnection()
        {
            try
            {
                _context.Database.ExecuteSqlRaw("SELECT 1");
                return Ok("Database connection successful.");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Database connection failed. Error: {ex.Message}");
            }
        }
    }
}
