using Microsoft.AspNetCore.Mvc.Rendering;
using RegistryWeb.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{
    public class ClaimReportsVM
    {
        public List<Executor> Executors { get; set; }
        public List<ClaimStateType> StateTypes { get; set; }
        public Executor CurrentExecutor { get; set; }
        public Dictionary<int, DateTime> MonthsList { get; set; }
    }
}
