using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs
{
    public class QueueVoiceProfileSummaryResponse
    {
        public int TotalProfile { get; set; }
        public int ActiveProfile { get; set; }
        public int InactiveProfile { get; set; }
        public int DefaultProfile { get; set; }
        public int FemaleProfile { get; set; }
        public int MaleProfile { get; set; }
        public int UnknownGenderProfile { get; set; }
    }

    public class QueueVoiceProfileResponse
    {
        public Guid Id { get; set; }
        public string VoiceCode { get; set; } = string.Empty;
        public string VoiceName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string GenderName { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string ModelPath { get; set; } = string.Empty;
        public decimal LengthScale { get; set; }
        public decimal NoiseScale { get; set; }
        public decimal NoiseW { get; set; }
        public decimal Volume { get; set; }
        public bool IsDefault { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class QueueVoiceProfileDetailResponse : QueueVoiceProfileResponse
    {
        public string? Description { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class QueueVoiceProfileOptionResponse
    {
        public Guid Id { get; set; }
        public string VoiceCode { get; set; } = string.Empty;
        public string VoiceName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string GenderName { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string ModelPath { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public int SortOrder { get; set; }
    }

    public class QueueVoiceProfileOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<QueueVoiceProfileOptionResponse> Items { get; set; } = new();
    }

    public class QueueVoiceProfileFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string CustomPeriodPriorityInfo { get; set; } =
            "Jika customPeriod diisi selain custom, maka startDate dan endDate akan diabaikan.";
        public string ResetButtonLabel { get; set; } = "Reset";

        public QueueVoiceProfileDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<QueueVoiceProfileCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<QueueVoiceProfileSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<QueueVoiceProfileEnumMetadataResponse> EnumOptions { get; set; } = new();
        public List<QueueVoiceProfileGenderOptionResponse> GenderOptions { get; set; } = new();
        public List<QueueVoiceProfileQueryParameterInfoResponse> QueryParameters { get; set; } = new();
        public List<QueueVoiceProfileFormFieldMetadataResponse> CreateFields { get; set; } = new();
        public List<QueueVoiceProfileFormFieldMetadataResponse> UpdateFields { get; set; } = new();
    }

    public class QueueVoiceProfileDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? Search { get; set; }
        public bool? IsActive { get; set; }
        public string? Gender { get; set; }
        public string? Language { get; set; }
        public bool? IsDefault { get; set; }
        public string SortBy { get; set; } = "sortOrder";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class QueueVoiceProfileEnumMetadataResponse
    {
        public string EnumName { get; set; } = string.Empty;
        public string FieldName { get; set; } = string.Empty;
        public string OptionsSource { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<QueueVoiceProfileEnumOptionResponse> Options { get; set; } = new();
    }

    public class QueueVoiceProfileEnumOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class QueueVoiceProfileGenderOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class QueueVoiceProfileCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool UsesStartDate { get; set; }
        public bool UsesEndDate { get; set; }
    }

    public class QueueVoiceProfileSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class QueueVoiceProfileQueryParameterInfoResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Required { get; set; } = "No";
        public string Description { get; set; } = string.Empty;
        public string? Example { get; set; }
    }

    public class QueueVoiceProfileFormFieldMetadataResponse
    {
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string InputType { get; set; } = string.Empty;
        public bool IsRequiredOnCreate { get; set; }
        public bool IsRequiredOnUpdate { get; set; }
        public string RequiredType { get; set; } = "Optional";
        public int? MaxLength { get; set; }
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? OptionsSource { get; set; }
        public string? Description { get; set; }
        public string? Example { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateQueueVoiceProfileRequest
    {
        [MaxLength(80)]
        public string? VoiceCode { get; set; }

        [Required]
        [MaxLength(150)]
        public string VoiceName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Gender { get; set; } = "Female";

        [MaxLength(20)]
        public string Language { get; set; } = "id-ID";

        [Required]
        [MaxLength(500)]
        public string ModelPath { get; set; } = string.Empty;

        public decimal LengthScale { get; set; } = 1.08m;
        public decimal NoiseScale { get; set; } = 0.65m;
        public decimal NoiseW { get; set; } = 0.80m;
        public decimal Volume { get; set; } = 1.15m;
        public bool IsDefault { get; set; } = false;
        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateQueueVoiceProfileRequest : CreateQueueVoiceProfileRequest
    {
        public bool IsActive { get; set; } = true;
    }

    public class UpdateQueueVoiceProfileStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(250)]
        public string? Reason { get; set; }
    }

    public class DeleteQueueVoiceProfileRequest
    {
        [MaxLength(250)]
        public string? DeleteReason { get; set; }
    }

    public class QueueVoiceProfileCreateResponse
    {
        public Guid Id { get; set; }
        public string VoiceCode { get; set; } = string.Empty;
        public string VoiceName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string GenderName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class QueueVoiceProfileUpdateResponse
    {
        public Guid Id { get; set; }
        public string VoiceCode { get; set; } = string.Empty;
        public string VoiceName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string GenderName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class QueueVoiceProfileDeleteResponse
    {
        public Guid Id { get; set; }
        public string VoiceCode { get; set; } = string.Empty;
        public string VoiceName { get; set; } = string.Empty;
        public DateTime? DeleteDateTime { get; set; }
        public Guid? DeleteBy { get; set; }
        public string? DeleteByName { get; set; }
    }
}
