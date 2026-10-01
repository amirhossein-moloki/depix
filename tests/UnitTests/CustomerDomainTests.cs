using Xunit;
using Modules.Customer.Domain.Entities;
using Modules.Customer.Domain.Events;

namespace UnitTests;

public class CustomerDomainTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldInitializeCustomerAndRaiseEvents()
    {
        var companyId = Guid.NewGuid();
        var customerNumber = "CUST-2026-0001";
        var primaryContactId = Guid.NewGuid();
        var assignedTo = Guid.NewGuid();

        var customer = Customer.Create(
            companyId,
            customerNumber,
            primaryContactId: primaryContactId,
            assignedTo: assignedTo,
            notes: "Initial customer notes");

        Assert.NotEqual(Guid.Empty, customer.Id);
        Assert.Equal(companyId, customer.CompanyId);
        Assert.Equal("CUST-2026-0001", customer.CustomerNumber);
        Assert.Equal("ACTIVE", customer.Status);
        Assert.Equal(primaryContactId, customer.PrimaryContactId);
        Assert.Equal(assignedTo, customer.AssignedTo);
        Assert.Equal("Initial customer notes", customer.Notes);
        Assert.False(customer.IsDeleted);

        Assert.Contains(customer.DomainEvents, e => e is CustomerCreatedEvent c && c.CustomerId == customer.Id);
        Assert.Contains(customer.DomainEvents, e => e is CustomerAssignedEvent a && a.AssignedTo == assignedTo);
    }

    [Fact]
    public void Create_WithEmptyCompanyId_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Customer.Create(Guid.Empty, "CUST-001"));
    }

    [Fact]
    public void Create_WithEmptyCustomerNumber_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Customer.Create(Guid.NewGuid(), "   "));
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateFieldsAndRaiseUpdatedEvent()
    {
        var customer = Customer.Create(Guid.NewGuid(), "CUST-001");
        customer.ClearDomainEvents();

        var newContactId = Guid.NewGuid();
        customer.UpdateDetails(newContactId, "Updated notes");

        Assert.Equal(newContactId, customer.PrimaryContactId);
        Assert.Equal("Updated notes", customer.Notes);
        Assert.Contains(customer.DomainEvents, e => e is CustomerPrimaryContactChangedEvent p && p.PrimaryContactId == newContactId);
        Assert.Contains(customer.DomainEvents, e => e is CustomerUpdatedEvent u && u.CustomerId == customer.Id);
    }

    [Fact]
    public void SetPrimaryContact_WhenContactChanged_ShouldUpdateAndRaiseEvents()
    {
        var customer = Customer.Create(Guid.NewGuid(), "CUST-001");
        customer.ClearDomainEvents();

        var newContactId = Guid.NewGuid();
        customer.SetPrimaryContact(newContactId);

        Assert.Equal(newContactId, customer.PrimaryContactId);
        Assert.Contains(customer.DomainEvents, e => e is CustomerPrimaryContactChangedEvent p && p.PrimaryContactId == newContactId);
    }

    [Fact]
    public void Assign_ShouldUpdateAssignedToAndRaiseAssignedEvent()
    {
        var customer = Customer.Create(Guid.NewGuid(), "CUST-001");
        customer.ClearDomainEvents();

        var newAccountManager = Guid.NewGuid();
        customer.Assign(newAccountManager);

        Assert.Equal(newAccountManager, customer.AssignedTo);
        Assert.Contains(customer.DomainEvents, e => e is CustomerAssignedEvent a && a.AssignedTo == newAccountManager);
    }

    [Fact]
    public void Activate_Deactivate_Suspend_ShouldUpdateStatusAndRaiseStatusChangedEvent()
    {
        var customer = Customer.Create(Guid.NewGuid(), "CUST-001");
        customer.ClearDomainEvents();

        customer.Deactivate();
        Assert.Equal("INACTIVE", customer.Status);
        Assert.Contains(customer.DomainEvents, e => e is CustomerStatusChangedEvent s && s.NewStatus == "INACTIVE");

        customer.ClearDomainEvents();
        customer.Suspend();
        Assert.Equal("SUSPENDED", customer.Status);
        Assert.Contains(customer.DomainEvents, e => e is CustomerStatusChangedEvent s && s.NewStatus == "SUSPENDED");

        customer.ClearDomainEvents();
        customer.Activate();
        Assert.Equal("ACTIVE", customer.Status);
        Assert.Contains(customer.DomainEvents, e => e is CustomerStatusChangedEvent s && s.NewStatus == "ACTIVE");
    }

    [Fact]
    public void UpdateStatus_WithInvalidStatus_ShouldThrowArgumentException()
    {
        var customer = Customer.Create(Guid.NewGuid(), "CUST-001");
        Assert.Throws<ArgumentException>(() => customer.UpdateStatus("INVALID_STATUS"));
    }

    [Fact]
    public void Archive_ShouldSetSoftDeleteAndArchivedStatus()
    {
        var customer = Customer.Create(Guid.NewGuid(), "CUST-001");
        customer.ClearDomainEvents();

        var archivedBy = Guid.NewGuid();
        customer.Archive(archivedBy);

        Assert.True(customer.IsDeleted);
        Assert.Equal("ARCHIVED", customer.Status);
        Assert.Equal(archivedBy, customer.DeletedBy);
        Assert.NotNull(customer.DeletedAt);
        Assert.Contains(customer.DomainEvents, e => e is CustomerArchivedEvent a && a.ArchivedBy == archivedBy);
        Assert.Contains(customer.DomainEvents, e => e is CustomerStatusChangedEvent s && s.NewStatus == "ARCHIVED");
    }

    [Fact]
    public void Reactivate_ShouldResetSoftDeleteAndSetActiveStatus()
    {
        var customer = Customer.Create(Guid.NewGuid(), "CUST-001");
        customer.Archive();
        customer.ClearDomainEvents();

        customer.Reactivate();

        Assert.False(customer.IsDeleted);
        Assert.Equal("ACTIVE", customer.Status);
        Assert.Null(customer.DeletedBy);
        Assert.Null(customer.DeletedAt);
        Assert.Contains(customer.DomainEvents, e => e is CustomerReactivatedEvent);
        Assert.Contains(customer.DomainEvents, e => e is CustomerStatusChangedEvent s && s.NewStatus == "ACTIVE");
    }
}
