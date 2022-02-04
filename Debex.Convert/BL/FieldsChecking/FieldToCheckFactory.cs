using Debex.Convert.BL.ExcelReading;
using Debex.Convert.Data;
using Debex.Convert.Extensions;
using System;
using System.Collections.Generic;
using System.Data;

namespace Debex.Convert.BL.FieldsChecking
{
    public class FieldToCheckFactory
    {
        public const string DpdLifoName = "Debex / Проверка 3: DPD (LIFO)";
        public const string DpdFifoName = "Debex / Проверка 4: DPD (FIFO)";
        public const string OszCheck = "Debex / Проверка 7: Общая сумма задолженности (ОСЗ) >= Итого основной долг";

        public static List<FieldToCheck> Create(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            return new()
            {
                new FieldToCheck
                {
                    ColumnName = "Debex / Проверка 1: Уникальность ID кредитного договора",
                    ShouldCreate = v => v.MatchedAny(FieldsIds.Id),
                    RepeatCalculator = CheckDuplicateIds(excel, fields)
                },
                new FieldToCheck
                {
                    ColumnName = "Debex / Проверка 2: Дата выдачи кд и Дата окончания кд",
                    ShouldCreate = v => v.MatchedAll(FieldsIds.IssueDate, FieldsIds.EndDate),
                    Calculator = DateIssueAndEndCalculator(excel, fields)
                },
                new FieldToCheck
                {
                    ColumnName = DpdLifoName,
                    ShouldCreate = v => v.MatchedAll(FieldsIds.RegistryCreationDate, FieldsIds.Dpd) && v.MatchedAny(FieldsIds.LastDelayDate),
                    Calculator = CheckDpdLifoCalculator(excel, fields)
                },
                new FieldToCheck
                {
                    ColumnName = DpdFifoName,
                    ShouldCreate = v => v.MatchedAll(FieldsIds.RegistryCreationDate, FieldsIds.Dpd) && v.MatchedAny(FieldsIds.FirstDelayDate),
                    Calculator = CheckDpdFifoCalculator(excel, fields)
                },
                new FieldToCheck
                {
                    ColumnName = "Debex / Проверка 5: Итого основной долг (ОД)",
                    ShouldCreate = v => v.MatchedAll(FieldsIds.BalanceOfArrearsTotalDebt, FieldsIds.BalanceOfUrgentTotalDebt, FieldsIds.TotalDebt),
                    Calculator = CheckTotalDebtCalculator(excel, fields)
                },
                new FieldToCheck
                {
                    ColumnName = "Debex / Проверка 6: Итого процентов",
                    ShouldCreate = v => v.MatchedAll(FieldsIds.BalanceOfArrearsTotalInterest, FieldsIds.BalanceOfUrgentTotalInterest, FieldsIds.TotalInterest),
                    Calculator = CheckTotalInterestCalculator(excel, fields)
                },
                new FieldToCheck
                {
                    ColumnName = OszCheck,
                    ShouldCreate = v => v.MatchedAll(FieldsIds.TotalOwed) &&
                        (v.MatchedAll(FieldsIds.TotalDebt) || v.MatchedAll(FieldsIds.BalanceOfArrearsTotalDebt, FieldsIds.BalanceOfUrgentTotalDebt)),
                    Calculator = TotalOwedMoreWhenDebtCalculator(excel, fields)
                },
                new FieldToCheck
                {
                    ColumnName = "Debex / Проверка 8: Дата последнего платежа <= Дата выгрузки",
                    ShouldCreate = v => v.MatchedAll(FieldsIds.RegistryCreationDate, FieldsIds.LastPaymentDate),
                    Calculator = CheckLastPaymentDateCalculator(excel, fields)
                },
                new FieldToCheck
                {
                    ColumnName = "Debex / Проверка 9: Дата и сумма последнего платежа",
                    ShouldCreate = v => v.MatchedAny(FieldsIds.LastPaymentDate, FieldsIds.LastPaymentAmount),
                    Calculator = CheckLastPaymentDateAndSum(excel, fields)
                },
                new FieldToCheck
                {
                    ColumnName = "Debex / Проверка 10: Платежи за 180 дней",
                    ShouldCreate = v => v.MatchedAll(FieldsIds.LastPaymentDate, FieldsIds.LastPaymentAmount, FieldsIds.AllPaymentsHalfYear, FieldsIds.RegistryCreationDate),
                    Calculator = CheckPaymentsForHalfYearCalculator(excel, fields)
                },
                new FieldToCheck
                {
                    ColumnName = "Debex / Проверка 11: Платежи за 360 дней",
                    ShouldCreate = v => v.MatchedAll(FieldsIds.LastPaymentDate, FieldsIds.LastPaymentAmount, FieldsIds.AllPaymentsYear, FieldsIds.RegistryCreationDate),
                    Calculator = CheckPaymentsForYearCalculator(excel, fields)
                },
                new FieldToCheck
                {
                    ColumnName = "Debex / Проверка 12: Проверка Даты Рождения",
                    ShouldCreate = v => v.MatchedAll(FieldsIds.BirthDate),
                    Calculator = CheckBirthDate(excel, fields)
                }
            };
        }

