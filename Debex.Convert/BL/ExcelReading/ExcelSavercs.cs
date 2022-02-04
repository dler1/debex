using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Debex.Convert.Annotations;
using Debex.Convert.Data;
using Debex.Convert.Environment;
using Debex.Convert.Extensions;
using DocumentFormat.OpenXml.Office2010.Excel;
using LargeXlsx;
using Color = System.Drawing.Color;

namespace Debex.Convert.BL.ExcelReading
{
    public class ExcelSaver
    {
        private readonly DialogService dialogService;

        public ExcelSaver(DialogService dialogService)
        {
            this.dialogService = dialogService;
        }

        public async Task<bool> Save(ReadExcelResult excel, List<BaseFieldToMatch> fields, List<CleanField> cleanFields,
            List<CalculatedField> calculatedFields, List<CheckField> checkFields)
        {

            var file = dialogService.SelectFileToSave();
            if (file.IsNullOrEmpty()) return false;

            return await Task.Run(() => SaveFile(file, excel, fields, cleanFields, calculatedFields, checkFields));
        }

        private bool SaveFile(string file, ReadExcelResult excel, List<BaseFieldToMatch> fields, List<CleanField> cleanFields, List<CalculatedField> calculatedFields, List<CheckField> checkFields)
        {

            var fi = new FileInfo(file);
            if (fi.Exists) fi.Delete();

            using var stream = new FileStream(file, FileMode.Create, FileAccess.Write);
            using var writer = new XlsxWriter(stream);

            var greenStyle = CreateStyle(fill: new XlsxFill(Color.LightGreen));
            var defaultStyle = CreateStyle();

            var data = excel.Data;
            writer.BeginWorksheet("Реестр");

            writer.BeginRow();
            var errorHeaders = GetErrorHeaders(data);

            for (int headerCol = 0; headerCol < data.Columns.Count; headerCol++)
            {

                var headerName = data.Columns[headerCol].ColumnName.Replace("❳", ")").Replace("❲", "(").Replace("「", "[").Replace("」", "]");
                var targetName = headerName;
                if (headerName == "Debex.info / сверка DPD (LIFO)") targetName = "Debex.info / разница дней DPD (LIFO)";
                else if (headerName == "Debex.info / сверка DPD (FIFO)") targetName = "Debex.info / разница дней DPD (FIFO)";

                var headersStyle = targetName.Contains("Debex") ? greenStyle : defaultStyle;

                if (errorHeaders.Contains(headerCol)) headersStyle = headersStyle.With(XlsxFont.Default.With(Color.Red));

                writer.Write(headerName, headersStyle);
            }

            writer.BeginRow();

            for (int headerCol = 0; headerCol < data.Columns.Count; headerCol++)
            {
                var headerName = data.Columns[headerCol].ColumnName;
                var headersStyle = headerName.Contains("Debex") ? greenStyle : defaultStyle;
                if (errorHeaders.Contains(headerCol)) headersStyle = headersStyle.With(XlsxFont.Default.With(Color.Red));

                var field = fields.FirstOrDefault(x => x.MatchedName == headerName);
                writer.Write(field?.Name ?? headerName, headersStyle);

            }

            var redStyle = CreateStyle(XlsxFont.Default.With(Color.Red));


            for (int row = 0; row < data.Rows.Count; row++)
            {
                writer.BeginRow();

                for (int col = 0; col < data.Columns.Count; col++)
                {
                    var val = data.Rows[row][col] as RowData;
                    var style = val?.IsError == true ? redStyle : defaultStyle;
                    if (val?.Value == null || val.Value == DBNull.Value)
                    {
                        writer.Write("", style);
                        continue;
                    }

                    if (val.Value is string str)
                    {
                        writer.Write(str, style);
                    }
                    else if (val.Value is decimal dc)
                    {
                        writer.Write(dc, style.With(XlsxNumberFormat.TwoDecimal));
                    }
                    else if (val.Value is DateTime dt)
                    {
                        writer.Write(dt, style.With(new XlsxNumberFormat("dd.MM.yyyy")));
                    }
                    else if (val.Value is int i)
                    {
                        writer.Write(i, style.With(XlsxNumberFormat.Integer));
                    }
                    else if (val.Value is double d)
                    {
                        style = Math.Abs(d % 1) <= (Double.Epsilon * 100) ? style.With(XlsxNumberFormat.Integer) : style.With(XlsxNumberFormat.TwoDecimal);
                        ;
                        writer.Write(d, style);
                    }
                    else
                    {

                    }

                }
            }

            WriteStatistics(writer, cleanFields, calculatedFields, checkFields);
            return true;

        }

