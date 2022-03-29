using System;
using System.Collections.Generic;
using RegistryDb.Models.Entities.Common;
using RegistryDb.Models.Entities.KumiAccounts;
using RegistryDb.Models.SqlViews;

namespace RegistryDb.Models.Entities.Tenancies
{
    public class TenancyProcess
    {
        public TenancyProcess()
        {
            TenancyBuildingsAssoc = new List<TenancyBuildingAssoc>();
            TenancyPersons = new List<TenancyPerson>();
            TenancyPremisesAssoc = new List<TenancyPremiseAssoc>();
            TenancyReasons = new List<TenancyReason>();
            TenancySubPremisesAssoc = new List<TenancySubPremiseAssoc>();
            TenancyAgreements = new List<TenancyAgreement>();
            TenancyRentPeriods = new List<TenancyRentPeriod>();
        }

        public int IdProcess { get; set; }
        public int? IdExecutor { get; set; }
        public int? IdRentType { get; set; }
        public int? IdRentTypeCategory { get; set; }
        public int IdEmployer { get; set; }
        public int? IdWarrant { get; set; }
        public int? IdAccount { get; set; }
        public string RegistrationNum { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? AnnualDate { get; set; }
        public bool UntilDismissal { get; set; }
        public string ResidenceWarrantNum { get; set; }
        public DateTime? ResidenceWarrantDate { get; set; }
        public string ProtocolNum { get; set; }
        public DateTime? ProtocolDate { get; set; }
        public string Description { get; set; }
        public DateTime? SubTenancyDate { get; set; }
        public string SubTenancyNum { get; set; }
        public byte Deleted { get; set; }

        public virtual KumiAccount IdAccountNavigation { get; set; }
        public virtual TenancyActiveProcess TenancyActiveContractNavigation { get; set; }
        public virtual RentTypeCategory IdRentTypeCategoryNavigation { get; set; }
        public virtual RentType IdRentTypeNavigation { get; set; }
        public virtual Executor IdExecutorNavigation { get; set; }
        public virtual Employer IdEmployerNavigation { get; set; }
        public virtual IList<TenancyBuildingAssoc> TenancyBuildingsAssoc { get; set; }
        public virtual IList<TenancyPerson> TenancyPersons { get; set; }
        public virtual IList<TenancyPremiseAssoc> TenancyPremisesAssoc { get; set; }
        public virtual IList<TenancyReason> TenancyReasons { get; set; }
        public virtual IList<TenancyAgreement> TenancyAgreements { get; set; }
        public virtual IList<TenancySubPremiseAssoc> TenancySubPremisesAssoc { get; set; }
        public virtual IList<TenancyRentPeriod> TenancyRentPeriods { get; set; }
        public virtual IList<TenancyFile> TenancyFiles { get; set; }
    }
}