        private static Func<RepeatCalculatorData, int?> CheckDuplicateIds(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            int idCol = fields.MatchedById(FieldsIds.Id).GetColumnIndex(excel);

            Dictionary<string, int> repeats = new Dictionary<string, int>();
            HashSet<string> outputed = new HashSet<string>();

            return data =>
            {
                DataRow row = data.Row;

                if (data.RepeatTryCount == 0)
                {
                    data.Parameter = repeats;
                    data.RepeatTryCount++;
                    data.OutputInfo = new RepeatOutputTable
                    {
                        Header = "Повторяющиеся значения",
                        RowsHeaders = new() { "Id кредитного договора", "количество повторов" },
                        Rows = new List<object[]>(),
                    };
                }

                if (data.RepeatTryCount == 1)
                {
                    string value = row.GetValue<string>(idCol);
                    if (value.IsNullOrEmpty())
                    {
                        return null;
                    }

                    if (repeats.ContainsKey(value))
                    {
                        repeats[value] += 1;
                    }
                    else
                    {
                        repeats[value] = 1;
                    }

                    return -2;
                }

                if (data.RepeatTryCount == 2)
                {
                    string value = row.GetValue<string>(idCol);
                    int isSuccess = repeats[value] == 1 ? 1 : -1;
                    if (isSuccess == -1 && !outputed.Contains(value))
                    {
                        data.OutputInfo.Rows.Add(new object[] { value, repeats[value] });
                        outputed.Add(value);
                    }

                    return isSuccess;
                }

                return -2;

            };
        }

        private static Func<DataRow, int?> CheckPaymentsForYearCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            int lastPayment = fields.MatchedById(FieldsIds.LastPaymentAmount).GetColumnIndex(excel);
            int lastDate = fields.MatchedById(FieldsIds.LastPaymentDate).GetColumnIndex(excel);
            int yearSum = fields.MatchedById(FieldsIds.AllPaymentsYear).GetColumnIndex(excel);
            int creationDate = fields.MatchedById(FieldsIds.RegistryCreationDate).GetColumnIndex(excel);
            return row =>
            {
                if (row.IsAnyError(lastPayment, lastDate, yearSum, creationDate))
                {
                    return -1;
                }

                decimal? payment = row.GetValue<decimal?>(lastPayment);
                DateTime? date = row.GetValue<DateTime?>(lastDate);
                decimal? sum = row.GetValue<decimal?>(yearSum);
                DateTime? creation = row.GetValue<DateTime?>(creationDate);

                if (payment.HasValue && date.HasValue && sum.HasValue && creation.HasValue)
                {
                    return (date >= creation.Value.AddDays(-360))
                        ? sum >= payment
                            ? 1
                            : -1
                        : 1;
                }

                return -2;
            };
        }

