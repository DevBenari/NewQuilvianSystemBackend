namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Keadaan satu sumber klinis ketika usulan isian resume pulang disusun. Lahir
    /// <c>BE-RWI-086</c> menyerap <c>RWI-DEC-112</c>.
    /// </summary>
    /// <remarks>
    /// <b>Tiga keadaan, bukan dua.</b> "Tidak ada isinya" dan "tidak dapat dibaca" adalah dua
    /// hal yang sangat berbeda bagi dokter yang menekan tombol "Isi dari data klinis". Yang
    /// pertama berarti memang tidak ada bahan; yang kedua berarti ada bahan tetapi sistem gagal
    /// mengambilnya, dan dokter perlu memeriksanya sendiri sebelum menandatangani. Menggabungkan
    /// keduanya menjadi "kosong" akan membuat kegagalan pembacaan terbaca sebagai fakta klinis.
    ///
    /// <para><b>Tidak dipersistensi.</b> Hanya hidup di dalam balasan endpoint usulan.</para>
    /// </remarks>
    public enum PrefillSourceStatus
    {
        /// <summary>Sumbernya terbaca dan ada isinya.</summary>
        Available = 1,

        /// <summary>Sumbernya terbaca, tetapi memang tidak ada isinya untuk episode ini.</summary>
        Empty = 2,

        /// <summary>
        /// Sumbernya gagal dibaca atau belum tersedia pada repository ini. Isian dikembalikan
        /// kosong beserta keterangannya, dan usulan bagian lain tetap disusun — roadmap
        /// <c>BE-RWI-086</c> acceptance criteria 3.
        /// </summary>
        Unavailable = 3
    }
}
