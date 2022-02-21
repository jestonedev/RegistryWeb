using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryDb.Interfaces
{
    public interface IDbConnectionSettings
    {
        string GetConnectionString();
        string GetDbName();
    }
}
