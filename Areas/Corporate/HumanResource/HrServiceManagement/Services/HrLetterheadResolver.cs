using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    /// <summary>Identitas rumah sakit untuk kop dan penerbit surat kepegawaian.</summary>
    public sealed record HrLetterhead(string Name, string Address, string Contact, string City);

    /// <summary>
    /// BE-HRD-381/384 (20 November 2026): kop surat dan alamat verifikasi diambil dari pengaturan, bukan teks tertanam.
    /// Sumber identitas: Master Lokasi Rumah Sakit (lokasi utama aktif, atau lokasi aktif pertama; nama badan hukum bila
    /// nama lokasi kosong).
    /// </summary>
    public static class HrLetterheadResolver
    {
        /// <summary>Teks pengganti bila identitas RS belum diatur; sengaja generik, bukan nama RS tertentu.</summary>
        public const string GenericHospitalLabel = "rumah sakit penerbit";

        public const string MissingIdentityMessage =
            "Identitas rumah sakit belum diatur. Lengkapi Master Lokasi Rumah Sakit (nama, alamat, kontak) sebelum menerbitkan surat.";

        /// <summary>Identitas RS dari Master Lokasi; <c>null</c> bila belum ada lokasi aktif bernama.</summary>
        public static async Task<HrLetterhead?> TryResolveAsync(ApplicationDbContext dbContext, CancellationToken cancellationToken)
        {
            var site = await dbContext.MstHospitalSites
                .AsNoTracking()
                .Where(s => s.IsActive && !s.IsDelete)
                .OrderByDescending(s => s.IsMainSite)
                .ThenBy(s => s.SiteName)
                .Select(s => new
                {
                    s.SiteName,
                    s.Address,
                    s.PhoneNumber,
                    s.Email,
                    CityName = s.City != null ? s.City.CityName : null,
                    LegalEntityName = s.LegalEntity != null ? s.LegalEntity.LegalEntityName : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (site == null)
            {
                return null;
            }

            var name = !string.IsNullOrWhiteSpace(site.SiteName) ? site.SiteName.Trim() : site.LegalEntityName?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var contact = string.Join(" | ", new[]
            {
                string.IsNullOrWhiteSpace(site.PhoneNumber) ? null : $"Telp: {site.PhoneNumber.Trim()}",
                string.IsNullOrWhiteSpace(site.Email) ? null : site.Email.Trim()
            }.Where(x => x != null));

            return new HrLetterhead(
                name,
                site.Address?.Trim() ?? string.Empty,
                string.IsNullOrEmpty(contact) ? string.Empty : $"| {contact}",
                site.CityName?.Trim() ?? string.Empty);
        }

        /// <summary>
        /// Alamat verifikasi publik: <c>HumanResource:DocumentVerificationBaseUrl</c> (mis. halaman frontend) bila diatur,
        /// selain itu endpoint API publik <c>/api/v1/public/hr-documents/verify/{hash}</c> di bawah <c>FileStorage:PublicBaseUrl</c>.
        /// </summary>
        public static string BuildVerificationUrl(IConfiguration configuration, string verificationHash)
        {
            var configured = configuration["HumanResource:DocumentVerificationBaseUrl"]?.Trim();
            if (!string.IsNullOrWhiteSpace(configured))
            {
                var separator = configured.Contains('?') ? "&" : "?";
                return $"{configured}{separator}hash={Uri.EscapeDataString(verificationHash)}";
            }

            var apiBase = configuration["FileStorage:PublicBaseUrl"]?.Trim().TrimEnd('/') ?? string.Empty;
            return $"{apiBase}/api/v1/public/hr-documents/verify/{Uri.EscapeDataString(verificationHash)}";
        }
    }
}
