using FinancialAccountTool.ExcelSerialzation.Attributes;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FinancialAccountTool.ExcelSerialzation
{
    public class ExcelSerializer : IDisposable
    {
        private readonly IWorkbook _workbook;
        private readonly string _filePath;

        public ExcelSerializer(string filePath)
        {
            _workbook = new XSSFWorkbook();
            _filePath = filePath;
        }

        public void Serialize<T>(IEnumerable<T> dataList)
        {
            if (!dataList.Any())
            {
                return;
            }

            var type = typeof(T);
            var worksheetAttribute =
                (ExcelWorksheet)type.GetCustomAttributes().First(attr => attr.GetType() == typeof(ExcelWorksheet));
            var worksheetName = worksheetAttribute.SheetName;

            var worksheet = _workbook.CreateSheet(worksheetName);
            var dataFormat = _workbook.CreateDataFormat();
            var patriarch = worksheet.CreateDrawingPatriarch();

            var headerRowStyle = _workbook.CreateCellStyle();
            var font = _workbook.CreateFont();
            font.Boldweight = (short)FontBoldWeight.Bold;
            font.FontHeightInPoints = 12;
            font.Color = HSSFColor.White.Index;
            headerRowStyle.SetFont(font);
            headerRowStyle.Alignment = HorizontalAlignment.Center;
            headerRowStyle.FillForegroundColor = HSSFColor.RoyalBlue.Index;
            headerRowStyle.FillPattern = FillPattern.SolidForeground;

            var dataProperties = type.GetProperties().OrderBy(prop =>
            {
                var columnAttribute =
                    (ExcelColumn)prop.GetCustomAttributes().First(attr => attr.GetType() == typeof(ExcelColumn));
                return columnAttribute.Index;
            });

            var rowIndex = 0;
            var columnIndex = 0;
            var row = worksheet.CreateRow(rowIndex++);
            foreach (var dataProperty in dataProperties)
            {
                var columnAttribute = (ExcelColumn)dataProperty.GetCustomAttributes()
                    .First(attr => attr.GetType() == typeof(ExcelColumn));

                var cell = row.CreateCell(columnIndex++);
                cell.SetCellType(CellType.String);
                cell.SetCellValue(columnAttribute.HeaderName);
                cell.CellStyle = headerRowStyle;

            }
            worksheet.SetAutoFilter(new CellRangeAddress(0, 0, 0, dataProperties.Count() - 1));

            foreach (var data in dataList)
            {
                row = worksheet.CreateRow(rowIndex++);
                columnIndex = 0;
                foreach (var dataProperty in dataProperties)
                {
                    var columnAttribute = (ExcelColumn)dataProperty.GetCustomAttributes()
                        .First(attr => attr.GetType() == typeof(ExcelColumn));
                    var dataValue = dataProperty.GetValue(data);

                    var cell = row.CreateCell(columnIndex++);
                    var cellStyle = _workbook.CreateCellStyle();
                    cellStyle.DataFormat = dataFormat.GetFormat(columnAttribute.DataFormat);
                    cell.CellStyle = cellStyle;

                    if (columnAttribute.IsList)
                    {
                        var outputList = (from object v in (IEnumerable)dataValue select Convert.ToString(v)).ToList();
                        cell.SetCellValue(string.Join(", ", outputList));
                    }
                    else if (columnAttribute.IsImageLink)
                    {

                        using (var webClient = new WebClient())
                        {
                            try
                            {
                                var image = Image.FromStream(webClient.OpenRead(new Uri(Convert.ToString(dataValue))));

                                var sizeRatio = ((decimal) image.Height / image.Width);
                                var thumbHeight = 100;
                                var thumbWidth = decimal.ToInt32(sizeRatio * thumbHeight);
                                var thumbStream =
                                    image.GetThumbnailImage(thumbWidth, thumbHeight, () => false, IntPtr.Zero);
                                var memoryStream = new MemoryStream();
                                thumbStream.Save(memoryStream, ImageFormat.Jpeg);

                                var pictureIndex = _workbook.AddPicture(memoryStream.ToArray(), PictureType.JPEG);

                                var anchor = new XSSFClientAnchor(0, 0, 0, 0, cell.ColumnIndex, cell.RowIndex, 0, 0);
                                var picture = patriarch.CreatePicture(anchor, pictureIndex);
                                var size = picture.GetImageDimension();
                                row.HeightInPoints = size.Height;
                                picture.Resize();

                                anchor.Dx1 = 5;
                                anchor.Dy1 = 2;
                            }
                            catch (Exception ex)
                            {
                                cell.SetCellValue(ex.Message);
                            }
                        }
                    }
                    else if (columnAttribute.IsLink)
                    {
                        var hlinkStyle = _workbook.CreateCellStyle();
                        var hlinkFont = _workbook.CreateFont();
                        hlinkFont.Underline = FontUnderlineType.Single;
                        hlinkFont.Color = HSSFColor.Blue.Index;
                        hlinkStyle.SetFont(hlinkFont);

                        var link =
                            new XSSFHyperlink(HyperlinkType.Url) { Address = (Convert.ToString(dataValue)) };
                        cell.Hyperlink = (link);
                        cell.SetCellValue(Convert.ToString(dataValue));
                        cell.CellStyle = (hlinkStyle);
                    }
                    else
                    {
                        if (dataValue is bool)
                        {
                            cell.SetCellValue((bool)dataValue);
                            cell.SetCellType(CellType.Boolean);
                        }
                        else if (dataValue is int)
                        {
                            cell.SetCellValue((int)dataValue);
                            cell.SetCellType(CellType.Numeric);
                        }
                        else if (dataValue is long)
                        {
                            cell.SetCellValue((long)dataValue);
                            cell.SetCellType(CellType.Numeric);
                        }
                        else if (dataValue is float)
                        {
                            cell.SetCellValue((float)dataValue);
                            cell.SetCellType(CellType.Numeric);
                        }
                        else if (dataValue is double)
                        {
                            cell.SetCellValue((double)dataValue);
                            cell.SetCellType(CellType.Numeric);
                        }
                        else if (dataValue is DateTime)
                        {
                            cell.SetCellValue((DateTime)dataValue);
                        }
                        else
                        {
                            cell.SetCellValue(Convert.ToString(dataValue));
                            cell.SetCellType(CellType.String);
                        }

                        //                        switch (dataValue.GetType())
                        //                        {
                        //                            case bool value:
                        //                                cell.SetCellValue(value);
                        //                                cell.SetCellType(CellType.Boolean);
                        //                                break;
                        //                            case int value:
                        //                                cell.SetCellValue(value);
                        //                                cell.SetCellType(CellType.Numeric);
                        //                                break;
                        //                            case long value:
                        //                                cell.SetCellValue(value);
                        //                                cell.SetCellType(CellType.Numeric);
                        //                                break;
                        //                            case float value:
                        //                                cell.SetCellValue(value);
                        //                                cell.SetCellType(CellType.Numeric);
                        //                                break;
                        //                            case double value:
                        //                                cell.SetCellValue(value);
                        //                                cell.SetCellType(CellType.Numeric);
                        //                                break;
                        //                            case DateTime value:
                        //                                cell.SetCellValue(value);
                        //                                break;
                        //                            default:
                        //                                cell.SetCellValue(Convert.ToString(dataValue));
                        //                                cell.SetCellType(CellType.String);
                        //                                break;
                        //                        }
                    }
                }
            }

            for (var i = 0; i < columnIndex; i++)
            {
                worksheet.AutoSizeColumn(i);
            }
        }

        public void Dispose()
        {
            try
            {
                using (var fileStream = File.OpenWrite(_filePath))
                {
                    _workbook.Write(fileStream);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }
}
