using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Pelunasan atas satu FinNonPatientReceivable (BE-FIN-056, FIN-DES-075). Baris pelunasan TIDAK
/// PERNAH dihapus — pembetulan pelunasan keliru dicatat sebagai baris baru bernilai negatif,
/// mengikuti pola "tidak pernah menghapus, selalu menambah baris" yang berlaku di seluruh
/// blueprint ini.
///
/// FIN-DES-075: entity ini BELUM tersambung ke kas harian, setoran bank, maupun kotak keluar
/// kejadian akuntansi — pelunasan yang dicatat di sini mengurangi OutstandingAmount piutang
/// sewa, tetapi TIDAK tercatat sebagai kas masuk di mana pun selama FIN-OQ-044 belum diputuskan.
/// Ini konsekuensi sah dari memisahkan jalur piutang sewa dari jalur penerimaan Billing
/// (FinReceipt tidak punya jalur manual), BUKAN cacat implementasi.
/// </summary>
[Table("FinNonPatientReceivableSettlement", Schema = "public")]
public sealed class FinNonPatientReceivableSettlement : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid NonPatientReceivableId { get; set; }
    public FinNonPatientReceivable? NonPatientReceivable { get; set; }

    public DateOnly SettlementDate { get; set; }

    /// <summary>Boleh negatif — membatalkan/membetulkan pelunasan sebelumnya yang keliru.</summary>
    public decimal Amount { get; set; }

    [Required, MaxLength(50)] public string PaymentMethod { get; set; } = string.Empty;

    [MaxLength(100)] public string? ReferenceNumber { get; set; }

    [MaxLength(500)] public string? Note { get; set; }
}
