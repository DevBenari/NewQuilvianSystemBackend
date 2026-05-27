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
        public string? RoomNumber { get; set; }
        public string? LocationName { get; set; }
        public string? FloorName { get; set; }
        public int Capacity { get; set; }
        public bool IsForMale { get; set; }
        public bool IsForFemale { get; set; }
        public bool IsForNewborn { get; set; }
        public bool IsIsolationRoom { get; set; }
        public bool IsIntensiveCare { get; set; }
        public bool IsOdcRoom { get; set; }
        public bool IsAvailableForAdmission { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class RoomDetailResponse : RoomResponse
    {
        public string? Description { get; set; }
    }

    public class RoomOptionResponse
    {
        public Guid Id { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitName { get; set; } = string.Empty;
        public Guid? PatientClassId { get; set; }
        public string? PatientClassName { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public RoomType RoomType { get; set; }
        public int Capacity { get; set; }
        public bool IsAvailableForAdmission { get; set; }
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
        public RoomDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<RoomSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<RoomEnumOptionResponse> RoomTypeOptions { get; set; } = new();
    }

    public class RoomDefaultFilterResponse
    {
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? PatientClassId { get; set; }
        public RoomType? RoomType { get; set; }
        public bool? IsIsolationRoom { get; set; }
        public bool? IsIntensiveCare { get; set; }
        public bool? IsOdcRoom { get; set; }
        public bool? IsForNewborn { get; set; }
        public bool? IsAvailableForAdmission { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class RoomSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreateRoomRequest
    {
        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? PatientClassId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoomCode { get; set; } = string.Empty;

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
        public bool IsForMale { get; set; } = true;
        public bool IsForFemale { get; set; } = true;
        public bool IsForNewborn { get; set; } = false;
        public bool IsIsolationRoom { get; set; } = false;
        public bool IsIntensiveCare { get; set; } = false;
        public bool IsOdcRoom { get; set; } = false;
        public bool IsAvailableForAdmission { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateRoomRequest : CreateRoomRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class RoomCreateResponse
    {
        public Guid Id { get; set; }
        public Guid ServiceUnitId { get; set; }
        public Guid? PatientClassId { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public RoomType RoomType { get; set; }
        public bool IsActive { get; set; }
    }
}