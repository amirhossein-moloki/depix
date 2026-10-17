using System.Text;
using Microsoft.EntityFrameworkCore;
using Modules.Identity.Application.Common.Interfaces;
using Modules.Identity.Domain.Entities;
using Modules.Identity.Domain.Repositories;
using BuildingBlocks.Infrastructure.Persistence;

namespace CrmErp.Host.Services;

public static class AdminCliHandler
{
    public static async Task<int> RunAsync(IServiceProvider serviceProvider, string[] args)
    {
        Console.WriteLine("=== Depix CRM/ERP Admin User Creation CLI ===");

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();

        string? email = GetArgValue(args, "--email");
        string? password = GetArgValue(args, "--password");
        string? firstName = GetArgValue(args, "--firstname") ?? "System";
        string? lastName = GetArgValue(args, "--lastname") ?? "Admin";

        if (string.IsNullOrWhiteSpace(email))
        {
            Console.Write("Enter Admin Email: ");
            email = Console.ReadLine()?.Trim();
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            Console.WriteLine("[Error] A valid email address is required.");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            Console.Write("Enter Admin Password: ");
            password = ReadPasswordSecurely();
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            Console.WriteLine("[Error] Password must be at least 6 characters long.");
            return 1;
        }

        try
        {
            // Ensure database is updated
            try
            {
                await dbContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Note] Database migration: {ex.Message}");
            }

            // Check if user already exists
            var existingUser = await userRepository.GetByEmailAsync(email);
            if (existingUser != null)
            {
                Console.WriteLine($"[Info] User with email '{email}' already exists with status: {existingUser.Status}.");
                return 0;
            }

            // Get or Create Admin Role
            var roles = await roleRepository.GetAllAsync();
            var adminRole = roles.FirstOrDefault(r => r.Name.Equals("Admin", StringComparison.OrdinalIgnoreCase) || r.Name.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase));

            if (adminRole == null)
            {
                adminRole = Role.Create("Admin", "Administrator role with full system access");

                // Seed default permissions if any exist in Permission table
                var allPermissions = await dbContext.Set<Permission>().ToListAsync();
                if (!allPermissions.Any())
                {
                    var defaultPermissions = new[]
                    {
                        Permission.Create("Identity.User.Read", "Read Users", "Identity"),
                        Permission.Create("Identity.User.Create", "Create Users", "Identity"),
                        Permission.Create("Identity.User.Update", "Update Users", "Identity"),
                        Permission.Create("Identity.User.Delete", "Delete Users", "Identity"),
                        Permission.Create("Identity.User.Approve", "Approve/Reject Users", "Identity"),
                        Permission.Create("CRM.Company.Read", "Read Companies", "CRM"),
                        Permission.Create("CRM.Company.Create", "Create Companies", "CRM"),
                        Permission.Create("CRM.Company.Update", "Update Companies", "CRM"),
                        Permission.Create("CRM.Company.Delete", "Delete Companies", "CRM"),
                        Permission.Create("CRM.Visit.Read", "Read Visits", "CRM"),
                        Permission.Create("CRM.Visit.Create", "Create Visits", "CRM")
                    };

                    await dbContext.Set<Permission>().AddRangeAsync(defaultPermissions);
                    await dbContext.SaveChangesAsync();
                    allPermissions = defaultPermissions.ToList();
                }

                foreach (var perm in allPermissions)
                {
                    adminRole.AddPermission(perm.Id);
                }

                await roleRepository.AddAsync(adminRole);
                Console.WriteLine($"[Info] Created 'Admin' role with {adminRole.RolePermissions.Count} permissions.");
            }

            // Hash Password and Create Admin User
            var hashedPassword = passwordHasher.HashPassword(password);
            var adminUser = User.Create(firstName, lastName, email, hashedPassword);
            adminUser.AddRole(adminRole.Id);

            await userRepository.AddAsync(adminUser);

            Console.WriteLine($"[Success] Admin user '{email}' created successfully with status 'Active'.");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] Could not create admin user due to database error: {ex.Message}");
            return 1;
        }
    }

    private static string? GetArgValue(string[] args, string name)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }
        return null;
    }

    private static string ReadPasswordSecurely()
    {
        var password = new StringBuilder();
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }
            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
                Console.Write("*");
            }
        }
        return password.ToString();
    }
}
