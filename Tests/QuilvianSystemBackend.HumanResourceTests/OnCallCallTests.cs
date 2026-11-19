using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-352 — cadangan siaga dan catatan panggilan: AT-HRD-PK-15–18, VR-PK-08–12, INT-HR-39 (HRD-DEC-198, 204, 205; HRD-ASM-75, 78).
// Contoh keputusan: dr. Andi siaga bedah malam 14 Feb 2027 (batas jawab 10 menit), cadangan dr. Budi dan dr. Citra; IGD memanggil 02.10.
public sealed class OnCallCallTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _igd = Guid.NewGuid(), _penugasan = Guid.NewGuid(), _aktor = Guid.NewGuid(), _lina = Guid.NewGuid();
    private readonly Guid _andi = Guid.NewGuid(), _budi = Guid.NewGuid(), _citra = Guid.NewGuid(), _dodi = Guid.NewGuid();
    private static DateTime Wib(int jam, int menit) => new DateTime(2027, 2, 14, jam, menit, 0, DateTimeKind.Utc).AddHours(-7).AddDays(jam < 12 ? 1 : 0);

    public OnCallCallTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _igd, LegalEntityId = Guid.NewGuid(), UnitCode = "IGD", UnitName = "IGD" });
        foreach (var (id, nama) in new[] { (_andi, "dr. Andi"), (_budi, "dr. Budi"), (_citra, "dr. Citra"), (_dodi, "dr. Dodi") })
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = nama, DisplayName = nama, PhoneNumber = "08-" + nama, UserType = UserType.PermanentDoctor, IsActive = true });
        ctx.Users.Add(new QuilvianSystemBackend.Models.ApplicationUser { Id = _lina, UserName = "lina", UserCode = "LINAXXXX", DisplayName = "Lina (IGD)", UserType = UserType.Employee, IsActive = true });
        var tipe = new MstOnCallType { OnCallTypeCode = "SDB", OnCallTypeName = "Siaga Dokter Bedah", AnswerTimeoutMinutes = 10, ResponseTimeMinutes = 30, IsActive = true };
        ctx.Set<MstOnCallType>().Add(tipe);
        var roster = Guid.NewGuid();
        ctx.TrxRosterPeriods.Add(new TrxRosterPeriod { Id = roster, OrganizationUnitId = _igd, RosterPeriodCode = "R", RosterPeriodName = "R", RosterStatus = "Published", PeriodStartDate = new DateOnly(2027, 2, 1), PeriodEndDate = new DateOnly(2027, 2, 28), IsActive = true });
        ctx.TrxOnCallAssignments.Add(new TrxOnCallAssignment
        {
            Id = _penugasan, RosterPeriodId = roster, WorkforceProfileId = _andi, OnCallTypeId = tipe.Id, UnitId = _igd,
            StartAt = Wib(20, 0), EndAt = Wib(20, 0).AddHours(12), AssignmentStatus = "Active", ExpectedResponseMinutes = 30, IsActive = true,
        });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private OnCallCallService Panggilan() => new(_db.CreateContext(), NullLogger<OnCallCallService>.Instance);
    private OnCallAssignmentService Penugasan() => new(_db.CreateContext(), NullLogger<OnCallAssignmentService>.Instance);

    private async Task Cadangan(params Guid[] urut)
    {
        var r = await Penugasan().SetBackupsAsync(_penugasan, new SetOnCallBackupsRequest { Items = urut.Select((id, i) => new SetOnCallBackupItemRequest { BackupSequence = i + 1, WorkforceProfileId = id }).ToList() }, _aktor);
        Assert.True(r.Success, r.Message);
    }

    private async Task<OnCallCallResponse> Panggil(Guid dokter, DateTime jam)
    {
        var r = await Panggilan().CreateCallAsync(_penugasan, new CreateOnCallCallRequest { CalledWorkforceProfileId = dokter, CallingOrganizationUnitId = _igd, CallReason = "Apendisitis perforasi", CalledAt = jam }, _lina);
        Assert.True(r.Success, r.Message);
        return r.Data!;
    }

    private TrxOnCallAssignment Baca()
    {
        using var ctx = _db.CreateContext();
        return ctx.TrxOnCallAssignments.AsNoTracking().Single(x => x.Id == _penugasan);
    }

    // AT-HRD-PK-15 + INV-A52-04: Dikirim → Diterima → Selesai; status penugasan tetap Active; INT-HR-39 tetap mengembalikan dr. Andi.
    [Fact]
    public async Task Tiga_transisi_tercatat_status_penugasan_tetap_dan_INT_HR_39_tetap()
    {
        var c = await Panggil(_andi, Wib(2, 10));
        Assert.Equal(("Sent", 0), (c.CallStatus, c.BackupSequence));
        Assert.Equal("Lina (IGD)", c.RecordedByName);

        var terima = await Panggilan().AcceptAsync(_penugasan, c.Id, new AnswerOnCallCallRequest { AnsweredAt = Wib(2, 14), RowVersion = c.RowVersion }, _lina);
        Assert.True(terima.Success, terima.Message);
        Assert.Equal("Accepted", terima.Data!.CallStatus);
        Assert.Equal(_lina, terima.Data.AnswerRecordedByUserId);   // HRD-DEC-204: petugas pemanggil yang mencatat jawaban

        var selesai = await Panggilan().CompleteAsync(_penugasan, c.Id, new CompleteOnCallCallRequest { ArrivedAt = Wib(2, 35), RowVersion = terima.Data.RowVersion }, _lina, Wib(2, 36));
        Assert.True(selesai.Success, selesai.Message);
        Assert.Equal(("Completed", 25), (selesai.Data!.CallStatus, selesai.Data.ResponseMinutes));

        var p = Baca();
        Assert.Equal("Active", p.AssignmentStatus);
        Assert.True(p.IsCalledIn);
        Assert.Equal(25, p.ResponseMinutes);
        Assert.True(p.IsSlaMet);
        var aktif = await Penugasan().GetActiveOnCallDoctorIdsAsync(null, Wib(2, 40));
        Assert.Equal(new[] { _andi }, aktif);

        // Panggilan kedua pada penugasan yang sama adalah catatan baru.
        await Panggil(_andi, Wib(4, 40));
        Assert.Equal(2, (await Panggilan().GetCallsAsync(_penugasan)).Data!.Count);
    }

    // AT-HRD-PK-16 / VR-PK-10: kedaluwarsa 5 menit sesudah panggilan ditolak; sesudah 10 menit berhasil dengan cadangan 1.
    [Fact]
    public async Task Kedaluwarsa_hanya_sesudah_batas_jawab_dan_menampilkan_cadangan_berikutnya()
    {
        await Cadangan(_budi, _citra);
        var c = await Panggil(_andi, Wib(2, 10));
        var dini = await Panggilan().ExpireAsync(_penugasan, c.Id, new ExpireOnCallCallRequest { RowVersion = c.RowVersion }, _lina, Wib(2, 15));
        Assert.Equal(422, dini.StatusCode);
        Assert.Equal("Batas jawab 10 menit belum lewat sejak panggilan 02.10", dini.Message);

        var lewat = await Panggilan().ExpireAsync(_penugasan, c.Id, new ExpireOnCallCallRequest { RowVersion = c.RowVersion }, _lina, Wib(2, 20));
        Assert.True(lewat.Success, lewat.Message);
        Assert.Equal("Expired", lewat.Data!.CallStatus);
        Assert.Equal((1, _budi, "08-dr. Budi"), (lewat.Data.NextBackup!.BackupSequence, lewat.Data.NextBackup.WorkforceProfileId, lewat.Data.NextBackup.PhoneNumber));
        Assert.Null(lewat.Data.NoBackupMessage);
    }

    // AT-HRD-PK-17: semua menolak → pesan menghubungi Kepala Unit; VR-PK-11 alasan wajib.
    [Fact]
    public async Task Cadangan_habis_sesudah_semua_menolak()
    {
        await Cadangan(_budi);
        var a = await Panggil(_andi, Wib(2, 10));
        var tanpaAlasan = await Panggilan().DeclineAsync(_penugasan, a.Id, new DeclineOnCallCallRequest { AnsweredAt = Wib(2, 11), DeclineReason = " ", RowVersion = a.RowVersion }, _lina);
        Assert.Equal((400, "Alasan penolakan wajib diisi"), (tanpaAlasan.StatusCode, tanpaAlasan.Message));

        var tolakA = await Panggilan().DeclineAsync(_penugasan, a.Id, new DeclineOnCallCallRequest { AnsweredAt = Wib(2, 11), DeclineReason = "Sedang operasi", RowVersion = a.RowVersion }, _lina);
        Assert.Equal(_budi, tolakA.Data!.NextBackup!.WorkforceProfileId);

        var b = await Panggil(_budi, Wib(2, 12));
        Assert.Equal(1, b.BackupSequence);
        var tolakB = await Panggilan().DeclineAsync(_penugasan, b.Id, new DeclineOnCallCallRequest { AnsweredAt = Wib(2, 13), DeclineReason = "Di luar kota", RowVersion = b.RowVersion }, _lina);
        Assert.Null(tolakB.Data!.NextBackup);
        Assert.Equal("Hubungi Kepala Unit pemilik jadwal siaga", tolakB.Data.NoBackupMessage);
        Assert.Equal("Active", Baca().AssignmentStatus);
    }

    // AT-HRD-PK-18 / HRD-ASM-75: panggilan pertama ditolak, cadangan menerima dan tiba 25 menit → ringkasan dipanggil = ya, respons 25.
    [Fact]
    public async Task Ringkasan_dari_panggilan_diterima_pertama()
    {
        await Cadangan(_budi);
        var a = await Panggil(_andi, Wib(2, 10));
        await Panggilan().DeclineAsync(_penugasan, a.Id, new DeclineOnCallCallRequest { AnsweredAt = Wib(2, 11), DeclineReason = "Sedang operasi", RowVersion = a.RowVersion }, _lina);
        Assert.False(Baca().IsCalledIn);

        var b = await Panggil(_budi, Wib(2, 21));
        var terima = await Panggilan().AcceptAsync(_penugasan, b.Id, new AnswerOnCallCallRequest { AnsweredAt = Wib(2, 22), RowVersion = b.RowVersion }, _lina);
        var p1 = Baca();
        Assert.True(p1.IsCalledIn);
        Assert.Equal(Wib(2, 21), DateTime.SpecifyKind(p1.CallInTime!.Value, DateTimeKind.Utc));
        Assert.Null(p1.ResponseMinutes);

        await Panggilan().CompleteAsync(_penugasan, b.Id, new CompleteOnCallCallRequest { ArrivedAt = Wib(2, 46), RowVersion = terima.Data!.RowVersion }, _lina, Wib(2, 47));
        var p2 = Baca();
        Assert.Equal((true, 25, "Active"), (p2.IsCalledIn, p2.ResponseMinutes!.Value, p2.AssignmentStatus));

        // Panggilan Diterima kedua (sesudahnya) tidak menimpa ringkasan.
        var c2 = await Panggil(_andi, Wib(4, 0));
        var t2 = await Panggilan().AcceptAsync(_penugasan, c2.Id, new AnswerOnCallCallRequest { AnsweredAt = Wib(4, 1), RowVersion = c2.RowVersion }, _lina);
        await Panggilan().CompleteAsync(_penugasan, c2.Id, new CompleteOnCallCallRequest { ArrivedAt = Wib(4, 5), RowVersion = t2.Data!.RowVersion }, _lina, Wib(4, 6));
        Assert.Equal(25, Baca().ResponseMinutes);
    }

    // VR-PK-09: penugasan tidak Active, atau yang dipanggil bukan siaga utama/cadangan → 422; INT-HR-39 tidak memuat cadangan (HRD-ASM-78).
    [Fact]
    public async Task Panggilan_hanya_untuk_siaga_pada_penugasan_aktif()
    {
        await Cadangan(_budi);
        var asing = await Panggilan().CreateCallAsync(_penugasan, new CreateOnCallCallRequest { CalledWorkforceProfileId = _dodi, CallingOrganizationUnitId = _igd, CallReason = "x", CalledAt = Wib(2, 10) }, _lina);
        Assert.Equal((422, "Pegawai ini tidak sedang siaga pada penugasan tersebut"), (asing.StatusCode, asing.Message));
        Assert.Equal(400, (await Panggilan().CreateCallAsync(_penugasan, new CreateOnCallCallRequest { CalledWorkforceProfileId = _andi, CallingOrganizationUnitId = _igd, CallReason = "", CalledAt = Wib(2, 10) }, _lina)).StatusCode);
        Assert.Equal(new[] { _andi }, await Penugasan().GetActiveOnCallDoctorIdsAsync(null, Wib(2, 10)));

        using (var ctx = _db.CreateContext())
        {
            ctx.TrxOnCallAssignments.Single().AssignmentStatus = "Scheduled";
            ctx.SaveChanges();
        }
        Assert.Equal(422, (await Panggilan().CreateCallAsync(_penugasan, new CreateOnCallCallRequest { CalledWorkforceProfileId = _andi, CallingOrganizationUnitId = _igd, CallReason = "x", CalledAt = Wib(2, 10) }, _lina)).StatusCode);
    }

    // VR-PK-12, transisi tidak sah, batas jawab saat menerima, dan konkurensi RowVersion.
    [Fact]
    public async Task Validasi_transisi_dan_konkurensi()
    {
        var c = await Panggil(_andi, Wib(2, 10));
        Assert.Equal(409, (await Panggilan().CompleteAsync(_penugasan, c.Id, new CompleteOnCallCallRequest { ArrivedAt = Wib(2, 30), RowVersion = c.RowVersion }, _lina, Wib(2, 31))).StatusCode);   // belum Diterima
        var telat = await Panggilan().AcceptAsync(_penugasan, c.Id, new AnswerOnCallCallRequest { AnsweredAt = Wib(2, 25), RowVersion = c.RowVersion }, _lina);
        Assert.Equal(422, telat.StatusCode);   // batas jawab lewat

        var t = await Panggilan().AcceptAsync(_penugasan, c.Id, new AnswerOnCallCallRequest { AnsweredAt = Wib(2, 12), RowVersion = c.RowVersion }, _lina);
        Assert.True(t.Success, t.Message);
        // Status Diterima sah untuk "tiba", tetapi RowVersion lama (sebelum diterima) → 409, tidak ada yang tertimpa.
        var basi = await Panggilan().CompleteAsync(_penugasan, c.Id, new CompleteOnCallCallRequest { ArrivedAt = Wib(2, 30), RowVersion = c.RowVersion }, _lina, Wib(2, 31));
        Assert.Equal(409, basi.StatusCode);

        var mundur = await Panggilan().CompleteAsync(_penugasan, c.Id, new CompleteOnCallCallRequest { ArrivedAt = Wib(2, 5), RowVersion = t.Data!.RowVersion }, _lina, Wib(2, 31));
        Assert.Equal((400, "Jam tiba tidak boleh lebih awal dari jam panggil"), (mundur.StatusCode, mundur.Message));
        using var ctx = _db.CreateContext();
        Assert.Equal("Accepted", ctx.HrdOnCallCalls.AsNoTracking().Single().CallStatus);
    }

    // VR-PK-08 dan ganti utuh daftar cadangan.
    [Fact]
    public async Task Daftar_cadangan_ganti_utuh_dan_VR_PK_08()
    {
        const string pesan = "Urutan cadangan dan nama cadangan tidak boleh ganda atau sama dengan siaga utama";
        foreach (var items in new[]
        {
            new[] { (1, _budi), (1, _citra) },
            new[] { (1, _budi), (2, _budi) },
            new[] { (1, _andi) },
        })
        {
            var r = await Penugasan().SetBackupsAsync(_penugasan, new SetOnCallBackupsRequest { Items = items.Select(x => new SetOnCallBackupItemRequest { BackupSequence = x.Item1, WorkforceProfileId = x.Item2 }).ToList() }, _aktor);
            Assert.Equal((422, pesan), (r.StatusCode, r.Message));
        }

        await Cadangan(_budi, _citra);
        await Cadangan(_citra);
        var daftar = (await Penugasan().GetBackupsAsync(_penugasan)).Data!;
        Assert.Equal((1, _citra), (Assert.Single(daftar).BackupSequence, daftar[0].WorkforceProfileId));
        Assert.DoesNotContain(typeof(OnCallBackupResponse).GetProperties(), p => p.Name.Contains("Phone"));   // kontak hanya di NextBackup
    }

    // call-in lama kini menulis satu catatan panggilan Completed.
    [Fact]
    public async Task Call_in_lama_menulis_catatan_Completed()
    {
        var r = await Penugasan().RecordCallInAsync(_penugasan, new RecordOnCallCallInRequest { CallInTime = Wib(2, 10), ArrivalTime = Wib(2, 35), CalledByUnitId = _igd, CallNotes = "Pasien IGD" }, _lina);
        Assert.True(r.Success, r.Message);
        using var ctx = _db.CreateContext();
        var c = Assert.Single(ctx.HrdOnCallCalls.AsNoTracking().ToList());
        Assert.Equal(("Completed", 0, 25, _andi), (c.CallStatus, c.BackupSequence, c.ResponseMinutes!.Value, c.CalledWorkforceProfileId));
        Assert.Equal("Active", Baca().AssignmentStatus);
        Assert.Equal(25, Baca().ResponseMinutes);
    }

    // HRD-ASM-75 pada call-in lama: sesudah ada panggilan Diterima lebih awal, call-in tetap menulis catatan tetapi tidak menimpa ringkasan.
    [Fact]
    public async Task Call_in_sesudah_panggilan_diterima_tidak_menimpa_ringkasan()
    {
        var a = await Panggil(_andi, Wib(2, 10));
        var t = await Panggilan().AcceptAsync(_penugasan, a.Id, new AnswerOnCallCallRequest { AnsweredAt = Wib(2, 12), RowVersion = a.RowVersion }, _lina);
        await Panggilan().CompleteAsync(_penugasan, a.Id, new CompleteOnCallCallRequest { ArrivedAt = Wib(2, 35), RowVersion = t.Data!.RowVersion }, _lina, Wib(2, 36));

        var r = await Penugasan().RecordCallInAsync(_penugasan, new RecordOnCallCallInRequest { CallInTime = Wib(4, 0), ArrivalTime = Wib(4, 7), CalledByUnitId = _igd }, _lina);
        Assert.True(r.Success, r.Message);
        var p = Baca();
        Assert.Equal((true, 25, "Active"), (p.IsCalledIn, p.ResponseMinutes!.Value, p.AssignmentStatus));
        Assert.Equal(Wib(2, 10), DateTime.SpecifyKind(p.CallInTime!.Value, DateTimeKind.Utc));
        Assert.Equal(2, (await Panggilan().GetCallsAsync(_penugasan)).Data!.Count(x => x.CallStatus == "Completed"));
    }
}
