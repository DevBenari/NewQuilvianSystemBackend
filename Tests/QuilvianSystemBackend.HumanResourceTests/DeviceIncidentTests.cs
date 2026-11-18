using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-356 — kejadian gangguan mesin dan rujukan pada koreksi (HRD-DEC-202, HRD-ASM-77, state 30.5, VR-PK-20 s.d. 22);
// AT-HRD-PK-35, AT-HRD-PK-36. Tanggal kerja = 2 hari lalu (WIB) agar lolos batas pengajuan koreksi 7 hari.
public sealed class DeviceIncidentTests : IDisposable
{
    private static readonly DateOnly Hari = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7)).AddDays(-2);
    private static DateTime Wib(int jam, int menit = 0, int tambahHari = 0) => new DateTime(Hari.Year, Hari.Month, Hari.Day, jam, menit, 0, DateTimeKind.Utc).AddDays(tambahHari).AddHours(-7);
    private static string Iso(DateTime utc) => utc.ToString("yyyy-MM-ddTHH:mm:ssZ");

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _igd = Guid.NewGuid(), _icu = Guid.NewGuid(), _petugasIt = Guid.NewGuid();
    private readonly Dictionary<string, (Guid Wf, Guid User, Guid Daily)> _p = new();

    public DeviceIncidentTests()
    {
        using var ctx = _db.CreateContext();
        ctx.Set<MstAttendanceDevice>().AddRange(
            new MstAttendanceDevice { Id = _igd, AttendanceDeviceCode = "IGD-01", AttendanceDeviceName = "Mesin Pintu IGD", IsActive = true },
            new MstAttendanceDevice { Id = _icu, AttendanceDeviceCode = "ICU-01", AttendanceDeviceName = "Mesin ICU", IsActive = true });
        ctx.Users.Add(new ApplicationUser { Id = _petugasIt, UserName = "it", UserCode = "PETUGIT1", DisplayName = "Petugas IT", UserType = UserType.Employee, IsActive = true });
        ctx.SaveChanges();
        foreach (var nama in new[] { "Budi", "Rina", "Tono" }) Pegawai(nama);

        // Log mesin lama milik pegawai lain: jumlahnya tidak boleh berubah karena kejadian gangguan (AT-HRD-PK-36).
        ctx.HrdAttendanceRawLogs.Add(new HrdAttendanceRawLog { WorkforceProfileId = _p["Tono"].Wf, AttendanceDeviceId = _igd, EventAt = Wib(5, 55), EventType = "CheckIn", SourceType = "Device", ProcessingStatus = "Processed" });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private void Pegawai(string nama)
    {
        using var ctx = _db.CreateContext();
        var wf = Guid.NewGuid();
        var user = Guid.NewGuid();
        ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = wf, ProfileCode = nama, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
        ctx.Users.Add(new ApplicationUser { Id = user, UserName = nama, UserCode = nama.ToUpperInvariant().PadRight(8, 'X')[..8], DisplayName = nama, UserType = UserType.Employee, WorkforceProfileId = wf, IsActive = true });
        var daily = new HrdAttendanceDaily
        {
            UserId = user, WorkforceProfileId = wf, UserType = UserType.Employee, AttendanceDate = Hari, AttendanceStatus = "NoScan",
            ScheduledCheckInAt = Wib(7), ScheduledCheckOutAt = Wib(14), ProcessingStatus = "Processed", PayrollInputStatus = "Ready", IsActive = true,
        };
        ctx.HrdAttendanceDailies.Add(daily);
        ctx.SaveChanges();
        _p[nama] = (wf, user, daily.Id);
    }

    private AttendanceDeviceIncidentService Insiden() => new(_db.CreateContext());

    // Hanya jalur buat/ubah/gerbang lampiran yang dipakai; workflow tidak tersentuh.
    private AttendanceCorrectionService Koreksi() => new(_db.CreateContext(), null!, null!, null!);

    private async Task<DeviceIncidentResponse> Catat(DateTime mulai, DateTime selesai, Guid? mesin = null)
    {
        var r = await Insiden().CreateAsync(new CreateDeviceIncidentRequest { AttendanceDeviceId = mesin ?? _igd, StartAt = mulai, EndAt = selesai, Description = "Mesin mati listrik" }, _petugasIt);
        Assert.True(r.Success, r.Message);
        return r.Data!;
    }

    private static CreateAttendanceCorrectionRequest KoreksiJam(Guid daily, Guid? insiden, params (string Field, DateTime Jam)[] jam) => new()
    {
        AttendanceDailyId = daily,
        DeviceIncidentId = insiden,
        CorrectionType = AttendanceValueConstants.CorrectionType.AttendanceTime,
        Reason = "Mesin IGD mati",
        Details = jam.Select(x => new AttendanceCorrectionDetailInputRequest { FieldName = x.Field, RequestedValue = Iso(x.Jam) }).ToList()
    };

    private async Task<Guid> BuatKoreksi(string nama, Guid? insiden, params (string, DateTime)[] jam)
    {
        var r = await Koreksi().CreateAsync(KoreksiJam(_p[nama].Daily, insiden, jam), _p[nama].User);
        Assert.True(r.Success, r.Message);
        return r.Data!.Id;
    }

    private async Task<(int, string)?> GerbangAjukan(Guid koreksi)
    {
        using var ctx = _db.CreateContext();
        var entity = await ctx.HrdAttendanceCorrectionRequests.Include(x => x.Details).Include(x => x.RequestReason).SingleAsync(x => x.Id == koreksi);
        return await new AttendanceCorrectionService(ctx, null!, null!, null!).ValidateSubmissionEvidenceAsync(entity, CancellationToken.None);
    }

    // AT-HRD-PK-36, state 30.5, VR-PK-20.
    [Fact]
    public async Task AT_HRD_PK_36_catat_kejadian_tidak_menambah_log_mesin()
    {
        int JumlahLog() { using var ctx = _db.CreateContext(); return ctx.HrdAttendanceRawLogs.Count(); }
        var sebelum = JumlahLog();

        var k = await Catat(Wib(6, 30), Wib(9));
        Assert.Equal(("Active", "Aktif", "IGD-01", "Mesin Pintu IGD", "Petugas IT", 0), (k.Status, k.StatusLabel, k.AttendanceDeviceCode, k.AttendanceDeviceName, k.ReportedByName, k.ReferencedCorrectionCount));
        Assert.Equal((Wib(6, 30), Wib(9)), (k.StartAt, k.EndAt));
        Assert.Equal(sebelum, JumlahLog());

        // Jam tanpa zona dibaca WIB.
        var tanpaZona = await Insiden().CreateAsync(new CreateDeviceIncidentRequest { AttendanceDeviceId = _icu, StartAt = new DateTime(Hari.Year, Hari.Month, Hari.Day, 10, 0, 0), EndAt = new DateTime(Hari.Year, Hari.Month, Hari.Day, 11, 0, 0), Description = "x" }, _petugasIt);
        Assert.Equal(Wib(10), tanpaZona.Data!.StartAt);

        async Task<(int, string)> Gagal(CreateDeviceIncidentRequest req) { var r = await Insiden().CreateAsync(req, _petugasIt); return (r.StatusCode, r.Message); }
        Assert.Equal((400, "Jam selesai gangguan harus sesudah jam mulai"), await Gagal(new() { AttendanceDeviceId = _igd, StartAt = Wib(9), EndAt = Wib(9), Description = "x" }));
        Assert.Equal((400, "Jam selesai gangguan harus sesudah jam mulai"), await Gagal(new() { AttendanceDeviceId = _igd, StartAt = Wib(9), EndAt = Wib(8), Description = "x" }));
        Assert.Equal(400, (await Gagal(new() { AttendanceDeviceId = _igd, StartAt = Wib(8), EndAt = Wib(9), Description = " " })).Item1);
        Assert.Equal((404, "Mesin absen tidak terdaftar."), await Gagal(new() { AttendanceDeviceId = Guid.NewGuid(), StartAt = Wib(8), EndAt = Wib(9), Description = "x" }));

        // Batal wajib alasan; sekali saja.
        Assert.Equal(400, (await Insiden().CancelAsync(k.Id, new CancelDeviceIncidentRequest(), _petugasIt)).StatusCode);
        var batal = await Insiden().CancelAsync(k.Id, new CancelDeviceIncidentRequest { CancelReason = "Salah mesin" }, _petugasIt);
        Assert.Equal(("Cancelled", "Salah mesin"), (batal.Data!.Status, batal.Data.CancelReason));
        Assert.Equal(409, (await Insiden().CancelAsync(k.Id, new CancelDeviceIncidentRequest { CancelReason = "x" }, _petugasIt)).StatusCode);
        Assert.Equal(404, (await Insiden().CancelAsync(Guid.NewGuid(), new CancelDeviceIncidentRequest { CancelReason = "x" }, _petugasIt)).StatusCode);
        Assert.Equal(sebelum, JumlahLog());
    }

    // HRD-DEC-202 butir 6: riwayat per mesin; pilihan formulir koreksi hanya kejadian Active pada tanggal itu.
    [Fact]
    public async Task Daftar_riwayat_per_mesin_dan_pilihan_koreksi()
    {
        var a = await Catat(Wib(6, 30), Wib(9));
        await Catat(Wib(22), Wib(2, 0, 1));                 // malam, lewat tengah malam
        var c = await Catat(Wib(13), Wib(14), _icu);
        var lain = await Catat(Wib(8, 0, -3), Wib(9, 0, -3)); // tiga hari sebelumnya
        await Insiden().CancelAsync(c.Id, new CancelDeviceIncidentRequest { CancelReason = "Salah catat" }, _petugasIt);

        async Task<int> Total(DeviceIncidentQueryRequest q) => (await Insiden().ListAsync(q)).Data!.TotalData;
        Assert.Equal(4, await Total(new DeviceIncidentQueryRequest()));
        Assert.Equal(3, await Total(new DeviceIncidentQueryRequest { AttendanceDeviceId = _igd }));
        Assert.Equal(3, await Total(new DeviceIncidentQueryRequest { FromDate = Hari, ToDate = Hari }));
        Assert.Equal(1, await Total(new DeviceIncidentQueryRequest { Status = "Cancelled" }));
        Assert.Equal(1, await Total(new DeviceIncidentQueryRequest { FromDate = Hari.AddDays(1), ToDate = Hari.AddDays(1) }));   // malam lewat tengah malam
        Assert.Equal(400, (await Insiden().ListAsync(new DeviceIncidentQueryRequest { FromDate = Hari, ToDate = Hari.AddDays(-1) })).StatusCode);

        var opsi = await Insiden().GetOptionsAsync(Hari);
        Assert.Equal(2, opsi.Count);                                                  // ICU dibatalkan dan kejadian 3 hari lalu tidak ikut
        Assert.Equal(a.Id, opsi[0].Value);
        Assert.Equal($"IGD-01 Mesin Pintu IGD — {Hari:dd-MM-yyyy} 06.30–09.00 — Mesin mati listrik", opsi[0].Label);
        Assert.DoesNotContain(opsi, x => x.Value == lain.Id);
    }

    // Uji (b)(d) + AT-HRD-PK-35 + VR-PK-21/22 + HRD-ASM-77.
    [Fact]
    public async Task AT_HRD_PK_35_rujukan_kejadian_pada_koreksi()
    {
        var k = await Catat(Wib(6, 30), Wib(9));

        // Koreksi 06.50 (dalam rentang) tanpa lampiran → lolos gerbang; 09.30 (di luar) tanpa lampiran → 422.
        var dalam = await BuatKoreksi("Budi", k.Id, ("FirstCheckInAt", Wib(6, 50)), ("LastCheckOutAt", Wib(9)));
        var luar = await BuatKoreksi("Rina", k.Id, ("FirstCheckInAt", Wib(9, 30)));
        Assert.Null(await GerbangAjukan(dalam));
        Assert.Equal((422, "Jam di luar rentang gangguan wajib disertai lampiran"), await GerbangAjukan(luar));

        // Satu kejadian dirujuk lebih dari satu koreksi (uji (d)); rujukan tersimpan dan tampil di detail.
        Assert.Equal(2, (await Insiden().ListAsync(new DeviceIncidentQueryRequest())).Data!.Items.Single().ReferencedCorrectionCount);
        Assert.Equal(k.Id, (await Koreksi().GetDetailAsync(dalam, null)).Data!.DeviceIncidentId);

        // Di luar rentang dengan lampiran → lolos (menjadi koreksi biasa).
        using (var ctx = _db.CreateContext())
        {
            ctx.HrdAttendanceCorrectionRequests.Single(x => x.Id == luar).EvidenceFilePath = "uji/cctv.jpg";
            ctx.SaveChanges();
        }
        Assert.Null(await GerbangAjukan(luar));

        // Koreksi tanpa jam (status saja) yang merujuk kejadian tidak dibebaskan dari lampiran.
        var tanpaJam = await Koreksi().CreateAsync(new CreateAttendanceCorrectionRequest
        {
            AttendanceDailyId = _p["Tono"].Daily, DeviceIncidentId = k.Id, CorrectionType = AttendanceValueConstants.CorrectionType.Status, Reason = "x",
            Details = new() { new AttendanceCorrectionDetailInputRequest { FieldName = "AttendanceStatus", RequestedValue = "Present" } }
        }, _p["Tono"].User);
        Assert.True(tanpaJam.Success, tanpaJam.Message);
        Assert.Equal(422, (await GerbangAjukan(tanpaJam.Data!.Id))!.Value.Item1);

        // Kejadian dibatalkan: koreksi yang sudah merujuk tidak dapat diajukan; koreksi baru yang merujuk ditolak (VR-PK-21).
        await Insiden().CancelAsync(k.Id, new CancelDeviceIncidentRequest { CancelReason = "Ternyata mesin normal" }, _petugasIt);
        Assert.Equal((422, "Kejadian gangguan ini sudah dibatalkan"), await GerbangAjukan(dalam));
        using (var ctx = _db.CreateContext())
        {
            foreach (var lama in ctx.HrdAttendanceCorrectionRequests.Where(x => x.WorkforceProfileId == _p["Budi"].Wf)) lama.IsDelete = true;
            ctx.SaveChanges();
        }
        var baru = await Koreksi().CreateAsync(KoreksiJam(_p["Budi"].Daily, k.Id, ("FirstCheckInAt", Wib(6, 50))), _p["Budi"].User);
        Assert.Equal((422, "Kejadian gangguan ini sudah dibatalkan"), (baru.StatusCode, baru.Message));
        Assert.Equal(404, (await Koreksi().CreateAsync(KoreksiJam(_p["Budi"].Daily, Guid.NewGuid(), ("FirstCheckInAt", Wib(6, 50))), _p["Budi"].User)).StatusCode);

        // Ubah: rujukan dapat dilepas atau diganti; kejadian dibatalkan tetap ditolak.
        var k2 = await Catat(Wib(6), Wib(10), _icu);
        var biasa = await BuatKoreksi("Budi", null, ("FirstCheckInAt", Wib(6, 50)));
        var ubah = new UpdateAttendanceCorrectionRequest
        {
            DeviceIncidentId = k2.Id, CorrectionType = AttendanceValueConstants.CorrectionType.AttendanceTime, Reason = "Mesin ICU juga mati",
            Details = new() { new AttendanceCorrectionDetailInputRequest { FieldName = "FirstCheckInAt", RequestedValue = Iso(Wib(6, 50)) } }
        };
        var hasilUbah = await Koreksi().UpdateAsync(biasa, ubah, _p["Budi"].User);
        Assert.True(hasilUbah.Success, hasilUbah.Message);
        Assert.Equal(k2.Id, (await Koreksi().GetDetailAsync(biasa, null)).Data!.DeviceIncidentId);
        Assert.Null(await GerbangAjukan(biasa));
        ubah.DeviceIncidentId = k.Id;
        Assert.Equal(422, (await Koreksi().UpdateAsync(biasa, ubah, _p["Budi"].User)).StatusCode);
    }

    // Tanpa rujukan kejadian, aturan lampiran lama tetap berlaku.
    [Fact]
    public async Task Tanpa_rujukan_aturan_lampiran_lama_tetap()
    {
        var alasan = new MstRequestReason { RequestType = "AttendanceCorrection", ReasonCode = "LUPA", ReasonName = "Lupa absen", IsAttachmentRequired = true, IsActive = true };
        using (var ctx = _db.CreateContext())
        {
            ctx.Set<MstRequestReason>().Add(alasan);
            ctx.SaveChanges();
        }

        var req = KoreksiJam(_p["Budi"].Daily, null, ("FirstCheckInAt", Wib(6, 50)));
        req.RequestReasonId = alasan.Id;
        var r = await Koreksi().CreateAsync(req, _p["Budi"].User);
        Assert.True(r.Success, r.Message);
        Assert.Equal((400, "Alasan pengajuan yang dipilih mewajibkan evidence attachment."), await GerbangAjukan(r.Data!.Id));

        var tanpaWajib = await BuatKoreksi("Rina", null, ("FirstCheckInAt", Wib(9, 30)));
        Assert.Null(await GerbangAjukan(tanpaWajib));

        // Rujukan dalam rentang membebaskan lampiran walau alasan pengajuan mewajibkannya (HRD-DEC-202 butir 4).
        var k = await Catat(Wib(6, 30), Wib(9));
        var reqDalam = KoreksiJam(_p["Tono"].Daily, k.Id, ("FirstCheckInAt", Wib(6, 50)));
        reqDalam.RequestReasonId = alasan.Id;
        var dalam = await Koreksi().CreateAsync(reqDalam, _p["Tono"].User);
        Assert.Null(await GerbangAjukan(dalam.Data!.Id));
    }
}
