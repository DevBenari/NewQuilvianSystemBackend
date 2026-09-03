using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Penyaring daftar alasan penolakan untuk layar pengelolaan. Mengikuti bentuk paging yang
    /// sudah dipakai keluarga endpoint lain di modul ini.
    /// </summary>
    public class LabRejectionReasonPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Menyaring aktif atau tidak. Kosong berarti keduanya ditampilkan — layar pengelolaan
        /// memang perlu melihat alasan yang sudah dinonaktifkan, tidak seperti jalur baca saat
        /// menolak sampel yang hanya menampilkan yang aktif.
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>Pencarian bebas pada kode, nama, dan keterangan alasan.</summary>
        public string? Search { get; set; }
    }

    /// <summary>
    /// Bentuk tampilan satu alasan penolakan.
    ///
    /// Dipakai dua jalur sekaligus: jalur baca lama pada
    /// <c>GET /lab-specimens/rejection-reasons</c> yang dipakai petugas saat menolak sampel, dan
    /// kelima endpoint pengelolaan pada <c>BE-LAB-06</c>. Karena itu ia tinggal di berkas ini,
    /// bukan di berkas milik salah satu jalur.
    /// </summary>
    public class LabRejectionReasonResponse
    {
        public Guid Id { get; set; }

        public string ReasonCode { get; set; } = string.Empty;

        public string ReasonName { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>
        /// Penanda kesalahan internal rumah sakit. Hanya dapat disetel lewat
        /// <c>PUT /{id}/system-flags</c> yang menuntut <c>LabRejectionReason : SystemFlag</c>.
        /// </summary>
        public bool IsInternalHospitalError { get; set; }

        /// <summary>
        /// Penanda wajib disertai catatan. Terkunci sama seperti penanda kesalahan internal.
        /// </summary>
        public bool RequiresNote { get; set; }

        /// <summary>
        /// Alasan yang nonaktif tidak lagi muncul saat petugas menolak sampel. Ruas ini
        /// ditambahkan untuk layar pengelolaan; jalur baca lama hanya mengembalikan baris aktif
        /// sehingga nilainya selalu benar di sana.
        /// </summary>
        public bool IsActive { get; set; }

        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Menambah alasan penolakan baru.
    ///
    /// Perhatikan yang <b>tidak</b> ada di sini: penanda kesalahan internal dan penanda wajib
    /// catatan. Keduanya sengaja tidak dapat diisi dari permintaan pembuatan (<c>AC-26</c>),
    /// sehingga alasan baru selalu lahir dengan nilai bawaan dan hanya berubah lewat
    /// <c>PUT /{id}/system-flags</c>.
    /// </summary>
    public class CreateLabRejectionReasonRequest
    {
        /// <summary>
        /// Kode alasan. Penanda teknis yang dipilih kepala instalasi saat membuat baru
        /// (<c>BR-15</c>), dinormalkan menjadi huruf kapital dan wajib unik (<c>VAL-36</c>).
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string ReasonCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ReasonName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>Urutan tampil pada daftar pilihan alasan penolakan.</summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Mengubah nama, keterangan, dan urutan tampil sebuah alasan penolakan.
    ///
    /// Kode alasan tidak ikut diubah: menurut <c>BR-15</c> ia hanya ditetapkan saat membuat
    /// baru, karena baris riwayat penolakan yang sudah tersimpan menunjuk kode itu.
    /// </summary>
    public class UpdateLabRejectionReasonRequest
    {
        [Required]
        [MaxLength(200)]
        public string ReasonName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }

        /// <summary>
        /// Penjaga <c>VAL-37</c>, bukan ruas yang boleh diisi.
        ///
        /// Ruas ini diterima justru agar permintaan yang menyelipkan perubahan penanda
        /// kesalahan internal dapat <b>ditolak secara terbuka</b> dengan <c>403</c>, bukan
        /// diabaikan diam-diam. Pemanggil yang mengira perubahannya tersimpan padahal tidak
        /// adalah keadaan yang justru berbahaya, karena penanda ini menentukan siapa menanggung
        /// biaya pengambilan ulang. Pola yang sama dipakai <c>VAL-28</c> pada
        /// <c>LabValueBoundService</c>.
        ///
        /// Biarkan kosong untuk permintaan ubah yang sah.
        /// </summary>
        public bool? IsInternalHospitalError { get; set; }

        /// <summary>Penjaga <c>VAL-37</c> untuk penanda wajib catatan. Biarkan kosong.</summary>
        public bool? RequiresNote { get; set; }
    }

    /// <summary>Mengaktifkan atau menonaktifkan satu alasan penolakan.</summary>
    public class SetLabRejectionReasonActivationRequest
    {
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Menyetel kedua penanda yang terkunci. Hanya dapat dipanggil pemegang
    /// <c>LabRejectionReason : SystemFlag</c>.
    /// </summary>
    public class SetLabRejectionReasonSystemFlagsRequest
    {
        /// <summary>
        /// Menandai alasan yang berakar pada kesalahan internal rumah sakit. Nilainya
        /// menentukan apakah pengambilan ulang ditanggung rumah sakit atau boleh dibebankan
        /// kepada pasien (<c>LAB-INH-011</c>).
        /// </summary>
        public bool IsInternalHospitalError { get; set; }

        /// <summary>Menandai alasan yang mewajibkan petugas mengisi catatan tambahan.</summary>
        public bool RequiresNote { get; set; }

        /// <summary>Alasan penyetelan, disimpan pada catatan log agar keputusannya dapat ditelusuri.</summary>
        [MaxLength(500)]
        public string? ChangeReason { get; set; }
    }
}
