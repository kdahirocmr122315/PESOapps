using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace webapi_peso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;

        public UserController(IDbContextFactory<ApplicationDbContext> _dbFactory, IConfiguration config)
        {
            dbFactory = _dbFactory;
        }


        [HttpGet("GetUser/{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            using var db = dbFactory.CreateDbContext();
            var user = await db.UserAccounts.FindAsync(userId);

            if (user == null)
                return NotFound($"User with ID {userId} not found.");

            return Ok(user);
        }
    }
}
