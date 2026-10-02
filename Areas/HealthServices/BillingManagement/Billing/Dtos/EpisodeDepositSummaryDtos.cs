namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class EpisodeDepositSummaryResponse
{
    public Guid EpisodeId { get; set; }
    public Guid EncounterId { get; set; }
    public Guid? DepositAccountId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string DepositStatus { get; set; } = string.Empty;
    public bool HasDepositAccount { get; set; }
    public bool IsPolicyRequired { get; set; }
    public decimal MinimumPolicyAmount { get; set; }
    public decimal TotalReceived { get; set; }
    public decimal TotalAllocated { get; set; }
    public decimal TotalRefunded { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal PolicyShortfallAmount { get; set; }
    public decimal FinalBillAmount { get; set; }
    public decimal FinalBillShortfallAmount { get; set; }
    public decimal OutstandingTopUp { get; set; }
    public int FollowUpIntervalDays { get; set; }
    public Guid? GuarantorId { get; set; }
    public Guid? PatientClassId { get; set; }
}
