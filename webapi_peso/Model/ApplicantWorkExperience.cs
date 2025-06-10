using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi_peso.Model
{
    public class ApplicantWorkExperience
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string Position { get; set; }
        public DateTime? InclusiveDateFrom { get; set; }
        public DateTime? InclusiveDateTo { get; set; }
        public string Status { get; set; }
        public long DateLastUpdate { get; set; }
        public ApplicantWorkExperience()
        {
            DateLastUpdate = Helper.currentTimeMillis();
        }
    }
}
