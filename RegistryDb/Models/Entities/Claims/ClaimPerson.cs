﻿using System;

namespace RegistryDb.Models.Entities.Claims
{
    public class ClaimPerson
    {
        public int IdPerson { get; set; }
        public int IdClaim { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PlaceOfBirth { get; set; }
        public string WorkPlace { get; set; }
        public string Passport { get; set; }
        public bool IsClaimer { get; set; }
        public byte Deleted { get; set; }
        public virtual Claim IdClaimNavigation { get; set; }
    }
}
