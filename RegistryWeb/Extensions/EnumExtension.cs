using RegistryWeb.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace RegistryWeb.Extensions
{
    public static class EnumExtension
    {
        public static string GetRuName(this Enum en)
        {
            return ((DisplayAttribute)en.GetType()
                .GetMember(en.ToString())
                .First()
                .GetCustomAttributes(typeof(DisplayAttribute), false)[0]).Name;
        }

        public static string GetRuName(this ProcessOwnershipTypeEnum en)
        {
            if (en == ProcessOwnershipTypeEnum.All)
                return "Все";
            if (en == ProcessOwnershipTypeEnum.Private)
                return "Частная";
            return "Муниципальная";
        }
    }
}
