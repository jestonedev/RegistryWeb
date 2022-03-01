using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net.Mail;
using System.Net;

namespace RegistryPaymentCalculator
{
    internal class SmtpSender
    {
        readonly string sHost;
        readonly int iPort;

        public SmtpSender(string host, int port)
        {
            sHost = host;
            iPort = port;
        }

        public void SendEmail(string subject, string body, string from, List<string> to)
        {
            if (!to.Any())
            {
                return;
            }
            try
            {

                using (MailMessage message = new MailMessage())
                {
                    message.From = new MailAddress(from);
                    foreach (var toEmail in to)
                    {
                        message.To.Add(new MailAddress(toEmail));
                    }
                    message.Subject = subject;//тема письма 
                    message.SubjectEncoding = Encoding.UTF8;
                    message.Body = body;// Текст письма 
                    message.BodyEncoding = Encoding.UTF8;
                    using (SmtpClient smtp = new SmtpClient(sHost, iPort))
                    {
                        smtp.Credentials = CredentialCache.DefaultNetworkCredentials;//для проверки подлинности отправителя
                        smtp.Send(message);//отправить сообщение 
                    }
                }
            }
            catch (SmtpFailedRecipientException)
            {

            }
        }
    }
}
