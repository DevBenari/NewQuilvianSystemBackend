using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

/// <summary>
/// Aggregate root utang jasa tenaga medis (FIN-DES-025, FIN-CAP-021, 02-backend-architecture.md §A.5).
/// Menggantikan rencana tabel FinDoctorPayable (revisi 1) agar dapat melayani dokter, perawat,
/// dan tenaga kesehatan lain tanpa membedakan tabel fisik. Jenis penerima dibedakan lewat
/// kolom PayeeType (DOCTOR, NURSE, OTHER_PRACTITIONER).
///
/// Invariant:
/// 1. OriginalAmount = OutstandingAmount + PaidAmount + AdjustedAmount (pola seimbang FIN-VAL-011).
/// 2. SourceMedicalServiceFeeId unik: satu hasil jasa yang disetujui menghasilkan paling banyak satu utang.
/// 3. PayeeType menentukan makna PayeeReferenceId (keduanya sensitif).
/// 4. OutstandingAmount MUST NOT negatif.
/// </summary>
[Table("FinMedicalServicePayable", Schema = "public")]
public sealed class FinMedicalServicePayable : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string PayableNumber { get; set; } = string.Empty;

    /// <summary>DOCTOR, NURSE, OTHER_PRACTITIONER (FIN-DES-025, CK_FinMedicalServicePayable_PayeeType).</summary>
    [Required, MaxLength(30)] public string PayeeType { get; set; } = FinMedicalServicePayeeTypes.Doctor;

    /// <summary>Id tenaga medis penerima jasa (sensitif, privasi terjaga).</summary>
    public Guid PayeeReferenceId { get; set; }

    /// <summary>FK logis ke hasil jasa Medical Fee (MdfServiceFee / MdfFinanceHandoff) — satu hasil jasa = satu utang.</summary>
    public Guid SourceMedicalServiceFeeId { get; set; }

    /// <summary>Rujukan opsional ke Id BilApHandoff kesiapan penyerahan.</summary>
    public Guid? SourceApHandoffId { get; set; }

    /// <summary>Kode periode perhitungan jasa, contoh '2026-08'.</summary>
    [Required, MaxLength(20)] public string PeriodCode { get; set; } = string.Empty;

    /// <summary>Jasa kotor disalin dari Medical Fee (MF-DEC-005).</summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>Sisa utang yang belum terbayar atau terpotong lunas.</summary>
    public decimal OutstandingAmount { get; set; }

    /// <summary>Berkurang sebesar alokasi pelunasan (FinPaymentAllocation), bukan sebesar transfer bersih.</summary>
    public decimal PaidAmount { get; set; } = 0m;

    /// <summary>Bersih dari koreksi yang disetujui (FinPayableAdjustment).</summary>
    public decimal AdjustedAmount { get; set; } = 0m;

    [Required, MaxLength(30)] public string Status { get; set; } = FinMedicalServicePayableStatuses.Outstanding;

    /// <summary>Waktu utang jasa diakui di sistem Finance.</summary>
    public DateTimeOffset RecognizedAt { get; set; } = DateTimeOffset.UtcNow;

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinMedicalServicePayableItem> Items { get; set; } = new List<FinMedicalServicePayableItem>();
    public ICollection<FinPaymentAllocation> PaymentAllocations { get; set; } = new List<FinPaymentAllocation>();
    public ICollection<FinPayableAdjustment> Adjustments { get; set; } = new List<FinPayableAdjustment>();
}

public static class FinMedicalServicePayableStatuses
{
    public const string Outstanding = "OUTSTANDING";
    public const string Partial = "PARTIAL";
    public const string Paid = "PAID";
    public const string Cancelled = "CANCELLED";
}

public static class FinMedicalServicePayeeTypes
{
    public const string Doctor = "DOCTOR";
    public const string Nurse = "NURSE";
    public const string OtherPractitioner = "OTHER_PRACTITIONER";
}
