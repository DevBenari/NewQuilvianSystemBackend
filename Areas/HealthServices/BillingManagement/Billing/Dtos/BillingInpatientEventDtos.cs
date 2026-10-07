namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;

/// <summary>
/// Isi satu ketukan pintu Rawat Inap ke Billing — kontrak <c>integrasi-billing</c> <c>1.1.0</c>
/// API 3.10 dan integrasi 4.2 (<c>INT-RWF-01</c>).
/// </summary>
/// <remarks>
/// Isinya <b>daftar putih</b> (<c>INV-RWF-05</c>): penanda kejadian saja. Tidak ada ruang, kelas,
/// waktu hunian, tarif, rupiah, maupun status kasir. Billing menghitung dari sumbernya sendiri
/// (linimasa penempatan bed dan master tarif), bukan dari salinan di dalam pesan.
/// </remarks>
public sealed class InpatientBillingEventEnvelope
{
    /// <summary><c>ADMISSION_CONFIRMED</c>, <c>BED_OCCUPIED</c>, <c>OCCUPANCY_CORRECTED</c>, atau <c>BED_RELEASED</c>.</summary>
    public string EventType { get; set; } = string.Empty;

    public Guid EpisodeId { get; set; }

    public Guid EncounterId { get; set; }

    /// <summary><c>ADMISSION</c>, <c>ROOM_STAY</c>, atau <c>DISCHARGE</c>.</summary>
    public string SourceType { get; set; } = string.Empty;

    public string SourceId { get; set; } = string.Empty;

    public int Version { get; set; }

    public DateTime OccurredAtUtc { get; set; }

    /// <summary>Format <c>INPATIENT:&lt;SourceType&gt;:&lt;SourceId&gt;:&lt;Version&gt;</c> (<c>RWI-DEC-161</c> butir 4).</summary>
    public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>
/// Tanda terima Billing atas satu ketukan pintu. Worker outbox Rawat Inap menandai pesan
/// <c>Published</c> <b>hanya</b> bila <see cref="Accepted"/> bernilai <c>true</c> (<c>INV-RWF-04</c>).
/// </summary>
public sealed class InpatientEventReceipt
{
    public bool Accepted { get; set; }

    /// <summary>Id <c>BilInpatientEventReceipt</c>; <see cref="Guid.Empty"/> bila pesan ditolak.</summary>
    public Guid ReceiptId { get; set; }

    /// <summary>Kosakata <c>BillingInpatientEventOutcomes</c>.</summary>
    public string Outcome { get; set; } = string.Empty;

    /// <summary>Keterangan teknis singkat, tanpa data klinis dan tanpa rupiah.</summary>
    public string? Message { get; set; }
}

/// <summary>Kosakata event ketukan pintu Rawat Inap — kontrak <c>integrasi-billing</c> <c>1.1.0</c> integrasi 4.2.</summary>
public static class InpatientBillingEventTypes
{
    public const string AdmissionConfirmed = "ADMISSION_CONFIRMED";
    public const string BedOccupied = "BED_OCCUPIED";
    public const string OccupancyCorrected = "OCCUPANCY_CORRECTED";
    public const string BedReleased = "BED_RELEASED";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        AdmissionConfirmed, BedOccupied, OccupancyCorrected, BedReleased
    };

    /// <summary>Tiga event yang memicu hitung ulang tarif kamar.</summary>
    public static readonly IReadOnlySet<string> RoomStayEvents = new HashSet<string>(StringComparer.Ordinal)
    {
        BedOccupied, OccupancyCorrected, BedReleased
    };
}

/// <summary>Kosakata <c>SourceType</c> ketukan pintu Rawat Inap.</summary>
public static class InpatientBillingEventSourceTypes
{
    public const string Admission = "ADMISSION";
    public const string RoomStay = "ROOM_STAY";
    public const string Discharge = "DISCHARGE";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        Admission, RoomStay, Discharge
    };
}
