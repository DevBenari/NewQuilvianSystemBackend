using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Tests.Nutrition.Infrastructure;

namespace QuilvianSystemBackend.Tests.Nutrition;

/// <summary>
/// Diagnosis gizi pada satu kunjungan, `GIZ-DEC-011`.
/// </summary>
/// <remarks>
/// Diagnosis gizi dipilih dari master berkode berbaseline IDNT, bukan diketik bebas. Uji di
/// bawah menjaga dua hal yang membuat pilihan itu bermakna: hanya baris yang memang dapat
/// dipilih yang boleh ditegakkan, dan hanya ada satu diagnosis primer per kunjungan.
/// </remarks>
public class DiagnosisRuleTests
{
    private static SaveGzCareRecordDiagnosesRequest Permintaan(
        params NutritionCareRecordDiagnosisRequest[] diagnosis) => new()
        {
            IdempotencyKey = Guid.NewGuid().ToString("N"),
            Diagnoses = [.. diagnosis]
        };

    private static NutritionCareRecordDiagnosisRequest Butir(Guid id, bool primer = false) =>
        new() { NutritionDiagnosisId = id, IsPrimary = primer };

    [Fact]
    public async Task Diagnosis_aktif_dapat_ditegakkan()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await h.RequirementService(konteks).SaveCareRecordDiagnosesAsync(
            h.OrderId, h.KunjunganId,
            Permintaan(Butir(h.DiagnosisAId, primer: true), Butir(h.DiagnosisBId)));

        Assert.Equal(2, hasil.Count);
        Assert.Single(hasil.Where(x => x.IsPrimary));
    }

    [Fact]
    public async Task Diagnosis_yang_sama_tidak_boleh_ditegakkan_dua_kali()
    {
        // `GIZ005`. Diagnosis kembar membuat jumlah masalah gizi pasien terbaca lebih banyak
        // daripada yang sebenarnya ditegakkan.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.RequirementService(konteks).SaveCareRecordDiagnosesAsync(
                h.OrderId, h.KunjunganId,
                Permintaan(Butir(h.DiagnosisAId), Butir(h.DiagnosisAId))));

        Assert.Equal("GIZ005", galat.Code);
    }

    [Fact]
    public async Task Hanya_satu_diagnosis_primer_per_kunjungan()
    {
        // `GIZ018`. Dua yang primer berarti tidak ada yang primer.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.RequirementService(konteks).SaveCareRecordDiagnosesAsync(
                h.OrderId, h.KunjunganId,
                Permintaan(Butir(h.DiagnosisAId, true), Butir(h.DiagnosisBId, true))));

        Assert.Equal("GIZ018", galat.Code);
    }

    [Fact]
    public async Task Diagnosis_yang_tidak_dapat_dipilih_ditolak()
    {
        // Baris induk pada hierarki IDNT ada sebagai pengelompokan, bukan sebagai diagnosis yang
        // ditegakkan. `IsSelectable` itulah yang membedakannya, dan uji ini menjaga pembedaan
        // tersebut benar-benar berlaku walaupun barisnya aktif.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.RequirementService(konteks).SaveCareRecordDiagnosesAsync(
                h.OrderId, h.KunjunganId, Permintaan(Butir(h.DiagnosisTerlarangId))));

        Assert.Equal("GIZ005", galat.Code);
    }

    [Fact]
    public async Task Diagnosis_asing_ditolak()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.RequirementService(konteks).SaveCareRecordDiagnosesAsync(
                h.OrderId, h.KunjunganId, Permintaan(Butir(Guid.NewGuid()))));

        Assert.Equal("GIZ005", galat.Code);
    }

    [Fact]
    public async Task Daftar_diagnosis_bersifat_pengganti_bukan_tambahan()
    {
        // Kontraknya menyatakan baris yang tidak disebut ditandai terhapus. Perilaku itu mudah
        // berubah tanpa sengaja ketika kode penyimpanannya disentuh, dan akibatnya diagnosis
        // yang sudah dicabut ahli gizi kembali muncul.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.RequirementService(konteks);

        await layanan.SaveCareRecordDiagnosesAsync(h.OrderId, h.KunjunganId,
            Permintaan(Butir(h.DiagnosisAId, true), Butir(h.DiagnosisBId)));

        var sesudah = await layanan.SaveCareRecordDiagnosesAsync(h.OrderId, h.KunjunganId,
            Permintaan(Butir(h.DiagnosisBId, true)));

        Assert.Single(sesudah);
        Assert.Equal(h.DiagnosisBId, sesudah[0].NutritionDiagnosisId);
    }

    [Fact]
    public async Task Daftar_kosong_mencabut_seluruh_diagnosis()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.RequirementService(konteks);

        await layanan.SaveCareRecordDiagnosesAsync(h.OrderId, h.KunjunganId,
            Permintaan(Butir(h.DiagnosisAId, true)));

        var sesudah = await layanan.SaveCareRecordDiagnosesAsync(
            h.OrderId, h.KunjunganId, Permintaan());

        Assert.Empty(sesudah);
        Assert.Empty(await layanan.GetCareRecordDiagnosesAsync(h.OrderId, h.KunjunganId));
    }

    [Fact]
    public async Task Kunjungan_milik_order_lain_tidak_dapat_didiagnosis()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            h.RequirementService(konteks).SaveCareRecordDiagnosesAsync(
                h.OrderTertutupId, h.KunjunganId, Permintaan(Butir(h.DiagnosisAId))));
    }

    [Fact]
    public void Nomor_status_order_tetap_pada_angka_yang_sama()
    {
        // Angka status ikut tersimpan di basis data dan dibaca frontend. Menggesernya memutus
        // seluruh pembacaan status yang sudah tersimpan.
        Assert.Equal(1, (int)GziOrderStatus.Requested);
        Assert.Equal(2, (int)GziOrderStatus.InProgress);
        Assert.Equal(3, (int)GziOrderStatus.Closed);
        Assert.Equal(4, (int)GziOrderStatus.Cancelled);
    }

    [Fact]
    public void Nomor_status_diet_dan_penyerahan_tetap()
    {
        Assert.Equal(1, (int)GziPatientDietStatus.Active);
        Assert.Equal(2, (int)GziPatientDietStatus.Changed);
        Assert.Equal(3, (int)GziPatientDietStatus.Stopped);

        Assert.Equal(1, (int)GziMealDeliveryStatus.Delivered);
        Assert.Equal(2, (int)GziMealDeliveryStatus.Refused);
        Assert.Equal(3, (int)GziMealDeliveryStatus.NotServed);

        Assert.Equal(1, (int)GziCareRecordType.Initial);
        Assert.Equal(2, (int)GziCareRecordType.FollowUp);
    }
}
