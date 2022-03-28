using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryWeb.ViewModel
{
    public class TenancyProcessVM
    {
        public TenancyProcess TenancyProcess { get; set; }
        public IList<TenancyRentObject> RentObjects { get; set; }
        public IList<RentType> RentTypes { get; set; }
        public IList<RentTypeCategory> RentTypeCategories { get; set; }
        public IList<Kinship> Kinships { get; set; }
        public IList<TenancyReasonType> TenancyReasonTypes { get; set; }
        public IList<Executor> Executors { get; set; }
        public IList<Employer> Employers { get; set; }
        public IList<KladrStreet> Streets { get; set; }
        public IList<DocumentType> DocumentTypes { get; set; }
        public IList<DocumentIssuedBy> DocumentIssuedBy { get; set; }
        public IList<TenancyProlongRentReason> TenancyProlongRentReasons { get; set; }
        public Executor CurrentExecutor { get; set; }
    }
}