using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs
{
    /// <summary>
    /// Response konfigurasi kebijakan presensi rumah sakit (BE-HRD-105, HRD-DEC-105).
    /// </summary>
    public class AttendanceSettingResponse
    {
        public Guid Id { get; set; }
        public string HospitalCode { get; set; } = string.Empty;
        public int GracePeriodMinutes { get; set; }
        public int MaxBackdatedCorrectionDays { get; set; }
        public bool RequireAttachmentForCorrection { get; set; }

        /// <summary>
        /// Scan masuk diterima mulai sekian menit sebelum jam mulai shift (HRD-DEC-211).
        /// </summary>
        public int ScanWindowBeforeMinutes { get; set; }

        /// <summary>
        /// Scan pulang diterima sampai sekian menit sesudah jam selesai shift (HRD-DEC-211).
        /// </summary>
        public int ScanWindowAfterMinutes { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid UpdatedBy { get; set; }
    }

    /// <summary>
    /// Permohonan pembaruan konfigurasi parameter presensi rumah sakit (BE-HRD-105, VR-ATT-17, VR-ATT-18).
    /// </summary>
    public class UpdateAttendanceSettingRequest
    {
        /// <summary>
        /// Batas toleransi keterlambatan dalam menit (0 s.d. 60 menit per VR-ATT-17).
        /// </summary>
        [Range(0, 60, ErrorMessage = "Nilai toleransi atau jendela scan di luar batas.")]
        public int GracePeriodMinutes { get; set; }

        /// <summary>
        /// Batas hari toleransi koreksi tanggal mundur (1 s.d. 30 hari kalender per VR-ATT-18).
        /// </summary>
        [Range(1, 30, ErrorMessage = "Batas toleransi koreksi tanggal mundur (MaxBackdatedCorrectionDays) harus antara 1 hingga 30 hari.")]
        public int MaxBackdatedCorrectionDays { get; set; } = 3;

        public bool RequireAttachmentForCorrection { get; set; } = true;

        /// <summary>
        /// Jendela scan masuk: menit sebelum jam mulai shift, 0 s.d. 720 (VR-PK-23). Kosong = nilai tersimpan tidak diubah
        /// (layar lama belum mengirim kolom ini).
        /// </summary>
        [Range(0, 720, ErrorMessage = "Nilai toleransi atau jendela scan di luar batas.")]
        public int? ScanWindowBeforeMinutes { get; set; }

        /// <summary>
        /// Jendela scan pulang: menit sesudah jam selesai shift, 0 s.d. 960 (VR-PK-23). Kosong = nilai tersimpan tidak diubah.
        /// </summary>
        [Range(0, 960, ErrorMessage = "Nilai toleransi atau jendela scan di luar batas.")]
        public int? ScanWindowAfterMinutes { get; set; }
    }
}
