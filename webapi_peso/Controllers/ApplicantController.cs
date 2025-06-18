using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using webapi_peso.ViewModels;

namespace webapi_peso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicantController : ControllerBase
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;
        private readonly IMemoryCache cache;

        public ApplicantController(IDbContextFactory<ApplicationDbContext> _dbFactory, IMemoryCache _cache)
        {
            dbFactory = _dbFactory;
            cache = _cache;
        }

        [HttpGet("GetJobLists")]
        public IActionResult GetJobLists()
        {
            using var db = dbFactory.CreateDbContext();
            var list = cache.Get<List<JobPostViewModel>>($"GetJobLists");
            if (list == null)
            {
                list = new List<JobPostViewModel>();
                //var listofjobs = db.EmployerJobPost.Where(x => x.IsVacant == 0 && !x.IsDeleted).ToList();
                var listofjobs = db.EmployerJobPost.Where(x => x.IsVacant == 0 && x.IsDeleted == false).ToList();



                foreach (var i in listofjobs)
                {
                    if (!i.Expiry.HasValue)
                    {
                        i.Expiry = DateTime.Now.AddMonths(2);
                        db.EmployerJobPost.Update(i);
                        db.SaveChanges();
                    }
                    if (i.Expiry.Value.ToString("yyyy-MM-dd") == "0001-01-01")
                    {
                        i.Expiry = DateTime.Now.AddMonths(2);
                        db.EmployerJobPost.Update(i);
                        db.SaveChanges();
                    }

                    var model = new JobPostViewModel();
                    model.JobPost = i;
                    var dir = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\files\\employers\\{i.EmployerDetailsId}");
                    if (Directory.Exists(dir))
                    {
                        var files = Directory.GetFiles(dir);
                        if (files != null && files.Length > 0)
                            model.FirstImage = files[0];
                    }
                    list.Add(model);
                }
                cache.Set($"GetJobLists", list, TimeSpan.FromSeconds(30));
            }
            return Ok(list);
        }

        [HttpGet("GetApplicantProfile/{accountId}")]
        public async Task<IActionResult> GetApplicantProfile(string accountId)
        {
            using var db = dbFactory.CreateDbContext();
            var model = cache.Get<AccountAndInformationViewModel>($"GetApplicantProfile/{accountId}");
            if (model == null)
            {
                model = new AccountAndInformationViewModel();
                var account = db.UserAccounts.Where(x => x.Id == accountId).FirstOrDefault();
                var email = account?.Email;
                if (string.IsNullOrEmpty(email))
                    return BadRequest("Invalid account: email is missing.");

                var appAccount = db.ApplicantAccount
                    .Where(x => x.Email == email)
                    .OrderByDescending(x => x.DateLastUpdate)
                    .FirstOrDefault();

                //var appAccount = db.ApplicantAccount.Where(x => x.Email == account.Email).OrderByDescending(x => x.DateLastUpdate).FirstOrDefault();
                if (appAccount != null)
                {
                    appAccount.Id = account.Id;
                    db.ApplicantAccount.Update(appAccount);
                    db.SaveChanges();
                }
                var information = db.ApplicantInformation.Where(x => x.Email == account.Email).OrderByDescending(x => x.DateLastUpdate).FirstOrDefault();
                if (information != null)
                {
                    information.AccountId = account.Id;
                    db.ApplicantInformation.Update(information);
                    db.SaveChanges();
                }
                if (information != null)
                {
                    // FOR JOBFAIR ONLY
                    if (ProjectConfig.JobFairEnabled)
                    {
                        if (string.IsNullOrEmpty(information.JobFairReferenceCode))
                        {
                            var refCode = Helper.Random6digitNumbers();
                            while (db.ApplicantInformation.Any(x => x.JobFairReferenceCode == refCode))
                            {
                                refCode = Helper.Random6digitNumbers();
                            }
                            information.JobFairReferenceCode = refCode;
                            db.ApplicantInformation.Update(information);
                            db.SaveChanges();
                        }
                    }
                }

                model.UserAccount = account;
                model.ApplicantAccount = appAccount;
                model.ApplicantInformation = information;
                //model.ApplicantInformation.PresentRegion = referenceAdd.FindRegion(model.ApplicantInformation.PresentRegion).regDesc;
                //model.ApplicantInformation.ProvincialRegion = referenceAdd.FindRegion(model.ApplicantInformation.ProvincialRegion).regDesc;
                //model.ApplicantInformation.PresentProvince = referenceAdd.FindProvince(model.ApplicantInformation.PresentProvince).provDesc;
                //model.ApplicantInformation.ProvincialProvince = referenceAdd.FindProvince(model.ApplicantInformation.ProvincialProvince).provDesc;
                //model.ApplicantInformation.PresentMunicipalityCity = referenceAdd.FindCity(model.ApplicantInformation.PresentMunicipalityCity).citymunDesc;
                //model.ApplicantInformation.ProvincialMunicipalityCity = referenceAdd.FindCity(model.ApplicantInformation.ProvincialMunicipalityCity).citymunDesc;
                //model.ApplicantInformation.PresentBarangay = (await referenceAdd.GetSpecificBrgy(model.ApplicantInformation.PresentBarangay)).brgyDesc;
                //model.ApplicantInformation.ProvincialBarangay = (await referenceAdd.GetSpecificBrgy(model.ApplicantInformation.ProvincialBarangay)).brgyDesc;



                cache.Set($"GetApplicantProfile/{accountId}", model, TimeSpan.FromSeconds(30));
            }

            return Ok(model);
        }

        [HttpGet("GetNumberOfVerifiedApplicant")]
        public IActionResult GetNumberOfVerifiedApplicant()
        {
            using var db = dbFactory.CreateDbContext();
            var count = cache.Get<int>("GetNumberOfVerifiedApplicant");
            if (count == 0)
            {
                count = db.ApplicantInformation.Where(x => db.ApplicantAccount.Any(a => a.IsReviewedReturned == 1 && a.IsRemoved == 0 && x.AccountId == a.Id)).MyDistinctBy(x => x.Email).Count();
                cache.Set("GetNumberOfVerifiedApplicant", count, TimeSpan.FromSeconds(20));
            }
            return Ok(count);
        }

        [HttpGet("GetEmployerDetailsByEmail/{AccountId}")]
        public IActionResult GetEmployerDetailsByEmail(string AccountId)
        {
            using var db = dbFactory.CreateDbContext();

            var account = db.UserAccounts.Where(x => x.Id == AccountId).FirstOrDefault();
            if (account != null)
            {
                var emDetails = db.EmployerDetails.Where(x => x.ContactEmailAddress == account.Email).FirstOrDefault();
                if (emDetails != null)
                {
                    return Ok(emDetails);
                }
            }

            return BadRequest();
        }
    }
}
