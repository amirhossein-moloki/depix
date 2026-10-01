using BuildingBlocks.Domain.Models;

namespace Modules.Project.Domain.Entities;

public class Deployment : Entity
{
    public Guid ProjectId { get; private set; }
    public string Environment { get; private set; } = string.Empty;
    public string Server { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public string Domain { get; private set; } = string.Empty;
    public string SslStatus { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;
    public DateTime DeployedAt { get; private set; } = DateTime.UtcNow;
    public string Notes { get; private set; } = string.Empty;

    private Deployment() { }

    public Deployment(
        Guid id,
        Guid projectId,
        string environment,
        string server,
        string provider,
        string domain,
        string sslStatus,
        string version,
        DateTime deployedAt,
        string notes) : base(id)
    {
        ProjectId = projectId;
        Environment = environment;
        Server = server;
        Provider = provider;
        Domain = domain;
        SslStatus = sslStatus;
        Version = version;
        DeployedAt = deployedAt;
        Notes = notes;
    }

    public static Deployment Create(
        Guid projectId,
        string environment,
        string? server = null,
        string? provider = null,
        string? domain = null,
        string? sslStatus = null,
        string? version = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(environment))
        {
            throw new ArgumentException("Deployment environment cannot be empty.", nameof(environment));
        }

        return new Deployment(
            Guid.NewGuid(),
            projectId,
            environment.Trim(),
            server?.Trim() ?? string.Empty,
            provider?.Trim() ?? string.Empty,
            domain?.Trim() ?? string.Empty,
            sslStatus?.Trim() ?? "Active",
            version?.Trim() ?? "1.0.0",
            DateTime.UtcNow,
            notes?.Trim() ?? string.Empty);
    }

    public void Update(
        string environment,
        string? server,
        string? provider,
        string? domain,
        string? sslStatus,
        string? version,
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(environment))
        {
            throw new ArgumentException("Deployment environment cannot be empty.", nameof(environment));
        }

        Environment = environment.Trim();
        Server = server?.Trim() ?? string.Empty;
        Provider = provider?.Trim() ?? string.Empty;
        Domain = domain?.Trim() ?? string.Empty;
        SslStatus = sslStatus?.Trim() ?? "Active";
        Version = version?.Trim() ?? "1.0.0";
        Notes = notes?.Trim() ?? string.Empty;
        DeployedAt = DateTime.UtcNow;
    }
}
