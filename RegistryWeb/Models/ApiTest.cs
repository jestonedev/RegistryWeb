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
            hpar.FullAddress = GetFullAddress();
            hpar.Stage = HouseStageEnum.Exploited;
            hpar.Inn = null;
            hpar.LastUpdate = DateTime.Parse("2015-05-22T20:42:02+03:00");
            hpar.HouseProfileData = GetHouseProfileData988();
            hpar.FilesInfo = null; // GetFileInfo();
            return hpar;
        }

        public static HouseProfileData988 GetHouseProfileData988()
        {
            var hpd = new HouseProfileData988();
            hpd.Report = GetHouseReport();
            hpd.Roofs = GetHouseRoofs();
            return hpd;
        }

        public static List<HouseRoof> GetHouseRoofs()
        {            
            var e1 = new HouseRoof();
            e1.Id = 0;
            e1.RoofingType = HouseRoofingTypeEnum.MetalTile;
            e1.RoofType = HouseRoofTypeEnum.Flat;
            var e2 = new HouseRoof();
            e2.Id = 1;
            e2.RoofingType = HouseRoofingTypeEnum.ProfiledFlooring;
            e2.RoofType = HouseRoofTypeEnum.Pitched;
            var list = new List<HouseRoof>();
            list.Add(e1);
            list.Add(e2);
            return list;
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

        public static FullAddress GetFullAddress()
        {
            var ad = new FullAddress();
            ad.RegionGuid = "6466c988-7ce3-45e5-8b97-90ae16cb1249";
            ad.RegionFormalName = "Иркутская";
            ad.RegionShortName = "обл";
            ad.RegionCode = "3800000000000";
            ad.AreaGuid = "";
            ad.AreaFormalName = "";
            ad.AreaShortName = "";
            ad.AreaCode = "";
            ad.City1Guid = "bdf6b629-b33e-4cfa-b4b2-7f693e1d821c";
            ad.City1FormalName = "Братск";
            ad.City1ShortName = "г";
            ad.City1Code = "3800000500000";
            ad.City2Guid = "0620d65c-c7d5-44e2-965d-00a11a28c7c2";
            ad.City2FormalName = "Центральный";
            ad.City2ShortName = "жилрайон";
            ad.City2Code = "3800000504100";
            ad.City3Guid = null;
            ad.City3FormalName = null;
            ad.City3ShortName = null;
            ad.City3Code = null;
            ad.StreetGuid = "f90b0f50-2b1e-4571-8f19-4f486422d24c";
            ad.StreetFormalName = "Баркова";
            ad.StreetShortName = "ул";
            ad.StreetCode = "38000005041000500";
            ad.AdditionalTerritory = null;
            ad.AdditionalTerritoryFormalName = null;
            ad.AdditionalTerritoryShortName = null;
            ad.AdditionalTerritoryCode = null;
            ad.Houseguid = "8044c3ac-9d1f-4c7f-94f9-4ab2fae2e2a5";
            ad.HouseNumber = "35";
            ad.Building = "";
            ad.Block = "";
            ad.Letter = "";
            ad.Structure = "";            
            return ad;
        }

        public static HouseReport GetHouseReport()
        {
            var report = new HouseReport();
            report.Common = GetHouseReportCommon();
            report.CommunalService = null;
            report.ClaimsToConsumers = null;
            report.HouseReportQualityOfWorkClaims = null;
            return report;
        }

        public static HouseReportCommon GetHouseReportCommon()
        {
            var hrc = new HouseReportCommon();
            return hrc;
        }

    }
}
