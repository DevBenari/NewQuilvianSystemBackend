using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// <c>PHA-DEC-018</c> — penyimpanan resep gagal jika jumlah obat melebihi stok tersedia.
/// </summary>
/// <remarks>
/// <para>
/// Keputusan ini disahkan product owner 19 Agustus 2026, tetapi gerbangnya baru terpasang
/// 15 Oktober 2026. Sebelum itu resep dapat difinalkan dan dibayar untuk obat yang stoknya
/// tidak mencukupi, lalu gagal di meja farmasi saat penyerahan mencoba menahan stok.
/// </para>
/// <para>
/// Seluruh uji di sini hanya menyaring kode <c>INSUFFICIENT_DRUG_STOCK</c>. Validasi lain pada
/// <c>ValidateForConsultationAsync</c> sengaja dibiarkan ikut menyala — harness tidak disiapkan
/// untuk lulus seluruh gerbang finalisasi, dan memaksanya lulus justru akan menguji harness,
/// bukan gerbang stoknya.
/// </para>
/// </remarks>
public class PrescriptionStockAvailabilityTests
{
    private const string Kode = "INSUFFICIENT_DRUG_STOCK";

    /// <summary>
    /// Menjadikan resep harness sebagai draft, karena gerbang stok — sama seperti
    /// <c>ValidateHeader</c> dan gerbang penanda keselamatan — hanya menyala untuk draft.
    /// </summary>
    private static async Task<Guid> JadikanDraftAsync(ApplicationDbContext konteks, Guid resepId)
    {
        var resep = await konteks.Set<PhmPrescription>().FirstAsync(x => x.Id == resepId);
        resep.PrescriptionStatus = PrescriptionStatus.Draft;
        await konteks.SaveChangesAsync();
        return resep.ConsultationId;
    }

    private static async Task<List<string>> TemuanStokAsync(
        PharmacyHarness harness, ApplicationDbContext konteks, Guid konsultasiId)
    {
        var temuan = await new PrescriptionValidationService(konteks)
            .ValidateForConsultationAsync(konsultasiId, harness.KunjunganId);

        return temuan
            .Where(x => x.Code == Kode && x.Severity == ConsultationValidationSeverity.Error)
            .Select(x => x.Message)
            .ToList();
    }

    [Fact]
    public async Task StokCukup_ResepLolosGerbangStok()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        // Butir harness meminta 10; stok disediakan 100.
        await harness.SediakanStokAsync(konteks, 100);
        var konsultasiId = await JadikanDraftAsync(konteks, harness.ResepId);

        Assert.Empty(await TemuanStokAsync(harness, konteks, konsultasiId));
    }

    [Fact]
    public async Task StokPersisSamaDenganPermintaan_ResepLolos()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        // Batasnya inklusif: tersedia 10 untuk permintaan 10 masih sah.
        await harness.SediakanStokAsync(konteks, 10);
        var konsultasiId = await JadikanDraftAsync(konteks, harness.ResepId);

        Assert.Empty(await TemuanStokAsync(harness, konteks, konsultasiId));
    }

    [Fact]
    public async Task StokKurang_ResepDitolakDanMenyebutAngkanya()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        await harness.SediakanStokAsync(konteks, 2);
        var konsultasiId = await JadikanDraftAsync(konteks, harness.ResepId);

        var pesan = Assert.Single(await TemuanStokAsync(harness, konteks, konsultasiId));

        Assert.Contains("Obat Uji Satu", pesan);
        Assert.Contains("diminta 10", pesan);
        Assert.Contains("tersedia 2", pesan);
        Assert.Contains("Pilih obat atau jumlah pengganti", pesan);
    }

    [Fact]
    public async Task ObatTanpaSaldoSamaSekali_DibacaNolBukanDilewati()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        // Tidak ada baris saldo untuk obat ini. Dibiarkan lewat berarti resep obat yang belum
        // pernah masuk gudang akan lolos — justru kasus terburuknya.
        var konsultasiId = await JadikanDraftAsync(konteks, harness.ResepId);

        var pesan = Assert.Single(await TemuanStokAsync(harness, konteks, konsultasiId));

        Assert.Contains("tersedia 0", pesan);
    }

    [Fact]
    public async Task StokYangSudahDitahanTidakDihitungTersedia()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        await harness.SediakanStokAsync(konteks, 10);

        // Sembilan dari sepuluh sudah ditahan resep lain, jadi yang benar-benar bebas hanya satu.
        var saldo = await konteks.Set<PhmDrugStockBalance>().FirstAsync();
        saldo.QuantityReserved = 9;
        await konteks.SaveChangesAsync();

        var konsultasiId = await JadikanDraftAsync(konteks, harness.ResepId);

        var pesan = Assert.Single(await TemuanStokAsync(harness, konteks, konsultasiId));

        Assert.Contains("tersedia 1", pesan);
    }

    [Fact]
    public async Task SaldoBerstatusBukanTersedia_TidakDihitung()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        await harness.SediakanStokAsync(konteks, 100);

        // Stok dikarantina. Jumlahnya besar, tetapi tidak boleh diserahkan kepada pasien.
        var saldo = await konteks.Set<PhmDrugStockBalance>().FirstAsync();
        saldo.Status = DrugStockStatus.Quarantine;
        await konteks.SaveChangesAsync();

        var konsultasiId = await JadikanDraftAsync(konteks, harness.ResepId);

        var pesan = Assert.Single(await TemuanStokAsync(harness, konteks, konsultasiId));

        Assert.Contains("tersedia 0", pesan);
    }

    [Fact]
    public async Task ButirBiasaDanBahanRacikanAtasObatSama_Dijumlahkan()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        // Tersedia 12. Butir biasa meminta 10, racikan meminta 5 atas obat yang sama.
        // Keduanya menarik dari saldo yang sama, jadi kebutuhan sesungguhnya 15 — kurang 3.
        await harness.SediakanStokAsync(konteks, 12);

        var racikanId = Guid.NewGuid();

        konteks.Set<PhmPrescriptionCompound>().Add(new PhmPrescriptionCompound
        {
            Id = racikanId,
            PrescriptionId = harness.ResepId,
            CompoundName = "Racikan Uji",
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        konteks.Set<PhmPrescriptionCompoundItem>().Add(new PhmPrescriptionCompoundItem
        {
            Id = Guid.NewGuid(),
            PrescriptionCompoundId = racikanId,
            DrugId = harness.ObatId,
            DrugCodeSnapshot = "UJI-OBAT-1",
            DrugNameSnapshot = "Obat Uji Satu",
            TotalQuantity = 5,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        await konteks.SaveChangesAsync();

        var konsultasiId = await JadikanDraftAsync(konteks, harness.ResepId);

        // Satu temuan, bukan dua: obatnya satu, jadi keluhannya satu.
        var pesan = Assert.Single(await TemuanStokAsync(harness, konteks, konsultasiId));

        Assert.Contains("diminta 15", pesan);
        Assert.Contains("tersedia 12", pesan);
    }

    [Fact]
    public async Task ResepYangBukanDraft_TidakTersentuhGerbangStok()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        // Harness memulai resep pada `Submitted` tanpa stok sama sekali. Resep yang sudah
        // terkirim bukan lagi milik dokter untuk diubah, jadi menolaknya di sini tidak
        // memberi jalan keluar kepada siapa pun.
        var resep = await konteks.Set<PhmPrescription>().FirstAsync(x => x.Id == harness.ResepId);

        Assert.Equal(PrescriptionStatus.Submitted, resep.PrescriptionStatus);
        Assert.Empty(await TemuanStokAsync(harness, konteks, resep.ConsultationId));
    }
}
