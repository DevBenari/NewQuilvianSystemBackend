using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

public sealed class CreateWriteOffRequest
{
    public Guid InvoiceId { get; set; }
    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Amount { get; set; }
    public Guid ExpectedInvoiceRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }

    // BE-BKC-029/BKC-DEC-080: opsional, bawaan "PATIENT_AR" bila tidak dikirim/kosong (BKC-DES-014
    // - konsumen lama tidak rusak oleh amendment ini). Hanya menerima dua nilai terdaftar
    // (PATIENT_AR, NON_BILLABLE_RESIDUAL); teks asing DITOLAK 422 (BIL-VAL-042), bukan diam-diam
    // diperlakukan sebagai bawaan.
    [MaxLength(30)] public string? Category { get; set; }

    // BE-BKC-029/BIL-VAL-041: penanda niat pengaju saat pengajuan. Kategori NON_BILLABLE_RESIDUAL
    // MUST NOT ditandai pelunasan penuh - selisih yang tidak dapat ditagihkan bukan piutang pasien,
    // sehingga tidak ada yang "dilunasi". Kategori PATIENT_AR tidak dipengaruhi field ini -
    // IsFullSettlement-nya tetap diturunkan server dari sisa outstanding saat approval, persis
    // seperti sebelum amendment ini (regresi wajib nol).
    public bool IsFullSettlement { get; set; }
}

public sealed class WriteOffApprovalRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public sealed class WriteOffResponse
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public bool IsFullSettlement { get; set; }
    public decimal OutstandingBefore { get; set; }
    public decimal OutstandingAfter { get; set; }
    // BE-BKC-029: selalu terisi. PATIENT_AR/NON_BILLABLE_RESIDUAL menentukan plafon apa yang
    // OutstandingBefore/After di atas rujuk - piutang pasien untuk yang pertama, sisa selisih
    // tidak dapat ditagihkan untuk yang kedua.
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid CorrelationId { get; set; }
    public Guid RowVersion { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public bool IsReplay { get; set; }
}
