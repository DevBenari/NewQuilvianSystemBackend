using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    /// <summary>
    /// Penyaring daftar jenis specimen untuk layar pengelolaan kepala instalasi. Mengikuti
    /// bentuk paging yang sudah dipakai keluarga endpoint lain di modul ini.
    /// </summary>
    public class LabSpecimenTypePagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Menyaring aktif atau tidak. Kosong berarti keduanya ditampilkan — layar pengelolaan
        /// memang perlu melihat jenis yang sudah dinonaktifkan, tidak seperti daftar pilihan
        /// saat mencatat wadah yang hanya menampilkan yang aktif.
        /// </summary>
        public bool? IsActive { get; set; }

        /// <summary>Pencarian bebas pada kode, nama, dan keterangan jenis.</summary>
        public string? Search { get; set; }
    }

    /// <summary>
    /// Penyaring daftar pilihan jenis specimen untuk layar penerimaan. Berbeda dari
    /// <see cref="LabSpecimenTypePagedQuery"/>: jalur ini <b>hanya</b> mengembalikan jenis yang
    /// aktif, karena petugas tidak boleh memilih jenis yang sudah ditarik dari peredaran
    /// (<c>VAL-55</c>).
    /// </summary>
    public class LabSpecimenTypeOptionQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 50;

        public string? Search { get; set; }
    }

    /// <summary>
    /// Penyaring daftar pantau pemakaian <c>Lainnya</c> (<c>FR-11.2</c>, <c>AC-60</c>).
    ///
    /// Rentang waktunya boleh dikosongkan; bila kosong dipakai 90 hari terakhir. Rentang yang
    /// panjang disengaja — yang dicari kepala instalasi adalah keterangan yang <b>berulang</b>,
    /// dan pengulangan tidak terlihat pada jendela satu minggu.
    /// </summary>
    public class LabSpecimenOtherUsageQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        /// <summary>Pencarian bebas pada keterangannya.</summary>
        public string? Search { get; set; }
    }

    /// <summary>
    /// Satu keterangan <c>Lainnya</c> beserta seberapa sering ia dipakai.
    ///
    /// <b>Tidak ada tabel yang menyimpan bentuk ini.</b> Seluruh isinya diturunkan dengan
    /// mengelompokkan <c>LabSpecimen</c> yang jenisnya ber-<c>IsOtherBucket</c>. Tabel ringkasan
    /// tersendiri akan menjadi salinan yang bisa basi tanpa menambah satu pun jawaban baru.
    /// </summary>
    public class LabSpecimenOtherUsageResponse
    {
        /// <summary>
        /// Keterangan yang ditulis petugas, <b>apa adanya</b>.
        ///
        /// Sengaja tidak dinormalkan. "cairan kista", "Cairan Kista", dan "c. kista" muncul
        /// sebagai tiga baris terpisah justru supaya kepala instalasi <b>melihat</b> bahwa
        /// ketiganya satu hal, lalu menaikkannya menjadi jenis tetap. Menggabungkannya di sini
        /// akan menyembunyikan keragaman ejaan yang menjadi alasan layar ini dibuat.
        /// </summary>
        public string OtherNote { get; set; } = string.Empty;

        /// <summary>Berapa kali keterangan ini dipakai pada rentang yang diminta.</summary>
        public int UsageCount { get; set; }

        /// <summary>Pemakaian paling akhir, dipakai menilai apakah keterangan ini masih hidup.</summary>
        public DateTime LastUsedAt { get; set; }

        /// <summary>Pemakaian paling awal, dipakai menilai sudah berapa lama ia berulang.</summary>
        public DateTime FirstUsedAt { get; set; }
    }

    /// <summary>Bentuk tampilan satu jenis specimen pada layar pengelolaan.</summary>
    public class LabSpecimenTypeResponse
    {
        public Guid Id { get; set; }

        public string SpecimenTypeCode { get; set; } = string.Empty;

        public string SpecimenTypeName { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>
        /// Penanda baris <c>Lainnya</c>. Wadah berjenis ini wajib disertai keterangan, dan
        /// layar penerimaan memakai penanda ini untuk memunculkan kolom keterangannya.
        /// </summary>
        public bool IsOtherBucket { get; set; }

        public bool IsActive { get; set; }

        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Bentuk ringan untuk daftar pilihan di layar penerimaan. Sengaja tidak memuat keterangan
    /// dan status aktif: daftar ini hanya berisi jenis aktif, sehingga keduanya tidak menambah
    /// satu pun keputusan bagi petugas.
    /// </summary>
    public class LabSpecimenTypeOptionResponse
    {
        public Guid Id { get; set; }

        public string SpecimenTypeCode { get; set; } = string.Empty;

        public string SpecimenTypeName { get; set; } = string.Empty;

        public bool IsOtherBucket { get; set; }

        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Menambah jenis specimen baru.
    ///
    /// Perhatikan yang <b>tidak</b> ada di sini: penanda <c>Lainnya</c>. Baris <c>Lainnya</c>
    /// lahir dari data awal dan tidak dapat dibuat lewat permintaan, karena hanya boleh ada
    /// satu yang aktif (<c>VAL-62</c>). Jenis baru selalu lahir sebagai jenis biasa.
    /// </summary>
    public class CreateLabSpecimenTypeRequest
    {
        /// <summary>
        /// Kode jenis, dinormalkan menjadi huruf kapital dan wajib unik (<c>VAL-61</c>).
        /// </summary>
        [Required]
        [MaxLength(32)]
        public string SpecimenTypeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        public string SpecimenTypeName { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }

    /// <summary>
    /// Mengubah nama, keterangan, dan urutan tampil.
    ///
    /// Kode jenis tidak ikut berubah: ia menjadi penanda yang dipakai seeder dan pemanggil
    /// lain, sehingga mengubahnya diam-diam memutus rujukan yang sudah ada. Penanda
    /// <c>Lainnya</c> juga tidak ada di sini — bila disertakan, permintaannya ditolak
    /// <c>422</c> oleh <c>VAL-62</c> alih-alih diabaikan diam-diam.
    /// </summary>
    public class UpdateLabSpecimenTypeRequest
    {
        [Required]
        [MaxLength(128)]
        public string SpecimenTypeName { get; set; } = string.Empty;

        [MaxLength(256)]
        public string? Description { get; set; }

        public int SortOrder { get; set; }

        /// <summary>
        /// Sengaja diterima lalu ditolak bila disertakan dan berbeda dari nilai tersimpan.
        /// Menolak secara terbuka lebih jujur daripada mengabaikan diam-diam, karena pemanggil
        /// yang mengira penandanya sudah berubah padahal tidak adalah keadaan yang justru
        /// berbahaya. Pola ini sama dengan <c>VAL-37</c> pada alasan penolakan sampel.
        /// </summary>
        public bool? IsOtherBucket { get; set; }
    }

    /// <summary>Mengaktifkan atau menonaktifkan satu jenis specimen.</summary>
    public class SetLabSpecimenTypeActivationRequest
    {
        public bool IsActive { get; set; }
    }
}
