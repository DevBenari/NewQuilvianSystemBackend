using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs
{
    // =====================================================================
    // Mesin HD
    // =====================================================================

    public class HmdMachinePagedQuery
    {
        public string? Search { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public HmdMachineStatus? MachineStatus { get; set; }
        public HmdIsolationRequirement? DedicatedFor { get; set; }
        public bool? IsSchedulable { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Penyaring feed pilihan mesin. Bawaannya hanya mesin aktif, boleh dijadwalkan, dan
    /// berstatus <c>Ready</c> — sehingga mesin diblokir, dalam perawatan, atau tidak laik tidak
    /// pernah muncul sebagai pilihan penjadwalan (<c>BE-HMD-04</c> kriteria 2).
    /// </summary>
    public class HmdMachineOptionsQuery
    {
        public string? Search { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public HmdIsolationRequirement? DedicatedFor { get; set; }
        public bool OnlySchedulable { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class CreateHmdMachineRequest
    {
        [Required, MaxLength(30)]
        public string MachineCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string MachineName { get; set; } = string.Empty;

        [Required]
        public Guid ServiceUnitId { get; set; }

        [MaxLength(150)]
        public string? Manufacturer { get; set; }

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        public HmdIsolationRequirement DedicatedFor { get; set; } = HmdIsolationRequirement.None;

        public bool IsSchedulable { get; set; } = true;
    }

    public class UpdateHmdMachineRequest
    {
        [Required, MaxLength(30)]
        public string MachineCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string MachineName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Manufacturer { get; set; }

        [MaxLength(100)]
        public string? SerialNumber { get; set; }

        public HmdIsolationRequirement DedicatedFor { get; set; } = HmdIsolationRequirement.None;

        public bool IsSchedulable { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }

    public class ChangeHmdMachineStatusRequest
    {
        [Required]
        public HmdMachineStatus ToStatus { get; set; }

        /// <summary>Alasan perubahan; wajib (<c>HMD-VAL-091</c>).</summary>
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class HmdMachineResponse
    {
        public Guid Id { get; set; }
        public string MachineCode { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public Guid ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public string? Manufacturer { get; set; }
        public string? SerialNumber { get; set; }
        public HmdMachineStatus MachineStatus { get; set; }
        public string MachineStatusName { get; set; } = string.Empty;
        public HmdIsolationRequirement DedicatedFor { get; set; }
        public string DedicatedForName { get; set; } = string.Empty;
        public bool IsSchedulable { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastStatusChangedAt { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid CreateBy { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid UpdateBy { get; set; }
    }

    public class HmdMachineOptionResponse
    {
        public Guid Id { get; set; }
        public string MachineCode { get; set; } = string.Empty;
        public string MachineName { get; set; } = string.Empty;
        public Guid ServiceUnitId { get; set; }
        public HmdIsolationRequirement DedicatedFor { get; set; }
        public string DedicatedForName { get; set; } = string.Empty;
    }

    public class HmdMachineStatusHistoryResponse
    {
        public Guid Id { get; set; }
        public Guid MachineId { get; set; }
        public HmdMachineStatus? FromStatus { get; set; }
        public string? FromStatusName { get; set; }
        public HmdMachineStatus ToStatus { get; set; }
        public string ToStatusName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public Guid ChangedByUserId { get; set; }
        public string? ChangedByName { get; set; }
        public DateTime ChangedAt { get; set; }
    }

    public class HmdMachineSummaryResponse
    {
        public int TotalMachine { get; set; }
        public int ActiveMachine { get; set; }
        public int InactiveMachine { get; set; }
        public int ReadyMachine { get; set; }
        public int BlockedMachine { get; set; }
        public int MaintenanceMachine { get; set; }
        public int NotEligibleMachine { get; set; }
        public int DedicatedIsolationMachine { get; set; }
    }

    public class HmdMachineDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public HmdMachineStatus? MachineStatus { get; set; }
        public HmdIsolationRequirement? DedicatedFor { get; set; }
        public bool? IsSchedulable { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "machineCode";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class HmdMachineFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public HmdMachineDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<HmdOptionItemResponse> MachineStatusOptions { get; set; } = new();
        public List<HmdOptionItemResponse> DedicatedForOptions { get; set; } = new();
        public List<HmdSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    // =====================================================================
    // Station HD
    // =====================================================================

    public class HmdStationPagedQuery
    {
        public string? Search { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public HmdStationStatus? StationStatus { get; set; }
        public bool? IsIsolationStation { get; set; }
        public bool? IsActive { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>Penyaring feed pilihan station. Bawaannya hanya station aktif dan tersedia.</summary>
    public class HmdStationOptionsQuery
    {
        public string? Search { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public bool? IsIsolationStation { get; set; }
        public bool OnlyAvailable { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }

    public class CreateHmdStationRequest
    {
        [Required, MaxLength(30)]
        public string StationCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string StationName { get; set; } = string.Empty;

        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? RoomId { get; set; }

        public bool IsIsolationStation { get; set; }
    }

    public class UpdateHmdStationRequest
    {
        [Required, MaxLength(30)]
        public string StationCode { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string StationName { get; set; } = string.Empty;

        public Guid? RoomId { get; set; }

        public bool IsIsolationStation { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class ChangeHmdStationStatusRequest
    {
        [Required]
        public HmdStationStatus ToStatus { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class HmdStationResponse
    {
        public Guid Id { get; set; }
        public string StationCode { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public Guid ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public Guid? RoomId { get; set; }
        public string? RoomName { get; set; }
        public HmdStationStatus StationStatus { get; set; }
        public string StationStatusName { get; set; } = string.Empty;
        public string? StatusReason { get; set; }
        public DateTime? LastStatusChangedAt { get; set; }
        public bool IsIsolationStation { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid CreateBy { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid UpdateBy { get; set; }
    }

    public class HmdStationOptionResponse
    {
        public Guid Id { get; set; }
        public string StationCode { get; set; } = string.Empty;
        public string StationName { get; set; } = string.Empty;
        public Guid ServiceUnitId { get; set; }
        public bool IsIsolationStation { get; set; }
    }

    public class HmdStationSummaryResponse
    {
        public int TotalStation { get; set; }
        public int ActiveStation { get; set; }
        public int InactiveStation { get; set; }
        public int AvailableStation { get; set; }
        public int BlockedStation { get; set; }
        public int MaintenanceStation { get; set; }
        public int IsolationStation { get; set; }
    }

    public class HmdStationDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public HmdStationStatus? StationStatus { get; set; }
        public bool? IsIsolationStation { get; set; }
        public bool? IsActive { get; set; }
        public string SortBy { get; set; } = "stationCode";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class HmdStationFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public HmdStationDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<HmdOptionItemResponse> StationStatusOptions { get; set; } = new();
        public List<HmdSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    // =====================================================================
    // Pengaturan unit HD
    // =====================================================================

    public class UpdateHmdSettingRequest
    {
        [Range(1, 50)]
        public int MaxPatientsPerNurse { get; set; }

        public bool EnforceNurseRatio { get; set; }

        /// <summary>Masa berlaku hasil pemeriksaan air, jam. Contoh 60 hari = 1440.</summary>
        [Range(1, 87600)]
        public int WaterResultValidityHours { get; set; }

        public bool EnforceCompetencyCheck { get; set; }

        public bool AllowMultipleActiveEpisodePerPatient { get; set; }

        [Range(0, 1440)]
        public int SessionStartGraceMinutes { get; set; }

        public bool RequireDifferentSigner { get; set; } = true;

        /// <summary>Tindakan <c>MstProcedure</c> yang dipakai sebagai tindakan hemodialisa.</summary>
        public Guid? ProcedureId { get; set; }
    }

    public class HmdSettingResponse
    {
        public Guid Id { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public int MaxPatientsPerNurse { get; set; }
        public bool EnforceNurseRatio { get; set; }
        public int WaterResultValidityHours { get; set; }
        public bool EnforceCompetencyCheck { get; set; }
        public bool AllowMultipleActiveEpisodePerPatient { get; set; }
        public int SessionStartGraceMinutes { get; set; }
        public bool RequireDifferentSigner { get; set; }
        public Guid? ProcedureId { get; set; }
        public string? ProcedureName { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid UpdateBy { get; set; }
    }

    // =====================================================================
    // Butir checklist Pra-HD
    // =====================================================================

    public class HmdChecklistItemQuery
    {
        public string? Search { get; set; }
        public HmdChecklistCategory? Category { get; set; }
        public bool? IsMandatory { get; set; }
        public bool? IsOverridable { get; set; }
        public bool? IsActive { get; set; }
    }

    public class CreateHmdChecklistItemRequest
    {
        [Required, MaxLength(50)]
        public string ItemCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        public HmdChecklistCategory Category { get; set; }

        public bool IsMandatory { get; set; } = true;

        [Range(0, 1000)]
        public int CheckSequence { get; set; }
    }

    public class UpdateHmdChecklistItemRequest
    {
        [Required, MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [Required]
        public HmdChecklistCategory Category { get; set; }

        public bool IsMandatory { get; set; } = true;

        [Range(0, 1000)]
        public int CheckSequence { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Menetapkan apakah butir boleh dilewati dokter. Catatan tata kelola klinis wajib diisi
    /// dan tersimpan pada butirnya, misalnya "Keputusan Komite Medis No. 42".
    /// </summary>
    public class SetOverridableRequest
    {
        public bool IsOverridable { get; set; }

        [MaxLength(1000)]
        public string? ClinicalGovernanceNote { get; set; }
    }

    public class HmdChecklistItemResponse
    {
        public Guid Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public HmdChecklistCategory Category { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public bool IsOverridable { get; set; }
        public string? OverridableDecisionNote { get; set; }
        public Guid? OverridableDecidedByUserId { get; set; }
        public DateTime? OverridableDecidedAt { get; set; }
        public int CheckSequence { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid UpdateBy { get; set; }
    }

    public class HmdChecklistItemOptionResponse
    {
        public Guid Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public HmdChecklistCategory Category { get; set; }
    }

    public class HmdChecklistItemSummaryResponse
    {
        public int TotalItem { get; set; }
        public int ActiveItem { get; set; }
        public int InactiveItem { get; set; }
        public int MandatoryItem { get; set; }
        public int OverridableItem { get; set; }
    }

    public class HmdChecklistItemFilterMetadataResponse
    {
        public string ResetButtonLabel { get; set; } = "Reset";
        public List<HmdOptionItemResponse> CategoryOptions { get; set; } = new();
    }
}
