using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using System.Reflection;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.MasterData;

/// <summary>
/// Bukti untuk daftar pilihan data induk perujuk — penutup butir Verifikasi <c>BE-EXT-02</c>
/// yang berbunyi *"kedua data induk dapat dipilih dari daftar"*.
///
/// <b>Kenapa butir itu sempat tidak terpenuhi.</b> <c>BE-EXT-02</c> membuat kedua entity
/// beserta <c>DbSet</c>-nya, tetapi tidak membuat satu pun endpoint — dan laporannya menyatakan
/// itu apa adanya. Selama endpointnya tidak ada, layar pendaftaran rujukan luar
/// (<c>FE-LAB-05</c>) tidak punya sumber pilihan, sehingga satu-satunya cara mengisi perujuk
/// adalah mengetiknya — persis yang dilarang <c>LAB-DEC-035</c> dan <c>AC-50</c>.
///
/// Yang dibuktikan di sini:
///   1. kedua grup endpoint <b>baca saja</b> — tidak ada satu pun jalur ubah;
///   2. penyaring aktif, pencarian, dan pengurutannya bekerja;
///   3. daftar dokter dapat disaring menurut instansinya, dan membawa nama instansi itu.
/// </summary>
public class ReferralMasterDataOptionsTests
{
    // =====================================================================
    // 1. Bentuk kontrak — baca saja
    // =====================================================================

    [Theory]
    [InlineData(typeof(ReferralInstitutionController), "ReferralInstitution")]
    [InlineData(typeof(ReferralDoctorController), "ReferralDoctor")]
    public void KeduaGrup_HanyaPunyaSatuJalurBacaDanTidakSatuPunJalurUbah(
        Type controllerType,
        string permissionResource)
    {
        var methods = controllerType
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(x => x.GetCustomAttributes<HttpMethodAttribute>().Any())
            .ToList();

        var method = Assert.Single(methods);

        var verb = Assert.IsType<HttpGetAttribute>(
            method.GetCustomAttributes().Single(x => x is HttpMethodAttribute));

        Assert.Equal("options", verb.Template);

        var permission = method.GetCustomAttribute<AccessPermissionAttribute>();

        Assert.NotNull(permission);

        var arguments = Assert.IsType<object[]>(permission!.Arguments);

        Assert.Equal(permissionResource, arguments[0]);
        Assert.Equal("Read", arguments[1]);

        // Penambahan dan penyuntingan data induk perujuk adalah pekerjaan modul Data Induk,
        // bukan modul yang memakainya. Ketiadaan jalur ubah di sini disengaja.
        var verbs = methods
            .SelectMany(x => x.GetCustomAttributes<HttpMethodAttribute>())
            .ToList();

        Assert.DoesNotContain(verbs, x => x is HttpPostAttribute);
        Assert.DoesNotContain(verbs, x => x is HttpPutAttribute);
        Assert.DoesNotContain(verbs, x => x is HttpDeleteAttribute);
        Assert.DoesNotContain(verbs, x => x is HttpPatchAttribute);
    }

    // =====================================================================
    // 2. Instansi perujuk
    // =====================================================================

    [Fact]
    public async Task DaftarInstansi_HanyaMemuatYangAktifDanTerurutMenurutNama()
    {
        using var context = CreateInMemoryContext();

        await SeedInstitutionAsync(context, "INS-02", "Puskesmas Melati");
        await SeedInstitutionAsync(context, "INS-01", "Klinik Sehat Sentosa");
        await SeedInstitutionAsync(context, "INS-03", "Klinik Tutup", isActive: false);

        var service = new ReferralMasterDataService(context);

        var hasil = await service.GetInstitutionOptionsAsync(new ReferralInstitutionOptionQuery());

        Assert.Equal(2, hasil.TotalData);
        Assert.Collection(
            hasil.Items,
            x => Assert.Equal("Klinik Sehat Sentosa", x.InstitutionName),
            x => Assert.Equal("Puskesmas Melati", x.InstitutionName));
    }

    /// <summary>
    /// Instansi yang tidak lagi bekerja sama <b>dinonaktifkan, bukan dihapus</b>, sehingga
    /// kunjungan lama yang menunjuknya tetap dapat dibaca. Karena itu ia tetap dapat
    /// ditampilkan bila memang diminta.
    /// </summary>
    [Fact]
    public async Task DaftarInstansi_DapatMenyertakanYangNonaktifBilaDiminta()
    {
        using var context = CreateInMemoryContext();

        await SeedInstitutionAsync(context, "INS-01", "Klinik Sehat Sentosa");
        await SeedInstitutionAsync(context, "INS-03", "Klinik Tutup", isActive: false);

        var service = new ReferralMasterDataService(context);

        var hasil = await service.GetInstitutionOptionsAsync(
            new ReferralInstitutionOptionQuery { OnlyActive = false });

        Assert.Equal(2, hasil.TotalData);
    }

