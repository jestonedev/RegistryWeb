using RegistryDb.Models;
using RegistryWeb.ViewOptions.Filter;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewOptions;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryServices.DataServices.KumiPayments;

namespace RegistryServices.DataFilterServices
{
    class KumiPaymentsFilterService : AbstractFilterService<KumiPayment, KumiPaymentsFilter>
    {
        private readonly RegistryContext registryContext;
        private readonly KumiUntiedPaymentsService untiedPaymentsService;

        public KumiPaymentsFilterService(RegistryContext registryContext, KumiUntiedPaymentsService untiedPaymentsService)
        {
            this.registryContext = registryContext;
            this.untiedPaymentsService = untiedPaymentsService;
        }

        public override IQueryable<KumiPayment> GetQueryFilter(IQueryable<KumiPayment> query, KumiPaymentsFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                if (filterOptions.IdParentPayment != null)
                {
                    return query.Where(r => r.IdPayment == filterOptions.IdParentPayment || r.IdParentPayment == filterOptions.IdParentPayment);
                }
                if (!string.IsNullOrWhiteSpace(filterOptions.CommonFilter))
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

        private IQueryable<KumiPayment> PayerFilter(IQueryable<KumiPayment> query, KumiPaymentsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.PayerAccount))
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

        private IQueryable<KumiPayment> RefFilter(IQueryable<KumiPayment> query, KumiPaymentsFilter filterOptions)
        {
            if (filterOptions.IdClaim != null)
            {
                var ids = registryContext.KumiPaymentClaims
                           .Where(r => r.IdClaim == filterOptions.IdClaim).Select(r => r.IdPayment).ToList();

                var untiedIds = untiedPaymentsService.GetUntiedPayments(null, filterOptions.IdClaim, null).Select(r => r.IdPayment);

                ids = ids.Union(untiedIds).ToList();

                query = query.Where(r => ids.Contains(r.IdPayment));
            }
            if (filterOptions.IdCharge != null)
            {
                var ids = registryContext.KumiPaymentCharges.Where(r => r.IdDisplayCharge == filterOptions.IdCharge).Select(r => r.IdPayment)
                    .Union(registryContext.KumiPaymentClaims.Where(r => r.IdDisplayCharge == filterOptions.IdCharge).Select(r => r.IdPayment)).ToList();

                var untiedIds = untiedPaymentsService.GetUntiedPayments(null, null, filterOptions.IdCharge).Select(r => r.IdPayment);

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

                var untiedIds = untiedPaymentsService.GetUntiedPayments(filterOptions.IdAccount, null, null).Select(r => r.IdPayment);

                ids = ids.Union(untiedIds).ToList();

                query = query.Where(r => ids.Contains(r.IdPayment));
            }
            return query;
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

        public override IQueryable<KumiPayment> GetQueryIncludes(IQueryable<KumiPayment> query)
        {
            return query
                .Include(r => r.PaymentCharges)
                .Include(r => r.PaymentClaims)
                .Include(r => r.ChildPayments)
                .Include(r => r.PaymentGroup);
        }

        public override IQueryable<KumiPayment> GetQueryOrder(IQueryable<KumiPayment> query, OrderOptions orderOptions)
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
    }
}
