using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debex.Convert.Data
{
    public class Buckets
    {
        public List<BucketSize> Dpd { get; set; }
        public List<BucketSize> Dplp { get; set; }
    }

    public class BucketSize
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public string Value { get; set; }
    }
}
