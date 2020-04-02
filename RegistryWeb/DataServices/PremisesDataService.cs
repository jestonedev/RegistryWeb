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
            return registryContext.Premises
                .Include(p => p.IdBuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(p => p.IdStateNavigation)        //Текущее состояние объекта
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
            if (filterOptions.IdPremisesType.HasValue)
            {
                query = query.Where(p => p.IdPremisesTypeNavigation.IdPremisesType == filterOptions.IdPremisesType.Value);
            }
            if (filterOptions.IdObjectState.HasValue)
            {
                query = query.Where(p => p.IdStateNavigation.IdState == filterOptions.IdObjectState.Value);
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

                query = query.Where(p => idPremises.Contains(p.IdPremises));
            }
            return query;
        }

        private IQueryable<Premise> AddressFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                return query.Where(q => q.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id));
            }
            int id = 0;
            if (!int.TryParse(filterOptions.Address.Id, out id))
                return query;
            if (filterOptions.Address.AddressType == AddressTypes.Building)
            {
                return query.Where(q => q.IdBuilding == id);
            }
            if (filterOptions.Address.AddressType == AddressTypes.Premise)
            {
                return query.Where(q => q.IdPremises == id);
            }
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

        internal void Create(Premise premise)
        {
            registryContext.Premises.Add(premise);
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

            /*ViewBag.ObjectStates = dataService.ObjectStates;
            ViewBag.StructureTypes = dataService.StructureTypes;
            ViewBag.KladrStreets = dataService.KladrStreets;
            ViewBag.HeatingTypes = dataService.HeatingTypes;*/
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
                CommentList = new SelectList(registryContext.PremisesComments, "IdPremisesComment", "PremisesCommentText")
            };

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
            return registryContext.Premises
                .Include(b => b.IdBuildingNavigation).ThenInclude(b => b.IdStreetNavigation)
                .Include(b => b.IdBuildingNavigation.IdHeatingTypeNavigation)
                .Include(b => b.IdStateNavigation)                              //Текущее состояние объекта
                .Include(b => b.IdBuildingNavigation.IdStructureTypeNavigation) //Тип помещения: квартира, комната, квартира с подселением
                .Include(b => b.FundsPremisesAssoc).ThenInclude(fpa => fpa.IdFundNavigation).ThenInclude(fh => fh.IdFundTypeNavigation)
                .Include(b => b.IdPremisesCommentNavigation).ThenInclude(fpa => fpa.Premises)
                .Include(b => b.IdPremisesTypeNavigation).ThenInclude(fpa => fpa.Premises)
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



        public IEnumerable<Premise> GetPremises(List<int> ids)
        {
            return registryContext.Premises
                .Include(b => b.IdBuildingNavigation.IdStreetNavigation)
                .Include(b => b.IdBuildingNavigation).ThenInclude(b => b.IdStreetNavigation)
                .Include(b => b.IdBuildingNavigation).ThenInclude(b => b.OwnershipBuildingsAssoc)
                .Include(b => b.IdBuildingNavigation.IdHeatingTypeNavigation)
                .Include(b => b.IdStateNavigation)                              //Текущее состояние объекта
                .Include(b => b.IdBuildingNavigation.IdStructureTypeNavigation) //Тип помещения: квартира, комната, квартира с подселением
                .Include(b => b.FundsPremisesAssoc).ThenInclude(fpa => fpa.IdFundNavigation).ThenInclude(fh => fh.IdFundTypeNavigation)
                .Include(b => b.IdPremisesCommentNavigation).ThenInclude(fpa => fpa.Premises)
                .Include(b => b.IdPremisesTypeNavigation).ThenInclude(fpa => fpa.Premises)
                .Include(b => b.IdPremisesDoorKeysNavigation)
                .Include(b => b.OwnershipPremisesAssoc)
                .Where(b => ids.Contains(b.IdBuilding));
        }

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
    }
}
