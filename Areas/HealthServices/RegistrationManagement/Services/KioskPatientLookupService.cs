using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Cek Nomor Rekam Medis dari Kiosk (<c>BE-KSK-001</c>, <c>KSK-CONTRACT-v1</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Hanya membaca. Tidak ada transaksi tulis, dan seluruh query <c>AsNoTracking</c>.
    /// Nilai masukan dinormalkan di sini lalu dikirim sebagai parameter; normalisasi di sisi
    /// kolom hanya diperlukan untuk No. HP karena data tersimpan tidak seragam
    /// (<c>KSK-FACT-006</c>, <c>KSK-DSN-002</c>).
    /// </para>
    /// <para>
    /// <b>Kenapa kartu hanya dikirim untuk tepat satu pasien aktif.</b> Nomor HP keluarga dipakai
    /// bersama, dan satu KTP bisa tertinggal pada pasien yang sudah digabung. Menampilkan salah
    /// satunya berarti mendaftarkan kunjungan ke rekam medis yang salah; menampilkan semuanya
    /// berarti satu nomor HP membuka data seluruh keluarga (<c>KSK-DEC-006/007/017</c>).
    /// </para>
    /// </remarks>
    public class KioskPatientLookupService
    {
        public const string NextActionExistingPatient = "EXISTING_PATIENT_REGISTRATION";
        public const string NextActionNewPatient = "NEW_PATIENT_REGISTRATION";
        public const string NextActionUseIdentityOrContactStaff = "USE_IDENTITY_NUMBER_OR_CONTACT_STAFF";
        public const string NextActionContactStaff = "CONTACT_STAFF";

        private const int MaxValueLength = 32;
        private const int MaxMergeHops = 3;

        private static readonly Regex IdentityNumberPattern = new("^[0-9]{16}$", RegexOptions.Compiled);
        private static readonly Regex PhoneAllowedCharacters = new(@"^[0-9+\s\-\.\(\)]+$", RegexOptions.Compiled);

        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<KioskPatientLookupService> _logger;

        public KioskPatientLookupService(
            ApplicationDbContext dbContext,
            ILogger<KioskPatientLookupService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<KioskPatientLookupServiceResult> LookupAsync(
            KioskPatientLookupRequest request,
            string? deviceUserId,
            CancellationToken cancellationToken = default)
        {
            var validation = Normalize(request);

            if (validation.ErrorMessage != null)
            {
                return KioskPatientLookupServiceResult.Invalid(validation.ErrorMessage);
            }

            var searchType = validation.SearchType;
            var value = validation.NormalizedValue!;

            var matchedPatientIds = await FindMatchedPatientIdsAsync(searchType, value, cancellationToken);
            var response = await ResolveResponseAsync(searchType, matchedPatientIds, cancellationToken);

            // Nilai utuh, nama, dan No. RM hasil sengaja tidak dicatat (SEC-KSK-005, KSK-AC-008).
            _logger.LogInformation(
                "Kiosk patient lookup. SearchType={SearchType} Result={Result} Value={MaskedValue} DeviceUserId={DeviceUserId}",
                searchType,
                response.Result,
                MaskValue(value),
                deviceUserId ?? "-");

            return KioskPatientLookupServiceResult.Success(response);
        }

        private static NormalizationResult Normalize(KioskPatientLookupRequest? request)
        {
            if (request?.SearchType == null || !Enum.IsDefined(request.SearchType.Value))
            {
                return NormalizationResult.Fail("Pilih cara pencarian terlebih dahulu.");
            }

            var searchType = request.SearchType.Value;
            var raw = request.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(raw))
            {
                return NormalizationResult.Fail("Nomor wajib diisi.");
            }

            if (raw.Length > MaxValueLength ||
                raw.Any(char.IsControl) ||
                raw.Contains('<') ||
                raw.Contains('>'))
            {
                return NormalizationResult.Fail("Isian mengandung karakter yang tidak diizinkan.");
            }

            switch (searchType)
            {
                case KioskPatientLookupSearchType.IdentityNumber:
                {
                    var identityNumber = new string(raw.Where(x => !char.IsWhiteSpace(x)).ToArray());

                    return IdentityNumberPattern.IsMatch(identityNumber)
                        ? NormalizationResult.Ok(searchType, identityNumber)
                        : NormalizationResult.Fail("Nomor KTP harus terdiri dari 16 digit.");
                }

                case KioskPatientLookupSearchType.PhoneNumber:
                {
                    if (!PhoneAllowedCharacters.IsMatch(raw))
                    {
                        return NormalizationResult.Fail("Nomor HP hanya boleh berisi angka.");
                    }

                    var phoneNumber = NormalizePhoneNumber(raw);

                    return phoneNumber.StartsWith("62", StringComparison.Ordinal) &&
                           phoneNumber.Length is >= 9 and <= 15
                        ? NormalizationResult.Ok(searchType, phoneNumber)
                        : NormalizationResult.Fail("Nomor HP tidak valid. Contoh: 081234567890.");
                }

                default:
                    return NormalizationResult.Ok(searchType, raw.Trim().ToLowerInvariant());
            }
        }

        /// <summary>
        /// Buang semua selain digit, lalu awalan <c>0</c> menjadi <c>62</c> (<c>KSK-DEC-018</c>).
        /// Contoh: <c>0812-3456-7890</c> dan <c>+62 812 3456 7890</c> sama-sama menjadi
        /// <c>6281234567890</c>. Aturan yang sama dipakai di sisi kolom pada
        /// <see cref="FindMatchedPatientIdsAsync"/>.
        /// </summary>
        internal static string NormalizePhoneNumber(string value)
        {
            var digits = new string(value.Where(char.IsDigit).ToArray());

            return digits.StartsWith('0') ? "62" + digits[1..] : digits;
        }

        private async Task<List<Guid>> FindMatchedPatientIdsAsync(
            KioskPatientLookupSearchType searchType,
            string value,
            CancellationToken cancellationToken)
        {
            var patients = _dbContext.Set<MstPatient>().AsNoTracking().Where(x => !x.IsDelete);

            switch (searchType)
            {
                case KioskPatientLookupSearchType.IdentityNumber:
                {
                    // Urutan sumber sama dengan KioskScanSessionController.FindPatientAsync:
                    // kolom pasien lebih dulu, lalu dokumen identitas aktif.
                    var fromPatient = patients
                        .Where(x => x.IdentityNumber != null && x.IdentityNumber.ToLower() == value)
                        .Select(x => x.Id);

                    var fromDocument = _dbContext.Set<MstPatientIdentityDocument>()
                        .AsNoTracking()
                        .Where(x => !x.IsDelete && x.IsActive && x.IdentityNumber.ToLower() == value)
                        .Select(x => x.PatientId);

                    return await fromPatient.Union(fromDocument).ToListAsync(cancellationToken);
                }

                case KioskPatientLookupSearchType.PhoneNumber:
                    // Npgsql 9 tidak menerjemahkan Regex.Replace (terbukti di runtime BE-KSK-001),
                    // sehingga normalisasi sisi kolom ditulis sebagai SQL. Nilai masukan tetap
                    // parameter lewat FromSqlInterpolated (SEC-KSK-003), bukan disambung ke teks.
                    return await _dbContext.Set<MstPatient>()
                        .FromSqlInterpolated($@"SELECT * FROM public.""MstPatient""
                            WHERE ""PhoneNumber"" IS NOT NULL
                              AND regexp_replace(regexp_replace(""PhoneNumber"", '[^0-9]', '', 'g'), '^0', '62') = {value}")
                        .AsNoTracking()
                        .Where(x => !x.IsDelete)
                        .Select(x => x.Id)
                        .ToListAsync(cancellationToken);

                case KioskPatientLookupSearchType.InsuranceCardNumber:
                    return await _dbContext.Set<MstPatientInsurance>()
                        .AsNoTracking()
                        .Where(x => !x.IsDelete && x.IsActive && x.CardNumber != null && x.CardNumber.ToLower() == value)
                        .Select(x => x.PatientId)
                        .Distinct()
                        .ToListAsync(cancellationToken);

                case KioskPatientLookupSearchType.MemberNumber:
                {
                    var fromMembership = _dbContext.Set<MstPatientMembership>()
                        .AsNoTracking()
                        .Where(x => !x.IsDelete && x.IsActive && x.MemberNumber.ToLower() == value)
                        .Select(x => x.PatientId);

                    var fromInsurance = _dbContext.Set<MstPatientInsurance>()
                        .AsNoTracking()
                        .Where(x => !x.IsDelete && x.IsActive && x.MemberNumber != null && x.MemberNumber.ToLower() == value)
                        .Select(x => x.PatientId);

                    return await fromMembership.Union(fromInsurance).ToListAsync(cancellationToken);
                }

                default:
                    return new List<Guid>();
            }
        }

        private async Task<KioskPatientLookupResponse> ResolveResponseAsync(
            KioskPatientLookupSearchType searchType,
            List<Guid> matchedPatientIds,
            CancellationToken cancellationToken)
        {
            if (matchedPatientIds.Count == 0)
            {
                return new KioskPatientLookupResponse
                {
                    Result = KioskPatientLookupResult.NotFound,
                    NextAction = NextActionNewPatient
                };
            }

            // Kunci unik per pasien akhir. Pasien yang rantai gabungnya tidak dapat diikuti tetap
            // dihitung sebagai satu kandidat tersendiri (memakai Id asalnya), supaya tidak hilang
            // diam-diam lalu membuat hasil lain terlihat tunggal.
            var resolved = new Dictionary<Guid, PatientSnapshot?>();

            foreach (var patientId in matchedPatientIds.Distinct())
            {
                var final = await FollowMergeChainAsync(patientId, cancellationToken);
                resolved.TryAdd(final?.Id ?? patientId, final);
            }

            if (resolved.Count > 1)
            {
                return new KioskPatientLookupResponse
                {
                    Result = KioskPatientLookupResult.MultipleMatch,
                    NextAction = searchType == KioskPatientLookupSearchType.PhoneNumber
                        ? NextActionUseIdentityOrContactStaff
                        : NextActionContactStaff
                };
            }

            var patient = resolved.Values.Single();

            if (patient == null || !IsEligible(patient))
            {
                return new KioskPatientLookupResponse
                {
                    Result = KioskPatientLookupResult.ContactStaff,
                    NextAction = NextActionContactStaff
                };
            }

            return new KioskPatientLookupResponse
            {
                Result = KioskPatientLookupResult.Found,
                NextAction = NextActionExistingPatient,
                Patient = new KioskPatientCardResponse
                {
                    PatientId = patient.Id,
                    MedicalRecordNumber = patient.MedicalRecordNumber,
                    PatientCode = patient.PatientCode,
                    FullName = patient.FullName,
                    PatientTypeName = BuildPatientTypeLabel(patient.PatientType),
                    GenderName = patient.Gender.HasValue ? BuildGenderLabel(patient.Gender.Value) : null,
                    BloodTypeName = BuildBloodTypeLabel(patient.BloodType)
                }
            };
        }

        /// <summary>
        /// Mengikuti <c>MergedToPatientId</c> paling banyak <see cref="MaxMergeHops"/> langkah
        /// (<c>KSK-DSN-008</c>). Mengembalikan <c>null</c> bila rantai berputar, putus, atau
        /// terlalu panjang — pemanggil memperlakukannya sebagai "hubungi petugas".
        /// </summary>
        private async Task<PatientSnapshot?> FollowMergeChainAsync(Guid patientId, CancellationToken cancellationToken)
        {
            var visited = new HashSet<Guid>();
            var currentId = patientId;

            for (var hop = 0; hop <= MaxMergeHops; hop++)
            {
                if (!visited.Add(currentId))
                {
                    return null;
                }

                var snapshot = await _dbContext.Set<MstPatient>()
                    .AsNoTracking()
                    .Where(x => x.Id == currentId && !x.IsDelete)
                    .Select(x => new PatientSnapshot(
                        x.Id,
                        x.MedicalRecordNumber,
                        x.PatientCode,
                        x.FullName,
                        x.PatientType,
                        x.Gender,
                        x.BloodType,
                        x.PatientStatus,
                        x.IsActive,
                        x.IsDeceased,
                        x.MergedToPatientId))
                    .FirstOrDefaultAsync(cancellationToken);

                if (snapshot == null)
                {
                    return null;
                }

                if (!snapshot.MergedToPatientId.HasValue)
                {
                    return snapshot;
                }

                currentId = snapshot.MergedToPatientId.Value;
            }

            return null;
        }

        private static bool IsEligible(PatientSnapshot patient) =>
            patient.PatientStatus == PatientStatus.Active &&
            patient.IsActive &&
            !patient.IsDeceased &&
            !patient.MergedToPatientId.HasValue;

        private static string MaskValue(string value) =>
            value.Length <= 4 ? "****" : "****" + value[^4..];

        // Label disalin dari PatientController.BuildEnumLabel (private) agar Kartu Pasien di Kiosk
        // sama dengan layar Cetak Kartu Pasien. Duplikasi dicatat di laporan BE-KSK-001.
        private static string BuildPatientTypeLabel(PatientType value) => value switch
        {
            PatientType.General => "Umum",
            PatientType.Mother => "Ibu",
            PatientType.Newborn => "Bayi Baru Lahir",
            PatientType.Child => "Anak",
            PatientType.Employee => "Karyawan",
            PatientType.Corporate => "Perusahaan",
            PatientType.Other => "Lainnya",
            _ => value.ToString()
        };

        private static string BuildGenderLabel(Gender value) => value switch
        {
            Gender.Male => "Laki-laki",
            Gender.Female => "Perempuan",
            _ => value.ToString()
        };

        private static string BuildBloodTypeLabel(BloodType value) => value switch
        {
            BloodType.Unknown => "Tidak Diketahui",
            BloodType.APositive => "A+",
            BloodType.ANegative => "A-",
            BloodType.BPositive => "B+",
            BloodType.BNegative => "B-",
            BloodType.ABPositive => "AB+",
            BloodType.ABNegative => "AB-",
            BloodType.OPositive => "O+",
            BloodType.ONegative => "O-",
            BloodType.NotDisclosed => "Tidak Diungkapkan",
            _ => value.ToString()
        };

        private sealed record PatientSnapshot(
            Guid Id,
            string MedicalRecordNumber,
            string PatientCode,
            string FullName,
            PatientType PatientType,
            Gender? Gender,
            BloodType BloodType,
            PatientStatus PatientStatus,
            bool IsActive,
            bool IsDeceased,
            Guid? MergedToPatientId);

        private sealed record NormalizationResult(
            KioskPatientLookupSearchType SearchType,
            string? NormalizedValue,
            string? ErrorMessage)
        {
            public static NormalizationResult Ok(KioskPatientLookupSearchType searchType, string value) =>
                new(searchType, value, null);

            public static NormalizationResult Fail(string message) =>
                new(default, null, message);
        }
    }

    public sealed record KioskPatientLookupServiceResult(
        KioskPatientLookupResponse? Data,
        string? ValidationError)
    {
        public bool IsValid => ValidationError == null;

        public static KioskPatientLookupServiceResult Success(KioskPatientLookupResponse data) => new(data, null);

        public static KioskPatientLookupServiceResult Invalid(string message) => new(null, message);
    }
}
