using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi_peso.Model
{
    public class ApplicantExpectedSalary
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string AccountId { get; set; }
        public double? From { get; set; }
        public double? To { get; set; }
        public long DateLastUpdate { get; set; }
        public ApplicantExpectedSalary()
        {
            DateLastUpdate = Helper.currentTimeMillis();
        }
    }
}
