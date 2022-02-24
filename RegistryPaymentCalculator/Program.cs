using RegistryDb.Models;
using RegistryWeb.DataServices;
using System;
using System.Configuration;
using System.Linq;

namespace RegistryPaymentCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            var dbName = ConfigurationManager.AppSettings["dbName"];
            var connectionString = string.Format(ConfigurationManager.ConnectionStrings["registry"].ConnectionString, dbName);

            using (var db = new RegistryContext(connectionString, dbName))
            {
                var service = new KumiAccountsDataService(db, new AddressesDataService(db));
                var accounts = db.KumiAccounts.Where(r => r.IdState == 1 && r.IdAccount == 7);
                var accountsPrepare = service.GetAccountsPrepareForPaymentCalculator(accounts);
                var accountsInfo = service.GetAccountInfoForPaymentCalculator(accountsPrepare);
            }
        }
    }
}
