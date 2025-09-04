using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class EmployerActiveStatus
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public string Id { get; set; }
        public string EmployerId { get; set; }
        public int Count { get; set; }
        public DateTime Date { get; set; }
        public EmployerActiveStatus()
        {
            Date = DateTime.Now;
        }
    }
}
