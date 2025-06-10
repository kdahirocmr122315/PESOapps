using webapi_peso.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class EmployerRegistrationViewModel
    {
        public EmployerDetails EmployerDetails { get; set; }
        public List<AttachementsViewModel> ListOfAttachments { get; set; }
    }
}
