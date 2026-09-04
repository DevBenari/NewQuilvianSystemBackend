using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;

namespace QuilvianSystemBackend.Tests.HealthServices.NutritionManagement;

/// <summary>
/// Pengujian alur Phase 1 modul Gizi, dari pasien rawat inap sampai makanan diserahkan.
/// </summary>
/// <remarks>
/// Yang diuji di sini bukan sekadar setiap perintah berhasil, melainkan janji yang paling
/// menentukan: riwayat diet tidak pernah hilang, dan snapshot produksi tidak pernah berubah
/// walaupun diet pasien berubah sesudahnya.
/// </remarks>
public sealed class NutritionDietServiceTests
{
    private static NutritionDietService Build(NutritionTestContext ctx) =>
        new(ctx.Context, ctx.Accessor, ctx.Logger);

    private static PrescribeGzDietRequest DietRequest(NutritionTestContext ctx, Guid dietTypeId,
        string key, string? changeReason = null) => new()
        {
            PatientId = ctx.ActivePatientId,
            EncounterId = ctx.ActiveEncounterId,
            DietTypeId = dietTypeId,
            FoodFormId = ctx.FoodFormRegularId,
            PrescribedByWorkforceId = ctx.WorkforceId,
            EnergyRequirementKcal = 1800,
            ChangeReason = changeReason,
            IdempotencyKey = key
        };

    // ------------------------------------------------- 1-2. daftar pasien gizi

    [Fact]
    public async Task DaftarPasien_MemuatPasienRawatInapYangMasihDirawat()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var service = Build(ctx);

        var result = await service.GetNutritionPatientsAsync(new GzNutritionPatientQuery());

