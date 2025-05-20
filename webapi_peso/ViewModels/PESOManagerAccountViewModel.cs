using webapi_peso.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class PESOManagerAccountViewModel
    {
        public PESOManagerAccountViewModel()
        {
            UserInformation = new PesoManagerInformation();
        }
        public UserAccount UserAccount { get; set; }
        public PesoManagerInformation UserInformation { get; set; }

        public int NumberOfApplicants { get; set; }
        public int NumberOfEmployers { get; set; }
    }
}
