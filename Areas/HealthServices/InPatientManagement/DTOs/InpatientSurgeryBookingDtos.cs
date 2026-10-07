using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs
{
    /// <summary>
    /// Permintaan memesan ruang bedah dari bangsal (<c>BE-RWI-175</c>, kontrak <c>0.10.0</c> API 11.2).
    /// </summary>
    /// <remarks>
    /// Dokter operator diambil dari order tindakan; dokter pemesan dari dokter pemberi instruksi
    /// order (atau dokter order bila instruksinya langsung); penginput dari akun login. Tidak ada
    /// satu pun isian dokter di sini — perawat tidak dapat memilih dokter lain dari layar ini.
    ///
    /// Contoh: Budi S., Melati 302/2, order "Appendektomi" aktif → <c>BookingTab = Surgery</c>,
    /// <c>PreferredAt = 2026-10-03T08:00</c>, <c>PlannedAnesthesiaType = General</c>,
    /// <c>Laterality = NotApplicable</c> → 201, kasus OK berstatus <c>Requested</c>.
    /// </remarks>
    public class SurgeryBookingRequest
    {
        /// <summary><c>Surgery</c> (Bedah Operasi) atau <c>Obstetric</c> (Bedah Obgyn).</summary>
        [Required(ErrorMessage = "Tab pemesanan wajib diisi.")]
        [MaxLength(20)]
        public string BookingTab { get; set; } = string.Empty;

        /// <summary>Tepat satu order tindakan operasi aktif milik kunjungan episode.</summary>
        [Required(ErrorMessage = "Order tindakan operasi wajib dipilih.")]
        public Guid PatientProcedureId { get; set; }

        /// <summary>Tanggal dan jam yang diinginkan; tidak lebih dari 15 menit di masa lalu.</summary>
        [Required(ErrorMessage = "Waktu yang diinginkan wajib diisi.")]
        public DateTime? PreferredAt { get; set; }

        /// <summary><c>General</c>, <c>Regional</c>, <c>Local</c>, atau <c>Sedation</c>.</summary>
        [Required(ErrorMessage = "Rencana jenis anestesi wajib diisi.")]
        [MaxLength(20)]
        public string PlannedAnesthesiaType { get; set; } = string.Empty;

        /// <summary><c>Routine</c>, <c>Urgent</c>, atau <c>Emergency</c>.</summary>
        [Required(ErrorMessage = "Prioritas wajib diisi.")]
        [MaxLength(20)]
        public string Priority { get; set; } = string.Empty;

        /// <summary><c>Elective</c> atau <c>Emergency</c>.</summary>
        [Required(ErrorMessage = "Jenis kasus wajib diisi.")]
        [MaxLength(20)]
        public string CaseType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Indikasi operasi wajib diisi.")]
        [MaxLength(4000, ErrorMessage = "Indikasi operasi maksimal 4000 karakter.")]
        public string Indication { get; set; } = string.Empty;

        /// <summary>Opsional: <c>Left</c>, <c>Right</c>, <c>Bilateral</c>, atau <c>NotApplicable</c>.</summary>
        [MaxLength(30)]
        public string? Laterality { get; set; }

        [Range(1, 1440, ErrorMessage = "Perkiraan durasi harus antara 1 dan 1440 menit.")]
        public int EstimatedMinutes { get; set; }

        [MaxLength(1000, ErrorMessage = "Catatan maksimal 1000 karakter.")]
        public string? Note { get; set; }
    }

    /// <summary>Nilai baku <see cref="SurgeryBookingRequest.BookingTab"/>.</summary>
    public static class SurgeryBookingTabs
    {
        public const string Surgery = "Surgery";
        public const string Obstetric = "Obstetric";
    }
}
