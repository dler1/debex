using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Debex.Convert.BL.ExcelReading;

namespace Debex.Convert.Extensions
{
    public static class DataTableExtensions
    {
        public static void ChangeValue(this DataTable dt, int row, int column, object value, bool isError = false, bool isCorrected = false)
        {
            var val = (RowData)dt.Rows[row][column];
            val.Value = value;
            val.IsError = isError;
            val.IsCorrected = isCorrected;
            val.OnChange();
        }

        public static object GetValue(this DataTable dt, int row, int column) => ((RowData)dt.Rows[row][column]).Value;
        public static object GetValue(this DataRow row, int col) => ((RowData)row[col]).Value;

        public static T GetValue<T>(this DataRow row, int col)
        {
            var rowData = (RowData)row[col];
            var val = rowData.Value;
            return val is T ? (T)val : default;
        }

        public static void SetRegionMatched(this DataTable dt, int row, int column)
        {
            var val = ((RowData)dt.Rows[row][column]);
            val.IsRegionMatched = true;
            val.OnChange();
        }

        public static bool IsAnyError(this DataRow row, params int[] cols)
        {
            foreach (var col in cols)
            {
                var rd = (RowData)row[col];
                if (rd?.IsError == true) return true;
            }

            return false;
        }
    }
}
