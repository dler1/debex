using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation;
using System.Windows.Controls.Ribbon;
using Debex.Convert.BL.ExcelReading;
using Debex.Convert.BL.FieldsChecking;
using Debex.Convert.Data;
using Debex.Convert.Extensions;

namespace Debex.Convert.BL.CalculatingFields
{
    public class FieldsCalculator
    {
        private readonly CalculatedFieldsFactory calculatedFieldsFactory;

        public FieldsCalculator(CalculatedFieldsFactory calculatedFieldsFactory)
        {
            this.calculatedFieldsFactory = calculatedFieldsFactory;
        }

        public async Task<List<CalculatedField>> CalculateFields(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            return await Task.Run(() =>
            {
                var errorsWhenNotZero = new HashSet<string>
                {
                    CalculatedFieldsFactory.DpdFifoName,
                    CalculatedFieldsFactory.DpdLifoName,
                    CalculatedFieldsFactory.OszCheck,
                    CalculatedFieldsFactory.CalculatedTotalDebtHeader
                };

                var fieldsToCalculate = calculatedFieldsFactory.CreateFieldsToCalculates(excel, fields);
                var calculators = new List<FieldToCalculate>();

                var calculatedFieldsResult = fieldsToCalculate.ToDictionary(
                    x => x.ColumnName,
                    v => new CalculatedField
                    {
                        Name = v.ColumnName.Replace("Debex.info / ", string.Empty).Trim().FirstLetterUpper(),
                        ColumnName = v.ColumnName
                    });

                foreach (var fieldToCalc in fieldsToCalculate)
                {
                    if (fieldToCalc.ShouldCreate(fields))
                    {
                        if (!excel.Headers.ContainsKey(fieldToCalc.ColumnName))
                            excel.Data.Columns.Add(fieldToCalc.ColumnName, typeof(RowData));

                        calculators.Add(fieldToCalc);
                    }
                    else if (fieldToCalc.Invisible)
                    {
                        calculatedFieldsResult.Remove(fieldToCalc.ColumnName);
                    }
                }

                excel.UpdateHeaders();

                for (int row = 0; row < excel.Data.Rows.Count; row++)
                {
                    var curRow = excel.Data.Rows[row];

                    foreach (var fieldToCalculate in calculators)
                    {
                        var result = fieldToCalculate.Calculator?.Invoke(curRow);


                        var isError = (result is int r && fieldToCalculate.OneIsError && r == 1) || result == null;

                        if (fieldToCalculate.NonZeroIsError)
                        {
                            var nonZeroCheck =
                                result is int ir && ir != 0 ||
                                result is double dr && dr != 0 ||
                                result is decimal der && der != 0 ||
                                result is float fr && fr != 0;

                            if (nonZeroCheck) isError = true;
                        }

                        var colId = excel.Headers[fieldToCalculate.ColumnName];
                        excel.Data.Rows[row][colId] = new RowData { Value = result, IsError = isError };

                        var fieldResult = calculatedFieldsResult[fieldToCalculate.ColumnName];

                        fieldResult.Total++;

                        if (isError) fieldResult.Errors++;
                        else if (result is string str && str.IsNullOrEmpty()) fieldResult.Empty++;
                        else fieldResult.Added++;


                    }
                }

                var results = calculatedFieldsResult.Values.ToList();
                CheckErrors(excel, fields, results);

                return results;
            });
        }

        private void CheckErrors(ReadExcelResult excel, List<BaseFieldToMatch> fields, List<CalculatedField> checkedFields)
        {
            var d = new List<string>
            {
                CalculatedFieldsFactory.DpdFifoName,
                CalculatedFieldsFactory.DpdLifoName,
                CalculatedFieldsFactory.OszCheck,
                CalculatedFieldsFactory.Flag554
            };


            foreach (var key in d)
            {
                var target = checkedFields.FirstOrDefault(x => x.ColumnName == key);
                if (target == null)
                {
                    continue;
                }

                if (!excel.Headers.TryGetValue(key, out var from)) continue;


                double? min = null, max = null, avg = 0;
                int count = 0;


                for (int row = 0; row < excel.Data.Rows.Count; row++)
                {

                    var value = excel.Data.Rows[row][from] as RowData;

                    if (value == null)
                    {
                        continue;
                    }

                    if (value.Value is string s && !double.TryParse(s, out var _))
                    {
                        continue;
                    }

                    double targetValue = System.Convert.ToDouble(value.Value);

                    var absValue = Math.Abs(targetValue);
                    if (absValue < 1) continue;

                    count++;

                    min = min.HasValue ? Math.Min(absValue, min.Value) : absValue;
                    max = max.HasValue ? Math.Max(absValue, max.Value) : absValue;
                    avg += absValue;

                }

                target.HasDetailedErrors = true;
                target.MinErrors = Math.Round(min ?? 0, 2);
                target.MaxErrors = Math.Round(max ?? 0, 2);
                target.AvgErrors = count > 0 ? Math.Round(avg.Value / count) : 0;
            }
        }
    }
}