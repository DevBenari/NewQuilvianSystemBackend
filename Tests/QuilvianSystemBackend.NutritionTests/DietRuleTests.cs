using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Tests.Nutrition.Infrastructure;

namespace QuilvianSystemBackend.Tests.Nutrition;

/// <summary>
/// Penetapan, penggantian, dan penghentian diet pasien.
/// </summary>
/// <remarks>
/// Diet aktif inilah satu-satunya baris yang dibaca dapur untuk menentukan makanan pasien.
/// Dua diet aktif pada satu kunjungan berarti dapur memilih sendiri mana yang dipakai; diet lama
/// yang tertimpa alih-alih ditutup berarti urutan Diet Biasa → Diabetes → Lunak → Puasa hilang
/// dan tidak dapat ditelusuri ketika hasil asuhan dipersoalkan.
/// </remarks>
public class DietRuleTests
{
    private static PrescribeGzDietRequest Penetapan(NutritionHarness h, string? alasan = null,
        string? kunci = null) => new()
        {
            PatientId = h.PasienId,
            EncounterId = h.KunjunganRawatId,
            DietTypeId = h.JenisDietId,
            FoodFormId = h.BentukMakananId,
            PrescribedByWorkforceId = h.AhliGiziId,
            EnergyRequirementKcal = 2000,
            Instruction = "Porsi kecil sering",
            ChangeReason = alasan,
            IdempotencyKey = kunci ?? Guid.NewGuid().ToString("N")
        };

    // ------------------------------------------------------------------ penetapan

    [Fact]
    public async Task Diet_valid_ditetapkan_sebagai_aktif()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await h.DietService(konteks).PrescribeAsync(Penetapan(h));

