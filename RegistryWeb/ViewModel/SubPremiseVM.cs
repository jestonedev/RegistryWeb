using RegistryWeb.Models.Entities;
using RegistryWeb.Models;
namespace RegistryWeb.ViewModel
{
    public class SubPremiseVM : SubPremise
    {
        public SubPremiseVM() { }

        public SubPremiseVM(SubPremise subpr)
        {
            IdSubPremises = subpr.IdSubPremises;
            IdPremises = subpr.IdPremises;
            IdState = subpr.IdState;
            SubPremisesNum = subpr.SubPremisesNum;
            TotalArea = subpr.TotalArea;
            LivingArea = subpr.LivingArea;
            Description = subpr.Description;
            StateDate = subpr.StateDate;
            IdPremisesNavigation = subpr.IdPremisesNavigation;
            IdStateNavigation = subpr.IdStateNavigation;
            FundsSubPremisesAssoc = subpr.FundsSubPremisesAssoc;            
            CadastralNum = subpr.CadastralNum;
            CadastralCost = subpr.CadastralCost;
            BalanceCost = subpr.BalanceCost;
            Account = subpr.Account;
            Deleted = subpr.Deleted;     
        }
    }
}
