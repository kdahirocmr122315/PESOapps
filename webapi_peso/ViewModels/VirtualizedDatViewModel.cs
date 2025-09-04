using webapi_peso.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.ViewModels
{
    public class VirtualizedDatViewModel
    {
        public List<ApplicantInformation> Items { get; set; }
        public int TotalCount { get; set; }
    }
}
