using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;

namespace QuilvianSystemBackend.Tests.Pharmacy;

/// <summary>
/// <c>PHA-DEC-066</c> — Stock Reservation Authority V1, dan <c>PHA-BE-003</c>.
/// </summary>
/// <remarks>
/// <para>
/// Keputusan ini memajukan penahanan stok dari penyiapan Farmasi ke finalisasi resep. Uji di
/// sini membuktikan kedelapan butirnya terhadap service produksi, bukan terhadap tiruan.
/// </para>
/// <para>
/// Harness Farmasi menyemai kunjungan tanpa jenis layanan dan depo tanpa unit layanan, karena
/// uji lain tidak membutuhkannya. Penahanan membutuhkan depo pelayanan authoritative, jadi
/// kedua hal itu dilengkapi di sini lewat <see cref="SiapkanRoutingAsync"/> — memakai aturan
/// `PHA-DEPOT-ROUTING-v1` yang sudah ada, bukan melewatinya.
/// </para>
/// </remarks>
public class PrescriptionStockReservationTests
{
    private static readonly Guid UnitRawatJalan = Guid.NewGuid();

    /// <summary>
    /// Melengkapi kunjungan dan depo supaya routing dapat memberi jawaban tepat satu.
    /// </summary>
    /// <param name="jenis">
    /// Jenis kunjungan. Menentukan apakah penahanan memiliki batas waktu:
    /// <c>PHA-DEC-066</c> butir 8 mengecualikan rawat inap dan IGD.
    /// </param>
    private static async Task SiapkanRoutingAsync(
        PharmacyHarness harness, ApplicationDbContext konteks,
        EncounterType jenis = EncounterType.Outpatient)
    {
        var kunjungan = await konteks.Set<RegPatientEncounter>()
            .FirstAsync(x => x.Id == harness.KunjunganId);
        kunjungan.EncounterType = jenis;
        kunjungan.ServiceUnitId = UnitRawatJalan;
        kunjungan.IsActive = true;

        var depo = await konteks.Set<MstDrugStorageLocation>()
            .FirstAsync(x => x.Id == harness.DepoId);
        depo.ServiceUnitId = UnitRawatJalan;
        depo.IsPharmacyLocation = true;
        depo.StorageLocationType = jenis == EncounterType.Emergency ? "emergency" : "pharmacy";
        depo.IsMainWarehouse = false;
        depo.IsQuarantineLocation = false;

        await konteks.SaveChangesAsync();
    }

    private static async Task<PhmPrescription> ResepDraftAsync(
        ApplicationDbContext konteks, Guid resepId)
    {
        var resep = await konteks.Set<PhmPrescription>().FirstAsync(x => x.Id == resepId);
        resep.PrescriptionStatus = PrescriptionStatus.Draft;
        await konteks.SaveChangesAsync();
        return resep;
    }

    private static PrescriptionStockReservationService Layanan(ApplicationDbContext k)
        => new(k, new PharmacyDepotRoutingService(k));

    private static async Task<List<PhmPrescriptionStockReservation>> PenahananAsync(
        ApplicationDbContext konteks, Guid resepId)
        => await konteks.Set<PhmPrescriptionStockReservation>()
            .Where(x => x.PrescriptionId == resepId)
            .ToListAsync();

    // ───────────────────────────────────────── butir 1 — penahanan saat finalisasi

    [Fact]
    public async Task StokCukup_PenahananDibuat()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        await harness.SediakanStokAsync(konteks, 100, denganPenahanan: false);
        await SiapkanRoutingAsync(harness, konteks);
        var resep = await ResepDraftAsync(konteks, harness.ResepId);

        var hasil = await Layanan(konteks).ReserveForPrescriptionAsync(
            resep, harness.DokterId, DateTime.UtcNow, "uji-1", "UJI");
        await konteks.SaveChangesAsync();

        Assert.True(hasil.IsSuccess, hasil.Message);
        Assert.Equal(10m, hasil.TotalQuantity);
        Assert.Equal(harness.DepoId, hasil.StorageLocationId);

