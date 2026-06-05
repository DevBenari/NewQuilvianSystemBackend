using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs
{
    public class WorkforceCompetencyAssessmentSummaryResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public int TotalAssessment { get; set; }
        public int ActiveAssessment { get; set; }
        public int InactiveAssessment { get; set; }
        public int VerifiedAssessment { get; set; }
        public int UnverifiedAssessment { get; set; }
        public int PassedAssessment { get; set; }
        public int FailedAssessment { get; set; }
        public int NeedTrainingAssessment { get; set; }
        public int NotAssessedAssessment { get; set; }
        public int ExpiredAssessment { get; set; }
        public int AssessmentWithFile { get; set; }
        public int AssessmentWithoutFile { get; set; }
    }

    public class WorkforceCompetencyAssessmentResponse
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public Guid CompetencyId { get; set; }
        public string CompetencyCode { get; set; } = string.Empty;
        public string CompetencyName { get; set; } = string.Empty;
        public CompetencyCategory CompetencyCategory { get; set; }
        public DateTime AssessmentDate { get; set; }
        public CompetencyLevel CompetencyLevel { get; set; }
        public CompetencyAssessmentResultStatus ResultStatus { get; set; }
        public Guid? AssessedByUserId { get; set; }
        public string? AssessedByUserName { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsExpired { get; set; }
        public string? FilePath { get; set; }
        public string? FileContentType { get; set; }
        public string? FileName { get; set; }
        public string? FilePreviewUrl { get; set; }
        public string? FileDownloadUrl { get; set; }
        public bool HasFile { get; set; }
        public string? Notes { get; set; }
        public bool IsVerified { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class WorkforceCompetencyAssessmentDetailResponse : WorkforceCompetencyAssessmentResponse
    {
        public DateTime? UpdateDateTime { get; set; }
        public Guid CreateBy { get; set; }
        public Guid UpdateBy { get; set; }
    }

    public class WorkforceCompetencyAssessmentOptionResponse
    {
        public Guid Id { get; set; }
        public Guid CompetencyId { get; set; }
        public string CompetencyCode { get; set; } = string.Empty;
        public string CompetencyName { get; set; } = string.Empty;
        public CompetencyCategory CompetencyCategory { get; set; }
        public DateTime AssessmentDate { get; set; }
        public CompetencyLevel CompetencyLevel { get; set; }
        public CompetencyAssessmentResultStatus ResultStatus { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsExpired { get; set; }
        public bool IsVerified { get; set; }
        public bool HasFile { get; set; }
    }

    public class WorkforceCompetencyAssessmentOptionPagedResponse
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkforceCompetencyAssessmentOptionResponse> Items { get; set; } = new();
    }

    public class WorkforceCompetencyAssessmentFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string DateTimeFormat { get; set; } = "yyyy-MM-ddTHH:mm:ssZ";
        public string ResetButtonLabel { get; set; } = "Reset";
        public string FileUploadInfo { get; set; } = "Gunakan multipart/form-data untuk POST dan PUT. Field File hanya diisi jika ada dokumen hasil assessment. Maksimal 10 MB. Format: PDF, JPG, JPEG, PNG, DOC, DOCX, XLS, XLSX.";
        public string RelationFilterInfo { get; set; } = "Filter relasi utama dibatasi 2: competencyId dan assessedByUserId. Filter kategori/level/status memakai enum agar frontend mudah membuat dropdown.";
        public WorkforceCompetencyAssessmentDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<WorkforceCompetencyAssessmentCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<WorkforceCompetencyAssessmentSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<WorkforceCompetencyAssessmentEnumOptionResponse> CompetencyCategories { get; set; } = new();
        public List<WorkforceCompetencyAssessmentEnumOptionResponse> CompetencyLevels { get; set; } = new();
        public List<WorkforceCompetencyAssessmentEnumOptionResponse> ResultStatuses { get; set; } = new();
        public List<WorkforceCompetencyAssessmentMatrixStatusOptionResponse> MatrixStatuses { get; set; } = new();
        public List<WorkforceCompetencyAssessmentGuideResponse> FrontendGuide { get; set; } = new();
    }

    public class WorkforceCompetencyAssessmentDefaultFilterResponse
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public Guid? CompetencyId { get; set; }
        public CompetencyCategory? CompetencyCategory { get; set; }
        public CompetencyLevel? CompetencyLevel { get; set; }
        public CompetencyAssessmentResultStatus? ResultStatus { get; set; }
        public Guid? AssessedByUserId { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsExpired { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "assessmentDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class WorkforceCompetencyAssessmentCustomPeriodOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceCompetencyAssessmentSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class WorkforceCompetencyAssessmentEnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class WorkforceCompetencyAssessmentMatrixStatusOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class WorkforceCompetencyAssessmentGuideResponse
    {
        public string FieldName { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Example { get; set; } = string.Empty;
    }

    public class CreateWorkforceCompetencyAssessmentRequest
    {
        [Required]
        [Display(Name = "Kompetensi", Description = "Ambil dari endpoint options master competency. Wajib dipilih.")]
        public Guid CompetencyId { get; set; }

        [Required]
        [Display(Name = "Tanggal Assessment", Description = "Tanggal assessment kompetensi. Format aman: yyyy-MM-ddTHH:mm:ssZ.")]
        public DateTime AssessmentDate { get; set; }

        [Display(Name = "Level Kompetensi", Description = "Gunakan enum CompetencyLevel dari filters/metadata.")]
        public CompetencyLevel CompetencyLevel { get; set; } = CompetencyLevel.None;

        [Display(Name = "Hasil Assessment", Description = "Gunakan enum ResultStatus dari filters/metadata, misalnya Passed, Failed, NeedTraining, atau NotAssessed.")]
        public CompetencyAssessmentResultStatus ResultStatus { get; set; } = CompetencyAssessmentResultStatus.NotAssessed;

        [Display(Name = "Assessor", Description = "User assessor. Nullable jika belum ada assessor.")]
        public Guid? AssessedByUserId { get; set; }

        [Display(Name = "Tanggal Expired", Description = "Nullable. Diisi jika assessment punya masa berlaku.")]
        public DateTime? ExpiredDate { get; set; }

        [Display(Name = "File Hasil Assessment", Description = "Upload dokumen hasil assessment. Maksimal 10 MB. PDF, JPG, JPEG, PNG, DOC, DOCX, XLS, XLSX.")]
        public IFormFile? File { get; set; }

        [MaxLength(500)]
        [Display(Name = "Catatan", Description = "Catatan hasil assessment atau rekomendasi training.")]
        public string? Notes { get; set; }

        [Display(Name = "Terverifikasi", Description = "true jika hasil assessment sudah diverifikasi HR/Assessor berwenang.")]
        public bool IsVerified { get; set; } = false;

        [Display(Name = "Aktif", Description = "Status aktif data assessment.")]
        public bool IsActive { get; set; } = true;
    }

    public class UpdateWorkforceCompetencyAssessmentRequest : CreateWorkforceCompetencyAssessmentRequest
    {
        [Display(Name = "Ganti File Lama", Description = "Jika true dan File diisi, file lama diganti. Jika true dan File kosong, file lama dihapus.")]
        public bool ReplaceExistingFile { get; set; } = false;
    }

    public class UpdateWorkforceCompetencyAssessmentStatusRequest
    {
        public bool IsActive { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class VerifyWorkforceCompetencyAssessmentRequest
    {
        public bool IsVerified { get; set; } = true;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class DeleteWorkforceCompetencyAssessmentFileRequest
    {
        public bool DeletePhysicalFile { get; set; } = true;
    }

    public class WorkforceCompetencyMatrixItemResponse
    {
        public Guid RequirementId { get; set; }
        public Guid PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public Guid CompetencyId { get; set; }
        public string CompetencyCode { get; set; } = string.Empty;
        public string CompetencyName { get; set; } = string.Empty;
        public CompetencyCategory CompetencyCategory { get; set; }
        public bool IsRequired { get; set; }
        public CompetencyLevel MinimumLevel { get; set; }
        public bool IsCertificationRequired { get; set; }
        public bool IsTrainingRequired { get; set; }
        public Guid? LatestAssessmentId { get; set; }
        public CompetencyLevel? LatestCompetencyLevel { get; set; }
        public CompetencyAssessmentResultStatus? LatestResultStatus { get; set; }
        public DateTime? LatestAssessmentDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public bool IsVerified { get; set; }
        public bool IsExpired { get; set; }
        public bool IsPassed { get; set; }
        public bool IsLevelMet { get; set; }
        public string Status { get; set; } = "Missing";
    }

    public class WorkforceCompetencyMatrixResponse
    {
        public Guid WorkforceProfileId { get; set; }
        public string ProfileCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserType UserType { get; set; }
        public Guid? PrimaryPositionId { get; set; }
        public string? PrimaryPositionName { get; set; }
        public int TotalRequirement { get; set; }
        public int CompletedRequirement { get; set; }
        public int MissingRequirement { get; set; }
        public int NeedTrainingRequirement { get; set; }
        public int ExpiredRequirement { get; set; }
        public int FailedRequirement { get; set; }
        public int NeedVerificationRequirement { get; set; }
        public int NotMetRequirement { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalData { get; set; }
        public int TotalPage { get; set; }
        public List<WorkforceCompetencyMatrixItemResponse> Items { get; set; } = new();
    }
}
