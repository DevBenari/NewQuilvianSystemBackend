using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services
{
    /// <summary>
    /// Mengelola penetapan penulis catatan sebagai berhalangan (RM-DEC-020).
    ///
    /// Hanya menangani penetapan manual oleh kepala unit. Penetapan karena akun nonaktif tidak
    /// pernah dibuat lewat service ini — sistem menyimpulkannya sendiri saat kewenangan
    /// addendum dinilai.
    /// </summary>
    public class ClinicalNoteAuthorDelegationService
    {
        private readonly ApplicationDbContext _dbContext;

        public ClinicalNoteAuthorDelegationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Menetapkan seorang penulis sebagai berhalangan.
        ///
        /// Menyimpan sendiri, karena hanya menyisipkan satu baris penetapan.
        /// </summary>
        public async Task<(IntegrityGuardResult Result, MrcClinicalNoteAuthorDelegation? Delegation)> CreateAsync(
            Guid originalAuthorUserId,
            Guid grantedByUserId,
            string grantReason,
            DateTime validUntilUtc,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            if (originalAuthorUserId == Guid.Empty)
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status400BadRequest,
                    "Penulis yang berhalangan wajib dipilih."), null);
            }

            if (string.IsNullOrWhiteSpace(grantReason))
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status400BadRequest,
                    "Alasan penetapan wajib diisi."), null);
            }

            // Menetapkan diri sendiri berhalangan tampak tidak masuk akal, tetapi bila
            // diizinkan akan menjadi cara memindahkan tanggung jawab atas catatan sendiri
            // kepada orang lain. Menutupnya sejak awal lebih murah daripada menjelaskannya
            // kemudian.
            if (originalAuthorUserId == grantedByUserId)
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status400BadRequest,
                    "Anda tidak dapat menetapkan diri sendiri berhalangan."), null);
            }

            if (validUntilUtc <= nowUtc)
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status400BadRequest,
                    "Batas waktu penetapan harus setelah hari ini."), null);
            }

            var penulis = await _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == originalAuthorUserId, cancellationToken);

            if (penulis == null)
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status404NotFound,
                    "Penulis yang dipilih tidak ditemukan."), null);
            }

            // Akun yang sudah nonaktif tidak perlu penetapan — jalurnya sudah terbuka otomatis.
            // Membiarkan penetapan dibuat justru menyesatkan, karena kepala unit akan mengira
            // kewenangan itu berasal dari penetapannya.
            if (!penulis.IsActive)
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status400BadRequest,
                    "Akun penulis sudah nonaktif, sehingga kewenangan pengganti terbuka " +
                    "otomatis tanpa penetapan."), null);
            }

            var sudahAda = await _dbContext.Set<MrcClinicalNoteAuthorDelegation>()
                .AsNoTracking()
                .AnyAsync(x => x.OriginalAuthorUserId == originalAuthorUserId
                               && x.IsActive
                               && !x.IsDelete
                               && x.RevokedAt == null
                               && (x.ValidUntil == null || x.ValidUntil >= nowUtc),
                          cancellationToken);

            if (sudahAda)
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status409Conflict,
                    "Penulis ini sudah memiliki penetapan yang masih berlaku."), null);
            }

            var penetapan = new MrcClinicalNoteAuthorDelegation
            {
                OriginalAuthorUserId = originalAuthorUserId,
                Trigger = AuthorDelegationTrigger.UnitHeadGrant,
                GrantedByUserId = grantedByUserId,
                GrantReason = grantReason.Trim(),
                ValidFrom = nowUtc,
                ValidUntil = validUntilUtc,
                IsActive = true,
                CreateDateTime = nowUtc,
                CreateBy = grantedByUserId
            };

            await _dbContext.Set<MrcClinicalNoteAuthorDelegation>()
                .AddAsync(penetapan, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (IntegrityGuardResult.Allowed(), penetapan);
        }

        /// <summary>
        /// Mencabut penetapan lebih awal, sebelum batas waktunya berakhir.
        /// </summary>
        public async Task<(IntegrityGuardResult Result, MrcClinicalNoteAuthorDelegation? Delegation)> RevokeAsync(
            Guid delegationId,
            Guid revokedByUserId,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            var penetapan = await _dbContext.Set<MrcClinicalNoteAuthorDelegation>()
                .FirstOrDefaultAsync(x => x.Id == delegationId && !x.IsDelete, cancellationToken);

            if (penetapan == null)
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status404NotFound, "Penetapan tidak ditemukan."), null);
            }

            if (penetapan.RevokedAt.HasValue || !penetapan.IsActive)
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status400BadRequest, "Penetapan ini sudah dicabut."), null);
            }

            penetapan.RevokedAt = nowUtc;
            penetapan.RevokedByUserId = revokedByUserId;
            penetapan.IsActive = false;
            penetapan.UpdateDateTime = nowUtc;
            penetapan.UpdateBy = revokedByUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (IntegrityGuardResult.Allowed(), penetapan);
        }
    }
}
