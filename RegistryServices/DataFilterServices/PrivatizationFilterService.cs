using RegistryDb.Models;
using System.Linq;
using RegistryWeb.ViewOptions;
using RegistryDb.Models.Entities.Privatization;
using RegistryWeb.ViewOptions.Filter;
using RegistryWeb.DataServices;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.Enums;
using System.Collections.Generic;

namespace RegistryServices.DataFilterServices
{
    class PrivatizationFilterService : AbstractFilterService<PrivContract, PrivatizationFilter>
    {
        private readonly RegistryContext registryContext;
        private readonly AddressesDataService addressesDataService;

        public PrivatizationFilterService(RegistryContext registryContext, AddressesDataService addressesDataService)
        {
            this.registryContext = registryContext;
            this.addressesDataService = addressesDataService;
        }

        public override IQueryable<PrivContract> GetQueryFilter(IQueryable<PrivContract> query, PrivatizationFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = ModalFilter(query, filterOptions);
                query = FrontSideRegNumberFilter(query, filterOptions);
                query = OldAddressFilter(query, filterOptions);
            }
            return query;
        }

        private IQueryable<PrivContract> FrontSideRegNumberFilter(IQueryable<PrivContract> query, PrivatizationFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.FrontSideRegNumber))
            {
                var regNum = filterOptions.FrontSideRegNumber.Trim();
                query = from row in query
                        where row.RegNumber.Contains(regNum)
                        select row;
            }
            return query;
        }

        private IQueryable<PrivContract> OldAddressFilter(IQueryable<PrivContract> query, PrivatizationFilter filterOptions)
        {
            if (string.IsNullOrEmpty(filterOptions.OldSystemAddress)) return query;
            var addressParts = filterOptions.OldSystemAddress.ToLowerInvariant().Split(' ', 3);
            var streetPart = addressParts[0];
            string housePart = null;
            string premisePart = null;
            if (addressParts.Count() > 1)
            {
                housePart = "д. " + addressParts[1];
            }
            if (addressParts.Count() > 2)
            {
                premisePart = "кв. " + addressParts[2];
            }
            query = query.Where(r => r.PrivAddress != null && r.PrivAddress.Contains(streetPart)
                && (housePart == null || r.PrivAddress.Contains(housePart))
                && (premisePart == null || r.PrivAddress.Contains(premisePart)));
            return query;
        }

        private IQueryable<PrivContract> AddressFilter(IQueryable<PrivContract> query, PrivatizationFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return query;

            var addresses = addressesDataService.GetAddressesByText(filterOptions.Address.Text).Select(r => r.Id);

            if (filterOptions.Address.AddressType == AddressTypes.Street)
            {
                return
                    from q in query
                    where addresses.Contains(q.IdStreet)
                    select q;
            }

            var addressesInt = addresses.Where(a => int.TryParse(a, out int aInt)).Select(a => int.Parse(a)).ToList();
            if (!addressesInt.Any())
                return query;

            if (filterOptions.Address.AddressType == AddressTypes.Building)
            {
                return
                    from q in query
                    where q.IdBuilding != null && addressesInt.Contains(q.IdBuilding.Value)
                    select q;
            }
            if (filterOptions.Address.AddressType == AddressTypes.Premise)
            {
                return
                    from q in query
                    where q.IdPremise != null && addressesInt.Contains(q.IdPremise.Value)
                    select q;
            }
            if (filterOptions.Address.AddressType == AddressTypes.SubPremise)
            {
                return
                    from q in query
                    where q.IdSubPremise != null && addressesInt.Contains(q.IdSubPremise.Value)
                    select q;
            }
            return query;
        }

        private IQueryable<PrivContract> ModalFilter(IQueryable<PrivContract> query, PrivatizationFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.RegNumber))
            {
                var regNum = filterOptions.RegNumber.Trim();
                query = from row in query
                        where row.RegNumber.Contains(regNum)
                        select row;
            }
            if (filterOptions.DateIssueCivil != null)
            {
                query = from row in query
                        where row.DateIssueCivil == filterOptions.DateIssueCivil
                        select row;
            }
            if (filterOptions.IsRefusenik)
            {
                query = from row in query
                        where row.IsRefusenik == true
                        select row;
            }

            IEnumerable<int> idsProcess = null;
            if (!string.IsNullOrEmpty(filterOptions.IdRegion))
            {
                var ids = query.Where(r => r.BuildingNavigation != null && r.BuildingNavigation.IdStreet.Contains(filterOptions.IdRegion)).Select(r => r.IdContract)
                    .ToList();
                idsProcess = ids;
            }
            if (!string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                var ids = query.Where(r => r.BuildingNavigation != null && r.BuildingNavigation.IdStreet == filterOptions.IdStreet).Select(r => r.IdContract)
                    .ToList();
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                var ids = query.Where(r => r.BuildingNavigation != null && r.BuildingNavigation.House == filterOptions.House).Select(r => r.IdContract).ToList();
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                var ids = query.Where(r => r.PremiseNavigation != null && r.PremiseNavigation.PremisesNum == filterOptions.PremisesNum).Select(r => r.IdContract).ToList();
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (!string.IsNullOrEmpty(filterOptions.Surname))
            {
                var ids = registryContext.PrivContractors.Where(r => r.Surname != null && r.Surname.Contains(filterOptions.Surname)).Select(r => r.IdContract).ToList();
                idsProcess = ids;
            }
            if (!string.IsNullOrEmpty(filterOptions.Name))
            {
                var ids = registryContext.PrivContractors.Where(r => r.Name != null && r.Name.Contains(filterOptions.Name)).Select(r => r.IdContract).ToList();
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (!string.IsNullOrEmpty(filterOptions.Patronymic))
            {
                var ids = registryContext.PrivContractors.Where(r => r.Patronymic != null && r.Patronymic.Contains(filterOptions.Patronymic)).Select(r => r.IdContract).ToList();
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (filterOptions.BirthDate != null)
            {
                var ids = registryContext.PrivContractors.Where(r => r.DateBirth == filterOptions.BirthDate).Select(r => r.IdContract).ToList();
                if (idsProcess == null)
                {
                    idsProcess = ids;
                }
                else
                {
                    idsProcess = idsProcess.Intersect(ids);
                }
            }
            if (idsProcess != null)
            {
                query = (from row in query
                         join id in idsProcess
                         on row.IdContract equals id
                         select row).Distinct();
            }
            return query;
        }

        public override IQueryable<PrivContract> GetQueryIncludes(IQueryable<PrivContract> query)
        {
            return query
                .Include(r => r.PrivAdditionalEstates)
                .Include(r => r.ExecutorNavigation)
                .Include(r => r.TypeOfProperty)
                .Include(r => r.BuildingNavigation)
                .Include(r => r.PremiseNavigation)
                .Include(r => r.SubPremiseNavigation);
        }

        public override IQueryable<PrivContract> GetQueryOrder(IQueryable<PrivContract> query, OrderOptions orderOptions)
        {
            return query.OrderByDescending(p => p.IdContract);
        }
    }
}
