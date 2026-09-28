using BuildingBlocks.Domain.Models;

namespace Modules.Support.Domain.Entities;

public class SupportPlan : Entity
{
    public Guid ProjectId { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public int Days { get; private set; }

    private SupportPlan() { }

    public SupportPlan(Guid id, Guid projectId, DateOnly startDate, DateOnly endDate, int days) : base(id)
    {
        ProjectId = projectId;
        StartDate = startDate;
        EndDate = endDate;
        Days = days;
    }

    public static SupportPlan Create(Guid projectId, DateOnly startDate, DateOnly endDate, int days)
    {
        return new SupportPlan(Guid.NewGuid(), projectId, startDate, endDate, days);
    }
}
