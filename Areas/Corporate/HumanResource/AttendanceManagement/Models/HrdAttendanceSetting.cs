using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models
{
    /// <summary>
    /// Pengaturan parameter kebijakan presensi dan kehadiran rumah sakit (BE-HRD-104, BE-HRD-105, HRD-DEC-105, HRD-DEC-106).
    /// </summary>
    [Table("HrdAttendanceSetting", Schema = "public")]
    public class HrdAttendanceSetting : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string HospitalCode { get; set; } = "RSMMC";

        /// <summary>
        /// Toleransi keterlambatan tanpa sanksi/late dalam menit (default 10 menit per HRD-DEC-105).
        /// </summary>
        public int GracePeriodMinutes { get; set; } = 10;

        /// <summary>
        /// Batas maksimal hari toleransi koreksi tanggal mundur bagi Kepala Ruangan (default 3 hari per HRD-DEC-102 & VR-ATT-18).
        /// </summary>
        public int MaxBackdatedCorrectionDays { get; set; } = 3;

        /// <summary>
        /// Kewajiban melampirkan berkas bukti dokumen pendukung saat permohonan koreksi.
        /// </summary>
        public bool RequireAttachmentForCorrection { get; set; } = true;

        /// <summary>
        /// Konversi otomatis sisa 1 tap (IncompletePunch) menjadi Mangkir (Absent) saat tutup periode (HRD-DEC-106).
        /// </summary>
        public bool AutoConvertIncompleteToAbsent { get; set; } = true;

        public bool IsActive { get; set; } = true;
    }
}
