using RegistryDb.Models;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.KumiAccounts;
using RegistryServices.ViewModel.Claims;
using RegistryDb.Models.Entities.Claims;
using RegistryDb.Models.Entities.Common;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.Entities.Payments;
using RegistryDb.Models.Entities.Tenancies;
using RegistryServices.Enums;
using RegistryServices.DataServices.KumiAccounts;
using RegistryServices.DataFilterServices;
using RegistryServices.DataServices.Claims;

namespace RegistryWeb.DataServices.Claims
{
    public class ClaimsDataService : ListDataService<ClaimsVM, ClaimsFilter>
    {
        private readonly SecurityServices.SecurityService securityService;
        private readonly ClaimsAssignedAccountsDataService assignedAccountsService;
        private readonly IConfiguration config;
        private readonly IFilterService<Claim, ClaimsFilter> filterService;

        public ClaimsDataService(RegistryContext registryContext, SecurityServices.SecurityService securityService,
            AddressesDataService addressesDataService, 
            ClaimsAssignedAccountsDataService assignedAccountsService,
            IConfiguration config,
            FilterServiceFactory<IFilterService<Claim, ClaimsFilter>> filterServiceFactory) : base(registryContext, addressesDataService)
        {
            this.securityService = securityService;
            this.assignedAccountsService = assignedAccountsService;
            this.config = config;
            filterService = filterServiceFactory.CreateInstance();
        }

        public override ClaimsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, ClaimsFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.Streets = addressesDataService.KladrStreets;
            viewModel.StateTypes = registryContext.ClaimStateTypes;
            viewModel.KladrRegionsList = new SelectList(addressesDataService.KladrRegions, "IdRegion", "Region");
            viewModel.KladrStreetsList = new SelectList(addressesDataService.GetKladrStreets(filterOptions?.IdRegion), "IdStreet", "StreetName");
            return viewModel;
        }

