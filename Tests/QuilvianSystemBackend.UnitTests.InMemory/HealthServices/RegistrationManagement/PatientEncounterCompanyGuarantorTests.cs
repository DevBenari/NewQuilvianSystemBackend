using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.HealthServices.RegistrationManagement;

/// <summary>
/// Membuktikan kontrak <c>RWI-ENC-PAYER-001 1.0.0</c> pada endpoint create encounter:
/// tipe pembayaran ketiga, matriks validasi tiga payer, snapshot yang tersimpan, pemisahan
/// jalur petugas dari kiosk, dan regresi dua payer lama.
/// </summary>
/// <remarks>
/// Ditulis oleh <c>BE-RWI-035</c>. Batas pembuktian provider InMemory dijelaskan pada
/// <see cref="PatientEncounterTestWorld"/>.
/// </remarks>
public sealed class PatientEncounterCompanyGuarantorTests
{
    private static DateTime Hari(int offsetHari = 0)
        => AppDateTimeHelper.OperationalDate().Date.AddDays(offsetHari);

    private static PatientEncounterCreateRequest PermintaanDasar(PatientEncounterTestWorld world)
        => new()
        {
            PatientId = world.Patient.Id,
            ServiceUnitId = world.ServiceUnit.Id,
            EncounterType = EncounterType.Outpatient,
            VisitType = VisitType.NewVisit,
            RegistrationSource = EncounterRegistrationSource.FrontDesk
        };

    private static PatientEncounterCreateRequest PermintaanPerusahaan(PatientEncounterTestWorld world)
    {
        var request = PermintaanDasar(world);
        request.PaymentType = EncounterPaymentType.CompanyGuarantor;
        request.PatientCompanyGuarantorId = world.PatientCompanyGuarantor.Id;
        return request;
    }

    // =====================================================================
    // 1. Kontrak nilai enum
    // =====================================================================

    /// <remarks>
    /// Acceptance criteria 1 dan 7: nilai lama tidak boleh bergeser, karena payload yang
    /// sudah tersimpan mengartikan angkanya apa adanya.
    /// </remarks>
    [Fact]
    public void MempertahankanNilaiTunaiDanAsuransiSertaMenambahPenjaminPerusahaan()
    {
        Assert.Equal(1, (int)EncounterPaymentType.Cash);
        Assert.Equal(2, (int)EncounterPaymentType.Insurance);
        Assert.Equal(3, (int)EncounterPaymentType.CompanyGuarantor);

        Assert.Equal(3, Enum.GetValues<EncounterPaymentType>().Length);
    }

    /// <remarks>
    /// Acceptance criteria 5: opsi filter otomatis menampilkan label Indonesia yang dikunci
    /// kontrak bagian 3, karena metadata dibangun dari atribut <c>[Display]</c> enum.
    /// </remarks>
    [Fact]
    public async Task MetadataFilterMenampilkanOpsiPenjaminPerusahaan()
    {
        var world = await PatientEncounterTestWorld.CreateAsync();

        var result = await world.Controller.GetFilterMetadataForAdmin();
        var data = PatientEncounterTestWorld.Data<PatientEncounterFilterMetadataResponse>(result);

        Assert.NotNull(data);

        var opsi = Assert.Single(data!.PaymentTypeOptions, x => x.Value == 3);

        Assert.Equal("CompanyGuarantor", opsi.Name);
        Assert.Equal("Penjamin Perusahaan", opsi.Label);
    }

    // =====================================================================
    // 2. Jalur sukses dan data yang disimpan
    // =====================================================================

