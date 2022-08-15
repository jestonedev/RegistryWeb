using RegistryWeb.Models;
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
        private readonly IConfiguration config;

        public ClaimsDataService(RegistryContext registryContext, SecurityServices.SecurityService securityService,
            AddressesDataService addressesDataService, IConfiguration config) : base(registryContext, addressesDataService)
        {
            this.securityService = securityService;
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
            viewModel.RentObjects = GetRentObjects(viewModel.Claims);
            viewModel.LastPaymentInfo = GetLastPaymentsInfo(viewModel.Claims);
            return viewModel;
        }

        private IQueryable<Claim> GetQuery()
        {
            return registryContext.Claims
                .Include(c => c.ClaimStates)
                .Include(c => c.IdAccountNavigation)
                .Include(c => c.IdAccountAdditionalNavigation);
        }

        private IQueryable<Claim> GetQueryIncludes(IQueryable<Claim> query)
        {
            return query
                .Include(c => c.ClaimStates)
                .Include(c => c.ClaimPersons)
                .Include(c => c.IdAccountNavigation)
                .Include(c => c.IdAccountAdditionalNavigation);
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

        internal void Create(Claim claim, List<Microsoft.AspNetCore.Http.IFormFile> files, LoadPersonsSourceEnum loadPersonsSource)
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
            if (claim.ClaimPersons == null)
                claim.ClaimPersons = new List<ClaimPerson>();
            switch (loadPersonsSource)
            {
                case LoadPersonsSourceEnum.None:
                    break;
                case LoadPersonsSourceEnum.Tenancy:
                    claim.ClaimPersons = claim.ClaimPersons.Union(GetClaimPersonsFromTenancy(claim.IdAccount)).ToList();
                    break;
                case LoadPersonsSourceEnum.PrevClaim:
                    claim.ClaimPersons = claim.ClaimPersons.Union(GetClaimPersonsFromPrevClaim(claim.IdAccount)).ToList();           
                    break;
            }

            claim.IdAccountNavigation = null;
            claim.IdAccountAdditionalNavigation = null;
            claim = FillClaimAmount(claim);
            registryContext.Claims.Add(claim);
            registryContext.SaveChanges();
        }

        public List<ClaimPerson> GetClaimPersonsFromPrevClaim(int idAccount)
        {
            var prevClaim = registryContext.Claims.Include(r => r.ClaimPersons).Where(r => r.IdAccount == idAccount).OrderByDescending(r => r.IdClaim).FirstOrDefault();
            if (prevClaim != null)
            {
                return prevClaim.ClaimPersons.Select(r => new ClaimPerson
                {
                    Surname = r.Surname,
                    Name = r.Name,
                    Patronymic = r.Patronymic,
                    DateOfBirth = r.DateOfBirth,
                    Passport = r.Passport,
                    PlaceOfBirth = r.PlaceOfBirth,
                    WorkPlace = r.WorkPlace,
                    IsClaimer = r.IsClaimer
                }).ToList();
            }
            return new List<ClaimPerson>();
        }

        public List<ClaimPerson> GetClaimPersonsFromTenancy(int idAccount)
        {
            var addressInfix = GetPaymentAccountAddressInfix(idAccount);
            var tenancyPersons = GetTenancyPersonForAddressInfix(addressInfix.Infix);
            return tenancyPersons.Select(r => new ClaimPerson
            {
                Surname = r.Surname,
                Name = r.Name,
                Patronymic = r.Patronymic,
                DateOfBirth = r.DateOfBirth,
                IsClaimer = r.IdKinship == 1
            }).ToList();
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

            var idProcess = tenancyProcesses.OrderByDescending(tp => tp.RegistrationDate).ThenByDescending(tp => tp.IdProcess ).First().IdProcess;
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
            viewModel.RentObjects = GetRentObjects(viewModel.Claims);
            return viewModel;
        }

        public IQueryable<Claim> GetClaimsForMassReports(List<int> ids)
        {
            return registryContext.Claims
                .Where(c => ids.Contains(c.IdClaim)).Include(c => c.IdAccountNavigation);
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

        internal PaymentAccount GetAccount(int idAccount)
        {
            return registryContext.PaymentAccounts
                .FirstOrDefault(pa => pa.IdAccount == idAccount);
        }

        internal IList<PaymentAccount> GetAccounts(string text)
        {
            return registryContext.PaymentAccounts
                .Where(pa => pa.Account.Contains(text)).Take(100).ToList();
        }

        public IQueryable<Claim> PaymentAccountFilter(IQueryable<Claim> query, ClaimsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.Crn))
            {
                query = query.Where(p => p.IdAccountNavigation.Crn.Contains(filterOptions.Crn));
            }
            if (!string.IsNullOrEmpty(filterOptions.Account))
            {
                query = query.Where(p => p.IdAccountNavigation.Account.Contains(filterOptions.Account));
            }
            if (!string.IsNullOrEmpty(filterOptions.RawAddress))
            {
                query = query.Where(p => p.IdAccountNavigation.RawAddress.Contains(filterOptions.RawAddress));
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
            if (filterOptions.IdAccount != null)
            {
                var ids = GetAccountIdsWithSameAddress(filterOptions.IdAccount.Value);
                query = query.Where(p => ids.Contains(p.IdAccount) || p.IdAccount == filterOptions.IdAccount || p.IdAccountAdditional == filterOptions.IdAccount);
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

            if (filterOptions.ClaimFormStatementSSPDateFrom != null)
            {
                var idClaimsLogs = (from log in registryContext.LogClaimStatementInSpp
                                    where DateComparison(
                                        filterOptions.ClaimFormStatementSSPDateOp,
                                        log.CreateDate,
                                        filterOptions.ClaimFormStatementSSPDateFrom,
                                        filterOptions.ClaimFormStatementSSPDateTo)
                                    select log.IdClaim).ToList();

                query = from row in query
                        where idClaimsLogs.Contains(row.IdClaim)
                        select row;
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


        internal Dictionary<int, List<Address>> GetRentObjects(IEnumerable<int> idAccounts)
        {
            var premises = from paRow in registryContext.PaymentAccountPremisesAssoc
                           join premiseRow in registryContext.Premises.Include(r => r.IdStateNavigation)
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
                                   ObjectState = premiseRow.IdStateNavigation,
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
                              join subPremiseRow in registryContext.SubPremises.Include(r => r.IdStateNavigation)
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
                                      ObjectState = subPremiseRow.IdStateNavigation,
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

        internal Dictionary<int, List<Address>> GetRentObjects(IEnumerable<Claim> claims)
        {
            var ids = claims.Select(r => r.IdAccount).Distinct();
            return GetRentObjects(ids);
        }

        internal Dictionary<int, Payment> GetLastPaymentsInfo(IEnumerable<Claim> claims)
        {
            var ids = claims.Select(r => r.IdAccount).Union(claims.Where(r => r.IdAccountAdditional != null).Select(r => r.IdAccountAdditional.Value)).Distinct();
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

        internal ClaimVM CreateClaimEmptyViewModel(int? idAccount = null, [CallerMemberName]string action = "")
        {
            var lastPaymentInfo = idAccount != null ?
                    GetLastPaymentsInfo(new List<int> { idAccount.Value }).Select(v => v.Value).FirstOrDefault() : null;

            return new ClaimVM
            {
                Claim = new Claim
                {
                    IdAccount = idAccount ?? 0,
                    IdAccountNavigation = idAccount != null ? GetAccount(idAccount.Value) : null,
                    AmountTenancy = lastPaymentInfo?.BalanceOutputTenancy,
                    AmountPenalties = lastPaymentInfo?.BalanceOutputPenalties,
                    AmountDgi = lastPaymentInfo?.BalanceOutputDgi,
                    AmountPadun = lastPaymentInfo?.BalanceOutputPadun,
                    AmountPkk = lastPaymentInfo?.BalanceOutputPkk
                },
                RentObjects = idAccount != null ?
                    GetRentObjects(new List<int> { idAccount.Value }).SelectMany(v => v.Value).ToList() : null,
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
            var claimVM = CreateClaimEmptyViewModel(null, action);
            claimVM.Claim = claim;
            claimVM.RentObjects = GetRentObjects(new List<Claim> { claim }).SelectMany(r => r.Value).ToList();
            claimVM.LastPaymentInfo = GetLastPaymentsInfo(new List<Claim> { claim }).Select(r => r.Value).FirstOrDefault();
            return claimVM;
        }

        public Claim GetClaim(int idClaim)
        {
            var claim = registryContext.Claims.Include(r => r.IdAccountNavigation)
                .Include(r => r.IdAccountAdditionalNavigation)
                .Include(r => r.ClaimStates)
                .Include(r => r.ClaimPersons)
                .Include(r => r.ClaimFiles)
                .Include(r => r.ClaimCourtOrders)
                .FirstOrDefault(r => r.IdClaim == idClaim);
            foreach(var state in claim.ClaimStates)
            {
                state.ClaimStateFiles = registryContext.ClaimStateFiles.Where(r => r.IdState == state.IdState).ToList();
            }
            return claim;
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

        public void ClaimLogCourtOsp(int idClaim)
        {
            var logClaimStatementInSppOrigs = registryContext.LogClaimStatementInSpp.Where(l => l.IdClaim == idClaim).ToList();

            if (logClaimStatementInSppOrigs.Count()==0)
            {
                LogClaimStatementInSpp logClaimStatementInSpp = new LogClaimStatementInSpp
                {
                    CreateDate = DateTime.Now,
                    IdClaim = idClaim,
                    ExecutorLogin = securityService.User.UserName
                };
                UinForClaimStatementInSsp uinForClaim = new UinForClaimStatementInSsp
                {
                    IdClaim = idClaim,
                    Uin = GetUinForClaim(idClaim)
                };

                registryContext.LogClaimStatementInSpp.Add(logClaimStatementInSpp);
                registryContext.UinForClaimStatementInSsp.Add(uinForClaim);
            }
            else
            {
                List<DateTime> statement_crdate;
                statement_crdate = registryContext.LogClaimStatementInSpp
                                                  .Where(l => l.IdClaim == idClaim)
                                                  .GroupBy(l=>l.IdClaim)
                                                  .Select(r => new { IdClaim = r.Key, Dates = r.Select(l=>l.CreateDate) })
                                                  .ToDictionary(v => v.IdClaim, v => v.Dates.ToList())[idClaim];

                if (!statement_crdate.Contains(DateTime.Now.Date))
                {
                    LogClaimStatementInSpp logClaimStatementInSpp = new LogClaimStatementInSpp
                    {
                        CreateDate = DateTime.Now,
                        IdClaim = idClaim,
                        ExecutorLogin = securityService.User.UserName
                    };
                    registryContext.LogClaimStatementInSpp.Add(logClaimStatementInSpp);

                    var uinForClaim = registryContext.UinForClaimStatementInSsp
                                        .FirstOrDefault(c => c.IdClaim == logClaimStatementInSpp.IdClaim);

                    if (uinForClaim != null)
                    {
                        uinForClaim.Uin = GetUinForClaim(uinForClaim.IdClaim);
                        registryContext.UinForClaimStatementInSsp.Update(uinForClaim);
                    }
                    else
                    {
                        UinForClaimStatementInSsp uinForClaimNew = new UinForClaimStatementInSsp
                        {
                            IdClaim = idClaim,
                            Uin = GetUinForClaim(idClaim)
                        };
                        registryContext.UinForClaimStatementInSsp.Add(uinForClaimNew);
                    }
                }
                else
                {
                    var logClaimStatementInSppOrig = logClaimStatementInSppOrigs.FirstOrDefault(l=>l.CreateDate==DateTime.Now.Date);

                    logClaimStatementInSppOrig.ExecutorLogin = logClaimStatementInSppOrig.ExecutorLogin == securityService.User.UserName
                                                                ? logClaimStatementInSppOrig.ExecutorLogin : securityService.User.UserName;
                    registryContext.LogClaimStatementInSpp.Update(logClaimStatementInSppOrig);
                }
            }
            registryContext.SaveChanges();
        }

        public string GetUinForClaim(int idClaim)
        {
            // +1-8 байт - urn в десятичной системе (дополнить нулями слева до 8) равен "00009703"
            // +9-18 байт - ЛС (account) с дополненными впереди нулями 
            // +19-24 байт - инкремент от 000000 до 999999  / время UTC в секундах с 1 января 1970 года без первого знака
            // 25 байт - контрольная сумма
            
            //var uinIncrement = 0;

            var account = (from cl in registryContext.Claims
                            join pa in registryContext.PaymentAccounts
                            on cl.IdAccount equals pa.IdAccount
                            where cl.IdClaim==idClaim
                            select pa.Account).FirstOrDefault();

            var curSec = DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString("F0"); // время UTC в секундах с 1 января 1970 года (не без первого знака, т.е. 10 символов)

            while (account.Count() != 10)
                account = account.Insert(0, "0");

            var promUin = "00009703"+ account + curSec.Substring(4); /*+ String.Format("{0:D6}", uinIncrement)*/

            int j = 1, summ = 0, checkBit = 0;

            for (var i = 0; i < 24; i++)
            {
                summ = summ + Convert.ToInt16(promUin.Substring(i, 1)) * j;

                if (j == 10)
                    j = 1;
                else
                    j++;
            }

            if (summ % 11 < 10)
                checkBit = summ % 11;
            else
            {
                j = 3;
                summ = 0;

                for (var i = 0; i < 24; i++)
                {
                    summ = summ + Convert.ToInt16(promUin.Substring(i, 1)) * j;

                    if (j == 10)
                        j = 1;
                    else
                        j++;
                }

                if (summ % 11 < 10)
                    checkBit = summ % 11;
                else
                    checkBit = 0;
            }

            promUin += checkBit.ToString();

            //uinIncrement++;

            return promUin;
        }
    }
}