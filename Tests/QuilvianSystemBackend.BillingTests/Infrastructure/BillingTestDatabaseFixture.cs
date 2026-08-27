using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
    ///
    /// Sejak keputusan pemilik pada RJ-BIL-BE-007, database bersama dapat dipakai — tetapi
    /// hanya melalui opt-in kedua QUILVIAN_BILLING_TEST_DB_ALLOW_SHARED yang harus diketik
    /// sengaja. Yang mencegah terulangnya insiden RJ-BIL-BE-002 bukan larangannya, melainkan
    /// hilangnya jalur diam: tidak ada lagi keadaan di mana migration terpasang ke database
    /// tim tanpa seseorang menyatakannya lebih dulu.
    ///
    /// Penanda production, staging, dan UAT tetap ditolak mutlak dan tidak mengenal opt-in.
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
        /// Environment variable kedua yang membuka penggunaan database bersama. Keputusan
        /// pemilik RJ-BIL-BE-007 mengizinkan test berjalan terhadap QuilvianNewDevTim01, tetapi
        /// izin itu sengaja tidak dijadikan perilaku bawaan.
        ///
        /// Insiden RJ-BIL-BE-002 terjadi karena fallback yang DIAM: seseorang menjalankan
        /// `dotnet test` dan migration ikut terpasang ke database tim tanpa ada yang
        /// memerintahkannya. Yang mencegah pengulangannya bukan larangan, melainkan syarat
        /// bahwa izin itu harus DIKETIK dengan sengaja. Gerbang tetap tertutup bagi siapa pun
        /// yang tidak tahu variable ini ada.
        /// </summary>
        public const string SharedDatabaseOptInVariable = "QUILVIAN_BILLING_TEST_DB_ALLOW_SHARED";

        /// <summary>
        /// Nilai yang harus diketik persis. Kalimatnya sengaja panjang dan menyatakan akibatnya,
        /// sehingga tidak mungkin terisi karena salah salin.
        /// </summary>
        public const string SharedDatabaseOptInValue = "I_ACCEPT_SHARED_DB_MUTATION";

        /// <summary>
        /// Nama database yang ditolak secara eksplisit. Daftar ini menutup database dev bersama
        /// yang pernah tersentuh pada RJ-BIL-BE-002. Dapat dibuka dengan opt-in.
        /// </summary>
        private static readonly string[] SharedDatabaseNames =
        {
            "QuilvianNewDevTim01"
        };

        /// <summary>
        /// Penanda nama yang menunjukkan database dipakai bersama anggota tim lain. Dapat dibuka
        /// dengan opt-in, karena "bersama" berarti mengganggu rekan kerja — bukan mengganggu
        /// pasien.
        /// </summary>
        private static readonly string[] SharedDatabaseMarkers =
        {
            "dev",
            "shared"
        };

        /// <summary>
        /// Penanda yang menunjukkan database melayani pengguna nyata atau menjadi gerbang rilis.
        /// Daftar ini TIDAK dapat dibuka oleh opt-in mana pun dan tidak boleh diberi jalan
        /// keluar. Menerapkan migration di sana bukan kecerobohan terhadap rekan kerja,
        /// melainkan terhadap orang yang datanya ada di dalamnya.
        /// </summary>
        private static readonly string[] AbsolutelyForbiddenMarkers =
        {
            "prod",
            "production",
            "live",
            "staging",
            "stage",
            "uat"
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
        private static string ResolveDedicatedTestConnectionString() =>
            ValidateTargetDatabase(
                Environment.GetEnvironmentVariable(ConnectionStringVariable),
                Environment.GetEnvironmentVariable(SharedDatabaseOptInVariable));

        /// <summary>
        /// Bentuk murni dari gerbang di atas: seluruh keputusan diambil dari kedua argumen dan
        /// tidak ada satu pun pembacaan environment di dalamnya.
        ///
        /// Pemisahan ini bukan kerapian belaka. Test yang menguji gerbang ini perlu mencoba
        /// belasan nama database, dan bila pengujiannya dilakukan dengan mengubah environment
        /// variable proses, ia akan bertabrakan dengan fixture kelas lain yang berjalan paralel
        /// dan sedang membaca variable yang sama.
        /// </summary>
        internal static string ValidateTargetDatabase(string? fromEnvironment, string? optInValue)
        {

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

            // Penanda production diperiksa lebih dulu dan tidak mengenal opt-in. Urutan ini
            // disengaja: apa pun isi variable opt-in, database yang melayani pengguna nyata
            // tetap ditolak.
            foreach (var marker in AbsolutelyForbiddenMarkers)
            {
                if (database.Contains(marker, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"{BlockedMarker}: nama database '{database}' mengandung penanda '{marker}', " +
                        "yang menandakan production, staging, atau UAT. Test ini menerapkan migration " +
                        "dan menulis baris. Penolakan ini mutlak dan tidak dapat dibuka oleh " +
                        $"{SharedDatabaseOptInVariable}.");
                }
            }

            var sharedOptIn = string.Equals(optInValue, SharedDatabaseOptInValue, StringComparison.Ordinal);

            var namedShared = SharedDatabaseNames.Any(
                forbidden => string.Equals(database, forbidden, StringComparison.OrdinalIgnoreCase));

            var markedShared = SharedDatabaseMarkers.FirstOrDefault(
                marker => database.Contains(marker, StringComparison.OrdinalIgnoreCase));

            var missingTestMarker = !database.Contains(RequiredTestMarker, StringComparison.OrdinalIgnoreCase);

            if (namedShared || markedShared is not null || missingTestMarker)
            {
                if (!sharedOptIn)
                {
                    var alasan = namedShared
                        ? $"database '{database}' termasuk daftar database yang dipakai bersama anggota tim lain"
                        : markedShared is not null
                            ? $"nama database '{database}' mengandung penanda '{markedShared}' yang menandakan database bersama"
                            : $"nama database '{database}' tidak mengandung penanda afirmatif '{RequiredTestMarker}'";

                    throw new InvalidOperationException(
                        $"{BlockedMarker}: {alasan}. Test ini menerapkan migration dan menulis baris, " +
                        "sehingga secara bawaan hanya berjalan terhadap database test tersendiri. " +
                        "Bila Anda memang bermaksud menjalankannya terhadap database bersama, isi " +
                        $"{SharedDatabaseOptInVariable} dengan nilai persis '{SharedDatabaseOptInValue}'. " +
                        "Izin itu sengaja tidak dijadikan perilaku bawaan agar tidak ada yang " +
                        "menerapkan migration ke database tim tanpa menyadarinya, seperti yang " +
                        "terjadi pada RJ-BIL-BE-002.");
                }

                Console.WriteLine(
                    $"[BILLING-TEST] PERINGATAN: {SharedDatabaseOptInVariable} aktif. Migration dan " +
                    $"penulisan baris akan dijalankan terhadap database bersama '{database}'. " +
                    "Dasar: keputusan pemilik pada RJ-BIL-BE-007.");
            }

            // Hanya nama host dan database yang dicetak. Username dan password tidak pernah
            // ikut ke output test.
            Console.WriteLine(
                $"[BILLING-TEST] Target database test '{database}' pada host '{builder.Host}'.");

            // Non-null karena pemeriksaan kosong di awal sudah melempar.
            return fromEnvironment!;
        }

        public Task DisposeAsync() => Task.CompletedTask;

        /// <summary>
        /// Menyediakan LoggerService untuk service yang membutuhkannya. Test tidak menjalankan
        /// HTTP request, sehingga HttpContext-nya memang kosong; LoggerService sudah menangani
        /// keadaan tersebut dengan menulis tanda "-".
        /// </summary>
        public static LoggerService CreateLoggerService() =>
            new(NullLogger<LoggerService>.Instance, new HttpContextAccessor());

        /// <summary>
        /// Menyediakan <see cref="IHttpContextAccessor"/> yang sudah memuat identitas petugas.
        ///
        /// Service klinis mengambil identitas pelakunya dari klaim pengguna pada HTTP context.
        /// Sebelum pembantu ini ada, test membuat <c>new HttpContextAccessor()</c> kosong,
        /// sehingga identitas pelakunya menjadi <c>Guid.Empty</c> dan seluruh penyerahan fakta
        /// ke Billing ditolak sebagai <c>CLIN_FACT_ACTOR_INVALID</c>. Yang diuji akhirnya bukan
        /// perilaku domain, melainkan ketiadaan pengguna yang login.
        /// </summary>
        public static IHttpContextAccessor CreateHttpContextAccessor(Guid actorUserId)
        {
            var identity = new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.NameIdentifier, actorUserId.ToString()) },
                authenticationType: "BillingTest");

            return new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        public ApplicationDbContext CreateContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseNpgsql(ConnectionString)
                .EnableSensitiveDataLogging()
                // EF Core 9 menjadikan PendingModelChangesWarning sebagai error, sehingga
                // Database.Migrate() menolak berjalan selama ada entity di model yang belum
                // punya migration. Keadaan itu memang sedang terjadi, dan penyebabnya di luar
                // blueprint ini: MstRegister, MstRoomChargePolicy, dan MstTaxRule milik modul
                // lain ada sebagai entity tetapi belum pernah dibuatkan migration.
                //
                // Membuatkan migration untuk modul orang lain bukan wewenang task ini, dan
                // menerapkannya ke database bersama justru menambah masalah. Yang ditekan di
                // sini hanya penjagaannya, bukan penyebabnya: migration yang tercatat tetap
                // diterapkan apa adanya, dan ketiga tabel yatim itu tetap tidak wujud. Test
                // Billing tidak menyentuh satu pun dari ketiganya.
                //
                // Penekanan ini sengaja dibatasi pada fixture test dan tidak menyentuh
                // konfigurasi aplikasi. Drift-nya dilaporkan, bukan disembunyikan.
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
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

                // Reconciliation case dibersihkan lebih dulu karena merujuk efek pemrosesan.
                // Tanpa ini, unique index case akan menabrak sisa baris test sebelumnya pada
                // database yang dipakai berulang kali.
                await context.BilReconciliationCases
                    .Where(x =>
                        x.EncounterId == seed.EncounterId ||
                        (x.FolioId != null && folioIds.Contains(x.FolioId.Value)) ||
                        (x.ChargeLineId != null && chargeLineIds.Contains(x.ChargeLineId.Value)))
                    .ExecuteDeleteAsync(cancellationToken);

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
                var specimenIds = await context.LabSpecimens
                    .Where(x => labOrderIds.Contains(x.LabOrderId))
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);

                await context.LabTransitionHistories
                    .Where(x =>
                        labOrderIds.Contains(x.LabOrderId) ||
                        (x.LabSpecimenId != null && specimenIds.Contains(x.LabSpecimenId.Value)))
                    .ExecuteDeleteAsync(cancellationToken);

                // Recollection membuat rantai specimen yang menunjuk specimen sebelumnya, sehingga
                // penghapusan diulang sampai tidak ada lagi baris yang tersisa.
                while (await context.LabSpecimens
                    .AnyAsync(x => labOrderIds.Contains(x.LabOrderId), cancellationToken))
                {
                    var removed = await context.LabSpecimens
                        .Where(x =>
                            labOrderIds.Contains(x.LabOrderId) &&
                            !context.LabSpecimens.Any(child => child.SupersededSpecimenId == x.Id))
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
            await context.BilClinicalMilestoneFacts
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
