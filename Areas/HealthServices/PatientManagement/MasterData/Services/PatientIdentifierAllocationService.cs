using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services
{
    public sealed record PatientIdentifierAllocationResult(
        RsmmcReservationGuardEvaluation Guard,
        string? PatientCode,
        string? MedicalRecordNumber)
    {
        public bool Succeeded => PatientCode is not null && MedicalRecordNumber is not null;

        /// <summary>Kode galat kontrak bila alokasi ditolak; <c>null</c> bila berhasil.</summary>
        public string? ErrorCode => Succeeded ? null : RsmmcIdentifierReservationProfile.GuardConfigInvalidErrorCode;
    }

    /// <summary>
    /// Allocator <c>PatientCode</c> dan MRN untuk pendaftaran pasien normal (`BE-PAT-MIG-003`).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Satu-satunya pemilik alokasi nomor pasien normal. <c>POST /</c>, <c>POST /kiosk</c>, dan
    /// <c>POST /admin</c> bermuara pada <c>PatientController.CreatePatient</c>, yang memanggil
    /// <see cref="AllocateAsync"/>; controller tidak lagi mengalokasikan nomor (`QBE-CODE-002`).
    /// Perlindungan ini berada di bawah otorisasi rute, sehingga rute kiosk yang dijaga policy
    /// berbeda tidak dapat melewatinya.
    /// </para>
    /// <para>
    /// Perilaku per environment mengikuti <see cref="RsmmcIdentifierReservationProfile.Evaluate"/>:
    /// </para>
    /// <list type="bullet">
    /// <item><description>
    /// Di luar <c>Staging</c> persis: algoritma lama apa adanya — nomor kosong terkecil mulai 1,
    /// tanpa kunci baru dan tanpa membaca konfigurasi guard — sehingga hasil penomoran Produksi
    /// tidak berubah (`PAT-DEC-066`, `PAT-DEC-067`).
    /// </description></item>
    /// <item><description>
    /// <c>Staging</c> persis dengan <c>Enabled = true</c>: suffix <c>PatientCode</c> pertama yang
    /// belum dipakai ≥ 705.250 dan MRN numerik pertama yang belum dipakai ≥ 890.188, di bawah
    /// <c>pg_advisory_xact_lock</c> di dalam transaksi pemanggil. Indeks unik <c>PatientCode</c>
    /// dan <c>MedicalRecordNumber</c> tetap jaring terakhir (`QBE-CODE-004`).
    /// </description></item>
    /// <item><description>
    /// <c>Staging</c> persis dengan konfigurasi lain: ditolak sebelum satu nomor pun dihitung.
    /// </description></item>
    /// </list>
    /// </remarks>
    public sealed class PatientIdentifierAllocationService
    {
        private const string MedicalRecordNumberExhaustedMessage =
            "Nomor rekam medis sudah mencapai batas maksimum 99-99-99-99.";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public PatientIdentifierAllocationService(
            ApplicationDbContext dbContext,
            IHostEnvironment environment,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _environment = environment;
            _configuration = configuration;
        }

        public RsmmcReservationGuardEvaluation EvaluateGuard() =>
            RsmmcIdentifierReservationProfile.Evaluate(_environment.EnvironmentName, _configuration);

        /// <summary>
        /// Mengalokasikan <c>PatientCode</c> dan MRN untuk satu pendaftaran pasien normal.
        /// </summary>
        /// <remarks>
        /// Saat guard aktif, wajib dipanggil di dalam transaksi pendaftaran: kunci database dipegang
        /// sampai transaksi itu selesai, sehingga pendaftaran lain — dari instance aplikasi mana
        /// pun, kiosk maupun admin — menunggu dan baru melihat nomor ini sesudah tersimpan. Bila
        /// ditolak, tidak ada yang dibaca atau ditulis; pemanggil wajib berhenti sebelum mutasi.
        /// </remarks>
        public async Task<PatientIdentifierAllocationResult> AllocateAsync(
            CancellationToken cancellationToken = default)
        {
            var guard = EvaluateGuard();

            if (guard.IsFailClosed)
            {
                return new PatientIdentifierAllocationResult(guard, null, null);
            }

            if (!guard.IsActive)
            {
                return new PatientIdentifierAllocationResult(
                    guard,
                    await AllocateLegacyPatientCodeAsync(cancellationToken),
                    await AllocateLegacyMedicalRecordNumberAsync(cancellationToken));
            }

            if (_dbContext.Database.CurrentTransaction is null)
            {
                throw new InvalidOperationException(
                    "Alokasi nomor pasien aman reservasi wajib berada di dalam transaction.");
            }

            // Kunci transaksi dua bagian dengan konstanta tetap (bukan hashtext), sama bagi seluruh
            // instance aplikasi dan lepas sendiri saat transaksi pendaftaran berakhir.
            // pg_advisory_xact_lock hanya ada di PostgreSQL; penyedia lain (SQLite pada test) tetap
            // berjalan di dalam transaction, dan kebenarannya saat berbarengan dibuktikan pada
            // PostgreSQL sungguhan (`PAT-MIG3-AC-13`).
            if (_dbContext.Database.IsNpgsql())
            {
                await _dbContext.Database.ExecuteSqlRawAsync(
                    "SELECT pg_advisory_xact_lock({0}, {1});",
                    [
                        RsmmcIdentifierReservationProfile.AllocationLockNamespace,
                        RsmmcIdentifierReservationProfile.AllocationLockId
                    ],
                    cancellationToken);
            }

            var patientCodeNumber = FirstUnusedAtOrAbove(
                await LoadPatientCodeNumbersAtOrAboveFloorAsync(cancellationToken),
                RsmmcIdentifierReservationProfile.PatientCodeFloor);

            var medicalRecordNumber = FirstUnusedAtOrAbove(
                await LoadMedicalRecordNumbersAtOrAboveFloorAsync(cancellationToken),
                RsmmcIdentifierReservationProfile.MedicalRecordNumberFloor);

            if (medicalRecordNumber > RsmmcIdentifierReservationProfile.MedicalRecordNumberMaximum)
            {
                throw new InvalidOperationException(MedicalRecordNumberExhaustedMessage);
            }

            return new PatientIdentifierAllocationResult(
                guard,
                RsmmcIdentifierReservationProfile.FormatPatientCode(patientCodeNumber),
                RsmmcIdentifierReservationProfile.FormatMedicalRecordNumber(medicalRecordNumber));
        }

        /// <summary>
        /// Pemeriksa baca-saja untuk <c>PAT-GATE-005</c> dan gerbang worker
        /// <c>NORMAL_ALLOCATION_GUARD</c>: menghitung nomor yang <b>akan</b> diberikan tanpa
        /// mengambil kunci, tanpa mencadangkan, dan tanpa menulis apa pun.
        /// </summary>
        /// <remarks>
        /// Di <c>Staging</c> persis nomor dihitung dengan aturan lantai walau guard belum aktif,
        /// supaya pemilik melihat nomor yang akan dipakai begitu guard dinyalakan;
        /// <see cref="RsmmcReservationGuardStatusResponse.OverallGuardReady"/> tetap <c>false</c>.
        /// </remarks>
        public async Task<RsmmcReservationGuardStatusResponse> CheckAsync(
            CancellationToken cancellationToken = default)
        {
            var guard = EvaluateGuard();

            long? nextPatientCode;
            long? nextMedicalRecordNumber;

            if (guard.IsExactStaging)
            {
                nextPatientCode = FirstUnusedAtOrAbove(
                    await LoadPatientCodeNumbersAtOrAboveFloorAsync(cancellationToken),
                    RsmmcIdentifierReservationProfile.PatientCodeFloor);

                var mrn = FirstUnusedAtOrAbove(
                    await LoadMedicalRecordNumbersAtOrAboveFloorAsync(cancellationToken),
                    RsmmcIdentifierReservationProfile.MedicalRecordNumberFloor);

                nextMedicalRecordNumber = mrn <= RsmmcIdentifierReservationProfile.MedicalRecordNumberMaximum
                    ? mrn
                    : null;
            }
            else
            {
                nextPatientCode = await NextLegacyPatientCodeNumberAsync(cancellationToken);
                nextMedicalRecordNumber = await TryNextLegacyMedicalRecordNumberAsync(cancellationToken);
            }

            var patientCodeFloorSatisfied =
                nextPatientCode >= RsmmcIdentifierReservationProfile.PatientCodeFloor;
            var mrnFloorSatisfied =
                nextMedicalRecordNumber >= RsmmcIdentifierReservationProfile.MedicalRecordNumberFloor;

            return new RsmmcReservationGuardStatusResponse
            {
                Environment = guard.EnvironmentName ?? string.Empty,
                GuardState = guard.StateCode,
                GuardEnabled = guard.IsActive,
                GuardConfigurationValid = guard.ConfigurationValid,
                PatientCodeFloor = RsmmcIdentifierReservationProfile.PatientCodeFloor,
                PatientCodeFloorFormatted = RsmmcIdentifierReservationProfile.FormatPatientCode(
                    RsmmcIdentifierReservationProfile.PatientCodeFloor),
                MrnFloor = RsmmcIdentifierReservationProfile.MedicalRecordNumberFloor,
                MrnFloorFormatted = RsmmcIdentifierReservationProfile.FormatMedicalRecordNumber(
                    RsmmcIdentifierReservationProfile.MedicalRecordNumberFloor),
                NextPatientCodeNumeric = nextPatientCode,
                NextPatientCodeFormatted = nextPatientCode is { } code
                    ? RsmmcIdentifierReservationProfile.FormatPatientCode(code)
                    : null,
                NextMrnNumeric = nextMedicalRecordNumber,
                NextMrnFormatted = nextMedicalRecordNumber is { } number
                    ? RsmmcIdentifierReservationProfile.FormatMedicalRecordNumber(number)
                    : null,
                PatientCodeFloorSatisfied = patientCodeFloorSatisfied,
                MrnFloorSatisfied = mrnFloorSatisfied,
                OverallGuardReady = guard.IsActive &&
                    guard.ConfigurationValid &&
                    patientCodeFloorSatisfied &&
                    mrnFloorSatisfied,
                CheckedAtUtc = DateTime.UtcNow
            };
        }

        // ---- Mode guard aktif -------------------------------------------------------------

        // Hanya kode yang suffix-nya mungkin ≥ lantai yang dibaca: suffix lebih dari enam digit,
        // atau tepat enam digit dan ≥ "705250". Untuk teks angka sepanjang sama, urutan teks sama
        // dengan urutan angka, sehingga ratusan ribu kode cadangan di bawah lantai tidak dimuat.
        // Pasien bertanda terhapus ikut dibaca karena indeks unik PatientCode tanpa syarat.
        private async Task<HashSet<long>> LoadPatientCodeNumbersAtOrAboveFloorAsync(
            CancellationToken cancellationToken)
        {
            const string prefix = RsmmcIdentifierReservationProfile.PatientCodePrefix;
            var floorText = RsmmcIdentifierReservationProfile.FormatPatientCode(
                RsmmcIdentifierReservationProfile.PatientCodeFloor);
            var floorLength = floorText.Length;

            var codes = await _dbContext.Set<MstPatient>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x =>
                    x.PatientCode.StartsWith(prefix) &&
                    (x.PatientCode.Length > floorLength ||
                        (x.PatientCode.Length == floorLength &&
                            string.Compare(x.PatientCode, floorText) >= 0)))
                .Select(x => x.PatientCode)
                .ToListAsync(cancellationToken);

            var used = new HashSet<long>();

            foreach (var code in codes)
            {
                if (code.StartsWith(prefix, StringComparison.Ordinal) &&
                    long.TryParse(
                        code.AsSpan(prefix.Length),
                        NumberStyles.None,
                        CultureInfo.InvariantCulture,
                        out var number) &&
                    number >= RsmmcIdentifierReservationProfile.PatientCodeFloor)
                {
                    used.Add(number);
                }
            }

            return used;
        }

        // MRN berformat kanonik 00-00-00-00 dibaca hanya bila ≥ "00-89-01-88". MRN berformat lain
        // seluruhnya dibaca lalu dinormalkan ke angka seperti pembangkit lama, supaya nomor yang
        // sama dalam format berbeda tetap dihitung terpakai — indeks unik tidak dapat menangkapnya.
        private async Task<HashSet<long>> LoadMedicalRecordNumbersAtOrAboveFloorAsync(
            CancellationToken cancellationToken)
        {
            var floorText = RsmmcIdentifierReservationProfile.FormatMedicalRecordNumber(
                RsmmcIdentifierReservationProfile.MedicalRecordNumberFloor);

            var numbers = await _dbContext.Set<MstPatient>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x =>
                    x.MedicalRecordNumber != null &&
                    x.MedicalRecordNumber != string.Empty &&
                    (!EF.Functions.Like(
                            x.MedicalRecordNumber,
                            RsmmcIdentifierReservationProfile.FormattedMedicalRecordNumberPattern) ||
                        string.Compare(x.MedicalRecordNumber, floorText) >= 0))
                .Select(x => x.MedicalRecordNumber)
                .ToListAsync(cancellationToken);

            var used = new HashSet<long>();

            foreach (var value in numbers)
            {
                var digits = PatientQrPayloadBuilder.NormalizeToRawDigits(value);

                if (digits is not null &&
                    long.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var number) &&
                    number >= RsmmcIdentifierReservationProfile.MedicalRecordNumberFloor)
                {
                    used.Add(number);
                }
            }

            return used;
        }

        private static long FirstUnusedAtOrAbove(HashSet<long> used, long floor)
        {
            var candidate = floor;

            while (used.Contains(candidate))
            {
                candidate++;
            }

            return candidate;
        }

        // ---- Mode lama, di luar Staging persis -------------------------------------------
        // Dipindah apa adanya dari PatientController (GeneratePatientCodeAsync,
        // GenerateRunningCodeAsync, GenerateMedicalRecordNumberAsync pada a52b3c98) supaya hasil
        // yang terlihat dari luar tidak berubah: termasuk int.TryParse/int.Parse, mulai dari 1,
        // dan galat yang sama bila MRN melewati 99-99-99-99.

        private async Task<string> AllocateLegacyPatientCodeAsync(CancellationToken cancellationToken)
        {
            var nextNumber = await NextLegacyPatientCodeNumberAsync(cancellationToken);

            return RsmmcIdentifierReservationProfile.PatientCodePrefix +
                nextNumber.ToString().PadLeft(RsmmcIdentifierReservationProfile.PatientCodeNumberLength, '0');
        }

        private async Task<int> NextLegacyPatientCodeNumberAsync(CancellationToken cancellationToken)
        {
            const string prefix = RsmmcIdentifierReservationProfile.PatientCodePrefix;

            var existingCodes = await _dbContext.Set<MstPatient>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Select(x => x.PatientCode)
                .Where(x => x.StartsWith(prefix))
                .ToListAsync(cancellationToken);

            var usedNumbers = existingCodes
                .Select(x => x.Replace(prefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return nextNumber;
        }

        private async Task<string> AllocateLegacyMedicalRecordNumberAsync(CancellationToken cancellationToken)
        {
            var nextNumber = await NextLegacyMedicalRecordNumberAsync(cancellationToken);

            if (nextNumber > RsmmcIdentifierReservationProfile.MedicalRecordNumberMaximum)
            {
                throw new InvalidOperationException(MedicalRecordNumberExhaustedMessage);
            }

            var rawNumber = nextNumber.ToString().PadLeft(8, '0');

            return PatientQrPayloadBuilder.FormatMedicalRecordNumber(rawNumber);
        }

        private async Task<int> NextLegacyMedicalRecordNumberAsync(CancellationToken cancellationToken)
        {
            var existingNumbers = await _dbContext.Set<MstPatient>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.MedicalRecordNumber != null && x.MedicalRecordNumber != string.Empty)
                .Select(x => x.MedicalRecordNumber)
                .ToListAsync(cancellationToken);

            var usedNumbers = existingNumbers
                .Select(PatientQrPayloadBuilder.NormalizeToRawDigits)
                .Where(x => x != null)
                .Select(x => int.Parse(x!))
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return nextNumber;
        }

        // Pemeriksa tidak boleh gagal hanya karena data MRN lama yang tidak dapat dibaca
        // pembangkit lama; nilainya dilaporkan kosong.
        private async Task<long?> TryNextLegacyMedicalRecordNumberAsync(CancellationToken cancellationToken)
        {
            try
            {
                var next = await NextLegacyMedicalRecordNumberAsync(cancellationToken);

                return next <= RsmmcIdentifierReservationProfile.MedicalRecordNumberMaximum ? next : null;
            }
            catch (OverflowException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}
