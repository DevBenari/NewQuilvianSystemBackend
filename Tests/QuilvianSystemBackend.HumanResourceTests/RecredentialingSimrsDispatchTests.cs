using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Controllers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-235 / AT-HRD-OPP-22 — putusan rekredensialing dikirim ke tabel wewenang klinis bersama (INT-HR-76) dan dibaca resolver
// Laboratorium serta Kamar Operasi yang sesungguhnya. Revoked = hard-stop (VR-OPP-22), Restricted = supervisi level 3 (VR-OPP-21).
// dr. Bima (Sp.PK) memegang LAB-VAL-PK dan LAB-REL-PK aktif sejak 1 Januari 2026, seperti di devYoga.
public sealed class RecredentialingSimrsDispatchTests : IDisposable
{
    private static DateTime Tgl(int tahun, int bulan, int hari) => new(tahun, bulan, hari, 0, 0, 0, DateTimeKind.Utc);

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _bima = Guid.NewGuid(), _akunBima = Guid.NewGuid(), _ketua = Guid.NewGuid(), _tanpaBaris = Guid.NewGuid();
    private int _urut;

    public RecredentialingSimrsDispatchTests()
    {
        using var ctx = _db.CreateContext();
        Dokter(ctx, _bima, "dr. Bima Prasetya, Sp.PK");
        Dokter(ctx, _tanpaBaris, "dr. Tanpa Baris");
        ctx.Users.Add(new ApplicationUser { Id = _akunBima, UserName = "bima", UserCode = "BIMA0001", DisplayName = "dr. Bima", UserType = UserType.PermanentDoctor, IsActive = true, WorkforceProfileId = _bima });
        ctx.Users.Add(new ApplicationUser { Id = _ketua, UserName = "ketua", UserCode = "KETUA001", DisplayName = "Ketua Komite Medik", UserType = UserType.PermanentDoctor, IsActive = true });
        foreach (var (kode, nama) in new[] { ("LAB-VAL-PK", "Validasi hasil Patologi Klinik"), ("LAB-REL-PK", "Rilis hasil Patologi Klinik") })
        {
            ctx.WfpClinicalPrivileges.Add(new WfpClinicalPrivilege
            {
                WorkforceProfileId = _bima, PrivilegeCode = kode, PrivilegeName = nama, EffectiveStartDate = Tgl(2026, 1, 1),
                PrivilegeStatus = ClinicalPrivilegeStatus.Active, IsActive = true,
            });
        }
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private void Dokter(ApplicationDbContext ctx, Guid wf, string nama)
    {
        var n = ++_urut;
        ctx.MstDoctors.Add(new MstDoctor
        {
            WorkforceProfileId = wf, DoctorCode = $"DOK-{n:000}", DoctorNumber = $"DR-{n:000}", FullName = nama, WorkforceTypeId = Guid.NewGuid(),
            EmployeeCategoryId = Guid.NewGuid(), EmploymentTypeId = Guid.NewGuid(), EmploymentStatusId = Guid.NewGuid(), IsActive = true,
            ClinicalPrivilegeStatus = ClinicalPrivilegeStatus.Active,
        });
    }

    private async Task<Guid> Putuskan(Guid wf, string putusan)
    {
        using var ctx = _db.CreateContext();
        var r = await new RecredentialingQualityService(ctx).IssueVerdictAsync(new CreateRecredVerdictRequestDto
        { WorkforceProfileId = wf, Verdict = putusan, CommitteeNotes = "Berita acara sidang pleno Komite Medik uji.", ApprovedByCommitteeChairmanId = _ketua });
        return r.Id;
    }

    private async Task<SimrsPrivilegeSyncAuditLogDto> Kirim(Guid verdictId, int level)
    {
        using var ctx = _db.CreateContext();
        return await new SimrsClinicalPrivilegeDispatcher(ctx).DispatchToSimrsAsync(verdictId, new DispatchSimrsPrivilegeRequestDto { VerdictId = verdictId, DoctorCode = "DOK-001", Level = level });
    }

    private List<WfpClinicalPrivilege> Baris(Guid wf)
    {
        using var ctx = _db.CreateContext();
        return ctx.WfpClinicalPrivileges.AsNoTracking().Where(x => x.WorkforceProfileId == wf).OrderBy(x => x.PrivilegeCode).ToList();
    }

    private async Task<(LabPrivilegeCheck Validasi, LabPrivilegeCheck Rilis, OprCredentialCheckStatus KamarOperasi)> Gerbang()
    {
        using var ctx = _db.CreateContext();
        var lab = new LabClinicalPrivilegeResolver(ctx);
        var v = await lab.ResolveAsync(_akunBima, LabDiscipline.ClinicalPathology, LabPrivilegeKind.Validation, DateTime.UtcNow);
        var r = await lab.ResolveAsync(_akunBima, LabDiscipline.ClinicalPathology, LabPrivilegeKind.Release, DateTime.UtcNow);
        var ok = (await new OperatingRoomCredentialResolver(ctx).ResolveAsync(new[] { _bima }, DateTime.UtcNow))[_bima];
        return (v, r, ok);
    }

    private int JumlahLog()
    {
        using var ctx = _db.CreateContext();
        return ctx.HrdSimrsPrivilegeSyncLogs.Count();
    }

    // AT-HRD-OPP-22: sebelum putusan dokter boleh memvalidasi dan merilis; sesudah putusan Revoked dikirim, seluruh baris wewenangnya
    // dicabut dan terblokir, gerbang Laboratorium dan Kamar Operasi sama-sama menolak (fail-closed), dan status dokter Revoked.
    [Fact]
    public async Task OPP22_Revoked_mengunci_Laboratorium_dan_Kamar_Operasi_seketika()
    {
        var sebelum = await Gerbang();
        Assert.True(sebelum.Validasi.IsGranted);
        Assert.True(sebelum.Rilis.IsGranted);
        Assert.Equal(OprCredentialCheckStatus.Valid, sebelum.KamarOperasi);

        var putusan = await Putuskan(_bima, "Revoked");
        var log = await Kirim(putusan, 1);

        Assert.Equal(("SYNCED", 200, true), (log.Status, log.ResponseStatusCode, log.IsHardStopBlocked));
        Assert.Contains("2 baris wewenang klinis diperbarui", log.Message);
        Assert.All(Baris(_bima), b =>
        {
            Assert.Equal(ClinicalPrivilegeStatus.Revoked, b.PrivilegeStatus);
            Assert.True(b.IsClinicalServiceBlocked && b.IsSchedulingBlocked);
            Assert.NotNull(b.RevokedAt);
            Assert.Equal(_ketua, b.RevokedByUserId);
            Assert.Contains("Revoked", b.RevocationReason);
        });

        var sesudah = await Gerbang();
        Assert.False(sesudah.Validasi.IsGranted);
        Assert.False(sesudah.Rilis.IsGranted);
        Assert.False(string.IsNullOrWhiteSpace(sesudah.Validasi.Message));
        Assert.Equal(OprCredentialCheckStatus.Invalid, sesudah.KamarOperasi);

        using var ctx = _db.CreateContext();
        Assert.Equal(ClinicalPrivilegeStatus.Revoked, ctx.MstDoctors.AsNoTracking().Single(x => x.WorkforceProfileId == _bima).ClinicalPrivilegeStatus);
    }

    // VR-OPP-22: putusan Revoked tidak boleh terkirim sebagai level lain. Level bawaan DTO adalah 4; sebelum perbaikan, Revoked yang
    // dikirim tanpa level justru memulihkan wewenang mandiri. Kini ditolak (400 lewat controller), tanpa baris berubah dan tanpa log.
    [Fact]
    public async Task Revoked_dengan_level_selain_1_ditolak_tanpa_perubahan()
    {
        var putusan = await Putuskan(_bima, "Revoked");
        foreach (var level in new[] { 4, 3, 2 })
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => Kirim(putusan, level));
            Assert.Contains($"Level {level} tidak sesuai putusan Revoked", ex.Message);
        }
        Assert.All(Baris(_bima), b => Assert.Equal((ClinicalPrivilegeStatus.Active, false, true), (b.PrivilegeStatus, b.IsClinicalServiceBlocked, b.IsActive)));
        Assert.Equal(0, JumlahLog());

