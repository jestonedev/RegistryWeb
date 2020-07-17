using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.DataHelpers;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using RegistryWeb.ReportServices;
using RegistryWeb.SecurityServices;

namespace RegistryWeb.DataServices
{
    public class BuildingsDataService : ListDataService<BuildingsVM, BuildingsFilter>
    {
        ReportService reportService;

        public BuildingsDataService(RegistryContext registryContext, ReportService reportService) : base(registryContext)
        {
            this.reportService = reportService;
        }

        public override BuildingsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, BuildingsFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
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
            viewModel.IsMunicipalDictionary = IsMunicipalDictionary(viewModel.Buildings);
            return viewModel;
        }

        private Dictionary<int, bool> IsMunicipalDictionary(List<Building> buildings)
        {
            var idsBuiling = buildings.Select(b => b.IdBuilding).ToList();
            var result = new Dictionary<int, bool>();
            var premises = registryContext.Premises
                .Include(p => p.SubPremises)
                .Where(p => idsBuiling.Contains(p.IdBuilding))
                .ToList();
            foreach (var b in buildings)
            {
                result[b.IdBuilding] = ObjectStateHelper.IsMunicipal(b.IdState);
                var premisesInsideCurrentBuilding = premises.Where(p => p.IdBuilding == b.IdBuilding);
                foreach (var pr in premisesInsideCurrentBuilding)
                {
                    result[b.IdBuilding] = result[b.IdBuilding] || ObjectStateHelper.IsMunicipal(pr.IdState);
                    var isMunicipalSubPremiseList = pr.SubPremises.Select(sp => ObjectStateHelper.IsMunicipal(sp.IdState));
                    foreach (var isMunicipalSubPremise in isMunicipalSubPremiseList)
                    {
                        result[b.IdBuilding] = result[b.IdBuilding] || isMunicipalSubPremise;
                    }
                }
            }
            return result;
        }

        public bool IsMunicipal(int idBuilding)
        {
            var builing = registryContext.Buildings.FirstOrDefault();
            return IsMunicipal(builing);
        }

        public bool IsMunicipal(Building building)
        {
            var premises = registryContext.Premises
                .Include(p => p.SubPremises)
                .Where(p => p.IdBuilding == building.IdBuilding)
                .ToList();
            var result = ObjectStateHelper.IsMunicipal(building.IdState);
            foreach (var pr in premises)
            {
                result = result || ObjectStateHelper.IsMunicipal(pr.IdState);
                var isMunicipalSubPremiseList = pr.SubPremises.Select(sp => ObjectStateHelper.IsMunicipal(sp.IdState));
                foreach (var isMunicipalSubPremise in isMunicipalSubPremiseList)
                {
                    result = result || isMunicipalSubPremise;
                }
            }
            return result;
        }

        public IQueryable<Building> GetQuery()
        {
            return registryContext.Buildings
                .Include(b => b.IdStreetNavigation)
                .Include(b => b.IdStateNavigation)
                .OrderBy(b => b.IdBuilding);
        }

