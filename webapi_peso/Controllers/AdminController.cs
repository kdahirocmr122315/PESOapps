
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace authpro.Controllers
{
    [Route("admin/[controller]")]
    [ApiController]

    public class AdminController : ControllerBase
    {

        private readonly IConfiguration  _configuration;


      
        private readonly string? _connectionString;
        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;

           
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

        
      

    }
}
