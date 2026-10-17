using Microsoft.Extensions.DependencyInjection;

namespace Depix.Cli;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cts.Cancel();
        };

        try
        {
            var (services, config, env) = ShellBootstrapper.BuildServiceProvider(args);

            if (args.Length > 0 && !args[0].StartsWith("-"))
            {
                // Non-interactive command execution mode
                var commandLine = string.Join(" ", args);
                var parsed = CommandParser.Parse(commandLine);

                using var scope = services.CreateScope();
                var executor = new CommandExecutor(services, config, env);

                await executor.ExecuteAsync(parsed, scope.ServiceProvider, Console.Out, cts.Token);
                return 0;
            }

            // Interactive Shell REPL mode
            var shell = new InteractiveShell(services, config, env);
            await shell.RunAsync(Console.In, Console.Out, cts.Token);
            return 0;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\nOperation cancelled.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[Fatal Error] {ex.Message}");
            return 1;
        }
    }
}
