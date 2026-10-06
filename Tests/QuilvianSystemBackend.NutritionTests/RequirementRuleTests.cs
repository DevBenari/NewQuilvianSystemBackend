using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Tests.Nutrition.Infrastructure;

namespace QuilvianSystemBackend.Tests.Nutrition;

/// <summary>
/// Aturan penetapan kebutuhan nutrisi, `GIZ-DEC-012`.
/// </summary>
/// <remarks>
/// Angka kebutuhan nutrisi dipakai merawat pasien selama rawatan berjalan, dan sekali tersimpan
/// ia menjadi dasar diet yang dibaca dapur. Aturan di bawah inilah yang menjaga angka itu tetap
/// dapat dipertanggungjawabkan: utuh, berbatas, berhistori, dan setiap koreksinya beralasan.
/// </remarks>
public class RequirementRuleTests
{
    /// <summary>
    /// Permintaan sah terkecil: seluruh parameter aktif terisi, karena `GIZ015` menuntut satu
    /// revisi memuat semuanya. Nilai bawaannya angka wajar sebagai data uji, bukan anjuran gizi.
    /// </summary>
    private static SaveGzRequirementRequest Permintaan(NutritionHarness h,
        decimal energi = 2000m, string? alasan = null, string? kunci = null)
    {
        var bawaan = new Dictionary<string, decimal>
        {
            ["ENERGY"] = energi,
            ["PROTEIN"] = 60m,
            ["FAT"] = 55m,
            ["CARBOHYDRATE"] = 275m,
            ["FLUID"] = 2000m,
        };

        return new SaveGzRequirementRequest
        {
            IdempotencyKey = kunci ?? Guid.NewGuid().ToString("N"),
            CareRecordId = h.KunjunganId,
            DeterminedByWorkforceId = h.AhliGiziId,
            ChangeReason = alasan,
            Items = [.. h.Parameter.Select(p => new GziRequirementItemRequest
            {
                NutritionParameterId = p.Id,
                FinalValue = bawaan[p.ParameterCode]
            })]
        };
    }

    private static GziRequirementItemRequest Butir(SaveGzRequirementRequest r, NutritionHarness h,
        string kode) => r.Items.Single(x =>
            x.NutritionParameterId == h.Parameter.Single(p => p.ParameterCode == kode).Id);

    [Fact]
    public async Task Penetapan_pertama_tersimpan_sebagai_revisi_satu()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await h.RequirementService(konteks).SaveAsync(h.OrderId, Permintaan(h));

