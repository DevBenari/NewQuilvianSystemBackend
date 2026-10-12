using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// Pengiriman tagihan obat ke invoice Billing pada transisi tahap 1 → 2.
/// </summary>
/// <remarks>
/// <para>
/// Produser ini satu-satunya jalan tagihan obat masuk ke <c>BilInvoiceItems</c>, dan surat
/// financial clearance Billing hanya menargetkan resep yang punya baris di sana. Kalau ia salah
/// bentuk atau melahirkan baris ganda, akibatnya bukan tagihan keliru saja: resep bisa tertahan
/// selamanya pada tahap 2, atau pasien tertagih dua kali untuk resep yang sama.
/// </para>
/// </remarks>
public class BillingChargeProducerTests
{
    private static PhmPrescription Resep(int jumlahItem = 2, decimal harga = 45000m) => new()
    {
        Id = Guid.NewGuid(),
        PrescriptionNumber = "UJI-RX-" + Guid.NewGuid().ToString("N")[..8],
        EncounterId = Guid.NewGuid(),
        PatientId = Guid.NewGuid(),
        DoctorId = Guid.NewGuid(),
        PrescriptionDateTime = DateTime.UtcNow,
        TotalItemCount = jumlahItem,
        TotalPrice = harga,
        PrescriptionStatus = PrescriptionStatus.Submitted,
        FulfillmentStatus = PrescriptionFulfillmentStatus.WaitingForPayment,
        IsActive = true,
        CreateDateTime = DateTime.UtcNow,
    };

    [Fact]
    public void Kunci_idempotensi_sama_untuk_resep_dan_revisi_yang_sama()
    {
        // Inilah yang membuat pengiriman ulang dikenali Billing sebagai replay. Kalau kuncinya
        // berubah tiap panggilan, finalisasi yang terkirim dua kali akan melahirkan dua baris
        // tagihan untuk satu resep.
        var resep = Guid.Parse("11111111-2222-3333-4444-555555555555");

        var a = PrescriptionBillingChargeProducer.DeterministicIdempotencyKey(resep, 1);
        var b = PrescriptionBillingChargeProducer.DeterministicIdempotencyKey(resep, 1);

        Assert.Equal(a, b);
        Assert.NotEqual(Guid.Empty, a);
    }

    [Fact]
    public void Kunci_idempotensi_berbeda_antar_resep()
    {
        var a = PrescriptionBillingChargeProducer.DeterministicIdempotencyKey(Guid.NewGuid(), 1);
        var b = PrescriptionBillingChargeProducer.DeterministicIdempotencyKey(Guid.NewGuid(), 1);

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Kunci_idempotensi_berbeda_antar_revisi()
    {
        // Revisi berikutnya memang harus memperbarui baris yang sama, bukan dianggap replay.
        var resep = Guid.NewGuid();

        Assert.NotEqual(
            PrescriptionBillingChargeProducer.DeterministicIdempotencyKey(resep, 1),
            PrescriptionBillingChargeProducer.DeterministicIdempotencyKey(resep, 2));
    }

    [Fact]
    public void Kunci_idempotensi_tidak_bergeser_dari_nilai_yang_sudah_terpakai()
    {
        // Kunci ini sudah tersimpan pada `BilChargeReceipts` milik Billing. Mengubah cara
        // menurunkannya membuat seluruh resep yang sudah pernah dikirim tampak belum terkirim,
        // dan pengiriman berikutnya melahirkan baris tagihan kedua.
        var kunci = PrescriptionBillingChargeProducer.DeterministicIdempotencyKey(
            Guid.Parse("4e4c6351-73aa-4cdf-84ad-f4f1c2936b92"), 1);

        Assert.Equal(Guid.Parse("363aa657-d512-9230-ca03-7d16b9fd8c4a"), kunci);
    }

    [Fact]
    public async Task Resep_tanpa_item_tidak_dikirim()
    {
        using var db = TestDatabase.Create();
        await using var konteks = db.CreateContext();
        var produser = new PrescriptionBillingChargeProducer(konteks, null!);

        var keluhan = await produser.SendForFinalizedPrescriptionAsync(
            Resep(jumlahItem: 0), Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid());

        Assert.Equal("PHM_CHARGE_NO_ITEM", keluhan);
    }

    [Fact]
    public async Task Resep_tanpa_harga_tidak_dikirim()
    {
        // Tagihan nol rupiah akan tetap menerbitkan surat clearance dan meloloskan resep tanpa
        // pasien membayar apa pun. Lebih baik dilaporkan sebagai keluhan daripada dikirim.
        using var db = TestDatabase.Create();
        await using var konteks = db.CreateContext();
        var produser = new PrescriptionBillingChargeProducer(konteks, null!);

        var keluhan = await produser.SendForFinalizedPrescriptionAsync(
            Resep(harga: 0m), Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid());

        Assert.Equal("PHM_CHARGE_NO_PRICE", keluhan);
    }

    [Fact]
    public async Task Tanpa_kategori_tarif_farmasi_pengiriman_dilaporkan_gagal()
    {
        // Tidak pernah menebak kategori: obat yang tertagih pada kelompok tagihan yang salah
        // lebih buruk daripada tagihan yang tertunda dan terlihat.
        using var db = TestDatabase.Create();
        await using var konteks = db.CreateContext();
        var produser = new PrescriptionBillingChargeProducer(konteks, null!);

        var keluhan = await produser.SendForFinalizedPrescriptionAsync(
            Resep(), Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid());

        Assert.Equal("PHM_CHARGE_NO_PHARMACY_CATEGORY", keluhan);
    }

    [Fact]
    public async Task Resep_null_ditolak_terang_terangan()
    {
        using var db = TestDatabase.Create();
        await using var konteks = db.CreateContext();
        var produser = new PrescriptionBillingChargeProducer(konteks, null!);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            produser.SendForFinalizedPrescriptionAsync(
                null!, Guid.NewGuid(), DateTimeOffset.UtcNow, Guid.NewGuid()));
    }

