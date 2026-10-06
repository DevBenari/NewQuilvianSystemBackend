using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Tests.Nutrition.Infrastructure;

namespace QuilvianSystemBackend.Tests.Nutrition;

/// <summary>
/// Daur hidup order konsultasi gizi: pembuatan, perubahan, penutupan, pembatalan.
/// </summary>
/// <remarks>
/// Order inilah yang membuka seluruh asuhan gizi satu pasien. Kalau ia bisa lahir ganda pada satu
/// episode rawat, dua ahli gizi akan merawat pasien yang sama tanpa saling tahu; kalau ia bisa
/// diubah setelah ditutup, catatan asuhan yang sudah selesai berubah di belakang.
/// </remarks>
public class OrderRuleTests
{
    private static CreateGzOrderRequest Pembuatan(NutritionHarness h, string? kunci = null,
        string alasan = "Skrining gizi berisiko") => new()
        {
            PatientId = h.PasienId,
            EncounterId = h.KunjunganRawatId,
            RequesterDoctorId = h.DokterId,
            AssignedWorkforceId = h.AhliGiziId,
            Priority = GziOrderPriority.Routine,
            ReasonForReferral = alasan,
            IdempotencyKey = kunci ?? Guid.NewGuid().ToString("N")
        };

    // ------------------------------------------------------------------ pembuatan

    [Fact]
    public async Task Order_valid_lahir_pada_status_requested()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var hasil = await h.OrderService(konteks).CreateAsync(Pembuatan(h));

