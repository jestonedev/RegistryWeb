using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistryWeb.ReportServices
{
    public class OwnerReportService: ReportService
    {
        public OwnerReportService(IConfiguration config, IHttpContextAccessor httpContextAccessor): base(config, httpContextAccessor)
        {
        }

        public byte[] Forma1(List<int> ids)
        {
            var fileNameReport = GenerateMultiReport(ids, "registry_web\\owners\\forma1");
            return DownloadFile(fileNameReport);
        }

        public string Forma2Ajax(List<int> ids)
        {
            return GenerateMultiReport(ids, "registry_web\\owners\\forma2");
        }

        public byte[] Forma2(List<int> ids)
        {
            var fileNameReport = GenerateMultiReport(ids, "registry_web\\owners\\forma2");
            return DownloadFile(fileNameReport);
        }

        public string Forma3Ajax(List<int> ids)
        {
            return GenerateMultiReport(ids, "registry_web\\owners\\forma3");
        }

        public byte[] Forma3(List<int> ids)
        {
            var fileNameReport = GenerateMultiReport(ids, "registry_web\\forma3");
            return DownloadFile(fileNameReport);
        }

        private string GenerateMultiReport(List<int> ids, string name)
        {
            var arguments = new Dictionary<string, object>();
            var idsTmpFile = WriteTmpIdsFile(ids);
            arguments.Add("idsTmpFile", idsTmpFile);
            var reportFile = GenerateReport(arguments, name);
            if (File.Exists(idsTmpFile))
                File.Delete(idsTmpFile);
            return reportFile;
        }

        private string WriteTmpIdsFile(List<int> ids)
        {
            var fileName = Path.GetTempFileName();
            using (var sw = new StreamWriter(fileName))
                sw.Write(ids.Select(id => id.ToString()).Aggregate((x, y) => x + "," + y));
            return fileName;
        }

        public byte[] AzfAreaAnalize()
        {
            var reportFile = GenerateReport(new Dictionary<string, object>(), "registry_web\\owners\\azf_area_analize");
            return DownloadFile(reportFile);
        }

        public byte[] AzfRoomsAnalize()
        {
            var reportFile = GenerateReport(new Dictionary<string, object>(), "registry_web\\owners\\azf_rooms_analize");
            return DownloadFile(reportFile);
        }

        public byte[] AzfRegionsAnalize()
        {
            var reportFile = GenerateReport(new Dictionary<string, object>(), "registry_web\\owners\\azf_regions_analize");
            return DownloadFile(reportFile);
        }

        public byte[] AzfWithoutPrivAnalize()
        {
            var reportFile = GenerateReport(new Dictionary<string, object>(), "registry_web\\owners\\azf_without_priv_analize");
            return DownloadFile(reportFile);
        }
    }
}
