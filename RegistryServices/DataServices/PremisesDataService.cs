using RegistryDb.Models;
using RegistryDb.Models.Entities;
using RegistryWeb.ViewModel;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Runtime.CompilerServices;
using RegistryWeb.DataHelpers;
using RegistryWeb.SecurityServices;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Configuration;
using RegistryDb.Models.SqlViews;
using RegistryWeb.Enums;

namespace RegistryWeb.DataServices
{
    public class PremisesDataService : ListDataService<PremisesVM<Premise>, PremisesListFilter>
    {
        private readonly IConfiguration config;
        private SecurityService securityService;
        public PremisesDataService(RegistryContext registryContext, SecurityService securityService, 
            AddressesDataService addressesDataService, IConfiguration config) : base(registryContext, addressesDataService)
        {
            this.securityService = securityService;
            this.config = config;
        }

        public override PremisesVM<Premise> InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, PremisesListFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.KladrRegionsList = new SelectList(addressesDataService.KladrRegions, "id_region", "region");
            viewModel.KladrStreetsList = new SelectList(addressesDataService.GetKladrStreets(filterOptions?.IdRegion), "IdStreet", "StreetName");
            viewModel.HeatingTypesList = new SelectList(HeatingTypes, "IdHeatingType", "IdHeatingType1");
            viewModel.StructureTypesList = new SelectList(StructureTypes, "IdStructureType", "StructureTypeName");
            viewModel.ObjectStatesList = new SelectList(ObjectStates, "IdState", "StateFemale");
            viewModel.PremisesTypesList = new SelectList(addressesDataService.PremisesTypes, "IdPremisesType", "PremisesTypeName");
            viewModel.FundTypesList = new SelectList(registryContext.FundTypes, "IdFundType", "FundTypeName");
            viewModel.OwnershipRightTypesList = new SelectList(registryContext.OwnershipRightTypes, "IdOwnershipRightType", "OwnershipRightTypeName");
            viewModel.RestrictionsList = new SelectList(registryContext.RestrictionTypes, "IdRestrictionType", "RestrictionTypeName");
            viewModel.LocationKeysList = new SelectList(registryContext.PremisesDoorKeys, "IdPremisesDoorKeys", "LocationOfKeys");
            viewModel.CommentList = new SelectList(registryContext.PremisesComments, "IdPremisesComment", "PremisesCommentText");
            viewModel.SignersList = new SelectList(registryContext.SelectableSigners.Where(s => s.IdSignerGroup == 1).ToList().Select(s => new {
                s.IdRecord,
                Snp = s.Surname + " " + s.Name + (s.Patronymic == null ? "" : " " + s.Patronymic)
            }), "IdRecord", "Snp");
            viewModel.PreparersList= new SelectList(registryContext.Preparers, "IdPreparer", "PreparerName");
            return viewModel;
        }

        public PremisesVM<Premise> GetViewModel(
            OrderOptions orderOptions,            
            PageOptions pageOptions,
            PremisesListFilter filterOptions, out List<int> filteredPremisesIds)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var query = GetQuery();
            viewModel.PageOptions.TotalRows = query.Count();
            query = GetQueryFilter(query, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            query = GetQueryIncludes(query);

            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            filteredPremisesIds = query.Select(p => p.IdPremises).ToList();


            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.Premises = GetQueryPage(query, viewModel.PageOptions).ToList();
            viewModel.PaymentsInfo = GetPaymentInfo(viewModel.Premises);
            viewModel.PremisesOwnershipRightCurrent = GetPremisesOwnershipRightCurrent(viewModel.Premises);
            viewModel.ActiveTenancies = GetActiveTenancies(viewModel.Premises);
            return viewModel;
        }

        private Dictionary<int, List<TenancyProcess>> GetActiveTenancies(List<Premise> premises)
        {
            var ids = premises.Select(p => p.IdPremises).ToList();

            var premiseProcesses = (from tpaRow in registryContext.TenancyPremisesAssoc
                                    join tpRow in registryContext.TenancyProcesses
                                    on tpaRow.IdProcess equals tpRow.IdProcess
                                    where ids.Contains(tpaRow.IdPremise) && tpRow.RegistrationNum != null && !tpRow.RegistrationNum.Contains("н")
                                    group tpRow by tpaRow.IdPremise into gp
                                    select new { IdPremise = gp.Key, Processes = gp.ToList() }).ToDictionary(a => a.IdPremise, b => b.Processes);

            var subPremiseProcesses = (from tspaRow in registryContext.TenancySubPremisesAssoc
                                      join spRow in registryContext.SubPremises
                                      on tspaRow.IdSubPremise equals spRow.IdSubPremises
                                      join tpRow in registryContext.TenancyProcesses
                                      on tspaRow.IdProcess equals tpRow.IdProcess
                                      where ids.Contains(spRow.IdPremises) && tpRow.RegistrationNum != null && !tpRow.RegistrationNum.Contains("н")
                                      group tpRow by spRow.IdPremises into gp
                                      select new { IdPremise = gp.Key, Processes = gp.Distinct().ToList() }).ToDictionary(a => a.IdPremise, b => b.Processes);

            return (from row in premiseProcesses.Union(subPremiseProcesses)
                   group row by row.Key into gp
                   select new { IdPremise = gp.Key, Processes = gp.Select(g => g.Value).ToList() })
                   .ToDictionary(a => a.IdPremise, b => b.Processes.SelectMany(p => p).ToList());
        }