        public ClaimsVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            ClaimsFilter filterOptions, out List<int> filteredIds)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var payments = GetQuery();
            viewModel.PageOptions.TotalRows = payments.Count();
            var query = filterService.GetQueryFilter(payments, viewModel.FilterOptions);
            query = filterService.GetQueryOrder(query, viewModel.OrderOptions);
            query = filterService.GetQueryIncludes(query);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            filteredIds = query.Select(c => c.IdClaim).ToList();

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            query = filterService.GetQueryPage(query, viewModel.PageOptions);
            viewModel.Claims = query.ToList();
            viewModel.RentObjectsBks = assignedAccountsService.GetRentObjectsBks(viewModel.Claims);
            viewModel.TenancyInfoKumi = assignedAccountsService.GetTenancyInfoKumi(viewModel.Claims);
            viewModel.LastPaymentInfo = assignedAccountsService.GetLastPaymentsInfo(viewModel.Claims);
            return viewModel;
        }

        public IQueryable<Claim> GetClaimsForPaymentDistribute(ClaimsFilter filterOptions)
        {
            var claims = GetQuery();
            var query = filterService.GetQueryFilter(claims, filterOptions).Where(r => r.IdAccountKumi != null);
            return query;
        }

        public IQueryable<Claim> GetClaimsByAccountIdsForPaymentDistribute(List<int> accountIds)
        {
            var claims = GetQuery().Include(r => r.IdAccountKumiNavigation).ThenInclude(r => r.Charges).Include(r => r.ClaimPersons);
            return claims.Where(r => r.IdAccountKumi != null && accountIds.Contains(r.IdAccountKumi.Value));
        }

        private IQueryable<Claim> GetQuery()
        {
            return registryContext.Claims
                .Include(c => c.ClaimStates)
                .Include(c => c.IdAccountNavigation)
                .Include(c => c.IdAccountAdditionalNavigation)
                .Include(c => c.IdAccountKumiNavigation).ThenInclude(r => r.State);
        }

        private Claim FillClaimAmount(Claim claim)
        {
            claim.AmountTenancy = claim.AmountTenancy ?? 0;
            claim.AmountPenalties = claim.AmountPenalties ?? 0;
            claim.AmountDgi = claim.AmountDgi ?? 0;
            claim.AmountPadun = claim.AmountPadun ?? 0;
            claim.AmountPkk = claim.AmountPkk ?? 0;
            if (claim.IdClaim != 0)
            {
                var claimDb = registryContext.Claims.AsNoTracking().FirstOrDefault(r => r.IdClaim == claim.IdClaim);
                if (claimDb != null)
                {
                    claim.AmountTenancyRecovered = claimDb.AmountTenancyRecovered;
                    claim.AmountPenaltiesRecovered = claimDb.AmountPenaltiesRecovered;
                    claim.AmountDgiRecovered = claimDb.AmountDgiRecovered;
                    claim.AmountPadun = claimDb.AmountPadun;
                    claim.AmountPkk = claimDb.AmountPkk;
                }
            }
            return claim;
        }

        public void Create(Claim claim, List<Microsoft.AspNetCore.Http.IFormFile> files, LoadPersonsSourceEnum loadPersonsSource)
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
            if (claim.ClaimPersons == null)
                claim.ClaimPersons = new List<ClaimPerson>();
            switch (loadPersonsSource)
            {
                case LoadPersonsSourceEnum.None:
                    break;
                case LoadPersonsSourceEnum.Tenancy:
                    claim.ClaimPersons = claim.ClaimPersons.Union(GetClaimPersonsFromTenancy(claim.IdAccount, claim.IdAccountKumi)).ToList();
                    break;
                case LoadPersonsSourceEnum.PrevClaim:
                    claim.ClaimPersons = claim.ClaimPersons.Union(GetClaimPersonsFromPrevClaim(claim.IdAccount, claim.IdAccountKumi)).ToList();
                    break;
            }

            claim.IdAccountNavigation = null;
            claim.IdAccountAdditionalNavigation = null;
            claim.IdAccountKumiNavigation = null;
            claim = FillClaimAmount(claim);
            registryContext.Claims.Add(claim);
            registryContext.SaveChanges();
        }

        public List<ClaimPerson> GetClaimPersonsFromPrevClaim(int? idAccountBks, int? idAccountKumi)
        {
            var prevClaim = registryContext.Claims.Include(r => r.ClaimPersons)
                .Where(r => idAccountKumi != null ? r.IdAccountKumi == idAccountKumi : idAccountBks != null ? r.IdAccount == idAccountBks : false)
                .OrderByDescending(r => r.IdClaim).FirstOrDefault();
            if (prevClaim != null)
            {
                return prevClaim.ClaimPersons.Select(r => new ClaimPerson {
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

        public List<ClaimPerson> GetClaimPersonsFromTenancy(int? idAccountBks, int? idAccountKumi)
        {
            var tenancyPersons = new List<TenancyPerson>();
            if (idAccountKumi != null)
            {
                var tenancyProcesses = (from tpRow in registryContext.TenancyProcesses.Include(tp => tp.AccountsTenancyProcessesAssoc)
                                    .Where(tp => (tp.RegistrationNum == null || !tp.RegistrationNum.Contains("н")) &&
                                        tp.AccountsTenancyProcessesAssoc.Count(atpa => atpa.IdAccount == idAccountKumi) > 0)
                                     select tpRow).ToList();
                if (tenancyProcesses.Any())
                {
                    var idProcess = tenancyProcesses.OrderByDescending(tp => tp.RegistrationDate).ThenByDescending(tp => tp.IdProcess) .First().IdProcess;
                    tenancyPersons = registryContext.TenancyPersons.Where(tp => tp.IdProcess == idProcess && tp.ExcludeDate == null).ToList();
                }
            } else
            if (idAccountBks != null)
            {
                var addressInfix = assignedAccountsService.GetPaymentAccountAddressInfix(idAccountBks.Value);
                tenancyPersons = GetTenancyPersonForAddressInfix(addressInfix.Infix);
            }
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

        public int GetIdJudge(int idAccount)
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

        public void Edit(Claim claim)
        {
            claim.IdAccountNavigation = null;
            claim.IdAccountAdditionalNavigation = null;
            claim.IdAccountKumiNavigation = null;
            claim = FillClaimAmount(claim);
            registryContext.Claims.Update(claim);
            registryContext.SaveChanges();
        }

        public void Delete(int idClaim)
        {
            var claim = registryContext.Claims
                    .Include(r => r.PaymentClaims)
                    .FirstOrDefault(op => op.IdClaim == idClaim);
            if (claim != null)
            {
                if (claim.PaymentClaims.Any()) throw new Exception("Нельзя удалить претензионно-исковую работу, на которую распределены платежи");
                claim.Deleted = 1;
                registryContext.SaveChanges();
            }
        }

        public ClaimsVM GetClaimsViewModelForMassReports(List<int> ids, PageOptions pageOptions)
        {
            var viewModel = InitializeViewModel(null, pageOptions, null);
            var claims = GetClaimsForMassReports(ids);
            viewModel.PageOptions.TotalRows = claims.Count();
            var count = claims.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.Claims = filterService.GetQueryPage(claims, viewModel.PageOptions).ToList();
            viewModel.RentObjectsBks = assignedAccountsService.GetRentObjectsBks(viewModel.Claims);
            viewModel.TenancyInfoKumi = assignedAccountsService.GetTenancyInfoKumi(viewModel.Claims);
            viewModel.LastPaymentInfo = assignedAccountsService.GetLastPaymentsInfo(viewModel.Claims);
            return viewModel;
        }

        public string GetClaimerByIdClaim(int idClaim)
        {
            var tenant = registryContext.ClaimPersons.Where(cp => cp.IsClaimer && cp.IdClaim == idClaim)
                           .Select(cp => string.Concat(cp.Surname, ' ', cp.Name, ' ', cp.Patronymic)).FirstOrDefault();
            if (!string.IsNullOrEmpty(tenant)) return tenant;
            var idAccount = registryContext.Claims.Where(r => r.IdClaim == idClaim).Select(r => r.IdAccountKumi).FirstOrDefault();
            if (idAccount == null) return null;
            var tenants = registryContext.GetTenantsByAccountIds(new List<int> { idAccount.Value });
            return tenants.FirstOrDefault()?.Tenant;
        }

        public IQueryable<Claim> GetClaimsForMassReports(List<int> ids)
        {
            return registryContext.Claims
                .Where(c => ids.Contains(c.IdClaim))
                .Include(c => c.IdAccountNavigation)
                .Include(c => c.IdAccountAdditionalNavigation)
                .Include(c => c.IdAccountKumiNavigation);
        }

        public void AddClaimStateMass(List<int> ids, ClaimState claimState)
        {
            foreach (var id in ids)
            {
                var claimStateCopy = JsonConvert.DeserializeObject<ClaimState>(JsonConvert.SerializeObject(claimState));
                claimStateCopy.IdClaim = id;
                registryContext.ClaimStates.Add(claimStateCopy);
            }
            registryContext.SaveChanges();
        }

        public void UpdateDeptPeriodInClaims(List<int> claimIds, DateTime? startDeptPeriod, DateTime? endDeptPeriod)
        {
            var claims = registryContext.Claims.Where(r => claimIds.Contains(r.IdClaim));
            foreach (var claim in claims)
            {
                claim.StartDeptPeriod = startDeptPeriod;
                claim.EndDeptPeriod = endDeptPeriod;
            }
            registryContext.SaveChanges();
        }

        public ClaimVM CreateClaimEmptyViewModel(int? idAccountBks = null, int? idAccountKumi = null, [CallerMemberName]string action = "")
        {
            var lastPaymentInfo = idAccountBks != null ?
                    assignedAccountsService.GetLastPaymentsInfo(new List<int> { idAccountBks.Value }).Select(v => v.Value).FirstOrDefault() : null;
            KumiAccount kumiAccount = null;
            if (idAccountKumi != null)
                kumiAccount = assignedAccountsService.GetAccountKumi(idAccountKumi.Value);

            return new ClaimVM
            {
                Claim = new Claim
                {
                    IdAccount = idAccountBks ?? null,
                    IdAccountNavigation = idAccountBks != null ? assignedAccountsService.GetAccountBks(idAccountBks.Value) : null,
                    IdAccountKumi = idAccountKumi ?? (idAccountBks == null ? 0 : (int?)null),
                    IdAccountKumiNavigation = kumiAccount,
                    AmountTenancy = kumiAccount != null ? kumiAccount.CurrentBalanceTenancy : lastPaymentInfo?.BalanceOutputTenancy,
                    AmountPenalties = kumiAccount != null ? kumiAccount.CurrentBalancePenalty : lastPaymentInfo?.BalanceOutputPenalties,
                    AmountDgi = kumiAccount != null ? null : lastPaymentInfo?.BalanceOutputDgi,
                    AmountPadun = kumiAccount != null ? null : lastPaymentInfo?.BalanceOutputPadun,
                    AmountPkk = kumiAccount != null ? null : lastPaymentInfo?.BalanceOutputPkk
                },
                RentObjectsBks = idAccountBks != null ?
                    assignedAccountsService.GetRentObjectsBks(new List<int> { idAccountBks.Value }).SelectMany(v => v.Value).ToList() : null,
                TenancyInfoKumi = idAccountKumi != null ? assignedAccountsService.GetTenancyInfoKumi(new List<int> { idAccountKumi.Value }).SelectMany(r => r.Value).ToList() : null,
                CurrentExecutor = CurrentExecutor,
                StateTypes = registryContext.ClaimStateTypes.ToList(),
                Executors = registryContext.Executors.ToList(),
                Judges = registryContext.Judges.Where(r => action == "Create" || !r.IsInactive).ToList(),
                Signers = registryContext.SelectableSigners.Where(r => r.IdSignerGroup == 3).ToList(),
                StateTypeRelations = registryContext.ClaimStateTypeRelations.ToList()
            };
        }

        public ClaimVM GetClaimViewModel(Claim claim, [CallerMemberName]string action = "")
        {
            var claimVM = CreateClaimEmptyViewModel(null, null, action);
            claimVM.Claim = claim;
            claimVM.RentObjectsBks = assignedAccountsService.GetRentObjectsBks(new List<Claim> { claim }).SelectMany(r => r.Value).ToList();
            claimVM.LastPaymentInfo = assignedAccountsService.GetLastPaymentsInfo(new List<Claim> { claim }).Select(r => r.Value).FirstOrDefault();
            claimVM.TenancyInfoKumi = assignedAccountsService.GetTenancyInfoKumi(new List<Claim> { claim }).SelectMany(r => r.Value).ToList();
            return claimVM;
        }

        public Claim GetClaim(int idClaim)
        {
            var claim = registryContext.Claims
                .Include(r => r.IdAccountNavigation)
                .Include(r => r.IdAccountAdditionalNavigation)
                .Include(r => r.IdAccountKumiNavigation)
                .Include(r => r.ClaimStates)
                .Include(r => r.ClaimPersons)
                .Include(r => r.ClaimFiles)
                .Include(r => r.ClaimCourtOrders)
                .Include(r => r.PaymentClaims)
                .FirstOrDefault(r => r.IdClaim == idClaim);
            if (claim == null) return claim;
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

                List<UinForClaimStatementInSsp> uinForClaims = GetUinPersonsForClaim(idClaim);
                 
                registryContext.LogClaimStatementInSpp.Add(logClaimStatementInSpp);
                registryContext.UinForClaimStatementInSsp.AddRange(uinForClaims);
            }
            else
            {
                List<DateTime> statementCrDate;
                statementCrDate = registryContext.LogClaimStatementInSpp
                                                  .Where(l => l.IdClaim == idClaim)
                                                  .GroupBy(l=>l.IdClaim)
                                                  .Select(r => new { IdClaim = r.Key, Dates = r.Select(l=>l.CreateDate) })
                                                  .ToDictionary(v => v.IdClaim, v => v.Dates.ToList())[idClaim];

                if (!statementCrDate.Contains(DateTime.Now.Date))
                {
                    LogClaimStatementInSpp logClaimStatementInSpp = new LogClaimStatementInSpp
                    {
                        CreateDate = DateTime.Now,
                        IdClaim = idClaim,
                        ExecutorLogin = securityService.User.UserName
                    };
                    registryContext.LogClaimStatementInSpp.Add(logClaimStatementInSpp);
                }
                else
                {
                    var logClaimStatementInSppOrig = logClaimStatementInSppOrigs.FirstOrDefault(l=>l.CreateDate==DateTime.Now.Date);

                    logClaimStatementInSppOrig.ExecutorLogin = logClaimStatementInSppOrig.ExecutorLogin == securityService.User.UserName
                                                                ? logClaimStatementInSppOrig.ExecutorLogin : securityService.User.UserName;
                    registryContext.LogClaimStatementInSpp.Update(logClaimStatementInSppOrig);
                }

                var oldUinForClaims = registryContext.UinForClaimStatementInSsp
                    .Where(c => c.IdClaim == idClaim).ToList(); //список существующих уин

                List<UinForClaimStatementInSsp> newUinForClaims = GetUinPersonsForClaim(idClaim);// количество совпадает со списком членов семьи

                foreach(var oldUinForClaim in oldUinForClaims)
                {
                    if (oldUinForClaim.IdPerson == null)
                    {
                        oldUinForClaim.IdPerson = registryContext.ClaimPersons
                                               .Where(c => c.IdClaim == idClaim)
                                               .FirstOrDefault(c => c.IsClaimer == true)?.IdPerson;
                        registryContext.UinForClaimStatementInSsp.Update(oldUinForClaim);
                        continue;
                    }
                    if (newUinForClaims.Any(r => r.IdPerson == oldUinForClaim.IdPerson)) continue;
                    registryContext.UinForClaimStatementInSsp.Remove(oldUinForClaim);

                }

                foreach(var newUinForClaim in newUinForClaims)
                {
                    if (oldUinForClaims.Any(r => r.IdPerson == newUinForClaim.IdPerson)) continue;
                    registryContext.UinForClaimStatementInSsp.Add(newUinForClaim);
                }
            }
            registryContext.SaveChanges();
        }

        public List<UinForClaimStatementInSsp> GetUinPersonsForClaim(int idClaim)
        {
            List<UinForClaimStatementInSsp> uinForClaims = new List<UinForClaimStatementInSsp>();
            
            var idPersons = registryContext.ClaimPersons
                                           .Where(c => c.IdClaim == idClaim && c.Deleted == 0).Select(c => c.IdPerson).ToList();

            if (idPersons.Count() != 0)
            {
                foreach (var idPerson in idPersons)
                {
                    var uinForClaim = new UinForClaimStatementInSsp
                    {
                        IdClaim = idClaim,
                        IdPerson = idPerson,
                        Uin = GetUinForClaim(assignedAccountsService.GetAccountForCLaim(idClaim), idPerson),
                    };
                    uinForClaims.Add(uinForClaim);
                }
            }
            else
            {
                var uinForClaim = new UinForClaimStatementInSsp
                {
                    IdClaim = idClaim,
                    IdPerson = null,
                    Uin = GetUinForClaim(assignedAccountsService.GetAccountForCLaim(idClaim)),
                };
                uinForClaims.Add(uinForClaim);
            }
            return uinForClaims;
        }

        public string GetUinForClaim(string account, int? idPerson = null)
        {
            // +1-8 байт - urn в десятичной системе (дополнить нулями слева до 8) равен "00009703"
            // +9-18 байт - ЛС (account) с дополненными впереди нулями 
            // +19-24 байт - инкремент от 000000 до 999999  / время UTC в секундах с 1 января 1970 года без первого знака
            // 25 байт - контрольная сумма

            //var uinIncrement = 0;

            while (account.Count() != 10)
                account = account.Insert(0, "0");

            var promUin = "00009703" + account;
            if (idPerson != null) // если члены семьи заполнены
            {
                var uniqVal = idPerson.ToString().PadLeft(6, '0');
                promUin += uniqVal;
            }
            else
            {
                promUin += DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds.ToString("F0").Substring(4);/* 
                                                    // время UTC в секундах с 1 января 1970 года (не без первого знака, т.е. 10 символов)
                                                         String.Format("{0:D6}", uinIncrement)*/
            }

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

        public int ReceivePersonCount(int idClaim)
        {
            return registryContext.ClaimPersons
                                           .Where(c => c.IdClaim == idClaim && c.Deleted == 0).Count();
        }
    }
}