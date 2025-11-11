using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using webapi_peso.Model;
using webapi_peso.ViewModels;

namespace webapi_peso.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerController : ControllerBase
    {
        private readonly IDbContextFactory<ApplicationDbContext> dbFactory;
        private readonly IMemoryCache cache;
        private readonly IWebHostEnvironment env;

        private static ViewModels.MailSettings _mailConfig = new();
        private static readonly string EMAIL_FOOTER = "<br /><br /><center style='color:red'>**********  This is a system generated email. Please do not reply.  **********</center><hr /><center><h1>Contact us</h1></center><center>pesomisamisoriental@gmail.com</center><center>(+63) 909 503 6246</center><center>(088)72-57-19</center><center>Opens: Monday - Friday</center><center>From 8:00AM - 5:00PM only</center><center><a href='https://pesomisor.com'>www.pesomisor.com</a> - <a href='https://www.misamisoriental.gov.ph'>Misamis Oriental</a></center>";
        private bool EnableEmailAndText = true;
       

        public ManagerController(IDbContextFactory<ApplicationDbContext> _dbFactory, IMemoryCache _cache, IWebHostEnvironment _env)
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

                var listofapplicants = db.ApplicantAccount.ToList();
                var appInfo = db.ApplicantInformation.ToList();
                var filteredList = listofapplicants.Where(x => x.IsReviewedReturned == 1 && x.IsRemoved == 0).ToList();
                foreach (var i in filteredList)
                {
                    var filteredAppInfo = appInfo.Where(x => x.AccountId == i.Id && x.PresentProvince == information.ProvCode && x.PresentMunicipalityCity == information.CityCode).MyDistinctBy(x => x.Email);
                    if (filteredAppInfo != null)
                        rs.NumberOfApplicants += filteredAppInfo.Count();
                }
                var employerList = db.EmployerDetails.ToList();
                rs.NumberOfEmployers = employerList.Where(x => x.Province == information.ProvCode && x.CityMunicipality == information.CityCode).Count();

                return Ok(rs);
            }
        }

        [HttpPost("UpdatePESOManagerAccount")]
        public IActionResult UpdatePESOManagerAccount(PESOManagerAccountViewModel data)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                db.UserAccounts.Update(data.UserAccount);
                db.PesoManagerInformation.Update(data.UserInformation);
                db.SaveChanges();

                return Ok(data);
            }
        }

        [HttpGet("GetNumberOfEmplyerAndApplicant/{provCode}/{cityCode}")]
        public IActionResult GetNumberOfEmplyerAndApplicant(string provCode, string cityCode)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = new PESOManagerAccountViewModel();
                var listofapplicants = db.ApplicantAccount.ToList();
                var appInfo = db.ApplicantInformation.ToList();
                //var listofapplicants = db.ApplicantAccount.Where(x => x.IsReviewedReturned == 1 && x.IsRemoved == 0).ToList();
                var filteredList = listofapplicants.Where(x => x.IsReviewedReturned == 1 && x.IsRemoved == 0).ToList();
                foreach (var i in filteredList)
                {
                    //var appInfo = db.ApplicantInformation.Where(x => x.AccountId == i.Id && x.PresentProvince == provCode && x.PresentMunicipalityCity == cityCode).MyDistinctBy(x => x.Email);
                    var filteredAppInfo = appInfo.Where(x => x.AccountId == i.Id && x.PresentProvince == provCode && x.PresentMunicipalityCity == cityCode).MyDistinctBy(x => x.Email);
                    if (filteredAppInfo != null)
                        rs.NumberOfApplicants += filteredAppInfo.Count();
                }
                //rs.NumberOfApplicants = db.ApplicantInformation.Where(x => x.PresentProvince == provCode && x.PresentMunicipalityCity == cityCode).Count();
                var employerList = db.EmployerDetails.ToList();
                rs.NumberOfEmployers = employerList.Where(x => x.Province == provCode && x.CityMunicipality == cityCode).Count();

                return Ok(rs);
            }
        }

        [HttpGet("GetListOfEmployers/{provCode}/{cityCode}")]
        public IActionResult GetListOfEmployers(string provCode, string cityCode)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = new List<EmployerDetails>();
                var listOfEmployers = db.EmployerDetails.ToList();
                rs = listOfEmployers.Where(x => x.Province == provCode && x.CityMunicipality == cityCode).ToList();
                return Ok(rs);
            }
        }

        [HttpGet("GetListOfApplicants/{provCode}/{cityCode}")]
        public IActionResult GetListOfApplicants(string provCode, string cityCode)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = new List<ApplicantInformation>();
                var listOfEmployers = db.ApplicantInformation.ToList();
                rs = listOfEmployers.Where(x => x.PresentProvince == provCode && x.PresentMunicipalityCity == cityCode).ToList();
                return Ok(rs);
            }
        }

        [HttpGet]
        [Route("GetNSRP/pending")]
        public List<AccountAndInformationViewModel> GetNSRP()
        {
            using var db = dbFactory.CreateDbContext();
            var rs = cache.Get<List<AccountAndInformationViewModel>>("GetNSRP");
            if (rs == null)
            {
                rs = new List<AccountAndInformationViewModel>();
                var listofaccount = db.ApplicantAccount.Where(x => (x.IsReviewedReturned == 0 || x.IsReviewedReturned == 2) && x.IsRemoved == 0).OrderByDescending(x => x.DateLastUpdate).OrderBy(x => x.IsReviewedReturned).ToList();
                foreach (var account in listofaccount)
                {
                    var information = db.ApplicantInformation.Where(x => x.AccountId == account.Id).FirstOrDefault();
                    if (information != null)
                    {
                        rs.Add(new AccountAndInformationViewModel()
                        {
                            ApplicantAccount = account,
                            ApplicantInformation = information
                        });
                    }
                }
                cache.Set("GetNSRP", rs, TimeSpan.FromSeconds(30));
            }

            return rs;
        }

        [HttpGet]
        [Route("GetNSRP/verified/{distinct}/{pageIndex}/{pageSize}")]
        public Pagination<ApplicantInformation> GetNSRPverified(bool distinct, int pageIndex = 1, int pageSize = 10)
        {
            int startIndex = pageIndex * pageSize;
            var cacheKey = $"GetNSRP/verified/{distinct}/{pageIndex}/{pageSize}";
            var db = dbFactory.CreateDbContext();
            var list = db.ApplicantInformation.Where(x => db.ApplicantAccount.Any(a => a.IsReviewedReturned == 1 && a.IsRemoved == 0 && x.AccountId == a.Id)).Skip(startIndex).Take(pageSize).OrderByDescending(x => x.DateLastUpdate);
            var rs = cache.Get<IEnumerable<ApplicantInformation>>(cacheKey);
            if (rs == null)
            {
                rs = new List<ApplicantInformation>();
                rs = distinct ? list.MyDistinctBy(x => x.Email) : list;
                cache.Set(cacheKey, rs, TimeSpan.FromSeconds(30));
            }
            var paged = new Pagination<ApplicantInformation>();
            paged.TotalAllItems = db.ApplicantInformation.Where(x => db.ApplicantAccount.Any(a => a.IsReviewedReturned == 1 && a.IsRemoved == 0 && x.AccountId == a.Id)).Count();
            paged.TotalResultItems = rs.Count();
            paged.Page = pageIndex;
            paged.PageSize = pageSize;
            paged.Results = rs.ToList();
            return paged;
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

        [HttpGet("GetListSkills")]
        public IActionResult GetListSkills()
        {
            using var db = dbFactory.CreateDbContext();
            var rs = cache.Get<List<string>>($"GetListSkills");
            if (rs == null)
            {
                rs = new List<string>();
                rs = db.ApplicantOtherSkills.MyDistinctBy(x => x.SkillName).Select(x => x.SkillName).ToList();
                rs.OrderBy(x => x);
                cache.Set($"GetListSkills", rs, TimeSpan.FromSeconds(30));
            }
            return Ok(rs);
        }

        [HttpGet("GetRegisteredApplicants/{month}/{year}/{isExport}")]
        public async Task<IActionResult> GetRegisteredApplicants(int month, int year, bool isExport)
        {
            using var db = dbFactory.CreateDbContext();
            var dTo = Helper.toUnixTime(DateTime.Parse($"{year}-{month}-{DateTime.DaysInMonth(year, month)}"));
            var dFrom = Helper.toUnixTime(DateTime.Parse($"{year}-{month}-1"));
            var list = new List<ApplicantInformation>();
            var listAppAcct = db.ApplicantAccount.Where(x => x.DateRegistered >= dFrom && x.DateRegistered <= dTo).ToList();
            foreach (var i in listAppAcct)
            {
                var a = db.ApplicantInformation.Where(x => x.AccountId == i.Id).FirstOrDefault();
                if (a != null)
                {
                    list.Add(a);
                }
            }
            if (isExport)
            {
                var csv = new StringBuilder();
                csv.AppendLine($"Generated Date:,{DateTime.Now.ToString("MM-dd-yyyy")},{DateTime.Now.ToString("hh:mmtt").ToUpper()}");
                csv.AppendLine($"FOR THE MONTH OF {Helper.ToMonthName(month).ToUpper()} {year}");
                csv.AppendLine("FIRSTNAME,LASTNAME,GENDER,AGE,PHONE NUMBER,EMAIL,RELIGION,CIVIL STATUS,PLACE OF BIRTH");
                if (list.Count > 0)
                {
                    foreach (var i in list.OrderBy(x => x.SurName))
                    {
                        var age = Helper.GetAge(Helper.toDateTime(i.DateOfBirth));
                        var newLine = $"{i.FirstName}, {i.SurName}, {i.Gender}, {age}, {i.CellphoneNumber}, {i.Email}, {i.Religion},{i.CivilStatus}, {i.PlaceOfBirth}";
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
                var fileName = System.IO.Path.Combine(dir, $"export_{DateTime.Now.ToString("MMddyyyyHHmmss")}.csv");
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

        [HttpGet("GetPreRegList1/{provCode}/{cityCode}")]
        public IActionResult GetPreRegList1(string provCode, string cityCode)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var query = db.ApplicantInformation.AsQueryable();

                // ✅ Apply province filter if selected
                if (!string.IsNullOrEmpty(provCode) && provCode != "0")
                    query = query.Where(x => x.ProvincialProvince == provCode);

                // ✅ Apply city filter if selected
                if (!string.IsNullOrEmpty(cityCode) && cityCode != "0")
                    query = query.Where(x => x.PresentMunicipalityCity == cityCode);

                var listApp = query.ToList();

                if (listApp == null || !listApp.Any())
                    return NotFound("No applicants found for the specified filters.");

                return Ok(listApp);
            }
        }

        [HttpGet("GetReferralStatus")]
        public IActionResult GetReferralStatus()
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = new ReferralViewModel();
                var lastData = db.JobApplicantsReferred.OrderByDescending(x => x.DateReferred).Take(1).FirstOrDefault();
                if (lastData != null)
                {
                    rs.LastDateSubmitted = lastData.DateReferred;
                    rs.IsAlreadySubmittedToday = lastData.DateReferred.Date == DateTime.Now.Date;
                }
                return Ok(rs);
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

        [HttpGet("GetEmployersJobPosted")]
        public IActionResult GetEmployersJobPosted()
        {
            using var db = dbFactory.CreateDbContext();
            var rs = new List<JobPostViewModel>();
            var list = db.EmployerJobPost.Where(x => x.Expiry.HasValue && x.Expiry <= DateTime.Now).ToList();
            foreach (var i in list)
            {
                var emp = db.EmployerDetails.Where(x => x.Id == i.EmployerDetailsId).FirstOrDefault();
                if (emp != null)
                {
                    rs.Add(new JobPostViewModel()
                    {
                        EmpDetails = emp,
                        JobPost = i
                    });
                }
            }
            return Ok(rs);
        }

        [HttpPost("SaveReferral")]
        public async Task<IActionResult> SaveReferral(ReferralViewModel data)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var emails = new List<EmailAddress>();
                foreach (var employer in data.Employers)
                {
                    emails.Add(new EmailAddress(employer.ContactEmailAddress));
                    foreach (var applicant in data.Applicants)
                    {
                        var refer = new JobApplicantsReferred();
                        refer.EmployerId = employer.Id;
                        refer.ApplicantAccountId = applicant.ApplicantInformation.Id;
                        refer.ApplicantCityMunCode = applicant.ApplicantInformation.PresentMunicipalityCity;
                        refer.Gender = applicant.ApplicantInformation.Gender;
                        refer.JobTitle = !string.IsNullOrEmpty(data.Skill) ? data.Skill : "Any";
                        db.JobApplicantsReferred.Add(refer);
                        db.SaveChanges();
                    }
                }
#if !DEBUG
                var body = data.Message +
                   "<br/><br/><br/>" +
                   $"{EMAIL_FOOTER}";
                var client = new SendGridClient(SENGRID_API_KEY);
                var msg = MailHelper.CreateSingleEmailToMultipleRecipients(Emailfrom, emails, "REFERRED APPLICANTS", "", body);
                await client.SendEmailAsync(msg);
#endif
                return Ok();
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

        [HttpPost("ExportCSV_PreRegisteredJobFair")]
        public async Task<IActionResult> ExportCSV_PreRegisteredJobFair(SearchApplicantsViewModel sData)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                var rs = new List<ApplicantDataViewModel>();
                long dFrom = Helper.toUnixTime(sData.DateFrom.Value.Date);
                long dTo = Helper.toUnixTime(sData.DateTo.Value.Date);

                var list = db.ApplicantAccount.Where(x => x.DateRegistered >= dFrom && x.DateRegistered <= dTo).ToList();
                //var list = db.ApplicantInformation.Where(x => !string.IsNullOrEmpty(x.JobFairReferenceCode)).ToList();
                foreach (var i in list)
                {
                    var data = new ApplicantDataViewModel();
                    var appInfo = db.ApplicantInformation.Where(x => x.Email == i.Email && !string.IsNullOrEmpty(x.JobFairReferenceCode)).FirstOrDefault();
                    if (appInfo != null)
                    {
                        data.ApplicantAccount = i;
                        data.ApplicantInformation = appInfo;
                        rs.Add(data);
                    }
                }
                var csv = new StringBuilder();
                csv.AppendLine($"Generated Date:,From:{sData.DateFrom.Value.ToString("MM-dd-yyyy")},To:{sData.DateTo.Value.ToString("MM-dd-yyyy").ToUpper()}");
                csv.AppendLine("No,LastName,FirstName,Code,Registered");
                if (rs.Count > 0)
                {
                    int count = 0;
                    foreach (var i in rs.OrderBy(x => x.ApplicantInformation.SurName))
                    {
                        count += 1;
                        var newLine = $"{count}, {i.ApplicantInformation.SurName}, {i.ApplicantInformation.FirstName}, {i.ApplicantInformation.JobFairReferenceCode}, {Helper.toDateTime(i.ApplicantAccount.DateRegistered).ToString("MM-dd-yyyy")}";
                        csv.AppendLine(newLine);
                    }
                }
                var dir = System.IO.Path.Combine(env.WebRootPath, "files", "csv");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                var fileName = System.IO.Path.Combine(dir, $"{DateTime.Now.ToString("MMddyyyyHHmmss")}.csv");
                using (StreamWriter sw = new StreamWriter(System.IO.File.Open(fileName, FileMode.Create), Encoding.UTF8))
                {
                    await sw.WriteAsync(csv.ToString());
                }
                var url = $"{ProjectConfig.API_HOST}/files/csv/{System.IO.Path.GetFileName(fileName)}";

                return Ok(url);
            }

        }

        [HttpGet("GetEstablishments/{status}")]
        public IActionResult GetEstablishments(ProjectConfig.ACCOUNT_STATUS status)
        {
            using var db = dbFactory.CreateDbContext();
            var rs = cache.Get<List<EmployerDetailsViewModel>>($"GetEstablishments/{status}");
            if (rs == null)
            {
                rs = new List<EmployerDetailsViewModel>();
                var list = db.EmployerDetails.Include(x => x.JobPosts).Where(x => x.Status == status).OrderBy(x => x.DateCreated).ToList();
                foreach (var i in list)
                {
                    var hired = db.EmployerHiredApplicants.Where(x => x.EmployerId == i.Id).Count();
                    var interviewed = db.EmployerInterviewedApplicants.Where(x => x.EmployerId == i.Id).Count();
                    rs.Add(new EmployerDetailsViewModel
                    {
                        EmployerDetails = i,
                        Hired = hired,
                        Interviewed = interviewed
                    });
                }
                cache.Set($"GetEstablishments/{status}", rs, TimeSpan.FromSeconds(30));
            }

            return Ok(rs);
        }

        [HttpGet("GetEstablishments/{month}/{year}/{isExport}")]
        public async Task<IActionResult> GetEstablishments(int month, int year, bool isExport)
        {
            using var db = dbFactory.CreateDbContext();
            var dTo = DateTime.Parse($"{month}/{DateTime.DaysInMonth(year, month)}/{year}");
            var dFrom = DateTime.Parse($"{month}/1/{year}");
            var list = db.EmployerDetails.Where(x => x.Status == ProjectConfig.ACCOUNT_STATUS.APPROVED && x.DateCreated >= dFrom && x.DateCreated <= dTo).ToList();
            if (isExport)
            {
                var csv = new StringBuilder();
                csv.AppendLine($"Generated Date:,{DateTime.Now.ToString("MM-dd-yyyy")},{DateTime.Now.ToString("hh:mmtt").ToUpper()}");
                csv.AppendLine($"FOR THE MONTH OF {Helper.ToMonthName(month).ToUpper()} {year}");
                csv.AppendLine("ESTABLISHMENT, ABBREVIATION, ADDRESS, LINE OF BUSINESS, CONTACT NAME, CONTACT NUMBER, CONTACT EMAIL");
                if (list.Count > 0)
                {
                    foreach (var i in list.OrderBy(x => x.EstablishmentName))
                    {
                        var newLine = $"{i.EstablishmentName}, {i.AcronymAbbreviation}, {i.Address}, {i.LineOfBusiness}, {i.ContactFullName}, {i.ContactMobileNo},{i.ContactEmailAddress}";
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
                var fileName = System.IO.Path.Combine(dir, $"export_{DateTime.Now.ToString("MMddyyyyHHmmss")}.csv");
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

        [HttpGet("GetEmails")]
        public IActionResult GetEmails()
        {
            using var db = dbFactory.CreateDbContext();
            var rs = cache.Get<List<AdminEmployerEmails>>($"GetEmails");
            if (rs == null)
            {
                rs = new List<AdminEmployerEmails>();
                rs = db.AdminEmployerEmails.Where(x => x.InActive == 0).OrderByDescending(x => x.DateAdded).ToList();
                cache.Set($"GetEmails", rs, TimeSpan.FromSeconds(30));
            }
            return Ok(rs);
        }

        [HttpPost("AddEmails")]
        public IActionResult AddEmails(List<AdminEmployerEmails> list)
        {
            using var db = dbFactory.CreateDbContext();
            db.AdminEmployerEmails.AddRange(list);
            db.SaveChanges();
            return Ok();
        }

        [HttpGet("RemoveEmail/{Id}")]
        public IActionResult RemoveEmail(string Id)
        {
            using var db = dbFactory.CreateDbContext();
            var em = db.AdminEmployerEmails.Find(Id);
            if (em != null)
            {
                db.AdminEmployerEmails.Remove(em);
                db.SaveChanges();
            }
            return Ok();
        }

        [HttpGet("SendEmailToEmployer")] //need validation
        public async Task<IActionResult> SendEmailToEmployer()
        {
            if (!EnableEmailAndText)
                return Ok();
            using var db = dbFactory.CreateDbContext();
            var listofemails = db.AdminEmployerEmails.ToList();
            foreach (var i in listofemails)
            {
                if (!string.IsNullOrEmpty(i.Emails))
                {
                    MailMessage message = new MailMessage();
                    SmtpClient smtp = new SmtpClient();
                    message.From = new MailAddress(_mailConfig.FromEmail);
                    message.To.Add(new MailAddress(i.Emails));
                    message.Subject = "[Website] PESO Misamis Oriental";
                    message.IsBodyHtml = true;
                    message.Body = "" +
                        "<div style='padding-left:20px;padding-right:20px'>" +
                        $"<h2 style='font-family:Century Gothic;font-weight:300'>Good day Ma’am/Sir:</h2>" +
                        $"<h2 style='font-family:Century Gothic;font-weight:300'>Please be informed that PESO Misamis Oriental has created an Online Skills Registry and Referral Program which will not only benefit the jobseekers, but the employers as well, for their manpower requirement.</h2>" +
                        $"<h2 style='font-family:Century Gothic;font-weight:300'>We will be sending you qualified applicants, as part of our employment facilitation service, hence, this request for registration of your company/establishment in our system. (Please visit https://pesomisor.com/reg/employer). </h2>" +
                        $"<h2 style='font-family:Century Gothic;font-weight:300'>Thank you very much for this partnership and cooperation, as we commit to provide fast and effective employment service.</h2>" +
                        $"<h2 style='font-family:Century Gothic;font-weight:300'>Have a great day ahead!!!</h2>" +
                        $"{EMAIL_FOOTER}" +
                        "</div>";
                    await SendEmail(message.Subject, i.Emails, message.Body);
                    db.AdminEmployerEmails.Update(i);
                    db.SaveChanges();
                    /* smtp.Port = _mailConfig.Port;
                     smtp.Host = _mailConfig.Host;
                     smtp.EnableSsl = true;
                     smtp.UseDefaultCredentials = false;
                     smtp.Credentials = new NetworkCredential(_mailConfig.Username, _mailConfig.Password);
                     smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                     await smtp.SendMailAsync(message);*/
                }
            }
            return Ok();
        }

        [HttpGet("SendEmail/{subject}/{mailTo}/{body}")]
        public async Task SendEmail(string subject, string mailTo, string body)
        {

            if (ProjectConfig.JobFairEnabled)
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();
                message.From = new MailAddress(_mailConfig.FromEmail);
                message.To.Add(new MailAddress(mailTo));
                message.Subject = subject;
                message.IsBodyHtml = true;
                message.Body = body;
                smtp.Port = _mailConfig.Port;
                smtp.Host = _mailConfig.Host;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(_mailConfig.Username.Trim(), _mailConfig.Password.Trim());
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                await smtp.SendMailAsync(message);
            }
        }

        [HttpGet("GetEmployerAttachments")]
        public IActionResult GetEmployerAttachments()
        {
            using var db = dbFactory.CreateDbContext();
            var rs = cache.Get<List<EstablishmentViewModel>>($"GetEmployerAttachments");
            if (rs == null)
            {
                rs = new List<EstablishmentViewModel>();
                var list = db.EmployerDetails.Where(x => x.Status == ProjectConfig.ACCOUNT_STATUS.APPROVED).OrderBy(x => x.LastUpdate).ToList();
                foreach (var i in list)
                {
                    var dir = System.IO.Path.Combine(env.WebRootPath, "files", "employers", i.Id);
                    if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                    rs.Add(new EstablishmentViewModel
                    {
                        EmployerDetails = i,
                        AttachmentsCount = Directory.GetFiles(dir).Count()
                    });
                }
                cache.Set($"GetEmployerAttachments", rs, TimeSpan.FromSeconds(30));
            }

            return Ok(rs);
        }

        [HttpPost("UploadAttachments")]
        public IActionResult UploadAttachments()
        {
            var files = Request.Form.Files;
            var result = new List<AttachementsViewModel>();
            var folderName = Request.Headers.Where(x => x.Key == "f").Select(x => x.Value).FirstOrDefault().ToString();
            var dir = System.IO.Path.Combine(env.WebRootPath, "files", "employers", folderName);
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var origFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                    var fileName = $"{origFileName}";
                    fileName = WebUtility.HtmlEncode(fileName);
                    var fullPath = System.IO.Path.Combine(dir, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        result.Add(new AttachementsViewModel()
                        {
                            FileName = fileName,
                            FileSize = Helper.SizeSuffix(file.Length),
                            FolderName = folderName
                        });
                        file.CopyTo(stream);
                    }
                }

            }
            return Ok(result);
        }

        [HttpPost("RemoveUploadedFile")]
        public IActionResult RemoveUploadedFile(AttachementsViewModel a)
        {
            var dir = System.IO.Path.Combine(env.WebRootPath, "files", "employers", a.Id);
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
            var fileToRemove = System.IO.Path.Combine(dir, a.FileName);
            if (System.IO.File.Exists(fileToRemove))
                System.IO.File.Delete(fileToRemove);
            return Ok();
        }

        [HttpGet("GetEmployerAttachments/{EmpId}")]
        public IActionResult GetEmployerAttachments(string EmpId)
        {
            using var db = dbFactory.CreateDbContext();
            var rs = cache.Get<List<AttachementsViewModel>>($"GetEmployerAttachments/{EmpId}");
            if (rs == null)
            {
                rs = new List<AttachementsViewModel>();
                var dir = System.IO.Path.Combine(env.WebRootPath, "files", "employers", EmpId);
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                foreach (var i in Directory.GetFiles(dir))
                {
                    var fileInfo = new FileInfo(i);
                    rs.Add(new AttachementsViewModel
                    {
                        FileName = Path.GetFileName(i),
                        FileSize = Helper.SizeSuffix(fileInfo.Length)
                    });
                }
                cache.Set($"GetEmployerAttachments/{EmpId}", rs, TimeSpan.FromSeconds(30));
            }
            return Ok(rs);
        }

        [HttpPost("SetJobFairStatus")]
        public async Task<IActionResult> SetJobFairStatus([FromBody] JobFairEnableLogViewModel accountId)
        {
            using var db = dbFactory.CreateDbContext();

            // Find the JobFairEnable record with Id = "2025"
            var jobFair = db.JobFairEnable.FirstOrDefault(x => x.Id == "2025");
            if (jobFair == null)
                return NotFound("JobFairEnable record not found.");

            // Toggle the status (assuming 0 = disabled, 1 = enabled)
            int newStatus = jobFair.JobFairStatus == 1 ? 0 : 1;
            jobFair.JobFairStatus = newStatus;
            db.JobFairEnable.Update(jobFair);

            // Log the action
            var log = new JobFairEnableLog
            {
                Id = Guid.NewGuid().ToString(),
                JobFairStatus = newStatus,
                AccountId = accountId.AccountId,
                DateUpdate = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            db.JobFairEnableLog.Add(log);

            await db.SaveChangesAsync();

            return Ok(new { jobFair.JobFairStatus });
        }

        [HttpGet("GetJobFairStatus")]
        public IActionResult GetJobFairStatus()
        {
            using var db = dbFactory.CreateDbContext();
            var jobFair = db.JobFairEnable.FirstOrDefault(x => x.Id == "2025");
            if (jobFair == null)
                return NotFound("JobFairEnable record not found.");
            return Ok(new { jobFair.JobFairStatus });
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

        [HttpGet("GetHiredApplicantsList/{month}/{year}/{cityCode}/{isExport}")]
        public async Task<IActionResult> GetHiredApplicantsList(int month, int year, string cityCode, bool isExport)
        {
            using (var db = dbFactory.CreateDbContext())
            {
                // ✅ Fetch all first (prevents open DataReader issue)
                var list = db.EmployerHiredApplicants
                    .Where(x => x.DateHired.Month == month && x.DateHired.Year == year)
                    .ToList();

                var rs = new List<JobApplicantsPlaced>();

                foreach (var hired in list)
                {
                    var emp = db.EmployerDetails
                        .FirstOrDefault(x => x.Id == hired.EmployerId && x.CityMunicipality == cityCode);
                    var applicant = db.ApplicantInformation
                        .FirstOrDefault(x => x.AccountId == hired.ApplicantAccountId);

                    if (emp != null && applicant != null)
                    {
                        rs.Add(new JobApplicantsPlaced
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
                    csv.AppendLine($"Generated Date:,{DateTime.Now:MM-dd-yyyy},{DateTime.Now:hh:mmtt}");
                    csv.AppendLine("No,NAME OF APPLICANT,AS (Position),TO (Employer)");

                    if (rs.Count > 0)
                    {
                        int count = 0;
                        foreach (var i in rs.OrderBy(x => x.DateHired))
                        {
                            count++;
                            csv.AppendLine($"{count},{i.ApplicantName},{i.JobTitle},{i.Company}");
                        }
                    }

                    var dir = Path.Combine(env.WebRootPath, "files", "csv");
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                    try
                    {
                        foreach (var item in Directory.GetFiles(dir))
                            System.IO.File.Delete(item);
                    }
                    catch { }

                    var fileName = Path.Combine(dir, $"export_placed_applicants_{DateTime.Now:MMddyyyyHHmmss}.csv");

                    await System.IO.File.WriteAllTextAsync(fileName, csv.ToString(), Encoding.UTF8);

                    var url = $"{ProjectConfig.API_HOST}/files/csv/{Path.GetFileName(fileName)}";
                    return Ok(url);
                }

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

        //GetAddress -----------------------------------------

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
                // Go up one directory from webapi_peso and into PESOapps.Shared/wwwroot/json/
                var file = Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory()).FullName,
                    "PESOapps.Shared",
                    "wwwroot",
                    "json",
                    "refcitymun.json"
                );

                if (!System.IO.File.Exists(file))
                    throw new FileNotFoundException($"JSON file not found: {file}");

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
