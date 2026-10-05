using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Piutang sewa non-pasien — parkir dan tenant (BE-FIN-056, FIN-DEC-099..104,
/// 02-backend-architecture.md AMENDMENT REVISI 13 §K). Aggregate root BERDIRI SENDIRI, TERPISAH
/// PENUH dari FinReceivable (FIN-DEC-101) — nol relasi database ke FinReceivable, FinReceipt,
/// atau BilInvoice. Invariant "piutang pasien wajib dari serah terima Billing" pada FinReceivable
/// TIDAK disentuh oleh entity ini sama sekali.
///
/// FIN-DEC-100: setiap periode dicatat manual oleh petugas AR, SATU baris per periode — TIDAK ADA
/// master kontrak sewa yang menerbitkan baris berikutnya otomatis. Risiko periode terlewat
/// diterima sadar oleh owner.
///
/// FIN-DEC-103: penghapusan (write-off) dan pembatalan (cancel) adalah SATU AKSI LANGSUNG oleh
/// staf AR, TANPA jenjang approval — berbeda sengaja dari FinReceivableWriteOff yang memakai
/// maker-checker. Kelonggaran ini HANYA berlaku untuk entity ini, MUST NOT dijadikan preseden
/// melonggarkan maker-checker piutang pasien.
/// </summary>
[Table("FinNonPatientReceivable", Schema = "public")]
public sealed class FinNonPatientReceivable : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string ReceivableNumber { get; set; } = string.Empty;

    /// <summary>FIN-DEC-104: PARKING atau TENANT — satu entity, kolom kategori.</summary>
    [Required, MaxLength(20)] public string Category { get; set; } = string.Empty;

    /// <summary>Nama penyewa. Teks bebas — FIN-DEC-100 meniadakan master penyewa.</summary>
    [Required, MaxLength(200)] public string CounterpartyName { get; set; } = string.Empty;

    /// <summary>Objek sewa (area parkir/unit tenant). Teks bebas, bukan rujukan master.</summary>
    [Required, MaxLength(200)] public string RentedObject { get; set; } = string.Empty;

    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public DateOnly DueDate { get; set; }

    public decimal BilledAmount { get; set; }

    /// <summary>FIN-DEC-102: diketik petugas, TIDAK PERNAH dihitung sistem.</summary>
    public decimal LateFeeAmount { get; set; } = 0;

    /// <summary>Dihitung dari BilledAmount + LateFeeAmount dikurangi jumlah pelunasan. Service
    /// ini (BE-FIN-057) satu-satunya penulisnya.</summary>
    public decimal OutstandingAmount { get; set; }

    [Required, MaxLength(20)] public string Status { get; set; } = FinNonPatientReceivableStatuses.Outstanding;

    [MaxLength(500)] public string? Note { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinNonPatientReceivableSettlement> Settlements { get; set; } = new List<FinNonPatientReceivableSettlement>();
}

public static class FinNonPatientReceivableCategories
{
    public const string Parking = "PARKING";
    public const string Tenant = "TENANT";
}

public static class FinNonPatientReceivableStatuses
{
    public const string Outstanding = "OUTSTANDING";
    public const string PartiallySettled = "PARTIALLY_SETTLED";
    public const string Settled = "SETTLED";
    public const string WrittenOff = "WRITTEN_OFF";
    public const string Cancelled = "CANCELLED";
}
