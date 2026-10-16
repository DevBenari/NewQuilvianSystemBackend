namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;

/// <summary>
/// Tingkat ketergantungan pasien (RWI-DEC-302). Tanpa nilai bawaan: <c>null</c> pada episode berarti
/// belum dinilai, bukan Low. Hanya dicatat; tidak memengaruhi kelayakan bed, kategori transfer,
/// maupun tarif (INV-DNH-05).
/// </summary>
public enum InpCareDependencyLevel
{
    High = 1,
    Low = 2
}
