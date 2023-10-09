using InvoiceGenerator;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.Entities.Tenancies;
using RegistryServices.DataHelpers;
using RegistryServices.ViewModel.KumiAccounts;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;


namespace RegistryWeb.ReportServices
{
    public class KumiAccountReportService : ReportService
    {
        private readonly SecurityService securityService;

        public KumiAccountReportService(IConfiguration config, IHttpContextAccessor httpContextAccessor, SecurityService securityService) : base(config, httpContextAccessor)
        {
            this.securityService = securityService;
        }

        public byte[] CalDept(KumiCharge lastPayment, Dictionary<string, object> personInfo, DateTime dateFrom, DateTime dateTo, int fileFormat)
        {
            var arguments = new Dictionary<string, object>
            {
                { "date_from", dateFrom.ToString("yyyy.MM.dd")},
                { "date_to", dateTo.ToString("yyyy.MM.dd")},
                { "id_account", lastPayment.Account.IdAccount },
                { "account", lastPayment.Account.Account },
                { "tenant", personInfo.Where(c => c.Key.Contains("tenant")).Select(c => c.Value).FirstOrDefault().ToString()},
                { "raw_address",personInfo.Where(c => c.Key.Contains("address")).Select(c => c.Value).FirstOrDefault().ToString()},
                { "prescribed", (int)personInfo.Where(c => c.Key.Contains("prescribed")).Select(c => c.Value).FirstOrDefault() },
                { "total_area", personInfo.Where(c => c.Key.Contains("totalArea")).Select(c => c.Value).FirstOrDefault().ToString().Replace(',', '.') },
                { "templateFileName", activityManagerPath + "templates\\registry\\kumi_accounts\\amount_debt_KUMI." + (fileFormat == 1 ? "xlsx" : "ods") },
            };
            var fileName = "registry\\kumi_accounts\\amount_debt_KUMI";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public Dictionary<Dictionary<string, object>, int> GenerateInvoices(List<InvoiceGeneratorParam> invoices, string action, string destDirectory = null)
        {
            var parametrs = new List<Dictionary<string, object>>();
            if (action == "Export")
            {
                if (Directory.Exists(destDirectory)) Directory.Delete(destDirectory);
                Directory.CreateDirectory(destDirectory);
            }
            foreach (var invoice in invoices)
            {
                var arguments = new Dictionary<string, object>
                {
                    { "id_account", invoice.IdAccount },
                    { "--address", invoice.Address },
                    { "--account", invoice.Account },
                    { "--account-gis-zkh", invoice.AccountGisZkh },
                    { "--tenant", invoice.Tenant },
                    { "--on-date", invoice.OnDate.ToString("dd.MM.yyyy")},
                    { "--balance-input", invoice.BalanceInput },
                    { "--charging-tenancy", invoice.ChargingTenancy },
                    { "--charging-penalty", invoice.ChargingPenalty },
                    { "--payed", invoice.Payed },
                    { "--recalc-tenancy", invoice.RecalcTenancy },
                    { "--recalc-penalty", invoice.RecalcPenalty },
                    { "--balance-output", invoice.BalanceOutput },
                    { "--total-area", invoice.TotalArea },
                    { "--tariff", invoice.Tariff },
                    { "--prescribed", invoice.Prescribed },
                    { "--message", invoice.TextMessage },
                };
                if (action == "Send")
                    arguments.Add("--email", invoice.Emails.Aggregate((x, y) => x + "," + y));
                else
                    arguments.Add("--move-to-filename", Path.Combine(destDirectory, string.Format("Счет-извещение по ЛС № {0}.pdf", invoice.Account)));
                parametrs.Add(arguments);
            }
            return GenerateInvoices(parametrs);
        }

        public byte[] ExportAccounts(List<int> idAccounts)
        {
            string columnHeaders;
            string columnPatterns;

            columnHeaders = "[{\"columnHeader\":\"Состояние на дату\"},{\"columnHeader\":\"Дата последнего начисления\"}," +
                "{\"columnHeader\":\"Адрес\"}," +
                "{\"columnHeader\":\"Лицевой счет\"},{\"columnHeader\":\"Наниматель\"}," +
                "{\"columnHeader\":\"Текущее состояние посл. иск. работы\"},{\"columnHeader\":\"Период посл. иск. работы с\"},{\"columnHeader\":\"Период посл. иск. работы по\"}," +
                "{\"columnHeader\":\"Общая площадь\"},{\"columnHeader\":\"Прописано\"}," +
                "{\"columnHeader\":\"Сальдо вх.\"},{\"columnHeader\":\"Сальдо вх. найм\"},{\"columnHeader\":\"Пени (вх.)\"}," +
                "{\"columnHeader\":\"Начисление итого\"},{\"columnHeader\":\"Начисление найм\"},{\"columnHeader\":\"Начисление пени\"}," +
                "{\"columnHeader\":\"Перерасчет найм\"},{\"columnHeader\":\"Перерасчет пени\"}," +
                "{\"columnHeader\":\"Оплата найм\"},{\"columnHeader\":\"Оплата пени\"},{\"columnHeader\":\"Сальдо исх.\"}," +
                "{\"columnHeader\":\"Сальдо исх. найм\"},{\"columnHeader\":\"Пени исх.\"}]";
            columnPatterns = "[{\"columnPattern\":\"$column0$\"},{\"columnPattern\":\"$column1$\"},{\"columnPattern\":\"$column2$\"}," +
                "{\"columnPattern\":\"$column3$\"},{\"columnPattern\":\"$column4$\"},{\"columnPattern\":\"$column5$\"},{\"columnPattern\":\"$column6$\"}," +
                "{\"columnPattern\":\"$column7$\"},{\"columnPattern\":\"$column8$\"},{\"columnPattern\":\"$column9$\"},{\"columnPattern\":\"$column10$\"}," +
                "{\"columnPattern\":\"$column11$\"},{\"columnPattern\":\"$column12$\"},{\"columnPattern\":\"$column13$\"},{\"columnPattern\":\"$column14$\"}," +
                "{\"columnPattern\":\"$column15$\"},{\"columnPattern\":\"$column16$\"},{\"columnPattern\":\"$column17$\"},{\"columnPattern\":\"$column18$\"}," +
                "{\"columnPattern\":\"$column19$\"},{\"columnPattern\":\"$column20$\"},{\"columnPattern\":\"$column21$\"},{\"columnPattern\":\"$column22$\"}]";

            var fileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(fileName))
                sw.Write(string.Format("(id_account IN ({0}))", idAccounts.Select(r => r.ToString()).Aggregate((v, acc) => v + "," + acc)));

            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", fileName },
                { "type", "6"},
                { "executor", securityService.User.UserName.Replace("PWR\\", "") },
                { "columnHeaders", columnHeaders },
                { "columnPatterns", columnPatterns },
                { "orderColumn", "id_account" }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\export");
            return DownloadFile(fileNameReport);
        }

