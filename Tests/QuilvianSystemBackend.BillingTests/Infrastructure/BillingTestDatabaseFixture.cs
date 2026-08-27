using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.Infrastructure
{
    /// <summary>
    /// Menyiapkan database PostgreSQL khusus test untuk acceptance criteria Billing.
    ///
    /// Fixture ini menjalankan <c>Database.Migrate()</c> dan menulis baris nyata, sehingga
    /// targetnya wajib database test tersendiri yang boleh dibuang kapan saja.
    ///
    /// Perilaku fail-closed sesuai keputusan author RJ-BIL-BE-003:
    /// connection string HANYA dibaca dari environment variable QUILVIAN_BILLING_TEST_DB.
    /// Bila variable tersebut kosong, fixture berhenti dengan configuration error dan tidak
    /// menyentuh database mana pun.
    ///
    /// Fallback ke appsettings.Development.json sengaja DIHAPUS. Pada RJ-BIL-BE-002 fallback
    /// itulah yang menyebabkan `dotnet test` menerapkan migration ke database dev bersama
    /// QuilvianNewDevTim01 tanpa ada yang memerintahkannya. Menghapus fallback juga membuat
    /// fixture tidak lagi membaca kredensial dari file konfigurasi mana pun.
    /// </summary>
    public sealed class BillingTestDatabaseFixture : IAsyncLifetime
    {
        public const string ConnectionStringVariable = "QUILVIAN_BILLING_TEST_DB";

        /// <summary>
        /// Penanda yang dicetak pada setiap configuration error agar hasil test dapat dilaporkan
        /// sebagai "terhalang konfigurasi", bukan sebagai kegagalan domain.
        /// </summary>
        public const string BlockedMarker = "BLOCKED_BY_TEST_DB_CONFIGURATION";

        /// <summary>
        /// Bukti afirmatif bahwa target memang database test. Fixture menolak berjalan tanpa
        /// penanda ini, sehingga salah ketik nama database berakhir sebagai penolakan, bukan
        /// sebagai migration yang terlanjur diterapkan ke database orang lain.
        /// </summary>
        private const string RequiredTestMarker = "test";

        /// <summary>
        /// Nama database yang ditolak secara eksplisit. Daftar ini menutup database dev bersama
        /// yang pernah tersentuh pada RJ-BIL-BE-002.
        /// </summary>
        private static readonly string[] ForbiddenDatabaseNames =
        {
            "QuilvianNewDevTim01"
        };

        /// <summary>
        /// Penanda nama yang menunjukkan database dipakai bersama orang lain atau melayani
        /// pengguna nyata. Test menerapkan migration dan menulis baris, sehingga tidak pernah
        /// benar dijalankan terhadap database seperti itu dan tidak disediakan override apa pun.
        /// </summary>
        private static readonly string[] ForbiddenDatabaseMarkers =
        {
            "prod",
            "production",
            "live",
            "staging",
            "stage",
            "uat",
            "dev",
            "shared"
        };

        public string ConnectionString { get; private set; } = string.Empty;

        public Task InitializeAsync()
        {
            ConnectionString = ResolveDedicatedTestConnectionString();

            using var context = CreateContext();
            context.Database.Migrate();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Mengambil connection string dari environment variable dan membuktikan bahwa targetnya
        /// database test tersendiri sebelum satu perintah pun dikirim ke server.
        ///
        /// Urutan pemeriksaan disusun agar pesan yang muncul adalah pesan yang paling berguna:
        /// variable kosong, nilai tidak sah, nama database kosong, nama terlarang, lalu bukti
        /// afirmatif penanda test.
        /// </summary>
        private static string ResolveDedicatedTestConnectionString()
        {
            var fromEnvironment = Environment.GetEnvironmentVariable(ConnectionStringVariable);

            if (string.IsNullOrWhiteSpace(fromEnvironment))
            {
                throw new InvalidOperationException(
                    $"{BlockedMarker}: environment variable {ConnectionStringVariable} belum diisi. " +
                    "Integration test Billing menjalankan migration dan menulis baris nyata, sehingga " +
                    "hanya boleh berjalan terhadap database test tersendiri yang boleh dibuang. " +
                    "Fallback ke appsettings.Development.json sudah dihapus setelah temuan " +
                    "RJ-BIL-BE-002 agar test tidak pernah lagi menerapkan migration ke database dev " +
                    "bersama. Isi variable tersebut dengan connection string PostgreSQL yang menunjuk " +
                    $"database khusus test, dan pastikan nama database-nya mengandung '{RequiredTestMarker}'.");
            }

            NpgsqlConnectionStringBuilder builder;

            try
            {
                builder = new NpgsqlConnectionStringBuilder(fromEnvironment);
            }
            catch (Exception exception)
            {
                // Pesan exception asli dapat memuat potongan connection string, sehingga tidak
                // ikut ditampilkan agar kredensial tidak bocor ke output test.
                throw new InvalidOperationException(
                    $"{BlockedMarker}: nilai {ConnectionStringVariable} bukan connection string " +
                    $"PostgreSQL yang sah ({exception.GetType().Name}).");
            }

            var database = builder.Database ?? string.Empty;

            if (string.IsNullOrWhiteSpace(database))
            {
                throw new InvalidOperationException(
                    $"{BlockedMarker}: {ConnectionStringVariable} tidak menyebutkan nama database. " +
                    "Tambahkan bagian 'Database=' pada connection string.");
            }

            foreach (var forbidden in ForbiddenDatabaseNames)
            {
                if (string.Equals(database, forbidden, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"{BlockedMarker}: database '{database}' termasuk daftar terlarang karena " +
                        "dipakai bersama anggota tim lain. Test ini menerapkan migration, sehingga " +
                        "menjalankannya di sana mengubah skema milik orang lain. Arahkan " +
                        $"{ConnectionStringVariable} ke database test tersendiri.");
                }
            }

            foreach (var marker in ForbiddenDatabaseMarkers)
            {
                if (database.Contains(marker, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"{BlockedMarker}: nama database '{database}' mengandung penanda '{marker}', " +
                        "yang menandakan database bersama, staging, atau production. Test ini " +
                        "menerapkan migration dan menulis baris, sehingga hanya boleh berjalan " +
                        "terhadap database test tersendiri.");
                }
            }

            if (!database.Contains(RequiredTestMarker, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"{BlockedMarker}: nama database '{database}' tidak mengandung penanda " +
                    $"'{RequiredTestMarker}'. Fixture menuntut bukti afirmatif bahwa targetnya memang " +
                    "database test, sehingga salah ketik nama berakhir sebagai penolakan dan bukan " +
                    "sebagai migration yang terlanjur diterapkan ke database yang salah. Beri nama " +
                    "database test Anda misalnya 'QuilvianBillingTest'.");
            }

            // Hanya nama host dan database yang dicetak. Username dan password tidak pernah
            // ikut ke output test.
            Console.WriteLine(
                $"[BILLING-TEST] Target database test '{database}' pada host '{builder.Host}'.");

            return fromEnvironment;
        }

        public Task DisposeAsync() => Task.CompletedTask;

        /// <summary>
        /// Menyediakan LoggerService untuk service yang membutuhkannya. Test tidak menjalankan
        /// HTTP request, sehingga HttpContext-nya memang kosong; LoggerService sudah menangani
        /// keadaan tersebut dengan menulis tanda "-".
        /// </summary>
        public static LoggerService CreateLoggerService() =>
            new(NullLogger<LoggerService>.Instance, new HttpContextAccessor());

        public ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(ConnectionString)
                .EnableSensitiveDataLogging()
                .Options;

            return new ApplicationDbContext(options);
        }

        /// <summary>
        /// Membuat prasyarat FK minimal untuk sebuah encounter: user, pasien, unit layanan,
        /// lalu encounter itu sendiri. Seluruhnya memakai GUID baru agar test tidak saling
        /// mengganggu dan tidak bergantung pada data yang kebetulan sudah ada.
        /// </summary>
        public async Task<EncounterSeed> SeedEncounterAsync(CancellationToken cancellationToken = default)
        {
            await using var context = CreateContext();

            var suffix = Guid.NewGuid().ToString("N")[..12];

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserCode = $"TST{suffix}",
                DisplayName = $"Billing Test Actor {suffix}",
                UserName = $"billing.test.{suffix}",
                NormalizedUserName = $"BILLING.TEST.{suffix}".ToUpperInvariant(),
                Email = $"billing.test.{suffix}@example.invalid",
                NormalizedEmail = $"BILLING.TEST.{suffix}@EXAMPLE.INVALID".ToUpperInvariant(),
                SecurityStamp = Guid.NewGuid().ToString("N"),
                ConcurrencyStamp = Guid.NewGuid().ToString("N"),
                IsActive = true
            };

            var patient = new MstPatient
            {
                Id = Guid.NewGuid(),
                PatientCode = $"PC{suffix}",
                MedicalRecordNumber = $"MR{suffix}",
                FullName = $"Pasien Test {suffix}"
            };

            var serviceUnit = new MstServiceUnit
            {
                Id = Guid.NewGuid(),
                ServiceUnitCode = $"SU{suffix}",
                ServiceUnitName = $"Unit Test {suffix}"
            };

            context.Users.Add(user);
            context.MstPatients.Add(patient);
            context.MstServiceUnits.Add(serviceUnit);
            await context.SaveChangesAsync(cancellationToken);

            var encounter = new TrxPatientEncounter
            {
                Id = Guid.NewGuid(),
                EncounterNumber = $"ENC{suffix}",
                EncounterDate = DateTime.UtcNow,
                PatientId = patient.Id,
                ServiceUnitId = serviceUnit.Id,
                RegisteredByUserId = user.Id
            };

            context.TrxPatientEncounters.Add(encounter);
            await context.SaveChangesAsync(cancellationToken);

            return new EncounterSeed(encounter.Id, user.Id, patient.Id, serviceUnit.Id);
        }

        /// <summary>
        /// Menghapus seluruh baris yang dibuat oleh satu test, dari anak ke induk agar tidak
        /// melanggar DeleteBehavior.Restrict.
        ///
        /// Teardown mencakup baris Billing sekaligus prasyarat FK-nya — encounter, pasien,
        /// unit layanan, dan user.
        /// </summary>
        public async Task CleanupEncounterAsync(
            EncounterSeed seed,
            CancellationToken cancellationToken = default)
        {
            await using var context = CreateContext();

            var folioIds = await context.BilFolios
                .Where(x => x.EncounterId == seed.EncounterId)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (folioIds.Count > 0)
            {
                var chargeLineIds = await context.BilChargeLines
                    .Where(x => folioIds.Contains(x.FolioId))
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);

                await context.BilProcessingEffects
                    .Where(x =>
                        (x.FolioId != null && folioIds.Contains(x.FolioId.Value)) ||
                        (x.ChargeLineId != null && chargeLineIds.Contains(x.ChargeLineId.Value)))
                    .ExecuteDeleteAsync(cancellationToken);

                await context.BilChargeComponents
                    .Where(x => chargeLineIds.Contains(x.ChargeLineId))
                    .ExecuteDeleteAsync(cancellationToken);

                await context.BilChargeLines
                    .Where(x => chargeLineIds.Contains(x.Id))
                    .ExecuteDeleteAsync(cancellationToken);

                await context.BilFolios
                    .Where(x => folioIds.Contains(x.Id))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            // Riwayat transisi dan specimen Lab (RJ-BIL-BE-003) menggantung pada LabOrder, dan
            // LabOrder menggantung pada encounter, sehingga urutannya dari yang paling anak.
            var labOrderIds = await context.LabOrders
                .Where(x => x.EncounterId == seed.EncounterId)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            if (labOrderIds.Count > 0)
            {
                var specimenIds = await context.TrxLabSpecimens
                    .Where(x => labOrderIds.Contains(x.LabOrderId))
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);

                await context.TrxLabTransitionHistories
                    .Where(x =>
                        labOrderIds.Contains(x.LabOrderId) ||
                        (x.LabSpecimenId != null && specimenIds.Contains(x.LabSpecimenId.Value)))
                    .ExecuteDeleteAsync(cancellationToken);

                // Recollection membuat rantai specimen yang menunjuk specimen sebelumnya, sehingga
                // penghapusan diulang sampai tidak ada lagi baris yang tersisa.
                while (await context.TrxLabSpecimens
                    .AnyAsync(x => labOrderIds.Contains(x.LabOrderId), cancellationToken))
                {
                    var removed = await context.TrxLabSpecimens
                        .Where(x =>
                            labOrderIds.Contains(x.LabOrderId) &&
                            !context.TrxLabSpecimens.Any(child => child.SupersededSpecimenId == x.Id))
                        .ExecuteDeleteAsync(cancellationToken);

                    if (removed == 0)
                        break;
                }

                await context.LabOrders
                    .Where(x => labOrderIds.Contains(x.Id))
                    .ExecuteDeleteAsync(cancellationToken);
            }

            // Ledger fakta klinis (RJ-BIL-BE-002) memiliki FK Restrict ke encounter, sehingga
            // wajib dihapus sebelum encounter-nya.
            await context.TrxClinicalMilestoneFacts
                .Where(x => x.EncounterId == seed.EncounterId)
                .ExecuteDeleteAsync(cancellationToken);

            await context.TrxPatientEncounters
                .Where(x => x.Id == seed.EncounterId)
                .ExecuteDeleteAsync(cancellationToken);

            await context.MstPatients
                .Where(x => x.Id == seed.PatientId)
                .ExecuteDeleteAsync(cancellationToken);

            await context.MstServiceUnits
                .Where(x => x.Id == seed.ServiceUnitId)
                .ExecuteDeleteAsync(cancellationToken);

            await context.Users
                .Where(x => x.Id == seed.ActorUserId)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
