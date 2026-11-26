using System.Data.Common;
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
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-376 (kontrak v28 INT-HR-109/110, State 32.2–32.3, VR-TRV-32; HRD-DEC-225, 226) — penangan Workflow V2 untuk SPPD:
// setuju → Disetujui (anggaran = estimasi, presensi Dinas Luar per hari, gembok roster, pelaku = penyetuju); kembalikan → NeedRevision;
// tolak (alasan CloseRequest) → Ditolak final; tarik → Dibatalkan; idempoten; instance lama diabaikan; status lanjutan tidak diturunkan;
// penyelarasan ulang menangkap SPPD yang tertinggal (keputusan pengguna 19 November 2026, pilihan (a)).
// AT-HRD-TRV-26, 27, 28, 32, 33. Workflow V2 NYATA di SQLite.
public sealed class TravelWorkflowLifecycleTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _unit = Guid.NewGuid();
    private readonly Guid _alasanTolak = Guid.NewGuid();
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

    public TravelWorkflowLifecycleTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _unit, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        var mulai = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        foreach (var nama in new[] { "Sari", "Bima" })
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

        ctx.WfpManagerAssignments.Add(new WfpManagerAssignment
        {
            WorkforceProfileId = _p["Sari"].Wf, ManagerWorkforceProfileId = _p["Bima"].Wf, ManagerType = "Direct", EffectiveStartDate = mulai,
            IsPrimaryManager = true, CanApproveRequests = true, IsActive = true
        });

        // Alasan penolakan final SPPD (RejectAction CloseRequest → workflow Rejected).
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

    private static BusinessTravelWorkflowLifecycleHandler Penangan(ApplicationDbContext ctx) =>
        new(ctx, new TravelAttendanceAndRosterLockService(ctx), NullLogger<BusinessTravelWorkflowLifecycleHandler>.Instance);

    private static WorkflowService Workflow(ApplicationDbContext ctx, Guid pelaku, bool denganPenangan = true)
    {
        var http = Pelaku.Sebagai(pelaku);
        var userManager = new UserManager<ApplicationUser>(
            new TanpaPeranStore(ctx), Options.Create(new IdentityOptions()), new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(), Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(), new IdentityErrorDescriber(), null!,
            NullLogger<UserManager<ApplicationUser>>.Instance);
        var handlers = denganPenangan ? new IWorkflowReferenceLifecycleHandler[] { Penangan(ctx) } : Array.Empty<IWorkflowReferenceLifecycleHandler>();
        var lifecycle = new WorkflowReferenceLifecycleService(
            ctx, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!,
            NullLogger<WorkflowReferenceLifecycleService>.Instance, handlers);
        return new WorkflowService(ctx, new HumanResourceContextService(ctx, http, userManager), userManager, http, lifecycle);
    }

    private Guid Sppd(string status = BusinessTravelRequestStatuses.Draft)
    {
        using var ctx = _db.CreateContext();
        var id = Guid.NewGuid();
        ctx.TrxBusinessTravelRequests.Add(new TrxBusinessTravelRequest
        {
            Id = id, TravelRequestNumber = "SPPD/UJI/" + id.ToString("N")[..6], WorkforceProfileId = _p["Sari"].Wf, TravelTypeId = Guid.NewGuid(),
            TravelTitle = "Seminar ICU", TravelPurpose = "Seminar nasional perawatan intensif", Origin = "Jakarta", Destination = "Surabaya",
            StartDate = new DateOnly(2026, 12, 1), EndDate = new DateOnly(2026, 12, 3), TravelDayCount = 3,
            EstimatedTotalAmount = 5_000_000m, EstimatedTransportationAmount = 5_000_000m, ApprovedBudgetAmount = 0m,
            TravelStatus = status, IsActive = true, CreateBy = _p["Sari"].User, CreateDateTime = DateTime.UtcNow
        });
        ctx.SaveChanges();
        return id;
    }

    private TrxBusinessTravelRequest Baca(Guid id)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxBusinessTravelRequests.AsNoTracking().Single(x => x.Id == id);
    }

    private List<TrxTravelAttendanceLink> Presensi(Guid id)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxTravelAttendanceLinks.AsNoTracking().Where(x => x.BusinessTravelRequestId == id).OrderBy(x => x.AttendanceDate).ToList();
    }

    private async Task<Guid> AjukanAsync(Guid id)
    {
        await using var ctx = Konteks();
        var hasil = await new BusinessTravelRequestService(ctx, new TanpaKonflik(), new TarifNol(), Workflow(ctx, _p["Sari"].User), new TravelAttendanceAndRosterLockService(ctx))
            .SubmitRequestAsync(id, _p["Sari"].User, _p["Sari"].Wf);
        return hasil.WorkflowInstanceId!.Value;
    }

    private Guid TugasBima(Guid workflowId)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxWorkflowApproverAssignments.AsNoTracking()
            .Where(x => x.WorkflowInstanceId == workflowId && x.AssignedApproverUserId == _p["Bima"].User)
            .OrderByDescending(x => x.CreateDateTime).First().Id;
    }

    private async Task SetujuiAsync(Guid workflowId, bool denganPenangan = true)
    {
        await using var ctx = Konteks();
        var r = await Workflow(ctx, _p["Bima"].User, denganPenangan).ApproveAsync(workflowId, TugasBima(workflowId), new WorkflowApproveRequest { Comment = "Disetujui, seminar relevan." });
        Assert.True(r.Success, r.Message);
    }

    // AT-HRD-TRV-26 / INT-HR-110 — persetujuan final mengisi status, anggaran, presensi per hari dinas, dan gembok roster.
    [Fact]
    public async Task Disetujui_mengisi_anggaran_presensi_dan_gembok_dengan_pelaku_penyetuju()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);
        Assert.Empty(Presensi(id));

        await SetujuiAsync(wf);

        var s = Baca(id);
        Assert.Equal("Approved", s.TravelStatus);
        Assert.NotNull(s.ApprovedAt);
        Assert.Equal(_p["Bima"].User, s.ApprovedByUserId);
        Assert.Equal(5_000_000m, s.ApprovedBudgetAmount);
        Assert.True(s.HasScheduleConflict);
        var presensi = Presensi(id);
        Assert.Equal(new[] { new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 2), new DateOnly(2026, 12, 3) }, presensi.Select(x => x.AttendanceDate).ToArray());
        Assert.All(presensi, x => Assert.Equal("Synced", x.AttendanceLinkStatus));
        Assert.All(presensi, x => Assert.Equal(_p["Bima"].User, x.CreateBy));

        await using var ctx = Konteks();
        var detail = await new BusinessTravelRequestService(ctx, new TanpaKonflik(), new TarifNol(), Workflow(ctx, _p["Sari"].User), new TravelAttendanceAndRosterLockService(ctx)).GetRequestByIdAsync(id);
        Assert.True(detail.IsApproved);
        Assert.False(detail.CanSubmit);
    }

    // AT-HRD-TRV-27 — dikembalikan → NeedRevision, dapat diubah, diajukan ulang memakai instance yang sama, lalu disetujui.
    [Fact]
    public async Task Dikembalikan_menjadi_NeedRevision_lalu_diajukan_ulang_dan_disetujui()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);
        await using (var ctx = Konteks())
        {
            var r = await Workflow(ctx, _p["Bima"].User).RequestRevisionAsync(wf, TugasBima(wf), new WorkflowRequestRevisionRequest { Comment = "Hotel di atas plafon, mohon diganti." });
            Assert.True(r.Success, r.Message);
        }

        Assert.Equal("NeedRevision", Baca(id).TravelStatus);
        Assert.Empty(Presensi(id));

        var wf2 = await AjukanAsync(id);
        Assert.Equal(wf, wf2);
        Assert.Equal("Submitted", Baca(id).TravelStatus);

        await SetujuiAsync(wf);
        Assert.Equal("Approved", Baca(id).TravelStatus);
        Assert.Equal(3, Presensi(id).Count);
    }

    // AT-HRD-TRV-28 / VR-TRV-28 — ditolak final: RejectedAt/RejectedByUserId, tanpa presensi, tidak dapat diajukan ulang.
    [Fact]
    public async Task Ditolak_final_tanpa_presensi_dan_tidak_dapat_diajukan_ulang()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);
        await using (var ctx = Konteks())
        {
            var r = await Workflow(ctx, _p["Bima"].User).RejectAsync(wf, TugasBima(wf), new WorkflowRejectRequest { RejectionReasonId = _alasanTolak, Comment = "Seminar tidak relevan dengan tugas di ICU." });
            Assert.True(r.Success, r.Message);
        }

        var s = Baca(id);
        Assert.Equal("Rejected", s.TravelStatus);
        Assert.NotNull(s.RejectedAt);
        Assert.Equal(_p["Bima"].User, s.RejectedByUserId);
        Assert.Null(s.ApprovedAt);
        Assert.Equal(0m, s.ApprovedBudgetAmount);
        Assert.False(s.HasScheduleConflict);
        Assert.Empty(Presensi(id));

        await using var c = Konteks();
        var galat = await Assert.ThrowsAsync<BusinessTravelRequestException>(() =>
            new BusinessTravelRequestService(c, new TanpaKonflik(), new TarifNol(), Workflow(c, _p["Sari"].User), new TravelAttendanceAndRosterLockService(c)).SubmitRequestAsync(id, _p["Sari"].User, _p["Sari"].Wf));
        Assert.Equal(409, galat.StatusCode);
    }

    // State 32.3 — workflow ditarik pemohon → SPPD Dibatalkan.
    [Fact]
    public async Task Ditarik_pemohon_menjadi_Cancelled()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);
        await using (var ctx = Konteks())
        {
            var r = await Workflow(ctx, _p["Sari"].User).WithdrawAsync(wf, new WorkflowWithdrawRequest { Reason = "Seminar dibatalkan panitia." });
            Assert.True(r.Success, r.Message);
        }

        var s = Baca(id);
        Assert.Equal("Cancelled", s.TravelStatus);
        Assert.NotNull(s.CancelledAt);
        Assert.Empty(Presensi(id));
    }

    // AT-HRD-TRV-33 — penyelarasan berulang tidak mengubah apa pun dan tidak menggandakan presensi.
    [Fact]
    public async Task Penyelarasan_berulang_idempoten()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);
        await SetujuiAsync(wf);
        var sebelum = Baca(id);
        var presensiSebelum = Presensi(id).Select(x => (x.Id, x.SyncedAt)).ToList();

        for (var i = 0; i < 2; i++)
        {
            await using var ctx = Konteks();
            var instance = ctx.TrxWorkflowInstances.AsNoTracking().Single(x => x.Id == wf);
            var hasil = await Penangan(ctx).SynchronizeAsync(instance, _p["Bima"].User, false, CancellationToken.None);
            Assert.True(hasil.IsHandled);
            Assert.False(hasil.StatusChanged);
        }

        var sesudah = Baca(id);
        Assert.Equal(sebelum.ApprovedAt, sesudah.ApprovedAt);
        Assert.Equal(sebelum.ApprovedByUserId, sesudah.ApprovedByUserId);
        Assert.Equal(presensiSebelum, Presensi(id).Select(x => (x.Id, x.SyncedAt)).ToList());
    }

    // Status lanjutan sesudah disetujui tidak pernah diturunkan; instance lama diabaikan; SPPD tidak ada → tidak ditangani.
    [Fact]
    public async Task Status_lanjutan_instance_lama_dan_referensi_hilang_tidak_mengubah_SPPD()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);
        await SetujuiAsync(wf);
        using (var ctx = _db.CreateContext())
        {
            var s = ctx.TrxBusinessTravelRequests.Single(x => x.Id == id);
            s.TravelStatus = "AdvancePaid";
            ctx.SaveChanges();
        }

        await using (var ctx = Konteks())
        {
            var instance = ctx.TrxWorkflowInstances.AsNoTracking().Single(x => x.Id == wf);
            var hasil = await Penangan(ctx).SynchronizeAsync(instance, _p["Bima"].User, false, CancellationToken.None);
            Assert.False(hasil.StatusChanged);

            var lama = new TrxWorkflowInstance { Id = Guid.NewGuid(), ReferenceType = BusinessTravelWorkflowCodes.ReferenceType, ReferenceId = id, WorkflowStatus = WorkflowValueConstants.WorkflowStatus.Cancelled };
            var hasilLama = await Penangan(ctx).SynchronizeAsync(lama, _p["Sari"].User, false, CancellationToken.None);
            Assert.True(hasilLama.IsHandled);
            Assert.False(hasilLama.StatusChanged);

            var hilang = new TrxWorkflowInstance { Id = Guid.NewGuid(), ReferenceType = BusinessTravelWorkflowCodes.ReferenceType, ReferenceId = Guid.NewGuid(), WorkflowStatus = WorkflowValueConstants.WorkflowStatus.Approved };
            var hasilHilang = await Penangan(ctx).SynchronizeAsync(hilang, _p["Sari"].User, false, CancellationToken.None);
            Assert.False(hasilHilang.IsHandled);
        }

        Assert.Equal("AdvancePaid", Baca(id).TravelStatus);
        Assert.Equal(3, Presensi(id).Count);
    }

    // Keputusan (a) — penangan gagal/tidak terpanggil: SPPD tertinggal "Submitted" walau workflow disetujui; penyelarasan ulang
    // memindahkannya ke Disetujui dengan presensi, dan putaran berikutnya tidak mengubah apa pun.
    [Fact]
    public async Task Penyelarasan_ulang_menangkap_SPPD_yang_tertinggal()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);
        await SetujuiAsync(wf, denganPenangan: false);

        Assert.Equal("Submitted", Baca(id).TravelStatus);
        Assert.Empty(Presensi(id));

        await using (var ctx = Konteks())
        {
            var jumlah = await new BusinessTravelWorkflowReconciliationService(ctx, Penangan(ctx), NullLogger<BusinessTravelWorkflowReconciliationService>.Instance)
                .ReconcileAsync(BusinessTravelNumberSeries.SystemFallbackUserId);
            Assert.Equal(1, jumlah);
        }

        var s = Baca(id);
        Assert.Equal("Approved", s.TravelStatus);
        Assert.Equal(_p["Bima"].User, s.ApprovedByUserId);
        Assert.Equal(5_000_000m, s.ApprovedBudgetAmount);
        Assert.Equal(3, Presensi(id).Count);

        await using (var ctx = Konteks())
        {
            var lagi = await new BusinessTravelWorkflowReconciliationService(ctx, Penangan(ctx), NullLogger<BusinessTravelWorkflowReconciliationService>.Instance)
                .ReconcileAsync(BusinessTravelNumberSeries.SystemFallbackUserId);
            Assert.Equal(0, lagi);
        }
    }

    // Instance lama (mis. ditarik sebelum SPPD memakai instance baru) tidak boleh membatalkan SPPD yang sedang menunggu.
    [Fact]
    public async Task Instance_lama_tidak_mengubah_SPPD_yang_sedang_menunggu()
    {
        var id = Sppd();
        await AjukanAsync(id);

        await using (var ctx = Konteks())
        {
            var lama = new TrxWorkflowInstance { Id = Guid.NewGuid(), ReferenceType = BusinessTravelWorkflowCodes.ReferenceType, ReferenceId = id, WorkflowStatus = WorkflowValueConstants.WorkflowStatus.Withdrawn };
            var hasil = await Penangan(ctx).SynchronizeAsync(lama, _p["Sari"].User, false, CancellationToken.None);
            Assert.True(hasil.IsHandled);
            Assert.False(hasil.StatusChanged);
        }

        var s = Baca(id);
        Assert.Equal("Submitted", s.TravelStatus);
        Assert.Null(s.CancelledAt);
    }

    // Idempoten selama menunggu: workflow InProgress dan SPPD sudah Submitted → tidak ada penulisan ulang.
    [Fact]
    public async Task Penyelarasan_saat_menunggu_tidak_menulis_ulang_SPPD()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);
        var sebelum = Baca(id);

        await using (var ctx = Konteks())
        {
            var instance = ctx.TrxWorkflowInstances.AsNoTracking().Single(x => x.Id == wf);
            var hasil = await Penangan(ctx).SynchronizeAsync(instance, BusinessTravelNumberSeries.SystemFallbackUserId, false, CancellationToken.None);
            Assert.False(hasil.StatusChanged);
        }

        var sesudah = Baca(id);
        Assert.Equal("Submitted", sesudah.TravelStatus);
        Assert.Equal(sebelum.UpdateDateTime, sesudah.UpdateDateTime);
        Assert.Equal(sebelum.UpdateBy, sesudah.UpdateBy);
    }

    // Proses terputus di antara pembuatan dan pengiriman workflow: instance masih Draft. Penyelarasan ulang tidak boleh
    // menjadikan SPPD "Menunggu" (belum ada tugas untuk atasan); Ajukan berikutnya memakai ulang instance Draft itu.
    [Fact]
    public async Task Instance_Draft_tidak_menjadikan_SPPD_menunggu_dan_dipakai_ulang_saat_Ajukan()
    {
        var id = Sppd();
        Guid draf;
        await using (var ctx = Konteks())
        {
            var buat = await Workflow(ctx, _p["Sari"].User).CreateAsync(new CreateWorkflowInstanceRequest
            {
                WorkflowDefinitionCode = BusinessTravelWorkflowCodes.DefinitionCode, ReferenceType = BusinessTravelWorkflowCodes.ReferenceType, ReferenceId = id
            });
            Assert.True(buat.Success, buat.Message);
            draf = buat.Data!.Id;
            var s = ctx.TrxBusinessTravelRequests.Single(x => x.Id == id);
            s.WorkflowInstanceId = draf;
            s.WorkflowDefinitionId = buat.Data.WorkflowDefinitionId;
            await ctx.SaveChangesAsync();
        }

        await using (var ctx = Konteks())
        {
            var jumlah = await new BusinessTravelWorkflowReconciliationService(ctx, Penangan(ctx), NullLogger<BusinessTravelWorkflowReconciliationService>.Instance)
                .ReconcileAsync(BusinessTravelNumberSeries.SystemFallbackUserId);
            Assert.Equal(0, jumlah);
        }

        Assert.Equal("Draft", Baca(id).TravelStatus);
        Assert.Null(Baca(id).SubmittedAt);

        var wf = await AjukanAsync(id);
        Assert.Equal(draf, wf);
        Assert.Equal("Submitted", Baca(id).TravelStatus);
    }

    // INT-HR-110 — pelepasan gembok mencatat pelaku (sebelumnya Guid.Empty).
    [Fact]
    public async Task Lepas_gembok_mencatat_pelaku()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);
        await SetujuiAsync(wf);

        await using (var ctx = Konteks())
        {
            await new TravelAttendanceAndRosterLockService(ctx).ReleaseRosterLockAsync(id, _p["Sari"].User);
        }

        var s = Baca(id);
        Assert.False(s.HasScheduleConflict);
        Assert.Equal(_p["Sari"].User, s.UpdateBy);
        Assert.All(Presensi(id), x => { Assert.Equal("Cancelled", x.AttendanceLinkStatus); Assert.Equal(_p["Sari"].User, x.UpdateBy); });
    }

    // ---- BE-HRD-377 (API 34.1 PUT /{id}/cancel, VR-TRV-33, State 32.2, Integration 25.2 pelepasan) ----

    private async Task<BusinessTravelResponseDto> BatalAsync(Guid id, string pelaku, string alasan = "Seminar dibatalkan oleh panitia penyelenggara.")
    {
        await using var ctx = Konteks();
        return await new BusinessTravelRequestService(ctx, new TanpaKonflik(), new TarifNol(), Workflow(ctx, _p[pelaku].User), new TravelAttendanceAndRosterLockService(ctx))
            .CancelRequestAsync(id, new CancelTravelRequestDto { CancellationReason = alasan }, _p[pelaku].User);
    }

    private string StatusWorkflow(Guid wf)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxWorkflowInstances.AsNoTracking().Single(x => x.Id == wf).WorkflowStatus;
    }

    // AT-HRD-TRV-29 — batal saat menunggu menarik workflow; tugas atasan tidak lagi terbuka.
    [Fact]
    public async Task Batal_saat_menunggu_menarik_workflow()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);

        var hasil = await BatalAsync(id, "Sari");

        Assert.Equal("Cancelled", hasil.TravelStatus);
        Assert.Equal(WorkflowValueConstants.WorkflowStatus.Withdrawn, StatusWorkflow(wf));
        var s = Baca(id);
        Assert.Equal("Cancelled", s.TravelStatus);
        Assert.Equal("Seminar dibatalkan oleh panitia penyelenggara.", s.CancellationReason);
        Assert.NotNull(s.CancelledAt);
        Assert.Equal(_p["Sari"].User, s.CancelledByUserId);
        using var ctx = _db.CreateContext();
        Assert.DoesNotContain(ctx.TrxWorkflowApproverAssignments.AsNoTracking().Where(x => x.WorkflowInstanceId == wf).Select(x => x.AssignmentStatus).ToList(),
            x => x == WorkflowValueConstants.AssignmentStatus.Available || x == WorkflowValueConstants.AssignmentStatus.Pending || x == WorkflowValueConstants.AssignmentStatus.InProgress);
    }

    // VR-TRV-33 — penarikan ditolak Workflow V2 (bukan pemohon): 409 dengan pesan engine, SPPD dan workflow tidak berubah.
    [Fact]
    public async Task Batal_ditolak_engine_409_dan_SPPD_tetap_menunggu()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);

        var galat = await Assert.ThrowsAsync<BusinessTravelRequestException>(() => BatalAsync(id, "Bima"));

        Assert.Equal(409, galat.StatusCode);
        Assert.Equal("Hanya pemohon yang dapat menarik workflow.", galat.Message);
        var s = Baca(id);
        Assert.Equal("Submitted", s.TravelStatus);
        Assert.Null(s.CancelledAt);
        Assert.Null(s.CancellationReason);
        Assert.Equal(WorkflowValueConstants.WorkflowStatus.InProgress, StatusWorkflow(wf));
    }

    // State 32.2 — batal SPPD yang dikembalikan menutup workflow-nya (dibatalkan, bukan ditarik).
    [Fact]
    public async Task Batal_saat_NeedRevision_membatalkan_workflow()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);
        await using (var ctx = Konteks())
        {
            var r = await Workflow(ctx, _p["Bima"].User).RequestRevisionAsync(wf, TugasBima(wf), new WorkflowRequestRevisionRequest { Comment = "Mohon lengkapi undangan seminar." });
            Assert.True(r.Success, r.Message);
        }

        var hasil = await BatalAsync(id, "Sari");

        Assert.Equal("Cancelled", hasil.TravelStatus);
        Assert.Equal(WorkflowValueConstants.WorkflowStatus.Cancelled, StatusWorkflow(wf));
        Assert.Equal("Cancelled", Baca(id).TravelStatus);
    }

    // Integration 25.2 — batal SPPD yang sudah disetujui melepas gembok dan presensi Dinas Luar dengan pelaku nyata.
    [Fact]
    public async Task Batal_sesudah_disetujui_melepas_gembok_dan_presensi()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);
        await SetujuiAsync(wf);
        Assert.True(Baca(id).HasScheduleConflict);

        var hasil = await BatalAsync(id, "Sari");

        Assert.Equal("Cancelled", hasil.TravelStatus);
        var s = Baca(id);
        Assert.False(s.HasScheduleConflict);
        Assert.Equal("Seminar dibatalkan oleh panitia penyelenggara.", s.CancellationReason);
        Assert.NotNull(s.ApprovedAt);
        Assert.All(Presensi(id), x => { Assert.Equal("Cancelled", x.AttendanceLinkStatus); Assert.Equal(_p["Sari"].User, x.UpdateBy); });
        // Workflow yang sudah selesai disetujui tidak disentuh (engine menyimpan Completed sesudah persetujuan final).
        Assert.Contains(StatusWorkflow(wf), new[] { WorkflowValueConstants.WorkflowStatus.Approved, WorkflowValueConstants.WorkflowStatus.Completed });
    }

    // Aturan lama tetap: Draf tanpa workflow dapat dibatalkan; alasan < 15 karakter ditolak; SPPD ditolak bersifat final.
    [Fact]
    public async Task Batal_Draf_alasan_pendek_dan_SPPD_ditolak()
    {
        var draf = Sppd();
        await Assert.ThrowsAsync<InvalidOperationException>(() => BatalAsync(draf, "Sari", "batal saja"));
        Assert.Equal("Draft", Baca(draf).TravelStatus);
        Assert.Equal("Cancelled", (await BatalAsync(draf, "Sari")).TravelStatus);

        var ditolak = Sppd("Rejected");
        var galat = await Assert.ThrowsAsync<InvalidOperationException>(() => BatalAsync(ditolak, "Sari"));
        Assert.Contains("ditolak", galat.Message);
        Assert.Equal("Rejected", Baca(ditolak).TravelStatus);
    }

    // ---- BE-HRD-378 (API 34.2–34.3, VR-TRV-30/31; HRD-DEC-226/227) ----

    private sealed class TanpaDeklarasi : IMissingReceiptDeclarationService
    {
        public void ValidateMissingReceipts(List<MissingReceiptItemDto> missingReceipts, decimal totalActualAmount) { }
        public Task StageItemsAsync(TrxBusinessTravelRequest travel, IReadOnlyCollection<MissingReceiptItemDto> items, Guid actorUserId) => Task.CompletedTask;
        public Task<List<MissingReceiptDeclarationDto>> GetDeclarationsAsync(Guid settlementId) => throw new NotSupportedException();
        public Task<MissingReceiptDeclarationDto> DeclareAsync(Guid settlementId, MissingReceiptItemDto item, Guid actorUserId) => throw new NotSupportedException();
        public Task<MissingReceiptDeclarationDto> ApproveAsync(Guid itemId, Guid actorUserId) => throw new NotSupportedException();
    }

    private async Task<TravelAdvanceResponseDto> UangMukaAsync(Guid id, decimal nominal)
    {
        await using var ctx = Konteks();
        return await new TravelAdvanceRequestService(ctx).CreateAdvanceRequestAsync(
            new CreateTravelAdvanceRequestDto { BusinessTravelRequestId = id, RequestedAmount = nominal }, _p["Sari"].User, _p["Sari"].Wf);
    }

    private async Task<TravelSettlementResponseDto> SettlementAsync(Guid id)
    {
        await using var ctx = Konteks();
        return await new TravelSettlementService(ctx, new TanpaDeklarasi()).SubmitSettlementAsync(new SubmitTravelSettlementDto
        {
            BusinessTravelRequestId = id,
            ActualExpenses = new List<ActualExpenseItemDto>
            {
                new() { ExpenseDate = new DateOnly(2026, 12, 1), ExpenseDescription = "Tiket kereta Jakarta–Surabaya", ClaimedAmount = 1_200_000m }
            }
        }, _p["Sari"].User);
    }

    private Guid UangMukaLamaDicairkan(Guid id)
    {
        using var ctx = _db.CreateContext();
        var adv = new TrxTravelAdvanceRequest
        {
            Id = Guid.NewGuid(), BusinessTravelRequestId = id, WorkforceProfileId = _p["Sari"].Wf, AdvanceRequestNumber = "ADV/UJI/" + id.ToString("N")[..6],
            RequestDate = new DateOnly(2026, 10, 1), RequestedAmount = 2_000_000m, ApprovedAmount = 2_000_000m, PaidAmount = 2_000_000m,
            AdvanceStatus = "Paid", PaidAt = new DateTime(2026, 10, 2, 3, 0, 0, DateTimeKind.Utc), IsActive = true, CreateBy = _p["Sari"].User, CreateDateTime = DateTime.UtcNow
        };
        ctx.TrxTravelAdvanceRequests.Add(adv);
        ctx.SaveChanges();
        return adv.Id;
    }

    private const string PesanUangMuka = "SPPD belum disetujui. Ajukan SPPD dan tunggu persetujuan sebelum mengajukan uang muka.";
    private const string PesanSettlement = "SPPD belum disetujui sehingga belum dapat dipertanggungjawabkan.";

    // AT-HRD-TRV-30 / VR-TRV-30 / VR-TRV-02 — uang muka ditolak untuk Draf dan Menunggu; lolos sesudah disetujui dengan plafon 80%.
    [Fact]
    public async Task Uang_muka_hanya_untuk_SPPD_disetujui_dengan_plafon_80_persen()
    {
        var id = Sppd();
        var draf = await Assert.ThrowsAsync<BusinessTravelRequestException>(() => UangMukaAsync(id, 1_000_000m));
        Assert.Equal(409, draf.StatusCode);
        Assert.Equal(PesanUangMuka, draf.Message);

        var wf = await AjukanAsync(id);
        var menunggu = await Assert.ThrowsAsync<BusinessTravelRequestException>(() => UangMukaAsync(id, 1_000_000m));
        Assert.Equal(409, menunggu.StatusCode);

        await SetujuiAsync(wf);
        var lebih = await Assert.ThrowsAsync<InvalidOperationException>(() => UangMukaAsync(id, 4_000_001m));
        Assert.Contains("VR-TRV-02", lebih.Message);

        var hasil = await UangMukaAsync(id, 4_000_000m);
        Assert.Equal(4_000_000m, hasil.RequestedAmount);
        using var ctx = _db.CreateContext();
        Assert.Single(ctx.TrxTravelAdvanceRequests.AsNoTracking().Where(x => x.BusinessTravelRequestId == id).ToList());
    }

    // VR-TRV-30 — SPPD dibatalkan/ditolak tetap ditolak (aturan lama), walaupun pernah disetujui.
    [Fact]
    public async Task Uang_muka_ditolak_untuk_SPPD_dibatalkan()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);
        await SetujuiAsync(wf);
        await BatalAsync(id, "Sari");

        await Assert.ThrowsAsync<InvalidOperationException>(() => UangMukaAsync(id, 1_000_000m));
    }

    // AT-HRD-TRV-31 / VR-TRV-31 — settlement ditolak tanpa persetujuan dan tanpa uang muka dicairkan; lolos sesudah disetujui.
    [Fact]
    public async Task Settlement_hanya_untuk_SPPD_disetujui()
    {
        var id = Sppd();
        await AjukanAsync(id);
        var galat = await Assert.ThrowsAsync<BusinessTravelRequestException>(() => SettlementAsync(id));
        Assert.Equal(409, galat.StatusCode);
        Assert.Equal(PesanSettlement, galat.Message);
        using (var ctx = _db.CreateContext())
        {
            Assert.Empty(ctx.TrxTravelSettlements.AsNoTracking().Where(x => x.BusinessTravelRequestId == id).ToList());
        }

        var id2 = Sppd();
        var wf2 = await AjukanAsync(id2);
        await SetujuiAsync(wf2);
        var hasil = await SettlementAsync(id2);
        Assert.Equal(id2, hasil.BusinessTravelRequestId);
    }

    // AT-HRD-TRV-34 / HRD-DEC-227 — SPPD lama "Submitted" tanpa workflow: uang muka ditolak; sesudah diajukan dan disetujui lolos.
    [Fact]
    public async Task SPPD_lama_harus_diajukan_ulang_sebelum_uang_muka()
    {
        var id = Sppd("Submitted");
        using (var ctx = _db.CreateContext())
        {
            var s = ctx.TrxBusinessTravelRequests.Single(x => x.Id == id);
            s.ApprovedBudgetAmount = s.EstimatedTotalAmount; // perilaku lama: anggaran disalin saat dibuat
            s.SubmittedAt = new DateTime(2026, 10, 1, 1, 0, 0, DateTimeKind.Utc);
            ctx.SaveChanges();
        }

        var galat = await Assert.ThrowsAsync<BusinessTravelRequestException>(() => UangMukaAsync(id, 1_000_000m));
        Assert.Equal(409, galat.StatusCode);

        var wf = await AjukanAsync(id);
        await SetujuiAsync(wf);
        var hasil = await UangMukaAsync(id, 1_000_000m);
        Assert.Equal(1_000_000m, hasil.RequestedAmount);
    }

    // AT-HRD-TRV-35 / HRD-DEC-227 — uang muka lama yang sudah dicairkan tetap dapat di-settle walau SPPD belum disetujui.
    [Fact]
    public async Task Uang_muka_lama_dicairkan_tetap_dapat_disettle()
    {
        var id = Sppd("Submitted");
        UangMukaLamaDicairkan(id);

        var hasil = await SettlementAsync(id);

        Assert.Equal(id, hasil.BusinessTravelRequestId);
        using var ctx = _db.CreateContext();
        Assert.Single(ctx.TrxTravelSettlements.AsNoTracking().Where(x => x.BusinessTravelRequestId == id).ToList());
    }

    // ---- BE-HRD-379 (Permission 32.2, HRD-DEC-228) ----

    private static readonly string[] DuaBelasEvent =
    {
        "HRD_TRV_REQUEST_SUBMITTED", "HRD_TRV_REQUEST_APPROVED", "HRD_TRV_ADVANCE_REQUESTED", "HRD_TRV_ADVANCE_PAID",
        "HRD_TRV_SETTLEMENT_SUBMITTED", "HRD_TRV_MISSING_RECEIPT_DECLARED", "HRD_TRV_SETTLEMENT_VERIFIED", "HRD_TRV_SETTLEMENT_CLOSED",
        "HRD_TRV_SPPD_FROZEN_OVERDUE", "HRD_TRV_PAYROLL_DEDUCTION_TRIGGERED", "HRD_TRV_ATTENDANCE_SYNCED", "HRD_TRV_ROSTER_LOCK_APPLIED"
    };

    private async Task<List<TravelAuditEventDto>> JejakAsync(Guid id)
    {
        await using var ctx = _db.CreateContext();
        return (await new TravelAuditTrailService(ctx).GetAuditTrailAsync(id, null, null, 1, 200)).Items.ToList();
    }

    // AT-HRD-TRV-36 — satu SPPD dijalankan sampai settlement ditutup, dengan deklarasi tanpa nota, tunggakan > 14 hari, dan
    // potongan payroll > 30 hari: jejak memuat ke-12 kode matriks; nama lama tidak muncul.
    [Fact]
    public async Task Jejak_audit_memuat_12_event_bernama_matriks()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);
        await SetujuiAsync(wf);
        var uangMuka = await UangMukaAsync(id, 2_000_000m);
        var t0 = new DateTime(2026, 12, 5, 2, 0, 0, DateTimeKind.Utc);
        var alasanPanjang = "Kuitansi taksi bandara hilang saat perjalanan pulang; " + new string('x', 300);
        using (var ctx = _db.CreateContext())
        {
            var adv = ctx.TrxTravelAdvanceRequests.Single(x => x.Id == uangMuka.Id);
            adv.AdvanceStatus = "Paid"; adv.PaidAmount = 2_000_000m; adv.PaidAt = t0;
            var klaim = new TrxTravelExpenseClaim { Id = Guid.NewGuid(), BusinessTravelRequestId = id, WorkforceProfileId = _p["Sari"].Wf, ExpenseClaimNumber = "EXP/UJI/379", IsActive = true, CreateDateTime = t0 };
            ctx.TrxTravelExpenseClaims.Add(klaim);
            ctx.TrxTravelExpenseItems.Add(new TrxTravelExpenseItem
            {
                TravelExpenseClaimId = klaim.Id, TravelExpenseCategoryId = Guid.NewGuid(), ExpenseDescription = "Taksi bandara", ClaimedAmount = 350_000m,
                IsMissingReceiptDeclared = true, MissingReceiptReason = alasanPanjang, MissingReceiptApprovedBy = _p["Bima"].User, IsActive = true, CreateDateTime = t0.AddDays(1)
            });
            ctx.TrxTravelSettlements.Add(new TrxTravelSettlement
            {
                BusinessTravelRequestId = id, SettlementNumber = "STL/UJI/379", SubmittedAt = t0.AddDays(2), SubmittedByUserId = _p["Sari"].User,
                VerifiedAt = t0.AddDays(20), VerifiedByUserId = _p["Bima"].User, DefaultedAt = t0.AddDays(17), SettledAt = t0.AddDays(40), IsActive = true, CreateDateTime = t0.AddDays(2)
            });
            ctx.TrxPayrollDeductionSchedules.Add(new TrxPayrollDeductionSchedule
            {
                ScheduleNumber = "DED-TRV-UJI-379", EmployeeId = Guid.NewGuid(), ExternalReferenceId = $"TRV-ADV-{uangMuka.Id:N}", TotalLoanPrincipal = 1_200_000m,
                MonthlyInstallmentAmount = 1_200_000m, RemainingPrincipal = 1_200_000m, StartDate = t0.AddDays(35), CreateDateTime = t0.AddDays(35)
            });
            ctx.SaveChanges();
        }

        var jejak = await JejakAsync(id);
        var kode = jejak.Select(x => x.EventType).Distinct().ToList();

        Assert.All(DuaBelasEvent, e => Assert.Contains(e, kode));
        Assert.DoesNotContain("HRD_TRV_REQUEST_APPROVAL", kode);
        Assert.DoesNotContain("HRD_TRV_ADVANCE_SUBMITTED", kode);
        Assert.DoesNotContain("HRD_TRV_SETTLEMENT_DEFAULTED", kode);
        Assert.Contains("HRD_TRV_REQUEST_CREATED", kode);

        Assert.Equal(3, jejak.Count(x => x.EventType == "HRD_TRV_ATTENDANCE_SYNCED"));
        Assert.All(jejak.Where(x => x.EventType == "HRD_TRV_ATTENDANCE_SYNCED"), x => Assert.Equal(_p["Bima"].User, x.ActorUserId));
        var disetujui = Assert.Single(jejak, x => x.EventType == "HRD_TRV_REQUEST_APPROVED");
        Assert.Equal(_p["Bima"].User, disetujui.ActorUserId);
        Assert.Matches(@"^SPPD disetujui. Anggaran disetujui 5[.,]000[.,]000.$", disetujui.Detail);
        var diajukan = Assert.Single(jejak, x => x.EventType == "HRD_TRV_REQUEST_SUBMITTED");
        Assert.Equal("SPPD diajukan ke Surabaya, berangkat 2026-12-01.", diajukan.Detail);
        var gembok = Assert.Single(jejak, x => x.EventType == "HRD_TRV_ROSTER_LOCK_APPLIED");
        Assert.Equal("Gembok roster dipasang 2026-12-01 s.d. 2026-12-03.", gembok.Detail);
        Assert.Equal(_p["Bima"].User, gembok.ActorUserId);
        var potongan = Assert.Single(jejak, x => x.EventType == "HRD_TRV_PAYROLL_DEDUCTION_TRIGGERED");
        Assert.Contains("DED-TRV-UJI-379", potongan.Detail);
        var nota = Assert.Single(jejak, x => x.EventType == "HRD_TRV_MISSING_RECEIPT_DECLARED");
        Assert.StartsWith("Deklarasi tanpa nota 350", nota.Detail);
        Assert.Equal(200, nota.Detail[(nota.Detail.IndexOf(": ") + 2)..].Length);
        Assert.Equal(_p["Bima"].User, nota.ActorUserId);
        Assert.Equal("Hak SPPD dibekukan: settlement menunggak lebih dari 14 hari.", Assert.Single(jejak, x => x.EventType == "HRD_TRV_SPPD_FROZEN_OVERDUE").Detail);
    }

    // Event tambahan dan batas: ditolak → REQUEST_REJECTED; tanpa persetujuan tidak ada event persetujuan, presensi, atau gembok;
    // gembok yang sudah dilepas (batal) tidak lagi tercatat sebagai terpasang.
    [Fact]
    public async Task Jejak_audit_ditolak_dan_gembok_dilepas()
    {
        var id = Sppd();
        var wf = await AjukanAsync(id);
        await using (var ctx = Konteks())
        {
            var r = await Workflow(ctx, _p["Bima"].User).RejectAsync(wf, TugasBima(wf), new WorkflowRejectRequest { RejectionReasonId = _alasanTolak, Comment = "Seminar tidak relevan dengan tugas di ICU." });
            Assert.True(r.Success, r.Message);
        }

        var kode = (await JejakAsync(id)).Select(x => x.EventType).ToList();
        Assert.Contains("HRD_TRV_REQUEST_REJECTED", kode);
        Assert.Contains("HRD_TRV_REQUEST_SUBMITTED", kode);
        Assert.DoesNotContain("HRD_TRV_REQUEST_APPROVED", kode);
        Assert.DoesNotContain("HRD_TRV_ATTENDANCE_SYNCED", kode);
        Assert.DoesNotContain("HRD_TRV_ROSTER_LOCK_APPLIED", kode);

        var id2 = Sppd();
        var wf2 = await AjukanAsync(id2);
        await SetujuiAsync(wf2);
        Assert.Contains("HRD_TRV_ROSTER_LOCK_APPLIED", (await JejakAsync(id2)).Select(x => x.EventType));
        await BatalAsync(id2, "Sari");
        Assert.DoesNotContain("HRD_TRV_ROSTER_LOCK_APPLIED", (await JejakAsync(id2)).Select(x => x.EventType));
    }

    [Fact]
    public void CanHandle_hanya_untuk_referensi_SPPD()
    {
        using var ctx = _db.CreateContext();
        var h = Penangan(ctx);
        Assert.True(h.CanHandle("HRD_TRV_REQUEST"));
        Assert.True(h.CanHandle(" HRD_TRV_REQUEST "));
        Assert.False(h.CanHandle("PBJ_PR"));
        Assert.False(h.CanHandle("LEAVE_REQUEST"));
        Assert.False(h.CanHandle(""));
    }
}
