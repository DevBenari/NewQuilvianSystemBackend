using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>Penyaring denah Monitoring Bed. Tanpa <c>FloorCode</c>, server memilih lantai dengan bed Tersedia terbanyak.</summary>
    public class FloorPlanMonitoringQuery
    {
        [MaxLength(10)]
        public string? FloorCode { get; set; }
    }

    /// <summary>
    /// Penyaring denah Pilih Bed admisi atau tujuan transfer (BE-RWI-239). <c>EpisodeId</c> berasal dari
    /// konteks layar, tidak diketik petugas.
    /// </summary>
    /// <remarks>
    /// <c>Admission</c> hanya untuk episode <c>Draft</c>; <c>Transfer</c> hanya untuk episode <c>Admitted</c> yang
    /// masih punya penempatan aktif. Selain itu ditolak 422 <c>FLOOR_PLAN_PURPOSE_MISMATCH</c>.
    /// </remarks>
    public class FloorPlanSelectionQuery
    {
        [MaxLength(10)]
        public string? FloorCode { get; set; }

        /// <summary><c>Admission</c> atau <c>Transfer</c>.</summary>
        [MaxLength(20)]
        public string? Purpose { get; set; }

        public Guid? EpisodeId { get; set; }
    }

    /// <summary>Denah satu lantai: kamar berkunci, status kamar, badge, ringkasan HD/LD, dan bed.</summary>
    public class FloorPlanResponse
    {
        public DateTime AsOfUtc { get; set; }
        public string? FloorCode { get; set; }
        public string? FloorName { get; set; }
        public List<FloorPlanFloorSummaryResponse> Floors { get; set; } = [];
        public List<FloorPlanRoomResponse> Rooms { get; set; } = [];
    }

    public class FloorPlanFloorSummaryResponse
    {
        public string FloorCode { get; set; } = string.Empty;
        public string FloorName { get; set; } = string.Empty;
        public int RoomCount { get; set; }
        public int AvailableBedCount { get; set; }
    }

    public class FloorPlanCodeLabelResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class FloorPlanWingResponse
    {
        public int Code { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Badge kamar <c>kapasitas/terisi</c>: kapasitas = bed aktif yang bukan Tidak Tersedia; terisi =
    /// Terisi + Dipesan. Tidak pernah berasal dari <c>MstRoom.Capacity</c>.
    /// </summary>
    public class FloorPlanCountsResponse
    {
        public int Capacity { get; set; }
        public int Filled { get; set; }
        public int Available { get; set; }
    }

    /// <summary>Ringkasan HD/LD penghuni bed Terisi. NULL bagi profil OperationalOnly.</summary>
    public class FloorPlanDependencySummaryResponse
    {
        public int HighCount { get; set; }
        public int LowCount { get; set; }
        public int UnassessedCount { get; set; }
        public bool IsHighDependency { get; set; }
    }

    public class FloorPlanRoomResponse
    {
        public Guid RoomId { get; set; }
        public string FloorPlanRoomKey { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string? RoomNumber { get; set; }
        public FloorPlanWingResponse? WingDirection { get; set; }
        public string? LocationName { get; set; }
        public string? ServiceUnitName { get; set; }
        public string? PatientClassName { get; set; }
        /// <summary>ICU, Isolasi, ODC, atau Perinatologi (RWI-DEC-312).</summary>
        public bool IsSpecialClass { get; set; }
        /// <summary><c>Unavailable</c>, <c>Available</c>, <c>Preparing</c>, atau <c>Full</c>.</summary>
        public FloorPlanCodeLabelResponse RoomStatus { get; set; } = new();
        /// <summary>Hitungan bed biasa.</summary>
        public FloorPlanCountsResponse Counts { get; set; } = new();
        /// <summary>Hitungan boks rawat gabung di kamar biasa (RWI-OQ-144 masih terbuka).</summary>
        public FloorPlanCountsResponse CribCounts { get; set; } = new();
        public bool HasAvailableCrib { get; set; }
        /// <summary>
        /// Outline hijau. Monitoring: ada bed biasa Tersedia. Pilih Bed: ada bed Tersedia yang layak
        /// untuk episode itu.
        /// </summary>
        public bool IsSelectable { get; set; }
        public FloorPlanDependencySummaryResponse? DependencySummary { get; set; }
        public List<FloorPlanBedResponse> Beds { get; set; } = [];
    }

    /// <summary>
    /// Satu bed pada denah: seluruh field <see cref="BedOperationalResponse"/> ditambah huruf dan penanda
    /// bed; khusus Pilih Bed ditambah kelayakan untuk episode serta kategori transfer.
    /// </summary>
    public class FloorPlanBedResponse : BedOperationalResponse
    {
        /// <summary>Huruf bed, mis. "C"; label yang dibaca petugas ada di <c>BedName</c> ("329-C").</summary>
        public string? BedNumber { get; set; }
        public bool IsIntensiveCareBed { get; set; }
        public FloorPlanBedEligibilityResponse? Eligibility { get; set; }
        /// <summary><c>DownGrade</c>/<c>UpGrade</c>/<c>SameGrade</c>/<c>Special</c>/<c>Unknown</c>; hanya untuk transfer.</summary>
        public string? ComparisonResult { get; set; }
        public string[] AllowedManualCategories { get; set; } = [];
    }

    public class FloorPlanBedEligibilityResponse
    {
        public bool IsEligible { get; set; }
        public List<FloorPlanEligibilityFailureResponse> Failures { get; set; } = [];
    }

    public class FloorPlanEligibilityFailureResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
