using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Satu komponen milik order: salinan komponen paket, atau komponen di luar paket
    /// (<see cref="PackageComponentId"/> kosong, <see cref="IsPackageCovered"/> = false, ditagih biasa —
    /// <c>MCU-DEC-036</c>).
    /// </summary>
    /// <remarks>
    /// Nilai pengembalian dan jasa medis disalin saat komponen lahir, sehingga perubahan paket
    /// kemudian tidak mengubah order yang sudah berjalan. Satu prosedur atau satu jenis pemeriksaan
    /// hanya sekali per order selama komponennya belum dibatalkan — dijaga unique index.
    /// </remarks>
    [Table("McuOrderComponent", Schema = "public")]
    public class McuOrderComponent : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid McuOrderId { get; set; }

        public McuOrder? McuOrder { get; set; }

        /// <summary>Kosong = komponen di luar paket.</summary>
        public Guid? PackageComponentId { get; set; }

        public McuPackageComponent? PackageComponent { get; set; }

        public McuComponentType ComponentType { get; set; }

        public Guid? ProcedureId { get; set; }

        public MstProcedure? Procedure { get; set; }

        public Guid? RadModalityId { get; set; }

        public MstRadModality? RadModality { get; set; }

        public Guid? ExaminationTypeId { get; set; }

        public McuExaminationType? ExaminationType { get; set; }

        [Required]
        [MaxLength(200)]
        public string ComponentNameSnapshot { get; set; } = string.Empty;

        public bool IsPackageCovered { get; set; } = true;

        public decimal RefundAmountSnapshot { get; set; }

        public decimal MedicalFeeAmountSnapshot { get; set; }

        public McuOrderComponentStatus ComponentStatus { get; set; } = McuOrderComponentStatus.Pending;

        /// <summary>Awalnya sama dengan tanggal pemeriksaan order; berubah bila dijadwal ulang.</summary>
        public DateOnly ScheduledDate { get; set; }

        /// <summary>Boleh sama untuk beberapa komponen satu disiplin Lab.</summary>
        public Guid? LabOrderId { get; set; }

        public LabOrder? LabOrder { get; set; }

        public Guid? RadOrderId { get; set; }

        public RadOrder? RadOrder { get; set; }

        /// <summary>Dokter pelaksana/pembaca, diisi saat selesai bila komponen bernilai jasa.</summary>
        public Guid? PerformingDoctorId { get; set; }

        public MstDoctor? PerformingDoctor { get; set; }

        public DateTime? CompletedAt { get; set; }

        /// <summary>Contoh <i>Kehamilan</i>.</summary>
        [MaxLength(500)]
        public string? CancelReason { get; set; }

        /// <summary>Dokter MCU yang membatalkan.</summary>
        public Guid? CancelledByDoctorId { get; set; }

        public MstDoctor? CancelledByDoctor { get; set; }

        public Guid? RefundCaseId { get; set; }

        public BilRefundCase? RefundCase { get; set; }

        /// <summary>Waktu komponen tersedia sebagai sumber jasa medis.</summary>
        public DateTime? MedicalFeeReleasedAt { get; set; }

        /// <summary>Dokter yang menambahkan komponen di luar paket.</summary>
        public Guid? AddedByDoctorId { get; set; }

        public MstDoctor? AddedByDoctor { get; set; }

        public McuExaminationResult? ExaminationResult { get; set; }
    }
}
