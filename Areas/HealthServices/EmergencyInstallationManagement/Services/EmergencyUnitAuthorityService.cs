using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Memeriksa apakah seorang petugas berwenang atas sebuah unit pelayanan.
    /// </summary>
    /// <remarks>
    /// Validation bagian 7, keputusan <c>IGD-DEC-086</c> dan <c>IGD-DEC-092</c>.
    ///
    /// <para>
    /// <b>Latar yang wajib diketahui pembaca.</b> Sampai <c>BE-IGD-010</c>, tidak ada satu pun
    /// jalur data dari pengguna ke unit pelayanan: <c>MstServiceUnit</c> tidak mengenal
    /// organisasi, dan <c>ApplicationUserOrganization</c> hanya mengenal departemen. Kolom
    /// <c>MstServiceUnit.OrganizationUnitId</c> ditambahkan justru untuk menutup jurang itu.
    /// </para>
    ///
    /// <para>
    /// <b>Kolom itu masih kosong untuk hampir seluruh unit.</b> Pengisiannya milik Master Data
    /// bersama Corporate/HR dan menulis ke basis data yang dipakai satu tim, sehingga belum
    /// dapat dijalankan dari sini. <c>IGD-DEC-092</c> menetapkan perlakuannya sebagai
    /// <b>keputusan sementara</b>: unit yang belum dipetakan bersifat <i>fail-closed</i> —
    /// ditolak — dengan <b>jalan keluar beralasan</b> yang tercatat, bukan diizinkan diam-diam.
    /// </para>
    ///
    /// <para>
    /// Fail-open akan menghapus penjagaan sama sekali dan membuat layar seolah terjaga padahal
    /// tidak. Fail-closed tanpa jalan keluar akan menghentikan pelayanan. Yang dipilih adalah
    /// menolak secara bawaan, lalu memberi jalan keluar yang meninggalkan jejak — sehingga
    /// setiap kali penjagaan ditembus, ada barisnya untuk ditinjau.
    /// </para>
    ///
    /// <para>
    /// <c>IGD-DEC-086</c> butir 7 tetap berlaku dan tidak boleh dilanggar: <b>pelayanan klinis
    /// darurat tidak pernah diblokir</b> ketiadaan penugasan. Penjagaan ini karena itu hanya
    /// dipasang pada tindakan administratif — mencatat kedatangan, menerima serah terima,
    /// menerima atau menolak pesanan — bukan pada tindakan klinis.
    /// </para>
    /// </remarks>
    public class EmergencyUnitAuthorityService
    {
        private readonly ApplicationDbContext _dbContext;

        public EmergencyUnitAuthorityService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>Hasil pemeriksaan kewenangan.</summary>
        public sealed record Hasil(bool Berwenang, bool UnitBelumDipetakan, string? Penolakan);

        public async Task<Hasil> PeriksaAsync(
            Guid userId,
            Guid serviceUnitId,
            DateTime now,
            string tindakan,
            CancellationToken cancellationToken = default)
        {
            var unit = await _dbContext.Set<MstServiceUnit>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == serviceUnitId && !x.IsDelete, cancellationToken);

            if (unit == null)
                return new Hasil(false, false, "Unit tujuan tidak ditemukan.");

            // IGD-DEC-092 - unit yang belum dipetakan ke simpul organisasi. Ditolak secara
            // bawaan, dengan jalan keluar beralasan yang dipanggil terpisah oleh controller.
            if (!unit.OrganizationUnitId.HasValue || unit.OrganizationUnitId.Value == Guid.Empty)
            {
                return new Hasil(
                    false,
                    true,
                    $"Unit {unit.ServiceUnitName} belum dipetakan ke simpul organisasi, sehingga " +
                    $"kewenangan {tindakan} belum dapat diperiksa sistem. Lanjutkan dengan " +
                    "menyertakan alasan, atau minta Master Data melengkapi pemetaan unit ini.");
            }

            var berwenang = await _dbContext.Set<ApplicationUserOrganization>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.UserId == userId
                        && x.DepartmentId == unit.OrganizationUnitId.Value
                        && x.IsActive
                        && !x.IsDelete
                        && (x.EffectiveStartDate == null || x.EffectiveStartDate <= now)
                        // Validation bagian 7 aturan 2 - penugasan yang sudah lewat tidak
                        // memberi kewenangan.
                        && (x.EffectiveEndDate == null || x.EffectiveEndDate >= now),
                    cancellationToken);

            if (berwenang)
                return new Hasil(true, false, null);

            return new Hasil(
                false,
                false,
                $"Anda tidak bertugas di unit {unit.ServiceUnitName}, sehingga tidak dapat " +
                $"{tindakan}.");
        }
    }
}
