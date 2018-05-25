using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinancialAccountTool.ExcelSerialzation.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExcelWorksheet : Attribute
    {
        public string SheetName { set; get; }
    }
}
