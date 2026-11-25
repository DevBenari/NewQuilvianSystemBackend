using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.HumanResource;

public sealed class KesiapanAtur(Guid[] gagal, Guid[] didampingi) : IHrdEmploymentReadinessReadService
{
    public Task<RecruitmentResult<SchedulingEligibilityResponse>> GetSchedulingEligibilityAsync(Guid id, DateOnly date, CancellationToken ct = default) =>
        Task.FromResult(gagal.Contains(id)
            ? RecruitmentResult<SchedulingEligibilityResponse>.Fail("tidak terbaca", 503)
            : RecruitmentResult<SchedulingEligibilityResponse>.Ok(new SchedulingEligibilityResponse { WorkforceProfileId = id, TargetDate = date, IsAccompanied = didampingi.Contains(id) }));
}

public sealed class PemberitahuanTiruan(bool gagal = false) : IUnifiedNotificationGatewayService
{
    public List<(Guid UserId, string Title)> Terkirim { get; } = new();

    public Task SendInstantNotificationAsync(Guid recipientEmployeeId, string title, string message, string channel = "WebPush") =>
        throw new NotSupportedException("Ganti Petugas wajib memakai SendInstantNotificationToUserAsync (INT-HR-105).");

    public Task SendInstantNotificationToUserAsync(Guid recipientUserId, string title, string message, string? deepLinkUrl = null, Guid? workflowInstanceId = null, string priority = "Normal", string? dedupKey = null)
    {
        if (gagal) throw new InvalidOperationException("Pusat pemberitahuan mati.");
        Terkirim.Add((recipientUserId, title));
        return Task.CompletedTask;
    }

    public Task<int> SendApprovalDigestAsync() => throw new NotSupportedException();
}

// Penyela: tepat sebelum pembaruan bersyarat dijalankan, permintaan lain lebih dulu mengganti petugas.
public sealed class PenyelaSerentak(Guid shiftId, Guid petugasLain) : DbCommandInterceptor
{
    public bool Terjadi { get; private set; }

    public override async ValueTask<InterceptionResult<int>> NonQueryExecutingAsync(DbCommand command, CommandEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        if (!Terjadi && command.CommandText.Contains("UPDATE \"TrxShiftAssignment\"", StringComparison.Ordinal))
        {
            Terjadi = true;
            await using var lain = command.Connection!.CreateCommand();
            lain.Transaction = command.Transaction;
            lain.CommandText = "UPDATE \"TrxShiftAssignment\" SET \"WorkforceProfileId\" = $baru WHERE \"Id\" = $id";
            var p1 = lain.CreateParameter(); p1.ParameterName = "$baru"; p1.Value = petugasLain.ToString().ToUpperInvariant(); lain.Parameters.Add(p1);
            var p2 = lain.CreateParameter(); p2.ParameterName = "$id"; p2.Value = shiftId.ToString().ToUpperInvariant(); lain.Parameters.Add(p2);
            Assert.Equal(1, await lain.ExecuteNonQueryAsync(ct));
        }

        return await base.NonQueryExecutingAsync(command, eventData, result, ct);
    }
}

// Memaksa penyimpanan riwayat gagal untuk membuktikan seluruh Ganti Petugas ikut batal (INV-A52-01).
public sealed class SimpanGagal : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        if (eventData.Context!.ChangeTracker.Entries<HrdShiftAssignmentRevision>().Any(e => e.State == EntityState.Added))
            throw new DbUpdateException("Riwayat gagal ditulis (simulasi).");
        return base.SavingChangesAsync(eventData, result, ct);
    }
}

