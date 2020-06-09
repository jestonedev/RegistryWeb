using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using RegistryWeb.ViewModel;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewOptions;
using RegistryWeb.ViewOptions.Filter;
using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Runtime.CompilerServices;

namespace RegistryWeb.DataServices
{
    public class PremisesDataService : ListDataService<PremisesVM<Premise>, PremisesListFilter>
    {
        private BuildingsDataService BuildingsDataService;
        public PremisesDataService(RegistryContext rc, BuildingsDataService BuildingsDataService) : base(rc)
        {
            this.BuildingsDataService = BuildingsDataService;
        }

        public override PremisesVM<Premise> InitializeViewModel(OrderOptions orderOptions, PageOptions pageOptions, PremisesListFilter filterOptions)
        {
            var viewModel = base.InitializeViewModel(orderOptions, pageOptions, filterOptions);
            viewModel.KladrStreetsList = new SelectList(KladrStreets, "IdStreet", "StreetName");            
            viewModel.PremisesTypesList = new SelectList(registryContext.PremisesTypes, "IdPremisesType", "PremisesType");
            viewModel.HeatingTypesList = new SelectList(HeatingTypes, "IdHeatingType", "IdHeatingType1");
            viewModel.StructureTypesList = new SelectList(StructureTypes, "IdStructureType", "StructureTypeName");
            viewModel.ObjectStatesList = new SelectList(ObjectStates, "IdState", "StateFemale");
            viewModel.PremisesTypesList = new SelectList(registryContext.PremisesTypes, "IdPremisesType", "PremisesTypeName");
            viewModel.FundTypesList = new SelectList(registryContext.FundTypes, "IdFundType", "FundTypeName");
            viewModel.OwnershipRightTypesList = new SelectList(registryContext.OwnershipRightTypes, "IdOwnershipRightType", "OwnershipRightTypeName");
            viewModel.RestrictionsList = new SelectList(registryContext.RestrictionTypes, "IdRestrictionType", "RestrictionTypeName");
            viewModel.LocationKeysList = new SelectList(registryContext.PremisesDoorKeys, "IdPremisesDoorKeys", "LocationOfKeys");
            viewModel.CommentList = new SelectList(registryContext.PremisesComments, "IdPremisesComment", "PremisesCommentText");
            return viewModel;
        }

        public PremisesVM<Premise> GetViewModel(
            OrderOptions orderOptions,            
            PageOptions pageOptions,
            PremisesListFilter filterOptions)
        {
            var viewModel = InitializeViewModel(orderOptions, pageOptions, filterOptions);
            var query = GetQuery();
            viewModel.PageOptions.TotalRows = query.Count();
            query = GetQueryFilter(query, viewModel.FilterOptions);
            query = GetQueryOrder(query, viewModel.OrderOptions);
            query = GetQueryIncludes(query);
            var count = query.Count();
            viewModel.PageOptions.Rows = count;
            viewModel.PageOptions.TotalPages = (int)Math.Ceiling(count / (double)viewModel.PageOptions.SizePage);
            if (viewModel.PageOptions.TotalPages < viewModel.PageOptions.CurrentPage)
                viewModel.PageOptions.CurrentPage = 1;
            viewModel.Premises = GetQueryPage(query, viewModel.PageOptions).ToList();
            viewModel.PaymentsInfo = GetPaymentInfo(viewModel.Premises);
            return viewModel;
        }

        private IQueryable<Premise> GetQueryIncludes(IQueryable<Premise> query)
        {
            return query
                .Include(p => p.IdBuildingNavigation)
                    .ThenInclude(b => b.IdStreetNavigation)
                .Include(p => p.IdStateNavigation)        //Текущее состояние объекта
                .Include(p => p.IdPremisesTypeNavigation) //Тип помещения: квартира, комната, квартира с подселением
                .Include(p => p.FundsPremisesAssoc)
                    .ThenInclude(fpa => fpa.IdFundNavigation)
                        .ThenInclude(fh => fh.IdFundTypeNavigation);
        }

        public IQueryable<Premise> GetQuery()
        {
            return GetQueryIncludes(registryContext.Premises);           
        }

