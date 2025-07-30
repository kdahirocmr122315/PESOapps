
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using webapi_peso;
using webapi_peso.Model;


namespace authpro.Controllers
{
    [Route("admin/[controller]")]
    [ApiController]

    public class AdminController : ControllerBase
    {

        private readonly IConfiguration  _configuration;
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;
        private readonly IMemoryCache cache;
        private readonly string? _connectionString;
        public AdminController(IConfiguration configuration, IDbContextFactory<ApplicationDbContext> _dbFactory, IMemoryCache _cache)
        {
            _configuration = configuration;
            dbFactory = _dbFactory;
            cache = _cache;
        }

        [HttpPost]
        public IActionResult Login(AdminAuth adminAuth)
        {
            


            try
            {
                if (string.IsNullOrEmpty(adminAuth.UserName) || string.IsNullOrEmpty(adminAuth.Password))
                    return BadRequest("Username and/or Password not specified");
                if (adminAuth.UserName.Equals("mis") && adminAuth.Password.Equals("mis"))
                {

                   

                    var secretKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["AppSettings:Token"]));
                    var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

                    var jwtSecurityToken = new JwtSecurityToken(
                        issuer: _configuration["AppSettings:Issuer"],
                        audience: _configuration["AppSettings:Audience"],
                        claims: new List<Claim>(),
                        expires: DateTime.Now.AddDays(Convert.ToDouble(_configuration["AppSettings:ndays_expired"])),
                        signingCredentials: signinCredentials
                    );
                    //Ok(new JwtSecurityTokenHandler().
                    //WriteToken(jwtSecurityToken));

                    var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                    return Ok(new { Token = tokenString });
                  
                }
            }
            catch
            {
                return BadRequest
                ("An error occurred in generating the token");
            }
            return Unauthorized();
        }

        [HttpGet("GetUser/{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            using var db = dbFactory.CreateDbContext();
            var user = await db.UserAccounts.FindAsync(userId);
            return Ok(user);
        }

        [HttpGet("GetUserAccounts")]
        public async Task<List<UserAccount>> GetUserAccounts()
        {
            using var db = dbFactory.CreateDbContext();
            var rs = cache.Get<List<UserAccount>>($"GetUserAccounts");
            if (rs == null)
            {
                rs = await db.UserAccounts.Where(x => x.Email.ToLower().Trim() != "superadmin").OrderByDescending(x => x.DateCreated).ToListAsync();
                cache.Set($"GetUserAccounts", rs, TimeSpan.FromSeconds(60));
            }
            return rs;
        }

        [HttpPost("CreateEditUserAccount")]
        public IActionResult CreateEditUserAccount(UserAccount data)
        {
            using var db = dbFactory.CreateDbContext();
            if (string.IsNullOrEmpty(data.Id))
            {
                db.UserAccounts.Add(data);
                db.SaveChanges();
                return Ok(data);
            }
            else
            {
                db.UserAccounts.Update(data);
                db.SaveChanges();
                return Ok(data);
            }
        }

        [HttpGet("GetSuggestions")]
        public IActionResult GetSuggestions()
        {
            using var db = dbFactory.CreateDbContext();
            var rs = db.UserSuggestions.OrderByDescending(x => x.DateCreated).ToList();
            return Ok(rs);
        }
        [HttpPost("SaveSuggestions")]
        public IActionResult SaveSuggestions(UserSuggestion data)
        {
            using var db = dbFactory.CreateDbContext();
            db.UserSuggestions.Add(data);
            db.SaveChanges();
            return Ok(1);
        }
        [HttpPost("UpdateSuggestion")]
        public IActionResult UpdateSuggestion(UserSuggestion data)
        {
            using var db = dbFactory.CreateDbContext();
            data.IsOk = data.IsOk == 1 ? 0 : 1;
            db.UserSuggestions.Update(data);
            db.SaveChanges();
            return Ok(1);
        }

    }
}
