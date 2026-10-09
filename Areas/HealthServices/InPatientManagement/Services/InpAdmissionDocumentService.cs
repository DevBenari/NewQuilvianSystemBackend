using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services
{
    /// <summary>
    /// Siklus dokumen admisi Workspace PPRI — <c>BE-RWI-194</c> (fondasi dan Permintaan Privasi),
    /// <c>BE-RWI-199</c> s.d. <c>202</c> (jenis lain); state matrix 10.1, validation 15.1–15.4.
    /// </summary>
    /// <remarks>
    /// Setiap jenis dokumen berperilaku sama: konsep → kunci (salinan beku) → tanda tangan per slot →
    /// lengkap; buka kunci hanya tanpa tanda tangan; versi koreksi hanya dari <c>Completed</c>; buang
    /// konsep oleh pembuatnya; batal beralasan oleh pemegang <c>Cancel</c>. Tidak ada hapus permanen
    /// maupun endpoint <c>DELETE</c> (<c>INV-RWA-03</c>). Isi khas per jenis ada di bagian
    /// <c>.Content.cs</c>.
    ///
    /// <para>
    /// Contoh lengkap: Serah Terima Tn. Budi dibuat Sari 09.58 (<c>Draft</c>), dikunci 10.05
    /// (<c>AwaitingSignature</c>), ditandatangani Sari 10.05, Dewi (CRO) 10.20, Andi (perawat) 10.40
    /// sesudah Budi menempati bed 10.35 → <c>Completed</c>.
    /// </para>
    /// </remarks>
    public sealed partial class InpAdmissionDocumentService
    {
        public const string PermissionResource = "InpatientAdmissionDocument";

        public const string StaleMessage = "Dokumen sudah diubah petugas lain. Muat ulang lalu ulangi perubahan Anda.";

        public const string NotFoundMessage = "Dokumen admisi tidak ditemukan.";

        public const int IdempotencyKeyMaxLength = 80;

        private const string ActiveIndexName = "UX_InpAdmissionDocument_Episode_Type_Active";
        private const string IdempotencyIndexName = "UX_InpAdmissionDocument_IdempotencyKey";
        private const string PreviousVersionIndexName = "UX_InpAdmissionDocument_PreviousVersionId";

        private readonly ApplicationDbContext _dbContext;
        private readonly InpAdmissionWriteGuard _writeGuard;
        private readonly InpAdmissionSourceReader _sourceReader;
        private readonly InpAdmissionSnapshotBuilder _snapshotBuilder;
        private readonly InpAdmissionPrefillService _prefillService;
        private readonly AccessPermissionService _permissions;

        public InpAdmissionDocumentService(
            ApplicationDbContext dbContext,
            InpAdmissionWriteGuard writeGuard,
            InpAdmissionSourceReader sourceReader,
            InpAdmissionSnapshotBuilder snapshotBuilder,
            InpAdmissionPrefillService prefillService,
            AccessPermissionService permissions)
        {
            _dbContext = dbContext;
            _writeGuard = writeGuard;
            _sourceReader = sourceReader;
            _snapshotBuilder = snapshotBuilder;
            _prefillService = prefillService;
            _permissions = permissions;
        }

        // =====================================================================
        // Bacaan
        // =====================================================================

        /// <summary>Daftar dokumen episode; versi lama dan dokumen batal hanya bila diminta.</summary>
        public async Task<InpAdmissionResult<List<AdmissionDocumentSummaryResponse>>> ListAsync(
            Guid episodeId,
            InpAdmissionDocumentType? documentType,
            bool includeHistory,
            CancellationToken cancellationToken = default)
        {
            var gate = await _writeGuard.EnsureReadableAsync(episodeId, cancellationToken);

            if (!gate.IsSuccess)
            {
                return gate.AsFailure<List<AdmissionDocumentSummaryResponse>>();
            }

            var query = _dbContext.Set<InpAdmissionDocument>()
                .AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete);

            if (documentType.HasValue)
            {
                query = query.Where(x => x.DocumentType == documentType.Value);
            }

            if (!includeHistory)
            {
                query = query.Where(x =>
                    x.Status == InpAdmissionDocumentStatus.Draft ||
                    x.Status == InpAdmissionDocumentStatus.AwaitingSignature ||
                    x.Status == InpAdmissionDocumentStatus.Completed);
            }

            var rows = await query
                .OrderBy(x => x.DocumentType)
                .ThenByDescending(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);

            var users = await _sourceReader.GetUsersAsync(rows.Select(x => x.CreateBy), cancellationToken);

            return InpAdmissionResult<List<AdmissionDocumentSummaryResponse>>.Ok(
                rows.Select(x =>
                {
                    var summary = new AdmissionDocumentSummaryResponse();
                    FillSummary(summary, x, users);
                    return summary;
                }).ToList(),
                "Daftar dokumen admisi berhasil diambil.");
        }

        /// <summary>Satu dokumen lengkap tanpa rupiah.</summary>
        public async Task<InpAdmissionResult<AdmissionDocumentResponse>> GetAsync(
            Guid episodeId,
            Guid documentId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var gate = await _writeGuard.EnsureReadableAsync(episodeId, cancellationToken);

            if (!gate.IsSuccess)
            {
                return gate.AsFailure<AdmissionDocumentResponse>();
            }

            var document = await LoadAsync(episodeId, documentId, tracked: false, cancellationToken);

            if (document == null)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(StatusCodes.Status404NotFound, NotFoundMessage);
            }

            return InpAdmissionResult<AdmissionDocumentResponse>.Ok(
                await BuildResponseAsync(document, gate.Data!, user, cancellationToken),
                "Dokumen admisi berhasil diambil.");
        }

        /// <summary>
        /// Angka Pelunasan Deposit: hidup dari Billing selama <c>Draft</c>, beku sesudah dikunci
        /// (<c>RWI-DEC-263</c>). Hanya <c>ViewAmount</c>.
        /// </summary>
        public async Task<InpAdmissionResult<AdmissionDocumentAmountsResponse>> GetAmountsAsync(
            Guid episodeId,
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            var gate = await _writeGuard.EnsureReadableAsync(episodeId, cancellationToken);

            if (!gate.IsSuccess)
            {
                return gate.AsFailure<AdmissionDocumentAmountsResponse>();
            }

            var document = await LoadAsync(episodeId, documentId, tracked: false, cancellationToken);

            if (document == null)
            {
                return InpAdmissionResult<AdmissionDocumentAmountsResponse>.Fail(StatusCodes.Status404NotFound, NotFoundMessage);
            }

            if (document.DocumentType != InpAdmissionDocumentType.DepositSettlementStatement)
            {
                return InpAdmissionResult<AdmissionDocumentAmountsResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity, "Dokumen ini tidak memuat angka rupiah.");
            }

            if (document.Status == InpAdmissionDocumentStatus.Draft)
            {
                var deposit = await _sourceReader.GetDepositSummaryAsync(episodeId, cancellationToken);

                if (!deposit.IsAvailable)
                {
                    return InpAdmissionResult<AdmissionDocumentAmountsResponse>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        "Data deposit tidak dapat dibaca dari kasir. Coba lagi beberapa saat.",
                        InpAdmissionCodes.DepositUnavailable);
                }

                return InpAdmissionResult<AdmissionDocumentAmountsResponse>.Ok(new AdmissionDocumentAmountsResponse
                {
                    Deposit = BuildDepositAmounts(
                        deposit.Value!.MinimumPolicyAmount,
                        deposit.Value.TotalReceived,
                        deposit.Value.PolicyShortfallAmount,
                        DateTime.UtcNow,
                        isFrozen: false)
                }, "Angka deposit berhasil diambil dari kasir.");
            }

            var statement = document.DepositStatement;

            return InpAdmissionResult<AdmissionDocumentAmountsResponse>.Ok(new AdmissionDocumentAmountsResponse
            {
                Deposit = BuildDepositAmounts(
                    statement?.MinimumPolicyAmount,
                    statement?.ReceivedAmount,
                    statement?.ShortfallAmount,
                    statement?.AmountsReadAt,
                    isFrozen: true)
            }, "Angka deposit beku berhasil diambil.");
        }

        // =====================================================================
        // Simpan konsep (POST /documents)
        // =====================================================================

        public async Task<InpAdmissionResult<AdmissionDocumentResponse>> CreateAsync(
            Guid episodeId,
            CreateAdmissionDocumentRequest request,
            string? idempotencyKey,
            Guid actorUserId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var key = NormalizeIdempotencyKey(idempotencyKey, out var keyInvalid);

            if (keyInvalid)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(StatusCodes.Status400BadRequest, "Kunci permintaan tidak sah.");
            }

            if (key != null)
            {
                // Pengulangan dengan kunci yang sama mengembalikan dokumen yang sama (FR-RWA-128).
                var replay = await ReplayByKeyAsync(episodeId, key, request.DocumentType, null, user, cancellationToken);

                if (replay != null)
                {
                    return replay;
                }
            }

            var gateResult = await _writeGuard.EnsureWritableAsync(episodeId, cancellationToken);

            if (!gateResult.IsSuccess)
            {
                return gateResult.AsFailure<AdmissionDocumentResponse>();
            }

            var gate = gateResult.Data!;

            if (!request.DocumentType.HasValue || !Enum.IsDefined(request.DocumentType.Value))
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(StatusCodes.Status400BadRequest, "Jenis dokumen wajib dipilih.");
            }

            var documentType = request.DocumentType.Value;

            if (!InpAdmissionDocumentRules.IsShipped(documentType))
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity, "Estimasi Biaya belum tersedia; menunggu keputusan DEC-INP-020.");
            }

            if (await HasActiveDocumentAsync(episodeId, documentType, null, cancellationToken))
            {
                return ActiveExists(documentType);
            }

            var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);
            var timeZone = InpAdmissionSourceReader.ResolveTimeZone(hospital);
            var setting = await _sourceReader.GetSettingAsync(cancellationToken);
            var now = DateTime.UtcNow;
            var today = InpAdmissionText.ToLocal(now, timeZone).Date;

            // Syarat membuat dokumen (validation 15.2) — sumber dibaca sebelum transaksi.
            InpEpisodeDepositSummaryDto? depositSummary = null;
            IReadOnlyList<InpAdmissionHandoverLine>? handoverLines = null;

            switch (documentType)
            {
                case InpAdmissionDocumentType.CostDifferenceStatement:
                    var payerFailure = await CheckCostDifferencePayerAsync(gate.EncounterId, cancellationToken);

                    if (payerFailure != null)
                    {
                        return payerFailure;
                    }

                    break;

                case InpAdmissionDocumentType.DepositSettlementStatement:
                    var deposit = await _sourceReader.GetDepositSummaryAsync(episodeId, cancellationToken);
                    var depositFailure = CheckDepositShortfall(deposit);

                    if (depositFailure != null)
                    {
                        return depositFailure;
                    }

                    depositSummary = deposit.Value;
                    break;

                case InpAdmissionDocumentType.NewPatientHandover:
                    handoverLines = await _prefillService.BuildHandoverLinesAsync(cancellationToken);

                    if (handoverLines.Count == 0)
                    {
                        return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                            StatusCodes.Status422UnprocessableEntity, "Butir serah terima belum diatur. Hubungi admin.");
                    }

                    break;
            }

            var document = new InpAdmissionDocument
            {
                Id = Guid.NewGuid(),
                EpisodeId = episodeId,
                PatientId = gate.PatientId,
                DocumentType = documentType,
                Status = InpAdmissionDocumentStatus.Draft,
                VersionNo = 1,
                SigningCity = setting.DocumentSigningCity,
                StatementDate = InpAdmissionDocumentRules.UsesStatementDate(documentType) ? AsDate(today) : null,
                IdempotencyKey = key,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            var contentFailure = await ApplyContentAsync(
                document, request, isCreate: true, handoverLines, depositSummary, timeZone, today, actorUserId, now, cancellationToken);

            if (contentFailure != null)
            {
                return contentFailure;
            }

            _dbContext.Set<InpAdmissionDocument>().Add(document);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception, out var constraint))
            {
                _dbContext.ChangeTracker.Clear();

                if (constraint == IdempotencyIndexName && key != null)
                {
                    var replay = await ReplayByKeyAsync(episodeId, key, documentType, null, user, cancellationToken);

                    if (replay != null)
                    {
                        return replay;
                    }
                }

                // Dua simpan konsep pertama bersamaan: unique index dokumen aktif menolak yang kalah.
                return ActiveExists(documentType);
            }

            return await ReloadResponseAsync(gate, document.Id, user, "Konsep dokumen admisi berhasil disimpan.", cancellationToken);
        }

        // =====================================================================
        // Ubah konsep (PUT /documents/{id})
        // =====================================================================

        public async Task<InpAdmissionResult<AdmissionDocumentResponse>> UpdateAsync(
            Guid episodeId,
            Guid documentId,
            UpdateAdmissionDocumentRequest request,
            Guid actorUserId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var loaded = await LoadForCommandAsync(episodeId, documentId, request.RowVersion, cancellationToken);

            if (!loaded.IsSuccess)
            {
                return loaded.AsFailure<AdmissionDocumentResponse>();
            }

            var (gate, document) = loaded.Data!;

            if (document.Status != InpAdmissionDocumentStatus.Draft)
            {
                return InvalidState(document.Status, "diubah", document.Status == InpAdmissionDocumentStatus.Completed
                    ? " Buat versi koreksi."
                    : document.Status == InpAdmissionDocumentStatus.AwaitingSignature
                        ? " Buka kunci lebih dulu bila belum ada tanda tangan."
                        : string.Empty);
            }

            var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);
            var timeZone = InpAdmissionSourceReader.ResolveTimeZone(hospital);
            var now = DateTime.UtcNow;
            var today = InpAdmissionText.ToLocal(now, timeZone).Date;

            InpEpisodeDepositSummaryDto? depositSummary = null;

            if (document.DocumentType == InpAdmissionDocumentType.DepositSettlementStatement)
            {
                // Batas atas jatuh tempo butuh interval kebijakan; bila Billing tidak terbaca, batas
                // atas diperiksa ulang saat kunci (VAL-RWA-12 hanya pada simpan pertama dan kunci).
                var deposit = await _sourceReader.GetDepositSummaryAsync(episodeId, cancellationToken);
                depositSummary = deposit.Value;
            }

            var contentFailure = await ApplyContentAsync(
                document, request, isCreate: false, null, depositSummary, timeZone, today, actorUserId, now, cancellationToken);

            if (contentFailure != null)
            {
                return contentFailure;
            }

            Touch(document, actorUserId, now);

            var saveFailure = await SaveAsync(cancellationToken);

            return saveFailure ?? await ReloadResponseAsync(gate, document.Id, user, "Konsep dokumen admisi berhasil diubah.", cancellationToken);
        }

        // =====================================================================
        // Kunci (PATCH /documents/{id}/lock)
        // =====================================================================

        public async Task<InpAdmissionResult<AdmissionDocumentResponse>> LockAsync(
            Guid episodeId,
            Guid documentId,
            RowVersionRequest request,
            Guid actorUserId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var loaded = await LoadForCommandAsync(episodeId, documentId, request.RowVersion, cancellationToken);

            if (!loaded.IsSuccess)
            {
                return loaded.AsFailure<AdmissionDocumentResponse>();
            }

            var (gate, document) = loaded.Data!;

            if (document.Status != InpAdmissionDocumentStatus.Draft)
            {
                return InvalidState(document.Status, "dikunci");
            }

            var now = DateTime.UtcNow;
            InpEpisodeDepositSummaryDto? depositSummary = null;

            // Syarat jenis yang dibaca ulang saat kunci (VAL-RWA-10, 11, 12).
            if (document.DocumentType == InpAdmissionDocumentType.CostDifferenceStatement)
            {
                var payerFailure = await CheckCostDifferencePayerAsync(gate.EncounterId, cancellationToken);

                if (payerFailure != null)
                {
                    return payerFailure;
                }
            }

            if (document.DocumentType == InpAdmissionDocumentType.DepositSettlementStatement)
            {
                var deposit = await _sourceReader.GetDepositSummaryAsync(episodeId, cancellationToken);
                var depositFailure = CheckDepositShortfall(deposit);

                if (depositFailure != null)
                {
                    return depositFailure;
                }

                depositSummary = deposit.Value;
            }

            // Pihak bersumber relasi, kontak darurat, atau pasien dibaca ulang dari pemiliknya.
            var partyFailure = await RefreshPartyFromSourceAsync(document, actorUserId, now, cancellationToken);

            if (partyFailure != null)
            {
                return partyFailure;
            }

            var hospital = await _sourceReader.GetHospitalProfileAsync(cancellationToken);
            var errors = ValidateForLock(document, depositSummary, InpAdmissionSourceReader.ResolveTimeZone(hospital));

            if (errors.Count > 0)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Dokumen belum dapat dikunci. Lengkapi isian berikut: " + string.Join(" ", errors.Select(x => x.Message)),
                    errors[0].Code,
                    errors);
            }

            var build = await _snapshotBuilder.BuildAsync(
                episodeId,
                document.PatientId,
                gate.EncounterId,
                document.DocumentType,
                document.VersionNo,
                document.SigningCity,
                depositSummary,
                now,
                cancellationToken);

            if (!build.CanFreeze)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Data pasien atau profil rumah sakit tidak dapat dimuat. Dokumen belum dapat dikunci; coba lagi.",
                    InpAdmissionCodes.SnapshotSourceUnavailable);
            }

            if (document.DocumentType == InpAdmissionDocumentType.DepositSettlementStatement && depositSummary != null)
            {
                // RWI-DEC-263: angka dibekukan saat dikunci, bukan saat lengkap.
                var statement = document.DepositStatement!;
                statement.MinimumPolicyAmount = depositSummary.MinimumPolicyAmount;
                statement.ReceivedAmount = depositSummary.TotalReceived;
                statement.ShortfallAmount = depositSummary.PolicyShortfallAmount;
                statement.AmountsReadAt = now;
                statement.PolicyFollowUpIntervalDays = depositSummary.FollowUpIntervalDays;
                statement.UpdateDateTime = now;
                statement.UpdateBy = actorUserId;
            }

            document.Status = InpAdmissionDocumentStatus.AwaitingSignature;
            document.LockedAt = now;
            document.LockedByUserId = actorUserId;
            document.SnapshotFormatVersion = InpAdmissionSnapshotBuilder.CurrentFormatVersion;
            document.SnapshotJson = InpAdmissionSnapshotBuilder.Serialize(build.Snapshot);
            Touch(document, actorUserId, now);

            var saveFailure = await SaveAsync(cancellationToken);

            return saveFailure ?? await ReloadResponseAsync(gate, document.Id, user,
                "Dokumen dikunci. Cetak lembar untuk ditandatangani.", cancellationToken);
        }

        // =====================================================================
        // Buka kunci (PATCH /documents/{id}/unlock)
        // =====================================================================

        public async Task<InpAdmissionResult<AdmissionDocumentResponse>> UnlockAsync(
            Guid episodeId,
            Guid documentId,
            RowVersionRequest request,
            Guid actorUserId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var loaded = await LoadForCommandAsync(episodeId, documentId, request.RowVersion, cancellationToken);

            if (!loaded.IsSuccess)
            {
                return loaded.AsFailure<AdmissionDocumentResponse>();
            }

            var (gate, document) = loaded.Data!;

            if (document.Status != InpAdmissionDocumentStatus.AwaitingSignature)
            {
                return InvalidState(document.Status, "dibuka kuncinya");
            }

            if (document.Signatures.Any(x => !x.IsDelete))
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Kunci tidak dapat dibuka karena sudah ada tanda tangan. Minta supervisor admisi membatalkan dokumen bila isinya salah.",
                    InpAdmissionCodes.UnlockWithSignature);
            }

            var now = DateTime.UtcNow;

            // Salinan beku dan angka beku dibuang; isi konsep tetap.
            document.Status = InpAdmissionDocumentStatus.Draft;
            document.LockedAt = null;
            document.LockedByUserId = null;
            document.SnapshotFormatVersion = null;
            document.SnapshotJson = null;

            if (document.DepositStatement != null)
            {
                document.DepositStatement.MinimumPolicyAmount = null;
                document.DepositStatement.ReceivedAmount = null;
                document.DepositStatement.ShortfallAmount = null;
                document.DepositStatement.AmountsReadAt = null;
                document.DepositStatement.PolicyFollowUpIntervalDays = null;
                document.DepositStatement.UpdateDateTime = now;
                document.DepositStatement.UpdateBy = actorUserId;
            }

            Touch(document, actorUserId, now);

            var saveFailure = await SaveAsync(cancellationToken);

            return saveFailure ?? await ReloadResponseAsync(gate, document.Id, user, "Kunci dokumen dibuka.", cancellationToken);
        }

        // =====================================================================
        // Buang konsep (PATCH /documents/{id}/discard) dan batal (PATCH /documents/{id}/cancel)
        // =====================================================================

        public async Task<InpAdmissionResult<AdmissionDocumentResponse>> DiscardAsync(
            Guid episodeId,
            Guid documentId,
            ReasonRequest request,
            Guid actorUserId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var loaded = await LoadForCommandAsync(episodeId, documentId, request.RowVersion, cancellationToken);

            if (!loaded.IsSuccess)
            {
                return loaded.AsFailure<AdmissionDocumentResponse>();
            }

            var (gate, document) = loaded.Data!;

            if (document.Status != InpAdmissionDocumentStatus.Draft)
            {
                return InvalidState(document.Status, "dibuang", " Gunakan Batalkan.");
            }

            var reason = InpAdmissionText.Clean(request.Reason);

            if (reason == null)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(StatusCodes.Status400BadRequest, "Isi alasan membuang konsep.");
            }

            if (reason.Length > 500)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(StatusCodes.Status400BadRequest, "Alasan maksimal 500 karakter.");
            }

            // VAL-RWA-08: kepemilikan konsep diturunkan dari pembuatnya, bukan dari nama peran.
            if (document.CreateBy != actorUserId)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Hanya pembuat konsep yang dapat membuangnya. Minta supervisor admisi membatalkan dokumen ini.",
                    InpAdmissionCodes.DiscardNotCreator);
            }

            var now = DateTime.UtcNow;
            document.Status = InpAdmissionDocumentStatus.Cancelled;
            document.CancelledAt = now;
            document.CancelledByUserId = actorUserId;
            document.CancelledReason = reason;
            Touch(document, actorUserId, now);

            var saveFailure = await SaveAsync(cancellationToken);

            return saveFailure ?? await ReloadResponseAsync(gate, document.Id, user, "Konsep dokumen dibuang.", cancellationToken);
        }

        public async Task<InpAdmissionResult<AdmissionDocumentResponse>> CancelAsync(
            Guid episodeId,
            Guid documentId,
            ReasonRequest request,
            Guid actorUserId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var loaded = await LoadForCommandAsync(episodeId, documentId, request.RowVersion, cancellationToken);

            if (!loaded.IsSuccess)
            {
                return loaded.AsFailure<AdmissionDocumentResponse>();
            }

            var (gate, document) = loaded.Data!;

            if (document.Status is not (InpAdmissionDocumentStatus.Draft
                or InpAdmissionDocumentStatus.AwaitingSignature
                or InpAdmissionDocumentStatus.Completed))
            {
                return InvalidState(document.Status, "dibatalkan");
            }

            var reason = InpAdmissionText.Clean(request.Reason);

            if (reason == null || reason.Length < 10)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Alasan pembatalan minimal 10 karakter.");
            }

            if (reason.Length > 500)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(StatusCodes.Status400BadRequest, "Alasan maksimal 500 karakter.");
            }

            var now = DateTime.UtcNow;
            document.Status = InpAdmissionDocumentStatus.Cancelled;
            document.CancelledAt = now;
            document.CancelledByUserId = actorUserId;
            document.CancelledReason = reason;
            Touch(document, actorUserId, now);

            var saveFailure = await SaveAsync(cancellationToken);

            return saveFailure ?? await ReloadResponseAsync(gate, document.Id, user,
                "Dokumen dibatalkan. Dokumen baru sejenis boleh dibuat.", cancellationToken);
        }

        // =====================================================================
        // Versi koreksi (POST /documents/{id}/revisions)
        // =====================================================================

        /// <remarks>
        /// Berurutan dalam satu transaksi: (1) versi lama <c>Completed</c> → <c>Superseded</c>,
        /// (2) versi baru <c>Draft</c> dibuat dari isi versi lama tanpa tanda tangan dan tanpa salinan
        /// beku. Urutan ini wajib karena unique index dokumen aktif.
        /// </remarks>
        public async Task<InpAdmissionResult<AdmissionDocumentResponse>> ReviseAsync(
            Guid episodeId,
            Guid documentId,
            ReviseAdmissionDocumentRequest request,
            string? idempotencyKey,
            Guid actorUserId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var key = NormalizeIdempotencyKey(idempotencyKey, out var keyInvalid);

            if (keyInvalid)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(StatusCodes.Status400BadRequest, "Kunci permintaan tidak sah.");
            }

            if (key != null)
            {
                var replay = await ReplayByKeyAsync(episodeId, key, null, documentId, user, cancellationToken);

                if (replay != null)
                {
                    return replay;
                }
            }

            var loaded = await LoadForCommandAsync(episodeId, documentId, request.RowVersion, cancellationToken);

            if (!loaded.IsSuccess)
            {
                return loaded.AsFailure<AdmissionDocumentResponse>();
            }

            var (gate, previous) = loaded.Data!;

            if (previous.Status != InpAdmissionDocumentStatus.Completed)
            {
                return InvalidState(previous.Status, "dibuat versi koreksinya");
            }

            var correctionReason = InpAdmissionText.Clean(request.CorrectionReason);

            if (correctionReason == null || correctionReason.Length < 10)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status400BadRequest, "Alasan koreksi minimal 10 karakter.");
            }

            if (correctionReason.Length > 500)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(StatusCodes.Status400BadRequest, "Alasan maksimal 500 karakter.");
            }

            var now = DateTime.UtcNow;
            var revision = CopyForRevision(previous, correctionReason, key, actorUserId, now);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                previous.Status = InpAdmissionDocumentStatus.Superseded;
                previous.SupersededAt = now;
                Touch(previous, actorUserId, now);
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
            catch (DbUpdateException exception) when (IsUniqueViolation(exception, out var constraint))
            {
                await transaction.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();

                if (constraint == IdempotencyIndexName && key != null)
                {
                    var replay = await ReplayByKeyAsync(episodeId, key, null, documentId, user, cancellationToken);

                    if (replay != null)
                    {
                        return replay;
                    }
                }

                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Versi koreksi untuk dokumen ini sudah dibuat petugas lain. Muat ulang dokumen.",
                    InpAdmissionCodes.InvalidDocumentState);
            }

            return await ReloadResponseAsync(gate, revision.Id, user,
                $"Versi koreksi {revision.VersionNo} dibuat; versi {previous.VersionNo} kini digantikan.", cancellationToken);
        }

        // =====================================================================
        // Pembantu bersama
        // =====================================================================

        /// <summary>Dokumen beserta seluruh anaknya; harus milik episode itu.</summary>
        internal Task<InpAdmissionDocument?> LoadAsync(
            Guid episodeId,
            Guid documentId,
            bool tracked,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<InpAdmissionDocument>()
                .Include(x => x.Party)
                .Include(x => x.PrivacyRequest)
                .Include(x => x.CostDifferenceStatement)
                .Include(x => x.DepositStatement)
                .Include(x => x.Signatures)
                .Include(x => x.HandoverItems)
                .Include(x => x.PrivacyEntries)
                .Include(x => x.BeliefItems)
                .AsSplitQuery()
                .Where(x => x.Id == documentId && x.EpisodeId == episodeId && !x.IsDelete);

            if (!tracked)
            {
                query = query.AsNoTracking();
            }

            return query.FirstOrDefaultAsync(cancellationToken);
        }

        /// <summary>
        /// Penjaga bersama setiap perintah atas dokumen yang sudah ada: episode berjalan, dokumen
        /// milik episode, dan <c>RowVersion</c> cocok (<c>VAL-RWA-01</c>, <c>02</c>, <c>04</c>).
        /// </summary>
        internal async Task<InpAdmissionResult<(InpAdmissionEpisodeGate Gate, InpAdmissionDocument Document)>> LoadForCommandAsync(
            Guid episodeId,
            Guid documentId,
            Guid? rowVersion,
            CancellationToken cancellationToken)
        {
            var gate = await _writeGuard.EnsureWritableAsync(episodeId, cancellationToken);

            if (!gate.IsSuccess)
            {
                return gate.AsFailure<(InpAdmissionEpisodeGate, InpAdmissionDocument)>();
            }

            var document = await LoadAsync(episodeId, documentId, tracked: true, cancellationToken);

            if (document == null)
            {
                return InpAdmissionResult<(InpAdmissionEpisodeGate, InpAdmissionDocument)>.Fail(
                    StatusCodes.Status404NotFound, NotFoundMessage);
            }

            if (!rowVersion.HasValue || rowVersion.Value == Guid.Empty)
            {
                return InpAdmissionResult<(InpAdmissionEpisodeGate, InpAdmissionDocument)>.Fail(
                    StatusCodes.Status400BadRequest, "RowVersion dokumen wajib dikirim.");
            }

            if (document.RowVersion != rowVersion.Value)
            {
                return InpAdmissionResult<(InpAdmissionEpisodeGate, InpAdmissionDocument)>.Fail(
                    StatusCodes.Status409Conflict, StaleMessage, InpAdmissionCodes.StaleRowVersion);
            }

            return InpAdmissionResult<(InpAdmissionEpisodeGate, InpAdmissionDocument)>.Ok((gate.Data!, document), string.Empty);
        }

        /// <summary>Simpan dengan penjaga konkurensi; kegagalan <c>RowVersion</c> menjadi <c>409 INP-ADM-DOC-004</c>.</summary>
        internal async Task<InpAdmissionResult<AdmissionDocumentResponse>?> SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return null;
            }
            catch (DbUpdateConcurrencyException)
            {
                _dbContext.ChangeTracker.Clear();
                return Stale();
            }
        }

        internal async Task<InpAdmissionResult<AdmissionDocumentResponse>> ReloadResponseAsync(
            InpAdmissionEpisodeGate gate,
            Guid documentId,
            ClaimsPrincipal user,
            string message,
            CancellationToken cancellationToken)
        {
            var reloaded = await LoadAsync(gate.EpisodeId, documentId, tracked: false, cancellationToken);

            if (reloaded == null)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(StatusCodes.Status404NotFound, NotFoundMessage);
            }

            return InpAdmissionResult<AdmissionDocumentResponse>.Ok(
                await BuildResponseAsync(reloaded, gate, user, cancellationToken),
                message);
        }

        /// <summary>Kunci idempoten yang pernah dipakai mengembalikan dokumen yang sama.</summary>
        private async Task<InpAdmissionResult<AdmissionDocumentResponse>?> ReplayByKeyAsync(
            Guid episodeId,
            string key,
            InpAdmissionDocumentType? expectedType,
            Guid? expectedPreviousVersionId,
            ClaimsPrincipal user,
            CancellationToken cancellationToken)
        {
            var existing = await _dbContext.Set<InpAdmissionDocument>()
                .AsNoTracking()
                .Where(x => x.IdempotencyKey == key && !x.IsDelete)
                .Select(x => new { x.Id, x.EpisodeId, x.DocumentType, x.PreviousVersionId })
                .FirstOrDefaultAsync(cancellationToken);

            if (existing == null)
            {
                return null;
            }

            var sameIntent = existing.EpisodeId == episodeId &&
                (!expectedType.HasValue || existing.DocumentType == expectedType.Value) &&
                (!expectedPreviousVersionId.HasValue || existing.PreviousVersionId == expectedPreviousVersionId.Value);

            if (!sameIntent)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status409Conflict, "Kunci permintaan sudah dipakai untuk permintaan lain.");
            }

            var gate = await _writeGuard.LoadAsync(episodeId, cancellationToken);

            return gate == null
                ? InpAdmissionResult<AdmissionDocumentResponse>.Fail(StatusCodes.Status404NotFound, InpAdmissionWriteGuard.EpisodeNotFoundMessage)
                : await ReloadResponseAsync(gate, existing.Id, user, "Permintaan yang sama sudah tersimpan sebelumnya.", cancellationToken);
        }

        private Task<bool> HasActiveDocumentAsync(
            Guid episodeId,
            InpAdmissionDocumentType documentType,
            Guid? excludeId,
            CancellationToken cancellationToken)
            => _dbContext.Set<InpAdmissionDocument>()
                .AsNoTracking()
                .AnyAsync(x => x.EpisodeId == episodeId && x.DocumentType == documentType && !x.IsDelete &&
                    (!excludeId.HasValue || x.Id != excludeId.Value) &&
                    (x.Status == InpAdmissionDocumentStatus.Draft ||
                     x.Status == InpAdmissionDocumentStatus.AwaitingSignature ||
                     x.Status == InpAdmissionDocumentStatus.Completed), cancellationToken);

        /// <summary>VAL-RWA-10: Selisih Biaya hanya untuk penjamin utama asuransi atau perusahaan.</summary>
        private async Task<InpAdmissionResult<AdmissionDocumentResponse>?> CheckCostDifferencePayerAsync(
            Guid encounterId,
            CancellationToken cancellationToken)
        {
            var guarantor = await _sourceReader.GetGuarantorAsync(encounterId, cancellationToken);

            if (!guarantor.IsAvailable)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Data penjamin kunjungan tidak dapat dibaca. Selisih Biaya belum dapat diproses; coba lagi.");
            }

            // RWI-DEC-256: penjamin yang melarang selisih dibebankan tetap wajib dan tetap boleh dibuat.
            return guarantor.Value!.PaymentType is EncounterPaymentType.Insurance or EncounterPaymentType.CompanyGuarantor
                ? null
                : InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Selisih Biaya hanya untuk pasien dengan penjamin asuransi atau perusahaan.",
                    InpAdmissionCodes.CostDifferencePayer);
        }

        /// <summary>VAL-RWA-11, 12: angka deposit harus terbaca dari Billing dan masih ada kekurangan.</summary>
        private static InpAdmissionResult<AdmissionDocumentResponse>? CheckDepositShortfall(
            InpAdmissionSourceResult<InpEpisodeDepositSummaryDto> deposit)
        {
            if (!deposit.IsAvailable)
            {
                return InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Data deposit tidak dapat dibaca dari kasir. Coba lagi beberapa saat.",
                    InpAdmissionCodes.DepositUnavailable);
            }

            return deposit.Value!.PolicyShortfallAmount > 0
                ? null
                : InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    "Deposit episode ini sudah memenuhi kebijakan. Surat pelunasan tidak diperlukan.",
                    InpAdmissionCodes.NoDepositShortfall);
        }

        private static InpAdmissionResult<AdmissionDocumentResponse> ActiveExists(InpAdmissionDocumentType documentType)
            => InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                StatusCodes.Status409Conflict,
                $"Sudah ada {InpAdmissionDocumentRules.DocumentName(documentType)} yang aktif untuk episode ini. Buka dokumen itu atau buat versi koreksi.",
                InpAdmissionCodes.ActiveDocumentExists);

        internal static InpAdmissionResult<AdmissionDocumentResponse> InvalidState(
            InpAdmissionDocumentStatus status,
            string action,
            string suffix = "")
            => InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                StatusCodes.Status409Conflict,
                $"Dokumen berstatus {InpAdmissionDocumentRules.StatusName(status)} tidak dapat {action}.{suffix}",
                InpAdmissionCodes.InvalidDocumentState);

        internal static InpAdmissionResult<AdmissionDocumentResponse> Stale()
            => InpAdmissionResult<AdmissionDocumentResponse>.Fail(
                StatusCodes.Status409Conflict, StaleMessage, InpAdmissionCodes.StaleRowVersion);

        internal static void Touch(InpAdmissionDocument document, Guid actorUserId, DateTime now)
        {
            document.RowVersion = Guid.NewGuid();
            document.UpdateDateTime = now;
            document.UpdateBy = actorUserId;
        }

        /// <summary>Kunci idempoten maksimal 80 karakter (<c>VAL-RWA-09</c>); kosong berarti tanpa kunci.</summary>
        internal static string? NormalizeIdempotencyKey(string? value, out bool invalid)
        {
            invalid = false;

            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var trimmed = value.Trim();

            if (trimmed.Length > IdempotencyKeyMaxLength)
            {
                invalid = true;
                return null;
            }

            return trimmed;
        }

        internal static bool IsUniqueViolation(DbUpdateException exception, out string? constraintName)
        {
            if (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } postgres)
            {
                constraintName = postgres.ConstraintName;
                return true;
            }

            constraintName = null;
            return false;
        }

        private static DateTime AsDate(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);

        internal static AdmissionDepositAmountsResponse BuildDepositAmounts(
            decimal? minimum,
            decimal? received,
            decimal? shortfall,
            DateTime? readAt,
            bool isFrozen)
            => new()
            {
                MinimumPolicyAmount = minimum,
                ReceivedAmount = received,
                ShortfallAmount = shortfall,
                ReadAt = readAt,
                IsFrozen = isFrozen,
                CalculationText = shortfall.HasValue && minimum.HasValue && received.HasValue
                    ? $"{InpAdmissionText.FormatRupiah(shortfall.Value)} ({InpAdmissionText.FormatRupiah(minimum.Value)} − {InpAdmissionText.FormatRupiah(received.Value)})"
                    : null
            };

        /// <summary>Isi versi baru = isi versi lama tanpa tanda tangan, salinan beku, maupun angka beku.</summary>
        private static InpAdmissionDocument CopyForRevision(
            InpAdmissionDocument previous,
            string correctionReason,
            string? idempotencyKey,
            Guid actorUserId,
            DateTime now)
        {
            var revision = new InpAdmissionDocument
            {
                Id = Guid.NewGuid(),
                EpisodeId = previous.EpisodeId,
                PatientId = previous.PatientId,
                DocumentType = previous.DocumentType,
                Status = InpAdmissionDocumentStatus.Draft,
                VersionNo = previous.VersionNo + 1,
                PreviousVersionId = previous.Id,
                CorrectionReason = correctionReason,
                SigningCity = previous.SigningCity,
                StatementDate = previous.StatementDate,
                Note = previous.Note,
                IdempotencyKey = idempotencyKey,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            if (previous.Party != null)
            {
                var p = previous.Party;
                revision.Party = new InpAdmissionDocumentParty
                {
                    Id = Guid.NewGuid(),
                    DocumentId = revision.Id,
                    SourceType = p.SourceType,
                    SourceRecordId = p.SourceRecordId,
                    FullName = p.FullName,
                    Relationship = p.Relationship,
                    RelationshipText = p.RelationshipText,
                    Address = p.Address,
                    BirthDate = p.BirthDate,
                    Gender = p.Gender,
                    Occupation = p.Occupation,
                    IdentityType = p.IdentityType,
                    IdentityNumber = p.IdentityNumber,
                    MobilePhone = p.MobilePhone,
                    OfficePhone = p.OfficePhone,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
            }

            foreach (var item in previous.HandoverItems.Where(x => !x.IsDelete).OrderBy(x => x.LineNo))
            {
                revision.HandoverItems.Add(new InpAdmissionHandoverItem
                {
                    Id = Guid.NewGuid(),
                    DocumentId = revision.Id,
                    ClearanceItemId = item.ClearanceItemId,
                    LineNo = item.LineNo,
                    ItemNumberSnapshot = item.ItemNumberSnapshot,
                    ParentItemNumberSnapshot = item.ParentItemNumberSnapshot,
                    ItemCodeSnapshot = item.ItemCodeSnapshot,
                    ItemNameSnapshot = item.ItemNameSnapshot,
                    Choice = item.Choice,
                    Note = item.Note,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            if (previous.PrivacyRequest != null)
            {
                revision.PrivacyRequest = new InpAdmissionPrivacyRequest
                {
                    Id = Guid.NewGuid(),
                    DocumentId = revision.Id,
                    IsTransportPrivacyRequested = previous.PrivacyRequest.IsTransportPrivacyRequested,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
            }

            foreach (var entry in previous.PrivacyEntries.Where(x => !x.IsDelete))
            {
                revision.PrivacyEntries.Add(new InpAdmissionPrivacyEntry
                {
                    Id = Guid.NewGuid(),
                    DocumentId = revision.Id,
                    EntryType = entry.EntryType,
                    LineNo = entry.LineNo,
                    Text = entry.Text,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            foreach (var belief in previous.BeliefItems.Where(x => !x.IsDelete))
            {
                revision.BeliefItems.Add(new InpAdmissionBeliefItem
                {
                    Id = Guid.NewGuid(),
                    DocumentId = revision.Id,
                    ItemNo = belief.ItemNo,
                    Text = belief.Text,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            if (previous.CostDifferenceStatement != null)
            {
                revision.CostDifferenceStatement = new InpAdmissionCostDifferenceStatement
                {
                    Id = Guid.NewGuid(),
                    DocumentId = revision.Id,
                    Subject = previous.CostDifferenceStatement.Subject,
                    SubjectOtherText = previous.CostDifferenceStatement.SubjectOtherText,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
            }

            if (previous.DepositStatement != null)
            {
                revision.DepositStatement = new InpAdmissionDepositStatement
                {
                    Id = Guid.NewGuid(),
                    DocumentId = revision.Id,
                    DueAt = previous.DepositStatement.DueAt,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
            }

            return revision;
        }
    }
}
