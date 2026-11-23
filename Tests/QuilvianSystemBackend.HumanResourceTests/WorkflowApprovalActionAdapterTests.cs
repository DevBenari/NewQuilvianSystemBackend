using System.Data.Common;
using System.Text.Json;
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
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
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

// BE-HRD-444 (kontrak v29 §35.2, §35.7; HRD-DEC-229, HRD-DEC-230; VR-WF-26 s.d. VR-WF-28, VR-WF-30) — aksi Setujui/Tolak/
// Kembalikan Kotak Masuk Terpadu diteruskan ke engine WorkflowService NYATA lewat controller: SPPD diajukan lewat
// BusinessTravelRequestService sehingga penangan modul (status SPPD, presensi Dinas Luar) ikut berjalan. Tolak memakai
// alasan berkode, Kembalikan wajib catatan, bukan penyetuju 403 tanpa perubahan, PIN tidak diminta dan tidak dihitung.
// AT-HRD-WF-24 s.d. AT-HRD-WF-28.
public sealed class WorkflowApprovalActionAdapterTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _unit = Guid.NewGuid();
    private readonly Guid _alasanWajibCatatan = Guid.NewGuid();
    private readonly Guid _alasanTanpaCatatan = Guid.NewGuid();
    private readonly Guid _alasanCuti = Guid.NewGuid();
    private readonly Guid _alasanNonaktif = Guid.NewGuid();
    private readonly Dictionary<string, (Guid Wf, Guid User)> _p = new();
    private readonly NotifikasiTercatat _notifikasi = new();

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

    private sealed class NotifikasiTercatat : IUnifiedNotificationGatewayService
    {
        public List<(Guid User, string Judul, string Pesan)> Terkirim { get; } = new();

        public Task SendInstantNotificationAsync(Guid recipientEmployeeId, string title, string message, string channel = "WebPush") => Task.CompletedTask;

        public Task SendInstantNotificationToUserAsync(Guid recipientUserId, string title, string message, string? deepLinkUrl = null, Guid? workflowInstanceId = null, string priority = HrdNotificationPriorities.Normal, string? dedupKey = null)
        {
            Terkirim.Add((recipientUserId, title, message));
            return Task.CompletedTask;
        }

        public Task<int> SendApprovalDigestAsync() => Task.FromResult(0);
    }

    public WorkflowApprovalActionAdapterTests()
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
                Id = _alasanWajibCatatan, RequestType = BusinessTravelWorkflowCodes.RequestType, ReasonCode = "TRV-TOLAK-01", ReasonName = "Kegiatan tidak relevan dengan tugas",
                RejectAction = WorkflowValueConstants.RejectAction.CloseRequest, IsCommentRequired = true, AllowResubmit = false, IsActive = true, SortOrder = 1
            },
            new MstRejectionReason
            {
                Id = _alasanTanpaCatatan, RequestType = BusinessTravelWorkflowCodes.RequestType, ReasonCode = "TRV-TOLAK-02", ReasonName = "Anggaran unit habis",
                RejectAction = WorkflowValueConstants.RejectAction.CloseRequest, IsCommentRequired = false, AllowResubmit = false, IsActive = true, SortOrder = 2
            },
            new MstRejectionReason
            {
                Id = _alasanCuti, RequestType = "LEAVE_REQUEST", ReasonCode = "CUTI-TOLAK-01", ReasonName = "Kuota cuti habis",
                RejectAction = WorkflowValueConstants.RejectAction.CloseRequest, IsActive = true, SortOrder = 1
            },
            new MstRejectionReason
            {
                Id = _alasanNonaktif, RequestType = BusinessTravelWorkflowCodes.RequestType, ReasonCode = "TRV-TOLAK-99", ReasonName = "Alasan lama",
                RejectAction = WorkflowValueConstants.RejectAction.CloseRequest, IsActive = false, SortOrder = 9
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

    // Controller kotak terpadu dengan adapter, engine, dan konteks permintaan atas nama satu akun.
    private (WorkflowApprovalActionsController Kontroler, ApplicationDbContext Ctx) Kontroler(string nama)
    {
        var ctx = Konteks();
        var adapter = new WorkflowApprovalActionService(
            ctx, Workflow(ctx, _p[nama].User), new ApprovalInboxQueryService(ctx), _notifikasi, NullLogger<WorkflowApprovalActionService>.Instance);
        var kontroler = new WorkflowApprovalActionsController(adapter, null!, null!)
        {
            ControllerContext = new ControllerContext { HttpContext = Pelaku.Sebagai(_p[nama].User).HttpContext! }
        };
        return (kontroler, ctx);
    }

    private static (int Kode, ApiResponse<WorkflowActionV2ResponseDto> Isi) Hasil(IActionResult r)
    {
        var obj = Assert.IsAssignableFrom<ObjectResult>(r);
        return (obj.StatusCode ?? 200, Assert.IsType<ApiResponse<WorkflowActionV2ResponseDto>>(obj.Value));
    }

    private async Task<(int Kode, ApiResponse<WorkflowActionV2ResponseDto> Isi)> SetujuiAsync(string nama, Guid step, ApproveActionRequestDto? body = null)
    {
        var (k, ctx) = Kontroler(nama);
        await using (ctx) return Hasil(await k.Approve(step, body ?? new ApproveActionRequestDto { Note = "Disetujui." }));
    }

    private async Task<(int Kode, ApiResponse<WorkflowActionV2ResponseDto> Isi)> TolakAsync(string nama, Guid step, Guid? alasan, string? catatan)
    {
        var (k, ctx) = Kontroler(nama);
        await using (ctx) return Hasil(await k.Reject(step, new RejectActionRequestDto { RejectionReasonId = alasan, Comment = catatan }));
    }

    private async Task<(int Kode, ApiResponse<WorkflowActionV2ResponseDto> Isi)> KembalikanAsync(string nama, Guid step, string? catatan)
    {
        var (k, ctx) = Kontroler(nama);
        await using (ctx) return Hasil(await k.ReturnForRevision(step, new ReturnForRevisionRequestDto { RevisionNotes = catatan }));
    }

    // Ajukan SPPD lewat layanan modul: engine membuat penugasan untuk Bima. stepInstanceId diambil dari antrean terpadu.
    private async Task<(Guid Sppd, Guid Workflow, Guid Step, Guid Tugas)> AjukanSppdAsync()
    {
        var id = Guid.NewGuid();
        await using (var tulis = _db.CreateContext())
        {
            tulis.TrxBusinessTravelRequests.Add(new TrxBusinessTravelRequest
            {
                Id = id, TravelRequestNumber = "SPPD/UJI/" + id.ToString("N")[..6], WorkforceProfileId = _p["Sari"].Wf, TravelTypeId = Guid.NewGuid(),
                TravelTitle = "Seminar ICU", TravelPurpose = "Seminar nasional perawatan intensif", Origin = "Jakarta", Destination = "Surabaya",
                StartDate = new DateOnly(2026, 12, 1), EndDate = new DateOnly(2026, 12, 3), TravelDayCount = 3,
                EstimatedTotalAmount = 5_000_000m, EstimatedTransportationAmount = 5_000_000m, ApprovedBudgetAmount = 0m,
                TravelStatus = BusinessTravelRequestStatuses.Draft, IsActive = true, CreateBy = _p["Sari"].User, CreateDateTime = DateTime.UtcNow
            });
            await tulis.SaveChangesAsync();
        }

        await using var ctx = Konteks();
        var hasil = await new BusinessTravelRequestService(ctx, new TanpaKonflik(), new TarifNol(), Workflow(ctx, _p["Sari"].User), new TravelAttendanceAndRosterLockService(ctx))
            .SubmitRequestAsync(id, _p["Sari"].User, _p["Sari"].Wf);
        var wf = hasil.WorkflowInstanceId!.Value;

        var butir = Assert.Single((await new ApprovalInboxQueryService(_db.CreateContext()).GetInboxAsync(_p["Bima"].User, null, null, null, null, 1, 25)).Items!);
        Assert.Equal(wf, butir.WorkflowInstanceId);
        return (id, wf, butir.StepInstanceId, butir.AssignmentId);
    }

    private void Delegasi(int mulaiHari = -1, int akhirHari = 3)
    {
        using var ctx = _db.CreateContext();
        ctx.Set<TrxApprovalDelegation>().Add(new TrxApprovalDelegation
        {
            DelegatorUserId = _p["Bima"].User, DelegateUserId = _p["Wakil"].User, DelegatorWorkforceProfileId = _p["Bima"].Wf, DelegateWorkforceProfileId = _p["Wakil"].Wf,
            DelegationNumber = "DLG-" + Guid.NewGuid().ToString("N")[..6], DelegationStatus = WorkflowValueConstants.DelegationStatus.Active,
            EffectiveStartAt = DateTime.UtcNow.AddDays(mulaiHari), EffectiveEndAt = DateTime.UtcNow.AddDays(akhirHari),
            AppliesToAllWorkflows = true, IsActive = true, CreateDateTime = DateTime.UtcNow
        });
        ctx.SaveChanges();
    }

    private TrxBusinessTravelRequest Sppd(Guid id)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxBusinessTravelRequests.AsNoTracking().Single(x => x.Id == id);
    }

    private (string Workflow, string Tugas, int JumlahAksi) Keadaan(Guid wf, Guid tugas)
    {
        using var ctx = _db.CreateContext();
        return (
            ctx.TrxWorkflowInstances.AsNoTracking().Single(x => x.Id == wf).WorkflowStatus,
            ctx.TrxWorkflowApproverAssignments.AsNoTracking().Single(x => x.Id == tugas).AssignmentStatus,
            ctx.TrxApprovalActions.AsNoTracking().Count(x => x.WorkflowInstanceId == wf));
    }

    private List<TrxApprovalAction> Aksi(Guid wf)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxApprovalActions.AsNoTracking().Where(x => x.WorkflowInstanceId == wf).ToList();
    }

    // AC1 (AT-HRD-WF-24) — Setujui dari kotak terpadu: engine menyelesaikan workflow, penangan SPPD menjadikan Approved
    // dengan presensi Dinas Luar dan pelaku = penyetuju; pemohon diberi tahu.
    [Fact]
    public async Task Setujui_SPPD_dari_kotak_terpadu_lewat_engine_menyelesaikan_SPPD_dan_presensi()
    {
        var (id, wf, step, tugas) = await AjukanSppdAsync();

        var (kode, isi) = await SetujuiAsync("Bima", step);

        Assert.Equal(200, kode);
        Assert.True(isi.Data!.Success);
        Assert.Equal(WorkflowValueConstants.ActionType.Approve, isi.Data.ActionType);
        Assert.Equal(wf, isi.Data.WorkflowInstanceId);
        Assert.Equal(WorkflowValueConstants.WorkflowStatus.Completed, isi.Data.ResultingWorkflowStatus);
        Assert.Equal(WorkflowValueConstants.StepStatus.Approved, isi.Data.ResultingStepStatus);

        var (statusWf, statusTugas, _) = Keadaan(wf, tugas);
        Assert.Equal(WorkflowValueConstants.WorkflowStatus.Completed, statusWf);
        Assert.Equal(WorkflowValueConstants.AssignmentStatus.Approved, statusTugas);

        var sppd = Sppd(id);
        Assert.Equal("Approved", sppd.TravelStatus);
        Assert.Equal(_p["Bima"].User, sppd.ApprovedByUserId);
        await using var ctx = _db.CreateContext();
        var presensi = ctx.TrxTravelAttendanceLinks.AsNoTracking().Where(x => x.BusinessTravelRequestId == id).OrderBy(x => x.AttendanceDate).ToList();
        Assert.Equal(new[] { new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 2), new DateOnly(2026, 12, 3) }, presensi.Select(x => x.AttendanceDate).ToArray());

        var setuju = Assert.Single(Aksi(wf), x => x.ActionType == WorkflowValueConstants.ActionType.Approve);
        Assert.Equal(tugas, setuju.WorkflowApproverAssignmentId);
        Assert.Equal(_p["Bima"].User, setuju.ActualActionByUserId);
        Assert.Equal("Disetujui.", setuju.Comment);

        Assert.Contains(_notifikasi.Terkirim, x => x.User == _p["Sari"].User && x.Judul == "Permohonan disetujui");
    }

    // AC2 (AT-HRD-WF-25) — Tolak: tanpa alasan, alasan jenis lain, alasan nonaktif, alasan wajib catatan tanpa catatan → 400
    // tanpa perubahan; lengkap → 200, SPPD ditolak lewat penangan modul.
    [Fact]
    public async Task Tolak_wajib_alasan_berkode_dan_catatan_bila_dituntut()
    {
        var (id, wf, step, tugas) = await AjukanSppdAsync();
        var awal = Keadaan(wf, tugas);

        foreach (var alasan in new Guid?[] { null, Guid.Empty, _alasanCuti, _alasanNonaktif, Guid.NewGuid() })
        {
            var (k, isi) = await TolakAsync("Bima", step, alasan, "Catatan apa saja.");
            Assert.Equal(400, k);
            Assert.Equal(WorkflowApprovalActionService.InvalidReasonMessage, isi.Message);
        }

        var (kodeCatatan, isiCatatan) = await TolakAsync("Bima", step, _alasanWajibCatatan, "   ");
        Assert.Equal(400, kodeCatatan);
        Assert.Equal(WorkflowApprovalActionService.CommentRequiredMessage, isiCatatan.Message);
        Assert.Equal(awal, Keadaan(wf, tugas));
        Assert.Equal(BusinessTravelRequestStatuses.Submitted, Sppd(id).TravelStatus);

        var (kode, hasil) = await TolakAsync("Bima", step, _alasanWajibCatatan, "Seminar tidak relevan dengan tugas di ICU.");
        Assert.Equal(200, kode);
        Assert.Equal(WorkflowValueConstants.ActionType.Reject, hasil.Data!.ActionType);
        Assert.Equal(WorkflowValueConstants.WorkflowStatus.Rejected, Keadaan(wf, tugas).Workflow);
        Assert.Equal("Rejected", Sppd(id).TravelStatus);

        var tolak = Assert.Single(Aksi(wf), x => x.ActionType == WorkflowValueConstants.ActionType.Reject);
        Assert.Equal(_alasanWajibCatatan, tolak.ActionReasonId);
        Assert.Equal("Seminar tidak relevan dengan tugas di ICU.", tolak.Comment);
        Assert.Contains(_notifikasi.Terkirim, x => x.User == _p["Sari"].User && x.Pesan.Contains("Kegiatan tidak relevan dengan tugas", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Tolak_dengan_alasan_tanpa_tuntutan_catatan_boleh_tanpa_catatan()
    {
        var (id, wf, step, _) = await AjukanSppdAsync();

        var (kode, _) = await TolakAsync("Bima", step, _alasanTanpaCatatan, null);

        Assert.Equal(200, kode);
        Assert.Equal("Rejected", Sppd(id).TravelStatus);
        Assert.Equal(_alasanTanpaCatatan, Assert.Single(Aksi(wf), x => x.ActionType == WorkflowValueConstants.ActionType.Reject).ActionReasonId);
    }

    // AC2 (AT-HRD-WF-26) — Kembalikan tanpa catatan → 400 tanpa perubahan; dengan catatan → 200 dan SPPD NeedRevision.
    [Fact]
    public async Task Kembalikan_wajib_catatan_dan_SPPD_menjadi_NeedRevision()
    {
        var (id, wf, step, tugas) = await AjukanSppdAsync();
        var awal = Keadaan(wf, tugas);

        foreach (var catatan in new[] { null, "", "   " })
        {
            var (k, isi) = await KembalikanAsync("Bima", step, catatan);
            Assert.Equal(400, k);
            Assert.Equal(WorkflowApprovalActionService.RevisionNotesRequiredMessage, isi.Message);
        }

        Assert.Equal(awal, Keadaan(wf, tugas));

        var (kode, hasil) = await KembalikanAsync("Bima", step, "Hotel di atas plafon, mohon diganti.");
        Assert.Equal(200, kode);
        Assert.Equal(WorkflowValueConstants.ActionType.RequestRevision, hasil.Data!.ActionType);
        Assert.Equal("NeedRevision", Sppd(id).TravelStatus);
        Assert.Equal("Hotel di atas plafon, mohon diganti.", Assert.Single(Aksi(wf), x => x.ActionType == WorkflowValueConstants.ActionType.RequestRevision).Comment);
    }

    // AC3 (AT-HRD-WF-27) — bukan penyetuju dan pemohon → 403 tanpa perubahan, untuk ketiga aksi.
    [Theory]
    [InlineData("Lain", WorkflowApprovalActionService.NotApproverMessage)]
    [InlineData("Wakil", WorkflowApprovalActionService.NotApproverMessage)]
    [InlineData("Sari", WorkflowApprovalActionService.OwnRequestMessage)]
    public async Task Bukan_penyetuju_atau_pemohon_ditolak_403_tanpa_perubahan(string nama, string pesan)
    {
        var (id, wf, step, tugas) = await AjukanSppdAsync();
        var awal = Keadaan(wf, tugas);

        var hasil = new[]
        {
            await SetujuiAsync(nama, step),
            await TolakAsync(nama, step, _alasanTanpaCatatan, null),
            await KembalikanAsync(nama, step, "Mohon diperbaiki.")
        };

        Assert.All(hasil, h => { Assert.Equal(403, h.Kode); Assert.Equal(pesan, h.Isi.Message); });
        Assert.Equal(awal, Keadaan(wf, tugas));
        Assert.Equal(BusinessTravelRequestStatuses.Submitted, Sppd(id).TravelStatus);
        Assert.Empty(_notifikasi.Terkirim);
    }

    // VR-WF-30 / AT-HRD-WF-33 (aksi) — Plh aktif menyetujui atas nama pejabat lewat kotak terpadu; jejak mencatat Plh.
    [Fact]
    public async Task Plh_aktif_menyetujui_atas_nama_pejabat_lewat_kotak_terpadu()
    {
        var (id, wf, step, tugas) = await AjukanSppdAsync();
        Delegasi();

        var (kode, _) = await SetujuiAsync("Wakil", step);

        Assert.Equal(200, kode);
        Assert.Equal("Approved", Sppd(id).TravelStatus);
        var setuju = Assert.Single(Aksi(wf), x => x.ActionType == WorkflowValueConstants.ActionType.Approve);
        Assert.Equal(tugas, setuju.WorkflowApproverAssignmentId);
        Assert.Equal(_p["Wakil"].User, setuju.ActualActionByUserId);
        Assert.True(setuju.IsDelegated);
        Assert.Equal(_p["Bima"].User, setuju.DelegatedFromUserId);
    }

    [Fact]
    public async Task Plh_dengan_delegasi_berakhir_ditolak_403()
    {
        var (_, wf, step, tugas) = await AjukanSppdAsync();
        Delegasi(mulaiHari: -5, akhirHari: -1);
        var awal = Keadaan(wf, tugas);

        var (kode, isi) = await SetujuiAsync("Wakil", step);

        Assert.Equal(403, kode);
        Assert.Equal(WorkflowApprovalActionService.NotApproverMessage, isi.Message);
        Assert.Equal(awal, Keadaan(wf, tugas));
    }

    // Kontrak §35.7 — tahap tidak dikenal 404; penugasan sudah diputuskan 409 (tidak ada keputusan ganda).
    [Fact]
    public async Task Tahap_tidak_ada_404_dan_keputusan_ulang_409()
    {
        var (_, wf, step, _) = await AjukanSppdAsync();

        Assert.Equal(404, (await SetujuiAsync("Bima", Guid.NewGuid())).Kode);

        Assert.Equal(200, (await SetujuiAsync("Bima", step)).Kode);
        var jumlahAksi = Aksi(wf).Count;

        var ulang = await SetujuiAsync("Bima", step);
        Assert.Equal(409, ulang.Kode);
        Assert.Equal(409, (await TolakAsync("Bima", step, _alasanTanpaCatatan, null)).Kode);
        Assert.Equal(jumlahAksi, Aksi(wf).Count);
    }

    // AC4 (AT-HRD-WF-28) — PIN tidak diminta: tanpa PIN dan dengan PIN acak (klien lama) → 200; DigitalSignatureHash kosong.
    [Fact]
    public async Task Tanpa_PIN_dan_dengan_PIN_acak_disetujui_tanpa_tanda_tangan()
    {
        Assert.DoesNotContain(typeof(ApproveActionRequestDto).GetProperties(), x => x.Name.Contains("Pin", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(typeof(RejectActionRequestDto).GetProperties(), x => x.Name.Contains("Pin", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(typeof(ReturnForRevisionRequestDto).GetProperties(), x => x.Name.Contains("Pin", StringComparison.OrdinalIgnoreCase));

        var (_, wf1, step1, _) = await AjukanSppdAsync();
        Assert.Equal(200, (await SetujuiAsync("Bima", step1, new ApproveActionRequestDto())).Kode);

        // Badan permintaan klien lama yang masih membawa pinSignature dibaca seperti oleh ASP.NET Core (opsi web).
        var lama = JsonSerializer.Deserialize<ApproveActionRequestDto>(
            "{\"note\":\"Setuju\",\"pinSignature\":\"" + Guid.NewGuid().ToString("N")[..7] + "\"}",
            new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        var (_, wf2, step2, _) = await AjukanSppdAsync();
        Assert.Equal(200, (await SetujuiAsync("Bima", step2, lama)).Kode);

        Assert.All(Aksi(wf1).Concat(Aksi(wf2)), x => Assert.True(string.IsNullOrEmpty(x.DigitalSignatureHash)));
    }

    // Penugasan engine V2 lama (ApproverEmployeeId terisi) tidak dijalankan kotak terpadu (HRD-DEC-229).
    [Fact]
    public async Task Penugasan_engine_lama_tidak_dijalankan_kotak_terpadu()
    {
        Guid step;
        Guid tugas;
        await using (var ctx = _db.CreateContext())
        {
            var definisi = Guid.NewGuid();
            var wf = Guid.NewGuid();
            step = Guid.NewGuid();
            tugas = Guid.NewGuid();
            ctx.Set<MstWorkflowDefinition>().Add(new MstWorkflowDefinition
            {
                Id = definisi, WorkflowCode = "UJI_LAMA_" + definisi.ToString("N")[..6], WorkflowName = "Cuti", RequestType = "LEAVE_REQUEST",
                WorkflowCategory = "Leave", Version = 1, WorkflowStatus = "Active", IsActive = true, CreateDateTime = DateTime.UtcNow
            });
            ctx.TrxWorkflowInstances.Add(new TrxWorkflowInstance
            {
                Id = wf, WorkflowDefinitionId = definisi, ReferenceType = "LEAVE_REQUEST", ReferenceId = Guid.NewGuid(), RequestNumber = "WF-LAMA-1",
                RequestedByUserId = _p["Sari"].User, WorkflowStatus = WorkflowValueConstants.WorkflowStatus.InProgress,
                SourceChannel = WorkflowValueConstants.SourceChannel.Web, StartedAt = DateTime.UtcNow, IsActive = true, CreateDateTime = DateTime.UtcNow
            });
            ctx.TrxWorkflowStepInstances.Add(new TrxWorkflowStepInstance
            {
                Id = step, WorkflowInstanceId = wf, WorkflowStepId = Guid.NewGuid(), StepOrder = 1, StepCodeSnapshot = "S1", StepNameSnapshot = "Atasan", StepTypeSnapshot = "Approval",
                ApprovalModeSnapshot = "Any", ApproverSourceSnapshot = "RequesterManager", StepStatus = WorkflowValueConstants.StepStatus.InProgress, IsCurrentStep = true,
                IsActive = true, CreateDateTime = DateTime.UtcNow
            });
            ctx.TrxWorkflowApproverAssignments.Add(new TrxWorkflowApproverAssignment
            {
                Id = tugas, WorkflowInstanceId = wf, WorkflowStepInstanceId = step, AssignedApproverUserId = _p["Bima"].User, ApproverEmployeeId = Guid.NewGuid(),
                ApproverSourceSnapshot = "RequesterManager", AssignmentStatus = WorkflowValueConstants.AssignmentStatus.Pending, IsCurrentAssignment = true,
                IsActive = true, CreateDateTime = DateTime.UtcNow
            });
            await ctx.SaveChangesAsync();
        }

        var (kode, isi) = await SetujuiAsync("Bima", step);

        Assert.Equal(403, kode);
        Assert.Equal(WorkflowApprovalActionService.NotApproverMessage, isi.Message);
        await using var baca = _db.CreateContext();
        Assert.Equal(WorkflowValueConstants.AssignmentStatus.Pending, baca.TrxWorkflowApproverAssignments.AsNoTracking().Single(x => x.Id == tugas).AssignmentStatus);
    }
}
