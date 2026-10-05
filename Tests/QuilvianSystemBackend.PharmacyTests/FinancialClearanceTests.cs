using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Konsumsi surat financial clearance Billing oleh Farmasi (`PHA-BE-005`, `PHA-BE-006`).
/// </summary>
/// <remarks>
/// <para>
/// Inilah satu-satunya jalan resep berpindah dari tahap 2 ke tahap 4, dan satu-satunya sumber
/// izin bagi keempat gerbang kerja apoteker. Kalau ia terlalu longgar, obat diserahkan sebelum
/// pasien membayar; kalau terlalu ketat, resep tertahan selamanya.
/// </para>
/// <para>
/// Penerbit suratnya milik Billing dan tidak disentuh di sini — surat disisipkan sebagai fixture,
/// tetapi konsumsinya dijalankan service produksi.
/// </para>
/// </remarks>
public class FinancialClearanceTests
{
    [Fact]
    public async Task Tanpa_surat_resep_tertahan_dengan_alasan_belum_diketahui()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        var keadaan = await h.ClearanceService(k).DescribeAsync(h.ResepId);

        Assert.False(keadaan.IsCleared);
        Assert.Equal(PrescriptionClearanceProjectionStatuses.Unknown, keadaan.ClearanceStatus);
        Assert.Equal(ClearanceHoldReasonCodes.Unknown, keadaan.HoldReasonCode);
        Assert.Equal(0, keadaan.FinancialVersion);
    }

    [Fact]
    public async Task Surat_cleared_dan_paid_memindahkan_resep_ke_antrean_farmasi()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        var hasil = await h.TerimaSuratAsync(k);

        Assert.Equal(1, hasil.Cleared);
        Assert.Equal(1, hasil.ReleasedToQueue);

        var resep = await h.ResepAsync(k);
        Assert.Equal(PrescriptionFulfillmentStatus.QueuedAtPharmacy, resep.FulfillmentStatus);
        Assert.Equal(PrescriptionPaymentStatus.Paid, resep.PaymentStatus);
    }

    [Theory]
    [InlineData(PrescriptionFinancialOutcomes.Paid, PrescriptionPaymentStatus.Paid)]
    [InlineData(PrescriptionFinancialOutcomes.InsuranceApproved, PrescriptionPaymentStatus.InsuranceApproved)]
    [InlineData(PrescriptionFinancialOutcomes.PaymentWaived, PrescriptionPaymentStatus.PaymentWaived)]
    public async Task Ketiga_hasil_finansial_disalin_apa_adanya(string hasilSurat,
        PrescriptionPaymentStatus diharapkan)
    {
        // Hasil penjaminan tercatat sebagai disetujui penjamin, bukan lunas tunai. Menyamakan
        // ketiganya akan membuat laporan penerimaan kasir menghitung piutang penjamin sebagai
        // uang yang sudah masuk.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        await h.TerimaSuratAsync(k, hasil: hasilSurat);

        Assert.Equal(diharapkan, (await h.ResepAsync(k)).PaymentStatus);
    }

    [Fact]
    public async Task Hasil_finansial_di_luar_ketiga_nilai_tidak_dapat_tersimpan_sama_sekali()
    {
        // Lapisan pertama justru di basis data: `CK_BilPrescriptionClearanceHandoff_FinancialOutcome`
        // hanya mengizinkan NULL, PAID, INSURANCE_APPROVED, dan PAYMENT_WAIVED. Jadi surat
        // berhasil karangan tidak pernah sampai ke Farmasi untuk ditafsirkan.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        await Assert.ThrowsAnyAsync<Exception>(() =>
            h.TerimaSuratAsync(k, hasil: "SESUATU_YANG_TIDAK_DIKENAL"));
    }

    [Fact]
    public async Task Surat_cleared_tanpa_hasil_finansial_gagal_tertutup()
    {
        // `PHA-DEC-067`. Inilah satu-satunya bentuk hasil tak dikenali yang dapat lolos basis
        // data: `CLEARED` dengan outcome NULL. Surat menyatakan boleh dikerjakan tetapi tidak
        // menyebut hasilnya; Farmasi tidak menebak. Salinan dicatat, izinnya tidak diberikan,
        // dan resep tidak dipindahkan.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        var hasil = await h.TerimaSuratAsync(k, hasil: null);

        Assert.Equal(1, hasil.FailedUnrecognizedOutcome);
        Assert.Equal(0, hasil.Cleared);

        Assert.Equal(PrescriptionFulfillmentStatus.WaitingForPayment,
            (await h.ResepAsync(k)).FulfillmentStatus);
        Assert.False((await h.ClearanceService(k).DescribeAsync(h.ResepId)).IsCleared);
    }

    [Fact]
    public async Task Surat_revoked_tidak_menurunkan_tahap_pemenuhan()
    {
        // Pencabutan menahan pekerjaan lewat gerbang, bukan dengan memundurkan tahap. Memundurkan
        // tahap akan mengulang antrean dan telaah yang sudah selesai.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        await h.TerimaSuratAsync(k);
        var sebelum = (await h.ResepAsync(k)).FulfillmentStatus;

        var hasil = await h.TerimaSuratAsync(k,
            status: PrescriptionClearanceProjectionStatuses.Revoked,
            hasil: null, versi: 2, alasan: "PAYMENT_REVERSED");

        Assert.Equal(1, hasil.Revoked);
        Assert.Equal(sebelum, (await h.ResepAsync(k)).FulfillmentStatus);
    }

    [Fact]
    public async Task Pencabutan_menutup_gerbang_telaah()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        await h.TerimaSuratAsync(k);
        await h.TerimaSuratAsync(k,
            status: PrescriptionClearanceProjectionStatuses.Revoked,
            hasil: null, versi: 2, alasan: "PAYMENT_REVERSED");

        var gerbang = await h.ClearanceService(k)
            .EvaluateGateAsync(h.ResepId, PrescriptionClearanceGate.Review);

        Assert.False(gerbang.Allowed);
    }

    [Fact]
    public async Task Surat_dengan_versi_lebih_rendah_tidak_pernah_dibaca()
    {
        // Surat dapat tiba tidak berurutan, dan versi lama yang diterapkan akan menghidupkan
        // kembali izin yang sudah dicabut. Penjagaannya dua lapis, dan lapisan pertama berada
        // pada penyaring pembacaan: surat bernomor versi tidak lebih tinggi daripada salinan
        // tidak ikut terambil sama sekali — bukan terambil lalu diabaikan. Tanpa penyaring itu,
        // sapuan bertahap akan mengambil surat terlama berulang kali dan tidak pernah maju.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        await h.TerimaSuratAsync(k, versi: 5);
        var hasil = await h.TerimaSuratAsync(k, versi: 3);

        Assert.Equal(0, hasil.Cleared);
        Assert.Equal(0, hasil.ReleasedToQueue);
        Assert.Equal(5, (await h.ClearanceService(k).DescribeAsync(h.ResepId)).FinancialVersion);
    }

    [Fact]
    public async Task Dua_surat_bernomor_versi_sama_tidak_dapat_hidup_bersama()
    {
        // Ditegakkan indeks unik `(PrescriptionId, FinancialVersion)` pada tabel Billing. Dua
        // surat bernomor sama berarti urutan penerapannya bergantung pada kebetulan.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        await h.TerimaSuratAsync(k, versi: 2);

        await Assert.ThrowsAnyAsync<Exception>(() => h.TerimaSuratAsync(k, versi: 2));
    }

    [Fact]
    public async Task Surat_untuk_resep_yang_tidak_dikenal_dilewati_tanpa_galat()
    {
        // Suratnya tetap utuh di sisi Billing; Farmasi hanya tidak punya resep untuk disalin.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        var asing = Guid.NewGuid();
        k.Set<BilPrescriptionClearanceHandoff>().Add(new BilPrescriptionClearanceHandoff
        {
            Id = Guid.NewGuid(),
            PrescriptionId = asing,
            InvoiceId = h.InvoiceId,
            ClearanceStatus = PrescriptionClearanceProjectionStatuses.Cleared,
            FinancialOutcome = PrescriptionFinancialOutcomes.Paid,
            ReasonCode = "INVOICE_SETTLED",
            FinancialVersion = 1,
            EffectiveAt = DateTimeOffset.UtcNow,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            Status = BillingHandoffStatuses.Created,
            CreateDateTime = DateTime.UtcNow
        });
        await k.SaveChangesAsync();

        var hasil = await h.ClearanceService(k).ConsumeForPrescriptionAsync(asing, h.ApotekerId);

        Assert.Equal(1, hasil.SkippedUnknownPrescription);
    }

    [Fact]
    public async Task Konsumsi_berulang_tidak_memindahkan_resep_dua_kali()
    {
        // Idempotensi consumer. Pemanggilan kedua tidak boleh melahirkan salinan kedua maupun
        // menaikkan tahap lagi — `GET` detail resep memanggil consumer ini setiap kali dibuka.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        await h.TerimaSuratAsync(k);
        var ulang = await h.ClearanceService(k).ConsumeForPrescriptionAsync(h.ResepId, h.ApotekerId);

        Assert.Equal(0, ulang.ReleasedToQueue);
        Assert.Equal(PrescriptionFulfillmentStatus.QueuedAtPharmacy,
            (await h.ResepAsync(k)).FulfillmentStatus);
        Assert.Single(k.PhmPrescriptionFinancialProjections.Where(x => x.PrescriptionId == h.ResepId));
    }

    [Fact]
    public async Task Resep_yang_sudah_bergerak_lebih_jauh_tidak_dikembalikan_ke_antrean()
    {
        // `PHA-DEC-069`. Surat pemulihan tidak mengulang telaah maupun penyiapan yang sudah
        // selesai; hanya resep yang masih menunggu pembayaran yang dipindahkan.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        await h.SampaiTelaahDisetujuiAsync(k);
        var sebelum = (await h.ResepAsync(k)).FulfillmentStatus;

        var hasil = await h.TerimaSuratAsync(k, versi: 9);

        Assert.Equal(0, hasil.ReleasedToQueue);
        Assert.Equal(sebelum, (await h.ResepAsync(k)).FulfillmentStatus);
    }

    [Fact]
    public async Task Clearance_tidak_mengubah_data_klinis_resep()
    {
        // Keadaan finansial tidak boleh menyentuh isi klinis. Yang berubah hanya tahap pemenuhan
        // dan salinan status pembayaran.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        var sebelum = await h.ResepAsync(k);
        var nomor = sebelum.PrescriptionNumber;
        var jumlahItem = sebelum.TotalItemCount;
        var harga = sebelum.TotalPrice;
        var statusKlinis = sebelum.PrescriptionStatus;
        var dokter = sebelum.DoctorId;

        await h.TerimaSuratAsync(k);

        var sesudah = await h.ResepAsync(k);
        Assert.Equal(nomor, sesudah.PrescriptionNumber);
        Assert.Equal(jumlahItem, sesudah.TotalItemCount);
        Assert.Equal(harga, sesudah.TotalPrice);
        Assert.Equal(statusKlinis, sesudah.PrescriptionStatus);
        Assert.Equal(dokter, sesudah.DoctorId);
    }

    [Fact]
    public async Task Keempat_gerbang_terbuka_setelah_clearance_sah()
    {
        // Gerbangnya empat, bukan hanya penyerahan — menjaga penyerahan saja berarti membiarkan
        // apoteker meracik obat yang pembayarannya belum beres.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);

        var layanan = h.ClearanceService(k);

        foreach (var gerbang in Enum.GetValues<PrescriptionClearanceGate>())
        {
            Assert.True((await layanan.EvaluateGateAsync(h.ResepId, gerbang)).Allowed,
                $"Gerbang {gerbang} seharusnya terbuka.");
        }
    }

    [Fact]
    public async Task Keempat_gerbang_tertutup_tanpa_clearance()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        var layanan = h.ClearanceService(k);

        foreach (var gerbang in Enum.GetValues<PrescriptionClearanceGate>())
        {
            var hasil = await layanan.EvaluateGateAsync(h.ResepId, gerbang);
            Assert.False(hasil.Allowed, $"Gerbang {gerbang} seharusnya tertutup.");
            Assert.NotNull(hasil.Message);
        }
    }

    [Fact]
    public async Task EnsureGateAllowed_melempar_ketika_tertutup()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            h.ClearanceService(k).EnsureGateAllowedAsync(
                h.ResepId, PrescriptionClearanceGate.Review));
    }

    [Fact]
    public async Task EnsureGateAllowed_lolos_tanpa_melempar_setelah_clearance()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);

        await h.ClearanceService(k).EnsureGateAllowedAsync(
            h.ResepId, PrescriptionClearanceGate.Review);
    }

    [Fact]
    public async Task Sapuan_batch_mengkonsumsi_surat_yang_menggantung()
    {
        // Jalur pemulihan ketika consumer per-resep tidak pernah terpanggil karena layarnya
        // tidak dibuka siapa pun.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        k.Set<BilPrescriptionClearanceHandoff>().Add(new BilPrescriptionClearanceHandoff
        {
            Id = Guid.NewGuid(),
            PrescriptionId = h.ResepId,
            InvoiceId = h.InvoiceId,
            ClearanceStatus = PrescriptionClearanceProjectionStatuses.Cleared,
            FinancialOutcome = PrescriptionFinancialOutcomes.Paid,
            ReasonCode = "INVOICE_SETTLED",
            FinancialVersion = 1,
            EffectiveAt = DateTimeOffset.UtcNow,
            CorrelationId = Guid.NewGuid(),
            CausationId = Guid.NewGuid(),
            Status = BillingHandoffStatuses.Created,
            CreateDateTime = DateTime.UtcNow
        });
        await k.SaveChangesAsync();

        var hasil = await h.ClearanceService(k).ConsumePendingAsync(h.ApotekerId, 50);

        Assert.Equal(1, hasil.Cleared);
        Assert.Equal(PrescriptionFulfillmentStatus.QueuedAtPharmacy,
            (await h.ResepAsync(k)).FulfillmentStatus);
    }
}
