using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using S = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.AttendanceStatus;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-357 — Dashboard Kehadiran (HRD-DEC-208, FR-PK-030/031, VR-PK-27); AT-HRD-PK-40, AT-HRD-PK-41.
// "Sekarang" = 10 Feb 2027 pukul 09.00 WIB. Hierarki: RS → Bidang Keperawatan → {ICU, HCU}; Farmasi di luar bidang.
public sealed class AttendanceDashboardTests : IDisposable
{
    private static DateOnly F(int hari) => new(2027, 2, hari);
    private static DateTime Wib(DateOnly d, int jam, int menit = 0) => new DateTime(d.Year, d.Month, d.Day, jam, menit, 0, DateTimeKind.Utc).AddHours(-7);
    private static readonly DateTime Sekarang = Wib(F(10), 9);

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _rs = Guid.NewGuid(), _bidang = Guid.NewGuid(), _icu = Guid.NewGuid(), _hcu = Guid.NewGuid(), _farmasi = Guid.NewGuid();
    private readonly Guid _uSari = Guid.NewGuid(), _uDodi = Guid.NewGuid(), _uAdmin = Guid.NewGuid(), _uRinaPerawat = Guid.NewGuid(), _uSuper = Guid.NewGuid();
    private readonly Dictionary<string, Guid> _wf = new();
    private readonly Guid _pagi = Guid.NewGuid(), _malam = Guid.NewGuid();

    private static readonly string[] Pegawai =
    {
        "Sari", "Dodi", "Admin", "Ani", "Bayu", "Tono", "Wati", "Yudi", "Zaki", "Lina", "Rina", "Edo", "Fani", "Gita", "Hadi", "Ika",
        "Andi", "Fara", "Budi", "Cici", "Joko", "Dokter", "Cadangan"
    };

    public AttendanceDashboardTests()
    {
        using var ctx = _db.CreateContext();
        var legal = Guid.NewGuid();
        MstOrganizationUnit Unit(Guid id, string kode, Guid? induk) => new() { Id = id, LegalEntityId = legal, UnitCode = kode, UnitName = kode, ParentOrganizationUnitId = induk };
        ctx.MstOrganizationUnits.AddRange(Unit(_rs, "RS", null), Unit(_bidang, "BIDKEP", _rs), Unit(_icu, "ICU", _bidang), Unit(_hcu, "HCU", _bidang), Unit(_farmasi, "FARMASI", _rs));
        ctx.MstShifts.AddRange(
            new MstShift { Id = _pagi, ShiftCode = "P", ShiftName = "Pagi", StartTime = new TimeOnly(7, 0), EndTime = new TimeOnly(14, 0), IsActive = true },
            new MstShift { Id = _malam, ShiftCode = "M", ShiftName = "Malam", StartTime = new TimeOnly(21, 0), EndTime = new TimeOnly(7, 0), IsActive = true });

        foreach (var nama in Pegawai)
        {
            var id = Guid.NewGuid();
            _wf[nama] = id;
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = "P-" + nama, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
        }

        ApplicationUser Akun(Guid id, Guid? profil, UserType tipe) => new() { Id = id, UserName = id.ToString(), UserCode = id.ToString("N")[..8], DisplayName = "Uji", UserType = tipe, WorkforceProfileId = profil, IsActive = true };
        ctx.Users.AddRange(Akun(_uSari, _wf["Sari"], UserType.Employee), Akun(_uDodi, _wf["Dodi"], UserType.Employee), Akun(_uAdmin, _wf["Admin"], UserType.Employee),
            Akun(_uRinaPerawat, _wf["Rina"], UserType.Employee), Akun(_uSuper, null, UserType.SuperAdmin));

        var mulai = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        WfpOrganizationAssignment Tempat(string nama, Guid unit, bool jabatan) => new()
        { WorkforceProfileId = _wf[nama], OrganizationUnitId = unit, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), IsManagerialAssignment = jabatan, IsPrimary = true, EffectiveStartDate = mulai, IsActive = true };
        ctx.WfpOrganizationAssignments.AddRange(Tempat("Sari", _icu, true), Tempat("Dodi", _bidang, true), Tempat("Rina", _icu, false));

