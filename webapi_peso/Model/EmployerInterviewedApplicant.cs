using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class EmployerInterviewedApplicant
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string EmployerId { get; set; }
        public string ApplicantAccountId { get; set; }
        public DateTime DateInterviewed { get; set; }
        public EmployerInterviewedApplicant()
        {
            DateInterviewed = DateTime.Now;
        }
    }
}