        Assert.Equal(GziOrderStatus.Requested, hasil.Status);
        Assert.Equal(h.PasienId, hasil.PatientId);
        Assert.StartsWith("GZ-", hasil.OrderNumber);
        Assert.Equal(0, hasil.Version);
    }

    [Fact]
    public async Task Alasan_rujukan_wajib_diisi()
    {
        // `GIZ003`. Order tanpa alasan membuat ahli gizi menerima rujukan tanpa tahu mengapa.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.OrderService(konteks).CreateAsync(Pembuatan(h, alasan: "   ")));

        Assert.Equal("GIZ003", galat.Code);
    }

    [Fact]
    public async Task Kunci_idempotensi_wajib_diisi()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            h.OrderService(konteks).CreateAsync(Pembuatan(h, kunci: "  ")));
    }

    [Fact]
    public async Task Kunjungan_yang_bukan_milik_pasien_ditolak()
    {
        // `GIZ001`. Bukan sekadar "kunjungan ada": kunjungan harus milik pasien yang dirujuk.
        // Tanpa pemeriksaan itu, asuhan gizi bisa menempel pada episode rawat orang lain.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Pembuatan(h);
        permintaan.PatientId = Guid.NewGuid();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.OrderService(konteks).CreateAsync(permintaan));

        Assert.Equal("GIZ001", galat.Code);
    }

    [Fact]
    public async Task Kunjungan_yang_tidak_ada_ditolak()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Pembuatan(h);
        permintaan.EncounterId = Guid.NewGuid();

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.OrderService(konteks).CreateAsync(permintaan));

        Assert.Equal("GIZ001", galat.Code);
    }

    [Fact]
    public async Task Dokter_pemohon_yang_tidak_aktif_ditolak()
    {
        // Harness memuat satu dokter tidak aktif justru untuk ini: tanpa baris itu, aturannya
        // hanya terbukti memeriksa keberadaan, bukan keaktifan.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Pembuatan(h);
        permintaan.RequesterDoctorId = h.DokterNonaktifId;

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.OrderService(konteks).CreateAsync(permintaan));

        Assert.Equal("GIZ001", galat.Code);
    }

    [Fact]
    public async Task Ahli_gizi_yang_tidak_aktif_ditolak()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Pembuatan(h);
        permintaan.AssignedWorkforceId = h.AhliGiziNonaktifId;

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            h.OrderService(konteks).CreateAsync(permintaan));

        Assert.Equal("GIZ001", galat.Code);
    }

    [Fact]
    public async Task Order_boleh_lahir_tanpa_ahli_gizi_yang_ditugaskan()
    {
        // Penugasan menyusul; dokter merujuk lebih dulu. Kontraknya memang nullable, dan uji ini
        // menjaga agar pemeriksaan keaktifan tidak ikut menolak keadaan "belum ditugaskan".
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        var permintaan = Pembuatan(h);
        permintaan.AssignedWorkforceId = null;

        var hasil = await h.OrderService(konteks).CreateAsync(permintaan);

        Assert.Null(hasil.AssignedWorkforceId);
    }

    [Fact]
    public async Task Satu_episode_rawat_hanya_boleh_punya_satu_order_berjalan()
    {
        // `GIZ002`. Dua order berjalan pada satu episode berarti dua ahli gizi merawat pasien
        // yang sama tanpa saling tahu, dan riwayat asuhannya terpecah dua.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.OrderService(konteks);

        await layanan.CreateAsync(Pembuatan(h));

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            layanan.CreateAsync(Pembuatan(h)));

        Assert.Equal("GIZ002", galat.Code);
    }

    [Fact]
    public async Task Order_baru_boleh_dibuat_setelah_order_sebelumnya_ditutup()
    {
        // Sisi lain `GIZ002`: yang dilarang order yang masih BERJALAN, bukan riwayatnya. Pasien
        // yang dirujuk ulang pada episode yang sama harus tetap bisa dibuatkan order.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.OrderService(konteks);

        var pertama = await layanan.CreateAsync(Pembuatan(h));
        await layanan.CloseAsync(pertama.Id, new CloseGzOrderRequest
        {
            ClosingNote = "Asuhan selesai",
            ExpectedVersion = pertama.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var kedua = await layanan.CreateAsync(Pembuatan(h));

        Assert.Equal(GziOrderStatus.Requested, kedua.Status);
        Assert.NotEqual(pertama.Id, kedua.Id);
    }

    [Fact]
    public async Task Pembuatan_ulang_dengan_kunci_sama_mengembalikan_order_yang_sama()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.OrderService(konteks);

        var kunci = Guid.NewGuid().ToString("N");
        var pertama = await layanan.CreateAsync(Pembuatan(h, kunci));
        var kedua = await layanan.CreateAsync(Pembuatan(h, kunci));

        Assert.Equal(pertama.Id, kedua.Id);
    }

    [Fact]
    public async Task Kunci_idempotensi_yang_dipakai_untuk_isi_berbeda_ditolak()
    {
        // `GIZ013`. Mengembalikan order lama untuk permintaan yang berbeda membuat pemohonnya
        // yakin rujukan barunya tercatat, padahal tidak.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.OrderService(konteks);

        var kunci = Guid.NewGuid().ToString("N");
        await layanan.CreateAsync(Pembuatan(h, kunci));

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            layanan.CreateAsync(Pembuatan(h, kunci, alasan: "Alasan lain")));

        Assert.Equal("GIZ013", galat.Code);
    }

    // ------------------------------------------------------------------ perubahan

    [Fact]
    public async Task Perubahan_order_menaikkan_versi()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.OrderService(konteks);

        var order = await layanan.CreateAsync(Pembuatan(h));

        var sesudah = await layanan.UpdateAsync(order.Id, new UpdateGzOrderRequest
        {
            AssignedWorkforceId = h.AhliGiziId,
            Priority = GziOrderPriority.Urgent,
            ReasonForReferral = "Berat badan turun cepat",
            ExpectedVersion = order.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        Assert.Equal(order.Version + 1, sesudah.Version);
        Assert.Equal(GziOrderPriority.Urgent, sesudah.Priority);
    }

    [Fact]
    public async Task Versi_yang_tidak_cocok_ditolak()
    {
        // `GIZ012`. Dua petugas yang membuka layar yang sama tidak boleh saling menimpa tanpa
        // ada yang tahu; yang kedua harus memuat ulang lebih dulu.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.OrderService(konteks);

        var order = await layanan.CreateAsync(Pembuatan(h));

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            layanan.UpdateAsync(order.Id, new UpdateGzOrderRequest
            {
                Priority = GziOrderPriority.Urgent,
                ReasonForReferral = "Berubah",
                ExpectedVersion = order.Version + 5,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("GIZ012", galat.Code);
    }

    [Fact]
    public async Task Order_yang_tidak_ada_tidak_dapat_diubah()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            h.OrderService(konteks).UpdateAsync(Guid.NewGuid(), new UpdateGzOrderRequest
            {
                Priority = GziOrderPriority.Routine,
                ReasonForReferral = "Apa pun",
                ExpectedVersion = 0,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));
    }

    // ------------------------------------------------------- penutupan dan pembatalan

    [Fact]
    public async Task Penutupan_memerlukan_catatan_penutup()
    {
        // `GIZ008`. Asuhan gizi ditutup dengan kesimpulan, bukan sekadar tombol.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.OrderService(konteks);

        var order = await layanan.CreateAsync(Pembuatan(h));

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            layanan.CloseAsync(order.Id, new CloseGzOrderRequest
            {
                ClosingNote = "  ",
                ExpectedVersion = order.Version,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("GIZ008", galat.Code);
    }

    [Fact]
    public async Task Penutupan_menyimpan_catatan_dan_waktu()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.OrderService(konteks);

        var order = await layanan.CreateAsync(Pembuatan(h));

        var ditutup = await layanan.CloseAsync(order.Id, new CloseGzOrderRequest
        {
            ClosingNote = "Target asupan tercapai",
            ExpectedVersion = order.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        Assert.Equal(GziOrderStatus.Closed, ditutup.Status);
        Assert.Equal("Target asupan tercapai", ditutup.ClosingNote);
        Assert.NotNull(ditutup.ClosedAt);
    }

    [Fact]
    public async Task Pembatalan_memerlukan_alasan()
    {
        // `GIZ009`.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.OrderService(konteks);

        var order = await layanan.CreateAsync(Pembuatan(h));

        var galat = await Assert.ThrowsAsync<NutritionUnprocessableException>(() =>
            layanan.CancelAsync(order.Id, new CancelGzOrderRequest
            {
                Reason = "",
                ExpectedVersion = order.Version,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("GIZ009", galat.Code);
    }

    [Fact]
    public async Task Pembatalan_yang_beralasan_menutup_order()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.OrderService(konteks);

        var order = await layanan.CreateAsync(Pembuatan(h));

        var dibatalkan = await layanan.CancelAsync(order.Id, new CancelGzOrderRequest
        {
            Reason = "Pasien pulang atas permintaan sendiri",
            ExpectedVersion = order.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        Assert.Equal(GziOrderStatus.Cancelled, dibatalkan.Status);
    }

    [Theory]
    [InlineData(true)]   // ditutup
    [InlineData(false)]  // dibatalkan
    public async Task Order_yang_sudah_selesai_tidak_dapat_diubah_lagi(bool lewatPenutupan)
    {
        // `GIZ004`. Order yang sudah ditutup atau dibatalkan adalah catatan, bukan pekerjaan
        // berjalan; mengubahnya berarti mengubah riwayat asuhan yang sudah selesai.
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.OrderService(konteks);

        var order = await layanan.CreateAsync(Pembuatan(h));

        if (lewatPenutupan)
        {
            await layanan.CloseAsync(order.Id, new CloseGzOrderRequest
            {
                ClosingNote = "Selesai",
                ExpectedVersion = order.Version,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            });
        }
        else
        {
            await layanan.CancelAsync(order.Id, new CancelGzOrderRequest
            {
                Reason = "Batal",
                ExpectedVersion = order.Version,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            });
        }

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            layanan.UpdateAsync(order.Id, new UpdateGzOrderRequest
            {
                Priority = GziOrderPriority.Urgent,
                ReasonForReferral = "Masih mau diubah",
                ExpectedVersion = order.Version + 1,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("GIZ004", galat.Code);
    }

    [Fact]
    public async Task Order_yang_sudah_ditutup_tidak_dapat_ditutup_ulang()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.OrderService(konteks);

        var order = await layanan.CreateAsync(Pembuatan(h));
        await layanan.CloseAsync(order.Id, new CloseGzOrderRequest
        {
            ClosingNote = "Selesai",
            ExpectedVersion = order.Version,
            IdempotencyKey = Guid.NewGuid().ToString("N")
        });

        var galat = await Assert.ThrowsAsync<NutritionConflictException>(() =>
            layanan.CloseAsync(order.Id, new CloseGzOrderRequest
            {
                ClosingNote = "Selesai lagi",
                ExpectedVersion = order.Version + 1,
                IdempotencyKey = Guid.NewGuid().ToString("N")
            }));

        Assert.Equal("GIZ004", galat.Code);
    }

    [Fact]
    public async Task Penutupan_yang_diulang_dengan_kunci_sama_tidak_berubah_dua_kali()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.OrderService(konteks);

        var order = await layanan.CreateAsync(Pembuatan(h));
        var kunci = Guid.NewGuid().ToString("N");

        var pertama = await layanan.CloseAsync(order.Id, new CloseGzOrderRequest
        {
            ClosingNote = "Selesai",
            ExpectedVersion = order.Version,
            IdempotencyKey = kunci
        });

        var kedua = await layanan.CloseAsync(order.Id, new CloseGzOrderRequest
        {
            ClosingNote = "Selesai",
            ExpectedVersion = order.Version,
            IdempotencyKey = kunci
        });

        Assert.Equal(pertama.Version, kedua.Version);
        Assert.Equal(GziOrderStatus.Closed, kedua.Status);
    }

    // ------------------------------------------------------------------ pembacaan

    [Fact]
    public async Task Detail_order_yang_tidak_ada_mengembalikan_null()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();

        Assert.Null(await h.OrderService(konteks).GetDetailAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task Daftar_order_memuat_order_yang_baru_dibuat()
    {
        using var h = new NutritionHarness();
        await using var konteks = h.CreateContext();
        var layanan = h.OrderService(konteks);

        var order = await layanan.CreateAsync(Pembuatan(h));

        var halaman = await layanan.GetPagedAsync(
            new GziOrderPagedQuery { PageNumber = 1, PageSize = 50 });

        Assert.Contains(order.Id, halaman.Items.Select(x => x.Id));
    }
}