        // Admin HRD: penanda AttendanceReport : ReadAllUnits lewat Akses Role.
        var dept = Guid.NewGuid();
        var pos = Guid.NewGuid();
        var controller = new SysControllerAccess { ModuleId = Guid.NewGuid(), ControllerName = "AttendanceReport", DisplayName = "Attendance Report" };
        var action = new SysActionAccess { ControllerAccessId = controller.Id, ActionName = "ReadAllUnits", DisplayName = "Read All", AccessType = "Read" };
        ctx.SysControllerAccesses.Add(controller);
        ctx.SysActionAccesses.Add(action);
        ctx.ApplicationUserOrganizations.Add(new ApplicationUserOrganization { UserId = _uAdmin, DepartmentId = dept, PositionId = pos, IsActive = true });
        ctx.SysAccessPolicies.Add(new SysAccessPolicy { DepartmentId = dept, PositionId = pos, ControllerAccessId = controller.Id, ActionAccessId = action.Id, IsAllowed = true, IsActive = true });

        HrdAttendanceDaily Harian(string nama, Guid unit, string status, int telat = 0, bool hadir = false) => new()
        {
            UserId = Guid.NewGuid(), WorkforceProfileId = _wf[nama], UserType = UserType.Employee, OrganizationUnitId = unit, AttendanceDate = F(10), AttendanceStatus = status,
            ShiftId = _pagi, ScheduledCheckInAt = Wib(F(10), 7), ScheduledCheckOutAt = Wib(F(10), 14), FirstCheckInAt = hadir ? Wib(F(10), 7, telat) : null,
            LateMinutes = telat, IsPresent = hadir, ProcessingStatus = "Processed", IsActive = true,
        };
        var ika = Harian("Ika", _icu, S.EarlyLeave, hadir: true);
        var joko = Harian("Joko", _icu, S.Present, hadir: true);
        ctx.HrdAttendanceDailies.AddRange(
            Harian("Ani", _icu, S.Present, hadir: true),
            Harian("Bayu", _icu, S.Present, hadir: true),
            Harian("Tono", _icu, S.Late, telat: 12, hadir: true),
            Harian("Wati", _icu, S.NotYetPresent),
            Harian("Yudi", _icu, S.NoScan),
            Harian("Zaki", _icu, S.IncompletePunch, telat: 5, hadir: true),
            Harian("Rina", _icu, S.NoScan),             // pegawai jadwal tetap yang sedang sakit: kategori Sakit menang
            Harian("Edo", _icu, S.Absent),               // Alpa data lama sebelum periode ditutup
            Harian("Fani", _icu, S.BusinessTrip),
            Harian("Gita", _icu, S.StudyAssignment),
            Harian("Hadi", _icu, S.RestDay),             // libur: tidak terjadwal
            ika,
            joko,                                        // izin pulang cepat masih diajukan: tetap Hadir
            Harian("Andi", _hcu, S.Late, telat: 20, hadir: true),
            Harian("Fara", _farmasi, S.Present, hadir: true));
        ctx.SaveChanges();

        var izinDisetujui = new HrdEarlyLeavePermission { AttendanceDailyId = ika.Id, WorkforceProfileId = _wf["Ika"], RequestedEarlyLeaveAt = Wib(F(10), 12), Reason = "Anak sakit di sekolah", Status = "Approved" };
        var izinDiajukan = new HrdEarlyLeavePermission { AttendanceDailyId = joko.Id, WorkforceProfileId = _wf["Joko"], RequestedEarlyLeaveAt = Wib(F(10), 12), Reason = "x", Status = "Submitted" };
        ctx.HrdEarlyLeavePermissions.AddRange(izinDisetujui, izinDiajukan);
        ctx.SaveChanges();
        ika.EarlyLeavePermissionId = izinDisetujui.Id;
        joko.EarlyLeavePermissionId = izinDiajukan.Id;

