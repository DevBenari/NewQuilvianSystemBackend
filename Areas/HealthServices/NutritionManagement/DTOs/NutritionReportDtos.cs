using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;

/// <summary>
/// Penyaring bersama ketiga laporan Gizi.
/// </summary>
/// <remarks>
/// Rentang tanggal dibiarkan boleh kosong supaya laporan tetap dapat dibuka tanpa penyaring,
/// tetapi layar mengisinya dengan 30 hari terakhir — mengikuti laporan Operasi, yang sengaja
/// tidak pernah membuka laporan tanpa batas waktu.
/// </remarks>
public class GziReportQuery
{
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 200)] public int PageSize { get; set; } = 20;

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public Guid? PatientId { get; set; }

    /// <summary>Ahli gizi pelaksana. Menunjuk `MstWorkforceProfile`.</summary>
    public Guid? WorkforceId { get; set; }

    /// <summary>Menunjuk `GziDietType`. Hanya berlaku bagi laporan diet.</summary>
    public Guid? DietTypeId { get; set; }

    /// <summary>Status order gizi. Hanya berlaku bagi laporan pelayanan.</summary>
    public GziOrderStatus? OrderStatus { get; set; }

    /// <summary>Status diet pasien. Hanya berlaku bagi laporan diet.</summary>
    public GziPatientDietStatus? DietStatus { get; set; }

    [MaxLength(100)] public string? Search { get; set; }
}

// ================================================= A. Laporan Pelayanan Gizi

public class GziServiceReportRow
{
    public Guid CareRecordId { get; set; }
    public Guid NutritionOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;

    public int VisitSequence { get; set; }
    public DateTime VisitAt { get; set; }
    public GziCareRecordType RecordType { get; set; }

    public string RecordedByName { get; set; } = string.Empty;

    /// <summary>Diagnosis gizi kunjungan ini, primer lebih dulu, dirangkai satu baris.</summary>
    public string DiagnosisSummary { get; set; } = string.Empty;

    /// <summary>Diet yang ditetapkan pada kunjungan ini, bila ada.</summary>
    public string DietTypeName { get; set; } = string.Empty;

    public GziOrderStatus OrderStatus { get; set; }
    public int? IntakePercent { get; set; }
}

// ==================================================== B. Laporan Diet Pasien

public class GziDietReportRow
{
    public Guid PatientDietId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;

    public string EncounterNumber { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;

    public string DietTypeName { get; set; } = string.Empty;
    public string FoodFormName { get; set; } = string.Empty;

    /// <summary>
    /// Salinan kebutuhan energi yang menyertai diet ini. Sumber kebenarannya tetap revisi
    /// kebutuhan yang ditunjuk <c>NutritionRequirementId</c>.
    /// </summary>
    public int? EnergyRequirementKcal { get; set; }

    public int? RequirementRevisionNumber { get; set; }

    public GziPatientDietStatus Status { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    public string PrescribedByName { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
}

// =============================================== C. Laporan Kebutuhan Nutrisi

public class GziRequirementReportRow
{
    public Guid RequirementId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string EncounterNumber { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;

    public int RevisionNumber { get; set; }
    public bool IsCurrent { get; set; }
    public DateTime EffectiveFrom { get; set; }

    /// <summary>
    /// Nilai final per parameter V1. Ditulis sebagai kolom tetap pada baris laporan karena
    /// laporan adalah tabel; parameter di luar kelima ini tetap tersimpan utuh pada
    /// <c>GziNutritionRequirementItem</c> dan terbaca pada layar kebutuhan.
    /// </summary>
    public decimal? EnergyKcal { get; set; }
    public decimal? ProteinGram { get; set; }
    public decimal? FatGram { get; set; }
    public decimal? CarbohydrateGram { get; set; }
    public decimal? FluidMl { get; set; }

    public string DeterminedByName { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
}

// ============================================================== D. Ringkasan

/// <summary>
/// Angka pembuka laporan, dihitung atas rentang dan penyaring yang sama dengan tabelnya.
/// </summary>
public class GziReportSummary
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public int TotalOrders { get; set; }
    public int OpenOrders { get; set; }
    public int ClosedOrders { get; set; }

    public int TotalCareRecords { get; set; }
    public int TotalPatients { get; set; }

    public int ActiveDiets { get; set; }
    public int RequirementRevisions { get; set; }

    /// <summary>Rata-rata kebutuhan energi pada revisi yang berlaku, kkal/hari.</summary>
    public decimal? AverageEnergyKcal { get; set; }
}
