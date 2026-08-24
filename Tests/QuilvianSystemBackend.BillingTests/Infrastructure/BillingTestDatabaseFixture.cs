using System.Text.Json;
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
    /// Menyiapkan database PostgreSQL untuk acceptance criteria RJ-BIL-BE-001.
    ///
    /// Connection string diambil dari environment variable QUILVIAN_BILLING_TEST_DB bila diisi,
    /// dan bila tidak, jatuh kembali ke ConnectionStrings:DefaultConnection pada
    /// appsettings.Development.json milik project utama.
    ///
    /// Test membuat lalu menghapus kembali seluruh baris miliknya sendiri dan tidak pernah
    /// menyentuh data yang sudah ada. Database production ditolak tanpa mekanisme override.
    /// </summary>
    public sealed class BillingTestDatabaseFixture : IAsyncLifetime
    {
        public const string ConnectionStringVariable = "QUILVIAN_BILLING_TEST_DB";

        /// <summary>
        /// Database dev bersama. Test tetap boleh berjalan di sini karena teardown menghapus
        /// kembali seluruh baris yang dibuatnya, tetapi namanya dicetak ke output test agar
        /// tidak pernah menjadi kejutan bagi yang menjalankannya.
        /// </summary>
        private static readonly string[] SharedDevelopmentDatabases =
        {
            "QuilvianNewDevTim01"
        };

        /// <summary>
        /// Penanda nama database production. Test menulis dan menghapus baris, sehingga
        /// menjalankannya terhadap production tidak pernah benar dan tidak disediakan
        /// mekanisme override apa pun.
        /// </summary>
        private static readonly string[] ProductionMarkers =
        {
            "prod",
            "production"
        };

        public string ConnectionString { get; private set; } = string.Empty;

        public Task InitializeAsync()
        {
            var (resolved, origin) = ResolveConnectionString();

            var database = new NpgsqlConnectionStringBuilder(resolved).Database ?? string.Empty;

            foreach (var marker in ProductionMarkers)
            {
                if (database.Contains(marker, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Database '{database}' terindikasi production. Test ini membuat dan menghapus " +
                        "baris, sehingga tidak pernah benar dijalankan terhadap production. " +
                        $"Arahkan {ConnectionStringVariable} ke database dev atau test.");
                }
            }

            foreach (var shared in SharedDevelopmentDatabases)
            {
                if (string.Equals(database, shared, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine(
                        $"[BILLING-TEST] Target database '{database}' adalah database dev bersama " +
                        $"(sumber: {origin}). Test membuat pasien, encounter, unit layanan, dan user " +
                        "sementara, lalu menghapus seluruhnya kembali pada teardown.");
                }
            }

            ConnectionString = resolved;

            using var context = CreateContext();
            context.Database.Migrate();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Mengambil connection string dari environment variable bila tersedia, dan bila tidak,
        /// jatuh kembali ke konfigurasi aplikasi. Fallback ini dipilih agar test tidak menuntut
        /// penyalinan kredensial ke environment variable atau perintah shell.
        /// </summary>
        private static (string ConnectionString, string Origin) ResolveConnectionString()
        {
            var fromEnvironment = Environment.GetEnvironmentVariable(ConnectionStringVariable);
            if (!string.IsNullOrWhiteSpace(fromEnvironment))
                return (fromEnvironment, ConnectionStringVariable);

            var configPath = LocateApplicationConfiguration();
            if (configPath == null)
            {
                throw new InvalidOperationException(
                    $"Environment variable {ConnectionStringVariable} belum diisi dan " +
                    "appsettings.Development.json tidak ditemukan pada direktori induk mana pun. " +
                    "Isi environment variable tersebut dengan connection string PostgreSQL.");
            }

            using var document = JsonDocument.Parse(File.ReadAllText(configPath));

            if (!document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings) ||
                !connectionStrings.TryGetProperty("DefaultConnection", out var defaultConnection))
            {
                throw new InvalidOperationException(
                    $"ConnectionStrings:DefaultConnection tidak ditemukan pada '{configPath}'.");
            }

            var value = defaultConnection.GetString();
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(
                    $"ConnectionStrings:DefaultConnection kosong pada '{configPath}'.");
            }

            return (value, Path.GetFileName(configPath));
        }

        /// <summary>
        /// Menelusuri direktori induk dari lokasi assembly test sampai menemukan project utama,
        /// lalu mengembalikan path appsettings.Development.json di sebelahnya.
        /// </summary>
        private static string? LocateApplicationConfiguration()
        {
            var directory = new DirectoryInfo(AppContext.BaseDirectory);

            while (directory != null)
            {
                var candidate = Path.Combine(directory.FullName, "appsettings.Development.json");
                var projectMarker = Path.Combine(directory.FullName, "QuilvianSystemBackend.csproj");

                if (File.Exists(candidate) && File.Exists(projectMarker))
                    return candidate;

                directory = directory.Parent;
            }

            return null;
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
        /// unit layanan, dan user. Ini penting ketika test dijalankan terhadap database
        /// bersama: tanpa teardown penuh, pasien dan encounter palsu akan tertinggal dan
        /// muncul pada daftar yang dilihat anggota tim lain.
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