        private IQueryable<Premise> GetQueryIncludes(IQueryable<Premise> query)
        {
            return query
                .Include(p => p.IdBuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(p => p.IdStateNavigation)        //Текущее состояние объекта
                .Include(p => p.IdPremisesTypeNavigation) //Тип помещения: квартира, комната, квартира с подселением
                .Include(p => p.FundsPremisesAssoc)
                    .ThenInclude(fpa => fpa.IdFundNavigation)
                        .ThenInclude(fh => fh.IdFundTypeNavigation)
                .Include(p => p.SubPremises);
        }

        public IQueryable<Premise> GetQuery()
        {
            return GetQueryIncludes(registryContext.Premises);           
        }


        private IQueryable<Premise> GetQueryFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                if (!filterOptions.IsAddressEmpty())
                    query = AddressFilter(query, filterOptions);

                query = IdPremisesFilter(query, filterOptions);
                query = IdBuildingFilter(query, filterOptions);
                query = RegionFilter(query, filterOptions);
                query = StreetFilter(query, filterOptions);
                query = HouseFilter(query, filterOptions);
                query = PremiseNumFilter(query, filterOptions);
                query = FloorsFilter(query, filterOptions);
                query = CadastralNumFilter(query, filterOptions);
                query = FundsFilter(query, filterOptions);
                query = ObjectStateFilter(query, filterOptions);
                query = CommentFilter(query, filterOptions);
                query = DoorKeysFilter(query, filterOptions);
                query = OwnershipRightFilter(query, filterOptions);
                query = RestrictionFilter(query, filterOptions);
                query = DateFilter(query, filterOptions);
            }
            return query;
        }

        private IQueryable<Premise> AddressFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return query;

            var addresses = addressesDataService.GetAddressesByText(filterOptions.Address.Text).Select(r => r.Id);

            if (filterOptions.Address.AddressType == AddressTypes.Street)            
                return query.Where(q => addresses.Contains(q.IdBuildingNavigation.IdStreet));

            var addressesInt = addresses.Where(a => int.TryParse(a, out int aInt)).Select(a => int.Parse(a));
            
            if (!addressesInt.Any())
                return query;

            if (filterOptions.Address.AddressType == AddressTypes.Building)            
                return query.Where(q => addressesInt.Contains(q.IdBuilding));
            
            if (filterOptions.Address.AddressType == AddressTypes.Premise)            
                return query.Where(q => addressesInt.Contains(q.IdPremises));
            
            if (filterOptions.Address.AddressType == AddressTypes.SubPremise)
            {
                return from q in query
                       join sp in registryContext.SubPremises
                       on q.IdPremises equals sp.IdPremises
                       where addressesInt.Contains(sp.IdSubPremises)
                       select q;
            }
            return query;
        }

        private IQueryable<Premise> IdPremisesFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IdPremise.HasValue)
            {
                query = query.Where(p => p.IdPremises == filterOptions.IdPremise.Value);
            }
            return query;
        }

        private IQueryable<Premise> IdBuildingFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IdBuilding.HasValue)
            {
                query = query.Where(b => b.IdBuilding == filterOptions.IdBuilding.Value);
            }
            return query;
        }

        private IQueryable<Premise> RegionFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (string.IsNullOrEmpty(filterOptions.IdStreet) && !string.IsNullOrEmpty(filterOptions.IdRegion))
            {
                query = query.Where(b => b.IdBuildingNavigation.IdStreet.Contains(filterOptions.IdRegion));
            }
            return query;
        }

        private IQueryable<Premise> StreetFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                query = query.Where(b => b.IdBuildingNavigation.IdStreet == filterOptions.IdStreet);
            }
            return query;
        }

        private IQueryable<Premise> HouseFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                query = query.Where(b => b.IdBuildingNavigation.House.ToLower() == filterOptions.House.ToLower());
            }
            return query;
        }

        private IQueryable<Premise> PremiseNumFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                query = query.Where(b => b.PremisesNum.ToLower() == filterOptions.PremisesNum.ToLower());
            }
            return query;
        }

        private IQueryable<Premise> FloorsFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.Floors.HasValue)
            {
                query = query.Where(b => b.Floor == filterOptions.Floors.Value);
            }
            return query;
        }

        private IQueryable<Premise> CadastralNumFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.CadastralNum))
            {
                query = query.Where(b => b.CadastralNum == filterOptions.CadastralNum);
            }
            return query;
        }

        private IQueryable<Premise> FundsFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IdFundType != null && filterOptions.IdFundType.Any())
            {
                var maxFunds = from fundRow in registryContext.FundsHistory
                               join fundPremisesRow in registryContext.FundsPremisesAssoc
                               on fundRow.IdFund equals fundPremisesRow.IdFund
                               where fundRow.ExcludeRestrictionDate == null
                               group fundPremisesRow.IdFund by fundPremisesRow.IdPremises into gs
                               select new
                               {
                                   IdPremises = gs.Key,
                                   IdFund = gs.Max()
                               };
                var idPremises = from fundRow in registryContext.FundsHistory
                                 join maxFundRow in maxFunds
                                 on fundRow.IdFund equals maxFundRow.IdFund
                                 join premisesRow in registryContext.Premises
                                 on maxFundRow.IdPremises equals premisesRow.IdPremises
                                 where filterOptions.IdFundType.Contains(fundRow.IdFundType) && ObjectStateHelper.IsMunicipal(premisesRow.IdState)
                                 select maxFundRow.IdPremises;

                var maxFundsSubPremises = from fundRow in registryContext.FundsHistory
                               join fundSubPremisesRow in registryContext.FundsSubPremisesAssoc
                               on fundRow.IdFund equals fundSubPremisesRow.IdFund
                               where fundRow.ExcludeRestrictionDate == null
                               group fundSubPremisesRow.IdFund by fundSubPremisesRow.IdSubPremises into gs
                               select new
                               {
                                   IdSubPremises = gs.Key,
                                   IdFund = gs.Max()
                               };

                var idPremisesByRooms = (from fundRow in registryContext.FundsHistory
                                        join maxFundRow in maxFundsSubPremises
                                        on fundRow.IdFund equals maxFundRow.IdFund
                                        join subPremiseRow in registryContext.SubPremises
                                        on maxFundRow.IdSubPremises equals subPremiseRow.IdSubPremises
                                        where filterOptions.IdFundType.Contains(fundRow.IdFundType)
                                        select subPremiseRow.IdPremises).Distinct();

                query = from row in query
                        join idPremise in idPremises.Union(idPremisesByRooms)
                        on row.IdPremises equals idPremise
                        select row;
            }
            return query;
        }

        private IQueryable<Premise> ObjectStateFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IdsObjectState != null && filterOptions.IdsObjectState.Any())
            {
                var ids = registryContext.SubPremises
                    .Where(sp => filterOptions.IdsObjectState.Contains(sp.IdState))
                    .Select(sp => sp.IdPremises)
                    .Distinct()
                    .ToList();
                query = query.Where(p =>
                    filterOptions.IdsObjectState.Contains(p.IdState) ||
                    ids.Contains(p.IdPremises)
                );
            }
            return query;
        }
        
        private IQueryable<Premise> CommentFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IdsComment != null && filterOptions.IdsComment.Any())
            {
                if (filterOptions.IdsCommentContains == null || filterOptions.IdsCommentContains.Value)
                    query = query.Where(p => filterOptions.IdsComment.Contains(p.IdPremisesComment));
                else
                    query = query.Where(p => !filterOptions.IdsComment.Contains(p.IdPremisesComment));
            }
            return query;
        }

        private IQueryable<Premise> DoorKeysFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IdsDoorKeys != null && filterOptions.IdsDoorKeys.Any())
            {
                if (filterOptions.IdsDoorKeysContains == null || filterOptions.IdsDoorKeysContains.Value)
                    query = query.Where(p => filterOptions.IdsDoorKeys.Contains(p.IdPremisesDoorKeys));
                else
                    query = query.Where(p => !filterOptions.IdsDoorKeys.Contains(p.IdPremisesDoorKeys));
            }
            return query;
        }

        private IQueryable<Premise> OwnershipRightFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.NumberOwnershipRight) || filterOptions.DateOwnershipRight != null)
            {
                query = (from q in query
                         join obaRow in registryContext.OwnershipBuildingsAssoc
                         on q.IdBuilding equals obaRow.IdBuilding into b
                         from bRow in b.DefaultIfEmpty()
                         join orRow in registryContext.OwnershipRights
                         on bRow.IdOwnershipRight equals orRow.IdOwnershipRight into bor
                         from borRow in bor.DefaultIfEmpty()

                         join opaRow in registryContext.OwnershipPremisesAssoc
                         on q.IdPremises equals opaRow.IdPremises into p
                         from pRow in p.DefaultIfEmpty()
                         join orRow in registryContext.OwnershipRights
                         on pRow.IdOwnershipRight equals orRow.IdOwnershipRight into por
                         from porRow in por.DefaultIfEmpty()

                         where (borRow != null &&
                            ((string.IsNullOrEmpty(filterOptions.NumberOwnershipRight) ||
                             borRow.Number.ToLower() == filterOptions.NumberOwnershipRight.ToLower()) &&
                             (filterOptions.DateOwnershipRight == null || borRow.Date == filterOptions.DateOwnershipRight))) ||
                             (porRow != null &&
                            ((string.IsNullOrEmpty(filterOptions.NumberOwnershipRight) ||
                             porRow.Number.ToLower() == filterOptions.NumberOwnershipRight.ToLower()) &&
                             (filterOptions.DateOwnershipRight == null || porRow.Date == filterOptions.DateOwnershipRight)))
                         select q).Distinct();
            }
            if (filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Any())
            {
                var idBuildings = registryContext.Premises
                .Select(p => p.IdBuilding);

                var buildings = from tbaRow in registryContext.OwnershipBuildingsAssoc
                                join buildingRow in registryContext.Buildings
                                on tbaRow.IdBuilding equals buildingRow.IdBuilding
                                join streetRow in registryContext.KladrStreets
                                on buildingRow.IdStreet equals streetRow.IdStreet
                                where idBuildings.Contains(tbaRow.IdBuilding)
                                select tbaRow;

                var premises = from tpaRow in registryContext.OwnershipPremisesAssoc
                               join premiseRow in registryContext.Premises
                               on tpaRow.IdPremises equals premiseRow.IdPremises
                               join buildingRow in registryContext.Buildings
                               on premiseRow.IdBuilding equals buildingRow.IdBuilding
                               join streetRow in registryContext.KladrStreets
                               on buildingRow.IdStreet equals streetRow.IdStreet
                               join premiseTypesRow in registryContext.PremisesTypes
                               on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                               select tpaRow;

                if (filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Any())
                {
                    var specialOwnershipRightTypeIds = new int[] { 1, 2, 6, 7 };
                    var specialIds = filterOptions.IdsOwnershipRightType.Where(id => specialOwnershipRightTypeIds.Contains(id));
                    var generalIds = filterOptions.IdsOwnershipRightType.Where(id => !specialOwnershipRightTypeIds.Contains(id));
                    var generalOwnershipRightsPremises = from owrRow in registryContext.OwnershipRights
                                                         join pRow in registryContext.OwnershipPremisesAssoc
                                                         on owrRow.IdOwnershipRight equals pRow.IdOwnershipRight
                                                         where generalIds.Contains(owrRow.IdOwnershipRightType)
                                                         select pRow.IdPremises;

                    var specialOwnershipRightsPremises = from owrRow in registryContext.PremisesOwnershipRightCurrent
                                                         where specialIds.Contains(owrRow.IdOwnershipRightType)
                                                         select owrRow.IdPremises;

                    var ownershipRightsPremisesList = generalOwnershipRightsPremises.Union(specialOwnershipRightsPremises).ToList();

                    var generalOwnershipRightsBuildings = from owrRow in registryContext.OwnershipRights
                                                          join bRow in registryContext.OwnershipBuildingsAssoc
                                                          on owrRow.IdOwnershipRight equals bRow.IdOwnershipRight
                                                          where generalIds.Contains(owrRow.IdOwnershipRightType)
                                                          select bRow.IdBuilding;

                    var specialOwnershipRightsBuildings = from owrRow in registryContext.BuildingsOwnershipRightCurrent
                                                          where specialIds.Contains(owrRow.IdOwnershipRightType)
                                                          select owrRow.IdBuilding;

                    var ownershipRightsBuildingsList = generalOwnershipRightsBuildings.Union(specialOwnershipRightsBuildings).ToList(); //

                    var premisesInBuildingsOwnershipRights = (from pRow in registryContext.Premises
                                                              where ownershipRightsBuildingsList.Contains(pRow.IdBuilding)
                                                              select pRow.IdPremises).ToList();

                    ownershipRightsPremisesList = ownershipRightsPremisesList.Union(premisesInBuildingsOwnershipRights).ToList(); //

                    var buildingIds = (from bRow in buildings
                                      where ownershipRightsBuildingsList.Contains(bRow.IdBuilding)
                                      select bRow.IdBuilding).ToList();

                    var premisesIds = (from pRow in premises
                                      where ownershipRightsPremisesList.Contains(pRow.IdPremises)
                                      select pRow.IdPremises).ToList();
                    
                    if (filterOptions.IdsOwnershipRightTypeContains == null || filterOptions.IdsOwnershipRightTypeContains.Value)
                    {
                        query = (from row in query
                                 where buildingIds.Contains(row.IdBuilding) || premisesIds.Contains(row.IdPremises)
                                 select row).Distinct();
                    } else
                    {
                        query = (from row in query
                                 where !buildingIds.Contains(row.IdBuilding) && !premisesIds.Contains(row.IdPremises)
                                 select row).Distinct();
                    }
                }
            }
            return query;
        }

        public void UpdateAreaAvgCost(TotalAreaAvgCost cost)
        {
            registryContext.TotalAreaAvgCosts.Update(cost);
            registryContext.SaveChanges();
        }

        public TotalAreaAvgCost GetAreaAvgCost()
        {
            return registryContext.TotalAreaAvgCosts.FirstOrDefault();
        }

        private IQueryable<Premise> RestrictionFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if ((filterOptions.IdsRestrictionType != null && filterOptions.IdsRestrictionType.Any()) ||
                !string.IsNullOrEmpty(filterOptions.RestrictionNum) || filterOptions.RestrictionDate != null)
            {
                if (filterOptions.IdsRestrictionType != null && filterOptions.IdsRestrictionType.Any())
                {
                    var contains = filterOptions.IdsRestrictionTypeContains == null || filterOptions.IdsRestrictionTypeContains.Value;

                    var idPremises = from rRow in registryContext.Restrictions
                                     join pRow in registryContext.RestrictionPremisesAssoc
                                     on rRow.IdRestriction equals pRow.IdRestriction
                                     where filterOptions.IdsRestrictionType != null &&
                                           filterOptions.IdsRestrictionType.Contains(rRow.IdRestrictionType)
                                     select pRow.IdPremises;

                    var idBuildings = from rRow in registryContext.Restrictions
                                      join bRow in registryContext.RestrictionBuildingsAssoc
                                      on rRow.IdRestriction equals bRow.IdRestriction
                                      where filterOptions.IdsRestrictionType != null &&
                                            filterOptions.IdsRestrictionType.Contains(rRow.IdRestrictionType)
                                      select bRow.IdBuilding;

                    query = (from q in query
                             where contains ? (idPremises.Contains(q.IdPremises) || idBuildings.Contains(q.IdBuilding))
                                : (!idPremises.Contains(q.IdPremises) && !idBuildings.Contains(q.IdBuilding))
                             select q).Distinct();
                }
                if (!string.IsNullOrEmpty(filterOptions.RestrictionNum) || filterOptions.RestrictionDate != null)
                {
                    query = (from q in query
                             join rbaRow in registryContext.RestrictionBuildingsAssoc
                             on q.IdBuilding equals rbaRow.IdBuilding into b
                             from bRow in b.DefaultIfEmpty()
                             join rRow in registryContext.Restrictions
                             on bRow.IdRestriction equals rRow.IdRestriction into bor
                             from borRow in bor.DefaultIfEmpty()

                             join rpaRow in registryContext.RestrictionPremisesAssoc
                             on q.IdPremises equals rpaRow.IdPremises into p
                             from pRow in p.DefaultIfEmpty()
                             join rRow in registryContext.Restrictions
                             on pRow.IdRestriction equals rRow.IdRestriction into por
                             from porRow in por.DefaultIfEmpty()

                             where (borRow != null &&
                                (string.IsNullOrEmpty(filterOptions.RestrictionNum) ||
                                 borRow.Number.ToLower() == filterOptions.RestrictionNum.ToLower()) &&
                                 (filterOptions.RestrictionDate == null || borRow.Date == filterOptions.RestrictionDate)) ||
                                 (porRow != null &&
                                (string.IsNullOrEmpty(filterOptions.RestrictionNum) ||
                                 porRow.Number.ToLower() == filterOptions.RestrictionNum.ToLower()) &&
                                 (filterOptions.RestrictionDate == null || porRow.Date == filterOptions.RestrictionDate))
                             select q).Distinct();
                }
            }
            return query;
        }

        private IQueryable<Premise> DateFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.StDateOwnershipRight.HasValue && !filterOptions.EndDateOwnershipRight.HasValue)
            {
                var stDate = filterOptions.StDateOwnershipRight.Value.Date;
                query = query.Where(r => r.RegDate != null && r.RegDate >= stDate);
            }
            else if (!filterOptions.StDateOwnershipRight.HasValue && filterOptions.EndDateOwnershipRight.HasValue)
            {
                var fDate = filterOptions.EndDateOwnershipRight.Value.Date.AddDays(1);
                query = query.Where(r => r.RegDate != null && r.RegDate < fDate);
            }
            else if (filterOptions.EndDateOwnershipRight.HasValue && filterOptions.StDateOwnershipRight.HasValue)
            {
                var stDate = filterOptions.StDateOwnershipRight.Value.Date;
                var fDate = filterOptions.EndDateOwnershipRight.Value.Date.AddDays(1);
                query = query.Where(r => r.RegDate != null && r.RegDate >= stDate && r.RegDate < fDate);
            }
            return query;
        }

        public void AddRestrictionsInPremises(List<Premise> processingPremises, Restriction restriction, IFormFile restrictionFile)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Restrictions\");

            for (var i = 0; i < processingPremises.Count(); i++)
            {

                var rest = new Restriction
                {
                    Number = restriction.Number,
                    Date = restriction.Date,
                    DateStateReg = restriction.DateStateReg,
                    Description = restriction.Description,
                    IdRestrictionType = restriction.IdRestrictionType,
                };

                if (restrictionFile != null)
                {
                    rest.FileDisplayName = restrictionFile.FileName;
                    rest.FileOriginName = Guid.NewGuid().ToString() + "." + new FileInfo(restrictionFile.FileName).Extension;
                    rest.FileMimeType = restrictionFile.ContentType;
                    var fileStream = new FileStream(Path.Combine(path, rest.FileOriginName), FileMode.CreateNew);
                    restrictionFile.OpenReadStream().CopyTo(fileStream);
                    fileStream.Close();
                }

                registryContext.Restrictions.Add(rest);
                var rpa = new RestrictionPremiseAssoc()
                {
                    IdPremises = processingPremises[i].IdPremises,
                    RestrictionNavigation = rest
                };
                registryContext.RestrictionPremisesAssoc.Add(rpa);
            }
            registryContext.SaveChanges();
        }

        public void AddOwnershipRightInPremises(List<Premise> processingPremises, OwnershipRight ownershipRight, IFormFile ownershipRightFile)
        {
            var path = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"OwnershipRights\");

            for (var i = 0; i < processingPremises.Count(); i++)
            {
                var owr = new OwnershipRight
                {
                    Number = ownershipRight.Number,
                    Date = ownershipRight.Date,
                    Description = ownershipRight.Description,
                    IdOwnershipRightType = ownershipRight.IdOwnershipRightType
                };

                if (ownershipRightFile != null)
                {
                    owr.FileDisplayName = ownershipRightFile.FileName;
                    owr.FileOriginName = Guid.NewGuid().ToString() + "." + new FileInfo(ownershipRightFile.FileName).Extension;
                    owr.FileMimeType = ownershipRightFile.ContentType;
                    var fileStream = new FileStream(Path.Combine(path, owr.FileOriginName), FileMode.CreateNew);
                    ownershipRightFile.OpenReadStream().CopyTo(fileStream);
                    fileStream.Close();
                }

                registryContext.OwnershipRights.Add(owr);
                var opa = new OwnershipPremiseAssoc()
                {
                    IdPremises = processingPremises[i].IdPremises,
                    OwnershipRightNavigation = owr
                };
                registryContext.OwnershipPremisesAssoc.Add(opa);
            }
            registryContext.SaveChanges();
        }

        private IQueryable<Premise> GetQueryOrder(IQueryable<Premise> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField) || orderOptions.OrderField == "IdPremises")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdPremises);
                else
                    return query.OrderByDescending(p => p.IdPremises);
            }
            if(orderOptions.OrderField == "Address")
            {
                var addresses = query.Select(p => new
                    {
                        p.IdPremises,
                        Address = string.Concat(p.IdBuildingNavigation.IdStreetNavigation.StreetName, ", ",
                            p.IdBuildingNavigation.House, ", ", p.PremisesNum)
                    });

                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                {
                    return from row in query
                            join addr in addresses
                             on row.IdPremises equals addr.IdPremises
                            orderby addr.Address
                            select row;
                }
                else
                {
                    return from row in query
                            join addr in addresses
                             on row.IdPremises equals addr.IdPremises
                            orderby addr.Address descending
                            select row;
                }
            }
            if (orderOptions.OrderField == "ObjectState")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdStateNavigation.StateNeutral);
                else
                    return query.OrderByDescending(p => p.IdStateNavigation.StateNeutral);
            }
            if (orderOptions.OrderField == "CurrentFund")
            {
                var maxFunds = from fundRow in registryContext.FundsHistory
                                   join fundPremisesRow in registryContext.FundsPremisesAssoc
                                   on fundRow.IdFund equals fundPremisesRow.IdFund
                                   where fundRow.ExcludeRestrictionDate == null
                                   group fundPremisesRow.IdFund by fundPremisesRow.IdPremises into gs
                                   select new
                                   {
                                       IdPremises = gs.Key,
                                       IdFund = gs.Max()
                                   };
                var funds = from fundRow in registryContext.FundsHistory
                            join maxFundRow in maxFunds
                            on fundRow.IdFund equals maxFundRow.IdFund
                            join fundTypeRow in registryContext.FundTypes
                            on fundRow.IdFundType equals fundTypeRow.IdFundType
                            select new
                            {
                                maxFundRow.IdPremises,
                                fundTypeRow.FundTypeName
                            };
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                {
                    return from row in query
                            join fund in funds
                             on row.IdPremises equals fund.IdPremises
                            orderby fund.FundTypeName
                            select row;
                }
                else
                {
                    return from row in query
                            join fund in funds
                             on row.IdPremises equals fund.IdPremises
                            orderby fund.FundTypeName descending
                            select row;
                }
            }
            return query;
        }

        public List<Premise> GetQueryPage(IQueryable<Premise> query, PageOptions pageOptions)
        {
            var result = query;
            result = result.Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage);
            result = result.Take(pageOptions.SizePage);
            return result.ToList();
        }

        public void Create(Premise premise, List<IFormFile> files, int? IdFundType)
        {
            if (IdFundType != null)
            {
                var fund = new FundHistory
                {
                    IdFundType = IdFundType.Value
                };

                var fpa = new FundPremiseAssoc
                {
                    IdFundNavigation = fund,
                    IdPremisesNavigation = premise
                };
                premise.FundsPremisesAssoc = new List<FundPremiseAssoc>
                {
                    fpa
                };
            }
            // Прикрепляем файлы реквизитов
            var restrictionFilePath = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Restrictions\");
            if (premise.RestrictionPremisesAssoc != null)
            {
                for (var i = 0; i < premise.RestrictionPremisesAssoc.Count; i ++)
                {
                    var file = files.Where(r => r.Name == "RestrictionFiles[" + i + "]").FirstOrDefault();
                    if (file == null) continue;
                    premise.RestrictionPremisesAssoc[i].RestrictionNavigation.FileDisplayName = file.FileName;
                    var fileOriginName = Guid.NewGuid().ToString() + "." + new FileInfo(file.FileName).Extension;
                    premise.RestrictionPremisesAssoc[i].RestrictionNavigation.FileOriginName = fileOriginName;
                    premise.RestrictionPremisesAssoc[i].RestrictionNavigation.FileMimeType = file.ContentType;
                    var fileStream = new FileStream(Path.Combine(restrictionFilePath, fileOriginName), FileMode.CreateNew);
                    file.OpenReadStream().CopyTo(fileStream);
                    fileStream.Close();
                }
            }
            // Прикрепляем файлы ограничений
            var ownershipRightsFilePath = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"OwnershipRights\");
            if (premise.OwnershipPremisesAssoc != null)
            {
                for (var i = 0; i < premise.OwnershipPremisesAssoc.Count; i++)
                {
                    var file = files.Where(r => r.Name == "OwnershipRightFiles[" + i + "]").FirstOrDefault();
                    if (file == null) continue;
                    premise.OwnershipPremisesAssoc[i].OwnershipRightNavigation.FileDisplayName = file.FileName;
                    var fileOriginName = Guid.NewGuid().ToString() + "." + new FileInfo(file.FileName).Extension;
                    premise.OwnershipPremisesAssoc[i].OwnershipRightNavigation.FileOriginName = fileOriginName;
                    premise.OwnershipPremisesAssoc[i].OwnershipRightNavigation.FileMimeType = file.ContentType;
                    var fileStream = new FileStream(Path.Combine(ownershipRightsFilePath, fileOriginName), FileMode.CreateNew);
                    file.OpenReadStream().CopyTo(fileStream);
                    fileStream.Close();
                }
            }
            // Прикрепляем файлы документов по переселению
            var resettleFilePath = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Resettles\");
            if (premise.ResettlePremisesAssoc != null)
            {
                for (var i = 0; i < premise.ResettlePremisesAssoc.Count; i++)
                {
                    var documents = premise.ResettlePremisesAssoc[i].ResettleInfoNavigation.ResettleDocuments;
                    if (documents == null) continue;
                    for (var j = 0; j < premise.ResettlePremisesAssoc[i].ResettleInfoNavigation.ResettleDocuments.Count; j++)
                    {
                        var file = files.Where(r => r.Name == "Premise.ResettlePremisesAssoc[" + i + "].ResettleDocumentFiles[" + j + "]").FirstOrDefault();
                        if (file == null) continue;
                        premise.ResettlePremisesAssoc[i].ResettleInfoNavigation.ResettleDocuments[j].FileDisplayName = file.FileName;
                        var fileOriginName = Guid.NewGuid().ToString() + "." + new FileInfo(file.FileName).Extension;
                        premise.ResettlePremisesAssoc[i].ResettleInfoNavigation.ResettleDocuments[j].FileOriginName = fileOriginName;
                        premise.ResettlePremisesAssoc[i].ResettleInfoNavigation.ResettleDocuments[j].FileMimeType = file.ContentType;
                        var fileStream = new FileStream(Path.Combine(resettleFilePath, fileOriginName), FileMode.CreateNew);
                        file.OpenReadStream().CopyTo(fileStream);
                        fileStream.Close();
                    }
                }
            }
            // Прикрепляем файлы судебных разбирательств
            var litigationFilePath = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Litigations\");
            if (premise.LitigationPremisesAssoc != null)
            {
                for (var i = 0; i < premise.LitigationPremisesAssoc.Count; i++)
                {
                    var file = files.Where(r => r.Name == "LitigationFiles[" + i + "]").FirstOrDefault();
                    if (file == null) continue;
                    premise.LitigationPremisesAssoc[i].LitigationNavigation.FileDisplayName = file.FileName;
                    var fileOriginName = Guid.NewGuid().ToString() + "." + new FileInfo(file.FileName).Extension;
                    premise.LitigationPremisesAssoc[i].LitigationNavigation.FileOriginName = fileOriginName;
                    premise.LitigationPremisesAssoc[i].LitigationNavigation.FileMimeType = file.ContentType;
                    var fileStream = new FileStream(Path.Combine(litigationFilePath, fileOriginName), FileMode.CreateNew);
                    file.OpenReadStream().CopyTo(fileStream);
                    fileStream.Close();
                }
            }
            registryContext.Premises.Add(premise);
            registryContext.SaveChanges();            
        }

        public Premise CreatePremise(int ?idBuilding)
        {
            var premise = new Premise {
                RegDate = new DateTime(1999, 10, 29),
                IdBuilding = idBuilding ?? 0,
                IdPremisesDoorKeys = 1,
                IdPremisesComment = 1,
                IdBuildingNavigation = idBuilding != null ? registryContext.Buildings.FirstOrDefault(b => b.IdBuilding == idBuilding.Value)  : null
            };
            premise.FundsPremisesAssoc = new List<FundPremiseAssoc>() { new FundPremiseAssoc() };
            premise.OwnerPremisesAssoc = new List<OwnerPremiseAssoc>() { new OwnerPremiseAssoc() };
            premise.TenancyPremisesAssoc = new List<TenancyPremiseAssoc>() { new TenancyPremiseAssoc() };

            var subpremise = new SubPremise();
            subpremise.FundsSubPremisesAssoc = new List<FundSubPremiseAssoc>() { new FundSubPremiseAssoc() };
            subpremise.OwnerSubPremisesAssoc = new List<OwnerSubPremiseAssoc>() { new OwnerSubPremiseAssoc() };
            subpremise.TenancySubPremisesAssoc = new List<TenancySubPremiseAssoc>() { new TenancySubPremiseAssoc() };
            premise.SubPremises = new List<SubPremise>() { subpremise };

            return premise;
        }

        public List<ObjectState> GetObjectStatesWithRights(string action, bool canEditBaseInfo)
        {
            var objectStates = ObjectStates.ToList();
            if ((action == "Create" || action == "Edit") && canEditBaseInfo)
            {
                objectStates = objectStates.Where(r => (
                securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && ObjectStateHelper.MunicipalIds().Contains(r.IdState) ||
                securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && !ObjectStateHelper.MunicipalIds().Contains(r.IdState))).ToList();
            }
            return objectStates;
        }

        public PremisesVM<Premise> GetPremiseView(Premise premise, [CallerMemberName]string action = "", bool canEditBaseInfo = false)
        {
            var objectStates = GetObjectStatesWithRights(action, canEditBaseInfo);
            var premisesVM = new PremisesVM<Premise>()
            {
                Premise = premise,
                KladrStreetsList = new SelectList(addressesDataService.KladrStreets, "IdStreet", "StreetName"),
                HeatingTypesList = new SelectList(HeatingTypes, "IdHeatingType", "IdHeatingType1"),
                StructureTypesList = new SelectList(StructureTypes, "IdStructureType", "StructureTypeName"),
                ObjectStatesList = new SelectList(objectStates, "IdState", "StateFemale"),
                PremisesTypesList = new SelectList(addressesDataService.PremisesTypes, "IdPremisesType", "PremisesTypeName"),
                FundTypesList = new SelectList(registryContext.FundTypes, "IdFundType", "FundTypeName"),
                LocationKeysList = new SelectList(registryContext.PremisesDoorKeys, "IdPremisesDoorKeys", "LocationOfKeys"),
                CommentList = new SelectList(registryContext.PremisesComments, "IdPremisesComment", "PremisesCommentText"),
                OwnershipRightTypesList = new SelectList(registryContext.OwnershipRightTypes, "IdOwnershipRightType", "OwnershipRightTypeName"),
                RestrictionsList = new SelectList(registryContext.RestrictionTypes, "IdRestrictionType", "RestrictionTypeName"),
                IdFundType = (from fhRow in registryContext.FundsHistory
                             join fpa in registryContext.FundsPremisesAssoc
                             on fhRow.IdFund equals fpa.IdFund
                             where fpa.IdPremises == premise.IdPremises && fhRow.ExcludeRestrictionDate == null
                             orderby fpa.IdFund descending
                             select fhRow.IdFundType).FirstOrDefault(),


                SignersList = new SelectList(registryContext.SelectableSigners.Where(s => s.IdSignerGroup == 1).ToList().Select(s => new {
                    s.IdRecord,
                    Snp = s.Surname + " " + s.Name + (s.Patronymic == null ? "" : " " + s.Patronymic)
                }), "IdRecord", "Snp"),
                PreparersList = new SelectList(registryContext.Preparers, "IdPreparer", "PreparerName")
            };

        if ((action == "Details" || action == "Delete") && securityService.HasPrivilege(Privileges.TenancyRead))
        {
            premisesVM.PaymentsInfo = GetPaymentInfo(new List<Premise> { premisesVM.Premise });
        }

        return premisesVM;
        }

        private List<PaymentsInfo> GetPaymentInfo(List<Premise> premises)
        {
            var ids = premises.Select(p => p.IdPremises).ToList();
            var paymentsInfo = new List<PaymentsInfo>();
            var payments = (from paymentRow in registryContext.TenancyPayments
                           where paymentRow.IdPremises != null && ids.Contains(paymentRow.IdPremises.Value)
                           select paymentRow).ToList();
            var tenanciesIds = payments.Select(p => p.IdProcess);

            var activeTenancies = (from tpRow in registryContext.TenancyProcesses
                                   join personRow in registryContext.TenancyPersons
                                   on tpRow.IdProcess equals personRow.IdProcess
                                    where tenanciesIds.Contains(tpRow.IdProcess) && (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н"))
                                       select tpRow.IdProcess).Distinct().ToList();

            var prePaymentsAfter28082019Premises = from tpaRow in registryContext.TenancyPremisesAssoc
                                                    join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                    on tpaRow.IdPremise equals paymentRow.IdPremises
                                                    where paymentRow.IdSubPremises == null &&
                                                          paymentRow.IdPremises != null && ids.Contains(paymentRow.IdPremises.Value)
                                                    select new
                                                    {
                                                        tpaRow.IdProcess,
                                                        paymentRow.IdPremises,
                                                        paymentRow.Hb,
                                                        paymentRow.K1,
                                                        paymentRow.K2,
                                                        paymentRow.K3,
                                                        paymentRow.KC,
                                                        paymentRow.RentArea
                                                    };

            var paymentsAfter28082019Premises = (from paymentRow in prePaymentsAfter28082019Premises
                                                 join tpRow in registryContext.TenancyProcesses.Include(tp => tp.TenancyPersons)
                                                     on paymentRow.IdProcess equals tpRow.IdProcess
                                                 where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                   tpRow.TenancyPersons.Any()
                                                 select paymentRow).Distinct().ToList();

            var prePaymentsAfter28082019SubPremises = from tspaRow in registryContext.TenancySubPremisesAssoc
                                                       join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                    on tspaRow.IdSubPremise equals paymentRow.IdSubPremises
                                                       where paymentRow.IdPremises != null && ids.Contains(paymentRow.IdPremises.Value)
                                                       select new
                                                       {
                                                           tspaRow.IdProcess,
                                                           paymentRow.IdSubPremises,
                                                           paymentRow.Hb,
                                                           paymentRow.K1,
                                                           paymentRow.K2,
                                                           paymentRow.K3,
                                                           paymentRow.KC,
                                                           paymentRow.RentArea
                                                       };

            var paymentsAfter28082019SubPremises = (from paymentRow in prePaymentsAfter28082019SubPremises
                                                    join tpRow in registryContext.TenancyProcesses.Include(tp => tp.TenancyPersons)
                                                        on paymentRow.IdProcess equals tpRow.IdProcess
                                                    where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                      tpRow.TenancyPersons.Any()
                                                    select paymentRow).Distinct().ToList();

            foreach(var payment in payments)
            {
                if (payment.IdPremises == null) continue;
                var addreType = payment.IdSubPremises == null ? AddressTypes.Premise : AddressTypes.SubPremise;
                var id = payment.IdSubPremises == null ? payment.IdPremises.Value : payment.IdSubPremises.Value;
                if (!activeTenancies.Contains(payment.IdProcess)) continue;
                paymentsInfo.Add(new PaymentsInfo {
                    IdObject = id,
                    AddresType = addreType,
                    Payment = payment.Payment
                });
            }
            foreach (var payment in paymentsAfter28082019Premises)
            {
                if (payment.IdPremises == null) continue;
                var paymentItem = paymentsInfo.FirstOrDefault(p => p.IdObject == payment.IdPremises && p.AddresType == AddressTypes.Premise);
                if (paymentItem == null)
                {
                    paymentItem = new PaymentsInfo {
                        IdObject = payment.IdPremises.Value,
                        AddresType = AddressTypes.Premise
                    };
                }
                paymentItem.Nb = payment.Hb;
                paymentItem.KC = payment.KC;
                paymentItem.K1 = payment.K1;
                paymentItem.K2 = payment.K2;
                paymentItem.K3 = payment.K3;
                paymentItem.PaymentAfter28082019 = Math.Round((payment.K1 + payment.K2 + payment.K3) / 3 * payment.KC * payment.Hb * (decimal)payment.RentArea, 2);
            }
            foreach (var payment in paymentsAfter28082019SubPremises)
            {
                if (payment.IdSubPremises == null) continue;
                var paymentItem = paymentsInfo.FirstOrDefault(p => p.IdObject == payment.IdSubPremises && p.AddresType == AddressTypes.SubPremise);
                if (paymentItem == null)
                {
                    paymentItem = new PaymentsInfo
                    {
                        IdObject = payment.IdSubPremises.Value,
                        AddresType = AddressTypes.SubPremise
                    };
                }
                paymentItem.Nb = payment.Hb;
                paymentItem.KC = payment.KC;
                paymentItem.K1 = payment.K1;
                paymentItem.K2 = payment.K2;
                paymentItem.K3 = payment.K3;
                paymentItem.PaymentAfter28082019 = Math.Round((payment.K1 + payment.K2 + payment.K3) / 3 * payment.KC * payment.Hb * (decimal)payment.RentArea, 2);
            }
            return paymentsInfo;
        }

        private List<PremiseOwnershipRightCurrent> GetPremisesOwnershipRightCurrent(List<Premise> premises)
        {
            var ids = premises.Select(p => p.IdPremises).ToList();
            return registryContext.PremisesOwnershipRightCurrent
                .Where(p => ids.Contains(p.IdPremises))
                .Select(p => new PremiseOwnershipRightCurrent { IdPremises = p.IdPremises, IdOwnershipRightType = p.IdOwnershipRightType })
                .ToList();            
        }

        public void Edit(Premise premise)
        {
            registryContext.Premises.Update(premise);
            registryContext.SaveChanges();
        }

        public void Delete(int idPremise)
        {
            var premise = registryContext.Premises
                .FirstOrDefault(op => op.IdPremises == idPremise);
            if (premise != null)
            {
                premise.Deleted = 1;
                registryContext.SaveChanges();
            }
        }

        public OwnerType GetOwnerType(int idOwnerType)
            => registryContext.OwnerType.FirstOrDefault(ot => ot.IdOwnerType == idOwnerType);


        public Premise GetPremise(int idPremise)
        {
            return registryContext.Premises.AsNoTracking()
                .Include(b => b.IdBuildingNavigation).ThenInclude(b => b.IdStreetNavigation)
                .Include(b => b.IdBuildingNavigation.IdHeatingTypeNavigation)
                .Include(b => b.IdStateNavigation)                              //Текущее состояние объекта
                .Include(b => b.IdBuildingNavigation.IdStructureTypeNavigation) //Тип помещения: квартира, комната, квартира с подселением
                .Include(b => b.FundsPremisesAssoc).ThenInclude(fpa => fpa.IdFundNavigation).ThenInclude(fh => fh.IdFundTypeNavigation)
                .Include(b => b.IdPremisesCommentNavigation)
                .Include(b => b.IdPremisesTypeNavigation)
                .Include(b => b.IdPremisesDoorKeysNavigation)
                .Include(b => b.SubPremises)
                .SingleOrDefault(b => b.IdPremises == idPremise);
        }

        public IQueryable<Premise> GetPremisesForMassReports(List<int> ids)
        {
            return registryContext.Premises
                .Include(b => b.IdBuildingNavigation).ThenInclude(b => b.IdStreetNavigation)
                .Where(b => ids.Contains(b.IdPremises));
        }

        public PremisesVM<Premise> GetPremisesViewModelForMassReports(List<int> ids, PageOptions pageOptions, bool canEditBaseInfo)
        {
            var viewModel = InitializeViewModel(null, pageOptions, null);
            viewModel.ObjectStatesList = new SelectList(GetObjectStatesWithRights("Edit", canEditBaseInfo), "IdState", "StateFemale");
            viewModel.CommisionList = viewModel.SignersList;
            var premises = GetPremisesForMassReports(ids);
            var count = premises.Count();
            viewModel.PageOptions.TotalRows = count;
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.Premises = GetQueryPage(premises, viewModel.PageOptions).ToList();
            return viewModel;
        }

        public IEnumerable<ObjectState> ObjectStates
        {
            get => registryContext.ObjectStates.AsNoTracking();
        }

        public IEnumerable<StructureType> StructureTypes
        {
            get => registryContext.StructureTypes.AsNoTracking();
        }

        public IEnumerable<HeatingType> HeatingTypes
        {
            get => registryContext.HeatingTypes.AsNoTracking();
        }

        public void UpdateInfomationInPremises(List<Premise> premises, string description, DateTime? regDate, int? idState)
        {
            foreach (Premise premise in premises)
            {
                if (!string.IsNullOrEmpty(description))
                {
                    if (string.IsNullOrEmpty(premise.Description))
                        premise.Description = description;
                    else premise.Description += "\n" + description;
                }

                if (regDate != null)
                    premise.RegDate = regDate.Value;
                if (idState != null)
                    premise.IdState = idState.Value;

                registryContext.Premises.Update(premise);
            }
            registryContext.SaveChanges();
        }
    }
}
