using Depix.Cli;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Depix.Cli.Tests;

public class InteractiveShellTests
{
    [Fact]
    public async Task RunAsync_ExitCommand_TerminatesCleanly()
    {
        var config = new ConfigurationBuilder().Build();
        var (services, _, env) = ShellBootstrapper.BuildServiceProvider(existingConfig: config);
        var shell = new InteractiveShell(services, config, env);

        var reader = new StringReader("exit\n");
        var writer = new StringWriter();

        await shell.RunAsync(reader, writer);

        var output = writer.ToString();
        Assert.Contains("Goodbye", output);
    }

    [Fact]
    public async Task RunAsync_ExecuteHelpThenExit_RunsCommandsSequence()
    {
        var config = new ConfigurationBuilder().Build();
        var (services, _, env) = ShellBootstrapper.BuildServiceProvider(existingConfig: config);
        var shell = new InteractiveShell(services, config, env);

        var reader = new StringReader("help\nexit\n");
        var writer = new StringWriter();

        await shell.RunAsync(reader, writer);

        var output = writer.ToString();
        Assert.Contains("Display available commands", output);
        Assert.Contains("Goodbye", output);
    }
}
