using Depix.Cli;
using Xunit;

namespace Depix.Cli.Tests;

public class SecretRedactorTests
{
    [Theory]
    [InlineData("ConnectionStrings:Database", "Host=postgres;Port=5432;Database=crm_erp_db;Username=postgres;Password=mysecretpass", "Host=postgres;Port=5432;Database=crm_erp_db;Username=postgres;Password=********")]
    [InlineData("Jwt:Secret", "SuperSecretJwtKeyThatIs32BytesLong!", "********")]
    [InlineData("Database:Password", "P@ssw0rd123!", "********")]
    [InlineData("ApiKey", "1234567890abcdef", "********")]
    public void RedactValue_SensitiveKeys_MasksSecrets(string key, string originalValue, string expectedSubstring)
    {
        var redacted = SecretRedactor.RedactValue(key, originalValue);
        Assert.Contains(expectedSubstring, redacted);
        if (!key.StartsWith("ConnectionStrings"))
        {
            Assert.DoesNotContain(originalValue, redacted);
        }
    }

    [Theory]
    [InlineData("ASPNETCORE_ENVIRONMENT", "Development", "Development")]
    [InlineData("Jwt:Issuer", "CrmErpApi", "CrmErpApi")]
    [InlineData("Cors:AllowedOrigins:0", "http://localhost:3000", "http://localhost:3000")]
    public void RedactValue_NonSensitiveKeys_LeavesValuesIntact(string key, string originalValue, string expectedValue)
    {
        var redacted = SecretRedactor.RedactValue(key, originalValue);
        Assert.Equal(expectedValue, redacted);
    }
}
