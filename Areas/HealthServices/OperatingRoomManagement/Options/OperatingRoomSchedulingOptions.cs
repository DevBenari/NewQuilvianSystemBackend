namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Options;

/// <summary>
/// Buffer dan tim minimum penjadwalan operasi (OPS-DEC-016, OPS-DEC-017).
/// Nilai dikonfigurasi pada seksi <c>OperatingRoom:Scheduling</c>.
/// </summary>
public class OperatingRoomSchedulingOptions
{
    public const string SectionName = "OperatingRoom:Scheduling";

    /// <summary>Buffer persiapan sebelum irisan pertama bila permintaan tidak menyebutkan nilai.</summary>
    public int DefaultBufferBeforeMinutes { get; set; } = 15;

    /// <summary>Buffer pembersihan ruang setelah operasi bila permintaan tidak menyebutkan nilai.</summary>
    public int DefaultBufferAfterMinutes { get; set; } = 30;

    /// <summary>Durasi minimum satu blok jadwal.</summary>
    public int MinimumDurationMinutes { get; set; } = 15;

    /// <summary>Durasi maksimum satu blok jadwal.</summary>
    public int MaximumDurationMinutes { get; set; } = 1440;
}
