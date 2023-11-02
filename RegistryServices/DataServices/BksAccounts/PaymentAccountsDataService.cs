using RegistryDb.Models;
using RegistryWeb.ViewModel;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.Enums;
using RegistryServices.ViewModel.Payments;
using RegistryDb.Models.Entities.Acl;
using RegistryDb.Models.Entities.Claims;
using RegistryDb.Models.Entities.Common;
using RegistryDb.Models.Entities.Payments;
using RegistryDb.Models.Entities.RegistryObjects.Premises;
using RegistryServices.Enums;
using RegistryWeb.DataServices.Claims;
using RegistryServices.DataFilterServices;
using RegistryWeb.DataServices;

namespace RegistryServices.DataServices.BksAccounts
{
    public class PaymentAccountsDataService : ListDataService<PaymentsVM, PaymentsFilter>
    {
        private readonly PaymentAccountsCommonService commonService;
        private readonly PaymentAccountsClaimsService claimsDataService;
        private readonly IConfiguration config;
        private readonly IFilterService<Payment, PaymentsFilter> filterService;

        public PaymentAccountsDataService(RegistryContext registryContext, 
            PaymentAccountsCommonService commonService,
            PaymentAccountsClaimsService claimsDataService,
            AddressesDataService addressesDataService, IConfiguration config,
            FilterServiceFactory<IFilterService<Payment, PaymentsFilter>> filterServiceFactory) : base(registryContext, addressesDataService)
        {
            this.commonService = commonService;
            this.claimsDataService = claimsDataService;
            this.config = config;
            filterService = filterServiceFactory.CreateInstance();
        }

        public override PaymentsVM InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, PaymentsFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.Streets = addressesDataService.KladrStreets;
            return viewModel;
        }

        public PaymentsVM GetViewModel(
            OrderOptions orderOptions,
            PageOptions pageOptions,
            PaymentsFilter filterOptions, out List<int> filteredIds)
        {
            
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.KladrRegionsList = new SelectList(addressesDataService.KladrRegions, "IdRegion", "Region");
            viewModel.KladrStreetsList = new SelectList(addressesDataService.GetKladrStreets(filterOptions?.IdRegion), "IdStreet", "StreetName");
            viewModel.BuildingManagmentOrgsList = new SelectList(registryContext.BuildingManagmentOrgs , "IdOrganization", "Name");

            if (viewModel.FilterOptions.IsEmpty())
            {
                viewModel.PageOptions.Rows = 0;
                viewModel.PageOptions.TotalPages = 0;
                filteredIds = null;
                viewModel.Payments = new List<Payment>();
                viewModel.MonthsList = new Dictionary<int, DateTime>();
                return viewModel;
            }

            var payments = commonService.GetQuery();
            viewModel.PageOptions.TotalRows = payments.Count();
            var query = filterService.GetQueryFilter(payments, viewModel.FilterOptions);
            
            query = filterService.GetQueryOrder(query, viewModel.OrderOptions);
            query = filterService.GetQueryIncludes(query);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            filteredIds = query.Select(c => c.IdAccount).ToList();
            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            query = filterService.GetQueryPage(query, viewModel.PageOptions);
            viewModel.Payments = query.ToList();
            viewModel.RentObjects = GetRentObjects(viewModel.Payments);
            viewModel.ClaimsByAddresses = claimsDataService.GetClaimsByAddresses(viewModel.Payments);

            var monthsList = registryContext.Payments
                                .Select(p => p.Date).Distinct()
                                .OrderByDescending(p => p.Date).Take(6)
                                .ToList();
            viewModel.MonthsList = new Dictionary<int, DateTime>();
            for (var i = 0; i < monthsList.Count(); i++)
                viewModel.MonthsList.Add(monthsList[i].Month, monthsList[i].Date);
            return viewModel;
        }

