using System.Text.Json;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Services;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-360 — daftar dan ringkasan kelengkapan data penggajian (kontrak v27 §33, Aturan A dan B, HRD-DEC-219, HRD-DEC-222).
public sealed class WorkforcePayrollDataListTests : IDisposable
{
    private static DateTime Tgl(int tahun, int bulan, int hari) => new(tahun, bulan, hari, 0, 0, 0, DateTimeKind.Utc);

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _struktur = Guid.NewGuid(), _grade = Guid.NewGuid(), _dept = Guid.NewGuid(), _pos = Guid.NewGuid();
    private readonly Guid _perawat = Guid.NewGuid(), _bidan = Guid.NewGuid(), _bidanNursing = Guid.NewGuid(), _admin = Guid.NewGuid(), _dokter = Guid.NewGuid();
    private int _urut;

    public WorkforcePayrollDataListTests()
    {
        using var ctx = _db.CreateContext();
        ctx.MstDepartments.Add(new MstDepartment { Id = _dept, DepartmentCode = "ICU", DepartmentName = "Instalasi ICU" });
        ctx.MstPositions.Add(new MstPosition { Id = _pos, DepartmentId = _dept, PositionCode = "PLK", PositionName = "Pelaksana" });
        ctx.Set<MstSalaryGrade>().Add(new MstSalaryGrade { Id = _grade, SalaryGradeCode = "G-10", SalaryGradeName = "Grade 10", MinimumSalary = 10_000_000m, MaximumSalary = 16_000_000m });
        ctx.Set<MstSalaryStructure>().Add(new MstSalaryStructure { Id = _struktur, SalaryGradeId = _grade, SalaryStructureCode = "SS-01", SalaryStructureName = "Struktur Staf" });
        // Master profesi devYoga 18 November 2026: "Perawat" dan "Bidan" sama-sama berkelompok "Nursing".
        ctx.Set<MstProfession>().AddRange(
            new MstProfession { Id = _perawat, ProfessionCode = "PRF-02", ProfessionName = "Perawat", ProfessionGroup = "Nursing" },
            new MstProfession { Id = _bidan, ProfessionCode = "BDN", ProfessionName = "Bidan", ProfessionGroup = "Kebidanan" },
            new MstProfession { Id = _bidanNursing, ProfessionCode = "PRF-03", ProfessionName = "Bidan", ProfessionGroup = "Nursing" },
            new MstProfession { Id = _admin, ProfessionCode = "PRF-05", ProfessionName = "Administrator Kesehatan", ProfessionGroup = "AlliedHealth" },
            new MstProfession { Id = _dokter, ProfessionCode = "PRF-01", ProfessionName = "Dokter Umum", ProfessionGroup = "Medical" });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private Guid Karyawan(string nama, Guid? profesi = null, bool aktif = true, bool hapus = false, Guid? wf = null)
    {
        using var ctx = _db.CreateContext();
        var id = wf ?? Guid.NewGuid();
        var n = ++_urut;
        ctx.MstEmployees.Add(new MstEmployee
        {
            WorkforceProfileId = id, EmployeeCode = $"EMP-{n:000}", EmployeeNumber = $"NIP-{n:000}", FullName = nama, IdentityType = "KTP", IdentityNumber = $"3171{n:000000000000}",
            Email = $"k{n}@uji.local", BirthDate = Tgl(1990, 1, 1), JoinDate = Tgl(2020, 1, 1), PrimaryDepartmentId = _dept, PrimaryPositionId = _pos,
            WorkforceTypeId = Guid.NewGuid(), EmployeeCategoryId = Guid.NewGuid(), EmploymentTypeId = Guid.NewGuid(), EmploymentStatusId = Guid.NewGuid(), ProfessionId = profesi,
            IsActive = aktif, IsDelete = hapus,
        });
        ctx.SaveChanges();
        return id;
    }

    private Guid Dokter(string nama, bool aktif = true, Guid? wf = null)
    {
        using var ctx = _db.CreateContext();
        var id = wf ?? Guid.NewGuid();
        var n = ++_urut;
        ctx.MstDoctors.Add(new MstDoctor
        {
            WorkforceProfileId = id, DoctorCode = $"DOK-{n:000}", DoctorNumber = $"DR-{n:000}", FullName = nama, WorkforceTypeId = Guid.NewGuid(), EmployeeCategoryId = Guid.NewGuid(),
            EmploymentTypeId = Guid.NewGuid(), EmploymentStatusId = Guid.NewGuid(), ProfessionId = _dokter, IsActive = aktif,
        });
        ctx.SaveChanges();
        return id;
    }

    private void Gaji(Guid wf, decimal pokok, DateTime mulai, bool disetujui = false, bool utama = true, bool aktif = true)
    {
        using var ctx = _db.CreateContext();
        ctx.WfpSalaryAssignments.Add(new WfpSalaryAssignment
        { WorkforceProfileId = wf, SalaryStructureId = _struktur, SalaryGradeId = _grade, BaseSalary = pokok, EffectiveStartDate = mulai, IsPrimary = utama, IsActive = aktif, ApprovedAt = disetujui ? Tgl(2026, 1, 5) : null });
        ctx.SaveChanges();
    }

    private void Rekening(Guid wf, string nomor, bool terverifikasi = false, bool payroll = true, bool utama = false, DateTime? dibuat = null, string bank = "Bank Uji")
    {
        using var ctx = _db.CreateContext();
        ctx.WfpBankAccounts.Add(new WfpBankAccount
        {
            WorkforceProfileId = wf, BankId = Guid.NewGuid(), BankName = bank, AccountNumber = nomor, AccountHolderName = "Pemilik", IsPayrollAccount = payroll,
            IsVerified = terverifikasi, IsPrimary = utama, CreateDateTime = dibuat ?? Tgl(2026, 1, 1),
        });
        ctx.SaveChanges();
    }

    // Daftar dan ringkasan tidak menulis log; LoggerService tidak disentuh.
    private WorkforcePayrollDataService Layanan() => new(_db.CreateContext(), null!);

    private async Task<List<WorkforcePayrollDataItemResponse>> Semua(string? jenis = null, string? kelengkapan = null, string? cari = null) =>
        (await Layanan().ListAsync(new WorkforcePayrollDataQuery { PersonType = jenis, Completeness = kelengkapan, Search = cari, PageSize = 100 }, CancellationToken.None)).Items;

    // AC1 (Aturan A): seluruh karyawan dan dokter aktif tampil, termasuk yang belum punya data; nonaktif dan terhapus tidak;
    // orang yang tercatat sebagai karyawan sekaligus dokter muncul sekali, sebagai karyawan. Ringkasan menghitung populasi yang sama.
    [Fact]
    public async Task AC1_populasi_karyawan_dan_dokter_aktif_tanpa_ganda()
    {
        var sari = Karyawan("Sari");
        Karyawan("Nonaktif", aktif: false);
        Karyawan("Terhapus", hapus: true);
        var ganda = Karyawan("Rudi");
        Dokter("dr. Rudi", wf: ganda);
        var bima = Dokter("dr. Bima");
        Dokter("dr. Lama", aktif: false);

        var rows = await Semua();

        Assert.Equal(new[] { "dr. Bima", "Rudi", "Sari" }, rows.Select(x => x.Name).OrderBy(x => x));
        Assert.Equal("Karyawan", rows.Single(x => x.WorkforceProfileId == ganda).PersonType);
        Assert.Equal(("Dokter", "DR-006"), (rows.Single(x => x.WorkforceProfileId == bima).PersonType, rows.Single(x => x.WorkforceProfileId == bima).Number));
        var s = rows.Single(x => x.WorkforceProfileId == sari);
        Assert.Equal(("NIP-001", "Instalasi ICU", "Pelaksana", false, "Belum ada"), (s.Number, s.DepartmentName, s.PositionName, s.IsComplete, s.BankStatus));
        Assert.Equal(new[] { "Belum ada penetapan gaji utama.", "Belum ada rekening payroll." }, s.Issues);

        var r = await Layanan().SummaryAsync(CancellationToken.None);
        Assert.Equal((3, 0, 0, 0), (r.TotalPeople, r.WithApprovedSalaryAssignment, r.WithVerifiedBankAccount, r.Complete));
    }

    // AC2 (HRD-DEC-222): perawat dikenali dari profesi; bidan bukan perawat walau kelompoknya "Nursing". Filter jenis, kelengkapan,
    // dan pencarian (nama atau nomor, tanpa membedakan huruf besar); halaman paling banyak 100 baris, urut nama.
    [Fact]
    public async Task AC2_perawat_terpisah_filter_pencarian_dan_batas_halaman()
    {
        var sari = Karyawan("Sari", _perawat);
        Karyawan("Bunga", _bidan);
        Karyawan("Bidan Nursing", _bidanNursing);
        Karyawan("Anton", _admin);
        Karyawan("Tanpa Profesi");
        Dokter("dr. Bima");
        Gaji(sari, 12_000_000m, Tgl(2026, 1, 1), disetujui: true);
        Rekening(sari, "1234567890", terverifikasi: true);

        Assert.Equal(new[] { "Sari" }, (await Semua("perawat")).Select(x => x.Name));
        Assert.Equal(new[] { "Anton", "Bidan Nursing", "Bunga", "Tanpa Profesi" }, (await Semua("Karyawan")).Select(x => x.Name));
        Assert.Equal(new[] { "dr. Bima" }, (await Semua("Dokter")).Select(x => x.Name));
        Assert.Equal(new[] { "Sari" }, (await Semua(kelengkapan: "Lengkap")).Select(x => x.Name));
        Assert.Equal(5, (await Semua(kelengkapan: "belum lengkap")).Count);
        Assert.Equal(new[] { "Bunga" }, (await Semua(cari: "  bUnGa  ")).Select(x => x.Name));
        Assert.Equal(new[] { "Bidan Nursing", "Bunga" }, (await Semua(cari: "b")).Select(x => x.Name).Where(x => x.StartsWith('B')));
        Assert.Equal(new[] { "Sari" }, (await Semua(cari: "nip-001")).Select(x => x.Name));
        Assert.Empty(await Semua("Perawat", "Belum lengkap"));

        for (var i = 0; i < 120; i++) Karyawan($"Massal {i:000}");
        var l = Layanan();
        var besar = await l.ListAsync(new WorkforcePayrollDataQuery { PageSize = 500 }, CancellationToken.None);
        Assert.Equal((100, 100, 126, 2), (besar.PageSize, besar.Items.Count, besar.TotalData, besar.TotalPage));
        var akhir = await l.ListAsync(new WorkforcePayrollDataQuery { PageSize = 100, PageNumber = 2 }, CancellationToken.None);
        Assert.Equal(26, akhir.Items.Count);
        var nol = await l.ListAsync(new WorkforcePayrollDataQuery { PageSize = 0, PageNumber = 0 }, CancellationToken.None);
        Assert.Equal((1, 1, "Anton"), (nol.PageNumber, nol.PageSize, nol.Items.Single().Name));
    }

    // AC3 (HRD-DEC-219): baris daftar tidak memuat gaji pokok; nomor rekening hanya empat karakter terakhir.
    [Fact]
    public async Task AC3_daftar_tanpa_gaji_pokok_dan_nomor_disamarkan()
    {
        var sari = Karyawan("Sari");
        Gaji(sari, 12_345_678m, Tgl(2026, 1, 1), disetujui: true);
        Rekening(sari, "1234567890", terverifikasi: true, bank: "BCA");
        var budi = Karyawan("Budi");
        Rekening(budi, "123");

        var rows = await Semua();
        var s = rows.Single(x => x.WorkforceProfileId == sari);
        Assert.Equal(("BCA", "****7890", "Struktur Staf"), (s.BankName, s.AccountNumberMasked, s.SalaryStructureName));
        Assert.Equal("****", rows.Single(x => x.WorkforceProfileId == budi).AccountNumberMasked);

        Assert.DoesNotContain(typeof(WorkforcePayrollDataItemResponse).GetProperties(), p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));
        var json = JsonSerializer.Serialize(rows);
        foreach (var rahasia in new[] { "12345678", "1234567890", "Pemilik" })
            Assert.DoesNotContain(rahasia, json);
    }

