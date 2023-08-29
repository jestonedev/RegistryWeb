using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NPOI.XSSF.UserModel;
using RegistryPaymentsLoader.TffStrings;

namespace RegistryPaymentsLoader.TffFileLoaders
{
    class VTBV2FileLoader : TffFileLoader
    {

        public VTBV2FileLoader()
        {
        }

        public override string Version => "VTBV2";

        protected override TffString LoadString(string[] tffStringParts)
        {
            if (tffStringParts.Length != 5) return null;
            return new VTBV2String(tffStringParts);
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
                if (Regex.IsMatch(cells[0], "^=[0-9]+$"))
                {
                    if (cells.Length == 6)
                    {
                        var noticeDateRaw = cells[5];
                        var noticeDateParts = noticeDateRaw.Split("-");
                        if (noticeDateParts.Length != 3) continue;
                        if (!int.TryParse(noticeDateParts[0], out int day)) continue;
                        if (!int.TryParse(noticeDateParts[1], out int month)) continue;
                        if (!int.TryParse(noticeDateParts[2], out int year)) continue;
                        NoticeDate = new DateTime(year, month, day);
                    }
                    continue;
                }
                if (cells.Length != 12) continue;
                
                var tenant = cells[5].ToString();
                var address = cells[6].ToString();
                var sum = cells[8];

                var payDate = cells[0].Trim();
                if (payDate.Length != 10) continue;
                payDate = payDate.Replace("-", ".");

                if (!Regex.IsMatch(sum, "^[0-9]+([.,][0-9]{1,2})?$") || 
                    !Regex.IsMatch(payDate, "^[0-9]{2}[.][0-9]{2}[.][0-9]{4}$"))
                    continue;
                tffStrings.Add(new VTBV2String(new string[] {
                    tenant,
                    address,
                    sum,
                    payDate
                }));
            }
            if (tffStrings.Count == 0) throw new BDFormatException("Не найдено ни одной строки с платежом. Возможно формат представления данных не соответсвуте ожидаемому");
            if (NoticeDate != null)
            {
                foreach(var tffString in tffStrings)
                {
                    (tffString as VTBV2String).NoticeDate = NoticeDate;
                }
            }
            return tffStrings;
        }
    }
}
