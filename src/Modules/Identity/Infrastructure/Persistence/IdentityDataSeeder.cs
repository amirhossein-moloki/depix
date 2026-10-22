using BuildingBlocks.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Modules.Identity.Domain.Entities;

namespace Modules.Identity.Infrastructure.Persistence;

public static class IdentityDataSeeder
{
    public static readonly (string Code, string Name, string Module)[] SystemPermissions = new (string Code, string Name, string Module)[]
    {
        // CRM
        ("CRM.Lead.Create", "Create Lead", "CRM"),
        ("CRM.Lead.Read", "Read Lead", "CRM"),
        ("CRM.Lead.Update", "Update Lead", "CRM"),
        ("CRM.Lead.Assign", "Assign Lead", "CRM"),
        ("CRM.Lead.StatusChange", "Change Lead Status", "CRM"),
        ("CRM.Lead.Qualify", "Qualify/Disqualify Lead", "CRM"),
        ("CRM.Lead.Convert", "Convert Lead", "CRM"),
        ("CRM.Lead.Delete", "Delete Lead", "CRM"),

        ("CRM.Company.Create", "Create Company", "CRM"),
        ("CRM.Company.Read", "Read Company", "CRM"),
        ("CRM.Company.Update", "Update Company", "CRM"),
        ("CRM.Company.Delete", "Delete Company", "CRM"),

        ("CRM.Contact.Create", "Create Contact", "CRM"),
        ("CRM.Contact.Read", "Read Contact", "CRM"),
        ("CRM.Contact.Update", "Update Contact", "CRM"),
        ("CRM.Contact.Delete", "Delete Contact", "CRM"),

        ("CRM.Activity.Create", "Create Activity", "CRM"),
        ("CRM.Activity.Read", "Read Activity", "CRM"),
        ("CRM.Activity.Update", "Update Activity", "CRM"),
        ("CRM.Activity.Delete", "Delete Activity", "CRM"),

        ("CRM.SalesNote.Create", "Create Sales Note", "CRM"),
        ("CRM.SalesNote.Read", "Read Sales Note", "CRM"),
        ("CRM.SalesNote.Update", "Update Sales Note", "CRM"),
        ("CRM.SalesNote.Delete", "Delete Sales Note", "CRM"),

        // Customer
        ("Customer.Create", "Create Customer", "Customer"),
        ("Customer.Read", "Read Customer", "Customer"),
        ("Customer.Update", "Update Customer", "Customer"),
        ("Customer.StatusChange", "Change Customer Status", "Customer"),
        ("Customer.Assign", "Assign Customer", "Customer"),
        ("Customer.Delete", "Delete Customer", "Customer"),

        // Sales
        ("Sales.Opportunity.Create", "Create Opportunity", "Sales"),
        ("Sales.Opportunity.Read", "Read Opportunity", "Sales"),
        ("Sales.Opportunity.Update", "Update Opportunity", "Sales"),
        ("Sales.Opportunity.Assign", "Assign Opportunity", "Sales"),
        ("Sales.Opportunity.StageChange", "Change Opportunity Stage", "Sales"),
        ("Sales.Opportunity.Close", "Close Opportunity", "Sales"),
        ("Sales.Opportunity.Delete", "Delete Opportunity", "Sales"),

        ("Sales.Proposal.Create", "Create Proposal", "Sales"),
        ("Sales.Proposal.Read", "Read Proposal", "Sales"),
        ("Sales.Proposal.Update", "Update Proposal", "Sales"),
        ("Sales.Proposal.StatusChange", "Change Proposal Status", "Sales"),
        ("Sales.Proposal.Delete", "Delete Proposal", "Sales"),

        // Project
        ("Project.Create", "Create Project", "Project"),
        ("Project.Read", "Read Project", "Project"),
        ("Project.Update", "Update Project", "Project"),
        ("Project.StatusChange", "Change Project Status", "Project"),
        ("Project.Delete", "Delete Project", "Project"),
        ("Project.Repository.Manage", "Manage Repositories", "Project"),
        ("Project.Deployment.Manage", "Manage Deployments", "Project"),

        // Finance
        ("Finance.Invoice.Create", "Create Invoice", "Finance"),
        ("Finance.Invoice.Read", "Read Invoice", "Finance"),
        ("Finance.Invoice.Update", "Update Invoice", "Finance"),
        ("Finance.Invoice.Issue", "Issue Invoice", "Finance"),
        ("Finance.Invoice.Cancel", "Cancel Invoice", "Finance"),
        ("Finance.Invoice.Delete", "Delete Invoice", "Finance"),
        ("Finance.Payment.Create", "Create Payment", "Finance"),
        ("Finance.Payment.Read", "Read Payment", "Finance"),

        // Support
        ("Support.Ticket.Create", "Create Ticket", "Support"),
        ("Support.Ticket.Read", "Read Ticket", "Support"),
        ("Support.Ticket.Update", "Update Ticket", "Support"),
        ("Support.Ticket.Assign", "Assign Ticket", "Support"),
        ("Support.Ticket.StatusChange", "Change Ticket Status", "Support"),
        ("Support.Ticket.Resolve", "Resolve Ticket", "Support"),
        ("Support.Ticket.Close", "Close Ticket", "Support"),
        ("Support.Ticket.Delete", "Delete Ticket", "Support"),
        ("Support.Comment.Create", "Create Ticket Comment", "Support"),
        ("Support.Comment.Read", "Read Ticket Comment", "Support"),

        // Platform
        ("Platform.Task.Create", "Create Task", "Platform"),
        ("Platform.Task.Read", "Read Task", "Platform"),
        ("Platform.Task.Update", "Update Task", "Platform"),
        ("Platform.Task.Delete", "Delete Task", "Platform"),

        ("Platform.File.Create", "Upload File", "Platform"),
        ("Platform.File.Read", "Read File", "Platform"),
        ("Platform.File.Delete", "Delete File", "Platform"),

        ("Platform.Audit.Read", "Read Audit Logs", "Platform"),

        ("Platform.Setting.Read", "Read System Settings", "Platform"),
        ("Platform.Setting.Write", "Write System Settings", "Platform"),
        ("Platform.Setting.Delete", "Delete System Settings", "Platform"),

        // Reporting
        ("Reporting.Read", "Read Summary Report", "Reporting"),
        ("Reporting.CRM.Read", "Read CRM Report", "Reporting"),
        ("Reporting.Sales.Read", "Read Sales Report", "Reporting"),
        ("Reporting.Customer.Read", "Read Customer Report", "Reporting"),
        ("Reporting.Project.Read", "Read Project Report", "Reporting"),
        ("Reporting.Finance.Read", "Read Finance Report", "Reporting"),
        ("Reporting.Support.Read", "Read Support Report", "Reporting")
    };

