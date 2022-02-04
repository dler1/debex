using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Runtime.Serialization.Json;
using Debex.Convert.BL.ExcelReading;
using Debex.Convert.Data;
using Debex.Convert.Environment;
using Debex.Convert.Extensions;
using ExcelDataReader;

namespace Debex.Convert.BL.CalculatingFields
{
    public class CalculatedFieldsFactory
    {
        private const string CalculatedCreditTermHeader = "Debex.info / расчетный срок кредита (в днях)";
        private const string CourtFlagName = "Debex.info / флаг суда";
        public const string CalculatedTotalDebtHeader = "Debex.info / Итого ОД";
        public const string DpdFifoName = "Debex.info / разница дней DPD (FIFO)";
        public const string DpdLifoName = "Debex.info / разница дней DPD (LIFO)";
        public const string OszCheck = "Debex.info / сверка ОСЗ (Общей суммы задолженности)";
        public const string Flag554 = "Debex.info / флаг нарушения 554-ФЗ (для МФО)";

        public List<FieldToCalculate> CreateFieldsToCalculates(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            return new()
            {
                new FieldToCalculate
                {
                    ColumnName = DpdFifoName,
                    Calculator = DpdFifoCalculator(excel, fields),
                    ShouldCreate = v =>
                        v.MatchedAll(FieldsIds.Dpd, FieldsIds.RegistryCreationDate) &&
                        v.MatchedAny(FieldsIds.FirstDelayDate),
                    Invisible = true,
                    NonZeroIsError = true
                },
                new FieldToCalculate
                {
                    ColumnName = DpdLifoName,
                    Calculator = DpdLifoCalculator(excel, fields),
                    ShouldCreate = v =>
                        v.MatchedAll(FieldsIds.Dpd, FieldsIds.RegistryCreationDate) &&
                        v.MatchedAny(FieldsIds.LastDelayDate),
                    Invisible = true,
                    NonZeroIsError = true

                },
                new FieldToCalculate
                {
                    Id = FieldsIds.CalculatedTotalDebt,
                    ColumnName = CalculatedTotalDebtHeader,
                    Calculator = TotalDebtCalculator(excel, fields),
                    Invisible = true,
                    NonZeroIsError = true,
                    ShouldCreate = v =>
                        !v.MatchedAll(FieldsIds.TotalDebt) && v.MatchedAll(FieldsIds.BalanceOfArrearsTotalDebt, FieldsIds.BalanceOfUrgentTotalDebt)
                },
                new FieldToCalculate
                {
                    ColumnName = "Debex.info / Итого процентов",
                    Calculator = TotalInterestCalculator(excel, fields),
                    Invisible = true,
                    ShouldCreate = v =>
                        !v.MatchedAll(FieldsIds.TotalInterest) && v.MatchedAll(FieldsIds.BalanceOfUrgentTotalDebt, FieldsIds.BalanceOfArrearsTotalInterest)
                },
                new FieldToCalculate
                {
                    ColumnName = OszCheck,
                    ShouldCreate = v => v.MatchedAll(FieldsIds.TotalOwed),
                    Calculator = CheckTotalOwedCalculator(excel, fields),
                    NonZeroIsError = true
                },
                new FieldToCalculate
                {
                    Id = FieldsIds.CalculatedCreditTerm,
                    ColumnName = CalculatedCreditTermHeader,
                    ShouldCreate = v => v.MatchedAll(FieldsIds.CreditTerm) || v.MatchedAll(FieldsIds.EndDate, FieldsIds.IssueDate),
                    Calculator = CalculatedCreditTermCalculator(excel, fields),
                },
                new FieldToCalculate
                {
                    ColumnName = "Debex.info / флаг истекшего СИД (срока исковой давности)",
                    ShouldCreate = v => v.MatchedAll(FieldsIds.LastPaymentDate, FieldsIds.EndDate, FieldsIds.RegistryCreationDate),
                    Calculator = LimitationPeriodLeftFlagCalculator(excel, fields)
                },
                new FieldToCalculate
                {
                    ColumnName = "Debex.info / Дней до истечения СИД",
                    ShouldCreate = v => v.MatchedAll(FieldsIds.LastPaymentDate, FieldsIds.EndDate, FieldsIds.RegistryCreationDate),
                    Calculator = DaysBeforeLimitationPeriodLeftCalculator(excel, fields)

                },

                new FieldToCalculate
                {
                    ColumnName = "Debex.info / Возраст должника",
                    ShouldCreate = v => v.MatchedAll(FieldsIds.RegistryCreationDate, FieldsIds.BirthDate),
                    Calculator = AgeCalculator(excel, fields)
                },
                new FieldToCalculate
                {
                    ColumnName = "Debex.info / флаг пенсионера",
                    ShouldCreate = v => v.MatchedAll(FieldsIds.Sex, FieldsIds.BirthDate, FieldsIds.RegistryCreationDate),
                    Calculator = RetireeFlagCalculator(excel, fields)
                },
                new FieldToCalculate
                {
                    ColumnName = CourtFlagName,
                    ShouldCreate = v => v.MatchedAny(FieldsIds.CourtClaimDate, FieldsIds.CourtDecisionDate, FieldsIds.CourtProcessingDate, FieldsIds.CourtStateFlag),
                    Calculator = CourtFlagCalculator(excel, fields)
                },
                new FieldToCalculate
                {
                    ColumnName = "Debex.info / флаг истекшего СПИЛ (срока предьявления исполнительного листа)",
                    ShouldCreate = v =>
                        v.MatchedAll(FieldsIds.RegistryCreationDate, FieldsIds.CourtProcessingDate)
                        && v.MatchedAny(FieldsIds.CourtClaimDate, FieldsIds.CourtDecisionDate, FieldsIds.CourtProcessingDate, FieldsIds.CourtStateFlag),
                    Calculator = SpilFlagCalculator(excel, fields)
                },
                new FieldToCalculate
                {
                    ColumnName = "Debex.info / DTD (дней в кредите до просрочки)",
                    Calculator = DaysBeforeDelayCalculator(excel, fields),
                    ShouldCreate = v => v.MatchedAll(FieldsIds.IssueDate) &&
                        (v.MatchedAny(FieldsIds.FirstDelayDate, FieldsIds.LastDelayDate) || v.MatchedAll(FieldsIds.Dpd, FieldsIds.RegistryCreationDate)),
                    Invisible = true
                },

                new FieldToCalculate()
                {
                    ColumnName = Flag554,
                    ShouldCreate = v =>
                        v.MatchedAll(FieldsIds.IssueDate, FieldsIds.TotalOwed, FieldsIds.CreditAmount)
                        && v.MatchedAny(FieldsIds.AllPayments, FieldsIds.AllPaymentsYear, FieldsIds.AllPaymentsHalfYear),
                    Calculator = Check554FZCalculator(excel, fields),
                    OneIsError = true
                },
                new FieldToCalculate
                {
                    ColumnName = "Debex.info / DPLP (дней с последнего платежа)",
                    ShouldCreate = v => v.MatchedAll(FieldsIds.RegistryCreationDate, FieldsIds.LastPaymentDate),
                    Calculator = DaysAfterLastPaymentCalculator(excel, fields)
                },

                new FieldToCalculate
                {
                    ColumnName = "Debex.info / PID (флаг платежа в просрочке)",
                    ShouldCreate = v => v.MatchedAll(FieldsIds.LastPaymentAmount, FieldsIds.LastPaymentDate, FieldsIds.RegistryCreationDate, FieldsIds.FirstDelayDate),
                    Calculator = DelayPaymentFlagCalculator(excel, fields)
                },
                new FieldToCalculate
                {
                    ColumnName = "Debex.info / флаг платежа в последние 12 мес",
                    ShouldCreate = v => true,
                    Calculator = LastYearPaymentFlag(excel, fields)
                },
                new FieldToCalculate
                {
                    ColumnName = "Debex.info / DPLP бакет",
                    ShouldCreate = v => v.MatchedAll(FieldsIds.LastPaymentDate, FieldsIds.RegistryCreationDate),
                    Calculator = DaysAfterLastPaymentBucketCalculator(excel, fields)
                },
                new FieldToCalculate
                {
                    ColumnName = "Debex.info / DPD бакет",
                    ShouldCreate = v => v.MatchedAny(FieldsIds.Dpd),
                    Calculator = DaysAfterDelayBucketCalculator(excel, fields)
                }
            };
        }

