using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceInsuranceSummaryResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int TotalInsurance { get; set; }
        public int ActiveInsurance { get; set; }
        public int InactiveInsurance { get; set; }
        public int ExpiredInsurance { get; set; }
        public int CurrentlyValidInsurance { get; set; }
        public int BpjsKesehatanInsurance { get; set; }
        public int BpjsKetenagakerjaanInsurance { get; set; }
        public int PrivateInsurance { get; set; }
    }

    public class WorkforceInsuranceResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        public bool IsBpjsKesehatanEnabled { get; set; }
        public string? BpjsKesehatanNumber { get; set; }
        public string? BpjsKesehatanNumberMasked { get; set; }

        public bool IsBpjsKetenagakerjaanEnabled { get; set; }
        public string? BpjsKetenagakerjaanNumber { get; set; }
        public string? BpjsKetenagakerjaanNumberMasked { get; set; }

        public bool IsPrivateInsuranceEnabled { get; set; }
        public Guid? PrivateInsuranceProviderId { get; set; }
        public string? PrivateInsuranceProviderCode { get; set; }
        public string? PrivateInsuranceProviderName { get; set; }
        public string? PrivateInsuranceProvider { get; set; }
        public string? PrivateInsuranceNumber { get; set; }
        public string? PrivateInsuranceNumberMasked { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsExpired { get; set; }
        public bool IsCurrentlyValid { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }
        public Guid? CreateBy { get; set; }
        public string? CreateByName { get; set; }
    }

    public class WorkforceInsuranceDetailResponse : WorkforceInsuranceResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid? UpdateBy { get; set; }
        public string? UpdateByName { get; set; }
    }

    public class WorkforceInsuranceOptionResponse
    {
        public Guid Id { get; set; }
        public string DisplayLabel { get; set; } = string.Empty;

        public bool IsBpjsKesehatanEnabled { get; set; }
        public string? BpjsKesehatanNumberMasked { get; set; }

        public bool IsBpjsKetenagakerjaanEnabled { get; set; }
        public string? BpjsKetenagakerjaanNumberMasked { get; set; }

        public bool IsPrivateInsuranceEnabled { get; set; }
        public Guid? PrivateInsuranceProviderId { get; set; }
        public string? PrivateInsuranceProviderCode { get; set; }
        public string? PrivateInsuranceProviderName { get; set; }
        public string? PrivateInsuranceProvider { get; set; }
        public string? PrivateInsuranceNumberMasked { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public bool IsCurrentlyValid { get; set; }
        public bool IsActive { get; set; }
    }

    public class WorkforceInsuranceOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkforceInsuranceOptionResponse> Items { get; set; } = new();
    }

    public class WorkforceInsuranceFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string DateFilterField { get; set; } = "EffectiveStartDate";
        public string ResetButtonLabel { get; set; } = "Reset";
        public WorkforceInsuranceDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkforceInsuranceCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkforceInsuranceSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<WorkforceInsuranceCoverageTypeOptionResponse> CoverageTypes { get; set; } = new();
        public WorkforceInsuranceFrontendGuideResponse FrontendGuide { get; set; } = new();
    }

    public class WorkforceInsuranceDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public string? CoverageType { get; set; }
        public Guid? PrivateInsuranceProviderId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsExpired { get; set; }
        public bool? IsCurrentlyValid { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "effectiveStartDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WorkforceInsuranceCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceInsuranceSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceInsuranceCoverageTypeOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class WorkforceInsuranceFrontendGuideResponse
    {
        public string GeneralInstruction { get; set; } =
            "Insurance workforce menyimpan profil asuransi karyawan/dokter/external user. Umumnya satu workforce profile cukup memiliki satu record aktif. Jika sudah ada, gunakan PUT untuk update.";

        public string BpjsKesehatanInstruction { get; set; } =
            "Jika IsBpjsKesehatanEnabled = true, maka BpjsKesehatanNumber wajib diisi. Jika false, backend akan menyimpan nomor sebagai null.";

        public string BpjsKetenagakerjaanInstruction { get; set; } =
            "Jika IsBpjsKetenagakerjaanEnabled = true, maka BpjsKetenagakerjaanNumber wajib diisi. Jika false, backend akan menyimpan nomor sebagai null.";

        public string PrivateInsuranceInstruction { get; set; } =
            "Jika IsPrivateInsuranceEnabled = true, maka PrivateInsuranceProviderId dan PrivateInsuranceNumber wajib diisi. Nama provider diambil dari MstInsuranceProvider.";

        public string DateInstruction { get; set; } =
            "EffectiveStartDate dan EffectiveEndDate bersifat nullable. Jika EffectiveEndDate sudah lewat, record dianggap expired.";

        public string SecurityInstruction { get; set; } =
            "Untuk list/dropdown, gunakan field nomor masked. Nomor penuh hanya dipakai pada halaman detail/edit yang berhak.";
    }

    public class CreateWorkforceInsuranceRequest
    {
        public bool IsBpjsKesehatanEnabled { get; set; } = false;

        [MaxLength(50)]
        [RegularExpression(@"^[0-9\s\-\.]*$", ErrorMessage = "BpjsKesehatanNumber hanya boleh berisi angka, spasi, titik, atau tanda strip.")]
        public string? BpjsKesehatanNumber { get; set; }

        public bool IsBpjsKetenagakerjaanEnabled { get; set; } = false;

        [MaxLength(50)]
        [RegularExpression(@"^[0-9\s\-\.]*$", ErrorMessage = "BpjsKetenagakerjaanNumber hanya boleh berisi angka, spasi, titik, atau tanda strip.")]
        public string? BpjsKetenagakerjaanNumber { get; set; }

        public bool IsPrivateInsuranceEnabled { get; set; } = false;

        public Guid? PrivateInsuranceProviderId { get; set; }

        [MaxLength(100)]
        public string? PrivateInsuranceNumber { get; set; }

        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceInsuranceRequest : CreateWorkforceInsuranceRequest
    {
    }

    public class UpdateWorkforceInsuranceStatusRequest
    {
        public bool IsActive { get; set; }
        public DateTime? EffectiveEndDate { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
