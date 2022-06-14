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

        public KumiPaymentsUploadStateModel UploadInfoFromTff(List<TffString> tffStrings, List<KumiPaymentGroupFile> kumiPaymentGroupFiles)
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

            var payments = knownPayments.Union(unknownPayments);

            var group = new KumiPaymentGroup {
                Date = DateTime.Now,
                PaymentGroupFiles = kumiPaymentGroupFiles,
                User = securityService.User.UserName
            };
            registryContext.KumiPaymentGroups.Add(group);

            UploadPayments(payments, group, extracts, loadState);
            UploadMemorialOrders(memorialOrders, group, loadState);

            registryContext.SaveChanges();

            return loadState;
        }

        public override KumiPaymentsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, KumiPaymentsFilter filterOptions)
        {
            var vm = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            vm.PaymentSourcesList = new SelectList(registryContext.KumiPaymentInfoSources.ToList(), "IdSource", "Name");
            return vm;
        }

        public KumiPaymentsVM GetViewModel(OrderOptions orderOptions, PageOptions pageOptions, KumiPaymentsFilter filterOptions)
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
            query = GetQueryPage(query, viewModel.PageOptions);
            viewModel.Payments = query.ToList();

            if (filterOptions?.IdAccount != null)
            {
                viewModel.RefAccount = registryContext.KumiAccounts.Include(r => r.Claims).Include(r => r.Charges).FirstOrDefault(r => r.IdAccount == filterOptions.IdAccount);
            }
            if (filterOptions?.IdClaim != null)
            {
                viewModel.RefClaim = registryContext.Claims.Include(r => r.IdAccountKumiNavigation)
                    .FirstOrDefault(r => r.IdClaim == filterOptions.IdClaim);
            }
            if (filterOptions?.IdCharge != null)
            {
                var charge = registryContext.KumiCharges.FirstOrDefault(r => r.IdCharge == filterOptions.IdCharge);
                if (charge != null)
                {
                    viewModel.RefAccount = registryContext.KumiAccounts.Include(r => r.Claims).Include(r => r.Charges).FirstOrDefault(r => r.IdAccount == charge.IdAccount);
                    viewModel.StartDate = charge.StartDate;
                    viewModel.EndDate = charge.EndDate;
                }
            }

            return viewModel;
        }

        private IQueryable<KumiPayment> GetQueryOrder(IQueryable<KumiPayment> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField))
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdPayment);
                else
                    return query.OrderByDescending(p => p.IdPayment);
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
                           .Where(r => r.IdClaim == filterOptions.IdClaim).Select(r => r.IdPayment);

                query = query.Where(r => ids.Contains(r.IdPayment));
            }
            if (filterOptions.IdCharge != null)
            {
                var ids = registryContext.KumiPaymentCharges.Where(r => r.IdDisplayCharge == filterOptions.IdCharge).Select(r => r.IdPayment)
                    .Union(registryContext.KumiPaymentClaims.Where(r => r.IdDisplayCharge == filterOptions.IdCharge).Select(r => r.IdPayment));
                query = query.Where(r => ids.Contains(r.IdPayment));
            }
            if (filterOptions.IdAccount != null)
            {
                var ids = registryContext.KumiPaymentCharges.Include(r => r.Charge).Where(r => r.Charge.IdAccount == filterOptions.IdAccount).Select(r => r.IdPayment)
                    .Union(
                        registryContext.KumiPaymentClaims.Include(r => r.Claim)
                            .Where(r => r.Claim.IdAccountKumi != null && r.Claim.IdAccountKumi == filterOptions.IdAccount).Select(r => r.IdPayment)
                    ).Distinct().ToList();

                query = query.Where(r => ids.Contains(r.IdPayment));
            }
            return query;
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

            if (payment.Guid == null)
            {
                payment.Guid = mo.Guid;
            }

            if (payment.Guid != mo.Guid)
                throw new ApplicationException(
                    string.Format("Глобальный идентификатор платежа {0} не соответсвуют глобальному идентификатору платежного документа в мемориальном ордере {1}", 
                    payment.Guid, mo.Guid));

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
            if (!string.IsNullOrEmpty(filterOptions.Uin))
            {
                query = query.Where(r => r.Uin != null && r.Uin.Contains(filterOptions.Uin));
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

            foreach(var paymentClaim in payment.PaymentClaims.GroupBy(r => r.IdClaim))
            {
                if (!idClaims.Contains(paymentClaim.Key)) continue;
                var claim = registryContext.Claims.FirstOrDefault(r => r.IdClaim == paymentClaim.Key);
                if (claim == null)
                    throw new ApplicationException(
                        string.Format("Произошла ошибка во время отмены распределения платежа. Не найдена исковая работа с идентификатором {0}", claim.IdClaim));

                claim.AmountTenancyRecovered = (claim.AmountTenancyRecovered ?? 0) - paymentClaim.Select(r => r.TenancyValue).Sum();
                claim.AmountPenaltiesRecovered = (claim.AmountPenaltiesRecovered ?? 0) - paymentClaim.Select(r => r.PenaltyValue).Sum();
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

            registryContext.SaveChanges();
            registryContext.DetachAllEntities();

            // Recalculate
            var startRewriteDate = DateTime.Now.Date;
            startRewriteDate = startRewriteDate.AddDays(-startRewriteDate.Day + 1);

            if (DateTime.Now.Date.Day <= 3) // Предыдущий период блокируется для перезаписи по истечении трех дней текущего периода
            {
                startRewriteDate = startRewriteDate.AddMonths(-1);
            }

            var endCalcDate = DateTime.Now.Date;
            endCalcDate = endCalcDate.AddDays(-endCalcDate.Day + 1).AddMonths(1).AddDays(-1);
            kumiAccountsDataService.RecalculateAccounts(accounts, startRewriteDate, endCalcDate);  

            return new KumiPaymentDistributionInfo
            {
                IdPayment = idPayment,
                Sum = payment.Sum,
                DistrubutedToTenancySum = 0,
                DistrubutedToPenaltySum = 0
            };
        }

        public KumiPaymentDistributionInfo DistributePaymentToAccount(int idPayment, int idObject, KumiPaymentDistributeToEnum distributeTo, decimal tenancySum, decimal penaltySum)
        {
            if (tenancySum < 0)
                throw new ApplicationException("Указана отрицательная сумма, распределяемая на найм");

            if (penaltySum < 0)
                throw new ApplicationException("Указана отрицательная сумма, распределяемая на пени");

            if(tenancySum + penaltySum == 0)
                throw new ApplicationException("Не указана распределяемая сумма");

            var payment = registryContext.KumiPayments.Include(r => r.PaymentClaims).Include(r => r.PaymentCharges).FirstOrDefault(r => r.IdPayment == idPayment);
            if (payment == null)
                throw new ApplicationException("Не найдена платеж в базе данных");
            var distributedTenancySum = payment.PaymentCharges.Select(r => r.TenancyValue).Sum() +
                payment.PaymentClaims.Select(r => r.TenancyValue).Sum();
            var distributedPenaltySum = payment.PaymentCharges.Select(r => r.PenaltyValue).Sum() +
                payment.PaymentClaims.Select(r => r.PenaltyValue).Sum();
            if (payment.Sum < distributedTenancySum + distributedPenaltySum + tenancySum + penaltySum)
                throw new ApplicationException(string.Format("Распределяемая сумма {0} превышает остаток по платежу {1}", tenancySum + penaltySum,
                    payment.Sum - distributedTenancySum - distributedPenaltySum));

            var isPl = payment.IdSource == 3 || payment.IdSource == 5;
            var date = payment.DateExecute ?? payment.DateIn ?? payment.DateExecute;
            if (date == null)
                throw new ApplicationException("В платеже не указана " + (isPl ? "дата исполнения распоряжения" : "дата списания со счета"));

            if (payment.Sum == distributedTenancySum + distributedPenaltySum + tenancySum + penaltySum)
            {
                payment.IsPosted = 1;
                registryContext.KumiPayments.Update(payment);
            }

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

                    var startPeriodDate = date.Value.AddDays(-date.Value.Day + 1);
                    var endPeriodDate = date.Value.AddDays(-date.Value.Day + 1).AddMonths(1).AddDays(-1);
                    var charge = registryContext.KumiCharges.AsNoTracking().FirstOrDefault(r => r.IdAccount == idObject && r.StartDate == startPeriodDate && r.EndDate == endPeriodDate);

                    var paymentCharge = new KumiPaymentCharge
                    {
                        IdPayment = idPayment,
                        TenancyValue = tenancySum,
                        PenaltyValue = penaltySum,
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

                    claim.AmountTenancyRecovered = (claim.AmountTenancyRecovered ?? 0) + tenancySum;
                    claim.AmountPenaltiesRecovered = (claim.AmountPenaltiesRecovered ?? 0) + penaltySum;
                    registryContext.Claims.Update(claim);

                    registryContext.KumiPaymentClaims.Add(new KumiPaymentClaim
                    {
                        IdPayment = idPayment,
                        IdClaim = idObject,
                        TenancyValue = tenancySum,
                        PenaltyValue = penaltySum,
                        Date = DateTime.Now.Date
                    });
                    break;
            }

            registryContext.SaveChanges();
            registryContext.DetachAllEntities();

            // Recalculate
            var startRewriteDate = DateTime.Now.Date;
            startRewriteDate = startRewriteDate.AddDays(-startRewriteDate.Day + 1);
            if (DateTime.Now.Date.Day <= 3) // Предыдущий период блокируется для перезаписи по истечении трех дней текущего периода
            {
                startRewriteDate = startRewriteDate.AddMonths(-1);
            }
            var endCalcDate = DateTime.Now.Date;
            endCalcDate = endCalcDate.AddDays(-endCalcDate.Day + 1).AddMonths(1).AddDays(-1);
            kumiAccountsDataService.RecalculateAccounts(accounts, startRewriteDate, endCalcDate);

            return new KumiPaymentDistributionInfo
            {
                IdPayment = idPayment,
                Sum = payment.Sum,
                DistrubutedToTenancySum = distributedTenancySum + tenancySum,
                DistrubutedToPenaltySum = distributedPenaltySum + penaltySum
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
                    loadState.MemorialOrdersDicitionaryBindErrors.Add(new Tuple<KumiMemorialOrder, string>(mo, e.Message));
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

                    loadState.BindedMemorialOrders.Add(new Tuple<KumiMemorialOrder, KumiPayment>(mo, dbPayment));

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
                    loadState.BindMemorialOrdersErrors.Add(new Tuple<KumiMemorialOrder, string>(mo, e.Message));
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
                            loadState.KnownPayments.Add(paymentLocal);
                            break;
                        default:
                            throw new KumiPaymentBindDictionaryException(string.Format("Неподдерживаемый источник платежа {0} - {1}", payment.Guid, payment.IdSource));
                    }
                }
                catch (KumiPaymentBindDictionaryException e)
                {
                    loadState.PaymentsDicitionaryBindErrors.Add(new Tuple<KumiPayment, string>(payment, e.Message));
                    continue;
                }
                catch (KumiPaymentCheckVtOperException e)
                {
                    loadState.CheckExtractErrors.Add(new Tuple<KumiPayment, string>(payment, e.Message));
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
            var date = DateTime.Now;
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

            if (extract.SumIn != payment.Sum)
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
                if (paymentKind == null)
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный вид платежа {0} в платеже {1}", payment.OperationType.Code, payment.Guid));
                payment.PaymentKind = null;
                payment.IdPaymentKind = paymentKind.IdPaymentKind;
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
                if (paymentReason == null && payment.PaymentReason.Code != "0")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный показатель основания платежа {0} в платеже {1}", payment.PaymentReason.Code, payment.Guid));
                payment.PaymentReason = null;
                payment.IdPaymentReason = paymentReason?.IdPaymentReason;
            }

            if (kumiPayerStatuses == null)
                kumiPayerStatuses = registryContext.KumiPayerStatuses.ToList();
            if (payment.PayerStatus != null)
            {
                var payerStatus = kumiPayerStatuses.FirstOrDefault(r => r.Code == payment.PayerStatus.Code);
                if (payerStatus == null && payment.PayerStatus.Code != "0")
                    throw new KumiPaymentBindDictionaryException(string.Format("Указан некорректный показатель основания платежа {0} в платеже {1}", payment.PayerStatus.Code, payment.Guid));
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
    }
}