        // Cuti dan sakit: Lina cuti tahunan tanpa rekap; Rina sakit dengan diagnosis di alasan; cuti ditolak tidak dihitung.
        var sakit = new MstLeaveType { LeaveTypeCode = "CS", LeaveTypeName = "Cuti Sakit Dengan Surat Dokter", LeaveCategory = "Sick", IsActive = true };
        var tahunan = new MstLeaveType { LeaveTypeCode = "CT", LeaveTypeName = "Cuti Tahunan", LeaveCategory = "Annual", IsActive = true };
        ctx.Set<MstLeaveType>().AddRange(sakit, tahunan);
        ctx.Set<WfpLeaveRequest>().AddRange(
            new WfpLeaveRequest { RequestNumber = "LV-1", WorkforceProfileId = _wf["Rina"], OrganizationUnitId = _icu, LeaveTypeId = sakit.Id, StartDate = F(9), EndDate = F(11), RequestedDays = 3, Reason = "Demam berdarah dengue, diagnosis A90", LeaveRequestStatus = "Approved" },
            new WfpLeaveRequest { RequestNumber = "LV-2", WorkforceProfileId = _wf["Lina"], OrganizationUnitId = _icu, LeaveTypeId = tahunan.Id, StartDate = F(10), EndDate = F(10), RequestedDays = 1, Reason = "Urusan keluarga", LeaveRequestStatus = "Approved" },
            new WfpLeaveRequest { RequestNumber = "LV-3", WorkforceProfileId = _wf["Ani"], OrganizationUnitId = _icu, LeaveTypeId = tahunan.Id, StartDate = F(10), EndDate = F(10), RequestedDays = 1, Reason = "x", LeaveRequestStatus = "Rejected" });

        // Roster: ICU Feb terbit (dengan peringatan W6/W7), ICU Feb Draft, ICU Maret Draft, ICU April Draft (di luar jendela), HCU Maret Draft.
        var rosterIcu = Guid.NewGuid();
        var rosterIcuDraft = Guid.NewGuid();
        TrxRosterPeriod Roster(Guid id, Guid unit, string kode, string status, DateOnly awal, DateOnly akhir, string? ringkasan = null) => new()
        { Id = id, OrganizationUnitId = unit, RosterPeriodCode = kode, RosterPeriodName = kode, RosterStatus = status, PeriodStartDate = awal, PeriodEndDate = akhir, ValidationSummaryJson = ringkasan, IsActive = true };

        Guid Shift(Guid roster, string nama, Guid shift, DateOnly d, int jamMulai, int jamSelesai, Action<TrxShiftAssignment>? ubah = null)
        {
            var ra = Guid.NewGuid();
            ctx.TrxRosterAssignments.Add(new TrxRosterAssignment { Id = ra, RosterPeriodId = roster, WorkforceProfileId = _wf[nama], AssignmentStatus = "Published", IsActive = true });
            var mulaiShift = Wib(d, jamMulai);
            var selesai = jamSelesai > jamMulai ? Wib(d, jamSelesai) : Wib(d.AddDays(1), jamSelesai);
            var s = new TrxShiftAssignment { RosterAssignmentId = ra, WorkforceProfileId = _wf[nama], ShiftId = shift, ShiftDate = d, ScheduledStartAt = mulaiShift, ScheduledEndAt = selesai, AssignmentStatus = "Published", IsActive = true };
            ubah?.Invoke(s);
            ctx.TrxShiftAssignments.Add(s);
            return s.Id;
        }

        var jangkar = Shift(rosterIcu, "Ani", _pagi, F(10), 7, 14);                                       // berjalan pukul 09.00
        Shift(rosterIcu, "Bayu", _pagi, F(10), 7, 14, s => s.OriginalEmployeeId = _wf["Budi"]);          // pengganti hasil Ganti Petugas
        Shift(rosterIcu, "Tono", _malam, F(9), 21, 7);                                                   // malam 9→10 Feb, selesai 07.00
        Shift(rosterIcu, "Yudi", _malam, F(10), 21, 7);                                                  // malam 10→11 Feb
        Shift(rosterIcu, "Wati", _pagi, F(10), 7, 14, s => s.IsCancel = true);                           // dihapus
        Shift(rosterIcuDraft, "Cici", _pagi, F(10), 7, 14);                                              // roster Draft
        Shift(rosterIcu, "Joko", _pagi, F(10), 7, 14, s => s.IsOnCall = true);                           // shift siaga dihitung dari penugasan siaga

