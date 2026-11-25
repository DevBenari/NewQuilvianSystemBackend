using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-359 — AT-HRD-PK-51 (HRD-DEC-240, kontrak v30 API 36.6, VR-PK-27, Permission 34.1, INT-HR-117): daftar baca
// penempatan sementara lintas pegawai. Contoh desain: Raka (ICU) ditempatkan sementara di IGD. Tanggal relatif hari ini (WIB).
public sealed class TemporaryAssignmentQueryTests : IDisposable
{
    private const string TolakBaca = "Anda tidak berwenang melihat data unit ini";

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _rs = Guid.NewGuid(), _icu = Guid.NewGuid(), _igd = Guid.NewGuid(), _farmasi = Guid.NewGuid(), _lab = Guid.NewGuid();
    private readonly Guid _uKepalaIcu = Guid.NewGuid(), _uKepalaIgd = Guid.NewGuid(), _uKepalaFarmasi = Guid.NewGuid(), _uHr = Guid.NewGuid(), _uBiasa = Guid.NewGuid();
    private readonly Guid _raka = Guid.NewGuid(), _nina = Guid.NewGuid(), _oki = Guid.NewGuid(), _tari = Guid.NewGuid();

    // Penempatan sementara: Raka ICU→IGD (berlangsung), Nina IGD→Farmasi (akan datang), Oki Lab→Farmasi (selesai),
    // Tari tanpa penempatan utama →ICU (berlangsung, tanpa akhir).
    private readonly Guid _tRaka = Guid.NewGuid(), _tNina = Guid.NewGuid(), _tOki = Guid.NewGuid(), _tTari = Guid.NewGuid();
    private readonly Guid _tDihapus = Guid.NewGuid(), _tNonaktif = Guid.NewGuid(), _sekunder = Guid.NewGuid();

