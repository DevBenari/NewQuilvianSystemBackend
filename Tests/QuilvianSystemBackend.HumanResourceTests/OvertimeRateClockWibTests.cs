using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-FIX-004 — rentang jam tarif lembur (Night/Custom) dicocokkan dengan jam WIB. Tarif AllDay 1,5× dan Night 22.00–06.00 2,0×;
// 22.00 WIB (= 15.00Z) harus Night, 10.00 WIB (= 03.00Z, "malam" bila dibaca UTC) harus AllDay. Shift Pagi 07.00–14.00.
public sealed class OvertimeRateClockWibTests : IDisposable
{
    private static readonly DateOnly Hari = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)).AddDays(2);
    private static DateTime Wib(int jam) => new DateTime(Hari.Year, Hari.Month, Hari.Day, jam, 0, 0, DateTimeKind.Utc).AddHours(-7);

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _user = Guid.NewGuid(), _wf = Guid.NewGuid(), _unit = Guid.NewGuid(), _pagi = Guid.NewGuid(), _periode = Guid.NewGuid(), _kebijakan = Guid.NewGuid();

    public OvertimeRateClockWibTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _unit, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        ctx.MstShifts.Add(new MstShift { Id = _pagi, ShiftCode = "P", ShiftName = "Pagi", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(14, 0), IsActive = true });
        ctx.HrdAttendanceSettings.Add(new HrdAttendanceSetting { HospitalCode = "RSMMC", GracePeriodMinutes = 10, ScanWindowBeforeMinutes = 240, ScanWindowAfterMinutes = 480, IsActive = true });
        ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = _wf, ProfileCode = "Budi", DisplayName = "Budi", Email = "budi@uji.local", UserType = UserType.Employee, IsActive = true });
        ctx.Users.Add(new ApplicationUser { Id = _user, UserName = "budi", Email = "budi@uji.local", UserCode = "BUDIXXXX", DisplayName = "Budi", UserType = UserType.Employee, WorkforceProfileId = _wf, IsActive = true });
        ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
        {
            WorkforceProfileId = _wf, OrganizationUnitId = _unit, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), AssignmentType = "Primary", IsPrimary = true,
            EffectiveStartDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true,
        });
        ctx.TrxRosterPeriods.Add(new TrxRosterPeriod
        {
            Id = _periode, OrganizationUnitId = _unit, RosterPeriodCode = "R", RosterPeriodName = "R", RosterStatus = "Published", IsPublished = true, VersionNumber = 1,
            PeriodStartDate = Hari.AddDays(-10), PeriodEndDate = Hari.AddDays(10), IsActive = true,
        });
        var ra = Guid.NewGuid();
        ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = ra, RosterPeriodId = _periode, WorkforceProfileId = _wf, AssignmentStatus = "Published", IsActive = true });
        ctx.TrxShiftAssignments.Add(new TrxShiftAssignment { RosterAssignmentId = ra, WorkforceProfileId = _wf, ShiftId = _pagi, ShiftDate = Hari, ScheduledStartAt = Wib(7), ScheduledEndAt = Wib(14), AssignmentStatus = "Published", IsActive = true });

        ctx.Set<MstOvertimePolicy>().Add(new MstOvertimePolicy
        {
            Id = _kebijakan, OvertimePolicyCode = "OTP-UJI", OvertimePolicyName = "Uji", RequirePreApproval = true, RequireAttendanceMatch = false, MinimumOvertimeMinutes = 30,
            RoundingIntervalMinutes = 30, RoundingMethod = "Down", AllowBeforeShift = true, AllowAfterShift = true, AllowRestDay = true, AllowHoliday = true,
            ApprovalWorkflowCode = "UJI_LEMBUR", IsActive = true,
        });
        ctx.Set<MstOvertimeRate>().Add(new MstOvertimeRate { OvertimePolicyId = _kebijakan, OvertimeRateCode = "OTR-HARIAN", OvertimeRateName = "Harian", DayType = "Workday", TimeBand = "AllDay", CalculationMethod = "Multiplier", RateMultiplier = 1.5m, IsActive = true });
        ctx.Set<MstOvertimeRate>().Add(new MstOvertimeRate
        {
            OvertimePolicyId = _kebijakan, OvertimeRateCode = "OTR-MALAM", OvertimeRateName = "Malam", DayType = "Workday", TimeBand = "Night", CalculationMethod = "Multiplier", RateMultiplier = 2m,
            StartTime = new TimeOnly(22, 0), EndTime = new TimeOnly(6, 0), IsActive = true,
        });
        ctx.MstWorkflowDefinitions.Add(new MstWorkflowDefinition { WorkflowCode = "UJI_LEMBUR", WorkflowName = "Uji", RequestType = OvertimeValueConstants.Workflow.RequestType, WorkflowStatus = OvertimeValueConstants.Workflow.ActiveStatus, IsActive = true });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    // Pengajuan lembur mandiri: pratinjau tarif.
    [Theory]
    [InlineData(22, 23, "OTR-MALAM")]
    [InlineData(14, 15, "OTR-HARIAN")]
    public async Task Lembur_mandiri_memilih_tarif_menurut_jam_WIB(int mulai, int selesai, string tarif)
    {
        var ctx = _db.CreateContext();
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        var konteks = new OvertimeSelfServiceContextService(ctx);
        // Integrasi workflow hanya dipakai saat submit; pratinjau tidak menyentuhnya.
        var svc = new OvertimeSelfServiceService(ctx, konteks, new OvertimeSelfServiceQueryService(ctx, konteks), new AttendanceScheduleResolverService(ctx, setelan),
            new OvertimePolicyResolverService(ctx), new OvertimeRateResolverService(ctx), null!, new OvertimePeriodGuardService(ctx));
        var r = await svc.PreviewAsync(_user, new PreviewMyOvertimeRequest
        { OvertimeDate = Hari, PlannedStartAt = Wib(mulai), PlannedEndAt = Wib(selesai), OvertimeCategory = "AfterShift", Reason = "uji", WorkDescription = "uji" });
        Assert.True(r.Success, r.Message);
        Assert.Equal(tarif, r.Data!.PreviewOvertimeRateCode);
    }

    // Rencana lembur manajer: pratinjau tarif detail.
    [Theory]
    [InlineData(22, 23, "OTR-MALAM")]
    [InlineData(10, 11, "OTR-HARIAN")]
    public async Task Rencana_lembur_memilih_tarif_menurut_jam_WIB(int mulai, int selesai, string tarif)
    {
        var rencana = Guid.NewGuid();
        using (var tulis = _db.CreateContext())
        {
            tulis.TrxOvertimePlans.Add(new TrxOvertimePlan { Id = rencana, PlanNumber = "OTP-UJI-1", PlanTitle = "Uji", OrganizationUnitId = _unit, PlanStartDate = Hari.AddDays(-1), PlanEndDate = Hari.AddDays(1), Reason = "uji" });
            tulis.SaveChanges();
        }

        var ctx = _db.CreateContext();
        var svc = new OvertimePlanningService(ctx, null!, new OvertimePolicyResolverService(ctx), new OvertimeRateResolverService(ctx), new OvertimePeriodGuardService(ctx));
        var r = await svc.PreviewDetailAsync(rencana, new OvertimePlanDetailPreviewRequest
        {
            WorkforceProfileId = _wf, OvertimePolicyId = _kebijakan, OvertimeDate = Hari, PlannedStartAt = Wib(mulai), PlannedEndAt = Wib(selesai),
            DayType = "Workday", OvertimeCategory = "Emergency", WorkDescription = "uji",
        });
        Assert.True(r.Success, r.Message);
        Assert.True(r.Data!.PreviewOvertimeRateCode == tarif, string.Join(" | ", r.Data.Issues.Select(x => $"{x.Code}: {x.Message}")) + $" → tarif {r.Data.PreviewOvertimeRateCode}");
    }

    // Realisasi (pencocokan presensi): rincian tarif per menit; kebijakan tanpa wajib presensi memakai jendela yang disetujui.
    [Theory]
    [InlineData(22, 23, "OTR-MALAM")]
    [InlineData(10, 11, "OTR-HARIAN")]
    public async Task Realisasi_memecah_tarif_menurut_jam_WIB(int mulai, int selesai, string tarif)
    {
        var permintaan = Guid.NewGuid();
        using (var tulis = _db.CreateContext())
        {
            tulis.WfpOvertimeRequests.Add(new WfpOvertimeRequest
            {
                Id = permintaan, RequestNumber = "OTR-UJI-1", WorkforceProfileId = _wf, OvertimePolicyId = _kebijakan, OvertimeDate = Hari, PlannedStartAt = Wib(mulai), PlannedEndAt = Wib(selesai),
                RequestedMinutes = 60, ApprovedMinutes = 60, Reason = "uji", OvertimeRequestStatus = OvertimeValueConstants.RequestStatus.ApprovedForWork,
            });
            tulis.Set<TrxOvertimeRequestDetail>().Add(new TrxOvertimeRequestDetail
            {
                OvertimeRequestId = permintaan, OvertimeDate = Hari, PlannedStartAt = Wib(mulai), PlannedEndAt = Wib(selesai), RequestedMinutes = 60, ApprovedMinutes = 60,
                DayType = "Workday", OvertimeCategory = "Emergency", WorkDescription = "uji", DetailStatus = "Approved",
            });
            tulis.SaveChanges();
        }

        var ctx = _db.CreateContext();
        var r = await new OvertimeAttendanceMatchingService(ctx, NullLogger<OvertimeAttendanceMatchingService>.Instance).PreviewAsync(permintaan);
        Assert.True(r.Success, r.Message);
        var rincian = r.Data!.Details.Single().RateBreakdown;
        Assert.True(rincian.Count > 0, string.Join(" | ", r.Data.Issues.Select(x => $"{x.Code}: {x.Message}")));
        Assert.All(rincian, x => Assert.Equal(tarif, x.OvertimeRateCode));
        Assert.Equal(60, rincian.Sum(x => x.Minutes));
    }
}
