namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    /// <summary>
    /// Jenis dosis satu butir resep — <c>BE-RWI-099</c>, migration <c>R4</c>, kamus data
    /// <c>0.5</c> bagian 13.1.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Bawaannya <c>Fixed</c> supaya seluruh butir resep yang sudah ada terbaca sebagai dosis tetap
    /// dan tidak perlu disentuh.
    /// </para>
    /// <para>
    /// <c>SlidingScale</c> dipakai butir insulin yang dosisnya ditentukan protokol sliding scale.
    /// Butir seperti itu wajib punya tepat satu <c>PhmSlidingScaleOrder</c> — penegakannya pada
    /// <c>BE-RWI-103</c>.
    /// </para>
    /// </remarks>
    public enum PrescriptionDoseKind
    {
        /// <summary>Dosis tetap sesuai isian butir resep. Bawaan bagi seluruh baris lama.</summary>
        Fixed = 0,

        /// <summary>Dosis mengikuti protokol sliding scale per pasien.</summary>
        SlidingScale = 1
    }
}
