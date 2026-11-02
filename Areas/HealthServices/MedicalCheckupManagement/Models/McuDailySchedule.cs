using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Hari buka MCU: kuota dan dokter jaga untuk satu tanggal. Contoh 27 Okt buka, kuota 30,
    /// dokter jaga dr. Rina.
    /// </summary>
    /// <remarks>
    /// <see cref="UsedQuota"/> hanya diubah service pakai/lepas kuota, dilindungi
    /// <see cref="Version"/> agar dua booking serentak tidak sama-sama mengambil kursi terakhir.
    /// </remarks>
    [Table("McuDailySchedule", Schema = "public")]
    public class McuDailySchedule : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Tanggal kalender WIB; satu baris per tanggal.</summary>
        public DateOnly ScheduleDate { get; set; }

        public bool IsOpen { get; set; } = true;

        /// <summary>Kuota dasar; ≥ 0.</summary>
        public int Quota { get; set; }

        public int UsedQuota { get; set; }

        /// <summary>Jumlah tambahan dari seluruh <see cref="McuQuotaOverride"/> yang disetujui.</summary>
        public int ApprovedOverrideQuota { get; set; }

        /// <summary>
        /// Dokter MCU jaga (<c>MCU-DEC-042</c>): dipakai otomatis oleh check-in kiosk dan menjadi isian
        /// bawaan check-in admisi. Kosong → kiosk menolak (<c>VAL-MCU-47</c>).
        /// </summary>
        public Guid? DutyMcuDoctorId { get; set; }

        public MstDoctor? DutyMcuDoctor { get; set; }

        /// <summary>Token konkurensi.</summary>
        public int Version { get; set; }

        public ICollection<McuQuotaOverride> QuotaOverrides { get; set; } = new List<McuQuotaOverride>();
    }
}
