using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class ActTypeDocument
    {
        /*public ActTypeDocument()
        {
        }*/

        public int Id { get; set; }
        public string ActFileType { get; set; }
        public string Name { get; set; }
    }
}
