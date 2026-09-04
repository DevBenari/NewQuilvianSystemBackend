using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.LaboratoryManagement;

/// <summary>
/// Bukti untuk <c>BE-LAB-07</c> — katalog, harga, dan cakupan penjamin
/// (<c>FR-09.1</c> .. <c>FR-09.5</c>; <c>LAB-DEC-033</c>, <c>LAB-DEC-036</c>).
///
/// Yang dibuktikan di sini:
///   1. <c>AC-43</c> — memilih tiga pemeriksaan menampilkan harga satuan yang benar, subtotal,
///      dan total, <b>tanpa</b> satu baris tagihan pun terbentuk;
///   2. <c>AC-47</c> — Laboratorium tidak memiliki tabel tarif; harga selalu berasal dari
///      <c>MstTariff</c>;
///   3. <c>AC-48</c> — grup katalog tidak punya satu pun jalur ubah, sehingga tarif tidak dapat
///      diubah dari modul Laboratorium (<c>VAL-50</c>);
///   4. <c>AC-51</c> / <c>VAL-46</c> — menambahkan Hemoglobin ke pesanan Mikrobiologi ditolak;
///   5. penyaringan katalog per disiplin bekerja, dan pemeriksaan yang belum digolongkan tidak
///      ikut hilang.
/// </summary>
public class LabCatalogTests
{
    private static readonly Guid Petugas = Guid.Parse("77777777-7777-7777-7777-777777777777");
    private static readonly Guid Penjamin = Guid.Parse("99999999-9999-9999-9999-999999999999");

    // =====================================================================
    // 1. Bentuk kontrak — baca saja
    // =====================================================================

    [Theory]
    [InlineData(nameof(LabCatalogController.GetExaminations), "examinations")]
    [InlineData(nameof(LabCatalogController.GetPrice), "examinations/{procedureId:guid}/price")]
    [InlineData(nameof(LabCatalogController.GetTariffs), "tariffs")]
    public void KetigaEndpoint_MemakaiGetDanPermissionYangDikunciKontrak(string methodName, string template)
    {
        var method = typeof(LabCatalogController).GetMethod(methodName);

        Assert.NotNull(method);

        var permission = method!.GetCustomAttribute<AccessPermissionAttribute>();

        Assert.NotNull(permission);

        var arguments = Assert.IsType<object[]>(permission!.Arguments);

        Assert.Equal("LabCatalog", arguments[0]);
        Assert.Equal("Read", arguments[1]);

        var verb = Assert.IsType<HttpGetAttribute>(
            method.GetCustomAttributes().Single(x => x is HttpMethodAttribute));

        Assert.Equal(template, verb.Template);
    }

    /// <summary>
    /// <c>AC-48</c> dan <c>VAL-50</c>. Tarif tidak dapat diubah lewat modul Laboratorium bukan
    /// karena ada penjaga yang menolaknya, melainkan karena <b>jalurnya tidak pernah dibuat</b>.
    /// Uji ini menjaga ketiadaan itu.
    /// </summary>
    [Fact]
    public void GrupKatalog_TidakMemilikiSatuPunJalurUbah()
    {
        var verbs = typeof(LabCatalogController)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .SelectMany(x => x.GetCustomAttributes<HttpMethodAttribute>())
            .ToList();

        Assert.Equal(3, verbs.Count);
        Assert.All(verbs, x => Assert.IsType<HttpGetAttribute>(x));

        Assert.DoesNotContain(verbs, x => x is HttpPostAttribute);
        Assert.DoesNotContain(verbs, x => x is HttpPutAttribute);
        Assert.DoesNotContain(verbs, x => x is HttpDeleteAttribute);
        Assert.DoesNotContain(verbs, x => x is HttpPatchAttribute);
    }

