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
            html += "<p>г. Братск, Ленина просп., д.37, тел.:349393, 349372 (по оплате), тел.:349632 (по начис.)</p>";
            html += "<p>Режим работы: Пн.-Пт. с 9.00-17.00, обед 13.00-14.00, сб.,вс-выходной</p>";
            html += "<p>Казн. счет 03100643000000013400 ИНН 3803201800 КПП 380401001 БИК 012520101</p>";
            html += "<p>ОТДЕЛЕНИЕ ИРКУТСКА БАНКА РОССИИ//УФК ПО ИРКУТСКОЙ ОБЛАСТИ г Иркутск</p>";
            html += "<p>Ед. каз. счет 40102810145370000026 КБК 901 1 11 09044 04 1000 120 ОКТМО 25714000</p>";
            html += string.Format("<p>Адрес: <b>{0}</b></p>", invoice.Address+(string.IsNullOrEmpty(invoice.PostIndex) ? "" : ", "+invoice.PostIndex));
            html += string.Format("<p>Лиц. счет: <b class=\"account\">{0}</b> ЕЛС ГИС ЖКХ: <b>{1}</b></p>", invoice.Account, invoice.AccountGisZkh);
            html += string.Format("<p>Потребитель: <b class=\"tenant\">{0}</b></p>", invoice.Tenant);

            html += "<table class=\"money-table\">";
            html += string.Format("<tr><td class=\"money-header\">Недоплата на {0}</td><td class=\"money-td\">{1}</td></tr>",
                new DateTime(invoice.OnDate.Year, invoice.OnDate.Month, 1).AddDays(-1).ToString("dd.MM.yyyy"),
                invoice.BalanceInput.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            html += string.Format("<tr><td class=\"money-header\">Начислено</td><td class=\"money-td\">{0}</td></tr>",
                invoice.Charging.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            html += string.Format("<tr><td class=\"money-header\">Оплачено</td><td class=\"money-td\">{0}</td></tr>",
                invoice.Payed.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            html += string.Format("<tr><td class=\"money-header\"><b>К оплате за {0}</b></td><td class=\"money-td\"><b>{1}</b></td></tr>",
                invoice.OnDate.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU")).ToLower(),
                invoice.BalanceOutput.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            html += "</table>";

            html += string.Format("<p>Срок оплаты по квитанции до <b>{0}</b> (статья 155 ЖК РФ)</p>",
                 new DateTime(invoice.OnDate.Year, invoice.OnDate.Month, 1).AddMonths(1).AddDays(9).ToString("dd.MM.yyyy"));

            html += "<table class=\"service-table\">";
            html += "<tr><td>Вид услуги</td><td>Общая площадь, кв.м</td><td>Тариф, руб.</td><td>Размер платы</td><td>Перерасчет</td><td>Итог за тек. мес.</td></tr>";
            html += string.Format("<tr><td>Плата за наем</td><td>{0}</td><td>{1}</td><td>{2}</td><td>{3}</td><td>{4}</td></tr>",
                 invoice.TotalArea.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")),
                 invoice.Tariff.ToString("N3", CultureInfo.GetCultureInfo("ru-RU")),
                 invoice.ChargingTenancy.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")),
                 invoice.RecalcTenancy.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")),
                 (invoice.ChargingTenancy + invoice.RecalcTenancy).ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            if ((invoice.ChargingPenalty + invoice.RecalcPenalty) != 0)
            {
                html += string.Format("<tr><td>Пени</td><td></td><td></td><td>{0}</td><td>{1}</td><td>{2}</td></tr>",
                    invoice.ChargingPenalty.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")),
                    invoice.RecalcPenalty.ToString("N2", CultureInfo.GetCultureInfo("ru-RU")),
                    (invoice.ChargingPenalty + invoice.RecalcPenalty).ToString("N2", CultureInfo.GetCultureInfo("ru-RU")));
            }
            html += string.Format("<tr><td><b>Итого</b></td><td></td><td></td><td><b>{0}</b></td><td><b>{1}</b></td><td><b>{2}</b></td></tr>",
                 (invoice.ChargingTenancy + invoice.ChargingPenalty).ToString("N2", CultureInfo.GetCultureInfo("ru-RU")),
                 (invoice.RecalcTenancy + invoice.RecalcPenalty).ToString("N2", CultureInfo.GetCultureInfo("ru-RU")),
                 (invoice.ChargingTenancy + invoice.RecalcTenancy + invoice.ChargingPenalty + invoice.RecalcPenalty).ToString("N2", CultureInfo.GetCultureInfo("ru-RU"))
                );
            html += "</table>";

            html += "</td>";

            html += "<td class=\"qr-td\">";
            html += string.Format("<table><tr><td><img src=\"/{0}\" class=\"qr\" /></td></tr></table>", Path.Combine(relativePath, qrFileName));
            html += string.Format("<table class=\"calc-center-table\"><tr><td class=\"calc-center-td\">Внимание!<br>"+
                "В счете-извещении<br>учтены платежи,<br>поступившие по<br>состоянию на<br>{0}</td></tr></table>", 
                new DateTime(invoice.OnDate.Year, invoice.OnDate.Month, 21).ToString("dd.MM.yyyy"));
            html += "</td>";

            html += "</tr></table>";
            return html;
        }
    }
}
