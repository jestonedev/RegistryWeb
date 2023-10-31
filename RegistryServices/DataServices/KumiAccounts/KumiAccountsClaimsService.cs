using RegistryDb.Models;
using RegistryDb.Models.Entities.Claims;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryDb.Models.Entities.Tenancies;
using RegistryDb.Models.Entities.Common;
using RegistryWeb.SecurityServices;

namespace RegistryServices.DataServices.KumiAccounts
{
    public class KumiAccountsClaimsService
    {
        private readonly RegistryContext registryContext;
        private readonly SecurityService securityService;

        public KumiAccountsClaimsService(RegistryContext registryContext,
            SecurityService securityService) {
            this.registryContext = registryContext;
            this.securityService = securityService;
        }

        internal Dictionary<int, List<ClaimInfo>> GetClaimsInfo(IEnumerable<KumiAccount> accounts)
        {
            var accountsIds = accounts.Select(r => r.IdAccount).ToList();
            var addressInfixes = registryContext.GetAddressByAccountIds(accountsIds);
            var actualAccountsIds = registryContext.GetKumiAccountIdsByAddressInfixes(addressInfixes.Select(r => r.Infix).ToList());

            var claims = registryContext.Claims.Where(c => c.IdAccountKumi != null && actualAccountsIds.Contains(c.IdAccountKumi.Value));
            var claimIds = claims.Select(r => r.IdClaim);

            var claimLastStatesIds = from row in registryContext.ClaimStates
                                     where claimIds.Contains(row.IdClaim)
                                     group row.IdState by row.IdClaim into gs
                                     select new
                                     {
                                         IdClaim = gs.Key,
                                         IdState = gs.Max()
                                     };

            var claimsInfo = from claimLastStateRow in claimLastStatesIds
                             join claimStateRow in registryContext.ClaimStates.Where(cs => claimIds.Contains(cs.IdClaim))
                             on claimLastStateRow.IdState equals claimStateRow.IdState
                             join claimStateTypeRow in registryContext.ClaimStateTypes
                             on claimStateRow.IdStateType equals claimStateTypeRow.IdStateType
                             select new ClaimInfo
                             {
                                 IdClaim = claimStateRow.IdClaim,
                                 IdClaimCurrentState = claimStateTypeRow.IdStateType,
                                 ClaimCurrentState = claimStateTypeRow.StateType
                             };

            claimsInfo = from claimRow in claims
                         join accountId in actualAccountsIds
                         on claimRow.IdAccountKumi equals accountId
                         join claimsInfoRow in claimsInfo
                         on claimRow.IdClaim equals claimsInfoRow.IdClaim into c
                         from cRow in c.DefaultIfEmpty()
                         select new ClaimInfo
                         {
                             IdClaim = claimRow.IdClaim,
                             StartDeptPeriod = claimRow.StartDeptPeriod,
                             EndDeptPeriod = claimRow.EndDeptPeriod,
                             IdAccount = accountId,
                             IdClaimCurrentState = cRow.IdClaimCurrentState,
                             ClaimCurrentState = cRow.ClaimCurrentState,
                             EndedForFilter = claimRow.EndedForFilter,
                             AmountTenancy = claimRow.AmountTenancy,
                             AmountPenalty = claimRow.AmountPenalties
                         };
            var result = new Dictionary<int, List<ClaimInfo>>();
            foreach (var idAccount in accountsIds)
            {
                var resultItems = new List<ClaimInfo>();
                var addressInfix = addressInfixes.FirstOrDefault(r => r.IdAccount == idAccount)?.Infix;
                if (addressInfix == null)
                {
                    result.Add(idAccount, resultItems);
                    continue;
                }
                var pairedAccountsIds = registryContext.GetKumiAccountIdsByAddressInfixes(new List<string> { addressInfix });
                resultItems = claimsInfo.Where(r => pairedAccountsIds.Contains(r.IdAccount)).ToList();
                result.Add(idAccount, resultItems);
            }

            return result;
        }



        public void CreateClaimMass(List<KumiAccount> accounts, DateTime atDate)
        {
            foreach (var account in accounts)
            {
                var claim = new Claim
                {
                    AtDate = atDate,
                    IdAccountKumi = account.IdAccount,
                    AmountTenancy = account.CurrentBalanceTenancy,
                    AmountPenalties = account.CurrentBalancePenalty,
                    AmountDgi = account.CurrentBalanceDgi,
                    AmountPadun = account.CurrentBalancePadun,
                    AmountPkk = account.CurrentBalancePkk,
                    ClaimStates = new List<ClaimState> {
                        new ClaimState {
                            IdStateType = registryContext.ClaimStateTypes.Where(r => r.IsStartStateType).First().IdStateType,
                            BksRequester = CurrentExecutor?.ExecutorName,
                            DateStartState = DateTime.Now.Date,
                            Executor = CurrentExecutor?.ExecutorName
                        }
                    },
                    ClaimPersons = new List<ClaimPerson>()
                };
                claim.ClaimPersons = GetClaimPersonsFromTenancy(claim.IdAccountKumi);
                if (claim.ClaimPersons.Count == 0)
                {
                    claim.ClaimPersons = GetClaimPersonsFromPrevClaim(claim.IdAccountKumi);
                }

                registryContext.Claims.Add(claim);
                registryContext.SaveChanges();
            }
        }

        private List<ClaimPerson> GetClaimPersonsFromPrevClaim(int? idAccount)
        {
            var prevClaim = registryContext.Claims.Include(r => r.ClaimPersons)
                .Where(r => idAccount != null ? r.IdAccountKumi == idAccount : false)
                .OrderByDescending(r => r.IdClaim).FirstOrDefault();
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

        private List<ClaimPerson> GetClaimPersonsFromTenancy(int? idAccount)
        {
            var tenancyPersons = new List<TenancyPerson>();
            if (idAccount != null)
            {
                var tenancyProcesses = (from tpRow in registryContext.TenancyProcesses.Include(tp => tp.AccountsTenancyProcessesAssoc)
                                    .Where(tp => (tp.RegistrationNum == null || !tp.RegistrationNum.Contains("н")) &&
                                         tp.AccountsTenancyProcessesAssoc.Count(atpa => atpa.IdAccount == idAccount) > 0)
                                        select tpRow).ToList();
                if (tenancyProcesses.Any())
                {
                    var idProcess = tenancyProcesses.OrderByDescending(tp => new { tp.RegistrationDate, tp.IdProcess }).First().IdProcess;
                    tenancyPersons = registryContext.TenancyPersons.Where(tp => tp.IdProcess == idProcess && tp.ExcludeDate == null).ToList();
                }
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

        private Executor CurrentExecutor
        {
            get
            {
                var userName = securityService.User?.UserName?.ToLowerInvariant();
                return registryContext.Executors.FirstOrDefault(e => e.ExecutorLogin != null &&
                                e.ExecutorLogin.ToLowerInvariant() == userName);
            }
        }

    }
}
