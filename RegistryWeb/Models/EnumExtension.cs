using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.Models
{
    public static class EnumExtension
    {
        public static string GetRuName(this ProcessOwnershipTypeEnum en)
        {
            if (en == ProcessOwnershipTypeEnum.Private)
                return "Частная";
            return "Муниципальная";
        }
    }
}
