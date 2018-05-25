using FinancialAccountTool.ExcelSerialzation.Attributes;
using System;

namespace FinanicalAccountModernClient.DO
{
    [ExcelWorksheet(SheetName = "Transactions")]
    public class Transaction : AccountsReceivable
    {
        public Transaction(Transaction transaction)
        {
            Assignment = transaction.Assignment;
            DocumentDate = transaction.DocumentDate;
            AmountInLocalCurrency = transaction.AmountInLocalCurrency;
            LocalCurrency = transaction.LocalCurrency;
            Account = transaction.Account;
            AssignmentBalance = transaction.AssignmentBalance;
            TransactionIndex = 1;
        }

        public Transaction(AccountsReceivable accountsReceivable)
        {
            Assignment = accountsReceivable.Assignment;
            DocumentDate = accountsReceivable.DocumentDate;
            AmountInLocalCurrency = accountsReceivable.AmountInLocalCurrency;
            LocalCurrency = accountsReceivable.LocalCurrency;
            Account = accountsReceivable.Account;
            AssignmentBalance = AmountInLocalCurrency;
            PaymentDate = null;
            TransactionIndex = 1;
        }

        [ExcelColumn(HeaderName = "Assignment Balance", DataFormat = "#,###,##0.00", Index = 6)]
        public float AssignmentBalance { set; get; }

        [ExcelColumn(HeaderName = "Payment Date", DataFormat = "yyyy-mm-dd", Index = 7)]
        public DateTime? PaymentDate { set; get; }

        [ExcelColumn(HeaderName = "Transaction Amount", DataFormat = "#,###,##0.00", Index = 8)]
        public float TransactionAmount { set; get; }

        [ExcelColumn(HeaderName = "Payment Currency", DataFormat = "@", Index = 9)]
        public string PaymentCurrency { set; get; }

        [ExcelColumn(HeaderName = "Transaction Index in Assignment", DataFormat = "#", Index = 10)]
        public int TransactionIndex { set; get; }

        public Payment Exchange(Payment payment)
        {
            TransactionAmount = AssignmentBalance >= payment.Balance ? payment.Balance : AssignmentBalance;
            payment.Balance -= TransactionAmount;
            PaymentDate = payment.Date;
            PaymentCurrency = payment.Currency;
            payment.FulfilledAssignments.Add(Assignment);
            return payment;
        }

        public Transaction GenerateNexTransaction()
        {
            var nextTransaction = new Transaction(this);
            nextTransaction.AssignmentBalance = nextTransaction.AssignmentBalance - TransactionAmount;
            nextTransaction.TransactionIndex++;
            return (nextTransaction.AssignmentBalance > 0) ? nextTransaction : null;
        }
    }
}
