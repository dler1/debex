using System;
using System.Collections.Generic;
using System.Data;
using Debex.Convert.Data;

namespace Debex.Convert.BL.CalculatingFields
{
    public class FieldToCalculate
    {
        public int Id { get; set; }
        public string ColumnName { get; set; }
        public Func<List<BaseFieldToMatch>, bool> ShouldCreate { get; set; }
        public bool Invisible { get; set; }
        public Func<DataRow, object> Calculator { get; set; }
        public bool OneIsError { get; set; }
        public bool NonZeroIsError { get; set; }

    }
}