    [Fact]
    public void Status_sumber_dan_versi_kontrak_tetap_pada_nilai_yang_diterima_Billing()
    {
        // Keduanya diperiksa `ContractBillingChargeSourceAdapter` di sisi Billing. Menggesernya
        // membuat setiap pengiriman ditolak 422, dan resep kembali tertahan pada tahap 2.
        Assert.Equal("PRESCRIBED", PrescriptionBillingChargeProducer.PrescribedSourceStatus);
        Assert.Equal("BIL-INTEGRATION-1.3", PrescriptionBillingChargeProducer.ContractVersion);
    }

    /// <summary>
    /// Membangun satu permintaan tagihan berbentuk sama seperti yang dikirim producer, dengan
    /// status dan versi kontrak dapat diganti supaya penolakannya dapat diperiksa dua arah.
    /// </summary>
    private static UpsertChargeRequest Tagihan(string? status = null, string? kontrak = null) => new()
    {
        EncounterId = Guid.NewGuid(),
        SourceDomain = "PHARMACY",
        SourceDetailId = Guid.NewGuid().ToString("D"),
        SourceVersion = 1,
        SourceStatus = status ?? PrescriptionBillingChargeProducer.PrescribedSourceStatus,
        OccurredAt = DateTimeOffset.UtcNow,
        CategoryId = Guid.NewGuid(),
        DescriptionSnapshot = "Obat resep uji",
        Quantity = 1,
        UnitPrice = 50_000m,
        DoctorShare = 0,
        ContractVersion = kontrak ?? PrescriptionBillingChargeProducer.ContractVersion,
        CorrelationId = Guid.NewGuid(),
        CausationId = Guid.NewGuid()
    };

    /// <summary>
    /// Uji yang paling berharga di berkas ini: tagihan tahap 1 dari Farmasi dilewatkan ke
    /// <b>validator Billing yang sebenarnya</b>, bukan ke tiruan.
    /// </summary>
    /// <remarks>
    /// Mengunci nilai konstanta saja tidak pernah cukup. Kedua konstanta itu pernah bernilai
    /// <c>SUBMITTED</c> dan <c>BIL-INTEGRATION-1.2</c> — keduanya konsisten di dalam Farmasi,
    /// lulus seluruh uji Farmasi, dan tetap ditolak Billing pada setiap pengiriman karena
    /// Billing menanti <c>PRESCRIBED</c> pada kontrak <c>1.3</c>. Kegagalannya tidak terlihat
    /// dari dalam modul; hanya pertemuan kedua sisi yang menampakkannya.
    ///
    /// Validator Billing dipakai <b>apa adanya</b> dan tidak diubah sedikit pun.
    /// </remarks>
    [Fact]
    public void Tagihan_tahap_satu_diterima_validator_Billing_yang_sebenarnya()
    {
        var adapter = new ContractBillingChargeSourceAdapter();

        var snapshot = adapter.ValidateAndNormalize(Tagihan());

        Assert.Equal("PHARMACY", snapshot.SourceDomain);
        Assert.Equal("PRESCRIBED", snapshot.SourceStatus);
    }

    /// <summary>
    /// Pembuktian negatif: nilai lama memang ditolak. Tanpa ini, uji di atas bisa lulus karena
    /// validatornya permisif, bukan karena nilainya benar.
    /// </summary>
    [Theory]
    [InlineData("SUBMITTED", "BIL-INTEGRATION-1.3")]
    [InlineData("PRESCRIBED", "BIL-INTEGRATION-1.2")]
    [InlineData("SUBMITTED", "BIL-INTEGRATION-1.2")]
    public void Status_atau_kontrak_lama_ditolak_validator_Billing(string status, string kontrak)
    {
        var adapter = new ContractBillingChargeSourceAdapter();

        Assert.ThrowsAny<Exception>(() =>
            adapter.ValidateAndNormalize(Tagihan(status, kontrak)));
    }

    /// <summary>
    /// Tahap 2 tetap berlaku seperti sebelumnya: jumlah yang benar-benar diserahkan boleh
    /// ditagih pada kontrak mana pun, termasuk yang lama. Penyelarasan tahap 1 tidak boleh
    /// mempersempit jalur yang sudah ada.
    /// </summary>
    [Theory]
    [InlineData("BIL-INTEGRATION-1.2")]
    [InlineData("BIL-INTEGRATION-1.3")]
    public void Tagihan_tahap_dua_tetap_diterima_pada_kontrak_lama_dan_baru(string kontrak)
    {
        var adapter = new ContractBillingChargeSourceAdapter();

        var snapshot = adapter.ValidateAndNormalize(Tagihan("DISPENSED", kontrak));

        Assert.Equal("DISPENSED", snapshot.SourceStatus);
    }
}
