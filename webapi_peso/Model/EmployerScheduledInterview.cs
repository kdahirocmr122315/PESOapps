using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class EmployerScheduledInterview
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string EmployerId { get; set; }
        public string ApplicantId { get; set; }
        public DateTime DateSchedule { get; set; }
        public DateTime DateCreated { get; set; }
        public EmployerScheduledInterview()
        {
            DateCreated = DateTime.Now;
        }
    }
}
