﻿using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;
using System;

namespace RegistryWeb.DataServices
{
    public class BuildingsDataService : ListDataService<BuildingsVM, BuildingsFilter>
    {
        public BuildingsDataService(RegistryContext registryContext) : base(registryContext) { }

        public override BuildingsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, BuildingsFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.ObjectStates = registryContext.ObjectStates;
            return viewModel;
        }

        public BuildingsVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            BuildingsFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var query = GetQuery();
            viewModel.PageOptions.TotalRows = query.Count();
            query = GetQueryFilter(query, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            viewModel.Buildings = GetQueryPage(query, viewModel.PageOptions).ToList();
            return viewModel;
        }

        public IQueryable<Building> GetQuery()
        {
            return registryContext.Buildings
                .Include(b => b.IdStreetNavigation)
                .Include(b => b.IdHeatingTypeNavigation)
                .Include(b => b.IdStateNavigation)
                .Include(b => b.IdStructureTypeNavigation)
                .Where(b => b.Deleted == 0)
                .OrderBy(b => b.IdBuilding);
        }
        private IQueryable<Building> GetQueryFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.Street))
            {
                query = query.Where(b => b.IdStreetNavigation.StreetLong.ToLowerInvariant().Contains(filterOptions.Street.ToLowerInvariant()));
            }
            if (filterOptions.IdObjectState.HasValue)
            {
                query = query.Where(b =>b.IdStateNavigation.IdState == filterOptions.IdObjectState.Value);
            }
            return query;
        }

        private IQueryable<Building> GetQueryOrder(IQueryable<Building> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField) || orderOptions.OrderField == "IdBuilding")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(b => b.IdBuilding);
                else
                    return query.OrderByDescending(b => b.IdBuilding);
            }
            if (orderOptions.OrderField == "ObjectState")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(b => b.IdStateNavigation.StateNeutral);
                else
                    return query.OrderByDescending(b => b.IdStateNavigation.StateNeutral);
            }
            return query;
        }

        public IQueryable<Building> GetQueryPage(IQueryable<Building> query, PageOptions pageOptions)
        {
            return query
            .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
            .Take(pageOptions.SizePage);
        }

        public Building GetBuilding(int idBuilding)
        {
            return registryContext.Buildings
                .Include(b => b.IdStreetNavigation)
                .Include(b => b.IdHeatingTypeNavigation)
                .Include(b => b.IdStateNavigation)
                .Include(b => b.IdStructureTypeNavigation)
                .SingleOrDefault(b => b.IdBuilding == idBuilding);
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
    }
}