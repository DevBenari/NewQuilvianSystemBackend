using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-336 — contoh 35.9.1: Sari (Kepala Ruang ICU) dan Pak Dodi (Bidang Keperawatan, induk ICU/HCU/Anak).
// Hierarki: RS → Bidang Keperawatan → {ICU, HCU, Bangsal Anak → Anak Isolasi}; unit Farmasi di luar bidang.
public sealed class UnitScopeTests : IDisposable
{
    private const string TolakKelola = "Anda tidak berwenang mengubah roster unit ini.";
    private const string TolakBaca = "Anda tidak berwenang melihat data unit ini.";

    private readonly TestDatabase _db = TestDatabase.Create();

    private readonly Guid _rs = Guid.NewGuid(), _bidang = Guid.NewGuid(), _icu = Guid.NewGuid(), _hcu = Guid.NewGuid(),
        _anak = Guid.NewGuid(), _anakIsolasi = Guid.NewGuid(), _farmasi = Guid.NewGuid();

    private readonly Guid _userSari = Guid.NewGuid(), _userDodi = Guid.NewGuid(), _userTanpaJabatan = Guid.NewGuid(),
        _userKedaluwarsa = Guid.NewGuid(), _userSuper = Guid.NewGuid(), _userHrScheduling = Guid.NewGuid();

    private readonly Guid _profilSari = Guid.NewGuid(), _profilDodi = Guid.NewGuid(), _profilTanpaJabatan = Guid.NewGuid(),
        _profilKedaluwarsa = Guid.NewGuid(), _profilHr = Guid.NewGuid(), _profilPerawat = Guid.NewGuid();

    private readonly Guid _periodeIcu = Guid.NewGuid(), _periodeHcu = Guid.NewGuid(), _periodeFarmasi = Guid.NewGuid(),
        _periodeTanpaUnit = Guid.NewGuid();

    private readonly Guid _shiftIcu = Guid.NewGuid(), _shiftHcu = Guid.NewGuid();

    private readonly Guid _deptHr = Guid.NewGuid(), _posisiHr = Guid.NewGuid();

    public UnitScopeTests()
    {
        using var ctx = _db.CreateContext();
        var legal = Guid.NewGuid();
        MstOrganizationUnit Unit(Guid id, string kode, Guid? induk) => new()
        { Id = id, LegalEntityId = legal, UnitCode = kode, UnitName = kode, ParentOrganizationUnitId = induk };
        ctx.MstOrganizationUnits.AddRange(
            Unit(_rs, "RS", null), Unit(_bidang, "BIDKEP", _rs), Unit(_icu, "ICU", _bidang), Unit(_hcu, "HCU", _bidang),
            Unit(_anak, "ANAK", _bidang), Unit(_anakIsolasi, "ANAK-ISO", _anak), Unit(_farmasi, "FARMASI", _rs));

        MstWorkforceProfile Profil(Guid id, string nama) => new() { Id = id, ProfileCode = nama, DisplayName = nama, UserType = UserType.Employee };
        ctx.MstWorkforceProfiles.AddRange(Profil(_profilSari, "Sari"), Profil(_profilDodi, "Dodi"), Profil(_profilTanpaJabatan, "Budi"),
            Profil(_profilKedaluwarsa, "Lama"), Profil(_profilHr, "HR"), Profil(_profilPerawat, "Perawat"));

        ApplicationUser Akun(Guid id, Guid? profil, UserType tipe) => new()
        { Id = id, UserName = id.ToString(), UserCode = id.ToString("N")[..8], DisplayName = "Uji", UserType = tipe, WorkforceProfileId = profil, IsActive = true };
        ctx.Users.AddRange(Akun(_userSari, _profilSari, UserType.Employee), Akun(_userDodi, _profilDodi, UserType.Employee),
            Akun(_userTanpaJabatan, _profilTanpaJabatan, UserType.Employee), Akun(_userKedaluwarsa, _profilKedaluwarsa, UserType.Employee),
            Akun(_userSuper, null, UserType.SuperAdmin), Akun(_userHrScheduling, _profilHr, UserType.Employee));

        var hariIni = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(7).Date, DateTimeKind.Utc);
        WfpOrganizationAssignment Penempatan(Guid profil, Guid unit, bool berjabatan, DateTime mulai, DateTime? akhir = null) => new()
        {
            WorkforceProfileId = profil, OrganizationUnitId = unit, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(),
            IsManagerialAssignment = berjabatan, EffectiveStartDate = mulai, EffectiveEndDate = akhir, IsActive = true,
        };
        ctx.WfpOrganizationAssignments.AddRange(
            Penempatan(_profilSari, _icu, true, hariIni.AddYears(-1)),
            Penempatan(_profilDodi, _bidang, true, hariIni.AddYears(-2)),
            Penempatan(_profilTanpaJabatan, _icu, false, hariIni.AddYears(-1)),            // bukan berjabatan
            Penempatan(_profilKedaluwarsa, _hcu, true, hariIni.AddYears(-3), hariIni.AddDays(-1)), // berakhir kemarin
            Penempatan(_profilKedaluwarsa, _farmasi, true, hariIni.AddDays(1)),            // baru mulai besok
            Penempatan(_profilHr, _farmasi, false, hariIni.AddYears(-1)));

