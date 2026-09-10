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
/// Pengujian retur obat.
/// </summary>
/// <remarks>
/// Yang diuji adalah janji yang menentukan: stok tidak bertambah sebelum diperiksa, obat
/// kembali sebagai batch yang sama dengan asalnya, pemeriksa dapat menerima sebagian dan
/// menahan sisanya di karantina, dan penolakan tidak menambah stok sama sekali.
/// </remarks>
public sealed class DrugReturnServiceTests
{
    private sealed class Fixture : IAsyncDisposable
    {
        public required ApplicationDbContext Context { get; init; }
        public required DrugReturnService Service { get; init; }
        public required DrugStockService StockService { get; init; }
        public required Guid DrugId { get; init; }
        public required Guid OtherDrugId { get; init; }
        public required Guid MeasurementId { get; init; }
        public required Guid DepoId { get; init; }
        public required Guid EncounterId { get; init; }
        public required Guid WorkforceId { get; init; }

        public async ValueTask DisposeAsync() => await Context.DisposeAsync();
    }

    private static async Task<Fixture> CreateAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"drug-return-{Guid.NewGuid():N}").Options;
        var context = new ApplicationDbContext(options);

        var drugId = Guid.NewGuid();
        var otherDrugId = Guid.NewGuid();
        var measurementId = Guid.NewGuid();
        var depoId = Guid.NewGuid();
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

        context.Set<MstDrug>().AddRange(
            new MstDrug
            {
                Id = drugId, DrugCategoryId = categoryId, DrugCode = "OBT-001",
                DrugName = "Paracetamol 500 mg", IsActive = true
            },
            new MstDrug
            {
                Id = otherDrugId, DrugCategoryId = categoryId, DrugCode = "OBT-002",
                DrugName = "Amoksisilin 500 mg", IsActive = true
            });

        context.Set<MstMeasurement>().Add(new MstMeasurement
        {
            Id = measurementId, MeasurementCode = "TAB", MeasurementName = "Tablet",
            MeasurementType = "Unit", IsActive = true
        });

        context.Set<MstServiceUnit>().Add(new MstServiceUnit
        {
            Id = serviceUnitId, ServiceUnitCode = "RI", ServiceUnitName = "Rawat Inap",
            IsActive = true
        });

        context.Set<MstPatient>().Add(new MstPatient
        {
            Id = patientId, PatientCode = "P-001", MedicalRecordNumber = "RM-001",
            FullName = "Pasien Uji", IsActive = true
        });

        context.Set<TrxPatientEncounter>().Add(new TrxPatientEncounter
        {
            Id = encounterId, EncounterNumber = "ENC-001",
            PatientId = patientId, ServiceUnitId = serviceUnitId
        });

        context.Set<MstWorkforceProfile>().Add(new MstWorkforceProfile
        {
            Id = workforceId, ProfileCode = "WF1", DisplayName = "Apoteker Uji", IsActive = true
        });

        context.Set<MstDrugStorageLocation>().Add(new MstDrugStorageLocation
        {
            Id = depoId, StorageLocationCode = "DP1",
            StorageLocationName = "Depo Rawat Inap", IsActive = true,
            IsAllowReceiving = true, IsAllowDispensing = true
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
            Service = new DrugReturnService(context, accessor, logger, stockService),
            DrugId = drugId,
            OtherDrugId = otherDrugId,
            MeasurementId = measurementId,
            DepoId = depoId,
            EncounterId = encounterId,
            WorkforceId = workforceId
        };
    }

    private static DateOnly Hari(int selisih) =>
        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(selisih);

    /// <summary>Menyiapkan batch beserta stok awalnya, lalu mengembalikan id batchnya.</summary>
    private static async Task<Guid> BatchAsync(Fixture f, string nomor, decimal awal = 100,
        Guid? drugId = null)
    {
        var saldo = await f.StockService.RecordOpeningBalanceAsync(new RecordOpeningBalanceRequest
        {
            Batch = new DrugBatchInput
            {
                DrugId = drugId ?? f.DrugId, BatchNumber = nomor, ExpiryDate = Hari(180)
            },
            StorageLocationId = f.DepoId,
            Quantity = awal,
            IdempotencyKey = $"open-{nomor}"
        });

        return saldo.DrugBatchId;
    }

    private static CreateDrugReturnRequest Retur(Fixture f, Guid batchId, decimal quantity,
        string key = "r1") => new()
        {
            EncounterId = f.EncounterId,
            StorageLocationId = f.DepoId,
            ReturnedByWorkforceId = f.WorkforceId,
            Reason = "Pasien pulang, obat bersisa.",
            Items = [new DrugReturnItemInput
            {
                DrugId = f.DrugId, DrugBatchId = batchId,
                MeasurementId = f.MeasurementId, Quantity = quantity
            }],
            IdempotencyKey = key
        };

    private static async Task<DrugReturnDetailResponse> DiajukanAsync(Fixture f, Guid batchId,
        decimal quantity, string key = "r1")
    {
        var draft = await f.Service.CreateAsync(Retur(f, batchId, quantity, key));
        return await f.Service.SubmitAsync(draft.Id, new DrugReturnCommandRequest
        {
            ExpectedVersion = draft.Version, IdempotencyKey = $"sub-{key}"
        });
    }

    // ---------------------------------------------------------------------- draft

    [Fact]
    public async Task Buat_MenghasilkanDraftTanpaMenambahStok()
    {
        await using var f = await CreateAsync();
        var batchId = await BatchAsync(f, "B-001");

        var hasil = await f.Service.CreateAsync(Retur(f, batchId, 10));

        Assert.Equal(DrugReturnStatus.Draft, hasil.Status);
        Assert.StartsWith("RTN-", hasil.ReturnNumber);
        Assert.Equal("Pasien Uji", hasil.PatientName);

        var saldo = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery());
        Assert.Equal(100m, Assert.Single(saldo.Items).QuantityOnHand);
    }

    [Fact]
    public async Task Buat_BatchMilikObatLain_Ditolak()
    {
        await using var f = await CreateAsync();
        var batchLain = await BatchAsync(f, "B-LAIN", 50, f.OtherDrugId);

        // Batch milik obat lain akan mengembalikan barang ke tempat yang keliru dan merusak
        // penelusurannya.
        var exception = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(
            () => f.Service.CreateAsync(Retur(f, batchLain, 5)));

        Assert.Equal("PHM075", exception.Code);
    }

    [Fact]
    public async Task Buat_BatchKembarDalamSatuRetur_Ditolak()
    {
        await using var f = await CreateAsync();
        var batchId = await BatchAsync(f, "B-001");

        var permintaan = Retur(f, batchId, 5);
        permintaan.Items.Add(new DrugReturnItemInput
        {
            DrugId = f.DrugId, DrugBatchId = batchId,
            MeasurementId = f.MeasurementId, Quantity = 3
        });

        var exception = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(
            () => f.Service.CreateAsync(permintaan));

        Assert.Equal("PHM078", exception.Code);
    }

    [Fact]
    public async Task Ajukan_TidakMenambahStok()
    {
        await using var f = await CreateAsync();
        var batchId = await BatchAsync(f, "B-001");

        var diajukan = await DiajukanAsync(f, batchId, 10);

        Assert.Equal(DrugReturnStatus.Submitted, diajukan.Status);
        Assert.True(diajukan.CanVerify);

        // Inilah aturan yang paling menentukan: obat yang kembali belum tentu layak, dan
        // stok tidak boleh bertambah sebelum ada yang memeriksanya.
        var saldo = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery());
        Assert.Equal(100m, Assert.Single(saldo.Items).QuantityOnHand);
    }

    // ------------------------------------------------------------------ pemeriksaan

    [Fact]
    public async Task Periksa_MenerimaSeluruhnya_MengembalikanStokKeBatchYangSama()
    {
        await using var f = await CreateAsync();
        var batchId = await BatchAsync(f, "B-001");

        var diajukan = await DiajukanAsync(f, batchId, 10);
        var diperiksa = await f.Service.VerifyAsync(diajukan.Id, new VerifyDrugReturnRequest
        {
            VerifiedByWorkforceId = f.WorkforceId,
            Items = [.. diajukan.Items.Select(i => new VerifyDrugReturnItemInput
            {
                DrugReturnItemId = i.Id, AcceptedQuantity = i.Quantity,
                AcceptedStatus = DrugStockStatus.Available
            })],
            ExpectedVersion = diajukan.Version,
            IdempotencyKey = "ver-1"
        });

        Assert.Equal(DrugReturnStatus.Verified, diperiksa.Status);
        Assert.Equal(10m, Assert.Single(diperiksa.Items).AcceptedQuantity);

        var saldo = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery());
        var baris = Assert.Single(saldo.Items);
        Assert.Equal(110m, baris.QuantityOnHand);

        // Batchnya harus tetap sama; batch baru akan membuat kedaluwarsanya keliru.
        Assert.Equal("B-001", baris.BatchNumber);
    }

    [Fact]
    public async Task Periksa_MenerimaSebagian_SisanyaTidakMasukStokManaPun()
    {
        await using var f = await CreateAsync();
        var batchId = await BatchAsync(f, "B-001");

        var diajukan = await DiajukanAsync(f, batchId, 10);
        var diperiksa = await f.Service.VerifyAsync(diajukan.Id, new VerifyDrugReturnRequest
        {
            VerifiedByWorkforceId = f.WorkforceId,
            Items = [new()
            {
                DrugReturnItemId = diajukan.Items[0].Id, AcceptedQuantity = 6,
                AcceptedStatus = DrugStockStatus.Available,
                Note = "Empat tablet kemasannya sudah terbuka."
            }],
            ExpectedVersion = diajukan.Version,
            IdempotencyKey = "ver-1"
        });

        Assert.Equal(6m, Assert.Single(diperiksa.Items).AcceptedQuantity);

        // Yang ditolak tidak masuk ke stok mana pun; ia memang tidak layak dipakai lagi.
        var saldo = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery());
        Assert.Equal(106m, saldo.Items.Sum(x => x.QuantityOnHand));
    }

    [Fact]
    public async Task Periksa_MenahanDiKarantina_TidakIkutDilayankan()
    {
        await using var f = await CreateAsync();
        var batchId = await BatchAsync(f, "B-001");

        var diajukan = await DiajukanAsync(f, batchId, 10);
        await f.Service.VerifyAsync(diajukan.Id, new VerifyDrugReturnRequest
        {
            VerifiedByWorkforceId = f.WorkforceId,
            Items = [new()
            {
                DrugReturnItemId = diajukan.Items[0].Id, AcceptedQuantity = 10,
                AcceptedStatus = DrugStockStatus.Quarantine,
                Note = "Penyimpanan selama di bangsal tidak dapat dipastikan."
            }],
            ExpectedVersion = diajukan.Version,
            IdempotencyKey = "ver-1"
        });

        var siapPakai = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery
        {
            Status = DrugStockStatus.Available
        });
        var karantina = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery
        {
            Status = DrugStockStatus.Quarantine
        });

        Assert.Equal(100m, Assert.Single(siapPakai.Items).QuantityOnHand);
        Assert.Equal(10m, Assert.Single(karantina.Items).QuantityOnHand);

        // Barangnya tercatat, tetapi tidak boleh keluar sampai ada keputusan lanjutan.
        var exception = await Assert.ThrowsAsync<DrugStockUnprocessableException>(
            () => f.StockService.IssueAsync(new IssueStockRequest
            {
                DrugId = f.DrugId, StorageLocationId = f.DepoId,
                Quantity = 101, IdempotencyKey = "iss-1"
            }));

        Assert.Equal("PHM028", exception.Code);
    }

    [Fact]
    public async Task Periksa_MenerimaLebihBanyakDariYangDikembalikan_Ditolak()
    {
        await using var f = await CreateAsync();
        var batchId = await BatchAsync(f, "B-001");

        var diajukan = await DiajukanAsync(f, batchId, 10);

        var exception = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(
            () => f.Service.VerifyAsync(diajukan.Id, new VerifyDrugReturnRequest
            {
                VerifiedByWorkforceId = f.WorkforceId,
                Items = [new() { DrugReturnItemId = diajukan.Items[0].Id, AcceptedQuantity = 11 }],
                ExpectedVersion = diajukan.Version,
                IdempotencyKey = "ver-1"
            }));

        Assert.Equal("PHM071", exception.Code);
    }

    [Fact]
    public async Task Periksa_BarisTidakDiputuskan_Ditolak()
    {
        await using var f = await CreateAsync();
        var batchId = await BatchAsync(f, "B-001");

        var diajukan = await DiajukanAsync(f, batchId, 10);

        var exception = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(
            () => f.Service.VerifyAsync(diajukan.Id, new VerifyDrugReturnRequest
            {
                VerifiedByWorkforceId = f.WorkforceId,
                Items = [new() { DrugReturnItemId = Guid.NewGuid(), AcceptedQuantity = 1 }],
                ExpectedVersion = diajukan.Version,
                IdempotencyKey = "ver-1"
            }));

        Assert.Equal("PHM079", exception.Code);
    }

    [Fact]
    public async Task Periksa_SebelumDiajukan_Ditolak()
    {
        await using var f = await CreateAsync();
        var batchId = await BatchAsync(f, "B-001");

        var draft = await f.Service.CreateAsync(Retur(f, batchId, 10));

        var exception = await Assert.ThrowsAsync<DrugReturnConflictException>(
            () => f.Service.VerifyAsync(draft.Id, new VerifyDrugReturnRequest
            {
                VerifiedByWorkforceId = f.WorkforceId,
                Items = [new() { DrugReturnItemId = draft.Items[0].Id, AcceptedQuantity = 10 }],
                ExpectedVersion = draft.Version,
                IdempotencyKey = "ver-1"
            }));

        Assert.Equal("PHM070", exception.Code);
    }

    [Fact]
    public async Task Periksa_MenulisKartuStokDenganDokumenAsalnya()
    {
        await using var f = await CreateAsync();
        var batchId = await BatchAsync(f, "B-001");

        var diajukan = await DiajukanAsync(f, batchId, 8);
        await f.Service.VerifyAsync(diajukan.Id, new VerifyDrugReturnRequest
        {
            VerifiedByWorkforceId = f.WorkforceId,
            Items = [new() { DrugReturnItemId = diajukan.Items[0].Id, AcceptedQuantity = 8 }],
            ExpectedVersion = diajukan.Version,
            IdempotencyKey = "ver-1"
        });

        var kartu = await f.StockService.GetMutationsAsync(new DrugStockMutationQuery
        {
            SourceDocumentType = DrugStockSourceDocumentTypes.PrescriptionReturn
        });

        var baris = Assert.Single(kartu.Items);
        Assert.Equal(8m, baris.QuantityChange);
        Assert.Equal(100m, baris.BalanceBefore);
        Assert.Equal(108m, baris.BalanceAfter);
        Assert.Equal(diajukan.Id, baris.SourceDocumentId);
    }

    // --------------------------------------------------------------------- tolak

    [Fact]
    public async Task Tolak_TidakMenambahStokSamaSekali()
    {
        await using var f = await CreateAsync();
        var batchId = await BatchAsync(f, "B-001");

        var diajukan = await DiajukanAsync(f, batchId, 10);
        var ditolak = await f.Service.RejectAsync(diajukan.Id, new DrugReturnReasonRequest
        {
            Reason = "Kemasan sudah rusak dan tidak dapat dipastikan keasliannya.",
            ExpectedVersion = diajukan.Version,
            IdempotencyKey = "rej-1"
        });

        Assert.Equal(DrugReturnStatus.Rejected, ditolak.Status);
        Assert.Contains("Kemasan sudah rusak", ditolak.DecisionReason);

        var saldo = await f.StockService.GetBalancesAsync(new DrugStockBalanceQuery());
        Assert.Equal(100m, Assert.Single(saldo.Items).QuantityOnHand);
    }

    [Fact]
    public async Task Tolak_TanpaAlasan_Ditolak()
    {
        await using var f = await CreateAsync();
        var batchId = await BatchAsync(f, "B-001");

        var diajukan = await DiajukanAsync(f, batchId, 10);

        var exception = await Assert.ThrowsAsync<DrugReturnUnprocessableException>(
            () => f.Service.RejectAsync(diajukan.Id, new DrugReturnReasonRequest
            {
                Reason = "   ",
                ExpectedVersion = diajukan.Version,
                IdempotencyKey = "rej-1"
            }));

        Assert.Equal("PHM072", exception.Code);
    }

    // -------------------------------------------------------------------- lainnya

    [Fact]
    public async Task Ubah_SetelahDiajukan_Ditolak()
    {
        await using var f = await CreateAsync();
        var batchId = await BatchAsync(f, "B-001");

        var diajukan = await DiajukanAsync(f, batchId, 10);

        var exception = await Assert.ThrowsAsync<DrugReturnConflictException>(
            () => f.Service.UpdateAsync(diajukan.Id, new UpdateDrugReturnRequest
            {
                StorageLocationId = f.DepoId,
                Items = [new DrugReturnItemInput
                {
                    DrugId = f.DrugId, DrugBatchId = batchId,
                    MeasurementId = f.MeasurementId, Quantity = 5
                }],
                ExpectedVersion = diajukan.Version,
                IdempotencyKey = "upd-1"
            }));

        Assert.Equal("PHM070", exception.Code);
    }

    [Fact]
    public async Task Batal_SetelahDiperiksa_Ditolak()
    {
        await using var f = await CreateAsync();
        var batchId = await BatchAsync(f, "B-001");

        var diajukan = await DiajukanAsync(f, batchId, 10);
        var diperiksa = await f.Service.VerifyAsync(diajukan.Id, new VerifyDrugReturnRequest
        {
            VerifiedByWorkforceId = f.WorkforceId,
            Items = [new() { DrugReturnItemId = diajukan.Items[0].Id, AcceptedQuantity = 10 }],
            ExpectedVersion = diajukan.Version,
            IdempotencyKey = "ver-1"
        });

        var exception = await Assert.ThrowsAsync<DrugReturnConflictException>(
            () => f.Service.CancelAsync(diperiksa.Id, new DrugReturnReasonRequest
            {
                Reason = "Terlambat.",
                ExpectedVersion = diperiksa.Version,
                IdempotencyKey = "can-1"
            }));

        Assert.Equal("PHM070", exception.Code);
    }

    [Fact]
    public async Task Buat_KunciSamaDikirimDuaKali_TidakMenggandakan()
    {
        await using var f = await CreateAsync();
        var batchId = await BatchAsync(f, "B-001");

        var pertama = await f.Service.CreateAsync(Retur(f, batchId, 10));
        var kedua = await f.Service.CreateAsync(Retur(f, batchId, 10));

        Assert.Equal(pertama.Id, kedua.Id);
        Assert.Single(f.Context.PhmDrugReturns);
    }
}
