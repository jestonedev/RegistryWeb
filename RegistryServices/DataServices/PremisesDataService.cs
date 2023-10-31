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
using RegistryServices.ViewModel.RegistryObjects;
using RegistryDb.Models.Entities.Owners;
using RegistryDb.Models.Entities.RegistryObjects.Buildings;
using RegistryDb.Models.Entities.RegistryObjects.Premises;
using RegistryDb.Models.Entities.RegistryObjects.Common.Ownerships;
using RegistryDb.Models.Entities.RegistryObjects.Common.Funds;
using RegistryDb.Models.Entities.RegistryObjects.Common;
using RegistryDb.Models.Entities.RegistryObjects.Common.Restrictions;
using RegistryDb.Models.Entities.Tenancies;
using RegistryServices.DataFilterServices;

namespace RegistryWeb.DataServices
{
    public class PremisesDataService : ListDataService<PremisesVM<Premise>, PremisesListFilter>
    {
        private readonly IConfiguration config;
        private readonly SecurityService securityService;
        private readonly IFilterService<Premise, PremisesListFilter> filterService;

        public PremisesDataService(RegistryContext registryContext, SecurityService securityService, 
            AddressesDataService addressesDataService, IConfiguration config,
            FilterServiceFactory<IFilterService<Premise, PremisesListFilter>> filterServiceFactory) : base(registryContext, addressesDataService)
        {
            this.securityService = securityService;
            this.config = config;
            filterService = filterServiceFactory.CreateInstance();
        }

        public override PremisesVM<Premise> InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, PremisesListFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.KladrRegionsList = new SelectList(addressesDataService.KladrRegions, "IdRegion", "Region");
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
            query = filterService.GetQueryFilter(query, viewModel.FilterOptions);
            query = filterService.GetQueryOrder(query, viewModel.OrderOptions);
            query = filterService.GetQueryIncludes(query);

            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            filteredPremisesIds = query.Select(p => p.IdPremises).ToList();


            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.Premises = filterService.GetQueryPage(query, viewModel.PageOptions).ToList();
            viewModel.PaymentsInfo = GetPaymentInfo(viewModel.Premises);
            viewModel.AreaAvgCostActualDate = registryContext.TotalAreaAvgCosts.FirstOrDefault()?.Date;
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

        public IQueryable<Premise> GetQuery()
        {
            return filterService.GetQueryIncludes(registryContext.Premises);           
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

        public Premise CreatePremise(int ?idBuilding, int? idPremises)        {
            var premise = new Premise {
                RegDate = new DateTime(1999, 10, 29),
                IdBuilding = idBuilding ?? 0,
                IdPremisesDoorKeys = 1,
                IdPremisesComment = 1,
                IdBuildingNavigation = idBuilding != null ? registryContext.Buildings.FirstOrDefault(b => b.IdBuilding == idBuilding.Value)  : null
            };

            if (idPremises != null)
            {
                premise = registryContext.Premises.Include(r => r.IdBuildingNavigation).FirstOrDefault(r => r.IdPremises == idPremises);
                if (premise == null) throw new ApplicationException("Ошибка копирования помещения");
                var canInsert = (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal)) ||
                        (securityService.HasPrivilege(Privileges.RegistryWriteMunicipal) && 
                        ObjectStateHelper.IsMunicipal(premise.IdState) && !premise.SubPremises.Any(sp => !ObjectStateHelper.IsMunicipal(sp.IdState))) ||
                        (securityService.HasPrivilege(Privileges.RegistryWriteNotMunicipal) && 
                        !ObjectStateHelper.IsMunicipal(premise.IdState) && !premise.SubPremises.Any(sp => ObjectStateHelper.IsMunicipal(sp.IdState)));

                if (!canInsert)
                {
                    throw new ApplicationException("Нет прав на копирование этого помещения");
                }
                premise.IdPremises = 0;
            }

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
                PreparersList = new SelectList(registryContext.Preparers, "IdPreparer", "PreparerName"),
                AreaAvgCostActualDate = registryContext.TotalAreaAvgCosts.FirstOrDefault()?.Date
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
                                                        RentArea = tpaRow.RentTotalArea == null ? paymentRow.RentArea : tpaRow.RentTotalArea
                                                    };

            var paymentsAfter28082019Premises = (from paymentRow in prePaymentsAfter28082019Premises
                                                 join tpRow in registryContext.TenancyProcesses.Include(tp => tp.TenancyPersons)
                                                     on paymentRow.IdProcess equals tpRow.IdProcess
                                                 where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                   tpRow.TenancyPersons.Any()
                                                 select paymentRow).ToList();

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
                                                           RentArea = tspaRow.RentTotalArea == null ? paymentRow.RentArea : tspaRow.RentTotalArea
                                                       };

            var paymentsAfter28082019SubPremises = (from paymentRow in prePaymentsAfter28082019SubPremises
                                                    join tpRow in registryContext.TenancyProcesses.Include(tp => tp.TenancyPersons)
                                                        on paymentRow.IdProcess equals tpRow.IdProcess
                                                    where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                      tpRow.TenancyPersons.Any()
                                                    select paymentRow).ToList();

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
                    paymentsInfo.Add(paymentItem);
                }
                paymentItem.Nb = payment.Hb;
                paymentItem.KC = payment.KC;
                paymentItem.K1 = payment.K1;
                paymentItem.K2 = payment.K2;
                paymentItem.K3 = payment.K3;
                paymentItem.PaymentAfter28082019 += Math.Round((payment.K1 + payment.K2 + payment.K3) / 3 * payment.KC * payment.Hb * (decimal)payment.RentArea, 2);
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
                    paymentsInfo.Add(paymentItem);
                }
                paymentItem.Nb = payment.Hb;
                paymentItem.KC = payment.KC;
                paymentItem.K1 = payment.K1;
                paymentItem.K2 = payment.K2;
                paymentItem.K3 = payment.K3;
                paymentItem.PaymentAfter28082019 += Math.Round((payment.K1 + payment.K2 + payment.K3) / 3 * payment.KC * payment.Hb * (decimal)payment.RentArea, 2);
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
            viewModel.Premises = filterService.GetQueryPage(premises, viewModel.PageOptions).ToList();
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
