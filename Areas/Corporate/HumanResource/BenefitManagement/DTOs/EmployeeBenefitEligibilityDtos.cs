using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs
{
    // BE-FIN-104, FIN-INTEGRATION-1.9 R.2.3 (PeriksaKelayakan). Kontrak in-process, bukan endpoint.

    public sealed class BenefitEligibilityRequest
    {
        public Guid EmployeeId { get; set; }

        /// <summary>Kosong bila pasiennya pegawai sendiri.</summary>
        public Guid? FamilyMemberId { get; set; }

        /// <summary>Tanggal layanan (tanggal operasional; jam diabaikan).</summary>
        public DateTime ServiceDate { get; set; }
    }

    public sealed class BenefitEligibilityResult
    {
        public bool IsEligible => ReasonCode == BenefitEligibilityReasonCodes.Ok;
        public string ReasonCode { get; set; } = string.Empty;
        public string? Message { get; set; }

        // Terisi hanya bila ReasonCode = OK. Tanpa saldo (R.2.3).
        public Guid EmployeeId { get; set; }
        public Guid? FamilyMemberId { get; set; }
        public string? RelationshipCode { get; set; }
        public string? BenefitPersonKind { get; set; }
        public Guid? BenefitPersonId { get; set; }
        public Guid? EnrollmentId { get; set; }
        public string? PlanCode { get; set; }
        public string? PlanName { get; set; }
        public string? CoverageType { get; set; }
        public DateTime? EffectiveStartDate { get; set; }
        public DateTime? EffectiveEndDate { get; set; }
    }
}
