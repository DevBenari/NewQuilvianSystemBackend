using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    /// <summary>
    /// Saringan kuota terpakai jadwal dokter (<c>RJ-REG-REV-BE-006</c>, <c>RJ-REG-DEC-010</c>).
    /// </summary>
    public class DoctorScheduleQuotaQuery
    {
        /// <summary>Tanggal kunjungan, format <c>yyyy-MM-dd</c>. Wajib.</summary>
        [Required(ErrorMessage = "Tanggal kunjungan wajib diisi.")]
        public string? VisitDate { get; set; }

        /// <summary>Opsional: batasi ke jadwal satu poliklinik.</summary>
        public Guid? ClinicId { get; set; }
    }

    /// <summary>
    /// Kuota dan pemakaian satu jadwal dokter pada satu tanggal kunjungan. Kuota <c>0</c> berarti
    /// tanpa batas, jadi tidak pernah penuh. Hitungannya sama dengan validasi simpan kunjungan.
    /// </summary>
    public class DoctorScheduleQuotaResponse
    {
        public Guid DoctorScheduleId { get; set; }
        public int MaxPatientQuota { get; set; }
        public int MaxWalkInQuota { get; set; }
        public int MaxAppointmentQuota { get; set; }
        public int UsedTotal { get; set; }
        public int UsedWalkIn { get; set; }
        public int UsedAppointment { get; set; }

        /// <summary>Pendaftaran walk-in (kunjungan hari ini) ditolak karena kuota total atau walk-in penuh.</summary>
        public bool IsFullForWalkIn { get; set; }

        /// <summary>Appointment (tanggal mendatang) ditolak karena kuota total atau appointment penuh.</summary>
        public bool IsFullForAppointment { get; set; }
    }
}
