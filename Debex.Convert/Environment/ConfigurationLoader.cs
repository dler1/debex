using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Debex.Convert.Data;
using Debex.Convert.Extensions;

namespace Debex.Convert.Environment
{
    public class ConfigurationLoader
    {
        private const string BaseFieldsPath = "Assets/Data/baseFields.json";
        private const string BaseBucketsPath = "Assets/Data/buckets.json";

        private readonly Lazy<Buckets> buckets;
        private readonly Lazy<List<BaseField>> baseFields;
        public List<BaseField> BaseFields => baseFields.Value;
        public Buckets Buckets => buckets.Value;
        
        public ConfigurationLoader()
        {
            baseFields = new Lazy<List<BaseField>>(LoadBaseFileds);
            buckets = new Lazy<Buckets>(LoadBuckets);
        }


        private Buckets LoadBuckets() => File.ReadAllText(BaseBucketsPath).FromJson<Buckets>();
        private List<BaseField> LoadBaseFileds()
        {
            var fields = File.ReadAllText(BaseFieldsPath).FromJson<List<BaseField>>();
            return fields;
        }
    }
}
