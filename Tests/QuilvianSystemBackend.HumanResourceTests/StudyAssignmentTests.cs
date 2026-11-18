using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using S = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.AttendanceStatus;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-355 — tugas belajar (HRD-DEC-201, state 30.4, VR-PK-18, VR-PK-19, 35.9.4 butir 1, INT-HR-102).
// Tanggal kerja Februari 2026 (lampau, shift sudah selesai). Shift Pagi 07.00–14.00.
public sealed class StudyAssignmentTests : IDisposable
{
    private static DateOnly F(int hari) => new(2026, 2, hari);
    private static DateTime Wib(DateOnly d, int jam, int menit = 0) => new DateTime(d.Year, d.Month, d.Day, jam, menit, 0, DateTimeKind.Utc).AddHours(-7);

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _unit = Guid.NewGuid(), _pagi = Guid.NewGuid(), _periode = Guid.NewGuid(), _admin = Guid.NewGuid();
    private readonly Dictionary<string, Guid> _wf = new();

    public StudyAssignmentTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _unit, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        ctx.MstShifts.Add(new MstShift { Id = _pagi, ShiftCode = "P", ShiftName = "Pagi", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(14, 0), IsActive = true });
        ctx.HrdAttendanceSettings.Add(new HrdAttendanceSetting { HospitalCode = "RSMMC", GracePeriodMinutes = 10, ScanWindowBeforeMinutes = 240, ScanWindowAfterMinutes = 480, IsActive = true });
        ctx.TrxRosterPeriods.Add(new TrxRosterPeriod
        {
            Id = _periode, OrganizationUnitId = _unit, RosterPeriodCode = "R", RosterPeriodName = "R", RosterStatus = "Published", IsPublished = true, VersionNumber = 1,
            PeriodStartDate = new DateOnly(2026, 1, 1), PeriodEndDate = new DateOnly(2026, 12, 31), IsActive = true,
        });
        ctx.Users.Add(new ApplicationUser { Id = _admin, UserName = "adminhrd", UserCode = "ADMINHRD", DisplayName = "Admin HRD", UserType = UserType.Employee, IsActive = true });
        ctx.SaveChanges();
        foreach (var nama in new[] { "Dewi", "Rina", "Tono" }) Pegawai(nama);
    }

    public void Dispose() => _db.Dispose();

    private void Pegawai(string nama)
    {
        using var ctx = _db.CreateContext();
        var id = Guid.NewGuid();
        ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = nama, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
        ctx.Users.Add(new ApplicationUser { Id = Guid.NewGuid(), UserName = nama, UserCode = nama.ToUpperInvariant().PadRight(8, 'X')[..8], DisplayName = nama, UserType = UserType.Employee, WorkforceProfileId = id, IsActive = true });
        ctx.WfpOrganizationAssignments.Add(new WfpOrganizationAssignment
        {
            WorkforceProfileId = id, OrganizationUnitId = _unit, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), AssignmentType = "Primary", IsPrimary = true,
            EffectiveStartDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsActive = true,
        });
        ctx.SaveChanges();
        _wf[nama] = id;
    }

    private void Shift(string nama, DateOnly d)
    {
        using var ctx = _db.CreateContext();
        var ra = Guid.NewGuid();
        ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = ra, RosterPeriodId = _periode, WorkforceProfileId = _wf[nama], AssignmentStatus = "Published", IsActive = true });
        ctx.TrxShiftAssignments.Add(new TrxShiftAssignment
        { RosterAssignmentId = ra, WorkforceProfileId = _wf[nama], ShiftId = _pagi, ShiftDate = d, ScheduledStartAt = Wib(d, 7), ScheduledEndAt = Wib(d, 14), AssignmentStatus = "Published", IsActive = true });
        ctx.SaveChanges();
    }

    private StudyAssignmentService Svc()
    {
        var ctx = _db.CreateContext();
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        return new StudyAssignmentService(ctx, Penjadwal.Buat(ctx, setelan), NullLogger<StudyAssignmentService>.Instance);
    }

    private async Task<HrdAttendanceDaily> Proses(string nama, DateOnly d)
    {
        var ctx = _db.CreateContext();
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        var svc = new AttendanceProcessingService(ctx, new AttendanceScheduleResolverService(ctx, setelan), setelan);
        var r = await svc.ProcessSingleAsync(new ProcessAttendanceSingleRequest { WorkforceProfileId = _wf[nama], WorkDate = d, ForceReprocess = true }, _admin);
        Assert.True(r.Success, r.Message);
        using var baca = _db.CreateContext();
        return baca.HrdAttendanceDailies.AsNoTracking().Include(x => x.Exceptions).Single(x => x.WorkforceProfileId == _wf[nama] && x.AttendanceDate == d);
    }

    private static CreateStudyAssignmentRequest Baru(Guid pegawai, DateOnly mulai, DateOnly selesai) => new()
    { WorkforceProfileId = pegawai, DecreeNumber = "SK/123/2026", InstitutionName = "Universitas Indonesia", ProgramName = "S1 Keperawatan", StartDate = mulai, EndDate = selesai };

    private async Task<StudyAssignmentResponse> Catat(string nama, DateOnly mulai, DateOnly selesai)
    {
        var r = await Svc().CreateAsync(Baru(_wf[nama], mulai, selesai), _admin);
        Assert.True(r.Success, r.Message);
        return r.Data!;
    }

    private List<HrdAttendanceSchedulerJob> Job()
    {
        using var ctx = _db.CreateContext();
        return ctx.Set<HrdAttendanceSchedulerJob>().AsNoTracking().OrderBy(x => x.CreateDateTime).ToList();
    }

    private static List<HrdAttendanceException> Aktif(HrdAttendanceDaily d) => d.Exceptions.Where(x => x.IsActive && !x.IsDelete).ToList();

    [Fact]
    public async Task Catat_validasi_dan_tumpang_tindih()
    {
        var a = await Catat("Dewi", F(1), F(28));
        Assert.Equal(("Active", "Aktif", "SK/123/2026", "Universitas Indonesia", "S1 Keperawatan", "Dewi", "Admin HRD"),
            (a.Status, a.StatusLabel, a.DecreeNumber, a.InstitutionName, a.ProgramName, a.EmployeeName, a.CreatedByName));
        Assert.Equal(0, a.ReprocessScheduled);
        Assert.Null(a.UpdatedAt);

        // VR-PK-19 tanggal; data SK wajib; pegawai tak dikenal.
        Assert.Equal((400, "Tanggal selesai harus sesudah tanggal mulai"), await Hasil(Svc().CreateAsync(Baru(_wf["Rina"], F(10), F(9)), _admin)));
        Assert.True((await Svc().CreateAsync(Baru(_wf["Rina"], F(10), F(10)), _admin)).Success);   // satu hari sah (EndDate ≥ StartDate)
        Assert.Equal(400, (await Svc().CreateAsync(new CreateStudyAssignmentRequest { WorkforceProfileId = _wf["Tono"], DecreeNumber = " ", InstitutionName = "UI", StartDate = F(1), EndDate = F(2) }, _admin)).StatusCode);
        Assert.Equal(404, (await Svc().CreateAsync(Baru(Guid.NewGuid(), F(1), F(2)), _admin)).StatusCode);

        // VR-PK-18: tumpang tindih dengan yang aktif ditolak; bersebelahan sah; sesudah dibatalkan sah.
        Assert.Equal((422, "Pegawai sudah punya tugas belajar aktif pada rentang ini"), await Hasil(Svc().CreateAsync(Baru(_wf["Dewi"], F(28), new DateOnly(2026, 3, 31)), _admin)));
        Assert.True((await Svc().CreateAsync(Baru(_wf["Dewi"], new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31)), _admin)).Success);
        Assert.Equal(200, (await Svc().CancelAsync(a.Id, new CancelStudyAssignmentRequest { CancelReason = "SK dicabut" }, _admin)).StatusCode);
        Assert.True((await Svc().CreateAsync(Baru(_wf["Dewi"], F(5), F(20)), _admin)).Success);
    }

    // HRD-DEC-201 uji (d), butir 5, state 30.4.
    [Fact]
    public async Task Ubah_dan_batal_wajib_alasan()
    {
        var a = await Catat("Dewi", F(1), F(28));
        var b = await Catat("Dewi", new DateOnly(2026, 3, 1), new DateOnly(2026, 3, 31));
        UpdateStudyAssignmentRequest Ubah(DateOnly selesai, string? alasan) => new()
        { DecreeNumber = "SK/123/2026", InstitutionName = "Universitas Indonesia", ProgramName = "S1 Keperawatan", StartDate = F(1), EndDate = selesai, ChangeReason = alasan };

        Assert.Equal((400, "Alasan perubahan wajib diisi"), await Hasil(Svc().UpdateAsync(a.Id, Ubah(F(14), null), _admin)));
        Assert.Equal((400, "Alasan perubahan wajib diisi"), await Hasil(Svc().UpdateAsync(a.Id, Ubah(F(14), "  "), _admin)));
        Assert.Equal(422, (await Svc().UpdateAsync(a.Id, Ubah(new DateOnly(2026, 3, 5), "Diperpanjang"), _admin)).StatusCode);   // menabrak b
        Assert.Equal(400, (await Svc().UpdateAsync(a.Id, new UpdateStudyAssignmentRequest { DecreeNumber = "x", InstitutionName = "y", StartDate = F(10), EndDate = F(9), ChangeReason = "z" }, _admin)).StatusCode);

        var ubah = await Svc().UpdateAsync(a.Id, Ubah(F(14), "Kembali lebih cepat"), _admin);
        Assert.True(ubah.Success, ubah.Message);
        Assert.Equal((F(14), "Kembali lebih cepat", "Admin HRD", _admin), (ubah.Data!.EndDate, ubah.Data.LastChangeReason, ubah.Data.UpdatedByName, ubah.Data.UpdatedBy!.Value));
        Assert.NotNull(ubah.Data.UpdatedAt);

        Assert.Equal((400, "Alasan perubahan wajib diisi"), await Hasil(Svc().CancelAsync(b.Id, new CancelStudyAssignmentRequest(), _admin)));
        var batal = await Svc().CancelAsync(b.Id, new CancelStudyAssignmentRequest { CancelReason = "SK dicabut" }, _admin);
        Assert.Equal(("Cancelled", "Dibatalkan", "SK dicabut"), (batal.Data!.Status, batal.Data.StatusLabel, batal.Data.LastChangeReason));
        Assert.Equal(409, (await Svc().CancelAsync(b.Id, new CancelStudyAssignmentRequest { CancelReason = "x" }, _admin)).StatusCode);
        Assert.Equal(409, (await Svc().UpdateAsync(b.Id, Ubah(F(14), "x"), _admin)).StatusCode);
        Assert.Equal(404, (await Svc().CancelAsync(Guid.NewGuid(), new CancelStudyAssignmentRequest { CancelReason = "x" }, _admin)).StatusCode);
    }

    [Fact]
    public async Task Daftar_dan_filter()
    {
        var a = await Catat("Dewi", F(1), F(28));
        await Catat("Rina", F(10), F(12));
        var c = await Catat("Tono", F(1), F(3));
        await Svc().CancelAsync(c.Id, new CancelStudyAssignmentRequest { CancelReason = "x" }, _admin);

        async Task<List<string>> Nama(StudyAssignmentQueryRequest q) => (await Svc().ListAsync(q)).Data!.Items.Select(x => x.EmployeeName!).OrderBy(x => x).ToList();
        Assert.Equal(new[] { "Dewi", "Rina", "Tono" }, await Nama(new StudyAssignmentQueryRequest()));
        Assert.Equal(new[] { "Dewi", "Rina" }, await Nama(new StudyAssignmentQueryRequest { ActiveOn = F(11) }));
        Assert.Equal(new[] { "Dewi" }, await Nama(new StudyAssignmentQueryRequest { ActiveOn = F(2) }));   // Tono dibatalkan
        Assert.Equal(new[] { "Tono" }, await Nama(new StudyAssignmentQueryRequest { Status = "Cancelled" }));
        Assert.Equal(new[] { "Dewi" }, await Nama(new StudyAssignmentQueryRequest { WorkforceProfileId = _wf["Dewi"] }));
        var hal = (await Svc().ListAsync(new StudyAssignmentQueryRequest { PageSize = 2, PageNumber = 2 })).Data!;
        Assert.Equal((3, 2, 1), (hal.TotalData, hal.TotalPage, hal.Items.Count));
        Assert.Equal(a.Id, (await Svc().GetAsync(a.Id)).Data!.Id);
        Assert.Equal(404, (await Svc().GetAsync(Guid.NewGuid())).StatusCode);
    }

    // FR-PK-027 / HRD-DEC-201 uji (b)(c): hari dalam masa studi "Tugas Belajar", bukan Alpa/Tidak Ada Scan, tanpa saldo cuti.
    // INT-HR-102: catat/ubah/batal menjadwalkan hitung ulang rekap yang sudah ada.
    [Fact]
    public async Task Status_harian_tugas_belajar_dan_hitung_ulang()
    {
        // Dewi dijadwalkan 4 Feb sebelum SK tercatat dan tidak datang → Tidak Ada Scan.
        Shift("Dewi", F(4));
        var sebelum = await Proses("Dewi", F(4));
        Assert.Equal(S.NoScan, sebelum.AttendanceStatus);
        Assert.NotEmpty(Aktif(sebelum));

        var sk = await Catat("Dewi", F(1), F(28));
        Assert.Equal(1, sk.ReprocessScheduled);
        var job = Assert.Single(Job());
        Assert.Equal((true, _wf["Dewi"], F(4), F(4)), (job.ForceReprocess, job.WorkforceProfileId!.Value, job.StartDate, job.EndDate));
        Assert.StartsWith($"ATT-STUDY-{sk.Id:N}-", job.CorrelationId);

        var selama = await Proses("Dewi", F(4));
        Assert.Equal((S.StudyAssignment, false, 0, "Processed"), (selama.AttendanceStatus, selama.IsAbsent, selama.LateMinutes, selama.ProcessingStatus));
        Assert.Empty(Aktif(selama));
        Assert.True(selama.IsPayrollEligible);
        Assert.Contains("Tugas belajar", selama.ProcessingMessage);

        // Tanggal tanpa shift (B8 mencegah penjadwalan) tetap "Tugas Belajar", bukan galat jadwal.
        var tanpaShift = await Proses("Dewi", F(5));
        Assert.Equal((S.StudyAssignment, "Processed"), (tanpaShift.AttendanceStatus, tanpaShift.ProcessingStatus));
        Assert.Empty(Aktif(tanpaShift));

        // Datang dan terlambat pun tetap "Tugas Belajar" tanpa menit terlambat.
        using (var ctx = _db.CreateContext())
        {
            ctx.HrdAttendanceRawLogs.Add(new HrdAttendanceRawLog { WorkforceProfileId = _wf["Dewi"], EventAt = Wib(F(4), 7, 50), EventType = "CheckIn", SourceType = "Device", ProcessingStatus = "Matched" });
            ctx.SaveChanges();
        }
        Assert.Equal((S.StudyAssignment, 0), ((await Proses("Dewi", F(4))).AttendanceStatus, (await Proses("Dewi", F(4))).LateMinutes));

        // Saldo cuti tidak disentuh.
        using (var ctx = _db.CreateContext())
        {
            Assert.Empty(ctx.WfpLeaveBalances);
            Assert.Empty(ctx.TrxLeaveBalanceTransactions);
            Assert.Empty(ctx.TrxLeaveBalanceLedgers);
        }

        // Kembali lebih cepat (selesai 3 Feb): rekap 4 dan 5 Feb dijadwalkan ulang dan kembali dihitung biasa.
        var ubah = await Svc().UpdateAsync(sk.Id, new UpdateStudyAssignmentRequest
        { DecreeNumber = sk.DecreeNumber, InstitutionName = sk.InstitutionName, StartDate = F(1), EndDate = F(3), ChangeReason = "Kembali lebih cepat" }, _admin);
        Assert.Equal(2, ubah.Data!.ReprocessScheduled);
        var keluar = await Proses("Dewi", F(4));
        Assert.Equal((S.IncompletePunch, 50), (keluar.AttendanceStatus, keluar.LateMinutes));   // satu scan 07.50 sesudah shift selesai

        // Batal: rekap 4 Feb di luar rentang 1–3 Feb → tidak dijadwalkan; tidak ada rekap dalam rentang.
        var batal = await Svc().CancelAsync(sk.Id, new CancelStudyAssignmentRequest { CancelReason = "SK dicabut" }, _admin);
        Assert.Equal(0, batal.Data!.ReprocessScheduled);

        // Rekap terkunci tidak dijadwalkan.
        Shift("Rina", F(6));
        await Proses("Rina", F(6));
        using (var ctx = _db.CreateContext())
        {
            ctx.HrdAttendanceDailies.Single(x => x.WorkforceProfileId == _wf["Rina"]).IsLocked = true;
            ctx.SaveChanges();
        }
        Assert.Equal(0, (await Catat("Rina", F(6), F(6))).ReprocessScheduled);
    }

    private static async Task<(int, string)> Hasil<T>(Task<StudyAssignmentServiceResult<T>> t)
    {
        var r = await t;
        return (r.StatusCode, r.Message);
    }
}
