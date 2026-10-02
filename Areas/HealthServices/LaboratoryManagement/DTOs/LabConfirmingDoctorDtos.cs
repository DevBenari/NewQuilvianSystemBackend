namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Satu dokter yang dapat dipilih sebagai Dokter Konfirmator.
    ///
    /// Ketiga ruasnya dibaca dari <c>MstDoctor</c>, <b>nol disalin</b> ke tabel Laboratorium
    /// (<c>LAB-DEC-111</c>).
    /// </summary>
    public class LabDoctorOption
    {
        public Guid DoctorId { get; set; }

        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Nomor WhatsApp. <b>Boleh kosong</b>, dan layar wajib menyiapkan keadaan itu —
        /// <c>MstDoctor.WhatsAppNumber</c> memang nullable, sehingga dokter yang benar dapat
        /// dipilih tanpa nomor yang dapat dihubungi.
        /// </summary>
        public string? WhatsAppNumber { get; set; }
    }

    /// <summary>
    /// Pilihan Dokter Konfirmator beserta <b>sumbernya</b> (<c>LAB-API-v1</c> <c>r26</c>
    /// bagian 21.7, <c>LAB-DEC-111</c>).
    /// </summary>
    public class LabConfirmingDoctorOptionsResponse
    {
        /// <summary>DPJP dari pesanan. Kosong bila kunjungannya memang tidak berdokter.</summary>
        public LabDoctorOption? AttendingDoctor { get; set; }

        /// <summary>
        /// Dokter yang penugasan jaganya <b>aktif pada jam permintaan</b>, dari
        /// <c>TrxOnCallAssignment</c>.
        /// </summary>
        public List<LabDoctorOption> OnDutyDoctors { get; set; } = [];

        /// <summary>
        /// <b>Ruas inilah yang membuat <c>LAB-DEC-111</c> dapat diuji.</b> Tanpa ia, layar tidak
        /// dapat membedakan <i>"malam ini memang tidak ada dokter jaga"</i> dari <i>"jadwal
        /// jaganya belum pernah diisi siapa pun"</i> — keduanya terlihat sama: daftar kosong.
        ///
        /// <c>TrxOnCallAssignment</c> hari ini nol punya endpoint pengisi (<c>LAB-COORD-014</c>),
        /// sehingga keadaan kedua itulah yang <b>pasti terjadi lebih dulu</b>.
        /// </summary>
        public bool OnDutyScheduleAvailable { get; set; }

        /// <summary>
        /// Daftar dokter aktif yang dapat dicari. <b>Hanya terisi</b> ketika
        /// <see cref="OnDutyScheduleAvailable"/> bernilai salah.
        /// </summary>
        public List<LabDoctorOption> FallbackDoctors { get; set; } = [];

        /// <summary>
        /// Keterangan yang wajib ditampilkan bersama jalur jatuh. Kosong ketika jadwal jaga
        /// tersedia.
        /// </summary>
        public string? FallbackNote { get; set; }

        /// <summary>
        /// Benar ketika <see cref="FallbackDoctors"/> dipotong oleh batas jumlah — layar perlu
        /// tahu bahwa mengetik pencarian akan memunculkan dokter lain.
        /// </summary>
        public bool FallbackTruncated { get; set; }
    }
}
