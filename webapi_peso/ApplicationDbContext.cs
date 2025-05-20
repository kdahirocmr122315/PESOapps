using Microsoft.EntityFrameworkCore;
using webapi_peso.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webapi_peso
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<ApplicantAccount> ApplicantAccount { get; set; }
        public DbSet<ApplicantEducationalBackground> ApplicantEducationalBackground { get; set; }
        public DbSet<ApplicantEligibility> ApplicantEligibility { get; set; }
        public DbSet<ApplicantExpectedSalary> ApplicantExpectedSalary { get; set; }
        public DbSet<ApplicantInformation> ApplicantInformation { get; set; }
        public DbSet<ApplicantJobPrefOccupation> ApplicantJobPrefOccupation { get; set; }
        public DbSet<ApplicantJobPrefWorkLocation> ApplicantJobPrefWorkLocation { get; set; }
        public DbSet<ApplicantLanguageDialectProf> ApplicantLanguageDialectProf { get; set; }
        public DbSet<ApplicantOtherSkills> ApplicantOtherSkills { get; set; }
        public DbSet<ApplicantProfessionalLicense> ApplicantProfessionalLicense { get; set; }
        public DbSet<ApplicantTechnicalVocational> ApplicantTechnicalVocational { get; set; }
        public DbSet<ApplicantWorkExperience> ApplicantWorkExperience { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<StaffActivity> StaffActivities { get; set; }
        public DbSet<EmployerDetails> EmployerDetails { get; set; }
        public DbSet<EmployerJobPost> EmployerJobPost { get; set; }
        public DbSet<UserAccount> UserAccounts { get; set; }
        public DbSet<ApplicantUpdateRequest> ApplicantUpdateRequest { get; set; }
        public DbSet<UpdateDetailsSession> UpdateDetailsSessions { get; set; }
        public DbSet<EmployerInterviewedApplicant> EmployerInterviewedApplicants { get; set; }
        public DbSet<EmployerHiredApplicant> EmployerHiredApplicants { get; set; }
        public DbSet<EmployerScheduledInterview> EmployerScheduledInterviews { get; set; }
        public DbSet<AdminEmployerEmails> AdminEmployerEmails { get; set; }
        public DbSet<EmployerActiveStatus> EmployerActiveStatus { get; set; }
        public DbSet<UserSuggestion> UserSuggestions { get; set; }
        public DbSet<JobApplicantion> JobApplicantion { get; set; }
        public DbSet<JobApplicantionAttachment> JobApplicantionAttachment { get; set; }
        public DbSet<VerificationCode> VerificationCode { get; set; }
        public DbSet<PesoManagerInformation> PesoManagerInformation { get; set; }
        public DbSet<JobVacancySolicited> JobVacancySolicited { get; set; }
        public DbSet<JobApplicantsPlaced> JobApplicantsPlaced { get; set; }
        public DbSet<JobApplicantsReferred> JobApplicantsReferred { get; set; }
        public DbSet<PESOManualReport> PESOManualReport { get; set; }
    }
}
