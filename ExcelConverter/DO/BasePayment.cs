using System;
using System.Windows;
using NPOI.SS.UserModel;
using FinancialAccountTool.ExcelSerialzation.Attributes;

namespace FinanicalAccountModernClient.DO
{
    public abstract class BasePayment
    {
        protected BasePayment()
        {
        }

        protected BasePayment(IRow row)
        {
            try
            {
                Date = row.Cells[0].DateCellValue;
                Account = Convert.ToInt32(row.Cells[1].ToString());
                Credit = (float) row.Cells[2].NumericCellValue;
                Currency = row.Cells[3].StringCellValue;
            }
            catch (FormatException e)
            {
                //MessageBox.Show($@"Parse failed because the data format is incorrect, please double check {this}. {Environment.NewLine} {e.Message}",
                //    @"Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //Application.Current.Shutdown();
                string errorMsg = string.Format($@"Parse failed because the data format is incorrect, please double check {this}. {Environment.NewLine} {e.Message}", @"Error");
                Exception ex = new Exception(errorMsg);
                throw (ex);
            }
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
