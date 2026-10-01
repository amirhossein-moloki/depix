using BuildingBlocks.Domain.Models;
using Modules.Customer.Domain.Events;

namespace Modules.Customer.Domain.Entities;

public class Customer : AuditableAggregateRoot, ISoftDelete
{
    public Guid CompanyId { get; private set; }
    public string CustomerNumber { get; private set; } = string.Empty;
    public DateOnly CustomerSince { get; private set; }
    public string Status { get; private set; } = "ACTIVE";
    public Guid? PrimaryContactId { get; private set; }
    public Guid? AssignedTo { get; private set; }
    public string? Notes { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    private Customer() { }

    public Customer(
        Guid id,
        Guid companyId,
        string customerNumber,
        DateOnly customerSince,
        string status = "ACTIVE",
        Guid? primaryContactId = null,
        Guid? assignedTo = null,
        string? notes = null) : base(id)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("Company ID cannot be empty.", nameof(companyId));

        if (string.IsNullOrWhiteSpace(customerNumber))
            throw new ArgumentException("Customer number is required.", nameof(customerNumber));

        CompanyId = companyId;
        CustomerNumber = customerNumber.Trim();
        CustomerSince = customerSince;
        Status = string.IsNullOrWhiteSpace(status) ? "ACTIVE" : status.ToUpperInvariant();
        PrimaryContactId = primaryContactId;
        AssignedTo = assignedTo;
        Notes = notes;
    }

    public static Customer Create(
        Guid companyId,
        string customerNumber,
        DateOnly? customerSince = null,
        Guid? primaryContactId = null,
        Guid? assignedTo = null,
        string? notes = null)
    {
        var since = customerSince ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var customer = new Customer(
            Guid.NewGuid(),
            companyId,
            customerNumber,
            since,
            "ACTIVE",
            primaryContactId,
            assignedTo,
            notes);

        customer.AddDomainEvent(new CustomerCreatedEvent(customer.Id, customer.CompanyId, customer.CustomerNumber));
        if (assignedTo.HasValue)
        {
            customer.AddDomainEvent(new CustomerAssignedEvent(customer.Id, assignedTo));
        }

        return customer;
    }

    public void UpdateDetails(
        Guid? primaryContactId,
        string? notes)
    {
        if (PrimaryContactId != primaryContactId)
        {
            SetPrimaryContact(primaryContactId);
        }

        Notes = notes;
        AddDomainEvent(new CustomerUpdatedEvent(Id));
    }

    public void SetPrimaryContact(Guid? contactId)
    {
        if (PrimaryContactId == contactId) return;

        PrimaryContactId = contactId;
        AddDomainEvent(new CustomerPrimaryContactChangedEvent(Id, contactId));
        AddDomainEvent(new CustomerUpdatedEvent(Id));
    }

    public void Assign(Guid? assignedTo)
    {
        AssignedTo = assignedTo;
        AddDomainEvent(new CustomerAssignedEvent(Id, assignedTo));
    }

    public void Activate()
    {
        UpdateStatus("ACTIVE");
    }

    public void Deactivate()
    {
        UpdateStatus("INACTIVE");
    }

    public void Suspend()
    {
        UpdateStatus("SUSPENDED");
    }

    public void UpdateStatus(string newStatus)
    {
        if (string.IsNullOrWhiteSpace(newStatus))
            throw new ArgumentException("Status cannot be empty.", nameof(newStatus));

        var normalized = newStatus.Trim().ToUpperInvariant();
        if (normalized != "ACTIVE" && normalized != "INACTIVE" && normalized != "SUSPENDED" && normalized != "ARCHIVED")
            throw new ArgumentException($"Invalid status '{newStatus}'. Allowed values are ACTIVE, INACTIVE, SUSPENDED, ARCHIVED.", nameof(newStatus));

        if (Status == normalized) return;

        var previousStatus = Status;
        Status = normalized;

        if (normalized == "ARCHIVED")
        {
            SoftDelete(null);
        }
        else if (IsDeleted)
        {
            UndoSoftDelete();
        }

        AddDomainEvent(new CustomerStatusChangedEvent(Id, previousStatus, normalized));
        AddDomainEvent(new CustomerUpdatedEvent(Id));
    }

    public void Archive(Guid? archivedBy = null)
    {
        if (IsDeleted && Status == "ARCHIVED") return;

        var previousStatus = Status;
        Status = "ARCHIVED";
        SoftDelete(archivedBy);
        AddDomainEvent(new CustomerStatusChangedEvent(Id, previousStatus, "ARCHIVED"));
        AddDomainEvent(new CustomerArchivedEvent(Id, archivedBy));
    }

    public void Reactivate()
    {
        if (!IsDeleted && Status == "ACTIVE") return;

        var previousStatus = Status;
        UndoSoftDelete();
        Status = "ACTIVE";
        AddDomainEvent(new CustomerStatusChangedEvent(Id, previousStatus, "ACTIVE"));
        AddDomainEvent(new CustomerReactivatedEvent(Id));
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
