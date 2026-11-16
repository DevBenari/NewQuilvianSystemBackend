using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Enums;
using S = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.AttendanceStatus;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-344 — AT-HRD-PK-22, -23, -31 (HRD-DEC-197, HRD-DEC-216, HRD-ASM-81; state matrix 30.6).
public sealed class ClosePeriodTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _icu = Guid.NewGuid(), _igd = Guid.NewGuid(), _aktor = Guid.NewGuid(), _periode = Guid.NewGuid();
    private static readonly DateOnly Mulai = new(2027, 2, 1), Selesai = new(2027, 2, 28);

    private static DateTime Wib(DateOnly d, int jam, int menit = 0) => new DateTime(d.Year, d.Month, d.Day, jam, menit, 0, DateTimeKind.Utc).AddHours(-7);

    public ClosePeriodTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _icu, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _igd, LegalEntityId = Guid.NewGuid(), UnitCode = "IGD", UnitName = "IGD" });
        ctx.HrdAttendanceSettings.Add(new HrdAttendanceSetting { HospitalCode = "RSMMC", GracePeriodMinutes = 10, ScanWindowBeforeMinutes = 240, ScanWindowAfterMinutes = 480, IsActive = true });
        ctx.HrdAttendancePeriods.Add(new HrdAttendancePeriod
        {
            Id = _periode, PeriodCode = "ATP-2027-02", PeriodName = "Februari 2027", StartDate = Mulai, EndDate = Selesai,
            OrganizationUnitId = _icu, PeriodStatus = AttendanceValueConstants.AttendancePeriodStatus.Open, RequirePayrollHandoff = false,
        });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private Guid Pegawai(string nama)
    {
        using var ctx = _db.CreateContext();
        var id = Guid.NewGuid();
        ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = "WFP-" + nama, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
        ctx.SaveChanges();
        return id;
    }

    private Guid Harian(Guid pegawai, DateOnly tanggal, string status, Guid? unit = null)
    {
        using var ctx = _db.CreateContext();
        var d = new HrdAttendanceDaily
        {
            UserId = Guid.NewGuid(), WorkforceProfileId = pegawai, UserType = UserType.Employee, OrganizationUnitId = unit ?? _icu,
            AttendanceDate = tanggal, AttendanceStatus = status, IsPresent = status == S.Present,
            ActualWorkMinutes = 120, PayableWorkMinutes = 120,
            ProcessingStatus = AttendanceValueConstants.AttendanceProcessingStatus.Processed, IsActive = true,
        };
        ctx.HrdAttendanceDailies.Add(d);
        ctx.SaveChanges();
        return d.Id;
    }

    private void ScanTertahan(DateTime waktu, Guid? site = null)
    {
        using var ctx = _db.CreateContext();
        ctx.HrdAttendanceRawLogs.Add(new HrdAttendanceRawLog
        {
            EventAt = waktu, EventType = "CheckIn", SourceType = AttendanceValueConstants.RawLogSourceType.Device, ExternalDeviceId = "IGD-99",
            HospitalSiteId = site, ProcessingStatus = AttendanceValueConstants.RawLogProcessingStatus.AwaitingDeviceRegistration,
        });
        ctx.SaveChanges();
    }

    private AttendancePeriodService Layanan(params IInterceptor[] i) => new(_db.CreateContext(i), NullLogger<AttendancePeriodService>.Instance);

    private async Task<AttendancePeriodClosePreviewResponse> Pratinjau()
    {
        var r = await Layanan().PreviewCloseAsync(_periode);
        Assert.True(r.Success, r.Message);
        return r.Data!;
    }

    private Task<AttendancePeriodSchedulerServiceResult<AttendancePeriodActionResponse>> Tutup(params IInterceptor[] i) =>
        Layanan(i).CloseAsync(_periode, new CloseAttendancePeriodRequest { Reason = "Tutup periode Februari 2027" }, _aktor);

    private HrdAttendanceDaily Baca(Guid id)
    {
        using var ctx = _db.CreateContext();
        return ctx.HrdAttendanceDailies.AsNoTracking().Single(x => x.Id == id);
    }

    // AT-HRD-PK-22: pratinjau memuat IncompletePunch, NoScan, dan Incomplete lama dengan nama, unit, tanggal; tutup → ketiganya Absent.
    [Fact]
    public async Task Tutup_mengonversi_Belum_Lengkap_Tidak_Ada_Scan_dan_nilai_lama()
    {
        var rina = Harian(Pegawai("Rina"), new DateOnly(2027, 2, 10), S.IncompletePunch);
        var budi = Harian(Pegawai("Budi"), new DateOnly(2027, 2, 12), S.NoScan);
        var sari = Harian(Pegawai("Sari"), new DateOnly(2027, 2, 3), S.Incomplete);
        var dodi = Harian(Pegawai("Dodi"), new DateOnly(2027, 2, 12), S.Present);
        var cuti = Harian(Pegawai("Tono"), new DateOnly(2027, 2, 12), S.Leave);
        var belum = Harian(Pegawai("Nadia"), new DateOnly(2027, 2, 14), S.NotYetPresent);
        var lainUnit = Harian(Pegawai("Andi"), new DateOnly(2027, 2, 12), S.NoScan, _igd);          // di luar cakupan periode ICU
        var lainTanggal = Harian(Pegawai("Dewi"), new DateOnly(2027, 3, 1), S.IncompletePunch);    // di luar tanggal periode

        var p = await Pratinjau();
        Assert.True(p.CanClose, string.Join("; ", p.Issues.Where(x => x.IsBlocking).Select(x => x.Message)));
        Assert.Equal(3, p.ConversionCandidateCount);
        Assert.False(p.IsConversionCandidateListTruncated);
        Assert.Equal(new[] { sari, rina, budi }, p.ConversionCandidates.Select(x => x.AttendanceDailyId));   // urut tanggal
        var r = p.ConversionCandidates.Single(x => x.AttendanceDailyId == rina);
        Assert.Equal("Rina", r.EmployeeName);
        Assert.Equal("WFP-Rina", r.ProfileCode);
        Assert.Equal("ICU", r.OrganizationUnitName);
        Assert.Equal(_icu, r.OrganizationUnitId);
        Assert.Equal(new DateOnly(2027, 2, 10), r.AttendanceDate);
        Assert.Equal(S.IncompletePunch, r.AttendanceStatus);
        Assert.Equal(S.NoScan, p.ConversionCandidates.Single(x => x.AttendanceDailyId == budi).AttendanceStatus);
        Assert.Equal(S.Incomplete, p.ConversionCandidates.Single(x => x.AttendanceDailyId == sari).AttendanceStatus);
        var isu = p.Issues.Single(x => x.Code == "INCOMPLETE_PUNCH_ATTENDANCE");
        Assert.Equal(3, isu.Count);
        Assert.False(isu.IsBlocking);

        var t = await Tutup();
        Assert.True(t.Success, t.Message);
        Assert.Equal(3, t.Data!.ConvertedIncompleteToAbsentCount);
        Assert.Equal(6, t.Data.AffectedAttendanceDailyCount);
        foreach (var id in new[] { rina, budi, sari })
        {
            var d = Baca(id);
            Assert.Equal(S.Absent, d.AttendanceStatus);
            Assert.True(d.IsAbsent);
            Assert.False(d.IsPresent);
            Assert.Equal(0, d.PayableWorkMinutes);
            Assert.True(d.IsLocked);
            Assert.Contains("HRD-DEC-216", d.IncompletePunchReason);
        }
        Assert.Equal(S.Present, Baca(dodi).AttendanceStatus);
        Assert.Equal(S.Leave, Baca(cuti).AttendanceStatus);
        Assert.Equal(S.NotYetPresent, Baca(belum).AttendanceStatus);   // tidak termasuk 30.6
        Assert.Equal(S.NoScan, Baca(lainUnit).AttendanceStatus);
        Assert.False(Baca(lainUnit).IsLocked);
        Assert.Equal(S.IncompletePunch, Baca(lainTanggal).AttendanceStatus);

        // Periode yang sudah ditutup ditolak 409 dan tidak mengubah apa pun.
        var ulang = await Tutup();
        Assert.False(ulang.Success);
        Assert.Equal(409, ulang.StatusCode);
    }

    // AT-HRD-PK-23: AutoConvertIncompleteToAbsent = false di basis data tidak menghentikan konversi.
    [Fact]
    public async Task Setelan_lama_mati_tetap_dikonversi()
    {
        using (var ctx = _db.CreateContext())
        {
            var s = ctx.HrdAttendanceSettings.Single();
            s.AutoConvertIncompleteToAbsent = false;
            ctx.SaveChanges();
        }
        var rina = Harian(Pegawai("Rina"), new DateOnly(2027, 2, 10), S.IncompletePunch);
        var budi = Harian(Pegawai("Budi"), new DateOnly(2027, 2, 12), S.NoScan);

        Assert.Equal(2, (await Pratinjau()).ConversionCandidateCount);
        var t = await Tutup();
        Assert.True(t.Success, t.Message);
        Assert.Equal(2, t.Data!.ConvertedIncompleteToAbsentCount);
        Assert.Equal(S.Absent, Baca(rina).AttendanceStatus);
        Assert.Equal(S.Absent, Baca(budi).AttendanceStatus);
    }

    // AT-HRD-PK-31: scan tertahan dihitung (batas hari WIB), hanya peringatan; tutup tetap diizinkan.
    [Fact]
    public async Task Scan_tertahan_dihitung_tanpa_menghalangi_tutup()
    {
        Harian(Pegawai("Dodi"), new DateOnly(2027, 2, 12), S.Present);
        ScanTertahan(Wib(Mulai, 0, 30));                      // 31 Jan 17.30 UTC — masuk (tanggal WIB 1 Feb)
        ScanTertahan(Wib(Selesai, 23, 30));                   // masuk
        ScanTertahan(Wib(Selesai.AddDays(1), 0, 30));         // 1 Mar WIB — di luar
        ScanTertahan(Wib(Mulai.AddDays(-1), 23, 59));         // 31 Jan WIB — di luar
        using (var ctx = _db.CreateContext())
        {
            // scan mesin terdaftar yang sudah diproses bukan scan tertahan
            ctx.HrdAttendanceRawLogs.Add(new HrdAttendanceRawLog
            {
                EventAt = Wib(Mulai, 8), EventType = "CheckIn", SourceType = AttendanceValueConstants.RawLogSourceType.Device,
                ProcessingStatus = AttendanceValueConstants.RawLogProcessingStatus.Processed,
            });
            ctx.SaveChanges();
        }

        var p = await Pratinjau();
        Assert.Equal(2, p.HeldDeviceScanCount);
        Assert.True(p.CanClose);
        var isu = p.Issues.Single(x => x.Code == "HELD_DEVICE_SCAN");
        Assert.Equal(2, isu.Count);
        Assert.False(isu.IsBlocking);
        Assert.Equal(0, p.ConversionCandidateCount);
        Assert.Empty(p.ConversionCandidates);

        var t = await Tutup();
        Assert.True(t.Success, t.Message);
        Assert.Equal(0, t.Data!.ConvertedIncompleteToAbsentCount);
    }

    // Periode berlingkup rumah sakit: scan tertahan tanpa rumah sakit ikut dihitung, rumah sakit lain tidak.
    [Fact]
    public async Task Scan_tertahan_mengikuti_rumah_sakit_periode()
    {
        var rs = Guid.NewGuid();
        using (var ctx = _db.CreateContext())
        {
            ctx.HrdAttendancePeriods.Single().HospitalSiteId = rs;
            ctx.SaveChanges();
        }
        ScanTertahan(Wib(Mulai, 8));
        ScanTertahan(Wib(Mulai, 9), rs);
        ScanTertahan(Wib(Mulai, 10), Guid.NewGuid());
        Assert.Equal(2, (await Pratinjau()).HeldDeviceScanCount);
    }

    // Risiko kartu: daftar dibatasi, jumlah total tetap lengkap.
    [Fact]
    public async Task Daftar_panjang_dibatasi_dengan_jumlah_total()
    {
        using (var ctx = _db.CreateContext())
        {
            for (var i = 0; i < 501; i++)
            {
                var id = Guid.NewGuid();
                ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = $"P{i:000}", DisplayName = $"Pegawai {i:000}", UserType = UserType.Employee, IsActive = true });
                ctx.HrdAttendanceDailies.Add(new HrdAttendanceDaily
                {
                    UserId = Guid.NewGuid(), WorkforceProfileId = id, UserType = UserType.Employee, OrganizationUnitId = _icu,
                    AttendanceDate = Mulai.AddDays(i % 28), AttendanceStatus = S.NoScan,
                    ProcessingStatus = AttendanceValueConstants.AttendanceProcessingStatus.Processed, IsActive = true,
                });
            }
            ctx.SaveChanges();
        }

        var p = await Pratinjau();
        Assert.Equal(501, p.ConversionCandidateCount);
        Assert.Equal(500, p.ConversionCandidates.Count);
        Assert.True(p.IsConversionCandidateListTruncated);
        var t = await Tutup();
        Assert.True(t.Success, t.Message);
        Assert.Equal(501, t.Data!.ConvertedIncompleteToAbsentCount);   // konversi tidak terbatas oleh daftar pratinjau
    }

    // Jalur gagal: galat di tengah transaksi → tidak ada status yang berubah, periode tetap Open.
    [Fact]
    public async Task Galat_saat_tutup_membatalkan_seluruh_konversi()
    {
        var rina = Harian(Pegawai("Rina"), new DateOnly(2027, 2, 10), S.IncompletePunch);
        var budi = Harian(Pegawai("Budi"), new DateOnly(2027, 2, 12), S.NoScan);

        var t = await Tutup(new GagalSimpanKedua());
        Assert.False(t.Success);
        Assert.Equal(500, t.StatusCode);
        Assert.Equal(S.IncompletePunch, Baca(rina).AttendanceStatus);
        Assert.Equal(S.NoScan, Baca(budi).AttendanceStatus);
        Assert.False(Baca(rina).IsLocked);
        using var ctx = _db.CreateContext();
        Assert.Equal(AttendanceValueConstants.AttendancePeriodStatus.Open, ctx.HrdAttendancePeriods.AsNoTracking().Single().PeriodStatus);
    }

    // Simpan pertama (status Closing) lolos; simpan kedua (konversi + Closed) gagal.
    private sealed class GagalSimpanKedua : SaveChangesInterceptor
    {
        private int _ke;
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (++_ke == 2) throw new InvalidOperationException("Galat uji di tengah transaksi tutup");
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
