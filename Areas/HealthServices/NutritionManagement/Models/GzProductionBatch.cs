using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;

/// <summary>
/// Satu angkatan produksi makanan untuk satu tanggal dan satu jadwal makan.
/// </summary>
/// <remarks>
/// <para>
/// Batch menyimpan keadaan diet pasien PADA SAAT batch dibuat, bukan menghitungnya ulang
/// setiap dibaca. Alasannya sederhana: begitu batch dibuat, dapur sudah memasak. Bila diet
/// pasien berubah setengah jam kemudian, yang benar bukan mengubah angka produksi diam-diam
/// — melainkan menyimpan apa yang benar-benar diproduksi, lalu menandai selisihnya agar
/// dapat ditindaklanjuti untuk penyajian berikutnya.
/// </para>
/// <para>
/// Satu tanggal dan satu jadwal makan hanya boleh punya satu batch yang tidak dibatalkan,
/// ditegakkan indeks unik tersaring di basis data.
/// </para>
/// </remarks>
[Table("GzProductionBatch", Schema = "public")]
public class GzProductionBatch : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string BatchNumber { get; set; } = string.Empty;

    public DateOnly ServiceDate { get; set; }
    [Required] public Guid MealScheduleId { get; set; }

    public GzProductionBatchStatus Status { get; set; } = GzProductionBatchStatus.Draft;

    /// <summary>Jumlah porsi seluruh detail; disimpan agar daftar batch tidak perlu menghitung ulang.</summary>
    public int TotalPortion { get; set; }

    public DateTime? ConfirmedAt { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    [MaxLength(1000)] public string? CancelReason { get; set; }
    [MaxLength(1000)] public string? Note { get; set; }

    public int Version { get; set; }

    public GzMealSchedule? MealSchedule { get; set; }
    public ICollection<GzProductionBatchDetail> Details { get; set; } = [];
}

/// <summary>
/// Satu porsi pada satu batch, beserta salinan keadaan pasien saat batch dibuat.
/// </summary>
/// <remarks>
/// Seluruh kolom bertanda <c>Snapshot</c> sengaja disalin, bukan dibaca lewat relasi.
/// Nama ruang, bed, dan diet dapat berubah setelah makanan diproduksi; catatan produksi
/// harus tetap menunjukkan apa yang berlaku saat itu, bukan apa yang berlaku sekarang.
/// </remarks>
[Table("GzProductionBatchDetail", Schema = "public")]
public class GzProductionBatchDetail : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid ProductionBatchId { get; set; }
    [Required] public Guid PatientId { get; set; }
    [Required] public Guid EncounterId { get; set; }

    /// <summary>Diet yang menjadi dasar porsi ini saat batch dibuat.</summary>
    [Required] public Guid PatientDietId { get; set; }

    [Required, MaxLength(200)] public string PatientNameSnapshot { get; set; } = string.Empty;
    [MaxLength(50)] public string? MedicalRecordNumberSnapshot { get; set; }
    [MaxLength(200)] public string? RoomNameSnapshot { get; set; }
    [MaxLength(200)] public string? BedNameSnapshot { get; set; }
    [MaxLength(200)] public string? DoctorNameSnapshot { get; set; }

    [Required, MaxLength(200)] public string DietTypeNameSnapshot { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string FoodFormNameSnapshot { get; set; } = string.Empty;
    public int? EnergyRequirementKcalSnapshot { get; set; }
    [MaxLength(1000)] public string? InstructionSnapshot { get; set; }

    public int Portion { get; set; } = 1;

    public GzProductionBatch? ProductionBatch { get; set; }
    public MstPatient? Patient { get; set; }
    public TrxPatientEncounter? Encounter { get; set; }
    public GzPatientDiet? PatientDiet { get; set; }
    public ICollection<GzMealDelivery> Deliveries { get; set; } = [];
}
