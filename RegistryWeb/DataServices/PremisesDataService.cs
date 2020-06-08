using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Runtime.CompilerServices;

namespace RegistryWeb.DataServices
{
    public class PremisesDataService : ListDataService<PremisesVM<Premise>, PremisesListFilter>
    {
        private BuildingsDataService BuildingsDataService;
        public PremisesDataService(RegistryContext rc, BuildingsDataService BuildingsDataService) : base(rc)
        {
            this.BuildingsDataService = BuildingsDataService;
        }

        public override PremisesVM<Premise> InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, PremisesListFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.KladrStreetsList = new SelectList(KladrStreets, "IdStreet", "StreetName");            
            viewModel.PremisesTypeAsNum = new SelectList(registryContext.PremisesTypes, "IdPremisesType", "PremisesTypeAsNum");
            viewModel.HeatingTypesList = new SelectList(HeatingTypes, "IdHeatingType", "IdHeatingType1");
            viewModel.StructureTypesList = new SelectList(StructureTypes, "IdStructureType", "StructureTypeName");
            viewModel.ObjectStatesList = new SelectList(ObjectStates, "IdState", "StateFemale");
            viewModel.PremisesTypesList = new SelectList(registryContext.PremisesTypes, "IdPremisesType", "PremisesTypeName");
            viewModel.FundTypesList = new SelectList(registryContext.FundTypes, "IdFundType", "FundTypeName");
            viewModel.OwnershipRightTypesList = new SelectList(registryContext.OwnershipRightTypes, "IdOwnershipRightType", "OwnershipRightTypeName");
            viewModel.RestrictionsList = new SelectList(registryContext.RestrictionTypes, "IdRestrictionType", "RestrictionTypeName");
            viewModel.LocationKeysList = new SelectList(registryContext.PremisesDoorKeys, "IdPremisesDoorKeys", "LocationOfKeys");
            viewModel.CommentList = new SelectList(registryContext.PremisesComments, "IdPremisesComment", "PremisesCommentText");
            return viewModel;
        }

