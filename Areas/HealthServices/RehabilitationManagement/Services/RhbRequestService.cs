using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
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
using R = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services.RhbResult<QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs.RehabRequestDetailResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Permintaan rehab dan disposisinya (<c>BE-RHB-007</c>; FR-RHB-001..003; FEAT-001..003;
    /// <c>state-transition-matrix.md</c> §1; V-REQ-01..09, V-GEN-01/02).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Dua tahap validasi.</b> Draft boleh belum lengkap, tetapi isian yang <i>sudah</i> diisi
    /// harus sah: pasien aktif, kunjungan milik pasien, layanan aktif. Kelengkapan (indikasi, dokter
    /// peminta, layanan minimal satu, duplikasi) baru ditagih saat <c>submit</c>. Pengecualian:
    /// sumber internal wajib berkunjungan sejak draft, karena <c>CK_RhbRequest_Source</c> menolak
    /// baris tanpa kunjungan.
    /// </para>
    /// <para>
    /// <b>Hak akses tidak diperiksa di sini.</b> Pembedaan <c>Return</c> (administratif) dan
    /// <c>Disposition</c> (klinis) dijaga atribut <c>[AccessPermission]</c> di controller
    /// (RHB-INV-02). Service menjaga status, versi, dan kepemilikan data.
    /// </para>
    /// <para>
    /// <b>Konkurensi.</b> Setiap ubah membawa <c>expectedVersion</c>; <c>Version</c> adalah
    /// concurrency token, sehingga dua penerimaan bersamaan hanya meloloskan satu (V-GEN-01).
    /// </para>
    /// </remarks>
    public class RhbRequestService
    {
        public const int IndicationMinLength = 5;

        /// <summary>Toleransi selisih jam perangkat klien untuk V-REQ-06.</summary>
        private static readonly TimeSpan ClockSkewTolerance = TimeSpan.FromMinutes(2);

        private static readonly RehabRequestStatus[] EditableStatuses =
            { RehabRequestStatus.Draft, RehabRequestStatus.NeedsClarification };

        private static readonly RehabRequestStatus[] CancellableStatuses =
            { RehabRequestStatus.Draft, RehabRequestStatus.Submitted, RehabRequestStatus.NeedsClarification };

        private static readonly RehabRequestStatus[] RunningStatuses =
            { RehabRequestStatus.Submitted, RehabRequestStatus.NeedsClarification };

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly RhbEpisodeService _episodeService;

        public RhbRequestService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator,
            RhbEpisodeService episodeService)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
            _episodeService = episodeService;
        }

        // =====================================================================
        // Label
        // =====================================================================

        public static string StatusName(RehabRequestStatus status) => status switch
        {
            RehabRequestStatus.Draft => "Draf",
            RehabRequestStatus.Submitted => "Diajukan",
            RehabRequestStatus.NeedsClarification => "Perlu klarifikasi",
            RehabRequestStatus.Accepted => "Diterima",
            RehabRequestStatus.Redirected => "Dialihkan",
            RehabRequestStatus.Rejected => "Ditolak",
            RehabRequestStatus.Cancelled => "Dibatalkan",
            _ => status.ToString()
        };

        public static string SourceName(RehabRequestSource source) => source switch
        {
            RehabRequestSource.Outpatient => "Rawat jalan",
            RehabRequestSource.Inpatient => "Rawat inap",
            RehabRequestSource.Emergency => "Gawat darurat",
            RehabRequestSource.ExternalReferral => "Rujukan eksternal",
            _ => source.ToString()
        };

        public static string EpisodeStatusName(RehabEpisodeStatus status) => status switch
        {
            RehabEpisodeStatus.AwaitingMedicalAssessment => "Menunggu asesmen medis",
            RehabEpisodeStatus.Planning => "Perencanaan",
            RehabEpisodeStatus.Active => "Terapi berjalan",
            RehabEpisodeStatus.Closed => "Ditutup",
            _ => status.ToString()
        };

        /// <summary>Aksi yang sah menurut status — <c>state-transition-matrix.md</c> §1.</summary>
        public static List<string> AvailableActions(RehabRequestStatus status) => status switch
        {
            RehabRequestStatus.Draft or RehabRequestStatus.NeedsClarification => ["update", "submit", "cancel"],
            RehabRequestStatus.Submitted => ["return", "request-clarification", "accept", "redirect", "reject", "cancel"],
            _ => []
        };

        // =====================================================================
        // Baca
        // =====================================================================

        public async Task<RehabRequestFilterMetadataResponse> BuildFilterMetadataAsync(CancellationToken cancellationToken) => new()
        {
            CustomPeriods =
            [
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thisMonth", Label = "Bulan ini" },
                new() { Value = "thisYear", Label = "Tahun ini" },
                new() { Value = "custom", Label = "Pilih tanggal", UsesStartDate = true, UsesEndDate = true }
            ],
            StatusOptions = Enum.GetValues<RehabRequestStatus>()
                .Select(x => new RehabOptionResponse { Value = ((int)x).ToString(), Label = StatusName(x) })
                .ToList(),
            SourceOptions = Enum.GetValues<RehabRequestSource>()
                .Select(x => new RehabOptionResponse { Value = ((int)x).ToString(), Label = SourceName(x) })
                .ToList(),
            DisciplineOptions = await _dbContext.RhbDisciplines.AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.DisciplineName)
                .Select(x => new RehabOptionResponse { Value = x.Id.ToString(), Label = x.DisciplineName })
                .ToListAsync(cancellationToken),
            SortOptions =
            [
                new() { Value = "requestedAt", Label = "Waktu permintaan" },
                new() { Value = "requestNumber", Label = "Nomor permintaan" },
                new() { Value = "requestStatus", Label = "Status" },
                new() { Value = "createDateTime", Label = "Tanggal dibuat" }
            ],
            SortDirections = ["asc", "desc"],
            PageSizeOptions = [10, 25, 50, 100],
            QueryParameters =
            [
                new() { Name = "startDate", Type = "date", Description = "Awal rentang waktu permintaan", Example = "2026-11-01" },
                new() { Name = "endDate", Type = "date", Description = "Akhir rentang waktu permintaan", Example = "2026-11-30" },
                new() { Name = "customPeriod", Type = "string", Description = "today, last7days, thisMonth, thisYear, custom", Example = "thisMonth" },
                new() { Name = "status", Type = "integer", Description = "1 Draf, 2 Diajukan, 3 Perlu klarifikasi, 4 Diterima, 5 Dialihkan, 6 Ditolak, 7 Dibatalkan", Example = "2" },
                new() { Name = "sourceType", Type = "integer", Description = "1 Rawat jalan, 2 Rawat inap, 3 Gawat darurat, 4 Rujukan eksternal", Example = "1" },
                new() { Name = "disciplineId", Type = "uuid", Description = "Layanan rehab yang diminta" },
                new() { Name = "search", Type = "string", Description = "Nomor permintaan, nama pasien, atau nomor rekam medis", Example = "RHB-REQ" },
                new() { Name = "sortBy", Type = "string", Description = "requestedAt, requestNumber, requestStatus, createDateTime", Example = "requestedAt" },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc", Example = "desc" },
                new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman, mulai 1", Example = "1" },
                new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, maksimum 100", Example = "25" }
            ]
        };

        public async Task<RhbResult<RehabRequestSummaryResponse>> GetSummaryAsync(RehabRequestQuery query, CancellationToken cancellationToken)
        {
            var (rows, rangeError) = Filter(query, includeStatus: false);
            if (rangeError != null)
                return RhbResult<RehabRequestSummaryResponse>.Invalid(RhbErrorCodes.PageRangeInvalid, rangeError);

            var counts = await rows
                .GroupBy(x => x.RequestStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            int Count(RehabRequestStatus status) => counts.Where(x => x.Status == status).Sum(x => x.Count);

            return RhbResult<RehabRequestSummaryResponse>.Ok(new RehabRequestSummaryResponse
            {
                Total = counts.Sum(x => x.Count),
                Draft = Count(RehabRequestStatus.Draft),
                Submitted = Count(RehabRequestStatus.Submitted),
                NeedsClarification = Count(RehabRequestStatus.NeedsClarification),
                Accepted = Count(RehabRequestStatus.Accepted),
                Redirected = Count(RehabRequestStatus.Redirected),
                Rejected = Count(RehabRequestStatus.Rejected),
                Cancelled = Count(RehabRequestStatus.Cancelled)
            });
        }

        public async Task<RhbResult<PagedResult<RehabRequestListItem>>> GetListAsync(RehabRequestQuery query, CancellationToken cancellationToken)
        {
            var (rows, rangeError) = Filter(query, includeStatus: true);
            if (rangeError != null)
                return RhbResult<PagedResult<RehabRequestListItem>>.Invalid(RhbErrorCodes.PageRangeInvalid, rangeError);

            var (pageNumber, pageSize) = RhbServiceSupport.NormalizePaging(query.PageNumber, query.PageSize);
            var descending = !string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            rows = (query.SortBy?.Trim().ToLowerInvariant()) switch
            {
                "requestnumber" => descending ? rows.OrderByDescending(x => x.RequestNumber) : rows.OrderBy(x => x.RequestNumber),
                "requeststatus" => descending ? rows.OrderByDescending(x => x.RequestStatus) : rows.OrderBy(x => x.RequestStatus),
                "createdatetime" => descending ? rows.OrderByDescending(x => x.CreateDateTime) : rows.OrderBy(x => x.CreateDateTime),
                _ => descending ? rows.OrderByDescending(x => x.RequestedAt) : rows.OrderBy(x => x.RequestedAt)
            };

            var total = await rows.CountAsync(cancellationToken);
            var items = await ProjectList(rows.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
            foreach (var item in items)
                LabelList(item);

            return RhbResult<PagedResult<RehabRequestListItem>>.Ok(new PagedResult<RehabRequestListItem>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = pageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            });
        }

        public async Task<RehabRequestDetailResponse?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
        {
            var detail = await _dbContext.RhbRequests
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new RehabRequestDetailResponse
                {
                    Id = x.Id,
                    RequestNumber = x.RequestNumber,
                    PatientId = x.PatientId,
                    PatientName = _dbContext.Set<MstPatient>().Where(p => p.Id == x.PatientId).Select(p => p.FullName).FirstOrDefault(),
                    MedicalRecordNumber = _dbContext.Set<MstPatient>().Where(p => p.Id == x.PatientId).Select(p => p.MedicalRecordNumber).FirstOrDefault(),
                    SourceType = x.SourceType,
                    RequestedAt = x.RequestedAt,
                    RequestingDoctorId = x.RequestingDoctorId,
                    RequestingDoctorName = _dbContext.Set<MstDoctor>().Where(d => d.Id == x.RequestingDoctorId).Select(d => d.FullName).FirstOrDefault(),
                    RequestStatus = x.RequestStatus,
                    Version = x.Version,
                    SourceEncounterId = x.SourceEncounterId,
                    SourceEncounterNumber = _dbContext.Set<RegPatientEncounter>().Where(e => e.Id == x.SourceEncounterId).Select(e => e.EncounterNumber).FirstOrDefault(),
                    InpEpisodeId = x.InpEpisodeId,
                    InternalReferralLetterId = x.InternalReferralLetterId,
                    InternalReferralLetterNumber = _dbContext.Set<CliDoctorCertificate>().Where(c => c.Id == x.InternalReferralLetterId).Select(c => c.CertificateNumber).FirstOrDefault(),
                    ExternalInstitutionName = x.ExternalInstitutionName,
                    ExternalReferralNumber = x.ExternalReferralNumber,
                    RequestingPractitionerName = x.RequestingPractitionerName,
                    IndicationText = x.IndicationText,
                    DuplicateOverrideReason = x.DuplicateOverrideReason,
                    ReturnReason = x.ReturnReason,
                    DispositionReason = x.DispositionReason,
                    RedirectTargetText = x.RedirectTargetText,
                    DispositionByUserId = x.DispositionByUserId,
                    DispositionByName = _dbContext.Users.Where(u => u.Id == x.DispositionByUserId).Select(u => u.DisplayName).FirstOrDefault(),
                    DispositionAt = x.DispositionAt,
                    SubmittedByUserId = x.SubmittedByUserId,
                    SubmittedAt = x.SubmittedAt,
                    CancelReason = x.CancelReason,
                    EpisodeId = x.Episode != null ? (Guid?)x.Episode.Id : null,
                    EpisodeNumber = x.Episode != null ? x.Episode.EpisodeNumber : null,
                    CreateDateTime = x.CreateDateTime,
                    Disciplines = x.Disciplines
                        .Where(d => !d.IsDelete)
                        .OrderBy(d => d.Discipline!.DisciplineName)
                        .Select(d => new RehabRequestDisciplineItem
                        {
                            DisciplineId = d.DisciplineId,
                            DisciplineCode = d.Discipline!.DisciplineCode,
                            DisciplineName = d.Discipline.DisciplineName,
                            IsActive = d.Discipline.IsActive && !d.Discipline.IsDelete
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (detail == null)
                return null;

            detail.DisciplineNames = detail.Disciplines.Select(x => x.DisciplineName).ToList();
            detail.AvailableActions = AvailableActions(detail.RequestStatus);
            LabelList(detail);
            return detail;
        }

        public async Task<RhbResult<List<RehabStatusHistoryItem>>> GetStatusHistoryAsync(Guid id, CancellationToken cancellationToken)
        {
            if (!await _dbContext.RhbRequests.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken))
                return RhbResult<List<RehabStatusHistoryItem>>.NotFound("Permintaan rehab tidak ditemukan.");

            return RhbResult<List<RehabStatusHistoryItem>>.Ok(
                await RhbStatusHistoryWriter.ReadAsync(_dbContext, RhbStatusHistoryWriter.Request, id, cancellationToken));
        }

        public async Task<RhbResult<List<RehabDuplicateCandidate>>> CheckDuplicatesAsync(RehabDuplicateCheckQuery query, CancellationToken cancellationToken)
        {
            if (!query.PatientId.HasValue || query.PatientId.Value == Guid.Empty)
                return RhbResult<List<RehabDuplicateCandidate>>.Invalid(RhbErrorCodes.PatientRequired, "Pilih pasien lebih dulu.").WithFields("patientId");

            return RhbResult<List<RehabDuplicateCandidate>>.Ok(
                await FindDuplicatesAsync(query.PatientId.Value, query.DisciplineIds, query.ExcludeRequestId, cancellationToken));
        }

        // =====================================================================
        // Draft
        // =====================================================================

        public async Task<R> CreateAsync(CreateRehabRequestRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var (error, values) = await ValidateDraftAsync(request, now, cancellationToken);
            if (error != null)
                return error;

            var number = await RhbServiceSupport.AllocateNumberAsync(
                _numberSeriesAllocator, RhbNumberSeries.RequestSequenceKey, RhbNumberSeries.RequestPrefix, actorUserId, cancellationToken);
            if (number == null)
                return R.Rule(RhbErrorCodes.NumberAllocationFailed, "Nomor permintaan belum dapat diterbitkan. Coba lagi.");

            var entity = new RhbRequest
            {
                RequestNumber = number,
                RequestStatus = RehabRequestStatus.Draft,
                CreateDateTime = now,
                CreateBy = actorUserId
            };
            Apply(entity, values!);

            foreach (var disciplineId in values!.DisciplineIds)
                entity.Disciplines.Add(new RhbRequestDiscipline { DisciplineId = disciplineId, CreateDateTime = now, CreateBy = actorUserId });

            _dbContext.RhbRequests.Add(entity);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Request, entity.Id, null,
                entity.RequestStatus.ToString(), "create", null, actorUserId, now);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return failure;

            return R.Created((await GetDetailAsync(entity.Id, cancellationToken))!);
        }

        public async Task<R> UpdateAsync(Guid id, UpdateRehabRequestRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var entity = await FindAsync(id, cancellationToken);
            if (entity == null)
                return NotFound();

            var guard = Guard(entity, request.ExpectedVersion, EditableStatuses);
            if (guard != null)
                return guard;

            var now = DateTime.UtcNow;
            var (error, values) = await ValidateDraftAsync(request, now, cancellationToken);
            if (error != null)
                return error;

            Apply(entity, values!);

            var existing = await _dbContext.RhbRequestDisciplines
                .Where(x => x.RequestId == entity.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var row in existing.Where(x => !values!.DisciplineIds.Contains(x.DisciplineId)))
            {
                row.IsDelete = true;
                row.DeleteBy = actorUserId;
                row.DeleteDateTime = now;
            }

            foreach (var disciplineId in values!.DisciplineIds.Where(d => existing.All(x => x.DisciplineId != d)))
                _dbContext.RhbRequestDisciplines.Add(new RhbRequestDiscipline { RequestId = entity.Id, DisciplineId = disciplineId, CreateDateTime = now, CreateBy = actorUserId });

            Touch(entity, actorUserId, now);
            return await SaveAndReturnAsync(entity, cancellationToken);
        }

        // =====================================================================
        // Aksi status
        // =====================================================================

        public async Task<R> SubmitAsync(Guid id, SubmitRehabRequestRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var entity = await FindAsync(id, cancellationToken);
            if (entity == null)
                return NotFound();

            var guard = Guard(entity, request.ExpectedVersion, EditableStatuses);
            if (guard != null)
                return guard;

            var overrideReason = RhbServiceSupport.Normalize(request.DuplicateOverrideReason);
            if (overrideReason?.Length > 500)
                return TooLong("duplicateOverrideReason", 500);

            var now = DateTime.UtcNow;
            var completeness = await ValidateForSubmitAsync(entity, now, cancellationToken);
            if (completeness != null)
                return completeness;

            // Dua pengajuan bersamaan untuk pasien yang sama mengantre di sini, sehingga pemeriksaan
            // duplikasi yang kedua melihat hasil yang pertama.
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await RhbServiceSupport.AcquireLocksAsync(_dbContext, cancellationToken, $"RHB_REQUEST_PATIENT:{entity.PatientId}");

            var disciplineIds = await ActiveDisciplineIdsAsync(entity.Id, cancellationToken);
            var duplicates = await FindDuplicatesAsync(entity.PatientId, disciplineIds, entity.Id, cancellationToken);
            if (duplicates.Count > 0 && overrideReason == null)
            {
                return R.Conflict(
                    RhbErrorCodes.DuplicateRequest,
                    "Pasien sudah memiliki permintaan/episode rehab yang masih berjalan. Periksa daftar di bawah, atau isi alasan untuk tetap melanjutkan.",
                    duplicates).WithFields("duplicateOverrideReason");
            }

            var from = entity.RequestStatus;
            entity.RequestStatus = RehabRequestStatus.Submitted;
            entity.DuplicateOverrideReason = duplicates.Count > 0 ? overrideReason : null;
            entity.SubmittedByUserId = actorUserId;
            entity.SubmittedAt = now;
            Touch(entity, actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Request, entity.Id, from.ToString(),
                entity.RequestStatus.ToString(), "submit", null, actorUserId, now);

            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return failure;

            await transaction.CommitAsync(cancellationToken);
            return R.Ok((await GetDetailAsync(entity.Id, cancellationToken))!);
        }

        /// <summary>Pengembalian administratif — butir <c>RehabRequest : Return</c>.</summary>
        public Task<R> ReturnAsync(Guid id, RehabReasonRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
            ToClarificationAsync(id, request, "return", actorUserId, cancellationToken);

        /// <summary>Permintaan klarifikasi klinis — butir <c>RehabRequest : Disposition</c>.</summary>
        public Task<R> RequestClarificationAsync(Guid id, RehabReasonRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
            ToClarificationAsync(id, request, "request-clarification", actorUserId, cancellationToken);

        public async Task<R> AcceptAsync(Guid id, AcceptRehabRequestRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var entity = await FindAsync(id, cancellationToken);
            if (entity == null)
                return NotFound();

            var guard = Guard(entity, request.ExpectedVersion, RehabRequestStatus.Submitted);
            if (guard != null)
                return guard;

            var note = RhbServiceSupport.Normalize(request.Note);
            if (note?.Length > 1000)
                return TooLong("note", 1000);

            if (!request.ResponsibleDoctorId.HasValue || !await IsActiveDoctorAsync(request.ResponsibleDoctorId.Value, cancellationToken))
                return R.Rule(RhbErrorCodes.ResponsibleDoctorRequired, "Pilih dokter penanggung jawab rehab.").WithFields("responsibleDoctorId");

            // Alokator menerbitkan nomor di transaksinya sendiri; bila penyimpanan di bawah gagal,
            // nomor itu hangus dan tidak pernah menempel pada episode lain (INV-PLT-002).
            var episodeNumber = await RhbServiceSupport.AllocateNumberAsync(
                _numberSeriesAllocator, RhbNumberSeries.EpisodeSequenceKey, RhbNumberSeries.EpisodePrefix, actorUserId, cancellationToken);
            if (episodeNumber == null)
                return R.Rule(RhbErrorCodes.NumberAllocationFailed, "Nomor episode belum dapat diterbitkan. Coba lagi.");

            var now = DateTime.UtcNow;
            var from = entity.RequestStatus;
            entity.RequestStatus = RehabRequestStatus.Accepted;
            entity.DispositionReason = note;
            entity.DispositionByUserId = actorUserId;
            entity.DispositionAt = now;
            Touch(entity, actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Request, entity.Id, from.ToString(),
                entity.RequestStatus.ToString(), "accept", null, actorUserId, now);

            _episodeService.OpenForAcceptedRequest(entity, request.ResponsibleDoctorId.Value, episodeNumber, actorUserId, now);

            // Satu SaveChanges = satu transaksi basis data: status permintaan, episode baru, dan
            // kedua riwayatnya tersimpan bersama atau tidak sama sekali.
            return await SaveAndReturnAsync(entity, cancellationToken);
        }

        public async Task<R> RedirectAsync(Guid id, RedirectRehabRequestRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var entity = await FindAsync(id, cancellationToken);
            if (entity == null)
                return NotFound();

            var guard = Guard(entity, request.ExpectedVersion, RehabRequestStatus.Submitted);
            if (guard != null)
                return guard;

            var reason = RhbServiceSupport.Normalize(request.Reason);
            var target = RhbServiceSupport.Normalize(request.RedirectTargetText);
            if (reason == null || target == null)
                return R.Rule(RhbErrorCodes.ReasonRequired, "Alasan dan tujuan pengalihan wajib diisi.")
                    .WithFields(reason == null ? "reason" : "redirectTargetText");
            if (reason.Length > 1000)
                return TooLong("reason", 1000);
            if (target.Length > 200)
                return TooLong("redirectTargetText", 200);

            var now = DateTime.UtcNow;
            var from = entity.RequestStatus;
            entity.RequestStatus = RehabRequestStatus.Redirected;
            entity.DispositionReason = reason;
            entity.RedirectTargetText = target;
            entity.DispositionByUserId = actorUserId;
            entity.DispositionAt = now;
            Touch(entity, actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Request, entity.Id, from.ToString(),
                entity.RequestStatus.ToString(), "redirect", reason, actorUserId, now);

            return await SaveAndReturnAsync(entity, cancellationToken);
        }

        public async Task<R> RejectAsync(Guid id, RehabReasonRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var entity = await FindAsync(id, cancellationToken);
            if (entity == null)
                return NotFound();

            var guard = Guard(entity, request.ExpectedVersion, RehabRequestStatus.Submitted);
            if (guard != null)
                return guard;

            var reason = RhbServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return ReasonRequired();
            if (reason.Length > 1000)
                return TooLong("reason", 1000);

            var now = DateTime.UtcNow;
            var from = entity.RequestStatus;
            entity.RequestStatus = RehabRequestStatus.Rejected;
            entity.DispositionReason = reason;
            entity.DispositionByUserId = actorUserId;
            entity.DispositionAt = now;
            Touch(entity, actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Request, entity.Id, from.ToString(),
                entity.RequestStatus.ToString(), "reject", reason, actorUserId, now);

            return await SaveAndReturnAsync(entity, cancellationToken);
        }

        public async Task<R> CancelAsync(Guid id, RehabReasonRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            var entity = await FindAsync(id, cancellationToken);
            if (entity == null)
                return NotFound();

            var guard = Guard(entity, request.ExpectedVersion, CancellableStatuses);
            if (guard != null)
                return guard;

            var reason = RhbServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return ReasonRequired();
            if (reason.Length > 500)
                return TooLong("reason", 500);

            var now = DateTime.UtcNow;
            var from = entity.RequestStatus;
            entity.RequestStatus = RehabRequestStatus.Cancelled;
            entity.CancelReason = reason;
            entity.IsCancel = true;
            entity.CancelBy = actorUserId;
            entity.CancelDateTime = now;
            Touch(entity, actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Request, entity.Id, from.ToString(),
                entity.RequestStatus.ToString(), "cancel", reason, actorUserId, now);

            return await SaveAndReturnAsync(entity, cancellationToken);
        }

        // =====================================================================
        // Penolong
        // =====================================================================

        private sealed record DraftValues(
            Guid PatientId,
            RehabRequestSource SourceType,
            Guid? SourceEncounterId,
            Guid? InpEpisodeId,
            Guid? InternalReferralLetterId,
            string? ExternalInstitutionName,
            string? ExternalReferralNumber,
            Guid? RequestingDoctorId,
            string? RequestingPractitionerName,
            string IndicationText,
            DateTime RequestedAt,
            List<Guid> DisciplineIds);

        private async Task<R> ToClarificationAsync(Guid id, RehabReasonRequest request, string action, Guid actorUserId, CancellationToken cancellationToken)
        {
            var entity = await FindAsync(id, cancellationToken);
            if (entity == null)
                return NotFound();

            var guard = Guard(entity, request.ExpectedVersion, RehabRequestStatus.Submitted);
            if (guard != null)
                return guard;

            var reason = RhbServiceSupport.Normalize(request.Reason);
            if (reason == null)
                return ReasonRequired();
            if (reason.Length > 500)
                return TooLong("reason", 500);

            var now = DateTime.UtcNow;
            var from = entity.RequestStatus;
            entity.RequestStatus = RehabRequestStatus.NeedsClarification;
            entity.ReturnReason = reason;
            Touch(entity, actorUserId, now);
            RhbStatusHistoryWriter.Record(_dbContext, RhbStatusHistoryWriter.Request, entity.Id, from.ToString(),
                entity.RequestStatus.ToString(), action, reason, actorUserId, now);

            return await SaveAndReturnAsync(entity, cancellationToken);
        }

        /// <summary>
        /// Validasi isian draft: yang sudah diisi harus sah. Mengembalikan nilai yang sudah
        /// dinormalkan, termasuk episode rawat inap yang diturunkan dari kunjungan.
        /// </summary>
        private async Task<(R? Error, DraftValues? Values)> ValidateDraftAsync(
            CreateRehabRequestRequest request,
            DateTime nowUtc,
            CancellationToken cancellationToken)
        {
            if (request.PatientId == Guid.Empty ||
                !await _dbContext.Set<MstPatient>().AsNoTracking().AnyAsync(x => x.Id == request.PatientId && !x.IsDelete && x.IsActive, cancellationToken))
                return (R.Rule(RhbErrorCodes.PatientRequired, "Pilih pasien yang terdaftar.").WithFields("patientId"), null);

            if (!Enum.IsDefined(request.SourceType))
                return (R.Invalid(RhbErrorCodes.SourceEncounterInvalid, "Pilih sumber permintaan.").WithFields("sourceType"), null);

            var indication = request.IndicationText?.Trim() ?? string.Empty;
            var institution = RhbServiceSupport.Normalize(request.ExternalInstitutionName);
            var referralNumber = RhbServiceSupport.Normalize(request.ExternalReferralNumber);
            var practitioner = RhbServiceSupport.Normalize(request.RequestingPractitionerName);

            if (indication.Length > 2000) return (TooLong("indicationText", 2000), null);
            if (institution?.Length > 200) return (TooLong("externalInstitutionName", 200), null);
            if (referralNumber?.Length > 100) return (TooLong("externalReferralNumber", 100), null);
            if (practitioner?.Length > 150) return (TooLong("requestingPractitionerName", 150), null);

            var requestedAt = request.RequestedAt.HasValue ? RhbServiceSupport.ToUtc(request.RequestedAt.Value) : nowUtc;
            if (requestedAt > nowUtc + ClockSkewTolerance)
                return (R.Rule(RhbErrorCodes.RequestedAtInFuture, "Waktu permintaan tidak boleh melewati waktu sekarang.").WithFields("requestedAt"), null);

            Guid? encounterId = null;
            Guid? inpEpisodeId = null;
            Guid? letterId = null;

            if (request.SourceType == RehabRequestSource.ExternalReferral)
            {
                // Rujukan eksternal tidak bertaut ke kunjungan (CK_RhbRequest_Source); isian kunjungan,
                // perawatan, dan surat internal dari klien diabaikan.
            }
            else
            {
                if (!request.SourceEncounterId.HasValue || request.SourceEncounterId.Value == Guid.Empty)
                    return (EncounterInvalid("Pilih kunjungan pasien yang menjadi asal permintaan."), null);

                var encounter = await _dbContext.Set<RegPatientEncounter>().AsNoTracking()
                    .Where(x => x.Id == request.SourceEncounterId.Value && !x.IsDelete)
                    .Select(x => new { x.PatientId, x.EncounterType })
                    .FirstOrDefaultAsync(cancellationToken);

                // Kunjungan milik pasien lain dijawab sama dengan kunjungan yang tidak ada (anti-IDOR).
                if (encounter == null || encounter.PatientId != request.PatientId)
                    return (EncounterInvalid("Pilih kunjungan pasien yang menjadi asal permintaan."), null);

                if (!EncounterMatchesSource(request.SourceType, encounter.EncounterType))
                    return (R.Rule(RhbErrorCodes.SourceEncounterInvalid, "Jenis kunjungan tidak sesuai dengan sumber permintaan.")
                        .WithFields("sourceType", "sourceEncounterId"), null);

                encounterId = request.SourceEncounterId.Value;

                if (request.SourceType == RehabRequestSource.Inpatient)
                {
                    inpEpisodeId = await _dbContext.Set<InpEpisode>().AsNoTracking()
                        .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                        .OrderByDescending(x => x.CreateDateTime)
                        .Select(x => (Guid?)x.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (!inpEpisodeId.HasValue)
                        return (R.Rule(RhbErrorCodes.SourceEncounterInvalid, "Kunjungan rawat inap ini belum memiliki data perawatan.")
                            .WithFields("sourceEncounterId"), null);

                    if (request.InpEpisodeId.HasValue && request.InpEpisodeId.Value != inpEpisodeId.Value)
                        return (R.Rule(RhbErrorCodes.SourceEncounterInvalid, "Data perawatan tidak sesuai dengan kunjungan.")
                            .WithFields("inpEpisodeId"), null);
                }

                if (request.InternalReferralLetterId.HasValue && request.InternalReferralLetterId.Value != Guid.Empty)
                {
                    var letterValid = await _dbContext.Set<CliDoctorCertificate>().AsNoTracking().AnyAsync(x =>
                        x.Id == request.InternalReferralLetterId.Value &&
                        !x.IsDelete &&
                        x.PatientId == request.PatientId &&
                        x.CertificateType == DoctorCertificateType.InpatientReferral &&
                        x.CertificateStatus == DoctorCertificateStatus.Issued,
                        cancellationToken);

                    if (!letterValid)
                        return (R.Rule(RhbErrorCodes.SourceEncounterInvalid, "Surat rujukan internal tidak ditemukan untuk pasien ini.")
                            .WithFields("internalReferralLetterId"), null);

                    letterId = request.InternalReferralLetterId.Value;
                }
            }

            if (request.RequestingDoctorId.HasValue && !await IsActiveDoctorAsync(request.RequestingDoctorId.Value, cancellationToken))
                return (R.Rule(RhbErrorCodes.RequestingDoctorInvalid, "Dokter peminta tidak ditemukan atau tidak aktif.").WithFields("requestingDoctorId"), null);

            var disciplineIds = (request.DisciplineIds ?? new List<Guid>()).Where(x => x != Guid.Empty).Distinct().ToList();
            if (disciplineIds.Count > 0)
            {
                var activeCount = await _dbContext.RhbDisciplines.AsNoTracking()
                    .CountAsync(x => disciplineIds.Contains(x.Id) && !x.IsDelete && x.IsActive && x.ProfessionId != null, cancellationToken);
                if (activeCount != disciplineIds.Count)
                    return (R.Rule(RhbErrorCodes.DisciplineRequired, "Pilih minimal satu layanan rehab yang tersedia.").WithFields("disciplineIds"), null);
            }

            return (null, new DraftValues(
                request.PatientId,
                request.SourceType,
                encounterId,
                inpEpisodeId,
                letterId,
                request.SourceType == RehabRequestSource.ExternalReferral ? institution : null,
                request.SourceType == RehabRequestSource.ExternalReferral ? referralNumber : null,
                request.RequestingDoctorId,
                request.SourceType == RehabRequestSource.ExternalReferral ? practitioner : null,
                indication,
                requestedAt,
                disciplineIds));
        }

        /// <summary>Kelengkapan saat <c>submit</c> — V-REQ-01..06.</summary>
        private async Task<R?> ValidateForSubmitAsync(RhbRequest entity, DateTime nowUtc, CancellationToken cancellationToken)
        {
            if (!await _dbContext.Set<MstPatient>().AsNoTracking().AnyAsync(x => x.Id == entity.PatientId && !x.IsDelete && x.IsActive, cancellationToken))
                return R.Rule(RhbErrorCodes.PatientRequired, "Pilih pasien yang terdaftar.").WithFields("patientId");

            if (entity.SourceType == RehabRequestSource.ExternalReferral)
            {
                if (string.IsNullOrWhiteSpace(entity.ExternalInstitutionName) || string.IsNullOrWhiteSpace(entity.RequestingPractitionerName))
                    return R.Rule(RhbErrorCodes.ExternalReferralIncomplete, "Lengkapi nama fasilitas dan perujuk.")
                        .WithFields("externalInstitutionName", "requestingPractitionerName");
            }
            else
            {
                var check = entity.SourceEncounterId.HasValue
                    ? await RhbServiceSupport.CheckEncounterAsync(_dbContext, entity.SourceEncounterId.Value, entity.PatientId, cancellationToken)
                    : null;
                if (check == null || !check.IsValid)
                    return EncounterInvalid("Pilih kunjungan pasien yang menjadi asal permintaan. Kunjungan yang dipilih sudah berakhir atau tidak sesuai.");

                if (!entity.RequestingDoctorId.HasValue || !await IsActiveDoctorAsync(entity.RequestingDoctorId.Value, cancellationToken))
                    return R.Rule(RhbErrorCodes.RequestingDoctorInvalid, "Pilih dokter peminta.").WithFields("requestingDoctorId");
            }

            if ((entity.IndicationText?.Trim().Length ?? 0) < IndicationMinLength)
                return R.Rule(RhbErrorCodes.IndicationRequired, "Tuliskan indikasi atau alasan rujukan.").WithFields("indicationText");

            var disciplineIds = await ActiveDisciplineIdsAsync(entity.Id, cancellationToken);
            var requested = await _dbContext.RhbRequestDisciplines.AsNoTracking()
                .CountAsync(x => x.RequestId == entity.Id && !x.IsDelete, cancellationToken);
            if (disciplineIds.Count == 0 || disciplineIds.Count != requested)
                return R.Rule(RhbErrorCodes.DisciplineRequired, "Pilih minimal satu layanan rehab yang tersedia.").WithFields("disciplineIds");

            if (entity.RequestedAt > nowUtc + ClockSkewTolerance)
                return R.Rule(RhbErrorCodes.RequestedAtInFuture, "Waktu permintaan tidak boleh melewati waktu sekarang.").WithFields("requestedAt");

            return null;
        }

        /// <summary>Layanan permintaan yang masih aktif dan berprofesi.</summary>
        private Task<List<Guid>> ActiveDisciplineIdsAsync(Guid requestId, CancellationToken cancellationToken) =>
            _dbContext.RhbRequestDisciplines.AsNoTracking()
                .Where(x => x.RequestId == requestId && !x.IsDelete &&
                            x.Discipline != null && !x.Discipline.IsDelete && x.Discipline.IsActive && x.Discipline.ProfessionId != null)
                .Select(x => x.DisciplineId)
                .ToListAsync(cancellationToken);

        /// <summary>
        /// Permintaan <c>Submitted</c>/<c>NeedsClarification</c> dan episode yang belum
        /// <c>Closed</c> milik pasien yang sama dengan layanan beririsan (V-REQ-07). Layanan episode
        /// dibaca dari permintaan asalnya dan dari resep asesmen medis.
        /// </summary>
        private async Task<List<RehabDuplicateCandidate>> FindDuplicatesAsync(
            Guid patientId,
            IReadOnlyCollection<Guid> disciplineIds,
            Guid? excludeRequestId,
            CancellationToken cancellationToken)
        {
            var ids = disciplineIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0)
                return new List<RehabDuplicateCandidate>();

            var requests = await _dbContext.RhbRequests.AsNoTracking()
                .Where(x => x.PatientId == patientId && !x.IsDelete &&
                            (!excludeRequestId.HasValue || x.Id != excludeRequestId.Value) &&
                            RunningStatuses.Contains(x.RequestStatus) &&
                            x.Disciplines.Any(d => !d.IsDelete && ids.Contains(d.DisciplineId)))
                .Select(x => new
                {
                    x.Id,
                    x.RequestNumber,
                    x.RequestStatus,
                    x.RequestedAt,
                    Names = x.Disciplines.Where(d => !d.IsDelete).Select(d => d.Discipline!.DisciplineName).ToList()
                })
                .ToListAsync(cancellationToken);

            var episodes = await _dbContext.RhbEpisodes.AsNoTracking()
                .Where(x => x.PatientId == patientId && !x.IsDelete &&
                            x.EpisodeStatus != RehabEpisodeStatus.Closed &&
                            (x.Disciplines.Any(d => !d.IsDelete && d.PrescribedByAssessment!.RecordStatus == RehabRecordStatus.Final && ids.Contains(d.DisciplineId)) ||
                             x.Request!.Disciplines.Any(d => !d.IsDelete && ids.Contains(d.DisciplineId))))
                .Select(x => new
                {
                    x.Id,
                    x.EpisodeNumber,
                    x.EpisodeStatus,
                    x.OpenedAt,
                    PrescribedNames = x.Disciplines.Where(d => !d.IsDelete && d.PrescribedByAssessment!.RecordStatus == RehabRecordStatus.Final).Select(d => d.Discipline!.DisciplineName).ToList(),
                    RequestedNames = x.Request!.Disciplines.Where(d => !d.IsDelete).Select(d => d.Discipline!.DisciplineName).ToList()
                })
                .ToListAsync(cancellationToken);

            return requests
                .Select(x => new RehabDuplicateCandidate
                {
                    CandidateType = RhbStatusHistoryWriter.Request,
                    Id = x.Id,
                    Number = x.RequestNumber,
                    Status = (int)x.RequestStatus,
                    StatusName = StatusName(x.RequestStatus),
                    StartedAt = x.RequestedAt,
                    DisciplineNames = x.Names.Distinct().OrderBy(n => n).ToList()
                })
                .Concat(episodes.Select(x => new RehabDuplicateCandidate
                {
                    CandidateType = RhbStatusHistoryWriter.Episode,
                    Id = x.Id,
                    Number = x.EpisodeNumber,
                    Status = (int)x.EpisodeStatus,
                    StatusName = EpisodeStatusName(x.EpisodeStatus),
                    StartedAt = x.OpenedAt,
                    DisciplineNames = x.PrescribedNames.Concat(x.RequestedNames).Distinct().OrderBy(n => n).ToList()
                }))
                .OrderByDescending(x => x.StartedAt)
                .ToList();
        }

        private (IQueryable<RhbRequest> Rows, string? RangeError) Filter(RehabRequestQuery query, bool includeStatus)
        {
            var (start, end, rangeError) = RhbServiceSupport.ResolveRange(query.StartDate, query.EndDate, query.CustomPeriod);
            var rows = _dbContext.RhbRequests.AsNoTracking().Where(x => !x.IsDelete);
            if (rangeError != null)
                return (rows, rangeError);

            if (start.HasValue)
                rows = rows.Where(x => x.RequestedAt >= start.Value);
            if (end.HasValue)
                rows = rows.Where(x => x.RequestedAt < end.Value);
            if (includeStatus && query.Status.HasValue)
                rows = rows.Where(x => x.RequestStatus == query.Status.Value);
            if (query.SourceType.HasValue)
                rows = rows.Where(x => x.SourceType == query.SourceType.Value);
            if (query.DisciplineId.HasValue)
                rows = rows.Where(x => x.Disciplines.Any(d => !d.IsDelete && d.DisciplineId == query.DisciplineId.Value));

            var keyword = RhbServiceSupport.Normalize(query.Search)?.ToLower();
            if (keyword != null)
            {
                rows = rows.Where(x =>
                    x.RequestNumber.ToLower().Contains(keyword) ||
                    _dbContext.Set<MstPatient>().Any(p => p.Id == x.PatientId &&
                        (p.FullName.ToLower().Contains(keyword) || p.MedicalRecordNumber.ToLower().Contains(keyword))));
            }

            return (rows, null);
        }

        private IQueryable<RehabRequestListItem> ProjectList(IQueryable<RhbRequest> rows) =>
            rows.Select(x => new RehabRequestListItem
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                PatientId = x.PatientId,
                PatientName = _dbContext.Set<MstPatient>().Where(p => p.Id == x.PatientId).Select(p => p.FullName).FirstOrDefault(),
                MedicalRecordNumber = _dbContext.Set<MstPatient>().Where(p => p.Id == x.PatientId).Select(p => p.MedicalRecordNumber).FirstOrDefault(),
                SourceType = x.SourceType,
                DisciplineNames = x.Disciplines.Where(d => !d.IsDelete).OrderBy(d => d.Discipline!.DisciplineName).Select(d => d.Discipline!.DisciplineName).ToList(),
                RequestedAt = x.RequestedAt,
                RequestingDoctorName = _dbContext.Set<MstDoctor>().Where(d => d.Id == x.RequestingDoctorId).Select(d => d.FullName).FirstOrDefault(),
                RequestStatus = x.RequestStatus,
                Version = x.Version
            });

        private static void LabelList(RehabRequestListItem item)
        {
            item.SourceTypeName = SourceName(item.SourceType);
            item.RequestStatusName = StatusName(item.RequestStatus);
        }

        private static void Apply(RhbRequest entity, DraftValues values)
        {
            entity.PatientId = values.PatientId;
            entity.SourceType = values.SourceType;
            entity.SourceEncounterId = values.SourceEncounterId;
            entity.InpEpisodeId = values.InpEpisodeId;
            entity.InternalReferralLetterId = values.InternalReferralLetterId;
            entity.ExternalInstitutionName = values.ExternalInstitutionName;
            entity.ExternalReferralNumber = values.ExternalReferralNumber;
            entity.RequestingDoctorId = values.RequestingDoctorId;
            entity.RequestingPractitionerName = values.RequestingPractitionerName;
            entity.IndicationText = values.IndicationText;
            entity.RequestedAt = values.RequestedAt;
        }

        /// <summary>Rawat jalan menerima kunjungan rawat jalan, MCU, dan telemedisin.</summary>
        private static bool EncounterMatchesSource(RehabRequestSource source, EncounterType type) => source switch
        {
            RehabRequestSource.Outpatient => type is EncounterType.Outpatient or EncounterType.MedicalCheckup or EncounterType.Telemedicine,
            RehabRequestSource.Inpatient => type == EncounterType.Inpatient,
            RehabRequestSource.Emergency => type == EncounterType.Emergency,
            _ => false
        };

        private static R? Guard(RhbRequest entity, int expectedVersion, params RehabRequestStatus[] allowed)
        {
            if (entity.Version != expectedVersion)
                return R.Conflict(RhbErrorCodes.StaleVersion, "Data sudah diubah pengguna lain. Muat ulang sebelum menyimpan.").WithFields("expectedVersion");

            if (!allowed.Contains(entity.RequestStatus))
                return R.Conflict(RhbErrorCodes.InvalidState,
                    $"Tindakan tidak dapat dilakukan pada status saat ini ({StatusName(entity.RequestStatus)}).");

            return null;
        }

        private static void Touch(RhbRequest entity, Guid actorUserId, DateTime nowUtc)
        {
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = nowUtc;
            entity.Version++;
        }

        private Task<bool> IsActiveDoctorAsync(Guid doctorId, CancellationToken cancellationToken) =>
            _dbContext.Set<MstDoctor>().AsNoTracking().AnyAsync(x => x.Id == doctorId && !x.IsDelete && x.IsActive, cancellationToken);

        private Task<RhbRequest?> FindAsync(Guid id, CancellationToken cancellationToken) =>
            _dbContext.RhbRequests.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

        private async Task<R> SaveAndReturnAsync(RhbRequest entity, CancellationToken cancellationToken)
        {
            var failure = await TrySaveAsync(cancellationToken);
            if (failure != null)
                return failure;

            return R.Ok((await GetDetailAsync(entity.Id, cancellationToken))!);
        }

        /// <summary>
        /// Menyimpan; bentrok versi atau indeks unik (misalnya penerimaan kedua untuk permintaan yang
        /// sama) dijawab 409 tanpa meninggalkan perubahan setengah jadi di change tracker.
        /// </summary>
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
                return R.Conflict(RhbErrorCodes.StaleVersion, "Data sudah diubah pengguna lain. Muat ulang sebelum menyimpan.");
            }
            catch (DbUpdateException exception) when (RhbServiceSupport.IsUniqueViolation(exception))
            {
                DetachPending();
                return R.Conflict(RhbErrorCodes.InvalidState, "Permintaan ini sudah diproses pengguna lain. Muat ulang data.");
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

        private static R NotFound() => R.NotFound("Permintaan rehab tidak ditemukan.");

        private static R ReasonRequired() => R.Rule(RhbErrorCodes.ReasonRequired, "Alasan wajib diisi.").WithFields("reason");

        private static R EncounterInvalid(string message) =>
            R.Rule(RhbErrorCodes.SourceEncounterInvalid, message).WithFields("sourceEncounterId");

        private static R TooLong(string field, int max) =>
            R.Invalid(RhbErrorCodes.FieldTooLong, $"Isian terlalu panjang (maksimal {max} karakter).").WithFields(field);
    }
}
