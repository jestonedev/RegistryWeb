using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.SecurityServices
{
    [Flags]
    public enum Privileges
    {
        None = 0,
        RegistryRead = 1,
        RegistryWriteNotMunicipal = 2,
        RegistryReadWriteNotMunicipal = 3,
        RegistryWriteMunicipal = 268435456,
        RegistryReadWriteMunicipal = 268435457,
        RegistryWriteAll = RegistryWriteMunicipal | RegistryWriteNotMunicipal,
        RegistryDirectoriesWrite = 4,
        RegistryDirectoriesReadWrite = 5,
        RegistryAnalize = 9,
        RegistryAll = RegistryReadWriteMunicipal | RegistryReadWriteNotMunicipal |
            RegistryDirectoriesReadWrite | RegistryAnalize,
        TenancyRead = 16,
        TenancyWrite = 32,
        TenancyReadWrite = 48,
        TenancyDirectoriesWrite = 64,
        TenancyDirectoriesReadWrite = 80,
        TenancyAnalize = 144,
        TenancyAll = TenancyReadWrite | TenancyDirectoriesReadWrite | TenancyAnalize,
        ClaimsRead = 256,
        ClaimsWrite = 512,
        ClaimsReadWrite = 768,
        ClaimsDirectoriesWrite = 1024,
        ClaimsDirectoriesReadWrite = 1280,
        ClaimsAnalize = 2304,
        ClaimsAll = ClaimsReadWrite | ClaimsDirectoriesReadWrite | ClaimsAnalize,
        PrivRead = 4096,
        PrivWrite = 8192,
        PrivReadWrite = 12288,
        PrivDirectoriesWrite = 16536,
        PrivDirectoriesReadWrite = 20480,
        PrivAnalize = 36864,
        PrivAll = PrivReadWrite | PrivDirectoriesReadWrite | PrivAnalize,
        ExchangeRead = 65536,
        ExchangeWrite = 131072,
        ExchangeReadWrite = 196608,
        ExchangeDirectoriesWrite = 262144,
        ExchangeDirectoriesReadWrite = 327680,
        ExchangeAnalize = 589824,
        ExchangeAll = ExchangeReadWrite | ExchangeDirectoriesReadWrite | ExchangeAnalize,
        OwnerRead = 1048576,
        OwnerWrite = 2097152,
        OwnerReadWrite = 3145728,
        OwnerDirectoriesWrite = 4194304,
        OwnerDirectoriesReadWrite = 5242880,
        OwnerAnalize = 9437184,
        ResettleRead = 16777216,
        ResettleWrite = 33554432,
        ResettleReadWrite = 50331648,
        ResettleDirectoriesWrite = 67108864,
        ResettleDirectoriesReadWrite = 83886080,
        ResettleAnalize = 150994944,
        ResettleAll = ResettleReadWrite | ResettleDirectoriesReadWrite | ResettleAnalize,
        OwnerAll = OwnerReadWrite | OwnerDirectoriesReadWrite | OwnerAnalize,
        All = RegistryAll | TenancyAll | ClaimsAll | PrivAll | ExchangeAll | OwnerAll | ResettleAll
    }
}
