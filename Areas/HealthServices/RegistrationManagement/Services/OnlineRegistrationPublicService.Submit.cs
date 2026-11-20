using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Services.Notification;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>Satu berkas surat rujukan dari bagian <c>referralDocuments</c> permintaan multipart.</summary>
    public sealed record OnlineRegistrationUploadFile(string FileName, long Length, Func<Stream> OpenReadStream);

    /// <summary>
    /// Pengiriman pendaftaran online dari halaman publik (<c>BE-KSK-PO-005</c>, <c>FR-KSK-PO-012</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Urutan.</b> Bentuk isian diperiksa lebih dulu tanpa menyentuh database, lalu captcha, lalu
    /// pencocokan ulang pasien lama (<c>KSK-DSN-PO-005</c>), lalu jadwal, penjamin, dan faskes perujuk.
    /// Sesudah itu transaksi dibuka: kunci pasien, kunci kuota, cek ulang satu-per-tanggal, kunjungan
    /// aktif, dan kuota, baru kode dan nomor diterbitkan (<c>KSK-DSN-PO-009</c>, <c>NFR-PO-002</c>).
    /// </para>
    /// <para>
    /// <b>Kode.</b> Delapan karakter dari 31 karakter yang tidak mirip satu sama lain, tampil
    /// <c>XXXX-XXXX</c>. Hanya hash SHA-256 yang tersimpan, dan kode asli hanya ada di respons ini
    /// (<c>KSK-DSN-PO-002</c>, <c>INV-PO-05</c>).
    /// </para>
    /// <para>
    /// <b>Berkas.</b> Surat rujukan ditulis ke root privat <c>FileStorage:PrivateRootPath</c> di
    /// <c>online-registration-referrals/{id}/</c>, memakai aturan isi berkas dan root yang sama dengan
    /// <see cref="ReferralDocumentStorageService"/>. Bila transaksi gagal, berkas yang sudah tertulis
    /// dihapus kembali.
    /// </para>
    /// <para>
    /// <b>Email.</b> Dikirim sesudah transaksi tersimpan. Kegagalan kirim tidak membatalkan pendaftaran
    /// (<c>KSK-DEC-042</c>); statusnya dicatat di <c>EmailDeliveryStatus</c> dan dikembalikan sebagai
    /// <c>emailStatus</c>. Isi email tanpa NIK, No. RM, tanggal lahir, maupun penjamin.
    /// </para>
    /// <para>
    /// Log hanya memuat id, nomor pendaftaran, dan kode alasan penolakan. NIK, nama, tanggal lahir,
    /// kontak, dan kode tidak pernah dicatat.
    /// </para>
    /// </remarks>
    public partial class OnlineRegistrationPublicService
    {
        public const string RegistrationNumberPrefix = "PO";
        public const string RegistrationSequenceKey = "REG_ONLINE_REGISTRATION_NUMBER";

        /// <summary>Huruf dan angka yang tidak mirip satu sama lain: tanpa I, L, O, 0, dan 1.</summary>
        public const string CodeAlphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

        public const int CodeLength = 8;

        public const string IncompleteSubmissionMessage = "Data pendaftaran tidak lengkap. Periksa kembali isian Anda.";
        public const string FullNameRequiredMessage = "Nama lengkap wajib diisi.";
        public const string GenderRequiredMessage = "Pilih jenis kelamin.";
        public const string ContactRequiredMessage = "Isi No. WhatsApp atau email untuk menerima kode.";
        public const string WhatsAppInvalidMessage = "No. WhatsApp tidak valid. Contoh: 081234567890.";
        public const string EmailInvalidMessage = "Alamat email tidak valid.";
        public const string ScheduleUnavailableMessage = "Jadwal dokter tidak tersedia pada tanggal ini. Pilih jadwal lain.";
        public const string QuotaFullMessage = "Kuota dokter pada jadwal ini sudah penuh. Pilih jadwal atau tanggal lain.";
        public const string DuplicateRegistrationMessage = "Anda sudah memiliki pendaftaran online pada tanggal ini.";
        public const string ActiveEncounterMessage = "Anda sudah terdaftar pada tanggal ini.";
        public const string PayerUnavailableMessage = "Kartu penjamin tidak dapat dipakai. Pilih kartu lain atau Tunai.";
        public const string NewInsuranceIncompleteMessage = "Lengkapi penyedia asuransi dan nomor kartu.";
        public const string ReferralInstitutionRequiredMessage = "Pilih faskes perujuk.";
        public const string ReferralDateInFutureMessage = "Tanggal surat rujukan tidak boleh melewati hari ini.";
        public const string ConsentIncompleteMessage = "Lengkapi persetujuan umum dan tanda tangan.";
        public const string SignatureTooLargeMessage = "Tanda tangan terlalu besar. Silakan ulangi.";
        public const string SaveFailedMessage =
            "Pendaftaran belum dapat disimpan. Silakan coba lagi atau daftar di loket pendaftaran.";

        private const int RegistrationSequenceDigits = 4;
        private const int MaxCodeAttempts = 5;
        private const string PatientLockKeyPrefix = "REG_ONLINE_PATIENT_";
        private const string ReferralFolderName = "online-registration-referrals";
        private const int DefaultMaxReferralDocumentSizeMb = 5;
        private const string SignatureDataUrlPrefix = "data:image/png;base64,";

        // KSK-DEC-043 + KSK-DEC-022: bentuk lokal 08…, paling panjang 13 angka. Batas bawah setara
        // 9 digit bentuk 62… pada aturan No. HP kiosk (KSK-VAL-006).
        private const int MinWhatsAppDigits = 8;
        private const int MaxWhatsAppDigits = 13;

        private const int MaxFullNameLength = 150;
        private const int MaxEmailLength = 150;
        private const int MaxPolicyNumberLength = 50;
        private const int MaxHolderNameLength = 150;
        private const int MaxHolderRelationshipLength = 50;
        private const int MaxReferralNumberLength = 50;
        private const int MaxReferralDoctorNameLength = 150;
        private const int MaxReferralNoteLength = 500;
        private const int MinSignerNameLength = 3;
        private const int MaxSignerNameLength = 150;
        private const int MaxSignerRelationshipLength = 100;
        private const int MaxSignerIdentityLength = 50;
        private const int MaxSignerPhoneDigits = 13;
        private const int MaxSignerAddressLength = 500;

        /// <summary>
        /// Pelaku alokasi nomor untuk halaman publik yang tidak punya pengguna. Nilai sama dengan
        /// <c>SystemFallbackUserId</c> modul lain; baris pendaftaran sendiri tetap <c>CreateBy</c> kosong.
        /// </summary>
        private static readonly Guid SystemFallbackUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        private static readonly string[] IndonesianDayNames =
            { "Minggu", "Senin", "Selasa", "Rabu", "Kamis", "Jumat", "Sabtu" };

        private static readonly string[] IndonesianMonthNames =
        {
            "Januari", "Februari", "Maret", "April", "Mei", "Juni",
            "Juli", "Agustus", "September", "Oktober", "November", "Desember"
        };

        /// <summary>Kode dinormalkan: huruf besar, tanpa spasi dan tanda hubung. Dipakai juga check-in.</summary>
        public static string NormalizeCode(string? code) =>
            new((code ?? string.Empty)
                .Where(ch => !char.IsWhiteSpace(ch) && ch != '-')
                .Select(char.ToUpperInvariant)
                .ToArray());

        /// <summary>SHA-256 hex huruf kecil dari kode yang sudah dinormalkan, sama dengan helper RP.</summary>
        public static string ComputeCodeHash(string normalizedCode) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalizedCode))).ToLowerInvariant();

        /// <summary>
        /// No. WhatsApp ke bentuk lokal <c>08…</c>, atau <c>null</c> bila tidak sah. Spasi, titik, tanda
        /// hubung, kurung, dan awalan <c>+62</c>/<c>62</c> diterima. Contoh: <c>0812-3456-7890</c> menjadi
        /// <c>081234567890</c>.
        /// </summary>
        public static string? NormalizeWhatsAppNumber(string? value)
        {
            var trimmed = value?.Trim() ?? string.Empty;

            if (trimmed.Length == 0 || trimmed.LastIndexOf('+') > 0)
                return null;

            if (trimmed.Any(ch => !char.IsAsciiDigit(ch) && ch is not (' ' or '-' or '.' or '(' or ')' or '+')))
                return null;

            var digits = new string(trimmed.Where(char.IsAsciiDigit).ToArray());

            if (digits.StartsWith("62", StringComparison.Ordinal))
                digits = "0" + digits[2..];

            if (!digits.StartsWith("08", StringComparison.Ordinal) ||
                digits.Length < MinWhatsAppDigits ||
                digits.Length > MaxWhatsAppDigits)
            {
                return null;
            }

            return digits;
        }

        /// <summary>
        /// Menyimpan pendaftaran online berstatus Menunggu Check-in dan mengembalikan kodenya sekali.
        /// </summary>
        public async Task<OnlineRegistrationPublicResult<OnlineRegistrationSubmitResponse>> SubmitAsync(
            OnlineRegistrationSubmitRequest? request,
            IReadOnlyList<OnlineRegistrationUploadFile> files,
            string? remoteIp,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
                return Disabled<OnlineRegistrationSubmitResponse>();

            if (request == null)
                return RejectSubmit(OnlineRegistrationPublicOutcome.Invalid, IncompleteSubmissionMessage, "PAYLOAD");

            var draft = new SubmitDraft();
            var shapeError = ValidateSubmitShape(request, files.Count, draft);
            if (shapeError != null)
                return RejectSubmit(OnlineRegistrationPublicOutcome.Invalid, shapeError.Value.Message, shapeError.Value.Rule);

            var documentError = await ReadReferralDocumentsAsync(files, draft, cancellationToken);
            if (documentError != null)
                return RejectSubmit(OnlineRegistrationPublicOutcome.Invalid, documentError.Value.Message, documentError.Value.Rule);

            var captcha = await _turnstileService.VerifyAsync(request.TurnstileToken, remoteIp, cancellationToken);
            if (captcha == TurnstileVerificationOutcome.Rejected)
                return RejectSubmit(OnlineRegistrationPublicOutcome.Invalid, CaptchaRejectedMessage, "KSK-VAL-PO-010");

            if (captcha == TurnstileVerificationOutcome.Unavailable)
                return RejectSubmit(OnlineRegistrationPublicOutcome.Unavailable, ServiceUnavailableMessage, "KSK-VAL-PO-011");

            if (draft.PatientMode == OnlineRegistrationPatientMode.Existing)
            {
                draft.PatientId = await FindExistingPatientIdAsync(
                    draft.ExistingIdentityType,
                    draft.ExistingIdentityValue,
                    draft.BirthDate,
                    cancellationToken);

                if (draft.PatientId == null)
                    return RejectSubmit(OnlineRegistrationPublicOutcome.NotFound, PatientNotFoundMessage, "KSK-VAL-PO-004");
            }

            var schedule = (await LoadOfferedSchedulesAsync(draft.ClinicId, draft.VisitDate, cancellationToken))
                .FirstOrDefault(x => x.Id == draft.DoctorScheduleId);

            if (schedule == null)
                return RejectSubmit(OnlineRegistrationPublicOutcome.Conflict, ScheduleUnavailableMessage, "KSK-VAL-PO-021");

            var referenceError = await ValidateReferencesAsync(draft, cancellationToken);
            if (referenceError != null)
                return RejectSubmit(OnlineRegistrationPublicOutcome.Invalid, referenceError.Value.Message, referenceError.Value.Rule);

            string? privateRoot = null;
            if (draft.Documents.Count > 0)
            {
                var rootError = _referralDocumentStorage.TryResolvePrivateRoot(out var resolvedRoot);
                if (rootError != null)
                {
                    _logger.LogError("Pendaftaran online: penyimpanan surat rujukan privat belum siap (FileStorage:PrivateRootPath).");
                    return RejectSubmit(OnlineRegistrationPublicOutcome.Unavailable, SaveFailedMessage, "STORAGE");
                }

                privateRoot = resolvedRoot;
            }

            return await SaveSubmissionAsync(draft, schedule, privateRoot, cancellationToken);
        }

        private async Task<OnlineRegistrationPublicResult<OnlineRegistrationSubmitResponse>> SaveSubmissionAsync(
            SubmitDraft draft,
            MstDoctorSchedule schedule,
            string? privateRoot,
            CancellationToken cancellationToken)
        {
            var registrationId = Guid.NewGuid();
            var written = new List<string>();
            string code;
            RegOnlineRegistration registration;

            try
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

                // Urutan kunci sama di semua jalur: pasien dulu, baru kuota (NFR-PO-002).
                await LockOnlinePatientAsync(draft, cancellationToken);
                await _quotaService.AcquireQuotaLockAsync(schedule.Id, draft.VisitDate, cancellationToken);

                if (await HasOpenRegistrationAsync(draft, cancellationToken))
                    return RejectSubmit(OnlineRegistrationPublicOutcome.Conflict, DuplicateRegistrationMessage, "KSK-VAL-PO-023");

                if (draft.PatientId.HasValue &&
                    OutpatientEncounterRules.IsActiveEncounterBlockEnabled(_configuration) &&
                    await HasActiveOutpatientEncounterOnAsync(draft.PatientId.Value, draft.VisitDate, cancellationToken))
                {
                    return RejectSubmit(OnlineRegistrationPublicOutcome.Conflict, ActiveEncounterMessage, "KSK-VAL-PO-024");
                }

                var usage = await _quotaService.GetUsageAsync(
                    schedule.Id,
                    DateTime.SpecifyKind(draft.VisitDate, DateTimeKind.Utc),
                    cancellationToken);

                if (DoctorScheduleQuotaService.IsFullForAppointment(schedule, usage))
                    return RejectSubmit(OnlineRegistrationPublicOutcome.Conflict, QuotaFullMessage, "KSK-VAL-PO-022");

                var issuedCode = await IssueUniqueCodeAsync(cancellationToken);
                if (issuedCode == null)
                {
                    _logger.LogError("Pendaftaran online: kode unik tidak didapat setelah {Attempts} percobaan.", MaxCodeAttempts);
                    return RejectSubmit(OnlineRegistrationPublicOutcome.Unavailable, SaveFailedMessage, "CODE");
                }

                code = issuedCode.Value.Code;
                var now = DateTime.UtcNow;
                string registrationNumber;

                try
                {
                    registrationNumber = await _numberSeriesAllocator.AllocateAsync(
                        new NumberAllocationRequest(
                            SequenceKey: RegistrationSequenceKey,
                            Prefix: RegistrationNumberPrefix,
                            ResetPolicy: NumberSeriesResetPolicies.Daily,
                            SequenceDigits: RegistrationSequenceDigits,
                            ActorUserId: SystemFallbackUserId,
                            Instant: now),
                        cancellationToken);
                }
                catch (NumberSeriesAllocationException exception)
                {
                    _logger.LogError(
                        "Pendaftaran online: nomor gagal diterbitkan. ValidationCode={ValidationCode}",
                        exception.ValidationCode);
                    return RejectSubmit(OnlineRegistrationPublicOutcome.Unavailable, SaveFailedMessage, "NUMBER");
                }

                registration = BuildRegistration(draft, registrationId, registrationNumber, issuedCode.Value.Hash, now);
                _dbContext.RegOnlineRegistrations.Add(registration);

                if (draft.Documents.Count > 0)
                {
                    var folder = Path.Combine(privateRoot!, ReferralFolderName, registrationId.ToString("N"));
                    Directory.CreateDirectory(folder);
                    var pageOrder = 0;

                    foreach (var document in draft.Documents)
                    {
                        var documentId = Guid.NewGuid();
                        var fileName = $"{documentId:N}{document.Extension}";
                        var physical = Path.Combine(folder, fileName);

                        await File.WriteAllBytesAsync(physical, document.Bytes, cancellationToken);
                        written.Add(physical);
                        pageOrder++;

                        _dbContext.RegOnlineRegistrationReferralDocuments.Add(new RegOnlineRegistrationReferralDocument
                        {
                            Id = documentId,
                            OnlineRegistrationId = registrationId,
                            OriginalFileName = ReferralDocumentStorageService.SanitizeFileName(document.FileName, document.Extension),
                            ContentType = document.ContentType,
                            SizeBytes = document.Bytes.LongLength,
                            StoragePath = $"{ReferralFolderName}/{registrationId:N}/{fileName}",
                            PageOrder = pageOrder,
                            CreateDateTime = now
                        });
                    }
                }

                _dbContext.RegOnlineRegistrationHistories.Add(new RegOnlineRegistrationHistory
                {
                    Id = Guid.NewGuid(),
                    OnlineRegistrationId = registrationId,
                    EventCode = RegOnlineRegistrationHistory.EventSubmitted,
                    FromStatus = null,
                    ToStatus = OnlineRegistrationStatus.AwaitingCheckIn,
                    Source = OnlineRegistrationEventSource.PublicPage,
                    ActorUserId = null,
                    ToVisitDate = registration.VisitDate,
                    ToDoctorScheduleId = registration.DoctorScheduleId,
                    OccurredAt = now,
                    CreateDateTime = now
                });

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException exception) when (IsUniqueViolation(exception))
            {
                ReferralDocumentStorageService.DeleteFiles(written);
                _dbContext.ChangeTracker.Clear();
                return RejectSubmit(OnlineRegistrationPublicOutcome.Conflict, SaveFailedMessage, "UNIQUE");
            }
            catch
            {
                // Transaksi gagal: berkas yang sudah tertulis tidak boleh tertinggal yatim.
                ReferralDocumentStorageService.DeleteFiles(written);
                throw;
            }

            _logger.LogInformation(
                "Pendaftaran online tersimpan. RegistrationId={RegistrationId} RegistrationNumber={RegistrationNumber} PatientMode={PatientMode} DocumentCount={DocumentCount}",
                registration.Id,
                registration.RegistrationNumber,
                registration.PatientMode,
                draft.Documents.Count);

            var emailStatus = await SendCodeEmailAsync(registration, schedule, code);

            return OnlineRegistrationPublicResult<OnlineRegistrationSubmitResponse>.Success(
                new OnlineRegistrationSubmitResponse
                {
                    RegistrationNumber = registration.RegistrationNumber,
                    Code = FormatCode(code),
                    VisitDate = registration.VisitDate.ToString("yyyy-MM-dd"),
                    ClinicName = schedule.Clinic?.ClinicName ?? string.Empty,
                    DoctorName = schedule.Doctor?.FullName ?? string.Empty,
                    StartTime = schedule.StartTime.ToString(@"hh\:mm"),
                    EndTime = schedule.EndTime.ToString(@"hh\:mm"),
                    EmailStatus = emailStatus,
                    WhatsAppNumber = registration.ContactWhatsAppNumber
                },
                "Pendaftaran online berhasil disimpan.");
        }

        // ------------------------------------------------------------------
        // Validasi bentuk isian (tanpa database)
        // ------------------------------------------------------------------

        private readonly record struct SubmitError(string Rule, string Message);

        private sealed class SubmitDocument
        {
            public string FileName { get; init; } = string.Empty;
            public string ContentType { get; init; } = string.Empty;
            public string Extension { get; init; } = string.Empty;
            public byte[] Bytes { get; init; } = Array.Empty<byte>();
        }

        /// <summary>Isian yang sudah dinormalkan dan lolos validasi bentuk.</summary>
        private sealed class SubmitDraft
        {
            public OnlineRegistrationPatientMode PatientMode { get; set; }
            public OnlineRegistrationIdentityType ExistingIdentityType { get; set; }
            public string ExistingIdentityValue { get; set; } = string.Empty;
            public Guid? PatientId { get; set; }
            public string? IdentityNumber { get; set; }
            public string? FullName { get; set; }
            public DateTime BirthDate { get; set; }
            public Gender? Gender { get; set; }
            public string? WhatsAppNumber { get; set; }
            public string? Email { get; set; }
            public DateTime VisitDate { get; set; }
            public Guid ClinicId { get; set; }
            public Guid DoctorScheduleId { get; set; }
            public OnlineRegistrationPatientType PatientType { get; set; }
            public EncounterPaymentType PaymentType { get; set; }
            public Guid? PatientInsuranceId { get; set; }
            public Guid? PatientCompanyGuarantorId { get; set; }
            public Guid? NewInsuranceProviderId { get; set; }
            public string? NewInsurancePolicyNumber { get; set; }
            public string? NewInsuranceHolderName { get; set; }
            public string? NewInsuranceHolderRelationship { get; set; }
            public bool IsReferral { get; set; }
            public Guid? ReferralInstitutionId { get; set; }
            public DateTime? ReferralDate { get; set; }
            public string? ReferralNumber { get; set; }
            public string? ReferralDoctorName { get; set; }
            public string? ReferralDiagnosisNote { get; set; }
            public string? ReferralReason { get; set; }
            public bool HasConsent { get; set; }
            public GeneralConsentSignerType? ConsentSignerType { get; set; }
            public string? ConsentSignerName { get; set; }
            public string? ConsentSignerRelationshipText { get; set; }
            public string? ConsentSignerIdentityNumber { get; set; }
            public string? ConsentSignerPhoneNumber { get; set; }
            public string? ConsentSignerAddress { get; set; }
            public int[] ConsentAgreedClauses { get; set; } = Array.Empty<int>();
            public string? ConsentSignatureImageBase64 { get; set; }
            public List<SubmitDocument> Documents { get; } = new();
        }

        private SubmitError? ValidateSubmitShape(OnlineRegistrationSubmitRequest request, int fileCount, SubmitDraft draft)
        {
            return ValidatePatient(request, draft)
                ?? ValidateContact(request.Contact, draft)
                ?? ValidateVisit(request, draft)
                ?? ValidatePayer(request, draft)
                ?? ValidateReferral(request, fileCount, draft)
                ?? ValidateConsent(request, draft);
        }

        private static SubmitError? ValidatePatient(OnlineRegistrationSubmitRequest request, SubmitDraft draft)
        {
            if (request.PatientMode == null || !Enum.IsDefined(request.PatientMode.Value))
                return new SubmitError("PATIENT_MODE", IncompleteSubmissionMessage);

            draft.PatientMode = request.PatientMode.Value;

            if (draft.PatientMode == OnlineRegistrationPatientMode.Existing)
            {
                if (request.NewPatient != null)
                    return new SubmitError("PATIENT_MODE", IncompleteSubmissionMessage);

                var existing = request.ExistingPatient;
                var identityError = ValidateIdentity(existing?.IdentityType, existing?.IdentityValue, existing?.BirthDate);
                if (identityError != null)
                    return new SubmitError("KSK-VAL-PO-001-003", identityError);

                draft.ExistingIdentityType = existing!.IdentityType!.Value;
                draft.ExistingIdentityValue = existing.IdentityValue!.Trim();
                draft.BirthDate = existing.BirthDate!.Value.Date;
                return null;
            }

            if (request.ExistingPatient != null)
                return new SubmitError("PATIENT_MODE", IncompleteSubmissionMessage);

            var newPatient = request.NewPatient;
            var nikError = ValidateIdentity(
                OnlineRegistrationIdentityType.IdentityNumber,
                newPatient?.IdentityNumber,
                newPatient?.BirthDate);

            if (nikError != null)
                return new SubmitError("KSK-VAL-PO-001-003", nikError);

            var fullName = newPatient!.FullName?.Trim() ?? string.Empty;
            if (fullName.Length == 0 || fullName.Length > MaxFullNameLength)
                return new SubmitError("KSK-VAL-PO-005", FullNameRequiredMessage);

            if (newPatient.Gender is not (Gender.Male or Gender.Female))
                return new SubmitError("KSK-VAL-PO-006", GenderRequiredMessage);

            draft.IdentityNumber = newPatient.IdentityNumber!.Trim();
            draft.FullName = fullName;
            draft.BirthDate = newPatient.BirthDate!.Value.Date;
            draft.Gender = newPatient.Gender;
            return null;
        }

        private static SubmitError? ValidateContact(string? contact, SubmitDraft draft)
        {
            var value = contact?.Trim() ?? string.Empty;

            if (value.Length == 0)
                return new SubmitError("KSK-VAL-PO-007", ContactRequiredMessage);

            if (value.Contains('@'))
            {
                if (value.Length > MaxEmailLength || !IsValidEmail(value))
                    return new SubmitError("KSK-VAL-PO-009", EmailInvalidMessage);

                draft.Email = value;
                return null;
            }

            var whatsApp = NormalizeWhatsAppNumber(value);
            if (whatsApp == null)
                return new SubmitError("KSK-VAL-PO-008", WhatsAppInvalidMessage);

            draft.WhatsAppNumber = whatsApp;
            return null;
        }

        private SubmitError? ValidateVisit(OnlineRegistrationSubmitRequest request, SubmitDraft draft)
        {
            if (!request.VisitDate.HasValue || !IsVisitDateAllowed(request.VisitDate.Value))
                return new SubmitError("KSK-VAL-PO-020", VisitDateOutOfRangeMessage);

            if (!request.ClinicId.HasValue || request.ClinicId.Value == Guid.Empty ||
                !request.DoctorScheduleId.HasValue || request.DoctorScheduleId.Value == Guid.Empty)
            {
                return new SubmitError("VISIT", "Pilih poli dan jadwal dokter.");
            }

            draft.VisitDate = ToCalendarDate(request.VisitDate.Value);
            draft.ClinicId = request.ClinicId.Value;
            draft.DoctorScheduleId = request.DoctorScheduleId.Value;
            return null;
        }

        /// <summary>
        /// Jenis pasien dan penjamin (<c>KSK-DSN-PO-012</c>): Pribadi hanya Tunai; Asuransi memakai
        /// kartu milik pasien, penjamin perusahaan milik pasien, atau kartu asuransi baru; Rujukan boleh
        /// ketiganya ditambah Tunai, sama dengan step Penjamin rujukan menu RJ.
        /// </summary>
        private static SubmitError? ValidatePayer(OnlineRegistrationSubmitRequest request, SubmitDraft draft)
        {
            if (request.PatientType == null || !Enum.IsDefined(request.PatientType.Value))
                return new SubmitError("PATIENT_TYPE", "Pilih jenis pasien.");

            if (request.PaymentType == null || !Enum.IsDefined(request.PaymentType.Value))
                return new SubmitError("PAYMENT_TYPE", "Pilih penjamin pembayaran.");

            var patientType = request.PatientType.Value;
            var paymentType = request.PaymentType.Value;
            const string payerMismatch = "Penjamin tidak sesuai dengan jenis pasien.";

            if (patientType == OnlineRegistrationPatientType.Personal && paymentType != EncounterPaymentType.Cash)
                return new SubmitError("PAYMENT_TYPE", payerMismatch);

            if (patientType == OnlineRegistrationPatientType.Insurance && paymentType == EncounterPaymentType.Cash)
                return new SubmitError("PAYMENT_TYPE", payerMismatch);

            var insuranceId = NullIfEmpty(request.PatientInsuranceId);
            var companyId = NullIfEmpty(request.PatientCompanyGuarantorId);
            var newInsurance = request.NewInsurance;
            var isExisting = draft.PatientMode == OnlineRegistrationPatientMode.Existing;

            switch (paymentType)
            {
                case EncounterPaymentType.Cash:
                    if (insuranceId != null || companyId != null || newInsurance != null)
                        return new SubmitError("PAYMENT_TYPE", payerMismatch);
                    break;

                case EncounterPaymentType.Insurance:
                    if (companyId != null || (insuranceId == null) == (newInsurance == null))
                        return new SubmitError("KSK-VAL-PO-025", PayerUnavailableMessage);

                    // Pasien baru belum punya kartu apa pun di master.
                    if (insuranceId != null && !isExisting)
                        return new SubmitError("KSK-VAL-PO-025", PayerUnavailableMessage);

                    if (newInsurance != null)
                    {
                        var policyNumber = newInsurance.PolicyNumber?.Trim() ?? string.Empty;
                        var holderName = CleanText(newInsurance.HolderName);
                        var holderRelationship = CleanText(newInsurance.HolderRelationship);

                        if (NullIfEmpty(newInsurance.InsuranceProviderId) == null ||
                            policyNumber.Length == 0 ||
                            policyNumber.Length > MaxPolicyNumberLength ||
                            holderName?.Length > MaxHolderNameLength ||
                            holderRelationship?.Length > MaxHolderRelationshipLength)
                        {
                            return new SubmitError("KSK-VAL-PO-026", NewInsuranceIncompleteMessage);
                        }

                        draft.NewInsuranceProviderId = newInsurance.InsuranceProviderId;
                        draft.NewInsurancePolicyNumber = policyNumber;
                        draft.NewInsuranceHolderName = holderName;
                        draft.NewInsuranceHolderRelationship = holderRelationship;
                    }

                    draft.PatientInsuranceId = insuranceId;
                    break;

                case EncounterPaymentType.CompanyGuarantor:
                    if (companyId == null || insuranceId != null || newInsurance != null || !isExisting)
                        return new SubmitError("KSK-VAL-PO-025", PayerUnavailableMessage);

                    draft.PatientCompanyGuarantorId = companyId;
                    break;
            }

            draft.PatientType = patientType;
            draft.PaymentType = paymentType;
            return null;
        }

        /// <summary>Rujukan eksternal saja (<c>KSK-DSN-PO-011</c>), 1 sampai batas berkas.</summary>
        private SubmitError? ValidateReferral(OnlineRegistrationSubmitRequest request, int fileCount, SubmitDraft draft)
        {
            draft.IsReferral = draft.PatientType == OnlineRegistrationPatientType.Referral;

            if (!draft.IsReferral)
            {
                return request.Referral != null || fileCount > 0
                    ? new SubmitError("REFERRAL", "Data rujukan hanya untuk Pasien Rujukan.")
                    : null;
            }

            var referral = request.Referral;
            if (referral == null || NullIfEmpty(referral.ReferralInstitutionId) == null)
                return new SubmitError("KSK-VAL-PO-027", ReferralInstitutionRequiredMessage);

            if (referral.ReferralDate.HasValue && referral.ReferralDate.Value.Date > AppDateTimeHelper.OperationalDate())
                return new SubmitError("KSK-VAL-PO-028", ReferralDateInFutureMessage);

            var referralNumber = CleanText(referral.ReferralNumber);
            var doctorName = CleanText(referral.ReferralDoctorName);
            var diagnosisNote = CleanText(referral.DiagnosisNote);
            var reason = CleanText(referral.Reason);

            if (referralNumber?.Length > MaxReferralNumberLength)
                return new SubmitError("REFERRAL", $"Nomor rujukan maksimal {MaxReferralNumberLength} karakter.");

            if (doctorName?.Length > MaxReferralDoctorNameLength)
                return new SubmitError("REFERRAL", $"Nama dokter perujuk maksimal {MaxReferralDoctorNameLength} karakter.");

            if (diagnosisNote?.Length > MaxReferralNoteLength || reason?.Length > MaxReferralNoteLength)
                return new SubmitError("REFERRAL", $"Catatan diagnosa dan alasan rujukan maksimal {MaxReferralNoteLength} karakter.");

            if (fileCount < 1 || fileCount > MaxReferralDocuments)
                return new SubmitError("KSK-VAL-PO-030", DocumentFormatMessage);

            draft.ReferralInstitutionId = referral.ReferralInstitutionId;
            draft.ReferralDate = referral.ReferralDate.HasValue ? ToCalendarDate(referral.ReferralDate.Value) : null;
            draft.ReferralNumber = referralNumber;
            draft.ReferralDoctorName = doctorName;
            draft.ReferralDiagnosisNote = diagnosisNote;
            draft.ReferralReason = reason;
            return null;
        }

        /// <summary>
        /// General Consent hanya untuk pasien baru (<c>KSK-DEC-034</c>), dengan aturan isi yang sama
        /// dengan menu RJ: seluruh klausul 1..<see cref="OutpatientGeneralConsentService.ClauseCount"/>
        /// dan tanda tangan PNG.
        /// </summary>
        private SubmitError? ValidateConsent(OnlineRegistrationSubmitRequest request, SubmitDraft draft)
        {
            var consent = request.Consent;

            if (draft.PatientMode != OnlineRegistrationPatientMode.New)
            {
                return consent != null
                    ? new SubmitError("CONSENT", "Persetujuan umum hanya diisi untuk pasien baru.")
                    : null;
            }

            if (consent == null ||
                consent.SignerType == null ||
                !Enum.IsDefined(consent.SignerType.Value))
            {
                return new SubmitError("KSK-VAL-PO-031", ConsentIncompleteMessage);
            }

            var signerName = consent.SignerName?.Trim() ?? string.Empty;
            if (signerName.Length < MinSignerNameLength)
                return new SubmitError("KSK-VAL-PO-031", ConsentIncompleteMessage);

            if (signerName.Length > MaxSignerNameLength)
                return new SubmitError("CONSENT", $"Nama penanda tangan maksimal {MaxSignerNameLength} karakter.");

            var relationship = CleanText(consent.SignerRelationshipText);
            var identityNumber = CleanText(consent.SignerIdentityNumber);
            var phone = CleanText(consent.SignerPhoneNumber);
            var address = CleanText(consent.SignerAddress);

            if (relationship?.Length > MaxSignerRelationshipLength ||
                identityNumber?.Length > MaxSignerIdentityLength ||
                address?.Length > MaxSignerAddressLength)
            {
                return new SubmitError("CONSENT", "Data penanda tangan melebihi panjang yang diizinkan.");
            }

            if (phone != null && (phone.Length > MaxSignerPhoneDigits || !phone.All(char.IsAsciiDigit)))
                return new SubmitError("CONSENT", "Nomor telepon penanda tangan hanya angka, maksimal 13 digit.");

            var clauses = (consent.AgreedClauses ?? new List<int>()).Distinct().OrderBy(x => x).ToArray();
            if (!clauses.SequenceEqual(Enumerable.Range(1, OutpatientGeneralConsentService.ClauseCount)))
                return new SubmitError("KSK-VAL-PO-031", ConsentIncompleteMessage);

            var signature = consent.SignatureImageBase64?.Trim() ?? string.Empty;
            if (signature.Length == 0)
                return new SubmitError("KSK-VAL-PO-031", ConsentIncompleteMessage);

            // Ukuran gambar dihitung dari panjang base64 sebelum didekode (3 byte per 4 karakter).
            var payloadLength = signature.StartsWith(SignatureDataUrlPrefix, StringComparison.OrdinalIgnoreCase)
                ? signature.Length - SignatureDataUrlPrefix.Length
                : signature.Length;

            if (payloadLength * 3L / 4L > Math.Max(1, _options.MaxSignatureKb) * 1024L)
                return new SubmitError("KSK-VAL-PO-032", SignatureTooLargeMessage);

            if (!OutpatientGeneralConsentService.IsPngDataUrl(signature))
                return new SubmitError("KSK-VAL-PO-031", ConsentIncompleteMessage);

            draft.HasConsent = true;
            draft.ConsentSignerType = consent.SignerType;
            draft.ConsentSignerName = signerName;
            draft.ConsentSignerRelationshipText = relationship;
            draft.ConsentSignerIdentityNumber = identityNumber;
            draft.ConsentSignerPhoneNumber = phone;
            draft.ConsentSignerAddress = address;
            draft.ConsentAgreedClauses = clauses;
            draft.ConsentSignatureImageBase64 = signature;
            return null;
        }

        private int MaxReferralDocuments => Math.Max(1, _options.MaxReferralDocuments);

        private string DocumentFormatMessage =>
            $"Unggah PDF, JPG, atau PNG, paling banyak {MaxReferralDocuments} berkas.";

        /// <summary>
        /// Membaca berkas ke memori dan memeriksa ukuran serta isinya (magic bytes) dengan aturan yang
        /// sama dengan <see cref="ReferralDocumentStorageService"/> (<c>KSK-VAL-PO-029</c>, <c>030</c>).
        /// </summary>
        private async Task<SubmitError?> ReadReferralDocumentsAsync(
            IReadOnlyList<OnlineRegistrationUploadFile> files,
            SubmitDraft draft,
            CancellationToken cancellationToken)
        {
            if (files.Count == 0)
                return null;

            var maxSizeMb = _configuration.GetValue<int?>("FileStorage:MaxReferralDocumentSizeMb") ?? DefaultMaxReferralDocumentSizeMb;
            var maxBytes = Math.Max(1, maxSizeMb) * 1024L * 1024L;

            if (files.Any(f => f.Length > maxBytes))
                return new SubmitError("KSK-VAL-PO-029", $"Ukuran berkas maksimal {maxSizeMb} MB.");

            foreach (var file in files)
            {
                if (file.Length <= 0)
                    return new SubmitError("KSK-VAL-PO-030", DocumentFormatMessage);

                await using var input = file.OpenReadStream();
                using var buffer = new MemoryStream();
                await input.CopyToAsync(buffer, cancellationToken);

                var bytes = buffer.ToArray();
                if (bytes.LongLength > maxBytes)
                    return new SubmitError("KSK-VAL-PO-029", $"Ukuran berkas maksimal {maxSizeMb} MB.");

                var detected = ReferralDocumentStorageService.DetectContentType(bytes);
                if (detected == null)
                    return new SubmitError("KSK-VAL-PO-030", DocumentFormatMessage);

                draft.Documents.Add(new SubmitDocument
                {
                    FileName = file.FileName,
                    ContentType = detected.Value.ContentType,
                    Extension = detected.Value.Extension,
                    Bytes = bytes
                });
            }

            return null;
        }

        // ------------------------------------------------------------------
        // Pemeriksaan data acuan (baca database, sebelum transaksi)
        // ------------------------------------------------------------------

        private async Task<SubmitError?> ValidateReferencesAsync(SubmitDraft draft, CancellationToken cancellationToken)
        {
            if (draft.PatientInsuranceId.HasValue || draft.PatientCompanyGuarantorId.HasValue)
            {
                // Kartu harus ada di daftar penjamin yang sama dengan hasil pencocokan (aktif, eligible,
                // berlaku, milik pasien ini, bukan Benefit Karyawan).
                var payers = await LoadMaskedPayersAsync(draft.PatientId!.Value, cancellationToken);
                var owned = draft.PatientInsuranceId.HasValue
                    ? payers.Any(x => x.PayerKind == OnlineRegistrationPayerKind.Insurance && x.Id == draft.PatientInsuranceId.Value)
                    : payers.Any(x => x.PayerKind == OnlineRegistrationPayerKind.Company && x.Id == draft.PatientCompanyGuarantorId!.Value);

                if (!owned)
                    return new SubmitError("KSK-VAL-PO-025", PayerUnavailableMessage);
            }

            if (draft.NewInsuranceProviderId.HasValue &&
                !await QueryActiveInsuranceProviders().AnyAsync(x => x.Id == draft.NewInsuranceProviderId.Value, cancellationToken))
            {
                return new SubmitError("KSK-VAL-PO-026", NewInsuranceIncompleteMessage);
            }

            if (draft.ReferralInstitutionId.HasValue &&
                !await _dbContext.Set<MstReferralInstitution>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == draft.ReferralInstitutionId.Value && x.IsActive && !x.IsDelete, cancellationToken))
            {
                return new SubmitError("KSK-VAL-PO-027", ReferralInstitutionRequiredMessage);
            }

            return null;
        }

        // ------------------------------------------------------------------
        // Di dalam transaksi
        // ------------------------------------------------------------------

        /// <summary>
        /// Kunci per pasien (<c>REG_ONLINE_PATIENT_{PatientId atau NIK}</c>), sehingga dua kiriman
        /// bersamaan untuk pasien yang sama diproses bergantian dan yang kedua ditolak
        /// <c>KSK-VAL-PO-023</c>.
        /// </summary>
        private async Task LockOnlinePatientAsync(SubmitDraft draft, CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsNpgsql())
                return;

            var key = draft.PatientId.HasValue
                ? $"{PatientLockKeyPrefix}{draft.PatientId.Value:N}"
                : $"{PatientLockKeyPrefix}NIK_{draft.IdentityNumber}";

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [key],
                cancellationToken);
        }

        /// <summary>
        /// <c>INV-PO-03</c>: satu pendaftaran Menunggu Check-in per pasien per tanggal. Pasien lama
        /// dikenali dari <c>PatientId</c> dan NIK masternya; pasien baru dari NIK, termasuk pendaftaran
        /// pasien lama yang NIK masternya sama.
        /// </summary>
        private async Task<bool> HasOpenRegistrationAsync(SubmitDraft draft, CancellationToken cancellationToken)
        {
            var visitDate = draft.VisitDate;
            var open = _dbContext.RegOnlineRegistrations
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.Status == OnlineRegistrationStatus.AwaitingCheckIn && x.VisitDate == visitDate);

            if (draft.PatientId.HasValue)
            {
                var patientId = draft.PatientId.Value;
                var masterIdentityNumber = await _dbContext.Set<MstPatient>()
                    .AsNoTracking()
                    .Where(x => x.Id == patientId)
                    .Select(x => x.IdentityNumber)
                    .FirstOrDefaultAsync(cancellationToken);

                var nik = string.IsNullOrWhiteSpace(masterIdentityNumber) ? null : masterIdentityNumber.Trim();

                return await open.AnyAsync(
                    x => x.PatientId == patientId || (nik != null && x.IdentityNumber == nik),
                    cancellationToken);
            }

            var identityNumber = draft.IdentityNumber!;
            var masterPatientIds = await _patientLookupService.FindPatientIdsByIdentityNumberAsync(identityNumber, cancellationToken);

            return await open.AnyAsync(
                x => x.IdentityNumber == identityNumber ||
                     (x.PatientId != null && masterPatientIds.Contains(x.PatientId.Value)),
                cancellationToken);
        }

        /// <summary>
        /// <c>KSK-VAL-PO-024</c>: kunjungan Rawat Jalan aktif pada tanggal itu, aturan yang sama dengan
        /// Reservasi Prioritas (<c>OutpatientEncounterRules.WhereBlocksRegistration</c>).
        /// </summary>
        private Task<bool> HasActiveOutpatientEncounterOnAsync(
            Guid patientId,
            DateTime visitDate,
            CancellationToken cancellationToken)
        {
            var start = DateTime.SpecifyKind(visitDate.Date, DateTimeKind.Utc);
            var end = start.AddDays(1);

            return _dbContext.Set<RegPatientEncounter>()
                .AsNoTracking()
                .WhereBlocksRegistration(_dbContext)
                .AnyAsync(x =>
                    x.PatientId == patientId &&
                    x.EncounterDate >= start &&
                    x.EncounterDate < end,
                    cancellationToken);
        }

        /// <summary>
        /// Kode acak yang hash-nya belum dipakai pendaftaran Menunggu Check-in lain. Index unik
        /// bersyarat tetap menjadi penjaga terakhir.
        /// </summary>
        private async Task<(string Code, string Hash)?> IssueUniqueCodeAsync(CancellationToken cancellationToken)
        {
            for (var attempt = 0; attempt < MaxCodeAttempts; attempt++)
            {
                var code = GenerateCode();
                var hash = ComputeCodeHash(code);

                var taken = await _dbContext.RegOnlineRegistrations
                    .AsNoTracking()
                    .AnyAsync(x => x.CodeHash == hash && x.Status == OnlineRegistrationStatus.AwaitingCheckIn, cancellationToken);

                if (!taken)
                    return (code, hash);
            }

            return null;
        }

        private static string GenerateCode()
        {
            Span<char> chars = stackalloc char[CodeLength];

            for (var i = 0; i < chars.Length; i++)
                chars[i] = CodeAlphabet[RandomNumberGenerator.GetInt32(CodeAlphabet.Length)];

            return new string(chars);
        }

        private static string FormatCode(string normalizedCode) =>
            $"{normalizedCode[..4]}-{normalizedCode[4..]}";

        private static RegOnlineRegistration BuildRegistration(
            SubmitDraft draft,
            Guid registrationId,
            string registrationNumber,
            string codeHash,
            DateTime now)
        {
            var isNew = draft.PatientMode == OnlineRegistrationPatientMode.New;

            return new RegOnlineRegistration
            {
                Id = registrationId,
                RegistrationNumber = registrationNumber,
                CodeHash = codeHash,
                Status = OnlineRegistrationStatus.AwaitingCheckIn,
                PatientMode = draft.PatientMode,
                PatientId = draft.PatientId,
                IdentityNumber = isNew ? draft.IdentityNumber : null,
                FullName = isNew ? draft.FullName : null,
                BirthDate = isNew ? ToCalendarDate(draft.BirthDate) : null,
                Gender = isNew ? draft.Gender : null,
                VisitDate = draft.VisitDate,
                ClinicId = draft.ClinicId,
                DoctorScheduleId = draft.DoctorScheduleId,
                PatientType = draft.PatientType,
                PaymentType = draft.PaymentType,
                PatientInsuranceId = draft.PatientInsuranceId,
                PatientCompanyGuarantorId = draft.PatientCompanyGuarantorId,
                NewInsuranceProviderId = draft.NewInsuranceProviderId,
                NewInsurancePolicyNumber = draft.NewInsurancePolicyNumber,
                NewInsuranceHolderName = draft.NewInsuranceHolderName,
                NewInsuranceHolderRelationship = draft.NewInsuranceHolderRelationship,
                IsReferral = draft.IsReferral,
                ReferralInstitutionId = draft.ReferralInstitutionId,
                ReferralNumber = draft.ReferralNumber,
                ReferralDate = draft.ReferralDate,
                ReferralDoctorName = draft.ReferralDoctorName,
                ReferralDiagnosisNote = draft.ReferralDiagnosisNote,
                ReferralReason = draft.ReferralReason,
                HasConsent = draft.HasConsent,
                ConsentSignerType = draft.ConsentSignerType,
                ConsentSignerName = draft.ConsentSignerName,
                ConsentSignerRelationshipText = draft.ConsentSignerRelationshipText,
                ConsentSignerIdentityNumber = draft.ConsentSignerIdentityNumber,
                ConsentSignerPhoneNumber = draft.ConsentSignerPhoneNumber,
                ConsentSignerAddress = draft.ConsentSignerAddress,
                ConsentAgreedClauses = draft.ConsentAgreedClauses,
                ConsentSignatureImageBase64 = draft.ConsentSignatureImageBase64,
                ConsentSignedAt = draft.HasConsent ? now : null,
                ContactWhatsAppNumber = draft.WhatsAppNumber,
                ContactEmail = draft.Email,
                EmailDeliveryStatus = OnlineRegistrationEmailStatus.NotRequested,
                SubmittedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = now
            };
        }

        // ------------------------------------------------------------------
        // Email sesudah transaksi
        // ------------------------------------------------------------------

        /// <summary>
        /// Mengirim kode lewat email bila kontaknya email, lalu mencatat hasilnya. Tidak pernah
        /// melempar: pendaftaran sudah tersimpan (<c>KSK-DEC-042</c>).
        /// </summary>
        private async Task<OnlineRegistrationEmailStatus> SendCodeEmailAsync(
            RegOnlineRegistration registration,
            MstDoctorSchedule schedule,
            string code)
        {
            if (string.IsNullOrWhiteSpace(registration.ContactEmail))
                return OnlineRegistrationEmailStatus.NotRequested;

            var status = OnlineRegistrationEmailStatus.Failed;

            try
            {
                var outcome = await _emailSender.SendAsync(
                    BuildCodeEmail(registration, schedule, code),
                    CancellationToken.None);

                status = outcome == EmailSendOutcome.Sent
                    ? OnlineRegistrationEmailStatus.Sent
                    : OnlineRegistrationEmailStatus.Failed;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    "Pendaftaran online: email kode gagal dikirim. RegistrationId={RegistrationId} Error={ErrorType}",
                    registration.Id,
                    exception.GetType().Name);
            }

            try
            {
                var attemptedAt = DateTime.UtcNow;
                await _dbContext.RegOnlineRegistrations
                    .Where(x => x.Id == registration.Id)
                    .ExecuteUpdateAsync(
                        setters => setters
                            .SetProperty(x => x.EmailDeliveryStatus, status)
                            .SetProperty(x => x.EmailAttemptedAt, attemptedAt),
                        CancellationToken.None);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    "Pendaftaran online: status email gagal dicatat. RegistrationId={RegistrationId} Error={ErrorType}",
                    registration.Id,
                    exception.GetType().Name);
            }

            _logger.LogInformation(
                "Pendaftaran online: email kode {EmailStatus}. RegistrationId={RegistrationId}",
                status,
                registration.Id);

            return status;
        }

        /// <summary>
        /// Isi email: nomor, kode, tanggal, poli, dokter, jam, dan petunjuk check-in. Tanpa NIK, No. RM,
        /// tanggal lahir, maupun penjamin (<c>KSK-PO-FE-003</c>, <c>integration-contract.md</c>).
        /// </summary>
        private static EmailMessage BuildCodeEmail(RegOnlineRegistration registration, MstDoctorSchedule schedule, string code)
        {
            var body = new StringBuilder()
                .AppendLine("Yth. Bapak/Ibu,")
                .AppendLine()
                .AppendLine("Pendaftaran online Rawat Jalan Anda berhasil disimpan.")
                .AppendLine()
                .AppendLine($"Nomor pendaftaran : {registration.RegistrationNumber}")
                .AppendLine($"Kode check-in     : {FormatCode(code)}")
                .AppendLine($"Tanggal kunjungan : {FormatIndonesianDate(registration.VisitDate)}")
                .AppendLine($"Poli              : {schedule.Clinic?.ClinicName}")
                .AppendLine($"Dokter            : {schedule.Doctor?.FullName}")
                .AppendLine($"Jam praktik       : {schedule.StartTime:hh\\:mm} - {schedule.EndTime:hh\\:mm} WIB")
                .AppendLine()
                .AppendLine("Cara check-in:")
                .AppendLine("1. Datang ke rumah sakit pada tanggal kunjungan.")
                .AppendLine("2. Di kiosk, pilih \"Check-in Pasien Online\".")
                .AppendLine("3. Masukkan kode check-in di atas dan tanggal lahir pasien.")
                .AppendLine()
                .AppendLine("Simpan kode ini dan jangan bagikan kepada orang lain. Bila lupa kode, pilih \"Lupa Kode\" di kiosk.")
                .AppendLine()
                .AppendLine("Email ini dikirim otomatis. Mohon tidak membalas email ini.")
                .ToString();

            return new EmailMessage(
                registration.ContactEmail!,
                $"Pendaftaran Online Rawat Jalan {registration.RegistrationNumber}",
                body);
        }

        private static string FormatIndonesianDate(DateTime date) =>
            $"{IndonesianDayNames[(int)date.DayOfWeek]}, {date.Day} {IndonesianMonthNames[date.Month - 1]} {date.Year}";

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private OnlineRegistrationPublicResult<OnlineRegistrationSubmitResponse> RejectSubmit(
            OnlineRegistrationPublicOutcome outcome,
            string message,
            string rule)
        {
            _logger.LogInformation("Pendaftaran online ditolak. Rule={Rule} Outcome={Outcome}", rule, outcome);
            return OnlineRegistrationPublicResult<OnlineRegistrationSubmitResponse>.Fail(outcome, message);
        }

        private static bool IsValidEmail(string value)
        {
            if (value.Any(char.IsWhiteSpace) || !MailAddress.TryCreate(value, out var address))
                return false;

            if (!string.Equals(address.Address, value, StringComparison.Ordinal))
                return false;

            var domain = value[(value.LastIndexOf('@') + 1)..];
            return domain.Contains('.') && !domain.StartsWith('.') && !domain.EndsWith('.');
        }

        private static Guid? NullIfEmpty(Guid? value) =>
            value.HasValue && value.Value != Guid.Empty ? value : null;

        private static string? CleanText(string? value)
        {
            var trimmed = value?.Trim();
            return string.IsNullOrEmpty(trimmed) ? null : trimmed;
        }

        private static DateTime ToCalendarDate(DateTime value) =>
            DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);

        private static bool IsUniqueViolation(DbUpdateException exception) =>
            exception.GetBaseException() is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
    }
}
