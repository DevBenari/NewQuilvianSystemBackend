using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.HealthServices.NutritionManagement;

namespace QuilvianSystemBackend.Tests.HealthServices.PharmacyManagement;

/// <summary>
/// Pengujian penyerahan obat resep per baris.
/// </summary>
/// <remarks>
/// Yang diuji adalah janji yang menentukan bagi copy resep: penyerahan bertahap menghasilkan
/// sisa yang benar, penanda <c>det</c>/<c>nedet</c> mengikuti sisa itu, stok berkurang tepat
/// satu kali, tahanan dilepas ketika penyiapan dibatalkan, dan sisa tidak pernah negatif.
/// </remarks>
public sealed class PrescriptionDispensingServiceTests
{
    private sealed class Fixture : IAsyncDisposable
    {
        public required ApplicationDbContext Context { get; init; }
        public required PrescriptionDispensingService Service { get; init; }
        public required DrugStockService StockService { get; init; }
        public required Guid DrugId { get; init; }
        public required Guid MeasurementId { get; init; }
        public required Guid DepoId { get; init; }
        public required Guid PrescriptionId { get; init; }
        public required Guid ItemId { get; init; }
        public required Guid WorkforceId { get; init; }

        public async ValueTask DisposeAsync() => await Context.DisposeAsync();
    }

