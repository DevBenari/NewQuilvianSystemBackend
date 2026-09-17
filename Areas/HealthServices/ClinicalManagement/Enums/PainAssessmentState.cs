namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums
{
    /// <summary>
    /// Keadaan penilaian nyeri pada dokumen pengkajian — <c>BE-RWI-110</c>, <c>BE-RWI-111</c>, arsitektur 0.4 bagian 11.6.
    /// </summary>
    /// <remarks>
    /// <c>HasPain</c> yang bertipe <c>bool</c> tidak dapat membedakan "tidak nyeri" dari "belum dinilai". Enum ini dapat,
    /// sehingga nyeri yang belum dikaji tidak pernah terbaca sebagai tidak nyeri (<c>BR-RWI-007</c>). Nilai lama tidak boleh bergeser.
    /// </remarks>
    public enum PainAssessmentState
    {
        /// <summary>Belum dinilai. Bawaan seluruh baris lama.</summary>
        NotAssessed = 0,

        /// <summary>Dinilai, pasien tidak nyeri.</summary>
        NoPain = 1,

        /// <summary>Dinilai, pasien nyeri.</summary>
        HasPain = 2,

        /// <summary>Tidak dapat dinilai, misalnya pasien tidak sadar.</summary>
        UnableToAssess = 3
    }
}
