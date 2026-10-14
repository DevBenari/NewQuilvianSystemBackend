using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs
{
    // BE-FIN-103, FIN-API-1.10 R.2. Kode hubungan TIDAK diterima dari klien — diturunkan dari
    // data anggota keluarga HR (FIN-INTEGRATION-1.9 R.1.1, FIN-VAL-260).

    public sealed class CreateEmployeeBenefitDependentRequest
    {
        // Delta kontrak BE-FIN-103: FIN-API-1.10 R.2 menyebut Description opsional, tetapi
        // TrxEmployeeBenefitDependent tidak punya kolom keterangan; field itu tidak dibuat.
        [Required] public Guid FamilyMemberId { get; set; }
    }

    public sealed class DependentTransitionRequest
    {
        [Required] public Guid RowVersion { get; set; }
    }

    public sealed class VerifyEmployeeBenefitDependentRequest
    {
        [Required] public Guid RowVersion { get; set; }
        [Required] public DateTime EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        [Range(typeof(decimal), "0.01", "999999999999999")] public decimal CoverageLimitAmount { get; set; }
        [MaxLength(500)] public string? VerificationNotes { get; set; }
    }

    public sealed class RejectEmployeeBenefitDependentRequest
    {
        [Required] public Guid RowVersion { get; set; }
        [Required, MaxLength(500)] public string EligibilityReason { get; set; } = string.Empty;
    }

    public sealed class EndEmployeeBenefitDependentRequest
    {
        [Required] public Guid RowVersion { get; set; }
        [Required] public DateTime EffectiveEndDate { get; set; }
        [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    }

    public sealed class EmployeeBenefitDependentResponse
    {
        public Guid Id { get; set; }
        public Guid EmployeeBenefitEnrollmentId { get; set; }
        public Guid? FamilyMemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string RelationshipCode { get; set; } = string.Empty;
        public string DependentStatus { get; set; } = string.Empty;
        public bool IsEligible { get; set; }
        public bool IsCovered { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
        public decimal LimitAmount { get; set; }
        public decimal UsedAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string? EligibilityReason { get; set; }
        public DateTime? RequestedAt { get; set; }
        public DateTime? VerifiedAt { get; set; }
        public string? VerificationNotes { get; set; }
        public Guid RowVersion { get; set; }
        public List<string> AvailableActions { get; set; } = new();
    }
}
