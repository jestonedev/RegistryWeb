﻿using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class Kinship
    {
        public Kinship()
        {
            PrivContractors = new List<PrivContractor>();
        }

        public int IdKinship { get; set; }
        public string KinshipName { get; set; }

        public virtual IList<PrivContractor> PrivContractors { get; set; }
    }
}
