using System;

namespace RegistryServices.Classes
{
    public class BksPenaltyMustBeInfo
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal ActualPenalty { get; set; }
        public decimal MustBePenalty { get; set; }
    }
}