        var ringkasan = JsonSerializer.Serialize(new RosterValidationResultResponse
        {
            RosterPeriodId = rosterIcu,
            WarningConflicts = 4,
            ConflictItems = new List<RosterValidationConflictItemDto>
            {
                new() { ShiftAssignmentId = jangkar, ShiftDate = F(12), RuleCode = "W6", Severity = "WARNING", Message = "W6: Shift Pagi 12-02-2027 di ICU berisi 1 Perawat, kurang dari kuota 2 per shift." },
                new() { ShiftAssignmentId = jangkar, ShiftDate = F(15), RuleCode = "W7", Severity = "WARNING", Message = "W7: Shift Pagi 15-02-2027 di ICU hanya memiliki 0 pegawai dengan kompetensi BLS, kurang dari 1." },
                new() { ShiftAssignmentId = jangkar, ShiftDate = F(5), RuleCode = "W6", Severity = "WARNING", Message = "W6: lampau" },
                new() { ShiftAssignmentId = jangkar, WorkforceProfileId = _wf["Tono"], StaffName = "Tono", ShiftDate = F(12), RuleCode = "W1", Severity = "WARNING", Message = "W1: jeda" },
            }
        });
        ctx.TrxRosterPeriods.AddRange(
            Roster(rosterIcu, _icu, "R-ICU-FEB", "Published", F(1), F(28), ringkasan),
            Roster(rosterIcuDraft, _icu, "R-ICU-FEB-DRAFT", "Draft", F(1), F(28)),
            Roster(Guid.NewGuid(), _icu, "R-ICU-MAR", "Draft", new DateOnly(2027, 3, 1), new DateOnly(2027, 3, 31)),
            Roster(Guid.NewGuid(), _icu, "R-ICU-APR", "Draft", new DateOnly(2027, 4, 1), new DateOnly(2027, 4, 30)),
            Roster(Guid.NewGuid(), _icu, "R-ICU-JAN", "Draft", new DateOnly(2027, 1, 1), new DateOnly(2027, 1, 31)),
            Roster(Guid.NewGuid(), _hcu, "R-HCU-MAR", "Validation", new DateOnly(2027, 3, 1), new DateOnly(2027, 3, 31)));

        // Siaga: utama ICU 08.00–20.00 (terhitung), cadangan (tidak), utama dibatalkan (tidak).
        var tipe = new MstOnCallType { OnCallTypeCode = "OC", OnCallTypeName = "Siaga Dokter", IsActive = true };
        ctx.Set<MstOnCallType>().Add(tipe);
        TrxOnCallAssignment Siaga(string nama, string peran, string status) => new()
        { RosterPeriodId = rosterIcu, WorkforceProfileId = _wf[nama], OnCallTypeId = tipe.Id, UnitId = _icu, OnCallRole = peran, AssignmentStatus = status, StartAt = Wib(F(10), 8), EndAt = Wib(F(10), 20), IsActive = true };
        ctx.Set<TrxOnCallAssignment>().AddRange(Siaga("Dokter", "Primary", "Scheduled"), Siaga("Cadangan", "Backup", "Scheduled"), Siaga("Andi", "Primary", "Cancelled"));

        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private AttendanceDashboardService Svc(Guid user)
    {
        var ctx = _db.CreateContext();
        return new AttendanceDashboardService(ctx, new HrdUnitScopeService(ctx, Pelaku.Izin(ctx), Pelaku.Sebagai(user)));
    }

    private async Task<AttendanceDashboardResponse> Dashboard(Guid user, Guid? unit = null, DateOnly? tanggal = null, DateTime? sekarang = null)
    {
        var r = await Svc(user).GetDashboardAsync(new AttendanceDashboardQueryRequest { Date = tanggal, OrganizationUnitId = unit }, sekarang ?? Sekarang);
        Assert.True(r.Success, r.Message);
        return r.Data!;
    }

