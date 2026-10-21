using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class RoomSummaryResponse
    {
        public int TotalRoom { get; set; }
        public int ActiveRoom { get; set; }
        public int InactiveRoom { get; set; }
        public int AdmissionAvailableRoom { get; set; }
        public int IsolationRoom { get; set; }
        public int IntensiveCareRoom { get; set; }
        public int OdcRoom { get; set; }
        public int NewbornRoom { get; set; }
    }

    public class RoomResponse
    {
        public BedOperationMeta? OperationMeta { get; set; }
        public long OperationalVersion { get; set; }
        public Guid CycleId { get; set; }
        public List<BedVersionExpectation> AffectedBedVersions { get; set; } = [];
        public Guid Id { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitCode { get; set; } = string.Empty;
        public string ServiceUnitName { get; set; } = string.Empty;
        public Guid? PatientClassId { get; set; }
        public string? PatientClassCode { get; set; }
        public string? PatientClassName { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public RoomType RoomType { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public string? RoomNumber { get; set; }
        public string? LocationName { get; set; }
        public string? FloorName { get; set; }
        /// <summary>Kode lantai denah (BE-RWI-232); <c>null</c> untuk kamar di luar denah.</summary>
        public string? FloorCode { get; set; }
        public RoomWingDirection? WingDirection { get; set; }
        public string? WingDirectionName { get; set; }
        /// <summary>Kunci bentuk kamar pada gambar denah, mis. <c>lantai-3/k-329</c>.</summary>
        public string? FloorPlanRoomKey { get; set; }
        public int Capacity { get; set; }
        public bool IsForNewborn { get; set; }
        public bool IsIsolationRoom { get; set; }
        public bool IsIntensiveCare { get; set; }
        public bool IsOdcRoom { get; set; }
        public bool IsAvailableForAdmission { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class RoomDetailResponse : RoomResponse
    {
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class RoomOptionResponse
    {
        public Guid Id { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitCode { get; set; } = string.Empty;
        public string ServiceUnitName { get; set; } = string.Empty;
        public Guid? PatientClassId { get; set; }
        public string? PatientClassCode { get; set; }
        public string? PatientClassName { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public RoomType RoomType { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public string? RoomNumber { get; set; }
        public int Capacity { get; set; }
        public bool IsForNewborn { get; set; }
        public bool IsIsolationRoom { get; set; }
        public bool IsIntensiveCare { get; set; }
        public bool IsOdcRoom { get; set; }
        public bool IsAvailableForAdmission { get; set; }
        public int SortOrder { get; set; }
    }

    public class RoomOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<RoomOptionResponse> Items { get; set; } = new();
    }

    public class RoomEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class RoomFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";
        public string ResetButtonLabel { get; set; } = "Reset";

        public RoomDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<RoomCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<RoomSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<RoomEnumOptionResponse> RoomTypeOptions { get; set; } = new();
        public List<RoomEnumOptionResponse> WingDirectionOptions { get; set; } = new();
        public List<RoomQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<RoomFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<RoomFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class RoomDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? PatientClassId { get; set; }
        public bool? IsActive { get; set; }
        public RoomType? RoomType { get; set; }
        public bool? IsForNewborn { get; set; }
        public bool? IsIsolationRoom { get; set; }
        public bool? IsIntensiveCare { get; set; }
        public bool? IsOdcRoom { get; set; }
        public bool? IsAvailableForAdmission { get; set; }
        public string? FloorCode { get; set; }
        public bool? HasFloorPlanKey { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class RoomCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class RoomSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class RoomQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class RoomFormFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool IsRequiredOnCreate { get; set; }
        public bool IsRequiredOnUpdate { get; set; }
        public string RequiredType { get; set; } = "Optional";
        public int? MaxLength { get; set; }
        public string? OptionsSource { get; set; }
        public string? Description { get; set; }
        public string? Example { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateRoomRequest
    {
        public long ExpectedBedVersion { get; set; }
        [MaxLength(500)] public string? OperationReason { get; set; }
        public List<BedVersionExpectation> AffectedBedVersions { get; set; } = [];
        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? PatientClassId { get; set; }

        [Required]
        [MaxLength(150)]
        public string RoomName { get; set; } = string.Empty;

        public RoomType RoomType { get; set; } = RoomType.Unknown;

        [MaxLength(50)]
        public string? RoomNumber { get; set; }

        [MaxLength(100)]
        public string? LocationName { get; set; }

        [MaxLength(50)]
        public string? FloorName { get; set; }

        public int Capacity { get; set; } = 1;
        public bool IsForNewborn { get; set; } = false;
        public bool IsIsolationRoom { get; set; } = false;
        public bool IsIntensiveCare { get; set; } = false;
        public bool IsOdcRoom { get; set; } = false;
        public bool IsAvailableForAdmission { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        /// <summary>
        /// Kode lantai denah. Wajib bila <see cref="FloorPlanRoomKey"/> diisi; setelah terisi tidak
        /// dapat diubah (422 <c>FLOOR_PLAN_KEY_IMMUTABLE</c>).
        /// </summary>
        [MaxLength(10)]
        public string? FloorCode { get; set; }

        /// <summary>Arah wing 1–8; nilai lain ditolak 400 <c>WING_DIRECTION_INVALID</c>. Kosong berarti tidak diubah.</summary>
        public RoomWingDirection? WingDirection { get; set; }

        /// <summary>Kunci denah <c>lantai-&lt;FloorCode&gt;/&lt;slug&gt;</c>; unik dan tetap setelah terisi.</summary>
        [MaxLength(100)]
        public string? FloorPlanRoomKey { get; set; }
    }

    public class UpdateRoomRequest : CreateRoomRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateRoomStatusRequest
    {
        public long ExpectedBedVersion { get; set; }
        [MaxLength(500)] public string? OperationReason { get; set; }
        public List<BedVersionExpectation> AffectedBedVersions { get; set; } = [];
        public bool IsActive { get; set; }
    }

    public class DeleteRoomRequest
    {
        public long ExpectedBedVersion { get; set; }
        [MaxLength(500)] public string? OperationReason { get; set; }
        public List<BedVersionExpectation> AffectedBedVersions { get; set; } = [];
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class RoomCreateResponse
    {
        public BedOperationMeta? OperationMeta { get; set; }
        public long OperationalVersion { get; set; }
        public Guid CycleId { get; set; }
        public List<BedVersionExpectation> AffectedBedVersions { get; set; } = [];
        public Guid Id { get; set; }
        public Guid ServiceUnitId { get; set; }
        public Guid? PatientClassId { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public RoomType RoomType { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public string? RoomNumber { get; set; }
        public string? FloorCode { get; set; }
        public RoomWingDirection? WingDirection { get; set; }
        public string? FloorPlanRoomKey { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class RoomUpdateResponse
    {
        public BedOperationMeta? OperationMeta { get; set; }
        public long OperationalVersion { get; set; }
        public Guid CycleId { get; set; }
        public List<BedVersionExpectation> AffectedBedVersions { get; set; } = [];
        public Guid Id { get; set; }
        public Guid ServiceUnitId { get; set; }
        public Guid? PatientClassId { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public RoomType RoomType { get; set; }
        public string RoomTypeName { get; set; } = string.Empty;
        public string? FloorCode { get; set; }
        public RoomWingDirection? WingDirection { get; set; }
        public string? FloorPlanRoomKey { get; set; }
        public string? LocationName { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class RoomDeleteResponse
    {
        public BedOperationMeta? OperationMeta { get; set; }
        public long OperationalVersion { get; set; }
        public Guid CycleId { get; set; }
        public List<BedVersionExpectation> AffectedBedVersions { get; set; } = [];
        public Guid Id { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}
