using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;

// ============================================================ master diagnosis

public class GziDiagnosisDomainResponse
{
    public Guid Id { get; set; }
    public string DomainCode { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int DiagnosisCount { get; set; }
}

public class SaveGzDiagnosisDomainRequest
{
    [Required, MaxLength(20)] public string DomainCode { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string DomainName { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class GziNutritionDiagnosisResponse
{
    public Guid Id { get; set; }
    public Guid DiagnosisDomainId { get; set; }
    public string DomainCode { get; set; } = string.Empty;
    public string DomainName { get; set; } = string.Empty;
    public Guid? ParentDiagnosisId { get; set; }
    public string? ParentDiagnosisCode { get; set; }
    public string DiagnosisCode { get; set; } = string.Empty;
    public string DiagnosisName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Standard { get; set; } = string.Empty;
    public string? StandardVersion { get; set; }
    public bool IsSelectable { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class SaveGzNutritionDiagnosisRequest
{
    [Required] public Guid DiagnosisDomainId { get; set; }
    public Guid? ParentDiagnosisId { get; set; }
    [Required, MaxLength(30)] public string DiagnosisCode { get; set; } = string.Empty;
    [Required, MaxLength(300)] public string DiagnosisName { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }

    /// <summary>Asal baris. Dibiarkan `IDNT` kecuali admin menyatakan lain.</summary>
    [MaxLength(50)] public string Standard { get; set; } = "IDNT";
    [MaxLength(30)] public string? StandardVersion { get; set; }

    public bool IsSelectable { get; set; } = true;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class GziDiagnosisQuery
{
    public Guid? DiagnosisDomainId { get; set; }
    public string? Search { get; set; }
    public bool OnlyActive { get; set; } = true;

    /// <summary>Menyaring baris kelompok yang tidak boleh ditegakkan sebagai diagnosis.</summary>
    public bool OnlySelectable { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

// =========================================================== master parameter

public class GziNutritionParameterResponse
{
    public Guid Id { get; set; }
    public string ParameterCode { get; set; } = string.Empty;
    public string ParameterName { get; set; } = string.Empty;
    public string UnitCode { get; set; } = string.Empty;
    public int ValueScale { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public class SaveGzNutritionParameterRequest
{
    [Required, MaxLength(30)] public string ParameterCode { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string ParameterName { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string UnitCode { get; set; } = string.Empty;
    [Range(0, 6)] public int ValueScale { get; set; }

    /// <summary>
    /// Batas wajar. Dibiarkan kosong berarti belum ditetapkan siapa pun — bukan berarti tak
    /// terbatas secara klinis.
    /// </summary>
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

// ============================================================= registry rumus

public class GziNutritionFormulaResponse
{
    public Guid Id { get; set; }
    public string FormulaCode { get; set; } = string.Empty;
    public string FormulaName { get; set; } = string.Empty;
    public string FormulaVersion { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SourceReference { get; set; }
    public string ImplementationKey { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    /// <summary>
    /// Benar bila ada kelas perhitungan terdaftar untuk <c>ImplementationKey</c> ini. Salah
    /// berarti barisnya ada tetapi tidak dapat menghitung apa pun.
    /// </summary>
    public bool IsImplemented { get; set; }
}

public class SaveGzNutritionFormulaRequest
{
    [Required, MaxLength(50)] public string FormulaCode { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string FormulaName { get; set; } = string.Empty;
    [Required, MaxLength(30)] public string FormulaVersion { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }

    /// <summary>Sumber resmi rumus. Diisi admin; tidak diambil sistem dari mana pun.</summary>
    [MaxLength(500)] public string? SourceReference { get; set; }

    [Required, MaxLength(100)] public string ImplementationKey { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

// ====================================================== kebutuhan nutrisi

public class GziRequirementItemRequest
{
    [Required] public Guid NutritionParameterId { get; set; }

    /// <summary>
    /// Nilai hasil rumus, bila ada. Dibiarkan kosong ketika kebutuhan ditetapkan tanpa
    /// kalkulasi — dan itulah keadaan V1 selama `GIZ-OQ-007` masih ditunda.
    /// </summary>
    public decimal? CalculatedValue { get; set; }

    [Required] public decimal FinalValue { get; set; }

    /// <summary>Wajib bila <c>FinalValue</c> berbeda dari <c>CalculatedValue</c> (`GIZ016`).</summary>
    [MaxLength(1000)] public string? AdjustmentReason { get; set; }
}

public class SaveGzRequirementRequest
{
    [Required] public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>Kunjungan yang melahirkan revisi ini. Boleh kosong.</summary>
    public Guid? CareRecordId { get; set; }

    [Required] public Guid DeterminedByWorkforceId { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public Guid? CalculationFormulaId { get; set; }

    /// <summary>Masukan rumus yang disalin apa adanya, supaya nilai lama tetap dapat dijelaskan.</summary>
    public GziCalculationInputDto? CalculationInput { get; set; }

    /// <summary>Wajib mulai revisi kedua (`GIZ017`).</summary>
    [MaxLength(1000)] public string? ChangeReason { get; set; }

    [Required, MinLength(1)] public List<GziRequirementItemRequest> Items { get; set; } = new();
}

public class GziCalculationInputDto
{
    public decimal? WeightKg { get; set; }
    public decimal? HeightCm { get; set; }
    public int? AgeYears { get; set; }
    public string? Gender { get; set; }
    public decimal? ActivityFactor { get; set; }
    public decimal? StressFactor { get; set; }
}

public class GziRequirementItemResponse
{
    public Guid Id { get; set; }
    public Guid NutritionParameterId { get; set; }
    public string ParameterCode { get; set; } = string.Empty;
    public string ParameterName { get; set; } = string.Empty;
    public string UnitCode { get; set; } = string.Empty;
    public int ValueScale { get; set; }
    public decimal? CalculatedValue { get; set; }
    public decimal FinalValue { get; set; }
    public string? AdjustmentReason { get; set; }
    public Guid? AdjustedByWorkforceId { get; set; }
    public DateTime? AdjustedAt { get; set; }
}

public class GziRequirementResponse
{
    public Guid Id { get; set; }
    public Guid NutritionOrderId { get; set; }
    public Guid? CareRecordId { get; set; }
    public int RevisionNumber { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public Guid? CalculationFormulaId { get; set; }
    public string? CalculationFormulaCode { get; set; }
    public string? CalculationFormulaName { get; set; }
    public GziCalculationInputDto? CalculationInput { get; set; }
    public DateTime? CalculationPerformedAt { get; set; }
    public Guid DeterminedByWorkforceId { get; set; }
    public string DeterminedByName { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
    public int Version { get; set; }
    public DateTime CreateDateTime { get; set; }
    public List<GziRequirementItemResponse> Items { get; set; } = new();
}

public class GziCalculationPreviewRequest
{
    public Guid? CalculationFormulaId { get; set; }
    public GziCalculationInputDto Input { get; set; } = new();
}

public class GziCalculationPreviewResponse
{
    public Guid? CalculationFormulaId { get; set; }
    public string? CalculationFormulaCode { get; set; }

    /// <summary>
    /// Benar bila rumus benar-benar dijalankan. Salah berarti nilai kalkulasi kosong dan ahli
    /// gizi mengisi nilai final sendiri — keadaan baku V1.
    /// </summary>
    public bool Calculated { get; set; }

    public string Message { get; set; } = string.Empty;
    public List<GziCalculatedValueResponse> Values { get; set; } = new();
}

public class GziCalculatedValueResponse
{
    public Guid NutritionParameterId { get; set; }
    public string ParameterCode { get; set; } = string.Empty;
    public string ParameterName { get; set; } = string.Empty;
    public string UnitCode { get; set; } = string.Empty;
    public decimal? CalculatedValue { get; set; }
}

// =============================================== diagnosis pada satu kunjungan

public class SaveGzCareRecordDiagnosesRequest
{
    [Required] public string IdempotencyKey { get; set; } = string.Empty;

    /// <summary>
    /// Daftar pengganti, bukan tambahan: baris yang tidak disebut akan ditandai terhapus.
    /// Daftar kosong berarti seluruh diagnosis kunjungan ini dicabut.
    /// </summary>
    public List<NutritionCareRecordDiagnosisRequest> Diagnoses { get; set; } = new();
}
