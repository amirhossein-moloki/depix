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

    public Company(
        Guid id,
        string name,
        string industry,
        string website,
        string phone,
        string email,
        Address address,
        string type = "LEAD",
        double? latitude = null,
        double? longitude = null) : base(id)
    {
        Name = name;
        Industry = industry;
        Website = website;
        Phone = phone;
        Email = email;
        Address = address;
        Type = type;
        SetLocation(latitude, longitude);
    }

    public static Company Create(
        string name,
        string industry,
        string website,
        string phone,
        string email,
        Address address,
        string type = "LEAD",
        double? latitude = null,
        double? longitude = null)
    {
        return new Company(Guid.NewGuid(), name, industry, website, phone, email, address, type, latitude, longitude);
    }

    public void SetLocation(double? latitude, double? longitude)
    {
        if (latitude.HasValue != longitude.HasValue)
        {
            throw new ArgumentException("Both latitude and longitude must be provided together, or both must be null.");
        }

        if (latitude.HasValue)
        {
            if (latitude.Value < -90.0 || latitude.Value > 90.0)
            {
                throw new ArgumentOutOfRangeException(nameof(latitude), "Latitude must be between -90 and 90 degrees.");
            }
            if (longitude!.Value < -180.0 || longitude.Value > 180.0)
            {
                throw new ArgumentOutOfRangeException(nameof(longitude), "Longitude must be between -180 and 180 degrees.");
            }
        }

        Latitude = latitude;
        Longitude = longitude;
        UpdateTimestamp(DateTime.UtcNow);
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

    public void UpdateInfo(
        string name,
        string industry,
        string website,
        string phone,
        string email,
        Address address,
        string type,
        double? latitude = null,
        double? longitude = null)
    {
        Name = name;
        Industry = industry;
        Website = website;
        Phone = phone;
        Email = email;
        Address = address;
        Type = type;
        SetLocation(latitude, longitude);
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
