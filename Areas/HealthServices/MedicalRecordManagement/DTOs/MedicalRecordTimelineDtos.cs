using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs
{
    /// <summary>
    /// Keterangan satu permintaan riwayat rekam medis.
    ///
    /// Seluruh pembatas ada di sini karena penggabungan tiga belas sumber tanpa pembatas dapat
    /// membebani basis data. Lihat `RM-CAP-004` dan acceptance criteria `BE-13` nomor 2 dan 3.
    /// </summary>
    public class MedicalRecordTimelineQuery
    {
        /// <summary>Pasien yang riwayatnya dibuka. Wajib.</summary>
        public Guid PatientId { get; set; }

        /// <summary>
        /// Jenis dokumen yang diminta. Bila kosong, seluruh tiga belas jenis diambil.
        ///
        /// Mengisi kolom ini adalah cara paling ampuh menekan jumlah query: satu jenis dokumen
        /// berarti satu query, bukan tiga belas.
        /// </summary>
        public IReadOnlyCollection<ClinicalDocumentKind>? DocumentKinds { get; set; }

        /// <summary>Bila diisi, hanya dokumen milik kunjungan itu yang diambil.</summary>
        public Guid? EncounterId { get; set; }

        /// <summary>Batas awal rentang tanggal. Diterapkan pada kolom tanggal masing-masing sumber.</summary>
        public DateTime? StartDate { get; set; }

        /// <summary>Batas akhir rentang tanggal.</summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Ikut menampilkan dokumen yang dibatalkan. Bawaannya tidak, mengikuti perilaku
        /// endpoint riwayat CPPT yang sudah berjalan.
        /// </summary>
        public bool IncludeCancelled { get; set; } = false;

        /// <summary>Terbaru dahulu. Bawaannya ya, karena berkas rekam medis lazim dibaca dari yang paling baru.</summary>
        public bool NewestFirst { get; set; } = true;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Satu baris riwayat, apa pun jenis dokumen asalnya.
    ///
    /// Bentuknya sengaja seragam untuk tiga belas sumber supaya layar rekam medis dapat
    /// menampilkannya sebagai satu daftar, bukan tiga belas daftar terpisah.
    /// </summary>
    public class MedicalRecordTimelineItemResponse
    {
        public ClinicalDocumentKind DocumentKind { get; set; }

        /// <summary>Nama jenis dokumen dalam Bahasa Indonesia, siap ditampilkan.</summary>
        public string DocumentKindName { get; set; } = string.Empty;

        /// <summary>Id baris pada tabel asalnya. Dipakai untuk membuka detail dokumen.</summary>
        public Guid DocumentId { get; set; }

        /// <summary>Kunjungan yang menaungi dokumen. Dapat kosong pada dokumen yang tidak terikat kunjungan.</summary>
        public Guid? EncounterId { get; set; }

        /// <summary>Waktu kejadian dokumen. Inilah yang dipakai mengurutkan seluruh daftar.</summary>
        public DateTime OccurredAt { get; set; }

        /// <summary>Nomor dokumen pada tabel asalnya, bila ada.</summary>
        public string? DocumentNumber { get; set; }

        /// <summary>Judul singkat, misalnya nama diagnosis atau judul surat.</summary>
        public string? Title { get; set; }

        /// <summary>Keterangan pendek. Dipotong di sisi server supaya daftar tidak membawa isi catatan penuh.</summary>
        public string? Summary { get; set; }

        public bool IsCancelled { get; set; }

        /// <summary>
        /// Apakah jenis dokumen ini sudah tunduk aturan keutuhan pada rilis sekarang.
        ///
        /// Rilis pertama hanya menegakkan CPPT (`RM-DEC-019`). Nilai `false` WAJIB dinyatakan
        /// terbuka di layar sesuai `RM-FE-009`, bukan didiamkan seolah-olah dokumennya sudah
        /// terlindungi.
        /// </summary>
        public bool IsIntegrityEnforced { get; set; }

        /// <summary>Status keutuhan dokumen. Kosong bila dokumen belum terdaftar pada daftar keutuhan.</summary>
        public ClinicalDocumentIntegrityStatus? IntegrityStatus { get; set; }

        public string? IntegrityStatusName { get; set; }
    }

    /// <summary>
    /// Sumber yang gagal diambil beserta alasannya.
    ///
    /// Ini penerapan acceptance criteria `BE-13` nomor 4. Satu sumber yang bermasalah tidak
    /// boleh menghapus seluruh riwayat pasien dari layar — tetapi pembacanya juga tidak boleh
    /// dibiarkan mengira daftar yang tampil sudah lengkap.
    /// </summary>
    public class MedicalRecordTimelineSourceFailure
    {
        public ClinicalDocumentKind DocumentKind { get; set; }

        public string DocumentKindName { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Hasil satu permintaan riwayat: isinya, sumber yang diminta, sumber yang gagal, dan
    /// penanda apakah daftarnya terpotong batas.
    /// </summary>
    public class MedicalRecordTimelineResult
    {
        public PagedResult<MedicalRecordTimelineItemResponse> Page { get; set; } = new();

        /// <summary>Jenis dokumen yang benar-benar ditanyakan ke basis data pada permintaan ini.</summary>
        public List<ClinicalDocumentKind> RequestedKinds { get; set; } = new();

        /// <summary>Kosong berarti seluruh sumber berhasil dibaca.</summary>
        public List<MedicalRecordTimelineSourceFailure> FailedSources { get; set; } = new();

        /// <summary>
        /// Benar bila ada sumber yang datanya melampaui batas pengambilan per sumber.
        ///
        /// <see cref="PagedResult{T}.TotalData"/> tetap jumlah yang sebenarnya, karena dihitung
        /// terpisah. Yang perlu diwaspadai adalah isi halamannya: bila sebuah sumber terpotong,
        /// masih mungkin ada dokumen yang seharusnya masuk urutan halaman ini tetapi tidak ikut
        /// terambil. Persempit rentang tanggal atau jenis dokumen bila penandanya menyala.
        /// </summary>
        public bool IsTruncated { get; set; }

        /// <summary>Ringkasan siap tampil untuk memberi tahu pembaca bahwa daftarnya tidak utuh.</summary>
        public bool IsComplete => FailedSources.Count == 0 && !IsTruncated;
    }
}