    private async Task<AttendanceDashboardDetailResponse> Rincian(Guid user, string kartu, string? metrik, Guid? unit = null, DateOnly? tanggal = null, DateTime? sekarang = null, int ukuran = 100)
    {
        var r = await Svc(user).GetCardDetailAsync(kartu, new AttendanceDashboardDetailQueryRequest { Date = tanggal, OrganizationUnitId = unit, Metric = metrik, PageSize = ukuran }, sekarang ?? Sekarang);
        Assert.True(r.Success, r.Message);
        return r.Data!;
    }

    private static Dictionary<string, int> Angka(AttendanceDashboardResponse d, string kartu) =>
        d.Cards.Single(x => x.CardCode == kartu).Metrics.ToDictionary(x => x.Metric, x => x.Count);

    private static IEnumerable<string> Nama(AttendanceDashboardDetailResponse r) => r.Items.Select(x => x.EmployeeName!);

    // AT-HRD-PK-40 + HRD-DEC-208 uji (a)(c): Sari membuka dashboard ICU pukul 09.00; setiap angka = jumlah baris rinciannya; jam hitung tampil.
    [Fact]
    public async Task AT_HRD_PK_40_angka_kartu_sama_dengan_rincian()
    {
        var d = await Dashboard(_uSari, _icu);
        Assert.Equal(F(10), d.Date);
        Assert.Equal(Sekarang, d.CalculatedAt);
        Assert.Equal(new[] { "attendance-today", "on-duty", "lateness", "absence", "incomplete-schedule" }, d.Cards.Select(x => x.CardCode));

        var total = 0;
        foreach (var kartu in d.Cards)
        {
            Assert.Equal(kartu.Metrics[0].Count, kartu.Count);
            foreach (var m in kartu.Metrics)
            {
                var r = await Rincian(_uSari, kartu.CardCode, m.Metric, _icu);
                Assert.True(m.Count == r.TotalData, $"{kartu.CardCode}/{m.Metric}: kartu {m.Count}, rincian {r.TotalData}");
                Assert.Equal(r.TotalData, r.Items.Count);
                Assert.Equal(Sekarang, r.CalculatedAt);
                total++;
            }
        }
        Assert.Equal(27, total);

        // Kosong = angka utama kartu.
        Assert.Equal("scheduled", (await Rincian(_uSari, "attendance-today", null, _icu)).Metric);
    }

    [Fact]
    public async Task Kehadiran_hari_ini_dan_ketidakhadiran()
    {
        var d = await Dashboard(_uSari);
        var hariIni = Angka(d, "attendance-today");
        Assert.Equal(13, hariIni["scheduled"]); // Hadi (libur) tidak terjadwal; Lina (cuti tanpa rekap) terjadwal
        Assert.Equal(3, hariIni["present"]);    // Ani, Bayu, Joko (izin belum disetujui)
        Assert.Equal(1, hariIni["late"]);
        Assert.Equal(1, hariIni["notYetPresent"]);
        Assert.Equal(2, hariIni["noScan"]);     // Yudi, Edo (Alpa data lama sebelum tutup)
        Assert.Equal(1, hariIni["leave"]);
        Assert.Equal(1, hariIni["sick"]);
        Assert.Equal(1, hariIni["permission"]);
        Assert.Equal(1, hariIni["businessTrip"]);
        Assert.Equal(1, hariIni["studyAssignment"]);

        Assert.Equal(new[] { "Ani", "Bayu", "Joko" }, Nama(await Rincian(_uSari, "attendance-today", "present")));
        Assert.Equal(new[] { "Ika" }, Nama(await Rincian(_uSari, "attendance-today", "permission")));
        Assert.Equal(new[] { "Rina" }, Nama(await Rincian(_uSari, "attendance-today", "sick")));

        var absen = Angka(d, "absence");
        Assert.Equal(new Dictionary<string, int>
        {
            ["total"] = 8, ["noScan"] = 2, ["incomplete"] = 1, ["leave"] = 1, ["sick"] = 1, ["permission"] = 1, ["businessTrip"] = 1, ["studyAssignment"] = 1, ["absent"] = 0
        }, absen);

        // Keterlambatan: menit sesudah toleransi, terlama di atas; Belum Lengkap yang terlambat ikut.
        var telat = await Rincian(_uSari, "lateness", "late");
        Assert.Equal(new[] { "Tono|12", "Zaki|5" }, telat.Items.Select(x => $"{x.EmployeeName}|{x.LateMinutes}"));
        Assert.Equal("Pagi", telat.Items[0].ShiftName);

        // HRD-DEC-208 butir 5: sakit hanya "Sakit", tanpa jenis cuti, alasan, atau diagnosis.
        var json = JsonSerializer.Serialize(await Rincian(_uSari, "absence", "total")) + JsonSerializer.Serialize(d);
        Assert.Contains("Sakit", json);
        foreach (var bocor in new[] { "Demam", "A90", "Surat Dokter", "keluarga", "Anak sakit" }) Assert.DoesNotContain(bocor, json);
        Assert.DoesNotContain(typeof(DashboardDetailRow).GetProperties(), p => p.Name.Contains("Reason") || p.Name.Contains("Attachment") || p.Name.Contains("Diagnos"));

        // Alpa hanya dari periode yang sudah ditutup.
        using (var ctx = _db.CreateContext())
        {
            ctx.HrdAttendancePeriods.Add(new HrdAttendancePeriod { PeriodCode = "ATP-2027-02-ICU", PeriodName = "Feb ICU", OrganizationUnitId = _icu, StartDate = F(1), EndDate = F(28), PeriodStatus = "Closed" });
            ctx.SaveChanges();
        }
        var tutup = await Dashboard(_uSari);
        Assert.Equal(1, Angka(tutup, "absence")["absent"]);
        Assert.Equal(1, Angka(tutup, "absence")["noScan"]);
        Assert.Equal(new[] { "Edo" }, Nama(await Rincian(_uSari, "absence", "absent")));
    }

