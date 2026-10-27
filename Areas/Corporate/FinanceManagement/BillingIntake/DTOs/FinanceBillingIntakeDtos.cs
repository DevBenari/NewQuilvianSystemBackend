using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Dtos;

public sealed class BillingIntakeQuery
{
    public string? Status { get; set; }
    public string? HandoffType { get; set; }
    public string SortBy { get; set; } = "createDateTime";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class BillingIntakeResponse
{
    public Guid Id { get; set; }
    public string HandoffType { get; set; } = string.Empty;
    public Guid SourceHandoffId { get; set; }
    public Guid SourceHandoffKey { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? TargetEntityId { get; set; }
    public DateTimeOffset? ConsumedAt { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid RowVersion { get; set; }
    public DateTime CreateDateTime { get; set; }
}

public sealed class BillingIntakeSummaryResponse
{
    public int TotalIntake { get; set; }
    public int NewCount { get; set; }
    public int ErrorCount { get; set; }
    public int ConsumedCount { get; set; }
    public int AcknowledgedCount { get; set; }
}

public sealed class BillingIntakeFilterMetadataResponse
{
    public List<int> PageSizeOptions { get; set; } = new();
    public List<string> SortableFields { get; set; } = new();
    public List<string> StatusOptions { get; set; } = new();
}

public sealed class SyncBillingIntakeResponse
{
    public int DiscoveredCount { get; set; }
}

/// <summary>
/// Fakta AR dari Billing yang belum menjadi piutang. Hitungan murni baca. Handoff lama yang sudah digantikan Berita
/// Acara Koreksi tidak dihitung: ia tidak boleh diakui sebagai piutang.
/// </summary>
public sealed class PendingArFactsResponse
{
    /// <summary>BilArHandoff berstatus CREATED yang belum tercatat sebagai fakta masuk (perlu POST billing-intake/sync).</summary>
    public int UndiscoveredCount { get; set; }

    /// <summary>Fakta masuk AR berstatus NEW (perlu POST billing-intake/process-pending-ar).</summary>
    public int NewIntakeCount { get; set; }

    /// <summary>Fakta masuk AR berstatus ERROR (dapat diulang setelah penyebabnya diperbaiki; lihat Pemantauan Fakta Masuk).</summary>
    public int ErrorIntakeCount { get; set; }
}

/// <summary>Satu fakta AR yang gagal diolah pada POST billing-intake/process-pending-ar.</summary>
public sealed class ProcessPendingArFailure
{
    public Guid IntakeId { get; set; }
    public Guid SourceHandoffKey { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>Hasil POST billing-intake/process-pending-ar. Kegagalan per fakta dilaporkan, tidak ditelan.</summary>
public sealed class ProcessPendingArResponse
{
    /// <summary>Jumlah fakta NEW yang dicoba pada panggilan ini (dibatasi per panggilan).</summary>
    public int AttemptedCount { get; set; }

    /// <summary>Fakta yang menjadi piutang (ACKNOWLEDGED).</summary>
    public int ProcessedCount { get; set; }

    /// <summary>Fakta handoff lama yang digantikan koreksi: ditutup tanpa membuat piutang (CONSUMED).</summary>
    public int SkippedSupersededCount { get; set; }

    public int FailedCount { get; set; }

    /// <summary>Fakta NEW yang masih tersisa; panggil lagi bila lebih dari nol.</summary>
    public int RemainingCount { get; set; }

    public List<ProcessPendingArFailure> Failures { get; set; } = new();
}
