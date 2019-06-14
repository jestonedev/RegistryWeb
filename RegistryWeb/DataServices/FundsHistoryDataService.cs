using RegistryWeb.Models;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace RegistryWeb.DataServices
{
    public class FundsHistoryDataService
    {
        private readonly RegistryContext rc;

        public FundsHistoryDataService(RegistryContext rc)
        {
            this.rc = rc;
        }

        public IEnumerable<FundHistory> GetViewModel(int idPremises)
        {
            var fundsHistory = GetQuery(idPremises);
            return fundsHistory;
        }

        public IEnumerable<FundHistory> GetQuery(int idPremises)
        {
            return rc.FundsPremisesAssoc
                .Include(fpa => fpa.IdFundNavigation)
                .Where(fpa => fpa.IdPremises == idPremises)
                .Select(fpa => fpa.IdFundNavigation)
                .Include(fh => fh.IdFundTypeNavigation);
        }

    }
}
