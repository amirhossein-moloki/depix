using BuildingBlocks.Domain.Models;
using Modules.CRM.Domain.ValueObjects;

namespace Modules.CRM.Domain.Entities;

public class Company : AuditableAggregateRoot, ISoftDelete
{
    private readonly List<Contact> _contacts = new();
    private readonly List<Lead> _leads = new();

    public string Name { get; private set; } = string.Empty;
    public string Industry { get; private set; } = string.Empty;
    public string Website { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public Address Address { get; private set; } = new Address(string.Empty);
    public string Type { get; private set; } = "LEAD"; // LEAD, CUSTOMER, INACTIVE
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public IReadOnlyCollection<Contact> Contacts => _contacts.AsReadOnly();
    public IReadOnlyCollection<Lead> Leads => _leads.AsReadOnly();

    private Company() { }

    public Company(Guid id, string name, string industry, string website, string phone, string email, Address address, string type = "LEAD", double? latitude = null, double? longitude = null) : base(id)
    {
        Name = name;
        Industry = industry;
        Website = website;
        Phone = phone;
        Email = email;
        Address = address;
        Type = type;
        if (latitude.HasValue || longitude.HasValue) SetLocation(latitude, longitude);
    }

    public static Company Create(string name, string industry, string website, string phone, string email, Address address, string type = "LEAD", double? latitude = null, double? longitude = null)
    {
        return new Company(Guid.NewGuid(), name, industry, website, phone, email, address, type, latitude, longitude);
    }

    public void MarkAsCustomer()
    {
        Type = "CUSTOMER";
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void MarkAsInactive()
    {
        Type = "INACTIVE";
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void UpdateInfo(string name, string industry, string website, string phone, string email, Address address, string type, double? latitude = null, double? longitude = null)
    {
        Name = name;
        Industry = industry;
        Website = website;
        Phone = phone;
        Email = email;
        Address = address;
        Type = type;
        if (latitude.HasValue || longitude.HasValue)
            SetLocation(latitude, longitude);
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void SetLocation(double? latitude, double? longitude)
    {
        if (latitude.HasValue != longitude.HasValue)
            throw new ArgumentException("Latitude and longitude must be provided together.");
        if (latitude is < -90 or > 90) throw new ArgumentOutOfRangeException(nameof(latitude));
        if (longitude is < -180 or > 180) throw new ArgumentOutOfRangeException(nameof(longitude));
        Latitude = latitude;
        Longitude = longitude;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void AddContact(Contact contact)
    {
        _contacts.Add(contact);
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