    // HRD-DEC-208 uji (d): shift malam yang berjalan pukul 02.00 masuk Pegawai Bertugas; pengganti dan siaga utama ikut.
    [Fact]
    public async Task Pegawai_bertugas()
    {
        var pagi = await Rincian(_uSari, "on-duty", "total");
        Assert.Equal(new[] { "Ani|Shift", "Bayu|Petugas Pengganti", "Dokter|Siaga Utama" }, pagi.Items.Select(x => $"{x.EmployeeName}|{x.Category}"));
        Assert.Equal(new Dictionary<string, int> { ["total"] = 3, ["shift"] = 2, ["onCall"] = 1 }, Angka(await Dashboard(_uSari), "on-duty"));

        var dini = Wib(F(11), 2);
        var malam = await Rincian(_uSari, "on-duty", "shift", tanggal: F(11), sekarang: dini);
        Assert.Equal(new[] { "Yudi" }, Nama(malam));
        Assert.Equal(dini, (await Dashboard(_uSari, tanggal: F(11), sekarang: dini)).OnDutyReferenceAt);

        // Tanggal lain: jam dinding yang sama pada tanggal itu (10 Feb 02.00 → malam Tono 9→10 Feb).
        var lampau = await Dashboard(_uSari, tanggal: F(10), sekarang: dini);
        Assert.Equal(Wib(F(10), 2), lampau.OnDutyReferenceAt);
        Assert.Equal(new[] { "Tono" }, Nama(await Rincian(_uSari, "on-duty", "shift", tanggal: F(10), sekarang: dini)));
    }

    [Fact]
    public async Task Jadwal_belum_lengkap()
    {
        var angka = Angka(await Dashboard(_uSari), "incomplete-schedule");
        Assert.Equal(new Dictionary<string, int> { ["total"] = 4, ["unpublishedRoster"] = 2, ["quotaWarning"] = 1, ["qualificationWarning"] = 1 }, angka);

        var roster = await Rincian(_uSari, "incomplete-schedule", "unpublishedRoster");
        Assert.Equal(new[] { "R-ICU-FEB-DRAFT", "R-ICU-MAR" }, roster.Items.Select(x => x.RosterPeriodCode));

        var w6 = Assert.Single((await Rincian(_uSari, "incomplete-schedule", "quotaWarning")).Items);
        Assert.Equal(("W6", F(12), "Pagi", "R-ICU-FEB"), (w6.RuleCode, w6.Date, w6.ShiftName, w6.RosterPeriodCode));
        Assert.StartsWith("W6: Shift Pagi 12-02-2027", w6.Message);

        // Kepala Bidang ikut melihat roster HCU yang belum terbit.
        Assert.Equal(5, Angka(await Dashboard(_uDodi), "incomplete-schedule")["total"]);
    }

