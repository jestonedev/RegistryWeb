using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities.Owners
{
    public partial class OwnerReasonType
    {
        public OwnerReasonType()
        {
            OwnerReasons = new List<OwnerReason>();
        }

        public int IdReasonType { get; set; }
        [Required(ErrorMessage = "Укажите наименование типа основания собственности")]
        public string ReasonName { get; set; }
        public byte Deleted { get; set; }

        public virtual IList<OwnerReason> OwnerReasons { get; set; }
    }
}
