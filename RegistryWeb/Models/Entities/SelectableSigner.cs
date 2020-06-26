using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class SelectableSigner
    {
        public SelectableSigner()
        {
        }

        public int IdRecord { get; set; }
        public int IdSignerGroup { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public string Post { get; set; }
        public string Phone { get; set; }
        public byte Deleted { get; set; }
    }
}
