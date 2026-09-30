using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;

/// <summary>
/// Potongan penerimaan piutang — PPh 23 atau biaya administrasi bank (FIN-DES-048, 049,
/// erd/data-dictionary.md §D.3-D.4(c), MENGGANTIKAN bentuk §C.15 yang belum pernah dibangun).
/// Melekat pada FinReceiptAllocation (bukan hanya FinReceipt) karena alokasilah yang menyebut
/// piutang mana yang dikurangi — tanpa itu tidak diketahui piutang mana yang benar-benar
/// berkurang saat satu penerimaan dialokasikan ke banyak piutang sekaligus.
///
/// Baris TIDAK PERNAH dihapus atau diubah nilainya. Potongan yang keliru dibetulkan dengan
/// membalik alokasinya (membuat baris IsReversal = true menunjuk baris asli lewat
/// ReversalOfDeductionId), lalu mencatat ulang — pola yang sama dengan FinReceiptAllocation
/// (FIN-DES-012). Amount SELALU positif, termasuk pada baris pembalik.
///
/// Belum ada penulis pada task ini — BE-FIN-038 murni entity+configuration+migration. Pemilihan
/// EventTypeCode kejadian Accounting dari DeductionType (mengikuti katalog final FIN-DEC-065:
/// POTONGAN-PPH23-PIUTANG / POTONGAN-BIAYA-BANK-PIUTANG, BUKAN POTONGAN-PIUTANG-NON-TUNAI yang
/// sudah superseded) adalah tanggung jawab service yang menyusul, BE-FIN-040.
/// </summary>
[Table("FinReceiptDeduction", Schema = "public")]
public sealed class FinReceiptDeduction : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string DeductionNumber { get; set; } = string.Empty;

    /// <summary>Induk penerimaan — disimpan untuk penyaringan cepat.</summary>
    public Guid ReceiptId { get; set; }
    public FinReceipt? Receipt { get; set; }

    /// <summary>Alokasi TargetType = RECEIVABLE tempat potongan melekat; menentukan piutang yang
    /// dikurangi (FIN-DES-048). Syarat ini tidak dapat dijaga check constraint dan MUST ditegakkan
    /// service pemanggil (FIN-VAL-129).</summary>
    public Guid ReceiptAllocationId { get; set; }
    public FinReceiptAllocation? ReceiptAllocation { get; set; }

    [Required, MaxLength(30)] public string DeductionType { get; set; } = string.Empty;

    /// <summary>Selalu positif, termasuk pada baris pembalik.</summary>
    public decimal Amount { get; set; }

    /// <summary>Wajib bila DeductionType = OTHER. Sensitif bila memuat keterangan pihak ketiga.</summary>
    [MaxLength(500)] public string? Reason { get; set; }

    [MaxLength(100)] public string? ReferenceNumber { get; set; }

    public bool IsReversal { get; set; } = false;

    /// <summary>Terisi hanya bila IsReversal = true. Satu baris hanya dibalik sekali (unique index parsial).</summary>
    public Guid? ReversalOfDeductionId { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

public static class FinReceiptDeductionTypes
{
    public const string Pph23 = "PPH23";
    public const string BankAdminFee = "BANK_ADMIN_FEE";
    public const string Other = "OTHER";
}
