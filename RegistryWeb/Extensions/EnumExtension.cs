using RegistryWeb.Models;

namespace RegistryWeb.Extensions
{
    public static class EnumExtension
    {
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
