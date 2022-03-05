using System;

namespace RegistryDb.Models.Entities.Payments
{
    public class Payment
    {
        public Payment()
        {
        }

        public int IdPayment { get; set; }
        public int IdAccount { get; set; }
        /// <summary>
        /// Дата, на которую заносятся данные о платежах
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Наниматель
        /// </summary>
        public string Tenant { get; set; }
        /// <summary>
        /// Общая площадь
        /// </summary>
        public double TotalArea { get; set; }
        /// <summary>
        /// Жилая площадь
        /// </summary>
        public double LivingArea { get; set; }
        /// <summary>
        /// Прописано
        /// </summary>
        public int Prescribed { get; set; }
        /// <summary>
        /// Сальдо входящее
        /// </summary>
        public decimal BalanceInput { get; set; }
        /// <summary>
        /// Сальдо вх. найм
        /// </summary>
        public decimal BalanceTenancy { get; set; }
        /// <summary>
        /// Сальдо вх. ДГИ
        /// </summary>
        public decimal BalanceDgi { get; set; }
        /// <summary>
        /// Сальдо вх. Падун
        /// </summary>
        public decimal? BalancePadun { get; set; }
        /// <summary>
        /// Сальдо вх. ПКК
        /// </summary>
        public decimal? BalancePkk { get; set; }
        /// <summary>
        /// Сальдо вх. пени
        /// </summary>
        public decimal? BalanceInputPenalties { get; set; }
        /// <summary>
        /// Начисление найм
        /// </summary>
        public decimal ChargingTenancy { get; set; }
        /// <summary>
        /// Начисление итого
        /// </summary>
        public decimal ChargingTotal { get; set; }
        /// <summary>
        /// Начисление ДГИ
        /// </summary>
        public decimal ChargingDgi { get; set; }
        /// <summary>
        /// Начисление Падун
        /// </summary>
        public decimal? ChargingPadun { get; set; }
        /// <summary>
        /// Начисление ПКК
        /// </summary>
        public decimal? ChargingPkk { get; set; }
        /// <summary>
        /// Начисление пени
        /// </summary>
        public decimal? ChargingPenalties { get; set; }
        /// <summary>
        /// Перерасчет найм
        /// </summary>
        public decimal RecalcTenancy { get; set; }
        /// <summary>
        /// Перерасчет ДГИ
        /// </summary>
        public decimal RecalcDgi { get; set; }
        /// <summary>
        /// Перерасчет Падун
        /// </summary>
        public decimal? RecalcPadun { get; set; }
        /// <summary>
        /// Перерасчет ПКК
        /// </summary>
        public decimal? RecalcPkk { get; set; }
        /// <summary>
        /// Перерасчет пени
        /// </summary>
        public decimal? RecalcPenalties { get; set; }
        /// <summary>
        /// Оплата найм
        /// </summary>
        public decimal PaymentTenancy { get; set; }
        /// <summary>
        /// Оплата ДГИ
        /// </summary>
        public decimal PaymentDgi { get; set; }
        /// <summary>
        /// Оплата Падун
        /// </summary>
        public decimal? PaymentPadun { get; set; }
        /// <summary>
        /// Оплата ПКК
        /// </summary>
        public decimal? PaymentPkk { get; set; }
        /// <summary>
        /// Оплата пени
        /// </summary>
        public decimal? PaymentPenalties { get; set; }
        /// <summary>
        /// Перенос сальдо
        /// </summary>
        public decimal TransferBalance { get; set; }
        /// <summary>
        /// Сальдо исходящее
        /// </summary>
        public decimal BalanceOutputTotal { get; set; }
        /// <summary>
        /// Сальдо исх. найм
        /// </summary>
        public decimal BalanceOutputTenancy { get; set; }
        /// <summary>
        /// Сальдо исх. ДГИ
        /// </summary>
        public decimal BalanceOutputDgi { get; set; }
        /// <summary>
        /// Сальдо исх. Падун
        /// </summary>
        public decimal? BalanceOutputPadun { get; set; }
        /// <summary>
        /// Сальдо исх. ПКК
        /// </summary>
        public decimal? BalanceOutputPkk { get; set; }
        /// <summary>
        /// Сальдо исх. пени
        /// </summary>
        public decimal? BalanceOutputPenalties { get; set; }

        public virtual PaymentAccount PaymentAccountNavigation { get; set; }
    }
}
