using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Seeders;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Bukti untuk <c>BE-LAB-06</c> — pengelolaan alasan penolakan sampel
/// (<c>FR-06.1</c> .. <c>FR-06.3</c>, <c>LAB-DEC-019</c>, <c>BR-15</c>).
///
/// Kelima baris <c>AC-26</c> pada <c>testing/acceptance-test-matrix.md</c> dibuktikan satu per
/// satu:
///   1. kepala instalasi menambah "Sampel tidak diberi label", dan penanda kesalahan internal
///      bernilai bawaan karena memang tidak dapat diisi dari permintaan;
///   2. <b>gagal</b> — kepala instalasi mengubah penanda kesalahan internal ditolak
///      <c>403</c> (<c>VAL-37</c>), dan penandanya tidak berubah;
///   3. administrator sistem menyetel penanda itu, dan perubahannya berhasil;
///   4. <b>gagal</b> — kode ganda ditolak <c>409</c> (<c>VAL-36</c>);
///   5. <b>gagal</b> — menonaktifkan alasan aktif terakhir ditolak <c>422</c> (<c>VAL-38</c>).
///
/// Ditambah bukti bahwa jalur baca lama <c>GET /lab-specimens/rejection-reasons</c> tidak
/// berubah perilakunya, dan bahwa seeder mengisi data awal tanpa menimpa keputusan pengguna.
/// </summary>
/// <remarks>
/// Provider InMemory dipakai supaya bukti ini berjalan tanpa database mana pun. Dua
/// konsekuensinya disebut apa adanya:
///
///   1. index unik fisik tidak ditegakkan di sini, sehingga <c>VAL-36</c> yang diuji adalah
///      pemeriksaan di service; penjaga terakhirnya adalah
///      <c>IX_MstLabRejectionReason_ReasonCode</c> yang sudah ada sejak migration
///      <c>20260824091610_AddLaboratorySpecimenLifecycle</c>;
///   2. penyaringan <c>Search</c> memakai <c>EF.Functions.ILike</c> yang hanya ada pada
///      PostgreSQL, sehingga jalur itu tidak diuji di sini.
///
/// Pengujian menyentuh service, bukan HTTP. Pemetaan exception menjadi kode status
/// — <c>403</c>, <c>409</c>, <c>422</c>, <c>404</c>, <c>400</c> — dilakukan controller lewat
/// tipe exception yang diperiksa di sini, dan pemetaannya sendiri diperiksa pada bagian
/// kontrak di bawah.
/// </remarks>
public class LabRejectionReasonServiceTests
{
    private static readonly Guid KepalaInstalasi = Guid.Parse("44444444-4444-4444-4444-444444444444");
    private static readonly Guid AdministratorSistem = Guid.Parse("55555555-5555-5555-5555-555555555555");

    // =====================================================================
    // 1. AC-26 jalur berhasil — kepala instalasi menambah alasan baru
    // =====================================================================

    [Fact]
    public async Task AC26_KepalaInstalasiMenambahAlasanBaru_LangsungTersimpanDanAktif()
    {
        await using var context = CreateContext();
        var service = CreateService(context, KepalaInstalasi);

        var hasil = await service.CreateAsync(new CreateLabRejectionReasonRequest
        {
            ReasonCode = "UNLABELED_SPECIMEN",
            ReasonName = "Sampel tidak diberi label",
            Description = "Wadah tiba tanpa label identitas pasien.",
            SortOrder = 10
        });

        Assert.Equal("UNLABELED_SPECIMEN", hasil.ReasonCode);
        Assert.Equal("Sampel tidak diberi label", hasil.ReasonName);
        Assert.Equal(10, hasil.SortOrder);

        // Alasan baru langsung aktif, sehingga petugas dapat memakainya saat itu juga.
        Assert.True(hasil.IsActive);

        // AC-26: kedua penanda terkunci bernilai bawaan, bukan nilai kiriman.
        Assert.False(hasil.IsInternalHospitalError);
        Assert.False(hasil.RequiresNote);

        var tersimpan = await context.MstLabRejectionReasons
            .AsNoTracking()
            .SingleAsync(x => x.ReasonCode == "UNLABELED_SPECIMEN");

        Assert.True(tersimpan.IsActive);
        Assert.False(tersimpan.IsInternalHospitalError);
        Assert.False(tersimpan.RequiresNote);
        Assert.Equal(KepalaInstalasi, tersimpan.CreateBy);
    }

