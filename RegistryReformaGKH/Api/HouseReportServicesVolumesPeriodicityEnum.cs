using System.Xml.Serialization;

namespace RegistryReformaGKH.Api
{
    public enum HouseReportServicesVolumesPeriodicityEnum
    {
        [XmlEnum(Name = "1")]
        Daily = 1, //Ежедневно

        [XmlEnum(Name = "2")]
        Weekly, //Еженедельно

        [XmlEnum(Name = "3")]
        Monthly, //Ежемесячно

        [XmlEnum(Name = "4")]
        Quarterly, //Ежеквартально

        [XmlEnum(Name = "5")]
        Yearly, //Ежегодно

        [XmlEnum(Name = "6")]
        OnceDay, //Один раз в сутки

        [XmlEnum(Name = "7")]
        TwiceDay, //Два и более раз в сутки

        [XmlEnum(Name = "8")]
        OnceWeek, //Один раз в неделю

        [XmlEnum(Name = "9")]
        TwiceWeek, //Два и более раз в неделю

        [XmlEnum(Name = "10")]
        OnceMonth, //Один раз в месяц

        [XmlEnum(Name = "11")]
        TwiceMonth, //Два и более раз в месяц

        [XmlEnum(Name = "12")]
        OnceQuarter, //Один раз в квартал

        [XmlEnum(Name = "13")]
        TwiceQuarter, //Два и более раз в квартал

        [XmlEnum(Name = "14")]
        OnceYear, //Один раз в год

        [XmlEnum(Name = "15")]
        TwiceYear, //Два и более раз в год

        [XmlEnum(Name = "16")]
        OnceDayNegativeTemp, //Один раз в сутки, при отрицательной температуре

        [XmlEnum(Name = "17")]
        TwiceDayNegativeTemp, //Два и более раз в сутки, при отрицательной температуре

        [XmlEnum(Name = "18")]
        OnceWeekNegativeTemp, //Один раз в неделю, при отрицательной температуре

        [XmlEnum(Name = "19")]
        TwiceWeekNegativeTemp, //Два и более раз в неделю, при отрицательной температуре

        [XmlEnum(Name = "20")]
        OnceDaySnowfall, //Один раз в сутки, при снегопаде

        [XmlEnum(Name = "21")]
        TwiceDaySnowfall, //Два и более раз в сутки, при снегопаде

        [XmlEnum(Name = "22")]
        OnceWeekSnowfall, //Один раз в неделю, при снегопаде

        [XmlEnum(Name = "23")]
        TwiceWeekSnowfall, //Два и более раз в неделю, при снегопаде

        [XmlEnum(Name = "24")]
        Overhaul, //При проведении капитального ремонта

        [XmlEnum(Name = "25")]
        CurrentRepair, //При проведении текущего ремонта

        [XmlEnum(Name = "26")]
        Winter, //При подготовке к зиме

        [XmlEnum(Name = "27")]
        id27, //По мере необходимости

        [XmlEnum(Name = "28")]
        Graphic, //По графику

        [XmlEnum(Name = "29")]
        id29, //По мере выявления

        [XmlEnum(Name = "30")]
        id30, //Ежедневно, кроме выходных и праздничных дней

        [XmlEnum(Name = "31")]
        id31, //Ежедневно, кроме воскресных и праздничных дней

        [XmlEnum(Name = "32")]
        id32, //Не реже одного раза в три дня

        [XmlEnum(Name = "33")]
        id33, //Постоянно при подготовке к отопительному сезону

        [XmlEnum(Name = "34")]
        id34, //После отопительного сезона

        [XmlEnum(Name = "35")]
        OnceMonthNovemberApril, //Один раз в месяц (с ноября по апрель)

        [XmlEnum(Name = "36")]
        Other //Иная
    }
}