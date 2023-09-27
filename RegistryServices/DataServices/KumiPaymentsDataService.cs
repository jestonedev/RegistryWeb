using RegistryDb.Models;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.ViewModel;
using RegistryServices.ViewModel.KumiAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryPaymentsLoader.TffStrings;
using RegistryServices.Models;
using RegistryPaymentsLoader.Models;
using RegistryServices;
using RegistryWeb.SecurityServices;
using RegistryWeb.ViewOptions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Text;
using RegistryPaymentsLoader.TffFileLoaders;
using RegistryDb.Models.Entities.Common;
using RegistryDb.Models.SqlViews;
using RegistryDb.Models.Entities.RegistryObjects.Kladr;
using RegistryDb.Models.Entities.Claims;
using RegistryServices.Enums;
using RegistryServices.Models.KumiPayments;
using System.Text.Json;
using System.Text.Json.Serialization;
using RegistryServices.Classes;
using System.Text.RegularExpressions;

namespace RegistryWeb.DataServices
{

    public class KumiPaymentsDataService : ListDataService<KumiPaymentsVM, KumiPaymentsFilter>
    {
        private readonly KumiAccountsDataService kumiAccountsDataService;
        private SecurityService securityService;

        public KumiPaymentsDataService(RegistryContext registryContext, KumiAccountsDataService kumiAccountsDataService,
            AddressesDataService addressesDataService, SecurityService securityService) : base(registryContext, addressesDataService)
        {
            this.kumiAccountsDataService = kumiAccountsDataService;
            this.securityService = securityService;
        }

        public KumiPaymentsUploadStateModel UploadInfoFromTff(List<TffString> tffStrings, List<KumiPaymentGroupFile> kumiPaymentGroupFiles,
            int? idParentPayment,
            out int idGroup)
        {
            var loadState = new KumiPaymentsUploadStateModel();

            var extracts = tffStrings.Where(r => r is TffStringVT)
                .Select(r => ((TffStringVT)r).ToExtract())
                .Where(r => !r.IsMemorialOrder()).ToList();

            var memorialOrders = tffStrings.Where(r => r is TffStringVT)
                .Select(r => ((TffStringVT)r).ToExtract())
                .Where(r => r.IsMemorialOrder())
                .Select(r => r.ToMemorialOrder()).ToList();

            var knownPayments = 
                tffStrings.Where(r => r is TffStringBD).Select(r => ((TffStringBD)r).ToPayment()).ToList();

            var unknownPayments = tffStrings.Where(r => r is TffStringZF).Select(r => ((TffStringZF)r).ToPayment()).ToList();

            var payments = knownPayments.Union(unknownPayments).Distinct();

            var group = new KumiPaymentGroup {
                Date = DateTime.Now,
                PaymentGroupFiles = kumiPaymentGroupFiles,
                User = securityService.User.UserName
            };
            registryContext.KumiPaymentGroups.Add(group);

            if (idParentPayment != null)
            {
                var parentPayment = registryContext.KumiPayments.Include(r => r.PaymentCharges)
                    .Include(r => r.PaymentClaims).FirstOrDefault(r => r.IdPayment == idParentPayment);
                if (parentPayment.PaymentCharges.Any() || parentPayment.PaymentClaims.Any())
                    throw new ApplicationException("Платеж, указанный как сводное платежное поручение частично или полностью распределен. Необходимо сначала отменить распределение выбранного сводного платежного поручения");
                var paymentsTotalSum = payments.Select(r => r.Sum).Sum();
                if (parentPayment == null || parentPayment.Sum != paymentsTotalSum)
                    throw new ApplicationException("Сумма, указанная в сводном платежном поручении не соответствует общей сумме загружаемых платежей");

                foreach(var payment in payments)
                {
                    payment.IdParentPayment = idParentPayment;
                }
                parentPayment.IsConsolidated = 1;
                registryContext.KumiPayments.Update(parentPayment);
            }

            UploadPayments(payments, group, extracts, loadState);
            UploadMemorialOrders(memorialOrders, group, loadState);

            registryContext.SaveChanges();
            registryContext.DetachAllEntities();

            loadState.AutoDistributedPayments = AutoDistributeUploadedPayments(loadState.InsertedPayments);

            var loadStateSerializeObject = JsonSerializer.Serialize(loadState, typeof(KumiPaymentsUploadStateModel), new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
            });

            var log = new KumiPaymentGroupLog()
            {
                IdGroup = group.IdGroup,
                Log = loadStateSerializeObject
            };
            registryContext.KumiPaymentGroupLog.Add(log);

            registryContext.SaveChanges();

            idGroup = log.IdGroup;
            return loadState;
        }

