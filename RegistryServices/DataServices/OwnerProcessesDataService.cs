using RegistryDb.Models;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.Extensions.Configuration;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.Owners;
using RegistryDb.Models.Entities.Owners;
using RegistryDb.Models.Entities.Owners.Log;
using RegistryServices.DataFilterServices;

namespace RegistryWeb.DataServices
{
    public class OwnerProcessesDataService : ListDataService<OwnerProcessesVM, OwnerProcessesFilter>
    {
        private readonly IFilterService<OwnerProcess, OwnerProcessesFilter> filterService;

        public string AttachmentsPath { get; private set; }

        public OwnerProcessesDataService(RegistryContext registryContext, AddressesDataService addressesDataService,
            IConfiguration config,
            FilterServiceFactory<IFilterService<OwnerProcess, OwnerProcessesFilter>> filterServiceFactory
            ) : base(registryContext, addressesDataService)
        {
            AttachmentsPath = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"OwnerProcesses");
            filterService = filterServiceFactory.CreateInstance();
        }

        public override OwnerProcessesVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, OwnerProcessesFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.KladrStreets = addressesDataService.KladrStreets;
            viewModel.OwnerTypes = registryContext.OwnerType.AsNoTracking();
            return viewModel;
        }

        public OwnerProcessesVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            OwnerProcessesFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var ownerProcesses = GetQuery();
            viewModel.PageOptions.TotalRows = ownerProcesses.Count();
            var query = filterService.GetQueryFilter(ownerProcesses, viewModel.FilterOptions);
            query = filterService.GetQueryOrder(query, viewModel.OrderOptions);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.OwnerProcesses = filterService.GetQueryPage(query, viewModel.PageOptions).ToList();
            viewModel.Addresses = GetAddresses(viewModel.OwnerProcesses);
            return viewModel;
        }

        public string GetHrefToReestr(int id, AddressTypes addressType)
        {
            if (addressType == AddressTypes.Building)
            {
                return "/Buildings/Details?idBuilding=" + id;
            }
            if (addressType == AddressTypes.Premise)
            {
                return "/Premises/Details?idPremises=" + id;
            }
            if (addressType == AddressTypes.SubPremise)
            {
                var idPremise = registryContext.SubPremises
                    .Single(sp => sp.IdSubPremises == id)
                    .IdPremises;
                return "/Premises/Details?idPremises=" + idPremise;
            }
            return "#";
        }

        private IQueryable<OwnerProcess> GetQuery()
        {
            return registryContext.OwnerProcesses.AsNoTracking();
        }

        private Dictionary<int, List<Address>> GetAddresses(IEnumerable<OwnerProcess> ownerProcesses)
        {
            var ownerBuildingsAssoc = registryContext.OwnerBuildingsAssoc
                .Include(oba => oba.BuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(oba => oba.IdProcessNavigation)
                .AsNoTracking();
            var ownerPremisesAssoc = registryContext.OwnerPremisesAssoc
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdBuildingNavigation)
                        .ThenInclude(b => b.IdStreetNavigation)
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.IdProcessNavigation)
                .AsNoTracking();
            var ownerSubPremisesAssoc = registryContext.OwnerSubPremisesAssoc
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdBuildingNavigation)
                            .ThenInclude(b => b.IdStreetNavigation)
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(oba => oba.IdProcessNavigation)
                .AsNoTracking();

            var addresses = new Dictionary<int, List<Address>>();
            var buildingsAssoc = ownerBuildingsAssoc.ToList();
            var premisesAssoc = ownerPremisesAssoc.ToList();
            var subPremisesAssoc = ownerSubPremisesAssoc.ToList();
            foreach (var ownerProcess in ownerProcesses)
            {
                var curOwnerProcessAddresses = new List<Address>();
                var addr = new Address();
                foreach (var oba in buildingsAssoc.Where(oba => oba.IdProcess == ownerProcess.IdProcess))
                {
                    addr = new Address();
                    addr.AddressType = AddressTypes.Building;
                    addr.Id = oba.IdBuilding.ToString();
                    addr.Text = oba.BuildingNavigation.GetAddress();
                    curOwnerProcessAddresses.Add(addr);
                }
                foreach (var opa in premisesAssoc.Where(opa => opa.IdProcess == ownerProcess.IdProcess))
                {
                    addr = new Address();
                    addr.AddressType = AddressTypes.Premise;
                    addr.Id = opa.IdPremise.ToString();
                    addr.Text = opa.PremiseNavigation.GetAddress();
                    curOwnerProcessAddresses.Add(addr);
                }
                foreach (var ospa in subPremisesAssoc.Where(ospa => ospa.IdProcess == ownerProcess.IdProcess))
                {
                    addr = new Address();
                    addr.AddressType = AddressTypes.SubPremise;
                    addr.Id = ospa.IdSubPremise.ToString();
                    addr.Text = ospa.SubPremiseNavigation.GetAddress();
                    curOwnerProcessAddresses.Add(addr);
                }
                addresses.Add(ownerProcess.IdProcess, curOwnerProcessAddresses);
            }
            return addresses;
        }      

        public OwnerProcess CreateOwnerProcess(OwnerProcessesFilter filterOptions)
        {
            var ownerProcess = new OwnerProcess();
            var owner = new Owner() { IdOwnerType = 1 };
            owner.IdOwnerTypeNavigation = GetOwnerType(1);
            owner.OwnerPerson = new OwnerPerson();
            ownerProcess.Owners = new List<Owner>() { owner };
            if (!filterOptions.IsAddressEmpty())
            {
                int id = 0;
                if (!int.TryParse(filterOptions.Address.Id, out id))
                    return ownerProcess;
                switch (filterOptions.Address.AddressType)
                {
                    case AddressTypes.Building:
                        ownerProcess.OwnerBuildingsAssoc = new List<OwnerBuildingAssoc>()
                        {
                            new OwnerBuildingAssoc()
                            {
                                IdBuilding = id,
                                BuildingNavigation = registryContext.Buildings
                                    .Include(b => b.IdStreetNavigation)
                                    .SingleOrDefault(b => b.IdBuilding == id)
                            }
                        };
                        break;
                    case AddressTypes.Premise:
                        ownerProcess.OwnerPremisesAssoc = new List<OwnerPremiseAssoc>()
                        {
                            new OwnerPremiseAssoc()
                            {
                                IdPremise = id,
                                PremiseNavigation = registryContext.Premises
                                    .Include(p => p.IdBuildingNavigation)
                                        .ThenInclude(b => b.IdStreetNavigation)
                                    .Include(p => p.IdPremisesTypeNavigation)
                                    .SingleOrDefault(b => b.IdPremises == id)
                            }
                        };
                        break;
                    case AddressTypes.SubPremise:
                        ownerProcess.OwnerSubPremisesAssoc = new List<OwnerSubPremiseAssoc>()
                        {
                            new OwnerSubPremiseAssoc()
                            {
                                IdSubPremise = id,
                                SubPremiseNavigation = registryContext.SubPremises
                                    .Include(sp => sp.IdPremisesNavigation)
                                        .ThenInclude(p => p.IdBuildingNavigation)
                                            .ThenInclude(b => b.IdStreetNavigation)
                                    .Include(sp => sp.IdPremisesNavigation)
                                        .ThenInclude(p => p.IdPremisesTypeNavigation)
                                    .SingleOrDefault(sp => sp.IdSubPremises == id)
                            }
                        };
                        break;
                }
            }
            return ownerProcess;
        }

        public IQueryable<OwnerReasonType> OwnerReasonTypes()
            => registryContext.OwnerReasonTypes.AsNoTracking();

        public OwnerFile GetOwnerFile(int id)
            => registryContext.OwnerFiles.AsNoTracking().FirstOrDefault(of => of.Id == id);

        public OwnerType GetOwnerType(int idOwnerType)
            => registryContext.OwnerType.FirstOrDefault(ot => ot.IdOwnerType == idOwnerType);

        public OwnerProcess GetOwnerProcess(int idProcess)
        {
            var ownerProcess = registryContext.OwnerProcesses
                .Include(op => op.Owners)
                    .ThenInclude(ow => ow.IdOwnerTypeNavigation)
                .Include(op => op.Owners)
                    .ThenInclude(ow => ow.OwnerPerson)
                .Include(op => op.Owners)
                    .ThenInclude(ow => ow.OwnerOrginfo)
                .Include(op => op.Owners)
                    .ThenInclude(ow => ow.OwnerReasons)
                .Include(op => op.OwnerFiles)
                .AsNoTracking()
                .FirstOrDefault(op => op.IdProcess == idProcess);
            ownerProcess.OwnerBuildingsAssoc =
                registryContext.OwnerBuildingsAssoc
                .Include(oba => oba.BuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Where(oba => oba.IdProcess == idProcess)
                .AsNoTracking()
                .ToList();
            ownerProcess.OwnerPremisesAssoc =
                registryContext.OwnerPremisesAssoc
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(opa => opa.PremiseNavigation)
                    .ThenInclude(p => p.IdBuildingNavigation)
                        .ThenInclude(b => b.IdStreetNavigation)
                .Where(opa => opa.IdProcess == idProcess)
                .AsNoTracking()
                .ToList();
            ownerProcess.OwnerSubPremisesAssoc =
                registryContext.OwnerSubPremisesAssoc
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdPremisesTypeNavigation)
                .Include(ospa => ospa.SubPremiseNavigation)
                    .ThenInclude(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdBuildingNavigation)
                            .ThenInclude(b => b.IdStreetNavigation)
                .Where(ospa => ospa.IdProcess == idProcess)
                .AsNoTracking()
                .ToList();
            return ownerProcess;
        }

        public void Create(OwnerProcess ownerProcess, IFormFileCollection attachmentFiles)
        {
            var ind = 0;
            foreach (var ownerFile in ownerProcess.OwnerFiles)
            {
                //если новый файл прикрепили в документ
                if (ownerFile.FileDisplayName != null)
                {
                    ownerFile.FileOriginName = Guid.NewGuid().ToString() + new FileInfo(attachmentFiles[ind].FileName).Extension;
                    ownerFile.FileMimeType = attachmentFiles[ind].ContentType;
                    var fileStream = new FileStream(Path.Combine(AttachmentsPath, ownerFile.FileOriginName), FileMode.CreateNew);
                    attachmentFiles[ind].OpenReadStream().CopyTo(fileStream);
                    fileStream.Close();
                    ind++;
                }
            }
            registryContext.OwnerProcesses.Add(ownerProcess);
            registryContext.SaveChanges();
        }

        public void Delete(int idProcess)
        {
            var ownerProcess = registryContext.OwnerProcesses
                .Include(op => op.OwnerBuildingsAssoc)
                .Include(op => op.OwnerPremisesAssoc)
                .Include(op => op.OwnerSubPremisesAssoc)
                .Include(op => op.Owners)
                    .ThenInclude(ow => ow.OwnerReasons)
                .Include(op => op.OwnerFiles)
                .FirstOrDefault(op => op.IdProcess == idProcess);
            ownerProcess.Deleted = 1;
            foreach (var o in ownerProcess.OwnerFiles)
            {
                o.Deleted = 1;
            }
            foreach (var o in ownerProcess.OwnerBuildingsAssoc)
            {
                o.Deleted = 1;
            }
            foreach (var o in ownerProcess.OwnerPremisesAssoc)
            {
                o.Deleted = 1;
            }
            foreach (var o in ownerProcess.OwnerSubPremisesAssoc)
            {
                o.Deleted = 1;
            }
            foreach (var owner in ownerProcess.Owners)
            {
                owner.Deleted = 1;
                foreach (var or in owner.OwnerReasons)
                {
                    or.Deleted = 1;
                }
            }
            registryContext.SaveChanges();
        }

        public void Edit(OwnerProcess newOwnerProcess, IFormFileCollection attachmentFiles, bool[] removeFiles)
        {
            var oldOwnerProcess = GetOwnerProcess(newOwnerProcess.IdProcess);
            //Добавление файлов
            var ind = 0;
            for (var i = 0; i < newOwnerProcess.OwnerFiles.Count(); i++)
            {
                var newOwnerFile = newOwnerProcess.OwnerFiles[i];
                var oldOnwerFile = oldOwnerProcess.OwnerFiles.FirstOrDefault(of => of.Id == newOwnerFile.Id);
                var isAddedFile = newOwnerFile.FileDisplayName != null
                    && (oldOnwerFile?.FileOriginName == null || removeFiles[i])
                    && attachmentFiles.Count() > 0;
                //удаляем старый файл, если он был прикреплен
                if (removeFiles[i])
                {
                    if (!string.IsNullOrEmpty(oldOnwerFile?.FileOriginName))
                    {
                        var filePath = Path.Combine(AttachmentsPath, oldOnwerFile?.FileOriginName);
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }
                    }
                }
                //если новый файл прикрепили в документ
                if (isAddedFile)
                {
                    newOwnerFile.FileOriginName = Guid.NewGuid().ToString() + new FileInfo(attachmentFiles[ind].FileName).Extension;
                    newOwnerFile.FileMimeType = attachmentFiles[ind].ContentType;
                    var fileStream = new FileStream(Path.Combine(AttachmentsPath, newOwnerFile.FileOriginName), FileMode.CreateNew);
                    attachmentFiles[ind].OpenReadStream().CopyTo(fileStream);
                    fileStream.Close();
                    ind++;
                }
                //если новый файл не прикрепляли
                else
                {
                    //если старый файл был удален, то null, иначе заносим старое значение
                    newOwnerFile.FileOriginName = removeFiles[i] ? null : oldOnwerFile?.FileOriginName;
                    newOwnerFile.FileMimeType = removeFiles[i] ? null : oldOnwerFile?.FileMimeType;
                }
            }
            //Удаление документов
            //При удалении документа файл не удаляется, а хранится в базе
            for (var i = 0; i < oldOwnerProcess.OwnerFiles.Count(); i++)
            {
                var oldOwnerFile = oldOwnerProcess.OwnerFiles[i];
                if (newOwnerProcess.OwnerFiles.Select(of => of.Id).Contains(oldOwnerFile.Id) == false)
                {
                    registryContext.Entry(oldOwnerFile).Property(p => p.Deleted).IsModified = true;
                    oldOwnerFile.Deleted = 1;
                    newOwnerProcess.OwnerFiles.Add(oldOwnerFile);
                }
            }
            foreach (var oldOwner in oldOwnerProcess.Owners)
            {
                if (newOwnerProcess.Owners.Select(owp => owp.IdOwner).Contains(oldOwner.IdOwner) == false)
                {
                    registryContext.Entry(oldOwner).Property(p => p.Deleted).IsModified = true;
                    oldOwner.Deleted = 1;
                    //случай, когда удаляется собственник. Все его документы должны удалиться автоматом
                    foreach (var oldOwnerReason in oldOwner.OwnerReasons)
                    {
                        registryContext.Entry(oldOwnerReason).Property(p => p.Deleted).IsModified = true;
                        oldOwnerReason.Deleted = 1;
                    }
                    newOwnerProcess.Owners.Add(oldOwner);
                }
                else
                {
                    //случай, когда удаляется документ.
                    var newOwner = newOwnerProcess.Owners.FirstOrDefault(ow => ow.IdOwner == oldOwner.IdOwner);
                    foreach (var oldOwnerReason in oldOwner.OwnerReasons)
                    {
                        if (newOwner.OwnerReasons.Select(or => or.IdReason).Contains(oldOwnerReason.IdReason) == false)
                        {
                            registryContext.Entry(oldOwnerReason).Property(p => p.Deleted).IsModified = true;
                            oldOwnerReason.Deleted = 1;
                            newOwner.OwnerReasons.Add(oldOwnerReason);
                        }
                    }
                }
            }
            foreach (var oba in oldOwnerProcess.OwnerBuildingsAssoc)
            {
                if (newOwnerProcess.OwnerBuildingsAssoc.Select(owba => owba.IdAssoc).Contains(oba.IdAssoc) == false)
                {
                    registryContext.Entry(oba).Property(p => p.Deleted).IsModified = true;
                    oba.Deleted = 1;
                    newOwnerProcess.OwnerBuildingsAssoc.Add(oba);
                }
            }
            foreach (var opa in oldOwnerProcess.OwnerPremisesAssoc)
            {
                if (newOwnerProcess.OwnerPremisesAssoc.Select(owpa => owpa.IdAssoc).Contains(opa.IdAssoc) == false)
                {
                    registryContext.Entry(opa).Property(p => p.Deleted).IsModified = true;
                    opa.Deleted = 1;
                    newOwnerProcess.OwnerPremisesAssoc.Add(opa);
                }
            }
            foreach (var ospa in oldOwnerProcess.OwnerSubPremisesAssoc)
            {
                if (newOwnerProcess.OwnerSubPremisesAssoc.Select(owspa => owspa.IdAssoc).Contains(ospa.IdAssoc) == false)
                {
                    registryContext.Entry(ospa).Property(p => p.Deleted).IsModified = true;
                    ospa.Deleted = 1;
                    newOwnerProcess.OwnerSubPremisesAssoc.Add(ospa);
                }
            }
            //Добавление и радактирование
            registryContext.OwnerProcesses.Update(newOwnerProcess);
            registryContext.SaveChanges();
        }

        public object GetAddressInfo(int id, AddressTypes addressType)
        {
            switch (addressType)
            {
                case AddressTypes.Building:
                    var building = registryContext.Buildings.Single(b => b.IdBuilding == id);
                    return new
                    {
                        building.NumRooms,
                        building.TotalArea,
                        building.LivingArea
                    };
                case AddressTypes.Premise:
                    var premise = registryContext.Premises.Single(p => p.IdPremises == id);
                    return new
                    {
                        premise.NumRooms,
                        premise.TotalArea,
                        premise.LivingArea
                    };
                default:
                    var subPremise = registryContext.SubPremises.Single(sp => sp.IdSubPremises == id);
                    return new
                    {
                        NumRooms = 1,
                        subPremise.TotalArea,
                        subPremise.LivingArea
                    };
            }
        }

        public IList<LogOwnerProcess> GetProcessLog(int idProcess)
        {
            var logs =
                registryContext.LogOwnerProcesses
                .Include(l => l.IdUserNavigation)
                .Include(l => l.LogOwnerProcessesValue)
                .Include(l => l.IdLogObjectNavigation)
                .Include(l => l.IdLogTypeNavigation)
                .Where(l => l.IdProcess == idProcess)
                .OrderByDescending(l => l.Date);
            if (logs == null)
                return new List<LogOwnerProcess>();
            return logs.ToList();
        }
    }
}