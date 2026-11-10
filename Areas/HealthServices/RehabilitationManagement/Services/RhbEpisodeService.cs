using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;
using P = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants.RehabilitationPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services
{
    /// <summary>
    /// Episode rehab (<c>RhbEpisode</c>): pembukaan (<c>BE-RHB-007</c>) serta worklist, rincian,
    /// timeline, dan riwayat (<c>BE-RHB-008</c>, <c>contracts/api-contract.md</c> v1 grup Rehab
    /// Episode).
    /// </summary>
    /// <remarks>
    /// Bagian baca tidak membuka transaksi. Daftar tidak memuat kolom bertanda S; timeline hanya
    /// memuat jenis, waktu, status, dan penulis dokumen, tidak pernah isi klinisnya.
    /// </remarks>
    public class RhbEpisodeService
    {
        private const string MedicalAssessment = "MedicalAssessment";
        private const string DisciplineAssessment = "DisciplineAssessment";
        private const string ProgramItem = "Program";
        private const string EducationItem = "Education";
        private const string SessionItem = "Session";
        private const string ProgressReviewItem = "ProgressReview";
        private const string DischargeSummaryItem = "DischargeSummary";

        /// <summary>Urutan tampil jenis dokumen bila waktunya sama — asesmen → program → sesi → reasesmen → ringkasan.</summary>
        private static readonly string[] TimelineOrder =
            [MedicalAssessment, DisciplineAssessment, ProgramItem, EducationItem, SessionItem, ProgressReviewItem, DischargeSummaryItem];

        private static readonly RehabEpisodeStatus[] OpenStatuses =
            [RehabEpisodeStatus.AwaitingMedicalAssessment, RehabEpisodeStatus.Planning, RehabEpisodeStatus.Active];

        private static readonly RehabEpisodeStatus[] PlanningOrActive =
            [RehabEpisodeStatus.Planning, RehabEpisodeStatus.Active];

        /// <summary>
        /// Aksi episode menurut <c>state-transition-matrix.md</c> §2–5: dokumen hanya dibuat selama
        /// episode belum <c>Closed</c>; asesmen disiplin, tujuan, dan program menunggu resep
        /// (<c>Planning</c>/<c>Active</c>); sesi menunggu program aktif (<c>Active</c>).
        /// </summary>
        private static readonly (string Action, string Label, string Resource, string Permission, RehabEpisodeStatus[] Statuses)[] ActionRules =
        [
            ("create-medical-assessment", "Asesmen medis", P.MedicalAssessment.Resource, P.Create, OpenStatuses),
            ("create-discipline-assessment", "Asesmen profesi", P.DisciplineAssessment.Resource, P.Create, PlanningOrActive),
            ("create-goal", "Tujuan terapi", P.Goal.Resource, P.Create, PlanningOrActive),
            ("create-program", "Program terapi", P.TherapyProgram.Resource, P.Create, PlanningOrActive),
            ("create-education", "Edukasi dan persetujuan", P.Education.Resource, P.Create, OpenStatuses),
            ("start-session", "Sesi terapi", P.Session.Resource, P.Create, [RehabEpisodeStatus.Active]),
            ("create-progress-review", "Reasesmen", P.ProgressReview.Resource, P.Create, PlanningOrActive),
            ("edit-discharge-summary", "Ringkasan akhir", P.DischargeSummary.Resource, P.Update, OpenStatuses)
        ];

        private readonly ApplicationDbContext _dbContext;
        private readonly AccessPermissionService _accessPermissionService;

        public RhbEpisodeService(ApplicationDbContext dbContext, AccessPermissionService accessPermissionService)
        {
            _dbContext = dbContext;
            _accessPermissionService = accessPermissionService;
        }

        // =====================================================================
        // Pembukaan (BE-RHB-007)
        // =====================================================================

        /// <summary>
        /// Membuka episode untuk permintaan yang diterima (<c>state-transition-matrix.md</c> §1–2):
        /// status <c>AwaitingMedicalAssessment</c>, pasien disalin dari permintaan.
        /// </summary>
        /// <remarks>
        /// <b>Tidak menyimpan.</b> Episode dan riwayatnya hanya ditambahkan ke change tracker supaya
        /// tersimpan dalam satu <c>SaveChanges</c> bersama perubahan status permintaan — penerimaan
        /// yang gagal tidak meninggalkan episode tanpa permintaan <c>Accepted</c>, dan sebaliknya.
        /// Indeks unik <c>RequestId</c> menolak episode kedua untuk permintaan yang sama.
        /// <para>
        /// Disiplin episode <b>tidak</b> disalin dari permintaan: layanan yang dijalankan ditetapkan
        /// resep asesmen medis (<c>RhbEpisodeDiscipline.PrescribedByAssessmentId</c>, RHB-DEC-005).
        /// </para>
        /// </remarks>
        public RhbEpisode OpenForAcceptedRequest(
            RhbRequest request,
            Guid responsibleDoctorId,
            string episodeNumber,
            Guid actorUserId,
            DateTime nowUtc)
        {
            var episode = new RhbEpisode
            {
                EpisodeNumber = episodeNumber,
                RequestId = request.Id,
                PatientId = request.PatientId,
                ResponsibleDoctorId = responsibleDoctorId,
                EpisodeStatus = RehabEpisodeStatus.AwaitingMedicalAssessment,
                OpenedAt = nowUtc,
                CreateDateTime = nowUtc,
                CreateBy = actorUserId
            };

            _dbContext.RhbEpisodes.Add(episode);

            RhbStatusHistoryWriter.Record(
                _dbContext,
                RhbStatusHistoryWriter.Episode,
                episode.Id,
                null,
                episode.EpisodeStatus.ToString(),
                "open",
                null,
                actorUserId,
                nowUtc);

            return episode;
        }

        // =====================================================================
        // Label
        // =====================================================================

        public static string StatusName(RehabEpisodeStatus status) => RhbRequestService.EpisodeStatusName(status);

        public static string RecordStatusName(RehabRecordStatus status) => status switch
        {
            RehabRecordStatus.Draft => "Draf",
            RehabRecordStatus.Final => "Final",
            RehabRecordStatus.Cancelled => "Dibatalkan",
            _ => status.ToString()
        };

        private static string ProgramStatusName(RehabProgramStatus status) => status switch
        {
            RehabProgramStatus.Draft => "Draf",
            RehabProgramStatus.Active => "Aktif",
            RehabProgramStatus.Superseded => "Digantikan versi baru",
            RehabProgramStatus.Stopped => "Dihentikan",
            RehabProgramStatus.Completed => "Selesai",
            RehabProgramStatus.Cancelled => "Dibatalkan",
            _ => status.ToString()
        };

        private static string SessionStatusName(RehabSessionStatus status) => status switch
        {
            RehabSessionStatus.InProgress => "Berlangsung",
            RehabSessionStatus.Completed => "Selesai",
            RehabSessionStatus.StoppedEarly => "Dihentikan lebih awal",
            RehabSessionStatus.NotDone => "Tidak terlaksana",
            _ => status.ToString()
        };

        private static string ItemTypeName(string itemType) => itemType switch
        {
            MedicalAssessment => "Asesmen medis",
            DisciplineAssessment => "Asesmen profesi",
            ProgramItem => "Program terapi",
            EducationItem => "Edukasi dan persetujuan",
            SessionItem => "Sesi terapi",
            ProgressReviewItem => "Reasesmen",
            DischargeSummaryItem => "Ringkasan akhir",
            _ => itemType
        };

        // =====================================================================
        // Baca (BE-RHB-008)
        // =====================================================================

        public async Task<RehabEpisodeFilterMetadataResponse> BuildFilterMetadataAsync(CancellationToken cancellationToken) => new()
        {
            CustomPeriods =
            [
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thisMonth", Label = "Bulan ini" },
                new() { Value = "thisYear", Label = "Tahun ini" },
                new() { Value = "custom", Label = "Pilih tanggal", UsesStartDate = true, UsesEndDate = true }
            ],
            StatusOptions = Enum.GetValues<RehabEpisodeStatus>()
                .Select(x => new RehabOptionResponse { Value = ((int)x).ToString(), Label = StatusName(x) })
                .ToList(),
            DisciplineOptions = await _dbContext.RhbDisciplines.AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.DisciplineName)
                .Select(x => new RehabOptionResponse { Value = x.Id.ToString(), Label = x.DisciplineName })
                .ToListAsync(cancellationToken),
            ResponsibleDoctorOptions = await _dbContext.Set<MstDoctor>().AsNoTracking()
                .Where(d => _dbContext.RhbEpisodes.Any(e => !e.IsDelete && e.ResponsibleDoctorId == d.Id))
                .OrderBy(d => d.FullName)
                .Select(d => new RehabOptionResponse { Value = d.Id.ToString(), Label = d.FullName })
                .ToListAsync(cancellationToken),
            SortOptions =
            [
                new() { Value = "openedAt", Label = "Tanggal dibuka" },
                new() { Value = "episodeNumber", Label = "Nomor episode" },
                new() { Value = "episodeStatus", Label = "Status" }
            ],
            SortDirections = ["asc", "desc"],
            PageSizeOptions = [10, 25, 50, 100],
            QueryParameters =
            [
                new() { Name = "startDate", Type = "date", Description = "Awal rentang tanggal episode dibuka", Example = "2026-11-01" },
                new() { Name = "endDate", Type = "date", Description = "Akhir rentang tanggal episode dibuka", Example = "2026-11-30" },
                new() { Name = "customPeriod", Type = "string", Description = "today, last7days, thisMonth, thisYear, custom", Example = "thisMonth" },
                new() { Name = "status", Type = "integer", Description = "1 Menunggu asesmen medis, 2 Perencanaan, 3 Terapi berjalan, 4 Ditutup", Example = "3" },
                new() { Name = "disciplineId", Type = "uuid", Description = "Layanan yang diminta atau diresepkan" },
                new() { Name = "responsibleDoctorId", Type = "uuid", Description = "Dokter penanggung jawab" },
                new() { Name = "needsReview", Type = "boolean", Description = "Program aktif perlu ditinjau", Example = "true" },
                new() { Name = "evaluationDue", Type = "boolean", Description = "Tanggal evaluasi program aktif hari ini atau sudah lewat", Example = "true" },
                new() { Name = "search", Type = "string", Description = "Nomor episode/permintaan, nama pasien, atau nomor rekam medis", Example = "RHB-EPS" },
                new() { Name = "sortBy", Type = "string", Description = "openedAt, episodeNumber, episodeStatus", Example = "openedAt" },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc", Example = "desc" },
                new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman, mulai 1", Example = "1" },
                new() { Name = "pageSize", Type = "integer", Description = "Jumlah baris per halaman, maksimum 100", Example = "25" }
            ]
        };

        public async Task<RhbResult<RehabEpisodeSummaryResponse>> GetSummaryAsync(RehabEpisodeQuery query, CancellationToken cancellationToken)
        {
            var (rows, rangeError) = Filter(query, includeStatus: false);
            if (rangeError != null)
                return RhbResult<RehabEpisodeSummaryResponse>.Invalid(RhbErrorCodes.PageRangeInvalid, rangeError);

            var counts = await rows
                .GroupBy(x => x.EpisodeStatus)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(cancellationToken);

            int Count(RehabEpisodeStatus status) => counts.Where(x => x.Status == status).Sum(x => x.Count);
            var today = RhbServiceSupport.TodayLocal();

            return RhbResult<RehabEpisodeSummaryResponse>.Ok(new RehabEpisodeSummaryResponse
            {
                Total = counts.Sum(x => x.Count),
                AwaitingMedicalAssessment = Count(RehabEpisodeStatus.AwaitingMedicalAssessment),
                Planning = Count(RehabEpisodeStatus.Planning),
                Active = Count(RehabEpisodeStatus.Active),
                Closed = Count(RehabEpisodeStatus.Closed),
                NeedsReview = await rows.CountAsync(x => x.Programs.Any(p => !p.IsDelete && p.ProgramStatus == RehabProgramStatus.Active && p.NeedsReview), cancellationToken),
                EvaluationDue = await rows.CountAsync(x => x.Programs.Any(p => !p.IsDelete && p.ProgramStatus == RehabProgramStatus.Active && p.EvaluationDueDate != null && p.EvaluationDueDate <= today), cancellationToken)
            });
        }

        public async Task<RhbResult<PagedResult<RehabEpisodeListItem>>> GetListAsync(RehabEpisodeQuery query, CancellationToken cancellationToken)
        {
            var (rows, rangeError) = Filter(query, includeStatus: true);
            if (rangeError != null)
                return RhbResult<PagedResult<RehabEpisodeListItem>>.Invalid(RhbErrorCodes.PageRangeInvalid, rangeError);

            var (pageNumber, pageSize) = RhbServiceSupport.NormalizePaging(query.PageNumber, query.PageSize);
            var descending = !string.Equals(query.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            rows = (query.SortBy?.Trim().ToLowerInvariant()) switch
            {
                "episodenumber" => descending ? rows.OrderByDescending(x => x.EpisodeNumber) : rows.OrderBy(x => x.EpisodeNumber),
                "episodestatus" => descending ? rows.OrderByDescending(x => x.EpisodeStatus) : rows.OrderBy(x => x.EpisodeStatus),
                _ => descending ? rows.OrderByDescending(x => x.OpenedAt) : rows.OrderBy(x => x.OpenedAt)
            };

            var total = await rows.CountAsync(cancellationToken);
            var items = await ProjectList(rows.Skip((pageNumber - 1) * pageSize).Take(pageSize)).ToListAsync(cancellationToken);
            foreach (var item in items)
                item.EpisodeStatusName = StatusName(item.EpisodeStatus);

            return RhbResult<PagedResult<RehabEpisodeListItem>>.Ok(new PagedResult<RehabEpisodeListItem>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = pageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            });
        }

        public async Task<RehabEpisodeDetailResponse?> GetDetailAsync(Guid id, ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var row = await ProjectList(_dbContext.RhbEpisodes.AsNoTracking().Where(x => x.Id == id && !x.IsDelete))
                .FirstOrDefaultAsync(cancellationToken);
            if (row == null)
                return null;

            var episode = await _dbContext.RhbEpisodes.AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new { x.ClosureReason, x.ClosedByUserId })
                .FirstAsync(cancellationToken);

            var detail = new RehabEpisodeDetailResponse
            {
                Id = row.Id,
                EpisodeNumber = row.EpisodeNumber,
                PatientId = row.PatientId,
                PatientName = row.PatientName,
                MedicalRecordNumber = row.MedicalRecordNumber,
                RequestId = row.RequestId,
                RequestNumber = row.RequestNumber,
                ResponsibleDoctorId = row.ResponsibleDoctorId,
                ResponsibleDoctorName = row.ResponsibleDoctorName,
                DisciplineNames = row.DisciplineNames,
                EpisodeStatus = row.EpisodeStatus,
                EpisodeStatusName = StatusName(row.EpisodeStatus),
                OpenedAt = row.OpenedAt,
                ClosedAt = row.ClosedAt,
                NeedsReview = row.NeedsReview,
                NextEvaluationDueDate = row.NextEvaluationDueDate,
                Version = row.Version,
                ClosureReason = episode.ClosureReason,
                ClosedByUserId = episode.ClosedByUserId
            };

            detail.Patient = await _dbContext.Set<MstPatient>().AsNoTracking()
                .Where(p => p.Id == row.PatientId)
                .Select(p => new RehabEpisodePatientInfo
                {
                    Id = p.Id,
                    FullName = p.FullName,
                    MedicalRecordNumber = p.MedicalRecordNumber,
                    BirthDate = p.BirthDate,
                    Gender = p.Gender
                })
                .FirstOrDefaultAsync(cancellationToken) ?? new RehabEpisodePatientInfo { Id = row.PatientId };

            detail.Request = await _dbContext.RhbRequests.AsNoTracking()
                .Where(r => r.Id == row.RequestId)
                .Select(r => new RehabEpisodeRequestInfo
                {
                    Id = r.Id,
                    RequestNumber = r.RequestNumber,
                    SourceType = r.SourceType,
                    SourceEncounterId = r.SourceEncounterId,
                    SourceEncounterNumber = _dbContext.Set<RegPatientEncounter>().Where(e => e.Id == r.SourceEncounterId).Select(e => e.EncounterNumber).FirstOrDefault(),
                    RequestedAt = r.RequestedAt,
                    RequestingDoctorName = _dbContext.Set<MstDoctor>().Where(d => d.Id == r.RequestingDoctorId).Select(d => d.FullName).FirstOrDefault(),
                    RequestingPractitionerName = r.RequestingPractitionerName,
                    IndicationText = r.IndicationText
                })
                .FirstAsync(cancellationToken);
            detail.Request.SourceTypeName = RhbRequestService.SourceName(detail.Request.SourceType);

            detail.Disciplines = await LoadDisciplinesAsync(row.Id, row.RequestId, cancellationToken);
            detail.Documents = await LoadDocumentSummaryAsync(row.Id, cancellationToken);
            detail.AvailableActions = await ResolveActionsAsync(row.EpisodeStatus, detail.Documents, user);

            return detail;
        }

        /// <summary>
        /// Kejadian dokumen episode urut waktu. Jenis yang belum punya data (atau slice-nya belum
        /// dibangun) cukup tidak menyumbang baris.
        /// </summary>
        public async Task<RhbResult<List<RehabTimelineItem>>> GetTimelineAsync(Guid id, CancellationToken cancellationToken)
        {
            if (!await _dbContext.RhbEpisodes.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken))
                return RhbResult<List<RehabTimelineItem>>.NotFound("Episode rehab tidak ditemukan.");

            var items = new List<RehabTimelineItem>();

            var assessments = await _dbContext.RhbAssessments.AsNoTracking()
                .Where(x => x.EpisodeId == id && !x.IsDelete)
                .Select(x => new { x.Id, x.AssessmentType, x.DisciplineId, DisciplineName = x.Discipline != null ? x.Discipline.DisciplineName : null, x.AssessedAt, x.RecordStatus, x.AuthorUserId })
                .ToListAsync(cancellationToken);
            items.AddRange(assessments.Select(x => Item(
                x.AssessmentType == RehabAssessmentType.MedicalInitial ? MedicalAssessment : DisciplineAssessment,
                x.Id, null, x.DisciplineId, x.DisciplineName, x.AssessedAt, x.RecordStatus.ToString(), RecordStatusName(x.RecordStatus), x.AuthorUserId)));

            var programs = await _dbContext.RhbPrograms.AsNoTracking()
                .Where(x => x.EpisodeId == id && !x.IsDelete)
                .Select(x => new { x.Id, x.ProgramVersion, x.DisciplineId, DisciplineName = x.Discipline!.DisciplineName, OccurredAt = x.ActivatedAt ?? x.CreateDateTime, x.ProgramStatus, x.AuthorUserId })
                .ToListAsync(cancellationToken);
            items.AddRange(programs.Select(x => Item(
                ProgramItem, x.Id, $"v{x.ProgramVersion}", x.DisciplineId, x.DisciplineName, x.OccurredAt, x.ProgramStatus.ToString(), ProgramStatusName(x.ProgramStatus), x.AuthorUserId)));

            var educations = await _dbContext.RhbEducationRecords.AsNoTracking()
                .Where(x => x.EpisodeId == id && !x.IsDelete)
                .Select(x => new { x.Id, x.EducatedAt, x.RecordStatus, x.EducatorUserId })
                .ToListAsync(cancellationToken);
            items.AddRange(educations.Select(x => Item(
                EducationItem, x.Id, null, null, null, x.EducatedAt, x.RecordStatus.ToString(), RecordStatusName(x.RecordStatus), x.EducatorUserId)));

            var sessions = await _dbContext.RhbSessions.AsNoTracking()
                .Where(x => x.EpisodeId == id && !x.IsDelete)
                .Select(x => new { x.Id, x.SessionNumber, DisciplineId = (Guid?)x.Program!.DisciplineId, DisciplineName = x.Program!.Discipline!.DisciplineName, OccurredAt = x.StartedAt ?? x.CreateDateTime, x.SessionStatus, x.RecordStatus, x.TherapistUserId })
                .ToListAsync(cancellationToken);
            items.AddRange(sessions.Select(x => Item(
                SessionItem, x.Id, x.SessionNumber, x.DisciplineId, x.DisciplineName, x.OccurredAt,
                x.SessionStatus.ToString(), $"{SessionStatusName(x.SessionStatus)} · {RecordStatusName(x.RecordStatus)}", x.TherapistUserId)));

            var reviews = await _dbContext.RhbProgressReviews.AsNoTracking()
                .Where(x => x.EpisodeId == id && !x.IsDelete)
                .Select(x => new { x.Id, x.DisciplineId, DisciplineName = x.Discipline != null ? x.Discipline.DisciplineName : null, x.ReviewedAt, x.RecordStatus, x.AuthorUserId })
                .ToListAsync(cancellationToken);
            items.AddRange(reviews.Select(x => Item(
                ProgressReviewItem, x.Id, null, x.DisciplineId, x.DisciplineName, x.ReviewedAt, x.RecordStatus.ToString(), RecordStatusName(x.RecordStatus), x.AuthorUserId)));

            var summaries = await _dbContext.RhbDischargeSummaries.AsNoTracking()
                .Where(x => x.EpisodeId == id && !x.IsDelete)
                .Select(x => new { x.Id, OccurredAt = x.SignedAt ?? x.CreateDateTime, x.RecordStatus, x.AuthorUserId })
                .ToListAsync(cancellationToken);
            items.AddRange(summaries.Select(x => Item(
                DischargeSummaryItem, x.Id, null, null, null, x.OccurredAt, x.RecordStatus.ToString(), RecordStatusName(x.RecordStatus), x.AuthorUserId)));

            var authorIds = items.Where(x => x.AuthorUserId.HasValue).Select(x => x.AuthorUserId!.Value).Distinct().ToList();
            var authors = await _dbContext.Users.AsNoTracking()
                .Where(u => authorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.DisplayName, cancellationToken);
            foreach (var item in items)
                item.AuthorName = item.AuthorUserId.HasValue && authors.TryGetValue(item.AuthorUserId.Value, out var name) ? name : null;

            return RhbResult<List<RehabTimelineItem>>.Ok(items
                .OrderBy(x => x.OccurredAt)
                .ThenBy(x => Array.IndexOf(TimelineOrder, x.ItemType))
                .ToList());
        }

        public async Task<RhbResult<List<RehabStatusHistoryItem>>> GetStatusHistoryAsync(Guid id, CancellationToken cancellationToken)
        {
            if (!await _dbContext.RhbEpisodes.AsNoTracking().AnyAsync(x => x.Id == id && !x.IsDelete, cancellationToken))
                return RhbResult<List<RehabStatusHistoryItem>>.NotFound("Episode rehab tidak ditemukan.");

            return RhbResult<List<RehabStatusHistoryItem>>.Ok(
                await RhbStatusHistoryWriter.ReadAsync(_dbContext, RhbStatusHistoryWriter.Episode, id, cancellationToken));
        }

        // =====================================================================
        // Penolong
        // =====================================================================

        private (IQueryable<RhbEpisode> Rows, string? RangeError) Filter(RehabEpisodeQuery query, bool includeStatus)
        {
            var (start, end, rangeError) = RhbServiceSupport.ResolveRange(query.StartDate, query.EndDate, query.CustomPeriod);
            var rows = _dbContext.RhbEpisodes.AsNoTracking().Where(x => !x.IsDelete);
            if (rangeError != null)
                return (rows, rangeError);

            if (start.HasValue)
                rows = rows.Where(x => x.OpenedAt >= start.Value);
            if (end.HasValue)
                rows = rows.Where(x => x.OpenedAt < end.Value);
            if (includeStatus && query.Status.HasValue)
                rows = rows.Where(x => x.EpisodeStatus == query.Status.Value);
            if (query.ResponsibleDoctorId.HasValue)
                rows = rows.Where(x => x.ResponsibleDoctorId == query.ResponsibleDoctorId.Value);
            if (query.DisciplineId.HasValue)
            {
                var disciplineId = query.DisciplineId.Value;
                rows = rows.Where(x =>
                    x.Disciplines.Any(d => !d.IsDelete && d.PrescribedByAssessment!.RecordStatus == RehabRecordStatus.Final && d.DisciplineId == disciplineId) ||
                    x.Request!.Disciplines.Any(d => !d.IsDelete && d.DisciplineId == disciplineId));
            }

            if (query.NeedsReview.HasValue)
            {
                rows = query.NeedsReview.Value
                    ? rows.Where(x => x.Programs.Any(p => !p.IsDelete && p.ProgramStatus == RehabProgramStatus.Active && p.NeedsReview))
                    : rows.Where(x => !x.Programs.Any(p => !p.IsDelete && p.ProgramStatus == RehabProgramStatus.Active && p.NeedsReview));
            }

            if (query.EvaluationDue.HasValue)
            {
                var today = RhbServiceSupport.TodayLocal();
                rows = query.EvaluationDue.Value
                    ? rows.Where(x => x.Programs.Any(p => !p.IsDelete && p.ProgramStatus == RehabProgramStatus.Active && p.EvaluationDueDate != null && p.EvaluationDueDate <= today))
                    : rows.Where(x => !x.Programs.Any(p => !p.IsDelete && p.ProgramStatus == RehabProgramStatus.Active && p.EvaluationDueDate != null && p.EvaluationDueDate <= today));
            }

            var keyword = RhbServiceSupport.Normalize(query.Search)?.ToLower();
            if (keyword != null)
            {
                rows = rows.Where(x =>
                    x.EpisodeNumber.ToLower().Contains(keyword) ||
                    x.Request!.RequestNumber.ToLower().Contains(keyword) ||
                    _dbContext.Set<MstPatient>().Any(p => p.Id == x.PatientId &&
                        (p.FullName.ToLower().Contains(keyword) || p.MedicalRecordNumber.ToLower().Contains(keyword))));
            }

            return (rows, null);
        }

        private IQueryable<RehabEpisodeListItem> ProjectList(IQueryable<RhbEpisode> rows) =>
            rows.Select(x => new RehabEpisodeListItem
            {
                Id = x.Id,
                EpisodeNumber = x.EpisodeNumber,
                PatientId = x.PatientId,
                PatientName = _dbContext.Set<MstPatient>().Where(p => p.Id == x.PatientId).Select(p => p.FullName).FirstOrDefault(),
                MedicalRecordNumber = _dbContext.Set<MstPatient>().Where(p => p.Id == x.PatientId).Select(p => p.MedicalRecordNumber).FirstOrDefault(),
                RequestId = x.RequestId,
                RequestNumber = x.Request!.RequestNumber,
                ResponsibleDoctorId = x.ResponsibleDoctorId,
                ResponsibleDoctorName = _dbContext.Set<MstDoctor>().Where(d => d.Id == x.ResponsibleDoctorId).Select(d => d.FullName).FirstOrDefault(),
                DisciplineNames = _dbContext.RhbDisciplines
                    .Where(d =>
                        x.Disciplines.Any(ed => !ed.IsDelete && ed.PrescribedByAssessment!.RecordStatus == RehabRecordStatus.Final && ed.DisciplineId == d.Id) ||
                        x.Request!.Disciplines.Any(rd => !rd.IsDelete && rd.DisciplineId == d.Id))
                    .OrderBy(d => d.DisciplineName)
                    .Select(d => d.DisciplineName)
                    .ToList(),
                EpisodeStatus = x.EpisodeStatus,
                OpenedAt = x.OpenedAt,
                ClosedAt = x.ClosedAt,
                NeedsReview = x.Programs.Any(p => !p.IsDelete && p.ProgramStatus == RehabProgramStatus.Active && p.NeedsReview),
                NextEvaluationDueDate = x.Programs
                    .Where(p => !p.IsDelete && p.ProgramStatus == RehabProgramStatus.Active && p.EvaluationDueDate != null)
                    .Min(p => p.EvaluationDueDate),
                Version = x.Version
            });

        private async Task<List<RehabEpisodeDisciplineInfo>> LoadDisciplinesAsync(Guid episodeId, Guid requestId, CancellationToken cancellationToken)
        {
            var requested = await _dbContext.RhbRequestDisciplines.AsNoTracking()
                .Where(x => x.RequestId == requestId && !x.IsDelete)
                .Select(x => x.DisciplineId)
                .ToListAsync(cancellationToken);

            // Resep dihitung hanya dari asesmen medis final; baris milik draf belum berlaku (BE-RHB-009).
            var prescribed = await _dbContext.RhbEpisodeDisciplines.AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete && x.PrescribedByAssessment!.RecordStatus == RehabRecordStatus.Final)
                .Select(x => new { x.DisciplineId, x.MedicalPrecautionText })
                .ToListAsync(cancellationToken);

            var activePrograms = await _dbContext.RhbPrograms.AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete && x.ProgramStatus == RehabProgramStatus.Active)
                .Select(x => new { x.DisciplineId, x.NeedsReview, x.EvaluationDueDate })
                .ToListAsync(cancellationToken);

            var ids = requested.Concat(prescribed.Select(x => x.DisciplineId)).Distinct().ToList();
            var disciplines = await _dbContext.RhbDisciplines.AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { x.Id, x.DisciplineCode, x.DisciplineName })
                .ToListAsync(cancellationToken);

            return disciplines
                .OrderBy(x => x.DisciplineName)
                .Select(x =>
                {
                    var prescription = prescribed.FirstOrDefault(p => p.DisciplineId == x.Id);
                    var program = activePrograms.FirstOrDefault(p => p.DisciplineId == x.Id);
                    return new RehabEpisodeDisciplineInfo
                    {
                        DisciplineId = x.Id,
                        DisciplineCode = x.DisciplineCode,
                        DisciplineName = x.DisciplineName,
                        IsRequested = requested.Contains(x.Id),
                        IsPrescribed = prescription != null,
                        MedicalPrecautionText = prescription?.MedicalPrecautionText,
                        ActiveProgramStatusName = program != null ? ProgramStatusName(RehabProgramStatus.Active) : null,
                        ActiveProgramNeedsReview = program?.NeedsReview ?? false,
                        ActiveProgramEvaluationDueDate = program?.EvaluationDueDate
                    };
                })
                .ToList();
        }

        private async Task<RehabEpisodeDocumentSummary> LoadDocumentSummaryAsync(Guid episodeId, CancellationToken cancellationToken)
        {
            var assessments = await _dbContext.RhbAssessments.AsNoTracking()
                .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                .GroupBy(x => new { x.AssessmentType, x.RecordStatus })
                .Select(g => new { g.Key.AssessmentType, g.Key.RecordStatus, Count = g.Count() })
                .ToListAsync(cancellationToken);

            int Assessment(RehabAssessmentType type, RehabRecordStatus status) =>
                assessments.Where(x => x.AssessmentType == type && x.RecordStatus == status).Sum(x => x.Count);

            return new RehabEpisodeDocumentSummary
            {
                MedicalAssessmentDraft = Assessment(RehabAssessmentType.MedicalInitial, RehabRecordStatus.Draft),
                MedicalAssessmentFinal = Assessment(RehabAssessmentType.MedicalInitial, RehabRecordStatus.Final),
                DisciplineAssessmentDraft = Assessment(RehabAssessmentType.DisciplineInitial, RehabRecordStatus.Draft),
                DisciplineAssessmentFinal = Assessment(RehabAssessmentType.DisciplineInitial, RehabRecordStatus.Final),
                ActiveGoal = await _dbContext.RhbGoals.AsNoTracking().CountAsync(x => x.EpisodeId == episodeId && !x.IsDelete && x.GoalStatus == RehabGoalStatus.Active, cancellationToken),
                ProgramDraft = await _dbContext.RhbPrograms.AsNoTracking().CountAsync(x => x.EpisodeId == episodeId && !x.IsDelete && x.ProgramStatus == RehabProgramStatus.Draft, cancellationToken),
                ProgramActive = await _dbContext.RhbPrograms.AsNoTracking().CountAsync(x => x.EpisodeId == episodeId && !x.IsDelete && x.ProgramStatus == RehabProgramStatus.Active, cancellationToken),
                EducationRecord = await _dbContext.RhbEducationRecords.AsNoTracking().CountAsync(x => x.EpisodeId == episodeId && !x.IsDelete && x.RecordStatus != RehabRecordStatus.Cancelled, cancellationToken),
                SessionInProgress = await _dbContext.RhbSessions.AsNoTracking().CountAsync(x => x.EpisodeId == episodeId && !x.IsDelete && x.SessionStatus == RehabSessionStatus.InProgress && x.RecordStatus == RehabRecordStatus.Draft, cancellationToken),
                SessionFinal = await _dbContext.RhbSessions.AsNoTracking().CountAsync(x => x.EpisodeId == episodeId && !x.IsDelete && x.RecordStatus == RehabRecordStatus.Final, cancellationToken),
                ProgressReviewFinal = await _dbContext.RhbProgressReviews.AsNoTracking().CountAsync(x => x.EpisodeId == episodeId && !x.IsDelete && x.RecordStatus == RehabRecordStatus.Final, cancellationToken),
                DischargeSummaryStatus = await _dbContext.RhbDischargeSummaries.AsNoTracking()
                    .Where(x => x.EpisodeId == episodeId && !x.IsDelete)
                    .Select(x => (RehabRecordStatus?)x.RecordStatus)
                    .FirstOrDefaultAsync(cancellationToken)
            };
        }

        /// <summary>
        /// Aksi = sah menurut status (<see cref="ActionRules"/>) ∩ butir hak akses pengguna
        /// (<see cref="AccessPermissionService.GetEffectivePermissionsAsync"/>, sumber yang sama
        /// dengan penjaga endpoint). Asesmen medis tidak ditawarkan lagi bila sudah ada draf atau
        /// final, dan ringkasan akhir tidak ditawarkan bila sudah final.
        /// </summary>
        private async Task<List<RehabEpisodeAction>> ResolveActionsAsync(
            RehabEpisodeStatus status,
            RehabEpisodeDocumentSummary documents,
            ClaimsPrincipal user)
        {
            var permissions = await _accessPermissionService.GetEffectivePermissionsAsync(user);

            bool Allowed(string resource, string action) =>
                permissions.IsSuperAdmin ||
                permissions.Permissions.Any(p => p.Resource == resource && p.Action == action);

            return ActionRules
                .Where(rule => rule.Statuses.Contains(status))
                .Where(rule => rule.Action != "create-medical-assessment" ||
                               (documents.MedicalAssessmentDraft == 0 && documents.MedicalAssessmentFinal == 0))
                .Where(rule => rule.Action != "edit-discharge-summary" || documents.DischargeSummaryStatus != RehabRecordStatus.Final)
                .Where(rule => Allowed(rule.Resource, rule.Permission))
                .Select(rule => new RehabEpisodeAction
                {
                    Action = rule.Action,
                    Label = rule.Label,
                    Resource = rule.Resource,
                    Permission = rule.Permission
                })
                .ToList();
        }

        private static RehabTimelineItem Item(
            string itemType,
            Guid id,
            string? reference,
            Guid? disciplineId,
            string? disciplineName,
            DateTime occurredAt,
            string status,
            string statusName,
            Guid? authorUserId) => new()
        {
            ItemType = itemType,
            ItemTypeName = ItemTypeName(itemType),
            Id = id,
            Reference = reference,
            DisciplineId = disciplineId,
            DisciplineName = disciplineName,
            OccurredAt = occurredAt,
            Status = status,
            StatusName = statusName,
            AuthorUserId = authorUserId
        };
    }
}
