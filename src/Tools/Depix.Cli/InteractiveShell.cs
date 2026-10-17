using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Depix.Cli;

public class InteractiveShell
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private readonly CommandExecutor _executor;
    private readonly CSharpScriptEvaluator _scriptEvaluator;

    public InteractiveShell(IServiceProvider serviceProvider, IConfiguration configuration, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _environment = environment;
        _executor = new CommandExecutor(_serviceProvider, _configuration, _environment);
        _scriptEvaluator = new CSharpScriptEvaluator(_serviceProvider, _configuration, _environment);
    }

    public async Task RunAsync(TextReader inputReader, TextWriter outputWriter, CancellationToken cancellationToken = default)
    {
        outputWriter.WriteLine("==========================================================================");
        outputWriter.WriteLine(" Depix Interactive Shell (ASP.NET Core / EF Core Modular Monolith)");
        outputWriter.WriteLine($" Environment: {_environment.EnvironmentName}");
        outputWriter.WriteLine(" Type 'help' for available commands, 'csharp' for REPL, or 'exit' to quit.");
        outputWriter.WriteLine("==========================================================================");
        outputWriter.WriteLine();

        bool inCSharpMode = false;

        while (!cancellationToken.IsCancellationRequested)
        {
            var prompt = inCSharpMode ? "depix (csharp)> " : "depix> ";
            outputWriter.Write(prompt);
            outputWriter.Flush();

            string? line = await inputReader.ReadLineAsync(cancellationToken);
            if (line == null)
            {
                break; // EOF
            }

            line = line.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (inCSharpMode)
            {
                if (string.Equals(line, "exit", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(line, "quit", StringComparison.OrdinalIgnoreCase))
                {
                    inCSharpMode = false;
                    outputWriter.WriteLine("Exited C# script mode.");
                    continue;
                }

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var result = await _scriptEvaluator.EvaluateAsync(line, scope.ServiceProvider, cancellationToken);
                    if (result != null)
                    {
                        outputWriter.WriteLine(result);
                    }
                }
                catch (Exception ex)
                {
                    outputWriter.WriteLine($"[Script Error] {ex.Message}");
                }
                continue;
            }

            var parsed = CommandParser.Parse(line);

            if (string.Equals(parsed.CommandName, "exit", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(parsed.CommandName, "quit", StringComparison.OrdinalIgnoreCase))
            {
                outputWriter.WriteLine("Goodbye.");
                break;
            }

            if (string.Equals(parsed.CommandName, "csharp", StringComparison.OrdinalIgnoreCase))
            {
                if (_scriptEvaluator.IsDisabledInCurrentEnvironment(out string? reason))
                {
                    outputWriter.WriteLine($"[Security Notice] {reason}");
                    continue;
                }

                inCSharpMode = true;
                outputWriter.WriteLine("Entering C# script mode. Multi-statement variables and LINQ are supported.");
                outputWriter.WriteLine("Type 'exit' to leave C# script mode.");
                continue;
            }

            if (string.Equals(parsed.CommandName, "eval", StringComparison.OrdinalIgnoreCase))
            {
                var code = string.Join(" ", parsed.Arguments);
                if (string.IsNullOrWhiteSpace(code))
                {
                    outputWriter.WriteLine("Usage: eval <csharp_expression>");
                    continue;
                }

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var result = await _scriptEvaluator.EvaluateAsync(code, scope.ServiceProvider, cancellationToken);
                    if (result != null)
                    {
                        outputWriter.WriteLine(result);
                    }
                }
                catch (Exception ex)
                {
                    outputWriter.WriteLine($"[Script Error] {ex.Message}");
                }
                continue;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                await _executor.ExecuteAsync(parsed, scope.ServiceProvider, outputWriter, cancellationToken);
            }
            catch (Exception ex)
            {
                outputWriter.WriteLine($"[Command Error] {ex.Message}");
            }
        }
    }
}
