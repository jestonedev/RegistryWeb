using Microsoft.EntityFrameworkCore;
using RegistryWeb.DataHelpers;
using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.DataServices
{
    public class PaymentAccountReportsDataService
    {
        private readonly RegistryContext registryContext;
        public PaymentAccountReportsDataService(RegistryContext registryContext)
        {
            this.registryContext = registryContext;
        }

        public Payment GetLastPayment(int idAccount)
        {
            var maxDatePayments = from row in registryContext.Payments
                                  group row.Date by row.IdAccount into gs
                                  select new
                                  {
                                      IdAccount = gs.Key,
                                      Date = gs.Max()
                                  };

            return (from row in registryContext.Payments
                    join maxDatePaymentsRow in maxDatePayments
                    on new { row.IdAccount, row.Date } equals new { maxDatePaymentsRow.IdAccount, maxDatePaymentsRow.Date }
                    where row.IdAccount == idAccount
                    select row).Include(p => p.PaymentAccountNavigation).FirstOrDefault();
        }
    }
}
