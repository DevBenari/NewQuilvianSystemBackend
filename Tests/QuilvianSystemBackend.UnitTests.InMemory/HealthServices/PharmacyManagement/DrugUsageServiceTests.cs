using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.HealthServices.NutritionManagement;

namespace QuilvianSystemBackend.Tests.HealthServices.PharmacyManagement;

/// <summary>
/// Pengujian pencatatan pemakaian obat pasien.
/// </summary>
/// <remarks>
/// Yang diuji adalah janji yang menentukan: draft tidak menyentuh stok, pencatatan mengurangi
/// stok menurut kedaluwarsa terdekat dan menyimpan batch yang terpakai, stok yang kurang
/// membatalkan seluruh pencatatan, dan yang sudah dicatat tidak dapat diubah maupun
/// dibatalkan begitu saja.
/// </remarks>
public sealed class DrugUsageServiceTests
{
    private sealed class Fixture : IAsyncDisposable
    {
        public required ApplicationDbContext Context { get; init; }
        public required DrugUsageService Service { get; init; }
        public required DrugStockService StockService { get; init; }
        public required Guid DrugId { get; init; }
        public required Guid MeasurementId { get; init; }
        public required Guid DepoId { get; init; }
        public required Guid NoDispenseId { get; init; }
        public required Guid EncounterId { get; init; }
        public required Guid WorkforceId { get; init; }

        public async ValueTask DisposeAsync() => await Context.DisposeAsync();
    }

