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
            /*viewModel.PremisesTypes = registryContext.PremisesTypes;
            viewModel.ObjectStates = registryContext.ObjectStates;
            viewModel.FundTypes = registryContext.FundTypes;*/
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
            //viewModel.RentList = new SelectList(registryContext.RentPremises, "IdPremises", "Payment");
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
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.Premises = GetQueryPage(query, viewModel.PageOptions);
            return viewModel;
        }



        public IQueryable<Premise> GetQuery()
        {

            /*var fundsHistory = registryContext.FundsPremisesAssoc.Select(fpa => fpa.IdFundNavigation);            
            var fundHistory = fundsHistory
                .FirstOrDefault(fh => fh.ExcludeRestrictionDate == null && fh.IdFund == fundsHistory.Max(f => f.IdFund));
                

            var t = registryContext.Premises
            .Include(p => p.IdBuildingNavigation)
                .ThenInclude(b => b.IdStreetNavigation)
            .Include(p => p.IdStateNavigation)        //Текущее состояние объекта
            .Include(p => p.IdPremisesTypeNavigation) //Тип помещения: квартира, комната, квартира с подселением
            .Include(p => p.FundsPremisesAssoc)
                .ThenInclude(fpa => fpa.IdFundNavigation)
                    .ThenInclude(fh => fh.IdFundTypeNavigation);

            var prim = from prem in registryContext.Premises
                       join fpa in registryContext.FundsPremisesAssoc
                       on prem.IdPremises equals fpa.IdPremises
                       join fph in registryContext.FundsHistory
                       on fpa.IdFund equals fph.IdFund
                       join fpt in registryContext.FundTypes
                       on fph.IdFundType equals fpt.IdFundType
                       select new PremiseWithFunType
                       {
                           Account=prem.Account,
                           BalanceCost=prem.BalanceCost,
                           CadastralCost=prem.CadastralCost,
                           CadastralNum=prem.CadastralNum,
                           Deleted=prem.Deleted,
                           Description=prem.Description,
                           Floor=prem.Floor,
                           FundsPremisesAssoc=prem.FundsPremisesAssoc,
                           FundType = fpa.IdFundNavigation.IdFundTypeNavigation.FundTypeName,
                           Height=prem.Height,
                           IdBuilding=prem.IdBuilding,
                           IdBuildingNavigation=prem.IdBuildingNavigation,
                           IdPremises=prem.IdPremises,
                           IdPremisesComment=prem.IdPremisesComment,
                           IdPremisesCommentNavigation=prem.IdPremisesCommentNavigation,
                           IdPremisesDoorKeys=prem.IdPremisesDoorKeys,
                           IdPremisesDoorKeysNavigation=prem.IdPremisesDoorKeysNavigation,
                           IdPremisesKind=prem.IdPremisesKind,
                           IdPremisesKindNavigation=prem.IdPremisesKindNavigation,
                           IdPremisesType=prem.IdPremisesType,
                           IdPremisesTypeNavigation=prem.IdPremisesTypeNavigation,
                           IdState=prem.IdState,
                           IdStateNavigation=prem.IdStateNavigation,
                           IsMemorial=prem.IsMemorial,
                           LivingArea=prem.LivingArea,
                           NumBeds=prem.NumBeds,
                           NumRooms=prem.NumRooms,
                           OwnerPremisesAssoc=prem.OwnerPremisesAssoc,
                           OwnershipPremisesAssoc=prem.OwnershipPremisesAssoc,
                           PremisesNum=prem.PremisesNum,
                           RegDate=prem.RegDate,
                           RestrictionPremisesAssoc=prem.RestrictionPremisesAssoc,
                           StateDate=prem.StateDate,
                           SubPremises=prem.SubPremises,
                           TenancyPremisesAssoc=prem.TenancyPremisesAssoc,
                           TotalArea=prem.TotalArea

                       };*/
            //return prim.Where(p=>p.Deleted!=1);

            /*var pr = prim.ToList();

            var fundsHistory = pr[1].FundsPremisesAssoc.Select(fpa => fpa.IdFundNavigation);
            var fundHistory = fundsHistory
                .FirstOrDefault(fh => fh.ExcludeRestrictionDate == null && fh.IdFund == fundsHistory.Max(f => f.IdFund));

            pr[1].FundType = fundHistory.IdFundTypeNavigation.FundTypeName;*/
            /*
                        return prim
                            .Include(p => p.IdBuildingNavigation)
                                .ThenInclude(b => b.IdStreetNavigation)
                            .Include(p => p.IdStateNavigation)        //Текущее состояние объекта
                            .Include(p => p.IdPremisesTypeNavigation) //Тип помещения: квартира, комната, квартира с подселением
                            .Include(p => p.FundsPremisesAssoc)
                                .ThenInclude(fpa => fpa.IdFundNavigation)
                                    .ThenInclude(fh => fh.IdFundTypeNavigation);
                                */
            return registryContext.Premises
                .Include(p => p.IdBuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(p => p.IdStateNavigation)        //Текущее состояние объекта
                .Include(p => p.IdRentPremiseNavigation)
                //.Include(p => p.IdRentPaymentNavigation)
                .Include(p => p.IdPremisesTypeNavigation) //Тип помещения: квартира, комната, квартира с подселением
                .Include(p => p.FundsPremisesAssoc)
                    .ThenInclude(fpa => fpa.IdFundNavigation)
                        .ThenInclude(fh => fh.IdFundTypeNavigation);           
        }

        //private IQueryable GetFundTypes()
        //{
        //    return 
        //        from pCurFund in
        //            from fpa in rc.FundsPremisesAssoc
        //            join fh in rc.FundsHistory on fpa.IdFund equals fh.IdFund
        //            where fh.ExcludeRestrictionDate == null
        //            group fpa by fpa.IdPremises into gr
        //            select new
        //            {
        //                IdPremises = gr.Key,
        //                IdFund = gr.Max(s => s.IdFund)
        //            }
        //        join fh in rc.FundsHistory
        //            on pCurFund.IdFund equals fh.IdFund
        //        join ft in rc.FundTypes
        //            on fh.IdFundType equals ft.IdFundType
        //        select new
        //        {
        //            pCurFund.IdPremises,
        //            fh.IdFund,
        //            fh.IdFundType,
        //            ft.FundType
        //        };
        //}

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
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                query = query.Where(b => b.IdBuildingNavigation.House.ToLower() == filterOptions.House.ToLower());
            }
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                query = query.Where(b => b.PremisesNum == filterOptions.PremisesNum);
            }
            if (filterOptions.Floors.HasValue)
            {
                query = query.Where(b => b.Floor == filterOptions.Floors.Value);
            }
            if (!string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                query = query.Where(b => b.IdBuildingNavigation.IdStreet == filterOptions.IdStreet);
            }
            if (!string.IsNullOrEmpty(filterOptions.CadastralNum))
            {
                query = query.Where(b => b.CadastralNum == filterOptions.CadastralNum);
            }
            if (filterOptions.IdFundType.HasValue)
            {
                var idPremises = registryContext.FundsPremisesAssoc
                    .Include(fpa => fpa.IdPremisesNavigation)
                    .Include(fpa => fpa.IdFundNavigation)
                        .ThenInclude(fh => fh.IdFundTypeNavigation)
                    .Where(fpa =>
                        fpa.IdFundNavigation.ExcludeRestrictionDate == null &&
                        fpa.IdFundNavigation.IdFundType == filterOptions.IdFundType.Value)
                    .Select(fpa => fpa.IdPremises);

                query = from row in query
                        join idPremise in idPremises
                        on row.IdPremises equals idPremise
                        select row;

            }
            /*if (!string.IsNullOrEmpty(filterOptions.RestrictionNum))
            {
                query = query.Where(b => b.RestrictionPremisesAssoc == filterOptions.RestrictionNum);
            }*/
            if (filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Count!=0)
            {
                var obas = registryContext.OwnershipBuildingsAssoc
                    .Include(oba => oba.OwnershipRightNavigation)
                    .AsTracking();
                var opas = registryContext.OwnershipPremisesAssoc
                    .Include(oba => oba.OwnershipRightNavigation)
                    .Join(registryContext.Premises, opa => opa.IdPremises, p => p.IdPremises, (opa, p) => new { idpremise = opa.IdPremises, idOPA = opa.IdOwnershipRight, owp = opa.OwnershipRightNavigation, idbuilding = p.IdBuilding })
                    .Join(obas, opa => opa.idbuilding, oba => oba.IdBuilding, (opa, oba) => new { opa.idpremise, opa.idOPA, opa.owp, idbuilding = oba.IdBuilding, odOBA = oba.IdOwnershipRight, owb = oba.OwnershipRightNavigation })
                    .AsTracking();

                if (!string.IsNullOrEmpty(filterOptions.NumberOwnershipRight))
                    opas = opas.Where(oba => oba.owp.Number == filterOptions.NumberOwnershipRight || oba.owb.Number == filterOptions.NumberOwnershipRight);
                if (filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Count != 0)
                    opas = opas.Where(oba => filterOptions.IdsOwnershipRightType.Contains(oba.owp.IdOwnershipRightType) || filterOptions.IdsOwnershipRightType.Contains(oba.owb.IdOwnershipRightType));
                query = from q in query
                        join idPremise in opas.Select(oba => oba.idpremise).Distinct()
                            on q.IdPremises equals idPremise
                        join idBuilding in opas.Select(oba => oba.idbuilding).Distinct()
                            on q.IdBuilding equals idBuilding
                        select q;
            }
            if (filterOptions.IdObjectState.HasValue && filterOptions.IdObjectState.Value != 0)
            {
                query = query.Where(p => p.IdStateNavigation.IdState == filterOptions.IdObjectState.Value);
            }
            if (filterOptions.IdComment.HasValue && filterOptions.IdComment.Value != 0)
            {
                query = query.Where(p => p.IdStateNavigation.IdState == filterOptions.IdObjectState.Value);
            }
            if (filterOptions.IdLocationDoorKeys.HasValue && filterOptions.IdLocationDoorKeys.Value != 0)
            {
                query = query.Where(p => p.IdPremisesDoorKeysNavigation.IdPremisesDoorKeys == filterOptions.IdLocationDoorKeys.Value);
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

        private bool FilterTypeFund(Premise premise, PremisesListFilter filterOptions)
        {
            var fundsHistory = premise.FundsPremisesAssoc.Select(fpa => fpa.IdFundNavigation);
            var fundHistory = fundsHistory
                .FirstOrDefault(fh => fh.ExcludeRestrictionDate == null && fh.IdFund == fundsHistory.Max(f => f.IdFund));
            return fundHistory == null ? false : fundHistory.IdFundTypeNavigation.IdFundType == filterOptions.IdFundType.Value;
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
            if(orderOptions.OrderField == "PremisesType")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdPremisesTypeNavigation.PremisesTypeName);
                else
                    return query.OrderByDescending(p => p.IdPremisesTypeNavigation.PremisesTypeName);
            }
            if (orderOptions.OrderField == "ObjectState")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdStateNavigation.StateNeutral);
                else
                    return query.OrderByDescending(p => p.IdStateNavigation.StateNeutral);
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
                Payment = registryContext.RentPremises.FirstOrDefault(rp => rp.IdPremises == premise.IdPremises).Payment,
                PaymentInfo = new PremisesPaymentInfo(),
            };

            premisesVM.Premise.IdPaymentNavigation = new PremisesPaymentInfo();
            premisesVM.Premise.IdPaymentNavigation.payment = premisesVM.Payment;
            premisesVM.Premise.IdPaymentNavigation.CPc = registryContext.TotalAreaAvgCosts.ToList()[0].Cost;
            premisesVM.Premise.IdPaymentNavigation.Hb = premisesVM.Premise.IdPaymentNavigation.CPc * 0.001;
            premisesVM.Premise.IdPaymentNavigation.Kc = 0.18;
            premisesVM.Premise.IdPaymentNavigation.K1 = ((premise.IdBuildingNavigation.IdStructureType == 1 ? 1.3 : 0.8) + (new int?[] { 4, 9 }.Contains(premise.IdBuildingNavigation.IdStructureType) ? 1 : premise.IdBuildingNavigation.IdStructureType == 3 ? 0.9 : new int?[] { 2, 5, 6, 8 }.Contains(premise.IdBuildingNavigation.IdStructureType) ? 0.8 : 0)) / 2;
            premisesVM.Premise.IdPaymentNavigation.K2 = premise.IdBuildingNavigation.HotWaterSupply.Value && premise.IdBuildingNavigation.Plumbing.Value && premise.IdBuildingNavigation.Canalization.Value && premise.IdPremisesType == 1 ? 1.3 : premise.IdBuildingNavigation.Plumbing.Value && premise.IdBuildingNavigation.Canalization.Value && premise.IdPremisesType == 1 ? 1 : 0.8;
            premisesVM.Premise.IdPaymentNavigation.K3 = premise.IdBuildingNavigation.IdStreet.StartsWith("380000050410") || premise.IdBuildingNavigation.IdStreet.StartsWith("380000050230") || premise.IdBuildingNavigation.IdStreet.StartsWith("380000050180") ? 1 : premise.IdBuildingNavigation.IdStreet.StartsWith("380000050130") ? 0.9 : 0.8;
            premisesVM.Premise.IdPaymentNavigation.rent=(premisesVM.Premise.IdPaymentNavigation.K1 + premisesVM.Premise.IdPaymentNavigation.K2 + premisesVM.Premise.IdPaymentNavigation.K3)/3 * premisesVM.Premise.IdPaymentNavigation.Hb * premisesVM.Premise.IdPaymentNavigation.Kc * registryContext.RentObjectsAreaAndCategories.SingleOrDefault(r=>r.IdPremises==premisesVM.Premise.IdPremises).RentArea;
            
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
