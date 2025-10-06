using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PESOServerAPI.Services;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using webapi_peso.Model;
using webapi_peso.ViewModels;

namespace webapi_peso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class ApplicantController : ControllerBase
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;
        private readonly IMemoryCache cache;
        private readonly IWebHostEnvironment env;
        private readonly IGmailServices gmail;

        public ApplicantController(IDbContextFactory<ApplicationDbContext> _dbFactory, IMemoryCache _cache, IWebHostEnvironment _env, IGmailServices gmail)
        {
            dbFactory = _dbFactory;
            cache = _cache;
            env = _env;
            this.gmail = gmail;
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
                if (listofjobs.Count == 0)
                    return NotFound();

                foreach (var i in listofjobs)
                {
                    if (!i.Expiry.HasValue || i.Expiry.Value.ToString("yyyy-MM-dd") == "0001-01-01")
                    {
                        i.Expiry = DateTime.Now.AddMonths(2);
                        db.EmployerJobPost.Update(i);
                        db.SaveChanges();
                    }

                    var model = new JobPostViewModel();
                    model.JobPost = i;

                    // ✅ Add this line to include EmployerDetails
                    model.EmpDetails = db.EmployerDetails.FirstOrDefault(e => e.Id == i.EmployerDetailsId);
                    if (model.EmpDetails == null)
                        continue; // Skip this job post

                    // ✅ Optional: attach first image
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

        [HttpGet("GetEmployerDetails/{empId}")]
        public IActionResult GetEmployerDetails(string empId)
        {
            using var db = dbFactory.CreateDbContext();
            var rs = cache.Get<EmployerDetails>($"GetEmployerDetails/{empId}");
            if (rs == null)
            {
                rs = new EmployerDetails();
                rs = db.EmployerDetails.Where(x => x.Id == empId).FirstOrDefault();
                cache.Set($"GetEmployerDetails/{empId}", rs, TimeSpan.FromSeconds(30));
            }
            return Ok(rs);
        }

        [HttpGet("GetAllApplicationsByApplicant/{applicantId}")]
        public async Task<IActionResult> GetAllApplicationsByApplicant(string applicantId)
        {
            using (var db = dbFactory.CreateDbContext()) 
            {
                var list = cache.Get<List<AppliedJobsViewModel>>($"GetAllApplicationsByApplicant/{applicantId}");
                if (list == null)
                {
                    list = new List<AppliedJobsViewModel>();
                    var jobPosts = db.JobApplicantion.Where(x => x.ApplicantId == applicantId).ToList();
                    if (jobPosts.Count == 0)
                        return NotFound();
                    foreach (var jobPost in jobPosts)
                    {
                        var model = new AppliedJobsViewModel();
                        var jobInfo = db.EmployerJobPost.Where(x => x.Id == jobPost.JobPostId).FirstOrDefault();
                        var employerDetails = db.EmployerDetails.Where( x => x.Id == jobInfo.EmployerDetailsId).FirstOrDefault();
                        model.Post = jobInfo;
                        model.EmployerDetails = employerDetails;
                        model.ApplicantId = applicantId;
                        model.DateCreated = jobPost.DateCreated;
                        model.JobPostId = jobPost.JobPostId;
                        list.Add(model);
                    }
                    cache.Set($"GetAllApplicationsByApplicant/{applicantId}", list, TimeSpan.FromSeconds(30));
                }
                return Ok(list);

            }
        }

        //NSRP

        [HttpGet("RemoveNSRPRecord/{accountId}")]
        public IActionResult RemoveNSRPRecord(string accountId)
        {
            using var db = dbFactory.CreateDbContext();
            var account = db.ApplicantAccount.Where(x => x.Id == accountId).FirstOrDefault();
            if (account != null)
            {
                account.IsRemoved = 1;
                account.DateLastUpdate = Helper.currentTimeMillis();
                db.ApplicantAccount.Update(account);

                var appInfo = db.ApplicantInformation.Where(x => x.AccountId == accountId).FirstOrDefault();
                if (appInfo != null)
                {
                    appInfo.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantInformation.Update(appInfo);
                }
            }
            db.SaveChanges();
            return Ok();
        }
        [HttpGet("RestoreNSRPRecord/{accountId}")]
        public IActionResult RestoreNSRPRecord(string accountId)
        {
            using var db = dbFactory.CreateDbContext();
            var account = db.ApplicantAccount.Find(accountId);
            if (account != null)
            {
                account.IsRemoved = 0;
                account.DateLastUpdate = Helper.currentTimeMillis();
                db.ApplicantAccount.Update(account);

                var appInfo = db.ApplicantInformation.Where(x => x.AccountId == accountId).FirstOrDefault();
                if (appInfo != null)
                {
                    appInfo.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantInformation.Update(appInfo);
                }
            }
            db.SaveChanges();
            return Ok();
        }

        [HttpGet("CheckEmailExist/{email}")]
        public IActionResult CheckEmailExist(string email)
        {
            using var db = dbFactory.CreateDbContext();
            var param = new ParameterViewModel();
            param.Result = db.UserAccounts.Any(x => x.Email == email);
            return Ok(param);
        }

        [HttpPost("SaveNSRP")]
        public IActionResult SaveNSRP(ApplicantDataViewModel data)
        {
            using var db = dbFactory.CreateDbContext();

            var userAccount = db.UserAccounts.Where(x => x.Email == data.ApplicantInformation.Email).FirstOrDefault();
            if (userAccount == null)
            {
                string base64Guid = Guid.NewGuid().ToString();
                userAccount = new UserAccount();
                userAccount.Id = base64Guid;
                userAccount.Email = data.ApplicantInformation.Email;
                userAccount.Password = Helper.RandomString(6).ToLower();
                userAccount.Name = $"{data.ApplicantInformation.FirstName} {data.ApplicantInformation.SurName}";
                userAccount.DateCreated = DateTime.Now;
                userAccount.UserType = ProjectConfig.USER_TYPE.APPLICANT;
                userAccount.InActive = 0;
                userAccount.LastLoggedIn = DateTime.Now;
                db.UserAccounts.Add(userAccount);
                db.SaveChanges();
            }
            var account = db.ApplicantAccount.Where(x => x.Email.ToLower() == data.ApplicantInformation.Email.ToLower()).FirstOrDefault();
            if (account == null)
            {
                account = new ApplicantAccount();
                account.Id = userAccount.Id;
                account.Username = userAccount.Email;
                account.Password = userAccount.Password;
                account.Email = userAccount.Email;
                account.IsEmailVerified = 0;
                account.IsReviewedReturned = 1;
                account.DateRegistered = Helper.currentTimeMillis();
                account.DateLastUpdate = Helper.currentTimeMillis();
                account.IsRemoved = 0;
                db.ApplicantAccount.Add(account);
                db.SaveChanges();
            }
            else
            {
                db.Entry(account).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                db.SaveChanges();

                account.DateLastUpdate = Helper.currentTimeMillis();
                account.Id = userAccount.Id;
                account.Username = userAccount.Email;
                account.Password = userAccount.Password;
                account.Email = userAccount.Email;
                account.IsReviewedReturned = 1;
                db.ApplicantAccount.Update(account);
                db.SaveChanges();
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ApplicantInformation.AccountId = userAccount.Id;
                    data.ApplicantExpectedSalary.AccountId = userAccount.Id;
                    data.ApplicantEducationalBackground.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantEligibility.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantJobPrefOccupation.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantJobPrefWorkLocation.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantLanguageDialectProf.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantOtherSkills.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantProfessionalLicense.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantTechnicalVocational.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantWorkExperience.ForEach(x => x.AccountId = userAccount.Id);

                    db.ApplicantInformation.Add(data.ApplicantInformation);
                    db.ApplicantEducationalBackground.AddRange(data.ApplicantEducationalBackground);
                    db.ApplicantEligibility.AddRange(data.ApplicantEligibility);
                    db.ApplicantExpectedSalary.Add(data.ApplicantExpectedSalary);
                    db.ApplicantJobPrefOccupation.AddRange(data.ApplicantJobPrefOccupation);
                    db.ApplicantJobPrefWorkLocation.AddRange(data.ApplicantJobPrefWorkLocation);
                    db.ApplicantLanguageDialectProf.AddRange(data.ApplicantLanguageDialectProf);
                    db.ApplicantOtherSkills.AddRange(data.ApplicantOtherSkills);
                    db.ApplicantProfessionalLicense.AddRange(data.ApplicantProfessionalLicense);
                    db.ApplicantTechnicalVocational.AddRange(data.ApplicantTechnicalVocational);
                    db.ApplicantWorkExperience.AddRange(data.ApplicantWorkExperience);
                    db.SaveChanges();
                    transaction.Commit();
                    return Ok(userAccount.Id);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Ok(ex.Message);
                }
            }
            return NotFound();
        }

        [AllowAnonymous]
        [HttpPost("SubmitNSRPForm")]
        public IActionResult SubmitNSRPForm(ApplicantDataViewModel data)
        {
            using var db = dbFactory.CreateDbContext();

            var userAccount = db.UserAccounts.Where(x => x.Email == data.ApplicantInformation.Email).FirstOrDefault();
            if (userAccount == null)
            {
                string base64Guid = Guid.NewGuid().ToString();
                userAccount = new UserAccount();
                userAccount.Id = base64Guid;
                userAccount.Email = data.ApplicantInformation.Email;
                userAccount.Password = Helper.RandomString(6).ToLower();
                userAccount.Name = $"{data.ApplicantInformation.FirstName} {data.ApplicantInformation.SurName}";
                userAccount.DateCreated = DateTime.Now;
                userAccount.UserType = ProjectConfig.USER_TYPE.APPLICANT;
                db.UserAccounts.Add(userAccount);
                db.SaveChanges();
            }
            var account = db.ApplicantAccount.Where(x => x.Email.ToLower() == data.ApplicantInformation.Email.ToLower()).FirstOrDefault();
            if (account == null)
            {
                account = new ApplicantAccount();
                account.Id = userAccount.Id;
                account.Username = userAccount.Email;
                account.Password = userAccount.Password;
                account.Email = userAccount.Email;
                account.IsReviewedReturned = 1;
                account.DateRegistered = Helper.currentTimeMillis();
                account.DateLastUpdate = Helper.currentTimeMillis();
                db.ApplicantAccount.Add(account);
                db.SaveChanges();
            }
            else
            {
                db.Entry(account).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                db.SaveChanges();

                account.DateLastUpdate = Helper.currentTimeMillis();
                account.Id = userAccount.Id;
                account.Username = userAccount.Email;
                account.Password = userAccount.Password;
                account.Email = userAccount.Email;
                account.IsReviewedReturned = 0;
                db.ApplicantAccount.Update(account);
                db.SaveChanges();
            }

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    data.ApplicantInformation.AccountId = userAccount.Id;
                    data.ApplicantExpectedSalary.AccountId = userAccount.Id;
                    data.ApplicantEducationalBackground.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantEligibility.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantJobPrefOccupation.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantJobPrefWorkLocation.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantLanguageDialectProf.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantOtherSkills.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantProfessionalLicense.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantTechnicalVocational.ForEach(x => x.AccountId = userAccount.Id);
                    data.ApplicantWorkExperience.ForEach(x => x.AccountId = userAccount.Id);

                    db.ApplicantInformation.Add(data.ApplicantInformation);
                    db.ApplicantEducationalBackground.AddRange(data.ApplicantEducationalBackground);
                    db.ApplicantEligibility.AddRange(data.ApplicantEligibility);
                    db.ApplicantExpectedSalary.Add(data.ApplicantExpectedSalary);
                    db.ApplicantJobPrefOccupation.AddRange(data.ApplicantJobPrefOccupation);
                    db.ApplicantJobPrefWorkLocation.AddRange(data.ApplicantJobPrefWorkLocation);
                    db.ApplicantLanguageDialectProf.AddRange(data.ApplicantLanguageDialectProf);
                    db.ApplicantOtherSkills.AddRange(data.ApplicantOtherSkills);
                    db.ApplicantProfessionalLicense.AddRange(data.ApplicantProfessionalLicense);
                    db.ApplicantTechnicalVocational.AddRange(data.ApplicantTechnicalVocational);
                    db.ApplicantWorkExperience.AddRange(data.ApplicantWorkExperience);
                    db.SaveChanges();
                    transaction.Commit();
                    return Ok(userAccount.Id);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Ok(ex.Message);
                }
            }
            return NotFound();
        }

        [HttpGet]
        [Route("ApplicantDataViewModel/{accountId}")]
        public ApplicantDataViewModel ApplicantDataViewModel(string accountId)
        {
            using var db = dbFactory.CreateDbContext();
            var rs = cache.Get<ApplicantDataViewModel>($"ApplicantDataViewModel/{accountId}");
            if (rs == null)
            {
                rs = new ApplicantDataViewModel();
                var account = db.ApplicantAccount.Where(x => x.Id == accountId).FirstOrDefault();
                if (account != null)
                {
                    var information = db.ApplicantInformation.Where(x => x.AccountId == account.Id).FirstOrDefault();
                    var educationbackgrond = db.ApplicantEducationalBackground.Where(x => x.AccountId == account.Id).ToList();
                    var eligibility = db.ApplicantEligibility.Where(x => x.AccountId == account.Id).ToList();
                    var expectedsalary = db.ApplicantExpectedSalary.Where(x => x.AccountId == account.Id).FirstOrDefault();
                    var jobprefoccupation = db.ApplicantJobPrefOccupation.Where(x => x.AccountId == account.Id).ToList();
                    var jobprefworklocation = db.ApplicantJobPrefWorkLocation.Where(x => x.AccountId == account.Id).ToList();
                    var languagedialect = db.ApplicantLanguageDialectProf.Where(x => x.AccountId == account.Id).ToList();
                    var otherskills = db.ApplicantOtherSkills.Where(x => x.AccountId == account.Id).ToList();
                    var preflicense = db.ApplicantProfessionalLicense.Where(x => x.AccountId == account.Id).ToList();
                    var techinalvocational = db.ApplicantTechnicalVocational.Where(x => x.AccountId == account.Id).ToList();
                    var workexp = db.ApplicantWorkExperience.Where(x => x.AccountId == account.Id).ToList();

                    rs = new ApplicantDataViewModel()
                    {
                        ApplicantAccount = account,
                        ApplicantInformation = information,
                        ApplicantEducationalBackground = educationbackgrond,
                        ApplicantEligibility = eligibility,
                        ApplicantExpectedSalary = expectedsalary,
                        ApplicantJobPrefOccupation = jobprefoccupation,
                        ApplicantJobPrefWorkLocation = jobprefworklocation,
                        ApplicantLanguageDialectProf = languagedialect,
                        ApplicantOtherSkills = otherskills,
                        ApplicantProfessionalLicense = preflicense,
                        ApplicantTechnicalVocational = techinalvocational,
                        ApplicantWorkExperience = workexp
                    };
                }
                cache.Set($"ApplicantDataViewModel/{accountId}", rs, TimeSpan.FromSeconds(30));
            }
            return rs;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("VerifyApplicant/{accountId}")]
        public int VerifyApplicant(string accountId)
        {
            using var db = dbFactory.CreateDbContext();
            var account = db.ApplicantAccount.Where(x => x.Id == accountId).FirstOrDefault();
            var rs = 0;
            if (account != null)
            {
                account.IsReviewedReturned = 1;
                account.DateLastUpdate = Helper.currentTimeMillis();
                db.ApplicantAccount.Update(account);
                db.SaveChanges();
                rs = 1;
            }
            return rs;
        }

        [HttpGet]
        [Route("GetNotificationCount")]
        public int GetNotificationCount()
        {
            using var db = dbFactory.CreateDbContext();
            return db.Notifications.Where(x => x.IsRead == 0).Count();
        }
        [HttpGet("GetApplicantInformation/{applicantid}")]
        public ApplicantInformation GetApplicantInformation(string applicantid)
        {
            using var db = dbFactory.CreateDbContext();
            var information = db.ApplicantInformation.Where(x => x.AccountId == applicantid).FirstOrDefault();

            if (information != null)
            {
                // FOR JOB FAIR ONLY
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
            return information;
        }

        [HttpPost]
        [Route("ApplicantEducationalBackground/{IsUpdate}/{AccountId}")]
        public List<ApplicantEducationalBackground> ApplicantEducationalBackground(bool IsUpdate, string AccountId, List<ApplicantEducationalBackground> data)
        {
            using var db = dbFactory.CreateDbContext();
            if (!IsUpdate)
            {
                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantEducationalBackground.Add(i);
                }
                db.SaveChanges();
            }
            else
            {
                var listforremove = db.ApplicantEducationalBackground.Where(x => x.AccountId == AccountId).ToList();
                db.RemoveRange(listforremove);
                db.SaveChanges();
                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantEducationalBackground.Add(i);
                }
                db.SaveChanges();
            }
            return data;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ApplicantEligibility/{IsUpdate}/{AccountId}")]
        public List<ApplicantEligibility> ApplicantEligibility(bool IsUpdate, string AccountId, List<ApplicantEligibility> data)
        {
            using var db = dbFactory.CreateDbContext();
            if (!IsUpdate)
            {
                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantEligibility.Add(i);
                }
                db.SaveChanges();
            }
            else
            {
                var listforremove = db.ApplicantEligibility.Where(x => x.AccountId == AccountId).ToList();
                db.RemoveRange(listforremove);
                db.SaveChanges();

                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantEligibility.Add(i);
                }
                db.SaveChanges();
            }
            return data;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ApplicantExpectedSalary/{IsUpdate}/{AccountId}")]
        public ApplicantExpectedSalary ApplicantExpectedSalary(bool IsUpdate, string AccountId, ApplicantExpectedSalary data)
        {
            using var db = dbFactory.CreateDbContext();
            if (!IsUpdate)
            {
                data.AccountId = AccountId;
                data.DateLastUpdate = Helper.currentTimeMillis();
                db.ApplicantExpectedSalary.Add(data);
                db.SaveChanges();
            }
            else
            {
                data.AccountId = AccountId;
                data.DateLastUpdate = Helper.currentTimeMillis();
                db.ApplicantExpectedSalary.Update(data);
                db.SaveChanges();
            }
            return data;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("ApplicantInformation/{IsUpdate}/{AccountId}")]
        public ApplicantInformation ApplicantInformation(bool IsUpdate, string AccountId, ApplicantInformation data)
        {
            using var db = dbFactory.CreateDbContext();
            if (!IsUpdate)
            {
                data.AccountId = AccountId;
                data.DateLastUpdate = Helper.currentTimeMillis();
                db.ApplicantInformation.Add(data);
                db.SaveChanges();
            }
            else
            {
                data.AccountId = AccountId;
                data.DateLastUpdate = Helper.currentTimeMillis();
                db.ApplicantInformation.Update(data);
                db.SaveChanges();
            }
            var userAccount = db.UserAccounts.Where(x => x.Email == data.Email).FirstOrDefault();
            if (userAccount != null)
            {
                userAccount.GivenName = data.FirstName;
                userAccount.Name = $"{data.FirstName} {data.SurName}";
                db.UserAccounts.Update(userAccount);
                db.SaveChanges();
            }

            return data;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("ApplicantJobPrefOccupation/{IsUpdate}/{AccountId}")]
        public List<ApplicantJobPrefOccupation> ApplicantJobPrefOccupation(bool IsUpdate, string AccountId, List<ApplicantJobPrefOccupation> data)
        {
            using var db = dbFactory.CreateDbContext();
            if (!IsUpdate)
            {
                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantJobPrefOccupation.Add(i);
                }
                db.SaveChanges();
            }
            else
            {
                var listforremove = db.ApplicantJobPrefOccupation.Where(x => x.AccountId == AccountId).ToList();
                db.RemoveRange(listforremove);
                db.SaveChanges();

                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantJobPrefOccupation.Add(i);
                }
                db.SaveChanges();
            }
            return data;
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("ApplicantJobPrefWorkLocation/{IsUpdate}/{AccountId}")]
        public List<ApplicantJobPrefWorkLocation> ApplicantJobPrefWorkLocation(bool IsUpdate, string AccountId, List<ApplicantJobPrefWorkLocation> data)
        {
            using var db = dbFactory.CreateDbContext();
            if (!IsUpdate)
            {
                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantJobPrefWorkLocation.Add(i);
                }
                db.SaveChanges();
            }
            else
            {
                var listforremove = db.ApplicantJobPrefWorkLocation.Where(x => x.AccountId == AccountId).ToList();
                db.RemoveRange(listforremove);
                db.SaveChanges();

                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantJobPrefWorkLocation.Add(i);
                }
                db.SaveChanges();
            }
            return data;
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("ApplicantLanguageDialectProf/{IsUpdate}/{AccountId}")]
        public List<ApplicantLanguageDialectProf> ApplicantLanguageDialectProf(bool IsUpdate, string AccountId, List<ApplicantLanguageDialectProf> data)
        {
            using var db = dbFactory.CreateDbContext();
            if (!IsUpdate)
            {
                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantLanguageDialectProf.Add(i);
                }
                db.SaveChanges();
            }
            else
            {
                var listforremove = db.ApplicantLanguageDialectProf.Where(x => x.AccountId == AccountId).ToList();
                db.RemoveRange(listforremove);
                db.SaveChanges();

                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantLanguageDialectProf.Add(i);
                }
                db.SaveChanges();
            }
            return data;
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("ApplicantOtherSkills/{IsUpdate}/{AccountId}")]
        public List<ApplicantOtherSkills> ApplicantOtherSkills(bool IsUpdate, string AccountId, List<ApplicantOtherSkills> data)
        {
            using var db = dbFactory.CreateDbContext();
            if (!IsUpdate)
            {
                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantOtherSkills.Add(i);
                }
                db.SaveChanges();
            }
            else
            {
                var listforremove = db.ApplicantOtherSkills.Where(x => x.AccountId == AccountId).ToList();
                db.RemoveRange(listforremove);
                db.SaveChanges();

                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantOtherSkills.Add(i);
                }
                db.SaveChanges();
            }
            return data;
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("ApplicantProfessionalLicense/{IsUpdate}/{AccountId}")]
        public List<ApplicantProfessionalLicense> ApplicantProfessionalLicense(bool IsUpdate, string AccountId, List<ApplicantProfessionalLicense> data)
        {
            using var db = dbFactory.CreateDbContext();
            if (!IsUpdate)
            {
                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantProfessionalLicense.Add(i);
                }
                db.SaveChanges();
            }
            else
            {
                var listforremove = db.ApplicantProfessionalLicense.Where(x => x.AccountId == AccountId).ToList();
                db.RemoveRange(listforremove);
                db.SaveChanges();

                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantProfessionalLicense.Add(i);
                }
                db.SaveChanges();
            }
            return data;
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("ApplicantTechnicalVocational/{IsUpdate}/{AccountId}")]
        public List<ApplicantTechnicalVocational> ApplicantTechnicalVocational(bool IsUpdate, string AccountId, List<ApplicantTechnicalVocational> data)
        {
            using var db = dbFactory.CreateDbContext();
            if (!IsUpdate)
            {
                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantTechnicalVocational.Add(i);
                }
                db.SaveChanges();
            }
            else
            {
                var listforremove = db.ApplicantTechnicalVocational.Where(x => x.AccountId == AccountId).ToList();
                db.RemoveRange(listforremove);
                db.SaveChanges();

                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantTechnicalVocational.Add(i);
                }
                db.SaveChanges();
            }
            return data;
        }
        [HttpPost]
        [AllowAnonymous]
        [Route("ApplicantWorkExperience/{IsUpdate}/{AccountId}")]
        public List<ApplicantWorkExperience> ApplicantWorkExperience(bool IsUpdate, string AccountId, List<ApplicantWorkExperience> data)
        {
            using var db = dbFactory.CreateDbContext();
            if (!IsUpdate)
            {
                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantWorkExperience.Add(i);
                }
                db.SaveChanges();
            }
            else
            {
                var listforremove = db.ApplicantWorkExperience.Where(x => x.AccountId == AccountId).ToList();
                db.RemoveRange(listforremove);
                db.SaveChanges();

                foreach (var i in data)
                {
                    i.AccountId = AccountId;
                    i.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantWorkExperience.Add(i);
                }
                db.SaveChanges();
            }
            return data;
        }

        [HttpPost]
        [Route("SaveApplicantAccount")]
        public ApplicantAccount SaveApplicantAccount(ApplicantAccount account)
        {
            using var db = dbFactory.CreateDbContext();
            var userAccount = db.UserAccounts.Where(x => x.Email == account.Email).FirstOrDefault();
            if (userAccount == null)
            {
                string base64Guid;
                do
                {
                    base64Guid = Guid.NewGuid().ToString();
                } while (db.UserAccounts.Any(x => x.Id == base64Guid));

                userAccount = new UserAccount
                {
                    Id = base64Guid,
                    Email = account.Email,
                    Password = Helper.RandomString(6).ToLower(),
                    DateCreated = DateTime.Now,
                    UserType = ProjectConfig.USER_TYPE.APPLICANT
                };

                db.UserAccounts.Add(userAccount);
                db.SaveChanges();  
            }

            var IsExist = db.ApplicantAccount.Any(x => x.Email.ToLower() == account.Email.ToLower());
            if (!IsExist)
            {
                account.Id = userAccount.Id;
                account.DateRegistered = Helper.currentTimeMillis();
                account.DateLastUpdate = Helper.currentTimeMillis();
                db.ApplicantAccount.Add(account);
                db.SaveChanges();
            }
            else
            {
                db.Entry(account).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                db.SaveChanges();

                account.DateLastUpdate = Helper.currentTimeMillis();
                account.Id = userAccount.Id;
                db.ApplicantAccount.Update(account);
                db.SaveChanges();
            }
            /*var tblHired = db.EmployerHiredApplicants.Where(x => x.ApplicantAccountId == account.Id).FirstOrDefault();
            if (tblHired != null)
            {
                db.EmployerHiredApplicants.Remove(tblHired);
                db.SaveChanges();
            }
            var tblInterviewed = db.EmployerInterviewedApplicants.Where(x => x.ApplicantAccountId == account.Id).FirstOrDefault();
            if (tblInterviewed != null)
            {
                db.EmployerInterviewedApplicants.Remove(tblInterviewed);
                db.SaveChanges();
            }
            var tblScheduled = db.EmployerScheduledInterviews.Where(x => x.ApplicantId == account.Id).FirstOrDefault();
            if (tblScheduled != null)
            {
                db.EmployerScheduledInterviews.Remove(tblScheduled);
                db.SaveChanges();
            }*/

            return account;
        }

        [HttpPost]
        [Route("SaveApplicantData/{IsUpdate}/{AccountId}")]
        public IActionResult SaveApplicantData(bool IsUpdate, string AccountId, ApplicantDataViewModel data)
        {
            using var db = dbFactory.CreateDbContext();

            if (!IsUpdate)
            {
                // Save Applicant Information
                data.ApplicantInformation.AccountId = AccountId;
                data.ApplicantInformation.DateLastUpdate = Helper.currentTimeMillis();
                db.ApplicantInformation.Add(data.ApplicantInformation);

                // Save Applicant Expected Salary
                data.ApplicantExpectedSalary.AccountId = AccountId;
                data.ApplicantExpectedSalary.DateLastUpdate = Helper.currentTimeMillis();
                db.ApplicantExpectedSalary.Add(data.ApplicantExpectedSalary);

                // Save Applicant Educational Background
                foreach (var item in data.ApplicantEducationalBackground)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantEducationalBackground.Add(item);
                }

                // Save Applicant Eligibility
                foreach (var item in data.ApplicantEligibility)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantEligibility.Add(item);
                }

                // Save Applicant Job Preference Occupation
                foreach (var item in data.ApplicantJobPrefOccupation)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantJobPrefOccupation.Add(item);
                }

                // Save Applicant Job Preference Work Location
                foreach (var item in data.ApplicantJobPrefWorkLocation)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantJobPrefWorkLocation.Add(item);
                }

                // Save Applicant Language Dialect Proficiency
                foreach (var item in data.ApplicantLanguageDialectProf)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantLanguageDialectProf.Add(item);
                }

                // Save Applicant Other Skills
                foreach (var item in data.ApplicantOtherSkills)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantOtherSkills.Add(item);
                }

                // Save Applicant Professional License
                foreach (var item in data.ApplicantProfessionalLicense)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantProfessionalLicense.Add(item);
                }

                // Save Applicant Technical Vocational
                foreach (var item in data.ApplicantTechnicalVocational)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantTechnicalVocational.Add(item);
                }

                // Save Applicant Work Experience
                foreach (var item in data.ApplicantWorkExperience)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantWorkExperience.Add(item);
                }
            }
            else
            {
                //// Update Applicant Account
                //data.ApplicantAccount.Id = AccountId;
                //data.ApplicantAccount.DateLastUpdate = Helper.currentTimeMillis();
                //db.ApplicantAccount.Update(data.ApplicantAccount);

                // Update Applicant Information
                data.ApplicantInformation.AccountId = AccountId;
                var existingInformation = db.ApplicantInformation.AsNoTracking().FirstOrDefault(x => x.AccountId == AccountId);
                if (existingInformation != null)
                {
                    data.ApplicantInformation.Email = existingInformation.Email; // Preserve the email
                }
                data.ApplicantInformation.DateLastUpdate = Helper.currentTimeMillis();
                db.ApplicantInformation.Update(data.ApplicantInformation);

                // Update Applicant Expected Salary
                data.ApplicantExpectedSalary.AccountId = AccountId;
                data.ApplicantExpectedSalary.DateLastUpdate = Helper.currentTimeMillis();
                db.ApplicantExpectedSalary.Update(data.ApplicantExpectedSalary);

                // Update Applicant Educational Background
                var existingEducationalBackground = db.ApplicantEducationalBackground.Where(x => x.AccountId == AccountId).ToList();
                db.ApplicantEducationalBackground.RemoveRange(existingEducationalBackground);
                foreach (var item in data.ApplicantEducationalBackground)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantEducationalBackground.Add(item);
                }

                // Update Applicant Eligibility
                var existingEligibility = db.ApplicantEligibility.Where(x => x.AccountId == AccountId).ToList();
                db.ApplicantEligibility.RemoveRange(existingEligibility);
                foreach (var item in data.ApplicantEligibility)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantEligibility.Add(item);
                }

                // Update Applicant Job Preference Occupation
                var existingJobPrefOccupation = db.ApplicantJobPrefOccupation.Where(x => x.AccountId == AccountId).ToList();
                db.ApplicantJobPrefOccupation.RemoveRange(existingJobPrefOccupation);
                foreach (var item in data.ApplicantJobPrefOccupation)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantJobPrefOccupation.Add(item);
                }

                // Update Applicant Job Preference Work Location
                var existingJobPrefWorkLocation = db.ApplicantJobPrefWorkLocation.Where(x => x.AccountId == AccountId).ToList();
                db.ApplicantJobPrefWorkLocation.RemoveRange(existingJobPrefWorkLocation);
                foreach (var item in data.ApplicantJobPrefWorkLocation)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantJobPrefWorkLocation.Add(item);
                }

                // Update Applicant Language Dialect Proficiency
                var existingLanguageDialectProf = db.ApplicantLanguageDialectProf.Where(x => x.AccountId == AccountId).ToList();
                db.ApplicantLanguageDialectProf.RemoveRange(existingLanguageDialectProf);
                foreach (var item in data.ApplicantLanguageDialectProf)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantLanguageDialectProf.Add(item);
                }

                // Update Applicant Other Skills
                var existingOtherSkills = db.ApplicantOtherSkills.Where(x => x.AccountId == AccountId).ToList();
                db.ApplicantOtherSkills.RemoveRange(existingOtherSkills);
                foreach (var item in data.ApplicantOtherSkills)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantOtherSkills.Add(item);
                }

                // Update Applicant Professional License
                var existingProfessionalLicense = db.ApplicantProfessionalLicense.Where(x => x.AccountId == AccountId).ToList();
                db.ApplicantProfessionalLicense.RemoveRange(existingProfessionalLicense);
                foreach (var item in data.ApplicantProfessionalLicense)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantProfessionalLicense.Add(item);
                }

                // Update Applicant Technical Vocational
                var existingTechnicalVocational = db.ApplicantTechnicalVocational.Where(x => x.AccountId == AccountId).ToList();
                db.ApplicantTechnicalVocational.RemoveRange(existingTechnicalVocational);
                foreach (var item in data.ApplicantTechnicalVocational)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantTechnicalVocational.Add(item);
                }

                // Update Applicant Work Experience
                var existingWorkExperience = db.ApplicantWorkExperience.Where(x => x.AccountId == AccountId).ToList();
                db.ApplicantWorkExperience.RemoveRange(existingWorkExperience);
                foreach (var item in data.ApplicantWorkExperience)
                {
                    item.AccountId = AccountId;
                    item.DateLastUpdate = Helper.currentTimeMillis();
                    db.ApplicantWorkExperience.Add(item);
                }
            }

            db.SaveChanges();
            return Ok(data);
        }

        [HttpPost]
        [Route("GenerateId")]
        public IActionResult GenerateId()
        {
            using var db = dbFactory.CreateDbContext();
            string base64Guid;
            do
            {
                base64Guid = Guid.NewGuid().ToString();
            } while (db.UserAccounts.Any(x => x.Id == base64Guid));

            return Ok(base64Guid);
        }

        [HttpPost("SaveJobApplication")]
        public IActionResult SaveJobApplication(JobApplicantion data)
        {
            using var db = dbFactory.CreateDbContext();
            data.Id = Guid.NewGuid().ToString().ToOwnGUID();
            data.DateCreated = DateTime.Now;
            db.JobApplicantion.Add(data);
            var rs = db.SaveChanges();
            if (rs > 0)
            {
                return Ok(data);
            }
            return BadRequest("Failed to save job application.");
        }

        [HttpPost("UploadApplicantResume")]
        public IActionResult UploadApplicantResume()
        {
            var files = Request.Form.Files;
            var result = new List<AttachementsViewModel>();
            var folderName = Request.Headers.Where(x => x.Key == "f").Select(x => x.Value).FirstOrDefault().ToString() ?? "";

            var dir = Path.Combine(env.ContentRootPath, "files", "applications", folderName);

            // ✅ Only create if not exists
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using var db = dbFactory.CreateDbContext();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var origFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fileName = WebUtility.HtmlEncode(origFileName);
                    var fullPath = Path.Combine(dir, fileName);

                    // ✅ Save (overwrite if exists)
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    // ✅ Check if record already exists in DB
                    var existing = db.JobApplicantionAttachment
                        .FirstOrDefault(x => x.JobApplicantionId == folderName && x.FileName == fileName);

                    if (existing == null)
                    {
                        // Insert new record
                        var attachment = new JobApplicantionAttachment
                        {
                            FileName = fileName,
                            JobApplicantionId = folderName
                        };
                        db.JobApplicantionAttachment.Add(attachment);
                    }
                    else
                    {
                        // Optionally update existing record (e.g., timestamp if you have it)
                        existing.FileName = fileName;
                    }

                    result.Add(new AttachementsViewModel()
                    {
                        FileName = fileName,
                        FileSize = Helper.SizeSuffix(file.Length),
                        FolderName = folderName
                    });
                }
            }

            db.SaveChanges();
            return Ok(result);
        }


        [HttpGet("CheckIfAlreadyApplied/{applicantId}/{jobpostId}")]
        public IActionResult CheckIfAlreadyApplied(string applicantId, string jobpostId)
        {
            using var db = dbFactory.CreateDbContext();
            var rs = new StatusViewModel();
            rs.IsExist = db.JobApplicantion.Any(x => x.ApplicantId == applicantId && x.JobPostId == jobpostId);
            return Ok(rs);
        }

        [HttpDelete("DeleteApplicantAttachment/{fileName}")]
        public IActionResult DeleteApplicantAttachment(string fileName)
        {
            var dir = System.IO.Path.Combine(env.ContentRootPath, "applications");
            var fullPath = System.IO.Path.Combine(dir, fileName);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);

                using var db = dbFactory.CreateDbContext();
                var attachment = db.JobApplicantionAttachment.FirstOrDefault(a => a.FileName == fileName);
                if (attachment != null)
                {
                    db.JobApplicantionAttachment.Remove(attachment);
                    db.SaveChanges();
                }

                return Ok(new { Message = "Attachment deleted successfully." });
            }

            return NotFound(new { Message = "Attachment not found." });
        }

        [HttpPost("SendUserReplyEmail")]
        public async Task<IActionResult> SendUserReplyEmail(EmailViewModel data)
        {
            await gmail.SendEmail(data.Email, data.MessageBody);
            return Ok(data);
        }

        [HttpPost("ChangeUserPassword")]
        public IActionResult ChangeUserPassword(ChangePasswordViewModel data)
        {
            using var db = dbFactory.CreateDbContext();
            var user = db.UserAccounts.Where(x => x.Id == data.Id).FirstOrDefault();
            if (user != null)
            {
                // Validate the current password
                if (user.Password != data.CurrentPassword)
                {
                    return BadRequest("The current password is incorrect.");
                }

                // Update the password
                user.Password = data.NewPassword;
                db.UserAccounts.Update(user);
                db.SaveChanges();
                return Ok(true);
            }
            return Ok(false);
        }

    }
}