        var c = new RecredentialingQualityController(new RecredentialingQualityService(_db.CreateContext()), new SimrsClinicalPrivilegeDispatcher(_db.CreateContext()));
        var hasil = Assert.IsAssignableFrom<ObjectResult>(await c.DispatchToSimrs(putusan, new DispatchSimrsPrivilegeRequestDto { VerdictId = putusan, DoctorCode = "DOK-001" }));
        Assert.Equal(400, hasil.StatusCode);
        Assert.True((await Gerbang()).Validasi.IsGranted);
    }

    // VR-OPP-21: Restricted = level 3 supervisi; dokter tetap boleh bertindak dengan kewajiban co-sign. Level lain ditolak. Upgrade
    // sesudahnya (level 4) mengangkat kewajiban supervisi.
    [Fact]
    public async Task Restricted_level_3_supervisi_lalu_Upgrade_level_4_mengangkat_supervisi()
    {
        var dibatasi = await Putuskan(_bima, "Restricted");
        await Assert.ThrowsAsync<InvalidOperationException>(() => Kirim(dibatasi, 4));
        var log = await Kirim(dibatasi, 3);
        Assert.Equal(("SYNCED", false), (log.Status, log.IsHardStopBlocked));
        Assert.All(Baris(_bima), b => Assert.Equal((ClinicalPrivilegeStatus.Active, true, false), (b.PrivilegeStatus, b.IsSupervisionRequired, b.IsClinicalServiceBlocked)));
        var g = await Gerbang();
        Assert.Equal((true, OprCredentialCheckStatus.Valid), (g.Validasi.IsGranted, g.KamarOperasi));

        var naik = await Putuskan(_bima, "Upgrade");
        await Assert.ThrowsAsync<InvalidOperationException>(() => Kirim(naik, 1));
        await Kirim(naik, 4);
        Assert.All(Baris(_bima), b => Assert.False(b.IsSupervisionRequired));
    }

    // Revoked yang sudah dieksekusi tidak dipulihkan otomatis oleh putusan level 4 berikutnya (pemulihan lewat kredensialing ulang).
    [Fact]
    public async Task Revoked_tidak_dipulihkan_oleh_RenewAsIs_level_4()
    {
        await Kirim(await Putuskan(_bima, "Revoked"), 1);
        await Kirim(await Putuskan(_bima, "RenewAsIs"), 4);
        Assert.All(Baris(_bima), b => Assert.Equal((ClinicalPrivilegeStatus.Revoked, true), (b.PrivilegeStatus, b.IsClinicalServiceBlocked)));
        Assert.False((await Gerbang()).Validasi.IsGranted);
    }

    // Dokter tanpa baris wewenang klinis: tercatat NO_PRIVILEGE_ROWS (404) dengan pesan; putusan yang tidak pernah disahkan ditolak.
    [Fact]
    public async Task Tanpa_baris_wewenang_tercatat_404_dan_putusan_fiktif_ditolak()
    {
        var log = await Kirim(await Putuskan(_tanpaBaris, "Revoked"), 1);
        Assert.Equal(("NO_PRIVILEGE_ROWS", 404), (log.Status, log.ResponseStatusCode));
        Assert.Contains("belum memiliki baris wewenang klinis", log.Message);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => Kirim(Guid.NewGuid(), 1));
    }

    [Fact]
    public void Peta_level_menurut_putusan()
    {
        Assert.Equal(((int?)1, (int?)3, (int?)4, (int?)4), (SimrsClinicalPrivilegeDispatcher.RequiredLevelFor("Revoked"), SimrsClinicalPrivilegeDispatcher.RequiredLevelFor("Restricted"),
            SimrsClinicalPrivilegeDispatcher.RequiredLevelFor("RenewAsIs"), SimrsClinicalPrivilegeDispatcher.RequiredLevelFor("Upgrade")));
        Assert.Null(SimrsClinicalPrivilegeDispatcher.RequiredLevelFor("Lainnya"));
    }
}