    // AC4 (Aturan B): penetapan yang dinilai = utama aktif dengan tanggal mulai terbaru; rekening = payroll aktif (terverifikasi →
    // utama → terbaru). Bukan-utama, nonaktif, dan rekening non-payroll diabaikan. Untuk orang dengan satu penetapan utama, status
    // gaji dan rekening sama dengan yang dibaca Kelengkapan Data Payroll (PayrollDataReadinessService).
    [Fact]
    public async Task AC4_penetapan_dan_rekening_yang_dinilai_sama_dengan_payroll()
    {
        var andi = Karyawan("Andi");                                            // penetapan terbaru belum disetujui
        Gaji(andi, 11_000_000m, Tgl(2025, 1, 1), disetujui: true);
        Gaji(andi, 12_000_000m, Tgl(2026, 1, 1));
        Rekening(andi, "9000011111", terverifikasi: true, dibuat: Tgl(2025, 1, 1));
        Rekening(andi, "2222", dibuat: Tgl(2026, 3, 1));

        var dewi = Karyawan("Dewi");                                            // hanya bukan-utama/nonaktif disetujui; non-payroll terverifikasi
        Gaji(dewi, 12_000_000m, Tgl(2026, 1, 1), disetujui: true, utama: false);
        Gaji(dewi, 12_000_000m, Tgl(2026, 2, 1), disetujui: true, aktif: false);
        Rekening(dewi, "3333", terverifikasi: true, payroll: false);
        Rekening(dewi, "4444", utama: true, bank: "Utama");
        Rekening(dewi, "5555", dibuat: Tgl(2026, 6, 1), bank: "Terbaru");

        var sari = Karyawan("Sari", _perawat);                                  // lengkap
        Gaji(sari, 12_000_000m, Tgl(2026, 1, 1), disetujui: true);
        Rekening(sari, "6666", terverifikasi: true);

        var tono = Karyawan("Tono");                                            // penetapan belum disetujui, rekening belum terverifikasi
        Gaji(tono, 12_000_000m, Tgl(2026, 1, 1));
        Rekening(tono, "7777");

        var rows = (await Semua()).ToDictionary(x => x.WorkforceProfileId);

        var a = rows[andi];
        Assert.Equal((false, true, "Terverifikasi", "****1111", Tgl(2026, 1, 1)), (a.HasApprovedSalaryAssignment, a.HasPendingSalaryAssignment, a.BankStatus, a.AccountNumberMasked, DateTime.SpecifyKind(a.SalaryEffectiveStartDate!.Value, DateTimeKind.Utc)));
        Assert.Equal(new[] { "Penetapan gaji utama belum disetujui." }, a.Issues);

        var d = rows[dewi];
        Assert.Equal((false, false, "Belum terverifikasi", "Utama"), (d.HasApprovedSalaryAssignment, d.HasPendingSalaryAssignment, d.BankStatus, d.BankName));
        Assert.Equal(new[] { "Belum ada penetapan gaji utama.", "Rekening payroll belum terverifikasi." }, d.Issues);

        Assert.True(rows[sari].IsComplete);
        Assert.Empty(rows[sari].Issues);
        Assert.Equal(new[] { "Penetapan gaji utama belum disetujui.", "Rekening payroll belum terverifikasi." }, rows[tono].Issues);

        var ringkas = await Layanan().SummaryAsync(CancellationToken.None);
        Assert.Equal((4, 1, 2, 1), (ringkas.TotalPeople, ringkas.WithApprovedSalaryAssignment, ringkas.WithVerifiedBankAccount, ringkas.Complete));

        // Pembanding: Kelengkapan Data Payroll membaca tabel HRD yang sama. Andi punya dua penetapan utama aktif (kasus tepi yang
        // dinilai berbeda: Payroll memakai "ada yang disetujui"), jadi dibandingkan hanya orang dengan satu penetapan utama.
        var ctxPayroll = _db.CreateContext();
        var payroll = (await new PayrollDataReadinessService(ctxPayroll, new PayrollHrdSourceReader(ctxPayroll))
                .ListAsync(new PayrollDataReadinessQuery { PageSize = 100 }, CancellationToken.None)).Items
            .ToDictionary(x => x.WorkforceProfileId);
        foreach (var wf in new[] { dewi, sari, tono })
        {
            Assert.Equal(rows[wf].HasApprovedSalaryAssignment, payroll[wf].HasApprovedSalaryAssignment);
            Assert.Equal(rows[wf].HasPendingSalaryAssignment, payroll[wf].HasPendingSalaryAssignment);
            Assert.Equal(rows[wf].BankStatus, payroll[wf].BankStatus);
        }
        Assert.Equal(rows[andi].BankStatus, payroll[andi].BankStatus);
        Assert.Equal(rows.Keys.OrderBy(x => x), payroll.Keys.OrderBy(x => x));
    }
}
