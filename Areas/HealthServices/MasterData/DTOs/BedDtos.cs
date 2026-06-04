using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    public class BedSummaryResponse
    {
        public int TotalBed { get; set; }
        public int ActiveBed { get; set; }
        public int InactiveBed { get; set; }
        public int AvailableBed { get; set; }
        public int ReservableBed { get; set; }
        public int IsolationBed { get; set; }
        public int IntensiveCareBed { get; set; }
        public int OdcBed { get; set; }
        public int NewbornBed { get; set; }
    }

    public class BedResponse
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public string RoomCode { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitCode { get; set; } = string.Empty;
        public string ServiceUnitName { get; set; } = string.Empty;
        public Guid? PatientClassId { get; set; }
        public string? PatientClassCode { get; set; }
        public string? PatientClassName { get; set; }
        public string BedCode { get; set; } = string.Empty;
        public string BedName { get; set; } = string.Empty;
        public string? BedNumber { get; set; }
        public BedStatus BedStatus { get; set; }
        public bool IsForMale { get; set; }
        public bool IsForFemale { get; set; }
        public bool IsForNewborn { get; set; }
        public bool IsIsolationBed { get; set; }
        public bool IsIntensiveCareBed { get; set; }
        public bool IsOdcBed { get; set; }
        public bool IsReservable { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class BedDetailResponse : BedResponse
    {
        public string? Description { get; set; }
    }

    public class BedOptionResponse
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitName { get; set; } = string.Empty;
        public Guid? PatientClassId { get; set; }
        public string? PatientClassName { get; set; }
        public string BedCode { get; set; } = string.Empty;
        public string BedName { get; set; } = string.Empty;
        public string? BedNumber { get; set; }
        public BedStatus BedStatus { get; set; }
        public bool IsForMale { get; set; }
        public bool IsForFemale { get; set; }
        public bool IsForNewborn { get; set; }
        public bool IsReservable { get; set; }
    }

    public class BedOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<BedOptionResponse> Items { get; set; } = new();
    }

    public class BedEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class BedFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";

        public BedDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<BedCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<BedSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<BedEnumOptionResponse> BedStatusOptions { get; set; } = new();
        public List<BedQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<BedFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<BedFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class BedDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? RoomId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public bool? IsActive { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class BedCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class BedSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class BedQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class BedFormFieldMetadataResponse
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

    public class CreateBedRequest
    {
        [Required]
        public Guid RoomId { get; set; }

        [Required]
        [MaxLength(100)]
        public string BedName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? BedNumber { get; set; }

        public BedStatus BedStatus { get; set; } = BedStatus.Available;
        public bool IsForMale { get; set; } = true;
        public bool IsForFemale { get; set; } = true;
        public bool IsForNewborn { get; set; } = false;
        public bool IsIsolationBed { get; set; } = false;
        public bool IsIntensiveCareBed { get; set; } = false;
        public bool IsOdcBed { get; set; } = false;
        public bool IsReservable { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateBedRequest
    {
        [Required]
        public Guid RoomId { get; set; }

        [Required]
        [MaxLength(100)]
        public string BedName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? BedNumber { get; set; }

        public BedStatus BedStatus { get; set; } = BedStatus.Available;
        public bool IsForMale { get; set; } = true;
        public bool IsForFemale { get; set; } = true;
        public bool IsForNewborn { get; set; } = false;
        public bool IsIsolationBed { get; set; } = false;
        public bool IsIntensiveCareBed { get; set; } = false;
        public bool IsOdcBed { get; set; } = false;
        public bool IsReservable { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class BedCreateResponse
    {
        public Guid Id { get; set; }
        public Guid RoomId { get; set; }
        public string BedCode { get; set; } = string.Empty;
        public string BedName { get; set; } = string.Empty;
        public string? BedNumber { get; set; }
        public BedStatus BedStatus { get; set; }
        public bool IsReservable { get; set; }
        public bool IsActive { get; set; }
    }

    public class BedUpdateResponse : BedCreateResponse
    {
    }
}
