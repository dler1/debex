using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Debex.Convert.BL.ExcelReading;
using Debex.Convert.Data;
using Debex.Convert.Environment;
using Debex.Convert.Extensions;

namespace Debex.Convert.BL.Stages.CleanFormatsStage
{
    public class FormatCleaner
    {

        private readonly Dictionary<BaseField.BaseFieldType, Func<object, AdditionalData, FormatResult>> processors;
        private readonly Logger logger;

        public FormatCleaner(Logger logger)
        {
            this.logger = logger;

            processors = new()
            {
                [BaseField.BaseFieldType.Date] = FormatDate,
                [BaseField.BaseFieldType.Int] = FormatInt,
                [BaseField.BaseFieldType.Decimal] = FormatDecimal,
                [BaseField.BaseFieldType.String] = FormatString,
            };
        }

        public async Task<List<CleanField>> FormatData(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            return await Task.Run(() =>
            {
                var matchedFields = fields.Where(x => x.IsMatched).ToList();
                var results = new List<CleanField>();

                foreach (var matchedField in matchedFields)
                {
                    var column = excel.Headers[matchedField.MatchedName];
                    var rows = excel.Data.Rows.Count;

                    var additionalData = new AdditionalData();
                    var toRepeatAfterParse = new List<(int row, int col, FormatResult result)>();
                    var cleanedField = new CleanField { Name = matchedField.Name };
                    results.Add(cleanedField);
                    var processor = processors[matchedField.Type];

                    for (int row = 0; row < rows; row++)
                    {
                        var value = excel.Data.GetValue(row, column);

                        if (value is string str && str.IsOneOf("*", "#", "-", "-", "N/A", "NULL", "н/д"))
                        {
                            ProcessResult(excel, cleanedField, FormatResult.Empty(), row, column);
                            continue;
                        }

                        var processed = processor(value, additionalData);

                        if (processed.Result == ResultCode.RepeatAfter)
                        {
                            toRepeatAfterParse.Add((row, column, processed));
                            continue;
                        }

                        ProcessResult(excel, cleanedField, processed, row, column);

                    }

                    foreach (var itemToRepeat in toRepeatAfterParse)
                    {
                        var processed = processor(itemToRepeat.result.Data, additionalData);
                        if (processed.Result == ResultCode.RepeatAfter) processed.Result = ResultCode.Error;
                        ProcessResult(excel, cleanedField, processed, itemToRepeat.row, itemToRepeat.col);
                    }

                }

                return results;
            });
        }

        private void ProcessResult(ReadExcelResult excel, CleanField cleanedField, FormatResult processed, int row, int column)
        {

            logger.LogIf($"До:{excel.Data.GetValue(row, column)} После: {processed.Data} - {processed.Message}", processed.Message.NotNullOrEmpty());
            cleanedField.Total++;

            excel.Data.ChangeValue(row, column, processed.Data, processed.Result == ResultCode.Error, processed.Result == ResultCode.Corrected);

            if (processed.Result == ResultCode.Correct)
            {
                cleanedField.Filled++;
                return;
            }


            if (processed.Result == ResultCode.Empty) cleanedField.Empty++;
            if (processed.Result == ResultCode.Error) cleanedField.Errors++;

            if (processed.Result == ResultCode.Corrected)
            {
                cleanedField.Filled++;
                cleanedField.Corrected++;
            }
        }


