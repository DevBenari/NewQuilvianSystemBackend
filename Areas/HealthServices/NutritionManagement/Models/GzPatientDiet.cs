using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
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

    /// <summary>
    /// Order konsultasi gizi, bila diet ini lahir dari rujukan ahli gizi. Boleh kosong,
    /// karena sebagian besar pasien rawat inap memerlukan diet tanpa pernah dirujuk.
    /// </summary>
    public Guid? NutritionOrderId { get; set; }

    [Required] public Guid PatientId { get; set; }

    /// <summary>
    /// Kunjungan rawat inap tempat diet ini berlaku.
    /// </summary>
    /// <remarks>
    /// Diet melekat pada kunjungan, bukan pada pasien. Pasien yang pernah dirawat dua kali
    /// punya dua riwayat diet yang terpisah; tanpa kolom ini keduanya tercampur dan
    /// pertanyaan "diet apa saat perawatan bulan lalu" tidak dapat dijawab.
    /// </remarks>
    [Required] public Guid EncounterId { get; set; }

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
    public TrxPatientEncounter? Encounter { get; set; }
    public MstPatient? Patient { get; set; }
    public GzDietType? DietType { get; set; }
    public GzFoodForm? FoodForm { get; set; }
    public MstWorkforceProfile? PrescribedByWorkforce { get; set; }
}

/// <summary>
/// Penyerahan makanan kepada satu pasien, berasal dari satu porsi pada batch produksi.
/// </summary>
/// <remarks>
/// Menggantung pada <c>GzProductionBatchDetail</c>, bukan langsung pada diet, sehingga
/// jejaknya utuh: produksi menghasilkan porsi, porsi diserahkan kepada pasien, dan porsi itu
/// tahu diet mana serta kunjungan mana yang melatarinya. Bila digantungkan pada diet, makanan
/// yang sudah terlanjur diproduksi kehilangan kaitannya begitu diet pasien berubah.
/// </remarks>
[Table("GzMealDelivery", Schema = "public")]
public class GzMealDelivery : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid ProductionBatchDetailId { get; set; }

    public GzMealDeliveryStatus Status { get; set; } = GzMealDeliveryStatus.Delivered;

    public DateTime? DeliveredAt { get; set; }
    [Required] public Guid DeliveredByWorkforceId { get; set; }

    /// <summary>
    /// Perkiraan sisa makanan dalam persen. Diisi bila petugas sempat mengamatinya;
    /// dibiarkan kosong lebih jujur daripada diisi angka tebakan.
    /// </summary>
    public int? LeftoverPercent { get; set; }

    [MaxLength(1000)] public string? Note { get; set; }

    public GzProductionBatchDetail? ProductionBatchDetail { get; set; }
    public MstWorkforceProfile? DeliveredByWorkforce { get; set; }
}
