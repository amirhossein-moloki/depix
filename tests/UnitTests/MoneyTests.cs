using BuildingBlocks.Domain.ValueObjects;
using Xunit;

namespace UnitTests;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmountAndCurrency_ShouldCreateMoneyInstance()
    {
        var money = Money.Create(100.50m, "USD");

        Assert.Equal(100.50m, money.Amount);
        Assert.Equal("USD", money.Currency);
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => Money.Create(-10m, "USD"));
    }

    [Fact]
    public void Add_WithSameCurrency_ShouldReturnSum()
    {
        var m1 = Money.Create(50m, "USD");
        var m2 = Money.Create(25m, "USD");

        var result = m1 + m2;

        Assert.Equal(75m, result.Amount);
        Assert.Equal("USD", result.Currency);
    }

    [Fact]
    public void Add_WithDifferentCurrency_ShouldThrowInvalidOperationException()
    {
        var m1 = Money.Create(50m, "USD");
        var m2 = Money.Create(25m, "EUR");

        Assert.Throws<InvalidOperationException>(() => m1 + m2);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldBeEqual()
    {
        var m1 = Money.Create(100m, "USD");
        var m2 = Money.Create(100m, "USD");

        Assert.Equal(m1, m2);
    }
}