    /// <remarks>
    /// Acceptance criteria 2 dan 4, beserta kontrak bagian 5: satu encounter, satu sumber
    /// pembayaran, kedua foreign key perusahaan terisi, dan ketiga referensi Tunai/Asuransi
    /// bernilai null.
    /// </remarks>
    [Fact]
    public async Task EncounterPetugasMenyimpanReferensiDanSnapshotPerusahaan()
    {
        var world = await PatientEncounterTestWorld.CreateAsync();

        var result = await world.Controller.CreateEncounterForAdmin(PermintaanPerusahaan(world));

        Assert.Equal(200, PatientEncounterTestWorld.KodeStatus(result));

        var encounter = await world.DbContext.Set<TrxPatientEncounter>().SingleAsync();
        var payment = await world.DbContext.Set<TrxPatientEncounterGuarantor>().SingleAsync();

        Assert.Equal(EncounterPaymentType.CompanyGuarantor, encounter.PaymentType);
        Assert.Null(encounter.PaymentMethodId);

        Assert.Equal(encounter.Id, payment.EncounterId);
        Assert.Equal(EncounterPaymentType.CompanyGuarantor, payment.PaymentType);

        // Kedua referensi perusahaan.
        Assert.Equal(world.PatientCompanyGuarantor.Id, payment.PatientCompanyGuarantorId);
        Assert.Equal(world.CompanyGuarantor.Id, payment.CompanyGuarantorId);

        // Ketiga referensi Tunai/Asuransi wajib kosong.
        Assert.Null(payment.PaymentMethodId);
        Assert.Null(payment.PatientInsuranceId);
        Assert.Null(payment.InsuranceProviderId);

        // Snapshot yang dikunci kontrak bagian 5.
        Assert.Equal("PT Sehat Sentosa", payment.PaymentSourceNameSnapshot);
        Assert.Equal("COMP-001", payment.CompanyGuarantorCodeSnapshot);
        Assert.Equal("EMP-00125", payment.EmployeeNumberSnapshot);
        Assert.Equal("Budi Santoso", payment.EmployeeNameSnapshot);
        Assert.Equal("BEN-CORP", payment.BenefitPlanCodeSnapshot);
        Assert.Equal("Corporate Gold", payment.PlanNameSnapshot);
        Assert.Equal("Kelas 1", payment.ClassNameSnapshot);

        Assert.True(payment.IsEligible);
        Assert.True(payment.IsPolicyActive);
    }

    /// <remarks>
    /// Kontrak bagian 1: snapshot tidak ikut berubah ketika master perusahaan disunting
    /// setelah registrasi. Inilah alasan snapshot disimpan, bukan dibaca ulang lewat join.
    /// </remarks>
    [Fact]
    public async Task SnapshotTidakBerubahKetikaMasterPerusahaanDisuntingSetelahRegistrasi()
    {
        var world = await PatientEncounterTestWorld.CreateAsync();

        await world.Controller.CreateEncounterForAdmin(PermintaanPerusahaan(world));

        var master = await world.DbContext.Set<MstCompanyGuarantor>().SingleAsync();

        master.CompanyGuarantorName = "PT Nama Baru";
        master.CompanyGuarantorCode = "COMP-999";
        await world.DbContext.SaveChangesAsync();

        var payment = await world.DbContext.Set<TrxPatientEncounterGuarantor>()
            .AsNoTracking()
            .SingleAsync();

        Assert.Equal("PT Sehat Sentosa", payment.PaymentSourceNameSnapshot);
        Assert.Equal("COMP-001", payment.CompanyGuarantorCodeSnapshot);
    }

    /// <remarks>
    /// Acceptance criteria 5: response create mengembalikan kelima field aditif kontrak
    /// bagian 6.
    /// </remarks>
    [Fact]
    public async Task ResponseCreateMengembalikanFieldAditifPenjaminPerusahaan()
    {
        var world = await PatientEncounterTestWorld.CreateAsync();

        var result = await world.Controller.CreateEncounterForAdmin(PermintaanPerusahaan(world));
        var data = PatientEncounterTestWorld.Data<PatientEncounterCreateResponse>(result);

        Assert.NotNull(data);
        Assert.NotNull(data!.Payment);

        var payment = data.Payment!;

        Assert.Equal(EncounterPaymentType.CompanyGuarantor, payment.PaymentType);
        Assert.Equal("Penjamin Perusahaan", payment.PaymentTypeName);
        Assert.Equal(world.PatientCompanyGuarantor.Id, payment.PatientCompanyGuarantorId);
        Assert.Equal(world.CompanyGuarantor.Id, payment.CompanyGuarantorId);
        Assert.Equal("COMP-001", payment.CompanyGuarantorCodeSnapshot);
        Assert.Equal("EMP-00125", payment.EmployeeNumberSnapshot);
        Assert.Equal("Budi Santoso", payment.EmployeeNameSnapshot);
    }

