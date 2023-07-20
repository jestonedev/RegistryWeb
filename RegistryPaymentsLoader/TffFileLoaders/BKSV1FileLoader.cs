using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NPOI.XSSF.UserModel;
using RegistryPaymentsLoader.TffStrings;

namespace RegistryPaymentsLoader.TffFileLoaders
{
    class BKSV1FileLoader : TffFileLoader
    {
        public BKSV1FileLoader(DateTime? dateEnrollUfk)
        {
            DateEnrollUfk = dateEnrollUfk;
        }

        public override string Version => "BKSV1";

        public DateTime? DateEnrollUfk { get; }

        protected override TffString LoadString(string[] tffStringParts)
        {
            if (tffStringParts.Length != 3) return null;
            return new BKSV1String(tffStringParts);
        }

        public override List<TffString> Load(Stream dataStream)
        {
            var workbook = new XSSFWorkbook(dataStream);
            var tffStrings = new List<TffString>();
            for(var i = 0; i < workbook.NumberOfSheets; i++)
            {
                var sheet = workbook.GetSheetAt(i);
                for (var j = 0; j <= sheet.LastRowNum; j++)
                {
                    var row = sheet.GetRow(j);
                    if (row.Cells.Count < 6) continue;
                    var account = "";
                    try
                    {
                        account = row.GetCell(0).NumericCellValue.ToString();
                    }
                    catch (Exception e) when (e is InvalidOperationException || e is FormatException)
                    {
                        account = row.GetCell(0).StringCellValue?.Trim();
                    }
                    catch (NullReferenceException)
                    {
                        continue;
                    }

                    var sum = "";
                    try
                    {
                        sum = row.GetCell(3).NumericCellValue.ToString();
                    }
                    catch (Exception e) when (e is InvalidOperationException || e is FormatException)
                    {
                        sum = row.GetCell(3).StringCellValue?.Trim();
                    }
                    catch (NullReferenceException)
                    {
                        continue;
                    }

                    var payDate = row.GetCell(5).StringCellValue?.Trim();
                    if (payDate.Length != 10) continue;

                    if (!Regex.IsMatch(account, "^[0-9]+$") ||
                        !Regex.IsMatch(sum, "^[0-9]+([.,][0-9]{1,2})?$") || 
                        !Regex.IsMatch(payDate, "^[0-9]{2}[.][0-9]{2}[.][0-9]{4}$"))
                        continue;
                    tffStrings.Add(new BKSV1String(new string[] {
                        account,
                        row.GetCell(1).StringCellValue,
                        row.GetCell(2).StringCellValue,
                        sum,
                        row.GetCell(4).StringCellValue,
                        payDate,
                        DateEnrollUfk == null ? null : DateEnrollUfk.Value.ToString("dd.MM.yyyy")
                    }));
                }
            }
            if (tffStrings.Count == 0) throw new BDFormatException("Не найдено ни одной строки с платежом. Возможно формат представления данных не соответсвуте ожидаемому. "+
                "Поддерживаемый формат: A(Номер ЛС), B(Адрес), C(ФИО), D(Сумма), E(УК), F(Дата оплаты)");
            return tffStrings;
        }
    }
}
