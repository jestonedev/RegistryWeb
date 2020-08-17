using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.DataHelpers;
using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace RegistryWeb.DataServices
{
    public class PaymentAccountsDataService : ListDataService<PaymentsVM, PaymentsFilter>
    {
        private readonly SecurityServices.SecurityService securityService;
        private readonly IConfiguration config;

        public PaymentAccountsDataService(RegistryContext registryContext, SecurityServices.SecurityService securityService, IConfiguration config) : base(registryContext)
        {
            this.securityService = securityService;
            this.config = config;
        }

        public override PaymentsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, PaymentsFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.Streets = registryContext.KladrStreets;
            return viewModel;
        }

        internal PaymentsVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            PaymentsFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var payments = GetQuery();
            viewModel.PageOptions.TotalRows = payments.Count();
            var query = GetQueryFilter(payments, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            query = GetQueryIncludes(query);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            query = GetQueryPage(query, viewModel.PageOptions);
            viewModel.Payments = query.ToList();
            viewModel.RentObjects = GetRentObjects(viewModel.Payments);
            viewModel.ClaimsByAddresses = GetClaimsByAddresses(viewModel.Payments);
            return viewModel;
        }

        private IQueryable<Payment> GetQuery()
        {
            var maxDatePayments = from row in registryContext.Payments
                                  group row.Date by row.IdAccount into gs
                                  select new
                                  {
                                      IdAccount = gs.Key,
                                      Date = gs.Max()
                                  };

            return (from row in registryContext.Payments
                    join maxDatePaymentsRow in maxDatePayments
                    on new { row.IdAccount, row.Date } equals new { maxDatePaymentsRow.IdAccount, maxDatePaymentsRow.Date }
                    select row).Include(p => p.PaymentAccountNavigation)
                .ThenInclude(p => p.PaymentAccountPremisesAssoc)
                .Include(p => p.PaymentAccountNavigation)
                .ThenInclude(p => p.PaymentAccountSubPremisesAssoc);
        }

        private IQueryable<Payment> GetQueryIncludes(IQueryable<Payment> query)
        {
            return query
                .Include(p => p.PaymentAccountNavigation)
                .ThenInclude(p => p.PaymentAccountPremisesAssoc)
                .Include(p => p.PaymentAccountNavigation)
                .ThenInclude(p => p.PaymentAccountSubPremisesAssoc);
        }

        private IQueryable<Payment> GetQueryFilter(IQueryable<Payment> query, PaymentsFilter filterOptions)
        {
            return query;
        }

        private IQueryable<Payment> GetQueryOrder(IQueryable<Payment> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField))
            {
                return  query.OrderByDescending(p => p.Date);
            }
            if (orderOptions.OrderField == "Date")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.Date);
                else
                    return query.OrderByDescending(p => p.Date);
            }
            if (orderOptions.OrderField == "Account")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.PaymentAccountNavigation.Account);
                else
                    return query.OrderByDescending(p => p.PaymentAccountNavigation.Account);
            }
            if (orderOptions.OrderField == "Address")
            {
                var addresses = 
                    registryContext.PaymentAccountPremisesAssoc
                    .Include(r => r.PremiseNavigation)
                    .ThenInclude(r => r.IdBuildingNavigation)
                    .ThenInclude(r => r.IdStreetNavigation)
                    .Select(
                    p => new
                    {
                        p.IdAccount,
                        Address = string.Concat(p.PremiseNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName, ", ",
                            p.PremiseNavigation.IdBuildingNavigation.House, ", ", p.PremiseNavigation.PremisesNum)
                    })
                .Union(registryContext.PaymentAccountSubPremisesAssoc
                    .Include(r => r.SubPremiseNavigation)
                    .ThenInclude(r => r.IdPremisesNavigation)
                    .ThenInclude(r => r.IdBuildingNavigation)
                    .ThenInclude(r => r.IdStreetNavigation)
                    .Select(
                    sp => new
                    {
                        sp.IdAccount,
                        Address = string.Concat(sp.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreetNavigation.StreetName, ", ",
                            sp.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.House, ", ",
                            sp.SubPremiseNavigation.IdPremisesNavigation.PremisesNum, ", ", sp.SubPremiseNavigation.SubPremisesNum)
                    }));
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                {
                    return (from row in query
                            join addr in addresses
                             on row.IdAccount equals addr.IdAccount into aq
                            from aqRow in aq.DefaultIfEmpty()
                            orderby aqRow.Address
                            select row).Distinct();
                }
                else
                {
                    return (from row in query
                            join addr in addresses
                             on row.IdAccount equals addr.IdAccount into aq
                            from aqRow in aq.DefaultIfEmpty()
                            orderby aqRow.Address descending
                            select row).Distinct();
                }
            }
            return query;
        }

        private IQueryable<Payment> GetQueryPage(IQueryable<Payment> query, PageOptions pageOptions)
        {
            return query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
        }

        private Dictionary<int, List<Address>> GetRentObjects(IEnumerable<Payment> payments)
        {
            var ids = payments.Select(r => r.IdAccount);
            var premises = from paRow in registryContext.PaymentAccountPremisesAssoc
                           join premiseRow in registryContext.Premises
                           on paRow.IdPremise equals premiseRow.IdPremises
                           join buildingRow in registryContext.Buildings
                           on premiseRow.IdBuilding equals buildingRow.IdBuilding
                           join streetRow in registryContext.KladrStreets
                           on buildingRow.IdStreet equals streetRow.IdStreet
                           join premiseTypesRow in registryContext.PremisesTypes
                           on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                           where ids.Contains(paRow.IdAccount)
                           select new
                           {
                               paRow.IdAccount,
                               Address = new Address
                               {
                                   AddressType = AddressTypes.Premise,
                                   Id = premiseRow.IdPremises.ToString(),
                                   IdParents = new Dictionary<string, string>
                                       {
                                           { AddressTypes.Street.ToString(), buildingRow.IdStreet },
                                           { AddressTypes.Building.ToString(), buildingRow.IdBuilding.ToString() }
                                       },
                                   Text = string.Concat(streetRow.StreetName, ", д.", buildingRow.House, ", ",
                                        premiseTypesRow.PremisesTypeShort, premiseRow.PremisesNum)
                               }
                           };
            var subPremises = from paRow in registryContext.PaymentAccountSubPremisesAssoc
                              join subPremiseRow in registryContext.SubPremises
                              on paRow.IdSubPremise equals subPremiseRow.IdSubPremises
                              join premiseRow in registryContext.Premises
                              on subPremiseRow.IdPremises equals premiseRow.IdPremises
                              join buildingRow in registryContext.Buildings
                              on premiseRow.IdBuilding equals buildingRow.IdBuilding
                              join streetRow in registryContext.KladrStreets
                              on buildingRow.IdStreet equals streetRow.IdStreet
                              join premiseTypesRow in registryContext.PremisesTypes
                              on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                              where ids.Contains(paRow.IdAccount)
                              select new
                              {
                                  paRow.IdAccount,
                                  Address = new Address
                                  {
                                      AddressType = AddressTypes.SubPremise,
                                      Id = subPremiseRow.IdSubPremises.ToString(),
                                      IdParents = new Dictionary<string, string>
                                           {
                                              { AddressTypes.Street.ToString(), buildingRow.IdStreet },
                                              { AddressTypes.Building.ToString(), buildingRow.IdBuilding.ToString() },
                                              { AddressTypes.Premise.ToString(), premiseRow.IdPremises.ToString() }
                                           },
                                      Text = string.Concat(streetRow.StreetName, ", д.", buildingRow.House, ", ",
                                            premiseTypesRow.PremisesTypeShort, premiseRow.PremisesNum, ", к.", subPremiseRow.SubPremisesNum)
                                  }
                              };

            var objects = premises.Union(subPremises).ToList();

            var result =
                objects.GroupBy(r => r.IdAccount)
                .Select(r => new { IdAccount = r.Key, Addresses = r.Select(v => v.Address) })
                .ToDictionary(v => v.IdAccount, v => v.Addresses.ToList());
            return result;
        }

        private Dictionary<int, List<ClaimInfo>> GetClaimsByAddresses(IEnumerable<Payment> payments)
        {
            var ids = payments.Select(r => r.IdAccount);
            var filteredPremises = (
                           from row in registryContext.PaymentAccountPremisesAssoc
                           join id in ids
                           on row.IdAccount equals id
                           select new PaymentAddressInfix
                           {
                               IdAccount = row.IdAccount,
                               Infix = "p"+row.IdPremise.ToString()
                           }).ToList();

            var filteredSubPremises = (from row in registryContext.PaymentAccountSubPremisesAssoc
                               join id in ids
                               on row.IdAccount equals id
                               select new PaymentAddressInfix
                               {
                                   IdAccount = row.IdAccount,
                                   Infix = "sp" + row.IdSubPremise.ToString()
                               }).ToList();
            var filteredObjects = from row in filteredPremises.Union(filteredSubPremises)
                          orderby row.Infix
                          group row.Infix by row.IdAccount into gs
                          select new
                          {
                              IdAccount = gs.Key,
                              AddressCode = gs.Aggregate((acc, v) => acc + v)
                          };
            filteredObjects = from paymentsRow in payments
                              join filteredObjectsRow in filteredObjects
                              on paymentsRow.IdAccount equals filteredObjectsRow.IdAccount into fo
                              from foRow in fo.DefaultIfEmpty()
                              select new
                              {
                                  paymentsRow.IdAccount,
                                  AddressCode = foRow != null ? foRow.AddressCode : paymentsRow.PaymentAccountNavigation.RawAddress
                              };

            var allPremises = (from row in registryContext.PaymentAccountPremisesAssoc
                                   select new PaymentAddressInfix
                                   {
                                       IdAccount = row.IdAccount,
                                       Infix = "p" + row.IdPremise.ToString()
                                   }).ToList();

            var allSubPremises = (from row in registryContext.PaymentAccountSubPremisesAssoc
                                      select new PaymentAddressInfix
                                      {
                                          IdAccount = row.IdAccount,
                                          Infix = "sp" + row.IdSubPremise.ToString()
                                      }).ToList();
            var allObjects = from row in allPremises.Union(allSubPremises)
                             orderby row.Infix
                             group row.Infix by row.IdAccount into gs
                             select new
                             {
                                 IdAccount = gs.Key,
                                 AddressCode = gs.Aggregate((acc, v) => acc + v)
                             };
            allObjects = from paymentsRow in registryContext.PaymentAccounts
                         join allObjectsRow in allObjects
                         on paymentsRow.IdAccount equals allObjectsRow.IdAccount into ao
                         from aoRow in ao.DefaultIfEmpty()
                         select new
                         {
                             paymentsRow.IdAccount,
                             AddressCode = aoRow != null ? aoRow.AddressCode : paymentsRow.RawAddress
                         };
            var accountsAssoc = (from filteredRow in filteredObjects
                                join allRow in allObjects
                                on filteredRow.AddressCode equals allRow.AddressCode
                                select new
                                {
                                    IdAccountFiltered = filteredRow.IdAccount,
                                    IdAccountActual = allRow.IdAccount
                                }).ToList();
            var claimLastStatesIds = from row in registryContext.ClaimStates
                                  group row.IdState by row.IdClaim into gs
                                  select new
                                  {
                                      IdClaim = gs.Key,
                                      IdState = gs.Max()
                                  };
            var claims = from claimRow in registryContext.Claims
                         join claimLastStateRow in claimLastStatesIds
                         on claimRow.IdClaim equals claimLastStateRow.IdClaim into cls
                         from clsRow in cls.DefaultIfEmpty()
                         join claimStateRow in registryContext.ClaimStates
                         on clsRow.IdState equals claimStateRow.IdState into cs
                         from csRow in cs.DefaultIfEmpty()
                         join claimStateTypeRow in registryContext.ClaimStateTypes
                         on csRow.IdStateType equals claimStateTypeRow.IdStateType into cst
                         from cstRow in cst.DefaultIfEmpty()
                         select new ClaimInfo
                         {
                             IdClaim = claimRow.IdClaim,
                             StartDeptPeriod = claimRow.StartDeptPeriod,
                             EndDeptPeriod = claimRow.EndDeptPeriod,
                             IdAccount = claimRow.IdAccount,
                             IdClaimCurrentState = csRow != null ? (int?)csRow.IdStateType : null,
                             ClaimCurrentState = cstRow != null ? cstRow.StateType : null
                         };

            var accountsIds = accountsAssoc.Select(r => r.IdAccountActual);
            claims = claims.Where(c => accountsIds.Contains(c.IdAccount));

            var result =
                    claims
                    .Select(c => new ClaimInfo {
                        ClaimCurrentState = c.ClaimCurrentState,
                        IdClaimCurrentState =c.IdClaimCurrentState,
                        IdClaim = c.IdClaim,
                        StartDeptPeriod = c.StartDeptPeriod,
                        EndDeptPeriod = c.EndDeptPeriod,
                        IdAccount = accountsAssoc.First(a => a.IdAccountActual == c.IdAccount).IdAccountFiltered
                    })
                    .GroupBy(r => r.IdAccount)
                    .Select(r => new { IdAccount = r.Key, Claims = r.OrderByDescending(v => v.IdClaim).Select(v => v) })
                    .ToDictionary(v => v.IdAccount, v => v.Claims.ToList());
            return result;
        }
    }
}