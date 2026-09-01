using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;

/// <summary>
/// Permintaan konsultasi gizi untuk satu episode rawat inap.
/// </summary>
/// <remarks>
/// Memakai entity sendiri dan bukan <c>TrxDoctorConsultation</c> sesuai `GIZ-DEC-001`:
/// order gizi membawa ahli gizi sebagai penerima, anjuran diet, dan lifecycle asuhan yang
/// berjalan berhari-hari — sementara konsultasi dokter membawa tanda vital dan poli yang
/// tidak relevan bagi gizi.
/// </remarks>
[Table("GzNutritionOrder", Schema = "public")]
public class GzNutritionOrder : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required] public Guid PatientId { get; set; }
    [Required] public Guid EncounterId { get; set; }
    [Required] public Guid RequesterDoctorId { get; set; }

    /// <summary>Ahli gizi yang menangani. Boleh kosong saat order baru dibuat.</summary>
    public Guid? AssignedWorkforceId { get; set; }

    public GzOrderStatus Status { get; set; } = GzOrderStatus.Requested;
    public GzOrderPriority Priority { get; set; } = GzOrderPriority.Routine;

    [Required, MaxLength(1000)]
    public string ReasonForReferral { get; set; } = string.Empty;

    /// <summary>
    /// Hasil skrining gizi disalin saat order dibuat, bukan dibaca ulang.
    /// </summary>
    /// <remarks>
    /// Skrining adalah alasan order ini lahir. Bila dibaca ulang setiap kali ditampilkan,
    /// angkanya ikut berubah ketika perawat memperbarui asesmen, dan alasan order tidak lagi
    /// cocok dengan apa yang dilihat dokter saat memesannya. Penyalinan nilai pada saat
    /// transaksi seperti ini adalah pengecualian yang sah pada aturan data bersama.
    /// </remarks>
    public NutritionRiskStatus? ScreeningRiskStatus { get; set; }
    public int? ScreeningScore { get; set; }

    public DateTime RequestedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    [MaxLength(2000)]
    public string? ClosingNote { get; set; }

    /// <summary>Token konkurensi; naik setiap perubahan.</summary>
    public int Version { get; set; }

    public MstPatient? Patient { get; set; }
    public TrxPatientEncounter? Encounter { get; set; }
    public MstDoctor? RequesterDoctor { get; set; }
    public MstWorkforceProfile? AssignedWorkforce { get; set; }
    public ICollection<GzNutritionCareRecord> CareRecords { get; set; } = [];
    public ICollection<GzNutritionOrderHistory> Histories { get; set; } = [];
}
