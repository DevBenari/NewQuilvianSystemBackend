using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs
{
    /// <summary>
    /// Bentuk balasan satu keperluan akses rekam medis.
    /// </summary>
    /// <remarks>
    /// Master ini menentukan perilaku layar lain, bukan sekadar daftar pilihan:
    /// <c>IsFreeTextRequired</c> memunculkan kotak alasan bebas pada kotak keperluan, dan
    /// <c>RequiresReview</c> menentukan apakah pembukaan berkas dengan keperluan ini masuk
    /// antrean tinjauan unit rekam medis.
    /// </remarks>
    public class MedicalRecordAccessPurposeResponse
    {
        public Guid Id { get; set; }

        /// <summary>Kode keperluan, unik di seluruh tabel. Contoh <c>RUJUKAN</c>.</summary>
        public string PurposeCode { get; set; } = string.Empty;

        public string PurposeName { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>
        /// Bila benar, pengguna wajib menuliskan alasannya sendiri. Dipakai pilihan seperti
        /// "Lainnya", yang tanpa penjelasan tambahan tidak berguna saat ditinjau.
        /// </summary>
        public bool IsFreeTextRequired { get; set; }

        /// <summary>
        /// Bila benar, akses dengan keperluan ini masuk antrean tinjauan unit rekam medis.
        /// </summary>
        public bool RequiresReview { get; set; }

        /// <summary>Urutan tampil keperluan pada kotak pilihan.</summary>
        public int SortOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreateDateTime { get; set; }

        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateMedicalRecordAccessPurposeRequest
    {
        [Required(ErrorMessage = "Kode keperluan wajib diisi.")]
        [MaxLength(50, ErrorMessage = "Kode keperluan terlalu panjang. Batasnya 50 huruf.")]
        public string PurposeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama keperluan wajib diisi.")]
        [MaxLength(150, ErrorMessage = "Nama keperluan terlalu panjang. Batasnya 150 huruf.")]
        public string PurposeName { get; set; } = string.Empty;

        [MaxLength(250, ErrorMessage = "Keterangan terlalu panjang. Batasnya 250 huruf.")]
        public string? Description { get; set; }

        public bool IsFreeTextRequired { get; set; } = false;

        /// <summary>
        /// Bawaannya <c>true</c>, mengikuti nilai bawaan pada entity. Keperluan baru yang
        /// belum dinilai unit rekam medis lebih baik ikut ditelaah daripada lolos diam-diam.
        /// </summary>
        public bool RequiresReview { get; set; } = true;

        [Range(0, 9999, ErrorMessage = "Urutan tampil harus antara 0 dan 9999.")]
        public int SortOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Bentuk permintaan mengubah keperluan akses. Sengaja memuat <c>PurposeCode</c>, karena
    /// keperluan yang salah ketik kodenya harus dapat dibetulkan selama kode barunya belum
    /// dipakai keperluan lain.
    /// </summary>
    public class UpdateMedicalRecordAccessPurposeRequest
        : CreateMedicalRecordAccessPurposeRequest
    {
    }

    /// <summary>
    /// Bentuk permintaan mengaktifkan atau menonaktifkan keperluan akses.
    /// </summary>
    /// <remarks>
    /// Menonaktifkan keperluan TIDAK menyentuh satu pun baris jejak akses yang sudah memakainya.
    /// Jejak adalah catatan bahwa seseorang pernah membuka berkas dengan alasan tertentu pada
    /// suatu waktu; ia harus tetap terbaca utuh puluhan tahun kemudian, apa pun yang terjadi
    /// pada masternya.
    ///
    /// Yang berubah hanya ke depan: keperluan yang dinonaktifkan tidak lagi muncul sebagai
    /// pilihan, dan pemakaiannya ditolak <c>400</c> oleh penilaian akses.
    /// </remarks>
    public class UpdateMedicalRecordAccessPurposeStatusRequest
    {
        public bool IsActive { get; set; }
    }
}
