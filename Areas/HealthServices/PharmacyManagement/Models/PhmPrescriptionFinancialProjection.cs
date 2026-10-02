using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;

/// <summary>
/// Salinan keadaan finansial sebuah resep menurut Billing (PHA-DEC-063, PHA-DES-001, PHA-DEC-071).
/// </summary>
/// <remarks>
/// <para>
/// Tabel ini <strong>bukan</strong> sumber kebenaran. Kebenaran finansial milik Billing; yang
/// disimpan di sini hanya keadaan terkini beserta nomor versinya, cukup untuk menolak surat basi
/// dan cukup untuk direkonsiliasi kapan saja. Tidak ada petugas Farmasi yang boleh mengubahnya,
/// dan tidak ada permukaan untuk itu (PHA-DEC-067).
/// </para>
/// <para>
/// Tepat satu baris per resep, bukan riwayat. Riwayat suratnya sudah dijaga permanen di sisi
/// Billing (BKC-DEC-109); menyalinnya ke sini hanya menciptakan salinan kedua yang dapat
/// menyimpang, lalu menimbulkan pertanyaan mana yang benar ketika keduanya berbeda.
/// </para>
/// <para>
/// Nol kolom klinis. Tidak ada nama obat, dosis, aturan pakai, diagnosis, maupun nama pasien —
/// surat dari Billing memang hanya membawa identitas dan keadaan.
/// </para>
/// <para>
/// <c>InvoiceId</c> dan <c>SourceHandoffId</c> <strong>sengaja bukan foreign key</strong>. Keduanya
/// menunjuk baris milik modul lain; mengunci skema lintas modul justru yang dihindari rancangan
/// surat ini.
/// </para>
/// </remarks>
[Table("PhmPrescriptionFinancialProjection", Schema = "public")]
public class PhmPrescriptionFinancialProjection : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Resep yang keadaannya disalin. Tepat satu baris per resep.</summary>
    public Guid PrescriptionId { get; set; }

    /// <summary>Identitas tagihan di Billing, tanpa foreign key lintas modul.</summary>
    public Guid InvoiceId { get; set; }

    /// <summary>
    /// CLEARED, REVOKED, UNKNOWN, PENDING_VERIFICATION, atau STALE. Hanya CLEARED yang menjadi
    /// izin; empat sisanya MUST NOT diperlakukan sebagai izin (PHA-DEC-067).
    /// </summary>
    [Required, MaxLength(25)]
    public string ClearanceStatus { get; set; } = PrescriptionClearanceProjectionStatuses.Unknown;

    /// <summary>
    /// PAID, INSURANCE_APPROVED, atau PAYMENT_WAIVED. Kosong selain saat CLEARED. Nilainya
    /// disalin apa adanya dari surat — Farmasi tidak menyimpulkannya sendiri (PHA-DEC-065).
    /// </summary>
    [MaxLength(30)]
    public string? FinancialOutcome { get; set; }

    /// <summary>Sebab perubahan terakhir, disalin apa adanya dari surat.</summary>
    [MaxLength(40)]
    public string? ReasonCode { get; set; }

    /// <summary>
    /// Nomor versi surat terakhir yang diterima. Surat bernomor lebih rendah ditolak diam-diam;
    /// itu keadaan normal, bukan kesalahan (PHA_CLR_STALE_VERSION).
    /// </summary>
    public long FinancialVersion { get; set; }

    /// <summary>Waktu berlaku menurut Billing, bukan waktu disalin.</summary>
    public DateTimeOffset? EffectiveAt { get; set; }

    /// <summary>Surat yang menghasilkan keadaan ini. Sengaja bukan foreign key.</summary>
    public Guid? SourceHandoffId { get; set; }

    /// <summary>NEVER_SYNCED, SYNCED, atau FAILED.</summary>
    [Required, MaxLength(30)]
    public string SyncState { get; set; } = PrescriptionClearanceSyncStates.NeverSynced;

    /// <summary>Kapan salinan terakhir berhasil diperbarui.</summary>
    public DateTimeOffset? SyncedAt { get; set; }

    /// <summary>Kapan percobaan terakhir dilakukan, berhasil atau tidak.</summary>
    public DateTimeOffset? LastAttemptAt { get; set; }

    /// <summary>Jumlah percobaan gagal berturut-turut.</summary>
    public int RetryCount { get; set; }

    /// <summary>Sebab kegagalan terakhir. MUST NOT memuat data klinis.</summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>Rantai telusur ke peristiwa di Billing.</summary>
    public Guid? CorrelationId { get; set; }

    /// <summary>Kendali konkurensi optimistik.</summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public PhmPrescription Prescription { get; set; } = null!;
}

/// <summary>
/// Keadaan salinan finansial di sisi Farmasi (PHA-STATE-CLEARANCE-v1).
/// </summary>
/// <remarks>
/// Tiga nilai pertama menyalin keadaan yang dikirim Billing. Dua terakhir milik Farmasi sendiri:
/// keduanya menandai salinan yang tidak dapat dipercaya, dan keduanya menolak seluruh gerbang.
/// Nilainya dituliskan tersendiri di sini, bukan meminjam konstanta Billing, supaya kolom milik
/// Farmasi tidak ikut berubah arti ketika modul lain menambah keadaannya sendiri.
/// </remarks>
public static class PrescriptionClearanceProjectionStatuses
{
    public const string Cleared = "CLEARED";
    public const string Revoked = "REVOKED";
    public const string Unknown = "UNKNOWN";
    public const string PendingVerification = "PENDING_VERIFICATION";
    public const string Stale = "STALE";
}

/// <summary>Kesegaran salinan terhadap Billing (PHA-DES-001).</summary>
public static class PrescriptionClearanceSyncStates
{
    public const string NeverSynced = "NEVER_SYNCED";
    public const string Synced = "SYNCED";
    public const string Failed = "FAILED";
}