        TrxRosterPeriod Periode(Guid id, Guid? unit, string kode) => new()
        { Id = id, OrganizationUnitId = unit, RosterPeriodCode = kode, RosterPeriodName = kode, RosterStatus = "Published",
          PeriodStartDate = new DateOnly(2026, 11, 1), PeriodEndDate = new DateOnly(2026, 11, 30), IsActive = true };
        ctx.TrxRosterPeriods.AddRange(Periode(_periodeIcu, _icu, "R-ICU"), Periode(_periodeHcu, _hcu, "R-HCU"),
            Periode(_periodeFarmasi, _farmasi, "R-FAR"), Periode(_periodeTanpaUnit, null, "R-NULL"));

        var shiftMaster = Guid.NewGuid();
        ctx.MstShifts.Add(new MstShift { Id = shiftMaster, ShiftCode = "P", ShiftName = "Pagi", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(14, 0) });
        foreach (var (shiftId, periode) in new[] { (_shiftIcu, _periodeIcu), (_shiftHcu, _periodeHcu) })
        {
            var ra = Guid.NewGuid();
            ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = ra, RosterPeriodId = periode, WorkforceProfileId = _profilPerawat });
            ctx.TrxShiftAssignments.Add(new TrxShiftAssignment
            {
                Id = shiftId, RosterAssignmentId = ra, WorkforceProfileId = _profilPerawat, ShiftId = shiftMaster,
                ShiftDate = new DateOnly(2026, 11, 20), ScheduledStartAt = new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc),
                ScheduledEndAt = new DateTime(2026, 11, 20, 7, 0, 0, DateTimeKind.Utc), AssignmentStatus = "Published",
            });
        }

        // Petugas HR Scheduling mendapat penanda RosterPeriod : ManageAllUnits lewat Akses Role (departemen + jabatan).
        var controller = new SysControllerAccess { ModuleId = Guid.NewGuid(), ControllerName = "RosterPeriod", DisplayName = "Roster" };
        var action = new SysActionAccess { ControllerAccessId = controller.Id, ActionName = "ManageAllUnits", DisplayName = "Manage All", AccessType = "Update" };
        ctx.SysControllerAccesses.Add(controller);
        ctx.SysActionAccesses.Add(action);
        ctx.ApplicationUserOrganizations.Add(new ApplicationUserOrganization { UserId = _userHrScheduling, DepartmentId = _deptHr, PositionId = _posisiHr, IsActive = true });
        ctx.SysAccessPolicies.Add(new SysAccessPolicy { DepartmentId = _deptHr, PositionId = _posisiHr, ControllerAccessId = controller.Id, ActionAccessId = action.Id, IsAllowed = true, IsActive = true });

        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private (HrdUnitScopeService scope, ApplicationDbContext ctx) Cakupan(Guid? userId)
    {
        var ctx = _db.CreateContext();
        return (new HrdUnitScopeService(ctx, Pelaku.Izin(ctx), Pelaku.Sebagai(userId)), ctx);
    }

    private RosterPeriodService Roster(Guid? userId)
    {
        var (scope, ctx) = Cakupan(userId);
        return new RosterPeriodService(ctx, Pengganti.Buat<IRosterValidationService>(), Pengganti.Buat<IHrdEmploymentReadinessReadService>(),
            NullLogger<RosterPeriodService>.Instance, scope, new ShiftRevisionRecorder(ctx));
    }

    private ShiftReplacementService Ganti(Guid? userId)
    {
        var (scope, ctx) = Cakupan(userId);
        return new ShiftReplacementService(ctx, Pengganti.Buat<IHrdEmploymentReadinessReadService>(),
            NullLogger<ShiftReplacementService>.Instance, Pengganti.Buat<IServiceProvider>(), scope,
            new RosterValidationService(ctx, new KesiapanTiruan(), new SetelanBawaan(), NullLogger<RosterValidationService>.Instance),
            new ShiftRevisionRecorder(ctx), Pengganti.Buat<QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services.IUnifiedNotificationGatewayService>(), new SetelanBawaan());
    }

    // ---------- HrdUnitScopeService ----------

    [Fact]
    public async Task Sari_kelola_hanya_ICU_dan_baca_ICU()
    {
        var (s, _) = Cakupan(_userSari);
        var kelola = await s.GetManageScopeAsync();
        var baca = await s.GetReadScopeAsync();
        Assert.False(kelola.IsAllUnits);
        Assert.Equal(new[] { _icu }, kelola.UnitIds);
        Assert.Equal(new[] { _icu }, baca.UnitIds);
        Assert.False(kelola.Contains(_hcu));
    }

    [Fact]
    public async Task Dodi_kelola_bidang_saja_baca_seluruh_turunan_tiga_tingkat()
    {
        var (s, _) = Cakupan(_userDodi);
        var kelola = await s.GetManageScopeAsync();
        var baca = await s.GetReadScopeAsync();
        Assert.Equal(new[] { _bidang }, kelola.UnitIds);
        Assert.Equal(new HashSet<Guid> { _bidang, _icu, _hcu, _anak, _anakIsolasi }, baca.UnitIds.ToHashSet());
        Assert.False(baca.Contains(_farmasi));
        Assert.False(baca.Contains(_rs));
    }

    [Theory]
    [InlineData("tanpaJabatan")]
    [InlineData("kedaluwarsa")]
    [InlineData("tanpaLogin")]
    public async Task Cakupan_kosong_gagal_tertutup(string kasus)
    {
        Guid? user = kasus switch { "tanpaJabatan" => _userTanpaJabatan, "kedaluwarsa" => _userKedaluwarsa, _ => null };
        var (s, _) = Cakupan(user);
        var kelola = await s.GetManageScopeAsync();
        var baca = await s.GetReadScopeAsync();
        Assert.False(kelola.IsAllUnits);
        Assert.Empty(kelola.UnitIds);
        Assert.False(baca.IsAllUnits);
        Assert.Empty(baca.UnitIds);
        Assert.False(kelola.Contains(null));
    }

    [Fact]
    public async Task Superadmin_dan_penanda_ManageAllUnits_mencakup_semua_unit()
    {
        foreach (var user in new[] { _userSuper, _userHrScheduling })
        {
            var (s, _) = Cakupan(user);
            Assert.True((await s.GetManageScopeAsync()).IsAllUnits);
            Assert.True((await s.GetReadScopeAsync()).IsAllUnits);
            Assert.True((await s.GetManageScopeAsync()).Contains(null));
        }
    }

    // ---------- RosterPeriodService ----------

    [Fact]
    public async Task Sari_tambah_penugasan_HCU_ditolak_403_ICU_lolos_cakupan()
    {
        var req = new CreateRosterAssignmentRequest { WorkforceProfileId = Guid.NewGuid() };
        var hcu = await Roster(_userSari).AddAssignmentAsync(_periodeHcu, req, _userSari);
        Assert.Equal(403, hcu.StatusCode);
        Assert.Equal(TolakKelola, hcu.Message);

        // Kontrol: di ICU pemeriksaan cakupan lolos, lalu berhenti di 404 karena pegawai tidak ada.
        var icu = await Roster(_userSari).AddAssignmentAsync(_periodeIcu, req, _userSari);
        Assert.Equal(404, icu.StatusCode);
    }

    [Fact]
    public async Task Dodi_boleh_baca_ICU_tetapi_ubah_ICU_ditolak_403()
    {
        var detail = await Roster(_userDodi).GetByIdAsync(_periodeIcu);
        Assert.Equal(200, detail.StatusCode);

        var tutup = await Roster(_userDodi).CloseRosterPeriodAsync(_periodeIcu, _userDodi);
        Assert.Equal(403, tutup.StatusCode);
        Assert.Equal(TolakKelola, tutup.Message);
    }

    [Fact]
    public async Task Sari_baca_detail_HCU_ditolak_403()
    {
        var r = await Roster(_userSari).GetByIdAsync(_periodeHcu);
        Assert.Equal(403, r.StatusCode);
        Assert.Equal(TolakBaca, r.Message);
    }

    [Fact]
    public async Task Daftar_dan_ringkasan_periode_tersaring_cakupan_baca()
    {
        var sari = await Roster(_userSari).GetPagedAsync();
        Assert.Equal(new[] { "R-ICU" }, sari.Items.Select(x => x.RosterPeriodCode));

        var dodi = await Roster(_userDodi).GetPagedAsync();
        Assert.Equal(new[] { "R-HCU", "R-ICU" }, dodi.Items.Select(x => x.RosterPeriodCode).OrderBy(x => x));

        var hr = await Roster(_userHrScheduling).GetPagedAsync();
        Assert.Equal(4, hr.TotalData); // termasuk periode tanpa unit

        var kosong = await Roster(_userTanpaJabatan).GetPagedAsync();
        Assert.Equal(0, kosong.TotalData);

        Assert.Equal(2, (await Roster(_userDodi).GetSummaryAsync()).TotalPeriods);
    }

    [Fact]
    public async Task Buat_periode_di_luar_cakupan_atau_tanpa_unit_ditolak_403()
    {
        CreateRosterPeriodRequest Req(Guid? unit) => new()
        { RosterPeriodName = "Des", OrganizationUnitId = unit, PeriodStartDate = new DateOnly(2026, 12, 1), PeriodEndDate = new DateOnly(2026, 12, 31) };

        Assert.Equal(403, (await Roster(_userSari).CreateAsync(Req(_hcu), _userSari)).StatusCode);
        Assert.Equal(403, (await Roster(_userSari).CreateAsync(Req(null), _userSari)).StatusCode);
        Assert.Equal(201, (await Roster(_userSari).CreateAsync(Req(_icu), _userSari)).StatusCode);
        Assert.Equal(201, (await Roster(_userHrScheduling).CreateAsync(Req(null), _userHrScheduling)).StatusCode);
    }

    [Fact]
    public async Task Periode_tanpa_unit_hanya_untuk_cakupan_semua_unit()
    {
        var sari = await Roster(_userSari).PublishRosterPeriodAsync(_periodeTanpaUnit, new PublishRosterPeriodRequest(), _userSari);
        Assert.Equal(403, sari.StatusCode);

        var validasi = await Roster(_userSari).ValidateRosterPeriodAsync(_periodeHcu, _userSari);
        Assert.Equal(403, validasi.StatusCode);
    }

    [Fact]
    public async Task Salin_dari_periode_sumber_di_luar_cakupan_baca_ditolak_403()
    {
        var r = await Roster(_userSari).CopyPreviousPeriodAsync(_periodeIcu, new CopyPreviousRosterPeriodRequest { SourceRosterPeriodId = _periodeFarmasi }, _userSari);
        Assert.Equal(403, r.StatusCode);
        Assert.Equal(TolakBaca, r.Message);
    }

    // ---------- ShiftReplacementService ----------

    [Fact]
    public async Task Ganti_petugas_shift_HCU_oleh_Sari_ditolak_403()
    {
        var hcu = await Ganti(_userSari).ExecuteDirectReplacementAsync(_shiftHcu, new DirectShiftReplacementRequest { ReplacementEmployeeId = Guid.NewGuid() }, _userSari);
        Assert.Equal(403, hcu.StatusCode);
        Assert.Equal(TolakKelola, hcu.Message);

        // Kontrol: di ICU cakupan lolos, lalu ditolak 400 karena pengganti sama dengan petugas.
        var icu = await Ganti(_userSari).ExecuteDirectReplacementAsync(_shiftIcu, new DirectShiftReplacementRequest { ReplacementEmployeeId = _profilPerawat }, _userSari);
        Assert.Equal(400, icu.StatusCode);
    }

    [Fact]
    public async Task Koreksi_mundur_shift_HCU_oleh_Sari_ditolak_403()
    {
        var r = await Ganti(_userSari).RetroactiveCorrectionAsync(_shiftHcu, new RetroactiveShiftCorrectionRequest(), _userSari);
        Assert.Equal(403, r.StatusCode);
    }
}
