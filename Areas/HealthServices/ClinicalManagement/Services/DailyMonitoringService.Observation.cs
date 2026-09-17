using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using System.Security.Claims;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Observasi harian terstruktur: diet, mobilisasi, lingkar perut, agitasi — <c>BE-RWI-119</c> kriteria 5,
    /// <c>FR-KEP-062</c>, <c>VAL-KEP-26b</c>.
    /// </summary>
    /// <remarks>
    /// Agitasi kosong berarti <b>belum dinilai</b>, bukan "tidak agitasi"; mobilisasi <c>NotAssessed</c> sama. Koreksi
    /// menyimpan seluruh isian lama sebagai JSON pada <c>CliDailyObservationRevision</c>.
    /// </remarks>
    public partial class DailyMonitoringService
    {
        public async Task<NursingResult<List<DailyObservationResponse>>> GetObservationsAsync(
            Guid episodeId,
            DateTime? from,
            DateTime? to,
            CancellationToken cancellationToken = default)
        {
            var rentang = ResolveRange(from, to);

            if (rentang.Error != null)
                return BadRequest<List<DailyObservationResponse>>(rentang.Error);

            var baris = await _dbContext.Set<CliDailyObservation>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete && x.ObservedAt >= rentang.From && x.ObservedAt <= rentang.To)
                .OrderBy(x => x.ObservedAt)
                .ToListAsync(cancellationToken);

            return NursingResult<List<DailyObservationResponse>>.Ok(await ToObservationResponsesAsync(baris, cancellationToken), "Observasi harian berhasil diambil.");
        }

        public async Task<NursingResult<DailyObservationResponse>> RecordObservationAsync(
            CreateDailyObservationRequest request,
            string? idempotencyKey,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var kunci = NursingEpisodeWriteGuard.NormalizeIdempotencyKey(idempotencyKey ?? request.IdempotencyKey);
            var ulang = await FindObservationReplayAsync(kunci, cancellationToken);

            if (ulang != null)
                return ulang;

            var penjaga = await _writeGuard.EnsureCanWriteAsync(request.EpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<DailyObservationResponse>();

            var konteks = penjaga.Value!;
            var now = DateTime.UtcNow;
            var waktu = request.ObservedAt.HasValue ? AsUtc(request.ObservedAt.Value) : now;

            var galat = ValidateObservationValues<DailyObservationResponse>(
                request.DietIntakePercent, request.DietNote, request.MobilizationLevel, request.AbdominalCircumferenceCm, request.IsAgitated, request.Note,
                waktu, konteks.AdmittedAt, now);

            if (galat != null)
                return galat;

            var observasi = new CliDailyObservation
            {
                Id = Guid.NewGuid(),
                EncounterId = konteks.EncounterId,
                PatientId = konteks.PatientId,
                InpEpisodeId = konteks.EpisodeId,
                ObservedAt = waktu,
                DietIntakePercent = request.DietIntakePercent,
                DietNote = Truncate(request.DietNote, 500),
                MobilizationLevel = request.MobilizationLevel,
                AbdominalCircumferenceCm = request.AbdominalCircumferenceCm,
                IsAgitated = request.IsAgitated,
                Note = Truncate(request.Note, 1000),
                RecordedByEmployeeId = konteks.ActorEmployeeId,
                RecordedByUserId = actorUserId,
                ObservationStatus = ClinicalMeasurementStatus.Active,
                RevisionNumber = 0,
                IdempotencyKey = kunci,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<CliDailyObservation>().Add(observasi);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException) when (kunci != null)
            {
                _dbContext.ChangeTracker.Clear();
                return await FindObservationReplayAsync(kunci, cancellationToken) ?? throw new InvalidOperationException("Observasi harian gagal disimpan.");
            }

            // DietNote dan Note sensitif — tidak masuk payload logger.
            await _loggerService.InfoAsync(LogCategory, "DailyObservation.Record", "Perawat mencatat observasi harian.",
                new { observasi.Id, observasi.InpEpisodeId, RecordedBy = actorUserId });

            return NursingResult<DailyObservationResponse>.Ok((await ToObservationResponsesAsync(new List<CliDailyObservation> { observasi }, cancellationToken)).Single(),
                "Observasi harian berhasil dicatat.", StatusCodes.Status201Created);
        }

        public async Task<NursingResult<DailyObservationResponse>> CorrectObservationAsync(
            Guid id,
            CorrectDailyObservationRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.CorrectionReason))
                return BadRequest<DailyObservationResponse>("Isi alasan koreksi.", "CORRECTION_REASON_REQUIRED");

            if (!request.ExpectedRevisionNumber.HasValue)
                return BadRequest<DailyObservationResponse>("Muat ulang observasi sebelum mengoreksi.", "DETAIL_NOT_LOADED");

            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var observasi = await _dbContext.Set<CliDailyObservation>()
                .FromSqlInterpolated($@"SELECT * FROM public.""CliDailyObservation"" WHERE ""Id"" = {id} AND ""IsDelete"" = false FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);

            if (observasi == null)
                return NursingResult<DailyObservationResponse>.Fail(StatusCodes.Status404NotFound, "Observasi harian tidak ditemukan.");

            var penjaga = await _writeGuard.EnsureCanWriteAsync(observasi.InpEpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<DailyObservationResponse>();

            if (observasi.ObservationStatus != ClinicalMeasurementStatus.Active)
                return Conflict<DailyObservationResponse>("Observasi yang dibatalkan tidak dapat dikoreksi.", "OBSERVATION_CANCELLED");

            if (request.ExpectedRevisionNumber.Value != observasi.RevisionNumber)
                return Stale<DailyObservationResponse>();

            var now = DateTime.UtcNow;
            var waktu = request.ObservedAt.HasValue ? AsUtc(request.ObservedAt.Value) : observasi.ObservedAt;

            var galat = ValidateObservationValues<DailyObservationResponse>(
                request.DietIntakePercent, request.DietNote, request.MobilizationLevel, request.AbdominalCircumferenceCm, request.IsAgitated, request.Note,
                waktu, penjaga.Value!.AdmittedAt, now);

            if (galat != null)
                return galat;

            var nomorRevisi = observasi.RevisionNumber + 1;

            _dbContext.Set<CliDailyObservationRevision>().Add(new CliDailyObservationRevision
            {
                Id = Guid.NewGuid(),
                ObservationId = observasi.Id,
                RevisionNumber = nomorRevisi,
                PreviousValuesJson = JsonSerializer.Serialize(new
                {
                    observasi.ObservedAt,
                    observasi.DietIntakePercent,
                    observasi.DietNote,
                    observasi.MobilizationLevel,
                    observasi.AbdominalCircumferenceCm,
                    observasi.IsAgitated,
                    observasi.Note
                }),
                CorrectionReason = Truncate(request.CorrectionReason, 500)!,
                CorrectedByUserId = actorUserId,
                CorrectedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

            observasi.ObservedAt = waktu;
            observasi.DietIntakePercent = request.DietIntakePercent;
            observasi.DietNote = Truncate(request.DietNote, 500);
            observasi.MobilizationLevel = request.MobilizationLevel;
            observasi.AbdominalCircumferenceCm = request.AbdominalCircumferenceCm;
            observasi.IsAgitated = request.IsAgitated;
            observasi.Note = Truncate(request.Note, 1000);
            observasi.RevisionNumber = nomorRevisi;
            observasi.UpdateDateTime = now;
            observasi.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaksi.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "DailyObservation.Correct", "Perawat mengoreksi observasi harian.",
                new { observasi.Id, observasi.RevisionNumber, CorrectedBy = actorUserId });

            return NursingResult<DailyObservationResponse>.Ok((await ToObservationResponsesAsync(new List<CliDailyObservation> { observasi }, cancellationToken)).Single(),
                "Koreksi observasi harian berhasil disimpan.");
        }

        public async Task<NursingResult<DailyObservationResponse>> CancelObservationAsync(
            Guid id,
            CancelClinicalMeasurementRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest<DailyObservationResponse>("Isi alasan pembatalan.", "CANCEL_REASON_REQUIRED");

            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var observasi = await _dbContext.Set<CliDailyObservation>()
                .FromSqlInterpolated($@"SELECT * FROM public.""CliDailyObservation"" WHERE ""Id"" = {id} AND ""IsDelete"" = false FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);

            if (observasi == null)
                return NursingResult<DailyObservationResponse>.Fail(StatusCodes.Status404NotFound, "Observasi harian tidak ditemukan.");

            var penjaga = await _writeGuard.EnsureCanWriteAsync(observasi.InpEpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<DailyObservationResponse>();

            if (observasi.ObservationStatus != ClinicalMeasurementStatus.Active)
                return Conflict<DailyObservationResponse>("Observasi ini sudah dibatalkan.", "OBSERVATION_CANCELLED");

            if (request.ExpectedRevisionNumber.HasValue && request.ExpectedRevisionNumber.Value != observasi.RevisionNumber)
                return Stale<DailyObservationResponse>();

            var now = DateTime.UtcNow;
            observasi.ObservationStatus = ClinicalMeasurementStatus.Cancelled;
            observasi.CancelReason = Truncate(request.Reason, 500);
            observasi.IsCancel = true;
            observasi.CancelDateTime = now;
            observasi.CancelBy = actorUserId;
            observasi.UpdateDateTime = now;
            observasi.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaksi.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "DailyObservation.Cancel", "Perawat membatalkan observasi harian.", new { observasi.Id, CancelledBy = actorUserId });

            return NursingResult<DailyObservationResponse>.Ok((await ToObservationResponsesAsync(new List<CliDailyObservation> { observasi }, cancellationToken)).Single(),
                "Observasi harian berhasil dibatalkan.");
        }

        // =====================================================================
        // Pembantu observasi
        // =====================================================================

        /// <summary><c>VAL-KEP-26b</c>, sekurang-kurangnya satu isian, dan waktu klinis.</summary>
        private static NursingResult<T>? ValidateObservationValues<T>(
            int? dietIntakePercent,
            string? dietNote,
            MobilizationLevel mobilizationLevel,
            decimal? abdominalCircumferenceCm,
            bool? isAgitated,
            string? note,
            DateTime atUtc,
            DateTime? admittedAt,
            DateTime now)
        {
            if (!Enum.IsDefined(mobilizationLevel))
                return BadRequest<T>("Tingkat mobilisasi tidak dikenal.", "MOBILIZATION_INVALID");

            if (dietIntakePercent is < 0 or > 100)
                return BadRequest<T>("Nilai asupan diet di luar batas.", "DIET_PERCENT_OUT_OF_RANGE");

            if (abdominalCircumferenceCm is < 20m or > 250m)
                return BadRequest<T>("Nilai lingkar perut di luar batas.", "ABDOMINAL_OUT_OF_RANGE");

            var adaIsian = dietIntakePercent.HasValue ||
                           !string.IsNullOrWhiteSpace(dietNote) ||
                           mobilizationLevel != MobilizationLevel.NotAssessed ||
                           abdominalCircumferenceCm.HasValue ||
                           isAgitated.HasValue ||
                           !string.IsNullOrWhiteSpace(note);

            if (!adaIsian)
                return BadRequest<T>("Isi sekurang-kurangnya satu observasi.", "OBSERVATION_EMPTY");

            if (IsClinicalTimeInvalid(atUtc, admittedAt, now))
                return BadRequest<T>("Waktu observasi tidak boleh di masa depan atau sebelum pasien masuk.", "OBSERVED_AT_INVALID");

            return null;
        }

        private async Task<NursingResult<DailyObservationResponse>?> FindObservationReplayAsync(string? kunci, CancellationToken cancellationToken)
        {
            if (kunci == null)
                return null;

            var ada = await _dbContext.Set<CliDailyObservation>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == kunci && !x.IsDelete, cancellationToken);

            if (ada == null)
                return null;

            var response = (await ToObservationResponsesAsync(new List<CliDailyObservation> { ada }, cancellationToken)).Single();
            response.IsReplay = true;

            return NursingResult<DailyObservationResponse>.Ok(response, "Observasi harian sudah tercatat sebelumnya.", isReplay: true);
        }

        private async Task<List<DailyObservationResponse>> ToObservationResponsesAsync(List<CliDailyObservation> baris, CancellationToken cancellationToken)
        {
            if (baris.Count == 0)
                return new List<DailyObservationResponse>();

            var namaPegawai = await EmployeeNamesAsync(baris.Select(x => x.RecordedByEmployeeId), cancellationToken);

            return baris.Select(x => new DailyObservationResponse
            {
                Id = x.Id,
                EncounterId = x.EncounterId,
                PatientId = x.PatientId,
                InpEpisodeId = x.InpEpisodeId,
                ObservedAt = x.ObservedAt,
                DietIntakePercent = x.DietIntakePercent,
                DietNote = x.DietNote,
                MobilizationLevel = x.MobilizationLevel,
                MobilizationLevelLabel = x.MobilizationLevel switch
                {
                    MobilizationLevel.Bedrest => "Tirah baring",
                    MobilizationLevel.SitInBed => "Duduk di tempat tidur",
                    MobilizationLevel.AssistedWalking => "Berjalan dibantu",
                    MobilizationLevel.Independent => "Mandiri",
                    _ => "Belum dinilai"
                },
                AbdominalCircumferenceCm = x.AbdominalCircumferenceCm,
                IsAgitated = x.IsAgitated,
                Note = x.Note,
                RecordedByEmployeeId = x.RecordedByEmployeeId,
                RecordedByName = namaPegawai.GetValueOrDefault(x.RecordedByEmployeeId),
                RecordedByUserId = x.RecordedByUserId,
                ObservationStatus = x.ObservationStatus,
                ObservationStatusLabel = StatusLabel(x.ObservationStatus),
                RevisionNumber = x.RevisionNumber,
                CancelReason = x.CancelReason,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime
            }).ToList();
        }
    }
}