    private static async Task<Fixture> CreateAsync(
        decimal prescribed = 10m,
        PrescriptionFulfillmentStatus fulfillment = PrescriptionFulfillmentStatus.ReadyToDispense,
        PrescriptionPaymentStatus payment = PrescriptionPaymentStatus.Paid)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"prescription-dispensing-{Guid.NewGuid():N}").Options;
        var context = new ApplicationDbContext(options);

        var drugId = Guid.NewGuid();
        var measurementId = Guid.NewGuid();
        var depoId = Guid.NewGuid();
        var encounterId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var serviceUnitId = Guid.NewGuid();
        var workforceId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var prescriptionId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        context.Set<MstDrugCategory>().Add(new MstDrugCategory
        {
            Id = categoryId, DrugCategoryCode = "K1", DrugCategoryName = "Umum",
            DrugCategoryType = "General", IsActive = true
        });

        context.Set<MstDrug>().Add(new MstDrug
        {
            Id = drugId, DrugCategoryId = categoryId, DrugCode = "OBT-001",
            DrugName = "Amoxicillin 500 mg", IsActive = true
        });

        context.Set<MstMeasurement>().Add(new MstMeasurement
        {
            Id = measurementId, MeasurementCode = "TAB", MeasurementName = "Tablet",
            MeasurementType = "Unit", IsActive = true
        });

        context.Set<MstServiceUnit>().Add(new MstServiceUnit
        {
            Id = serviceUnitId, ServiceUnitCode = "RJ", ServiceUnitName = "Rawat Jalan",
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
            Id = depoId, StorageLocationCode = "DP1", StorageLocationName = "Depo Rawat Jalan",
            IsActive = true, IsAllowDispensing = true
        });

        context.Set<TrxPrescription>().Add(new TrxPrescription
        {
            Id = prescriptionId,
            PrescriptionNumber = "RX-001",
            EncounterId = encounterId,
            PatientId = patientId,
            DoctorId = doctorId,
            PrescriptionDateTime = DateTime.UtcNow,
            PrescriptionStatus = PrescriptionStatus.Submitted,
            FulfillmentStatus = fulfillment,
            PaymentStatus = payment
        });

        context.Set<TrxPrescriptionItem>().Add(new TrxPrescriptionItem
        {
            Id = itemId,
            PrescriptionId = prescriptionId,
            DrugId = drugId,
            DrugCodeSnapshot = "OBT-001",
            DrugNameSnapshot = "Amoxicillin 500 mg",
            DispenseUnitMeasurementId = measurementId,
            DispenseUnitNameSnapshot = "Tablet",
            Quantity = prescribed,
            SortOrder = 1
        });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var accessor = new MutableHttpContextAccessor();
        accessor.SetUser(Guid.NewGuid());
        var logger = new LoggerService(NullLogger<LoggerService>.Instance, accessor);
        var stockService = new DrugStockService(context, accessor, logger);

        return new Fixture
        {
            Context = context,
            StockService = stockService,
            Service = new PrescriptionDispensingService(context, accessor, logger, stockService),
            DrugId = drugId,
            MeasurementId = measurementId,
            DepoId = depoId,
            PrescriptionId = prescriptionId,
            ItemId = itemId,
            WorkforceId = workforceId
        };
    }

    private static Task<DrugStockBalanceResponse> IsiAsync(Fixture f, string batch,
        DateOnly expiry, decimal quantity) =>
        f.StockService.RecordOpeningBalanceAsync(new RecordOpeningBalanceRequest
        {
            Batch = new DrugBatchInput
            {
                DrugId = f.DrugId, BatchNumber = batch, ExpiryDate = expiry
            },
            StorageLocationId = f.DepoId,
            Quantity = quantity,
            IdempotencyKey = $"open-{batch}"
        });

    private static DateOnly Hari(int selisih) =>
        DateOnly.FromDateTime(DateTime.UtcNow).AddDays(selisih);

    private static PreparePrescriptionDispensingRequest Siapkan(Fixture f, decimal quantity,
        string key) => new()
        {
            StorageLocationId = f.DepoId,
            PreparedByWorkforceId = f.WorkforceId,
            Items = [new PrescriptionDispensingItemInput
            {
                PrescriptionItemId = f.ItemId, Quantity = quantity
            }],
            IdempotencyKey = key
        };

    private static async Task<Guid> UsageIdAsync(Fixture f) =>
        (await f.Context.TrxDrugUsages.AsNoTracking()
            .Where(x => x.Status == DrugUsageStatus.Draft)
            .OrderByDescending(x => x.CreateDateTime)
            .Select(x => x.Id)
            .FirstAsync());

    private static Task<decimal> OnHandAsync(Fixture f) =>
        f.Context.TrxDrugStockBalances.AsNoTracking().SumAsync(x => x.QuantityOnHand);

    private static Task<decimal> ReservedAsync(Fixture f) =>
        f.Context.TrxDrugStockBalances.AsNoTracking().SumAsync(x => x.QuantityReserved);

    // ------------------------------------------------------------- penyerahan penuh

    /// <summary>
    /// Penyerahan penuh: sisa nol, penandanya `det`, dan stok berkurang sebesar itu.
    /// </summary>
    [Fact]
    public async Task PenyerahanPenuh_SisaNolDanDitandaiDet()
    {
        await using var f = await CreateAsync(prescribed: 10m);
        await IsiAsync(f, "B-001", Hari(90), 100m);

        await f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 10m, "p1"));
        var usageId = await UsageIdAsync(f);

        // Menyiapkan belum memindahkan barang; ia baru menahannya.
        Assert.Equal(100m, await OnHandAsync(f));
        Assert.Equal(10m, await ReservedAsync(f));

        var hasil = await f.Service.DispenseAsync(f.PrescriptionId, usageId,
            new PrescriptionDispensingCommandRequest { ExpectedVersion = 0, IdempotencyKey = "d1" });

        var baris = Assert.Single(hasil.Items);
        Assert.Equal(10m, baris.QuantityPrescribed);
        Assert.Equal(10m, baris.QuantityDispensed);
        Assert.Equal(0m, baris.QuantityRemaining);
        Assert.Equal(PrescriptionItemDispensingMark.Det, baris.Mark);
        Assert.True(hasil.IsFullyDispensed);
        Assert.Equal(PrescriptionFulfillmentStatus.Dispensed, hasil.FulfillmentStatus);

        // Saldo berkurang, dan tahanannya ikut terlepas — bukan tertinggal menahan stok.
        Assert.Equal(90m, await OnHandAsync(f));
        Assert.Equal(0m, await ReservedAsync(f));
    }

    // --------------------------------------------------------- penyerahan bertahap

    /// <summary>
    /// Penyerahan bertahap: sisa berkurang bertahap, penandanya `nedet` selama masih ada sisa.
    /// </summary>
    [Fact]
    public async Task PenyerahanBertahap_SisaBerkurangDanDitandaiNedet()
    {
        await using var f = await CreateAsync(prescribed: 10m);
        await IsiAsync(f, "B-001", Hari(90), 100m);

        await f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 4m, "p1"));
        var pertama = await UsageIdAsync(f);
        var setelahPertama = await f.Service.DispenseAsync(f.PrescriptionId, pertama,
            new PrescriptionDispensingCommandRequest { ExpectedVersion = 0, IdempotencyKey = "d1" });

        var barisPertama = Assert.Single(setelahPertama.Items);
        Assert.Equal(4m, barisPertama.QuantityDispensed);
        Assert.Equal(6m, barisPertama.QuantityRemaining);
        Assert.Equal(PrescriptionItemDispensingMark.Nedet, barisPertama.Mark);
        Assert.False(setelahPertama.IsFullyDispensed);
        Assert.Equal(PrescriptionFulfillmentStatus.PartiallyDispensed,
            setelahPertama.FulfillmentStatus);

        await f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 6m, "p2"));
        var kedua = await UsageIdAsync(f);
        var setelahKedua = await f.Service.DispenseAsync(f.PrescriptionId, kedua,
            new PrescriptionDispensingCommandRequest { ExpectedVersion = 0, IdempotencyKey = "d2" });

        var barisKedua = Assert.Single(setelahKedua.Items);
        Assert.Equal(10m, barisKedua.QuantityDispensed);
        Assert.Equal(0m, barisKedua.QuantityRemaining);
        Assert.Equal(PrescriptionItemDispensingMark.Det, barisKedua.Mark);
        Assert.Equal(PrescriptionFulfillmentStatus.Dispensed, setelahKedua.FulfillmentStatus);

        Assert.Equal(90m, await OnHandAsync(f));
        Assert.Equal(2, setelahKedua.History.Count);
    }

    /// <summary>
    /// Menyiapkan melebihi sisa ditolak — kalau lolos, penyerahan akan melampaui resepnya.
    /// </summary>
    [Fact]
    public async Task Menyiapkan_MelebihiSisa_Ditolak()
    {
        await using var f = await CreateAsync(prescribed: 10m);
        await IsiAsync(f, "B-001", Hari(90), 100m);

        await f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 7m, "p1"));
        var usageId = await UsageIdAsync(f);
        await f.Service.DispenseAsync(f.PrescriptionId, usageId,
            new PrescriptionDispensingCommandRequest { ExpectedVersion = 0, IdempotencyKey = "d1" });

        var exception = await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(
            () => f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 4m, "p2")));

        Assert.Equal("PHM104", exception.Code);
    }

    /// <summary>
    /// Dua penyiapan berturut-turut tidak boleh sama-sama menahan sisa yang sama.
    /// </summary>
    [Fact]
    public async Task DuaPenyiapan_TidakBolehMenahanSisaYangSama()
    {
        await using var f = await CreateAsync(prescribed: 10m);
        await IsiAsync(f, "B-001", Hari(90), 100m);

        await f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 6m, "p1"));

        var exception = await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(
            () => f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 6m, "p2")));

        Assert.Equal("PHM104", exception.Code);
        Assert.Equal(6m, await ReservedAsync(f));
    }

    // ------------------------------------------------------------------ pembatalan

    /// <summary>
    /// Membatalkan penyiapan melepas tahanan stok, dan sisanya kembali utuh.
    /// </summary>
    [Fact]
    public async Task Pembatalan_MelepasTahananDanMengembalikanSisa()
    {
        await using var f = await CreateAsync(prescribed: 10m);
        await IsiAsync(f, "B-001", Hari(90), 100m);

        await f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 10m, "p1"));
        var usageId = await UsageIdAsync(f);
        Assert.Equal(10m, await ReservedAsync(f));

        var hasil = await f.Service.CancelAsync(f.PrescriptionId, usageId,
            new CancelPrescriptionDispensingRequest
            {
                Reason = "Pasien menunda pengambilan.",
                ExpectedVersion = 0,
                IdempotencyKey = "c1"
            });

        Assert.Equal(0m, await ReservedAsync(f));
        Assert.Equal(100m, await OnHandAsync(f));

        var baris = Assert.Single(hasil.Items);
        Assert.Equal(0m, baris.QuantityDispensed);
        Assert.Equal(10m, baris.QuantityRemaining);
        Assert.Equal(0m, baris.QuantityReserved);
    }

    /// <summary>
    /// Obat yang sudah diserahkan tidak dapat dibatalkan; jalannya retur, bukan pembatalan.
    /// </summary>
    [Fact]
    public async Task Pembatalan_SetelahDiserahkan_Ditolak()
    {
        await using var f = await CreateAsync(prescribed: 10m);
        await IsiAsync(f, "B-001", Hari(90), 100m);

        await f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 10m, "p1"));
        var usageId = await UsageIdAsync(f);
        await f.Service.DispenseAsync(f.PrescriptionId, usageId,
            new PrescriptionDispensingCommandRequest { ExpectedVersion = 0, IdempotencyKey = "d1" });

        var exception = await Assert.ThrowsAsync<PrescriptionDispensingConflictException>(
            () => f.Service.CancelAsync(f.PrescriptionId, usageId,
                new CancelPrescriptionDispensingRequest
                {
                    Reason = "Salah serah.", ExpectedVersion = 1, IdempotencyKey = "c1"
                }));

        Assert.Equal("PHM107", exception.Code);
    }

    // -------------------------------------------------------------- stok tak cukup

    /// <summary>
    /// Stok yang kurang menggagalkan penyiapan, dan tidak menahan apa pun.
    /// </summary>
    [Fact]
    public async Task StokKurang_MenggagalkanPenyiapanTanpaMenahanStok()
    {
        await using var f = await CreateAsync(prescribed: 10m);
        await IsiAsync(f, "B-001", Hari(90), 3m);

        await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(
            () => f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 10m, "p1")));

        Assert.Equal(3m, await OnHandAsync(f));
        Assert.Equal(0m, await ReservedAsync(f));
    }

    // ------------------------------------------------------------------ idempotency

    /// <summary>
    /// Penyiapan dengan kunci yang sama tidak menahan stok dua kali.
    /// </summary>
    [Fact]
    public async Task PenyiapanDiulang_TidakMenahanStokDuaKali()
    {
        await using var f = await CreateAsync(prescribed: 10m);
        await IsiAsync(f, "B-001", Hari(90), 100m);

        var permintaan = Siapkan(f, 10m, "p1");
        await f.Service.PrepareAsync(f.PrescriptionId, permintaan);
        await f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 10m, "p1"));

        Assert.Equal(10m, await ReservedAsync(f));
        Assert.Equal(1, await f.Context.TrxDrugUsages.CountAsync());
    }

    /// <summary>
    /// Penyerahan yang diulang tidak memotong stok dua kali.
    /// </summary>
    [Fact]
    public async Task PenyerahanDiulang_TidakMemotongStokDuaKali()
    {
        await using var f = await CreateAsync(prescribed: 10m);
        await IsiAsync(f, "B-001", Hari(90), 100m);

        await f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 10m, "p1"));
        var usageId = await UsageIdAsync(f);

        await f.Service.DispenseAsync(f.PrescriptionId, usageId,
            new PrescriptionDispensingCommandRequest { ExpectedVersion = 0, IdempotencyKey = "d1" });
        var kedua = await f.Service.DispenseAsync(f.PrescriptionId, usageId,
            new PrescriptionDispensingCommandRequest { ExpectedVersion = 0, IdempotencyKey = "d1" });

        Assert.Equal(90m, await OnHandAsync(f));
        Assert.Equal(10m, Assert.Single(kedua.Items).QuantityDispensed);

        // Hanya satu baris pengeluaran. Saldo pembuka juga menulis kartu stok, jadi yang
        // dihitung khusus yang keluar — bukan seluruh mutasi.
        Assert.Equal(1, await f.Context.TrxDrugStockMutations
            .CountAsync(x => x.MutationType == DrugStockMutationType.StockOut));
    }

    // ------------------------------------------------------------------ gerbang

    /// <summary>
    /// Obat tidak boleh diserahkan sebelum pembayaran atau penjaminan memenuhi syarat.
    /// </summary>
    [Fact]
    public async Task BelumLunas_TidakBolehDisiapkan()
    {
        await using var f = await CreateAsync(
            payment: PrescriptionPaymentStatus.WaitingForPayment);
        await IsiAsync(f, "B-001", Hari(90), 100m);

        var exception = await Assert.ThrowsAsync<PrescriptionDispensingConflictException>(
            () => f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 10m, "p1")));

        Assert.Equal("PHM111", exception.Code);
        Assert.Equal(0m, await ReservedAsync(f));
    }

    /// <summary>
    /// Penjaminan yang disetujui sah sebagai dasar penyerahan meski tidak ada uang berpindah.
    /// </summary>
    [Fact]
    public async Task DijaminAsuransi_BolehDiserahkan()
    {
        await using var f = await CreateAsync(
            payment: PrescriptionPaymentStatus.InsuranceApproved);
        await IsiAsync(f, "B-001", Hari(90), 100m);

        await f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 10m, "p1"));

        Assert.Equal(10m, await ReservedAsync(f));
    }

    /// <summary>
    /// Resep yang penyiapannya belum selesai belum boleh diserahkan.
    /// </summary>
    [Fact]
    public async Task BelumSiapDiserahkan_Ditolak()
    {
        await using var f = await CreateAsync(
            fulfillment: PrescriptionFulfillmentStatus.InPreparation);
        await IsiAsync(f, "B-001", Hari(90), 100m);

        var exception = await Assert.ThrowsAsync<PrescriptionDispensingConflictException>(
            () => f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 10m, "p1")));

        Assert.Equal("PHM110", exception.Code);
    }

    // ----------------------------------------------------------------- copy resep

    /// <summary>
    /// FEFO tetap berlaku: batch yang lebih dahulu kedaluwarsa keluar lebih dahulu, dan
    /// batch yang terpakai tercatat pada histori penyerahan.
    /// </summary>
    [Fact]
    public async Task Penyerahan_MengambilBatchKedaluwarsaTerdekatDanMencatatnya()
    {
        await using var f = await CreateAsync(prescribed: 10m);
        await IsiAsync(f, "B-LAMBAT", Hari(365), 100m);
        await IsiAsync(f, "B-CEPAT", Hari(30), 4m);

        await f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 10m, "p1"));
        var usageId = await UsageIdAsync(f);
        var hasil = await f.Service.DispenseAsync(f.PrescriptionId, usageId,
            new PrescriptionDispensingCommandRequest { ExpectedVersion = 0, IdempotencyKey = "d1" });

        var cepat = await f.Context.TrxDrugStockBalances.AsNoTracking()
            .SingleAsync(x => x.DrugBatch!.BatchNumber == "B-CEPAT");
        Assert.Equal(0m, cepat.QuantityOnHand);

        var alokasi = Assert.Single(hasil.History).Items.Single().Allocations;
        Assert.Equal(2, alokasi.Count);
        Assert.Equal("B-CEPAT", alokasi[0].BatchNumber);
        Assert.Equal(4m, alokasi[0].Quantity);
        Assert.Equal("B-LAMBAT", alokasi[1].BatchNumber);
        Assert.Equal(6m, alokasi[1].Quantity);
    }

    /// <summary>
    /// Resep yang belum pernah diserahkan sama sekali: seluruh barisnya `nedet`.
    /// </summary>
    [Fact]
    public async Task BelumAdaPenyerahan_SeluruhBarisNedet()
    {
        await using var f = await CreateAsync(prescribed: 10m);

        var hasil = await f.Service.GetSummaryAsync(f.PrescriptionId);

        Assert.NotNull(hasil);
        var baris = Assert.Single(hasil!.Items);
        Assert.Equal(0m, baris.QuantityDispensed);
        Assert.Equal(10m, baris.QuantityRemaining);
        Assert.Equal(PrescriptionItemDispensingMark.Nedet, baris.Mark);
        Assert.False(hasil.IsFullyDispensed);
        Assert.Empty(hasil.History);
    }

    /// <summary>
    /// Penyiapan milik resep lain tidak boleh diserahkan lewat resep ini.
    /// </summary>
    [Fact]
    public async Task Penyerahan_MilikResepLain_Ditolak()
    {
        await using var f = await CreateAsync(prescribed: 10m);
        await IsiAsync(f, "B-001", Hari(90), 100m);

        await f.Service.PrepareAsync(f.PrescriptionId, Siapkan(f, 10m, "p1"));
        var usageId = await UsageIdAsync(f);

        // Resep kedua yang benar-benar ada, supaya yang diuji adalah pemeriksaan kepemilikan
        // — bukan sekadar resep yang tidak ditemukan.
        var resepLain = Guid.NewGuid();
        f.Context.Set<TrxPrescription>().Add(new TrxPrescription
        {
            Id = resepLain,
            PrescriptionNumber = "RX-002",
            EncounterId = (await f.Context.TrxPrescriptions.AsNoTracking()
                .Where(x => x.Id == f.PrescriptionId).Select(x => x.EncounterId).FirstAsync()),
            PatientId = Guid.NewGuid(),
            DoctorId = Guid.NewGuid(),
            PrescriptionDateTime = DateTime.UtcNow,
            PrescriptionStatus = PrescriptionStatus.Submitted,
            FulfillmentStatus = PrescriptionFulfillmentStatus.ReadyToDispense,
            PaymentStatus = PrescriptionPaymentStatus.Paid
        });
        await f.Context.SaveChangesAsync();
        f.Context.ChangeTracker.Clear();

        var exception = await Assert.ThrowsAsync<PrescriptionDispensingUnprocessableException>(
            () => f.Service.DispenseAsync(resepLain, usageId,
                new PrescriptionDispensingCommandRequest
                {
                    ExpectedVersion = 0, IdempotencyKey = "d1"
                }));

        Assert.Equal("PHM108", exception.Code);
    }
}
