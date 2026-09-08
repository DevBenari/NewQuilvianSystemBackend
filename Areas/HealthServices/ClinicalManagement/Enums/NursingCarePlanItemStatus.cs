namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Keadaan satu butir masalah keperawatan pada rencana asuhan —
    /// <c>BE-RWI-059</c>, <c>state-transition-matrix.md</c> bagian 2.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tepat tiga nilai, dan tidak ada nilai untuk "terhapus". <c>CAP-013</c> aturan 6 melarang
    /// penutupan butir menghapus tindakan maupun evaluasi yang sudah dikerjakan, sehingga butir
    /// yang selesai tetap tersimpan beserta seluruh jejaknya.
    /// </para>
    /// <para>
    /// <b>Bedanya <c>Resolved</c> dan <c>Discontinued</c> menentukan.</b> <c>Resolved</c> berarti
    /// masalah pasien benar-benar teratasi, dan karena itu menuntut sekurang-kurangnya satu
    /// catatan evaluasi sebagai dasarnya — <c>VAL-KEP-16</c>. <c>Discontinued</c> berarti masalah
    /// itu tidak lagi relevan, misalnya karena rencana asuhannya diganti, dan menuntut alasan.
    /// Menyamakan keduanya membuat rekam medis tidak dapat membedakan pasien yang membaik dari
    /// rencana yang dibatalkan.
    /// </para>
    /// <para>
    /// <b>Tidak ada nilai untuk "sudah dikoreksi".</b> Perubahan rencana asuhan adalah
    /// perkembangan klinis, bukan pembetulan kesalahan, sehingga ia memakai mesin versi butir —
    /// <c>RWI-DEC-091</c> — bukan mesin addendum milik <c>MedicalRecordManagement</c>.
    /// </para>
    /// </remarks>
    public enum NursingCarePlanItemStatus
    {
        /// <summary>Masalah keperawatan masih dikerjakan.</summary>
        Active = 0,

        /// <summary>Masalah dinyatakan teratasi; menuntut sekurang-kurangnya satu evaluasi.</summary>
        Resolved = 1,

        /// <summary>Masalah dihentikan karena tidak lagi relevan; menuntut alasan.</summary>
        Discontinued = 2
    }
}
