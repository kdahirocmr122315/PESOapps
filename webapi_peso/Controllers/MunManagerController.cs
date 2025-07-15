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
    public class MunManagerController : ControllerBase
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;
        private readonly IMemoryCache cache;
        private readonly IWebHostEnvironment env;

        public MunManagerController(IDbContextFactory<ApplicationDbContext> _dbFactory, IMemoryCache _cache, IWebHostEnvironment _env)
        {
            dbFactory = _dbFactory;
            cache = _cache;
            env = _env;
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

        [HttpGet("GetRegion")]
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

        [HttpGet("GetReferralReport/{year}/{isExport}")]
        public async Task<IActionResult> GetReferralReport(int year, bool isExport)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = new List<ConsolidatedReportViewModel>();
                var list = db.JobApplicantsReferred.Where(x => x.DateReferred.Year == year).MyDistinctBy(x => x.EmployerId).ToList();
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
                rs.AddRange(AddPESOProvinceData(db, noRow, year));

                if (isExport)
                {
                    var csv = new StringBuilder();
                    csv.AppendLine($"Generated Date:,{DateTime.Now.ToString("MM-dd-yyyy")},{DateTime.Now.ToString("hh:mmtt").ToUpper()}");
                    csv.AppendLine("NO.,MUNICIPALITY,APPLICANTS REFERRED");
                    if (rs.Count > 0)
                    {
                        foreach (var i in resultList.OrderBy(x => x.RowNumber))
                        {
                            var newLine = $"{i.RowNumber}, {i.MunicipalityName}, {i.NumberOfApplicants}";
                            csv.AppendLine(newLine);
                        }
                    }
                    var dir = System.IO.Path.Combine(env.WebRootPath, "files", "csv");
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
                    var fileName = System.IO.Path.Combine(dir, $"export_referred_applicants_{DateTime.Now.ToString("MMddyyyyHHmmss")}.csv");
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

                return Ok(resultList);
            }
        }

        [HttpGet("FindCity/{code}")]
        public RefCityMun FindCity(string code)
        {
            if (code == "PESO")
            {
                return new RefCityMun() { citymunCode = code, citymunDesc = "PESO Province", provCode = "1043" };
            }
            var rs = GetCity().Where(x => x.citymunCode == code).FirstOrDefault();
            return rs;
        }

        [HttpGet("GetHiredApplicantsList/{month}/{year}/{cityCode}/{isExport}")]
        public async Task<IActionResult> GetHiredApplicantsList(int month, int year, string cityCode, bool isExport)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var list = db.EmployerHiredApplicants.Where(x => x.DateHired.Month == month && x.DateHired.Year == year);
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
                    var dir = System.IO.Path.Combine(env.WebRootPath, "files", "csv");
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

        [HttpGet("GetSolicitedReportChart/{startMonth}/{endMonth}/{year}")]
        public IActionResult GetSolicitedReportChart(int startMonth, int endMonth, int year)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var solicited = db.JobVacancySolicited.Where(x => x.DateCreated.Month >= startMonth && x.DateCreated.Month <= endMonth && x.DateCreated.Year == year).ToList();

                return Ok(solicited);
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

        [HttpGet("GetHiredApplicantsListChart/{startMonth}/{endMonth}/{year}")]
        public IActionResult GetHiredApplicantsListChart(int startMonth, int endMonth, int year)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var list = db.JobApplicantsPlaced.Where(x => x.DateHired.Month >= startMonth && x.DateHired.Month <= endMonth && x.DateHired.Year == year).ToList();

                return Ok(list);
            }
        }

        [HttpGet("GetOverallMonthlyReport1/{year}/{myPage}/{isExport}/{title}")]
        public async Task<IActionResult> GetOverallMonthlyReport1(int year, string myPage, bool isExport, string title)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var listOfMunicipality = cache.Get<List<RefCityMun>>("list_city");
                if (listOfMunicipality == null)
                {
                    listOfMunicipality = GetCity().Where(x => x.provCode == "1043").ToList();
                    cache.Set("list_city", listOfMunicipality);
                }

                string key = $"GetOverallMonthlyReport1/{year}/{myPage}/{isExport}/{title}";
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
                                var d = Helper.toDateTime(x.DateLastUpdate);
                                if (d.Year == year && d.Month == i)
                                {
                                    registered += 1;
                                }
                            }

                            var solicited = db.JobVacancySolicited.Where(x => x.CityMunCode == city.citymunCode && x.Year == year && x.Month == i).Sum(x => x.NumberOfVacancy);
                            var referred = db.JobApplicantsReferred.Where(x => x.ApplicantCityMunCode == city.citymunCode && x.DateReferred.Year == year && x.DateReferred.Month == i).Count();
                            var placed = db.JobApplicantsPlaced.Where(x => x.CityMunCode == city.citymunCode && x.DateHired.Year == year && x.Month == i).Count();

                            var otherRegistered = db.PESOManualReport.Where(x => x.MunicipalityCode == city.citymunCode && x.Year == year && x.Month == i).Sum(x => x.Registered);
                            var otherSolicited = db.PESOManualReport.Where(x => x.MunicipalityCode == city.citymunCode && x.Year == year && x.Month == i).Sum(x => x.Solicited);
                            var otherReferred = db.PESOManualReport.Where(x => x.MunicipalityCode == city.citymunCode && x.Year == year && x.Month == i).Sum(x => x.Referred);
                            var otherPlaced = db.PESOManualReport.Where(x => x.MunicipalityCode == city.citymunCode && x.Year == year && x.Month == i).Sum(x => x.Placed);

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

                if (isExport)
                {
                    var csv = new StringBuilder();
                    csv.AppendLine($"Generated Date:,{DateTime.Now.ToString("MM-dd-yyyy")},{DateTime.Now.ToString("hh:mmtt").ToUpper()}");
                    csv.AppendLine($"MONTHLY REPORT BY MUNICIPALITY/CITY");
                    csv.AppendLine($"FOR THE YEAR {year}");
                    csv.AppendLine($"{title}");
                    csv.AppendLine("NO.,MUNICIPALITY, JAN, FEB, MAR, APR, MAY, JUN, JUL, AUG, SEP, OCT, NOV, DEC");
                    if (list.Count > 0)
                    {
                        foreach (var item in list.MyDistinctBy(x => x.RowNumber))
                        {
                            var data = list.Where(x => x.RowNumber == item.RowNumber).ToList();
                            var str = new StringBuilder();
                            str.Append($"{item.RowNumber}, {item.MunicipalityName}");
                            for (int i = 1; i <= 12; i++)
                            {
                                if (myPage == "solicited")
                                {
                                    var d = data.Where(x => x.Month == i).Sum(x => x.Solicited);
                                    str.Append($", {d}");
                                }
                                if (myPage == "registered")
                                {
                                    var d = data.Where(x => x.Month == i).Sum(x => x.Registered);
                                    str.Append($", {d}");
                                }
                                if (myPage == "referred")
                                {
                                    var d = data.Where(x => x.Month == i).Sum(x => x.Referred);
                                    str.Append($", {d}");
                                }
                                if (myPage == "placed")
                                {
                                    var d = data.Where(x => x.Month == i).Sum(x => x.Placed);
                                    str.Append($", {d}");
                                }

                            }
                            csv.AppendLine(str.ToString());

                        }
                    }
                    var dir = System.IO.Path.Combine(env.WebRootPath, "files", "csv");
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
                    var fileName = System.IO.Path.Combine(dir, $"export_solicited_{DateTime.Now.ToString("MMddyyyyHHmmss")}.csv");
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

                return Ok(list);
            }
        }

        [HttpGet("GetSolicitedReport1/{year}/{isExport}")]
        public async Task<IActionResult> GetSolicitedReport1(int year, bool isExport)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var listOfMunicipality = cache.Get<List<RefCityMun>>("list_city");
                if (listOfMunicipality == null)
                {
                    listOfMunicipality = GetCity().Where(x => x.provCode == "1043").ToList();
                    cache.Set("list_city", listOfMunicipality);
                }
                string key = $"GetSolicitedReport1/{year}/{isExport}";
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
                        var solicited = db.JobVacancySolicited.Where(x => x.CityMunCode == city.citymunCode && x.Year == year).Sum(x => x.NumberOfVacancy);
                        var pesoReport = db.PESOManualReport.Where(x => x.Year == year && x.MunicipalityCode == city.citymunCode).Sum(x => x.Solicited);
                        rs.Solicited = solicited + pesoReport;
                        list.Add(rs);
                    }
                    cache.Set(key, list, TimeSpan.FromMinutes(5));
                }

                //list.AddRange(AddPESOProvinceData(db, noRow, year));

                if (isExport)
                {
                    var csv = new StringBuilder();
                    csv.AppendLine($"Generated Date:,{DateTime.Now.ToString("MM-dd-yyyy")},{DateTime.Now.ToString("hh:mmtt").ToUpper()}");
                    csv.AppendLine($"CONSOLIDATED ANNUAL REPORT BY MUNICIPALITY/CITY");
                    csv.AppendLine($"JANUARY - DECEMBER {year}");
                    csv.AppendLine($"JOB VACANCIES SOLICITED");
                    csv.AppendLine("NO.,MUNICIPALITY,JOB VACANCIES SOLICITED");
                    if (list.Count > 0)
                    {
                        foreach (var i in list.OrderBy(x => x.RowNumber))
                        {
                            var newLine = $"{i.RowNumber}, {i.MunicipalityName}, {i.Solicited}";
                            csv.AppendLine(newLine);
                        }
                        var newLineTotal = $"TOTAL,-, {list.Sum(x => x.Solicited)}";
                        csv.AppendLine(newLineTotal);
                    }
                    var dir = System.IO.Path.Combine(env.WebRootPath, "files", "csv");
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
                    var fileName = System.IO.Path.Combine(dir, $"export_solicited_{DateTime.Now.ToString("MMddyyyyHHmmss")}.csv");
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

                return Ok(list);
            }
        }

        [HttpGet("GetReferredReport1/{year}/{isExport}")]
        public async Task<IActionResult> GetReferredReport1(int year, bool isExport)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var listOfMunicipality = cache.Get<List<RefCityMun>>("list_city");
                if (listOfMunicipality == null)
                {
                    listOfMunicipality = GetCity().Where(x => x.provCode == "1043").ToList();
                    cache.Set("list_city", listOfMunicipality);
                }
                string key = $"GetReferredReport1/{year}/{isExport}";
                var list = cache.Get<List<ConsolidatedReportViewModel>>(key);
                if (list == null)
                {
                    list = new();
                    int noRow = 0;
                    foreach (var city in listOfMunicipality)
                    {
                        noRow += 1;
                        var referred = db.JobApplicantsReferred.Where(x => x.ApplicantCityMunCode == city.citymunCode && x.DateReferred.Year == year).Count();
                        var rs = new ConsolidatedReportViewModel();
                        rs.RowNumber = noRow;
                        rs.MunicipalityName = city.citymunDesc;

                        var pesoReport = db.PESOManualReport.Where(x => x.Year == year && x.MunicipalityCode == city.citymunCode).Sum(x => x.Referred);

                        rs.Referred = referred + pesoReport;
                        list.Add(rs);
                    }
                    cache.Set(key, list, TimeSpan.FromMinutes(5));
                }

                //list.AddRange(AddPESOProvinceData(db, noRow, year));

                if (isExport)
                {
                    var csv = new StringBuilder();
                    csv.AppendLine($"Generated Date:,{DateTime.Now.ToString("MM-dd-yyyy")},{DateTime.Now.ToString("hh:mmtt").ToUpper()}");
                    csv.AppendLine($"CONSOLIDATED ANNUAL REPORT BY MUNICIPALITY/CITY");
                    csv.AppendLine($"JANUARY - DECEMBER {year}");
                    csv.AppendLine($"JOB APPLICANTS REFERRED");
                    csv.AppendLine("NO.,MUNICIPALITY, JOB APPLICANTS REFERRED");
                    if (list.Count > 0)
                    {
                        foreach (var i in list.OrderBy(x => x.RowNumber))
                        {
                            var newLine = $"{i.RowNumber}, {i.MunicipalityName}, {i.Referred}";
                            csv.AppendLine(newLine);
                        }
                        var newLineTotal = $"TOTAL,-, {list.Sum(x => x.Referred)}";
                        csv.AppendLine(newLineTotal);
                    }
                    var dir = System.IO.Path.Combine(env.WebRootPath, "files", "csv");
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
                    var fileName = System.IO.Path.Combine(dir, $"export_consolidated_{DateTime.Now.ToString("MMddyyyyHHmmss")}.csv");
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

                return Ok(list);
            }
        }

        [HttpGet("GetPlacedReport1/{year}/{isExport}")]
        public async Task<IActionResult> GetPlacedReport1(int year, bool isExport)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var listOfMunicipality = cache.Get<List<RefCityMun>>("list_city");
                if (listOfMunicipality == null)
                {
                    listOfMunicipality = GetCity().Where(x => x.provCode == "1043").ToList();
                    cache.Set("list_city", listOfMunicipality);
                }
                string key = $"GetPlacedReport1/{year}/{isExport}";
                var list = cache.Get<List<ConsolidatedReportViewModel>>(key);
                if (list == null)
                {
                    list = new();
                    int noRow = 0;
                    foreach (var city in listOfMunicipality)
                    {
                        noRow += 1;
                        var placed = db.JobApplicantsPlaced.Where(x => x.CityMunCode == city.citymunCode && x.DateHired.Year == year).Count();
                        var rs = new ConsolidatedReportViewModel();
                        rs.RowNumber = noRow;
                        rs.MunicipalityName = city.citymunDesc;

                        var pesoReport = db.PESOManualReport.Where(x => x.Year == year && x.MunicipalityCode == city.citymunCode).Sum(x => x.Placed);

                        rs.Placed = placed + pesoReport;
                        list.Add(rs);
                    }
                    cache.Set(key, list, TimeSpan.FromMinutes(5));
                }

                //list.AddRange(AddPESOProvinceData(db, noRow, year));

                if (isExport)
                {
                    var csv = new StringBuilder();
                    csv.AppendLine($"Generated Date:,{DateTime.Now.ToString("MM-dd-yyyy")},{DateTime.Now.ToString("hh:mmtt").ToUpper()}");
                    csv.AppendLine($"CONSOLIDATED ANNUAL REPORT BY MUNICIPALITY/CITY");
                    csv.AppendLine($"JANUARY - DECEMBER {year}");
                    csv.AppendLine($"JOB APPLICANTS PLACED");
                    csv.AppendLine("NO.,MUNICIPALITY, JOB APPLICANTS PLACED");
                    if (list.Count > 0)
                    {
                        foreach (var i in list.OrderBy(x => x.RowNumber))
                        {
                            var newLine = $"{i.RowNumber}, {i.MunicipalityName}, {i.Placed}";
                            csv.AppendLine(newLine);
                        }
                        var newLineTotal = $"TOTAL,-, {list.Sum(x => x.Placed)}";
                        csv.AppendLine(newLineTotal);
                    }
                    var dir = System.IO.Path.Combine(env.WebRootPath, "files", "csv");
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
                    var fileName = System.IO.Path.Combine(dir, $"export_consolidated_{DateTime.Now.ToString("MMddyyyyHHmmss")}.csv");
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

                return Ok(list);
            }
        }

        [HttpGet("GetRegisteredReport1/{year}/{isExport}")]
        public async Task<IActionResult> GetRegisteredReport1(int year, bool isExport)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var listOfMunicipality = cache.Get<List<RefCityMun>>("list_city");
                if (listOfMunicipality == null)
                {
                    listOfMunicipality = GetCity().Where(x => x.provCode == "1043").ToList();
                    cache.Set("list_city", listOfMunicipality);
                }
                string key = $"GetRegisteredReport1/{year}/{isExport}";
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
                            var d = Helper.toDateTime(i.DateLastUpdate);
                            if (d.Year == year)
                            {
                                registered += 1;
                            }
                        }
                        var pesoReport = db.PESOManualReport.Where(x => x.Year == year && x.MunicipalityCode == city.citymunCode).Sum(x => x.Registered);

                        rs.Registered = registered + pesoReport;
                        rs.RowNumber = noRow;
                        rs.MunicipalityName = city.citymunDesc;
                        list.Add(rs);
                    }
                    cache.Set(key, list, TimeSpan.FromMinutes(5));
                }

                //list.AddRange(AddPESOProvinceData(db, noRow, year));

                if (isExport)
                {
                    var csv = new StringBuilder();
                    csv.AppendLine($"Generated Date:,{DateTime.Now.ToString("MM-dd-yyyy")},{DateTime.Now.ToString("hh:mmtt").ToUpper()}");
                    csv.AppendLine($"CONSOLIDATED ANNUAL REPORT BY MUNICIPALITY/CITY");
                    csv.AppendLine($"JANUARY - DECEMBER {year}");
                    csv.AppendLine($"JOB APPLICANTS REGISTERED");
                    csv.AppendLine("NO.,MUNICIPALITY, JOB APPLICANTS REGISTERED");
                    if (list.Count > 0)
                    {
                        foreach (var i in list.OrderBy(x => x.RowNumber))
                        {
                            var newLine = $"{i.RowNumber}, {i.MunicipalityName}, {i.Registered}";
                            csv.AppendLine(newLine);
                        }
                        var newLineTotal = $"TOTAL,-, {list.Sum(x => x.Registered)}";
                        csv.AppendLine(newLineTotal);
                    }
                    var dir = System.IO.Path.Combine(env.WebRootPath, "files", "csv");
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
                    var fileName = System.IO.Path.Combine(dir, $"export_consolidated_{DateTime.Now.ToString("MMddyyyyHHmmss")}.csv");
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

                return Ok(list);
            }
        }

        [HttpGet("GetMonthlyReport1/{year}/{isExport}")]
        public async Task<IActionResult> GetMonthlyReport1(int year, bool isExport)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var listOfMunicipality = cache.Get<List<RefCityMun>>("list_city");
                if (listOfMunicipality == null)
                {
                    listOfMunicipality = GetCity().Where(x => x.provCode == "1043").ToList();
                    cache.Set("list_city", listOfMunicipality);
                }

                string key = $"GetMonthlyReport1/{year}/{isExport}";
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
                            var d = Helper.toDateTime(i.DateLastUpdate);
                            if (d.Year == year)
                            {
                                registered += 1;
                            }
                        }

                        var solicited = db.JobVacancySolicited.Where(x => x.CityMunCode == city.citymunCode && x.Year == year).Sum(x => x.NumberOfVacancy);
                        var referred = db.JobApplicantsReferred.Where(x => x.ApplicantCityMunCode == city.citymunCode && x.DateReferred.Year == year).Count();
                        var placed = db.JobApplicantsPlaced.Where(x => x.CityMunCode == city.citymunCode && x.DateHired.Year == year).Count();

                        var otherRegistered = db.PESOManualReport.Where(x => x.MunicipalityCode == city.citymunCode && x.Year == year).Sum(x => x.Registered);
                        var otherSolicited = db.PESOManualReport.Where(x => x.MunicipalityCode == city.citymunCode && x.Year == year).Sum(x => x.Solicited);
                        var otherReferred = db.PESOManualReport.Where(x => x.MunicipalityCode == city.citymunCode && x.Year == year).Sum(x => x.Referred);
                        var otherPlaced = db.PESOManualReport.Where(x => x.MunicipalityCode == city.citymunCode && x.Year == year).Sum(x => x.Placed);


                        var rs = new ConsolidatedReportViewModel();
                        rs.RowNumber = noRow;
                        rs.MunicipalityName = city.citymunDesc;
                        rs.Solicited = solicited + otherSolicited;
                        rs.Registered = registered + otherRegistered;
                        rs.Referred = referred + otherReferred;
                        rs.Placed = placed + otherPlaced;
                        list.Add(rs);
                    }
                    list.AddRange(AddPESOProvinceData(db, noRow, year));
                    cache.Set(key, list, TimeSpan.FromMinutes(5));
                }

                if (isExport)
                {
                    var csv = new StringBuilder();
                    csv.AppendLine($"Generated Date:,{DateTime.Now.ToString("MM-dd-yyyy")},{DateTime.Now.ToString("hh:mmtt").ToUpper()}");
                    csv.AppendLine($"CONSOLIDATED ANNUAL REPORT BY MUNICIPALITY/CITY");
                    csv.AppendLine($"FOR THE YEAR {year}");
                    csv.AppendLine("NO.,MUNICIPALITY,JOB VACANCIES SOLICITED, JOB APPLICANTS REGISTERED, JOB APPLICANTS REFERRED, JOB APPLICANTS PLACED");
                    if (list.Count > 0)
                    {
                        foreach (var i in list.OrderBy(x => x.RowNumber))
                        {
                            var newLine = $"{i.RowNumber}, {i.MunicipalityName}, {i.Solicited}, {i.Registered}, {i.Referred}, {i.Placed}";
                            csv.AppendLine(newLine);
                        }
                        var newLineTotal = $"TOTAL,-, {list.Sum(x => x.Solicited)}, {list.Sum(x => x.Registered)}, {list.Sum(x => x.Referred)}, {list.Sum(x => x.Placed)}";
                        csv.AppendLine(newLineTotal);
                    }
                    var dir = System.IO.Path.Combine(env.WebRootPath, "files", "csv");
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
                    var fileName = System.IO.Path.Combine(dir, $"export_consolidated_{DateTime.Now.ToString("MMddyyyyHHmmss")}.csv");
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

                return Ok(list);
            }
        }

        [HttpPost("GetPreRegList")]
        public IActionResult GetPreRegList(SearchApplicantsViewModel searchData)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = new List<ApplicantInformation>();
                //var list = db.ApplicantInformation.Where(x => !string.IsNullOrEmpty(x.JobFairReferenceCode)).ToList();
                long dateFrom = Helper.toUnixTime(searchData.DateFrom.Value);
                long dateTo = 0;
                try
                {
                    dateTo = Helper.toUnixTime(searchData.DateTo.Value);
                }
                catch (Exception) { }

                var list = db.ApplicantInformation.Join(db.ApplicantAccount, appInfo => appInfo.Email, appAccount => appAccount.Email, (appInfo, appAccount) => new AccountAndInformationViewModel() { ApplicantInformation = appInfo, ApplicantAccount = appAccount })
                    .Where(x => x.ApplicantInformation.Email == x.ApplicantAccount.Email && x.ApplicantAccount.DateRegistered >= dateFrom && x.ApplicantAccount.DateRegistered <= dateTo).OrderBy(x => x.ApplicantInformation.SurName).ToList();
                /*foreach (var i in list)
                {
                    
                    var appAccount = db.ApplicantAccount.Where(x =>
                        x.Email == i.Email &&
                        x.DateRegistered >= dateFrom &&
                        x.DateRegistered <= dateTo).FirstOrDefault();
                    if (appAccount != null)
                        rs.Add(i);
                }*/
                rs = list.Select(x => x.ApplicantInformation).ToList();
                if (searchData != null && !string.IsNullOrEmpty(searchData.ProvinceCode))
                {

                    rs = db.ApplicantInformation.Where(x =>
                        x.ProvincialProvince == searchData.ProvinceCode
                        && !string.IsNullOrEmpty(x.JobFairReferenceCode)).ToList();

                    if (!string.IsNullOrEmpty(searchData.CityCode))
                        rs = rs.Where(x =>
                        x.ProvincialMunicipalityCity == searchData.CityCode
                        && !string.IsNullOrEmpty(x.JobFairReferenceCode)).ToList();
                    if (!string.IsNullOrEmpty(searchData.BarangayCode))
                        rs = rs.Where(x =>
                        x.ProvincialBarangay == searchData.BarangayCode
                        && !string.IsNullOrEmpty(x.JobFairReferenceCode)).ToList();
                }

                return Ok(rs);
            }
        }

        [HttpGet("ValidateReferenceNumber/{refNumber}")]
        public IActionResult ValidateReferenceNumber(string refNumber)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = cache.Get<ApplicantInformation>($"ValidateReferenceNumber/{refNumber}");
                if (rs == null)
                {
                    rs = db.ApplicantInformation.Where(x => x.JobFairReferenceCode == refNumber).FirstOrDefault();
                    cache.Set($"ValidateReferenceNumber/{refNumber}", rs, TimeSpan.FromMinutes(1));
                }
                if (rs == null)
                    return NotFound();
                return Ok(rs);
            }
        }

        List<ConsolidatedReportViewModel> AddPESOProvinceData(ApplicationDbContext db, int noRow, int year, int month = 0)
        {
            var rs = new List<ConsolidatedReportViewModel>();
            var pesoReport = new PESOManualReport();
            if (month > 0)
            {
                pesoReport = db.PESOManualReport.Where(x => x.Year == year && x.Month == month).FirstOrDefault();
            }
            else
            {
                pesoReport = db.PESOManualReport.Where(x => x.Year == year).FirstOrDefault();
            }

            if (pesoReport == null)
            {
                pesoReport = new PESOManualReport();
                pesoReport.Month = month;
                pesoReport.Year = year;
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
    }
}