    // HRD-DEC-208 uji (b)(e), VR-PK-27, AT-HRD-PK-41 (lapis cakupan; lapis hak akses diuji di A52PermissionRegistryTests).
    [Fact]
    public async Task AT_HRD_PK_41_cakupan_unit()
    {
        // Kepala Unit tidak dapat memilih unit lain.
        foreach (var unit in new[] { _hcu, _farmasi, _bidang })
        {
            var tolak = await Svc(_uSari).GetDashboardAsync(new AttendanceDashboardQueryRequest { OrganizationUnitId = unit }, Sekarang);
            Assert.Equal((403, "Anda tidak berwenang melihat data unit ini"), (tolak.StatusCode, tolak.Message));
            var tolakRincian = await Svc(_uSari).GetCardDetailAsync("lateness", new AttendanceDashboardDetailQueryRequest { OrganizationUnitId = unit }, Sekarang);
            Assert.Equal(403, tolakRincian.StatusCode);
            Assert.Null(tolakRincian.Data);
        }

        // Pegawai biasa (tanpa penempatan berjabatan) ditolak, dengan atau tanpa unit.
        Assert.Equal(403, (await Svc(_uRinaPerawat).GetDashboardAsync(new AttendanceDashboardQueryRequest(), Sekarang)).StatusCode);
        Assert.Equal(403, (await Svc(_uRinaPerawat).GetDashboardAsync(new AttendanceDashboardQueryRequest { OrganizationUnitId = _icu }, Sekarang)).StatusCode);
        Assert.Equal(403, (await Svc(_uRinaPerawat).GetCardDetailAsync("attendance-today", new AttendanceDashboardDetailQueryRequest(), Sekarang)).StatusCode);

        // Sari: hanya ICU. Dodi: ICU + HCU. Admin HRD (ReadAllUnits) dan superadmin: semua, termasuk Farmasi.
        Assert.DoesNotContain("Andi", Nama(await Rincian(_uSari, "lateness", "late")));
        Assert.Equal(new[] { "Andi", "Tono", "Zaki" }, Nama(await Rincian(_uDodi, "lateness", "late")));
        Assert.Equal(new[] { "Andi" }, Nama(await Rincian(_uDodi, "lateness", "late", _hcu)));
        foreach (var user in new[] { _uAdmin, _uSuper })
        {
            Assert.Equal(15, Angka(await Dashboard(user), "attendance-today")["scheduled"]);
            Assert.Contains("Fara", Nama(await Rincian(user, "attendance-today", "present")));
        }
    }

    [Fact]
    public async Task Kode_kartu_metrik_dan_paginasi()
    {
        var tak = await Svc(_uSari).GetCardDetailAsync("payroll", new AttendanceDashboardDetailQueryRequest(), Sekarang);
        Assert.Equal(404, tak.StatusCode);
        var metrik = await Svc(_uSari).GetCardDetailAsync("on-duty", new AttendanceDashboardDetailQueryRequest { Metric = "sick" }, Sekarang);
        Assert.Equal(400, metrik.StatusCode);
        Assert.Contains("total, shift, onCall", metrik.Message);

        var hal2 = await Svc(_uSari).GetCardDetailAsync("attendance-today", new AttendanceDashboardDetailQueryRequest { Metric = "scheduled", PageNumber = 2, PageSize = 5 }, Sekarang);
        Assert.Equal((13, 3, 5), (hal2.Data!.TotalData, hal2.Data.TotalPage, hal2.Data.Items.Count));
        var semua = await Rincian(_uSari, "attendance-today", "scheduled");
        Assert.Equal(semua.Items.Skip(5).Take(5).Select(x => x.EmployeeName), hal2.Data.Items.Select(x => x.EmployeeName));

        // Tanggal default = hari ini (WIB): pukul 23.30 WIB 10 Feb = 16.30 UTC.
        Assert.Equal(F(10), (await Dashboard(_uSari, sekarang: Wib(F(10), 23, 30))).Date);
    }
}