    /// <summary>
    /// <c>AC-47</c>. Harga selalu berasal dari data induk milik Master Data. Laboratorium tidak
    /// boleh punya tabel tarif sendiri — sekali ada, harga akan hidup di dua tempat dan
    /// keduanya dapat berbeda tanpa ada yang menyadarinya.
    /// </summary>
    [Fact]
    public void Laboratorium_TidakMemilikiSatuPunTabelTarif()
    {
        using var context = CreateInMemoryContext();

        var entityLaboratorium = context.Model
            .GetEntityTypes()
            .Where(x => x.ClrType.Namespace != null &&
                        x.ClrType.Namespace.StartsWith(
                            "QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement",
                            StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(entityLaboratorium);

        // Nama tipe tidak boleh memuat istilah tarif atau harga.
        Assert.DoesNotContain(entityLaboratorium, x =>
            x.ClrType.Name.Contains("Tariff", StringComparison.OrdinalIgnoreCase) ||
            x.ClrType.Name.Contains("Price", StringComparison.OrdinalIgnoreCase));

        // Dua bentuk yang tetap boleh ada, dan keduanya bukan tabel tarif:
        //   - penunjuk `TariffId` ke tarif milik Master Data;
        //   - salinan `*Snapshot` sebagai bukti nilai saat kejadian.
        // Yang dilarang adalah entity tarif tersendiri milik Laboratorium.
        var kolomTarif = entityLaboratorium
            .SelectMany(x => x.GetProperties().Select(p => new { Entity = x.ClrType.Name, p.Name }))
            .Where(x => x.Name.Contains("Tariff", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.NotEmpty(kolomTarif);

        Assert.All(kolomTarif, x => Assert.True(
            x.Name == "TariffId" || x.Name.EndsWith("Snapshot", StringComparison.Ordinal),
            $"{x.Entity}.{x.Name} bukan penunjuk maupun salinan tarif."));
    }

    // =====================================================================
    // 2. AC-43 — harga satuan, subtotal, total, tanpa tagihan
    // =====================================================================

    [Fact]
    public async Task AC43_MemilihTigaPemeriksaan_MenampilkanHargaSatuanDanTotalTanpaMembentukTagihan()
    {
        await using var context = CreateInMemoryContext();
        var service = new LabCatalogService(context);

        var hemoglobin = await SeedProcedureAsync(context, "LAB-HB", "Hemoglobin", LabDiscipline.ClinicalPathology, 35_000m);
        var leukosit = await SeedProcedureAsync(context, "LAB-WBC", "Leukosit", LabDiscipline.ClinicalPathology, 30_000m);
        var natrium = await SeedProcedureAsync(context, "LAB-NA", "Natrium", LabDiscipline.ClinicalPathology, 40_000m);

        var harga = new List<decimal>();

        foreach (var procedureId in new[] { hemoglobin, leukosit, natrium })
        {
            var hasil = await service.GetPriceAsync(procedureId, new LabPriceQuery());

            Assert.NotNull(hasil.HospitalPrice);
            harga.Add(hasil.HospitalPrice!.Value);
        }

        Assert.Equal(new[] { 35_000m, 30_000m, 40_000m }, harga);
        Assert.Equal(105_000m, harga.Sum());

        // Batas yang dijaga AC-43: melihat harga bukan memesan, dan memesan bukan menagih.
        Assert.Equal(0, await context.BilChargeLines.CountAsync());
        Assert.Equal(0, await context.CliClinicalMilestoneFacts.CountAsync());
        Assert.Equal(0, await context.LabExaminations.CountAsync());
    }

    [Fact]
    public async Task TarifBelumDiatur_HargaKosongDanDisertaiKeterangan()
    {
        await using var context = CreateInMemoryContext();
        var service = new LabCatalogService(context);

        var tanpaTarif = await SeedProcedureAsync(
            context, "LAB-NOP", "Pemeriksaan Tanpa Tarif", LabDiscipline.ClinicalPathology, harga: null);

        var hasil = await service.GetPriceAsync(tanpaTarif, new LabPriceQuery());

        Assert.Null(hasil.HospitalPrice);
        Assert.Contains("belum diatur", hasil.Note);
    }

    [Fact]
    public async Task PenjaminBerkontrak_MenampilkanHargaKontraknya()
    {
        await using var context = CreateInMemoryContext();
        var service = new LabCatalogService(context);

        var hemoglobin = await SeedProcedureAsync(
            context, "LAB-HB", "Hemoglobin", LabDiscipline.ClinicalPathology, 35_000m);

        await SeedKontrakAsync(context, hemoglobin, "INS-HB", 28_000m);

        var hasil = await service.GetPriceAsync(
            hemoglobin,
            new LabPriceQuery { InsuranceProviderId = Penjamin });

        Assert.Equal(35_000m, hasil.HospitalPrice);
        Assert.Equal(28_000m, hasil.ContractPrice);
        Assert.Equal("INS-HB", hasil.InsuranceTariffCode);
        Assert.False(hasil.IsNotCovered);
    }

    [Fact]
    public async Task PenjaminTanpaKontrak_DitandaiTidakTercakupDanBukanBerartiGratis()
    {
        await using var context = CreateInMemoryContext();
        var service = new LabCatalogService(context);

        var hemoglobin = await SeedProcedureAsync(
            context, "LAB-HB", "Hemoglobin", LabDiscipline.ClinicalPathology, 35_000m);

        var hasil = await service.GetPriceAsync(
            hemoglobin,
            new LabPriceQuery { InsuranceProviderId = Penjamin });

        Assert.Equal(35_000m, hasil.HospitalPrice);
        Assert.Null(hasil.ContractPrice);
        Assert.True(hasil.IsNotCovered);
        Assert.Contains("tidak memiliki harga kontrak", hasil.Note);
    }

    // =====================================================================
    // 3. Penyaringan katalog per disiplin
    // =====================================================================

    [Fact]
    public async Task KatalogTersaringPerDisiplin_HanyaMenampilkanDisiplinYangDiminta()
    {
        await using var context = CreateInMemoryContext();
        var service = new LabCatalogService(context);

        await SeedProcedureAsync(context, "LAB-HB", "Hemoglobin", LabDiscipline.ClinicalPathology, 35_000m);
        await SeedProcedureAsync(context, "LAB-KUL", "Kultur darah", LabDiscipline.Microbiology, 120_000m);
        await SeedProcedureAsync(context, "LAB-PA", "Biopsi", LabDiscipline.AnatomicalPathology, 300_000m);

        var mikro = await service.GetExaminationsAsync(
            new LabCatalogQuery { Discipline = "Microbiology" });

        var baris = Assert.Single(mikro.Items);

        Assert.Equal("Kultur darah", baris.ProcedureName);
        Assert.Equal(nameof(LabDiscipline.Microbiology), baris.Discipline);
        Assert.Equal(120_000m, baris.UnitPrice);
    }

    /// <summary>
    /// Katalog yang belum digolongkan tetap tampil ketika penyaring disiplin tidak dikirim.
    /// Menyembunyikannya membuat katalog tampak kosong pada rumah sakit yang penggolongannya
    /// belum diisi, dan petugas menyimpulkan sistemnya rusak.
    /// </summary>
    [Fact]
    public async Task PemeriksaanBelumDigolongkan_TetapTampilTanpaPenyaringDisiplin()
    {
        await using var context = CreateInMemoryContext();
        var service = new LabCatalogService(context);

        await SeedProcedureAsync(context, "LAB-HB", "Hemoglobin", LabDiscipline.ClinicalPathology, 35_000m);
        await SeedProcedureAsync(context, "LAB-XX", "Belum Digolongkan", discipline: null, harga: 10_000m);

        var seluruhnya = await service.GetExaminationsAsync(new LabCatalogQuery());

        Assert.Equal(2, seluruhnya.TotalData);
        Assert.Contains(seluruhnya.Items, x => x.Discipline == null);

        // Tetapi ia tidak ikut muncul pada penyaring disiplin mana pun.
        var klinik = await service.GetExaminationsAsync(
            new LabCatalogQuery { Discipline = "ClinicalPathology" });

        Assert.Equal("Hemoglobin", Assert.Single(klinik.Items).ProcedureName);
    }

    [Fact]
    public async Task DaftarTarif_HanyaMemuatTarifPemeriksaanLaboratorium()
    {
        await using var context = CreateInMemoryContext();
        var service = new LabCatalogService(context);

        await SeedProcedureAsync(context, "LAB-HB", "Hemoglobin", LabDiscipline.ClinicalPathology, 35_000m);
        await SeedProcedureAsync(context, "RAD-TX", "Foto toraks", discipline: null, harga: 150_000m, isLab: false);

        var hasil = await service.GetTariffsAsync(new LabTariffQuery());

        var baris = Assert.Single(hasil.Items);

        Assert.Equal("Hemoglobin", baris.ProcedureName);
        Assert.Equal(35_000m, baris.NormalPrice);
        Assert.Equal(nameof(LabDiscipline.ClinicalPathology), baris.Discipline);
    }

    // =====================================================================
    // 4. AC-51 / VAL-46 — INV-22
    // =====================================================================

    [Fact]
    public async Task VAL46_MenambahkanHemoglobinKePesananMikrobiologi_Ditolak()
    {
        await using var context = CreateInMemoryContext();

        var hemoglobin = await SeedProcedureAsync(
            context, "LAB-HB", "Hemoglobin", LabDiscipline.ClinicalPathology, 35_000m);

        var (orderId, specimenId) = await SeedPesananAsync(context, LabDiscipline.Microbiology);

        var galat = await Assert.ThrowsAsync<LabExaminationValidationException>(
            () => CreateExaminationService(context).AddAsync(
                orderId,
                new AddLabExaminationRequest { SpecimenId = specimenId, ProcedureId = hemoglobin }));

        Assert.Contains("bukan bagian dari", galat.Message);
        Assert.Contains(nameof(LabDiscipline.Microbiology), galat.Message);

        Assert.Equal(0, await context.LabExaminations.CountAsync());
    }

    [Fact]
    public async Task DisiplinSesuai_PemeriksaanDapatDitambahkan()
    {
        await using var context = CreateInMemoryContext();

        var hemoglobin = await SeedProcedureAsync(
            context, "LAB-HB", "Hemoglobin", LabDiscipline.ClinicalPathology, 35_000m);

        var (orderId, specimenId) = await SeedPesananAsync(context, LabDiscipline.ClinicalPathology);

        var hasil = await CreateExaminationService(context).AddAsync(
            orderId,
            new AddLabExaminationRequest { SpecimenId = specimenId, ProcedureId = hemoglobin });

        Assert.Equal("Hemoglobin", hasil.ProcedureName);
        Assert.Equal(35_000m, hasil.UnitPrice);
    }

    /// <summary>
    /// Penegakan <c>INV-22</c> menuntut <b>kedua</b> disiplin diketahui. Pesanan peninggalan dan
    /// katalog yang belum digolongkan bernilai kosong; menolaknya akan mematikan pemesanan pada
    /// rumah sakit yang data induknya belum lengkap.
    /// </summary>
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public async Task SalahSatuDisiplinBelumDiketahui_TidakDitolak(bool pesananBerdisiplin, bool katalogBerdisiplin)
    {
        await using var context = CreateInMemoryContext();

        var procedureId = await SeedProcedureAsync(
            context, "LAB-HB", "Hemoglobin",
            katalogBerdisiplin ? LabDiscipline.ClinicalPathology : null,
            35_000m);

        var (orderId, specimenId) = await SeedPesananAsync(
            context, pesananBerdisiplin ? LabDiscipline.Microbiology : null);

        var hasil = await CreateExaminationService(context).AddAsync(
            orderId,
            new AddLabExaminationRequest { SpecimenId = specimenId, ProcedureId = procedureId });

        Assert.Equal("Hemoglobin", hasil.ProcedureName);
    }

    // =====================================================================
    // Pembantu
    // =====================================================================

    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"lab-catalog-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }

    private static LabExaminationService CreateExaminationService(ApplicationDbContext context)
    {
        var identity = new ClaimsIdentity(
            new[] { new Claim(ClaimTypes.NameIdentifier, Petugas.ToString()) },
            authenticationType: "LabCatalogTest");

        var accessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };

        return new LabExaminationService(
            context,
            accessor,
            new LoggerService(NullLogger<LoggerService>.Instance, accessor));
    }

    private static async Task<Guid> SeedProcedureAsync(
        ApplicationDbContext context,
        string kode,
        string nama,
        LabDiscipline? discipline,
        decimal? harga,
        bool isLab = true)
    {
        var suffix = Guid.NewGuid().ToString("N")[..6];

        var procedure = new MstProcedure
        {
            Id = Guid.NewGuid(),
            ProcedureCode = $"{kode}-{suffix}",
            ProcedureName = nama,
            ProcedureType = isLab ? "Laboratory" : "Radiology",
            IsLaboratory = isLab,
            IsRadiology = !isLab,
            LabDiscipline = discipline,
            IsActive = true,
            IsCoveredByInsuranceDefault = true
        };

        context.Set<MstProcedure>().Add(procedure);

        if (harga.HasValue)
        {
            context.Set<MstTariff>().Add(new MstTariff
            {
                Id = Guid.NewGuid(),
                ProcedureId = procedure.Id,
                TariffCode = $"TRF-{suffix}",
                TariffName = $"Tarif {nama}",
                NormalPrice = harga.Value,
                IsActive = true
            });
        }

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return procedure.Id;
    }

    private static async Task SeedKontrakAsync(
        ApplicationDbContext context, Guid procedureId, string kode, decimal hargaKontrak)
    {
        var tariffId = await context.Set<MstTariff>()
            .AsNoTracking()
            .Where(x => x.ProcedureId == procedureId)
            .Select(x => x.Id)
            .FirstAsync();

        context.Set<MstInsuranceTariff>().Add(new MstInsuranceTariff
        {
            Id = Guid.NewGuid(),
            InsuranceProviderId = Penjamin,
            TariffId = tariffId,
            InsuranceTariffCode = kode,
            InsuranceTariffName = kode,
            ContractPrice = hargaKontrak,
            IsActive = true
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
    }

    private static async Task<(Guid OrderId, Guid SpecimenId)> SeedPesananAsync(
        ApplicationDbContext context, LabDiscipline? discipline)
    {
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var order = new LabOrder
        {
            Id = Guid.NewGuid(),
            EncounterId = Guid.NewGuid(),
            ProcedureId = Guid.NewGuid(),
            Discipline = discipline,
            OrderStatus = LabOrderStatus.Requested,
            RequestedAt = DateTime.UtcNow
        };

        var specimen = new LabSpecimen
        {
            Id = Guid.NewGuid(),
            LabOrderId = order.Id,
            SpecimenBarcode = $"LSP-{suffix}",
            SpecimenSequence = 1,
            SpecimenStatus = LabSpecimenStatus.Received
        };

        context.LabOrders.Add(order);
        context.LabSpecimens.Add(specimen);

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return (order.Id, specimen.Id);
    }
}
