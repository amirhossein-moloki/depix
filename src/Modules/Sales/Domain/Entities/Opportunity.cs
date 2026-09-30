using BuildingBlocks.Domain.Models;
using BuildingBlocks.Domain.ValueObjects;
using Modules.Sales.Domain.Constants;
using Modules.Sales.Domain.Events;

namespace Modules.Sales.Domain.Entities;

public class Opportunity : AuditableAggregateRoot, ISoftDelete
{
    private readonly List<Proposal> _proposals = new();

    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? LeadId { get; private set; }
    public Guid? CustomerId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? ContactId { get; private set; }
    public string Stage { get; private set; } = OpportunityStage.New;
    public string Status { get; private set; } = OpportunityStatus.Open;
    public Money Value { get; private set; } = Money.Zero();
    public decimal EstimatedValue => Value?.Amount ?? 0m;
    public int Probability { get; private set; } = 10;
    public DateOnly ExpectedCloseDate { get; private set; }
    public Guid? AssignedTo { get; private set; }
    public string? Source { get; private set; }
    public DateTime? WonAt { get; private set; }
    public DateTime? LostAt { get; private set; }
    public string? LossReason { get; private set; }

    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }

    public IReadOnlyCollection<Proposal> Proposals => _proposals.AsReadOnly();

    private Opportunity() { }

    public Opportunity(
        Guid id,
        string title,
        string? description,
        Guid? leadId,
        Guid? customerId,
        Guid? companyId,
        Guid? contactId,
        string stage,
        Money value,
        int probability,
        DateOnly expectedCloseDate,
        Guid? assignedTo = null,
        string? source = null) : base(id)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Opportunity title is required.", nameof(title));
        }

        if (probability < 0 || probability > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(probability), "Probability must be between 0 and 100.");
        }

        string normalizedStage = string.IsNullOrWhiteSpace(stage) ? OpportunityStage.New : stage.Trim();
        if (!OpportunityStage.IsValid(normalizedStage))
        {
            normalizedStage = OpportunityStage.New;
        }

        Title = title.Trim();
        Description = description?.Trim();
        LeadId = leadId;
        CustomerId = customerId;
        CompanyId = companyId;
        ContactId = contactId;
        Stage = normalizedStage;
        Value = value ?? Money.Zero();
        Probability = probability;
        ExpectedCloseDate = expectedCloseDate;
        AssignedTo = assignedTo;
        Source = source?.Trim();
        Status = normalizedStage.Equals(OpportunityStage.Won, StringComparison.OrdinalIgnoreCase) ? OpportunityStatus.Won :
                 normalizedStage.Equals(OpportunityStage.Lost, StringComparison.OrdinalIgnoreCase) ? OpportunityStatus.Lost :
                 OpportunityStatus.Open;

        if (Status == OpportunityStatus.Won)
        {
            WonAt = DateTime.UtcNow;
        }
        else if (Status == OpportunityStatus.Lost)
        {
            LostAt = DateTime.UtcNow;
        }
    }

    public static Opportunity Create(
        string title,
        Guid? leadId = null,
        Guid? customerId = null,
        Guid? companyId = null,
        Guid? contactId = null,
        string stage = OpportunityStage.New,
        Money? value = null,
        int? probability = null,
        DateOnly? expectedCloseDate = null,
        Guid? assignedTo = null,
        string? source = null,
        string? description = null)
    {
        var id = Guid.NewGuid();
        var actualValue = value ?? Money.Zero();
        var actualStage = string.IsNullOrWhiteSpace(stage) ? OpportunityStage.New : stage.Trim();
        var actualProbability = probability ?? OpportunityStage.GetDefaultProbability(actualStage);
        var actualCloseDate = expectedCloseDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30));

        var opportunity = new Opportunity(
            id,
            title,
            description,
            leadId,
            customerId,
            companyId,
            contactId,
            actualStage,
            actualValue,
            actualProbability,
            actualCloseDate,
            assignedTo,
            source);

        opportunity.AddDomainEvent(new OpportunityCreatedEvent(
            opportunity.Id,
            opportunity.Title,
            opportunity.LeadId,
            opportunity.CustomerId,
            opportunity.CompanyId,
            opportunity.Value,
            opportunity.Stage));

        return opportunity;
    }

    public void UpdateDetails(
        string title,
        string? description,
        Money value,
        int probability,
        DateOnly expectedCloseDate,
        string? source = null,
        Guid? contactId = null)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Opportunity title is required.", nameof(title));
        }

        if (probability < 0 || probability > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(probability), "Probability must be between 0 and 100.");
        }

        Title = title.Trim();
        Description = description?.Trim();
        Value = value ?? Money.Zero();
        Probability = probability;
        ExpectedCloseDate = expectedCloseDate;
        Source = source?.Trim();
        if (contactId.HasValue)
        {
            ContactId = contactId.Value;
        }

        UpdateTimestamp(DateTime.UtcNow);
    }

    public void MoveToStage(string newStage, int? probability = null)
    {
        if (string.IsNullOrWhiteSpace(newStage))
        {
            throw new ArgumentException("Stage is required.", nameof(newStage));
        }

        var oldStage = Stage;
        var normalizedStage = newStage.Trim();

        if (!OpportunityStage.IsValid(normalizedStage))
        {
            throw new InvalidOperationException($"Invalid stage '{newStage}'.");
        }

        Stage = normalizedStage;
        Probability = probability ?? OpportunityStage.GetDefaultProbability(normalizedStage);

        if (normalizedStage.Equals(OpportunityStage.Won, StringComparison.OrdinalIgnoreCase))
        {
            Status = OpportunityStatus.Won;
            WonAt ??= DateTime.UtcNow;
            LostAt = null;
            LossReason = null;
            AddDomainEvent(new OpportunityWonEvent(Id, Value, WonAt.Value));
        }
        else if (normalizedStage.Equals(OpportunityStage.Lost, StringComparison.OrdinalIgnoreCase))
        {
            Status = OpportunityStatus.Lost;
            LostAt ??= DateTime.UtcNow;
            WonAt = null;
            AddDomainEvent(new OpportunityLostEvent(Id, LossReason ?? "Moved to lost stage", LostAt.Value));
        }
        else
        {
            Status = OpportunityStatus.Open;
            WonAt = null;
            LostAt = null;
            LossReason = null;
        }

        UpdateTimestamp(DateTime.UtcNow);
        AddDomainEvent(new OpportunityStageChangedEvent(Id, oldStage, Stage, Probability));
    }

    public void AssignTo(Guid? userId)
    {
        AssignedTo = userId;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void MarkAsWon(DateTime? wonAt = null)
    {
        Stage = OpportunityStage.Won;
        Status = OpportunityStatus.Won;
        Probability = 100;
        WonAt = wonAt ?? DateTime.UtcNow;
        LostAt = null;
        LossReason = null;

        UpdateTimestamp(DateTime.UtcNow);
        AddDomainEvent(new OpportunityWonEvent(Id, Value, WonAt.Value));
    }

    public void MarkAsLost(string lossReason, DateTime? lostAt = null)
    {
        if (string.IsNullOrWhiteSpace(lossReason))
        {
            throw new ArgumentException("Loss reason is required when marking an opportunity as lost.", nameof(lossReason));
        }

        Stage = OpportunityStage.Lost;
        Status = OpportunityStatus.Lost;
        Probability = 0;
        LossReason = lossReason.Trim();
        LostAt = lostAt ?? DateTime.UtcNow;
        WonAt = null;

        UpdateTimestamp(DateTime.UtcNow);
        AddDomainEvent(new OpportunityLostEvent(Id, LossReason, LostAt.Value));
    }

    public void Reopen(string initialStage = OpportunityStage.New)
    {
        var targetStage = OpportunityStage.IsValid(initialStage) ? initialStage : OpportunityStage.New;
        Stage = targetStage;
        Status = OpportunityStatus.Open;
        Probability = OpportunityStage.GetDefaultProbability(targetStage);
        WonAt = null;
        LostAt = null;
        LossReason = null;

        UpdateTimestamp(DateTime.UtcNow);
        AddDomainEvent(new OpportunityStageChangedEvent(Id, OpportunityStage.Lost, Stage, Probability));
    }

    public void SoftDelete(Guid? deletedBy = null)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
        Status = OpportunityStatus.Archived;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void UndoSoftDelete()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
        Status = Stage.Equals(OpportunityStage.Won, StringComparison.OrdinalIgnoreCase) ? OpportunityStatus.Won :
                 Stage.Equals(OpportunityStage.Lost, StringComparison.OrdinalIgnoreCase) ? OpportunityStatus.Lost :
                 OpportunityStatus.Open;
        UpdateTimestamp(DateTime.UtcNow);
    }

    public void Archive(Guid? archivedBy = null)
    {
        SoftDelete(archivedBy);
    }

    public void AddProposal(Proposal proposal)
    {
        if (proposal != null)
        {
            _proposals.Add(proposal);
            UpdateTimestamp(DateTime.UtcNow);
        }
    }

    public void UpdateStage(string stage, int probability)
    {
        MoveToStage(stage, probability);
    }
}
