using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class PremisesComments
    {
        public PremisesComments()
        {
            Premises = new List<Premises>();
        }

        public int IdPremisesComment { get; set; }
        public string PremisesCommentText { get; set; }

        public virtual IList<Premises> Premises { get; set; }
    }
}
