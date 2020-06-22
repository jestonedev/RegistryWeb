using RegistryWeb.Models.Entities;
using RegistryWeb.Models;
namespace RegistryWeb.ViewModel
{
    public class ResettleInfoVM : ResettleInfo
    {
        public Address Address { get; set; }

        public ResettleInfoVM() { }

        public ResettleInfoVM(ResettleInfo ri, Address address)
        {
            IdResettleInfo = ri.IdResettleInfo;
            IdResettleKind = ri.IdResettleKind;
            ResettleDate = ri.ResettleDate;
            FinanceSource1 = ri.FinanceSource1;
            FinanceSource2 = ri.FinanceSource2;
            FinanceSource3 = ri.FinanceSource3;
            FinanceSource4 = ri.FinanceSource4;
            Deleted = ri.Deleted;
            ResettleKindNavigation = ri.ResettleKindNavigation;
            ResettleInfoSubPremisesFrom = ri.ResettleInfoSubPremisesFrom;
            ResettlePremisesAssoc = ri.ResettlePremisesAssoc;
            ResettleDocuments = ri.ResettleDocuments;
            ResettleInfoTo = ri.ResettleInfoTo;
            Address = address;
        }
    }
}
