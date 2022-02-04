using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Accessibility;
using Debex.Convert.BL.CalculatingFields;
using Debex.Convert.BL.ExcelReading;
using Debex.Convert.Data;

namespace Debex.Convert.BL.FieldsChecking
{
    public class FieldsChecker
    {
        public async Task<List<CheckField>> CheckFields(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            return await Task.Run(() => Run(excel, fields));
        }

        private List<CheckField> Run(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {

            var checkers = FieldToCheckFactory.Create(excel, fields);
            var calculators = new List<FieldToCheck>();


            var checkedFieldsResult = checkers.ToDictionary(
                x => x.ColumnName,
                v => new CheckField()
                {
                    Name = Regex.Replace(v.ColumnName, @"Debex / Проверка\s\d{1,2}:\s{1}", string.Empty),
                    ColumnName = v.ColumnName
                });


            foreach (var fieldToCalc in checkers)
            {
                if (fieldToCalc.ShouldCreate(fields))
                {
                    if (!excel.Headers.ContainsKey(fieldToCalc.ColumnName))
                        excel.Data.Columns.Add(fieldToCalc.ColumnName, typeof(RowData));

                    checkedFieldsResult[fieldToCalc.ColumnName].HasData = true;
                    calculators.Add(fieldToCalc);
                }
            }

            excel.UpdateHeaders();

            for (int row = 0; row < excel.Data.Rows.Count; row++)
            {
                var curRow = excel.Data.Rows[row];

                foreach (var fieldToCalculate in calculators)
                {
                    if (fieldToCalculate.Calculator == null) continue;

                    var result = fieldToCalculate.Calculator?.Invoke(curRow);
                    var fieldResult = checkedFieldsResult[fieldToCalculate.ColumnName];
                    if (result == null)
                    {
                        fieldResult.Empty++;
                        result = -1;
                    }
                    if (result == -2)
                    {
                        result = null;
                    }

                    var colId = excel.Headers[fieldToCalculate.ColumnName];
                    excel.Data.Rows[row][colId] = new RowData { Value = result, IsError = result == -1 };


                    fieldResult.Total++;

                    ApplyResult(result, fieldResult);

                }
            }

            foreach (var fieldToCheck in calculators.Where(x => x.RepeatCalculator != null))
            {
                var data = new RepeatCalculatorData { };
                var toRetry = new List<int>();
                for (int row = 0; row < excel.Data.Rows.Count; row++)
                {
                    var curRow = excel.Data.Rows[row];
                    data.Row = curRow;
                    var result = fieldToCheck.RepeatCalculator(data);

                    var fieldResult = checkedFieldsResult[fieldToCheck.ColumnName];

                    if (result == -2)
                    {
                        toRetry.Add(row);
                        continue;
                    }
                    var colId = excel.Headers[fieldToCheck.ColumnName];
                    excel.Data.Rows[row][colId] = new RowData { Value = result, IsError = result == -1 };


                    fieldResult.Total++;
                    ApplyResult(result, fieldResult);


                }

                data.RepeatTryCount++;
                foreach (var row in toRetry)
                {
                    var curRow = excel.Data.Rows[row];
                    data.Row = curRow;
                    var result = fieldToCheck.RepeatCalculator(data);
                    var fieldResult = checkedFieldsResult[fieldToCheck.ColumnName];
                    var colId = excel.Headers[fieldToCheck.ColumnName];
                    excel.Data.Rows[row][colId] = new RowData { Value = result, IsError = result == -1 };


                    fieldResult.Total++;
                    ApplyResult(result, fieldResult);
                }

                checkedFieldsResult[fieldToCheck.ColumnName].OutputTable = data.OutputInfo;

            }


            var results = checkedFieldsResult.Values.ToList();

            
            return results;
        }

      


        private static void ApplyResult(int? result, CheckField fieldResult)
        {
            if (!result.HasValue) fieldResult.Empty++;
            else if (result == -1) fieldResult.Errors++;
            else if (result == 1) fieldResult.Correct++;
        }
    }
}