        private static Func<DataRow, int?> CheckPaymentsForHalfYearCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            int lastPayment = fields.MatchedById(FieldsIds.LastPaymentAmount).GetColumnIndex(excel);
            int lastDate = fields.MatchedById(FieldsIds.LastPaymentDate).GetColumnIndex(excel);
            int halfYearSum = fields.MatchedById(FieldsIds.AllPaymentsHalfYear).GetColumnIndex(excel);
            int creationDate = fields.MatchedById(FieldsIds.RegistryCreationDate).GetColumnIndex(excel);
            return row =>
            {
                if (row.IsAnyError(lastPayment, lastDate, halfYearSum, creationDate))
                {
                    return -1;
                }

                decimal? payment = row.GetValue<decimal?>(lastPayment);
                DateTime? date = row.GetValue<DateTime?>(lastDate);
                decimal? sum = row.GetValue<decimal?>(halfYearSum);
                DateTime? creation = row.GetValue<DateTime?>(creationDate);

                if (payment.HasValue && date.HasValue && sum.HasValue && creation.HasValue)
                {
                    return (date >= creation.Value.AddDays(-180))
                        ? sum >= payment
                            ? 1
                            : -1
                        : 1;
                }

                return -2;
            };
        }
        private static Func<DataRow, int?> CheckBirthDate(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            int birthCol = fields.MatchedById(FieldsIds.BirthDate).GetColumnIndex(excel);
            return row =>
            {
                if (row.IsAnyError(birthCol))
                {
                    return -1;
                }
                var dt = row.GetValue<DateTime?>(birthCol);

                if (!dt.HasValue)
                {
                    return -2;
                }

                if (dt > DateTime.Today)
                {
                    return -1;
                }

                var age = DateTime.Now - dt.Value;
                var totalYears = age.TotalDays / 365;
                return totalYears >= 18 && totalYears <= 100 ? 1 : -1;

            };
        }

        private static Func<DataRow, int?> CheckLastPaymentDateAndSum(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            int paymentDate = fields.MatchedById(FieldsIds.LastPaymentDate).GetColumnIndex(excel);
            int paymentSum = fields.MatchedById(FieldsIds.LastPaymentAmount).GetColumnIndex(excel);

            return row =>
            {
                if (paymentDate == -1 || paymentSum == -1)
                {
                    return null;
                }

                if (row.IsAnyError(paymentSum, paymentDate))
                {
                    return -1;
                }

                decimal? sum = row.GetValue<decimal?>(paymentSum);
                DateTime? date = row.GetValue<DateTime?>(paymentDate);
                if (!sum.HasValue && !date.HasValue)
                {
                    return -2;
                }

                if (sum == 0 && !date.HasValue)
                {
                    return -2;
                }

                if (sum == 0 && date == default(DateTime))
                {
                    return -2;
                }

                return sum > 0 && date.HasValue ? 1 : -1;
            };
        }

        private static Func<DataRow, int?> CheckLastPaymentDateCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            int creationDate = fields.MatchedById(FieldsIds.RegistryCreationDate).GetColumnIndex(excel);
            int paymentDate = fields.MatchedById(FieldsIds.LastPaymentDate).GetColumnIndex(excel);

            return row =>
            {
                if (row.IsAnyError(creationDate, paymentDate))
                {
                    return -1;
                }

                DateTime? creation = row.GetValue<DateTime?>(creationDate);
                DateTime? payment = row.GetValue<DateTime?>(paymentDate);

                if (creation.HasValue && payment.HasValue)
                {
                    return creation >= payment ? 1 : -1;
                }

                return -2;
            };
        }

        private static Func<DataRow, int?> TotalOwedMoreWhenDebtCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            int totalOwed = fields.MatchedById(FieldsIds.TotalOwed).GetColumnIndex(excel);
            int totalDebt = fields.MatchedById(FieldsIds.TotalDebt).GetColumnIndex(excel);

            int totalArr = fields.MatchedById(FieldsIds.BalanceOfArrearsTotalDebt).GetColumnIndex(excel);
            int totalUrg = fields.MatchedById(FieldsIds.BalanceOfUrgentTotalDebt).GetColumnIndex(excel);

            return row =>
            {
                if (row.IsAnyError(totalOwed))
                {
                    return -1;
                }

                if (totalDebt < 0 && totalArr < 0 && totalUrg < 0)
                {
                    return -1;
                }

                decimal? deb = totalDebt == -1
                    ? row.GetValue<decimal?>(totalArr) + row.GetValue<decimal?>(totalUrg)
                    : row.GetValue<decimal?>(totalDebt);

                decimal? owed = row.GetValue<decimal?>(totalOwed);

                if (owed.HasValue && deb.HasValue)
                {
                    return owed > 0 && Math.Round(owed.Value, 2) >= Math.Round(deb.Value, 2) ? 1 : -1;
                }

                return null;
            };
        }

        private static Func<DataRow, int?> CheckTotalInterestCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            int arrearsDebt = fields.MatchedById(FieldsIds.BalanceOfArrearsTotalInterest).GetColumnIndex(excel);
            int urgentDebt = fields.MatchedById(FieldsIds.BalanceOfUrgentTotalInterest).GetColumnIndex(excel);
            int totalDebt = fields.MatchedById(FieldsIds.TotalInterest).GetColumnIndex(excel);

            return row =>
            {
                if (row.IsAnyError(arrearsDebt, urgentDebt, totalDebt))
                {
                    return null;
                }

                decimal? arrears = row.GetValue<decimal?>(arrearsDebt);
                decimal? urgent = row.GetValue<decimal?>(urgentDebt);
                decimal? total = row.GetValue<decimal?>(totalDebt);
                if (arrears.HasValue && urgent.HasValue && total.HasValue)
                {
                    return arrears + urgent == total ? 1 : -1;
                }

                return null;
            };
        }

        private static Func<DataRow, int?> CheckTotalDebtCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            int arrearsDebt = fields.MatchedById(FieldsIds.BalanceOfArrearsTotalDebt).GetColumnIndex(excel);
            int urgentDebt = fields.MatchedById(FieldsIds.BalanceOfUrgentTotalDebt).GetColumnIndex(excel);
            int totalDebt = fields.MatchedById(FieldsIds.TotalDebt).GetColumnIndex(excel);

            return row =>
            {
                if (row.IsAnyError(arrearsDebt, urgentDebt, totalDebt))
                {
                    return null;
                }

                decimal? arrears = row.GetValue<decimal?>(arrearsDebt);
                decimal? urgent = row.GetValue<decimal?>(urgentDebt);
                decimal? total = row.GetValue<decimal?>(totalDebt);

                if (arrears.HasValue && urgent.HasValue && total.HasValue)
                {
                    return arrears + urgent == total ? 1 : -1;
                }

                return null;
            };


        }

        private static Func<DataRow, int?> CheckDpdLifoCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            int targetDate = fields.MatchedById(FieldsIds.LastDelayDate).GetColumnIndex(excel);


            int creationDate = fields.MatchedById(FieldsIds.RegistryCreationDate).GetColumnIndex(excel);
            int dpdCol = fields.MatchedById(FieldsIds.Dpd).GetColumnIndex(excel);

            return row =>
            {
                if (row.IsAnyError(targetDate, creationDate, dpdCol))
                {
                    return -1;
                }

                DateTime? target = row.GetValue<DateTime?>(targetDate);
                int? dpd = row.GetValue<int?>(dpdCol);
                DateTime? creation = row.GetValue<DateTime?>(creationDate);

                if (target.HasValue && dpd.HasValue && creation.HasValue)
                {
                    return (creation.Value - target.Value).TotalDays == dpd.Value ? 1 : -1;
                }

                return -2;
            };

        }

        private static Func<DataRow, int?> CheckDpdFifoCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            int targetDate = fields.MatchedById(FieldsIds.FirstDelayDate).GetColumnIndex(excel);

            int creationDate = fields.MatchedById(FieldsIds.RegistryCreationDate).GetColumnIndex(excel);
            int dpdCol = fields.MatchedById(FieldsIds.Dpd).GetColumnIndex(excel);

            return row =>
            {
                if (row.IsAnyError(targetDate, creationDate, dpdCol))
                {
                    return -1;
                }

                DateTime? target = row.GetValue<DateTime?>(targetDate);
                int? dpd = row.GetValue<int?>(dpdCol);
                DateTime? creation = row.GetValue<DateTime?>(creationDate);

                if (target.HasValue && dpd.HasValue && creation.HasValue)
                {
                    return (creation.Value - target.Value).TotalDays == dpd.Value ? 1 : -1;
                }

                return -2;
            };

        }

        private static Func<DataRow, int?> DateIssueAndEndCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            int issueDate = fields.MatchedById(FieldsIds.IssueDate).GetColumnIndex(excel);
            int endDate = fields.MatchedById(FieldsIds.EndDate).GetColumnIndex(excel);

            return row =>
            {
                DateTime? issue = row.GetValue<DateTime?>(issueDate);
                DateTime? end = row.GetValue<DateTime?>(endDate);

                if (row.IsAnyError(issueDate, endDate))
                {
                    return -1;
                }

                if (!end.HasValue && !issue.HasValue)
                {
                    return -2;
                }

                if (end.HasValue && issue.HasValue)
                {
                    return end.Value > issue.Value ? 1 : -1;
                }

                return null;
            };

        }


    }
}