using RegistryWeb.Models.Entities;
using RegistryWeb.ViewOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewModel
{
    public class PrivRealtorVM<T>
    {
        public List<PrivRealtor> PrivRealtors { get; set; }
        public PrivRealtor PrivRealtor { get; set; }
        public PageOptions PageOptions { get; set; }
    }
}
