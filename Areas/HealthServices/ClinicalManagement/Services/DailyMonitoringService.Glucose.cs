using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using System.Globalization;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// GDS bangsal — <c>BE-RWI-119</c> kriteria 4, <c>FR-KEP-061</c>, <c>VAL-KEP-25</c>, <c>RWI-DEC-148</c>, <c>RWI-DEC-150</c> b.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Satuan wajib, tanpa bawaan, tanpa konversi.</b> 280 mg/dL dan 15,5 mmol/L tersimpan apa adanya beserta satuannya.
    /// Kiriman tanpa satuan → <c>400</c> "Pilih satuan gula darah: mg/dL atau mmol/L." Nilai 280 dengan satuan mmol/L →
    /// <c>400</c> "Nilai gula darah 280 mmol/L tidak mungkin. Periksa angka dan satuan."
    /// </para>
    /// <para>
    /// <b>GDS yang sudah dipakai menghitung insulin</b> tidak dapat dibatalkan (<c>VAL-KEP-25c</c>). Koreksinya diizinkan,
    /// dan pelaksanaan sliding scale yang memakainya ditandai <c>ReadingCorrectedAfterExecution</c> — pelaksanaan lama tidak
    /// dihitung ulang.
    /// </para>
    /// </remarks>
    public partial class DailyMonitoringService
    {
        public async Task<NursingResult<List<BloodGlucoseReadingResponse>>> GetGlucoseReadingsAsync(
            Guid episodeId,
            DateTime? from,
            DateTime? to,
            CancellationToken cancellationToken = default)
        {
            var rentang = ResolveRange(from, to);

            if (rentang.Error != null)
                return BadRequest<List<BloodGlucoseReadingResponse>>(rentang.Error);

            var baris = await _dbContext.Set<CliBloodGlucoseReading>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete && x.MeasuredAt >= rentang.From && x.MeasuredAt <= rentang.To)
                .OrderBy(x => x.MeasuredAt)
                .ToListAsync(cancellationToken);

            return NursingResult<List<BloodGlucoseReadingResponse>>.Ok(await ToGlucoseResponsesAsync(baris, cancellationToken), "GDS bangsal berhasil diambil.");
        }

        public async Task<NursingResult<BloodGlucoseReadingResponse>> RecordGlucoseAsync(
            CreateBloodGlucoseReadingRequest request,
            string? idempotencyKey,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var kunci = NursingEpisodeWriteGuard.NormalizeIdempotencyKey(idempotencyKey ?? request.IdempotencyKey);
            var ulang = await FindGlucoseReplayAsync(kunci, cancellationToken);

            if (ulang != null)
                return ulang;

            var penjaga = await _writeGuard.EnsureCanWriteAsync(request.EpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<BloodGlucoseReadingResponse>();

            var dibuat = AddGlucoseReading(penjaga.Value!, request.MeasuredAt, request.GlucoseValue, request.GlucoseUnit, kunci, actorUserId);

            if (!dibuat.IsSuccess)
                return dibuat.Cast<BloodGlucoseReadingResponse>();

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException) when (kunci != null)
            {
                _dbContext.ChangeTracker.Clear();
                return await FindGlucoseReplayAsync(kunci, cancellationToken) ?? throw new InvalidOperationException("GDS gagal disimpan.");
            }

            var gds = dibuat.Value!;

            await _loggerService.InfoAsync(LogCategory, "BloodGlucose.Record", "Perawat mencatat GDS bangsal.",
                new { gds.Id, gds.InpEpisodeId, gds.GlucoseValue, gds.GlucoseUnit, RecordedBy = actorUserId });

            return NursingResult<BloodGlucoseReadingResponse>.Ok((await ToGlucoseResponsesAsync(new List<CliBloodGlucoseReading> { gds }, cancellationToken)).Single(),
                "GDS bangsal berhasil dicatat.", StatusCodes.Status201Created);
        }

        /// <summary>
        /// Memvalidasi dan menambahkan satu GDS bangsal ke pelacak perubahan <b>tanpa menyimpan</b>. Dipakai
        /// <c>SlidingScaleExecutionService</c> supaya GDS, dosis MAR, dan pelaksanaan tersimpan dalam satu transaksi
        /// (<c>INT-KEP-11</c> langkah 2), dan oleh pencatatan GDS biasa.
        /// </summary>
        public NursingResult<CliBloodGlucoseReading> AddGlucoseReading(
            NursingWriteContext context,
            DateTime? measuredAt,
            decimal? glucoseValue,
            BloodGlucoseUnit? glucoseUnit,
            string? idempotencyKey,
            Guid actorUserId)
        {
            var now = DateTime.UtcNow;
            var waktu = measuredAt.HasValue ? AsUtc(measuredAt.Value) : now;

            var galat = ValidateGlucoseValues<CliBloodGlucoseReading>(glucoseValue, glucoseUnit, waktu, context.AdmittedAt, now);

            if (galat != null)
                return galat;

            var gds = new CliBloodGlucoseReading
            {
                Id = Guid.NewGuid(),
                EncounterId = context.EncounterId,
                PatientId = context.PatientId,
                InpEpisodeId = context.EpisodeId,
                MeasuredAt = waktu,
                GlucoseValue = glucoseValue!.Value,
                GlucoseUnit = glucoseUnit!.Value,
                Method = BloodGlucoseMethod.WardGlucometer,
                RecordedByEmployeeId = context.ActorEmployeeId,
                RecordedByUserId = actorUserId,
                ReadingStatus = ClinicalMeasurementStatus.Active,
                RevisionNumber = 0,
                IdempotencyKey = idempotencyKey,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<CliBloodGlucoseReading>().Add(gds);

            return NursingResult<CliBloodGlucoseReading>.Ok(gds, string.Empty);
        }

        public async Task<NursingResult<BloodGlucoseReadingResponse>> CorrectGlucoseAsync(
            Guid id,
            CorrectBloodGlucoseReadingRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.CorrectionReason))
                return BadRequest<BloodGlucoseReadingResponse>("Isi alasan koreksi.", "CORRECTION_REASON_REQUIRED");

            if (!request.ExpectedRevisionNumber.HasValue)
                return BadRequest<BloodGlucoseReadingResponse>("Muat ulang GDS sebelum mengoreksi.", "DETAIL_NOT_LOADED");

            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var gds = await _dbContext.Set<CliBloodGlucoseReading>()
                .FromSqlInterpolated($@"SELECT * FROM public.""CliBloodGlucoseReading"" WHERE ""Id"" = {id} AND ""IsDelete"" = false FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);

            if (gds == null)
                return NursingResult<BloodGlucoseReadingResponse>.Fail(StatusCodes.Status404NotFound, "GDS tidak ditemukan.");

            var penjaga = await _writeGuard.EnsureCanWriteAsync(gds.InpEpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<BloodGlucoseReadingResponse>();

            if (gds.ReadingStatus != ClinicalMeasurementStatus.Active)
                return Conflict<BloodGlucoseReadingResponse>("GDS yang dibatalkan tidak dapat dikoreksi.", "READING_CANCELLED");

            if (request.ExpectedRevisionNumber.Value != gds.RevisionNumber)
                return Stale<BloodGlucoseReadingResponse>();

            var now = DateTime.UtcNow;
            var waktu = request.MeasuredAt.HasValue ? AsUtc(request.MeasuredAt.Value) : gds.MeasuredAt;

            var galat = ValidateGlucoseValues<BloodGlucoseReadingResponse>(request.GlucoseValue, request.GlucoseUnit, waktu, penjaga.Value!.AdmittedAt, now);

            if (galat != null)
                return galat;

            var nomorRevisi = gds.RevisionNumber + 1;

            _dbContext.Set<CliBloodGlucoseReadingRevision>().Add(new CliBloodGlucoseReadingRevision
            {
                Id = Guid.NewGuid(),
                ReadingId = gds.Id,
                RevisionNumber = nomorRevisi,
                PreviousValue = gds.GlucoseValue,
                PreviousUnit = gds.GlucoseUnit,
                PreviousMeasuredAt = gds.MeasuredAt,
                CorrectionReason = Truncate(request.CorrectionReason, 500)!,
                CorrectedByUserId = actorUserId,
                CorrectedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

            gds.GlucoseValue = request.GlucoseValue!.Value;
            gds.GlucoseUnit = request.GlucoseUnit!.Value;
            gds.MeasuredAt = waktu;
            gds.RevisionNumber = nomorRevisi;
            gds.UpdateDateTime = now;
            gds.UpdateBy = actorUserId;

            // RWI-DEC-148 (b) — pelaksanaan lama ditandai, tidak dihitung ulang.
            var pelaksanaan = await _dbContext.Set<PhmSlidingScaleExecution>()
                .Where(x => x.BloodGlucoseReadingId == gds.Id && x.ExecutionStatus == SlidingScaleExecutionStatus.Recorded && !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var p in pelaksanaan)
            {
                p.ReadingCorrectedAfterExecution = true;
                p.UpdateDateTime = now;
                p.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaksi.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "BloodGlucose.Correct", "Perawat mengoreksi GDS bangsal.",
                new { gds.Id, gds.RevisionNumber, gds.GlucoseValue, gds.GlucoseUnit, FlaggedExecutions = pelaksanaan.Count, CorrectedBy = actorUserId });

            return NursingResult<BloodGlucoseReadingResponse>.Ok((await ToGlucoseResponsesAsync(new List<CliBloodGlucoseReading> { gds }, cancellationToken)).Single(),
                pelaksanaan.Count > 0
                    ? "Koreksi GDS tersimpan. Pelaksanaan sliding scale yang memakai GDS ini ditandai."
                    : "Koreksi GDS berhasil disimpan.");
        }

        public async Task<NursingResult<BloodGlucoseReadingResponse>> CancelGlucoseAsync(
            Guid id,
            CancelClinicalMeasurementRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest<BloodGlucoseReadingResponse>("Isi alasan pembatalan.", "CANCEL_REASON_REQUIRED");

            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var gds = await _dbContext.Set<CliBloodGlucoseReading>()
                .FromSqlInterpolated($@"SELECT * FROM public.""CliBloodGlucoseReading"" WHERE ""Id"" = {id} AND ""IsDelete"" = false FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);

            if (gds == null)
                return NursingResult<BloodGlucoseReadingResponse>.Fail(StatusCodes.Status404NotFound, "GDS tidak ditemukan.");

            var penjaga = await _writeGuard.EnsureCanWriteAsync(gds.InpEpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<BloodGlucoseReadingResponse>();

            if (gds.ReadingStatus != ClinicalMeasurementStatus.Active)
                return Conflict<BloodGlucoseReadingResponse>("GDS ini sudah dibatalkan.", "READING_CANCELLED");

            if (request.ExpectedRevisionNumber.HasValue && request.ExpectedRevisionNumber.Value != gds.RevisionNumber)
                return Stale<BloodGlucoseReadingResponse>();

            // VAL-KEP-25c
            var dipakai = await _dbContext.Set<PhmSlidingScaleExecution>().AsNoTracking()
                .AnyAsync(x => x.BloodGlucoseReadingId == gds.Id && x.ExecutionStatus == SlidingScaleExecutionStatus.Recorded && !x.IsDelete, cancellationToken);

            if (dipakai)
                return Conflict<BloodGlucoseReadingResponse>("Gula darah ini sudah dipakai menghitung dosis insulin. Koreksi nilainya, jangan dibatalkan.", "READING_USED_BY_SLIDING_SCALE");

            var now = DateTime.UtcNow;
            gds.ReadingStatus = ClinicalMeasurementStatus.Cancelled;
            gds.CancelReason = Truncate(request.Reason, 500);
            gds.IsCancel = true;
            gds.CancelDateTime = now;
            gds.CancelBy = actorUserId;
            gds.UpdateDateTime = now;
            gds.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaksi.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "BloodGlucose.Cancel", "Perawat membatalkan GDS bangsal.", new { gds.Id, CancelledBy = actorUserId });

            return NursingResult<BloodGlucoseReadingResponse>.Ok((await ToGlucoseResponsesAsync(new List<CliBloodGlucoseReading> { gds }, cancellationToken)).Single(),
                "GDS bangsal berhasil dibatalkan.");
        }

        public async Task<NursingResult<List<BloodGlucoseReadingRevisionResponse>>> GetGlucoseRevisionsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var ada = await _dbContext.Set<CliBloodGlucoseReading>().AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (!ada)
                return NursingResult<List<BloodGlucoseReadingRevisionResponse>>.Fail(StatusCodes.Status404NotFound, "GDS tidak ditemukan.");

            var revisi = await _dbContext.Set<CliBloodGlucoseReadingRevision>().AsNoTracking()
                .Where(x => x.ReadingId == id && !x.IsDelete)
                .OrderByDescending(x => x.RevisionNumber)
                .ToListAsync(cancellationToken);

            var nama = await UserNamesAsync(revisi.Select(x => x.CorrectedByUserId), cancellationToken);

            return NursingResult<List<BloodGlucoseReadingRevisionResponse>>.Ok(revisi.Select(x => new BloodGlucoseReadingRevisionResponse
            {
                Id = x.Id,
                RevisionNumber = x.RevisionNumber,
                PreviousValue = x.PreviousValue,
                PreviousUnit = x.PreviousUnit,
                PreviousUnitLabel = GlucoseUnitLabel(x.PreviousUnit),
                PreviousMeasuredAt = x.PreviousMeasuredAt,
                CorrectionReason = x.CorrectionReason,
                CorrectedByUserId = x.CorrectedByUserId,
                CorrectedByName = nama.GetValueOrDefault(x.CorrectedByUserId),
                CorrectedAt = x.CorrectedAt
            }).ToList(), "Riwayat koreksi GDS berhasil diambil.");
        }

        // =====================================================================
        // Pembantu GDS
        // =====================================================================

        /// <summary><c>VAL-KEP-25a</c>, <c>VAL-KEP-25b</c>, dan waktu klinis.</summary>
        private static NursingResult<T>? ValidateGlucoseValues<T>(
            decimal? value,
            BloodGlucoseUnit? unit,
            DateTime atUtc,
            DateTime? admittedAt,
            DateTime now)
        {
            if (!unit.HasValue || !Enum.IsDefined(unit.Value))
                return BadRequest<T>("Pilih satuan gula darah: mg/dL atau mmol/L.", "GLUCOSE_UNIT_REQUIRED");

            if (!value.HasValue)
                return BadRequest<T>("Isi nilai gula darah.", "GLUCOSE_VALUE_REQUIRED");

            var mungkin = unit.Value == BloodGlucoseUnit.MgPerDl
                ? value.Value is >= 10m and <= 1000m
                : value.Value is >= 0.6m and <= 55.5m;

            if (!mungkin)
                return BadRequest<T>(
                    $"Nilai gula darah {value.Value.ToString("0.##", CultureInfo.GetCultureInfo("id-ID"))} {GlucoseUnitLabel(unit.Value)} tidak mungkin. Periksa angka dan satuan.",
                    "GLUCOSE_VALUE_IMPOSSIBLE");

            if (IsClinicalTimeInvalid(atUtc, admittedAt, now))
                return BadRequest<T>("Waktu pengukuran tidak boleh di masa depan atau sebelum pasien masuk.", "MEASURED_AT_INVALID");

            return null;
        }

        private async Task<NursingResult<BloodGlucoseReadingResponse>?> FindGlucoseReplayAsync(string? kunci, CancellationToken cancellationToken)
        {
            if (kunci == null)
                return null;

            var ada = await _dbContext.Set<CliBloodGlucoseReading>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == kunci && !x.IsDelete, cancellationToken);

            if (ada == null)
                return null;

            var response = (await ToGlucoseResponsesAsync(new List<CliBloodGlucoseReading> { ada }, cancellationToken)).Single();
            response.IsReplay = true;

            return NursingResult<BloodGlucoseReadingResponse>.Ok(response, "GDS sudah tercatat sebelumnya.", isReplay: true);
        }

        private async Task<List<BloodGlucoseReadingResponse>> ToGlucoseResponsesAsync(List<CliBloodGlucoseReading> baris, CancellationToken cancellationToken)
        {
            if (baris.Count == 0)
                return new List<BloodGlucoseReadingResponse>();

            var namaPegawai = await EmployeeNamesAsync(baris.Select(x => x.RecordedByEmployeeId), cancellationToken);
            var ids = baris.Select(x => x.Id).ToList();

            var pelaksanaan = (await _dbContext.Set<PhmSlidingScaleExecution>().AsNoTracking()
                    .Where(x => ids.Contains(x.BloodGlucoseReadingId) && x.ExecutionStatus == SlidingScaleExecutionStatus.Recorded && !x.IsDelete)
                    .Select(x => new { x.BloodGlucoseReadingId, x.Id })
                    .ToListAsync(cancellationToken))
                .GroupBy(x => x.BloodGlucoseReadingId)
                .ToDictionary(g => g.Key, g => g.First().Id);

            return baris.Select(x => new BloodGlucoseReadingResponse
            {
                Id = x.Id,
                EncounterId = x.EncounterId,
                PatientId = x.PatientId,
                InpEpisodeId = x.InpEpisodeId,
                MeasuredAt = x.MeasuredAt,
                GlucoseValue = x.GlucoseValue,
                GlucoseUnit = x.GlucoseUnit,
                GlucoseUnitLabel = GlucoseUnitLabel(x.GlucoseUnit),
                Method = x.Method,
                RecordedByEmployeeId = x.RecordedByEmployeeId,
                RecordedByName = namaPegawai.GetValueOrDefault(x.RecordedByEmployeeId),
                RecordedByUserId = x.RecordedByUserId,
                ReadingStatus = x.ReadingStatus,
                ReadingStatusLabel = StatusLabel(x.ReadingStatus),
                RevisionNumber = x.RevisionNumber,
                CancelReason = x.CancelReason,
                UsedBySlidingScaleExecutionId = pelaksanaan.TryGetValue(x.Id, out var p) ? p : null,
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime
            }).ToList();
        }

        public static string GlucoseUnitLabel(BloodGlucoseUnit unit) =>
            unit == BloodGlucoseUnit.MmolPerL ? "mmol/L" : "mg/dL";
    }
}