        Assert.Equal(GziPatientDietStatus.Active, hasil.Status);
        Assert.Equal(h.KunjunganRawatId, hasil.EncounterId);
        Assert.Equal(0, hasil.Version);
    }

    [Fact]
    public async Task Kunci_idempotensi_wajib_diisi()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            h.DietService(konteks).PrescribeAsync(Penetapan(h, kunci: " ")));
    }

    [Fact]
    public async Task Penetapan_ulang_dengan_kunci_sama_mengembalikan_diet_yang_sama()
    {
        // Idempotensinya lewat id deterministik, bukan tabel riwayat tersendiri. Tanpa ini,
        // permintaan yang diulang akan menutup diet yang baru saja dibuatnya sendiri dan
        // melahirkan baris kedua — riwayat palsu yang terlihat seperti penggantian diet.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.DietService(konteks);

        var kunci = Guid.NewGuid().ToString("N");
        var pertama = await layanan.PrescribeAsync(Penetapan(h, kunci: kunci));
        var kedua = await layanan.PrescribeAsync(Penetapan(h, kunci: kunci));

        Assert.Equal(pertama.Id, kedua.Id);
        Assert.Single(await layanan.GetDietHistoryAsync(h.KunjunganRawatId));
    }

    [Fact]
    public async Task Kunjungan_yang_bukan_milik_pasien_ditolak()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Penetapan(h);
        permintaan.PatientId = Guid.NewGuid();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.DietService(konteks).PrescribeAsync(permintaan));

        Assert.Equal("GIZ001", galat.Code);
    }

    [Fact]
    public async Task Episode_rawat_inap_yang_sudah_pulang_ditolak()
    {
        // Harness memuat satu episode berstatus Discharged justru untuk ini: yang dituntut
        // episode yang masih AKTIF, bukan sekadar ada.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Penetapan(h);
        permintaan.EncounterId = h.KunjunganPulangId;

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.DietService(konteks).PrescribeAsync(permintaan));

        Assert.Equal("GIZ001", galat.Code);
    }

    [Fact]
    public async Task Kunjungan_tanpa_episode_rawat_inap_ditolak()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Penetapan(h);
        permintaan.EncounterId = Guid.NewGuid();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.DietService(konteks).PrescribeAsync(permintaan));

        Assert.Equal("GIZ001", galat.Code);
    }

    [Fact]
    public async Task Jenis_diet_yang_tidak_aktif_ditolak()
    {
        // `GIZ014`. Jenis diet yang sudah ditarik instalasi gizi tidak boleh dipakai lagi,
        // walaupun barisnya masih ada untuk membaca riwayat lama.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Penetapan(h);
        permintaan.DietTypeId = h.JenisDietNonaktifId;

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.DietService(konteks).PrescribeAsync(permintaan));

        Assert.Equal("GIZ014", galat.Code);
    }

    [Fact]
    public async Task Bentuk_makanan_yang_tidak_aktif_ditolak()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Penetapan(h);
        permintaan.FoodFormId = h.BentukMakananNonaktifId;

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.DietService(konteks).PrescribeAsync(permintaan));

        Assert.Equal("GIZ014", galat.Code);
    }

    [Fact]
    public async Task Jenis_diet_asing_ditolak()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Penetapan(h);
        permintaan.DietTypeId = Guid.NewGuid();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.DietService(konteks).PrescribeAsync(permintaan));

        Assert.Equal("GIZ014", galat.Code);
    }

    // ------------------------------------------------------------------ penggantian

    [Fact]
    public async Task Penggantian_diet_wajib_beralasan()
    {
        // `GIZ010`. Diet pertama tidak perlu alasan; menggantinya mengubah makanan pasien yang
        // sedang berjalan, dan perubahan tanpa alasan tidak dapat ditelaah.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.DietService(konteks);

        await layanan.PrescribeAsync(Penetapan(h));

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            layanan.PrescribeAsync(Penetapan(h)));

        Assert.Equal("GIZ010", galat.Code);
    }

    [Fact]
    public async Task Penggantian_beralasan_menutup_diet_lama_tanpa_menghapusnya()
    {
        // Inti riwayat diet. Diet lama ditandai `Changed` dan tetap tersimpan, sehingga urutan
        // perubahan diet satu pasien dapat dibaca seluruhnya di kemudian hari.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.DietService(konteks);

        var lama = await layanan.PrescribeAsync(Penetapan(h));
        var baru = await layanan.PrescribeAsync(Penetapan(h, alasan: "Gula darah naik"));

        var riwayat = await layanan.GetDietHistoryAsync(h.KunjunganRawatId);

        Assert.Equal(2, riwayat.Count);
        Assert.Single(riwayat.Where(x => x.Status == GziPatientDietStatus.Active));
        Assert.Equal(baru.Id, riwayat.Single(x => x.Status == GziPatientDietStatus.Active).Id);

        var ditutup = riwayat.Single(x => x.Id == lama.Id);
        Assert.Equal(GziPatientDietStatus.Changed, ditutup.Status);
        Assert.Equal("Gula darah naik", ditutup.ChangeReason);
        Assert.NotNull(ditutup.EndAt);
    }

    [Fact]
    public async Task Hanya_satu_diet_aktif_per_kunjungan_setelah_beberapa_penggantian()
    {
        // Dapur membaca satu baris. Tiga penggantian tidak boleh menyisakan dua yang aktif.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.DietService(konteks);

        await layanan.PrescribeAsync(Penetapan(h));
        await layanan.PrescribeAsync(Penetapan(h, alasan: "Perubahan 1"));
        await layanan.PrescribeAsync(Penetapan(h, alasan: "Perubahan 2"));

        var riwayat = await layanan.GetDietHistoryAsync(h.KunjunganRawatId);

        Assert.Equal(3, riwayat.Count);
        Assert.Single(riwayat.Where(x => x.Status == GziPatientDietStatus.Active));
    }

    [Fact]
    public async Task Diet_tetap_terikat_pada_kunjungan_dan_pasiennya()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.DietService(konteks);

        var diet = await layanan.PrescribeAsync(Penetapan(h));

        Assert.Equal(h.PasienId, diet.PatientId);
        Assert.Equal(h.KunjunganRawatId, diet.EncounterId);
        Assert.Empty(await layanan.GetDietHistoryAsync(h.KunjunganPulangId));
    }

    // ------------------------------------------------------------------ penghentian

    [Fact]
    public async Task Penghentian_diet_wajib_beralasan()
    {
        // `GIZ011`.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.DietService(konteks);

        var diet = await layanan.PrescribeAsync(Penetapan(h));

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            layanan.StopAsync(diet.Id, new StopGzDietRequest
            {
                Reason = "   ",
                ExpectedVersion = diet.Version,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("GIZ011", galat.Code);
    }

    [Fact]
    public async Task Penghentian_yang_beralasan_menandai_diet_berhenti()
    {
        // `Stopped` dibedakan dari `Changed` dengan sengaja: berhenti tanpa pengganti — pasien
        // puasa atau pulang — bukan hal yang sama dengan diganti diet lain.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.DietService(konteks);

        var diet = await layanan.PrescribeAsync(Penetapan(h));

        var berhenti = await layanan.StopAsync(diet.Id, new StopGzDietRequest
        {
            Reason = "Pasien dipuasakan menjelang operasi",
            ExpectedVersion = diet.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        Assert.Equal(GziPatientDietStatus.Stopped, berhenti.Status);
        Assert.NotNull(berhenti.EndAt);
        Assert.Equal(diet.Version + 1, berhenti.Version);
    }

    [Fact]
    public async Task Diet_yang_sudah_berhenti_tidak_dapat_dihentikan_lagi()
    {
        // `GIZ004`.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.DietService(konteks);

        var diet = await layanan.PrescribeAsync(Penetapan(h));
        await layanan.StopAsync(diet.Id, new StopGzDietRequest
        {
            Reason = "Puasa",
            ExpectedVersion = diet.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            layanan.StopAsync(diet.Id, new StopGzDietRequest
            {
                Reason = "Puasa lagi",
                ExpectedVersion = diet.Version + 1,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("GIZ004", galat.Code);
    }

    [Fact]
    public async Task Diet_yang_sudah_diganti_tidak_dapat_dihentikan()
    {
        // Diet lama sudah berstatus `Changed`; menghentikannya akan menimpa riwayat
        // penggantiannya dengan alasan penghentian yang tidak pernah terjadi.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.DietService(konteks);

        var lama = await layanan.PrescribeAsync(Penetapan(h));
        await layanan.PrescribeAsync(Penetapan(h, alasan: "Diganti"));

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            layanan.StopAsync(lama.Id, new StopGzDietRequest
            {
                Reason = "Mau dihentikan",
                ExpectedVersion = lama.Version + 1,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("GIZ004", galat.Code);
    }

    [Fact]
    public async Task Versi_yang_tidak_cocok_saat_menghentikan_ditolak()
    {
        // `GIZ012`.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.DietService(konteks);

        var diet = await layanan.PrescribeAsync(Penetapan(h));

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            layanan.StopAsync(diet.Id, new StopGzDietRequest
            {
                Reason = "Puasa",
                ExpectedVersion = diet.Version + 7,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("GIZ012", galat.Code);
    }

    [Fact]
    public async Task Diet_yang_tidak_ada_tidak_dapat_dihentikan()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            h.DietService(konteks).StopAsync(Guid.NewGuid(), new StopGzDietRequest
            {
                Reason = "Apa pun",
                ExpectedVersion = 0,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));
    }

    [Fact]
    public async Task Setelah_dihentikan_diet_baru_tidak_lagi_menuntut_alasan_perubahan()
    {
        // Sisi lain `GIZ010`: yang menuntut alasan adalah adanya diet AKTIF. Setelah dihentikan,
        // kunjungan itu kembali kosong dan diet berikutnya adalah penetapan baru.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.DietService(konteks);

        var diet = await layanan.PrescribeAsync(Penetapan(h));
        await layanan.StopAsync(diet.Id, new StopGzDietRequest
        {
            Reason = "Puasa",
            ExpectedVersion = diet.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var baru = await layanan.PrescribeAsync(Penetapan(h));

        Assert.Equal(GziPatientDietStatus.Active, baru.Status);
    }

    [Fact]
    public async Task Riwayat_diet_kunjungan_tanpa_diet_kosong()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        Assert.Empty(await h.DietService(konteks).GetDietHistoryAsync(Guid.NewGuid()));
    }
}
