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
}
