using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;

namespace InvoiceGenerator
{
    public class Qr
    {
        public string GetQrInvoiceContent(InvoiceGeneratorParamTyped invoiceInfo)
        {
            var snpParts = invoiceInfo.Tenant.Split(" ");
            return string.Format(@"ST00011|Name=УФК по Иркутской области (КУМИ г.Братска)|PersonalAcc=03100643000000013400|KPP=380401001|BankName=Отд. Иркутск Банка России//УФК по Ирк.обл.г.Иркутск|BIC=012520101|CorrespAcc=40102810145370000026|Sum={0}|Purpose=Оплата коммунальных услуг|PayeeINN=3803201800|lastName={1}|firstName={2}|middleName={3}|PayerAddress={4}|PersAcc={5}|PaymPeriod={6}|ServiceName=2222|CBC=90111109044041000120|OKTMO=25714000|category=Коммунальные услуги|",
                invoiceInfo.BalanceOutput.ToString().Replace(".", "").Replace(",", ""),
                snpParts.Length > 0 ? snpParts[0]?.Trim() : "",
                snpParts.Length > 1 ? snpParts[1]?.Trim() : "",
                snpParts.Length > 2 ? snpParts[2]?.Trim() : "",
                invoiceInfo.Address,
                invoiceInfo.Account,
                invoiceInfo.OnDate.ToString("MMMM yyyy", CultureInfo.GetCultureInfo("ru-RU")).ToLower());
        }

        public Bitmap QrGenerate(string content)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            return qrCode.GetGraphic(2);
        }

        public bool QrSave(Bitmap qr, string fileName)
        {
            try
            {
                qr.Save(fileName);
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
