namespace UnifiedEconomy
{
    using Exiled.API.Interfaces;

    public class UETranslation : ITranslation
    {
        public string BalanceCommandResult { get; set; } = "Your balance is %money%";
    }
}
