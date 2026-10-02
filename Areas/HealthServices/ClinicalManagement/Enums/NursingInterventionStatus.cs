namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Keadaan satu catatan tindakan keperawatan — <c>BE-RWI-061</c>,
    /// <c>state-transition-matrix.md</c> bagian 3.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tepat dua nilai. <b>Tidak ada nilai <c>Amended</c>,</b> dan itu bukan kelalaian: kontrak
    /// <c>0.3.0</c> mencabutnya lewat <c>RWI-DEC-091</c>. Catatan yang dikoreksi <b>tetap</b>
    /// <c>Finalized</c>, dan koreksinya tersimpan sebagai addendum bernomor pada mesin keutuhan
    /// milik <c>MedicalRecordManagement</c>. Menambahkan nilai itu kembali akan melahirkan dua
    /// sumber jawaban atas pertanyaan "apakah catatan ini pernah dikoreksi"; yang berlaku adalah
    /// riwayat addendum.
    /// </para>
    /// <para>
    /// <b>Keadaan ini terpisah dari keadaan pengiriman tagihan.</b> Kegagalan sistem tagihan
    /// tidak pernah mengubah nilai di sini — lihat <see cref="NursingBillingDispatchStatus"/>.
    /// </para>
    /// </remarks>
    public enum NursingInterventionStatus
    {
        /// <summary>Tindakan tercatat dan masih dapat disunting penulisnya.</summary>
        Recorded = 0,

        /// <summary>Catatan dinyatakan final, tertanda tangan, dan hanya dapat dikoreksi lewat addendum.</summary>
        Finalized = 1
    }
}
