using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs
{
    public class RecordSkpActivityRequestDto
    {
        [Required(ErrorMessage = "WorkforceProfileId wajib diisi.")]
        public Guid WorkforceProfileId { get; set; }

        public Guid? TrainingCertificateId { get; set; }

        [Required(ErrorMessage = "Nama kegiatan SKP wajib diisi.")]
        [MaxLength(100)]
        public string ActivityName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kategori SKP wajib diisi (Pembelajaran, Pelayanan, Pengabdian).")]
        [MaxLength(50)]
        public string SkpCategory { get; set; } = "Pembelajaran";

        [Range(0.25, 50.00, ErrorMessage = "Poin SKP berada di rentang 0.25 sampai 50.00.")]
        public decimal SkpPoints { get; set; } = 1.00m;

        public DateTime ActivityDate { get; set; }

        [MaxLength(100)]
        public string? OrganizerName { get; set; }

        [MaxLength(100)]
        public string? PlataranSehatCertificateId { get; set; }

        [MaxLength(1000)]
        public string? EvidenceDocumentUrl { get; set; }
    }

    public class PlataranSehatSkpRecordDto
    {
        public Guid Id { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string? StaffName { get; set; }
        public Guid? TrainingCertificateId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public string SkpCategory { get; set; } = "Pembelajaran";
        public decimal SkpPoints { get; set; }
        public DateTime ActivityDate { get; set; }
        public string? OrganizerName { get; set; }
        public string? PlataranSehatCertificateId { get; set; }
        public bool IsVerifiedByPlataranSehat { get; set; }
        public DateTime? PlataranSehatVerifiedAt { get; set; }
        public bool IsSyncedToOppe { get; set; }
        public DateTime? OppeSyncedAt { get; set; }
        public string? EvidenceDocumentUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class SkpPortfolioSummaryDto
    {
        public Guid WorkforceProfileId { get; set; }
        public string? StaffName { get; set; }
        public decimal TotalSkpTarget { get; set; } = 50.00m; // Target 5 tahun regulasi Kemenkes
        public decimal TotalSkpEarned { get; set; }
        public decimal SkpPembelajaranEarned { get; set; }
        public decimal SkpPelayananEarned { get; set; }
        public decimal SkpPengabdianEarned { get; set; }
        public decimal ProgressPercentage => Math.Round(Math.Min(100m, (TotalSkpEarned / TotalSkpTarget) * 100m), 2);
        public bool IsTargetAchieved => TotalSkpEarned >= TotalSkpTarget;
        public int TotalActivitiesCount { get; set; }
        public int VerifiedByPlataranSehatCount { get; set; }
        public int SyncedToOppeCount { get; set; }
        public string StatusSummary { get; set; } = string.Empty;
        public List<PlataranSehatSkpRecordDto> Records { get; set; } = new List<PlataranSehatSkpRecordDto>();
    }

    public class SyncOppeResponseDto
    {
        public Guid WorkforceProfileId { get; set; }
        public string? StaffName { get; set; }
        public int SyncedRecordsCount { get; set; }
        public decimal TotalSyncedSkpPoints { get; set; }
        public DateTime SyncedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
