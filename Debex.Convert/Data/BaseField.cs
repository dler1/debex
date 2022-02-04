using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debex.Convert.Data
{
    public interface IHasId { int Id { get; set; } }
    public class BaseField : IHasId
    {
        public enum BaseFieldType
        {
            Date,
            String,
            Int,
            Decimal
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public BaseFieldType Type { get; set; }
        public bool? Required { get; set; }
        public bool? RegionCheck { get; set; }
    }
}
