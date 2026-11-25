using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Tests.HumanResource;

public sealed class SetelanJendela(int hari) : ISchedulingSettingService
{
    public Task<RecruitmentResult<SchedulingSettingResponse>> GetSettingAsync(Guid hospitalSiteId, CancellationToken ct = default) =>
        Task.FromResult(RecruitmentResult<SchedulingSettingResponse>.Ok(new SchedulingSettingResponse { RetroactiveCorrectionWindowDays = hari }));

    public Task<RecruitmentResult<SchedulingSettingResponse>> UpdateSettingAsync(Guid hospitalSiteId, UpdateSchedulingSettingRequest request, ClaimsPrincipal user, CancellationToken ct = default) =>
        throw new NotSupportedException();
}

// Penyedia layanan tanpa layanan hitung ulang kehadiran (GetService memulangkan null).
public sealed class PenyediaKosong : IServiceProvider
{
    public object? GetService(Type serviceType) => null;
}

// BE-HRD-339 — AT-HRD-PK-07, HRD-DEC-217 (contoh: Andi Malam 21.00–07.00 → Siang 14.00–21.00, "Salah catat").
public sealed class RetroactiveTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _icu = Guid.NewGuid(), _malam = Guid.NewGuid(), _siang = Guid.NewGuid(), _periode = Guid.NewGuid(),
        _shiftAndi = Guid.NewGuid(), _andi = Guid.NewGuid(), _lain = Guid.NewGuid(), _sari = Guid.NewGuid(), _rekap = Guid.NewGuid();
    private readonly DateOnly _tanggal;

    // Jam dinding WIB → UTC (WIB = UTC+7), dihitung manual sebagai pembanding independen.
    private static DateTime Wib(DateOnly d, int jam) => new DateTime(d.Year, d.Month, d.Day, jam, 0, 0, DateTimeKind.Utc).AddHours(-7);

    // SQLite memulangkan DateTime tanpa Kind; nilai yang tersimpan sudah UTC.
    private static DateTime U(DateTime x) => DateTime.SpecifyKind(x, DateTimeKind.Utc);

    public RetroactiveTests()
    {
        var hariIniWib = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
        _tanggal = hariIniWib.AddDays(-2);

        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _icu, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        ctx.MstShifts.Add(new MstShift { Id = _malam, ShiftCode = "M", ShiftName = "Malam", StartTime = new TimeOnly(21, 0), EndTime = new TimeOnly(7, 0), IsNightShift = true, IsOvernight = true, PaidWorkMinutes = 600, IsActive = true });
        ctx.MstShifts.Add(new MstShift { Id = _siang, ShiftCode = "S", ShiftName = "Siang", StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(21, 0), PaidWorkMinutes = 420, IsActive = true });
        foreach (var (id, nama) in new[] { (_andi, "Andi"), (_lain, "Lain") })
        {
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = nama, DisplayName = nama, UserType = UserType.Employee });
            ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
            {
                WorkforceProfileId = id, OrganizationUnitId = _icu, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(),
                AssignmentType = "Primary", EffectiveStartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true,
            });
        }

        ctx.Users.Add(new ApplicationUser { Id = _sari, UserName = "sari", UserCode = "SARI0001", DisplayName = "Sari", UserType = UserType.SuperAdmin, IsActive = true });
        ctx.TrxRosterPeriods.Add(new TrxRosterPeriod
        {
            Id = _periode, OrganizationUnitId = _icu, RosterPeriodCode = "R-ICU", RosterPeriodName = "ICU", RosterStatus = "Published",
            IsPublished = true, VersionNumber = 3, PeriodStartDate = _tanggal.AddDays(-10), PeriodEndDate = _tanggal.AddDays(10), IsActive = true,
        });
        var ra = Guid.NewGuid();
        ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = ra, RosterPeriodId = _periode, WorkforceProfileId = _andi });
        ctx.TrxShiftAssignments.Add(new TrxShiftAssignment
        {
            Id = _shiftAndi, RosterAssignmentId = ra, WorkforceProfileId = _andi, ShiftId = _malam, ShiftDate = _tanggal,
            ScheduledStartAt = Wib(_tanggal, 21), ScheduledEndAt = Wib(_tanggal.AddDays(1), 7), PlannedWorkMinutes = 600,
            IsNightShift = true, AssignmentStatus = "Published", IsActive = true,
        });
        ctx.HrdAttendanceDailies.Add(new HrdAttendanceDaily
        {
            Id = _rekap, UserId = Guid.NewGuid(), WorkforceProfileId = _andi, AttendanceDate = _tanggal, ScheduleSource = "Roster", ShiftId = _malam,
            ProcessingStatus = AttendanceValueConstants.AttendanceProcessingStatus.Processed, ProcessingMessage = "Pesan lama",
        });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private ShiftReplacementService Layanan(IServiceProvider? penyedia = null, int jendela = 3, params IInterceptor[] interceptors)
    {
        var ctx = _db.CreateContext(interceptors);
        var kesiapan = new KesiapanTiruan();
        var scope = new QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services.HrdUnitScopeService(ctx, Pelaku.Izin(ctx), Pelaku.Sebagai(_sari));
        return new ShiftReplacementService(ctx, kesiapan, NullLogger<ShiftReplacementService>.Instance, penyedia ?? new PenyediaKosong(), scope,
            new RosterValidationService(ctx, kesiapan, new SetelanBawaan(), NullLogger<RosterValidationService>.Instance),
            new ShiftRevisionRecorder(ctx), new PemberitahuanTiruan(), new SetelanJendela(jendela));
    }

    private static RetroactiveShiftCorrectionRequest Req(Guid shift, string kategori = "Salah catat", string catatan = "Tertukar dengan jadwal Siang") =>
        new() { NewShiftId = shift, ReasonCategory = kategori, Notes = catatan };

    private List<HrdShiftAssignmentRevision> Riwayat()
    {
        using var ctx = _db.CreateContext();
        return ctx.HrdShiftAssignmentRevisions.AsNoTracking().ToList();
    }

    private TrxShiftAssignment Shift()
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxShiftAssignments.AsNoTracking().Single(s => s.Id == _shiftAndi);
    }

    private HrdAttendanceDaily Rekap()
    {
        using var ctx = _db.CreateContext();
        return ctx.HrdAttendanceDailies.AsNoTracking().Single(d => d.Id == _rekap);
    }

    // AT-HRD-PK-07 + HRD-DEC-217 butir 1–2
    [Fact]
    public async Task Koreksi_mundur_menulis_satu_riwayat_dengan_shift_dan_jam_lama_serta_baru()
    {
        var r = await Layanan().RetroactiveCorrectionAsync(_shiftAndi, Req(_siang), _sari);
        Assert.True(r.Success, r.Message);

        var riwayat = Assert.Single(Riwayat());
        Assert.Equal(r.Data!.RevisionId, riwayat.Id);
        Assert.Equal("RetroactiveCorrection", riwayat.ChangeType);
        Assert.Equal(_andi, riwayat.PreviousWorkforceProfileId);
        Assert.Equal(_andi, riwayat.NewWorkforceProfileId);
        Assert.Equal(_malam, riwayat.PreviousShiftId);
        Assert.Equal(_siang, riwayat.NewShiftId);
        Assert.Equal(Wib(_tanggal, 21), U(riwayat.PreviousStartAt!.Value));
        Assert.Equal(Wib(_tanggal.AddDays(1), 7), U(riwayat.PreviousEndAt!.Value));
        Assert.Equal(Wib(_tanggal, 14), U(riwayat.NewStartAt!.Value));
        Assert.Equal(Wib(_tanggal, 21), U(riwayat.NewEndAt!.Value));
        Assert.Equal("Salah catat", riwayat.ReasonCategory);
        Assert.Equal("Tertukar dengan jadwal Siang", riwayat.ReasonNote);
        Assert.Equal(_sari, riwayat.ChangedByUserId);
        Assert.Equal(_tanggal, riwayat.ShiftDate);
        Assert.Null(riwayat.ShiftReplacementId);

        var shift = Shift();
        Assert.Equal(_siang, shift.ShiftId);
        Assert.Equal(Wib(_tanggal, 14), U(shift.ScheduledStartAt));
        Assert.Equal(Wib(_tanggal, 21), U(shift.ScheduledEndAt));
        Assert.Equal(420, shift.PlannedWorkMinutes);
        Assert.False(shift.IsNightShift);
        Assert.Equal("RetroactiveCorrection", shift.AssignmentSource);

        using var ctx = _db.CreateContext();
        var periode = ctx.TrxRosterPeriods.AsNoTracking().Single(p => p.Id == _periode);
        Assert.Equal("Published", periode.RosterStatus);
        Assert.Equal(3, periode.VersionNumber);
    }

    // Kriteria: kegagalan hitung ulang menandai ReprocessRequired; koreksi tetap tersimpan.
    [Fact]
    public async Task Hitung_ulang_tidak_tersedia_menandai_rekap_ReprocessRequired()
    {
        var r = await Layanan(new PenyediaKosong()).RetroactiveCorrectionAsync(_shiftAndi, Req(_siang), _sari);
        Assert.True(r.Success, r.Message);
        Assert.False(r.Data!.AttendanceReevaluationTriggered);
        Assert.Contains("ditandai perlu diproses ulang", r.Message);
        Assert.Equal(AttendanceValueConstants.AttendanceProcessingStatus.ReprocessRequired, Rekap().ProcessingStatus);
        Assert.StartsWith("Jadwal dikoreksi mundur", Rekap().ProcessingMessage);
        Assert.EndsWith("Pesan lama", Rekap().ProcessingMessage);
        Assert.Single(Riwayat());
    }

    [Fact]
    public async Task Hitung_ulang_melempar_galat_menandai_rekap_ReprocessRequired()
    {
        // Pengganti melempar galat pada setiap pemanggilan GetService.
        var r = await Layanan(Pengganti.Buat<IServiceProvider>()).RetroactiveCorrectionAsync(_shiftAndi, Req(_siang), _sari);
        Assert.True(r.Success, r.Message);
        Assert.False(r.Data!.AttendanceReevaluationTriggered);
        Assert.Equal(AttendanceValueConstants.AttendanceProcessingStatus.ReprocessRequired, Rekap().ProcessingStatus);
        Assert.Equal(_siang, Shift().ShiftId);
    }

    // HRD-DEC-217 butir 4 / uji keputusan (c): batas hari tetap berlaku, dibaca dari setelan.
    [Fact]
    public async Task Lewat_batas_hari_ditolak_422_tanpa_perubahan()
    {
        var r = await Layanan(jendela: 1).RetroactiveCorrectionAsync(_shiftAndi, Req(_siang), _sari);
        Assert.Equal(422, r.StatusCode);
        Assert.Contains("maksimal 1 hari", r.Message);
        Assert.Empty(Riwayat());
        Assert.Equal(_malam, Shift().ShiftId);

        // Kontrol: setelan 2 hari meloloskan shift 2 hari lalu.
        var ok = await Layanan(jendela: 2).RetroactiveCorrectionAsync(_shiftAndi, Req(_siang), _sari);
        Assert.True(ok.Success, ok.Message);
    }

    // VR-PK-07
    [Theory]
    [InlineData("", "catatan")]
    [InlineData("Salah catat", "   ")]
    public async Task Alasan_kosong_ditolak_400(string kategori, string catatan)
    {
        var r = await Layanan().RetroactiveCorrectionAsync(_shiftAndi, Req(_siang, kategori, catatan), _sari);
        Assert.Equal(400, r.StatusCode);
        Assert.Equal("Alasan perubahan wajib diisi.", r.Message);
        Assert.Empty(Riwayat());
    }

    [Fact]
    public async Task Kategori_lebih_dari_50_karakter_ditolak_400()
    {
        var r = await Layanan().RetroactiveCorrectionAsync(_shiftAndi, Req(_siang, new string('x', 51)), _sari);
        Assert.Equal(400, r.StatusCode);
        Assert.Empty(Riwayat());
    }

    // Perubahan lain di antara baca dan simpan: 409, tanpa riwayat.
    [Fact]
    public async Task Shift_berubah_di_antara_baca_dan_simpan_ditolak_409()
    {
        var penyela = new PenyelaSerentak(_shiftAndi, _lain);
        var r = await Layanan(null, 3, penyela).RetroactiveCorrectionAsync(_shiftAndi, Req(_siang), _sari);
        Assert.True(penyela.Terjadi);
        Assert.Equal(409, r.StatusCode);
        Assert.Empty(Riwayat());
        Assert.Equal(_malam, Shift().ShiftId);
    }

    // INV-A52-01: riwayat gagal ditulis → koreksi ikut batal.
    [Fact]
    public async Task Riwayat_gagal_ditulis_membatalkan_koreksi()
    {
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            Layanan(null, 3, new SimpanGagal()).RetroactiveCorrectionAsync(_shiftAndi, Req(_siang), _sari));
        Assert.Empty(Riwayat());
        Assert.Equal(_malam, Shift().ShiftId);
        Assert.Equal(Wib(_tanggal, 21), U(Shift().ScheduledStartAt));
    }

    [Fact]
    public async Task Periode_roster_ditutup_ditolak_403()
    {
        using (var ctx = _db.CreateContext())
        {
            var p = ctx.TrxRosterPeriods.Single(x => x.Id == _periode);
            p.RosterStatus = "Closed";
            ctx.SaveChanges();
        }

        var r = await Layanan().RetroactiveCorrectionAsync(_shiftAndi, Req(_siang), _sari);
        Assert.Equal(403, r.StatusCode);
        Assert.Empty(Riwayat());
    }
}