    [Fact]
    public async Task DaftarInstansi_DapatDicariLewatKodeMaupunNama()
    {
        using var context = CreateInMemoryContext();

        await SeedInstitutionAsync(context, "INS-01", "Klinik Sehat Sentosa");
        await SeedInstitutionAsync(context, "INS-02", "Puskesmas Melati");

        var service = new ReferralMasterDataService(context);

        var lewatNama = await service.GetInstitutionOptionsAsync(
            new ReferralInstitutionOptionQuery { Search = "Sentosa" });

        var lewatKode = await service.GetInstitutionOptionsAsync(
            new ReferralInstitutionOptionQuery { Search = "INS-02" });

        Assert.Equal("Klinik Sehat Sentosa", Assert.Single(lewatNama.Items).InstitutionName);
        Assert.Equal("Puskesmas Melati", Assert.Single(lewatKode.Items).InstitutionName);
    }

    [Fact]
    public async Task DaftarInstansi_TidakMemuatBarisTerhapus()
    {
        using var context = CreateInMemoryContext();

        var terhapus = await SeedInstitutionAsync(context, "INS-09", "Klinik Terhapus");
        terhapus.IsDelete = true;
        await context.SaveChangesAsync();

        var service = new ReferralMasterDataService(context);

        var hasil = await service.GetInstitutionOptionsAsync(new ReferralInstitutionOptionQuery());

        Assert.Empty(hasil.Items);
    }

    // =====================================================================
    // 3. Dokter perujuk
    // =====================================================================

    /// <summary>
    /// Pendaftaran rujukan menolak dokter yang tidak berpraktik pada instansi yang dipilih.
    /// Daftar yang tidak tersaring hanya akan menawarkan pilihan yang pasti ditolak — karena
    /// itu penyaring instansi harus benar-benar bekerja.
    /// </summary>
    [Fact]
    public async Task DaftarDokter_DisaringMenurutInstansinya()
    {
        using var context = CreateInMemoryContext();

        var sentosa = await SeedInstitutionAsync(context, "INS-01", "Klinik Sehat Sentosa");
        var melati = await SeedInstitutionAsync(context, "INS-02", "Puskesmas Melati");

        await SeedDoctorAsync(context, sentosa.Id, "dr. Andi Wijaya");
        await SeedDoctorAsync(context, melati.Id, "dr. Budi Hartono");

        var service = new ReferralMasterDataService(context);

        var hasil = await service.GetDoctorOptionsAsync(
            new ReferralDoctorOptionQuery { ReferralInstitutionId = sentosa.Id });

        var dokter = Assert.Single(hasil.Items);

        Assert.Equal("dr. Andi Wijaya", dokter.DoctorName);
        Assert.Equal(sentosa.Id, dokter.ReferralInstitutionId);
    }

    [Fact]
    public async Task DaftarDokter_MembawaNamaInstansinya()
    {
        using var context = CreateInMemoryContext();

        var sentosa = await SeedInstitutionAsync(context, "INS-01", "Klinik Sehat Sentosa");
        await SeedDoctorAsync(context, sentosa.Id, "dr. Andi Wijaya");

        var service = new ReferralMasterDataService(context);

        var hasil = await service.GetDoctorOptionsAsync(new ReferralDoctorOptionQuery());

        Assert.Equal("Klinik Sehat Sentosa", Assert.Single(hasil.Items).InstitutionName);
    }

    [Fact]
    public async Task DaftarDokter_HanyaMemuatYangAktifSecaraBawaan()
    {
        using var context = CreateInMemoryContext();

        var sentosa = await SeedInstitutionAsync(context, "INS-01", "Klinik Sehat Sentosa");

        await SeedDoctorAsync(context, sentosa.Id, "dr. Andi Wijaya");
        await SeedDoctorAsync(context, sentosa.Id, "dr. Sudah Pensiun", isActive: false);

        var service = new ReferralMasterDataService(context);

        var bawaan = await service.GetDoctorOptionsAsync(new ReferralDoctorOptionQuery());
        var semua = await service.GetDoctorOptionsAsync(
            new ReferralDoctorOptionQuery { OnlyActive = false });

        Assert.Equal("dr. Andi Wijaya", Assert.Single(bawaan.Items).DoctorName);
        Assert.Equal(2, semua.TotalData);
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private static async Task<MstReferralInstitution> SeedInstitutionAsync(
        ApplicationDbContext context,
        string kode,
        string nama,
        bool isActive = true)
    {
        var institution = new MstReferralInstitution
        {
            Id = Guid.NewGuid(),
            InstitutionCode = kode,
            InstitutionName = nama,
            IsActive = isActive,
            IsDelete = false
        };

        context.Set<MstReferralInstitution>().Add(institution);
        await context.SaveChangesAsync();

        return institution;
    }

    private static async Task<MstReferralDoctor> SeedDoctorAsync(
        ApplicationDbContext context,
        Guid institutionId,
        string nama,
        bool isActive = true)
    {
        var doctor = new MstReferralDoctor
        {
            Id = Guid.NewGuid(),
            ReferralInstitutionId = institutionId,
            DoctorName = nama,
            IsActive = isActive,
            IsDelete = false
        };

        context.Set<MstReferralDoctor>().Add(doctor);
        await context.SaveChangesAsync();

        return doctor;
    }

    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"referral-options-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }
}
