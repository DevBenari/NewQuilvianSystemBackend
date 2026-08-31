using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services
{
    /// <summary>
    /// Menegakkan aturan koreksi catatan klinis yang sudah terkunci (RM-DEC-004, RM-DEC-020).
    ///
    /// Addendum tidak pernah menimpa isi lama; ia menempel di bawahnya. Karena itu tidak ada
    /// metode mengubah maupun menghapus addendum di sini — koreksi atas addendum dibuat sebagai
    /// addendum berikutnya.
    ///
    /// Kewenangan pengganti tidak dibaca sendiri oleh service ini, melainkan diterima sebagai
    /// masukan <c>actorHasSubstituteAuthority</c>. Dengan begitu aturan bisnisnya tetap dapat
    /// diuji tanpa menyalakan seluruh sistem hak akses, dan sumber kewenangannya tetap satu
    /// tempat yang jelas di controller.
    /// </summary>
    public class ClinicalNoteAddendumService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ClinicalDocumentIntegrityService _integrityService;

        public ClinicalNoteAddendumService(
            ApplicationDbContext dbContext,
            ClinicalDocumentIntegrityService integrityService)
        {
            _dbContext = dbContext;
            _integrityService = integrityService;
        }

        /// <summary>
        /// Menjawab apakah seorang pengguna boleh membuat addendum pada sebuah dokumen, dan
        /// atas dasar apa.
        ///
        /// Pemeriksaannya bertingkat, mengikuti RM-DEC-004:
        /// <list type="number">
        /// <item>Pengguna adalah penulis asli — boleh.</item>
        /// <item>Akun penulis asli nonaktif dan pengguna berwenang sebagai pengganti — boleh.</item>
        /// <item>Ada penetapan berhalangan yang masih berlaku dan pengguna berwenang — boleh.</item>
        /// <item>Selain itu — ditolak.</item>
        /// </list>
        /// </summary>
        /// <param name="nowUtc">
        /// Waktu yang dipakai menilai apakah penetapan berhalangan masih berlaku. Diterima
        /// sebagai masukan, bukan dibaca sendiri, supaya aturannya dapat diuji dan supaya
        /// seluruh pemeriksaan pada satu permintaan memakai waktu yang sama.
        /// </param>
        public async Task<AddendumAuthorityResponse> ResolveAuthorityAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            Guid actorUserId,
            bool actorHasSubstituteAuthority,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            var keutuhan = await _integrityService.FindAsync(documentKind, documentId, cancellationToken);

            if (keutuhan == null)
            {
                return Tolak("Catatan tidak ditemukan pada daftar keutuhan.");
            }

            if (keutuhan.IntegrityStatus == ClinicalDocumentIntegrityStatus.Draft)
            {
                return Tolak("Catatan ini belum terkunci. Perbaiki langsung pada catatannya.");
            }

            if (keutuhan.IntegrityStatus == ClinicalDocumentIntegrityStatus.Cancelled)
            {
                return Tolak("Catatan ini sudah dibatalkan dan tidak dapat dikoreksi.");
            }

            // Tingkat 1 — penulis asli.
            if (keutuhan.AuthorUserId == actorUserId)
            {
                return new AddendumAuthorityResponse
                {
                    IsAllowed = true,
                    IsOriginalAuthor = true,
                    IsSubstituteAuthor = false,
                    Explanation = "Anda penulis catatan ini, sehingga dapat menambahkan koreksi."
                };
            }

            // Tingkat 2 dan 3 hanya terbuka bagi yang berwenang sebagai pengganti.
            if (!actorHasSubstituteAuthority)
            {
                return Tolak("Hanya penulis catatan yang dapat menambahkan koreksi.");
            }

            // Tingkat 2 — akun penulis sudah nonaktif. Disimpulkan sistem, tanpa perlu
            // penetapan manual.
            var penulisNonaktif = await _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == keutuhan.AuthorUserId && !x.IsActive, cancellationToken);

            if (penulisNonaktif)
            {
                return new AddendumAuthorityResponse
                {
                    IsAllowed = true,
                    IsOriginalAuthor = false,
                    IsSubstituteAuthor = true,
                    DelegationTrigger = AuthorDelegationTrigger.InactiveAccount,
                    Explanation = "Akun penulis catatan ini sudah nonaktif, sehingga Anda dapat " +
                                  "menambahkan koreksi atas nama Anda sendiri."
                };
            }

            // Tingkat 3 — penetapan berhalangan yang masih berlaku.
            var penetapan = await CariPenetapanBerlakuAsync(
                keutuhan.AuthorUserId, nowUtc, cancellationToken);

            if (penetapan != null)
            {
                return new AddendumAuthorityResponse
                {
                    IsAllowed = true,
                    IsOriginalAuthor = false,
                    IsSubstituteAuthor = true,
                    DelegationId = penetapan.Id,
                    DelegationTrigger = penetapan.Trigger,
                    Explanation = "Penulis catatan ini dinyatakan berhalangan, sehingga Anda dapat " +
                                  "menambahkan koreksi atas nama Anda sendiri."
                };
            }

            // Bila pernah ada penetapan tetapi sudah lewat, pesannya dibedakan supaya pengguna
            // tahu harus meminta perpanjangan, bukan mengira dirinya memang tidak berhak.
            var pernahAdaPenetapan = await _dbContext.Set<TrxClinicalNoteAuthorDelegation>()
                .AsNoTracking()
                .AnyAsync(x => x.OriginalAuthorUserId == keutuhan.AuthorUserId && !x.IsDelete,
                          cancellationToken);

            return Tolak(pernahAdaPenetapan
                ? "Penetapan kewenangan pengganti sudah berakhir. Hubungi kepala unit."
                : "Hanya penulis catatan yang dapat menambahkan koreksi.");
        }

        /// <summary>
        /// Membuat satu addendum pada dokumen yang sudah terkunci.
        ///
        /// Menyimpan sendiri, karena hanya menyisipkan satu addendum dan memperbarui penghitung
        /// pada baris keutuhan. Isi dokumen induk tidak tersentuh — itulah inti addendum.
        /// </summary>
        public async Task<(IntegrityGuardResult Result, TrxClinicalNoteAddendum? Addendum)> CreateAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            Guid actorUserId,
            bool actorHasSubstituteAuthority,
            string addendumText,
            string correctionReason,
            string? deviceInfo,
            string? ipAddress,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(addendumText))
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status400BadRequest, "Isi koreksi wajib diisi."), null);
            }

            if (string.IsNullOrWhiteSpace(correctionReason))
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status400BadRequest, "Alasan koreksi wajib diisi."), null);
            }

            var kewenangan = await ResolveAuthorityAsync(
                documentKind, documentId, actorUserId, actorHasSubstituteAuthority,
                nowUtc, cancellationToken);

            if (!kewenangan.IsAllowed)
            {
                // Dokumen yang belum terkunci atau sudah dibatalkan adalah kesalahan permintaan,
                // bukan kekurangan wewenang. Bedanya penting bagi pengguna.
                var kode = kewenangan.Explanation.Contains("belum terkunci")
                           || kewenangan.Explanation.Contains("dibatalkan")
                           || kewenangan.Explanation.Contains("tidak ditemukan")
                    ? StatusCodes.Status400BadRequest
                    : StatusCodes.Status403Forbidden;

                return (IntegrityGuardResult.Denied(kode, kewenangan.Explanation), null);
            }

            var keutuhan = await _dbContext.Set<TrxClinicalDocumentIntegrity>()
                .FirstAsync(x => x.DocumentKind == documentKind
                                 && x.DocumentId == documentId
                                 && !x.IsDelete, cancellationToken);

            var urutanTerakhir = await _dbContext.Set<TrxClinicalNoteAddendum>()
                .Where(x => x.IntegrityId == keutuhan.Id && !x.IsDelete)
                .Select(x => (int?)x.Sequence)
                .MaxAsync(cancellationToken) ?? 0;

            var addendum = new TrxClinicalNoteAddendum
            {
                IntegrityId = keutuhan.Id,
                Sequence = urutanTerakhir + 1,
                AuthorUserId = actorUserId,
                IsSubstituteAuthor = kewenangan.IsSubstituteAuthor,
                DelegationId = kewenangan.DelegationId,
                AddendumText = addendumText.Trim(),
                CorrectionReason = correctionReason.Trim(),
                SignedAt = nowUtc,
                SignatureDeviceInfo = Potong(deviceInfo, 250),
                SignatureIpAddress = Potong(ipAddress, 64),
                CreateDateTime = nowUtc,
                CreateBy = actorUserId
            };

            await _dbContext.Set<TrxClinicalNoteAddendum>().AddAsync(addendum, cancellationToken);

            // Status dokumen SENGAJA tidak berubah. Dokumen yang Signed tetap Signed setelah
            // dikoreksi sepuluh kali. Addendum adalah lampiran, bukan perubahan keadaan.
            keutuhan.AddendumCount += 1;
            keutuhan.UpdateDateTime = nowUtc;
            keutuhan.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return (IntegrityGuardResult.Allowed(), addendum);
        }

        /// <summary>
        /// Daftar addendum sebuah dokumen, urut dari koreksi pertama.
        /// </summary>
        public async Task<List<TrxClinicalNoteAddendum>> ListByDocumentAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            var keutuhan = await _integrityService.FindAsync(documentKind, documentId, cancellationToken);

            if (keutuhan == null)
                return [];

            return await _dbContext.Set<TrxClinicalNoteAddendum>()
                .AsNoTracking()
                .Where(x => x.IntegrityId == keutuhan.Id && !x.IsDelete)
                .OrderBy(x => x.Sequence)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Mencari penetapan berhalangan yang masih berlaku untuk seorang penulis.
        /// </summary>
        public Task<TrxClinicalNoteAuthorDelegation?> CariPenetapanBerlakuAsync(
            Guid originalAuthorUserId,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
            => _dbContext.Set<TrxClinicalNoteAuthorDelegation>()
                .AsNoTracking()
                .Where(x => x.OriginalAuthorUserId == originalAuthorUserId
                            && x.IsActive
                            && !x.IsDelete
                            && x.RevokedAt == null
                            && x.ValidFrom <= nowUtc
                            && (x.ValidUntil == null || x.ValidUntil >= nowUtc))
                .OrderByDescending(x => x.ValidFrom)
                .FirstOrDefaultAsync(cancellationToken);

        private static AddendumAuthorityResponse Tolak(string penjelasan) => new()
        {
            IsAllowed = false,
            IsOriginalAuthor = false,
            IsSubstituteAuthor = false,
            Explanation = penjelasan
        };

        private static string? Potong(string? nilai, int panjangMaksimum)
        {
            if (string.IsNullOrWhiteSpace(nilai))
                return null;

            var bersih = nilai.Trim();
            return bersih.Length <= panjangMaksimum ? bersih : bersih[..panjangMaksimum];
        }
    }
}
