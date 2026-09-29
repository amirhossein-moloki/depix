using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Activities.Queries;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class ActivityQueryHandlerTests
{
    private readonly IActivityRepository _activityRepository = Substitute.For<IActivityRepository>();
    private readonly ILeadRepository _leadRepository = Substitute.For<ILeadRepository>();

    [Fact]
    public async Task GetActivityByIdQueryHandler_WithExistingActivity_ShouldReturnDto()
    {
        var activity = Activity.Create(Guid.NewGuid(), Guid.NewGuid(), "CALL", "Call Subject", "Desc", "Outcome", 80);
        _activityRepository.GetByIdAsync(activity.Id, Arg.Any<CancellationToken>()).Returns(activity);

        var handler = new GetActivityByIdQueryHandler(_activityRepository);
        var result = await handler.HandleAsync(new GetActivityByIdQuery(activity.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(activity.Id, result.Id);
        Assert.Equal("Call Subject", result.Subject);
    }

    [Fact]
    public async Task GetActivityByIdQueryHandler_WithDeletedActivity_ShouldThrowEntityNotFoundException()
    {
        var activity = Activity.Create(Guid.NewGuid(), Guid.NewGuid(), "CALL", "Call Subject", "Desc", "Outcome", 80);
        activity.SoftDelete();
        _activityRepository.GetByIdAsync(activity.Id, Arg.Any<CancellationToken>()).Returns(activity);

        var handler = new GetActivityByIdQueryHandler(_activityRepository);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(new GetActivityByIdQuery(activity.Id), CancellationToken.None));
    }

    [Fact]
    public async Task GetLeadActivitiesQueryHandler_WithValidLead_ShouldReturnLeadActivityHistory()
    {
        var lead = Lead.Create(Guid.NewGuid(), "Website Lead", "Web");
        _leadRepository.GetByIdAsync(lead.Id, Arg.Any<CancellationToken>()).Returns(lead);

        var activities = new List<Activity>
        {
            Activity.Create(lead.Id, Guid.NewGuid(), "CALL", "Call 1", "Desc 1", "Res 1", 70),
            Activity.Create(lead.Id, Guid.NewGuid(), "MEETING", "Meeting 1", "Desc 2", "Res 2", 90)
        };

        _activityRepository.CountAsync(lead.Id, null, null, null, null, null, null, null, null, Arg.Any<CancellationToken>()).Returns(2);
        _activityRepository.GetListAsync(1, 20, lead.Id, null, null, null, null, null, null, null, null, Arg.Any<CancellationToken>()).Returns(activities);

        var handler = new GetLeadActivitiesQueryHandler(_activityRepository, _leadRepository);
        var result = await handler.HandleAsync(new GetLeadActivitiesQuery(lead.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(lead.Id, result.LeadId);
        Assert.Equal(2, result.TotalActivitiesCount);
        Assert.Equal(2, result.Activities.Count);
        Assert.NotNull(result.LastInteractionAt);
    }
}