// BE-HRD-338 — AT-HRD-PK-01..04, contoh 35.9.3 (Budi → Tono → Andi).
public sealed class ReplacementTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _icu = Guid.NewGuid(), _malam = Guid.NewGuid(), _periode = Guid.NewGuid(), _shiftBudi = Guid.NewGuid();
    private readonly Guid _budi = Guid.NewGuid(), _tono = Guid.NewGuid(), _andi = Guid.NewGuid(), _dina = Guid.NewGuid(),
        _eka = Guid.NewGuid(), _fitri = Guid.NewGuid();
    private readonly Guid _sari = Guid.NewGuid(), _akunBudi = Guid.NewGuid(), _akunTono = Guid.NewGuid();

    public ReplacementTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _icu, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        ctx.MstShifts.Add(new MstShift { Id = _malam, ShiftCode = "M", ShiftName = "Malam", StartTime = new TimeOnly(21, 0), EndTime = new TimeOnly(7, 0), IsNightShift = true });
        foreach (var (id, nama) in new[] { (_budi, "Budi"), (_tono, "Tono"), (_andi, "Andi"), (_dina, "Dina"), (_eka, "Eka"), (_fitri, "Fitri") })
        {
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = nama, DisplayName = nama, UserType = UserType.Employee });
            ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
            {
                WorkforceProfileId = id, OrganizationUnitId = _icu, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(),
                AssignmentType = "Primary", EffectiveStartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true,
            });
        }

        ApplicationUser Akun(Guid id, Guid? profil, UserType tipe) => new()
        { Id = id, UserName = id.ToString(), UserCode = id.ToString("N")[..8], DisplayName = "Uji", UserType = tipe, WorkforceProfileId = profil, IsActive = true };
        ctx.Users.AddRange(Akun(_sari, null, UserType.SuperAdmin), Akun(_akunBudi, _budi, UserType.Employee), Akun(_akunTono, _tono, UserType.Employee));

        // Eka punya cuti yang masih menunggu (W4).
        ctx.WfpLeaveRequests.Add(new WfpLeaveRequest
        {
            WorkforceProfileId = _eka, StartDate = new DateOnly(2027, 2, 14), EndDate = new DateOnly(2027, 2, 14), LeaveRequestStatus = "Submitted",
        });

        ctx.TrxRosterPeriods.Add(new TrxRosterPeriod
        {
            Id = _periode, OrganizationUnitId = _icu, RosterPeriodCode = "R-ICU-FEB", RosterPeriodName = "ICU Feb 2027", RosterStatus = "Published",
            IsPublished = true, VersionNumber = 3, PeriodStartDate = new DateOnly(2027, 2, 1), PeriodEndDate = new DateOnly(2027, 2, 28), IsActive = true,
        });
        var ra = Guid.NewGuid();
        ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = ra, RosterPeriodId = _periode, WorkforceProfileId = _budi });
        ctx.TrxShiftAssignments.Add(new TrxShiftAssignment
        {
            Id = _shiftBudi, RosterAssignmentId = ra, WorkforceProfileId = _budi, ShiftId = _malam, ShiftDate = new DateOnly(2027, 2, 14),
            ScheduledStartAt = new DateTime(2027, 2, 14, 14, 0, 0, DateTimeKind.Utc), ScheduledEndAt = new DateTime(2027, 2, 15, 0, 0, 0, DateTimeKind.Utc),
            IsNightShift = true, AssignmentStatus = "Published", IsActive = true,
        });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private (ShiftReplacementService Service, PemberitahuanTiruan Notif) Layanan(
        IHrdEmploymentReadinessReadService? kesiapan = null, bool notifGagal = false, params IInterceptor[] interceptors)
    {
        var ctx = _db.CreateContext(interceptors);
        var notif = new PemberitahuanTiruan(notifGagal);
        kesiapan ??= new KesiapanAtur(new[] { _fitri }, new[] { _dina });
        var scope = new HrdUnitScopeService(ctx, Pelaku.Izin(ctx), Pelaku.Sebagai(_sari));
        var service = new ShiftReplacementService(ctx, kesiapan, NullLogger<ShiftReplacementService>.Instance, Pengganti.Buat<IServiceProvider>(), scope,
            new RosterValidationService(ctx, kesiapan, new SetelanBawaan(), NullLogger<RosterValidationService>.Instance),
            new ShiftRevisionRecorder(ctx), notif, new SetelanBawaan());
        return (service, notif);
    }

    private static DirectShiftReplacementRequest Req(Guid pengganti, Guid? harap, string kategori = "Sakit Mendadak", string catatan = "Budi menelepon sakit") =>
        new() { ReplacementEmployeeId = pengganti, ExpectedWorkforceProfileId = harap, ReasonCategory = kategori, Notes = catatan };

    private List<HrdShiftAssignmentRevision> Riwayat()
    {
        using var ctx = _db.CreateContext();
        return ctx.HrdShiftAssignmentRevisions.AsNoTracking().OrderBy(x => x.ChangedAt).ToList();
    }

    private TrxShiftAssignment Shift()
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxShiftAssignments.AsNoTracking().Single(s => s.Id == _shiftBudi);
    }

    // AT-HRD-PK-01 + contoh 35.9.3
    [Fact]
    public async Task Budi_Tono_Andi_menulis_dua_baris_riwayat_dengan_petugas_sebelum_yang_benar()
    {
        var (svc1, notif1) = Layanan();
        var r1 = await svc1.ExecuteDirectReplacementAsync(_shiftBudi, Req(_tono, _budi), _sari);
        Assert.True(r1.Success, r1.Message);
        Assert.Equal(_budi, r1.Data!.OriginalEmployeeId);

        var (svc2, _) = Layanan();
        var r2 = await svc2.ExecuteDirectReplacementAsync(_shiftBudi, Req(_andi, _tono, "Lainnya", "Tono dipanggil siaga IGD"), _sari);
        Assert.True(r2.Success, r2.Message);
        Assert.Equal(_tono, r2.Data!.OriginalEmployeeId);

        var riwayat = Riwayat();
        Assert.Equal(2, riwayat.Count);
        Assert.All(riwayat, x => Assert.Equal("Replacement", x.ChangeType));
        Assert.Equal((_budi, _tono), (riwayat[0].PreviousWorkforceProfileId!.Value, riwayat[0].NewWorkforceProfileId!.Value));
        Assert.Equal((_tono, _andi), (riwayat[1].PreviousWorkforceProfileId!.Value, riwayat[1].NewWorkforceProfileId!.Value));
        Assert.Equal("Lainnya", riwayat[1].ReasonCategory);
        Assert.Equal(_sari, riwayat[1].ChangedByUserId);
        Assert.Equal(r2.Data.RevisionId, riwayat[1].Id);

        var shift = Shift();
        Assert.Equal(_andi, shift.WorkforceProfileId);
        Assert.Equal(_budi, shift.OriginalEmployeeId); // petugas pertama tetap tersimpan

        using var ctx = _db.CreateContext();
        var periode = ctx.TrxRosterPeriods.AsNoTracking().Single(p => p.Id == _periode);
        Assert.Equal("Published", periode.RosterStatus);
        Assert.Equal(3, periode.VersionNumber);
        var jejak = ctx.TrxShiftReplacements.AsNoTracking().OrderBy(x => x.CreateDateTime).Select(x => x.OriginalWorkforceProfileId).ToList();
        Assert.Equal(new[] { _budi, _tono }, jejak);

        // Pemberitahuan dikirim ke akun Budi dan Tono (Andi belum punya akun).
        Assert.Equal(new[] { _akunBudi, _akunTono }, notif1.Terkirim.Select(x => x.UserId));
    }

    // AT-HRD-PK-02
    [Fact]
    public async Task Pengganti_Didampingi_tanpa_perawat_Mandiri_ditolak_422_B7_tanpa_perubahan()
    {
        var (svc, _) = Layanan();
        var r = await svc.ExecuteDirectReplacementAsync(_shiftBudi, Req(_dina, _budi), _sari);
        Assert.Equal(422, r.StatusCode);
        Assert.Contains("B7", r.Message);
        var errors = Assert.IsAssignableFrom<IEnumerable<RosterValidationConflictItemDto>>(r.Errors);
        Assert.Contains(errors, x => x.RuleCode == "B7");
        Assert.Equal(_budi, Shift().WorkforceProfileId);
        Assert.Empty(Riwayat());
    }

    // AT-HRD-PK-03
    [Fact]
    public async Task Kesiapan_tidak_terbaca_ditolak_422_B6_bukan_dilewati()
    {
        var (svc, _) = Layanan();
        var r = await svc.ExecuteDirectReplacementAsync(_shiftBudi, Req(_fitri, _budi), _sari);
        Assert.Equal(422, r.StatusCode);
        Assert.Equal("Status kesiapan pegawai tidak dapat dibaca. Coba lagi atau hubungi HRD.", r.Message);
        Assert.Contains((IEnumerable<RosterValidationConflictItemDto>)r.Errors!, x => x.RuleCode == "B6");
        Assert.Equal(_budi, Shift().WorkforceProfileId);
        Assert.Empty(Riwayat());
    }

    // AT-HRD-PK-04 — petugas sudah berubah sebelum layar disimpan
    [Fact]
    public async Task Petugas_yang_diharapkan_berbeda_ditolak_409()
    {
        var (svc1, _) = Layanan();
        Assert.True((await svc1.ExecuteDirectReplacementAsync(_shiftBudi, Req(_tono, _budi), _sari)).Success);

        var (svc2, _) = Layanan();
        var kedua = await svc2.ExecuteDirectReplacementAsync(_shiftBudi, Req(_andi, _budi), _sari);
        Assert.Equal(409, kedua.StatusCode);
        Assert.Equal("Shift ini baru saja diubah orang lain. Muat ulang lalu coba lagi.", kedua.Message);
        Assert.Single(Riwayat());
        Assert.Equal(_tono, Shift().WorkforceProfileId);
    }

    // AT-HRD-PK-04 — dua permintaan serentak: yang kalah ditahan pembaruan bersyarat di basis data
    [Fact]
    public async Task Ganti_Petugas_serentak_yang_kalah_409_dan_tidak_menulis_riwayat()
    {
        var penyela = new PenyelaSerentak(_shiftBudi, _andi);
        var (svc, notif) = Layanan(interceptors: penyela);
        var r = await svc.ExecuteDirectReplacementAsync(_shiftBudi, Req(_tono, _budi), _sari);

        Assert.True(penyela.Terjadi);
        Assert.Equal(409, r.StatusCode);
        Assert.Empty(Riwayat());
        Assert.Empty(notif.Terkirim);
    }

    [Fact]
    public async Task Riwayat_gagal_ditulis_membatalkan_seluruh_Ganti_Petugas()
    {
        var (svc, notif) = Layanan(interceptors: new SimpanGagal());
        await Assert.ThrowsAsync<DbUpdateException>(() => svc.ExecuteDirectReplacementAsync(_shiftBudi, Req(_tono, _budi), _sari));

        Assert.Equal(_budi, Shift().WorkforceProfileId); // pembaruan bersyarat ikut dibatalkan
        Assert.Empty(Riwayat());
        Assert.Empty(notif.Terkirim);
    }

    [Fact]
    public async Task Peringatan_tanpa_alasan_ditolak_422_dengan_alasan_berhasil()
    {
        var (svc1, _) = Layanan();
        var tanpa = await svc1.ExecuteDirectReplacementAsync(_shiftBudi, Req(_eka, _budi), _sari);
        Assert.Equal(422, tanpa.StatusCode);
        Assert.Equal("Pilih alasan untuk setiap peringatan sebelum menyimpan.", tanpa.Message);
        Assert.Contains((IEnumerable<RosterValidationConflictItemDto>)tanpa.Errors!, x => x.RuleCode == "W4");
        Assert.Empty(Riwayat());

        var (svc2, _) = Layanan();
        var req = Req(_eka, _budi);
        req.WarningReasons = new List<WarningReasonItemRequest> { new() { WarningCode = "W4", ReasonCategory = "Kekurangan tenaga" } };
        var dengan = await svc2.ExecuteDirectReplacementAsync(_shiftBudi, req, _sari);
        Assert.True(dengan.Success, dengan.Message);
        Assert.Contains(dengan.Data!.Warnings, x => x.RuleCode == "W4");
        Assert.Contains("Kuota unit belum ditetapkan", dengan.Data.InfoMessages);
    }

    [Fact]
    public async Task Pemberitahuan_gagal_tidak_membatalkan_Ganti_Petugas()
    {
        var (svc, _) = Layanan(notifGagal: true);
        var r = await svc.ExecuteDirectReplacementAsync(_shiftBudi, Req(_tono, _budi), _sari);
        Assert.True(r.Success, r.Message);
        Assert.Contains(r.Data!.NotificationsDispatched, x => x.Contains("gagal dikirim"));
        Assert.Single(Riwayat());
        Assert.Equal(_tono, Shift().WorkforceProfileId);
    }

    [Theory]
    [InlineData("  ", "Budi sakit")]
    [InlineData("Sakit Mendadak", "   ")]
    public async Task Alasan_kosong_ditolak_400(string kategori, string catatan)
    {
        var (svc, _) = Layanan();
        var r = await svc.ExecuteDirectReplacementAsync(_shiftBudi, Req(_tono, _budi, kategori, catatan), _sari);
        Assert.Equal(400, r.StatusCode);
        Assert.Equal("Alasan perubahan wajib diisi.", r.Message);
    }

    [Fact]
    public async Task Tanpa_ExpectedWorkforceProfileId_tetap_berhasil_selama_FE_lama()
    {
        var (svc, _) = Layanan();
        var r = await svc.ExecuteDirectReplacementAsync(_shiftBudi, Req(_tono, null), _sari);
        Assert.True(r.Success, r.Message);
        Assert.Single(Riwayat());
    }
}
