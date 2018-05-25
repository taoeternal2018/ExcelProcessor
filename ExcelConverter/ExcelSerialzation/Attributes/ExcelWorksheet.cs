using System;

namespace FinancialAccountTool.ExcelSerialzation.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExcelWorksheet : Attribute
    {
        public string SheetName { set; get; }
    }
}
