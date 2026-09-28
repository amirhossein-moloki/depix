using BuildingBlocks.Domain.Models;

namespace Modules.CRM.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Text { get; private set; } = string.Empty;

    private Address() { }

    public Address(string text)
    {
        Text = text ?? string.Empty;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Text;
    }
}