        private Func<DataRow, object> TotalInterestCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            var arCol = fields.MatchedById(FieldsIds.BalanceOfUrgentTotalDebt).GetColumnIndex(excel);
            var urCol = fields.MatchedById(FieldsIds.BalanceOfArrearsTotalInterest).GetColumnIndex(excel);
            return row =>
            {
                var ar = row.GetValue<decimal?>(arCol) ?? 0;
                var ur = row.GetValue<decimal?>(urCol) ?? 0;

                return ar + ur;
            };
        }

        private Func<DataRow, object> TotalDebtCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            var urgent = fields.MatchedById(FieldsIds.BalanceOfUrgentTotalDebt).GetColumnIndex(excel);
            var arrears = fields.MatchedById(FieldsIds.BalanceOfArrearsTotalDebt).GetColumnIndex(excel);

            return row =>
            {
                var urVal = row.GetValue<decimal?>(urgent) ?? 0;
                var arVal = row.GetValue<decimal?>(arrears) ?? 0;

                var sum = urVal + arVal;
                return sum;
            };
        }

        private static Func<DataRow, object> DaysAfterDelayBucketCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            var dpdCol = fields.MatchedById(FieldsIds.Dpd).GetColumnIndex(excel);
            var cfg = IoC.Get<ConfigurationLoader>().Buckets.Dpd;

            return row =>
            {
                if (row.IsAnyError(dpdCol)) return null;

                var dpd = row.GetValue<int?>(dpdCol);

                if (IsAnyFalse(dpd.HasValue)) return "нет данных";

                return cfg.FirstOrDefault(x => x.Min <= dpd && x.Max >= dpd)?.Value ?? "нет данных";
            };

        }

        private static Func<DataRow, object> DaysAfterLastPaymentBucketCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            var lastPayment = fields.MatchedById(FieldsIds.LastPaymentDate).GetColumnIndex(excel);
            var creationDate = fields.MatchedById(FieldsIds.RegistryCreationDate).GetColumnIndex(excel);

            var cfg = IoC.Get<ConfigurationLoader>().Buckets;

            return row =>
            {
                if (row.IsAnyError(lastPayment, creationDate)) return null;

                var payment = row.GetValue<DateTime?>(lastPayment);
                var creation = row.GetValue<DateTime?>(creationDate);

                if (IsAnyFalse(payment.HasValue, creation.HasValue)) return "нет данных";

                var days = (creation.Value.Date - payment.Value.Date).TotalDays;
                return cfg.Dplp.FirstOrDefault(x => x.Min <= days && x.Max >= days)?.Value ?? "нет данных";
            };
        }

        private static Func<DataRow, object> SpilFlagCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            var creationDate = fields.MatchedById(FieldsIds.RegistryCreationDate).GetColumnIndex(excel);
            var courtProcessingDate = fields.MatchedById(FieldsIds.CourtProcessingDate).GetColumnIndex(excel);

            return row =>
            {
                var creation = row.GetValue<DateTime?>(creationDate);
                var processing = row.GetValue<DateTime?>(courtProcessingDate);

                var courtFlagField = excel.Headers[CourtFlagName];
                var courtFlag = row.GetValue<int?>(courtFlagField);

                if (!courtFlag.HasValue || courtFlag == 0) return 0;
                if (IsAnyFalse(creation.HasValue, processing.HasValue)) return 0;

                return Math.Abs((creation.Value - processing.Value).TotalDays) >= 1095 ? 1 : 0;
            };

        }

        private static Func<DataRow, object> CourtFlagCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            var decisionDate = fields.MatchedById(FieldsIds.CourtDecisionDate).GetColumnIndex(excel);
            var claimDate = fields.MatchedById(FieldsIds.CourtClaimDate).GetColumnIndex(excel);
            var processingDate = fields.MatchedById(FieldsIds.CourtProcessingDate).GetColumnIndex(excel);
            var flagField = fields.MatchedById(FieldsIds.CourtStateFlag).GetColumnIndex(excel);

            return row =>
            {
                var decision = decisionDate >= 0 ? row.GetValue<DateTime?>(decisionDate) : null;
                var claim = claimDate >= 0 ? row.GetValue<DateTime?>(claimDate) : null;
                var processing = processingDate >= 0 ? row.GetValue<DateTime?>(processingDate) : null;

                if (decision.HasValue || claim.HasValue || processing.HasValue) return 1;

                if (flagField == -1) return 0;

                var flag = row.GetValue<string>(flagField);
                return flag.StartsWithOneOf("ДА", "YES", "1") ? 1 : 0;
            };
        }

        private static Func<DataRow, object> RetireeFlagCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            var sexField = fields.MatchedById(FieldsIds.Sex).GetColumnIndex(excel);
            var birthDate = fields.MatchedById(FieldsIds.BirthDate).GetColumnIndex(excel);
            var creationDate = fields.MatchedById(FieldsIds.RegistryCreationDate).GetColumnIndex(excel);

            if (IsAnyFieldNotMatched(sexField, birthDate, creationDate)) return null;

            return row =>
            {
                var sex = row.GetValue<string>(sexField);
                var birth = row.GetValue<DateTime?>(birthDate);
                var creation = row.GetValue<DateTime?>(creationDate);

                if (row.IsAnyError(sexField, birthDate, creationDate)) return 0;

                if (IsAnyFalse(sex.NotNullOrEmpty(), birth.HasValue, creation.HasValue)) return 0;

                var isFemale = sex.StartsWith("Ж", StringComparison.InvariantCultureIgnoreCase);
                var retirementAge = isFemale ? 55 : 60;
                return birth.Value.AddYears(retirementAge) <= creation ? 1 : 0;

            };


        }

        private static Func<DataRow, object> DaysBeforeLimitationPeriodLeftCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            var endDate = fields.MatchedById(FieldsIds.EndDate).GetColumnIndex(excel);
            var lastPaymentDate = fields.MatchedById(FieldsIds.LastPaymentDate).GetColumnIndex(excel);
            var creationDate = fields.MatchedById(FieldsIds.RegistryCreationDate).GetColumnIndex(excel);

            if (IsAnyFieldNotMatched(endDate, lastPaymentDate, creationDate)) return null;

            return row =>
            {
                var end = row.GetValue<DateTime?>(endDate);
                var start = row.GetValue<DateTime?>(creationDate);
                var lastPayment = row.GetValue<DateTime?>(lastPaymentDate);

                if (row.IsAnyError(endDate, lastPaymentDate, creationDate)) return null;
                if (IsAnyFalse(end.HasValue, start.HasValue, lastPayment.HasValue)) return string.Empty;

                var target = lastPayment.Value > end.Value ? lastPayment : end;
                return 1095 - Math.Abs((target - start).Value.TotalDays);

            };
        }

        private static Func<DataRow, object> LimitationPeriodLeftFlagCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            var endDate = fields.MatchedById(FieldsIds.EndDate).GetColumnIndex(excel);
            var lastPaymentDate = fields.MatchedById(FieldsIds.LastPaymentDate).GetColumnIndex(excel);
            var creationDate = fields.MatchedById(FieldsIds.RegistryCreationDate).GetColumnIndex(excel);

            if (IsAnyFieldNotMatched(endDate, lastPaymentDate, creationDate)) return null;


            return row =>
            {
                var end = row.GetValue<DateTime?>(endDate);
                var start = row.GetValue<DateTime?>(creationDate);
                var lastPayment = row.GetValue<DateTime?>(lastPaymentDate);

                if (row.IsAnyError(endDate, lastPaymentDate, creationDate)) return 0;
                if (IsAnyFalse(end.HasValue, start.HasValue, lastPayment.HasValue)) return 0;


                var target = lastPayment.Value > end.Value ? lastPayment : end;
                return Math.Abs((target - start).Value.TotalDays) >= 1095 ? 1 : 0;
            };
        }

        private static Func<DataRow, object> CalculatedCreditTermCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            var dateStart = fields.MatchedById(FieldsIds.IssueDate).GetColumnIndex(excel);
            var dateEnd = fields.MatchedById(FieldsIds.EndDate).GetColumnIndex(excel);

            if (IsAllFieldsMatched(dateStart, dateEnd))
            {
                return row =>
                {
                    var start = row.GetValue<DateTime?>(dateStart);
                    var end = row.GetValue<DateTime?>(dateEnd);

                    if (row.IsAnyError(dateStart, dateEnd)) return null;
                    if (IsAnyFalse(start.HasValue, end.HasValue)) return string.Empty;

                    return (int)(end - start).Value.TotalDays;
                };
            }

            var creditTerm = fields.MatchedById(FieldsIds.CreditTerm).GetColumnIndex(excel);

            return row =>
            {
                var term = row.GetValue<int?>(creditTerm);
                if (!term.HasValue) return null;

                return (int)(term < 30 ? term * 30.5 : term);
            };
        }

        private static Func<DataRow, object> CheckTotalOwedCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            var totalOwed = fields.MatchedById(FieldsIds.TotalOwed).GetColumnIndex(excel);

            var terms = fields
                .MatchedByIds(FieldsIds.Fines, FieldsIds.Tax, FieldsIds.Penalties, FieldsIds.Commissions)
                .Select(x => x.GetColumnIndex(excel))
                .ToList();

            var totalDebt = fields.MatchedById(FieldsIds.TotalDebt).GetColumnIndex(excel);
            var totalInterest = fields.MatchedById(FieldsIds.TotalInterest).GetColumnIndex(excel);

            if (totalDebt < 0 && totalInterest < 0)
            {
                var additionalTerms = fields
                    .MatchedByIds(FieldsIds.BalanceOfArrearsTotalDebt,
                        FieldsIds.BalanceOfArrearsTotalInterest,
                        FieldsIds.BalanceOfUrgentTotalDebt,
                        FieldsIds.BalanceOfUrgentTotalInterest)
                    .Select(x => x.GetColumnIndex(excel))
                    .ToList();
                terms.AddRange(additionalTerms);
            }
            else
            {
                if (totalDebt >= 0)
                {
                    terms.Add(totalDebt);
                }
                if (totalInterest >= 0)
                {
                    terms.Add(totalInterest);
                }
            }

            return row =>
            {
                var total = row.GetValue<decimal?>(totalOwed);
                var termSum = terms.Select(x => row.GetValue<decimal?>(x) ?? 0).Sum();
                return total - termSum;
            };

        }

        private static Func<DataRow, object> AgeCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            var birthCol = fields.MatchedById(FieldsIds.BirthDate).GetColumnIndex(excel);
            var creationCol = fields.MatchedById(FieldsIds.RegistryCreationDate).GetColumnIndex(excel);

            if (IsAnyFieldNotMatched(birthCol, creationCol)) return null;

            return row =>
            {
                var birth = row.GetValue<DateTime?>(birthCol);
                var creation = row.GetValue<DateTime?>(creationCol);

                if (row.IsAnyError(birthCol, creationCol)) return null;
                if (IsAnyFalse(birth.HasValue, creation.HasValue)) return string.Empty;

                return (int)((creation.Value.Date - birth.Value.Date).TotalDays / 365);
            };
        }

        private static Func<DataRow, object> DelayPaymentFlagCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            var lastPaymentAmount = fields.MatchedById(FieldsIds.LastPaymentAmount).GetColumnIndex(excel);
            var lastPaymentDate = fields.MatchedById(FieldsIds.LastPaymentDate).GetColumnIndex(excel);
            var creationDate = fields.MatchedById(FieldsIds.RegistryCreationDate).GetColumnIndex(excel);
            var firstDelayDate = fields.MatchedById(FieldsIds.FirstDelayDate).GetColumnIndex(excel);

            if (new[] { lastPaymentAmount, lastPaymentDate, creationDate, firstDelayDate }.Any(x => x < 0)) return null;

            return row =>
            {
                var paymentAmount = row.GetValue<decimal?>(lastPaymentAmount);
                var paymentDate = row.GetValue<DateTime?>(lastPaymentDate);
                var creationDt = row.GetValue<DateTime?>(creationDate);
                var delayDate = row.GetValue<DateTime?>(firstDelayDate);

                if (row.IsAnyError(lastPaymentAmount, lastPaymentDate, creationDate, firstDelayDate)) return null;
                if (IsAnyFalse(paymentAmount.HasValue, paymentDate.HasValue, creationDt.HasValue, delayDate.HasValue)) return string.Empty;

                return paymentAmount >= 100 && paymentDate.Between(delayDate.Value, creationDt.Value) ? 1 : 0;
            };

        }

        private static Func<DataRow, object> DaysBeforeDelayCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {

            var issueDateCol = fields.MatchedById(FieldsIds.IssueDate).GetColumnIndex(excel);
            var firstDelayCol = fields.MatchedById(FieldsIds.FirstDelayDate).GetColumnIndex(excel);
            var lastDelayCol = fields.MatchedById(FieldsIds.LastDelayDate).GetColumnIndex(excel);
            var targetCol = firstDelayCol >= 0 ? firstDelayCol : lastDelayCol;

            var dpdCol = fields.MatchedById(FieldsIds.Dpd).GetColumnIndex(excel);
            var regCreationCol = fields.MatchedById(FieldsIds.RegistryCreationDate).GetColumnIndex(excel);

            if (targetCol < 0)
                return row =>
                {

                    var dpd = row.GetValue<int?>(dpdCol);
                    var reg = row.GetValue<DateTime?>(regCreationCol);
                    var issue = row.GetValue<DateTime?>(issueDateCol);
                    if (!dpd.HasValue || !reg.HasValue || !issue.HasValue) return string.Empty;


                    return ((reg.Value.Date.AddDays(-dpd.Value)) - issue.Value.Date).TotalDays;
                };


            return row =>
            {
                var target = row.GetValue<DateTime?>(targetCol);
                var issue = row.GetValue<DateTime?>(issueDateCol);

                if (IsAnyFalse(target.HasValue, issue.HasValue)) return string.Empty;

                return (target.Value.Date - issue.Value.Date).TotalDays;
            };

        }

        private static Func<DataRow, object> DpdFifoCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {

            var dpdField = fields.ById(FieldsIds.Dpd);
            if (dpdField.IsMatched != true) return null;

            var registryField = fields.ById(FieldsIds.RegistryCreationDate);
            if (registryField.IsMatched != true) return null;

            var targetCol = fields.MatchedById(FieldsIds.FirstDelayDate).GetColumnIndex(excel);


            var dpdCol = excel.Headers[dpdField.MatchedName];
            var regCol = excel.Headers[registryField.MatchedName];


            Func<DataRow, object> calc = (row) =>
            {
                var dpd = row.GetValue<int?>(dpdCol);
                var reg = row.GetValue<DateTime?>(regCol);
                var target = row.GetValue<DateTime?>(targetCol);


                if (row.IsAnyError(dpdCol, regCol, targetCol)) return null;
                if (IsAnyFalse(dpd.HasValue, reg.HasValue, target.HasValue)) return string.Empty;

                var result = dpd - (reg.Value - target.Value).TotalDays;
                return result;
            };

            return calc;


        }

        private static Func<DataRow, object> DpdLifoCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {

            var dpdField = fields.ById(FieldsIds.Dpd);
            if (dpdField.IsMatched != true) return null;

            var registryField = fields.ById(FieldsIds.RegistryCreationDate);
            if (registryField.IsMatched != true) return null;

            var targetCol = fields.MatchedById(FieldsIds.LastDelayDate).GetColumnIndex(excel);


            var dpdCol = excel.Headers[dpdField.MatchedName];
            var regCol = excel.Headers[registryField.MatchedName];


            Func<DataRow, object> calc = (row) =>
            {
                var dpd = row.GetValue<int?>(dpdCol);
                var reg = row.GetValue<DateTime?>(regCol);
                var target = row.GetValue<DateTime?>(targetCol);


                if (row.IsAnyError(dpdCol, regCol, targetCol)) return null;
                if (IsAnyFalse(dpd.HasValue, reg.HasValue, target.HasValue)) return string.Empty;

                var result = dpd - (reg.Value - target.Value).TotalDays;
                return result;
            };

            return calc;


        }

        private static Func<DataRow, object> Check554FZCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            var paymentsField = fields.MatchedById(FieldsIds.AllPayments);
            paymentsField ??= fields.MatchedById(FieldsIds.AllPaymentsYear);
            paymentsField ??= fields.MatchedById(FieldsIds.AllPaymentsHalfYear);

            if (paymentsField == null) return null;

            var paymentCol = paymentsField.GetColumnIndex(excel);


            var issueDateField = fields.ById(FieldsIds.IssueDate).GetColumnIndex(excel);
            var totalOwedField = fields.ById(FieldsIds.TotalOwed).GetColumnIndex(excel);

            var totalDebtField = fields.MatchedById(FieldsIds.TotalDebt).GetColumnIndex(excel);


            var creditAmountField = fields.ById(FieldsIds.CreditAmount).GetColumnIndex(excel);

            var taxCol = fields.MatchedById(FieldsIds.Tax).GetColumnIndex(excel);

            if (new[] { issueDateField, totalOwedField, creditAmountField, paymentCol }.Any(x => x < 0)) return null;

            return row =>
            {

                var debtId = totalDebtField >= 0 ? totalDebtField : excel.Headers.TryGetValue(CalculatedTotalDebtHeader, out var i) ? i : -1;
                if (debtId < 0) return string.Empty;

                if (!excel.Headers.ContainsKey(CalculatedCreditTermHeader)) return 0;

                var creditTermField = excel.Headers[CalculatedCreditTermHeader];

                var totalPayments = row.GetValue<decimal?>(paymentCol);
                var issueDate = row.GetValue<DateTime?>(issueDateField);
                var totalOwed = row.GetValue<decimal?>(totalOwedField);
                var totalDebt = row.GetValue<decimal?>(debtId);
                var creditAmount = row.GetValue<decimal?>(creditAmountField);
                var creditTerm = row.GetValue<int?>(creditTermField);

                var tax = taxCol >= 0 ? row.GetValue<decimal?>(taxCol) : 0;

                if (row.IsAnyError(issueDateField, totalOwedField, debtId, creditAmountField)) return 0;

                if (IsAnyFalse(
                    issueDate.HasValue,
                    creditTerm.HasValue,
                    totalOwed.HasValue,
                    totalDebt.HasValue,
                    creditAmount.HasValue
                    )) return 0;


                var cost = totalOwed + totalPayments - creditAmount - tax;

                if (creditTerm > 365) return 0;

                if (issueDate.Between(new DateTime(2019, 1, 26), new DateTime(2019, 6, 30)) && (cost) > (creditAmount * 2.5m)) return 1;
                if (issueDate.Between(new DateTime(2019, 7, 1), new DateTime(2019, 12, 31)) && cost > creditAmount * 2) return 1;
                if (issueDate >= new DateTime(2020, 1, 1) && cost > creditAmount * 1.5m) return 1;

                return 0;

            };

        }

        private static Func<DataRow, object> DaysAfterLastPaymentCalculator(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {
            var creationDate = fields.MatchedById(FieldsIds.RegistryCreationDate).GetColumnIndex(excel);
            var lastPayment = fields.MatchedById(FieldsIds.LastPaymentDate).GetColumnIndex(excel);

            if (new[] { creationDate, lastPayment }.Any(x => x < 0)) return null;

            return row =>
            {
                var creation = row.GetValue<DateTime?>(creationDate);
                var payment = row.GetValue<DateTime?>(lastPayment);

                if (row.IsAnyError(creationDate, lastPayment)) return null;
                if (new[] { creation, payment }.Any(x => !x.HasValue)) return string.Empty;

                return (creation.Value.Date - payment.Value.Date).TotalDays;
            };

        }

        private static Func<DataRow, object> LastYearPaymentFlag(ReadExcelResult excel, List<BaseFieldToMatch> fields)
        {

            var lastPaymentAmountCol = fields.MatchedById(FieldsIds.LastPaymentAmount).GetColumnIndex(excel);
            var creationDateCol = fields.MatchedById(FieldsIds.RegistryCreationDate).GetColumnIndex(excel);
            var lastPaymentDateCol = fields.MatchedById(FieldsIds.LastPaymentDate).GetColumnIndex(excel);


            return row =>
            {
                if (IsAnyFalse(lastPaymentDateCol >= 0, creationDateCol >= 0, lastPaymentAmountCol >= 0)) return 0;

                var paymentAmount = row.GetValue<decimal?>(lastPaymentAmountCol);
                var creation = row.GetValue<DateTime?>(creationDateCol);
                var lastPayment = row.GetValue<DateTime?>(lastPaymentDateCol);

                if (row.IsAnyError(lastPaymentAmountCol, creationDateCol, lastPaymentDateCol)) return 0;
                if (IsAnyFalse(paymentAmount.HasValue, creation.HasValue, lastPayment.HasValue))
                    return 0;

                return paymentAmount >= 100 && (creation.Value.Date - lastPayment.Value.Date).TotalDays <= 365 ? 1 : 0;

            };
        }


        private static bool IsAnyFalse(params bool[] values) => values.Any(x => !x);

        private static bool IsAnyFieldNotMatched(params int[] indexes) => indexes.Any(x => x < 0);

        private static bool IsAllFieldsMatched(params int[] indexes) => indexes.All(x => x >= 0);
    }

}