        public PremisesVM<Premise> GetViewModel(
            OrderOptions orderOptions,            
            PageOptions pageOptions,
            PremisesListFilter filterOptions)
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
            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.Premises = GetQueryPage(query, viewModel.PageOptions);
            return viewModel;
        }

        private IQueryable<Premise> GetQueryIncludes(IQueryable<Premise> query)
        {
            return query
                .Include(p => p.IdBuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(p => p.IdStateNavigation)        //Текущее состояние объекта
                .Include(p => p.IdRentPremiseNavigation)
                .Include(p => p.IdPremisesTypeNavigation) //Тип помещения: квартира, комната, квартира с подселением
                .Include(p => p.FundsPremisesAssoc)
                    .ThenInclude(fpa => fpa.IdFundNavigation)
                        .ThenInclude(fh => fh.IdFundTypeNavigation);
        }

        public IQueryable<Premise> GetQuery()
        {
            return GetQueryIncludes(registryContext.Premises);           
        }

        private IQueryable<Premise> GetQueryFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (!filterOptions.IsAddressEmpty())
            {
                query = AddressFilter(query, filterOptions);
            }
            if (filterOptions.IdPremise.HasValue)
            {
                query = query.Where(p => p.IdPremises == filterOptions.IdPremise.Value);
            }
            if (!string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                query = query.Where(b => b.IdBuildingNavigation.IdStreet == filterOptions.IdStreet);
            }
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                query = query.Where(b => b.IdBuildingNavigation.House.ToLower() == filterOptions.House.ToLower());
            }
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                query = query.Where(b => b.PremisesNum.ToLower() == filterOptions.PremisesNum.ToLower());
            }
            if (filterOptions.Floors.HasValue)
            {
                query = query.Where(b => b.Floor == filterOptions.Floors.Value);
            }
            if (!string.IsNullOrEmpty(filterOptions.CadastralNum))
            {
                query = query.Where(b => b.CadastralNum == filterOptions.CadastralNum);
            }
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
                                 where filterOptions.IdFundType.Contains(fundRow.IdFundType)
                                 select maxFundRow.IdPremises;
                query = from row in query
                        join idPremise in idPremises
                        on row.IdPremises equals idPremise
                        select row;
            }
            if (filterOptions.IdsObjectState != null && filterOptions.IdsObjectState.Any())
            {
                query = query.Where(p => filterOptions.IdsObjectState.Contains(p.IdState));
            }
            if (filterOptions.IdsComment != null && filterOptions.IdsComment.Any())
            {
                query = query.Where(p => filterOptions.IdsComment.Contains(p.IdPremisesComment));
            }
            if (filterOptions.IdsDoorKeys != null && filterOptions.IdsDoorKeys.Any())
            {
                query = query.Where(p => filterOptions.IdsDoorKeys.Contains(p.IdPremisesDoorKeys));
            }

            if ((filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Any()) ||
                !string.IsNullOrEmpty(filterOptions.NumberOwnershipRight) || filterOptions.DateOwnershipRight != null)
            {
                // TODO: Упрощенная фильтрация без учета исключения из аварийного
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
                            ((filterOptions.IdsOwnershipRightType == null || !filterOptions.IdsOwnershipRightType.Any() ||
                            filterOptions.IdsOwnershipRightType.Contains(borRow.IdOwnershipRightType)) &&
                            (string.IsNullOrEmpty(filterOptions.NumberOwnershipRight) ||
                             borRow.Number.ToLower() == filterOptions.NumberOwnershipRight.ToLower()) &&
                             (filterOptions.DateOwnershipRight == null || borRow.Date == filterOptions.DateOwnershipRight))) ||
                             (porRow != null &&
                            ((filterOptions.IdsOwnershipRightType == null || !filterOptions.IdsOwnershipRightType.Any() ||
                            filterOptions.IdsOwnershipRightType.Contains(porRow.IdOwnershipRightType)) &&
                            (string.IsNullOrEmpty(filterOptions.NumberOwnershipRight) ||
                             porRow.Number.ToLower() == filterOptions.NumberOwnershipRight.ToLower()) &&
                             (filterOptions.DateOwnershipRight == null || porRow.Date == filterOptions.DateOwnershipRight)))
                         select q).Distinct();
            }

            if ((filterOptions.IdsRestrictionType != null && filterOptions.IdsRestrictionType.Any()) ||
                !string.IsNullOrEmpty(filterOptions.RestrictionNum) || filterOptions.RestrictionDate != null)
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
                            ((filterOptions.IdsRestrictionType == null || !filterOptions.IdsRestrictionType.Any() ||
                            filterOptions.IdsRestrictionType.Contains(borRow.IdRestrictionType)) &&
                            (string.IsNullOrEmpty(filterOptions.RestrictionNum) ||
                             borRow.Number.ToLower() == filterOptions.RestrictionNum.ToLower()) &&
                             (filterOptions.RestrictionDate == null || borRow.Date == filterOptions.RestrictionDate))) ||
                             (porRow != null &&
                            ((filterOptions.IdsRestrictionType == null || !filterOptions.IdsRestrictionType.Any() ||
                            filterOptions.IdsRestrictionType.Contains(porRow.IdRestrictionType)) &&
                            (string.IsNullOrEmpty(filterOptions.RestrictionNum) ||
                             porRow.Number.ToLower() == filterOptions.RestrictionNum.ToLower()) &&
                             (filterOptions.RestrictionDate == null || porRow.Date == filterOptions.RestrictionDate)))
                         select q).Distinct();
            }

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

        private IQueryable<Premise> AddressFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return query;


            if (filterOptions.Address.AddressType == AddressTypes.Street)            
                return query.Where(q => q.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id));

            int id = 0;
            if (!int.TryParse(filterOptions.Address.Id, out id))
                return query;

            if (filterOptions.Address.AddressType == AddressTypes.Building)            
                return query.Where(q => q.IdBuilding == id);
            
            if (filterOptions.Address.AddressType == AddressTypes.Premise)            
                return query.Where(q => q.IdPremises == id);
            
            if (filterOptions.Address.AddressType == AddressTypes.SubPremise)
            {
                return from q in query

                       join sp in registryContext.SubPremises
                       on q.IdPremises equals sp.IdPremises
                       where sp.IdSubPremises == id
                       select q;
            }
            return query;
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

        internal void Create(Premise premise, int IdFundType)
        {
            premise.IdBuildingNavigation = null;
            premise.IdPremisesCommentNavigation = null;
            premise.IdPremisesDoorKeysNavigation = null;
            premise.IdPremisesKindNavigation = null;
            premise.IdPremisesTypeNavigation = null;
            registryContext.Premises.Add(premise);

            /*
            //добавление записей в FundHistory и FundPremiseAssoc
            var funfh = new FundHistory
            {
                IdFundType = IdFundType
            };
            registryContext.FundsHistory.Add(funfh);

            var fpa = new FundPremiseAssoc
            {
                IdFund= funfh.IdFund,
                IdPremises=premise.IdPremises
            };
            registryContext.FundsPremisesAssoc.Add(fpa);
            */

            registryContext.SaveChanges();            
        }

        internal Premise CreatePremise()
        {
            var premise = new Premise();
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


        public PremisesVM<Premise> GetPremiseView(Premise premise, [CallerMemberName]string action = "")
        {
            //ViewBag.Action = action;
            var premisesVM = new PremisesVM<Premise>()
            {
                Premise = premise,
                KladrStreetsList = new SelectList(KladrStreets, "IdStreet", "StreetName"),
                HeatingTypesList = new SelectList(HeatingTypes, "IdHeatingType", "IdHeatingType1"),
                StructureTypesList = new SelectList(StructureTypes, "IdStructureType", "StructureTypeName"),
                ObjectStatesList = new SelectList(ObjectStates, "IdState", "StateFemale"),
                PremisesTypesList = new SelectList(registryContext.PremisesTypes, "IdPremisesType", "PremisesTypeName"),
                PremisesTypeAsNum = new SelectList(registryContext.PremisesTypes, "IdPremisesType", "PremisesTypeAsNum"),
                FundTypesList = new SelectList(registryContext.FundTypes, "IdFundType", "FundTypeName"),
                LocationKeysList = new SelectList(registryContext.PremisesDoorKeys, "IdPremisesDoorKeys", "LocationOfKeys"),
                CommentList = new SelectList(registryContext.PremisesComments, "IdPremisesComment", "PremisesCommentText"),
                OwnershipRightTypesList = new SelectList(registryContext.OwnershipRightTypes, "IdOwnershipRightType", "OwnershipRightTypeName"),
                RestrictionsList = new SelectList(registryContext.RestrictionTypes, "IdRestrictionType", "RestrictionTypeName"),
                Payment = registryContext.RentPremises.FirstOrDefault(rp => rp.IdPremises == premise.IdPremises)?.Payment ?? 0,
                PaymentInfo = new PremisesPaymentInfo(),
            };

            if (action == "Details")
            {
                premisesVM.Premise.IdPaymentNavigation = new PremisesPaymentInfo();
                premisesVM.Premise.IdPaymentNavigation.payment = premisesVM.Payment;
                premisesVM.Premise.IdPaymentNavigation.CPc = registryContext.TotalAreaAvgCosts.ToList()[0].Cost;
                premisesVM.Premise.IdPaymentNavigation.Hb = premisesVM.Premise.IdPaymentNavigation.CPc * 0.001;
                premisesVM.Premise.IdPaymentNavigation.Kc = 0.18;
                premisesVM.Premise.IdPaymentNavigation.K1 = ((premise.IdBuildingNavigation.IdStructureType == 1 ? 1.3 : 0.8) + (new int?[] { 4, 9 }.Contains(premise.IdBuildingNavigation.IdStructureType) ? 1 : premise.IdBuildingNavigation.IdStructureType == 3 ? 0.9 : new int?[] { 2, 5, 6, 8 }.Contains(premise.IdBuildingNavigation.IdStructureType) ? 0.8 : 0)) / 2;
                premisesVM.Premise.IdPaymentNavigation.K2 = premise.IdBuildingNavigation.HotWaterSupply.Value && premise.IdBuildingNavigation.Plumbing.Value && premise.IdBuildingNavigation.Canalization.Value && premise.IdPremisesType == 1 ? 1.3 : premise.IdBuildingNavigation.Plumbing.Value && premise.IdBuildingNavigation.Canalization.Value && premise.IdPremisesType == 1 ? 1 : 0.8;
                premisesVM.Premise.IdPaymentNavigation.K3 = premise.IdBuildingNavigation.IdStreet.StartsWith("380000050410") || premise.IdBuildingNavigation.IdStreet.StartsWith("380000050230") || premise.IdBuildingNavigation.IdStreet.StartsWith("380000050180") ? 1 : premise.IdBuildingNavigation.IdStreet.StartsWith("380000050130") ? 0.9 : 0.8;
                premisesVM.Premise.IdPaymentNavigation.rent = (premisesVM.Premise.IdPaymentNavigation.K1 + premisesVM.Premise.IdPaymentNavigation.K2 + premisesVM.Premise.IdPaymentNavigation.K3) / 3 * premisesVM.Premise.IdPaymentNavigation.Hb * premisesVM.Premise.IdPaymentNavigation.Kc * registryContext.RentObjectsAreaAndCategories.SingleOrDefault(r => r.IdPremises == premisesVM.Premise.IdPremises).RentArea;
            }

            return premisesVM;
            //return View("Premise", premisesVM);
        }

        internal void Edit(Premise premise)
        {
            var oldPremise = GetPremise(premise.IdPremises);

            foreach (var fpa in oldPremise.FundsPremisesAssoc)
            {
                if (premise.FundsPremisesAssoc.Select(owba => owba.IdPremises).Contains(fpa.IdPremises) == false)
                {
                    fpa.Deleted = 1;
                    premise.FundsPremisesAssoc.Add(fpa);
                }
            }
            foreach (var oba in oldPremise.OwnershipPremisesAssoc)
            {
                if (premise.OwnershipPremisesAssoc.Select(owba => owba.IdOwnershipRight).Contains(oba.IdOwnershipRight) == false)
                {
                    oba.Deleted = 1;
                    premise.OwnershipPremisesAssoc.Add(oba);
                }
            }
            foreach (var opa in oldPremise.OwnerPremisesAssoc)
            {
                if (premise.OwnerPremisesAssoc.Select(owpa => owpa.IdAssoc).Contains(opa.IdAssoc) == false)
                {
                    opa.Deleted = 1;
                    premise.OwnerPremisesAssoc.Add(opa);
                }
            }
            foreach (var ospa in oldPremise.OwnerPremisesAssoc)
            {
                if (premise.OwnerPremisesAssoc.Select(owspa => owspa.IdAssoc).Contains(ospa.IdAssoc) == false)
                {
                    ospa.Deleted = 1;
                    premise.OwnerPremisesAssoc.Add(ospa);
                }
            }
            foreach (var ospa in oldPremise.SubPremises)
            {
                if (premise.SubPremises.Select(owspa => owspa.IdPremises).Contains(ospa.IdPremises) == false)
                {
                    ospa.Deleted = 1;
                    premise.SubPremises.Add(ospa);
                }
            }
            foreach (var tpa in oldPremise.TenancyPremisesAssoc)
            {
                if (premise.TenancyPremisesAssoc.Select(owspa => owspa.IdPremise).Contains(tpa.IdPremise) == false)
                {
                    tpa.Deleted = 1;
                    premise.TenancyPremisesAssoc.Add(tpa);
                }
            }

            premise.IdBuildingNavigation = null;
            premise.IdPremisesCommentNavigation = null;
            premise.IdPremisesDoorKeysNavigation = null;
            premise.IdPremisesKindNavigation = null;
            premise.IdPremisesTypeNavigation = null;

            //Добавление и радактирование
            registryContext.Premises.Update(premise);
            registryContext.SaveChanges();
        }

        internal void Delete(int idPremise)
        {
            var oldPremise = GetPremise(idPremise);

            oldPremise.Deleted = 1;
            foreach (var fpa in oldPremise.FundsPremisesAssoc)
            {
                fpa.Deleted = 1;
            }
            foreach (var opa in oldPremise.OwnerPremisesAssoc)
            {
                opa.Deleted = 1;
            }
            foreach (var ospa in oldPremise.OwnershipPremisesAssoc)
            {
                ospa.Deleted = 1;
            }
            foreach (var sp in oldPremise.SubPremises)
            {
                sp.Deleted = 1;
            }
            foreach (var tnpa in oldPremise.TenancyPremisesAssoc)
            {
                tnpa.Deleted = 1;
            }
            registryContext.SaveChanges();
        }

        internal OwnerType GetOwnerType(int idOwnerType)
            => registryContext.OwnerType.FirstOrDefault(ot => ot.IdOwnerType == idOwnerType);


        public Premise GetPremise(int idPremise)
        {

            /*var query = GetQuery();
            return query
                .Include(b => b.IdBuildingNavigation).ThenInclude(b => b.IdStreetNavigation)
                .Include(b => b.IdBuildingNavigation.IdHeatingTypeNavigation)
                .Include(b => b.IdStateNavigation)                              //Текущее состояние объекта
                .Include(b => b.IdBuildingNavigation.IdStructureTypeNavigation) //Тип помещения: квартира, комната, квартира с подселением
                .Include(b => b.FundsPremisesAssoc).ThenInclude(fpa => fpa.IdFundNavigation).ThenInclude(fh => fh.IdFundTypeNavigation)
                .Include(b => b.IdPremisesCommentNavigation).ThenInclude(fpa => fpa.Premises)
                .Include(b => b.IdPremisesTypeNavigation).ThenInclude(fpa => fpa.Premises)
                .Include(b => b.IdPremisesDoorKeysNavigation)
                .SingleOrDefault(b => b.IdPremises == idPremise);

            Where(p=>p.IdPremises== idPremise)*/
            return registryContext.Premises.AsNoTracking()
                .Include(b => b.IdBuildingNavigation).ThenInclude(b => b.IdStreetNavigation)
                .Include(b => b.IdBuildingNavigation.IdHeatingTypeNavigation)
                .Include(b => b.IdStateNavigation)                              //Текущее состояние объекта
                .Include(b => b.IdBuildingNavigation.IdStructureTypeNavigation) //Тип помещения: квартира, комната, квартира с подселением
                .Include(b => b.FundsPremisesAssoc).ThenInclude(fpa => fpa.IdFundNavigation).ThenInclude(fh => fh.IdFundTypeNavigation)
                .Include(b => b.IdPremisesCommentNavigation)
                .Include(b => b.IdPremisesTypeNavigation)
                .Include(b => b.IdPremisesDoorKeysNavigation)
                .SingleOrDefault(b => b.IdPremises == idPremise);
        }

        public IEnumerable<ObjectState> ObjectStates
        {
            get => registryContext.ObjectStates.AsNoTracking();
        }

        public IEnumerable<StructureType> StructureTypes
        {
            get => registryContext.StructureTypes.AsNoTracking();
        }

        public IEnumerable<KladrStreet> KladrStreets
        {
            get => registryContext.KladrStreets.AsNoTracking();
        }

        public IEnumerable<HeatingType> HeatingTypes
        {
            get => registryContext.HeatingTypes.AsNoTracking();
        }



        /*public IEnumerable<Premise> GetPremises(List<int> ids)
        {
            return registryContext.Premises
                .Include(b => b.IdBuildingNavigation)
                .Include(b => b.IdStateNavigation)                              //Текущее состояние объекта
                .Include(b => b.IdBuildingNavigation.IdStructureTypeNavigation) //Тип помещения: квартира, комната, квартира с подселением
                .Include(b => b.FundsPremisesAssoc).ThenInclude(fpa => fpa.IdFundNavigation).ThenInclude(fh => fh.IdFundTypeNavigation)
                .Include(b => b.IdPremisesCommentNavigation)
                .Include(b => b.IdPremisesTypeNavigation)
                .Include(b => b.IdPremisesDoorKeysNavigation)
                //.Include(b => b.OwnershipPremisesAssoc)
                .Where(b => ids.Contains(b.IdBuilding));
        }*/

        private IQueryable<Premise> GetQueryOrderMask(IQueryable<Premise> query, bool compare, OrderOptions orderOptions,
            Expression<Func<Premise, int>> expression)
        {
            //query = GetQueryOrderMask(
            //    query,
            //    (string.IsNullOrEmpty(orderOptions.OrderField) || orderOptions.OrderField == "IdPremises"),
            //    orderOptions,
            //    p => p.IdPremises
            //    );
            if (compare)
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    query = query.OrderBy(expression);
                else
                    query = query.OrderByDescending(expression);
            }
            return query;
        }

        public List<Building> GetHouses(string streetId)
        {

            var house = from bui in registryContext.Buildings
                        where bui.IdStreet.Contains(streetId)
                        select bui;
            return house.ToList();
        }
    }
}
