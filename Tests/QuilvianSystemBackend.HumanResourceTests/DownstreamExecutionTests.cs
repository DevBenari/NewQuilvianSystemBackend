using System.Data.Common;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Hubs;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-316 — WF-B8 eksekusi hilir sesudah persetujuan final (kartu roadmap: AT-HRD-WF-18 idempoten, AT-HRD-WF-19 pengingat Plh).
// Jalur yang diuji: cuti tahunan pejabat struktural 3 hari (saldo 12) disetujui final → status cuti Approved, saldo terpotong 3 hari
// sekali saja (INT-HR-93), dan pejabat tanpa Plh yang menutup masa cutinya menerima pengingat menunjuk Plh sekali saja (INT-HR-94).
public sealed class DownstreamExecutionTests : IDisposable
{
    private static readonly DateOnly Hari = DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _kepalaWf = Guid.NewGuid(), _kepalaEmp = Guid.NewGuid(), _akunKepala = Guid.NewGuid(), _penyetuju = Guid.NewGuid(), _stafWf = Guid.NewGuid();
    private readonly Guid _tipe = Guid.NewGuid(), _kebijakan = Guid.NewGuid(), _saldo = Guid.NewGuid(), _cuti = Guid.NewGuid(), _workflow = Guid.NewGuid();

    private sealed class HubTidakAda : IHubContext<HrNotificationHub>
    {
        public IHubClients Clients => throw new NotSupportedException("SignalR tidak dipakai di harness");
        public IGroupManager Groups => throw new NotSupportedException("SignalR tidak dipakai di harness");
    }

    // Kunci baris saldo memakai SQL PostgreSQL (`public."WfpLeaveBalance" … FOR UPDATE`); di SQLite skema dan klausa kunci dilepas.
    // Perilaku kunci PostgreSQL sendiri tidak dibuktikan di sini.
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

