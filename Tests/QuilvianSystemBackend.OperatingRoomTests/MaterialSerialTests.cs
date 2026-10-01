using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Tests.OperatingRoom.Infrastructure;

namespace QuilvianSystemBackend.Tests.OperatingRoom;

/// <summary>
/// Ledger pemakaian material dan keunikan nomor serial implant (`BE-OPR-008`, `OPR014`).
/// </summary>
/// <remarks>
/// Satu implant bernomor seri hanya ada satu benda. Mencatatnya dua kali sebagai terpakai
/// pada satu operasi berarti menyatakan benda yang sama terpasang dua kali, dan itu merusak
/// penelusuran implant yang justru menjadi alasan nomor seri dicatat.
/// </remarks>
public class MaterialSerialTests
{
    private const string Serial = "UJI-SN-0001";

    private static CreateOprMaterialUsageRequest Permintaan(OperatingRoomSeed seed, string kunci,
        OprMaterialOutcome hasil = OprMaterialOutcome.Used,
        OprMaterialItemType jenis = OprMaterialItemType.Implant,
        string? serial = Serial) => new()
        {
            ExternalItemId = seed.Hasil.MaterialItemIds[0],
            ItemType = jenis,
            Quantity = 1,
            UnitMeasurementId = seed.Hasil.MeasurementId,
            UnitCode = "UJI-PCS",
            Outcome = hasil,
            BatchNumber = "UJI-BATCH-01",
            SerialNumber = serial,
            IdempotencyKey = kunci
        };

    [Fact]
    public async Task Serial_implant_yang_sah_dapat_dicatat()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        var hasil = await harness.MaterialService(OperatingRoomSeed.AkunBedah)
            .RecordAsync(kasusId, Permintaan(seed, alur.Kunci("MAT")));

        Assert.Equal(Serial, hasil.SerialNumber);
    }

    [Fact]
    public async Task Serial_implant_yang_sama_ditolak_pada_kasus_yang_sama()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        await harness.MaterialService(OperatingRoomSeed.AkunBedah)
            .RecordAsync(kasusId, Permintaan(seed, alur.Kunci("MAT")));

        var galat = await Assert.ThrowsAsync<OperatingRoomConflictException>(() =>
            harness.MaterialService(OperatingRoomSeed.AkunBedah)
                .RecordAsync(kasusId, Permintaan(seed, alur.Kunci("MAT"))));

        Assert.Equal("OPR014", galat.Code);

        await using var db = harness.NewContext();
        var jumlah = await db.OprMaterialUsages.CountAsync(x =>
            x.OprCaseId == kasusId && x.SerialNumber == Serial && !x.IsDelete);
        Assert.Equal(1, jumlah);
    }

    [Fact]
    public async Task Pengulangan_permintaan_yang_sama_tidak_dianggap_serial_ganda()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        var kunci = alur.Kunci("MATULANG");
        var pertama = await harness.MaterialService(OperatingRoomSeed.AkunBedah)
            .RecordAsync(kasusId, Permintaan(seed, kunci));
        var kedua = await harness.MaterialService(OperatingRoomSeed.AkunBedah)
            .RecordAsync(kasusId, Permintaan(seed, kunci));

        Assert.Equal(pertama.Id, kedua.Id);

        await using var db = harness.NewContext();
        Assert.Equal(1, await db.OprMaterialUsages.CountAsync(x =>
            x.OprCaseId == kasusId && x.SerialNumber == Serial && !x.IsDelete));
    }

    [Fact]
    public async Task Serial_yang_sama_boleh_muncul_pada_kasus_operasi_yang_berbeda()
    {
        // Cakupan keunikan V1 adalah satu kasus. Implant yang gagal dan diganti pada operasi
        // revisi tetap harus dapat dicatat dengan nomor seri yang sama pada kasus lain.
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);

        var pertama = await alur.KasusBerjalanAsync();
        await harness.MaterialService(OperatingRoomSeed.AkunBedah)
            .RecordAsync(pertama, Permintaan(seed, alur.Kunci("MAT")));

        var kedua = await alur.KasusBerjalanAsync();
        var hasil = await harness.MaterialService(OperatingRoomSeed.AkunBedah)
            .RecordAsync(kedua, Permintaan(seed, alur.Kunci("MAT")));

        Assert.Equal(Serial, hasil.SerialNumber);
    }

    [Fact]
    public async Task Retur_dengan_serial_yang_sama_tidak_ditolak()
    {
        // `Returned` justru menyatakan implant tidak jadi terpasang, jadi ia tidak boleh
        // dihitung sebagai pemakaian kedua.
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        await harness.MaterialService(OperatingRoomSeed.AkunBedah)
            .RecordAsync(kasusId, Permintaan(seed, alur.Kunci("MAT"), OprMaterialOutcome.Returned));

        var hasil = await harness.MaterialService(OperatingRoomSeed.AkunBedah)
            .RecordAsync(kasusId, Permintaan(seed, alur.Kunci("MAT")));

        Assert.Equal(Serial, hasil.SerialNumber);
    }

    [Fact]
    public async Task Bahan_habis_pakai_tanpa_serial_tidak_terkena_aturan_ini()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        var material = harness.MaterialService(OperatingRoomSeed.AkunBedah);
        await material.RecordAsync(kasusId,
            Permintaan(seed, alur.Kunci("MAT"), jenis: OprMaterialItemType.Consumable, serial: null));
        await material.RecordAsync(kasusId,
            Permintaan(seed, alur.Kunci("MAT"), jenis: OprMaterialItemType.Consumable, serial: null));

        await using var db = harness.NewContext();
        Assert.Equal(2, await db.OprMaterialUsages.CountAsync(x => x.OprCaseId == kasusId && !x.IsDelete));
    }

    [Fact]
    public async Task Implant_tanpa_batch_dan_serial_ditolak()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        var permintaan = Permintaan(seed, alur.Kunci("MAT"), serial: null);
        permintaan.BatchNumber = null;

        var galat = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            harness.MaterialService(OperatingRoomSeed.AkunBedah).RecordAsync(kasusId, permintaan));

        Assert.Equal("OPR009", galat.Code);
    }

    [Fact]
    public async Task Jumlah_nol_ditolak()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        var alur = new AlurOperasi(harness, seed);
        var kasusId = await alur.KasusBerjalanAsync();

        var permintaan = Permintaan(seed, alur.Kunci("MAT"));
        permintaan.Quantity = 0;

        var galat = await Assert.ThrowsAsync<OperatingRoomUnprocessableException>(() =>
            harness.MaterialService(OperatingRoomSeed.AkunBedah).RecordAsync(kasusId, permintaan));

        Assert.Equal("OPR008", galat.Code);
    }
}
