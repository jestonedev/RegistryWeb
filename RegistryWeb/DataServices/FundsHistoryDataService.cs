using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RegistryWeb.ViewModel;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.ViewOptions;

namespace RegistryWeb.DataServices
{
    public class FundsHistoryDataService
    {    
        private readonly RegistryContext rc;

        public FundsHistoryDataService(RegistryContext rc)
        {
            this.rc = rc;
        }

        public FundHistoryVM GetListViewModel(int idObject, string typeObject)
        {
            var fundHistoryVM = new FundHistoryVM()
            {
                FundsHistory = GetQuery(idObject, typeObject),
                FundHistory = CreateFundHistory(),
                FundTypesList = new SelectList(rc.FundTypes, "IdFundType", "FundTypeName"),
                IdObject= idObject,
                TypeObject= typeObject
            };

            return fundHistoryVM;
        }

        internal FundHistory CreateFundHistory()
        {
            var fundhistory = new FundHistory();
            fundhistory.FundsPremisesAssoc = new List<FundPremiseAssoc>() { new FundPremiseAssoc() };
            fundhistory.FundsBuildingsAssoc = new List<FundBuildingAssoc>() { new FundBuildingAssoc() };
            fundhistory.FundsSubPremisesAssoc = new List<FundSubPremiseAssoc>() { new FundSubPremiseAssoc() };
            return fundhistory;
        }

        public IEnumerable<FundHistory> GetQuery(int idObject, string typeObject)
        {
            var funds = typeObject =="Premise"  ?    from fpa in rc.FundsPremisesAssoc
                                                    join fh in rc.FundsHistory on fpa.IdFund equals fh.IdFund
                                                    where fpa.IdPremises == idObject
                                                    select fh   
                                                : from fba in rc.FundsBuildingsAssoc
                                                    join fh in rc.FundsHistory on fba.IdFund equals fh.IdFund
                                                    where fba.IdBuilding == idObject
                                                    select fh;


            funds = funds.Include(f => f.IdFundTypeNavigation);
            return funds;
        }

        public FundHistoryVM GetViewModel(FundHistory fundsHistory, [CallerMemberName]string action = "")
        {
            var fundHistoryVM = new FundHistoryVM()
            {
                FundTypesList = new SelectList(rc.FundTypes, "IdFundType", "FundTypeName")
            };

            return fundHistoryVM;
        }

        public List<FundHistory> GetFundHistory(int idFund)
        {
            var fund = from fun in rc.FundsHistory
                        join ftn in rc.FundTypes 
                        on fun.IdFundType equals ftn.IdFundType
                        where fun.IdFund==idFund
                        select fun;
            return fund.ToList();
        }

        public FundHistory GetFund(int idFund)
        {
            var fund = rc.FundsHistory.AsNoTracking()
                        .Include(fh => fh.IdFundTypeNavigation)
                        .Include(fh => fh.FundsPremisesAssoc)
                        .Include(fh => fh.FundsBuildingsAssoc)
                        .Include(fh => fh.FundsSubPremisesAssoc)
                        .FirstOrDefault(fh => fh.IdFund == idFund);
            return fund;
        }

        public FundHistoryVM GetFundHistoryView(FundHistory fundhis, int IdObject, string typeObject, [CallerMemberName]string action = "")
        {
            var fhVM = new FundHistoryVM()
            {
                FundHistory = fundhis,
                FundTypesList = new SelectList(rc.FundTypes, "IdFundType", "FundTypeName"),
                IdObject = IdObject,
                TypeObject = typeObject
            };
            return fhVM;
        }

        internal void Create(FundHistory fh, int IdObject, string typeObject)
        {
            fh.IdFundType = fh.IdFundTypeNavigation.IdFundType;
            fh.IdFundTypeNavigation = null;
            rc.FundsHistory.Add(fh);

            if (typeObject == "Premise")
            {
                var foa = new FundPremiseAssoc
                {
                    IdFund = fh.IdFund,
                    IdPremises = IdObject
                };
                rc.FundsPremisesAssoc.Add(foa);
            }
            else if (typeObject == "Building")
            {
                var foa = new FundBuildingAssoc
                {
                    IdFund = fh.IdFund,
                    IdBuilding = IdObject
                };
                rc.FundsBuildingsAssoc.Add(foa);
            }
            else if (typeObject == "SubPremise")
            {
                var foa = new FundSubPremiseAssoc
                {
                    IdFund = fh.IdFund,
                    IdSubPremises = IdObject
                };
                rc.FundsSubPremisesAssoc.Add(foa);
            }

            rc.SaveChanges();
        }

        internal void Edit(FundHistory fh)
        {
            var oldFH = GetFund(fh.IdFund);

            foreach (var fpa in oldFH.FundsPremisesAssoc)
            {
                if (fh.FundsPremisesAssoc.Select(owba => owba.IdPremises).Contains(fpa.IdPremises) == false)
                {
                    fpa.Deleted = 1;
                    fh.FundsPremisesAssoc.Add(fpa);
                }
            } 
            foreach (var fpa in oldFH.FundsSubPremisesAssoc)
            {
                if (fh.FundsSubPremisesAssoc.Select(owba => owba.IdSubPremises).Contains(fpa.IdSubPremises) == false)
                {
                    fpa.Deleted = 1;
                    fh.FundsSubPremisesAssoc.Add(fpa);
                }
            } 
            foreach (var fpa in oldFH.FundsBuildingsAssoc)
            {
                if (fh.FundsBuildingsAssoc.Select(owba => owba.IdBuilding).Contains(fpa.IdBuilding) == false)
                {
                    fpa.Deleted = 1;
                    fh.FundsBuildingsAssoc.Add(fpa);
                }
            }

            //Добавление и радактирование
            rc.FundsHistory.Update(fh);
            rc.SaveChanges();
        }

        internal void Delete(int idFund)
        {
            var oldFund = GetFund(idFund);

            oldFund.Deleted = 1;
            foreach (var fpa in oldFund.FundsPremisesAssoc)
            {
                fpa.Deleted = 1;
            }
            foreach (var fpa in oldFund.FundsBuildingsAssoc)
            {
                fpa.Deleted = 1;
            }
            foreach (var fpa in oldFund.FundsSubPremisesAssoc)
            {
                fpa.Deleted = 1;
            }
            rc.FundsHistory.Update(oldFund);
            rc.SaveChanges();
        }


    }
}
