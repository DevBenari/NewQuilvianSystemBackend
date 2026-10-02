namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums
{
    /// <summary>
    /// Status versi protokol sliding scale — <c>BE-RWI-102</c>, state matrix 0.6.0 bagian 8.6.
    /// </summary>
    /// <remarks>
    /// <c>Draft</c> dapat diubah dan tidak dapat dipesan. <c>Approved</c> tepat satu per template dan
    /// satu-satunya yang dapat dipesan. <c>Retired</c> terminal — order yang sudah memakainya tidak
    /// terpengaruh karena rentangnya sudah tersalin ke order.
    /// </remarks>
    public enum SlidingScaleVersionStatus
    {
        Draft = 1,
        Approved = 2,
        Retired = 3
    }
}
