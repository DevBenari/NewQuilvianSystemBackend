using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Nutrition.Infrastructure;

namespace QuilvianSystemBackend.Tests.Nutrition;

/// <summary>
/// Master data Gizi: jenis diet, bentuk makanan, jadwal makan, domain dan diagnosis IDNT,
/// parameter kebutuhan, serta registry rumus.
/// </summary>
/// <remarks>
/// <para>
/// Master inilah yang membatasi semua pilihan di layar. Kode yang bentrok membuat dua baris
/// berbeda tampak sama di daftar pilihan, dan diagnosis yang menggantung tanpa domain yang sah
/// membuat pengelompokan IDNT-nya tidak dapat dibaca.
/// </para>
/// <para>
/// Diuji langsung terhadap controller karena logikanya memang berada di sana — ia pembungkus
/// tipis di atas <c>ApplicationDbContext</c>, tanpa service di antaranya. Yang diperiksa kode
/// status HTTP-nya, bukan hanya apakah barisnya tersimpan.
/// </para>
/// </remarks>
public class MasterControllerTests
{
    private static NutritionMasterController Controller(ApplicationDbContext konteks) =>
        new(konteks);

    private static SaveGzMasterRequest Master(string kode, string nama, bool aktif = true) => new()
    {
        Code = kode,
        Name = nama,
        SortOrder = 50,
        IsActive = aktif
    };

    private static int Status(IActionResult hasil) => hasil switch
    {
        ObjectResult o => o.StatusCode ?? 200,
        StatusCodeResult s => s.StatusCode,
        _ => 200
    };

    // ------------------------------------------------------------- jenis diet

