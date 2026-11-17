using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    /// <summary>
    /// Dampak penempatan sementara terhadap roster terbit unit asal (HRD-DEC-213, HRD-ASM-82, INT-HR-104).
    ///
    /// Sesudah penempatan Temporary disimpan, shift pegawai pada roster Published unit lain dalam rentang
    /// penempatan dicari, lalu Kepala Unit pemilik roster itu (pemegang penempatan berjabatan yang berlaku
    /// hari ini) diberi tahu agar melakukan Ganti Petugas. Roster tidak diubah otomatis.
    ///
    /// Tidak pernah melempar galat ke pemanggil: penempatan sudah tersimpan, sehingga kegagalan apa pun di
    /// sini hanya dicatat di log.
    /// </summary>
    public class TemporaryPlacementRosterImpactService
    {
        public const string TemporaryAssignmentType = "Temporary";
        private const string PublishedStatus = "Published";
        private const int MaxListedDates = 10;

        private static readonly CultureInfo Indonesian = CultureInfo.GetCultureInfo("id-ID");

        private readonly ApplicationDbContext _context;
        private readonly IUnifiedNotificationGatewayService _notificationGateway;
        private readonly ILogger<TemporaryPlacementRosterImpactService> _logger;

        public TemporaryPlacementRosterImpactService(
            ApplicationDbContext context,
            IUnifiedNotificationGatewayService notificationGateway,
            ILogger<TemporaryPlacementRosterImpactService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _notificationGateway = notificationGateway ?? throw new ArgumentNullException(nameof(notificationGateway));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TemporaryPlacementRosterImpactResult> NotifyOriginUnitsAsync(
            Guid organizationAssignmentId,
            CancellationToken cancellationToken = default)
        {
            var result = new TemporaryPlacementRosterImpactResult();
            try
            {
                var placement = await _context.WfpOrganizationAssignments
                    .AsNoTracking()
                    .Where(a => a.Id == organizationAssignmentId)
                    .Select(a => new
                    {
                        a.Id,
                        a.WorkforceProfileId,
                        a.OrganizationUnitId,
                        UnitName = a.OrganizationUnit != null ? a.OrganizationUnit.UnitName : null,
                        a.AssignmentType,
                        a.EffectiveStartDate,
                        a.EffectiveEndDate,
                        a.IsActive,
                        a.IsDelete,
                        a.IsCancel
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (placement == null ||
                    !placement.IsActive || placement.IsDelete || placement.IsCancel ||
                    !placement.OrganizationUnitId.HasValue ||
                    !string.Equals(placement.AssignmentType, TemporaryAssignmentType, StringComparison.OrdinalIgnoreCase))
                {
                    return result;
                }

                // Tanggal efektif HR disimpan sebagai tanggal kalender pada tengah malam.
                var start = DateOnly.FromDateTime(placement.EffectiveStartDate);
                var end = placement.EffectiveEndDate.HasValue ? DateOnly.FromDateTime(placement.EffectiveEndDate.Value) : (DateOnly?)null;
                var targetUnitId = placement.OrganizationUnitId.Value;

                var shifts = await _context.TrxShiftAssignments
                    .AsNoTracking()
                    .Where(s =>
                        s.WorkforceProfileId == placement.WorkforceProfileId &&
                        s.IsActive &&
                        !s.IsCancel &&
                        !s.IsDayOff &&
                        s.ShiftDate >= start &&
                        (!end.HasValue || s.ShiftDate <= end.Value) &&
                        s.RosterAssignment != null &&
                        s.RosterAssignment.IsActive &&
                        s.RosterAssignment.RosterPeriod != null &&
                        s.RosterAssignment.RosterPeriod.IsActive &&
                        s.RosterAssignment.RosterPeriod.RosterStatus == PublishedStatus &&
                        s.RosterAssignment.RosterPeriod.OrganizationUnitId.HasValue &&
                        s.RosterAssignment.RosterPeriod.OrganizationUnitId != targetUnitId)
                    .Select(s => new
                    {
                        s.Id,
                        s.ShiftDate,
                        PeriodId = s.RosterAssignment!.RosterPeriodId,
                        PeriodName = s.RosterAssignment.RosterPeriod!.RosterPeriodName,
                        UnitId = s.RosterAssignment.RosterPeriod.OrganizationUnitId!.Value,
                        UnitName = s.RosterAssignment.RosterPeriod.OrganizationUnit != null ? s.RosterAssignment.RosterPeriod.OrganizationUnit.UnitName : null
                    })
                    .ToListAsync(cancellationToken);

                if (shifts.Count == 0)
                {
                    return result;
                }

                var staffName = await _context.MstWorkforceProfiles
                    .AsNoTracking()
                    .Where(w => w.Id == placement.WorkforceProfileId)
                    .Select(w => w.DisplayName)
                    .FirstOrDefaultAsync(cancellationToken) ?? "Pegawai";

                var today = DateTime.SpecifyKind(AppDateTimeHelper.OperationalDate().Date, DateTimeKind.Utc);
                var range = RosterValidationService.FormatRange(start, end);

                foreach (var period in shifts.GroupBy(s => new { s.PeriodId, s.PeriodName, s.UnitId, s.UnitName }))
                {
                    result.AffectedShiftCount += period.Count();

                    // Kepala Unit asal = pemegang penempatan berjabatan di unit roster yang berlaku hari ini (35.9.1).
                    var headProfileIds = await _context.WfpOrganizationAssignments
                        .AsNoTracking()
                        .Where(a =>
                            a.OrganizationUnitId == period.Key.UnitId &&
                            a.IsManagerialAssignment &&
                            a.IsActive &&
                            !a.IsDelete &&
                            !a.IsCancel &&
                            a.EffectiveStartDate <= today &&
                            (!a.EffectiveEndDate.HasValue || a.EffectiveEndDate.Value >= today))
                        .Select(a => a.WorkforceProfileId)
                        .Distinct()
                        .ToListAsync(cancellationToken);

                    var userIds = await _context.Users
                        .AsNoTracking()
                        .Where(u => u.IsActive && u.WorkforceProfileId.HasValue && headProfileIds.Contains(u.WorkforceProfileId.Value))
                        .Select(u => u.Id)
                        .ToListAsync(cancellationToken);

                    if (userIds.Count == 0)
                    {
                        result.UnitsWithoutRecipient++;
                        _logger.LogWarning(
                            "Penempatan sementara {PlacementId}: {Count} shift di roster terbit {PeriodId} terdampak, tetapi unit {UnitId} tidak punya Kepala Unit berakun; pemberitahuan tidak dikirim.",
                            placement.Id, period.Count(), period.Key.PeriodId, period.Key.UnitId);
                        continue;
                    }

                    var dates = period.Select(s => s.ShiftDate).Distinct().OrderBy(d => d).ToList();
                    var listed = string.Join(", ", dates.Take(MaxListedDates).Select(d => d.ToString("d MMM", Indonesian)));
                    if (dates.Count > MaxListedDates) listed += $", dan {dates.Count - MaxListedDates} tanggal lain";

                    var title = "Penempatan sementara memengaruhi roster terbit";
                    var message =
                        $"{staffName} ditempatkan di {placement.UnitName ?? "unit lain"} {range}, tetapi masih terjadwal " +
                        $"{period.Count()} shift di roster {period.Key.PeriodName} ({period.Key.UnitName ?? "unit Anda"}): {listed}. " +
                        "Lakukan Ganti Petugas; roster tidak diubah otomatis.";
                    var dedupBase = $"temporary-placement-impact:{placement.Id}:{period.Key.PeriodId}:{start:yyyyMMdd}-{end?.ToString("yyyyMMdd") ?? "open"}";

                    foreach (var userId in userIds)
                    {
                        try
                        {
                            await _notificationGateway.SendInstantNotificationToUserAsync(
                                userId, title, message, dedupKey: $"{dedupBase}:{userId}");
                            result.NotifiedUserIds.Add(userId);
                        }
                        catch (Exception ex)
                        {
                            result.FailedNotificationCount++;
                            _logger.LogWarning(ex,
                                "Pemberitahuan dampak penempatan sementara {PlacementId} ke pengguna {UserId} gagal; penempatan tetap tersimpan.",
                                placement.Id, userId);
                        }
                    }
                }

                _logger.LogInformation(
                    "Penempatan sementara {PlacementId}: {Shifts} shift terbit terdampak, {Notified} pemberitahuan terkirim, {Failed} gagal.",
                    placement.Id, result.AffectedShiftCount, result.NotifiedUserIds.Count, result.FailedNotificationCount);
            }
            catch (Exception ex)
            {
                result.Failed = true;
                _logger.LogError(ex, "Pemeriksaan dampak penempatan sementara {PlacementId} gagal; penempatan tetap tersimpan.", organizationAssignmentId);
            }

            return result;
        }
    }

    public sealed class TemporaryPlacementRosterImpactResult
    {
        public int AffectedShiftCount { get; set; }
        public List<Guid> NotifiedUserIds { get; } = new();
        public int FailedNotificationCount { get; set; }
        public int UnitsWithoutRecipient { get; set; }
        public bool Failed { get; set; }
    }
}
