using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.DTOs
{
    /// <summary>
    /// Permintaan pembuatan draf standar kebutuhan tenaga baru untuk satu unit (BE-HRD-035).
    /// </summary>
    public sealed class CreateStaffingStandardRequest
    {
        [Required(ErrorMessage = "Unit organisasi wajib dipilih.")]
        public Guid OrganizationUnitId { get; set; }

        [Required(ErrorMessage = "Tanggal berlaku mulai wajib diisi.")]
        public DateOnly EffectiveStartDate { get; set; }
    }

    /// <summary>
    /// Permintaan pembaruan draf atau versi standar yang dikembalikan.
    /// </summary>
    public sealed class UpdateStaffingStandardRequest
    {
        [Required(ErrorMessage = "Tanggal berlaku mulai wajib diisi.")]
        public DateOnly EffectiveStartDate { get; set; }

        [MaxLength(1000, ErrorMessage = "Justifikasi maksimal 1000 karakter.")]
        public string? Justification { get; set; }

        public List<StaffingStandardLineRequest> Lines { get; set; } = new();

        public List<StaffingSkillMixRuleRequest> SkillMixRules { get; set; } = new();

        [Required(ErrorMessage = "RowVersion wajib disertakan.")]
        public Guid RowVersion { get; set; }
    }

    /// <summary>
    /// Data satu baris formasi standar kebutuhan tenaga jabatan.
    /// </summary>
    public sealed class StaffingStandardLineRequest
    {
        [Required(ErrorMessage = "Jabatan wajib dipilih.")]
        public Guid PositionId { get; set; }

        public Guid? ProfessionId { get; set; }

        [Required(ErrorMessage = "Metode perhitungan wajib dipilih.")]
        public StaffingCalculationMethod CalculationMethod { get; set; }

        public decimal? MinimumHeadcount { get; set; }

        public decimal? RatioStaffQuantity { get; set; }

        public decimal? RatioWorkloadQuantity { get; set; }

        [MaxLength(50, ErrorMessage = "Satuan beban maksimal 50 karakter.")]
        public string? WorkloadUnit { get; set; }

        [Range(1, 10, ErrorMessage = "Jumlah shift per hari harus antara 1 dan 10.")]
        public int? ShiftsPerDay { get; set; }

        public decimal? StaffPerShift { get; set; }

        public decimal? ServiceHoursPerDay { get; set; }

        public decimal? ReliefFactor { get; set; }

        [Required(ErrorMessage = "Metode pembulatan wajib dipilih.")]
        public StaffingRoundingMethod RoundingMethod { get; set; }

        public bool CountsOutsourced { get; set; } = false;

        [MaxLength(500, ErrorMessage = "Catatan maksimal 500 karakter.")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Aturan kombinasi keahlian (Skill Mix) untuk standar kebutuhan tenaga.
    /// </summary>
    public sealed class StaffingSkillMixRuleRequest
    {
        public Guid? ShiftId { get; set; }

        public Guid? ProfessionId { get; set; }

        [Required(ErrorMessage = "Kompetensi wajib dipilih.")]
        public Guid CompetencyId { get; set; }

        [MaxLength(50, ErrorMessage = "Level kompetensi minimal maksimal 50 karakter.")]
        public string? MinimumCompetencyLevel { get; set; }

        [Range(0.01, 999999.99, ErrorMessage = "Jumlah minimum tenaga berkualifikasi harus lebih besar dari 0.")]
        public decimal MinimumQualifiedHeadcount { get; set; }

        public bool IsMandatory { get; set; } = true;

        [MaxLength(500, ErrorMessage = "Catatan maksimal 500 karakter.")]
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Permintaan pengajuan dokumen standar kebutuhan tenaga ke workflow T9.
    /// </summary>
    public sealed class SubmitStaffingStandardRequest
    {
        [MaxLength(500, ErrorMessage = "Catatan maksimal 500 karakter.")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "RowVersion wajib disertakan.")]
        public Guid RowVersion { get; set; }
    }

    /// <summary>
    /// Permintaan pembatalan dokumen standar kebutuhan tenaga.
    /// </summary>
    public sealed class CancelStaffingStandardRequest
    {
        [MaxLength(500, ErrorMessage = "Alasan pembatalan maksimal 500 karakter.")]
        public string? Reason { get; set; }

        [Required(ErrorMessage = "RowVersion wajib disertakan.")]
        public Guid RowVersion { get; set; }
    }

    /// <summary>
    /// Item ringkas daftar standar kebutuhan tenaga berhalaman.
    /// </summary>
    public sealed class StaffingStandardListItemResponse
    {
        public Guid Id { get; set; }
        public string StandardNumber { get; set; } = string.Empty;
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitCode { get; set; }
        public string? OrganizationUnitName { get; set; }
        public int VersionNumber { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? EffectiveEndDate { get; set; }
        public StaffingStandardStatus StandardStatus { get; set; }
        public string StandardStatusName => StandardStatus.ToString();
        public int TotalLines { get; set; }
        public int TotalSkillMixRules { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? SubmittedByUserName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid RowVersion { get; set; }
    }

    /// <summary>
    /// Detail lengkap standar kebutuhan tenaga beserta baris jabatan dan skill mix.
    /// </summary>
    public sealed class StaffingStandardResponse
    {
        public Guid Id { get; set; }
        public string StandardNumber { get; set; } = string.Empty;
        public Guid HospitalSiteId { get; set; }
        public string? HospitalSiteName { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitCode { get; set; }
        public string? OrganizationUnitName { get; set; }
        public int VersionNumber { get; set; }
        public DateOnly EffectiveStartDate { get; set; }
        public DateOnly? EffectiveEndDate { get; set; }
        public StaffingStandardStatus StandardStatus { get; set; }
        public string StandardStatusName => StandardStatus.ToString();
        public string? Justification { get; set; }
        public Guid? SupersedesStandardId { get; set; }
        public Guid? WorkflowInstanceId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? SubmittedByUserName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedByUserName { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid RowVersion { get; set; }

        public List<StaffingStandardLineResponse> Lines { get; set; } = new();
        public List<StaffingSkillMixRuleResponse> SkillMixRules { get; set; } = new();
    }

    /// <summary>
    /// Detail satu baris jabatan pada standar kebutuhan tenaga.
    /// </summary>
    public sealed class StaffingStandardLineResponse
    {
        public Guid Id { get; set; }
        public Guid PositionId { get; set; }
        public string? PositionCode { get; set; }
        public string? PositionName { get; set; }
        public Guid? ProfessionId { get; set; }
        public string? ProfessionName { get; set; }
        public StaffingCalculationMethod CalculationMethod { get; set; }
        public string CalculationMethodName => CalculationMethod.ToString();
        public decimal? MinimumHeadcount { get; set; }
        public decimal? RatioStaffQuantity { get; set; }
        public decimal? RatioWorkloadQuantity { get; set; }
        public string? WorkloadUnit { get; set; }
        public int? ShiftsPerDay { get; set; }
        public decimal? StaffPerShift { get; set; }
        public decimal? ServiceHoursPerDay { get; set; }
        public decimal? ReliefFactor { get; set; }
        public StaffingRoundingMethod RoundingMethod { get; set; }
        public string RoundingMethodName => RoundingMethod.ToString();
        public bool CountsOutsourced { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Detail aturan skill mix pada standar kebutuhan tenaga.
    /// </summary>
    public sealed class StaffingSkillMixRuleResponse
    {
        public Guid Id { get; set; }
        public Guid? ShiftId { get; set; }
        public string? ShiftName { get; set; }
        public Guid? ProfessionId { get; set; }
        public string? ProfessionName { get; set; }
        public Guid CompetencyId { get; set; }
        public string? CompetencyCode { get; set; }
        public string? CompetencyName { get; set; }
        public string? MinimumCompetencyLevel { get; set; }
        public decimal MinimumQualifiedHeadcount { get; set; }
        public bool IsMandatory { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Hasil rekomendasi kebutuhan tenaga per unit untuk satu tanggal periode.
    /// </summary>
    public sealed class StaffingRecommendationResponse
    {
        public Guid StaffingStandardId { get; set; }
        public string StandardNumber { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public Guid OrganizationUnitId { get; set; }
        public string? OrganizationUnitName { get; set; }
        public DateOnly PeriodStartDate { get; set; }

        public List<StaffingLineRecommendationResponse> Lines { get; set; } = new();
    }

    /// <summary>
    /// Hasil rekomendasi untuk satu baris jabatan beserta rincian formula.
    /// </summary>
    public sealed class StaffingLineRecommendationResponse
    {
        public Guid PositionId { get; set; }
        public string? PositionCode { get; set; }
        public string? PositionName { get; set; }
        public StaffingCalculationMethod CalculationMethod { get; set; }
        public string CalculationMethodName => CalculationMethod.ToString();
        public decimal? RecommendedQuantity { get; set; }
        public string? Breakdown { get; set; }
        public string? MissingInput { get; set; }
    }

    /// <summary>
    /// Parameter pencarian standar kebutuhan tenaga berhalaman.
    /// </summary>
    public sealed class StaffingStandardPagedQuery
    {
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public StaffingStandardStatus? Status { get; set; }
        public string? Search { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Metadata filter untuk layar standar kebutuhan tenaga.
    /// </summary>
    public sealed class StaffingStandardFilterMetadataResponse
    {
        public List<EnumOptionResponse> Statuses { get; set; } = new();
        public List<EnumOptionResponse> CalculationMethods { get; set; } = new();
        public List<EnumOptionResponse> RoundingMethods { get; set; } = new();
    }

    public sealed class EnumOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
    }
}
