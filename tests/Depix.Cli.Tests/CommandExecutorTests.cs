using Depix.Cli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Depix.Cli.Tests;

public class CommandExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_HelpCommand_PrintsHelpTable()
    {
        var config = new ConfigurationBuilder().Build();
        var (services, _, env) = ShellBootstrapper.BuildServiceProvider(existingConfig: config);
        var executor = new CommandExecutor(services, config, env);

        var writer = new StringWriter();
        var parsed = CommandParser.Parse("help");

        using var scope = services.CreateScope();
        await executor.ExecuteAsync(parsed, scope.ServiceProvider, writer);

        var output = writer.ToString();
        Assert.Contains("Display available commands", output);
        Assert.Contains("config show", output);
    }

    [Fact]
    public async Task ExecuteAsync_VersionCommand_PrintsVersionDetails()
    {
        var config = new ConfigurationBuilder().Build();
        var (services, _, env) = ShellBootstrapper.BuildServiceProvider(existingConfig: config);
        var executor = new CommandExecutor(services, config, env);

        var writer = new StringWriter();
        var parsed = CommandParser.Parse("version");

        using var scope = services.CreateScope();
        await executor.ExecuteAsync(parsed, scope.ServiceProvider, writer);

        var output = writer.ToString();
        Assert.Contains("Depix CRM/ERP Modular Monolith", output);
    }

    [Fact]
    public async Task ExecuteAsync_ConfigShowCommand_MasksSensitiveSettings()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Secret", "SuperSecretKey12345678901234567890" },
            { "ConnectionStrings:Database", "Host=localhost;Database=db;Username=usr;Password=secretpass" },
            { "ASPNETCORE_ENVIRONMENT", "Development" }
        };

        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();
        var (services, _, env) = ShellBootstrapper.BuildServiceProvider(existingConfig: config);
        var executor = new CommandExecutor(services, config, env);

        var writer = new StringWriter();
        var parsed = CommandParser.Parse("config show");

        using var scope = services.CreateScope();
        await executor.ExecuteAsync(parsed, scope.ServiceProvider, writer);

        var output = writer.ToString();
        Assert.DoesNotContain("SuperSecretKey12345678901234567890", output);
        Assert.DoesNotContain("secretpass", output);
        Assert.Contains("********", output);
        Assert.Contains("Development", output);
    }
}
