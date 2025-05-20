using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi_peso.Model
{
    public class ApplicantProfessionalLicense
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string AccountId { get; set; }
        public string ProfName { get; set; }
        public DateTime? Validity { get; set; }
        public long DateLastUpdate { get; set; }
        public ApplicantProfessionalLicense()
        {
            DateLastUpdate = Helper.currentTimeMillis();
        }
    }
}
