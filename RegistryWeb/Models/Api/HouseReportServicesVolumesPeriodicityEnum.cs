namespace RegistryWeb.Models.Api
{
    public enum HouseReportServicesVolumesPeriodicityEnum
    {
        Daily = 1, //Ежедневно
        Weekly, //Еженедельно
        Monthly, //Ежемесячно
        Quarterly, //Ежеквартально
        Yearly, //Ежегодно
        OnceDay, //Один раз в сутки
        TwiceDay, //Два и более раз в сутки
        OnceWeek, //Один раз в неделю
        TwiceWeek, //Два и более раз в неделю
        OnceMonth, //Один раз в месяц
        TwiceMonth, //Два и более раз в месяц
        OnceQuarter, //Один раз в квартал
        TwiceQuarter, //Два и более раз в квартал
        OnceYear, //Один раз в год
        TwiceYear, //Два и более раз в год
        OnceDayNegativeTemp, //Один раз в сутки, при отрицательной температуре
        TwiceDayNegativeTemp, //Два и более раз в сутки, при отрицательной температуре
        OnceWeekNegativeTemp, //Один раз в неделю, при отрицательной температуре
        TwiceWeekNegativeTemp, //Два и более раз в неделю, при отрицательной температуре
        OnceDaySnowfall, //Один раз в сутки, при снегопаде
        TwiceDaySnowfall, //Два и более раз в сутки, при снегопаде
        OnceWeekSnowfall, //Один раз в неделю, при снегопаде
        TwiceWeekSnowfall, //Два и более раз в неделю, при снегопаде
        Overhaul, //При проведении капитального ремонта
        CurrentRepair, //При проведении текущего ремонта
        Winter, //При подготовке к зиме
        //По мере необходимости
        Graphic,//По графику
        //По мере выявления
        //Ежедневно, кроме выходных и праздничных дней
        //Ежедневно, кроме воскресных и праздничных дней
        //Не реже одного раза в три дня
        //Постоянно при подготовке к отопительному сезону
        //После отопительного сезона
        OnceMonthNovemberApril, //Один раз в месяц (с ноября по апрель)
        Other //Иная
    }
}