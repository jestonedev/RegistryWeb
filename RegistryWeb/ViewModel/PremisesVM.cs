﻿using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.Models;
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

        /*public IEnumerable<OwnershipRight> OwnershipRights { get; set; }
        public IEnumerable<PremisesType> PremisesTypes { get; set; }
        public IEnumerable<ObjectState> ObjectStates { get; set; }
        public IEnumerable<FundType> FundTypes { get; set; }*/

        public SelectList KladrStreetsList { get; set; }
        public SelectList HeatingTypesList { get; set; }
        public SelectList StructureTypesList { get; set; }
        public SelectList ObjectStatesList { get; set; }
        public SelectList LocationKeysList { get; set; }
        public SelectList CommentList { get; set; }
        public SelectList FundTypesList { get; set; }
        public SelectList OwnershipRightTypesList { get; set; }
        public SelectList RestrictionsList { get; set; }
        public SelectList HousesList { get; set; }

        public SelectList PremisesTypeAsNum { get; set; }
        public SelectList PremisesTypesList { get; set; }
    }
}