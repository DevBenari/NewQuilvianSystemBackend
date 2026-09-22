namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Satuan gula darah — arsitektur backend 0.5 bagian 11.5.11, gate <c>G-25</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Enum ini dirancang sub-modul <c>keperawatan</c> untuk hasil gula darah Pengawasan Harian
    /// (<c>RWI-DEC-148</c>) dan dipakai <c>dokter-rawat-inap</c> sebagai satuan versi protokol sliding
    /// scale. Dibuat lebih dulu oleh <c>BE-RWI-102</c> karena kolom
    /// <c>PhmSlidingScaleTemplateVersion.GlucoseUnit</c> membutuhkannya; nilainya persis rancangan
    /// arsitektur sehingga task keperawatan memakainya apa adanya.
    /// </para>
    /// <para>
    /// Tidak berbawaan: 180 mg/dL dan 180 mmol/L adalah keadaan klinis yang sangat berbeda, sehingga
    /// satuan wajib dipilih eksplisit.
    /// </para>
    /// </remarks>
    public enum BloodGlucoseUnit
    {
        /// <summary>Miligram per desiliter.</summary>
        MgPerDl = 1,

        /// <summary>Milimol per liter.</summary>
        MmolPerL = 2
    }
}
