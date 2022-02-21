using RegistryDb.Models.Entities;
using RegistryDb.Models;
using System.Linq;

namespace RegistryWeb.ViewModel
{
    public class SubPremiseVM : SubPremise
    {
        public PaymentsInfo PaymentInfo { get; set; }
        public int? IdFundType { get; set; }
        public SubPremiseVM() { }

        public SubPremiseVM(SubPremise subPremise, PaymentsInfo paymentsInfo)
        {
            IdSubPremises = subPremise.IdSubPremises;
            IdPremises = subPremise.IdPremises;
            IdState = subPremise.IdState;
            SubPremisesNum = subPremise.SubPremisesNum;
            TotalArea = subPremise.TotalArea;
            LivingArea = subPremise.LivingArea;
            Description = subPremise.Description;         
            CadastralNum = subPremise.CadastralNum;
            CadastralCost = subPremise.CadastralCost;
            BalanceCost = subPremise.BalanceCost;
            Account = subPremise.Account;
            PaymentInfo = paymentsInfo;

            var fundsHistory = subPremise.FundsSubPremisesAssoc.Select(fpa => fpa.IdFundNavigation).Where(fh => fh.ExcludeRestrictionDate == null);
            var fundHistory = fundsHistory.FirstOrDefault(fh => fh.IdFund == fundsHistory.Max(f => f.IdFund));
            IdFundType = fundHistory?.IdFundType;
        }
    }
}
