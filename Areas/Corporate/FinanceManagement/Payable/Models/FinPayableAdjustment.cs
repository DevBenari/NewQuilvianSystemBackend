using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

/// <summary>
/// Koreksi nilai utang, maker-checker (FIN-DES-014, pola identik FinReceivableAdjustment —
/// 02-backend-architecture.md §4.15). Polimorfik lewat PayableType: tepat satu dari
/// SupplierPayableId/MedicalServicePayableId terisi dan MUST cocok dengan PayableType
/// (FIN-DES-016, ditegakkan check constraint).
///
/// MedicalServicePayableId disiapkan TANPA foreign key constraint karena FinMedicalServicePayable
/// belum dibangun (BE-FIN-021, BLOCKED menunggu modul Medical Fee) — kolom sudah disiapkan
/// sekarang mengikuti preseden "kolom disiapkan lebih dulu supaya skema tidak berubah lagi
/// nanti" (01-backend-roadmap.md bagian 5, EPIC FIN-04). Menambahkan FK constraint-nya adalah
/// pekerjaan aditif murni untuk BE-FIN-021 nanti (ALTER TABLE ADD CONSTRAINT setelah tabelnya
/// ada), bukan migrasi yang merusak.
///
/// Task ini (BE-FIN-019) HANYA membangun jalur PayableType = SUPPLIER; FinanceSupplierPayableService
/// tidak pernah membuat baris dengan PayableType = MEDICAL_SERVICE.
/// </summary>
[Table("FinPayableAdjustment", Schema = "public")]
public sealed class FinPayableAdjustment : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string AdjustmentNumber { get; set; } = string.Empty;

    [Required, MaxLength(30)] public string PayableType { get; set; } = FinPayableAdjustmentPayableTypes.Supplier;

    public Guid? SupplierPayableId { get; set; }
    public FinSupplierPayable? SupplierPayable { get; set; }

    /// <summary>FK ke FinMedicalServicePayable bila PayableType = MEDICAL_SERVICE (BE-FIN-021).</summary>
    public Guid? MedicalServicePayableId { get; set; }
    public FinMedicalServicePayable? MedicalServicePayable { get; set; }

    [Required, MaxLength(10)] public string Direction { get; set; } = string.Empty;

    /// <summary>Selalu positif; arah ditentukan Direction. Untuk utang: DEBIT mengurangi, CREDIT menambah (state-transition-matrix.md §5) — kebalikan konvensi piutang karena utang adalah akun liabilitas (double-entry normal balance).</summary>
    public decimal Amount { get; set; }

    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;

    [Required, MaxLength(30)] public string Status { get; set; } = FinPayableAdjustmentStatuses.Requested;

    public Guid RequestedBy { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    [MaxLength(500)] public string? RejectionReason { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

public static class FinPayableAdjustmentPayableTypes
{
    public const string Supplier = "SUPPLIER";
    public const string MedicalService = "MEDICAL_SERVICE";
}

public static class FinPayableAdjustmentDirections
{
    public const string Debit = "DEBIT";
    public const string Credit = "CREDIT";
}

/// <summary>Sama persis dengan FinReceivableApprovalStatuses (Receivable) — tidak dipakai bersama karena tabelnya berbeda domain.</summary>
public static class FinPayableAdjustmentStatuses
{
    public const string Requested = "REQUESTED";
    public const string Approved = "APPROVED";
    public const string Rejected = "REJECTED";
}
