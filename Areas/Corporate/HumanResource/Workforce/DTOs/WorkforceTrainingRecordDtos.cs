using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public enum WorkforceTrainingRecordType
    {
        Unknown = 0,
        Seminar = 1,
        Workshop = 2,
        Course = 3,
        Webinar = 4,
        InHouseTraining = 5,
        Conference = 6,
        E_Learning = 7,
        Other = 99
    }

    public class WorkforceTrainingRecordSummaryResponse
    {
        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public int TotalTrainingRecord { get; set; }

        public int ActiveTrainingRecord { get; set; }

        public int InactiveTrainingRecord { get; set; }

        public int VerifiedTrainingRecord { get; set; }

        public int UnverifiedTrainingRecord { get; set; }

        public int TrainingWithFile { get; set; }

        public decimal TotalCreditPoint { get; set; }
    }

    public class WorkforceTrainingRecordResponse
    {
        public Guid Id { get; set; }

        public Guid WorkforceProfileId { get; set; }

        public string ProfileCode { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? RequirementCode { get; set; }

        public string TrainingType { get; set; } = string.Empty;

        public string TrainingName { get; set; } = string.Empty;

        public string? Organizer { get; set; }

        public string? Location { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? CertificateNumber { get; set; }

        public decimal CreditPoint { get; set; }

        public string? FilePath { get; set; }

        public string? FileContentType { get; set; }

        public bool HasFile { get; set; }

        public string? FilePreviewUrl { get; set; }

        public string? FileDownloadUrl { get; set; }

        public bool IsVerified { get; set; }

        public bool IsActive { get; set; }

        public string? Description { get; set; }

        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceTrainingRecordDetailResponse : WorkforceTrainingRecordResponse
    {
        public DateTime? UpdateDateTime { get; set; }

        public Guid CreateBy { get; set; }

        public Guid UpdateBy { get; set; }
    }

    public class WorkforceTrainingRecordOptionResponse
    {
        public Guid Id { get; set; }

        public string? RequirementCode { get; set; }

        public string TrainingType { get; set; } = string.Empty;

        public string TrainingName { get; set; } = string.Empty;

        public string? Organizer { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal CreditPoint { get; set; }

        public bool IsVerified { get; set; }

        public bool IsActive { get; set; }
    }

    public class WorkforceTrainingRecordOptionPagedResponse
    {
        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public int TotalData { get; set; }

        public int TotalPage { get; set; }

        public List<WorkforceTrainingRecordOptionResponse> Items { get; set; } = new();
    }

    public class WorkforceTrainingRecordFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public string DateFilterInfo { get; set; } =
            "Filter tanggal menggunakan StartDate. Jika customPeriod diisi selain custom, startDate dan endDate boleh dikosongkan.";

        public string ResetButtonLabel { get; set; } = "Reset";

        public WorkforceTrainingRecordDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<WorkforceTrainingRecordCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();

        public List<WorkforceTrainingRecordSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<WorkforceTrainingRecordTypeOptionResponse> TrainingTypeOptions { get; set; } = new();

        public WorkforceTrainingRecordCodeInfoResponse CodeInfo { get; set; } = new();

        public WorkforceTrainingRecordFileUploadInfoResponse FileUploadInfo { get; set; } = new();

        public WorkforceTrainingRecordFrontendGuideResponse FrontendGuide { get; set; } = new();
    }

    public class WorkforceTrainingRecordDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? CustomPeriod { get; set; }

        public WorkforceTrainingRecordType? TrainingType { get; set; }

        public bool? IsVerified { get; set; }

        public bool? IsActive { get; set; }

        public string? Search { get; set; }

        public string SortBy { get; set; } = "startDate";

        public string SortDirection { get; set; } = "desc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class WorkforceTrainingRecordCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceTrainingRecordSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceTrainingRecordTypeOptionResponse
    {
        public WorkforceTrainingRecordType Value { get; set; }

        public string ValueName { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<string> TrainingNameExamples { get; set; } = new();
    }

    public class WorkforceTrainingRecordCodeInfoResponse
    {
        public string FieldName { get; set; } = "RequirementCode";

        public string CodeFormat { get; set; } = "TRN-RSMMC-00001";

        public string Description { get; set; } =
            "RequirementCode pada WfpTrainingRecord digunakan sebagai kode record training otomatis. Frontend tidak perlu mengirim field ini pada POST/PUT.";
    }

    public class WorkforceTrainingRecordFileUploadInfoResponse
    {
        public long MaxFileSizeMb { get; set; } = 10;

        public List<string> AllowedExtensions { get; set; } = new()
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png",
            ".doc",
            ".docx",
            ".xls",
            ".xlsx"
        };

        public string PreviewInfo { get; set; } =
            "Endpoint preview dapat dipakai langsung untuk PDF dan image. Untuk DOC/DOCX/XLS/XLSX, frontend sebaiknya menyediakan fallback download atau viewer dokumen.";
    }

    public class WorkforceTrainingRecordFrontendGuideResponse
    {
        public string Purpose { get; set; } =
            "Training record menyimpan riwayat pelatihan, seminar, workshop, webinar, course, dan SKP/credit point milik workforce profile.";

        public string TrainingTypeInstruction { get; set; } =
            "TrainingType adalah kategori kegiatan training. Nama kegiatan sebenarnya diisi pada TrainingName.";

        public string Example { get; set; } =
            "Untuk pelatihan BTCLS, isi TrainingType = Course atau InHouseTraining, TrainingName = BTCLS, Organizer = lembaga penyelenggara, CreditPoint = nilai SKP jika ada.";

        public string CreditPointInstruction { get; set; } =
            "CreditPoint digunakan untuk SKP atau nilai kredit pelatihan. Jika tidak ada, isi 0.";

        public string VerificationInstruction { get; set; } =
            "Gunakan endpoint verify untuk memverifikasi training setelah dokumen atau informasi valid.";
    }

    public class CreateWorkforceTrainingRecordRequest
    {
        [Required]
        public WorkforceTrainingRecordType TrainingType { get; set; } = WorkforceTrainingRecordType.Unknown;

        [Required]
        [MaxLength(200)]
        public string TrainingName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Organizer { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        [Range(0, 999999)]
        public decimal CreditPoint { get; set; } = 0;

        public IFormFile? File { get; set; }

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceTrainingRecordRequest
    {
        [Required]
        public WorkforceTrainingRecordType TrainingType { get; set; } = WorkforceTrainingRecordType.Unknown;

        [Required]
        [MaxLength(200)]
        public string TrainingName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Organizer { get; set; }

        [MaxLength(200)]
        public string? Location { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [MaxLength(100)]
        public string? CertificateNumber { get; set; }

        [Range(0, 999999)]
        public decimal CreditPoint { get; set; } = 0;

        public IFormFile? File { get; set; }

        public bool ReplaceExistingFile { get; set; } = false;

        public bool IsVerified { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [MaxLength(250)]
        public string? Description { get; set; }
    }

    public class UpdateWorkforceTrainingRecordStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class VerifyWorkforceTrainingRecordRequest
    {
        public bool IsVerified { get; set; } = true;
    }
}
