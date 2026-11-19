using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-353 — Dispensasi: pengajuan, catat atas nama, keputusan (HRD-DEC-200, HRD-ASM-76, state 30.3, VR-PK-13 s.d. 17);
// AT-HRD-PK-33, AT-HRD-PK-34. "Sekarang" = 5 Feb 2027 pukul 10.00 WIB. Hierarki: RS → Bidang Keperawatan → {ICU, HCU}.
// Atasan langsung: Budi → Sari (Kepala Unit ICU), Sari → Dodi (Kepala Bidang), Andi (HCU) → Hani; Tono hanya punya atasan
// tanpa hak setuju; Rina tanpa atasan (atasan = diri sendiri sudah dilarang CK_WfpManagerAssignment_NotSelf).
public sealed class AttendanceDispensationTests : IDisposable
{
    private static DateOnly F(int hari) => new(2027, 2, hari);
    private static DateTime Wib(DateOnly d, int jam, int menit = 0) => new DateTime(d.Year, d.Month, d.Day, jam, menit, 0, DateTimeKind.Utc).AddHours(-7);
    private static readonly DateTime Sekarang = Wib(F(5), 10);

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _rs = Guid.NewGuid(), _bidang = Guid.NewGuid(), _icu = Guid.NewGuid(), _hcu = Guid.NewGuid();
    private readonly Dictionary<string, Guid> _wf = new();
    private readonly Dictionary<string, Guid> _u = new();
    private readonly PemberitahuanTiruan _lonceng = new();

