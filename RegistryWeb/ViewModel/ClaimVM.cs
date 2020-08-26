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
        public IList<Address> RentObjects { get; set; }
        public Payment LastPaymentInfo { get; set; }
        public IEnumerable<ClaimStateType> StateTypes { get; set; }
        public Executor CurrentExecutor { get; set; }
    }
}