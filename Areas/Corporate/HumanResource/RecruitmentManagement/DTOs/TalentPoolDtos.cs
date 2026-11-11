using System;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.RecruitmentManagement.DTOs
{
    public sealed class EnrollTalentPoolRequest
    {
        [Required(ErrorMessage = "ID Kandidat (CandidateId) wajib diisi.")]
        public Guid CandidateId { get; set; }

        [Required(ErrorMessage = "ID Lowongan Asal (OriginalVacancyId) wajib diisi.")]
        public Guid OriginalVacancyId { get; set; }

        [Required(ErrorMessage = "Ringkasan kualifikasi wajib diisi.")]
        [MaxLength(500, ErrorMessage = "Ringkasan kualifikasi maksimal 500 karakter.")]
        public string QualificationSummary { get; set; } = string.Empty;

        [Required(ErrorMessage = "Skor pemeringkatan (RankingScore) wajib diisi.")]
        [Range(0, 100, ErrorMessage = "Skor pemeringkatan berada pada rentang 0 sampai 100.")]
        public decimal RankingScore { get; set; }

        [Required(ErrorMessage = "Posisi peringkat (RankingPosition) wajib diisi.")]
        [Range(1, 999, ErrorMessage = "Posisi peringkat minimal 1.")]
        public int RankingPosition { get; set; }

        [Range(1, 12, ErrorMessage = "Durasi retensi antara 1 sampai 12 bulan.")]
        public int RetentionMonths { get; set; } = 6;
    }

    public sealed class FastTrackPromoteRequest
    {
        [Required(ErrorMessage = "ID Lowongan Target (TargetVacancyId) wajib diisi.")]
        public Guid TargetVacancyId { get; set; }

        [Required(ErrorMessage = "Gaji pokok penawaran (OfferedBasicSalary) wajib diisi.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Gaji pokok harus lebih dari 0.")]
        public decimal OfferedBasicSalary { get; set; }

        [Required(ErrorMessage = "ID Skala Upah (SalaryGradeId) wajib diisi.")]
        public Guid SalaryGradeId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public sealed class TalentPoolResponse
    {
        public Guid Id { get; set; }
        public Guid CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateNik { get; set; } = string.Empty;
        public Guid OriginalVacancyId { get; set; }
        public string OriginalVacancyTitle { get; set; } = string.Empty;
        public string QualificationSummary { get; set; } = string.Empty;
        public decimal RankingScore { get; set; }
        public int RankingPosition { get; set; }
        public DateTime EnrolledDate { get; set; }
        public DateTime RetentionExpiryDate { get; set; }
        public TalentPoolStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public Guid? PromotedToOfferId { get; set; }
        public bool IsExpired => DateTime.UtcNow > RetentionExpiryDate;
        public DateTime CreateDateTime { get; set; }
    }

    public sealed class TalentPoolListItemResponse
    {
        public Guid Id { get; set; }
        public Guid CandidateId { get; set; }
        public string CandidateName { get; set; } = string.Empty;
        public string CandidateNik { get; set; } = string.Empty;
        public string OriginalVacancyTitle { get; set; } = string.Empty;
        public decimal RankingScore { get; set; }
        public int RankingPosition { get; set; }
        public DateTime RetentionExpiryDate { get; set; }
        public TalentPoolStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public bool IsExpired => DateTime.UtcNow > RetentionExpiryDate;
    }

    public sealed class TalentPoolQuery
    {
        public Guid? VacancyId { get; set; }
        public TalentPoolStatus? Status { get; set; }
        public string? SearchTerm { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
