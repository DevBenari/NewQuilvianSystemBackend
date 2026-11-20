using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Notification;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>Hasil satu permintaan halaman publik Pendaftaran Pasien Online.</summary>
    public enum OnlineRegistrationPublicOutcome
    {
        Ok = 1,

        /// <summary>Isian tidak sah atau captcha ditolak (<c>400</c>).</summary>
        Invalid = 2,

        /// <summary>Data pasien tidak cocok (<c>404</c>, pesan seragam).</summary>
        NotFound = 3,

        /// <summary>Captcha tidak dapat diperiksa atau fitur dimatikan (<c>503</c>).</summary>
        Unavailable = 4,

        /// <summary>Kuota habis, jadwal tidak tersedia, atau sudah ada pendaftaran/kunjungan (<c>409</c>).</summary>
        Conflict = 5
    }

    public sealed record OnlineRegistrationPublicResult<T>(OnlineRegistrationPublicOutcome Outcome, T? Data, string Message)
    {
        public static OnlineRegistrationPublicResult<T> Success(T data, string message) =>
            new(OnlineRegistrationPublicOutcome.Ok, data, message);

        public static OnlineRegistrationPublicResult<T> Fail(OnlineRegistrationPublicOutcome outcome, string message) =>
            new(outcome, default, message);
    }

    /// <summary>
    /// Opsi, pencocokan pasien, dan pengiriman pendaftaran untuk halaman publik Pendaftaran Pasien
    /// Online (kiosk Amendment PO, <c>BE-KSK-PO-004</c>, <c>BE-KSK-PO-005</c>). Tanpa sesi
    /// (<c>KSK-DSN-PO-005</c>): setiap permintaan yang butuh konteks pasien membawa ulang pasangan
    /// identitas dan tanggal lahir. Pengiriman ada di <c>OnlineRegistrationPublicService.Submit.cs</c>.
    /// </summary>
    /// <remarks>
    /// Data yang keluar dibatasi <c>KSK-PO-FE-002</c>: nama dan nomor kartu tersamar saja. NIK, No. RM,
    /// alamat, dan data pasien lain tidak pernah dikembalikan, dan tidak dicatat di log.
    /// </remarks>
    public partial class OnlineRegistrationPublicService
    {
        public const string PatientNotFoundMessage =
            "Data tidak ditemukan. Periksa kembali isian Anda atau daftar di loket pendaftaran.";

        public const string CaptchaRejectedMessage = "Verifikasi keamanan gagal. Silakan ulangi.";

        public const string ServiceUnavailableMessage =
            "Layanan sedang tidak tersedia. Silakan coba lagi atau daftar di loket pendaftaran.";

        public const string VisitDateOutOfRangeMessage =
            "Pendaftaran online hanya untuk besok sampai 30 hari ke depan. Untuk berobat hari ini, silakan daftar di kiosk rumah sakit.";

        private const int MinimumSearchLength = 3;
        private const int MaxReferralInstitutionResults = 20;
        private const int MaxMedicalRecordDigits = 20;

        private static readonly Regex IdentityNumberPattern = new("^[0-9]{16}$", RegexOptions.Compiled);
        private static readonly Regex DigitsOnlyPattern = new("^[0-9]+$", RegexOptions.Compiled);

        private readonly ApplicationDbContext _dbContext;
        private readonly DoctorScheduleQuotaService _quotaService;
        private readonly KioskPatientLookupService _patientLookupService;
        private readonly TurnstileVerificationService _turnstileService;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly ReferralDocumentStorageService _referralDocumentStorage;
        private readonly SmtpEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly OnlineRegistrationOptions _options;
        private readonly ILogger<OnlineRegistrationPublicService> _logger;

        public OnlineRegistrationPublicService(
            ApplicationDbContext dbContext,
            DoctorScheduleQuotaService quotaService,
            KioskPatientLookupService patientLookupService,
            TurnstileVerificationService turnstileService,
            NumberSeriesAllocator numberSeriesAllocator,
            ReferralDocumentStorageService referralDocumentStorage,
            SmtpEmailSender emailSender,
            IConfiguration configuration,
            IOptions<OnlineRegistrationOptions> options,
            ILogger<OnlineRegistrationPublicService> logger)
        {
            _dbContext = dbContext;
            _quotaService = quotaService;
            _patientLookupService = patientLookupService;
            _turnstileService = turnstileService;
            _numberSeriesAllocator = numberSeriesAllocator;
            _referralDocumentStorage = referralDocumentStorage;
            _emailSender = emailSender;
            _configuration = configuration;
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>
        /// Tanggal kunjungan sah bila H+<c>MinDaysAhead</c> s.d. H+<c>MaxDaysAhead</c> dari hari ini WIB
        /// (<c>KSK-DEC-047</c>, <c>KSK-VAL-PO-020</c>). Dipakai juga pengiriman pendaftaran.
        /// </summary>
        public bool IsVisitDateAllowed(DateTime visitDate)
        {
            var today = AppDateTimeHelper.OperationalDate();
            var date = visitDate.Date;
            return date >= today.AddDays(Math.Max(1, _options.MinDaysAhead)) &&
                   date <= today.AddDays(Math.Max(1, _options.MaxDaysAhead));
        }

        public async Task<OnlineRegistrationPublicResult<List<OnlineRegistrationClinicOption>>> GetClinicOptionsAsync(
            DateTime? visitDate,
            CancellationToken cancellationToken = default)
        {
            var guard = GuardVisitDate<List<OnlineRegistrationClinicOption>>(visitDate);
            if (guard != null)
                return guard;

            var schedules = await LoadOfferedSchedulesAsync(null, visitDate!.Value, cancellationToken);

            var clinics = schedules
                .Where(x => x.Clinic != null)
                .GroupBy(x => x.ClinicId)
                .Select(g => new OnlineRegistrationClinicOption
                {
                    Id = g.Key,
                    Name = g.First().Clinic!.ClinicName
                })
                .OrderBy(x => x.Name)
                .ToList();

            return OnlineRegistrationPublicResult<List<OnlineRegistrationClinicOption>>.Success(
                clinics,
                "Daftar poli berhasil diambil.");
        }

        public async Task<OnlineRegistrationPublicResult<List<OnlineRegistrationScheduleOption>>> GetScheduleOptionsAsync(
            Guid? clinicId,
            DateTime? visitDate,
            CancellationToken cancellationToken = default)
        {
            var guard = GuardVisitDate<List<OnlineRegistrationScheduleOption>>(visitDate);
            if (guard != null)
                return guard;

            if (!clinicId.HasValue || clinicId.Value == Guid.Empty)
            {
                return OnlineRegistrationPublicResult<List<OnlineRegistrationScheduleOption>>.Fail(
                    OnlineRegistrationPublicOutcome.Invalid,
                    "Pilih poli terlebih dahulu.");
            }

            var schedules = await LoadOfferedSchedulesAsync(clinicId.Value, visitDate!.Value, cancellationToken);
            var usages = await _quotaService.GetUsagesAsync(
                schedules.Select(x => x.Id).ToList(),
                DateTime.SpecifyKind(visitDate.Value.Date, DateTimeKind.Utc),
                cancellationToken);

            var options = schedules
                .OrderBy(x => x.StartTime)
                .ThenBy(x => x.Doctor!.FullName)
                .Select(schedule =>
                {
                    var usage = usages.TryGetValue(schedule.Id, out var found) ? found : default;
                    return new OnlineRegistrationScheduleOption
                    {
                        Id = schedule.Id,
                        DoctorName = schedule.Doctor!.FullName,
                        StartTime = schedule.StartTime.ToString(@"hh\:mm"),
                        EndTime = schedule.EndTime.ToString(@"hh\:mm"),
                        SessionName = schedule.SessionName,
                        RemainingAppointmentQuota = ResolveRemainingAppointmentQuota(schedule, usage),
                        IsFull = DoctorScheduleQuotaService.IsFullForAppointment(schedule, usage)
                    };
                })
                .ToList();

            return OnlineRegistrationPublicResult<List<OnlineRegistrationScheduleOption>>.Success(
                options,
                "Daftar jadwal dokter berhasil diambil.");
        }

        public async Task<OnlineRegistrationPublicResult<List<OnlineRegistrationInsuranceProviderOption>>> GetInsuranceProviderOptionsAsync(
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                return Disabled<List<OnlineRegistrationInsuranceProviderOption>>();

            var providers = await QueryActiveInsuranceProviders()
                .OrderBy(x => x.InsuranceProviderName)
                .Select(x => new OnlineRegistrationInsuranceProviderOption
                {
                    Id = x.Id,
                    Name = x.InsuranceProviderName
                })
                .ToListAsync(cancellationToken);

            return OnlineRegistrationPublicResult<List<OnlineRegistrationInsuranceProviderOption>>.Success(
                providers,
                "Daftar penyedia asuransi berhasil diambil.");
        }

        public async Task<OnlineRegistrationPublicResult<List<OnlineRegistrationReferralInstitutionOption>>> GetReferralInstitutionOptionsAsync(
            string? search,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                return Disabled<List<OnlineRegistrationReferralInstitutionOption>>();

            var keyword = search?.Trim() ?? string.Empty;

            if (keyword.Length < MinimumSearchLength || keyword.Length > 100)
            {
                return OnlineRegistrationPublicResult<List<OnlineRegistrationReferralInstitutionOption>>.Fail(
                    OnlineRegistrationPublicOutcome.Invalid,
                    "Ketik minimal 3 huruf nama atau kode faskes.");
            }

            var pattern = "%" + keyword.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";

            var institutions = await _dbContext.Set<MstReferralInstitution>()
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    (EF.Functions.ILike(x.InstitutionName, pattern, "\\") ||
                     EF.Functions.ILike(x.InstitutionCode, pattern, "\\")))
                .OrderBy(x => x.InstitutionName)
                .Take(MaxReferralInstitutionResults)
                .Select(x => new OnlineRegistrationReferralInstitutionOption
                {
                    Id = x.Id,
                    Code = x.InstitutionCode,
                    Name = x.InstitutionName
                })
                .ToListAsync(cancellationToken);

            return OnlineRegistrationPublicResult<List<OnlineRegistrationReferralInstitutionOption>>.Success(
                institutions,
                "Daftar faskes perujuk berhasil diambil.");
        }

        /// <summary>
        /// Pencocokan pasien lama (<c>KSK-DEC-031</c>): cocok atau tidak, tanpa daftar hasil. Bila cocok,
        /// hanya nama tersamar dan kartu penjamin tersamar yang keluar.
        /// </summary>
        public async Task<OnlineRegistrationPublicResult<OnlineRegistrationPatientMatchResponse>> MatchPatientAsync(
            OnlineRegistrationPatientMatchRequest? request,
            string? remoteIp,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                return Disabled<OnlineRegistrationPatientMatchResponse>();

            var validationError = ValidateIdentity(request?.IdentityType, request?.IdentityValue, request?.BirthDate);
            if (validationError != null)
            {
                return OnlineRegistrationPublicResult<OnlineRegistrationPatientMatchResponse>.Fail(
                    OnlineRegistrationPublicOutcome.Invalid,
                    validationError);
            }

            var captcha = await _turnstileService.VerifyAsync(request!.TurnstileToken, remoteIp, cancellationToken);
            if (captcha == TurnstileVerificationOutcome.Rejected)
            {
                return OnlineRegistrationPublicResult<OnlineRegistrationPatientMatchResponse>.Fail(
                    OnlineRegistrationPublicOutcome.Invalid,
                    CaptchaRejectedMessage);
            }

            if (captcha == TurnstileVerificationOutcome.Unavailable)
            {
                return OnlineRegistrationPublicResult<OnlineRegistrationPatientMatchResponse>.Fail(
                    OnlineRegistrationPublicOutcome.Unavailable,
                    ServiceUnavailableMessage);
            }

            var patientId = await FindExistingPatientIdAsync(
                request.IdentityType!.Value,
                request.IdentityValue!,
                request.BirthDate!.Value,
                cancellationToken);

            if (patientId == null)
            {
                _logger.LogInformation("Pencocokan pasien halaman publik: tidak cocok. IdentityType={IdentityType}", request.IdentityType);
                return OnlineRegistrationPublicResult<OnlineRegistrationPatientMatchResponse>.Fail(
                    OnlineRegistrationPublicOutcome.NotFound,
                    PatientNotFoundMessage);
            }

            var fullName = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => x.Id == patientId.Value)
                .Select(x => x.FullName)
                .FirstAsync(cancellationToken);

            var response = new OnlineRegistrationPatientMatchResponse
            {
                MaskedName = PatientNameMasker.Mask(fullName),
                Payers = await LoadMaskedPayersAsync(patientId.Value, cancellationToken)
            };

            _logger.LogInformation("Pencocokan pasien halaman publik: cocok. IdentityType={IdentityType}", request.IdentityType);

            return OnlineRegistrationPublicResult<OnlineRegistrationPatientMatchResponse>.Success(
                response,
                "Data pasien ditemukan.");
        }

        /// <summary>
        /// Id pasien lama yang cocok dengan NIK/No. RM dan tanggal lahir, atau <c>null</c>. Pasien
        /// gabungan tanpa tujuan, non-aktif, meninggal, lebih dari satu kandidat, maupun tanggal lahir
        /// berbeda semuanya menghasilkan <c>null</c>, supaya pesan ke publik selalu sama. Dipakai ulang
        /// pengiriman pendaftaran (<c>BE-KSK-PO-005</c>) untuk mencocokkan ulang tanpa sesi.
        /// </summary>
        public async Task<Guid?> FindExistingPatientIdAsync(
            OnlineRegistrationIdentityType identityType,
            string identityValue,
            DateTime birthDate,
            CancellationToken cancellationToken = default)
        {
            List<Guid> candidates;

            if (identityType == OnlineRegistrationIdentityType.IdentityNumber)
            {
                candidates = await _patientLookupService.FindPatientIdsByIdentityNumberAsync(
                    identityValue.Trim(),
                    cancellationToken);
            }
            else
            {
                var digits = NormalizeMedicalRecordNumber(identityValue);
                candidates = await _dbContext.Set<MstPatient>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.MedicalRecordNumber.Replace("-", "") == digits)
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);
            }

            var patientId = await _patientLookupService.ResolveSingleEligiblePatientIdAsync(candidates, cancellationToken);
            if (patientId == null)
                return null;

            var storedBirthDate = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => x.Id == patientId.Value)
                .Select(x => x.BirthDate)
                .FirstOrDefaultAsync(cancellationToken);

            return storedBirthDate.HasValue && storedBirthDate.Value.Date == birthDate.Date
                ? patientId
                : null;
        }

        /// <summary>Validasi bentuk identitas dan tanggal lahir (<c>KSK-VAL-PO-001</c> s.d. <c>003</c>).</summary>
        public static string? ValidateIdentity(
            OnlineRegistrationIdentityType? identityType,
            string? identityValue,
            DateTime? birthDate)
        {
            if (identityType == null || !Enum.IsDefined(identityType.Value))
                return "Pilih NIK atau No. Rekam Medis.";

            var value = identityValue?.Trim() ?? string.Empty;

            if (identityType == OnlineRegistrationIdentityType.IdentityNumber && !IdentityNumberPattern.IsMatch(value))
                return "NIK harus terdiri dari 16 digit angka.";

            if (identityType == OnlineRegistrationIdentityType.MedicalRecordNumber)
            {
                var digits = NormalizeMedicalRecordNumber(value);
                if (digits.Length == 0 || digits.Length > MaxMedicalRecordDigits || !DigitsOnlyPattern.IsMatch(digits))
                    return "No. Rekam Medis hanya berisi angka.";
            }

            if (!birthDate.HasValue)
                return "Tanggal lahir tidak valid.";

            var today = AppDateTimeHelper.OperationalDate();
            if (birthDate.Value.Date > today || birthDate.Value.Date < today.AddYears(-150))
                return "Tanggal lahir tidak valid.";

            return null;
        }

        private static string NormalizeMedicalRecordNumber(string value) =>
            new string(value.Where(ch => !char.IsWhiteSpace(ch) && ch != '-' && ch != '.').ToArray());

        /// <summary>
        /// Jadwal yang ditawarkan halaman publik: aktif, berstatus Aktif, mengizinkan appointment
        /// <b>dan</b> pendaftaran kiosk (<c>KSK-DSN-PO-013</c>), dengan dokter, unit, dan poli aktif, serta
        /// berlaku pada tanggal kunjungan menurut aturan yang sama dengan jalur kunjungan.
        /// </summary>
        internal async Task<List<MstDoctorSchedule>> LoadOfferedSchedulesAsync(
            Guid? clinicId,
            DateTime visitDate,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .Include(x => x.Doctor)
                .Include(x => x.Clinic)
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    x.ScheduleStatus == DoctorScheduleStatus.Active &&
                    x.IsAllowAppointment &&
                    x.IsAllowKioskRegistration &&
                    x.Doctor != null && x.Doctor.IsActive && !x.Doctor.IsDelete &&
                    x.ServiceUnit != null && x.ServiceUnit.IsActive && !x.ServiceUnit.IsDelete &&
                    x.Clinic != null && x.Clinic.IsActive && !x.Clinic.IsDelete);

            if (clinicId.HasValue)
                query = query.Where(x => x.ClinicId == clinicId.Value);

            var candidates = await query.ToListAsync(cancellationToken);

            return candidates
                .Where(x => DoctorScheduleQuotaService.IsScheduleValidForVisitDate(x, visitDate))
                .ToList();
        }

        /// <summary>
        /// Penyedia asuransi yang boleh dipilih untuk kartu baru: aktif dan kontraknya berlaku hari ini.
        /// Dipakai opsi publik dan pemeriksaan ulang saat kirim.
        /// </summary>
        private IQueryable<MstInsuranceProvider> QueryActiveInsuranceProviders()
        {
            var today = AppDateTimeHelper.OperationalDate();

            return _dbContext.Set<MstInsuranceProvider>()
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    (!x.ContractStartDate.HasValue || x.ContractStartDate.Value.Date <= today) &&
                    (!x.ContractEndDate.HasValue || x.ContractEndDate.Value.Date >= today));
        }

        private static int? ResolveRemainingAppointmentQuota(MstDoctorSchedule schedule, DoctorScheduleQuotaUsage usage)
        {
            int? remaining = null;

            if (schedule.MaxAppointmentQuota > 0)
                remaining = schedule.MaxAppointmentQuota - usage.Appointment;

            if (schedule.MaxPatientQuota > 0)
            {
                var byTotal = schedule.MaxPatientQuota - usage.Total;
                remaining = remaining.HasValue ? Math.Min(remaining.Value, byTotal) : byTotal;
            }

            return remaining.HasValue ? Math.Max(0, remaining.Value) : null;
        }

        private async Task<List<OnlineRegistrationPayerOption>> LoadMaskedPayersAsync(
            Guid patientId,
            CancellationToken cancellationToken)
        {
            var today = AppDateTimeHelper.OperationalDate();

            var insurances = await _dbContext.Set<MstPatientInsurance>()
                .AsNoTracking()
                .Where(x =>
                    x.PatientId == patientId &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.IsEligible &&
                    x.InsuranceProvider != null &&
                    x.InsuranceProvider.IsActive &&
                    !x.InsuranceProvider.IsDelete &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today))
                .OrderByDescending(x => x.IsPrimary)
                .Select(x => new
                {
                    x.Id,
                    ProviderName = x.InsuranceProvider!.InsuranceProviderName,
                    Number = x.CardNumber ?? x.PolicyNumber
                })
                .ToListAsync(cancellationToken);

            var companies = await _dbContext.Set<MstPatientCompanyGuarantor>()
                .AsNoTracking()
                .Where(x =>
                    x.PatientId == patientId &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.IsEligible &&
                    x.CompanyGuarantor != null &&
                    x.CompanyGuarantor.IsActive &&
                    !x.CompanyGuarantor.IsDelete &&
                    !x.CompanyGuarantor.IsEmployeeBenefitGuarantor &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today))
                .OrderByDescending(x => x.IsPrimary)
                .Select(x => new
                {
                    x.Id,
                    ProviderName = x.CompanyGuarantor!.CompanyGuarantorName,
                    Number = x.EmployeeNumber
                })
                .ToListAsync(cancellationToken);

            return insurances
                .Select(x => new OnlineRegistrationPayerOption
                {
                    Id = x.Id,
                    PayerKind = OnlineRegistrationPayerKind.Insurance,
                    ProviderName = x.ProviderName,
                    MaskedNumber = MaskNumber(x.Number)
                })
                .Concat(companies.Select(x => new OnlineRegistrationPayerOption
                {
                    Id = x.Id,
                    PayerKind = OnlineRegistrationPayerKind.Company,
                    ProviderName = x.ProviderName,
                    MaskedNumber = MaskNumber(x.Number)
                }))
                .ToList();
        }

        internal static string MaskNumber(string? value)
        {
            var trimmed = value?.Trim() ?? string.Empty;
            return trimmed.Length <= 4 ? "****" : "****" + trimmed[^4..];
        }

        private OnlineRegistrationPublicResult<T>? GuardVisitDate<T>(DateTime? visitDate)
        {
            if (!_options.Enabled)
                return Disabled<T>();

            if (!visitDate.HasValue || !IsVisitDateAllowed(visitDate.Value))
                return OnlineRegistrationPublicResult<T>.Fail(OnlineRegistrationPublicOutcome.Invalid, VisitDateOutOfRangeMessage);

            return null;
        }

        private static OnlineRegistrationPublicResult<T> Disabled<T>() =>
            OnlineRegistrationPublicResult<T>.Fail(
                OnlineRegistrationPublicOutcome.Unavailable,
                "Pendaftaran online sedang tidak tersedia. Silakan daftar di loket pendaftaran.");
    }
}
