﻿using RegistryDb.Models;
using RegistryWeb.DataServices;
using RegistryWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace RegistryPaymentCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = string.Format(Configuration.ConnectionString, Configuration.DbName);

            using (var db = new RegistryContext(connectionString, Configuration.DbName))
            {
                var service = new KumiAccountsDataService(db, new AddressesDataService(db));
                var accountsInfo = new List<KumiAccountInfoForPaymentCalculator>();
                try
                {
                    ConsoleLogger.Log("Выборка лицевых счетов");
                    var accounts = db.KumiAccounts.Where(r => r.IdState == 1);
                    ConsoleLogger.Log("Подготовка лицевых счетов");
                    var accountsPrepare = service.GetAccountsPrepareForPaymentCalculator(accounts);
                    accountsInfo = service.GetAccountInfoForPaymentCalculator(accountsPrepare);
                } catch(Exception e)
                {
                    ConsoleLogger.Error(string.Format("Ошибка: {0}", e.Message));

                    var sender = new SmtpSender(Configuration.SmtpHost, Configuration.SmtpPort);
                    sender.SendEmail("Ошибка во время подготовки лицевых счетов к выставлению начислений", 
                        e.Message, Configuration.SmtpFrom, Configuration.SmtpErrorTo);
                    return;
                }

                var forceDeleteOldCharges = false;
                var startDate = DateTime.Now.Date;
                startDate = startDate.AddDays(-startDate.Day+1).AddMonths(-1);
                foreach(var arg in args)
                {
                    if (string.IsNullOrWhiteSpace(arg)) continue;
                    
                    var argParts = arg.Split('=');
                    if (argParts.Length == 2 && argParts[0].Trim() == "--start-date")
                    {
                        var startDateStr = argParts[1];
                        var startDateParts = startDateStr.Split('.');
                        if (startDateParts.Length != 3) continue;
                        if (!int.TryParse(startDateParts[0], out int day)) continue;
                        if (!int.TryParse(startDateParts[1], out int month)) continue;
                        if (!int.TryParse(startDateParts[2], out int year)) continue;
                        startDate = new DateTime(year, month, day);
                        if (startDate.Day != 1)
                        {
                            startDate = startDate.AddDays(-startDate.Day + 1);
                        }
                    }
                    if (argParts.Length == 1 && argParts[0].Trim() == "--delete-old-charges")
                    {
                        forceDeleteOldCharges = true;
                    }
                }

                var endDate = DateTime.Now.Date;
                endDate = endDate.AddDays(-endDate.Day);

                foreach (var account in accountsInfo)
                {
                    try
                    {
                        ConsoleLogger.Log(string.Format("Выставление начисления по ЛС {0}", account.Account));
                        var chargingInfo = service.CalcChargesInfo(account, startDate, endDate, forceDeleteOldCharges);
                        service.UpdateChargesIntoDb(account, chargingInfo, forceDeleteOldCharges);
                    } catch(Exception e)
                    {
                        ConsoleLogger.Error(string.Format("Ошибка: {0}", e.Message));

                        var sender = new SmtpSender(Configuration.SmtpHost, Configuration.SmtpPort);
                        sender.SendEmail("Ошибка во время выставления начислений",
                            e.Message, Configuration.SmtpFrom, Configuration.SmtpErrorTo);
                        if (e.GetType() != typeof(ApplicationException))
                        {
                            return;
                        }
                    }
                }
            }
        }
    }
}
