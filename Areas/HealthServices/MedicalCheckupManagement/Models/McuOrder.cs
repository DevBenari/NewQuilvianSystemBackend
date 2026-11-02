using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Satu order paket MCU untuk satu peserta pada satu tanggal — akar seluruh perjalanan peserta,
    /// dari booking (<i>Terjadwal</i>) sampai hasil diserahkan.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Booking sama dengan pendaftaran</b> (<c>MCU-DEC-003</c>): order lahir berstatus
    /// <i>Terjadwal</i> tanpa kunjungan dan tanpa tagihan. Check-in mengisi <see cref="EncounterId"/>,
    /// <see cref="McuDoctorId"/>, dan <see cref="PackageInvoiceItemId"/> pada order yang <b>sama</b>.
    /// </para>
    /// <para>
    /// Sebelum peserta tertaut ke data induk pasien, identitasnya disimpan sementara pada
    /// <c>Participant*</c>. Kolom-kolom itu SENSITIF: tidak boleh masuk log aplikasi.
    /// </para>
    /// </remarks>
    [Table("McuOrder", Schema = "public")]
    public class McuOrder : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary><c>MCU-RSMMC-000001</c>, dialokasikan service lewat seri nomor (QBE-CODE-001..004).</summary>
        [Required]
        [MaxLength(30)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public Guid PackageId { get; set; }

        public McuPackage? Package { get; set; }

        [Required]
        [MaxLength(30)]
        public string PackageCodeSnapshot { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PackageNameSnapshot { get; set; } = string.Empty;

        /// <summary>Wajib sejak <i>Hadir</i>; kosong selama identitas masih sementara.</summary>
        public Guid? PatientId { get; set; }

        public MstPatient? Patient { get; set; }

        /// <summary>SENSITIF.</summary>
        [Required]
        [MaxLength(150)]
        public string ParticipantName { get; set; } = string.Empty;

        /// <summary>SENSITIF. 16 digit; wajib bila belum pasien.</summary>
        [MaxLength(16)]
        public string? ParticipantNik { get; set; }

        /// <summary>SENSITIF.</summary>
        public DateOnly? ParticipantBirthDate { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(20)]
        public string? ParticipantPhone { get; set; }

        public McuPatientMatchMethod? PatientMatchMethod { get; set; }

        /// <summary>Petugas yang mengonfirmasi kecocokan pasien (<c>MCU-DEC-039</c>).</summary>
        public Guid? PatientMatchConfirmedByUserId { get; set; }

        public McuOrderSource Source { get; set; }

        /// <summary>Kunjungan RJ/RI/IGD asal rekomendasi.</summary>
        public Guid? SourceEncounterId { get; set; }

        public RegPatientEncounter? SourceEncounter { get; set; }

        public Guid? RecommendingDoctorId { get; set; }

        public MstDoctor? RecommendingDoctor { get; set; }

        /// <summary>Terisi untuk peserta perusahaan.</summary>
        public Guid? CompanyGuarantorId { get; set; }

        public MstCompanyGuarantor? CompanyGuarantor { get; set; }

        public Guid? BookingImportId { get; set; }

        public McuBookingImport? BookingImport { get; set; }

        /// <summary>Tanggal kalender WIB.</summary>
        public DateOnly ExaminationDate { get; set; }

        [Required]
        public Guid DailyScheduleId { get; set; }

        public McuDailySchedule? DailySchedule { get; set; }

        public McuOrderStatus OrderStatus { get; set; } = McuOrderStatus.Scheduled;

        /// <summary>Konfirmasi rencana datang lewat WhatsApp (<c>MCU-DEC-030</c>); kelak.</summary>
        public DateTime? ArrivalConfirmedAt { get; set; }

        /// <summary>Kunjungan <c>MedicalCheckup</c> yang dibuat Registrasi saat check-in; unik.</summary>
        public Guid? EncounterId { get; set; }

        public RegPatientEncounter? Encounter { get; set; }

        public McuCheckInChannel? CheckInChannel { get; set; }

        public DateTime? CheckedInAt { get; set; }

        /// <summary>Akun kiosk untuk jalur kiosk.</summary>
        public Guid? CheckedInByUserId { get; set; }

        /// <summary>Salinan dokter yang berlaku dari <see cref="McuOrderDoctorAssignment"/>.</summary>
        public Guid? McuDoctorId { get; set; }

        public MstDoctor? McuDoctor { get; set; }

        /// <summary>Tarif umum atau tarif perusahaan yang dipakai saat check-in.</summary>
        public Guid? AppliedTariffId { get; set; }

        public MstTariff? AppliedTariff { get; set; }

        /// <summary>Baris paket pada tagihan.</summary>
        public Guid? PackageInvoiceItemId { get; set; }

        public BilInvoiceItem? PackageInvoiceItem { get; set; }

        public McuClearanceBasis? ClearanceBasis { get; set; }

        public DateTime? ExaminationStartedAt { get; set; }

        public DateTime? NoShowAt { get; set; }

        /// <summary>Kolom <c>IsCancel</c>/<c>CancelDateTime</c>/<c>CancelBy</c> ikut diisi saat batal.</summary>
        [MaxLength(500)]
        public string? CancelReason { get; set; }

        /// <summary>Token konkurensi.</summary>
        public int Version { get; set; }

        public ICollection<McuOrderComponent> Components { get; set; } = new List<McuOrderComponent>();

        public ICollection<McuOrderRequirementCheck> RequirementChecks { get; set; } =
            new List<McuOrderRequirementCheck>();

        public ICollection<McuOrderStatusHistory> StatusHistories { get; set; } =
            new List<McuOrderStatusHistory>();

        public ICollection<McuOrderDoctorAssignment> DoctorAssignments { get; set; } =
            new List<McuOrderDoctorAssignment>();

        public ICollection<McuResume> Resumes { get; set; } = new List<McuResume>();
    }
}
