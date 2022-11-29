using System.Collections.Generic;

namespace RegistryDb.Models.Entities.RegistryObjects.Premises
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