    /// <remarks>
    /// Acceptance criteria 5: detail dan summary mengenali tipe 3 tanpa merusak angka lama.
    /// </remarks>
    [Fact]
    public async Task DetailDanSummaryMengenaliEncounterPenjaminPerusahaan()
    {
        var world = await PatientEncounterTestWorld.CreateAsync();

        var created = await world.Controller.CreateEncounterForAdmin(PermintaanPerusahaan(world));
        var createdData = PatientEncounterTestWorld.Data<PatientEncounterCreateResponse>(created);

        var detail = await world.Controller.GetById(createdData!.EncounterId);
        var detailData = PatientEncounterTestWorld.Data<PatientEncounterDetailResponse>(detail);

        Assert.NotNull(detailData);
        Assert.Equal(EncounterPaymentType.CompanyGuarantor, detailData!.PaymentType);
        Assert.Equal("Penjamin Perusahaan", detailData.PaymentTypeName);
        Assert.Equal("PT Sehat Sentosa", detailData.PaymentSourceNameSnapshot);
        Assert.NotNull(detailData.Payment);
        Assert.Equal(world.CompanyGuarantor.Id, detailData.Payment!.CompanyGuarantorId);

        var summary = await world.Controller.GetSummary(
            startDate: null,
            endDate: null,
            customPeriod: null,
            patientId: null,
            serviceUnitId: null,
            encounterStatus: null,
            encounterType: null,
            paymentType: null,
            isActive: null);

        var summaryData = PatientEncounterTestWorld.Data<PatientEncounterSummaryResponse>(summary);

        Assert.NotNull(summaryData);
        Assert.Equal(1, summaryData!.CompanyGuarantorEncounter);
        Assert.Equal(0, summaryData.CashEncounter);
        Assert.Equal(0, summaryData.InsuranceEncounter);
        Assert.Equal(1, summaryData.TotalEncounter);
    }

    // =====================================================================
    // 3. Matriks payload campuran
    // =====================================================================

    /// <remarks>
    /// Acceptance criteria 2 dan kontrak bagian 4: satu encounter hanya boleh punya satu
    /// sumber pembayaran, sehingga setiap kombinasi campuran ditolak 400.
    /// </remarks>
    [Theory]
    [InlineData(EncounterPaymentType.CompanyGuarantor, false, true, false, "PatientInsuranceId harus kosong untuk pembayaran Penjamin Perusahaan.")]
    [InlineData(EncounterPaymentType.CompanyGuarantor, true, false, false, "PaymentMethodId harus kosong untuk pembayaran Penjamin Perusahaan.")]
    [InlineData(EncounterPaymentType.CompanyGuarantor, false, false, false, "PatientCompanyGuarantorId wajib diisi untuk pembayaran Penjamin Perusahaan.")]
    [InlineData(EncounterPaymentType.Cash, false, false, true, "PatientCompanyGuarantorId harus kosong untuk pembayaran Tunai.")]
    [InlineData(EncounterPaymentType.Insurance, false, true, true, "PatientCompanyGuarantorId harus kosong untuk pembayaran Asuransi.")]
    public async Task MenolakPayloadPayerCampuran(
        EncounterPaymentType paymentType,
        bool kirimPaymentMethod,
        bool kirimAsuransi,
        bool kirimPerusahaan,
        string pesanDiharapkan)
    {
        var world = await PatientEncounterTestWorld.CreateAsync();

        var request = PermintaanDasar(world);
        request.PaymentType = paymentType;

        if (kirimPaymentMethod)
        {
            request.PaymentMethodId = world.PaymentMethod.Id;
        }

        if (kirimAsuransi)
        {
            request.PatientInsuranceId = world.PatientInsurance.Id;
        }

        if (kirimPerusahaan)
        {
            request.PatientCompanyGuarantorId = world.PatientCompanyGuarantor.Id;
        }

        var result = await world.Controller.CreateEncounterForAdmin(request);

        Assert.Equal(400, PatientEncounterTestWorld.KodeStatus(result));
        Assert.Equal(pesanDiharapkan, PatientEncounterTestWorld.Pesan(result));

        Assert.False(await world.DbContext.Set<TrxPatientEncounter>().AnyAsync());
        Assert.False(await world.DbContext.Set<TrxPatientEncounterGuarantor>().AnyAsync());
    }

    // =====================================================================
    // 4. Kelayakan kartu perusahaan
    // =====================================================================

