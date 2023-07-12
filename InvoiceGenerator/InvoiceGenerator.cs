using System;
using System.Globalization;
using System.IO;

namespace InvoiceGenerator
{
    public class InvoiceGenerator
    {
        private readonly string baseDirectory;
        private readonly string relativePath;

        public InvoiceGenerator(string baseDirectory, string relativePath)
        {
            var directory = Path.Combine(baseDirectory, relativePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            this.baseDirectory = baseDirectory;
            this.relativePath = relativePath;
        }

        public string GenerateHtml(InvoiceGeneratorParamTyped invoice)
        {
            var qrFileName = Guid.NewGuid() + ".bmp";
            var qrFullFileName = Path.Combine(baseDirectory, relativePath, qrFileName);
            var qr = new Qr();
            if (!qr.QrSave(qr.QrGenerate(qr.GetQrInvoiceContent(invoice)), qrFullFileName))
                throw new Exception("Ошибка формирования QR-кода для лицевого счета № "+invoice.Account);

            //var invoiceTyped = TypedIGP(invoice);

            var html = "<table class=\"main-table\"><tr>";

            html += "<td class=\"invoice-header-td\"><img src=\"/image/invoice-title.png\" width=\"19\" height=\"147\" /></td>";

            html += "<td class=\"main-content-td\">";
            html += "<p>Получатель: <b>УФК по Иркутской области (КУМИ г.Братска)</b></p>";
            html += "<p>г. Братск, Ленина просп., д.37, email: , тел.:349372 (по оплате), тел.:349390 (по начис.)</p>";
            html += "<p>Режим работы: Пн.-Пт. с 9.00-17.00, обед 13.00-14.00, сб.,вс-выходной</p>";
            html += "<p>Расчетный счет 03100643000000013400 ИНН 3803201800 БИК 012520101</p>";
            html += "<p>Отд. Иркутск Банка России//УФК по Ирк.обл.г.Иркутск, к/с 40102810145370000026</p>";
            html += string.Format("<p>Адрес: <b>{0}</b></p>", invoice.Address);
            html += string.Format("<p>Лиц. счет: <b class=\"account\">{0}</b> ЕЛС ГИС ЖКХ:</p>", invoice.Account);
            html += string.Format("<p>Потребитель: <b>{0}</b></p>", invoice.Tenant);

            html += "<table class=\"money-table\">";
            html += string.Format("<tr><td class=\"money-header\">Недоплата на {0}г.</td><td class=\"money-td\">{1}</td></tr>",
                new DateTime(invoice.OnDate.Year, invoice.OnDate.Month, 1).AddDays(-1).ToString("dd.MM.yyyy"),
                invoice.BalanceInput.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            html += string.Format("<tr><td class=\"money-header\">Начислено</td><td class=\"money-td\">{0}</td></tr>",
                invoice.Charging.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            html += string.Format("<tr><td class=\"money-header\">Оплачено</td><td class=\"money-td\">{0}</td></tr>",
                invoice.Payed.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            html += string.Format("<tr><td class=\"money-header\"><b>К оплате за {0}г.</b></td><td class=\"money-td\"><b>{1}</b></td></tr>",
                invoice.OnDate.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU")).ToLower(),
                invoice.BalanceOutput.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            html += "</table>";

            html += string.Format("<p>Площадь общая: <b class=\"total-area\">{0}</b> количество проживающих: <b>{0}</b> чел.</p>",
                invoice.TotalArea.ToString("N1", CultureInfo.GetCultureInfo("ru-RU")),
                invoice.Prescribed);

            html += "<table class=\"area-table\"><tr>";
            html += "<td class=\"area-header\">Общая площадь ж.п.:</td>";
            html += "<td class=\"area-header\">Площадь нежил. п.:</td>";
            html += "<td class=\"area-header\">Площадь ОИ:</td>";
            html += "<td class=\"area-header\">Площадь ОИ э/э:</td>";
            html += "</tr></table>";

            html += "<table class=\"service-table\">";
            html += "<tr><td>Вид услуги</td><td>Ед. изм.</td><td>Норматив</td><td>Объем/Кол-во</td><td>Тариф, руб.</td><td>Размер платы</td><td>Пере-расчет</td><td>Итог за тек. мес.</td></tr>";
            html += string.Format("<tr><td>Плата за наем КБК 90111109044041000120</td><td>кв.м.</td><td></td><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>",
                 invoice.TotalArea.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")),
                 invoice.TotalArea == 0 ? "0" : Math.Round(invoice.ChargingTenancy / (decimal)invoice.TotalArea, 3).ToString("N3", CultureInfo.GetCultureInfo("ru-RU")),
                 invoice.ChargingTenancy.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")),
                 invoice.RecalcTenancy.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")),
                 (invoice.ChargingTenancy + invoice.RecalcTenancy).ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            html += string.Format("<tr><td><b>Итого</b></td><td></td><td></td><td></td><td></td><td><b>{0}</b></td><td><b>{1}</b></td><td><b>{2}</b></td></tr>",
                 invoice.ChargingTenancy.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")),
                 invoice.RecalcTenancy.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")),
                 (invoice.ChargingTenancy + invoice.RecalcTenancy).ToString("N2", CultureInfo.GetCultureInfo("ru-RU"))
                );
            if ((invoice.ChargingPenalty + invoice.RecalcPenalty) != 0)
            {
                html += string.Format("<tr><td>Пени</td><td></td><td></td><td></td><td></td><td>{0}</td><td>{1}</td><td>{2}</td></tr>",
                    invoice.ChargingPenalty.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")),
                    invoice.RecalcTenancy.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")),
                    (invoice.ChargingPenalty + invoice.RecalcPenalty).ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            }
            html += "</table>";

            html += "</td>";

            html += "<td class=\"qr-td\">";
            html += string.Format("<table><tr><td><img src=\"/{0}\" class=\"qr\" /></td></tr></table>", Path.Combine(relativePath, qrFileName));
            html += "<table class=\"calc-center-table\"><tr><td class=\"calc-center-td\"><img src=\"/image/calc-center.png\" width=\"63\" height=\"168\" /></td></tr></table>";
            html += "</td>";

            html += "</tr></table>";
            return html;
        }
    }
}
