using webapi_peso.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_peso
{
    public static class ExtensionClass
    {
        public static IEnumerable<T> MyDistinctBy<T, TKey>(this IEnumerable<T> enumerable, Func<T, TKey> keySelector)
        {
            return enumerable.GroupBy(keySelector).Select(grp => grp.First());
        }
        public static string ToRegion(this string regCode, List<RefRegion> list)
        {
            return list.Where(x => x.regCode == regCode).Select(x=>x.regDesc).FirstOrDefault();
        }
        public static string ToCityMun(this string cityMunCode, List<RefCityMun> list)
        {
            return list.Where(x => x.citymunCode == cityMunCode).Select(x=>x.citymunDesc).FirstOrDefault();
        }
        public static string ToProvince(this string provCode, List<RefProvince> list)
        {
            return list.Where(x => x.provCode == provCode).Select(x=>x.provDesc).FirstOrDefault();
        }
        public static string ToBarangay(this string brgyCode, List<RefBrgy> list)
        {
            return list.Where(x => x.brgyCode == brgyCode).Select(x=>x.brgyDesc).FirstOrDefault();
        }
        public static string ToOwnGUID(this string guid)
        {
            string base64Guid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            return base64Guid;
        }
    }
}
