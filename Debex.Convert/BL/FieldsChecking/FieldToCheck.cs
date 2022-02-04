using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debex.Convert.Data;

namespace Debex.Convert.BL.FieldsChecking
{
    public class FieldToCheck
    {
        public int Id { get; set; }
        public string ColumnName { get; set; }
        public Func<List<BaseFieldToMatch>, bool> ShouldCreate { get; set; }
        public Func<DataRow, int?> Calculator { get; set; }
        public Func<RepeatCalculatorData, int?> RepeatCalculator { get; set; }
    }

    public class RepeatCalculatorData
    {
        public DataRow Row { get; set; }
        public object Parameter { get; set; }
        public int RepeatTryCount { get; set; }
        public RepeatOutputTable OutputInfo { get; set; }

    }

    public class RepeatOutputTable
    {
        public string Header { get; set; }
        public List<string> RowsHeaders { get; set; }
        public List<object[]> Rows { get; set; }
        public bool HasData => Rows?.Count > 0;
    }
}
