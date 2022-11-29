using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities.Tenancies
{
    public partial class DocumentIssuedBy
    {
        public int IdDocumentIssuedBy { get; set; }
        [Required(ErrorMessage = "Укажите наименование органа, выдавшего документ")]
        public string DocumentIssuedByName { get; set; }
        public byte Deleted { get; set; }
    }
}