    public DownstreamExecutionTests()
    {
        using var ctx = _db.CreateContext();
        ctx.Users.Add(new ApplicationUser { Id = _akunKepala, UserName = "kepala", UserCode = "KEPALA01", DisplayName = "Kepala Ruang ICU", UserType = UserType.Employee, IsActive = true, WorkforceProfileId = _kepalaWf });
        ctx.Set<MstLeaveType>().Add(new MstLeaveType { Id = _tipe, LeaveTypeCode = "CT", LeaveTypeName = "Cuti Tahunan", LeaveCategory = "Annual", IsBalanceDeducted = true, IsActive = true });
        ctx.Set<MstLeavePolicy>().Add(new MstLeavePolicy
        {
            Id = _kebijakan, LeaveTypeId = _tipe, LeavePolicyCode = "CT-STD", LeavePolicyName = "Cuti Tahunan Standar",
            ReservationTiming = LeaveValueConstants.ReservationTiming.None, DeductionTiming = LeaveValueConstants.DeductionTiming.OnApproval,
        });
        ctx.WfpLeaveBalances.Add(new WfpLeaveBalance
        {
            Id = _saldo, WorkforceProfileId = _kepalaWf, LeaveTypeId = _tipe, LeavePolicyId = _kebijakan, Year = Hari.Year,
            EntitlementDays = 12, RemainingDays = 12, AvailableDays = 12, BalanceStatus = LeaveValueConstants.BalanceStatus.Active,
        });
        ctx.WfpLeaveRequests.Add(new WfpLeaveRequest
        {
            Id = _cuti, RequestNumber = "LV-001", WorkforceProfileId = _kepalaWf, EmployeeId = _kepalaEmp, LeaveTypeId = _tipe, LeavePolicyId = _kebijakan, LeaveBalanceId = _saldo,
            StartDate = Hari.AddDays(10), EndDate = Hari.AddDays(12), RequestedDays = 3, EstimatedBalanceDeduction = 3, Reason = "Urusan keluarga",
            LeaveRequestStatus = LeaveRequestValueConstants.Status.WaitingApproval,
        });
        ctx.TrxWorkflowInstances.Add(new TrxWorkflowInstance
        {
            Id = _workflow, WorkflowDefinitionId = Guid.NewGuid(), RequestedByUserId = _akunKepala, RequestedByWorkforceProfileId = _kepalaWf, RequestedByEmployeeId = _kepalaEmp,
            ReferenceType = WorkflowReferenceLifecycleService.LeaveRequestReferenceType, ReferenceId = _cuti, WorkflowStatus = WorkflowValueConstants.WorkflowStatus.Approved, CurrentStepOrder = 2,
        });
        // Pemohon adalah atasan yang menyetujui permohonan staf (pejabat struktural menurut INT-HR-94).
        ctx.WfpManagerAssignments.Add(new WfpManagerAssignment
        { WorkforceProfileId = _stafWf, ManagerWorkforceProfileId = _kepalaWf, ManagerType = "Direct", EffectiveStartDate = Hari.AddYears(-1).ToDateTime(TimeOnly.MinValue), IsPrimaryManager = true, CanApproveRequests = true, IsActive = true });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private async Task Eksekusi()
    {
        await using var ctx = _db.CreateContext(new KunciPostgresKeSqlite());
        var lifecycle = new WorkflowReferenceLifecycleService(
            ctx, null!, null!, null!,
            new LeaveRequestWorkflowLifecycleService(ctx, new LeaveRequestBalanceLifecycleService(ctx), NullLogger<LeaveRequestWorkflowLifecycleService>.Instance),
            null!, null!, null!, null!, null!, null!, null!,
            NullLogger<WorkflowReferenceLifecycleService>.Instance, Array.Empty<IWorkflowReferenceLifecycleHandler>());
        var http = new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, _penyetuju.ToString()) }, "uji")) } };
        var layanan = new IdempotentDownstreamExecutionService(lifecycle, http, ctx, new HrdNotificationService(ctx, new HubTidakAda(), NullLogger<HrdNotificationService>.Instance), NullLogger<IdempotentDownstreamExecutionService>.Instance);
        await layanan.ExecuteDownstreamActionsAsync(_workflow);
    }

    private (WfpLeaveRequest Cuti, WfpLeaveBalance Saldo, List<TrxLeaveBalanceTransaction> Mutasi, List<Areas.Corporate.HumanResource.HrServiceManagement.Models.HrdNotification> Pemberitahuan) Keadaan()
    {
        using var ctx = _db.CreateContext();
        return (ctx.WfpLeaveRequests.AsNoTracking().Single(x => x.Id == _cuti), ctx.WfpLeaveBalances.AsNoTracking().Single(x => x.Id == _saldo),
            ctx.TrxLeaveBalanceTransactions.AsNoTracking().Where(x => x.LeaveRequestId == _cuti).ToList(), ctx.HrdNotifications.AsNoTracking().ToList());
    }

    private void Ubah(Action<ApplicationDbContext> ubah)
    {
        using var ctx = _db.CreateContext();
        ubah(ctx);
        ctx.SaveChanges();
    }

    // AT-HRD-WF-18 (kartu roadmap): persetujuan final memperbarui domain hilir seketika, dan eksekusi ulang tidak menggandakan transaksi.
    [Fact]
    public async Task WF18_persetujuan_final_memotong_saldo_sekali_walau_dieksekusi_ulang()
    {
        await Eksekusi();
        var k = Keadaan();
        Assert.Equal(LeaveRequestValueConstants.Status.Approved, k.Cuti.LeaveRequestStatus);
        Assert.Equal((3m, 9m), (k.Saldo.UsedDays, k.Saldo.AvailableDays));
        var m = Assert.Single(k.Mutasi);
        Assert.Equal(($"LEAVE-REQUEST-DEDUCTION:{_cuti:N}:ON_APPROVAL", LeaveValueConstants.TransactionType.Deduction, 3m), (m.IdempotencyKey, m.TransactionType, m.UsedDelta));

        await Eksekusi();
        await Eksekusi();
        var ulang = Keadaan();
        Assert.Single(ulang.Mutasi);
        Assert.Equal((3m, 9m), (ulang.Saldo.UsedDays, ulang.Saldo.AvailableDays));
    }

    // AT-HRD-WF-19 (kartu roadmap, INT-HR-94): pejabat yang cutinya disetujui tanpa Plh menerima pengingat menunjuk Plh, sekali saja.
    [Fact]
    public async Task WF19_pejabat_tanpa_Plh_menerima_pengingat_sekali()
    {
        await Eksekusi();
        await Eksekusi();
        var p = Assert.Single(Keadaan().Pemberitahuan);
        Assert.Equal((_akunKepala, HrdNotificationPriorities.High, HrdNotificationCategories.Workflow, _cuti), (p.RecipientUserId, p.Priority, p.Category, p.ReferenceId));
        Assert.Equal("Cuti Anda disetujui — tunjuk Pelaksana Harian (Plh) sebelum cuti dimulai", p.Title);
        Assert.Equal($"/hrd/workflow/delegations/new?leaveStart={Hari.AddDays(10):yyyy-MM-dd}&leaveEnd={Hari.AddDays(12):yyyy-MM-dd}", p.DeepLinkUrl);
    }

    // Plh yang menutup seluruh masa cuti → tanpa pengingat; Plh yang hanya menutup sebagian → tetap diingatkan.
    [Theory]
    [InlineData(9, 13, 0)]
    [InlineData(10, 11, 1)]
    public async Task WF19_delegasi_Plh_yang_menutup_masa_cuti_meniadakan_pengingat(int mulai, int akhir, int diharapkan)
    {
        Ubah(ctx => ctx.TrxApprovalDelegations.Add(new TrxApprovalDelegation
        {
            DelegatorUserId = _akunKepala, DelegatorWorkforceProfileId = _kepalaWf, DelegateUserId = Guid.NewGuid(), DelegationNumber = "DLG-001", DelegationStatus = "Active", AppliesToAllWorkflows = true,
            EffectiveStartAt = Hari.AddDays(mulai).ToDateTime(TimeOnly.MinValue), EffectiveEndAt = Hari.AddDays(akhir).ToDateTime(TimeOnly.MaxValue), IsActive = true,
        }));
        await Eksekusi();
        Assert.Equal(diharapkan, Keadaan().Pemberitahuan.Count);
    }

    // Bukan pejabat (tidak memegang penempatan atasan yang boleh menyetujui) → tanpa pengingat; saldo tetap terpotong.
    [Fact]
    public async Task WF19_bukan_pejabat_tidak_diingatkan()
    {
        Ubah(ctx => ctx.WfpManagerAssignments.Single().CanApproveRequests = false);
        await Eksekusi();
        var k = Keadaan();
        Assert.Empty(k.Pemberitahuan);
        Assert.Equal(3m, k.Saldo.UsedDays);
    }

    // Workflow belum final (masih menunggu) → cuti tetap menunggu, saldo tidak dipotong, tanpa pengingat.
    [Fact]
    public async Task Workflow_belum_final_tidak_memotong_saldo_dan_tidak_mengingatkan()
    {
        Ubah(ctx => ctx.TrxWorkflowInstances.Single().WorkflowStatus = WorkflowValueConstants.WorkflowStatus.InProgress);
        await Eksekusi();
        var k = Keadaan();
        Assert.Equal(LeaveRequestValueConstants.Status.WaitingApproval, k.Cuti.LeaveRequestStatus);
        Assert.Equal((0m, 12m), (k.Saldo.UsedDays, k.Saldo.AvailableDays));
        Assert.Empty(k.Mutasi);
        Assert.Empty(k.Pemberitahuan);
    }
}
