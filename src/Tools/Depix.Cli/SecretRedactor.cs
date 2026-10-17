using System.Text.RegularExpressions;

namespace Depix.Cli;

public static class SecretRedactor
{
    private static readonly string[] SensitiveKeywords = new[]
    {
        "password",
        "secret",
        "token",
        "key",
        "credential",
        "connectionstring",
        "pwd"
    };

    private static readonly Regex PasswordInConnectionStringRegex = new Regex(
        @"(Password|Pwd)\s*=\s*[^;]+",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string RedactValue(string key, string? value)
    {
        if (string.IsNullOrEmpty(value)) return value ?? string.Empty;

        var lowerKey = key.ToLowerInvariant();

        // Check if value is a connection string
        if (value.Contains(";") && (value.Contains("Host=") || value.Contains("Server=") || value.Contains("Data Source=")))
        {
            return PasswordInConnectionStringRegex.Replace(value, "$1=********");
        }

        // Check if key implies a secret
        if (SensitiveKeywords.Any(k => lowerKey.Contains(k)))
        {
            return "********";
        }

        return value;
    }

    public static string RedactConnectionString(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return string.Empty;
        return PasswordInConnectionStringRegex.Replace(connectionString, "$1=********");
    }
}
