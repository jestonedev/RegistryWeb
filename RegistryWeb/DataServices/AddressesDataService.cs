using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using RegistryDb.Models.Entities;
using RegistryDb.Models.SqlViews;
using RegistryDb.Models;
using RegistryWeb.Enums;
using RegistryWeb.ViewModel;

namespace RegistryWeb.DataServices
{
    public class AddressesDataService
    {
        private readonly RegistryContext registryContext;

        public AddressesDataService(RegistryContext registryContext) 
        {
            this.registryContext = registryContext;
        }

        public List<Address> GetAddressesByText(string text, AddressTypes addressTypes = AddressTypes.Premise)
        {
            if (string.IsNullOrEmpty(text)) return new List<Address>();
            var match = Regex.Match(text, @"^(.*?)[,]*[ ]*(д\.?)?[ ]*(\d+[а-яА-Я]?([\\\/]\d+[а-яА-Я]?)?)?[ ,-]*(кв\.?|ком\.?|пом\.?|кв\.?[ ]?ком\.?|ком\.?[ ]?кв\.?)?[ ]*(\d+[а-яА-Я]?)?[ ]*((([,]*[ ]+|[,]*[ ]*к\.?[ ]*|[,]*[ ]*ком\.[ ]*|[,][ ]*)?)(\d+[а-яА-Я]?|[ ]+[а-яА-Я]?))?$");
            var addressWordsList = new List<string>();
            if (addressTypes == AddressTypes.None) return new List<Address>();
            if (!string.IsNullOrEmpty(match.Groups[1].Value))
            {
                addressWordsList.Add(match.Groups[1].Value);
            }
            if (!string.IsNullOrEmpty(match.Groups[3].Value))
            {
                addressWordsList.Add(match.Groups[3].Value);
            }
            if (!string.IsNullOrEmpty(match.Groups[6].Value))
            {
                addressWordsList.Add(match.Groups[6].Value);
            }
            if (!string.IsNullOrEmpty(match.Groups[10].Value))
            {
                addressWordsList.Add(match.Groups[10].Value);
            }
            var addressWords = addressWordsList.ToArray();
            var street = addressWords[0].ToLowerInvariant();
            if (addressWords.Length == 1)
            {
                return registryContext.KladrStreets
                    .AsNoTracking()
                    .Where(s => s.StreetLong.ToLowerInvariant().Contains(street) ||
                                s.StreetName.ToLowerInvariant().Contains(street))
                    .Select(s => new Address
                    {
                        Id = s.IdStreet,
                        Text = s.StreetName,
                        AddressType = AddressTypes.Street
                    }).ToList();
            }
            else
            {
                var house = addressWords[1].ToLowerInvariant();
                if (addressWords.Length == 2 && addressTypes != AddressTypes.Street)
                {
                    return registryContext.Buildings
                        .Include(b => b.IdStreetNavigation)
                        .AsNoTracking()
                        .Where(b => (b.IdStreetNavigation.StreetLong.ToLowerInvariant().Contains(street) ||
                                    b.IdStreetNavigation.StreetName.ToLowerInvariant().Contains(street))
                                    && b.House.ToLowerInvariant() == house)
                        .Select(b => new Address
                        {
                            Id = b.IdBuilding.ToString(),
                            Text = string.Concat(b.IdStreetNavigation.StreetName, ", д.", b.House),
                            AddressType = AddressTypes.Building
                        }).ToList();
                }
                else
                {
                    var premiseNum = addressWords[2].ToLowerInvariant();
                    if (addressWords.Length == 3 && addressTypes != AddressTypes.Street && addressTypes != AddressTypes.Building)
                    {
                        return registryContext.Premises
                            .Include(p => p.IdPremisesType)
                            .Include(p => p.IdBuildingNavigation)
                                .ThenInclude(b => b.IdStreetNavigation)
                            .AsNoTracking()
                            .Where(p => (p.IdBuildingNavigation.IdStreetNavigation.StreetLong.ToLowerInvariant().Contains(street) ||
                                        p.IdBuildingNavigation.IdStreetNavigation.StreetName.ToLowerInvariant().Contains(street))
                                        && p.IdBuildingNavigation.House.ToLowerInvariant() == house
                                        && p.PremisesNum.ToLowerInvariant() == premiseNum)
                            .Select(p => new Address
                            {
                                Id = p.IdPremises.ToString(),
                                Text = string.Concat(p.IdBuildingNavigation.IdStreetNavigation.StreetName, ", д.", p.IdBuildingNavigation.House,
                                    ", ", p.IdPremisesTypeNavigation.PremisesTypeShort, p.PremisesNum),
                                AddressType = AddressTypes.Premise
                            }).ToList();
                    }
                    else if (addressWords.Length == 4 && addressTypes == AddressTypes.SubPremise)
                    {
                        var subPremisesNum = addressWords[3].ToLowerInvariant();
                        return registryContext.SubPremises
                            .Include(sp => sp.IdPremisesNavigation)
                                .ThenInclude(p => p.IdPremisesType)
                            .Include(sp => sp.IdPremisesNavigation)
                                .ThenInclude(p => p.IdBuildingNavigation)
                                    .ThenInclude(b => b.IdStreetNavigation)
                            .AsNoTracking()
                            .Where(sp => (sp.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetLong.ToLowerInvariant().Contains(street) ||
                                        sp.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName.ToLowerInvariant().Contains(street))
                                        && sp.IdPremisesNavigation.IdBuildingNavigation.House.ToLowerInvariant() == house
                                        && sp.IdPremisesNavigation.PremisesNum.ToLowerInvariant() == premiseNum
                                        && sp.SubPremisesNum.ToLowerInvariant() == subPremisesNum)
                            .Select(sp => new Address
                            {
                                Id = sp.IdSubPremises.ToString(),
                                Text = string.Concat(sp.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName, ", д.",
                                    sp.IdPremisesNavigation.IdBuildingNavigation.House, ", ",
                                    sp.IdPremisesNavigation.IdPremisesTypeNavigation.PremisesTypeShort,
                                    sp.IdPremisesNavigation.PremisesNum, ", к.", sp.SubPremisesNum),
                                AddressType = AddressTypes.SubPremise
                            }).ToList();
                    }
                }
            }
            return new List<Address>();
        }

