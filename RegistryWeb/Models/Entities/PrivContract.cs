using System;
using System.Collections.Generic;

namespace RegistryWeb.Models.Entities
{
    public class PrivContract
    {
        public PrivContract()
        {
            PrivContractors = new List<PrivContractor>();
        }

        public int IdContract { get; set; }
        public string RegNumber { get; set; } //Регистрационный номер договора

        // Информация из АИС "Приватизация"
        public string PrivAddress { get; set; } //Адрес в приватизации
        public string PrivFloor { get; set; } //Этаж
        public int? PrivRooms { get; set; } //Количество комнат
        public decimal? PrivTotalSpace { get; set; } //Общая площадь
        public decimal? PrivLivingSpace { get; set; } //Жилая площадь
        public decimal? PrivApartmentSpace { get; set; } //Площадь квартиры (жилая + подсобная)
        public decimal? PrivLoggiaSpace { get; set; } //Площадь лоджий и балконов
        public decimal? PrivAncillarySpace { get; set; } //Подсобная площадь
        public decimal? PrivCeilingHeight { get; set; } //Высота помещения
        public string PrivCadastreNumber { get; set; } //Кадастровый номер

        // Адрес приватизируемого ЖП
        public string IdStreet { get; set; }
        public int? IdBuilding { get; set; }
        public int? IdPremise { get; set; }
        public int? IdSubPremise { get; set; }

        //
        public int IdExecutor { get; set; } = 65536;
        public DateTime? ApplicationDate { get; set; } //Дата подачи заявления
        public DateTime? DateIssue { get; set; } //Дата выдачи
        public DateTime? RegistrationDate { get; set; } //Дата регистрации
        public DateTime? DateIssueCivil { get; set; } //Дата выдачи договора гражданам

        //
        public string SocrentRegNumber { get; set; }
        public DateTime? SocrentDate { get; set; }

        //
        public int? IdTypeProperty { get; set; } //Тип собственности
        public bool IsRefusenik { get; set; } //Отказник
        public bool IsRasprivatization { get; set; } //Расприватизация
        public bool IsRelocation { get; set; } //Переселение
        public bool IsRefuse { get; set; } //Отказ со стороны муниципалитета

        //
        public string AdditionalInfo { get; set; }
        public string Description { get; set; } //Основание расприватизации или отказа

        public bool Deleted { get; set; }

        public virtual KladrStreet StreetNavigation { get; set; }
        public virtual Building BuildingNavigation { get; set; }
        public virtual Premise PremiseNavigation { get; set; }
        public virtual SubPremise SubPremiseNavigation { get; set; }
        public virtual Executor ExecutorNavigation { get; set; }
        public virtual PrivTypeOfProperty TypeOfProperty { get; set; }
        public virtual IList<PrivContractor> PrivContractors { get; set; }
    }
}