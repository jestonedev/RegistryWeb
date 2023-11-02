using RegistryDb.Models;
using RegistryDb.Models.Entities.Claims;
using RegistryDb.Models.Entities.Common;
using RegistryDb.Models.Entities.Payments;
using RegistryServices.Enums;
using RegistryWeb.DataServices.Claims;
using RegistryWeb.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryServices.DataServices.BksAccounts
{
    public class PaymentAccountsClaimsService
    {
        private readonly RegistryContext registryContext;
        private readonly PaymentAccountsCommonService commonService;
        private readonly ClaimsDataService claimsDataService;
        private readonly RegistryWeb.SecurityServices.SecurityService securityService;

        public PaymentAccountsClaimsService(RegistryContext registryContext, PaymentAccountsCommonService commonService,
            ClaimsDataService claimsDataService, RegistryWeb.SecurityServices.SecurityService securityService) {
            this.registryContext = registryContext;
            this.commonService = commonService;
            this.claimsDataService = claimsDataService;
            this.securityService = securityService;
        }

        public void CreateClaimMass(List<int> accountIds, DateTime atDate)
        {
            var payments = commonService.GetPaymentsForMassReports(accountIds).ToList();
            foreach (var payment in payments)
            {
                var claim = new Claim
                {
                    AtDate = atDate,
                    IdAccount = payment.IdAccount,
                    AmountTenancy = payment.BalanceOutputTenancy,
                    AmountPenalties = payment.BalanceOutputPenalties,
                    AmountDgi = payment.BalanceOutputDgi,
                    AmountPadun = payment.BalanceOutputPadun,
                    AmountPkk = payment.BalanceOutputPkk,
                    ClaimStates = new List<ClaimState> {
                        new ClaimState {
                            IdStateType = registryContext.ClaimStateTypes.Where(r => r.IsStartStateType).First().IdStateType,
                            BksRequester = securityService.CurrentExecutor?.ExecutorName,
                            DateStartState = DateTime.Now.Date,
                            Executor = securityService.CurrentExecutor?.ExecutorName
                        }
                    },
                    ClaimPersons = new List<ClaimPerson>()
                };
                claim.ClaimPersons = claimsDataService.GetClaimPersonsFromTenancy(claim.IdAccount, null);
                if (claim.ClaimPersons.Count == 0)
                {
                    claim.ClaimPersons = claimsDataService.GetClaimPersonsFromPrevClaim(claim.IdAccount, null);
                }

                claimsDataService.Create(claim, new List<Microsoft.AspNetCore.Http.IFormFile>(), LoadPersonsSourceEnum.None);
            }
        }

        public Dictionary<int, List<ClaimInfo>> GetClaimsByAddresses(IEnumerable<Payment> payments)
        {
            var accountsAssoc = commonService.GetAccountIdsAssocs(payments);
            var accountsIds = accountsAssoc.Select(r => r.IdAccountActual);
            var claims = registryContext.Claims.Where(c => c.IdAccount != null && accountsIds.Contains(c.IdAccount.Value));
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
                                 ClaimCurrentState = claimStateTypeRow.StateType,
                                 ClaimCurrentStateDate = claimStateRow.DateStartState
                             };

            claimsInfo = from claimRow in claims
                         join accountsAssocRow in accountsAssoc
                         on claimRow.IdAccount equals accountsAssocRow.IdAccountActual
                         join claimsInfoRow in claimsInfo
                         on claimRow.IdClaim equals claimsInfoRow.IdClaim into c
                         from cRow in c.DefaultIfEmpty()
                         select new ClaimInfo
                         {
                             IdClaim = claimRow.IdClaim,
                             StartDeptPeriod = claimRow.StartDeptPeriod,
                             EndDeptPeriod = claimRow.EndDeptPeriod,
                             IdAccount = accountsAssocRow.IdAccountFiltered,
                             IdClaimCurrentState = cRow.IdClaimCurrentState,
                             ClaimCurrentState = cRow.ClaimCurrentState,
                             EndedForFilter = claimRow.EndedForFilter,
                             ClaimDescription = claimRow.Description,
                             ClaimCurrentStateDate = cRow.ClaimCurrentStateDate
                         };


            var result =
                    claimsInfo
                    .Select(c => new ClaimInfo
                    {
                        ClaimCurrentState = c.ClaimCurrentState,
                        ClaimCurrentStateDate = c.ClaimCurrentStateDate,
                        IdClaimCurrentState = c.IdClaimCurrentState,
                        IdClaim = c.IdClaim,
                        StartDeptPeriod = c.StartDeptPeriod,
                        EndDeptPeriod = c.EndDeptPeriod,
                        ClaimDescription = c.ClaimDescription,
                        EndedForFilter = c.EndedForFilter,
                        IdAccount = c.IdAccount
                    })
                    .GroupBy(r => r.IdAccount)
                    .Select(r => new { IdAccount = r.Key, Claims = r.OrderByDescending(v => v.IdClaim).Select(v => v) })
                    .ToDictionary(v => v.IdAccount, v => v.Claims.ToList());
            return result;
        }
    }
}
