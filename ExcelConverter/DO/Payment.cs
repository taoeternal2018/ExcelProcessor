using FinancialAccountTool.ExcelSerialzation.Attributes;
using NPOI.SS.UserModel;
using System.Collections.Generic;
using System.Globalization;

namespace FinanicalAccountModernClient.DO
{
    [ExcelWorksheet(SheetName = "Payments")]
    public class Payment : BasePayment
    {
        public Payment(IRow row) : base(row)
        {
            Balance = Credit;
            FulfilledAssignments = new List<long>();
        }

        public string GetKey()
        {
            return Account + Date.ToString(CultureInfo.CurrentCulture);
        }

        [ExcelColumn(HeaderName = "Balance", DataFormat = "#,###,##0.00", Index = 5)]
        public float Balance { set; get; }

        [ExcelColumn(HeaderName = "Fulfilled Assignments", DataFormat = "@", IsList = true, ListType = typeof(List<long>), Index = 6)]
        public List<long> FulfilledAssignments { set; get; }
    }
}
