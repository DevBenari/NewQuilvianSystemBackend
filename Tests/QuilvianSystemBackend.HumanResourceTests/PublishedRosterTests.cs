using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Controllers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-340 — AT-HRD-PK-05, -06, -08 (HRD-DEC-195): roster terbit tetap Published saat ditambah/dihapus shift.
public sealed class PublishedRosterTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _icu = Guid.NewGuid(), _pagi = Guid.NewGuid(), _malam = Guid.NewGuid(), _periode = Guid.NewGuid(),
        _draf = Guid.NewGuid(), _raBudi = Guid.NewGuid(), _shiftBudi = Guid.NewGuid(), _shiftDraf = Guid.NewGuid();
    private readonly Guid _budi = Guid.NewGuid(), _tono = Guid.NewGuid(), _dina = Guid.NewGuid();
    private readonly Guid _sari = Guid.NewGuid(), _orangLuar = Guid.NewGuid();
    private static readonly DateOnly T14 = new(2027, 2, 14), T15 = new(2027, 2, 15);

    private static DateTime U(DateTime x) => DateTime.SpecifyKind(x, DateTimeKind.Utc);

    public PublishedRosterTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _icu, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        ctx.MstShifts.Add(new MstShift { Id = _pagi, ShiftCode = "P", ShiftName = "Pagi", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(14, 0), PaidWorkMinutes = 420, IsActive = true });
        ctx.MstShifts.Add(new MstShift { Id = _malam, ShiftCode = "M", ShiftName = "Malam", StartTime = new TimeOnly(21, 0), EndTime = new TimeOnly(7, 0), IsNightShift = true, IsOvernight = true, PaidWorkMinutes = 600, IsActive = true });
        foreach (var (id, nama, diIcu) in new[] { (_budi, "Budi", true), (_tono, "Tono", true), (_dina, "Dina", false) })
        {
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = nama, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
            if (diIcu)
            {
                ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
                {
                    WorkforceProfileId = id, OrganizationUnitId = _icu, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(),
                    AssignmentType = "Primary", EffectiveStartDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true,
                });
            }
        }

        ctx.Users.Add(new ApplicationUser { Id = _sari, UserName = "sari", UserCode = "SARI0001", DisplayName = "Sari", UserType = UserType.SuperAdmin, IsActive = true });
        ctx.Users.Add(new ApplicationUser { Id = _orangLuar, UserName = "luar", UserCode = "LUAR0001", DisplayName = "Luar", UserType = UserType.Employee, IsActive = true });

        foreach (var (id, status, versi) in new[] { (_periode, "Published", 3), (_draf, "Draft", 1) })
        {
            ctx.TrxRosterPeriods.Add(new TrxRosterPeriod
            {
                Id = id, OrganizationUnitId = _icu, RosterPeriodCode = "R-" + status, RosterPeriodName = "ICU " + status, RosterStatus = status,
                IsPublished = status == "Published", VersionNumber = versi, PeriodStartDate = new DateOnly(2027, 2, 1), PeriodEndDate = new DateOnly(2027, 2, 28),
                IsActive = true, TotalAssignmentCount = 1,
            });
        }

        var raDraf = Guid.NewGuid();
        ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = _raBudi, RosterPeriodId = _periode, WorkforceProfileId = _budi, AssignmentStatus = "Published", PlannedShiftCount = 1, PlannedWorkMinutes = 600, PlannedNightShiftCount = 1, IsActive = true });
        ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = raDraf, RosterPeriodId = _draf, WorkforceProfileId = _budi, IsActive = true });
        ctx.TrxShiftAssignments.Add(new TrxShiftAssignment
        {
            Id = _shiftBudi, RosterAssignmentId = _raBudi, WorkforceProfileId = _budi, ShiftId = _malam, ShiftDate = T14,
            ScheduledStartAt = new DateTime(2027, 2, 14, 14, 0, 0, DateTimeKind.Utc), ScheduledEndAt = new DateTime(2027, 2, 15, 0, 0, 0, DateTimeKind.Utc),
            PlannedWorkMinutes = 600, IsNightShift = true, AssignmentStatus = "Published", IsActive = true,
        });
        ctx.TrxShiftAssignments.Add(new TrxShiftAssignment
        {
            Id = _shiftDraf, RosterAssignmentId = raDraf, WorkforceProfileId = _budi, ShiftId = _malam, ShiftDate = T14,
            ScheduledStartAt = new DateTime(2027, 2, 14, 14, 0, 0, DateTimeKind.Utc), ScheduledEndAt = new DateTime(2027, 2, 15, 0, 0, 0, DateTimeKind.Utc),
            IsNightShift = true, AssignmentStatus = "Draft", IsActive = true,
        });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private RosterPeriodService Layanan(Guid? pelaku = null, params Microsoft.EntityFrameworkCore.Diagnostics.IInterceptor[] interceptors)
    {
        var ctx = _db.CreateContext(interceptors);
        var kesiapan = new KesiapanTiruan();
        var scope = new HrdUnitScopeService(ctx, Pelaku.Izin(ctx), Pelaku.Sebagai(pelaku ?? _sari));
        return new RosterPeriodService(ctx, new RosterValidationService(ctx, kesiapan, new SetelanBawaan(), NullLogger<RosterValidationService>.Instance),
            kesiapan, NullLogger<RosterPeriodService>.Instance, scope, new ShiftRevisionRecorder(ctx));
    }

    private static CreateRosterAssignmentRequest Tambah(Guid pegawai, params (DateOnly Tanggal, Guid Shift)[] shifts) => new()
    {
        WorkforceProfileId = pegawai,
        ReasonCategory = "Kekurangan tenaga",
        ReasonNote = "Pasien ICU bertambah",
        Shifts = shifts.Select(s => new RosterShiftItemRequest { ShiftDate = s.Tanggal, ShiftId = s.Shift }).ToList(),
    };

    private (TrxRosterPeriod Periode, List<HrdShiftAssignmentRevision> Riwayat, List<TrxShiftAssignment> Shift) Keadaan(Guid periode)
    {
        using var ctx = _db.CreateContext();
        return (ctx.TrxRosterPeriods.AsNoTracking().Single(p => p.Id == periode),
            ctx.HrdShiftAssignmentRevisions.AsNoTracking().Where(r => r.RosterPeriodId == periode).ToList(),
            ctx.TrxShiftAssignments.AsNoTracking().Where(s => s.RosterAssignment!.RosterPeriodId == periode).ToList());
    }

    // AT-HRD-PK-05
    [Fact]
    public async Task Tambah_shift_pada_roster_terbit_tetap_Published_dan_menulis_riwayat_AddShift()
    {
        var r = await Layanan().AddAssignmentAsync(_periode, Tambah(_tono, (T15, _pagi)), _sari);
        Assert.True(r.Success, r.Message);
        Assert.Equal(201, r.StatusCode);

        var (periode, riwayat, shift) = Keadaan(_periode);
        Assert.Equal("Published", periode.RosterStatus);
        Assert.Equal(3, periode.VersionNumber);

        var baris = Assert.Single(riwayat);
        Assert.Equal(baris.Id, Assert.Single(r.Data!.RevisionIds));
        Assert.Equal("AddShift", baris.ChangeType);
        Assert.Null(baris.PreviousWorkforceProfileId);
        Assert.Null(baris.PreviousShiftId);
        Assert.Equal(_tono, baris.NewWorkforceProfileId);
        Assert.Equal(_pagi, baris.NewShiftId);
        Assert.Equal(new DateTime(2027, 2, 15, 0, 0, 0, DateTimeKind.Utc), U(baris.NewStartAt!.Value)); // 07.00 WIB
        Assert.Equal(new DateTime(2027, 2, 15, 7, 0, 0, DateTimeKind.Utc), U(baris.NewEndAt!.Value));   // 14.00 WIB
        Assert.Equal("Kekurangan tenaga", baris.ReasonCategory);

        var baru = shift.Single(s => s.WorkforceProfileId == _tono);
        Assert.Equal("Published", baru.AssignmentStatus);
        Assert.Equal(baru.Id, baris.ShiftAssignmentId);
    }

    [Fact]
    public async Task Tambah_shift_pegawai_yang_sudah_ada_memakai_penugasan_yang_sama()
    {
        var r = await Layanan().AddAssignmentAsync(_periode, Tambah(_budi, (T15.AddDays(1), _pagi)), _sari);
        Assert.True(r.Success, r.Message);
        Assert.Equal(_raBudi, r.Data!.Id);

        using var ctx = _db.CreateContext();
        Assert.Single(ctx.TrxRosterAssignments.Where(a => a.RosterPeriodId == _periode && a.WorkforceProfileId == _budi));
        Assert.Equal(2, ctx.TrxRosterAssignments.Single(a => a.Id == _raBudi).PlannedShiftCount);
        Assert.Equal(1, ctx.TrxRosterPeriods.Single(p => p.Id == _periode).TotalAssignmentCount);
    }

    [Theory]
    [InlineData(null, "catatan")]
    [InlineData("Kekurangan tenaga", "  ")]
    public async Task Tambah_shift_roster_terbit_tanpa_alasan_400(string? kategori, string catatan)
    {
        var req = Tambah(_tono, (T15, _pagi));
        req.ReasonCategory = kategori;
        req.ReasonNote = catatan;
        var r = await Layanan().AddAssignmentAsync(_periode, req, _sari);
        Assert.Equal(400, r.StatusCode);
        Assert.Empty(Keadaan(_periode).Riwayat);
    }

    // INV-A52-03: pemeriksaan penuh — B3 (tanpa penempatan di ICU) dan B1 (tabrakan dengan shift yang ada).
    [Fact]
    public async Task Tambah_shift_yang_terblokir_ditolak_422_tanpa_perubahan()
    {
        var b3 = await Layanan().AddAssignmentAsync(_periode, Tambah(_dina, (T15, _pagi)), _sari);
        Assert.Equal(422, b3.StatusCode);
        Assert.Contains(((IEnumerable<RosterValidationConflictItemDto>)b3.Errors!), e => e.RuleCode == "B3");

        var b1 = await Layanan().AddAssignmentAsync(_periode, Tambah(_budi, (T14, _malam)), _sari);
        Assert.Equal(422, b1.StatusCode);
        Assert.Contains(((IEnumerable<RosterValidationConflictItemDto>)b1.Errors!), e => e.RuleCode == "B1");

        var (periode, riwayat, shift) = Keadaan(_periode);
        Assert.Empty(riwayat);
        Assert.Single(shift);
        Assert.Equal("Published", periode.RosterStatus);
    }

    [Fact]
    public async Task Dua_shift_bertabrakan_dalam_satu_permintaan_ditolak_400()
    {
        var r = await Layanan().AddAssignmentAsync(_periode, Tambah(_tono, (T15, _pagi), (T15, _pagi)), _sari);
        Assert.Equal(400, r.StatusCode);
        Assert.Empty(Keadaan(_periode).Riwayat);
    }

    // Regresi: roster draf tetap memakai perilaku lama (tanpa alasan, tanpa riwayat).
    [Fact]
    public async Task Tambah_shift_roster_draf_tanpa_alasan_dan_tanpa_riwayat()
    {
        var r = await Layanan().AddAssignmentAsync(_draf, new CreateRosterAssignmentRequest
        {
            WorkforceProfileId = _tono,
            Shifts = new() { new RosterShiftItemRequest { ShiftDate = T15, ShiftId = _pagi } }
        }, _sari);
        Assert.True(r.Success, r.Message);
        var (periode, riwayat, _) = Keadaan(_draf);
        Assert.Equal("Draft", periode.RosterStatus);
        Assert.Empty(riwayat);
    }

    // AT-HRD-PK-06
    [Fact]
    public async Task Hapus_shift_tanpa_alasan_400_lalu_dengan_alasan_IsCancel_dan_riwayat_RemoveShift()
    {
        var tanpa = await Layanan().RemovePublishedShiftAsync(_periode, _shiftBudi, new RemovePublishedShiftRequest(), _sari);
        Assert.Equal(400, tanpa.StatusCode);
        Assert.Equal("Alasan perubahan wajib diisi.", tanpa.Message);
        Assert.Empty(Keadaan(_periode).Riwayat);

        var r = await Layanan().RemovePublishedShiftAsync(_periode, _shiftBudi,
            new RemovePublishedShiftRequest { ReasonCategory = "Ruang ditutup", ReasonNote = "ICU bed 3 renovasi" }, _sari);
        Assert.True(r.Success, r.Message);
        Assert.Equal("RemoveShift", r.Data!.ChangeType);
        Assert.Equal("Hapus Shift", r.Data.ChangeTypeLabel);
        Assert.Equal("Budi", r.Data.PreviousWorkforceName);
        Assert.Equal("Malam", r.Data.PreviousShiftName);
        Assert.Null(r.Data.NewWorkforceProfileId);
        Assert.Equal("Sari", r.Data.ChangedByName);

        var (periode, riwayat, shift) = Keadaan(_periode);
        Assert.Equal("Published", periode.RosterStatus);
        Assert.Equal(3, periode.VersionNumber);
        var baris = Assert.Single(riwayat);
        Assert.Equal(_budi, baris.PreviousWorkforceProfileId);
        Assert.Equal(_malam, baris.PreviousShiftId);
        Assert.Equal(new DateTime(2027, 2, 14, 14, 0, 0, DateTimeKind.Utc), U(baris.PreviousStartAt!.Value));
        var s = Assert.Single(shift);
        Assert.True(s.IsCancel);
        Assert.False(s.IsActive);
        Assert.Equal("Cancelled", s.AssignmentStatus);

        using var ctx = _db.CreateContext();
        var ra = ctx.TrxRosterAssignments.Single(a => a.Id == _raBudi);
        Assert.Equal(0, ra.PlannedShiftCount);
        Assert.Equal(0, ra.PlannedNightShiftCount);

        // Hapus kedua kalinya: shift sudah batal → 404.
        var lagi = await Layanan().RemovePublishedShiftAsync(_periode, _shiftBudi,
            new RemovePublishedShiftRequest { ReasonCategory = "Ruang ditutup", ReasonNote = "ulang" }, _sari);
        Assert.Equal(404, lagi.StatusCode);
    }

    [Fact]
    public async Task Hapus_shift_yang_baru_diganti_petugas_ditolak_409()
    {
        var penyela = new PenyelaSerentak(_shiftBudi, _tono);
        var r = await Layanan(null, penyela).RemovePublishedShiftAsync(_periode, _shiftBudi,
            new RemovePublishedShiftRequest { ReasonCategory = "Ruang ditutup", ReasonNote = "renovasi" }, _sari);
        Assert.True(penyela.Terjadi);
        Assert.Equal(409, r.StatusCode);
        Assert.Empty(Keadaan(_periode).Riwayat);
    }

    [Fact]
    public async Task Hapus_shift_pada_roster_draf_ditolak_409()
    {
        var r = await Layanan().RemovePublishedShiftAsync(_draf, _shiftDraf,
            new RemovePublishedShiftRequest { ReasonCategory = "x", ReasonNote = "y" }, _sari);
        Assert.Equal(409, r.StatusCode);
    }

    [Fact]
    public async Task Hapus_shift_milik_periode_lain_404()
    {
        var r = await Layanan().RemovePublishedShiftAsync(_periode, _shiftDraf,
            new RemovePublishedShiftRequest { ReasonCategory = "x", ReasonNote = "y" }, _sari);
        Assert.Equal(404, r.StatusCode);
    }

    // Riwayat: urutan terbaru di atas, filter, label, dan cakupan baca (VR-PK-27).
    [Fact]
    public async Task Riwayat_periode_dapat_dibaca_dan_difilter()
    {
        Assert.True((await Layanan().AddAssignmentAsync(_periode, Tambah(_tono, (T15, _pagi)), _sari)).Success);
        Assert.True((await Layanan().RemovePublishedShiftAsync(_periode, _shiftBudi,
            new RemovePublishedShiftRequest { ReasonCategory = "Ruang ditutup", ReasonNote = "renovasi" }, _sari)).Success);

        var semua = await Layanan().GetRevisionsAsync(_periode, null, null, null, null, 1, 20);
        Assert.True(semua.Success, semua.Message);
        Assert.Equal(2, semua.Data!.TotalData);
        Assert.Equal("RemoveShift", semua.Data.Items[0].ChangeType);
        Assert.Equal("Tambah Shift", semua.Data.Items[1].ChangeTypeLabel);
        Assert.Equal("Tono", semua.Data.Items[1].NewWorkforceName);
        Assert.Equal("Pagi", semua.Data.Items[1].NewShiftName);

        Assert.Equal(1, (await Layanan().GetRevisionsAsync(_periode, null, null, null, "AddShift", 1, 20)).Data!.TotalData);
        Assert.Equal(1, (await Layanan().GetRevisionsAsync(_periode, null, null, _budi, null, 1, 20)).Data!.TotalData);
        Assert.Equal(1, (await Layanan().GetRevisionsAsync(_periode, T15, T15, null, null, 1, 20)).Data!.TotalData);
        Assert.Equal(400, (await Layanan().GetRevisionsAsync(_periode, T15, T14, null, null, 1, 20)).StatusCode);

        var luar = await Layanan(_orangLuar).GetRevisionsAsync(_periode, null, null, null, null, 1, 20);
        Assert.Equal(403, luar.StatusCode);
    }

    // AT-HRD-PK-08 / INV-A52-02: tidak ada endpoint ubah atau hapus riwayat di controller Penjadwalan
    // (rute "revision" di modul lain milik alur persetujuan/dokumen yang tidak berhubungan).
    [Fact]
    public void Riwayat_tidak_punya_endpoint_ubah_atau_hapus()
    {
        var aksi = typeof(RosterPeriodController).Assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && (t.Namespace ?? string.Empty).Contains(".SchedulingManagement.", StringComparison.Ordinal))
            .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .SelectMany(m => m.GetCustomAttributes<HttpMethodAttribute>()
                    .Select(a => (Controller: t, Template: a.Template ?? string.Empty, Metode: a.HttpMethods.ToList()))))
            .Where(x => x.Template.Contains("revision", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Contains(aksi, x => x.Controller == typeof(RosterPeriodController) && x.Metode.Contains("GET"));
        Assert.DoesNotContain(aksi, x => x.Metode.Any(m => m is "PUT" or "PATCH" or "DELETE" or "POST"));
    }

    [Fact]
    public void Tambah_dan_hapus_memakai_hak_akses_kontrak()
    {
        string Izin(string metode) => typeof(RosterPeriodController).GetMethod(metode)!.GetCustomAttribute<AccessPermissionAttribute>() is { } a
            ? $"{a.Arguments[0]}:{a.Arguments[1]}" : "-";
        Assert.Equal("RosterPeriod:Create", Izin(nameof(RosterPeriodController.AddAssignment)));
        Assert.Equal("RosterPeriod:RemoveShift", Izin(nameof(RosterPeriodController.RemoveShift)));
        Assert.Equal("RosterPeriod:Read", Izin(nameof(RosterPeriodController.GetRevisions)));
    }
}
