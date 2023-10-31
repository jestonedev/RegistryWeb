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
using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.RegistryObjects;
using RegistryDb.Models.Entities.Common;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;
using RegistryDb.Models.Entities.RegistryObjects.Common.Ownerships;
using RegistryDb.Models.Entities.RegistryObjects.Common;
using RegistryServices.DataFilterServices;

namespace RegistryWeb.DataServices
{
    public class BuildingsDataService : ListDataService<BuildingsVM, BuildingsFilter>
    {
        private readonly ReportService reportService;
        private readonly IFilterService<Building, BuildingsFilter> filterService;

        public BuildingsDataService(RegistryContext registryContext, 
            AddressesDataService addressesDataService, 
            ReportService reportService,
            FilterServiceFactory<IFilterService<Building, BuildingsFilter>> filterServiceFactory
            ) : base(registryContext, addressesDataService)
        {
            this.reportService = reportService;
            filterService = filterServiceFactory.CreateInstance();
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
            query = filterService.GetQueryFilter(query, viewModel.FilterOptions);
            query = filterService.GetQueryOrder(query, viewModel.OrderOptions);
            query = filterService.GetQueryIncludes(query);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            viewModel.Buildings = filterService.GetQueryPage(query, viewModel.PageOptions).ToList();
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

        public IEnumerable<BuildingManagmentOrg> BuildingManagmentOrgs
        {
            get => registryContext.BuildingManagmentOrgs.AsNoTracking();
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