    [Fact]
    public async Task Jenis_diet_baru_tersimpan()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks)
            .CreateDietType(Master("UJI-DIET-BARU", "Diet Rendah Garam (Uji)"), default);

        Assert.Equal(200, Status(hasil));
        Assert.Contains("UJI-DIET-BARU", konteks.GziDietTypes.Select(x => x.DietTypeCode));
    }

    [Fact]
    public async Task Kode_jenis_diet_yang_sudah_dipakai_ditolak_409()
    {
        // Kode bentrok membuat dua baris berbeda tampak sama di daftar pilihan petugas.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks)
            .CreateDietType(Master("UJI-DIET-1", "Nama lain"), default);

        Assert.Equal(StatusCodes.Status409Conflict, Status(hasil));
    }

    [Fact]
    public async Task Daftar_jenis_diet_bawaan_hanya_yang_aktif()
    {
        // `onlyActive` bernilai true secara bawaan: layar pemilihan tidak boleh menawarkan baris
        // yang sudah ditarik instalasi gizi, walaupun barisnya tetap ada untuk riwayat lama.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks).GetDietTypes(cancellationToken: default);
        var isi = Isi<List<GziMasterOptionResponse>>(hasil);

        Assert.Contains(isi, x => x.Id == h.JenisDietId);
        Assert.DoesNotContain(isi, x => x.Id == h.JenisDietNonaktifId);
    }

    [Fact]
    public async Task Daftar_jenis_diet_dapat_memuat_yang_tidak_aktif()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks).GetDietTypes(onlyActive: false, default);
        var isi = Isi<List<GziMasterOptionResponse>>(hasil);

        Assert.Contains(isi, x => x.Id == h.JenisDietNonaktifId);
    }

    // ------------------------------------------------------------- bentuk makanan

    [Fact]
    public async Task Bentuk_makanan_baru_tersimpan()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks)
            .CreateFoodForm(Master("UJI-FORM-BARU", "Bubur (Uji)"), default);

        Assert.Equal(200, Status(hasil));
    }

    [Fact]
    public async Task Kode_bentuk_makanan_yang_sudah_dipakai_ditolak_409()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks)
            .CreateFoodForm(Master("UJI-FORM-1", "Nama lain"), default);

        Assert.Equal(StatusCodes.Status409Conflict, Status(hasil));
    }

    [Fact]
    public async Task Daftar_bentuk_makanan_bawaan_hanya_yang_aktif()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var isi = Isi<List<GziMasterOptionResponse>>(
            await Controller(konteks).GetFoodForms(cancellationToken: default));

        Assert.Contains(isi, x => x.Id == h.BentukMakananId);
        Assert.DoesNotContain(isi, x => x.Id == h.BentukMakananNonaktifId);
    }

    // ------------------------------------------------------------- jadwal makan

    [Fact]
    public async Task Jadwal_makan_baru_tersimpan()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Master("UJI-JADWAL-BARU", "Selingan Pagi (Uji)");
        permintaan.ServingTime = new TimeOnly(10, 0);
        permintaan.IsMainMeal = false;

        var hasil = await Controller(konteks).CreateMealSchedule(permintaan, default);

        Assert.Equal(200, Status(hasil));
    }

    [Fact]
    public async Task Kode_jadwal_makan_yang_sudah_dipakai_ditolak_409()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks)
            .CreateMealSchedule(Master("UJI-JADWAL-1", "Nama lain"), default);

        Assert.Equal(StatusCodes.Status409Conflict, Status(hasil));
    }

    [Fact]
    public async Task Daftar_jadwal_makan_bawaan_hanya_yang_aktif()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var isi = Isi<List<GziMasterOptionResponse>>(
            await Controller(konteks).GetMealSchedules(cancellationToken: default));

        Assert.Contains(isi, x => x.Id == h.JadwalMakanId);
        Assert.DoesNotContain(isi, x => x.Id == h.JadwalMakanNonaktifId);
    }

    // ------------------------------------------------------------- domain diagnosis

    [Fact]
    public async Task Tiga_domain_keputusan_GIZ_DEC_011_tersedia()
    {
        // `NI`, `NC`, `NB` ditanam konfigurasi EF karena `GIZ-DEC-011` menyebutnya langsung.
        // Uji ini menjaga ketiganya tetap terbaca lewat endpoint yang dipakai layar.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var isi = Isi<List<GziDiagnosisDomainResponse>>(
            await Controller(konteks).GetDiagnosisDomains(cancellationToken: default));

        Assert.Equal(
            new[] { "NB", "NC", "NI" },
            isi.Select(x => x.DomainCode).OrderBy(x => x, StringComparer.Ordinal));
    }

    [Fact]
    public async Task Kode_domain_yang_sudah_dipakai_ditolak_409()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks).CreateDiagnosisDomain(
            new SaveGzDiagnosisDomainRequest
            {
                DomainCode = "NI",
                DomainName = "Nutrition Intake lain"
            }, default);

        Assert.Equal(StatusCodes.Status409Conflict, Status(hasil));
    }

    // ------------------------------------------------------------- diagnosis IDNT

    [Fact]
    public async Task Diagnosis_gizi_baru_tersimpan_dengan_baseline_IDNT()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks).CreateNutritionDiagnosis(
            new SaveGzNutritionDiagnosisRequest
            {
                DiagnosisDomainId = h.DomainNiId,
                DiagnosisCode = "ni-2.1",
                DiagnosisName = "Malnutrisi (Uji)"
            }, default);

        Assert.Equal(200, Status(hasil));

        // Kode dinormalkan menjadi huruf besar oleh controller; tanpa itu "NI-2.1" dan "ni-2.1"
        // akan hidup berdampingan sebagai dua diagnosis berbeda.
        var tersimpan = konteks.GziNutritionDiagnoses.Single(x => x.DiagnosisName == "Malnutrisi (Uji)");
        Assert.Equal("NI-2.1", tersimpan.DiagnosisCode);
        Assert.Equal("IDNT", tersimpan.Standard);
    }

    [Fact]
    public async Task Kode_diagnosis_yang_sudah_dipakai_ditolak_409()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks).CreateNutritionDiagnosis(
            new SaveGzNutritionDiagnosisRequest
            {
                DiagnosisDomainId = h.DomainNiId,
                DiagnosisCode = "NI-1.1",
                DiagnosisName = "Nama lain"
            }, default);

        Assert.Equal(StatusCodes.Status409Conflict, Status(hasil));
    }

    [Fact]
    public async Task Domain_diagnosis_yang_tidak_ada_ditolak_422()
    {
        // Diagnosis tanpa domain yang sah menggantung di luar pengelompokan IDNT dan tidak
        // dapat dibaca sebagai bagian dari NI, NC, maupun NB.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks).CreateNutritionDiagnosis(
            new SaveGzNutritionDiagnosisRequest
            {
                DiagnosisDomainId = Guid.NewGuid(),
                DiagnosisCode = "NI-9.9",
                DiagnosisName = "Domain asing (Uji)"
            }, default);

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, Status(hasil));
    }

    [Fact]
    public async Task Diagnosis_induk_yang_tidak_ada_ditolak_422()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks).CreateNutritionDiagnosis(
            new SaveGzNutritionDiagnosisRequest
            {
                DiagnosisDomainId = h.DomainNiId,
                ParentDiagnosisId = Guid.NewGuid(),
                DiagnosisCode = "NI-9.8",
                DiagnosisName = "Induk asing (Uji)"
            }, default);

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, Status(hasil));
    }

    [Fact]
    public async Task Diagnosis_induk_yang_sah_diterima()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks).CreateNutritionDiagnosis(
            new SaveGzNutritionDiagnosisRequest
            {
                DiagnosisDomainId = h.DomainNiId,
                ParentDiagnosisId = h.DiagnosisTerlarangId,
                DiagnosisCode = "NI-1.3",
                DiagnosisName = "Anak diagnosis (Uji)"
            }, default);

        Assert.Equal(200, Status(hasil));
    }

    [Fact]
    public async Task Penyaring_hanya_yang_dapat_dipilih_membuang_baris_kelompok()
    {
        // Baris induk pada hierarki IDNT ada sebagai pengelompokan, bukan diagnosis yang
        // ditegakkan. `OnlySelectable` itulah yang membedakannya bagi layar pemilihan.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var pengendali = Controller(konteks);

        var semua = Halaman(await pengendali.GetNutritionDiagnoses(
            new GziDiagnosisQuery { PageSize = 50 }, default));
        var dapatDipilih = Halaman(await pengendali.GetNutritionDiagnoses(
            new GziDiagnosisQuery { PageSize = 50, OnlySelectable = true }, default));

        Assert.Contains(semua, x => x.Id == h.DiagnosisTerlarangId);
        Assert.DoesNotContain(dapatDipilih, x => x.Id == h.DiagnosisTerlarangId);
        Assert.Contains(dapatDipilih, x => x.Id == h.DiagnosisAId);
    }

    [Fact]
    public async Task Penyaring_domain_membuang_domain_lain()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var pengendali = Controller(konteks);

        var milikNi = Halaman(await pengendali.GetNutritionDiagnoses(
            new GziDiagnosisQuery { PageSize = 50, DiagnosisDomainId = h.DomainNiId }, default));
        var milikAsing = Halaman(await pengendali.GetNutritionDiagnoses(
            new GziDiagnosisQuery { PageSize = 50, DiagnosisDomainId = Guid.NewGuid() }, default));

        Assert.NotEmpty(milikNi);
        Assert.Empty(milikAsing);
    }

    [Fact]
    public async Task Pencarian_diagnosis_mengembalikan_kosong_untuk_kata_yang_tidak_ada()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = Halaman(await Controller(konteks).GetNutritionDiagnoses(
            new GziDiagnosisQuery { PageSize = 50, Search = "tidak-ada-sama-sekali" }, default));

        Assert.Empty(hasil);
    }

    // ------------------------------------------------------------- parameter

    [Fact]
    public async Task Lima_parameter_keputusan_GIZ_DEC_012_tersedia()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var isi = Isi<List<GziNutritionParameterResponse>>(
            await Controller(konteks).GetNutritionParameters(cancellationToken: default));

        Assert.Equal(
            new[] { "CARBOHYDRATE", "ENERGY", "FAT", "FLUID", "PROTEIN" },
            isi.Select(x => x.ParameterCode).OrderBy(x => x, StringComparer.Ordinal));
    }

    [Fact]
    public async Task Parameter_baru_tersimpan()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks).CreateNutritionParameter(
            new SaveGzNutritionParameterRequest
            {
                ParameterCode = "UJI-SERAT",
                ParameterName = "Serat (Uji)",
                UnitCode = "gram/hari",
                SortOrder = 90
            }, default);

        Assert.Equal(200, Status(hasil));
    }

    [Fact]
    public async Task Kode_parameter_yang_sudah_dipakai_ditolak_409()
    {
        // Parameter kembar akan membuat `GIZ015` menuntut dua nilai untuk hal yang sama.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks).CreateNutritionParameter(
            new SaveGzNutritionParameterRequest
            {
                ParameterCode = "ENERGY",
                ParameterName = "Energi lain",
                UnitCode = "kkal/hari"
            }, default);

        Assert.Equal(StatusCodes.Status409Conflict, Status(hasil));
    }

    // ------------------------------------------------------------- registry rumus

    [Fact]
    public async Task Rumus_baru_tersimpan_beserta_rujukannya()
    {
        // `GIZ-OQ-007` menunda rumusnya, bukan registrynya. Rujukan sumber disimpan apa adanya
        // dari admin — sistem tidak mengambilnya dari mana pun.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks).CreateNutritionFormula(
            new NutritionRequirementCalculator([]),
            new SaveGzNutritionFormulaRequest
            {
                FormulaCode = "UJI-RUMUS-BARU",
                FormulaName = "Rumus uji kedua",
                FormulaVersion = "1.0",
                ImplementationKey = "uji-2",
                SourceReference = "Data uji; bukan rujukan klinis"
            }, default);

        Assert.Equal(200, Status(hasil));

        var tersimpan = konteks.GziNutritionFormulas.Single(x => x.FormulaCode == "UJI-RUMUS-BARU");
        Assert.Equal("Data uji; bukan rujukan klinis", tersimpan.SourceReference);
    }

    [Fact]
    public async Task Pasangan_kode_dan_versi_rumus_yang_sudah_terdaftar_ditolak_409()
    {
        // Yang dijaga PASANGAN kode dan versi, bukan kodenya saja — rumus yang sama memang boleh
        // punya beberapa versi, dan versi barulah yang menggantikan tanpa menghapus yang lama.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks).CreateNutritionFormula(
            new NutritionRequirementCalculator([]),
            new SaveGzNutritionFormulaRequest
            {
                FormulaCode = "UJI-GZI-RUMUS",
                FormulaName = "Nama lain",
                FormulaVersion = "1.0",
                ImplementationKey = "uji-lain"
            }, default);

        Assert.Equal(StatusCodes.Status409Conflict, Status(hasil));
    }

    [Fact]
    public async Task Versi_baru_dari_rumus_yang_sama_diterima()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await Controller(konteks).CreateNutritionFormula(
            new NutritionRequirementCalculator([]),
            new SaveGzNutritionFormulaRequest
            {
                FormulaCode = "UJI-GZI-RUMUS",
                FormulaName = "Rumus uji versi dua",
                FormulaVersion = "2.0",
                ImplementationKey = "uji-v2"
            }, default);

        Assert.Equal(200, Status(hasil));
    }

    // ------------------------------------------------------------- penolong

    private static T Isi<T>(IActionResult hasil)
    {
        var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
        var pembungkus = objek.Value!;
        var data = pembungkus.GetType().GetProperty("Data")!.GetValue(pembungkus);
        return (T)data!;
    }

    private static List<GziNutritionDiagnosisResponse> Halaman(IActionResult hasil)
    {
        var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
        var pembungkus = objek.Value!;
        var data = pembungkus.GetType().GetProperty("Data")!.GetValue(pembungkus)!;
        return (List<GziNutritionDiagnosisResponse>)
            data.GetType().GetProperty("Items")!.GetValue(data)!;
    }
}
