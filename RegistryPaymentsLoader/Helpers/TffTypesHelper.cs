using System;
using System.Collections.Generic;
using System.Text;

namespace RegistryPaymentsLoader.Helpers
{
    internal static class TffTypesHelper
    {
        internal static DateTime? StringToDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var valueParts = value.Split(".");
            if (valueParts.Length != 3) return null;
            if (!int.TryParse(valueParts[0], out int day)) return null;
            if (!int.TryParse(valueParts[1], out int month)) return null;
            if (!int.TryParse(valueParts[2], out int year)) return null;
            return new DateTime(year, month, day);
        }

        internal static int? StringToInt(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (!int.TryParse(value, out int result)) return null;
            return result;
        }

        internal static decimal? StringToDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (!decimal.TryParse(value.Replace(".", ","), out decimal result)) return null;
            return result;
        }
    }
}
