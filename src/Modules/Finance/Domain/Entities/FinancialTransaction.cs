using BuildingBlocks.Domain.Models;

namespace Modules.Finance.Domain.Entities;

public class FinancialTransaction : Entity
{
    public Guid CustomerId { get; private set; }
    public Guid? ContractId { get; private set; }
    public decimal Amount { get; private set; }
    public string Type { get; private set; } = string.Empty; // PAYMENT, REFUND, INVOICE
    public string Status { get; private set; } = string.Empty; // SUCCESS, FAILED, PENDING
    public DateOnly TransactionDate { get; private set; }

    private FinancialTransaction() { }

    public FinancialTransaction(Guid id, Guid customerId, Guid? contractId, decimal amount, string type, string status, DateOnly transactionDate) : base(id)
    {
        CustomerId = customerId;
        ContractId = contractId;
        Amount = amount;
        Type = type;
        Status = status;
        TransactionDate = transactionDate;
    }

    public static FinancialTransaction Create(Guid customerId, Guid? contractId, decimal amount, string type, string status, DateOnly transactionDate)
    {
        return new FinancialTransaction(Guid.NewGuid(), customerId, contractId, amount, type, status, transactionDate);
    }
}
