using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Debex.Convert.Extensions
{
    public static class Serialization
    {
        public static T FromJson<T>(this string val)
        {
            return JsonConvert.DeserializeObject<T>(val);
        }

        public static string ToJson<T>(this T obj) => JsonConvert.SerializeObject(obj);
    }
}
