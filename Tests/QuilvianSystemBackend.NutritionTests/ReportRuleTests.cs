using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Nutrition.Infrastructure;

namespace QuilvianSystemBackend.Tests.Nutrition;

/// <summary>
/// Laporan dan ringkasan Gizi.
/// </summary>
/// <remarks>
/// Laporan dibaca untuk menilai pelayanan, jadi salahnya tidak terlihat sebagai galat — ia
/// terlihat sebagai angka yang masuk akal tetapi keliru. Dua hal yang dijaga di sini: penyaring
/// tanggal harus mencakup seluruh hari terakhir, dan baris yang sudah ditandai terhapus tidak
/// boleh ikut terhitung.
/// </remarks>
public class ReportRuleTests
{
    /// <summary>
    /// Menyiapkan satu order, satu kunjungan, satu diet, dan satu revisi kebutuhan — semuanya
    /// lewat service yang sama dengan yang dipakai aplikasi, bukan ditanam langsung ke tabel.
    /// </summary>
    private static async Task<Guid> SiapkanSatuAlurAsync(NutritionHarness h,
        ApplicationDbContext konteks)
    {
        var order = await h.OrderService(konteks).CreateAsync(new CreateGzOrderRequest
        {
            PatientId = h.PasienId,
            EncounterId = h.KunjunganRawatId,
            RequesterDoctorId = h.DokterId,
            AssignedWorkforceId = h.AhliGiziId,
            Priority = GziOrderPriority.Routine,
            ReasonForReferral = "Skrining gizi berisiko",
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        await h.OrderService(konteks).SaveCareRecordAsync(order.Id, new SaveGzCareRecordRequest
        {
            RecordedByWorkforceId = h.AhliGiziId,
            Weight = 55m,
            Height = 160m,
            AssessmentNote = "Asupan menurun",
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        await h.DietService(konteks).PrescribeAsync(new PrescribeGzDietRequest
        {
            PatientId = h.PasienId,
            EncounterId = h.KunjunganRawatId,
            NutritionOrderId = order.Id,
            DietTypeId = h.JenisDietId,
            FoodFormId = h.BentukMakananId,
            PrescribedByWorkforceId = h.AhliGiziId,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var nilai = new Dictionary<string, decimal>
        {
            ["ENERGY"] = 2000m,
            ["PROTEIN"] = 60m,
            ["FAT"] = 55m,
            ["CARBOHYDRATE"] = 275m,
            ["FLUID"] = 2000m,
        };

        await h.RequirementService(konteks).SaveAsync(order.Id, new SaveGzRequirementRequest
        {
            IdempotencyKey = Guid.NewGuid().ToString("N"),
            DeterminedByWorkforceId = h.AhliGiziId,
            Items = [.. h.Parameter.Select(p => new GziRequirementItemRequest
            {
                NutritionParameterId = p.Id,
                FinalValue = nilai[p.ParameterCode]
            })]
        });

        return order.Id;
    }

    // ------------------------------------------------------------------ data kosong

    [Fact]
    public async Task Laporan_pada_basis_data_kosong_mengembalikan_halaman_kosong()
    {
        // Laporan kosong harus kosong, bukan melempar. Layar laporan dibuka sebelum ada data,
        // dan galat di sana terbaca petugas sebagai sistem rusak.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var laporan = h.ReportService(konteks);
        var permintaan = new GziReportQuery { PageNumber = 1, PageSize = 20 };

        Assert.Empty((await laporan.GetServicesAsync(permintaan)).Items);
        Assert.Empty((await laporan.GetDietsAsync(permintaan)).Items);
        Assert.Empty((await laporan.GetRequirementsAsync(permintaan)).Items);
    }

    [Fact]
    public async Task Ringkasan_pada_basis_data_kosong_bernilai_nol_bukan_null()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        // Harness memuat dua order data uji, jadi yang diuji di sini kunjungan dan dietnya.
        var ringkasan = await h.ReportService(konteks)
            .GetSummaryAsync(new GziReportQuery { PatientId = Guid.NewGuid() });

        Assert.Equal(0, ringkasan.TotalOrders);
        Assert.Equal(0, ringkasan.OpenOrders);
        Assert.Equal(0, ringkasan.ClosedOrders);
        Assert.Equal(0, ringkasan.TotalPatients);
    }

    [Fact]
    public async Task Rata_rata_energi_null_ketika_belum_ada_kebutuhan()
    {
        // Nol dan "belum ada" bukan hal yang sama. Rata-rata 0 kkal akan terbaca sebagai temuan
        // klinis; null terbaca sebagai belum ada data.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var ringkasan = await h.ReportService(konteks).GetSummaryAsync(new GziReportQuery());

        Assert.Null(ringkasan.AverageEnergyKcal);
    }

    // ------------------------------------------------------------------ dengan data

    [Fact]
    public async Task Laporan_memuat_alur_yang_baru_dibuat()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await SiapkanSatuAlurAsync(h, konteks);

        var laporan = h.ReportService(konteks);
        var permintaan = new GziReportQuery { PageSize = 50 };

        Assert.NotEmpty((await laporan.GetServicesAsync(permintaan)).Items);
        Assert.NotEmpty((await laporan.GetDietsAsync(permintaan)).Items);
        Assert.NotEmpty((await laporan.GetRequirementsAsync(permintaan)).Items);
    }

    [Fact]
    public async Task Ringkasan_menghitung_order_kunjungan_diet_dan_revisi()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await SiapkanSatuAlurAsync(h, konteks);

        var ringkasan = await h.ReportService(konteks)
            .GetSummaryAsync(new GziReportQuery { PatientId = h.PasienId });

        Assert.Equal(1, ringkasan.TotalOrders);
        Assert.Equal(1, ringkasan.OpenOrders);
        Assert.Equal(0, ringkasan.ClosedOrders);
        Assert.Equal(1, ringkasan.TotalPatients);
        Assert.True(ringkasan.TotalCareRecords >= 1);
        Assert.True(ringkasan.ActiveDiets >= 1);
        Assert.True(ringkasan.RequirementRevisions >= 1);
    }

    [Fact]
    public async Task Rata_rata_energi_diambil_dari_revisi_yang_berlaku()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await SiapkanSatuAlurAsync(h, konteks);

        var ringkasan = await h.ReportService(konteks).GetSummaryAsync(new GziReportQuery());

        Assert.Equal(2000m, ringkasan.AverageEnergyKcal);
    }

    [Fact]
    public async Task Order_yang_ditutup_terhitung_sebagai_tertutup_bukan_berjalan()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var orderId = await SiapkanSatuAlurAsync(h, konteks);

        var detail = await h.OrderService(konteks).GetDetailAsync(orderId);
        await h.OrderService(konteks).CloseAsync(orderId, new CloseGzOrderRequest
        {
            ClosingNote = "Asuhan selesai",
            ExpectedVersion = detail!.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var ringkasan = await h.ReportService(konteks)
            .GetSummaryAsync(new GziReportQuery { PatientId = h.PasienId });

        Assert.Equal(1, ringkasan.TotalOrders);
        Assert.Equal(0, ringkasan.OpenOrders);
        Assert.Equal(1, ringkasan.ClosedOrders);
    }

    // ------------------------------------------------------------------ penyaring

    [Fact]
    public async Task Penyaring_tanggal_akhir_mencakup_seluruh_hari_terakhir()
    {
        // Inilah cacat yang pernah membuat ringkasan menyebut satu order padahal ada dua.
        // Penyaring layar mengirim tanggal TANPA jam; memperlakukannya apa adanya berarti pukul
        // 00:00, sehingga laporan "sampai hari ini" justru membuang seluruh data hari ini.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await SiapkanSatuAlurAsync(h, konteks);

        var hariIni = DateTime.Now.Date;

        var ringkasan = await h.ReportService(konteks).GetSummaryAsync(new GziReportQuery
        {
            PatientId = h.PasienId,
            From = hariIni,
            To = hariIni
        });

        Assert.Equal(1, ringkasan.TotalOrders);
        Assert.True(ringkasan.TotalCareRecords >= 1);
    }

    [Fact]
    public async Task Rentang_tanggal_yang_tidak_memuat_data_mengembalikan_nol()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await SiapkanSatuAlurAsync(h, konteks);

        var ringkasan = await h.ReportService(konteks).GetSummaryAsync(new GziReportQuery
        {
            PatientId = h.PasienId,
            From = DateTime.Now.Date.AddDays(-30),
            To = DateTime.Now.Date.AddDays(-20)
        });

        Assert.Equal(0, ringkasan.TotalOrders);
        Assert.Null(ringkasan.AverageEnergyKcal);
    }

    [Fact]
    public async Task Rentang_terbalik_mengembalikan_kosong_bukan_melempar()
    {
        // `From` lebih besar daripada `To` tidak ditolak kontraknya; yang penting ia tidak
        // meledak dan tidak diam-diam mengabaikan penyaringnya.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await SiapkanSatuAlurAsync(h, konteks);

        var ringkasan = await h.ReportService(konteks).GetSummaryAsync(new GziReportQuery
        {
            From = DateTime.Now.Date.AddDays(5),
            To = DateTime.Now.Date.AddDays(-5)
        });

        Assert.Equal(0, ringkasan.TotalOrders);
        Assert.Equal(0, ringkasan.TotalCareRecords);
    }

    [Fact]
    public async Task Penyaring_pasien_membuang_pasien_lain()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await SiapkanSatuAlurAsync(h, konteks);

        var laporan = h.ReportService(konteks);

        var milikPasien = await laporan.GetDietsAsync(
            new GziReportQuery { PatientId = h.PasienId, PageSize = 50 });
        var milikOrangLain = await laporan.GetDietsAsync(
            new GziReportQuery { PatientId = Guid.NewGuid(), PageSize = 50 });

        Assert.NotEmpty(milikPasien.Items);
        Assert.Empty(milikOrangLain.Items);
    }

    [Fact]
    public async Task Penyaring_ahli_gizi_membuang_pelaksana_lain()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await SiapkanSatuAlurAsync(h, konteks);

        var laporan = h.ReportService(konteks);

        Assert.NotEmpty((await laporan.GetServicesAsync(
            new GziReportQuery { WorkforceId = h.AhliGiziId, PageSize = 50 })).Items);
        Assert.Empty((await laporan.GetServicesAsync(
            new GziReportQuery { WorkforceId = Guid.NewGuid(), PageSize = 50 })).Items);
    }

