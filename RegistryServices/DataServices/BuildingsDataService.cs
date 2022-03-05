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
using RegistryDb.Models.SqlViews;
using RegistryDb.Models;
using RegistryDb.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.RegistryObjects;
using RegistryDb.Models.Entities.Common;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;
using RegistryDb.Models.Entities.RegistryObjects.Common.Ownerships;
using RegistryDb.Models.Entities.RegistryObjects.Common;

namespace RegistryWeb.DataServices
{
    public class BuildingsDataService : ListDataService<BuildingsVM, BuildingsFilter>
    {
        ReportService reportService;

        public BuildingsDataService(RegistryContext registryContext, AddressesDataService addressesDataService, ReportService reportService) : base(registryContext, addressesDataService)
        {
            this.reportService = reportService;
        }

        public override BuildingsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, BuildingsFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.KladrRegionsList = new SelectList(addressesDataService.KladrRegions, "IdRegion", "Region");
            viewModel.KladrStreetsList = new SelectList(addressesDataService.GetKladrStreets(filterOptions?.IdRegion), "IdStreet", "StreetName");
            viewModel.ObjectStatesList = new SelectList(ObjectStates, "IdState", "StateFemale");
            viewModel.OwnershipRightTypesList = new SelectList(registryContext.OwnershipRightTypes, "IdOwnershipRightType", "OwnershipRightTypeName");
            viewModel.RestrictionsList = new SelectList(registryContext.RestrictionTypes, "IdRestrictionType", "RestrictionTypeName");
            viewModel.SignersList = new SelectList(registryContext.SelectableSigners.Where(s => s.IdSignerGroup == 1).ToList().Select(s => new {
                s.IdRecord,
                Snp = s.Surname + " " + s.Name + (s.Patronymic == null ? "" : " " + s.Patronymic)
            }), "IdRecord", "Snp");
            viewModel.GovernmentDecreesList = new SelectList(registryContext.GovernmentDecrees, "IdDecree", "Number");
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
            query = GetQueryIncludes(query);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            viewModel.Buildings = GetQueryPage(query, viewModel.PageOptions).ToList();
            viewModel.IsMunicipalDictionary = IsMunicipalDictionary(viewModel.Buildings);
            viewModel.BuildingsOwnershipRightCurrent = GetBuildingsOwnershipRightCurrent(viewModel.Buildings);
            return viewModel;
        }

        private List<BuildingOwnershipRightCurrent> GetBuildingsOwnershipRightCurrent(List<Building> buildings)
        {
            var ids = buildings.Select(p => p.IdBuilding).ToList();
            return registryContext.BuildingsOwnershipRightCurrent
                .Where(p => ids.Contains(p.IdBuilding))
                .Select(p => new BuildingOwnershipRightCurrent { IdBuilding = p.IdBuilding, IdOwnershipRightType = p.IdOwnershipRightType })
                .ToList();
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
                .Include(b => b.IdStateNavigation);
        }

        public IQueryable<Building> GetQueryIncludes(IQueryable<Building> query)
        {
            return query
                .Include(b => b.IdStreetNavigation)
                .Include(b => b.IdStateNavigation)
                .Include(b => b.IdStructureTypeNavigation);
        }

        private IQueryable<Building> GetQueryFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = IdBuildingFilter(query, filterOptions);
                query = IdDecreeFilter(query, filterOptions);
                query = RegionFilter(query, filterOptions);
                query = StreetFilter(query, filterOptions);
                query = HouseFilter(query, filterOptions);
                query = FloorsFilter(query, filterOptions);
                query = CadastralNumFilter(query, filterOptions);
                query = StartupDateFilter(query, filterOptions);
                query = EntrancesFilter(query, filterOptions);
                query = OwnershipRightFilter(query, filterOptions);
                query = RestrictionFilter(query, filterOptions);
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
                if (!string.IsNullOrEmpty(filterOptions.NumberOwnershipRight) || filterOptions.DateOwnershipRight != null)
                {
                    if (filterOptions.DateOwnershipRight.HasValue)
                        obas = obas.Where(oba => oba.OwnershipRightNavigation.Date == filterOptions.DateOwnershipRight.Value);
                    if (!string.IsNullOrEmpty(filterOptions.NumberOwnershipRight))
                        obas = obas.Where(oba => oba.OwnershipRightNavigation.Number == filterOptions.NumberOwnershipRight);
                
                    query = from q in query
                            join idBuilding in obas.Select(oba => oba.IdBuilding).Distinct()
                                on q.IdBuilding equals idBuilding
                            select q;
                }