        public IEnumerable<KladrStreet> GetStreetsByText(string text)
        {
            return registryContext.KladrStreets
                .AsNoTracking()
                .Where(s => s.StreetLong.Contains(text) || s.StreetName.Contains(text));
        }

        public IEnumerable<SubPremise> GetSubPremisesByPremisee(int idPremise)
        {
            return registryContext.SubPremises
                .AsNoTracking()
                .Where(sp => sp.IdPremises == idPremise);
        }

        public IEnumerable<Premise> GetPremisesByBuildingAndType(int idBuilding, int idPremisesType)
        {
            return 
                registryContext.Premises
                .AsNoTracking()
                .Where(p => p.IdBuilding == idBuilding && p.IdPremisesType == idPremisesType);
        }

        public IEnumerable<PremisesType> GetPremiseTypesByBuilding(int idBuilding)
        {
            return registryContext.Premises
                .Include(p => p.IdPremisesTypeNavigation)
                .AsNoTracking()
                .Where(p => p.IdBuilding == idBuilding)
                .Select(p => p.IdPremisesTypeNavigation)
                .Distinct();
        }
        public IEnumerable<KladrStreet> GetKladrStreets(string idRegion)
        {
            if (idRegion == null)
                return registryContext.KladrStreets.AsNoTracking();
            return registryContext.KladrStreets.Where(s => s.IdStreet.Contains(idRegion)).AsNoTracking();
        }

        //GetBuildingsByStreet
        //GetHouses
        public IEnumerable<Building> GetBuildings(string idStreet)
        {
            return registryContext.Buildings
                .AsNoTracking()
                .Where(b => b.IdStreet == idStreet);
        }

