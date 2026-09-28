using BuildingBlocks.Domain.Models;

namespace Modules.Project.Domain.Entities;

public class Deployment : Entity
{
    public Guid ProjectId { get; private set; }
    public string Server { get; private set; } = string.Empty;
    public string Provider { get; private set; } = string.Empty;
    public string Domain { get; private set; } = string.Empty;
    public string SslStatus { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;
    public DateTime DeployedAt { get; private set; } = DateTime.UtcNow;

    private Deployment() { }

    public Deployment(Guid id, Guid projectId, string server, string provider, string domain, string sslStatus, string version, DateTime deployedAt) : base(id)
    {
        ProjectId = projectId;
        Server = server;
        Provider = provider;
        Domain = domain;
        SslStatus = sslStatus;
        Version = version;
        DeployedAt = deployedAt;
    }

    public static Deployment Create(Guid projectId, string server, string provider, string domain, string sslStatus, string version)
    {
        return new Deployment(Guid.NewGuid(), projectId, server, provider, domain, sslStatus, version, DateTime.UtcNow);
    }
}
