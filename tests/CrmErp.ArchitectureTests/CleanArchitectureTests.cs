using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace CrmErp.ArchitectureTests;

public class CleanArchitectureTests
{
    private static readonly (string ModuleName, Assembly Domain, Assembly Application, Assembly Infrastructure, Assembly Api)[] ModuleAssemblies =
    new[]
    {
        ("Identity", Modules.Identity.Domain.AssemblyReference.Assembly, Modules.Identity.Application.AssemblyReference.Assembly, Modules.Identity.Infrastructure.AssemblyReference.Assembly, Modules.Identity.API.AssemblyReference.Assembly),
        ("CRM", Modules.CRM.Domain.AssemblyReference.Assembly, Modules.CRM.Application.AssemblyReference.Assembly, Modules.CRM.Infrastructure.AssemblyReference.Assembly, Modules.CRM.API.AssemblyReference.Assembly),
        ("Sales", Modules.Sales.Domain.AssemblyReference.Assembly, Modules.Sales.Application.AssemblyReference.Assembly, Modules.Sales.Infrastructure.AssemblyReference.Assembly, Modules.Sales.API.AssemblyReference.Assembly),
        ("Customer", Modules.Customer.Domain.AssemblyReference.Assembly, Modules.Customer.Application.AssemblyReference.Assembly, Modules.Customer.Infrastructure.AssemblyReference.Assembly, Modules.Customer.API.AssemblyReference.Assembly),
        ("Project", Modules.Project.Domain.AssemblyReference.Assembly, Modules.Project.Application.AssemblyReference.Assembly, Modules.Project.Infrastructure.AssemblyReference.Assembly, Modules.Project.API.AssemblyReference.Assembly),
        ("Finance", Modules.Finance.Domain.AssemblyReference.Assembly, Modules.Finance.Application.AssemblyReference.Assembly, Modules.Finance.Infrastructure.AssemblyReference.Assembly, Modules.Finance.API.AssemblyReference.Assembly),
        ("Support", Modules.Support.Domain.AssemblyReference.Assembly, Modules.Support.Application.AssemblyReference.Assembly, Modules.Support.Infrastructure.AssemblyReference.Assembly, Modules.Support.API.AssemblyReference.Assembly),
        ("Platform", Modules.Platform.Domain.AssemblyReference.Assembly, Modules.Platform.Application.AssemblyReference.Assembly, Modules.Platform.Infrastructure.AssemblyReference.Assembly, Modules.Platform.API.AssemblyReference.Assembly),
    };

    [Fact]
    public void DomainLayers_ShouldNotHaveDependencyOnOtherLayers()
    {
        foreach (var mod in ModuleAssemblies)
        {
            var result = Types.InAssembly(mod.Domain)
                .ShouldNot()
                .HaveDependencyOnAny(
                    $"Modules.{mod.ModuleName}.Application",
                    $"Modules.{mod.ModuleName}.Infrastructure",
                    $"Modules.{mod.ModuleName}.API",
                    "CrmErp.Host"
                )
                .GetResult();

            Assert.True(result.IsSuccessful, $"Domain layer for {mod.ModuleName} has invalid dependencies.");
        }
    }

    [Fact]
    public void ApplicationLayers_ShouldNotDependOnInfrastructureOrApiOrHost()
    {
        foreach (var mod in ModuleAssemblies)
        {
            var result = Types.InAssembly(mod.Application)
                .ShouldNot()
                .HaveDependencyOnAny(
                    $"Modules.{mod.ModuleName}.Infrastructure",
                    $"Modules.{mod.ModuleName}.API",
                    "CrmErp.Host"
                )
                .GetResult();

            Assert.True(result.IsSuccessful, $"Application layer for {mod.ModuleName} has invalid dependencies.");
        }
    }

    [Fact]
    public void ApiLayers_ShouldNotDependOnInfrastructure()
    {
        foreach (var mod in ModuleAssemblies)
        {
            var result = Types.InAssembly(mod.Api)
                .ShouldNot()
                .HaveDependencyOnAny($"Modules.{mod.ModuleName}.Infrastructure")
                .GetResult();

            Assert.True(result.IsSuccessful, $"API layer for {mod.ModuleName} directly depends on Infrastructure.");
        }
    }

    [Fact]
    public void Modules_ShouldNotDependOnOtherModules()
    {
        var allModuleNames = ModuleAssemblies.Select(m => m.ModuleName).ToArray();

        foreach (var mod in ModuleAssemblies)
        {
            var otherModules = allModuleNames.Where(m => m != mod.ModuleName).Select(m => $"Modules.{m}").ToArray();

            var assembliesToTest = new[] { mod.Domain, mod.Application, mod.Infrastructure, mod.Api };

            foreach (var assembly in assembliesToTest)
            {
                var result = Types.InAssembly(assembly)
                    .ShouldNot()
                    .HaveDependencyOnAny(otherModules)
                    .GetResult();

                Assert.True(result.IsSuccessful, $"Module {mod.ModuleName} assembly '{assembly.GetName().Name}' has direct dependencies on other modules.");
            }
        }
    }
}
