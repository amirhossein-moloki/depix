using BuildingBlocks.Domain.Models;

namespace Modules.Finance.Domain.Entities;

public class ContractPayment : Entity
{
    public Guid ContractId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public DateOnly DueDate { get; private set; }
    public string Status { get; private set; } = string.Empty; // PENDING, PAID, OVERDUE, CANCELLED

    private ContractPayment() { }

    public ContractPayment(Guid id, Guid contractId, string title, decimal amount, DateOnly dueDate, string status) : base(id)
    {
        ContractId = contractId;
        Title = title;
        Amount = amount;
        DueDate = dueDate;
        Status = status;
    }

    public static ContractPayment Create(Guid contractId, string title, decimal amount, DateOnly dueDate, string status = "PENDING")
    {
        return new ContractPayment(Guid.NewGuid(), contractId, title, amount, dueDate, status);
    }

    public void MarkAsPaid()
    {
        Status = "PAID";
    }
}
