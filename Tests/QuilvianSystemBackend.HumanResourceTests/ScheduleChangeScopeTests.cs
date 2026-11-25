using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Controllers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Responses;
using Ubah = QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants.SchedulingRequestValueConstants.ScheduleChangeStatus;
using Tukar = QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Constants.SchedulingRequestValueConstants.ShiftSwapStatus;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-452 — AT-HRD-PK-52 (HRD-DEC-242, kontrak v30 API 36.7, VR-PK-35, Permission 34.2): Perubahan Shift dan
// Tukar Dinas sisi HR mengikuti cakupan unit. Lewat controller HR. Sari = Kepala Unit ICU; IGD di luar cakupannya.
public sealed class ScheduleChangeScopeTests : IDisposable
{
    private const string TolakKelola = "Anda tidak berwenang mengubah roster unit ini.";

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _rs = Guid.NewGuid(), _icu = Guid.NewGuid(), _icuIso = Guid.NewGuid(), _igd = Guid.NewGuid();
    private readonly Guid _uSari = Guid.NewGuid(), _uHr = Guid.NewGuid(), _uBudi = Guid.NewGuid();
    private readonly Guid _deptHr = Guid.NewGuid(), _posisiHr = Guid.NewGuid();
    private readonly Guid _pagi = Guid.NewGuid(), _malam = Guid.NewGuid();
    private readonly Guid _ana = Guid.NewGuid(), _bayu = Guid.NewGuid(), _citra = Guid.NewGuid(), _dina = Guid.NewGuid(), _eko = Guid.NewGuid(), _fani = Guid.NewGuid(), _gita = Guid.NewGuid();

    // Shift: ICU (Ana, Bayu, Dina), IGD (Citra, Eko ×2), Fani pada periode tanpa unit tetapi shift-nya bertanda ICU,
    // Gita di ICU Isolasi (subunit ICU: dalam cakupan baca Sari, di luar cakupan kelolanya).
    private readonly Guid _sAna = Guid.NewGuid(), _sBayu = Guid.NewGuid(), _sDina = Guid.NewGuid(),
        _sCitra = Guid.NewGuid(), _sEko = Guid.NewGuid(), _sEko2 = Guid.NewGuid(), _sFani = Guid.NewGuid(), _sGita = Guid.NewGuid();

    // Perubahan Shift: ICU, IGD, Fani (unit dari shift), ICU Isolasi, dan jadwal tetap tanpa shift.
    private readonly Guid _ubahIcu = Guid.NewGuid(), _ubahIgd = Guid.NewGuid(), _ubahFani = Guid.NewGuid(), _ubahIso = Guid.NewGuid(), _ubahTetap = Guid.NewGuid();

    // Tukar Dinas: ICU–ICU, ICU–IGD (lintas), IGD–IGD.
    private readonly Guid _tukarIcu = Guid.NewGuid(), _tukarLintas = Guid.NewGuid(), _tukarIgd = Guid.NewGuid();

