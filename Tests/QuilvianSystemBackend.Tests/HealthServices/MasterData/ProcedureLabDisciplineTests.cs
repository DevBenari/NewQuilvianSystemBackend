using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.MasterData;

/// <summary>
/// Jalur pengisian disiplin laboratorium lewat Master Data.
///
/// <c>BE-EXT-01</c> menambahkan kolom <c>MstProcedure.LabDiscipline</c> tetapi sengaja tidak
/// mengisinya, karena penggolongannya keputusan klinis. Yang luput saat itu: kolomnya juga
/// tidak pernah dibuka pada DTO maupun controller Master Data, sehingga tidak ada satu pun
/// jalur untuk mengisinya — bukan lewat API, bukan lewat layar. Selama itu, penyaring katalog
/// per disiplin dan ketiga layar monitoring selalu kosong.
///
/// Uji di bawah menahan jalur itu tetap ada dan tetap berbatas:
///   1. golongan yang dikirim benar-benar tersimpan dan terbaca kembali;
///   2. golongan pada tindakan non-laboratorium <b>ditolak</b>, bukan diabaikan diam-diam;
///   3. mengosongkan golongan mencabutnya, sehingga penanda Laboratorium tidak pernah mati
///      sambil meninggalkan golongan yatim;
///   4. nama disiplin yang tidak dikenal ditolak;
///   5. daftar pilihannya berasal dari enum <c>LabDiscipline</c>, bukan salinan teks.
/// </summary>
public class ProcedureLabDisciplineTests
{
    private static readonly Guid Petugas = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task Membuat_PemeriksaanLaboratorium_MenyimpanDisiplinnya()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);

        var hasil = await controller.CreateProcedure(Permintaan(
            nama: "Kultur darah",
            isLaboratory: true,
            disiplin: nameof(LabDiscipline.Microbiology)));

        Assert.IsType<OkObjectResult>(hasil);

        var tersimpan = await context.Set<MstProcedure>()
            .AsNoTracking()
            .SingleAsync(x => x.ProcedureName == "Kultur darah");

        Assert.Equal(LabDiscipline.Microbiology, tersimpan.LabDiscipline);
    }

    [Fact]
    public async Task Membaca_PemeriksaanLaboratorium_MembawaNamaDanLabelDisiplinnya()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);

        var id = await SeedAsync(context, "Hemoglobin", LabDiscipline.ClinicalPathology);

        var hasil = Assert.IsType<OkObjectResult>(await controller.GetProcedureById(id));
        var response = Assert.IsType<ApiResponse<ProcedureDetailResponse>>(hasil.Value);

        Assert.Equal(nameof(LabDiscipline.ClinicalPathology), response.Data!.LabDiscipline);

        // Labelnya wajib sama dengan yang dipakai Laboratorium pada layar-layarnya; satu
        // golongan yang terbaca dengan dua nama membuat petugas mengira keduanya berbeda.
        Assert.Equal("Patologi Klinik", response.Data.LabDisciplineName);
    }

    [Fact]
    public async Task Membuat_TindakanNonLaboratorium_DenganDisiplin_Ditolak()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);

        var hasil = await controller.CreateProcedure(Permintaan(
            nama: "Foto toraks",
            isLaboratory: false,
            disiplin: nameof(LabDiscipline.ClinicalPathology)));

        Assert.IsType<BadRequestObjectResult>(hasil);

        // Ditolak berarti tidak ada baris setengah jadi yang tertinggal.
        Assert.False(await context.Set<MstProcedure>().AnyAsync());
    }

    [Fact]
    public async Task Membuat_DenganDisiplinTidakDikenal_Ditolak()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);

        var hasil = await controller.CreateProcedure(Permintaan(
            nama: "Pemeriksaan entah",
            isLaboratory: true,
            disiplin: "Hematologi"));

        Assert.IsType<BadRequestObjectResult>(hasil);
        Assert.False(await context.Set<MstProcedure>().AnyAsync());
    }

    [Fact]
    public async Task Mengubah_TanpaDisiplin_MencabutGolongannya()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);

        var id = await SeedAsync(context, "Hemoglobin", LabDiscipline.ClinicalPathology);

        var permintaan = new UpdateProcedureRequest
        {
            ProcedureName = "Hemoglobin",
            ProcedureType = "Laboratory",
            IsLaboratory = true,
            LabDiscipline = null,
            IsActive = true
        };

        Assert.IsType<OkObjectResult>(await controller.UpdateProcedure(id, permintaan));

        context.ChangeTracker.Clear();

        var tersimpan = await context.Set<MstProcedure>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == id);

        Assert.Null(tersimpan.LabDiscipline);
    }

    [Fact]
    public async Task Mengubah_MematikanPenandaLaboratorium_TidakMenyisakanGolonganYatim()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);

        var id = await SeedAsync(context, "Hemoglobin", LabDiscipline.ClinicalPathology);

        // Golongan lama ikut dikirim; yang berubah hanya penanda Laboratorium. Permintaan
        // seperti inilah yang dikirim layar ketika petugas mematikan penandanya tanpa
        // mengosongkan pilihan disiplinnya lebih dulu.
        var permintaan = new UpdateProcedureRequest
        {
            ProcedureName = "Hemoglobin",
            ProcedureType = "General",
            IsLaboratory = false,
            LabDiscipline = nameof(LabDiscipline.ClinicalPathology),
            IsActive = true
        };

        Assert.IsType<BadRequestObjectResult>(await controller.UpdateProcedure(id, permintaan));

        context.ChangeTracker.Clear();

        // Ditolak, jadi barisnya utuh seperti semula — bukan setengah berubah.
        var tersimpan = await context.Set<MstProcedure>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == id);

        Assert.True(tersimpan.IsLaboratory);
        Assert.Equal(LabDiscipline.ClinicalPathology, tersimpan.LabDiscipline);
    }

    [Fact]
    public async Task Metadata_MembawaSeluruhDisiplinDariEnumnya()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);

        var hasil = Assert.IsType<OkObjectResult>(await controller.GetFilterMetadata());
        var response = Assert.IsType<ApiResponse<ProcedureFilterMetadataResponse>>(hasil.Value);

        var pilihan = response.Data!.LabDisciplineOptions;

        // Jumlahnya menelusuri enum, bukan angka yang ditulis tangan: menambah disiplin
        // keempat tanpa menerbitkannya ke layar akan membuat uji ini gagal.
        Assert.Equal(Enum.GetNames<LabDiscipline>().Length, pilihan.Count);

        Assert.Contains(pilihan, x =>
            x.Value == nameof(LabDiscipline.AnatomicalPathology) && x.Label == "Patologi Anatomi");
    }

    [Fact]
    public async Task Metadata_FormMemuatRuasDisiplinSetelahPenandaLaboratorium()
    {
        await using var context = CreateContext();
        var controller = CreateController(context);

        var hasil = Assert.IsType<OkObjectResult>(await controller.GetFilterMetadata());
        var response = Assert.IsType<ApiResponse<ProcedureFilterMetadataResponse>>(hasil.Value);

        var ruas = response.Data!.CreateFields;

        var penandaLab = ruas.Single(x => x.Name == "isLaboratory");
        var disiplin = ruas.Single(x => x.Name == "labDiscipline");

        Assert.Equal("select", disiplin.InputType);
        Assert.Equal("labDisciplineOptions", disiplin.OptionsSource);

        // Urutannya bermakna: disiplin hanya masuk akal dibaca tepat sesudah penandanya.
        Assert.True(disiplin.SortOrder > penandaLab.SortOrder);

        // Menyisipkan ruas baru tanpa menggeser nomor urut yang lain akan membuat dua ruas
        // berbagi nomor, dan urutan formnya menjadi bergantung pada kebetulan.
        Assert.DoesNotContain(ruas, x =>
            x.Name != "labDiscipline" && x.SortOrder == disiplin.SortOrder);
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private static CreateProcedureRequest Permintaan(
        string nama,
        bool isLaboratory,
        string? disiplin) =>
        new()
        {
            ProcedureName = nama,
            ProcedureType = isLaboratory ? "Laboratory" : "Radiology",
            IsLaboratory = isLaboratory,
            IsRadiology = !isLaboratory,
            LabDiscipline = disiplin
        };

    private static async Task<Guid> SeedAsync(
        ApplicationDbContext context,
        string nama,
        LabDiscipline disiplin)
    {
        var entity = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = "PR-RSMMC-00001",
            ProcedureName = nama,
            ProcedureType = "Laboratory",
            IsLaboratory = true,
            LabDiscipline = disiplin,
            IsActive = true
        };

        context.Set<MstProcedure>().Add(entity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return entity.Id;
    }

    private static ProcedureController CreateController(ApplicationDbContext context)
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, Petugas.ToString()) },
            authenticationType: "ProcedureLabDisciplineTest");

        var httpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        var accessor = new HttpContextAccessor { HttpContext = httpContext };

        return new ProcedureController(
            context,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor))
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext }
        };
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"procedure-lab-discipline-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }
}
