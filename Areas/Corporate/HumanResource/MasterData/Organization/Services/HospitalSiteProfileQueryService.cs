using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Services
{
    /// <summary>
    /// Service baca profil situs rumah sakit utama untuk kop surat — <c>BE-RWI-188</c>, kontrak
    /// <c>episode-rawat-inap</c> <c>0.11.0</c> backend 13.6 dan 13.8.4, <c>INT-RWA-12</c>
    /// (<c>RWI-DEC-247</c>, <c>264</c>, disetujui <c>RWI-DEC-266</c>).
    /// </summary>
    /// <remarks>
    /// Service ini <b>hanya membaca</b>; tabel maupun endpoint HR tidak berubah, dan
    /// <c>HospitalSiteController</c> tidak disentuh.
    ///
    /// <para>
    /// <b>Tidak pernah menebak.</b> Bila tidak ada situs utama aktif, atau ada lebih dari satu,
    /// hasilnya <see cref="HospitalSiteProfile.IsAvailable"/> = <c>false</c> beserta alasannya.
    /// Cetakan kemudian mencetak kop tanpa identitas, bukan nilai yang ditanam di program
    /// (<c>NFR-RWA-11</c>). Contoh: dua situs aktif bertanda utama setelah admin lupa mencabut tanda
    /// situs lama — kop dibiarkan kosong sampai data HR dibetulkan.
    /// </para>
    /// </remarks>
    public sealed class HospitalSiteProfileQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public HospitalSiteProfileQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>Profil situs aktif bertanda <c>IsMainSite</c>.</summary>
        public async Task<HospitalSiteProfile> GetMainSiteProfileAsync(
            CancellationToken cancellationToken = default)
        {
            var sites = await _dbContext.Set<MstHospitalSite>()
                .AsNoTracking()
                .Where(x => x.IsMainSite && x.IsActive && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.SiteCode,
                    x.SiteName,
                    x.Address,
                    DistrictName = x.District != null ? x.District.DistrictName : null,
                    CityName = x.City != null ? x.City.CityName : null,
                    ProvinceName = x.Province != null ? x.Province.ProvinceName : null,
                    PostalCode = x.PostalCode != null ? x.PostalCode.PostalCode : null,
                    x.PhoneNumber,
                    x.Email,
                    x.TimeZoneId
                })
                .Take(2)
                .ToListAsync(cancellationToken);

            if (sites.Count == 0)
            {
                return HospitalSiteProfile.Unavailable(
                    "Belum ada situs rumah sakit utama yang aktif. Tandai satu situs sebagai situs utama di master HR.");
            }

            if (sites.Count > 1)
            {
                return HospitalSiteProfile.Unavailable(
                    "Lebih dari satu situs rumah sakit utama aktif; profil tidak ditebak. Betulkan tanda situs utama di master HR.");
            }

            var site = sites[0];

            // Baris kop: alamat jalan, lalu kecamatan-kota-provinsi, lalu kode pos. Baris yang
            // sumbernya kosong tidak dicetak.
            var addressLines = new List<string>();

            if (!string.IsNullOrWhiteSpace(site.Address))
            {
                addressLines.Add(site.Address.Trim());
            }

            var regionLine = string.Join(", ", new[] { site.DistrictName, site.CityName, site.ProvinceName }
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!.Trim()));

            if (!string.IsNullOrWhiteSpace(site.PostalCode))
            {
                regionLine = string.IsNullOrWhiteSpace(regionLine)
                    ? site.PostalCode.Trim()
                    : $"{regionLine} {site.PostalCode.Trim()}";
            }

            if (!string.IsNullOrWhiteSpace(regionLine))
            {
                addressLines.Add(regionLine);
            }

            return new HospitalSiteProfile
            {
                IsAvailable = true,
                SiteId = site.Id,
                SiteCode = site.SiteCode,
                SiteName = site.SiteName,
                AddressLines = addressLines,
                PhoneNumber = string.IsNullOrWhiteSpace(site.PhoneNumber) ? null : site.PhoneNumber.Trim(),
                Email = string.IsNullOrWhiteSpace(site.Email) ? null : site.Email.Trim(),
                TimeZoneId = string.IsNullOrWhiteSpace(site.TimeZoneId) ? null : site.TimeZoneId.Trim()
            };
        }
    }

    /// <summary>Profil situs rumah sakit utama (<c>BE-RWI-188</c>). Bukan kontrak API.</summary>
    public sealed class HospitalSiteProfile
    {
        public bool IsAvailable { get; init; }

        /// <summary>Alasan bila <see cref="IsAvailable"/> = <c>false</c>.</summary>
        public string? UnavailableReason { get; init; }

        public Guid? SiteId { get; init; }

        public string? SiteCode { get; init; }

        public string? SiteName { get; init; }

        public IReadOnlyList<string> AddressLines { get; init; } = Array.Empty<string>();

        public string? PhoneNumber { get; init; }

        public string? Email { get; init; }

        /// <summary>Zona waktu situs; kosong berarti pemakai jatuh ke <c>Asia/Jakarta</c>.</summary>
        public string? TimeZoneId { get; init; }

        public static HospitalSiteProfile Unavailable(string reason)
            => new() { IsAvailable = false, UnavailableReason = reason };
    }
}
