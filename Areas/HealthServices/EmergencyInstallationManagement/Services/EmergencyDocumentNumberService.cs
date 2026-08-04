namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Membentuk nomor dokumen operasional IGD. Tidak menggunakan interface sesuai pola proyek.
    /// </summary>
    public class EmergencyDocumentNumberService
    {
        public string Generate(string prefix, DateTime now)
        {
            var normalizedPrefix = string.IsNullOrWhiteSpace(prefix)
                ? "IGD"
                : prefix.Trim().ToUpperInvariant();

            return $"{normalizedPrefix}-{now:yyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
        }
    }
}
