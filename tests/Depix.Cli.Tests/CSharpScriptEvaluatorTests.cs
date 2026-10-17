using Depix.Cli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Depix.Cli.Tests;

public class CSharpScriptEvaluatorTests
{
    [Fact]
    public void IsDisabledInCurrentEnvironment_WhenProductionAndNotAllowed_ReturnsTrue()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ASPNETCORE_ENVIRONMENT", "Production" },
            { "ConnectionStrings:Database", "Host=localhost;Database=db;Username=usr;Password=pwd" }
        };

        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();
        var env = new CliHostEnvironment("Production", "Depix.Cli", Directory.GetCurrentDirectory());

        var (services, _, _) = ShellBootstrapper.BuildServiceProvider(existingConfig: config);
        var evaluator = new CSharpScriptEvaluator(services, config, env);

        var isDisabled = evaluator.IsDisabledInCurrentEnvironment(out var reason);

        Assert.True(isDisabled);
        Assert.NotNull(reason);
        Assert.Contains("disabled in Production", reason);
    }

    [Fact]
    public void IsDisabledInCurrentEnvironment_WhenProductionAndAllowed_ReturnsFalse()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ASPNETCORE_ENVIRONMENT", "Production" },
            { "DEPIX_ALLOW_PROD_SCRIPTING", "true" },
            { "ConnectionStrings:Database", "Host=localhost;Database=db;Username=usr;Password=pwd" }
        };

        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();
        var env = new CliHostEnvironment("Production", "Depix.Cli", Directory.GetCurrentDirectory());

        var (services, _, _) = ShellBootstrapper.BuildServiceProvider(existingConfig: config);
        var evaluator = new CSharpScriptEvaluator(services, config, env);

        var isDisabled = evaluator.IsDisabledInCurrentEnvironment(out var reason);

        Assert.False(isDisabled);
        Assert.Null(reason);
    }

    [Fact]
    public async Task EvaluateAsync_InDevelopment_EvaluatesExpressionCorrectly()
    {
        var config = new ConfigurationBuilder().Build();
        var env = new CliHostEnvironment("Development", "Depix.Cli", Directory.GetCurrentDirectory());

        var (services, _, _) = ShellBootstrapper.BuildServiceProvider(existingConfig: config);
        var evaluator = new CSharpScriptEvaluator(services, config, env);

        using var scope = services.CreateScope();
        var result = await evaluator.EvaluateAsync("2 + 3", scope.ServiceProvider);

        Assert.Equal(5, result);
    }
}