    /// <summary>
    /// Bukti bentuk kontrak: penanda terkunci memang <b>tidak ada</b> pada permintaan
    /// pembuatan, sehingga tidak ada jalan mengisinya walaupun pemanggil mengirimkannya.
    /// </summary>
    [Fact]
    public void AC26_PermintaanMembuatAlasan_TidakPunyaRuasPenandaTerkunciSamaSekali()
    {
        var properties = typeof(CreateLabRejectionReasonRequest)
            .GetProperties()
            .Select(x => x.Name)
            .ToList();

        Assert.DoesNotContain(nameof(MstLabRejectionReason.IsInternalHospitalError), properties);
        Assert.DoesNotContain(nameof(MstLabRejectionReason.RequiresNote), properties);
    }

    [Fact]
    public async Task MembuatAlasan_KodeDinormalkanMenjadiHurufKapital()
    {
        await using var context = CreateContext();
        var service = CreateService(context, KepalaInstalasi);

        var hasil = await service.CreateAsync(new CreateLabRejectionReasonRequest
        {
            ReasonCode = "  unlabeled_specimen  ",
            ReasonName = "  Sampel tidak diberi label  ",
            SortOrder = 10
        });

        Assert.Equal("UNLABELED_SPECIMEN", hasil.ReasonCode);
        Assert.Equal("Sampel tidak diberi label", hasil.ReasonName);
    }

    // =====================================================================
    // 2. VAL-36 — kode ganda ditolak 409
    // =====================================================================

    [Fact]
    public async Task VAL36_MenambahAlasanDenganKodeYangSudahDipakai_Ditolak()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        var galat = await Assert.ThrowsAsync<LabRejectionReasonConflictException>(() =>
            service.CreateAsync(new CreateLabRejectionReasonRequest
            {
                ReasonCode = "CLOTTED",
                ReasonName = "Sampel menggumpal — duplikat",
                SortOrder = 50
            }));

        Assert.Equal(
            "Kode alasan ini sudah dipakai data lain, jadi tidak bisa disimpan.",
            galat.Message);

        // Tidak ada baris kedua yang terlanjur tersimpan.
        var jumlah = await context.MstLabRejectionReasons
            .AsNoTracking()
            .CountAsync(x => x.ReasonCode == "CLOTTED");

