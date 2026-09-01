using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;

// ===================================================================== diet pasien

public class PrescribeGzDietRequest
{
    [Required] public Guid NutritionOrderId { get; set; }
    [Required] public Guid DietTypeId { get; set; }
    [Required] public Guid FoodFormId { get; set; }
    [Required] public Guid PrescribedByWorkforceId { get; set; }
    [Range(1, 10000)] public int? EnergyRequirementKcal { get; set; }
    [MaxLength(1000)] public string? Instruction { get; set; }

    /// <summary>
    /// Wajib diisi bila pasien sudah punya diet aktif, karena penetapan ini menghentikan
    /// diet tersebut. Alasan perubahan adalah bagian dari rekam asuhan, bukan formalitas.
    /// </summary>
    [MaxLength(1000)] public string? ChangeReason { get; set; }

    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class StopGzDietRequest
{
    [Required, MaxLength(1000)] public string Reason { get; set; } = string.Empty;
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class GzPatientDietResponse
{
    public Guid Id { get; set; }
    public Guid NutritionOrderId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;

    public Guid DietTypeId { get; set; }
    public string DietTypeName { get; set; } = string.Empty;
    public Guid FoodFormId { get; set; }
    public string FoodFormName { get; set; } = string.Empty;

    public int? EnergyRequirementKcal { get; set; }
    public string? Instruction { get; set; }
    public GzPatientDietStatus Status { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string? ChangeReason { get; set; }
    public string PrescribedByName { get; set; } = string.Empty;
    public int Version { get; set; }
}

// ===================================================================== produksi

/// <summary>
/// Rekap kebutuhan produksi untuk satu jadwal makan pada satu tanggal.
/// </summary>
/// <remarks>
/// Seluruhnya HASIL HITUNGAN atas diet yang sedang aktif, bukan data tersimpan. Dengan
/// begitu dapur selalu memasak berdasarkan diet yang benar-benar berlaku saat itu.
/// </remarks>
public class GzProductionSummaryResponse
{
    public DateOnly ServiceDate { get; set; }
    public Guid MealScheduleId { get; set; }
    public string MealScheduleName { get; set; } = string.Empty;
    public TimeOnly ServingTime { get; set; }
    public int TotalPortion { get; set; }
    public List<GzProductionBreakdownResponse> Breakdown { get; set; } = [];
}

public class GzProductionBreakdownResponse
{
    public Guid DietTypeId { get; set; }
    public string DietTypeName { get; set; } = string.Empty;
    public Guid FoodFormId { get; set; }
    public string FoodFormName { get; set; } = string.Empty;
    public bool IsSpecialDiet { get; set; }
    public int Portion { get; set; }
}

// ===================================================================== distribusi

public class RecordGzMealDeliveryRequest
{
    [Required] public Guid PatientDietId { get; set; }
    [Required] public Guid MealScheduleId { get; set; }
    public DateOnly? ServiceDate { get; set; }
    [Required] public GzMealDeliveryStatus Status { get; set; } = GzMealDeliveryStatus.Delivered;
    [Required] public Guid DeliveredByWorkforceId { get; set; }
    [Range(0, 100)] public int? LeftoverPercent { get; set; }
    [MaxLength(1000)] public string? Note { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

/// <summary>Satu baris daftar distribusi: pasien, dietnya, dan apakah sudah diserahkan.</summary>
public class GzMealDistributionRowResponse
{
    public Guid PatientDietId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string DietTypeName { get; set; } = string.Empty;
    public string FoodFormName { get; set; } = string.Empty;
    public int? EnergyRequirementKcal { get; set; }
    public string? Instruction { get; set; }

    public Guid? DeliveryId { get; set; }
    public GzMealDeliveryStatus? DeliveryStatus { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public int? LeftoverPercent { get; set; }
    public string? DeliveryNote { get; set; }
}

// ===================================================================== master

public class GzMasterOptionResponse
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSpecialDiet { get; set; }
    public TimeOnly? ServingTime { get; set; }
    public bool IsActive { get; set; }
}

public class SaveGzMasterRequest
{
    [Required, MaxLength(50)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string Name { get; set; } = string.Empty;
    [MaxLength(1000)] public string? Description { get; set; }
    public bool IsSpecialDiet { get; set; }
    public TimeOnly? ServingTime { get; set; }
    public bool IsMainMeal { get; set; } = true;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