        private FormatResult FormatDate(object value, AdditionalData additionalData)
        {
            if (value is DateTime) return FormatResult.Correct(value);


            var str = value?.ToString()?.Trim()?.Replace("?", string.Empty);
            if (str.IsNullOrEmpty() || str == "0") return FormatResult.Empty();

            var constsFormatsDate = str.TryParseDate(new[] { "yyyy-MM-dd HH:mm:ss" });
            if (constsFormatsDate.HasValue)
            {
                return FormatResult.Correct(constsFormatsDate.Value.Date);
            }
            if (additionalData.Format.NotNullOrEmpty())
            {
                var dt = str.TryParseDate(additionalData.Format);
                return dt.HasValue
                    ? FormatResult.Corrected(dt.Value)
                    : FormatResult.Error(value, $"Значение не соответствует формату {additionalData.Format}");
            }

            var separators = str.Where(x => !char.IsDigit(x)).ToHashSet();
            if (separators.Count != 1) return FormatResult.Error(value, "Не удалось разбить дату на составляющие");

            var splittedStrings = str.Split(separators.Single()).ToList();
            var splitted = splittedStrings.Select(x => x.ToInt()).ToList();

            if (splitted.Count != 3) return FormatResult.Error(value, $"Числовых компонентов {splitted.Count}. Ожидается 3 компонента - год, месяц, день");

            var year = splitted.FirstOrDefault(x => x > 1000);
            if (year == 0) return FormatResult.Error(value, "Не удалось определить год");

            var day = splitted.FirstOrDefault(x => x > 12 && x < 32);
            if (day == 0) return FormatResult.RepeatAfter(value, "Не удалось однозначно определить день, повторная попытка будет осуществленна после подбора формата");

            var month = splitted.FirstOrDefault(x => x >= 1 && x <= 12);
            if (month == 0) return FormatResult.Error(value, "Не удалось определить месяц");

            var resultDt = new DateTime(year, month, day);

            var yearStr = splittedStrings.FirstOrDefault(x => x.ToInt() == year);
            var monthStr = splittedStrings.FirstOrDefault(x => x.ToInt() == month);
            var dayStr = splittedStrings.FirstOrDefault(x => x.ToInt() == day);

            additionalData.Format = str.Replace($"{dayStr}", "dd").Replace($"{monthStr}", "MM").Replace($"{yearStr}", "yyyy");
            return FormatResult.Corrected(resultDt, $"Формат успешно распознан {additionalData.Format}");

        }

        private FormatResult FormatInt(object value, AdditionalData additionalData)
        {
            if (value is int) return FormatResult.Correct(value);

            if (value is double db)
            {
                return db % 1 == 0
                    ? FormatResult.Correct((int)db)
                    : FormatResult.Corrected((int)Math.Round(db), "целое число получено путем округления");
            }

            if (value is float f) return FormatResult.Correct((int)Math.Round(f), "целое число получено путем округления");
            if (value is decimal d) return FormatResult.Correct((int)(Math.Round(d)), "целое число получено путем округления");

            var str = value?.ToString()?.Trim()?.Replace("?", string.Empty);
            if (str.IsNullOrEmpty()) return FormatResult.Empty();

            var parsed = str.ToInt(int.MinValue);
            if (parsed != int.MinValue) return FormatResult.Corrected(parsed, "значение получено из строки");

            var separator = str.Where(x => !char.IsDigit(x)).ToHashSet();
            if (separator.Count != 1) return FormatResult.Error(value, $"Найдено несколько неизвестных символов {separator.JoinToString(",")}");

            var coerced = str.Replace(separator.Single(), ',');
            var toFloat = coerced.ToFloat();
            if (toFloat == float.MinValue) return FormatResult.Error(value, "Не удалось привести значение к числу");

            return FormatResult.Corrected((int)Math.Round(toFloat), $"значение получено путем определения разделителя {separator.Single()}");

        }

        private FormatResult FormatDecimal(object value, AdditionalData additionalData)
        {
            var decimalResult = decimal.MinValue;

            if (value is int i) decimalResult = i;
            if (value is decimal d) decimalResult = d;
            if (value is double db) decimalResult = (decimal)db;
            if (value is float f) decimalResult = (decimal)f;

            if (decimalResult != Decimal.MinValue) return FormatResult.Correct(decimalResult);

            var str = value?.ToString()?.Trim().Replace(",", ".").Replace("?", string.Empty);
            if (str.IsNullOrEmpty()) return FormatResult.Empty();

            var parsed = str.ToDecimal();
            if (parsed != decimal.MinValue) return FormatResult.Correct(parsed);

            var separator = str.Where(x => !char.IsDigit(x)).ToHashSet();
            if (separator.Count != 1) return FormatResult.Error(value, $"Не удалось определить разделитель :{separator.JoinToString()}");

            var parsedWithSeparator = str.Replace(separator.Single(), '.').ToDecimal();
            if (parsedWithSeparator == decimal.MinValue) return FormatResult.Error(value, "Не удалось привести к числу");

            return FormatResult.Corrected(parsedWithSeparator, "было получено путем корректировки разделителя");


        }

        private FormatResult FormatString(object value, AdditionalData additionalData)
        {
            if (value is null) return FormatResult.Empty();
            if (value is string str) return FormatResult.Correct(str.Trim());
            if (value is DateTime dt) return FormatResult.Correct(dt.ToString("dd.MM.yyyy"));

            var stringified = value.ToString().Trim();


            return FormatResult.Corrected(stringified, $"значение было получено из {value.GetType().Name}");
        }
    }
}