﻿using RegistryDb.Models;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryServices.DataFilterServices;
using RegistryServices.DataServices.KumiAccounts;
using RegistryWeb.DataServices;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions.Filter;
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
            var runDate = DateTime.Now.Date;
            var preLaunch = false;
            foreach(var arg in args)
            {
                if (arg == "--pre-launch") preLaunch = true;
            }

            var connectionString = string.Format(Configuration.ConnectionString, Configuration.DbName);

            using (var db = new RegistryContext(connectionString, Configuration.DbName))
            {
                var massChargeInfo = db.KumiChargeMassChargeInfos.First();
                var lastCalcDate = DateTime.Now.Date;

                var service = new KumiAccountsCalculationService(db, new KumiAccountsTenanciesService(db));
                var sender = new SmtpSender(Configuration.SmtpHost, Configuration.SmtpPort);
                var accountsInfo = new List<KumiAccountInfoForPaymentCalculator>();
                try
                {
                    Logger.Log("Выборка лицевых счетов");
                    lastCalcDate = lastCalcDate.AddDays(-lastCalcDate.Day);
                    if (preLaunch) lastCalcDate = lastCalcDate.AddDays(1).AddMonths(1).AddDays(-1);

                    if (massChargeInfo.LastCalcDate == lastCalcDate) return;

                    var accounts = db.KumiAccounts.Where(r => (r.IdState == 1 || r.IdState == 3) && (r.LastCalcDate == null || r.LastCalcDate < lastCalcDate));
                    Logger.Log(string.Format("Найдено {0} лицевых счетов", accounts.Count()));
                    Logger.Log("Подготовка лицевых счетов");
                    var accountsPrepare = service.GetAccountsPrepareForPaymentCalculator(accounts);
                    accountsInfo = service.GetAccountInfoForPaymentCalculator(accountsPrepare);
                } catch(Exception e)
                {
                    Logger.Error(string.Format("Ошибка: {0}", e.Message));
                    sender.SendEmail("Ошибка во время подготовки лицевых счетов к выставлению начислений", 
                        e.Message, Configuration.SmtpFrom, Configuration.SmtpErrorTo);
                    return;
                }
                
                var startRewriteDate = DateTime.Now.Date;
                startRewriteDate = startRewriteDate.AddDays(-startRewriteDate.Day+1);

                var endCalcDate = DateTime.Now.Date;
                endCalcDate = endCalcDate.AddDays(-endCalcDate.Day + 1).AddMonths(1).AddDays(-1);

                /*if (DateTime.Now.Date.Day >= 25) // Предыдущий период блокируется после 25 числа
                {
                    startRewriteDate = startRewriteDate.AddMonths(1);
                    endCalcDate = endCalcDate.AddDays(1).AddMonths(1).AddDays(-1);
                }*/
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


                var i = 0;
                var accountIndex = 0;
                var accountCount = accountsInfo.Count();
                foreach (var account in accountsInfo)
                {
                    accountIndex++;
                    var accountLocal = account;
                    try
                    {
                        Logger.Log(string.Format("[{1}/{2}] Выставление начисления по ЛС {0}", account.Account, accountIndex, accountCount));
                        // Проверка на изменения в течение выполнения массового расчета
                        var accountDb = db.KumiAccounts.FirstOrDefault(r => r.IdAccount == account.IdAccount);
                        if (accountDb == null || (accountDb.IdState != 1 && accountDb.IdState != 3)) continue;
                        if (accountDb.LastCalcDate >= runDate)
                        {
                            accountLocal = 
                                service.GetAccountInfoForPaymentCalculator(
                                    service.GetAccountsPrepareForPaymentCalculator(db.KumiAccounts.Where(r => r.IdAccount == account.IdAccount))).FirstOrDefault();
                        }

                        var startCalcDate = service.GetAccountStartCalcDate(accountLocal);
                        if (startCalcDate == null) continue;
                        var dbChargingInfo = accountLocal.Charges;

                        startRewriteDate = service.CorrectStartRewriteDate(startRewriteDate, startCalcDate.Value, dbChargingInfo);

                        var chargingInfo = service.CalcChargesInfo(accountLocal, dbChargingInfo, startCalcDate.Value, endCalcDate, startRewriteDate, preLaunch);
                        var recalcInsertIntoCharge = new KumiCharge();
                        if (chargingInfo.Any())
                        {
                            recalcInsertIntoCharge = chargingInfo.Last();
                        }
                        
                        service.CalcRecalcInfo(accountLocal, chargingInfo, dbChargingInfo, recalcInsertIntoCharge, startCalcDate.Value, endCalcDate, startRewriteDate);
                        service.UpdateChargesIntoDb(accountLocal, chargingInfo, dbChargingInfo, startCalcDate.Value, endCalcDate, startRewriteDate, lastCalcDate,
                            DateTime.Now.Day >= 25);
                    } catch(Exception e)
                    {
                        var message = e.InnerException != null ? e.InnerException.Message : e.Message;
                        Logger.Error(string.Format("Ошибка во время выставления начислений по лицевому счету {1}: {0}", message, accountLocal.Account));

                        
                        sender.SendEmail("Ошибка во время выставления начислений по лицевому счету " + accountLocal.Account,
                            message, Configuration.SmtpFrom, Configuration.SmtpErrorTo);
                        
                        if (e.GetType() != typeof(ApplicationException))
                        {
                            i++;
                            if (i == 10)
                                return;
                        }
                    }
                }

                if (i == 0)
                {
                    try
                    {
                        var title = string.Format("Массовый расчет платы за найм на {0} завершен", lastCalcDate.ToString("dd.MM.yyyy"));
                        var accountCountFact = db.KumiAccounts.Count(r => (r.IdState == 1 || r.IdState == 3) && r.LastCalcDate == lastCalcDate);
                        var body = title + string.Format(".\r\nОбработано {0} лицевых счетов", accountCountFact);
                        Logger.Log(title);
                        Logger.Log(string.Format("Обработано {0} лицевых счетов", accountCountFact));
                        sender.SendEmail(title, body, Configuration.SmtpFrom, Configuration.SmtpSuccessTo);
                        massChargeInfo.LastCalcDate = lastCalcDate;
                        db.SaveChanges();
                    } catch(Exception e)
                    {
                        Logger.Error(string.Format("Ошибка: {0}", e.Message));
                        sender.SendEmail("Ошибка во время отправки уведомления об успешности расчета платы за найм",
                            e.Message, Configuration.SmtpFrom, Configuration.SmtpErrorTo);
                        return;
                    }
                }
            }
        }
    }
}
