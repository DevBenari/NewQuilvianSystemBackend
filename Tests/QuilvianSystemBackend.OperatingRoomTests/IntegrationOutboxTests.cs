using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Tests.OperatingRoom.Infrastructure;

namespace QuilvianSystemBackend.Tests.OperatingRoom;

/// <summary>
/// Outbox kejadian modul Operasi (`BE-OPR-009`, `OPR-INT-001`/`OPR-INT-002`).
/// </summary>
/// <remarks>
/// <para>
/// Yang dibuktikan di sini adalah sisi Operasi: setiap kejadian bisnis meninggalkan tepat satu
/// pesan, isinya lengkap dan beku, pengulangan tidak menggandakannya, kejadian berbeda
/// menghasilkan pesan berbeda, dan kegagalan pengiriman tidak merusak apa pun yang sudah
/// terjadi pada pasien.
/// </para>
/// <para>
/// Bentuk pesan yang diterima Billing, Inventory, atau SATUSEHAT tidak diuji dan memang bukan
/// urusan modul ini. Penerjemahannya milik integration layer. Modul Operasi tidak memanggil
/// consumer mana pun dan tidak bergantung pada ketersediaan mereka.
/// </para>
/// </remarks>
public class IntegrationOutboxTests
{
    private static CreateOprMaterialUsageRequest Pemakaian(OperatingRoomSeed seed, string kunci,
        decimal jumlah = 2) => new()
        {
            ExternalItemId = seed.Hasil.MaterialItemIds[0],
            ItemType = OprMaterialItemType.Consumable,
            Quantity = jumlah,
            UnitMeasurementId = seed.Hasil.MeasurementId,
            UnitCode = "UJI-PCS",
            Outcome = OprMaterialOutcome.Used,
            BatchNumber = "UJI-BATCH-01",
            IdempotencyKey = kunci
        };

    [Fact]
    public async Task Satu_kejadian_bisnis_meninggalkan_tepat_satu_pesan()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        await harness.MaterialService(OperatingRoomSeed.AkunBedah)
            .RecordAsync(kasusId, Pemakaian(seed, alur.Kunci("INT")));

        await using var db = harness.NewContext();
        var pesan = await db.OprIntegrationDeliveries.AsNoTracking()
            .Where(x => x.OprCaseId == kasusId && !x.IsDelete).ToListAsync();

