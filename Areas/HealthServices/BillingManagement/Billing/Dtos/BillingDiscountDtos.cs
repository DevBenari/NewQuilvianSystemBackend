using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class ApplyDiscountRequest
{
    public Guid DiscountPolicyId { get; set; }
    [MaxLength(50)] public string? VoucherCode { get; set; }
    public Guid? InvoiceItemId { get; set; }
    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal? RequestedAmount { get; set; }
    public Guid ExpectedRowVersion { get; set; }
    [MaxLength(500)] public string Reason { get; set; } = string.Empty;
    [MaxLength(500)] public string? DoctorDiscountMemoFile { get; set; }
}

public sealed class ApproveDiscountRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    [MaxLength(500)] public string? DoctorDiscountMemoFile { get; set; }
}

public sealed class CancelDiscountRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public sealed class DiscountResponse
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid? InvoiceItemId { get; set; }
    public Guid DiscountPolicyId { get; set; }
    public string PolicyCode { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public string TargetComponent { get; set; } = string.Empty;
    public decimal RequestedAmount { get; set; }
    public decimal Amount { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;
    public Guid RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? DoctorDiscountMemoFile { get; set; }
    public bool IsEffective { get; set; }
    public bool RequiresFinanceApproval { get; set; }
    public Guid InvoiceRowVersion { get; set; }
    public DateTime CreateDateTime { get; set; }
    public DateTime? UpdateDateTime { get; set; }

    /// <summary>BE fix (orchestration Apply Promo): total invoice terbaru setelah diskon ini
    /// benar-benar efektif - reuse penuh bentuk <see cref="CalculationResponse"/> yang sama
    /// dengan endpoint Recalculate, bukan field baru yang bersaing (mis. RemainingAmount/
    /// OutstandingAmount/SisaBayar terpisah). <c>null</c> ketika diskon belum efektif (jasa
    /// dokter masih PendingDoctor/PendingFinance) atau saat baris ini muncul di listing riwayat
    /// diskon lama (BillingInvoiceService), bukan hasil aksi Apply/Approve saat ini.</summary>
    public CalculationResponse? Calculation { get; set; }
}

public sealed class DiscountCalculationResponse
{
    public Guid DiscountApplicationId { get; set; }
    public Guid DiscountPolicyId { get; set; }
    public string PolicyCode { get; set; } = string.Empty;
    public string DiscountType { get; set; } = string.Empty;
    public string TargetComponent { get; set; } = string.Empty;
    public Guid? InvoiceItemId { get; set; }
    public string ValueType { get; set; } = string.Empty;
    public decimal PolicyValue { get; set; }
    public decimal? PolicyLimit { get; set; }
    public decimal BasisAmount { get; set; }
    public decimal AppliedAmount { get; set; }
}

// Antrean approval Diskon Dokter untuk layar dokter yang berdiri sendiri. Dokter menyetujui
// pengajuan tanpa harus membuka Menu Pembayaran milik kasir, sehingga invoice yang terkunci oleh
// diskon pending (BKC-DEC-046) punya jalur penyelesaian sendiri.
public sealed class DoctorDiscountApprovalQuery
{
    public string? Search { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class DoctorDiscountApprovalResponse
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public Guid InvoiceRowVersion { get; set; }
    public Guid EncounterId { get; set; }
    public string EncounterNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string PolicyCode { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? DoctorDiscountMemoFile { get; set; }
    public Guid RequestedBy { get; set; }
    public string? RequestedByName { get; set; }
    public DateTime CreateDateTime { get; set; }

    // Pengaju tidak boleh menyetujui pengajuannya sendiri (aturan yang sama ditegakkan
    // BillingDiscountService.ApproveDoctorAsync). Ditandai di sini supaya barisnya tetap terlihat
    // dokter beserta alasannya, bukan hilang diam-diam dari antrean.
    public bool CanApprove { get; set; }
    public string? BlockedReason { get; set; }
}
