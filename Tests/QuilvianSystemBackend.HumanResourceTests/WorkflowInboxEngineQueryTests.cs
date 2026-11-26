using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Controllers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-443 (kontrak v29 §35.1, §35.6; INT-HR-111, INT-HR-113; HRD-DEC-229, HRD-DEC-234) — Kotak Masuk Persetujuan Terpadu
// membaca penugasan engine WorkflowService NYATA: SPPD diajukan lewat BusinessTravelRequestService, cuti sebagai instance
// engine kedua. Antrean/ringkasan per akun, penugasan engine lama tidak tampil, Plh aktif melihat tugas pejabat
// (isDelegated), myAction pada detail instance, alasan penolakan sama dengan kotak masuk lama.
// AT-HRD-WF-23, AT-HRD-WF-33 (bagian baca).
public sealed class WorkflowInboxEngineQueryTests : IDisposable
{
    private const string JenisCuti = "LEAVE_REQUEST";

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _unit = Guid.NewGuid();
    private readonly Guid _alasanTolak = Guid.NewGuid();
    private readonly Guid _alasanLain = Guid.NewGuid();
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

    public WorkflowInboxEngineQueryTests()
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

        ctx.Set<MstRejectionReason>().AddRange(
            new MstRejectionReason
            {
                Id = _alasanTolak, RequestType = BusinessTravelWorkflowCodes.RequestType, ReasonCode = "TRV-TOLAK-01", ReasonName = "Kegiatan tidak relevan dengan tugas",
                RejectAction = WorkflowValueConstants.RejectAction.CloseRequest, IsCommentRequired = true, AllowResubmit = false, IsActive = true, SortOrder = 1
            },
            new MstRejectionReason
            {
                Id = _alasanLain, RequestType = JenisCuti, ReasonCode = "CUTI-TOLAK-01", ReasonName = "Kuota cuti habis",
                RejectAction = WorkflowValueConstants.RejectAction.CloseRequest, IsActive = true, SortOrder = 1
            });

