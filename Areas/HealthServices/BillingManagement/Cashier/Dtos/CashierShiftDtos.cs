using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Dtos;

public sealed class CashierShiftQuery
{
    public Guid? CashierId { get; set; }
    public Guid? RegisterId { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset? OpenedFrom { get; set; }
    public DateTimeOffset? OpenedTo { get; set; }
    public string? Search { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class OpenShiftRequest
{
    public Guid RegisterId { get; set; }
    // Batas Range wajib di-parse invariant: tanpa ini RangeAttribute memakai culture aktif
    // server, dan pada culture yang memakai koma sebagai pemisah desimal string "9999999999999999.99"
    // gagal di-parse sehingga validasi melempar dan request berakhir 500.
    [Range(
        typeof(decimal),
        "0",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal OpeningCash { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class HandoverShiftRequest
{
    public Guid ReceivingCashierId { get; set; }
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class CloseShiftRequest
{
    // Batas Range wajib di-parse invariant: tanpa ini RangeAttribute memakai culture aktif
    // server, dan pada culture yang memakai koma sebagai pemisah desimal string "9999999999999999.99"
    // gagal di-parse sehingga validasi melempar dan request berakhir 500.
    [Range(
        typeof(decimal),
        "0",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal PhysicalCash { get; set; }
    public Guid ExpectedRowVersion { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class ReviewVarianceRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Resolution { get; set; } = string.Empty;
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class ReopenShiftRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

// Dipakai layar "Konfirmasi Terima Shift" untuk mengganti input Shift ID/Row Version manual
// dengan daftar pencarian - satu baris per handover PENDING yang IncomingCashierId-nya adalah
// pemanggil endpoint. ShiftRowVersion sengaja diambil dari BilCashierShift.RowVersion (bukan
// RowVersion milik handover-nya sendiri), karena itulah nilai yang divalidasi HandoverAsync
// (EnsureCurrent) saat konfirmasi dikirim.
public sealed class CashierShiftPendingHandoverResponse
{
    public Guid ShiftId { get; set; }
    public string ShiftNumber { get; set; } = string.Empty;
    public Guid ShiftRowVersion { get; set; }
    public Guid OutgoingCashierId { get; set; }
    public string OutgoingCashierName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset InitiatedAt { get; set; }
}

// Dipakai layar "Ajukan Handover" untuk mengganti input ID Kasir Penerima manual dengan daftar
// pencarian - hanya user aktif yang saat ini TIDAK memiliki shift aktif (kandidat valid sesuai
// aturan HandoverAsync: "Kasir penerima masih memiliki shift aktif" akan selalu ditolak server
// jika dipaksakan), dan bukan diri sendiri.
public sealed class CashierUserOptionResponse
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
}

public sealed class CashierShiftResponse
{
    public Guid Id { get; set; }
    public string ShiftNumber { get; set; } = string.Empty;
    public Guid CashierId { get; set; }
    public Guid RegisterId { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal SystemCash { get; set; }
    public decimal ExpectedClosingCash { get; set; }
    public decimal PhysicalCash { get; set; }
    public decimal Variance { get; set; }
    public string Status { get; set; } = CashierShiftStatuses.Open;
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid RowVersion { get; set; }
    public bool VarianceRequiresReview { get; set; }
    public string? PendingHandoverStatus { get; set; }
    public Guid? ReceivingCashierId { get; set; }
    public bool IsReplay { get; set; }
}

public sealed class CashVarianceResponse
{
    public Guid Id { get; set; }
    public Guid ShiftId { get; set; }
    public Guid ReviewerId { get; set; }
    public decimal Variance { get; set; }
    public string Resolution { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset ReviewedAt { get; set; }
    public CashierShiftResponse Shift { get; set; } = new();
    public bool IsReplay { get; set; }
}
