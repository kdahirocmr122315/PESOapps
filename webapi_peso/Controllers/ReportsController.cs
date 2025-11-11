using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Text;
using webapi_peso.Model;
using webapi_peso.ViewModels;

namespace webapi_peso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;
        private readonly IMemoryCache cache;
        private readonly IWebHostEnvironment env;

        public ReportsController(IDbContextFactory<ApplicationDbContext> _dbFactory, IMemoryCache _cache, IWebHostEnvironment _env)
        {
            dbFactory = _dbFactory;
            cache = _cache;
            env = _env;
        }

        [HttpGet("GetRegisteredReferredReports")]
        public IActionResult GetRegisteredReferredReports()
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = new List<JobRegisteredReferredViewModel>();
                var list = db.JobApplicantsReferred.ToList();
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

        [HttpGet("GetJobVacancySolicitedReport")]
        public IActionResult GetJobVacancySolicitedReport()
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var result = new List<ReportVacancyViewModel>();
                    var emps = db.EmployerDetails.ToList();
                    if (emps != null && emps.Count > 0)
                    {
                        int count = 0;
                        foreach (var emp in emps)
                        {
                            foreach (var jobposts in db.EmployerJobPost.Where(x => x.EmployerDetailsId == emp.Id))
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
                var companies = db.JobVacancySolicited.MyDistinctBy(x => x.Company);
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

        [HttpGet("GetRecordedJobTitle")]
        public IActionResult GetRecordedJobTitle()
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var result = db.JobApplicantsPlaced.MyDistinctBy(x => x.JobTitle).Select(x => x.JobTitle).ToList();
                var list2 = db.JobVacancySolicited.MyDistinctBy(x => x.JobTitle).Select(x => x.JobTitle).ToList();
                result.AddRange(list2);
                result.ForEach(x => x.ToUpper());
                return Ok(result.MyDistinctBy(x => x).ToList());
            }
        }

        [HttpGet("CompanyList")]
        public IActionResult CompanyList()
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = cache.Get<List<string>>("CompanyList");
                if (rs == null)
                {
                    rs = new List<string>();
                    rs = db.EmployerDetails.Where(x => x.Status == ProjectConfig.ACCOUNT_STATUS.APPROVED).Select(x => x.EstablishmentName.ToUpper()).MyDistinctBy(x => x).ToList();
                    cache.Set("CompanyList", rs, TimeSpan.FromSeconds(30));
                }
                return Ok(rs);
            }
        }
        [HttpGet("CompanyList/{strSearch}")]
        public IActionResult CompanyList(string strSearch)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = cache.Get<List<string>>($"CompanyList/{strSearch}");
                if (rs == null)
                {
                    rs = new List<string>();
                    var list = db.EmployerDetails.Where(x => x.Status == ProjectConfig.ACCOUNT_STATUS.APPROVED).Select(x => x.EstablishmentName.ToUpper()).MyDistinctBy(x => x).ToList();
                    rs = list.Where(x => x.Contains(strSearch, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    cache.Set($"CompanyList/{strSearch}", rs, TimeSpan.FromSeconds(30));
                }
                return Ok(rs);
            }
        }

        [HttpPost("SaveSolicitedVacancies")]
        public IActionResult SaveSolicitedVacancies(List<JobVacancySolicited> data)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                data.ForEach(x => x.DateCreated = DateTime.Now);
                db.JobVacancySolicited.AddRange(data);
                db.SaveChanges();
                if (data != null && data.Count > 0)
                    cache.Remove($"GetReports/JobVacancies/{data.FirstOrDefault().Month}/{data.FirstOrDefault().Year}/{data.FirstOrDefault().CityMunCode}");
                return Ok();
            }
        }

        [HttpGet("GetReferralReport")]
        public async Task<IActionResult> GetReferralReport()
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = new List<ConsolidatedReportViewModel>();
                var list = db.JobApplicantsReferred.MyDistinctBy(x => x.EmployerId).ToList();
                int noRow = 0;
                foreach (var i in list)
                {
                    var empDetails = db.EmployerDetails.Where(x => x.Id == i.EmployerId).FirstOrDefault();
                    if (empDetails != null)
                    {
                        var city = FindCity(empDetails.CityMunicipality);
                        rs.Add(new ConsolidatedReportViewModel()
                        {
                            RowNumber = noRow,
                            ReportName = "Referred Applicants",
                            MunicipalityName = city.citymunDesc,
                            NumberOfApplicants = db.JobApplicantsReferred.Where(x => x.EmployerId == empDetails.Id).Count()
                        });
                    }
                }
                var resultList = rs.MyDistinctBy(x => x.MunicipalityName);
                foreach (var i in resultList)
                {
                    noRow += 1;
                    i.RowNumber = noRow;
                }
                rs.AddRange(AddPESOProvinceData(db, noRow));

                return Ok(resultList);
            }
        }

        [HttpGet("GetHiredApplicantsList/{month}/{year}/{cityCode}/{isExport}")]
        public async Task<IActionResult> GetHiredApplicantsList(int month, int year, string cityCode, bool isExport)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var list = db.EmployerHiredApplicants.Where(x => x.DateHired.Month == month && x.DateHired.Year == year).ToList();
                var rs = new List<JobApplicantsPlaced>();
                foreach (var hired in list)
                {
                    var emp = db.EmployerDetails.Where(x => x.Id == hired.EmployerId && x.CityMunicipality == cityCode).FirstOrDefault();
                    var applicant = db.ApplicantInformation.Where(x => x.AccountId == hired.ApplicantAccountId).FirstOrDefault();
                    if (emp != null && applicant != null)
                    {
                        rs.Add(new JobApplicantsPlaced()
                        {
                            Company = emp.EstablishmentName,
                            ApplicantName = $"{applicant.FirstName} {applicant.SurName}",
                            CityMunCode = cityCode,
                            DateHired = hired.DateHired,
                            JobTitle = hired.HiredPosition
                        });
                    }
                }
                if (isExport)
                {
                    var csv = new StringBuilder();
                    csv.AppendLine($"Generated Date:,{DateTime.Now.ToString("MM-dd-yyyy")},{DateTime.Now.ToString("hh:mmtt").ToUpper()}");
                    csv.AppendLine("No,NAME OF APPLICANT, AS (Position), TO (Employer)");
                    if (rs.Count > 0)
                    {
                        int count = 0;
                        foreach (var i in rs.OrderBy(x => x.DateCreated))
                        {
                            count += 1;
                            var newLine = $"{count}, {i.ApplicantName}, {i.JobTitle}, {i.Company}";
                            csv.AppendLine(newLine);
                        }
                    }
                    var dir = System.IO.Path.Combine(env.ContentRootPath, "files", "csv");
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    try
                    {
                        foreach (var item in Directory.GetFiles(dir))
                        {
                            if (System.IO.File.Exists(item))
                                System.IO.File.Delete(item);
                        }
                    }
                    catch (Exception) { }
                    var fileName = System.IO.Path.Combine(dir, $"export_placed_applicants_{DateTime.Now.ToString("MMddyyyyHHmmss")}.csv");
                    using (StreamWriter sw = new StreamWriter(System.IO.File.Open(fileName, FileMode.Create), Encoding.UTF8))
                    {
                        await sw.WriteAsync(csv.ToString());
                    }
                    var url = $"{ProjectConfig.API_HOST}/files/csv/{System.IO.Path.GetFileName(fileName)}";
                    using (Stream stream = System.IO.File.OpenRead(fileName))
                    {
                        var data = new System.IO.MemoryStream();
                        stream.CopyTo(data);
                        data.Seek(0, SeekOrigin.Begin);
                        var buf = new byte[data.Length];
                        data.Read(buf, 0, buf.Length);

                        var f = File(fileContents: buf,
                            contentType: "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                            fileDownloadName: System.IO.Path.GetFileName(fileName));
                        return Ok(url);
                    }

                }
                return Ok(rs);
            }
        }

        [HttpGet("GetPreRegListChart")]
        public IActionResult GetPreRegListChart()
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = new List<ApplicantInformation>();
                var list = db.ApplicantInformation.ToList();


                return Ok(list);
            }
        }

        [HttpGet("GetReferredApplicantChart/{startMonth}/{endMonth}/{year}")]
        public IActionResult GetReferredApplicantChart(int startMonth, int endMonth, int year)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = new List<ReferralViewModel>();
                var list = db.JobApplicantsReferred.Where(x => x.DateReferred.Month >= startMonth && x.DateReferred.Month <= endMonth && x.DateReferred.Year == year).ToList();

                return Ok(list);
            }


        }

        [HttpGet("GetHiredApplicantsListChart")]
        public IActionResult GetHiredApplicantsListChart()
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var list = db.JobApplicantsPlaced.ToList();

                return Ok(list);
            }
        }

        [HttpGet("GetSolicitedReportChart/{startMonth}/{endMonth}/{year}")]
        public IActionResult GetSolicitedReportChart(int startMonth, int endMonth, int year)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var solicited = db.JobVacancySolicited
                    .Where(x => x.DateCreated != null) // ✅ Prevent null crash
                    .Where(x => x.DateCreated.Month >= startMonth
                             && x.DateCreated.Month <= endMonth
                             && x.DateCreated.Year == year)
                    .Select(x => new JobVacancySolicited
                    {
                        NumberOfVacancy = x.NumberOfVacancy,
                        Month = x.DateCreated.Month,
                        Year = x.DateCreated.Year,
                        JobTitle = x.JobTitle,
                        Company = x.Company
                    })
                    .ToList();

                return Ok(solicited);
            }
        }


        [HttpGet("GetReportByGender")]
        public IActionResult GetReportByGender()
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var listOfMunicipality = GetCity().Where(x => x.provCode == "1043").ToList();
                var list = new List<ConsolidatedReportViewModel>();

                int noRow = 0;
                foreach (var city in listOfMunicipality)
                {
                    noRow += 1;
                    var registered = 0;
                    var registeredFemale = 0;
                    var listOfRegistered = db.ApplicantInformation.Where(x => x.PresentMunicipalityCity == city.citymunCode);
                    foreach (var i in listOfRegistered)
                    {
                        registered += 1;
                        if (i.Gender.ToLower() == "female")
                                registeredFemale += 1;
                        
                    }

                    var solicited = db.JobVacancySolicited.Sum(x => x.NumberOfVacancy);
                    var solicitedFemale = db.JobVacancySolicited.Sum(x => x.NumberOfVacancy);
                    var referred = db.JobApplicantsReferred.Count();
                    var referredFemale = db.JobApplicantsReferred.Count();
                    var placed = db.JobApplicantsPlaced.Count();
                    var placedFemale = db.JobApplicantsPlaced.Count();

                    var otherRegistered = db.PESOManualReport.Sum(x => x.Registered);
                    var otherRegisteredFemale = db.PESOManualReport.Sum(x => x.RegisteredFemale);
                    var otherSolicited = db.PESOManualReport.Sum(x => x.Solicited);
                    var otherSolicitedFemale = db.PESOManualReport.Sum(x => x.SolicitedFemale);
                    var otherReferred = db.PESOManualReport.Sum(x => x.Referred);
                    var otherReferredFemale = db.PESOManualReport.Sum(x => x.ReferredFemale);
                    var otherPlaced = db.PESOManualReport.Sum(x => x.Placed);
                    var otherPlacedFemale = db.PESOManualReport.Sum(x => x.PlacedFemale);

                    var rs = new ConsolidatedReportViewModel();
                    rs.RowNumber = noRow;
                    rs.MunicipalityName = city.citymunDesc;
                    rs.Solicited = solicited + otherSolicited;
                    rs.SolicitedFemale = solicitedFemale + otherSolicitedFemale;
                    rs.Registered = registered + otherRegistered;
                    rs.RegisteredFemale = registeredFemale + otherRegisteredFemale;
                    rs.Referred = referred + otherReferred;
                    rs.ReferredFemale = referredFemale + otherReferredFemale;
                    rs.Placed = placed + otherPlaced;
                    rs.PlacedFemale = placedFemale + otherPlacedFemale;
                    list.Add(rs);
                }
                list.AddRange(AddPESOProvinceData(db, noRow));

                return Ok(list);
            }
        }

        [HttpGet("GetOverallMonthlyReport1/{myPage}/{title}")]
        public async Task<IActionResult> GetOverallMonthlyReport1(string myPage, string title)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var listOfMunicipality = cache.Get<List<RefCityMun>>("list_city");
                if (listOfMunicipality == null)
                {
                    listOfMunicipality = GetCity().Where(x => x.provCode == "1043").ToList();
                    cache.Set("list_city", listOfMunicipality);
                }

                string key = $"GetOverallMonthlyReport1/{myPage}/{title}";
                var list = cache.Get<List<ConsolidatedReportViewModel>>(key);
                if (list == null)
                {
                    list = new();
                    int noRow = 0;
                    foreach (var city in listOfMunicipality)
                    {
                        noRow += 1;
                        for (int i = 1; i <= 12; i++)
                        {
                            var registered = 0;
                            var listOfRegistered = db.ApplicantInformation.Where(x => x.PresentMunicipalityCity == city.citymunCode);
                            foreach (var x in listOfRegistered)
                            {
                                    registered += 1;
                            }

                            var solicited = db.JobVacancySolicited.Sum(x => x.NumberOfVacancy);
                            var referred = db.JobApplicantsReferred.Count();
                            var placed = db.JobApplicantsPlaced.Count();

                            var otherRegistered = db.PESOManualReport.Sum(x => x.Registered);
                            var otherSolicited = db.PESOManualReport.Sum(x => x.Solicited);
                            var otherReferred = db.PESOManualReport.Sum(x => x.Referred);
                            var otherPlaced = db.PESOManualReport.Sum(x => x.Placed);

                            var rs = new ConsolidatedReportViewModel();
                            rs.RowNumber = noRow;
                            rs.MunicipalityName = city.citymunDesc;
                            rs.MonthName = Helper.ToMonthName(i);
                            rs.Month = i;
                            rs.Solicited = solicited + otherSolicited;
                            rs.Registered = registered + otherRegistered;
                            rs.Referred = referred + otherReferred;
                            rs.Placed = placed + otherPlaced;

                            list.Add(rs);
                        };
                    }
                    cache.Set(key, list, TimeSpan.FromMinutes(5));
                }


                //list.AddRange(AddPESOProvinceData(db, noRow, year));

                return Ok(list);
            }
        }

        [HttpGet("GetMonthlyReport1")]
        public async Task<IActionResult> GetMonthlyReport1()
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var listOfMunicipality = cache.Get<List<RefCityMun>>("list_city");
                if (listOfMunicipality == null)
                {
                    listOfMunicipality = GetCity().Where(x => x.provCode == "1043").ToList();
                    cache.Set("list_city", listOfMunicipality);
                }

                string key = $"GetMonthlyReport1";
                var list = cache.Get<List<ConsolidatedReportViewModel>>(key);
                if (list == null)
                {
                    list = new();
                    int noRow = 0;
                    foreach (var city in listOfMunicipality)
                    {
                        noRow += 1;
                        var registered = 0;
                        var listOfRegistered = db.ApplicantInformation.Where(x => x.PresentMunicipalityCity == city.citymunCode);
                        foreach (var i in listOfRegistered)
                        {
                                registered += 1;
                        }

                        var solicited = db.JobVacancySolicited.Sum(x => x.NumberOfVacancy);
                        var referred = db.JobApplicantsReferred.Count();
                        var placed = db.JobApplicantsPlaced.Count();

                        var otherRegistered = db.PESOManualReport.Sum(x => x.Registered);
                        var otherSolicited = db.PESOManualReport.Sum(x => x.Solicited);
                        var otherReferred = db.PESOManualReport.Sum(x => x.Referred);
                        var otherPlaced = db.PESOManualReport.Sum(x => x.Placed);


                        var rs = new ConsolidatedReportViewModel();
                        rs.RowNumber = noRow;
                        rs.MunicipalityName = city.citymunDesc;
                        rs.Solicited = solicited + otherSolicited;
                        rs.Registered = registered + otherRegistered;
                        rs.Referred = referred + otherReferred;
                        rs.Placed = placed + otherPlaced;
                        list.Add(rs);
                    }
                    list.AddRange(AddPESOProvinceData(db, noRow));
                    cache.Set(key, list, TimeSpan.FromMinutes(5));
                }

                return Ok(list);
            }
        }

        [HttpGet("GetSolicitedReport1")]
        public async Task<IActionResult> GetSolicitedReport1()
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var listOfMunicipality = cache.Get<List<RefCityMun>>("list_city");
                if (listOfMunicipality == null)
                {
                    listOfMunicipality = GetCity().Where(x => x.provCode == "1043").ToList();
                    cache.Set("list_city", listOfMunicipality);
                }
                string key = $"GetSolicitedReport1";
                var list = cache.Get<List<ConsolidatedReportViewModel>>(key);
                if (list == null)
                {
                    list = new();
                    int noRow = 0;
                    foreach (var city in listOfMunicipality)
                    {
                        noRow += 1;
                        var rs = new ConsolidatedReportViewModel();
                        rs.RowNumber = noRow;
                        rs.MunicipalityName = city.citymunDesc;
                        var solicited = db.JobVacancySolicited.Sum(x => x.NumberOfVacancy);
                        var pesoReport = db.PESOManualReport.Sum(x => x.Solicited);
                        rs.Solicited = solicited + pesoReport;
                        list.Add(rs);
                    }
                    cache.Set(key, list, TimeSpan.FromMinutes(5));
                }

                //list.AddRange(AddPESOProvinceData(db, noRow, year));

                return Ok(list);
            }
        }

        [HttpGet("GetRegisteredReport1")]
        public async Task<IActionResult> GetRegisteredReport1()
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var listOfMunicipality = cache.Get<List<RefCityMun>>("list_city");
                if (listOfMunicipality == null)
                {
                    listOfMunicipality = GetCity().Where(x => x.provCode == "1043").ToList();
                    cache.Set("list_city", listOfMunicipality);
                }
                string key = $"GetRegisteredReport1";
                var list = cache.Get<List<ConsolidatedReportViewModel>>(key);
                if (list == null)
                {
                    list = new();
                    int noRow = 0;
                    foreach (var city in listOfMunicipality)
                    {
                        var rs = new ConsolidatedReportViewModel();
                        noRow += 1;
                        var registered = 0;
                        var listOfRegistered = db.ApplicantInformation.Where(x => x.PresentMunicipalityCity == city.citymunCode);
                        foreach (var i in listOfRegistered)
                        {
                                registered += 1;
                        }
                        var pesoReport = db.PESOManualReport.Sum(x => x.Registered);

                        rs.Registered = registered + pesoReport;
                        rs.RowNumber = noRow;
                        rs.MunicipalityName = city.citymunDesc;
                        list.Add(rs);
                    }
                    cache.Set(key, list, TimeSpan.FromMinutes(5));
                }

                //list.AddRange(AddPESOProvinceData(db, noRow, year));

                return Ok(list);
            }
        }

        [HttpGet("GetReferredReport1")]
        public async Task<IActionResult> GetReferredReport1()
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var listOfMunicipality = cache.Get<List<RefCityMun>>("list_city");
                if (listOfMunicipality == null)
                {
                    listOfMunicipality = GetCity().Where(x => x.provCode == "1043").ToList();
                    cache.Set("list_city", listOfMunicipality);
                }
                string key = $"GetReferredReport1";
                var list = cache.Get<List<ConsolidatedReportViewModel>>(key);
                if (list == null)
                {
                    list = new();
                    int noRow = 0;
                    foreach (var city in listOfMunicipality)
                    {
                        noRow += 1;
                        var referred = db.JobApplicantsReferred.Count();
                        var rs = new ConsolidatedReportViewModel();
                        rs.RowNumber = noRow;
                        rs.MunicipalityName = city.citymunDesc;

                        var pesoReport = db.PESOManualReport.Sum(x => x.Referred);

                        rs.Referred = referred + pesoReport;
                        list.Add(rs);
                    }
                    cache.Set(key, list, TimeSpan.FromMinutes(5));
                }

                //list.AddRange(AddPESOProvinceData(db, noRow, year));

                return Ok(list);
            }
        }

        [HttpGet("GetPlacedReport1")]
        public async Task<IActionResult> GetPlacedReport1()
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var listOfMunicipality = cache.Get<List<RefCityMun>>("list_city");
                if (listOfMunicipality == null)
                {
                    listOfMunicipality = GetCity().Where(x => x.provCode == "1043").ToList();
                    cache.Set("list_city", listOfMunicipality);
                }
                string key = $"GetPlacedReport1";
                var list = cache.Get<List<ConsolidatedReportViewModel>>(key);
                if (list == null)
                {
                    list = new();
                    int noRow = 0;
                    foreach (var city in listOfMunicipality)
                    {
                        noRow += 1;
                        var placed = db.JobApplicantsPlaced.Count();
                        var rs = new ConsolidatedReportViewModel();
                        rs.RowNumber = noRow;
                        rs.MunicipalityName = city.citymunDesc;

                        var pesoReport = db.PESOManualReport.Sum(x => x.Placed);

                        rs.Placed = placed + pesoReport;
                        list.Add(rs);
                    }
                    cache.Set(key, list, TimeSpan.FromMinutes(5));
                }

                //list.AddRange(AddPESOProvinceData(db, noRow, year));

                return Ok(list);
            }
        }

        List<ConsolidatedReportViewModel> AddPESOProvinceData(ApplicationDbContext db, int noRow)
        {
            var rs = new List<ConsolidatedReportViewModel>();
            var pesoReport = new PESOManualReport();

            if (pesoReport == null)
            {
                pesoReport = new PESOManualReport();
                pesoReport.MunicipalityCode = "PESO";
                db.PESOManualReport.Add(pesoReport);
                db.SaveChanges();
            }

            if (!string.IsNullOrEmpty(pesoReport.MunicipalityCode))
            {
                rs.Add(new ConsolidatedReportViewModel()
                {
                    Month = pesoReport.Month,
                    MonthName = Helper.ToMonthName(pesoReport.Month),
                    MunicipalityName = FindCity(pesoReport.MunicipalityCode).citymunDesc,
                    RowNumber = noRow + 1,
                    Solicited = pesoReport.Solicited,
                    SolicitedFemale = pesoReport.SolicitedFemale,
                    Registered = pesoReport.Registered,
                    RegisteredFemale = pesoReport.RegisteredFemale,
                    Referred = pesoReport.Referred,
                    ReferredFemale = pesoReport.ReferredFemale,
                    Placed = pesoReport.Placed,
                    PlacedFemale = pesoReport.PlacedFemale
                });
            }

            return rs;
        }

        [HttpGet("GetPESOManualReport/{year}/{cityCode}")]
        public IActionResult GetPESOManualReport(int year, string cityCode)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = new ManualReportViewModel();
                var listOfMunicipality = GetCity().Where(x => x.provCode == "1043").ToList();
                //listOfMunicipality.Add(new RefCityMun() { citymunCode = "PESO", citymunDesc = "PESO Province" });

                var list = db.PESOManualReport.Where(x => x.Year == year && x.Month > 0 && x.MunicipalityCode == cityCode).OrderByDescending(x => x.Year).ToList();

                rs.ListOfMunicipality = listOfMunicipality;
                rs.ListOfReports = list;
                return Ok(rs);
            }
        }

        [HttpGet("GetSinglePESOManualReport/{year}/{month}/{cityCode}")]
        public async Task<IActionResult> GetSinglePESOManualReport(int year, int month, string cityCode)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                if (cityCode == "PESO")
                {
                    var list = db.PESOManualReport.Where(x => string.IsNullOrEmpty(x.MunicipalityCode));
                    await list.ForEachAsync(x => x.MunicipalityCode = "PESO");
                    db.PESOManualReport.UpdateRange(list);
                    db.SaveChanges();
                }
                var listOfMunicipality = GetCity().Where(x => x.provCode == "1043").ToList();
                listOfMunicipality.Add(new RefCityMun() { citymunCode = "PESO", citymunDesc = "PESO Province" });
                var data = db.PESOManualReport.Where(x => x.Year == year && x.Month == month && x.MunicipalityCode == cityCode).FirstOrDefault();
                var rs = new ManualReportViewModel();
                if (data != null)
                {
                    rs.Report = data;
                    rs.ListOfMunicipality = listOfMunicipality;
                    return Ok(rs);
                }
                return Ok(new ManualReportViewModel() { ListOfMunicipality = listOfMunicipality, Report = new PESOManualReport() });
            }
        }

        //Get Address ------------------

        [HttpGet("FindRegion/{code}")]
        public RefRegion FindRegion(string code)
        {
            var data = cache.Get<List<RefRegion>>("region");
            if (data == null)
            {
                var file = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\{"json\\refregion.json"}");
                var json = System.IO.File.ReadAllText(file);
                data = JsonConvert.DeserializeObject<List<RefRegion>>(json);
                cache.Set("region", data);
            }
            return data.Where(x => x.regCode == code).FirstOrDefault();
        }
        [HttpGet("GetRegion")]
        [AllowAnonymous]
        public List<RefRegion> GetRegion()
        {
            var data = cache.Get<List<RefRegion>>("region");
            if (data == null)
            {
                var file = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\{"json\\refregion.json"}");
                var json = System.IO.File.ReadAllText(file);
                data = JsonConvert.DeserializeObject<List<RefRegion>>(json);
                cache.Set("region", data);
            }
            return data;
        }
        [HttpGet("GetProvince")]
        [AllowAnonymous]
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
        [HttpGet("GetProvince/{limit}")]
        [AllowAnonymous]
        public List<RefProvince> GetProvince(int limit)
        {
            var data = cache.Get<List<RefProvince>>($"GetProvince/{limit}");
            if (data == null)
            {
                var file = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\{"json\\refprovince.json"}");
                var json = System.IO.File.ReadAllText(file);
                data = JsonConvert.DeserializeObject<List<RefProvince>>(json);
                cache.Set($"GetProvince/{limit}", data.Take(limit).ToList());
            }
            return data.Take(limit).ToList();
        }
        [HttpGet("FindProvince/{code}")]
        [AllowAnonymous]
        public RefProvince FindProvince(string code)
        {
            return GetProvince().Where(x => x.provCode == code).FirstOrDefault();
        }
        [HttpGet("GetCity")]
        [AllowAnonymous]
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
        [HttpGet("GetCity/{limit}")]
        [AllowAnonymous]
        public List<RefCityMun> GetCity(int limit)
        {
            var data = cache.Get<List<RefCityMun>>($"city/{limit}");
            if (data == null)
            {
                var file = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\{"json\\refcitymun.json"}");
                var json = System.IO.File.ReadAllText(file);
                data = JsonConvert.DeserializeObject<List<RefCityMun>>(json);
                cache.Set($"city/{limit}", data.Take(limit).ToList());
            }
            return data.Take(limit).ToList();
        }
        [HttpGet("GetCity/{limit}/{provCode}")]
        [AllowAnonymous]
        public List<RefCityMun> GetCity(int limit, string provCode)
        {
            var data = cache.Get<List<RefCityMun>>($"city/{limit}/{provCode}");
            if (data == null)
            {
                var file = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\{"json\\refcitymun.json"}");
                var json = System.IO.File.ReadAllText(file);
                data = JsonConvert.DeserializeObject<List<RefCityMun>>(json);
                cache.Set($"city/{limit}/{provCode}", data.Where(x => x.provCode == provCode).Take(limit).ToList());
            }
            return data.Where(x => x.provCode == provCode).Take(limit).ToList();
        }
        [HttpGet("FindCity/{code}")]
        [AllowAnonymous]
        public RefCityMun FindCity(string code)
        {
            if (code == "PESO")
            {
                return new RefCityMun() { citymunCode = code, citymunDesc = "PESO Province", provCode = "1043" };
            }
            var rs = GetCity().Where(x => x.citymunCode == code).FirstOrDefault();
            return rs;
        }
        [HttpGet("SearchCity/{str}")]
        [AllowAnonymous]
        public List<RefCityMun> SearchCity(string str)
        {
            return GetCity().Where(x => x.citymunDesc.Contains(str, StringComparison.OrdinalIgnoreCase) || x.citymunDesc.StartsWith(str, StringComparison.OrdinalIgnoreCase)).Take(5).ToList();
        }
        [HttpGet("SearchCity/{str}/{provCode}")]
        [AllowAnonymous]
        public List<RefCityMun> SearchCity(string str, string provCode)
        {
            return GetCity().Where(x => x.provCode == provCode && x.citymunDesc.Contains(str, StringComparison.OrdinalIgnoreCase)).Take(5).ToList();
        }
        [HttpGet("SearchProvince/{str}")]
        [AllowAnonymous]
        public List<RefProvince> SearchProvince(string str)
        {
            return GetProvince().Where(x => x.provDesc.Contains(str, StringComparison.OrdinalIgnoreCase) || x.provDesc.StartsWith(str, StringComparison.OrdinalIgnoreCase)).Take(5).ToList();
        }
        [HttpGet("GetBarangay/{citycode}")]
        [AllowAnonymous]
        public async Task<List<RefBrgy>> GetBarangay(string citycode)
        {
            var rs = (await GetAllBarangay()).Where(x => x.citymunCode == citycode).ToList();
            return rs.ToList();
        }
        [HttpGet("GetBarangay/{citycode}/{value}")]
        [AllowAnonymous]
        public async Task<List<RefBrgy>> GetBarangay(string citycode, string value)
        {
            var rs = (await GetAllBarangay()).Where(x => x.citymunCode == citycode).ToList();
            return rs.Where(x => x.brgyDesc.Contains(value, StringComparison.InvariantCultureIgnoreCase)).Take(10).ToList();
        }
        [HttpGet("GetSpecificBarangay/{brgyCode}")]
        [AllowAnonymous]
        public async Task<RefBrgy> GetSpecificBarangay(string brgyCode)
        {
            var rs = (await GetAllBarangay()).Where(x => x.brgyCode == brgyCode).FirstOrDefault();
            return rs;
        }
        [HttpGet("GetAllBarangay")]
        [AllowAnonymous]
        public async Task<List<RefBrgy>> GetAllBarangay()
        {
            var data = cache.Get<List<RefBrgy>>("barangay");
            if (data == null)
            {
                var file = Path.Combine(Directory.GetCurrentDirectory(), $"wwwroot\\{"json\\refbrgy.json"}");
                var json = await System.IO.File.ReadAllTextAsync(file);
                data = JsonConvert.DeserializeObject<List<RefBrgy>>(json);
                cache.Set("barangay", data);
            }
            return data;
        }
    }
}
