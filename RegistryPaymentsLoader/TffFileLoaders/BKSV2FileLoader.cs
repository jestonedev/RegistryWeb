using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NPOI.XSSF.UserModel;
using RegistryPaymentsLoader.TffStrings;

namespace RegistryPaymentsLoader.TffFileLoaders
{
    class BKSV2FileLoader : TffFileLoader
    {
        public BKSV2FileLoader(DateTime? dateEnrollUfk)
        {
            DateEnrollUfk = dateEnrollUfk;
            NoticeDate = dateEnrollUfk;
        }

        public override string Version => "BKSV2";

        public DateTime? DateEnrollUfk { get; }

        protected override TffString LoadString(string[] tffStringParts)
        {
            if (tffStringParts.Length != 3) return null;
            return new BKSV1String(tffStringParts);
        }

        public override List<TffString> Load(Stream dataStream)
        {
            var tffStrings = new List<TffString>();
            var txt = new StreamReader(dataStream, System.Text.Encoding.GetEncoding("windows-1251")).ReadToEnd();
            dataStream.Close();
            var lines = txt.Split("\r\n");
            for (var j = 0; j < lines.Length; j++)
            {
                var line = lines[j];
                if (string.IsNullOrEmpty(line)) continue;
                var cells = line.Split(";");
                if (cells.Length != 11) continue;
                if (cells[0] != "- - -") continue;
                var account = cells[2].ToString();

                var sum = cells[3];

                var payDate = cells[8].Trim();
                if (payDate.Length != 10) continue;
                payDate = payDate.Replace("/", ".");

                if (!Regex.IsMatch(account, "^[0-9]+$") ||
                    !Regex.IsMatch(sum, "^[-]?[0-9]+([.,][0-9]{1,2})?$") || 
                    !Regex.IsMatch(payDate, "^[0-9]{2}[.][0-9]{2}[.][0-9]{4}$"))
                    continue;
                tffStrings.Add(new BKSV2String(new string[] {
                    account,
                    cells[1],
                    sum,
                    payDate,
                    DateEnrollUfk == null ? null : DateEnrollUfk.Value.ToString("dd.MM.yyyy")
                }));
            }
            if (tffStrings.Count == 0) throw new BDFormatException("Не найдено ни одной строки с платежом. Возможно формат представления данных не соответсвуте ожидаемому. "+
                "Поддерживаемый формат: - - -;<адрес>;<ЛС>;<сумма>;<номер услуги>;;;;<дата платежа>;;");
            return tffStrings;
        }
    }
}
