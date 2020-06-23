using RegistryWeb.Models.Entities;
using RegistryWeb.Models;
using System.Collections.Generic;
using System.Linq;

namespace RegistryWeb.ViewModel
{
    public class ResettleInfoVM : ResettleInfo
    {
        public Address Address { get; set; }
        public string IdStreet { get; set; }
        public int? IdBuilding { get; set; }
        public int? IdPremise { get; set; }
        public List<int> IdSubPremises { get; set; }
        public List<Building> Buildings { get; set; }
        public List<Premise> Premises { get; set; }
        public List<SubPremise> SubPremises { get; set; }

        public ResettleInfoVM() { }

        public ResettleInfoVM(ResettleInfo ri, Address address, RegistryContext registryContext)
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

            var resettleTo = ri.ResettleInfoTo.FirstOrDefault();
            IdSubPremises = new List<int>();
            if (resettleTo != null)
            {
                switch (resettleTo.ObjectType)
                {
                    case "Building":
                        IdBuilding = resettleTo.IdObject;
                        IdStreet = registryContext.Buildings.FirstOrDefault(b => b.IdBuilding == IdBuilding)?.IdStreet;
                        break;
                    case "Premise":
                        IdPremise = resettleTo.IdObject;
                        IdBuilding = registryContext.Premises.FirstOrDefault(p => p.IdPremises == IdPremise)?.IdBuilding;
                        IdStreet = registryContext.Buildings.FirstOrDefault(b => b.IdBuilding == IdBuilding)?.IdStreet;
                        break;
                    case "SubPremise":
                        IdSubPremises = ri.ResettleInfoTo.Where(r => r.ObjectType == "SubPremise").Select(r => r.IdObject).ToList();
                        IdPremise = registryContext.SubPremises.FirstOrDefault(sp => IdSubPremises.Contains(sp.IdSubPremises))?.IdPremises;
                        IdBuilding = registryContext.Premises.FirstOrDefault(p => p.IdPremises == IdPremise)?.IdBuilding;
                        IdStreet = registryContext.Buildings.FirstOrDefault(b => b.IdBuilding == IdBuilding)?.IdStreet;
                        break;
                }
            }
            Buildings = registryContext.Buildings.Where(b => b.IdStreet == IdStreet).ToList();
            Premises = registryContext.Premises.Where(p => p.IdBuilding == IdBuilding).ToList();
            SubPremises = registryContext.SubPremises.Where(sp => sp.IdPremises == IdPremise).ToList();
        }
    }
}
