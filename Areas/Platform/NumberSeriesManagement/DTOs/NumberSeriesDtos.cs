namespace QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.DTOs
{
    /// <summary>
    /// Keadaan satu deret nomor pada satu periode, sebagaimana dibaca layar pemantauan.
    /// </summary>
    /// <remarks>
    /// <b>Seluruh field di sini hanya dibaca.</b> Tidak ada DTO permintaan tulis pada modul ini,
    /// dan ketiadaan itu disengaja: menyunting pencacah berarti menerbitkan ulang nomor yang
    /// sudah menempel pada catatan lain (<c>INV-PLT-001</c>).
    /// </remarks>
    public class NumberSeriesResponse
    {
        public Guid Id { get; set; }

        /// <summary>Penanda deret milik modul pemakainya, contoh <c>BBK_BLOOD_ORDER</c>.</summary>
        public string SequenceKey { get; set; } = string.Empty;

        /// <summary>Periode berlakunya pencacah — <c>GLOBAL</c> untuk deret yang tidak diulang.</summary>
        public string ScopeKey { get; set; } = string.Empty;

        public string ResetPolicy { get; set; } = string.Empty;

        /// <summary>
        /// Nilai terakhir yang <b>sudah terbit</b> — bukan nomor berikutnya.
        /// </summary>
        /// <remarks>
        /// Perbedaan satu langkah ini menyesatkan bila labelnya kabur, sehingga layar pemantauan
        /// wajib menyebutkannya apa adanya.
        /// </remarks>
        public long CurrentValue { get; set; }

        public DateTimeOffset LastAllocatedAt { get; set; }
    }

    /// <summary>
    /// Ringkasan tiga angka untuk kepala layar pemantauan.
    /// </summary>
    public class NumberSeriesSummaryResponse
    {
        /// <summary>Jumlah <c>SequenceKey</c> yang berbeda.</summary>
        public int TotalSeries { get; set; }

        /// <summary>Jumlah baris — yaitu deret dikali periode.</summary>
        public int TotalScope { get; set; }

        /// <summary>
        /// Waktu alokasi terakhir di seluruh deret. <c>null</c> ketika belum ada satu deret pun.
        /// </summary>
        /// <remarks>
        /// Tabel ini lahir kosong dan barisnya baru muncul pada alokasi pertama, sehingga keadaan
        /// "belum ada apa-apa" adalah keadaan sah yang wajib terbaca, bukan galat.
        /// </remarks>
        public DateTimeOffset? LastAllocatedAt { get; set; }
    }

    public class NumberSeriesSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// Nilai bawaan penyaring saat layar pemantauan pertama dibuka.
    /// </summary>
    public class NumberSeriesDefaultFilterResponse
    {
        public string? Search { get; set; }

        public string? SequenceKey { get; set; }

        public string? ScopeKey { get; set; }

        public string? ResetPolicy { get; set; }

        public string SortBy { get; set; } = "sequenceKey";

        public string SortDirection { get; set; } = "asc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Konfigurasi penyaring dan pengurutan halaman pemantauan.
    /// </summary>
    /// <remarks>
    /// <b>Tidak memuat metadata field buat maupun sunting</b>, tidak seperti metadata master data.
    /// Modul ini tidak punya satu pun endpoint tulis, sehingga menyertakannya akan menjanjikan
    /// tombol yang tidak pernah ada.
    /// </remarks>
    public class NumberSeriesFilterMetadataResponse
    {
        public NumberSeriesDefaultFilterResponse DefaultFilter { get; set; } = new();

        public List<NumberSeriesSortOptionResponse> SortOptions { get; set; } = new();

        public List<string> SortDirections { get; set; } = new();

        public List<int> PageSizeOptions { get; set; } = new();

        /// <summary>Keempat kebijakan pengulangan yang sah, untuk kotak penyaring.</summary>
        public List<string> ResetPolicyOptions { get; set; } = new();

        public string ResetButtonLabel { get; set; } = "Reset";
    }

    /// <summary>
    /// Parameter permintaan daftar deret. Dibungkus satu kelas agar tanda tangan query service
    /// tidak menjadi deretan parameter longgar yang mudah tertukar urutannya.
    /// </summary>
    public class NumberSeriesPagedQuery
    {
        public string? Search { get; set; }

        public string? SequenceKey { get; set; }

        public string? ScopeKey { get; set; }

        public string? ResetPolicy { get; set; }

        public string? SortBy { get; set; } = "sequenceKey";

        public string? SortDirection { get; set; } = "asc";

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }
}
