using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    public class ShiftReplacementService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHrdEmploymentReadinessReadService _readinessService;
        private readonly ILogger<ShiftReplacementService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly HrdUnitScopeService _unitScopeService;
        private readonly IRosterValidationService _rosterValidationService;
        private readonly ShiftRevisionRecorder _revisionRecorder;
        private readonly IUnifiedNotificationGatewayService _notificationGateway;
        private readonly ISchedulingSettingService _settingService;

        // VR-PK-03, VR-PK-04, VR-PK-05, VR-PK-06 (kontrak v26).
        private const string ManageScopeDeniedMessage = "Anda tidak berwenang mengubah roster unit ini.";
        private const string ReadinessUnreadableMessage = "Status kesiapan pegawai tidak dapat dibaca. Coba lagi atau hubungi HRD.";
        private const string WarningReasonRequiredMessage = "Pilih alasan untuk setiap peringatan sebelum menyimpan.";
        private const string ShiftChangedByOthersMessage = "Shift ini baru saja diubah orang lain. Muat ulang lalu coba lagi.";

        public ShiftReplacementService(
            ApplicationDbContext context,
            IHrdEmploymentReadinessReadService readinessService,
            ILogger<ShiftReplacementService> logger,
            IServiceProvider serviceProvider,
            HrdUnitScopeService unitScopeService,
            IRosterValidationService rosterValidationService,
            ShiftRevisionRecorder revisionRecorder,
            IUnifiedNotificationGatewayService notificationGateway,
            ISchedulingSettingService settingService)
        {
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            _rosterValidationService = rosterValidationService ?? throw new ArgumentNullException(nameof(rosterValidationService));
            _revisionRecorder = revisionRecorder ?? throw new ArgumentNullException(nameof(revisionRecorder));
            _notificationGateway = notificationGateway ?? throw new ArgumentNullException(nameof(notificationGateway));
            _unitScopeService = unitScopeService ?? throw new ArgumentNullException(nameof(unitScopeService));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _readinessService = readinessService ?? throw new ArgumentNullException(nameof(readinessService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<PagedResult<ShiftReplacementResponse>> GetPagedAsync(
            int pageNumber = 1,
            int pageSize = 20,
            Guid? shiftAssignmentId = null,
            string? status = null,
            CancellationToken cancellationToken = default)
        {
            var query = _context.TrxShiftReplacements
                .AsNoTracking()
                .Include(r => r.ShiftAssignment)
                    .ThenInclude(s => s!.Shift)
                .Include(r => r.OriginalWorkforceProfile)
                .Include(r => r.ReplacementWorkforceProfile)
                .Where(r => r.IsActive)
                .AsQueryable();

            // Unit sebuah shift = unit periode roster-nya, atau unit shift itu sendiri bila periode tidak
            // bertautan unit. Shift tanpa keduanya hanya terbaca oleh cakupan semua unit.
            var readScope = await _unitScopeService.GetReadScopeAsync(cancellationToken);
            if (!readScope.IsAllUnits)
            {
                var unitIds = readScope.UnitIds.Select(x => (Guid?)x).ToList();
                query = query.Where(r =>
                    r.ShiftAssignment != null &&
                    unitIds.Contains(
                        r.ShiftAssignment.RosterAssignment!.RosterPeriod!.OrganizationUnitId ??
                        r.ShiftAssignment.OrganizationUnitId));
            }

            if (shiftAssignmentId.HasValue && shiftAssignmentId.Value != Guid.Empty)
            {
                query = query.Where(r => r.ShiftAssignmentId == shiftAssignmentId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(r => r.ReplacementStatus == status);
            }

            var totalData = await query.CountAsync(cancellationToken);
            var items = await query
                .OrderByDescending(r => r.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ShiftReplacementResponse
                {
                    Id = r.Id,
                    ShiftAssignmentId = r.ShiftAssignmentId,
                    ShiftDate = r.ShiftAssignment != null ? r.ShiftAssignment.ShiftDate : default,
                    ShiftName = r.ShiftAssignment != null && r.ShiftAssignment.Shift != null ? r.ShiftAssignment.Shift.ShiftName : string.Empty,
                    OriginalWorkforceProfileId = r.OriginalWorkforceProfileId,
                    OriginalWorkforceDisplayName = r.OriginalWorkforceProfile != null ? r.OriginalWorkforceProfile.DisplayName : string.Empty,
                    ReplacementWorkforceProfileId = r.ReplacementWorkforceProfileId,
                    ReplacementWorkforceDisplayName = r.ReplacementWorkforceProfile != null ? r.ReplacementWorkforceProfile.DisplayName : string.Empty,
                    ReplacementType = r.ReplacementType,
                    Reason = r.Reason,
                    ReplacementStatus = r.ReplacementStatus,
                    CreateDateTime = r.CreateDateTime,
                    ApprovedAt = r.ApprovedAt,
                    AppliedAt = r.AppliedAt
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<ShiftReplacementResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        public async Task<SchedulingRequestServiceResult<ShiftReplacementResponse>> CreateReplacementAsync(
            CreateShiftReplacementRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var shiftAssignment = await _context.TrxShiftAssignments
                .Include(s => s.Shift)
                .Include(s => s.RosterAssignment)
                    .ThenInclude(a => a!.RosterPeriod)
                .FirstOrDefaultAsync(s => s.Id == req.ShiftAssignmentId && s.IsActive, cancellationToken);

            if (shiftAssignment == null)
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Penugasan shift dengan ID '{req.ShiftAssignmentId}' tidak ditemukan.");
            }

            var period = shiftAssignment.RosterAssignment?.RosterPeriod;

            if (!await CanManageUnitAsync(period?.OrganizationUnitId ?? shiftAssignment.OrganizationUnitId, cancellationToken))
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ManageScopeDeniedMessage);
            }

            if (period != null && (period.IsLocked || period.RosterStatus == "Closed"))
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Periode roster telah ditutup dan dikunci mutlak dari seluruh penggantian shift (HRD-DEC-102).");
            }

            var origStaff = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == req.OriginalWorkforceProfileId && w.IsActive, cancellationToken);

            if (origStaff == null)
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Pegawai asal dengan ID '{req.OriginalWorkforceProfileId}' tidak ditemukan.");
            }

            var replStaff = await _context.MstWorkforceProfiles
                .FirstOrDefaultAsync(w => w.Id == req.ReplacementWorkforceProfileId && w.IsActive, cancellationToken);

            if (replStaff == null)
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Pegawai pengganti dengan ID '{req.ReplacementWorkforceProfileId}' tidak ditemukan.");
            }

            var replacement = new TrxShiftReplacement
            {
                Id = Guid.NewGuid(),
                ShiftAssignmentId = req.ShiftAssignmentId,
                RosterPeriodId = period?.Id,
                OriginalWorkforceProfileId = req.OriginalWorkforceProfileId,
                ReplacementWorkforceProfileId = req.ReplacementWorkforceProfileId,
                ReplacementType = req.ReplacementType.Trim(),
                ReasonCategory = req.ReplacementType.Trim(),
                Reason = req.Reason.Trim(),
                Notes = req.Reason.Trim(),
                ReplacementStatus = "Draft",
                IsActive = true,
                CreateBy = currentUserId,
                CreateDateTime = DateTime.UtcNow
            };

            _context.TrxShiftReplacements.Add(replacement);
            await _context.SaveChangesAsync(cancellationToken);

            var response = new ShiftReplacementResponse
            {
                Id = replacement.Id,
                ShiftAssignmentId = replacement.ShiftAssignmentId,
                ShiftDate = shiftAssignment.ShiftDate,
                ShiftName = shiftAssignment.Shift?.ShiftName ?? string.Empty,
                OriginalWorkforceProfileId = replacement.OriginalWorkforceProfileId,
                OriginalWorkforceDisplayName = origStaff.DisplayName,
                ReplacementWorkforceProfileId = replacement.ReplacementWorkforceProfileId,
                ReplacementWorkforceDisplayName = replStaff.DisplayName,
                ReplacementType = replacement.ReplacementType,
                Reason = replacement.Reason,
                ReplacementStatus = replacement.ReplacementStatus,
                CreateDateTime = replacement.CreateDateTime
            };

            return SchedulingRequestServiceResult<ShiftReplacementResponse>.Ok(
                response,
                "Penggantian shift operasional darurat berhasil dicatat.",
                StatusCodes.Status201Created);
        }

        public async Task<SchedulingRequestServiceResult<ShiftReplacementResponse>> ApproveReplacementAsync(
            Guid id,
            Guid currentUserId,
            string? notes,
            CancellationToken cancellationToken = default)
        {
            var replacement = await _context.TrxShiftReplacements
                .Include(r => r.ShiftAssignment)
                    .ThenInclude(s => s!.Shift)
                .Include(r => r.ShiftAssignment)
                    .ThenInclude(s => s!.RosterAssignment)
                        .ThenInclude(a => a!.RosterPeriod)
                .Include(r => r.OriginalWorkforceProfile)
                .Include(r => r.ReplacementWorkforceProfile)
                .FirstOrDefaultAsync(r => r.Id == id && r.IsActive, cancellationToken);

            if (replacement == null)
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Permintaan penggantian shift dengan ID '{id}' tidak ditemukan.");
            }

            var period = replacement.ShiftAssignment?.RosterAssignment?.RosterPeriod;

            if (!await CanManageUnitAsync(period?.OrganizationUnitId ?? replacement.ShiftAssignment?.OrganizationUnitId, cancellationToken))
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ManageScopeDeniedMessage);
            }

            if (period != null && (period.IsLocked || period.RosterStatus == "Closed"))
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Periode roster telah ditutup dan dikunci mutlak dari seluruh penggantian shift (HRD-DEC-102).");
            }

            if (replacement.ReplacementStatus == "Applied" || replacement.ReplacementStatus == "Approved")
            {
                return SchedulingRequestServiceResult<ShiftReplacementResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Permintaan penggantian shift ini sudah disetujui sebelumnya.");
            }

            replacement.ReplacementStatus = "Applied";
            replacement.ApprovedAt = DateTime.UtcNow;
            replacement.ApprovedByUserId = currentUserId;
            replacement.AppliedAt = DateTime.UtcNow;
            replacement.ExecutedAt = DateTime.UtcNow;
            replacement.ExecutedByUserId = currentUserId;
            replacement.ApprovalNotes = notes?.Trim();

            // Terapkan perubahan ke TrxShiftAssignment
            if (replacement.ShiftAssignment != null)
            {
                replacement.ShiftAssignment.OriginalEmployeeId ??= replacement.ShiftAssignment.WorkforceProfileId;
                replacement.ShiftAssignment.WorkforceProfileId = replacement.ReplacementWorkforceProfileId;
                replacement.ShiftAssignment.AssignmentSource = replacement.ReplacementType;
                replacement.ShiftAssignment.IsManualOverride = true;
                replacement.ShiftAssignment.OverrideReason = $"Penggantian shift ({replacement.ReplacementType}): {replacement.Reason}";
            }

            await _context.SaveChangesAsync(cancellationToken);

            var response = new ShiftReplacementResponse
            {
                Id = replacement.Id,
                ShiftAssignmentId = replacement.ShiftAssignmentId,
                ShiftDate = replacement.ShiftAssignment != null ? replacement.ShiftAssignment.ShiftDate : default,
                ShiftName = replacement.ShiftAssignment?.Shift?.ShiftName ?? string.Empty,
                OriginalWorkforceProfileId = replacement.OriginalWorkforceProfileId,
                OriginalWorkforceDisplayName = replacement.OriginalWorkforceProfile?.DisplayName ?? string.Empty,
                ReplacementWorkforceProfileId = replacement.ReplacementWorkforceProfileId,
                ReplacementWorkforceDisplayName = replacement.ReplacementWorkforceProfile?.DisplayName ?? string.Empty,
                ReplacementType = replacement.ReplacementType,
                Reason = replacement.Reason,
                ReplacementStatus = replacement.ReplacementStatus,
                CreateDateTime = replacement.CreateDateTime,
                ApprovedAt = replacement.ApprovedAt,
                AppliedAt = replacement.AppliedAt
            };

            return SchedulingRequestServiceResult<ShiftReplacementResponse>.Ok(
                response,
                "Penggantian shift berhasil disetujui dan diterapkan langsung ke jadwal operasional.");
        }

        /// <summary>
        /// Ganti Petugas Langsung Pasca-Terbit oleh Kepala Unit (BE-HRD-095, HRD-DEC-101).
        /// Periode roster tetap Published, pengganti divalidasi keselamatannya, langsung efektif.
        /// </summary>
        public async Task<SchedulingRequestServiceResult<DirectShiftReplacementResponse>> ExecuteDirectReplacementAsync(
            Guid shiftAssignmentId,
            DirectShiftReplacementRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var shiftAssignment = await _context.TrxShiftAssignments
                .Include(s => s.Shift)
                .Include(s => s.WorkforceProfile)
                .Include(s => s.RosterAssignment)
                    .ThenInclude(a => a!.RosterPeriod)
                .FirstOrDefaultAsync(s => s.Id == shiftAssignmentId && s.IsActive, cancellationToken);

            if (shiftAssignment == null)
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Penugasan shift dengan ID '{shiftAssignmentId}' tidak ditemukan.");
            }

            var period = shiftAssignment.RosterAssignment?.RosterPeriod;

            if (!await CanManageUnitAsync(period?.OrganizationUnitId ?? shiftAssignment.OrganizationUnitId, cancellationToken))
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ManageScopeDeniedMessage);
            }

            if (period != null && (period.IsLocked || period.RosterStatus == "Closed"))
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Periode roster telah ditutup dan dikunci mutlak dari seluruh penggantian shift (HRD-DEC-102).");
            }

            if (period == null)
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Periode roster untuk shift ini tidak ditemukan.");
            }

            // VR-PK-07: alasan wajib (Riwayat Revisi tidak boleh tanpa alasan).
            var reasonCategory = req.ReasonCategory?.Trim() ?? string.Empty;
            var reasonNote = req.Notes?.Trim() ?? string.Empty;
            if (reasonCategory.Length == 0 || reasonNote.Length == 0)
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan perubahan wajib diisi.");
            }

            if (reasonCategory.Length > ShiftRevisionRecorder.ReasonCategoryMaxLength)
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Kategori alasan maksimal {ShiftRevisionRecorder.ReasonCategoryMaxLength} karakter.");
            }

            // VR-PK-05: petugas sudah diubah orang lain sejak layar dibuka.
            var previousWorkforceProfileId = shiftAssignment.WorkforceProfileId;
            if (req.ExpectedWorkforceProfileId.HasValue && req.ExpectedWorkforceProfileId.Value != previousWorkforceProfileId)
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    ShiftChangedByOthersMessage);
            }

            // Staf pengganti tidak boleh sama dengan staf asal yang sedang ditugaskan
            if (previousWorkforceProfileId == req.ReplacementEmployeeId)
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Pegawai pengganti tidak boleh sama dengan pegawai yang sedang bertugas.");
            }

            var replStaff = await _context.MstWorkforceProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Id == req.ReplacementEmployeeId && w.IsActive, cancellationToken);

            if (replStaff == null)
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Pegawai pengganti dengan ID '{req.ReplacementEmployeeId}' tidak ditemukan.");
            }

            // Pemeriksaan penuh B1–B8 dan W1–W7 dengan evaluator yang sama dengan terbit (INV-A52-03).
            // B6 (kesiapan tidak terbaca) ikut sebagai blokir, sehingga gagal tertutup.
            var check = await _rosterValidationService.ValidateSingleShiftAsync(new RosterSingleShiftValidationRequest
            {
                RosterPeriodId = period.Id,
                ShiftAssignmentId = shiftAssignment.Id,
                WorkforceProfileId = req.ReplacementEmployeeId,
                ShiftId = shiftAssignment.ShiftId,
                ShiftDate = shiftAssignment.ShiftDate,
                ScheduledStartAt = shiftAssignment.ScheduledStartAt,
                ScheduledEndAt = shiftAssignment.ScheduledEndAt,
                IsNightShift = shiftAssignment.IsNightShift,
                IsDayOff = shiftAssignment.IsDayOff,
            }, cancellationToken);

            if (check == null)
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Periode roster untuk shift ini tidak ditemukan.");
            }

            var blocking = check.Items.Where(x => x.Severity == "BLOCKING").ToList();
            if (blocking.Count > 0)
            {
                var message = blocking.Any(x => x.RuleCode == "B6")
                    ? ReadinessUnreadableMessage
                    : $"Ganti Petugas ditolak: {string.Join(" ", blocking.Select(x => x.Message).Distinct())}";
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    message,
                    check.Items);
            }

            // VR-PK-04: setiap kode peringatan wajib punya alasan.
            var warnings = check.Items.Where(x => x.Severity == "WARNING").ToList();
            var reasonedCodes = (req.WarningReasons ?? new List<WarningReasonItemRequest>())
                .Where(x => !string.IsNullOrWhiteSpace(x.WarningCode) && !string.IsNullOrWhiteSpace(x.ReasonCategory))
                .Select(x => x.WarningCode.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            if (warnings.Any(w => !reasonedCodes.Contains(w.RuleCode)))
            {
                return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    WarningReasonRequiredMessage,
                    warnings);
            }

            var originalStaffName = shiftAssignment.WorkforceProfile?.DisplayName ?? "Staf Asal";
            var now = DateTime.UtcNow;

            // Petugas pertama shift tetap disimpan di OriginalEmployeeId; riwayat mencatat petugas tepat sebelum
            // perubahan ini (35.9.3: Budi → Tono → Andi menulis Tono pada baris kedua).
            var firstEmployeeId = shiftAssignment.OriginalEmployeeId ?? previousWorkforceProfileId;
            var overrideReason = $"[{reasonCategory}] {reasonNote}";

            HrdShiftAssignmentRevision revision;
            await using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                // Pembaruan bersyarat: hanya berhasil bila petugas di basis data masih sama dengan yang dibaca.
                // Dua Ganti Petugas serentak pada shift yang sama: satu berhasil, satu 409 (AT-HRD-PK-04).
                var updated = await _context.TrxShiftAssignments
                    .Where(s => s.Id == shiftAssignment.Id && s.IsActive && s.WorkforceProfileId == previousWorkforceProfileId)
                    .ExecuteUpdateAsync(set => set
                        .SetProperty(s => s.WorkforceProfileId, req.ReplacementEmployeeId)
                        .SetProperty(s => s.OriginalEmployeeId, firstEmployeeId)
                        .SetProperty(s => s.IsManualOverride, true)
                        .SetProperty(s => s.OverrideReason, overrideReason)
                        .SetProperty(s => s.AssignmentSource, "DirectReplacement")
                        .SetProperty(s => s.UpdateBy, currentUserId)
                        .SetProperty(s => s.UpdateDateTime, now),
                        cancellationToken);

                if (updated == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        ShiftChangedByOthersMessage);
                }

                var replacementRecord = new TrxShiftReplacement
                {
                    Id = Guid.NewGuid(),
                    ShiftAssignmentId = shiftAssignment.Id,
                    RosterPeriodId = period.Id,
                    OriginalWorkforceProfileId = previousWorkforceProfileId,
                    ReplacementWorkforceProfileId = req.ReplacementEmployeeId,
                    ReplacementType = "DirectReplacement",
                    ReasonCategory = reasonCategory,
                    Reason = reasonNote,
                    Notes = reasonNote,
                    ReplacementStatus = "Applied",
                    ExecutedByUserId = currentUserId,
                    ExecutedAt = now,
                    AppliedAt = now,
                    ApprovedAt = now,
                    ApprovedByUserId = currentUserId,
                    CreateBy = currentUserId,
                    CreateDateTime = now,
                    IsActive = true
                };
                _context.TrxShiftReplacements.Add(replacementRecord);

                revision = _revisionRecorder.RecordReplacement(
                    shiftAssignment,
                    period.Id,
                    previousWorkforceProfileId,
                    req.ReplacementEmployeeId,
                    reasonCategory,
                    reasonNote,
                    replacementRecord.Id,
                    currentUserId,
                    now);

                // Shift yang dimuat tidak ikut disimpan lagi; perubahannya sudah ditulis lewat pembaruan bersyarat.
                _context.Entry(shiftAssignment).State = EntityState.Unchanged;

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            // Pemberitahuan sesudah commit; kegagalannya dicatat dan tidak membatalkan Ganti Petugas.
            var notifications = new List<string>();
            var shiftLabel = $"{shiftAssignment.Shift?.ShiftName ?? "Shift"} {shiftAssignment.ShiftDate:dd-MM-yyyy}";
            foreach (var (profileId, recipientName, title, body) in new[]
            {
                (previousWorkforceProfileId, originalStaffName, "Jadwal Anda diganti",
                    $"{shiftLabel}: tugas Anda dialihkan ke {replStaff.DisplayName}."),
                (req.ReplacementEmployeeId, replStaff.DisplayName, "Tugas shift baru",
                    $"{shiftLabel}: Anda ditugaskan menggantikan {originalStaffName}."),
            })
            {
                try
                {
                    var userIds = await _context.Users
                        .AsNoTracking()
                        .Where(u => u.WorkforceProfileId == profileId && u.IsActive)
                        .Select(u => u.Id)
                        .ToListAsync(cancellationToken);

                    if (userIds.Count == 0)
                    {
                        notifications.Add($"{recipientName} belum punya akun aktif; pemberitahuan tidak dikirim.");
                        continue;
                    }

                    foreach (var userId in userIds)
                    {
                        await _notificationGateway.SendInstantNotificationToUserAsync(
                            userId, title, body, dedupKey: $"shift-replacement:{revision.Id}:{userId}");
                    }

                    notifications.Add($"Pemberitahuan terkirim ke {recipientName}.");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Pemberitahuan Ganti Petugas untuk shift {ShiftId} ke profil {ProfileId} gagal; perubahan tetap berlaku.",
                        shiftAssignment.Id, profileId);
                    notifications.Add($"Pemberitahuan ke {recipientName} gagal dikirim.");
                }
            }

            var response = new DirectShiftReplacementResponse
            {
                ShiftAssignmentId = shiftAssignment.Id,
                RosterPeriodStatus = period.RosterStatus,
                IsManualOverride = true,
                AssignedToEmployeeId = replStaff.Id,
                AssignedToEmployeeName = replStaff.DisplayName,
                OriginalEmployeeId = previousWorkforceProfileId,
                OriginalEmployeeName = originalStaffName,
                EffectiveImmediately = true,
                NotificationsDispatched = notifications,
                RevisionId = revision.Id,
                Warnings = warnings,
                InfoMessages = check.InfoMessages
            };

            _logger.LogInformation("Ganti petugas langsung berhasil untuk shift {ShiftId} pada {Date}: {Orig} digantikan oleh {Repl}",
                shiftAssignment.Id, shiftAssignment.ShiftDate, originalStaffName, replStaff.DisplayName);

            return SchedulingRequestServiceResult<DirectShiftReplacementResponse>.Ok(
                response,
                $"Ganti petugas langsung berhasil. Shift dinas resmi dialihkan ke {replStaff.DisplayName}.");
        }

        /// <summary>
        /// Koreksi mundur jadwal dinas (<= 3 hari kalender) & rekalkulasi kehadiran (BE-HRD-097, HRD-DEC-102, INT-HR-36).
        /// </summary>
        public async Task<SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>> RetroactiveCorrectionAsync(
            Guid shiftAssignmentId,
            RetroactiveShiftCorrectionRequest req,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var shiftAssignment = await _context.TrxShiftAssignments
                .Include(s => s.Shift)
                .Include(s => s.WorkforceProfile)
                .Include(s => s.RosterAssignment)
                    .ThenInclude(a => a!.RosterPeriod)
                .FirstOrDefaultAsync(s => s.Id == shiftAssignmentId && s.IsActive, cancellationToken);

            if (shiftAssignment == null)
            {
                return SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Penugasan shift dengan ID '{shiftAssignmentId}' tidak ditemukan.");
            }

            var period = shiftAssignment.RosterAssignment?.RosterPeriod;

            if (!await CanManageUnitAsync(period?.OrganizationUnitId ?? shiftAssignment.OrganizationUnitId, cancellationToken))
            {
                return SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    ManageScopeDeniedMessage);
            }

            if (period != null && (period.IsLocked || period.RosterStatus == "Closed"))
            {
                return SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Periode roster telah ditutup dan dikunci mutlak dari seluruh koreksi (HRD-DEC-102).");
            }

            if (period == null)
            {
                return SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Periode roster untuk shift ini tidak ditemukan.");
            }

            // VR-PK-07: alasan wajib; kategori harus muat di kolom Riwayat Revisi.
            var reasonCategory = req.ReasonCategory?.Trim() ?? string.Empty;
            var reasonNote = req.Notes?.Trim() ?? string.Empty;
            if (reasonCategory.Length == 0 || reasonNote.Length == 0)
            {
                return SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan perubahan wajib diisi.");
            }

            if (reasonCategory.Length > ShiftRevisionRecorder.ReasonCategoryMaxLength)
            {
                return SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Kategori alasan maksimal {ShiftRevisionRecorder.ReasonCategoryMaxLength} karakter.");
            }

            // HRD-DEC-102 (tetap berlaku menurut HRD-DEC-217 butir 4): batas hari dibaca dari setelan penjadwalan,
            // dihitung terhadap tanggal operasional WIB, bukan tanggal UTC.
            var settingResult = await _settingService.GetSettingAsync(period.HospitalSiteId ?? Guid.Empty, cancellationToken);
            var windowDays = (settingResult.Data ?? new SchedulingSettingResponse()).RetroactiveCorrectionWindowDays;
            var today = DateOnly.FromDateTime(AppDateTimeHelper.OperationalDate());
            var daysBack = today.DayNumber - shiftAssignment.ShiftDate.DayNumber;

            if (daysBack > windowDays)
            {
                return SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Koreksi mundur jadwal pada tanggal {shiftAssignment.ShiftDate:yyyy-MM-dd} ({daysBack} hari yang lalu) ditolak. Batas toleransi koreksi mundur mandiri Kepala Unit adalah maksimal {windowDays} hari kalender ke belakang (VR-SCH-17 / HRD-DEC-102). Silakan hubungi Tim HRD.");
            }

            var newShift = await _context.MstShifts
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == req.NewShiftId && s.IsActive, cancellationToken);

            if (newShift == null)
            {
                return SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Master shift tujuan dengan ID '{req.NewShiftId}' tidak ditemukan.");
            }

            // Jam shift adalah jam dinding WIB; disimpan sebagai UTC seperti saat shift ditambahkan ke roster.
            var newStart = AppDateTimeHelper.ToUtc(shiftAssignment.ShiftDate.ToDateTime(newShift.StartTime));
            var newEnd = AppDateTimeHelper.ToUtc(newShift.IsOvernight || newShift.EndTime < newShift.StartTime
                ? shiftAssignment.ShiftDate.AddDays(1).ToDateTime(newShift.EndTime)
                : shiftAssignment.ShiftDate.ToDateTime(newShift.EndTime));

            var paidMinutes = newShift.PaidWorkMinutes > 0
                ? newShift.PaidWorkMinutes
                : (int)(newEnd - newStart).TotalMinutes - newShift.BreakDurationMinutes;
            if (paidMinutes < 0) paidMinutes = 0;

            // Nilai sebelum koreksi, untuk pembaruan bersyarat dan Riwayat Revisi (HRD-DEC-217 butir 2).
            var workforceProfileId = shiftAssignment.WorkforceProfileId;
            var oldShiftId = shiftAssignment.ShiftId;
            var oldStart = shiftAssignment.ScheduledStartAt;
            var oldEnd = shiftAssignment.ScheduledEndAt;
            var overrideReason = $"[KOREKSI MUNDUR - {reasonCategory}] {reasonNote}";
            var now = DateTime.UtcNow;

            HrdShiftAssignmentRevision revision;
            await using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                // Pembaruan bersyarat: hanya berhasil bila shift di basis data masih sama dengan yang dibaca, sehingga
                // perubahan lain yang terjadi di antaranya tidak tertimpa diam-diam dan riwayat memuat nilai lama yang benar.
                var updated = await _context.TrxShiftAssignments
                    .Where(s =>
                        s.Id == shiftAssignment.Id &&
                        s.IsActive &&
                        s.WorkforceProfileId == workforceProfileId &&
                        s.ShiftId == oldShiftId &&
                        s.ScheduledStartAt == oldStart &&
                        s.ScheduledEndAt == oldEnd)
                    .ExecuteUpdateAsync(set => set
                        .SetProperty(s => s.ShiftId, newShift.Id)
                        .SetProperty(s => s.ScheduledStartAt, newStart)
                        .SetProperty(s => s.ScheduledEndAt, newEnd)
                        .SetProperty(s => s.PlannedWorkMinutes, paidMinutes)
                        .SetProperty(s => s.BreakDurationMinutes, newShift.BreakDurationMinutes)
                        .SetProperty(s => s.IsNightShift, newShift.IsNightShift)
                        .SetProperty(s => s.IsOnCall, newShift.IsOnCall)
                        .SetProperty(s => s.IsDayOff, newShift.IsOffShift)
                        .SetProperty(s => s.IsManualOverride, true)
                        .SetProperty(s => s.OverrideReason, overrideReason)
                        .SetProperty(s => s.AssignmentSource, "RetroactiveCorrection")
                        .SetProperty(s => s.UpdateBy, currentUserId)
                        .SetProperty(s => s.UpdateDateTime, now),
                        cancellationToken);

                if (updated == 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        ShiftChangedByOthersMessage);
                }

                revision = _revisionRecorder.Record(new HrdShiftAssignmentRevision
                {
                    ShiftAssignmentId = shiftAssignment.Id,
                    RosterPeriodId = period.Id,
                    ShiftDate = shiftAssignment.ShiftDate,
                    ChangeType = SchedulingRequestValueConstants.ShiftRevisionChangeType.RetroactiveCorrection,
                    PreviousWorkforceProfileId = workforceProfileId,
                    NewWorkforceProfileId = workforceProfileId,
                    PreviousShiftId = oldShiftId,
                    NewShiftId = newShift.Id,
                    PreviousStartAt = oldStart,
                    PreviousEndAt = oldEnd,
                    NewStartAt = newStart,
                    NewEndAt = newEnd,
                    ReasonCategory = reasonCategory,
                    ReasonNote = reasonNote,
                    ChangedByUserId = currentUserId,
                    ChangedAt = now,
                });

                // Shift yang dimuat tidak ikut disimpan; perubahannya sudah ditulis lewat pembaruan bersyarat.
                _context.Entry(shiftAssignment).State = EntityState.Unchanged;

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            // Hitung ulang kehadiran sesudah commit (INT-HR-101). Kegagalannya tidak membatalkan koreksi;
            // rekap tanggal itu ditandai ReprocessRequired (02-backend-architecture.md 35.10).
            var reprocessed = false;
            var attendancePeriodClosed = false;
            string recalculationNote;
            try
            {
                var attendanceConsumer = _serviceProvider.GetService<AttendanceScheduleMutationConsumer>();
                if (attendanceConsumer == null)
                {
                    recalculationNote = "Layanan hitung ulang kehadiran tidak tersedia.";
                }
                else
                {
                    var result = await attendanceConsumer.ConsumeRetroactiveShiftCorrectionAsync(new RetroactiveShiftCorrectedPayload
                    {
                        ShiftAssignmentId = shiftAssignment.Id,
                        WorkforceProfileId = workforceProfileId,
                        ShiftDate = shiftAssignment.ShiftDate,
                        OldShiftId = oldShiftId,
                        NewShiftId = newShift.Id,
                        TriggeredByUserId = currentUserId,
                        Reason = reasonCategory
                    }, cancellationToken);

                    attendancePeriodClosed = result.IsPeriodClosed;
                    recalculationNote = result.IsPeriodClosed
                        ? "Periode kehadiran tanggal ini sudah ditutup; kehadiran tidak dihitung ulang."
                        : result.Message;

                    // Hitung ulang yang berjalan tetapi berakhir Error (misalnya jadwal tidak terselesaikan) juga
                    // kegagalan; Error → ReprocessRequired adalah transisi sistem yang sah (state matrix 30.x).
                    reprocessed = result.Success && !await _context.HrdAttendanceDailies
                        .AsNoTracking()
                        .AnyAsync(d =>
                            d.WorkforceProfileId == workforceProfileId &&
                            d.AttendanceDate == shiftAssignment.ShiftDate &&
                            !d.IsDelete &&
                            d.ProcessingStatus == AttendanceValueConstants.AttendanceProcessingStatus.Error,
                            cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hitung ulang kehadiran sesudah koreksi mundur shift {ShiftId} tanggal {ShiftDate} gagal.",
                    shiftAssignment.Id, shiftAssignment.ShiftDate);
                recalculationNote = "Hitung ulang kehadiran gagal.";
            }

            var markedForReprocess = false;
            if (!reprocessed && !attendancePeriodClosed)
            {
                markedForReprocess = await MarkAttendanceReprocessRequiredAsync(
                    workforceProfileId, shiftAssignment.ShiftDate, currentUserId, cancellationToken);
            }

            _logger.LogInformation(
                "Koreksi mundur shift {ShiftId} tanggal {ShiftDate} ke {NewShift}; riwayat {RevisionId}; hitung ulang {Reprocessed}; ditandai proses ulang {Marked}.",
                shiftAssignment.Id, shiftAssignment.ShiftDate, newShift.ShiftName, revision.Id, reprocessed, markedForReprocess);

            var message = reprocessed
                ? $"Jadwal shift tanggal {shiftAssignment.ShiftDate:yyyy-MM-dd} berhasil dikoreksi ke '{newShift.ShiftName}' dan kehadiran dihitung ulang."
                : markedForReprocess
                    ? $"Jadwal shift tanggal {shiftAssignment.ShiftDate:yyyy-MM-dd} berhasil dikoreksi ke '{newShift.ShiftName}'. Hitung ulang kehadiran belum berhasil; rekap tanggal itu ditandai perlu diproses ulang."
                    : $"Jadwal shift tanggal {shiftAssignment.ShiftDate:yyyy-MM-dd} berhasil dikoreksi ke '{newShift.ShiftName}'. {recalculationNote}";

            var response = new RetroactiveShiftCorrectionResponse
            {
                ShiftAssignmentId = shiftAssignment.Id,
                IsRetroactive = daysBack > 0,
                DaysBack = daysBack > 0 ? daysBack : 0,
                AttendanceReevaluationTriggered = reprocessed,
                RevisionId = revision.Id,
                Message = message
            };

            return SchedulingRequestServiceResult<RetroactiveShiftCorrectionResponse>.Ok(
                response,
                response.Message);
        }

        /// <summary>
        /// Menandai rekap harian pegawai pada tanggal itu ReprocessRequired. Konteks dibersihkan lebih dulu karena
        /// hitung ulang yang gagal dapat meninggalkan entitas setengah berubah di konteks yang sama.
        /// </summary>
        private async Task<bool> MarkAttendanceReprocessRequiredAsync(
            Guid workforceProfileId,
            DateOnly attendanceDate,
            Guid currentUserId,
            CancellationToken cancellationToken)
        {
            try
            {
                _context.ChangeTracker.Clear();
                var now = DateTime.UtcNow;
                var marked = await _context.HrdAttendanceDailies
                    .Where(d => d.WorkforceProfileId == workforceProfileId && d.AttendanceDate == attendanceDate && !d.IsDelete)
                    .ExecuteUpdateAsync(set => set
                        .SetProperty(d => d.ProcessingStatus, AttendanceValueConstants.AttendanceProcessingStatus.ReprocessRequired)
                        // Pesan pemrosesan sebelumnya (misalnya "Schedule belum terselesaikan") dipertahankan di belakang.
                        .SetProperty(d => d.ProcessingMessage, d =>
                            ("Jadwal dikoreksi mundur; hitung ulang otomatis gagal, perlu diproses ulang. " + (d.ProcessingMessage ?? string.Empty))
                                .Substring(0, 1000))
                        .SetProperty(d => d.UpdateBy, currentUserId)
                        .SetProperty(d => d.UpdateDateTime, now),
                        cancellationToken);
                return marked > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal menandai rekap kehadiran {WorkforceProfileId} tanggal {Date} perlu diproses ulang.",
                    workforceProfileId, attendanceDate);
                return false;
            }
        }

        // Shift tanpa unit (periode maupun shift) hanya boleh diubah oleh cakupan semua unit.
        private async Task<bool> CanManageUnitAsync(Guid? organizationUnitId, CancellationToken cancellationToken) =>
            (await _unitScopeService.GetManageScopeAsync(cancellationToken)).Contains(organizationUnitId);
    }
}