        public byte[] KumiAccountAct(KumiAccount account, List<KumiActChargeVM> actChargeVMs, DateTime atDate)
        {
            var workbook = new HSSFWorkbook();
            var sheet = workbook.CreateSheet("Акт");
            sheet.SetColumnWidth(0, 3575);
            sheet.SetColumnWidth(1, 4150);
            sheet.SetColumnWidth(2, 3575);
            sheet.SetColumnWidth(3, 2700);
            sheet.SetColumnWidth(4, 2700);
            sheet.SetColumnWidth(5, 1620);
            sheet.SetColumnWidth(6, 2100);
            sheet.SetColumnWidth(7, 2100);
            sheet.SetColumnWidth(8, 9850);
            sheet.SetColumnWidth(9, 4150);

            var headerStyle = NPOIHelper.GetActHeaderCellStyle(workbook);
            
            NPOIHelper.CreateActSpanedCell(sheet, 0, 0, 1, 10, 
                string.Format("Акт по лицевому счету № {0} на {1}", account.Account, atDate.ToString("dd.MM.yyyy")), headerStyle);
            
            NPOIHelper.CreateActSpanedCell(sheet, 1, 0, 2, 1, "Месяц", headerStyle);
            NPOIHelper.CreateActSpanedCell(sheet, 1, 1, 2, 1, "Начислено", headerStyle);
            NPOIHelper.CreateActSpanedCell(sheet, 1, 2, 2, 1, "Долг", headerStyle);
            NPOIHelper.CreateActSpanedCell(sheet, 1, 3, 1, 3, "Период просрочки", headerStyle);
            NPOIHelper.CreateActSpanedCell(sheet, 1, 6, 2, 1, "Ставка", headerStyle);
            NPOIHelper.CreateActSpanedCell(sheet, 1, 7, 2, 1, "Доля ставки", headerStyle);
            NPOIHelper.CreateActSpanedCell(sheet, 1, 8, 2, 1, "Формула", headerStyle);
            NPOIHelper.CreateActSpanedCell(sheet, 1, 9, 2, 1, "Пени", headerStyle);
            
            NPOIHelper.CreateActCell(sheet, 2, 3, "с", headerStyle);
            NPOIHelper.CreateActCell(sheet, 2, 4, "по", headerStyle);
            NPOIHelper.CreateActCell(sheet, 2, 5, "дней", headerStyle);
            
            var rowIndex = 3;
            var penaltyAcc = 0.0m;
            var totalPenalty = 0.0m;

            foreach (var charge in actChargeVMs.OrderBy(r => r.Date))
            {
                var monthStr = charge.Date.ToString("MMM.yyyy");
                var chargeStr = charge.Value.ToString(CultureInfo.GetCultureInfo("ru-RU"))+" руб.";
                if (!charge.Events.Any())
                {
                    NPOIHelper.CreateActCell(sheet, rowIndex, 0, monthStr, NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Center));
                    NPOIHelper.CreateActCell(sheet, rowIndex, 1, chargeStr, NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
                    NPOIHelper.CreateActSpanedCell(sheet, rowIndex, 2, 1, 8, "Периоды просрочки, платежи и исковые работы отсутствуют", 
                        NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Center, true));
                    rowIndex++;
                } else
                {
                    var sortedEvents = charge.Events.OrderBy(r => r.Date).ToList();
                    var hasDeptPeriods = true;

                    if (!sortedEvents.Where(r => r is KumiActPeniCalcEventVM).Any())
                    {
                        hasDeptPeriods = false;
                    }

                    for(var i = 0; i < sortedEvents.Count; i++)
                    {
                        if (i == 0)
                        {
                            NPOIHelper.CreateActSpanedCell(sheet, rowIndex, 0, sortedEvents.Count + (hasDeptPeriods ? 0 : 1), 1,  monthStr, 
                                NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Center));
                            NPOIHelper.CreateActSpanedCell(sheet, rowIndex, 1, sortedEvents.Count + (hasDeptPeriods ? 0 : 1), 1, 
                                sortedEvents[i].Date == DateTime.MinValue ? (charge.Value - sortedEvents[i].Tenancy).ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб." : chargeStr, 
                                NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
                        }

                        var e = sortedEvents[i];
                        switch(e)
                        {
                            case KumiActPeniCalcEventVM peni:
                                var peniTaxStr = peni.Tenancy.ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб.";
                                var days = (peni.EndDate - peni.StartDate).TotalDays + 1;
                                var keyRateCoef = peni.KeyRateCoef == 1 / 300m ? "1/300" : peni.KeyRateCoef == 1 / 130m ? "1/130" : "0";
                                var penaltyRound = Math.Round(peni.Penalty + penaltyAcc, 2);
                                totalPenalty += penaltyRound;
                                penaltyAcc += peni.Penalty - penaltyRound;
                                NPOIHelper.CreateActCell(sheet, rowIndex, 2, peniTaxStr, NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
                                if (peni.Date != DateTime.MinValue)
                                {
                                    if (charge.IsBksCharge)
                                    {
                                        NPOIHelper.CreateActSpanedCell(sheet, rowIndex, 3, 1, 6, "Расчет БКС",
                                         NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Center, true));
                                    }
                                    else
                                    {
                                        NPOIHelper.CreateActCell(sheet, rowIndex, 3, peni.StartDate.ToString("dd.MM.yyyy"),
                                            NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Center));
                                        NPOIHelper.CreateActCell(sheet, rowIndex, 4, peni.EndDate.ToString("dd.MM.yyyy"),
                                            NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Center));
                                        NPOIHelper.CreateActCell(sheet, rowIndex, 5, days,
                                            NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Center), CellType.Numeric);
                                        NPOIHelper.CreateActCell(sheet, rowIndex, 6, (double)peni.KeyRate,
                                            NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right), CellType.Numeric);
                                        NPOIHelper.CreateActCell(sheet, rowIndex, 7, keyRateCoef,
                                            NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
                                        NPOIHelper.CreateActCell(sheet, rowIndex, 8,
                                            string.Format("{0} x {1} x {2} x {3} %", peniTaxStr, days, keyRateCoef, peni.KeyRate.ToString(CultureInfo.GetCultureInfo("ru-RU"))),
                                            NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
                                    }
                                } else
                                {
                                    NPOIHelper.CreateActSpanedCell(sheet, rowIndex, 3, 1, 6, "Перенесенный долг с другого ЛС",
                                     NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Center, true));
                                }
                                NPOIHelper.CreateActCell(sheet, rowIndex, 9, penaltyRound.ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб.",
                                    NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
                                break;
                            case KumiActPaymentEventVM payment:
                                var paymentDiff = -Math.Min(payment.TenancyTail, charge.Value);
                                var paymentRequsits = "Платеж: ";
                                if (!string.IsNullOrEmpty(payment.NumDocument))
                                {
                                    paymentRequsits += "№ ПД " + payment.NumDocument + " ";
                                }
                                else
                                if (payment.IdPayment <= Int32.MaxValue - 100000)  // Зарезервированные идентификаторы для платежей БКС
                                {
                                    paymentRequsits += "Id " + payment.IdPayment + " ";
                                }
                                paymentRequsits += " на сумму найм " + payment.Tenancy.ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб. и пени " +
                                    payment.Penalty.ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб.";
                                if (payment.Tenancy > payment.TenancyTail || payment.Penalty > payment.PenaltyTail)
                                {
                                    paymentRequsits += ", ранее неучтеный остаток найм " + payment.TenancyTail.ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб.";
                                }
                                NPOIHelper.CreateActCell(sheet, rowIndex, 2, paymentDiff.ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб.",
                                    NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
                                NPOIHelper.CreateActCell(sheet, rowIndex, 3, payment.Date.ToString("dd.MM.yyyy"),
                                    NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Center));
                                NPOIHelper.CreateActSpanedCell(sheet, rowIndex, 4, 1, 5, paymentRequsits,
                                    NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Left));
                                var paymentPenatlyTail = "-";
                                if (payment.PenaltyTail > 0)
                                {
                                    paymentPenatlyTail = "-" + payment.PenaltyTail.ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб.";
                                }
                                NPOIHelper.CreateActCell(sheet, rowIndex, 9, paymentPenatlyTail,
                                    NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
                                break;
                            case KumiActClaimEventVM claim:
                                var claimDiff = -Math.Min(claim.TenancyTail, charge.Value);
                                var claimRequsits = "ПИР: период взыскания ";
                                if (claim.StartDeptPeriod != null)
                                {
                                    claimRequsits += "с " + claim.StartDeptPeriod.Value.ToString("dd.MM.yyyy") + " ";
                                }
                                claimRequsits += "по " + claim.EndDeptPeriod.Value.ToString("dd.MM.yyyy") + ", ";
                                claimRequsits += "предъявленный долг найм " + claim.Tenancy.ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб. и пени " +
                                    claim.Penalty.ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб.";

                                if (claim.Tenancy > claim.TenancyTail || claim.Penalty > claim.PenaltyTail)
                                {
                                    claimRequsits += ", ранее неучтеный остаток найм " + claim.TenancyTail.ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб."; ;
                                }
                                NPOIHelper.CreateActCell(sheet, rowIndex, 2, claimDiff.ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб.",
                                    NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
                                NPOIHelper.CreateActCell(sheet, rowIndex, 3, claim.Date.ToString("dd.MM.yyyy"),
                                    NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Center));
                                NPOIHelper.CreateActSpanedCell(sheet, rowIndex, 4, 1, 5, claimRequsits,
                                    NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Left));
                                var claimPenatlyTail = "-";
                                if (claim.PenaltyTail > 0)
                                {
                                    claimPenatlyTail = "-" + claim.PenaltyTail.ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб.";
                                }
                                NPOIHelper.CreateActCell(sheet, rowIndex, 9, claimPenatlyTail,
                                    NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
                                break;
                        }
                        rowIndex++;
                    }
                    if (!hasDeptPeriods)
                    {
                        NPOIHelper.CreateActSpanedCell(sheet, rowIndex, 2, 1, 8, "Периоды просрочки отсутствуют",
                                    NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Center, true));
                        rowIndex++;
                    }
                }
            }

            NPOIHelper.CreateActSpanedCell(sheet, rowIndex, 0, 4, 1, "Итого",
                                NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Left, false, true));
            NPOIHelper.CreateActSpanedCell(sheet, rowIndex, 1, 1, 8, "Сумма основного долга начислено",
                                NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
            NPOIHelper.CreateActCell(sheet, rowIndex, 9, actChargeVMs.Select(r => r.Value).Sum().ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб.",
                                NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
            rowIndex++;
            NPOIHelper.CreateActSpanedCell(sheet, rowIndex, 1, 1, 8, "Сумма основного долга оплачено",
                                NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
            var claimsTenancySum = actChargeVMs.SelectMany(r => r.Events).Where(r => r is KumiActClaimEventVM)
                        .Select(r => (KumiActClaimEventVM)r).GroupBy(r => r.IdClaim).Select(r => r.FirstOrDefault()?.Tenancy ?? 0.0m).Sum();
            var paymentsTenancySum = actChargeVMs.SelectMany(r => r.Events).Where(r => r is KumiActPaymentEventVM)
                .Select(r => (KumiActPaymentEventVM)r).GroupBy(r => r.IdPayment).Select(r => r.FirstOrDefault()?.Tenancy ?? 0.0m).Sum();
            NPOIHelper.CreateActCell(sheet, rowIndex, 9, (claimsTenancySum + paymentsTenancySum).ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб.",
                                NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
            rowIndex++;
            NPOIHelper.CreateActSpanedCell(sheet, rowIndex, 1, 1, 8, "Пени начислено",
                NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
            NPOIHelper.CreateActCell(sheet, rowIndex, 9, totalPenalty.ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб.",
                                NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
            rowIndex++;

            NPOIHelper.CreateActSpanedCell(sheet, rowIndex, 1, 1, 8, "Пени оплачено",
                NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));
            var totalPenaltyPayed = actChargeVMs.SelectMany(r => r.Events).Where(r => r is IChargePaymentEventVM)
                        .Select(r => (IChargePaymentEventVM)r).Select(r => r.PenaltyTail).Sum();
            NPOIHelper.CreateActCell(sheet, rowIndex, 9, totalPenaltyPayed.ToString(CultureInfo.GetCultureInfo("ru-RU")) + " руб.",
                                NPOIHelper.GetActBaseDataCellStyle(workbook, HorizontalAlignment.Right));

            using (var exportData = new MemoryStream())
            {
                workbook.Write(exportData);
                return exportData.GetBuffer();
            }
        }
    }
}
