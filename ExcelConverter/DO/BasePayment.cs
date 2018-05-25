using FinancialAccountTool.ExcelSerialzation.Attributes;
using NPOI.SS.UserModel;
using System;

namespace FinanicalAccountModernClient.DO
{
    public abstract class BasePayment
    {
        protected BasePayment()
        {
        }

        protected BasePayment(IRow row)
        {
            Date = row.Cells[0].DateCellValue;
            Account = Convert.ToInt32(row.Cells[1].ToString());
            Credit = (float) row.Cells[2].NumericCellValue;
            Currency = row.Cells[3].StringCellValue;
        }

        public override string ToString()
        {
            return $"{nameof(Date)}: {Date}, {nameof(Account)}: {Account}, {nameof(Credit)}: {Credit}, {nameof(Currency)}: {Currency}";
        }

        [ExcelColumn(HeaderName = "Account", DataFormat = "@", Index = 1)]
        public int Account { set; get; }

        [ExcelColumn(HeaderName = "Date", DataFormat = "yyyy-mm-dd", Index = 2)]
        public DateTime Date { set; get; }

        [ExcelColumn(HeaderName = "Currency", DataFormat = "@", Index = 3)]
        public string Currency { set; get; }

        [ExcelColumn(HeaderName = "Credit", DataFormat = "#,###,##0.00", Index = 4)]
        public float Credit { set; get; }
    }
}
