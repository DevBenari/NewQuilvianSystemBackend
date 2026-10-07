using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    /// <summary>
    /// Serah terima klinis saat transfer antarunit (<c>BE-RWI-183</c>, <c>RWI-DEC-182</c>,
    /// <c>RWI-DEC-189</c>, <c>INV-RWF-33</c>, state matrix 9.5, API 11.8, <c>P2</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Tidak pernah menahan transfer.</b> Dokumen dibuat <b>sesudah</b> transfer tersimpan; bila
    /// pembuatannya gagal, transfer tetap sah dan dokumennya dibuat ulang oleh pengecekan saat daftar
    /// dibaca (<see cref="EnsureMissingDocumentsAsync"/>).
    /// </para>
    /// <para>
    /// Hanya perpindahan ke unit lain yang membuat dokumen. Perpindahan bed di unit yang sama dan
    /// koreksi salah catat penempatan (<c>CorrectsPlacementId</c> terisi) tidak membuat dokumen.
    /// </para>
    /// </remarks>
    public sealed class CliTransferHandoverService
    {
        /// <summary>422 — penerima sama dengan pengirim.</summary>
        public const string SameAccountCode = "CLI-TRH-001";

        /// <summary>422 — penerima harus bertugas di unit tujuan (pasien menempati bed aktif di unit itu).</summary>
        public const string NotInDestinationUnitCode = "CLI-TRH-002";

        private const string LogCategory = "HealthServices.ClinicalManagement.TransferHandover";

        /// <summary>Jendela balance cairan pada potret.</summary>
        private static readonly TimeSpan FluidBalanceWindow = TimeSpan.FromHours(24);

        private static readonly PatientVitalSignStatus[] UsableVitalStatuses =
            [PatientVitalSignStatus.Recorded, PatientVitalSignStatus.Verified, PatientVitalSignStatus.Corrected];

        private static readonly JsonSerializerOptions SnapshotJson = new(JsonSerializerDefaults.Web);

        private readonly ApplicationDbContext _dbContext;
        private readonly InpPatientLocationQuery _patientLocation;
        private readonly LoggerService _loggerService;

        public CliTransferHandoverService(ApplicationDbContext dbContext, InpPatientLocationQuery patientLocation,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _patientLocation = patientLocation;
            _loggerService = loggerService;
        }

        // ---------------------------------------------------------------------
        // Pembuatan (tanpa endpoint)
        // ---------------------------------------------------------------------

        /// <summary>
        /// Membuat dokumen <c>NotSent</c> untuk satu transfer antarunit. Idempoten per penempatan tujuan.
        /// </summary>
        /// <returns>Id dokumen, atau <c>null</c> bila transfer itu tidak membutuhkan dokumen.</returns>
        public async Task<Guid?> CreateForTransferAsync(Guid episodeId, Guid fromPlacementId, Guid toPlacementId,
            Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var existing = await _dbContext.CliTransferHandovers.AsNoTracking()
                .Where(x => x.ToPlacementId == toPlacementId)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (existing.HasValue) return existing;

            var placements = await _dbContext.InpBedPlacements.AsNoTracking()
                .Where(x => (x.Id == fromPlacementId || x.Id == toPlacementId) && x.EpisodeId == episodeId && !x.IsDelete)
                .Select(x => new { x.Id, x.ServiceUnitId, x.CorrectsPlacementId })
                .ToListAsync(cancellationToken);
            var from = placements.FirstOrDefault(x => x.Id == fromPlacementId);
            var to = placements.FirstOrDefault(x => x.Id == toPlacementId);

            // Unit sama atau koreksi salah catat → tidak ada dokumen (AC 1, AC 5).
            if (from == null || to == null || from.ServiceUnitId == to.ServiceUnitId || to.CorrectsPlacementId.HasValue)
                return null;

            var now = DateTime.UtcNow;
            var document = new CliTransferHandover
            {
                Id = Guid.NewGuid(),
                InpEpisodeId = episodeId,
                FromPlacementId = from.Id,
                ToPlacementId = to.Id,
                FromServiceUnitId = from.ServiceUnitId,
                ToServiceUnitId = to.ServiceUnitId,
                Status = CliTransferHandoverStatus.NotSent,
                Version = 0,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            _dbContext.CliTransferHandovers.Add(document);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // Dua pembuat hampir bersamaan: index unik ToPlacementId menolak yang kalah.
                _dbContext.Entry(document).State = EntityState.Detached;
                return await _dbContext.CliTransferHandovers.AsNoTracking()
                    .Where(x => x.ToPlacementId == toPlacementId)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            await _loggerService.AuditAsync(LogCategory, "CliTransferHandover.CreateForTransfer",
                "Membuat dokumen serah terima transfer antarunit.",
                new { DocumentId = document.Id, EpisodeId = episodeId, document.FromServiceUnitId, document.ToServiceUnitId });
            return document.Id;
        }

        /// <summary>
        /// Membuat ulang dokumen yang gagal dibuat sesudah transfer (backend 12.5). Dibatasi pada
        /// episode yang masih hadir dan pada lingkup bacaan (episode atau unit), supaya pengecekannya
        /// ringan.
        /// </summary>
        public async Task<int> EnsureMissingDocumentsAsync(Guid? episodeId, Guid? serviceUnitId, Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (!episodeId.HasValue && !serviceUnitId.HasValue) return 0;

            var query = _dbContext.InpBedPlacements.AsNoTracking()
                .Where(x => !x.IsDelete && x.Episode != null && !x.Episode.IsDelete &&
                    (x.Episode.EpisodeStatus == InpEpisodeStatus.Admitted ||
                     x.Episode.EpisodeStatus == InpEpisodeStatus.DischargePending));
            query = episodeId.HasValue
                ? query.Where(x => x.EpisodeId == episodeId.Value)
                : query.Where(x => x.Episode!.BedPlacements.Any(p => p.ServiceUnitId == serviceUnitId!.Value && !p.IsDelete));

            var placements = await query
                .Select(x => new { x.Id, x.EpisodeId, x.SequenceNumber, x.ServiceUnitId, x.EndReason, x.CorrectsPlacementId })
                .ToListAsync(cancellationToken);

            var placementIds = placements.Select(p => p.Id).ToList();
            var documented = await _dbContext.CliTransferHandovers.AsNoTracking()
                .Where(x => placementIds.Contains(x.ToPlacementId))
                .Select(x => x.ToPlacementId)
                .ToListAsync(cancellationToken);
            var documentedSet = documented.ToHashSet();

            var created = 0;
            foreach (var episode in placements.GroupBy(x => x.EpisodeId))
            {
                var ordered = episode.OrderBy(x => x.SequenceNumber).ToList();
                for (var i = 1; i < ordered.Count; i++)
                {
                    var from = ordered[i - 1];
                    var to = ordered[i];
                    if (from.EndReason != InpBedPlacementEndReason.Transfer || to.CorrectsPlacementId.HasValue ||
                        from.ServiceUnitId == to.ServiceUnitId || documentedSet.Contains(to.Id))
                        continue;

                    if (await CreateForTransferAsync(episode.Key, from.Id, to.Id, actorUserId, cancellationToken) != null)
                        created++;
                }
            }
            return created;
        }

        // ---------------------------------------------------------------------
        // Bacaan
        // ---------------------------------------------------------------------

        public async Task<List<TransferHandoverResponse>> GetListAsync(TransferHandoverQuery request, Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            // Pengecekan pembuatan ulang dokumen yang tertinggal pada lingkup bacaan ini.
            await EnsureMissingDocumentsAsync(request.EpisodeId, request.ServiceUnitId, actorUserId, cancellationToken);

            var query = _dbContext.CliTransferHandovers.AsNoTracking().Where(x => !x.IsDelete);
            if (request.EpisodeId.HasValue)
                query = query.Where(x => x.InpEpisodeId == request.EpisodeId.Value);
            if (request.ServiceUnitId.HasValue)
                query = query.Where(x => x.FromServiceUnitId == request.ServiceUnitId.Value ||
                    x.ToServiceUnitId == request.ServiceUnitId.Value);
            if (request.Status.HasValue)
                query = query.Where(x => x.Status == request.Status.Value);

            var ids = await query.OrderByDescending(x => x.CreateDateTime).Take(200)
                .Select(x => x.Id).ToListAsync(cancellationToken);
            var result = new List<TransferHandoverResponse>();
            foreach (var id in ids)
            {
                var item = await GetByIdAsync(id, cancellationToken);
                if (item != null) result.Add(item);
            }
            return result;
        }

        public async Task<TransferHandoverResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var row = await _dbContext.CliTransferHandovers.AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new
                {
                    Document = x,
                    EpisodeNumber = x.Episode != null ? x.Episode.EpisodeNumber : string.Empty,
                    PatientId = x.Episode != null ? x.Episode.PatientId : Guid.Empty,
                    PatientName = x.Episode != null && x.Episode.Patient != null ? x.Episode.Patient.FullName : string.Empty,
                    MedicalRecordNumber = x.Episode != null && x.Episode.Patient != null ? x.Episode.Patient.MedicalRecordNumber : null,
                    FromUnitName = x.FromServiceUnit != null ? x.FromServiceUnit.ServiceUnitName : null,
                    ToUnitName = x.ToServiceUnit != null ? x.ToServiceUnit.ServiceUnitName : null,
                    TransferredAt = x.ToPlacement != null ? x.ToPlacement.StartDateTime : x.CreateDateTime,
                    TransferReason = x.ToPlacement != null ? x.ToPlacement.TransferReason : null
                })
                .FirstOrDefaultAsync(cancellationToken);
            if (row == null) return null;

            var document = row.Document;
            var userIds = new[] { document.SentByUserId, document.ReceivedByUserId }
                .Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var names = userIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.Users.AsNoTracking().Where(x => userIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

            return new TransferHandoverResponse
            {
                Id = document.Id,
                InpEpisodeId = document.InpEpisodeId,
                EpisodeNumber = row.EpisodeNumber,
                PatientId = row.PatientId,
                PatientName = row.PatientName,
                MedicalRecordNumber = row.MedicalRecordNumber,
                FromPlacementId = document.FromPlacementId,
                ToPlacementId = document.ToPlacementId,
                FromServiceUnitId = document.FromServiceUnitId,
                FromServiceUnitName = row.FromUnitName,
                ToServiceUnitId = document.ToServiceUnitId,
                ToServiceUnitName = row.ToUnitName,
                TransferredAt = row.TransferredAt,
                TransferReason = row.TransferReason,
                Status = document.Status,
                IsPending = document.Status != CliTransferHandoverStatus.Accepted,
                SoapSummary = document.SoapSummary,
                HandedItems = document.HandedItems,
                SpecialInstructions = document.SpecialInstructions,
                Snapshot = Deserialize(document.SnapshotJson),
                SentByName = document.SentByUserId.HasValue && names.TryGetValue(document.SentByUserId.Value, out var sender) ? sender : null,
                SentAt = document.SentAt,
                ReceivedByName = document.ReceivedByUserId.HasValue && names.TryGetValue(document.ReceivedByUserId.Value, out var receiver) ? receiver : null,
                ReceivedAt = document.ReceivedAt,
                RejectionReason = document.RejectionReason,
                Version = document.Version
            };
        }

        // ---------------------------------------------------------------------
        // Tulis
        // ---------------------------------------------------------------------

        /// <summary>Melengkapi bagian yang diketik pengirim selama dokumen belum dikirim atau ditolak.</summary>
        public async Task<CliTransferHandoverResult> SaveDraftAsync(Guid id, SaveTransferHandoverDraftRequest request,
            Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var document = await _dbContext.CliTransferHandovers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (document == null) return CliTransferHandoverResult.NotFound();
            if (document.Status is not (CliTransferHandoverStatus.NotSent or CliTransferHandoverStatus.Rejected))
                return CliTransferHandoverResult.Fail(StatusCodes.Status409Conflict, null,
                    "Dokumen serah terima sudah dikirim atau diterima, sehingga tidak dapat diubah.");
            if (document.Version != request.ExpectedVersion)
                return CliTransferHandoverResult.VersionChanged();

            var now = DateTime.UtcNow;
            document.SoapSummary = Truncate(request.SoapSummary, 4000);
            document.HandedItems = Truncate(request.HandedItems, 2000);
            document.SpecialInstructions = Truncate(request.SpecialInstructions, 2000);
            document.Version++;
            document.UpdateDateTime = now;
            document.UpdateBy = actorUserId;
            return await SaveAndReadAsync(document, "Draf serah terima transfer berhasil disimpan.", cancellationToken);
        }

        /// <summary>Mengirim dokumen; GCS, tanda vital, nyeri, risiko jatuh, dan balance cairan dibekukan.</summary>
        public async Task<CliTransferHandoverResult> SendAsync(Guid id, SendTransferHandoverRequest request, Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var document = await _dbContext.CliTransferHandovers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (document == null) return CliTransferHandoverResult.NotFound();
            if (document.Status is not (CliTransferHandoverStatus.NotSent or CliTransferHandoverStatus.Rejected))
                return CliTransferHandoverResult.Fail(StatusCodes.Status409Conflict, null,
                    "Dokumen serah terima sudah dikirim atau diterima.");
            if (document.Version != request.ExpectedVersion)
                return CliTransferHandoverResult.VersionChanged();

            var now = DateTime.UtcNow;
            document.SnapshotJson = JsonSerializer.Serialize(
                await BuildSnapshotAsync(document.InpEpisodeId, now, cancellationToken), SnapshotJson);
            document.Status = CliTransferHandoverStatus.Sent;
            document.SentByUserId = actorUserId;
            document.SentAt = now;
            document.ReceivedByUserId = null;
            document.ReceivedAt = null;
            document.Version++;
            document.UpdateDateTime = now;
            document.UpdateBy = actorUserId;
            return await SaveAndReadAsync(document, "Serah terima transfer berhasil dikirim.", cancellationToken);
        }

        /// <summary>
        /// Terima atau tolak beralasan oleh pemegang <c>TransferHandover : Receive</c> yang bukan pengirim.
        /// Menerima menuntut pasien sudah menempati bed aktif di unit tujuan (<c>VAL-RWF-91</c>).
        /// </summary>
        public async Task<CliTransferHandoverResult> AcceptAsync(Guid id, AcceptTransferHandoverRequest request,
            Guid actorUserId, CancellationToken cancellationToken = default)
        {
            var document = await _dbContext.CliTransferHandovers
                .Include(x => x.Episode)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (document == null) return CliTransferHandoverResult.NotFound();
            if (document.Status != CliTransferHandoverStatus.Sent)
                return CliTransferHandoverResult.Fail(StatusCodes.Status409Conflict, null,
                    "Hanya serah terima yang sudah dikirim yang dapat diterima atau ditolak.");
            if (document.Version != request.ExpectedVersion)
                return CliTransferHandoverResult.VersionChanged();

            // VAL-RWF-92: tolak wajib beralasan.
            var reason = string.IsNullOrWhiteSpace(request.RejectionReason) ? null : request.RejectionReason.Trim();
            if (!request.Accept && reason == null)
                return CliTransferHandoverResult.Fail(StatusCodes.Status400BadRequest, null, "Isi alasan penolakan");

            // VAL-RWF-91: penerima (atau penolak) ≠ pengirim.
            if (document.SentByUserId == actorUserId)
                return CliTransferHandoverResult.Fail(StatusCodes.Status422UnprocessableEntity, SameAccountCode,
                    "Pengirim tidak dapat menerima serah terima sendiri");

            if (request.Accept && (document.Episode == null ||
                !await _patientLocation.IsPatientInUnitAsync(document.Episode.PatientId, document.ToServiceUnitId, cancellationToken)))
                return CliTransferHandoverResult.Fail(StatusCodes.Status422UnprocessableEntity, NotInDestinationUnitCode,
                    "Penerima harus bertugas di unit tujuan pasien");

            var now = DateTime.UtcNow;
            document.Status = request.Accept ? CliTransferHandoverStatus.Accepted : CliTransferHandoverStatus.Rejected;
            document.ReceivedByUserId = actorUserId;
            document.ReceivedAt = now;
            if (!request.Accept) document.RejectionReason = Truncate(reason, 1000);
            document.Version++;
            document.UpdateDateTime = now;
            document.UpdateBy = actorUserId;
            return await SaveAndReadAsync(document,
                request.Accept ? "Serah terima transfer berhasil diterima." : "Serah terima transfer ditolak.",
                cancellationToken);
        }

        // ---------------------------------------------------------------------
        // Pembantu
        // ---------------------------------------------------------------------

        private async Task<TransferHandoverSnapshot> BuildSnapshotAsync(Guid episodeId, DateTime now,
            CancellationToken cancellationToken)
        {
            var episode = await _dbContext.InpEpisodes.AsNoTracking()
                .Where(x => x.Id == episodeId)
                .Select(x => new { x.EncounterId, x.PatientId })
                .FirstAsync(cancellationToken);

            var snapshot = new TransferHandoverSnapshot { CapturedAt = now };

            var vital = await _dbContext.Set<TrxPatientVitalSign>().AsNoTracking()
                .Where(x => x.EncounterId == episode.EncounterId && x.PatientId == episode.PatientId && !x.IsDelete &&
                    !x.IsCancel && UsableVitalStatuses.Contains(x.VitalSignStatus))
                .OrderByDescending(x => x.ObservationDateTime)
                .FirstOrDefaultAsync(cancellationToken);
            if (vital != null)
            {
                snapshot.Vital = new TransferHandoverVitalSnapshot
                {
                    VitalSignId = vital.Id, RecordedAt = vital.ObservationDateTime,
                    SystolicBp = vital.BloodPressureSystolic, DiastolicBp = vital.BloodPressureDiastolic,
                    PulseRate = vital.PulseRate, RespiratoryRate = vital.RespiratoryRate,
                    Temperature = vital.Temperature, SpO2 = vital.OxygenSaturation,
                    GcsEye = vital.GcsEye, GcsVerbal = vital.GcsVerbal, GcsMotor = vital.GcsMotor,
                    GcsTotal = vital.GcsTotal, Consciousness = vital.ConsciousnessStatus.ToString()
                };
            }

            var pain = await _dbContext.Set<TrxPatientAssessment>().AsNoTracking()
                .Where(x => x.EncounterId == episode.EncounterId && x.PatientId == episode.PatientId && !x.IsDelete &&
                    !x.IsCancel && x.AssessmentType == PatientAssessmentType.PainMonitoring &&
                    x.AssessmentStatus == PatientAssessmentStatus.Completed &&
                    x.PainAssessmentState != PainAssessmentState.NotAssessed)
                .OrderByDescending(x => x.AssessmentDateTime)
                .Select(x => new { x.Id, x.PainScale, x.PainAssessmentState, x.AssessmentDateTime })
                .FirstOrDefaultAsync(cancellationToken);
            if (pain != null)
            {
                snapshot.Pain = new TransferHandoverPainSnapshot
                {
                    Score = pain.PainAssessmentState == PainAssessmentState.NoPain ? 0 : pain.PainScale,
                    State = pain.PainAssessmentState.ToString(), RecordedAt = pain.AssessmentDateTime,
                    SourceId = pain.Id, SourceKind = "PainAssessment"
                };
            }
            else if (vital != null && (vital.HasPain || vital.PainScale.HasValue))
            {
                snapshot.Pain = new TransferHandoverPainSnapshot
                {
                    Score = vital.PainScale ?? 0, State = vital.HasPain ? "HasPain" : "NoPain",
                    RecordedAt = vital.ObservationDateTime, SourceId = vital.Id, SourceKind = "VitalSign"
                };
            }

            var fallRisk = await _dbContext.Set<TrxPatientAssessment>().AsNoTracking()
                .Where(x => x.EncounterId == episode.EncounterId && x.PatientId == episode.PatientId && !x.IsDelete &&
                    !x.IsCancel && x.AssessmentType == PatientAssessmentType.FallRisk &&
                    x.AssessmentStatus == PatientAssessmentStatus.Completed)
                .OrderByDescending(x => x.AssessmentDateTime)
                .Select(x => new { x.Id, x.FallRiskScore, x.FallRiskStatus, x.AssessmentDateTime })
                .FirstOrDefaultAsync(cancellationToken);
            if (fallRisk != null)
            {
                snapshot.FallRisk = new TransferHandoverFallRiskSnapshot
                {
                    Score = fallRisk.FallRiskScore, Status = fallRisk.FallRiskStatus.ToString(),
                    RecordedAt = fallRisk.AssessmentDateTime, AssessmentId = fallRisk.Id
                };
            }

            var windowStart = now - FluidBalanceWindow;
            var fluids = await _dbContext.CliFluidBalanceEntries.AsNoTracking()
                .Where(x => x.InpEpisodeId == episodeId && !x.IsDelete && x.EntryStatus == ClinicalMeasurementStatus.Active &&
                    x.EntryDateTime >= windowStart && x.EntryDateTime <= now)
                .Select(x => new { x.Direction, x.VolumeMl })
                .ToListAsync(cancellationToken);
            if (fluids.Count > 0)
            {
                var intake = fluids.Where(x => x.Direction == FluidDirection.Intake).Sum(x => x.VolumeMl);
                var output = fluids.Where(x => x.Direction == FluidDirection.Output).Sum(x => x.VolumeMl);
                snapshot.FluidBalance = new TransferHandoverFluidBalanceSnapshot
                {
                    WindowStart = windowStart, WindowEnd = now, IntakeMl = intake, OutputMl = output,
                    BalanceMl = intake - output, EntryCount = fluids.Count
                };
            }

            return snapshot;
        }

        private async Task<CliTransferHandoverResult> SaveAndReadAsync(CliTransferHandover document, string message,
            CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                _dbContext.ChangeTracker.Clear();
                return CliTransferHandoverResult.VersionChanged();
            }

            await _loggerService.AuditAsync(LogCategory, "CliTransferHandover.Update",
                message,
                new { DocumentId = document.Id, Status = document.Status.ToString(), document.Version });
            return CliTransferHandoverResult.Ok(await GetByIdAsync(document.Id, cancellationToken), message);
        }

        private static TransferHandoverSnapshot? Deserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;
            try { return JsonSerializer.Deserialize<TransferHandoverSnapshot>(json, SnapshotJson); }
            catch (JsonException) { return null; }
        }

        private static string? Truncate(string? value, int max)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var trimmed = value.Trim();
            return trimmed.Length <= max ? trimmed : trimmed[..max];
        }
    }

    /// <summary>Hasil tindakan pada dokumen serah terima transfer.</summary>
    public sealed class CliTransferHandoverResult
    {
        public int StatusCode { get; private init; }
        public string? Code { get; private init; }
        public string Message { get; private init; } = string.Empty;
        public TransferHandoverResponse? Data { get; private init; }
        public bool IsSuccess => StatusCode == StatusCodes.Status200OK;

        public static CliTransferHandoverResult Ok(TransferHandoverResponse? data, string message) =>
            new() { StatusCode = StatusCodes.Status200OK, Data = data, Message = message };

        public static CliTransferHandoverResult Fail(int statusCode, string? code, string message) =>
            new() { StatusCode = statusCode, Code = code, Message = message };

        public static CliTransferHandoverResult NotFound() =>
            Fail(StatusCodes.Status404NotFound, null, "Dokumen serah terima transfer tidak ditemukan.");

        public static CliTransferHandoverResult VersionChanged() =>
            Fail(StatusCodes.Status409Conflict, null, "Dokumen telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
    }
}
