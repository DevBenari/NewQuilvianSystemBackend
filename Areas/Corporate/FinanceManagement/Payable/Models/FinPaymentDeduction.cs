using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;

/// <summary>
/// Rincian potongan dan tambahan pembayaran (FIN-DES-026, FIN-DES-027, Amendment A.3, FR-FIN-050).
/// Dipakai saat menyusun pembayaran rekap, terutama untuk honor tenaga medis/dokter kotor
/// yang terkena PPh 21, kasbon, iuran, potongan utang pasien yang dijamin potong honor,
/// atau tambahan sitting fee.
///
/// Invariant:
/// - Direction IN ('DEDUCTION','ADDITION') (CK_FinPaymentDeduction_Direction).
/// - Amount > 0 (CK_FinPaymentDeduction_Amount).
/// - DeductionType IN ('PPH21','KASBON','PATIENT_DEBT','SITTING_FEE','KSO','IURAN','OTHER') (CK_FinPaymentDeduction_Type).
/// - DeductionType != 'OTHER' OR Reason IS NOT NULL (CK_FinPaymentDeduction_OtherReason).
/// </summary>
[Table("FinPaymentDeduction", Schema = "public")]
public sealed class FinPaymentDeduction : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>FK ke induk pembayaran FinPayment.</summary>
    public Guid PaymentId { get; set; }
    public FinPayment? Payment { get; set; }

    /// <summary>PPH21, KASBON, PATIENT_DEBT, SITTING_FEE, KSO, IURAN, atau OTHER.</summary>
    [Required, MaxLength(30)] public string DeductionType { get; set; } = string.Empty;

    /// <summary>DEDUCTION (mengurangi transfer) atau ADDITION (menambah transfer).</summary>
    [Required, MaxLength(10)] public string Direction { get; set; } = FinPaymentDeductionDirections.Deduction;

    /// <summary>Selalu positif; arah ditentukan kolom Direction.</summary>
    public decimal Amount { get; set; }

    /// <summary>Keterangan, wajib diisi bila DeductionType = OTHER.</summary>
    [MaxLength(500)] public string? Reason { get; set; }

    /// <summary>Nomor bukti rujukan (misalnya nomor kasbon, nomor bukti setor iuran, dll.).</summary>
    [MaxLength(100)] public string? ReferenceNumber { get; set; }
}

public static class FinPaymentDeductionTypes
{
    public const string Pph21 = "PPH21";
    public const string Kasbon = "KASBON";
    public const string PatientDebt = "PATIENT_DEBT";
    public const string SittingFee = "SITTING_FEE";
    public const string Kso = "KSO";
    public const string Iuran = "IURAN";
    public const string Other = "OTHER";
}

public static class FinPaymentDeductionDirections
{
    public const string Deduction = "DEDUCTION";
    public const string Addition = "ADDITION";
}