    /// <remarks>
    /// Acceptance criteria 3: kartu milik pasien lain ditolak, dan pesannya tidak menyebut
    /// nama, nomor rekam medis, maupun nomor karyawan pasien pemilik kartu.
    /// </remarks>
    [Fact]
    public async Task MenolakKartuPerusahaanMilikPasienLainTanpaMembocorkanDataPasienItu()
    {
        var world = await PatientEncounterTestWorld.CreateAsync();

        var request = PermintaanPerusahaan(world);
        request.PatientId = world.OtherPatient.Id;

        var result = await world.Controller.CreateEncounterForAdmin(request);

        Assert.Equal(400, PatientEncounterTestWorld.KodeStatus(result));

        var pesan = PatientEncounterTestWorld.Pesan(result);

        Assert.Equal(
            "Penjamin perusahaan yang dipilih bukan milik pasien pada encounter.",
            pesan);

        Assert.DoesNotContain("Budi Santoso", pesan);
        Assert.DoesNotContain("EMP-00125", pesan);
        Assert.DoesNotContain("RM-0000001", pesan);
    }

    /// <remarks>
    /// Acceptance criteria 3: kartu wajib aktif, eligible, dan menunjuk perusahaan aktif.
    /// </remarks>
    [Theory]
    [InlineData(false, true, true, "Penjamin perusahaan pasien tidak aktif.")]
    [InlineData(true, false, true, "Penjamin perusahaan pasien tidak eligible.")]
    [InlineData(true, true, false, "Perusahaan penjamin tidak valid atau tidak aktif.")]
    public async Task MenolakKartuAtauPerusahaanYangTidakLayak(
        bool kartuAktif,
        bool kartuEligible,
        bool perusahaanAktif,
        string pesanDiharapkan)
    {
        var world = await PatientEncounterTestWorld.CreateAsync(
            companyCardActive: kartuAktif,
            companyCardEligible: kartuEligible,
            companyMasterActive: perusahaanAktif);

        var result = await world.Controller.CreateEncounterForAdmin(PermintaanPerusahaan(world));

        Assert.Equal(400, PatientEncounterTestWorld.KodeStatus(result));
        Assert.Equal(pesanDiharapkan, PatientEncounterTestWorld.Pesan(result));
        Assert.False(await world.DbContext.Set<TrxPatientEncounter>().AnyAsync());
    }

    /// <remarks>
    /// Acceptance criteria 3 dan kontrak bagian 4: masa berlaku bersifat inklusif pada
    /// kedua ujungnya.
    /// </remarks>
    [Fact]
    public async Task MenerimaEncounterPadaHariTerakhirMasaBerlaku()
    {
        var world = await PatientEncounterTestWorld.CreateAsync(
            companyEffectiveStartDate: Hari(-30),
            companyEffectiveEndDate: Hari(0));

        var result = await world.Controller.CreateEncounterForAdmin(PermintaanPerusahaan(world));

        Assert.Equal(200, PatientEncounterTestWorld.KodeStatus(result));
    }

    [Fact]
    public async Task MenolakEncounterSetelahMasaBerlakuBerakhir()
    {
        var world = await PatientEncounterTestWorld.CreateAsync(
            companyEffectiveStartDate: Hari(-30),
            companyEffectiveEndDate: Hari(-1));

        var result = await world.Controller.CreateEncounterForAdmin(PermintaanPerusahaan(world));

        Assert.Equal(400, PatientEncounterTestWorld.KodeStatus(result));
        Assert.Equal(
            "Penjamin perusahaan sudah kedaluwarsa pada tanggal kunjungan.",
            PatientEncounterTestWorld.Pesan(result));
    }

    [Fact]
    public async Task MenolakEncounterSebelumMasaBerlakuDimulai()
    {
        var world = await PatientEncounterTestWorld.CreateAsync(
            companyEffectiveStartDate: Hari(1),
            companyEffectiveEndDate: Hari(30));

        var request = PermintaanPerusahaan(world);
        request.VisitDate = Hari(0);

        var result = await world.Controller.CreateEncounterForAdmin(request);

        Assert.Equal(400, PatientEncounterTestWorld.KodeStatus(result));
        Assert.Equal(
            "Penjamin perusahaan belum berlaku pada tanggal kunjungan.",
            PatientEncounterTestWorld.Pesan(result));
    }

