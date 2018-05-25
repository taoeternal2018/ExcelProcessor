using System;
using System.Windows;
using NPOI.SS.UserModel;
using System.Windows.Forms;
using FinancialAccountTool.ExcelSerialzation.Attributes;

namespace FinanicalAccountModernClient.DO
{
    public class AccountsReceivable
    {
        public AccountsReceivable()
        {
        }
        public AccountsReceivable(IRow row)
        {
            Assignment = Convert.ToInt64(row.Cells[0].StringCellValue);
            DocumentDate = row.Cells[1].DateCellValue;
            AmountInLocalCurrency = (float)row.Cells[2].NumericCellValue;
            LocalCurrency = row.Cells[3].StringCellValue;
            Account = Convert.ToInt32(row.Cells[4].StringCellValue);
        }

        public override string ToString()
        {
            return $"{nameof(Assignment)}: {Assignment}, {nameof(DocumentDate)}: {DocumentDate}, {nameof(AmountInLocalCurrency)}: {AmountInLocalCurrency}, {nameof(LocalCurrency)}: {LocalCurrency}, {nameof(Account)}: {Account}";
        }

        [ExcelColumn (HeaderName = "Assignment", DataFormat = "@", Index = 1)]
        public long Assignment { set; get; }

        [ExcelColumn(HeaderName = "Document Date", DataFormat = "yyyy-mm-dd", Index = 2)]
        public DateTime DocumentDate { set; get; }

        [ExcelColumn(HeaderName = "Account", DataFormat = "@", Index = 3)]
        public int Account { set; get; }

        [ExcelColumn(HeaderName = "Amount in Local Currency", DataFormat = "#,###,##0.00", Index = 4)]
        public float AmountInLocalCurrency { set; get; }

        [ExcelColumn(HeaderName = "Local Currency", DataFormat = "@", Index = 5)]
        public string LocalCurrency { set; get; }
    }
}
