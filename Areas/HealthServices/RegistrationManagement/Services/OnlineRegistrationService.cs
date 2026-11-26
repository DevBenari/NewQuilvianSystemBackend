using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Layar Pendaftaran Online untuk petugas (kiosk Amendment PO, <c>KSK-DEC-050</c>). Task ini
    /// <c>BE-KSK-PO-009</c> memuat kueri: daftar, ringkasan, detail beserta riwayat, dan pembukaan
    /// berkas surat rujukan. <c>BE-KSK-PO-010</c> menambah ubah jadwal, ubah kontak, dan batal
    /// (<c>OnlineRegistrationService.Commands.cs</c>). Penutupan Jalur Lain dan Tidak Datang menyusul di
    /// <c>BE-KSK-PO-008</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Petugas melihat data pribadi lengkap (nama, NIK, No. RM, kontak) karena memang harus
    /// melayani pasien dan memeriksa pendaftaran mencurigakan. Dua hal tetap tidak keluar: kode
    /// pendaftaran (hanya hash yang tersimpan, <c>INV-PO-05</c>) dan nomor kartu penjamin yang sudah
    /// ada (tersamar).
    /// </para>
    /// <para>
    /// Pencarian kode memakai hash persis, sehingga kode tidak pernah dibandingkan dalam bentuk asli.
    /// Nomor pendaftaran, nama, No. RM, dan NIK dicocokkan sebagian.
    /// </para>
    /// </remarks>
    public partial class OnlineRegistrationService
    {
        public const string NotFoundMessage = "Pendaftaran online tidak ditemukan.";
        public const string SearchTooShortMessage = "Kata kunci pencarian minimal 3 karakter.";
        public const string InvalidDateRangeMessage = "Rentang tanggal tidak valid.";
        public const string DocumentNotFoundMessage = "Berkas surat rujukan tidak ditemukan.";

        private const int MinimumSearchLength = 3;
        private const int MaximumPageSize = 100;
        private const int MaximumRangeDays = 62;

        private static readonly Dictionary<string, string> EventNames = new(StringComparer.Ordinal)
        {
            [RegOnlineRegistrationHistory.EventSubmitted] = "Didaftarkan online",
            [RegOnlineRegistrationHistory.EventCheckedIn] = "Check-in di kiosk",
            [RegOnlineRegistrationHistory.EventRegisteredElsewhere] = "Didaftarkan lewat jalur lain",
            [RegOnlineRegistrationHistory.EventNoShow] = "Tidak datang",
            [RegOnlineRegistrationHistory.EventCancelled] = "Dibatalkan",
            [RegOnlineRegistrationHistory.EventScheduleChanged] = "Jadwal diubah",
            [RegOnlineRegistrationHistory.EventContactChanged] = "Kontak diubah"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly ReferralDocumentStorageService _documentStorage;
        private readonly OnlineRegistrationPublicService _publicService;
        private readonly DoctorScheduleQuotaService _quotaService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OnlineRegistrationService> _logger;
        private readonly TimeProvider _timeProvider;
        private readonly TimeZoneInfo _businessTimeZone;

        public OnlineRegistrationService(
            ApplicationDbContext dbContext,
            ReferralDocumentStorageService documentStorage,
            OnlineRegistrationPublicService publicService,
            DoctorScheduleQuotaService quotaService,
            IConfiguration configuration,
            ILogger<OnlineRegistrationService> logger,
            TimeProvider? timeProvider = null)
        {
            _dbContext = dbContext;
            _documentStorage = documentStorage;
            _publicService = publicService;
            _quotaService = quotaService;
            _configuration = configuration;
            _logger = logger;
            _timeProvider = timeProvider ?? TimeProvider.System;
            _businessTimeZone = ResolveBusinessTimeZone();
        }

        public sealed record DocumentContent(Stream Stream, string ContentType, string FileName);

        // ------------------------------------------------------------------
        // Daftar dan ringkasan
        // ------------------------------------------------------------------

        public async Task<(PagedResult<OnlineRegistrationListItemResponse>? Result, string? Error)> GetPagedAsync(
            OnlineRegistrationListQuery query,
            CancellationToken cancellationToken = default)
        {
            var range = ResolveRange(query.VisitDateFrom, query.VisitDateTo);
            if (range == null)
                return (null, InvalidDateRangeMessage);

            if (query.Status.HasValue && !Enum.IsDefined(query.Status.Value))
                return (null, "Status pendaftaran tidak dikenal.");

            var search = query.Search?.Trim();
            if (!string.IsNullOrEmpty(search) && search.Length < MinimumSearchLength)
                return (null, SearchTooShortMessage);

            var (from, to) = range.Value;
            var pageNumber = Math.Max(1, query.PageNumber);
            var pageSize = Math.Clamp(query.PageSize, 1, MaximumPageSize);

            var source = _dbContext.RegOnlineRegistrations
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.VisitDate >= from && x.VisitDate <= to);

            if (query.Status.HasValue)
                source = source.Where(x => x.Status == query.Status.Value);

            if (!string.IsNullOrEmpty(search))
            {
                var pattern = BuildContainsPattern(search);
                var codeHash = TryHashCode(search);

                source = source.Where(x =>
                    (codeHash != null && x.CodeHash == codeHash) ||
                    EF.Functions.ILike(x.RegistrationNumber, pattern, "\\") ||
                    (x.FullName != null && EF.Functions.ILike(x.FullName, pattern, "\\")) ||
                    (x.IdentityNumber != null && EF.Functions.ILike(x.IdentityNumber, pattern, "\\")) ||
                    (x.Patient != null &&
                     (EF.Functions.ILike(x.Patient.FullName, pattern, "\\") ||
                      EF.Functions.ILike(x.Patient.MedicalRecordNumber, pattern, "\\") ||
                      (x.Patient.IdentityNumber != null && EF.Functions.ILike(x.Patient.IdentityNumber, pattern, "\\")))));
            }

            var totalData = await source.CountAsync(cancellationToken);

            var rows = await source
                .OrderBy(x => x.VisitDate)
                .ThenBy(x => x.DoctorSchedule != null ? x.DoctorSchedule.StartTime : TimeSpan.Zero)
                .ThenBy(x => x.RegistrationNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(RowProjection)
                .ToListAsync(cancellationToken);

            return (new PagedResult<OnlineRegistrationListItemResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = rows.Select(row => Fill(new OnlineRegistrationListItemResponse(), row)).ToList()
            }, null);
        }

        public async Task<(OnlineRegistrationSummaryResponse? Result, string? Error)> GetSummaryAsync(
            DateTime? visitDateFrom,
            DateTime? visitDateTo,
            CancellationToken cancellationToken = default)
        {
            var range = ResolveRange(visitDateFrom, visitDateTo);
            if (range == null)
                return (null, InvalidDateRangeMessage);

            var (from, to) = range.Value;

            var counts = await _dbContext.RegOnlineRegistrations
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.VisitDate >= from && x.VisitDate <= to)
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

            int Count(OnlineRegistrationStatus status) => counts.GetValueOrDefault(status);

            return (new OnlineRegistrationSummaryResponse
            {
                VisitDateFrom = from.ToString("yyyy-MM-dd"),
                VisitDateTo = to.ToString("yyyy-MM-dd"),
                AwaitingCheckIn = Count(OnlineRegistrationStatus.AwaitingCheckIn),
                CheckedIn = Count(OnlineRegistrationStatus.CheckedIn),
                RegisteredElsewhere = Count(OnlineRegistrationStatus.RegisteredElsewhere),
                NoShow = Count(OnlineRegistrationStatus.NoShow),
                Cancelled = Count(OnlineRegistrationStatus.Cancelled),
                Total = counts.Values.Sum()
            }, null);
        }

        // ------------------------------------------------------------------
        // Detail dan berkas
        // ------------------------------------------------------------------

        public async Task<OnlineRegistrationDetailResponse?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var row = await _dbContext.RegOnlineRegistrations
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(RowProjection)
                .FirstOrDefaultAsync(cancellationToken);

            if (row == null)
                return null;

            var extra = await _dbContext.RegOnlineRegistrations
                .AsNoTracking()
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    x.BirthDate,
                    PatientBirthDate = x.Patient != null ? x.Patient.BirthDate : null,
                    x.Gender,
                    PatientGender = x.Patient != null ? (Gender?)x.Patient.Gender : null,
                    x.ClinicId,
                    x.DoctorScheduleId,
                    SessionName = x.DoctorSchedule != null ? x.DoctorSchedule.SessionName : null,
                    x.PatientInsuranceId,
                    InsuranceProviderName = x.PatientInsurance != null && x.PatientInsurance.InsuranceProvider != null
                        ? x.PatientInsurance.InsuranceProvider.InsuranceProviderName
                        : null,
                    InsuranceNumber = x.PatientInsurance != null ? x.PatientInsurance.CardNumber ?? x.PatientInsurance.PolicyNumber : null,
                    x.PatientCompanyGuarantorId,
                    CompanyName = x.PatientCompanyGuarantor != null && x.PatientCompanyGuarantor.CompanyGuarantor != null
                        ? x.PatientCompanyGuarantor.CompanyGuarantor.CompanyGuarantorName
                        : null,
                    CompanyNumber = x.PatientCompanyGuarantor != null ? x.PatientCompanyGuarantor.EmployeeNumber : null,
                    x.NewInsuranceProviderId,
                    NewProviderName = x.NewInsuranceProvider != null ? x.NewInsuranceProvider.InsuranceProviderName : null,
                    x.NewInsurancePolicyNumber,
                    x.NewInsuranceHolderName,
                    x.NewInsuranceHolderRelationship,
                    x.IsReferral,
                    InstitutionName = x.ReferralInstitution != null ? x.ReferralInstitution.InstitutionName : null,
                    x.ReferralNumber,
                    x.ReferralDate,
                    x.ReferralDoctorName,
                    x.ReferralDiagnosisNote,
                    x.ReferralReason,
                    x.HasConsent,
                    x.ConsentSignedAt,
                    x.CheckedInAt,
                    x.ClosedAt,
                    x.CancelReason,
                    Documents = x.ReferralDocuments
                        .Where(d => !d.IsDelete)
                        .OrderBy(d => d.PageOrder)
                        .Select(d => new OnlineRegistrationDocumentResponse
                        {
                            Id = d.Id,
                            OriginalFileName = d.OriginalFileName,
                            ContentType = d.ContentType,
                            SizeBytes = d.SizeBytes,
                            PageOrder = d.PageOrder
                        })
                        .ToList()
                })
                .FirstAsync(cancellationToken);

            var detail = Fill(new OnlineRegistrationDetailResponse(), row);
            var isNew = row.PatientMode == OnlineRegistrationPatientMode.New && row.PatientId == null;
            var birthDate = isNew ? extra.BirthDate : extra.PatientBirthDate ?? extra.BirthDate;
            var gender = isNew ? extra.Gender : extra.PatientGender ?? extra.Gender;

            detail.BirthDate = birthDate?.ToString("yyyy-MM-dd");
            detail.Gender = gender;
            detail.GenderName = gender.HasValue ? GetEnumDisplayName(gender.Value) : null;
            detail.ClinicId = extra.ClinicId;
            detail.DoctorScheduleId = extra.DoctorScheduleId;
            detail.SessionName = extra.SessionName;
            detail.PayerName = extra.PatientInsuranceId.HasValue ? extra.InsuranceProviderName
                : extra.PatientCompanyGuarantorId.HasValue ? extra.CompanyName
                : null;
            detail.MaskedCardNumber = extra.PatientInsuranceId.HasValue ? OnlineRegistrationPublicService.MaskNumber(extra.InsuranceNumber)
                : extra.PatientCompanyGuarantorId.HasValue ? OnlineRegistrationPublicService.MaskNumber(extra.CompanyNumber)
                : null;
            detail.NewInsurance = extra.NewInsuranceProviderId.HasValue
                ? new OnlineRegistrationNewInsuranceResponse
                {
                    InsuranceProviderId = extra.NewInsuranceProviderId.Value,
                    ProviderName = extra.NewProviderName ?? string.Empty,
                    PolicyNumber = extra.NewInsurancePolicyNumber ?? string.Empty,
                    HolderName = extra.NewInsuranceHolderName,
                    HolderRelationship = extra.NewInsuranceHolderRelationship
                }
                : null;
            detail.IsReferral = extra.IsReferral;
            detail.ReferralInstitutionName = extra.InstitutionName;
            detail.ReferralNumber = extra.ReferralNumber;
            detail.ReferralDate = extra.ReferralDate?.ToString("yyyy-MM-dd");
            detail.ReferralDoctorName = extra.ReferralDoctorName;
            detail.DiagnosisNote = extra.ReferralDiagnosisNote;
            detail.Reason = extra.ReferralReason;
            detail.Documents = extra.Documents;
            detail.HasConsent = extra.HasConsent;
            detail.ConsentSignedAt = AsUtc(extra.ConsentSignedAt);
            detail.CheckedInAt = AsUtc(extra.CheckedInAt);
            detail.ClosedAt = AsUtc(extra.ClosedAt);
            detail.CancelReason = extra.CancelReason;
            detail.Histories = await LoadHistoriesAsync(id, cancellationToken);

            return detail;
        }

        /// <summary>
        /// Membuka satu berkas surat rujukan milik pendaftaran ini dari root privat. Path dijaga tetap
        /// di dalam root, sama dengan <see cref="ReferralDocumentStorageService.OpenAsync"/>.
        /// </summary>
        public async Task<(string? Error, bool NotFound, DocumentContent? Content)> OpenDocumentAsync(
            Guid registrationId,
            Guid documentId,
            CancellationToken cancellationToken = default)
        {
            var document = await _dbContext.RegOnlineRegistrationReferralDocuments
                .AsNoTracking()
                .Where(x =>
                    x.Id == documentId &&
                    x.OnlineRegistrationId == registrationId &&
                    !x.IsDelete &&
                    x.OnlineRegistration != null &&
                    !x.OnlineRegistration.IsDelete)
                .Select(x => new { x.StoragePath, x.ContentType, x.OriginalFileName })
                .FirstOrDefaultAsync(cancellationToken);

            if (document == null)
                return (DocumentNotFoundMessage, true, null);

            var (error, notFound, stream) = _documentStorage.OpenStoredFile(document.StoragePath);
            if (stream == null)
                return (error ?? DocumentNotFoundMessage, notFound, null);

            return (null, false, new DocumentContent(stream, document.ContentType, document.OriginalFileName));
        }

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        /// <summary>Kolom yang sama untuk item daftar dan kepala detail.</summary>
        private sealed class Row
        {
            public Guid Id { get; init; }
            public string RegistrationNumber { get; init; } = string.Empty;
            public OnlineRegistrationStatus Status { get; init; }
            public DateTime VisitDate { get; init; }
            public string? ClinicName { get; init; }
            public string? DoctorName { get; init; }
            public TimeSpan? StartTime { get; init; }
            public TimeSpan? EndTime { get; init; }
            public OnlineRegistrationPatientMode PatientMode { get; init; }
            public Guid? PatientId { get; init; }
            public string? OnlineFullName { get; init; }
            public string? PatientFullName { get; init; }
            public string? MedicalRecordNumber { get; init; }
            public string? OnlineIdentityNumber { get; init; }
            public string? PatientIdentityNumber { get; init; }
            public OnlineRegistrationPatientType PatientType { get; init; }
            public EncounterPaymentType PaymentType { get; init; }
            public string? ContactWhatsAppNumber { get; init; }
            public string? ContactEmail { get; init; }
            public OnlineRegistrationEmailStatus EmailStatus { get; init; }
            public DateTime SubmittedAt { get; init; }
            public string? EncounterNumber { get; init; }
            public Guid RowVersion { get; init; }
        }

        private static readonly System.Linq.Expressions.Expression<Func<RegOnlineRegistration, Row>> RowProjection = x => new Row
        {
            Id = x.Id,
            RegistrationNumber = x.RegistrationNumber,
            Status = x.Status,
            VisitDate = x.VisitDate,
            ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
            DoctorName = x.DoctorSchedule != null && x.DoctorSchedule.Doctor != null ? x.DoctorSchedule.Doctor.FullName : null,
            StartTime = x.DoctorSchedule != null ? x.DoctorSchedule.StartTime : (TimeSpan?)null,
            EndTime = x.DoctorSchedule != null ? x.DoctorSchedule.EndTime : (TimeSpan?)null,
            PatientMode = x.PatientMode,
            PatientId = x.PatientId,
            OnlineFullName = x.FullName,
            PatientFullName = x.Patient != null ? x.Patient.FullName : null,
            MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : null,
            OnlineIdentityNumber = x.IdentityNumber,
            PatientIdentityNumber = x.Patient != null ? x.Patient.IdentityNumber : null,
            PatientType = x.PatientType,
            PaymentType = x.PaymentType,
            ContactWhatsAppNumber = x.ContactWhatsAppNumber,
            ContactEmail = x.ContactEmail,
            EmailStatus = x.EmailDeliveryStatus,
            SubmittedAt = x.SubmittedAt,
            EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : null,
            RowVersion = x.RowVersion
        };

        private static TItem Fill<TItem>(TItem item, Row row) where TItem : OnlineRegistrationListItemResponse
        {
            var isAwaiting = row.Status == OnlineRegistrationStatus.AwaitingCheckIn;

            item.Id = row.Id;
            item.RegistrationNumber = row.RegistrationNumber;
            item.Status = row.Status;
            item.StatusName = GetEnumDisplayName(row.Status);
            item.VisitDate = row.VisitDate.ToString("yyyy-MM-dd");
            item.ClinicName = row.ClinicName ?? string.Empty;
            item.DoctorName = row.DoctorName ?? string.Empty;
            item.StartTime = row.StartTime?.ToString(@"hh\:mm") ?? string.Empty;
            item.EndTime = row.EndTime?.ToString(@"hh\:mm") ?? string.Empty;
            item.PatientMode = row.PatientMode;
            item.PatientModeName = GetEnumDisplayName(row.PatientMode);
            item.PatientName = row.PatientFullName ?? row.OnlineFullName ?? string.Empty;
            item.MedicalRecordNumber = row.MedicalRecordNumber;
            item.IdentityNumber = row.PatientIdentityNumber ?? row.OnlineIdentityNumber;
            item.PatientType = row.PatientType;
            item.PatientTypeName = GetEnumDisplayName(row.PatientType);
            item.PaymentType = row.PaymentType;
            item.PaymentTypeName = GetEnumDisplayName(row.PaymentType);
            item.Contact = row.ContactWhatsAppNumber ?? row.ContactEmail;
            item.EmailStatus = row.EmailStatus;
            item.EmailStatusName = GetEnumDisplayName(row.EmailStatus);
            item.SubmittedAt = DateTime.SpecifyKind(row.SubmittedAt, DateTimeKind.Utc);
            item.EncounterNumber = string.IsNullOrEmpty(row.EncounterNumber) ? null : row.EncounterNumber;
            item.CanChangeSchedule = isAwaiting;
            item.CanChangeContact = isAwaiting;
            item.CanCancel = isAwaiting;
            item.RowVersion = row.RowVersion;
            return item;
        }

        private async Task<List<OnlineRegistrationHistoryResponse>> LoadHistoriesAsync(Guid registrationId, CancellationToken cancellationToken)
        {
            var histories = await _dbContext.RegOnlineRegistrationHistories
                .AsNoTracking()
                .Where(x => x.OnlineRegistrationId == registrationId && !x.IsDelete)
                .OrderBy(x => x.OccurredAt)
                .ToListAsync(cancellationToken);

            var actorIds = histories.Where(x => x.ActorUserId.HasValue).Select(x => x.ActorUserId!.Value).Distinct().ToList();
            var actorNames = actorIds.Count == 0
                ? new Dictionary<Guid, string>()
                : await _dbContext.Users
                    .AsNoTracking()
                    .Where(x => actorIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

            var scheduleIds = histories
                .SelectMany(x => new[] { x.FromDoctorScheduleId, x.ToDoctorScheduleId })
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();

            var scheduleLabels = scheduleIds.Count == 0
                ? new Dictionary<Guid, string>()
                : (await _dbContext.Set<MstDoctorSchedule>()
                    .AsNoTracking()
                    .Where(x => scheduleIds.Contains(x.Id))
                    .Select(x => new { x.Id, DoctorName = x.Doctor != null ? x.Doctor.FullName : null, x.StartTime, x.EndTime })
                    .ToListAsync(cancellationToken))
                    .ToDictionary(
                        x => x.Id,
                        x => $"{x.DoctorName ?? "-"} {x.StartTime:hh\\:mm}–{x.EndTime:hh\\:mm}");

            return histories.Select(x => new OnlineRegistrationHistoryResponse
            {
                EventCode = x.EventCode,
                EventName = EventNames.GetValueOrDefault(x.EventCode, x.EventCode),
                FromStatus = x.FromStatus,
                ToStatus = x.ToStatus,
                SourceName = GetEnumDisplayName(x.Source),
                ActorName = x.ActorUserId.HasValue && actorNames.TryGetValue(x.ActorUserId.Value, out var name) && !string.IsNullOrWhiteSpace(name)
                    ? name
                    : GetEnumDisplayName(x.Source),
                Reason = x.Reason,
                FromVisitDate = x.FromVisitDate?.ToString("yyyy-MM-dd"),
                ToVisitDate = x.ToVisitDate?.ToString("yyyy-MM-dd"),
                FromScheduleLabel = x.FromDoctorScheduleId.HasValue ? scheduleLabels.GetValueOrDefault(x.FromDoctorScheduleId.Value) : null,
                ToScheduleLabel = x.ToDoctorScheduleId.HasValue ? scheduleLabels.GetValueOrDefault(x.ToDoctorScheduleId.Value) : null,
                FromContact = x.FromContact,
                ToContact = x.ToContact,
                OccurredAt = DateTime.SpecifyKind(x.OccurredAt, DateTimeKind.Utc)
            }).ToList();
        }

        /// <summary>Rentang bawaan hari ini (WIB); ditolak bila terbalik atau lebih dari 62 hari (<c>KSK-VAL-PO-069</c>).</summary>
        private (DateTime From, DateTime To)? ResolveRange(DateTime? visitDateFrom, DateTime? visitDateTo)
        {
            var today = TimeZoneInfo.ConvertTime(_timeProvider.GetUtcNow(), _businessTimeZone).Date;
            var from = ToCalendarDate(visitDateFrom ?? visitDateTo ?? today);
            var to = ToCalendarDate(visitDateTo ?? visitDateFrom ?? today);

            if (from > to || (to - from).Days + 1 > MaximumRangeDays)
                return null;

            return (from, to);
        }

        /// <summary>Hash kode bila kata kunci berbentuk kode pendaftaran; selain itu <c>null</c>.</summary>
        private static string? TryHashCode(string search)
        {
            var normalized = OnlineRegistrationPublicService.NormalizeCode(search);
            return normalized.Length == OnlineRegistrationPublicService.CodeLength &&
                   normalized.All(ch => OnlineRegistrationPublicService.CodeAlphabet.Contains(ch))
                ? OnlineRegistrationPublicService.ComputeCodeHash(normalized)
                : null;
        }

        private static string BuildContainsPattern(string search) =>
            "%" + search.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";

        private static DateTime ToCalendarDate(DateTime value) => DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);

        private static DateTime? AsUtc(DateTime? value) =>
            value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null;

        private static string GetEnumDisplayName<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
        }

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