                if (filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Count != 0)
                {

                    var buildings = from tbaRow in registryContext.OwnershipBuildingsAssoc
                                    join buildingRow in registryContext.Buildings
                                    on tbaRow.IdBuilding equals buildingRow.IdBuilding
                                    join streetRow in registryContext.KladrStreets
                                    on buildingRow.IdStreet equals streetRow.IdStreet
                                    select tbaRow;
                    if (filterOptions.DateOwnershipRight.HasValue)
                        buildings = buildings.Where(oba => oba.OwnershipRightNavigation.Date == filterOptions.DateOwnershipRight.Value);
                    if (!string.IsNullOrEmpty(filterOptions.NumberOwnershipRight))
                        buildings = buildings.Where(oba => oba.OwnershipRightNavigation.Number == filterOptions.NumberOwnershipRight);

                    if (filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Any())
                    {
                        var specialOwnershipRightTypeIds = new int[] { 1, 2, 6, 7 };
                        var specialIds = filterOptions.IdsOwnershipRightType.Where(id => specialOwnershipRightTypeIds.Contains(id));
                        var generalIds = filterOptions.IdsOwnershipRightType.Where(id => !specialOwnershipRightTypeIds.Contains(id));

                        var generalOwnershipRightsBuildings = from owrRow in registryContext.OwnershipRights
                                                              join bRow in registryContext.OwnershipBuildingsAssoc
                                                              on owrRow.IdOwnershipRight equals bRow.IdOwnershipRight
                                                              where generalIds.Contains(owrRow.IdOwnershipRightType)
                                                              select bRow.IdBuilding;

                        var specialOwnershipRightsBuildings = from owrRow in registryContext.BuildingsOwnershipRightCurrent
                                                              where specialIds.Contains(owrRow.IdOwnershipRightType)
                                                              select owrRow.IdBuilding;

                        var ownershipRightsBuildingsList = generalOwnershipRightsBuildings.Union(specialOwnershipRightsBuildings).ToList();

                        var buildingIds = (from bRow in buildings
                                          where ownershipRightsBuildingsList.Contains(bRow.IdBuilding)
                                          select bRow.IdBuilding).ToList();

                        if (filterOptions.IdsOwnershipRightTypeContains == null || filterOptions.IdsOwnershipRightTypeContains.Value)
                        {
                            query = (from row in query
                                     where buildingIds.Contains(row.IdBuilding)
                                     select row).Distinct();
                        }
                        else
                        {
                            query = (from row in query
                                     where !buildingIds.Contains(row.IdBuilding)
                                     select row).Distinct();
                        }
                    }
                }
            }
            
            return query;
        }

        private IQueryable<Building> RestrictionFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
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

                         where (borRow != null &&
                            ((filterOptions.IdsRestrictionType == null || !filterOptions.IdsRestrictionType.Any() ||
                            filterOptions.IdsRestrictionType.Contains(borRow.IdRestrictionType)) &&
                            (string.IsNullOrEmpty(filterOptions.RestrictionNum) ||
                             borRow.Number.ToLower() == filterOptions.RestrictionNum.ToLower()) &&
                             (filterOptions.RestrictionDate == null || borRow.Date == filterOptions.RestrictionDate)))
                         select q).Distinct();
            }
            return query;
        }

        private IQueryable<Building> AddressFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return query;

            var addresses = addressesDataService.GetAddressesByText(filterOptions.Address.Text).Select(r => r.Id);

            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                return query.Where(q => addresses.Contains(q.IdStreet));
            }

            if (filterOptions.Address.AddressType == AddressTypes.Building)
            {
                var addressesInt = addresses.Where(a => int.TryParse(a, out int aInt)).Select(a => int.Parse(a));
                if (!addressesInt.Any())
                    return query;
                return query.Where(q => addressesInt.Contains(q.IdBuilding));
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

        private IQueryable<Building> RegionFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.IdRegion))
            {
                query = query.Where(b => b.IdStreet.Contains(filterOptions.IdRegion));
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

        private IQueryable<Building> CadastralNumFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.CadastralNum))
            {
                query = query.Where(b => b.CadastralNum == filterOptions.CadastralNum);
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

        private IQueryable<Building> StartupDateFilter(IQueryable<Building> query, BuildingsFilter filterOptions)
        {
            if (filterOptions.StartupYear.HasValue)
            {
                query = query.Where(b => b.StartupYear == filterOptions.StartupYear.Value);
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
            if (orderOptions.OrderField == "Address")
            {
                var addresses = query.Select(b => new
                {
                    b.IdBuilding,
                    Address = string.Concat(b.IdStreetNavigation.StreetName, ", ", b.House)
                });

                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                {
                    return from row in query
                           join addr in addresses
                            on row.IdBuilding equals addr.IdBuilding
                           orderby addr.Address
                           select row;
                }
                else
                {
                    return from row in query
                           join addr in addresses
                            on row.IdBuilding equals addr.IdBuilding
                           orderby addr.Address descending
                           select row;
                }
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

        public double GetPremisesTotalAreaByStates(int idBuilding, List<int> states, bool contains = true)
        {
            return (from row in registryContext.Premises
                    where row.IdBuilding == idBuilding && ((contains && states.Contains(row.IdState)) || (!contains && !states.Contains(row.IdState)))
                    select row.TotalArea).Sum();
        }

        public double GetPremisesLivingAreaByStates(int idBuilding, List<int> states, bool contains = true)
        {
            return (from row in registryContext.Premises
                    where row.IdBuilding == idBuilding && ((contains && states.Contains(row.IdState)) || (!contains && !states.Contains(row.IdState)))
                    select row.LivingArea).Sum();
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

        public IEnumerable<SelectableSigner> SelectableSigners
        {
            get => registryContext.SelectableSigners.Where(s => s.IdSignerGroup == 1).ToList();
        }

        public Building CreateBuilding()
        {
            var building = new Building {
                IdDecree = 1
            };
            return building;
        }

        public void Create(Building building, List<Microsoft.AspNetCore.Http.IFormFile> files)
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
            // Прикрепляем прочие файлы
            if (building.BuildingAttachmentFilesAssoc != null)
            {
                for (var i = 0; i < building.BuildingAttachmentFilesAssoc.Count; i++)
                {
                    var file = files.Where(r => r.Name == "AttachmentFile[" + i + "]").FirstOrDefault();
                    if (file == null) continue;
                    building.BuildingAttachmentFilesAssoc[i].ObjectAttachmentFileNavigation.FileDisplayName = file.FileName;
                    var fileOriginName = reportService.SaveFormFileToRepository(file, ActFileTypes.Attachment);
                    building.BuildingAttachmentFilesAssoc[i].ObjectAttachmentFileNavigation.FileOriginName = fileOriginName;
                    building.BuildingAttachmentFilesAssoc[i].ObjectAttachmentFileNavigation.FileMimeType = file.ContentType;
                }
            }
            registryContext.Buildings.Add(building);
            registryContext.SaveChanges();
        }

        public void Delete(int idBuilding)
        {
            var building = registryContext.Buildings
                .FirstOrDefault(op => op.IdBuilding == idBuilding);
            building.Deleted = 1;
            registryContext.SaveChanges();
        }

        public void Edit(Building newBuilding, bool canEditBaseInfo, bool canEditLandInfo)
        {
            //Добавление и радактирование
            if (!canEditBaseInfo && canEditLandInfo)
            {
                var building = registryContext.Buildings.FirstOrDefault(r => r.IdBuilding == newBuilding.IdBuilding);
                building.LandArea = newBuilding.LandArea;
                building.LandCadastralDate = newBuilding.LandCadastralDate;
                building.LandCadastralNum = newBuilding.LandCadastralNum;
                registryContext.SaveChanges();
            }
            else
            if (canEditBaseInfo)
            {
                registryContext.Buildings.Update(newBuilding);
                registryContext.SaveChanges();
            }
        }
    }
}
