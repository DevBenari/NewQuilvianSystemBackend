namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    /// <summary>
    /// Status order sliding scale per pasien — <c>BE-RWI-103</c>, state matrix 0.6.0 bagian 8.7.
    /// </summary>
    /// <remarks>
    /// <c>Stopped</c> terminal: order yang dihentikan tidak dapat disesuaikan maupun dihidupkan
    /// kembali; bila masih dibutuhkan, dokter memesan order baru.
    /// </remarks>
    public enum SlidingScaleOrderStatus
    {
        Active = 1,
        Stopped = 2
    }
}
