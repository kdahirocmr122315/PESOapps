using webapi_peso.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class AccountAndInformationViewModel
    {
        public UserAccount? UserAccount { get; set; }
        public ApplicantAccount? ApplicantAccount { get; set; }
        public ApplicantInformation? ApplicantInformation { get; set; }
    }
}