        var patient = Assert.Single(result.Items);
        Assert.Equal(ctx.ActivePatientId, patient.PatientId);
        Assert.Equal("Melati 1", patient.RoomName);
        Assert.Equal("Bed 1", patient.BedName);
        Assert.Equal("dr. Uji", patient.DoctorName);
    }

    [Fact]
    public async Task DaftarPasien_TidakMemuatPasienYangSudahPulang()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var service = Build(ctx);

        var result = await service.GetNutritionPatientsAsync(new GzNutritionPatientQuery());

        Assert.DoesNotContain(result.Items, x => x.PatientId == ctx.DischargedPatientId);
    }

    // -------------------------------------------------------- 3-6. diet pasien

    [Fact]
    public async Task Diet_DapatDitetapkanUntukPasienRawatInapAktif()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var service = Build(ctx);

        var diet = await service.PrescribeAsync(DietRequest(ctx, ctx.DietTypeRegularId, "k1"));

        Assert.Equal(GzPatientDietStatus.Active, diet.Status);
        Assert.Equal(ctx.ActiveEncounterId, diet.EncounterId);
        Assert.Equal("Diet Biasa", diet.DietTypeName);
    }

    [Fact]
    public async Task Diet_UntukKunjunganYangTidakAktif_Ditolak()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var service = Build(ctx);

        var request = DietRequest(ctx, ctx.DietTypeRegularId, "k1");
        request.PatientId = ctx.DischargedPatientId;
        request.EncounterId = ctx.DischargedEncounterId;

        var exception = await Assert.ThrowsAsync<NutritionUnprocessableException>(
            () => service.PrescribeAsync(request));

        Assert.Equal("GIZ001", exception.Code);
    }

    [Fact]
    public async Task Diet_DiubahTanpaAlasan_Ditolak()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var service = Build(ctx);
        await service.PrescribeAsync(DietRequest(ctx, ctx.DietTypeRegularId, "k1"));

        var exception = await Assert.ThrowsAsync<NutritionUnprocessableException>(
            () => service.PrescribeAsync(DietRequest(ctx, ctx.DietTypeDiabetesId, "k2")));

        Assert.Equal("GIZ010", exception.Code);
    }

    [Fact]
    public async Task Diet_RiwayatnyaTidakPernahHilangDanHanyaSatuYangAktif()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var service = Build(ctx);

        await service.PrescribeAsync(DietRequest(ctx, ctx.DietTypeRegularId, "k1"));
        await service.PrescribeAsync(
            DietRequest(ctx, ctx.DietTypeDiabetesId, "k2", "Gula darah naik"));

        var history = await service.GetDietHistoryAsync(ctx.ActiveEncounterId);

        Assert.Equal(2, history.Count);
        Assert.Single(history, x => x.Status == GzPatientDietStatus.Active);

        var previous = Assert.Single(history, x => x.Status == GzPatientDietStatus.Changed);
        Assert.Equal("Diet Biasa", previous.DietTypeName);
        Assert.Equal("Gula darah naik", previous.ChangeReason);
        Assert.NotNull(previous.EndAt);
    }

    [Fact]
    public async Task Diet_DihentikanTanpaPengganti_MenjadiStopped()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var service = Build(ctx);
        var diet = await service.PrescribeAsync(DietRequest(ctx, ctx.DietTypeRegularId, "k1"));

        var stopped = await service.StopAsync(diet.Id, new StopGzDietRequest
        {
            Reason = "Pasien puasa untuk operasi",
            ExpectedVersion = diet.Version,
            IdempotencyKey = "stop1"
        });

        Assert.Equal(GzPatientDietStatus.Stopped, stopped.Status);
        Assert.NotNull(stopped.EndAt);
    }

    // ----------------------------------------------------------- 7-10. produksi

    private static async Task<(NutritionDietService Service, GzProductionBatchDetailResponse Batch)>
        CreateBatchAsync(NutritionTestContext ctx)
    {
        var service = Build(ctx);
        await service.PrescribeAsync(DietRequest(ctx, ctx.DietTypeRegularId, "k1"));

        var batch = await service.CreateBatchAsync(new CreateGzProductionBatchRequest
        {
            MealScheduleId = ctx.MealScheduleId,
            IdempotencyKey = "batch1"
        });

        return (service, batch);
    }

    [Fact]
    public async Task Produksi_MemakaiDietYangSedangAktif()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var (_, batch) = await CreateBatchAsync(ctx);

        Assert.Equal(1, batch.TotalPortion);
        var portion = Assert.Single(batch.Portions);
        Assert.Equal("Diet Biasa", portion.DietTypeName);
        Assert.Equal("Pasien AKTIF", portion.PatientName);
    }

    [Fact]
    public async Task Produksi_TanpaPasienBerdiet_Ditolak()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var service = Build(ctx);

        var exception = await Assert.ThrowsAsync<NutritionUnprocessableException>(
            () => service.CreateBatchAsync(new CreateGzProductionBatchRequest
            {
                MealScheduleId = ctx.MealScheduleId,
                IdempotencyKey = "batch1"
            }));

        Assert.Equal("GIZ016", exception.Code);
    }

    [Fact]
    public async Task Produksi_MenyimpanSnapshotRuangBedDanDokter()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var (_, batch) = await CreateBatchAsync(ctx);

        var portion = Assert.Single(batch.Portions);
        Assert.Equal("Melati 1", portion.RoomName);
        Assert.Equal("Bed 1", portion.BedName);
        Assert.Equal("dr. Uji", portion.DoctorName);
    }

    [Fact]
    public async Task Produksi_DietBerubahSesudahnya_SnapshotTIDAKIkutBerubah()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var (service, batch) = await CreateBatchAsync(ctx);

        await service.PrescribeAsync(
            DietRequest(ctx, ctx.DietTypeDiabetesId, "k2", "Gula darah naik"));

        var refreshed = await service.GetBatchDetailAsync(batch.Id);
        var portion = Assert.Single(refreshed!.Portions);

        // Inilah janji terpenting batch: apa yang sudah diproduksi tetap tercatat apa adanya.
        Assert.Equal("Diet Biasa", portion.DietTypeName);
        Assert.True(portion.IsDietChangedAfterProduction);
        Assert.Equal("Diet Diabetes", portion.CurrentDietTypeName);
        Assert.Equal(1, refreshed.DietChangedCount);
    }

    [Fact]
    public async Task Produksi_BatchKeduaPadaJadwalSama_Ditolak()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var (service, _) = await CreateBatchAsync(ctx);

        var exception = await Assert.ThrowsAsync<NutritionConflictException>(
            () => service.CreateBatchAsync(new CreateGzProductionBatchRequest
            {
                MealScheduleId = ctx.MealScheduleId,
                IdempotencyKey = "batch2"
            }));

        Assert.Equal("GIZ015", exception.Code);
    }

    [Fact]
    public async Task Produksi_TransisiStatusYangTidakSah_Ditolak()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var (service, batch) = await CreateBatchAsync(ctx);

        // Draft tidak boleh langsung menjadi Completed.
        var exception = await Assert.ThrowsAsync<NutritionConflictException>(
            () => service.ChangeBatchStatusAsync(batch.Id, new ChangeGzBatchStatusRequest
            {
                Status = GzProductionBatchStatus.Completed,
                ExpectedVersion = batch.Version,
                IdempotencyKey = "st1"
            }));

        Assert.Equal("GIZ019", exception.Code);
    }

    // -------------------------------------------------------- 11-15. distribusi

    private static async Task<GzProductionBatchDetailResponse> AdvanceToReadyAsync(
        NutritionDietService service, GzProductionBatchDetailResponse batch)
    {
        var current = batch;
        foreach (var status in new[]
        {
            GzProductionBatchStatus.Confirmed,
            GzProductionBatchStatus.InProduction,
            GzProductionBatchStatus.ReadyForDistribution
        })
        {
            current = await service.ChangeBatchStatusAsync(current.Id, new ChangeGzBatchStatusRequest
            {
                Status = status,
                ExpectedVersion = current.Version,
                IdempotencyKey = $"st-{status}"
            });
        }
        return current;
    }

    [Fact]
    public async Task Distribusi_SebelumBatchSiap_Ditolak()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var (service, batch) = await CreateBatchAsync(ctx);

        var exception = await Assert.ThrowsAsync<NutritionConflictException>(
            () => service.RecordDeliveryAsync(new RecordGzMealDeliveryRequest
            {
                ProductionBatchDetailId = batch.Portions[0].Id,
                Status = GzMealDeliveryStatus.Delivered,
                DeliveredByWorkforceId = ctx.WorkforceId,
                IdempotencyKey = "d1"
            }));

        Assert.Equal("GIZ018", exception.Code);
    }

    [Fact]
    public async Task Distribusi_SetelahBatchSiap_TercatatBesertaSisaMakanan()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var (service, batch) = await CreateBatchAsync(ctx);
        var ready = await AdvanceToReadyAsync(service, batch);

        var result = await service.RecordDeliveryAsync(new RecordGzMealDeliveryRequest
        {
            ProductionBatchDetailId = ready.Portions[0].Id,
            Status = GzMealDeliveryStatus.Delivered,
            DeliveredByWorkforceId = ctx.WorkforceId,
            LeftoverPercent = 25,
            IdempotencyKey = "d1"
        });

        var portion = Assert.Single(result.Portions);
        Assert.Equal(GzMealDeliveryStatus.Delivered, portion.DeliveryStatus);
        Assert.Equal(25, portion.LeftoverPercent);
        Assert.NotNull(portion.DeliveredAt);
    }

    [Fact]
    public async Task Distribusi_DicatatDuaKali_TidakMenggandakanBaris()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var (service, batch) = await CreateBatchAsync(ctx);
        var ready = await AdvanceToReadyAsync(service, batch);

        await service.RecordDeliveryAsync(new RecordGzMealDeliveryRequest
        {
            ProductionBatchDetailId = ready.Portions[0].Id,
            Status = GzMealDeliveryStatus.Delivered,
            DeliveredByWorkforceId = ctx.WorkforceId,
            IdempotencyKey = "d1"
        });

        var second = await service.RecordDeliveryAsync(new RecordGzMealDeliveryRequest
        {
            ProductionBatchDetailId = ready.Portions[0].Id,
            Status = GzMealDeliveryStatus.Refused,
            DeliveredByWorkforceId = ctx.WorkforceId,
            Note = "Pasien menolak",
            IdempotencyKey = "d2"
        });

        Assert.Single(ctx.Context.GzMealDeliveries);
        Assert.Equal(GzMealDeliveryStatus.Refused, second.Portions[0].DeliveryStatus);
    }

    [Fact]
    public async Task AlurPenuh_DariPasienSampaiMakananDiserahkan()
    {
        await using var ctx = await NutritionTestContext.CreateAsync();
        var service = Build(ctx);

        // pasien rawat inap aktif -> tampil di daftar
        var patients = await service.GetNutritionPatientsAsync(new GzNutritionPatientQuery());
        var patient = Assert.Single(patients.Items);
        Assert.Null(patient.PatientDietId);

        // -> diberi diet aktif
        await service.PrescribeAsync(DietRequest(ctx, ctx.DietTypeRegularId, "k1"));
        var withDiet = await service.GetNutritionPatientsAsync(new GzNutritionPatientQuery());
        Assert.NotNull(withDiet.Items[0].PatientDietId);

        // -> masuk kebutuhan produksi
        var batch = await service.CreateBatchAsync(new CreateGzProductionBatchRequest
        {
            MealScheduleId = ctx.MealScheduleId,
            IdempotencyKey = "batch1"
        });
        Assert.Equal(1, batch.TotalPortion);
        Assert.Single(batch.Groups);

        // -> batch berjalan sampai siap distribusi
        var ready = await AdvanceToReadyAsync(service, batch);
        Assert.Equal(GzProductionBatchStatus.ReadyForDistribution, ready.Status);

        // -> makanan diserahkan dan tercatat
        var delivered = await service.RecordDeliveryAsync(new RecordGzMealDeliveryRequest
        {
            ProductionBatchDetailId = ready.Portions[0].Id,
            Status = GzMealDeliveryStatus.Delivered,
            DeliveredByWorkforceId = ctx.WorkforceId,
            IdempotencyKey = "d1"
        });

        Assert.Equal(GzMealDeliveryStatus.Delivered, delivered.Portions[0].DeliveryStatus);

        // -> jejaknya utuh: distribusi tahu porsi, porsi tahu kunjungan dan pasien
        Assert.Equal(ctx.ActiveEncounterId, delivered.Portions[0].EncounterId);
        Assert.Equal(ctx.ActivePatientId, delivered.Portions[0].PatientId);
    }
}
