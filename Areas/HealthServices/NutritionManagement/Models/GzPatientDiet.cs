using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;

/// <summary>
/// Diet yang ditetapkan untuk seorang pasien pada satu rentang waktu.
/// </summary>
/// <remarks>
/// <para>
/// Perubahan diet TIDAK menimpa baris yang sedang berlaku. Diet lama dihentikan dan diet
/// baru dibuat sebagai baris tersendiri. Dengan begitu pertanyaan "diet apa yang berlaku
/// tanggal sekian" selalu terjawab, dan itu penting ketika terjadi keluhan.
/// </para>
/// <para>
/// Satu pasien hanya boleh punya satu diet aktif pada satu waktu; ditegakkan indeks unik
/// tersaring di basis data supaya dapur tidak pernah menerima dua perintah berbeda untuk
/// pasien yang sama.
/// </para>
/// </remarks>
[Table("GzPatientDiet", Schema = "public")]
public class GzPatientDiet : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid NutritionOrderId { get; set; }
    [Required] public Guid PatientId { get; set; }

    [Required] public Guid DietTypeId { get; set; }
    [Required] public Guid FoodFormId { get; set; }

    /// <summary>Kebutuhan energi yang menyertai diet ini, diketik ahli gizi (`GIZ-DEC-012`).</summary>
    public int? EnergyRequirementKcal { get; set; }

    [MaxLength(1000)] public string? Instruction { get; set; }

    public GzPatientDietStatus Status { get; set; } = GzPatientDietStatus.Active;

    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }

    /// <summary>Alasan diet dihentikan atau diganti. Wajib saat status berubah.</summary>
    [MaxLength(1000)] public string? ChangeReason { get; set; }

    [Required] public Guid PrescribedByWorkforceId { get; set; }

    public int Version { get; set; }

    public GzNutritionOrder? NutritionOrder { get; set; }
    public MstPatient? Patient { get; set; }
    public GzDietType? DietType { get; set; }
    public GzFoodForm? FoodForm { get; set; }
    public MstWorkforceProfile? PrescribedByWorkforce { get; set; }
    public ICollection<GzMealDelivery> Deliveries { get; set; } = [];
}

/// <summary>
/// Penyerahan makanan kepada satu pasien pada satu tanggal dan jadwal makan.
/// </summary>
/// <remarks>
/// Baris ini dibuat saat makanan diserahkan, bukan direncanakan di muka. Rencana produksi
/// dihitung dari diet yang aktif, sehingga tidak perlu disimpan lebih dulu dan tidak dapat
/// menjadi basi.
/// </remarks>
[Table("GzMealDelivery", Schema = "public")]
public class GzMealDelivery : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid PatientDietId { get; set; }
    [Required] public Guid MealScheduleId { get; set; }

    /// <summary>Tanggal pelayanan; dipisah dari waktu agar rekap harian sederhana.</summary>
    public DateOnly ServiceDate { get; set; }

    public GzMealDeliveryStatus Status { get; set; } = GzMealDeliveryStatus.Delivered;

    public DateTime? DeliveredAt { get; set; }
    [Required] public Guid DeliveredByWorkforceId { get; set; }

    /// <summary>
    /// Perkiraan sisa makanan dalam persen. Diisi bila petugas sempat mengamatinya;
    /// dibiarkan kosong lebih jujur daripada diisi angka tebakan.
    /// </summary>
    public int? LeftoverPercent { get; set; }

    [MaxLength(1000)] public string? Note { get; set; }

    public GzPatientDiet? PatientDiet { get; set; }
    public GzMealSchedule? MealSchedule { get; set; }
    public MstWorkforceProfile? DeliveredByWorkforce { get; set; }
}
