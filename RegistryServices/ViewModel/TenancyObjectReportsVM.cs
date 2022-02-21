using Microsoft.AspNetCore.Mvc.Rendering;

namespace RegistryWeb.ViewModel
{
    public class TenancyObjectReportsVM
    {
        public SelectList RegionsList { get; set; }
        public SelectList StreetsList { get; set; }
        public SelectList HousesList { get; set; }
        public SelectList PremisesNumList { get; set; }
        public SelectList RentTypesList { get; set; }
        public SelectList TenancyReasonTypesList { get; set; }
        public SelectList PreparersList { get; set; }
        public SelectList LawyersList { get; set; }
    }
}
