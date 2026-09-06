using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Services.Logging;
using System.Text.Json;

namespace QuilvianSystemBackend.Tests.BillingManagement;

public sealed class BillingCalculationServiceTests
{
    // BE-BKC-026 (perbaikan fixture basi): kategori item di sini WAJIB IsPharmacy=true. Test ini
    // ditulis sebelum PPN dibatasi ke item Pharmacy/Drug/Alkes (lihat BillingCalculationService.cs
    // ApplyInvoiceTax) - komentar lama "*" menandai tax rule TINGKAT INVOICE sudah tidak benar
    // sejak perbaikan itu; TaxableCategory kini murni label, dan basis pajak SELALU digerbangi
    // item.IsPharmacy, bukan level invoice. Tanpa perbaikan ini test salah mengasumsikan item non-
    // pharmacy tetap kena pajak dan akan gagal di TaxAmount (lihat OutpatientNonPharmacyItemsAre
    // NeverPartOfTaxBase di bawah untuk bukti sebaliknya).
    [Fact]
    public async Task RecalculateCreatesImmutableVersionsWithTaxProvenance()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isPharmacy: true);
        db.MstTaxRules.Add(TaxRule("*", at));
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var first = await service.RecalculateAsync(invoice.Id, Request(invoice.RowVersion, "Kalkulasi awal"), Guid.NewGuid(), CancellationToken.None);
        var second = await service.RecalculateAsync(invoice.Id, Request(first.InvoiceRowVersion, "Tarif diverifikasi ulang"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(1, first.VersionNo);
        Assert.Equal(2, second.VersionNo);
        Assert.Equal(100_000m, first.GrossAmount);
        Assert.Equal(11_000m, first.TaxAmount);
        Assert.Equal(111_000m, first.PatientAmount);
        Assert.Equal(BillingCalculationContract.Version, first.Breakdown.ContractVersion);
        Assert.Single(first.Breakdown.Taxes);
        Assert.Equal("Kalkulasi awal", (await db.BilCalculationVersions.SingleAsync(x => x.VersionNo == 1)).Reason);
        Assert.Equal(2, await db.BilCalculationVersions.CountAsync());
        Assert.Equal(2, (await db.BilInvoices.FindAsync(invoice.Id))!.CurrentCalculationVersion);
    }

    // BE-BKC-026 / BIL-AT-044: gerbang PPN rawat inap (isOutpatientForTax = ServiceType != Ranap)
    // sudah terimplementasi lewat BE-BKC-FIX-004 di luar roadmap - task ini memverifikasi, bukan
    // membangun. Obat/alkes pada tagihan RANAP dibebaskan PPN sepenuhnya, terlepas dari tarif pajak
    // yang aktif.
    [Fact]
    public async Task InpatientPharmacyItemsAreExemptFromTax()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RANAP", at, isPharmacy: true);
        db.MstTaxRules.Add(TaxRule("*", at));
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Obat rawat inap bebas PPN"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(0m, result.TaxAmount);
        Assert.Empty(result.Breakdown.Taxes);
        Assert.Equal(100_000m, result.PatientAmount);
    }

    // BE-BKC-026 / BIL-AT-045: obat/alkes rawat JALAN tetap kena PPN, kebalikan langsung dari test
    // di atas - keduanya sengaja memakai fixture identik (hanya ServiceType berbeda) supaya
    // perbandingannya tidak bisa disangkal kebetulan setup.
    [Fact]
    public async Task OutpatientPharmacyItemsAreTaxed()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isPharmacy: true);
        db.MstTaxRules.Add(TaxRule("*", at));
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Obat rawat jalan kena PPN"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(11_000m, result.TaxAmount);
        Assert.Equal(111_000m, result.PatientAmount);
    }

    // BE-BKC-026 / BIL-AT-046: IGD diperlakukan SAMA dengan rawat jalan, bukan dibebaskan seperti
    // RANAP - satu-satunya gerbang pembebasan adalah ServiceType == Ranap secara eksplisit.
    [Fact]
    public async Task EmergencyPharmacyItemsAreTaxedLikeOutpatient()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "IGD", at, isPharmacy: true);
        db.MstTaxRules.Add(TaxRule("*", at));
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Obat IGD kena PPN"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(11_000m, result.TaxAmount);
        Assert.Equal(111_000m, result.PatientAmount);
    }

    // BE-BKC-026 / BIL-AT-051: MCU belum dipakai sebagai ServiceType aktif (jawaban pemilik atas
    // BKC-CQ-03) - tidak ada konstanta MCU pada AdministrationFeeServiceTypes. Hasil uji ini tetap
    // dilampirkan sebagai bahan keputusan sebelum MCU diaktifkan: perilaku bawaannya sudah benar
    // (tetap kena PPN) karena gerbangnya hanya mengecualikan Ranap secara eksplisit, ServiceType
    // lain apa pun otomatis mengikuti jalur rawat jalan.
    [Fact]
    public async Task McuPharmacyItemsAreTaxedAsDefaultBehavior()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "MCU", at, isPharmacy: true);
        db.MstTaxRules.Add(TaxRule("*", at));
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Obat MCU kena PPN sebagai default"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(11_000m, result.TaxAmount);
        Assert.Equal(111_000m, result.PatientAmount);
    }

    // BE-BKC-026 / BIL-VAL-039: jenis kunjungan kosong atau tidak dikenal tetap dikenai PPN dan
    // TIDAK menghentikan perhitungan - "!= Ranap" bernilai true untuk string apa pun selain "RANAP"
    // persis, termasuk string kosong, sehingga tidak perlu penjaga eksplisit terpisah untuk kasus ini.
    [Fact]
    public async Task UnknownOrEmptyServiceTypeIsStillTaxedAndDoesNotHaltCalculation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "", at, isPharmacy: true);
        db.MstTaxRules.Add(TaxRule("*", at));
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "ServiceType kosong tetap kena PPN"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(11_000m, result.TaxAmount);
        Assert.Equal(111_000m, result.PatientAmount);
    }

    // BE-BKC-026 (acceptance criteria #6 - basis pajak): jasa konsultasi/tindakan tidak pernah masuk
    // basis PPN, bahkan pada rawat jalan yang sepenuhnya kena pajak untuk item Pharmacy. Ini juga
    // pembuktian terbalik dari RecalculateCreatesImmutableVersionsWithTaxProvenance di atas - fixture
    // identik (RAJAL, tax rule sama) tapi isPharmacy=false (default) harus menghasilkan TaxAmount nol.
    [Fact]
    public async Task OutpatientNonPharmacyItemsAreNeverPartOfTaxBase()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        db.MstTaxRules.Add(TaxRule("*", at));
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Jasa tindakan tidak kena PPN"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(0m, result.TaxAmount);
        Assert.Empty(result.Breakdown.Taxes);
        Assert.Equal(100_000m, result.PatientAmount);
    }

    [Fact]
    public async Task CoverageWaterfallAppliesPrimaryThenExcessThenPatient()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", DateTimeOffset.UtcNow);
        var adapter = new AllocatingCoverageAdapter("APPROVED", "APPROVED", 60_000m, 25_000m, 0);
        var service = CreateService(db, adapter);

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Coverage dihitung"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(60_000m, result.PrimaryAmount);
        Assert.Equal(40_000m, result.Breakdown.Coverage.ResidualAfterPrimary);
        Assert.Equal(25_000m, result.ExcessAmount);
        Assert.Equal(15_000m, result.Breakdown.Coverage.ResidualAfterExcess);
        Assert.Equal(15_000m, result.PatientAmount);
    }

    [Fact]
    public async Task CoverageCapRejectsAmountAboveEligibleWithoutCreatingVersion()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", DateTimeOffset.UtcNow);
        var adapter = new FixedCoverageAdapter(new BillingCoverageDecision(
            "INSURER-CONTRACT-TEST", "APPROVED", "APPROVED", 80_000m, 30_000m, 0, [], [], 0, 0, []));
        var service = CreateService(db, adapter);

        var exception = await Assert.ThrowsAsync<BillingCalculationValidationException>(() =>
            service.RecalculateAsync(invoice.Id, Request(invoice.RowVersion, "Coverage invalid"), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("melebihi biaya", exception.Message);
        Assert.Empty(db.BilCalculationVersions);
    }

    // BE-BKC-025 / BIL-VAL-036 / BIL-AT-043 (menggantikan RejectedCoverageRemainsUnresolvedAndDoes
    // NotShiftToPatient di rilis sebelumnya): penjaga diretarget dari menguji UnresolvedAmount
    // menjadi menguji DataAnomalyAmount (BKC-DES-012). Decision yang menyatakan PrimaryStatus
    // REJECTED TANPA DataAnomalyAmount terisi kini DITOLAK - lewat
    // RegistrationBillingCoverageAdapter keadaan ini seharusnya tidak pernah tercapai lagi (jalur
    // REJECTED lama sudah diganti Anomaly(), lihat RegistrationCoverageAdapterPayerNotEligible...
    // di bawah); dipalsukan di sini lewat FixedCoverageAdapter untuk membuktikan penjaganya
    // sendiri bekerja terhadap IBillingCoverageAdapter mana pun yang masih mengklaim REJECTED
    // tanpa anomali.
    [Fact]
    public async Task RejectedCoverageWithoutDataAnomalyIsRejected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", DateTimeOffset.UtcNow);
        var adapter = new FixedCoverageAdapter(new BillingCoverageDecision(
            "INSURER-CONTRACT-TEST", "REJECTED", "NOT_CONFIGURED", 0, 0, 100_000m, [], [], 0, 0, []));
        var service = CreateService(db, adapter);

        var exception = await Assert.ThrowsAsync<BillingCalculationValidationException>(() =>
            service.RecalculateAsync(invoice.Id, Request(invoice.RowVersion, "Claim ditolak tanpa anomali"), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("tanpa policy kontrak", exception.Message);
        Assert.Empty(db.BilCalculationVersions);
    }

    [Fact]
    public async Task AdministrationFeeIsOncePerLocalDayAndRanapAppliesReplacementDifference()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var patientId = Guid.NewGuid();
        var firstAt = new DateTimeOffset(2026, 8, 21, 1, 0, 0, TimeSpan.Zero);
        var secondAt = firstAt.AddHours(2);
        var inpatientAt = firstAt.AddHours(4);
        db.MstAdministrationFeePolicies.AddRange(
            AdministrationPolicy("ADM-RAJAL", "RAJAL", 20_000m, 10, firstAt),
            AdministrationPolicy("ADM-RANAP", "RANAP", 50_000m, 100, firstAt));
        var first = await SeedInvoiceAsync(db, patientId, "RAJAL", firstAt);
        var second = await SeedInvoiceAsync(db, patientId, "RAJAL", secondAt);
        var inpatient = await SeedInvoiceAsync(db, patientId, "RANAP", inpatientAt);
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var firstResult = await service.RecalculateAsync(first.Id, Request(first.RowVersion, "Kunjungan pertama"), Guid.NewGuid(), CancellationToken.None);
        var secondResult = await service.RecalculateAsync(second.Id, Request(second.RowVersion, "Kunjungan kedua"), Guid.NewGuid(), CancellationToken.None);
        var inpatientResult = await service.RecalculateAsync(inpatient.Id, Request(inpatient.RowVersion, "Transfer rawat inap"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(20_000m, firstResult.AdministrationFeeAmount);
        Assert.Equal(0, secondResult.AdministrationFeeAmount);
        Assert.Equal(30_000m, inpatientResult.AdministrationFeeAmount);
        Assert.True(inpatientResult.Breakdown.AdministrationFee.ReplacesEarlierFee);
        Assert.Equal(50_000m, firstResult.AdministrationFeeAmount + secondResult.AdministrationFeeAmount + inpatientResult.AdministrationFeeAmount);
    }

    // BIL-AT-011 (BE-BKC-017): "insurer cover flag true -> coverage mengikuti policy" untuk admin
    // fee. Separuh acceptance ini (diskon admin fee ditolak) sudah dibuktikan di
    // BillingDiscountServiceTests.AdministrationFeeCategoryCannotBeDiscounted; separuh ini
    // membuktikan flag Coverable pada MstAdministrationFeePolicy benar-benar menjadi gerbang
    // policy untuk coverage waterfall, bukan sekadar kolom dekoratif - lihat
    // ApplyCoverageWaterfall/BuildCoverageComponents (coverableAmount hanya menjumlahkan komponen
    // yang Coverable=true).
    // BE-BKC-022 / BIL-VAL-028 (BIL-AT-029 jalur gagal): rincian per baris yang tidak menjumlah ke
    // total tanggungan MUST menghentikan perhitungan, bukan diteruskan. Selisihnya sengaja dibuat
    // Rp 10.000 dari total Rp 60.000 supaya jelas ini bukan soal pembulatan.
    [Fact]
    public async Task RincianTanggunganPerBarisYangTidakMenjumlahMenghentikanPerhitungan()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", DateTimeOffset.UtcNow);
        var adapter = new MisallocatingCoverageAdapter(primaryTotal: 60_000m, allocatedTotal: 50_000m);
        var service = CreateService(db, adapter);

        var exception = await Assert.ThrowsAsync<BillingCalculationValidationException>(() =>
            service.RecalculateAsync(
                invoice.Id, Request(invoice.RowVersion, "Alokasi tidak menjumlah"), Guid.NewGuid(), CancellationToken.None));

        Assert.Contains("tidak menjumlah ke total tanggungan", exception.Message);
        Assert.Empty(db.BilCalculationVersions);
    }

    // BE-BKC-022 / BKC-DES-004: perhitungan yang baru dijalankan selalu menyatakan rinciannya
    // tersedia. Penanda inilah yang membedakan "penjamin menanggung Rp 0" dari "kami tidak punya
    // rinciannya" - dan consumer wajib memeriksanya, bukan memeriksa versi kontrak kalkulasi.
    [Fact]
    public async Task PerhitunganBaruMenyatakanRincianPerBarisTersedia()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", DateTimeOffset.UtcNow);
        var service = CreateService(db, new AllocatingCoverageAdapter("APPROVED", "NOT_CONFIGURED", 60_000m, 0, 0));

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Coverage dihitung"), Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.Breakdown.Coverage.IsPerItemAllocationAvailable);
    }

    // BE-BKC-022 / BIL-AT-050: versi kalkulasi yang tersimpan sebelum penanda ini ada tidak memuat
    // propertinya sama sekali. Deserialisasi MUST menghasilkan false tanpa galat - bukan true, dan
    // bukan melempar - supaya rincian Rp 0 milik snapshot lama tidak terbaca sebagai angka sungguhan.
    [Fact]
    public void SnapshotLamaTanpaPenandaTerbacaSebagaiRincianTidakTersedia()
    {
        const string snapshotLama = """
            {"contractVersion":"BIL-CALCULATION-0.4","primaryStatus":"APPROVED","excessStatus":"NOT_CONFIGURED",
             "eligibleAmount":100000,"primaryAmount":60000,"residualAfterPrimary":40000,"excessAmount":0,
             "residualAfterExcess":40000,"unresolvedAmount":0,"patientAmount":40000,"appliedRuleIds":[]}
            """;

        var coverage = JsonSerializer.Deserialize<CoverageCalculationResponse>(
            snapshotLama, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(coverage);
        Assert.Equal(60_000m, coverage!.PrimaryAmount);
        Assert.False(coverage.IsPerItemAllocationAvailable);
    }

    [Fact]
    public async Task AdministrationFeeCoverableFlagGatesWhetherInsurerCanCoverIt()
    {
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        // Item dasar 100.000 (selalu coverable, lihat SeedInvoiceAsync) + admin fee 20.000 -> total
        // eligible 120.000. Decision penjamin mencoba menanggung SELURUH 120.000, termasuk admin
        // fee - hanya sah bila policy admin fee Coverable=true.
        // Alokasi diturunkan dari komponen yang benar-benar diterima adapter, bukan daftar
        // hardcoded: BIL-VAL-028 menolak decision yang totalnya tidak beralamat baris. Pada cabang
        // admin fee tidak coverable, komponen yang tersedia hanya menyerap 100.000 sementara total
        // yang dideklarasikan tetap 120.000 - penjaga cap yang berjalan lebih dulu tetap menangkapnya.
        static AllocatingCoverageAdapter NewAdapter() =>
            new("APPROVED", "NOT_CONFIGURED", 120_000m, 0, 0);

        await using (var coverableDb = IsolatedBillingDbContextFactory.Create())
        {
            coverableDb.MstAdministrationFeePolicies.Add(
                AdministrationPolicy("ADM-COVERABLE", "RAJAL", 20_000m, 10, at, coverable: true));
            var invoice = await SeedInvoiceAsync(coverableDb, Guid.NewGuid(), "RAJAL", at);
            await coverableDb.SaveChangesAsync();
            var service = CreateService(coverableDb, NewAdapter());

            var result = await service.RecalculateAsync(
                invoice.Id, Request(invoice.RowVersion, "Admin fee coverable"), Guid.NewGuid(), CancellationToken.None);

            Assert.Equal(20_000m, result.AdministrationFeeAmount);
            Assert.Equal(120_000m, result.PrimaryAmount);
            Assert.Equal(0m, result.PatientAmount);
        }

        await using (var notCoverableDb = IsolatedBillingDbContextFactory.Create())
        {
            notCoverableDb.MstAdministrationFeePolicies.Add(
                AdministrationPolicy("ADM-NOT-COVERABLE", "RAJAL", 20_000m, 10, at, coverable: false));
            var invoice = await SeedInvoiceAsync(notCoverableDb, Guid.NewGuid(), "RAJAL", at);
            await notCoverableDb.SaveChangesAsync();
            var service = CreateService(notCoverableDb, NewAdapter());

            var exception = await Assert.ThrowsAsync<BillingCalculationValidationException>(() =>
                service.RecalculateAsync(
                    invoice.Id, Request(invoice.RowVersion, "Admin fee tidak coverable"), Guid.NewGuid(), CancellationToken.None));

            Assert.Contains("melebihi biaya", exception.Message);
        }
    }

    // BE-BKC-017 hardening (26 Agustus 2026): CalculateAdministrationFeeAsync mendapat SQL pre-filter
    // pada TrxPatientEncounter.EncounterDate (menggantikan penarikan seluruh riwayat pasien ke memori)
    // - lihat catatan di BillingCalculationService.cs. Test ini secara khusus membuktikan pre-filter
    // itu tetap benar untuk dua encounter yang berada pada businessDate WIB YANG SAMA tapi tanggal
    // kalender UTC-nya BERBEDA (melintasi batas 17:00 UTC), skenario yang akan salah bila pre-filter
    // naif hanya membandingkan tanggal kalender UTC alih-alih rentang WIB yang benar.
    [Fact]
    public async Task AdministrationFeeAcrossUtcMidnightBoundaryIsStillDetectedAsSameBusinessDay()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var patientId = Guid.NewGuid();
        // 2026-08-21T18:00:00Z = WIB 2026-08-22 01:00 (awal businessDate WIB 22 Agustus, tanggal kalender UTC-nya 21 Agustus).
        var firstAt = new DateTimeOffset(2026, 8, 21, 18, 0, 0, TimeSpan.Zero);
        // 2026-08-22T10:00:00Z = WIB 2026-08-22 17:00 (masih businessDate WIB yang sama, tanggal kalender UTC-nya 22 Agustus).
        var secondAt = new DateTimeOffset(2026, 8, 22, 10, 0, 0, TimeSpan.Zero);
        Assert.Equal(
            AdministrationFeePolicyService.GetBusinessDate(firstAt),
            AdministrationFeePolicyService.GetBusinessDate(secondAt));
        Assert.NotEqual(firstAt.UtcDateTime.Date, secondAt.UtcDateTime.Date);

        db.MstAdministrationFeePolicies.Add(AdministrationPolicy("ADM-RAJAL-BOUNDARY", "RAJAL", 20_000m, 10, firstAt));
        var first = await SeedInvoiceAsync(db, patientId, "RAJAL", firstAt);
        var second = await SeedInvoiceAsync(db, patientId, "RAJAL", secondAt);
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var firstResult = await service.RecalculateAsync(first.Id, Request(first.RowVersion, "Kunjungan pertama"), Guid.NewGuid(), CancellationToken.None);
        var secondResult = await service.RecalculateAsync(second.Id, Request(second.RowVersion, "Kunjungan kedua, businessDate WIB sama"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(20_000m, firstResult.AdministrationFeeAmount);
        Assert.Equal(0, secondResult.AdministrationFeeAmount);
    }

    [Fact]
    public async Task RegistrationCoverageAdapterUsesApprovedGenericPrimaryRule()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        var providerId = Guid.NewGuid();
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = invoice.EncounterId,
            PatientId = (await db.TrxPatientEncounters.FindAsync(invoice.EncounterId))!.PatientId,
            PaymentSourceNumber = "PAY-TEST",
            PaymentType = EncounterPaymentType.Insurance,
            InsuranceProviderId = providerId,
            IsEligible = true,
            IsPolicyActive = true,
            IsActive = true
        });
        db.MstInsuranceCoverageRules.Add(new MstInsuranceCoverageRule
        {
            InsuranceProviderId = providerId,
            RuleCode = "COV-PROC",
            RuleName = "Coverage procedure test",
            ItemType = "Procedure",
            CoverageStatus = "Covered",
            CoveragePercent = 80,
            IsAllowExcessPaymentByPatient = true,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new RegistrationBillingCoverageAdapter(db));

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Coverage primary"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(80_000m, result.PrimaryAmount);
        Assert.Equal(20_000m, result.PatientAmount);
        Assert.Equal("NOT_CONFIGURED", result.Breakdown.Coverage.ExcessStatus);
        Assert.Single(result.Breakdown.Coverage.AppliedRuleIds);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task RegistrationCoverageAdapterCoversItemEvenWhenRuleNeedsApprovalOrGuaranteeLetter(
        bool needApproval, bool needGuaranteeLetter)
    {
        // BKC-DEC-062 (amendment BKC-DEC-042): rule Covered yang butuh approval dan/atau surat
        // jaminan TIDAK LAGI digeser ke unresolved - approval/SJP adalah proses administratif
        // terpisah, bukan penolakan coverage. Sebelum perbaikan ini, ketiga kombinasi flag di atas
        // akan menghasilkan PrimaryAmount=0 dan seluruh gross jatuh ke UnresolvedAmount.
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        var providerId = Guid.NewGuid();
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = invoice.EncounterId,
            PatientId = (await db.TrxPatientEncounters.FindAsync(invoice.EncounterId))!.PatientId,
            PaymentSourceNumber = "PAY-APPROVAL",
            PaymentType = EncounterPaymentType.Insurance,
            InsuranceProviderId = providerId,
            IsEligible = true,
            IsPolicyActive = true,
            IsActive = true
        });
        db.MstInsuranceCoverageRules.Add(new MstInsuranceCoverageRule
        {
            InsuranceProviderId = providerId,
            RuleCode = "COV-APPROVAL",
            RuleName = "Coverage butuh approval/SJP",
            ItemType = "Procedure",
            CoverageStatus = "Covered",
            CoveragePercent = 80,
            IsNeedApproval = needApproval,
            IsNeedGuaranteeLetter = needGuaranteeLetter,
            IsAllowExcessPaymentByPatient = true,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new RegistrationBillingCoverageAdapter(db));

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Coverage butuh approval tetap dihitung"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(80_000m, result.PrimaryAmount);
        Assert.Equal(20_000m, result.PatientAmount);
        Assert.Equal(0m, result.Breakdown.Coverage.UnresolvedAmount);
    }

    // BE-BKC-024 / BIL-AT-036 / BKC-DEC-071: CoverageStatus="NeedApproval" TIDAK LAGI menggeser
    // komponen ke unresolved (gate dicabut penuh, kembali dari
    // RegistrationCoverageAdapterStillGatesRuleWithNeedApprovalCoverageStatus di rilis sebelumnya).
    // Rule dengan status ini kini dihitung persis seperti Covered biasa.
    [Fact]
    public async Task RegistrationCoverageAdapterNoLongerGatesRuleWithNeedApprovalCoverageStatus()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        var providerId = Guid.NewGuid();
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = invoice.EncounterId,
            PatientId = (await db.TrxPatientEncounters.FindAsync(invoice.EncounterId))!.PatientId,
            PaymentSourceNumber = "PAY-STATUS",
            PaymentType = EncounterPaymentType.Insurance,
            InsuranceProviderId = providerId,
            IsEligible = true,
            IsPolicyActive = true,
            IsActive = true
        });
        db.MstInsuranceCoverageRules.Add(new MstInsuranceCoverageRule
        {
            InsuranceProviderId = providerId,
            RuleCode = "COV-NEED-APPROVAL-STATUS",
            RuleName = "Status coverage belum diputuskan",
            ItemType = "Procedure",
            CoverageStatus = "NeedApproval",
            CoveragePercent = 80,
            IsAllowExcessPaymentByPatient = true,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new RegistrationBillingCoverageAdapter(db));

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Status coverage belum diputuskan, tetap dihitung"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(80_000m, result.PrimaryAmount);
        Assert.Equal(20_000m, result.PatientAmount);
        Assert.Equal(0m, result.Breakdown.Coverage.UnresolvedAmount);
    }

    // BE-BKC-024 / BIL-AT-037 / BKC-DEC-071 (jawaban pemilik atas BKC-CQ-07): limit bulanan
    // (MaxAmountPerMonth/MaxQuantityPerMonth) TIDAK LAGI menggeser komponen ke unresolved - kembali
    // dari RegistrationCoverageAdapterStillGatesRuleWithMonthlyLimit di rilis sebelumnya. Diperlakukan
    // SELALU TERSEDIA sampai mesin pemakaian kumulatif dibangun (coverage gap tertunda, bukan
    // bagian rilis ini) - lihat 01-existing-capability-map.md 17.4.E.
    [Fact]
    public async Task RegistrationCoverageAdapterNoLongerGatesRuleWithMonthlyLimit()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        var providerId = Guid.NewGuid();
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = invoice.EncounterId,
            PatientId = (await db.TrxPatientEncounters.FindAsync(invoice.EncounterId))!.PatientId,
            PaymentSourceNumber = "PAY-MONTHLY",
            PaymentType = EncounterPaymentType.Insurance,
            InsuranceProviderId = providerId,
            IsEligible = true,
            IsPolicyActive = true,
            IsActive = true
        });
        db.MstInsuranceCoverageRules.Add(new MstInsuranceCoverageRule
        {
            InsuranceProviderId = providerId,
            RuleCode = "COV-MONTHLY-LIMIT",
            RuleName = "Limit bulanan",
            ItemType = "Procedure",
            CoverageStatus = "Covered",
            CoveragePercent = 80,
            MaxAmountPerMonth = 50_000m,
            MaxQuantityPerMonth = 1,
            IsAllowExcessPaymentByPatient = true,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new RegistrationBillingCoverageAdapter(db));

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Limit bulanan diperlakukan selalu tersedia"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(80_000m, result.PrimaryAmount);
        Assert.Equal(20_000m, result.PatientAmount);
        Assert.Equal(0m, result.Breakdown.Coverage.UnresolvedAmount);
    }

    // BE-BKC-024 (regresi wajib diperiksa): batas PER KUNJUNGAN (MaxAmountPerVisit) bertetangga
    // dengan limit bulanan yang baru saja dicabut di atas, tapi BKC-DEC-071 hanya mencabut limit
    // bulanan. Rule di sini sengaja mengisi MaxAmountPerMonth (harus diabaikan) BERSAMA
    // MaxAmountPerVisit (harus tetap menjepit) supaya keduanya tidak tertukar.
    [Fact]
    public async Task RegistrationCoverageAdapterStillEnforcesPerVisitLimit()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        var providerId = Guid.NewGuid();
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = invoice.EncounterId,
            PatientId = (await db.TrxPatientEncounters.FindAsync(invoice.EncounterId))!.PatientId,
            PaymentSourceNumber = "PAY-PER-VISIT",
            PaymentType = EncounterPaymentType.Insurance,
            InsuranceProviderId = providerId,
            IsEligible = true,
            IsPolicyActive = true,
            IsActive = true
        });
        db.MstInsuranceCoverageRules.Add(new MstInsuranceCoverageRule
        {
            InsuranceProviderId = providerId,
            RuleCode = "COV-PER-VISIT-LIMIT",
            RuleName = "Batas per kunjungan tetap berlaku",
            ItemType = "Procedure",
            CoverageStatus = "Covered",
            CoveragePercent = 80,
            MaxAmountPerVisit = 30_000m,
            MaxAmountPerMonth = 999_999m,
            IsAllowExcessPaymentByPatient = true,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new RegistrationBillingCoverageAdapter(db));

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Batas per kunjungan tetap menjepit"), Guid.NewGuid(), CancellationToken.None);

        // 80% dari 100.000 = 80.000, tetapi dijepit ke MaxAmountPerVisit = 30.000. Sisanya
        // (70.000) menjadi porsi pasien karena IsAllowExcessPaymentByPatient = true (bukan unresolved).
        Assert.Equal(30_000m, result.PrimaryAmount);
        Assert.Equal(70_000m, result.PatientAmount);
        Assert.Equal(0m, result.Breakdown.Coverage.UnresolvedAmount);
    }

    // BE-BKC-024 / BIL-AT-039: aturan NotCovered dengan IsAllowExcessPaymentByPatient = true membuat
    // seluruh nominal menjadi porsi pasien (bukan unresolved). Cabang ini tidak disentuh task ini,
    // tapi termasuk acceptance criteria yang wajib diverifikasi tetap benar.
    [Fact]
    public async Task RegistrationCoverageAdapterNotCoveredRuleWithExcessAllowedBecomesPatientPortion()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        var providerId = Guid.NewGuid();
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = invoice.EncounterId,
            PatientId = (await db.TrxPatientEncounters.FindAsync(invoice.EncounterId))!.PatientId,
            PaymentSourceNumber = "PAY-NOT-COVERED",
            PaymentType = EncounterPaymentType.Insurance,
            InsuranceProviderId = providerId,
            IsEligible = true,
            IsPolicyActive = true,
            IsActive = true
        });
        db.MstInsuranceCoverageRules.Add(new MstInsuranceCoverageRule
        {
            InsuranceProviderId = providerId,
            RuleCode = "COV-NOT-COVERED-EXCESS",
            RuleName = "NotCovered, excess diizinkan ke pasien",
            ItemType = "Procedure",
            CoverageStatus = "NotCovered",
            IsAllowExcessPaymentByPatient = true,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new RegistrationBillingCoverageAdapter(db));

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "NotCovered jadi porsi pasien"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(0m, result.PrimaryAmount);
        Assert.Equal(100_000m, result.PatientAmount);
        Assert.Equal(0m, result.Breakdown.Coverage.UnresolvedAmount);
    }

    // BE-BKC-030 / BIL-AT-062 / BKC-DEC-089 / BKC-DES-026: uji pasangan langsung dengan test di
    // atas - rule NotCovered yang PERSIS sama kecuali IsAllowExcessPaymentByPatient=false kini
    // masuk nonBillableResidual (akumulator yang sama dengan jalur 5), BUKAN unresolvedAmount lagi.
    [Fact]
    public async Task RegistrationCoverageAdapterNotCoveredRuleWithExcessDisallowedBecomesNonBillableResidual()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        var categoryId = (await db.BilInvoiceItems.AsNoTracking().SingleAsync(x => x.InvoiceId == invoice.Id)).CategoryId;
        var providerId = Guid.NewGuid();
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = invoice.EncounterId,
            PatientId = (await db.TrxPatientEncounters.FindAsync(invoice.EncounterId))!.PatientId,
            PaymentSourceNumber = "PAY-NOT-COVERED-NON-BILLABLE",
            PaymentType = EncounterPaymentType.Insurance,
            InsuranceProviderId = providerId,
            IsEligible = true,
            IsPolicyActive = true,
            IsActive = true
        });
        db.MstInsuranceCoverageRules.Add(new MstInsuranceCoverageRule
        {
            InsuranceProviderId = providerId,
            RuleCode = "COV-NOT-COVERED-NON-BILLABLE",
            RuleName = "NotCovered, dilarang ditagih ke pasien",
            ItemType = "ServiceCategory",
            TariffCategoryId = categoryId,
            CoverageStatus = "NotCovered",
            IsAllowExcessPaymentByPatient = false,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new RegistrationBillingCoverageAdapter(db));

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "NotCovered dilarang ditagih ke pasien"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(0m, result.PrimaryAmount);
        Assert.Equal(0m, result.PatientAmount);
        Assert.Equal(0m, result.Breakdown.Coverage.UnresolvedAmount);
        Assert.Equal(100_000m, result.Breakdown.Coverage.NonBillableResidualAmount);
        Assert.True(result.Breakdown.Coverage.HasNonBillableResidual);
        Assert.Equal(100_000m, result.GrossAmount);
    }

    // BE-BKC-030 / BIL-AT-063 / BKC-DES-026: satu tagihan yang memuat nominal dari jalur (2) DAN
    // jalur (5) sekaligus menghasilkan SATU nominal nonBillableResidual gabungan - membuktikan
    // kedua cabang benar-benar memakai akumulator yang sama, bukan dua akumulator yang dijumlahkan
    // belakangan (godaan yang secara eksplisit dilarang BKC-DES-026's catatan desain).
    [Fact]
    public async Task RegistrationCoverageAdapterCombinesNotCoveredAndResidualIntoSingleNonBillableAmount()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        var firstCategoryId = (await db.BilInvoiceItems.AsNoTracking().SingleAsync(x => x.InvoiceId == invoice.Id)).CategoryId;
        var providerId = Guid.NewGuid();
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = invoice.EncounterId,
            PatientId = (await db.TrxPatientEncounters.FindAsync(invoice.EncounterId))!.PatientId,
            PaymentSourceNumber = "PAY-COMBINED",
            PaymentType = EncounterPaymentType.Insurance,
            InsuranceProviderId = providerId,
            IsEligible = true,
            IsPolicyActive = true,
            IsActive = true
        });

        // Item kedua (kategori terpisah): item pertama (dari SeedInvoiceAsync) menempuh jalur (2)
        // NotCovered; item kedua ini menempuh jalur (5) residual dari Covered 70%.
        var secondCategory = new MstTariffCategory
        {
            Id = Guid.NewGuid(),
            TariffCategoryCode = "PROC-2",
            TariffCategoryName = "Procedure test kedua",
            IsProcedure = true,
            IsCoveredByInsuranceDefault = true,
            IsActive = true
        };
        db.MstTariffCategories.Add(secondCategory);
        db.BilInvoiceItems.Add(new BilInvoiceItem
        {
            InvoiceId = invoice.Id,
            SourceDomain = "PROCEDURE",
            SourceDetailId = Guid.NewGuid().ToString(),
            SourceVersion = 1,
            SourceContractVersion = "TEST-1",
            SourceStatus = "CONFIRMED",
            SourceOccurredAt = at,
            CategoryId = secondCategory.Id,
            Category = secondCategory,
            DescriptionSnapshot = "Fisioterapi fiktif",
            Quantity = 1,
            UnitPrice = 100_000m,
            Status = BillingInvoiceItemStatuses.Active,
            SourcePayloadHash = new string('B', 64)
        });
        db.MstInsuranceCoverageRules.Add(new MstInsuranceCoverageRule
        {
            InsuranceProviderId = providerId,
            RuleCode = "COV-COMBINED-NOT-COVERED",
            RuleName = "NotCovered dilarang ditagih ke pasien - item pertama",
            ItemType = "ServiceCategory",
            TariffCategoryId = firstCategoryId,
            CoverageStatus = "NotCovered",
            IsAllowExcessPaymentByPatient = false,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        });
        db.MstInsuranceCoverageRules.Add(new MstInsuranceCoverageRule
        {
            InsuranceProviderId = providerId,
            RuleCode = "COV-COMBINED-RESIDUAL",
            RuleName = "Residual dilarang ditagih ke pasien - item kedua",
            ItemType = "ServiceCategory",
            TariffCategoryId = secondCategory.Id,
            CoverageStatus = "Covered",
            CoveragePercent = 70,
            IsAllowExcessPaymentByPatient = false,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new RegistrationBillingCoverageAdapter(db));

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Jalur (2) dan (5) digabung satu nominal"), Guid.NewGuid(), CancellationToken.None);

        // Item 1 (100.000, NotCovered): seluruhnya nonBillableResidual.
        // Item 2 (100.000, Covered 70%): primary=70.000, residual 30.000 nonBillableResidual.
        Assert.Equal(70_000m, result.PrimaryAmount);
        Assert.Equal(0m, result.PatientAmount);
        Assert.Equal(0m, result.Breakdown.Coverage.UnresolvedAmount);
        Assert.Equal(130_000m, result.Breakdown.Coverage.NonBillableResidualAmount);
        Assert.True(result.Breakdown.Coverage.HasNonBillableResidual);
        Assert.Equal(200_000m, result.GrossAmount);
    }

    // BE-BKC-028 / BIL-AT-055 / BKC-DEC-080 / BKC-DES-021/022: residual jalur (5)
    // (CalculateCoveredAmount, bukan NotCovered) dengan IsAllowExcessPaymentByPatient=false kini
    // masuk NonBillableResidualAmount, BUKAN pasien maupun UnresolvedAmount - menunggu Finance
    // mengajukan write-off kategori NON_BILLABLE_RESIDUAL, bukan menggantung tanpa tindak lanjut.
    [Fact]
    public async Task RegistrationCoverageAdapterResidualBecomesNonBillableWhenExcessNotAllowed()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        var providerId = Guid.NewGuid();
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = invoice.EncounterId,
            PatientId = (await db.TrxPatientEncounters.FindAsync(invoice.EncounterId))!.PatientId,
            PaymentSourceNumber = "PAY-NON-BILLABLE",
            PaymentType = EncounterPaymentType.Insurance,
            InsuranceProviderId = providerId,
            IsEligible = true,
            IsPolicyActive = true,
            IsActive = true
        });
        db.MstInsuranceCoverageRules.Add(new MstInsuranceCoverageRule
        {
            InsuranceProviderId = providerId,
            RuleCode = "COV-NON-BILLABLE",
            RuleName = "Residual dilarang ditagih ke pasien",
            ItemType = "Procedure",
            CoverageStatus = "Covered",
            CoveragePercent = 70,
            IsAllowExcessPaymentByPatient = false,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new RegistrationBillingCoverageAdapter(db));

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Residual tidak boleh ditagih ke pasien"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(70_000m, result.PrimaryAmount);
        Assert.Equal(0m, result.PatientAmount);
        Assert.Equal(0m, result.Breakdown.Coverage.UnresolvedAmount);
        Assert.Equal(30_000m, result.Breakdown.Coverage.NonBillableResidualAmount);
        Assert.True(result.Breakdown.Coverage.HasNonBillableResidual);
        Assert.Equal(100_000m, result.GrossAmount);
    }

    // BE-BKC-028 / BIL-AT-056: uji pasangan langsung dengan test di atas - rule yang PERSIS sama
    // kecuali IsAllowExcessPaymentByPatient=true menghasilkan porsi pasien, BUKAN residual.
    // Membuktikan cabang true amendment ini benar-benar tidak disentuh (BKC-DEC-070).
    [Fact]
    public async Task RegistrationCoverageAdapterResidualStaysPatientPortionWhenExcessAllowed()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        var providerId = Guid.NewGuid();
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = invoice.EncounterId,
            PatientId = (await db.TrxPatientEncounters.FindAsync(invoice.EncounterId))!.PatientId,
            PaymentSourceNumber = "PAY-EXCESS-ALLOWED",
            PaymentType = EncounterPaymentType.Insurance,
            InsuranceProviderId = providerId,
            IsEligible = true,
            IsPolicyActive = true,
            IsActive = true
        });
        db.MstInsuranceCoverageRules.Add(new MstInsuranceCoverageRule
        {
            InsuranceProviderId = providerId,
            RuleCode = "COV-EXCESS-ALLOWED",
            RuleName = "Residual boleh ditagih ke pasien",
            ItemType = "Procedure",
            CoverageStatus = "Covered",
            CoveragePercent = 70,
            IsAllowExcessPaymentByPatient = true,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new RegistrationBillingCoverageAdapter(db));

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Residual boleh ditagih ke pasien"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(70_000m, result.PrimaryAmount);
        Assert.Equal(30_000m, result.PatientAmount);
        Assert.Equal(0m, result.Breakdown.Coverage.NonBillableResidualAmount);
        Assert.False(result.Breakdown.Coverage.HasNonBillableResidual);
    }

    // BE-BKC-028 / BIL-AT-057 / BKC-DES-023: mesin kalkulasi hanya MENDETEKSI dan MENANDAI residual
    // non-billable - ia MUST NOT pernah membuat BilWriteOffCase sendiri (pengajuan tetap perbuatan
    // manusia/Finance). Membuka layar perhitungan berulang (PreviewCalculationAsync, dipanggil
    // Menu Pembayaran setiap kali dibuka) TIDAK BOLEH melahirkan satu pun kasus penanggungan.
    [Fact]
    public async Task CalculationEnginePreviewNeverCreatesWriteOffCaseForNonBillableResidual()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        var providerId = Guid.NewGuid();
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = invoice.EncounterId,
            PatientId = (await db.TrxPatientEncounters.FindAsync(invoice.EncounterId))!.PatientId,
            PaymentSourceNumber = "PAY-NO-AUTO-WRITEOFF",
            PaymentType = EncounterPaymentType.Insurance,
            InsuranceProviderId = providerId,
            IsEligible = true,
            IsPolicyActive = true,
            IsActive = true
        });
        db.MstInsuranceCoverageRules.Add(new MstInsuranceCoverageRule
        {
            InsuranceProviderId = providerId,
            RuleCode = "COV-NO-AUTO-WRITEOFF",
            RuleName = "Residual non-billable, tidak boleh melahirkan write-off otomatis",
            ItemType = "Procedure",
            CoverageStatus = "Covered",
            CoveragePercent = 70,
            IsAllowExcessPaymentByPatient = false,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new RegistrationBillingCoverageAdapter(db));

        for (var i = 0; i < 10; i++)
        {
            var preview = await service.PreviewCalculationAsync(invoice.Id, Guid.NewGuid(), CancellationToken.None);
            Assert.Equal(30_000m, preview.Breakdown.Coverage.NonBillableResidualAmount);
        }

        Assert.Empty(db.BilWriteOffCases);
        Assert.Empty(db.BilCalculationVersions);
    }

    // BE-BKC-025 / BIL-AT-041 / BKC-DEC-073 / BKC-DES-010/011: penjamin belum dinyatakan layak
    // (IsEligible=false) TIDAK LAGI membuat komponen menggantung (unresolved) - kalkulasi
    // BERHASIL, seluruh biaya coverable jatuh ke pasien, dan kode PAYER_NOT_ELIGIBLE tercatat
    // sebagai anomali data (bukan tagihan yang tidak dapat dialokasikan ke siapa pun seperti
    // perilaku "Penjamin Belum Terverifikasi" sebelumnya).
    [Fact]
    public async Task RegistrationCoverageAdapterPayerNotEligibleBecomesAnomalyAndPatientPortion()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        var providerId = Guid.NewGuid();
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = invoice.EncounterId,
            PatientId = (await db.TrxPatientEncounters.FindAsync(invoice.EncounterId))!.PatientId,
            PaymentSourceNumber = "PAY-NOT-ELIGIBLE",
            PaymentType = EncounterPaymentType.Insurance,
            InsuranceProviderId = providerId,
            IsEligible = false,
            IsPolicyActive = true,
            IsActive = true
        });
        // Rule Covered SENGAJA disediakan untuk membuktikan anomali bergantung pada precondition
        // penjamin (IsEligible), bukan pada ketiadaan rule yang cocok (itu jalur (1), sudah
        // tercakup RegistrationCoverageAdapterUsesApprovedGenericPrimaryRule).
        db.MstInsuranceCoverageRules.Add(new MstInsuranceCoverageRule
        {
            InsuranceProviderId = providerId,
            RuleCode = "COV-NOT-ELIGIBLE",
            RuleName = "Coverage tersedia tetapi penjamin belum eligible",
            ItemType = "Procedure",
            CoverageStatus = "Covered",
            CoveragePercent = 100,
            IsAllowExcessPaymentByPatient = true,
            EffectiveStartDate = DateTime.UtcNow.AddDays(-1),
            EffectiveEndDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new RegistrationBillingCoverageAdapter(db));

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Penjamin belum eligible"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(0m, result.PrimaryAmount);
        Assert.Equal(100_000m, result.PatientAmount);
        Assert.Equal(0m, result.Breakdown.Coverage.UnresolvedAmount);
        Assert.Equal(100_000m, result.Breakdown.Coverage.DataAnomalyAmount);
        Assert.True(result.Breakdown.Coverage.HasDataAnomaly);
        Assert.Equal(["PAYER_NOT_ELIGIBLE"], result.Breakdown.Coverage.AnomalyCodes);
        Assert.Single(result.Breakdown.Coverage.AnomalyMessages);
        // BE-BKC-028 / BIL-AT-055 acceptance 5: jalur anomali data tetap mengembalikan selisih
        // non-billable bernilai nol - Anomaly() TIDAK PERNAH mengisi NonBillableResidualAmount.
        Assert.Equal(0m, result.Breakdown.Coverage.NonBillableResidualAmount);
        Assert.False(result.Breakdown.Coverage.HasNonBillableResidual);
        Assert.Contains("Registrasi", result.Breakdown.Coverage.AnomalyMessages[0]);
    }

    // BE-BKC-025 / BIL-AT-042: polis tercatat tidak aktif menghasilkan kode POLICY_INACTIVE dengan
    // perilaku nominal yang sama dengan PAYER_NOT_ELIGIBLE - logikanya sudah dibuktikan test di
    // atas, di sini hanya membuktikan kodenya berbeda dan tepat.
    [Fact]
    public async Task RegistrationCoverageAdapterPolicyInactiveBecomesAnomalyWithCorrectCode()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        var providerId = Guid.NewGuid();
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = invoice.EncounterId,
            PatientId = (await db.TrxPatientEncounters.FindAsync(invoice.EncounterId))!.PatientId,
            PaymentSourceNumber = "PAY-POLICY-INACTIVE",
            PaymentType = EncounterPaymentType.Insurance,
            InsuranceProviderId = providerId,
            IsEligible = true,
            IsPolicyActive = false,
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new RegistrationBillingCoverageAdapter(db));

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Polis tidak aktif"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(100_000m, result.PatientAmount);
        Assert.Equal(100_000m, result.Breakdown.Coverage.DataAnomalyAmount);
        Assert.Equal(["POLICY_INACTIVE"], result.Breakdown.Coverage.AnomalyCodes);
    }

    // BE-BKC-025: penjamin berjenis Insurance tetapi InsuranceProviderId kosong menghasilkan kode
    // INSURANCE_PROVIDER_MISSING - keadaan data yang seharusnya dicegah form registrasi, tetapi
    // adapter tetap MUST menanganinya sebagai anomali, bukan melempar galat tak tertangani.
    // Cakupan DoD "empat kode dihasilkan pada keadaan yang benar"; ENCOUNTER_NOT_FOUND tidak diuji
    // di sini karena CalculateAsync sudah memuat encounter lebih dulu sebelum memanggil adapter -
    // jalur itu seharusnya tidak dapat dipicu lewat pipeline normal (dicatat sebagai keterbatasan
    // pada laporan task, bukan diuji dengan cara yang dipaksakan).
    [Fact]
    public async Task RegistrationCoverageAdapterMissingInsuranceProviderBecomesAnomalyWithCorrectCode()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", at, isProcedure: true);
        db.TrxPatientEncounterGuarantors.Add(new TrxPatientEncounterGuarantor
        {
            EncounterId = invoice.EncounterId,
            PatientId = (await db.TrxPatientEncounters.FindAsync(invoice.EncounterId))!.PatientId,
            PaymentSourceNumber = "PAY-PROVIDER-MISSING",
            PaymentType = EncounterPaymentType.Insurance,
            InsuranceProviderId = null,
            IsEligible = true,
            IsPolicyActive = true,
            IsActive = true
        });
        await db.SaveChangesAsync();
        var service = CreateService(db, new RegistrationBillingCoverageAdapter(db));

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Perusahaan asuransi belum dipilih"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(100_000m, result.PatientAmount);
        Assert.Equal(["INSURANCE_PROVIDER_MISSING"], result.Breakdown.Coverage.AnomalyCodes);
    }

    [Fact]
    public async Task StaleRowVersionAndClosedInvoiceAreRejected()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var invoice = await SeedInvoiceAsync(db, Guid.NewGuid(), "RAJAL", DateTimeOffset.UtcNow);
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        await Assert.ThrowsAsync<BillingCalculationConflictException>(() => service.RecalculateAsync(
            invoice.Id, Request(Guid.NewGuid(), "Versi stale"), Guid.NewGuid(), CancellationToken.None));

        invoice.Status = BillingInvoiceStatuses.Final;
        await db.SaveChangesAsync();
        await Assert.ThrowsAsync<BillingCalculationValidationException>(() => service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Final tidak boleh berubah"), Guid.NewGuid(), CancellationToken.None));
    }

    // BE-BKC-018 (BKC-DEC-043): Room charge dihitung ulang penuh setiap recalculate langsung dari
    // InpBedPlacement (occupancy timeline), bukan BilInvoiceItem - lihat CalculateRoomChargeAsync.
    [Fact]
    public async Task RoomChargeAppliesCeilingRoundingForClosedSegmentAtOccupancyStartTariff()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var patientId = Guid.NewGuid();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, patientId, "RANAP", at);
        var serviceUnitId = Guid.NewGuid();
        var patientClassId = Guid.NewGuid();
        var placementStart = at.UtcDateTime.AddDays(-2);
        // 26 jam = 1560 menit -> ceiling(1560/1440) = 2 unit.
        var placementEnd = placementStart.AddHours(26);
        db.MstRoomChargePolicies.Add(RoomChargePolicy(
            "RCP-CEIL", 1440, 1440, RoomChargePolicyValues.CeilingPeriod, RoomChargePolicyValues.OccupancyStart, at));
        db.Set<MstTariff>().Add(RoomTariff("TRF-ROOM-CEIL", serviceUnitId, patientClassId, 300_000m));
        await SeedInpatientOccupancyAsync(
            db, invoice.EncounterId, patientId, serviceUnitId, patientClassId, placementStart, placementEnd);
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Room charge segment tertutup"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(600_000m, result.RoomChargeAmount);
        var segment = Assert.Single(result.Breakdown.RoomCharge.Segments);
        Assert.Equal(2, segment.ChargeUnits);
        Assert.False(segment.IsOngoing);
        Assert.False(segment.MissingTariff);
    }

    // BKC-DEC-043 "tarif awal periode": perubahan tarif di tengah rawat inap hanya berlaku untuk
    // periode SETELAH perubahan - periode yang sudah lewat tetap memakai tarif saat periode itu
    // mulai, sehingga histori tidak pernah ditimpa retroaktif.
    [Fact]
    public async Task RoomChargeWithPeriodStartTariffMomentOnlyAppliesNewRateToLaterPeriods()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var patientId = Guid.NewGuid();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, patientId, "RANAP", at);
        var serviceUnitId = Guid.NewGuid();
        var patientClassId = Guid.NewGuid();
        var placementStart = at.UtcDateTime.AddDays(-3);
        // 3 hari penuh (4320 menit), whole-periods, tidak ada sisa - unit = 3 tepat.
        var placementEnd = placementStart.AddMinutes(3 * 1440);
        var rateChangeAt = placementStart.AddMinutes(1440);
        db.MstRoomChargePolicies.Add(RoomChargePolicy(
            "RCP-PERIOD", 1440, 1440, RoomChargePolicyValues.WholePeriods, RoomChargePolicyValues.PeriodStart, at));
        db.Set<MstTariff>().AddRange(
            RoomTariff("TRF-ROOM-OLD", serviceUnitId, patientClassId, 200_000m, effectiveEnd: rateChangeAt),
            RoomTariff("TRF-ROOM-NEW", serviceUnitId, patientClassId, 250_000m, effectiveStart: rateChangeAt));
        await SeedInpatientOccupancyAsync(
            db, invoice.EncounterId, patientId, serviceUnitId, patientClassId, placementStart, placementEnd);
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Room charge tarif berubah di tengah"), Guid.NewGuid(), CancellationToken.None);

        // Periode 1 tetap tarif lama (200.000); periode 2 dan 3 tarif baru (250.000 x 2).
        Assert.Equal(700_000m, result.RoomChargeAmount);
    }

    [Fact]
    public async Task RoomChargeChargesOngoingSegmentLiveUpToCalculationTime()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var patientId = Guid.NewGuid();
        var at = DateTimeOffset.UtcNow;
        var invoice = await SeedInvoiceAsync(db, patientId, "RANAP", at);
        var serviceUnitId = Guid.NewGuid();
        var patientClassId = Guid.NewGuid();
        var placementStart = at.UtcDateTime.AddDays(-2);
        db.MstRoomChargePolicies.Add(RoomChargePolicy(
            "RCP-LIVE", 1440, 1440, RoomChargePolicyValues.CeilingPeriod, RoomChargePolicyValues.OccupancyStart, at));
        db.Set<MstTariff>().Add(RoomTariff("TRF-ROOM-LIVE", serviceUnitId, patientClassId, 300_000m));
        // EndDateTime null - pasien masih dirawat; dihitung sampai sekarang.
        await SeedInpatientOccupancyAsync(
            db, invoice.EncounterId, patientId, serviceUnitId, patientClassId, placementStart, null);
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Room charge segment berjalan"), Guid.NewGuid(), CancellationToken.None);

        Assert.True(result.RoomChargeAmount > 0);
        var segment = Assert.Single(result.Breakdown.RoomCharge.Segments);
        Assert.True(segment.IsOngoing);
        Assert.Null(segment.EndDateTime);
    }

    [Fact]
    public async Task RoomChargeFlagsMissingTariffInsteadOfFailingRecalculation()
    {
        await using var db = IsolatedBillingDbContextFactory.Create();
        var patientId = Guid.NewGuid();
        var at = new DateTimeOffset(2026, 8, 21, 2, 0, 0, TimeSpan.Zero);
        var invoice = await SeedInvoiceAsync(db, patientId, "RANAP", at);
        var serviceUnitId = Guid.NewGuid();
        var patientClassId = Guid.NewGuid();
        var placementStart = at.UtcDateTime.AddDays(-2);
        var placementEnd = placementStart.AddHours(26);
        db.MstRoomChargePolicies.Add(RoomChargePolicy(
            "RCP-MISSING", 1440, 1440, RoomChargePolicyValues.CeilingPeriod, RoomChargePolicyValues.OccupancyStart, at));
        // Sengaja tidak menambahkan MstTariff sama sekali - BKC-BLK-DATA-001 (seed belum diserahkan).
        await SeedInpatientOccupancyAsync(
            db, invoice.EncounterId, patientId, serviceUnitId, patientClassId, placementStart, placementEnd);
        await db.SaveChangesAsync();
        var service = CreateService(db, SelfPayCoverageAdapter.Instance);

        var result = await service.RecalculateAsync(
            invoice.Id, Request(invoice.RowVersion, "Room charge tanpa tarif"), Guid.NewGuid(), CancellationToken.None);

        Assert.Equal(0, result.RoomChargeAmount);
        var segment = Assert.Single(result.Breakdown.RoomCharge.Segments);
        Assert.True(segment.MissingTariff);
        Assert.Equal(2, segment.ChargeUnits);
    }

    [Fact]
    public void CalculationVersionHasUniqueInvoiceVersionIndex()
    {
        using var db = IsolatedBillingDbContextFactory.Create();
        var entity = db.Model.FindEntityType(typeof(BilCalculationVersion));
        var index = entity!.GetIndexes().Single(x =>
            x.Properties.Select(p => p.Name).SequenceEqual([nameof(BilCalculationVersion.InvoiceId), nameof(BilCalculationVersion.VersionNo)]));
        Assert.True(index.IsUnique);
    }

    private static BillingCalculationService CreateService(Repositories.ApplicationDbContext db, IBillingCoverageAdapter adapter) =>
        new(
            db,
            adapter,
            new BillingAllocationService(
                db,
                new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor())),
            new LoggerService(NullLogger<LoggerService>.Instance, new HttpContextAccessor()));

    private static RecalculateInvoiceRequest Request(Guid rowVersion, string reason) => new()
    {
        ExpectedRowVersion = rowVersion,
        Reason = reason
    };

    private static async Task<BilInvoice> SeedInvoiceAsync(
        Repositories.ApplicationDbContext db,
        Guid patientId,
        string serviceType,
        DateTimeOffset at,
        bool isProcedure = false,
        bool isPharmacy = false)
    {
        var encounter = new TrxPatientEncounter
        {
            Id = Guid.NewGuid(),
            EncounterNumber = $"ENC-{Guid.NewGuid():N}",
            PatientId = patientId,
            ServiceUnitId = Guid.NewGuid(),
            EncounterType = serviceType == "RANAP" ? EncounterType.Inpatient : EncounterType.Outpatient,
            EncounterStatus = EncounterStatus.Registered,
            EncounterDate = at.UtcDateTime,
            IsActive = true
        };
        var category = new MstTariffCategory
        {
            Id = Guid.NewGuid(),
            TariffCategoryCode = "PROC",
            TariffCategoryName = "Procedure test",
            IsProcedure = isProcedure,
            IsPharmacy = isPharmacy,
            IsCoveredByInsuranceDefault = true,
            IsActive = true
        };
        var invoice = new BilInvoice
        {
            EncounterId = encounter.Id,
            InvoiceNumber = $"BIL-{Guid.NewGuid():N}",
            ServiceType = serviceType,
            Status = BillingInvoiceStatuses.Open,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = at.UtcDateTime
        };
        invoice.Items.Add(new BilInvoiceItem
        {
            InvoiceId = invoice.Id,
            SourceDomain = isProcedure ? "PROCEDURE" : "SERVICE",
            SourceDetailId = Guid.NewGuid().ToString(),
            SourceVersion = 1,
            SourceContractVersion = "TEST-1",
            SourceStatus = "CONFIRMED",
            SourceOccurredAt = at,
            CategoryId = category.Id,
            Category = category,
            DescriptionSnapshot = "Pelayanan fiktif",
            Quantity = 1,
            UnitPrice = 100_000m,
            Status = BillingInvoiceItemStatuses.Active,
            SourcePayloadHash = new string('A', 64)
        });

        db.TrxPatientEncounters.Add(encounter);
        db.MstTariffCategories.Add(category);
        db.BilInvoices.Add(invoice);
        await db.SaveChangesAsync();
        return invoice;
    }

    private static MstTaxRule TaxRule(string category, DateTimeOffset at) => new()
    {
        Code = "TAX-TEST",
        Name = "Tax test",
        TaxableCategory = category,
        Rate = 11,
        RoundingMode = TaxRuleValues.HalfUp,
        AllocationRule = TaxRuleValues.Proportional,
        EffectiveFrom = at.AddDays(-1),
        EffectiveTo = at.AddDays(1),
        IsActive = true
    };

    private static MstAdministrationFeePolicy AdministrationPolicy(
        string code,
        string serviceType,
        decimal amount,
        int priority,
        DateTimeOffset at,
        bool coverable = false) => new()
    {
        Code = code,
        Name = code,
        ServiceType = serviceType,
        Amount = amount,
        OncePerPatientLocalDay = true,
        ReplacementPriority = priority,
        Coverable = coverable,
        Discountable = false,
        EffectiveFrom = at.AddDays(-1),
        EffectiveTo = at.AddDays(1),
        IsActive = true
    };

    private static async Task SeedInpatientOccupancyAsync(
        Repositories.ApplicationDbContext db,
        Guid encounterId,
        Guid patientId,
        Guid serviceUnitId,
        Guid patientClassId,
        DateTime startDateTime,
        DateTime? endDateTime)
    {
        var episode = new InpEpisode
        {
            EpisodeNumber = $"EP-{Guid.NewGuid():N}",
            EncounterId = encounterId,
            PatientId = patientId,
            ServiceUnitId = serviceUnitId,
            PatientClassId = patientClassId,
            EpisodeStatus = InpEpisodeStatus.Admitted,
            IsActive = true
        };
        db.Set<InpEpisode>().Add(episode);
        db.Set<InpBedPlacement>().Add(new InpBedPlacement
        {
            EpisodeId = episode.Id,
            BedId = Guid.NewGuid(),
            RoomId = Guid.NewGuid(),
            ServiceUnitId = serviceUnitId,
            PatientClassId = patientClassId,
            SequenceNumber = 1,
            StartDateTime = startDateTime,
            EndDateTime = endDateTime,
            EndReason = endDateTime.HasValue ? InpBedPlacementEndReason.Transfer : null,
            PlacedByUserId = Guid.NewGuid(),
            IsActive = true
        });
        await Task.CompletedTask;
    }

    private static MstRoomChargePolicy RoomChargePolicy(
        string code,
        int minimumMinutes,
        int periodMinutes,
        string remainderRounding,
        string tariffMoment,
        DateTimeOffset at) => new()
    {
        Code = code,
        Name = code,
        MinimumMinutes = minimumMinutes,
        PeriodMinutes = periodMinutes,
        RemainderRounding = remainderRounding,
        TariffMoment = tariffMoment,
        LeaveRule = RoomChargePolicyValues.IncludeLeave,
        EffectiveFrom = at.AddYears(-1),
        EffectiveTo = at.AddYears(1),
        IsActive = true
    };

    private static MstTariff RoomTariff(
        string code,
        Guid serviceUnitId,
        Guid patientClassId,
        decimal price,
        DateTime? effectiveStart = null,
        DateTime? effectiveEnd = null) => new()
    {
        TariffCode = code,
        TariffName = code,
        TariffCategoryId = Guid.NewGuid(),
        ServiceUnitId = serviceUnitId,
        PatientClassId = patientClassId,
        IsRoomCharge = true,
        NormalPrice = price,
        EffectiveStartDate = effectiveStart,
        EffectiveEndDate = effectiveEnd,
        IsActive = true
    };

    private sealed class FixedCoverageAdapter(BillingCoverageDecision decision) : IBillingCoverageAdapter
    {
        public Task<BillingCoverageDecision> ResolveAsync(BillingCoverageContext context, CancellationToken cancellationToken) =>
            Task.FromResult(decision);
    }

    // BE-BKC-022: menurunkan alokasi per komponen dari komponen yang benar-benar diterima adapter.
    // Fixture tidak dapat menebak InvoiceItemId yang baru dibuat SeedInvoiceAsync, sementara
    // BIL-VAL-028 menuntut jumlah alokasi menyamai total tanggungan. PrimaryAmount pada decision
    // tetap nilai yang DIDEKLARASIKAN - bukan hasil alokasi - supaya penjaga cap masih dapat diuji
    // dengan total yang sengaja melebihi biaya coverable.
    private sealed class AllocatingCoverageAdapter(
        string primaryStatus,
        string excessStatus,
        decimal primaryTotal,
        decimal excessTotal,
        decimal unresolvedTotal) : IBillingCoverageAdapter
    {
        public Task<BillingCoverageDecision> ResolveAsync(BillingCoverageContext context, CancellationToken cancellationToken)
        {
            var outcomes = new List<BillingCoverageComponentOutcome>();
            var remaining = primaryTotal;
            foreach (var component in context.Components.Where(x => x.Coverable && x.Amount > 0))
            {
                var covered = Math.Min(remaining, component.Amount);
                remaining -= covered;
                outcomes.Add(new BillingCoverageComponentOutcome(
                    component.ComponentId, component.ComponentType, covered, 0, 0, 0));
            }

            return Task.FromResult(new BillingCoverageDecision(
                "INSURER-CONTRACT-TEST", primaryStatus, excessStatus,
                primaryTotal, excessTotal, unresolvedTotal, [], outcomes, 0, 0, []));
        }
    }

    // BE-BKC-022: sengaja menghasilkan decision yang cacat - total tanggungan menyatakan satu angka,
    // sementara alokasi per komponennya menjumlah ke angka lain. Inilah bentuk bug yang BIL-VAL-028
    // ada untuk menangkapnya, dan satu-satunya cara mengujinya adalah memalsukannya di sini.
    private sealed class MisallocatingCoverageAdapter(decimal primaryTotal, decimal allocatedTotal) : IBillingCoverageAdapter
    {
        public Task<BillingCoverageDecision> ResolveAsync(BillingCoverageContext context, CancellationToken cancellationToken)
        {
            var component = context.Components.First(x => x.Coverable && x.Amount > 0);
            var outcomes = new List<BillingCoverageComponentOutcome>
            {
                new(component.ComponentId, component.ComponentType, allocatedTotal, 0, 0, 0)
            };

            return Task.FromResult(new BillingCoverageDecision(
                "INSURER-CONTRACT-TEST", "APPROVED", "NOT_CONFIGURED",
                primaryTotal, 0, 0, [], outcomes, 0, 0, []));
        }
    }

    private sealed class SelfPayCoverageAdapter : IBillingCoverageAdapter
    {
        public static readonly SelfPayCoverageAdapter Instance = new();
        public Task<BillingCoverageDecision> ResolveAsync(BillingCoverageContext context, CancellationToken cancellationToken) =>
            Task.FromResult(new BillingCoverageDecision("SELF-PAY-TEST", "SELF_PAY", "NOT_APPLICABLE", 0, 0, 0, [], [], 0, 0, []));
    }
}
