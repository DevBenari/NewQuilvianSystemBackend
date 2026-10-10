using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Satu baris jadwal angsuran pada satu periode gaji (FIN-DES-099). Dibuat sekaligus sebanyak
/// InstallmentCount saat perjanjian induknya disetujui. Baris ini MUST NOT dihapus ketika potongan
/// gagal — ia ditandai TERTUNGGAK dan sisanya ikut ke baris berikutnya (FIN-DEC-168).
/// </summary>
[Table("FinReceivableInstallment", Schema = "public")]
public sealed class FinReceivableInstallment : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid PlanId { get; set; }
    public FinReceivableInstallmentPlan? Plan { get; set; }

    public int InstallmentNumber { get; set; }

    [Required, MaxLength(7)] public string DeductionPeriod { get; set; } = string.Empty;

    public decimal ScheduledAmount { get; set; }

    /// <summary>Tunggakan dari periode sebelumnya yang ikut dipotong (FIN-DEC-168).</summary>
    public decimal CarriedOverAmount { get; set; }

    /// <summary>Yang benar-benar terpotong. Bertambah bertahap bila potongan sebagian (FIN-DEC-179).</summary>
    public decimal PaidAmount { get; set; }

    /// <summary>MUST sama dengan ScheduledAmount + CarriedOverAmount - PaidAmount (CK_FinReceivableInstallment_Balance).</summary>
    public decimal OutstandingAmount { get; set; }

    [Required, MaxLength(30)] public string Status { get; set; } = FinReceivableInstallmentStatuses.Dijadwalkan;

    public DateTimeOffset? LastResultAt { get; set; }

    /// <summary>BE-FIN-095, FIN-VAL-240: PayrollPeriodId terakhir yang dikirim HR untuk baris ini —
    /// kunci idempotensi (InstallmentId, PayrollPeriodId). Kiriman ulang dengan PayrollPeriodId yang
    /// SAMA PERSIS adalah duplikat (dibalas 200 tanpa mengubah apa pun). Field ini tidak ada pada
    /// kamus data BE-FIN-092 (temuan drift, dicatat laporan task) — ditambahkan task ini karena
    /// FinReceivableInstallment belum pernah punya tempat menyimpan kunci idempotensi payroll.</summary>
    public Guid? LastPayrollPeriodId { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

public static class FinReceivableInstallmentStatuses
{
    public const string Dijadwalkan = "DIJADWALKAN";
    public const string TerbayarSebagian = "TERBAYAR_SEBAGIAN";
    public const string Terbayar = "TERBAYAR";
    public const string Tertunggak = "TERTUNGGAK";
    public const string Dibatalkan = "DIBATALKAN";
}
