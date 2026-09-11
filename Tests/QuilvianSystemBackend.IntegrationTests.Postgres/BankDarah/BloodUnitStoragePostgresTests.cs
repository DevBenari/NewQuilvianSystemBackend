using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.BillingTests.Infrastructure;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.BankDarah
{
    /// <summary>
    /// Bukti PostgreSQL untuk <c>BE-BD-015</c> — hal yang tidak dapat dibuktikan provider InMemory.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    /// <item>
    /// <b>Dua petugas memindahkan kantong yang sama ke dua lokasi berbeda hampir bersamaan</b>
    /// (matriks acceptance §7): tepat satu berhasil, yang kalah menerima <c>409</c>, dan kantong tidak
    /// pernah punya dua penempatan berlaku.
    /// </item>
    /// <item>Sepuluh perpindahan serentak tanpa token layar — riwayat tetap satu rantai yang utuh.</item>
    /// <item>Dua penetapan pertama serentak — tepat satu penempatan lahir.</item>
    /// <item><b>Index unik terfilter fisik</b> <c>IX_BbkBloodUnitPlacement_CurrentUnit</c> hasil migration.</item>
    /// <item><b>FK <c>Restrict</c></b> penempatan ke master lokasi.</item>
    /// <item>
    /// Penerjemahan SQL nyata atas pencacahan <c>VAL-BD-068</c> dan gerbang alokasi terhadap lokasi
    /// yang dinonaktifkan lalu diaktifkan kembali.
    /// </item>
    /// </list>
    /// <para>
    /// <b>Yang ditulis lalu dihapus kembali:</b> kunjungan uji milik fixture, satu komponen, tiga lokasi
    /// penyimpanan ber-GUID, dan seluruh baris Bank Darah turunannya. Pencacah deret nomor tidak
    /// dikembalikan (<c>INV-PLT-001</c>).
    /// </para>
    /// </remarks>
    public sealed class BloodUnitStoragePostgresTests : IClassFixture<BillingTestDatabaseFixture>, IAsyncLifetime
    {
        private static readonly Guid PetugasUnit = Guid.Parse("77777777-0000-0000-0000-0000000bd151");
        private static readonly Guid PetugasBdrsA = Guid.Parse("77777777-0000-0000-0000-0000000bd15a");
        private static readonly Guid PetugasBdrsB = Guid.Parse("77777777-0000-0000-0000-0000000bd15b");

        private readonly BillingTestDatabaseFixture _fixture;
        private readonly List<EncounterSeed> _seeds = new();
        private readonly List<Guid> _componentIds = new();
        private readonly List<Guid> _locationIds = new();

        public BloodUnitStoragePostgresTests(BillingTestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        [Fact]
        public async Task DuaPetugasMemindahkanBersamaanKeLokasiBerbeda_TepatSatuBerhasil_TidakPernahDuaPenempatanBerlaku()
        {
            var dunia = await SemaiAsync(jumlahKantong: 1);
            var id = dunia.UnitIds[0];

            await SimpanAsync(id, dunia.LokasiA);

            // Kedua petugas membuka layar yang sama: token Version yang dipegang keduanya sama.
            var hasil = await Task.WhenAll(
                new[] { (Lokasi: dunia.LokasiB, Petugas: PetugasBdrsA), (Lokasi: dunia.LokasiC, Petugas: PetugasBdrsB) }
                    .Select(x => Task.Run(async () =>
                    {
                        await using var context = _fixture.CreateContext();

                        return await new BbkBloodUnitService(context).MoveStorageLocationAsync(
                            id,
                            new MoveStorageLocationRequest { StorageLocationId = x.Lokasi, Version = 1 },
                            x.Petugas);
                    })));

            Assert.Equal(1, hasil.Count(x => x.Outcome == BloodUnitOutcome.Success));
            Assert.Equal(1, hasil.Count(x => x.Outcome == BloodUnitOutcome.VersionConflict));

            await using var baca = _fixture.CreateContext();

            var penempatan = await baca.BbkBloodUnitPlacements.Where(x => x.BloodUnitId == id).ToListAsync();
            Assert.Equal(2, penempatan.Count);

            var berlaku = Assert.Single(penempatan, x => x.IsCurrent);
            var kantong = await baca.BbkBloodUnits.SingleAsync(x => x.Id == id);

            Assert.Equal(berlaku.Id, kantong.CurrentPlacementId);
            Assert.Equal(2, kantong.Version);
            Assert.Equal(BbkBloodUnitStatus.Available, kantong.UnitStatus);
            Assert.Equal(penempatan.Single(x => !x.IsCurrent).Id, berlaku.PreviousPlacementId);
        }

        [Fact]
        public async Task SepuluhPerpindahanSerentakTanpaTokenLayar_RiwayatTetapSatuRantaiUtuh()
        {
            var dunia = await SemaiAsync(jumlahKantong: 1);
            var id = dunia.UnitIds[0];

            await SimpanAsync(id, dunia.LokasiA);

            var hasil = await Task.WhenAll(Enumerable.Range(0, 10).Select(i => Task.Run(async () =>
            {
                await using var context = _fixture.CreateContext();

                return await new BbkBloodUnitService(context).MoveStorageLocationAsync(
                    id,
                    new MoveStorageLocationRequest { StorageLocationId = i % 2 == 0 ? dunia.LokasiB : dunia.LokasiC },
                    PetugasBdrsA);
            })));

            Assert.All(hasil, x => Assert.Contains(x.Outcome, new[] { BloodUnitOutcome.Success, BloodUnitOutcome.VersionConflict }));

            var berhasil = hasil.Count(x => x.Outcome == BloodUnitOutcome.Success);
            Assert.True(berhasil >= 1);

            await using var baca = _fixture.CreateContext();

            var penempatan = await baca.BbkBloodUnitPlacements.Where(x => x.BloodUnitId == id).ToListAsync();
            Assert.Equal(1 + berhasil, penempatan.Count);

            var berlaku = Assert.Single(penempatan, x => x.IsCurrent);
            Assert.Equal(berlaku.Id, (await baca.BbkBloodUnits.SingleAsync(x => x.Id == id)).CurrentPlacementId);

            // Satu rantai: tepat satu penempatan pertama, dan setiap penempatan lain ditunjuk tepat sekali.
            Assert.Single(penempatan, x => x.PreviousPlacementId == null);
            var ditunjuk = penempatan.Where(x => x.PreviousPlacementId.HasValue).Select(x => x.PreviousPlacementId!.Value).ToList();
            Assert.Equal(ditunjuk.Count, ditunjuk.Distinct().Count());
            Assert.All(ditunjuk, x => Assert.Contains(penempatan, p => p.Id == x));
        }

        [Fact]
        public async Task DuaPenetapanPertamaSerentak_TepatSatuPenempatanLahir()
        {
            var dunia = await SemaiAsync(jumlahKantong: 1);
            var id = dunia.UnitIds[0];

            var hasil = await Task.WhenAll(new[] { dunia.LokasiA, dunia.LokasiB }.Select(lokasi => Task.Run(async () =>
            {
                await using var context = _fixture.CreateContext();

                return await new BbkBloodUnitService(context).AssignStorageLocationAsync(
                    id,
                    new AssignStorageLocationRequest { StorageLocationId = lokasi, Version = 0 },
                    PetugasBdrsA);
            })));

            Assert.Equal(1, hasil.Count(x => x.Outcome == BloodUnitOutcome.Success));
            Assert.Equal(1, hasil.Count(x => x.Outcome == BloodUnitOutcome.VersionConflict));

            await using var baca = _fixture.CreateContext();

            Assert.Single(await baca.BbkBloodUnitPlacements.Where(x => x.BloodUnitId == id).ToListAsync());
            Assert.Equal(
                2,
                await baca.BbkTransitionHistories.CountAsync(x =>
                    x.Scope == BbkTransitionScopes.BloodUnit &&
                    x.EntityId == id &&
                    x.FromStatus != null));
        }

        [Fact]
        public async Task PenempatanBerlakuKedua_DitolakIndexUnikTerfilterFisik_PenempatanLamaTidakTerhitung()
        {
            var dunia = await SemaiAsync(jumlahKantong: 1);
            var id = dunia.UnitIds[0];

            await SimpanAsync(id, dunia.LokasiA);

            await using (var penyusup = _fixture.CreateContext())
            {
                penyusup.BbkBloodUnitPlacements.Add(Mentah(id, dunia.LokasiB, isCurrent: true));

                var galat = await Assert.ThrowsAsync<DbUpdateException>(() => penyusup.SaveChangesAsync());
                var postgres = Assert.IsType<PostgresException>(galat.InnerException);

                Assert.Equal(PostgresErrorCodes.UniqueViolation, postgres.SqlState);
                Assert.Equal("IX_BbkBloodUnitPlacement_CurrentUnit", postgres.ConstraintName);
            }

            // Penempatan yang tidak berlaku tidak ikut terhitung oleh index terfilter.
            await using var sah = _fixture.CreateContext();
            sah.BbkBloodUnitPlacements.Add(Mentah(id, dunia.LokasiB, isCurrent: false));
            await sah.SaveChangesAsync();

            Assert.Equal(2, await sah.BbkBloodUnitPlacements.CountAsync(x => x.BloodUnitId == id));
        }

        [Fact]
        public async Task PenempatanKeLokasiFiktif_DitolakForeignKeyDatabase()
        {
            var dunia = await SemaiAsync(jumlahKantong: 1);

            await using var context = _fixture.CreateContext();

            context.BbkBloodUnitPlacements.Add(Mentah(dunia.UnitIds[0], Guid.NewGuid(), isCurrent: true));

            var galat = await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
            var postgres = Assert.IsType<PostgresException>(galat.InnerException);

            Assert.Equal(PostgresErrorCodes.ForeignKeyViolation, postgres.SqlState);
        }

        /// <summary>
        /// <c>AC-BD-067</c>, <c>AC-BD-068</c>, <c>AC-BD-069</c>, dan <c>AC-BD-070</c> terhadap SQL nyata:
        /// pencacahan kantong tertahan, gerbang yang tertutup dan terbuka kembali, dan nol baris kantong
        /// yang disunting oleh penonaktifan.
        /// </summary>
        [Fact]
        public async Task PenonaktifanLokasi_MenyebutJumlah_MenutupGerbang_TanpaMenyuntingKantong()
        {
            var dunia = await SemaiAsync(jumlahKantong: 2);

            await SimpanAsync(dunia.UnitIds[0], dunia.LokasiA);
            await SimpanAsync(dunia.UnitIds[1], dunia.LokasiA);

            var sebelum = await PotretKantongAsync(dunia.UnitIds);

            await using (var context = _fixture.CreateContext())
            {
                var hasil = await new BloodStorageLocationService(context).UpdateStatusAsync(dunia.LokasiA, isActive: false, PetugasBdrsA);

                Assert.Equal(BloodStorageLocationStatus.Success, hasil.Status);
                Assert.Equal(2, hasil.HeldUnitCount);
                Assert.Contains("Ada 2 kantong", hasil.Message);
            }

            Assert.Equal(sebelum, await PotretKantongAsync(dunia.UnitIds));

            await using (var context = _fixture.CreateContext())
            {
                var gerbang = await new BbkBloodUnitService(context).EvaluateAllocationGateAsync(dunia.UnitIds[0]);
                Assert.Equal("VAL-BD-064", gerbang!.RuleCode);
            }

            await using (var context = _fixture.CreateContext())
            {
                var pindah = await new BbkBloodUnitService(context).MoveStorageLocationAsync(
                    dunia.UnitIds[0],
                    new MoveStorageLocationRequest { StorageLocationId = dunia.LokasiB },
                    PetugasBdrsB);

                Assert.Equal(BloodUnitOutcome.Success, pindah.Outcome);
            }

            await using (var context = _fixture.CreateContext())
            {
                var service = new BbkBloodUnitService(context);

                Assert.True((await service.EvaluateAllocationGateAsync(dunia.UnitIds[0]))!.IsOpen);
                Assert.False((await service.EvaluateAllocationGateAsync(dunia.UnitIds[1]))!.IsOpen);
            }
        }

        // =================================================================
        // Penolong
        // =================================================================

        private sealed record Dunia(EncounterSeed Seed, List<Guid> UnitIds, Guid LokasiA, Guid LokasiB, Guid LokasiC);

        private async Task SimpanAsync(Guid unitId, Guid lokasi)
        {
            await using var context = _fixture.CreateContext();

            var hasil = await new BbkBloodUnitService(context).AssignStorageLocationAsync(
                unitId,
                new AssignStorageLocationRequest { StorageLocationId = lokasi },
                PetugasBdrsA);

            Assert.True(hasil.Outcome == BloodUnitOutcome.Success, hasil.Message);
        }

        private async Task<string> PotretKantongAsync(List<Guid> unitIds)
        {
            await using var baca = _fixture.CreateContext();

            var baris = await baca.BbkBloodUnits
                .AsNoTracking()
                .Where(x => unitIds.Contains(x.Id))
                .OrderBy(x => x.Id)
                .Select(x => $"{x.Id}|{x.UnitStatus}|{x.CurrentPlacementId}|{x.Version}|{x.UpdateDateTime:O}")
                .ToListAsync();

            var penempatan = await baca.BbkBloodUnitPlacements.CountAsync(x => unitIds.Contains(x.BloodUnitId));

            return string.Join(";", baris) + "#" + penempatan;
        }

        private static BbkBloodUnitPlacement Mentah(Guid unitId, Guid lokasi, bool isCurrent) => new()
        {
            Id = Guid.NewGuid(),
            BloodUnitId = unitId,
            StorageLocationId = lokasi,
            PlacedAt = DateTime.UtcNow,
            PlacedByUserId = PetugasBdrsA,
            IsCurrent = isCurrent,
            CreateBy = PetugasBdrsA
        };

        private async Task<Dunia> SemaiAsync(int jumlahKantong)
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

            Assert.True(doctorId.HasValue, "Prasyarat uji: minimal satu MstDoctor yang tidak dihapus.");

            var suffix = Guid.NewGuid().ToString("N")[..10];

            var component = new MstBloodComponent
            {
                Id = Guid.NewGuid(),
                ComponentCode = $"US{suffix}",
                ComponentName = $"Komponen Uji Simpan {suffix}",
                IsActive = true
            };

            context.MstBloodComponents.Add(component);

            var lokasi = new[] { "A", "B", "C" }
                .Select(x => new MstBloodStorageLocation
                {
                    Id = Guid.NewGuid(),
                    StorageLocationCode = $"UJI{x}{suffix}",
                    StorageLocationName = $"Kulkas Uji {x} {suffix}",
                    IsActive = true
                })
                .ToList();

            context.MstBloodStorageLocations.AddRange(lokasi);
            await context.SaveChangesAsync();

            _componentIds.Add(component.Id);
            _locationIds.AddRange(lokasi.Select(x => x.Id));

            Guid orderId;

            await using (var orderContext = _fixture.CreateContext())
            {
                var order = await new BbkBloodOrderService(
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
                            Lines = new List<BloodOrderLineRequest>
                            {
                                new() { BloodComponentId = component.Id, RequestedQuantity = jumlahKantong }
                            }
                        },
                        PetugasUnit);

                Assert.True(order.Outcome == BloodOrderOutcome.Success, order.Message);
                orderId = order.Entity!.Id;
            }

            var nomor = Enumerable.Range(1, jumlahKantong).Select(i => $"UJS-{suffix}-{i}").ToList();

            await using (var requestContext = _fixture.CreateContext())
            {
                var service = new BbkProviderRequestService(
                    requestContext,
                    new NumberSeriesAllocator(new FixtureContextFactory(_fixture)),
                    new BbkEncounterStatusReader(requestContext));

                var permintaan = await service.CreateAsync(new CreateProviderRequestRequest { BloodOrderId = orderId }, PetugasBdrsA);
                Assert.True(permintaan.Outcome == ProviderRequestOutcome.Success, permintaan.Message);

                var penerimaan = await service.RecordReceiptAsync(
                    permintaan.Entity!.Id,
                    new RecordReceiptRequest
                    {
                        Units = nomor
                            .Select(x => new ReceivedBloodUnitRequest { PmiBagNumber = x, BloodComponentId = component.Id })
                            .ToList()
                    },
                    PetugasBdrsA);

                Assert.True(penerimaan.Outcome == ProviderRequestOutcome.Success, penerimaan.Message);
            }

            await using var baca = _fixture.CreateContext();

            var kantong = await baca.BbkBloodUnits
                .Where(x => nomor.Contains(x.PmiBagNumber))
                .Select(x => new { x.Id, x.PmiBagNumber })
                .ToListAsync();

            return new Dunia(
                seed,
                nomor.Select(n => kantong.Single(x => x.PmiBagNumber == n).Id).ToList(),
                lokasi[0].Id,
                lokasi[1].Id,
                lokasi[2].Id);
        }

        /// <summary>
        /// Menghapus seluruh baris yang dibuat test ini. Simpul melingkar kantong ⇄ penempatan dan
        /// rantai penempatan ⇄ penempatan sebelumnya dilepas lebih dulu, karena FK-nya <c>RESTRICT</c>
        /// diperiksa per baris.
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

                    var requestIds = await context.BbkProviderRequests
                        .Where(x => orderIds.Contains(x.BloodOrderId))
                        .Select(x => x.Id)
                        .ToListAsync();

                    var unitIds = await context.BbkBloodUnits
                        .Where(x => requestIds.Contains(x.ProviderRequestId))
                        .Select(x => x.Id)
                        .ToListAsync();

                    await context.BbkBloodUnits
                        .Where(x => unitIds.Contains(x.Id))
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.CurrentPlacementId, (Guid?)null));

                    await context.BbkBloodUnitPlacements
                        .Where(x => unitIds.Contains(x.BloodUnitId))
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.PreviousPlacementId, (Guid?)null));

                    await context.BbkBloodUnitPlacements
                        .Where(x => unitIds.Contains(x.BloodUnitId))
                        .ExecuteDeleteAsync();

                    await context.BbkTransitionHistories
                        .Where(x =>
                            (x.Scope == BbkTransitionScopes.BloodUnit && unitIds.Contains(x.EntityId)) ||
                            (x.Scope == BbkTransitionScopes.ProviderRequest && requestIds.Contains(x.EntityId)) ||
                            (x.Scope == BbkTransitionScopes.BloodOrder && orderIds.Contains(x.EntityId)))
                        .ExecuteDeleteAsync();

                    await context.BbkBloodUnits.Where(x => unitIds.Contains(x.Id)).ExecuteDeleteAsync();
                    await context.BbkBloodUnitReceipts.Where(x => requestIds.Contains(x.ProviderRequestId)).ExecuteDeleteAsync();
                    await context.BbkProviderRequests.Where(x => requestIds.Contains(x.Id)).ExecuteDeleteAsync();
                    await context.BbkBloodOrderLines.Where(x => orderIds.Contains(x.BloodOrderId)).ExecuteDeleteAsync();
                    await context.BbkBloodOrders.Where(x => orderIds.Contains(x.Id)).ExecuteDeleteAsync();
                }

                await context.MstBloodStorageLocations.Where(x => _locationIds.Contains(x.Id)).ExecuteDeleteAsync();
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
