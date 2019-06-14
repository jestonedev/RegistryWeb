using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public partial class PremisesComment
    {
        public PremisesComment()
        {
            Premises = new List<Premise>();
        }

        public int IdPremisesComment { get; set; }
        public string PremisesCommentText { get; set; }

        public virtual IList<Premise> Premises { get; set; }
    }
}
