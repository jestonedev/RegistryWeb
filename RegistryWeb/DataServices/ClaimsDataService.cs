﻿using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.DataHelpers;
using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore;
namespace RegistryWeb.DataServices
{
    public class ClaimsDataService : ListDataService<ClaimsVM, ClaimsFilter>
    {
        private readonly SecurityServices.SecurityService securityService;
        private readonly KumiAccountsDataService kumiAccountsDataService;
        private readonly IConfiguration config;

        public ClaimsDataService(RegistryContext registryContext, SecurityServices.SecurityService securityService,
            AddressesDataService addressesDataService, KumiAccountsDataService kumiAccountsDataService, IConfiguration config) : base(registryContext, addressesDataService)
        {
            this.securityService = securityService;
            this.kumiAccountsDataService = kumiAccountsDataService;
            this.config = config;
        }

        public override ClaimsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, ClaimsFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.Streets = addressesDataService.KladrStreets;
            viewModel.StateTypes = registryContext.ClaimStateTypes;
            viewModel.KladrRegionsList = new SelectList(addressesDataService.KladrRegions, "id_region", "region");
            viewModel.KladrStreetsList = new SelectList(addressesDataService.GetKladrStreets(filterOptions?.IdRegion), "IdStreet", "StreetName");
            return viewModel;
        }

