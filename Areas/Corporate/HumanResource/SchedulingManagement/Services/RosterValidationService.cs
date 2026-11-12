using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Enums;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;
using RuleCode = QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants.SchedulingRequestValueConstants.RosterRuleCode;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services
{
    /// <summary>
    /// Mesin validasi keselamatan jadwal dinas rumah sakit (Roster Validation Engine).
    /// Mengevaluasi aturan memblokir B1–B8 dan peringatan kerja W1–W7.
    /// Mengakomodasi HRD-DEC-099, HRD-DEC-100, HRD-DEC-201, HRD-DEC-213, INT-HR-31, INT-HR-106, INT-HR-107,
    /// VR-SCH-01 s.d. VR-SCH-07, VR-SCH-80, VR-PK-01, VR-PK-02, VR-PK-04.
    ///
    /// Validasi satu periode (<see cref="ValidateRosterPeriodAsync"/>) dan validasi satu shift
    /// (<see cref="ValidateSingleShiftAsync"/>) memakai evaluator yang sama, sehingga Ganti Petugas dan
    /// tambah shift lolos pemeriksaan yang sama dengan terbit (INV-A52-03).
    /// </summary>
    public class RosterValidationService : IRosterValidationService
    {
        private const string Blocking = "BLOCKING";
        private const string Warning = "WARNING";
        private const string NoStaffingStandardInfo = "Kuota unit belum ditetapkan";

        private static readonly CultureInfo Indonesian = CultureInfo.GetCultureInfo("id-ID");

        private static readonly string[] PendingLeaveStatuses =
        {
            "Submitted", "WaitingSupervisorApproval", "WaitingManagerApproval", "WaitingHrVerification"
        };

        private readonly ApplicationDbContext _context;
        private readonly IHrdEmploymentReadinessReadService _readinessService;
        private readonly ISchedulingSettingService _settingService;
        private readonly ILogger<RosterValidationService> _logger;

        public RosterValidationService(
            ApplicationDbContext context,
            IHrdEmploymentReadinessReadService readinessService,
            ISchedulingSettingService settingService,
            ILogger<RosterValidationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _readinessService = readinessService ?? throw new ArgumentNullException(nameof(readinessService));
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<RosterValidationResultResponse> ValidateRosterPeriodAsync(
            Guid rosterPeriodId,
            Guid currentUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.TrxRosterPeriods
                .Include(p => p.RosterAssignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.ShiftAssignments.Where(s => s.IsActive))
                        .ThenInclude(s => s.Shift)
                .Include(p => p.RosterAssignments.Where(a => a.IsActive))
                    .ThenInclude(a => a.WorkforceProfile)
                .FirstOrDefaultAsync(p => p.Id == rosterPeriodId && p.IsActive, cancellationToken);

            if (period == null)
            {
                return new RosterValidationResultResponse
                {
                    RosterPeriodId = rosterPeriodId,
                    IsValidationPassed = false,
                    TotalConflicts = 1,
                    BlockingConflicts = 1,
                    ConflictMessages = new List<string> { $"Periode roster dengan ID '{rosterPeriodId}' tidak ditemukan." }
                };
            }

            var rules = await LoadRuleSettingsAsync(period.HospitalSiteId, cancellationToken);

            var checksByStaff = period.RosterAssignments
                .Select(assignment => assignment.ShiftAssignments
                    .OrderBy(s => s.ScheduledStartAt)
                    .Select(s => ShiftCheck.FromEntity(
                        s,
                        assignment.WorkforceProfile?.DisplayName ?? assignment.WorkforceProfileId.ToString(),
                        assignment.PositionId ?? assignment.WorkforceProfile?.PrimaryPositionId))
                    .ToList())
                .ToList();

            var allChecks = checksByStaff.SelectMany(x => x).ToList();
            var reference = await LoadReferenceDataAsync(
                period.OrganizationUnitId,
                allChecks.Select(x => x.WorkforceProfileId).Distinct().ToList(),
                period.PeriodStartDate,
                period.PeriodEndDate,
                cancellationToken);

            var sink = new ValidationSink();

            // 1. Evaluasi per pegawai
            foreach (var staffChecks in checksByStaff)
            {
                for (var i = 0; i < staffChecks.Count; i++)
                {
                    await EvaluateShiftAsync(
                        staffChecks[i],
                        staffChecks,
                        i > 0 ? staffChecks[i - 1] : null,
                        next: null,
                        rules,
                        reference,
                        sink,
                        cancellationToken);
                }

                EvaluateStreaks(staffChecks, focus: null, rules, sink);
            }

            // 2. Evaluasi komposisi per slot shift: B7, W6, W7
            foreach (var slot in allChecks.Where(s => !s.IsDayOff && s.ShiftId.HasValue).GroupBy(s => new { s.ShiftDate, s.ShiftId }))
            {
                EvaluateSlot(slot.ToList(), focus: null, reference, sink);
            }

            // 3. Salin hasil ke entitas shift dan periode
            foreach (var check in allChecks)
            {
                var entity = check.Entity!;
                entity.HasDoubleShiftConflict = check.HasDoubleShiftConflict;
                entity.HasLeaveConflict = check.HasLeaveConflict;
                entity.HasMinimumRestConflict = check.HasMinimumRestConflict;
                entity.HasWorkHourLimitConflict = check.HasWorkHourLimitConflict;
                entity.HasBlockingConflict = check.HasBlockingConflict;
                if (check.IsAccompanied)
                {
                    entity.IsAccompanied = true;
                }

                entity.IsValidationPassed = !check.HasBlockingConflict;
                entity.ValidatedAt = DateTime.UtcNow;
                entity.ValidatedByUserId = currentUserId;
            }

            period.TotalConflictCount = sink.TotalConflicts;
            period.BlockingConflictCount = sink.BlockingConflicts;
            period.WarningCount = sink.WarningConflicts;
            period.IsValidationPassed = sink.BlockingConflicts == 0;
            period.ValidatedAt = DateTime.UtcNow;
            period.ValidatedByUserId = currentUserId;

            var response = new RosterValidationResultResponse
            {
                RosterPeriodId = rosterPeriodId,
                IsValidationPassed = period.IsValidationPassed,
                TotalConflicts = sink.TotalConflicts,
                BlockingConflicts = sink.BlockingConflicts,
                WarningConflicts = sink.WarningConflicts,
                ConflictMessages = sink.ConflictMessages,
                WarningMessages = sink.WarningMessages,
                InfoMessages = reference.InfoMessages.ToList(),
                ConflictItems = sink.Items
            };

            period.ValidationSummaryJson = JsonSerializer.Serialize(response);
            await _context.SaveChangesAsync(cancellationToken);

            return response;
        }

        public async Task<RosterSingleShiftValidationResult?> ValidateSingleShiftAsync(
            RosterSingleShiftValidationRequest request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var period = await _context.TrxRosterPeriods
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == request.RosterPeriodId && p.IsActive, cancellationToken);

            if (period == null)
            {
                return null;
            }

            var excludedId = request.ShiftAssignmentId;

            var staff = await _context.MstWorkforceProfiles
                .AsNoTracking()
                .Where(w => w.Id == request.WorkforceProfileId)
                .Select(w => new { w.DisplayName, w.PrimaryPositionId })
                .FirstOrDefaultAsync(cancellationToken);

            var candidatePositionId = request.PositionId ?? await _context.TrxRosterAssignments
                .AsNoTracking()
                .Where(a => a.RosterPeriodId == period.Id && a.WorkforceProfileId == request.WorkforceProfileId && a.IsActive)
                .Select(a => a.PositionId)
                .FirstOrDefaultAsync(cancellationToken) ?? staff?.PrimaryPositionId;

            // Shift lain di periode ini: milik pegawai yang sama (B1, W1–W3) dan satu slot (B7, W6, W7).
            // Shift yang sedang diubah tidak ikut, karena kandidatlah yang menggantikannya.
            var relatedShifts = await _context.TrxShiftAssignments
                .AsNoTracking()
                .Include(s => s.Shift)
                .Include(s => s.RosterAssignment)
                    .ThenInclude(a => a!.WorkforceProfile)
                .Where(s =>
                    s.IsActive &&
                    (!excludedId.HasValue || s.Id != excludedId.Value) &&
                    s.RosterAssignment != null &&
                    s.RosterAssignment.RosterPeriodId == period.Id &&
                    s.RosterAssignment.IsActive &&
                    (s.WorkforceProfileId == request.WorkforceProfileId ||
                     (request.ShiftId.HasValue && s.ShiftId == request.ShiftId && s.ShiftDate == request.ShiftDate)))
                .ToListAsync(cancellationToken);

            var candidate = new ShiftCheck
            {
                ShiftAssignmentId = excludedId ?? Guid.Empty,
                WorkforceProfileId = request.WorkforceProfileId,
                StaffName = staff?.DisplayName ?? request.WorkforceProfileId.ToString(),
                PositionId = candidatePositionId,
                ShiftId = request.ShiftId,
                ShiftName = request.ShiftId.HasValue
                    ? await _context.MstShifts.AsNoTracking().Where(x => x.Id == request.ShiftId).Select(x => x.ShiftName).FirstOrDefaultAsync(cancellationToken)
                    : null,
                ShiftDate = request.ShiftDate,
                ScheduledStartAt = request.ScheduledStartAt,
                ScheduledEndAt = request.ScheduledEndAt,
                IsNightShift = request.IsNightShift,
                IsDayOff = request.IsDayOff,
            };

            var staffChecks = relatedShifts
                .Where(s => s.WorkforceProfileId == request.WorkforceProfileId)
                .Select(s => ShiftCheck.FromEntity(s, candidate.StaffName, candidatePositionId, track: false))
                .Append(candidate)
                .OrderBy(s => s.ScheduledStartAt)
                .ToList();

            var slotChecks = relatedShifts
                .Where(s =>
                    request.ShiftId.HasValue &&
                    s.ShiftId == request.ShiftId &&
                    s.ShiftDate == request.ShiftDate &&
                    !s.IsDayOff &&
                    s.WorkforceProfileId != request.WorkforceProfileId)
                .Select(s => ShiftCheck.FromEntity(
                    s,
                    s.RosterAssignment?.WorkforceProfile?.DisplayName ?? s.WorkforceProfileId.ToString(),
                    s.RosterAssignment?.PositionId ?? s.RosterAssignment?.WorkforceProfile?.PrimaryPositionId,
                    track: false))
                .Append(candidate)
                .ToList();

            var rules = await LoadRuleSettingsAsync(period.HospitalSiteId, cancellationToken);
            var reference = await LoadReferenceDataAsync(
                period.OrganizationUnitId,
                slotChecks.Select(x => x.WorkforceProfileId).Distinct().ToList(),
                request.ShiftDate,
                request.ShiftDate,
                cancellationToken);

            var sink = new ValidationSink();
            var index = staffChecks.IndexOf(candidate);

            await EvaluateShiftAsync(
                candidate,
                staffChecks,
                index > 0 ? staffChecks[index - 1] : null,
                index < staffChecks.Count - 1 ? staffChecks[index + 1] : null,
                rules,
                reference,
                sink,
                cancellationToken);

            EvaluateStreaks(staffChecks, candidate, rules, sink);

            if (!candidate.IsDayOff && candidate.ShiftId.HasValue)
            {
                EvaluateSlot(slotChecks, candidate, reference, sink);
            }

            return new RosterSingleShiftValidationResult
            {
                RosterPeriodId = period.Id,
                IsPassed = sink.BlockingConflicts == 0,
                BlockingConflicts = sink.BlockingConflicts,
                WarningConflicts = sink.WarningConflicts,
                Items = sink.Items,
                InfoMessages = reference.InfoMessages.ToList()
            };
        }

        // ------------------------------------------------------------------------------------
        // Evaluasi per shift: B1, B2, W4, B3, B8, B4/B5/B6/W5, W1
        // ------------------------------------------------------------------------------------

        private async Task EvaluateShiftAsync(
            ShiftCheck current,
            IReadOnlyList<ShiftCheck> staffShifts,
            ShiftCheck? previous,
            ShiftCheck? next,
            RuleSettings rules,
            ReferenceData reference,
            ValidationSink sink,
            CancellationToken cancellationToken)
        {
            var staffName = current.StaffName;
            var shiftDate = current.ShiftDate;

            // --- B1: Tabrakan Jam Shift (Double Shift) ---
            foreach (var other in staffShifts)
            {
                if (ReferenceEquals(other, current)) continue;
                var overlaps = current.ScheduledStartAt < other.ScheduledEndAt && other.ScheduledStartAt < current.ScheduledEndAt;

                if (overlaps && !current.IsDayOff && !other.IsDayOff)
                {
                    current.HasDoubleShiftConflict = true;
                    sink.Add(current, "B1", Blocking,
                        $"VR-SCH-01 (B1): Pegawai {staffName} dijadwalkan dua shift yang bertabrakan pada {shiftDate:dd-MM-yyyy} ({current.ShiftName ?? "Shift"}) dan {other.ShiftDate:dd-MM-yyyy} ({other.ShiftName ?? "Shift"}).");
                    break;
                }
            }

            // --- B2: Cuti Disetujui / W4: Cuti Pending ---
            if (!current.IsDayOff)
            {
                var leaves = reference.LeavesFor(current.WorkforceProfileId, shiftDate);
                if (leaves.Any(l => l.Status == "Approved"))
                {
                    current.HasLeaveConflict = true;
                    sink.Add(current, "B2", Blocking,
                        $"VR-SCH-02 (B2): Pegawai {staffName} sedang cuti yang telah disetujui pada {shiftDate:dd-MM-yyyy}.");
                }

                if (leaves.Any(l => PendingLeaveStatuses.Contains(l.Status)))
                {
                    sink.Add(current, "W4", Warning,
                        $"W4: Pegawai {staffName} memiliki pengajuan cuti yang masih berstatus pending pada {shiftDate:dd-MM-yyyy}.");
                }
            }

            // --- B3: Penempatan di unit roster (HRD-DEC-099, HRD-DEC-213) ---
            if (!current.IsDayOff)
            {
                var placementMessage = reference.CheckPlacement(current.WorkforceProfileId, staffName, shiftDate);
                if (placementMessage != null)
                {
                    sink.Add(current, RuleCode.NoActivePlacementInUnit, Blocking, $"VR-PK-01 (B3): {placementMessage}");
                }
            }

            // --- B8: Tugas belajar (HRD-DEC-201); tidak dapat diturunkan menjadi peringatan ---
            if (!current.IsDayOff)
            {
                var study = reference.StudyFor(current.WorkforceProfileId, shiftDate);
                if (study != null)
                {
                    sink.Add(current, RuleCode.ActiveStudyAssignment, Blocking,
                        $"VR-PK-02 (B8): {staffName} sedang tugas belajar {FormatRange(study.StartDate, study.EndDate)}.");
                }
            }

            // --- B4, B5, B6 & W5: Evaluasi Kesiapan Kerja (INT-HR-31) ---
            const string readinessFailedTemplate = "VR-SCH-06 (B6): Status kesiapan kerja pegawai {0} gagal dibaca dari sistem (INT-HR-31).";
            try
            {
                var readiness = await _readinessService.GetSchedulingEligibilityAsync(current.WorkforceProfileId, shiftDate, cancellationToken);
                if (!readiness.Success || readiness.Data == null)
                {
                    // B6: Status kesiapan gagal dibaca (gagal tertutup)
                    sink.Add(current, "B6", Blocking, string.Format(readinessFailedTemplate, staffName));
                }
                else
                {
                    var r = readiness.Data;
                    if (r.EligibilityStatus == "NotReady")
                    {
                        sink.Add(current, "B4", Blocking,
                            $"VR-SCH-04 (B4): Pegawai {staffName} belum dinyatakan siap kerja oleh HRD (HRD-DEC-088).");
                    }
                    else if (r.EligibilityStatus == "ContractHold")
                    {
                        sink.Add(current, "B5", Blocking,
                            $"VR-SCH-05 (B5): Tanggal shift {shiftDate:dd-MM-yyyy} berada sesudah tanggal akhir kontrak {staffName} yang belum diputuskan (HRD-DEC-089).");
                    }

                    if (r.IsAccompanied)
                    {
                        current.IsAccompanied = true;
                    }

                    if (!string.IsNullOrWhiteSpace(r.CredentialWarning))
                    {
                        sink.Add(current, "W5", Warning,
                            $"W5: Pegawai {staffName} memiliki peringatan kredensial pada {shiftDate:dd-MM-yyyy}: {r.CredentialWarning}.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal memanggil HrdEmploymentReadinessReadService untuk pegawai {StaffId}", current.WorkforceProfileId);
                sink.Add(current, "B6", Blocking, string.Format(readinessFailedTemplate, staffName));
            }

            // --- W1: Jeda Istirahat Minimum Antar Shift ---
            if (previous != null)
            {
                EvaluateRest(current, previous, current, rules, sink);
            }

            // Pada validasi satu shift, jeda ke shift berikutnya juga milik kandidat.
            if (next != null)
            {
                EvaluateRest(current, current, next, rules, sink);
            }
        }

        private static void EvaluateRest(ShiftCheck reported, ShiftCheck earlier, ShiftCheck later, RuleSettings rules, ValidationSink sink)
        {
            var gapHours = (later.ScheduledStartAt - earlier.ScheduledEndAt).TotalHours;
            if (gapHours >= rules.MinRestHours || later.IsDayOff || earlier.IsDayOff)
            {
                return;
            }

            reported.HasMinimumRestConflict = true;
            if (rules.RestHoursBlocked)
            {
                sink.Add(reported, "W1", Blocking,
                    $"B (Eskalasi W1): Jeda istirahat pegawai {reported.StaffName} ({gapHours:F1} jam) kurang dari standar {rules.MinRestHours} jam antara shift {earlier.ScheduledEndAt:dd-MM-yyyy HH:mm} dan {later.ScheduledStartAt:dd-MM-yyyy HH:mm}.");
            }
            else
            {
                sink.Add(reported, "W1", Warning,
                    $"W1: Jeda istirahat pegawai {reported.StaffName} ({gapHours:F1} jam) kurang dari anjuran {rules.MinRestHours} jam antara {earlier.ScheduledEndAt:dd-MM-yyyy HH:mm} dan {later.ScheduledStartAt:dd-MM-yyyy HH:mm}.");
            }
        }

        // ------------------------------------------------------------------------------------
        // W2 dan W3: rangkaian berturut-turut
        // ------------------------------------------------------------------------------------

        /// <summary>
        /// Pada validasi periode, setiap shift yang melampaui batas dilaporkan. Pada validasi satu shift
        /// (<paramref name="focus"/> terisi), pelanggaran dilaporkan sekali pada kandidat bila rangkaian
        /// yang melampaui batas memuat kandidat.
        /// </summary>
        private static void EvaluateStreaks(IReadOnlyList<ShiftCheck> staffShifts, ShiftCheck? focus, RuleSettings rules, ValidationSink sink)
        {
            EvaluateStreak(staffShifts, focus, s => !s.IsDayOff, rules.MaxWorkDays, rules.WorkDaysBlocked, (shift, count, blocked) =>
            {
                shift.HasWorkHourLimitConflict = true;
                var text = blocked
                    ? $"B (Eskalasi W2): Pegawai {shift.StaffName} bekerja {count} hari berturut-turut tanpa libur pada {shift.ShiftDate:dd-MM-yyyy} (Batas maksimal: {rules.MaxWorkDays} hari)."
                    : $"W2: Pegawai {shift.StaffName} bekerja {count} hari berturut-turut tanpa libur pada {shift.ShiftDate:dd-MM-yyyy} (Batas anjuran: {rules.MaxWorkDays} hari).";
                sink.Add(shift, RuleCode.ConsecutiveWorkDays, blocked ? Blocking : Warning, text);
            });

            EvaluateStreak(staffShifts, focus, s => s.IsNightShift && !s.IsDayOff, rules.MaxNightShifts, rules.NightShiftsBlocked, (shift, count, blocked) =>
            {
                var text = blocked
                    ? $"B (Eskalasi W3): Pegawai {shift.StaffName} dijadwalkan shift malam {count} kali berturut-turut pada {shift.ShiftDate:dd-MM-yyyy} (Batas maksimal: {rules.MaxNightShifts} malam)."
                    : $"W3: Pegawai {shift.StaffName} dijadwalkan shift malam {count} kali berturut-turut pada {shift.ShiftDate:dd-MM-yyyy} (Batas anjuran: {rules.MaxNightShifts} malam).";
                sink.Add(shift, RuleCode.ConsecutiveNightShifts, blocked ? Blocking : Warning, text);
            });
        }

        private static void EvaluateStreak(
            IReadOnlyList<ShiftCheck> shifts,
            ShiftCheck? focus,
            Func<ShiftCheck, bool> counts,
            int max,
            bool blocked,
            Action<ShiftCheck, int, bool> report)
        {
            var focusIndex = focus == null ? -1 : IndexOf(shifts, focus);
            var streak = 0;
            var streakStart = 0;
            var worstForFocus = 0;

            for (var i = 0; i < shifts.Count; i++)
            {
                if (!counts(shifts[i]))
                {
                    streak = 0;
                    continue;
                }

                if (streak == 0) streakStart = i;
                streak++;

                if (streak <= max) continue;

                if (focus == null)
                {
                    report(shifts[i], streak, blocked);
                }
                else if (streakStart <= focusIndex && focusIndex <= i)
                {
                    worstForFocus = Math.Max(worstForFocus, streak);
                }
            }

            if (focus != null && worstForFocus > 0)
            {
                report(focus, worstForFocus, blocked);
            }
        }

        private static int IndexOf(IReadOnlyList<ShiftCheck> shifts, ShiftCheck target)
        {
            for (var i = 0; i < shifts.Count; i++)
            {
                if (ReferenceEquals(shifts[i], target)) return i;
            }

            return -1;
        }

        // ------------------------------------------------------------------------------------
        // Evaluasi per slot shift: B7, W6, W7
        // ------------------------------------------------------------------------------------

        private static void EvaluateSlot(List<ShiftCheck> slot, ShiftCheck? focus, ReferenceData reference, ValidationSink sink)
        {
            var shiftDate = slot[0].ShiftDate;
            var shiftName = slot[0].ShiftName ?? "Shift";

            // --- B7: Staf Didampingi Tanpa Perawat Mandiri (HRD-DEC-100) ---
            var accompanied = slot.Where(s => s.IsAccompanied).ToList();
            if (accompanied.Any() && slot.All(s => s.IsAccompanied))
            {
                // Pada validasi satu shift, kandidat pasti termasuk staf Didampingi (seluruh slot Didampingi).
                foreach (var acc in focus == null ? accompanied : new List<ShiftCheck> { focus })
                {
                    sink.Add(acc, "B7", Blocking,
                        $"VR-SCH-07 (B7): Shift {shiftDate:dd-MM-yyyy} memiliki staf Didampingi ({acc.StaffName}) tanpa ada perawat Mandiri yang bertugas bersama (HRD-DEC-100). Ditolak demi keselamatan pasien.");
                }
            }

            // Laporan tingkat slot dipasang pada kandidat, atau pada shift pertama slot pada validasi periode.
            var anchor = focus ?? slot.OrderBy(s => s.ScheduledStartAt).ThenBy(s => s.StaffName, StringComparer.Ordinal).First();

            var standard = reference.StandardFor(shiftDate);
            if (standard == null)
            {
                // INT-HR-106: tanpa standar aktif, W6/W7 tidak dievaluasi.
                reference.InfoMessages.Add(NoStaffingStandardInfo);
                return;
            }

            // --- W6: Kuota tenaga per jabatan (HRD-DEC-100) ---
            foreach (var line in standard.Lines)
            {
                var required = line.StaffPerShift ?? line.MinimumHeadcount;
                if (!required.HasValue || required.Value <= 0) continue;

                var present = slot.Count(s => s.PositionId == line.PositionId);
                if (present < required.Value)
                {
                    sink.AddSlot(anchor, RuleCode.StaffingQuotaShortage, Warning,
                        $"W6: Shift {shiftName} {shiftDate:dd-MM-yyyy} di {reference.UnitName} berisi {present} {line.PositionName}, kurang dari kuota {required.Value:0.##} per shift.");
                }
            }

            // --- W7: Komposisi keahlian (skill-mix) wajib (INT-HR-107) ---
            foreach (var rule in standard.SkillMixRules.Where(r => !r.ShiftId.HasValue || r.ShiftId == slot[0].ShiftId))
            {
                if (!reference.CompetencyIdsWithData.Contains(rule.CompetencyId))
                {
                    reference.InfoMessages.Add($"Data kompetensi {rule.CompetencyName} belum tersedia; komposisi keahlian (W7) tidak diperiksa");
                    continue;
                }

                var qualified = slot.Count(s => reference.IsQualified(s.WorkforceProfileId, rule, shiftDate));
                if (qualified < rule.MinimumQualifiedHeadcount)
                {
                    sink.AddSlot(anchor, RuleCode.SkillMixNotMet, Warning,
                        $"W7: Shift {shiftName} {shiftDate:dd-MM-yyyy} di {reference.UnitName} hanya memiliki {qualified} pegawai dengan kompetensi {rule.CompetencyName}, kurang dari {rule.MinimumQualifiedHeadcount:0.##}.");
                }
            }
        }

        // ------------------------------------------------------------------------------------
        // Pemuatan data — sekali per pemeriksaan, bukan per shift
        // ------------------------------------------------------------------------------------

        private async Task<RuleSettings> LoadRuleSettingsAsync(Guid? hospitalSiteId, CancellationToken cancellationToken)
        {
            var settingResult = await _settingService.GetSettingAsync(hospitalSiteId ?? Guid.Empty, cancellationToken);
            var setting = settingResult.Data ?? new SchedulingSettingResponse();

            return new RuleSettings(
                setting.MinRestHoursBetweenShifts,
                setting.MaxConsecutiveWorkDays,
                setting.MaxConsecutiveNightShifts,
                setting.RestHoursEscalatedToBlock,
                setting.ConsecutiveDaysEscalatedToBlock,
                setting.NightShiftsEscalatedToBlock);
        }

        private async Task<ReferenceData> LoadReferenceDataAsync(
            Guid? organizationUnitId,
            List<Guid> staffIds,
            DateOnly fromDate,
            DateOnly toDate,
            CancellationToken cancellationToken)
        {
            var data = new ReferenceData { UnitId = organizationUnitId };

            data.Leaves = await _context.WfpLeaveRequests
                .AsNoTracking()
                .Where(l =>
                    staffIds.Contains(l.WorkforceProfileId) &&
                    !l.IsDelete &&
                    (l.LeaveRequestStatus == "Approved" || PendingLeaveStatuses.Contains(l.LeaveRequestStatus)) &&
                    l.StartDate <= toDate &&
                    l.EndDate >= fromDate)
                .Select(l => new LeaveRow(l.WorkforceProfileId, l.StartDate, l.EndDate, l.LeaveRequestStatus))
                .ToListAsync(cancellationToken);

            // Rentang dilebarkan sehari: tanggal efektif HR disimpan sebagai tanggal kalender pada tengah malam.
            var rangeStart = fromDate.AddDays(-1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var rangeEnd = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

            if (organizationUnitId.HasValue)
            {
                data.UnitName = await _context.MstOrganizationUnits
                    .AsNoTracking()
                    .Where(u => u.Id == organizationUnitId.Value)
                    .Select(u => u.UnitName)
                    .FirstOrDefaultAsync(cancellationToken) ?? "unit ini";

                data.Placements = await _context.WfpOrganizationAssignments
                    .AsNoTracking()
                    .Where(a =>
                        staffIds.Contains(a.WorkforceProfileId) &&
                        a.IsActive &&
                        !a.IsDelete &&
                        !a.IsCancel &&
                        a.EffectiveStartDate <= rangeEnd &&
                        (!a.EffectiveEndDate.HasValue || a.EffectiveEndDate.Value >= rangeStart))
                    .Select(a => new PlacementRow(
                        a.WorkforceProfileId,
                        a.OrganizationUnitId,
                        a.OrganizationUnit != null ? a.OrganizationUnit.UnitName : null,
                        a.AssignmentType,
                        a.EffectiveStartDate,
                        a.EffectiveEndDate))
                    .ToListAsync(cancellationToken);

                var standards = await _context.HrdStaffingStandards
                    .AsNoTracking()
                    .Where(s =>
                        s.OrganizationUnitId == organizationUnitId.Value &&
                        s.StandardStatus == StaffingStandardStatus.Active &&
                        !s.IsDelete &&
                        s.EffectiveStartDate <= toDate &&
                        (!s.EffectiveEndDate.HasValue || s.EffectiveEndDate.Value >= fromDate))
                    .Select(s => new StandardRow(
                        s.EffectiveStartDate,
                        s.EffectiveEndDate,
                        s.VersionNumber,
                        s.Lines
                            .Where(l => !l.IsDelete)
                            .Select(l => new StandardLineRow(
                                l.PositionId,
                                l.Position != null ? l.Position.PositionName : "pegawai",
                                l.StaffPerShift,
                                l.MinimumHeadcount))
                            .ToList(),
                        s.SkillMixRules
                            .Where(r => !r.IsDelete && r.IsMandatory)
                            .Select(r => new SkillMixRuleRow(
                                r.ShiftId,
                                r.CompetencyId,
                                r.Competency != null ? r.Competency.CompetencyName : "wajib",
                                r.MinimumCompetencyLevel,
                                r.MinimumQualifiedHeadcount))
                            .ToList()))
                    .ToListAsync(cancellationToken);
                data.Standards = standards;

                var competencyIds = standards.SelectMany(s => s.SkillMixRules).Select(r => r.CompetencyId).Distinct().ToList();
                if (competencyIds.Count > 0)
                {
                    data.CompetencyIdsWithData = (await _context.WfpCompetencyAssessments
                        .AsNoTracking()
                        .Where(c => competencyIds.Contains(c.CompetencyId) && c.IsActive && !c.IsDelete)
                        .Select(c => c.CompetencyId)
                        .Distinct()
                        .ToListAsync(cancellationToken))
                        .ToHashSet();

                    data.Competencies = await _context.WfpCompetencyAssessments
                        .AsNoTracking()
                        .Where(c =>
                            staffIds.Contains(c.WorkforceProfileId) &&
                            competencyIds.Contains(c.CompetencyId) &&
                            c.IsActive &&
                            !c.IsDelete &&
                            c.ResultStatus == CompetencyAssessmentResultStatus.Passed)
                        .Select(c => new CompetencyRow(c.WorkforceProfileId, c.CompetencyId, c.CompetencyLevel, c.ExpiredDate))
                        .ToListAsync(cancellationToken);
                }
            }

            data.Studies = await _context.HrdStudyAssignments
                .AsNoTracking()
                .Where(s =>
                    staffIds.Contains(s.WorkforceProfileId) &&
                    !s.IsDelete &&
                    s.Status == AttendanceValueConstants.StudyAssignmentStatus.Active &&
                    s.StartDate <= toDate &&
                    s.EndDate >= fromDate)
                .Select(s => new StudyRow(s.WorkforceProfileId, s.StartDate, s.EndDate))
                .ToListAsync(cancellationToken);

            return data;
        }

        internal static string FormatRange(DateOnly start, DateOnly? end)
        {
            if (!end.HasValue)
            {
                return $"mulai {start.ToString("d MMMM yyyy", Indonesian)}";
            }

            var e = end.Value;
            if (start.Year == e.Year && start.Month == e.Month)
            {
                return $"{start.Day}–{e.ToString("d MMMM yyyy", Indonesian)}";
            }

            return start.Year == e.Year
                ? $"{start.ToString("d MMMM", Indonesian)} – {e.ToString("d MMMM yyyy", Indonesian)}"
                : $"{start.ToString("d MMMM yyyy", Indonesian)} – {e.ToString("d MMMM yyyy", Indonesian)}";
        }

        // ------------------------------------------------------------------------------------
        // Tipe kerja internal
        // ------------------------------------------------------------------------------------

        private sealed record RuleSettings(
            int MinRestHours,
            int MaxWorkDays,
            int MaxNightShifts,
            bool RestHoursBlocked,
            bool WorkDaysBlocked,
            bool NightShiftsBlocked);

        private sealed class ShiftCheck
        {
            public TrxShiftAssignment? Entity { get; init; }
            public Guid ShiftAssignmentId { get; init; }
            public Guid WorkforceProfileId { get; init; }
            public string StaffName { get; init; } = string.Empty;
            public Guid? PositionId { get; init; }
            public Guid? ShiftId { get; init; }
            public string? ShiftName { get; init; }
            public DateOnly ShiftDate { get; init; }
            public DateTime ScheduledStartAt { get; init; }
            public DateTime ScheduledEndAt { get; init; }
            public bool IsNightShift { get; init; }
            public bool IsDayOff { get; init; }

            public bool IsAccompanied { get; set; }
            public bool HasDoubleShiftConflict { get; set; }
            public bool HasLeaveConflict { get; set; }
            public bool HasMinimumRestConflict { get; set; }
            public bool HasWorkHourLimitConflict { get; set; }
            public bool HasBlockingConflict { get; set; }

            public static ShiftCheck FromEntity(TrxShiftAssignment s, string staffName, Guid? positionId, bool track = true) => new()
            {
                Entity = track ? s : null,
                ShiftAssignmentId = s.Id,
                WorkforceProfileId = s.WorkforceProfileId,
                StaffName = staffName,
                PositionId = positionId,
                ShiftId = s.ShiftId,
                ShiftName = s.Shift?.ShiftName,
                ShiftDate = s.ShiftDate,
                ScheduledStartAt = s.ScheduledStartAt,
                ScheduledEndAt = s.ScheduledEndAt,
                IsNightShift = s.IsNightShift,
                IsDayOff = s.IsDayOff,
                IsAccompanied = s.IsAccompanied,
            };
        }

        private sealed class ValidationSink
        {
            public int TotalConflicts { get; private set; }
            public int BlockingConflicts { get; private set; }
            public int WarningConflicts { get; private set; }
            public List<string> ConflictMessages { get; } = new();
            public List<string> WarningMessages { get; } = new();
            public List<RosterValidationConflictItemDto> Items { get; } = new();

            public void Add(ShiftCheck shift, string ruleCode, string severity, string message) =>
                AddCore(shift, shift.WorkforceProfileId, shift.StaffName, ruleCode, severity, message);

            /// <summary>Temuan tingkat slot (W6, W7) tidak menunjuk satu pegawai.</summary>
            public void AddSlot(ShiftCheck anchor, string ruleCode, string severity, string message) =>
                AddCore(anchor, Guid.Empty, string.Empty, ruleCode, severity, message);

            private void AddCore(ShiftCheck shift, Guid staffId, string staffName, string ruleCode, string severity, string message)
            {
                TotalConflicts++;
                if (severity == Blocking)
                {
                    BlockingConflicts++;
                    if (staffId != Guid.Empty) shift.HasBlockingConflict = true;
                    if (!ConflictMessages.Contains(message)) ConflictMessages.Add(message);
                }
                else
                {
                    WarningConflicts++;
                    if (!WarningMessages.Contains(message)) WarningMessages.Add(message);
                }

                Items.Add(new RosterValidationConflictItemDto
                {
                    ShiftAssignmentId = shift.ShiftAssignmentId,
                    WorkforceProfileId = staffId,
                    StaffName = staffName,
                    ShiftDate = shift.ShiftDate,
                    RuleCode = ruleCode,
                    Severity = severity,
                    Message = message
                });
            }
        }

        private sealed record LeaveRow(Guid WorkforceProfileId, DateOnly StartDate, DateOnly EndDate, string Status);

        private sealed record PlacementRow(
            Guid WorkforceProfileId,
            Guid? OrganizationUnitId,
            string? UnitName,
            string AssignmentType,
            DateTime EffectiveStartDate,
            DateTime? EffectiveEndDate)
        {
            public DateOnly Start => DateOnly.FromDateTime(EffectiveStartDate);
            public DateOnly? End => EffectiveEndDate.HasValue ? DateOnly.FromDateTime(EffectiveEndDate.Value) : null;
            public bool CoversDate(DateOnly date) => Start <= date && (!End.HasValue || End.Value >= date);
        }

        private sealed record StudyRow(Guid WorkforceProfileId, DateOnly StartDate, DateOnly EndDate);

        private sealed record StandardLineRow(Guid PositionId, string PositionName, decimal? StaffPerShift, decimal? MinimumHeadcount);

        private sealed record SkillMixRuleRow(
            Guid? ShiftId,
            Guid CompetencyId,
            string CompetencyName,
            string? MinimumCompetencyLevel,
            decimal MinimumQualifiedHeadcount);

        private sealed record StandardRow(
            DateOnly EffectiveStartDate,
            DateOnly? EffectiveEndDate,
            int VersionNumber,
            List<StandardLineRow> Lines,
            List<SkillMixRuleRow> SkillMixRules);

        private sealed record CompetencyRow(Guid WorkforceProfileId, Guid CompetencyId, CompetencyLevel Level, DateTime? ExpiredDate);

        private sealed class ReferenceData
        {
            public Guid? UnitId { get; init; }
            public string UnitName { get; set; } = "unit ini";
            public List<LeaveRow> Leaves { get; set; } = new();
            public List<PlacementRow> Placements { get; set; } = new();
            public List<StudyRow> Studies { get; set; } = new();
            public List<StandardRow> Standards { get; set; } = new();
            public HashSet<Guid> CompetencyIdsWithData { get; set; } = new();
            public List<CompetencyRow> Competencies { get; set; } = new();

            /// <summary>Keterangan, bukan peringatan: tidak dihitung dan tidak butuh alasan saat terbit.</summary>
            public SortedSet<string> InfoMessages { get; } = new(StringComparer.Ordinal);

            public IEnumerable<LeaveRow> LeavesFor(Guid staffId, DateOnly date) =>
                Leaves.Where(l => l.WorkforceProfileId == staffId && l.StartDate <= date && l.EndDate >= date);

            public StudyRow? StudyFor(Guid staffId, DateOnly date) =>
                Studies.FirstOrDefault(s => s.WorkforceProfileId == staffId && s.StartDate <= date && s.EndDate >= date);

            public StandardRow? StandardFor(DateOnly date) =>
                Standards
                    .Where(s => s.EffectiveStartDate <= date && (!s.EffectiveEndDate.HasValue || s.EffectiveEndDate.Value >= date))
                    .OrderByDescending(s => s.VersionNumber)
                    .FirstOrDefault();

            /// <summary>
            /// B3 — null bila penempatan sah. Penempatan Temporary yang berlaku pada tanggal itu
            /// mengalahkan penempatan lain (HRD-DEC-213). Periode tanpa unit tidak dapat diperiksa.
            /// </summary>
            public string? CheckPlacement(Guid staffId, string staffName, DateOnly date)
            {
                if (!UnitId.HasValue)
                {
                    return null;
                }

                var active = Placements.Where(p => p.WorkforceProfileId == staffId && p.CoversDate(date)).ToList();
                var temporary = active.Where(p => string.Equals(p.AssignmentType, "Temporary", StringComparison.OrdinalIgnoreCase)).ToList();
                var effective = temporary.Count > 0 ? temporary : active;

                if (effective.Any(p => p.OrganizationUnitId == UnitId))
                {
                    return null;
                }

                var elsewhere = temporary.FirstOrDefault();
                if (elsewhere != null)
                {
                    return $"{staffName} ditempatkan di {elsewhere.UnitName ?? "unit lain"} {FormatRange(elsewhere.Start, elsewhere.End)}.";
                }

                return $"{staffName} tidak memiliki penempatan aktif di {UnitName} pada {date.ToString("d MMMM yyyy", Indonesian)}.";
            }

            public bool IsQualified(Guid staffId, SkillMixRuleRow rule, DateOnly date)
            {
                CompetencyLevel? minimum = Enum.TryParse<CompetencyLevel>(rule.MinimumCompetencyLevel, true, out var parsed) ? parsed : null;
                var dateTime = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

                return Competencies.Any(c =>
                    c.WorkforceProfileId == staffId &&
                    c.CompetencyId == rule.CompetencyId &&
                    (!c.ExpiredDate.HasValue || c.ExpiredDate.Value >= dateTime) &&
                    (!minimum.HasValue || c.Level >= minimum.Value));
            }
        }
    }
}
