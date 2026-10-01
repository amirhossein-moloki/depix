using System.Linq.Expressions;
using System.Reflection;
using BuildingBlocks.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var moduleAssemblies = GetModuleAssemblies();

        foreach (var assembly in moduleAssemblies)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);
        }

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDelete).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDelete.IsDeleted));
                var filter = Expression.Lambda(Expression.Equal(property, Expression.Constant(false)), parameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    private static Assembly[] GetModuleAssemblies()
    {
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName != null && (a.FullName.StartsWith("Modules.") || a.FullName.StartsWith("BuildingBlocks.")))
            .ToList();

        var moduleNames = new[] { "Identity", "CRM", "Sales", "Customer", "Project", "Finance", "Support", "Platform" };

        foreach (var module in moduleNames)
        {
            var infraAssemblyName = $"Modules.{module}.Infrastructure";
            if (!loadedAssemblies.Any(a => a.GetName().Name == infraAssemblyName))
            {
                try
                {
                    var loaded = Assembly.Load(infraAssemblyName);
                    if (loaded != null)
                    {
                        loadedAssemblies.Add(loaded);
                    }
                }
                catch
                {
                    // Ignore assembly load failures if a module infrastructure is missing
                }
            }
        }

        return loadedAssemblies.ToArray();
    }
}
