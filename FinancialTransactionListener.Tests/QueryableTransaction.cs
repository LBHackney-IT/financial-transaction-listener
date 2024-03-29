using FinancialTransactionListener.Domain.Transaction;
using FinancialTransactionListener.V1.Domain.Transaction;
using Nest;
using System;

namespace FinancialTransactionListener.Tests
{
    public class QueryableTransaction
    {
        public Transaction Create()
        {


            return new Transaction
            {
                Address = Address,
                BalanceAmount = BalanceAmount,
                BankAccountNumber = BankAccountNumber,
                ChargedAmount = ChargedAmount,
                FinancialMonth = FinancialMonth,
                Fund = Fund,
                FinancialYear = FinancialYear,
                HousingBenefitAmount = HousingBenefitAmount,
                Id = Id,
                IsSuspense = IsSuspense,
                PaidAmount = PaidAmount,
                TransactionAmount = TransactionAmount,
                TransactionType = TransactionType,
                TransactionDate = TransactionDate,
                SuspenseResolutionInfo = SuspenseResolutionInfo,
                PeriodNo = PeriodNo,
                TransactionSource = TransactionSource,
                PaymentReference = PaymentReference,
                Person = Person
            };
        }

        [Text(Name = "id")]
        public Guid Id { get; set; }
        [Number(Name = "periodNo")]
        public short PeriodNo { get; set; }

        [Number(Name = "financialYear")]
        private short FinancialYear { get; set; }

        [Number(Name = "financialMonth")]
        public short FinancialMonth { get; set; }

        [Text(Name = "transactionSource")]
        public string TransactionSource { get; set; }

        [Text(Name = "transactionType")]
        public TransactionType TransactionType { get; set; }

        [Date(Name = "transactionDate")]
        public DateTime TransactionDate { get; set; }

        [Number(Name = "transactionAmount")]
        public decimal TransactionAmount { get; set; }

        [Text(Name = "paymentReference")]
        public string PaymentReference { get; set; }

        [Text(Name = "bankAccountNumber")]
        public string BankAccountNumber { get; set; }

        [Boolean(Name = "isSuspense")]
        public bool IsSuspense { get; set; }

        [Text(Name = "suspenseResolutionInfo")]
        public SuspenseResolutionInfo SuspenseResolutionInfo { get; set; }

        [Number(Name = "paidAmount")]
        public decimal PaidAmount { get; set; }

        [Number(Name = "chargedAmount")]
        public decimal ChargedAmount { get; set; }

        [Number(Name = "balanceAmount")]
        public decimal BalanceAmount { get; set; }

        [Number(Name = "housingBenefitAmount")]
        public decimal HousingBenefitAmount { get; set; }

        [Text(Name = "address")]
        public string Address { get; set; }

        [Text(Name = "person")]
        public Person Person { get; set; }

        [Text(Name = "fund")]
        public string Fund { get; set; }
    }
}
