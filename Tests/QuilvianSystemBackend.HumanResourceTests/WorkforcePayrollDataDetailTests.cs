using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-361 — detail isian penggajian satu orang (kontrak v27 bagian 33, HRD-DEC-219). Grade G-10 Rp 10–16 juta.
public sealed class WorkforcePayrollDataDetailTests : IDisposable
{
    private const string Pesan404 = "Pegawai, perawat, atau dokter tidak ditemukan atau sudah tidak aktif.";
    private static DateTime Tgl(int tahun, int bulan, int hari) => new(tahun, bulan, hari, 0, 0, 0, DateTimeKind.Utc);

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _struktur = Guid.NewGuid(), _grade = Guid.NewGuid(), _perawat = Guid.NewGuid(), _bidan = Guid.NewGuid(), _dokter = Guid.NewGuid(), _dept = Guid.NewGuid(), _pos = Guid.NewGuid();
    private int _urut;

    public WorkforcePayrollDataDetailTests()
    {
        using var ctx = _db.CreateContext();
        // Departemen dan jabatan utama karyawan adalah FK wajib (INNER JOIN), jadi harus ada barisnya.
        ctx.MstDepartments.Add(new MstDepartment { Id = _dept, DepartmentCode = "ICU", DepartmentName = "Instalasi ICU" });
        ctx.MstPositions.Add(new MstPosition { Id = _pos, DepartmentId = _dept, PositionCode = "PLK", PositionName = "Pelaksana" });
        ctx.Set<MstSalaryGrade>().Add(new MstSalaryGrade { Id = _grade, SalaryGradeCode = "G-10", SalaryGradeName = "Grade 10", MinimumSalary = 10_000_000m, MaximumSalary = 16_000_000m });
        ctx.Set<MstSalaryStructure>().Add(new MstSalaryStructure { Id = _struktur, SalaryGradeId = _grade, SalaryStructureCode = "SS-01", SalaryStructureName = "Struktur Staf" });
        ctx.Set<MstProfession>().AddRange(
            new MstProfession { Id = _perawat, ProfessionCode = "PRW", ProfessionName = "Perawat Pelaksana", ProfessionGroup = "Keperawatan" },
            new MstProfession { Id = _bidan, ProfessionCode = "BDN", ProfessionName = "Bidan", ProfessionGroup = "Kebidanan" },
            new MstProfession { Id = _dokter, ProfessionCode = "DU", ProfessionName = "Dokter Umum", ProfessionGroup = "Medis" });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private Guid Karyawan(string nama, Guid? profesi = null, bool aktif = true, Guid? wf = null)
    {
        using var ctx = _db.CreateContext();
        var id = wf ?? Guid.NewGuid();
        var n = ++_urut;
        ctx.MstEmployees.Add(new MstEmployee
        {
            WorkforceProfileId = id, EmployeeCode = $"EMP-{n:000}", EmployeeNumber = $"NIP-{n:000}", FullName = nama, IdentityType = "KTP", IdentityNumber = $"3171{n:000000000000}",
            Email = $"k{n}@uji.local", BirthDate = Tgl(1990, 1, 1), JoinDate = Tgl(2020, 1, 1), PrimaryDepartmentId = _dept, PrimaryPositionId = _pos,
            WorkforceTypeId = Guid.NewGuid(), EmployeeCategoryId = Guid.NewGuid(), EmploymentTypeId = Guid.NewGuid(), EmploymentStatusId = Guid.NewGuid(), ProfessionId = profesi, IsActive = aktif,
        });
        ctx.SaveChanges();
        return id;
    }

    private Guid Dokter(string nama, Guid? wf = null, bool hapus = false)
    {
        using var ctx = _db.CreateContext();
        var id = wf ?? Guid.NewGuid();
        var n = ++_urut;
        ctx.MstDoctors.Add(new MstDoctor
        {
            WorkforceProfileId = id, DoctorCode = $"DOK-{n:000}", DoctorNumber = $"DR-{n:000}", FullName = nama, WorkforceTypeId = Guid.NewGuid(), EmployeeCategoryId = Guid.NewGuid(),
            EmploymentTypeId = Guid.NewGuid(), EmploymentStatusId = Guid.NewGuid(), ProfessionId = _dokter, IsActive = true, IsDelete = hapus,
        });
        ctx.SaveChanges();
        return id;
    }

    private void Gaji(Guid wf, decimal pokok, DateTime mulai, bool utama = true, bool disetujui = false, bool aktif = true)
    {
        using var ctx = _db.CreateContext();
        ctx.WfpSalaryAssignments.Add(new WfpSalaryAssignment
        { WorkforceProfileId = wf, SalaryStructureId = _struktur, SalaryGradeId = _grade, BaseSalary = pokok, EffectiveStartDate = mulai, IsPrimary = utama, IsActive = aktif, ApprovedAt = disetujui ? Tgl(2026, 1, 5) : null });
        ctx.SaveChanges();
    }

    private void Rekening(Guid wf, string nomor, bool payroll = true, bool terverifikasi = false, bool utama = false, DateTime? dibuat = null)
    {
        using var ctx = _db.CreateContext();
        ctx.WfpBankAccounts.Add(new WfpBankAccount
        {
            WorkforceProfileId = wf, BankId = Guid.NewGuid(), BankName = "Bank Uji", AccountNumber = nomor, AccountHolderName = "Pemilik " + nomor, IsPayrollAccount = payroll,
            IsVerified = terverifikasi, IsPrimary = utama, CreateDateTime = dibuat ?? Tgl(2026, 1, 1),
        });
        ctx.SaveChanges();
    }

    // Detail tidak menulis log; LoggerService tidak disentuh.
    private Task<WorkforcePayrollDataDetailResponse> Detail(Guid wf) => new WorkforcePayrollDataService(_db.CreateContext(), null!).GetDetailAsync(wf, CancellationToken.None);

    // AC1 (bagian data): pemegang hak ubah menerima gaji pokok dan nomor rekening lengkap. Yang dipakai: penetapan UTAMA aktif dengan
    // tanggal mulai terbaru, dan rekening PAYROLL aktif (terverifikasi → utama → terbaru) — sama dengan daftar (Aturan B).
    [Fact]
    public async Task Detail_memuat_gaji_pokok_dan_nomor_rekening_lengkap_yang_dipakai()
    {
        var sari = Karyawan("Sari");
        Gaji(sari, 11_000_000m, Tgl(2025, 1, 1), disetujui: true);
        Gaji(sari, 12_500_000m, Tgl(2026, 1, 1), disetujui: true);
        Gaji(sari, 20_000_000m, Tgl(2026, 6, 1), utama: false);
        Gaji(sari, 30_000_000m, Tgl(2026, 9, 1), aktif: false);
        Rekening(sari, "1234567890", terverifikasi: true, dibuat: Tgl(2025, 1, 1));
        Rekening(sari, "9999999999", dibuat: Tgl(2026, 3, 1));
        Rekening(sari, "5555555555", payroll: false, terverifikasi: true, utama: true);

        var d = await Detail(sari);

        Assert.Equal(("Karyawan", "Sari", "Instalasi ICU", "Pelaksana"), (d.PersonType, d.Name, d.DepartmentName, d.PositionName));
        Assert.NotNull(d.Salary);
        Assert.Equal(12_500_000m, d.Salary!.BaseSalary);
        Assert.Equal(Tgl(2026, 1, 1), DateTime.SpecifyKind(d.Salary.EffectiveStartDate, DateTimeKind.Utc));
        Assert.Equal(("SS-01", "Struktur Staf", "G-10"), (d.Salary.SalaryStructureCode, d.Salary.SalaryStructureName, d.Salary.SalaryGradeCode));
        Assert.Equal((10_000_000m, 16_000_000m), (d.Salary.GradeMinimum, d.Salary.GradeMaximum));
        Assert.True(d.Salary.IsApproved);
        Assert.NotNull(d.Salary.ApprovedAt);
        Assert.NotNull(d.BankAccount);
        Assert.Equal(("1234567890", "Pemilik 1234567890", true), (d.BankAccount!.AccountNumber, d.BankAccount.AccountHolderName, d.BankAccount.IsVerified));
    }

    // AC3: bagian yang belum ada dikembalikan null, bukan galat. Penetapan belum disetujui → IsApproved false.
    [Fact]
    public async Task Bagian_yang_belum_ada_dikembalikan_null()
    {
        var budi = Karyawan("Budi");
        var kosong = await Detail(budi);
        Assert.Null(kosong.Salary);
        Assert.Null(kosong.BankAccount);

        Gaji(budi, 9_000_000m, Tgl(2026, 1, 1));
        var gajiSaja = await Detail(budi);
        Assert.False(gajiSaja.Salary!.IsApproved);
        Assert.Null(gajiSaja.Salary.ApprovedAt);
        Assert.Null(gajiSaja.BankAccount);

        var rina = Karyawan("Rina");
        Rekening(rina, "777", payroll: false);
        Assert.Null((await Detail(rina)).BankAccount);
    }

    // Jenis orang (HRD-DEC-222): perawat dari profesi karyawan; bidan bukan perawat; karyawan sekaligus dokter dihitung karyawan.
    [Fact]
    public async Task Jenis_orang_karyawan_perawat_dan_dokter()
    {
        Assert.Equal("Perawat", (await Detail(Karyawan("Ners Ani", _perawat))).PersonType);
        Assert.Equal("Karyawan", (await Detail(Karyawan("Bidan Wati", _bidan))).PersonType);
        Assert.Equal("Dokter", (await Detail(Dokter("dr. Bima"))).PersonType);

        var ganda = Karyawan("dr. Arif");
        Dokter("dr. Arif", ganda);
        var d = await Detail(ganda);
        Assert.Equal(("Karyawan", "dr. Arif"), (d.PersonType, d.Name));
    }

    // AC2: bukan karyawan atau dokter aktif → 404 berpesan.
    [Fact]
    public async Task Bukan_karyawan_atau_dokter_aktif_menghasilkan_404_berpesan()
    {
        var nonaktif = Karyawan("Tono", aktif: false);
        Gaji(nonaktif, 12_000_000m, Tgl(2026, 1, 1));
        var dihapus = Dokter("dr. Lama", hapus: true);

        foreach (var wf in new[] { Guid.NewGuid(), nonaktif, dihapus })
        {
            var ex = await Assert.ThrowsAsync<WorkforcePayrollDataException>(() => Detail(wf));
            Assert.Equal((404, Pesan404), (ex.StatusCode, ex.Message));
        }
    }

    // AC1 (bagian hak akses): GET detail dijaga WorkforcePayrollData : Update (bukan Read), daftar dan ringkasan : Read; keduanya
    // terdaftar di registri (tampil di Akses Role) dan controller lolos validator startup.
    [Fact]
    public void Detail_dijaga_hak_Update_sedangkan_daftar_hak_Read()
    {
        (string, string) Izin(string metode)
        {
            var a = typeof(WorkforcePayrollDataController).GetMethod(metode)!.GetCustomAttribute<AccessPermissionAttribute>()!;
            return ((string)a.Arguments![0], (string)a.Arguments[1]);
        }
        Assert.Equal(("WorkforcePayrollData", "Update"), Izin(nameof(WorkforcePayrollDataController.Detail)));
        Assert.Equal(("WorkforcePayrollData", "Read"), Izin(nameof(WorkforcePayrollDataController.List)));
        Assert.Equal(("WorkforcePayrollData", "Read"), Izin(nameof(WorkforcePayrollDataController.Summary)));

        var snapshot = PermissionRegistryDescriptor.BuildFromAssembly(typeof(WorkforcePayrollDataController).Assembly);
        foreach (var aksi in new[] { "Read", "Update" })
            Assert.Contains(snapshot.Actions, x => x.ModuleCode == "HUMAN_RESOURCE_WORKFORCE_CORE" && x.ResourceName == "WorkforcePayrollData" && x.ActionName == aksi);

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new DiagnosticListener("uji"));
        services.AddSingleton<DiagnosticSource>(sp => sp.GetRequiredService<DiagnosticListener>());
        services.AddControllers().AddApplicationPart(typeof(WorkforcePayrollDataController).Assembly);
        using var provider = services.BuildServiceProvider();
        var hasil = new PermissionRegistryValidator(provider.GetRequiredService<IActionDescriptorCollectionProvider>(), NullLogger<PermissionRegistryValidator>.Instance)
            .ValidateAndReport(throwOnFailure: false);
        var temuan = hasil.UnregisterableEndpoints.Concat(hasil.DuplicateResourceIdentities).Concat(hasil.InvalidAccessTypes)
            .Concat(hasil.UnenforcedEndpoints).Concat(hasil.NakedBusinessEndpoints)
            .Where(x => x.StartsWith("WorkforcePayrollData.", StringComparison.Ordinal) || x.Contains("'WorkforcePayrollData'", StringComparison.Ordinal) || x.Contains(nameof(WorkforcePayrollDataController), StringComparison.Ordinal))
            .ToList();
        Assert.True(temuan.Count == 0, string.Join(Environment.NewLine, temuan));
    }

