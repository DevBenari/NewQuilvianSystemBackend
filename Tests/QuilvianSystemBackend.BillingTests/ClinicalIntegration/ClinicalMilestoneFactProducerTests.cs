using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalBillingIntegration.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalBillingIntegration.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalBillingIntegration.Services;
using QuilvianSystemBackend.BillingTests.Infrastructure;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.ClinicalIntegration
{
    /// <summary>
    /// Acceptance criteria RJ-BIL-BE-002:
    ///   1. Clinical endpoint tidak menetapkan Paid.
    ///   2. Retry tidak menggandakan charge.
    ///   3. Correction memakai version baru.
    ///
    /// Ditambah kebijakan author atas pembatalan klinis:
    ///   CASE A — batal sebelum charge terbentuk: tidak ada charge sama sekali.
    ///   CASE B — batal setelah charge terbentuk: charge asli tidak dihapus.
    ///   CASE C — hasil sebelumnya tidak diketahui: wajib rekonsiliasi, dilarang koreksi buta.
    /// </summary>
    public sealed class ClinicalMilestoneFactProducerTests
        : IClassFixture<BillingTestDatabaseFixture>, IAsyncLifetime
    {
        private readonly BillingTestDatabaseFixture _fixture;
        private readonly List<EncounterSeed> _seeds = new();

        public ClinicalMilestoneFactProducerTests(BillingTestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            foreach (var seed in _seeds)
                await _fixture.CleanupEncounterAsync(seed);
        }

        private async Task<EncounterSeed> NewEncounterAsync()
        {
            var seed = await _fixture.SeedEncounterAsync();
            _seeds.Add(seed);
            return seed;
        }

        private ClinicalMilestoneFactProducer CreateProducer(ApplicationDbContext context) =>
            new(context, new BillingFolioService(context), BillingTestDatabaseFixture.CreateLoggerService());

        private static ClinicalMilestoneFactRequest BuildRequest(
            Guid encounterId,
            Guid prescriptionId,
            DateTime occurredAt) =>
            new()
            {
                SourceContext = BillingSourceContract.PrescriptionSourceContext,
                SourceAggregateId = prescriptionId,
                EffectType = BillingSourceContract.PrescriptionChargeEffectType,
                EncounterId = encounterId,
                OccurredAt = occurredAt,
                Quantity = 2m,
                Unit = "ITEM",
                TariffSnapshot = "{\"source\":\"ClinicalSnapshot\",\"totalPrice\":150000}"
            };

        // ---------------------------------------------------------------------
        // Skenario 1 — Fakta klinis pertama menghasilkan tepat satu efek finansial
        // ---------------------------------------------------------------------

        [Fact]
        public async Task FaktaKlinisPertama_MenghasilkanTepatSatuChargeLine()
        {
            var seed = await NewEncounterAsync();
            var prescriptionId = Guid.NewGuid();

            await using var context = _fixture.CreateContext();
            var producer = CreateProducer(context);

            var hasil = await producer.EmitChargeEligibilityAsync(
                BuildRequest(seed.EncounterId, prescriptionId, DateTime.UtcNow),
                seed.ActorUserId);

            Assert.Equal(ClinicalFactEmissionKind.Emitted, hasil.Kind);
            Assert.Equal(ClinicalFactDispatchStatus.Dispatched, hasil.DispatchStatus);
            Assert.Equal(1, hasil.MilestoneFactVersion);
            Assert.NotNull(hasil.BillingChargeLineId);

            await using var verify = _fixture.CreateContext();

            var charges = await verify.BilChargeLines
                .Where(x => x.SourceAggregateId == prescriptionId)
                .ToListAsync();

            Assert.Single(charges);
            Assert.Equal(BillingSourceContract.PrescriptionSourceContext, charges[0].SourceContext);

            // Invariant: Billing belum punya formula tarif yang disahkan, sehingga charge masuk
            // tinjauan finansial. Yang penting, angkanya bukan berasal dari modul klinis.
            Assert.Equal(BillingChargeCalculationStatus.PendingFinancialReview, charges[0].CalculationStatus);

            var facts = await verify.TrxClinicalMilestoneFacts
                .Where(x => x.SourceAggregateId == prescriptionId)
                .ToListAsync();

            Assert.Single(facts);
            Assert.Equal(ClinicalMilestoneKind.ChargeEligibility, facts[0].MilestoneKind);
        }

        // ---------------------------------------------------------------------
        // Skenario 2 — Retry tidak menggandakan charge
        // ---------------------------------------------------------------------

        [Fact]
        public async Task FaktaSamaDikirimUlang_TidakMenggandakanCharge()
        {
            var seed = await NewEncounterAsync();
            var prescriptionId = Guid.NewGuid();
            var occurredAt = DateTime.UtcNow;

            await using var context = _fixture.CreateContext();
            var producer = CreateProducer(context);

            var pertama = await producer.EmitChargeEligibilityAsync(
                BuildRequest(seed.EncounterId, prescriptionId, occurredAt),
                seed.ActorUserId);

            var kedua = await producer.EmitChargeEligibilityAsync(
                BuildRequest(seed.EncounterId, prescriptionId, occurredAt),
                seed.ActorUserId);

            Assert.Equal(ClinicalFactEmissionKind.Emitted, pertama.Kind);
            Assert.Equal(ClinicalFactEmissionKind.Replayed, kedua.Kind);
            Assert.Equal(pertama.MilestoneFactId, kedua.MilestoneFactId);
            Assert.Equal(pertama.MilestoneFactVersion, kedua.MilestoneFactVersion);

            await using var verify = _fixture.CreateContext();

            Assert.Equal(1, await verify.BilChargeLines
                .CountAsync(x => x.SourceAggregateId == prescriptionId));

            Assert.Equal(1, await verify.TrxClinicalMilestoneFacts
                .CountAsync(x => x.SourceAggregateId == prescriptionId));

            // Kunci idempotency harus stabil, bukan dibangkitkan ulang setiap percobaan.
            // Bila stabil, mengirimkannya lagi langsung ke Billing menghasilkan replay canonical.
            var fact = await verify.TrxClinicalMilestoneFacts
                .AsNoTracking()
                .FirstAsync(x => x.SourceAggregateId == prescriptionId);

            await using var replayContext = _fixture.CreateContext();
            var billing = new BillingFolioService(replayContext);

            var replay = await billing.RecognizeMilestoneAsync(
                new RecognizeBillingMilestoneRequest
                {
                    IdempotencyKey = fact.IdempotencyKey,
                    MilestoneFactId = fact.MilestoneFactId,
                    MilestoneFactVersion = fact.MilestoneFactVersion,
                    EncounterId = fact.EncounterId,
                    SourceContext = fact.SourceContext,
                    SourceAggregateId = fact.SourceAggregateId,
                    SourceItemId = fact.SourceItemId,
                    EffectType = fact.EffectType,
                    OccurredAt = fact.OccurredAt,
                    Quantity = fact.Quantity,
                    Unit = fact.Unit,
                    TariffSnapshot = fact.TariffSnapshot,
                    CorrelationId = fact.CorrelationId
                },
                seed.ActorUserId,
                BillingFolioService.ClinicalFactConsumer);

            Assert.Equal(BillingServiceResultKind.Success, replay.Kind);
            Assert.True(replay.Value!.IsReplay);

            await using var verifyAgain = _fixture.CreateContext();
            Assert.Equal(1, await verifyAgain.BilChargeLines
                .CountAsync(x => x.SourceAggregateId == prescriptionId));
        }

        // ---------------------------------------------------------------------
        // Skenario 3 — Koreksi memakai versi baru dan tidak menimpa charge lama
        // ---------------------------------------------------------------------

        [Fact]
        public async Task RevisiFaktaBaru_MembuatVersiBaruTanpaMenghapusChargeAsli()
        {
            var seed = await NewEncounterAsync();
            var prescriptionId = Guid.NewGuid();

            await using var context = _fixture.CreateContext();
            var producer = CreateProducer(context);

            var pertama = await producer.EmitChargeEligibilityAsync(
                BuildRequest(seed.EncounterId, prescriptionId, DateTime.UtcNow.AddMinutes(-10)),
                seed.ActorUserId);

            var revisi = await producer.EmitChargeEligibilityAsync(
                BuildRequest(seed.EncounterId, prescriptionId, DateTime.UtcNow),
                seed.ActorUserId);

            Assert.Equal(ClinicalFactEmissionKind.Emitted, revisi.Kind);
            Assert.Equal(pertama.MilestoneFactId, revisi.MilestoneFactId);
            Assert.Equal(2, revisi.MilestoneFactVersion);

            await using var verify = _fixture.CreateContext();

            // Charge asli tetap satu baris, versinya tidak diubah, dan tidak dihapus.
            var charges = await verify.BilChargeLines
                .Where(x => x.SourceAggregateId == prescriptionId)
                .ToListAsync();

            Assert.Single(charges);
            Assert.Equal(1, charges[0].MilestoneFactVersion);
            Assert.False(charges[0].IsDelete);

            // Folio diarahkan ke tinjauan agar Billing yang memutuskan koreksinya.
            var folio = await verify.BilFolios.FirstAsync(x => x.EncounterId == seed.EncounterId);
            Assert.Equal(BillingFolioStatus.ReviewRequired, folio.Status);

            // Dua revisi menghasilkan dua jejak pemrosesan, bukan satu yang ditimpa.
            Assert.Equal(2, await verify.BilProcessingEffects
                .CountAsync(x => x.MilestoneFactId == pertama.MilestoneFactId));

            Assert.Equal(2, await verify.TrxClinicalMilestoneFacts
                .CountAsync(x => x.SourceAggregateId == prescriptionId));
        }

        // ---------------------------------------------------------------------
        // CASE A — Pembatalan sebelum charge terbentuk
        // ---------------------------------------------------------------------

        [Fact]
        public async Task PembatalanSebelumChargeTerbentuk_TidakMembuatChargeApaPun()
        {
            var seed = await NewEncounterAsync();
            var prescriptionId = Guid.NewGuid();

            await using var context = _fixture.CreateContext();
            var producer = CreateProducer(context);

            var hasil = await producer.EmitClinicalCancellationAsync(
                BuildRequest(seed.EncounterId, prescriptionId, DateTime.UtcNow),
                seed.ActorUserId);

            Assert.Equal(ClinicalFactEmissionKind.SuppressedNoPriorCharge, hasil.Kind);
            Assert.True(hasil.IsClinicallySafe);

            await using var verify = _fixture.CreateContext();

            Assert.Equal(0, await verify.BilFolios.CountAsync(x => x.EncounterId == seed.EncounterId));
            Assert.Equal(0, await verify.BilChargeLines.CountAsync(x => x.SourceAggregateId == prescriptionId));
            Assert.Equal(0, await verify.BilProcessingEffects.CountAsync(x => x.MilestoneFactId == hasil.MilestoneFactId));

            // Histori klinis tetap tercatat walaupun tidak ada akibat finansial.
            var fact = await verify.TrxClinicalMilestoneFacts
                .SingleAsync(x => x.SourceAggregateId == prescriptionId);

            Assert.Equal(ClinicalMilestoneKind.ClinicalCancellation, fact.MilestoneKind);
            Assert.Equal(ClinicalFactDispatchStatus.SuppressedNoPriorCharge, fact.DispatchStatus);
        }

        // ---------------------------------------------------------------------
        // CASE B — Pembatalan setelah charge terbentuk
        // ---------------------------------------------------------------------

        [Fact]
        public async Task PembatalanSetelahChargeTerbentuk_TidakMenghapusChargeAsli()
        {
            var seed = await NewEncounterAsync();
            var prescriptionId = Guid.NewGuid();

            await using var context = _fixture.CreateContext();
            var producer = CreateProducer(context);

            var eligibility = await producer.EmitChargeEligibilityAsync(
                BuildRequest(seed.EncounterId, prescriptionId, DateTime.UtcNow.AddMinutes(-5)),
                seed.ActorUserId);

            var chargeLineId = eligibility.BillingChargeLineId;
            Assert.NotNull(chargeLineId);

            var pembatalan = await producer.EmitClinicalCancellationAsync(
                BuildRequest(seed.EncounterId, prescriptionId, DateTime.UtcNow),
                seed.ActorUserId);

            Assert.Equal(ClinicalFactEmissionKind.Emitted, pembatalan.Kind);
            Assert.Equal(eligibility.MilestoneFactId, pembatalan.MilestoneFactId);
            Assert.Equal(2, pembatalan.MilestoneFactVersion);

            await using var verify = _fixture.CreateContext();

            // Charge asli masih ada, dengan Id yang sama dan tidak ditandai terhapus.
            var charge = await verify.BilChargeLines.SingleAsync(x => x.Id == chargeLineId!.Value);
            Assert.False(charge.IsDelete);
            Assert.Equal(1, charge.MilestoneFactVersion);

            // Fakta pembatalan tercatat sebagai revisi baru atas identitas yang sama.
            var faktaPembatalan = await verify.TrxClinicalMilestoneFacts
                .SingleAsync(x => x.SourceAggregateId == prescriptionId && x.MilestoneFactVersion == 2);

            Assert.Equal(ClinicalMilestoneKind.ClinicalCancellation, faktaPembatalan.MilestoneKind);
            Assert.Equal(ClinicalFactDispatchStatus.Dispatched, faktaPembatalan.DispatchStatus);

            // Billing menerima revisi baru dan mengarahkan folio ke tinjauan, bukan menghapus.
            var folio = await verify.BilFolios.FirstAsync(x => x.EncounterId == seed.EncounterId);
            Assert.Equal(BillingFolioStatus.ReviewRequired, folio.Status);
        }

        // ---------------------------------------------------------------------
        // CASE C — Hasil sebelumnya tidak diketahui
        // ---------------------------------------------------------------------

        [Fact]
        public async Task RevisiSetelahOutcomeUnknown_MemintaRekonsiliasiDanTidakMengoreksiButa()
        {
            var seed = await NewEncounterAsync();
            var prescriptionId = Guid.NewGuid();

            await using var context = _fixture.CreateContext();
            var producer = CreateProducer(context);

            await producer.EmitChargeEligibilityAsync(
                BuildRequest(seed.EncounterId, prescriptionId, DateTime.UtcNow.AddMinutes(-5)),
                seed.ActorUserId);

            // Meniru keadaan nyata: pengiriman terjadi tetapi hasilnya tidak terkonfirmasi.
            await using (var arrange = _fixture.CreateContext())
            {
                var fact = await arrange.TrxClinicalMilestoneFacts
                    .SingleAsync(x => x.SourceAggregateId == prescriptionId);

                fact.DispatchStatus = ClinicalFactDispatchStatus.OutcomeUnknown;
                await arrange.SaveChangesAsync();
            }

            await using var retryContext = _fixture.CreateContext();
            var retryProducer = CreateProducer(retryContext);

            var hasil = await retryProducer.EmitClinicalCancellationAsync(
                BuildRequest(seed.EncounterId, prescriptionId, DateTime.UtcNow),
                seed.ActorUserId);

            Assert.Equal(ClinicalFactEmissionKind.ReconciliationRequired, hasil.Kind);
            Assert.Equal("CLIN_FACT_RECONCILIATION_REQUIRED", hasil.Code);

            await using var verify = _fixture.CreateContext();

            // Tidak ada revisi baru dan tidak ada koreksi finansial yang diterbitkan.
            Assert.Equal(1, await verify.TrxClinicalMilestoneFacts
                .CountAsync(x => x.SourceAggregateId == prescriptionId));

            Assert.Equal(1, await verify.BilChargeLines
                .CountAsync(x => x.SourceAggregateId == prescriptionId));
        }

        // ---------------------------------------------------------------------
        // Batas kontrak sumber
        // ---------------------------------------------------------------------

        [Theory]
        [InlineData("Pharmacy")]
        [InlineData("Laboratory")]
        [InlineData("Radiology")]
        public async Task KonteksYangBelumDikontrak_DitolakProducer(string sourceContext)
        {
            var seed = await NewEncounterAsync();
            var aggregateId = Guid.NewGuid();

            await using var context = _fixture.CreateContext();
            var producer = CreateProducer(context);

            var request = BuildRequest(seed.EncounterId, aggregateId, DateTime.UtcNow);
            request.SourceContext = sourceContext;

            var hasil = await producer.EmitChargeEligibilityAsync(request, seed.ActorUserId);

            Assert.Equal(ClinicalFactEmissionKind.Invalid, hasil.Kind);
            Assert.Equal("CLIN_FACT_SOURCE_INVALID", hasil.Code);

            await using var verify = _fixture.CreateContext();
            Assert.Equal(0, await verify.TrxClinicalMilestoneFacts.CountAsync(x => x.SourceAggregateId == aggregateId));
            Assert.Equal(0, await verify.BilChargeLines.CountAsync(x => x.SourceAggregateId == aggregateId));
        }

        [Fact]
        public async Task EffectTypeMilikKontekLain_DitolakProducer()
        {
            var seed = await NewEncounterAsync();
            var aggregateId = Guid.NewGuid();

            await using var context = _fixture.CreateContext();
            var producer = CreateProducer(context);

            // Konteks Prescription memakai effect type milik Procedure.
            var request = BuildRequest(seed.EncounterId, aggregateId, DateTime.UtcNow);
            request.EffectType = BillingSourceContract.ProcedureChargeEffectType;

            var hasil = await producer.EmitChargeEligibilityAsync(request, seed.ActorUserId);

            Assert.Equal(ClinicalFactEmissionKind.Invalid, hasil.Kind);
            Assert.Equal("CLIN_FACT_SOURCE_INVALID", hasil.Code);

            await using var verify = _fixture.CreateContext();
            Assert.Equal(0, await verify.BilChargeLines.CountAsync(x => x.SourceAggregateId == aggregateId));
        }

        // ---------------------------------------------------------------------
        // Batas transaksi
        // ---------------------------------------------------------------------

        [Fact]
        public async Task PenerbitanDidalamTransaksiKlinis_DitolakDenganPesanJelas()
        {
            var seed = await NewEncounterAsync();
            var prescriptionId = Guid.NewGuid();

            await using var context = _fixture.CreateContext();
            var producer = CreateProducer(context);

            await using var transaction = await context.Database.BeginTransactionAsync();

            var kesalahan = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                producer.EmitChargeEligibilityAsync(
                    BuildRequest(seed.EncounterId, prescriptionId, DateTime.UtcNow),
                    seed.ActorUserId));

            Assert.Contains("transaksi klinis", kesalahan.Message);

            await transaction.RollbackAsync();
        }
    }
}
