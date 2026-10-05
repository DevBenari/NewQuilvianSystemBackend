using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Rantai kerja apoteker: telaah resep, penyiapan, dan telaah obat akhir.
/// </summary>
/// <remarks>
/// Ketiganya berantai, dan tiap mata rantai menjaga satu hal yang tidak dapat diperbaiki
/// sesudahnya. Telaah yang dapat disetujui dengan hard stop terbuka berarti obat berinteraksi
/// berat lolos; penyiapan yang dapat dimulai sebelum telaah disetujui berarti obat diracik
/// sebelum diperiksa; telaah akhir yang dapat dilewati berarti obat sampai ke pasien tanpa
/// pemeriksaan terakhir.
/// </remarks>
public class PharmacyWorkflowTests
{
    // ------------------------------------------------------------------ telaah

    [Fact]
    public async Task Telaah_tidak_dapat_dimulai_sebelum_clearance()
    {
        // Gerbang finansial pertama dari empat (`PHA-BE-005`). Telaah tidak dimulai selama kasir
        // belum memastikan pembayarannya.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            h.ReviewService(k).StartAsync(h.ResepId, h.ApotekerId, null));
    }

    [Fact]
    public async Task Telaah_dimulai_setelah_clearance_dan_memuat_kriteria_aktif()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);

        var review = await h.ReviewService(k).StartAsync(h.ResepId, h.ApotekerId, null);

        Assert.Equal(PrescriptionReviewStatus.InReview, review.Status);
        Assert.Equal(1, review.ReviewVersion);

        // Dua kriteria aktif ikut, yang tidak aktif tidak.
        Assert.Equal(2, review.Items.Count);
        Assert.DoesNotContain("UJI-CRIT-OFF", review.Items.Select(x => x.CriterionCode));
    }

    [Fact]
    public async Task Telaah_menaikkan_tahap_ke_antrean_farmasi()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);

        await h.ReviewService(k).StartAsync(h.ResepId, h.ApotekerId, null);

        Assert.Equal(PrescriptionFulfillmentStatus.QueuedAtPharmacy,
            (await h.ResepAsync(k)).FulfillmentStatus);
    }

    [Fact]
    public async Task Resep_yang_belum_diajukan_dokter_tidak_dapat_ditelaah()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);

        var resep = await h.ResepAsync(k);
        resep.PrescriptionStatus = PrescriptionStatus.Draft;
        await k.SaveChangesAsync();

        var galat = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            h.ReviewService(k).StartAsync(h.ResepId, h.ApotekerId, null));

        Assert.Contains("sudah diajukan dokter", galat.Message);
    }

    [Fact]
    public async Task Resep_yang_tidak_ada_tidak_dapat_ditelaah()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            h.ReviewService(k).StartAsync(Guid.NewGuid(), h.ApotekerId, null));
    }

    [Fact]
    public async Task Memulai_telaah_dua_kali_memakai_ulang_telaah_yang_sama()
    {
        // Bukan menerbitkan versi telaah kedua: apoteker yang membuka layar dua kali tidak
        // sedang menelaah ulang resep.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);
        var layanan = h.ReviewService(k);

        var pertama = await layanan.StartAsync(h.ResepId, h.ApotekerId, null);
        var kedua = await layanan.StartAsync(h.ResepId, h.ApotekerId, null);

        Assert.Equal(pertama.Id, kedua.Id);
        Assert.Equal(1, kedua.ReviewVersion);
    }

    [Fact]
    public async Task Telaah_tidak_dapat_diselesaikan_selama_ada_kriteria_belum_ditelaah()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);
        var layanan = h.ReviewService(k);

        var review = await layanan.StartAsync(h.ResepId, h.ApotekerId, null);

        var galat = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            layanan.CompleteAsync(review.Id, approve: true, null, h.ApotekerId));

        Assert.Contains("Seluruh kriteria wajib ditelaah", galat.Message);
    }

    [Fact]
    public async Task Telaah_yang_seluruh_kriterianya_sesuai_dapat_disetujui()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);
        var layanan = h.ReviewService(k);

        var review = await layanan.StartAsync(h.ResepId, h.ApotekerId, null);
        await h.SetujuiSeluruhKriteriaAsync(k, review.Id);

        var selesai = await layanan.CompleteAsync(review.Id, approve: true, null, h.ApotekerId);

        Assert.Equal(PrescriptionReviewStatus.Approved, selesai.Status);
        Assert.Equal(PrescriptionFulfillmentStatus.VerifiedByPharmacy,
            (await h.ResepAsync(k)).FulfillmentStatus);
    }

    [Fact]
    public async Task Telaah_dengan_hard_stop_tidak_dapat_disetujui()
    {
        // Hard stop adalah temuan yang tidak boleh dilewati apoteker sendiri — jalan keluarnya
        // klarifikasi ke dokter, bukan persetujuan.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);
        var layanan = h.ReviewService(k);

        var review = await layanan.StartAsync(h.ResepId, h.ApotekerId, null);
        var hardStop = review.Items.Single(x => x.CriterionCode == "UJI-CRIT-2");

        await layanan.UpdateItemsAsync(review.Id, new UpdatePrescriptionReviewItemsRequest
        {
            Items = [.. review.Items.Select(x => new UpdatePrescriptionReviewItemRequest
            {
                ReviewItemId = x.Id,
                Result = x.Id == hardStop.Id
                    ? PrescriptionReviewResult.NotCompliant
                    : PrescriptionReviewResult.Compliant,
                Severity = x.Id == hardStop.Id ? PrescriptionIssueSeverity.HardStop : null
            })]
        }, h.ApotekerId);

        var galat = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            layanan.CompleteAsync(review.Id, approve: true, null, h.ApotekerId));

        Assert.Contains("hard stop", galat.Message);
    }

    [Fact]
    public async Task Telaah_dapat_ditolak_walaupun_ada_hard_stop()
    {
        // Penolakan memang jalan keluarnya; yang dilarang hanya menyetujui.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);
        var layanan = h.ReviewService(k);

        var review = await layanan.StartAsync(h.ResepId, h.ApotekerId, null);
        await h.SetujuiSeluruhKriteriaAsync(k, review.Id);

        var ditolak = await layanan.CompleteAsync(review.Id, approve: false, "Ditolak", h.ApotekerId);

        Assert.NotEqual(PrescriptionReviewStatus.Approved, ditolak.Status);
        Assert.NotEqual(PrescriptionFulfillmentStatus.VerifiedByPharmacy,
            (await h.ResepAsync(k)).FulfillmentStatus);
    }

    [Fact]
    public async Task Kriteria_yang_tidak_dikenal_ditolak()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);
        var layanan = h.ReviewService(k);

        var review = await layanan.StartAsync(h.ResepId, h.ApotekerId, null);

        var galat = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            layanan.UpdateItemsAsync(review.Id, new UpdatePrescriptionReviewItemsRequest
            {
                Items =
                [
                    new UpdatePrescriptionReviewItemRequest
                    {
                        ReviewItemId = Guid.NewGuid(),
                        Result = PrescriptionReviewResult.Compliant
                    }
                ]
            }, h.ApotekerId));

        Assert.Contains("kriteria telaah tidak ditemukan", galat.Message);
    }

    [Fact]
    public async Task Telaah_yang_sudah_selesai_tidak_dapat_diubah()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);
        var layanan = h.ReviewService(k);

        var review = await layanan.StartAsync(h.ResepId, h.ApotekerId, null);
        await h.SetujuiSeluruhKriteriaAsync(k, review.Id);
        await layanan.CompleteAsync(review.Id, approve: true, null, h.ApotekerId);

        var galat = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            layanan.UpdateItemsAsync(review.Id, new UpdatePrescriptionReviewItemsRequest
            {
                Items = []
            }, h.ApotekerId));

        Assert.Contains("sudah selesai tidak dapat diubah", galat.Message);
    }

    // ------------------------------------------------------- klarifikasi dokter

    [Fact]
    public async Task Klarifikasi_dapat_dibuka_lalu_dijawab_dan_ditutup()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);
        var layanan = h.ReviewService(k);

        var review = await layanan.StartAsync(h.ResepId, h.ApotekerId, null);

        var klarifikasi = await layanan.CreateClarificationAsync(review.Id,
            new CreatePrescriptionClarificationRequest { ProblemCode = "UJI-PRB-1", ProblemDescription = "Dosis dikonfirmasi?" },
            h.ApotekerId);

        var dijawab = await layanan.RespondClarificationAsync(klarifikasi.Id,
            new DoctorClarificationResponseRequest { DoctorResponse = "Dosis benar" }, h.DokterId);
        Assert.NotNull(dijawab.DoctorResponse);

        var ditutup = await layanan.CloseClarificationAsync(klarifikasi.Id, new ClosePrescriptionClarificationRequest { Accepted = true }, h.ApotekerId);
        Assert.NotNull(ditutup.ClosedAt);
    }

    [Fact]
    public async Task Klarifikasi_terbuka_menahan_persetujuan_telaah()
    {
        // Sisi lain aturan hard stop: pertanyaan ke dokter yang belum tuntas juga menahan.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);
        var layanan = h.ReviewService(k);

        var review = await layanan.StartAsync(h.ResepId, h.ApotekerId, null);
        await h.SetujuiSeluruhKriteriaAsync(k, review.Id);
        await layanan.CreateClarificationAsync(review.Id,
            new CreatePrescriptionClarificationRequest { ProblemCode = "UJI-PRB-2", ProblemDescription = "Mohon konfirmasi" },
            h.ApotekerId);

        var galat = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            layanan.CompleteAsync(review.Id, approve: true, null, h.ApotekerId));

        Assert.Contains("klarifikasi terbuka", galat.Message);
    }

    [Fact]
    public async Task Klarifikasi_yang_sudah_ditutup_tidak_dapat_dijawab_lagi()
    {
        // Regresi `BUG-PHA-BE-002`. Penjaga lamanya hanya memeriksa status `Closed` dan
        // `Cancelled`, padahal `CloseClarificationAsync` menyetel `AcceptedByPharmacist` atau
        // `Rejected` — jadi ia tidak pernah menyala lewat jalur penutupan normal, dan jawaban
        // dokter yang tiba terlambat memundurkan telaah yang sudah beres menjadi
        // `RevisedByDoctor` sementara klarifikasinya tetap tampak tertutup di layar.
        //
        // Penjaga sekarang `ClosedAt != null`, yang terisi pada setiap jalur penutupan.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);
        var layanan = h.ReviewService(k);

        var review = await layanan.StartAsync(h.ResepId, h.ApotekerId, null);
        var klarifikasi = await layanan.CreateClarificationAsync(review.Id,
            new CreatePrescriptionClarificationRequest { ProblemCode = "UJI-PRB-3", ProblemDescription = "Tanya" }, h.ApotekerId);

        var ditutup = await layanan.CloseClarificationAsync(klarifikasi.Id,
            new ClosePrescriptionClarificationRequest { Accepted = true }, h.ApotekerId);
        Assert.Equal(PrescriptionClarificationStatus.AcceptedByPharmacist, ditutup.Status);
        Assert.NotNull(ditutup.ClosedAt);

        var galat = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            layanan.RespondClarificationAsync(klarifikasi.Id,
                new DoctorClarificationResponseRequest { DoctorResponse = "Jawab" }, h.DokterId));

        Assert.Contains("sudah ditutup", galat.Message);
    }

    [Fact]
    public async Task Klarifikasi_yang_ditolak_apoteker_juga_tidak_dapat_dijawab_lagi()
    {
        // Sisi lain regresi yang sama: `Rejected` pun mengisi `ClosedAt`, jadi pintunya tertutup
        // tanpa perlu status itu didaftarkan tersendiri.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);
        var layanan = h.ReviewService(k);

        var review = await layanan.StartAsync(h.ResepId, h.ApotekerId, null);
        var klarifikasi = await layanan.CreateClarificationAsync(review.Id,
            new CreatePrescriptionClarificationRequest
            {
                ProblemCode = "UJI-PRB-4",
                ProblemDescription = "Tanya lagi"
            }, h.ApotekerId);

        await layanan.CloseClarificationAsync(klarifikasi.Id,
            new ClosePrescriptionClarificationRequest { Accepted = false }, h.ApotekerId);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            layanan.RespondClarificationAsync(klarifikasi.Id,
                new DoctorClarificationResponseRequest { DoctorResponse = "Jawab" }, h.DokterId));
    }

    [Fact]
    public async Task Telaah_tidak_mundur_karena_jawaban_dokter_yang_terlambat()
    {
        // Akibat yang sebenarnya dijaga `BUG-PHA-BE-002`: bukan hanya penolakan responsnya,
        // melainkan bahwa status telaah tidak kembali ke `RevisedByDoctor` sesudah ditutup.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);
        var layanan = h.ReviewService(k);

        var review = await layanan.StartAsync(h.ResepId, h.ApotekerId, null);
        var klarifikasi = await layanan.CreateClarificationAsync(review.Id,
            new CreatePrescriptionClarificationRequest
            {
                ProblemCode = "UJI-PRB-5",
                ProblemDescription = "Tanya"
            }, h.ApotekerId);

        await layanan.CloseClarificationAsync(klarifikasi.Id,
            new ClosePrescriptionClarificationRequest { Accepted = true }, h.ApotekerId);

        var sesudahTutup = await layanan.GetActiveAsync(h.ResepId);
        Assert.NotEqual(PrescriptionReviewStatus.RevisedByDoctor, sesudahTutup!.Status);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            layanan.RespondClarificationAsync(klarifikasi.Id,
                new DoctorClarificationResponseRequest { DoctorResponse = "Terlambat" }, h.DokterId));

        var sesudahJawab = await layanan.GetActiveAsync(h.ResepId);
        Assert.Equal(sesudahTutup.Status, sesudahJawab!.Status);
    }

    [Fact]
    public async Task Jawaban_yang_sama_dikirim_ulang_sebelum_penutupan_aman()
    {
        // Acceptance ketiga `BUG-PHA-BE-002`: pengiriman ulang tidak melahirkan transisi kedua.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);
        var layanan = h.ReviewService(k);

        var review = await layanan.StartAsync(h.ResepId, h.ApotekerId, null);
        var klarifikasi = await layanan.CreateClarificationAsync(review.Id,
            new CreatePrescriptionClarificationRequest
            {
                ProblemCode = "UJI-PRB-6",
                ProblemDescription = "Tanya"
            }, h.ApotekerId);

        var jawaban = new DoctorClarificationResponseRequest { DoctorResponse = "Dosis benar" };

        var pertama = await layanan.RespondClarificationAsync(klarifikasi.Id, jawaban, h.DokterId);
        var kedua = await layanan.RespondClarificationAsync(klarifikasi.Id, jawaban, h.DokterId);

        Assert.Equal(pertama.Status, kedua.Status);
        Assert.Equal(pertama.DoctorResponse, kedua.DoctorResponse);
    }

    [Fact]
    public async Task Pengganti_penjaga_itu_adalah_penolakan_klarifikasi_yang_tidak_ada()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        var galat = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            h.ReviewService(k).CloseClarificationAsync(Guid.NewGuid(),
                new ClosePrescriptionClarificationRequest { Accepted = true }, h.ApotekerId));

        Assert.Contains("Klarifikasi tidak ditemukan", galat.Message);
    }

    [Fact]
    public async Task Klarifikasi_yang_tidak_ada_tidak_dapat_dijawab()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            h.ReviewService(k).RespondClarificationAsync(Guid.NewGuid(),
                new DoctorClarificationResponseRequest { DoctorResponse = "Jawab" }, h.DokterId));
    }

    // ------------------------------------------------------------------ penyiapan

    [Fact]
    public async Task Penyiapan_tidak_dapat_dimulai_sebelum_telaah_disetujui()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.TerimaSuratAsync(k);

        var galat = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            h.PreparationService(k).StartAsync(h.ResepId, h.ApotekerId, null));

        Assert.Contains("setelah telaah farmasi disetujui", galat.Message);
    }

    [Fact]
    public async Task Penyiapan_dimulai_setelah_telaah_disetujui()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiTelaahDisetujuiAsync(k);

        var penyiapan = await h.PreparationService(k).StartAsync(h.ResepId, h.ApotekerId, null);

        Assert.Equal(PrescriptionPreparationStatus.InPreparation, penyiapan.Status);
        Assert.Equal(PrescriptionFulfillmentStatus.InPreparation,
            (await h.ResepAsync(k)).FulfillmentStatus);
    }

    [Fact]
    public async Task Penyiapan_tidak_dapat_diselesaikan_sebelum_dimulai()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiTelaahDisetujuiAsync(k);

        var galat = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            h.PreparationService(k).CompleteAsync(h.ResepId,
                new CompletePrescriptionPreparationRequest { Items = [h.ButirPenyiapan()] },
                h.ApotekerId));

        Assert.Contains("belum dimulai", galat.Message);
    }

    [Fact]
    public async Task Penyiapan_selesai_menaikkan_tahap_ke_menunggu_telaah_akhir()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiTelaahDisetujuiAsync(k);
        var layanan = h.PreparationService(k);

        await layanan.StartAsync(h.ResepId, h.ApotekerId, null);
        var selesai = await layanan.CompleteAsync(h.ResepId,
            new CompletePrescriptionPreparationRequest { Items = [h.ButirPenyiapan()] },
            h.ApotekerId);

        Assert.Equal(PrescriptionPreparationStatus.Prepared, selesai.Status);
        Assert.Equal(PrescriptionFulfillmentStatus.AwaitingFinalCheck,
            (await h.ResepAsync(k)).FulfillmentStatus);
    }

    [Fact]
    public async Task Baris_penyiapan_harus_merujuk_item_resep_atau_bahan_racikan()
    {
        // Baris yang tidak merujuk apa pun membuat jumlah yang disiapkan tidak dapat
        // dibandingkan dengan yang diresepkan.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiTelaahDisetujuiAsync(k);
        var layanan = h.PreparationService(k);

        await layanan.StartAsync(h.ResepId, h.ApotekerId, null);

        var butir = h.ButirPenyiapan();
        butir.PrescriptionItemId = null;

        var galat = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            layanan.CompleteAsync(h.ResepId,
                new CompletePrescriptionPreparationRequest { Items = [butir] }, h.ApotekerId));

        Assert.Contains("item resep reguler atau bahan racikan", galat.Message);
    }

    [Fact]
    public async Task Jumlah_teoretis_dan_jumlah_sebenarnya_tersimpan_terpisah()
    {
        // Keduanya disimpan apa adanya, bukan salah satu: tanpa jumlah teoretis, selisih antara
        // yang seharusnya dan yang benar-benar dipakai hilang, dan jejak obat yang terbuang
        // tidak dapat ditelusuri. `WasteQuantity` sendiri dikirim pelaksana, bukan dihitung
        // sistem — uji ini tidak mengarang perhitungan yang tidak ada di source.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiTelaahDisetujuiAsync(k);
        var layanan = h.PreparationService(k);

        await layanan.StartAsync(h.ResepId, h.ApotekerId, null);
        await layanan.CompleteAsync(h.ResepId,
            new CompletePrescriptionPreparationRequest
            {
                Items = [h.ButirPenyiapan(teori: 10, aktual: 12)]
            }, h.ApotekerId);

        // Diperiksa pada keadaan tersimpan, bukan pada respons. Respons penyiapan masih memuat
        // baris lama yang baru ditandai terhapus pada permintaan yang sama, karena koleksinya
        // dipetakan apa adanya dari induk yang sedang dilacak.
        var baris = k.Set<TrxPrescriptionPreparationItem>()
            .Single(x => x.PrescriptionItemId == h.ItemResepId && !x.IsDelete);

        Assert.Equal(10, baris.TheoreticalQuantity);
        Assert.Equal(12, baris.ActualQuantity);
    }

    [Fact]
    public async Task Penyiapan_tidak_dapat_dimulai_dua_kali()
    {
        // Gerbangnya menolak begitu tahapnya bukan lagi `VerifiedByPharmacy` — jadi memanggil
        // `start` ulang untuk memulihkan keadaan tidak bekerja, dan layar harus membaca tahap
        // dari resepnya.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiTelaahDisetujuiAsync(k);
        var layanan = h.PreparationService(k);

        await layanan.StartAsync(h.ResepId, h.ApotekerId, null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            layanan.StartAsync(h.ResepId, h.ApotekerId, null));
    }

    // ------------------------------------------------------------- telaah akhir

    [Fact]
    public async Task Telaah_akhir_hanya_pada_tahap_menunggu_telaah_akhir()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiTelaahDisetujuiAsync(k);

        var galat = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            h.FinalCheckService(k).CompleteAsync(h.ResepId,
                new CompletePrescriptionFinalCheckRequest
                {
                    Items = [Kriteria("ACC-1", "Ketepatan obat")]
                }, h.ApotekerId));

        Assert.Contains("menunggu telaah obat akhir", galat.Message);
    }

    [Fact]
    public async Task Telaah_akhir_wajib_memuat_kriteria()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiMenungguTelaahAkhirAsync(k);

        var galat = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            h.FinalCheckService(k).CompleteAsync(h.ResepId,
                new CompletePrescriptionFinalCheckRequest { Items = [] }, h.ApotekerId));

        Assert.Contains("wajib diisi", galat.Message);
    }

    [Fact]
    public async Task Telaah_akhir_lolos_menaikkan_tahap_ke_siap_diserahkan()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();
        await h.SampaiMenungguTelaahAkhirAsync(k);

        await h.FinalCheckService(k).CompleteAsync(h.ResepId,
            new CompletePrescriptionFinalCheckRequest
            {
                Items = [Kriteria("ACC-1", "Ketepatan obat"), Kriteria("ACC-2", "Ketepatan dosis")]
            }, h.ApotekerId);

        Assert.Equal(PrescriptionFulfillmentStatus.ReadyToDispense,
            (await h.ResepAsync(k)).FulfillmentStatus);
    }

    [Fact]
    public async Task Resep_yang_tidak_ada_tidak_dapat_ditelaah_akhir()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            h.FinalCheckService(k).CompleteAsync(Guid.NewGuid(),
                new CompletePrescriptionFinalCheckRequest
                {
                    Items = [Kriteria("ACC-1", "Ketepatan obat")]
                }, h.ApotekerId));
    }

    // ------------------------------------------------------- tangga tahap penuh

    [Fact]
    public async Task Tangga_tahap_berjalan_berurutan_dari_dua_ke_tujuh()
    {
        // Rantai penuh dalam satu uji, supaya urutannya sendiri ikut terjaga — bukan hanya
        // tiap transisinya secara terpisah.
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        Assert.Equal(PrescriptionFulfillmentStatus.WaitingForPayment,
            (await h.ResepAsync(k)).FulfillmentStatus);

        await h.TerimaSuratAsync(k);
        Assert.Equal(PrescriptionFulfillmentStatus.QueuedAtPharmacy,
            (await h.ResepAsync(k)).FulfillmentStatus);

        var telaah = h.ReviewService(k);
        var review = await telaah.StartAsync(h.ResepId, h.ApotekerId, null);
        await h.SetujuiSeluruhKriteriaAsync(k, review.Id);
        await telaah.CompleteAsync(review.Id, approve: true, null, h.ApotekerId);
        Assert.Equal(PrescriptionFulfillmentStatus.VerifiedByPharmacy,
            (await h.ResepAsync(k)).FulfillmentStatus);

        var penyiapan = h.PreparationService(k);
        await penyiapan.StartAsync(h.ResepId, h.ApotekerId, null);
        Assert.Equal(PrescriptionFulfillmentStatus.InPreparation,
            (await h.ResepAsync(k)).FulfillmentStatus);

        await penyiapan.CompleteAsync(h.ResepId,
            new CompletePrescriptionPreparationRequest { Items = [h.ButirPenyiapan()] },
            h.ApotekerId);
        Assert.Equal(PrescriptionFulfillmentStatus.AwaitingFinalCheck,
            (await h.ResepAsync(k)).FulfillmentStatus);

        await h.FinalCheckService(k).CompleteAsync(h.ResepId,
            new CompletePrescriptionFinalCheckRequest
            {
                Items = [Kriteria("ACC-1", "Ketepatan obat")]
            }, h.ApotekerId);
        Assert.Equal(PrescriptionFulfillmentStatus.ReadyToDispense,
            (await h.ResepAsync(k)).FulfillmentStatus);
    }

    [Fact]
    public async Task Seluruh_rantai_tidak_mengubah_data_klinis_resep()
    {
        using var h = new PharmacyHarness();
        await using var k = h.CreateContext();

        var sebelum = await h.ResepAsync(k);
        var nomor = sebelum.PrescriptionNumber;
        var item = sebelum.TotalItemCount;
        var harga = sebelum.TotalPrice;
        var dokter = sebelum.DoctorId;
        var pasien = sebelum.PatientId;

        await h.SampaiMenungguTelaahAkhirAsync(k);

        var sesudah = await h.ResepAsync(k);
        Assert.Equal(nomor, sesudah.PrescriptionNumber);
        Assert.Equal(item, sesudah.TotalItemCount);
        Assert.Equal(harga, sesudah.TotalPrice);
        Assert.Equal(dokter, sesudah.DoctorId);
        Assert.Equal(pasien, sesudah.PatientId);
        Assert.Equal(PrescriptionStatus.Submitted, sesudah.PrescriptionStatus);
    }

    private static CompletePrescriptionFinalCheckItemRequest Kriteria(string kode, string nama)
        => new()
        {
            CriterionCode = kode,
            CriterionName = nama,
            Result = PrescriptionReviewResult.Compliant
        };
}
