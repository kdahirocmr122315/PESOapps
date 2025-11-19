
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using webapi_peso;
using webapi_peso.Model;
using webapi_peso.ViewModels;


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
        public readonly IWebHostEnvironment env;
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

        [HttpGet("GetEstablishmentData/{userId}")]
        public EmployerRegistrationViewModel GetEstablishmentData(string userId)
        {
            using var db = dbFactory.CreateDbContext();
            var data = cache.Get<EmployerRegistrationViewModel>($"GetEstablishmentData/{userId}");
            if (data == null)
            {
                data = new EmployerRegistrationViewModel();
                data.EmployerDetails = db.EmployerDetails.Include(x => x.JobPosts).Where(x => x.Id == userId).FirstOrDefault();
                data.ListOfAttachments = new();
                var attachmentsDirectory = System.IO.Path.Combine(env.ContentRootPath, "files", "employers", data.EmployerDetails.Id);
                if (!System.IO.Directory.Exists(attachmentsDirectory))
                    System.IO.Directory.CreateDirectory(attachmentsDirectory);
                foreach (var file in Directory.GetFiles(attachmentsDirectory))
                {
                    var fileInfo = new FileInfo(file);
                    data.ListOfAttachments.Add(new()
                    {
                        FileName = System.IO.Path.GetFileName(file),
                        FolderName = data.EmployerDetails.Id,
                        FileSize = Helper.SizeSuffix(fileInfo.Length),
                        IsAlreadyUploaded = 1
                    });
                }
                cache.Set($"GetEstablishmentData/{userId}", data, TimeSpan.FromSeconds(30));
            }

            return data;
        }

        [HttpPost("SetEstablishmentStatus")]
        public async Task<EmployerRegistrationViewModel> SetEstablishmentStatus(EmployerDetails em)
        {
            using var db = dbFactory.CreateDbContext();
            /*
                        if (em.Status == ProjectConfig.ACCOUNT_STATUS.RETURNED)
                        {
                            var session = db.UpdateDetailsSessions.Where(x => x.AccountId == em.Id).FirstOrDefault();
                            if(session != null)
                            {
                                db.UpdateDetailsSessions.Remove(session);
                                db.SaveChanges();
                                db.Entry(session).State = EntityState.Detached;
                                db.SaveChanges();
                            }
                            session = new();
                            session.AccountId = em.Id;
                            session.DateExpiry = DateTime.Now.AddDays(1);
                            db.UpdateDetailsSessions.Add(session);
                            db.SaveChanges();
                        }*/
            var account = db.UserAccounts.Where(x => x.Email == em.ContactEmailAddress).FirstOrDefault();
            if (account == null)
            {
                account = new();
                account.Email = em.ContactEmailAddress;
                account.Name = em.EstablishmentName;
                account.InActive = 0;
                account.UserType = ProjectConfig.USER_TYPE.EMPLOYER;
                account.GivenName = em.AcronymAbbreviation;
                account.Password = Helper.RandomString(6).ToLower();
                account.DateCreated = DateTime.Now;
                db.UserAccounts.Add(account);
                db.SaveChanges();
            }
            db.EmployerDetails.Update(em);
            db.SaveChanges();
            //if (em.Status == ProjectConfig.ACCOUNT_STATUS.APPROVED)
            //{
            //    MailMessage message = new MailMessage();
            //    SmtpClient smtp = new SmtpClient();
            //    message.From = new MailAddress(_mailConfig.FromEmail);
            //    message.To.Add(new MailAddress(em.ContactEmailAddress));
            //    message.Subject = "ESTABLISHMENT REGISTRATION";
            //    message.IsBodyHtml = true;
            //    message.Body = "" +
            //        "<div style='padding-left:20px;padding-right:20px'>" +
            //        "<h2 style='font-family:Century Gothic;font-weight:300'>Hi Ma’am/Sir,</h2>" +
            //        "<h2 style='font-family:Century Gothic;font-weight:300'>Good day!</h2>" +
            //        "<h2 style='font-family:Century Gothic;font-weight:300'>It is our pleasure to inform you that your Application for Registration has been approved. You can now access our website (<a href='https://pesomisor.com'>www.pesomisor.com</a>) using your account with the hereunder details:</h2>" +
            //        $"<h2 style='font-family:Century Gothic;font-weight:300'><b>Username: {account.Email}</b><br /><b>Password: {account.Password}</b><br /></h2>" +
            //        "<h2 style='font-family:Century Gothic;font-weight:300'>We look forward for your cooperation by providing us regularly with job vacancies, as this Office aims to ensure prompt and timely provision of Referral and Placement Services to its clients.</h2>" +
            //        "<h2 style='font-family:Century Gothic;font-weight:300'>Thank you very much and have a great day ahead!!!</h2>" +
            //        $"{EMAIL_FOOTER}" +
            //        "</div>";
            //    await SendEmail(message.Subject, em.ContactEmailAddress, message.Body);
                /*smtp.Port = _mailConfig.Port;
                smtp.Host = _mailConfig.Host;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(_mailConfig.Username, _mailConfig.Password);
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                await smtp.SendMailAsync(message);*/
            //}
            return GetEstablishmentData(em.Id);
        }

    }
}
