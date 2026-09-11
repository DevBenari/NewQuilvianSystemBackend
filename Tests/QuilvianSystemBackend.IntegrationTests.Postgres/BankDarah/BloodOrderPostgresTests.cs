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
    /// Bukti PostgreSQL untuk <c>BE-BD-003</c> — hal yang <b>mustahil</b> dibuktikan provider
    /// InMemory. Blueprint <c>BD-BP-001</c>, kontrak <c>v4</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Empat hal yang dibuktikan di sini, dan kenapa hanya di sini.</b>
    /// </para>
    /// <list type="number">
    /// <item>
    /// <b>Deteksi order ganda tahan berebut.</b> Sepuluh permintaan order yang sama persis dikirim
    /// serentak. Tanpa <c>pg_advisory_xact_lock</c> per pasien + kunjungan, kesepuluhnya dapat lolos
    /// deteksi ganda bersamaan. InMemory tidak punya kunci itu sama sekali.
    /// </item>
    /// <item><b>Index unik fisik <c>OrderNumber</c></b> hasil migration.</item>
    /// <item><b>FK <c>Restrict</c></b> ke <c>MstPatient</c> hasil migration.</item>
    /// <item>
    /// <b>Concurrency token <c>Version</c></b> pada <c>UPDATE</c> sungguhan: dua petugas yang
    /// membatalkan order yang sama bersamaan menghasilkan tepat satu baris riwayat.
    /// </item>
    /// </list>
    /// <para>
    /// <b>Prasyarat yang dibaca, tidak ditulis.</b> <c>MstDoctor</c> menuntut profil tenaga kerja
    /// dan tiga data induk HR lewat FK sungguhan. Membuat dokter berarti menulis ke tabel milik
    /// modul HR, sehingga uji ini hanya <b>membaca</b> satu dokter yang sudah ada dan tidak pernah
    /// mengubahnya. Seluruh baris lain dibuat dengan GUID baru dan dihapus kembali pada
    /// <see cref="DisposeAsync"/>.
    /// </para>
    /// <para>
    /// <b>Yang sengaja tertinggal.</b> Pencacah deret <c>BBK_BLOOD_ORDER</c> pada
    /// <c>NumNumberSeries</c> naik beberapa angka dan <b>tidak</b> dikembalikan — pencacah nomor
    /// tidak boleh mundur (<c>INV-PLT-001</c>). Deret itu berlubang, dan lubang memang sah.
    /// </para>
    /// </remarks>
    public sealed class BloodOrderPostgresTests : IClassFixture<BillingTestDatabaseFixture>, IAsyncLifetime
    {
        private static readonly Guid PetugasUnit = Guid.Parse("77777777-0000-0000-0000-0000000bd003");
        private static readonly Guid PetugasBdrsA = Guid.Parse("77777777-0000-0000-0000-0000000bd00a");
        private static readonly Guid PetugasBdrsB = Guid.Parse("77777777-0000-0000-0000-0000000bd00b");

        private readonly BillingTestDatabaseFixture _fixture;
        private readonly List<EncounterSeed> _seeds = new();
        private readonly List<Guid> _componentIds = new();
        private readonly List<Guid> _reasonIds = new();

        public BloodOrderPostgresTests(BillingTestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        // =================================================================
        // 1. Deteksi ganda tahan berebut — BD-XINV-01, AC-BD-001
        // =================================================================

        /// <summary>
        /// Sepuluh petugas menyimpan order PRC yang sama untuk pasien dan kunjungan yang sama pada
        /// saat bersamaan. Tepat <b>satu</b> order lahir; sembilan sisanya tertahan
        /// <c>VAL-BD-001</c> — persis seperti bila mereka menyimpannya bergantian.
        /// </summary>
        [Fact]
        public async Task SepuluhOrderSerentak_PasienKunjunganKomponenSama_HanyaSatuYangLahir()
        {
            var dunia = await SemaiAsync();

            var hasil = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
            {
                await using var context = _fixture.CreateContext();

                return await CreateService(context).CreateAsync(Permintaan(dunia), PetugasUnit);
            })));

            Assert.Equal(1, hasil.Count(x => x.Outcome == BloodOrderOutcome.Success));
            Assert.Equal(9, hasil.Count(x => x.Outcome == BloodOrderOutcome.DuplicateOrder));

            await using var baca = _fixture.CreateContext();

            Assert.Equal(1, await baca.BbkBloodOrders.CountAsync(x => x.EncounterId == dunia.Seed.EncounterId));
        }

        // =================================================================
        // 2. Penjaga fisik hasil migration
        // =================================================================

        /// <summary>
        /// <c>QBE-CODE-004</c> — nomor bisnis unik dijaga index unik di database, bukan hanya oleh
        /// alokatornya. Baris yang menyusup dengan nomor yang sudah terpakai ditolak server.
        /// </summary>
        [Fact]
        public async Task NomorOrder_DijagaIndexUnikFisikDiDatabase()
        {
            var dunia = await SemaiAsync();

            string nomor;

            await using (var context = _fixture.CreateContext())
                nomor = (await CreateService(context).CreateAsync(Permintaan(dunia), PetugasUnit)).Entity!.OrderNumber;

            await using var penyusup = _fixture.CreateContext();

            penyusup.BbkBloodOrders.Add(OrderMentah(dunia, nomor, dunia.Seed.PatientId));

            var galat = await Assert.ThrowsAsync<DbUpdateException>(() => penyusup.SaveChangesAsync());
            var postgres = Assert.IsType<PostgresException>(galat.InnerException);

            Assert.Equal(PostgresErrorCodes.UniqueViolation, postgres.SqlState);
            Assert.Equal("IX_BbkBloodOrder_OrderNumber", postgres.ConstraintName);
        }

        /// <summary>
        /// Order tidak dapat menunjuk pasien yang tidak ada: FK <c>Restrict</c> ke
        /// <c>MstPatient</c> hidup di database, bukan hanya di pemeriksaan service.
        /// </summary>
        [Fact]
        public async Task OrderTanpaPasienSah_DitolakForeignKeyDatabase()
        {
            var dunia = await SemaiAsync();

            await using var context = _fixture.CreateContext();

            context.BbkBloodOrders.Add(OrderMentah(
                dunia,
                "UJI-FK-" + Guid.NewGuid().ToString("N")[..10],
                patientId: Guid.NewGuid()));

            var galat = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            var postgres = Assert.IsType<PostgresException>(galat.InnerException);

            Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, postgres.SqlState);
        }

        // =================================================================
        // 3. Concurrency token pada UPDATE sungguhan
        // =================================================================

        /// <summary>
        /// Dua petugas membuka order yang sama, lalu keduanya membatalkan. Yang pertama berhasil;
        /// yang kedua ditolak <c>VersionConflict</c> karena <c>UPDATE ... WHERE "Version" = @lama</c>
        /// tidak menemukan baris. Hasilnya tepat satu baris riwayat pembatalan (<c>INV-BD-035</c>).
        /// </summary>
        [Fact]
        public async Task PembatalanBersamaanDuaPetugas_HanyaSatuYangTercatat()
        {
            var dunia = await SemaiAsync();

            Guid orderId;

            await using (var context = _fixture.CreateContext())
                orderId = (await CreateService(context).CreateAsync(Permintaan(dunia), PetugasUnit)).Entity!.Id;

            await using var konteksA = _fixture.CreateContext();
            await using var konteksB = _fixture.CreateContext();

            // Keduanya membaca order sebelum siapa pun menyimpan.
            await konteksA.BbkBloodOrders.Include(x => x.Lines).SingleAsync(x => x.Id == orderId);
            await konteksB.BbkBloodOrders.Include(x => x.Lines).SingleAsync(x => x.Id == orderId);

            var pertama = await CreateService(konteksA).CancelAsync(
                orderId, new CancelBloodOrderRequest { ReasonCode = dunia.ReasonCode }, PetugasBdrsA);
            var kedua = await CreateService(konteksB).CancelAsync(
                orderId, new CancelBloodOrderRequest { ReasonCode = dunia.ReasonCode }, PetugasBdrsB);

            Assert.Equal(BloodOrderOutcome.Success, pertama.Outcome);
            Assert.Equal(BloodOrderOutcome.VersionConflict, kedua.Outcome);

            await using var baca = _fixture.CreateContext();

            Assert.Equal(1, await baca.BbkTransitionHistories
                .CountAsync(x => x.EntityId == orderId && x.Action == "Cancel"));
            Assert.Equal(
                BbkBloodOrderStatus.Cancelled,
                (await baca.BbkBloodOrders.SingleAsync(x => x.Id == orderId)).OrderStatus);
        }

        // =================================================================
        // Penolong
        // =================================================================

        private sealed record Dunia(EncounterSeed Seed, Guid DoctorId, Guid ComponentId, string ReasonCode);

        private async Task<Dunia> SemaiAsync()
        {
            var seed = await _fixture.SeedEncounterAsync();
            _seeds.Add(seed);

            await using var context = _fixture.CreateContext();

            // Unit ini dibuat fixture khusus untuk test ini, sehingga kewenangannya boleh disetel.
            await context.MstServiceUnits
                .Where(x => x.Id == seed.ServiceUnitId)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsAvailableForBloodOrder, true));

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
                ComponentCode = $"UJ{suffix}",
                ComponentName = $"Komponen Uji {suffix}",
                IsActive = true
            };

            var reason = new MstBloodBankReason
            {
                Id = Guid.NewGuid(),
                ReasonCode = $"UJI-BTL-{suffix}".ToUpperInvariant(),
                ReasonText = "Pembatalan operasional uji integrasi",
                ReasonCategory = BloodBankReasonCategories.OrderCancellationOperational,
                IsActive = true
            };

            context.MstBloodComponents.Add(component);
            context.MstBloodBankReasons.Add(reason);
            await context.SaveChangesAsync();

            _componentIds.Add(component.Id);
            _reasonIds.Add(reason.Id);

            return new Dunia(seed, doctorId!.Value, component.Id, reason.ReasonCode);
        }

        private static CreateBloodOrderRequest Permintaan(Dunia dunia) => new()
        {
            PatientId = dunia.Seed.PatientId,
            EncounterId = dunia.Seed.EncounterId,
            ServiceUnitId = dunia.Seed.ServiceUnitId,
            RequestingDoctorId = dunia.DoctorId,
            Lines = new List<BloodOrderLineRequest>
            {
                new() { BloodComponentId = dunia.ComponentId, RequestedQuantity = 2 }
            }
        };

        private static BbkBloodOrder OrderMentah(Dunia dunia, string nomor, Guid patientId) => new()
        {
            Id = Guid.NewGuid(),
            OrderNumber = nomor,
            PatientId = patientId,
            EncounterId = dunia.Seed.EncounterId,
            ServiceUnitId = dunia.Seed.ServiceUnitId,
            RequestingDoctorId = dunia.DoctorId,
            InputByUserId = PetugasUnit,
            CreateBy = PetugasUnit
        };

        private BbkBloodOrderService CreateService(ApplicationDbContext context)
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
            if (_seeds.Count == 0 && _componentIds.Count == 0 && _reasonIds.Count == 0)
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

                    await context.BbkTransitionHistories
                        .Where(x => x.Scope == BbkTransitionScopes.BloodOrder && orderIds.Contains(x.EntityId))
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

                await context.MstBloodBankReasons
                    .Where(x => _reasonIds.Contains(x.Id))
                    .ExecuteDeleteAsync();
            }

            foreach (var seed in _seeds)
                await _fixture.CleanupEncounterAsync(seed);
        }

        /// <summary>
        /// Factory yang memberi alokator konteks dengan koneksi tersendiri, sama seperti
        /// pendaftaran <c>AddDbContextFactory</c> pada aplikasi.
        /// </summary>
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
