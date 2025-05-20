using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Net;
using webapi_peso.ViewModels;

namespace webapi_peso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

        [HttpPost("GetApplicantsAppliedToJob")]
        public IActionResult GetApplicantsAppliedToJob(RelatedJobViewModel param)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var list = cache.Get<List<AppliedApplicantViewModel>>($"GetApplicantsAppliedToJob/{param.EmpId}/{param.JobPostId}");
                if (list == null)
                {
                    list = new List<AppliedApplicantViewModel>();
                    var jobPosts = db.JobApplicantion.Where(x => x.JobPostId == param.JobPostId).OrderByDescending(x => x.DateCreated).MyDistinctBy(x => x.ApplicantId).ToList();
                    foreach (var i in jobPosts)
                    {
                        var model = new AppliedApplicantViewModel();
                        var empInfo = db.ApplicantInformation.Where(x => x.AccountId == i.ApplicantId).FirstOrDefault();
                        model.Applicant = empInfo;
                        var dir = System.IO.Path.Combine(env.WebRootPath, "files", "applications", i.Id);
                        var files = Directory.GetFiles(dir, "*.pdf");
                        var fileName = WebUtility.HtmlEncode(files[0]);
                        var fileInfo = new FileInfo(fileName);
                        model.FilePath = $"{ProjectConfig.API_HOST}/files/applications/{i.Id}/{fileInfo.Name}";
                        model.DateApplied = i.DateCreated;
                        model.IsInterviewed = db.EmployerInterviewedApplicants.Any(x => x.EmployerId == param.EmpId && x.ApplicantAccountId == i.ApplicantId);
                        model.IsHired = db.EmployerHiredApplicants.Any(x => x.EmployerId == param.EmpId && x.ApplicantAccountId == i.ApplicantId);
                        list.Add(model);
                    }
                    cache.Set($"GetApplicantsAppliedToJob/{param.EmpId}/{param.JobPostId}", list, TimeSpan.FromSeconds(30));
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
    }
}
