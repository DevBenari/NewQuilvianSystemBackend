using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs
{
    // =========================================================================
    // DTOs Validasi Gerbang SIMRS EMR (BE-HRD-216, INT-HR-69)
    // =========================================================================

    public class ClinicalPrivilegeGateValidationRequestDto
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PrivilegeCode { get; set; } = string.Empty;

        public Guid? EncounterId { get; set; }
        public Guid? PatientId { get; set; }
    }

    public class ClinicalPrivilegeGateValidationResultDto
    {
        public bool IsAllowed { get; set; }
        public string GateDecision { get; set; } = "HardStopBlocked"; // Allowed, SupervisionRequired, HardStopBlocked, ExpiredLicenseBlocked
        public string SupervisionLevel { get; set; } = "Level4_NotPermitted"; // Level1_Independent, Level2_UnderSupervision, Level3_Proctored, Level4_NotPermitted
        public bool RequiresCoSignature { get; set; }
        public Guid? AssignedSupervisorUserId { get; set; }
        public string? AssignedSupervisorName { get; set; }
        public string Message { get; set; } = string.Empty;
        public string LicenseRadarStatus { get; set; } = "Normal";
        public bool IsLifeSavingOverrideAvailable { get; set; } = true;
        public DateTime CheckedAtUtc { get; set; } = DateTime.UtcNow;
    }

    // =========================================================================
    // DTOs Alur Co-Signature Supervisi 24 Jam (BE-HRD-217, VR-CRD-07)
    // =========================================================================

    public class CoSignatureActionDto
    {
        [Required]
        public Guid ClinicalPrivilegeId { get; set; }

        [Required]
        [RegularExpression(@"^(Approved|Rejected)$", ErrorMessage = "Keputusan Co-Signature wajib Approved atau Rejected.")]
        public string Decision { get; set; } = "Approved";

        [MaxLength(1000)]
        public string? SupervisorNotes { get; set; }
    }

    public class CoSignatureResultDto
    {
        public bool IsSuccess { get; set; }
        public string Decision { get; set; } = string.Empty;
        public Guid PrivilegeId { get; set; }
        public DateTime CoSignedAtUtc { get; set; }
        public string SupervisorUserName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    // =========================================================================
    // DTOs Life-Saving Emergency Override (BE-HRD-218, INT-HR-70)
    // =========================================================================

    public class ClinicalEmergencyOverrideRequestDto
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MaxLength(100)]
        public string PrivilegeItemCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(250)]
        public string ProcedureName { get; set; } = string.Empty;

        public Guid? PatientId { get; set; }
        public Guid? EncounterId { get; set; }

        [Required]
        [MinLength(20, ErrorMessage = "Justifikasi medis darurat (Life-Saving) wajib diisi minimal 20 karakter secara jelas.")]
        [MaxLength(2000)]
        public string EmergencyJustification { get; set; } = string.Empty;

        public Guid? WitnessUserId { get; set; }

        [MaxLength(150)]
        public string? WitnessName { get; set; }
    }

    public class EmergencyOverrideLogDto
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string DoctorCode { get; set; } = string.Empty;
        public string PrivilegeItemCode { get; set; } = string.Empty;
        public string ProcedureName { get; set; } = string.Empty;
        public Guid? PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public string OverrideReason { get; set; } = string.Empty;
        public DateTime OverrideAtUtc { get; set; }
        public string? WitnessName { get; set; }
        public bool IsReviewedByKmkp { get; set; }
        public string KmkpVerdict { get; set; } = "PendingReview";
        public string? KmkpReviewNotes { get; set; }
        public DateTime? ReviewedAtUtc { get; set; }
        public string? ReviewerUserName { get; set; }
    }

    public class ReviewEmergencyOverrideDto
    {
        [Required]
        public Guid OverrideLogId { get; set; }

        [Required]
        [RegularExpression(@"^(Justified|Unjustified|DisciplinaryAction)$", ErrorMessage = "Putusan KMKP wajib Justified, Unjustified, atau DisciplinaryAction.")]
        public string Verdict { get; set; } = "Justified";

        [Required]
        [MaxLength(2000)]
        public string ReviewNotes { get; set; } = string.Empty;
    }

    // =========================================================================
    // DTOs Temporary & Emergency Privilege (BE-HRD-219, HRD-DEC-150)
    // =========================================================================

    public class GrantTemporaryPrivilegeDto : IValidatableObject
    {
        [Required]
        public Guid WorkforceProfileId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Daftar butir kewenangan klinis sementara tidak boleh kosong.")]
        public List<Guid> PrivilegeCatalogIds { get; set; } = new List<Guid>();

        [Required]
        [RegularExpression(@"^(Provisional|Emergency)$", ErrorMessage = "Sifat kewenangan sementara wajib Provisional atau Emergency.")]
        public string PrivilegeNature { get; set; } = "Provisional"; // Provisional (Dokter Tamu), Emergency (Bencana)

        [Range(1, 90, ErrorMessage = "Masa berlaku kewenangan sementara maksimal 90 hari.")]
        public int ValidityDays { get; set; } = 30;

        public Guid? SupervisorUserId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ValidityDays > 90)
            {
                yield return new ValidationResult(
                    "Kewenangan klinis sementara tidak boleh melebihi batas waktu 90 hari sesuai Permenkes.",
                    new[] { nameof(ValidityDays) });
            }
        }
    }

    public class TemporaryPrivilegeResultDto
    {
        public bool IsSuccess { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string PrivilegeNature { get; set; } = string.Empty;
        public DateTime EffectiveStartDate { get; set; }
        public DateTime EffectiveEndDate { get; set; }
        public int TotalPrivilegesGranted { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
