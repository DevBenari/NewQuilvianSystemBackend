using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs
{
    public class OsceRubricItemDto
    {
        [Required]
        public string ItemCode { get; set; } = string.Empty;

        [Required]
        public string ItemDescription { get; set; } = string.Empty;

        public decimal MaxScore { get; set; } = 20.00m;

        public decimal EarnedScore { get; set; }

        public bool IsCriticalStepPassed { get; set; } = true;

        public string? AssessorNotes { get; set; }
    }

    public class SubmitClinicalOsceRequestDto
    {
        [Required(ErrorMessage = "WorkforceProfileId wajib diisi.")]
        public Guid WorkforceProfileId { get; set; }

        public Guid? CompetencyId { get; set; }

        [Required(ErrorMessage = "Nama/kode stase OSCE wajib diisi.")]
        [MaxLength(150)]
        public string StationName { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "Nilai total harus berada di rentang 0 sampai 100.")]
        public decimal TotalScore { get; set; }

        public decimal PassingScore { get; set; } = 80.00m;

        [Required(ErrorMessage = "Tanda tangan digital dokter assessor wajib disertakan (VR-TRN-06).")]
        public string DigitalSignatureHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID Dokter Assessor penguji wajib diisi.")]
        public Guid AssessorDoctorId { get; set; }

        [MaxLength(200)]
        public string AssessorDoctorName { get; set; } = string.Empty;

        public List<OsceRubricItemDto> RubricItems { get; set; } = new List<OsceRubricItemDto>();

        [MaxLength(2000)]
        public string? ClinicalFeedback { get; set; }
    }

    public class ClinicalOsceResultDto
    {
        public Guid Id { get; set; }
        public Guid TrainingSessionId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? StaffName { get; set; }
        public Guid? CompetencyId { get; set; }
        public string? CompetencyName { get; set; }
        public string StationName { get; set; } = string.Empty;
        public decimal TotalScore { get; set; }
        public decimal PassingScore { get; set; } = 80.00m;
        public bool IsPassed { get; set; }
        public Guid AssessorDoctorId { get; set; }
        public string AssessorDoctorName { get; set; } = string.Empty;
        public string DigitalSignatureHash { get; set; } = string.Empty;
        public bool IsSignatureVerified { get; set; }
        public string ResultVerdict { get; set; } = string.Empty;
        public DateTime AssessedAt { get; set; }
        public string? ClinicalFeedback { get; set; }
    }
}
