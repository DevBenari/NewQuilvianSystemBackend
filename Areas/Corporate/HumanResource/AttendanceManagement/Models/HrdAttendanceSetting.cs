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
        /// Usang sejak HRD-DEC-216: konversi saat tutup periode selalu terjadi. Kolom dibiarkan agar
        /// rollback aman; tidak dibaca dan tidak dikirim lewat DTO. Dihapus pada task perapian terpisah.
        /// </summary>
        public bool AutoConvertIncompleteToAbsent { get; set; } = true;

        /// <summary>
        /// Scan masuk diterima mulai sekian menit sebelum jam mulai shift (HRD-DEC-211, HRD-DEC-215).
        /// </summary>
        public int ScanWindowBeforeMinutes { get; set; } = 240;

        /// <summary>
        /// Scan pulang diterima sampai sekian menit sesudah jam selesai shift (HRD-DEC-211, HRD-DEC-215).
        /// </summary>
        public int ScanWindowAfterMinutes { get; set; } = 480;

        public bool IsActive { get; set; } = true;
    }
}
