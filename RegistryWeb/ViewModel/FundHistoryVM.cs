using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.Models.Entities;
using System.Collections.Generic;

namespace RegistryWeb.ViewModel
{
    public class FundHistoryVM
    {
        public FundHistory FundHistory { get; set; }
        public IEnumerable<FundHistory> FundsHistory { get; set; }
        public SelectList FundTypesList { get; set; }
    }
}
