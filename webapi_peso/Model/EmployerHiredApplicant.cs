using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class EmployerHiredApplicant
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string EmployerId { get; set; }
        public string ApplicantAccountId { get; set; }
        public string? HiredPosition { get; set; }
        public DateTime DateHired { get; set; }
        public EmployerHiredApplicant()
        {
            DateHired = DateTime.Now;
        }
    }
}
