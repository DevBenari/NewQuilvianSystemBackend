using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Tangga tahap pemenuhan resep, dan regresi `GAP-PHA-BE-001`.
/// </summary>
/// <remarks>
/// <para>
/// Cacat yang dijaga di sini pernah membuat seluruh alur farmasi tidak pernah terbuka, dan tidak
/// terlihat sampai konsultasinya diselesaikan. Resep lahir ditandai <c>Draft</c> — artinya dokter
/// belum memfinalkannya — tetapi tahap pemenuhannya sudah disetel <c>WaitingForPayment</c>,
/// artinya finalisasi itu sudah lewat. Dua pernyataan yang bertentangan pada satu baris data.
/// </para>
/// <para>
/// Akibatnya baru muncul di tempat lain: finalisasi menuntut tahap
/// <c>WaitingForClinicalFinalization</c>, dan penolakannya berkeparahan <b>Error</b> yang tidak
/// dapat diakui, sehingga resep tertahan selamanya. Uji di bawah menjaga tangganya tetap urut:
/// tahap 1 saat lahir, tahap 2 hanya sebagai akibat finalisasi yang sah.
/// </para>
/// </remarks>
public class PrescriptionFulfillmentStageTests
{
    // Resep uji sengaja TIDAK dilekatkan ke konteks. Yang diuji di sini mesin statusnya, dan
    // `FinalizeFromConsultationAsync` memutuskannya atas entitas yang diberikan. Melekatkannya
    // hanya menuntut seluruh baris acuan — kunjungan, konsultasi, pasien, dokter — ikut dibuat,
    // tanpa menambah satu pun hal yang dibuktikan. `SaveChangesAsync` di dalam service menjadi
    // tanpa efek, dan perubahan statusnya tetap terjadi pada entitasnya.
    private static PhmPrescription ResepBaru() => new()
    {
        Id = Guid.NewGuid(),
        PrescriptionNumber = "UJI-RX-" + Guid.NewGuid().ToString("N")[..8],
        EncounterId = Guid.NewGuid(),
        ConsultationId = Guid.NewGuid(),
        PatientId = Guid.NewGuid(),
        DoctorId = Guid.NewGuid(),
        PrescriptionDateTime = DateTime.UtcNow,
        TotalItemCount = 1,
        IsActive = true,
        CreateDateTime = DateTime.UtcNow,
    };

    [Fact]
    public void Resep_baru_lahir_pada_tahap_menunggu_finalisasi_klinis()
    {
        // Regresi GAP-PHA-BE-001. Bila baris ini kembali menjadi WaitingForPayment, resep yang
        // dibuat lewat POST /prescriptions tidak akan pernah dapat difinalkan lagi.
        var resep = ResepBaru();

        PrescriptionWorkflowService.ApplyInitialClinicalState(resep);

        Assert.Equal(PrescriptionStatus.Draft, resep.PrescriptionStatus);
        Assert.Equal(PrescriptionPaymentStatus.NotBilled, resep.PaymentStatus);
        Assert.Equal(
            PrescriptionFulfillmentStatus.WaitingForClinicalFinalization,
            resep.FulfillmentStatus);
    }

    [Fact]
    public void Bawaan_entitas_juga_tahap_menunggu_finalisasi_klinis()
    {
        // Lapisan kedua: bahkan tanpa memanggil apa pun, entitasnya tidak boleh lahir melewati
        // finalisasi klinis.
        Assert.Equal(
            PrescriptionFulfillmentStatus.WaitingForClinicalFinalization,
            new PhmPrescription().FulfillmentStatus);
    }

    [Fact]
    public async Task Finalisasi_konsultasi_menaikkan_tahap_ke_menunggu_pembayaran()
    {
        using var db = TestDatabase.Create();
        await using var konteks = db.CreateContext();
        var layanan = new PrescriptionWorkflowService(konteks);

        var resep = ResepBaru();
        PrescriptionWorkflowService.ApplyInitialClinicalState(resep);

        var hasil = await layanan.FinalizeFromConsultationAsync(resep, Guid.NewGuid(), DateTime.UtcNow);

        Assert.True(hasil.IsSuccess, hasil.ErrorMessage);
        Assert.Equal(PrescriptionStatus.Submitted, resep.PrescriptionStatus);
        Assert.Equal(PrescriptionFulfillmentStatus.WaitingForPayment, resep.FulfillmentStatus);
        Assert.NotNull(resep.SubmittedAt);
    }

