namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs
{
    /// <summary>Filter daftar sanggahan dan pengajuan jenjang klinis seluruh pegawai (MSL-14, DEC-FER-05 opsi A, 18 Nov 2026).</summary>
    public class PerformanceListQuery
    {
        public string? Search { get; set; }
        public string? Status { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class AppraisalDisputeListItemDto
    {
        public Guid Id { get; set; }
        public string DisputeNumber { get; set; } = string.Empty;
        public Guid PerformanceAppraisalId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? ProfileCode { get; set; }
        public DateTime DisputeDate { get; set; }
        public string DisputedComponent { get; set; } = string.Empty;
        public string DisputeReason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? FinalVerdict { get; set; }
        public decimal? AdjustedCompositeScore { get; set; }
        public DateTime? VerdictDeliveredAt { get; set; }
        // Nama yang sama dengan respons sanggahan per pegawai, supaya layar memakai satu set kolom.
        public decimal? FinalResolutionScore { get; set; }
        public string? InvestigationNotes { get; set; }
        public DateTime? AdjudicatedDateTime { get; set; }
    }

    public class CareerLadderApplicationListItemDto
    {
        public Guid Id { get; set; }
        public string ReviewNumber { get; set; } = string.Empty;
        public Guid WorkforceProfileId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string? ProfileCode { get; set; }
        public string CurrentClinicalLevel { get; set; } = string.Empty;
        public string TargetClinicalLevel { get; set; } = string.Empty;
        public decimal ClinicalExperienceYears { get; set; }
        public bool MeetsPerformanceThreshold { get; set; }
        public bool MeetsClinicalCompetency { get; set; }
        public bool FreeFromSentinelIncident { get; set; }
        public string? CommitteeRecommendation { get; set; }
        public string FinalStatus { get; set; } = string.Empty;
        public DateTime? ApprovedAt { get; set; }
        public string? NewSkNumber { get; set; }
        public DateTime SubmittedAt { get; set; }
        // Nama yang sama dengan respons pengajuan jenjang (apply/verdict), supaya layar dan modal putusan memakai satu bentuk.
        public string CurrentLevel { get; set; } = string.Empty;
        public string RequestedLevel { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; }
        public string? KomiteRecommendation { get; set; }
        public string? DecreeNumber { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
