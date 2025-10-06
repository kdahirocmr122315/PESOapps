using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace webapi_peso.Model
{
    public class ApplicantJobPrefWorkLocation
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string? AccountId { get; set; }
        public string WorkLocationName { get; set; }
        public string WorkLocationType { get; set; }
        public long DateLastUpdate { get; set; }
        public int IsRemoved { get; set; }
        public ApplicantJobPrefWorkLocation()
        {
            DateLastUpdate = Helper.currentTimeMillis();
        }
    }
}
