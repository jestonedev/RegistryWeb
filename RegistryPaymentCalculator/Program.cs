using RegistryDb.Models;
using RegistryDb.Models.Entities.KumiAccounts;
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
                var service = new KumiAccountsDataService(db, new AddressesDataService(db), new RegistryWeb.SecurityServices.SecurityService(db, null));
                var accountsInfo = new List<KumiAccountInfoForPaymentCalculator>();
                try
                {
                    ConsoleLogger.Log("Выборка лицевых счетов");
                    var lastChargeDate = DateTime.Now.Date;
                    lastChargeDate = lastChargeDate.AddDays(-lastChargeDate.Day);
                    var accounts = db.KumiAccounts.Where(r => (r.IdState == 1 || r.IdState == 3) && r.LastChargeDate < lastChargeDate);
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
                
                var startRewriteDate = DateTime.Now.Date;
                startRewriteDate = startRewriteDate.AddDays(-startRewriteDate.Day+1);
                if (DateTime.Now.Date.Day <= 3) // Предыдущий период блокируется для перезаписи по истечении трех дней текущего периода
                {
                    startRewriteDate = startRewriteDate.AddMonths(-1);
                }
                foreach(var arg in args)
                {
                    if (string.IsNullOrWhiteSpace(arg)) continue;
                    
                    var argParts = arg.Split('=');
                    if (argParts.Length == 2 && argParts[0].Trim() == "--start-rewrite-date")
                    {
                        var startDateStr = argParts[1];
                        var startDateParts = startDateStr.Split('.');
                        if (startDateParts.Length != 3) continue;
                        if (!int.TryParse(startDateParts[0], out int day)) continue;
                        if (!int.TryParse(startDateParts[1], out int month)) continue;
                        if (!int.TryParse(startDateParts[2], out int year)) continue;
                        startRewriteDate = new DateTime(year, month, day);
                        if (startRewriteDate.Day != 1)
                        {
                            startRewriteDate = startRewriteDate.AddDays(-startRewriteDate.Day + 1);
                        }
                    }
                }

                var endCalcDate = DateTime.Now.Date;
                endCalcDate = endCalcDate.AddDays(-endCalcDate.Day + 1).AddMonths(1).AddDays(-1);

                var i = 0;
                foreach (var account in accountsInfo)
                {
                    try
                    {
                        ConsoleLogger.Log(string.Format("Выставление начисления по ЛС {0}", account.Account));
                        var startCalcDate = service.GetAccountStartCalcDate(account);
                        if (startCalcDate == null) continue;
                        var dbChargingInfo = service.GetDbChargingInfo(account);

                        startRewriteDate = service.CorrectStartRewriteDate(startRewriteDate, startCalcDate.Value, dbChargingInfo);

                        var chargingInfo = service.CalcChargesInfo(account, dbChargingInfo, startCalcDate.Value, endCalcDate, startRewriteDate);
                        var recalcInsertIntoCharge = new KumiCharge();
                        if (chargingInfo.Any())
                        {
                            recalcInsertIntoCharge = chargingInfo.Last();
                        }
                        
                        service.CalcRecalcInfo(account, chargingInfo, dbChargingInfo, recalcInsertIntoCharge, startCalcDate.Value, endCalcDate, startRewriteDate);
                        service.UpdateChargesIntoDb(account, chargingInfo, dbChargingInfo, startCalcDate.Value, endCalcDate, startRewriteDate);
                    } catch(Exception e)
                    {
                        var message = e.InnerException != null ? e.InnerException.Message : e.Message;
                        ConsoleLogger.Error(string.Format("Ошибка: {0}", message));

                        var sender = new SmtpSender(Configuration.SmtpHost, Configuration.SmtpPort);
                        sender.SendEmail("Ошибка во время выставления начислений по лицевому счету " + account.Account,
                            message, Configuration.SmtpFrom, Configuration.SmtpErrorTo);
                        
                        if (e.GetType() != typeof(ApplicationException))
                        {
                            i++;
                            if (i == 10)
                                return;
                        }
                    }
                }
            }
        }
    }
}
