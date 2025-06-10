using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class PESOManualReport
    {
        public int Id { get; set; }
        public string MunicipalityCode { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Solicited { get; set; }
        public int SolicitedFemale { get; set; }
        public int Registered { get; set; }
        public int RegisteredFemale { get; set; }
        public int Referred { get; set; }
        public int ReferredFemale { get; set; }
        public int Placed { get; set; }
        public int PlacedFemale { get; set; }

    }
}
