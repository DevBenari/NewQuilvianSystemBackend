using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.Infrastructure;

namespace QuilvianSystemBackend.Tests.HealthServices.MasterData
{
    /// <summary>
    /// Bukti regresi untuk bug laporan pengguna: <c>POST insurance-coverage-rules</c>
    /// mengembalikan 500 (duplicate key pada <c>IX_MstInsuranceCoverageRule_RuleCode</c>) begitu
    /// ada rule yang pernah di-soft-delete.
    ///
    /// Akar masalah: <c>GenerateRuleCodeAsync</c> sebelumnya menghitung nomor urut RuleCode hanya
    /// dari baris yang belum di-soft-delete, padahal unique index pada RuleCode tidak difilter
    /// IsDelete - berlaku ke seluruh baris di database. Rule yang sudah dihapus tetap menempati
    /// nomornya selamanya, sehingga generator memilih ulang nomor yang sama dan insert berikutnya
    /// selalu gagal. Dipakai SQLite (bukan EF InMemory) karena EF InMemory provider tidak
    /// menegakkan unique index, sehingga tidak akan mereproduksi bug yang sebenarnya terjadi di
    /// PostgreSQL produksi.
    /// </summary>
    public class InsuranceCoverageRuleControllerTests
    {
        [Fact]
        public async Task CreateSetelahRuleLamaDihapusTidakBentrokRuleCode()
        {
            using var testDb = TestDatabase.Create();
            await using var context = testDb.CreateContext();

            var pembeda = Guid.NewGuid().ToString("N")[..8];

            var kategoriTarif = new MstTariffCategory
            {
                TariffCategoryCode = $"CAT-{pembeda}",
                TariffCategoryName = "Konsultasi",
                IsConsultationFee = true
            };
            context.Set<MstTariffCategory>().Add(kategoriTarif);
            await context.SaveChangesAsync();

            var tarif = new MstTariff
            {
                TariffCode = $"TRF-{pembeda}",
                TariffName = "Konsul Rajal Dokter Umum",
                TariffCategoryId = kategoriTarif.Id,
                NormalPrice = 150_000
            };
            context.Set<MstTariff>().Add(tarif);

            var provider = new MstInsuranceProvider
            {
                InsuranceProviderCode = $"INS-{pembeda}",
                InsuranceProviderName = "Allianz Indonesia"
            };
            context.Set<MstInsuranceProvider>().Add(provider);
            await context.SaveChangesAsync();

            var actorId = Guid.NewGuid();
            var controller = new InsuranceCoverageRuleController(context, ControllerTestHarness.BuatLoggerService(actorId))
                .DenganPengguna(actorId);

            var request = new CreateInsuranceCoverageRuleRequest
            {
                InsuranceProviderId = provider.Id,
                TariffId = tarif.Id,
                ItemType = "Tariff",
                RuleName = "Konsul Rajal Dokter Umum",
                CoverageStatus = "Covered",
                CoveragePercent = 100
            };

            var hasilPertama = await controller.CreateInsuranceCoverageRule(request);
            var responsPertama = Assert.IsType<ApiResponse<InsuranceCoverageRuleCreateResponse>>(
                Assert.IsType<OkObjectResult>(hasilPertama).Value);
            Assert.True(responsPertama.Success);
            var ruleCodePertama = responsPertama.Data!.RuleCode;
            var ruleIdPertama = responsPertama.Data!.Id;

            // Soft-delete: RuleCode pertama tetap menempati unique index di database walau
            // barisnya sudah ditandai terhapus - persis kondisi yang memicu bug di produksi.
            var hasilHapus = await controller.DeleteInsuranceCoverageRule(ruleIdPertama, deleteRequest: null);
            Assert.IsType<OkObjectResult>(hasilHapus);

            // RuleName yang sama boleh dipakai lagi karena pengecekan duplikat nama mengecualikan
            // baris yang sudah di-soft-delete.
            var hasilKedua = await controller.CreateInsuranceCoverageRule(request);
            var responsKedua = Assert.IsType<ApiResponse<InsuranceCoverageRuleCreateResponse>>(
                Assert.IsType<OkObjectResult>(hasilKedua).Value);

            Assert.True(responsKedua.Success);
            Assert.NotEqual(ruleCodePertama, responsKedua.Data!.RuleCode);
        }
    }
}
