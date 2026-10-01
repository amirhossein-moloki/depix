namespace Modules.Finance.Domain.Constants;

public static class PaymentMethod
{
    public const string Cash = "CASH";
    public const string BankTransfer = "BANK_TRANSFER";
    public const string Card = "CARD";
    public const string Online = "ONLINE";
    public const string Other = "OTHER";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        Cash,
        BankTransfer,
        Card,
        Online,
        Other
    };
}
