using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryWeb.ViewOptions.Filter
{
    public class PrivatizationFilter: FilterAddressOptions
    {
        public string RegNumber { get; set; }
        public string IdRegion { get; set; }
        public string IdStreet { get; set; }
        public string House { get; set; }
        public string PremisesNum { get; set; }
        public DateTime? DateIssueCivil { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public DateTime? BirthDate { get; set; }
        public bool IsRefusenik { get; set; }

        public bool IsModalEmpty()
        {
            return RegNumber == null && IdRegion == null && IdStreet == null && House == null && 
                PremisesNum == null && DateIssueCivil == null && Surname == null &&
                Name == null && Patronymic == null && BirthDate == null && IsRefusenik == false;
        }

        public bool IsEmpty()
        {
            return IsAddressEmpty() && IsModalEmpty();
        }
    }
}
