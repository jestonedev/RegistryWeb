﻿using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
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
        }

        public int IdProcess { get; set; }
        public int? IdExecutor { get; set; }
        public int? IdRentType { get; set; }
        public int? IdRentTypeCategory { get; set; }
        public int? IdWarrant { get; set; }
        public string RegistrationNum { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public byte UntilDismissal { get; set; }
        public string ResidenceWarrantNum { get; set; }
        public DateTime? ResidenceWarrantDate { get; set; }
        public string ProtocolNum { get; set; }
        public DateTime? ProtocolDate { get; set; }
        public string Description { get; set; }
        public DateTime? SubTenancyDate { get; set; }
        public string SubTenancyNum { get; set; }
        public byte Deleted { get; set; }

        public virtual TenancyActiveProcess TenancyActiveContractNavigation { get; set; }
        public virtual RentTypeCategory IdRentTypeCategoryNavigation { get; set; }
        public virtual RentType IdRentTypeNavigation { get; set; }
        public virtual IList<TenancyBuildingAssoc> TenancyBuildingsAssoc { get; set; }
        public virtual IList<TenancyPerson> TenancyPersons { get; set; }
        public virtual IList<TenancyPremiseAssoc> TenancyPremisesAssoc { get; set; }
        public virtual IList<TenancyReason> TenancyReasons { get; set; }
        public virtual IList<TenancySubPremiseAssoc> TenancySubPremisesAssoc { get; set; }
    }
}
