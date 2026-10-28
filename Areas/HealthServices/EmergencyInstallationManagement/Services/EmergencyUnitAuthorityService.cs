using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Memeriksa apakah seorang petugas berwenang atas sebuah unit pelayanan atau tindakan klinis IGD.
    /// </summary>
    /// <remarks>
    /// Validation bagian 7, keputusan <c>IGD-DEC-086</c>, <c>IGD-DEC-092</c>, dan <c>IGD-DEC-372</c>.
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

        /// <summary>
        /// Memeriksa apakah pengguna saat ini adalah SuperAdmin.
        /// </summary>
        public bool IsSuperAdmin(ClaimsPrincipal user)
        {
            if (user.IsInRole("SuperAdmin"))
                return true;

            var roleClaims = user.FindAll(ClaimTypes.Role).Select(x => x.Value);
            if (roleClaims.Any(x => x.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
                return true;

            var userTypeClaim = user.FindFirstValue("user_type") ?? user.FindFirstValue("UserType");
            return !string.IsNullOrWhiteSpace(userTypeClaim) &&
                   (userTypeClaim == "1" || userTypeClaim.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Menyelesaikan DoctorId pengguna saat ini bila akunnya terhubung ke master dokter.
        /// </summary>
        public async Task<Guid?> ResolveDoctorIdAsync(
            ClaimsPrincipal user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var doctorIdClaim = user.FindFirstValue("doctor_id") ?? user.FindFirstValue("DoctorId");
            if (Guid.TryParse(doctorIdClaim, out var doctorId) && doctorId != Guid.Empty)
                return doctorId;

            var workforceClaim = user.FindFirstValue("workforce_profile_id") ?? user.FindFirstValue("WorkforceProfileId");
            if (Guid.TryParse(workforceClaim, out var workforceProfileId) && workforceProfileId != Guid.Empty)
            {
                var doctorByWorkforce = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive && x.WorkforceProfileId == workforceProfileId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (doctorByWorkforce != Guid.Empty)
                    return doctorByWorkforce;
            }

            if (actorUserId != Guid.Empty)
            {
                var currentUser = await _dbContext.Users.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == actorUserId, cancellationToken);
                if (currentUser?.Email != null)
                {
                    var email = currentUser.Email.ToLower();
                    var doctorByEmail = await _dbContext.Set<MstDoctor>()
                        .AsNoTracking()
                        .Where(x => !x.IsDelete && x.IsActive && x.Email.ToLower() == email)
                        .Select(x => x.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (doctorByEmail != Guid.Empty)
                        return doctorByEmail;
                }
            }

            return null;
        }

        /// <summary>
        /// Memeriksa apakah pengguna berwenang melakukan tindakan pemeriksaan / asuhan keperawatan IGD.
        /// IGD-DEC-372: Hanya SuperAdmin atau staf yang bertugas di Instalasi Gawat Darurat yang diizinkan.
        /// </summary>
        public async Task<Hasil> PeriksaWewenangKlinisIgdAsync(
            ClaimsPrincipal user,
            Guid actorUserId,
            DateTime now,
            string tindakan,
            CancellationToken cancellationToken = default)
        {
            if (IsSuperAdmin(user))
                return new Hasil(true, false, null);

            if (actorUserId == Guid.Empty)
                return new Hasil(false, false, "Identitas pengguna tidak valid.");

            // Periksa penempatan organisasi aktif pengguna di IGD
            var berwenang = await _dbContext.Set<ApplicationUserOrganization>()
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Position)
                .AnyAsync(
                    x => x.UserId == actorUserId
                        && x.IsActive
                        && !x.IsDelete
                        && (x.EffectiveStartDate == null || x.EffectiveStartDate <= now)
                        && (x.EffectiveEndDate == null || x.EffectiveEndDate >= now)
                        && (
                            (x.Department != null && (
                                EF.Functions.ILike(x.Department.DepartmentName, "%Gawat Darurat%") ||
                                EF.Functions.ILike(x.Department.DepartmentName, "%IGD%")
                            ))
                            ||
                            (x.Position != null && (
                                EF.Functions.ILike(x.Position.PositionName, "%IGD%") ||
                                EF.Functions.ILike(x.Position.PositionName, "%Gawat Darurat%")
                            ))
                            ||
                            (x.SourceAssignmentId != null && _dbContext.Set<WfpOrganizationAssignment>().Any(
                                penugasan => penugasan.Id == x.SourceAssignmentId &&
                                    _dbContext.Set<MstServiceUnit>().Any(
                                        u => u.OrganizationUnitId == penugasan.OrganizationUnitId &&
                                             u.ServiceUnitType == ServiceUnitType.Emergency
                                    )
                            ))
                        ),
                    cancellationToken);

            if (berwenang)
                return new Hasil(true, false, null);

            return new Hasil(
                false,
                false,
                $"Pemeriksaan dan asuhan keperawatan IGD hanya dapat dilakukan oleh staf atau perawat yang bertugas di Instalasi Gawat Darurat (tindakan: {tindakan}).");
        }

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
                    $"kewenangan {tindakan} belum dapat diperiksa sistem. Minta Master Data " +
                    "melengkapi pemetaan unit ini.");
            }

            var berwenang = await _dbContext.Set<ApplicationUserOrganization>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.UserId == userId
                        && x.SourceAssignmentId != null
                        && x.IsActive
                        && !x.IsDelete
                        && (x.EffectiveStartDate == null || x.EffectiveStartDate <= now)
                        // Validation bagian 7 aturan 2 - penugasan yang sudah lewat tidak
                        // memberi kewenangan.
                        && (x.EffectiveEndDate == null || x.EffectiveEndDate >= now)
                        && _dbContext.Set<WfpOrganizationAssignment>().Any(
                            penugasan => penugasan.Id == x.SourceAssignmentId
                                && penugasan.OrganizationUnitId == unit.OrganizationUnitId.Value),
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