        private void WriteStatistics(XlsxWriter writer, List<CleanField> cleanFields, List<CalculatedField> calculatedFields, List<CheckField> checkFields)
        {
            writer.BeginWorksheet("Статистика");

            var errorStyle = CreateStyle(font: XlsxFont.Default.With(Color.Red));
            var greenStyle = CreateStyle(font: XlsxFont.Default.With(Color.Green));
            var boldStyle = CreateStyle(font: XlsxFont.Default.WithBold());


            foreach (var checkField in checkFields.Where(x => x.OutputTable?.HasData == true))
            {
                writer.BeginRow();
                writer.Write(checkField.OutputTable.Header, boldStyle);
                writer.BeginRow();
                foreach (var header in checkField.OutputTable.RowsHeaders)
                {
                    writer.Write(header);
                }

                foreach (var row in checkField.OutputTable.Rows)
                {
                    var style = CreateStyle();
                    writer.BeginRow();
                    foreach (var obj in row)
                    {
                        if (obj is string str)
                        {
                            writer.Write(str, style);
                        }
                        else if (obj is decimal dc)
                        {
                            writer.Write(dc, style.With(XlsxNumberFormat.TwoDecimal));
                        }
                        else if (obj is DateTime dt)
                        {
                            writer.Write(dt, style.With(new XlsxNumberFormat("dd.MM.yyyy")));
                        }
                        else if (obj is int i)
                        {
                            writer.Write(i, style.With(XlsxNumberFormat.Integer));
                        }
                        else if (obj is double d)
                        {
                            style = Math.Abs(d % 1) <= (Double.Epsilon * 100) ? style.With(XlsxNumberFormat.Integer) : style.With(XlsxNumberFormat.TwoDecimal);
                            ;
                            writer.Write(d, style);
                        }
                        else
                        {

                        }
                    }
                }

            }

            writer.BeginRow();

            if (cleanFields.Any())
            {
                writer.BeginRow();
                writer.Write("Исправление формата данных", boldStyle);
                writer.BeginRow();
                foreach (var header in new[] { "", "Всего ячеек", "Заполнено", "Пустые", "Исправлено", "Ошибки формы" })
                {
                    writer.Write(header);
                }

                foreach (var cleanField in cleanFields)
                {
                    writer.BeginRow();
                    writer.Write(cleanField.Name, boldStyle);
                    writer.Write(cleanField.Total);
                    writer.Write(cleanField.Filled);
                    writer.Write(cleanField.Empty);
                    writer.Write(cleanField.Corrected);
                    writer.Write(cleanField.Errors, cleanField.Errors > 0 ? errorStyle : greenStyle);
                }
            }

            writer.BeginRow();

            if (calculatedFields.Any())
            {
                writer.BeginRow();
                writer.Write("Расчетные столбцы", boldStyle);
                writer.BeginRow();
                foreach (var header in new[] { "", "Всего ячеек", "Пустые", "Добавлено ячеек", "Ошибка в ячейке", "min error", "max error", "avg error" })
                    writer.Write(header);

                foreach (var calculatedField in calculatedFields)
                {
                    writer.BeginRow();
                    writer.Write(calculatedField.Name, boldStyle);
                    writer.Write(calculatedField.Total);
                    writer.Write(calculatedField.Empty);
                    writer.Write(calculatedField.Added);
                    writer.Write(calculatedField.Errors, calculatedField.Errors > 0 ? errorStyle : greenStyle);
                    if (calculatedField.HasDetailedErrors)
                    {
                        writer.Write(calculatedField.MinErrors, calculatedField.MinErrors > 0 ? errorStyle : greenStyle);
                        writer.Write(calculatedField.MaxErrors, calculatedField.MaxErrors > 0 ? errorStyle : greenStyle);
                        writer.Write(calculatedField.AvgErrors, calculatedField.AvgErrors > 0 ? errorStyle : greenStyle);
                    }
                }
            }

            writer.BeginRow();
            if (checkFields.Any())
            {
                writer.BeginRow();
                writer.Write("Проверка логики данных", boldStyle);

                writer.BeginRow();
                foreach (var header in new[] { "", "Всего ячеек", "Пустые", "Корректно", "Есть ошибка" })
                    writer.Write(header);

                foreach (var f in checkFields)
                {
                    writer.BeginRow();
                    writer.Write(f.Name, boldStyle);
                    writer.Write(f.Total);
                    writer.Write(f.Empty);
                    writer.Write(f.Correct);
                    writer.Write(f.Errors, f.Errors > 0 ? errorStyle : greenStyle);

                }
            }
        }

        private static HashSet<int> GetErrorHeaders(DataTable data)
        {
            var errorHeaders = new HashSet<int>();

            for (int col = 0; col < data.Columns.Count; col++)
            {
                for (int row = 0; row < data.Rows.Count; row++)
                {
                    if (data.Rows[row][col] is RowData rd && rd.IsError)
                    {
                        errorHeaders.Add(col);
                        break;
                    }
                }
            }

            return errorHeaders;
        }

        private XlsxStyle CreateStyle(
            XlsxFont font = null,
            XlsxFill fill = null,
            XlsxBorder border = null,
            XlsxNumberFormat numberFormat = null,
            XlsxAlignment alignment = null)
        {
            return new XlsxStyle(font ?? XlsxFont.Default, fill ?? XlsxFill.None, border ?? XlsxBorder.None,
                numberFormat ?? XlsxNumberFormat.Text, alignment ?? XlsxAlignment.Default);
        }
    }
}
