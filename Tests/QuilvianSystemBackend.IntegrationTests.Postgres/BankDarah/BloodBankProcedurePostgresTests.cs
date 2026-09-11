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
    /// Bukti PostgreSQL untuk <c>BE-BD-012</c> — hal yang tidak dapat dibuktikan provider InMemory.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    /// <item><b>Penomoran serentak</b>: sepuluh tindakan dicatat bersamaan, sepuluh nomor berbeda.</item>
    /// <item><b>Index unik fisik <c>ProcedureNumber</c></b> hasil migration.</item>
    /// <item>
    /// <b>Concurrency token <c>ProcedureStatus</c></b> pada <c>UPDATE</c> sungguhan: dua penyelesaian
    /// serentak menghasilkan tepat satu baris riwayat.
    /// </item>
    /// <item><b>FK <c>Restrict</c> ke <c>MstTariff</c></b>: tindakan tidak dapat menunjuk tarif fiktif.</item>
    /// </list>
    /// <para>
    /// <b>Prasyarat yang dibaca, tidak ditulis:</b> satu <c>MstDoctor</c>, satu <c>MstPatientClass</c>,
    /// dan satu <c>MstTariffCategory</c> yang sudah ada. Yang ditulis dan dihapus kembali: satu
    /// <c>MstProcedure</c> dan satu <c>MstTariff</c> uji ber-GUID, beserta seluruh baris Bank Darah.
    /// Pencacah deret <c>BBK_PROCEDURE</c> dan <c>BBK_BLOOD_ORDER</c> tidak dikembalikan
    /// (<c>INV-PLT-001</c>).
    /// </para>
    /// </remarks>
    public sealed class BloodBankProcedurePostgresTests : IClassFixture<BillingTestDatabaseFixture>, IAsyncLifetime
    {
        private static readonly Guid PetugasUnit = Guid.Parse("77777777-0000-0000-0000-0000000bd121");
        private static readonly Guid PetugasBdrsA = Guid.Parse("77777777-0000-0000-0000-0000000bd12a");
        private static readonly Guid PetugasBdrsB = Guid.Parse("77777777-0000-0000-0000-0000000bd12b");

        private readonly BillingTestDatabaseFixture _fixture;
        private readonly List<EncounterSeed> _seeds = new();
        private readonly List<Guid> _componentIds = new();
        private readonly List<Guid> _procedureIds = new();
        private readonly List<Guid> _tariffIds = new();

        public BloodBankProcedurePostgresTests(BillingTestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        [Fact]
        public async Task SepuluhTindakanSerentak_SepuluhNomorBerbeda()
        {
            var dunia = await SemaiAsync();

            var hasil = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
            {
                await using var context = _fixture.CreateContext();

                return await CreateService(context).CreateAsync(Permintaan(dunia), PetugasBdrsA);
            })));

            Assert.All(hasil, x => Assert.Equal(BloodBankProcedureOutcome.Success, x.Outcome));
            Assert.Equal(10, hasil.Select(x => x.Entity!.ProcedureNumber).Distinct().Count());

            await using var baca = _fixture.CreateContext();
            Assert.Equal(10, await baca.BbkBloodBankProcedures.CountAsync(x => x.BloodOrderId == dunia.OrderId));
        }

        [Fact]
        public async Task NomorTindakan_DijagaIndexUnikFisikDiDatabase()
        {
            var dunia = await SemaiAsync();

            BbkBloodBankProcedure asli;

            await using (var context = _fixture.CreateContext())
                asli = (await CreateService(context).CreateAsync(Permintaan(dunia), PetugasBdrsA)).Entity!;

            await using var penyusup = _fixture.CreateContext();

            penyusup.BbkBloodBankProcedures.Add(Mentah(asli, asli.ProcedureNumber, asli.TariffId));

            var galat = await Assert.ThrowsAsync<DbUpdateException>(() => penyusup.SaveChangesAsync());
            var postgres = Assert.IsType<PostgresException>(galat.InnerException);

            Assert.Equal(PostgresErrorCodes.UniqueViolation, postgres.SqlState);
            Assert.Equal("IX_BbkBloodBankProcedure_ProcedureNumber", postgres.ConstraintName);
        }

        [Fact]
        public async Task PenyelesaianBersamaanDuaPetugas_HanyaSatuYangTercatat()
        {
            var dunia = await SemaiAsync();

            Guid id;

            await using (var context = _fixture.CreateContext())
                id = (await CreateService(context).CreateAsync(Permintaan(dunia), PetugasBdrsA)).Entity!.Id;

            await using var konteksA = _fixture.CreateContext();
            await using var konteksB = _fixture.CreateContext();

            await konteksA.BbkBloodBankProcedures.SingleAsync(x => x.Id == id);
            await konteksB.BbkBloodBankProcedures.SingleAsync(x => x.Id == id);

            var pertama = await CreateService(konteksA).CompleteAsync(id, PetugasBdrsA);
            var kedua = await CreateService(konteksB).CompleteAsync(id, PetugasBdrsB);

            Assert.Equal(BloodBankProcedureOutcome.Success, pertama.Outcome);
            Assert.Equal(BloodBankProcedureOutcome.VersionConflict, kedua.Outcome);

            await using var baca = _fixture.CreateContext();

            Assert.Equal(1, await baca.BbkTransitionHistories.CountAsync(x => x.EntityId == id && x.Action == "Complete"));

            var tersimpan = await baca.BbkBloodBankProcedures.SingleAsync(x => x.Id == id);
            Assert.Equal(BbkProcedureStatus.Completed, tersimpan.ProcedureStatus);
            Assert.Equal(PetugasBdrsA, tersimpan.UpdateBy);
        }

        [Fact]
        public async Task TindakanDenganTarifFiktif_DitolakForeignKeyDatabase()
        {
            var dunia = await SemaiAsync();

            BbkBloodBankProcedure asli;

            await using (var context = _fixture.CreateContext())
                asli = (await CreateService(context).CreateAsync(Permintaan(dunia), PetugasBdrsA)).Entity!;

            await using var penyusup = _fixture.CreateContext();

            penyusup.BbkBloodBankProcedures.Add(Mentah(asli, $"UJI-FK-{Guid.NewGuid():N}"[..24], tariffId: Guid.NewGuid()));

            var galat = await Assert.ThrowsAsync<DbUpdateException>(() => penyusup.SaveChangesAsync());
            var postgres = Assert.IsType<PostgresException>(galat.InnerException);

            Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, postgres.SqlState);
        }

        // =================================================================
        // Penolong
        // =================================================================

        private sealed record Dunia(EncounterSeed Seed, Guid DoctorId, Guid OrderId, Guid ProcedureId);

        private async Task<Dunia> SemaiAsync()
        {
            var seed = await _fixture.SeedEncounterAsync();
            _seeds.Add(seed);

            await using var context = _fixture.CreateContext();

            var doctorId = await context.MstDoctors.Where(x => !x.IsDelete).OrderBy(x => x.Id).Select(x => (Guid?)x.Id).FirstOrDefaultAsync();
            var classId = await context.MstPatientClasses.Where(x => !x.IsDelete).OrderBy(x => x.Id).Select(x => (Guid?)x.Id).FirstOrDefaultAsync();
            var categoryId = await context.MstTariffCategories.Where(x => !x.IsDelete).OrderBy(x => x.Id).Select(x => (Guid?)x.Id).FirstOrDefaultAsync();

            Assert.True(doctorId.HasValue, "Prasyarat uji: minimal satu MstDoctor yang tidak dihapus.");
            Assert.True(classId.HasValue, "Prasyarat uji: minimal satu MstPatientClass yang tidak dihapus.");
            Assert.True(categoryId.HasValue, "Prasyarat uji: minimal satu MstTariffCategory yang tidak dihapus.");

            // Unit dan kunjungan dibuat fixture khusus untuk test ini, sehingga keduanya boleh disetel.
            await context.MstServiceUnits
                .Where(x => x.Id == seed.ServiceUnitId)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsAvailableForBloodOrder, true));

            await context.TrxPatientEncounters
                .Where(x => x.Id == seed.EncounterId)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.PatientClassId, classId));

            var suffix = Guid.NewGuid().ToString("N")[..10];

            var component = new MstBloodComponent { Id = Guid.NewGuid(), ComponentCode = $"UT{suffix}", ComponentName = $"Komponen Uji {suffix}", IsActive = true };
            var procedure = new MstProcedure { Id = Guid.NewGuid(), ProcedureCode = $"UJI-BDR-{suffix}", ProcedureName = $"Tindakan Uji {suffix}", IsActive = true };
            var tariff = new MstTariff
            {
                Id = Guid.NewGuid(),
                TariffCode = $"UJI-TRF-{suffix}",
                TariffName = $"Tarif Uji {suffix}",
                TariffCategoryId = categoryId!.Value,
                ProcedureId = procedure.Id,
                NormalPrice = 150_000m,
                IsActive = true
            };

            context.MstBloodComponents.Add(component);
            context.MstProcedures.Add(procedure);
            context.MstTariffs.Add(tariff);
            await context.SaveChangesAsync();

            _componentIds.Add(component.Id);
            _procedureIds.Add(procedure.Id);
            _tariffIds.Add(tariff.Id);

            Guid orderId;

            await using (var orderContext = _fixture.CreateContext())
            {
                var hasil = await new BbkBloodOrderService(
                        orderContext,
                        new NumberSeriesAllocator(new FixtureContextFactory(_fixture)),
                        new BbkEncounterStatusReader(orderContext))
                    .CreateAsync(
                        new CreateBloodOrderRequest
                        {
                            PatientId = seed.PatientId,
                            EncounterId = seed.EncounterId,
                            ServiceUnitId = seed.ServiceUnitId,
                            RequestingDoctorId = doctorId!.Value,
                            Lines = new List<BloodOrderLineRequest> { new() { BloodComponentId = component.Id, RequestedQuantity = 1 } }
                        },
                        PetugasUnit);

                Assert.True(hasil.Outcome == BloodOrderOutcome.Success, hasil.Message);
                orderId = hasil.Entity!.Id;
            }

            return new Dunia(seed, doctorId.Value, orderId, procedure.Id);
        }

        private static CreateBloodBankProcedureRequest Permintaan(Dunia dunia) => new()
        {
            BloodOrderId = dunia.OrderId,
            ProcedureRefId = dunia.ProcedureId,
            BdrsDoctorId = dunia.DoctorId
        };

        private static BbkBloodBankProcedure Mentah(BbkBloodBankProcedure asli, string nomor, Guid tariffId) => new()
        {
            Id = Guid.NewGuid(),
            ProcedureNumber = nomor,
            BloodOrderId = asli.BloodOrderId,
            ServiceUnitId = asli.ServiceUnitId,
            BdrsDoctorId = asli.BdrsDoctorId,
            PerformedByUserId = asli.PerformedByUserId,
            PatientClassId = asli.PatientClassId,
            ProcedureRefId = asli.ProcedureRefId,
            TariffId = tariffId,
            ProcedureCodeSnapshot = asli.ProcedureCodeSnapshot,
            ProcedureNameSnapshot = asli.ProcedureNameSnapshot,
            TariffAmountSnapshot = asli.TariffAmountSnapshot,
            CreateBy = PetugasBdrsA
        };

        private BbkBloodBankProcedureService CreateService(ApplicationDbContext context)
            => new(context, new NumberSeriesAllocator(new FixtureContextFactory(_fixture)));

        /// <summary>
        /// Menghapus seluruh baris yang dibuat test ini, dari anak ke induk mengikuti
        /// <c>DeleteBehavior.Restrict</c>, lalu mengembalikan kelas kunjungan dan menyerahkan
        /// prasyarat encounter ke fixture.
        /// </summary>
        public async Task DisposeAsync()
        {
            if (_seeds.Count == 0)
                return;

            await using (var context = _fixture.CreateContext())
            {
                foreach (var seed in _seeds)
                {
                    var orderIds = await context.BbkBloodOrders
                        .Where(x => x.EncounterId == seed.EncounterId)
                        .Select(x => x.Id)
                        .ToListAsync();

                    var procedureIds = await context.BbkBloodBankProcedures
                        .Where(x => orderIds.Contains(x.BloodOrderId))
                        .Select(x => x.Id)
                        .ToListAsync();

                    await context.BbkTransitionHistories
                        .Where(x =>
                            (x.Scope == BbkTransitionScopes.BloodBankProcedure && procedureIds.Contains(x.EntityId)) ||
                            (x.Scope == BbkTransitionScopes.BloodOrder && orderIds.Contains(x.EntityId)))
                        .ExecuteDeleteAsync();

                    await context.BbkBloodBankProcedures
                        .Where(x => orderIds.Contains(x.BloodOrderId))
                        .ExecuteDeleteAsync();

                    await context.BbkBloodOrderLines
                        .Where(x => orderIds.Contains(x.BloodOrderId))
                        .ExecuteDeleteAsync();

                    await context.BbkBloodOrders
                        .Where(x => orderIds.Contains(x.Id))
                        .ExecuteDeleteAsync();

                    await context.TrxPatientEncounters
                        .Where(x => x.Id == seed.EncounterId)
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.PatientClassId, (Guid?)null));
                }

                await context.MstTariffs.Where(x => _tariffIds.Contains(x.Id)).ExecuteDeleteAsync();
                await context.MstProcedures.Where(x => _procedureIds.Contains(x.Id)).ExecuteDeleteAsync();
                await context.MstBloodComponents.Where(x => _componentIds.Contains(x.Id)).ExecuteDeleteAsync();
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
