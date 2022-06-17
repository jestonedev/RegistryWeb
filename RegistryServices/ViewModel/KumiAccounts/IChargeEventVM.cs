using System;

namespace RegistryServices.ViewModel.KumiAccounts
{
    public interface IChargeEventVM
    {
        DateTime Date { get; set; } // Дата события: платежа или расчета пени
        decimal Tenancy { get; set; }
        decimal Penalty { get; set; }
    }
}