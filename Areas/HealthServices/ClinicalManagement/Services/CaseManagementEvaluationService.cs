using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Evaluasi Awal Manajer Pelayanan Pasien — <c>BE-RWI-113</c>, <c>FR-KEP-053</c> s.d. <c>FR-KEP-055</c>,
    /// <c>RWI-DEC-118</c>, <c>RWI-DEC-131</c>, <c>RWI-DEC-150</c> (usulan <c>G-09</c>), state matrix 0.5.0 bagian 5.3.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Proses bisnisnya.</b> Ns. Dewi, MPP Bangsal Melati, membuat konsep Evaluasi Awal Budi, mengisi
    /// delapan bagian checklist, lalu menyelesaikannya. Ns. Siti (perawat pelaksana) membaca dokumen yang
    /// sama tanpa tombol ubah. <b>Satu dokumen hidup per episode</b>: konsep atau dokumen selesai kedua
    /// ditolak <c>409</c> "Pasien ini sudah punya Evaluasi Awal. Lengkapi lewat addendum."
    /// </para>
    /// <para>
    /// <b>Dua penjaga yang sama-sama wajib</b> (<c>VAL-KEP-23a</c>). Butir hak akses
    /// <c>CaseManagementEvaluation : Create</c>/<c>Update</c> — penanda MPP yang diatur layar Akses Role —
    /// <b>dan</b> penempatan di unit episode saat simpan. Budi dipindah ke Anggrek pukul 14.00; konsep yang
    /// Dewi simpan pukul 14.10 ditolak <c>403</c>. Tidak ada nama peran yang dibaca kode.
    /// </para>
    /// <para>
    /// <b>Addendum belum tersedia</b> dan itu keadaan yang diketahui, bukan cacat: jenis dokumen
    /// <c>CaseManagementEvaluation = 14</c> pada mesin keutuhan rekam medis masih diminta kepada pemilik
    /// <c>MedicalRecordManagement</c> (<c>INT-KEP-12</c>). Jalur addendum menjawab <c>501</c>.
    /// </para>
    /// </remarks>
    public class CaseManagementEvaluationService
    {
        private const string LogCategory = "HealthServices.Clinical.CaseManagementEvaluation";
        private const string SequenceKey = "CLI_CASE_MANAGEMENT_EVALUATION";
        private const string NumberPrefix = "MPP";

        public const string PenolakanUnitMpp = "Evaluasi Awal hanya ditulis MPP yang ditempatkan di unit pasien ini.";
        public const string AlasanAddendumBelumTersedia =
            "Addendum Evaluasi Awal belum tersedia: jenis dokumen Evaluasi Awal pada mesin keutuhan rekam medis masih menunggu persetujuan pemilik MedicalRecordManagement (INT-KEP-12).";

        private static readonly TimeSpan ToleransiJejakWaktu = TimeSpan.FromMilliseconds(1);

        private readonly ApplicationDbContext _dbContext;
        private readonly NursingEpisodeWriteGuard _writeGuard;
        private readonly ClinicalInstrumentService _instrumentService;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly LoggerService _loggerService;

        public CaseManagementEvaluationService(
            ApplicationDbContext dbContext,
            NursingEpisodeWriteGuard writeGuard,
            ClinicalInstrumentService instrumentService,
            NumberSeriesAllocator numberSeriesAllocator,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _writeGuard = writeGuard;
            _instrumentService = instrumentService;
            _numberSeriesAllocator = numberSeriesAllocator;
            _loggerService = loggerService;
        }

        // =====================================================================
        // Baca
        // =====================================================================

        /// <summary>Evaluasi Awal hidup satu episode; <c>null</c> bila belum ada — bukan galat.</summary>
        public async Task<CaseManagementEvaluationResponse?> GetByEpisodeAsync(Guid episodeId, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var dokumen = await _dbContext.Set<CliCaseManagementEvaluation>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete && x.EvaluationStatus != CaseManagementEvaluationStatus.Cancelled)
                .OrderByDescending(x => x.CreateDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            return dokumen == null ? null : await ToResponseAsync(dokumen, actorUserId, false, cancellationToken);
        }

        public async Task<NursingResult<CaseManagementEvaluationResponse>> GetAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var dokumen = await _dbContext.Set<CliCaseManagementEvaluation>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            return dokumen == null
                ? NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status404NotFound, "Evaluasi Awal tidak ditemukan.")
                : NursingResult<CaseManagementEvaluationResponse>.Ok(await ToResponseAsync(dokumen, actorUserId, false, cancellationToken), "Evaluasi Awal berhasil diambil.");
        }

        public Task<NursingResult<ResolvedInstrumentResponse>> ResolveChecklistAsync(Guid episodeId, CancellationToken cancellationToken = default) =>
            _instrumentService.ResolveForEpisodeAsync(ClinicalInstrumentKind.CaseManagementChecklist, episodeId, cancellationToken);

        // =====================================================================
        // Tulis
        // =====================================================================

        public async Task<NursingResult<CaseManagementEvaluationResponse>> CreateAsync(
            CreateCaseManagementEvaluationRequest request,
            string? idempotencyKey,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var kunci = NursingEpisodeWriteGuard.NormalizeIdempotencyKey(idempotencyKey ?? request.IdempotencyKey);

            if (kunci != null)
            {
                var sudahAda = await _dbContext.Set<CliCaseManagementEvaluation>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IdempotencyKey == kunci && !x.IsDelete, cancellationToken);

                if (sudahAda != null)
                    return NursingResult<CaseManagementEvaluationResponse>.Ok(await ToResponseAsync(sudahAda, actorUserId, true, cancellationToken), "Evaluasi Awal sudah tercatat sebelumnya.", isReplay: true);
            }

            var penjaga = await _writeGuard.EnsureCanWriteAsync(request.EpisodeId, user, actorUserId, cancellationToken, PenolakanUnitMpp);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<CaseManagementEvaluationResponse>();

            var konteks = penjaga.Value!;

            // VAL-KEP-23b — satu dokumen hidup per episode.
            var adaHidup = await _dbContext.Set<CliCaseManagementEvaluation>().AsNoTracking()
                .AnyAsync(x => x.InpEpisodeId == konteks.EpisodeId && !x.IsDelete &&
                               (x.EvaluationStatus == CaseManagementEvaluationStatus.Draft || x.EvaluationStatus == CaseManagementEvaluationStatus.Completed),
                    cancellationToken);

            if (adaHidup)
                return Conflict("Pasien ini sudah punya Evaluasi Awal. Lengkapi lewat addendum.", "EVALUATION_ALREADY_EXISTS");

            var now = DateTime.UtcNow;
            var dokumen = new CliCaseManagementEvaluation
            {
                Id = Guid.NewGuid(),
                EncounterId = konteks.EncounterId,
                InpEpisodeId = konteks.EpisodeId,
                PatientId = konteks.PatientId,
                ServiceUnitIdSnapshot = konteks.ServiceUnitId,
                EvaluationStatus = CaseManagementEvaluationStatus.Draft,
                AuthorEmployeeId = konteks.ActorEmployeeId,
                AuthorUserId = actorUserId,
                ClinicalDateTime = request.ClinicalDateTime ?? now,
                IdempotencyKey = kunci,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            if (dokumen.ClinicalDateTime > now.AddMinutes(5))
                return NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status400BadRequest, "Waktu klinis tidak boleh di masa depan.");

            var terapan = await ApplyChecklistAsync(dokumen, request.InstrumentVersionId, request.Responses, actorUserId, now, cancellationToken);

            if (terapan != null)
                return terapan;

            try
            {
                dokumen.EvaluationNumber = await _numberSeriesAllocator.AllocateAsync(
                    new NumberAllocationRequest(SequenceKey, NumberPrefix, NumberSeriesResetPolicies.Daily, 4, actorUserId, DateTimeOffset.UtcNow),
                    cancellationToken);
            }
            catch (NumberSeriesAllocationException)
            {
                return NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status422UnprocessableEntity,
                    "Nomor Evaluasi Awal gagal diterbitkan. Hubungi administrator sistem.");
            }

            _dbContext.Set<CliCaseManagementEvaluation>().Add(dokumen);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException) when (kunci != null)
            {
                _dbContext.ChangeTracker.Clear();

                var pemenang = await _dbContext.Set<CliCaseManagementEvaluation>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.IdempotencyKey == kunci && !x.IsDelete, cancellationToken);

                return pemenang != null
                    ? NursingResult<CaseManagementEvaluationResponse>.Ok(await ToResponseAsync(pemenang, actorUserId, true, cancellationToken), "Evaluasi Awal sudah tercatat sebelumnya.", isReplay: true)
                    : Conflict("Pasien ini sudah punya Evaluasi Awal. Lengkapi lewat addendum.", "EVALUATION_ALREADY_EXISTS");
            }
            catch (DbUpdateException)
            {
                _dbContext.ChangeTracker.Clear();
                return Conflict("Pasien ini sudah punya Evaluasi Awal. Lengkapi lewat addendum.", "EVALUATION_ALREADY_EXISTS");
            }

            await _loggerService.InfoAsync(LogCategory, "CaseManagementEvaluation.Create", "MPP membuat konsep Evaluasi Awal.",
                new { dokumen.Id, dokumen.EvaluationNumber, dokumen.InpEpisodeId, dokumen.AuthorEmployeeId });

            return NursingResult<CaseManagementEvaluationResponse>.Ok(await ToResponseAsync(dokumen, actorUserId, false, cancellationToken), "Konsep Evaluasi Awal berhasil dibuat.", StatusCodes.Status201Created);
        }

        public async Task<NursingResult<CaseManagementEvaluationResponse>> UpdateAsync(
            Guid id,
            UpdateCaseManagementEvaluationRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var (dokumen, gagal) = await LoadDraftForAuthorAsync(id, user, actorUserId, cancellationToken);

            if (gagal != null)
                return gagal;

            var tersimpan = dokumen!.UpdateDateTime ?? dokumen.CreateDateTime;

            if (!request.ExpectedUpdateDate.HasValue)
                return NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status400BadRequest, "Muat ulang Evaluasi Awal sebelum menyunting.", "DETAIL_NOT_LOADED");

            if ((tersimpan - request.ExpectedUpdateDate.Value).Duration() > ToleransiJejakWaktu)
                return Conflict("Data sudah diubah pengguna lain. Muat ulang.", "STALE_EVALUATION");

            var now = DateTime.UtcNow;

            if (request.ClinicalDateTime.HasValue)
            {
                if (request.ClinicalDateTime.Value > now.AddMinutes(5))
                    return NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status400BadRequest, "Waktu klinis tidak boleh di masa depan.");

                dokumen.ClinicalDateTime = request.ClinicalDateTime.Value;
            }

            var terapan = await ApplyChecklistAsync(dokumen, request.InstrumentVersionId, request.Responses, actorUserId, now, cancellationToken);

            if (terapan != null)
                return terapan;

            dokumen.UpdateDateTime = now;
            dokumen.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return NursingResult<CaseManagementEvaluationResponse>.Ok(await ToResponseAsync(dokumen, actorUserId, false, cancellationToken), "Konsep Evaluasi Awal berhasil disimpan.");
        }

        public async Task<NursingResult<CaseManagementEvaluationResponse>> CompleteAsync(
            Guid id,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var (dokumen, gagal) = await LoadDraftForAuthorAsync(id, user, actorUserId, cancellationToken);

            if (gagal != null)
                return gagal;

            var baris = await _dbContext.Set<CliAssessmentInstrumentResponse>()
                .FirstOrDefaultAsync(x => x.CaseManagementEvaluationId == dokumen!.Id && !x.IsDelete, cancellationToken);

            CliClinicalInstrumentVersion? versi;
            Dictionary<string, JsonElement> jawaban;

            if (baris != null)
            {
                versi = await _dbContext.Set<CliClinicalInstrumentVersion>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == baris.InstrumentVersionId, cancellationToken);
                jawaban = ClinicalInstrumentDefinitionEngine.ParseResponses(baris.ResponsesJson);
            }
            else
            {
                var berlaku = await ResolveChecklistAsync(dokumen!.InpEpisodeId, cancellationToken);

                if (!berlaku.IsSuccess)
                    return NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status422UnprocessableEntity, "Checklist Evaluasi Awal belum tersedia.", "CHECKLIST_NOT_AVAILABLE");

                versi = await _dbContext.Set<CliClinicalInstrumentVersion>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == berlaku.Value!.VersionId, cancellationToken);
                jawaban = new Dictionary<string, JsonElement>();
            }

            if (versi == null || (versi.VersionStatus != ClinicalInstrumentVersionStatus.Approved && !_instrumentService.IsDraftAllowedInThisEnvironment))
                return NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status422UnprocessableEntity, "Checklist Evaluasi Awal belum disahkan.", "CHECKLIST_NOT_APPROVED");

            var definisi = ClinicalInstrumentDefinitionEngine.Parse(versi.DefinitionJson, out var galat);

            if (definisi == null)
                return NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status422UnprocessableEntity, galat ?? "Checklist tidak dapat dibaca.");

            var kosong = ClinicalInstrumentDefinitionEngine.MissingRequiredItems(definisi, jawaban, _ => false);

            if (kosong.Count > 0)
                return NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status422UnprocessableEntity, $"Isian wajib belum lengkap: {string.Join(", ", kosong)}.", "REQUIRED_ITEMS_MISSING");

            var now = DateTime.UtcNow;
            dokumen!.EvaluationStatus = CaseManagementEvaluationStatus.Completed;
            dokumen.CompletedAt = now;
            dokumen.CompletedByUserId = actorUserId;
            dokumen.UpdateDateTime = now;
            dokumen.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "CaseManagementEvaluation.Complete", "MPP menyelesaikan Evaluasi Awal.",
                new { dokumen.Id, dokumen.EvaluationNumber, CompletedBy = actorUserId });

            return NursingResult<CaseManagementEvaluationResponse>.Ok(await ToResponseAsync(dokumen, actorUserId, false, cancellationToken), "Evaluasi Awal berhasil diselesaikan.");
        }

        public async Task<NursingResult<CaseManagementEvaluationResponse>> CancelAsync(
            Guid id,
            string? reason,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(reason))
                return NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status400BadRequest, "Alasan pembatalan wajib diisi.");

            var (dokumen, gagal) = await LoadDraftForAuthorAsync(id, user, actorUserId, cancellationToken);

            if (gagal != null)
                return gagal;

            var now = DateTime.UtcNow;
            dokumen!.EvaluationStatus = CaseManagementEvaluationStatus.Cancelled;
            dokumen.CancelledAt = now;
            dokumen.CancelledByUserId = actorUserId;
            dokumen.CancelReason = reason.Trim();
            dokumen.IsCancel = true;
            dokumen.CancelDateTime = now;
            dokumen.CancelBy = actorUserId;
            dokumen.UpdateDateTime = now;
            dokumen.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            // CancelReason sensitif — tidak masuk payload logger.
            await _loggerService.InfoAsync(LogCategory, "CaseManagementEvaluation.Cancel", "MPP membatalkan konsep Evaluasi Awal.",
                new { dokumen.Id, dokumen.EvaluationNumber, CancelledBy = actorUserId });

            return NursingResult<CaseManagementEvaluationResponse>.Ok(await ToResponseAsync(dokumen, actorUserId, false, cancellationToken), "Konsep Evaluasi Awal berhasil dibatalkan.");
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>
        /// Memuat konsep yang hendak diubah, diselesaikan, atau dibatalkan beserta seluruh penjaganya:
        /// konsep (<c>409</c>), penulis konsep (<c>403</c> <c>VAL-KEP-23c</c>), dan penempatan unit (<c>403</c>).
        /// </summary>
        private async Task<(CliCaseManagementEvaluation? Dokumen, NursingResult<CaseManagementEvaluationResponse>? Gagal)> LoadDraftForAuthorAsync(
            Guid id,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var dokumen = await _dbContext.Set<CliCaseManagementEvaluation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (dokumen == null)
                return (null, NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status404NotFound, "Evaluasi Awal tidak ditemukan."));

            if (dokumen.EvaluationStatus != CaseManagementEvaluationStatus.Draft)
            {
                return (null, Conflict(dokumen.EvaluationStatus == CaseManagementEvaluationStatus.Completed
                    ? "Evaluasi Awal yang sudah selesai dilengkapi lewat addendum."
                    : "Evaluasi Awal yang sudah dibatalkan tidak dapat diubah. Buat dokumen baru.", "EVALUATION_NOT_DRAFT"));
            }

            if (dokumen.AuthorUserId != actorUserId)
                return (null, NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status403Forbidden, "Konsep ini ditulis MPP lain.", "NOT_DOCUMENT_AUTHOR"));

            var penjaga = await _writeGuard.EnsureCanWriteAsync(dokumen.InpEpisodeId, user, actorUserId, cancellationToken, PenolakanUnitMpp);

            if (!penjaga.IsSuccess)
                return (null, penjaga.Cast<CaseManagementEvaluationResponse>());

            return (dokumen, null);
        }

        /// <summary>Menyimpan jawaban checklist bila dikirim — versi wajib versi yang berlaku (<c>VAL-KEP-21c</c>).</summary>
        private async Task<NursingResult<CaseManagementEvaluationResponse>?> ApplyChecklistAsync(
            CliCaseManagementEvaluation dokumen,
            Guid? versionId,
            Dictionary<string, JsonElement>? responses,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken)
        {
            if (responses == null)
                return null;

            var berlaku = await _instrumentService.ResolveForPatientAsync(ClinicalInstrumentKind.CaseManagementChecklist, dokumen.PatientId, now, cancellationToken);

            if (!berlaku.IsSuccess)
                return NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status422UnprocessableEntity, "Checklist Evaluasi Awal belum tersedia.", "CHECKLIST_NOT_AVAILABLE");

            if (!versionId.HasValue || versionId.Value != berlaku.Value!.VersionId)
                return Conflict("Formulir sudah diganti versi baru. Muat ulang formulir; isian Anda tetap tersimpan di layar.", "INSTRUMENT_VERSION_CHANGED");

            var definisi = ClinicalInstrumentDefinitionEngine.Parse(berlaku.Value.Definition.GetRawText(), out var galat);

            if (definisi == null)
                return NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status400BadRequest, galat ?? "Checklist tidak dapat dibaca.");

            var skor = ClinicalInstrumentDefinitionEngine.Score(definisi, responses);

            if (skor.Errors.Count > 0)
                return NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status400BadRequest, string.Join(" ", skor.Errors));

            var baris = _dbContext.Set<CliAssessmentInstrumentResponse>().Local
                            .FirstOrDefault(x => x.CaseManagementEvaluationId == dokumen.Id && !x.IsDelete)
                        ?? await _dbContext.Set<CliAssessmentInstrumentResponse>()
                            .FirstOrDefaultAsync(x => x.CaseManagementEvaluationId == dokumen.Id && !x.IsDelete, cancellationToken);

            if (baris == null)
            {
                baris = new CliAssessmentInstrumentResponse
                {
                    Id = Guid.NewGuid(),
                    CaseManagementEvaluationId = dokumen.Id,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };

                _dbContext.Set<CliAssessmentInstrumentResponse>().Add(baris);
            }
            else
            {
                baris.UpdateDateTime = now;
                baris.UpdateBy = actorUserId;
            }

            baris.InstrumentVersionId = berlaku.Value.VersionId;
            baris.DefinitionHashSnapshot = berlaku.Value.DefinitionHash;
            baris.ResponsesJson = JsonSerializer.Serialize(responses, ClinicalInstrumentDefinitionEngine.JsonOptions);
            baris.TotalScore = skor.TotalScore;
            baris.BandCode = skor.BandCode;
            baris.BandLabelSnapshot = skor.BandLabel;
            baris.IsAlertBand = skor.IsAlertBand;
            baris.ComputedAt = now;

            return null;
        }

        private async Task<CaseManagementEvaluationResponse> ToResponseAsync(CliCaseManagementEvaluation x, Guid actorUserId, bool isReplay, CancellationToken cancellationToken)
        {
            var nama = await _dbContext.Set<MstEmployee>().AsNoTracking()
                .Where(e => e.Id == x.AuthorEmployeeId)
                .Select(e => e.FullName)
                .FirstOrDefaultAsync(cancellationToken);

            var checklist = await _dbContext.Set<CliAssessmentInstrumentResponse>().AsNoTracking()
                .Where(r => r.CaseManagementEvaluationId == x.Id && !r.IsDelete)
                .Join(_dbContext.Set<CliClinicalInstrumentVersion>().AsNoTracking(), r => r.InstrumentVersionId, v => v.Id, (r, v) => new { r, v })
                .Join(_dbContext.Set<CliClinicalInstrument>().AsNoTracking(), rv => rv.v.InstrumentId, i => i.Id, (rv, i) => new { rv.r, rv.v, i })
                .FirstOrDefaultAsync(cancellationToken);

            var response = new CaseManagementEvaluationResponse
            {
                Id = x.Id,
                EvaluationNumber = x.EvaluationNumber,
                EncounterId = x.EncounterId,
                InpEpisodeId = x.InpEpisodeId,
                PatientId = x.PatientId,
                ServiceUnitIdSnapshot = x.ServiceUnitIdSnapshot,
                EvaluationStatus = x.EvaluationStatus,
                EvaluationStatusLabel = x.EvaluationStatus switch
                {
                    CaseManagementEvaluationStatus.Draft => "Konsep",
                    CaseManagementEvaluationStatus.Completed => "Selesai",
                    _ => "Dibatalkan"
                },
                AuthorEmployeeId = x.AuthorEmployeeId,
                AuthorName = nama,
                AuthorUserId = x.AuthorUserId,
                ClinicalDateTime = x.ClinicalDateTime,
                CompletedAt = x.CompletedAt,
                CompletedByUserId = x.CompletedByUserId,
                CancelledAt = x.CancelledAt,
                CancelledByUserId = x.CancelledByUserId,
                CancelReason = x.CancelReason,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime,
                IsAddendumAvailable = false,
                AddendumUnavailableReason = AlasanAddendumBelumTersedia,
                IsReplay = isReplay
            };

            if (checklist != null)
            {
                using var dokumen = JsonDocument.Parse(string.IsNullOrWhiteSpace(checklist.r.ResponsesJson) ? "{}" : checklist.r.ResponsesJson);
                response.Checklist = new AssessmentInstrumentResultResponse
                {
                    Id = checklist.r.Id,
                    InstrumentVersionId = checklist.r.InstrumentVersionId,
                    InstrumentName = checklist.i.Name,
                    InstrumentKind = checklist.i.InstrumentKind,
                    VersionNumber = checklist.v.VersionNumber,
                    VersionStatus = checklist.v.VersionStatus,
                    DefinitionHashSnapshot = checklist.r.DefinitionHashSnapshot,
                    Responses = dokumen.RootElement.Clone(),
                    TotalScore = checklist.r.TotalScore,
                    BandCode = checklist.r.BandCode,
                    BandLabel = checklist.r.BandLabelSnapshot,
                    IsAlertBand = checklist.r.IsAlertBand,
                    ComputedAt = checklist.r.ComputedAt
                };
            }

            if (x.EvaluationStatus == CaseManagementEvaluationStatus.Draft && x.AuthorUserId == actorUserId)
                response.AvailableActions.AddRange(new[] { "Update", "Complete", "Cancel" });

            return response;
        }

        private static NursingResult<CaseManagementEvaluationResponse> Conflict(string message, string code) =>
            NursingResult<CaseManagementEvaluationResponse>.Fail(StatusCodes.Status409Conflict, message, code);
    }
}
