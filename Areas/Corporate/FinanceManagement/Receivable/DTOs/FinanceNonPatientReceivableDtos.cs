using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

public sealed class NonPatientReceivableQuery
{
    public string? Category { get; set; }
    public string? Status { get; set; }
    public DateOnly? PeriodStart { get; set; }
    public DateOnly? PeriodEnd { get; set; }
    public DateOnly? DueDateFrom { get; set; }
    public DateOnly? DueDateTo { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "dueDate";
    public string SortDirection { get; set; } = "asc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class NonPatientReceivableAgingQuery
{
    public DateOnly? AsOfDate { get; set; }
    public string? Category { get; set; }
}

/// <summary>Field persis tidak dirinci kontrak (api-contract.md §E.1 baris `GET /summary`
/// hanya menyebut nama DTO) — diisi mengikuti pola saringan Category yang sama persis dengan
/// `GET /aging` pada kapabilitas ini, bukan kebijakan baru.</summary>
public sealed class NonPatientReceivableSummaryQuery
{
    public string? Category { get; set; }
}

public sealed class CreateNonPatientReceivableRequest
{
    [Required(ErrorMessage = "Jenis sewa wajib diisi.")]
    public string Category { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama penyewa wajib diisi.")]
    [MaxLength(200)]
    public string CounterpartyName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Objek sewa wajib diisi.")]
    [MaxLength(200)]
    public string RentedObject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Awal periode wajib diisi.")]
    public DateOnly PeriodStart { get; set; }

    [Required(ErrorMessage = "Akhir periode wajib diisi.")]
    public DateOnly PeriodEnd { get; set; }

    [Required(ErrorMessage = "Tanggal jatuh tempo wajib diisi.")]
    public DateOnly DueDate { get; set; }

    [Required(ErrorMessage = "Nominal tagihan wajib diisi.")]
    public decimal BilledAmount { get; set; }

    public decimal LateFeeAmount { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}

public sealed class UpdateNonPatientReceivableRequest : CreateNonPatientReceivableRequest
{
    public Guid ExpectedRowVersion { get; set; }
}

public sealed class CreateNonPatientReceivableSettlementRequest
{
    public Guid ExpectedRowVersion { get; set; }

    [Required(ErrorMessage = "Tanggal pelunasan wajib diisi.")]
    public DateOnly SettlementDate { get; set; }

    [Required(ErrorMessage = "Nominal pelunasan wajib diisi.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Metode pembayaran wajib diisi.")]
    [MaxLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}

public sealed class WriteOffNonPatientReceivableRequest
{
    public Guid ExpectedRowVersion { get; set; }

    [Required(ErrorMessage = "Alasan wajib diisi.")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

public sealed class CancelNonPatientReceivableRequest
{
    public Guid ExpectedRowVersion { get; set; }

    [Required(ErrorMessage = "Alasan wajib diisi.")]
    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

public sealed class NonPatientReceivableResponse
{
    public Guid Id { get; set; }
    public string ReceivableNumber { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string CounterpartyName { get; set; } = string.Empty;
    public string RentedObject { get; set; } = string.Empty;
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal BilledAmount { get; set; }
    public decimal LateFeeAmount { get; set; }
    public decimal TotalBilledAmount { get; set; }
    public decimal SettledAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public int DaysPastDue { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Note { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class NonPatientReceivableDetailResponse
{
    public NonPatientReceivableResponse Receivable { get; set; } = new();
    public List<SettlementRowResponse> Settlements { get; set; } = new();
}

public sealed class SettlementRowResponse
{
    public Guid Id { get; set; }
    public DateOnly SettlementDate { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? ReferenceNumber { get; set; }
    public string? Note { get; set; }
}

public sealed class NonPatientReceivableSummaryResponse
{
    public int TotalReceivable { get; set; }
    public int OutstandingCount { get; set; }
    public int PartiallySettledCount { get; set; }
    public int SettledCount { get; set; }
    public int WrittenOffCount { get; set; }
    public int CancelledCount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
}

public sealed class NonPatientReceivableFilterMetadataResponse
{
    public List<int> PageSizeOptions { get; set; } = new();
    public List<string> SortableFields { get; set; } = new();
    public List<string> CategoryOptions { get; set; } = new();
    public List<string> StatusOptions { get; set; } = new();
}
