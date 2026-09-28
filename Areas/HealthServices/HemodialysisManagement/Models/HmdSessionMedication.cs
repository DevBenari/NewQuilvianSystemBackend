using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Catatan klinis pemberian obat selama sesi. Stok dimiliki Farmasi; faktanya diteruskan ke
    /// <c>PhmDrugUsage</c> setelah catatan ini tersimpan.
    /// </summary>
    /// <remarks>
    /// Bila penerusan ke Farmasi gagal, baris ini <b>tetap tersimpan</b> — obatnya memang sudah
    /// masuk ke tubuh pasien. Status penerusannya tinggal di <see cref="HandoffStatus"/>.
    /// </remarks>
    [Table("HmdSessionMedication", Schema = "public")]
    public class HmdSessionMedication : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SessionId { get; set; }

        public HmdSession? Session { get; set; }

        [Required]
        public Guid DrugId { get; set; }

        public MstDrug? Drug { get; set; }

        public decimal Dose { get; set; }

        [Required]
        [MaxLength(50)]
        public string DoseUnit { get; set; } = string.Empty;

        public HmdMedicationRoute Route { get; set; }

        public Guid? InstructedByDoctorId { get; set; }

        public MstDoctor? InstructedByDoctor { get; set; }

        [Required]
        public Guid AdministeredByUserId { get; set; }

        public DateTime AdministeredAt { get; set; } = DateTime.UtcNow;

        /// <summary>Rujukan ke pemakaian obat milik Farmasi, terisi setelah penerusan berhasil.</summary>
        public Guid? DrugUsageId { get; set; }

        public HmdPharmacyHandoffStatus HandoffStatus { get; set; } = HmdPharmacyHandoffStatus.Pending;

        /// <summary>Lokasi stok Farmasi sumber obat, dibutuhkan penerusan ke <c>PhmDrugUsage</c>.</summary>
        public Guid? PharmacyStorageLocationId { get; set; }

        /// <summary>Satuan stok Farmasi untuk jumlah yang diteruskan.</summary>
        public Guid? PharmacyMeasurementId { get; set; }

        /// <summary>Jumlah dalam satuan stok Farmasi — tidak selalu sama dengan dosis klinis.</summary>
        public decimal? PharmacyQuantity { get; set; }

        public DateTime? HandoffAttemptedAt { get; set; }

        [MaxLength(1000)]
        public string? HandoffError { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(500)]
        public string? Note { get; set; }
    }
}
