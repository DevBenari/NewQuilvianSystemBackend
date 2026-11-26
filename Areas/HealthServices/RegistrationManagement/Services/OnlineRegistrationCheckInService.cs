using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Check-in pendaftaran online di kiosk (kiosk Amendment PO, <c>KSK-DSN-PO-008</c>). Berkas ini
    /// (<c>BE-KSK-PO-006</c>) memuat langkah pertama: verifikasi dan penerbitan token check-in.
    /// Pembuatan kunjungan dari token itu ada di <c>OnlineRegistrationCheckInService.Encounter.cs</c>
    /// (<c>BE-KSK-PO-007</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Dua cara menemukan pendaftaran.</b> Kode + tanggal lahir (<c>KSK-DEC-033</c>), atau Lupa Kode:
    /// NIK/No. RM + tanggal lahir (<c>KSK-DEC-040</c>). Pasien baru belum punya No. RM, jadi Lupa
    /// Kode-nya memakai NIK yang diisi online.
    /// </para>
    /// <para>
    /// <b>Satu pesan untuk semua yang tidak cocok.</b> Kode salah, tanggal lahir salah, pendaftaran
    /// sudah berstatus akhir, atau tanggal kunjungan sudah lewat dijawab <c>404</c> dengan pesan yang
    /// sama (<c>KSK-VAL-PO-041</c>). Tanggal kunjungan baru disebut (<c>409</c>,
    /// <c>KSK-VAL-PO-042</c>) sesudah pemohon membuktikan kode/identitas <b>dan</b> tanggal lahir.
    /// </para>
    /// <para>
    /// <b>Token.</b> 256 bit acak, base64url. Hanya hash SHA-256-nya yang disimpan, berlaku
    /// <c>CheckInTokenMinutes</c> (bawaan 15 menit). Verifikasi ulang menimpa token sebelumnya.
    /// Verifikasi tidak mengubah status maupun <c>RowVersion</c>, sehingga perubahan petugas yang
    /// sedang berjalan tidak ikut gagal karenanya.
    /// </para>
    /// <para>
    /// Log hanya memuat id pendaftaran dan kode aturan penolakan; kode, NIK, No. RM, dan tanggal
    /// lahir tidak pernah dicatat.
    /// </para>
    /// </remarks>
    public partial class OnlineRegistrationCheckInService
    {
        public const string IncompleteMessage = "Isian tidak lengkap. Periksa kembali isian Anda.";
        public const string CodeFormatMessage = "Kode terdiri dari 8 huruf dan angka, contoh K7QM-4TRZ.";
        public const string BirthDateInvalidMessage = "Tanggal lahir tidak valid.";
        public const string NotFoundMessage =
            "Data tidak sesuai atau pendaftaran sudah tidak berlaku. Periksa kembali atau hubungi petugas.";

        private const int TokenBytes = 32;
        private const int DefaultTokenMinutes = 15;

        private static readonly string[] IndonesianShortMonthNames =
            { "Jan", "Feb", "Mar", "Apr", "Mei", "Jun", "Jul", "Agu", "Sep", "Okt", "Nov", "Des" };

        private readonly ApplicationDbContext _dbContext;
        private readonly OnlineRegistrationPublicService _publicService;
        private readonly OnlineRegistrationOptions _options;
        private readonly ILogger<OnlineRegistrationCheckInService> _logger;
        private readonly TimeProvider _timeProvider;
        private readonly TimeZoneInfo _businessTimeZone;

        public OnlineRegistrationCheckInService(
            ApplicationDbContext dbContext,
            OnlineRegistrationPublicService publicService,
            IOptions<OnlineRegistrationOptions> options,
            ILogger<OnlineRegistrationCheckInService> logger,
            TimeProvider? timeProvider = null)
        {
            _dbContext = dbContext;
            _publicService = publicService;
            _options = options.Value;
            _logger = logger;
            _timeProvider = timeProvider ?? TimeProvider.System;
            _businessTimeZone = ResolveBusinessTimeZone();
        }

        /// <summary>SHA-256 hex huruf kecil dari token check-in. Dipakai juga <c>BE-KSK-PO-007</c>.</summary>
        public static string ComputeTokenHash(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token.Trim()))).ToLowerInvariant();

        /// <summary>
        /// Memeriksa kode atau identitas + tanggal lahir, lalu menerbitkan token check-in dan paket
        /// check-in untuk pendaftaran Menunggu Check-in bertanggal hari ini (WIB).
        /// </summary>
        public async Task<OnlineRegistrationPublicResult<OnlineRegistrationCheckInPackage>> VerifyAsync(
            OnlineRegistrationCheckInVerifyRequest? request,
            CancellationToken cancellationToken = default)
        {
            var validation = Validate(request);
            if (validation != null)
                return Reject(OnlineRegistrationPublicOutcome.Invalid, validation.Value.Message, validation.Value.Rule);

            var now = _timeProvider.GetUtcNow();
            var today = DateTime.SpecifyKind(TimeZoneInfo.ConvertTime(now, _businessTimeZone).Date, DateTimeKind.Unspecified);
            var birthDate = request!.BirthDate!.Value.Date;

            var candidates = request.Mode == OnlineRegistrationCheckInMode.Code
                ? await FindByCodeAsync(request.Code!, birthDate, cancellationToken)
                : await FindByIdentityAsync(request.IdentityType!.Value, request.IdentityValue!, birthDate, cancellationToken);

            // Tanggal yang sudah lewat = pendaftaran tidak berlaku lagi (akan ditutup Tidak Datang).
            var usable = candidates.Where(x => x.VisitDate >= today).OrderBy(x => x.VisitDate).ThenBy(x => x.SubmittedAt).ToList();

            if (usable.Count == 0)
                return Reject(OnlineRegistrationPublicOutcome.NotFound, NotFoundMessage, "KSK-VAL-PO-041");

            var target = usable.FirstOrDefault(x => x.VisitDate == today);
            if (target == null)
            {
                return Reject(
                    OnlineRegistrationPublicOutcome.Conflict,
                    $"Check-in hanya dapat dilakukan pada {FormatShortDate(usable[0].VisitDate)}.",
                    "KSK-VAL-PO-042");
            }

            var token = GenerateToken();
            var expiresAt = now.UtcDateTime.AddMinutes(_options.CheckInTokenMinutes > 0 ? _options.CheckInTokenMinutes : DefaultTokenMinutes);

            var registration = await _dbContext.RegOnlineRegistrations
                .FirstOrDefaultAsync(
                    x => x.Id == target.Id && !x.IsDelete && x.Status == OnlineRegistrationStatus.AwaitingCheckIn,
                    cancellationToken);

            if (registration == null)
                return Reject(OnlineRegistrationPublicOutcome.NotFound, NotFoundMessage, "KSK-VAL-PO-041");

            registration.CheckInTokenHash = ComputeTokenHash(token);
            registration.CheckInTokenExpiresAt = expiresAt;

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Petugas atau penutupan harian mengubah pendaftaran pada saat yang sama.
                _dbContext.ChangeTracker.Clear();
                return Reject(OnlineRegistrationPublicOutcome.NotFound, NotFoundMessage, "CONCURRENCY");
            }

            var package = await BuildPackageAsync(registration.Id, token, expiresAt, cancellationToken);

            _logger.LogInformation(
                "Verifikasi check-in pendaftaran online berhasil. RegistrationId={RegistrationId} Mode={Mode}",
                registration.Id,
                request.Mode);

            return OnlineRegistrationPublicResult<OnlineRegistrationCheckInPackage>.Success(
                package,
                "Pendaftaran online ditemukan. Silakan lanjutkan check-in.");
        }

        // ------------------------------------------------------------------
        // Validasi dan pencarian
        // ------------------------------------------------------------------

        private readonly record struct VerifyError(string Rule, string Message);

        private sealed record Candidate(Guid Id, DateTime VisitDate, DateTime SubmittedAt);

        private VerifyError? Validate(OnlineRegistrationCheckInVerifyRequest? request)
        {
            if (request?.Mode == null || !Enum.IsDefined(request.Mode.Value))
                return new VerifyError("MODE", IncompleteMessage);

            if (request.Mode == OnlineRegistrationCheckInMode.Code)
            {
                var code = OnlineRegistrationPublicService.NormalizeCode(request.Code);
                if (code.Length != OnlineRegistrationPublicService.CodeLength ||
                    code.Any(ch => !OnlineRegistrationPublicService.CodeAlphabet.Contains(ch)))
                {
                    return new VerifyError("KSK-VAL-PO-040", CodeFormatMessage);
                }

                if (!IsBirthDateValid(request.BirthDate))
                    return new VerifyError("KSK-VAL-PO-003", BirthDateInvalidMessage);

                return null;
            }

            // Lupa Kode: aturan bentuk NIK/No. RM dan tanggal lahir sama dengan halaman publik (KSK-VAL-PO-043).
            var identityError = OnlineRegistrationPublicService.ValidateIdentity(
                request.IdentityType,
                request.IdentityValue,
                request.BirthDate);

            return identityError != null ? new VerifyError("KSK-VAL-PO-043", identityError) : null;
        }

        private bool IsBirthDateValid(DateTime? birthDate)
        {
            if (!birthDate.HasValue)
                return false;

            var today = TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), _businessTimeZone).Date;
            return birthDate.Value.Date <= today && birthDate.Value.Date >= today.AddYears(-150);
        }

        /// <summary>
        /// Kode → satu pendaftaran Menunggu Check-in (index unik bersyarat), lalu tanggal lahir
        /// dicocokkan: dari master untuk pasien lama, dari isian online untuk pasien baru.
        /// </summary>
        private async Task<List<Candidate>> FindByCodeAsync(string code, DateTime birthDate, CancellationToken cancellationToken)
        {
            var hash = OnlineRegistrationPublicService.ComputeCodeHash(OnlineRegistrationPublicService.NormalizeCode(code));

            var match = await _dbContext.RegOnlineRegistrations
                .AsNoTracking()
                .Where(x => x.CodeHash == hash && !x.IsDelete && x.Status == OnlineRegistrationStatus.AwaitingCheckIn)
                .Select(x => new
                {
                    x.Id,
                    x.VisitDate,
                    x.SubmittedAt,
                    x.PatientMode,
                    RegistrationBirthDate = x.BirthDate,
                    PatientBirthDate = x.Patient != null ? x.Patient.BirthDate : null
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (match == null)
                return new List<Candidate>();

            var storedBirthDate = match.PatientMode == OnlineRegistrationPatientMode.New
                ? match.RegistrationBirthDate
                : match.PatientBirthDate;

            return storedBirthDate.HasValue && storedBirthDate.Value.Date == birthDate
                ? new List<Candidate> { new(match.Id, match.VisitDate.Date, match.SubmittedAt) }
                : new List<Candidate>();
        }

        /// <summary>
        /// Lupa Kode: pasien lama dikenali lewat aturan pencocokan yang sama dengan halaman publik
        /// (<c>FindExistingPatientIdAsync</c>, termasuk tanggal lahir); pasien baru lewat NIK dan tanggal
        /// lahir yang diisi online.
        /// </summary>
        private async Task<List<Candidate>> FindByIdentityAsync(
            OnlineRegistrationIdentityType identityType,
            string identityValue,
            DateTime birthDate,
            CancellationToken cancellationToken)
        {
            var patientId = await _publicService.FindExistingPatientIdAsync(identityType, identityValue, birthDate, cancellationToken);
            var nik = identityType == OnlineRegistrationIdentityType.IdentityNumber ? identityValue.Trim() : null;
            var calendarBirthDate = DateTime.SpecifyKind(birthDate, DateTimeKind.Unspecified);

            if (patientId == null && nik == null)
                return new List<Candidate>();

            var rows = await _dbContext.RegOnlineRegistrations
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.Status == OnlineRegistrationStatus.AwaitingCheckIn)
                .Where(x =>
                    (patientId != null && x.PatientId == patientId) ||
                    (nik != null &&
                     x.PatientMode == OnlineRegistrationPatientMode.New &&
                     x.IdentityNumber == nik &&
                     x.BirthDate == calendarBirthDate))
                .Select(x => new { x.Id, x.VisitDate, x.SubmittedAt })
                .ToListAsync(cancellationToken);

            return rows.Select(x => new Candidate(x.Id, x.VisitDate.Date, x.SubmittedAt)).ToList();
        }

        // ------------------------------------------------------------------
        // Paket check-in
        // ------------------------------------------------------------------

        private async Task<OnlineRegistrationCheckInPackage> BuildPackageAsync(
            Guid registrationId,
            string token,
            DateTime expiresAt,
            CancellationToken cancellationToken)
        {
            var data = await _dbContext.RegOnlineRegistrations
                .AsNoTracking()
                .Where(x => x.Id == registrationId)
                .Select(x => new
                {
                    Registration = x,
                    PatientName = x.Patient != null ? x.Patient.FullName : null,
                    Schedule = x.DoctorSchedule,
                    ServiceUnitName = x.DoctorSchedule != null && x.DoctorSchedule.ServiceUnit != null ? x.DoctorSchedule.ServiceUnit.ServiceUnitName : null,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                    DoctorName = x.DoctorSchedule != null && x.DoctorSchedule.Doctor != null ? x.DoctorSchedule.Doctor.FullName : null,
                    InsuranceProviderName = x.PatientInsurance != null && x.PatientInsurance.InsuranceProvider != null
                        ? x.PatientInsurance.InsuranceProvider.InsuranceProviderName
                        : null,
                    InsuranceNumber = x.PatientInsurance != null ? x.PatientInsurance.CardNumber ?? x.PatientInsurance.PolicyNumber : null,
                    CompanyName = x.PatientCompanyGuarantor != null && x.PatientCompanyGuarantor.CompanyGuarantor != null
                        ? x.PatientCompanyGuarantor.CompanyGuarantor.CompanyGuarantorName
                        : null,
                    CompanyNumber = x.PatientCompanyGuarantor != null ? x.PatientCompanyGuarantor.EmployeeNumber : null,
                    NewProviderName = x.NewInsuranceProvider != null ? x.NewInsuranceProvider.InsuranceProviderName : null,
                    InstitutionName = x.ReferralInstitution != null ? x.ReferralInstitution.InstitutionName : null,
                    DocumentCount = x.ReferralDocuments.Count(d => !d.IsDelete)
                })
                .FirstAsync(cancellationToken);

            var r = data.Registration;
            var schedule = data.Schedule!;
            var isNew = r.PatientMode == OnlineRegistrationPatientMode.New;

            return new OnlineRegistrationCheckInPackage
            {
                OnlineRegistrationId = r.Id,
                RegistrationNumber = r.RegistrationNumber,
                CheckInToken = token,
                ExpiresAt = DateTime.SpecifyKind(expiresAt, DateTimeKind.Utc),
                PatientMode = r.PatientMode,
                PatientId = isNew ? null : r.PatientId,
                MaskedName = PatientNameMasker.Mask(isNew ? r.FullName : data.PatientName),
                IdentityNumberForScan = isNew ? r.IdentityNumber : null,
                ContactWhatsAppNumber = isNew ? r.ContactWhatsAppNumber : null,
                ContactEmail = isNew ? r.ContactEmail : null,
                Visit = new OnlineRegistrationCheckInVisit
                {
                    VisitDate = r.VisitDate.ToString("yyyy-MM-dd"),
                    ServiceUnitId = schedule.ServiceUnitId,
                    ServiceUnitName = data.ServiceUnitName ?? string.Empty,
                    ClinicId = r.ClinicId,
                    ClinicName = data.ClinicName ?? string.Empty,
                    DoctorId = schedule.DoctorId,
                    DoctorName = data.DoctorName ?? string.Empty,
                    DoctorScheduleId = r.DoctorScheduleId,
                    RoomId = schedule.RoomId,
                    StartTime = schedule.StartTime.ToString(@"hh\:mm"),
                    EndTime = schedule.EndTime.ToString(@"hh\:mm"),
                    SessionName = schedule.SessionName
                },
                Payer = new OnlineRegistrationCheckInPayer
                {
                    PatientType = r.PatientType,
                    PaymentType = r.PaymentType,
                    PatientInsuranceId = r.PatientInsuranceId,
                    PatientCompanyGuarantorId = r.PatientCompanyGuarantorId,
                    PayerName = r.PatientInsuranceId.HasValue ? data.InsuranceProviderName
                        : r.PatientCompanyGuarantorId.HasValue ? data.CompanyName
                        : null,
                    MaskedNumber = r.PatientInsuranceId.HasValue ? OnlineRegistrationPublicService.MaskNumber(data.InsuranceNumber)
                        : r.PatientCompanyGuarantorId.HasValue ? OnlineRegistrationPublicService.MaskNumber(data.CompanyNumber)
                        : null,
                    NewInsurance = r.NewInsuranceProviderId.HasValue
                        ? new OnlineRegistrationCheckInNewInsurance
                        {
                            InsuranceProviderId = r.NewInsuranceProviderId.Value,
                            ProviderName = data.NewProviderName ?? string.Empty,
                            PolicyNumber = r.NewInsurancePolicyNumber ?? string.Empty,
                            HolderName = r.NewInsuranceHolderName,
                            HolderRelationship = r.NewInsuranceHolderRelationship
                        }
                        : null
                },
                Referral = r.IsReferral && r.ReferralInstitutionId.HasValue
                    ? new OnlineRegistrationCheckInReferral
                    {
                        ReferralInstitutionId = r.ReferralInstitutionId.Value,
                        InstitutionName = data.InstitutionName ?? string.Empty,
                        ReferralNumber = r.ReferralNumber,
                        ReferralDate = r.ReferralDate?.ToString("yyyy-MM-dd"),
                        ReferralDoctorName = r.ReferralDoctorName,
                        DiagnosisNote = r.ReferralDiagnosisNote,
                        Reason = r.ReferralReason,
                        DocumentCount = data.DocumentCount
                    }
                    : null,
                HasConsent = r.HasConsent
            };
        }

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private OnlineRegistrationPublicResult<OnlineRegistrationCheckInPackage> Reject(
            OnlineRegistrationPublicOutcome outcome,
            string message,
            string rule)
        {
            _logger.LogInformation("Verifikasi check-in pendaftaran online ditolak. Rule={Rule} Outcome={Outcome}", rule, outcome);
            return OnlineRegistrationPublicResult<OnlineRegistrationCheckInPackage>.Fail(outcome, message);
        }

        private static string GenerateToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(TokenBytes);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        /// <summary>Contoh: <c>25 Nov 2026</c> (<c>KSK-DEC-033</c>).</summary>
        private static string FormatShortDate(DateTime date) =>
            $"{date.Day} {IndonesianShortMonthNames[date.Month - 1]} {date.Year}";

        private static TimeZoneInfo ResolveBusinessTimeZone()
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Jakarta");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
        }
    }
}
