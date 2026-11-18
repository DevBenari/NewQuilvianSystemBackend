using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-362 — simpan penetapan gaji utama dan rekening payroll satu orang (kontrak v27 §33, VR-WPD-01..16, Aturan C–E,
// HRD-DEC-218). Grade G-10 Rp 10.000.000 – Rp 16.000.000; struktur SS-01 aktif, SS-LAMA nonaktif; Bank BCA aktif, Bank Tutup nonaktif.
public sealed class WorkforcePayrollDataSaveTests : IDisposable
{
    private static DateTime Tgl(int tahun, int bulan, int hari) => new(tahun, bulan, hari, 0, 0, 0, DateTimeKind.Utc);

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _struktur = Guid.NewGuid(), _strukturNonaktif = Guid.NewGuid(), _grade = Guid.NewGuid(), _bca = Guid.NewGuid(), _bankTutup = Guid.NewGuid();
    private readonly Guid _dept = Guid.NewGuid(), _pos = Guid.NewGuid(), _aktor = Guid.NewGuid(), _super = Guid.NewGuid();
    private readonly PenangkapLog _log = new();
    private int _urut;

    public WorkforcePayrollDataSaveTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstDepartments.Add(new MstDepartment { Id = _dept, DepartmentCode = "ICU", DepartmentName = "Instalasi ICU" });
        ctx.MstPositions.Add(new MstPosition { Id = _pos, DepartmentId = _dept, PositionCode = "PLK", PositionName = "Pelaksana" });
        ctx.Set<MstSalaryGrade>().Add(new MstSalaryGrade { Id = _grade, SalaryGradeCode = "G-10", SalaryGradeName = "Grade 10", MinimumSalary = 10_000_000m, MaximumSalary = 16_000_000m });
        ctx.Set<MstSalaryStructure>().AddRange(
            new MstSalaryStructure { Id = _struktur, SalaryGradeId = _grade, SalaryStructureCode = "SS-01", SalaryStructureName = "Struktur Staf" },
            new MstSalaryStructure { Id = _strukturNonaktif, SalaryGradeId = _grade, SalaryStructureCode = "SS-LAMA", SalaryStructureName = "Struktur Lama", IsActive = false });
        ctx.MstBanks.AddRange(
            new MstBank { Id = _bca, BankCode = "BCA", BankName = "Bank Central Asia" },
            new MstBank { Id = _bankTutup, BankCode = "TTP", BankName = "Bank Tutup", IsActive = false });
        ApplicationUser Akun(Guid id, UserType tipe) => new() { Id = id, UserName = id.ToString(), UserCode = id.ToString("N")[..8], DisplayName = "Uji", UserType = tipe, IsActive = true };
        ctx.Users.AddRange(Akun(_aktor, UserType.Employee), Akun(_super, UserType.SuperAdmin));
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private sealed class PenangkapLog : Microsoft.Extensions.Logging.ILogger<LoggerService>
    {
        public List<string> Pesan { get; } = new();
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => true;
        public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) => Pesan.Add(formatter(state, exception));
    }

    private Guid Karyawan(string nama)
    {
        using var ctx = _db.CreateContext();
        var id = Guid.NewGuid();
        var n = ++_urut;
        ctx.MstEmployees.Add(new MstEmployee
        {
            WorkforceProfileId = id, EmployeeCode = $"EMP-{n:000}", EmployeeNumber = $"NIP-{n:000}", FullName = nama, IdentityType = "KTP", IdentityNumber = $"3171{n:000000000000}",
            Email = $"k{n}@uji.local", BirthDate = Tgl(1990, 1, 1), JoinDate = Tgl(2020, 1, 1), PrimaryDepartmentId = _dept, PrimaryPositionId = _pos,
            WorkforceTypeId = Guid.NewGuid(), EmployeeCategoryId = Guid.NewGuid(), EmploymentTypeId = Guid.NewGuid(), EmploymentStatusId = Guid.NewGuid(), IsActive = true,
        });
        ctx.SaveChanges();
        return id;
    }

    private Guid Gaji(Guid wf, decimal pokok, DateTime mulai, bool disetujui = false, bool utama = true, DateTime? berakhir = null)
    {
        using var ctx = _db.CreateContext();
        var a = new WfpSalaryAssignment
        {
            WorkforceProfileId = wf, SalaryStructureId = _struktur, SalaryGradeId = _grade, BaseSalary = pokok, EffectiveStartDate = mulai, EffectiveEndDate = berakhir, IsPrimary = utama,
            ApprovedAt = disetujui ? Tgl(2026, 1, 5) : null, ApprovedByUserId = disetujui ? Guid.NewGuid() : null,
        };
        ctx.WfpSalaryAssignments.Add(a);
        ctx.SaveChanges();
        return a.Id;
    }

    private Guid Rekening(Guid wf, string nomor, bool payroll = true, bool terverifikasi = false, bool utama = true)
    {
        using var ctx = _db.CreateContext();
        var r = new WfpBankAccount
        {
            WorkforceProfileId = wf, BankId = _bca, BankName = "Bank Central Asia", AccountNumber = nomor, AccountHolderName = "Pemilik Lama", IsPayrollAccount = payroll, IsPrimary = utama,
            IsVerified = terverifikasi, VerifiedAt = terverifikasi ? Tgl(2026, 1, 6) : null,
        };
        ctx.WfpBankAccounts.Add(r);
        ctx.SaveChanges();
        return r.Id;
    }

    private WorkforcePayrollDataService Layanan() =>
        new(_db.CreateContext(), new LoggerService(_log, Pelaku.Sebagai(_aktor)));

    private Task<WorkforcePayrollDataSaveResponse> Simpan(Guid wf, UpdateWorkforcePayrollDataRequest r) => Layanan().SaveAsync(wf, r, _aktor, CancellationToken.None);

    private async Task<WorkforcePayrollDataException> Tolak(Guid wf, UpdateWorkforcePayrollDataRequest r) =>
        await Assert.ThrowsAsync<WorkforcePayrollDataException>(() => Simpan(wf, r));

    private UpdateWorkforcePayrollSalaryRequest GajiBaru(decimal pokok, DateTime? mulai = null, Guid? struktur = null) =>
        new() { SalaryStructureId = struktur ?? _struktur, BaseSalary = pokok, EffectiveStartDate = mulai ?? Tgl(2026, 1, 1) };

    private UpdateWorkforcePayrollBankRequest RekeningBaru(string nomor, string pemilik = "Sari Lestari", Guid? bank = null) =>
        new() { BankId = bank ?? _bca, AccountNumber = nomor, AccountHolderName = pemilik };

    private (List<WfpSalaryAssignment> Gaji, List<WfpBankAccount> Rekening) Baca(Guid wf)
    {
        using var ctx = _db.CreateContext();
        return (ctx.WfpSalaryAssignments.AsNoTracking().Where(x => x.WorkforceProfileId == wf).ToList(),
            ctx.WfpBankAccounts.AsNoTracking().Where(x => x.WorkforceProfileId == wf).ToList());
    }

    // AC6 + label hasil kontrak: penetapan dan rekening baru dibuat; simpan ulang yang sama → "Tidak diubah" tanpa log.
    [Fact]
    public async Task AC6_penetapan_dan_rekening_baru_dibuat_lalu_simpan_ulang_tidak_diubah()
    {
        var sari = Karyawan("Sari");
        var r = await Simpan(sari, new UpdateWorkforcePayrollDataRequest { Salary = GajiBaru(12_500_000m), BankAccount = RekeningBaru(" 1234-5678 90 ", "  Sari Lestari ") });
        Assert.Equal(("Dibuat", "Dibuat"), (r.SalaryAssignment, r.BankAccount));
        Assert.Empty(r.Warnings);

        var (gaji, rek) = Baca(sari);
        var g = Assert.Single(gaji);
        Assert.Equal((12_500_000m, true, true, _grade), (g.BaseSalary, g.IsPrimary, g.IsConfidential, g.SalaryGradeId));
        Assert.Null(g.ApprovedAt);
        var a = Assert.Single(rek);
        Assert.Equal(("1234567890", "Sari Lestari", "Bank Central Asia", true, true, false), (a.AccountNumber, a.AccountHolderName, a.BankName, a.IsPayrollAccount, a.IsPrimary, a.IsVerified));

        var ulang = await Simpan(sari, new UpdateWorkforcePayrollDataRequest { Salary = GajiBaru(12_500_000m), BankAccount = RekeningBaru("1234567890") });
        Assert.Equal(("Tidak diubah", "Tidak diubah"), (ulang.SalaryAssignment, ulang.BankAccount));
        Assert.Single(_log.Pesan);   // hanya simpan pertama yang dicatat
    }

    // AC1 + VR-WPD-11: permintaan tanpa bagian dan tanpa opsi ditolak; bagian null tidak disentuh.
    [Fact]
    public async Task AC1_permintaan_kosong_ditolak_dan_bagian_null_tidak_disentuh()
    {
        var budi = Karyawan("Budi");
        Gaji(budi, 12_000_000m, Tgl(2025, 1, 1), disetujui: true);

        var kosong = await Tolak(budi, new UpdateWorkforcePayrollDataRequest());
        Assert.Equal((400, "Tidak ada perubahan untuk disimpan."), (kosong.StatusCode, kosong.Message));

        var r = await Simpan(budi, new UpdateWorkforcePayrollDataRequest { BankAccount = RekeningBaru("998877") });
        Assert.Equal(("Tidak diubah", "Dibuat"), (r.SalaryAssignment, r.BankAccount));
        var g = Assert.Single(Baca(budi).Gaji);
        Assert.Equal(12_000_000m, g.BaseSalary);
        Assert.NotNull(g.ApprovedAt);
    }

    // AC2 + Aturan C (contoh roadmap): nomor rekening yang sudah tercatat di baris lain ditolak, dan gaji yang diisi bersamaan juga
    // tidak tersimpan. Seluruh kesalahan dilaporkan sekaligus.
    [Fact]
    public async Task AC2_satu_isian_ditolak_maka_tidak_ada_yang_tersimpan()
    {
        var sari = Karyawan("Sari");
        Rekening(sari, "1234567890", payroll: false, utama: false);

        var ex = await Tolak(sari, new UpdateWorkforcePayrollDataRequest { Salary = GajiBaru(12_000_000m), BankAccount = RekeningBaru("1234-567-890") });
        Assert.Equal((400, "Nomor rekening sudah tercatat untuk orang ini."), (ex.StatusCode, ex.Message));
        var (gaji, rek) = Baca(sari);
        Assert.Empty(gaji);
        Assert.Single(rek);

        var banyak = await Tolak(sari, new UpdateWorkforcePayrollDataRequest
        {
            Salary = new UpdateWorkforcePayrollSalaryRequest { SalaryStructureId = _strukturNonaktif, BaseSalary = 0m, EffectiveStartDate = null },
            BankAccount = new UpdateWorkforcePayrollBankRequest { BankId = _bankTutup, AccountNumber = " - ", AccountHolderName = new string('A', 201) },
        });
        foreach (var pesan in new[] { "Gaji pokok harus lebih dari nol.", "Tanggal mulai berlaku wajib diisi.", "Struktur gaji tidak ditemukan atau tidak aktif.",
                     "Bank tidak ditemukan atau tidak aktif.", "Nomor rekening wajib diisi.", "Nama pemilik rekening paling panjang 200 karakter." })
            Assert.Contains(pesan, banyak.Message);
        Assert.Empty(Baca(sari).Gaji);
        Assert.Empty(_log.Pesan);
    }

    // VR-WPD-05, -06, -13: tanggal mulai melewati tanggal berakhir; periode beririsan dengan struktur dan grade sama; opsi tanpa data.
    [Fact]
    public async Task Aturan_periode_dan_opsi_tanpa_data()
    {
        var andi = Karyawan("Andi");
        Gaji(andi, 12_000_000m, Tgl(2026, 1, 1), berakhir: Tgl(2026, 12, 31));
        var lewat = await Tolak(andi, new UpdateWorkforcePayrollDataRequest { Salary = GajiBaru(12_000_000m, Tgl(2027, 2, 1)) });
        Assert.Equal("Tanggal mulai berlaku tidak boleh lewat dari tanggal berakhir penetapan ini (31/12/2026).", lewat.Message);

        var dewi = Karyawan("Dewi");
        Gaji(dewi, 11_000_000m, Tgl(2025, 1, 1), utama: false);   // penetapan lain (bukan utama), periode terbuka
        var irisan = await Tolak(dewi, new UpdateWorkforcePayrollDataRequest { Salary = GajiBaru(12_000_000m, Tgl(2026, 1, 1)) });
        Assert.Equal("Sudah ada penetapan gaji dengan struktur dan grade yang sama pada periode yang beririsan.", irisan.Message);

        var tono = Karyawan("Tono");
        var tanpa = await Tolak(tono, new UpdateWorkforcePayrollDataRequest { ApproveSalaryAssignment = true, MarkBankAccountVerified = true });
        Assert.Contains("Belum ada penetapan gaji utama untuk disetujui.", tanpa.Message);
        Assert.Contains("Belum ada rekening payroll untuk diverifikasi.", tanpa.Message);

        var hilang = await Tolak(Guid.NewGuid(), new UpdateWorkforcePayrollDataRequest { BankAccount = RekeningBaru("123") });
        Assert.Equal((404, "Pegawai, perawat, atau dokter tidak ditemukan atau sudah tidak aktif."), (hilang.StatusCode, hilang.Message));
    }

    // AC3 (keputusan 1): mengubah penetapan yang sudah disetujui / rekening yang sudah terverifikasi mencabut persetujuan/verifikasi,
    // kecuali opsi setuju/verifikasi dipilih.
    [Fact]
    public async Task AC3_perubahan_mencabut_persetujuan_dan_verifikasi_kecuali_opsi_dipilih()
    {
        var sari = Karyawan("Sari");
        Gaji(sari, 12_000_000m, Tgl(2026, 1, 1), disetujui: true);
        Rekening(sari, "1234567890", terverifikasi: true);

        var r = await Simpan(sari, new UpdateWorkforcePayrollDataRequest { Salary = GajiBaru(13_000_000m), BankAccount = RekeningBaru("1234567891", "Pemilik Lama") });
        Assert.Equal(("Diperbarui", "Diperbarui"), (r.SalaryAssignment, r.BankAccount));
        Assert.Equal(new[] { "Penetapan gaji berubah, jadi persetujuannya dicabut dan perlu disetujui kembali.", "Data rekening berubah, jadi verifikasinya dicabut dan perlu diverifikasi kembali." }, r.Warnings);
        var (gaji, rek) = Baca(sari);
        Assert.Equal((13_000_000m, (DateTime?)null, (Guid?)null), (gaji.Single().BaseSalary, gaji.Single().ApprovedAt, gaji.Single().ApprovedByUserId));
        Assert.Equal(("1234567891", false, (DateTime?)null), (rek.Single().AccountNumber, rek.Single().IsVerified, rek.Single().VerifiedAt));

        var setuju = await Simpan(sari, new UpdateWorkforcePayrollDataRequest
        { Salary = GajiBaru(13_500_000m), BankAccount = RekeningBaru("1234567892", "Pemilik Lama"), ApproveSalaryAssignment = true, MarkBankAccountVerified = true });
        Assert.Equal(("Diperbarui dan disetujui", "Diperbarui dan terverifikasi"), (setuju.SalaryAssignment, setuju.BankAccount));
        Assert.Empty(setuju.Warnings);
        (gaji, rek) = Baca(sari);
        Assert.Equal((_aktor, true), (gaji.Single().ApprovedByUserId!.Value, gaji.Single().ApprovedAt.HasValue));
        Assert.Equal((_aktor, true), (rek.Single().VerifiedByUserId!.Value, rek.Single().IsVerified));

        var opsiSaja = await Simpan(Karyawan("Budi") is var budi ? budi : default, new UpdateWorkforcePayrollDataRequest { Salary = GajiBaru(12_000_000m), ApproveSalaryAssignment = true });
        Assert.Equal("Dibuat dan disetujui", opsiSaja.SalaryAssignment);
        Gaji(Karyawan("Rina") is var rina ? rina : default, 12_000_000m, Tgl(2026, 1, 1));
        Assert.Equal("Disetujui", (await Simpan(rina, new UpdateWorkforcePayrollDataRequest { ApproveSalaryAssignment = true })).SalaryAssignment);
    }

    // AC5 + VR-WPD-16 (Aturan D): grade 10–16 juta, gaji pokok Rp 9.000.000 tersimpan dengan peringatan.
    [Fact]
    public async Task AC5_gaji_di_luar_rentang_grade_hanya_peringatan()
    {
        var sari = Karyawan("Sari");
        var r = await Simpan(sari, new UpdateWorkforcePayrollDataRequest { Salary = GajiBaru(9_000_000m) });
        Assert.Equal("Dibuat", r.SalaryAssignment);
        Assert.Equal("Gaji pokok di luar rentang grade G-10 (Rp 10.000.000 – Rp 16.000.000).", Assert.Single(r.Warnings));
        Assert.Equal(9_000_000m, Baca(sari).Gaji.Single().BaseSalary);
    }

    // AC7 (Aturan E): satu log audit per simpan yang mengubah data, memuat pelaku (dari sesi), orang, dan hasil; tanpa gaji pokok dan
    // tanpa nomor rekening.
    [Fact]
    public async Task AC7_log_audit_memuat_pelaku_dan_hasil_tanpa_nominal_dan_nomor_rekening()
    {
        var sari = Karyawan("Sari");
        await Simpan(sari, new UpdateWorkforcePayrollDataRequest { Salary = GajiBaru(12_345_678m), BankAccount = RekeningBaru("987654321012") });

        var log = Assert.Single(_log.Pesan);
        Assert.Contains(_aktor.ToString(), log);
        Assert.Contains($"WorkforceProfileId={sari}", log);
        Assert.Contains("penetapan gaji: Dibuat", log);
        Assert.Contains("rekening payroll: Dibuat", log);
        foreach (var rahasia in new[] { "12345678", "12.345.678", "12,345,678", "987654321012", "Sari Lestari" })
            Assert.DoesNotContain(rahasia, log);
    }

    // AC4 (VR-WPD-14/15, HRD-DEC-218): opsi setuju tanpa WfpSalaryAssignment : Update dan opsi verifikasi tanpa
    // WorkforceBankAccount : Update ditolak 403 oleh controller sebelum apa pun disimpan; superadmin lolos.
    [Fact]
    public async Task AC4_opsi_setuju_dan_verifikasi_tanpa_hak_ditolak_403()
    {
        var sari = Karyawan("Sari");

        WorkforcePayrollDataController Controller(Guid user)
        {
            var ctx = _db.CreateContext();
            var akses = Pelaku.Sebagai(user);
            var c = new WorkforcePayrollDataController(new WorkforcePayrollDataService(ctx, new LoggerService(_log, akses)), Pelaku.Izin(ctx));
            c.ControllerContext = new ControllerContext { HttpContext = akses.HttpContext! };
            return c;
        }

        async Task<(int, string?)> Kirim(Guid user, UpdateWorkforcePayrollDataRequest r)
        {
            var hasil = Assert.IsAssignableFrom<ObjectResult>(await Controller(user).Save(sari, r, CancellationToken.None));
            return (hasil.StatusCode ?? 200, (hasil.Value as ApiResponse<object>)?.Message);
        }

        Assert.Equal((403, "Anda tidak berwenang menyetujui penetapan gaji."),
            await Kirim(_aktor, new UpdateWorkforcePayrollDataRequest { Salary = GajiBaru(12_000_000m), ApproveSalaryAssignment = true }));
        Assert.Equal((403, "Anda tidak berwenang memverifikasi rekening bank."),
            await Kirim(_aktor, new UpdateWorkforcePayrollDataRequest { BankAccount = RekeningBaru("123456"), MarkBankAccountVerified = true }));
        var (gaji, rek) = Baca(sari);
        Assert.Empty(gaji);
        Assert.Empty(rek);
        Assert.Empty(_log.Pesan);

        Assert.Equal(200, (await Kirim(_aktor, new UpdateWorkforcePayrollDataRequest { Salary = GajiBaru(12_000_000m) })).Item1);
        Assert.Equal(200, (await Kirim(_super, new UpdateWorkforcePayrollDataRequest { ApproveSalaryAssignment = true, BankAccount = RekeningBaru("123456"), MarkBankAccountVerified = true })).Item1);
        (gaji, rek) = Baca(sari);
        Assert.Equal((true, true), (gaji.Single().ApprovedAt.HasValue, rek.Single().IsVerified));
    }
}
