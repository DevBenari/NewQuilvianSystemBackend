namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;

/// <summary>Kebijakan kirim ulang otomatis (<c>BE-RJE-012</c>, <c>RJ-E2E-DEC-009</c>).</summary>
public sealed class BillingSyncPolicyResponse
{
    public Guid Id { get; set; }
    public string PolicyCode { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;
    public int MaxAttemptCount { get; set; }
    public int BaseDelaySeconds { get; set; }
    public int MaxDelaySeconds { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public Guid RowVersion { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// <c>MaxAttemptCount</c> 0–20; <c>BaseDelaySeconds</c> 10–3600; <c>MaxDelaySeconds</c> ≥
/// <c>BaseDelaySeconds</c> dan ≤ 86400. Batas sama dengan check constraint tabel.
/// </summary>
public sealed class UpdateBillingSyncPolicyRequest
{
    public int MaxAttemptCount { get; set; }
    public int BaseDelaySeconds { get; set; }
    public int MaxDelaySeconds { get; set; }
    public bool IsActive { get; set; }
    public Guid RowVersion { get; set; }
}
