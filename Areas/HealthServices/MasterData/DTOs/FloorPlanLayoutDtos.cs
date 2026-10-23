using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    // =====================================================================
    // BE-RWI-236 — Susunan lantai editor Master Bed dan aktivasi ruang cadangan (API 14.3)
    // =====================================================================

    /// <summary>Penyaring susunan satu lantai. <c>FloorCode</c> wajib; kosong ditolak 400 <c>FLOOR_CODE_REQUIRED</c>.</summary>
    public class FloorPlanLayoutQuery
    {
        [MaxLength(10)]
        public string? FloorCode { get; set; }
    }

    /// <summary>Susunan satu lantai untuk editor Master Bed, tanpa nama maupun No. RM penghuni.</summary>
    public class FloorPlanLayoutResponse
    {
        public string FloorCode { get; set; } = string.Empty;
        public string FloorName { get; set; } = string.Empty;
        public List<FloorPlanLayoutRoomResponse> Rooms { get; set; } = [];
        /// <summary>Kamar rawat inap yang belum punya posisi di denah (daftar "Kamar tanpa posisi").</summary>
        public List<FloorPlanRoomWithoutKeyResponse> RoomsWithoutFloorPlanKey { get; set; } = [];
    }

    public class FloorPlanLayoutRoomResponse
    {
        public Guid RoomId { get; set; }
        public string FloorPlanRoomKey { get; set; } = string.Empty;
        public string RoomCode { get; set; } = string.Empty;
        public string? RoomNumber { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public Guid? PatientClassId { get; set; }
        public string? PatientClassName { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public RoomWingDirection? WingDirection { get; set; }
        public string? WingDirectionName { get; set; }
        public bool IsActive { get; set; }
        public bool IsAvailableForAdmission { get; set; }
        /// <summary>Jumlah penempatan aktif; dipakai layar sebelum menawarkan ubah kelas.</summary>
        public int OccupiedCount { get; set; }
        /// <summary>Jumlah pesanan aktif yang belum kedaluwarsa.</summary>
        public int ReservedCount { get; set; }
        /// <summary>Versi seluruh bed kamar; dikirim kembali saat mengubah kamar.</summary>
        public List<BedVersionExpectation> AffectedBedVersions { get; set; } = [];
        public List<FloorPlanLayoutBedResponse> Beds { get; set; } = [];
    }

    public class FloorPlanLayoutBedResponse
    {
        public Guid BedId { get; set; }
        public string BedCode { get; set; } = string.Empty;
        /// <summary>Label yang dibaca petugas, mis. "329-C".</summary>
        public string BedName { get; set; } = string.Empty;
        public string? BedNumber { get; set; }
        public bool IsForNewborn { get; set; }
        public bool IsActive { get; set; }
        public string StatusCode { get; set; } = "Unavailable";
        public string? StatusLabel { get; set; }
        /// <summary>Benar bila bed pernah dipesan atau ditempati; bed seperti ini dinonaktifkan, tidak dihapus.</summary>
        public bool HasUsageHistory { get; set; }
        public long OperationalVersion { get; set; }
    }

    public class FloorPlanRoomWithoutKeyResponse
    {
        public Guid RoomId { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public string? FloorName { get; set; }
        public string? ServiceUnitName { get; set; }
    }

    /// <summary>
    /// Mengaktifkan ruang cadangan menjadi kamar beserta bed-nya dalam satu transaksi
    /// (header <c>Idempotency-Key</c> wajib). Semua wajib kecuali <c>CribCount</c>.
    /// </summary>
    /// <example>Kunci <c>lantai-3/cadangan-1</c>, nomor 324, Kelas 2, 3 bed → "Kelas 2 324" dengan 324-A s.d. 324-C.</example>
    public class ActivateFloorPlanRoomRequest
    {
        [MaxLength(100)]
        public string? FloorPlanRoomKey { get; set; }

        [MaxLength(10)]
        public string? FloorCode { get; set; }

        [MaxLength(50)]
        public string? RoomNumber { get; set; }

        /// <summary>Dipilih dari daftar kelas berlabel; Guid tidak diketik petugas.</summary>
        public Guid PatientClassId { get; set; }

        /// <summary>Dipilih dari daftar unit berlabel; Guid tidak diketik petugas.</summary>
        public Guid ServiceUnitId { get; set; }

        public RoomWingDirection? WingDirection { get; set; }

        /// <summary>Jumlah bed biasa, 1–20.</summary>
        public int BedCount { get; set; }

        /// <summary>Jumlah boks bayi rawat gabung, 0–10.</summary>
        public int CribCount { get; set; }
    }

    public class FloorPlanRoomActivationResponse
    {
        public RoomCreateResponse Room { get; set; } = new();
        public List<FloorPlanActivatedBedResponse> Beds { get; set; } = [];
        public BedOperationMeta? OperationMeta { get; set; }
    }

    public class FloorPlanActivatedBedResponse
    {
        public Guid BedId { get; set; }
        public string BedCode { get; set; } = string.Empty;
        public string BedName { get; set; } = string.Empty;
        public bool IsForNewborn { get; set; }
    }

    // =====================================================================
    // BE-RWI-237 — Impor susunan denah (API 14.3)
    // =====================================================================

    /// <summary>
    /// Manifest impor yang disusun layar Impor dari data denah frontend. <c>DryRun</c> = true hanya
    /// memeriksa dan tidak menulis apa pun. Kelas dan unit dipilih dari daftar berlabel.
    /// </summary>
    public class FloorPlanImportRequest
    {
        public bool? DryRun { get; set; }

        [MaxLength(50)]
        public string? ManifestVersion { get; set; }

        /// <summary>1–300 kamar.</summary>
        public List<FloorPlanImportRoomRequest> Rooms { get; set; } = [];
    }

    public class FloorPlanImportRoomRequest
    {
        [MaxLength(100)]
        public string? FloorPlanRoomKey { get; set; }

        [MaxLength(10)]
        public string? FloorCode { get; set; }

        [MaxLength(50)]
        public string? RoomNumber { get; set; }

        /// <summary>Nama ruang fisik; wajib untuk kamar tanpa nomor angka, mis. "R. ICU CK".</summary>
        [MaxLength(150)]
        public string? RoomName { get; set; }

        public Guid PatientClassId { get; set; }

        /// <summary>Unit layanan hasil pemetaan admin; kosong → baris ditolak <c>ROOM_UNIT_MAPPING_MISSING</c>.</summary>
        public Guid? ServiceUnitId { get; set; }

        public RoomWingDirection? WingDirection { get; set; }

        /// <summary>Kamar tanpa kunci yang dipilih admin untuk dilekatkan ke kunci ini; bed-nya tidak dibuat.</summary>
        public Guid? ExistingRoomId { get; set; }

        /// <summary>0–20 bed; huruf bed unik per kamar.</summary>
        public List<FloorPlanImportBedRequest> Beds { get; set; } = [];
    }

    public class FloorPlanImportBedRequest
    {
        /// <summary>Huruf bed, mis. "C".</summary>
        [MaxLength(50)]
        public string? BedNumber { get; set; }

        public bool IsForNewborn { get; set; }
    }

    public class FloorPlanImportResult
    {
        public bool DryRun { get; set; }
        /// <summary>Identitas catatan run; kosong pada dry run.</summary>
        public Guid? RunId { get; set; }
        public string ManifestHash { get; set; } = string.Empty;
        public FloorPlanImportSummaryResponse Summary { get; set; } = new();
        public List<FloorPlanImportRowResponse> Rows { get; set; } = [];
        public BedOperationMeta? OperationMeta { get; set; }
    }

    public class FloorPlanImportSummaryResponse
    {
        public FloorPlanImportCountResponse Rooms { get; set; } = new();
        public FloorPlanImportCountResponse Beds { get; set; } = new();
        public int BedsMarkedReady { get; set; }
    }

    public class FloorPlanImportCountResponse
    {
        public int Created { get; set; }
        public int Updated { get; set; }
        public int Unchanged { get; set; }
        public int Rejected { get; set; }
    }

    public class FloorPlanImportRowResponse
    {
        public string FloorPlanRoomKey { get; set; } = string.Empty;
        /// <summary><c>Created</c>, <c>Updated</c>, <c>Unchanged</c>, atau <c>Rejected</c>.</summary>
        public string Outcome { get; set; } = string.Empty;
        public List<string> Codes { get; set; } = [];
        public List<string> Messages { get; set; } = [];
    }
}
