using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models
{
    public interface IAddressAssoc : IAddress
    {
        int IdAssoc { get; set; }
        int IdProcess { get; set; }
        byte Deleted { get; set; }

        string GetTable();
        string GetFieldAdress();
        int GetValueAddress();
    }
}
