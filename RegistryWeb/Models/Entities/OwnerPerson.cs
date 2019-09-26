using System;
using System.ComponentModel.DataAnnotations;

namespace RegistryWeb.Models.Entities
{
    public partial class OwnerPerson : IEquatable<OwnerPerson>
    {
        public int IdOwner { get; set; }
        [Required(ErrorMessage = "Поле «Фамилия» является обязательным для заполнения")]
        public string Surname { get; set; }
        [Required(ErrorMessage = "Поле «Имя» является обязательным для заполнения")]
        public string Name { get; set; }
        public string Patronymic { get; set; }

        public virtual Owner IdOwnerNavigation { get; set; }

        public bool Equals(OwnerPerson op)
        {
            if (op == null)
                return false;
            if (ReferenceEquals(this, op))
                return true;
            return IdOwner == op.IdOwner && Surname == op.Surname &&
                Name == op.Name && Patronymic == op.Patronymic;
        }

        public bool IsEdit(OwnerPerson ownerPerson)
        {
            if (ownerPerson == null)
                throw new NullReferenceException();
            return IdOwner != ownerPerson.IdOwner || Surname != ownerPerson.Surname ||
                Name != ownerPerson.Name || Patronymic != ownerPerson.Patronymic;
        }
    }
}
