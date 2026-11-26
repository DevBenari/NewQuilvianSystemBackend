using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Perintah petugas atas pendaftaran online (<c>BE-KSK-PO-010</c>): ubah jadwal, ubah kontak, batal
    /// (<c>KSK-DEC-051</c>, <c>052</c>). Ketiganya hanya untuk status Menunggu Check-in, memakai
    /// <c>expectedRowVersion</c> (<c>GATE-PO-16</c>), dan menulis riwayat beserta nilai lama/baru.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Ubah jadwal</b> memakai aturan yang sama dengan pendaftaran baru: H+1 s.d. H+30 dari hari
    /// perubahan, jadwal di poli yang sama yang mengizinkan appointment dan kiosk, kuota appointment
    /// masih ada, dan satu pendaftaran terbuka per pasien per tanggal. Kuota jadwal lama dilepas dan
    /// kuota jadwal baru dipakai dalam satu langkah, karena kuota dihitung dari pendaftaran itu sendiri.
    /// Kunci pasien diambil sebelum kunci kuota jadwal baru, urutan yang sama dengan pengiriman
    /// pendaftaran. Kode tidak berubah (<c>KSK-PO-AS-003</c>); token check-in yang terlanjur terbit dihapus.
    /// </para>
    /// <para>
    /// <b>Ubah kontak</b> tidak mengirim ulang kode. <b>Batal</b> menutup pendaftaran sebagai Dibatalkan:
    /// kode mati karena verifikasi kiosk hanya mencari status Menunggu Check-in, dan kuota langsung lepas.
    /// </para>
    /// <para>Log tidak memuat kontak lama maupun baru; nilai itu hanya ada di riwayat.</para>
    /// </remarks>
    public partial class OnlineRegistrationService
    {
        public const string NotChangeableMessage = "Pendaftaran sudah tidak dapat diubah.";
        public const string StaleMessage = "Data sudah berubah. Muat ulang lalu coba lagi.";
        public const string VisitDateRangeMessage = "Tanggal kunjungan harus antara besok sampai 30 hari ke depan.";
        public const string OtherClinicMessage =
            "Jadwal baru harus di poli yang sama. Untuk pindah poli, batalkan lalu minta pasien mendaftar ulang.";
        public const string DateConflictMessage = "Pasien sudah memiliki pendaftaran atau kunjungan pada tanggal itu.";
        public const string NoScheduleChangeMessage = "Tidak ada perubahan jadwal.";
        public const string NoContactChangeMessage = "Tidak ada perubahan kontak.";
        public const string CancelReasonMessage = "Alasan pembatalan wajib diisi (5–500 karakter).";

        private const string RowVersionRequiredMessage = "Versi data pendaftaran wajib dikirim. Muat ulang lalu coba lagi.";
        private const int MinCancelReasonLength = 5;
        private const int MaxCancelReasonLength = 500;

        /// <summary>Ubah tanggal dan/atau jadwal dokter di poli yang sama (<c>KSK-VAL-PO-060</c> s.d. <c>066</c>).</summary>
        public async Task<OnlineRegistrationPublicResult<OnlineRegistrationDetailResponse>> ChangeScheduleAsync(
            Guid id,
            OnlineRegistrationScheduleChangeRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null || request.ExpectedRowVersion == Guid.Empty)
                return Reject(OnlineRegistrationPublicOutcome.Invalid, RowVersionRequiredMessage, "ROW_VERSION");

            if (!request.VisitDate.HasValue || !_publicService.IsVisitDateAllowed(request.VisitDate.Value))
                return Reject(OnlineRegistrationPublicOutcome.Invalid, VisitDateRangeMessage, "KSK-VAL-PO-061");

            if (!request.DoctorScheduleId.HasValue || request.DoctorScheduleId.Value == Guid.Empty)
                return Reject(OnlineRegistrationPublicOutcome.Invalid, "Pilih jadwal dokter.", "SCHEDULE");

            var registration = await LoadForCommandAsync(id, request.ExpectedRowVersion, cancellationToken);
            if (registration.Failure != null)
                return registration.Failure;

            var entity = registration.Entity!;
            var newVisitDate = ToCalendarDate(request.VisitDate.Value);
            var newScheduleId = request.DoctorScheduleId.Value;
            var oldVisitDate = ToCalendarDate(entity.VisitDate);
            var oldScheduleId = entity.DoctorScheduleId;

            if (newVisitDate == oldVisitDate && newScheduleId == oldScheduleId)
                return Reject(OnlineRegistrationPublicOutcome.Invalid, NoScheduleChangeMessage, "KSK-VAL-PO-066");

            var newClinicId = await _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .Where(x => x.Id == newScheduleId && !x.IsDelete)
                .Select(x => (Guid?)x.ClinicId)
                .FirstOrDefaultAsync(cancellationToken);

            if (newClinicId == null)
                return Reject(OnlineRegistrationPublicOutcome.Conflict, OnlineRegistrationPublicService.ScheduleUnavailableMessage, "KSK-VAL-PO-063");

            if (newClinicId != entity.ClinicId)
                return Reject(OnlineRegistrationPublicOutcome.Invalid, OtherClinicMessage, "KSK-VAL-PO-062");

            var schedule = (await _publicService.LoadOfferedSchedulesAsync(entity.ClinicId, newVisitDate, cancellationToken))
                .FirstOrDefault(x => x.Id == newScheduleId);

            if (schedule == null)
                return Reject(OnlineRegistrationPublicOutcome.Conflict, OnlineRegistrationPublicService.ScheduleUnavailableMessage, "KSK-VAL-PO-063");

            await using (var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                await _publicService.LockOnlinePatientAsync(entity.PatientId, entity.IdentityNumber, cancellationToken);
                await _quotaService.AcquireQuotaLockAsync(schedule.Id, newVisitDate, cancellationToken);

                if (newVisitDate != oldVisitDate)
                {
                    var hasOpen = await _publicService.HasOpenRegistrationAsync(
                        entity.PatientId,
                        entity.IdentityNumber,
                        newVisitDate,
                        entity.Id,
                        cancellationToken);

                    var hasEncounter = entity.PatientId.HasValue &&
                        OutpatientEncounterRules.IsActiveEncounterBlockEnabled(_configuration) &&
                        await _publicService.HasActiveOutpatientEncounterOnAsync(entity.PatientId.Value, newVisitDate, cancellationToken);

                    if (hasOpen || hasEncounter)
                        return Reject(OnlineRegistrationPublicOutcome.Conflict, DateConflictMessage, "KSK-VAL-PO-065");
                }

                // Pendaftaran ini belum terhitung pada jadwal/tanggal baru, karena salah satunya berbeda.
                var usage = await _quotaService.GetUsageAsync(
                    schedule.Id,
                    DateTime.SpecifyKind(newVisitDate, DateTimeKind.Utc),
                    cancellationToken);

                if (DoctorScheduleQuotaService.IsFullForAppointment(schedule, usage))
                    return Reject(OnlineRegistrationPublicOutcome.Conflict, OnlineRegistrationPublicService.QuotaFullMessage, "KSK-VAL-PO-064");

                var now = _timeProvider.GetUtcNow().UtcDateTime;
                entity.VisitDate = newVisitDate;
                entity.DoctorScheduleId = schedule.Id;
                entity.CheckInTokenHash = null;
                entity.CheckInTokenExpiresAt = null;
                Touch(entity, actorUserId, now);

                AddHistory(entity, RegOnlineRegistrationHistory.EventScheduleChanged, OnlineRegistrationStatus.AwaitingCheckIn, actorUserId, now, history =>
                {
                    history.FromVisitDate = oldVisitDate;
                    history.ToVisitDate = newVisitDate;
                    history.FromDoctorScheduleId = oldScheduleId;
                    history.ToDoctorScheduleId = schedule.Id;
                });

                var saved = await SaveAsync(cancellationToken);
                if (saved != null)
                    return saved;

                await transaction.CommitAsync(cancellationToken);
            }

            return await SuccessAsync(entity, "Jadwal pendaftaran online berhasil diubah. Kode pendaftaran tetap sama.", "SCHEDULE_CHANGED", cancellationToken);
        }

        /// <summary>Ubah No. WA atau email (<c>KSK-VAL-PO-067</c>). Kode tidak dikirim ulang.</summary>
        public async Task<OnlineRegistrationPublicResult<OnlineRegistrationDetailResponse>> ChangeContactAsync(
            Guid id,
            OnlineRegistrationContactChangeRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request == null || request.ExpectedRowVersion == Guid.Empty)
                return Reject(OnlineRegistrationPublicOutcome.Invalid, RowVersionRequiredMessage, "ROW_VERSION");

            var contactError = OnlineRegistrationPublicService.ParseContact(request.Contact, out var whatsApp, out var email);
            if (contactError != null)
                return Reject(OnlineRegistrationPublicOutcome.Invalid, contactError.Value.Message, contactError.Value.Rule);

            var registration = await LoadForCommandAsync(id, request.ExpectedRowVersion, cancellationToken);
            if (registration.Failure != null)
                return registration.Failure;

            var entity = registration.Entity!;
            var oldContact = entity.ContactWhatsAppNumber ?? entity.ContactEmail;
            var newContact = whatsApp ?? email;

            if (string.Equals(oldContact, newContact, StringComparison.OrdinalIgnoreCase))
                return Reject(OnlineRegistrationPublicOutcome.Invalid, NoContactChangeMessage, "CONTACT_SAME");

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            entity.ContactWhatsAppNumber = whatsApp;
            entity.ContactEmail = email;
            Touch(entity, actorUserId, now);

            AddHistory(entity, RegOnlineRegistrationHistory.EventContactChanged, OnlineRegistrationStatus.AwaitingCheckIn, actorUserId, now, history =>
            {
                history.FromContact = oldContact;
                history.ToContact = newContact;
            });

            var saved = await SaveAsync(cancellationToken);
            if (saved != null)
                return saved;

            return await SuccessAsync(entity, "Kontak pendaftaran online berhasil diubah. Kode tidak dikirim ulang.", "CONTACT_CHANGED", cancellationToken);
        }

        /// <summary>Batal dengan alasan wajib (<c>KSK-VAL-PO-070</c> s.d. <c>072</c>). Tidak dapat dibatalkan ulang.</summary>
        public async Task<OnlineRegistrationPublicResult<OnlineRegistrationDetailResponse>> CancelAsync(
            Guid id,
            OnlineRegistrationCancelRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var reason = request?.Reason?.Trim() ?? string.Empty;
            if (reason.Length < MinCancelReasonLength || reason.Length > MaxCancelReasonLength)
                return Reject(OnlineRegistrationPublicOutcome.Invalid, CancelReasonMessage, "KSK-VAL-PO-070");

            if (request!.ExpectedRowVersion == Guid.Empty)
                return Reject(OnlineRegistrationPublicOutcome.Invalid, RowVersionRequiredMessage, "ROW_VERSION");

            var registration = await LoadForCommandAsync(id, request.ExpectedRowVersion, cancellationToken);
            if (registration.Failure != null)
                return registration.Failure;

            var entity = registration.Entity!;
            var now = _timeProvider.GetUtcNow().UtcDateTime;

            entity.Status = OnlineRegistrationStatus.Cancelled;
            entity.ClosedAt = now;
            entity.CancelReason = reason;
            entity.IsCancel = true;
            entity.CancelBy = actorUserId;
            entity.CancelDateTime = now;
            entity.CheckInTokenHash = null;
            entity.CheckInTokenExpiresAt = null;
            Touch(entity, actorUserId, now);

            AddHistory(entity, RegOnlineRegistrationHistory.EventCancelled, OnlineRegistrationStatus.Cancelled, actorUserId, now, history =>
            {
                history.Reason = reason;
            });

            var saved = await SaveAsync(cancellationToken);
            if (saved != null)
                return saved;

            return await SuccessAsync(entity, "Pendaftaran online dibatalkan. Kode tidak berlaku lagi dan kuota sudah dilepas.", "CANCELLED", cancellationToken);
        }

        // ------------------------------------------------------------------
        // Pembantu perintah
        // ------------------------------------------------------------------

        private sealed record CommandTarget(
            RegOnlineRegistration? Entity,
            OnlineRegistrationPublicResult<OnlineRegistrationDetailResponse>? Failure);

        /// <summary>Pendaftaran yang boleh diubah: ada, versi cocok (<c>072</c>), Menunggu Check-in (<c>060</c>/<c>071</c>).</summary>
        private async Task<CommandTarget> LoadForCommandAsync(Guid id, Guid expectedRowVersion, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.RegOnlineRegistrations
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return new CommandTarget(null, Reject(OnlineRegistrationPublicOutcome.NotFound, NotFoundMessage, "NOT_FOUND"));

            if (entity.RowVersion != expectedRowVersion)
                return new CommandTarget(null, Reject(OnlineRegistrationPublicOutcome.Conflict, StaleMessage, "KSK-VAL-PO-072"));

            if (entity.Status != OnlineRegistrationStatus.AwaitingCheckIn)
                return new CommandTarget(null, Reject(OnlineRegistrationPublicOutcome.Conflict, NotChangeableMessage, "KSK-VAL-PO-060"));

            return new CommandTarget(entity, null);
        }

        private static void Touch(RegOnlineRegistration entity, Guid actorUserId, DateTime now)
        {
            entity.RowVersion = Guid.NewGuid();
            entity.UpdateBy = actorUserId;
            entity.UpdateDateTime = now;
        }

        private void AddHistory(
            RegOnlineRegistration entity,
            string eventCode,
            OnlineRegistrationStatus toStatus,
            Guid actorUserId,
            DateTime now,
            Action<RegOnlineRegistrationHistory> fill)
        {
            var history = new RegOnlineRegistrationHistory
            {
                Id = Guid.NewGuid(),
                OnlineRegistrationId = entity.Id,
                EventCode = eventCode,
                FromStatus = OnlineRegistrationStatus.AwaitingCheckIn,
                ToStatus = toStatus,
                Source = OnlineRegistrationEventSource.Staff,
                ActorUserId = actorUserId == Guid.Empty ? null : actorUserId,
                OccurredAt = now,
                CreateBy = actorUserId,
                CreateDateTime = now
            };

            fill(history);
            _dbContext.RegOnlineRegistrationHistories.Add(history);
        }

        /// <summary>Simpan; bentrok versi di database dijawab sama dengan <c>KSK-VAL-PO-072</c>.</summary>
        private async Task<OnlineRegistrationPublicResult<OnlineRegistrationDetailResponse>?> SaveAsync(CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return null;
            }
            catch (DbUpdateConcurrencyException)
            {
                _dbContext.ChangeTracker.Clear();
                return Reject(OnlineRegistrationPublicOutcome.Conflict, StaleMessage, "KSK-VAL-PO-072");
            }
        }

        private async Task<OnlineRegistrationPublicResult<OnlineRegistrationDetailResponse>> SuccessAsync(
            RegOnlineRegistration entity,
            string message,
            string eventCode,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Pendaftaran online diubah petugas. RegistrationId={RegistrationId} Event={Event}",
                entity.Id,
                eventCode);

            var detail = await GetDetailAsync(entity.Id, cancellationToken);
            return OnlineRegistrationPublicResult<OnlineRegistrationDetailResponse>.Success(detail!, message);
        }

        private OnlineRegistrationPublicResult<OnlineRegistrationDetailResponse> Reject(
            OnlineRegistrationPublicOutcome outcome,
            string message,
            string rule)
        {
            _logger.LogInformation("Perintah pendaftaran online ditolak. Rule={Rule} Outcome={Outcome}", rule, outcome);
            return OnlineRegistrationPublicResult<OnlineRegistrationDetailResponse>.Fail(outcome, message);
        }
    }
}