        public override KumiPaymentsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, KumiPaymentsFilter filterOptions)
        {
            var vm = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            vm.PaymentSourcesList = new SelectList(registryContext.KumiPaymentInfoSources.ToList(), "IdSource", "Name");
            return vm;
        }

        public KumiPaymentsVM GetViewModel(OrderOptions orderOptions, PageOptions pageOptions, KumiPaymentsFilter filterOptions, out List<int> filteredPaymentsIds)
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

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;

            filteredPaymentsIds = query.Select(r => r.IdPayment).ToList();

            query = GetQueryPage(query, viewModel.PageOptions);
            viewModel.Payments = query.ToList();
            viewModel.DistributionInfoToObjects = GetDistributionInfoToObjects(viewModel.Payments.Select(r => r.IdPayment).ToList());
            if (filterOptions?.IdCharge != null)
            {
                var charge = registryContext.KumiCharges.Where(r => r.IdCharge == filterOptions.IdCharge).FirstOrDefault();
                if (charge != null)
                {
                    viewModel.StartDate = charge.StartDate;
                    viewModel.EndDate = charge.EndDate;
                    viewModel.Account = registryContext.KumiAccounts.FirstOrDefault(r => r.IdAccount == charge.IdAccount)?.Account;
                }
            }
            if (filterOptions?.IdAccount != null)
            {
                viewModel.Account = registryContext.KumiAccounts.FirstOrDefault(r => r.IdAccount == filterOptions.IdAccount)?.Account;
            }

            return viewModel;
        }

        private List<KumiPaymentDistributionInfoToObject> GetDistributionInfoToObjects(List<int> idPayments)
        {
            var consolidatedPaymentsIds = registryContext.KumiPayments.Where(r => idPayments.Contains(r.IdPayment) && r.IsConsolidated == 1).Select(r => r.IdPayment).ToList();
            var childPayments = registryContext.KumiPayments
                .Where(r => r.IdParentPayment != null && consolidatedPaymentsIds.Contains(r.IdParentPayment.Value)).ToList();
            var consolidatedPayments = new Dictionary<int, List<KumiPayment>>();
            if (childPayments.Any())
            {
                foreach (var group in childPayments.GroupBy(r => r.IdParentPayment.Value))
                {
                    consolidatedPayments.Add(group.Key, group.Select(r => r).ToList());
                    var subChildPayments = group.ToList();
                    while (true)
                    {
                        var newParentIds = subChildPayments.Select(r => r.IdPayment);
                        subChildPayments = registryContext.KumiPayments.Where(r => r.IdParentPayment != null &&
                            newParentIds.Contains(r.IdParentPayment.Value)).ToList();
                        if (!subChildPayments.Any())
                            break;
                        consolidatedPayments[group.Key].AddRange(subChildPayments);
                    }
                }
            }

            var idPaymentsAll = idPayments.Union(consolidatedPayments.SelectMany(r => r.Value).Select(r => r.IdPayment)).ToList();

            var kpc = (from kpcRow in registryContext.KumiPaymentCharges
                      where idPaymentsAll.Contains(kpcRow.IdPayment)
                      select kpcRow).ToList();

            var cIds = kpc.Select(r => r.IdCharge);

            var c = (from cRow in registryContext.KumiCharges
                     where cIds.Contains(cRow.IdCharge)
                     select cRow).ToList();

            var aIds = c.Select(r => r.IdAccount);

            var a = (from aRow in registryContext.KumiAccounts
                     where aIds.Contains(aRow.IdAccount)
                     select aRow).ToList();

            var charges = (from kpcRow in kpc
                           join cRow in c
                           on kpcRow.IdCharge equals cRow.IdCharge
                           join aRow in a
                           on cRow.IdAccount equals aRow.IdAccount
                           select new
                           {
                               kpcRow,
                               cRow,
                               aRow
                           }).ToList();

            var tenants = (from assocRow in registryContext.KumiAccountsTenancyProcessesAssocs
                           join tRow in registryContext.TenancyProcesses
                           on assocRow.IdProcess equals tRow.IdProcess
                           join tpRow in registryContext.TenancyPersons
                           on tRow.IdProcess equals tpRow.IdProcess
                           where aIds.Contains(assocRow.IdAccount) && tpRow.IdKinship == 1 && tpRow.ExcludeDate == null
                           select new
                           {
                               assocRow.IdAccount,
                               tRow.IdProcess,
                               Tenant = string.Concat(tpRow.Surname, ' ', tpRow.Name, ' ', tpRow.Patronymic)
                           }).ToList();

            var paymentChargesInfo =charges.Select(r => new KumiPaymentDistributionInfoToAccount
                {
                    ObjectType = KumiPaymentDistributeToEnum.ToKumiAccount,
                    IdPayment = r.kpcRow.IdPayment,
                    IdAccount = r.cRow.IdAccount,
                    IdCharge = r.kpcRow.IdDisplayCharge ?? r.kpcRow.IdCharge,
                    Account = r.aRow.Account,
                    DistrubutedToPenaltySum = r.kpcRow.PenaltyValue,
                    DistrubutedToTenancySum = r.kpcRow.TenancyValue,
                    DistrubutedToDgiSum = r.kpcRow.DgiValue,
                    DistrubutedToPkkSum = r.kpcRow.PkkValue,
                    DistrubutedToPadunSum = r.kpcRow.PadunValue,
                    Sum = r.kpcRow.PenaltyValue + r.kpcRow.TenancyValue + r.kpcRow.DgiValue + r.kpcRow.PkkValue + r.kpcRow.PadunValue,
                    Tenant = tenants.Where(t => t.IdAccount == r.cRow.IdAccount).OrderByDescending(t => t.IdProcess)
                                .Select(t => t.Tenant).FirstOrDefault()
                }).ToList();

            var kpcClaims = (from kpcRow in registryContext.KumiPaymentClaims
                             where idPaymentsAll.Contains(kpcRow.IdPayment)
                             select kpcRow).ToList();

            cIds = kpcClaims.Select(r => r.IdClaim);

            var cClaims = (from cRow in registryContext.Claims
                            where cIds.Contains(cRow.IdClaim)
                            select cRow).ToList();

            aIds = cClaims.Where(r => r.IdAccountKumi != null).Select(r => (int)r.IdAccountKumi);

            var aClaims = (from aRow in registryContext.KumiAccounts
                     where aIds.Contains(aRow.IdAccount)
                     select aRow).ToList();

            var claims = (from kpcRow in kpcClaims
                           join cRow in cClaims
                           on kpcRow.IdClaim equals cRow.IdClaim
                           join aRow in aClaims
                           on cRow.IdAccountKumi equals aRow.IdAccount
                           select new
                           {
                               kpcRow,
                               cRow,
                               aRow
                           }).ToList();

            var tenantsClaims = (from assocRow in registryContext.KumiAccountsTenancyProcessesAssocs
                           join tRow in registryContext.TenancyProcesses
                           on assocRow.IdProcess equals tRow.IdProcess
                           join tpRow in registryContext.TenancyPersons
                           on tRow.IdProcess equals tpRow.IdProcess
                           where aIds.Contains(assocRow.IdAccount) && tpRow.IdKinship == 1 && tpRow.ExcludeDate == null
                           select new
                           {
                               assocRow.IdAccount,
                               tRow.IdProcess,
                               Tenant = string.Concat(tpRow.Surname, ' ', tpRow.Name, ' ', tpRow.Patronymic)
                           }).ToList();

            var paymentClaimsInfo = claims.Select(r => new KumiPaymentDistributionInfoToClaim
            {
                ObjectType = KumiPaymentDistributeToEnum.ToClaim,
                IdPayment = r.kpcRow.IdPayment,
                IdClaim = r.cRow.IdClaim,
                IdCharge = r.kpcRow.IdDisplayCharge ?? 0,
                IdAccountKumi = r.cRow.IdAccountKumi,
                Account = r.aRow.Account,
                DistrubutedToPenaltySum = r.kpcRow.PenaltyValue,
                DistrubutedToTenancySum = r.kpcRow.TenancyValue,
                DistrubutedToDgiSum = r.kpcRow.DgiValue,
                DistrubutedToPkkSum = r.kpcRow.PkkValue,
                DistrubutedToPadunSum = r.kpcRow.PadunValue,
                Sum = r.kpcRow.PenaltyValue + r.kpcRow.TenancyValue + r.kpcRow.DgiValue + r.kpcRow.PkkValue + r.kpcRow.PadunValue,
                Tenant = tenantsClaims.Where(t => t.IdAccount == r.cRow.IdAccountKumi).OrderByDescending(t => t.IdProcess)
                                .Select(t => t.Tenant).FirstOrDefault()
            }).ToList();

            foreach(var consolidatedPayment in consolidatedPayments)
            {
                var childPaymentsIds = consolidatedPayment.Value.Select(r => r.IdPayment).ToList();
                var comparedPaymentChargesInfo = paymentChargesInfo.Where(r => childPaymentsIds.Contains(r.IdPayment)).ToList();
                foreach(var paymentChargeInfo in comparedPaymentChargesInfo)
                {
                    paymentChargesInfo.Add(new KumiPaymentDistributionInfoToAccount {
                        ObjectType = paymentChargeInfo.ObjectType,
                        IdPayment = consolidatedPayment.Key,
                        IdAccount = paymentChargeInfo.IdAccount,
                        IdCharge = paymentChargeInfo.IdCharge,
                        Account = paymentChargeInfo.Account,
                        DistrubutedToPenaltySum = paymentChargeInfo.DistrubutedToPenaltySum,
                        DistrubutedToTenancySum = paymentChargeInfo.DistrubutedToTenancySum,
                        DistrubutedToDgiSum = paymentChargeInfo.DistrubutedToDgiSum,
                        DistrubutedToPkkSum = paymentChargeInfo.DistrubutedToPkkSum,
                        DistrubutedToPadunSum = paymentChargeInfo.DistrubutedToPadunSum,
                        Sum = paymentChargeInfo.Sum,
                        Tenant = paymentChargeInfo.Tenant
                    });
                }

                var comparedPaymentClaimsInfo = paymentClaimsInfo.Where(r => childPaymentsIds.Contains(r.IdPayment)).ToList();
                foreach (var paymentClaimInfo in comparedPaymentClaimsInfo)
                {
                    paymentClaimsInfo.Add(new KumiPaymentDistributionInfoToClaim
                    {
                        ObjectType = paymentClaimInfo.ObjectType,
                        IdPayment = consolidatedPayment.Key,
                        IdClaim = paymentClaimInfo.IdClaim,
                        IdCharge = paymentClaimInfo.IdCharge,
                        IdAccountKumi = paymentClaimInfo.IdAccountKumi,
                        Account = paymentClaimInfo.Account,
                        DistrubutedToPenaltySum = paymentClaimInfo.DistrubutedToPenaltySum,
                        DistrubutedToTenancySum = paymentClaimInfo.DistrubutedToTenancySum,
                        DistrubutedToDgiSum = paymentClaimInfo.DistrubutedToDgiSum,
                        DistrubutedToPkkSum = paymentClaimInfo.DistrubutedToPkkSum,
                        DistrubutedToPadunSum = paymentClaimInfo.DistrubutedToPadunSum,
                        Sum = paymentClaimInfo.Sum,
                        Tenant = paymentClaimInfo.Tenant
                    });
                }
            }

            return paymentChargesInfo.Select(r => (KumiPaymentDistributionInfoToObject)r).Union(paymentClaimsInfo).ToList();
        }

        private IQueryable<KumiPayment> GetQueryOrder(IQueryable<KumiPayment> query, OrderOptions orderOptions)
        {
            if (!string.IsNullOrEmpty(orderOptions.OrderField) && orderOptions.OrderField == "Date")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.DateDocument ?? p.DateIn ?? p.DateExecute);
                else
                    return query.OrderByDescending(p => p.DateDocument ?? p.DateIn ?? p.DateExecute);
            }
            return query;
        }

        private IQueryable<KumiPayment> GetQuery()
        {
            return registryContext.KumiPayments
                .Include(r => r.PaymentCharges)
                .Include(r => r.PaymentClaims)
                .Include(r => r.ChildPayments)
                .Include(r => r.PaymentGroup);
        }

        private IQueryable<KumiPayment> GetQueryIncludes(IQueryable<KumiPayment> query)
        {
            return query
                .Include(r => r.PaymentCharges)
                .Include(r => r.PaymentClaims)
                .Include(r => r.ChildPayments)
                .Include(r => r.PaymentGroup);
        }

        private IQueryable<KumiPayment> GetQueryFilter(IQueryable<KumiPayment> query, KumiPaymentsFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                if (filterOptions.IdParentPayment != null)
                {
                    return query.Where(r => r.IdPayment == filterOptions.IdParentPayment || r.IdParentPayment == filterOptions.IdParentPayment);
                }
                if(!string.IsNullOrWhiteSpace(filterOptions.CommonFilter))
                {
                    return CommonFilter(query, filterOptions.CommonFilter);
                }
                if (!filterOptions.IsModalEmpty())
                {
                    query = CommonPaymentFilter(query, filterOptions);
                    query = PayerFilter(query, filterOptions);
                    query = RecipientFilter(query, filterOptions);
                }
                if (!filterOptions.IsRefEmpty())
                {
                    query = RefFilter(query, filterOptions);
                }
            }
            return query;
        }

        private IQueryable<KumiPayment> RefFilter(IQueryable<KumiPayment> query, KumiPaymentsFilter filterOptions)
        {
            if (filterOptions.IdClaim != null)
            {
                var ids = registryContext.KumiPaymentClaims
                           .Where(r => r.IdClaim == filterOptions.IdClaim).Select(r => r.IdPayment).ToList();

                var untiedIds = GetUntiedPayments(null, filterOptions.IdClaim, null).Select(r => r.IdPayment);

                ids = ids.Union(untiedIds).ToList();

                query = query.Where(r => ids.Contains(r.IdPayment));
            }
            if (filterOptions.IdCharge != null)
            {
                var ids = registryContext.KumiPaymentCharges.Where(r => r.IdDisplayCharge == filterOptions.IdCharge).Select(r => r.IdPayment)
                    .Union(registryContext.KumiPaymentClaims.Where(r => r.IdDisplayCharge == filterOptions.IdCharge).Select(r => r.IdPayment)).ToList();

                var untiedIds = GetUntiedPayments(null, null, filterOptions.IdCharge).Select(r => r.IdPayment);

                ids = ids.Union(untiedIds).ToList();

                query = query.Where(r => ids.Contains(r.IdPayment));
            }
            if (filterOptions.IdAccount != null)
            {
                var idCharges = registryContext.KumiCharges.Where(r => r.IdAccount == filterOptions.IdAccount).Select(r => r.IdCharge).ToList();
                var idClaims = registryContext.Claims.Where(r => r.IdAccountKumi != null && r.IdAccountKumi == filterOptions.IdAccount).Select(r => r.IdClaim).ToList();

                var ids = registryContext.KumiPaymentCharges.Where(r => idCharges.Contains(r.IdCharge)).Select(r => r.IdPayment)
                    .Union(
                        registryContext.KumiPaymentClaims.Where(r => idClaims.Contains(r.IdClaim))
                        .Select(r => r.IdPayment)
                    ).Distinct().ToList();

                var untiedIds = GetUntiedPayments(filterOptions.IdAccount, null, null).Select(r => r.IdPayment);

                ids = ids.Union(untiedIds).ToList();

                query = query.Where(r => ids.Contains(r.IdPayment));
            }
            return query;
        }

        public IList<KumiPaymentUntied> GetUntiedPayments(int? idAccount, int? idClaim, int? idCharge)
        {
            if (idCharge != null)
            {
                return registryContext.KumiPaymentsUntied
                    .Where(r => r.IdCharge == idCharge).ToList();
            }
            if (idClaim != null)
            {
                return registryContext.KumiPaymentsUntied
                    .Where(r => r.IdClaim != null && r.IdClaim == idClaim).ToList();
            }
            if (idAccount != null)
            {
                var idCharges = registryContext.KumiCharges.Where(r => r.IdAccount == idAccount).Select(r => r.IdCharge).ToList();
                var idClaims = registryContext.Claims.Where(r => r.IdAccountKumi != null && r.IdAccountKumi == idAccount)
                    .Select(r => r.IdClaim).ToList();

                return registryContext.KumiPaymentsUntied.Where(r => idCharges.Contains(r.IdCharge))
                    .Union(
                        registryContext.KumiPaymentsUntied.Where(r => r.IdClaim != null && idClaims.Contains(r.IdClaim.Value))
                    ).Distinct().ToList();
            }
            return new List<KumiPaymentUntied>();
        }

        public void UpdateDescription(int idPayment, string description)
        {
            var payment = registryContext.KumiPayments.FirstOrDefault(r => r.IdPayment == idPayment);
            payment.Description = description;
            registryContext.SaveChanges();
        }

        private IQueryable<KumiPayment> RecipientFilter(IQueryable<KumiPayment> query, KumiPaymentsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.RecipientAccount))
            {
                query = query.Where(r => r.RecipientAccount != null && r.RecipientAccount.Contains(filterOptions.RecipientAccount));
            }
            if (!string.IsNullOrEmpty(filterOptions.RecipientInn))
            {
                query = query.Where(r => r.RecipientInn != null && r.RecipientInn.Contains(filterOptions.RecipientInn));
            }
            if (!string.IsNullOrEmpty(filterOptions.RecipientKpp))
            {
                query = query.Where(r => r.RecipientKpp != null && r.RecipientKpp.Contains(filterOptions.RecipientKpp));
            }
            if (!string.IsNullOrEmpty(filterOptions.RecipientName))
            {
                query = query.Where(r => r.RecipientName != null && r.RecipientName.Contains(filterOptions.RecipientName));
            }
            if (!string.IsNullOrEmpty(filterOptions.RecipientBankName))
            {
                query = query.Where(r => r.RecipientBankName != null && r.RecipientName.Contains(filterOptions.RecipientBankName));
            }
            if (!string.IsNullOrEmpty(filterOptions.RecipientBankBik))
            {
                query = query.Where(r => r.RecipientBankBik != null && r.RecipientName.Contains(filterOptions.RecipientBankBik));
            }
            if (!string.IsNullOrEmpty(filterOptions.RecipientBankAccount))
            {
                query = query.Where(r => r.RecipientBankAccount != null && r.RecipientName.Contains(filterOptions.RecipientBankAccount));
            }
            return query;
        }

        public int? GetKumiPaymentUfsLastNumber()
        {
            var lastNum = registryContext.KumiPaymentUfs.LastOrDefault()?.NumUf;
            if (int.TryParse(lastNum, out int lastNumInt)) return lastNumInt;
            return null;
        }

        public SelectableSigner GetSigner(int idSigner)
        {
            return registryContext.SelectableSigners.FirstOrDefault(r => r.IdRecord == idSigner);
        }

        public KumiPaymentUf GetKumiPaymentUf(int idPaymentUf)
        {
            return registryContext.KumiPaymentUfs
                .Include(r => r.KbkType)
                .Include(r => r.Payment).ThenInclude(r => r.PaymentDocCode)
                .Include(r => r.Payment).ThenInclude(r => r.KbkType)
                .AsNoTracking()
                .FirstOrDefault(r => r.IdPaymentUf == idPaymentUf);
        }

        public List<KumiPaymentUf> GetKumiPaymentUfs(DateTime dateUf)
        {
            return registryContext.KumiPaymentUfs
                    .Include(r => r.KbkType)
                    .Include(r => r.Payment).ThenInclude(r => r.PaymentDocCode)
                    .Include(r => r.Payment).ThenInclude(r => r.KbkType)
                    .AsNoTracking()
                    .Where(r => r.DateUf == dateUf).ToList();
        }

        public byte[] GetPaymentUfsFile(List<KumiPaymentUf> paymentUfs, KumiPaymentSettingSet paymentSettings, SelectableSigner signer, DateTime signDate)
        {
            return new TXUF180101FileCreator().CreateFile(paymentUfs, paymentSettings, securityService.Executor, signer, signDate);
        }

        public KumiPaymentSettingSet GetKumiPaymentSettings()
        {
            return registryContext.KumiPaymentSettingSets.FirstOrDefault();
        }

        private IQueryable<KumiPayment> PayerFilter(IQueryable<KumiPayment> query, KumiPaymentsFilter filterOptions)
        {
            if(!string.IsNullOrEmpty(filterOptions.PayerAccount))
            {
                query = query.Where(r => r.PayerAccount != null && r.PayerAccount.Contains(filterOptions.PayerAccount));
            }
            if (!string.IsNullOrEmpty(filterOptions.PayerInn))
            {
                query = query.Where(r => r.PayerInn != null && r.PayerInn.Contains(filterOptions.PayerInn));
            }
            if (!string.IsNullOrEmpty(filterOptions.PayerKpp))
            {
                query = query.Where(r => r.PayerKpp != null && r.PayerKpp.Contains(filterOptions.PayerKpp));
            }
            if (!string.IsNullOrEmpty(filterOptions.PayerName))
            {
                query = query.Where(r => r.PayerName != null && r.PayerName.Contains(filterOptions.PayerName));
            }
            if (!string.IsNullOrEmpty(filterOptions.PayerBankName))
            {
                query = query.Where(r => r.PayerBankName != null && r.PayerName.Contains(filterOptions.PayerBankName));
            }
            if (!string.IsNullOrEmpty(filterOptions.PayerBankBik))
            {
                query = query.Where(r => r.PayerBankBik != null && r.PayerName.Contains(filterOptions.PayerBankBik));
            }
            if (!string.IsNullOrEmpty(filterOptions.PayerBankAccount))
            {
                query = query.Where(r => r.PayerBankAccount != null && r.PayerName.Contains(filterOptions.PayerBankAccount));
            }
            return query;
        }
        public KumiPayment CreateByMemorialOrder(int idOrder)
        {
            var mo = registryContext.KumiMemorialOrders.Include(r => r.MemorialOrderPaymentAssocs)
                .FirstOrDefault(r => r.IdOrder == idOrder);

            if (mo == null) throw new ApplicationException("Не удалось найти мемориальный ордер");

            if (mo.MemorialOrderPaymentAssocs != null && mo.MemorialOrderPaymentAssocs.Any())
                throw new ApplicationException("Мемориальный оредр уже привязан к платежу");

            if (mo.SumZach < 0)
                throw new ApplicationException("Нельзя создать платеж на основе ордера с отрицательной суммой");

            var payment = new KumiPayment {
                IdSource = 1,
                Sum = mo.SumZach,
                Kbk = mo.Kbk,
                IdKbkType = mo.IdKbkType,
                Guid = mo.Guid,
                Okato = mo.Okato,
                RecipientInn = mo.InnAdb,
                RecipientKpp = mo.KppAdb,
                TargetCode = mo.TargetCode,
                MemorialOrderPaymentAssocs = new List<KumiMemorialOrderPaymentAssoc> {
                    new KumiMemorialOrderPaymentAssoc
                    {
                        IdOrder = idOrder
                    }
                }
            };
            registryContext.KumiPayments.Add(payment);
            registryContext.SaveChanges();
            return payment;

        }

        public void ApplyMemorialOrderToPayment(int idPayment, int idOrder, out bool updatedExistsPayment)
        {
            updatedExistsPayment = true;
            // Если ордер уже подвязан, то пропускаем
            var mo = registryContext.KumiMemorialOrders.Include(r => r.MemorialOrderPaymentAssocs)
                .FirstOrDefault(r => r.IdOrder == idOrder);

            if (mo == null) throw new ApplicationException("Не удалось найти мемориальный ордер");

            if (mo.MemorialOrderPaymentAssocs != null && mo.MemorialOrderPaymentAssocs.Any())
                throw new ApplicationException("Мемориальный оредр уже привязан к платежу");

            var payment = GetQuery().FirstOrDefault(r => r.IdPayment == idPayment);

            if (payment == null) throw new ApplicationException("Не удалось найти платеж");

            if ((payment.PaymentCharges != null && payment.PaymentCharges.Any()) || (payment.PaymentClaims != null && payment.PaymentClaims.Any()))
                throw new ApplicationException("Платеж распределен. Для привязки мемориального ордера необходимо отменить распределение платежа");

            if (payment.IsConsolidated != 0)
                throw new ApplicationException("Платеж является сводным платежным поручением. Нельзя уточнять сводные платежные поручения после подругзки детализирующего реестра платежей");

            var idParentPayment = payment.IdPayment;

            var copyPayment = false;

            ClearMemorialOrderFieldsValues(mo);

           if (mo.SumZach >= 0 &&
                (payment.Kbk != mo.Kbk || payment.IdKbkType != mo.IdKbkType || payment.TargetCode != mo.TargetCode ||
                    payment.Okato != mo.Okato || payment.RecipientInn != mo.InnAdb || payment.RecipientKpp != mo.KppAdb))
            {
                copyPayment = true;
            }
           

            if (copyPayment)
                payment = payment.Copy(true);

            payment = ApplyMemorialOrder(payment, mo);

            if (payment.IdPayment != 0)
            {
                var corrections = payment.PaymentCorrections;
                payment.PaymentCorrections = null;
                registryContext.KumiPayments.Update(payment);
                foreach (var correction in corrections.Where(r => r.IdCorrection == 0))
                {
                    correction.IdPayment = payment.IdPayment;
                    registryContext.KumiPaymentCorrections.Add(correction);
                }
                updatedExistsPayment = true;
            }
            else
            {
                payment.IdParentPayment = idParentPayment;
                registryContext.KumiPayments.Add(payment);
                updatedExistsPayment = false;
            }
            registryContext.SaveChanges();
        }

        public List<KumiMemorialOrder> GetKumiPaymentMemorialOrderPairs(int idOrder)
        {
            var order = registryContext.KumiMemorialOrders.FirstOrDefault(r => r.IdOrder == idOrder);
            if (order == null) throw new Exception("Не удалось найти мемориальный ордер");
            return registryContext.KumiMemorialOrders.Where(r => r.NumDocument == order.NumDocument && r.DateDocument == order.DateDocument
                        && r.IdGroup == order.IdGroup).ToList();
        }

        private void ClearMemorialOrderFieldsValues(KumiMemorialOrder mo)
        {
            if (string.IsNullOrEmpty(mo.InnAdb))
                mo.InnAdb = null;
            if (string.IsNullOrEmpty(mo.KppAdb))
                mo.KppAdb = null;
            if (string.IsNullOrEmpty(mo.TargetCode))
                mo.TargetCode = null;
            if (string.IsNullOrEmpty(mo.Kbk))
                mo.Kbk = null;
            if (string.IsNullOrEmpty(mo.NumDocument))
                mo.NumDocument = null;
        }

        public IQueryable<KumiMemorialOrder> GetMemorialOrders(MemorialOrderFilter filterOptions)
        {
            var query = registryContext.KumiMemorialOrders.Include(mo => mo.MemorialOrderPaymentAssocs).Where(mo => mo.MemorialOrderPaymentAssocs.Count == 0);
            if (!string.IsNullOrEmpty(filterOptions.NumDocument))
                query = query.Where(r => r.NumDocument == filterOptions.NumDocument);
            if (filterOptions.DateDocument != null)
                query = query.Where(r => r.DateDocument == filterOptions.DateDocument);
            if (filterOptions.Sum != null)
                query = query.Where(r => r.SumZach == filterOptions.Sum);
            if (!string.IsNullOrEmpty(filterOptions.Kbk))
                query = query.Where(r => r.Kbk != null && r.Kbk.Contains(filterOptions.Kbk));
            if (!string.IsNullOrEmpty(filterOptions.Okato))
                query = query.Where(r => r.Okato != null && r.Okato.Contains(filterOptions.Okato));
            return query;
        }

        private IQueryable<KumiPayment> CommonPaymentFilter(IQueryable<KumiPayment> query, KumiPaymentsFilter filterOptions)
        {
            if (filterOptions.IdsSource != null && filterOptions.IdsSource.Any())
            {
                query = query.Where(r => filterOptions.IdsSource.Contains(r.IdSource));
            }
            if (filterOptions.LoadDate != null)
            {
                var endDate = filterOptions.LoadDate.Value.Date.AddDays(1);
                query = query.Where(r => r.PaymentGroup.Date >= filterOptions.LoadDate && r.PaymentGroup.Date < endDate);
            }
            if (filterOptions.IsPosted != null)
            {
                var isPosted = filterOptions.IsPosted.Value ? 1 : 0;
                query = query.Where(r => (r.IsPosted == isPosted && r.Sum != 0) || (isPosted == 1 && r.Sum == 0));
            }
            if (!string.IsNullOrEmpty(filterOptions.NumDocument))
            {
                query = query.Where(r => r.NumDocument != null && r.NumDocument.Contains(filterOptions.NumDocument));
            }
            if (filterOptions.DateDocument != null)
            {
                query = query.Where(r => r.DateDocument == filterOptions.DateDocument);
            }
            if (filterOptions.DateIn != null)
            {
                query = query.Where(r => r.DateIn == filterOptions.DateIn);
            }
            if (filterOptions.DateExecute != null)
            {
                query = query.Where(r => r.DateExecute == filterOptions.DateExecute);
            }
            if (filterOptions.DateEnrollUfk != null)
            {
                query = query.Where(r => r.DateEnrollUfk == filterOptions.DateEnrollUfk);
            }
            if (!string.IsNullOrEmpty(filterOptions.Uin))
            {
                query = query.Where(r => r.Uin != null && r.Uin.Contains(filterOptions.Uin));
            }
            if (filterOptions.Sum != null)
            {
                query = query.Where(r => r.Sum == filterOptions.Sum);
            }
            if (!string.IsNullOrEmpty(filterOptions.Purpose))
            {
                query = query.Where(r => r.Purpose != null && r.Purpose.Contains(filterOptions.Purpose));
            }
            if (!string.IsNullOrEmpty(filterOptions.Kbk))
            {
                query = query.Where(r => r.Kbk != null && r.Kbk.Contains(filterOptions.Kbk));
            }
            if (!string.IsNullOrEmpty(filterOptions.Okato))
            {
                query = query.Where(r => r.Okato != null && r.Okato.Contains(filterOptions.Okato));
            }
            return query;
        }

        public void UpdateBksPaymentsDateEnrollUfkForm(DateTime dateDoc, DateTime dateEnrollUfk)
        {
            var payments = registryContext.KumiPayments.Where(r => r.DateDocument == dateDoc && r.IdSource == 6);
            foreach(var payment in payments)
            {
                payment.DateEnrollUfk = dateEnrollUfk;
            }
            registryContext.SaveChanges();
        }

        public KumiPaymentDistributionInfo CancelDistributePaymentToAccount(int idPayment, List<int> idClaims, List<int> idAccounts)
        {
            var payment = registryContext.KumiPayments.Include(r => r.PaymentClaims).Include(r => r.PaymentCharges).FirstOrDefault(r => r.IdPayment == idPayment);
            if (payment == null)
                throw new ApplicationException("Не найдена платеж в базе данных");

            var idClaimsLocal = idClaims.ToList();
            List<int> idAccountsLocal =
                idAccounts.Union(
                registryContext.Claims.Where(r => r.IdAccountKumi != null && idClaimsLocal.Contains(r.IdClaim)).Select(r => r.IdAccountKumi.Value)).Distinct().ToList();

            IQueryable<KumiAccount> accounts = registryContext.KumiAccounts.Where(r => idAccountsLocal.Contains(r.IdAccount));

            foreach(var account in accounts)
            {
                if (account.IdState == 2)
                    throw new ApplicationException("Нельзя отменить распределение, т.к. связанный лицевой счет аннулирован");
            }

            foreach(var paymentClaim in payment.PaymentClaims.GroupBy(r => r.IdClaim))
            {
                if (!idClaims.Contains(paymentClaim.Key)) continue;
                var claim = registryContext.Claims.FirstOrDefault(r => r.IdClaim == paymentClaim.Key);
                if (claim == null)
                    throw new ApplicationException(
                        string.Format("Произошла ошибка во время отмены распределения платежа. Не найдена исковая работа с идентификатором {0}", claim.IdClaim));

                claim.AmountTenancyRecovered = (claim.AmountTenancyRecovered ?? 0) - paymentClaim.Select(r => r.TenancyValue).Sum();
                claim.AmountPenaltiesRecovered = (claim.AmountPenaltiesRecovered ?? 0) - paymentClaim.Select(r => r.PenaltyValue).Sum();
                claim.AmountDgiRecovered = (claim.AmountDgiRecovered ?? 0) - paymentClaim.Select(r => r.DgiValue).Sum();
                claim.AmountPkkRecovered = (claim.AmountPkkRecovered ?? 0) - paymentClaim.Select(r => r.PkkValue).Sum();
                claim.AmountPadunRecovered = (claim.AmountPadunRecovered ?? 0) - paymentClaim.Select(r => r.PadunValue).Sum();
                registryContext.Claims.Update(claim);
            }

            foreach(var paymentClaim in payment.PaymentClaims)
            {
                if (!idClaims.Contains(paymentClaim.IdClaim)) continue;
                registryContext.KumiPaymentClaims.Remove(paymentClaim);
            }

            foreach (var paymentCharge in payment.PaymentCharges)
            {
                var charge = registryContext.KumiCharges.Where(r => r.IdCharge == paymentCharge.IdCharge).FirstOrDefault();
                if (charge == null || !idAccounts.Contains(charge.IdAccount)) continue;
                registryContext.KumiPaymentCharges.Remove(paymentCharge);
            }

            payment.IsPosted = 0;

            var parentPayment = registryContext.KumiPayments.FirstOrDefault(r => r.IdPayment == payment.IdParentPayment);
            KumiPayment consolidatedParentPayment = null;
            while (parentPayment != null)
            {
                if (parentPayment.IsConsolidated != 0)
                {
                    consolidatedParentPayment = parentPayment;
                    break;
                }
                else
                if (parentPayment.IdParentPayment != null)
                    parentPayment = registryContext.KumiPayments.FirstOrDefault(r => r.IdPayment == parentPayment.IdParentPayment);
                else
                    break;
            }
            if (consolidatedParentPayment != null)
            {
                consolidatedParentPayment.IsPosted = 0;
                registryContext.KumiPayments.Update(consolidatedParentPayment);
            }

            registryContext.SaveChanges();
            registryContext.DetachAllEntities();

            // Recalculate
            var startRewriteDate = DateTime.Now.Date;
            startRewriteDate = startRewriteDate.AddDays(-startRewriteDate.Day + 1);


            var endCalcDate = DateTime.Now.Date;
            endCalcDate = endCalcDate.AddDays(-endCalcDate.Day + 1).AddMonths(1).AddDays(-1);

            if (DateTime.Now.Date.Day >= 25) // Предыдущий период блокируется для перезаписи по истечении трех дней текущего периода
            {
                startRewriteDate = startRewriteDate.AddMonths(1);
                endCalcDate = endCalcDate.AddDays(1).AddMonths(1).AddDays(-1);
            }

            kumiAccountsDataService.RecalculateAccounts(accounts, startRewriteDate, endCalcDate, false);  

            return new KumiPaymentDistributionInfo
            {
                IdPayment = idPayment,
                Sum = payment.Sum,
                DistrubutedToTenancySum = 0,
                DistrubutedToPenaltySum = 0,
                DistrubutedToDgiSum = 0,
                DistrubutedToPkkSum = 0,
                DistrubutedToPadunSum = 0
            };
        }

        public List<KumiPayment> SearchPaymentsRaw(string text)
        {
            var terms = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            DateTime? docDate = null;
            decimal? sum = null;
            string docNum = null;
            foreach (var term in terms)
            {
                var termPrepared = term.Trim(new char[] { '№', '#', '.', ',' });
                if (termPrepared.Length == 10 && new Regex(@"^[0-9]{2}\.[0-9]{2}\.[0-9]{4}$").IsMatch(termPrepared))
                {
                    // Дата документа
                    var dateParts = termPrepared.Split('.');
                    docDate = new DateTime(int.Parse(dateParts[2]), int.Parse(dateParts[1]), int.Parse(dateParts[0]));
                } else
                if (int.TryParse(termPrepared, out int digBuffer))
                {
                    // Номер документа
                    docNum = digBuffer.ToString();
                } else
                if (decimal.TryParse(termPrepared.Replace(".", ","), out decimal sumBuffer))
                {
                    // Сумма платежа
                    sum = sumBuffer;
                }
            }
            if (docDate == null && sum == null && string.IsNullOrEmpty(docNum)) return new List<KumiPayment>();
            return registryContext.KumiPayments.Where(r => 
                (docNum == null || r.NumDocument == docNum) &&
                (docDate == null || r.DateDocument == docDate) &&
                (sum == null || r.Sum == sum)).Take(10).ToList();
        }

        public Dictionary<int, string> GetAccountsTenants(IEnumerable<KumiAccount> accounts)
        {
            var tenants = registryContext.GetTenantsByAccountIds(accounts.Select(r => r.IdAccount).ToList());
            var result = new Dictionary<int, string>();
            foreach (var account in accounts)
            {
                var tenant = account.Owner;
                if (string.IsNullOrWhiteSpace(tenant))
                    tenant = tenants.FirstOrDefault(r => r.IdAccount == account.IdAccount)?.Tenant;
                if (!result.ContainsKey(account.IdAccount))
                    result.Add(account.IdAccount, tenant);
            }
            return result;
        }

        public List<RegistryTuple<KumiPayment, KumiAccount>> AutoDistributeUploadedPayments(List<KumiPayment> insertedPayments)
        {
            var filteredPaymentsInfo = AutoDistributeFilterPayments(insertedPayments);
            var accountsInfo = new Dictionary<int, KumiAccount>();
            foreach (var paymentInfo in filteredPaymentsInfo)
            {
                var account = (from aRow in registryContext.KumiAccounts.Include(r => r.Charges).AsNoTracking()
                              where aRow.Account == paymentInfo.Key.Item1
                               select aRow).FirstOrDefault();
                if (account != null) accountsInfo.Add(paymentInfo.Value.IdPayment, account);
            }
            var accountIds = accountsInfo.Select(r => r.Value.IdAccount).ToList();
            var tenantsInfo = registryContext.GetTenantsByAccountIds(accountIds);
            var result = new List<RegistryTuple<KumiPayment, KumiAccount>>();
            foreach (var paymentInfo in filteredPaymentsInfo)
            {
                var accounts = accountsInfo.Where(r => r.Key == paymentInfo.Value.IdPayment);
                if (!accounts.Any()) continue;
                var account = accounts.First();
                var tenantInfo = tenantsInfo.FirstOrDefault(r => r.IdAccount == account.Value.IdAccount);
                var tenant = string.IsNullOrWhiteSpace(account.Value.Owner) ? tenantInfo?.Tenant : account.Value.Owner;
                if (tenant == null) continue;
                var paymentTenant = (paymentInfo.Key.Item2 ?? "").Split("$$", 2)[0];
                var paymentTenantParts = paymentTenant.ToLowerInvariant().Split(new char[] { '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                var tenantParts = tenant.ToLowerInvariant().Split(" ");
                if (paymentTenantParts.Length != tenantParts.Length) continue;
                var tenantEqual = true;
                for(var i = 0; i < paymentTenantParts.Length; i++)
                {
                    var initial = paymentTenantParts[i];
                    if (!(paymentTenantParts[i] == tenantParts[i] ||
                        (initial.Length == 1 && initial == tenantParts[i].Substring(0, 1))))
                        tenantEqual = false;
                }
                if (!tenantEqual) continue;

                var chargeTenancy = (account.Value.LastCalcDate > DateTime.Now.Date) ? (account.Value.CurrentBalanceTenancy - (account.Value.Charges
                                .FirstOrDefault(c => c.EndDate == account.Value.LastChargeDate)?.ChargeTenancy ?? 0)) : account.Value.CurrentBalanceTenancy ?? 0;
                var chargePenalty = (account.Value.LastCalcDate > DateTime.Now.Date) ? (account.Value.CurrentBalancePenalty - (account.Value.Charges
                                .FirstOrDefault(c => c.EndDate == account.Value.LastChargeDate)?.ChargePenalty ?? 0)) : account.Value.CurrentBalancePenalty ?? 0;
                var chargeDgi = (account.Value.LastCalcDate > DateTime.Now.Date) ? (account.Value.CurrentBalanceDgi - (account.Value.Charges
                                .FirstOrDefault(c => c.EndDate == account.Value.LastChargeDate)?.ChargeDgi ?? 0)) : account.Value.CurrentBalanceDgi ?? 0;
                var chargePkk = (account.Value.LastCalcDate > DateTime.Now.Date) ? (account.Value.CurrentBalancePkk - (account.Value.Charges
                                .FirstOrDefault(c => c.EndDate == account.Value.LastChargeDate)?.ChargePkk ?? 0)) : account.Value.CurrentBalancePkk ?? 0;
                var chargePadun = (account.Value.LastCalcDate > DateTime.Now.Date) ? (account.Value.CurrentBalancePadun - (account.Value.Charges
                                .FirstOrDefault(c => c.EndDate == account.Value.LastChargeDate)?.ChargePadun ?? 0)) : account.Value.CurrentBalancePadun ?? 0;

                try
                {
                    if (paymentInfo.Value.Kbk == "90111109044041000120" && paymentInfo.Value.Sum == chargeTenancy + chargePenalty + chargePkk + chargePadun)
                    {
                        DistributePaymentToAccount(paymentInfo.Value.IdPayment, account.Value.IdAccount, KumiPaymentDistributeToEnum.ToKumiAccount,
                        chargeTenancy ?? 0, chargePenalty ?? 0, 0, chargePkk ?? 0, chargePadun ?? 0);
                    }
                    else

                    if (paymentInfo.Value.Kbk == "90111705040041111180" && paymentInfo.Value.Sum == chargeDgi)
                    {
                        DistributePaymentToAccount(paymentInfo.Value.IdPayment, account.Value.IdAccount, 
                            KumiPaymentDistributeToEnum.ToKumiAccount, 0, 0, chargeDgi ?? 0, 0, 0);
                    }
                    else continue;
                    
                } catch {
                    continue;
                }
                result.Add(new RegistryTuple<KumiPayment, KumiAccount> { Item1 = paymentInfo.Value, Item2 = account.Value });
            }
            return result;
        }

        private Dictionary<Tuple<string, string>, KumiPayment> AutoDistributeFilterPayments(List<KumiPayment> insertedPayments)
        {
            var result = new Dictionary<Tuple<string, string>, KumiPayment>();
            var i = 0;
            foreach (var payment in insertedPayments)
            {
                if (!new[] { "90111109044041000120", "90111705040041111180" }.Contains(payment.Kbk)) continue;
                if (payment.Sum == 0) continue;
                var purpose = payment.Purpose;
                if (string.IsNullOrEmpty(purpose)) continue;
                var courtOrderRegex = new Regex(@"(ИД)[ ]*([0-9][-][0-9]{1,6}[ ]?[\/][ ]?([0-9]{4}|[0-9]{2}))");
                if (courtOrderRegex.IsMatch(purpose)) continue;

                var tenant = (string)null;
                var account = (string)null;
                var accountRegex = new Regex(@"(ЛИЦ(\.?|ЕВОЙ)?[ ]+СЧЕТ|ЛС)[ ]*[:]?[ ]*([0-9]{6,})");
                var accountMatch = accountRegex.Match(purpose);
                if (accountMatch.Success)
                    account = accountMatch.Groups[3].Value;

                var payerRegExp1 = new Regex(@"ФИО_ПЛАТЕЛЬЩИКА:([а-яА-Я ]+)");
                if (payment.IdSource == 6)
                    tenant = payment.PayerName;
                else
                if (new Regex(@"\/\/без НДС$").IsMatch(purpose))
                {
                    var purposeParts = purpose.Split("//");
                    if (purposeParts.Length > 3)
                        tenant = purposeParts[purposeParts.Length - 3];
                }
                else
                if (payerRegExp1.IsMatch(purpose))
                {
                    var payerMatch = payerRegExp1.Match(purpose);
                    tenant = payerMatch.Groups[1].Value;
                }
                else
                if (payment.PayerName != null && new Regex("^[а-яА-Я]+[ ][а-яА-Я]+([ ][а-яА-Я]+)?$").IsMatch(payment.PayerName))
                {
                    tenant = payment.PayerName;
                }
                if (account == null || tenant == null) continue;
                result.Add(new Tuple<string, string>(account, tenant+"$$"+i.ToString()), payment);
                i++;
            }
            return result;
        }

        public KumiPaymentDistributionInfo DistributePaymentToAccount(int idPayment, int idObject, KumiPaymentDistributeToEnum distributeTo, 
            decimal tenancySum, decimal penaltySum, decimal dgiSum, decimal pkkSum, decimal padunSum)
        {
            var payment = registryContext.KumiPayments.Include(r => r.PaymentClaims).Include(r => r.PaymentCharges).FirstOrDefault(r => r.IdPayment == idPayment);
            if (payment == null)
                throw new ApplicationException("Не найдена платеж в базе данных");

            if (payment.IsConsolidated != 0)
                throw new ApplicationException("Платеж помечен как сводное платежное поручение и не может быть распределен. Вместо него необходимо распределять детализирующие платежи");

            if (tenancySum + penaltySum + dgiSum + pkkSum + padunSum == 0)
                throw new ApplicationException("Не указана распределяемая сумма");

            if (payment.IdSource != 8 && tenancySum < 0)
                throw new ApplicationException("Указана отрицательная сумма, распределяемая на найм. Указание отрицательной суммы разрешено только для возвратов");

            if (payment.IdSource != 8 && penaltySum < 0)
                throw new ApplicationException("Указана отрицательная сумма, распределяемая на пени. Указание отрицательной суммы разрешено только для возвратов");

            if (payment.IdSource != 8 && dgiSum < 0)
                throw new ApplicationException("Указана отрицательная сумма, распределяемая на ДГИ. Указание отрицательной суммы разрешено только для возвратов");

            if (payment.IdSource != 8 && pkkSum < 0)
                throw new ApplicationException("Указана отрицательная сумма, распределяемая на ПКК. Указание отрицательной суммы разрешено только для возвратов");

            if (payment.IdSource != 8 && padunSum < 0)
                throw new ApplicationException("Указана отрицательная сумма, распределяемая на Падун. Указание отрицательной суммы разрешено только для возвратов");

            if (!new[] { "90111109044041000120", "90111705040041111180" }.Contains(payment.Kbk))
                throw new ApplicationException(string.Format("Нельзя распределить платеж с КБК {0}. Допускаются только платежи с КБК 90111109044041000120 (плата за наем) и 90111705040041111180 (возмещение ДГИ)", payment.Kbk));
       
            var distributedTenancySum = payment.PaymentCharges.Select(r => r.TenancyValue).Sum() +
                payment.PaymentClaims.Select(r => r.TenancyValue).Sum();
            var distributedPenaltySum = payment.PaymentCharges.Select(r => r.PenaltyValue).Sum() +
                payment.PaymentClaims.Select(r => r.PenaltyValue).Sum();
            var distributedDgiSum = payment.PaymentCharges.Select(r => r.DgiValue).Sum() +
                payment.PaymentClaims.Select(r => r.DgiValue).Sum();
            var distributedPkkSum = payment.PaymentCharges.Select(r => r.PkkValue).Sum() +
                payment.PaymentClaims.Select(r => r.PkkValue).Sum();
            var distributedPadunSum = payment.PaymentCharges.Select(r => r.PadunValue).Sum() +
                payment.PaymentClaims.Select(r => r.PadunValue).Sum();

            if (Math.Abs(payment.Sum) < Math.Abs(distributedTenancySum + distributedPenaltySum + distributedDgiSum + distributedPkkSum + distributedPadunSum 
                + tenancySum + penaltySum + dgiSum + pkkSum + padunSum))
                throw new ApplicationException(string.Format("Распределяемая сумма {0} превышает остаток по платежу {1}", 
                    tenancySum + penaltySum + dgiSum + pkkSum + padunSum,
                    payment.Sum - distributedTenancySum - distributedPenaltySum - distributedDgiSum - distributedPkkSum - distributedPadunSum));

            if (payment.Kbk == "90111705040041111180" && (tenancySum != 0 || penaltySum != 0 || pkkSum != 0 || padunSum != 0))
                throw new ApplicationException("Платежи с КБК 90111705040041111180 (возмещение ДГИ) можно распределять только на задолженность ДГИ");

            if (payment.Kbk == "90111109044041000120" && (dgiSum != 0))
                throw new ApplicationException("Платежи с КБК 90111109044041000120 (плата за наем) нельзя распределять на задолженность ДГИ");

            IQueryable<KumiAccount> accounts = null;

            switch (distributeTo)
            {
                case KumiPaymentDistributeToEnum.ToKumiAccount:
                    accounts = registryContext.KumiAccounts.Where(r => r.IdAccount == idObject);
                    if (accounts.Count() == 0)
                        throw new ApplicationException(string.Format("Не найден лицевой счет с реестровым номером {0}", idObject));

                    var account = accounts.First();
                    if (accounts.First().IdState == 2)
                        throw new ApplicationException(string.Format("Нельзя распределить платеж на аннулированный лицевой счет {0}", account.Account));

                    var date = DateTime.Now.Date;
                    var startPeriodDate = date.AddDays(-date.Day + 1);
                    var endPeriodDate = date.AddDays(-date.Day + 1).AddMonths(1).AddDays(-1);
                    if (date.Day >= 25)
                    {
                        startPeriodDate = startPeriodDate.AddMonths(1);
                        endPeriodDate = endPeriodDate.AddDays(1).AddMonths(1).AddDays(-1);
                    }
                    var charge = registryContext.KumiCharges.AsNoTracking()
                        .FirstOrDefault(r => r.IdAccount == idObject && r.StartDate == startPeriodDate && r.EndDate == endPeriodDate);

                    var paymentCharge = new KumiPaymentCharge
                    {
                        IdPayment = idPayment,
                        TenancyValue = tenancySum,
                        PenaltyValue = penaltySum,
                        DgiValue = dgiSum,
                        PkkValue = pkkSum,
                        PadunValue = padunSum,
                        Date = DateTime.Now.Date
                    };

                    if (charge == null)
                    {
                        charge = new KumiCharge
                        {
                            IdAccount = idObject,
                            StartDate = startPeriodDate,
                            EndDate = endPeriodDate,
                            PaymentTenancy = tenancySum,
                            PaymentPenalty = penaltySum,
                            PaymentDgi = dgiSum,
                            PaymentPkk = pkkSum,
                            PaymentPadun = padunSum,
                            Hidden = 1
                        };
                        paymentCharge.Charge = charge;
                        registryContext.KumiCharges.Add(charge);
                    } else
                    {
                        paymentCharge.IdCharge = charge.IdCharge;
                    }
                    registryContext.KumiPaymentCharges.Add(paymentCharge);
                    break;
                case KumiPaymentDistributeToEnum.ToClaim:
                    var claim = registryContext.Claims.Include(r => r.ClaimStates).FirstOrDefault(r => r.IdClaim == idObject);
                    var idAccount = claim.IdAccountKumi;
                    if (claim == null)
                        throw new ApplicationException(string.Format("Не найдена исковая работа с реестровым номером {0}", idObject));
                    if (claim.ClaimStates.Any())
                    {
                        var lastState = claim.ClaimStates.Last();
                        if (lastState.IdStateType == 6)
                            throw new ApplicationException(string.Format("Нельзя распределить платеж на завершенную исковую работу", idObject));
                    }
                    if (idAccount == null)
                        throw new ApplicationException(string.Format("Исковая работа {0} не привязана к лицевому счету КУМИ", idObject));
                    accounts = registryContext.KumiAccounts.Where(r => r.IdAccount == idAccount);
                    
                    if (accounts.Count() == 0)
                        throw new ApplicationException(string.Format("Не найден лицевой счет с реестровым номером {0}", idAccount));

                    account = accounts.First();
                    if (accounts.First().IdState == 2)
                        throw new ApplicationException(string.Format("Нельзя распределить платеж на исковую работу, привязанную к аннулированному лицевому счету {0}", account.Account));

                    claim.AmountTenancyRecovered = (claim.AmountTenancyRecovered ?? 0) + tenancySum;
                    claim.AmountPenaltiesRecovered = (claim.AmountPenaltiesRecovered ?? 0) + penaltySum;
                    claim.AmountDgiRecovered = (claim.AmountDgiRecovered ?? 0) + dgiSum;
                    claim.AmountPkkRecovered = (claim.AmountPkkRecovered ?? 0) + pkkSum;
                    claim.AmountPadunRecovered = (claim.AmountPadunRecovered ?? 0) + padunSum;
                    registryContext.Claims.Update(claim);

                    registryContext.KumiPaymentClaims.Add(new KumiPaymentClaim
                    {
                        IdPayment = idPayment,
                        IdClaim = idObject,
                        TenancyValue = tenancySum,
                        PenaltyValue = penaltySum,
                        DgiValue = dgiSum,
                        PkkValue = pkkSum,
                        PadunValue = padunSum,
                        Date = DateTime.Now.Date
                    });
                    break;
            }

            if (payment.Sum == distributedTenancySum + distributedPenaltySum + distributedDgiSum + distributedPkkSum + distributedPadunSum 
                + tenancySum + penaltySum + dgiSum + pkkSum + padunSum)
            {
                payment.IsPosted = 1;
                registryContext.KumiPayments.Update(payment);
                
                if (payment.IdParentPayment != null)
                {
                    var parentPayment = registryContext.KumiPayments.FirstOrDefault(r => r.IdPayment == payment.IdParentPayment);
                    KumiPayment consolidatedParentPayment = null;
                    while (parentPayment != null)
                    {
                        if (parentPayment.IsConsolidated != 0)
                        {
                            consolidatedParentPayment = parentPayment;
                            break;
                        } else
                        if (parentPayment.IdParentPayment != null)
                            parentPayment = registryContext.KumiPayments.FirstOrDefault(r => r.IdPayment == parentPayment.IdParentPayment);
                        else
                            break;
                    }
                    if (consolidatedParentPayment != null)
                    {
                        var childPayments = registryContext.KumiPayments.Where(r => r.IdParentPayment == consolidatedParentPayment.IdPayment).ToList();
                        var resultChildPayments = new List<KumiPayment>();
                        while (true)
                        {
                            if (!childPayments.Any())
                                break;
                            resultChildPayments.AddRange(childPayments);
                            var childPaymentsIds = childPayments.Select(r => r.IdPayment).ToList();
                            childPayments = registryContext.KumiPayments.Where(r => r.IdParentPayment != null && 
                                childPaymentsIds.Contains(r.IdParentPayment.Value)).ToList();
                        }
                        var childPaymentsSum = resultChildPayments.Where(r => r.IsPosted == 1).Select(r => r.Sum).Sum();
                        if (consolidatedParentPayment.Sum == childPaymentsSum)
                        {
                            consolidatedParentPayment.IsPosted = 1;
                            registryContext.KumiPayments.Update(consolidatedParentPayment);
                        }
                    }
                }
            }

            registryContext.SaveChanges();

            registryContext.DetachAllEntities();

            // Recalculate
            var startRewriteDate = DateTime.Now.Date;
            startRewriteDate = startRewriteDate.AddDays(-startRewriteDate.Day + 1);


            var endCalcDate = DateTime.Now.Date;
            endCalcDate = endCalcDate.AddDays(-endCalcDate.Day + 1).AddMonths(1).AddDays(-1);

            if (DateTime.Now.Date.Day >= 25) // Предыдущий период блокируется для перезаписи по истечении трех дней текущего периода
            {
                startRewriteDate = startRewriteDate.AddMonths(1);
                endCalcDate = endCalcDate.AddDays(1).AddMonths(1).AddDays(-1);
            }
            
            kumiAccountsDataService.RecalculateAccounts(accounts, startRewriteDate, endCalcDate, false);

            return new KumiPaymentDistributionInfo
            {
                IdPayment = idPayment,
                Sum = payment.Sum,
                DistrubutedToTenancySum = distributedTenancySum + tenancySum,
                DistrubutedToPenaltySum = distributedPenaltySum + penaltySum,
                DistrubutedToDgiSum = distributedDgiSum + dgiSum,
                DistrubutedToPkkSum = distributedPkkSum + pkkSum,
                DistrubutedToPadunSum = distributedPadunSum + padunSum
            };
        }

        private IQueryable<KumiPayment> CommonFilter(IQueryable<KumiPayment> query, string commonFilter)
        {
            return query.Where(r =>
                (r.Uin != null && r.Uin.Contains(commonFilter)) ||
                (r.Purpose != null && r.Purpose.Contains(commonFilter)) ||
                (r.PayerInn != null && r.PayerInn.Contains(commonFilter)) ||
                (r.PayerKpp != null && r.PayerKpp.Contains(commonFilter)) ||
                (r.PayerName != null && r.PayerName.Contains(commonFilter)) ||
                (r.Kbk != null && r.Kbk.Contains(commonFilter)));
        }

        private IQueryable<KumiPayment> GetQueryPage(IQueryable<KumiPayment> query, PageOptions pageOptions)
        {
            return query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
        }

        private void UploadMemorialOrders(List<KumiMemorialOrder> memorialOrders, KumiPaymentGroup group, KumiPaymentsUploadStateModel loadState)
        {
            foreach (var mo in memorialOrders.OrderBy(r => r.Guid).ThenBy(r => r.NumDocument).ThenBy(r => r.DateDocument).ThenBy(r => r.SumZach))
            {
                try
                {
                    var localMo = BindDictionariesToMemorialOrder(mo);
                    localMo.PaymentGroup = group;
                    ClearMemorialOrderFieldsValues(mo);

                    var dbMo = registryContext.KumiMemorialOrders.Include(r => r.MemorialOrderPaymentAssocs).FirstOrDefault(
                        r => r.Guid == mo.Guid &&
                                r.NumDocument == localMo.NumDocument &&
                                r.DateDocument == localMo.DateDocument &&
                                r.SumIn == mo.SumIn &&
                                r.SumZach == mo.SumZach &&
                                r.IdKbkType == localMo.IdKbkType &&
                                r.Kbk == mo.Kbk &&
                                r.TargetCode == mo.TargetCode &&
                                r.Okato == mo.Okato &&
                                r.KppAdb == mo.KppAdb &&
                                r.InnAdb == mo.InnAdb);
                    if (dbMo == null)
                    {
                        registryContext.KumiMemorialOrders.Add(localMo);
                        loadState.InsertedMemorialOrders.Add(localMo);
                    }
                    else
                    {
                        localMo.IdOrder = dbMo.IdOrder;
                        localMo.MemorialOrderPaymentAssocs = dbMo.MemorialOrderPaymentAssocs;
                        loadState.SkipedMemorialOrders.Add(localMo);
                    }
                }
                catch (KumiPaymentBindDictionaryException e)
                {
                    loadState.MemorialOrdersDicitionaryBindErrors.Add(new RegistryTuple<KumiMemorialOrder, string>(mo, e.Message));
                    continue;
                }

                // Если ордер уже подвязан, то пропускаем
                if (mo.MemorialOrderPaymentAssocs != null && mo.MemorialOrderPaymentAssocs.Any()) continue;

                // Если сумма ордера отрицательная, то производим списание с платежа, соответствующего всем критериям
                // Если сумма положительная, то создаем новый платеж на основании имеющегося и обновляем целевые строки
                var dbPayments = registryContext.KumiPayments
                    .Include(r => r.MemorialOrderPaymentAssocs)
                    .Include(r => r.PaymentUfs)
                    .Include(r => r.PaymentCorrections)
                    .Include(r => r.PaymentCharges)
                    .Include(r => r.PaymentClaims)
                    .AsNoTracking()
                    .Where(r => r.Guid == mo.Guid &&
                        r.PaymentUfs.Count(ru => ru.NumUf == mo.NumDocument && ru.DateUf == mo.DateDocument) > 0);

                var dbConcretPayments = dbPayments.Where(r => r.Kbk == mo.Kbk && r.IdKbkType == mo.IdKbkType && r.TargetCode == mo.TargetCode &&
                       r.Okato == mo.Okato && r.RecipientInn == mo.InnAdb && r.RecipientKpp == mo.KppAdb);

                var copyPayment = true;

                if (dbConcretPayments.Count() > 0)
                {
                    dbPayments = dbConcretPayments;
                    copyPayment = false;
                }

                // Если платежей больше не строго один, то пропускаем
                if (dbPayments.Count() != 1) continue;

                var dbPayment = dbPayments.First();

                // Если платеж уже распределен, то пропускаем
                if ((dbPayment.PaymentCharges != null && dbPayment.PaymentCharges.Any()) || (dbPayment.PaymentClaims != null && dbPayment.PaymentClaims.Any()))
                    continue;

                var idParentPayment = dbPayment.IdPayment;

                if (copyPayment)
                    dbPayment = dbPayment.Copy(true);

                try
                {
                    dbPayment = ApplyMemorialOrder(dbPayment, mo);

                    loadState.BindedMemorialOrders.Add(new RegistryTuple<KumiMemorialOrder, KumiPayment>(mo, dbPayment));

                    if (dbPayment.IdPayment != 0)
                    {
                        var corrections = dbPayment.PaymentCorrections;
                        dbPayment.PaymentCorrections = null;
                        registryContext.KumiPayments.Update(dbPayment);
                        foreach (var correction in corrections.Where(r => r.IdCorrection == 0))
                        {
                            correction.IdPayment = dbPayment.IdPayment;
                            registryContext.KumiPaymentCorrections.Add(correction);
                        }
                    }
                    else
                    {
                        dbPayment.IdParentPayment = idParentPayment;
                        registryContext.KumiPayments.Add(dbPayment);
                    }
                }
                catch (KumiPaymentCheckVtOperException e)
                {
                    loadState.BindMemorialOrdersErrors.Add(new RegistryTuple<KumiMemorialOrder, string>(mo, e.Message));
                }
            }
        }

        private void UploadPayments(IEnumerable<KumiPayment> payments, KumiPaymentGroup group, List<KumiPaymentExtract> extracts, KumiPaymentsUploadStateModel loadState)
        {
            var appliedExtracts = new List<KumiPaymentExtract>();

            foreach (var payment in payments)
            {
                var paymentLocal = payment;

                // Подготовка платежа к загрузке
                paymentLocal.PaymentGroup = group;
                try
                {
                    paymentLocal = BindDictionariesToPayment(payment);
                    var paymentExtracts = extracts.Where(r => r.Guid == paymentLocal.Guid).ToList();
                    if (paymentExtracts.Any())
                    {
                        if (paymentExtracts.Count > 1)
                        {
                            throw new KumiPaymentCheckVtOperException(string.Format("Количество строк выписки по платежу {0} больше одной", payment.Guid));
                        }
                        var extract = paymentExtracts.First();
                        paymentLocal = ApplyPaymentExtract(paymentLocal, extract);
                        appliedExtracts.Add(extract);
                    }
                    else
                    {
                        loadState.PaymentsWithoutExtract.Add(paymentLocal);
                    }

                    switch (paymentLocal.IdSource)
                    {
                        case 4:
                        case 5:
                            loadState.UnknownPayments.Add(paymentLocal);
                            break;
                        case 2:
                        case 3:
                        case 6:
                        case 7:
                            loadState.KnownPayments.Add(paymentLocal);
                            break;
                        case 8:
                            loadState.ReturnPayments.Add(paymentLocal);
                            break;
                        default:
                            throw new KumiPaymentBindDictionaryException(string.Format("Неподдерживаемый источник платежа {0} - {1}", payment.Guid, payment.IdSource));
                    }
                }
                catch (KumiPaymentBindDictionaryException e)
                {
                    loadState.PaymentsDicitionaryBindErrors.Add(new RegistryTuple<KumiPayment, string>(payment, e.Message));
                    continue;
                }
                catch (KumiPaymentCheckVtOperException e)
                {
                    loadState.CheckExtractErrors.Add(new RegistryTuple<KumiPayment, string>(payment, e.Message));
                    continue;
                }

                // Загрузка в БД
                var dbPayments = registryContext.KumiPayments.Include(r => r.PaymentCharges)
                    .Include(r => r.PaymentClaims)
                    .Include(r => r.PaymentCorrections)
                    .Include(r => r.MemorialOrderPaymentAssocs)
                    .Include(r => r.PaymentUfs)
                    .AsNoTracking()
                    .Where(v => v.Guid == payment.Guid);

                if (dbPayments.Count() > 1)
                {
                    loadState.SkipedPayments.Add(payment);
                } else
                if (dbPayments.Count() == 1)
                {
                    var dbPayment = dbPayments.First();
                    if (dbPayment.PaymentClaims.Any() || dbPayment.PaymentCharges.Any() || dbPayment.PaymentCorrections.Any() ||
                            dbPayment.MemorialOrderPaymentAssocs.Any() || dbPayment.PaymentUfs.Any())
                    {
                        loadState.SkipedPayments.Add(payment);
                    }
                    else
                    {
                        payment.IdPayment = dbPayment.IdPayment;
                        registryContext.KumiPayments.Update(payment);
                        loadState.UpdatedPayments.Add(payment);
                    }
                } else
                {
                    registryContext.KumiPayments.Add(payment);
                    loadState.InsertedPayments.Add(payment);
                }
            }

            var notAppliedExtracts = extracts.Except(appliedExtracts);
            foreach(var extract in notAppliedExtracts)
            {
                // Загрузка в БД
                var dbPayments = registryContext.KumiPayments.Include(r => r.PaymentCharges)
                    .Include(r => r.PaymentClaims)
                    .Include(r => r.PaymentCorrections)
                    .Include(r => r.MemorialOrderPaymentAssocs)
                    .Include(r => r.PaymentUfs)
                    .Where(v => v.Guid == extract.Guid);
                if (dbPayments.Count() == 1)
                {
                    var dbPayment = dbPayments.First();
                    if (!dbPayment.PaymentClaims.Any() && !dbPayment.PaymentCharges.Any() && !dbPayment.PaymentCorrections.Any() &&
                            !dbPayment.MemorialOrderPaymentAssocs.Any() && !dbPayment.PaymentUfs.Any())
                    {
                        dbPayment = ApplyPaymentExtract(dbPayment, extract);
                        registryContext.KumiPayments.Update(dbPayment);
                        loadState.BindedExtractsToDbPayments.Add(dbPayment);
                    }
                } else
                {
                    loadState.UnknownPaymentExtracts.Add(extract);
                }
            }
        }

        // Buffer
        private List<KumiPaymentDocCode> kumiPaymentDocCodes { get; set; }

        private KumiPayment ApplyMemorialOrder(KumiPayment payment, KumiMemorialOrder memorialOrder)
        {
            AddPaymentCorrections(payment, memorialOrder);
            if (memorialOrder.SumZach < 0)
            {
                if (payment.Kbk != memorialOrder.Kbk || payment.IdKbkType != memorialOrder.IdKbkType || payment.TargetCode != memorialOrder.TargetCode ||
                    payment.Okato != memorialOrder.Okato || payment.RecipientInn != memorialOrder.InnAdb || payment.RecipientKpp != memorialOrder.KppAdb)
                    throw new KumiPaymentCheckVtOperException(string.Format("Несоответствие данных мемориального ордера на списание по платежу {0}", payment.Guid));
                payment.Sum = payment.Sum + memorialOrder.SumZach;
            }
            else
                payment.Sum = memorialOrder.SumZach;
            if (payment.Sum < 0)
            {
                throw new KumiPaymentCheckVtOperException(string.Format("При применении мемориального ордера к платежу {0} скорректированная сумма получилась отрицательной", payment.Guid));
            }
            payment.IdKbkType = memorialOrder.IdKbkType;
            payment.Kbk = memorialOrder.Kbk;
            payment.TargetCode = memorialOrder.TargetCode;
            payment.Okato = memorialOrder.Okato;
            payment.RecipientInn = memorialOrder.InnAdb;
            payment.RecipientKpp = memorialOrder.KppAdb;

            // Связываем мемориальные ордера с платежом
            if (payment.MemorialOrderPaymentAssocs == null)
            {
                payment.MemorialOrderPaymentAssocs = new List<KumiMemorialOrderPaymentAssoc>();
            }
            payment.MemorialOrderPaymentAssocs.Add(new KumiMemorialOrderPaymentAssoc { Order = memorialOrder });

            return payment;
        }

        private void AddPaymentCorrections(KumiPayment payment, KumiMemorialOrder memorialOrder)
        {
            var date = memorialOrder.DateDocument;
            if (payment.Sum != memorialOrder.SumZach)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    IdCorrection = 0,
                    Payment = payment,
                    FieldName = "Sum",
                    FieldValue = payment.Sum.ToString(),
                    Date = date
                });
            }
            if (payment.IdKbkType != memorialOrder.IdKbkType)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    FieldName = "IdKbkType",
                    FieldValue = payment.IdKbkType?.ToString(),
                    Date = date
                });
            }
            if (payment.Kbk != memorialOrder.Kbk)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    FieldName = "Kbk",
                    FieldValue = payment.Kbk,
                    Date = date
                });
            }
            if (payment.TargetCode != memorialOrder.TargetCode)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    FieldName = "TargetCode",
                    FieldValue = payment.TargetCode,
                    Date = date
                });
            }
            if (payment.Okato != memorialOrder.Okato)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    FieldName = "Okato",
                    FieldValue = payment.Okato,
                    Date = date
                });
            }
            if (payment.RecipientInn != memorialOrder.InnAdb)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    FieldName = "RecipientInn",
                    FieldValue = payment.RecipientInn,
                    Date = date
                });
            }
            if (payment.RecipientKpp != memorialOrder.KppAdb)
            {
                payment.PaymentCorrections.Add(new KumiPaymentCorrection
                {
                    FieldName = "RecipientKpp",
                    FieldValue = payment.RecipientKpp,
                    Date = date
                });
            }
        }

        private KumiPayment ApplyPaymentExtract(KumiPayment payment, KumiPaymentExtract extract)
        {
            if (kumiPaymentDocCodes == null)
                kumiPaymentDocCodes = registryContext.KumiPaymentDocCodes.ToList();
            if (extract.Guid != payment.Guid)
                throw new KumiPaymentCheckVtOperException(string.Format("Попытка привязать к платежу {0} строку выписки {1}", payment.Guid, extract.Guid));

            if (extract.CodeDocAdp == "ZV" && extract.SumOut == payment.Sum)
            {
                payment.Sum = -payment.Sum;
                payment.IdSource = 8;
            }
            else if (extract.SumIn != payment.Sum)
                throw new KumiPaymentCheckVtOperException(string.Format("Несоответствие суммы в расчетном документе {0} ({1}) и выписке {2} ({3})",
                    payment.Guid, payment.Sum, extract.Guid, extract.SumIn));
            if (extract.NumDoc != payment.NumDocument)
                throw new KumiPaymentCheckVtOperException(string.Format("Несоответствие номера платежного документа в расчетном документе {0} ({1}) и выписке {2} ({3})",
                    payment.Guid, payment.NumDocument, extract.Guid, extract.NumDoc));
            if (extract.DateDoc != payment.DateDocument)
                throw new KumiPaymentCheckVtOperException(string.Format("Несоответствие даты платежного документа в расчетном документе {0} ({1}) и выписке {2} ({3})",
                    payment.Guid, payment.DateDocument?.ToString("dd.MM.yyyy"), extract.Guid, extract.DateDoc.ToString("dd.MM.yyyy")));
            var docCode = kumiPaymentDocCodes.FirstOrDefault(r => r.Code == extract.CodeDoc);
            if (docCode == null)
                throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный код документа, подтверждающего проведение операции {0} в выписке по платежу {1}", 
                    extract.CodeDoc, extract.Guid));
            payment.IdPaymentDocCode = docCode.IdPaymentDocCode;
            return payment;
        }

        // Buffer
        private List<KumiPaymentKind> kumiPaymentKinds { get; set; }
        private List<KumiOperationType> kumiOperationTypes { get; set; }
        private List<KumiKbkType> kumiKbkTypes { get; set; }
        private List<KumiPaymentReason> kumiPaymentReasons { get; set; }
        private List<KumiPayerStatus> kumiPayerStatuses { get; set; }

        private KumiPayment BindDictionariesToPayment(KumiPayment payment)
        {
            if (kumiPaymentKinds == null)
                kumiPaymentKinds = registryContext.KumiPaymentKinds.ToList();
            if (payment.PaymentKind != null)
            {
                var paymentKind = kumiPaymentKinds.FirstOrDefault(r => r.Code == payment.PaymentKind.Code);
                if (paymentKind == null && payment.PaymentKind.Code != "")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный вид платежа {0} в платеже {1}", payment.PaymentKind.Code, payment.Guid));
                payment.PaymentKind = null;
                payment.IdPaymentKind = paymentKind?.IdPaymentKind;
            }

            if (kumiOperationTypes == null)
                kumiOperationTypes = registryContext.KumiOperationTypes.ToList();
            if (payment.OperationType != null)
            {
                var operationType = kumiOperationTypes.FirstOrDefault(r => r.Code == payment.OperationType.Code);
                if (operationType == null && payment.OperationType.Code != "0")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный вид операции {0} в платеже {1}", payment.OperationType.Code, payment.Guid));
                payment.OperationType = null;
                payment.IdOperationType = operationType?.IdOperationType;
            }

            if (kumiKbkTypes == null)
                kumiKbkTypes = registryContext.KumiKbkTypes.ToList();
            if (payment.KbkType != null)
            {
                var kbkType = kumiKbkTypes.FirstOrDefault(r => r.Code == payment.KbkType.Code);
                if (kbkType == null && payment.KbkType.Code != "0")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный тип КБК {0} в платеже {1}", payment.KbkType.Code, payment.Guid));
                payment.KbkType = null;
                payment.IdKbkType = kbkType?.IdKbkType;
            }

            if (kumiPaymentReasons == null)
                kumiPaymentReasons = registryContext.KumiPaymentReasons.ToList();
            if (payment.PaymentReason != null)
            {
                var paymentReason = kumiPaymentReasons.FirstOrDefault(r => r.Code == payment.PaymentReason.Code);
                if (paymentReason == null && payment.PaymentReason.Code != "0" && payment.PaymentReason.Code != "00")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный показатель основания платежа {0} в платеже {1}", payment.PaymentReason.Code, payment.Guid));
                payment.PaymentReason = null;
                payment.IdPaymentReason = paymentReason?.IdPaymentReason;
            }

            if (kumiPayerStatuses == null)
                kumiPayerStatuses = registryContext.KumiPayerStatuses.ToList();
            if (payment.PayerStatus != null)
            {
                var payerStatus = kumiPayerStatuses.FirstOrDefault(r => r.Code == payment.PayerStatus.Code);
                if (payerStatus == null && payment.PayerStatus.Code != "0" && payment.PayerStatus.Code != "00")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный статус составителя расчетного документа {0} в платеже {1}", payment.PayerStatus.Code, payment.Guid));
                payment.PayerStatus = null;
                payment.IdPayerStatus = payerStatus?.IdPayerStatus;
            }
            return payment;
        }

        private KumiMemorialOrder BindDictionariesToMemorialOrder(KumiMemorialOrder order)
        {
            if (kumiKbkTypes == null)
                kumiKbkTypes = registryContext.KumiKbkTypes.ToList();
            if (order.KbkType != null)
            {
                var kbkType = kumiKbkTypes.FirstOrDefault(r => r.Code == order.KbkType.Code);
                if (kbkType == null && order.KbkType.Code != "0")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный тип КБК {0} в мемориальном ордере {1}", order.KbkType.Code, order.Guid));
                order.KbkType = null;
                order.IdKbkType = kbkType?.IdKbkType;
            }
            
            return order;
        }

        public KumiPayment GetKumiPayment(int idPayment)
        {
            var payment = registryContext.KumiPayments
                .Include(r => r.PaymentCharges)
                .Include(r => r.PaymentClaims)
                .Include(r => r.ChildPayments)
                .Include(r => r.PaymentGroup)
                .Include(r => r.PaymentUfs)
                .Include(r => r.PaymentCorrections)
                .Include(r => r.MemorialOrderPaymentAssocs).AsNoTracking()
                .SingleOrDefault(a => a.IdPayment == idPayment);
            foreach(var assoc in payment.MemorialOrderPaymentAssocs)
            {
                assoc.Order = registryContext.KumiMemorialOrders.FirstOrDefault(r => r.IdOrder == assoc.IdOrder);
                assoc.IdOrder = assoc.Order?.IdOrder ?? 0;
            }
            foreach (var assoc in payment.PaymentCharges)
            {
                assoc.Charge = registryContext.KumiCharges.Include(r => r.Account).FirstOrDefault(r => r.IdCharge == assoc.IdCharge);
                assoc.IdCharge = assoc.Charge?.IdCharge ?? 0;
            }
            foreach (var assoc in payment.PaymentClaims)
            {
                assoc.Claim = registryContext.Claims.Include(r => r.IdAccountKumiNavigation).FirstOrDefault(r => r.IdClaim == assoc.IdClaim);
                assoc.IdClaim = assoc.Claim?.IdClaim ?? 0;
            }
            return payment;
        }

        public List<KumiMemorialOrder> GetKumiPaymentMemorialOrders(int idPayment)
        {
            var orders = (from order in registryContext.KumiMemorialOrders
                         join assoc in registryContext.KumiMemorialOrderPaymentAssocs
                         on order.IdOrder equals assoc.IdOrder
                         where assoc.IdPayment == idPayment
                         select order).ToList();
            return orders;
        }

        public void Create(KumiPayment payment)
        {
            registryContext.KumiPayments.Add(payment);
            registryContext.SaveChanges();
        }

        public void Edit(KumiPayment payment)
        {
            registryContext.KumiPayments.Update(payment);
            registryContext.SaveChanges();
        }

        public void Delete(int idPayment)
        {
            var payments = registryContext.KumiPayments
                .Include(r => r.MemorialOrderPaymentAssocs)
                .FirstOrDefault(pc => pc.IdPayment == idPayment);
            payments.Deleted = 1;
            foreach(var mo in payments.MemorialOrderPaymentAssocs)
            {
                registryContext.KumiMemorialOrderPaymentAssocs.Remove(mo);
            }
            registryContext.SaveChanges();
        }

        public bool AccountExists(string account, int idAccount)
        {
            var curAccount = registryContext.KumiAccounts
                .SingleOrDefault(a => a.IdAccount == idAccount)
                ?.Account;
            if (curAccount == account)
                return false;
            return registryContext.KumiAccounts
                .Select(a => a.Account).Count(num => num != null && num == account) > 0;
        }

        public List<KumiPaymentGroup> PaymentGroups { get => registryContext.KumiPaymentGroups.ToList(); }
        public List<KumiPaymentInfoSource> PaymentInfoSources { get => registryContext.KumiPaymentInfoSources.ToList(); }
        public List<KumiPaymentDocCode> PaymentDocCodes { get => registryContext.KumiPaymentDocCodes.ToList(); }
        public List<KumiPaymentKind> PaymentKinds { get => registryContext.KumiPaymentKinds.ToList(); }
        public List<KumiOperationType> OperationTypes { get => registryContext.KumiOperationTypes.ToList(); }
        public List<KumiKbkType> KbkTypes { get => registryContext.KumiKbkTypes.ToList(); }
        public List<KumiKbkDescription> KbkDescriptions { get => registryContext.KumiKbkDescriptions.ToList(); }
        public List<KumiPaymentReason> PaymentReasons { get => registryContext.KumiPaymentReasons.ToList(); }
        public List<KumiPayerStatus> PayerStatuses { get => registryContext.KumiPayerStatuses.ToList(); }
        public List<SelectableSigner> PaymentUfSigners { get => registryContext.SelectableSigners.Where(r => r.IdSignerGroup == 5).ToList(); }
        public List<KladrRegion> Regions { get => registryContext.KladrRegions.ToList(); }
        public List<KladrStreet> Streets { get => registryContext.KladrStreets.ToList(); }
        public List<KumiAccountState> AccountStates { get => registryContext.KumiAccountStates.ToList(); }
        public List<ClaimStateType> ClaimStateTypes { get => registryContext.ClaimStateTypes.ToList(); }

        public KumiPaymentsVM GetKumiPaymentViewModelForMassDistribution(List<int> ids)
        {
            var viewModel = InitializeViewModel(null,null, null);
            viewModel.Payments = GetPaymentsForMassDistribution(ids).ToList();
            viewModel.DistributionInfoToObjects = GetDistributionInfoToObjects(viewModel.Payments.Select(r => r.IdPayment).ToList());
            return viewModel;
        }

        public IQueryable<KumiPayment> GetPaymentsForMassDistribution(List<int> ids)
        {
            return registryContext.KumiPayments
                .Where(b => ids.Contains(b.IdPayment));
        }

        public IQueryable<KumiPaymentGroup> GetPaymentLogs()
        {
            
            return registryContext.KumiPaymentGroups
                .Include(r => r.PaymentGroupFiles);
        }
        public KumiPaymentGroupsVM GetPaymentGroupsVM(PageOptions pageOptions)
        {
            var vm = new KumiPaymentGroupsVM();
            vm.PageOptions = pageOptions ?? vm.PageOptions;
            var query = registryContext.KumiPaymentGroups
                .Include(r => r.PaymentGroupFiles).OrderByDescending(c=> c.Date).ThenByDescending(c=> c.IdGroup);
            vm.PageOptions.TotalRows = query.Count();
            var count = query.Count();
            vm.PageOptions.Rows = count;
            vm.PageOptions.TotalPages = Math.Max((int)Math.Ceiling(count / (double)vm.PageOptions.SizePage), 1);
            vm.PageOptions.CurrentPage = Math.Min(vm.PageOptions.CurrentPage, vm.PageOptions.TotalPages);

            vm.paymentGroups = GetKumiPaymentGroupPage(query, vm.PageOptions).ToList();

            return vm ;
        }

        private IQueryable<KumiPaymentGroup> GetKumiPaymentGroupPage(IQueryable<KumiPaymentGroup> query, PageOptions pageOptions)
        {
            var page = query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
            return page;
        }


        public KumiPaymentsUploadStateModel UploadLogPaymentGroups(int idGroup)
        {
            var log = registryContext.KumiPaymentGroupLog.FirstOrDefault(c => c.IdGroup == idGroup).Log;

            var loadState = JsonSerializer.Deserialize<KumiPaymentsUploadStateModel>(log, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
            });
            return loadState;
        }

        public Dictionary<Tuple<int, int>, KumiPayment> GetPaymentsByOrders(List<KumiMemorialOrder> orders)
        {
            var idOrders = orders.Select(r => r.IdOrder).ToList();
            var orderNums = orders.Select(r => r.NumDocument).ToList();
            var paymentRows = from assocRow in registryContext.KumiMemorialOrderPaymentAssocs
                              join paymentRow in registryContext.KumiPayments
                              on assocRow.IdPayment equals paymentRow.IdPayment
                              where idOrders.Contains(assocRow.IdOrder)
                              select new
                              {
                                  assocRow.IdOrder,
                                  paymentRow
                              };

            var paymentRowsDict = new Dictionary<Tuple<int, int>, KumiPayment>();
            foreach (var paymentRow in paymentRows)
            {
                paymentRowsDict.Add(new Tuple<int, int>(paymentRow.IdOrder, paymentRow.paymentRow.IdPayment), paymentRow.paymentRow);
            }

            return paymentRowsDict;
        }
    }
}
