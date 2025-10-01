using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using webapi_peso.Model;
using webapi_peso.ViewModels;

namespace webapi_peso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class EmployerController : ControllerBase
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;
        private readonly IMemoryCache cache;
        private readonly IWebHostEnvironment env;

        public EmployerController(IDbContextFactory<ApplicationDbContext> _dbFactory, IMemoryCache _cache, IWebHostEnvironment _env)
        {
            dbFactory = _dbFactory;
            cache = _cache;
            env = _env;
        }

        //[HttpPost("GetApplicantsAppliedToJob")]
        //public IActionResult GetApplicantsAppliedToJob(RelatedJobViewModel param)
        //{
        //    using (var db = dbFactory.CreateDbContext())
        //    {
        //        var list = cache.Get<List<AppliedApplicantViewModel>>($"GetApplicantsAppliedToJob/{param.EmpId}/{param.JobPostId}");
        //        if (list == null)
        //        {
        //            list = new List<AppliedApplicantViewModel>();
        //            var jobPosts = db.JobApplicantion.Where(x => x.JobPostId == param.JobPostId).OrderByDescending(x => x.DateCreated).MyDistinctBy(x => x.ApplicantId).ToList();
        //            foreach (var i in jobPosts)
        //            {
        //                var model = new AppliedApplicantViewModel();
        //                var empInfo = db.ApplicantInformation.Where(x => x.AccountId == i.ApplicantId).FirstOrDefault();
        //                model.Applicant = empInfo;
        //                var dir = System.IO.Path.Combine(env.ContentRootPath, "files", "applications", i.Id);
        //                var files = Directory.GetFiles(dir, "*.pdf");
        //                var fileName = WebUtility.HtmlEncode(files[0]);
        //                var fileInfo = new FileInfo(fileName);
        //                model.FilePath = $"{ProjectConfig.API_HOST}/files/applications/{i.Id}/{fileInfo.Name}";
        //                model.DateApplied = i.DateCreated;
        //                model.IsInterviewed = db.EmployerInterviewedApplicants.Any(x => x.EmployerId == param.EmpId && x.ApplicantAccountId == i.ApplicantId);
        //                model.IsHired = db.EmployerHiredApplicants.Any(x => x.EmployerId == param.EmpId && x.ApplicantAccountId == i.ApplicantId);
        //                list.Add(model);
        //            }
        //            cache.Set($"GetApplicantsAppliedToJob/{param.EmpId}/{param.JobPostId}", list, TimeSpan.FromSeconds(30));
        //        }
        //        return Ok(list);
        //    }
        //}

        [HttpGet("GetApplicantsByJobPost/{jobPostId}")]
        public IActionResult GetApplicantsByJobPost(string jobPostId)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var list = cache.Get<List<AppliedApplicantViewModel>>($"GetApplicantsAppliedToJob/{jobPostId}");
                if (list == null)
                {
                    list = new List<AppliedApplicantViewModel>();
                    var jobPosts = db.JobApplicantion
                        .Where(x => x.JobPostId == jobPostId)
                        .OrderByDescending(x => x.DateCreated)
                        .MyDistinctBy(x => x.ApplicantId)
                        .ToList();

                    list.AddRange(jobPosts.Select(i => new AppliedApplicantViewModel
                    {
                        Applicant = db.ApplicantInformation.FirstOrDefault(x => x.AccountId == i.ApplicantId),
                        DateApplied = i.DateCreated,
                        IsInterviewed = db.EmployerInterviewedApplicants.Any(x =>
                            x.EmployerId == db.EmployerJobPost.FirstOrDefault(e => e.Id == jobPostId).EmployerDetailsId &&
                            x.ApplicantAccountId == i.ApplicantId),
                        IsHired = db.EmployerHiredApplicants.Any(x =>
                            x.EmployerId == db.EmployerJobPost.FirstOrDefault(e => e.Id == jobPostId).EmployerDetailsId &&
                            x.ApplicantAccountId == i.ApplicantId),

                        // ✅ Attachments linked to JobApplicationId
                        Attachments = db.JobApplicantionAttachment
                        .Where(att => att.JobApplicantionId == i.Id) // same Id as application
                        .Select(att => new AttachementsViewModel
                        {
                            FileName = att.FileName,
                            FolderName = i.Id
                        }).ToList()
                    }));

                    cache.Set($"GetApplicantsAppliedToJob/{jobPostId}", list, TimeSpan.FromSeconds(30));
                }
                return Ok(list);
            }
        }


        [HttpGet("GetApplicantsByEmployer/{employerDetailsId}")]
        public IActionResult GetApplicantsByEmployer(string employerDetailsId)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var list = cache.Get<List<ApplicantsPerJobPostModel>>($"GetApplicantsByEmployer/{employerDetailsId}");
                if (list == null)
                {
                    var employerDetails = db.EmployerDetails
                        .FirstOrDefault(ed => ed.ContactEmailAddress == db.UserAccounts
                            .Where(ua => ua.Id == employerDetailsId)
                            .Select(ua => ua.Email)
                            .FirstOrDefault());

                    if (employerDetails == null)
                    {
                        return NotFound("Employer details not found.");
                    }

                    var jobPostIds = db.EmployerJobPost
                        .Where(ejp => ejp.EmployerDetailsId == employerDetails.Id)
                        .Select(ejp => ejp.Id)
                        .ToList();

                    list = jobPostIds.Select(jobPostId => new ApplicantsPerJobPostModel
                    {
                        Id = jobPostId,
                        Description = db.EmployerJobPost
                            .Where(ejp => ejp.Id == jobPostId)
                            .Select(ejp => ejp.Description)
                            .FirstOrDefault() ?? string.Empty,
                        Applicants = db.JobApplicantion
                            .Where(ja => ja.JobPostId == jobPostId)
                            .OrderByDescending(ja => ja.DateCreated)
                            .Select(applicant => new AppliedApplicantViewModel
                            {
                                Applicant = db.ApplicantInformation
                                    .FirstOrDefault(ai => ai.AccountId == applicant.ApplicantId) ?? new ApplicantInformation(),
                                DateApplied = applicant.DateCreated,
                                IsInterviewed = db.EmployerInterviewedApplicants
                                    .Any(eia => eia.EmployerId == employerDetails.Id && eia.ApplicantAccountId == applicant.ApplicantId),
                                IsHired = db.EmployerHiredApplicants
                                    .Any(eha => eha.EmployerId == employerDetails.Id && eha.ApplicantAccountId == applicant.ApplicantId)
                            })
                            .ToList()
                    }).ToList();

                    cache.Set($"GetApplicantsByEmployer/{employerDetailsId}", list, TimeSpan.FromSeconds(30));
                }
                return Ok(list);
            }
        }

        [HttpGet("GetJobDescription/{Id}")]
        public IActionResult GetJobDescription(string Id)
        {
            using var db = dbFactory.CreateDbContext();
            var data = cache.Get<JobDescriptionViewModel>($"GetJobDescription/{Id}");
            if (data == null)
            {
                data = new JobDescriptionViewModel();
                var rs = db.EmployerJobPost.Where(x => x.Id == Id).FirstOrDefault();
                if (rs != null)
                {
                    data = new JobDescriptionViewModel();
                    data.EmployerJobPost = rs;
                    //var dir = System.IO.Path.Combine(env.WebRootPath, "files", "job_post_image", Id);
                    //if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                    //if (System.IO.Directory.GetFiles(dir).Length > 0)
                    //{
                    //    var filename = System.IO.Directory.GetFiles(dir)[0];
                    //    data.ImageLinkPath = $"files/job_post_image/{Id}/{System.IO.Path.GetFileName(filename)}";
                    //}
                    cache.Set($"GetJobDescription/{Id}", data, TimeSpan.FromSeconds(30));
                }
            }

            return Ok(data);
        }

        [HttpGet]
        [AllowAnonymous]
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

        [HttpGet("GetNearbyApplicantsV2")]
        public IActionResult GetNearbyApplicantsV2b([FromQuery] SearchApplicantsViewModel search)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                string Id = search.EmployerId;
                string gender = search.Gender;
                int count = search.Count;
                int startIndex = search.StartIndex;

                var emDetails = db.EmployerDetails.Find(Id);
                var mainList = db.ApplicantInformation.Where(x =>
                    !db.EmployerHiredApplicants.Any(b => b.EmployerId == Id && b.ApplicantAccountId == x.AccountId) &&
                    !db.EmployerInterviewedApplicants.Any(b => b.EmployerId == Id && b.ApplicantAccountId == x.AccountId) &&
                    !db.EmployerScheduledInterviews.Any(b => b.EmployerId == Id && b.ApplicantId == x.AccountId) &&
                    db.ApplicantAccount.Any(a => a.Id == x.AccountId && a.IsReviewedReturned == 1 && a.IsRemoved == 0))
                    .OrderByDescending(x => x.DateLastUpdate)
                    .MyDistinctBy(x => x.Email)
                    .OrderBy(x => x.SurName)
                    .ToList();

                if (!string.IsNullOrEmpty(search.BarangayCode))
                {
                    mainList = mainList.Where(x => x.PresentBarangay == search.BarangayCode).ToList();
                }
                if (!string.IsNullOrEmpty(search.CityCode))
                {
                    mainList = mainList.Where(x => x.PresentMunicipalityCity == search.CityCode).ToList();
                }
                if (!string.IsNullOrEmpty(search.ProvinceCode))
                {
                    mainList = mainList.Where(x => x.PresentProvince == search.ProvinceCode).ToList();
                }
                if (string.IsNullOrEmpty(search.BarangayCode) && string.IsNullOrEmpty(search.CityCode) && string.IsNullOrEmpty(search.ProvinceCode))
                {
                    mainList = mainList.Where(x =>
                        x.PresentBarangay == emDetails.Barangay ||
                        x.PresentMunicipalityCity == emDetails.CityMunicipality ||
                        x.PresentProvince == emDetails.Province).ToList();
                }

                if (gender != "ANY")
                {
                    mainList = mainList.Where(x => x.Gender.ToUpper() == gender.ToUpper()).ToList();
                }

                var totalCount = mainList.Count();
                var numCardDeets = Math.Min(count, totalCount - startIndex);

                var list = mainList.Skip(startIndex).Take(numCardDeets).ToList();

                var results = list.Count == 0
                    ? db.ApplicantInformation.Where(x => db.ApplicantAccount.Any(a => a.IsReviewedReturned == 1 && a.IsRemoved == 0 && x.AccountId == a.Id))
                        .OrderByDescending(x => x.DateLastUpdate)
                        .MyDistinctBy(x => x.Email)
                        .Skip(startIndex)
                        .Take(numCardDeets)
                        .ToList()
                    : list.MyDistinctBy(x => x.Email).ToList();

                var _myRs = new VirtualizedDatViewModel
                {
                    Items = results,
                    TotalCount = totalCount
                };

                return Ok(_myRs);
            }
        }

        [HttpPost("GetNearbyApplicantsV2")]
        public IActionResult GetNearbyApplicantsV2(SearchApplicantsViewModel search)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                string Id = search.EmployerId;
                string gender = search.Gender;
                int count = search.Count;
                int startIndex = search.StartIndex;

                var emDetails = db.EmployerDetails.Find(Id);
                var mainList = db.ApplicantInformation.Where(x =>
                    !db.EmployerHiredApplicants.Any(b => b.EmployerId == Id && b.ApplicantAccountId == x.AccountId) &&
                    !db.EmployerInterviewedApplicants.Any(b => b.EmployerId == Id && b.ApplicantAccountId == x.AccountId) &&
                    !db.EmployerScheduledInterviews.Any(b => b.EmployerId == Id && b.ApplicantId == x.AccountId) &&
                    db.ApplicantAccount.Any(a => a.Id == x.AccountId && a.IsReviewedReturned == 1 && a.IsRemoved == 0)).OrderByDescending(x => x.DateLastUpdate).MyDistinctBy(x => x.Email).OrderBy(x => x.SurName).ToList();
                if (!string.IsNullOrEmpty(search.BarangayCode))
                {
                    mainList = db.ApplicantInformation.Where(x =>
                        !db.EmployerHiredApplicants.Any(b => b.EmployerId == Id && b.ApplicantAccountId == x.AccountId) &&
                        !db.EmployerInterviewedApplicants.Any(b => b.EmployerId == Id && b.ApplicantAccountId == x.AccountId) &&
                        !db.EmployerScheduledInterviews.Any(b => b.EmployerId == Id && b.ApplicantId == x.AccountId) &&
                        db.ApplicantAccount.Any(a => a.Id == x.AccountId && a.IsReviewedReturned == 1 && a.IsRemoved == 0)).OrderByDescending(x => x.DateLastUpdate).MyDistinctBy(x => x.Email).OrderBy(x => x.SurName).ToList();
                    mainList = mainList.Where(x => x.PresentBarangay == search.BarangayCode).ToList();
                }
                if (!string.IsNullOrEmpty(search.CityCode))
                {
                    mainList = db.ApplicantInformation.Where(x =>
                        !db.EmployerHiredApplicants.Any(b => b.EmployerId == Id && b.ApplicantAccountId == x.AccountId) &&
                        !db.EmployerInterviewedApplicants.Any(b => b.EmployerId == Id && b.ApplicantAccountId == x.AccountId) &&
                        !db.EmployerScheduledInterviews.Any(b => b.EmployerId == Id && b.ApplicantId == x.AccountId) &&
                        db.ApplicantAccount.Any(a => a.Id == x.AccountId && a.IsReviewedReturned == 1 && a.IsRemoved == 0)).OrderByDescending(x => x.DateLastUpdate).MyDistinctBy(x => x.Email).OrderBy(x => x.SurName).ToList();
                    mainList = mainList.Where(x => x.PresentMunicipalityCity == search.CityCode).ToList();
                }
                if (!string.IsNullOrEmpty(search.ProvinceCode))
                {
                    mainList = db.ApplicantInformation.Where(x =>
                        !db.EmployerHiredApplicants.Any(b => b.EmployerId == Id && b.ApplicantAccountId == x.AccountId) &&
                        !db.EmployerInterviewedApplicants.Any(b => b.EmployerId == Id && b.ApplicantAccountId == x.AccountId) &&
                        !db.EmployerScheduledInterviews.Any(b => b.EmployerId == Id && b.ApplicantId == x.AccountId) &&
                        db.ApplicantAccount.Any(a => a.Id == x.AccountId && a.IsReviewedReturned == 1 && a.IsRemoved == 0)).OrderByDescending(x => x.DateLastUpdate).MyDistinctBy(x => x.Email).OrderBy(x => x.SurName).ToList();
                    mainList = mainList.Where(x => x.PresentProvince == search.ProvinceCode).ToList();
                }
                if (string.IsNullOrEmpty(search.BarangayCode) && string.IsNullOrEmpty(search.CityCode) && string.IsNullOrEmpty(search.ProvinceCode))
                {
                    mainList = mainList.Where(x =>
                        x.PresentBarangay == emDetails.Barangay ||
                        x.PresentMunicipalityCity == emDetails.CityMunicipality ||
                        x.PresentProvince == emDetails.Province).ToList();
                }

                if (gender != "ANY")
                {
                    mainList = mainList.Where(x => x.Gender.ToUpper() == gender.ToUpper()).ToList();
                }
                var totalCount = mainList.Count();
                var numCardDeets = Math.Min(count, totalCount - startIndex);

                var list = mainList.Skip(startIndex).Take(numCardDeets).ToList();

                var results = list.Count == 0 ? db.ApplicantInformation.Where(x => db.ApplicantAccount.Any(a => a.IsReviewedReturned == 1 && a.IsRemoved == 0 && x.AccountId == a.Id)).OrderByDescending(x => x.DateLastUpdate).MyDistinctBy(x => x.Email).Skip(startIndex).Take(numCardDeets).ToList() : list.MyDistinctBy(x => x.Email).ToList();

                var _myRs = new VirtualizedDatViewModel();
                _myRs.Items = results;
                _myRs.TotalCount = totalCount;
                return Ok(_myRs);
            }
        }


        [HttpGet("GetNearbyApplicants/{Id}/{count}")]
        public IActionResult GetNearbyApplicants(string Id, int count)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = cache.Get<NearbyApplicantsViewModel>($"GetNearbyApplicants/{Id}/{count}");
                if (rs == null)
                {
                    rs = new NearbyApplicantsViewModel();
                    var emDetails = db.EmployerDetails.Find(Id);
                    var list = db.ApplicantInformation.Where(x =>
                        !db.EmployerHiredApplicants.Any(b => b.EmployerId == Id && b.ApplicantAccountId == x.AccountId) &&
                        !db.EmployerInterviewedApplicants.Any(b => b.EmployerId == Id && b.ApplicantAccountId == x.AccountId) &&
                        !db.EmployerScheduledInterviews.Any(b => b.EmployerId == Id && b.ApplicantId == x.AccountId) &&
                        db.ApplicantAccount.Any(a => a.Id == x.AccountId && a.IsReviewedReturned == 1 && a.IsRemoved == 0) &&
                        (x.PresentProvince == emDetails.Province || x.ProvincialProvince == emDetails.Province)).OrderByDescending(x => x.DateLastUpdate).MyDistinctBy(x => x.Email).OrderBy(x => Guid.NewGuid()).Take(count).ToList();

                    rs.ApplicantInformationList = list.Count == 0 ? db.ApplicantInformation.Where(x => db.ApplicantAccount.Any(a => a.IsReviewedReturned == 1 && a.IsRemoved == 0 && x.AccountId == a.Id)).OrderByDescending(x => x.DateLastUpdate).MyDistinctBy(x => x.Email).ToList() : list.MyDistinctBy(x => x.Email).ToList();
                    rs.MoreCount = db.ApplicantInformation.Where(x =>
                        !db.EmployerHiredApplicants.Any(b => b.EmployerId == Id && b.ApplicantAccountId == x.AccountId) &&
                        !db.EmployerInterviewedApplicants.Any(b => b.EmployerId == Id && b.ApplicantAccountId == x.AccountId) &&
                        !db.EmployerScheduledInterviews.Any(b => b.EmployerId == Id && b.ApplicantId == x.AccountId) &&
                        db.ApplicantAccount.Any(a => a.Id == x.AccountId && a.IsReviewedReturned == 1 && a.IsRemoved == 0) &&
                        (x.PresentProvince == emDetails.Province || x.ProvincialProvince == emDetails.Province) && !list.Contains(x)).MyDistinctBy(x => x.Email).Count();

                    rs.InterviewedCount = db.EmployerInterviewedApplicants.Where(x =>
                    !db.EmployerHiredApplicants.Any(b => b.EmployerId == Id && b.ApplicantAccountId == x.ApplicantAccountId) &&
                    x.EmployerId == Id).MyDistinctBy(x => x.ApplicantAccountId).Count();
                    rs.HiredCount = (int)emDetails.NumberOfHiredApplicants;
                    cache.Set($"GetNearbyApplicants/{Id}/{count}", rs, TimeSpan.FromSeconds(30));
                }
                return Ok(rs);
            }
        }

        [HttpGet("GetAllApplicants")]
        public IActionResult GetAllApplicants()
        {
            using var connection = dbFactory.CreateDbContext().Database.GetDbConnection();
            var cacheKey = "GetAllApplicantsWithoutHiredAndNotWilling";
            var applicants = cache.Get<List<ApplicantInformation>>(cacheKey);

            if (applicants == null)
            {
                var query = @"   
                    SELECT * 
                    FROM ApplicantInformation a
                    WHERE NOT EXISTS (
                        SELECT 1 
                        FROM EmployerHiredApplicants h 
                        WHERE h.ApplicantAccountId = a.AccountId
                    )
                    AND (a.WillingToWorkNow IS NULL OR LTRIM(RTRIM(a.WillingToWorkNow)) != 'no')";

                applicants = connection.Query<ApplicantInformation>(query).ToList();
                cache.Set(cacheKey, applicants, TimeSpan.FromSeconds(30));
            }

            return Ok(applicants);
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

        [HttpGet("GetEstablishmentDataWithUserAccountId/{userId}")]
        public EmployerRegistrationViewModel GetEstablishmentDataWithUserAccountId(string userId)
        {
            using var db = dbFactory.CreateDbContext();
            var data = cache.Get<EmployerRegistrationViewModel>($"GetEstablishmentDataWithUserAccountId/{userId}");
            if (data == null)
            {
                data = new EmployerRegistrationViewModel();
                var userAccount = db.UserAccounts.Find(userId);
                if (userAccount != null)
                {
                    data.EmployerDetails = db.EmployerDetails.Include(x => x.JobPosts.Where(x => (bool)!x.IsDeleted)).Where(x => x.ContactEmailAddress == userAccount.Email).FirstOrDefault();
                    if (data.EmployerDetails != null && data.EmployerDetails.JobPosts != null)
                    {
                        foreach (var job in data.EmployerDetails.JobPosts)
                        {
                            if (!job.Expiry.HasValue)
                            {
                                job.Expiry = DateTime.Now.AddMonths(2);
                            }
                            if (job.Expiry.Value.ToString("yyyy-MM-dd") == "0001-01-01")
                            {
                                job.Expiry = DateTime.Now.AddMonths(2);
                            }
                            db.EmployerJobPost.Update(job);
                        }
                        db.SaveChanges();
                    }
                    data.ListOfAttachments = new();
                    //var attachmentsDirectory = System.IO.Path.Combine(env.WebRootPath, "files", "employers", data.EmployerDetails.Id);
                    //if (!System.IO.Directory.Exists(attachmentsDirectory))
                    //    System.IO.Directory.CreateDirectory(attachmentsDirectory);
                    //foreach (var file in Directory.GetFiles(attachmentsDirectory))
                    //{
                    //    var fileInfo = new FileInfo(file);
                    //    data.ListOfAttachments.Add(new()
                    //    {
                    //        Id = data.EmployerDetails.Id,
                    //        FileName = System.IO.Path.GetFileName(file),
                    //        FolderName = data.EmployerDetails.Id,
                    //        FileSize = Helper.SizeSuffix(fileInfo.Length),
                    //        IsAlreadyUploaded = 1
                    //    });
                    //}
                }
                cache.Set($"GetEstablishmentDataWithUserAccountId/{userId}", data, TimeSpan.FromSeconds(30));
            }

            return data;
        }

        [HttpPost("AddEmpJobPost")]
        public IActionResult AddEmpJobPost(EmployerDetails data)
        {
            using var db = dbFactory.CreateDbContext();
            db.EmployerDetails.Update(data);
            db.SaveChanges();
            return Ok(data.JobPosts.OrderByDescending(x => x.DatePosted).FirstOrDefault());
        }
        [HttpPost("UpdateEmpJobPost")]
        public IActionResult UpdateEmpJobPost(EmployerJobPost data)
        {
            using var db = dbFactory.CreateDbContext();
            db.EmployerJobPost.Update(data);
            db.SaveChanges();
            return Ok(data);
        }
        [HttpGet("DeleteEmpJobPost/{Id}/{UserId}")]
        public IActionResult DeleteEmpJobPost(string Id, string UserId)
        {
            using var db = dbFactory.CreateDbContext();
            var job = db.EmployerJobPost.Where(x => x.Id == Id).FirstOrDefault();
            if (job != null)
            {
                job.IsDeleted = true;
                db.EmployerJobPost.Update(job);
                db.SaveChanges();
            }
            cache.Remove($"GetEstablishmentDataWithUserAccountId/{UserId}");
            return Ok();
        }

        [HttpPost("UpdateEmployer")]
        public EmployerDetails UpdateEmployer(EmployerRegistrationViewModel data)
        {
            using var db = dbFactory.CreateDbContext();
            db.EmployerDetails.Update(data.EmployerDetails);
            db.SaveChanges();
            var folderDestination = System.IO.Path.Combine(env.ContentRootPath, "files", "employers", data.EmployerDetails.Id);
            if (!System.IO.Directory.Exists(folderDestination))
                System.IO.Directory.CreateDirectory(folderDestination);
            foreach (var f in data.ListOfAttachments)
            {
                var filepath = System.IO.Path.Combine(env.ContentRootPath, "file_temp", f.FolderName, f.FileName);
                if (System.IO.File.Exists(filepath))
                    System.IO.File.Move(filepath, System.IO.Path.Combine(folderDestination, f.FileName));
            }
            return db.EmployerDetails.Include(x => x.JobPosts).Where(x => x.Id == data.EmployerDetails.Id).FirstOrDefault();
        }

        //added controllers (cuebee)
        //added controllers (cuebee)
        //added controllers (cuebee)
        //added controllers (cuebee)

        [HttpGet("GetEmployerDetailsByUserId/{userId}")]
        public IActionResult GetEmployerDetailsByUserId(string userId)
        {
            using var db = dbFactory.CreateDbContext();

            // Find the user account first
            var account = db.UserAccounts.FirstOrDefault(u => u.Id == userId);
            if (account == null)
                return NotFound();

            // EmployerDetails doesn't have UserId property - match by contact email address
            var employer = db.EmployerDetails.FirstOrDefault(e => e.ContactEmailAddress == account.Email);

            if (employer == null)
                return NotFound();

            return Ok(employer);
        }

        [HttpPost("UploadEmployerFile")]
        public async Task<IActionResult> UploadEmployerFile([FromForm] string employerId, [FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
                return BadRequest("No files uploaded.");

            var folderPath = Path.Combine(env.ContentRootPath, "files", "employers", employerId);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var results = new List<AttachementsViewModel>();

            foreach (var file in files)
            {
                var filePath = Path.Combine(folderPath, file.FileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                results.Add(new AttachementsViewModel
                {
                    Id = "0",
                    FolderName = employerId, // since folder is tied to employerId
                    FileName = file.FileName,
                    FileSize = $"{Math.Round(file.Length / 1024.0, 2)} KB",
                    IsAlreadyUploaded = 0
                });
            }

            return Ok(results);
        }


        [HttpGet("GetEmployerFiles/{employerId}")]
        public IActionResult GetEmployerFiles(string employerId)
        {
            var folderPath = Path.Combine(env.ContentRootPath, "files", "employers", employerId);
            if (!Directory.Exists(folderPath))
                return Ok(new List<AttachementsViewModel>());

            var files = Directory.GetFiles(folderPath)
                .Select(f => new FileInfo(f))
                .Select(fi => new AttachementsViewModel
                {
                    Id = "0",
                    FileName = fi.Name,
                    FileSize = $"{Math.Round(fi.Length / 1024.0, 2)} KB",
                    FolderName = employerId,
                    IsAlreadyUploaded = 1
                })
                .ToList();

            return Ok(files);
        }

        [HttpDelete("DeleteEmployerFile/{employerId}/{fileName}")]
        public IActionResult DeleteEmployerFile(string employerId, string fileName)
        {
            var folderPath = Path.Combine(env.ContentRootPath, "files", "employers", employerId);
            var filePath = Path.Combine(folderPath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return Ok();
            }

            return NotFound("File not found.");
        }

        //added controllers (cuebee)
        //added controllers (cuebee)
        //added controllers (cuebee)
        //added controllers (cuebee

        [HttpPost("AddJobPost")]
        public IActionResult AddJobPost(EmployerJobPost jobPost)
        {
            using var db = dbFactory.CreateDbContext();
            db.EmployerJobPost.Add(jobPost);
            db.SaveChanges();
            return Ok(jobPost);
        }
    }
}
