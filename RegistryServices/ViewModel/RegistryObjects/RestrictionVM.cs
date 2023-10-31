﻿using RegistryDb.Models.Entities.RegistryObjects.Common.Restrictions;
using RegistryWeb.Enums;

namespace RegistryServices.ViewModel.RegistryObjects
{
    public class RestrictionVM : Restriction
    {
        public AddressTypes AddressType { get; set; }

        public RestrictionVM() { }

        public RestrictionVM(Restriction owr, AddressTypes type)
        {
            IdRestriction = owr.IdRestriction;
            IdRestrictionType = owr.IdRestrictionType;
            Number = owr.Number;
            Date = owr.Date;
            Description = owr.Description;
            DateStateReg = owr.DateStateReg;
            FileOriginName = owr.FileOriginName;
            FileDisplayName = owr.FileDisplayName;
            FileMimeType = owr.FileMimeType;
            Deleted = owr.Deleted;
            RestrictionTypeNavigation = owr.RestrictionTypeNavigation;
            RestrictionBuildingsAssoc = owr.RestrictionBuildingsAssoc;
            RestrictionPremisesAssoc = owr.RestrictionPremisesAssoc;
            AddressType = type;
        }
    }
}
