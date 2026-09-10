namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class DepositPolicyResponse
{
    public bool IsRequired { get; set; }
    public decimal MinimumAmount { get; set; }
    public int FollowUpIntervalDays { get; set; }
    public Guid? GuarantorId { get; set; }
    public Guid? PatientClassId { get; set; }
    public Guid? PolicyId { get; set; }
    public string? PolicyCode { get; set; }
    public string? PolicyName { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? EffectiveFrom { get; set; }
    public DateTimeOffset? EffectiveTo { get; set; }
}
