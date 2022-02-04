using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debex.Convert.Environment
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class HasViewModelAttribute : Attribute
    {
        public HasViewModelAttribute()
        {

        }
    }
}