    [Fact]
    public async Task MenolakKartuPerusahaanYangTidakDitemukan()
    {
        var world = await PatientEncounterTestWorld.CreateAsync();

        var request = PermintaanPerusahaan(world);
        request.PatientCompanyGuarantorId = Guid.NewGuid();

        var result = await world.Controller.CreateEncounterForAdmin(request);

        Assert.Equal(400, PatientEncounterTestWorld.KodeStatus(result));
        Assert.Equal(
            "Penjamin perusahaan pasien tidak ditemukan.",
            PatientEncounterTestWorld.Pesan(result));
    }

    // =====================================================================
    // 5. Pemisahan jalur petugas dan kiosk
    // =====================================================================

    /// <remarks>
    /// Acceptance criteria 6: kiosk tidak boleh ikut memperoleh kemampuan payer ketiga.
    /// Kedua route kiosk memakai satu method yang sama, sehingga satu pembuktian cukup.
    /// </remarks>
    [Fact]
    public async Task RouteKioskMenolakPenjaminPerusahaan()
    {
        var world = await PatientEncounterTestWorld.CreateAsync();

        var result = await world.Controller.CreateEncounterForKiosk(PermintaanPerusahaan(world));

        Assert.Equal(400, PatientEncounterTestWorld.KodeStatus(result));
        Assert.Equal(
            "Tipe pembayaran Penjamin Perusahaan hanya tersedia pada registrasi petugas.",
            PatientEncounterTestWorld.Pesan(result));

        Assert.False(await world.DbContext.Set<TrxPatientEncounter>().AnyAsync());
        Assert.False(await world.DbContext.Set<TrxPatientEncounterGuarantor>().AnyAsync());
    }

    /// <remarks>
    /// Acceptance criteria 6 dan 7: kiosk tetap dapat membuat encounter Tunai dan Asuransi
    /// seperti sebelum task ini.
    /// </remarks>
    [Theory]
    [InlineData(EncounterPaymentType.Cash)]
    [InlineData(EncounterPaymentType.Insurance)]
    public async Task RouteKioskTetapMenerimaTunaiDanAsuransi(EncounterPaymentType paymentType)
    {
        var world = await PatientEncounterTestWorld.CreateAsync();

        var request = PermintaanDasar(world);
        request.PaymentType = paymentType;

        if (paymentType == EncounterPaymentType.Insurance)
        {
            request.PatientInsuranceId = world.PatientInsurance.Id;
        }

        var result = await world.Controller.CreateEncounterForKiosk(request);

        Assert.Equal(200, PatientEncounterTestWorld.KodeStatus(result));

        var payment = await world.DbContext.Set<TrxPatientEncounterGuarantor>().SingleAsync();

        Assert.Equal(paymentType, payment.PaymentType);
        Assert.Null(payment.PatientCompanyGuarantorId);
        Assert.Null(payment.CompanyGuarantorId);
    }

    // =====================================================================
    // 6. Regresi dua payer lama
    // =====================================================================

    /// <remarks>
    /// Acceptance criteria 7: payload Tunai lama tidak berubah, dan kelima field baru
    /// bernilai null.
    /// </remarks>
    [Fact]
    public async Task EncounterTunaiTetapTersimpanTanpaFieldPerusahaan()
    {
        var world = await PatientEncounterTestWorld.CreateAsync();

        var request = PermintaanDasar(world);
        request.PaymentType = EncounterPaymentType.Cash;
        request.PaymentMethodId = world.PaymentMethod.Id;

        var result = await world.Controller.CreateEncounterForAdmin(request);

        Assert.Equal(200, PatientEncounterTestWorld.KodeStatus(result));

        var payment = await world.DbContext.Set<TrxPatientEncounterGuarantor>().SingleAsync();

        Assert.Equal(EncounterPaymentType.Cash, payment.PaymentType);
        Assert.Equal(world.PaymentMethod.Id, payment.PaymentMethodId);
        Assert.Equal("Tunai", payment.PaymentSourceNameSnapshot);
        Assert.True(payment.IsEligible);
        Assert.False(payment.IsPolicyActive);

        Assert.Null(payment.PatientCompanyGuarantorId);
        Assert.Null(payment.CompanyGuarantorId);
        Assert.Null(payment.CompanyGuarantorCodeSnapshot);
        Assert.Null(payment.EmployeeNumberSnapshot);
        Assert.Null(payment.EmployeeNameSnapshot);
    }

