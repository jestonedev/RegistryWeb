using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models
{
    public interface IPrivEstateBinder
    {
        string IdStreet { get; set; }
        int? IdBuilding { get; set; }
        int? IdPremise { get; set; }
        int? IdSubPremise { get; set; }
    }
}
