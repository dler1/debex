using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Debex.Convert.Data
{
    public class Consts
    {
        public enum FieldsMatchState
        {
            None,
            Partial,
            All
        }

        public static class CalculatedColumns
        {
            public const string DpdName = "Сверка DPD";
        }
    }

    public static class FieldsIds
    {
        public const int Id = 1;
        public const int RegistryCreationDate = 3;

        public const int BirthDate = 8;
        public const int Sex = 9;

        public const int FirstDelayDate = 10;
        public const int LastDelayDate = 11;
        public const int Dpd = 12;
        public const int CreditAmount = 13;
        public const int IssueDate = 14;
        public const int EndDate = 15;

        public const int BalanceOfArrearsTotalDebt = 16;
        public const int BalanceOfUrgentTotalDebt = 17;
        public const int TotalDebt = 18;
        public const int BalanceOfArrearsTotalInterest = 19;
        public const int BalanceOfUrgentTotalInterest = 20;

        public const int TotalInterest = 21;
        public const int Fines = 22;
        public const int Penalties = 23;
        public const int Commissions = 24;
        public const int Tax = 25;

        public const int TotalOwed = 26;

        public const int LastPaymentDate = 28;
        public const int LastPaymentAmount = 29;
        public const int AllPaymentsHalfYear = 30;
        public const int AllPaymentsYear = 31;
        public const int AllPayments = 32;

        public const int CourtDecisionDate = 33;
        public const int CourtClaimDate = 34;
        public const int CourtProcessingDate = 35;
        public const int CourtStateFlag = 36;

        public const int CreditTerm = 39;

        public const int CalculatedCreditTerm = 100;
        public const int CalculatedTotalDebt = 101;
    }
}
