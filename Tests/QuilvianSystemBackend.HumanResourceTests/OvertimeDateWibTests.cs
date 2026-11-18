using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-FIX-003 — tanggal lembur dalam WIB (temuan uji asap 17 Nov 2026). Shift Pagi 07.00–14.00 pada tanggal kerja.
public sealed class OvertimeDateWibTests : IDisposable
{
    private static readonly DateOnly Hari = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)).AddDays(2);
    private static DateTime Wib(DateOnly d, int jam, int menit = 0) => new DateTime(d.Year, d.Month, d.Day, jam, menit, 0, DateTimeKind.Utc).AddHours(-7);

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _user = Guid.NewGuid(), _wf = Guid.NewGuid(), _unit = Guid.NewGuid(), _pagi = Guid.NewGuid(), _periode = Guid.NewGuid();

    public OvertimeDateWibTests()
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
        ctx.TrxShiftAssignments.Add(new TrxShiftAssignment { RosterAssignmentId = ra, WorkforceProfileId = _wf, ShiftId = _pagi, ShiftDate = Hari, ScheduledStartAt = Wib(Hari, 7), ScheduledEndAt = Wib(Hari, 14), AssignmentStatus = "Published", IsActive = true });

        var kebijakan = new MstOvertimePolicy
        {
            OvertimePolicyCode = "OTP-UJI", OvertimePolicyName = "Uji", RequirePreApproval = true, MinimumOvertimeMinutes = 30, RoundingIntervalMinutes = 30, RoundingMethod = "Down",
            AllowBeforeShift = true, AllowAfterShift = true, AllowRestDay = true, AllowHoliday = true, ApprovalWorkflowCode = "UJI_LEMBUR", IsActive = true,
        };
        ctx.MstWorkflowDefinitions.Add(new MstWorkflowDefinition { WorkflowCode = "UJI_LEMBUR", WorkflowName = "Uji", RequestType = OvertimeValueConstants.Workflow.RequestType, WorkflowStatus = OvertimeValueConstants.Workflow.ActiveStatus, IsActive = true });
        ctx.Set<MstOvertimePolicy>().Add(kebijakan);
        ctx.Set<MstOvertimeRate>().Add(new MstOvertimeRate { OvertimePolicyId = kebijakan.Id, OvertimeRateCode = "OTR-UJI", OvertimeRateName = "Uji", DayType = "Workday", TimeBand = "AllDay", CalculationMethod = "Multiplier", RateMultiplier = 1.5m, IsActive = true });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private async Task<MyOvertimePreviewResponse> Pratinjau(DateOnly tanggal, DateTime mulai, DateTime selesai, string kategori)
    {
        var ctx = _db.CreateContext();
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        var konteks = new OvertimeSelfServiceContextService(ctx);
        // Integrasi workflow hanya dipakai saat submit; pratinjau tidak menyentuhnya.
        var svc = new OvertimeSelfServiceService(ctx, konteks, new OvertimeSelfServiceQueryService(ctx, konteks), new AttendanceScheduleResolverService(ctx, setelan),
            new OvertimePolicyResolverService(ctx), new OvertimeRateResolverService(ctx), null!, new OvertimePeriodGuardService(ctx));
        var r = await svc.PreviewAsync(_user, new PreviewMyOvertimeRequest
        { OvertimeDate = tanggal, PlannedStartAt = mulai, PlannedEndAt = selesai, OvertimeCategory = kategori, Reason = "uji", WorkDescription = "uji" });
        Assert.True(r.Success, r.Message);
        return r.Data!;
    }

    private static IEnumerable<string> Kode(MyOvertimePreviewResponse r) => r.Issues.Select(x => x.Code);

    // Lembur 05.00–07.00 WIB sebelum shift Pagi: tanggal UTC = kemarin, tanggal WIB = hari kerja → tidak boleh ditolak.
    [Fact]
    public async Task Lembur_pagi_WIB_tidak_lagi_ditolak_tanggal_UTC()
    {
        var r = await Pratinjau(Hari, Wib(Hari, 5), Wib(Hari, 7), "BeforeShift");
        Assert.DoesNotContain("OVERTIME_DATE_MISMATCH", Kode(r));
        Assert.True(r.IsPolicyCompliant, string.Join(" | ", r.Issues.Select(x => $"{x.Code}: {x.Message}")));
        Assert.Equal((Hari, Hari), (r.OvertimeDate, r.PlannedEndDate));
    }

    // Jam tanpa zona dibaca WIB (sama dengan AppDateTimeHelper.ToUtc), bukan UTC.
    [Fact]
    public async Task Jam_tanpa_zona_dibaca_WIB()
    {
        var mulai = new DateTime(Hari.Year, Hari.Month, Hari.Day, 5, 0, 0, DateTimeKind.Unspecified);
        var r = await Pratinjau(Hari, mulai, mulai.AddHours(2), "BeforeShift");
        Assert.Equal(Wib(Hari, 5), DateTime.SpecifyKind(r.PlannedStartAt, DateTimeKind.Utc));
        Assert.DoesNotContain("OVERTIME_DATE_MISMATCH", Kode(r));
        Assert.DoesNotContain("CATEGORY_SCHEDULE_MISMATCH", Kode(r));
    }

    // Regresi: tanggal yang memang berbeda tetap ditolak; lembur sore dan lewat tengah malam memakai tanggal WIB.
    [Fact]
    public async Task Tanggal_berbeda_tetap_ditolak_dan_tanggal_selesai_WIB()
    {
        var besok = await Pratinjau(Hari, Wib(Hari.AddDays(1), 5), Wib(Hari.AddDays(1), 6), "BeforeShift");
        Assert.Contains("OVERTIME_DATE_MISMATCH", Kode(besok));

        var sore = await Pratinjau(Hari, Wib(Hari, 14), Wib(Hari, 16), "AfterShift");
        Assert.DoesNotContain("OVERTIME_DATE_MISMATCH", Kode(sore));
        Assert.True(sore.IsPolicyCompliant, string.Join(" | ", sore.Issues.Select(x => $"{x.Code}: {x.Message}")));

        // 23.00–01.00 WIB: selesai pada tanggal WIB berikutnya (dalam UTC keduanya masih tanggal yang sama).
        var malam = await Pratinjau(Hari, Wib(Hari, 23), Wib(Hari.AddDays(1), 1), "AfterShift");
        Assert.Equal(Hari.AddDays(1), malam.PlannedEndDate);
    }
}
