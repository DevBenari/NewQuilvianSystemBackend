using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Nutrition.Infrastructure;

namespace QuilvianSystemBackend.Tests.Nutrition;

/// <summary>
/// Batch produksi makanan dan distribusinya.
/// </summary>
/// <remarks>
/// <para>
/// Batch adalah daftar porsi yang dibawa ke dapur, dan isinya dibekukan sebagai snapshot saat
/// batch dibuat. Dua batch untuk tanggal dan jadwal makan yang sama berarti dapur memasak dua
/// kali untuk satu waktu makan; batch selesai yang bisa mundur ke draft berarti jejak distribusi
/// yang sudah terjadi dapat dihapus.
/// </para>
/// <para>
/// Distribusi hanya boleh dicatat setelah batch siap — makanan yang belum dimasak tidak dapat
/// diserahkan — dan pencatatan ulang harus MEMPERBARUI baris yang ada, bukan menambah baris
/// baru, supaya rekap sisa makanan tidak terhitung dua kali.
/// </para>
/// </remarks>
public class ProductionBatchRuleTests
{
    /// <summary>
    /// Satu pasien rawat inap dengan diet aktif — syarat minimum agar batch punya isi.
    /// </summary>
    private static async Task TetapkanDietAsync(NutritionHarness h, ApplicationDbContext konteks)
    {
        await h.DietService(konteks).PrescribeAsync(new PrescribeGzDietRequest
        {
            PatientId = h.PasienId,
            EncounterId = h.KunjunganRawatId,
            DietTypeId = h.JenisDietId,
            FoodFormId = h.BentukMakananId,
            PrescribedByWorkforceId = h.AhliGiziId,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });
    }

    private static CreateGzProductionBatchRequest Pembuatan(NutritionHarness h,
        Guid? jadwal = null, DateOnly? tanggal = null, string? kunci = null) => new()
        {
            ServiceDate = tanggal,
            MealScheduleId = jadwal ?? h.JadwalMakanId,
            Note = "Batch uji",
            IdempotencyKey = kunci ?? Guid.NewGuid().ToString("N")
        };

    private static async Task<GziProductionBatchDetailResponse> BatchSiapAsync(
        NutritionHarness h, ApplicationDbContext konteks)
    {
        var layanan = h.DietService(konteks);
        var batch = await layanan.CreateBatchAsync(Pembuatan(h));

        foreach (var status in new[]
        {
            GziProductionBatchStatus.Confirmed,
            GziProductionBatchStatus.InProduction,
            GziProductionBatchStatus.ReadyForDistribution
        })
        {
            batch = await layanan.ChangeBatchStatusAsync(batch.Id, new ChangeGzBatchStatusRequest
            {
                Status = status,
                ExpectedVersion = batch.Version,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            });
        }

        return batch;
    }

    // ------------------------------------------------------------- pembuatan batch

