using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using RegistryWeb.Models;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistryWeb.ReportServices
{
    public class RegistryObjectsReportService: ReportService
    {
        private readonly RegistryContext rc;
        private readonly SecurityService securityService;
        public RegistryObjectsReportService(RegistryContext rc, IConfiguration config, IHttpContextAccessor httpContextAccessor, SecurityService securityService) : base(config, httpContextAccessor)
        {
            this.rc = rc;
            this.securityService = securityService;
        }

        public RegistryObjectReportsVM GetViewModel()
        {
            var viewModel = new RegistryObjectReportsVM
            {
                KladrRegionsList = new SelectList(rc.KladrRegions, "id_region", "region")
            };
            return viewModel;
        }

        public byte[] GetJFReport(string JFType, string regions, out string NameReport)
        {
            var arguments = new Dictionary<string, object>
            {
                { "regions", regions },
                { "executor", securityService.User.UserName }
            };
            var fileNameReport = "";
            NameReport = null;
            switch (JFType)
            {
                case "0":
                    fileNameReport = GenerateReport(arguments, "registry\\registry\\ownerships_report");
                    //fileNameReport = GenerateReport(arguments, @"D:\Projects\Всячина проектов\RegistryWeb\Отчёты\ownerships_report");
                    NameReport = "Аварийное и снесеное жилье";
                    break;
                case "1":
                    fileNameReport = GenerateReport(arguments, "registry\\registry\\commercial_fund");
                    //fileNameReport = GenerateReport(arguments, @"D:\Projects\Всячина проектов\RegistryWeb\Отчёты\commercial_fund");
                    NameReport = "Коммерческий жилой фонд";
                    break;
                case "2":
                    fileNameReport = GenerateReport(arguments, "registry\\registry\\special_fund");
                    //fileNameReport = GenerateReport(arguments, @"D:\Projects\Всячина проектов\RegistryWeb\Отчёты\special_fund");
                    NameReport = "Специализированный жилой фонд";
                    break;
                case "3":
                    fileNameReport = GenerateReport(arguments, "registry\\registry\\social_fund");
                    //fileNameReport = GenerateReport(arguments, @"D:\Projects\Всячина проектов\RegistryWeb\Отчёты\social_fund");
                    NameReport = "Социальный жилой фонд";
                    break;
                case "4":
                    fileNameReport = GenerateReport(arguments, "registry\\registry\\municipal_premises_current_fund");
                    //fileNameReport = GenerateReport(arguments, @"D:\Projects\Всячина проектов\RegistryWeb\Отчёты\municipal_premises_current_fund");
                    NameReport = "Текущий фонд муниципальных жилых помещений";
                    break;
            }

            return DownloadFile(fileNameReport);
        }
        
        public byte[] GetStatisticBuildingReport(string TypeReport)
        {
            var arguments = new Dictionary<string, object>
            {
                { "reportType", TypeReport },
                { "executor", securityService.User.UserName }
            };

            var fileNameReport = GenerateReport(arguments, "registry\\registry\\municipal_buildings");
            //var fileNameReport = GenerateReport(arguments, @"D:\Projects\Всячина проектов\RegistryWeb\Отчёты\municipal_buildings");
            return DownloadFile(fileNameReport);
        }
        
        public byte[] GetStatisticPremiseReport(string TypeReport)
        {
            var arguments = new Dictionary<string, object>
            {
                { "reportType", TypeReport },
                { "executor", securityService.User.UserName }
            };

            var fileNameReport = GenerateReport(arguments, "registry\\registry\\municipal_premises");
            //var fileNameReport = GenerateReport(arguments, @"D:\Projects\Всячина проектов\RegistryWeb\Отчёты\municipal_premises");
            return DownloadFile(fileNameReport);
        }        
    }
}
