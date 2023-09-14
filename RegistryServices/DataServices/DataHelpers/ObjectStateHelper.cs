using System.Collections.Generic;

namespace RegistryWeb.DataHelpers
{
    public static class ObjectStateHelper
    {
        public static List<int> MunicipalIds()
        {
            return new List<int> { 4, 5, 9, 11, 12, 14 };
        }

        public static bool IsMunicipal(int idState)
        {
            return MunicipalIds().Contains(idState);
        }
    }
}