        private IQueryable<Premise> GetQueryFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (!filterOptions.IsAddressEmpty())
            {
                query = AddressFilter(query, filterOptions);
            }
            if (filterOptions.IdPremise.HasValue)
            {
                query = query.Where(p => p.IdPremises == filterOptions.IdPremise.Value);
            }
            if (!string.IsNullOrEmpty(filterOptions.IdStreet))
            {
                query = query.Where(b => b.IdBuildingNavigation.IdStreet == filterOptions.IdStreet);
            }
            if (!string.IsNullOrEmpty(filterOptions.House))
            {
                query = query.Where(b => b.IdBuildingNavigation.House.ToLower() == filterOptions.House.ToLower());
            }
            if (!string.IsNullOrEmpty(filterOptions.PremisesNum))
            {
                query = query.Where(b => b.PremisesNum.ToLower() == filterOptions.PremisesNum.ToLower());
            }
            if (filterOptions.Floors.HasValue)
            {
                query = query.Where(b => b.Floor == filterOptions.Floors.Value);
            }
            if (!string.IsNullOrEmpty(filterOptions.CadastralNum))
            {
                query = query.Where(b => b.CadastralNum == filterOptions.CadastralNum);
            }
            if (filterOptions.IdFundType != null && filterOptions.IdFundType.Any())
            {
                var maxFunds = from fundRow in registryContext.FundsHistory
                               join fundPremisesRow in registryContext.FundsPremisesAssoc
                               on fundRow.IdFund equals fundPremisesRow.IdFund
                               where fundRow.ExcludeRestrictionDate == null
                               group fundPremisesRow.IdFund by fundPremisesRow.IdPremises into gs
                               select new
                               {
                                   IdPremises = gs.Key,
                                   IdFund = gs.Max()
                               };
                var idPremises = from fundRow in registryContext.FundsHistory
                                 join maxFundRow in maxFunds
                                 on fundRow.IdFund equals maxFundRow.IdFund
                                 where filterOptions.IdFundType.Contains(fundRow.IdFundType)
                                 select maxFundRow.IdPremises;
                query = from row in query
                        join idPremise in idPremises
                        on row.IdPremises equals idPremise
                        select row;
            }
            if (filterOptions.IdsObjectState != null && filterOptions.IdsObjectState.Any())
            {
                query = query.Where(p => filterOptions.IdsObjectState.Contains(p.IdState));
            }
            if (filterOptions.IdsComment != null && filterOptions.IdsComment.Any())
            {
                query = query.Where(p => filterOptions.IdsComment.Contains(p.IdPremisesComment));
            }
            if (filterOptions.IdsDoorKeys != null && filterOptions.IdsDoorKeys.Any())
            {
                query = query.Where(p => filterOptions.IdsDoorKeys.Contains(p.IdPremisesDoorKeys));
            }

            if ((filterOptions.IdsOwnershipRightType != null && filterOptions.IdsOwnershipRightType.Any()) ||
                !string.IsNullOrEmpty(filterOptions.NumberOwnershipRight) || filterOptions.DateOwnershipRight != null)
            {
                // TODO: Упрощенная фильтрация без учета исключения из аварийного
                query = (from q in query
                         join obaRow in registryContext.OwnershipBuildingsAssoc
                         on q.IdBuilding equals obaRow.IdBuilding into b
                         from bRow in b.DefaultIfEmpty()
                         join orRow in registryContext.OwnershipRights
                         on bRow.IdOwnershipRight equals orRow.IdOwnershipRight into bor
                         from borRow in bor.DefaultIfEmpty()

                         join opaRow in registryContext.OwnershipPremisesAssoc
                         on q.IdPremises equals opaRow.IdPremises into p
                         from pRow in p.DefaultIfEmpty()
                         join orRow in registryContext.OwnershipRights
                         on pRow.IdOwnershipRight equals orRow.IdOwnershipRight into por
                         from porRow in por.DefaultIfEmpty()

                         where (borRow != null &&
                            ((filterOptions.IdsOwnershipRightType == null || !filterOptions.IdsOwnershipRightType.Any() ||
                            filterOptions.IdsOwnershipRightType.Contains(borRow.IdOwnershipRightType)) &&
                            (string.IsNullOrEmpty(filterOptions.NumberOwnershipRight) ||
                             borRow.Number.ToLower() == filterOptions.NumberOwnershipRight.ToLower()) &&
                             (filterOptions.DateOwnershipRight == null || borRow.Date == filterOptions.DateOwnershipRight))) ||
                             (porRow != null &&
                            ((filterOptions.IdsOwnershipRightType == null || !filterOptions.IdsOwnershipRightType.Any() ||
                            filterOptions.IdsOwnershipRightType.Contains(porRow.IdOwnershipRightType)) &&
                            (string.IsNullOrEmpty(filterOptions.NumberOwnershipRight) ||
                             porRow.Number.ToLower() == filterOptions.NumberOwnershipRight.ToLower()) &&
                             (filterOptions.DateOwnershipRight == null || porRow.Date == filterOptions.DateOwnershipRight)))
                         select q).Distinct();
            }

