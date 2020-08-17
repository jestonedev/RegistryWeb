using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RegistryWeb.Models.SqlViews;

namespace RegistryWeb.Models.Entities
{
    public class ClaimState
    {
        public int IdState { get; set; }
        public int IdClaim { get; set; }
        public int IdStateType { get; set; }
        public DateTime? DateStartState { get; set; }
        public string Executor { get; set; }
        public string Description { get; set; }
        // TODO: Вставить недостающие поля по этапам состояний
        public byte Deleted { get; set; }
        public virtual Claim IdClaimNavigation { get; set; }
        public virtual ClaimStateType IdStateTypeNavigation { get; set; }
    }
}