        private Dictionary<int, List<Address>> GetRentObjects(IEnumerable<Payment> payments)
        {
            var ids = payments.Select(r => r.IdAccount);
            var premises = from paRow in registryContext.PaymentAccountPremisesAssoc
                           join premiseRow in registryContext.Premises.Include(r => r.IdStateNavigation)
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
                                   ObjectState = premiseRow.IdStateNavigation,
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
                              join subPremiseRow in registryContext.SubPremises.Include(r => r.IdStateNavigation)
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
                                      ObjectState = subPremiseRow.IdStateNavigation,
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

        public PaymentsAccountTableVM GetPaymentHistoryTable(AclUser user, int idAccount)
        {
            var viewModel = new PaymentsAccountTableVM();
            viewModel.LastPayment = commonService.GetQuery().Single(r => r.IdAccount == idAccount);
            viewModel.Payments = (from row in registryContext.Payments.Include(r => r.PaymentAccountNavigation)
                                  where row.IdAccount == idAccount
                                  orderby row.Date
                                  select row).ToList();
            viewModel.Comment = registryContext.PaymentAccountComments.FirstOrDefault(c => c.IdAccount == idAccount);
            var lastPaymentList = new List<Payment>();
            lastPaymentList.Add(viewModel.LastPayment);
            viewModel.RentObjects = GetRentObjects(lastPaymentList);

            var json = registryContext.PersonalSettings
                .SingleOrDefault(ps => ps.IdUser == user.IdUser)
                ?.PaymentAccauntTableJson;
            if (json != null)
            {
                viewModel.PaymentAccountTableJson =
                    JsonSerializer.Deserialize<PaymentAccountTableJson>(json);
            }
            return viewModel;
        }

        public PaymentsAccountTableVM GetPaymentHistoryRentObjectTable(AclUser user, int idAccount)
        {
            var viewModel = new PaymentsAccountTableVM();
            var lastPayment = commonService.GetQuery().Where(r => r.IdAccount == idAccount).ToList();
            var accountIds = commonService.GetAccountIdsAssocs(lastPayment).Select(r => r.IdAccountActual);
            viewModel.Payments = (from row in registryContext.Payments.Include(r => r.PaymentAccountNavigation)
                                  where accountIds.Contains(row.IdAccount)
                                  orderby row.Date ascending
                                  select row).ToList();
            viewModel.RentObjects = GetRentObjects(lastPayment);
            viewModel.LastPayment = viewModel.Payments.LastOrDefault();

            var accounts = registryContext.PaymentAccounts.Where(r => accountIds.Contains(r.IdAccount)).ToList();
            var comments = registryContext.PaymentAccountComments.Where(r => accountIds.Contains(r.IdAccount)).ToList();
            
            if (comments.Any())
            {
                viewModel.Comment = new PaymentAccountComment
                {
                    IdAccount = idAccount,
                    Comment = (from row in accounts
                                                join com in comments
                                                   on row.IdAccount equals com.IdAccount into a
                                                from b in a.DefaultIfEmpty()
                                                where b != null
                                                group new { b, row } by b.Comment into g
                                                select comments.Select(r => r.Comment).Distinct().Count() > 1 ?
                                                              g.Aggregate("", (acc, v) => acc + "ЛС №:" + v.row.Account + ", ").Trim(new char[] { ' ', ',' }) + ": " + g.Key
                                                                  : g.Key)
                           .Aggregate((x, y) => x + "\r\n" + y)
                };
            }
            
            var json = registryContext.PersonalSettings
                .SingleOrDefault(ps => ps.IdUser == user.IdUser)
                ?.PaymentAccauntTableJson;
            if (json != null)
            {
                viewModel.PaymentAccountTableJson =
                    JsonSerializer.Deserialize<PaymentAccountTableJson>(json);
            }
            return viewModel;
        }

        public bool SavePaymentAccountTableJson(AclUser user, PaymentAccountTableJson vm)
        {
            try
            {
                var json = JsonSerializer.Serialize(vm);
                var personalSetting = registryContext.PersonalSettings
                    .SingleOrDefault(ps => ps.IdUser == user.IdUser);
                if (personalSetting == null)
                {
                    var newPersonalSetting = new PersonalSetting()
                    {
                        IdUser = user.IdUser,
                        PaymentAccauntTableJson = json
                    };
                    registryContext.PersonalSettings.Add(newPersonalSetting);
                }
                else
                {
                    personalSetting.PaymentAccauntTableJson = json;
                }
                registryContext.SaveChanges();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public PaymentsVM GetPaymentsViewModelForMassReports(List<int> ids, PageOptions pageOptions)
        {
            var viewModel = InitializeViewModel(null, pageOptions, null);
            var payments = commonService.GetPaymentsForMassReports(ids);
            viewModel.PageOptions.TotalRows = payments.Count();
            var count = payments.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);

            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.Payments = filterService.GetQueryPage(payments, viewModel.PageOptions).ToList();
            viewModel.RentObjects = GetRentObjects(viewModel.Payments);
            viewModel.ClaimsByAddresses = claimsDataService.GetClaimsByAddresses(viewModel.Payments);
                       
            var monthsList = registryContext.Payments
                            .Select(p => p.Date).Distinct()
                            .OrderByDescending(p => p.Date).Take(6)
                            .ToList();

            viewModel.MonthsList = new Dictionary<int, DateTime>();
            for (var i = 0; i < monthsList.Count(); i++)
                viewModel.MonthsList.Add(monthsList[i].Month, monthsList[i].Date);


            return viewModel;
        }

        public Premise GetPremiseJson(int idPremise)
        {
            var premise = registryContext.Premises
                .Include(p => p.IdStateNavigation)
                .AsNoTracking()
                .SingleOrDefault(p => p.IdPremises == idPremise);
            return premise;
        }

        public List<SelectableSigner> Signers => registryContext.SelectableSigners.ToList();

        public bool AddCommentsForPaymentAccount(int idAccount, string comment, string path)
        {
            try
            {
                switch(path)
                {
                    case "PaymentAccountsTable":
                        SaveComment(idAccount, comment);
                        registryContext.SaveChanges();
                        break;
                    case "PaymentAccountsRentObjectTable":
                        var lastPayment = commonService.GetQuery().Where(r => r.IdAccount == idAccount).ToList();
                        var accounts = commonService.GetAccountIdsAssocs(lastPayment).Select(c=> c.IdAccountActual).ToList();
                        foreach(var item in accounts)
                        {
                            SaveComment(item, comment);
                        }
                        registryContext.SaveChanges();
                        break;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SaveComment(int idAccount, string comment)
        {
            var savedComment = registryContext.PaymentAccountComments
                                                            .Where(c => c.IdAccount == idAccount)
                                                            .FirstOrDefault();
            if (savedComment != null)
            {
                savedComment.Comment = comment;
                registryContext.PaymentAccountComments.Update(savedComment);
            }
            else
            {
                var newComment = new PaymentAccountComment()
                {
                    IdAccount = idAccount,
                    Comment = comment
                };
                registryContext.PaymentAccountComments.Add(newComment);
            }
        }
    }
}