    private static readonly DateOnly HariIni = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));

    public TemporaryAssignmentQueryTests()
    {
        using var ctx = _db.CreateContext();
        var legal = Guid.NewGuid();
        MstOrganizationUnit Unit(Guid id, string kode, Guid? induk) => new() { Id = id, LegalEntityId = legal, UnitCode = kode, UnitName = kode, ParentOrganizationUnitId = induk };
        ctx.MstOrganizationUnits.AddRange(Unit(_rs, "RS", null), Unit(_icu, "ICU", _rs), Unit(_igd, "IGD", _rs), Unit(_farmasi, "FARMASI", _rs), Unit(_lab, "LAB", _rs));

        Guid pIcu = Guid.NewGuid(), pIgd = Guid.NewGuid(), pFarmasi = Guid.NewGuid(), pHr = Guid.NewGuid(), pBiasa = Guid.NewGuid();
        foreach (var (id, kode, nama) in new[]
                 {
                     (pIcu, "EMP-SARI", "Sari"), (pIgd, "EMP-DODI", "Dodi"), (pFarmasi, "EMP-FINA", "Fina"), (pHr, "EMP-HR", "Petugas HR"), (pBiasa, "EMP-BUDI", "Budi"),
                     (_raka, "EMP-RAK", "Raka Firmansyah"), (_nina, "EMP-NIN", "Nina"), (_oki, "EMP-OKI", "Oki"), (_tari, "EMP-TAR", "Tari"),
                 })
        {
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = kode, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
        }

        ApplicationUser Akun(Guid id, Guid profil) => new()
        { Id = id, UserName = id.ToString(), UserCode = id.ToString("N")[..8], DisplayName = "Uji", UserType = UserType.Employee, WorkforceProfileId = profil, IsActive = true };
        ctx.Users.AddRange(Akun(_uKepalaIcu, pIcu), Akun(_uKepalaIgd, pIgd), Akun(_uKepalaFarmasi, pFarmasi), Akun(_uHr, pHr), Akun(_uBiasa, pBiasa));

        WfpOrganizationAssignment Penempatan(Guid profil, Guid? unit, string jenis, bool utama, int mulai, int? akhir, bool berjabatan = false, Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(), WorkforceProfileId = profil, OrganizationUnitId = unit, DepartmentId = Guid.NewGuid(), PositionId = Guid.NewGuid(),
            AssignmentType = jenis, IsPrimary = utama, IsManagerialAssignment = berjabatan, IsActive = true,
            EffectiveStartDate = Tanggal(mulai), EffectiveEndDate = akhir.HasValue ? Tanggal(akhir.Value) : null,
        };

        ctx.WfpOrganizationAssignments.AddRange(
            // Kepala Unit (penempatan berjabatan yang berlaku hari ini).
            Penempatan(pIcu, _icu, "Primary", true, -400, null, berjabatan: true),
            Penempatan(pIgd, _igd, "Primary", true, -400, null, berjabatan: true),
            Penempatan(pFarmasi, _farmasi, "Primary", true, -400, null, berjabatan: true),
            Penempatan(pBiasa, _icu, "Primary", true, -400, null),

            // Raka: dulu utama di Farmasi (berakhir), kini utama di ICU; utama baru di Lab mulai sesudah penempatan sementara.
            Penempatan(_raka, _farmasi, "Primary", true, -400, -101),
            Penempatan(_raka, _icu, "Primary", true, -100, null),
            Penempatan(_raka, _lab, "Primary", true, 30, null),
            Penempatan(_raka, _lab, "Secondary", false, -20, null, id: _sekunder),   // bukan utama: tidak menjadi unit asal
            Penempatan(_raka, _igd, "Temporary", false, -3, 10, id: _tRaka),

            Penempatan(_nina, _igd, "Primary", true, -400, null),
            Penempatan(_nina, _farmasi, "Temporary", false, 5, 12, id: _tNina),

            Penempatan(_oki, _lab, "Primary", true, -400, null),
            Penempatan(_oki, _farmasi, "Primary", true, -60, -30),   // utama yang sudah berakhir sebelum tanggal mulai
            Penempatan(_oki, _farmasi, "Temporary", false, -20, -10, id: _tOki),

            Penempatan(_tari, _icu, "Temporary", false, -1, null, id: _tTari),

            // Tidak boleh tampil: dihapus dan nonaktif (keduanya bertujuan ICU, dalam cakupan Kepala ICU).
            Penempatan(_nina, _icu, "Temporary", false, 1, 3, id: _tDihapus),
            Penempatan(_oki, _icu, "Temporary", false, 1, 3, id: _tNonaktif));

        ctx.SaveChanges();
        var dihapus = ctx.WfpOrganizationAssignments.Single(x => x.Id == _tDihapus);
        dihapus.IsDelete = true;
        var nonaktif = ctx.WfpOrganizationAssignments.Single(x => x.Id == _tNonaktif);
        nonaktif.IsActive = false;
        var raka = ctx.WfpOrganizationAssignments.Single(x => x.Id == _tRaka);
        raka.AssignmentNumber = "SPT-2027-031";
        raka.Description = "Bantuan IGD saat lonjakan pasien";

        // Petugas HR Scheduling: penanda RosterPeriod : ManageAllUnits (cakupan baca semua unit).
        var controller = new SysControllerAccess { ModuleId = Guid.NewGuid(), ControllerName = "RosterPeriod", DisplayName = "Roster" };
        var action = new SysActionAccess { ControllerAccessId = controller.Id, ActionName = "ManageAllUnits", DisplayName = "Manage All", AccessType = "Update" };
        Guid dept = Guid.NewGuid(), pos = Guid.NewGuid();
        ctx.SysControllerAccesses.Add(controller);
        ctx.SysActionAccesses.Add(action);
        ctx.ApplicationUserOrganizations.Add(new ApplicationUserOrganization { UserId = _uHr, DepartmentId = dept, PositionId = pos, IsActive = true });
        ctx.SysAccessPolicies.Add(new SysAccessPolicy { DepartmentId = dept, PositionId = pos, ControllerAccessId = controller.Id, ActionAccessId = action.Id, IsAllowed = true, IsActive = true });
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private static DateTime Tanggal(int hari) => DateTime.SpecifyKind(HariIni.AddDays(hari).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

    private TemporaryAssignmentController Sebagai(Guid userId)
    {
        var ctx = _db.CreateContext();
        var akses = Pelaku.Sebagai(userId);
        var service = new TemporaryAssignmentQueryService(ctx, new HrdUnitScopeService(ctx, Pelaku.Izin(ctx), akses));
        return new TemporaryAssignmentController(service) { ControllerContext = new ControllerContext { HttpContext = akses.HttpContext! } };
    }

    private async Task<(int Status, PagedResult<TemporaryAssignmentResponse>? Data, string Message)> Daftar(Guid userId, TemporaryAssignmentPagedQuery? query = null)
    {
        var hasil = Assert.IsAssignableFrom<ObjectResult>(await Sebagai(userId).GetPaged(query ?? new TemporaryAssignmentPagedQuery(), default));
        return hasil.Value switch
        {
            ApiResponse<PagedResult<TemporaryAssignmentResponse>> ok => (hasil.StatusCode ?? 200, ok.Data, ok.Message),
            ApiResponse<object> gagal => (hasil.StatusCode ?? 200, null, gagal.Message),
            _ => throw new InvalidOperationException("Tipe respons tidak dikenal")
        };
    }

    private async Task<List<Guid>> Id(Guid userId, TemporaryAssignmentPagedQuery? query = null)
    {
        var (status, data, pesan) = await Daftar(userId, query);
        Assert.True(status == 200, pesan);
        return data!.Items.Select(x => x.Id).OrderBy(x => x).ToList();
    }

    private static List<Guid> Urut(params Guid[] id) => id.OrderBy(x => x).ToList();

    // AC1: Raka ICU → IGD tampil bagi Kepala Unit ICU (unit asal) dan IGD (unit tujuan), dengan unit asal dan tujuan.
    [Fact]
    public async Task Penempatan_tampil_bagi_Kepala_Unit_asal_dan_tujuan()
    {
        foreach (var kepala in new[] { _uKepalaIcu, _uKepalaIgd })
        {
            var (status, data, _) = await Daftar(kepala);
            Assert.Equal(200, status);
            var raka = Assert.Single(data!.Items, x => x.Id == _tRaka);
            Assert.Equal(_raka, raka.WorkforceProfileId);
            Assert.Equal("EMP-RAK", raka.ProfileCode);
            Assert.Equal("Raka Firmansyah", raka.EmployeeName);
            Assert.Equal("ICU", raka.OriginUnitName);          // utama yang berlaku pada tanggal mulai, bukan Farmasi lama atau Lab nanti
            Assert.Equal(_igd, raka.DestinationUnitId);
            Assert.Equal("IGD", raka.DestinationUnitName);
            Assert.Equal(HariIni.AddDays(-3), raka.EffectiveStartDate);
            Assert.Equal(HariIni.AddDays(10), raka.EffectiveEndDate);
            Assert.Equal("Ongoing", raka.TimeStatus);
            Assert.Equal("Berlangsung", raka.TimeStatusLabel);
            Assert.Equal("SPT-2027-031", raka.AssignmentNumber);
            Assert.Equal("Bantuan IGD saat lonjakan pasien", raka.Description);
        }
    }

    // AC2: penempatan unit lain tidak tampak; filter unit di luar cakupan 403.
    [Fact]
    public async Task Penempatan_unit_lain_tidak_tampak_dan_filter_di_luar_cakupan_ditolak()
    {
        Assert.Equal(Urut(_tRaka, _tTari), await Id(_uKepalaIcu));
        Assert.Equal(Urut(_tRaka, _tNina), await Id(_uKepalaIgd));        // Nina: unit asal IGD
        Assert.Equal(Urut(_tNina, _tOki), await Id(_uKepalaFarmasi));      // tujuan Farmasi

        var (status, data, pesan) = await Daftar(_uKepalaIcu, new TemporaryAssignmentPagedQuery { OrganizationUnitId = _igd });
        Assert.Equal(403, status);
        Assert.Null(data);
        Assert.Equal(TolakBaca, pesan);

        // Filter unit dalam cakupan: asal atau tujuan = unit itu.
        Assert.Equal(Urut(_tRaka, _tTari), await Id(_uKepalaIcu, new TemporaryAssignmentPagedQuery { OrganizationUnitId = _icu }));
        Assert.Equal(Urut(_tRaka), await Id(_uKepalaIgd, new TemporaryAssignmentPagedQuery { OrganizationUnitId = _igd, TimeStatus = "Ongoing" }));

        // Tanpa penempatan berjabatan: cakupan kosong.
        Assert.Empty(await Id(_uBiasa));
        Assert.Equal(403, (await Daftar(_uBiasa, new TemporaryAssignmentPagedQuery { OrganizationUnitId = _icu })).Status);
    }

    // AC3: status waktu menurut tanggal WIB; bukan Temporary, dihapus, atau nonaktif tidak tampil.
    [Fact]
    public async Task Status_waktu_dan_hanya_penempatan_sementara_aktif()
    {
        var (_, semua, _) = await Daftar(_uHr);
        Assert.Equal(Urut(_tRaka, _tNina, _tOki, _tTari), semua!.Items.Select(x => x.Id).OrderBy(x => x).ToList());
        Assert.DoesNotContain(semua.Items, x => x.Id == _tDihapus || x.Id == _tNonaktif || x.Id == _sekunder);

        var status = semua.Items.ToDictionary(x => x.Id, x => (x.TimeStatus, x.TimeStatusLabel));
        Assert.Equal(("Ongoing", "Berlangsung"), status[_tRaka]);
        Assert.Equal(("Upcoming", "Akan Datang"), status[_tNina]);
        Assert.Equal(("Ended", "Selesai"), status[_tOki]);
        Assert.Equal(("Ongoing", "Berlangsung"), status[_tTari]);   // tanpa tanggal akhir

        Assert.Equal(Urut(_tNina), await Id(_uHr, new TemporaryAssignmentPagedQuery { TimeStatus = "Upcoming" }));
        Assert.Equal(Urut(_tRaka, _tTari), await Id(_uHr, new TemporaryAssignmentPagedQuery { TimeStatus = "ongoing" }));
        Assert.Equal(Urut(_tOki), await Id(_uHr, new TemporaryAssignmentPagedQuery { TimeStatus = "Ended" }));
        Assert.Equal(400, (await Daftar(_uHr, new TemporaryAssignmentPagedQuery { TimeStatus = "Selesai" })).Status);
    }

    // Batas hari: penempatan yang selesai hari ini masih Berlangsung; yang mulai besok Akan Datang.
    [Fact]
    public async Task Batas_hari_status_waktu()
    {
        using (var ctx = _db.CreateContext())
        {
            var raka = ctx.WfpOrganizationAssignments.Single(x => x.Id == _tRaka);
            raka.EffectiveEndDate = Tanggal(0);
            var nina = ctx.WfpOrganizationAssignments.Single(x => x.Id == _tNina);
            nina.EffectiveStartDate = Tanggal(1);
            var oki = ctx.WfpOrganizationAssignments.Single(x => x.Id == _tOki);
            oki.EffectiveEndDate = Tanggal(-1);
            var tari = ctx.WfpOrganizationAssignments.Single(x => x.Id == _tTari);
            tari.EffectiveStartDate = Tanggal(0);
            ctx.SaveChanges();
        }

        Assert.Equal(Urut(_tRaka, _tTari), await Id(_uHr, new TemporaryAssignmentPagedQuery { TimeStatus = "Ongoing" }));
        Assert.Equal(Urut(_tNina), await Id(_uHr, new TemporaryAssignmentPagedQuery { TimeStatus = "Upcoming" }));
        Assert.Equal(Urut(_tOki), await Id(_uHr, new TemporaryAssignmentPagedQuery { TimeStatus = "Ended" }));

        var status = (await Daftar(_uHr)).Data!.Items.ToDictionary(x => x.Id, x => x.TimeStatus);
        Assert.Equal("Ongoing", status[_tRaka]);   // selesai hari ini
        Assert.Equal("Ongoing", status[_tTari]);   // mulai hari ini
        Assert.Equal("Upcoming", status[_tNina]);  // mulai besok
        Assert.Equal("Ended", status[_tOki]);      // selesai kemarin
    }

    // Unit asal kosong bila pegawai tidak punya penempatan utama pada tanggal mulai; baris tetap tampil karena tujuan dalam cakupan.
    // Unit asal hanya dari penempatan utama yang berlaku pada tanggal mulai (bukan yang sudah berakhir atau bukan utama).
    [Fact]
    public async Task Unit_asal_dari_penempatan_utama_yang_berlaku_pada_tanggal_mulai()
    {
        var (_, data, _) = await Daftar(_uKepalaIcu);
        var tari = Assert.Single(data!.Items, x => x.Id == _tTari);
        Assert.Null(tari.OriginUnitName);
        Assert.Equal("ICU", tari.DestinationUnitName);
        Assert.Null(tari.EffectiveEndDate);

        var (_, semua, _) = await Daftar(_uHr);
        Assert.Equal("LAB", Assert.Single(semua!.Items, x => x.Id == _tOki).OriginUnitName);
        Assert.Equal("IGD", Assert.Single(semua.Items, x => x.Id == _tNina).OriginUnitName);
        Assert.Equal("ICU", Assert.Single(semua.Items, x => x.Id == _tRaka).OriginUnitName);
    }

    // Rentang tanggal bersinggungan, pencarian, dan paginasi di basis data (maks. 100).
    [Fact]
    public async Task Rentang_tanggal_pencarian_dan_paginasi()
    {
        Assert.Equal(Urut(_tRaka, _tNina, _tTari),
            await Id(_uHr, new TemporaryAssignmentPagedQuery { FromDate = HariIni.AddDays(6), ToDate = HariIni.AddDays(30) }));
        Assert.Equal(Urut(_tOki, _tRaka),
            await Id(_uHr, new TemporaryAssignmentPagedQuery { FromDate = HariIni.AddDays(-15), ToDate = HariIni.AddDays(-2) }));
        Assert.Equal(400, (await Daftar(_uHr, new TemporaryAssignmentPagedQuery { FromDate = HariIni, ToDate = HariIni.AddDays(-1) })).Status);

        Assert.Equal(Urut(_tRaka), await Id(_uHr, new TemporaryAssignmentPagedQuery { Search = "firman" }));
        Assert.Equal(Urut(_tOki), await Id(_uHr, new TemporaryAssignmentPagedQuery { Search = "emp-oki" }));
        Assert.Equal(Urut(_tRaka), await Id(_uHr, new TemporaryAssignmentPagedQuery { Search = "spt-2027" }));

        var (_, halaman2, _) = await Daftar(_uHr, new TemporaryAssignmentPagedQuery { PageNumber = 2, PageSize = 1 });
        Assert.Equal(4, halaman2!.TotalData);
        Assert.Equal(4, halaman2.TotalPage);
        Assert.Single(halaman2.Items);
        Assert.Equal(_tTari, halaman2.Items[0].Id);   // urut tanggal mulai terbaru: Nina, Tari, Raka, Oki

        Assert.Equal(100, (await Daftar(_uHr, new TemporaryAssignmentPagedQuery { PageSize = 500 })).Data!.PageSize);
    }

    // AC4: kedua endpoint dijaga TemporaryAssignment : Read di modul Workforce Core; tanpa kebijakan Akses Role ditolak.
    [Fact]
    public async Task Tanpa_hak_TemporaryAssignment_Read_ditolak()
    {
        var aksi = typeof(TemporaryAssignmentController).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => m.GetCustomAttribute<HttpGetAttribute>() != null)
            .ToList();
        Assert.Equal(2, aksi.Count);
        Assert.All(aksi, m =>
        {
            var izin = m.GetCustomAttribute<AccessPermissionAttribute>();
            Assert.NotNull(izin);
            Assert.Equal(new object[] { "TemporaryAssignment", "Read" }, izin!.Arguments!.Take(2).ToArray());
        });

        var snapshot = PermissionRegistryDescriptor.BuildFromAssembly(typeof(TemporaryAssignmentController).Assembly);
        Assert.Contains(snapshot.Actions, x => x.ModuleCode == "HUMAN_RESOURCE_WORKFORCE_CORE" && x.ResourceName == "TemporaryAssignment" && x.ActionName == "Read");

        // Validator startup Development tidak menemukan masalah anotasi pada controller ini.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(new DiagnosticListener("uji"));
        services.AddSingleton<DiagnosticSource>(sp => sp.GetRequiredService<DiagnosticListener>());
        services.AddControllers().AddApplicationPart(typeof(TemporaryAssignmentController).Assembly);
        using var provider = services.BuildServiceProvider();
        var hasil = new PermissionRegistryValidator(provider.GetRequiredService<IActionDescriptorCollectionProvider>(), NullLogger<PermissionRegistryValidator>.Instance)
            .ValidateAndReport(throwOnFailure: false);
        var temuan = hasil.UnregisterableEndpoints.Concat(hasil.DuplicateResourceIdentities).Concat(hasil.InvalidAccessTypes)
            .Concat(hasil.UnenforcedEndpoints).Concat(hasil.NakedBusinessEndpoints)
            .Where(x => x.StartsWith("TemporaryAssignment.", StringComparison.Ordinal) || x.Contains("'TemporaryAssignment'", StringComparison.Ordinal))
            .ToList();
        Assert.True(temuan.Count == 0, string.Join(Environment.NewLine, temuan));

        // Pegawai berjabatan (Kepala ICU) tanpa kebijakan Akses Role ditolak; superadmin lolos.
        Guid super = Guid.NewGuid();
        using (var ctx = _db.CreateContext())
        {
            ctx.Users.Add(new ApplicationUser { Id = super, UserName = "super", UserCode = "SUPER01", DisplayName = "Super", UserType = UserType.SuperAdmin, IsActive = true });
            ctx.SaveChanges();
        }
        using var baca = _db.CreateContext();
        Assert.False(await Pelaku.Izin(baca).HasAccessAsync(Pelaku.Sebagai(_uKepalaIcu).HttpContext!.User, "TemporaryAssignment", "Read"));
        Assert.True(await Pelaku.Izin(baca).HasAccessAsync(Pelaku.Sebagai(super).HttpContext!.User, "TemporaryAssignment", "Read"));
    }

    [Fact]
    public void Metadata_memuat_tiga_status_waktu()
    {
        var hasil = Assert.IsType<OkObjectResult>(Sebagai(_uHr).GetFilterMetadata());
        var data = Assert.IsType<ApiResponse<TemporaryAssignmentFilterMetadataResponse>>(hasil.Value).Data!;
        Assert.Equal(new[] { "Upcoming", "Ongoing", "Ended" }, data.TimeStatusOptions.Select(x => x.Value));
        Assert.Equal(100, data.PageSizeOptions.Max());
    }
}
