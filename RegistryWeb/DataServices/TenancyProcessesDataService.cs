using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace RegistryWeb.DataServices
{
    public class TenancyProcessesDataService : ListDataService<TenancyProcessesVM, TenancyProcessesFilter>
    {
        private readonly IQueryable<TenancyBuildingAssoc> tenancyBuildingsAssoc;
        private readonly IQueryable<TenancyPremiseAssoc> tenancyPremisesAssoc;
        private readonly IQueryable<TenancySubPremiseAssoc> tenancySubPremisesAssoc;

        public TenancyProcessesDataService(RegistryContext registryContext) : base(registryContext)
        {
            tenancyBuildingsAssoc = registryContext.TenancyBuildingsAssoc
                    .Include(oba => oba.BuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                    .Include(oba => oba.ProcessNavigation)
                .AsNoTracking();
            tenancyPremisesAssoc = registryContext.TenancyPremisesAssoc
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdBuildingNavigation)
                        .ThenInclude(b => b.IdStreetNavigation)
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.ProcessNavigation)
                .AsNoTracking();
            tenancySubPremisesAssoc = registryContext.TenancySubPremisesAssoc
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdBuildingNavigation)
                            .ThenInclude(b => b.IdStreetNavigation)
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.ProcessNavigation)
                .AsNoTracking();
        }

        public override TenancyProcessesVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, TenancyProcessesFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.ReasonTypes = registryContext.TenancyReasonTypes;
            viewModel.RentTypes = registryContext.RentTypes;
            viewModel.Streets = registryContext.KladrStreets;
            viewModel.OwnershipRightTypes = registryContext.OwnershipRightTypes;
            viewModel.ObjectStates = registryContext.ObjectStates;
            return viewModel;
        }

        internal TenancyProcessesVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            TenancyProcessesFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var tenancyProcesses = GetQuery();
            viewModel.PageOptions.TotalRows = tenancyProcesses.Count();
            var query = GetQueryFilter(tenancyProcesses, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.TenancyProcesses = GetQueryPage(query, viewModel.PageOptions).ToList();
            viewModel.Addresses = GetAddresses(viewModel.TenancyProcesses);
            return viewModel;
        }

        private IQueryable<TenancyProcess> GetQuery()
        {
            return registryContext.TenancyProcesses
                .Include(tp => tp.IdRentTypeNavigation)
                .Include(tp => tp.TenancyPersons).AsNoTracking();
        }

        private Dictionary<int, List<Address>> GetAddresses(IEnumerable<TenancyProcess> tenancyProcesses)
        {
            var buildings = from tbaRow in tenancyBuildingsAssoc
                                 join tpRow in tenancyProcesses
                                 on tbaRow.IdProcess equals tpRow.IdProcess
                                 join buildingRow in registryContext.Buildings
                                 on tbaRow.IdBuilding equals buildingRow.IdBuilding
                                 join streetRow in registryContext.KladrStreets
                                 on buildingRow.IdStreet equals streetRow.IdStreet
                                 select new
                                 {
                                     tpRow.IdProcess,
                                     Addresses = new Address
                                     {
                                         AddressType = AddressTypes.Building,
                                         Id = buildingRow.IdBuilding.ToString(),
                                         Text = streetRow.StreetName + ", д." + buildingRow.House
                                     }
                                 };
            var premises = from tpaRow in tenancyPremisesAssoc
                                join tpRow in tenancyProcesses
                                on tpaRow.IdProcess equals tpRow.IdProcess
                                join premiseRow in registryContext.Premises
                                on tpaRow.IdPremise equals premiseRow.IdPremises
                                join buildingRow in registryContext.Buildings
                                on premiseRow.IdBuilding equals buildingRow.IdBuilding
                                join streetRow in registryContext.KladrStreets
                                on buildingRow.IdStreet equals streetRow.IdStreet
                                join premiseTypesRow in registryContext.PremisesTypes
                                on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                                select new
                                {
                                    tpRow.IdProcess,
                                    Addresses = new Address
                                    {
                                        AddressType = AddressTypes.Premise,
                                        Id = premiseRow.IdPremises.ToString(),
                                        Text = streetRow.StreetName + ", д." + buildingRow.House + ", " +
                                        premiseTypesRow.PremisesTypeShort + premiseRow.PremisesNum
                                    }
                                };
            var subPremises = from tspaRow in tenancySubPremisesAssoc
                                   join tpRow in tenancyProcesses
                                   on tspaRow.IdProcess equals tpRow.IdProcess
                                   join subPremiseRow in registryContext.SubPremises
                                   on tspaRow.IdSubPremise equals subPremiseRow.IdSubPremises
                                   join premiseRow in registryContext.Premises
                                   on subPremiseRow.IdPremises equals premiseRow.IdPremises
                                   join buildingRow in registryContext.Buildings
                                   on premiseRow.IdBuilding equals buildingRow.IdBuilding
                                   join streetRow in registryContext.KladrStreets
                                   on buildingRow.IdStreet equals streetRow.IdStreet
                                   join premiseTypesRow in registryContext.PremisesTypes
                                   on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                                   select new
                                   {
                                       tpRow.IdProcess,
                                       Addresses = new Address
                                       {
                                           AddressType = AddressTypes.SubPremise,
                                           Id = subPremiseRow.IdSubPremises.ToString(),
                                           Text = streetRow.StreetName + ", д." + buildingRow.House + ", " +
                                           premiseTypesRow.PremisesTypeShort + premiseRow.PremisesNum + ", к." + subPremiseRow.SubPremisesNum
                                       }
                                   };
            var result = buildings.Union(premises).Union(subPremises).ToList().GroupBy(r => r.IdProcess)
                .Select(r => new { IdProcess = r.Key, Addresses = r.Select(v => v.Addresses) })
                .ToDictionary(v => v.IdProcess, v => v.Addresses.ToList());
            return result;
        }

        private IQueryable<TenancyProcess> GetQueryFilter(IQueryable<TenancyProcess> query, TenancyProcessesFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
            }
            return query;
        }

        private IQueryable<TenancyProcess> AddressFilter(IQueryable<TenancyProcess> query, TenancyProcessesFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return query;
            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                var idBuildingProcesses = tenancyBuildingsAssoc
                    .Where(oba => oba.BuildingNavigation.IdStreet.Equals(filterOptions.Address.Id))
                    .Select(oba => oba.IdProcess);
                var idPremiseProcesses = tenancyPremisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id))
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = tenancySubPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id))
                    .Select(ospa => ospa.IdProcess);
                var idProcesses = idBuildingProcesses.Union(idPremiseProcesses).Union(idSubPremiseProcesses);
                return
                    from q in query
                    join idProcess in idProcesses on q.IdProcess equals idProcess
                    select q;
            }
            int id = 0;
            if (!int.TryParse(filterOptions.Address.Id, out id))
                return query;
            if (filterOptions.Address.AddressType == AddressTypes.Building)
            {
                var idBuildingProcesses = tenancyBuildingsAssoc
                    .Where(oba => oba.IdBuilding == id)
                    .Select(oba => oba.IdProcess);
                var idPremiseProcesses = tenancyPremisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuilding == id)
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = tenancySubPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuilding == id)
                    .Select(ospa => ospa.IdProcess);
                var idProcesses = idBuildingProcesses.Union(idPremiseProcesses).Union(idSubPremiseProcesses);
                return
                    from q in query
                    join idProcess in idProcesses on q.IdProcess equals idProcess
                    select q;
            }
            if (filterOptions.Address.AddressType == AddressTypes.Premise)
            {
                var idPremiseProcesses = tenancyPremisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdPremises == id)
                    .Select(opa => opa.IdProcess);
                var idSubPremiseProcesses = tenancySubPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdPremises == id)
                    .Select(ospa => ospa.IdProcess);
                var idProcesses = idPremiseProcesses.Union(idSubPremiseProcesses);
                return
                    from q in query
                    join idProcess in idProcesses on q.IdProcess equals idProcess
                    select q;
            }
            if (filterOptions.Address.AddressType == AddressTypes.SubPremise)
            {
                var idProcesses = tenancySubPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdSubPremises == id)
                    .Select(ospa => ospa.IdProcess);
                return
                    from q in query
                    join idProcess in idProcesses on q.IdProcess equals idProcess
                    select q;
            }
            return query;
        }

        private IQueryable<TenancyProcess> GetQueryOrder(IQueryable<TenancyProcess> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField) || orderOptions.OrderField == "IdProcess")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdProcess);
                else
                    return query.OrderByDescending(p => p.IdProcess);
            }
            if (orderOptions.OrderField == "RegistrationNum")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.RegistrationNum == null ? p.ResidenceWarrantNum : p.RegistrationNum);
                else
                    return query.OrderByDescending(p => p.RegistrationNum == null ? p.ResidenceWarrantNum : p.RegistrationNum);
            }
            if (orderOptions.OrderField == "Address")
            {
                var addresses = tenancyBuildingsAssoc.Select(b => new
                {
                    b.IdProcess,
                    Address = string.Concat(b.BuildingNavigation.IdStreetNavigation.StreetName, ", ", b.BuildingNavigation.House)
                }).Union(tenancyPremisesAssoc.Select(
                    p => new
                    {
                        p.IdProcess,
                        Address = string.Concat(p.PremiseNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName, ", ",
                            p.PremiseNavigation.IdBuildingNavigation.House, ", ", p.PremiseNavigation.PremisesNum)
                    })
                ).Union(tenancySubPremisesAssoc.Select(
                    sp => new
                    {
                        sp.IdProcess,
                        Address = string.Concat(sp.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName, ", ",
                            sp.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.House, ", ",
                            sp.SubPremiseNavigation.IdPremisesNavigation.PremisesNum, ", ", sp.SubPremiseNavigation.SubPremisesNum)
                    }));
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                {
                    return (from row in query
                            join addr in addresses
                             on row.IdProcess equals addr.IdProcess into aq
                            from aqRow in aq.DefaultIfEmpty()
                            orderby aqRow.Address
                            select row).Distinct();
                } else
                {
                    return (from row in query
                            join addr in addresses
                             on row.IdProcess equals addr.IdProcess into aq
                            from aqRow in aq.DefaultIfEmpty()
                            orderby aqRow.Address descending
                            select row).Distinct();
                }
            }
            return query;
        }

        private IQueryable<TenancyProcess> GetQueryPage(IQueryable<TenancyProcess> query, PageOptions pageOptions)
        {
            return query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
        }
    }
}