using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.Tenancies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RegistryWeb.DataHelpers
{
    public static class AddressHelper
    {
        public static string GetAddress(TenancyBuildingAssoc tba)
        {
            if (tba == null)
                throw new NullReferenceException("tba=null");
            if (tba.BuildingNavigation == null)
                throw new NullReferenceException("IdBuildingNavigation не подгружен");
            if (tba.BuildingNavigation.IdStreetNavigation == null)
                throw new NullReferenceException("IdStreetNavigation не подгружен");
            var address =
                tba.BuildingNavigation.IdStreetNavigation.StreetName + ", д." +
                tba.BuildingNavigation.House;
            return address;
        }

        public static string GetAddress(TenancyPremiseAssoc tpa)
        {
            if (tpa == null)
                throw new NullReferenceException("tpa=null");
            if (tpa.PremiseNavigation == null)
                throw new NullReferenceException("PremiseNavigation не подгружен");
            if (tpa.PremiseNavigation.IdPremisesTypeNavigation == null)
                throw new NullReferenceException("IdPremisesTypeNavigation не подгружен");
            if (tpa.PremiseNavigation.IdBuildingNavigation == null)
                throw new NullReferenceException("IdBuildingNavigation не подгружен");
            if (tpa.PremiseNavigation.IdBuildingNavigation.IdStreetNavigation == null)
                throw new NullReferenceException("IdStreetNavigation не подгружен");
            var address =
                tpa.PremiseNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName + ", д." +
                tpa.PremiseNavigation.IdBuildingNavigation.House + ", " +
                tpa.PremiseNavigation.IdPremisesTypeNavigation.PremisesTypeShort +
                tpa.PremiseNavigation.PremisesNum;
            return address;
        }

        public static string GetAddress(TenancySubPremiseAssoc tspa)
        {
            if (tspa == null)
                throw new NullReferenceException("tspa=null");
            if (tspa.SubPremiseNavigation == null)
                throw new Exception("SubPremiseNavigation не подгружен");
            if (tspa.SubPremiseNavigation.IdPremisesNavigation == null)
                throw new Exception("IdPremisesNavigation не подгружен");
            if (tspa.SubPremiseNavigation.IdPremisesNavigation.IdPremisesTypeNavigation == null)
                throw new Exception("IdPremisesTypeNavigation не подгружен");
            if (tspa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation == null)
                throw new Exception("IdBuildingNavigation не подгружен");
            if (tspa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation == null)
                throw new Exception("IdStreetNavigation не подгружен");
            var address =
                tspa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName +
                ", д." + tspa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.House + ", " +
                tspa.SubPremiseNavigation.IdPremisesNavigation.IdPremisesTypeNavigation.PremisesTypeShort +
                tspa.SubPremiseNavigation.IdPremisesNavigation.PremisesNum + ", к." +
                tspa.SubPremiseNavigation.SubPremisesNum;
            return address;
        }

        public static string JoinAddresses(List<string> addressList)
        {
            if (addressList.Count == 0) return "";
            if (addressList.Count == 1) return addressList.First();

            var address = "";
            var addressPartsList = addressList.Select(r => r.Split(", ", 5)).ToList();
            var groupedByRegion = addressPartsList.GroupBy(r => r[0]);
            foreach (var groupRegion in groupedByRegion)
            {
                if (groupRegion.All(r => r.Count() > 1))
                {
                    var groupedByStreet = groupRegion.GroupBy(r => r[1]);

                    foreach (var groupStreet in groupedByStreet)
                    {
                        if (groupStreet.All(r => r.Count() > 2))
                        {
                            var groupedByHouse = groupStreet.GroupBy(r => r[2]);
                            foreach (var groupHouse in groupedByHouse)
                            {
                                if (groupHouse.All(r => r.Count() > 3))
                                {
                                    var groupedByPremise = groupHouse.GroupBy(r => r[3]);
                                    foreach (var groupPremise in groupedByPremise)
                                    {
                                        if (!string.IsNullOrWhiteSpace(address))
                                        {
                                            address += ", ";
                                        }
                                        address += groupRegion.Key + ", " + groupStreet.Key + ", " + groupHouse.Key + ", " +
                                            groupPremise.Key + groupPremise.Aggregate("", (acc, v) => acc + (v.Length == 5 ? ", " + v[4] : ""));
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrWhiteSpace(address))
                                    {
                                        address += ", ";
                                    }
                                    address += groupRegion.Key + ", " + groupStreet.Key + ", " + groupHouse.Key;
                                }
                            }

                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(address))
                            {
                                address += ", ";
                            }
                            address += groupRegion.Key + ", " + groupStreet.Key;
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(address))
                    {
                        address += ", ";
                    }
                    address += groupRegion.Key;
                }
            }
            return address;
        }
    }
}