        Assert.Equal(1, jumlah);
    }

    /// <summary>
    /// Normalisasi kode membuat "clotted" dan "CLOTTED" terhitung sebagai kode yang sama.
    /// Tanpa ini, dua baris berbeda akan lolos dan petugas melihat dua alasan kembar.
    /// </summary>
    [Fact]
    public async Task VAL36_KodeYangSamaDenganHurufKecil_TetapDianggapGanda()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        await Assert.ThrowsAsync<LabRejectionReasonConflictException>(() =>
            service.CreateAsync(new CreateLabRejectionReasonRequest
            {
                ReasonCode = "clotted",
                ReasonName = "Sampel menggumpal",
                SortOrder = 51
            }));
    }

    // =====================================================================
    // 3. VAL-37 — penanda terkunci tidak dapat diubah kepala instalasi
    // =====================================================================

    [Fact]
    public async Task VAL37_KepalaInstalasiMengubahPenandaKesalahanInternal_Ditolak()
    {
        await using var context = CreateContext();
        var (clottedId, _, _) = await SeedAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        var galat = await Assert.ThrowsAsync<LabRejectionReasonForbiddenException>(() =>
            service.UpdateAsync(clottedId, new UpdateLabRejectionReasonRequest
            {
                ReasonName = "Sampel menggumpal",
                SortOrder = 1,
                IsInternalHospitalError = false
            }));

        Assert.Equal(
            "Kedua penanda ini hanya dapat diubah administrator sistem, karena menentukan siapa menanggung biaya pengambilan ulang.",
            galat.Message);

        context.ChangeTracker.Clear();

        // Penandanya tidak berubah — inilah inti AC-26 jalur gagal.
        var tersimpan = await context.MstLabRejectionReasons
            .AsNoTracking()
            .SingleAsync(x => x.Id == clottedId);

        Assert.True(tersimpan.IsInternalHospitalError);
    }

    [Fact]
    public async Task VAL37_KepalaInstalasiMengubahPenandaWajibCatatan_Ditolak()
    {
        await using var context = CreateContext();
        var (_, _, otherId) = await SeedAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        await Assert.ThrowsAsync<LabRejectionReasonForbiddenException>(() =>
            service.UpdateAsync(otherId, new UpdateLabRejectionReasonRequest
            {
                ReasonName = "Lainnya",
                SortOrder = 99,
                RequiresNote = false
            }));

        context.ChangeTracker.Clear();

        var tersimpan = await context.MstLabRejectionReasons
            .AsNoTracking()
            .SingleAsync(x => x.Id == otherId);

        Assert.True(tersimpan.RequiresNote);
    }

    /// <summary>
    /// Penolakan terjadi sebelum satu ruas pun disentuh, sehingga permintaan yang menyelipkan
    /// penanda terkunci tidak menyisakan perubahan nama atau urutan yang terlanjur tersimpan.
    /// </summary>
    [Fact]
    public async Task VAL37_PermintaanDitolakSeluruhnya_NamaDanUrutanTidakIkutBerubah()
    {
        await using var context = CreateContext();
        var (clottedId, _, _) = await SeedAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        await Assert.ThrowsAsync<LabRejectionReasonForbiddenException>(() =>
            service.UpdateAsync(clottedId, new UpdateLabRejectionReasonRequest
            {
                ReasonName = "Nama baru yang tidak boleh tersimpan",
                SortOrder = 77,
                IsInternalHospitalError = true
            }));

        context.ChangeTracker.Clear();

        var tersimpan = await context.MstLabRejectionReasons
            .AsNoTracking()
            .SingleAsync(x => x.Id == clottedId);

        Assert.Equal("Sampel menggumpal", tersimpan.ReasonName);
        Assert.Equal(1, tersimpan.SortOrder);
    }

    [Fact]
    public async Task MengubahAlasanTanpaPenandaTerkunci_Berhasil()
    {
        await using var context = CreateContext();
        var (clottedId, _, _) = await SeedAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        var hasil = await service.UpdateAsync(clottedId, new UpdateLabRejectionReasonRequest
        {
            ReasonName = "Sampel menggumpal (bekuan terlihat)",
            Description = "Terlihat bekuan pada tabung EDTA.",
            SortOrder = 3
        });

        Assert.Equal("Sampel menggumpal (bekuan terlihat)", hasil.ReasonName);
        Assert.Equal("Terlihat bekuan pada tabung EDTA.", hasil.Description);
        Assert.Equal(3, hasil.SortOrder);

        // Penanda terkunci tetap seperti semula, tidak ikut tersapu menjadi false.
        Assert.True(hasil.IsInternalHospitalError);

        context.ChangeTracker.Clear();

        var tersimpan = await context.MstLabRejectionReasons
            .AsNoTracking()
            .SingleAsync(x => x.Id == clottedId);

        Assert.Equal(KepalaInstalasi, tersimpan.UpdateBy);
    }

    [Fact]
    public async Task MengubahAlasanYangTidakAda_Ditolak()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            service.UpdateAsync(Guid.NewGuid(), new UpdateLabRejectionReasonRequest
            {
                ReasonName = "Tidak ada",
                SortOrder = 1
            }));
    }

    // =====================================================================
    // 4. AC-26 — administrator sistem menyetel penanda terkunci
    // =====================================================================

    [Fact]
    public async Task AC26_AdministratorSistemMenyetelPenandaKesalahanInternal_Berhasil()
    {
        await using var context = CreateContext();
        var (_, insufficientId, _) = await SeedAsync(context);
        var service = CreateService(context, AdministratorSistem);

        var hasil = await service.SetSystemFlagsAsync(insufficientId, new SetLabRejectionReasonSystemFlagsRequest
        {
            IsInternalHospitalError = true,
            RequiresNote = true,
            ChangeReason = "Disepakati bersama Billing pada rapat 2026-09-03."
        });

        Assert.True(hasil.IsInternalHospitalError);
        Assert.True(hasil.RequiresNote);

        context.ChangeTracker.Clear();

        var tersimpan = await context.MstLabRejectionReasons
            .AsNoTracking()
            .SingleAsync(x => x.Id == insufficientId);

        Assert.True(tersimpan.IsInternalHospitalError);
        Assert.True(tersimpan.RequiresNote);
        Assert.Equal(AdministratorSistem, tersimpan.UpdateBy);
    }

    [Fact]
    public async Task MenyetelPenandaSistem_DapatJugaMencabutnya()
    {
        await using var context = CreateContext();
        var (clottedId, _, _) = await SeedAsync(context);
        var service = CreateService(context, AdministratorSistem);

        var hasil = await service.SetSystemFlagsAsync(clottedId, new SetLabRejectionReasonSystemFlagsRequest
        {
            IsInternalHospitalError = false,
            RequiresNote = false
        });

        Assert.False(hasil.IsInternalHospitalError);
        Assert.False(hasil.RequiresNote);
    }

    // =====================================================================
    // 5. VAL-38 — alasan aktif terakhir tidak boleh dinonaktifkan
    // =====================================================================

    [Fact]
    public async Task VAL38_MenonaktifkanAlasanAktifTerakhir_Ditolak()
    {
        await using var context = CreateContext();
        var satuSatunya = await SeedSatuAlasanAktifAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        var galat = await Assert.ThrowsAsync<LabRejectionReasonValidationException>(() =>
            service.SetActivationAsync(satuSatunya, new SetLabRejectionReasonActivationRequest
            {
                IsActive = false
            }));

        Assert.Equal(
            "Sekurang-kurangnya satu alasan penolakan harus tetap aktif.",
            galat.Message);

        context.ChangeTracker.Clear();

        var tersimpan = await context.MstLabRejectionReasons
            .AsNoTracking()
            .SingleAsync(x => x.Id == satuSatunya);

        Assert.True(tersimpan.IsActive);
    }

    [Fact]
    public async Task MenonaktifkanAlasan_BerhasilSelamaMasihAdaYangLainAktif()
    {
        await using var context = CreateContext();
        var (clottedId, _, _) = await SeedAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        var hasil = await service.SetActivationAsync(clottedId, new SetLabRejectionReasonActivationRequest
        {
            IsActive = false
        });

        Assert.False(hasil.IsActive);
    }

    [Fact]
    public async Task MengaktifkanKembaliAlasanYangNonaktif_Berhasil()
    {
        await using var context = CreateContext();
        var (clottedId, _, _) = await SeedAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        await service.SetActivationAsync(clottedId, new SetLabRejectionReasonActivationRequest { IsActive = false });
        var hasil = await service.SetActivationAsync(clottedId, new SetLabRejectionReasonActivationRequest { IsActive = true });

        Assert.True(hasil.IsActive);
    }

    /// <summary>
    /// Menonaktifkan alasan yang memang sudah nonaktif bukan kesalahan; ia hanya tidak
    /// mengubah apa pun, termasuk tidak memicu <c>VAL-38</c>.
    /// </summary>
    [Fact]
    public async Task MenonaktifkanAlasanYangSudahNonaktif_TidakMengubahApaPun()
    {
        await using var context = CreateContext();
        var satuSatunya = await SeedSatuAlasanAktifAsync(context);

        var nonaktif = new MstLabRejectionReason
        {
            Id = Guid.NewGuid(),
            ReasonCode = "ARCHIVED",
            ReasonName = "Alasan lama",
            IsActive = false,
            SortOrder = 90
        };

        context.MstLabRejectionReasons.Add(nonaktif);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = CreateService(context, KepalaInstalasi);

        var hasil = await service.SetActivationAsync(nonaktif.Id, new SetLabRejectionReasonActivationRequest
        {
            IsActive = false
        });

        Assert.False(hasil.IsActive);
        Assert.NotEqual(Guid.Empty, satuSatunya);
    }

    // =====================================================================
    // 6. Daftar untuk layar pengelolaan
    // =====================================================================

    [Fact]
    public async Task DaftarPengelolaan_MenampilkanAlasanNonaktifJuga()
    {
        await using var context = CreateContext();
        var (clottedId, _, _) = await SeedAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        await service.SetActivationAsync(clottedId, new SetLabRejectionReasonActivationRequest { IsActive = false });
        context.ChangeTracker.Clear();

        var semua = await service.GetListAsync(new LabRejectionReasonPagedQuery());

        Assert.Equal(3, semua.TotalData);
        Assert.Contains(semua.Items, x => !x.IsActive);

        var hanyaAktif = await service.GetListAsync(new LabRejectionReasonPagedQuery { IsActive = true });

        Assert.Equal(2, hanyaAktif.TotalData);
        Assert.All(hanyaAktif.Items, x => Assert.True(x.IsActive));
    }

    [Fact]
    public async Task DaftarPengelolaan_DiurutkanMenurutUrutanTampilLaluKode()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        var hasil = await service.GetListAsync(new LabRejectionReasonPagedQuery());

        Assert.Equal(
            new[] { "CLOTTED", "INSUFFICIENT_QUANTITY", "OTHER" },
            hasil.Items.Select(x => x.ReasonCode));
    }

    [Fact]
    public async Task DaftarPengelolaan_MemakaiBentukPagingYangSudahMapan()
    {
        await using var context = CreateContext();
        await SeedAsync(context);
        var service = CreateService(context, KepalaInstalasi);

        var hasil = await service.GetListAsync(new LabRejectionReasonPagedQuery
        {
            PageNumber = 2,
            PageSize = 2
        });

        Assert.Equal(2, hasil.PageNumber);
        Assert.Equal(2, hasil.PageSize);
        Assert.Equal(3, hasil.TotalData);
        Assert.Equal(2, hasil.TotalPage);
        Assert.Single(hasil.Items);
        Assert.Equal("OTHER", hasil.Items[0].ReasonCode);
    }

    [Fact]
    public async Task DaftarPengelolaan_TidakMenampilkanBarisYangSudahDitandaiTerhapus()
    {
        await using var context = CreateContext();
        var (clottedId, _, _) = await SeedAsync(context);

        var terhapus = await context.MstLabRejectionReasons.SingleAsync(x => x.Id == clottedId);
        terhapus.IsDelete = true;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var service = CreateService(context, KepalaInstalasi);
        var hasil = await service.GetListAsync(new LabRejectionReasonPagedQuery());

        Assert.Equal(2, hasil.TotalData);
        Assert.DoesNotContain(hasil.Items, x => x.ReasonCode == "CLOTTED");
    }

    // =====================================================================
    // 7. Bentuk kontrak — LAB-API-v1 r3 grup Lab Rejection Reason
    // =====================================================================

    [Theory]
    [InlineData(nameof(LabRejectionReasonController.GetList), "Read", typeof(HttpGetAttribute), null)]
    [InlineData(nameof(LabRejectionReasonController.Create), "Create", typeof(HttpPostAttribute), null)]
    [InlineData(nameof(LabRejectionReasonController.Update), "Update", typeof(HttpPutAttribute), "{id:guid}")]
    [InlineData(nameof(LabRejectionReasonController.SetActivation), "Update", typeof(HttpPutAttribute), "{id:guid}/activation")]
    [InlineData(nameof(LabRejectionReasonController.SetSystemFlags), "SystemFlag", typeof(HttpPutAttribute), "{id:guid}/system-flags")]
    public void KelimaEndpoint_MemakaiRouteDanPermissionYangDikunciKontrak(
        string methodName,
        string action,
        Type verbAttribute,
        string? template)
    {
        var method = typeof(LabRejectionReasonController).GetMethod(methodName);

        Assert.NotNull(method);

        var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();

        Assert.NotNull(permission);

        var arguments = Assert.IsType<object[]>(permission!.Arguments);

        Assert.Equal("LabRejectionReason", arguments[0]);
        Assert.Equal(action, arguments[1]);

        var verb = method.GetCustomAttributes(verbAttribute, inherit: false).SingleOrDefault();

        Assert.NotNull(verb);
        Assert.Equal(template, ((IRouteTemplateProvider)verb!).Template);
    }

    [Fact]
    public void ControllerPengelolaan_MemakaiBaseRouteYangDikunciKontrak()
    {
        var route = typeof(LabRejectionReasonController).GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(route);
        Assert.Equal(
            "api/v1/health-services/laboratory-management/lab-rejection-reasons",
            route!.Template);

        var endpoints = typeof(LabRejectionReasonController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttributes<AccessPermissionAttribute>().Any())
            .ToList();

        Assert.Equal(5, endpoints.Count);

        // Hanya satu endpoint yang menuntut SystemFlag. Pemisahan inilah yang membuat kepala
        // instalasi tidak dapat memindahkan beban biaya pengambilan ulang sendirian.
        var systemFlagCount = endpoints.Count(x =>
            ((object[])x.GetCustomAttribute<AccessPermissionAttribute>()!.Arguments!)[1] as string == "SystemFlag");

        Assert.Equal(1, systemFlagCount);
    }

    /// <summary>
    /// Grup ini tidak memiliki jalur hapus. Alasan penolakan yang sudah pernah dipakai menempel
    /// pada riwayat penolakan sampel; ia dinonaktifkan, bukan dihapus.
    /// </summary>
    [Fact]
    public void ControllerPengelolaan_TidakMemilikiJalurHapus()
    {
        var deleteEndpoints = typeof(LabRejectionReasonController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttributes<HttpDeleteAttribute>().Any())
            .ToList();

        Assert.Empty(deleteEndpoints);
    }

    // =====================================================================
    // 8. Jalur baca lama tidak berubah perilakunya
    // =====================================================================

    /// <summary>
    /// <c>GET /lab-specimens/rejection-reasons</c> tetap berdiri dengan route, verb, dan hak
    /// akses yang sama seperti sebelum <c>BE-LAB-06</c>. Endpoint pengelolaan adalah jalur
    /// terpisah, bukan penggantinya.
    /// </summary>
    [Fact]
    public void JalurBacaLama_RouteDanHakAksesnyaTidakBerubah()
    {
        var method = typeof(LabSpecimenController)
            .GetMethod(nameof(LabSpecimenController.GetRejectionReasons));

        Assert.NotNull(method);

        var verb = method!.GetCustomAttribute<HttpGetAttribute>();

        Assert.NotNull(verb);
        Assert.Equal("rejection-reasons", verb!.Template);

        var permission = method.GetCustomAttribute<AccessPermissionAttribute>();
        var arguments = Assert.IsType<object[]>(permission!.Arguments);

        Assert.Equal("LabSpecimen", arguments[0]);
        Assert.Equal("Read", arguments[1]);
    }

    /// <summary>
    /// Bentuk muatan jalur baca lama hanya bertambah satu ruas <c>IsActive</c>; tujuh ruas
    /// aslinya tetap ada dengan nama dan tipe yang sama, sehingga pemanggil lama tidak rusak.
    /// </summary>
    [Fact]
    public void BentukTanggapanAlasanPenolakan_TetapMemuatSeluruhRuasAsli()
    {
        var properties = typeof(LabRejectionReasonResponse)
            .GetProperties()
            .ToDictionary(x => x.Name, x => x.PropertyType);

        Assert.Equal(typeof(Guid), properties[nameof(LabRejectionReasonResponse.Id)]);
        Assert.Equal(typeof(string), properties[nameof(LabRejectionReasonResponse.ReasonCode)]);
        Assert.Equal(typeof(string), properties[nameof(LabRejectionReasonResponse.ReasonName)]);
        Assert.Equal(typeof(string), properties[nameof(LabRejectionReasonResponse.Description)]);
        Assert.Equal(typeof(bool), properties[nameof(LabRejectionReasonResponse.IsInternalHospitalError)]);
        Assert.Equal(typeof(bool), properties[nameof(LabRejectionReasonResponse.RequiresNote)]);
        Assert.Equal(typeof(int), properties[nameof(LabRejectionReasonResponse.SortOrder)]);
        Assert.Equal(typeof(bool), properties[nameof(LabRejectionReasonResponse.IsActive)]);
        Assert.Equal(8, properties.Count);
    }

    // =====================================================================
    // 9. Seeder data awal
    // =====================================================================

    [Fact]
    public async Task Seeder_MengisiTabelKosongDenganSepuluhAlasanBaseline()
    {
        await using var context = CreateContext();

        var ditambahkan = await LabRejectionReasonSeeder.SeedAsync(
            context, NullLogger.Instance);

        Assert.Equal(10, ditambahkan);

        context.ChangeTracker.Clear();

        var tersimpan = await context.MstLabRejectionReasons.AsNoTracking().ToListAsync();

        Assert.Equal(10, tersimpan.Count);
        Assert.All(tersimpan, x => Assert.True(x.IsActive));

        // Petugas selalu punya jalan keluar untuk keadaan yang tidak terdaftar, dan jalan itu
        // menuntut catatan supaya jejak auditnya tetap lengkap.
        var lainnya = tersimpan.Single(x => x.ReasonCode == "OTHER");

        Assert.True(lainnya.RequiresNote);
        Assert.False(lainnya.IsInternalHospitalError);
    }

    [Fact]
    public async Task Seeder_DijalankanDuaKali_TidakMenambahBarisKembar()
    {
        await using var context = CreateContext();

        await LabRejectionReasonSeeder.SeedAsync(context, NullLogger.Instance);
        context.ChangeTracker.Clear();

        var ditambahkanLagi = await LabRejectionReasonSeeder.SeedAsync(
            context, NullLogger.Instance);

        Assert.Equal(0, ditambahkanLagi);

        var jumlah = await context.MstLabRejectionReasons.AsNoTracking().CountAsync();

        Assert.Equal(10, jumlah);
    }

    /// <summary>
    /// Inti keselamatan seeder ini: nama, urutan, status aktif, dan penanda terkunci yang sudah
    /// diputuskan pengguna tidak boleh tersapu setiap kali server dinyalakan ulang.
    /// </summary>
    [Fact]
    public async Task Seeder_TidakMenimpaBarisYangSudahDiubahPengguna()
    {
        await using var context = CreateContext();

        await LabRejectionReasonSeeder.SeedAsync(context, NullLogger.Instance);
        context.ChangeTracker.Clear();

        var lainnya = await context.MstLabRejectionReasons.SingleAsync(x => x.ReasonCode == "OTHER");
        lainnya.ReasonName = "Lain-lain menurut SOP 2026";
        lainnya.SortOrder = 50;
        lainnya.IsActive = false;
        lainnya.RequiresNote = false;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        await LabRejectionReasonSeeder.SeedAsync(context, NullLogger.Instance);
        context.ChangeTracker.Clear();

        var sesudah = await context.MstLabRejectionReasons
            .AsNoTracking()
            .SingleAsync(x => x.ReasonCode == "OTHER");

        Assert.Equal("Lain-lain menurut SOP 2026", sesudah.ReasonName);
        Assert.Equal(50, sesudah.SortOrder);
        Assert.False(sesudah.IsActive);
        Assert.False(sesudah.RequiresNote);
    }

    /// <summary>
    /// Baris baseline yang sengaja dihapus pengguna tidak dihidupkan lagi oleh seeder.
    /// </summary>
    [Fact]
    public async Task Seeder_TidakMenghidupkanKembaliBarisYangSudahDihapus()
    {
        await using var context = CreateContext();

        await LabRejectionReasonSeeder.SeedAsync(context, NullLogger.Instance);
        context.ChangeTracker.Clear();

        var dihapus = await context.MstLabRejectionReasons.SingleAsync(x => x.ReasonCode == "DUPLICATE_OR_NOT_REQUIRED");
        dihapus.IsDelete = true;
        dihapus.IsActive = false;
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var ditambahkanLagi = await LabRejectionReasonSeeder.SeedAsync(
            context, NullLogger.Instance);

        Assert.Equal(0, ditambahkanLagi);

        var jumlah = await context.MstLabRejectionReasons
            .AsNoTracking()
            .CountAsync(x => x.ReasonCode == "DUPLICATE_OR_NOT_REQUIRED");

        Assert.Equal(1, jumlah);
    }

    /// <summary>
    /// Identitas baris baseline sama persis dengan yang dipakai migration
    /// <c>20260824091610_AddLaboratorySpecimenLifecycle</c>. Bila keduanya berbeda, lingkungan
    /// yang menjalankan migration lalu seeder akan memiliki dua baris untuk alasan yang sama.
    /// </summary>
    [Fact]
    public async Task Seeder_MemakaiIdentitasYangSamaDenganMigration()
    {
        await using var context = CreateContext();

        await LabRejectionReasonSeeder.SeedAsync(context, NullLogger.Instance);
        context.ChangeTracker.Clear();

        var identityMismatch = await context.MstLabRejectionReasons
            .AsNoTracking()
            .SingleAsync(x => x.ReasonCode == "IDENTITY_MISMATCH");

        Assert.Equal(
            Guid.Parse("1f2a4c60-0001-4a10-9f01-6b1d0a5e7c01"),
            identityMismatch.Id);
        Assert.True(identityMismatch.IsInternalHospitalError);
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-rejection-reason-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static LabRejectionReasonService CreateService(
        ApplicationDbContext context,
        Guid actorUserId)
    {
        var httpContextAccessor = CreateHttpContextAccessor(actorUserId);

        return new LabRejectionReasonService(
            context,
            httpContextAccessor,
            new LoggerService(NullLogger<LoggerService>.Instance, httpContextAccessor));
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(Guid actorUserId)
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
            authenticationType: "LabRejectionReasonServiceTest");

        return new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    /// <summary>
    /// Tiga alasan penolakan: satu berpenanda kesalahan internal, satu tanpa penanda apa pun,
    /// dan satu yang mewajibkan catatan.
    /// </summary>
    private static async Task<(Guid ClottedId, Guid InsufficientId, Guid OtherId)> SeedAsync(
        ApplicationDbContext context)
    {
        var clotted = new MstLabRejectionReason
        {
            Id = Guid.NewGuid(),
            ReasonCode = "CLOTTED",
            ReasonName = "Sampel menggumpal",
            IsInternalHospitalError = true,
            RequiresNote = false,
            IsActive = true,
            SortOrder = 1
        };

        var insufficient = new MstLabRejectionReason
        {
            Id = Guid.NewGuid(),
            ReasonCode = "INSUFFICIENT_QUANTITY",
            ReasonName = "Jumlah sampel tidak mencukupi",
            IsInternalHospitalError = false,
            RequiresNote = false,
            IsActive = true,
            SortOrder = 2
        };

        var other = new MstLabRejectionReason
        {
            Id = Guid.NewGuid(),
            ReasonCode = "OTHER",
            ReasonName = "Lainnya",
            IsInternalHospitalError = false,
            RequiresNote = true,
            IsActive = true,
            SortOrder = 99
        };

        context.MstLabRejectionReasons.AddRange(clotted, insufficient, other);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return (clotted.Id, insufficient.Id, other.Id);
    }

    private static async Task<Guid> SeedSatuAlasanAktifAsync(ApplicationDbContext context)
    {
        var satuSatunya = new MstLabRejectionReason
        {
            Id = Guid.NewGuid(),
            ReasonCode = "CLOTTED",
            ReasonName = "Sampel menggumpal",
            IsActive = true,
            SortOrder = 1
        };

        context.MstLabRejectionReasons.Add(satuSatunya);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return satuSatunya.Id;
    }
}
