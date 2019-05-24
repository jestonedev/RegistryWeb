using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class PremisesComments
    {
        public PremisesComments()
        {
            Premises = new HashSet<Premises>();
        }

        public int IdPremisesComment { get; set; }
        public string PremisesCommentText { get; set; }

        public virtual ICollection<Premises> Premises { get; set; }
    }
}
