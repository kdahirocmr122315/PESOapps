using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace webapi_peso.Model
{
    public class ApplicantAccount
    {
        [Key]
        public int? AppAccountId { get; set; }
        public string? Id { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public int? IsEmailVerified { get; set; }
        public int? IsReviewedReturned { get; set; }
        public string? Remarks { get; set; }
        public int? IsRemoved { get; set; }
        public long DateRegistered { get; set; }
        public long? DateLastUpdate { get; set; }
        public ApplicantAccount()
        {
            DateRegistered = Helper.currentTimeMillis();
            DateLastUpdate = Helper.currentTimeMillis();
        }
    }
}
