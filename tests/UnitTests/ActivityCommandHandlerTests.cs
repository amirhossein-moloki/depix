using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Modules.CRM.Application.Features.Activities.Commands;
using Modules.CRM.Domain.Entities;
using Modules.CRM.Domain.Events;
using Modules.CRM.Domain.Repositories;
using NSubstitute;
using Xunit;

namespace UnitTests;

public class ActivityCommandHandlerTests
{
    private readonly IActivityRepository _activityRepository = Substitute.For<IActivityRepository>();
    private readonly ILeadRepository _leadRepository = Substitute.For<ILeadRepository>();
    private readonly IContactRepository _contactRepository = Substitute.For<IContactRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task CreateActivityCommandHandler_WithValidData_ShouldCreateActivityAndAddDomainEvent()
    {
        var companyId = Guid.NewGuid();
        var lead = Lead.Create(companyId, "Website Lead", "Web");
        _leadRepository.GetByIdAsync(lead.Id, Arg.Any<CancellationToken>()).Returns(lead);

        var handler = new CreateActivityCommandHandler(_activityRepository, _leadRepository, _contactRepository, _unitOfWork);
        var command = new CreateActivityCommand(
            lead.Id,
            Guid.NewGuid(),
            "CALL",
            "Discovery Call",
            "Discussed project scope",
            "Interested",
            90
        );

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(lead.Id, result.LeadId);
        Assert.Equal("CALL", result.Type);
        Assert.Equal("Discovery Call", result.Subject);
        Assert.Equal("Interested", result.Result);
        Assert.Equal(90, result.QualityScore);

        await _activityRepository.Received(1).AddAsync(Arg.Any<Activity>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Contains(lead.DomainEvents, e => e is ActivityCreatedEvent);
    }

    [Fact]
    public async Task CreateActivityCommandHandler_WithNonExistentLead_ShouldThrowEntityNotFoundException()
    {
        _leadRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Lead?)null);

        var handler = new CreateActivityCommandHandler(_activityRepository, _leadRepository, _contactRepository, _unitOfWork);
        var command = new CreateActivityCommand(Guid.NewGuid(), Guid.NewGuid(), "CALL", "Call", "Desc", "Result");

        await Assert.ThrowsAsync<EntityNotFoundException>(() => handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateActivityCommandHandler_WithValidData_ShouldUpdateActivity()
    {
        var activity = Activity.Create(Guid.NewGuid(), Guid.NewGuid(), "CALL", "Initial Call", "Desc", "Pending", 50);
        _activityRepository.GetByIdAsync(activity.Id, Arg.Any<CancellationToken>()).Returns(activity);

        var handler = new UpdateActivityCommandHandler(_activityRepository, _contactRepository, _unitOfWork);
        var command = new UpdateActivityCommand(
            activity.Id,
            "MEETING",
            "Updated Subject",
            "Updated Desc",
            "Successful",
            95
        );

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.Equal("MEETING", result.Type);
        Assert.Equal("Updated Subject", result.Subject);
        Assert.Equal("Successful", result.Result);
        Assert.Equal(95, result.QualityScore);

        _activityRepository.Received(1).Update(activity);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ArchiveActivityCommandHandler_WithValidActivity_ShouldSoftDeleteActivity()
    {
        var activity = Activity.Create(Guid.NewGuid(), Guid.NewGuid(), "CALL", "Call", "Desc", "Result", 60);
        _activityRepository.GetByIdAsync(activity.Id, Arg.Any<CancellationToken>()).Returns(activity);

        var handler = new ArchiveActivityCommandHandler(_activityRepository, _unitOfWork);
        var command = new ArchiveActivityCommand(activity.Id, Guid.NewGuid());

        await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(activity.IsDeleted);
        _activityRepository.Received(1).Update(activity);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
