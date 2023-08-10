using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using RegistryDb.Models;
using RegistryDb.Models.IEntityTypeConfiguration.KumiAccounts;
using RegistryServices.DataHelpers;
using RegistryWeb.SecurityServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace RegistryWeb.ReportServices
{
    public class ClaimReportService : ReportService
    {
        private readonly RegistryContext registryContext;
        private readonly SecurityService securityService;

        public ClaimReportService(RegistryContext registryContext, IConfiguration config, 
            IHttpContextAccessor httpContextAccessor, SecurityService securityService) : base(config, httpContextAccessor)
        {
            this.registryContext = registryContext;
            this.securityService = securityService;
        }

        public byte[] TransferToLegal(List<int> idClaims, int idSigner, DateTime dateValue)
        {
            var tmpFileName = Path.GetTempFileName();
            var idClaimsStr = idClaims.Select(id => id.ToString()).Aggregate((x, y) => x + "," + y);
            using (var sw = new StreamWriter(tmpFileName))
                sw.Write(idClaimsStr);
            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", tmpFileName },
                { "request_date_from", dateValue.ToString("dd.MM.yyyy") },
                { "signer", idSigner },
                { "executor", securityService.Executor?.ExecutorLogin?.Split("\\")[1] }
            };
            var fileName = "registry\\claims_correction\\transfer_JD";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] RequestToBks(List<int> idClaims, int idSigner, DateTime dateValue, int idReportBKSType)
        {
            var tmpFileName = Path.GetTempFileName();
            var fileconfigname = "";
            var idClaimsStr = idClaims.Select(id => id.ToString()).Aggregate((x, y) => x + "," + y);
            using (var sw = new StreamWriter(tmpFileName))
                sw.Write(idClaimsStr);

            switch (idReportBKSType)
            {
                case 1:
                    fileconfigname = "request_BKS";
                    break;
                case 2:
                    fileconfigname = "request_BKS_with_period";
                    break;
            }

            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", tmpFileName },
                { "request_date_from", dateValue.ToString("dd.MM.yyyy") },
                { "signer", idSigner },
                { "executor", securityService.Executor?.ExecutorLogin?.Split("\\")[1] }
            };

            var fileName = "registry\\claims_correction\\" + fileconfigname;
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] CourtOrderStatement(int idClaim, int idOrder)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id_claim", idClaim },
                { "id_order", idOrder }
            };
            var fileName = "registry\\claims_correction\\judicial_order";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ExportClaims(List<int> idClaims)
        {
            string columnHeaders;
            string columnPatterns;

            columnHeaders = "[{\"columnHeader\":\"№ п/п\"},{\"columnHeader\":\"№\"},{\"columnHeader\":\"Лицевой счет\"},{\"columnHeader\":\"Адрес\"}," +
                "{\"columnHeader\":\"Наниматель\"},{\"columnHeader\":\"Дата формирования\"},{\"columnHeader\":\"Состояние установлено\"},"+
                "{\"columnHeader\":\"Текущее состояние\"},{\"columnHeader\":\"Период с\"},{\"columnHeader\":\"Период по\"},{\"columnHeader\":\"Номер с/п\"}," +
                "{\"columnHeader\":\"Сумма долга итого\"},{\"columnHeader\":\"Сумма долга найм\"},{\"columnHeader\":\"Сумма долга ДГИ\"},{\"columnHeader\":\"Сумма долга Падун\"}," +
                "{\"columnHeader\":\"Сумма долга ПКК\"},{\"columnHeader\":\"Сумма долга пени\"},{\"columnHeader\":\"Примечание\"}]";
            columnPatterns = "[{\"columnPattern\":\"$num$\"},{\"columnPattern\":\"$column0$\"},{\"columnPattern\":\"$column1$\"},{\"columnPattern\":\"$column2$\"}," +
                "{\"columnPattern\":\"$column3$\"},{\"columnPattern\":\"$column4$\"},{\"columnPattern\":\"$column5$\"},{\"columnPattern\":\"$column6$\"},"+
                "{\"columnPattern\":\"$column7$\"},{\"columnPattern\":\"$column8$\"},{\"columnPattern\":\"$column9$\"},{\"columnPattern\":\"$column10$\"}," +
                "{\"columnPattern\":\"$column11$\"},{\"columnPattern\":\"$column12$\"}," +
                "{\"columnPattern\":\"$column13$\"},{\"columnPattern\":\"$column14$\"},{\"columnPattern\":\"$column15$\"},{\"columnPattern\":\"$description$\"}]";

            var fileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(fileName))
                sw.Write(string.Format("(id_claim IN ({0}))", ClaimsIdsToString(idClaims)));

            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", fileName },
                { "type", "4"},
                { "executor", securityService.User.UserName },
                { "columnHeaders", columnHeaders },
                { "columnPatterns", columnPatterns }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\claims_correction\\export");
            return DownloadFile(fileNameReport);
        }

        private string ClaimsIdsToString(List<int> idClaims)
        {
            return idClaims.Select(r => r.ToString()).Aggregate((v, acc) => v+","+acc);
        }

        public byte[] SplitAccountsReport()
        {
            var arguments = new Dictionary<string, object>();
            var fileName = "registry\\claims_correction\\payment_accounts_duplicate_statistic";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimExecutorReport(DateTime startDate, DateTime endDate, string executor)
        {
            var arguments = new Dictionary<string, object> {
                { "date_from", startDate.ToString("dd.MM.yyyy") },
                { "date_to", endDate.ToString("dd.MM.yyyy") },
                { "implementer", executor }
            };
            var fileName = "registry\\claims_correction\\claim_states";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimStatesReport(DateTime startDate, DateTime endDate, int idStateType, bool isCurrentState)        {
            var arguments = new Dictionary<string, object> {
                { "date_from", startDate.ToString("dd.MM.yyyy") },
                { "date_to", endDate.ToString("dd.MM.yyyy") },
                { "claim_state_type", idStateType },
                { "only_current_claim_state", isCurrentState ? "1" : "0" }
            };
            var fileName = "registry\\claims_correction\\claim_states_executors";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimStatesAllDatesReport(DateTime startDate, DateTime endDate, int idStateType, bool isCurrentState)
        {
            var arguments = new Dictionary<string, object> {
                { "date_from", startDate.ToString("dd.MM.yyyy") },
                { "date_to", endDate.ToString("dd.MM.yyyy") },
                { "claim_state_type", idStateType },
                { "only_current_claim_state", isCurrentState ? "1" : "0" }
            };
            var fileName = "registry\\claims_correction\\claim_states_all_dates";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimCourtReport(DateTime startDate, DateTime endDate)        {
            var arguments = new Dictionary<string, object> {
                { "date_from", startDate.ToString("dd.MM.yyyy") },
                { "date_to", endDate.ToString("dd.MM.yyyy") }
            };
            var fileName = "registry\\claims_correction\\claim_сcourt_orders_prepare";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimEmergencyTariffReport(DateTime startDate, DateTime endDate)
        {
            var arguments = new Dictionary<string, object> {
                { "date_from", startDate.ToString("dd.MM.yyyy") },
                { "date_to", endDate.ToString("dd.MM.yyyy") }
            };
            var fileName = "registry\\claims_correction\\tariffs";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimFactMailingReport(int flag, DateTime startDate, DateTime endDate)
        {
            var resultcodes = new List<int>() { -9, -8, -7, -6, -5, -4, -3, -2, -1, 0 };
            switch (flag)
            {
                case 1:
                    resultcodes = resultcodes.Where(r => r == 0).ToList();
                    break;
                case 2:
                    resultcodes = resultcodes.Where(r => r != 0).ToList();
                    break;
            }

            endDate = endDate != null ? endDate : DateTime.Now;

            var fileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(fileName))
                sw.Write(ResultCodesIdsToString(resultcodes));

            var arguments = new Dictionary<string, object> {
                { "filterTmpFile", fileName },
                { "date_from", startDate.ToString("yyyy-MM-dd") },
                { "date_to", endDate.ToString("yyyy-MM-dd") }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\claims_correction\\claim_fact_mailing");
            return DownloadFile(fileNameReport);
        }

        private string ResultCodesIdsToString(List<int> resultcodes)
        {
            return resultcodes.Aggregate("", (current, id) => current + id.ToString(CultureInfo.InvariantCulture) + ",").TrimEnd(',');
        }

        public byte[] ClaimCourtOspReport(int idClaim, DateTime createDate, int personsCount, int idSigner)
        {
            var fileconfigname = "";

            switch (personsCount)
            {
                case  0:
                    fileconfigname = "statement_in_osp_with_uin";
                    break;
                default:
                    fileconfigname = "statement_in_osp_with_uinAllFamily";
                    break;
            }

            var arguments = new Dictionary<string, object>
            {
                { "id_claim", idClaim },
                { "create_date", createDate },
                { "signer", idSigner }
            };

            var fileName = "registry\\claims\\" + fileconfigname;
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }
        private string getCourt(int? idCourt)
        {
            switch (idCourt)
            {
                case 1:
                    return "Падунское ОСП ГУ ФССП по Иркутской области";
                case 2:
                    return "ОСП по г. Братску и Братскому району ГУ ФССП по Иркутской области";
                default:
                    return "";
            }
        }
        public byte[] ClaimCourtSpiReport(int idClaim, int idCourtType)
        {
            var arguments = new Dictionary<string, object>
            {
                { "id_claim", idClaim },
                { "id_court_type",getCourt(idCourtType)},
                { "executor", securityService.User.UserName.Replace("PWR\\", "") }
            };
            var fileName = "registry\\claims\\statement_in_spi";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimsForDoverie(List<int> idClaims, int statusSending)
        {
            var tmpFileName = Path.GetTempFileName();
            var idClaimsStr = idClaims.Select(id => id.ToString()).Aggregate((x, y) => x + "," + y);
            using (var sw = new StreamWriter(tmpFileName))
                sw.Write(idClaimsStr);
            var arguments = new Dictionary<string, object>
            {
                { "filterTmpFile", tmpFileName },
                { "statusSending", statusSending}
            };
            var fileName = "registry\\claims\\claim_statements_for_doverie_with_status";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimDateReferralCcoBailiffs(DateTime startDate, DateTime endDate)
        {
            var arguments = new Dictionary<string, object> {
                { "date_from", startDate.ToString("dd.MM.yyyy") },
                { "date_to", endDate.ToString("dd.MM.yyyy") }
            };
            var fileName = "registry\\claims_correction\\date_referral_cco_bailiffs";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] ClaimExecutedWork(DateTime startDate, DateTime endDate)
        {
            var arguments = new Dictionary<string, object> {
                { "from_date", startDate.ToString("dd.MM.yyyy") },
                { "to_date", endDate.ToString("dd.MM.yyyy") }
            };
            var fileName = "registry\\claims\\cnt_statistic_claim";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] UkInvoiceAgg(List<int> idsOrganization)
        {
            var arguments = new Dictionary<string, object> {
                { "ids", idsOrganization.Aggregate("", (acc, v) => acc+","+v.ToString() ) }
            };
            var fileName = "registry\\claims\\uk_invoice_agg";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] UkInvoiceDetails(List<int> idsOrganization)
        {
            var arguments = new Dictionary<string, object> {
                { "ids", idsOrganization.Aggregate("", (acc, v) => acc+","+v.ToString() ) }
            };
            var fileName = "registry\\claims\\uk_invoice_details";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] UkInvoiceAggKumi()
        {
            var arguments = new Dictionary<string, object> {
            };
            var fileName = "registry\\kumi_accounts\\uk_invoice_agg_kumi";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] UkInvoiceDetailsKumi()
        {
            var arguments = new Dictionary<string, object> {
            };
            var fileName = "registry\\kumi_accounts\\uk_invoice_details_kumi";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        /*
        public byte[] PaymentsForPeriod(DateTime startDate, DateTime endDate)
        {
            var arguments = new Dictionary<string, object> {
                { "from_date", startDate.ToString("dd.MM.yyyy") },
                { "to_date", endDate.ToString("dd.MM.yyyy") }
            };
            var fileName = "registry\\kumi_accounts\\payments_for_period";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }*/

        public byte[] BalanceForPeriod(DateTime startDate)
        {
            var paymentDate = new DateTime(startDate.Year, startDate.Month, 1);
            paymentDate = paymentDate.AddMonths(1).AddDays(-1);

            var arguments = new Dictionary<string, object> {
                { "for_date", startDate.ToString("dd.MM.yyyy") }
            };
            var fileName = "registry\\kumi_accounts\\balance_for_period";
            var fileNameReport = GenerateReport(arguments, fileName);
            return DownloadFile(fileNameReport);
        }

        public byte[] SberbankFile(DateTime startDate)
        {
            var destFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", Guid.NewGuid().ToString() + ".csv");
            if (!File.Exists(destFile)) File.Create(destFile).Close();

            var rows = registryContext.GetSberbankFileRows(startDate);

            var csv = "";
            foreach(var row in rows)
            {
                csv +=  row.Account + ";" + row.Tenant + ";" + row.Address + ";" + row.Kbk + ";" + row.Okato + ";" + row.Sum + "\r\n";
            }
            File.WriteAllText(destFile, csv);
            
            return DownloadFile(new FileInfo(destFile).Name);
        }

        public byte[] PaymentsForPeriod(DateTime startDate, DateTime endDate)
        {
            var paymentsKbk = registryContext.GetPaymentsForPeriods(startDate, endDate);

            var workbook = new HSSFWorkbook();
            var sheet = workbook.CreateSheet("Платежи КБК");
            sheet.SetColumnWidth(0, 2000);
            sheet.SetColumnWidth(1, 3000);
            sheet.SetColumnWidth(2, 12000);
            sheet.SetColumnWidth(3, 3000);
            sheet.SetColumnWidth(4, 12000);
            sheet.SetColumnWidth(5, 5000);
            sheet.SetColumnWidth(6, 8000);

            var headerStyle = NPOIHelper.GetPaymentsKbkHeaderCellStyle(workbook);

            var head = workbook.CreateCellStyle();
            var headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerFont.Color = HSSFColor.RoyalBlue.Index;
            headerFont.FontName = "Tahoma";
            headerFont.FontHeightInPoints = 20;
            head.SetFont(headerFont);
          

            NPOIHelper.CreateActSpanedCell(sheet, 0, 0, 1, 7,
                string.Format("КУМИ г.Братска"), head);
            NPOIHelper.CreateActSpanedCell(sheet, 1, 0, 1, 7,
                string.Format("Платежи за период с {0} по {1} по КБК найма", 
                startDate.ToString("dd.MM.yyyy"), endDate.ToString("dd.MM.yyyy")), head);

            NPOIHelper.CreateActSpanedCell(sheet, 2, 0, 1, 2, "Документ", headerStyle);
            NPOIHelper.CreateActCell(sheet, 3, 0, "№", headerStyle);
            NPOIHelper.CreateActCell(sheet, 3, 1, "Дата", headerStyle);

            NPOIHelper.CreateActSpanedCell(sheet, 2, 2, 2 , 1, "Наименование", headerStyle);

            NPOIHelper.CreateActSpanedCell(sheet, 2, 3, 1, 2, "Платеж", headerStyle);
            NPOIHelper.CreateActCell(sheet, 3, 3, "Сумма", headerStyle);
            NPOIHelper.CreateActCell(sheet, 3, 4, "Назначение платежа", headerStyle);

            NPOIHelper.CreateActSpanedCell(sheet, 2, 5, 2, 1, "Примечание", headerStyle);
            NPOIHelper.CreateActSpanedCell(sheet, 2, 6, 2, 1, "Распределен на", headerStyle);

            var rowIndex = 4;
            var groupIndex = 0;
            var result = new List<PaymentsForPeriod>();
            decimal totalSum = 0;
            decimal superTotalSum = 0;

            for (var i = 0; i < paymentsKbk.Count; i++)
            {
                if (paymentsKbk[i].group_index != groupIndex)
                {
                    if (groupIndex == 0)
                    {
                        groupIndex = paymentsKbk[i].group_index;
                    }
                    else
                    {
                        groupIndex = paymentsKbk[i].group_index;
                        result.Add(new PaymentsForPeriod
                        {
                            num_d = null,
                            date_d_str = null,
                            payer_name = null,
                            sum = Math.Round((decimal)totalSum * 100) / 100,
                            purpose = null,
                            note = null,
                            account_info = null
                        });
                        totalSum = 0;
                    }
                }
                var sumD =  paymentsKbk[i]?.sum == null ? 0 : paymentsKbk[i].sum;
                var groupRow = new PaymentsForPeriod
                {
                    num_d = paymentsKbk[i].num_d,
                    date_d_str = paymentsKbk[i].date_d_str,
                    payer_name = paymentsKbk[i].payer_name,
                    sum = sumD,
                    purpose = paymentsKbk[i].purpose,
                    note = paymentsKbk[i].note,
                    account_info = paymentsKbk[i].account_info
                };
                result.Add(groupRow);
                totalSum += sumD;
                superTotalSum += sumD;
            }
            if (groupIndex != 0)
            {
                result.Add(new PaymentsForPeriod
                {
                    num_d = null,
                    date_d_str = null,
                    payer_name = null,
                    sum = Math.Round((decimal)totalSum * 100) / 100,
                    purpose = null,
                    note = null,
                    account_info = null
                });
            }

            result.Add(new PaymentsForPeriod{
                num_d = null,
                date_d_str = null,
                payer_name = "Итого",
                sum = Math.Round((decimal)superTotalSum * 100) / 100,
                purpose = null,
                note = null,
                account_info = null
            }
            );

            foreach (var payment in result)
            {
                NPOIHelper.CreateActCell(sheet, rowIndex, 0, payment.num_d?.ToString(),
                                            NPOIHelper.GetPaymentsKbkBaseDataCellStyle(workbook, HorizontalAlignment.Center, VerticalAlignment.Center));
                NPOIHelper.CreateActCell(sheet, rowIndex, 1, payment.date_d_str?.ToString(),
                                            NPOIHelper.GetPaymentsKbkBaseDataCellStyle(workbook, HorizontalAlignment.Center, VerticalAlignment.Center));
               switch(payment.payer_name)
                {
                    case null:
                        NPOIHelper.CreateActCell(sheet, rowIndex, 3, (double)payment.sum,
                                           NPOIHelper.GetPaymentsKbkBaseDataCellStyle(workbook, HorizontalAlignment.Center, VerticalAlignment.Center, false, true));
                        break;
                    case "Итого":
                        NPOIHelper.CreateActCell(sheet, rowIndex, 2, payment.payer_name?.ToString(),
                                            NPOIHelper.GetPaymentsKbkBaseDataCellStyle(workbook, HorizontalAlignment.Left, VerticalAlignment.Center, false, true));
                        NPOIHelper.CreateActCell(sheet, rowIndex, 3, (double)payment.sum,
                                           NPOIHelper.GetPaymentsKbkBaseDataCellStyle(workbook, HorizontalAlignment.Center, VerticalAlignment.Center, false, true));
                        break;
                    default:
                        NPOIHelper.CreateActCell(sheet, rowIndex, 2, payment.payer_name?.ToString(),
                                       NPOIHelper.GetPaymentsKbkBaseDataCellStyle(workbook, HorizontalAlignment.Left, VerticalAlignment.Center));
                        NPOIHelper.CreateActCell(sheet, rowIndex, 3, (double)payment.sum,
                                            NPOIHelper.GetPaymentsKbkBaseDataCellStyle(workbook, HorizontalAlignment.Center, VerticalAlignment.Center));
                        break;
                }
               
                NPOIHelper.CreateActCell(sheet, rowIndex, 4, payment.purpose?.ToString(),
                                            NPOIHelper.GetPaymentsKbkBaseDataCellStyle(workbook, HorizontalAlignment.Left, VerticalAlignment.Center));
                NPOIHelper.CreateActCell(sheet, rowIndex, 5, payment.note?.ToString(),
                                            NPOIHelper.GetPaymentsKbkBaseDataCellStyle(workbook, HorizontalAlignment.Center, VerticalAlignment.Center));
                NPOIHelper.CreateActCell(sheet, rowIndex, 6, payment.account_info?.ToString(), 
                                            NPOIHelper.GetPaymentsKbkBaseDataCellStyle(workbook, HorizontalAlignment.Left, VerticalAlignment.Center));
                rowIndex++;
            }

            using (var exportData = new MemoryStream())
            {
                workbook.Write(exportData);
                return exportData.GetBuffer();
            }
        }
        
    }
}
