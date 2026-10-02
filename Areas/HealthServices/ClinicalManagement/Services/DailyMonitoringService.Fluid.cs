using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Entri cairan masuk dan keluar — <c>BE-RWI-119</c> (<c>FR-KEP-057</c>), intake obat tertaut dosis MAR —
    /// <c>BE-RWI-122</c> (<c>FR-KEP-058</c>, <c>FR-KEP-063</c>, <c>VAL-KEP-24</c>, <c>INT-KEP-10</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Contoh.</b> Infus RL 500 ml pukul 10.00 dicatat; pukul 14.30 dikoreksi 450 ml "sisa 50 ml di kantong" →
    /// <c>RevisionNumber = 1</c>, revisi menyimpan 500 ml, total 24 jam turun 50 ml.
    /// </para>
    /// <para>
    /// <b>Intake obat.</b> Ceftriaxone 1 g dalam NaCl 100 ml diberikan 08.05. Perawat mencatat sumber Obat, memilih dosis
    /// 08.05, dan <b>mengetik</b> 100 ml — volume aktual termasuk pelarut (<c>RWI-DEC-149</c>). Dosis yang sama tidak
    /// dapat ditunjuk entri kedua (unique <c>UX_CliFluidBalanceEntry_MedicationAdministration_Active</c>). Bila dosis itu
    /// kemudian dikoreksi menjadi ditahan, entri ini <b>ditandai</b> "perlu ditinjau" — volumenya tidak diubah otomatis.
    /// </para>
    /// </remarks>
    public partial class DailyMonitoringService
    {
        private static readonly HashSet<FluidSourceCategory> SumberMasuk = new()
        {
            FluidSourceCategory.Infusion,
            FluidSourceCategory.Oral,
            FluidSourceCategory.NasogastricIntake,
            FluidSourceCategory.Blood,
            FluidSourceCategory.Medication
        };

        private static readonly HashSet<FluidSourceCategory> SumberKeluar = new()
        {
            FluidSourceCategory.Urine,
            FluidSourceCategory.Stool,
            FluidSourceCategory.NasogastricOutput,
            FluidSourceCategory.DrainOrWsd,
            FluidSourceCategory.OtherOutput
        };

        public async Task<NursingResult<List<FluidBalanceEntryResponse>>> GetFluidEntriesAsync(
            Guid episodeId,
            DateTime? from,
            DateTime? to,
            bool includeCancelled,
            CancellationToken cancellationToken = default)
        {
            var rentang = ResolveRange(from, to);

            if (rentang.Error != null)
                return BadRequest<List<FluidBalanceEntryResponse>>(rentang.Error);

            var query = _dbContext.Set<CliFluidBalanceEntry>().AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete &&
                            x.EntryDateTime >= rentang.From && x.EntryDateTime <= rentang.To);

            if (!includeCancelled)
                query = query.Where(x => x.EntryStatus == ClinicalMeasurementStatus.Active);

            var baris = await query.OrderBy(x => x.EntryDateTime).ToListAsync(cancellationToken);

            return NursingResult<List<FluidBalanceEntryResponse>>.Ok(await ToFluidResponsesAsync(baris, cancellationToken), "Entri cairan berhasil diambil.");
        }

        public async Task<NursingResult<FluidBalanceEntryResponse>> RecordFluidAsync(
            CreateFluidBalanceEntryRequest request,
            string? idempotencyKey,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var kunci = NursingEpisodeWriteGuard.NormalizeIdempotencyKey(idempotencyKey ?? request.IdempotencyKey);
            var ulang = await FindFluidReplayAsync(kunci, cancellationToken);

            if (ulang != null)
                return ulang;

            var penjaga = await _writeGuard.EnsureCanWriteAsync(request.EpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<FluidBalanceEntryResponse>();

            var konteks = penjaga.Value!;
            var now = DateTime.UtcNow;
            var waktu = request.EntryDateTime.HasValue ? AsUtc(request.EntryDateTime.Value) : now;

            var galat = ValidateFluidValues<FluidBalanceEntryResponse>(request.Direction, request.SourceCategory, request.VolumeMl, waktu, konteks.AdmittedAt, now);

            if (galat != null)
                return galat;

            // VAL-KEP-24d
            var sumberObat = request.SourceCategory == FluidSourceCategory.Medication;

            if (sumberObat != request.MedicationAdministrationId.HasValue)
                return BadRequest<FluidBalanceEntryResponse>("Intake obat wajib memilih dosis yang diberikan dari MAR.", "MEDICATION_LINK_INVALID");

            if (sumberObat)
            {
                var tautan = await ValidateMedicationLinkAsync<FluidBalanceEntryResponse>(request.MedicationAdministrationId!.Value, konteks.EpisodeId, cancellationToken);

                if (tautan != null)
                    return tautan;
            }

            var entri = new CliFluidBalanceEntry
            {
                Id = Guid.NewGuid(),
                EncounterId = konteks.EncounterId,
                PatientId = konteks.PatientId,
                InpEpisodeId = konteks.EpisodeId,
                Direction = request.Direction,
                SourceCategory = request.SourceCategory,
                SourceDetail = Truncate(request.SourceDetail, 200),
                VolumeMl = request.VolumeMl,
                EntryDateTime = waktu,
                RecordedByEmployeeId = konteks.ActorEmployeeId,
                RecordedByUserId = actorUserId,
                MedicationAdministrationId = request.MedicationAdministrationId,
                EntryStatus = ClinicalMeasurementStatus.Active,
                RevisionNumber = 0,
                IdempotencyKey = kunci,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.Set<CliFluidBalanceEntry>().Add(entri);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Kiriman ulang serentak (kunci sama) atau dosis yang sama ditautkan dua perawat bersamaan.
                _dbContext.ChangeTracker.Clear();

                var pemenang = await FindFluidReplayAsync(kunci, cancellationToken);

                if (pemenang != null)
                    return pemenang;

                if (sumberObat)
                {
                    var tautan = await ValidateMedicationLinkAsync<FluidBalanceEntryResponse>(request.MedicationAdministrationId!.Value, konteks.EpisodeId, cancellationToken);

                    if (tautan != null)
                        return tautan;
                }

                throw;
            }

            await _loggerService.InfoAsync(LogCategory, "FluidBalance.Record", "Perawat mencatat entri cairan.",
                new { entri.Id, entri.InpEpisodeId, entri.Direction, entri.SourceCategory, entri.VolumeMl, entri.MedicationAdministrationId, RecordedBy = actorUserId });

            return NursingResult<FluidBalanceEntryResponse>.Ok((await ToFluidResponsesAsync(new List<CliFluidBalanceEntry> { entri }, cancellationToken)).Single(),
                "Entri cairan berhasil dicatat.", StatusCodes.Status201Created);
        }

        public async Task<NursingResult<FluidBalanceEntryResponse>> CorrectFluidAsync(
            Guid id,
            CorrectFluidBalanceEntryRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            // VAL-KEP-24h
            if (string.IsNullOrWhiteSpace(request.CorrectionReason))
                return BadRequest<FluidBalanceEntryResponse>("Isi alasan koreksi.", "CORRECTION_REASON_REQUIRED");

            if (!request.ExpectedRevisionNumber.HasValue)
                return BadRequest<FluidBalanceEntryResponse>("Muat ulang entri sebelum mengoreksi.", "DETAIL_NOT_LOADED");

            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var entri = await _dbContext.Set<CliFluidBalanceEntry>()
                .FromSqlInterpolated($@"SELECT * FROM public.""CliFluidBalanceEntry"" WHERE ""Id"" = {id} AND ""IsDelete"" = false FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);

            if (entri == null)
                return NursingResult<FluidBalanceEntryResponse>.Fail(StatusCodes.Status404NotFound, "Entri cairan tidak ditemukan.");

            var penjaga = await _writeGuard.EnsureCanWriteAsync(entri.InpEpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<FluidBalanceEntryResponse>();

            if (entri.EntryStatus != ClinicalMeasurementStatus.Active)
                return Conflict<FluidBalanceEntryResponse>("Entri yang dibatalkan tidak dapat dikoreksi.", "ENTRY_CANCELLED");

            if (request.ExpectedRevisionNumber.Value != entri.RevisionNumber)
                return Stale<FluidBalanceEntryResponse>();

            var now = DateTime.UtcNow;
            var waktu = request.EntryDateTime.HasValue ? AsUtc(request.EntryDateTime.Value) : entri.EntryDateTime;

            var galat = ValidateFluidValues<FluidBalanceEntryResponse>(entri.Direction, request.SourceCategory, request.VolumeMl, waktu, penjaga.Value!.AdmittedAt, now);

            if (galat != null)
                return galat;

            // Tautan dosis tidak berpindah lewat koreksi: sumber Obat tetap Obat, sumber lain tidak menjadi Obat.
            if ((entri.SourceCategory == FluidSourceCategory.Medication) != (request.SourceCategory == FluidSourceCategory.Medication))
                return BadRequest<FluidBalanceEntryResponse>("Sumber obat tidak dapat diubah lewat koreksi. Batalkan entri lalu catat ulang.", "MEDICATION_SOURCE_LOCKED");

            var nomorRevisi = entri.RevisionNumber + 1;

            _dbContext.Set<CliFluidBalanceEntryRevision>().Add(new CliFluidBalanceEntryRevision
            {
                Id = Guid.NewGuid(),
                EntryId = entri.Id,
                RevisionNumber = nomorRevisi,
                PreviousVolumeMl = entri.VolumeMl,
                PreviousEntryDateTime = entri.EntryDateTime,
                PreviousSourceCategory = entri.SourceCategory,
                PreviousSourceDetail = entri.SourceDetail,
                CorrectionReason = Truncate(request.CorrectionReason, 500)!,
                CorrectedByUserId = actorUserId,
                CorrectedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId
            });

            entri.VolumeMl = request.VolumeMl;
            entri.EntryDateTime = waktu;
            entri.SourceCategory = request.SourceCategory;
            entri.SourceDetail = Truncate(request.SourceDetail, 200);
            entri.RevisionNumber = nomorRevisi;
            entri.UpdateDateTime = now;
            entri.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaksi.CommitAsync(cancellationToken);

            // CorrectionReason sensitif — tidak masuk payload logger.
            await _loggerService.InfoAsync(LogCategory, "FluidBalance.Correct", "Perawat mengoreksi entri cairan.",
                new { entri.Id, entri.RevisionNumber, entri.VolumeMl, CorrectedBy = actorUserId });

            return NursingResult<FluidBalanceEntryResponse>.Ok((await ToFluidResponsesAsync(new List<CliFluidBalanceEntry> { entri }, cancellationToken)).Single(),
                "Koreksi entri cairan berhasil disimpan.");
        }

        public async Task<NursingResult<FluidBalanceEntryResponse>> CancelFluidAsync(
            Guid id,
            CancelClinicalMeasurementRequest request,
            ClaimsPrincipal? user,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
                return BadRequest<FluidBalanceEntryResponse>("Isi alasan pembatalan.", "CANCEL_REASON_REQUIRED");

            await using var transaksi = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var entri = await _dbContext.Set<CliFluidBalanceEntry>()
                .FromSqlInterpolated($@"SELECT * FROM public.""CliFluidBalanceEntry"" WHERE ""Id"" = {id} AND ""IsDelete"" = false FOR UPDATE")
                .FirstOrDefaultAsync(cancellationToken);

            if (entri == null)
                return NursingResult<FluidBalanceEntryResponse>.Fail(StatusCodes.Status404NotFound, "Entri cairan tidak ditemukan.");

            var penjaga = await _writeGuard.EnsureCanWriteAsync(entri.InpEpisodeId, user, actorUserId, cancellationToken);

            if (!penjaga.IsSuccess)
                return penjaga.Cast<FluidBalanceEntryResponse>();

            if (entri.EntryStatus != ClinicalMeasurementStatus.Active)
                return Conflict<FluidBalanceEntryResponse>("Entri ini sudah dibatalkan.", "ENTRY_CANCELLED");

            if (request.ExpectedRevisionNumber.HasValue && request.ExpectedRevisionNumber.Value != entri.RevisionNumber)
                return Stale<FluidBalanceEntryResponse>();

            var now = DateTime.UtcNow;
            entri.EntryStatus = ClinicalMeasurementStatus.Cancelled;
            entri.CancelReason = Truncate(request.Reason, 500);
            entri.IsCancel = true;
            entri.CancelDateTime = now;
            entri.CancelBy = actorUserId;
            entri.UpdateDateTime = now;
            entri.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaksi.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "FluidBalance.Cancel", "Perawat membatalkan entri cairan.",
                new { entri.Id, CancelledBy = actorUserId });

            return NursingResult<FluidBalanceEntryResponse>.Ok((await ToFluidResponsesAsync(new List<CliFluidBalanceEntry> { entri }, cancellationToken)).Single(),
                "Entri cairan berhasil dibatalkan.");
        }

        public async Task<NursingResult<List<FluidBalanceEntryRevisionResponse>>> GetFluidRevisionsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var ada = await _dbContext.Set<CliFluidBalanceEntry>().AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (!ada)
                return NursingResult<List<FluidBalanceEntryRevisionResponse>>.Fail(StatusCodes.Status404NotFound, "Entri cairan tidak ditemukan.");

            var revisi = await _dbContext.Set<CliFluidBalanceEntryRevision>().AsNoTracking()
                .Where(x => x.EntryId == id && !x.IsDelete)
                .OrderByDescending(x => x.RevisionNumber)
                .ToListAsync(cancellationToken);

            var nama = await UserNamesAsync(revisi.Select(x => x.CorrectedByUserId), cancellationToken);

            return NursingResult<List<FluidBalanceEntryRevisionResponse>>.Ok(revisi.Select(x => new FluidBalanceEntryRevisionResponse
            {
                Id = x.Id,
                RevisionNumber = x.RevisionNumber,
                PreviousVolumeMl = x.PreviousVolumeMl,
                PreviousEntryDateTime = x.PreviousEntryDateTime,
                PreviousSourceCategory = x.PreviousSourceCategory,
                PreviousSourceCategoryLabel = SourceLabel(x.PreviousSourceCategory),
                PreviousSourceDetail = x.PreviousSourceDetail,
                CorrectionReason = x.CorrectionReason,
                CorrectedByUserId = x.CorrectedByUserId,
                CorrectedByName = nama.GetValueOrDefault(x.CorrectedByUserId),
                CorrectedAt = x.CorrectedAt
            }).ToList(), "Riwayat koreksi entri cairan berhasil diambil.");
        }

        /// <summary>
        /// Menandai entri intake yang menunjuk dosis yang dikoreksi menjadi selain <c>Administered</c> — usulan <c>G-26</c>,
        /// <c>VAL-KEP-36f</c>. Dipanggil <c>MedicationAdministrationService.CorrectAsync</c> di dalam transaksinya; tidak
        /// menyimpan sendiri dan tidak mengubah volume.
        /// </summary>
        public async Task<int> FlagLinkedFluidEntryAsync(
            Guid medicationAdministrationId,
            DateTime flaggedAtUtc,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var entri = await _dbContext.Set<CliFluidBalanceEntry>()
                .Where(x => x.MedicationAdministrationId == medicationAdministrationId && !x.IsDelete && x.EntryStatus == ClinicalMeasurementStatus.Active)
                .ToListAsync(cancellationToken);

            foreach (var x in entri)
                x.DoseCorrectionFlaggedAt = flaggedAtUtc;

            _ = actorUserId;

            return entri.Count;
        }

        // =====================================================================
        // Pembantu cairan
        // =====================================================================

        /// <summary><c>VAL-KEP-24a</c>–<c>24c</c>.</summary>
        private static NursingResult<T>? ValidateFluidValues<T>(
            FluidDirection direction,
            FluidSourceCategory source,
            decimal volumeMl,
            DateTime atUtc,
            DateTime? admittedAt,
            DateTime now)
        {
            if (!Enum.IsDefined(direction) || !Enum.IsDefined(source))
                return BadRequest<T>("Arah dan sumber cairan wajib dipilih.", "FLUID_SOURCE_REQUIRED");

            var sejalan = direction == FluidDirection.Intake ? SumberMasuk.Contains(source) : SumberKeluar.Contains(source);

            if (!sejalan)
                return BadRequest<T>($"Sumber {SourceLabel(source)} bukan cairan {(direction == FluidDirection.Intake ? "masuk" : "keluar")}.", "SOURCE_DIRECTION_MISMATCH");

            if (volumeMl <= 0 || volumeMl > 10000)
                return BadRequest<T>("Volume harus lebih dari 0 dan paling banyak 10.000 ml.", "VOLUME_OUT_OF_RANGE");

            if (IsClinicalTimeInvalid(atUtc, admittedAt, now))
                return BadRequest<T>("Waktu pencatatan tidak boleh di masa depan atau sebelum pasien masuk.", "ENTRY_TIME_INVALID");

            return null;
        }

        /// <summary><c>VAL-KEP-24e</c>–<c>24g</c>.</summary>
        private async Task<NursingResult<T>?> ValidateMedicationLinkAsync<T>(Guid administrationId, Guid episodeId, CancellationToken cancellationToken)
        {
            var dosis = await _dbContext.Set<PhmMedicationAdministration>().AsNoTracking()
                .Where(x => x.Id == administrationId && !x.IsDelete)
                .Select(x => new { x.InpEpisodeId, x.DoseStatus })
                .FirstOrDefaultAsync(cancellationToken);

            if (dosis == null)
                return NursingResult<T>.Fail(StatusCodes.Status404NotFound, "Dosis MAR tidak ditemukan.");

            if (dosis.InpEpisodeId != episodeId)
                return NursingResult<T>.Fail(StatusCodes.Status422UnprocessableEntity, "Dosis ini bukan milik pasien ini.", "DOSE_OTHER_EPISODE");

            if (dosis.DoseStatus != MedicationDoseStatus.Administered)
                return Conflict<T>("Dosis ini belum tercatat diberikan.", "DOSE_NOT_ADMINISTERED");

            var intake = await _dbContext.Set<CliFluidBalanceEntry>().AsNoTracking()
                .Where(x => x.MedicationAdministrationId == administrationId && !x.IsDelete && x.EntryStatus == ClinicalMeasurementStatus.Active)
                .Select(x => (DateTime?)x.EntryDateTime)
                .FirstOrDefaultAsync(cancellationToken);

            if (intake.HasValue)
                return Conflict<T>($"Dosis ini sudah punya entri intake pukul {HospitalTimeZone.ToLocal(intake.Value):HH.mm}. Koreksi entri itu bila volumenya salah.", "DOSE_ALREADY_HAS_INTAKE");

            return null;
        }

        private async Task<NursingResult<FluidBalanceEntryResponse>?> FindFluidReplayAsync(string? kunci, CancellationToken cancellationToken)
        {
            if (kunci == null)
                return null;

            var ada = await _dbContext.Set<CliFluidBalanceEntry>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == kunci && !x.IsDelete, cancellationToken);

            if (ada == null)
                return null;

            var response = (await ToFluidResponsesAsync(new List<CliFluidBalanceEntry> { ada }, cancellationToken)).Single();
            response.IsReplay = true;

            return NursingResult<FluidBalanceEntryResponse>.Ok(response, "Entri cairan sudah tercatat sebelumnya.", isReplay: true);
        }

        private async Task<List<FluidBalanceEntryResponse>> ToFluidResponsesAsync(List<CliFluidBalanceEntry> entri, CancellationToken cancellationToken)
        {
            if (entri.Count == 0)
                return new List<FluidBalanceEntryResponse>();

            var namaPegawai = await EmployeeNamesAsync(entri.Select(x => x.RecordedByEmployeeId), cancellationToken);

            var dosisIds = entri.Where(x => x.MedicationAdministrationId.HasValue).Select(x => x.MedicationAdministrationId!.Value).Distinct().ToList();
            Dictionary<Guid, (string Nomor, string Obat)> dosis = dosisIds.Count == 0
                ? new Dictionary<Guid, (string Nomor, string Obat)>()
                : (await _dbContext.Set<PhmMedicationAdministration>().AsNoTracking()
                        .Where(x => dosisIds.Contains(x.Id))
                        .Join(_dbContext.Set<PhmPrescriptionItem>().AsNoTracking(), d => d.PrescriptionItemId, i => i.Id,
                            (d, i) => new { d.Id, d.AdministrationNumber, i.DrugNameSnapshot })
                        .ToListAsync(cancellationToken))
                    .ToDictionary(x => x.Id, x => (Nomor: x.AdministrationNumber, Obat: x.DrugNameSnapshot));

            return entri.Select(x => new FluidBalanceEntryResponse
            {
                Id = x.Id,
                EncounterId = x.EncounterId,
                PatientId = x.PatientId,
                InpEpisodeId = x.InpEpisodeId,
                Direction = x.Direction,
                DirectionLabel = x.Direction == FluidDirection.Intake ? "Masuk" : "Keluar",
                SourceCategory = x.SourceCategory,
                SourceCategoryLabel = SourceLabel(x.SourceCategory),
                SourceDetail = x.SourceDetail,
                VolumeMl = x.VolumeMl,
                EntryDateTime = x.EntryDateTime,
                RecordedByEmployeeId = x.RecordedByEmployeeId,
                RecordedByName = namaPegawai.GetValueOrDefault(x.RecordedByEmployeeId),
                RecordedByUserId = x.RecordedByUserId,
                MedicationAdministrationId = x.MedicationAdministrationId,
                MedicationAdministrationNumber = x.MedicationAdministrationId.HasValue && dosis.TryGetValue(x.MedicationAdministrationId.Value, out var d1) ? d1.Nomor : null,
                MedicationDrugName = x.MedicationAdministrationId.HasValue && dosis.TryGetValue(x.MedicationAdministrationId.Value, out var d2) ? d2.Obat : null,
                EntryStatus = x.EntryStatus,
                EntryStatusLabel = StatusLabel(x.EntryStatus),
                RevisionNumber = x.RevisionNumber,
                CancelReason = x.CancelReason,
                DoseCorrectionFlaggedAt = x.DoseCorrectionFlaggedAt,
                NeedsReview = x.DoseCorrectionFlaggedAt.HasValue &&
                              x.EntryStatus == ClinicalMeasurementStatus.Active &&
                              (!x.UpdateDateTime.HasValue || x.UpdateDateTime.Value < x.DoseCorrectionFlaggedAt.Value),
                CreateDateTime = x.CreateDateTime,
                UpdateDateTime = x.UpdateDateTime
            }).ToList();
        }

        private static string SourceLabel(FluidSourceCategory source) => source switch
        {
            FluidSourceCategory.Infusion => "Infus",
            FluidSourceCategory.Oral => "Oral",
            FluidSourceCategory.NasogastricIntake => "NGT masuk",
            FluidSourceCategory.Blood => "Darah",
            FluidSourceCategory.Medication => "Obat",
            FluidSourceCategory.Urine => "Urin",
            FluidSourceCategory.Stool => "Feses",
            FluidSourceCategory.NasogastricOutput => "NGT keluar",
            FluidSourceCategory.DrainOrWsd => "Drain/WSD",
            FluidSourceCategory.OtherOutput => "Keluaran lain",
            _ => source.ToString()
        };
    }
}
