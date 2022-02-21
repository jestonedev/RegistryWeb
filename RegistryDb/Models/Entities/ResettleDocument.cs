using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryDb.Models.Entities
{
    public partial class ResettleDocument
    {
        public ResettleDocument()
        {
        }

        public int IdDocument{ get; set; }
        [Required]
        public int IdDocumentType { get; set; }
        [Required]
        public int IdResettleInfo { get; set; }
        public string Number { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string FileOriginName { get; set; }
        public string FileDisplayName { get; set; }
        public string FileMimeType { get; set; }
        public byte Deleted { get; set; }

        public virtual ResettleInfo ResettleInfoNavigation { get; set; }
        public virtual ResettleDocumentType ResettleDocumentTypeNavigation { get; set; }
    }
}
