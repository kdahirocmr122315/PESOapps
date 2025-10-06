using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi_peso.Model
{
    public class ApplicantEligibility
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string? AccountId { get; set; }
        public string EligibilityName { get; set; }
        public int Rating { get; set; }
        public DateTime? DateOfExamination { get; set; }
        public long DateLastUpdate { get; set; }
        public ApplicantEligibility()
        {
            DateLastUpdate = Helper.currentTimeMillis();
        }
    }
}
