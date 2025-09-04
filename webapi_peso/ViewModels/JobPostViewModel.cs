using webapi_peso.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class JobPostViewModel
    {
        public EmployerDetails? EmpDetails { get; set; }
        public EmployerJobPost? JobPost { get; set; }
        public string? FirstImage { get; set; }
    }
}
