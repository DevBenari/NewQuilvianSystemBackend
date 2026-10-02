namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;

/// <summary>
/// Keadaan finansial sebuah resep menurut Billing, sebagaimana terbaca di layar kerja Farmasi
/// (PHA-BE-006, PHA-API-CLEARANCE-v1, PHA-DEC-069).
/// </summary>
/// <remarks>
/// <para>
/// Seluruh isinya <strong>keterangan, bukan kewenangan</strong>. Menampilkannya tidak memberi
/// siapa pun kemampuan mengubahnya, dan tidak ada permukaan untuk mengubahnya di Farmasi.
/// </para>
/// <para>
/// Resep yang keadaan finansialnya belum diketahui tetap dikembalikan beserta penanda belum
/// diketahui — bukan galat dan bukan <c>404</c>, karena resepnya sendiri ada dan sah.
/// </para>
/// </remarks>
public class PrescriptionFinancialClearanceResponse
{
    /// <summary>CLEARED, REVOKED, UNKNOWN, PENDING_VERIFICATION, atau STALE.</summary>
    public string ClearanceStatus { get; set; } = string.Empty;

    /// <summary>PAID, INSURANCE_APPROVED, atau PAYMENT_WAIVED. Kosong selain saat CLEARED.</summary>
    public string? FinancialOutcome { get; set; }

    /// <summary>Sebab perubahan terakhir menurut Billing.</summary>
    public string? ReasonCode { get; set; }

    /// <summary>Benar hanya bila Billing menyatakan resep beres secara finansial.</summary>
    public bool IsCleared { get; set; }

    /// <summary>Salah bila belum pernah ada surat untuk resep ini.</summary>
    public bool IsKnown { get; set; }

    /// <summary>
    /// Kode alasan pekerjaan tertahan, mengikuti <c>PHA-VAL-CLEARANCE-v1</c>. Kosong ketika
    /// pekerjaan tidak tertahan.
    /// </summary>
    public string? HoldReasonCode { get; set; }

    /// <summary>Alasan penahanan dalam kalimat yang dibaca petugas. Kosong bila tidak tertahan.</summary>
    public string? HoldReason { get; set; }

    /// <summary>Kesegaran salinan: NEVER_SYNCED, SYNCED, atau FAILED.</summary>
    public string SyncState { get; set; } = string.Empty;

    /// <summary>Kapan salinan terakhir berhasil diperbarui.</summary>
    public DateTimeOffset? SyncedAt { get; set; }

    /// <summary>Nomor versi surat terakhir yang tersalin.</summary>
    public long FinancialVersion { get; set; }

    /// <summary>Waktu berlaku menurut Billing.</summary>
    public DateTimeOffset? EffectiveAt { get; set; }
}
