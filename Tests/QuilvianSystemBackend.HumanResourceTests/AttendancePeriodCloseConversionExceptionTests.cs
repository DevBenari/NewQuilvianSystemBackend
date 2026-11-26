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
using T = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.AttendanceExceptionType;
using X = QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Constants.AttendanceValueConstants.AttendanceExceptionStatus;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-464 — AT-HRD-PK-48 bagian harness (HRD-DEC-249, kontrak v31 API 37.3, VR-PK-31, state 35.2, INT-HR-120):
// pengecualian scan hilang pada hari calon konversi tidak memblokir tutup dan ditutup dalam transaksi tutup.
public sealed class AttendancePeriodCloseConversionExceptionTests : IDisposable
{
    private const string Kode = "ATP-2027-02";
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _icu = Guid.NewGuid(), _igd = Guid.NewGuid(), _aktor = Guid.NewGuid(), _periode = Guid.NewGuid();
    private static readonly DateOnly Mulai = new(2027, 2, 1), Selesai = new(2027, 2, 28);

    public AttendancePeriodCloseConversionExceptionTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _icu, LegalEntityId = Guid.NewGuid(), UnitCode = "ICU", UnitName = "ICU" });
        ctx.MstOrganizationUnits.Add(new MstOrganizationUnit { Id = _igd, LegalEntityId = Guid.NewGuid(), UnitCode = "IGD", UnitName = "IGD" });
        ctx.HrdAttendanceSettings.Add(new HrdAttendanceSetting { HospitalCode = "RSMMC", GracePeriodMinutes = 10, ScanWindowBeforeMinutes = 240, ScanWindowAfterMinutes = 480, IsActive = true });
        ctx.HrdAttendancePeriods.Add(new HrdAttendancePeriod
        {
            Id = _periode, PeriodCode = Kode, PeriodName = "Februari 2027", StartDate = Mulai, EndDate = Selesai,
            OrganizationUnitId = _icu, PeriodStatus = AttendanceValueConstants.AttendancePeriodStatus.Open, RequirePayrollHandoff = false,
        });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private Guid Harian(string nama, DateOnly tanggal, string status, Guid? unit = null)
    {
        using var ctx = _db.CreateContext();
        var pegawai = Guid.NewGuid();
        ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = pegawai, ProfileCode = "WFP-" + nama, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
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

    private Guid Pengecualian(Guid harian, string tipe, string status = X.Open, bool blokirPayroll = true, bool dihapus = false)
    {
        using var ctx = _db.CreateContext();
        var e = new HrdAttendanceException
        {
            AttendanceDailyId = harian, ExceptionCode = "EXC-" + tipe, ExceptionType = tipe, ExceptionStatus = status,
            Severity = "High", IsPayrollBlocking = blokirPayroll, IsDelete = dihapus, IsActive = true,
        };
        ctx.HrdAttendanceExceptions.Add(e);
        ctx.SaveChanges();
        return e.Id;
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

    private HrdAttendanceDaily BacaHarian(Guid id)
    {
        using var ctx = _db.CreateContext();
        return ctx.HrdAttendanceDailies.AsNoTracking().Single(x => x.Id == id);
    }

    private HrdAttendanceException BacaPengecualian(Guid id)
    {
        using var ctx = _db.CreateContext();
        return ctx.HrdAttendanceExceptions.AsNoTracking().Single(x => x.Id == id);
    }

    private string StatusPeriode()
    {
        using var ctx = _db.CreateContext();
        return ctx.HrdAttendancePeriods.AsNoTracking().Single().PeriodStatus;
    }

    private void TetapTerbuka(Guid pengecualian, string status = X.Open)
    {
        var e = BacaPengecualian(pengecualian);
        Assert.Equal(status, e.ExceptionStatus);
        Assert.Null(e.ResolvedByUserId);
        Assert.Null(e.ResolvedAt);
        Assert.Null(e.ResolutionNote);
    }

    // AC1: satu IncompletePunch + MissingCheckOut → pratinjau tanpa pemblokir, tutup 200, Absent, pengecualian Closed bercatatan.
    [Fact]
    public async Task Belum_Lengkap_dengan_MissingCheckOut_tidak_memblokir_dan_ditutup_saat_tutup()
    {
        var rina = Harian("Rina", new DateOnly(2027, 2, 10), S.IncompletePunch);
        var keluar = Pengecualian(rina, T.MissingCheckOut);

        var p = await Pratinjau();
        Assert.True(p.CanClose, string.Join("; ", p.Issues.Where(x => x.IsBlocking).Select(x => x.Message)));
        Assert.Equal(0, p.OpenPayrollBlockingException);
        Assert.Equal(1, p.ConversionExceptionCount);
        Assert.DoesNotContain(p.Issues, x => x.Code == "PAYROLL_BLOCKING_EXCEPTION");   // isu hanya muncul bila jumlahnya > 0

        var t = await Tutup();
        Assert.True(t.Success, t.Message);
        Assert.Equal(200, t.StatusCode);
        Assert.Equal(1, t.Data!.ConvertedIncompleteToAbsentCount);
        Assert.Equal(1, t.Data.ClosedConversionExceptionCount);
        Assert.Contains("ditutup 1 pengecualian", t.Message);

        Assert.Equal(S.Absent, BacaHarian(rina).AttendanceStatus);
        var e = BacaPengecualian(keluar);
        Assert.Equal(X.Closed, e.ExceptionStatus);
        Assert.Equal(_aktor, e.ResolvedByUserId);
        Assert.Equal(t.Data.ActionAt, e.ResolvedAt);
        Assert.Equal($"Diselesaikan oleh penutupan periode {Kode} (HRD-DEC-249): hari dikonversi menjadi Absent", e.ResolutionNote);
        Assert.Equal(_aktor, e.UpdateBy);
        Assert.Equal(AttendanceValueConstants.AttendancePeriodStatus.Closed, StatusPeriode());
    }

    // Ketiga tipe pada ketiga status calon konversi, Open maupun UnderReview, berpengaruh payroll maupun tidak, ikut ditutup.
    // Pengecualian yang sudah selesai, dihapus, atau di luar cakupan periode tidak disentuh.
    [Fact]
    public async Task Ketiga_tipe_pada_calon_konversi_ditutup_dan_yang_lain_tidak_disentuh()
    {
        var rina = Harian("Rina", new DateOnly(2027, 2, 10), S.IncompletePunch);
        var budi = Harian("Budi", new DateOnly(2027, 2, 12), S.NoScan);
        var sari = Harian("Sari", new DateOnly(2027, 2, 3), S.Incomplete);
        var andi = Harian("Andi", new DateOnly(2027, 2, 12), S.NoScan, _igd);          // di luar cakupan periode ICU

        var masuk = Pengecualian(rina, T.MissingCheckIn);
        var absen = Pengecualian(budi, T.Absent, X.UnderReview);
        var keluar = Pengecualian(sari, T.MissingCheckOut, blokirPayroll: false);
        var dimaafkan = Pengecualian(rina, T.MissingCheckOut, X.Waived);
        var dihapus = Pengecualian(budi, T.MissingCheckIn, dihapus: true);
        var unitLain = Pengecualian(andi, T.Absent);

        var p = await Pratinjau();
        Assert.True(p.CanClose, string.Join("; ", p.Issues.Where(x => x.IsBlocking).Select(x => x.Message)));
        Assert.Equal(0, p.OpenPayrollBlockingException);
        Assert.Equal(3, p.ConversionExceptionCount);

        var t = await Tutup();
        Assert.True(t.Success, t.Message);
        Assert.Equal(3, t.Data!.ConvertedIncompleteToAbsentCount);
        Assert.Equal(3, t.Data.ClosedConversionExceptionCount);
        foreach (var id in new[] { masuk, absen, keluar })
        {
            var e = BacaPengecualian(id);
            Assert.Equal(X.Closed, e.ExceptionStatus);
            Assert.Equal(_aktor, e.ResolvedByUserId);
        }
        TetapTerbuka(dimaafkan, X.Waived);
        TetapTerbuka(dihapus);
        TetapTerbuka(unitLain);
        Assert.Equal(S.NoScan, BacaHarian(andi).AttendanceStatus);
    }

    // AC2: ScheduleConflict pada hari calon konversi, atau MissingCheckOut pada hari yang bukan calon konversi → tetap 409.
    [Theory]
    [InlineData(S.IncompletePunch, T.ScheduleConflict)]
    [InlineData(S.Present, T.MissingCheckOut)]
    [InlineData(S.Late, T.MissingCheckIn)]
    public async Task Pengecualian_lain_tetap_memblokir_tutup(string statusHarian, string tipe)
    {
        var rina = Harian("Rina", new DateOnly(2027, 2, 10), statusHarian);
        var pemblokir = Pengecualian(rina, tipe);
        var budi = Harian("Budi", new DateOnly(2027, 2, 12), S.IncompletePunch);
        var konversi = Pengecualian(budi, T.MissingCheckOut);

        var p = await Pratinjau();
        Assert.False(p.CanClose);
        Assert.Equal(1, p.OpenPayrollBlockingException);
        Assert.Equal(1, p.ConversionExceptionCount);
        Assert.True(p.Issues.Single(x => x.Code == "PAYROLL_BLOCKING_EXCEPTION").IsBlocking);

        var t = await Tutup();
        Assert.False(t.Success);
        Assert.Equal(409, t.StatusCode);
        Assert.Equal("Masih terdapat attendance exception yang memblokir payroll.", t.Message);
        TetapTerbuka(pemblokir);
        TetapTerbuka(konversi);
        Assert.Equal(statusHarian, BacaHarian(rina).AttendanceStatus);
        Assert.Equal(S.IncompletePunch, BacaHarian(budi).AttendanceStatus);
        Assert.Equal(AttendanceValueConstants.AttendancePeriodStatus.Open, StatusPeriode());
    }

    // AC3: galat di tengah transaksi → periode, rekap, dan pengecualian tidak berubah.
    [Fact]
    public async Task Galat_di_tengah_transaksi_tidak_mengubah_pengecualian()
    {
        var rina = Harian("Rina", new DateOnly(2027, 2, 10), S.IncompletePunch);
        var keluar = Pengecualian(rina, T.MissingCheckOut);

        var t = await Tutup(new GagalSimpanKedua());
        Assert.False(t.Success);
        Assert.Equal(500, t.StatusCode);
        TetapTerbuka(keluar);
        Assert.Equal(S.IncompletePunch, BacaHarian(rina).AttendanceStatus);
        Assert.False(BacaHarian(rina).IsLocked);
        Assert.Equal(AttendanceValueConstants.AttendancePeriodStatus.Open, StatusPeriode());
    }

    // AC4: buka kembali periode tidak membuka ulang pengecualian dan tidak membatalkan konversi.
    [Fact]
    public async Task Buka_kembali_tidak_membuka_ulang_pengecualian()
    {
        var rina = Harian("Rina", new DateOnly(2027, 2, 10), S.IncompletePunch);
        var keluar = Pengecualian(rina, T.MissingCheckOut);
        var t = await Tutup();
        Assert.True(t.Success, t.Message);
        var ditutup = BacaPengecualian(keluar);

        var b = await Layanan().ReopenAsync(_periode, new ReopenAttendancePeriodRequest { Reason = "Koreksi presensi Rina tanggal 10" }, _aktor);
        Assert.True(b.Success, b.Message);
        Assert.Equal(AttendanceValueConstants.AttendancePeriodStatus.Reopened, StatusPeriode());

        var e = BacaPengecualian(keluar);
        Assert.Equal(X.Closed, e.ExceptionStatus);
        Assert.Equal(ditutup.ResolvedAt, e.ResolvedAt);
        Assert.Equal(ditutup.ResolutionNote, e.ResolutionNote);
        Assert.Equal(S.Absent, BacaHarian(rina).AttendanceStatus);
        Assert.False(BacaHarian(rina).IsLocked);

        // Pratinjau tutup berikutnya tidak lagi menghitung pengecualian itu.
        Assert.Equal(0, (await Pratinjau()).ConversionExceptionCount);
    }

    // Simpan pertama (status Closing) lolos; simpan kedua (konversi + pengecualian + Closed) gagal.
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