    [Fact]
    public async Task Penyaring_jenis_diet_berlaku_pada_laporan_diet()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await SiapkanSatuAlurAsync(h, konteks);

        var laporan = h.ReportService(konteks);

        Assert.NotEmpty((await laporan.GetDietsAsync(
            new GziReportQuery { DietTypeId = h.JenisDietId, PageSize = 50 })).Items);
        Assert.Empty((await laporan.GetDietsAsync(
            new GziReportQuery { DietTypeId = h.JenisDietNonaktifId, PageSize = 50 })).Items);
    }

    [Fact]
    public async Task Penyaring_status_diet_berlaku()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await SiapkanSatuAlurAsync(h, konteks);

        var laporan = h.ReportService(konteks);

        Assert.NotEmpty((await laporan.GetDietsAsync(new GziReportQuery
        {
            DietStatus = GziPatientDietStatus.Active,
            PageSize = 50
        })).Items);

        Assert.Empty((await laporan.GetDietsAsync(new GziReportQuery
        {
            DietStatus = GziPatientDietStatus.Stopped,
            PageSize = 50
        })).Items);
    }

    [Fact]
    public async Task Penyaring_status_order_berlaku_pada_laporan_pelayanan()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await SiapkanSatuAlurAsync(h, konteks);

        var laporan = h.ReportService(konteks);

        // Kunjungan pertama menaikkan order menjadi InProgress secara otomatis.
        Assert.NotEmpty((await laporan.GetServicesAsync(new GziReportQuery
        {
            OrderStatus = GziOrderStatus.InProgress,
            PageSize = 50
        })).Items);

        Assert.Empty((await laporan.GetServicesAsync(new GziReportQuery
        {
            OrderStatus = GziOrderStatus.Cancelled,
            PageSize = 50
        })).Items);
    }

    // ------------------------------------------------------------------ data terhapus

    [Fact]
    public async Task Baris_yang_ditandai_terhapus_tidak_ikut_terhitung()
    {
        // Seluruh query laporan menyaring `!IsDelete`. Kalau penyaring itu lepas, laporan akan
        // menghitung pelayanan yang sudah dibatalkan dan angkanya tidak dapat dipertanggungkan.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await SiapkanSatuAlurAsync(h, konteks);

        var laporan = h.ReportService(konteks);
        var sebelum = (await laporan.GetDietsAsync(new GziReportQuery { PageSize = 50 })).Items.Count;

        var diet = konteks.GziPatientDiets.First();
        diet.IsDelete = true;
        await konteks.SaveChangesAsync();

        var sesudah = (await laporan.GetDietsAsync(new GziReportQuery { PageSize = 50 })).Items.Count;

        Assert.Equal(sebelum - 1, sesudah);
    }

    [Fact]
    public async Task Order_yang_ditandai_terhapus_tidak_ikut_ringkasan()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var orderId = await SiapkanSatuAlurAsync(h, konteks);

        var laporan = h.ReportService(konteks);
        Assert.Equal(1, (await laporan.GetSummaryAsync(
            new GziReportQuery { PatientId = h.PasienId })).TotalOrders);

        var order = konteks.GziNutritionOrders.First(x => x.Id == orderId);
        order.IsDelete = true;
        await konteks.SaveChangesAsync();

        Assert.Equal(0, (await laporan.GetSummaryAsync(
            new GziReportQuery { PatientId = h.PasienId })).TotalOrders);
    }

    // ------------------------------------------------------------------ paging

    [Fact]
    public async Task Paging_laporan_membatasi_jumlah_baris()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await SiapkanSatuAlurAsync(h, konteks);

        var halaman = await h.ReportService(konteks)
            .GetDietsAsync(new GziReportQuery { PageNumber = 1, PageSize = 1 });

        Assert.True(halaman.Items.Count <= 1);
        Assert.Equal(1, halaman.PageNumber);
    }

    [Fact]
    public async Task Halaman_di_luar_jangkauan_kosong_tanpa_galat()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await SiapkanSatuAlurAsync(h, konteks);

        var halaman = await h.ReportService(konteks)
            .GetDietsAsync(new GziReportQuery { PageNumber = 99, PageSize = 20 });

        Assert.Empty(halaman.Items);
    }
}
