using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Service penyimpanan dan pembacaan Formulir Persetujuan Rawat Inap dan Tanda Tangan Digital (BE-RWI-205).
    /// Mengimplementasikan penegakan invariant penanda tangan bayi baru lahir (VAL-ADM-05, RWI-DEC-273).
    /// </summary>
    /// <remarks>
    /// <b>Berversi sejak BE-RWI-227</b> (<c>PLAN-REPAIR-EPS-008</c>). Persetujuan yang sudah
    /// <c>Completed</c> tidak pernah ditimpa: setiap koreksi — Edit Persetujuan di Workspace PPRI
    /// maupun simpan ulang berisi berbeda dari langkah admisi — menjadikan versi lama
    /// <c>Superseded</c> dan membuat versi baru yang menunjuknya (<c>INV-RWA-05</c>, <c>RWI-DEC-240</c>).
    ///
    /// <para>
    /// Contoh: versi 1 Ny. Rina (istri) disimpan Sari pukul 09.40 dengan NIK salah satu angka. Pukul
    /// 10.15 Dewi mengoreksi NIK beralasan "NIK penanda tangan salah ketik saat admisi" → versi 1
    /// <c>Superseded</c> beserta isi dan tanda tangannya utuh, versi 2 <c>Completed</c>. Riwayat
    /// menampilkan "No. Identitas: …98 → …99" oleh Dewi.
    /// </para>
    /// </remarks>
    public class InpatientAdmissionConsentService
    {
        public const string NotFoundMessage = "Formulir persetujuan rawat inap belum diisi atau tidak ditemukan.";

        public const string StaleMessage = "Persetujuan sudah diubah petugas lain. Muat ulang lalu ulangi perubahan Anda.";

        public const string NewbornSignerCode = "BAYI_PENANDATANGAN_WAJIB_WALI";

        /// <summary>Jenis penanda tangan berganti tanpa tanda tangan baru (K-01 PLAN-REPAIR-EPS-008).</summary>
        public const string NewSignatureRequiredCode = "TTD_BARU_WAJIB";

        public const string NoChangeCode = "TIDAK_ADA_PERUBAHAN";

        private const string NewbornSignerMessage =
            "Pasien bayi baru lahir tidak dapat menandatangani sendiri; penanda tangan wajib orang tua atau wali yang sah.";

        private const string AdmissionStepRevisionReason = "Disimpan ulang dari langkah Formulir Persetujuan admisi.";

        private const int CorrectionReasonMinLength = 10;
        private const int CorrectionReasonMaxLength = 500;

        private static readonly JsonSerializerOptions SnapshotReadOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly ApplicationDbContext _dbContext;
        private readonly InpAdmissionWriteGuard _writeGuard;
        private readonly InpAdmissionSourceReader _sourceReader;

        public InpatientAdmissionConsentService(
            ApplicationDbContext dbContext,
            InpAdmissionWriteGuard writeGuard,
            InpAdmissionSourceReader sourceReader)
        {
            _dbContext = dbContext;
            _writeGuard = writeGuard;
            _sourceReader = sourceReader;
        }

        /// <summary>
        /// Menyimpan formulir persetujuan dan tanda tangan digital ke episode rawat inap.
        /// </summary>
        /// <remarks>
        /// Bila episode sudah punya persetujuan <c>Completed</c>: isi sama → versi aktif dikembalikan
        /// tanpa perubahan; isi berbeda → versi baru beralasan otomatis (BE-RWI-227). Waktu tanda
        /// tangan yang dikirim ulang wizard tidak dihitung sebagai perubahan.
        /// </remarks>
        public async Task<InpAdmissionResult<InpatientAdmissionConsentResponse>> SaveConsentAsync(
            Guid episodeId,
            InpatientAdmissionConsentCreateDto request,
            Guid? actorUserId = null,
            CancellationToken cancellationToken = default)
        {
            // 1. Validasi Keberadaan Episode
            var episode = await _dbContext.Set<InpEpisode>()
                .Include(e => e.Patient)
                .FirstOrDefaultAsync(e => e.Id == episodeId && !e.IsDelete, cancellationToken);

            if (episode == null)
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status404NotFound, "Episode rawat inap tidak ditemukan.");
            }

            // 2. VAL-ADM-04: Pernyataan persetujuan umum (12 klausul) & tanda tangan digital wajib dilengkapi
            if (request.AgreedClauses == null || request.AgreedClauses.Count < 12 ||
                string.IsNullOrWhiteSpace(request.SignatureImageBase64) ||
                string.IsNullOrWhiteSpace(request.SignerName) ||
                request.SignerName.Trim().Length < 3)
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pernyataan persetujuan umum dan tanda tangan digital wajib dilengkapi sebelum cetak.",
                    "VAL-ADM-04");
            }

            // 3. VAL-ADM-05 & RWI-DEC-273: Penanda tangan Bayi Baru Lahir dilarang diri sendiri, wajib orang tua atau wali
            var isSelfSigner = IsSelfSigner(request.SignerType, request.SignerRelationship);

            if (IsNewborn(episode, request.PatientCategory) && isSelfSigner)
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity, NewbornSignerMessage, NewbornSignerCode);
            }

            // 4. Cari dokumen persetujuan rawat inap aktif yang sudah ada untuk episode ini
            var existingDoc = await LoadActiveAsync(episodeId, tracked: true, cancellationToken);

            var now = DateTime.UtcNow;
            var content = new InpatientAdmissionConsentResponse
            {
                EpisodeId = episodeId,
                PatientCategory = request.PatientCategory,
                SignerType = request.SignerType,
                SignerName = request.SignerName.Trim(),
                SignerRelationship = request.SignerRelationship,
                SignerRelationshipText = request.SignerRelationshipText,
                SignerPhoneNumber = request.SignerPhoneNumber,
                SignerIdentityNumber = request.SignerIdentityNumber,
                SignerAddress = request.SignerAddress,
                AgreedClauses = request.AgreedClauses,
                SignatureImageBase64 = request.SignatureImageBase64,
                SignedAt = ToUtc(request.SignedAt) ?? now,
                GuidanceReceived = request.GuidanceReceived,
                GuidanceNote = InpAdmissionText.Clean(request.GuidanceNote),
                Status = "Signed",
                IsSigned = true
            };

            // 5. BE-RWI-227: persetujuan yang sudah lengkap tidak ditimpa — simpan ulang menjadi versi baru
            if (existingDoc?.Status == InpAdmissionDocumentStatus.Completed)
            {
                var previous = ReadContent(existingDoc);
                var signatureReplaced = !string.Equals(
                    previous.SignatureImageBase64, content.SignatureImageBase64, StringComparison.Ordinal);

                content.DocumentNumber = previous.DocumentNumber;

                if (!signatureReplaced)
                {
                    content.SignedAt = previous.SignedAt ?? content.SignedAt;
                }

                if (DescribeChanges(previous, content, signatureReplaced).Count == 0)
                {
                    await FillVersionInfoAsync(existingDoc, previous, cancellationToken);
                    return InpAdmissionResult<InpatientAdmissionConsentResponse>.Ok(
                        previous, "Formulir persetujuan rawat inap sudah tersimpan; tidak ada perubahan.");
                }

                return await CreateRevisionAsync(
                    existingDoc, content, AdmissionStepRevisionReason, actorUserId ?? Guid.Empty, isSelfSigner, now, cancellationToken);
            }

            var docId = existingDoc?.Id ?? Guid.NewGuid();
            content.ConsentId = docId;
            content.DocumentNumber = $"GCR-{now:yyyyMM}-{(episode.EpisodeNumber.Length > 4 ? episode.EpisodeNumber[^4..] : Guid.NewGuid().ToString("N")[..4])}";

            var snapshotJson = JsonSerializer.Serialize(content);
            InpAdmissionDocument savedDoc;

            if (existingDoc == null)
            {
                var doc = new InpAdmissionDocument
                {
                    Id = docId,
                    EpisodeId = episodeId,
                    PatientId = episode.PatientId,
                    DocumentType = InpAdmissionDocumentType.GeneralConsent,
                    Status = InpAdmissionDocumentStatus.Completed,
                    VersionNo = 1,
                    StatementDate = now.Date,
                    LockedAt = now,
                    LockedByUserId = actorUserId,
                    CompletedAt = now,
                    SnapshotJson = snapshotJson,
                    SnapshotFormatVersion = 1,
                    RowVersion = Guid.NewGuid(),
                    CreateBy = actorUserId ?? Guid.Empty,
                    CreateDateTime = now
                };

                _dbContext.Set<InpAdmissionDocument>().Add(doc);
                _dbContext.Set<InpAdmissionDocumentParty>().Add(BuildParty(docId, content, isSelfSigner, actorUserId, now));
                _dbContext.Set<InpAdmissionDocumentSignature>().Add(BuildSignature(docId, content, actorUserId, now));
                savedDoc = doc;
            }
            else
            {
                // Konsep/menunggu tanda tangan dari alur dokumen generik: belum pernah lengkap,
                // sehingga dilengkapi di tempat tanpa menghapus versi bertanda tangan apa pun.
                existingDoc.Status = InpAdmissionDocumentStatus.Completed;
                existingDoc.CompletedAt = now;
                existingDoc.LockedAt ??= now;
                existingDoc.LockedByUserId ??= actorUserId;
                existingDoc.SnapshotJson = snapshotJson;
                existingDoc.SnapshotFormatVersion = 1;
                existingDoc.RowVersion = Guid.NewGuid();
                existingDoc.UpdateBy = actorUserId ?? Guid.Empty;
                existingDoc.UpdateDateTime = now;

                if (existingDoc.Party != null)
                {
                    existingDoc.Party.FullName = content.SignerName;
                    existingDoc.Party.Relationship = MapRelationship(request.SignerRelationship);
                    existingDoc.Party.RelationshipText = request.SignerRelationshipText ?? request.SignerRelationship;
                    existingDoc.Party.Address = request.SignerAddress;
                    existingDoc.Party.MobilePhone = TruncatePhone(request.SignerPhoneNumber);
                    existingDoc.Party.IdentityNumber = request.SignerIdentityNumber;
                    existingDoc.Party.UpdateBy = actorUserId ?? Guid.Empty;
                    existingDoc.Party.UpdateDateTime = now;
                }

                var sig = existingDoc.Signatures?.FirstOrDefault(s => s.Slot == InpAdmissionSignatureSlot.PatientOrFamily && !s.IsDelete);
                if (sig != null)
                {
                    sig.SignerName = content.SignerName;
                    sig.SignerRelationship = MapRelationship(request.SignerRelationship);
                    sig.SignerRelationshipText = request.SignerRelationshipText ?? request.SignerRelationship;
                    sig.SignedAt = content.SignedAt ?? now;
                    sig.RecordedAt = now;
                    sig.VerifiedByUserId = actorUserId;
                    sig.UpdateBy = actorUserId ?? Guid.Empty;
                    sig.UpdateDateTime = now;
                }

                savedDoc = existingDoc;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await FillVersionInfoAsync(savedDoc, content, cancellationToken);

            return InpAdmissionResult<InpatientAdmissionConsentResponse>.Ok(
                content, "Formulir persetujuan rawat inap dan tanda tangan digital berhasil disimpan.");
        }

        /// <summary>
        /// Mengambil formulir persetujuan rawat inap dan citra tanda tangan digital dari episode rawat inap.
        /// </summary>
        /// <remarks>Selalu versi aktif; versi <c>Superseded</c> hanya terbaca lewat riwayat (BE-RWI-227).</remarks>
        public async Task<InpAdmissionResult<InpatientAdmissionConsentResponse>> GetConsentAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var doc = await LoadActiveAsync(episodeId, tracked: false, cancellationToken);

            if (doc == null || string.IsNullOrWhiteSpace(doc.SnapshotJson))
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status404NotFound, NotFoundMessage);
            }

            var response = ReadContent(doc);
            response.Status = doc.Status.ToString();
            await FillVersionInfoAsync(doc, response, cancellationToken);

            return InpAdmissionResult<InpatientAdmissionConsentResponse>.Ok(
                response, "Formulir persetujuan rawat inap berhasil dimuat.");
        }

        /// <summary>
        /// Edit Persetujuan dari Workspace PPRI: selalu membuat versi baru beralasan (BE-RWI-227).
        /// </summary>
        /// <remarks>
        /// Urutan penolakan: episode tertutup (409) → belum ada persetujuan (404) → <c>RowVersion</c>
        /// (400/409) → alasan dan nama (400) → bayi menandatangani sendiri (422) → jenis penanda
        /// tangan berganti tanpa tanda tangan baru (422) → tidak ada perubahan (422).
        /// </remarks>
        public async Task<InpAdmissionResult<InpatientAdmissionConsentResponse>> ReviseConsentAsync(
            Guid episodeId,
            InpatientAdmissionConsentRevisionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var gate = await _writeGuard.EnsureWritableAsync(episodeId, cancellationToken);

            if (!gate.IsSuccess)
            {
                return gate.AsFailure<InpatientAdmissionConsentResponse>();
            }

            var existingDoc = await LoadActiveAsync(episodeId, tracked: true, cancellationToken);

            if (existingDoc == null ||
                existingDoc.Status != InpAdmissionDocumentStatus.Completed ||
                string.IsNullOrWhiteSpace(existingDoc.SnapshotJson))
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status404NotFound, NotFoundMessage);
            }

            if (!request.RowVersion.HasValue || request.RowVersion.Value == Guid.Empty)
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status400BadRequest, "RowVersion persetujuan wajib dikirim.");
            }

            if (existingDoc.RowVersion != request.RowVersion.Value)
            {
                return Stale();
            }

            var correctionReason = InpAdmissionText.Clean(request.CorrectionReason);

            if (correctionReason == null || correctionReason.Length < CorrectionReasonMinLength)
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Alasan perubahan minimal 10 karakter.");
            }

            if (correctionReason.Length > CorrectionReasonMaxLength)
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Alasan perubahan maksimal 500 karakter.");
            }

            var signerName = InpAdmissionText.Clean(request.SignerName);

            if (signerName == null || signerName.Length < 3)
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Nama penanda tangan minimal 3 karakter.");
            }

            var previous = ReadContent(existingDoc);
            var isSelfSigner = IsSelfSigner(request.SignerType, request.SignerRelationship);

            var episode = await _dbContext.Set<InpEpisode>()
                .AsNoTracking()
                .Include(e => e.Patient)
                .FirstAsync(e => e.Id == episodeId, cancellationToken);

            if (IsNewborn(episode, previous.PatientCategory) && isSelfSigner)
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity, NewbornSignerMessage, NewbornSignerCode);
            }

            var newSignature = InpAdmissionText.Clean(request.SignatureImageBase64);

            if (newSignature != null && !newSignature.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase))
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Format tanda tangan tidak dikenali. Bubuhkan ulang tanda tangan.");
            }

            var signerTypeChanged = !string.Equals(
                InpAdmissionText.Clean(previous.SignerType), InpAdmissionText.Clean(request.SignerType), StringComparison.OrdinalIgnoreCase);

            if (signerTypeChanged && newSignature == null)
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Jenis penanda tangan berubah. Bubuhkan tanda tangan penanda tangan yang baru.",
                    NewSignatureRequiredCode);
            }

            var now = DateTime.UtcNow;
            var signatureReplaced = newSignature != null &&
                !string.Equals(newSignature, previous.SignatureImageBase64, StringComparison.Ordinal);

            var content = new InpatientAdmissionConsentResponse
            {
                EpisodeId = episodeId,
                DocumentNumber = previous.DocumentNumber,
                PatientCategory = previous.PatientCategory,
                SignerType = request.SignerType.Trim(),
                SignerName = signerName,
                SignerRelationship = InpAdmissionText.Clean(request.SignerRelationship) ?? request.SignerType.Trim(),
                SignerRelationshipText = InpAdmissionText.Clean(request.SignerRelationshipText),
                SignerPhoneNumber = InpAdmissionText.Clean(request.SignerPhoneNumber),
                SignerIdentityNumber = InpAdmissionText.Clean(request.SignerIdentityNumber),
                SignerAddress = InpAdmissionText.Clean(request.SignerAddress),
                AgreedClauses = previous.AgreedClauses,
                SignatureImageBase64 = signatureReplaced ? newSignature! : previous.SignatureImageBase64,
                SignedAt = signatureReplaced ? now : previous.SignedAt,
                GuidanceReceived = request.GuidanceReceived,
                GuidanceNote = InpAdmissionText.Clean(request.GuidanceNote),
                Status = "Signed",
                IsSigned = true
            };

            if (DescribeChanges(previous, content, signatureReplaced).Count == 0)
            {
                return InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity, "Tidak ada data yang berubah.", NoChangeCode);
            }

            return await CreateRevisionAsync(
                existingDoc, content, correctionReason, actorUserId, isSelfSigner, now, cancellationToken);
        }

        /// <summary>Seluruh versi persetujuan episode, terbaru di atas, beserta pengubah dan isian yang berubah.</summary>
        public async Task<InpAdmissionResult<List<InpatientAdmissionConsentHistoryResponse>>> GetConsentHistoryAsync(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var gate = await _writeGuard.EnsureReadableAsync(episodeId, cancellationToken);

            if (!gate.IsSuccess)
            {
                return gate.AsFailure<List<InpatientAdmissionConsentHistoryResponse>>();
            }

            var versions = await ConsentVersions(episodeId)
                .AsNoTracking()
                .Include(d => d.Party)
                .Where(d => d.SnapshotJson != null)
                .OrderBy(d => d.CreateDateTime)
                .ThenBy(d => d.VersionNo)
                .ToListAsync(cancellationToken);

            var users = await _sourceReader.GetUsersAsync(versions.Select(x => x.CreateBy), cancellationToken);
            var contents = versions.ToDictionary(x => x.Id, ReadContent);
            var rows = new List<InpatientAdmissionConsentHistoryResponse>(versions.Count);

            foreach (var version in versions)
            {
                var current = contents[version.Id];
                var previous = version.PreviousVersionId.HasValue &&
                               contents.TryGetValue(version.PreviousVersionId.Value, out var found)
                    ? found
                    : null;

                var signatureReplaced = previous != null && !string.Equals(
                    previous.SignatureImageBase64, current.SignatureImageBase64, StringComparison.Ordinal);

                users.TryGetValue(version.CreateBy, out var user);

                rows.Add(new InpatientAdmissionConsentHistoryResponse
                {
                    VersionNo = version.VersionNo,
                    Status = StatusLabel(version.Status),
                    IsActive = version.Status == InpAdmissionDocumentStatus.Completed,
                    ChangedAt = version.CreateDateTime,
                    ChangedByName = user?.DisplayName,
                    ChangedByPosition = user?.PositionName,
                    CorrectionReason = version.CorrectionReason,
                    IsSignatureReplaced = signatureReplaced,
                    Changes = previous == null
                        ? new List<InpatientAdmissionConsentChangeResponse>()
                        : DescribeChanges(previous, current, signatureReplaced)
                });
            }

            rows.Reverse();

            return InpAdmissionResult<List<InpatientAdmissionConsentHistoryResponse>>.Ok(
                rows,
                rows.Count == 0
                    ? "Belum ada persetujuan rawat inap tersimpan."
                    : "Riwayat persetujuan rawat inap berhasil dimuat.");
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <remarks>
        /// Satu transaksi dengan urutan wajib karena unique index dokumen aktif: (1) versi lama
        /// menjadi <c>Superseded</c> dan disimpan, (2) versi baru <c>Completed</c> ditambahkan —
        /// sama dengan <c>InpAdmissionDocumentService.ReviseAsync</c>. Versi lama tidak diubah isinya.
        /// </remarks>
        private async Task<InpAdmissionResult<InpatientAdmissionConsentResponse>> CreateRevisionAsync(
            InpAdmissionDocument previousDoc,
            InpatientAdmissionConsentResponse content,
            string correctionReason,
            Guid actorUserId,
            bool isSelfSigner,
            DateTime now,
            CancellationToken cancellationToken)
        {
            var revisionId = Guid.NewGuid();
            content.ConsentId = revisionId;

            var revision = new InpAdmissionDocument
            {
                Id = revisionId,
                EpisodeId = previousDoc.EpisodeId,
                PatientId = previousDoc.PatientId,
                DocumentType = InpAdmissionDocumentType.GeneralConsent,
                Status = InpAdmissionDocumentStatus.Completed,
                VersionNo = previousDoc.VersionNo + 1,
                PreviousVersionId = previousDoc.Id,
                CorrectionReason = correctionReason,
                SigningCity = previousDoc.SigningCity,
                StatementDate = previousDoc.StatementDate,
                Note = previousDoc.Note,
                LockedAt = now,
                LockedByUserId = actorUserId,
                CompletedAt = now,
                SnapshotJson = JsonSerializer.Serialize(content),
                SnapshotFormatVersion = 1,
                RowVersion = Guid.NewGuid(),
                CreateBy = actorUserId,
                CreateDateTime = now
            };

            revision.Party = BuildParty(revisionId, content, isSelfSigner, actorUserId, now);
            revision.Signatures.Add(BuildSignature(revisionId, content, actorUserId, now));

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                previousDoc.Status = InpAdmissionDocumentStatus.Superseded;
                previousDoc.SupersededAt = now;
                InpAdmissionDocumentService.Touch(previousDoc, actorUserId, now);
                await _dbContext.SaveChangesAsync(cancellationToken);

                _dbContext.Set<InpAdmissionDocument>().Add(revision);
                await _dbContext.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
                return Stale();
            }
            catch (DbUpdateException exception) when (InpAdmissionDocumentService.IsUniqueViolation(exception, out _))
            {
                // Petugas lain membuat versi berikutnya lebih dulu (unique index dokumen aktif atau PreviousVersionId).
                await transaction.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
                return Stale();
            }

            await FillVersionInfoAsync(revision, content, cancellationToken);

            return InpAdmissionResult<InpatientAdmissionConsentResponse>.Ok(
                content, $"Persetujuan diperbarui menjadi versi {revision.VersionNo}.");
        }

        private IQueryable<InpAdmissionDocument> ConsentVersions(Guid episodeId)
            => _dbContext.Set<InpAdmissionDocument>()
                .Where(d => d.EpisodeId == episodeId &&
                            d.DocumentType == InpAdmissionDocumentType.GeneralConsent &&
                            !d.IsDelete);

        /// <summary>Versi aktif (<c>UX_InpAdmissionDocument_Episode_Type_Active</c>): paling banyak satu.</summary>
        private Task<InpAdmissionDocument?> LoadActiveAsync(Guid episodeId, bool tracked, CancellationToken cancellationToken)
        {
            var query = ConsentVersions(episodeId)
                .Include(d => d.Party)
                .Include(d => d.Signatures)
                .Where(d => d.Status == InpAdmissionDocumentStatus.Draft ||
                            d.Status == InpAdmissionDocumentStatus.AwaitingSignature ||
                            d.Status == InpAdmissionDocumentStatus.Completed);

            if (!tracked)
            {
                query = query.AsNoTracking();
            }

            return query.OrderByDescending(d => d.VersionNo).FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>Isi persetujuan dari salinan beku; jatuh ke data pihak penanda tangan bila salinan tidak terbaca.</summary>
        private static InpatientAdmissionConsentResponse ReadContent(InpAdmissionDocument doc)
        {
            InpatientAdmissionConsentResponse? content = null;

            if (!string.IsNullOrWhiteSpace(doc.SnapshotJson))
            {
                try
                {
                    content = JsonSerializer.Deserialize<InpatientAdmissionConsentResponse>(doc.SnapshotJson, SnapshotReadOptions);
                }
                catch (Exception exception) when (exception is JsonException or NotSupportedException)
                {
                    // Fallback bila deserialisasi snapshot gagal
                }
            }

            content ??= new InpatientAdmissionConsentResponse
            {
                DocumentNumber = $"GCR-{doc.StatementDate:yyyyMM}-{doc.Id.ToString("N")[..4]}",
                SignerName = doc.Party?.FullName ?? "Pasien / Keluarga",
                SignerRelationshipText = doc.Party?.RelationshipText,
                SignerPhoneNumber = doc.Party?.MobilePhone,
                SignerIdentityNumber = doc.Party?.IdentityNumber,
                SignerAddress = doc.Party?.Address,
                Status = doc.Status.ToString()
            };

            content.ConsentId = doc.Id;
            content.EpisodeId = doc.EpisodeId;
            content.IsSigned = true;
            return content;
        }

        /// <summary>Field versi selalu dari kepala dokumen; "terakhir diubah" = pembuat versi ini.</summary>
        private async Task FillVersionInfoAsync(
            InpAdmissionDocument doc,
            InpatientAdmissionConsentResponse response,
            CancellationToken cancellationToken)
        {
            var users = await _sourceReader.GetUsersAsync(new[] { doc.CreateBy }, cancellationToken);

            response.ConsentId = doc.Id;
            response.VersionNo = doc.VersionNo;
            response.RowVersion = doc.RowVersion;
            response.LastChangedAt = doc.CreateDateTime;
            response.LastChangedByName = users.TryGetValue(doc.CreateBy, out var user) ? user.DisplayName : null;
        }

        /// <summary>
        /// Isian yang berubah antara dua versi. Waktu tanda tangan, kategori pasien, dan 12 klausul
        /// tidak dibandingkan: wizard mengirim ulang waktu baru dan klausul selalu lengkap.
        /// </summary>
        private static List<InpatientAdmissionConsentChangeResponse> DescribeChanges(
            InpatientAdmissionConsentResponse before,
            InpatientAdmissionConsentResponse after,
            bool signatureReplaced)
        {
            var changes = new List<InpatientAdmissionConsentChangeResponse>();

            AddIfChanged(changes, "SignerType", "Jenis penanda tangan", SignerTypeLabel(before.SignerType), SignerTypeLabel(after.SignerType));
            AddIfChanged(changes, "SignerName", "Nama penanda tangan", before.SignerName, after.SignerName);
            AddIfChanged(changes, "SignerRelationshipText", "Keterangan hubungan", before.SignerRelationshipText, after.SignerRelationshipText);
            AddIfChanged(changes, "SignerIdentityNumber", "No. Identitas", before.SignerIdentityNumber, after.SignerIdentityNumber);
            AddIfChanged(changes, "SignerPhoneNumber", "No. Handphone", before.SignerPhoneNumber, after.SignerPhoneNumber);
            AddIfChanged(changes, "SignerAddress", "Alamat penanda tangan", before.SignerAddress, after.SignerAddress);
            AddIfChanged(changes, "GuidanceReceived", "Sudah menerima panduan rawat inap", GuidanceLabel(before.GuidanceReceived), GuidanceLabel(after.GuidanceReceived));
            AddIfChanged(changes, "GuidanceNote", "Keterangan panduan", before.GuidanceNote, after.GuidanceNote);

            if (signatureReplaced)
            {
                changes.Add(new InpatientAdmissionConsentChangeResponse
                {
                    Field = "Signature",
                    Label = "Tanda tangan",
                    Before = "Tanda tangan sebelumnya",
                    After = "Tanda tangan baru dibubuhkan"
                });
            }

            return changes;
        }

        private static void AddIfChanged(
            List<InpatientAdmissionConsentChangeResponse> changes,
            string field,
            string label,
            string? before,
            string? after)
        {
            var cleanBefore = InpAdmissionText.Clean(before);
            var cleanAfter = InpAdmissionText.Clean(after);

            if (string.Equals(cleanBefore, cleanAfter, StringComparison.Ordinal))
            {
                return;
            }

            changes.Add(new InpatientAdmissionConsentChangeResponse
            {
                Field = field,
                Label = label,
                Before = cleanBefore,
                After = cleanAfter
            });
        }

        /// <summary>Label sama dengan pilihan layar admisi, misalnya <c>Spouse</c> → "Suami / Istri".</summary>
        private static string? SignerTypeLabel(string? signerType)
            => InpAdmissionText.Clean(signerType)?.ToLowerInvariant() switch
            {
                null => null,
                "self" or "pasien" => "Pasien Sendiri",
                "parent" => "Orang Tua",
                "guardian" => "Wali yang Sah",
                "spouse" => "Suami / Istri",
                "family" => "Keluarga Lainnya",
                _ => signerType!.Trim()
            };

        private static string GuidanceLabel(bool? guidanceReceived) => guidanceReceived switch
        {
            true => "Ya",
            false => "Tidak",
            null => "Belum diisi"
        };

        private static string StatusLabel(InpAdmissionDocumentStatus status) => status switch
        {
            InpAdmissionDocumentStatus.Completed => "Aktif",
            InpAdmissionDocumentStatus.Superseded => "Digantikan",
            InpAdmissionDocumentStatus.Cancelled => "Dibatalkan",
            _ => "Konsep"
        };

        private static bool IsNewborn(InpEpisode episode, string? patientCategory)
            => string.Equals(patientCategory, "BayiBaruLahir", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(patientCategory, "baby", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(patientCategory, "neonatus", StringComparison.OrdinalIgnoreCase) ||
               episode.MotherEpisodeId.HasValue ||
               (episode.Patient?.IsNewborn ?? false);

        private static bool IsSelfSigner(string? signerType, string? signerRelationship)
            => string.Equals(signerType, "Self", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(signerType, "Pasien", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(signerRelationship, "Self", StringComparison.OrdinalIgnoreCase);

        /// <summary>Pihak penanda tangan (Party) satu versi.</summary>
        private static InpAdmissionDocumentParty BuildParty(
            Guid documentId,
            InpatientAdmissionConsentResponse content,
            bool isSelfSigner,
            Guid? actorUserId,
            DateTime now)
            => new()
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                FullName = content.SignerName,
                SourceType = isSelfSigner ? InpAdmissionPartySource.Patient : InpAdmissionPartySource.Manual,
                Relationship = MapRelationship(content.SignerRelationship),
                RelationshipText = content.SignerRelationshipText ?? content.SignerRelationship,
                Address = content.SignerAddress,
                MobilePhone = TruncatePhone(content.SignerPhoneNumber),
                IdentityNumber = content.SignerIdentityNumber,
                CreateBy = actorUserId ?? Guid.Empty,
                CreateDateTime = now
            };

        /// <summary>Slot pasien/keluarga satu versi; diverifikasi petugas yang menyimpan.</summary>
        private static InpAdmissionDocumentSignature BuildSignature(
            Guid documentId,
            InpatientAdmissionConsentResponse content,
            Guid? actorUserId,
            DateTime now)
            => new()
            {
                Id = Guid.NewGuid(),
                DocumentId = documentId,
                Slot = InpAdmissionSignatureSlot.PatientOrFamily,
                Method = InpAdmissionSignatureMethod.PaperRecorded,
                SignerName = content.SignerName,
                SignerRelationship = MapRelationship(content.SignerRelationship),
                SignerRelationshipText = content.SignerRelationshipText ?? content.SignerRelationship,
                SignedAt = content.SignedAt ?? now,
                RecordedAt = now,
                VerifiedByUserId = actorUserId,
                CreateBy = actorUserId ?? Guid.Empty,
                CreateDateTime = now
            };

        private static string? TruncatePhone(string? phone)
            => phone != null && phone.Length > 13 ? phone[..13] : phone;

        private static DateTime? ToUtc(DateTime? value) => value?.Kind switch
        {
            null => null,
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value!.Value, DateTimeKind.Utc)
        };

        private static InpAdmissionResult<InpatientAdmissionConsentResponse> Stale()
            => InpAdmissionResult<InpatientAdmissionConsentResponse>.Fail(
                StatusCodes.Status409Conflict, StaleMessage, InpAdmissionCodes.StaleRowVersion);

        private static InpAdmissionPartyRelationship? MapRelationship(string? relationship)
        {
            if (string.IsNullOrWhiteSpace(relationship)) return InpAdmissionPartyRelationship.Other;

            return relationship.Trim().ToLowerInvariant() switch
            {
                "self" or "pasien" or "dirisendiri" => InpAdmissionPartyRelationship.Self,
                "spouse" or "suami" or "istri" => InpAdmissionPartyRelationship.Spouse,
                "child" or "anak" => InpAdmissionPartyRelationship.Child,
                "parent" or "orangtua" or "orang tua" or "ayah" or "ibu" => InpAdmissionPartyRelationship.Parent,
                "sibling" or "saudara" => InpAdmissionPartyRelationship.Sibling,
                "guardian" or "wali" => InpAdmissionPartyRelationship.Guardian,
                _ => InpAdmissionPartyRelationship.Other
            };
        }
    }
}
