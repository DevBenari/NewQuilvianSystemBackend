namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Peran dokter pada satu kejadian visite.
    /// </summary>
    /// <remarks>
    /// Peran ini melekat pada <b>kejadiannya</b>, bukan pada dokternya. Seorang DPJP yang
    /// mendatangi pasien lain sebagai konsulen tetap tercatat sebagai konsulen pada kejadian
    /// itu, dan kejadian itulah yang kelak dibaca Billing maupun laporan kunjungan dokter.
    /// </remarks>
    public enum PhysicianVisitRole
    {
        /// <summary>Dokter penanggung jawab pelayanan pasien tersebut.</summary>
        Dpjp = 0,

        /// <summary>Dokter konsulen yang diminta pendapatnya.</summary>
        Consultant = 1,

        /// <summary>Dokter jaga yang mendatangi pasien di luar jam DPJP.</summary>
        OnCall = 2
    }
}