    /// <remarks>
    /// Acceptance criteria 7: payload Asuransi lama tidak berubah, dan kelima field baru
    /// bernilai null.
    /// </remarks>
    [Fact]
    public async Task EncounterAsuransiTetapTersimpanTanpaFieldPerusahaan()
    {
        var world = await PatientEncounterTestWorld.CreateAsync();

        var request = PermintaanDasar(world);
        request.PaymentType = EncounterPaymentType.Insurance;
        request.PatientInsuranceId = world.PatientInsurance.Id;

        var result = await world.Controller.CreateEncounterForAdmin(request);

        Assert.Equal(200, PatientEncounterTestWorld.KodeStatus(result));

        var payment = await world.DbContext.Set<TrxPatientEncounterGuarantor>().SingleAsync();

        Assert.Equal(EncounterPaymentType.Insurance, payment.PaymentType);
        Assert.Equal(world.PatientInsurance.Id, payment.PatientInsuranceId);
        Assert.Equal(world.InsuranceProvider.Id, payment.InsuranceProviderId);
        Assert.Equal("PT Asuransi Sehat", payment.PaymentSourceNameSnapshot);
        Assert.Equal("POL-0001", payment.PolicyNumberSnapshot);

        Assert.Null(payment.PatientCompanyGuarantorId);
        Assert.Null(payment.CompanyGuarantorId);
        Assert.Null(payment.CompanyGuarantorCodeSnapshot);
        Assert.Null(payment.EmployeeNumberSnapshot);
        Assert.Null(payment.EmployeeNameSnapshot);
    }

    // =====================================================================
    // 7. Atomisitas
    // =====================================================================

    /// <remarks>
    /// Acceptance criteria 4: encounter dan sumber pembayaran masuk ke satu
    /// <c>SaveChangesAsync</c>, sehingga kegagalan penyimpanan menyisakan nol baris.
    /// Bahwa PostgreSQL benar-benar mengembalikan perubahan saat transaksi digagalkan
    /// adalah pembuktian terpisah terhadap database sungguhan, dan dicatat pada laporan.
    /// </remarks>
    [Fact]
    public async Task KegagalanPenyimpananTidakMenyisakanEncounterMaupunSumberPembayaran()
    {
        var databaseRoot = new InMemoryDatabaseRoot();
        var databaseName = $"patient-encounter-atomic-{Guid.NewGuid():N}";

        // Master disemai lewat context normal supaya penyemaiannya tidak ikut digagalkan.
        using var setupContext = new ApplicationDbContext(
            PatientEncounterTestWorld.BuildOptions(databaseName, databaseRoot));

        var world = await PatientEncounterTestWorld.CreateAsync(setupContext);

        // Create dijalankan lewat context yang menolak menyimpan, di atas store yang sama.
        using var failingContext = new ApplicationDbContextYangGagalMenyimpan(
            PatientEncounterTestWorld.BuildOptions(databaseName, databaseRoot));

        var controller = PatientEncounterTestWorld.BuildController(failingContext);

        var result = await controller.CreateEncounterForAdmin(PermintaanPerusahaan(world));

        Assert.Equal(500, PatientEncounterTestWorld.KodeStatus(result));
        Assert.Equal(1, failingContext.SaveAttempts);

        using var verifyContext = new ApplicationDbContext(
            PatientEncounterTestWorld.BuildOptions(databaseName, databaseRoot));

        Assert.False(await verifyContext.Set<TrxPatientEncounter>().AnyAsync());
        Assert.False(await verifyContext.Set<TrxPatientEncounterGuarantor>().AnyAsync());
    }

    /// <summary>
    /// Context yang menolak menyimpan, meniru
    /// <c>FailingSaveApplicationDbContext</c> pada folder InPatientManagement.
    /// </summary>
    private sealed class ApplicationDbContextYangGagalMenyimpan : ApplicationDbContext
    {
        public ApplicationDbContextYangGagalMenyimpan(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public int SaveAttempts { get; private set; }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveAttempts++;

            throw new InvalidOperationException("Penyimpanan sengaja digagalkan oleh test.");
        }
    }
}