        Assert.Equal(1, hasil.RevisionNumber);
        Assert.True(hasil.IsCurrent);
        Assert.Equal(5, hasil.Items.Count);
    }

    [Fact]
    public async Task Revisi_kedua_wajib_beralasan()
    {
        // `GIZ017`. Revisi pertama adalah penetapan awal; yang kedua mengubah angka yang sudah
        // dipakai merawat pasien, dan perubahan tanpa alasan tidak dapat ditelaah kemudian.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.RequirementService(konteks);

        await layanan.SaveAsync(h.OrderId, Permintaan(h));

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            layanan.SaveAsync(h.OrderId, Permintaan(h, energi: 2200m)));

        Assert.Equal("GIZ017", galat.Code);
    }

    [Fact]
    public async Task Revisi_kedua_yang_beralasan_menggeser_revisi_berlaku()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.RequirementService(konteks);

        await layanan.SaveAsync(h.OrderId, Permintaan(h));
        var kedua = await layanan.SaveAsync(h.OrderId,
            Permintaan(h, energi: 2200m, alasan: "Berat badan naik"));

        Assert.Equal(2, kedua.RevisionNumber);
        Assert.True(kedua.IsCurrent);

        var riwayat = await layanan.GetHistoryAsync(h.OrderId);
        Assert.Equal(2, riwayat.Count);
        Assert.Single(riwayat.Where(x => x.IsCurrent));

        var berlaku = await layanan.GetCurrentAsync(h.OrderId);
        Assert.Equal(2, berlaku!.RevisionNumber);
    }

    [Fact]
    public async Task Revisi_tidak_boleh_memuat_sebagian_parameter_saja()
    {
        // `GIZ015`. Revisi sebagian membuat pembacanya menyangka parameter yang hilang bernilai
        // nol, padahal ia sekadar tidak ikut dikirim.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Permintaan(h);
        permintaan.Items.Remove(Butir(permintaan, h, "FLUID"));

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.RequirementService(konteks).SaveAsync(h.OrderId, permintaan));

        Assert.Equal("GIZ015", galat.Code);
        Assert.Contains("Cairan", galat.Message);
    }

    [Fact]
    public async Task Lima_zat_gizi_keputusan_GIZ_DEC_012_yang_tersimpan()
    {
        // `GIZ-DEC-012` menyebut lima angka: energi, protein, lemak, karbohidrat, cairan.
        // Kelimanya ditanam konfigurasi EF sebagai master parameter, dan uji ini menjaga daftar
        // itu tidak menyusut maupun bertambah tanpa keputusan baru pemilik proses.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await h.RequirementService(konteks).SaveAsync(h.OrderId, Permintaan(h));

        Assert.Equal(
            new[] { "CARBOHYDRATE", "ENERGY", "FAT", "FLUID", "PROTEIN" },
            hasil.Items.Select(x => x.ParameterCode).OrderBy(x => x, StringComparer.Ordinal));

        Assert.Equal("kkal/hari", hasil.Items.Single(x => x.ParameterCode == "ENERGY").UnitCode);
        Assert.Equal("ml/hari", hasil.Items.Single(x => x.ParameterCode == "FLUID").UnitCode);
    }

    [Fact]
    public async Task Satu_parameter_tidak_boleh_dikirim_dua_kali()
    {
        // `GIZ014`. Dua nilai untuk satu parameter membuat angka mana yang berlaku bergantung
        // pada urutan baris — bukan pada keputusan siapa pun.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Permintaan(h);
        permintaan.Items.Add(new GziRequirementItemRequest
        {
            NutritionParameterId = h.Energi.Id,
            FinalValue = 3000m
        });

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.RequirementService(konteks).SaveAsync(h.OrderId, permintaan));

        Assert.Equal("GIZ014", galat.Code);
    }

    [Fact]
    public async Task Parameter_yang_tidak_dikenal_ditolak()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Permintaan(h);
        Butir(permintaan, h, "ENERGY").NutritionParameterId = Guid.NewGuid();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.RequirementService(konteks).SaveAsync(h.OrderId, permintaan));

        Assert.Equal("GIZ014", galat.Code);
    }

    [Theory]
    [InlineData(0)]        // di bawah batas bawah energi (MinValue 1)
    [InlineData(99999)]    // di atas batas atas energi (MaxValue 10000)
    public async Task Nilai_di_luar_batas_parameter_ditolak(int energi)
    {
        // `GIZ006`. Batasnya milik master parameter, bukan milik kode — menggesernya keputusan
        // instalasi gizi, dan uji ini hanya memastikan batas yang ada benar-benar ditegakkan.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.RequirementService(konteks).SaveAsync(h.OrderId, Permintaan(h, energi: energi)));

        Assert.Equal("GIZ006", galat.Code);
        Assert.Contains("Energi", galat.Message);
    }

    [Fact]
    public async Task Koreksi_atas_hasil_hitungan_wajib_beralasan()
    {
        // `GIZ016`. Ahli gizi boleh mengoreksi hasil rumus — itu memang hak yang diberikan
        // `GIZ-DEC-012` — tetapi koreksi tanpa alasan tidak dapat dinilai kewajarannya ketika
        // hasilnya dipersoalkan kemudian.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Permintaan(h);
        Butir(permintaan, h, "ENERGY").CalculatedValue = 1800m;
        Butir(permintaan, h, "ENERGY").FinalValue = 2200m;

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.RequirementService(konteks).SaveAsync(h.OrderId, permintaan));

        Assert.Equal("GIZ016", galat.Code);
    }

    [Fact]
    public async Task Koreksi_beralasan_menyimpan_nilai_hitungan_dan_nilai_final()
    {
        // Keduanya disimpan, bukan salah satu: tanpa nilai hitungan, besarnya koreksi hilang.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Permintaan(h);
        Butir(permintaan, h, "ENERGY").CalculatedValue = 1800m;
        Butir(permintaan, h, "ENERGY").FinalValue = 2200m;
        Butir(permintaan, h, "ENERGY").AdjustmentReason = "Pasien demam, kebutuhan energi dinaikkan";

        var hasil = await h.RequirementService(konteks).SaveAsync(h.OrderId, permintaan);

        var energi = hasil.Items.Single(x => x.ParameterCode == "ENERGY");
        Assert.Equal(1800m, energi.CalculatedValue);
        Assert.Equal(2200m, energi.FinalValue);
    }

    [Fact]
    public async Task Nilai_sama_dengan_hasil_hitungan_tidak_menuntut_alasan()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Permintaan(h);
        Butir(permintaan, h, "ENERGY").CalculatedValue = 2000m;
        Butir(permintaan, h, "ENERGY").FinalValue = 2000m;

        var hasil = await h.RequirementService(konteks).SaveAsync(h.OrderId, permintaan);

        Assert.Equal(1, hasil.RevisionNumber);
    }

    [Fact]
    public async Task Rumus_yang_tidak_terdaftar_ditolak()
    {
        // `GIZ019`. Nilai yang mengaku berasal dari rumus tertentu harus benar-benar berasal
        // dari sana; rumus asing diabaikan diam-diam akan membuat jejaknya berbohong.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Permintaan(h);
        permintaan.CalculationFormulaId = Guid.NewGuid();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.RequirementService(konteks).SaveAsync(h.OrderId, permintaan));

        Assert.Equal("GIZ019", galat.Code);
    }

    [Fact]
    public async Task Rumus_terdaftar_diterima()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Permintaan(h);
        permintaan.CalculationFormulaId = h.RumusId;

        var hasil = await h.RequirementService(konteks).SaveAsync(h.OrderId, permintaan);

        Assert.Equal(1, hasil.RevisionNumber);
    }

    [Fact]
    public async Task Order_yang_sudah_ditutup_tidak_menerima_kebutuhan_baru()
    {
        // `GIZ004`. Kebutuhan nutrisi hanya bermakna selama order gizinya masih berjalan.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Permintaan(h);
        permintaan.CareRecordId = null;

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            h.RequirementService(konteks).SaveAsync(h.OrderTertutupId, permintaan));

        Assert.Equal("GIZ004", galat.Code);
    }

    [Fact]
    public async Task Kunjungan_milik_order_lain_ditolak()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Permintaan(h);
        permintaan.CareRecordId = Guid.NewGuid();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.RequirementService(konteks).SaveAsync(h.OrderId, permintaan));

        Assert.Equal("GIZ004", galat.Code);
    }

    [Fact]
    public async Task Ahli_gizi_yang_tidak_ada_ditolak()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Permintaan(h);
        permintaan.DeterminedByWorkforceId = Guid.NewGuid();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.RequirementService(konteks).SaveAsync(h.OrderId, permintaan));

        Assert.Equal("GIZ004", galat.Code);
    }

    [Fact]
    public async Task Kunci_idempotensi_wajib_diisi()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Permintaan(h);
        permintaan.IdempotencyKey = "   ";

        await Assert.ThrowsAsync<ArgumentException>(() =>
            h.RequirementService(konteks).SaveAsync(h.OrderId, permintaan));
    }

    [Fact]
    public async Task Pengiriman_ulang_dengan_kunci_sama_tidak_menambah_revisi()
    {
        // Permintaan yang diulang — jaringan terputus, tombol ditekan dua kali — tidak boleh
        // melahirkan revisi kedua, karena revisi kedua berarti angka pasien dianggap berubah.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.RequirementService(konteks);

        var kunci = Guid.NewGuid().ToString("N");
        var pertama = await layanan.SaveAsync(h.OrderId, Permintaan(h, kunci: kunci));
        var kedua = await layanan.SaveAsync(h.OrderId, Permintaan(h, kunci: kunci));

        Assert.Equal(pertama.Id, kedua.Id);
        Assert.Equal(1, kedua.RevisionNumber);
        Assert.Single(await layanan.GetHistoryAsync(h.OrderId));
    }

    [Fact]
    public async Task Kunci_idempotensi_yang_dipakai_untuk_isi_berbeda_ditolak()
    {
        // `GIZ013`. Mengembalikan hasil lama untuk isi yang berbeda akan membuat pengirimnya
        // yakin angka barunya tersimpan, padahal tidak.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.RequirementService(konteks);

        var kunci = Guid.NewGuid().ToString("N");
        await layanan.SaveAsync(h.OrderId, Permintaan(h, kunci: kunci));

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            layanan.SaveAsync(h.OrderId, Permintaan(h, energi: 2500m, kunci: kunci)));

        Assert.Equal("GIZ013", galat.Code);
    }
}
