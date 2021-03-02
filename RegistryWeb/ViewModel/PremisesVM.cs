using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.Models;
using RegistryWeb.Models.SqlViews;
using RegistryWeb.DataServices;

namespace RegistryWeb.ViewModel
{
    public class PremisesVM<T> : ListVM<PremisesListFilter>
    {
        public PremisesDataService premisesService;

        public PremisesVM(PremisesDataService premisesService)
        {
            this.premisesService = premisesService;
        }

        public PremisesVM()
        {
        }
        
        public List<Premise> Premises { get; set; }
        public Premise Premise { get; set; }
        public List<PremiseWithFunType> PremisesWFT { get; set; }
        public PremiseWithFunType PremiseWFT { get; set; }
        public List<Premise> PremiseWithFundType { get; set; }
        public int? IdFundType { get; set; }
        public SelectList KladrRegionsList { get; set; }
        public SelectList KladrStreetsList { get; set; }
        public SelectList RentList { get; set; }
        public SelectList HeatingTypesList { get; set; }
        public SelectList StructureTypesList { get; set; }
        public SelectList ObjectStatesList { get; set; }
        public SelectList LocationKeysList { get; set; }
        public SelectList CommentList { get; set; }
        public SelectList FundTypesList { get; set; }
        public SelectList OwnershipRightTypesList { get; set; }
        public SelectList RestrictionsList { get; set; }
        public SelectList HousesList { get; set; }
        public SelectList PremisesTypesList { get; set; }
        public SelectList CommisionList { get; set; }
        public SelectList SignersList { get; set; }
        public SelectList PreparersList { get; set; }
        public List<PaymentsInfo> PaymentsInfo { get; set; }
        public List<PremiseOwnershipRightCurrent> PremisesOwnershipRightCurrent { get; set; }
    }
}
