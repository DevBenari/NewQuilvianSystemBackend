using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

/// <summary>
/// Rincian alokasi pembayaran ke utang tertentu (FIN-DES-015, FIN-DES-016, Amendment A.5, 02-backend-architecture.md §4.14).
/// Polimorfik lewat PayableType: tepat satu dari SupplierPayableId/MedicalServicePayableId terisi
/// dan MUST cocok dengan PayableType (ditegakkan check constraint CK_FinPaymentAllocation_ExactlyOneTarget).
///
/// MedicalServicePayableId disiapkan TANPA foreign key constraint karena FinMedicalServicePayable
/// belum dibangun (BE-FIN-021, BLOCKED menunggu modul Medical Fee) — mengikuti preseden yang sama
/// dengan FinPayableAdjustment (BE-FIN-019). Menambahkan FK constraint-nya nanti adalah operasi
/// ALTER TABLE ADD CONSTRAINT aditif murni setelah tabelnya ada.
/// </summary>
[Table("FinPaymentAllocation", Schema = "public")]
public sealed class FinPaymentAllocation : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>FK ke induk pembayaran FinPayment.</summary>
    public Guid PaymentId { get; set; }
    public FinPayment? Payment { get; set; }

    /// <summary>SUPPLIER atau MEDICAL_SERVICE (Amendment A.5).</summary>
    [Required, MaxLength(30)] public string PayableType { get; set; } = FinPaymentAllocationPayableTypes.Supplier;

    /// <summary>FK ke FinSupplierPayable bila PayableType = SUPPLIER.</summary>
    public Guid? SupplierPayableId { get; set; }
    public FinSupplierPayable? SupplierPayable { get; set; }

    /// <summary>FK ke FinMedicalServicePayable bila PayableType = MEDICAL_SERVICE (BE-FIN-021).</summary>
    public Guid? MedicalServicePayableId { get; set; }
    public FinMedicalServicePayable? MedicalServicePayable { get; set; }

    /// <summary>Nilai pelunasan utang, selalu positif (CK_FinPaymentAllocation_Amount).</summary>
    public decimal Amount { get; set; }

    /// <summary>Menandai baris alokasi pembalik.</summary>
    public bool IsReversal { get; set; } = false;

    /// <summary>FK self-reference ke baris alokasi yang dibalik bila IsReversal = true.</summary>
    public Guid? ReversalOfAllocationId { get; set; }
    public FinPaymentAllocation? ReversalOfAllocation { get; set; }
}

public static class FinPaymentAllocationPayableTypes
{
    public const string Supplier = "SUPPLIER";
    public const string MedicalService = "MEDICAL_SERVICE";
}
