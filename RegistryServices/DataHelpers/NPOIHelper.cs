using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.Collections.Generic;

namespace RegistryServices.DataHelpers
{
    public static class NPOIHelper
    {
        public static ICellStyle headerCellStyle = null;
        public static Dictionary<string, ICellStyle> baseDataCellStyles = new Dictionary<string, ICellStyle>();
        private static HSSFWorkbook workbook;

        public static ICellStyle GetActHeaderCellStyle(HSSFWorkbook workbook)
        {
            if (workbook != NPOIHelper.workbook)
            {
                baseDataCellStyles = new Dictionary<string, ICellStyle>();
                headerCellStyle = null;
            }
            if (headerCellStyle != null) return headerCellStyle;
            headerCellStyle = workbook.CreateCellStyle();
            headerCellStyle.BorderTop = BorderStyle.Thin;
            headerCellStyle.BorderBottom = BorderStyle.Thin;
            headerCellStyle.BorderLeft = BorderStyle.Thin;
            headerCellStyle.BorderRight = BorderStyle.Thin;
            headerCellStyle.WrapText = true;
            headerCellStyle.Alignment = HorizontalAlignment.Center;
            var headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerCellStyle.SetFont(headerFont);
            return headerCellStyle;
        }

        public static ICellStyle GetActBaseDataCellStyle(HSSFWorkbook workbook, HorizontalAlignment alignment, bool isItalic = false, bool isBold = false)
        {
            if (workbook != NPOIHelper.workbook)
            {
                baseDataCellStyles = new Dictionary<string, ICellStyle>();
                headerCellStyle = null;
            }
            var keyName = alignment.ToString() + isItalic.ToString() + isBold.ToString();
            if (baseDataCellStyles.ContainsKey(keyName))
                return baseDataCellStyles[keyName];
            var cellStyle = workbook.CreateCellStyle();
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;
            cellStyle.WrapText = true;
            cellStyle.Alignment = alignment;
            var font = workbook.CreateFont();
            font.IsItalic = isItalic;
            font.IsBold = isBold;
            cellStyle.SetFont(font);
            baseDataCellStyles.Add(keyName, cellStyle);
            return cellStyle;
        }

        public static void CreateActSpanedCell(ISheet sheet, int rowIndex, int columnIndex, int spanRowCount, int spanColumnCount, string value, ICellStyle style, CellType cellType = CellType.String)
        {
            if (spanRowCount == 1 && spanColumnCount == 1)
            {
                CreateActCell(sheet, rowIndex, columnIndex, value, style, cellType);
                return;
            }
            for (var j = rowIndex; j < rowIndex + spanRowCount; j++)
            {
                var row = sheet.GetRow(j);
                if (row == null) row = sheet.CreateRow(j);
                for (var i = columnIndex; i < columnIndex + spanColumnCount; i++)
                {
                    var cell = row.CreateCell(i, cellType);
                    if (i == columnIndex)
                        cell.SetCellValue(value);
                    cell.CellStyle = style;
                }
            }
            var mergeRange = new CellRangeAddress(rowIndex, rowIndex - 1 + spanRowCount, columnIndex, columnIndex - 1 + spanColumnCount);
            sheet.AddMergedRegion(mergeRange);
        }

        public static void CreateActCell(ISheet sheet, int rowIndex, int columnIndex, string value, ICellStyle style, CellType cellType = CellType.String)
        {
            var row = sheet.GetRow(rowIndex);
            if (row == null) row = sheet.CreateRow(rowIndex);
            var cell = row.CreateCell(columnIndex, cellType);
            cell.SetCellValue(value);
            cell.CellStyle = style;
        }

        public static void CreateActCell(ISheet sheet, int rowIndex, int columnIndex, double value, ICellStyle style, CellType cellType = CellType.String)
        {
            var row = sheet.GetRow(rowIndex);
            if (row == null) row = sheet.CreateRow(rowIndex);
            var cell = row.CreateCell(columnIndex, cellType);
            cell.SetCellValue(value);
            cell.CellStyle = style;
        }

        public static ICellStyle GetPaymentsKbkHeaderCellStyle(HSSFWorkbook workbook)
        {
            if (workbook != NPOIHelper.workbook)
            {
                baseDataCellStyles = new Dictionary<string, ICellStyle>();
                headerCellStyle = null;
                NPOIHelper.workbook = workbook;
            }
            if (headerCellStyle != null) return headerCellStyle;
            headerCellStyle = workbook.CreateCellStyle();
            headerCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.RoyalBlue.Index;
            headerCellStyle.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.RoyalBlue.Index;
            headerCellStyle.FillPattern = FillPattern.SolidForeground;
            headerCellStyle.BorderTop = BorderStyle.Thin;
            headerCellStyle.BorderBottom = BorderStyle.Thin;
            headerCellStyle.BorderLeft = BorderStyle.Thin;
            headerCellStyle.BorderRight = BorderStyle.Thin;
            headerCellStyle.WrapText = true;
            headerCellStyle.Alignment = HorizontalAlignment.Center;
            var headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerFont.Color = HSSFColor.White.Index;
            headerFont.FontHeightInPoints = 12;
            headerFont.FontName = "Tahoma";
            headerCellStyle.SetFont(headerFont);
            return headerCellStyle;
        }

        public static ICellStyle GetPaymentsKbkBaseDataCellStyle(HSSFWorkbook workbook, HorizontalAlignment horizontal, VerticalAlignment vertical, bool isItalic = false, bool isBold = false)
        {
            if (workbook != NPOIHelper.workbook)
            {
                baseDataCellStyles = new Dictionary<string, ICellStyle>();
                headerCellStyle = null;
                NPOIHelper.workbook = workbook;
            }
            var keyName = horizontal.ToString() + isItalic.ToString() + isBold.ToString();
            if (baseDataCellStyles.ContainsKey(keyName))
                return baseDataCellStyles[keyName];
            var cellStyle = workbook.CreateCellStyle();
            cellStyle.BorderTop = BorderStyle.Thin;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderRight = BorderStyle.Thin;
            cellStyle.WrapText = true;
            cellStyle.Alignment = horizontal;
            cellStyle.VerticalAlignment = vertical;
            var font = workbook.CreateFont();
            font.IsItalic = isItalic;
            font.IsBold = isBold;
            cellStyle.SetFont(font);
            baseDataCellStyles.Add(keyName, cellStyle);
            return cellStyle;
        }
    }
}
