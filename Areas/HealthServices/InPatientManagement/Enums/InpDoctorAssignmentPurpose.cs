namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums
{
    /// <summary>
    /// Alasan seorang dokter ditugaskan pada satu baris penugasan episode rawat inap. Lahir
    /// <c>BE-RWI-079</c> menyerap <c>RWI-DEC-130</c>.
    /// </summary>
    /// <remarks>
    /// <b>Tujuan berbeda dari peran.</b> <see cref="InpDoctorAssignmentRole"/> menjawab
    /// "kewenangan apa yang dibawa baris ini"; enum ini menjawab "kenapa baris ini dibuat".
    /// Satu dokter jaga dapat ditugaskan untuk menjaga shift — tujuan <see cref="Regular"/> —
    /// atau hanya untuk menyelesaikan catatan yang tertinggal — tujuan
    /// <see cref="LateDocumentation"/>. Keduanya berperan sama, tetapi yang kedua tidak boleh
    /// ikut terhitung pada laporan jumlah jaga (<c>RWI-DEC-130</c> konsekuensi (1)).
    ///
    /// <para>
    /// <b>Nilai <c>0</c> sengaja dipakai di sini,</b> berbeda dari
    /// <see cref="InpDoctorAssignmentRole"/> yang memulai deretnya dari <c>1</c>. Alasannya
    /// terbalik: seluruh baris lama memang <b>benar-benar</b> penugasan biasa, sehingga nilai
    /// bawaan database <c>0</c> bukan "belum ditetapkan" melainkan jawaban yang tepat —
    /// <c>data-dictionary.md</c> bagian 18.1.
    /// </para>
    ///
    /// <para>
    /// <b>Enum, bukan boolean.</b> Kolom <c>IsLateDocumentation</c> akan menuntut kolom kedua
    /// begitu tujuan ketiga muncul, dan sejak saat itu dua boolean dapat menyala bersamaan
    /// tanpa ada yang melarang — <c>02-backend-architecture.md</c> revision <c>0.8</c> bagian
    /// 11.10.
    /// </para>
    /// </remarks>
    public enum InpDoctorAssignmentPurpose
    {
        /// <summary>
        /// Penugasan biasa. Bawaan bagi seluruh baris yang sudah ada, dan bagi DPJP, konsulen,
        /// serta dokter jaga yang dilibatkan untuk merawat pasien.
        /// </summary>
        Regular = 0,

        /// <summary>
        /// Penugasan singkat yang dibuat kepala ruangan atau supervisor semata-mata agar
        /// seorang dokter dapat menyelesaikan catatan klinis yang tertinggal.
        /// </summary>
        /// <remarks>
        /// Tiga syarat melekat padanya dan ditegakkan di <b>dua tempat</b> — service dan check
        /// constraint <c>CK_InpDoctorAssignment_LateDocumentation</c>: perannya wajib
        /// <see cref="InpDoctorAssignmentRole.OnCallDoctor"/>, waktu selesainya wajib terisi
        /// dan lebih besar dari waktu mulai, dan alasannya wajib terisi. Penugasan tanpa waktu
        /// selesai bukan "penugasan singkat" — ia jendela penulisan yang tidak pernah
        /// tertutup — <c>INV-INP-12</c>.
        /// </remarks>
        LateDocumentation = 1
    }
}
