using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class EmployerJobPost
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string? Id { get; set; }
        public string? Description2 { get; set; }
        public string? Description { get; set; }
        public string? ImageName { get; set; }
        public int? IsVacant { get; set; }
        public string? EmployerDetailsId { get; set; }
        public int? NumberOfVacancy { get; set; }
        public int? AgeFrom { get; set; }
        public int? AgeTo { get; set; }
        public string? Gender { get; set; }
        public string? CivilStatus { get; set; }
        public string? EducationalAttainment { get; set; }
        public string? WorkExperience { get; set; }
        public bool? ReasonExpansion { get; set; }
        public bool? ReasonReplaceMent { get; set; }
        public bool? ReasonOthers { get; set; }
        public bool? IsDeleted { get; set; }
        public double? Salary { get; set; }
        public DateTime DatePosted { get; set; }
        public DateTime? Expiry { get; set; }
        public EmployerJobPost()
        {
            DatePosted = DateTime.Now;
        }
    }
}
