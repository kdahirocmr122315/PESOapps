using webapi_peso.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class EmployerDetailsViewModel
    {
        public EmployerDetails EmployerDetails { get; set; }
        public int Hired { get; set; }
        public int Interviewed { get; set; }
        public int CurrentPage { get; set; }
        public EmployerActiveStatus EmployerActiveStatus { get; set; }

    }
}
