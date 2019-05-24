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

namespace RegistryWeb.DataServices
{
    public class PremisesListDataService : ListDataService<PremisesListVM, PremisesListFilter>
    {
        public PremisesListDataService(RegistryContext rc) : base(rc) { }

        public override PremisesListVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, PremisesListFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.PremisesTypes = registryContext.PremisesTypes;
            viewModel.ObjectStates = registryContext.ObjectStates;
            viewModel.FundTypes = registryContext.FundTypes;
            return viewModel;
        }

        public PremisesListVM GetViewModel(
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
            viewModel.Premises = GetQueryPage(query, viewModel.PageOptions);
            return viewModel;
        }

        public IQueryable<Premises> GetQuery()
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

        private IQueryable<Premises> GetQueryFilter(IQueryable<Premises> query, PremisesListFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.Street))
            {
                query = query.Where(p => p.IdBuildingNavigation.IdStreetNavigation.StreetLong.ToLowerInvariant().Contains(filterOptions.Street.ToLowerInvariant()));
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

        private bool FilterTypeFund(Premises premise, PremisesListFilter filterOptions)
        {
            var fundsHistory = premise.FundsPremisesAssoc.Select(fpa => fpa.IdFundNavigation);
            var fundHistory = fundsHistory
                .FirstOrDefault(fh => fh.ExcludeRestrictionDate == null && fh.IdFund == fundsHistory.Max(f => f.IdFund));
            return fundHistory == null ? false : fundHistory.IdFundTypeNavigation.IdFundType == filterOptions.IdFundType.Value;
        }

        private IQueryable<Premises> GetQueryOrder(IQueryable<Premises> query, OrderOptions orderOptions)
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
                    return query.OrderBy(p => p.IdPremisesTypeNavigation.PremisesType);
                else
                    return query.OrderByDescending(p => p.IdPremisesTypeNavigation.PremisesType);
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

        public List<Premises> GetQueryPage(IQueryable<Premises> query, PageOptions pageOptions)
            => query
            .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
            .Take(pageOptions.SizePage).ToList();


        private IQueryable<Premises> GetQueryOrderMask(
            IQueryable<Premises> query,
            bool compare,
            OrderOptions orderOptions,
            Expression<Func<Premises, int>> expression)
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
