namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Jenis pengkajian pasien pada <c>TrxPatientAssessment</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Satu enum dipakai dua profesi.</b> Empat nilai pertama adalah pengkajian keperawatan
    /// dan diminta sub-modul <c>keperawatan</c>; dua nilai terakhir adalah kajian medis dan
    /// diminta sub-modul <c>dokter-rawat-inap</c>. Enum ini dibuat <b>sekali</b> — siapa pun yang
    /// mendarat lebih dulu membuatnya, yang kedua menambah nilainya, sesuai <c>INT-DOK-09</c>.
    /// Berkas ini lahir dari <c>BE-RWI-040</c> karena saat itu enumnya memang belum ada.
    /// </para>
    /// <para>
    /// <b>Kenapa kajian medis tidak punya tabel sendiri.</b> <c>TrxPatientAssessment</c> sudah
    /// memuat keluhan utama, riwayat, alergi, tanda vital, kesadaran, dan pemeriksaan umum, dan
    /// sudah memiliki kolom <c>DoctorId</c> — ia memang tidak pernah menjadi tabel milik perawat
    /// saja. Tabel tersendiri berarti menyalin puluhan kolom yang sama. Pembedaan antara kajian
    /// medis dan pengkajian keperawatan karena itu dijaga aturan bisnis lewat nilai enum ini,
    /// bukan oleh mesin hak akses yang hanya melihat satu sumber daya.
    /// </para>
    /// <para>
    /// <b>Nilai lama tidak boleh berubah.</b> Nilainya dipersistensi sebagai integer, sehingga
    /// menggeser angka berarti menulis ulang arti baris yang sudah ada. Nilai baru ditambahkan di
    /// bawah.
    /// </para>
    /// </remarks>
    public enum PatientAssessmentType
    {
        /// <summary>Pengkajian awal keperawatan. Bawaan bagi seluruh baris yang sudah ada.</summary>
        Initial = 0,

        /// <summary>Pengkajian ulang keperawatan.</summary>
        Reassessment = 1,

        /// <summary>Pengkajian ulang harian keperawatan.</summary>
        DailyReassessment = 2,

        /// <summary>Pengkajian rencana pemulangan.</summary>
        DischargePlanning = 3,

        /// <summary>Kajian medis awal oleh DPJP — <c>CAP-022</c>, <c>AC-CAP022-02</c>.</summary>
        MedicalInitial = 4,

        /// <summary>Kajian medis ulang oleh dokter.</summary>
        MedicalReassessment = 5
    }
}
