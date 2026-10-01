using BuildingBlocks.Application.Contracts;
using BuildingBlocks.Application.Persistence;
using BuildingBlocks.Common.Exceptions;
using Moq;
using Modules.Project.Application.Commands;
using Modules.Project.Domain.Entities;
using Modules.Project.Domain.Repositories;
using Xunit;

namespace UnitTests;

public class ProjectCommandHandlerTests
{
    private readonly Mock<IProjectRepository> _projectRepoMock = new();
    private readonly Mock<ICustomerService> _customerServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();

    [Fact]
    public async Task CreateProjectCommandHandler_ValidCustomer_ShouldCreateAndSave()
    {
        var customerId = Guid.NewGuid();
        _customerServiceMock.Setup(c => c.CustomerExistsAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateProjectCommandHandler(_projectRepoMock.Object, _customerServiceMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProjectCommand(customerId, "New Web App", "WebDevelopment");

        var result = await handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Equal("New Web App", result.Name);
        Assert.Equal(customerId, result.CustomerId);
        Assert.Equal("PLANNED", result.Status);

        _projectRepoMock.Verify(r => r.AddAsync(It.IsAny<Project>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProjectCommandHandler_InvalidCustomer_ShouldThrowBusinessRuleException()
    {
        var customerId = Guid.NewGuid();
        _customerServiceMock.Setup(c => c.CustomerExistsAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateProjectCommandHandler(_projectRepoMock.Object, _customerServiceMock.Object, _unitOfWorkMock.Object);
        var command = new CreateProjectCommand(customerId, "New Web App");

        await Assert.ThrowsAsync<BusinessRuleException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task StartProjectCommandHandler_ExistingProject_ShouldStartAndSave()
    {
        var project = Project.Create(Guid.NewGuid(), "API Gateway");
        _projectRepoMock.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var handler = new StartProjectCommandHandler(_projectRepoMock.Object, _unitOfWorkMock.Object);
        var command = new StartProjectCommand(project.Id, new DateOnly(2026, 10, 1));

        var result = await handler.HandleAsync(command);

        Assert.Equal("IN_PROGRESS", result.Status);
        Assert.Equal(new DateOnly(2026, 10, 1), result.StartDate);

        _projectRepoMock.Verify(r => r.Update(project), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddRepositoryCommandHandler_ExistingProject_ShouldAddRepository()
    {
        var project = Project.Create(Guid.NewGuid(), "Mobile Backend");
        _projectRepoMock.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var handler = new AddRepositoryCommandHandler(_projectRepoMock.Object, _unitOfWorkMock.Object);
        var command = new AddRepositoryCommand(project.Id, "GitHub", "https://github.com/org/repo", "Mobile API", "main", "Main repo");

        var result = await handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Equal("https://github.com/org/repo", result.Url);
        Assert.Equal("Mobile API", result.Name);

        _projectRepoMock.Verify(r => r.Update(project), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddDeploymentCommandHandler_ExistingProject_ShouldAddDeployment()
    {
        var project = Project.Create(Guid.NewGuid(), "Mobile Backend");
        _projectRepoMock.Setup(r => r.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var handler = new AddDeploymentCommandHandler(_projectRepoMock.Object, _unitOfWorkMock.Object);
        var command = new AddDeploymentCommand(project.Id, "Staging", "s1.prod", "GCP", "staging.api.com", "Active", "1.1.0", "Staging environment");

        var result = await handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Equal("Staging", result.Environment);
        Assert.Equal("staging.api.com", result.Domain);

        _projectRepoMock.Verify(r => r.Update(project), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
