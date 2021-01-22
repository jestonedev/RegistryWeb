using RegistryWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using RegistryWeb.Models.Entities;

namespace RegistryWeb.DataServices
{
    public class AddressesDataService
    {
        private readonly RegistryContext registryContext;

        public AddressesDataService(RegistryContext registryContext) 
        {
            this.registryContext = registryContext;
        }

        public List<Address> GetAddressesByText(string text, bool isBuildings = false)
        {
            if (string.IsNullOrEmpty(text)) return new List<Address>();
            var match = Regex.Match(text, @"^(.*?)[,]*[ ]*(д\.?)?[ ]*(\d+[а-яА-Я]?([\\\/]\d+[а-яА-Я]?)?)?[ ,-]*(кв\.?|ком\.?|пом\.?|кв\.?ком.?)?[ ]*(\d+[а-яА-Я]?)?[ ]*$");
            var addressWordsList = new List<string>();
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
                if (addressWords.Length == 2)
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
                else if (!isBuildings)
                {
                    var premiseNum = addressWords[2].ToLowerInvariant();
                    if (addressWords.Length == 3)
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
                    else if (addressWords.Length == 4)
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

        public IQueryable<KladrStreet> GetStreetsByText(string text)
        {
            return registryContext.KladrStreets
                .AsNoTracking()
                .Where(s => s.StreetLong.Contains(text) || s.StreetName.Contains(text));
        }

        public IQueryable<SubPremise> GetSubPremisesByPremisee(int idPremise)
        {
            return registryContext.SubPremises
                .AsNoTracking()
                .Where(sp => sp.IdPremises == idPremise);
        }

        public IQueryable<Premise> GetPremisesByBuildingAndType(int idBuilding, int idPremisesType)
        {
            return 
                registryContext.Premises
                .AsNoTracking()
                .Where(p => p.IdBuilding == idBuilding && p.IdPremisesType == idPremisesType);
        }

        public IQueryable<PremisesType> GetPremiseTypesByBuilding(int idBuilding)
        {
            return registryContext.Premises
                .Include(p => p.IdPremisesTypeNavigation)
                .AsNoTracking()
                .Where(p => p.IdBuilding == idBuilding)
                .Select(p => p.IdPremisesTypeNavigation)
                .Distinct();
        }

        public IQueryable<Building> GetBuildingsByStreet(string idStreet)
        {
            return registryContext.Buildings
                .AsNoTracking()
                .Where(b => b.IdStreet == idStreet);
        }

        public IQueryable<PremisesType> PremisesTypes { get {
                return registryContext.PremisesTypes;
            }
        }
    }
}
