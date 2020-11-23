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
        public double TotalArea { get; set; }
        public double LivingArea { get; set; }
        public List<int> IdSubPremises { get; set; }
        public List<Building> Buildings { get; set; }
        public List<Premise> Premises { get; set; }
        public List<SubPremise> SubPremises { get; set; }


        public string IdStreetFact { get; set; }
        public int? IdBuildingFact { get; set; }
        public int? IdPremiseFact { get; set; }
        public double TotalAreaFact { get; set; }
        public double LivingAreaFact { get; set; }
        public List<int> IdSubPremisesFact { get; set; }
        public List<Building> BuildingsFact { get; set; }
        public List<Premise> PremisesFact { get; set; }
        public List<SubPremise> SubPremisesFact { get; set; }

        public ResettleInfoVM() { }

        public ResettleInfoVM(ResettleInfo ri, Address address, RegistryContext registryContext)
        {
            IdResettleInfo = ri.IdResettleInfo;
            IdResettleKind = ri.IdResettleKind;
            IdResettleKindFact = ri.IdResettleKindFact;
            IdResettleStage = ri.IdResettleStage;
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
            ResettleInfoToFact = ri.ResettleInfoToFact;
            Address = address;

            // Плановый адрес переселения
            var resettleTo = ri.ResettleInfoTo.FirstOrDefault();
            IdSubPremises = new List<int>();
            if (resettleTo != null)
            {
                switch (resettleTo.ObjectType)
                {
                    case "Building":
                        IdBuilding = resettleTo.IdObject;
                        var building = registryContext.Buildings.FirstOrDefault(b => b.IdBuilding == IdBuilding);
                        TotalArea = building?.TotalArea ?? 0;
                        LivingArea = building?.LivingArea ?? 0;
                        IdStreet = building?.IdStreet;
                        break;
                    case "Premise":
                        IdPremise = resettleTo.IdObject;
                        var premise = registryContext.Premises.FirstOrDefault(p => p.IdPremises == IdPremise);
                        TotalArea = premise?.TotalArea ?? 0;
                        LivingArea = premise?.LivingArea ?? 0;
                        IdBuilding = premise?.IdBuilding;
                        IdStreet = registryContext.Buildings.FirstOrDefault(b => b.IdBuilding == IdBuilding)?.IdStreet;
                        break;
                    case "SubPremise":
                        IdSubPremises = ri.ResettleInfoTo.Where(r => r.ObjectType == "SubPremise").Select(r => r.IdObject).ToList();
                        TotalArea = registryContext.SubPremises.Where(sp => IdSubPremises.Contains(sp.IdSubPremises)).Select(sp => sp.TotalArea).Sum();
                        LivingArea = registryContext.SubPremises.Where(sp => IdSubPremises.Contains(sp.IdSubPremises)).Select(sp => sp.LivingArea).Sum();
                        IdPremise = registryContext.SubPremises.FirstOrDefault(sp => IdSubPremises.Contains(sp.IdSubPremises))?.IdPremises;
                        IdBuilding = registryContext.Premises.FirstOrDefault(p => p.IdPremises == IdPremise)?.IdBuilding;
                        IdStreet = registryContext.Buildings.FirstOrDefault(b => b.IdBuilding == IdBuilding)?.IdStreet;
                        break;
                }
            }
            Buildings = registryContext.Buildings.Where(b => b.IdStreet == IdStreet).ToList();
            Premises = registryContext.Premises.Where(p => p.IdBuilding == IdBuilding).ToList();
            SubPremises = registryContext.SubPremises.Where(sp => sp.IdPremises == IdPremise).ToList();

            // Фактический адрес переселения
            var resettleToFact = ri.ResettleInfoToFact.FirstOrDefault();
            IdSubPremisesFact = new List<int>();
            if (resettleToFact != null)
            {
                switch (resettleToFact.ObjectType)
                {
                    case "Building":
                        IdBuildingFact = resettleToFact.IdObject;
                        var building = registryContext.Buildings.FirstOrDefault(b => b.IdBuilding == IdBuildingFact);
                        TotalAreaFact = building?.TotalArea ?? 0;
                        LivingAreaFact = building?.LivingArea ?? 0;
                        IdStreetFact = building?.IdStreet;
                        break;
                    case "Premise":
                        IdPremiseFact = resettleToFact.IdObject;
                        var premise = registryContext.Premises.FirstOrDefault(p => p.IdPremises == IdPremiseFact);
                        TotalAreaFact = premise?.TotalArea ?? 0;
                        LivingAreaFact = premise?.LivingArea ?? 0;
                        IdBuildingFact = premise?.IdBuilding;
                        IdStreetFact = registryContext.Buildings.FirstOrDefault(b => b.IdBuilding == IdBuildingFact)?.IdStreet;
                        break;
                    case "SubPremise":
                        IdSubPremisesFact = ri.ResettleInfoToFact.Where(r => r.ObjectType == "SubPremise").Select(r => r.IdObject).ToList();
                        TotalAreaFact = registryContext.SubPremises.Where(sp => IdSubPremisesFact.Contains(sp.IdSubPremises)).Select(sp => sp.TotalArea).Sum();
                        LivingAreaFact = registryContext.SubPremises.Where(sp => IdSubPremisesFact.Contains(sp.IdSubPremises)).Select(sp => sp.LivingArea).Sum();
                        IdPremiseFact = registryContext.SubPremises.FirstOrDefault(sp => IdSubPremisesFact.Contains(sp.IdSubPremises))?.IdPremises;
                        IdBuildingFact = registryContext.Premises.FirstOrDefault(p => p.IdPremises == IdPremiseFact)?.IdBuilding;
                        IdStreetFact = registryContext.Buildings.FirstOrDefault(b => b.IdBuilding == IdBuildingFact)?.IdStreet;
                        break;
                }
            }
            BuildingsFact = registryContext.Buildings.Where(b => b.IdStreet == IdStreetFact).ToList();
            PremisesFact = registryContext.Premises.Where(p => p.IdBuilding == IdBuildingFact).ToList();
            SubPremisesFact = registryContext.SubPremises.Where(sp => sp.IdPremises == IdPremiseFact).ToList();
        }
    }
}