        public IEnumerable<Premise> GetPremises(int? idBuilding)
        {
            return registryContext.Premises
                .AsNoTracking()
                .Where(r => r.IdBuilding == idBuilding);
        }

        public IEnumerable<SubPremise> GetSubPremises(int? idPremise)
        {
            return registryContext.SubPremises
                .AsNoTracking()
                .Where(r => r.IdPremises == idPremise);
        }

        public IEnumerable<KladrRegion> KladrRegions
        {
            get => registryContext.KladrRegions.AsNoTracking();
        }

        public IEnumerable<KladrStreet> KladrStreets
        {
            get => registryContext.KladrStreets.AsNoTracking();
        }
        public IEnumerable<PremisesType> PremisesTypes
        {
            get => registryContext.PremisesTypes.AsNoTracking();
        }

        public List<Tuple<Address, EmergencyInfo>> GetAddressesFromHisParts(PartsAddress parts)
        {
            if (parts.IdStreet == null)
                return null;
            parts.House = parts.House.ToLowerInvariant();
            parts.PremisesNum = parts.PremisesNum?.ToLowerInvariant();
            parts.SubPremisesNum = parts.SubPremisesNum?.ToLowerInvariant();
            if (parts.SubPremisesNum != null)
            {
                return registryContext.SubPremises
                    .Include(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdBuildingNavigation)
                            .ThenInclude(b => b.IdStreetNavigation)
                    .Include(sp => sp.IdPremisesNavigation)
                        .ThenInclude(p => p.IdPremisesTypeNavigation)
                    .AsNoTracking()
                    .Where(sp => sp.SubPremisesNum.ToLowerInvariant() == parts.SubPremisesNum
                        && sp.IdPremisesNavigation.PremisesNum.ToLowerInvariant() == parts.PremisesNum
                        && sp.IdPremisesNavigation.IdBuildingNavigation.House.ToLowerInvariant() == parts.House
                        && sp.IdPremisesNavigation.IdBuildingNavigation.IdStreet == parts.IdStreet).ToList()
                    .Select(sp => new Tuple<Address, EmergencyInfo>(new Address
                    {
                        AddressType = AddressTypes.SubPremise,
                        Id = sp.IdSubPremises.ToString(),
                        IdParents = new Dictionary<string, string>
                        {
                            { "IdBuilding", sp.IdPremisesNavigation.IdBuilding.ToString() },
                            { "IdPremise", sp.IdPremises.ToString() },
                            { "IdSubPremise", sp.IdSubPremises.ToString() },
                        },
                        Text = sp.GetAddress()
                    },
                    new EmergencyInfo
                    {
                        DemolishedDate = GetDemolishedDate(sp.IdPremisesNavigation.IdBuilding, sp.IdPremises),
                        EmergencyDate = GetEmergencyDate(sp.IdPremisesNavigation.IdBuilding, sp.IdPremises),
                        ExcludeEmergencyDate = GetExcludeEmergencyDate(sp.IdPremisesNavigation.IdBuilding, sp.IdPremises),
                    })).ToList();
            }
            if (parts.PremisesNum != null)
            {
                return registryContext.Premises
                    .Include(p => p.IdBuildingNavigation)
                        .ThenInclude(b => b.IdStreetNavigation)
                    .Include(p => p.IdPremisesTypeNavigation)
                    .AsNoTracking()
                    .Where(p => p.PremisesNum.ToLowerInvariant() == parts.PremisesNum
                        && p.IdBuildingNavigation.House.ToLowerInvariant() == parts.House
                        && p.IdBuildingNavigation.IdStreet == parts.IdStreet).ToList()
                    .Select(p => new Tuple<Address, EmergencyInfo>(new Address
                    {
                        AddressType = AddressTypes.Premise,
                        Id = p.IdPremises.ToString(),
                        IdParents = new Dictionary<string, string>
                        {
                            { "IdBuilding", p.IdBuilding.ToString() },
                            { "IdPremise", p.IdPremises.ToString() },
                            { "IdSubPremise", null },
                        },
                        Text = p.GetAddress(),
                    },
                    new EmergencyInfo
                    {
                        DemolishedDate = GetDemolishedDate(p.IdBuilding, p.IdPremises),
                        EmergencyDate = GetEmergencyDate(p.IdBuilding, p.IdPremises),
                        ExcludeEmergencyDate = GetExcludeEmergencyDate(p.IdBuilding, p.IdPremises)
                    })).ToList();
            }
            if (parts.House != null)
            {
                return registryContext.Buildings
                        .Include(b => b.IdStreetNavigation).AsNoTracking()
                       .Where(b => b.House.ToLowerInvariant() == parts.House
                           && b.IdStreet == parts.IdStreet).ToList()
                       .Select(b => new Tuple<Address, EmergencyInfo>(new Address
                       {
                           AddressType = AddressTypes.Building,
                           Id = b.IdBuilding.ToString(),
                           IdParents = new Dictionary<string, string>
                           {
                            { "IdBuilding", b.IdBuilding.ToString() },
                            { "IdPremise", null },
                            { "IdSubPremise", null },
                           },
                           Text = b.GetAddress()
                       },
                        new EmergencyInfo
                        {
                            DemolishedDate = GetDemolishedDate(b.IdBuilding, null),
                            EmergencyDate = GetEmergencyDate(b.IdBuilding, null),
                            ExcludeEmergencyDate = GetExcludeEmergencyDate(b.IdBuilding, null)
                        })).ToList();
            }
            return null;
        }