    public AttendanceDispensationTests()
    {
        using var ctx = _db.CreateContext();
        var legal = Guid.NewGuid();
        MstOrganizationUnit Unit(Guid id, string kode, Guid? induk) => new() { Id = id, LegalEntityId = legal, UnitCode = kode, UnitName = kode, ParentOrganizationUnitId = induk };
        ctx.MstOrganizationUnits.AddRange(Unit(_rs, "RS", null), Unit(_bidang, "BIDKEP", _rs), Unit(_icu, "ICU", _bidang), Unit(_hcu, "HCU", _bidang));

        foreach (var nama in new[] { "Sari", "Dodi", "Budi", "Tono", "Andi", "Hani", "Rina", "Admin" })
        {
            _wf[nama] = Guid.NewGuid();
            _u[nama] = Guid.NewGuid();
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = _wf[nama], ProfileCode = "P-" + nama, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
            ctx.Users.Add(new ApplicationUser { Id = _u[nama], UserName = nama, UserCode = _u[nama].ToString("N")[..8], DisplayName = nama, UserType = UserType.Employee, WorkforceProfileId = _wf[nama], IsActive = true });
        }

        var mulai = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        WfpOrganizationAssignment Tempat(string nama, Guid unit, bool jabatan) => new()
        { WorkforceProfileId = _wf[nama], OrganizationUnitId = unit, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(), IsManagerialAssignment = jabatan, IsPrimary = true, EffectiveStartDate = mulai, IsActive = true };
        ctx.WfpOrganizationAssignments.AddRange(
            Tempat("Sari", _icu, true), Tempat("Dodi", _bidang, true), Tempat("Budi", _icu, false), Tempat("Tono", _icu, false),
            Tempat("Andi", _hcu, false), Tempat("Hani", _hcu, true), Tempat("Rina", _icu, false));

        WfpManagerAssignment Atasan(string pegawai, string atasan, bool bolehSetuju = true) => new()
        { WorkforceProfileId = _wf[pegawai], ManagerWorkforceProfileId = _wf[atasan], ManagerType = "Direct", EffectiveStartDate = mulai, IsPrimaryManager = true, CanApproveRequests = bolehSetuju, IsActive = true };
        ctx.WfpManagerAssignments.AddRange(Atasan("Budi", "Sari"), Atasan("Sari", "Dodi"), Atasan("Andi", "Hani"), Atasan("Tono", "Dodi", bolehSetuju: false));

        // Admin HRD: penanda AttendanceReport : ReadAllUnits (membaca dispensasi semua unit).
        var dept = Guid.NewGuid();
        var pos = Guid.NewGuid();
        var controller = new SysControllerAccess { ModuleId = Guid.NewGuid(), ControllerName = "AttendanceReport", DisplayName = "Attendance Report" };
        var action = new SysActionAccess { ControllerAccessId = controller.Id, ActionName = "ReadAllUnits", DisplayName = "Read All", AccessType = "Read" };
        ctx.SysControllerAccesses.Add(controller);
        ctx.SysActionAccesses.Add(action);
        ctx.ApplicationUserOrganizations.Add(new ApplicationUserOrganization { UserId = _u["Admin"], DepartmentId = dept, PositionId = pos, IsActive = true });
        ctx.SysAccessPolicies.Add(new SysAccessPolicy { DepartmentId = dept, PositionId = pos, ControllerAccessId = controller.Id, ActionAccessId = action.Id, IsAllowed = true, IsActive = true });

        ctx.HrdAttendanceSettings.Add(new HrdAttendanceSetting { HospitalCode = "RSMMC", GracePeriodMinutes = 10, ScanWindowBeforeMinutes = 240, ScanWindowAfterMinutes = 480, MaxBackdatedCorrectionDays = 3, IsActive = true });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private AttendanceDispensationService Svc(string sebagai, PemberitahuanTiruan? lonceng = null)
    {
        var ctx = _db.CreateContext();
        var scope = new HrdUnitScopeService(ctx, Pelaku.Izin(ctx), Pelaku.Sebagai(_u[sebagai]));
        var setelan = new AttendanceSettingService(ctx, NullLogger<AttendanceSettingService>.Instance);
        return new AttendanceDispensationService(ctx, scope, setelan, lonceng ?? _lonceng, Penjadwal.Buat(ctx, setelan), NullLogger<AttendanceDispensationService>.Instance);
    }

    private static CreateMyDispensationRequest Pengajuan(DateOnly d, int jamMulai = 7, int menitMulai = 0, int jamSelesai = 7, int menitSelesai = 40, string jenis = "LateArrival", string alasan = "Mengantar rujukan pasien") =>
        new() { AttendanceDate = d, DispensationType = jenis, StartAt = Wib(d, jamMulai, menitMulai), EndAt = Wib(d, jamSelesai, menitSelesai), Reason = alasan };

    private static CreateDispensationRequest AtasNama(Guid pegawai, DateOnly d) =>
        new() { WorkforceProfileId = pegawai, AttendanceDate = d, DispensationType = "LateArrival", StartAt = Wib(d, 7), EndAt = Wib(d, 7, 40), Reason = "Mengantar rujukan pasien" };

    private async Task<DispensationResponse> Ajukan(string siapa, CreateMyDispensationRequest req)
    {
        var r = await Svc(siapa).CreateMineAsync(_u[siapa], req, Sekarang);
        Assert.True(r.Success, r.Message);
        return r.Data!;
    }

    // HRD-DEC-200 contoh: Budi mengajukan 07.00–07.40; penyetuju ditetapkan server (Sari); Sari menyetujui; Budi diberi tahu.
    [Fact]
    public async Task Pengajuan_sendiri_penyetuju_dari_server_lalu_disetujui()
    {
        var d = await Ajukan("Budi", Pengajuan(F(3)));
        Assert.Equal(("Submitted", "Diajukan", false, _wf["Sari"], "Sari", "ICU"), (d.Status, d.StatusLabel, d.IsOnBehalf, d.ApproverWorkforceProfileId!.Value, d.ApproverName, d.OrganizationUnitName));
        Assert.Equal((Wib(F(3), 7), Wib(F(3), 7, 40)), (d.StartAt, d.EndAt));
        Assert.True(d.CanCancel);
        Assert.False(d.CanDecide);
        Assert.DoesNotContain(typeof(CreateMyDispensationRequest).GetProperties(), p => p.Name.Contains("Approver"));
        Assert.DoesNotContain(typeof(CreateDispensationRequest).GetProperties(), p => p.Name.Contains("Approver"));

        var sari = await Svc("Sari").GetAsync(_u["Sari"], d.Id);
        Assert.True(sari.Data!.CanDecide);

        var setuju = await Svc("Sari").ApproveAsync(_u["Sari"], d.Id, new DispensationDecisionRequest { RowVersion = d.RowVersion });
        Assert.True(setuju.Success, setuju.Message);
        Assert.Equal(("Approved", _u["Sari"]), (setuju.Data!.Status, setuju.Data.DecidedByUserId!.Value));
        Assert.NotEqual(d.RowVersion, setuju.Data.RowVersion);
        Assert.Equal(new[] { (_u["Budi"], "Dispensasi Anda disetujui") }, _lonceng.Terkirim);

        // Log mesin tidak disentuh (INV-A52-07).
        using var ctx = _db.CreateContext();
        Assert.Empty(ctx.HrdAttendanceRawLogs);
    }

    // AT-HRD-PK-33 + VR-PK-17: Kepala Unit tidak menyetujui dispensasinya sendiri; atasannya (Dodi) yang memutuskan.
    [Fact]
    public async Task AT_HRD_PK_33_kepala_unit_tidak_menyetujui_dispensasinya_sendiri()
    {
        var d = await Ajukan("Sari", Pengajuan(F(4)));
        Assert.Equal(_wf["Dodi"], d.ApproverWorkforceProfileId);

        foreach (var siapa in new[] { "Sari", "Budi", "Hani", "Admin" })
        {
            var tolak = await Svc(siapa).ApproveAsync(_u[siapa], d.Id, new DispensationDecisionRequest { RowVersion = d.RowVersion });
            Assert.Equal((403, "Anda tidak berwenang memutuskan dispensasi ini"), (tolak.StatusCode, tolak.Message));
            var tolak2 = await Svc(siapa).RejectAsync(_u[siapa], d.Id, new RejectDispensationRequest { RowVersion = d.RowVersion, RejectionReason = "x" });
            Assert.Equal(403, tolak2.StatusCode);
        }

        using (var ctx = _db.CreateContext()) Assert.Equal("Submitted", ctx.HrdAttendanceDispensations.Single().Status);

        var dodi = await Svc("Dodi").ApproveAsync(_u["Dodi"], d.Id, new DispensationDecisionRequest { RowVersion = d.RowVersion });
        Assert.True(dodi.Success, dodi.Message);

        // Pegawai tanpa atasan langsung → VR-PK-16.
        var rina = await Svc("Rina").CreateMineAsync(_u["Rina"], Pengajuan(F(4)), Sekarang);
        Assert.Equal((422, "Atasan langsung Anda belum ditetapkan. Hubungi HRD"), (rina.StatusCode, rina.Message));
    }

    // AT-HRD-PK-34 + VR-PK-14: batas mundur dari setelan (3 hari); hari ke-3 boleh, hari ke-5 ditolak 422.
    [Fact]
    public async Task AT_HRD_PK_34_batas_mundur_dari_setelan()
    {
        var lima = await Svc("Budi").CreateMineAsync(_u["Budi"], Pengajuan(F(5).AddDays(-5)), Sekarang);
        Assert.Equal((422, "Dispensasi hanya bisa diajukan sampai 3 hari ke belakang"), (lima.StatusCode, lima.Message));
        var empat = await Svc("Budi").CreateMineAsync(_u["Budi"], Pengajuan(F(1)), Sekarang);
        Assert.Equal(422, empat.StatusCode);
        Assert.True((await Svc("Budi").CreateMineAsync(_u["Budi"], Pengajuan(F(2)), Sekarang)).Success);

        using (var ctx = _db.CreateContext())
        {
            ctx.HrdAttendanceSettings.Single().MaxBackdatedCorrectionDays = 5;
            ctx.SaveChanges();
        }
        Assert.True((await Svc("Budi").CreateMineAsync(_u["Budi"], Pengajuan(F(5).AddDays(-5)), Sekarang)).Success);
        Assert.Equal(5, (await Svc("Budi").GetMetadataAsync()).MaxBackdatedDays);
    }

    [Fact]
    public async Task Validasi_pengajuan()
    {
        async Task<(int, string)> Coba(CreateMyDispensationRequest req, string siapa = "Budi")
        {
            var r = await Svc(siapa).CreateMineAsync(_u[siapa], req, Sekarang);
            return (r.StatusCode, r.Message);
        }

        Assert.Equal((400, "Jam selesai harus sesudah jam mulai"), await Coba(Pengajuan(F(3), 7, 40, 7, 0)));       // VR-PK-13
        Assert.Equal((400, "Jam selesai harus sesudah jam mulai"), await Coba(Pengajuan(F(3), 7, 0, 7, 0)));
        Assert.Equal((400, "Alasan dispensasi wajib diisi."), await Coba(Pengajuan(F(3), alasan: "   ")));            // HRD-DEC-200 uji (d)
        Assert.Equal(400, (await Coba(Pengajuan(F(3), jenis: "FullDay"))).Item1);                                      // butir 5: bukan sehari penuh
        Assert.Equal(400, (await Coba(new CreateMyDispensationRequest { AttendanceDate = F(3), DispensationType = "LateArrival", StartAt = Wib(F(1), 7), EndAt = Wib(F(1), 8), Reason = "x" })).Item1);
        Assert.Equal((422, "Atasan langsung Anda belum ditetapkan. Hubungi HRD"), await Coba(Pengajuan(F(3)), "Tono")); // VR-PK-16 (atasan tanpa hak setuju)

        // Shift malam: pulang lebih awal 05.00–07.00 hari berikutnya sah; jenis tidak peka huruf.
        var malam = await Svc("Budi").CreateMineAsync(_u["Budi"], new CreateMyDispensationRequest { AttendanceDate = F(3), DispensationType = "earlydeparture", StartAt = Wib(F(4), 5), EndAt = Wib(F(4), 7), Reason = "Anak sakit" }, Sekarang);
        Assert.True(malam.Success, malam.Message);
        Assert.Equal(("EarlyDeparture", "Pulang lebih awal"), (malam.Data!.DispensationType, malam.Data.DispensationTypeLabel));

        // Jam tanpa zona dibaca sebagai WIB.
        var tanpaZona = await Svc("Budi").CreateMineAsync(_u["Budi"], new CreateMyDispensationRequest { AttendanceDate = F(4), DispensationType = "LateArrival", StartAt = new DateTime(2027, 2, 4, 7, 0, 0), EndAt = new DateTime(2027, 2, 4, 7, 30, 0), Reason = "x" }, Sekarang);
        Assert.Equal(Wib(F(4), 7), tanpaZona.Data!.StartAt);

        // Rentang bertumpuk dengan dispensasi aktif sejenis → 409; jenis lain pada jam yang sama boleh (pemrosesan memisahkan jenis).
        Assert.Equal(409, (await Coba(Pengajuan(F(4), 7, 15, 7, 45))).Item1);
        Assert.Equal(200, (await Coba(Pengajuan(F(4), 7, 15, 7, 45, jenis: "EarlyDeparture"))).Item1);
    }

    // VR-PK-15: periode ditutup → 403, untuk pengajuan maupun keputusan yang tertunda.
    [Fact]
    public async Task Periode_ditutup_menolak_pengajuan_dan_keputusan()
    {
        var tertunda = await Ajukan("Budi", Pengajuan(F(3)));
        using (var ctx = _db.CreateContext())
        {
            ctx.HrdAttendancePeriods.Add(new HrdAttendancePeriod { PeriodCode = "ATP-2027-02-ICU", PeriodName = "Feb ICU", OrganizationUnitId = _icu, StartDate = F(1), EndDate = F(28), PeriodStatus = "Closed" });
            ctx.SaveChanges();
        }

        var baru = await Svc("Budi").CreateMineAsync(_u["Budi"], Pengajuan(F(4)), Sekarang);
        Assert.Equal((403, "Periode kehadiran Februari 2027 sudah ditutup"), (baru.StatusCode, baru.Message));
        var putus = await Svc("Sari").ApproveAsync(_u["Sari"], tertunda.Id, new DispensationDecisionRequest { RowVersion = tertunda.RowVersion });
        Assert.Equal(403, putus.StatusCode);
        Assert.Equal(403, (await Svc("Budi").CancelMineAsync(_u["Budi"], tertunda.Id, new DispensationDecisionRequest { RowVersion = tertunda.RowVersion })).StatusCode);

        // Periode unit lain tidak berlaku untuk HCU.
        Assert.True((await Svc("Andi").CreateMineAsync(_u["Andi"], Pengajuan(F(4)), Sekarang)).Success);
    }

    // Matriks 30.2: Kepala Unit mencatat atas nama pegawai unitnya; unit lain dan Kepala Bidang 403.
    [Fact]
    public async Task Catat_atas_nama_hanya_pegawai_unit_sendiri()
    {
        var r = await Svc("Sari").CreateOnBehalfAsync(_u["Sari"], AtasNama(_wf["Budi"], F(3)), Sekarang);
        Assert.True(r.Success, r.Message);
        Assert.Equal((true, _u["Sari"], _wf["Sari"]), (r.Data!.IsOnBehalf, r.Data.RequestedByUserId, r.Data.ApproverWorkforceProfileId!.Value));

        var hcu = await Svc("Sari").CreateOnBehalfAsync(_u["Sari"], AtasNama(_wf["Andi"], F(3)), Sekarang);
        Assert.Equal((403, "Anda tidak berwenang mencatat dispensasi untuk pegawai unit ini"), (hcu.StatusCode, hcu.Message));
        Assert.Equal(403, (await Svc("Dodi").CreateOnBehalfAsync(_u["Dodi"], AtasNama(_wf["Budi"], F(4)), Sekarang)).StatusCode);
        Assert.Equal(403, (await Svc("Budi").CreateOnBehalfAsync(_u["Budi"], AtasNama(_wf["Tono"], F(4)), Sekarang)).StatusCode);

        var tanpaAtasan = await Svc("Sari").CreateOnBehalfAsync(_u["Sari"], AtasNama(_wf["Tono"], F(3)), Sekarang);
        Assert.Equal((422, "Atasan langsung pegawai ini belum ditetapkan. Hubungi HRD"), (tanpaAtasan.StatusCode, tanpaAtasan.Message));
        Assert.Equal(404, (await Svc("Sari").CreateOnBehalfAsync(_u["Sari"], AtasNama(Guid.NewGuid(), F(3)), Sekarang)).StatusCode);

        // Pegawai tidak dapat membatalkan dispensasi yang dicatat Kepala Unit; Sari tetap dapat menolaknya. Pengaju = pemutus → tanpa lonceng.
        var budiBatal = await Svc("Budi").CancelMineAsync(_u["Budi"], r.Data.Id, new DispensationDecisionRequest { RowVersion = r.Data.RowVersion });
        Assert.Equal(403, budiBatal.StatusCode);
        var tolak = await Svc("Sari").RejectAsync(_u["Sari"], r.Data.Id, new RejectDispensationRequest { RowVersion = r.Data.RowVersion, RejectionReason = "Rujukan tidak tercatat" });
        Assert.True(tolak.Success, tolak.Message);
        Assert.Equal(("Rejected", "Rujukan tidak tercatat"), (tolak.Data!.Status, tolak.Data.RejectionReason));
        Assert.Empty(_lonceng.Terkirim);
    }

    // State 30.3 dan 35.9 (klik ganda): transisi hanya dari Submitted; token basi 409; alasan tolak wajib; batal oleh pengaju.
    [Fact]
    public async Task Transisi_dan_konkurensi()
    {
        var d = await Ajukan("Budi", Pengajuan(F(3)));
        var pertama = await Svc("Sari").ApproveAsync(_u["Sari"], d.Id, new DispensationDecisionRequest { RowVersion = d.RowVersion });
        Assert.True(pertama.Success);
        var kedua = await Svc("Sari").ApproveAsync(_u["Sari"], d.Id, new DispensationDecisionRequest { RowVersion = d.RowVersion });
        Assert.Equal(409, kedua.StatusCode);
        var tokenBaru = await Svc("Sari").RejectAsync(_u["Sari"], d.Id, new RejectDispensationRequest { RowVersion = pertama.Data!.RowVersion, RejectionReason = "x" });
        Assert.Equal(409, tokenBaru.StatusCode);
        Assert.Contains("Disetujui", tokenBaru.Message);
        Assert.Equal(409, (await Svc("Budi").CancelMineAsync(_u["Budi"], d.Id, new DispensationDecisionRequest { RowVersion = pertama.Data.RowVersion })).StatusCode);
        Assert.Single(_lonceng.Terkirim);

        var e = await Ajukan("Budi", Pengajuan(F(4)));
        Assert.Equal((400, "Alasan penolakan wajib diisi."), await Hasil(Svc("Sari").RejectAsync(_u["Sari"], e.Id, new RejectDispensationRequest { RowVersion = e.RowVersion, RejectionReason = " " })));
        Assert.Equal(404, (await Svc("Tono").CancelMineAsync(_u["Tono"], e.Id, new DispensationDecisionRequest { RowVersion = e.RowVersion })).StatusCode);
        var batal = await Svc("Budi").CancelMineAsync(_u["Budi"], e.Id, new DispensationDecisionRequest { RowVersion = e.RowVersion });
        Assert.Equal("Cancelled", batal.Data!.Status);
        Assert.Equal(409, (await Svc("Sari").ApproveAsync(_u["Sari"], e.Id, new DispensationDecisionRequest { RowVersion = batal.Data.RowVersion })).StatusCode);
        Assert.Equal(404, (await Svc("Sari").ApproveAsync(_u["Sari"], Guid.NewGuid(), new DispensationDecisionRequest())).StatusCode);

        // Pemberitahuan gagal tidak membatalkan keputusan (INT-HR-105).
        var f = await Ajukan("Budi", Pengajuan(F(5), 13, 0, 14, 0, jenis: "EarlyDeparture"));
        var mati = await Svc("Sari", new PemberitahuanTiruan(gagal: true)).ApproveAsync(_u["Sari"], f.Id, new DispensationDecisionRequest { RowVersion = f.RowVersion });
        Assert.True(mati.Success, mati.Message);
        using var ctx = _db.CreateContext();
        Assert.Equal("Approved", ctx.HrdAttendanceDispensations.Single(x => x.Id == f.Id).Status);
    }

    private static async Task<(int, string)> Hasil<T>(Task<AttendanceDispensationServiceResult<T>> t)
    {
        var r = await t;
        return (r.StatusCode, r.Message);
    }

    // Daftar: cakupan baca + yang menunggu keputusan; layanan mandiri hanya milik sendiri.
    [Fact]
    public async Task Daftar_dan_detail_mengikuti_cakupan()
    {
        var budi = await Ajukan("Budi", Pengajuan(F(3)));
        var sari = await Ajukan("Sari", Pengajuan(F(4)));
        var andi = await Ajukan("Andi", Pengajuan(F(4)));

        async Task<List<string>> Daftar(string siapa, DispensationQueryRequest? q = null)
        {
            var r = await Svc(siapa).ListAsync(_u[siapa], q ?? new DispensationQueryRequest(), Sekarang);
            Assert.True(r.Success, r.Message);
            return r.Data!.Items.Select(x => x.EmployeeName!).OrderBy(x => x).ToList();
        }

        Assert.Equal(new[] { "Budi", "Sari" }, await Daftar("Sari"));
        Assert.Equal(new[] { "Andi", "Budi", "Sari" }, await Daftar("Dodi"));
        Assert.Equal(new[] { "Sari" }, await Daftar("Dodi", new DispensationQueryRequest { AwaitingMyDecision = true }));
        Assert.Equal(new[] { "Budi" }, await Daftar("Sari", new DispensationQueryRequest { AwaitingMyDecision = true }));
        Assert.Equal(new[] { "Andi", "Budi", "Sari" }, await Daftar("Admin"));
        Assert.Equal(new[] { "Andi" }, await Daftar("Hani"));
        Assert.Empty(await Daftar("Budi"));
        Assert.Equal(new[] { "Andi" }, await Daftar("Dodi", new DispensationQueryRequest { OrganizationUnitId = _hcu }));
        Assert.Equal(403, (await Svc("Sari").ListAsync(_u["Sari"], new DispensationQueryRequest { OrganizationUnitId = _hcu }, Sekarang)).StatusCode);
        Assert.Empty(await Daftar("Sari", new DispensationQueryRequest { FromDate = F(10), ToDate = F(20) }));
        Assert.Equal(new[] { "Budi" }, await Daftar("Sari", new DispensationQueryRequest { Status = "Submitted", FromDate = F(3), ToDate = F(3) }));

        Assert.Equal(403, (await Svc("Sari").GetAsync(_u["Sari"], andi.Id)).StatusCode);
        Assert.True((await Svc("Hani").GetAsync(_u["Hani"], andi.Id)).Success);
        Assert.True((await Svc("Dodi").GetAsync(_u["Dodi"], sari.Id)).Data!.CanDecide);
        Assert.Equal(404, (await Svc("Sari").GetAsync(_u["Sari"], Guid.NewGuid())).StatusCode);

        var milikBudi = await Svc("Budi").ListMineAsync(_u["Budi"], new MyDispensationQueryRequest(), Sekarang);
        Assert.Equal(new[] { budi.Id }, milikBudi.Data!.Items.Select(x => x.Id));
        var catatan = await Svc("Sari").CreateOnBehalfAsync(_u["Sari"], AtasNama(_wf["Budi"], F(5)), Sekarang);
        var milikBudi2 = await Svc("Budi").ListMineAsync(_u["Budi"], new MyDispensationQueryRequest(), Sekarang);
        Assert.Equal(2, milikBudi2.Data!.TotalData);
        Assert.False(milikBudi2.Data.Items.Single(x => x.Id == catatan.Data!.Id).CanCancel);
    }
}