        // Penghitung saldo lama tetap menjadi sumber kebenaran jumlah.
        var saldo = await konteks.Set<PhmDrugStockBalance>().FirstAsync();
        Assert.Equal(10m, saldo.QuantityReserved);
    }

    [Fact]
    public async Task StokKurang_GagalDanNolPenahananParsial()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        // Butir resep meminta 10; tersedia hanya 4.
        await harness.SediakanStokAsync(konteks, 4, denganPenahanan: false);
        await SiapkanRoutingAsync(harness, konteks);
        var resep = await ResepDraftAsync(konteks, harness.ResepId);

        var hasil = await Layanan(konteks).ReserveForPrescriptionAsync(
            resep, harness.DokterId, DateTime.UtcNow, "uji-2", "UJI");

        Assert.False(hasil.IsSuccess);
        Assert.Equal(PrescriptionReservationErrorCodes.InsufficientStock, hasil.Code);
        Assert.Contains("Kurang 6", hasil.Message);

        // Tidak disimpan: pemanggil produksi membatalkan transaksinya. Yang dibuktikan di sini
        // adalah hasilnya gagal sebelum satu pun baris tersimpan.
        Assert.Empty(await PenahananAsync(konteks, harness.ResepId));
    }

    [Fact]
    public async Task DuaResepBerebutStok_HanyaJumlahYangValidBerhasil()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        // Tersedia 12. Resep pertama meminta 10, sisa 2 — resep kedua yang meminta 10 ditolak.
        await harness.SediakanStokAsync(konteks, 12, denganPenahanan: false);
        await SiapkanRoutingAsync(harness, konteks);
        var resep = await ResepDraftAsync(konteks, harness.ResepId);

        var pertama = await Layanan(konteks).ReserveForPrescriptionAsync(
            resep, harness.DokterId, DateTime.UtcNow, "uji-3a", "UJI");
        await konteks.SaveChangesAsync();

        Assert.True(pertama.IsSuccess);

        var resepKedua = await BuatResepKeduaAsync(harness, konteks, 10);

        var kedua = await Layanan(konteks).ReserveForPrescriptionAsync(
            resepKedua, harness.DokterId, DateTime.UtcNow, "uji-3b", "UJI");

        Assert.False(kedua.IsSuccess);
        Assert.Equal(PrescriptionReservationErrorCodes.InsufficientStock, kedua.Code);

        // Resep pertama tidak terganggu.
        var saldo = await konteks.Set<PhmDrugStockBalance>().FirstAsync();
        Assert.Equal(10m, saldo.QuantityReserved);
    }

    [Fact]
    public async Task RetryFinalisasi_TidakMenahanDuaKali()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        await harness.SediakanStokAsync(konteks, 100, denganPenahanan: false);
        await SiapkanRoutingAsync(harness, konteks);
        var resep = await ResepDraftAsync(konteks, harness.ResepId);

        var layanan = Layanan(konteks);
        var kunci = $"PHA-RSV:{resep.Id}";

        await layanan.ReserveForPrescriptionAsync(resep, harness.DokterId, DateTime.UtcNow, kunci, "UJI");
        await konteks.SaveChangesAsync();

        var ulang = await layanan.ReserveForPrescriptionAsync(
            resep, harness.DokterId, DateTime.UtcNow, kunci, "UJI");
        await konteks.SaveChangesAsync();

        Assert.True(ulang.IsSuccess);
        Assert.True(ulang.WasAlreadyReserved);

        var saldo = await konteks.Set<PhmDrugStockBalance>().FirstAsync();
        Assert.Equal(10m, saldo.QuantityReserved);
        Assert.Single(await PenahananAsync(konteks, harness.ResepId));
    }

    // ───────────────────────────────────────── butir 2 dan 7 — pembayaran

    [Fact]
    public async Task PembayaranValid_PenahananTetapAktif()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        await SiapkanAktifAsync(harness, konteks);
        var layanan = Layanan(konteks);

        var hasil = await layanan.ConfirmFinanciallyClearedAsync(
            harness.ResepId, harness.ApotekerId, DateTime.UtcNow);
        await konteks.SaveChangesAsync();

        Assert.True(hasil.IsSuccess, hasil.Message);

        var keadaan = await layanan.EnsureActiveAsync(harness.ResepId, DateTime.UtcNow);
        Assert.True(keadaan.IsSuccess);
    }

    [Fact]
    public async Task PembayaranSebelumBatasWaktu_PenahananTidakLagiDilepasOtomatis()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        var mulai = DateTime.UtcNow;
        await SiapkanAktifAsync(harness, konteks, mulai);
        var layanan = Layanan(konteks);

        await layanan.ConfirmFinanciallyClearedAsync(harness.ResepId, harness.ApotekerId, mulai);
        await konteks.SaveChangesAsync();

        // Lima jam kemudian — melewati TTL 4 jam — penyapu tidak boleh menyentuhnya lagi.
        var dilepas = await layanan.ReleaseExpiredAsync(mulai.AddHours(5));
        await konteks.SaveChangesAsync();

        Assert.Equal(0, dilepas);

        var baris = Assert.Single(await PenahananAsync(konteks, harness.ResepId));
        Assert.Equal(PrescriptionReservationState.Reserved, baris.State);
        Assert.Null(baris.ExpiresAt);
    }

    [Fact]
    public async Task BatasWaktuEmpatJamSebelumPembayaran_Dilepas()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        var mulai = DateTime.UtcNow;
        await SiapkanAktifAsync(harness, konteks, mulai);

        var dilepas = await Layanan(konteks).ReleaseExpiredAsync(mulai.AddHours(4).AddMinutes(1));
        await konteks.SaveChangesAsync();

        Assert.Equal(1, dilepas);

        var baris = Assert.Single(await PenahananAsync(konteks, harness.ResepId));
        Assert.Equal(PrescriptionReservationState.Expired, baris.State);
        Assert.Equal(
            PrescriptionReservationReleaseReasons.ExpiredBeforePayment,
            baris.ReleaseReasonCode);

        // Stok kembali tersedia untuk pasien lain.
        var saldo = await konteks.Set<PhmDrugStockBalance>().FirstAsync();
        Assert.Equal(0m, saldo.QuantityReserved);
    }

    [Fact]
    public async Task PembayaranSetelahKedaluwarsa_TidakLangsungSiapFarmasi()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        var mulai = DateTime.UtcNow;
        await SiapkanAktifAsync(harness, konteks, mulai);
        var layanan = Layanan(konteks);

        await layanan.ReleaseExpiredAsync(mulai.AddHours(5));
        await konteks.SaveChangesAsync();

        var hasil = await layanan.ConfirmFinanciallyClearedAsync(
            harness.ResepId, harness.ApotekerId, mulai.AddHours(5));

        Assert.False(hasil.IsSuccess);
        Assert.Equal(PrescriptionReservationErrorCodes.ReservationExpired, hasil.Code);
        // Keadaan bisnis, bukan galat internal: pesannya menjelaskan apa yang harus dilakukan.
        Assert.Contains("ditahan ulang", hasil.Message);
    }

    // ───────────────────────────────────────── butir 4 dan 5 — konsumsi dan pelepasan

    [Fact]
    public async Task Penyerahan_MengonsumsiPenahananSekali()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        await SiapkanAktifAsync(harness, konteks);
        var layanan = Layanan(konteks);

        var pertama = await layanan.ConsumeAsync(harness.ResepId, harness.ApotekerId, DateTime.UtcNow);
        await konteks.SaveChangesAsync();

        Assert.True(pertama.IsSuccess);

        var baris = Assert.Single(await PenahananAsync(konteks, harness.ResepId));
        Assert.Equal(PrescriptionReservationState.Consumed, baris.State);
        Assert.NotNull(baris.ConsumedAt);
    }

    [Fact]
    public async Task PenyerahanDiulang_TidakMengonsumsiDuaKali()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        await SiapkanAktifAsync(harness, konteks);
        var layanan = Layanan(konteks);

        await layanan.ConsumeAsync(harness.ResepId, harness.ApotekerId, DateTime.UtcNow);
        await konteks.SaveChangesAsync();

        var kedua = await layanan.ConsumeAsync(harness.ResepId, harness.ApotekerId, DateTime.UtcNow);

        Assert.False(kedua.IsSuccess);
        Assert.Equal(PrescriptionReservationErrorCodes.ReservationUnavailable, kedua.Code);

        var baris = await PenahananAsync(konteks, harness.ResepId);
        Assert.Single(baris);
        Assert.Equal(PrescriptionReservationState.Consumed, baris[0].State);
    }

    [Fact]
    public async Task ResepDibatalkan_PenahananDilepas()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        await SiapkanAktifAsync(harness, konteks);

        var jumlah = await Layanan(konteks).ReleaseAsync(
            harness.ResepId,
            PrescriptionReservationReleaseReasons.PrescriptionCancelled,
            harness.ApotekerId,
            DateTime.UtcNow);
        await konteks.SaveChangesAsync();

        Assert.Equal(1, jumlah);

        var baris = Assert.Single(await PenahananAsync(konteks, harness.ResepId));
        Assert.Equal(PrescriptionReservationState.Released, baris.State);
        Assert.Equal(
            PrescriptionReservationReleaseReasons.PrescriptionCancelled,
            baris.ReleaseReasonCode);

        var saldo = await konteks.Set<PhmDrugStockBalance>().FirstAsync();
        Assert.Equal(0m, saldo.QuantityReserved);
    }

    [Fact]
    public async Task PembayaranTidakDilanjutkan_PenahananDilepasIdempoten()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        await SiapkanAktifAsync(harness, konteks);
        var layanan = Layanan(konteks);

        var pertama = await layanan.ReleaseAsync(
            harness.ResepId, PrescriptionReservationReleaseReasons.PaymentNotContinued,
            harness.ApotekerId, DateTime.UtcNow);
        await konteks.SaveChangesAsync();

        // PHA-DEC-023: pembatalan yang diulang tetap sah dan tidak melepas dua kali.
        var kedua = await layanan.ReleaseAsync(
            harness.ResepId, PrescriptionReservationReleaseReasons.PaymentNotContinued,
            harness.ApotekerId, DateTime.UtcNow);
        await konteks.SaveChangesAsync();

        Assert.Equal(1, pertama);
        Assert.Equal(0, kedua);

        var saldo = await konteks.Set<PhmDrugStockBalance>().FirstAsync();
        Assert.Equal(0m, saldo.QuantityReserved);
    }

    // ───────────────────────────────────────── butir 8 — rawat inap dan IGD

    [Fact]
    public async Task RawatInap_TidakTerkenaBatasWaktuRawatJalan()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        await harness.SediakanStokAsync(konteks, 100, denganPenahanan: false);
        await SiapkanRoutingAsync(harness, konteks, EncounterType.Inpatient);
        var resep = await ResepDraftAsync(konteks, harness.ResepId);

        var mulai = DateTime.UtcNow;
        var hasil = await Layanan(konteks).ReserveForPrescriptionAsync(
            resep, harness.DokterId, mulai, "uji-inap", "UJI");
        await konteks.SaveChangesAsync();

        Assert.True(hasil.IsSuccess, hasil.Message);
        Assert.Null(hasil.ExpiresAt);

        var dilepas = await Layanan(konteks).ReleaseExpiredAsync(mulai.AddDays(7));
        await konteks.SaveChangesAsync();

        Assert.Equal(0, dilepas);
    }

    [Fact]
    public async Task Igd_TidakTerkenaBatasWaktuRawatJalan()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        await harness.SediakanStokAsync(konteks, 100, denganPenahanan: false);
        await SiapkanRoutingAsync(harness, konteks, EncounterType.Emergency);
        var resep = await ResepDraftAsync(konteks, harness.ResepId);

        var mulai = DateTime.UtcNow;
        var hasil = await Layanan(konteks).ReserveForPrescriptionAsync(
            resep, harness.DokterId, mulai, "uji-igd", "UJI");
        await konteks.SaveChangesAsync();

        Assert.True(hasil.IsSuccess, hasil.Message);
        Assert.Null(hasil.ExpiresAt);
    }

    // ───────────────────────────────────────── routing

    [Fact]
    public async Task PenahananMemakaiDepoAuthoritative()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        await harness.SediakanStokAsync(konteks, 100, denganPenahanan: false);
        await SiapkanRoutingAsync(harness, konteks);
        var resep = await ResepDraftAsync(konteks, harness.ResepId);

        var hasil = await Layanan(konteks).ReserveForPrescriptionAsync(
            resep, harness.DokterId, DateTime.UtcNow, "uji-depo", "UJI");
        await konteks.SaveChangesAsync();

        var baris = Assert.Single(hasil.Reservations);
        Assert.Equal(harness.DepoId, baris.StorageLocationId);
    }

    [Fact]
    public async Task DepoTidakDapatDitentukan_GalatBisnisEksplisit()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        await harness.SediakanStokAsync(konteks, 100, denganPenahanan: false);
        // Routing sengaja tidak disiapkan: kunjungan tetap tanpa jenis layanan.
        var resep = await ResepDraftAsync(konteks, harness.ResepId);

        var hasil = await Layanan(konteks).ReserveForPrescriptionAsync(
            resep, harness.DokterId, DateTime.UtcNow, "uji-depo-gagal", "UJI");

        Assert.False(hasil.IsSuccess);
        Assert.Equal(PrescriptionReservationErrorCodes.DepotUnresolved, hasil.Code);
        Assert.Empty(await PenahananAsync(konteks, harness.ResepId));
    }

    [Fact]
    public async Task ResepTanpaPenahanan_DibedakanDariYangKedaluwarsa()
    {
        using var harness = new PharmacyHarness();
        using var konteks = harness.CreateContext();

        var hasil = await Layanan(konteks).EnsureActiveAsync(harness.ResepId, DateTime.UtcNow);

        Assert.False(hasil.IsSuccess);
        // Belum pernah menahan bukan hal yang sama dengan penahanan yang kedaluwarsa;
        // keduanya menuntut tindakan berbeda dari petugas.
        Assert.Equal(PrescriptionReservationErrorCodes.ReservationUnavailable, hasil.Code);
    }

    // ───────────────────────────────────────── pembantu

    /// <summary>Resep yang sudah memegang satu penahanan aktif sebanyak 10.</summary>
    private static async Task SiapkanAktifAsync(
        PharmacyHarness harness, ApplicationDbContext konteks, DateTime? waktu = null)
    {
        await harness.SediakanStokAsync(konteks, 100, denganPenahanan: false);
        await SiapkanRoutingAsync(harness, konteks);
        var resep = await ResepDraftAsync(konteks, harness.ResepId);

        var hasil = await Layanan(konteks).ReserveForPrescriptionAsync(
            resep, harness.DokterId, waktu ?? DateTime.UtcNow, $"PHA-RSV:{resep.Id}", "UJI");
        await konteks.SaveChangesAsync();

        Assert.True(hasil.IsSuccess, hasil.Message);
    }

    /// <summary>Resep kedua pada obat yang sama, dipakai menguji perebutan stok.</summary>
    private static async Task<PhmPrescription> BuatResepKeduaAsync(
        PharmacyHarness harness, ApplicationDbContext konteks, decimal jumlah)
    {
        var resepId = Guid.NewGuid();

        konteks.Set<PhmPrescription>().Add(new PhmPrescription
        {
            Id = resepId,
            PrescriptionNumber = "UJI-RX-0002",
            EncounterId = harness.KunjunganId,
            ConsultationId = Guid.NewGuid(),
            PatientId = harness.PasienId,
            DoctorId = harness.DokterId,
            PrescriptionDateTime = DateTime.UtcNow,
            PrescriptionStatus = PrescriptionStatus.Draft,
            TotalItemCount = 1,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        konteks.Set<PhmPrescriptionItem>().Add(new PhmPrescriptionItem
        {
            Id = Guid.NewGuid(),
            PrescriptionId = resepId,
            DrugId = harness.ObatId,
            DrugNameSnapshot = "Obat Uji Satu",
            Quantity = jumlah,
            DispenseUnitMeasurementId = harness.SatuanId,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow
        });

        await konteks.SaveChangesAsync();

        return await konteks.Set<PhmPrescription>().FirstAsync(x => x.Id == resepId);
    }
}