            if ((filterOptions.IdsRestrictionType != null && filterOptions.IdsRestrictionType.Any()) ||
                !string.IsNullOrEmpty(filterOptions.RestrictionNum) || filterOptions.RestrictionDate != null)
            {
                query = (from q in query
                         join rbaRow in registryContext.RestrictionBuildingsAssoc
                         on q.IdBuilding equals rbaRow.IdBuilding into b
                         from bRow in b.DefaultIfEmpty()
                         join rRow in registryContext.Restrictions
                         on bRow.IdRestriction equals rRow.IdRestriction into bor
                         from borRow in bor.DefaultIfEmpty()

                         join rpaRow in registryContext.RestrictionPremisesAssoc
                         on q.IdPremises equals rpaRow.IdPremises into p
                         from pRow in p.DefaultIfEmpty()
                         join rRow in registryContext.Restrictions
                         on pRow.IdRestriction equals rRow.IdRestriction into por
                         from porRow in por.DefaultIfEmpty()

                         where (borRow != null &&
                            ((filterOptions.IdsRestrictionType == null || !filterOptions.IdsRestrictionType.Any() ||
                            filterOptions.IdsRestrictionType.Contains(borRow.IdRestrictionType)) &&
                            (string.IsNullOrEmpty(filterOptions.RestrictionNum) ||
                             borRow.Number.ToLower() == filterOptions.RestrictionNum.ToLower()) &&
                             (filterOptions.RestrictionDate == null || borRow.Date == filterOptions.RestrictionDate))) ||
                             (porRow != null &&
                            ((filterOptions.IdsRestrictionType == null || !filterOptions.IdsRestrictionType.Any() ||
                            filterOptions.IdsRestrictionType.Contains(porRow.IdRestrictionType)) &&
                            (string.IsNullOrEmpty(filterOptions.RestrictionNum) ||
                             porRow.Number.ToLower() == filterOptions.RestrictionNum.ToLower()) &&
                             (filterOptions.RestrictionDate == null || porRow.Date == filterOptions.RestrictionDate)))
                         select q).Distinct();
            }

            if (filterOptions.StDateOwnershipRight.HasValue && !filterOptions.EndDateOwnershipRight.HasValue)
            {
                var stDate = filterOptions.StDateOwnershipRight.Value.Date;
                query = query.Where(r => r.RegDate != null && r.RegDate >= stDate);
            }
            else if (!filterOptions.StDateOwnershipRight.HasValue && filterOptions.EndDateOwnershipRight.HasValue)
            {
                var fDate = filterOptions.EndDateOwnershipRight.Value.Date.AddDays(1);
                query = query.Where(r => r.RegDate != null && r.RegDate < fDate);
            }
            else if (filterOptions.EndDateOwnershipRight.HasValue && filterOptions.StDateOwnershipRight.HasValue)
            {
                var stDate = filterOptions.StDateOwnershipRight.Value.Date;
                var fDate = filterOptions.EndDateOwnershipRight.Value.Date.AddDays(1);
                query = query.Where(r => r.RegDate != null && r.RegDate >= stDate && r.RegDate < fDate);
            }
            return query;
        }