    [Fact]
    public async Task Resep_yang_lahir_pada_tahap_pembayaran_tidak_dapat_difinalkan()
    {
        // Inilah bentuk cacatnya, dijaga apa adanya: bila suatu saat ada jalur pembuatan baru
        // yang kembali menaruh tahap 2 saat lahir, uji ini yang menjelaskan akibatnya.
        using var db = TestDatabase.Create();
        await using var konteks = db.CreateContext();
        var layanan = new PrescriptionWorkflowService(konteks);

        var resep = ResepBaru();
        resep.PrescriptionStatus = PrescriptionStatus.Draft;
        resep.PaymentStatus = PrescriptionPaymentStatus.NotBilled;
        resep.FulfillmentStatus = PrescriptionFulfillmentStatus.WaitingForPayment;

        var hasil = await layanan.FinalizeFromConsultationAsync(resep, Guid.NewGuid(), DateTime.UtcNow);

        Assert.False(hasil.IsSuccess);
        Assert.Contains("tidak valid untuk finalisasi klinis", hasil.ErrorMessage ?? "");
        Assert.Equal(PrescriptionStatus.Draft, resep.PrescriptionStatus);
    }

    [Fact]
    public async Task Finalisasi_yang_diulang_ditolak_tanpa_merusak_keadaan()
    {
        using var db = TestDatabase.Create();
        await using var konteks = db.CreateContext();
        var layanan = new PrescriptionWorkflowService(konteks);

        var resep = ResepBaru();
        PrescriptionWorkflowService.ApplyInitialClinicalState(resep);

        var pertama = await layanan.FinalizeFromConsultationAsync(resep, Guid.NewGuid(), DateTime.UtcNow);
        Assert.True(pertama.IsSuccess, pertama.ErrorMessage);
        var diajukanPada = resep.SubmittedAt;

        var kedua = await layanan.FinalizeFromConsultationAsync(resep, Guid.NewGuid(), DateTime.UtcNow);

        Assert.False(kedua.IsSuccess);
        Assert.Equal(PrescriptionStatus.Submitted, resep.PrescriptionStatus);
        Assert.Equal(PrescriptionFulfillmentStatus.WaitingForPayment, resep.FulfillmentStatus);
        Assert.Equal(diajukanPada, resep.SubmittedAt);
    }

    [Fact]
    public async Task Resep_tanpa_obat_tidak_dapat_difinalkan()
    {
        using var db = TestDatabase.Create();
        await using var konteks = db.CreateContext();
        var layanan = new PrescriptionWorkflowService(konteks);

        var resep = ResepBaru();
        resep.TotalItemCount = 0;
        PrescriptionWorkflowService.ApplyInitialClinicalState(resep);

        var hasil = await layanan.FinalizeFromConsultationAsync(resep, Guid.NewGuid(), DateTime.UtcNow);

        Assert.False(hasil.IsSuccess);
        Assert.Contains("belum memiliki item obat", hasil.ErrorMessage ?? "");
        Assert.Equal(
            PrescriptionFulfillmentStatus.WaitingForClinicalFinalization,
            resep.FulfillmentStatus);
    }

    [Fact]
    public void Resep_tidak_dapat_diajukan_di_luar_finalisasi_konsultasi()
    {
        // Pengajuan terpisah sengaja ditutup: resep terbit bersama catatan dokternya.
        using var db = TestDatabase.Create();
        using var konteks = db.CreateContext();
        var layanan = new PrescriptionWorkflowService(konteks);

        var hasil = layanan.SubmitAsync(ResepBaru(), Guid.NewGuid(), DateTime.UtcNow).Result;

        Assert.False(hasil.IsSuccess);
        Assert.Contains("Selesaikan konsultasi dokter", hasil.ErrorMessage ?? "");
    }

    [Fact]
    public void Tangga_tahap_pemenuhan_tetap_pada_angka_yang_sama()
    {
        // Angka tahap ikut tersimpan di basis data dan dibaca frontend. Menggesernya memutus
        // seluruh pembacaan status yang sudah tersimpan.
        Assert.Equal(1, (int)PrescriptionFulfillmentStatus.WaitingForClinicalFinalization);
        Assert.Equal(2, (int)PrescriptionFulfillmentStatus.WaitingForPayment);
        Assert.Equal(3, (int)PrescriptionFulfillmentStatus.ReadyForPharmacy);
        Assert.Equal(4, (int)PrescriptionFulfillmentStatus.QueuedAtPharmacy);
        Assert.Equal(5, (int)PrescriptionFulfillmentStatus.VerifiedByPharmacy);
        Assert.Equal(6, (int)PrescriptionFulfillmentStatus.InPreparation);
        Assert.Equal(7, (int)PrescriptionFulfillmentStatus.ReadyToDispense);
        Assert.Equal(12, (int)PrescriptionFulfillmentStatus.AwaitingFinalCheck);
    }
}
