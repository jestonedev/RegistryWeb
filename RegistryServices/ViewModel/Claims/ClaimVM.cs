using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Claims;
using RegistryDb.Models.Entities.Common;
using RegistryDb.Models.Entities.Payments;
using RegistryServices.ViewModel.KumiAccounts;
using RegistryWeb.ViewModel;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.Claims
{
    public class ClaimVM
    {
        public Claim Claim { get; set; }
        public IList<Address> RentObjectsBks { get; set; }
        public IList<KumiAccountTenancyInfoVM> TenancyInfoKumi { get; set; }
        public Payment LastPaymentInfo { get; set; }
        public IEnumerable<ClaimStateType> StateTypes { get; set; }
        public IEnumerable<Executor> Executors { get; set; }
        public IEnumerable<Judge> Judges { get; set; }
        public IEnumerable<SelectableSigner> Signers { get; set; }
        public Executor CurrentExecutor { get; set; }
        public IEnumerable<ClaimStateTypeRelation> StateTypeRelations { get; set; }
    }
}