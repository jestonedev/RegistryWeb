namespace RegistryServices.ViewModel.KumiAccounts
{
    public interface IChargePaymentEventVM: IChargeEventVM
    {
        decimal TenancyTail { get; set; }
        decimal PenaltyTail { get; set; }
    }
}