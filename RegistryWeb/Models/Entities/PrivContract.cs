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
        public string CadastreNumber { get; set; } //Кадастровый номер
        public string Floor { get; set; } //Этаж
        public int? Rooms { get; set; } //Количество комнат
        public decimal? LivingSpace { get; set; } //Жилая площадь
        public decimal? LoggiaSpace { get; set; } //Площадь лоджий и балконов
        public decimal? AncillarySpace { get; set; } //Подсобная площадь
        public decimal? ApartmentSpace { get; set; } //Площадь квартиры (жилая + подсобная)
        public decimal? TotalSpace { get; set; } //Общая площадь
        public decimal? CeilingHeight { get; set; } //Высота помещения
        public DateTime? ApplicationDate { get; set; } //Дата подачи заявления
        public DateTime? DateIssue { get; set; } //Дата выдачи
        public DateTime? RegistrationDate { get; set; } //Дата регистрации
        public int? IdTypeProperty { get; set; } //Тип собственности
        public int? IdRroadType { get; set; } //Тип дороги
        public DateTime? InsertDate { get; set; } //Дата ввода данных о договоре
        public string Description { get; set; } //Основание расприватизации или отказа
        public int? IdType { get; set; } //Тип жилого помещения
        public string RegisterMfcNumber { get; set; } //Регистрационный МФЦ №
        public DateTime? DateIssueCivil { get; set; } //Дата выдачи договора гражданам
        public string AdditionalInfo { get; set; }

        public bool IsRefusenik { get; set; } //Отказник
        public bool IsShares { get; set; } //Подселение
        public bool IsRasprivatization { get; set; } //Расприватизация
        public bool? IsHostel { get; set; } //Общежитие
        public bool IsRelocation { get; set; } //Переселение
        public bool IsRefuse { get; set; } //Отказ со стороны муниципалитета

        public bool Deleted { get; set; }

        //
        public string User { get; set; } //Договор подготовил
        public int? IdSpecialist { get; set; } //Специалист, подготовивший договор
        public int IdExecutor { get; set; }

        //
        public string AddressPrivatization { get; set; } //Адрес в приватизации
        public string IdStreet { get; set; }
        public int? IdBuilding { get; set; }
        public int? IdPremise { get; set; }
        public int? IdSubPremise { get; set; }

        public virtual KladrStreet StreetNavigation { get; set; }
        public virtual Building BuildingNavigation { get; set; }
        public virtual Premise PremiseNavigation { get; set; }
        public virtual SubPremise SubPremiseNavigation { get; set; }
        public virtual Executor ExecutorNavigation { get; set; }
        public virtual IList<PrivContractor> PrivContractors { get; set; }
    }
}