namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs
{
    // =====================================================================
    // Dua data induk alasan — r34 bagian 29.6, BE-LAB-71.
    //
    // SATU set DTO dipakai LabResultCorrectionReason dan LabFourEyesExceptionReason: bentuk
    // kolom keduanya identik (kamus data 17.1-17.2). Menyalin pola LabMicrobiologyMasterDataDtos,
    // dengan DUA perbedaan yang disengaja:
    //
    // 1. UpdateRequest TIDAK membawa IsActive. Status hanya berubah lewat StatusRequest, sehingga
    //    jebakan yang ditemukan FE-LAB-24 — IsActive yang tidak dikirim terbaca true lalu
    //    menghidupkan kembali baris nonaktif diam-diam — tidak dapat terjadi.
    // 2. CreateRequest TIDAK membawa RequiresNote. Penanda itu hanya lewat SystemFlagsRequest,
    //    yang dipegang admin sistem saja (pola LAB-DEC-019).
    // =====================================================================

    /// <summary>Penyaring daftar alasan untuk layar pengelolaan. Memuat yang nonaktif juga.</summary>
    public class LabResultReasonPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        /// <summary>Kosong berarti aktif dan nonaktif ditampilkan.</summary>
        public bool? IsActive { get; set; }

        /// <summary>Pencarian bebas pada kode, nama, dan keterangan.</summary>
        public string? Search { get; set; }
    }

    public class LabResultReasonResponse
    {
        public Guid Id { get; set; }

        public string ReasonCode { get; set; } = string.Empty;

        public string ReasonName { get; set; } = string.Empty;

        public string? Description { get; set; }

        /// <summary>Catatan bebas wajib diisi saat alasan ini dipilih. Hanya diubah lewat <c>PUT /{id}/system-flags</c>.</summary>
        public bool RequiresNote { get; set; }

        public bool IsActive { get; set; }

        public int SortOrder { get; set; }
    }

    /// <summary>Satu pilihan alasan <b>aktif</b> — untuk kotak pilihan pada tindakan hasil.</summary>
    public class LabResultReasonOptionResponse
    {
        public Guid Id { get; set; }

        public string ReasonCode { get; set; } = string.Empty;

        public string ReasonName { get; set; } = string.Empty;

        /// <summary>Layar memakainya untuk mewajibkan catatan <b>sebelum</b> dikirim.</summary>
        public bool RequiresNote { get; set; }

        public int SortOrder { get; set; }
    }

    public class LabResultReasonCreateRequest
    {
        /// <summary>
        /// Maks. 32 karakter; hanya huruf besar, angka, dan tanda hubung (<c>VAL-141</c>), misalnya
        /// <c>SAMPEL-TERTUKAR</c>. Tidak dapat diubah sesudah dibuat (<c>VAL-142</c>).
        /// </summary>
        public string ReasonCode { get; set; } = string.Empty;

        /// <summary>Maks. 200 karakter.</summary>
        public string ReasonName { get; set; } = string.Empty;

        /// <summary>Maks. 256 karakter.</summary>
        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }

    public class LabResultReasonUpdateRequest
    {
        /// <summary>
        /// Opsional, dan <b>hanya</b> diterima bila sama dengan kode yang tersimpan. Kode yang
        /// berbeda ditolak <c>VAL-142</c>, bukan diabaikan diam-diam: petugas yang mengira sudah
        /// mengganti kode harus tahu bahwa laporan mutu tetap menghitung kode lamanya.
        /// </summary>
        public string? ReasonCode { get; set; }

        public string ReasonName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int SortOrder { get; set; }
    }

    public class LabResultReasonStatusRequest
    {
        public bool IsActive { get; set; }
    }

    public class LabResultReasonSystemFlagsRequest
    {
        public bool RequiresNote { get; set; }
    }

    public class LabResultReasonSummaryResponse
    {
        public int TotalReason { get; set; }

        public int ActiveReason { get; set; }

        public int InactiveReason { get; set; }

        /// <summary>Alasan yang mewajibkan catatan bebas.</summary>
        public int RequiresNoteReason { get; set; }
    }

    /// <summary>
    /// Bentuk layar kedua data induk alasan. <c>SortOptions</c> sengaja kosong: daftarnya
    /// diurutkan tetap — urutan tampil lalu nama — dan query-nya tidak punya ruas urut.
    /// </summary>
    public class LabResultReasonFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        public List<LabSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        public List<LabQueryParameterInfoResponse> QueryParameters { get; set; } = new();

        public bool SupportsServerSideFiltering { get; set; } = true;

        public bool SupportsServerSidePaging { get; set; } = true;

        /// <summary><b>Salah, dan disengaja.</b> Alasan yang pernah dipakai tidak boleh hilang dari riwayat.</summary>
        public bool IsDeletable { get; set; }
    }
}
