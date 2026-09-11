using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.BillingTests.Infrastructure;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.BankDarah
{
    /// <summary>
    /// Bukti PostgreSQL untuk <c>BE-BD-004</c> — hal yang <b>mustahil</b> dibuktikan provider
    /// InMemory. Blueprint <c>BD-BP-001</c>, kontrak <c>v4</c>.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    /// <item>
    /// <b><c>BD-XINV-02</c> tahan berebut.</b> Sepuluh permintaan untuk order yang sama dikirim
    /// serentak; tepat satu lahir.
    /// </item>
    /// <item>
    /// <b><c>BD-XINV-03</c> tahan berebut.</b> Dua penerimaan hampir bersamaan pada permintaan
    /// 3 kantong, masing-masing 2 kantong: keduanya dicatat — penerimaan tidak pernah ditolak
    /// karena kelebihan — dan tepat satu kantong ditandai berlebih. Sisa tidak pernah negatif.
    /// </item>
    /// <item><b>Index unik fisik <c>PmiBagNumber</c></b> hasil migration.</item>
    /// <item>
    /// <b>Index unik terfilter permintaan berjalan</b> hasil migration: permintaan berjalan kedua
    /// ditolak database, permintaan yang sudah dibatalkan tidak ikut terhitung.
    /// </item>
    /// <item><b>FK <c>Restrict</c></b> kantong ke permintaan asalnya.</item>
    /// </list>
    /// <para>
    /// <b>Yang sengaja tertinggal.</b> Pencacah deret <c>BBK_BLOOD_ORDER</c> dan
    /// <c>BBK_PROVIDER_REQUEST</c> pada <c>NumNumberSeries</c> naik dan <b>tidak</b> dikembalikan —
    /// pencacah nomor tidak boleh mundur (<c>INV-PLT-001</c>). Seluruh baris lain dihapus kembali
    /// pada <see cref="DisposeAsync"/>.
    /// </para>
    /// </remarks>
    public sealed class ProviderRequestPostgresTests : IClassFixture<BillingTestDatabaseFixture>, IAsyncLifetime
    {
        private static readonly Guid PetugasUnit = Guid.Parse("77777777-0000-0000-0000-0000000bd041");
        private static readonly Guid PetugasBdrs = Guid.Parse("77777777-0000-0000-0000-0000000bd042");

        private readonly BillingTestDatabaseFixture _fixture;
        private readonly List<EncounterSeed> _seeds = new();
        private readonly List<Guid> _componentIds = new();

        public ProviderRequestPostgresTests(BillingTestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        // =================================================================
        // 1. BD-XINV-02 — satu order, satu permintaan berjalan
        // =================================================================

        [Fact]
        public async Task SepuluhPermintaanSerentak_OrderSama_HanyaSatuYangLahir()
        {
            var dunia = await SemaiAsync();
            var orderId = await BuatOrderAsync(dunia, jumlah: 3);

            var hasil = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
            {
                await using var context = _fixture.CreateContext();

                return await CreateService(context).CreateAsync(
                    new CreateProviderRequestRequest { BloodOrderId = orderId },
                    PetugasBdrs);
            })));

            Assert.Equal(1, hasil.Count(x => x.Outcome == ProviderRequestOutcome.Success));
            Assert.Equal(9, hasil.Count(x => x.Outcome == ProviderRequestOutcome.DuplicateRequest));

            await using var baca = _fixture.CreateContext();

            Assert.Equal(1, await baca.BbkProviderRequests.CountAsync(x => x.BloodOrderId == orderId));
        }

        // =================================================================
        // 2. BD-XINV-03 — sisa tidak pernah negatif di bawah penerimaan serentak
        // =================================================================

        [Fact]
        public async Task DuaPenerimaanSerentak_KeduanyaDicatat_TepatSatuBerlebih_SisaTidakNegatif()
        {
            var dunia = await SemaiAsync();
            var requestId = await BuatPermintaanAsync(dunia, jumlah: 3);
            var suffix = Guid.NewGuid().ToString("N")[..8];

            var hasil = await Task.WhenAll(new[] { "A", "B" }.Select(kelompok => Task.Run(async () =>
            {
                await using var context = _fixture.CreateContext();

                return await CreateService(context).RecordReceiptAsync(
                    requestId,
                    new RecordReceiptRequest
                    {
                        Units = Enumerable.Range(1, 2)
                            .Select(i => new ReceivedBloodUnitRequest
                            {
                                PmiBagNumber = $"UJI-{suffix}-{kelompok}{i}",
                                BloodComponentId = dunia.ComponentId
                            })
                            .ToList()
                    },
                    PetugasBdrs);
            })));

            Assert.All(hasil, x => Assert.Equal(ProviderRequestOutcome.Success, x.Outcome));
            Assert.Equal(1, hasil.Sum(x => x.ExcessUnitCount));

            await using var baca = _fixture.CreateContext();

            var kantong = await baca.BbkBloodUnits.Where(x => x.ProviderRequestId == requestId).ToListAsync();
            Assert.Equal(4, kantong.Count);
            Assert.Equal(1, kantong.Count(x => x.IsExcess));

            var permintaan = await baca.BbkProviderRequests.SingleAsync(x => x.Id == requestId);
            Assert.Equal(BbkProviderRequestStatus.Fulfilled, permintaan.RequestStatus);
            Assert.Equal(2, permintaan.Version);

            Assert.Equal(
                new[] { 1, 2 },
                await baca.BbkBloodUnitReceipts
                    .Where(x => x.ProviderRequestId == requestId)
                    .OrderBy(x => x.Sequence)
                    .Select(x => x.Sequence)
                    .ToListAsync());
        }

        // =================================================================
        // 3. Penjaga fisik hasil migration
        // =================================================================

        [Fact]
        public async Task NomorKantongPmi_DijagaIndexUnikFisikDiDatabase()
        {
            var dunia = await SemaiAsync();
            var requestId = await BuatPermintaanAsync(dunia, jumlah: 2);
            var nomor = $"UJI-{Guid.NewGuid():N}"[..20];

            await using (var context = _fixture.CreateContext())
            {
                var hasil = await CreateService(context).RecordReceiptAsync(
                    requestId,
                    new RecordReceiptRequest
                    {
                        Units = new List<ReceivedBloodUnitRequest> { new() { PmiBagNumber = nomor, BloodComponentId = dunia.ComponentId } }
                    },
                    PetugasBdrs);

                Assert.Equal(ProviderRequestOutcome.Success, hasil.Outcome);
            }

            await using var penyusup = _fixture.CreateContext();

            var asli = await penyusup.BbkBloodUnits.AsNoTracking().SingleAsync(x => x.PmiBagNumber == nomor);

            penyusup.BbkBloodUnits.Add(new BbkBloodUnit
            {
                Id = Guid.NewGuid(),
                PmiBagNumber = nomor,
                ProviderRequestId = asli.ProviderRequestId,
                ReceiptId = asli.ReceiptId,
                BloodComponentId = asli.BloodComponentId,
                CreateBy = PetugasBdrs
            });

            var galat = await Assert.ThrowsAsync<DbUpdateException>(() => penyusup.SaveChangesAsync());
            var postgres = Assert.IsType<PostgresException>(galat.InnerException);

            Assert.Equal(PostgresErrorCodes.UniqueViolation, postgres.SqlState);
            Assert.Equal("IX_BbkBloodUnit_PmiBagNumber", postgres.ConstraintName);
        }

        [Fact]
        public async Task PermintaanBerjalanKedua_DitolakIndexUnikTerfilter_PermintaanBatalTidakTerhitung()
        {
            var dunia = await SemaiAsync();
            var orderId = await BuatOrderAsync(dunia, jumlah: 2);
            var requestId = await BuatPermintaanUntukOrderAsync(orderId);

            await using (var penyusup = _fixture.CreateContext())
            {
                penyusup.BbkProviderRequests.Add(PermintaanMentah(orderId, dunia, BbkProviderRequestStatus.Requested));

                var galat = await Assert.ThrowsAsync<DbUpdateException>(() => penyusup.SaveChangesAsync());
                var postgres = Assert.IsType<PostgresException>(galat.InnerException);

                Assert.Equal(PostgresErrorCodes.UniqueViolation, postgres.SqlState);
                Assert.Equal("IX_BbkProviderRequest_BloodOrderId_Active", postgres.ConstraintName);
            }

            await using var sah = _fixture.CreateContext();

            // Permintaan yang sudah dibatalkan tidak ikut terhitung oleh index terfilter.
            sah.BbkProviderRequests.Add(PermintaanMentah(orderId, dunia, BbkProviderRequestStatus.Cancelled));
            await sah.SaveChangesAsync();

            Assert.Equal(2, await sah.BbkProviderRequests.CountAsync(x => x.BloodOrderId == orderId));
            Assert.True(requestId != Guid.Empty);
        }

        [Fact]
        public async Task KantongTanpaPermintaanSah_DitolakForeignKeyDatabase()
        {
            var dunia = await SemaiAsync();

            await using var context = _fixture.CreateContext();

            context.BbkBloodUnits.Add(new BbkBloodUnit
            {
                Id = Guid.NewGuid(),
                PmiBagNumber = $"UJI-FK-{Guid.NewGuid():N}"[..24],
                ProviderRequestId = Guid.NewGuid(),
                ReceiptId = Guid.NewGuid(),
                BloodComponentId = dunia.ComponentId,
                CreateBy = PetugasBdrs
            });

            var galat = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            var postgres = Assert.IsType<PostgresException>(galat.InnerException);

            Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, postgres.SqlState);
        }

        // =================================================================
        // Penolong
        // =================================================================

        private sealed record Dunia(EncounterSeed Seed, Guid DoctorId, Guid ComponentId);

        private async Task<Dunia> SemaiAsync()
        {
            var seed = await _fixture.SeedEncounterAsync();
            _seeds.Add(seed);

            await using var context = _fixture.CreateContext();

            // Unit ini dibuat fixture khusus untuk test ini, sehingga kewenangannya boleh disetel.
            await context.MstServiceUnits
                .Where(x => x.Id == seed.ServiceUnitId)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsAvailableForBloodOrder, true));

            // Prasyarat dibaca, tidak ditulis: membuat dokter berarti menulis ke tabel milik HR.
            var doctorId = await context.MstDoctors
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.Id)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync();

            Assert.True(
                doctorId.HasValue,
                "Prasyarat uji: database target wajib memuat minimal satu MstDoctor yang tidak dihapus.");

            var suffix = Guid.NewGuid().ToString("N")[..10];

            var component = new MstBloodComponent
            {
                Id = Guid.NewGuid(),
                ComponentCode = $"UP{suffix}",
                ComponentName = $"Komponen Uji PMI {suffix}",
                IsActive = true
            };

            context.MstBloodComponents.Add(component);
            await context.SaveChangesAsync();

            _componentIds.Add(component.Id);

            return new Dunia(seed, doctorId!.Value, component.Id);
        }

        private async Task<Guid> BuatOrderAsync(Dunia dunia, int jumlah)
        {
            await using var context = _fixture.CreateContext();

            var hasil = await new BbkBloodOrderService(
                    context,
                    new NumberSeriesAllocator(new FixtureContextFactory(_fixture)),
                    new BbkEncounterStatusReader(context))
                .CreateAsync(
                    new CreateBloodOrderRequest
                    {
                        PatientId = dunia.Seed.PatientId,
                        EncounterId = dunia.Seed.EncounterId,
                        ServiceUnitId = dunia.Seed.ServiceUnitId,
                        RequestingDoctorId = dunia.DoctorId,
                        Lines = new List<BloodOrderLineRequest>
                        {
                            new() { BloodComponentId = dunia.ComponentId, RequestedQuantity = jumlah }
                        }
                    },
                    PetugasUnit);

            Assert.True(hasil.Outcome == BloodOrderOutcome.Success, hasil.Message);

            return hasil.Entity!.Id;
        }

        private async Task<Guid> BuatPermintaanAsync(Dunia dunia, int jumlah)
            => await BuatPermintaanUntukOrderAsync(await BuatOrderAsync(dunia, jumlah));

        private async Task<Guid> BuatPermintaanUntukOrderAsync(Guid orderId)
        {
            await using var context = _fixture.CreateContext();

            var hasil = await CreateService(context).CreateAsync(
                new CreateProviderRequestRequest { BloodOrderId = orderId },
                PetugasBdrs);

            Assert.True(hasil.Outcome == ProviderRequestOutcome.Success, hasil.Message);

            return hasil.Entity!.Id;
        }

        private static BbkProviderRequest PermintaanMentah(Guid orderId, Dunia dunia, BbkProviderRequestStatus status) => new()
        {
            Id = Guid.NewGuid(),
            RequestNumber = $"UJI-{Guid.NewGuid():N}"[..24],
            BloodOrderId = orderId,
            PatientId = dunia.Seed.PatientId,
            RequestStatus = status,
            CreateBy = PetugasBdrs
        };

        private BbkProviderRequestService CreateService(ApplicationDbContext context)
            => new(
                context,
                new NumberSeriesAllocator(new FixtureContextFactory(_fixture)),
                new BbkEncounterStatusReader(context));

        /// <summary>
        /// Menghapus seluruh baris yang dibuat test ini, dari anak ke induk mengikuti
        /// <c>DeleteBehavior.Restrict</c>, lalu menyerahkan prasyarat encounter ke fixture.
        /// </summary>
        public async Task DisposeAsync()
        {
            if (_seeds.Count == 0 && _componentIds.Count == 0)
                return;

            await using (var context = _fixture.CreateContext())
            {
                foreach (var seed in _seeds)
                {
                    var orderIds = await context.BbkBloodOrders
                        .Where(x => x.EncounterId == seed.EncounterId)
                        .Select(x => x.Id)
                        .ToListAsync();

                    if (orderIds.Count == 0)
                        continue;

                    var requestIds = await context.BbkProviderRequests
                        .Where(x => orderIds.Contains(x.BloodOrderId))
                        .Select(x => x.Id)
                        .ToListAsync();

                    var unitIds = await context.BbkBloodUnits
                        .Where(x => requestIds.Contains(x.ProviderRequestId))
                        .Select(x => x.Id)
                        .ToListAsync();

                    await context.BbkTransitionHistories
                        .Where(x =>
                            (x.Scope == BbkTransitionScopes.BloodUnit && unitIds.Contains(x.EntityId)) ||
                            (x.Scope == BbkTransitionScopes.ProviderRequest && requestIds.Contains(x.EntityId)) ||
                            (x.Scope == BbkTransitionScopes.BloodOrder && orderIds.Contains(x.EntityId)))
                        .ExecuteDeleteAsync();

                    await context.BbkBloodUnits
                        .Where(x => requestIds.Contains(x.ProviderRequestId))
                        .ExecuteDeleteAsync();

                    await context.BbkBloodUnitReceipts
                        .Where(x => requestIds.Contains(x.ProviderRequestId))
                        .ExecuteDeleteAsync();

                    await context.BbkProviderRequests
                        .Where(x => orderIds.Contains(x.BloodOrderId))
                        .ExecuteDeleteAsync();

                    await context.BbkBloodOrderLines
                        .Where(x => orderIds.Contains(x.BloodOrderId))
                        .ExecuteDeleteAsync();

                    await context.BbkBloodOrders
                        .Where(x => orderIds.Contains(x.Id))
                        .ExecuteDeleteAsync();
                }

                await context.MstBloodComponents
                    .Where(x => _componentIds.Contains(x.Id))
                    .ExecuteDeleteAsync();
            }

            foreach (var seed in _seeds)
                await _fixture.CleanupEncounterAsync(seed);
        }

        private sealed class FixtureContextFactory : IDbContextFactory<ApplicationDbContext>
        {
            private readonly BillingTestDatabaseFixture _fixture;

            public FixtureContextFactory(BillingTestDatabaseFixture fixture)
            {
                _fixture = fixture;
            }

            public ApplicationDbContext CreateDbContext() => _fixture.CreateContext();
        }
    }
}
