using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Hasil <see cref="OnlineRegistrationCheckInService.PrepareCheckInAsync"/>: potret pendaftaran yang
    /// diperiksa sebelum transaksi kunjungan, untuk diperiksa ulang di dalam transaksi.
    /// </summary>
    public sealed class OnlineRegistrationCheckInPreparation
    {
        public Guid OnlineRegistrationId { get; init; }

        public string CheckInTokenHash { get; init; } = string.Empty;

        public DateTime VisitDate { get; init; }

        public Guid ClinicId { get; init; }

        public Guid DoctorScheduleId { get; init; }

        public OnlineRegistrationPatientMode PatientMode { get; init; }
    }

    /// <summary>
    /// Pendaftaran berubah (dibatalkan, dipindah, ditutup, atau diverifikasi ulang) di antara
    /// pemeriksaan dan penyimpanan kunjungan (<c>KSK-VAL-PO-055</c>). Jalur kunjungan membatalkan
    /// transaksinya sehingga kunjungan tidak terbentuk dan status tetap Menunggu Check-in.
    /// </summary>
    public sealed class OnlineRegistrationCheckInConflictException : Exception
    {
        public OnlineRegistrationCheckInConflictException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Langkah kedua check-in (<c>BE-KSK-PO-007</c>, <c>KSK-DSN-PO-008</c>, PO.4.6): kunjungan dibuat
    /// lewat <c>POST patient-encounters/kiosk</c> yang sudah ada, dengan <c>onlineRegistrationId</c> dan
    /// token check-in. Polanya sama dengan hook Reservasi Prioritas di <c>PatientEncounterController</c>:
    /// pemeriksaan sebelum transaksi, penyelesaian di dalam transaksi tanpa <c>SaveChanges</c> sendiri.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kiosk tidak dapat mengganti pilihan pasien.</b> Poli, jadwal, dokter, jenis pembayaran, kartu,
    /// dan faskes perujuk wajib sama dengan pendaftaran (<c>KSK-VAL-PO-048</c>). Pasien lama dicocokkan
    /// lewat <c>PatientId</c> (<c>049</c>); pasien baru lewat NIK master pasien yang dikirim
    /// (<c>052</c>, <c>KSK-DEC-048</c> sisi server).
    /// </para>
    /// <para>
    /// <b>Backend yang menentukan.</b> Tanggal kunjungan, <c>IsAppointment = true</c>,
    /// <c>IsWalkIn = false</c>, <c>IsFromKiosk = true</c>, <c>RegistrationSource = OnlineRegistration</c>,
    /// dan rincian rujukan diambil dari pendaftaran, bukan dari kiosk (<c>KSK-DSN-PO-014</c>).
    /// </para>
    /// <para>
    /// <b>Kuota.</b> Pendaftaran yang sedang di-check-in dikecualikan dari hitungan kuota kunjungan oleh
    /// <see cref="OnlineRegistrationService.CountClosableOnScheduleAsync"/> (aturan pencocokan pasien yang
    /// sama), sehingga slot yang sudah dipegangnya tidak dihitung dua kali.
    /// </para>
    /// </remarks>
    public partial class OnlineRegistrationCheckInService
    {
        public const string TokenExpiredMessage = "Waktu check-in habis. Silakan ulangi dari awal.";
        public const string StaffChannelMessage = "Pendaftaran online hanya dapat di-check-in di kiosk.";
        public const string CheckInMismatchMessage = "Data check-in tidak sesuai dengan pendaftaran online. Silakan ulangi.";
        public const string IdentityMismatchMessage =
            "Data kartu identitas tidak sesuai dengan pendaftaran online. Silakan ke loket pendaftaran.";
        public const string ScheduleChangedMessage = "Jadwal dokter berubah, silakan ke loket pendaftaran.";
        public const string QuotaFullMessage = "Kuota dokter sudah penuh, silakan ke loket pendaftaran.";
        public const string RegistrationChangedMessage = "Pendaftaran sudah berubah. Silakan hubungi petugas.";

        /// <summary>
        /// Pemeriksaan sebelum transaksi kunjungan. Bila lolos, permintaan kunjungan dinormalisasi dari
        /// pendaftaran dan potret pendaftaran dikembalikan untuk <see cref="CompleteCheckInInTransactionAsync"/>.
        /// </summary>
        public async Task<OnlineRegistrationPublicResult<OnlineRegistrationCheckInPreparation>> PrepareCheckInAsync(
            PatientEncounterCreateRequest request,
            CancellationToken cancellationToken = default)
        {
            var registrationId = request.OnlineRegistrationId ?? Guid.Empty;
            var token = request.OnlineRegistrationCheckInToken?.Trim();

            if (registrationId == Guid.Empty || string.IsNullOrEmpty(token))
                return RejectCheckIn(OnlineRegistrationPublicOutcome.Invalid, TokenExpiredMessage, "KSK-VAL-PO-046", registrationId);

            var registration = await _dbContext.RegOnlineRegistrations
                .AsNoTracking()
                .Include(x => x.DoctorSchedule)
                .FirstOrDefaultAsync(x => x.Id == registrationId && !x.IsDelete, cancellationToken);

            // Id tanpa pendaftaran dijawab sama dengan token salah, supaya id tidak dapat ditebak.
            if (registration == null)
                return RejectCheckIn(OnlineRegistrationPublicOutcome.Invalid, TokenExpiredMessage, "KSK-VAL-PO-046", registrationId);

            if (registration.Status != OnlineRegistrationStatus.AwaitingCheckIn)
                return RejectCheckIn(OnlineRegistrationPublicOutcome.Conflict, RegistrationChangedMessage, "KSK-VAL-PO-055", registrationId);

            var now = _timeProvider.GetUtcNow();
            var tokenHash = ComputeTokenHash(token);

            if (!TokenMatches(registration.CheckInTokenHash, tokenHash) ||
                registration.CheckInTokenExpiresAt == null ||
                ToUtc(registration.CheckInTokenExpiresAt.Value) <= now.UtcDateTime)
            {
                return RejectCheckIn(OnlineRegistrationPublicOutcome.Invalid, TokenExpiredMessage, "KSK-VAL-PO-046", registrationId);
            }

            // Token hanya terbit untuk tanggal kunjungan hari ini; lewat tengah malam berarti harus ulang.
            var today = TimeZoneInfo.ConvertTime(now, _businessTimeZone).Date;
            if (registration.VisitDate.Date != today)
                return RejectCheckIn(OnlineRegistrationPublicOutcome.Invalid, TokenExpiredMessage, "KSK-VAL-PO-046", registrationId);

            var mismatchRule = await FindVisitMismatchAsync(registration, request, cancellationToken);
            if (mismatchRule != null)
                return RejectCheckIn(OnlineRegistrationPublicOutcome.Invalid, CheckInMismatchMessage, mismatchRule, registrationId);

            if (registration.PatientMode == OnlineRegistrationPatientMode.New)
            {
                var masterNik = await _dbContext.Set<MstPatient>()
                    .AsNoTracking()
                    .Where(x => x.Id == request.PatientId && !x.IsDelete)
                    .Select(x => x.IdentityNumber)
                    .FirstOrDefaultAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(masterNik) ||
                    !string.Equals(masterNik.Trim(), registration.IdentityNumber?.Trim(), StringComparison.Ordinal))
                {
                    return RejectCheckIn(OnlineRegistrationPublicOutcome.Invalid, IdentityMismatchMessage, "KSK-VAL-PO-052", registrationId);
                }
            }
            else if (registration.PatientId != request.PatientId)
            {
                return RejectCheckIn(OnlineRegistrationPublicOutcome.Invalid, CheckInMismatchMessage, "KSK-VAL-PO-049", registrationId);
            }

            var paymentRule = await FindPaymentMismatchAsync(registration, request, cancellationToken);
            if (paymentRule != null)
                return RejectCheckIn(OnlineRegistrationPublicOutcome.Invalid, CheckInMismatchMessage, paymentRule, registrationId);

            if (!ReferralMatches(registration, request))
                return RejectCheckIn(OnlineRegistrationPublicOutcome.Invalid, CheckInMismatchMessage, "KSK-VAL-PO-048/REFERRAL", registrationId);

            NormalizeRequest(registration, request);

            return OnlineRegistrationPublicResult<OnlineRegistrationCheckInPreparation>.Success(
                new OnlineRegistrationCheckInPreparation
                {
                    OnlineRegistrationId = registration.Id,
                    CheckInTokenHash = tokenHash,
                    VisitDate = registration.VisitDate.Date,
                    ClinicId = registration.ClinicId,
                    DoctorScheduleId = registration.DoctorScheduleId,
                    PatientMode = registration.PatientMode
                },
                string.Empty);
        }

        /// <summary>
        /// Di dalam transaksi kunjungan, sesudah kunjungan dan rincian rujukannya ditambahkan ke context:
        /// status Menjadi Kunjungan, tautan kunjungan, consent tampungan, berkas rujukan, hapus token, dan
        /// riwayat <c>CHECKED_IN</c>. Tidak memanggil <c>SaveChanges</c>.
        /// </summary>
        /// <exception cref="OnlineRegistrationCheckInConflictException">Pendaftaran berubah sesudah diperiksa.</exception>
        public async Task<RegOnlineRegistration> CompleteCheckInInTransactionAsync(
            OnlineRegistrationCheckInPreparation preparation,
            RegPatientEncounter encounter,
            RegEncounterReferral? encounterReferral,
            Guid actorUserId,
            DateTime now,
            CancellationToken cancellationToken = default)
        {
            var registration = await _dbContext.RegOnlineRegistrations
                .Include(x => x.ReferralDocuments.Where(d => !d.IsDelete))
                .FirstOrDefaultAsync(x => x.Id == preparation.OnlineRegistrationId && !x.IsDelete, cancellationToken);

            // Pembatalan, pindah jadwal, penutupan, atau verifikasi ulang di kiosk lain sesudah pemeriksaan.
            if (registration == null ||
                registration.Status != OnlineRegistrationStatus.AwaitingCheckIn ||
                !TokenMatches(registration.CheckInTokenHash, preparation.CheckInTokenHash) ||
                registration.VisitDate.Date != preparation.VisitDate ||
                registration.ClinicId != preparation.ClinicId ||
                registration.DoctorScheduleId != preparation.DoctorScheduleId)
            {
                throw new OnlineRegistrationCheckInConflictException(RegistrationChangedMessage);
            }

            registration.Status = OnlineRegistrationStatus.CheckedIn;
            registration.EncounterId = encounter.Id;
            registration.CheckedInAt = now;
            registration.CheckInTokenHash = null;
            registration.CheckInTokenExpiresAt = null;
            registration.RowVersion = Guid.NewGuid();
            registration.UpdateBy = actorUserId;
            registration.UpdateDateTime = now;

            // Data kamus PO-D.2: PatientId pasien baru diisi saat check-in.
            if (registration.PatientMode == OnlineRegistrationPatientMode.New)
                registration.PatientId = encounter.PatientId;

            if (registration.HasConsent)
                AddGeneralConsent(registration, encounter, actorUserId, now);

            if (encounterReferral != null)
            {
                encounterReferral.CaptureSource = ReferralCaptureSource.OnlineRegistration;
                var documentCount = LinkReferralDocuments(registration, encounterReferral, actorUserId, now);
                EncounterReferralService.ApplyCompletion(encounterReferral, encounter, documentCount, now);
            }

            _dbContext.RegOnlineRegistrationHistories.Add(new RegOnlineRegistrationHistory
            {
                Id = Guid.NewGuid(),
                OnlineRegistrationId = registration.Id,
                EventCode = RegOnlineRegistrationHistory.EventCheckedIn,
                FromStatus = OnlineRegistrationStatus.AwaitingCheckIn,
                ToStatus = OnlineRegistrationStatus.CheckedIn,
                Source = OnlineRegistrationEventSource.Kiosk,
                ActorUserId = actorUserId == Guid.Empty ? null : actorUserId,
                EncounterId = encounter.Id,
                OccurredAt = now,
                CreateBy = actorUserId,
                CreateDateTime = now
            });

            _logger.LogInformation(
                "Check-in pendaftaran online menjadi kunjungan. RegistrationId={RegistrationId} EncounterId={EncounterId}",
                registration.Id,
                encounter.Id);

            return registration;
        }

        // ------------------------------------------------------------------
        // Pemeriksaan isi kunjungan
        // ------------------------------------------------------------------

        /// <summary>Poli, jadwal, dokter, unit, jenis kunjungan, dan <c>IsAppointment</c>.</summary>
        private async Task<string?> FindVisitMismatchAsync(
            RegOnlineRegistration registration,
            PatientEncounterCreateRequest request,
            CancellationToken cancellationToken)
        {
            if (request.EncounterType != EncounterType.Outpatient || !request.IsAppointment)
                return "KSK-VAL-PO-048/APPOINTMENT";

            if (request.ClinicId != registration.ClinicId || request.DoctorScheduleId != registration.DoctorScheduleId)
                return "KSK-VAL-PO-048/SCHEDULE";

            // Jadwal dibaca apa adanya (aktif atau tidak); keabsahannya diperiksa jalur kunjungan (KSK-VAL-PO-053).
            var schedule = registration.DoctorSchedule
                ?? await _dbContext.Set<MstDoctorSchedule>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == registration.DoctorScheduleId, cancellationToken);

            if (schedule == null || request.ServiceUnitId != schedule.ServiceUnitId)
                return "KSK-VAL-PO-048/SCHEDULE";

            if (request.DoctorId.HasValue && request.DoctorId.Value != Guid.Empty && request.DoctorId.Value != schedule.DoctorId)
                return "KSK-VAL-PO-048/DOCTOR";

            request.DoctorId = schedule.DoctorId;

            // Penjamin turunan bayi baru lahir tidak berlaku untuk check-in.
            if (request.InheritedPaymentSourceId.HasValue && request.InheritedPaymentSourceId.Value != Guid.Empty)
                return "KSK-VAL-PO-048/PAYMENT";

            return null;
        }

        /// <summary>
        /// Jenis pembayaran sama. Kartu/penjamin milik pasien: id sama. Kartu baru yang ditambahkan online
        /// (dibuat kiosk sebelum check-in): penyedia dan nomor polis sama, milik pasien kunjungan.
        /// </summary>
        private async Task<string?> FindPaymentMismatchAsync(
            RegOnlineRegistration registration,
            PatientEncounterCreateRequest request,
            CancellationToken cancellationToken)
        {
            if (request.PaymentType != registration.PaymentType)
                return "KSK-VAL-PO-048/PAYMENT";

            switch (registration.PaymentType)
            {
                case EncounterPaymentType.Insurance when registration.PatientInsuranceId.HasValue:
                    return request.PatientInsuranceId == registration.PatientInsuranceId ? null : "KSK-VAL-PO-048/CARD";

                case EncounterPaymentType.Insurance:
                {
                    if (!registration.NewInsuranceProviderId.HasValue ||
                        !request.PatientInsuranceId.HasValue ||
                        request.PatientInsuranceId.Value == Guid.Empty)
                    {
                        return "KSK-VAL-PO-048/CARD";
                    }

                    var card = await _dbContext.Set<MstPatientInsurance>()
                        .AsNoTracking()
                        .Where(x => x.Id == request.PatientInsuranceId.Value && x.PatientId == request.PatientId && !x.IsDelete)
                        .Select(x => new { x.InsuranceProviderId, x.PolicyNumber })
                        .FirstOrDefaultAsync(cancellationToken);

                    var matches = card != null &&
                        card.InsuranceProviderId == registration.NewInsuranceProviderId.Value &&
                        string.Equals(
                            card.PolicyNumber?.Trim(),
                            registration.NewInsurancePolicyNumber?.Trim(),
                            StringComparison.OrdinalIgnoreCase);

                    return matches ? null : "KSK-VAL-PO-048/CARD";
                }

                case EncounterPaymentType.CompanyGuarantor:
                    return request.PatientCompanyGuarantorId == registration.PatientCompanyGuarantorId ? null : "KSK-VAL-PO-048/CARD";

                default:
                    return null;
            }
        }

        /// <summary>Faskes perujuk sama; kiosk tidak dapat menambah atau menghapus rujukan.</summary>
        private static bool ReferralMatches(RegOnlineRegistration registration, PatientEncounterCreateRequest request)
        {
            if (request.Referral?.SourceType == ReferralSourceType.Internal)
                return false;

            if (!registration.IsReferral)
                return !request.IsReferral;

            return request.IsReferral && request.ReferralInstitutionId == registration.ReferralInstitutionId;
        }

        /// <summary>Nilai kunjungan yang ditentukan backend dari pendaftaran (<c>KSK-DSN-PO-014</c>).</summary>
        private void NormalizeRequest(RegOnlineRegistration registration, PatientEncounterCreateRequest request)
        {
            request.VisitDate = DateTime.SpecifyKind(registration.VisitDate.Date, DateTimeKind.Unspecified);
            request.IsAppointment = true;
            request.IsWalkIn = false;
            request.IsFromKiosk = true;
            request.RegistrationSource = EncounterRegistrationSource.OnlineRegistration;

            if (!registration.IsReferral)
            {
                request.ReferralNumber = null;
                request.ReferralDoctorId = null;
                request.Referral = null;
                return;
            }

            request.ReferralInstitutionId = registration.ReferralInstitutionId;
            request.ReferralNumber = registration.ReferralNumber;
            request.ReferralDoctorId = null;
            request.Referral = new EncounterReferralCreateRequest
            {
                SourceType = ReferralSourceType.External,
                TargetUnitType = ReferralTargetUnitType.Clinic,
                // Tanggal surat (kalender WIB) disimpan sebagai awal hari WIB dalam UTC.
                ReferralDateTime = registration.ReferralDate.HasValue
                    ? TimeZoneInfo.ConvertTimeToUtc(
                        DateTime.SpecifyKind(registration.ReferralDate.Value.Date, DateTimeKind.Unspecified),
                        _businessTimeZone)
                    : null,
                DiagnosisNote = registration.ReferralDiagnosisNote,
                ReferralReason = registration.ReferralReason,
                ReferralDoctorName = registration.ReferralDoctorName
            };
        }

        // ------------------------------------------------------------------
        // Penyelesaian
        // ------------------------------------------------------------------

        /// <summary>Consent yang ditandatangani online, dengan waktu tanda tangan aslinya (<c>KSK-DEC-034</c>).</summary>
        private void AddGeneralConsent(RegOnlineRegistration registration, RegPatientEncounter encounter, Guid actorUserId, DateTime now)
        {
            _dbContext.Set<RegPatientGeneralConsent>().Add(new RegPatientGeneralConsent
            {
                Id = Guid.NewGuid(),
                PatientId = encounter.PatientId,
                PatientEncounterId = encounter.Id,
                SignerType = registration.ConsentSignerType ?? GeneralConsentSignerType.Self,
                SignerName = registration.ConsentSignerName ?? string.Empty,
                SignerRelationshipText = registration.ConsentSignerRelationshipText,
                SignerIdentityNumber = registration.ConsentSignerIdentityNumber,
                SignerPhoneNumber = registration.ConsentSignerPhoneNumber,
                SignerAddress = registration.ConsentSignerAddress,
                AgreedClauses = registration.ConsentAgreedClauses ?? Array.Empty<int>(),
                SignatureImageBase64 = registration.ConsentSignatureImageBase64 ?? string.Empty,
                SignedAt = ToUtc(registration.ConsentSignedAt ?? registration.SubmittedAt),
                CreateBy = actorUserId,
                CreateDateTime = now
            });
        }

        /// <summary>
        /// Berkas rujukan online ditautkan ke rincian rujukan kunjungan tanpa disalin
        /// (<c>KSK-DSN-PO-010</c>): baris baru menunjuk <c>StoragePath</c> yang sama.
        /// </summary>
        private int LinkReferralDocuments(
            RegOnlineRegistration registration,
            RegEncounterReferral encounterReferral,
            Guid actorUserId,
            DateTime now)
        {
            var documents = registration.ReferralDocuments
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.PageOrder)
                .ToList();

            foreach (var document in documents)
            {
                var linked = new RegEncounterReferralDocument
                {
                    Id = Guid.NewGuid(),
                    EncounterReferralId = encounterReferral.Id,
                    OriginalFileName = document.OriginalFileName,
                    ContentType = document.ContentType,
                    SizeBytes = document.SizeBytes,
                    StoragePath = document.StoragePath,
                    PageOrder = document.PageOrder,
                    CreateBy = actorUserId,
                    CreateDateTime = now
                };

                _dbContext.Set<RegEncounterReferralDocument>().Add(linked);
                document.EncounterReferralDocumentId = linked.Id;
            }

            return documents.Count;
        }

        // ------------------------------------------------------------------
        // Pembantu
        // ------------------------------------------------------------------

        private static bool TokenMatches(string? storedHash, string candidateHash)
        {
            if (string.IsNullOrEmpty(storedHash))
                return false;

            return CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(storedHash),
                Encoding.ASCII.GetBytes(candidateHash));
        }

        private static DateTime ToUtc(DateTime value) => value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        private OnlineRegistrationPublicResult<OnlineRegistrationCheckInPreparation> RejectCheckIn(
            OnlineRegistrationPublicOutcome outcome,
            string message,
            string rule,
            Guid registrationId)
        {
            _logger.LogInformation(
                "Check-in pendaftaran online ditolak sebelum transaksi. Rule={Rule} RegistrationId={RegistrationId}",
                rule,
                registrationId);

            return OnlineRegistrationPublicResult<OnlineRegistrationCheckInPreparation>.Fail(outcome, message);
        }
    }
}
