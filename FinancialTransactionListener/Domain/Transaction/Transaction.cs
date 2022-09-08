using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using FinancialTransactionListener.V1.Domain.Transaction;

namespace FinancialTransactionListener.Domain.Transaction
{
    public class Transaction
    {

        public Guid Id { get; set; }
        [NotNull]
        public Guid TargetId { get; set; }
        [Required]
        public short PeriodNo { get; set; }
        [Required]
        public short FinancialYear { get; set; }
        [Required]
        public short FinancialMonth { get; set; }
        [Required]
        public string TransactionSource { get; set; }
        public TransactionType TransactionType { get; set; }

        public DateTime TransactionDate { get; set; }
        public decimal TransactionAmount { get; set; }
        [Required]
        public string PaymentReference { get; set; }
        [AllowNull]
        public string BankAccountNumber { get; set; }
        [Required]
        public bool IsSuspense { get; set; }
        [AllowNull]
        public SuspenseResolutionInfo SuspenseResolutionInfo { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal ChargedAmount { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal HousingBenefitAmount { get; set; }

        public string Address { get; set; }
        [Required]
        public Person Person { get; set; }
        [Required]
        public string Fund { get; set; }
    }
}
