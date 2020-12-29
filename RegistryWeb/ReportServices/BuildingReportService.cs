using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using RegistryWeb.SecurityServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ReportServices
{
    public class BuildingReportService : ReportService
    {
        private readonly SecurityService securityService;

        public BuildingReportService(IConfiguration config, IHttpContextAccessor httpContextAccessor, SecurityService securityService) : base(config, httpContextAccessor)
        {
            this.securityService = securityService;
        }

        public byte[] ExcerptBuilding(int idBuilding, string excerptNumber, DateTime excerptDateFrom, int signer)
        {
            var arguments = new Dictionary<string, object>
            {
                { "ids", idBuilding },
                { "executor", securityService.User.UserName.Replace("PWR\\", "") },
                { "excerpt_number", excerptNumber },
                { "excerpt_date_from", excerptDateFrom.ToString("dd.MM.yyyy") },
                { "signer", signer }
            };
            var fileNameReport = GenerateReport(arguments, "registry\\registry\\excerpt_build");
            return DownloadFile(fileNameReport);
        }
    }
}
