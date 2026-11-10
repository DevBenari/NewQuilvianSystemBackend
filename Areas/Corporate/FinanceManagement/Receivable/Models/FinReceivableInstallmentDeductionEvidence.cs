using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Penghubung bukti potong gaji (laporan penggajian) ke SATU baris jadwal angsuran. Berkasnya TIDAK disimpan di sini:
/// ia tetap satu baris <see cref="FinTransactionProof"/> (jenis LAPORAN-PENGGAJIAN) di penyimpanan berkas yang sama dengan
/// batch hasil potongan Finance, sehingga satu laporan penggajian yang mencakup banyak pegawai cukup diunggah sekali dan
/// dirujuk banyak baris ini.
///
/// BUKAN pencatatan pembayaran: baris ini tidak pernah menyentuh FinReceivable.OutstandingAmount/AllocatedAmount,
/// FinReceivableInstallment.PaidAmount, maupun buku mutasi. Saldo hanya berkurang lewat
/// FinanceReceivablePayrollSyncService.ApplyResultCoreAsync (hasil payroll/batch yang sah). <see cref="DeductedAmount"/>
/// adalah angka yang TERTERA pada dokumen bukti — pembanding bagi verifikator, bukan sumber saldo.
/// </summary>
[Table("FinReceivableInstallmentDeductionEvidence", Schema = "public")]
public sealed class FinReceivableInstallmentDeductionEvidence : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid InstallmentId { get; set; }
    public FinReceivableInstallment? Installment { get; set; }

    /// <summary>Denormalisasi dari angsuran supaya bukti dapat dibaca per perjanjian/piutang tanpa join berantai.</summary>
    public Guid PlanId { get; set; }
    public Guid ReceivableId { get; set; }

    /// <summary>Identitas canonical pemilik benefit saat bukti diunggah. Null bila piutang lama belum terpetakan.</summary>
    public Guid? WorkforceProfileId { get; set; }

    [Required, MaxLength(7)] public string DeductionPeriod { get; set; } = string.Empty;

    /// <summary>Rujukan logis ke MstPayrollPeriod.Id; tersedia bila bukti berasal dari periode payroll yang terdaftar.</summary>
    public Guid? PayrollPeriodId { get; set; }

    /// <summary>Rujukan logis ke run payroll bila bukti berasal dari mesin payroll.</summary>
    public Guid? PayrollRunId { get; set; }

    /// <summary>Nomor laporan penggajian sebagaimana tertera pada dokumen (dari payroll, bukan nomor yang dibangkitkan sistem).</summary>
    [Required, MaxLength(100)] public string DocumentNumber { get; set; } = string.Empty;

    public Guid ProofId { get; set; }
    public FinTransactionProof? Proof { get; set; }

    /// <summary>BERHASIL, SEBAGIAN, GAGAL — hasil yang tertera pada dokumen (FinPayrollDeductionItemResultStatuses).</summary>
    [Required, MaxLength(10)] public string ResultStatus { get; set; } = FinPayrollDeductionItemResultStatuses.Berhasil;

    /// <summary>Nominal terpotong yang tertera pada dokumen. Informasi pembanding; bukan sumber saldo.</summary>
    public decimal DeductedAmount { get; set; }

    public DateTimeOffset? DeductionDate { get; set; }

    /// <summary>Alasan potongan gagal/sebagian bila tertera pada dokumen.</summary>
    [MaxLength(500)] public string? Notes { get; set; }

    [Required, MaxLength(30)] public string Status { get; set; } = FinInstallmentDeductionEvidenceStatuses.Diunggah;

    public Guid UploadedBy { get; set; }
    public DateTimeOffset UploadedAt { get; set; }

    public Guid? VerifiedBy { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }

    /// <summary>Alasan penolakan, atau catatan verifikator.</summary>
    [MaxLength(500)] public string? VerificationNotes { get; set; }

    [ConcurrencyCheck] public Guid RowVersion { get; set; } = Guid.NewGuid();
}

/// <summary>
/// Status bukti yang tersimpan. BELUM_ADA_BUKTI tidak disimpan — ia adalah ketiadaan baris. MENUNGGU_VERIFIKASI tidak
/// dipisah dari BUKTI_DIUNGGAH: setiap bukti yang diunggah otomatis menunggu verifikasi, jadi memisahkannya hanya
/// menghasilkan dua status yang selalu bersamaan.
/// </summary>
public static class FinInstallmentDeductionEvidenceStatuses
{
    public const string None = "BELUM_ADA_BUKTI";
    public const string Diunggah = "BUKTI_DIUNGGAH";
    public const string Terverifikasi = "TERVERIFIKASI";
    public const string Ditolak = "DITOLAK";
}
