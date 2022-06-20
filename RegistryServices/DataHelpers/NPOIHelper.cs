using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryServices.DataHelpers
{
    public static class NPOIHelper
    {

        public static ICellStyle GetActHeaderCellStyle(HSSFWorkbook workbook)
        {
            var headerCellStyle = workbook.CreateCellStyle();
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
            return cellStyle;
        }

        public static void CreateActSpanedCell(ISheet sheet, int rowIndex, int columnIndex, int spanRowCount, int spanColumnCount, string value, ICellStyle style, CellType cellType = CellType.String)
        {
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
    }
}
