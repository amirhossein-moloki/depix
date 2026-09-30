namespace Modules.CRM.Application.Features.Leads.DTOs;

public class LeadConversionResultDto
{
    public Guid LeadId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? ContactId { get; set; }
    public DateTime ConvertedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}
