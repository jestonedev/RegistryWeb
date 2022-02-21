using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RegistryDb.Models.Entities
{
    public class TenancyFile
    {
        public int IdFile { get; set; }
        public int IdProcess { get; set; }
        public string FileName { get; set; }
        public string DisplayName { get; set; }
        public string MimeType { get; set; }
        public string Description { get; set; }

        public virtual TenancyProcess IdProcessNavigation { get; set; }
    }
}
