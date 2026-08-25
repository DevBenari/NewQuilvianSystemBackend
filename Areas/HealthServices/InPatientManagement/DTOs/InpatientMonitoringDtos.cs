using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>Penyaring daftar pantau penempatan tidak sesuai kebutuhan isolasi.</summary>
    public class IsolationMismatchQuery
    {
        public Guid? ServiceUnitId { get; set; }

        public Guid? RoomId { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Satu episode yang kebutuhan isolasinya tidak cocok dengan sifat tempat tidur yang
    /// sedang ditempatinya.
    /// </summary>
    /// <remarks>
    /// <b>Daftar ini adalah pengganti penolakan, bukan pelengkapnya.</b> Ketika kondisi klinis
    /// berubah di tengah perawatan, pencatatannya tidak pernah ditahan: fakta klinis dicatat
    /// lebih dulu, lalu episodenya muncul di sini supaya penempatannya dibetulkan. Menahan
    /// pencatatan demi menjaga aturan penempatan adalah urutan yang terbalik —
    /// <c>RWI-RULE-012</c> bagian A aturan 7.
    /// </remarks>
    public class IsolationMismatchItemResponse
    {
        public Guid EpisodeId { get; set; }

        public string EpisodeNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public Guid BedId { get; set; }

        public string? BedCode { get; set; }

        public string? BedName { get; set; }

        public Guid RoomId { get; set; }

        public string? RoomName { get; set; }

        public Guid ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        public bool RequiresIsolation { get; set; }

        public bool IsIsolationBed { get; set; }

        /// <summary>
        /// Arah ketidakcocokannya. <c>NeedsIsolationBed</c> berarti pasien membutuhkan isolasi
        /// tetapi berada di tempat tidur biasa; <c>OccupiesIsolationBed</c> berarti sebaliknya,
        /// yaitu kapasitas isolasi terpakai pasien yang tidak membutuhkannya.
        /// </summary>
        public string MismatchKind { get; set; } = string.Empty;

        public string MismatchMessage { get; set; } = string.Empty;

        public DateTime PlacementStartDateTime { get; set; }

        public DateTime? IsolationSetAt { get; set; }
    }

    /// <summary>Daftar pantau penempatan tidak sesuai, bertingkat.</summary>
    public class IsolationMismatchPagedResult : PagedResult<IsolationMismatchItemResponse>
    {
    }

    /// <summary>Penyaring daftar pantau yang dikelompokkan menurut unit layanan.</summary>
    public class InpatientMonitoringQuery
    {
        public Guid? ServiceUnitId { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Satu episode yang sudah boleh pulang tetapi belum ditutup melewati ambang waktu.
    /// </summary>
    /// <remarks>
    /// Daftar ini lahir dari <c>RWI-RULE-010</c>: yang memutuskan pulang dan yang menutup
    /// episode adalah orang yang berbeda, sehingga episode dapat menggantung di
    /// <c>DischargePending</c> bila petugas admisi lalai. Tanpa daftar ini, satu-satunya cara
    /// menemukannya adalah menunggu ada yang mengeluh.
    /// </remarks>
    public class PendingClosureItemResponse
    {
        public Guid EpisodeId { get; set; }

        public string EpisodeNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public Guid ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        public string? BedName { get; set; }

        public string? RoomName { get; set; }

        public DateTime? DischargeDecidedAt { get; set; }

        public DateTime? PhysicallyLeftAt { get; set; }

        /// <summary>Lama menggantung dalam jam, dihitung sejak keputusan pulang.</summary>
        public int PendingHours { get; set; }

        /// <summary>Ambang yang berlaku saat daftar ini dibaca, dari master pengaturan.</summary>
        public int ThresholdHours { get; set; }

        /// <summary>Benar bila tempat tidurnya masih tertahan karena kepergian belum dicatat.</summary>
        public bool IsBedStillHeld { get; set; }
    }

    /// <summary>Daftar pantau penutupan tertunda, bertingkat.</summary>
    public class PendingClosurePagedResult : PagedResult<PendingClosureItemResponse>
    {
    }

    /// <summary>Satu episode yang ditutup menembus gerbang keuangan.</summary>
    public class OverrideClosureItemResponse
    {
        public Guid EpisodeId { get; set; }

        public string EpisodeNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }

        public string? PatientName { get; set; }

        public string? MedicalRecordNumber { get; set; }

        public Guid ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        public DateTime? ClosedAt { get; set; }

        public Guid ClosedByUserId { get; set; }

        /// <summary>Alasan supervisor menembus gerbang keuangan. Wajib terisi.</summary>
        public string? ClosedWithoutClearanceReason { get; set; }
    }

    /// <summary>Daftar pantau penutupan menembus gerbang keuangan, bertingkat.</summary>
    public class OverrideClosurePagedResult : PagedResult<OverrideClosureItemResponse>
    {
    }

    /// <summary>Daftar pantau episode aktif tanpa perawat penanggung jawab, bertingkat.</summary>
    public class UnassignedNursePagedResult : PagedResult<CensusItemResponse>
    {
    }

    /// <summary>
    /// Satu tempat tidur yang salinan statusnya tidak cocok dengan catatan penempatan.
    /// </summary>
    /// <remarks>
    /// <b>Ini satu-satunya pengawas atas satu-satunya arah tulis lintas modul.</b>
    /// <c>MstBed.BedStatus</c> adalah salinan; sumber kebenarannya adalah <c>InpBedPlacement</c>
    /// dan <c>InpBedReservation</c>. Bila laporan ini tidak pernah dibaca siapa pun, salinan
    /// itu akan menyimpang diam-diam dan tidak ada yang menyadarinya sampai seorang pasien
    /// ditempatkan di tempat tidur yang sudah ada orangnya. Ini soal proses, bukan kode.
    /// </remarks>
    public class BedDriftItemResponse
    {
        public Guid BedId { get; set; }

        public string BedCode { get; set; } = string.Empty;

        public string BedName { get; set; } = string.Empty;

        public Guid RoomId { get; set; }

        public string? RoomName { get; set; }

        public Guid ServiceUnitId { get; set; }

        public string? ServiceUnitName { get; set; }

        /// <summary>Nilai salinan pada <c>MstBed</c>.</summary>
        public int CopiedStatus { get; set; }

        public string CopiedStatusName { get; set; } = string.Empty;

        /// <summary>Nilai yang seharusnya, diturunkan dari catatan penempatan dan pemesanan.</summary>
        public int ExpectedStatus { get; set; }

        public string ExpectedStatusName { get; set; } = string.Empty;

        public bool HasActivePlacement { get; set; }

        public bool HasActiveReservation { get; set; }

        /// <summary>Nomor episode yang sedang memegang tempat tidur ini, bila memang ada.</summary>
        public string? HoldingEpisodeNumber { get; set; }

        public string DriftMessage { get; set; } = string.Empty;
    }

    /// <summary>Laporan selisih salinan status tempat tidur, bertingkat.</summary>
    public class BedDriftPagedResult : PagedResult<BedDriftItemResponse>
    {
    }
}
