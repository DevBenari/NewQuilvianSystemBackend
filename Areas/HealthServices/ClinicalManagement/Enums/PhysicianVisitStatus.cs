namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Keadaan satu kejadian visite dokter.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Tepat dua nilai, dan itu disengaja. Kejadian visite tidak punya alur persetujuan: ia
    /// tercatat, atau ia dibatalkan beserta alasannya. Kejadian yang dibatalkan <b>tetap
    /// tersimpan</b> dan tetap tampil pada riwayat — <c>INV-DOK-08</c> — sehingga auditor dapat
    /// melihat bahwa pernah ada catatan yang dibatalkan.
    /// </para>
    /// <para>
    /// Tidak ada nilai untuk "sudah dikoreksi". Koreksi dilakukan dengan membatalkan kejadian
    /// lama lalu mencatat kejadian baru yang menunjuk kejadian yang digantikannya lewat
    /// <c>CorrectsVisitId</c>. Menyunting waktu maupun peran di tempat dilarang
    /// <c>RWI-DEC-085</c>.
    /// </para>
    /// </remarks>
    public enum PhysicianVisitStatus
    {
        /// <summary>Kejadian tercatat dan ikut dihitung.</summary>
        Recorded = 0,

        /// <summary>Kejadian dibatalkan beserta alasannya; tetap tersimpan, tidak dihitung.</summary>
        Cancelled = 1
    }
}
