using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Detail dan edit data pendaftaran satu kunjungan pada Daftar Pasien Rawat Jalan
    /// (RJ-REG-REV-BE-003, RJ-REG-DEC-005).
    /// </summary>
    /// <remarks>
    /// Cakupan memakai <see cref="OutpatientEncounterListService.BuildScopedQuery"/> yang sama
    /// dengan daftar, sehingga detail tidak pernah membuka kunjungan yang tidak tampil di daftar
    /// pengguna. Kunjungan di luar cakupan dijawab <c>404</c>, bukan <c>403</c>, agar
    /// keberadaannya tidak bocor.
    /// </remarks>
    public class OutpatientEncounterDetailService
    {
        public const string UpdateAction = "Update";
        private const string ReferralLetterNotFoundMessage = "Surat Rujukan tidak ditemukan pada kunjungan ini.";
        private const string LogCategory = "HealthServices.RegistrationManagement";
        private static readonly Regex PhonePattern = new(@"^\+?[0-9][0-9 \-]{5,28}$", RegexOptions.Compiled);

        private readonly ApplicationDbContext _dbContext;
        private readonly OutpatientEncounterListService _listService;
        private readonly AccessPermissionService _accessPermissionService;
        private readonly EncounterReferralService _encounterReferralService;
        private readonly LoggerService _loggerService;

        private readonly OutpatientEncounterScheduleService _scheduleService;
        private readonly DoctorCertificateService _doctorCertificateService;

        public OutpatientEncounterDetailService(
            ApplicationDbContext dbContext,
            OutpatientEncounterListService listService,
            AccessPermissionService accessPermissionService,
            EncounterReferralService encounterReferralService,
            LoggerService loggerService,
            OutpatientEncounterScheduleService scheduleService,
            DoctorCertificateService doctorCertificateService)
        {
            _scheduleService = scheduleService;
            _doctorCertificateService = doctorCertificateService;
            _dbContext = dbContext;
            _listService = listService;
            _accessPermissionService = accessPermissionService;
            _encounterReferralService = encounterReferralService;
            _loggerService = loggerService;
        }

        public async Task<OutpatientEncounterDetailResponse> GetDetailAsync(
            ClaimsPrincipal user,
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var scope = await _listService.ResolveScopeOrThrowAsync(user, cancellationToken);

            var row = await _listService.BuildScopedQuery(scope)
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    Encounter = x,
                    Patient = x.Patient!,
                    NationalityCountryName = x.Patient!.NationalityCountry != null ? x.Patient.NationalityCountry.CountryName : null,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : string.Empty,
                    DoctorName = x.Doctor != null ? x.Doctor.FullName : null,
                    DoctorSpecialistName = x.Doctor != null ? x.Doctor.SpecialistName : null,
                    ScheduleSessionName = x.DoctorSchedule != null ? x.DoctorSchedule.SessionName : null,
                    ScheduleStartTime = x.DoctorSchedule != null ? (TimeSpan?)x.DoctorSchedule.StartTime : null,
                    ScheduleEndTime = x.DoctorSchedule != null ? (TimeSpan?)x.DoctorSchedule.EndTime : null,
                    SchedulePracticeLocation = x.DoctorSchedule != null ? x.DoctorSchedule.PracticeLocation : null,
                    PaymentSourceName = x.PaymentSource != null ? x.PaymentSource.PaymentSourceNameSnapshot : null,
                    PolicyNumber = x.PaymentSource != null ? x.PaymentSource.PolicyNumberSnapshot : null,
                    CardNumber = x.PaymentSource != null ? x.PaymentSource.CardNumberSnapshot : null,
                    ReferralIsComplete = _dbContext.Set<RegEncounterReferral>()
                        .Where(r => r.PatientEncounterId == x.Id && !r.IsDelete)
                        .Select(r => (bool?)r.IsComplete)
                        .FirstOrDefault(),
                    QueueCode = _dbContext.Set<RegQueue>()
                        .Where(q => q.EncounterId == x.Id && !q.IsDelete && !q.IsCancel)
                        .OrderByDescending(q => q.CreateDateTime)
                        .Select(q => q.QueueCode)
                        .FirstOrDefault(),
                    QueueStatus = _dbContext.Set<RegQueue>()
                        .Where(q => q.EncounterId == x.Id && !q.IsDelete && !q.IsCancel)
                        .OrderByDescending(q => q.CreateDateTime)
                        .Select(q => (QueueStatus?)q.QueueStatus)
                        .FirstOrDefault(),
                    QueueDate = _dbContext.Set<RegQueue>()
                        .Where(q => q.EncounterId == x.Id && !q.IsDelete && !q.IsCancel)
                        .OrderByDescending(q => q.CreateDateTime)
                        .Select(q => (DateTime?)q.QueueDate)
                        .FirstOrDefault(),
                    IsBooking = _dbContext.Set<RegPatientEncounter>()
                        .Where(OutpatientEncounterRules.IsBookingState)
                        .Any(b => b.Id == x.Id),
                    HasActiveConsultation = _dbContext.Set<TrxDoctorConsultation>()
                        .Any(c => c.EncounterId == x.Id && !c.IsDelete && !c.IsCancel)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (row == null)
                throw new OutpatientEncounterNotFoundException();

            var encounter = row.Encounter;
            var patient = row.Patient;
            var canUpdate = await _accessPermissionService.HasAccessAsync(
                user, ClinicalActorScopeService.ResourceName, UpdateAction);
            var blockedReason = GetEditBlockedReason(encounter);
            var scheduleBlockedReason = blockedReason ?? await _scheduleService.GetBlockedReasonAsync(encounter, cancellationToken);
            var canCancelPermission = await _accessPermissionService.HasAccessAsync(
                user, ClinicalActorScopeService.ResourceName, OutpatientEncounterListService.CancelAction);
            var cancelBlockedReason = OutpatientEncounterListService.GetCancelBlockedReason(
                encounter.EncounterStatus, encounter.IsCancel, encounter.CompletedAt != null, row.HasActiveConsultation);
            var referral = encounter.IsReferral
                ? await _encounterReferralService.GetAsync(encounter.Id, cancellationToken)
                : null;
            var referralLetters = await LoadReferralLettersAsync(encounter.Id, referral?.SourceLetter?.CertificateId, cancellationToken);

            return new OutpatientEncounterDetailResponse
            {
                Id = encounter.Id,
                EncounterNumber = encounter.EncounterNumber,
                EncounterDate = encounter.EncounterDate,
                EncounterStatus = (int)encounter.EncounterStatus,
                EncounterStatusName = GetDisplayName(encounter.EncounterStatus),
                VisitTypeName = GetDisplayName(encounter.VisitType),
                IsAppointment = encounter.IsAppointment,
                IsWalkIn = encounter.IsWalkIn,
                IsFromKiosk = encounter.IsFromKiosk,
                IsCancelled = encounter.IsCancel,
                IsBooking = row.IsBooking,
                CancelledAt = encounter.CancelledAt,
                CancelReason = encounter.CancelReason,
                RegisteredAt = encounter.RegisteredAt,
                CheckedInAt = encounter.CheckedInAt,
                CompletedAt = encounter.CompletedAt,
                ClinicId = encounter.ClinicId!.Value,
                ClinicName = row.ClinicName,
                DoctorId = encounter.DoctorId,
                DoctorName = row.DoctorName,
                DoctorSpecialistName = row.DoctorSpecialistName,
                DoctorScheduleId = encounter.DoctorScheduleId,
                ScheduleLabel = BuildScheduleLabel(row.ScheduleSessionName, row.ScheduleStartTime, row.ScheduleEndTime, row.SchedulePracticeLocation),
                QueueCode = row.QueueCode,
                QueueStatusName = row.QueueStatus.HasValue ? GetDisplayName(row.QueueStatus.Value) : null,
                QueueDate = row.QueueDate,
                PaymentTypeName = GetDisplayName(encounter.PaymentType),
                PaymentLabel = !string.IsNullOrWhiteSpace(row.PaymentSourceName)
                    ? row.PaymentSourceName!
                    : GetDisplayName(encounter.PaymentType),
                PolicyNumber = row.PolicyNumber,
                CardNumber = row.CardNumber,
                ReferralStatus = ResolveReferralStatus(encounter, row.ReferralIsComplete),
                ChiefComplaint = encounter.ChiefComplaint,
                Notes = encounter.Notes,
                Referral = referral,
                ReferralLetters = referralLetters,
                Patient = new OutpatientEncounterPatientResponse
                {
                    Id = patient.Id,
                    MedicalRecordNumber = patient.MedicalRecordNumber,
                    FullName = patient.FullName,
                    BirthDate = patient.BirthDate,
                    GenderName = patient.Gender.HasValue ? GetDisplayName(patient.Gender.Value) : null,
                    IdentityType = patient.IdentityType,
                    IdentityNumber = patient.IdentityNumber,
                    PhoneNumber = patient.PhoneNumber,
                    WhatsAppNumber = patient.WhatsAppNumber,
                    Email = patient.Email,
                    Address = patient.Address,
                    Citizenship = patient.Citizenship,
                    CitizenshipName = patient.Citizenship.HasValue ? GetDisplayName(patient.Citizenship.Value) : null,
                    NationalityCountryId = patient.NationalityCountryId,
                    NationalityCountryName = row.NationalityCountryName
                },
                CanEdit = canUpdate && blockedReason == null,
                EditBlockedReason = blockedReason,
                CanChangeSchedule = canUpdate && scheduleBlockedReason == null,
                ScheduleChangeBlockedReason = scheduleBlockedReason,
                CanCancel = canCancelPermission && cancelBlockedReason == null,
                CancelBlockedReason = cancelBlockedReason,
                UpdateDateTime = encounter.UpdateDateTime
            };
        }

        /// <summary>
        /// Isi resmi satu Surat Rujukan terkait kunjungan untuk preview dan cetak ulang petugas
        /// (<c>RJ-REF-REV-004</c>).
        /// </summary>
        /// <remarks>
        /// Kunjungan harus dalam cakupan pengguna (di luar cakupan <c>404</c>), dan surat harus
        /// terhubung sah dengan kunjungan: diterbitkan pada kunjungan ini atau menjadi dasar rujukan
        /// internal kunjungan ini. ID surat lain dijawab <c>404</c> (cegah IDOR). Hanya surat
        /// berstatus terbit yang dikembalikan; isi selalu dari data tersimpan, bukan draft. Akses
        /// dicatat audit tanpa isi medis.
        /// </remarks>
        public async Task<DoctorCertificateResponse> GetReferralLetterAsync(
            ClaimsPrincipal user,
            Guid id,
            Guid certificateId,
            CancellationToken cancellationToken = default)
        {
            var scope = await _listService.ResolveScopeOrThrowAsync(user, cancellationToken);

            var visible = await _listService.BuildScopedQuery(scope).AnyAsync(x => x.Id == id, cancellationToken);
            if (!visible)
                throw new OutpatientEncounterNotFoundException();

            var linked = await _dbContext.Set<CliDoctorCertificate>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == certificateId &&
                    !x.IsDelete &&
                    x.CertificateType == DoctorCertificateType.InpatientReferral &&
                    (x.EncounterId == id ||
                     _dbContext.Set<RegEncounterReferral>().Any(r =>
                         r.PatientEncounterId == id &&
                         !r.IsDelete &&
                         r.SourceDoctorCertificateId == x.Id)))
                .Select(x => new { x.CertificateStatus })
                .FirstOrDefaultAsync(cancellationToken);

            if (linked == null)
                throw new OutpatientEncounterNotFoundException(ReferralLetterNotFoundMessage);

            if (linked.CertificateStatus != DoctorCertificateStatus.Issued)
                throw new OutpatientEncounterValidationException("Surat Rujukan sudah dibatalkan dokter sehingga tidak dapat dicetak sebagai surat resmi.");

            var letter = await _doctorCertificateService.GetByIdAsync(certificateId, cancellationToken)
                ?? throw new OutpatientEncounterNotFoundException(ReferralLetterNotFoundMessage);

            await _loggerService.AuditAsync(LogCategory, "ReadOutpatientReferralLetter",
                "Surat Rujukan dibuka untuk preview/cetak dari Daftar Pasien Rawat Jalan.",
                new
                {
                    EntityId = id,
                    CertificateId = certificateId,
                    Controller = "OutpatientEncounter",
                    Action = "Read",
                    ActorUserId = GetUserId(user)
                });

            return letter;
        }

        /// <summary>
        /// Surat Rujukan keluar (diterbitkan pada kunjungan ini) dan masuk (dasar rujukan internal),
        /// termasuk yang dibatalkan supaya histori tetap terbaca.
        /// </summary>
        private async Task<List<OutpatientReferralLetterResponse>> LoadReferralLettersAsync(
            Guid encounterId,
            Guid? incomingCertificateId,
            CancellationToken cancellationToken)
        {
            var rows = await _dbContext.Set<CliDoctorCertificate>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.CertificateType == DoctorCertificateType.InpatientReferral &&
                    (x.EncounterId == encounterId || x.Id == incomingCertificateId))
                .OrderByDescending(x => x.IssuedDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Select(x => new
                {
                    x.Id,
                    x.CertificateNumber,
                    x.EncounterId,
                    x.IssuedDate,
                    x.DoctorNameSnapshot,
                    x.ClinicNameSnapshot,
                    x.ReferralTargetNameSnapshot,
                    x.CertificateStatus,
                    SourceEncounterNumber = _dbContext.Set<RegPatientEncounter>()
                        .Where(e => e.Id == x.EncounterId)
                        .Select(e => e.EncounterNumber)
                        .FirstOrDefault(),
                    FollowUpEncounterNumber = _dbContext.Set<RegEncounterReferral>()
                        .Where(r =>
                            r.SourceDoctorCertificateId == x.Id &&
                            !r.IsDelete &&
                            r.PatientEncounter != null &&
                            !r.PatientEncounter.IsDelete &&
                            !r.PatientEncounter.IsCancel)
                        .Select(r => r.PatientEncounter!.EncounterNumber)
                        .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            return rows.Select(x =>
            {
                var outgoing = x.EncounterId == encounterId;
                var issued = x.CertificateStatus == DoctorCertificateStatus.Issued;

                return new OutpatientReferralLetterResponse
                {
                    CertificateId = x.Id,
                    CertificateNumber = x.CertificateNumber,
                    Direction = outgoing ? OutpatientReferralLetterResponse.DirectionOutgoing : OutpatientReferralLetterResponse.DirectionIncoming,
                    DirectionName = outgoing ? "Diterbitkan pada kunjungan ini" : "Dasar rujukan kunjungan ini",
                    IssuedDate = x.IssuedDate,
                    DoctorName = x.DoctorNameSnapshot,
                    SourceClinicName = x.ClinicNameSnapshot,
                    SourceEncounterNumber = x.SourceEncounterNumber,
                    TargetName = x.ReferralTargetNameSnapshot,
                    CertificateStatus = (int)x.CertificateStatus,
                    CertificateStatusName = issued ? "Terbit" : "Dibatalkan",
                    CanPrint = issued,
                    FollowUpEncounterNumber = outgoing ? x.FollowUpEncounterNumber : null
                };
            }).ToList();
        }

        /// <summary>
        /// Mengubah kontak pasien, kewarganegaraan, dan catatan kunjungan (RJ-REG-DEC-005).
        /// </summary>
        /// <remarks>
        /// Baris kunjungan dikunci <c>FOR UPDATE</c>, lalu status diperiksa ulang di dalam
        /// transaksi supaya kunjungan yang baru saja masuk konsultasi tidak ikut terubah.
        /// Perubahan kontak berlaku pada data induk pasien (satu sumber kebenaran), bukan salinan
        /// per kunjungan. Audit hanya mencatat nama field yang berubah, tanpa nilainya.
        /// </remarks>
        public async Task<OutpatientEncounterDetailResponse> UpdateRegistrationDataAsync(
            ClaimsPrincipal user,
            Guid id,
            OutpatientEncounterRegistrationDataUpdateRequest? request,
            CancellationToken cancellationToken = default)
        {
            if (request == null)
                throw new OutpatientEncounterValidationException("Data pendaftaran wajib diisi.");

            var scope = await _listService.ResolveScopeOrThrowAsync(user, cancellationToken);

            var visible = await _listService.BuildScopedQuery(scope).AnyAsync(x => x.Id == id, cancellationToken);
            if (!visible)
                throw new OutpatientEncounterNotFoundException();

            var phone = NormalizeText(request.PhoneNumber);
            var whatsApp = NormalizeText(request.WhatsAppNumber);
            var email = NormalizeText(request.Email)?.ToLowerInvariant();
            var address = NormalizeText(request.Address);
            var notes = NormalizeText(request.Notes);

            ValidateLength(phone, 30, "No. HP");
            ValidateLength(whatsApp, 30, "No. WhatsApp");
            ValidateLength(email, 200, "Email");
            ValidateLength(address, 500, "Alamat");
            ValidateLength(notes, 500, "Catatan");

            if (phone != null && !PhonePattern.IsMatch(phone))
                throw new OutpatientEncounterValidationException("No. HP tidak valid.");

            if (whatsApp != null && !PhonePattern.IsMatch(whatsApp))
                throw new OutpatientEncounterValidationException("No. WhatsApp tidak valid.");

            if (email != null && !new EmailAddressAttribute().IsValid(email))
                throw new OutpatientEncounterValidationException("Format email tidak valid.");

            Guid? nationalityCountryId = null;

            if (request.Citizenship.HasValue)
            {
                if (!Enum.IsDefined(typeof(PatientCitizenship), request.Citizenship.Value))
                    throw new OutpatientEncounterValidationException("Kewarganegaraan tidak valid. Gunakan WNI atau WNA.");

                if (request.Citizenship.Value == PatientCitizenship.Wna)
                {
                    if (!request.NationalityCountryId.HasValue || request.NationalityCountryId.Value == Guid.Empty)
                        throw new OutpatientEncounterValidationException("Negara asal wajib diisi untuk pasien WNA.");

                    var countryId = request.NationalityCountryId.Value;
                    var countryExists = await _dbContext.MstCountries
                        .AsNoTracking()
                        .AnyAsync(x => x.Id == countryId && x.IsActive && !x.IsDelete, cancellationToken);

                    if (!countryExists)
                        throw new OutpatientEncounterValidationException("Negara asal tidak valid atau tidak aktif.");

                    nationalityCountryId = countryId;
                }
            }

            var actorUserId = GetUserId(user);
            var now = DateTime.UtcNow;
            var changedFields = new List<string>();
            Guid patientId;
            OutpatientEncounterScheduleService.ScheduleChange? scheduleChange;

            await using (var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                var encounter = await _dbContext.Set<RegPatientEncounter>()
                    .FromSqlInterpolated($@"
                        SELECT *
                        FROM public.""RegPatientEncounter""
                        WHERE ""Id"" = {id}
                          AND ""IsDelete"" = false
                        FOR UPDATE")
                    .FirstOrDefaultAsync(cancellationToken);

                if (encounter == null)
                    throw new OutpatientEncounterNotFoundException();

                var blockedReason = GetEditBlockedReason(encounter);
                if (blockedReason != null)
                    throw new OutpatientEncounterValidationException(blockedReason);

                var patient = await _dbContext.Set<MstPatient>()
                    .FirstOrDefaultAsync(x => x.Id == encounter.PatientId && !x.IsDelete, cancellationToken);

                if (patient == null)
                    throw new OutpatientEncounterNotFoundException();

                patientId = patient.Id;

                SetIfChanged(patient.PhoneNumber, phone, v => patient.PhoneNumber = v, "PhoneNumber", changedFields);
                SetIfChanged(patient.WhatsAppNumber, whatsApp, v => patient.WhatsAppNumber = v, "WhatsAppNumber", changedFields);
                SetIfChanged(patient.Email, email, v => patient.Email = v, "Email", changedFields);
                SetIfChanged(patient.Address, address, v => patient.Address = v, "Address", changedFields);

                // null = tidak diubah, sama dengan PUT pasien (RJ-REG-DEC-006).
                if (request.Citizenship.HasValue &&
                    (patient.Citizenship != request.Citizenship || patient.NationalityCountryId != nationalityCountryId))
                {
                    patient.Citizenship = request.Citizenship;
                    patient.NationalityCountryId = nationalityCountryId;
                    changedFields.Add("Citizenship");
                }

                if (changedFields.Count > 0)
                {
                    patient.UpdateDateTime = now;
                    patient.UpdateBy = actorUserId;
                }

                // Ganti tanggal/sesi praktik dokter yang sama; dokter tidak dapat diganti.
                scheduleChange = await _scheduleService.ApplyAsync(
                    encounter, request.VisitDate, request.DoctorScheduleId, actorUserId, now, cancellationToken);
                if (scheduleChange != null)
                    changedFields.Add("Schedule");

                if (!string.Equals(encounter.Notes, notes, StringComparison.Ordinal))
                {
                    encounter.Notes = notes;
                    encounter.UpdateDateTime = now;
                    encounter.UpdateBy = actorUserId;
                    changedFields.Add("EncounterNotes");
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }

            if (scheduleChange != null)
                await _scheduleService.NotifyAsync(scheduleChange, actorUserId);

            if (changedFields.Count > 0)
            {
                // Tanpa nilai lama/baru kontak dan alamat (data pribadi); jadwal dicatat lengkap.
                await _loggerService.AuditAsync(LogCategory, "UpdateOutpatientEncounterRegistrationData",
                    "Data pendaftaran kunjungan rawat jalan diperbarui.",
                    new
                    {
                        EntityId = id,
                        PatientId = patientId,
                        Controller = "OutpatientEncounter",
                        Action = UpdateAction,
                        ActorUserId = actorUserId,
                        ChangedFields = changedFields,
                        OldEncounterDate = scheduleChange?.OldEncounterDate,
                        OldDoctorScheduleId = scheduleChange?.OldDoctorScheduleId,
                        NewEncounterDate = scheduleChange != null ? request.VisitDate : null,
                        NewDoctorScheduleId = scheduleChange != null ? request.DoctorScheduleId : null
                    });
            }

            return await GetDetailAsync(user, id, cancellationToken);
        }

        /// <summary>
        /// Alasan edit ditutup, atau <c>null</c> bila status mengizinkan: belum batal/selesai dan
        /// masih sebelum <c>Sedang Konsultasi</c> (RJ-REG-DEC-005).
        /// </summary>
        public static string? GetEditBlockedReason(RegPatientEncounter encounter)
        {
            if (encounter.IsCancel || encounter.CancelledAt.HasValue)
                return "Kunjungan sudah dibatalkan sehingga data pendaftaran tidak dapat diubah.";

            if (encounter.CompletedAt.HasValue || encounter.NoShowAt.HasValue ||
                encounter.EncounterStatus >= EncounterStatus.InConsultation)
                return $"Kunjungan dengan status {GetDisplayName(encounter.EncounterStatus)} tidak dapat diubah dari Daftar Pasien Rawat Jalan.";

            return null;
        }

        private string ResolveReferralStatus(RegPatientEncounter encounter, bool? referralIsComplete)
        {
            if (!encounter.IsReferral)
                return OutpatientReferralStatuses.NotReferral;

            if (referralIsComplete.HasValue)
                return referralIsComplete.Value ? OutpatientReferralStatuses.Complete : OutpatientReferralStatuses.Incomplete;

            return encounter.EncounterDate >= _encounterReferralService.ReferralDetailRequiredFrom
                ? OutpatientReferralStatuses.Incomplete
                : OutpatientReferralStatuses.Complete;
        }

        private static string? BuildScheduleLabel(string? sessionName, TimeSpan? start, TimeSpan? end, string? location)
        {
            if (!start.HasValue && sessionName == null && location == null)
                return null;

            var parts = new List<string?>
            {
                sessionName,
                start.HasValue && end.HasValue ? $"{start.Value:hh\\:mm}–{end.Value:hh\\:mm}" : null,
                location
            };

            return string.Join(" · ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        private static void SetIfChanged(string? current, string? next, Action<string?> apply, string field, List<string> changed)
        {
            if (string.Equals(current, next, StringComparison.Ordinal))
                return;

            apply(next);
            changed.Add(field);
        }

        private static void ValidateLength(string? value, int max, string label)
        {
            if (value != null && value.Length > max)
                throw new OutpatientEncounterValidationException($"{label} maksimal {max} karakter.");
        }

        private static string? NormalizeText(string? value)
        {
            var text = value?.Trim();
            return string.IsNullOrEmpty(text) ? null : text;
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }

        private static string GetDisplayName<TEnum>(TEnum value) where TEnum : struct, Enum
        {
            var member = typeof(TEnum).GetMember(value.ToString()).FirstOrDefault();
            return member?.GetCustomAttribute<DisplayAttribute>()?.Name ?? value.ToString();
        }
    }
}
