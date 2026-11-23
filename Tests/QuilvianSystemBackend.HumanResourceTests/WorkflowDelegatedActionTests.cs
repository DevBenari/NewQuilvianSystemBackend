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
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
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

// BE-HRD-441 (kontrak v29 VR-WF-30, VR-WF-25, VR-WF-02, INT-HR-113; HRD-DEC-234, HRD-DEC-230) — engine WorkflowService NYATA:
// Plh aktif boleh menyetujui/menolak/mengembalikan penugasan pejabat yang SUDAH ADA sebelum delegasi didaftarkan, tanpa
// memindahkan penugasan; jejak aksi mencatat pelaku Plh dan pejabat definitif; delegasi kedaluwarsa/dicabut/berbeda cakupan
// ditolak; Plh yang adalah pemohon ditolak; pejabat definitif tetap dapat bertindak; tanpa DigitalSignatureHash.
// AT-HRD-WF-33 (bagian engine), AT-HRD-WF-34, AT-HRD-WF-28 (bagian engine).
public sealed class WorkflowDelegatedActionTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _unit = Guid.NewGuid();
    private readonly Guid _alasanTolak = Guid.NewGuid();
    private readonly Guid _pegawaiBima = Guid.NewGuid();
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

    public WorkflowDelegatedActionTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _unit, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        var mulai = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        foreach (var nama in new[] { "Sari", "Bima", "Wakil", "Lain" })
        {
            var wf = Guid.NewGuid();
            var user = Guid.NewGuid();
            _p[nama] = (wf, user);
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = wf, ProfileCode = "P-" + nama, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
            ctx.Users.Add(new ApplicationUser { Id = user, UserName = nama, UserCode = nama.ToUpperInvariant().PadRight(8, 'X')[..8], DisplayName = nama, UserType = UserType.Employee, WorkforceProfileId = wf, IsActive = true });
            ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
            {
                WorkforceProfileId = wf, OrganizationUnitId = _unit, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), AssignmentType = "Primary",
                IsPrimary = true, IsManagerialAssignment = nama == "Bima", EffectiveStartDate = mulai, IsActive = true,
            });
        }

        // Sari dan Wakil beratasan langsung Bima (Kepala Ruang).
        foreach (var nama in new[] { "Sari", "Wakil" })
        {
            ctx.WfpManagerAssignments.Add(new WfpManagerAssignment
            {
                WorkforceProfileId = _p[nama].Wf, ManagerWorkforceProfileId = _p["Bima"].Wf, ManagerType = "Direct", EffectiveStartDate = mulai,
                IsPrimaryManager = true, CanApproveRequests = true, IsActive = true
            });
        }

        ctx.Set<MstRejectionReason>().Add(new MstRejectionReason
        {
            Id = _alasanTolak, RequestType = BusinessTravelWorkflowCodes.RequestType, ReasonCode = "TRV-TOLAK-01", ReasonName = "Kegiatan tidak relevan dengan tugas",
            RejectAction = WorkflowValueConstants.RejectAction.CloseRequest, IsCommentRequired = true, AllowResubmit = false, IsActive = true
        });

        ctx.SaveChanges();
        BusinessTravelWorkflowDefinitionSeeder.SeedAsync(ctx).GetAwaiter().GetResult();
    }

    public void Dispose() => _db.Dispose();

    private ApplicationDbContext Konteks() => _db.CreateContext(new KunciPostgresKeSqlite());

    private static WorkflowService Workflow(ApplicationDbContext ctx, Guid pelaku)
    {
        var http = Pelaku.Sebagai(pelaku);
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

    private Guid Sppd(string pemohon)
    {
        using var ctx = _db.CreateContext();
        var id = Guid.NewGuid();
        ctx.TrxBusinessTravelRequests.Add(new TrxBusinessTravelRequest
        {
            Id = id, TravelRequestNumber = "SPPD/UJI/" + id.ToString("N")[..6], WorkforceProfileId = _p[pemohon].Wf, TravelTypeId = Guid.NewGuid(),
            TravelTitle = "Seminar ICU", TravelPurpose = "Seminar nasional perawatan intensif", Origin = "Jakarta", Destination = "Surabaya",
            StartDate = new DateOnly(2026, 12, 1), EndDate = new DateOnly(2026, 12, 3), TravelDayCount = 3,
            EstimatedTotalAmount = 5_000_000m, EstimatedTransportationAmount = 5_000_000m, ApprovedBudgetAmount = 0m,
            TravelStatus = BusinessTravelRequestStatuses.Draft, IsActive = true, CreateBy = _p[pemohon].User, CreateDateTime = DateTime.UtcNow
        });
        ctx.SaveChanges();
        return id;
    }

    // Ajukan SPPD: engine membuat penugasan untuk Bima SEBELUM delegasi apa pun didaftarkan.
    private async Task<(Guid Sppd, Guid Workflow, Guid Tugas)> AjukanAsync(string pemohon = "Sari")
    {
        var id = Sppd(pemohon);
        await using var ctx = Konteks();
        var hasil = await new BusinessTravelRequestService(ctx, new TanpaKonflik(), new TarifNol(), Workflow(ctx, _p[pemohon].User), new TravelAttendanceAndRosterLockService(ctx))
            .SubmitRequestAsync(id, _p[pemohon].User, _p[pemohon].Wf);
        var wf = hasil.WorkflowInstanceId!.Value;
        var tugas = ctx.TrxWorkflowApproverAssignments.AsNoTracking().Single(x => x.WorkflowInstanceId == wf && x.AssignedApproverUserId == _p["Bima"].User);
        Assert.False(tugas.IsDelegated);
        return (id, wf, tugas.Id);
    }

    private TrxApprovalDelegation Delegasi(string dari = "Bima", string ke = "Wakil", string status = WorkflowValueConstants.DelegationStatus.Active,
        int mulaiHari = -1, int akhirHari = 3, Guid? definisi = null, bool semua = true)
    {
        using var ctx = _db.CreateContext();
        var d = new TrxApprovalDelegation
        {
            DelegatorUserId = _p[dari].User, DelegateUserId = _p[ke].User, DelegatorWorkforceProfileId = _p[dari].Wf, DelegateWorkforceProfileId = _p[ke].Wf,
            PrincipalEmployeeId = _pegawaiBima, DelegationNumber = "DLG-" + Guid.NewGuid().ToString("N")[..6], DelegationStatus = status,
            EffectiveStartAt = DateTime.UtcNow.AddDays(mulaiHari), EffectiveEndAt = DateTime.UtcNow.AddDays(akhirHari),
            AppliesToAllWorkflows = semua, WorkflowDefinitionId = definisi, IsActive = true, CreateDateTime = DateTime.UtcNow
        };
        ctx.Set<TrxApprovalDelegation>().Add(d);
        ctx.SaveChanges();
        return d;
    }

    private TrxApprovalAction AksiTerakhir(Guid wf, string jenis)
    {
        using var ctx = _db.CreateContext();
        return ctx.Set<TrxApprovalAction>().AsNoTracking().Where(x => x.WorkflowInstanceId == wf && x.ActionType == jenis).OrderByDescending(x => x.ActionAt).First();
    }

    private TrxWorkflowApproverAssignment Tugas(Guid id)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxWorkflowApproverAssignments.AsNoTracking().Single(x => x.Id == id);
    }

    private string StatusSppd(Guid id)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxBusinessTravelRequests.AsNoTracking().Single(x => x.Id == id).TravelStatus;
    }

    // AC1 — Plh aktif menyetujui penugasan yang dibuat sebelum delegasi; jejak berisi pelaku Plh dan pejabat definitif.
    [Fact]
    public async Task Plh_aktif_menyetujui_penugasan_yang_sudah_ada_dan_jejak_mencatat_pejabat_definitif()
    {
        var (sppd, wf, tugas) = await AjukanAsync();
        var d = Delegasi();

        await using var ctx = Konteks();
        var r = await Workflow(ctx, _p["Wakil"].User).ApproveAsync(wf, tugas, new WorkflowApproveRequest { Comment = "Disetujui atas nama Kepala Ruang." });

        Assert.True(r.Success, r.Message);
        Assert.Equal(BusinessTravelRequestStatuses.Approved, StatusSppd(sppd));
        var t = Tugas(tugas);
        Assert.Equal(WorkflowValueConstants.AssignmentStatus.Approved, t.AssignmentStatus);
        Assert.Equal(_p["Bima"].User, t.AssignedApproverUserId);   // penugasan tidak dipindahkan

        var a = AksiTerakhir(wf, WorkflowValueConstants.ActionType.Approve);
        Assert.Equal(_p["Wakil"].User, a.ActualActionByUserId);
        Assert.True(a.IsDelegated);
        Assert.Equal(d.Id, a.ApprovalDelegationId);
        Assert.Equal(_p["Bima"].User, a.DelegatedFromUserId);
        Assert.Equal(_p["Bima"].Wf, a.DelegatedFromWorkforceProfileId);
        Assert.Equal(_pegawaiBima, a.OnBehalfOfEmployeeId);
        Assert.Equal(_p["Bima"].User, a.AssignedApproverUserId);
        Assert.Null(a.DigitalSignatureHash);
    }

    // AC1 — Plh juga dapat menolak (alasan berkode) dan mengembalikan untuk revisi.
    [Fact]
    public async Task Plh_aktif_dapat_menolak_dan_mengembalikan()
    {
        var (sppdTolak, wfTolak, tugasTolak) = await AjukanAsync();
        var (sppdRevisi, wfRevisi, tugasRevisi) = await AjukanAsync();
        Delegasi();   // didaftarkan sesudah kedua penugasan dibuat
        await using (var ctx = Konteks())
        {
            var r = await Workflow(ctx, _p["Wakil"].User).RejectAsync(wfTolak, tugasTolak, new WorkflowRejectRequest { RejectionReasonId = _alasanTolak, Comment = "Kegiatan tidak relevan untuk ICU." });
            Assert.True(r.Success, r.Message);
        }

        Assert.Equal(BusinessTravelRequestStatuses.Rejected, StatusSppd(sppdTolak));
        var tolak = AksiTerakhir(wfTolak, WorkflowValueConstants.ActionType.Reject);
        Assert.True(tolak.IsDelegated);
        Assert.Equal(_p["Wakil"].User, tolak.ActualActionByUserId);
        Assert.Equal(_p["Bima"].User, tolak.DelegatedFromUserId);
        Assert.Equal(_alasanTolak, tolak.ActionReasonId);

        await using (var ctx = Konteks())
        {
            var r = await Workflow(ctx, _p["Wakil"].User).RequestRevisionAsync(wfRevisi, tugasRevisi, new WorkflowRequestRevisionRequest { Comment = "Lengkapi undangan seminar." });
            Assert.True(r.Success, r.Message);
        }

        Assert.Equal(BusinessTravelRequestStatuses.NeedRevision, StatusSppd(sppdRevisi));
        var revisi = AksiTerakhir(wfRevisi, WorkflowValueConstants.ActionType.RequestRevision);
        Assert.True(revisi.IsDelegated);
        Assert.Equal(_p["Wakil"].User, revisi.ActualActionByUserId);
    }

    // AC2 — delegasi berakhir, dicabut, belum mulai, atau cakupan definisi lain: Plh ditolak 403, data tidak berubah.
    [Theory]
    [InlineData("berakhir")]
    [InlineData("dicabut")]
    [InlineData("belum-mulai")]
    [InlineData("definisi-lain")]
    [InlineData("tanpa-delegasi")]
    public async Task Plh_tanpa_delegasi_aktif_yang_mencakup_ditolak(string kasus)
    {
        var (sppd, wf, tugas) = await AjukanAsync();
        switch (kasus)
        {
            case "berakhir": Delegasi(mulaiHari: -5, akhirHari: -1); break;
            case "dicabut": Delegasi(status: WorkflowValueConstants.DelegationStatus.Revoked); break;
            case "belum-mulai": Delegasi(mulaiHari: 1, akhirHari: 5); break;
            case "definisi-lain": Delegasi(semua: false, definisi: Guid.NewGuid()); break;
        }

        await using var ctx = Konteks();
        var r = await Workflow(ctx, _p["Wakil"].User).ApproveAsync(wf, tugas, new WorkflowApproveRequest());

        Assert.False(r.Success);
        Assert.Equal(StatusCodes.Status403Forbidden, r.StatusCode);
        Assert.Equal("User login bukan approver yang ditugaskan.", r.Message);
        Assert.Equal(BusinessTravelRequestStatuses.Submitted, StatusSppd(sppd));
        Assert.Equal(WorkflowValueConstants.AssignmentStatus.Available, Tugas(tugas).AssignmentStatus);
    }

    // AC2 — delegasi yang menunjuk orang lain tidak memberi hak kepada pengguna login.
    [Fact]
    public async Task Delegasi_untuk_orang_lain_tidak_memberi_hak()
    {
        var (_, wf, tugas) = await AjukanAsync();
        Delegasi(ke: "Lain");

        await using var ctx = Konteks();
        var r = await Workflow(ctx, _p["Wakil"].User).RejectAsync(wf, tugas, new WorkflowRejectRequest { RejectionReasonId = _alasanTolak, Comment = "Uji." });

        Assert.Equal(StatusCodes.Status403Forbidden, r.StatusCode);
    }

    // AC3 — Plh yang adalah pemohon tidak boleh memutuskan permohonannya sendiri (VR-WF-02).
    [Fact]
    public async Task Plh_yang_juga_pemohon_ditolak()
    {
        var (sppd, wf, tugas) = await AjukanAsync("Wakil");
        Delegasi();

        await using var ctx = Konteks();
        var r = await Workflow(ctx, _p["Wakil"].User).ApproveAsync(wf, tugas, new WorkflowApproveRequest());

        Assert.False(r.Success);
        Assert.Equal(StatusCodes.Status403Forbidden, r.StatusCode);
        Assert.Equal("Pejabat tidak boleh menyetujui permohonan sendiri.", r.Message);
        Assert.Equal(BusinessTravelRequestStatuses.Submitted, StatusSppd(sppd));
    }

    // AC4 — pejabat definitif tetap dapat bertindak selama delegasi aktif; jejaknya bukan delegasi.
    [Fact]
    public async Task Pejabat_definitif_tetap_dapat_menyetujui_selama_delegasi_aktif()
    {
        var (sppd, wf, tugas) = await AjukanAsync();
        Delegasi();

        await using var ctx = Konteks();
        var r = await Workflow(ctx, _p["Bima"].User).ApproveAsync(wf, tugas, new WorkflowApproveRequest());

        Assert.True(r.Success, r.Message);
        Assert.Equal(BusinessTravelRequestStatuses.Approved, StatusSppd(sppd));
        var a = AksiTerakhir(wf, WorkflowValueConstants.ActionType.Approve);
        Assert.False(a.IsDelegated);
        Assert.Null(a.ApprovalDelegationId);
        Assert.Equal(_p["Bima"].User, a.ActualActionByUserId);
        Assert.Null(a.DigitalSignatureHash);
    }

    // AC4 — delegasi yang sudah ada saat penugasan dibuat tetap dialihkan seperti sebelumnya (helper bersama tidak mengubah perilaku).
    [Fact]
    public async Task Delegasi_saat_penugasan_dibuat_tetap_mengalihkan_penugasan()
    {
        var d = Delegasi();
        var id = Sppd("Sari");
        await using var ctx = Konteks();
        var hasil = await new BusinessTravelRequestService(ctx, new TanpaKonflik(), new TarifNol(), Workflow(ctx, _p["Sari"].User), new TravelAttendanceAndRosterLockService(ctx))
            .SubmitRequestAsync(id, _p["Sari"].User, _p["Sari"].Wf);

        var t = ctx.TrxWorkflowApproverAssignments.AsNoTracking().Single(x => x.WorkflowInstanceId == hasil.WorkflowInstanceId);
        Assert.True(t.IsDelegated);
        Assert.Equal(_p["Wakil"].User, t.AssignedApproverUserId);
        Assert.Equal(_p["Bima"].User, t.OriginalApproverUserId);
        Assert.Equal(d.Id, t.ApprovalDelegationId);
    }
}
