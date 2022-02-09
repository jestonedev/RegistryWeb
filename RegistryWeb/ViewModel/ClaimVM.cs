using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryWeb.ViewModel
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