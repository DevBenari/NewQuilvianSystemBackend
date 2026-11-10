using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using R = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services.RhbResult<QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs.RehabSessionDetailResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Sesi terapi — <c>BE-RHB-012</c>, FR-RHB-030..032, 036, 037; <c>state-transition-matrix.md</c> §5;
    /// V-SES-01..10; RHB-INV-04 (sesi hanya pada program aktif dan kunjungan terbuka), RHB-INV-06
    /// (<c>NotDone</c> tidak pernah tertagih).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Dokumen klinis jenis <c>RehabSession</c> (49): didaftarkan saat dimulai/dicatat, setiap ubah lewat
    /// <c>EnsureMutableAsync</c>, dikunci saat ditandatangani. Nomor sesi dari <c>NumberSeriesAllocator</c>
    /// deret <c>RHB_SESSION</c>; nomor yang gagal tersimpan hangus (INV-PLT-002).
    /// </para>
    /// <para>
    /// <b>Billing belum tersambung.</b> Sampai <c>BE-RHB-013</c> selesai, tanda tangan tidak membentuk
    /// tindakan bertarif: <c>BillingHandoffStatus</c> tetap <c>NotRequired</c> dan
    /// <c>PatientProcedureId</c> intervensi tetap kosong.
    /// </para>
    /// <para>
    /// <b>Kewenangan atas data.</b> Memulai/mencatat hanya oleh pemegang profesi layanan program
    /// (<c>RHB-VAL-031</c>); seluruh perubahan sesudahnya hanya oleh pelaksana (<c>RHB-VAL-022</c>).
    /// </para>
    /// </remarks>
    public class RhbSessionService
    {
        private static readonly TimeSpan ClockSkewTolerance = TimeSpan.FromMinutes(2);
        private const int MaxInterventions = 30;

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly RhbDocumentIntegrityGateway _integrity;
        private readonly RhbProfessionAuthority _professionAuthority;
        private readonly RhbBillingHandoffService _billingHandoff;

        public RhbSessionService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator,
            RhbDocumentIntegrityGateway integrity,
            RhbProfessionAuthority professionAuthority,
            RhbBillingHandoffService billingHandoff)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
            _integrity = integrity;
            _professionAuthority = professionAuthority;
            _billingHandoff = billingHandoff;
        }

        // =====================================================================
        // Label
        // =====================================================================

        public static string StatusName(RehabSessionStatus status) => status switch
        {
            RehabSessionStatus.InProgress => "Berlangsung",
            RehabSessionStatus.Completed => "Selesai",
            RehabSessionStatus.StoppedEarly => "Dihentikan dini",
            RehabSessionStatus.NotDone => "Tidak terlaksana",
            _ => status.ToString()
        };

        public static string NotDoneReasonName(RehabNotDoneReason reason) => reason switch
        {
            RehabNotDoneReason.NoShow => "Pasien tidak hadir",
            RehabNotDoneReason.CancelledByPatient => "Dibatalkan pasien",
            RehabNotDoneReason.Postponed => "Ditunda",
            RehabNotDoneReason.PatientRefused => "Pasien menolak",
            RehabNotDoneReason.ClinicalCondition => "Kondisi klinis tidak memungkinkan",
            RehabNotDoneReason.ResourceUnavailable => "Sumber daya tidak tersedia",
            _ => reason.ToString()
        };

        public static string DeviationName(RehabDeviationType type) => type switch
        {
            RehabDeviationType.None => "Tanpa deviasi",
            RehabDeviationType.Modified => "Dimodifikasi",
            RehabDeviationType.StoppedEarly => "Dihentikan dini",
            RehabDeviationType.Escalated => "Dieskalasi",
            _ => type.ToString()
        };

        public static string BillingName(RehabBillingHandoffStatus status) => status switch
        {
            RehabBillingHandoffStatus.NotRequired => "Tidak ditagihkan",
            RehabBillingHandoffStatus.Pending => "Menunggu",
            RehabBillingHandoffStatus.Succeeded => "Terkirim",
            RehabBillingHandoffStatus.Failed => "Gagal",
            _ => status.ToString()
        };

        private static string IntegrityStatusName(ClinicalDocumentIntegrityStatus status) => status switch
        {
            ClinicalDocumentIntegrityStatus.Draft => "Draf",
            ClinicalDocumentIntegrityStatus.Signed => "Ditandatangani",
            ClinicalDocumentIntegrityStatus.LockedUnsigned => "Terkunci, tidak ditandatangani",
            ClinicalDocumentIntegrityStatus.Cancelled => "Dibatalkan",
            _ => status.ToString()
        };

        // =====================================================================
        // Worklist
        // =====================================================================

        public async Task<RehabSessionFilterMetadataResponse> BuildFilterMetadataAsync(CancellationToken cancellationToken) => new()
        {
            CustomPeriods =
            [
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thisMonth", Label = "Bulan ini" },
                new() { Value = "thisYear", Label = "Tahun ini" },
                new() { Value = "custom", Label = "Pilih tanggal", UsesStartDate = true, UsesEndDate = true }
            ],
            StatusOptions = Enum.GetValues<RehabSessionStatus>().Select(x => Option((int)x, StatusName(x))).ToList(),
            RecordStatusOptions = Enum.GetValues<RehabRecordStatus>().Select(x => Option((int)x, RhbEpisodeService.RecordStatusName(x))).ToList(),
            BillingHandoffStatusOptions = Enum.GetValues<RehabBillingHandoffStatus>().Select(x => Option((int)x, BillingName(x))).ToList(),
            NotDoneReasonOptions = Enum.GetValues<RehabNotDoneReason>().Select(x => Option((int)x, NotDoneReasonName(x))).ToList(),
            DisciplineOptions = await _dbContext.RhbDisciplines.AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.DisciplineName)
                .Select(x => new RehabOptionResponse { Value = x.Id.ToString(), Label = x.DisciplineName })
                .ToListAsync(cancellationToken),
            TherapistOptions = await _dbContext.Users.AsNoTracking()
                .Where(u => _dbContext.RhbSessions.Any(s => !s.IsDelete && s.TherapistUserId == u.Id))
                .OrderBy(u => u.DisplayName)
                .Select(u => new RehabOptionResponse { Value = u.Id.ToString(), Label = u.DisplayName ?? u.Id.ToString() })
                .ToListAsync(cancellationToken),
            SortOptions =
            [
                new() { Value = "sessionAt", Label = "Waktu sesi" },
                new() { Value = "sessionNumber", Label = "Nomor sesi" },
                new() { Value = "sessionStatus", Label = "Status" }
            ],
            SortDirections = ["asc", "desc"],
            PageSizeOptions = [10, 25, 50, 100],
            QueryParameters =
            [
                new() { Name = "startDate", Type = "date", Description = "Awal rentang tanggal sesi", Example = "2026-11-01" },
                new() { Name = "endDate", Type = "date", Description = "Akhir rentang tanggal sesi", Example = "2026-11-30" },
                new() { Name = "customPeriod", Type = "string", Description = "today, last7days, thisMonth, thisYear, custom", Example = "today" },
                new() { Name = "status", Type = "integer", Description = "1 Berlangsung, 2 Selesai, 3 Dihentikan dini, 4 Tidak terlaksana", Example = "1" },
                new() { Name = "recordStatus", Type = "integer", Description = "1 Draf, 2 Final, 3 Dibatalkan", Example = "1" },
                new() { Name = "disciplineId", Type = "uuid", Description = "Layanan program" },
                new() { Name = "therapistUserId", Type = "uuid", Description = "Pelaksana" },
                new() { Name = "mine", Type = "boolean", Description = "Hanya sesi yang saya laksanakan", Example = "true" },
                new() { Name = "billingHandoffStatus", Type = "integer", Description = "0 Tidak ditagihkan, 1 Menunggu, 2 Terkirim, 3 Gagal", Example = "3" },
                new() { Name = "search", Type = "string", Description = "Nomor sesi/episode, nama pasien, atau nomor rekam medis", Example = "RHB-SES" },
                new() { Name = "sortBy", Type = "string", Description = "sessionAt, sessionNumber, sessionStatus", Example = "sessionAt" },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc", Example = "desc" },
                new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman, mulai 1", Example = "1" },
                new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, maksimum 100", Example = "25" }
            ]
        };

        public async Task<RhbResult<RehabSessionSummaryResponse>> GetSummaryAsync(RehabSessionQuery query, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (rows, rangeError) = Filter(query, actorUserId, includeStatus: false);
            if (rangeError != null)
                return RhbResult<RehabSessionSummaryResponse>.Invalid(RhbErrorCodes.PageRangeInvalid, rangeError);

            var counts = await rows
                .GroupBy(x => new { x.SessionStatus, x.RecordStatus })
                .Select(g => new { g.Key.SessionStatus, g.Key.RecordStatus, Count = g.Count() })
                .ToListAsync(cancellationToken);

            int Count(RehabSessionStatus status) => counts.Where(x => x.SessionStatus == status && x.RecordStatus != RehabRecordStatus.Cancelled).Sum(x => x.Count);

            return RhbResult<RehabSessionSummaryResponse>.Ok(new RehabSessionSummaryResponse
            {
                Total = counts.Where(x => x.RecordStatus != RehabRecordStatus.Cancelled).Sum(x => x.Count),
                InProgress = Count(RehabSessionStatus.InProgress),
                Completed = Count(RehabSessionStatus.Completed),
                StoppedEarly = Count(RehabSessionStatus.StoppedEarly),
                NotDone = Count(RehabSessionStatus.NotDone),
                AwaitingSignature = counts.Where(x => x.RecordStatus == RehabRecordStatus.Draft && x.SessionStatus != RehabSessionStatus.InProgress).Sum(x => x.Count)
            });
        }

        public async Task<RhbResult<PagedResult<RehabSessionListItem>>> GetListAsync(RehabSessionQuery query, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (rows, rangeError) = Filter(query, actorUserId, includeStatus: true);
            if (rangeError != null)
                return RhbResult<PagedResult<RehabSessionListItem>>.Invalid(RhbErrorCodes.PageRangeInvalid, rangeError);

            var (pageNumber, pageSize) = RhbServiceSupport.NormalizePaging(query.PageNumber, query.PageSize);
            var descending = !string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            rows = (query.SortBy?.Trim().ToLowerInvariant()) switch
            {
                "sessionnumber" => descending ? rows.OrderByDescending(x => x.SessionNumber) : rows.OrderBy(x => x.SessionNumber),
                "sessionstatus" => descending ? rows.OrderByDescending(x => x.SessionStatus) : rows.OrderBy(x => x.SessionStatus),
                _ => descending ? rows.OrderByDescending(x => x.StartedAt ?? x.CreateDateTime) : rows.OrderBy(x => x.StartedAt ?? x.CreateDateTime)
            };

            var total = await rows.CountAsync(cancellationToken);
            var items = await ProjectList(rows.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
            await FillListAsync(items, cancellationToken);

            return RhbResult<PagedResult<RehabSessionListItem>>.Ok(new PagedResult<RehabSessionListItem>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = pageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            });
        }

        public async Task<RhbResult<List<RehabSessionListItem>>> ListByEpisodeAsync(Guid episodeId, RehabSessionByEpisodeQuery query, CancellationToken cancellationToken)
        {
            if (!await _dbContext.RhbEpisodes.AsNoTracking().AnyAsync(x => x.Id == episodeId && !x.IsDelete, cancellationToken))
                return RhbResult<List<RehabSessionListItem>>.NotFound("Episode rehab tidak ditemukan.");

            var rows = _dbContext.RhbSessions.AsNoTracking().Where(x => x.EpisodeId == episodeId && !x.IsDelete);
            if (query.ProgramId.HasValue)
                rows = rows.Where(x => x.ProgramId == query.ProgramId.Value);

            var items = await ProjectList(rows.OrderByDescending(x => x.StartedAt ?? x.CreateDateTime)).ToListAsync(cancellationToken);
            await FillListAsync(items, cancellationToken);
            return RhbResult<List<RehabSessionListItem>>.Ok(items);
        }

        public async Task<R> GetDetailAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
        {
            var detail = await LoadAsync(id, actorUserId, cancellationToken);
            return detail == null ? NotFound() : R.Ok(detail);
        }

        // =====================================================================
        // Mulai dan tidak terlaksana
        // =====================================================================

        public async Task<R> StartAsync(Guid programId, StartRehabSessionRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (program, episode, guard) = await LoadProgramForSessionAsync(programId, actorUserId, cancellationToken);
            if (guard != null)
                return guard;

            // V-SES-02/03: kunjungan terbuka milik pasien; pasien RWI belum pulang fisik.
            if (!request.EncounterId.HasValue)
                return R.Rule(RhbErrorCodes.SessionEncounterInvalid, "Pilih kunjungan pasien hari ini yang masih terbuka.").WithFields("encounterId");
            var encounter = await RhbServiceSupport.CheckEncounterAsync(_dbContext, request.EncounterId.Value, episode!.PatientId, cancellationToken);
            // Kepemilikan diperiksa lebih dulu, supaya kunjungan pasien lain tidak membocorkan status pulangnya.
            if (encounter.Exists && encounter.BelongsToPatient && encounter.IsOpen && encounter.PhysicallyDischarged)
                return R.Rule(RhbErrorCodes.PatientDischarged, "Pasien sudah pulang dari rawat inap.").WithFields("encounterId");
            if (!encounter.IsValid)
                return R.Rule(RhbErrorCodes.SessionEncounterInvalid, "Pilih kunjungan pasien hari ini yang masih terbuka.").WithFields("encounterId");

            var inpEpisodeId = await InpatientEpisodeIdAsync(request.EncounterId.Value, encounter.EncounterType, cancellationToken);
            if (request.InpEpisodeId.HasValue && request.InpEpisodeId != inpEpisodeId)
                return R.Rule(RhbErrorCodes.SessionEncounterInvalid, "Perawatan rawat inap tidak sesuai dengan kunjungan.").WithFields("inpEpisodeId");

            // V-SES-04/05: verifikasi identitas dan program, kondisi terkini, kontraindikasi.
            var condition = RhbServiceSupport.Normalize(request.CurrentConditionText);
            var safety = new List<string>();
            if (!request.IdentityVerified) safety.Add("identityVerified");
            if (!request.ProgramVerified) safety.Add("programVerified");
            if (condition == null) safety.Add("currentConditionText");
            if (safety.Count > 0)
                return R.Rule(RhbErrorCodes.SafetyCheckIncomplete, "Verifikasi identitas pasien dan program, lalu catat kondisi terkini sebelum memulai.")
                    .WithFields(safety.ToArray());

            var contraindication = RhbServiceSupport.Normalize(request.ContraindicationText);
            if (request.ContraindicationFound && contraindication == null)
                return R.Rule(RhbErrorCodes.ContraindicationUnexplained, "Jelaskan kontraindikasi yang ditemukan.").WithFields("contraindicationText");
            if (condition!.Length > 1000) return TooLong("currentConditionText", 1000);
            if (contraindication?.Length > 1000) return TooLong("contraindicationText", 1000);

            var number = await AllocateNumberAsync(actorUserId, cancellationToken);
            if (number == null)
                return R.Rule(RhbErrorCodes.NumberAllocationFailed, "Nomor sesi belum dapat diterbitkan. Coba lagi.");

            var now = DateTime.UtcNow;
            var entity = new RhbSession
            {
                SessionNumber = number,
                EpisodeId = program!.EpisodeId,
                ProgramId = program.Id,
                EncounterId = request.EncounterId.Value,
                InpEpisodeId = inpEpisodeId,
                TherapistUserId = actorUserId,
                SessionStatus = RehabSessionStatus.InProgress,
                IdentityVerified = true,
                ProgramVerified = true,
                CurrentConditionText = condition,
                ContraindicationFound = request.ContraindicationFound,
                ContraindicationText = request.ContraindicationFound ? contraindication : null,
                StartedAt = now,
                RecordStatus = RehabRecordStatus.Draft,
                BillingHandoffStatus = RehabBillingHandoffStatus.NotRequired,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            return await CreateAsync(entity, episode.PatientId, "start", actorUserId, now, cancellationToken);
        }

        public async Task<R> RecordNotDoneAsync(Guid programId, RecordNotDoneRehabSessionRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (program, episode, guard) = await LoadProgramForSessionAsync(programId, actorUserId, cancellationToken);
            if (guard != null)
                return guard;

            if (!request.NotDoneReason.HasValue || !Enum.IsDefined(request.NotDoneReason.Value))
                return R.Rule(RhbErrorCodes.NotDoneReasonRequired, "Pilih alasan sesi tidak terlaksana.").WithFields("notDoneReason");

            var note = RhbServiceSupport.Normalize(request.Note);
            if (note?.Length > 1000)
                return TooLong("note", 1000);

            // Pasien tidak hadir bisa tanpa kunjungan; dokumen RM tetap butuh kunjungan, maka dipakai kunjungan
            // penyusunan program. Kunjungan yang dikirim cukup milik pasien (boleh sudah ditutup).
            var encounterId = request.EncounterId ?? program!.AuthoredEncounterId;
            var encounter = await RhbServiceSupport.CheckEncounterAsync(_dbContext, encounterId, episode!.PatientId, cancellationToken);
            if (!encounter.Exists || !encounter.BelongsToPatient)
                return R.Rule(RhbErrorCodes.SessionEncounterInvalid, "Kunjungan tidak ditemukan atau bukan milik pasien episode ini.").WithFields("encounterId");

            var number = await AllocateNumberAsync(actorUserId, cancellationToken);
            if (number == null)
                return R.Rule(RhbErrorCodes.NumberAllocationFailed, "Nomor sesi belum dapat diterbitkan. Coba lagi.");

            var now = DateTime.UtcNow;
            var entity = new RhbSession
            {
                SessionNumber = number,
                EpisodeId = program!.EpisodeId,
                ProgramId = program.Id,
                EncounterId = encounterId,
                InpEpisodeId = await InpatientEpisodeIdAsync(encounterId, encounter.EncounterType, cancellationToken),
                TherapistUserId = actorUserId,
                SessionStatus = RehabSessionStatus.NotDone,
                NotDoneReason = request.NotDoneReason.Value,
                ImmediateOutcomeText = note,
                RecordStatus = RehabRecordStatus.Draft,
                BillingHandoffStatus = RehabBillingHandoffStatus.NotRequired,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            return await CreateAsync(entity, episode.PatientId, "not-done", actorUserId, now, cancellationToken);
        }

        // =====================================================================
        // Selama berlangsung
        // =====================================================================

        public async Task<R> UpdateAsync(Guid id, UpdateRehabSessionRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (entity, program, guard) = await LoadForPerformerAsync(id, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;
            if (entity!.SessionStatus == RehabSessionStatus.NotDone)
                return InvalidState(entity.SessionStatus);

            // Sesi yang sudah selesai/dihenti dini tetapi belum ditandatangani masih boleh dilengkapi intervensi,
            // respons, dan hasil segera (V-SES-10 menuntut respons saat tanda tangan); deviasinya tetap.
            var inProgress = entity.SessionStatus == RehabSessionStatus.InProgress;
            var deviationType = inProgress ? request.DeviationType : entity.DeviationType;
            if (inProgress && (request.DeviationType == RehabDeviationType.StoppedEarly || !Enum.IsDefined(request.DeviationType)))
                return R.Invalid(RhbErrorCodes.DeviationIncomplete, "Henti dini dicatat lewat tindakan Henti Dini.").WithFields("deviationType");

            var (error, interventions) = await ValidateInterventionsAsync(request.Interventions ?? new(), program!, deviationType, cancellationToken);
            if (error != null)
                return error;
            if (entity.SessionStatus == RehabSessionStatus.Completed && interventions!.Count == 0)
                return R.Rule(RhbErrorCodes.InterventionNotRecorded, "Sesi selesai wajib punya tindakan yang diberikan.").WithFields("interventions");

            var deviation = inProgress
                ? ValidateDeviation(request.DeviationType, request.DeviationReasonText, request.ImmediateActionText, request.ContactedPartyText)
                : (null, entity.DeviationReasonText, entity.ImmediateActionText, entity.ContactedPartyText);
            if (deviation.Error != null)
                return deviation.Error;

            var response = RhbServiceSupport.Normalize(request.ResponseText);
            var outcome = RhbServiceSupport.Normalize(request.ImmediateOutcomeText);
            if (response?.Length > 2000) return TooLong("responseText", 2000);
            if (outcome?.Length > 1000) return TooLong("immediateOutcomeText", 1000);

            var now = DateTime.UtcNow;
            entity.ResponseText = response;
            entity.ImmediateOutcomeText = outcome;
            entity.DeviationType = deviationType;
            entity.DeviationReasonText = deviation.Reason;
            entity.ImmediateActionText = deviation.Action;
            entity.ContactedPartyText = deviation.Contacted;
            Touch(entity, actorUserId, now);
            if (inProgress && request.NeedsProgramReview && request.DeviationType != RehabDeviationType.None)
                MarkProgramForReview(program!, actorUserId, now);

            var old = await _dbContext.RhbSessionInterventions.Where(x => x.SessionId == entity.Id && !x.IsDelete).ToListAsync(cancellationToken);
            foreach (var row in old)
            {
                row.IsDelete = true;
                row.DeleteBy = actorUserId;
                row.DeleteDateTime = now;
                row.UpdateBy = actorUserId;
                row.UpdateDateTime = now;
            }
            foreach (var item in interventions!)
            {
                _dbContext.RhbSessionInterventions.Add(new RhbSessionIntervention
                {
                    SessionId = entity.Id,
                    ProgramInterventionId = item.ProgramInterventionId,
                    ProcedureId = item.ProcedureId,
                    ActualParametersText = item.Parameters,
                    DurationMinutes = item.Duration,
                    Quantity = item.Quantity,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }

            var failure = await TrySaveAsync(cancellationToken);
            return failure ?? R.Ok((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        public async Task<R> CompleteAsync(Guid id, RehabVersionRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (entity, _, guard) = await LoadForPerformerAsync(id, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;
            if (entity!.SessionStatus != RehabSessionStatus.InProgress)
                return InvalidState(entity.SessionStatus);

            if (!await _dbContext.RhbSessionInterventions.AsNoTracking().AnyAsync(x => x.SessionId == entity.Id && !x.IsDelete, cancellationToken))
                return R.Rule(RhbErrorCodes.InterventionNotRecorded, "Catat tindakan yang benar-benar diberikan.").WithFields("interventions");

            var now = DateTime.UtcNow;
            entity.SessionStatus = RehabSessionStatus.Completed;
            entity.EndedAt = now;
            Touch(entity, actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Session, entity.Id,
                RehabSessionStatus.InProgress.ToString(), RehabSessionStatus.Completed.ToString(), "complete", null, actorUserId, now);

            var failure = await TrySaveAsync(cancellationToken);
            return failure ?? R.Ok((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        public async Task<R> StopEarlyAsync(Guid id, StopEarlyRehabSessionRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (entity, program, guard) = await LoadForPerformerAsync(id, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;
            if (entity!.SessionStatus != RehabSessionStatus.InProgress)
                return InvalidState(entity.SessionStatus);

            var type = request.DeviationType ?? RehabDeviationType.StoppedEarly;
            if (type is not (RehabDeviationType.StoppedEarly or RehabDeviationType.Escalated))
                return R.Invalid(RhbErrorCodes.DeviationIncomplete, "Jenis deviasi henti dini adalah Dihentikan dini atau Dieskalasi.").WithFields("deviationType");

            var deviation = ValidateDeviation(type, request.DeviationReasonText, request.ImmediateActionText, request.ContactedPartyText);
            if (deviation.Error != null)
                return deviation.Error;

            var now = DateTime.UtcNow;
            entity.SessionStatus = RehabSessionStatus.StoppedEarly;
            entity.EndedAt = now;
            entity.DeviationType = type;
            entity.DeviationReasonText = deviation.Reason;
            entity.ImmediateActionText = deviation.Action;
            entity.ContactedPartyText = deviation.Contacted;
            Touch(entity, actorUserId, now);
            if (request.NeedsProgramReview ?? true)
                MarkProgramForReview(program!, actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Session, entity.Id,
                RehabSessionStatus.InProgress.ToString(), RehabSessionStatus.StoppedEarly.ToString(), "stop-early", deviation.Reason, actorUserId, now);

            var failure = await TrySaveAsync(cancellationToken);
            return failure ?? R.Ok((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        // =====================================================================
        // Tanda tangan dan batal
        // =====================================================================

        public async Task<R> SignAsync(
            Guid id,
            RehabSignRequest request,
            Guid actorUserId,
            string? deviceInfo,
            string? ipAddress,
            CancellationToken cancellationToken)
        {
            var (entity, _, guard) = await LoadForPerformerAsync(id, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;
            if (entity!.SessionStatus == RehabSessionStatus.InProgress)
                return R.Conflict(RhbErrorCodes.InvalidState, "Sesi masih berlangsung. Selesaikan atau hentikan dulu sebelum menandatangani.");

            var hasInterventions = await _dbContext.RhbSessionInterventions.AsNoTracking().AnyAsync(x => x.SessionId == entity.Id && !x.IsDelete, cancellationToken);
            if (hasInterventions && string.IsNullOrWhiteSpace(entity.ResponseText))
                return R.Rule(RhbErrorCodes.ResponseRequired, "Catat respons pasien terhadap terapi.").WithFields("responseText");

            var now = DateTime.UtcNow;
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            entity.RecordStatus = RehabRecordStatus.Final;
            entity.SignedAt = now;
            // BE-RHB-013 (INT-RHB-08): satu TrxPatientProcedure per intervensi di transaksi tanda tangan.
            // Sesi tidak terlaksana atau tanpa intervensi tidak menagih (state §6).
            var billableCount = await _billingHandoff.CreateProceduresAsync(entity, actorUserId, now, cancellationToken);
            entity.BillingHandoffStatus = billableCount > 0 ? RehabBillingHandoffStatus.Pending : RehabBillingHandoffStatus.NotRequired;
            Touch(entity, actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Session, entity.Id,
                RehabRecordStatus.Draft.ToString(), RehabRecordStatus.Final.ToString(), "sign", null, actorUserId, now);

            try
            {
                var (result, _) = await _integrity.SignAsync(ClinicalDocumentKind.RehabSession, entity.Id, actorUserId, deviceInfo, ipAddress, now, cancellationToken);
                if (!result.IsAllowed)
                {
                    DetachPending();
                    return result.StatusCode == StatusCodes.Status403Forbidden
                        ? R.Forbidden(RhbErrorCodes.NotAuthor, result.ErrorMessage ?? "Hanya pelaksana sesi yang dapat menandatanganinya.")
                        : R.Conflict(RhbErrorCodes.DocumentLocked, result.ErrorMessage ?? "Catatan sudah final, gunakan addendum.");
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                DetachPending();
                return Stale();
            }

            await transaction.CommitAsync(cancellationToken);

            // INT-RHB-09: fakta charge sesudah commit. Kegagalan Billing tidak membatalkan tanda tangan;
            // status handoff menjadi Failed/Pending untuk diulang lewat /billing-handoff/retry.
            if (entity.BillingHandoffStatus == RehabBillingHandoffStatus.Pending)
                await _billingHandoff.HandOffAsync(entity.Id, actorUserId, cancellationToken);

            return R.Ok((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        public async Task<R> CancelAsync(Guid id, RehabReasonRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var (entity, _, guard) = await LoadForPerformerAsync(id, request.ExpectedVersion, actorUserId, cancellationToken);
            if (guard != null)
                return guard;
            if (entity!.SessionStatus is not (RehabSessionStatus.InProgress or RehabSessionStatus.NotDone))
                return R.Conflict(RhbErrorCodes.InvalidState, $"Sesi berstatus {StatusName(entity.SessionStatus)} tidak dapat dibatalkan. Tanda tangani lalu koreksi lewat addendum.");

            var reason = RhbServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return R.Rule(RhbErrorCodes.ReasonRequired, "Alasan wajib diisi, misalnya salah pasien.").WithFields("reason");
            if (reason.Length > 500)
                return TooLong("reason", 500);

            var now = DateTime.UtcNow;
            entity.RecordStatus = RehabRecordStatus.Cancelled;
            if (entity.SessionStatus == RehabSessionStatus.InProgress)
                entity.EndedAt = now;
            Touch(entity, actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Session, entity.Id,
                RehabRecordStatus.Draft.ToString(), RehabRecordStatus.Cancelled.ToString(), "cancel", reason, actorUserId, now);
            await _integrity.MarkCancelledAsync(ClinicalDocumentKind.RehabSession, entity.Id, actorUserId, reason, now, cancellationToken);

            var failure = await TrySaveAsync(cancellationToken);
            return failure ?? R.Ok((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        // =====================================================================
        // Validasi
        // =====================================================================

        private sealed record InterventionValue(Guid? ProgramInterventionId, Guid ProcedureId, string Parameters, int? Duration, int Quantity);

        /// <summary>
        /// Tindakan harus ada di daftar tindakan aktif layanan program (V-PRG-03). Intervensi rencana harus milik
        /// program sesi dan tindakannya sama; intervensi di luar rencana wajib <c>DeviationType = Modified</c>
        /// (V-SES-08).
        /// </summary>
        private async Task<(R? Error, List<InterventionValue>? Values)> ValidateInterventionsAsync(
            List<RehabSessionInterventionRequest> input,
            RhbProgram program,
            RehabDeviationType deviationType,
            CancellationToken cancellationToken)
        {
            if (input.Count > MaxInterventions)
                return (R.Invalid(RhbErrorCodes.InterventionNotRecorded, $"Intervensi maksimal {MaxInterventions} baris.").WithFields("interventions"), null);

            var allowed = (await (from map in _dbContext.RhbDisciplineProcedures.AsNoTracking()
                                  join procedure in _dbContext.Set<MstProcedure>().AsNoTracking() on map.ProcedureId equals procedure.Id
                                  where map.DisciplineId == program.DisciplineId && map.IsActive && !map.IsDelete && procedure.IsActive && !procedure.IsDelete
                                  select map.ProcedureId).ToListAsync(cancellationToken)).ToHashSet();
            var planned = await _dbContext.RhbProgramInterventions.AsNoTracking()
                .Where(x => x.ProgramId == program.Id && !x.IsDelete)
                .ToDictionaryAsync(x => x.Id, x => x.ProcedureId, cancellationToken);

            var values = new List<InterventionValue>();
            for (var i = 0; i < input.Count; i++)
            {
                var item = input[i];
                var field = $"interventions[{i}]";
                if (!allowed.Contains(item.ProcedureId))
                    return (R.Rule(RhbErrorCodes.InterventionRequired, "Tindakan tidak terdaftar untuk layanan ini.").WithFields($"{field}.procedureId"), null);

                if (item.ProgramInterventionId.HasValue)
                {
                    if (!planned.TryGetValue(item.ProgramInterventionId.Value, out var plannedProcedure) || plannedProcedure != item.ProcedureId)
                        return (R.Rule(RhbErrorCodes.UnplannedInterventionWithoutDeviation, "Intervensi rencana tidak cocok dengan program sesi ini.").WithFields($"{field}.programInterventionId"), null);
                }
                else if (deviationType != RehabDeviationType.Modified)
                {
                    return (R.Rule(RhbErrorCodes.UnplannedInterventionWithoutDeviation,
                        "Tindakan di luar program wajib dicatat sebagai modifikasi beserta alasannya.").WithFields($"{field}.programInterventionId", "deviationType"), null);
                }

                var parameters = RhbServiceSupport.Normalize(item.ActualParametersText);
                if (parameters == null)
                    return (R.Rule(RhbErrorCodes.InterventionNotRecorded, "Catat tindakan aktual yang diberikan.").WithFields($"{field}.actualParametersText"), null);
                if (parameters.Length > 1000)
                    return (TooLong($"{field}.actualParametersText", 1000), null);
                if (item.DurationMinutes is < 1 or > 600)
                    return (R.Invalid(RhbErrorCodes.InterventionNotRecorded, "Durasi 1–600 menit.").WithFields($"{field}.durationMinutes"), null);
                var quantity = item.Quantity ?? 1;
                if (quantity is < 1 or > 99)
                    return (R.Invalid(RhbErrorCodes.InterventionNotRecorded, "Jumlah 1–99.").WithFields($"{field}.quantity"), null);

                values.Add(new InterventionValue(item.ProgramInterventionId, item.ProcedureId, parameters, item.DurationMinutes, quantity));
            }

            return (null, values);
        }

        /// <summary>V-SES-07: deviasi wajib beralasan; henti dini/eskalasi wajib tindakan segera; eskalasi wajib pihak yang dihubungi.</summary>
        private static (R? Error, string? Reason, string? Action, string? Contacted) ValidateDeviation(
            RehabDeviationType type,
            string? reasonText,
            string? actionText,
            string? contactedText)
        {
            if (type == RehabDeviationType.None)
                return (null, null, null, null);

            var reason = RhbServiceSupport.Normalize(reasonText);
            var action = RhbServiceSupport.Normalize(actionText);
            var contacted = RhbServiceSupport.Normalize(contactedText);

            var missing = new List<string>();
            if (reason == null) missing.Add("deviationReasonText");
            if (type is RehabDeviationType.StoppedEarly or RehabDeviationType.Escalated && action == null) missing.Add("immediateActionText");
            if (type == RehabDeviationType.Escalated && contacted == null) missing.Add("contactedPartyText");
            if (missing.Count > 0)
                return (R.Rule(RhbErrorCodes.DeviationIncomplete, "Lengkapi alasan dan tindakan segera.").WithFields(missing.ToArray()), null, null, null);

            if (reason!.Length > 1000) return (TooLong("deviationReasonText", 1000), null, null, null);
            if (action?.Length > 1000) return (TooLong("immediateActionText", 1000), null, null, null);
            if (contacted?.Length > 200) return (TooLong("contactedPartyText", 200), null, null, null);

            return (null, reason, action, contacted);
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        /// <summary>Program aktif (V-SES-01), episode belum ditutup, dan profesi pengguna = layanan program.</summary>
        private async Task<(RhbProgram? Program, RhbEpisode? Episode, R? Error)> LoadProgramForSessionAsync(
            Guid programId,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var program = await _dbContext.RhbPrograms.AsNoTracking().FirstOrDefaultAsync(x => x.Id == programId && !x.IsDelete, cancellationToken);
            if (program == null)
                return (null, null, R.NotFound("Program terapi tidak ditemukan."));

            var episode = await _dbContext.RhbEpisodes.AsNoTracking().FirstAsync(x => x.Id == program.EpisodeId, cancellationToken);
            if (episode.EpisodeStatus == RehabEpisodeStatus.Closed)
                return (null, null, R.Conflict(RhbErrorCodes.EpisodeClosed, "Episode rehab sudah ditutup."));
            if (program.ProgramStatus != RehabProgramStatus.Active)
                return (null, null, R.Conflict(RhbErrorCodes.ProgramNotActive, "Program terapi tidak aktif."));

            if (!await _professionAuthority.CanActForDisciplineAsync(actorUserId, program.DisciplineId, cancellationToken))
                return (null, null, R.Forbidden(RhbErrorCodes.ProfessionMismatch, "Profesi Anda tidak sesuai dengan layanan ini."));

            return (program, episode, null);
        }

        /// <summary>Sesi draft milik pelaksana: versi, status rekam, keutuhan RM, profesi.</summary>
        private async Task<(RhbSession? Entity, RhbProgram? Program, R? Error)> LoadForPerformerAsync(
            Guid id,
            int expectedVersion,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.RhbSessions.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (entity == null)
                return (null, null, NotFound());

            if (await _dbContext.RhbEpisodes.AsNoTracking().AnyAsync(x => x.Id == entity.EpisodeId && x.EpisodeStatus == RehabEpisodeStatus.Closed, cancellationToken))
                return (null, null, R.Conflict(RhbErrorCodes.EpisodeClosed, "Episode rehab sudah ditutup."));

            if (entity.RecordStatus == RehabRecordStatus.Final)
                return (null, null, R.Conflict(RhbErrorCodes.DocumentLocked, "Catatan sudah final, gunakan addendum."));
            if (entity.RecordStatus != RehabRecordStatus.Draft)
                return (null, null, R.Conflict(RhbErrorCodes.InvalidState, "Sesi sudah dibatalkan."));

            if (entity.Version != expectedVersion)
                return (null, null, Stale().WithFields("expectedVersion"));

            if (entity.TherapistUserId != actorUserId)
                return (null, null, R.Forbidden(RhbErrorCodes.NotAuthor, "Hanya pelaksana sesi yang dapat mengubah, menyelesaikan, menandatangani, atau membatalkannya."));

            var program = await _dbContext.RhbPrograms.FirstAsync(x => x.Id == entity.ProgramId, cancellationToken);
            if (!await _professionAuthority.CanActForDisciplineAsync(actorUserId, program.DisciplineId, cancellationToken))
                return (null, null, R.Forbidden(RhbErrorCodes.ProfessionMismatch, "Profesi Anda tidak sesuai dengan layanan ini."));

            var mutable = await _integrity.EnsureMutableAsync(ClinicalDocumentKind.RehabSession, entity.Id, cancellationToken);
            if (!mutable.IsAllowed)
                return (null, null, R.Conflict(RhbErrorCodes.DocumentLocked, mutable.ErrorMessage ?? "Catatan sudah terkunci, gunakan addendum."));

            return (entity, program, null);
        }

        private async Task<R> CreateAsync(RhbSession entity, Guid patientId, string action, Guid actorUserId, DateTime nowUtc, CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            _dbContext.RhbSessions.Add(entity);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Session, entity.Id, null, entity.SessionStatus.ToString(), action, null, actorUserId, nowUtc);
            await _integrity.RegisterDraftAsync(ClinicalDocumentKind.RehabSession, entity.Id, patientId, entity.EncounterId, actorUserId, cancellationToken);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return failure;

            await transaction.CommitAsync(cancellationToken);
            return R.Created((await LoadAsync(entity.Id, actorUserId, cancellationToken))!);
        }

        private Task<string?> AllocateNumberAsync(Guid actorUserId, CancellationToken cancellationToken) =>
            RhbServiceSupport.AllocateNumberAsync(_numberSeriesAllocator, RhbNumberSeries.SessionSequenceKey, RhbNumberSeries.SessionPrefix, actorUserId, cancellationToken);

        private async Task<Guid?> InpatientEpisodeIdAsync(Guid encounterId, EncounterType? type, CancellationToken cancellationToken) =>
            type != EncounterType.Inpatient
                ? null
                : await _dbContext.Set<InpEpisode>().AsNoTracking()
                    .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                    .OrderByDescending(x => x.CreateDateTime)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

        /// <summary>Deviasi yang memengaruhi sesi berikutnya menandai program perlu telaah tanpa mengubah statusnya (§4).</summary>
        private static void MarkProgramForReview(RhbProgram program, Guid actorUserId, DateTime nowUtc)
        {
            if (program.NeedsReview)
                return;
            program.NeedsReview = true;
            program.UpdateBy = actorUserId;
            program.UpdateDateTime = nowUtc;
            program.Version++;
        }

        private static void Touch(RhbSession entity, Guid actorUserId, DateTime nowUtc)
        {
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = nowUtc;
            entity.Version++;
        }

        private (IQueryable<RhbSession> Rows, string? RangeError) Filter(RehabSessionQuery query, Guid actorUserId, bool includeStatus)
        {
            var (start, end, rangeError) = RhbServiceSupport.ResolveRange(query.StartDate, query.EndDate, query.CustomPeriod);
            var rows = _dbContext.RhbSessions.AsNoTracking().Where(x => !x.IsDelete);
            if (rangeError != null)
                return (rows, rangeError);

            if (start.HasValue)
                rows = rows.Where(x => (x.StartedAt ?? x.CreateDateTime) >= start.Value);
            if (end.HasValue)
                rows = rows.Where(x => (x.StartedAt ?? x.CreateDateTime) < end.Value);
            if (includeStatus && query.Status.HasValue)
                rows = rows.Where(x => x.SessionStatus == query.Status.Value);
            if (query.RecordStatus.HasValue)
                rows = rows.Where(x => x.RecordStatus == query.RecordStatus.Value);
            if (query.DisciplineId.HasValue)
                rows = rows.Where(x => x.Program!.DisciplineId == query.DisciplineId.Value);
            if (query.TherapistUserId.HasValue)
                rows = rows.Where(x => x.TherapistUserId == query.TherapistUserId.Value);
            if (query.Mine == true)
                rows = rows.Where(x => x.TherapistUserId == actorUserId);
            if (query.BillingHandoffStatus.HasValue)
                rows = rows.Where(x => x.BillingHandoffStatus == query.BillingHandoffStatus.Value);

            var keyword = RhbServiceSupport.Normalize(query.Search)?.ToLower();
            if (keyword != null)
            {
                rows = rows.Where(x =>
                    x.SessionNumber.ToLower().Contains(keyword) ||
                    x.Episode!.EpisodeNumber.ToLower().Contains(keyword) ||
                    _dbContext.Set<MstPatient>().Any(p => p.Id == x.Episode.PatientId &&
                        (p.FullName.ToLower().Contains(keyword) || p.MedicalRecordNumber.ToLower().Contains(keyword))));
            }

            return (rows, null);
        }

        private IQueryable<RehabSessionListItem> ProjectList(IQueryable<RhbSession> rows) =>
            rows.Select(x => new RehabSessionListItem
            {
                Id = x.Id,
                SessionNumber = x.SessionNumber,
                EpisodeId = x.EpisodeId,
                EpisodeNumber = x.Episode!.EpisodeNumber,
                PatientId = x.Episode.PatientId,
                PatientName = _dbContext.Set<MstPatient>().Where(p => p.Id == x.Episode.PatientId).Select(p => p.FullName).FirstOrDefault(),
                MedicalRecordNumber = _dbContext.Set<MstPatient>().Where(p => p.Id == x.Episode.PatientId).Select(p => p.MedicalRecordNumber).FirstOrDefault(),
                ProgramId = x.ProgramId,
                ProgramVersion = x.Program!.ProgramVersion,
                DisciplineId = x.Program.DisciplineId,
                DisciplineName = x.Program.Discipline!.DisciplineName,
                EncounterId = x.EncounterId,
                EncounterNumber = _dbContext.Set<RegPatientEncounter>().Where(e => e.Id == x.EncounterId).Select(e => e.EncounterNumber).FirstOrDefault(),
                TherapistUserId = x.TherapistUserId,
                SessionAt = x.StartedAt ?? x.CreateDateTime,
                EndedAt = x.EndedAt,
                SessionStatus = x.SessionStatus,
                RecordStatus = x.RecordStatus,
                DeviationType = x.DeviationType,
                BillingHandoffStatus = x.BillingHandoffStatus,
                InterventionCount = x.Interventions.Count(i => !i.IsDelete),
                Version = x.Version
            });

        private async Task FillListAsync<T>(List<T> items, CancellationToken cancellationToken) where T : RehabSessionListItem
        {
            var therapistIds = items.Select(x => x.TherapistUserId).Distinct().ToList();
            var therapists = await _dbContext.Users.AsNoTracking()
                .Where(u => therapistIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken);
            foreach (var item in items)
            {
                item.SessionStatusName = StatusName(item.SessionStatus);
                item.RecordStatusName = RhbEpisodeService.RecordStatusName(item.RecordStatus);
                item.BillingHandoffStatusName = BillingName(item.BillingHandoffStatus);
                item.TherapistName = therapists.TryGetValue(item.TherapistUserId, out var name) ? name : null;
            }
        }

        private async Task<RehabSessionDetailResponse?> LoadAsync(Guid id, Guid actorUserId, CancellationToken cancellationToken)
        {
            var row = await _dbContext.RhbSessions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
            if (row == null)
                return null;

            var list = await ProjectList(_dbContext.RhbSessions.AsNoTracking().Where(x => x.Id == id)).FirstAsync(cancellationToken);
            var detail = new RehabSessionDetailResponse
            {
                Id = list.Id, SessionNumber = list.SessionNumber, EpisodeId = list.EpisodeId, EpisodeNumber = list.EpisodeNumber,
                PatientId = list.PatientId, PatientName = list.PatientName, MedicalRecordNumber = list.MedicalRecordNumber,
                ProgramId = list.ProgramId, ProgramVersion = list.ProgramVersion, DisciplineId = list.DisciplineId, DisciplineName = list.DisciplineName,
                EncounterId = list.EncounterId, EncounterNumber = list.EncounterNumber, TherapistUserId = list.TherapistUserId,
                SessionAt = list.SessionAt, EndedAt = list.EndedAt, SessionStatus = list.SessionStatus, RecordStatus = list.RecordStatus,
                DeviationType = list.DeviationType, BillingHandoffStatus = list.BillingHandoffStatus, InterventionCount = list.InterventionCount,
                Version = list.Version,
                InpEpisodeId = row.InpEpisodeId,
                NotDoneReason = row.NotDoneReason,
                NotDoneReasonName = row.NotDoneReason.HasValue ? NotDoneReasonName(row.NotDoneReason.Value) : null,
                IdentityVerified = row.IdentityVerified,
                ProgramVerified = row.ProgramVerified,
                CurrentConditionText = row.CurrentConditionText,
                ContraindicationFound = row.ContraindicationFound,
                ContraindicationText = row.ContraindicationText,
                StartedAt = row.StartedAt,
                ResponseText = row.ResponseText,
                ImmediateOutcomeText = row.ImmediateOutcomeText,
                DeviationTypeName = DeviationName(row.DeviationType),
                DeviationReasonText = row.DeviationReasonText,
                ImmediateActionText = row.ImmediateActionText,
                ContactedPartyText = row.ContactedPartyText,
                SignedAt = row.SignedAt,
                BillingHandoffAt = row.BillingHandoffAt,
                BillingHandoffError = row.BillingHandoffError,
                ProgramNeedsReview = await _dbContext.RhbPrograms.AsNoTracking().Where(p => p.Id == row.ProgramId).Select(p => p.NeedsReview).FirstAsync(cancellationToken)
            };
            await FillListAsync([detail], cancellationToken);

            detail.Interventions = await (
                    from i in _dbContext.RhbSessionInterventions.AsNoTracking()
                    join p in _dbContext.Set<MstProcedure>().AsNoTracking() on i.ProcedureId equals p.Id into procedures
                    from p in procedures.DefaultIfEmpty()
                    where i.SessionId == id && !i.IsDelete
                    orderby p.ProcedureName
                    select new RehabSessionInterventionItem
                    {
                        Id = i.Id,
                        ProgramInterventionId = i.ProgramInterventionId,
                        IsPlanned = i.ProgramInterventionId != null,
                        ProcedureId = i.ProcedureId,
                        ProcedureCode = p != null ? p.ProcedureCode : null,
                        ProcedureName = p != null ? p.ProcedureName : null,
                        ActualParametersText = i.ActualParametersText,
                        DurationMinutes = i.DurationMinutes,
                        Quantity = i.Quantity,
                        PatientProcedureId = i.PatientProcedureId
                    })
                .ToListAsync(cancellationToken);

            detail.PlannedInterventions = await (
                    from i in _dbContext.RhbProgramInterventions.AsNoTracking()
                    join p in _dbContext.Set<MstProcedure>().AsNoTracking() on i.ProcedureId equals p.Id into procedures
                    from p in procedures.DefaultIfEmpty()
                    where i.ProgramId == row.ProgramId && !i.IsDelete
                    orderby p.ProcedureName
                    select new RehabProgramInterventionItem
                    {
                        ProgramInterventionId = i.Id,
                        ProcedureId = i.ProcedureId,
                        ProcedureCode = p != null ? p.ProcedureCode : null,
                        ProcedureName = p != null ? p.ProcedureName : null,
                        ParametersText = i.ParametersText
                    })
                .ToListAsync(cancellationToken);

            var integrity = await _integrity.FindAsync(ClinicalDocumentKind.RehabSession, id, cancellationToken);
            detail.IntegrityStatus = integrity?.IntegrityStatus;
            detail.IntegrityStatusName = integrity != null ? IntegrityStatusName(integrity.IntegrityStatus) : null;

            var performer = row.TherapistUserId == actorUserId;
            var mutable = row.RecordStatus == RehabRecordStatus.Draft && integrity?.IntegrityStatus is null or ClinicalDocumentIntegrityStatus.Draft;
            detail.AvailableActions = row.RecordStatus == RehabRecordStatus.Final || integrity?.IntegrityStatus == ClinicalDocumentIntegrityStatus.LockedUnsigned
                ? ["addendum"]
                : !mutable || !performer
                    ? []
                    : row.SessionStatus switch
                    {
                        RehabSessionStatus.InProgress => ["update", "complete", "stop-early", "cancel"],
                        RehabSessionStatus.NotDone => ["sign", "cancel"],
                        _ => ["update", "sign"]
                    };

            return detail;
        }

        private async Task<R?> TrySaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return null;
            }
            catch (DbUpdateConcurrencyException)
            {
                DetachPending();
                return Stale();
            }
            catch (DbUpdateException exception) when (RhbServiceSupport.IsUniqueViolation(exception))
            {
                DetachPending();
                return R.Conflict(RhbErrorCodes.InvalidState, "Sesi ini sudah diproses pengguna lain. Muat ulang data.");
            }
        }

        private void DetachPending()
        {
            foreach (var entry in _dbContext.ChangeTracker.Entries().ToList())
            {
                if (entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                    entry.State = EntityState.Detached;
            }
        }

        private static RehabOptionResponse Option(int value, string label) => new() { Value = value.ToString(), Label = label };

        private static R InvalidState(RehabSessionStatus status) =>
            R.Conflict(RhbErrorCodes.InvalidState, $"Tindakan tidak dapat dilakukan pada sesi berstatus {StatusName(status)}.");

        private static R NotFound() => R.NotFound("Sesi terapi tidak ditemukan.");

        private static R Stale() => R.Conflict(RhbErrorCodes.StaleVersion, "Data sudah diubah pengguna lain. Muat ulang sebelum menyimpan.");

        private static R TooLong(string field, int max) =>
            R.Invalid(RhbErrorCodes.FieldTooLong, $"Isian terlalu panjang (maksimal {max} karakter).").WithFields(field);
    }
}
