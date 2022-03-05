using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RegistryDb.Interfaces;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;
using RegistryServices.ViewModel.RegistryObjects;
using RegistryWeb.Enums;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace RegistryWeb.DataServices
{
    public class ReestrEmergencyPremisesDataService : ListDataService<ReestrEmergencyPremisesVM, ReestrEmergencyPremisesFilter>
    {
        private readonly string sqlDriver;
        private readonly string connString;
        private readonly string activityManagerPath;

        public ReestrEmergencyPremisesDataService(RegistryContext registryContext, AddressesDataService addressesDataService,
            IConfiguration config, IHttpContextAccessor httpContextAccessor) : base(registryContext, addressesDataService)
        {
            sqlDriver = config.GetValue<string>("SqlDriver");
            connString = httpContextAccessor.HttpContext.User.FindFirst("connString").Value;
            activityManagerPath = config.GetValue<string>("ActivityManagerPath");
        }

        public override ReestrEmergencyPremisesVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, ReestrEmergencyPremisesFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            return viewModel;
        }

        public ReestrEmergencyPremisesVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            ReestrEmergencyPremisesFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var mkd = GetEmergencyMKD();
            var tenancyProcessesReestr = GetTenancyProcessesReestr(mkd);
            var onwerProcessesReestr = GetOwnerProcessesReestr(mkd);
            var reestr = tenancyProcessesReestr.Union(onwerProcessesReestr);
            viewModel.PageOptions.TotalRows = reestr.Count();
            reestr = ReestrFilter(reestr, viewModel.FilterOptions);
            var count = reestr.Count();
            viewModel.PageOptions.Rows = count;                
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.Reestr = GetReestrPage(reestr, viewModel.PageOptions).ToList();
            return viewModel;
        }

        private IEnumerable<ProcessOwnership> ReestrFilter(IEnumerable<ProcessOwnership> reestr, ReestrEmergencyPremisesFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                reestr = ProcessOwnershipTypeFilter(reestr, filterOptions);
                reestr = PersonsFilter(reestr, filterOptions);
                reestr = AddressFilter2(reestr, filterOptions);
            }
            return reestr;
        }

        private IEnumerable<ProcessOwnership> ProcessOwnershipTypeFilter(IEnumerable<ProcessOwnership> reestr, ReestrEmergencyPremisesFilter filterOptions)
        {
            if (filterOptions.ProcessOwnershipType == ProcessOwnershipTypeEnum.All)
                return reestr;
            return reestr.Where(r => r.Type == filterOptions.ProcessOwnershipType);
        }

        private IEnumerable<ProcessOwnership> PersonsFilter(IEnumerable<ProcessOwnership> reestr, ReestrEmergencyPremisesFilter filterOptions)
        {
            if (string.IsNullOrWhiteSpace(filterOptions.Persons))
                return reestr;
            return reestr.Where(p => p.Persons.ToLower().Contains(filterOptions.Persons.Trim().ToLower()));
        }

        private IEnumerable<ProcessOwnership> AddressFilter2(IEnumerable<ProcessOwnership> reestr, ReestrEmergencyPremisesFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return reestr;
            int id = 0;
            int.TryParse(filterOptions.Address.Id, out id);
            var tenancyProcess = (
                from tp in registryContext.TenancyProcesses
                join tba in registryContext.TenancyBuildingsAssoc
                    on tp.IdProcess equals tba.IdProcess into TbaL
                from subTbaL in TbaL.DefaultIfEmpty()
                join tpa in registryContext.TenancyPremisesAssoc
                    on tp.IdProcess equals tpa.IdProcess into TpaL
                from subTpaL in TpaL.DefaultIfEmpty()
                join tspa in registryContext.TenancySubPremisesAssoc
                    on tp.IdProcess equals tspa.IdProcess into TspaL
                from subTspaL in TspaL.DefaultIfEmpty()
                where
                    subTbaL != null && subTbaL.BuildingNavigation.IdStreet.Equals(filterOptions.Address.Id) ||
                    subTpaL != null && subTpaL.PremiseNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id) ||
                    subTspaL != null && subTspaL.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id) ||
                    subTbaL != null && subTbaL.BuildingNavigation.IdBuilding == id ||
                    subTpaL != null && subTpaL.PremiseNavigation.IdBuildingNavigation.IdBuilding == id ||
                    subTspaL != null && subTspaL.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdBuilding == id ||
                    subTpaL != null && subTpaL.PremiseNavigation.IdPremises == id ||
                    subTspaL != null && subTspaL.SubPremiseNavigation.IdPremisesNavigation.IdPremises == id ||
                    subTspaL != null && subTspaL.SubPremiseNavigation.IdSubPremises == id
                select tp);
            var ownerProcess = (
                from op in registryContext.OwnerProcesses
                join oba in registryContext.OwnerBuildingsAssoc
                    on op.IdProcess equals oba.IdProcess into ObaL
                from subObaL in ObaL.DefaultIfEmpty()
                join opa in registryContext.OwnerPremisesAssoc
                    on op.IdProcess equals opa.IdProcess into OpaL
                from subOpaL in OpaL.DefaultIfEmpty()
                join ospa in registryContext.OwnerSubPremisesAssoc
                    on op.IdProcess equals ospa.IdProcess into OspaL
                from subOspaL in OspaL.DefaultIfEmpty()
                where
                    subObaL != null && subObaL.BuildingNavigation.IdStreet.Equals(filterOptions.Address.Id) ||
                    subOpaL != null && subOpaL.PremiseNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id) ||
                    subOspaL != null && subOspaL.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id) ||
                    subObaL != null && subObaL.BuildingNavigation.IdBuilding == id ||
                    subOpaL != null && subOpaL.PremiseNavigation.IdBuildingNavigation.IdBuilding == id ||
                    subOspaL != null && subOspaL.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdBuilding == id ||
                    subOpaL != null && subOpaL.PremiseNavigation.IdPremises == id ||
                    subOspaL != null && subOspaL.SubPremiseNavigation.IdPremisesNavigation.IdPremises == id ||
                    subOspaL != null && subOspaL.SubPremiseNavigation.IdSubPremises == id
                select op);
            var pr1 =
                from r in reestr
                join tp in tenancyProcess
                    on r.Id equals tp.IdProcess
                where r.Type == ProcessOwnershipTypeEnum.Municipal
                select r;
            var pr2 =
                from r in reestr
                join op in ownerProcess
                    on r.Id equals op.IdProcess
                where r.Type == ProcessOwnershipTypeEnum.Private
                select r;
            var pr = pr1.Union(pr2);
            return pr;
        }

        private IEnumerable<ProcessOwnership> AddressFilter(IEnumerable<ProcessOwnership> reestr, ReestrEmergencyPremisesFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return reestr;
            var ownerBuildingsAssoc = registryContext.OwnerBuildingsAssoc
                .Include(oba => oba.BuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(oba => oba.IdProcessNavigation)
                .AsNoTracking();
            var ownerPremisesAssoc = registryContext.OwnerPremisesAssoc
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdBuildingNavigation)
                        .ThenInclude(b => b.IdStreetNavigation)
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.IdProcessNavigation)
                .AsNoTracking();
            var ownerSubPremisesAssoc = registryContext.OwnerSubPremisesAssoc
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdBuildingNavigation)
                            .ThenInclude(b => b.IdStreetNavigation)
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.IdProcessNavigation)
                .AsNoTracking();
            var tenancyBuildingsAssoc = registryContext.TenancyBuildingsAssoc
                .Include(oba => oba.BuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(oba => oba.ProcessNavigation)
                .AsNoTracking();
            var tenancyPremisesAssoc = registryContext.TenancyPremisesAssoc
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdBuildingNavigation)
                        .ThenInclude(b => b.IdStreetNavigation)
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.ProcessNavigation)
                .AsNoTracking();
            var tenancySubPremisesAssoc = registryContext.TenancySubPremisesAssoc
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdBuildingNavigation)
                            .ThenInclude(b => b.IdStreetNavigation)
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.ProcessNavigation)
                .AsNoTracking();
            if (filterOptions.ProcessOwnershipType == ProcessOwnershipTypeEnum.Private)
                return GetAddressFilter(reestr, ProcessOwnershipTypeEnum.Private, filterOptions, ownerBuildingsAssoc, ownerPremisesAssoc, ownerSubPremisesAssoc);
            if (filterOptions.ProcessOwnershipType == ProcessOwnershipTypeEnum.Municipal)
                return GetAddressFilter(reestr, ProcessOwnershipTypeEnum.Municipal, filterOptions, tenancyBuildingsAssoc, tenancyPremisesAssoc, tenancySubPremisesAssoc);
            var ownerReestr = GetAddressFilter(reestr, ProcessOwnershipTypeEnum.Private, filterOptions, ownerBuildingsAssoc, ownerPremisesAssoc, ownerSubPremisesAssoc);
            var tenancyReestr = GetAddressFilter(reestr, ProcessOwnershipTypeEnum.Municipal, filterOptions, tenancyBuildingsAssoc, tenancyPremisesAssoc, tenancySubPremisesAssoc);
            return ownerReestr.Union(tenancyReestr);
        }

        private IEnumerable<ProcessOwnership> GetAddressFilter(IEnumerable<ProcessOwnership> reestr, ProcessOwnershipTypeEnum type, ReestrEmergencyPremisesFilter filterOptions,
            IQueryable<IBuildingAssoc> buildingsAssoc, IQueryable<IPremiseAssoc> premisesAssoc, IQueryable<ISubPremiseAssoc> subPremisesAssoc)
        {
            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                var idBuildingProcesses = buildingsAssoc
                    .Where(oba => oba.BuildingNavigation.IdStreet.Equals(filterOptions.Address.Id))
                    .Select(oba => oba.IdProcess);
                var idPremiseProcesses = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id))
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id))
                    .Select(ospa => ospa.IdProcess);
                var idProcesses = idBuildingProcesses.Union(idPremiseProcesses).Union(idSubPremiseProcesses);
                var results = (
                    from r in reestr
                    join idProcess in idProcesses on r.Id equals idProcess
                    where r.Type == type
                    select r);
                return results;
            }
            int id = 0;
            if (!int.TryParse(filterOptions.Address.Id, out id))
                return reestr;
            if (filterOptions.Address.AddressType == AddressTypes.Building)
            {
                var idBuildingProcesses = buildingsAssoc
                    .Where(oba => oba.IdBuilding == id)
                    .Select(oba => oba.IdProcess);
                var idPremiseProcesses = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuilding == id)
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuilding == id)
                    .Select(ospa => ospa.IdProcess);
                var idProcesses = idBuildingProcesses.Union(idPremiseProcesses).Union(idSubPremiseProcesses);
                var results = (
                    from r in reestr
                    join idProcess in idProcesses on r.Id equals idProcess
                    where r.Type == type
                    select r);
                return results;
            }
            if (filterOptions.Address.AddressType == AddressTypes.Premise)
            {
                var idPremiseProcesses = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdPremises == id)
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdPremises == id)
                    .Select(ospa => ospa.IdProcess);
                var idProcesses = idPremiseProcesses.Union(idSubPremiseProcesses);
                var results = (
                    from r in reestr
                    join idProcess in idProcesses on r.Id equals idProcess
                    where r.Type == type
                    select r);
                return results;
            }
            if (filterOptions.Address.AddressType == AddressTypes.SubPremise)
            {
                var idProcesses = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdSubPremises == id)
                    .Select(ospa => ospa.IdProcess);
                var results = (
                    from r in reestr
                    join idProcess in idProcesses on r.Id equals idProcess
                    where r.Type == type
                    select r);
                return results;
            }
            return reestr;
        }

        private IEnumerable<ProcessOwnership> GetReestrPage(IEnumerable<ProcessOwnership> reestr, PageOptions pageOptions)
        {
            return reestr
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
        }

        public byte[] GetFileReestr()
        {
            var logStr = new StringBuilder();
            try
            {
                var p = new Process();
                var configXml = activityManagerPath + "templates\\registry_web\\owners\\reestr.xml";
                var destFileName = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", "reestr_" + Guid.NewGuid().ToString() + ".docx");

                p.StartInfo.UseShellExecute = false;
                p.StartInfo.FileName = activityManagerPath + "ActivityManager.exe";
                p.StartInfo.Arguments = " config=\"" + configXml + "\" destFileName=\"" + destFileName +
                    "\" connectionString=\"Driver={" + sqlDriver + "};" +
                    connString + "\"";
                logStr.Append("<dl>\n<dt>Arguments\n<dd>" + p.StartInfo.Arguments + "\n");
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();
                var file = File.ReadAllBytes(destFileName);
                File.Delete(destFileName);
                return file;
            }
            catch (Exception ex)
            {
                logStr.Append("<dl>\n<dt>Error\n<dd>" + ex.Message + "\n</dl>");
                throw new Exception(logStr.ToString());
            }
        }

        public List<Building> GetEmergencyMKD()
        {
            var numbers = new int[] { 1, 2, 6, 7 };
            var mkd = (
                from b in registryContext.Buildings
                join ttt in (
                    from ggg in (
                        from oba in registryContext.OwnershipBuildingsAssoc
                        join owr in registryContext.OwnershipRights
                            on oba.IdOwnershipRight equals owr.IdOwnershipRight
                        where numbers.Contains(owr.IdOwnershipRightType)
                        orderby oba.IdBuilding ascending, owr.Date descending
                        select new { oba, owr }
                    )
                    group ggg by ggg.oba.IdBuilding into gr
                    select new
                    {
                        oba = gr.Select(g => g.oba).FirstOrDefault(),
                        owr = gr.Select(g => g.owr).FirstOrDefault()
                    }
                ) on b.IdBuilding equals ttt.oba.IdBuilding
                where ttt.owr.IdOwnershipRightType == 7
                select b
            );
            return mkd.ToList();
        }

        public List<ProcessOwnership> GetTenancyProcessesReestr(List<Building> mkd)
        {
            var tap1 =
                from tap in registryContext.TenancyActiveProcesses
                join tpa in registryContext.TenancyPremisesAssoc
                    on tap.IdProcess equals tpa.IdProcess
                join p in registryContext.Premises
                    on tpa.IdPremise equals p.IdPremises
                join premType in registryContext.PremisesTypes
                    on p.IdPremisesType equals premType.IdPremisesType
                join b in registryContext.Buildings
                    on p.IdBuilding equals b.IdBuilding
                join street in registryContext.KladrStreets
                    on b.IdStreet equals street.IdStreet
                join m in mkd
                    on b.IdBuilding equals m.IdBuilding
                group new
                {
                    tap,
                    address = street.StreetName + ", д." + b.House + ", " + premType.PremisesTypeShort + p.PremisesNum,
                    numRooms = p.NumRooms,
                    totalArea = p.TotalArea,
                    livingArea = p.LivingArea
                } by tap into procGr
                select new ProcessOwnership()
                {
                    Id = procGr.Key.IdProcess,
                    Addresses = procGr.Select(p => p.address),
                    Type = ProcessOwnershipTypeEnum.Municipal,
                    Persons = procGr.Key.Tenants,
                    CountPersons = procGr.Key.CountTenants,
                    NumRooms = procGr.Sum(p => p.numRooms),
                    TotalArea = procGr.Sum(p => p.totalArea),
                    LivingArea = procGr.Sum(p => p.livingArea)
                };
            var tap2 =
                from tap in registryContext.TenancyActiveProcesses
                join tspa in registryContext.TenancySubPremisesAssoc
                    on tap.IdProcess equals tspa.IdProcess
                join sp in registryContext.SubPremises
                    on tspa.IdSubPremise equals sp.IdSubPremises
                join p in registryContext.Premises
                    on sp.IdPremises equals p.IdPremises
                join premType in registryContext.PremisesTypes
                    on p.IdPremisesType equals premType.IdPremisesType
                join b in registryContext.Buildings
                    on p.IdBuilding equals b.IdBuilding
                join street in registryContext.KladrStreets
                    on b.IdStreet equals street.IdStreet
                join m in mkd
                    on b.IdBuilding equals m.IdBuilding
                group new
                {
                    tap,
                    address = street.StreetName + ", д." + b.House + ", " + premType.PremisesTypeShort + p.PremisesNum + ", ком." + sp.SubPremisesNum,
                    numRooms = 1,
                    totalArea = sp.TotalArea,
                    livingArea = sp.LivingArea
                } by tap into procGr
                select new ProcessOwnership()
                {
                    Id = procGr.Key.IdProcess,
                    Addresses = procGr.Select(sp => sp.address),
                    Type = ProcessOwnershipTypeEnum.Municipal,
                    Persons = procGr.Key.Tenants,
                    CountPersons = procGr.Key.CountTenants,
                    NumRooms = procGr.Sum(sp => sp.numRooms),
                    TotalArea = procGr.Sum(sp => sp.totalArea),
                    LivingArea = procGr.Sum(sp => sp.livingArea)
                };
            return tap1.Union(tap2).ToList();
        }

        public List<ProcessOwnership> GetOwnerProcessesReestr(List<Building> mkd)
        {
            var oap1 =
                from oap in registryContext.OwnerActiveProcesses
                join opa in registryContext.OwnerPremisesAssoc
                    on oap.IdProcess equals opa.IdProcess
                join p in registryContext.Premises
                    on opa.IdPremise equals p.IdPremises
                join premType in registryContext.PremisesTypes
                    on p.IdPremisesType equals premType.IdPremisesType
                join b in registryContext.Buildings
                    on p.IdBuilding equals b.IdBuilding
                join street in registryContext.KladrStreets
                    on b.IdStreet equals street.IdStreet
                join m in mkd
                    on b.IdBuilding equals m.IdBuilding
                group new
                {
                    oap,
                    address = street.StreetName + ", д." + b.House + ", " + premType.PremisesTypeShort + p.PremisesNum,
                    numRooms = p.NumRooms,
                    totalArea = p.TotalArea,
                    livingArea = p.LivingArea
                } by oap into procGr
                select new ProcessOwnership()
                {
                    Id = procGr.Key.IdProcess,
                    Addresses = procGr.Select(p => p.address),
                    Type = ProcessOwnershipTypeEnum.Private,
                    Persons = procGr.Key.Owners,
                    CountPersons = procGr.Key.CountOwners,
                    NumRooms = procGr.Sum(p => p.numRooms),
                    TotalArea = procGr.Sum(p => p.totalArea),
                    LivingArea = procGr.Sum(p => p.livingArea)
                };
            var oap2 =
                from oap in registryContext.OwnerActiveProcesses
                join ospa in registryContext.OwnerSubPremisesAssoc
                    on oap.IdProcess equals ospa.IdProcess
                join sp in registryContext.SubPremises
                    on ospa.IdSubPremise equals sp.IdSubPremises
                join p in registryContext.Premises
                    on sp.IdPremises equals p.IdPremises
                join premType in registryContext.PremisesTypes
                    on p.IdPremisesType equals premType.IdPremisesType
                join b in registryContext.Buildings
                    on p.IdBuilding equals b.IdBuilding
                join street in registryContext.KladrStreets
                    on b.IdStreet equals street.IdStreet
                join m in mkd
                    on b.IdBuilding equals m.IdBuilding
                group new
                {
                    oap,
                    address = street.StreetName + ", д." + b.House + ", " + premType.PremisesTypeShort + p.PremisesNum + ", ком." + sp.SubPremisesNum,
                    numRooms = 1,
                    totalArea = sp.TotalArea,
                    livingArea = sp.LivingArea
                } by oap into procGr
                select new ProcessOwnership()
                {
                    Id = procGr.Key.IdProcess,
                    Addresses = procGr.Select(sp => sp.address),
                    Type = ProcessOwnershipTypeEnum.Private,
                    Persons = procGr.Key.Owners,
                    CountPersons = procGr.Key.CountOwners,
                    NumRooms = procGr.Sum(sp => sp.numRooms),
                    TotalArea = procGr.Sum(sp => sp.totalArea),
                    LivingArea = procGr.Sum(sp => sp.livingArea)
                };
            return oap1.Union(oap2).ToList();
        }
    }
}
