using RegistryWeb.Models.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models
{
    public static class ApiTest
    {

        public static ReportingPeriod GetReportingPeriod(int id, DateTime dateStart, DateTime dateEnd, string name, ReportingPeriodStateEnum state, bool is988)
        {
            var period = new ReportingPeriod();
            period.Id = id;
            period.DateStart = dateStart;
            period.DateEnd = dateEnd;
            period.Name = name;
            period.State = state;
            period.Is988 = is988;
            return period;
        }
        public static PeriodListResult GetPeriodListResult()
        {
            var reportingPeriods = new PeriodListResult();

            var r1 = new ReportingPeriod();
            r1.Id = 3;
            r1.DateStart = new DateTime(2012, 1, 1);
            r1.DateEnd = new DateTime(2012, 12, 31);
            r1.Name = "2012 год";
            r1.State = ReportingPeriodStateEnum.Archive;
            r1.Is988 = false;
            reportingPeriods.GetReportingPeriodListResult.Add(r1);

            var r2 = GetReportingPeriod(465, new DateTime(2019, 1, 1), new DateTime(2019, 12, 31), "2019 год", ReportingPeriodStateEnum.Current, true);
            reportingPeriods.GetReportingPeriodListResult.Add(r2);

            return reportingPeriods;
        }
        public static HouseProfileActualResult GetHouseProfileActualResult()
        {
            var hpar = new HouseProfileActualResult();
            hpar.HouseId = 7947873;
            hpar.FullAddress = null;
            hpar.Stage = HouseStageEnum.Exploited;
            hpar.Inn = null;
            hpar.LastUpdate = DateTime.Parse("2015-05-22T20:42:02+03:00");
            hpar.HouseProfileData = null;
            hpar.FilesInfo = GetFileInfo();
            return hpar;
        }

        public static FileInfo GetFileInfo()
        {
            var fileInfo = new FileInfo();
            fileInfo.FileId = 1;
            fileInfo.Name = "asdf";
            fileInfo.Extension = "pdf";
            fileInfo.Size = 1234523;
            fileInfo.CreateDate = new DateTime(2019, 12, 27);
            return fileInfo;
        }
    }
}
