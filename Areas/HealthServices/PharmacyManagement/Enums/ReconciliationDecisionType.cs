namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    /// <summary>
    /// Keputusan dokter atas satu obat bawaan pasien — <c>BE-RWI-101</c>, <c>RWI-DEC-132</c>,
    /// kamus data 0.5 bagian 13.2 dan 13.3.
    /// </summary>
    /// <remarks>
    /// <c>Pending</c> hanya dipakai sebagai <c>CurrentDecision</c> obat yang belum diputuskan. Tabel
    /// riwayat keputusan tidak pernah menyimpan nilai <c>0</c>.
    /// </remarks>
    public enum ReconciliationDecisionType
    {
        /// <summary>Belum diputuskan dokter.</summary>
        Pending = 0,

        /// <summary>Lanjut Sama — obat masuk draft resep dengan aturan pakai yang sama.</summary>
        ContinueSame = 1,

        /// <summary>Lanjut Ubah — obat masuk draft resep untuk diubah aturan pakainya.</summary>
        ContinueModified = 2,

        /// <summary>Hentikan — obat tidak dilanjutkan dan tidak membentuk resep.</summary>
        Stopped = 3
    }
}
