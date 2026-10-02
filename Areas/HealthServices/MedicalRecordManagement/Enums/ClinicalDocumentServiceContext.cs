namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums
{
    /// <summary>
    /// Penyaring layanan pada daftar dokumen milik penulis — <c>BE-RWI-092</c>,
    /// <c>RWI-DEC-142</c>, <c>api-contract.md</c> `0.6.0` bagian 12.13.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa penyaring ini dibutuhkan.</b> Seorang dokter menulis di banyak tempat: poliklinik
    /// pagi, bangsal rawat inap siang, IGD malam. Ruang kerja rawat inap yang menampilkan
    /// seluruhnya memaksa dokter memilah sendiri catatan bangsal dari catatan poliklinik pada
    /// satu daftar panjang. Penyaring ini mempersempitnya di <b>server</b>, bukan di layar.
    /// </para>
    /// <para>
    /// <b><see cref="All"/> adalah bawaannya, dan itu disengaja.</b> Endpoint
    /// <c>my-unsigned</c> sudah dipakai layar rekam medis yang memang ingin melihat seluruh
    /// konsep. Menjadikan <c>Inpatient</c> sebagai bawaan akan menyembunyikan konsep poliklinik
    /// dari layar yang sekarang menampilkannya — perubahan yang tidak diminta siapa pun.
    /// </para>
    /// </remarks>
    public enum ClinicalDocumentServiceContext
    {
        /// <summary>Seluruh layanan. Nilai bawaan; perilaku lama endpoint.</summary>
        All = 0,

        /// <summary>
        /// Hanya dokumen di bawah perawatan rawat inap. Ditentukan dari keberadaan baris
        /// <c>InpEpisode</c> pada kunjungan dokumen itu — bukan dari kolom salinan, sehingga
        /// tidak dapat basi.
        /// </summary>
        Inpatient = 1,

        /// <summary>
        /// Hanya dokumen di luar rawat inap: poliklinik, medical check-up, dan IGD. Kebalikan
        /// tepat dari <see cref="Inpatient"/>, sehingga kedua penyaring bersama-sama selalu
        /// menjumlah menjadi <see cref="All"/>.
        /// </summary>
        Outpatient = 2
    }
}
