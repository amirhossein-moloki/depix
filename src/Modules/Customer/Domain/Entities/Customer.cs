using BuildingBlocks.Domain.Models;

namespace Modules.Customer.Domain.Entities;

public class Customer : AuditableAggregateRoot, ISoftDelete
{
    public Guid CompanyId { get; private set; }
    public DateOnly CustomerSince { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private Customer() { }

    public Customer(Guid id, Guid companyId, DateOnly customerSince) : base(id)
    {
        CompanyId = companyId;
        CustomerSince = customerSince;
    }

    public static Customer Create(Guid companyId, DateOnly customerSince)
    {
        return new Customer(Guid.NewGuid(), companyId, customerSince);
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
