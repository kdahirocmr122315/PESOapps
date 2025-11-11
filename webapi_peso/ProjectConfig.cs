using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso
{
    public class ProjectConfig
    {
        public const bool IS_UNDER_MAINTENANCE = false;
#if !DEBUG
        public static string HOST = "https://pesomisor.com";
        public static string API_HOST = "https://pesoapi.misamisoriental.gov.ph";
        public static readonly bool IsProduction = true;
#else
        public static string HOST = "https://localhost:44360";
        public static string API_HOST = "http://localhost:5167";
        public static readonly bool IsProduction = false;
#endif
        public const bool JobFairEnabled = true;
        public enum USER_TYPE
        {
            EMPLOYER,
            PESO_PERSONNEL, 
            SYSTEM_ADMINISTRATOR,
            APPLICANT,
            PESO_MANAGER
        }
        public enum ACCOUNT_STATUS
        {
            PENDING,
            APPROVED,
            DENIED,
            RETURNED
        }

        public enum REPORT_TYPE
        {
            ALL,
            SOLICITED,
            REGISTERED,
            REFERRED,
            PLACED
        }
        public ProjectConfig()
        {
            if (!IsProduction)
            {
                HOST = "https://localhost:44360";
                API_HOST = "http://localhost:5167";
            }
        }
    }
}
