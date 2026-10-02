namespace QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services
{
    /// <summary>
    /// Alokasi nomor ditolak. Dilempar kepada service modul pemanggil, bukan dipulangkan sebagai
    /// response HTTP — alokasi bukan endpoint.
    /// </summary>
    /// <remarks>
    /// <b>Modul pemanggil yang menerjemahkannya menjadi pesan bagi petugas</b>, karena hanya
    /// modul itu yang tahu tindakan apa yang sedang dicoba petugas. Platform tidak mengenal
    /// order darah, invoice, maupun kunjungan.
    ///
    /// <b>Setiap kali exception ini dilempar, nol nomor terbit.</b> Penolakan terjadi sebelum
    /// pencacah di-<c>commit</c>, sehingga deret tidak berlubang karena permintaan yang ditolak.
    /// Lubang hanya lahir ketika nomor sudah terbit lalu pekerjaan bisnisnya dibatalkan
    /// (<c>DEC-PLT-008</c>).
    /// </remarks>
    public class NumberSeriesAllocationException : Exception
    {
        public NumberSeriesAllocationException(string message)
            : base(message)
        {
        }

        public NumberSeriesAllocationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Kode validasi pada <c>contracts/validation-matrix.md</c>, contoh <c>VAL-PLT-002</c>.
        /// Disimpan supaya modul pemanggil dapat menelusuri sebabnya tanpa mencocokkan teks pesan.
        /// </summary>
        public string? ValidationCode { get; init; }
    }
}
