using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class PrivRealtor
    {
        public PrivRealtor()
        {
        }

        public int IdRealtor { get; set; }
        [Required(ErrorMessage = "Укажите ФИО риелтора")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Укажите паспортные данные")]
        public string Passport { get; set; }
        [Required(ErrorMessage = "Укажите дату рождения")]
        public DateTime DateBirth { get; set; }
        [Required(ErrorMessage = "Укажите место регистрации")]
        public string PlaceOfRegistration { get; set; }
        public byte Deleted { get; set; }
    }
}
