using System;
using System.Reflection;

namespace FinancialAccountTool.ExcelSerialzation.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ExcelColumn : Attribute
    {
        public string HeaderName { set; get; }
        public string DataFormat { set; get; }
        public int Index { set; get; }
        public bool IsList { set; get; }
        public bool IsLink { set; get; }
        public bool IsImageLink { set; get; }
        public Type ListType { set; get; }
    }
}
