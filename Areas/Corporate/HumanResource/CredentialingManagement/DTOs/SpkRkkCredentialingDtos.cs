using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs
{
    // =========================================================================
    // DTOs Komite Kredensial & Katalog Whitebook (BE-HRD-211)
    // =========================================================================

    public class CredentialingCommitteeDto
    {
        public Guid Id { get; set; }
        public string CommitteeCode { get; set; } = string.Empty; // MEDIK, NURSING, OTHER_HEALTH
        public string CommitteeName { get; set; } = string.Empty;
        public string SubCommitteeCredentialName { get; set; } = string.Empty;
        public Guid? ChairpersonWorkforceProfileId { get; set; }
        public string? ChairpersonName { get; set; }
        public Guid? SecretaryWorkforceProfileId { get; set; }
        public string? SecretaryName { get; set; }
        public int MinimumPeerReviewersCount { get; set; } = 2;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class ClinicalPrivilegeCatalogItemDto
    {
        public Guid Id { get; set; }
        public string PrivilegeCode { get; set; } = string.Empty;
        public string PrivilegeName { get; set; } = string.Empty;
        public string PrivilegeCategory { get; set; } = string.Empty;
        public string? ReferenceProcedureCode { get; set; }
        public bool RequiresSupervision { get; set; }
        public bool AllowsIndependentPractice { get; set; }
        public bool IsHighRisk { get; set; }
        public string? Description { get; set; }
    }

    // =========================================================================
    // DTOs Pengajuan Kredensial & Panel Mitra Bestari (BE-HRD-212)
    // =========================================================================

    public class CreateCredentialingApplicationDto
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        public Guid CommitteeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ApplicationType { get; set; } = "InitialCredentialing"; // InitialCredentialing, Recredentialing, PrivilegeExpansion

        public Guid? ProfessionId { get; set; }
        public Guid? SpecializationId { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public List<Guid>? RequestedPrivilegeCatalogIds { get; set; } = new List<Guid>();
    }

    public class CredentialingApplicationDto
    {
        public Guid Id { get; set; }
        public string ApplicationNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string ApplicantName { get; set; } = string.Empty;
        public string ApplicantEmployeeCode { get; set; } = string.Empty;
        public string CommitteeCode { get; set; } = string.Empty;
        public string CommitteeName { get; set; } = string.Empty;
        public string ApplicationType { get; set; } = string.Empty;
        public string ApplicationStatus { get; set; } = string.Empty; // Draft, Submitted, InReview, PlenarySession, Approved, Rejected
        public DateTime ApplicationDate { get; set; }
        public bool IsLicensePsvVerified { get; set; }
        public List<string> AssignedReviewerNames { get; set; } = new List<string>();
        public string? Notes { get; set; }
    }

    public class AssignPeerReviewersDto : IValidatableObject
    {
        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "Panel Mitra Bestari wajib beranggotakan minimal 2 orang reviewer sebidang (HRD-DEC-147).")]
        public List<Guid> ReviewerWorkforceProfileIds { get; set; } = new List<Guid>();

        [MaxLength(1000)]
        public string? AssignmentNotes { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ReviewerWorkforceProfileIds.Distinct().Count() < ReviewerWorkforceProfileIds.Count)
            {
                yield return new ValidationResult(
                    "Anggota panel Mitra Bestari tidak boleh duplikat.",
                    new[] { nameof(ReviewerWorkforceProfileIds) });
            }
        }
    }

    // =========================================================================
    // DTOs Asesmen Mitra Bestari & Sidang Pleno (BE-HRD-213)
    // =========================================================================

    public class PrivilegeAssessmentItemDto
    {
        [Required]
        public Guid CatalogId { get; set; }

        [Required]
        public SupervisionLevel RecommendedSupervisionLevel { get; set; } = SupervisionLevel.Level1_Independent;

        [MaxLength(1000)]
        public string? ReviewerNotes { get; set; }
    }

    public class SubmitPeerReviewAssessmentDto
    {
        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        public Guid ReviewerWorkforceProfileId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Daftar butir asesmen kewenangan klinis tidak boleh kosong.")]
        public List<PrivilegeAssessmentItemDto> AssessmentItems { get; set; } = new List<PrivilegeAssessmentItemDto>();

        [MaxLength(2000)]
        public string? OverallRecommendation { get; set; }
    }

    public class ConcludePlenarySessionDto
    {
        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        [MaxLength(80)]
        public string PlenaryBapNumber { get; set; } = string.Empty; // Nomor Berita Acara Pleno

        [Required]
        public DateTime PlenaryDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string PlenaryDecision { get; set; } = "Recommended"; // Recommended, ConditionallyRecommended, Rejected

        [MaxLength(2000)]
        public string? CommitteeChairpersonNotes { get; set; }

        [Required]
        public List<PrivilegeAssessmentItemDto> FinalApprovedPrivileges { get; set; } = new List<PrivilegeAssessmentItemDto>();
    }

    // =========================================================================
    // DTOs Penerbitan & Pengesahan SPK-RKK (BE-HRD-214 & BE-HRD-215)
    // =========================================================================

    public class IssueSpkRkkRequestDto
    {
        [Required]
        public Guid ApplicationId { get; set; }

        [Required]
        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

        [MaxLength(50)]
        public string ClinicalPrivilegeType { get; set; } = "Regular"; // Regular, Provisional, Emergency

        [MaxLength(1000)]
        public string? Notes { get; set; }
    }

    public class SignSpkRkkDto
    {
        [Required]
        public Guid SpkRkkIssuanceId { get; set; }

        [MaxLength(500)]
        public string? DigitalSignaturePassphrase { get; set; } // Passphrase e-Sign Direktur Utama
    }

    public class SpkRkkDetailDto
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeeCode { get; set; } = string.Empty;
        public string Profession { get; set; } = string.Empty;
        public string CommitteeName { get; set; } = string.Empty;
        public string SpkNumber { get; set; } = string.Empty;
        public string RkkNumber { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }
        public string ClinicalPrivilegeType { get; set; } = string.Empty;
        public string DirectorGeneralName { get; set; } = string.Empty;
        public bool IsSignedByDirectorGeneral { get; set; }
        public DateTime? SignedAt { get; set; }
        public string? VerificationHash { get; set; }
        public string? SpkPdfUrl { get; set; }
        public string? QrCodeUrl { get; set; }
        public Guid? EDossierDocumentId { get; set; }
        public string IssuanceStatus { get; set; } = "Active";
        public List<PrivilegeAssessmentItemDto> ApprovedPrivileges { get; set; } = new List<PrivilegeAssessmentItemDto>();
    }
}