    [Fact]
    public async Task Batch_lahir_pada_status_draft_dengan_porsi_sebanyak_pasien_berdiet()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);

        var batch = await h.DietService(konteks).CreateBatchAsync(Pembuatan(h));

        Assert.Equal(GziProductionBatchStatus.Draft, batch.Status);
        Assert.Equal(1, batch.TotalPortion);
        Assert.Single(batch.Portions);
        Assert.StartsWith("PRD-", batch.BatchNumber);
        Assert.Equal(0, batch.Version);
    }

    [Fact]
    public async Task Batch_tidak_dapat_dibuat_tanpa_pasien_berdiet_aktif()
    {
        // `GIZ016`. Batch kosong akan dikirim ke dapur sebagai daftar nol porsi, dan itu bukan
        // keadaan yang dapat ditindaklanjuti siapa pun — dietnya yang belum ditetapkan.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.DietService(konteks).CreateBatchAsync(Pembuatan(h)));

        Assert.Equal("GIZ016", galat.Code);
    }

    [Fact]
    public async Task Jadwal_makan_yang_tidak_aktif_ditolak()
    {
        // `GIZ014`.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.DietService(konteks).CreateBatchAsync(Pembuatan(h, jadwal: h.JadwalMakanNonaktifId)));

        Assert.Equal("GIZ014", galat.Code);
    }

    [Fact]
    public async Task Jadwal_makan_asing_ditolak()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.DietService(konteks).CreateBatchAsync(Pembuatan(h, jadwal: Guid.NewGuid())));

        Assert.Equal("GIZ014", galat.Code);
    }

    [Fact]
    public async Task Batch_ganda_untuk_tanggal_dan_jadwal_yang_sama_ditolak()
    {
        // `GIZ015`. Dua batch untuk satu waktu makan berarti dapur memasak dua kali, dan rekap
        // porsinya terhitung ganda.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        await layanan.CreateBatchAsync(Pembuatan(h));

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            layanan.CreateBatchAsync(Pembuatan(h)));

        Assert.Equal("GIZ015", galat.Code);
    }

    [Fact]
    public async Task Jadwal_makan_berbeda_pada_tanggal_sama_tetap_boleh()
    {
        // Sisi lain `GIZ015`: yang dilarang pasangan tanggal DAN jadwal, bukan tanggalnya saja.
        // Satu hari memang punya beberapa waktu makan.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var siang = await layanan.CreateBatchAsync(Pembuatan(h, jadwal: h.JadwalMakanId));
        var malam = await layanan.CreateBatchAsync(Pembuatan(h, jadwal: h.JadwalMakanLainId));

        Assert.NotEqual(siang.Id, malam.Id);
    }

    [Fact]
    public async Task Tanggal_berbeda_pada_jadwal_sama_tetap_boleh()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var hariIni = DateOnly.FromDateTime(DateTime.UtcNow);
        await layanan.CreateBatchAsync(Pembuatan(h, tanggal: hariIni));
        var besok = await layanan.CreateBatchAsync(Pembuatan(h, tanggal: hariIni.AddDays(1)));

        Assert.Equal(GziProductionBatchStatus.Draft, besok.Status);
    }

    [Fact]
    public async Task Batch_yang_dibatalkan_tidak_menghalangi_batch_baru()
    {
        // Larangan ganda hanya memandang batch yang BUKAN Cancelled. Batch yang dibatalkan
        // karena salah jadwal tidak boleh mengunci waktu makan itu seharian.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var pertama = await layanan.CreateBatchAsync(Pembuatan(h));
        await layanan.ChangeBatchStatusAsync(pertama.Id, new ChangeGzBatchStatusRequest
        {
            Status = GziProductionBatchStatus.Cancelled,
            Reason = "Salah jadwal",
            ExpectedVersion = pertama.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var kedua = await layanan.CreateBatchAsync(Pembuatan(h));

        Assert.NotEqual(pertama.Id, kedua.Id);
        Assert.Equal(GziProductionBatchStatus.Draft, kedua.Status);
    }

    [Fact]
    public async Task Pembuatan_ulang_dengan_kunci_sama_tidak_menambah_batch()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var kunci = Guid.NewGuid().ToString("N");
        var pertama = await layanan.CreateBatchAsync(Pembuatan(h, kunci: kunci));
        var kedua = await layanan.CreateBatchAsync(Pembuatan(h, kunci: kunci));

        Assert.Equal(pertama.Id, kedua.Id);

        var daftar = await layanan.GetBatchesAsync(null);
        Assert.Single(daftar.Where(x => x.Id == pertama.Id));
    }

    [Fact]
    public async Task Kunci_idempotensi_wajib_diisi()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            h.DietService(konteks).CreateBatchAsync(Pembuatan(h, kunci: " ")));
    }

    [Fact]
    public async Task Isi_batch_dibekukan_sebagai_snapshot_diet_saat_dibuat()
    {
        // Snapshot disengaja: daftar yang sudah dibawa ke dapur tidak boleh berubah sendiri
        // ketika diet pasien diganti di tengah jalan.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);

        var batch = await h.DietService(konteks).CreateBatchAsync(Pembuatan(h));
        var porsi = batch.Portions.Single();

        Assert.Equal(h.PasienId, porsi.PatientId);
        Assert.Equal("Diet Biasa (Uji)", porsi.DietTypeName);
        Assert.Equal("Makanan Biasa (Uji)", porsi.FoodFormName);
        Assert.Equal(1, porsi.Portion);
    }

    // ------------------------------------------------------------- transisi status

    [Fact]
    public async Task Daur_hidup_batch_berjalan_berurutan_sampai_selesai()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var batch = await BatchSiapAsync(h, konteks);
        Assert.Equal(GziProductionBatchStatus.ReadyForDistribution, batch.Status);

        var selesai = await layanan.ChangeBatchStatusAsync(batch.Id, new ChangeGzBatchStatusRequest
        {
            Status = GziProductionBatchStatus.Completed,
            ExpectedVersion = batch.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        Assert.Equal(GziProductionBatchStatus.Completed, selesai.Status);
        Assert.NotNull(selesai.CompletedAt);
    }

    [Theory]
    [InlineData(GziProductionBatchStatus.InProduction)]          // Draft tidak boleh melompat
    [InlineData(GziProductionBatchStatus.ReadyForDistribution)]
    [InlineData(GziProductionBatchStatus.Completed)]
    public async Task Draft_tidak_dapat_melompati_konfirmasi(GziProductionBatchStatus tujuan)
    {
        // `GIZ019`. Melompati konfirmasi berarti batch masuk produksi tanpa pernah disetujui.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var batch = await layanan.CreateBatchAsync(Pembuatan(h));

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            layanan.ChangeBatchStatusAsync(batch.Id, new ChangeGzBatchStatusRequest
            {
                Status = tujuan,
                ExpectedVersion = batch.Version,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("GIZ019", galat.Code);
    }

    [Fact]
    public async Task Batch_selesai_tidak_dapat_mundur_ke_status_sebelumnya()
    {
        // Inti aturan transisi: batch yang sudah selesai tidak boleh dikembalikan menjadi draft,
        // karena itu menghapus jejak distribusi yang sudah terjadi.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var batch = await BatchSiapAsync(h, konteks);
        var selesai = await layanan.ChangeBatchStatusAsync(batch.Id, new ChangeGzBatchStatusRequest
        {
            Status = GziProductionBatchStatus.Completed,
            ExpectedVersion = batch.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        foreach (var mundur in new[]
        {
            GziProductionBatchStatus.Draft,
            GziProductionBatchStatus.Confirmed,
            GziProductionBatchStatus.InProduction,
            GziProductionBatchStatus.ReadyForDistribution,
            GziProductionBatchStatus.Cancelled
        })
        {
            var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
                layanan.ChangeBatchStatusAsync(selesai.Id, new ChangeGzBatchStatusRequest
                {
                    Status = mundur,
                    Reason = "Apa pun",
                    ExpectedVersion = selesai.Version,
                    IdempotencyKey = Guid.NewGuid().ToString("N")
                }));

            Assert.Equal("GIZ019", galat.Code);
        }
    }

    [Fact]
    public async Task Batch_siap_distribusi_tidak_dapat_dibatalkan()
    {
        // Setelah makanan siap, pembatalan bukan lagi pilihan yang sah menurut daur hidupnya —
        // yang tersisa hanya menyelesaikannya.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var batch = await BatchSiapAsync(h, konteks);

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            layanan.ChangeBatchStatusAsync(batch.Id, new ChangeGzBatchStatusRequest
            {
                Status = GziProductionBatchStatus.Cancelled,
                Reason = "Berubah pikiran",
                ExpectedVersion = batch.Version,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("GIZ019", galat.Code);
    }

    [Fact]
    public async Task Pembatalan_batch_wajib_beralasan()
    {
        // `GIZ017`.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var batch = await layanan.CreateBatchAsync(Pembuatan(h));

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            layanan.ChangeBatchStatusAsync(batch.Id, new ChangeGzBatchStatusRequest
            {
                Status = GziProductionBatchStatus.Cancelled,
                Reason = "   ",
                ExpectedVersion = batch.Version,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("GIZ017", galat.Code);
    }

    [Fact]
    public async Task Pembatalan_yang_beralasan_menyimpan_alasannya()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var batch = await layanan.CreateBatchAsync(Pembuatan(h));

        var dibatalkan = await layanan.ChangeBatchStatusAsync(batch.Id,
            new ChangeGzBatchStatusRequest
            {
                Status = GziProductionBatchStatus.Cancelled,
                Reason = "Pasokan bahan terlambat",
                ExpectedVersion = batch.Version,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            });

        Assert.Equal(GziProductionBatchStatus.Cancelled, dibatalkan.Status);
        Assert.Equal("Pasokan bahan terlambat", dibatalkan.CancelReason);
    }

    [Fact]
    public async Task Versi_yang_tidak_cocok_saat_mengubah_status_ditolak()
    {
        // `GIZ012`.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var batch = await layanan.CreateBatchAsync(Pembuatan(h));

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            layanan.ChangeBatchStatusAsync(batch.Id, new ChangeGzBatchStatusRequest
            {
                Status = GziProductionBatchStatus.Confirmed,
                ExpectedVersion = batch.Version + 3,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("GIZ012", galat.Code);
    }

    [Fact]
    public async Task Batch_yang_tidak_ada_tidak_dapat_diubah()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            h.DietService(konteks).ChangeBatchStatusAsync(Guid.NewGuid(),
                new ChangeGzBatchStatusRequest
                {
                    Status = GziProductionBatchStatus.Confirmed,
                    ExpectedVersion = 0,
                    IdempotencyKey = Guid.NewGuid().ToString("N")
                }));
    }

    [Fact]
    public async Task Detail_batch_yang_tidak_ada_mengembalikan_null()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        Assert.Null(await h.DietService(konteks).GetBatchDetailAsync(Guid.NewGuid()));
    }

    // ------------------------------------------------------------- distribusi

    [Fact]
    public async Task Distribusi_tidak_dapat_dicatat_sebelum_batch_siap()
    {
        // `GIZ018`. Makanan yang belum dimasak tidak dapat diserahkan, dan mencatatnya lebih
        // dulu akan membuat rekap asupan pasien berisi penyerahan yang tidak pernah terjadi.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var batch = await layanan.CreateBatchAsync(Pembuatan(h));
        var porsiId = batch.Portions.Single().Id;

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            layanan.RecordDeliveryAsync(new RecordGzMealDeliveryRequest
            {
                ProductionBatchDetailId = porsiId,
                Status = GziMealDeliveryStatus.Delivered,
                DeliveredByWorkforceId = h.AhliGiziId,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("GIZ018", galat.Code);
    }

    [Fact]
    public async Task Distribusi_tercatat_setelah_batch_siap()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var batch = await BatchSiapAsync(h, konteks);
        var porsiId = batch.Portions.Single().Id;

        var sesudah = await layanan.RecordDeliveryAsync(new RecordGzMealDeliveryRequest
        {
            ProductionBatchDetailId = porsiId,
            Status = GziMealDeliveryStatus.Delivered,
            DeliveredByWorkforceId = h.AhliGiziId,
            LeftoverPercent = 10,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var porsi = sesudah.Portions.Single();
        Assert.Equal(GziMealDeliveryStatus.Delivered, porsi.DeliveryStatus);
        Assert.Equal(10, porsi.LeftoverPercent);
        Assert.NotNull(porsi.DeliveredAt);
    }

    [Fact]
    public async Task Pencatatan_ulang_memperbarui_baris_yang_ada_bukan_menambah()
    {
        // Rekap sisa makanan dijumlahkan dari baris penyerahan. Baris kedua untuk satu porsi
        // membuat sisa makanan terhitung dua kali, dan evaluasi asupan pasien ikut salah.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var batch = await BatchSiapAsync(h, konteks);
        var porsiId = batch.Portions.Single().Id;

        await layanan.RecordDeliveryAsync(new RecordGzMealDeliveryRequest
        {
            ProductionBatchDetailId = porsiId,
            Status = GziMealDeliveryStatus.Delivered,
            DeliveredByWorkforceId = h.AhliGiziId,
            LeftoverPercent = 10,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var sesudah = await layanan.RecordDeliveryAsync(new RecordGzMealDeliveryRequest
        {
            ProductionBatchDetailId = porsiId,
            Status = GziMealDeliveryStatus.Refused,
            DeliveredByWorkforceId = h.AhliGiziId,
            LeftoverPercent = 100,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        Assert.Single(sesudah.Portions);
        var porsi = sesudah.Portions.Single();
        Assert.Equal(GziMealDeliveryStatus.Refused, porsi.DeliveryStatus);
        Assert.Equal(100, porsi.LeftoverPercent);

        Assert.Equal(1, konteks.GziMealDeliveries
            .Count(x => x.ProductionBatchDetailId == porsiId && !x.IsDelete));
    }

    [Fact]
    public async Task Penolakan_pasien_tidak_mencatat_waktu_penyerahan()
    {
        // `Refused` dibedakan dari `Delivered` supaya terlihat di evaluasi asupan; makanan yang
        // ditolak tidak pernah berpindah ke pasien, jadi waktu penyerahannya harus kosong.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var batch = await BatchSiapAsync(h, konteks);

        var sesudah = await layanan.RecordDeliveryAsync(new RecordGzMealDeliveryRequest
        {
            ProductionBatchDetailId = batch.Portions.Single().Id,
            Status = GziMealDeliveryStatus.Refused,
            DeliveredByWorkforceId = h.AhliGiziId,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var porsi = sesudah.Portions.Single();
        Assert.Equal(GziMealDeliveryStatus.Refused, porsi.DeliveryStatus);
        Assert.Null(porsi.DeliveredAt);
    }

    [Fact]
    public async Task Tidak_tersaji_juga_tidak_mencatat_waktu_penyerahan()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var batch = await BatchSiapAsync(h, konteks);

        var sesudah = await layanan.RecordDeliveryAsync(new RecordGzMealDeliveryRequest
        {
            ProductionBatchDetailId = batch.Portions.Single().Id,
            Status = GziMealDeliveryStatus.NotServed,
            DeliveredByWorkforceId = h.AhliGiziId,
            Note = "Pasien sedang tindakan",
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var porsi = sesudah.Portions.Single();
        Assert.Equal(GziMealDeliveryStatus.NotServed, porsi.DeliveryStatus);
        Assert.Null(porsi.DeliveredAt);
    }

    [Fact]
    public async Task Distribusi_masih_boleh_dicatat_setelah_batch_selesai()
    {
        // Batch `Completed` ikut diizinkan oleh source — penyerahan yang terlambat dicatat tetap
        // harus dapat masuk, karena menolaknya akan membuat porsi itu hilang dari rekap.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var batch = await BatchSiapAsync(h, konteks);
        var selesai = await layanan.ChangeBatchStatusAsync(batch.Id, new ChangeGzBatchStatusRequest
        {
            Status = GziProductionBatchStatus.Completed,
            ExpectedVersion = batch.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var sesudah = await layanan.RecordDeliveryAsync(new RecordGzMealDeliveryRequest
        {
            ProductionBatchDetailId = selesai.Portions.Single().Id,
            Status = GziMealDeliveryStatus.Delivered,
            DeliveredByWorkforceId = h.AhliGiziId,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        Assert.Equal(GziMealDeliveryStatus.Delivered, sesudah.Portions.Single().DeliveryStatus);
    }

    [Fact]
    public async Task Distribusi_pada_batch_yang_dibatalkan_ditolak()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        await TetapkanDietAsync(h, konteks);
        var layanan = h.DietService(konteks);

        var batch = await layanan.CreateBatchAsync(Pembuatan(h));
        var porsiId = batch.Portions.Single().Id;

        await layanan.ChangeBatchStatusAsync(batch.Id, new ChangeGzBatchStatusRequest
        {
            Status = GziProductionBatchStatus.Cancelled,
            Reason = "Dibatalkan",
            ExpectedVersion = batch.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            layanan.RecordDeliveryAsync(new RecordGzMealDeliveryRequest
            {
                ProductionBatchDetailId = porsiId,
                Status = GziMealDeliveryStatus.Delivered,
                DeliveredByWorkforceId = h.AhliGiziId,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("GIZ018", galat.Code);
    }

    [Fact]
    public async Task Porsi_yang_tidak_ada_tidak_dapat_dicatat()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            h.DietService(konteks).RecordDeliveryAsync(new RecordGzMealDeliveryRequest
            {
                ProductionBatchDetailId = Guid.NewGuid(),
                Status = GziMealDeliveryStatus.Delivered,
                DeliveredByWorkforceId = h.AhliGiziId,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));
    }

    [Fact]
    public async Task Kunci_idempotensi_distribusi_wajib_diisi()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            h.DietService(konteks).RecordDeliveryAsync(new RecordGzMealDeliveryRequest
            {
                ProductionBatchDetailId = Guid.NewGuid(),
                Status = GziMealDeliveryStatus.Delivered,
                DeliveredByWorkforceId = h.AhliGiziId,
                IdempotencyKey = ""
            }));
    }

    [Fact]
    public void Nomor_status_batch_tetap_pada_angka_yang_sama()
    {
        Assert.Equal(1, (int)GziProductionBatchStatus.Draft);
        Assert.Equal(2, (int)GziProductionBatchStatus.Confirmed);
        Assert.Equal(3, (int)GziProductionBatchStatus.InProduction);
        Assert.Equal(4, (int)GziProductionBatchStatus.ReadyForDistribution);
        Assert.Equal(5, (int)GziProductionBatchStatus.Completed);
        Assert.Equal(6, (int)GziProductionBatchStatus.Cancelled);
    }
}