        private DateTime? GetDemolishedDate(int idBuilding, int? idPremises)
        {
            var oba = registryContext.OwnershipBuildingsAssoc.Where(o => o.IdBuilding == idBuilding).Select(o => o.IdOwnershipRight);
            if (idPremises != null)
            {
                oba = oba.Union(registryContext.OwnershipPremisesAssoc.Where(o => o.IdPremises == idPremises)
                                                .Select(o => o.IdOwnershipRight));
            }
            var or = (from row in oba
                      join oRow in registryContext.OwnershipRights on row equals oRow.IdOwnershipRight
                      where oRow.IdOwnershipRightType == 1
                      select oRow.Date).ToList();
            if (or.Any())
                return or.Max();
            else return null;
        }

        private DateTime? GetEmergencyDate(int idBuilding, int? idPremises)
        {
            var oba = registryContext.OwnershipBuildingsAssoc.Where(o => o.IdBuilding == idBuilding).Select(o => o.IdOwnershipRight);
            if (idPremises != null)
            {
                oba = oba.Union(registryContext.OwnershipPremisesAssoc.Where(o => o.IdPremises == idPremises)
                                                .Select(o => o.IdOwnershipRight));
            }
            var or = (from row in oba
                    join oRow in registryContext.OwnershipRights on row equals oRow.IdOwnershipRight
                    where new int[] { 2, 7 }.Contains(oRow.IdOwnershipRightType)
                    select oRow.Date).ToList();

            if (or.Any())
                return or.Max();
            else return null;
        }

        private DateTime? GetExcludeEmergencyDate(int idBuilding, int? idPremises)
        {
            var oba = registryContext.OwnershipBuildingsAssoc.Where(o => o.IdBuilding == idBuilding).Select(o => o.IdOwnershipRight);
            if (idPremises != null)
            {
                oba = oba.Union(registryContext.OwnershipPremisesAssoc.Where(o => o.IdPremises == idPremises)
                                                .Select(o => o.IdOwnershipRight));
            }
            var or = (from row in oba
                    join oRow in registryContext.OwnershipRights on row equals oRow.IdOwnershipRight
                    where oRow.IdOwnershipRightType == 6
                    select oRow.Date).ToList();
            if (or.Any())
                return or.Max();
            else return null;
        }
    }
}
