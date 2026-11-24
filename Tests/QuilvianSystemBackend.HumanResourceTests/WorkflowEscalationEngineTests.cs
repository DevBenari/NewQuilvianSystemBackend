using System.Data.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Seeders;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-442 (kontrak v29 state 33.2–33.4, VR-WF-31, VR-WF-11; HRD-DEC-232, HRD-DEC-185) — engine WorkflowService NYATA:
// eskalasi menambahkan penugasan atasan SETARA (Available, tidak wajib, EscalatedFromAssignmentId, DueAt baru, aksi Escalate);
// penugasan awal tetap boleh memutuskan; mode Any selesai oleh siapa pun di kelompok; mode All butuh slot lain; kembaran Skipped;
// eskalasi kedua, tujuan pemohon/nonaktif/sudah penyetuju, dan eskalasi otomatis sebelum DueAt ditolak; tanpa persetujuan otomatis.
// AT-HRD-WF-30 (bagian engine), AT-HRD-WF-31, AT-HRD-WF-32 (bagian engine).
public sealed class WorkflowEscalationEngineTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _unit = Guid.NewGuid();
    private readonly Dictionary<string, (Guid Wf, Guid User)> _p = new();

    private sealed class KunciPostgresKeSqlite : DbCommandInterceptor
    {
        private static void Ubah(DbCommand c)
        {
            if (!c.CommandText.Contains("FOR UPDATE", StringComparison.Ordinal)) return;
            c.CommandText = c.CommandText.Replace("public.\"", "\"", StringComparison.Ordinal).Replace("FOR UPDATE", string.Empty, StringComparison.Ordinal);
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand c, CommandEventData e, InterceptionResult<DbDataReader> r) { Ubah(c); return r; }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand c, CommandEventData e, InterceptionResult<DbDataReader> r, CancellationToken t = default) { Ubah(c); return ValueTask.FromResult(r); }
    }

    private sealed class TanpaKonflik : ITravelBudgetAndConflictValidationService
    {
        public Task ValidateTravelRequestAsync(Guid workforceProfileId, DateOnly startDate, DateOnly endDate, Guid? costCenterId, decimal estimatedTotalAmount, Guid? ignoreTravelId = null) => Task.CompletedTask;
        public Task<bool> IsEmployeeBlockedFromTravelAsync(Guid workforceProfileId) => Task.FromResult(false);
    }

    private sealed class TarifNol : IMstTravelAllowanceRateService
    {
        public Task<decimal> CalculatePerDiemRateAsync(Guid? destinationZoneId, Guid? employeeGradeId) => Task.FromResult(0m);
        public Task<decimal> CalculateTotalAllowanceAsync(Guid? destinationZoneId, Guid? employeeGradeId, int travelDayCount) => Task.FromResult(0m);
        public Task<List<TravelAllowanceRateResponseDto>> GetRatesAsync(Guid? zoneId, Guid? gradeId) => throw new NotSupportedException();
        public Task<TravelAllowanceRateResponseDto> ConfigureRateAsync(ConfigureTravelRateDto dto, Guid currentUserId) => throw new NotSupportedException();
        public Task<List<TravelDestinationZoneResponseDto>> GetZonesAsync() => throw new NotSupportedException();
    }

    public WorkflowEscalationEngineTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _unit, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        var mulai = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        foreach (var nama in new[] { "Sari", "Bima", "Kepala", "Direktur", "Kedua", "Nonaktif" })
        {
            var wf = Guid.NewGuid();
            var user = Guid.NewGuid();
            _p[nama] = (wf, user);
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = wf, ProfileCode = "P-" + nama, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
            ctx.Users.Add(new ApplicationUser { Id = user, UserName = nama, UserCode = nama.ToUpperInvariant().PadRight(8, 'X')[..8], DisplayName = nama, UserType = UserType.Employee, WorkforceProfileId = wf, IsActive = nama != "Nonaktif" });
            ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
            {
                WorkforceProfileId = wf, OrganizationUnitId = _unit, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), AssignmentType = "Primary",
                IsPrimary = true, IsManagerialAssignment = nama != "Sari", EffectiveStartDate = mulai, IsActive = true,
            });
        }

        ctx.WfpManagerAssignments.Add(new WfpManagerAssignment
        {
            WorkforceProfileId = _p["Sari"].Wf, ManagerWorkforceProfileId = _p["Bima"].Wf, ManagerType = "Direct", EffectiveStartDate = mulai,
            IsPrimaryManager = true, CanApproveRequests = true, IsActive = true
        });

        ctx.SaveChanges();
        BusinessTravelWorkflowDefinitionSeeder.SeedAsync(ctx).GetAwaiter().GetResult();
    }

    public void Dispose() => _db.Dispose();

    private ApplicationDbContext Konteks() => _db.CreateContext(new KunciPostgresKeSqlite());

    private static WorkflowService Workflow(ApplicationDbContext ctx, Guid? pelaku)
    {
        var http = pelaku.HasValue ? Pelaku.Sebagai(pelaku.Value) : Pelaku.Sebagai(null);
        var userManager = new UserManager<ApplicationUser>(
            new TanpaPeranStore(ctx), Options.Create(new IdentityOptions()), new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(), Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null!,
            NullLogger<UserManager<ApplicationUser>>.Instance);
        var handler = new BusinessTravelWorkflowLifecycleHandler(ctx, new TravelAttendanceAndRosterLockService(ctx), NullLogger<BusinessTravelWorkflowLifecycleHandler>.Instance);
        var lifecycle = new WorkflowReferenceLifecycleService(
            ctx, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!,
            NullLogger<WorkflowReferenceLifecycleService>.Instance, new IWorkflowReferenceLifecycleHandler[] { handler });
        return new WorkflowService(ctx, new HumanResourceContextService(ctx, http, userManager), userManager, http, lifecycle);
    }

    private async Task<(Guid Sppd, Guid Workflow, Guid Tugas)> AjukanAsync()
    {
        Guid id;
        using (var c = _db.CreateContext())
        {
            id = Guid.NewGuid();
            c.TrxBusinessTravelRequests.Add(new TrxBusinessTravelRequest
            {
                Id = id, TravelRequestNumber = "SPPD/UJI/" + id.ToString("N")[..6], WorkforceProfileId = _p["Sari"].Wf, TravelTypeId = Guid.NewGuid(),
                TravelTitle = "Seminar ICU", TravelPurpose = "Seminar nasional perawatan intensif", Origin = "Jakarta", Destination = "Surabaya",
                StartDate = new DateOnly(2026, 12, 1), EndDate = new DateOnly(2026, 12, 3), TravelDayCount = 3,
                EstimatedTotalAmount = 5_000_000m, EstimatedTransportationAmount = 5_000_000m, ApprovedBudgetAmount = 0m,
                TravelStatus = BusinessTravelRequestStatuses.Draft, IsActive = true, CreateBy = _p["Sari"].User, CreateDateTime = DateTime.UtcNow
            });
            c.SaveChanges();
        }

        await using var ctx = Konteks();
        var hasil = await new BusinessTravelRequestService(ctx, new TanpaKonflik(), new TarifNol(), Workflow(ctx, _p["Sari"].User), new TravelAttendanceAndRosterLockService(ctx))
            .SubmitRequestAsync(id, _p["Sari"].User, _p["Sari"].Wf);
        var wf = hasil.WorkflowInstanceId!.Value;
        var tugas = ctx.TrxWorkflowApproverAssignments.AsNoTracking().Single(x => x.WorkflowInstanceId == wf).Id;
        return (id, wf, tugas);
    }

    private void LewatBatas(Guid tugas)
    {
        using var ctx = _db.CreateContext();
        var t = ctx.TrxWorkflowApproverAssignments.Single(x => x.Id == tugas);
        t.DueAt = DateTime.UtcNow.AddHours(-1);
        ctx.SaveChanges();
    }

    // Mode All dengan dua penyetuju wajib: Bima (tugas asal) dan Kedua.
    private Guid JadikanModeAll(Guid wf)
    {
        using var ctx = _db.CreateContext();
        var step = ctx.TrxWorkflowStepInstances.Single(x => x.WorkflowInstanceId == wf);
        step.ApprovalModeSnapshot = WorkflowValueConstants.ApprovalMode.All;
        var asal = ctx.TrxWorkflowApproverAssignments.Single(x => x.WorkflowInstanceId == wf);
        var kedua = new TrxWorkflowApproverAssignment
        {
            WorkflowInstanceId = wf, WorkflowStepInstanceId = step.Id, AssignedApproverUserId = _p["Kedua"].User, AssignedApproverWorkforceProfileId = _p["Kedua"].Wf,
            ApproverSourceSnapshot = asal.ApproverSourceSnapshot, AssignmentOrder = 2, AssignmentStatus = WorkflowValueConstants.AssignmentStatus.Available,
            AvailableAt = DateTime.UtcNow, DueAt = asal.DueAt, IsRequired = true, IsCurrentAssignment = true, IsActive = true, CreateDateTime = DateTime.UtcNow
        };
        ctx.TrxWorkflowApproverAssignments.Add(kedua);
        ctx.SaveChanges();
        return kedua.Id;
    }

    private async Task<WorkflowServiceResult<Guid>> EskalasiAsync(Guid tugas, string ke, bool sistem = true, string? alasan = null)
    {
        await using var ctx = Konteks();
        return await Workflow(ctx, null).AddEscalationAssignmentAsync(tugas, _p[ke].User, alasan, sistem, sistem ? null : _p["Direktur"].User);
    }

    private async Task<WorkflowServiceResult<WorkflowInstanceDetailResponse>> SetujuiAsync(Guid wf, Guid tugas, string oleh)
    {
        await using var ctx = Konteks();
        return await Workflow(ctx, _p[oleh].User).ApproveAsync(wf, tugas, new WorkflowApproveRequest());
    }

    private TrxWorkflowApproverAssignment Tugas(Guid id)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxWorkflowApproverAssignments.AsNoTracking().Single(x => x.Id == id);
    }

    private (string Sppd, string Workflow) Status(Guid sppd, Guid wf)
    {
        using var ctx = _db.CreateContext();
        return (ctx.TrxBusinessTravelRequests.AsNoTracking().Single(x => x.Id == sppd).TravelStatus,
            ctx.TrxWorkflowInstances.AsNoTracking().Single(x => x.Id == wf).WorkflowStatus);
    }

    private List<TrxApprovalAction> Aksi(Guid wf, string jenis)
    {
        using var ctx = _db.CreateContext();
        return ctx.Set<TrxApprovalAction>().AsNoTracking().Where(x => x.WorkflowInstanceId == wf && x.ActionType == jenis).ToList();
    }

    // AC1 — eskalasi otomatis membuat penugasan atasan setara; penugasan awal tetap terbuka; tidak ada persetujuan otomatis.
    [Fact]
    public async Task Eskalasi_otomatis_menambah_penugasan_atasan_setara_tanpa_persetujuan_otomatis()
    {
        var (sppd, wf, tugas) = await AjukanAsync();
        LewatBatas(tugas);

        var r = await EskalasiAsync(tugas, "Kepala");

        Assert.True(r.Success, r.Message);
        var baru = Tugas(r.Data);
        Assert.Equal(_p["Kepala"].User, baru.AssignedApproverUserId);
        Assert.Equal(WorkflowValueConstants.AssignmentStatus.Available, baru.AssignmentStatus);
        Assert.False(baru.IsRequired);
        Assert.True(baru.IsCurrentAssignment);
        Assert.Equal(tugas, baru.EscalatedFromAssignmentId);
        Assert.Equal(2, baru.AssignmentOrder);
        Assert.NotNull(baru.DueAt);
        Assert.True(baru.DueAt > DateTime.UtcNow.AddHours(47));

        var asal = Tugas(tugas);
        Assert.Equal(WorkflowValueConstants.AssignmentStatus.Available, asal.AssignmentStatus);
        Assert.True(asal.IsCurrentAssignment);

        var eskalasi = Assert.Single(Aksi(wf, WorkflowValueConstants.ActionType.Escalate));
        Assert.True(eskalasi.IsSystemAction);
        Assert.Equal(tugas, eskalasi.WorkflowApproverAssignmentId);
        Assert.Contains("Kepala", eskalasi.Comment);
        Assert.Null(eskalasi.ActualActionByUserId);

        Assert.Equal((BusinessTravelRequestStatuses.Submitted, WorkflowValueConstants.WorkflowStatus.InProgress), Status(sppd, wf));
    }

    // AC2 (Any) — atasan hasil eskalasi menyetujui: tahap selesai, SPPD Disetujui, penugasan awal Skipped.
    [Fact]
    public async Task Mode_Any_atasan_eskalasi_menyetujui_menyelesaikan_tahap()
    {
        var (sppd, wf, tugas) = await AjukanAsync();
        LewatBatas(tugas);
        var eskalasi = (await EskalasiAsync(tugas, "Kepala")).Data;

        var r = await SetujuiAsync(wf, eskalasi, "Kepala");

        Assert.True(r.Success, r.Message);
        Assert.Equal(BusinessTravelRequestStatuses.Approved, Status(sppd, wf).Sppd);
        Assert.Equal(WorkflowValueConstants.AssignmentStatus.Skipped, Tugas(tugas).AssignmentStatus);
        Assert.Equal(WorkflowValueConstants.AssignmentStatus.Approved, Tugas(eskalasi).AssignmentStatus);
    }

    // AC2 (Any) — penyetuju awal tetap dapat menyetujui sesudah eskalasi; penugasan eskalasi Skipped.
    [Fact]
    public async Task Mode_Any_penyetuju_awal_tetap_dapat_menyetujui_sesudah_eskalasi()
    {
        var (sppd, wf, tugas) = await AjukanAsync();
        LewatBatas(tugas);
        var eskalasi = (await EskalasiAsync(tugas, "Kepala")).Data;

        var r = await SetujuiAsync(wf, tugas, "Bima");

        Assert.True(r.Success, r.Message);
        Assert.Equal(BusinessTravelRequestStatuses.Approved, Status(sppd, wf).Sppd);
        Assert.Equal(WorkflowValueConstants.AssignmentStatus.Skipped, Tugas(eskalasi).AssignmentStatus);
    }

    // AC2 (All) — slot eskalasi disetujui tidak cukup sampai penyetuju wajib lain menyetujui; kembaran Skipped dengan aksi Skip.
    [Fact]
    public async Task Mode_All_butuh_slot_lain_dan_kembaran_ditutup_Skipped()
    {
        var (sppd, wf, tugas) = await AjukanAsync();
        var kedua = JadikanModeAll(wf);
        LewatBatas(tugas);
        var eskalasi = (await EskalasiAsync(tugas, "Kepala")).Data;

        var r1 = await SetujuiAsync(wf, eskalasi, "Kepala");
        Assert.True(r1.Success, r1.Message);
        Assert.Equal((BusinessTravelRequestStatuses.Submitted, WorkflowValueConstants.WorkflowStatus.InProgress), Status(sppd, wf));
        Assert.Equal(WorkflowValueConstants.AssignmentStatus.Skipped, Tugas(tugas).AssignmentStatus);
        var skip = Assert.Single(Aksi(wf, WorkflowValueConstants.ActionType.Skip));
        Assert.Equal(tugas, skip.WorkflowApproverAssignmentId);
        Assert.True(skip.IsSystemAction);
        Assert.Equal(WorkflowValueConstants.AssignmentStatus.Available, Tugas(kedua).AssignmentStatus);

        var r2 = await SetujuiAsync(wf, kedua, "Kedua");
        Assert.True(r2.Success, r2.Message);
        Assert.Equal(BusinessTravelRequestStatuses.Approved, Status(sppd, wf).Sppd);
    }

    // AC2 (All) — tanpa eskalasi, satu persetujuan dari dua penyetuju wajib tidak menyelesaikan tahap (perilaku asal tetap).
    [Fact]
    public async Task Mode_All_tanpa_eskalasi_tetap_butuh_semua_penyetuju()
    {
        var (sppd, wf, tugas) = await AjukanAsync();
        var kedua = JadikanModeAll(wf);

        Assert.True((await SetujuiAsync(wf, tugas, "Bima")).Success);
        Assert.Equal(BusinessTravelRequestStatuses.Submitted, Status(sppd, wf).Sppd);
        Assert.Equal(WorkflowValueConstants.AssignmentStatus.Available, Tugas(kedua).AssignmentStatus);
        Assert.Empty(Aksi(wf, WorkflowValueConstants.ActionType.Skip));

        Assert.True((await SetujuiAsync(wf, kedua, "Kedua")).Success);
        Assert.Equal(BusinessTravelRequestStatuses.Approved, Status(sppd, wf).Sppd);
    }

    // AC3 — eskalasi kedua atas penugasan sama ditolak; rantai: penugasan eskalasi yang lewat batas dapat dieskalasi lagi.
    [Fact]
    public async Task Eskalasi_sekali_per_penugasan_dan_rantai_berlanjut()
    {
        var (sppd, wf, tugas) = await AjukanAsync();
        LewatBatas(tugas);
        var eskalasi = (await EskalasiAsync(tugas, "Kepala")).Data;

        var kedua = await EskalasiAsync(tugas, "Direktur");
        Assert.False(kedua.Success);
        Assert.Equal(StatusCodes.Status409Conflict, kedua.StatusCode);
        Assert.Equal("Penugasan ini sudah pernah dieskalasi.", kedua.Message);

        LewatBatas(eskalasi);
        var rantai = await EskalasiAsync(eskalasi, "Direktur");
        Assert.True(rantai.Success, rantai.Message);
        Assert.Equal(eskalasi, Tugas(rantai.Data).EscalatedFromAssignmentId);

        Assert.True((await SetujuiAsync(wf, rantai.Data, "Direktur")).Success);
        Assert.Equal(BusinessTravelRequestStatuses.Approved, Status(sppd, wf).Sppd);
        Assert.Equal(WorkflowValueConstants.AssignmentStatus.Skipped, Tugas(tugas).AssignmentStatus);
        Assert.Equal(WorkflowValueConstants.AssignmentStatus.Skipped, Tugas(eskalasi).AssignmentStatus);
    }

    // AC3 — tujuan tidak sah dan eskalasi otomatis sebelum batas waktu ditolak tanpa perubahan data.
    [Theory]
    [InlineData("Sari", true, true, 400)]      // pemohon
    [InlineData("Nonaktif", true, true, 400)]  // akun nonaktif
    [InlineData("Bima", true, true, 409)]      // sudah penyetuju pada tahap ini
    [InlineData("Kepala", true, false, 409)]   // otomatis sebelum DueAt
    public async Task Eskalasi_ditolak_untuk_tujuan_tidak_sah_atau_sebelum_batas(string ke, bool sistem, bool lewat, int kode)
    {
        var (_, wf, tugas) = await AjukanAsync();
        if (lewat) LewatBatas(tugas);

        var r = await EskalasiAsync(tugas, ke, sistem);

        Assert.False(r.Success);
        Assert.Equal(kode, r.StatusCode);
        using var ctx = _db.CreateContext();
        Assert.Single(ctx.TrxWorkflowApproverAssignments.AsNoTracking().Where(x => x.WorkflowInstanceId == wf));
        Assert.Empty(Aksi(wf, WorkflowValueConstants.ActionType.Escalate));
    }

    // AC3 — eskalasi manual oleh HR boleh sebelum batas waktu dan mencatat pelaku serta alasan.
    [Fact]
    public async Task Eskalasi_manual_boleh_sebelum_batas_dan_mencatat_pelaku()
    {
        var (_, wf, tugas) = await AjukanAsync();

        var r = await EskalasiAsync(tugas, "Kepala", sistem: false, alasan: "Kepala Ruang cuti mendadak.");

        Assert.True(r.Success, r.Message);
        var a = Assert.Single(Aksi(wf, WorkflowValueConstants.ActionType.Escalate));
        Assert.False(a.IsSystemAction);
        Assert.Equal(_p["Direktur"].User, a.ActualActionByUserId);
        Assert.Equal("Kepala Ruang cuti mendadak.", a.Comment);
    }

    // AC3 — penugasan yang sudah diputuskan tidak dapat dieskalasi.
    [Fact]
    public async Task Penugasan_yang_sudah_diputuskan_tidak_dapat_dieskalasi()
    {
        var (_, wf, tugas) = await AjukanAsync();
        Assert.True((await SetujuiAsync(wf, tugas, "Bima")).Success);
        LewatBatas(tugas);

        var r = await EskalasiAsync(tugas, "Kepala");

        Assert.Equal(StatusCodes.Status409Conflict, r.StatusCode);
    }
}