        Assert.Single(pesan);
        Assert.Equal(OprDeliveryStatus.Pending, pesan[0].Status);
        Assert.Equal(OperatingRoomIntegrationService.MaterialEventType, pesan[0].EventType);
        Assert.Equal(OperatingRoomIntegrationService.EventVersion, pesan[0].EventVersion);
    }

    [Fact]
    public async Task Amplop_kejadian_memuat_seluruh_bidang_wajib()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        await harness.MaterialService(OperatingRoomSeed.AkunBedah)
            .RecordAsync(kasusId, Pemakaian(seed, alur.Kunci("INT")));

        await using var db = harness.NewContext();
        var baris = await db.OprIntegrationDeliveries.AsNoTracking()
            .FirstAsync(x => x.OprCaseId == kasusId && !x.IsDelete);
        var kasus = await db.OprCases.AsNoTracking().FirstAsync(x => x.Id == kasusId);

        var peristiwa = await harness.IntegrationService(OperatingRoomSeed.AkunBedah)
            .GetEventAsync(kasusId, baris.Id);

        Assert.NotNull(peristiwa);
        Assert.NotEqual(Guid.Empty, peristiwa!.EventId);
        Assert.Equal(baris.EventId, peristiwa.EventId);
        Assert.Equal(OperatingRoomIntegrationService.MaterialEventType, peristiwa.EventType);
        Assert.Equal(OperatingRoomIntegrationService.EventVersion, peristiwa.EventVersion);
        Assert.NotEqual(default, peristiwa.OccurredAt);

        Assert.Equal(kasusId, peristiwa.CaseId);
        Assert.Equal(kasus.CaseNumber, peristiwa.CaseNumber);
        Assert.Equal(seed.PasienId, peristiwa.PatientId);
        Assert.Equal(seed.KunjunganId, peristiwa.EncounterId);
        Assert.NotNull(peristiwa.ServiceRequestId);

        // Tindakan dan pelaksana ikut dibekukan, bukan dibiarkan sebagai rujukan.
        Assert.NotEmpty(peristiwa.Procedures);
        Assert.All(peristiwa.Procedures, x => Assert.NotEqual(Guid.Empty, x.PatientProcedureId));
        Assert.Contains(peristiwa.Procedures, x => x.IsPrimary);
        Assert.All(peristiwa.Procedures, x => Assert.False(string.IsNullOrWhiteSpace(x.ProcedureCode)));

        Assert.Equal(4, peristiwa.Performers.Count);
        Assert.Contains(peristiwa.Performers, x => x.Role == nameof(OprTeamRole.PrimarySurgeon) && x.IsLead);
        Assert.Contains(peristiwa.Performers, x => x.Role == nameof(OprTeamRole.Anesthesiologist));

        Assert.NotNull(peristiwa.Material);
        Assert.Equal(seed.Hasil.MaterialItemIds[0], peristiwa.Material!.ExternalItemId);
        Assert.Equal(2, peristiwa.Material.Quantity);
        Assert.Equal(nameof(OprMaterialOutcome.Used), peristiwa.Material.Outcome);
        Assert.NotEqual(Guid.Empty, peristiwa.Material.RecordedBy);
    }

    [Fact]
    public async Task Pengulangan_permintaan_tidak_melahirkan_pesan_kedua()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        var kunci = alur.Kunci("INTULANG");
        await harness.MaterialService(OperatingRoomSeed.AkunBedah).RecordAsync(kasusId, Pemakaian(seed, kunci));
        await harness.MaterialService(OperatingRoomSeed.AkunBedah).RecordAsync(kasusId, Pemakaian(seed, kunci));
        await harness.MaterialService(OperatingRoomSeed.AkunBedah).RecordAsync(kasusId, Pemakaian(seed, kunci));

        await using var db = harness.NewContext();
        var pesan = await db.OprIntegrationDeliveries.AsNoTracking()
            .Where(x => x.OprCaseId == kasusId && !x.IsDelete).ToListAsync();

        Assert.Single(pesan);
        Assert.Single(pesan.Select(x => x.EventId).Distinct());
    }

    [Fact]
    public async Task Identitas_kejadian_tidak_berubah_walau_permintaannya_diulang()
    {
        // `EventId` diturunkan dari tujuan dan kunci idempotency, bukan diacak. Karena itu ia
        // sama walau dihitung ulang pada proses yang berbeda, dan indeks uniknya benar-benar
        // menjaga keesaan pesan.
        var pertama = OperatingRoomIntegrationService.DeterministicEventId("Inventory", "kasus:usage:1");
        var kedua = OperatingRoomIntegrationService.DeterministicEventId("Inventory", " kasus:usage:1 ");
        var lain = OperatingRoomIntegrationService.DeterministicEventId("Inventory", "kasus:usage:2");
        var tujuanLain = OperatingRoomIntegrationService.DeterministicEventId("Billing", "kasus:usage:1");

        Assert.Equal(pertama, kedua);
        Assert.NotEqual(pertama, lain);
        Assert.NotEqual(pertama, tujuanLain);
    }

    [Fact]
    public async Task Kejadian_berbeda_menghasilkan_pesan_berbeda()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        var material = harness.MaterialService(OperatingRoomSeed.AkunBedah);
        await material.RecordAsync(kasusId, Pemakaian(seed, alur.Kunci("INT"), 2));
        await material.RecordAsync(kasusId, Pemakaian(seed, alur.Kunci("INT"), 3));

        await using var db = harness.NewContext();
        var pesan = await db.OprIntegrationDeliveries.AsNoTracking()
            .Where(x => x.OprCaseId == kasusId && !x.IsDelete).ToListAsync();

        Assert.Equal(2, pesan.Count);
        Assert.Equal(2, pesan.Select(x => x.EventId).Distinct().Count());
        Assert.Equal(2, pesan.Select(x => x.IdempotencyKey).Distinct().Count());

        var jumlah = pesan
            .Select(x => JsonSerializer.Deserialize<OprIntegrationEvent>(x.PayloadJson,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })!)
            .Select(x => x.Material!.Quantity)
            .OrderBy(x => x)
            .ToList();

        Assert.Equal([2m, 3m], jumlah);
    }

    [Fact]
    public async Task Kejadian_tagihan_dan_kejadian_material_terpisah()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        await harness.MaterialService(OperatingRoomSeed.AkunBedah)
            .RecordAsync(kasusId, Pemakaian(seed, alur.Kunci("INT")));

        await using var db = harness.NewContext();
        var integrasi = harness.IntegrationService(OperatingRoomSeed.AkunBedah, db);
        await integrasi.StageChargeDeliveryAsync(kasusId, "procedure", 1,
            OperatingRoomSeed.AkunBedah, DateTime.UtcNow);
        await db.SaveChangesAsync();

        await using var baca = harness.NewContext();
        var pesan = await baca.OprIntegrationDeliveries.AsNoTracking()
            .Where(x => x.OprCaseId == kasusId && !x.IsDelete).ToListAsync();

        Assert.Equal(2, pesan.Count);
        Assert.Contains(pesan, x => x.EventType == OperatingRoomIntegrationService.MaterialEventType);
        Assert.Contains(pesan, x => x.EventType == OperatingRoomIntegrationService.ChargeEventType);
        Assert.Equal(2, pesan.Select(x => x.EventId).Distinct().Count());
        Assert.Equal(2, pesan.Select(x => x.Destination).Distinct().Count());
    }

    [Fact]
    public async Task Kegagalan_pengiriman_tidak_merusak_data_operasi()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        var pemakaian = await harness.MaterialService(OperatingRoomSeed.AkunBedah)
            .RecordAsync(kasusId, Pemakaian(seed, alur.Kunci("INT")));

        await using var db = harness.NewContext();
        var baris = await db.OprIntegrationDeliveries.AsNoTracking()
            .FirstAsync(x => x.OprCaseId == kasusId && !x.IsDelete);

        var statusSebelum = await alur.StatusAsync(kasusId);

        // Consumer menolak. Itu kabar buruk bagi pengirimannya, bukan bagi operasinya.
        var gagal = await harness.IntegrationService(OperatingRoomSeed.AkunBedah)
            .RecordAttemptAsync(kasusId, baris.Id, new RecordOprDeliveryAttemptRequest
            {
                Accepted = false, ErrorCode = "CONSUMER_TIMEOUT", IdempotencyKey = alur.Kunci("ATTEMPT")
            });

        Assert.Equal(OprDeliveryStatus.Failed, gagal.Status);
        Assert.Equal("CONSUMER_TIMEOUT", gagal.LastErrorCode);

        await using var periksa = harness.NewContext();
        Assert.Equal(statusSebelum, await alur.StatusAsync(kasusId));
        Assert.Equal(1, await periksa.OprMaterialUsages.CountAsync(x => x.OprCaseId == kasusId && !x.IsDelete));
        Assert.True(await periksa.OprMaterialUsages.AnyAsync(x => x.Id == pemakaian.Id && !x.IsDelete));

        // Amplopnya tetap utuh dan tetap dapat dibaca untuk dikirim ulang.
        var peristiwa = await harness.IntegrationService(OperatingRoomSeed.AkunBedah)
            .GetEventAsync(kasusId, baris.Id);
        Assert.NotNull(peristiwa);
        Assert.Equal(baris.EventId, peristiwa!.EventId);
    }

    [Fact]
    public async Task Pengiriman_gagal_dapat_diantrekan_ulang_tanpa_pesan_baru()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        await harness.MaterialService(OperatingRoomSeed.AkunBedah)
            .RecordAsync(kasusId, Pemakaian(seed, alur.Kunci("INT")));

        await using var db = harness.NewContext();
        var baris = await db.OprIntegrationDeliveries.AsNoTracking()
            .FirstAsync(x => x.OprCaseId == kasusId && !x.IsDelete);

        await harness.IntegrationService(OperatingRoomSeed.AkunBedah)
            .RecordAttemptAsync(kasusId, baris.Id, new RecordOprDeliveryAttemptRequest
            {
                Accepted = false, ErrorCode = "CONSUMER_TIMEOUT", IdempotencyKey = alur.Kunci("ATTEMPT")
            });

        var diantrekan = await harness.IntegrationService(OperatingRoomSeed.AkunBedah)
            .RetryAsync(kasusId, baris.Id);

        Assert.Equal(OprDeliveryStatus.Pending, diantrekan.Status);
        Assert.Equal(baris.EventId, diantrekan.EventId);

        await using var periksa = harness.NewContext();
        var pesan = await periksa.OprIntegrationDeliveries.AsNoTracking()
            .Where(x => x.OprCaseId == kasusId && !x.IsDelete).ToListAsync();

        Assert.Single(pesan);
        Assert.Equal(baris.EventId, pesan[0].EventId);
        Assert.Equal(1, pesan[0].RetryCount);
    }

    [Fact]
    public async Task Pesan_yang_sudah_diterima_tidak_dapat_dikirim_ulang()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        await harness.MaterialService(OperatingRoomSeed.AkunBedah)
            .RecordAsync(kasusId, Pemakaian(seed, alur.Kunci("INT")));

        await using var db = harness.NewContext();
        var baris = await db.OprIntegrationDeliveries.AsNoTracking()
            .FirstAsync(x => x.OprCaseId == kasusId && !x.IsDelete);

        await harness.IntegrationService(OperatingRoomSeed.AkunBedah)
            .RecordAttemptAsync(kasusId, baris.Id, new RecordOprDeliveryAttemptRequest
            {
                Accepted = true, AcceptedReference = "CONSUMER-REF-1", IdempotencyKey = alur.Kunci("ATTEMPT")
            });

        var galat = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            harness.IntegrationService(OperatingRoomSeed.AkunBedah)
                .RecordAttemptAsync(kasusId, baris.Id, new RecordOprDeliveryAttemptRequest
                {
                    Accepted = true, AcceptedReference = "CONSUMER-REF-2",
                    IdempotencyKey = alur.Kunci("ATTEMPT")
                }));

        Assert.Equal("InvalidStateTransition", galat.Code);
    }

    [Fact]
    public async Task Antrean_menunggu_dapat_dibaca_integration_layer()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        await harness.MaterialService(OperatingRoomSeed.AkunBedah)
            .RecordAsync(kasusId, Pemakaian(seed, alur.Kunci("INT")));

        var menunggu = await harness.IntegrationService(OperatingRoomSeed.AkunBedah)
            .GetPendingAsync(OperatingRoomIntegrationService.InventoryDestination);

        Assert.Single(menunggu);
        Assert.Equal(OprDeliveryStatus.Pending, menunggu[0].Status);
        Assert.NotEqual(Guid.Empty, menunggu[0].EventId);

        // Tujuan lain tidak ikut terbawa.
        Assert.Empty(await harness.IntegrationService(OperatingRoomSeed.AkunBedah)
            .GetPendingAsync(OperatingRoomIntegrationService.BillingDestination));
    }
}