        internal ClaimsVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            ClaimsFilter filterOptions, out List<int> filteredIds)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var payments = GetQuery();
            viewModel.PageOptions.TotalRows = payments.Count();
            var query = GetQueryFilter(payments, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            query = GetQueryIncludes(query);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            filteredIds = query.Select(c => c.IdClaim).ToList();

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            query = GetQueryPage(query, viewModel.PageOptions);
            viewModel.Claims = query.ToList();
            viewModel.RentObjectsBks = GetRentObjectsBks(viewModel.Claims);
            viewModel.TenancyInfoKumi = GetTenancyInfoKumi(viewModel.Claims);
            viewModel.LastPaymentInfo = GetLastPaymentsInfo(viewModel.Claims);
            return viewModel;
        }

        private IQueryable<Claim> GetQuery()
        {
            return registryContext.Claims
                .Include(c => c.ClaimStates)
                .Include(c => c.IdAccountNavigation)
                .Include(c => c.IdAccountAdditionalNavigation)
                .Include(c => c.IdAccountKumiNavigation);
        }

        private IQueryable<Claim> GetQueryIncludes(IQueryable<Claim> query)
        {
            return query
                .Include(c => c.ClaimStates)
                .Include(c => c.IdAccountNavigation)
                .Include(c => c.IdAccountAdditionalNavigation)
                .Include(c => c.IdAccountKumiNavigation);
        }

        private IQueryable<Claim> GetQueryFilter(IQueryable<Claim> query, ClaimsFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = PaymentAccountFilter(query, filterOptions);
                query = ClaimFilter(query, filterOptions);
            }
            return query;
        }

        private Claim FillClaimAmount(Claim claim)
        {
            claim.AmountTenancy = claim.AmountTenancy ?? 0;
            claim.AmountPenalties = claim.AmountPenalties ?? 0;
            claim.AmountDgi = claim.AmountDgi ?? 0;
            claim.AmountPadun = claim.AmountPadun ?? 0;
            claim.AmountPkk = claim.AmountPkk ?? 0;
            return claim;
        }

        internal void Create(Claim claim, List<Microsoft.AspNetCore.Http.IFormFile> files)
        {
            // Прикрепляем документы
            var claimFilesPath = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Claims\");
            if (claim.ClaimFiles != null)
            {
                for (var i = 0; i < claim.ClaimFiles.Count; i++)
                {
                    claim.ClaimFiles[i].FileName = "";
                    var file = files.Where(r => r.Name == "ClaimFile[" + i + "]").FirstOrDefault();
                    if (file == null) continue;
                    claim.ClaimFiles[i].DisplayName = file.FileName;
                    claim.ClaimFiles[i].FileName = Guid.NewGuid().ToString() + "." + new FileInfo(file.FileName).Extension;
                    claim.ClaimFiles[i].MimeType = file.ContentType;
                    var fileStream = new FileStream(Path.Combine(claimFilesPath, claim.ClaimFiles[i].FileName), FileMode.CreateNew);
                    file.OpenReadStream().CopyTo(fileStream);
                    fileStream.Close();
                }
            }
            if (claim.ClaimPersons == null || !claim.ClaimPersons.Any())
            {
                var addressInfix = GetPaymentAccountAddressInfix(claim.IdAccount ?? 0);
                var tenancyPersons = GetTenancyPersonForAddressInfix(addressInfix.Infix);
                claim.ClaimPersons = tenancyPersons.Select(r => new ClaimPerson
                {
                    Surname = r.Surname,
                    Name = r.Name,
                    Patronymic = r.Patronymic,
                    DateOfBirth = r.DateOfBirth,
                    IsClaimer = r.IdKinship == 1
                }).ToList();
            }

            claim.IdAccountNavigation = null;
            claim.IdAccountAdditionalNavigation = null;
            claim.IdAccountKumiNavigation = null;
            claim = FillClaimAmount(claim);
            registryContext.Claims.Add(claim);
            registryContext.SaveChanges();
        }

        private List<TenancyPerson> GetTenancyPersonForAddressInfix(string infix)
        {
            var allObjects = (from row in
                                (from row in registryContext.TenancyBuildingsAssoc
                                 select new
                                 {
                                     row.IdProcess,
                                     Infix = string.Concat("b", row.IdBuilding)
                                 })
                                .Union(from row in registryContext.TenancyPremisesAssoc
                                       select new
                                       {
                                           row.IdProcess,
                                           Infix = string.Concat("p", row.IdPremise)
                                       })
                                .Union(from row in registryContext.TenancySubPremisesAssoc
                                       select new
                                       {
                                           row.IdProcess,
                                           Infix = string.Concat("sp", row.IdSubPremise)
                                       })
                              orderby row.Infix
                              group row.Infix by row.IdProcess into gs
                              select new
                              {
                                  IdProcess = gs.Key,
                                  AddressCode = string.Join("", gs)
                              }).AsEnumerable();
            var tenancyProcesses = (from tpRow in registryContext.TenancyProcesses
                                    .Where(tp => tp.RegistrationNum == null || !tp.RegistrationNum.Contains("н"))
                                    join allObjectsRow in allObjects
                                    on tpRow.IdProcess equals allObjectsRow.IdProcess
                                    where allObjectsRow.AddressCode == infix
                                    select tpRow).ToList();
            if (!tenancyProcesses.Any())
                return new List<TenancyPerson>();

            var idProcess = tenancyProcesses.OrderByDescending(tp => new { tp.RegistrationDate, tp.IdProcess }).First().IdProcess;
            return registryContext.TenancyPersons.Where(tp => tp.IdProcess == idProcess && tp.ExcludeDate == null).ToList();
        }

        private PaymentAddressInfix GetPaymentAccountAddressInfix(int idAccount)
        {
            return (from row in
                            (from row in registryContext.PaymentAccountPremisesAssoc
                             where row.IdAccount == idAccount
                             select new PaymentAddressInfix
                             {
                                 IdAccount = row.IdAccount,
                                 Infix = string.Concat("p", row.IdPremise)
                             }).Union(from row in registryContext.PaymentAccountSubPremisesAssoc
                                      where row.IdAccount == idAccount
                                      select new PaymentAddressInfix
                                      {
                                          IdAccount = row.IdAccount,
                                          Infix = string.Concat("sp", row.IdSubPremise)
                                      })
                    orderby row.Infix
                    group row.Infix by row.IdAccount into gs
                    select new PaymentAddressInfix
                    {
                        IdAccount = gs.Key,
                        Infix = string.Join("", gs)
                    }).FirstOrDefault();
        }

        internal int GetIdJudge(int idAccount)
        {
            var idBuilding = (from papRow in registryContext.PaymentAccountPremisesAssoc
                              join pRow in registryContext.Premises
                              on papRow.IdPremise equals pRow.IdPremises
                              where papRow.IdAccount == idAccount
                              select pRow.IdBuilding).Union(
                                from paspRow in registryContext.PaymentAccountSubPremisesAssoc
                                join spRow in registryContext.SubPremises
                                on paspRow.IdSubPremise equals spRow.IdSubPremises
                                join pRow in registryContext.Premises
                                on spRow.IdPremises equals pRow.IdPremises
                                where paspRow.IdAccount == idAccount
                                select pRow.IdBuilding
                            ).FirstOrDefault();
            return registryContext.JudgeBuildingsAssoc.FirstOrDefault(r => r.IdBuilding == idBuilding)?.IdJudge ?? 0;
        }

        internal void Edit(Claim claim)
        {
            claim.IdAccountNavigation = null;
            claim.IdAccountAdditionalNavigation = null;
            claim.IdAccountKumiNavigation = null;
            claim = FillClaimAmount(claim);
            registryContext.Claims.Update(claim);
            registryContext.SaveChanges();
        }

        internal void Delete(int idClaim)
        {
            var claims = registryContext.Claims
                    .FirstOrDefault(op => op.IdClaim == idClaim);
            if (claims != null)
            {
                claims.Deleted = 1;
                registryContext.SaveChanges();
            }
        }

        internal ClaimsVM GetClaimsViewModelForMassReports(List<int> ids, PageOptions pageOptions)
        {
            var viewModel = InitializeViewModel(null, pageOptions, null);
            var claims = GetClaimsForMassReports(ids);
            viewModel.PageOptions.TotalRows = claims.Count();
            var count = claims.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.Claims = GetQueryPage(claims, viewModel.PageOptions).ToList();
            viewModel.RentObjectsBks = GetRentObjectsBks(viewModel.Claims);
            viewModel.TenancyInfoKumi = GetTenancyInfoKumi(viewModel.Claims);
            viewModel.LastPaymentInfo = GetLastPaymentsInfo(viewModel.Claims);
            return viewModel;
        }

        public IQueryable<Claim> GetClaimsForMassReports(List<int> ids)
        {
            return registryContext.Claims
                .Where(c => ids.Contains(c.IdClaim))
                .Include(c => c.IdAccountNavigation)
                .Include(c => c.IdAccountAdditionalNavigation)
                .Include(c => c.IdAccountKumiNavigation);
        }

        public IQueryable<Claim> AddressFilter(IQueryable<Claim> query, ClaimsFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty() &&
                string.IsNullOrEmpty(filterOptions.IdStreet) &&
                string.IsNullOrEmpty(filterOptions.House) &&
                string.IsNullOrEmpty(filterOptions.PremisesNum) &&
                string.IsNullOrEmpty(filterOptions.IdRegion) &&
                filterOptions.IdBuilding == null &&
                filterOptions.IdPremises == null &&
                filterOptions.IdSubPremises == null)
                return query;

            var addresses = addressesDataService.GetAddressesByText(filterOptions.Address.Text).Select(r => r.Id);

            var premisesAssoc = registryContext.PaymentAccountPremisesAssoc
                .Include(p => p.PremiseNavigation)
                .ThenInclude(b => b.IdBuildingNavigation);

            var subPremisesAssoc = registryContext.PaymentAccountSubPremisesAssoc
                .Include(sp => sp.SubPremiseNavigation)
                .ThenInclude(p => p.IdPremisesNavigation)
                .ThenInclude(b => b.IdBuildingNavigation);

            IEnumerable<int> idAccounts = new List<int>();
            var filtered = false;

            if (filterOptions.Address.AddressType == AddressTypes.Street || !string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                var streets = filterOptions.Address.AddressType == AddressTypes.Street ? addresses : new List<string> { filterOptions.IdStreet };
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => streets.Contains(opa.PremiseNavigation.IdBuildingNavigation.IdStreet))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => streets.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet))
                    .Select(ospa => ospa.IdAccount);
                idAccounts = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            else if (!string.IsNullOrEmpty(filterOptions.IdRegion))
            {
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuildingNavigation.IdStreet.StartsWith(filterOptions.IdRegion))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation
                    .IdBuildingNavigation.IdStreet.StartsWith(filterOptions.IdRegion))
                    .Select(ospa => ospa.IdAccount);
                idAccounts = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            var addressesInt = addresses.Where(a => int.TryParse(a, out int aInt)).Select(a => int.Parse(a));
            if ((filterOptions.Address.AddressType == AddressTypes.Building && addressesInt.Any()) || filterOptions.IdBuilding != null)
            {
                if (filterOptions.IdBuilding != null)
                {
                    addressesInt = new List<int> { filterOptions.IdBuilding.Value };
                }
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => addressesInt.Contains(opa.PremiseNavigation.IdBuilding))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuilding))
                    .Select(ospa => ospa.IdAccount);
                idAccounts = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            if ((filterOptions.Address.AddressType == AddressTypes.Premise && addressesInt.Any()) || filterOptions.IdPremises != null)
            {
                if (filterOptions.IdPremises != null)
                {
                    addressesInt = new List<int> { filterOptions.IdPremises.Value };
                }
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => addressesInt.Contains(opa.PremiseNavigation.IdPremises))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdPremisesNavigation.IdPremises))
                    .Select(ospa => ospa.IdAccount);
                idAccounts = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            if ((filterOptions.Address.AddressType == AddressTypes.SubPremise && addressesInt.Any()) || filterOptions.IdSubPremises != null)
            {
                if (filterOptions.IdSubPremises != null)
                {
                    addressesInt = new List<int> { filterOptions.IdSubPremises.Value };
                }
                idAccounts = subPremisesAssoc
                    .Where(ospa => addressesInt.Contains(ospa.SubPremiseNavigation.IdSubPremises))
                    .Select(ospa => ospa.IdAccount);
                filtered = true;
            }
            if (filtered)
            {
                query = from q in query
                        join idAccount in idAccounts on q.IdAccount equals idAccount
                        select q;
            }
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuildingNavigation.House.ToLowerInvariant().Equals(filterOptions.House.ToLowerInvariant()))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.House.ToLowerInvariant().Equals(filterOptions.House.ToLowerInvariant()))
                    .Select(ospa => ospa.IdAccount);
                query = from q in query
                        join idAccount in idPremiseAccounts.Union(idSubPremiseAccounts) on q.IdAccount equals idAccount
                        select q;
            }
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.PremisesNum.ToLowerInvariant().Equals(filterOptions.PremisesNum.ToLowerInvariant()))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.PremisesNum.ToLowerInvariant().Equals(filterOptions.PremisesNum.ToLowerInvariant()))
                    .Select(ospa => ospa.IdAccount);
                query = from q in query
                        join idAccount in idPremiseAccounts.Union(idSubPremiseAccounts) on q.IdAccount equals idAccount
                        select q;
            }
            return query;
        }

        internal void AddClaimStateMass(List<int> ids, ClaimState claimState)
        {
            foreach (var id in ids)
            {
                var claimStateCopy = JsonConvert.DeserializeObject<ClaimState>(JsonConvert.SerializeObject(claimState));
                claimStateCopy.IdClaim = id;
                registryContext.ClaimStates.Add(claimStateCopy);
            }
            registryContext.SaveChanges();
        }

        internal void UpdateDeptPeriodInClaims(List<int> claimIds, DateTime? startDeptPeriod, DateTime? endDeptPeriod)
        {
            var claims = registryContext.Claims.Where(r => claimIds.Contains(r.IdClaim));
            foreach (var claim in claims)
            {
                claim.StartDeptPeriod = startDeptPeriod;
                claim.EndDeptPeriod = endDeptPeriod;
            }
            registryContext.SaveChanges();
        }

        internal PaymentAccount GetAccountBks(int idAccount)
        {
            return registryContext.PaymentAccounts
                .FirstOrDefault(pa => pa.IdAccount == idAccount);
        }

        internal KumiAccount GetAccountKumi(int idAccount)
        {
            return registryContext.KumiAccounts
                .FirstOrDefault(a => a.IdAccount == idAccount);
        }

        internal IList<AccountBase> GetAccounts(string text, string type)
        {
            if (type == "BKS")
                return registryContext.PaymentAccounts.Where(pa => pa.Account.Contains(text)).Take(100).Select(r => (AccountBase)r).ToList();
            else
            if (type == "KUMI")
                return registryContext.KumiAccounts.Where(pa => pa.Account.Contains(text)).Take(100).Select(r => (AccountBase)r).ToList();
            return null;
        }

        public IQueryable<Claim> PaymentAccountFilter(IQueryable<Claim> query, ClaimsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.Crn))
            {
                query = query.Where(p => p.IdAccountNavigation.Crn.Contains(filterOptions.Crn) 
                    || p.IdAccountAdditionalNavigation.Crn.Contains(filterOptions.Crn));
            }
            if (!string.IsNullOrEmpty(filterOptions.Account))
            {
                query = query.Where(p => p.IdAccountNavigation.Account.Contains(filterOptions.Account) 
                    || p.IdAccountAdditionalNavigation.Account.Contains(filterOptions.Account)
                    || p.IdAccountKumiNavigation.Account.Contains(filterOptions.Account));
            }
            if (!string.IsNullOrEmpty(filterOptions.RawAddress))
            {
                query = query.Where(p => p.IdAccountNavigation.RawAddress.Contains(filterOptions.RawAddress) 
                    || p.IdAccountAdditionalNavigation.RawAddress.Contains(filterOptions.RawAddress));
            }
            return query;
        }

        private List<int> GetAccountIdsWithSameAddress(int idAccount)
        {
            var paymentAddressInfix = GetPaymentAccountAddressInfix(idAccount);

            var filteredObjects = paymentAddressInfix != null ? new List<PaymentAddressInfix> { paymentAddressInfix } : new List<PaymentAddressInfix>();

            var allObjects = (from row in (from row in registryContext.PaymentAccountPremisesAssoc
                                           select new PaymentAddressInfix
                                           {
                                               IdAccount = row.IdAccount,
                                               Infix = string.Concat("p", row.IdPremise)
                                           }).Union(from row in registryContext.PaymentAccountSubPremisesAssoc
                                                    select new PaymentAddressInfix
                                                    {
                                                        IdAccount = row.IdAccount,
                                                        Infix = string.Concat("sp", row.IdSubPremise)
                                                    })
                              orderby row.Infix
                              group row.Infix by row.IdAccount into gs
                              select new
                              {
                                  IdAccount = gs.Key,
                                  AddressCode = string.Join("", gs)
                              }).AsEnumerable();
            allObjects = from paymentsRow in registryContext.PaymentAccounts
                         join allObjectsRow in allObjects
                         on paymentsRow.IdAccount equals allObjectsRow.IdAccount into ao
                         from aoRow in ao.DefaultIfEmpty()
                         select new
                         {
                             paymentsRow.IdAccount,
                             AddressCode = aoRow != null ? aoRow.AddressCode : paymentsRow.RawAddress
                         };
            return (from filteredRow in filteredObjects
                    join allRow in allObjects
                    on filteredRow.Infix equals allRow.AddressCode
                    select allRow.IdAccount).ToList();
        }

        private bool DateComparison(ComparisonSignEnum sign, DateTime? date, DateTime? dateFrom, DateTime? dateTo)
        {
            switch (sign)
            {
                case ComparisonSignEnum.GreaterThanOrEqual:
                    return date >= dateFrom;
                case ComparisonSignEnum.LessThanOrEqual:
                    return date <= dateFrom;
                case ComparisonSignEnum.Equal:
                    return date == dateFrom;
                case ComparisonSignEnum.Between:
                    return date >= dateFrom && date <= dateTo;
            }
            return false;
        }

        public IQueryable<Claim> ClaimFilter(IQueryable<Claim> query, ClaimsFilter filterOptions)
        {
            if (filterOptions.IdAccountBks != null)
            {
                var ids = GetAccountIdsWithSameAddress(filterOptions.IdAccountBks.Value);
                query = query.Where(p => ids.Contains(p.IdAccount ?? 0) || p.IdAccount == filterOptions.IdAccountBks || p.IdAccountAdditional == filterOptions.IdAccountBks);
            }
            if (filterOptions.IdAccountKumi != null)
            {
                query = query.Where(p => p.IdAccountKumi == filterOptions.IdAccountKumi);
            }

            if (filterOptions.AmountTotal != null)
            {
                query = query.Where(p => filterOptions.AmountTotalOp == 1 ?
                    p.AmountTenancy + p.AmountPenalties + p.AmountDgi + p.AmountPadun + p.AmountPkk >= filterOptions.AmountTotal :
                    p.AmountTenancy + p.AmountPenalties + p.AmountDgi + p.AmountPadun + p.AmountPkk <= filterOptions.AmountTotal);
            }
            if (filterOptions.AmountTenancy != null)
            {
                query = query.Where(p => filterOptions.AmountTenancyOp == 1 ?
                    p.AmountTenancy >= filterOptions.AmountTenancy :
                    p.AmountTenancy <= filterOptions.AmountTenancy);
            }
            if (filterOptions.AmountPenalties != null)
            {
                query = query.Where(p => filterOptions.AmountPenaltiesOp == 1 ?
                    p.AmountPenalties >= filterOptions.AmountPenalties :
                    p.AmountPenalties <= filterOptions.AmountPenalties);
            }
            if (filterOptions.AmountDgiPadunPkk != null)
            {
                query = query.Where(p => filterOptions.AmountDgiPadunPkkOp == 1 ?
                    (p.AmountDgi >= filterOptions.AmountDgiPadunPkk ||
                     p.AmountPadun >= filterOptions.AmountDgiPadunPkk ||
                     p.AmountPkk >= filterOptions.AmountDgiPadunPkk) :
                    (p.AmountDgi <= filterOptions.AmountDgiPadunPkk ||
                     p.AmountPadun <= filterOptions.AmountDgiPadunPkk ||
                     p.AmountPkk <= filterOptions.AmountDgiPadunPkk));
            }

            if (filterOptions.AtDate != null)
            {
                query = query.Where(p => p.AtDate == filterOptions.AtDate);
            }

            if (!string.IsNullOrEmpty(filterOptions.CourtOrderNum))
            {
                var idClaims = registryContext.ClaimStates.Where(cs => cs.IdStateType == 4 && cs.CourtOrderNum.Contains(filterOptions.CourtOrderNum)).Select(r => r.IdClaim).ToList();
                query = query.Where(p => idClaims.Contains(p.IdClaim));
            }
            

            if (filterOptions.IdClaimState != null)
            {
                if (filterOptions.IsCurrentState)
                {
                    var maxDateClaimStates =
                        from row in registryContext.ClaimStates
                        group row.IdState by row.IdClaim into gs
                        select new
                        {
                            IdClaim = gs.Key,
                            IdState = gs.Max()
                        };
                    var lastClaimsStates =
                        (from row in registryContext.ClaimStates
                        join maxDateClaimStatesRow in maxDateClaimStates
                        on row.IdState equals maxDateClaimStatesRow.IdState
                        select new
                        {
                            row.IdClaim,
                            row.IdStateType,
                            row.DateStartState,
                            row.ClaimDirectionDate,
                            row.CourtOrderDate,
                            row.ObtainingCourtOrderDate
                        }).ToList();

                    query = from row in query
                            join lastClaimsStatesRow in lastClaimsStates
                            on row.IdClaim equals lastClaimsStatesRow.IdClaim
                            where lastClaimsStatesRow.IdStateType == filterOptions.IdClaimState
                            select row;

                    if (filterOptions.ClaimDirectionDateFrom != null)
                    {
                        query = from row in query
                                join lastClaimsStatesRow in lastClaimsStates
                                on row.IdClaim equals lastClaimsStatesRow.IdClaim
                                where DateComparison(
                                    filterOptions.ClaimDirectionDateOp,
                                    lastClaimsStatesRow.ClaimDirectionDate,
                                    filterOptions.ClaimDirectionDateFrom,
                                    filterOptions.ClaimDirectionDateTo)
                                select row;
                    }

                    if (filterOptions.CourtOrderDateFrom != null)
                    {
                        query = from row in query
                                join lastClaimsStatesRow in lastClaimsStates
                                on row.IdClaim equals lastClaimsStatesRow.IdClaim
                                where DateComparison(
                                    filterOptions.CourtOrderDateOp,
                                    lastClaimsStatesRow.CourtOrderDate,
                                    filterOptions.CourtOrderDateFrom,
                                    filterOptions.CourtOrderDateTo)
                                select row;
                    }

                    if (filterOptions.ObtainingCourtOrderDateFrom != null)
                    {
                        query = from row in query
                                join lastClaimsStatesRow in lastClaimsStates
                                on row.IdClaim equals lastClaimsStatesRow.IdClaim
                                where DateComparison(
                                    filterOptions.ObtainingCourtOrderDateOp,
                                    lastClaimsStatesRow.ObtainingCourtOrderDate,
                                    filterOptions.ObtainingCourtOrderDateFrom,
                                    filterOptions.ObtainingCourtOrderDateTo)
                                select row;
                    }
                }
                else
                {
                    query = from row in query
                            join claimsStatesRow in registryContext.ClaimStates
                            on row.IdClaim equals claimsStatesRow.IdClaim
                            where claimsStatesRow.IdStateType == filterOptions.IdClaimState
                            select row;

                    if (filterOptions.ClaimDirectionDateFrom != null)
                    {
                        query = from row in query
                                join claimsStatesRow in registryContext.ClaimStates
                                on row.IdClaim equals claimsStatesRow.IdClaim
                                where DateComparison(
                                    filterOptions.ClaimDirectionDateOp,
                                    claimsStatesRow.ClaimDirectionDate,
                                    filterOptions.ClaimDirectionDateFrom,
                                    filterOptions.ClaimDirectionDateTo)
                                select row;
                    }

                    if (filterOptions.CourtOrderDateFrom != null)
                    {
                        query = from row in query
                                join claimsStatesRow in registryContext.ClaimStates
                                on row.IdClaim equals claimsStatesRow.IdClaim
                                where DateComparison(
                                    filterOptions.CourtOrderDateOp,
                                    claimsStatesRow.CourtOrderDate,
                                    filterOptions.CourtOrderDateFrom,
                                    filterOptions.CourtOrderDateTo)
                                select row;
                    }

                    if (filterOptions.ObtainingCourtOrderDateFrom != null)
                    {
                        query = from row in query
                                join claimsStatesRow in registryContext.ClaimStates
                                on row.IdClaim equals claimsStatesRow.IdClaim
                                where DateComparison(
                                    filterOptions.ObtainingCourtOrderDateOp,
                                    claimsStatesRow.ObtainingCourtOrderDate,
                                    filterOptions.ObtainingCourtOrderDateFrom,
                                    filterOptions.ObtainingCourtOrderDateTo)
                                select row;
                    }
                }
            }

            if (filterOptions.ClaimStateDateFrom != null)
            {
                query = from row in query
                        join claimsStatesRow in registryContext.ClaimStates
                        on row.IdClaim equals claimsStatesRow.IdClaim
                        where DateComparison(
                            filterOptions.ClaimStateDateOp,
                            claimsStatesRow.DateStartState,
                            filterOptions.ClaimStateDateFrom,
                            filterOptions.ClaimStateDateTo)
                        select row;
            }

            if (filterOptions.BalanceOutputTotal != null || filterOptions.BalanceOutputTenancy != null ||
                filterOptions.BalanceOutputPenalties != null || filterOptions.BalanceOutputDgiPadunPkk != null)
            {
                var maxDatePayments = from row in registryContext.Payments
                                      group row.Date by row.IdAccount into gs
                                      select new
                                      {
                                          IdAccount = gs.Key,
                                          Date = gs.Max()
                                      };

                var lastPayments = (from row in registryContext.Payments
                                    join maxDatePaymentsRow in maxDatePayments
                                    on new { row.IdAccount, row.Date } equals new { maxDatePaymentsRow.IdAccount, maxDatePaymentsRow.Date }
                                    select new
                                    {
                                        row.IdAccount,
                                        row.BalanceOutputTotal,
                                        row.BalanceOutputTenancy,
                                        row.BalanceOutputPenalties,
                                        row.BalanceOutputDgi,
                                        row.BalanceOutputPkk,
                                        row.BalanceOutputPadun,
                                    }).ToList();

                if (filterOptions.BalanceOutputTotal != null)
                {
                    query = from row in query
                            join lastPaymentsRow in lastPayments
                            on row.IdAccount equals lastPaymentsRow.IdAccount
                            where filterOptions.BalanceOutputTotalOp == 1 ?
                               lastPaymentsRow.BalanceOutputTotal >= filterOptions.BalanceOutputTotal :
                               lastPaymentsRow.BalanceOutputTotal <= filterOptions.BalanceOutputTotal
                            select row;
                }
                if (filterOptions.BalanceOutputTenancy != null)
                {
                    query = from row in query
                            join lastPaymentsRow in lastPayments
                            on row.IdAccount equals lastPaymentsRow.IdAccount
                            where filterOptions.BalanceOutputTenancyOp == 1 ?
                               lastPaymentsRow.BalanceOutputTenancy >= filterOptions.BalanceOutputTenancy :
                               lastPaymentsRow.BalanceOutputTenancy <= filterOptions.BalanceOutputTenancy
                            select row;
                }
                if (filterOptions.BalanceOutputPenalties != null)
                {
                    query = from row in query
                            join lastPaymentsRow in lastPayments
                            on row.IdAccount equals lastPaymentsRow.IdAccount
                            where filterOptions.BalanceOutputPenaltiesOp == 1 ?
                               lastPaymentsRow.BalanceOutputPenalties >= filterOptions.BalanceOutputPenalties :
                               lastPaymentsRow.BalanceOutputPenalties <= filterOptions.BalanceOutputPenalties
                            select row;
                }
                if (filterOptions.BalanceOutputDgiPadunPkk != null)
                {
                    query = from row in query
                            join lastPaymentsRow in lastPayments
                            on row.IdAccount equals lastPaymentsRow.IdAccount
                            where filterOptions.BalanceOutputDgiPadunPkkOp == 1 ?
                                (lastPaymentsRow.BalanceOutputDgi >= filterOptions.BalanceOutputDgiPadunPkk ||
                                 lastPaymentsRow.BalanceOutputPadun >= filterOptions.BalanceOutputDgiPadunPkk ||
                                 lastPaymentsRow.BalanceOutputPkk >= filterOptions.BalanceOutputDgiPadunPkk) :
                                (lastPaymentsRow.BalanceOutputDgi <= filterOptions.BalanceOutputDgiPadunPkk ||
                                 lastPaymentsRow.BalanceOutputPadun <= filterOptions.BalanceOutputDgiPadunPkk ||
                                 lastPaymentsRow.BalanceOutputPkk <= filterOptions.BalanceOutputDgiPadunPkk)
                            select row;
                }
            }
            return query;
        }

        private IQueryable<Claim> GetQueryOrder(IQueryable<Claim> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField) || orderOptions.OrderField == "IdClaim")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdClaim);
                else
                    return query.OrderByDescending(p => p.IdClaim);
            }
            if (orderOptions.OrderField == "AtDate")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.AtDate);
                else
                    return query.OrderByDescending(p => p.AtDate);
            }
            if (orderOptions.OrderField == "Address")
            {
                var addresses =
                    registryContext.PaymentAccountPremisesAssoc
                    .Include(r => r.PremiseNavigation)
                    .ThenInclude(r => r.IdBuildingNavigation)
                    .ThenInclude(r => r.IdStreetNavigation)
                    .Select(
                    p => new
                    {
                        p.IdAccount,
                        Address = string.Concat(p.PremiseNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName, ", ",
                            p.PremiseNavigation.IdBuildingNavigation.House, ", ", p.PremiseNavigation.PremisesNum)
                    })
                .Union(registryContext.PaymentAccountSubPremisesAssoc
                    .Include(r => r.SubPremiseNavigation)
                    .ThenInclude(r => r.IdPremisesNavigation)
                    .ThenInclude(r => r.IdBuildingNavigation)
                    .ThenInclude(r => r.IdStreetNavigation)
                    .Select(
                    sp => new
                    {
                        sp.IdAccount,
                        Address = string.Concat(sp.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName, ", ",
                            sp.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.House, ", ",
                            sp.SubPremiseNavigation.IdPremisesNavigation.PremisesNum, ", ", sp.SubPremiseNavigation.SubPremisesNum)
                    }));
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                {
                    return (from row in query
                            join addr in addresses
                             on row.IdAccount equals addr.IdAccount into aq
                            from aqRow in aq.DefaultIfEmpty()
                            orderby aqRow.Address
                            select row).Distinct();
                }
                else
                {
                    return (from row in query
                            join addr in addresses
                             on row.IdAccount equals addr.IdAccount into aq
                            from aqRow in aq.DefaultIfEmpty()
                            orderby aqRow.Address descending
                            select row).Distinct();
                }
            }
            return query;
        }

        private IQueryable<Claim> GetQueryPage(IQueryable<Claim> query, PageOptions pageOptions)
        {
            return query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
        }


        internal Dictionary<int, List<Address>> GetRentObjectsBks(IEnumerable<int> idAccounts)
        {
            var premises = from paRow in registryContext.PaymentAccountPremisesAssoc
                           join premiseRow in registryContext.Premises
                           on paRow.IdPremise equals premiseRow.IdPremises
                           join buildingRow in registryContext.Buildings
                           on premiseRow.IdBuilding equals buildingRow.IdBuilding
                           join streetRow in registryContext.KladrStreets
                           on buildingRow.IdStreet equals streetRow.IdStreet
                           join premiseTypesRow in registryContext.PremisesTypes
                           on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                           where idAccounts.Contains(paRow.IdAccount)
                           select new
                           {
                               paRow.IdAccount,
                               Address = new Address
                               {
                                   AddressType = AddressTypes.Premise,
                                   Id = premiseRow.IdPremises.ToString(),
                                   IdParents = new Dictionary<string, string>
                                       {
                                           { AddressTypes.Street.ToString(), buildingRow.IdStreet },
                                           { AddressTypes.Building.ToString(), buildingRow.IdBuilding.ToString() }
                                       },
                                   Text = string.Concat(streetRow.StreetName, ", д.", buildingRow.House, ", ",
                                        premiseTypesRow.PremisesTypeShort, premiseRow.PremisesNum)
                               }
                           };
            var subPremises = from paRow in registryContext.PaymentAccountSubPremisesAssoc
                              join subPremiseRow in registryContext.SubPremises
                              on paRow.IdSubPremise equals subPremiseRow.IdSubPremises
                              join premiseRow in registryContext.Premises
                              on subPremiseRow.IdPremises equals premiseRow.IdPremises
                              join buildingRow in registryContext.Buildings
                              on premiseRow.IdBuilding equals buildingRow.IdBuilding
                              join streetRow in registryContext.KladrStreets
                              on buildingRow.IdStreet equals streetRow.IdStreet
                              join premiseTypesRow in registryContext.PremisesTypes
                              on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                              where idAccounts.Contains(paRow.IdAccount)
                              select new
                              {
                                  paRow.IdAccount,
                                  Address = new Address
                                  {
                                      AddressType = AddressTypes.SubPremise,
                                      Id = subPremiseRow.IdSubPremises.ToString(),
                                      IdParents = new Dictionary<string, string>
                                           {
                                              { AddressTypes.Street.ToString(), buildingRow.IdStreet },
                                              { AddressTypes.Building.ToString(), buildingRow.IdBuilding.ToString() },
                                              { AddressTypes.Premise.ToString(), premiseRow.IdPremises.ToString() }
                                           },
                                      Text = string.Concat(streetRow.StreetName, ", д.", buildingRow.House, ", ",
                                            premiseTypesRow.PremisesTypeShort, premiseRow.PremisesNum, ", к.", subPremiseRow.SubPremisesNum)
                                  }
                              };

            var objects = premises.Union(subPremises).ToList();

            var result =
                objects.GroupBy(r => r.IdAccount)
                .Select(r => new { IdAccount = r.Key, Addresses = r.Select(v => v.Address) })
                .ToDictionary(v => v.IdAccount, v => v.Addresses.ToList());
            return result;
        }

        internal Dictionary<int, List<Address>> GetRentObjectsBks(IEnumerable<Claim> claims)
        {
            var ids = claims.Where(r => r.IdAccount != null).Select(r => r.IdAccount.Value).Distinct();
            return GetRentObjectsBks(ids);
        }

        internal Dictionary<int, List<KumiAccountTenancyInfoVM>> GetTenancyInfoKumi(IEnumerable<Claim> claims)
        {
            var accounts = claims.Where(r => r.IdAccountKumi != null).Select(r => r.IdAccountKumiNavigation).Distinct();
            return kumiAccountsDataService.GetTenancyInfo(accounts);
        }

        internal Dictionary<int, List<KumiAccountTenancyInfoVM>> GetTenancyInfoKumi(IEnumerable<int> idAccounts)
        {
            var accounts = idAccounts.Select(r => new KumiAccount {
                IdAccount = r
            } ).Distinct();
            return kumiAccountsDataService.GetTenancyInfo(accounts);
        }

        internal Dictionary<int, Payment> GetLastPaymentsInfo(IEnumerable<Claim> claims)
        {
            var ids = claims.Where(r => r.IdAccount != null).Select(r => r.IdAccount.Value).Distinct();
            return GetLastPaymentsInfo(ids);
        }

        internal Dictionary<int, Payment> GetLastPaymentsInfo(IEnumerable<int> idsAccount)
        {
            var maxDatePayments = from row in registryContext.Payments
                                  where idsAccount.Contains(row.IdAccount)
                                  group row.Date by row.IdAccount into gs
                                  select new
                                  {
                                      IdAccount = gs.Key,
                                      Date = gs.Max()
                                  };

            var lastPayments = from row in registryContext.Payments
                               join maxDatePaymentsRow in maxDatePayments
                               on new { row.IdAccount, row.Date } equals new { maxDatePaymentsRow.IdAccount, maxDatePaymentsRow.Date }
                               select row;

            var result =
                lastPayments.GroupBy(r => r.IdAccount)
                .Select(r => new { IdAccount = r.Key, Payment = r.FirstOrDefault() })
                .Where(r => r.Payment != null)
                .ToDictionary(v => v.IdAccount, v => v.Payment);
            return result;
        }

        internal ClaimVM CreateClaimEmptyViewModel(int? idAccountBks = null, int? idAccountKumi = null, [CallerMemberName]string action = "")
        {
            var lastPaymentInfo = idAccountBks != null ?
                    GetLastPaymentsInfo(new List<int> { idAccountBks.Value }).Select(v => v.Value).FirstOrDefault() : null;
            KumiAccount kumiAccount = null;
            if (idAccountKumi != null)
                kumiAccount = GetAccountKumi(idAccountKumi.Value);

            return new ClaimVM
            {
                Claim = new Claim
                {
                    IdAccount = idAccountBks ?? 0,
                    IdAccountNavigation = idAccountBks != null ? GetAccountBks(idAccountBks.Value) : null,
                    IdAccountKumi = idAccountKumi ?? 0,
                    IdAccountKumiNavigation = kumiAccount,
                    AmountTenancy = kumiAccount != null ? kumiAccount.CurrentBalanceTenancy : lastPaymentInfo?.BalanceOutputTenancy,
                    AmountPenalties = kumiAccount != null ? kumiAccount.CurrentBalancePenalty : lastPaymentInfo?.BalanceOutputPenalties,
                    AmountDgi = kumiAccount != null ? null : lastPaymentInfo?.BalanceOutputDgi,
                    AmountPadun = kumiAccount != null ? null : lastPaymentInfo?.BalanceOutputPadun,
                    AmountPkk = kumiAccount != null ? null : lastPaymentInfo?.BalanceOutputPkk
                },
                RentObjectsBks = idAccountBks != null ?
                    GetRentObjectsBks(new List<int> { idAccountBks.Value }).SelectMany(v => v.Value).ToList() : null,
                TenancyInfoKumi = idAccountKumi != null ? GetTenancyInfoKumi(new List<int> { idAccountKumi.Value }).SelectMany(r => r.Value).ToList() : null,
                CurrentExecutor = CurrentExecutor,
                StateTypes = registryContext.ClaimStateTypes.ToList(),
                Executors = registryContext.Executors.ToList(),
                Judges = registryContext.Judges.Where(r => action == "Create" || !r.IsInactive).ToList(),
                Signers = registryContext.SelectableSigners.Where(r => r.IdSignerGroup == 3).ToList(),
                StateTypeRelations = registryContext.ClaimStateTypeRelations.ToList()
            };
        }

        internal ClaimVM GetClaimViewModel(Claim claim, [CallerMemberName]string action = "")
        {
            var claimVM = CreateClaimEmptyViewModel(null, null, action);
            claimVM.Claim = claim;
            claimVM.RentObjectsBks = GetRentObjectsBks(new List<Claim> { claim }).SelectMany(r => r.Value).ToList();
            claimVM.LastPaymentInfo = GetLastPaymentsInfo(new List<Claim> { claim }).Select(r => r.Value).FirstOrDefault();
            claimVM.TenancyInfoKumi = GetTenancyInfoKumi(new List<Claim> { claim }).SelectMany(r => r.Value).ToList();
            return claimVM;
        }

        public Claim GetClaim(int idClaim)
        {
            return registryContext.Claims
                .Include(r => r.IdAccountNavigation)
                .Include(r => r.IdAccountAdditionalNavigation)
                .Include(r => r.IdAccountKumiNavigation)
                .Include(r => r.ClaimStates)
                .Include(r => r.ClaimPersons)
                .Include(r => r.ClaimFiles)
                .Include(r => r.ClaimCourtOrders)
                .FirstOrDefault(r => r.IdClaim == idClaim);
        }

        public List<ClaimState> GetClaimStates(int idClaim)
        {
            return registryContext.ClaimStates.Where(cs => cs.IdClaim == idClaim).AsNoTracking().ToList();
        }

        public List<ClaimStateType> StateTypes => registryContext.ClaimStateTypes.ToList();
        public List<ClaimStateTypeRelation> StateTypeRelations => registryContext.ClaimStateTypeRelations.ToList();
        public List<Executor> Executors => registryContext.Executors.ToList();
        public List<Judge> Judges => registryContext.Judges.ToList();
        public List<SelectableSigner> Signers => registryContext.SelectableSigners.ToList();

        public Executor CurrentExecutor
        {
            get
            {
                var userName = securityService.User.UserName.ToLowerInvariant();
                return registryContext.Executors.FirstOrDefault(e => e.ExecutorLogin != null &&
                                e.ExecutorLogin.ToLowerInvariant() == userName);
            }
        }
    }
}