using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

/// <summary>
/// Perintah administratif backfill tagihan untuk membership Menunggu Pembayaran yang didaftarkan sebelum fitur
/// tagihan iuran ada. Bawaannya simulasi (<see cref="DryRun"/> = true); tidak pernah berjalan sendiri.
/// </summary>
public sealed class MembershipFeeBackfillRequest
{
    /// <summary>Simulasi saja bila true (bawaan): tidak ada data yang ditulis.</summary>
    public bool DryRun { get; set; } = true;

    /// <summary>Batasi ke satu nomor membership, mis. MBR-000001.</summary>
    [MaxLength(50)] public string? MemberNumber { get; set; }

    /// <summary>Batasi ke membership tertentu; kosong = semua kandidat.</summary>
    [MaxLength(200)] public List<Guid>? MembershipIds { get; set; }

    /// <summary>Jumlah maksimum membership yang diproses per panggilan (1-200).</summary>
    [Range(1, 200)] public int Limit { get; set; } = 100;
}

public static class MembershipFeeBackfillOutcomes
{
    public const string Created = "CREATED";
    public const string Linked = "LINKED";
    public const string AlreadyLinked = "ALREADY_LINKED";
    public const string WouldCreate = "WOULD_CREATE";
    public const string WouldLink = "WOULD_LINK";
    public const string SkippedNotPending = "SKIPPED_NOT_PENDING";
    public const string SkippedOverdue = "SKIPPED_OVERDUE";
    public const string NotFound = "NOT_FOUND";
    public const string Failed = "FAILED";
}

public sealed class MembershipFeeBackfillItem
{
    public Guid MembershipId { get; set; }
    public string MemberNumber { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public Guid? ChargeId { get; set; }
    public string? ChargeNumber { get; set; }
    public decimal FeeAmount { get; set; }
    public DateTimeOffset? PaymentDueAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class MembershipFeeBackfillResponse
{
    public bool DryRun { get; set; }
    public int TotalCandidates { get; set; }
    public int Processed { get; set; }
    public int Created { get; set; }
    public int Linked { get; set; }
    public int AlreadyLinked { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public IReadOnlyList<MembershipFeeBackfillItem> Items { get; set; } = [];
}
