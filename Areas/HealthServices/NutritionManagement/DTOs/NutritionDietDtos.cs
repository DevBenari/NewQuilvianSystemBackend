using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;

// ============================================================ daftar pasien gizi

/// <summary>
/// Satu pasien rawat inap aktif beserta diet yang sedang berlaku.
/// </summary>
/// <remarks>
/// Seluruh identitas pasien, ruang, bed, dan DPJP dibaca dari modul yang memilikinya —
/// tidak ada satu pun yang disalin ke tabel Gizi.
/// </remarks>
public class GzNutritionPatientResponse
{
    public Guid PatientId { get; set; }
    public Guid EncounterId { get; set; }
    public Guid EpisodeId { get; set; }
    public string EpisodeNumber { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;

    public string? RoomName { get; set; }
    public string? BedName { get; set; }
    public string? ServiceUnitName { get; set; }
    public string? DoctorName { get; set; }

    public string EpisodeStatus { get; set; } = string.Empty;
    public DateTime? AdmittedAt { get; set; }

    public Guid? PatientDietId { get; set; }
    public string? DietTypeName { get; set; }
    public string? FoodFormName { get; set; }
    public int? EnergyRequirementKcal { get; set; }
    public string? DietInstruction { get; set; }

    /// <summary>Kosong bila pasien belum punya diet aktif.</summary>
    public GzPatientDietStatus? DietStatus { get; set; }
    public int? DietVersion { get; set; }
}

public class GzNutritionPatientQuery
{
    public string? Search { get; set; }
    public Guid? ServiceUnitId { get; set; }

    /// <summary>Bila benar, hanya pasien yang belum punya diet aktif yang ditampilkan.</summary>
    public bool? WithoutDiet { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

// =================================================================== diet pasien

public class PrescribeGzDietRequest
{
    [Required] public Guid PatientId { get; set; }
    [Required] public Guid EncounterId { get; set; }
    public Guid? NutritionOrderId { get; set; }
    [Required] public Guid DietTypeId { get; set; }
    [Required] public Guid FoodFormId { get; set; }
    [Required] public Guid PrescribedByWorkforceId { get; set; }

    /// <summary>Kapan diet mulai berlaku. Kosong berarti sekarang.</summary>
    public DateTime? EffectiveStartAt { get; set; }

    [Range(1, 10000)] public int? EnergyRequirementKcal { get; set; }
    [MaxLength(1000)] public string? Instruction { get; set; }

    /// <summary>
    /// Wajib bila kunjungan ini sudah punya diet aktif, karena penetapan ini menghentikan
    /// diet tersebut. Alasan perubahan bagian dari rekam asuhan, bukan formalitas.
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
    public Guid? NutritionOrderId { get; set; }
    public Guid PatientId { get; set; }
    public Guid EncounterId { get; set; }
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

public class CreateGzProductionBatchRequest
{
    public DateOnly? ServiceDate { get; set; }
    [Required] public Guid MealScheduleId { get; set; }
    [MaxLength(1000)] public string? Note { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class ChangeGzBatchStatusRequest
{
    [Required] public GzProductionBatchStatus Status { get; set; }
    [MaxLength(1000)] public string? Reason { get; set; }
    [Required] public int ExpectedVersion { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class GzProductionBatchSummaryResponse
{
    public Guid Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateOnly ServiceDate { get; set; }
    public Guid MealScheduleId { get; set; }
    public string MealScheduleName { get; set; } = string.Empty;
    public GzProductionBatchStatus Status { get; set; }
    public int TotalPortion { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int Version { get; set; }

    /// <summary>Jumlah porsi yang dietnya sudah berubah sejak batch dibuat.</summary>
    public int DietChangedCount { get; set; }
}

public class GzProductionBatchDetailResponse : GzProductionBatchSummaryResponse
{
    public string? Note { get; set; }
    public string? CancelReason { get; set; }
    public List<GzProductionPortionResponse> Portions { get; set; } = [];
    public List<GzProductionGroupResponse> Groups { get; set; } = [];
}

/// <summary>Rekap porsi per jenis diet dan bentuk makanan; yang dipakai dapur memasak.</summary>
public class GzProductionGroupResponse
{
    public string DietTypeName { get; set; } = string.Empty;
    public string FoodFormName { get; set; } = string.Empty;
    public int Portion { get; set; }
}

public class GzProductionPortionResponse
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public Guid EncounterId { get; set; }
    public Guid PatientDietId { get; set; }

    public string PatientName { get; set; } = string.Empty;
    public string? MedicalRecordNumber { get; set; }
    public string? RoomName { get; set; }
    public string? BedName { get; set; }
    public string? DoctorName { get; set; }

    public string DietTypeName { get; set; } = string.Empty;
    public string FoodFormName { get; set; } = string.Empty;
    public int? EnergyRequirementKcal { get; set; }
    public string? Instruction { get; set; }
    public int Portion { get; set; }

    /// <summary>
    /// Benar bila diet pasien sudah berubah setelah batch ini dibuat. Snapshot tidak
    /// diubah; penanda ini yang memberi tahu petugas bahwa perlu penyesuaian.
    /// </summary>
    public bool IsDietChangedAfterProduction { get; set; }
    public string? CurrentDietTypeName { get; set; }

    public Guid? DeliveryId { get; set; }
    public GzMealDeliveryStatus? DeliveryStatus { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public int? LeftoverPercent { get; set; }
    public string? DeliveryNote { get; set; }
}

// =================================================================== distribusi

public class RecordGzMealDeliveryRequest
{
    [Required] public Guid ProductionBatchDetailId { get; set; }
    [Required] public GzMealDeliveryStatus Status { get; set; } = GzMealDeliveryStatus.Delivered;
    [Required] public Guid DeliveredByWorkforceId { get; set; }
    [Range(0, 100)] public int? LeftoverPercent { get; set; }
    [MaxLength(1000)] public string? Note { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

// ======================================================================= master

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