    public static async Task SeedPermissionsAndRolesAsync(ApplicationDbContext dbContext, ILogger? logger = null, CancellationToken cancellationToken = default)
    {
        var existingPermissions = await dbContext.Set<Permission>().ToListAsync(cancellationToken);
        var permissionsMap = existingPermissions.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);
        bool hasChanges = false;

        foreach (var p in SystemPermissions)
        {
            if (!permissionsMap.ContainsKey(p.Code))
            {
                var newPerm = Permission.Create(p.Code, p.Name, p.Module);
                await dbContext.Set<Permission>().AddAsync(newPerm, cancellationToken);
                permissionsMap[p.Code] = newPerm;
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            logger?.LogInformation("Seeded missing system permissions into database.");
        }

        var defaultRoles = new (string Name, string Description, Func<Permission, bool> PermFilter)[]
        {
            ("Admin", "System Administrator Role", _ => true),
            ("User", "Standard System User Role", p =>
                p.Code.EndsWith(".Read") ||
                p.Code == "CRM.Lead.Create" || p.Code == "CRM.Activity.Create" || p.Code == "CRM.SalesNote.Create" ||
                p.Code == "Support.Ticket.Create" || p.Code == "Support.Comment.Create" ||
                p.Code == "Platform.Task.Create" || p.Code == "Platform.Task.Update" || p.Code == "Platform.File.Create"),
            ("SalesManager", "Sales Manager Role", p =>
                p.Module == "CRM" || p.Module == "Sales" ||
                p.Module == "Customer" ||
                p.Code == "Reporting.Read" || p.Code == "Reporting.CRM.Read" || p.Code == "Reporting.Sales.Read" || p.Code == "Reporting.Customer.Read" ||
                (p.Module == "Platform" && !p.Code.StartsWith("Platform.Audit") && !p.Code.StartsWith("Platform.Setting.Write"))),
            ("Sales", "Sales Representative Role", p =>
                (p.Module == "CRM" && !p.Code.EndsWith(".Delete")) ||
                (p.Module == "Sales" && !p.Code.EndsWith(".Delete") && p.Code != "Sales.Opportunity.Close") ||
                p.Code == "Customer.Read" ||
                p.Code == "Reporting.Read" || p.Code == "Reporting.CRM.Read" || p.Code == "Reporting.Sales.Read" ||
                p.Code.StartsWith("Platform.Task.") || p.Code.StartsWith("Platform.File.")),
            ("SalesRep", "Sales Representative Role", p =>
                (p.Module == "CRM" && !p.Code.EndsWith(".Delete")) ||
                (p.Module == "Sales" && !p.Code.EndsWith(".Delete") && p.Code != "Sales.Opportunity.Close") ||
                p.Code == "Customer.Read" ||
                p.Code == "Reporting.Read" || p.Code == "Reporting.CRM.Read" || p.Code == "Reporting.Sales.Read" ||
                p.Code.StartsWith("Platform.Task.") || p.Code.StartsWith("Platform.File.")),
            ("SupportAgent", "Support Specialist Role", p =>
                p.Module == "Support" ||
                p.Code == "Customer.Read" || p.Code == "Customer.Update" || p.Code == "CRM.Contact.Read" ||
                p.Code == "Reporting.Read" || p.Code == "Reporting.Support.Read" || p.Code == "Reporting.Customer.Read" ||
                p.Code.StartsWith("Platform.Task.") || p.Code.StartsWith("Platform.File.")),
            ("Support", "Support Specialist Role", p =>
                p.Module == "Support" ||
                p.Code == "Customer.Read" || p.Code == "Customer.Update" || p.Code == "CRM.Contact.Read" ||
                p.Code == "Reporting.Read" || p.Code == "Reporting.Support.Read" || p.Code == "Reporting.Customer.Read" ||
                p.Code.StartsWith("Platform.Task.") || p.Code.StartsWith("Platform.File.")),
            ("FinanceManager", "Finance Manager Role", p =>
                p.Module == "Finance" ||
                p.Code == "Sales.Proposal.Read" || p.Code == "Customer.Read" ||
                p.Code == "Reporting.Read" || p.Code == "Reporting.Finance.Read" ||
                p.Code.StartsWith("Platform.Task.") || p.Code.StartsWith("Platform.File.")),
            ("Finance", "Finance Specialist Role", p =>
                p.Module == "Finance" ||
                p.Code == "Sales.Proposal.Read" || p.Code == "Customer.Read" ||
                p.Code == "Reporting.Read" || p.Code == "Reporting.Finance.Read" ||
                p.Code.StartsWith("Platform.Task.") || p.Code.StartsWith("Platform.File.")),
            ("ProjectManager", "Project Manager Role", p =>
                p.Module == "Project" ||
                p.Code == "Customer.Read" ||
                p.Code == "Reporting.Read" || p.Code == "Reporting.Project.Read" ||
                p.Code.StartsWith("Platform.Task.") || p.Code.StartsWith("Platform.File.")),
            ("Project", "Project Specialist Role", p =>
                p.Module == "Project" ||
                p.Code == "Customer.Read" ||
                p.Code == "Reporting.Read" || p.Code == "Reporting.Project.Read" ||
                p.Code.StartsWith("Platform.Task.") || p.Code.StartsWith("Platform.File."))
        };

        var existingRoles = await dbContext.Set<Role>().ToListAsync(cancellationToken);
        var rolesMap = existingRoles.ToDictionary(r => r.Name, StringComparer.OrdinalIgnoreCase);
        var newlyCreatedRoleIds = new HashSet<Guid>();
        bool rolesCreated = false;

        foreach (var def in defaultRoles)
        {
            if (!rolesMap.TryGetValue(def.Name, out var roleEntity))
            {
                roleEntity = Role.Create(def.Name, def.Description);
                await dbContext.Set<Role>().AddAsync(roleEntity, cancellationToken);
                rolesMap[def.Name] = roleEntity;
                newlyCreatedRoleIds.Add(roleEntity.Id);
                rolesCreated = true;
            }
        }

        if (rolesCreated)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            logger?.LogInformation("Seeded missing system default roles in database.");
        }