    // AC1: pegawai yang jabatannya hanya diberi WorkforcePayrollData : Read ditolak memuat detail (filter memeriksa Update);
    // jabatan yang diberi Update lolos; superadmin lolos.
    [Fact]
    public async Task Pemegang_Read_saja_ditolak_pemegang_Update_lolos()
    {
        var hanyaBaca = Guid.NewGuid();
        var pengubah = Guid.NewGuid();
        var super = Guid.NewGuid();
        var (deptBaca, posBaca, deptUbah, posUbah) = (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        using (var ctx = _db.CreateContext())
        {
            ApplicationUser Akun(Guid id, UserType tipe) => new() { Id = id, UserName = id.ToString(), UserCode = id.ToString("N")[..8], DisplayName = "Uji", UserType = tipe, IsActive = true };
            ctx.Users.AddRange(Akun(hanyaBaca, UserType.Employee), Akun(pengubah, UserType.Employee), Akun(super, UserType.SuperAdmin));
            var c = new SysControllerAccess { ModuleId = Guid.NewGuid(), ControllerName = "WorkforcePayrollData", DisplayName = "Workforce Payroll Data" };
            var baca = new SysActionAccess { ControllerAccessId = c.Id, ActionName = "Read", DisplayName = "Read", AccessType = "Read" };
            var ubah = new SysActionAccess { ControllerAccessId = c.Id, ActionName = "Update", DisplayName = "Update", AccessType = "Update" };
            ctx.SysControllerAccesses.Add(c);
            ctx.SysActionAccesses.AddRange(baca, ubah);
            ctx.ApplicationUserOrganizations.AddRange(
                new ApplicationUserOrganization { UserId = hanyaBaca, DepartmentId = deptBaca, PositionId = posBaca, IsActive = true },
                new ApplicationUserOrganization { UserId = pengubah, DepartmentId = deptUbah, PositionId = posUbah, IsActive = true });
            ctx.SysAccessPolicies.AddRange(
                new SysAccessPolicy { DepartmentId = deptBaca, PositionId = posBaca, ControllerAccessId = c.Id, ActionAccessId = baca.Id, IsAllowed = true, IsActive = true },
                new SysAccessPolicy { DepartmentId = deptUbah, PositionId = posUbah, ControllerAccessId = c.Id, ActionAccessId = ubah.Id, IsAllowed = true, IsActive = true });
            ctx.SaveChanges();
        }

        async Task<bool> Boleh(Guid user, string aksi)
        {
            using var ctx = _db.CreateContext();
            return await Pelaku.Izin(ctx).HasAccessAsync(Pelaku.Sebagai(user).HttpContext!.User, "WorkforcePayrollData", aksi);
        }

        Assert.True(await Boleh(hanyaBaca, "Read"));
        Assert.False(await Boleh(hanyaBaca, "Update"));
        Assert.True(await Boleh(pengubah, "Update"));
        Assert.True(await Boleh(super, "Update"));
    }
}
