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
                FundTypesList = new SelectList(rc.FundTypes, "IdFundType", "FundTypeName")
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
                //FundsHistory = GetFundHistory(),
                FundTypesList = new SelectList(rc.FundTypes, "IdFundType", "FundTypeName")
            };

            return fundHistoryVM;
        }

        public List<FundHistory> GetFundHistory(int idFund)
        {
            /*var fundHistoryVM = new FundHistoryVM()
            {
                FundHistory = rc.FundsHistory
                                .Include(fh => fh.IdFundTypeNavigation)
                                .FirstOrDefault(fh => fh.IdFund == idFund),
                FundTypesList = new SelectList(rc.FundTypes, "IdFundType", "FundTypeName")
            };
            return fundHistoryVM;*/

            var fund = from fun in rc.FundsHistory
                        join ftn in rc.FundTypes 
                        on fun.IdFundType equals ftn.IdFundType
                        where fun.IdFund==idFund
                        select fun;
            return fund.ToList();

            /*return rc.FundsHistory
                .Include(fh => fh.IdFundTypeNavigation)
                .FirstOrDefault(fh=>fh.IdFund== idFund);*/
        }

    }
}
