﻿using System;

namespace RegistryDb.Models.Entities.Tenancies
{
    public class TenancyPerson
    {
        public int IdPerson { get; set; }
        public int IdProcess { get; set; }
        public int IdKinship { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
        public string Patronymic { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int IdDocumentType { get; set; }
        public DateTime? DateOfDocumentIssue { get; set; }
        public string DocumentNum { get; set; }
        public string DocumentSeria { get; set; }
        public int? IdDocumentIssuedBy { get; set; }
        public string Snils { get; set; }
        public string RegistrationIdStreet { get; set; }
        public string RegistrationHouse { get; set; }
        public string RegistrationFlat { get; set; }
        public string RegistrationRoom { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public string ResidenceIdStreet { get; set; }
        public string ResidenceHouse { get; set; }
        public string ResidenceFlat { get; set; }
        public string ResidenceRoom { get; set; }
        public string PersonalAccount { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
        public DateTime? IncludeDate { get; set; }
        public DateTime? ExcludeDate { get; set; }
        public byte Deleted { get; set; }

        public virtual TenancyProcess IdProcessNavigation { get; set; }
    }
}
