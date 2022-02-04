using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debex.Convert.Data
{
    public enum SortType
    {
        Error,
        Corrected,
        RegionMatched,
        RegionNotMatched,
        CalculatedError,
        CheckedError
    }

    public record SortInfo(string ColumnName, SortType Type);
}
