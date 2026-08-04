using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    internal sealed class LeaveRequestActorContext
    {
        public Guid UserId { get; set; }
        public Guid WorkforceProfileId { get; set; }
        public Guid? EmployeeId { get; set; }
        public string? WorkforceProfileCode { get; set; }
        public string? WorkforceDisplayName { get; set; }
        public Guid? LegalEntityId { get; set; }
        public Guid? HospitalSiteId { get; set; }
        public Guid? OrganizationUnitId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PositionId { get; set; }
        public Guid? WorkLocationId { get; set; }
        public Guid? OrganizationAssignmentId { get; set; }
        public Guid? WorkforceTypeId { get; set; }
        public Guid? EmployeeCategoryId { get; set; }
        public Guid? EmploymentTypeId { get; set; }
        public Guid? EmploymentStatusId { get; set; }
        public Guid? ContractTypeId { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? ProbationEndDate { get; set; }
    }

    public class LeaveRequestCalculationService
    {
        private static readonly string[] ActiveOverlapStatuses =
        {
            LeaveRequestValueConstants.Status.Submitted,
            LeaveRequestValueConstants.Status.WaitingApproval,
            LeaveRequestValueConstants.Status.NeedRevision,
            LeaveRequestValueConstants.Status.Approved,
            LeaveRequestValueConstants.Status.Taken
        };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly AttendanceScheduleResolverService _scheduleResolver;

        public LeaveRequestCalculationService(
            ApplicationDbContext dbContext,
            AttendanceScheduleResolverService scheduleResolver)
        {
            _dbContext = dbContext;
            _scheduleResolver = scheduleResolver;
        }

        internal async Task<LeaveRequestServiceResult<LeaveRequestActorContext>> GetActorContextAsync(
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return LeaveRequestServiceResult<LeaveRequestActorContext>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid.");
            }

            var user = await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == actorUserId)
                .Select(x => new { x.Id, x.WorkforceProfileId, x.EmployeeId })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null || !user.WorkforceProfileId.HasValue)
            {
                return LeaveRequestServiceResult<LeaveRequestActorContext>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Akun login belum terhubung dengan workforce profile.");
            }

            var workforce = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(x =>
                    x.Id == user.WorkforceProfileId.Value &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (workforce == null)
            {
                return LeaveRequestServiceResult<LeaveRequestActorContext>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile user login tidak ditemukan atau tidak aktif.");
            }

            var now = DateTime.UtcNow;
            var assignment = await _dbContext.Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforce.Id &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.EffectiveStartDate <= now &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now))
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .FirstOrDefaultAsync(cancellationToken);

            var employee = workforce.Employee;
            return LeaveRequestServiceResult<LeaveRequestActorContext>.Ok(
                new LeaveRequestActorContext
                {
                    UserId = actorUserId,
                    WorkforceProfileId = workforce.Id,
                    EmployeeId = employee?.Id ?? user.EmployeeId,
                    WorkforceProfileCode = workforce.ProfileCode,
                    WorkforceDisplayName = workforce.DisplayName,
                    LegalEntityId = assignment?.LegalEntityId,
                    HospitalSiteId = assignment?.HospitalSiteId,
                    OrganizationUnitId = assignment?.OrganizationUnitId,
                    DepartmentId = assignment?.DepartmentId ?? workforce.PrimaryDepartmentId,
                    PositionId = assignment?.PositionId ?? workforce.PrimaryPositionId,
                    WorkLocationId = assignment?.WorkLocationId,
                    OrganizationAssignmentId = assignment?.Id,
                    WorkforceTypeId = employee?.WorkforceTypeId,
                    EmployeeCategoryId = employee?.EmployeeCategoryId,
                    EmploymentTypeId = employee?.EmploymentTypeId,
                    EmploymentStatusId = employee?.EmploymentStatusId,
                    ContractTypeId = employee?.ContractTypeId,
                    JoinDate = employee?.JoinDate,
                    ProbationEndDate = employee?.ProbationEndDate
                },
                "Context employee berhasil diselesaikan.");
        }

        internal async Task<MstLeavePolicy?> ResolvePolicyForActorAsync(
            LeaveRequestActorContext actor,
            Guid leaveTypeId,
            DateOnly requestDate,
            CancellationToken cancellationToken = default)
        {
            var date = requestDate.ToDateTime(TimeOnly.MinValue).Date;
            var policies = await _dbContext.Set<MstLeavePolicy>()
                .AsNoTracking()
                .Where(x =>
                    x.LeaveTypeId == leaveTypeId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= date) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= date))
                .ToListAsync(cancellationToken);

            return policies
                .Where(x => Matches(x.LegalEntityId, actor.LegalEntityId))
                .Where(x => Matches(x.HospitalSiteId, actor.HospitalSiteId))
                .Where(x => Matches(x.OrganizationUnitId, actor.OrganizationUnitId))
                .Where(x => Matches(x.DepartmentId, actor.DepartmentId))
                .Where(x => Matches(x.PositionId, actor.PositionId))
                .Where(x => Matches(x.WorkLocationId, actor.WorkLocationId))
                .Where(x => Matches(x.WorkforceTypeId, actor.WorkforceTypeId))
                .Where(x => Matches(x.EmployeeCategoryId, actor.EmployeeCategoryId))
                .Where(x => Matches(x.EmploymentTypeId, actor.EmploymentTypeId))
                .Where(x => Matches(x.EmploymentStatusId, actor.EmploymentStatusId))
                .Where(x => Matches(x.ContractTypeId, actor.ContractTypeId))
                .OrderByDescending(GetSpecificity)
                .ThenByDescending(x => x.Priority)
                .ThenByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.IsFallback)
                .FirstOrDefault();
        }

        public async Task<LeaveRequestServiceResult<LeaveRequestCalculationResponse>> CalculateAsync(
            Guid actorUserId,
            LeaveRequestCalculationRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorResult = await GetActorContextAsync(actorUserId, cancellationToken);
            if (!actorResult.Success || actorResult.Data == null)
            {
                return LeaveRequestServiceResult<LeaveRequestCalculationResponse>.Fail(
                    actorResult.StatusCode,
                    actorResult.Message);
            }

            return await CalculateForActorAsync(actorResult.Data, request, cancellationToken);
        }

        internal async Task<LeaveRequestServiceResult<LeaveRequestCalculationResponse>> CalculateForActorAsync(
            LeaveRequestActorContext actor,
            LeaveRequestCalculationRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.LeaveTypeId == Guid.Empty)
            {
                return LeaveRequestServiceResult<LeaveRequestCalculationResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Jenis cuti wajib dipilih.");
            }

            if (request.StartDate == default || request.EndDate == default || request.EndDate < request.StartDate)
            {
                return LeaveRequestServiceResult<LeaveRequestCalculationResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Rentang tanggal pengajuan tidak valid.");
            }

            var totalDays = request.EndDate.DayNumber - request.StartDate.DayNumber + 1;
            if (totalDays > 366)
            {
                return LeaveRequestServiceResult<LeaveRequestCalculationResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Rentang pengajuan cuti maksimal 366 hari.");
            }

            var leaveType = await _dbContext.Set<MstLeaveType>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.LeaveTypeId &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (leaveType == null)
            {
                return LeaveRequestServiceResult<LeaveRequestCalculationResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Jenis cuti tidak ditemukan atau tidak aktif.");
            }

            WfpLeaveBalance? balance = null;
            if (request.LeaveBalanceId.HasValue && request.LeaveBalanceId.Value != Guid.Empty)
            {
                balance = await _dbContext.Set<WfpLeaveBalance>()
                    .AsNoTracking()
                    .Include(x => x.LeavePolicy)
                    .Include(x => x.LeaveEntitlementPeriod)
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.LeaveBalanceId.Value &&
                        x.WorkforceProfileId == actor.WorkforceProfileId &&
                        x.LeaveTypeId == request.LeaveTypeId &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                if (balance == null)
                {
                    return LeaveRequestServiceResult<LeaveRequestCalculationResponse>.Fail(
                        StatusCodes.Status404NotFound,
                        "Saldo cuti tidak ditemukan atau bukan milik employee login.");
                }
            }

            if (leaveType.IsBalanceDeducted && balance == null)
            {
                return LeaveRequestServiceResult<LeaveRequestCalculationResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Jenis cuti ini memerlukan leave balance aktif.");
            }

            var policy = balance?.LeavePolicy ?? await ResolvePolicyForActorAsync(
                actor,
                leaveType.Id,
                request.StartDate,
                cancellationToken);

            if (policy == null)
            {
                return LeaveRequestServiceResult<LeaveRequestCalculationResponse>.Fail(
                    StatusCodes.Status404NotFound,
                    "Leave policy aktif tidak ditemukan untuk employee dan jenis cuti tersebut.");
            }

            var effectiveAvailable = balance?.AvailableDays ?? 0;
            if (request.ExcludeLeaveRequestId.HasValue)
            {
                var ownReserved = await _dbContext.Set<TrxLeaveBalanceTransaction>()
                    .AsNoTracking()
                    .Where(x =>
                        x.LeaveRequestId == request.ExcludeLeaveRequestId.Value &&
                        x.TransactionStatus == LeaveValueConstants.TransactionStatus.Posted &&
                        !x.IsDelete)
                    .SumAsync(x => (decimal?)x.ReservedDelta, cancellationToken) ?? 0;
                effectiveAvailable += Math.Max(0, ownReserved);
            }

            var response = new LeaveRequestCalculationResponse
            {
                WorkforceProfileId = actor.WorkforceProfileId,
                WorkforceProfileCode = actor.WorkforceProfileCode,
                WorkforceDisplayName = actor.WorkforceDisplayName,
                LeaveTypeId = leaveType.Id,
                LeaveTypeCode = leaveType.LeaveTypeCode,
                LeaveTypeName = leaveType.LeaveTypeName,
                LeaveBalanceId = balance?.Id,
                LeavePolicyId = policy.Id,
                LeavePolicyCode = policy.LeavePolicyCode,
                LeavePolicyName = policy.LeavePolicyName,
                DayCalculationMethod = policy.DayCalculationMethod,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RequestedMinutes = request.RequestedMinutes,
                BalanceBeforeRequest = effectiveAvailable,
                RequiresReplacement = policy.RequireReplacementEmployee
            };

            ValidateRequestMode(request, leaveType, policy, response);
            ValidatePolicyDates(actor, request, policy, response);

            var schedules = await ResolveSchedulesAsync(
                actor.WorkforceProfileId,
                request.StartDate,
                request.EndDate,
                cancellationToken);

            if (!schedules.Success || schedules.Data == null)
            {
                return LeaveRequestServiceResult<LeaveRequestCalculationResponse>.Fail(
                    schedules.StatusCode,
                    schedules.Message);
            }

            response.Days = CalculateDays(request, policy, schedules.Data, response);
            CalculateTotals(leaveType, balance, policy, response);

            MstRequestReason? requestReason = null;
            if (request.RequestReasonId.HasValue)
            {
                requestReason = await _dbContext.Set<MstRequestReason>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.RequestReasonId.Value &&
                        x.IsActive &&
                        x.IsEmployeeSelectable &&
                        !x.IsDelete &&
                        (x.RequestType == "LeaveRequest" || x.RequestType == "LEAVE_REQUEST"),
                        cancellationToken);

                if (requestReason == null)
                {
                    response.Errors.Add("Alasan pengajuan tidak valid atau tidak tersedia untuk employee.");
                }
            }

            response.RequiresAttachment =
                leaveType.RequiresAttachment ||
                policy.RequireAttachment ||
                requestReason?.IsAttachmentRequired == true ||
                (leaveType.AttachmentRequiredAfterDays.HasValue &&
                 response.RequestedDays >= leaveType.AttachmentRequiredAfterDays.Value) ||
                (policy.AttachmentRequiredAfterDays.HasValue &&
                 response.RequestedDays >= policy.AttachmentRequiredAfterDays.Value);

            response.RequiresMedicalCertificate = leaveType.RequiresMedicalCertificate;

            if (response.RequiresReplacement && !request.ReplacementWorkforceProfileId.HasValue)
            {
                response.Errors.Add("Employee pengganti wajib dipilih berdasarkan leave policy.");
            }

            if (request.ReplacementWorkforceProfileId == actor.WorkforceProfileId)
            {
                response.Errors.Add("Employee pengganti tidak boleh sama dengan pemohon.");
            }

            if (request.ReplacementWorkforceProfileId.HasValue)
            {
                var replacementValid = await _dbContext.Set<MstWorkforceProfile>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == request.ReplacementWorkforceProfileId.Value &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                if (!replacementValid)
                {
                    response.Errors.Add("Employee pengganti tidak ditemukan atau tidak aktif.");
                }
            }

            response.HasOverlap = await HasOverlapAsync(
                actor.WorkforceProfileId,
                request.StartDate,
                request.EndDate,
                request.ExcludeLeaveRequestId,
                cancellationToken);

            if (response.HasOverlap)
            {
                response.Errors.Add("Terdapat pengajuan cuti aktif yang bertabrakan pada rentang tanggal tersebut.");
            }

            response.HasRosterConflict = response.Days.Any(x => x.HasBlockingConflict);
            response.IsValid = response.Errors.Count == 0;
            response.CalculationSnapshotJson = JsonSerializer.Serialize(new
            {
                calculatedAt = DateTime.UtcNow,
                actor.WorkforceProfileId,
                leaveType = new { leaveType.Id, leaveType.LeaveTypeCode, leaveType.LeaveTypeName },
                policy = new
                {
                    policy.Id,
                    policy.LeavePolicyCode,
                    policy.DayCalculationMethod,
                    policy.ExcludeHoliday,
                    policy.ExcludeWeeklyOff,
                    policy.ReservationTiming,
                    policy.DeductionTiming,
                    policy.AllowNegativeBalance,
                    policy.NegativeBalanceLimitDays
                },
                request.StartDate,
                request.EndDate,
                request.IsHalfDay,
                request.HalfDayPeriod,
                request.IsHourly,
                request.RequestedMinutes,
                response.RequestedDays,
                response.CalculatedWorkingDays,
                response.ExcludedHolidayDays,
                response.ExcludedWeeklyOffDays,
                response.BalanceBeforeRequest,
                response.EstimatedBalanceDeduction,
                response.EstimatedBalanceAfterRequest,
                response.RequiresAttachment,
                response.RequiresReplacement,
                response.HasOverlap,
                response.HasRosterConflict,
                response.Errors,
                response.Warnings,
                days = response.Days
            }, JsonOptions);

            return LeaveRequestServiceResult<LeaveRequestCalculationResponse>.Ok(
                response,
                response.IsValid
                    ? "Perhitungan pengajuan cuti berhasil."
                    : "Perhitungan selesai, tetapi masih terdapat validasi yang harus diperbaiki.");
        }

        private async Task<LeaveRequestServiceResult<List<AttendanceScheduleResolutionResponse>>> ResolveSchedulesAsync(
            Guid workforceProfileId,
            DateOnly startDate,
            DateOnly endDate,
            CancellationToken cancellationToken)
        {
            var result = new List<AttendanceScheduleResolutionResponse>();
            for (var chunkStart = startDate; chunkStart <= endDate; chunkStart = chunkStart.AddDays(31))
            {
                var chunkEnd = chunkStart.AddDays(30);
                if (chunkEnd > endDate) chunkEnd = endDate;

                var range = await _scheduleResolver.ResolveRangeAsync(
                    workforceProfileId,
                    chunkStart,
                    chunkEnd,
                    cancellationToken);

                if (!range.Success || range.Data == null)
                {
                    return LeaveRequestServiceResult<List<AttendanceScheduleResolutionResponse>>.Fail(
                        range.StatusCode,
                        range.Message);
                }

                result.AddRange(range.Data.Items);
            }

            return LeaveRequestServiceResult<List<AttendanceScheduleResolutionResponse>>.Ok(
                result,
                "Jadwal employee berhasil diselesaikan.");
        }

        private static List<LeaveRequestCalculationDayResponse> CalculateDays(
            LeaveRequestCalculationRequest request,
            MstLeavePolicy policy,
            List<AttendanceScheduleResolutionResponse> schedules,
            LeaveRequestCalculationResponse response)
        {
            var days = new List<LeaveRequestCalculationDayResponse>();
            foreach (var schedule in schedules.OrderBy(x => x.WorkDate))
            {
                var item = new LeaveRequestCalculationDayResponse
                {
                    Date = schedule.WorkDate,
                    IsResolved = schedule.IsResolved,
                    IsRestDay = schedule.IsRestDay,
                    IsHoliday = schedule.IsHoliday,
                    HasBlockingConflict = schedule.HasBlockingConflict,
                    ScheduleSource = schedule.ScheduleSource,
                    ShiftCode = schedule.ShiftCode,
                    ShiftName = schedule.ShiftName,
                    ScheduledStartAt = schedule.ScheduledStartAt,
                    ScheduledEndAt = schedule.ScheduledEndAt,
                    PlannedWorkMinutes = schedule.PlannedWorkMinutes,
                    HolidayNames = schedule.Holidays.Select(x => x.HolidayName).ToList(),
                    Warnings = schedule.Warnings.ToList(),
                    ConflictCodes = schedule.ConflictCodes.ToList()
                };

                if (policy.DayCalculationMethod == LeaveValueConstants.DayCalculationMethod.CalendarDays)
                {
                    item.IsCounted = true;
                    item.CountedDays = 1;
                }
                else
                {
                    if (!schedule.IsResolved && !schedule.IsRestDay)
                    {
                        response.Errors.Add($"Jadwal tanggal {schedule.WorkDate:yyyy-MM-dd} belum dapat diselesaikan.");
                    }

                    if (schedule.HasBlockingConflict)
                    {
                        response.Errors.Add($"Jadwal tanggal {schedule.WorkDate:yyyy-MM-dd} memiliki blocking conflict.");
                    }

                    var excludedHoliday = policy.ExcludeHoliday && schedule.IsHoliday;
                    var excludedWeeklyOff = policy.ExcludeWeeklyOff && schedule.IsRestDay;
                    item.IsCounted = schedule.IsResolved && !schedule.HasBlockingConflict && !excludedHoliday && !excludedWeeklyOff;
                    item.CountedDays = item.IsCounted ? 1 : 0;
                    if (excludedHoliday) response.ExcludedHolidayDays += 1;
                    if (excludedWeeklyOff) response.ExcludedWeeklyOffDays += 1;
                }

                days.Add(item);
            }

            if (request.IsHalfDay && days.Count == 1)
            {
                if (days[0].IsCounted) days[0].CountedDays = 0.5m;
                else response.Errors.Add("Half-day hanya dapat diajukan pada hari kerja yang valid.");
            }

            if (request.IsHourly && days.Count == 1 && request.RequestedMinutes.HasValue)
            {
                if (!days[0].IsCounted)
                {
                    response.Errors.Add("Cuti per jam hanya dapat diajukan pada hari kerja yang valid.");
                }
                else
                {
                    var planned = days[0].PlannedWorkMinutes > 0 ? days[0].PlannedWorkMinutes : 480;
                    if (request.RequestedMinutes.Value > planned)
                    {
                        response.Errors.Add("RequestedMinutes tidak boleh melebihi durasi kerja terjadwal.");
                    }
                    days[0].CountedDays = Math.Round(request.RequestedMinutes.Value / (decimal)planned, 4, MidpointRounding.AwayFromZero);
                }
            }

            return days;
        }

        private static void CalculateTotals(
            MstLeaveType leaveType,
            WfpLeaveBalance? balance,
            MstLeavePolicy policy,
            LeaveRequestCalculationResponse response)
        {
            response.CalculatedWorkingDays = response.Days.Sum(x => x.CountedDays);
            response.RequestedDays = response.CalculatedWorkingDays;

            if (policy.MaximumRequestDays.HasValue && response.RequestedDays > policy.MaximumRequestDays.Value)
            {
                response.Errors.Add($"Jumlah cuti melebihi batas maksimum {policy.MaximumRequestDays.Value} hari per pengajuan.");
            }

            if (response.RequestedDays <= 0)
            {
                response.Errors.Add("Tidak ada hari kerja yang dapat dihitung pada rentang pengajuan.");
            }

            response.EstimatedBalanceDeduction = leaveType.IsBalanceDeducted ? response.RequestedDays : 0;
            response.EstimatedBalanceAfterRequest = response.BalanceBeforeRequest - response.EstimatedBalanceDeduction;

            if (!leaveType.IsBalanceDeducted)
            {
                response.IsBalanceSufficient = true;
                return;
            }

            if (balance == null)
            {
                response.IsBalanceSufficient = false;
                response.Errors.Add("Saldo cuti aktif tidak tersedia.");
                return;
            }

            if (balance.IsLocked ||
                balance.BalanceStatus == LeaveValueConstants.BalanceStatus.Locked ||
                balance.BalanceStatus == LeaveValueConstants.BalanceStatus.Closed)
            {
                response.Errors.Add("Saldo cuti sedang dikunci atau sudah ditutup.");
            }

            var minimumAllowed = policy.AllowNegativeBalance
                ? -(policy.NegativeBalanceLimitDays ?? decimal.MaxValue)
                : 0;

            response.IsBalanceSufficient = response.EstimatedBalanceAfterRequest >= minimumAllowed;
            if (!response.IsBalanceSufficient)
            {
                response.Errors.Add(policy.AllowNegativeBalance
                    ? $"Saldo setelah pengajuan melebihi batas negatif {policy.NegativeBalanceLimitDays ?? 0} hari."
                    : "Saldo cuti tidak mencukupi.");
            }
        }

        private static void ValidateRequestMode(
            LeaveRequestCalculationRequest request,
            MstLeaveType leaveType,
            MstLeavePolicy policy,
            LeaveRequestCalculationResponse response)
        {
            if (request.IsHalfDay && request.IsHourly)
                response.Errors.Add("Pengajuan tidak dapat sekaligus berupa half-day dan hourly.");

            if ((request.IsHalfDay || request.IsHourly) && request.StartDate != request.EndDate)
                response.Errors.Add("Pengajuan half-day atau hourly hanya dapat dilakukan pada satu tanggal.");

            if (request.IsHalfDay)
            {
                if (!leaveType.AllowHalfDay) response.Errors.Add("Jenis cuti tidak mengizinkan half-day.");
                if (request.HalfDayPeriod != LeaveRequestValueConstants.HalfDayPeriod.FirstHalf &&
                    request.HalfDayPeriod != LeaveRequestValueConstants.HalfDayPeriod.SecondHalf)
                    response.Errors.Add("HalfDayPeriod harus FirstHalf atau SecondHalf.");
            }

            if (request.IsHourly)
            {
                if (!leaveType.AllowHourly) response.Errors.Add("Jenis cuti tidak mengizinkan pengajuan per jam.");
                if (!request.RequestedMinutes.HasValue || request.RequestedMinutes.Value <= 0)
                    response.Errors.Add("RequestedMinutes wajib diisi untuk pengajuan hourly.");
                if (policy.MinimumRequestMinutes.HasValue &&
                    request.RequestedMinutes.HasValue &&
                    request.RequestedMinutes.Value < policy.MinimumRequestMinutes.Value)
                    response.Errors.Add($"Durasi minimum pengajuan adalah {policy.MinimumRequestMinutes.Value} menit.");
                if (!request.StartTime.HasValue || !request.EndTime.HasValue || request.EndTime.Value <= request.StartTime.Value)
                    response.Errors.Add("Jam mulai dan selesai pengajuan hourly tidak valid.");
            }
        }

        private static void ValidatePolicyDates(
            LeaveRequestActorContext actor,
            LeaveRequestCalculationRequest request,
            MstLeavePolicy policy,
            LeaveRequestCalculationResponse response)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var noticeDays = request.StartDate.DayNumber - today.DayNumber;

            if (request.StartDate < today)
            {
                if (!policy.AllowBackdatedRequest)
                    response.Errors.Add("Leave policy tidak mengizinkan pengajuan backdated.");
                else if (today.DayNumber - request.StartDate.DayNumber > policy.BackdatedLimitDays)
                    response.Errors.Add($"Pengajuan backdated melebihi batas {policy.BackdatedLimitDays} hari.");
            }
            else
            {
                if (!policy.AllowFutureDatedRequest)
                    response.Errors.Add("Leave policy tidak mengizinkan pengajuan tanggal mendatang.");
                if (noticeDays < policy.MinimumNoticeDays)
                    response.Errors.Add($"Pengajuan harus dilakukan minimal {policy.MinimumNoticeDays} hari sebelum tanggal cuti.");
                if (policy.MaximumAdvanceRequestDays.HasValue && noticeDays > policy.MaximumAdvanceRequestDays.Value)
                    response.Errors.Add($"Pengajuan melebihi batas maksimal {policy.MaximumAdvanceRequestDays.Value} hari ke depan.");
            }

            if (!policy.AllowDuringProbation && actor.ProbationEndDate.HasValue &&
                actor.ProbationEndDate.Value.Date >= request.StartDate.ToDateTime(TimeOnly.MinValue).Date)
                response.Errors.Add("Leave policy tidak mengizinkan pengajuan selama masa probation.");

            if (actor.JoinDate.HasValue && policy.MinimumServiceMonths > 0)
            {
                var minimumServiceDate = actor.JoinDate.Value.Date.AddMonths(policy.MinimumServiceMonths);
                if (request.StartDate.ToDateTime(TimeOnly.MinValue).Date < minimumServiceDate)
                    response.Errors.Add($"Masa kerja minimum {policy.MinimumServiceMonths} bulan belum terpenuhi.");
            }
        }

        private async Task<bool> HasOverlapAsync(
            Guid workforceProfileId,
            DateOnly startDate,
            DateOnly endDate,
            Guid? excludeRequestId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<WfpLeaveRequest>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    (!excludeRequestId.HasValue || x.Id != excludeRequestId.Value) &&
                    ActiveOverlapStatuses.Contains(x.LeaveRequestStatus) &&
                    x.StartDate <= endDate &&
                    x.EndDate >= startDate &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel,
                    cancellationToken);
        }

        private static bool Matches(Guid? policyValue, Guid? actorValue) =>
            !policyValue.HasValue || policyValue == actorValue;

        private static int GetSpecificity(MstLeavePolicy policy)
        {
            Guid?[] values =
            {
                policy.LegalEntityId,
                policy.HospitalSiteId,
                policy.OrganizationUnitId,
                policy.DepartmentId,
                policy.PositionId,
                policy.WorkLocationId,
                policy.WorkforceTypeId,
                policy.EmployeeCategoryId,
                policy.EmploymentTypeId,
                policy.EmploymentStatusId,
                policy.ContractTypeId
            };
            return values.Count(x => x.HasValue);
        }
    }
}
