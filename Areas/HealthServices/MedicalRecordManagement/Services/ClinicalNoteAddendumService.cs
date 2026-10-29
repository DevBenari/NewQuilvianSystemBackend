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
        private readonly MedicalRecordCompletenessQueue? _completenessQueue;

        public ClinicalNoteAddendumService(
            ApplicationDbContext dbContext,
            ClinicalDocumentIntegrityService integrityService,
            MedicalRecordCompletenessQueue? completenessQueue = null)
        {
            _dbContext = dbContext;
            _integrityService = integrityService;
            _completenessQueue = completenessQueue;
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
                // BE-RWI-038, VAL-DOK-32. Untuk jenis dokumen yang tunduk aturan keutuhan,
                // tidak adanya baris keutuhan berarti dokumennya memang BELUM difinalkan —
                // pendaftarannya justru terjadi pada saat finalisasi. Arahan yang benar bagi
                // pengguna karena itu bukan "dokumen tidak dikenal", melainkan "perbaiki
                // langsung pada catatannya", karena catatan yang belum final memang masih
                // dapat disunting biasa dan tidak memerlukan koreksi beralasan.
                return Tolak(ClinicalDocumentIntegrityService.DitegakkanUntuk(documentKind)
                    ? "Catatan ini belum final. Perbaiki langsung pada catatannya."
                    : "Catatan tidak ditemukan pada daftar keutuhan.");
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
            var pernahAdaPenetapan = await _dbContext.Set<MrcClinicalNoteAuthorDelegation>()
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
        public Task<(IntegrityGuardResult Result, MrcClinicalNoteAddendum? Addendum)> CreateAsync(
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
            => CreateAsync(documentKind, documentId, actorUserId, actorHasSubstituteAuthority,
                addendumText, correctionReason, deviceInfo, ipAddress, nowUtc,
                ClinicalNoteAddendumPurpose.Correction, cancellationToken);

        /// <summary>
        /// Sama dengan overload di atas, dengan maksud addendum (`BE-38`, `RM-DEC-049`). Overload
        /// terpisah supaya pemanggil di modul lain yang meneruskan token secara posisional tidak
        /// berubah.
        ///
        /// ADDENDUM PENGESAHAN (<see cref="ClinicalNoteAddendumPurpose.Attestation"/>) menyatakan isi
        /// catatan <c>LockedUnsigned</c> benar. Urutan pemeriksaannya mengikuti S-C:
        /// <list type="number">
        /// <item>catatan bukan <c>LockedUnsigned</c> → <c>409</c> `VAL-RM-A01`;</item>
        /// <item>sudah ada pengesahan → <c>409</c> `VAL-RM-A02`;</item>
        /// <item>bukan penulis dan tanpa jalur pengganti sah → <c>403</c> `VAL-RM-A03`, pesan addendum
        /// yang sudah ada.</item>
        /// </list>
        /// Status catatan TIDAK berubah — tetap <c>LockedUnsigned</c> sebagai jejak jujur bahwa
        /// catatan tidak pernah ditandatangani pada waktunya.
        /// </summary>
        public async Task<(IntegrityGuardResult Result, MrcClinicalNoteAddendum? Addendum)> CreateAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            Guid actorUserId,
            bool actorHasSubstituteAuthority,
            string addendumText,
            string correctionReason,
            string? deviceInfo,
            string? ipAddress,
            DateTime nowUtc,
            ClinicalNoteAddendumPurpose purpose,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(purpose))
            {
                return (IntegrityGuardResult.Denied(
                    StatusCodes.Status400BadRequest, "Maksud addendum tidak dikenal."), null);
            }

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

            if (purpose == ClinicalNoteAddendumPurpose.Attestation)
            {
                var penolakan = await PeriksaPengesahanAsync(documentKind, documentId, cancellationToken);
                if (penolakan != null)
                    return (penolakan, null);
            }

            var kewenangan = await ResolveAuthorityAsync(
                documentKind, documentId, actorUserId, actorHasSubstituteAuthority,
                nowUtc, cancellationToken);

            if (!kewenangan.IsAllowed)
            {
                // Dokumen yang belum terkunci atau sudah dibatalkan adalah kesalahan permintaan,
                // bukan kekurangan wewenang. Bedanya penting bagi pengguna.
                var kode = kewenangan.Explanation.Contains("belum terkunci")
                           || kewenangan.Explanation.Contains("belum final")
                           || kewenangan.Explanation.Contains("dibatalkan")
                           || kewenangan.Explanation.Contains("tidak ditemukan")
                    ? StatusCodes.Status400BadRequest
                    : StatusCodes.Status403Forbidden;

                return (IntegrityGuardResult.Denied(kode, kewenangan.Explanation), null);
            }

            var keutuhan = await _dbContext.Set<MrcClinicalDocumentIntegrity>()
                .FirstAsync(x => x.DocumentKind == documentKind
                                 && x.DocumentId == documentId
                                 && !x.IsDelete, cancellationToken);

            var urutanTerakhir = await _dbContext.Set<MrcClinicalNoteAddendum>()
                .Where(x => x.IntegrityId == keutuhan.Id && !x.IsDelete)
                .Select(x => (int?)x.Sequence)
                .MaxAsync(cancellationToken) ?? 0;

            var addendum = new MrcClinicalNoteAddendum
            {
                IntegrityId = keutuhan.Id,
                AddendumPurpose = purpose,
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

            await _dbContext.Set<MrcClinicalNoteAddendum>().AddAsync(addendum, cancellationToken);

            // Status dokumen SENGAJA tidak berubah. Dokumen yang Signed tetap Signed setelah
            // dikoreksi sepuluh kali. Addendum adalah lampiran, bukan perubahan keadaan.
            keutuhan.AddendumCount += 1;
            keutuhan.UpdateDateTime = nowUtc;
            keutuhan.UpdateBy = actorUserId;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException) when (purpose == ClinicalNoteAddendumPurpose.Attestation)
            {
                // Dua pengesahan atas catatan yang sama hampir bersamaan: keduanya lolos
                // pemeriksaan di atas, lalu index unik terfilter menolak salah satunya. Yang ditolak
                // dijawab VAL-RM-A02, dan tidak ada yang tersimpan setengah.
                _dbContext.Entry(addendum).State = EntityState.Detached;
                _dbContext.Entry(keutuhan).State = EntityState.Unchanged;

                var sudahDisahkan = await _dbContext.Set<MrcClinicalNoteAddendum>()
                    .AsNoTracking()
                    .AnyAsync(x => x.IntegrityId == keutuhan.Id
                                   && x.AddendumPurpose == ClinicalNoteAddendumPurpose.Attestation
                                   && !x.IsDelete, cancellationToken);

                // Tabrakan lain (mis. nomor urut kembar dengan koreksi serentak) bukan VAL-RM-A02;
                // perilakunya sama dengan addendum koreksi sebelum BE-38.
                if (!sudahDisahkan)
                    throw;

                return (IntegrityGuardResult.Denied(StatusCodes.Status409Conflict, SudahDisahkanMessage), null);
            }

            // A4.12 / BE-40: pengesahan menuntaskan kekurangan "tidak ditandatangani", sehingga
            // kunjungannya dinilai ulang. Hanya antrean memori; addendum tidak menunggu penilaian.
            if (purpose == ClinicalNoteAddendumPurpose.Attestation)
                _completenessQueue?.EnqueueReevaluation(keutuhan.EncounterId);

            return (IntegrityGuardResult.Allowed(), addendum);
        }

        public const string BukanTerkunciTanpaTandaTanganMessage =
            "Pengesahan hanya untuk catatan yang terkunci tanpa tanda tangan.";

        public const string SudahDisahkanMessage = "Catatan ini sudah disahkan sebelumnya.";

        /// <summary>
        /// Syarat pengesahan yang berasal dari keadaan catatan (`VAL-RM-A01`, `A02`). Catatan yang
        /// belum terdaftar pada daftar keutuhan dibiarkan lolos ke pemeriksaan kewenangan, yang
        /// sudah menjawabnya dengan arahan yang tepat.
        /// </summary>
        private async Task<IntegrityGuardResult?> PeriksaPengesahanAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            CancellationToken cancellationToken)
        {
            var keutuhan = await _integrityService.FindAsync(documentKind, documentId, cancellationToken);
            if (keutuhan == null)
                return null;

            if (keutuhan.IntegrityStatus != ClinicalDocumentIntegrityStatus.LockedUnsigned)
                return IntegrityGuardResult.Denied(StatusCodes.Status409Conflict, BukanTerkunciTanpaTandaTanganMessage);

            var sudahDisahkan = await _dbContext.Set<MrcClinicalNoteAddendum>()
                .AsNoTracking()
                .AnyAsync(x => x.IntegrityId == keutuhan.Id
                               && x.AddendumPurpose == ClinicalNoteAddendumPurpose.Attestation
                               && !x.IsDelete, cancellationToken);

            return sudahDisahkan
                ? IntegrityGuardResult.Denied(StatusCodes.Status409Conflict, SudahDisahkanMessage)
                : null;
        }

        /// <summary>
        /// Daftar addendum sebuah dokumen, urut dari koreksi pertama.
        /// </summary>
        public async Task<List<MrcClinicalNoteAddendum>> ListByDocumentAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            var keutuhan = await _integrityService.FindAsync(documentKind, documentId, cancellationToken);

            if (keutuhan == null)
                return [];

            return await _dbContext.Set<MrcClinicalNoteAddendum>()
                .AsNoTracking()
                .Where(x => x.IntegrityId == keutuhan.Id && !x.IsDelete)
                .OrderBy(x => x.Sequence)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Mencari penetapan berhalangan yang masih berlaku untuk seorang penulis.
        /// </summary>
        public Task<MrcClinicalNoteAuthorDelegation?> CariPenetapanBerlakuAsync(
            Guid originalAuthorUserId,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
            => _dbContext.Set<MrcClinicalNoteAuthorDelegation>()
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
