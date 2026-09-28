using BuildingBlocks.Domain.Models;

namespace Modules.Finance.Domain.Entities;

public class Contract : AuditableAggregateRoot, ISoftDelete
{
    private readonly List<ContractPayment> _contractPayments = new();

    public Guid CustomerId { get; private set; }
    public Guid? ProjectId { get; private set; }
    public string ContractNumber { get; private set; } = string.Empty;
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string Status { get; private set; } = string.Empty;

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public IReadOnlyCollection<ContractPayment> ContractPayments => _contractPayments.AsReadOnly();

    private Contract() { }

    public Contract(Guid id, Guid customerId, Guid? projectId, string contractNumber, DateOnly startDate, DateOnly endDate, decimal totalAmount, string status) : base(id)
    {
        CustomerId = customerId;
        ProjectId = projectId;
        ContractNumber = contractNumber;
        StartDate = startDate;
        EndDate = endDate;
        TotalAmount = totalAmount;
        Status = status;
    }

    public static Contract Create(Guid customerId, Guid? projectId, string contractNumber, DateOnly startDate, DateOnly endDate, decimal totalAmount, string status = "DRAFT")
    {
        return new Contract(Guid.NewGuid(), customerId, projectId, contractNumber, startDate, endDate, totalAmount, status);
    }

    public void AddPaymentSchedule(ContractPayment payment)
    {
        _contractPayments.Add(payment);
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void UpdateStatus(string status)
    {
        Status = status;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void SoftDelete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    public void UndoSoftDelete()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