        private IQueryable<Building> GetQueryFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = IdBuildingFilter(query, filterOptions);
                query = IdDecreeFilter(query, filterOptions);
                query = StreetFilter(query, filterOptions);
                query = HouseFilter(query, filterOptions);
                query = FloorsFilter(query, filterOptions);
                query = EntrancesFilter(query, filterOptions);
                query = OwnershipRightFilter(query, filterOptions);
                query = ObjectStateFilter(query, filterOptions);
            }
            return query;
        }

        private IQueryable<Building> OwnershipRightFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!filterOptions.IsOwnershipRightEmpty())
            {
                var obas = registryContext.OwnershipBuildingsAssoc
                    .Include(oba => oba.OwnershipRightNavigation)
                    .AsTracking();
                if (filterOptions.DateOwnershipRight.HasValue)
                    obas = obas.Where(oba => oba.OwnershipRightNavigation.Date == filterOptions.DateOwnershipRight.Value);
                if (!string.IsNullOrEmpty(filterOptions.NumberOwnershipRight))
                    obas = obas.Where(oba => oba.OwnershipRightNavigation.Number == filterOptions.NumberOwnershipRight);
                if (filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Count != 0)
                        obas = obas.Where(oba => filterOptions.IdsOwnershipRightType.Contains(oba.OwnershipRightNavigation.IdOwnershipRightType));
                query = from q in query
                        join idBuilding in obas.Select(oba => oba.IdBuilding).Distinct()
                            on q.IdBuilding equals idBuilding
                        select q;
            }
            return query;
        }

        private IQueryable<Building> AddressFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return query;
            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                return query.Where(q => q.IdStreet.Equals(filterOptions.Address.Id));
            }
            int id = 0;
            if (!int.TryParse(filterOptions.Address.Id, out id))
                return query;
            if (filterOptions.Address.AddressType == AddressTypes.Building)
            {
                return query.Where(q => q.IdBuilding == id);
            }
            return query;
        }

        private IQueryable<Building> IdBuildingFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.IdBuilding.HasValue)
            {
                query = query.Where(b => b.IdBuilding == filterOptions.IdBuilding.Value);
            }
            return query;
        }

        private IQueryable<Building> IdDecreeFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.IdDecree.HasValue)
            {
                query = query.Where(b => b.IdDecree == filterOptions.IdDecree.Value);
            }
            return query;
        }

        private IQueryable<Building> StreetFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                query = query.Where(b => b.IdStreet == filterOptions.IdStreet );
            }
            return query;
        }

        private IQueryable<Building> HouseFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                query = query.Where(b => b.House.ToLower() == filterOptions.House.ToLower());
            }
            return query;
        }

        private IQueryable<Building> FloorsFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.Floors.HasValue)
            {
                query = query.Where(b => b.Floors == filterOptions.Floors.Value);
            }
            return query;
        }

        private IQueryable<Building> EntrancesFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.Entrances.HasValue)
            {
                query = query.Where(b => b.Entrances == filterOptions.Entrances.Value);
            }
            return query;
        }

        private IQueryable<Building> ObjectStateFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.IdsObjectState != null && filterOptions.IdsObjectState.Count != 0)
            {
                query = query.Where(b => filterOptions.IdsObjectState.Contains(b.IdStateNavigation.IdState));
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
                .Include(b => b.StructureTypeOverlapNavigation)
                .Include(b => b.FoundationTypeNavigation)
                .Include(b => b.GovernmentDecreeNavigation)
                .SingleOrDefault(b => b.IdBuilding == idBuilding);
        }

        public IEnumerable<Building> GetBuildings(List<int> ids)
        {
            return registryContext.Buildings
                .Include(b => b.IdStreetNavigation)
                .Where(b => ids.Contains(b.IdBuilding));
        }

        public IEnumerable<ObjectState> ObjectStates
        {
            get => registryContext.ObjectStates.AsNoTracking();
        }

        public IEnumerable<ObjectState> GetObjectStates(SecurityService securityService, string action, bool canEditBaseInfo = false)
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

        public IEnumerable<StructureType> StructureTypes
        {
            get => registryContext.StructureTypes.AsNoTracking();
        }

        public IEnumerable<StructureTypeOverlap> StructureTypeOverlaps
        {
            get => registryContext.StructureTypeOverlaps.AsNoTracking();
        }

        public IEnumerable<KladrStreet> KladrStreets
        {
            get => registryContext.KladrStreets.AsNoTracking();
        }

        public IEnumerable<HeatingType> HeatingTypes
        {
            get => registryContext.HeatingTypes.AsNoTracking();
        }

        public IEnumerable<OwnershipRightType> OwnershipRightTypes
        {
            get => registryContext.OwnershipRightTypes.AsNoTracking();
        }

        public IEnumerable<GovernmentDecree> GovernmentDecrees
        {
            get => registryContext.GovernmentDecrees.AsNoTracking();
        }

        public IEnumerable<FoundationType> FoundationTypes
        {
            get => registryContext.FoundationTypes.AsNoTracking();
        }

        internal Building CreateBuilding()
        {
            var building = new Building();
            return building;
        }

        internal void Create(Building building, List<Microsoft.AspNetCore.Http.IFormFile> files)
        {
            // Прикрепляем файлы реквизитов
            if (building.RestrictionBuildingsAssoc != null)
            {
                for (var i = 0; i < building.RestrictionBuildingsAssoc.Count; i++)
                {
                    var file = files.Where(r => r.Name == "RestrictionFile[" + i + "]").FirstOrDefault();
                    if (file == null) continue;
                    building.RestrictionBuildingsAssoc[i].RestrictionNavigation.FileDisplayName = file.FileName;
                    var fileOriginName = reportService.SaveFormFileToRepository(file, ActFileTypes.Restriction);
                    building.RestrictionBuildingsAssoc[i].RestrictionNavigation.FileOriginName = fileOriginName;
                    building.RestrictionBuildingsAssoc[i].RestrictionNavigation.FileMimeType = file.ContentType;
                }
            }
            // Прикрепляем файлы ограничений
            if (building.OwnershipBuildingsAssoc != null)
            {
                for (var i = 0; i < building.OwnershipBuildingsAssoc.Count; i++)
                {
                    var file = files.Where(r => r.Name == "OwnershipRightFile[" + i + "]").FirstOrDefault();
                    if (file == null) continue;
                    building.OwnershipBuildingsAssoc[i].OwnershipRightNavigation.FileDisplayName = file.FileName;
                    var fileOriginName = reportService.SaveFormFileToRepository(file, ActFileTypes.OwnershipRight);
                    building.OwnershipBuildingsAssoc[i].OwnershipRightNavigation.FileOriginName = fileOriginName;
                    building.OwnershipBuildingsAssoc[i].OwnershipRightNavigation.FileMimeType = file.ContentType;
                }
            }
            // Прикрепляем файлы о сносе
            if (building.BuildingDemolitionActFiles != null)
            {
                for (var i = 0; i < building.BuildingDemolitionActFiles.Count; i++)
                {
                    var file = files.Where(r => r.Name == "BuildingDemolitionActFile[" + i + "]").FirstOrDefault();
                    if (file == null) continue;
                    var actFile = new ActFile();
                    actFile.FileName = reportService.SaveFormFileToRepository(file, ActFileTypes.BuildingDemolitionActFile);
                    actFile.OriginalName = file.FileName;
                    actFile.MimeType = file.ContentType;
                    building.BuildingDemolitionActFiles[i].ActFile = actFile; 
                }
            }
            registryContext.Buildings.Add(building);
            registryContext.SaveChanges();
        }

        internal void Delete(int idBuilding)
        {
            var building = registryContext.Buildings
                .FirstOrDefault(op => op.IdBuilding == idBuilding);
            building.Deleted = 1;
            registryContext.SaveChanges();
        }

        internal void Edit(Building newBuilding)
        {
            //Добавление и радактирование
            registryContext.Buildings.Update(newBuilding);
            registryContext.SaveChanges();
        }
    }
}