    public ScheduleChangeScopeTests()
    {
        using var ctx = _db.CreateContext();
        var legal = Guid.NewGuid();
        ctx.MstOrganizationUnits.AddRange(
            new MstOrganizationUnit { Id = _rs, LegalEntityId = legal, UnitCode = "RS", UnitName = "RS" },
            new MstOrganizationUnit { Id = _icu, LegalEntityId = legal, UnitCode = "ICU", UnitName = "ICU", ParentOrganizationUnitId = _rs },
            new MstOrganizationUnit { Id = _icuIso, LegalEntityId = legal, UnitCode = "ICU-ISO", UnitName = "ICU Isolasi", ParentOrganizationUnitId = _icu },
            new MstOrganizationUnit { Id = _igd, LegalEntityId = legal, UnitCode = "IGD", UnitName = "IGD", ParentOrganizationUnitId = _rs });

        Guid pSari = Guid.NewGuid(), pHr = Guid.NewGuid(), pBudi = Guid.NewGuid();
        foreach (var (id, nama) in new[] { (pSari, "Sari"), (pHr, "HR"), (pBudi, "Budi"), (_ana, "Ana"), (_bayu, "Bayu"), (_citra, "Citra"), (_dina, "Dina"), (_eko, "Eko"), (_fani, "Fani"), (_gita, "Gita") })
        {
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = nama.ToUpperInvariant(), DisplayName = nama, UserType = UserType.Employee, IsActive = true });
        }

        ApplicationUser Akun(Guid id, Guid profil) => new()
        { Id = id, UserName = id.ToString(), UserCode = id.ToString("N")[..8], DisplayName = "Uji", UserType = UserType.Employee, WorkforceProfileId = profil, IsActive = true };
        ctx.Users.AddRange(Akun(_uSari, pSari), Akun(_uHr, pHr), Akun(_uBudi, pBudi));

        var hariIni = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(7).Date, DateTimeKind.Utc);
        ctx.WfpOrganizationAssignments.AddRange(
            new WfpOrganizationAssignment { WorkforceProfileId = pSari, OrganizationUnitId = _icu, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), IsManagerialAssignment = true, EffectiveStartDate = hariIni.AddYears(-1), IsActive = true },
            new WfpOrganizationAssignment { WorkforceProfileId = pBudi, OrganizationUnitId = _icu, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), IsManagerialAssignment = false, EffectiveStartDate = hariIni.AddYears(-1), IsActive = true });

        // Petugas HR Scheduling: penanda RosterPeriod : ManageAllUnits lewat Akses Role (departemen + jabatan).
        var controller = new SysControllerAccess { ModuleId = Guid.NewGuid(), ControllerName = "RosterPeriod", DisplayName = "Roster" };
        var action = new SysActionAccess { ControllerAccessId = controller.Id, ActionName = "ManageAllUnits", DisplayName = "Manage All", AccessType = "Update" };
        ctx.SysControllerAccesses.Add(controller);
        ctx.SysActionAccesses.Add(action);
        ctx.ApplicationUserOrganizations.Add(new ApplicationUserOrganization { UserId = _uHr, DepartmentId = _deptHr, PositionId = _posisiHr, IsActive = true });
        ctx.SysAccessPolicies.Add(new SysAccessPolicy { DepartmentId = _deptHr, PositionId = _posisiHr, ControllerAccessId = controller.Id, ActionAccessId = action.Id, IsAllowed = true, IsActive = true });

        ctx.MstShifts.AddRange(
            new MstShift { Id = _pagi, ShiftCode = "P", ShiftName = "Pagi", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(14, 0), IsActive = true },
            new MstShift { Id = _malam, ShiftCode = "M", ShiftName = "Malam", StartTime = new TimeOnly(21, 0), EndTime = new TimeOnly(7, 0), IsOvernight = true, IsActive = true });

        Guid rpIcu = Guid.NewGuid(), rpIgd = Guid.NewGuid(), rpIso = Guid.NewGuid(), rpTanpaUnit = Guid.NewGuid();
        TrxRosterPeriod Periode(Guid id, Guid? unit, string kode) => new()
        { Id = id, OrganizationUnitId = unit, RosterPeriodCode = kode, RosterPeriodName = kode, RosterStatus = "Published",
          PeriodStartDate = new DateOnly(2026, 11, 1), PeriodEndDate = new DateOnly(2026, 11, 30), IsActive = true };
        ctx.TrxRosterPeriods.AddRange(Periode(rpIcu, _icu, "R-ICU"), Periode(rpIgd, _igd, "R-IGD"), Periode(rpIso, _icuIso, "R-ISO"), Periode(rpTanpaUnit, null, "R-NULL"));

        void Shift(Guid id, Guid periode, Guid pegawai, int tanggal, Guid? unitShift = null)
        {
            var ra = Guid.NewGuid();
            ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = ra, RosterPeriodId = periode, WorkforceProfileId = pegawai, IsActive = true });
            ctx.TrxShiftAssignments.Add(new TrxShiftAssignment
            {
                Id = id, RosterAssignmentId = ra, WorkforceProfileId = pegawai, ShiftId = _pagi, OrganizationUnitId = unitShift,
                ShiftDate = new DateOnly(2026, 11, tanggal), ScheduledStartAt = new DateTime(2026, 11, tanggal, 0, 0, 0, DateTimeKind.Utc),
                ScheduledEndAt = new DateTime(2026, 11, tanggal, 7, 0, 0, DateTimeKind.Utc), AssignmentStatus = "Published", IsActive = true,
            });
        }
        Shift(_sAna, rpIcu, _ana, 20);
        Shift(_sBayu, rpIcu, _bayu, 21);
        Shift(_sDina, rpIcu, _dina, 22);
        Shift(_sCitra, rpIgd, _citra, 20);
        Shift(_sEko, rpIgd, _eko, 23);
        Shift(_sEko2, rpIgd, _eko, 25);
        Shift(_sFani, rpTanpaUnit, _fani, 24, _icu);
        Shift(_sGita, rpIso, _gita, 27);

        WfpScheduleChangeRequest UbahShift(Guid id, string nomor, Guid pegawai, Guid? shift, int tanggal) => new()
        {
            Id = id, RequestNumber = nomor, WorkforceProfileId = pegawai, CurrentShiftAssignmentId = shift,
            CurrentShiftId = shift.HasValue ? _pagi : null, RequestedShiftId = shift.HasValue ? _malam : null,
            RequestType = shift.HasValue ? SchedulingRequestValueConstants.ScheduleChangeType.ShiftChange : SchedulingRequestValueConstants.ScheduleChangeType.ScheduleChange,
            RequestedDate = new DateOnly(2026, 11, tanggal), EffectiveStartDate = new DateOnly(2026, 11, tanggal),
            Reason = "Uji BE-HRD-452", RequestStatus = shift.HasValue ? Ubah.Approved : Ubah.Submitted, IsActive = true,
        };
        ctx.WfpScheduleChangeRequests.AddRange(
            UbahShift(_ubahIcu, "PSJ-ICU", _ana, _sAna, 20),
            UbahShift(_ubahIgd, "PSJ-IGD", _citra, _sCitra, 20),
            UbahShift(_ubahFani, "PSJ-FANI", _fani, _sFani, 24),
            UbahShift(_ubahIso, "PSJ-ISO", _gita, _sGita, 27),
            UbahShift(_ubahTetap, "PSJ-TETAP", _ana, null, 26));

        WfpShiftSwapRequest TukarDinas(Guid id, string nomor, Guid pemohon, Guid shiftPemohon, int tPemohon, Guid tujuan, Guid shiftTujuan, int tTujuan) => new()
        {
            Id = id, RequestNumber = nomor, RequesterWorkforceProfileId = pemohon, RequesterShiftAssignmentId = shiftPemohon,
            RequesterShiftDate = new DateOnly(2026, 11, tPemohon), TargetWorkforceProfileId = tujuan, TargetShiftAssignmentId = shiftTujuan,
            TargetShiftDate = new DateOnly(2026, 11, tTujuan), Reason = "Uji BE-HRD-452", RequestStatus = Tukar.Approved, IsAcceptedByTarget = true, IsActive = true,
        };
        ctx.WfpShiftSwapRequests.AddRange(
            TukarDinas(_tukarIcu, "TD-ICU", _bayu, _sBayu, 21, _dina, _sDina, 22),
            TukarDinas(_tukarLintas, "TD-LINTAS", _ana, _sAna, 20, _eko, _sEko, 23),
            TukarDinas(_tukarIgd, "TD-IGD", _citra, _sCitra, 20, _eko, _sEko2, 25));

        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private sealed record Layanan(ScheduleChangeController UbahHr, ShiftSwapController TukarHr, ScheduleChangeService Ubah, ShiftSwapService Tukar);

    private Layanan Sebagai(Guid userId)
    {
        var ctx = _db.CreateContext();
        var akses = Pelaku.Sebagai(userId);
        var cakupan = new HrdUnitScopeService(ctx, Pelaku.Izin(ctx), akses);
        var ubah = new ScheduleChangeService(ctx, cakupan);
        var tukar = new ShiftSwapService(ctx, cakupan);
        var cc = new ControllerContext { HttpContext = akses.HttpContext! };
        // Engine workflow tidak dibangun: pengajuan uji belum punya instance workflow, jadi jalurnya tidak menyentuh engine.
        return new Layanan(
            new ScheduleChangeController(ubah, new ScheduleChangeWorkflowIntegrationService(ctx, ubah, null!, null!)) { ControllerContext = cc },
            new ShiftSwapController(tukar, new ShiftSwapWorkflowIntegrationService(ctx, tukar, null!, null!)) { ControllerContext = cc },
            ubah, tukar);
    }

    private static (int Status, T? Data, string Message) Baca<T>(IActionResult hasil)
    {
        var o = Assert.IsAssignableFrom<ObjectResult>(hasil);
        return o.Value switch
        {
            ApiResponse<T> a => (o.StatusCode ?? 200, a.Data, a.Message),
            ApiResponse<object> f => (o.StatusCode ?? 200, default, f.Message),
            _ => throw new InvalidOperationException($"Tipe respons tidak dikenal: {o.Value?.GetType().Name}")
        };
    }

    private static int Status(IActionResult hasil) => Assert.IsAssignableFrom<ObjectResult>(hasil).StatusCode ?? 200;

    private Task<IActionResult> DaftarUbah(Layanan l) => l.UbahHr.GetPaged(null, null, null, null, null, null);
    private Task<IActionResult> DaftarTukar(Layanan l) => l.TukarHr.GetPaged(null, null, null, null, null);

    private (WfpScheduleChangeRequest Ubah, TrxShiftAssignment Shift) KeadaanUbah(Guid id)
    {
        using var ctx = _db.CreateContext();
        var u = ctx.WfpScheduleChangeRequests.AsNoTracking().Single(x => x.Id == id);
        return (u, ctx.TrxShiftAssignments.AsNoTracking().Single(x => x.Id == u.CurrentShiftAssignmentId));
    }

    private (WfpShiftSwapRequest Tukar, TrxShiftAssignment Pemohon, TrxShiftAssignment Tujuan) KeadaanTukar(Guid id)
    {
        using var ctx = _db.CreateContext();
        var t = ctx.WfpShiftSwapRequests.AsNoTracking().Single(x => x.Id == id);
        return (t,
            ctx.TrxShiftAssignments.AsNoTracking().Single(x => x.Id == t.RequesterShiftAssignmentId),
            ctx.TrxShiftAssignments.AsNoTracking().Single(x => x.Id == t.TargetShiftAssignmentId));
    }

    // AC1 (Perubahan Shift): Kepala Unit ICU melihat pengajuan ICU, subunitnya, dan yang unitnya dari shift; IGD dan jadwal tetap tidak.
    [Fact]
    public async Task Kepala_ICU_hanya_melihat_perubahan_shift_ICU()
    {
        var sari = Sebagai(_uSari);

        var (status, daftar, _) = Baca<PagedResult<ScheduleChangeListResponse>>(await DaftarUbah(sari));
        Assert.Equal(200, status);
        Assert.Equal(new[] { _ubahFani, _ubahIcu, _ubahIso }.OrderBy(x => x), daftar!.Items.Select(x => x.Id).OrderBy(x => x));
        Assert.Equal(3, daftar.TotalData);

        var (_, ringkasan, _) = Baca<ScheduleChangeSummaryResponse>(await sari.UbahHr.GetSummary(null, default));
        Assert.Equal(3, ringkasan!.TotalData);
        Assert.Equal(3, ringkasan.Approved);
        Assert.Equal(0, ringkasan.WaitingApproval);   // jadwal tetap (Submitted) tidak bertautan shift → di luar cakupan unit

        var (sIgd, _, pesan) = Baca<ScheduleChangeDetailResponse>(await sari.UbahHr.GetById(_ubahIgd, default));
        Assert.Equal(404, sIgd);
        Assert.Equal("Pengajuan perubahan jadwal tidak ditemukan.", pesan);
        Assert.Equal(404, Status(await sari.UbahHr.GetWorkflow(_ubahIgd, default)));
        Assert.Equal(404, Status(await sari.UbahHr.GetById(_ubahTetap, default)));

        Assert.Equal(200, Status(await sari.UbahHr.GetById(_ubahIcu, default)));
        Assert.Equal(200, Status(await sari.UbahHr.GetWorkflow(_ubahIcu, default)));
        Assert.Equal(200, Status(await sari.UbahHr.GetById(_ubahIso, default)));
    }

    // AC1 (Tukar Dinas): tampil bila shift pemohon ATAU tujuan dalam cakupan baca; IGD–IGD tidak.
    [Fact]
    public async Task Kepala_ICU_melihat_tukar_dinas_ICU_dan_lintas_ICU_IGD()
    {
        var sari = Sebagai(_uSari);

        var (_, daftar, _) = Baca<PagedResult<ShiftSwapListResponse>>(await DaftarTukar(sari));
        Assert.Equal(new[] { _tukarIcu, _tukarLintas }.OrderBy(x => x), daftar!.Items.Select(x => x.Id).OrderBy(x => x));

        var (_, ringkasan, _) = Baca<ShiftSwapSummaryResponse>(await sari.TukarHr.GetSummary(null, default));
        Assert.Equal(2, ringkasan!.TotalData);

        Assert.Equal(404, Status(await sari.TukarHr.GetById(_tukarIgd, default)));
        Assert.Equal(404, Status(await sari.TukarHr.GetWorkflow(_tukarIgd, default)));
        Assert.Equal(200, Status(await sari.TukarHr.GetById(_tukarLintas, default)));
        Assert.Equal(200, Status(await sari.TukarHr.GetWorkflow(_tukarLintas, default)));
    }

    // AC2 (Perubahan Shift): Terapkan/sinkronisasi pengajuan IGD → 403 tanpa perubahan; pengajuan ICU lolos penjaga.
    [Fact]
    public async Task Terapkan_dan_sinkronisasi_perubahan_shift_di_luar_cakupan_kelola_ditolak()
    {
        var sari = Sebagai(_uSari);

        var (s, _, pesan) = Baca<ScheduleChangeDetailResponse>(await sari.UbahHr.Apply(_ubahIgd, new ScheduleChangeApplyRequest { Notes = "Uji" }, default));
        Assert.Equal(403, s);
        Assert.Equal(TolakKelola, pesan);
        var sinkron = Baca<object>(await sari.UbahHr.Synchronize(_ubahIgd, default));
        Assert.Equal(403, sinkron.Status);
        Assert.Equal(TolakKelola, sinkron.Message);

        // Subunit ICU Isolasi terbaca, tetapi Terapkan butuh cakupan kelola (hanya ICU).
        Assert.Equal(403, Status(await sari.UbahHr.Apply(_ubahIso, new ScheduleChangeApplyRequest(), default)));
        Assert.Equal(403, Status(await sari.UbahHr.Synchronize(_ubahIso, default)));
        Assert.Equal(Ubah.Approved, KeadaanUbah(_ubahIso).Ubah.RequestStatus);

        var (igd, shiftIgd) = KeadaanUbah(_ubahIgd);
        Assert.Equal(Ubah.Approved, igd.RequestStatus);
        Assert.False(igd.IsAppliedToRoster);
        Assert.Equal(_pagi, shiftIgd.ShiftId);

        // Dalam cakupan: penjaga lolos — Terapkan berhasil, sinkronisasi sampai ke aturan workflow (belum ada workflow → 404).
        Assert.Equal(200, Status(await sari.UbahHr.Apply(_ubahIcu, new ScheduleChangeApplyRequest(), default)));
        var (icu, shiftIcu) = KeadaanUbah(_ubahIcu);
        Assert.Equal(Ubah.Applied, icu.RequestStatus);
        Assert.Equal(_malam, shiftIcu.ShiftId);
        var (sSinkron, _, pSinkron) = Baca<object>(await sari.UbahHr.Synchronize(_ubahIcu, default));
        Assert.Equal(404, sSinkron);
        Assert.Equal("Workflow perubahan jadwal tidak ditemukan.", pSinkron);

        Assert.Equal(404, Status(await sari.UbahHr.Apply(Guid.NewGuid(), new ScheduleChangeApplyRequest(), default)));
    }

    // AC2 (Tukar Dinas): lintas ICU–IGD tanpa cakupan IGD → 403 tanpa perubahan; IGD–IGD → 403; ICU–ICU lolos.
    [Fact]
    public async Task Terapkan_tukar_dinas_butuh_kedua_unit_dalam_cakupan_kelola()
    {
        var sari = Sebagai(_uSari);

        foreach (var id in new[] { _tukarLintas, _tukarIgd })
        {
            var (s, _, pesan) = Baca<ShiftSwapDetailResponse>(await sari.TukarHr.Apply(id, new ShiftSwapApplyRequest(), default));
            Assert.Equal(403, s);
            Assert.Equal(TolakKelola, pesan);
            Assert.Equal(403, Status(await sari.TukarHr.Synchronize(id, default)));

            var (t, pemohon, tujuan) = KeadaanTukar(id);
            Assert.Equal(Tukar.Approved, t.RequestStatus);
            Assert.False(t.IsAppliedToRoster);
            Assert.Equal(t.RequesterShiftDate, pemohon.ShiftDate);
            Assert.Equal(t.TargetShiftDate, tujuan.ShiftDate);
        }

        Assert.Equal(200, Status(await sari.TukarHr.Apply(_tukarIcu, new ShiftSwapApplyRequest(), default)));
        Assert.Equal(Tukar.Applied, KeadaanTukar(_tukarIcu).Tukar.RequestStatus);
        Assert.Equal(404, Status(await sari.TukarHr.Synchronize(_tukarIcu, default)));
    }

    // AC3: pemegang RosterPeriod : ManageAllUnits melihat dan menerapkan semua unit.
    [Fact]
    public async Task ManageAllUnits_melihat_dan_menerapkan_semua()
    {
        var hr = Sebagai(_uHr);

        var (_, ubah, _) = Baca<PagedResult<ScheduleChangeListResponse>>(await DaftarUbah(hr));
        Assert.Equal(5, ubah!.TotalData);
        Assert.Equal(5, Baca<ScheduleChangeSummaryResponse>(await hr.UbahHr.GetSummary(null, default)).Data!.TotalData);
        var (_, tukar, _) = Baca<PagedResult<ShiftSwapListResponse>>(await DaftarTukar(hr));
        Assert.Equal(3, tukar!.TotalData);
        Assert.Equal(200, Status(await hr.UbahHr.GetById(_ubahTetap, default)));
        Assert.Equal(200, Status(await hr.TukarHr.GetById(_tukarIgd, default)));

        Assert.Equal(200, Status(await hr.UbahHr.Apply(_ubahIgd, new ScheduleChangeApplyRequest(), default)));
        Assert.Equal(Ubah.Applied, KeadaanUbah(_ubahIgd).Ubah.RequestStatus);
        Assert.Equal(200, Status(await hr.TukarHr.Apply(_tukarLintas, new ShiftSwapApplyRequest(), default)));
        Assert.Equal(Tukar.Applied, KeadaanTukar(_tukarLintas).Tukar.RequestStatus);
    }

    // Gagal tertutup: pengguna tanpa penempatan berjabatan tidak melihat dan tidak menerapkan apa pun.
    [Fact]
    public async Task Pengguna_tanpa_cakupan_tidak_melihat_dan_tidak_menerapkan()
    {
        var budi = Sebagai(_uBudi);

        Assert.Equal(0, Baca<PagedResult<ScheduleChangeListResponse>>(await DaftarUbah(budi)).Data!.TotalData);
        Assert.Equal(0, Baca<PagedResult<ShiftSwapListResponse>>(await DaftarTukar(budi)).Data!.TotalData);
        Assert.Equal(0, Baca<ScheduleChangeSummaryResponse>(await budi.UbahHr.GetSummary(null, default)).Data!.TotalData);
        Assert.Equal(0, Baca<ShiftSwapSummaryResponse>(await budi.TukarHr.GetSummary(null, default)).Data!.TotalData);
        Assert.Equal(404, Status(await budi.UbahHr.GetById(_ubahIcu, default)));
        Assert.Equal(403, Status(await budi.UbahHr.Apply(_ubahIcu, new ScheduleChangeApplyRequest(), default)));
        Assert.Equal(403, Status(await budi.TukarHr.Apply(_tukarIcu, new ShiftSwapApplyRequest(), default)));
        Assert.Equal(Ubah.Approved, KeadaanUbah(_ubahIcu).Ubah.RequestStatus);
    }

    // AC4: method bersama yang dipakai ESS dan penerapan otomatis workflow tidak disaring cakupan unit.
    [Fact]
    public async Task Jalur_ESS_dan_penerapan_otomatis_workflow_tidak_berubah()
    {
        var budi = Sebagai(_uBudi);   // cakupan kosong — tetap melihat dan menerapkan lewat jalur bersama

        var milikCitra = await budi.Ubah.GetPagedAsync(_citra, null, null, null, null, null, "desc", 1, 25);
        Assert.Equal(_ubahIgd, Assert.Single(milikCitra.Items).Id);
        Assert.Equal(5, (await budi.Ubah.GetSummaryAsync(null)).TotalData);
        Assert.True((await budi.Ubah.GetByIdAsync(_ubahIgd)).Success);

        var tukarEko = await budi.Tukar.GetPagedAsync(_eko, "all", null, null, null, null, "desc", 1, 25);
        Assert.Equal(new[] { _tukarIgd, _tukarLintas }.OrderBy(x => x), tukarEko.Items.Select(x => x.Id).OrderBy(x => x));
        Assert.Equal(3, (await budi.Tukar.GetSummaryAsync(null)).TotalData);
        Assert.True((await budi.Tukar.GetByIdAsync(_tukarIgd)).Success);

        // ScheduleChange/ShiftSwapWorkflowLifecycleService memanggil ApplyAsync sesudah persetujuan engine.
        var ubah = await budi.Ubah.ApplyAsync(_ubahIgd, _uBudi, null);
        Assert.True(ubah.Success, ubah.Message);
        var tukar = await budi.Tukar.ApplyAsync(_tukarIgd, _uBudi, null);
        Assert.True(tukar.Success, tukar.Message);
    }
}
