using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using webapi_peso.Model;
using webapi_peso.ViewModels;

namespace webapi_peso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MunManagerController : ControllerBase
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;
        private readonly IMemoryCache cache;

        public MunManagerController(IDbContextFactory<ApplicationDbContext> _dbFactory, IMemoryCache _cache)
        {
            dbFactory = _dbFactory;
            cache = _cache;
        }

        [HttpGet("GetUser/{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            using var db = dbFactory.CreateDbContext();
            var user = await db.UserAccounts.FindAsync(userId);
            return Ok(user);
        }

        [HttpGet("GetPESOManagerAccount/{accountId}")]
        public IActionResult GetPESOManagerAccount(string accountId)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = new PESOManagerAccountViewModel();
                var account = db.UserAccounts.Where(x => x.Id == accountId).FirstOrDefault();
                rs.UserAccount = account;
                var information = db.PesoManagerInformation.Where(x => x.AccountId == accountId).FirstOrDefault();
                if (information != null)
                {
                    rs.UserInformation = information;
                }
                else
                {
                    information = new PesoManagerInformation();
                    information.AccountId = accountId;
                    db.PesoManagerInformation.Add(information);
                    db.SaveChanges();
                    rs.UserInformation = information;
                }
                return Ok(rs);
            }
        }

        [HttpGet("GetNumberOfEmplyerAndApplicant/{provCode}/{cityCode}")]
        public IActionResult GetNumberOfEmplyerAndApplicant(string provCode, string cityCode)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = new PESOManagerAccountViewModel();
                var listofapplicants = db.ApplicantAccount.Where(x => x.IsReviewedReturned == 1 && x.IsRemoved == 0).ToList();
                foreach (var i in listofapplicants)
                {
                    var appInfo = db.ApplicantInformation.Where(x => x.AccountId == i.Id && x.PresentProvince == provCode && x.PresentMunicipalityCity == cityCode).MyDistinctBy(x => x.Email);
                    if (appInfo != null)
                        rs.NumberOfApplicants += appInfo.Count();
                }
                //rs.NumberOfApplicants = db.ApplicantInformation.Where(x => x.PresentProvince == provCode && x.PresentMunicipalityCity == cityCode).Count();
                rs.NumberOfEmployers = db.EmployerDetails.Where(x => x.Province == provCode && x.CityMunicipality == cityCode).Count();

                return Ok(rs);
            }
        }

        [HttpGet("GetProvince")]
        public List<RefProvince> GetProvince()
        {
            var data = cache.Get<List<RefProvince>>("province");
            if (data == null)
            {
                var file = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\{"json\\refprovince.json"}");
                var json = System.IO.File.ReadAllText(file);
                data = JsonConvert.DeserializeObject<List<RefProvince>>(json);
                cache.Set("province", data);
            }
            return data;
        }

        [HttpGet("GetCity")]
        public List<RefCityMun> GetCity()
        {
            var data = cache.Get<List<RefCityMun>>("city");
            if (data == null)
            {
                var file = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\{"json\\refcitymun.json"}");
                var json = System.IO.File.ReadAllText(file);
                data = JsonConvert.DeserializeObject<List<RefCityMun>>(json);

                var a = new RefCityMun() { citymunCode = "PESO", citymunDesc = "PESO Province", provCode = "1043" };
                data.Add(a);
                cache.Set("city", data);
            }
            return data;
        }

        [HttpGet("GetRegisteredReferredReports/{month}/{year}")]
        public IActionResult GetRegisteredReferredReports(int month, int year)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = new List<JobRegisteredReferredViewModel>();
                var list = db.JobApplicantsReferred.Where(x => x.DateReferred.Month == month && x.DateReferred.Year == year).ToList();
                foreach (var i in list)
                {
                    var appInfo = db.ApplicantInformation.Where(x => x.Id == i.ApplicantAccountId).FirstOrDefault();
                    var empInfo = db.EmployerDetails.Where(x => x.Id == i.EmployerId).FirstOrDefault();
                    var educ = db.ApplicantEducationalBackground.Where(x => x.AccountId == i.ApplicantAccountId).ToList();
                    string educationalAttainment = "N/A";
                    if (educ != null)
                    {
                        if (educ.Any(x => x.LevelName == "Graduate Studies" && (!string.IsNullOrEmpty(x.Course) || !string.IsNullOrEmpty(x.School))))
                        {
                            educationalAttainment = "Master's Degree";
                        }
                        else if (educ.Any(x => x.LevelName == "Tertiary" && (!string.IsNullOrEmpty(x.Course) || !string.IsNullOrEmpty(x.School))))
                        {
                            educationalAttainment = "College Level";
                        }
                        else if (educ.Any(x => x.LevelName == "Secondary" && (!string.IsNullOrEmpty(x.Course) || !string.IsNullOrEmpty(x.School))))
                        {
                            educationalAttainment = "Highschool Level";
                        }
                        else if (educ.Any(x => x.LevelName == "Elementary" && (!string.IsNullOrEmpty(x.Course) || !string.IsNullOrEmpty(x.School))))
                        {
                            educationalAttainment = "Elementary Level";
                        }
                    }
                    if (appInfo != null)
                    {
                        rs.Add(new JobRegisteredReferredViewModel()
                        {
                            ApplicantName = $"{appInfo.FirstName} {appInfo.SurName}",
                            Skills = i.JobTitle,
                            Gender = appInfo.Gender,
                            Age = Helper.GetAge(appInfo.DateOfBirth).ToString(),
                            CivilStatus = appInfo.CivilStatus,
                            Education = educationalAttainment,
                            EmploymentStatus = appInfo.EmpStatus,
                            ReferredAs = i.JobTitle,
                            ReferredTo = empInfo != null ? empInfo.EstablishmentName : ""
                        });
                    }
                }

                return Ok(rs);
            }
        }

        [HttpGet("GetJobVacancySolicitedReport/{cityCode}/{month}/{year}/{isEmployerIncluded}")]
        public IActionResult GetJobVacancySolicitedReport(string cityCode, int month, int year, bool isEmployerIncluded)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var result = new List<ReportVacancyViewModel>();
                if (isEmployerIncluded)
                {
                    var emps = db.EmployerDetails.Where(x => x.CityMunicipality == cityCode).ToList();
                    if (emps != null && emps.Count > 0)
                    {
                        int count = 0;
                        foreach (var emp in emps)
                        {
                            foreach (var jobposts in db.EmployerJobPost.Where(x => x.EmployerDetailsId == emp.Id && x.DatePosted.Month == month && x.DatePosted.Year == year))
                            {
                                count += 1;
                                result.Add(new ReportVacancyViewModel()
                                {
                                    Count = count,
                                    Company = emp.EstablishmentName,
                                    ListOfJobs = new List<JobVacancySolicited>()
                                    {
                                       new JobVacancySolicited()
                                       {
                                           AgeFrom = jobposts.AgeFrom,
                                           AgeTo = jobposts.AgeTo,
                                           CityMunCode = emp.CityMunicipality,
                                           JobTitle = jobposts.Description,
                                           Company = emp.EstablishmentName,
                                           NumberOfVacancy = jobposts.NumberOfVacancy,
                                           Month = jobposts.DatePosted.Month,
                                           Year = jobposts.DatePosted.Year,
                                           MajorOccCode = "",
                                           Gender = jobposts.Gender,
                                           CivilStatus = jobposts.CivilStatus,
                                           EducationalAttainment = jobposts.EducationalAttainment,
                                           WorkExperience = jobposts.WorkExperience,
                                           Salary = jobposts.Salary.HasValue ? jobposts.Salary.Value : 0,
                                           ReasonExpansion = jobposts.ReasonExpansion,
                                           ReasonReplaceMent = jobposts.ReasonReplaceMent,
                                           ReasonOthers = jobposts.ReasonOthers,
                                           IndustryCode = ""
                                       }
                                    }
                                });
                            }
                        }
                    }
                }
                var companies = db.JobVacancySolicited.Where(x => x.CityMunCode == cityCode && x.Month == month && x.Year == year).MyDistinctBy(x => x.Company);
                if (companies.Any())
                {
                    int count = 0;
                    foreach (var c in companies)
                    {
                        count += 1;
                        result.Add(new ReportVacancyViewModel()
                        {
                            Count = count,
                            Company = c.Company,
                            ListOfJobs = db.JobVacancySolicited.Where(x => x.Company == c.Company).ToList()
                        });
                    }
                }
                return Ok(result);
            }
        }
    }
}
