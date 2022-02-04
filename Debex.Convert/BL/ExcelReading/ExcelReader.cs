using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Debex.Convert.Annotations;
using Debex.Convert.Data;
using ExcelDataReader;

namespace Debex.Convert.BL.ExcelReading
{
    public class ReadExcelResult
    {
        public Dictionary<string, int> Headers { get; set; }
        public DataTable Data { get; set; }


        public int GetColumnId(BaseFieldToMatch field) => Headers[field.MatchedName];
        
        public void UpdateHeaders()
        {
            Headers.Clear();
            for (int i = 0; i < Data.Columns.Count; i++) Headers[Data.Columns[i].ColumnName] = i;
        }

    }

    public class RowData : INotifyPropertyChanged
    {
        public object Value { get; set; }
        public bool IsError { get; set; }
        public bool IsCorrected { get; set; }
        public bool IsRegionMatched { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnChange()
        {
            OnPropertyChanged(nameof(Value));
            OnPropertyChanged(nameof(IsError));
            OnPropertyChanged(nameof(IsCorrected));
            OnPropertyChanged(nameof(IsRegionMatched));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class ExcelReader
    {

        public async Task<ReadExcelResult> ReadExcel(string filePath)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return await Task.Run(() =>
            {

                using var file = File.OpenRead(filePath);
                using var reader = ExcelReaderFactory.CreateOpenXmlReader(file, new ExcelReaderConfiguration { });
                var ds = reader.AsDataSet(new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration { UseHeaderRow = true },
                    UseColumnDataType = false
                });

                var headers = new Dictionary<string, int>();
                var data = ds.Tables[0];

                var dataTable = new DataTable();

                for (int colIndex = 0; colIndex < data.Columns.Count; colIndex++)
                {
                    var col = data.Columns[colIndex];
                    var colColumnName = col.ColumnName.Replace("(", "❲").Replace(")", "❳").Replace("[", "「").Replace("]", "」");
                    headers[colColumnName] = colIndex;
                    dataTable.Columns.Add(new DataColumn(colColumnName, typeof(RowData)));
                }

                foreach (DataRow row in data.Rows)
                {
                    var dataRow = dataTable.NewRow();
                    for (int columnIndex = 0; columnIndex < row.ItemArray.Length; columnIndex++)
                    {
                        dataRow[columnIndex] = new RowData { Value = row[columnIndex] };
                    }

                    dataTable.Rows.Add(dataRow);
                }


                return new ReadExcelResult
                {
                    Headers = headers,
                    Data = dataTable
                };
            });
        }
    }
}
