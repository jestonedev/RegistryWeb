using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities
{
    public partial class ResettleInfoSubPremiseFrom
    {
        public int IdKey { get; set; }
        [Required]
        public int IdSubPremise { get; set; }
        [Required]
        public int IdResettleInfo { get; set; }
        public byte Deleted { get; set; }

        public virtual SubPremise SubPremiseNavigation { get; set; }
        public virtual ResettleInfo ResettleInfoNavigation { get; set; }
    }
}
