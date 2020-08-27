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
    public class ClaimsDataService : ListDataService<ClaimsVM, ClaimsFilter>
    {
        private readonly SecurityServices.SecurityService securityService;
        private readonly IConfiguration config;

        public ClaimsDataService(RegistryContext registryContext, SecurityServices.SecurityService securityService, IConfiguration config) : base(registryContext)
        {
            this.securityService = securityService;
            this.config = config;
        }

        public override ClaimsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, ClaimsFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.Streets = registryContext.KladrStreets;
            viewModel.StateTypes = registryContext.ClaimStateTypes;
            return viewModel;
        }

        internal ClaimsVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            ClaimsFilter filterOptions)
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
            viewModel.Claims = query.ToList();
            viewModel.RentObjects = GetRentObjects(viewModel.Claims);
            viewModel.LastPaymentInfo = GetLastPaymentsInfo(viewModel.Claims);
            return viewModel;
        }

        private IQueryable<Claim> GetQuery()
        {
            return registryContext.Claims
                .Include(c => c.ClaimStates)
                .Include(c => c.IdAccountNavigation);
        }

        private IQueryable<Claim> GetQueryIncludes(IQueryable<Claim> query)
        {
            return query
                .Include(c => c.ClaimStates)
                .Include(c => c.IdAccountNavigation);
        }

        private IQueryable<Claim> GetQueryFilter(IQueryable<Claim> query, ClaimsFilter filterOptions)
        {
            if (!filterOptions.IsEmpty())
            {
                query = AddressFilter(query, filterOptions);
                query = PaymentAccountFilter(query, filterOptions);
                query = ClaimFilter(query, filterOptions);
            }
            return query;
        }

        private Claim FillClaimAmount(Claim claim)
        {
            claim.AmountTenancy = claim.AmountTenancy ?? 0;
            claim.AmountPenalties = claim.AmountPenalties ?? 0;
            claim.AmountDgi = claim.AmountDgi ?? 0;
            claim.AmountPadun = claim.AmountPadun ?? 0;
            claim.AmountPkk = claim.AmountPkk ?? 0;
            return claim;
        }

        internal void Create(Claim claim, List<Microsoft.AspNetCore.Http.IFormFile> files)
        {
            // Прикрепляем документы
            var claimFilesPath = Path.Combine(config.GetValue<string>("AttachmentsPath"), @"Claims\");
            if (claim.ClaimFiles != null)
            {
                for (var i = 0; i < claim.ClaimFiles.Count; i++)
                {
                    claim.ClaimFiles[i].FileName = "";
                    var file = files.Where(r => r.Name == "ClaimFile[" + i + "]").FirstOrDefault();
                    if (file == null) continue;
                    claim.ClaimFiles[i].DisplayName = file.FileName;
                    claim.ClaimFiles[i].FileName = Guid.NewGuid().ToString() + "." + new FileInfo(file.FileName).Extension;
                    claim.ClaimFiles[i].MimeType = file.ContentType;
                    var fileStream = new FileStream(Path.Combine(claimFilesPath, claim.ClaimFiles[i].FileName), FileMode.CreateNew);
                    file.OpenReadStream().CopyTo(fileStream);
                    fileStream.Close();
                }
            }

            claim.IdAccountNavigation = null;
            claim = FillClaimAmount(claim);
            registryContext.Claims.Add(claim);
            registryContext.SaveChanges();
        }

        internal void Edit(Claim claim)
        {
            claim.IdAccountNavigation = null;
            claim = FillClaimAmount(claim);
            registryContext.Claims.Update(claim);
            registryContext.SaveChanges();
        }

        internal void Delete(int idClaim)
        {
            var claims = registryContext.Claims
                    .FirstOrDefault(op => op.IdClaim == idClaim);
            if (claims != null)
            {
                claims.Deleted = 1;
                registryContext.SaveChanges();
            }
        }

        public IQueryable<Claim> AddressFilter(IQueryable<Claim> query, ClaimsFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty() &&
                string.IsNullOrEmpty(filterOptions.IdStreet) &&
                string.IsNullOrEmpty(filterOptions.House) &&
                string.IsNullOrEmpty(filterOptions.PremisesNum) &&
                filterOptions.IdBuilding == null &&
                filterOptions.IdPremises == null &&
                filterOptions.IdSubPremises == null)
                return query;

            var premisesAssoc = registryContext.PaymentAccountPremisesAssoc
                .Include(p => p.PremiseNavigation)
                .ThenInclude(b => b.IdBuildingNavigation);

            var subPremisesAssoc = registryContext.PaymentAccountSubPremisesAssoc
                .Include(sp => sp.SubPremiseNavigation)
                .ThenInclude(p => p.IdPremisesNavigation)
                .ThenInclude(b => b.IdBuildingNavigation);

            IEnumerable<int> idAccounts = new List<int>();
            var filtered = false;

            if (filterOptions.Address.AddressType == AddressTypes.Street || !string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                var street = filterOptions.Address.AddressType == AddressTypes.Street ? filterOptions.Address.Id : filterOptions.IdStreet;
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuildingNavigation.IdStreet.Equals(street))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.IdStreet.Equals(street))
                    .Select(ospa => ospa.IdAccount);
                idAccounts = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            var id = 0;
            if ((filterOptions.Address.AddressType == AddressTypes.Building && int.TryParse(filterOptions.Address.Id, out id)) || filterOptions.IdBuilding != null)
            {
                if (filterOptions.IdBuilding != null)
                {
                    id = filterOptions.IdBuilding.Value;
                }
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuilding == id)
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuilding == id)
                    .Select(ospa => ospa.IdAccount);
                idAccounts = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            if ((filterOptions.Address.AddressType == AddressTypes.Premise && int.TryParse(filterOptions.Address.Id, out id)) || filterOptions.IdPremises != null)
            {
                if (filterOptions.IdPremises != null)
                {
                    id = filterOptions.IdPremises.Value;
                }
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdPremises == id)
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdPremises == id)
                    .Select(ospa => ospa.IdAccount);
                idAccounts = idPremiseAccounts.Union(idSubPremiseAccounts);
                filtered = true;
            }
            if ((filterOptions.Address.AddressType == AddressTypes.SubPremise && int.TryParse(filterOptions.Address.Id, out id)) || filterOptions.IdSubPremises != null)
            {
                if (filterOptions.IdSubPremises != null)
                {
                    id = filterOptions.IdSubPremises.Value;
                }
                idAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdSubPremises == id)
                    .Select(ospa => ospa.IdAccount);
                filtered = true;
            }
            if (filtered)
            {
                query = from q in query
                        join idAccount in idAccounts on q.IdAccount equals idAccount
                        select q;
            }
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.IdBuildingNavigation.House.ToLowerInvariant().Equals(filterOptions.House.ToLowerInvariant()))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.IdBuildingNavigation.House.ToLowerInvariant().Equals(filterOptions.House.ToLowerInvariant()))
                    .Select(ospa => ospa.IdAccount);
                query = from q in query
                        join idAccount in idPremiseAccounts.Union(idSubPremiseAccounts) on q.IdAccount equals idAccount
                        select q;
            }
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                var idPremiseAccounts = premisesAssoc
                    .Where(opa => opa.PremiseNavigation.PremisesNum.ToLowerInvariant().Equals(filterOptions.PremisesNum.ToLowerInvariant()))
                    .Select(opa => opa.IdAccount);
                var idSubPremiseAccounts = subPremisesAssoc
                    .Where(ospa => ospa.SubPremiseNavigation.IdPremisesNavigation.PremisesNum.ToLowerInvariant().Equals(filterOptions.PremisesNum.ToLowerInvariant()))
                    .Select(ospa => ospa.IdAccount);
                query = from q in query
                        join idAccount in idPremiseAccounts.Union(idSubPremiseAccounts) on q.IdAccount equals idAccount
                        select q;
            }
            return query;
        }

        internal PaymentAccount GetAccount(int idAccount)
        {
            return registryContext.PaymentAccounts
                .FirstOrDefault(pa => pa.IdAccount == idAccount);
        }

        internal IList<PaymentAccount> GetAccounts(string text)
        {
            return registryContext.PaymentAccounts
                .Where(pa => pa.Account.Contains(text)).Take(100).ToList();
        }

        public IQueryable<Claim> PaymentAccountFilter(IQueryable<Claim> query, ClaimsFilter filterOptions)
        {
            if (!string.IsNullOrEmpty(filterOptions.Crn))
            {
                query = query.Where(p => p.IdAccountNavigation.Crn.Contains(filterOptions.Crn));
            }
            if (!string.IsNullOrEmpty(filterOptions.Account))
            {
                query = query.Where(p => p.IdAccountNavigation.Account.Contains(filterOptions.Account));
            }
            if (!string.IsNullOrEmpty(filterOptions.RawAddress))
            {
                query = query.Where(p => p.IdAccountNavigation.RawAddress.Contains(filterOptions.RawAddress));
            }
            return query;
        }

        private List<int> GetAccountIdsWithSameAddress(int idAccount)
        {
            var filteredObjects = (from row in
                            (from row in registryContext.PaymentAccountPremisesAssoc
                             where row.IdAccount == idAccount
                             select new
                             {
                                 row.IdAccount,
                                 Infix = string.Concat("p", row.IdPremise)
                             }).Union(from row in registryContext.PaymentAccountSubPremisesAssoc
                                      where row.IdAccount == idAccount
                                      select new
                                      {
                                          row.IdAccount,
                                          Infix = string.Concat("sp", row.IdSubPremise)
                                      })
                                   orderby row.Infix
                                   group row.Infix by row.IdAccount into gs
                                   select new
                                   {
                                       IdAccount = gs.Key,
                                       AddressCode = string.Join("", gs)
                                   }).AsEnumerable();

            var allObjects = (from row in (from row in registryContext.PaymentAccountPremisesAssoc
                                           select new PaymentAddressInfix
                                           {
                                               IdAccount = row.IdAccount,
                                               Infix = string.Concat("p", row.IdPremise)
                                           }).Union(from row in registryContext.PaymentAccountSubPremisesAssoc
                                                    select new PaymentAddressInfix
                                                    {
                                                        IdAccount = row.IdAccount,
                                                        Infix = string.Concat("sp", row.IdSubPremise)
                                                    })
                              orderby row.Infix
                              group row.Infix by row.IdAccount into gs
                              select new
                              {
                                  IdAccount = gs.Key,
                                  AddressCode = string.Join("", gs)
                              }).AsEnumerable();
            allObjects = from paymentsRow in registryContext.PaymentAccounts
                         join allObjectsRow in allObjects
                         on paymentsRow.IdAccount equals allObjectsRow.IdAccount into ao
                         from aoRow in ao.DefaultIfEmpty()
                         select new
                         {
                             paymentsRow.IdAccount,
                             AddressCode = aoRow != null ? aoRow.AddressCode : paymentsRow.RawAddress
                         };
            return (from filteredRow in filteredObjects
                    join allRow in allObjects
                    on filteredRow.AddressCode equals allRow.AddressCode
                    select allRow.IdAccount).ToList();
        }

        public IQueryable<Claim> ClaimFilter(IQueryable<Claim> query, ClaimsFilter filterOptions)
        {
            if (filterOptions.IdAccount != null)
            {
                var ids = GetAccountIdsWithSameAddress(filterOptions.IdAccount.Value);
                query = query.Where(p => ids.Contains(p.IdAccount));
            }

            if (filterOptions.AmountTotal != null)
            {
                query = query.Where(p => filterOptions.AmountTotalOp == 1 ?
                    p.AmountTenancy + p.AmountPenalties + p.AmountDgi + p.AmountPadun + p.AmountPkk >= filterOptions.AmountTotal :
                    p.AmountTenancy + p.AmountPenalties + p.AmountDgi + p.AmountPadun + p.AmountPkk <= filterOptions.AmountTotal);
            }
            if (filterOptions.AmountTenancy != null)
            {
                query = query.Where(p => filterOptions.AmountTenancyOp == 1 ?
                    p.AmountTenancy >= filterOptions.AmountTenancy :
                    p.AmountTenancy <= filterOptions.AmountTenancy);
            }
            if (filterOptions.AmountPenalties != null)
            {
                query = query.Where(p => filterOptions.AmountPenaltiesOp == 1 ?
                    p.AmountPenalties >= filterOptions.AmountPenalties :
                    p.AmountPenalties <= filterOptions.AmountPenalties);
            }
            if (filterOptions.AmountDgiPadunPkk != null)
            {
                query = query.Where(p => filterOptions.AmountDgiPadunPkkOp == 1 ?
                    (p.AmountDgi >= filterOptions.AmountDgiPadunPkk ||
                     p.AmountPadun >= filterOptions.AmountDgiPadunPkk ||
                     p.AmountPkk >= filterOptions.AmountDgiPadunPkk) :
                    (p.AmountDgi <= filterOptions.AmountDgiPadunPkk ||
                     p.AmountPadun <= filterOptions.AmountDgiPadunPkk ||
                     p.AmountPkk <= filterOptions.AmountDgiPadunPkk));
            }

            if (filterOptions.AtDate != null)
            {
                query = query.Where(p => p.AtDate == filterOptions.AtDate);
            }

            if (!string.IsNullOrEmpty(filterOptions.CourtOrderNum))
            {
                var idClaims = registryContext.ClaimStates.Where(cs => cs.IdStateType == 4 && cs.CourtOrderNum.Contains(filterOptions.CourtOrderNum)).Select(r => r.IdClaim).ToList();
                query = query.Where(p => idClaims.Contains(p.IdClaim));
            }

            if (filterOptions.IdClaimState != null || filterOptions.ClaimStateDate != null)
            {
                var maxDateClaimStates = from row in registryContext.ClaimStates
                                         group row.IdState by row.IdClaim into gs
                                         select new
                                         {
                                             IdClaim = gs.Key,
                                             IdState = gs.Max()
                                         };

                var lastClaimsStates = (from row in registryContext.ClaimStates
                                        join maxDateClaimStatesRow in maxDateClaimStates
                                        on row.IdState equals maxDateClaimStatesRow.IdState
                                        select new
                                        {
                                            row.IdClaim,
                                            row.IdStateType,
                                            row.DateStartState
                                        }).ToList();
                if (filterOptions.IdClaimState != null)
                {
                    query = from row in query
                            join lastClaimsStatesRow in lastClaimsStates
                            on row.IdClaim equals lastClaimsStatesRow.IdClaim
                            where lastClaimsStatesRow.IdStateType == filterOptions.IdClaimState
                            select row;
                }

                if (filterOptions.ClaimStateDate != null)
                {
                    query = from row in query
                            join lastClaimsStatesRow in lastClaimsStates
                            on row.IdClaim equals lastClaimsStatesRow.IdClaim
                            where filterOptions.ClaimStateDateOp == 1 ?
                               lastClaimsStatesRow.DateStartState >= filterOptions.ClaimStateDate :
                               lastClaimsStatesRow.DateStartState <= filterOptions.ClaimStateDate
                            select row;
                }
            }

            if (filterOptions.BalanceOutputTotal != null || filterOptions.BalanceOutputTenancy != null ||
                filterOptions.BalanceOutputPenalties != null || filterOptions.BalanceOutputDgiPadunPkk != null)
            {
                var maxDatePayments = from row in registryContext.Payments
                                      group row.Date by row.IdAccount into gs
                                      select new
                                      {
                                          IdAccount = gs.Key,
                                          Date = gs.Max()
                                      };

                var lastPayments = (from row in registryContext.Payments
                                    join maxDatePaymentsRow in maxDatePayments
                                    on new { row.IdAccount, row.Date } equals new { maxDatePaymentsRow.IdAccount, maxDatePaymentsRow.Date }
                                    select new {
                                        row.IdAccount,
                                        row.BalanceOutputTotal,
                                        row.BalanceOutputTenancy,
                                        row.BalanceOutputPenalties,
                                        row.BalanceOutputDgi,
                                        row.BalanceOutputPkk,
                                        row.BalanceOutputPadun,
                                    }).ToList();

                if (filterOptions.BalanceOutputTotal != null)
                {
                    query = from row in query
                            join lastPaymentsRow in lastPayments
                            on row.IdAccount equals lastPaymentsRow.IdAccount
                            where filterOptions.BalanceOutputTotalOp == 1 ?
                               lastPaymentsRow.BalanceOutputTotal >= filterOptions.BalanceOutputTotal :
                               lastPaymentsRow.BalanceOutputTotal <= filterOptions.BalanceOutputTotal
                            select row;
                }
                if (filterOptions.BalanceOutputTenancy != null)
                {
                    query = from row in query
                            join lastPaymentsRow in lastPayments
                            on row.IdAccount equals lastPaymentsRow.IdAccount
                            where filterOptions.BalanceOutputTenancyOp == 1 ?
                               lastPaymentsRow.BalanceOutputTenancy >= filterOptions.BalanceOutputTenancy :
                               lastPaymentsRow.BalanceOutputTenancy <= filterOptions.BalanceOutputTenancy
                            select row;
                }
                if (filterOptions.BalanceOutputPenalties != null)
                {
                    query = from row in query
                            join lastPaymentsRow in lastPayments
                            on row.IdAccount equals lastPaymentsRow.IdAccount
                            where filterOptions.BalanceOutputPenaltiesOp == 1 ?
                               lastPaymentsRow.BalanceOutputPenalties >= filterOptions.BalanceOutputPenalties :
                               lastPaymentsRow.BalanceOutputPenalties <= filterOptions.BalanceOutputPenalties
                            select row;
                }
                if (filterOptions.BalanceOutputDgiPadunPkk != null)
                {
                    query = from row in query
                            join lastPaymentsRow in lastPayments
                            on row.IdAccount equals lastPaymentsRow.IdAccount
                            where filterOptions.BalanceOutputDgiPadunPkkOp == 1 ?
                                (lastPaymentsRow.BalanceOutputDgi >= filterOptions.BalanceOutputDgiPadunPkk ||
                                 lastPaymentsRow.BalanceOutputPadun >= filterOptions.BalanceOutputDgiPadunPkk ||
                                 lastPaymentsRow.BalanceOutputPkk >= filterOptions.BalanceOutputDgiPadunPkk) :
                                (lastPaymentsRow.BalanceOutputDgi <= filterOptions.BalanceOutputDgiPadunPkk ||
                                 lastPaymentsRow.BalanceOutputPadun <= filterOptions.BalanceOutputDgiPadunPkk ||
                                 lastPaymentsRow.BalanceOutputPkk <= filterOptions.BalanceOutputDgiPadunPkk)
                            select row;
                }
            }
            return query;
        }

        private IQueryable<Claim> GetQueryOrder(IQueryable<Claim> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField) || orderOptions.OrderField == "IdClaim")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdClaim);
                else
                    return query.OrderByDescending(p => p.IdClaim);
            }
            if (orderOptions.OrderField == "AtDate")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.AtDate);
                else
                    return query.OrderByDescending(p => p.AtDate);
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

        private IQueryable<Claim> GetQueryPage(IQueryable<Claim> query, PageOptions pageOptions)
        {
            return query
                .Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage)
                .Take(pageOptions.SizePage);
        }


        internal Dictionary<int, List<Address>> GetRentObjects(IEnumerable<int> idAccounts)
        {
            var premises = from paRow in registryContext.PaymentAccountPremisesAssoc
                           join premiseRow in registryContext.Premises
                           on paRow.IdPremise equals premiseRow.IdPremises
                           join buildingRow in registryContext.Buildings
                           on premiseRow.IdBuilding equals buildingRow.IdBuilding
                           join streetRow in registryContext.KladrStreets
                           on buildingRow.IdStreet equals streetRow.IdStreet
                           join premiseTypesRow in registryContext.PremisesTypes
                           on premiseRow.IdPremisesType equals premiseTypesRow.IdPremisesType
                           where idAccounts.Contains(paRow.IdAccount)
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
                              where idAccounts.Contains(paRow.IdAccount)
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

        internal Dictionary<int, List<Address>> GetRentObjects(IEnumerable<Claim> claims)
        {
            var ids = claims.Select(r => r.IdAccount).Distinct();
            return GetRentObjects(ids);
        }

        private Dictionary<int, Payment> GetLastPaymentsInfo(IEnumerable<Claim> claims)
        {
            var ids = claims.Select(r => r.IdAccount).Distinct();

            var maxDatePayments = from row in registryContext.Payments
                                  where ids.Contains(row.IdAccount)
                                  group row.Date by row.IdAccount into gs
                                  select new
                                  {
                                      IdAccount = gs.Key,
                                      Date = gs.Max()
                                  };

            var lastPayments = from row in registryContext.Payments
                               join maxDatePaymentsRow in maxDatePayments
                               on new { row.IdAccount, row.Date } equals new { maxDatePaymentsRow.IdAccount, maxDatePaymentsRow.Date }
                               select row;

            var result =
                lastPayments.GroupBy(r => r.IdAccount)
                .Select(r => new { IdAccount = r.Key, Payment = r.FirstOrDefault() })
                .Where(r => r.Payment != null)
                .ToDictionary(v => v.IdAccount, v => v.Payment);
            return result;
        }

        internal ClaimVM CreateClaimEmptyViewModel([CallerMemberName]string action = "")
        {
            return new ClaimVM
            {
                Claim = new Claim(),
                CurrentExecutor = CurrentExecutor,
                StateTypes = registryContext.ClaimStateTypes.ToList(),
                StateTypeRelations = registryContext.ClaimStateTypeRelations.ToList()
            };
        }

        internal ClaimVM GetClaimViewModel(Claim claim, [CallerMemberName]string action = "")
        {
            var claimVM = CreateClaimEmptyViewModel(action);
            claimVM.Claim = claim;
            claimVM.RentObjects = GetRentObjects(new List<Claim> { claim }).SelectMany(r => r.Value).ToList();
            claimVM.LastPaymentInfo = GetLastPaymentsInfo(new List<Claim> { claim }).Select(r => r.Value).FirstOrDefault();
            return claimVM;
        }

        public Claim GetClaim(int idClaim)
        {
            return registryContext.Claims.Include(r => r.IdAccountNavigation)
                .Include(r => r.ClaimStates)
                .Include(c => c.ClaimFiles)
                .FirstOrDefault(r => r.IdClaim == idClaim);
        }

        public List<ClaimStateType> StateTypes => registryContext.ClaimStateTypes.ToList();
        public List<ClaimStateTypeRelation> StateTypeRelations => registryContext.ClaimStateTypeRelations.ToList();

        public Executor CurrentExecutor
        {
            get
            {
                var userName = securityService.User.UserName.ToLowerInvariant();
                return registryContext.Executors.FirstOrDefault(e => e.ExecutorLogin != null &&
                                e.ExecutorLogin.ToLowerInvariant() == userName);
            }
        }
    }
}