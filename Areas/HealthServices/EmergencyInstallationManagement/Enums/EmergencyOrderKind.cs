namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums
{
    /// <summary>
    /// Jenis pesanan yang belum selesai saat pasien pergi dari IGD.
    /// </summary>
    /// <remarks>
    /// <c>BE-IGD-035</c>. <c>RadiologyOrder</c> ditambahkan mengikuti <c>IGD-DEC-099</c>:
    /// pemesanan radiologi adalah kebutuhan klinis IGD meski modul Radiologi belum ada,
    /// sehingga pesanannya dibuat di luar sistem dan dicatat sebagai pesanan <c>External</c>.
    /// </remarks>
    public enum EmergencyOrderKind
    {
        Medication = 1,
        Procedure = 2,
        LaboratoryOrder = 3,
        RadiologyOrder = 4
    }
}
