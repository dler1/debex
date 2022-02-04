using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debex.Convert.BL.ExcelReading;
using Debex.Convert.Data;

namespace Debex.Convert.Extensions
{
    public static class Object
    {

        public static bool IsOneOf<T>(this T val, params T[] oneOf) => oneOf.Contains(val);

        public static int GetColumnIndex(this BaseFieldToMatch field, ReadExcelResult excel)
        {
            if (field == null || !field.IsMatched) return -1;
            return excel.Headers[field.MatchedName];
        }

        public static bool Between(this DateTime dt, DateTime min, DateTime max)
        {
            return dt >= min && dt <= max;
        }

        public static bool Between(this DateTime? dt, DateTime min, DateTime max)
        {
            return dt >= min && dt <= max;
        }

        public static RunResult GetResult(this IEnumerable<IHasErrors> vals) => vals.Any(r => r.Errors > 0) ? RunResult.WithErrors : RunResult.Success;
        public static RunResult GetResult(this IEnumerable<RegionMatch> vals) => vals.Any(r => r.NotFound > 0) ? RunResult.WithErrors : RunResult.Success;
    }

}