        private IQueryable<Premise> AddressFilter(IQueryable<Premise> query, PremisesListFilter filterOptions)
        {
            if (filterOptions.IsAddressEmpty())
                return query;


            if (filterOptions.Address.AddressType == AddressTypes.Street)            
                return query.Where(q => q.IdBuildingNavigation.IdStreet.Equals(filterOptions.Address.Id));

            int id = 0;
            if (!int.TryParse(filterOptions.Address.Id, out id))
                return query;

            if (filterOptions.Address.AddressType == AddressTypes.Building)            
                return query.Where(q => q.IdBuilding == id);
            
            if (filterOptions.Address.AddressType == AddressTypes.Premise)            
                return query.Where(q => q.IdPremises == id);
            
            if (filterOptions.Address.AddressType == AddressTypes.SubPremise)
            {
                return from q in query

                       join sp in registryContext.SubPremises
                       on q.IdPremises equals sp.IdPremises
                       where sp.IdSubPremises == id
                       select q;
            }
            return query;
        }

        private IQueryable<Premise> GetQueryOrder(IQueryable<Premise> query, OrderOptions orderOptions)
        {
            if (string.IsNullOrEmpty(orderOptions.OrderField) || orderOptions.OrderField == "IdPremises")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdPremises);
                else
                    return query.OrderByDescending(p => p.IdPremises);
            }
            if(orderOptions.OrderField == "Address")
            {
                var addresses = query.Select(p => new
                    {
                        p.IdPremises,
                        Address = string.Concat(p.IdBuildingNavigation.IdStreetNavigation.StreetName, ", ",
                            p.IdBuildingNavigation.House, ", ", p.PremisesNum)
                    });

                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                {
                    return from row in query
                            join addr in addresses
                             on row.IdPremises equals addr.IdPremises
                            orderby addr.Address
                            select row;
                }
                else
                {
                    return from row in query
                            join addr in addresses
                             on row.IdPremises equals addr.IdPremises
                            orderby addr.Address descending
                            select row;
                }
            }
            if (orderOptions.OrderField == "ObjectState")
            {
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                    return query.OrderBy(p => p.IdStateNavigation.StateNeutral);
                else
                    return query.OrderByDescending(p => p.IdStateNavigation.StateNeutral);
            }
            if (orderOptions.OrderField == "CurrentFund")
            {
                var maxFunds = from fundRow in registryContext.FundsHistory
                                   join fundPremisesRow in registryContext.FundsPremisesAssoc
                                   on fundRow.IdFund equals fundPremisesRow.IdFund
                                   where fundRow.ExcludeRestrictionDate == null
                                   group fundPremisesRow.IdFund by fundPremisesRow.IdPremises into gs
                                   select new
                                   {
                                       IdPremises = gs.Key,
                                       IdFund = gs.Max()
                                   };
                var funds = from fundRow in registryContext.FundsHistory
                            join maxFundRow in maxFunds
                            on fundRow.IdFund equals maxFundRow.IdFund
                            join fundTypeRow in registryContext.FundTypes
                            on fundRow.IdFundType equals fundTypeRow.IdFundType
                            select new
                            {
                                maxFundRow.IdPremises,
                                fundTypeRow.FundTypeName
                            };
                if (orderOptions.OrderDirection == OrderDirection.Ascending)
                {
                    return from row in query
                            join fund in funds
                             on row.IdPremises equals fund.IdPremises
                            orderby fund.FundTypeName
                            select row;
                }
                else
                {
                    return from row in query
                            join fund in funds
                             on row.IdPremises equals fund.IdPremises
                            orderby fund.FundTypeName descending
                            select row;
                }
            }
            return query;
        }

        public List<Premise> GetQueryPage(IQueryable<Premise> query, PageOptions pageOptions)
        {
            var result = query;
            result = result.Skip((pageOptions.CurrentPage - 1) * pageOptions.SizePage);
            result = result.Take(pageOptions.SizePage);
            return result.ToList();
        }

        internal void Create(Premise premise, int? IdFundType)
        {
            if (IdFundType != null)
            {
                var fund = new FundHistory
                {
                    IdFundType = IdFundType.Value
                };

                var fpa = new FundPremiseAssoc
                {
                    IdFundNavigation = fund,
                    IdPremisesNavigation = premise
                };
                premise.FundsPremisesAssoc = new List<FundPremiseAssoc>
                {
                    fpa
                };
            }
            registryContext.Premises.Add(premise);
            registryContext.SaveChanges();            
        }

        internal Premise CreatePremise()
        {
            var premise = new Premise {
                RegDate = new DateTime(1999, 10, 29)
            };
            premise.FundsPremisesAssoc = new List<FundPremiseAssoc>() { new FundPremiseAssoc() };
            premise.OwnerPremisesAssoc = new List<OwnerPremiseAssoc>() { new OwnerPremiseAssoc() };
            premise.TenancyPremisesAssoc = new List<TenancyPremiseAssoc>() { new TenancyPremiseAssoc() };

            var subpremise = new SubPremise();
            subpremise.FundsSubPremisesAssoc = new List<FundSubPremiseAssoc>() { new FundSubPremiseAssoc() };
            subpremise.OwnerSubPremisesAssoc = new List<OwnerSubPremiseAssoc>() { new OwnerSubPremiseAssoc() };
            subpremise.TenancySubPremisesAssoc = new List<TenancySubPremiseAssoc>() { new TenancySubPremiseAssoc() };
            premise.SubPremises = new List<SubPremise>() { subpremise };

            return premise;
        }


        public PremisesVM<Premise> GetPremiseView(Premise premise, [CallerMemberName]string action = "")
        {
            var premisesVM = new PremisesVM<Premise>()
            {
                Premise = premise,
                KladrStreetsList = new SelectList(KladrStreets, "IdStreet", "StreetName"),
                HeatingTypesList = new SelectList(HeatingTypes, "IdHeatingType", "IdHeatingType1"),
                StructureTypesList = new SelectList(StructureTypes, "IdStructureType", "StructureTypeName"),
                ObjectStatesList = new SelectList(ObjectStates, "IdState", "StateFemale"),
                PremisesTypesList = new SelectList(registryContext.PremisesTypes, "IdPremisesType", "PremisesTypeName"),
                FundTypesList = new SelectList(registryContext.FundTypes, "IdFundType", "FundTypeName"),
                LocationKeysList = new SelectList(registryContext.PremisesDoorKeys, "IdPremisesDoorKeys", "LocationOfKeys"),
                CommentList = new SelectList(registryContext.PremisesComments, "IdPremisesComment", "PremisesCommentText"),
                OwnershipRightTypesList = new SelectList(registryContext.OwnershipRightTypes, "IdOwnershipRightType", "OwnershipRightTypeName"),
                RestrictionsList = new SelectList(registryContext.RestrictionTypes, "IdRestrictionType", "RestrictionTypeName"),
                IdFundType = (from fhRow in registryContext.FundsHistory
                             join fpa in registryContext.FundsPremisesAssoc
                             on fhRow.IdFund equals fpa.IdFund
                             where fpa.IdPremises == premise.IdPremises && fhRow.ExcludeRestrictionDate == null
                             orderby fpa.IdFund descending
                             select fhRow.IdFundType).FirstOrDefault()
            };

            if (action == "Details" || action == "Delete")
            {
                premisesVM.PaymentsInfo = GetPaymentInfo(new List<Premise> { premisesVM.Premise });
            }

            return premisesVM;
        }

        private List<PaymentsInfo> GetPaymentInfo(List<Premise> premises)
        {
            var ids = premises.Select(p => p.IdPremises).ToList();
            var paymentsInfo = new List<PaymentsInfo>();
            var paymentsPremises = (from paymentRow in registryContext.TenancyPayments
                           join tpRow in registryContext.TenancyProcesses.Include(tp => tp.TenancyPersons)
                           on paymentRow.IdProcess equals tpRow.IdProcess
                           where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                               paymentRow.IdSubPremises == null &&
                               paymentRow.IdPremises != null && ids.Contains(paymentRow.IdPremises.Value)
                           select new {
                               paymentRow.IdPremises,
                               personCnt = tpRow.TenancyPersons.Count(),
                               paymentRow.Payment }).ToList();

            var paymentsSubPremises = (from paymentRow in registryContext.TenancyPayments
                                    join tpRow in registryContext.TenancyProcesses.Include(tp => tp.TenancyPersons)
                                    on paymentRow.IdProcess equals tpRow.IdProcess
                                    where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                        paymentRow.IdSubPremises != null &&
                                        ids.Contains(paymentRow.IdPremises.Value)
                                    select new
                                    {
                                        paymentRow.IdSubPremises,
                                        personCnt = tpRow.TenancyPersons.Count(),
                                        paymentRow.Payment
                                    }).ToList();

            var prePaymentsAfter28082019Premises = (from tpaRow in registryContext.TenancyPremisesAssoc
                                                    join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                    on tpaRow.IdPremise equals paymentRow.IdPremises
                                                    where paymentRow.IdSubPremises == null &&
                                                          paymentRow.IdPremises != null && ids.Contains(paymentRow.IdPremises.Value)
                                                    select new
                                                    {
                                                        tpaRow.IdProcess,
                                                        paymentRow.IdPremises,
                                                        paymentRow.Hb,
                                                        paymentRow.K1,
                                                        paymentRow.K2,
                                                        paymentRow.K3,
                                                        paymentRow.KC,
                                                        paymentRow.RentArea
                                                    }).Distinct().ToList();

            var paymentsAfter28082019Premises = (from paymentRow in prePaymentsAfter28082019Premises
                                                 join tpRow in registryContext.TenancyProcesses.Include(tp => tp.TenancyPersons)
                                                     on paymentRow.IdProcess equals tpRow.IdProcess
                                                 where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                   tpRow.TenancyPersons.Any()
                                                 select paymentRow).Distinct().ToList();

            var prePaymentsAfter28082019SubPremises = (from tspaRow in registryContext.TenancySubPremisesAssoc
                                                       join paymentRow in registryContext.TenancyPaymentsAfter28082019
                                                    on tspaRow.IdSubPremise equals paymentRow.IdSubPremises
                                                       where paymentRow.IdPremises != null && ids.Contains(paymentRow.IdPremises.Value)
                                                       select new
                                                       {
                                                           tspaRow.IdProcess,
                                                           paymentRow.IdSubPremises,
                                                           paymentRow.Hb,
                                                           paymentRow.K1,
                                                           paymentRow.K2,
                                                           paymentRow.K3,
                                                           paymentRow.KC,
                                                           paymentRow.RentArea
                                                       }).Distinct().ToList();

            var paymentsAfter28082019SubPremises = (from paymentRow in prePaymentsAfter28082019SubPremises
                                                    join tpRow in registryContext.TenancyProcesses.Include(tp => tp.TenancyPersons)
                                                        on paymentRow.IdProcess equals tpRow.IdProcess
                                                    where (tpRow.RegistrationNum == null || !tpRow.RegistrationNum.EndsWith("н")) &&
                                                      tpRow.TenancyPersons.Any()
                                                    select paymentRow).Distinct().ToList();

            foreach(var payment in paymentsPremises)
            {
                if (payment.IdPremises == null) continue;
                if (payment.personCnt == 0) continue;
                paymentsInfo.Add(new PaymentsInfo {
                    IdObject = payment.IdPremises.Value,
                    AddresType = AddressTypes.Premise,
                    Payment = payment.Payment
                });
            }
            foreach (var payment in paymentsSubPremises)
            {
                if (payment.IdSubPremises == null) continue;
                if (payment.personCnt == 0) continue;
                paymentsInfo.Add(new PaymentsInfo
                {
                    IdObject = payment.IdSubPremises.Value,
                    AddresType = AddressTypes.SubPremise,
                    Payment = payment.Payment
                });
            }
            foreach (var payment in paymentsAfter28082019Premises)
            {
                if (payment.IdPremises == null) continue;
                var paymentItem = paymentsInfo.FirstOrDefault(p => p.IdObject == payment.IdPremises && p.AddresType == AddressTypes.Premise);
                if (paymentItem == null)
                {
                    paymentItem = new PaymentsInfo {
                        IdObject = payment.IdPremises.Value,
                        AddresType = AddressTypes.Premise
                    };
                }
                paymentItem.Nb = payment.Hb;
                paymentItem.KC = payment.KC;
                paymentItem.K1 = payment.K1;
                paymentItem.K2 = payment.K2;
                paymentItem.K3 = payment.K3;
                paymentItem.PaymentAfter28082019 = Math.Round((payment.K1 + payment.K2 + payment.K3) / 3 * payment.KC * payment.Hb * (decimal)payment.RentArea, 2);
            }
            foreach (var payment in paymentsAfter28082019SubPremises)
            {
                if (payment.IdSubPremises == null) continue;
                var paymentItem = paymentsInfo.FirstOrDefault(p => p.IdObject == payment.IdSubPremises && p.AddresType == AddressTypes.SubPremise);
                if (paymentItem == null)
                {
                    paymentItem = new PaymentsInfo
                    {
                        IdObject = payment.IdSubPremises.Value,
                        AddresType = AddressTypes.SubPremise
                    };
                }
                paymentItem.Nb = payment.Hb;
                paymentItem.KC = payment.KC;
                paymentItem.K1 = payment.K1;
                paymentItem.K2 = payment.K2;
                paymentItem.K3 = payment.K3;
                paymentItem.PaymentAfter28082019 = Math.Round((payment.K1 + payment.K2 + payment.K3) / 3 * payment.KC * payment.Hb * (decimal)payment.RentArea, 2);
            }
            return paymentsInfo;
        }

        internal void Edit(Premise premise)
        {
            registryContext.Premises.Update(premise);
            registryContext.SaveChanges();
        }

        internal void Delete(int idPremise)
        {
            var premise = registryContext.Premises
                .FirstOrDefault(op => op.IdPremises == idPremise);
            if (premise != null)
            {
                premise.Deleted = 1;
                registryContext.SaveChanges();
            }
        }

        internal OwnerType GetOwnerType(int idOwnerType)
            => registryContext.OwnerType.FirstOrDefault(ot => ot.IdOwnerType == idOwnerType);


        public Premise GetPremise(int idPremise)
        {
            return registryContext.Premises.AsNoTracking()
                .Include(b => b.IdBuildingNavigation).ThenInclude(b => b.IdStreetNavigation)
                .Include(b => b.IdBuildingNavigation.IdHeatingTypeNavigation)
                .Include(b => b.IdStateNavigation)                              //Текущее состояние объекта
                .Include(b => b.IdBuildingNavigation.IdStructureTypeNavigation) //Тип помещения: квартира, комната, квартира с подселением
                .Include(b => b.FundsPremisesAssoc).ThenInclude(fpa => fpa.IdFundNavigation).ThenInclude(fh => fh.IdFundTypeNavigation)
                .Include(b => b.IdPremisesCommentNavigation)
                .Include(b => b.IdPremisesTypeNavigation)
                .Include(b => b.IdPremisesDoorKeysNavigation)
                .SingleOrDefault(b => b.IdPremises == idPremise);
        }

        public IEnumerable<ObjectState> ObjectStates
        {
            get => registryContext.ObjectStates.AsNoTracking();
        }

        public IEnumerable<StructureType> StructureTypes
        {
            get => registryContext.StructureTypes.AsNoTracking();
        }

        public IEnumerable<KladrStreet> KladrStreets
        {
            get => registryContext.KladrStreets.AsNoTracking();
        }

        public IEnumerable<HeatingType> HeatingTypes
        {
            get => registryContext.HeatingTypes.AsNoTracking();
        }

        public List<Building> GetHouses(string streetId)
        {

            var house = from bui in registryContext.Buildings
                        where bui.IdStreet.Contains(streetId)
                        select bui;
            return house.ToList();
        }
    }
}
