using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryDb.Models.Entities;
using RegistryDb.Models.Entities.RegistryObjects.Common.Funds;
using System.Collections.Generic;

namespace RegistryServices.ViewModel.RegistryObjects
{
    public class FundHistoryVM
    {
        public int IdObject { get; set; }
        public string TypeObject { get; set; }
        public FundHistory FundHistory { get; set; }
        public IEnumerable<FundHistory> FundsHistory { get; set; }
        public SelectList FundTypesList { get; set; }
    }
}
