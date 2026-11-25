using System.Data.Common;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Controllers;
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
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-375 (kontrak v28 API 34.1/34.4/34.5, Validation VR-TRV-26..29, State 32.2, Permission 32.1; HRD-DEC-224, 225, 227) —
// SPPD dibuat sebagai Draf, isi terkunci selama menunggu, dan Ajukan memulai Workflow V2 NYATA (definisi BUSINESS_TRAVEL_REQUEST
// dari seeder BE-HRD-374, tugas untuk atasan langsung pemohon). AT-HRD-TRV-23, 24, 25, 37, 34 (bagian ajukan).
public sealed class TravelSubmitTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _unit = Guid.NewGuid();
    private readonly Dictionary<string, (Guid Wf, Guid User)> _p = new();

    // Workflow V2 mengunci baris dengan SQL PostgreSQL (public."…" FOR UPDATE); SQLite tidak mengenalnya.
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

    // Mensimulasikan submit Workflow V2 yang gagal sesudah instance draf berhasil dibuat: penyimpanan yang memindahkan
    // instance dari Draft ke status lain ditolak (Workflow V2 mengubahnya menjadi hasil gagal 409).
    private sealed class GagalkanSubmitWorkflow : SaveChangesInterceptor
    {
        private static void Periksa(DbContext? ctx)
        {
            if (ctx == null) return;
            var keluarDraf = ctx.ChangeTracker.Entries<TrxWorkflowInstance>().Any(e =>
                e.State == EntityState.Modified &&
                (string?)e.OriginalValues[nameof(TrxWorkflowInstance.WorkflowStatus)] == WorkflowValueConstants.WorkflowStatus.Draft &&
                e.Entity.WorkflowStatus != WorkflowValueConstants.WorkflowStatus.Draft);
            if (keluarDraf) throw new DbUpdateException("Simulasi kegagalan submit workflow.");
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData e, InterceptionResult<int> r) { Periksa(e.Context); return r; }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData e, InterceptionResult<int> r, CancellationToken t = default) { Periksa(e.Context); return ValueTask.FromResult(r); }
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

    public TravelSubmitTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _unit, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        var mulai = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        foreach (var nama in new[] { "Sari", "Bima", "Budi", "Tono" })
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

        // Sari dan Budi beratasan langsung Bima (Kepala Ruang); Tono tidak punya atasan (VR-TRV-29).
        foreach (var nama in new[] { "Sari", "Budi" })
        {
            ctx.WfpManagerAssignments.Add(new WfpManagerAssignment
            {
                WorkforceProfileId = _p[nama].Wf, ManagerWorkforceProfileId = _p["Bima"].Wf, ManagerType = "Direct", EffectiveStartDate = mulai,
                IsPrimaryManager = true, CanApproveRequests = true, IsActive = true
            });
        }

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
        // Komposisi produksi: penangan SPPD (BE-HRD-376) terdaftar pada Workflow V2.
        var handler = new BusinessTravelWorkflowLifecycleHandler(ctx, new TravelAttendanceAndRosterLockService(ctx), NullLogger<BusinessTravelWorkflowLifecycleHandler>.Instance);
        var lifecycle = new WorkflowReferenceLifecycleService(
            ctx, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!, null!,
            NullLogger<WorkflowReferenceLifecycleService>.Instance, new IWorkflowReferenceLifecycleHandler[] { handler });
        return new WorkflowService(ctx, new HumanResourceContextService(ctx, http, userManager), userManager, http, lifecycle);
    }

    private static BusinessTravelRequestService Layanan(ApplicationDbContext ctx, Guid pelaku) =>
        new(ctx, new TanpaKonflik(), new TarifNol(), Workflow(ctx, pelaku), new TravelAttendanceAndRosterLockService(ctx));

    private Guid Sppd(string pemohon, string status = BusinessTravelRequestStatuses.Draft, Guid? workflowInstanceId = null, DateTime? approvedAt = null)
    {
        using var ctx = _db.CreateContext();
        var id = Guid.NewGuid();
        ctx.TrxBusinessTravelRequests.Add(new TrxBusinessTravelRequest
        {
            Id = id, TravelRequestNumber = "SPPD/UJI/" + id.ToString("N")[..6], WorkforceProfileId = _p[pemohon].Wf, TravelTypeId = Guid.NewGuid(),
            TravelTitle = "Seminar ICU", TravelPurpose = "Seminar nasional perawatan intensif", Origin = "Jakarta", Destination = "Surabaya",
            StartDate = new DateOnly(2026, 12, 1), EndDate = new DateOnly(2026, 12, 3), TravelDayCount = 3,
            EstimatedTotalAmount = 5_000_000m, EstimatedTransportationAmount = 5_000_000m, ApprovedBudgetAmount = 0m,
            TravelStatus = status, WorkflowInstanceId = workflowInstanceId, ApprovedAt = approvedAt,
            IsActive = true, CreateBy = _p[pemohon].User, CreateDateTime = DateTime.UtcNow
        });
        ctx.SaveChanges();
        return id;
    }

    private TrxBusinessTravelRequest Baca(Guid id)
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxBusinessTravelRequests.AsNoTracking().Single(x => x.Id == id);
    }

    private async Task<BusinessTravelDetailResponseDto> AjukanAsync(Guid id, string pelaku)
    {
        await using var ctx = Konteks();
        return await Layanan(ctx, _p[pelaku].User).SubmitRequestAsync(id, _p[pelaku].User, _p[pelaku].Wf);
    }

    private async Task<BusinessTravelRequestException> AjukanGagalAsync(Guid id, string pelaku, Guid? wfPelaku = null)
    {
        await using var ctx = Konteks();
        return await Assert.ThrowsAsync<BusinessTravelRequestException>(() =>
            Layanan(ctx, _p[pelaku].User).SubmitRequestAsync(id, _p[pelaku].User, wfPelaku ?? _p[pelaku].Wf));
    }

    private static UpdateBusinessTravelRequestDto Ubahan(string tujuan) => new()
    {
        TravelTitle = "Seminar ICU", TravelPurpose = "Seminar nasional perawatan intensif", Origin = "Jakarta", Destination = tujuan,
        StartDate = new DateOnly(2026, 12, 1), EndDate = new DateOnly(2026, 12, 3), EstimatedTransportationAmount = 4_000_000m, EstimatedAllowanceAmount = 1_500_000m
    };

    // AT-HRD-TRV-23 — SPPD baru berstatus Draf, belum ke atasan, anggaran disetujui 0.
    [Fact]
    public async Task Buat_SPPD_menghasilkan_Draf_tanpa_workflow()
    {
        await using var ctx = Konteks();
        var hasil = await Layanan(ctx, _p["Sari"].User).CreateRequestAsync(new CreateBusinessTravelRequestDto
        {
            TravelTitle = "Seminar ICU", TravelPurpose = "Seminar nasional perawatan intensif", Origin = "Jakarta", Destination = "Surabaya",
            StartDate = new DateOnly(2026, 12, 1), EndDate = new DateOnly(2026, 12, 3),
            EstimatedTransportationAmount = 3_000_000m, EstimatedAccommodationAmount = 1_500_000m, EstimatedAllowanceAmount = 500_000m
        }, _p["Sari"].User, _p["Sari"].Wf);

        Assert.Equal("Draft", hasil.TravelStatus);
        Assert.Null(hasil.WorkflowInstanceId);
        Assert.Null(hasil.SubmittedAt);
        Assert.Null(hasil.ApprovedAt);
        Assert.False(hasil.IsApproved);
        Assert.True(hasil.CanSubmit);
        Assert.Equal(5_000_000m, hasil.EstimatedTotalAmount);
        Assert.Equal(0m, hasil.ApprovedBudgetAmount);

        var simpan = Baca(hasil.Id);
        Assert.Equal("Draft", simpan.TravelStatus);
        Assert.Null(simpan.SubmittedByUserId);
        Assert.Empty(ctx.TrxWorkflowInstances.AsNoTracking().Where(x => x.ReferenceId == hasil.Id).ToList());
    }

    // AT-HRD-TRV-24 — Ajukan membuat instance BUSINESS_TRAVEL_REQUEST dan tugas untuk atasan langsung pemohon.
    [Fact]
    public async Task Ajukan_memulai_workflow_dengan_tugas_untuk_atasan_langsung()
    {
        var id = Sppd("Sari");

        var hasil = await AjukanAsync(id, "Sari");

        Assert.Equal("Submitted", hasil.TravelStatus);
        Assert.NotNull(hasil.WorkflowInstanceId);
        Assert.NotNull(hasil.SubmittedAt);
        Assert.False(hasil.CanSubmit);
        Assert.False(hasil.IsApproved);
        Assert.Equal(1, hasil.CurrentApprovalStep);

        var sppd = Baca(id);
        Assert.Equal(_p["Sari"].User, sppd.SubmittedByUserId);
        Assert.Equal(0m, sppd.ApprovedBudgetAmount);

        using var ctx = _db.CreateContext();
        var wf = ctx.TrxWorkflowInstances.AsNoTracking().Single(x => x.Id == hasil.WorkflowInstanceId);
        Assert.Equal("HRD_TRV_REQUEST", wf.ReferenceType);
        Assert.Equal(id, wf.ReferenceId);
        Assert.Contains(wf.WorkflowStatus, new[] { WorkflowValueConstants.WorkflowStatus.Submitted, WorkflowValueConstants.WorkflowStatus.InProgress });
        var definisi = ctx.Set<MstWorkflowDefinition>().AsNoTracking().Single(x => x.Id == wf.WorkflowDefinitionId);
        Assert.Equal("BUSINESS_TRAVEL_REQUEST", definisi.WorkflowCode);
        Assert.Equal(definisi.Id, sppd.WorkflowDefinitionId);

        var tugas = ctx.TrxWorkflowApproverAssignments.AsNoTracking().Where(x => x.WorkflowInstanceId == wf.Id).ToList();
        var satu = Assert.Single(tugas);
        Assert.Equal(_p["Bima"].User, satu.AssignedApproverUserId);
    }

    // AT-HRD-TRV-25 / VR-TRV-27 — isi terkunci selama menunggu; Draf tetap dapat diubah tanpa mengisi anggaran disetujui.
    [Fact]
    public async Task Ubah_ditolak_409_saat_menunggu_dan_Draf_tetap_dapat_diubah()
    {
        var id = Sppd("Sari");
        await using (var ctx = Konteks())
        {
            var ubah = await Layanan(ctx, _p["Sari"].User).UpdateRequestAsync(id, Ubahan("Malang"), _p["Sari"].User);
            Assert.Equal("Malang", ubah.Destination);
            Assert.Equal(0m, ubah.ApprovedBudgetAmount);
            Assert.Equal(5_500_000m, ubah.EstimatedTotalAmount);
        }

        await AjukanAsync(id, "Sari");

        await using (var ctx = Konteks())
        {
            var galat = await Assert.ThrowsAsync<BusinessTravelRequestException>(() =>
                Layanan(ctx, _p["Sari"].User).UpdateRequestAsync(id, Ubahan("Bali"), _p["Sari"].User));
            Assert.Equal(409, galat.StatusCode);
            Assert.Equal("SPPD yang sedang menunggu persetujuan atau sudah diputuskan tidak dapat diubah.", galat.Message);
        }

        Assert.Equal("Malang", Baca(id).Destination);
    }

    // VR-TRV-27 — status yang sudah diputuskan tidak dapat diubah.
    [Theory]
    [InlineData("Approved")]
    [InlineData("Rejected")]
    [InlineData("Cancelled")]
    [InlineData("AdvancePaid")]
    public async Task Ubah_ditolak_409_untuk_status_sudah_diputuskan(string status)
    {
        var id = Sppd("Sari", status);
        await using var ctx = Konteks();
        var galat = await Assert.ThrowsAsync<BusinessTravelRequestException>(() =>
            Layanan(ctx, _p["Sari"].User).UpdateRequestAsync(id, Ubahan("Bali"), _p["Sari"].User));
        Assert.Equal(409, galat.StatusCode);
        Assert.Equal("Surabaya", Baca(id).Destination);
    }

    // VR-TRV-26 — Ajukan dari status yang tidak sah.
    [Theory]
    [InlineData("Approved")]
    [InlineData("Cancelled")]
    [InlineData("AdvancePaid")]
    public async Task Ajukan_ditolak_409_dari_status_tidak_sah(string status)
    {
        var id = Sppd("Sari", status);

        var galat = await AjukanGagalAsync(id, "Sari");

        Assert.Equal(409, galat.StatusCode);
        Assert.Equal("SPPD hanya dapat diajukan dari status Draft atau Dikembalikan untuk revisi.", galat.Message);
        Assert.Equal(status, Baca(id).TravelStatus);
    }

    // VR-TRV-26 — SPPD yang sudah menunggu dengan workflow tidak dapat diajukan dua kali.
    [Fact]
    public async Task Ajukan_kedua_kali_ditolak_409_dan_tidak_membuat_workflow_baru()
    {
        var id = Sppd("Sari");
        await AjukanAsync(id, "Sari");

        var galat = await AjukanGagalAsync(id, "Sari");

        Assert.Equal(409, galat.StatusCode);
        using var ctx = _db.CreateContext();
        Assert.Single(ctx.TrxWorkflowInstances.AsNoTracking().Where(x => x.ReferenceId == id && !x.IsDelete).ToList());
    }

    // VR-TRV-28 — penolakan bersifat final.
    [Fact]
    public async Task Ajukan_SPPD_ditolak_409_dengan_pesan_buat_baru()
    {
        var id = Sppd("Sari", "Rejected");

        var galat = await AjukanGagalAsync(id, "Sari");

        Assert.Equal(409, galat.StatusCode);
        Assert.Equal("SPPD yang ditolak tidak dapat diajukan ulang. Buat SPPD baru.", galat.Message);
    }

    // AT-HRD-TRV-37 (bagian pemohon) — hanya pemohon yang boleh mengajukan.
    [Fact]
    public async Task Ajukan_oleh_bukan_pemohon_ditolak_403()
    {
        var id = Sppd("Sari");

        var olehBudi = await AjukanGagalAsync(id, "Budi");
        var tanpaProfil = await AjukanGagalAsync(id, "Sari", Guid.Empty);

        Assert.Equal(403, olehBudi.StatusCode);
        Assert.Equal("Hanya pemohon SPPD yang dapat mengajukan SPPD ini.", olehBudi.Message);
        Assert.Equal(403, tanpaProfil.StatusCode);
        Assert.Equal("Draft", Baca(id).TravelStatus);
        using var ctx = _db.CreateContext();
        Assert.Empty(ctx.TrxWorkflowInstances.AsNoTracking().Where(x => x.ReferenceId == id).ToList());
    }

    // VR-TRV-29 — penyetuju tidak dapat ditentukan: pesan Workflow V2 diteruskan, SPPD tidak berubah, draf workflow dibuang.
    [Fact]
    public async Task Ajukan_tanpa_atasan_ditolak_dan_SPPD_tetap_Draf()
    {
        var id = Sppd("Tono");

        var galat = await AjukanGagalAsync(id, "Tono");

        Assert.Equal(400, galat.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(galat.Message));
        var sppd = Baca(id);
        Assert.Equal("Draft", sppd.TravelStatus);
        Assert.Null(sppd.WorkflowInstanceId);
        Assert.Null(sppd.WorkflowDefinitionId);
        Assert.Null(sppd.SubmittedAt);
        using var ctx = _db.CreateContext();
        Assert.Empty(ctx.TrxWorkflowInstances.AsNoTracking().Where(x => x.ReferenceId == id && !x.IsDelete).ToList());
    }

    // VR-TRV-29 — instance dibuat tetapi submit gagal: SPPD kembali seperti sebelum Ajukan, draf workflow dibuang,
    // dan Ajukan berikutnya berhasil dengan instance baru.
    [Fact]
    public async Task Submit_workflow_gagal_mengembalikan_SPPD_dan_membuang_draf_workflow()
    {
        var id = Sppd("Sari");

        await using (var ctx = _db.CreateContext(new KunciPostgresKeSqlite(), new GagalkanSubmitWorkflow()))
        {
            var galat = await Assert.ThrowsAsync<BusinessTravelRequestException>(() =>
                Layanan(ctx, _p["Sari"].User).SubmitRequestAsync(id, _p["Sari"].User, _p["Sari"].Wf));
            Assert.Equal(409, galat.StatusCode);
        }

        var sppd = Baca(id);
        Assert.Equal("Draft", sppd.TravelStatus);
        Assert.Null(sppd.WorkflowInstanceId);
        Assert.Null(sppd.WorkflowDefinitionId);
        Assert.Null(sppd.SubmittedAt);
        using (var ctx = _db.CreateContext())
        {
            var semua = ctx.TrxWorkflowInstances.AsNoTracking().Where(x => x.ReferenceId == id).ToList();
            var draf = Assert.Single(semua);
            Assert.True(draf.IsDelete);
            Assert.Equal(WorkflowValueConstants.WorkflowStatus.Draft, draf.WorkflowStatus);
        }

        var ulang = await AjukanAsync(id, "Sari");
        Assert.Equal("Submitted", ulang.TravelStatus);
        using var c = _db.CreateContext();
        Assert.Single(c.TrxWorkflowInstances.AsNoTracking().Where(x => x.ReferenceId == id && !x.IsDelete).ToList());
    }

    // VR-TRV-29 — definisi workflow dinonaktifkan admin: 409, SPPD tidak berubah.
    [Fact]
    public async Task Ajukan_saat_definisi_nonaktif_ditolak_409()
    {
        using (var ctx = _db.CreateContext())
        {
            var d = ctx.Set<MstWorkflowDefinition>().Single(x => x.WorkflowCode == BusinessTravelWorkflowCodes.DefinitionCode);
            d.IsActive = false;
            ctx.SaveChanges();
        }

        var id = Sppd("Sari");
        var galat = await AjukanGagalAsync(id, "Sari");

        Assert.Equal(409, galat.StatusCode);
        Assert.Contains("BUSINESS_TRAVEL_REQUEST", galat.Message);
        Assert.Equal("Draft", Baca(id).TravelStatus);
        Assert.Null(Baca(id).WorkflowInstanceId);
    }

    // AT-HRD-TRV-34 (bagian ajukan) / HRD-DEC-227 — SPPD lama "Submitted" tanpa workflow dapat diajukan.
    [Fact]
    public async Task SPPD_lama_Submitted_tanpa_workflow_dapat_diajukan()
    {
        var id = Sppd("Sari", "Submitted");
        await using (var ctx = Konteks())
        {
            var detail = await Layanan(ctx, _p["Sari"].User).GetRequestByIdAsync(id);
            Assert.True(detail.CanSubmit);
        }

        var hasil = await AjukanAsync(id, "Sari");

        Assert.Equal("Submitted", hasil.TravelStatus);
        Assert.NotNull(hasil.WorkflowInstanceId);
        Assert.False(hasil.CanSubmit);
    }

    // State 32.2 — SPPD yang dikembalikan diajukan ulang memakai instance yang sama.
    [Fact]
    public async Task Ajukan_ulang_sesudah_dikembalikan_memakai_instance_yang_sama()
    {
        var id = Sppd("Sari");
        var pertama = await AjukanAsync(id, "Sari");

        Guid assignmentId;
        using (var ctx = _db.CreateContext())
        {
            assignmentId = ctx.TrxWorkflowApproverAssignments.AsNoTracking().Single(x => x.WorkflowInstanceId == pertama.WorkflowInstanceId).Id;
        }

        await using (var ctx = Konteks())
        {
            var kembalikan = await Workflow(ctx, _p["Bima"].User).RequestRevisionAsync(
                pertama.WorkflowInstanceId!.Value, assignmentId, new WorkflowRequestRevisionRequest { Comment = "Hotel terlalu mahal, ganti yang sesuai plafon." });
            Assert.True(kembalikan.Success, kembalikan.Message);
        }

        // BE-HRD-376: penangan SPPD menyalin "dikembalikan" ke status SPPD.
        Assert.Equal(BusinessTravelRequestStatuses.NeedRevision, Baca(id).TravelStatus);

        await using (var ctx = Konteks())
        {
            var ubah = await Layanan(ctx, _p["Sari"].User).UpdateRequestAsync(id, Ubahan("Surabaya"), _p["Sari"].User);
            Assert.Equal("NeedRevision", ubah.TravelStatus);
        }

        var kedua = await AjukanAsync(id, "Sari");

        Assert.Equal("Submitted", kedua.TravelStatus);
        Assert.Equal(pertama.WorkflowInstanceId, kedua.WorkflowInstanceId);
        using var c = _db.CreateContext();
        Assert.Single(c.TrxWorkflowInstances.AsNoTracking().Where(x => x.ReferenceId == id && !x.IsDelete).ToList());
        var status = c.TrxWorkflowInstances.AsNoTracking().Single(x => x.Id == kedua.WorkflowInstanceId).WorkflowStatus;
        Assert.Contains(status, new[] { WorkflowValueConstants.WorkflowStatus.Submitted, WorkflowValueConstants.WorkflowStatus.InProgress });
    }

    [Fact]
    public async Task Ajukan_SPPD_tidak_ada_404()
    {
        await using var ctx = Konteks();
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            Layanan(ctx, _p["Sari"].User).SubmitRequestAsync(Guid.NewGuid(), _p["Sari"].User, _p["Sari"].Wf));
    }

    // AT-HRD-TRV-37 (bagian hak) / API 34.1 — endpoint Ajukan dijaga hak yang sama dengan membuat SPPD.
    [Fact]
    public void Endpoint_submit_sesuai_kontrak()
    {
        var aksi = typeof(BusinessTravelRequestsController).GetMethod(nameof(BusinessTravelRequestsController.SubmitRequest))!;
        Assert.Equal("{id}/submit", aksi.GetCustomAttribute<HttpPostAttribute>()!.Template);
        var izin = aksi.GetCustomAttribute<AccessPermissionAttribute>()!;
        var aksiAkses = aksi.GetCustomAttribute<AccessActionAttribute>()!;
        var buat = typeof(BusinessTravelRequestsController).GetMethod(nameof(BusinessTravelRequestsController.CreateRequest))!;
        var izinBuat = buat.GetCustomAttribute<AccessPermissionAttribute>()!;
        Assert.Equal(((string)izinBuat.Arguments![0], (string)izinBuat.Arguments[1]), ((string)izin.Arguments![0], (string)izin.Arguments[1]));
        Assert.Equal(("BusinessTravelRequest", "Create"), ((string)izin.Arguments[0], (string)izin.Arguments[1]));
        Assert.Equal("Create", aksiAkses.ActionName);
        Assert.NotNull(typeof(BusinessTravelRequestsController).GetCustomAttribute<AuthorizeAttribute>());
        var kode = aksi.GetCustomAttributes<ProducesResponseTypeAttribute>().Select(x => x.StatusCode).OrderBy(x => x).ToArray();
        Assert.Equal(new[] { 200, 400, 403, 404, 409 }, kode);
    }
}
