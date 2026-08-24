using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services;
using QuilvianSystemBackend.BillingTests.Infrastructure;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.Operational
{
    /// <summary>
    /// Acceptance criteria RJ-BIL-BE-001:
    ///   1. Folio unik per encounter.
    ///   2. Duplicate key menghasilkan replay.
    ///   3. Stale version ditolak.
    ///
    /// Ketiganya adalah invariant persistence dan concurrency, sehingga diuji di level
    /// service terhadap PostgreSQL sungguhan. Provider InMemory tidak dipakai karena tidak
    /// menegakkan unique index — test akan lulus secara semu.
    /// </summary>
    public sealed class BillingFolioServiceTests : IClassFixture<BillingTestDatabaseFixture>, IAsyncLifetime
    {
        private readonly BillingTestDatabaseFixture _fixture;
        private readonly List<EncounterSeed> _seeds = new();

        public BillingFolioServiceTests(BillingTestDatabaseFixture fixture)
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

        private static RecognizeBillingMilestoneRequest BuildRequest(
            Guid encounterId,
            string idempotencyKey,
            Guid milestoneFactId,
            int milestoneFactVersion = 1,
            Guid? sourceAggregateId = null,
            Guid? sourceItemId = null)
        {
            return new RecognizeBillingMilestoneRequest
            {
                IdempotencyKey = idempotencyKey,
                MilestoneFactId = milestoneFactId,
                MilestoneFactVersion = milestoneFactVersion,
                EncounterId = encounterId,
                SourceContext = BillingFolioService.InternalTestSourceContext,
                SourceAggregateId = sourceAggregateId ?? Guid.NewGuid(),
                SourceItemId = sourceItemId ?? Guid.NewGuid(),
                EffectType = BillingFolioService.InternalTestEffectType,
                OccurredAt = DateTime.UtcNow,
                Quantity = 1m,
                Unit = "EA"
            };
        }

        // ---------------------------------------------------------------------
        // Acceptance criteria 1 — Folio unik per encounter
        // ---------------------------------------------------------------------

        [Fact]
        public async Task DuaMilestoneBerbedaPadaEncounterSama_HanyaMenghasilkanSatuFolio()
        {
            var seed = await NewEncounterAsync();

            await using var context = _fixture.CreateContext();
            var service = new BillingFolioService(context);

            var pertama = await service.RecognizeMilestoneAsync(
                BuildRequest(seed.EncounterId, $"key-a-{Guid.NewGuid():N}", Guid.NewGuid()),
                seed.ActorUserId);

            var kedua = await service.RecognizeMilestoneAsync(
                BuildRequest(seed.EncounterId, $"key-b-{Guid.NewGuid():N}", Guid.NewGuid()),
                seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Success, pertama.Kind);
            Assert.Equal(BillingServiceResultKind.Success, kedua.Kind);

            await using var verify = _fixture.CreateContext();
            var jumlahFolio = await verify.BilFolios
                .CountAsync(x => x.EncounterId == seed.EncounterId && !x.IsDelete);

            Assert.Equal(1, jumlahFolio);
            Assert.Equal(pertama.Value!.FolioId, kedua.Value!.FolioId);

            // Dua milestone berbeda tetap menghasilkan dua charge line pada folio yang sama.
            var jumlahCharge = await verify.BilChargeLines
                .CountAsync(x => x.FolioId == pertama.Value!.FolioId!.Value);

            Assert.Equal(2, jumlahCharge);
        }

        [Fact]
        public async Task FolioKeduaUntukEncounterSama_DitolakUniqueIndexDatabase()
        {
            var seed = await NewEncounterAsync();

            await using var context = _fixture.CreateContext();
            var service = new BillingFolioService(context);

            var hasil = await service.RecognizeMilestoneAsync(
                BuildRequest(seed.EncounterId, $"key-{Guid.NewGuid():N}", Guid.NewGuid()),
                seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Success, hasil.Kind);

            // Menyisipkan folio kanonik kedua secara langsung harus ditolak database,
            // bukan hanya dicegah oleh logika aplikasi.
            await using var direct = _fixture.CreateContext();
            direct.BilFolios.Add(new BilFolio
            {
                Id = Guid.NewGuid(),
                EncounterId = seed.EncounterId,
                Status = BillingFolioStatus.Open,
                Version = 1,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = seed.ActorUserId,
                IsDelete = false,
                IsCancel = false
            });

            var exception = await Assert.ThrowsAsync<DbUpdateException>(
                () => direct.SaveChangesAsync());

            var postgres = Assert.IsType<PostgresException>(exception.InnerException);
            Assert.Equal(PostgresErrorCodes.UniqueViolation, postgres.SqlState);
            Assert.Equal("IX_BilFolio_EncounterId", postgres.ConstraintName);
        }

        // ---------------------------------------------------------------------
        // Acceptance criteria 2 — Duplicate key menghasilkan replay
        // ---------------------------------------------------------------------

        [Fact]
        public async Task IdempotencyKeySamaDenganPayloadSama_MenghasilkanReplayTanpaChargeGanda()
        {
            var seed = await NewEncounterAsync();
            var idempotencyKey = $"key-replay-{Guid.NewGuid():N}";
            var milestoneFactId = Guid.NewGuid();
            var sourceAggregateId = Guid.NewGuid();
            var sourceItemId = Guid.NewGuid();

            await using var context = _fixture.CreateContext();
            var service = new BillingFolioService(context);

            var request = BuildRequest(
                seed.EncounterId,
                idempotencyKey,
                milestoneFactId,
                sourceAggregateId: sourceAggregateId,
                sourceItemId: sourceItemId);

            var pertama = await service.RecognizeMilestoneAsync(request, seed.ActorUserId);

            // Permintaan kedua identik, meniru retry infrastruktur.
            var ulangan = BuildRequest(
                seed.EncounterId,
                idempotencyKey,
                milestoneFactId,
                sourceAggregateId: sourceAggregateId,
                sourceItemId: sourceItemId);
            ulangan.OccurredAt = request.OccurredAt;

            var kedua = await service.RecognizeMilestoneAsync(ulangan, seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Success, pertama.Kind);
            Assert.Equal(BillingServiceResultKind.Success, kedua.Kind);

            Assert.False(pertama.Value!.IsReplay);
            Assert.True(kedua.Value!.IsReplay);

            // Replay mengembalikan efek kanonik yang sama, bukan efek baru.
            Assert.Equal(pertama.Value!.ProcessingEffectId, kedua.Value!.ProcessingEffectId);
            Assert.Equal(pertama.Value!.ChargeLineId, kedua.Value!.ChargeLineId);

            await using var verify = _fixture.CreateContext();

            var jumlahEffect = await verify.BilProcessingEffects
                .CountAsync(x => x.IdempotencyKey == idempotencyKey);
            Assert.Equal(1, jumlahEffect);

            var jumlahCharge = await verify.BilChargeLines
                .CountAsync(x => x.MilestoneFactId == milestoneFactId);
            Assert.Equal(1, jumlahCharge);
        }

        // ---------------------------------------------------------------------
        // Acceptance criteria 3 — Stale version ditolak
        // ---------------------------------------------------------------------

        [Fact]
        public async Task VersiLamaSetelahVersiBaruApplied_DitolakDenganVersionConflict()
        {
            var seed = await NewEncounterAsync();
            var milestoneFactId = Guid.NewGuid();
            var sourceAggregateId = Guid.NewGuid();
            var sourceItemId = Guid.NewGuid();

            await using var context = _fixture.CreateContext();
            var service = new BillingFolioService(context);

            // Versi 2 diproses lebih dulu.
            var versiBaru = await service.RecognizeMilestoneAsync(
                BuildRequest(
                    seed.EncounterId,
                    $"key-v2-{Guid.NewGuid():N}",
                    milestoneFactId,
                    milestoneFactVersion: 2,
                    sourceAggregateId: sourceAggregateId,
                    sourceItemId: sourceItemId),
                seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Success, versiBaru.Kind);

            // Versi 1 datang terlambat dengan idempotency key berbeda.
            var versiLama = await service.RecognizeMilestoneAsync(
                BuildRequest(
                    seed.EncounterId,
                    $"key-v1-{Guid.NewGuid():N}",
                    milestoneFactId,
                    milestoneFactVersion: 1,
                    sourceAggregateId: sourceAggregateId,
                    sourceItemId: sourceItemId),
                seed.ActorUserId);

            Assert.Equal(BillingServiceResultKind.Conflict, versiLama.Kind);
            Assert.Equal("BIL_VERSION_CONFLICT", versiLama.ErrorCode);
            Assert.Equal(2, versiLama.AppliedVersion);

            await using var verify = _fixture.CreateContext();

            // Histori versi 2 tetap utuh dan tidak tertimpa versi lama.
            var effects = await verify.BilProcessingEffects
                .Where(x => x.MilestoneFactId == milestoneFactId)
                .ToListAsync();

            Assert.Single(effects);
            Assert.Equal(2, effects[0].MilestoneFactVersion);

            var charges = await verify.BilChargeLines
                .Where(x => x.MilestoneFactId == milestoneFactId)
                .ToListAsync();

            Assert.Single(charges);
            Assert.Equal(2, charges[0].MilestoneFactVersion);
        }
    }
}
