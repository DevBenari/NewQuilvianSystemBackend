using System.Globalization;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QRCoder;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;
using QuilvianSystemBackend.Shared.HumanResource.Helpers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services
{
    /// <summary>Benturan status pada alur Surat Tugas; dipetakan controller ke <c>409 Conflict</c>.</summary>
    public sealed class HrServiceConflictException : InvalidOperationException
    {
        public HrServiceConflictException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Surat Tugas multi-pegawai berjenjang (BE-HRD-382/383/384): Draf → Diajukan → Verifikasi HRD (lulus / minta
    /// revisi / tolak) → Menunggu Pengesahan → Disahkan → Diterbitkan (nomor resmi, kop dari pengaturan, daftar
    /// peserta, QR) → Dilaksanakan → Selesai, dengan pembatalan berjustifikasi.
    /// </summary>
    /// <remarks>
    /// Kewenangan memakai hak akses Akses Role (<c>AssignmentLetter : Create/Update/Submit/Verify/Endorse/Issue/Report/Cancel</c>)
    /// dan penanda <c>AssignmentLetter : ManageAllUnits</c>; tidak ada nama peran tertanam. Pengaju, verifikator, dan
    /// pengesah wajib orang berbeda.
    /// </remarks>
    public class AssignmentLetterService
    {
        public const string Resource = "AssignmentLetter";
        private const string SequenceKey = "HRD_ASSIGNMENT_LETTER";
        private const string SeriesPrefix = "ST-RSMMC";
        private const int SequenceDigits = 6;
        private const int QrPixelsPerModule = 6;
        private static readonly CultureInfo IdCulture = new("id-ID");

        private static readonly string[] CancellableBeforeIssue =
        {
            AssignmentLetterStatuses.Draft, AssignmentLetterStatuses.Submitted, AssignmentLetterStatuses.RevisionNeeded,
            AssignmentLetterStatuses.PendingSignOff, AssignmentLetterStatuses.Endorsed
        };

        private static readonly string[] InactiveForConflict =
        {
            AssignmentLetterStatuses.Draft, AssignmentLetterStatuses.Rejected, AssignmentLetterStatuses.Cancelled, AssignmentLetterStatuses.Completed
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly AccessPermissionService _accessPermissionService;
        private readonly HrdUnitScopeService _unitScopeService;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly OfficialDutyRequestService _officialDutyRequestService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AssignmentLetterService> _logger;

        public AssignmentLetterService(
            ApplicationDbContext dbContext,
            AccessPermissionService accessPermissionService,
            HrdUnitScopeService unitScopeService,
            NumberSeriesAllocator numberSeriesAllocator,
            OfficialDutyRequestService officialDutyRequestService,
            IConfiguration configuration,
            ILogger<AssignmentLetterService> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _accessPermissionService = accessPermissionService ?? throw new ArgumentNullException(nameof(accessPermissionService));
            _unitScopeService = unitScopeService ?? throw new ArgumentNullException(nameof(unitScopeService));
            _numberSeriesAllocator = numberSeriesAllocator ?? throw new ArgumentNullException(nameof(numberSeriesAllocator));
            _officialDutyRequestService = officialDutyRequestService ?? throw new ArgumentNullException(nameof(officialDutyRequestService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private sealed record ActorContext(
            Guid UserId,
            Guid? ProfileId,
            bool ManageAllUnits,
            bool CanVerify,
            bool CanEndorse,
            bool CanIssue,
            bool CanCancel,
            bool CanReport,
            IReadOnlyCollection<Guid> ManagedUnitIds)
        {
            public bool CanSeeAll => ManageAllUnits || CanVerify || CanEndorse || CanIssue;
        }

        // =====================================================================
        // Baca
        // =====================================================================

        public async Task<PagedResult<AssignmentLetterListItemResponse>> GetPagedAsync(
            AssignmentLetterQuery query, ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            query ??= new AssignmentLetterQuery();
            var actor = await LoadActorAsync(user, cancellationToken);
            var (page, size) = HrCrossEmployeeLookup.NormalizePage(query.PageNumber, query.PageSize);

            var letters = ApplyReadScope(_dbContext.Set<HrdAssignmentLetter>().AsNoTracking().Where(x => !x.IsDelete), actor);

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                var status = query.Status.Trim();
                letters = letters.Where(x => x.LetterStatus == status);
            }

            var pattern = HrCrossEmployeeLookup.LikePattern(query.Search);
            if (pattern != null)
            {
                letters = letters.Where(x => EF.Functions.ILike(x.ActivityName, pattern)
                    || EF.Functions.ILike(x.Destination, pattern)
                    || (x.LetterNumber != null && EF.Functions.ILike(x.LetterNumber, pattern))
                    || x.Participants.Any(p => !p.IsDelete && EF.Functions.ILike(p.DisplayNameSnapshot, pattern)));
            }

            if (query.StartDate.HasValue)
            {
                var from = DateTime.SpecifyKind(query.StartDate.Value.Date, DateTimeKind.Utc);
                letters = letters.Where(x => x.EndAt >= from);
            }

            if (query.EndDate.HasValue)
            {
                var to = DateTime.SpecifyKind(query.EndDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                letters = letters.Where(x => x.StartAt < to);
            }

            var total = await letters.CountAsync(cancellationToken);
            var rows = await letters
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(x => new AssignmentLetterListItemResponse
                {
                    Id = x.Id,
                    LetterNumber = x.LetterNumber,
                    ActivityName = x.ActivityName,
                    DutyCategory = x.DutyCategory,
                    Destination = x.Destination,
                    StartAt = x.StartAt,
                    EndAt = x.EndAt,
                    LetterStatus = x.LetterStatus,
                    ParticipantCount = x.Participants.Count(p => !p.IsDelete),
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            rows.ForEach(r => r.LetterStatusLabel = AssignmentLetterStatuses.Label(r.LetterStatus));
            return HrCrossEmployeeLookup.ToPaged(rows, total, page, size);
        }

        public async Task<AssignmentLetterDetailResponse> GetByIdAsync(Guid id, ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            var actor = await LoadActorAsync(user, cancellationToken);
            var letter = await ApplyReadScope(_dbContext.Set<HrdAssignmentLetter>().AsNoTracking(), actor)
                .Include(x => x.Participants.Where(p => !p.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Surat Tugas tidak ditemukan.");

            return await MapDetailAsync(letter, actor, cancellationToken);
        }

        public async Task<(byte[] Content, string ContentType, string FileName)> GetDocumentAsync(
            Guid id, ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            var actor = await LoadActorAsync(user, cancellationToken);
            var letter = await ApplyReadScope(_dbContext.Set<HrdAssignmentLetter>().AsNoTracking(), actor)
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new { x.LetterNumber, x.DocumentSnapshotHtml })
                .FirstOrDefaultAsync(cancellationToken);

            if (letter == null || string.IsNullOrWhiteSpace(letter.DocumentSnapshotHtml))
            {
                throw new KeyNotFoundException("Naskah Surat Tugas belum diterbitkan atau tidak ditemukan.");
            }

            var safeName = (letter.LetterNumber ?? "surat-tugas").Replace('/', '-');
            return (Encoding.UTF8.GetBytes(letter.DocumentSnapshotHtml), "text/html; charset=utf-8", $"{safeName}.html");
        }

        // =====================================================================
        // Tulis
        // =====================================================================

        public async Task<AssignmentLetterDetailResponse> CreateAsync(
            SaveAssignmentLetterRequest request, ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            var actor = await LoadActorAsync(user, cancellationToken);
            ValidatePeriod(request);
            EnsureRequestingUnitAllowed(request.RequestingOrganizationUnitId, actor);
            var participants = await ResolveParticipantsAsync(request.Participants, actor, cancellationToken);
            var conflicts = await ComputeConflictsAsync(participants.Select(p => p.ProfileId).ToList(), request.StartAt, request.EndAt, null, cancellationToken);

            var now = DateTime.UtcNow;
            var letter = new HrdAssignmentLetter
            {
                Id = Guid.NewGuid(),
                CreateBy = actor.UserId,
                CreateDateTime = now,
                LetterStatus = AssignmentLetterStatuses.Draft,
                IsActive = true
            };
            ApplyHeader(letter, request);

            foreach (var p in participants)
            {
                letter.Participants.Add(NewParticipant(letter.Id, p, conflicts, actor.UserId, now));
            }

            _dbContext.Set<HrdAssignmentLetter>().Add(letter);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetByIdAsync(letter.Id, user, cancellationToken);
        }

        public async Task<AssignmentLetterDetailResponse> UpdateAsync(
            Guid id, SaveAssignmentLetterRequest request, ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            var actor = await LoadActorAsync(user, cancellationToken);
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var letter = await LoadForWriteAsync(id, actor, cancellationToken);

            if (letter.LetterStatus != AssignmentLetterStatuses.Draft && letter.LetterStatus != AssignmentLetterStatuses.RevisionNeeded)
            {
                throw new HrServiceConflictException("Surat Tugas hanya dapat diubah saat berstatus Draf atau Perlu Revisi.");
            }

            EnsureOwnerOrManager(letter, actor, "mengubah");
            ValidatePeriod(request);
            EnsureRequestingUnitAllowed(request.RequestingOrganizationUnitId, actor);
            var participants = await ResolveParticipantsAsync(request.Participants, actor, cancellationToken);
            var conflicts = await ComputeConflictsAsync(participants.Select(p => p.ProfileId).ToList(), request.StartAt, request.EndAt, letter.Id, cancellationToken);

            var now = DateTime.UtcNow;
            ApplyHeader(letter, request);
            letter.UpdateBy = actor.UserId;
            letter.UpdateDateTime = now;

            var keep = participants.ToDictionary(p => p.ProfileId);
            foreach (var existing in letter.Participants.Where(p => !p.IsDelete).ToList())
            {
                if (keep.TryGetValue(existing.WorkforceProfileId, out var snapshot))
                {
                    existing.ParticipantRole = snapshot.Role;
                    existing.DisplayNameSnapshot = snapshot.DisplayName;
                    existing.ProfileCodeSnapshot = snapshot.ProfileCode;
                    existing.PositionNameSnapshot = snapshot.PositionName;
                    existing.UnitNameSnapshot = snapshot.UnitName;
                    existing.ScheduleConflictNote = ConflictNote(conflicts, existing.WorkforceProfileId);
                    existing.UpdateBy = actor.UserId;
                    existing.UpdateDateTime = now;
                    keep.Remove(existing.WorkforceProfileId);
                }
                else
                {
                    existing.IsDelete = true;
                    existing.DeleteBy = actor.UserId;
                    existing.DeleteDateTime = now;
                }
            }

            foreach (var p in keep.Values)
            {
                _dbContext.Set<HrdAssignmentLetterParticipant>().Add(NewParticipant(letter.Id, p, conflicts, actor.UserId, now));
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return await GetByIdAsync(letter.Id, user, cancellationToken);
        }

        public Task<AssignmentLetterDetailResponse> SubmitAsync(Guid id, ClaimsPrincipal user, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, user, cancellationToken, (letter, actor, now) =>
            {
                if (letter.LetterStatus != AssignmentLetterStatuses.Draft && letter.LetterStatus != AssignmentLetterStatuses.RevisionNeeded)
                {
                    throw new HrServiceConflictException("Hanya Surat Tugas berstatus Draf atau Perlu Revisi yang dapat diajukan.");
                }

                EnsureOwnerOrManager(letter, actor, "mengajukan");
                if (!letter.Participants.Any(p => !p.IsDelete))
                {
                    throw new InvalidOperationException("Surat Tugas wajib memuat minimal satu peserta sebelum diajukan.");
                }

                letter.LetterStatus = AssignmentLetterStatuses.Submitted;
                letter.SubmittedByUserId = actor.UserId;
                letter.SubmittedAt = now;
                return Task.CompletedTask;
            });

        public Task<AssignmentLetterDetailResponse> VerifyAsync(
            Guid id, VerifyAssignmentLetterRequest request, ClaimsPrincipal user, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, user, cancellationToken, (letter, actor, now) =>
            {
                if (letter.LetterStatus != AssignmentLetterStatuses.Submitted)
                {
                    throw new HrServiceConflictException("Verifikasi HRD hanya untuk Surat Tugas berstatus Diajukan.");
                }

                if (actor.UserId == letter.CreateBy || actor.UserId == letter.SubmittedByUserId)
                {
                    throw new UnauthorizedAccessException("Pembuat/pengaju Surat Tugas tidak dapat memverifikasi pengajuannya sendiri.");
                }

                var note = request.Note?.Trim();
                switch (request.Decision)
                {
                    case "Approve":
                        letter.LetterStatus = AssignmentLetterStatuses.PendingSignOff;
                        letter.VerifiedByUserId = actor.UserId;
                        letter.VerifiedAt = now;
                        letter.VerificationNote = note;
                        break;
                    case "RequestRevision":
                        RequireText(note, "Catatan revisi wajib diisi.");
                        letter.LetterStatus = AssignmentLetterStatuses.RevisionNeeded;
                        letter.VerifiedByUserId = actor.UserId;
                        letter.VerifiedAt = now;
                        letter.RevisionNote = note;
                        break;
                    default:
                        RequireText(note, "Alasan penolakan wajib diisi.");
                        letter.LetterStatus = AssignmentLetterStatuses.Rejected;
                        letter.RejectedByUserId = actor.UserId;
                        letter.RejectedAt = now;
                        letter.RejectionReason = note;
                        break;
                }

                return Task.CompletedTask;
            });

        public Task<AssignmentLetterDetailResponse> EndorseAsync(
            Guid id, EndorseAssignmentLetterRequest request, ClaimsPrincipal user, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, user, cancellationToken, async (letter, actor, now) =>
            {
                if (letter.LetterStatus != AssignmentLetterStatuses.PendingSignOff)
                {
                    throw new HrServiceConflictException("Pengesahan hanya untuk Surat Tugas yang sudah lulus verifikasi HRD (Menunggu Pengesahan).");
                }

                if (actor.UserId == letter.CreateBy || actor.UserId == letter.SubmittedByUserId || actor.UserId == letter.VerifiedByUserId)
                {
                    throw new UnauthorizedAccessException("Pengesah harus berbeda dari pembuat, pengaju, dan verifikator Surat Tugas.");
                }

                var note = request.Note?.Trim();
                if (request.Decision == "Endorse")
                {
                    var signer = await _dbContext.Users.AsNoTracking()
                        .Where(u => u.Id == actor.UserId)
                        .Select(u => new { u.DisplayName, u.UserName })
                        .FirstOrDefaultAsync(cancellationToken);
                    var positionName = actor.ProfileId.HasValue
                        ? await _dbContext.MstWorkforceProfiles.AsNoTracking()
                            .Where(p => p.Id == actor.ProfileId.Value)
                            .Select(p => p.PrimaryPosition != null ? p.PrimaryPosition.PositionName : null)
                            .FirstOrDefaultAsync(cancellationToken)
                        : null;

                    letter.LetterStatus = AssignmentLetterStatuses.Endorsed;
                    letter.EndorsedByUserId = actor.UserId;
                    letter.EndorsedAt = now;
                    letter.EndorsementNote = note;
                    letter.SignerName = !string.IsNullOrWhiteSpace(signer?.DisplayName) ? signer!.DisplayName.Trim() : signer?.UserName;
                    letter.SignerTitle = !string.IsNullOrWhiteSpace(request.SignerTitle) ? request.SignerTitle.Trim() : positionName;
                    if (string.IsNullOrWhiteSpace(letter.SignerName) || string.IsNullOrWhiteSpace(letter.SignerTitle))
                    {
                        throw new InvalidOperationException("Nama dan jabatan pengesah wajib tersedia (isi jabatan penandatangan bila akun belum memiliki jabatan utama).");
                    }
                }
                else
                {
                    RequireText(note, "Alasan penolakan pengesahan wajib diisi.");
                    letter.LetterStatus = AssignmentLetterStatuses.Rejected;
                    letter.RejectedByUserId = actor.UserId;
                    letter.RejectedAt = now;
                    letter.RejectionReason = note;
                }
            });

        public async Task<AssignmentLetterDetailResponse> IssueAsync(Guid id, ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            var actor = await LoadActorAsync(user, cancellationToken);
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var letter = await LoadForWriteAsync(id, actor, cancellationToken);

            // BE-HRD-383: tidak ada nomor resmi dan penerbitan sebelum disahkan pejabat.
            if (letter.LetterStatus != AssignmentLetterStatuses.Endorsed)
            {
                throw new HrServiceConflictException("Surat Tugas hanya dapat diterbitkan sesudah disahkan pejabat berwenang.");
            }

            var letterhead = await HrLetterheadResolver.TryResolveAsync(_dbContext, cancellationToken)
                ?? throw new InvalidOperationException(HrLetterheadResolver.MissingIdentityMessage);

            var now = DateTime.UtcNow;
            var number = await _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: SequenceKey,
                    Prefix: SeriesPrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: SequenceDigits,
                    ActorUserId: actor.UserId,
                    Instant: DateTimeOffset.UtcNow),
                cancellationToken);
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{letter.Id}|{number}|{now.Ticks}|HRD_ASSIGNMENT_LETTER"))).ToLowerInvariant();
            var verificationUrl = HrLetterheadResolver.BuildVerificationUrl(_configuration, hash);

            var participants = letter.Participants.Where(p => !p.IsDelete).OrderBy(p => p.DisplayNameSnapshot).ToList();
            var endorserProfileId = letter.EndorsedByUserId.HasValue
                ? await _dbContext.Users.AsNoTracking().Where(u => u.Id == letter.EndorsedByUserId.Value).Select(u => u.WorkforceProfileId).FirstOrDefaultAsync(cancellationToken)
                : null;

            // Presensi: satu penugasan dinas luar disetujui per peserta, lewat service presensi yang sudah ada
            // (sehingga hari-hari penugasan tercatat OnDuty dan periode presensi tertutup tetap dijaga).
            foreach (var participant in participants)
            {
                var created = await _officialDutyRequestService.CreateAsync(new CreateOfficialDutyRequest
                {
                    WorkforceProfileId = participant.WorkforceProfileId,
                    DutyCategory = Truncate(letter.DutyCategory, 50),
                    AssignmentOrderNumber = number,
                    StartAt = letter.StartAt,
                    EndAt = letter.EndAt,
                    Destination = Truncate(letter.Destination, 255),
                    Purpose = $"{letter.ActivityName}: {letter.TaskDescription}"
                }, actor.UserId, cancellationToken);
                if (!created.Success || created.Data == null)
                {
                    throw DutyFailure(participant.DisplayNameSnapshot, created.StatusCode, created.Message);
                }

                var approved = await _officialDutyRequestService.ApproveAsync(created.Data.Id, new ApproveOfficialDutyRequest
                {
                    ApproverWorkforceProfileId = endorserProfileId,
                    ApprovalNote = $"Surat Tugas {number}"
                }, actor.UserId, cancellationToken);
                if (!approved.Success)
                {
                    throw DutyFailure(participant.DisplayNameSnapshot, approved.StatusCode, approved.Message);
                }

                participant.OfficialDutyRequestId = created.Data.Id;
                participant.UpdateBy = actor.UserId;
                participant.UpdateDateTime = now;
            }

            letter.LetterNumber = number;
            letter.VerificationHash = hash;
            letter.IssuedByUserId = actor.UserId;
            letter.IssuedAt = now;
            letter.LetterStatus = AssignmentLetterStatuses.Issued;
            letter.DocumentSnapshotHtml = RenderLetterHtml(letter, participants, letterhead, verificationUrl, now);
            letter.UpdateBy = actor.UserId;
            letter.UpdateDateTime = now;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Surat Tugas {Number} diterbitkan untuk {Count} peserta.", number, participants.Count);
            return await GetByIdAsync(letter.Id, user, cancellationToken);
        }

        public Task<AssignmentLetterDetailResponse> StartExecutionAsync(Guid id, ClaimsPrincipal user, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, user, cancellationToken, (letter, actor, now) =>
            {
                if (letter.LetterStatus != AssignmentLetterStatuses.Issued)
                {
                    throw new HrServiceConflictException("Pelaksanaan hanya dapat dimulai untuk Surat Tugas yang sudah diterbitkan.");
                }

                EnsureExecutionActor(letter, actor);
                if (now < letter.StartAt)
                {
                    throw new HrServiceConflictException("Pelaksanaan belum dapat dimulai sebelum tanggal mulai penugasan.");
                }

                letter.LetterStatus = AssignmentLetterStatuses.InExecution;
                letter.ExecutionStartedByUserId = actor.UserId;
                letter.ExecutionStartedAt = now;
                return Task.CompletedTask;
            });

        public Task<AssignmentLetterDetailResponse> CompleteAsync(
            Guid id, CompleteAssignmentLetterRequest request, ClaimsPrincipal user, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, user, cancellationToken, (letter, actor, now) =>
            {
                if (letter.LetterStatus != AssignmentLetterStatuses.Issued && letter.LetterStatus != AssignmentLetterStatuses.InExecution)
                {
                    throw new HrServiceConflictException("Hanya Surat Tugas yang diterbitkan atau sedang dilaksanakan yang dapat diselesaikan.");
                }

                EnsureExecutionActor(letter, actor);
                if (now < letter.StartAt)
                {
                    throw new HrServiceConflictException("Surat Tugas belum dapat diselesaikan sebelum tanggal mulai penugasan.");
                }

                RequireText(request.ResultReport, "Laporan hasil tugas wajib diisi.");
                letter.LetterStatus = AssignmentLetterStatuses.Completed;
                letter.ExecutionStartedAt ??= now;
                letter.ExecutionStartedByUserId ??= actor.UserId;
                letter.CompletedByUserId = actor.UserId;
                letter.CompletedAt = now;
                letter.ResultReport = request.ResultReport.Trim();
                letter.ResultEvidenceReference = request.ResultEvidenceReference?.Trim();
                return Task.CompletedTask;
            });

        public Task<AssignmentLetterDetailResponse> CancelAsync(
            Guid id, CancelAssignmentLetterRequest request, ClaimsPrincipal user, CancellationToken cancellationToken = default) =>
            TransitionAsync(id, user, cancellationToken, async (letter, actor, now) =>
            {
                RequireText(request.Reason, "Alasan pembatalan wajib diisi.");
                var isIssued = letter.LetterStatus == AssignmentLetterStatuses.Issued;
                if (!CancellableBeforeIssue.Contains(letter.LetterStatus) && !isIssued)
                {
                    throw new HrServiceConflictException($"Surat Tugas berstatus {AssignmentLetterStatuses.Label(letter.LetterStatus)} tidak dapat dibatalkan.");
                }

                if (isIssued && now >= letter.StartAt)
                {
                    throw new HrServiceConflictException("Surat Tugas yang sudah memasuki masa pelaksanaan tidak dapat dibatalkan; selesaikan dengan laporan hasil.");
                }

                if (letter.CreateBy != actor.UserId && !actor.ManageAllUnits && !actor.CanIssue)
                {
                    throw new UnauthorizedAccessException("Hanya pembuat Surat Tugas atau petugas penerbit yang dapat membatalkan.");
                }

                if (isIssued)
                {
                    await RevertAttendanceDutiesAsync(letter, actor.UserId, now, cancellationToken);
                }

                letter.LetterStatus = AssignmentLetterStatuses.Cancelled;
                letter.CancelledByUserId = actor.UserId;
                letter.CancelledAt = now;
                letter.CancellationReason = request.Reason.Trim();
            });

        // =====================================================================
        // Bantuan
        // =====================================================================

        private async Task<AssignmentLetterDetailResponse> TransitionAsync(
            Guid id, ClaimsPrincipal user, CancellationToken cancellationToken, Func<HrdAssignmentLetter, ActorContext, DateTime, Task> apply)
        {
            var actor = await LoadActorAsync(user, cancellationToken);
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            var letter = await LoadForWriteAsync(id, actor, cancellationToken);
            var now = DateTime.UtcNow;
            await apply(letter, actor, now);
            letter.UpdateBy = actor.UserId;
            letter.UpdateDateTime = now;
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return await GetByIdAsync(id, user, cancellationToken);
        }

        /// <summary>Kunci transaksi per surat + baca ulang, sehingga dua aksi paralel tidak sama-sama lolos cek status.</summary>
        private async Task<HrdAssignmentLetter> LoadForWriteAsync(Guid id, ActorContext actor, CancellationToken cancellationToken)
        {
            if (_dbContext.Database.IsNpgsql())
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock(hashtext({0}));", [$"HRD_ASSIGNMENT_LETTER_{id:N}"], cancellationToken);
            }

            return await ApplyReadScope(_dbContext.Set<HrdAssignmentLetter>(), actor)
                .Include(x => x.Participants)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Surat Tugas tidak ditemukan.");
        }

        private async Task<ActorContext> LoadActorAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var raw = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub") ?? user.FindFirstValue("id");
            if (!Guid.TryParse(raw, out var userId) || userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Identitas pengguna tidak valid.");
            }

            var profileId = await _dbContext.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => u.WorkforceProfileId)
                .FirstOrDefaultAsync(cancellationToken);

            var scope = await _unitScopeService.GetManagerialReadScopeAsync(cancellationToken);
            return new ActorContext(
                userId,
                profileId,
                await HasAsync(user, AssignmentLetterPermissions.ManageAllUnits),
                await HasAsync(user, "Verify"),
                await HasAsync(user, "Endorse"),
                await HasAsync(user, "Issue"),
                await HasAsync(user, "Cancel"),
                await HasAsync(user, "Report"),
                scope.UnitIds.ToList());
        }

        private Task<bool> HasAsync(ClaimsPrincipal user, string action) => _accessPermissionService.HasAccessAsync(user, Resource, action);

        private static IQueryable<HrdAssignmentLetter> ApplyReadScope(IQueryable<HrdAssignmentLetter> query, ActorContext actor)
        {
            if (actor.CanSeeAll)
            {
                return query;
            }

            var userId = actor.UserId;
            var profileId = actor.ProfileId ?? Guid.Empty;
            var units = actor.ManagedUnitIds.ToList();
            return query.Where(x => x.CreateBy == userId
                || x.Participants.Any(p => !p.IsDelete && p.WorkforceProfileId == profileId)
                || (x.RequestingOrganizationUnitId != null && units.Contains(x.RequestingOrganizationUnitId.Value)));
        }

        private static void EnsureOwnerOrManager(HrdAssignmentLetter letter, ActorContext actor, string action)
        {
            if (letter.CreateBy != actor.UserId && !actor.ManageAllUnits)
            {
                throw new UnauthorizedAccessException($"Hanya pembuat Surat Tugas yang dapat {action} surat ini.");
            }
        }

        private static void EnsureExecutionActor(HrdAssignmentLetter letter, ActorContext actor)
        {
            var isParticipant = actor.ProfileId.HasValue && letter.Participants.Any(p => !p.IsDelete && p.WorkforceProfileId == actor.ProfileId.Value);
            if (letter.CreateBy != actor.UserId && !isParticipant && !actor.ManageAllUnits)
            {
                throw new UnauthorizedAccessException("Status pelaksanaan hanya dapat dicatat oleh pembuat surat atau peserta yang ditugaskan.");
            }
        }

        private static void EnsureRequestingUnitAllowed(Guid? unitId, ActorContext actor)
        {
            if (unitId.HasValue && !actor.ManageAllUnits && !actor.ManagedUnitIds.Contains(unitId.Value))
            {
                throw new UnauthorizedAccessException("Unit pengaju harus unit yang Anda pimpin.");
            }
        }

        private static void ValidatePeriod(SaveAssignmentLetterRequest request)
        {
            if (request.EndAt <= request.StartAt)
            {
                throw new InvalidOperationException("Waktu selesai penugasan harus sesudah waktu mulai.");
            }
        }

        private static void ApplyHeader(HrdAssignmentLetter letter, SaveAssignmentLetterRequest request)
        {
            letter.ActivityName = request.ActivityName.Trim();
            letter.DutyCategory = request.DutyCategory.Trim();
            letter.BasisDescription = request.BasisDescription?.Trim();
            letter.BasisDocumentReference = request.BasisDocumentReference?.Trim();
            letter.TaskDescription = request.TaskDescription.Trim();
            letter.Destination = request.Destination.Trim();
            letter.StartAt = DateTime.SpecifyKind(request.StartAt, request.StartAt.Kind == DateTimeKind.Unspecified ? DateTimeKind.Utc : request.StartAt.Kind).ToUniversalTime();
            letter.EndAt = DateTime.SpecifyKind(request.EndAt, request.EndAt.Kind == DateTimeKind.Unspecified ? DateTimeKind.Utc : request.EndAt.Kind).ToUniversalTime();
            letter.RequestingOrganizationUnitId = request.RequestingOrganizationUnitId;
        }

        private sealed record ParticipantSnapshot(Guid ProfileId, string? Role, string DisplayName, string? ProfileCode, string? PositionName, string? UnitName);

        /// <summary>
        /// BE-HRD-382/383: peserta harus ada, aktif, tidak ganda, dan (kecuali pemegang penanda semua-unit) berada di unit
        /// yang dipimpin pengaju menurut penempatan hari ini.
        /// </summary>
        private async Task<List<ParticipantSnapshot>> ResolveParticipantsAsync(
            List<AssignmentLetterParticipantInput> inputs, ActorContext actor, CancellationToken cancellationToken)
        {
            inputs ??= new List<AssignmentLetterParticipantInput>();
            var ids = inputs.Select(i => i.WorkforceProfileId).Where(x => x != Guid.Empty).ToList();
            if (ids.Count == 0)
            {
                throw new InvalidOperationException("Surat Tugas wajib memuat minimal satu peserta.");
            }

            if (ids.Distinct().Count() != ids.Count)
            {
                throw new InvalidOperationException("Peserta Surat Tugas tidak boleh ganda.");
            }

            var profiles = await _dbContext.MstWorkforceProfiles.AsNoTracking()
                .Where(p => ids.Contains(p.Id) && !p.IsDelete)
                .Select(p => new
                {
                    p.Id,
                    p.IsActive,
                    p.DisplayName,
                    p.ProfileCode,
                    PositionName = p.PrimaryPosition != null ? p.PrimaryPosition.PositionName : null,
                    UnitName = p.PrimaryDepartment != null ? p.PrimaryDepartment.DepartmentName : null
                })
                .ToListAsync(cancellationToken);

            if (profiles.Count != ids.Count)
            {
                throw new InvalidOperationException("Sebagian peserta tidak ditemukan.");
            }

            var inactive = profiles.Where(p => !p.IsActive).Select(p => p.DisplayName).ToList();
            if (inactive.Count > 0)
            {
                throw new InvalidOperationException("Peserta berikut tidak aktif sehingga tidak dapat ditugaskan: " + string.Join(", ", inactive) + ".");
            }

            if (!actor.ManageAllUnits)
            {
                var today = DateTime.UtcNow.Date;
                var units = actor.ManagedUnitIds.ToList();
                var inScope = units.Count == 0
                    ? new List<Guid>()
                    : await _dbContext.WfpOrganizationAssignments.AsNoTracking()
                        .Where(a => ids.Contains(a.WorkforceProfileId) &&
                                    a.OrganizationUnitId.HasValue && units.Contains(a.OrganizationUnitId.Value) &&
                                    a.IsActive && !a.IsDelete &&
                                    a.EffectiveStartDate <= today &&
                                    (a.EffectiveEndDate == null || a.EffectiveEndDate >= today))
                        .Select(a => a.WorkforceProfileId)
                        .Distinct()
                        .ToListAsync(cancellationToken);

                var outside = profiles
                    .Where(p => !inScope.Contains(p.Id) && p.Id != actor.ProfileId)
                    .Select(p => p.DisplayName)
                    .ToList();
                if (outside.Count > 0)
                {
                    throw new UnauthorizedAccessException("Peserta berikut di luar unit yang Anda pimpin: " + string.Join(", ", outside) + ".");
                }
            }

            var roles = inputs.ToDictionary(i => i.WorkforceProfileId, i => i.ParticipantRole?.Trim());
            return profiles
                .Select(p => new ParticipantSnapshot(p.Id, roles.GetValueOrDefault(p.Id), p.DisplayName, p.ProfileCode, p.PositionName, p.UnitName))
                .ToList();
        }

        private static HrdAssignmentLetterParticipant NewParticipant(
            Guid letterId, ParticipantSnapshot p, Dictionary<Guid, List<string>> conflicts, Guid actorUserId, DateTime now) => new()
        {
            Id = Guid.NewGuid(),
            AssignmentLetterId = letterId,
            WorkforceProfileId = p.ProfileId,
            ParticipantRole = p.Role,
            DisplayNameSnapshot = p.DisplayName,
            ProfileCodeSnapshot = p.ProfileCode,
            PositionNameSnapshot = p.PositionName,
            UnitNameSnapshot = p.UnitName,
            ScheduleConflictNote = ConflictNote(conflicts, p.ProfileId),
            IsActive = true,
            CreateBy = actorUserId,
            CreateDateTime = now
        };

        private static string? ConflictNote(Dictionary<Guid, List<string>> conflicts, Guid profileId) =>
            conflicts.TryGetValue(profileId, out var list) && list.Count > 0 ? Truncate(string.Join("; ", list), 500) : null;

        /// <summary>
        /// BE-HRD-383: peringatan benturan jadwal (tidak memblokir) — shift terjadwal, Surat Tugas lain yang aktif, dan
        /// dinas luar presensi lain yang disetujui, yang tumpang tindih dengan periode penugasan.
        /// </summary>
        private async Task<Dictionary<Guid, List<string>>> ComputeConflictsAsync(
            List<Guid> profileIds, DateTime startAt, DateTime endAt, Guid? excludeLetterId, CancellationToken cancellationToken)
        {
            var result = profileIds.ToDictionary(id => id, _ => new List<string>());
            if (profileIds.Count == 0)
            {
                return result;
            }

            var start = startAt.ToUniversalTime();
            var end = endAt.ToUniversalTime();

            var shifts = await _dbContext.Set<TrxShiftAssignment>().AsNoTracking()
                .Where(s => profileIds.Contains(s.WorkforceProfileId) && !s.IsDelete && s.AssignmentStatus != "Cancelled"
                            && s.ScheduledStartAt < end && s.ScheduledEndAt > start)
                .Select(s => new { s.WorkforceProfileId, s.ShiftDate })
                .ToListAsync(cancellationToken);
            foreach (var s in shifts)
            {
                result[s.WorkforceProfileId].Add($"Jadwal shift {s.ShiftDate.ToString("dd MMM yyyy", IdCulture)}");
            }

            var letters = await _dbContext.Set<HrdAssignmentLetterParticipant>().AsNoTracking()
                .Where(p => profileIds.Contains(p.WorkforceProfileId) && !p.IsDelete
                            && p.AssignmentLetter != null && !p.AssignmentLetter.IsDelete
                            && (excludeLetterId == null || p.AssignmentLetterId != excludeLetterId.Value)
                            && !InactiveForConflict.Contains(p.AssignmentLetter.LetterStatus)
                            && p.AssignmentLetter.StartAt < end && p.AssignmentLetter.EndAt > start)
                .Select(p => new { p.WorkforceProfileId, p.AssignmentLetter!.ActivityName })
                .ToListAsync(cancellationToken);
            foreach (var l in letters)
            {
                result[l.WorkforceProfileId].Add($"Surat Tugas lain: {l.ActivityName}");
            }

            var linkedDutyIds = _dbContext.Set<HrdAssignmentLetterParticipant>()
                .Where(p => p.OfficialDutyRequestId != null)
                .Select(p => p.OfficialDutyRequestId!.Value);
            var duties = await _dbContext.Set<HrdOfficialDutyRequest>().AsNoTracking()
                .Where(d => profileIds.Contains(d.WorkforceProfileId) && !d.IsDelete && d.Status == "Approved"
                            && d.StartAt < end && d.EndAt > start && !linkedDutyIds.Contains(d.Id))
                .Select(d => new { d.WorkforceProfileId, d.AssignmentOrderNumber })
                .ToListAsync(cancellationToken);
            foreach (var d in duties)
            {
                result[d.WorkforceProfileId].Add($"Dinas luar {d.AssignmentOrderNumber}");
            }

            return result;
        }

        /// <summary>
        /// Pembatalan Surat Tugas yang sudah terbit (sebelum masa pelaksanaan): penugasan dinas luar presensi ditandai
        /// <c>Cancelled</c> dan hari presensi yang ditautkan dikembalikan untuk diproses ulang (bukan dihapus).
        /// </summary>
        private async Task RevertAttendanceDutiesAsync(HrdAssignmentLetter letter, Guid actorUserId, DateTime now, CancellationToken cancellationToken)
        {
            var dutyIds = letter.Participants.Where(p => p.OfficialDutyRequestId.HasValue).Select(p => p.OfficialDutyRequestId!.Value).ToList();
            if (dutyIds.Count == 0)
            {
                return;
            }

            var duties = await _dbContext.Set<HrdOfficialDutyRequest>().Where(d => dutyIds.Contains(d.Id)).ToListAsync(cancellationToken);
            foreach (var duty in duties)
            {
                duty.Status = "Cancelled";
                duty.RejectionReason = $"Surat Tugas {letter.LetterNumber} dibatalkan.";
                duty.UpdateBy = actorUserId;
                duty.UpdateDateTime = now;
            }

            var dailies = await _dbContext.Set<HrdAttendanceDaily>()
                .Where(d => d.OfficialDutyRequestId.HasValue && dutyIds.Contains(d.OfficialDutyRequestId.Value) && !d.IsDelete)
                .ToListAsync(cancellationToken);
            foreach (var daily in dailies)
            {
                daily.OfficialDutyRequestId = null;
                daily.AttendanceStatus = AttendanceValueConstants.AttendanceStatus.Unprocessed;
                daily.IsPresent = false;
                daily.ProcessingStatus = AttendanceValueConstants.AttendanceProcessingStatus.ReprocessRequired;
                daily.ProcessingMessage = $"Surat Tugas {letter.LetterNumber} dibatalkan; hari ini perlu diproses ulang.";
                daily.UpdateBy = actorUserId;
                daily.UpdateDateTime = now;
            }
        }

        private static Exception DutyFailure(string participantName, int statusCode, string message)
        {
            var text = $"Penugasan presensi untuk {participantName} gagal dibuat: {message}";
            return statusCode == StatusCodes.Status409Conflict ? new HrServiceConflictException(text) : new InvalidOperationException(text);
        }

        private async Task<AssignmentLetterDetailResponse> MapDetailAsync(HrdAssignmentLetter letter, ActorContext actor, CancellationToken cancellationToken)
        {
            var participants = letter.Participants.Where(p => !p.IsDelete).OrderBy(p => p.DisplayNameSnapshot).ToList();
            var live = InactiveForConflict.Contains(letter.LetterStatus) && letter.LetterStatus != AssignmentLetterStatuses.Draft
                ? new Dictionary<Guid, List<string>>()
                : await ComputeConflictsAsync(participants.Select(p => p.WorkforceProfileId).ToList(), letter.StartAt, letter.EndAt, letter.Id, cancellationToken);

            var actorIds = new[]
            {
                letter.CreateBy, letter.SubmittedByUserId ?? Guid.Empty, letter.VerifiedByUserId ?? Guid.Empty, letter.EndorsedByUserId ?? Guid.Empty,
                letter.IssuedByUserId ?? Guid.Empty, letter.RejectedByUserId ?? Guid.Empty, letter.CancelledByUserId ?? Guid.Empty,
                letter.ExecutionStartedByUserId ?? Guid.Empty, letter.CompletedByUserId ?? Guid.Empty
            }.Where(x => x != Guid.Empty).Distinct().ToList();
            var names = await _dbContext.Users.AsNoTracking()
                .Where(u => actorIds.Contains(u.Id))
                .Select(u => new { u.Id, u.DisplayName, u.UserName })
                .ToDictionaryAsync(u => u.Id, u => string.IsNullOrWhiteSpace(u.DisplayName) ? u.UserName : u.DisplayName, cancellationToken);
            string? NameOf(Guid? id) => id.HasValue && names.TryGetValue(id.Value, out var n) ? n : null;

            var timeline = new List<AssignmentLetterTimelineEntry>
            {
                new() { Step = "Dibuat", ActorUserId = letter.CreateBy, ActorName = NameOf(letter.CreateBy), At = letter.CreateDateTime }
            };
            void Add(string step, Guid? by, DateTime? at, string? note = null)
            {
                if (at.HasValue) timeline.Add(new AssignmentLetterTimelineEntry { Step = step, ActorUserId = by, ActorName = NameOf(by), At = at, Note = note });
            }
            Add("Diajukan", letter.SubmittedByUserId, letter.SubmittedAt);
            Add("Diverifikasi HRD", letter.VerifiedByUserId, letter.VerifiedAt, letter.RevisionNote ?? letter.VerificationNote);
            Add("Disahkan", letter.EndorsedByUserId, letter.EndorsedAt, letter.EndorsementNote);
            Add("Diterbitkan", letter.IssuedByUserId, letter.IssuedAt, letter.LetterNumber);
            Add("Mulai dilaksanakan", letter.ExecutionStartedByUserId, letter.ExecutionStartedAt);
            Add("Selesai", letter.CompletedByUserId, letter.CompletedAt, letter.ResultReport);
            Add("Ditolak", letter.RejectedByUserId, letter.RejectedAt, letter.RejectionReason);
            Add("Dibatalkan", letter.CancelledByUserId, letter.CancelledAt, letter.CancellationReason);

            return new AssignmentLetterDetailResponse
            {
                Id = letter.Id,
                LetterNumber = letter.LetterNumber,
                ActivityName = letter.ActivityName,
                DutyCategory = letter.DutyCategory,
                Destination = letter.Destination,
                StartAt = letter.StartAt,
                EndAt = letter.EndAt,
                LetterStatus = letter.LetterStatus,
                LetterStatusLabel = AssignmentLetterStatuses.Label(letter.LetterStatus),
                ParticipantCount = participants.Count,
                CreateDateTime = letter.CreateDateTime,
                BasisDescription = letter.BasisDescription,
                BasisDocumentReference = letter.BasisDocumentReference,
                TaskDescription = letter.TaskDescription,
                RequestingOrganizationUnitId = letter.RequestingOrganizationUnitId,
                SignerName = letter.SignerName,
                SignerTitle = letter.SignerTitle,
                RevisionNote = letter.RevisionNote,
                RejectionReason = letter.RejectionReason,
                CancellationReason = letter.CancellationReason,
                ResultReport = letter.ResultReport,
                ResultEvidenceReference = letter.ResultEvidenceReference,
                HasDocument = !string.IsNullOrWhiteSpace(letter.DocumentSnapshotHtml),
                Participants = participants.Select(p => new AssignmentLetterParticipantResponse
                {
                    Id = p.Id,
                    WorkforceProfileId = p.WorkforceProfileId,
                    ParticipantRole = p.ParticipantRole,
                    DisplayName = p.DisplayNameSnapshot,
                    ProfileCode = p.ProfileCodeSnapshot,
                    PositionName = p.PositionNameSnapshot,
                    UnitName = p.UnitNameSnapshot,
                    OfficialDutyRequestId = p.OfficialDutyRequestId,
                    ScheduleConflicts = live.TryGetValue(p.WorkforceProfileId, out var c) ? c : new List<string>()
                }).ToList(),
                Timeline = timeline.OrderBy(t => t.At).ToList(),
                AvailableActions = ResolveActions(letter, actor)
            };
        }

        private static List<string> ResolveActions(HrdAssignmentLetter letter, ActorContext actor)
        {
            var actions = new List<string>();
            var isOwner = letter.CreateBy == actor.UserId || actor.ManageAllUnits;
            var isParticipant = actor.ProfileId.HasValue && letter.Participants.Any(p => !p.IsDelete && p.WorkforceProfileId == actor.ProfileId.Value);
            var now = DateTime.UtcNow;
            switch (letter.LetterStatus)
            {
                case AssignmentLetterStatuses.Draft:
                case AssignmentLetterStatuses.RevisionNeeded:
                    if (isOwner) actions.AddRange(new[] { "update", "submit" });
                    break;
                case AssignmentLetterStatuses.Submitted:
                    if (actor.CanVerify && letter.CreateBy != actor.UserId && letter.SubmittedByUserId != actor.UserId) actions.Add("verify");
                    break;
                case AssignmentLetterStatuses.PendingSignOff:
                    if (actor.CanEndorse && letter.CreateBy != actor.UserId && letter.SubmittedByUserId != actor.UserId && letter.VerifiedByUserId != actor.UserId) actions.Add("endorse");
                    break;
                case AssignmentLetterStatuses.Endorsed:
                    if (actor.CanIssue) actions.Add("issue");
                    break;
                case AssignmentLetterStatuses.Issued:
                    if ((isOwner || isParticipant) && actor.CanReport && now >= letter.StartAt) actions.AddRange(new[] { "start-execution", "complete" });
                    break;
                case AssignmentLetterStatuses.InExecution:
                    if ((isOwner || isParticipant) && actor.CanReport) actions.Add("complete");
                    break;
            }

            var cancellable = CancellableBeforeIssue.Contains(letter.LetterStatus)
                || (letter.LetterStatus == AssignmentLetterStatuses.Issued && now < letter.StartAt);
            if (cancellable && actor.CanCancel && (letter.CreateBy == actor.UserId || actor.ManageAllUnits || actor.CanIssue))
            {
                actions.Add("cancel");
            }

            return actions;
        }

        /// <summary>
        /// BE-HRD-384: naskah SURAT TUGAS — kop dari Master Lokasi RS, nomor resmi, dasar, daftar peserta (tabel), tugas,
        /// tujuan, periode, penandatangan, dan QR verifikasi. Semua nilai di-encode HTML.
        /// </summary>
        private static string RenderLetterHtml(
            HrdAssignmentLetter letter, List<HrdAssignmentLetterParticipant> participants, HrLetterhead letterhead, string verificationUrl, DateTime issuedAt)
        {
            static string E(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
            var tz = ResolveBusinessTimeZone();
            string Local(DateTime utc) => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), tz).ToString("dd MMMM yyyy HH:mm", IdCulture);
            var issuedLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(issuedAt, DateTimeKind.Utc), tz).ToString("dd MMMM yyyy", IdCulture);

            string qrDataUri;
            using (var generator = new QRCodeGenerator())
            using (var data = generator.CreateQrCode(verificationUrl, QRCodeGenerator.ECCLevel.M))
            using (var code = new PngByteQRCode(data))
            {
                qrDataUri = "data:image/png;base64," + Convert.ToBase64String(code.GetGraphic(QrPixelsPerModule, true));
            }

            var rows = new StringBuilder();
            for (var i = 0; i < participants.Count; i++)
            {
                var p = participants[i];
                rows.Append($"<tr><td>{i + 1}</td><td>{E(p.DisplayNameSnapshot)}</td><td>{E(p.ProfileCodeSnapshot)}</td><td>{E(p.PositionNameSnapshot)}</td><td>{E(p.UnitNameSnapshot)}</td><td>{E(p.ParticipantRole)}</td></tr>");
            }

            var basis = string.IsNullOrWhiteSpace(letter.BasisDescription) && string.IsNullOrWhiteSpace(letter.BasisDocumentReference)
                ? string.Empty
                : $"<p><strong>Dasar:</strong> {E(letter.BasisDescription)} {E(letter.BasisDocumentReference)}</p>";
            var city = string.IsNullOrWhiteSpace(letterhead.City) ? string.Empty : $"{E(letterhead.City)}, ";

            return $@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'/>
<title>Surat Tugas {E(letter.LetterNumber)}</title>
<style>
body {{ font-family: 'Times New Roman', serif; margin: 40px; color: #1e293b; line-height: 1.5; }}
.header {{ text-align: center; border-bottom: 3px double #0f172a; padding-bottom: 8px; margin-bottom: 20px; }}
.header h2 {{ margin: 0; font-size: 20px; text-transform: uppercase; }}
.header p {{ margin: 2px 0; font-size: 12px; }}
.title {{ text-align: center; margin: 16px 0; }}
.title h3 {{ margin: 0; text-decoration: underline; font-size: 18px; }}
table.peserta {{ width: 100%; border-collapse: collapse; font-size: 13px; margin: 10px 0 16px; }}
table.peserta th, table.peserta td {{ border: 1px solid #334155; padding: 4px 6px; text-align: left; }}
.meta td {{ padding: 2px 8px; vertical-align: top; font-size: 14px; }}
.footer {{ margin-top: 30px; }}
.sign {{ float: right; width: 280px; text-align: center; font-size: 14px; }}
.verify {{ float: left; width: 240px; font-size: 11px; }}
</style>
</head>
<body>
<div class='header'><h2>{E(letterhead.Name)}</h2><p>{E(letterhead.Address)} {E(letterhead.Contact)}</p></div>
<div class='title'><h3>SURAT TUGAS</h3><p>Nomor: {E(letter.LetterNumber)}</p></div>
{basis}
<p>Yang bertanda tangan di bawah ini menugaskan:</p>
<table class='peserta'><thead><tr><th>No</th><th>Nama</th><th>NIP/Kode</th><th>Jabatan</th><th>Unit</th><th>Peran</th></tr></thead><tbody>{rows}</tbody></table>
<table class='meta'>
<tr><td>Kegiatan</td><td>: {E(letter.ActivityName)}</td></tr>
<tr><td>Untuk</td><td>: {E(letter.TaskDescription)}</td></tr>
<tr><td>Tujuan / Lokasi</td><td>: {E(letter.Destination)}</td></tr>
<tr><td>Waktu</td><td>: {E(Local(letter.StartAt))} s.d. {E(Local(letter.EndAt))} WIB</td></tr>
</table>
<p>Demikian Surat Tugas ini dibuat untuk dilaksanakan dengan penuh tanggung jawab dan dilaporkan hasilnya setelah selesai.</p>
<div class='footer'>
<div class='verify'><img src='{qrDataUri}' alt='QR verifikasi' width='120' height='120'/><p>Verifikasi keaslian: {E(verificationUrl)}</p></div>
<div class='sign'><p>{city}{E(issuedLocal)}<br/><strong>{E(letter.SignerTitle)}</strong></p><div style='height: 50px;'></div><p style='text-decoration: underline; font-weight: bold; margin: 0;'>{E(letter.SignerName)}</p><p style='margin: 0; font-size: 12px;'>[Disahkan secara elektronik]</p></div>
<div style='clear: both;'></div>
</div>
</body>
</html>";
        }

        private static void RequireText(string? value, string message)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(message);
            }
        }

        private static string Truncate(string value, int max) => value.Length <= max ? value : value[..max];

        private static TimeZoneInfo ResolveBusinessTimeZone()
        {
            foreach (var id in new[] { "Asia/Jakarta", "SE Asia Standard Time" })
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return TimeZoneInfo.CreateCustomTimeZone("WIB", TimeSpan.FromHours(7), "WIB", "WIB");
        }
    }
}