        var allRolePermissions = await dbContext.Set<RolePermission>().ToListAsync(cancellationToken);
        var rolePermSet = allRolePermissions.Select(rp => (rp.RoleId, rp.PermissionId)).ToHashSet();
        bool rolePermsChanged = false;

        foreach (var def in defaultRoles)
        {
            if (rolesMap.TryGetValue(def.Name, out var roleEntity))
            {
                // To preserve administrator customizations, default permissions are only attached
                // when a role is newly created during initial database setup or when adding a new system role.
                // For existing roles already in the database, we leave their permission configuration intact
                // so administrator additions or removals persist across restarts.
                bool isNewlyCreatedRole = newlyCreatedRoleIds.Contains(roleEntity.Id);
                bool isRoleWithoutPermissions = !allRolePermissions.Any(rp => rp.RoleId == roleEntity.Id);

                if (isNewlyCreatedRole || isRoleWithoutPermissions)
                {
                    var targetPerms = permissionsMap.Values.Where(def.PermFilter).ToList();
                    foreach (var perm in targetPerms)
                    {
                        if (!rolePermSet.Contains((roleEntity.Id, perm.Id)))
                        {
                            await dbContext.Set<RolePermission>().AddAsync(new RolePermission(roleEntity.Id, perm.Id), cancellationToken);
                            rolePermSet.Add((roleEntity.Id, perm.Id));
                            rolePermsChanged = true;
                        }
                    }
                }
            }
        }

        if (rolePermsChanged)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            logger?.LogInformation("Attached permissions to newly created system roles successfully.");
        }
    }
}
