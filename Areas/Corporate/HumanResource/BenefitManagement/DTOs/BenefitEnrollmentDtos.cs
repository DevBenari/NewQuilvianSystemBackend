using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs
{
    // =========================================================================
    // 1. QUERY & LIST DTOS ENROLLMENT ASURANSI
    // =========================================================================

    public class EmployeeEnrollmentListQuery
    {
        public string? Status { get; set; }
        public Guid? UnitId { get; set; }
        public string? SearchKeyword { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class EmployeeEnrollmentListItemDto
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string EmploymentStatus { get; set; } = string.Empty;
        public DateTime? JoinDate { get; set; }
        public string BenefitPlanName { get; set; } = string.Empty;
        public string CoverageTier { get; set; } = string.Empty;
        public string? BpjsKetenagakerjaanNumber { get; set; }
        public string? BpjsKesehatanNumber { get; set; }
        public string? PrivateInsurancePolicyNumber { get; set; }
        public string EnrollmentStatus { get; set; } = "Draft"; // Draft, PendingActivation, Active, Suspended, Terminated
        public DateTime? EffectiveStartDate { get; set; }
        public int ActiveDependentCount { get; set; } = 0;
        public bool IsProbation { get; set; } = false;
    }

    public class ActivateProbationRequestDto
    {
        [Required(ErrorMessage = "ID evaluasi masa percobaan wajib diisi.")]
        public Guid ProbationEvaluationId { get; set; }

        public DateTime? EffectiveDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class UpdatePolicyNumbersRequestDto
    {
        [RegularExpression(@"^[0-9]{13}$", ErrorMessage = "Nomor kartu BPJS Kesehatan tidak valid. Nomor kepesertaan BPJS Kesehatan wajib 13 digit angka.")]
        public string? BpjsKesehatanNumber { get; set; }

        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "Nomor kartu BPJS Ketenagakerjaan tidak valid. Nomor KPJ BPJS TK wajib 11 digit angka.")]
        public string? BpjsKetenagakerjaanNumber { get; set; }

        [MaxLength(100)]
        public string? PrivateInsurancePolicyNumber { get; set; }

        [MaxLength(150)]
        public string? PrivateInsuranceProviderName { get; set; }
    }

    public class BenefitEnrollmentDetailDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeBenefitEnrollmentId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public Guid BenefitPlanId { get; set; }
        public string BenefitPlanCode { get; set; } = string.Empty;
        public string BenefitPlanName { get; set; } = string.Empty;
        public string BenefitCategory { get; set; } = string.Empty;
        public string CoverageTier { get; set; } = string.Empty;
        public string? BpjsKetenagakerjaanNumber { get; set; }
        public string? BpjsKesehatanNumber { get; set; }
        public string? PrivateInsurancePolicyNumber { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string EnrollmentStatus { get; set; } = "Draft";
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public Guid? ProbationEvaluationId { get; set; }
        public int CoveredDependentCount { get; set; } = 0;
        public List<BenefitDependentDetailDto> Dependents { get; set; } = new();
    }

    // =========================================================================
    // 2. DTOS PENDAFTARAN TANGGUNGAN ESS & KUOTA KELUARGA
    // =========================================================================

    public class CreateEssDependentRequestDto
    {
        [Required(ErrorMessage = "Format NIK tidak valid. NIK wajib terdiri dari tepat 16 digit angka kependudukan.")]
        [RegularExpression(@"^[0-9]{16}$", ErrorMessage = "Format NIK tidak valid. NIK wajib terdiri dari tepat 16 digit angka kependudukan.")]
        public string NationalIdNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nomor Kartu Keluarga tidak valid. Nomor KK wajib terdiri dari tepat 16 digit angka.")]
        [RegularExpression(@"^[0-9]{16}$", ErrorMessage = "Nomor Kartu Keluarga tidak valid. Nomor KK wajib terdiri dari tepat 16 digit angka.")]
        public string FamilyCardNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama lengkap anggota keluarga wajib diisi.")]
        [MaxLength(150, ErrorMessage = "Nama tanggungan maksimal 150 karakter.")]
        public string DependentName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Jenis hubungan keluarga wajib diisi (Spouse atau Child).")]
        public string RelationshipType { get; set; } = string.Empty; // "Spouse" atau "Child"

        [Required(ErrorMessage = "Tanggal lahir anggota keluarga wajib diisi.")]
        public DateTime BirthDate { get; set; }

        public string? Gender { get; set; } // Male / Female

        public bool IsActiveStudent { get; set; } = false;

        public string? StudentVerificationLetterPath { get; set; }

        public DateTime? StudentVerificationExpiryDate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class EmployeeDependentSummaryDto
    {
        public Guid Id { get; set; }
        public string DependentName { get; set; } = string.Empty;
        public string NationalIdNumber { get; set; } = string.Empty;
        public string FamilyCardNumber { get; set; } = string.Empty;
        public string RelationshipType { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public int AgeYears { get; set; }
        public string? Gender { get; set; }
        public bool IsActiveStudent { get; set; }
        public string? StudentVerificationLetterPath { get; set; }
        public DateTime? StudentVerificationExpiryDate { get; set; }
        public string VerificationStatus { get; set; } = "Draft"; // Draft, Submitted, Verified, Rejected, ExpiredAgeLimit
        public decimal CoverageLimitAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class EmployeeFamilyQuotaSummaryDto
    {
        public int MaxSpouse { get; set; } = 1;
        public int CurrentSpouseCount { get; set; }
        public bool CanAddSpouse { get; set; }

        public int MaxChild { get; set; } = 3;
        public int CurrentChildCount { get; set; }
        public bool CanAddChild { get; set; }

        public int TotalDependents { get; set; }
        public int VerifiedDependents { get; set; }
        public int PendingVerificationCount { get; set; }
    }

    // =========================================================================
    // 3. MEJA KERJA VERIFIKASI DOKUMEN HR BENEFIT OFFICER
    // =========================================================================

    public class PendingDependentVerificationQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SearchKeyword { get; set; }
    }

    public class PendingDependentVerificationDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeBenefitEnrollmentId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string DependentName { get; set; } = string.Empty;
        public string NationalIdNumber { get; set; } = string.Empty;
        public string FamilyCardNumber { get; set; } = string.Empty;
        public string RelationshipType { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public int AgeYears { get; set; }
        public bool IsActiveStudent { get; set; }
        public string? StudentVerificationLetterPath { get; set; }
        public DateTime? StudentVerificationExpiryDate { get; set; }
        public string VerificationStatus { get; set; } = "Submitted";
        public DateTime? RequestedAt { get; set; }
    }

    public class VerifyDependentRequestDto
    {
        [Required(ErrorMessage = "Status verifikasi wajib diisi ('Verified' atau 'Rejected').")]
        public string VerificationStatus { get; set; } = "Verified"; // Verified, Rejected

        public decimal? CoverageLimitAmount { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        [MaxLength(1000)]
        public string? VerificationNotes { get; set; }
    }

    public class BenefitDependentDetailDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeBenefitEnrollmentId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string DependentName { get; set; } = string.Empty;
        public string NationalIdNumber { get; set; } = string.Empty;
        public string FamilyCardNumber { get; set; } = string.Empty;
        public string RelationshipType { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public int AgeYears { get; set; }
        public string? GenderSnapshot { get; set; }
        public bool IsActiveStudent { get; set; }
        public string? StudentVerificationLetterPath { get; set; }
        public DateTime? StudentVerificationExpiryDate { get; set; }
        public string VerificationStatus { get; set; } = "Draft";
        public decimal CoverageLimitAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public bool IsEligible { get; set; }
        public bool IsCovered { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerifiedBy { get; set; }
        public string? RejectionReason { get; set; }
        public string? VerificationNotes { get; set; }
    }
}
