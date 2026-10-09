using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Seeders;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Regresi untuk <see cref="PharmacyModuleCodeConsolidationSeeder"/>.
/// </summary>
/// <remarks>
/// <para>
/// Yang dijaga di sini satu hal yang tidak dapat dibuktikan dengan membaca source: bahwa
/// perpindahan modul <b>tidak memutus satu pun izin yang sudah diberikan</b>.
/// </para>
/// <para>
/// Bahayanya senyap. <c>SysAccessPolicy</c> menunjuk <c>ControllerAccessId</c> dan
/// <c>ActionAccessId</c>; bila perpindahan modul membuat baris registry baru ber-<c>Id</c> baru,
/// policy lama tetap menunjuk baris yang sudah ditutup dan setiap izin Farmasi berhenti
/// berlaku tanpa galat. Uji di bawah membuktikan <c>Id</c>-nya memang dipertahankan, bukan
/// sekadar bahwa seeder-nya berjalan.
/// </para>
/// </remarks>
public class ModuleCodeConsolidationTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();

    public void Dispose() => _db.Dispose();

    private const string Legacy = PharmacyModuleCodeConsolidationSeeder.KodeLegacy;
    private const string Kanonik = PharmacyModuleCodeConsolidationSeeder.KodeKanonik;

    private static Guid Modul(ApplicationDbContext k, string kode, bool aktif = true)
    {
        var id = Guid.NewGuid();

        k.SysApplicationModules.Add(new SysApplicationModule
        {
            Id = id,
            ModuleCode = kode,
            ModuleName = kode,
            IsActive = aktif,
            CreateDateTime = DateTime.UtcNow
        });

        k.SaveChanges();
        return id;
    }

    private static Guid Controller(ApplicationDbContext k, Guid modulId, string nama)
    {
        var id = Guid.NewGuid();

        k.SysControllerAccesses.Add(new SysControllerAccess
        {
            Id = id,
            ModuleId = modulId,
            ControllerName = nama,
            DisplayName = nama,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        k.SaveChanges();
        return id;
    }

    private static Guid Aksi(ApplicationDbContext k, Guid controllerId, string nama)
    {
        var id = Guid.NewGuid();

        k.SysActionAccesses.Add(new SysActionAccess
        {
            Id = id,
            ControllerAccessId = controllerId,
            ActionName = nama,
            DisplayName = nama,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        k.SaveChanges();
        return id;
    }

    private static Guid Policy(ApplicationDbContext k, Guid controllerId, Guid aksiId,
        Guid? departemen = null, Guid? jabatan = null)
    {
        var id = Guid.NewGuid();

        k.SysAccessPolicies.Add(new SysAccessPolicy
        {
            Id = id,
            DepartmentId = departemen ?? Guid.NewGuid(),
            PositionId = jabatan ?? Guid.NewGuid(),
            ControllerAccessId = controllerId,
            ActionAccessId = aksiId,
            IsAllowed = true,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        k.SaveChanges();
        return id;
    }

    // ── Jalur normal ──────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Controller_legacy_berpindah_ke_modul_kanonik()
    {
        using var k = _db.CreateContext();
        var legacy = Modul(k, Legacy);
        var kanonik = Modul(k, Kanonik);
        var controller = Controller(k, legacy, "Prescription");

        var hasil = await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        Assert.Equal(1, hasil.ControllerDipindahkan);
        Assert.Equal(0, hasil.ControllerDigabungkan);

        var sesudah = await k.SysControllerAccesses.AsNoTracking()
            .SingleAsync(x => x.Id == controller);

        Assert.Equal(kanonik, sesudah.ModuleId);
    }

    /// <summary>
    /// Inti seluruh pekerjaan ini. <c>Id</c> harus sama persis sebelum dan sesudah, karena
    /// itulah yang membuat policy tetap sah.
    /// </summary>
    [Fact]
    public async Task Id_controller_dan_aksi_dipertahankan_bukan_dibuat_ulang()
    {
        using var k = _db.CreateContext();
        var legacy = Modul(k, Legacy);
        Modul(k, Kanonik);
        var controller = Controller(k, legacy, "Prescription");
        var aksi = Aksi(k, controller, "Read");

        await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        Assert.Single(await k.SysControllerAccesses.AsNoTracking()
            .Where(x => x.ControllerName == "Prescription").ToListAsync());

        var aksiSesudah = await k.SysActionAccesses.AsNoTracking()
            .SingleAsync(x => x.ActionName == "Read");

        Assert.Equal(aksi, aksiSesudah.Id);
        Assert.Equal(controller, aksiSesudah.ControllerAccessId);
    }

    [Fact]
    public async Task Policy_existing_tetap_menunjuk_controller_dan_aksi_yang_sama()
    {
        using var k = _db.CreateContext();
        var legacy = Modul(k, Legacy);
        Modul(k, Kanonik);
        var controller = Controller(k, legacy, "Prescription");
        var aksi = Aksi(k, controller, "Read");
        var policy = Policy(k, controller, aksi);

        var hasil = await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        var sesudah = await k.SysAccessPolicies.AsNoTracking().SingleAsync(x => x.Id == policy);

        Assert.Equal(controller, sesudah.ControllerAccessId);
        Assert.Equal(aksi, sesudah.ActionAccessId);
        Assert.True(sesudah.IsActive);
        Assert.False(sesudah.IsDelete);

        // Terdampak, tetapi sengaja TIDAK disentuh — nol yang dialihkan pada jalur normal.
        Assert.Equal(1, hasil.PolicyTerdampak);
        Assert.Equal(0, hasil.PolicyDialihkan);
    }

    [Fact]
    public async Task Nol_policy_dihapus()
    {
        using var k = _db.CreateContext();
        var legacy = Modul(k, Legacy);
        Modul(k, Kanonik);

        for (var i = 0; i < 3; i++)
        {
            var c = Controller(k, legacy, $"Resource{i}");
            Policy(k, c, Aksi(k, c, "Read"));
        }

        var sebelum = await k.SysAccessPolicies.CountAsync();

        await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        Assert.Equal(3, sebelum);
        Assert.Equal(3, await k.SysAccessPolicies.CountAsync());
        Assert.Empty(await k.SysAccessPolicies.Where(x => x.IsDelete).ToListAsync());
    }

    [Fact]
    public async Task Controller_yang_sudah_di_modul_kanonik_tidak_disentuh()
    {
        using var k = _db.CreateContext();
        Modul(k, Legacy);
        var kanonik = Modul(k, Kanonik);
        var sudahBenar = Controller(k, kanonik, "DrugReturn");

        await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        var sesudah = await k.SysControllerAccesses.AsNoTracking()
            .SingleAsync(x => x.Id == sudahBenar);

        Assert.Equal(kanonik, sesudah.ModuleId);
        Assert.True(sesudah.IsActive);
        Assert.False(sesudah.IsDelete);
    }

    // ── Idempotensi ───────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Dijalankan_dua_kali_tidak_mengubah_apa_pun_pada_kali_kedua()
    {
        using var k = _db.CreateContext();
        var legacy = Modul(k, Legacy);
        Modul(k, Kanonik);
        var controller = Controller(k, legacy, "Prescription");
        var aksi = Aksi(k, controller, "Read");
        Policy(k, controller, aksi);

        var pertama = await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);
        var kedua = await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        Assert.Equal(1, pertama.ControllerDipindahkan);
        Assert.Equal(0, kedua.ControllerDipindahkan);
        Assert.Equal(0, kedua.ControllerDigabungkan);
        Assert.Equal(0, kedua.PolicyDialihkan);

        Assert.Single(await k.SysControllerAccesses.AsNoTracking().ToListAsync());
        Assert.Single(await k.SysAccessPolicies.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task Lingkungan_tanpa_modul_legacy_tidak_melakukan_apa_pun()
    {
        using var k = _db.CreateContext();
        var kanonik = Modul(k, Kanonik);
        Controller(k, kanonik, "Prescription");

        var hasil = await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        Assert.Equal(0, hasil.ControllerDipindahkan);
        Assert.Equal(0, hasil.ControllerDigabungkan);
        Assert.False(hasil.ModulLegacyDinonaktifkan);
    }

    /// <summary>
    /// Lingkungan yang hanya mengenal kode lama: modul kanonik harus dibuat di sini, karena
    /// <c>AccessMenuSeeder</c> yang biasanya membuatnya justru berjalan sesudah seeder ini.
    /// </summary>
    [Fact]
    public async Task Modul_kanonik_dibuat_bila_belum_ada()
    {
        using var k = _db.CreateContext();
        var legacy = Modul(k, Legacy);
        var controller = Controller(k, legacy, "Prescription");

        var hasil = await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        var kanonik = await k.SysApplicationModules.AsNoTracking()
            .SingleAsync(x => x.ModuleCode == Kanonik);

        Assert.Equal(1, hasil.ControllerDipindahkan);
        Assert.Equal(kanonik.Id,
            (await k.SysControllerAccesses.AsNoTracking().SingleAsync(x => x.Id == controller))
                .ModuleId);
    }

    // ── Penonaktifan modul lama ───────────────────────────────────────────────────────────

    [Fact]
    public async Task Modul_legacy_dinonaktifkan_setelah_kosong()
    {
        using var k = _db.CreateContext();
        var legacy = Modul(k, Legacy);
        Modul(k, Kanonik);
        Controller(k, legacy, "Prescription");

        var hasil = await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        Assert.True(hasil.ModulLegacyDinonaktifkan);

        var modul = await k.SysApplicationModules.AsNoTracking()
            .SingleAsync(x => x.ModuleCode == Legacy);

        Assert.False(modul.IsActive);

        // Dinonaktifkan, bukan dihapus: jejaknya tetap dapat ditelusuri.
        Assert.False(modul.IsDelete);
    }

    /// <summary>
    /// Baris registry yang sudah ditutup <b>ikut</b> dipindahkan, bukan ditinggalkan.
    /// </summary>
    /// <remarks>
    /// Pilihan ini disengaja. Policy yang masih menunjuk baris tertutup adalah keadaan yang
    /// sudah ada sebelum konsolidasi; memindahkan barisnya mempertahankan <c>Id</c> sehingga
    /// policy itu tetap menunjuk baris yang sama, dan <c>AccessMenuSeeder</c> sesudahnya dapat
    /// menghidupkannya kembali bila controllernya memang masih dideklarasikan source.
    /// Meninggalkannya di modul lama akan lebih buruk: izinnya menunjuk ke dalam modul yang
    /// ditinggalkan, dan modul lama tidak akan pernah dapat ditutup.
    /// </remarks>
    [Fact]
    public async Task Baris_yang_sudah_ditutup_ikut_dipindahkan_dengan_id_dipertahankan()
    {
        using var k = _db.CreateContext();
        var legacy = Modul(k, Legacy);
        var kanonik = Modul(k, Kanonik);

        var tertutup = Controller(k, legacy, "Peninggalan");
        var aksi = Aksi(k, tertutup, "Read");
        var policy = Policy(k, tertutup, aksi);

        var barisTertutup = await k.SysControllerAccesses.SingleAsync(x => x.Id == tertutup);
        barisTertutup.IsActive = false;
        barisTertutup.IsDelete = true;
        await k.SaveChangesAsync();

        await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        var sesudah = await k.SysControllerAccesses.AsNoTracking()
            .SingleAsync(x => x.Id == tertutup);

        Assert.Equal(kanonik, sesudah.ModuleId);

        // Keadaan tertutupnya tidak diubah — konsolidasi memindahkan, bukan menghidupkan.
        Assert.False(sesudah.IsActive);

        // Dan izinnya tetap menunjuk baris yang sama, tidak dihapus.
        var policySesudah = await k.SysAccessPolicies.AsNoTracking()
            .SingleAsync(x => x.Id == policy);

        Assert.Equal(tertutup, policySesudah.ControllerAccessId);
        Assert.Equal(aksi, policySesudah.ActionAccessId);
    }

    /// <summary>
    /// Nama kembar membuat baris lama digabungkan lalu ditutup. Karena policy-nya ikut
    /// dialihkan ke baris kanonik, tidak ada lagi yang menggantung dan modulnya aman ditutup.
    /// </summary>
    [Fact]
    public async Task Modul_legacy_aman_ditutup_setelah_penggabungan_mengalihkan_policy()
    {
        using var k = _db.CreateContext();
        var legacy = Modul(k, Legacy);
        var kanonik = Modul(k, Kanonik);

        var tujuan = Controller(k, kanonik, "Prescription");
        Aksi(k, tujuan, "Read");

        var asal = Controller(k, legacy, "Prescription");
        Policy(k, asal, Aksi(k, asal, "Read"));

        var hasil = await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        Assert.Equal(1, hasil.ControllerDigabungkan);
        Assert.True(hasil.ModulLegacyDinonaktifkan);

        var modul = await k.SysApplicationModules.AsNoTracking()
            .SingleAsync(x => x.ModuleCode == Legacy);

        Assert.False(modul.IsActive);
    }

    // ── Jalur penggabungan ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Nama controller yang sudah ada di modul kanonik tidak dapat dipindahkan: indeks unik
    /// <c>(ModuleId, ControllerName)</c> akan menolaknya. Keduanya digabungkan, dan policy milik
    /// baris lama dialihkan ke baris kanonik alih-alih dibuang.
    /// </summary>
    [Fact]
    public async Task Nama_kembar_digabungkan_dan_policy_dialihkan_bukan_dibuang()
    {
        using var k = _db.CreateContext();
        var legacy = Modul(k, Legacy);
        var kanonik = Modul(k, Kanonik);

        var tujuan = Controller(k, kanonik, "Prescription");
        var aksiTujuan = Aksi(k, tujuan, "Read");

        var asal = Controller(k, legacy, "Prescription");
        var aksiAsal = Aksi(k, asal, "Read");

        var departemen = Guid.NewGuid();
        var jabatan = Guid.NewGuid();
        var policy = Policy(k, asal, aksiAsal, departemen, jabatan);

        var hasil = await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        Assert.Equal(0, hasil.ControllerDipindahkan);
        Assert.Equal(1, hasil.ControllerDigabungkan);
        Assert.Equal(1, hasil.PolicyDialihkan);

        var sesudah = await k.SysAccessPolicies.AsNoTracking().SingleAsync(x => x.Id == policy);

        Assert.Equal(tujuan, sesudah.ControllerAccessId);
        Assert.Equal(aksiTujuan, sesudah.ActionAccessId);

        // Policy-nya tetap satu: dialihkan, tidak diduplikasi.
        Assert.Single(await k.SysAccessPolicies.AsNoTracking().ToListAsync());
    }

    /// <summary>
    /// Bila peran yang sama sudah memiliki izin kembarnya, pengalihan akan melanggar indeks
    /// unik <c>(DepartmentId, PositionId, ControllerAccessId, ActionAccessId)</c>. Yang sudah
    /// ada dibiarkan, bukan ditimpa — keduanya menyatakan izin yang sama.
    /// </summary>
    [Fact]
    public async Task Policy_kembar_pada_peran_yang_sama_tidak_diduplikasi()
    {
        using var k = _db.CreateContext();
        var legacy = Modul(k, Legacy);
        var kanonik = Modul(k, Kanonik);

        var tujuan = Controller(k, kanonik, "Prescription");
        var aksiTujuan = Aksi(k, tujuan, "Read");

        var asal = Controller(k, legacy, "Prescription");
        var aksiAsal = Aksi(k, asal, "Read");

        var departemen = Guid.NewGuid();
        var jabatan = Guid.NewGuid();

        Policy(k, tujuan, aksiTujuan, departemen, jabatan);
        Policy(k, asal, aksiAsal, departemen, jabatan);

        var hasil = await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        // Tidak dialihkan, karena kembarannya sudah ada.
        Assert.Equal(0, hasil.PolicyDialihkan);

        var semua = await k.SysAccessPolicies.AsNoTracking().ToListAsync();

        // Dua baris tetap dua baris — nol yang dihapus — dan tidak ada pasangan kembar baru
        // pada peran yang sama terhadap aksi kanonik.
        Assert.Equal(2, semua.Count);
        Assert.Single(semua.Where(x =>
            x.DepartmentId == departemen &&
            x.PositionId == jabatan &&
            x.ControllerAccessId == tujuan &&
            x.ActionAccessId == aksiTujuan));
    }

    [Fact]
    public async Task Aksi_yang_belum_ada_di_tujuan_dipindahkan_dengan_id_dipertahankan()
    {
        using var k = _db.CreateContext();
        var legacy = Modul(k, Legacy);
        var kanonik = Modul(k, Kanonik);

        var tujuan = Controller(k, kanonik, "Prescription");
        Aksi(k, tujuan, "Read");

        var asal = Controller(k, legacy, "Prescription");
        var aksiKhas = Aksi(k, asal, "Finalize");
        var policy = Policy(k, asal, aksiKhas);

        var hasil = await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        var sesudah = await k.SysActionAccesses.AsNoTracking()
            .SingleAsync(x => x.ActionName == "Finalize");

        // Id aksinya dipertahankan, jadi policy-nya tidak perlu dialihkan sama sekali.
        Assert.Equal(aksiKhas, sesudah.Id);
        Assert.Equal(tujuan, sesudah.ControllerAccessId);
        Assert.Equal(0, hasil.PolicyDialihkan);

        var policySesudah = await k.SysAccessPolicies.AsNoTracking()
            .SingleAsync(x => x.Id == policy);

        Assert.Equal(aksiKhas, policySesudah.ActionAccessId);
        Assert.Equal(tujuan, policySesudah.ControllerAccessId);
    }

    // ── Bentuk nyata: 12 berpindah, 11 sudah benar ────────────────────────────────────────

    /// <summary>
    /// Meniru bentuk sebenarnya yang dihadapi di lingkungan: 12 controller di modul lama dan
    /// 11 di modul kanonik, masing-masing dengan satu izin yang sudah diberikan.
    /// </summary>
    [Fact]
    public async Task Bentuk_nyata_dua_belas_berpindah_dan_seluruh_izin_tetap_sah()
    {
        using var k = _db.CreateContext();
        var legacy = Modul(k, Legacy);
        var kanonik = Modul(k, Kanonik);

        var petaPolicy = new Dictionary<Guid, (Guid Controller, Guid Aksi)>();

        for (var i = 0; i < 12; i++)
        {
            var c = Controller(k, legacy, $"Legacy{i}");
            var a = Aksi(k, c, "Read");
            petaPolicy[Policy(k, c, a)] = (c, a);
        }

        for (var i = 0; i < 11; i++)
        {
            var c = Controller(k, kanonik, $"Kanonik{i}");
            var a = Aksi(k, c, "Read");
            petaPolicy[Policy(k, c, a)] = (c, a);
        }

        var hasil = await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        Assert.Equal(12, hasil.ControllerDipindahkan);
        Assert.Equal(0, hasil.ControllerDigabungkan);
        Assert.Equal(0, hasil.PolicyDialihkan);
        Assert.Equal(12, hasil.PolicyTerdampak);
        Assert.True(hasil.ModulLegacyDinonaktifkan);

        // Seluruh 23 controller kini di satu modul, dan nol yang ditutup.
        var controllers = await k.SysControllerAccesses.AsNoTracking().ToListAsync();
        Assert.Equal(23, controllers.Count);
        Assert.All(controllers, x =>
        {
            Assert.Equal(kanonik, x.ModuleId);
            Assert.True(x.IsActive);
            Assert.False(x.IsDelete);
        });

        // Dan setiap izin masih menunjuk baris yang sama persis seperti sebelum perpindahan.
        var policies = await k.SysAccessPolicies.AsNoTracking().ToListAsync();
        Assert.Equal(23, policies.Count);

        foreach (var policy in policies)
        {
            var diharapkan = petaPolicy[policy.Id];
            Assert.Equal(diharapkan.Controller, policy.ControllerAccessId);
            Assert.Equal(diharapkan.Aksi, policy.ActionAccessId);
        }
    }

    /// <summary>
    /// Pembuktian negatif: setiap izin harus masih dapat menemukan controller dan aksi yang
    /// <b>aktif</b>. Inilah persisnya yang akan gagal bila perpindahan dilakukan dengan membuat
    /// baris baru, karena baris lama ditutup dan `AccessPermissionService` menyaring
    /// <c>IsActive &amp;&amp; !IsDelete</c>.
    /// </summary>
    [Fact]
    public async Task Setiap_izin_masih_menemukan_controller_dan_aksi_yang_aktif()
    {
        using var k = _db.CreateContext();
        var legacy = Modul(k, Legacy);
        Modul(k, Kanonik);

        for (var i = 0; i < 5; i++)
        {
            var c = Controller(k, legacy, $"Legacy{i}");
            Policy(k, c, Aksi(k, c, "Read"));
        }

        await PharmacyModuleCodeConsolidationSeeder.ConsolidateAsync(k);

        var yatim = await k.SysAccessPolicies.AsNoTracking()
            .Where(p => !k.SysControllerAccesses.Any(c =>
                            c.Id == p.ControllerAccessId && c.IsActive && !c.IsDelete)
                     || !k.SysActionAccesses.Any(a =>
                            a.Id == p.ActionAccessId && a.IsActive && !a.IsDelete))
            .ToListAsync();

        Assert.Empty(yatim);
    }
}