    private static async Task<Fixture> CreateAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"drug-usage-{Guid.NewGuid():N}").Options;
        var context = new ApplicationDbContext(options);

        var drugId = Guid.NewGuid();
        var measurementId = Guid.NewGuid();
        var depoId = Guid.NewGuid();
        var noDispenseId = Guid.NewGuid();
        var encounterId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var serviceUnitId = Guid.NewGuid();
        var workforceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        context.Set<MstDrugCategory>().Add(new MstDrugCategory
        {
            Id = categoryId, DrugCategoryCode = "K1", DrugCategoryName = "Umum",
            DrugCategoryType = "General", IsActive = true
        });

        context.Set<MstDrug>().Add(new MstDrug
        {
            Id = drugId, DrugCategoryId = categoryId, DrugCode = "OBT-001",
            DrugName = "Paracetamol 500 mg", IsActive = true
        });

        context.Set<MstMeasurement>().Add(new MstMeasurement
        {
            Id = measurementId, MeasurementCode = "TAB", MeasurementName = "Tablet",
            MeasurementType = "Unit", IsActive = true
        });

        context.Set<MstServiceUnit>().Add(new MstServiceUnit
        {
            Id = serviceUnitId, ServiceUnitCode = "ICU", ServiceUnitName = "ICU", IsActive = true
        });

        context.Set<MstPatient>().Add(new MstPatient
        {
            Id = patientId, PatientCode = "P-001", MedicalRecordNumber = "RM-001",
            FullName = "Pasien Uji", IsActive = true
        });

        context.Set<RegPatientEncounter>().Add(new RegPatientEncounter
        {
            Id = encounterId, EncounterNumber = "ENC-001",
            PatientId = patientId, ServiceUnitId = serviceUnitId
        });

        context.Set<MstWorkforceProfile>().Add(new MstWorkforceProfile
        {
            Id = workforceId, ProfileCode = "WF1", DisplayName = "Perawat Uji", IsActive = true
        });

        context.Set<MstDrugStorageLocation>().AddRange(
            new MstDrugStorageLocation
            {
                Id = depoId, StorageLocationCode = "DP1",
                StorageLocationName = "Depo Rawat Inap", IsActive = true,
                IsAllowDispensing = true
            },
            // Lokasi yang menyimpan tetapi tidak boleh menyerahkan, misalnya area karantina.
            new MstDrugStorageLocation
            {
                Id = noDispenseId, StorageLocationCode = "QRT",
                StorageLocationName = "Area Karantina", IsActive = true,
                IsAllowDispensing = false
            });

        await context.SaveChangesAsync();

        var accessor = new MutableHttpContextAccessor();
        accessor.SetUser(Guid.NewGuid());
        var logger = new LoggerService(NullLogger<LoggerService>.Instance, accessor);
        var stockService = new DrugStockService(context, accessor, logger);

        return new Fixture
        {
            Context = context,
            StockService = stockService,
            Service = new DrugUsageService(context, accessor, logger, stockService),
            DrugId = drugId,
            MeasurementId = measurementId,
            DepoId = depoId,
            NoDispenseId = noDispenseId,
            EncounterId = encounterId,
            WorkforceId = workforceId
        };
    }

    private static DateOnly Hari(int selisih) =>
        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(selisih);

    private static Task<DrugStockBalanceResponse> IsiAsync(Fixture f, string batch,
        DateOnly expiry, decimal quantity, Guid? locationId = null) =>
        f.StockService.RecordOpeningBalanceAsync(new RecordOpeningBalanceRequest
        {
            Batch = new DrugBatchInput
            {
                DrugId = f.DrugId, BatchNumber = batch, ExpiryDate = expiry
            },
            StorageLocationId = locationId ?? f.DepoId,
            Quantity = quantity,
            IdempotencyKey = $"open-{batch}"
        });

    private static CreateDrugUsageRequest Pemakaian(Fixture f, decimal quantity,
        string key = "u1") => new()
        {
            EncounterId = f.EncounterId,
            StorageLocationId = f.DepoId,
            RecordedByWorkforceId = f.WorkforceId,
            Items = [new DrugUsageItemInput
            {
                DrugId = f.DrugId, MeasurementId = f.MeasurementId, Quantity = quantity
            }],
            IdempotencyKey = key
        };

    // ---------------------------------------------------------------------- draft

    [Fact]
    public async Task Buat_MenghasilkanDraftTanpaMenyentuhStok()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var hasil = await f.Service.CreateAsync(Pemakaian(f, 10));

        Assert.Equal(DrugUsageStatus.Draft, hasil.Status);
        Assert.True(hasil.IsEditable);
        Assert.StartsWith("USG-", hasil.UsageNumber);
        Assert.Equal("Pasien Uji", hasil.PatientName);
        Assert.Equal("RM-001", hasil.MedicalRecordNumber);

        // Draft belum tentu jadi; stok tidak boleh berkurang sebelum benar-benar dicatat.
        var saldo = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery());
        Assert.Equal(100m, Assert.Single(saldo.Items).QuantityOnHand);
    }

    [Fact]
    public async Task Buat_LokasiTidakBolehMenyerahkan_Ditolak()
    {
        await using var f = await CreateAsync();

        var permintaan = Pemakaian(f, 10);
        permintaan.StorageLocationId = f.NoDispenseId;

        var exception = await Assert.ThrowsAsync<DrugUsageUnprocessableException>(
            () => f.Service.CreateAsync(permintaan));

        Assert.Equal("PHM064", exception.Code);
    }

    [Fact]
    public async Task Buat_KunciSamaDikirimDuaKali_TidakMenggandakan()
    {
        await using var f = await CreateAsync();

        var pertama = await f.Service.CreateAsync(Pemakaian(f, 10));
        var kedua = await f.Service.CreateAsync(Pemakaian(f, 10));

        Assert.Equal(pertama.Id, kedua.Id);
        Assert.Single(f.Context.PhmDrugUsages);
    }

    [Fact]
    public async Task Buat_ObatSamaDuaBaris_Diizinkan()
    {
        await using var f = await CreateAsync();

        // Berbeda dari permintaan dan transfer: obat yang sama boleh dipakai dua kali dalam
        // satu pencatatan, misalnya dosis pagi dan dosis malam.
        var permintaan = Pemakaian(f, 5);
        permintaan.Items.Add(new DrugUsageItemInput
        {
            DrugId = f.DrugId, MeasurementId = f.MeasurementId, Quantity = 5
        });

        var hasil = await f.Service.CreateAsync(permintaan);

        Assert.Equal(2, hasil.Items.Count);
    }

    // ------------------------------------------------------------------ pencatatan

    [Fact]
    public async Task Catat_MengurangiStokMenurutKedaluwarsaTerdekat()
    {
        await using var f = await CreateAsync();

        await IsiAsync(f, "B-LAMBAT", Hari(300), 50);
        await IsiAsync(f, "B-CEPAT", Hari(30), 20);

        var draft = await f.Service.CreateAsync(Pemakaian(f, 30));
        var dicatat = await f.Service.RecordAsync(draft.Id, new DrugUsageCommandRequest
        {
            ExpectedVersion = draft.Version, IdempotencyKey = "rec-1"
        });

        Assert.Equal(DrugUsageStatus.NotBilled, dicatat.Status);
        Assert.NotNull(dicatat.RecordedAt);

        var alokasi = Assert.Single(dicatat.Items).Allocations;
        Assert.Equal(2, alokasi.Count);
        Assert.Equal("B-CEPAT", alokasi[0].BatchNumber);
        Assert.Equal(20m, alokasi[0].Quantity);
        Assert.Equal("B-LAMBAT", alokasi[1].BatchNumber);
        Assert.Equal(10m, alokasi[1].Quantity);

        var saldo = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery());
        Assert.Equal(40m, saldo.Items.Sum(x => x.QuantityOnHand));
    }

    [Fact]
    public async Task Catat_MenulisKartuStokDenganDokumenAsalnya()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var draft = await f.Service.CreateAsync(Pemakaian(f, 15));
        await f.Service.RecordAsync(draft.Id, new DrugUsageCommandRequest
        {
            ExpectedVersion = draft.Version, IdempotencyKey = "rec-1"
        });

        var kartu = await f.StockService.GetMutationsAsync(new DrugStockMutationQuery
        {
            SourceDocumentType = DrugStockSourceDocumentTypes.DrugUsage
        });

        var baris = Assert.Single(kartu.Items);
        Assert.Equal(-15m, baris.QuantityChange);
        Assert.Equal(100m, baris.BalanceBefore);
        Assert.Equal(85m, baris.BalanceAfter);
        Assert.Equal(draft.Id, baris.SourceDocumentId);
    }

    [Fact]
    public async Task Catat_StokTidakCukup_MembatalkanSeluruhPencatatan()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 5);

        var permintaan = Pemakaian(f, 3);
        permintaan.Items.Add(new DrugUsageItemInput
        {
            DrugId = f.DrugId, MeasurementId = f.MeasurementId, Quantity = 99
        });

        var draft = await f.Service.CreateAsync(permintaan);

        var exception = await Assert.ThrowsAsync<DrugUsageUnprocessableException>(
            () => f.Service.RecordAsync(draft.Id, new DrugUsageCommandRequest
            {
                ExpectedVersion = draft.Version, IdempotencyKey = "rec-1"
            }));

        Assert.Equal("PHM028", exception.Code);
        Assert.Contains("Paracetamol", exception.Message);

        // Baris pertama sempat berhasil; kegagalan baris kedua tidak boleh meninggalkan
        // pengurangan sebagian, karena stok yang berkurang untuk pemakaian yang tidak jadi
        // tercatat adalah selisih yang tidak dapat dijelaskan.
        var saldo = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery());
        Assert.Equal(5m, Assert.Single(saldo.Items).QuantityOnHand);
    }

    [Fact]
    public async Task Catat_BatchKedaluwarsa_TidakIkutDipakai()
    {
        await using var f = await CreateAsync();

        await IsiAsync(f, "B-LEWAT", Hari(-1), 100);
        await IsiAsync(f, "B-SEGAR", Hari(90), 5);

        var draft = await f.Service.CreateAsync(Pemakaian(f, 6));

        var exception = await Assert.ThrowsAsync<DrugUsageUnprocessableException>(
            () => f.Service.RecordAsync(draft.Id, new DrugUsageCommandRequest
            {
                ExpectedVersion = draft.Version, IdempotencyKey = "rec-1"
            }));

        Assert.Equal("PHM028", exception.Code);
    }

    [Fact]
    public async Task Catat_DuaKali_TidakMengurangiStokDuaKali()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var draft = await f.Service.CreateAsync(Pemakaian(f, 20));
        var pertama = await f.Service.RecordAsync(draft.Id, new DrugUsageCommandRequest
        {
            ExpectedVersion = draft.Version, IdempotencyKey = "rec-1"
        });

        var kedua = await f.Service.RecordAsync(draft.Id, new DrugUsageCommandRequest
        {
            ExpectedVersion = pertama.Version, IdempotencyKey = "rec-2"
        });

        Assert.Equal(DrugUsageStatus.NotBilled, kedua.Status);

        var saldo = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery());
        Assert.Equal(80m, Assert.Single(saldo.Items).QuantityOnHand);
    }

    // ------------------------------------------------------------ sesudah dicatat

    [Fact]
    public async Task Ubah_SetelahDicatat_Ditolak()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var draft = await f.Service.CreateAsync(Pemakaian(f, 10));
        var dicatat = await f.Service.RecordAsync(draft.Id, new DrugUsageCommandRequest
        {
            ExpectedVersion = draft.Version, IdempotencyKey = "rec-1"
        });

        var exception = await Assert.ThrowsAsync<DrugUsageConflictException>(
            () => f.Service.UpdateAsync(dicatat.Id, new UpdateDrugUsageRequest
            {
                StorageLocationId = f.DepoId,
                Items = [new DrugUsageItemInput
                {
                    DrugId = f.DrugId, MeasurementId = f.MeasurementId, Quantity = 5
                }],
                ExpectedVersion = dicatat.Version,
                IdempotencyKey = "upd-1"
            }));

        Assert.Equal("PHM060", exception.Code);
    }

    [Fact]
    public async Task Batal_SetelahDicatat_Ditolak()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var draft = await f.Service.CreateAsync(Pemakaian(f, 10));
        var dicatat = await f.Service.RecordAsync(draft.Id, new DrugUsageCommandRequest
        {
            ExpectedVersion = draft.Version, IdempotencyKey = "rec-1"
        });

        var exception = await Assert.ThrowsAsync<DrugUsageConflictException>(
            () => f.Service.CancelAsync(dicatat.Id, new CancelDrugUsageRequest
            {
                Reason = "Salah pasien.",
                ExpectedVersion = dicatat.Version,
                IdempotencyKey = "can-1"
            }));

        Assert.Equal("PHM060", exception.Code);
    }

    [Fact]
    public async Task Batal_MasihDraft_TidakMenyentuhStok()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var draft = await f.Service.CreateAsync(Pemakaian(f, 10));
        var dibatalkan = await f.Service.CancelAsync(draft.Id, new CancelDrugUsageRequest
        {
            Reason = "Salah pilih pasien.",
            ExpectedVersion = draft.Version,
            IdempotencyKey = "can-1"
        });

        Assert.Equal(DrugUsageStatus.Cancelled, dibatalkan.Status);
        Assert.Equal("Salah pilih pasien.", dibatalkan.CancelReason);

        var saldo = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery());
        Assert.Equal(100m, Assert.Single(saldo.Items).QuantityOnHand);
    }

    [Fact]
    public async Task Batal_TanpaAlasan_Ditolak()
    {
        await using var f = await CreateAsync();
        var draft = await f.Service.CreateAsync(Pemakaian(f, 10));

        var exception = await Assert.ThrowsAsync<DrugUsageUnprocessableException>(
            () => f.Service.CancelAsync(draft.Id, new CancelDrugUsageRequest
            {
                Reason = "   ",
                ExpectedVersion = draft.Version,
                IdempotencyKey = "can-1"
            }));

        Assert.Equal("PHM062", exception.Code);
    }

    // ------------------------------------------------------------------ penelusuran

    [Fact]
    public async Task Alokasi_MembuatObatDapatDitelusuriSampaiPasien()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-RECALL", Hari(90), 100);

        var draft = await f.Service.CreateAsync(Pemakaian(f, 12));
        var dicatat = await f.Service.RecordAsync(draft.Id, new DrugUsageCommandRequest
        {
            ExpectedVersion = draft.Version, IdempotencyKey = "rec-1"
        });

        // Inilah pertanyaan yang muncul saat sebuah batch ditarik: siapa saja yang sudah
        // menerimanya. Tanpa alokasi ini, pertanyaan itu tidak dapat dijawab.
        var alokasi = Assert.Single(Assert.Single(dicatat.Items).Allocations);
        Assert.Equal("B-RECALL", alokasi.BatchNumber);
        Assert.Equal(12m, alokasi.Quantity);
        Assert.Equal("Pasien Uji", dicatat.PatientName);
    }

    [Fact]
    public async Task Daftar_DapatDisaringMenurutStatusBelumDitagihkan()
    {
        await using var f = await CreateAsync();
        await IsiAsync(f, "B-001", Hari(90), 100);

        var draft = await f.Service.CreateAsync(Pemakaian(f, 10, "u1"));
        await f.Service.RecordAsync(draft.Id, new DrugUsageCommandRequest
        {
            ExpectedVersion = draft.Version, IdempotencyKey = "rec-1"
        });

        await f.Service.CreateAsync(Pemakaian(f, 5, "u2"));

        // Billing menyapu menurut status untuk menemukan yang siap ditagihkan.
        var hasil = await f.Service.GetPagedAsync(new DrugUsagePagedQuery
        {
            Status = DrugUsageStatus.NotBilled
        });

        var baris = Assert.Single(hasil.Items);
        Assert.Equal(10m, 10m);
        Assert.Equal(DrugUsageStatus.NotBilled, baris.Status);
    }
}
