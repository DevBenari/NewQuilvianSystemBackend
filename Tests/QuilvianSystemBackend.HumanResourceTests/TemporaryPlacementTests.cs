using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-341 — AT-HRD-PK-45, HRD-ASM-82, INT-HR-104: penempatan Temporary sesudah roster asal terbit
// memberi tahu Kepala Unit asal; roster tidak berubah.
public sealed class TemporaryPlacementTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _icu = Guid.NewGuid(), _hcu = Guid.NewGuid(), _igd = Guid.NewGuid(), _pagi = Guid.NewGuid();
    private readonly Guid _tono = Guid.NewGuid(), _sari = Guid.NewGuid(), _akunSari = Guid.NewGuid(), _akunTono = Guid.NewGuid();
    private readonly Guid _rosterIcu = Guid.NewGuid(), _rosterIcuDraf = Guid.NewGuid(), _rosterHcu = Guid.NewGuid(), _rosterIgd = Guid.NewGuid();

    public TemporaryPlacementTests()
    {
        using var ctx = _db.CreateContext();
        foreach (var (id, nama) in new[] { (_icu, "ICU"), (_hcu, "HCU"), (_igd, "IGD") })
            ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = id, LegalEntityId = Guid.NewGuid(), UnitCode = nama, UnitName = nama });
        ctx.MstShifts.Add(new MstShift { Id = _pagi, ShiftCode = "P", ShiftName = "Pagi", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(14, 0), IsActive = true });
        ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = _tono, ProfileCode = "TONO", DisplayName = "Tono", UserType = UserType.Employee });
        ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = _sari, ProfileCode = "SARI", DisplayName = "Sari", UserType = UserType.Employee });

        // Sari Kepala Unit ICU (berjabatan, berlaku hari ini). IGD tidak punya Kepala Unit.
        ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
        {
            WorkforceProfileId = _sari, OrganizationUnitId = _icu, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), AssignmentType = "Primary",
            IsManagerialAssignment = true, EffectiveStartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true,
        });
        ctx.Users.Add(new ApplicationUser { Id = _akunSari, UserName = "sari", UserCode = "SARI0001", DisplayName = "Sari", UserType = UserType.Employee, WorkforceProfileId = _sari, IsActive = true });
        ctx.Users.Add(new ApplicationUser { Id = _akunTono, UserName = "tono", UserCode = "TONO0001", DisplayName = "Tono", UserType = UserType.Employee, WorkforceProfileId = _tono, IsActive = true });

        void Roster(Guid id, Guid unit, string status, params DateOnly[] tanggal)
        {
            ctx.TrxRosterPeriods.Add(new TrxRosterPeriod
            {
                Id = id, OrganizationUnitId = unit, RosterPeriodCode = id.ToString("N")[..8], RosterPeriodName = $"Roster {status}", RosterStatus = status,
                IsPublished = status == "Published", VersionNumber = 1, PeriodStartDate = new DateOnly(2027, 3, 1), PeriodEndDate = new DateOnly(2027, 3, 31), IsActive = true,
            });
            var ra = Guid.NewGuid();
            ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = ra, RosterPeriodId = id, WorkforceProfileId = _tono, IsActive = true });
            foreach (var t in tanggal)
            {
                ctx.TrxShiftAssignments.Add(new TrxShiftAssignment
                {
                    RosterAssignmentId = ra, WorkforceProfileId = _tono, ShiftId = _pagi, ShiftDate = t,
                    ScheduledStartAt = t.ToDateTime(new TimeOnly(0, 0), DateTimeKind.Utc), ScheduledEndAt = t.ToDateTime(new TimeOnly(7, 0), DateTimeKind.Utc),
                    AssignmentStatus = status, IsActive = true,
                });
            }
        }

        // Roster ICU terbit: 5 Mar (di dalam rentang) dan 20 Mar (di luar). Roster ICU draf 6 Mar dan roster HCU
        // (unit tujuan) 7 Mar tidak dihitung. Roster IGD terbit 8 Mar: unit tanpa Kepala Unit.
        Roster(_rosterIcu, _icu, "Published", new DateOnly(2027, 3, 5), new DateOnly(2027, 3, 20));
        Roster(_rosterIcuDraf, _icu, "Draft", new DateOnly(2027, 3, 6));
        Roster(_rosterHcu, _hcu, "Published", new DateOnly(2027, 3, 7));
        Roster(_rosterIgd, _igd, "Published", new DateOnly(2027, 3, 8));
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private Guid Penempatan(string jenis = "Temporary", Guid? unit = null, bool aktif = true)
    {
        using var ctx = _db.CreateContext();
        var p = new WfpOrganizationAssignment
        {
            WorkforceProfileId = _tono, OrganizationUnitId = unit ?? _hcu, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), AssignmentType = jenis,
            EffectiveStartDate = new DateTime(2027, 3, 1, 0, 0, 0, DateTimeKind.Utc), EffectiveEndDate = new DateTime(2027, 3, 14, 0, 0, 0, DateTimeKind.Utc), IsActive = aktif,
        };
        ctx.WfpOrganizationAssignments.Add(p);
        ctx.SaveChanges();
        return p.Id;
    }

    private (TemporaryPlacementRosterImpactService Layanan, PemberitahuanPenampung Notif) Layanan(bool gagal = false)
    {
        var notif = new PemberitahuanPenampung(gagal);
        return (new TemporaryPlacementRosterImpactService(_db.CreateContext(), notif, NullLogger<TemporaryPlacementRosterImpactService>.Instance), notif);
    }

    private string SidikShift()
    {
        using var ctx = _db.CreateContext();
        return string.Join("|", ctx.TrxShiftAssignments.AsNoTracking().OrderBy(s => s.Id)
            .Select(s => $"{s.Id}:{s.WorkforceProfileId}:{s.ShiftDate}:{s.IsActive}:{s.IsCancel}:{s.AssignmentStatus}:{s.UpdateDateTime}"));
    }

    // AT-HRD-PK-45
    [Fact]
    public async Task Penempatan_sementara_memberi_tahu_Kepala_Unit_asal_tanpa_mengubah_roster()
    {
        var sidik = SidikShift();
        var (svc, notif) = Layanan();
        var r = await svc.NotifyOriginUnitsAsync(Penempatan());

        Assert.False(r.Failed);
        Assert.Equal(2, r.AffectedShiftCount);          // ICU 5 Mar + IGD 8 Mar; bukan draf, bukan HCU, bukan 20 Mar
        Assert.Equal(1, r.UnitsWithoutRecipient);      // IGD tanpa Kepala Unit
        var kirim = Assert.Single(notif.Terkirim);
        Assert.Equal(_akunSari, kirim.UserId);
        Assert.Contains("Tono ditempatkan di HCU 1–14 Maret 2027", kirim.Pesan);
        Assert.Contains("5 Mar", kirim.Pesan);
        Assert.DoesNotContain("20 Mar", kirim.Pesan);
        Assert.Contains("Ganti Petugas", kirim.Pesan);
        Assert.Equal(sidik, SidikShift());
    }

    [Fact]
    public async Task Pemberitahuan_gagal_tidak_melempar_dan_roster_tetap()
    {
        var sidik = SidikShift();
        var (svc, _) = Layanan(gagal: true);
        var r = await svc.NotifyOriginUnitsAsync(Penempatan());
        Assert.Equal(1, r.FailedNotificationCount);
        Assert.Empty(r.NotifiedUserIds);
        Assert.Equal(sidik, SidikShift());
    }

    [Fact]
    public async Task Galat_basis_data_tidak_melempar()
    {
        var id = Penempatan();
        var ctx = _db.CreateContext();
        ctx.Dispose();
        var svc = new TemporaryPlacementRosterImpactService(ctx, new PemberitahuanPenampung(), NullLogger<TemporaryPlacementRosterImpactService>.Instance);
        var r = await svc.NotifyOriginUnitsAsync(id);
        Assert.True(r.Failed);
    }

    [Theory]
    [InlineData("Secondary", true)]
    [InlineData("Temporary", false)]
    public async Task Bukan_penempatan_sementara_aktif_tidak_memberi_tahu(string jenis, bool aktif)
    {
        var (svc, notif) = Layanan();
        var r = await svc.NotifyOriginUnitsAsync(Penempatan(jenis, aktif: aktif));
        Assert.Equal(0, r.AffectedShiftCount);
        Assert.Empty(notif.Terkirim);
    }

    [Fact]
    public async Task Penempatan_sementara_ke_unit_roster_sendiri_tidak_memberi_tahu()
    {
        // Sementara di ICU: roster ICU adalah unit tujuan, bukan unit asal, sehingga Sari tidak diberi tahu. Roster HCU
        // dan IGD (unit lain, keduanya tanpa Kepala Unit) tetap terhitung terdampak.
        var (svc, notif) = Layanan();
        var r = await svc.NotifyOriginUnitsAsync(Penempatan(unit: _icu));
        Assert.Empty(notif.Terkirim);
        Assert.Equal(2, r.AffectedShiftCount);
        Assert.Equal(2, r.UnitsWithoutRecipient);
    }
}

public sealed class PemberitahuanPenampung(bool gagal = false) : QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services.IUnifiedNotificationGatewayService
{
    public List<(Guid UserId, string Judul, string Pesan, string? Dedup)> Terkirim { get; } = new();

    public Task SendInstantNotificationAsync(Guid recipientEmployeeId, string title, string message, string channel = "WebPush") =>
        throw new NotSupportedException();

    public Task SendInstantNotificationToUserAsync(Guid recipientUserId, string title, string message, string? deepLinkUrl = null, Guid? workflowInstanceId = null, string priority = "Normal", string? dedupKey = null)
    {
        if (gagal) throw new InvalidOperationException("Pusat pemberitahuan mati.");
        Terkirim.Add((recipientUserId, title, message, dedupKey));
        return Task.CompletedTask;
    }

    public Task<int> SendApprovalDigestAsync() => throw new NotSupportedException();
}
