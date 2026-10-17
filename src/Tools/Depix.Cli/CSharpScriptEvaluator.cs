using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Depix.Cli;

public class DepixShellGlobals
{
    public ApplicationDbContext dbContext { get; set; } = default!;
    public IServiceProvider services { get; set; } = default!;
    public IConfiguration configuration { get; set; } = default!;
    public IHostEnvironment environment { get; set; } = default!;
}

public class CSharpScriptEvaluator
{
    private readonly IServiceProvider _rootServiceProvider;
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;
    private ScriptState<object>? _scriptState;
    private DepixShellGlobals? _globals;

    public CSharpScriptEvaluator(IServiceProvider rootServiceProvider, IConfiguration configuration, IHostEnvironment environment)
    {
        _rootServiceProvider = rootServiceProvider;
        _configuration = configuration;
        _environment = environment;
    }

    public bool IsDisabledInCurrentEnvironment(out string? reason)
    {
        var isProd = _environment.IsProduction() ||
                     string.Equals(_configuration["ASPNETCORE_ENVIRONMENT"], "Production", StringComparison.OrdinalIgnoreCase);

        var allowProdScripting = string.Equals(_configuration["DEPIX_ALLOW_PROD_SCRIPTING"], "true", StringComparison.OrdinalIgnoreCase);

        if (isProd && !allowProdScripting)
        {
            reason = "C# interactive scripting is disabled in Production by default. Set DEPIX_ALLOW_PROD_SCRIPTING=true to override if explicitly authorized.";
            return true;
        }

        reason = null;
        return false;
    }

    public async Task<object?> EvaluateAsync(string code, IServiceProvider scopedServices, CancellationToken cancellationToken = default)
    {
        if (IsDisabledInCurrentEnvironment(out string? reason))
        {
            throw new InvalidOperationException(reason);
        }

        var dbContext = scopedServices.GetRequiredService<ApplicationDbContext>();

        if (_globals == null)
        {
            _globals = new DepixShellGlobals
            {
                dbContext = dbContext,
                services = scopedServices,
                configuration = _configuration,
                environment = _environment
            };
        }
        else
        {
            _globals.dbContext = dbContext;
            _globals.services = scopedServices;
        }

        var options = ScriptOptions.Default
            .WithImports(
                "System",
                "System.IO",
                "System.Linq",
                "System.Text",
                "System.Threading.Tasks",
                "System.Collections.Generic",
                "Microsoft.EntityFrameworkCore",
                "BuildingBlocks.Infrastructure.Persistence",
                "BuildingBlocks.Domain.Models",
                "Modules.Identity.Domain.Entities",
                "Modules.CRM.Domain.Entities",
                "Modules.Sales.Domain.Entities",
                "Modules.Customer.Domain.Entities",
                "Modules.Project.Domain.Entities",
                "Modules.Finance.Domain.Entities",
                "Modules.Support.Domain.Entities",
                "Modules.Platform.Domain.Entities"
            )
            .WithReferences(
                typeof(object).Assembly,
                typeof(Enumerable).Assembly,
                typeof(DbContext).Assembly,
                typeof(ApplicationDbContext).Assembly,
                typeof(Modules.Identity.Domain.Entities.User).Assembly,
                typeof(Modules.CRM.Domain.Entities.Lead).Assembly,
                typeof(Modules.Sales.Domain.Entities.Opportunity).Assembly,
                typeof(Modules.Customer.Domain.Entities.Customer).Assembly,
                typeof(Modules.Project.Domain.Entities.Project).Assembly,
                typeof(Modules.Finance.Domain.Entities.Invoice).Assembly,
                typeof(Modules.Support.Domain.Entities.Ticket).Assembly,
                typeof(Modules.Platform.Domain.Entities.AuditLog).Assembly,
                typeof(Modules.Reporting.Domain.AssemblyReference).Assembly
            );

        if (_scriptState == null)
        {
            _scriptState = await CSharpScript.RunAsync(code, options, _globals, typeof(DepixShellGlobals), cancellationToken);
        }
        else
        {
            // Update globals instance for current evaluation scope
            _scriptState = await _scriptState.ContinueWithAsync(code, options, cancellationToken: cancellationToken);
        }

        return _scriptState.ReturnValue;
    }

    public void ResetState()
    {
        _scriptState = null;
        _globals = null;
    }
}