        ctx.SaveChanges();
        BusinessTravelWorkflowDefinitionSeeder.SeedAsync(ctx).GetAwaiter().GetResult();
    }

    public void Dispose() => _db.Dispose();

    private ApplicationDbContext Konteks() => _db.CreateContext(new KunciPostgresKeSqlite());

    private static UserManager<ApplicationUser> Pengguna(ApplicationDbContext ctx) => new(
        new TanpaPeranStore(ctx), Options.Create(new IdentityOptions()), new PasswordHasher<ApplicationUser>(),
        Array.Empty<IUserValidator<ApplicationUser>>(), Array.Empty<IPasswordValidator<ApplicationUser>>(),
        new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null!,
        NullLogger<UserManager<ApplicationUser>>.Instance);

    private static WorkflowService Workflow(ApplicationDbContext ctx, Guid pelaku)
    {
        var http = Pelaku.Sebagai(pelaku);
        var userManager = Pengguna(ctx);
        var handler = new BusinessTravelWorkflowLifecycleHandler(ctx, new TravelAttendanceAndRosterLockService(ctx), NullLogger<BusinessTravelWorkflowLifecycleHandler>.Instance);
        var lifecycle = new WorkflowReferenceLifecycleService(
            ctx, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!,
            NullLogger<WorkflowReferenceLifecycleService>.Instance, new IWorkflowReferenceLifecycleHandler[] { handler });
        return new WorkflowService(ctx, new HumanResourceContextService(ctx, http, userManager), userManager, http, lifecycle);
    }

    private ApprovalInboxQueryService Antrean() => new(_db.CreateContext());

    private async Task<PagedResult<ApprovalInboxItemV2ResponseDto>> AntreanAsync(
        string nama, string? jenis = null, bool? mendesak = null, bool? plh = null)
        => await Antrean().GetInboxAsync(_p[nama].User, null, jenis, mendesak, plh, 1, 25);

    private WorkflowInboxAndInstancesController Kontroler(string nama)
    {
        var ctx = _db.CreateContext();
        return new WorkflowInboxAndInstancesController(new ApprovalInboxQueryService(ctx), new WorkflowInstanceCreationService(ctx, null!, null!, null!, null!))
        {
            ControllerContext = new ControllerContext { HttpContext = Pelaku.Sebagai(_p[nama].User).HttpContext! }
        };
    }

    private async Task<WorkflowMyActionDto?> AksiSayaAsync(string nama, Guid wf)
    {
        var hasil = Assert.IsType<OkObjectResult>(await Kontroler(nama).GetInstanceDetail(wf));
        return Assert.IsType<ApiResponse<WorkflowInstanceDetailV2ResponseDto>>(hasil.Value).Data!.MyAction;
    }

    // Ajukan SPPD lewat layanan modul: engine membuat penugasan untuk Bima.
    private async Task<(Guid Sppd, string Nomor, Guid Workflow, Guid Tugas)> AjukanSppdAsync(string pemohon = "Sari")
    {
        var id = Guid.NewGuid();
        var nomor = "SPPD/UJI/" + id.ToString("N")[..6];
        await using (var tulis = _db.CreateContext())
        {
            tulis.TrxBusinessTravelRequests.Add(new TrxBusinessTravelRequest
            {
                Id = id, TravelRequestNumber = nomor, WorkforceProfileId = _p[pemohon].Wf, TravelTypeId = Guid.NewGuid(),
                TravelTitle = "Seminar ICU", TravelPurpose = "Seminar nasional perawatan intensif", Origin = "Jakarta", Destination = "Surabaya",
                StartDate = new DateOnly(2026, 12, 1), EndDate = new DateOnly(2026, 12, 3), TravelDayCount = 3,
                EstimatedTotalAmount = 5_000_000m, EstimatedTransportationAmount = 5_000_000m, ApprovedBudgetAmount = 0m,
                TravelStatus = BusinessTravelRequestStatuses.Draft, IsActive = true, CreateBy = _p[pemohon].User, CreateDateTime = DateTime.UtcNow
            });
            await tulis.SaveChangesAsync();
        }

        await using var ctx = Konteks();
        var hasil = await new BusinessTravelRequestService(ctx, new TanpaKonflik(), new TarifNol(), Workflow(ctx, _p[pemohon].User), new TravelAttendanceAndRosterLockService(ctx))
            .SubmitRequestAsync(id, _p[pemohon].User, _p[pemohon].Wf);
        var wf = hasil.WorkflowInstanceId!.Value;
        var tugas = ctx.TrxWorkflowApproverAssignments.AsNoTracking().Single(x => x.WorkflowInstanceId == wf && x.AssignedApproverUserId == _p["Bima"].User);
        return (id, nomor, wf, tugas.Id);
    }

    // Instance engine (cuti) atau engine V2 lama (ApproverEmployeeId terisi) dengan satu penugasan untuk penyetuju.
    // Keduanya diberi definisi nyata agar yang membedakan hanya ApproverEmployeeId (aturan kontrak §35.1).
    private (Guid Workflow, Guid Tugas) Sisip(string jenis, string pemohon, string penyetuju, bool engineLama = false,
        string statusTugas = WorkflowValueConstants.AssignmentStatus.Available, DateTime? jatuhTempo = null, Guid? akunPenyetuju = null)
    {
        using var ctx = _db.CreateContext();
        var id = Guid.NewGuid();
        var step = Guid.NewGuid();
        var tugas = Guid.NewGuid();
        var definisi = Guid.NewGuid();
        ctx.Set<MstWorkflowDefinition>().Add(new MstWorkflowDefinition
        {
            Id = definisi, WorkflowCode = "UJI_CUTI_" + definisi.ToString("N")[..6], WorkflowName = "Persetujuan Cuti", RequestType = jenis,
            WorkflowCategory = "Leave", Version = 1, WorkflowStatus = "Active", IsActive = true, CreateDateTime = DateTime.UtcNow
        });

        ctx.TrxWorkflowInstances.Add(new TrxWorkflowInstance
        {
            Id = id, WorkflowDefinitionId = definisi, ReferenceType = jenis, ReferenceId = Guid.NewGuid(),
            RequestNumber = "WF-UJI-" + id.ToString("N")[..6], ExternalReferenceNumber = "CT-UJI-" + id.ToString("N")[..4],
            RequestedByUserId = _p[pemohon].User, RequestedByWorkforceProfileId = _p[pemohon].Wf, WorkflowStatus = WorkflowValueConstants.WorkflowStatus.InProgress,
            SourceChannel = WorkflowValueConstants.SourceChannel.Web, StartedAt = DateTime.UtcNow, SubmittedAt = DateTime.UtcNow, IsActive = true, CreateDateTime = DateTime.UtcNow
        });
        ctx.TrxWorkflowStepInstances.Add(new TrxWorkflowStepInstance
        {
            Id = step, WorkflowInstanceId = id, WorkflowStepId = Guid.NewGuid(), StepOrder = 1, StepCodeSnapshot = "S1", StepNameSnapshot = "Atasan", StepTypeSnapshot = "Approval",
            ApprovalModeSnapshot = "Any", ApproverSourceSnapshot = "RequesterManager", StepStatus = WorkflowValueConstants.StepStatus.InProgress, IsCurrentStep = true,
            IsActive = true, CreateDateTime = DateTime.UtcNow
        });
        ctx.TrxWorkflowApproverAssignments.Add(new TrxWorkflowApproverAssignment
        {
            Id = tugas, WorkflowInstanceId = id, WorkflowStepInstanceId = step, AssignedApproverUserId = akunPenyetuju ?? _p[penyetuju].User,
            ApproverEmployeeId = engineLama ? Guid.NewGuid() : null, ApproverSourceSnapshot = "RequesterManager", AssignmentStatus = statusTugas,
            DueAt = jatuhTempo ?? DateTime.UtcNow.AddDays(2), IsCurrentAssignment = true, IsActive = true, CreateDateTime = DateTime.UtcNow
        });
        ctx.SaveChanges();
        return (id, tugas);
    }

    private void Delegasi(string dari = "Bima", string ke = "Wakil", string status = WorkflowValueConstants.DelegationStatus.Active,
        int mulaiHari = -1, int akhirHari = 3, Guid? definisi = null, bool semua = true)
    {
        using var ctx = _db.CreateContext();
        ctx.Set<TrxApprovalDelegation>().Add(new TrxApprovalDelegation
        {
            DelegatorUserId = _p[dari].User, DelegateUserId = _p[ke].User, DelegatorWorkforceProfileId = _p[dari].Wf, DelegateWorkforceProfileId = _p[ke].Wf,
            DelegationNumber = "DLG-" + Guid.NewGuid().ToString("N")[..6], DelegationStatus = status,
            EffectiveStartAt = DateTime.UtcNow.AddDays(mulaiHari), EffectiveEndAt = DateTime.UtcNow.AddDays(akhirHari),
            AppliesToAllWorkflows = semua, WorkflowDefinitionId = definisi, IsActive = true, CreateDateTime = DateTime.UtcNow
        });
        ctx.SaveChanges();
    }

    private void TandaiMassal(bool boleh)
    {
        using var ctx = _db.CreateContext();
        var d = ctx.Set<MstWorkflowDefinition>().Single(x => x.WorkflowCode == BusinessTravelWorkflowCodes.DefinitionCode);
        d.AllowBulkApproval = boleh;
        ctx.SaveChanges();
    }

    private void UbahJatuhTempo(Guid tugas, DateTime dueAt)
    {
        using var ctx = _db.CreateContext();
        ctx.TrxWorkflowApproverAssignments.Single(x => x.Id == tugas).DueAt = dueAt;
        ctx.SaveChanges();
    }

    // AC1 — SPPD dan cuti engine tampil di antrean atasan dengan ruas baru; engine lama, penugasan orang lain, dan
    // penugasan yang sudah selesai tidak tampil; ringkasan sama dengan isi antrean.
    [Fact]
    public async Task Antrean_atasan_memuat_SPPD_dan_cuti_engine_tanpa_engine_lama_dan_ringkasan_sama()
    {
        var (_, nomor, wf, tugas) = await AjukanSppdAsync();
        var (wfCuti, tugasCuti) = Sisip(JenisCuti, "Wakil", "Bima");
        Sisip(JenisCuti, "Sari", "Bima", engineLama: true);
        Sisip(JenisCuti, "Sari", "Lain");
        Sisip(JenisCuti, "Sari", "Bima", statusTugas: WorkflowValueConstants.AssignmentStatus.Approved);

        var antrean = await AntreanAsync("Bima");

        Assert.Equal(2, antrean.TotalData);
        var sppd = Assert.Single(antrean.Items!, x => x.WorkflowInstanceId == wf);
        Assert.Equal(tugas, sppd.AssignmentId);
        Assert.Equal(nomor, sppd.ReferenceNumber);
        Assert.Equal(BusinessTravelWorkflowCodes.ReferenceType, sppd.RequestType);
        Assert.Equal("Sari", sppd.ApplicantName);
        Assert.StartsWith(nomor + " — ", sppd.SummaryText);
        Assert.False(string.IsNullOrWhiteSpace(sppd.WorkflowNumber));
        Assert.False(sppd.IsDelegated);
        Assert.NotNull(sppd.SlaDeadline);
        Assert.False(sppd.CanBulkApprove);

        var cuti = Assert.Single(antrean.Items!, x => x.WorkflowInstanceId == wfCuti);
        Assert.Equal(tugasCuti, cuti.AssignmentId);
        Assert.Equal(JenisCuti, cuti.RequestType);

        var ringkasan = await Antrean().GetInboxSummaryAsync(_p["Bima"].User);
        Assert.Equal(antrean.TotalData, ringkasan.TotalPendingCount);
        Assert.Equal(1, ringkasan.ByCategoryCounts[BusinessTravelWorkflowCodes.ReferenceType]);
        Assert.Equal(1, ringkasan.ByCategoryCounts[JenisCuti]);
        Assert.Equal(0, ringkasan.DelegatedCount);

        Assert.Equal(1, (await AntreanAsync("Lain")).TotalData);
        Assert.Equal(0, (await AntreanAsync("Wakil")).TotalData);
    }

    [Fact]
    public async Task Antrean_dibaca_lewat_controller_dengan_akun_login()
    {
        await AjukanSppdAsync();

        var hasil = Assert.IsType<OkObjectResult>(await Kontroler("Bima").GetInbox(null, null, null, null));
        Assert.Equal(1, Assert.IsType<ApiResponse<PagedResult<ApprovalInboxItemV2ResponseDto>>>(hasil.Value).Data!.TotalData);

        var ringkasan = Assert.IsType<OkObjectResult>(await Kontroler("Bima").GetInboxSummary());
        Assert.Equal(1, Assert.IsType<ApiResponse<ApprovalInboxSummaryV2ResponseDto>>(ringkasan.Value).Data!.TotalPendingCount);
    }

    [Fact]
    public async Task Filter_jenis_cocok_persis_tanpa_membedakan_huruf_besar()
    {
        await AjukanSppdAsync();
        Sisip(JenisCuti, "Wakil", "Bima");

        Assert.Equal(1, (await AntreanAsync("Bima", "hrd_trv_request")).TotalData);
        Assert.Equal(0, (await AntreanAsync("Bima", "TRV")).TotalData);
        Assert.Equal(1, (await AntreanAsync("Bima", JenisCuti)).TotalData);
    }

    [Fact]
    public async Task Mendesak_dan_lewat_SLA_dihitung_dari_jatuh_tempo_penugasan()
    {
        var (_, _, _, tugas) = await AjukanSppdAsync();
        Sisip(JenisCuti, "Wakil", "Bima", jatuhTempo: DateTime.UtcNow.AddHours(-1));
        UbahJatuhTempo(tugas, DateTime.UtcNow.AddHours(2));

        var antrean = await AntreanAsync("Bima");
        Assert.All(antrean.Items!, x => Assert.True(x.IsUrgent));
        Assert.Single(antrean.Items!, x => x.IsOverdue && x.RiskIndicator == "Red");
        Assert.Equal(0, (await AntreanAsync("Bima", mendesak: false)).TotalData);

        var ringkasan = await Antrean().GetInboxSummaryAsync(_p["Bima"].User);
        Assert.Equal(2, ringkasan.UrgentCount);
        Assert.Equal(1, ringkasan.OverdueCount);
    }

    // canBulkApprove (VR-WF-32): definisi bertanda, bukan permohonan sendiri, tidak mendesak/lewat SLA.
    [Fact]
    public async Task Kelayakan_massal_mengikuti_penanda_definisi_dan_SLA()
    {
        var (_, _, _, tugas) = await AjukanSppdAsync();
        Assert.False((await AntreanAsync("Bima")).Items!.Single().CanBulkApprove);

        TandaiMassal(true);
        Assert.True((await AntreanAsync("Bima")).Items!.Single().CanBulkApprove);

        UbahJatuhTempo(tugas, DateTime.UtcNow.AddHours(3));
        Assert.False((await AntreanAsync("Bima")).Items!.Single().CanBulkApprove);
    }

    // AC2 — Plh aktif melihat tugas pejabat (isDelegated); delegasi tidak aktif atau berbeda cakupan tidak.
    [Fact]
    public async Task Plh_aktif_melihat_tugas_pejabat_dengan_tanda_delegasi()
    {
        var (_, _, wf, tugas) = await AjukanSppdAsync();
        Assert.Equal(0, (await AntreanAsync("Wakil")).TotalData);

        Delegasi();

        var antrean = await AntreanAsync("Wakil");
        var butir = Assert.Single(antrean.Items!);
        Assert.Equal(wf, butir.WorkflowInstanceId);
        Assert.Equal(tugas, butir.AssignmentId);
        Assert.True(butir.IsDelegated);
        Assert.Equal("Bima", butir.DelegatedFromEmployeeName);
        Assert.Equal(1, (await AntreanAsync("Wakil", plh: true)).TotalData);
        Assert.Equal(0, (await AntreanAsync("Wakil", plh: false)).TotalData);
        Assert.Equal(1, (await Antrean().GetInboxSummaryAsync(_p["Wakil"].User)).DelegatedCount);

        // Pejabat definitif tetap melihat tugasnya tanpa tanda delegasi.
        Assert.False(Assert.Single((await AntreanAsync("Bima")).Items!).IsDelegated);
    }

    [Theory]
    [InlineData("berakhir")]
    [InlineData("belum-mulai")]
    [InlineData("dicabut")]
    [InlineData("definisi-lain")]
    public async Task Delegasi_tidak_aktif_atau_berbeda_cakupan_tidak_menampilkan_tugas(string kasus)
    {
        await AjukanSppdAsync();
        switch (kasus)
        {
            case "berakhir": Delegasi(mulaiHari: -5, akhirHari: -1); break;
            case "belum-mulai": Delegasi(mulaiHari: 1, akhirHari: 5); break;
            case "dicabut": Delegasi(status: WorkflowValueConstants.DelegationStatus.Revoked); break;
            case "definisi-lain": Delegasi(semua: false, definisi: Guid.NewGuid()); break;
        }

        Assert.Equal(0, (await AntreanAsync("Wakil")).TotalData);
    }

    // VR-WF-02: Plh tidak melihat permohonannya sendiri sebagai tugas Plh.
    [Fact]
    public async Task Plh_tidak_melihat_permohonannya_sendiri()
    {
        await AjukanSppdAsync("Wakil");
        Delegasi();

        Assert.Equal(0, (await AntreanAsync("Wakil")).TotalData);
        Assert.Null(await AksiSayaAsync("Wakil", (await AntreanAsync("Bima")).Items!.Single().WorkflowInstanceId));
    }

    // AC3 — myAction terisi untuk pemegang penugasan dan Plh, null untuk orang lain; alasan = kotak masuk lama.
    [Fact]
    public async Task MyAction_terisi_untuk_pemegang_dan_Plh_serta_null_untuk_lainnya()
    {
        var (_, _, wf, tugas) = await AjukanSppdAsync();

        var milik = await AksiSayaAsync("Bima", wf);
        Assert.NotNull(milik);
        Assert.Equal(tugas, milik!.AssignmentId);
        Assert.False(milik.IsDelegated);
        Assert.Null(milik.OnBehalfOfName);
        Assert.Equal(new[] { WorkflowValueConstants.ActionType.Approve, WorkflowValueConstants.ActionType.Reject, WorkflowValueConstants.ActionType.RequestRevision }, milik.AvailableActions);
        Assert.Equal(_alasanTolak, Assert.Single(milik.RejectionReasons).Id);

        Assert.Null(await AksiSayaAsync("Wakil", wf));
        Assert.Null(await AksiSayaAsync("Lain", wf));
        Assert.Null(await AksiSayaAsync("Sari", wf));

        Delegasi();
        var plh = await AksiSayaAsync("Wakil", wf);
        Assert.NotNull(plh);
        Assert.Equal(tugas, plh!.AssignmentId);
        Assert.True(plh.IsDelegated);
        Assert.Equal("Bima", plh.OnBehalfOfName);
    }

    [Fact]
    public async Task Alasan_penolakan_myAction_sama_dengan_kotak_masuk_lama()
    {
        var (_, _, wf, tugas) = await AjukanSppdAsync();
        var milik = await AksiSayaAsync("Bima", wf);

        await using var ctx = Konteks();
        var http = Pelaku.Sebagai(_p["Bima"].User);
        var lama = await new ApprovalInboxService(ctx, new HumanResourceContextService(ctx, http, Pengguna(ctx)), Workflow(ctx, _p["Bima"].User))
            .GetByIdAsync(tugas);

        Assert.True(lama.Success, lama.Message);
        Assert.Equal(
            lama.Data!.RejectionReasons.Select(x => (x.Id, x.ReasonCode, x.IsCommentRequired)),
            milik!.RejectionReasons.Select(x => (x.Id, x.ReasonCode, x.IsCommentRequired)));
    }

    [Fact]
    public async Task Sesudah_diputuskan_tugas_hilang_dari_antrean_dan_myAction_null()
    {
        var (_, _, wf, tugas) = await AjukanSppdAsync();

        await using (var ctx = Konteks())
        {
            var r = await Workflow(ctx, _p["Bima"].User).ApproveAsync(wf, tugas, new WorkflowApproveRequest { Comment = "Disetujui." });
            Assert.True(r.Success, r.Message);
        }

        Assert.Equal(0, (await AntreanAsync("Bima")).TotalData);
        Assert.Equal(0, (await Antrean().GetInboxSummaryAsync(_p["Bima"].User)).TotalPendingCount);
        Assert.Null(await AksiSayaAsync("Bima", wf));
    }

    [Fact]
    public async Task Tanpa_akun_antrean_kosong()
    {
        await AjukanSppdAsync();
        // Penugasan rusak tanpa akun penyetuju tidak boleh tampil bagi identitas kosong.
        var (wfRusak, _) = Sisip(JenisCuti, "Sari", "Bima", akunPenyetuju: Guid.Empty);
        Assert.Null(await Antrean().GetMyActionAsync(wfRusak, Guid.Empty));

        Assert.Equal(0, (await Antrean().GetInboxAsync(Guid.Empty, null, null, null, null, 1, 25)).TotalData);
        Assert.Null(await Antrean().GetMyActionAsync(Guid.NewGuid(), Guid.Empty));
    }
}
