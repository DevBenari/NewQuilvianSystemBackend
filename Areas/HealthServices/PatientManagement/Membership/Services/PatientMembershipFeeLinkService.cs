using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services
{
    /// <summary>Hal-hal yang dibutuhkan Billing dari sebuah membership untuk membuat dan memeriksa tagihan iuran.</summary>
    public sealed record MembershipFeeContext(
        Guid MembershipId,
        Guid PatientId,
        string MemberNumber,
        string TierName,
        MembershipStatus Status,
        MembershipRegistrationType RegistrationType,
        decimal FeeAmountSnapshot,
        DateTimeOffset? PaymentDueAt,
        Guid? FeeChargeReferenceId,
        Guid RowVersion);

    public enum MembershipFeeConfirmationOutcome
    {
        /// <summary>Membership baru/upgrade menjadi Aktif.</summary>
        Activated = 1,

        /// <summary>Perpanjangan menjadi Terjadwal (menyambung periode lama).</summary>
        Scheduled = 2,

        /// <summary>Konfirmasi yang sama sudah pernah diterapkan; tidak ada efek baru.</summary>
        AlreadyConfirmed = 3,

        NotFound = 4,

        /// <summary>Membership sudah tidak Menunggu Pembayaran (mis. dibatalkan); dana perlu ditinjau, bukan diaktifkan.</summary>
        NotPending = 5,

        /// <summary>Rujukan tagihan tidak cocok dengan tagihan milik membership ini.</summary>
        ChargeMismatch = 6,

        /// <summary>Dibayar setelah batas bayar (CRM-DEC-014, VAL-20): ditolak, billing memproses pengembalian.</summary>
        PaidAfterDue = 7,

        /// <summary>Pasien sudah punya membership Aktif/Terjadwal lain yang melarang aktivasi (INV-01, INV-02).</summary>
        ActiveConflict = 8
    }

    public sealed record MembershipFeeConfirmationResult(MembershipFeeConfirmationOutcome Outcome, string Message)
    {
        public bool IsSuccess => Outcome is MembershipFeeConfirmationOutcome.Activated
            or MembershipFeeConfirmationOutcome.Scheduled
            or MembershipFeeConfirmationOutcome.AlreadyConfirmed;
    }

    /// <summary>
    /// Titik masuk internal Billing → CRM untuk iuran (`integration-contract.md` INT-05). Tidak bergantung pada
    /// Billing, sehingga Billing dapat memanggilnya tanpa membentuk siklus dependensi. Seluruh perubahan status
    /// membership karena iuran lunas terjadi di sini, bukan lewat endpoint publik.
    /// </summary>
    public class PatientMembershipFeeLinkService
    {
        public const string FeePaidActionCode = "FEE_PAID";
        public const string ActivatedActionCode = "ACTIVATED";
        public const string ScheduledActionCode = "SCHEDULED";
        public const string UpgradedActionCode = "UPGRADED";
        public const string FeeChargeLinkedActionCode = "FEE_CHARGE_LINKED";

        private const string LogCategory = "HealthServices.PatientManagement.Membership";
        private const string RefundRequestedAction = "REFUND_REQUESTED";
        private const string RefundPendingNote = "Menunggu proses refund kasir; penyetuju refund belum ditetapkan (DEC-CRM-001).";

        private readonly ApplicationDbContext _dbContext;
        private readonly PatientMembershipStatusResolver _statusResolver;
        private readonly LoggerService _loggerService;
        private readonly TimeProvider _timeProvider;

        public PatientMembershipFeeLinkService(
            ApplicationDbContext dbContext,
            PatientMembershipStatusResolver statusResolver,
            LoggerService loggerService,
            TimeProvider? timeProvider = null)
        {
            _dbContext = dbContext;
            _statusResolver = statusResolver;
            _loggerService = loggerService;
            _timeProvider = timeProvider ?? TimeProvider.System;
        }

        public async Task<MembershipFeeContext?> GetFeeContextAsync(Guid membershipId, CancellationToken cancellationToken) =>
            await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x => x.Id == membershipId && !x.IsDelete)
                .Select(x => new MembershipFeeContext(
                    x.Id,
                    x.PatientId,
                    x.MemberNumber,
                    x.MembershipTier!.TierName,
                    x.MembershipStatus,
                    x.RegistrationType,
                    x.FeeAmountSnapshot,
                    x.PaymentDueAt,
                    x.FeeChargeReferenceId,
                    x.RowVersion))
                .FirstOrDefaultAsync(cancellationToken);

        /// <summary>
        /// Menyimpan rujukan tagihan di membership (<c>FeeChargeReferenceId</c>). Idempotent: rujukan yang sama
        /// tidak mengubah apa pun; rujukan berbeda ditolak. Tidak membuka transaksi sendiri.
        /// </summary>
        public async Task AttachChargeAsync(
            Guid membershipId,
            Guid chargeId,
            Guid actorUserId,
            CancellationToken cancellationToken,
            string? reason = null)
        {
            var membership = await _dbContext.Set<MstPatientMembership>()
                .FirstOrDefaultAsync(x => x.Id == membershipId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Membership tidak ditemukan.");

            if (membership.FeeChargeReferenceId == chargeId) return;
            if (membership.FeeChargeReferenceId.HasValue)
                throw new InvalidOperationException("Membership sudah terhubung ke tagihan iuran lain.");

            var now = _timeProvider.GetUtcNow();
            membership.FeeChargeReferenceId = chargeId;
            membership.RowVersion = Guid.NewGuid();
            membership.UpdateDateTime = now.UtcDateTime;
            membership.UpdateBy = actorUserId;

            var history = NewHistory(membership.Id, membership.MembershipStatus, membership.MembershipStatus,
                FeeChargeLinkedActionCode, actorUserId, now, MembershipChangeSource.Billing);
            history.ReferenceId = chargeId;
            history.Reason = reason ?? "Tagihan iuran dibuat di Billing.";
            _dbContext.Set<PatMembershipStatusHistory>().Add(history);

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// `ConfirmFeePaidAsync(membershipId, chargeReferenceId, paidAt)` (INT-05, VAL-20, VAL-21). Mengaktifkan
        /// membership hanya bila: tagihan memang milik membership ini, statusnya Menunggu Pembayaran, dibayar
        /// sebelum batas bayar, dan tidak melanggar invariant satu Aktif / satu Terjadwal per pasien. Idempotent
        /// menurut (membership, tagihan): panggilan ulang tidak menggandakan riwayat atau aktivasi.
        /// Dijalankan satu transaksi di bawah kunci per pasien yang sama dengan <see cref="PatientMembershipService"/>.
        /// </summary>
        public async Task<MembershipFeeConfirmationResult> ConfirmFeePaidAsync(
            Guid membershipId,
            Guid chargeReferenceId,
            DateTimeOffset paidAt,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var patientId = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x => x.Id == membershipId && !x.IsDelete)
                .Select(x => (Guid?)x.PatientId)
                .FirstOrDefaultAsync(cancellationToken);
            if (patientId == null)
                return new(MembershipFeeConfirmationOutcome.NotFound, "Membership tidak ditemukan.");

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await AcquirePatientLockAsync(patientId.Value, cancellationToken);

            var membership = await _dbContext.Set<MstPatientMembership>()
                .FirstOrDefaultAsync(x => x.Id == membershipId && !x.IsDelete, cancellationToken);
            if (membership == null)
                return new(MembershipFeeConfirmationOutcome.NotFound, "Membership tidak ditemukan.");

            if (membership.FeeChargeReferenceId != chargeReferenceId)
                return new(MembershipFeeConfirmationOutcome.ChargeMismatch,
                    "Tagihan tidak sesuai dengan membership ini.");

            if (membership.MembershipStatus != MembershipStatus.PendingPayment)
            {
                var alreadyApplied = await _dbContext.Set<PatMembershipStatusHistory>()
                    .AsNoTracking()
                    .AnyAsync(h => h.PatientMembershipId == membership.Id
                        && h.ActionCode == FeePaidActionCode
                        && h.ReferenceId == chargeReferenceId, cancellationToken);

                return alreadyApplied
                    ? new(MembershipFeeConfirmationOutcome.AlreadyConfirmed, "Pelunasan sudah diterapkan sebelumnya.")
                    : new(MembershipFeeConfirmationOutcome.NotPending,
                        "Membership tidak lagi menunggu pembayaran; pembayaran iuran perlu ditinjau.");
            }

            if (membership.PaymentDueAt.HasValue && paidAt > membership.PaymentDueAt.Value)
                return new(MembershipFeeConfirmationOutcome.PaidAfterDue,
                    "Pembayaran iuran masuk setelah batas bayar; membership tidak diaktifkan.");

            var now = _timeProvider.GetUtcNow();
            var today = _statusResolver.GetTodayWib();
            var paidDate = _statusResolver.ToWibDate(paidAt);

            var others = await _dbContext.Set<MstPatientMembership>()
                .Where(x => x.PatientId == membership.PatientId && !x.IsDelete && x.Id != membership.Id
                    && (x.MembershipStatus == MembershipStatus.Active || x.MembershipStatus == MembershipStatus.Scheduled))
                .ToListAsync(cancellationToken);

            var runningActive = others.FirstOrDefault(x =>
                x.MembershipStatus == MembershipStatus.Active && (x.ExpiredDate == null || x.ExpiredDate >= today));
            var staleActive = others.Where(x =>
                x.MembershipStatus == MembershipStatus.Active && x.ExpiredDate != null && x.ExpiredDate < today).ToList();
            var scheduled = others.Where(x => x.MembershipStatus == MembershipStatus.Scheduled).ToList();

            // Periode lama yang sudah lewat tetapi belum disapu proses harian ditutup dulu (index unik satu Aktif).
            foreach (var stale in staleActive)
            {
                stale.MembershipStatus = MembershipStatus.Expired;
                stale.IsActive = false;
                stale.IsPrimary = false;
                Touch(stale, now, actorUserId);
                AddHistory(stale.Id, MembershipStatus.Active, MembershipStatus.Expired, "EXPIRED", actorUserId, now,
                    MembershipChangeSource.System, null, null);
            }

            var outcome = MembershipFeeConfirmationOutcome.Activated;
            DateTime periodStart;
            var activateNow = true;

            switch (membership.RegistrationType)
            {
                case MembershipRegistrationType.New:
                    if (runningActive != null || scheduled.Count > 0)
                        return await RejectConflictAsync(transaction, "Pasien sudah punya membership Aktif atau Terjadwal; pendaftaran baru tidak dapat diaktifkan.");
                    periodStart = paidDate;
                    break;

                case MembershipRegistrationType.Renewal:
                    if (runningActive != null)
                    {
                        // CRM-DEC-024: disambung setelah periode lama, paling banyak satu Terjadwal.
                        if (scheduled.Count > 0)
                            return await RejectConflictAsync(transaction, "Perpanjangan untuk periode berikutnya sudah dibayar.");
                        periodStart = (runningActive.ExpiredDate ?? paidDate).AddDays(1);
                        activateNow = false;
                        outcome = MembershipFeeConfirmationOutcome.Scheduled;
                    }
                    else
                    {
                        if (scheduled.Count > 0)
                            return await RejectConflictAsync(transaction, "Pasien sudah punya periode Terjadwal.");
                        periodStart = paidDate;
                    }
                    break;

                case MembershipRegistrationType.Upgrade:
                    // CRM-DEC-012: periode baru 12 bulan dari tanggal lunas; membership lama ditutup (Upgraded), sisa hangus.
                    if (runningActive != null)
                    {
                        runningActive.MembershipStatus = MembershipStatus.Upgraded;
                        runningActive.IsActive = false;
                        runningActive.IsPrimary = false;
                        Touch(runningActive, now, actorUserId);
                        AddHistory(runningActive.Id, MembershipStatus.Active, MembershipStatus.Upgraded, UpgradedActionCode,
                            actorUserId, now, MembershipChangeSource.Billing,
                            $"Diganti oleh {membership.MemberNumber} setelah iuran upgrade lunas.", membership.Id);
                    }
                    // CRM-DEC-025: periode Terjadwal dibatalkan; refund penuh diminta, tidak dieksekusi otomatis.
                    foreach (var next in scheduled)
                    {
                        next.MembershipStatus = MembershipStatus.Cancelled;
                        next.CancellationCategory = MembershipCancellationCategory.SupersededByUpgrade;
                        next.CancellationReason = $"Digantikan upgrade {membership.MemberNumber}.";
                        next.IsActive = false;
                        next.IsPrimary = false;
                        next.IsCancel = true;
                        next.CancelDateTime = now.UtcDateTime;
                        next.CancelBy = actorUserId;
                        Touch(next, now, actorUserId);
                        AddHistory(next.Id, MembershipStatus.Scheduled, MembershipStatus.Cancelled, "CANCELLED", actorUserId, now,
                            MembershipChangeSource.Billing, next.CancellationReason, membership.Id);
                        AddHistory(next.Id, MembershipStatus.Cancelled, MembershipStatus.Cancelled, RefundRequestedAction, actorUserId, now,
                            MembershipChangeSource.Billing, RefundPendingNote, next.FeeChargeReferenceId);
                    }
                    periodStart = paidDate;
                    break;

                default:
                    return new(MembershipFeeConfirmationOutcome.NotPending,
                        "Jenis pendaftaran ini tidak diaktifkan lewat pelunasan iuran.");
            }

            // Status membership lama harus tersimpan lebih dulu: index unik "satu Aktif per pasien" diperiksa per baris.
            await _dbContext.SaveChangesAsync(cancellationToken);

            membership.PeriodStartDate = periodStart;
            membership.ExpiredDate = periodStart.AddYears(1).AddDays(-1);
            Touch(membership, now, actorUserId);

            var fromStatus = membership.MembershipStatus;
            if (activateNow)
            {
                membership.MembershipStatus = MembershipStatus.Active;
                membership.IsActive = true;
                membership.IsPrimary = true;
                membership.ActivatedAt = paidAt;
            }
            else
            {
                membership.MembershipStatus = MembershipStatus.Scheduled;
                membership.IsActive = false;
                membership.IsPrimary = false;
            }

            AddHistory(membership.Id, fromStatus, fromStatus, FeePaidActionCode, actorUserId, now,
                MembershipChangeSource.Billing, $"Iuran lunas pada {paidAt:yyyy-MM-dd HH:mm} UTC.", chargeReferenceId);
            AddHistory(membership.Id, fromStatus, membership.MembershipStatus,
                activateNow ? ActivatedActionCode : ScheduledActionCode, actorUserId, now,
                MembershipChangeSource.Billing, null, chargeReferenceId);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await _statusResolver.SyncPatientProjectionAsync(membership.PatientId, actorUserId, cancellationToken);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
            {
                // Pengaman kedua: index unik per status menolak bila kunci per pasien terlewati.
                await transaction.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
                return new(MembershipFeeConfirmationOutcome.ActiveConflict,
                    "Aktivasi ditolak karena pasien sudah punya membership Aktif atau Terjadwal lain.");
            }

            await _loggerService.AuditAsync(
                LogCategory,
                "PatientMembership.FeePaidConfirmed",
                $"Iuran membership {membership.MemberNumber} lunas; status menjadi {membership.MembershipStatus}.",
                new
                {
                    MembershipId = membership.Id,
                    ChargeReferenceId = chargeReferenceId,
                    membership.RegistrationType,
                    membership.PeriodStartDate,
                    membership.ExpiredDate,
                    ActorUserId = actorUserId
                });

            return new(outcome, outcome == MembershipFeeConfirmationOutcome.Activated
                ? "Membership aktif."
                : "Perpanjangan terjadwal menyambung periode berjalan.");
        }

        private async Task<MembershipFeeConfirmationResult> RejectConflictAsync(
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction,
            string message)
        {
            // Perubahan penutupan periode basi (jika ada) dibatalkan bersama transaksi; dana tetap tercatat di Billing.
            await transaction.RollbackAsync();
            _dbContext.ChangeTracker.Clear();
            return new(MembershipFeeConfirmationOutcome.ActiveConflict, message);
        }

        private static void Touch(MstPatientMembership membership, DateTimeOffset now, Guid actorUserId)
        {
            membership.RowVersion = Guid.NewGuid();
            membership.UpdateDateTime = now.UtcDateTime;
            membership.UpdateBy = actorUserId;
        }

        private void AddHistory(
            Guid membershipId,
            MembershipStatus? fromStatus,
            MembershipStatus toStatus,
            string actionCode,
            Guid actorUserId,
            DateTimeOffset occurredAt,
            MembershipChangeSource source,
            string? reason,
            Guid? referenceId)
        {
            var history = NewHistory(membershipId, fromStatus, toStatus, actionCode, actorUserId, occurredAt, source);
            history.Reason = reason is { Length: > 500 } ? reason[..500] : reason;
            history.ReferenceId = referenceId;
            _dbContext.Set<PatMembershipStatusHistory>().Add(history);
        }

        private static PatMembershipStatusHistory NewHistory(
            Guid membershipId,
            MembershipStatus? fromStatus,
            MembershipStatus toStatus,
            string actionCode,
            Guid actorUserId,
            DateTimeOffset occurredAt,
            MembershipChangeSource source) => new()
        {
            Id = Guid.NewGuid(),
            PatientMembershipId = membershipId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ActionCode = actionCode,
            Source = source,
            ActorUserId = actorUserId == Guid.Empty ? null : actorUserId,
            OccurredAt = occurredAt,
            CreateDateTime = occurredAt.UtcDateTime,
            CreateBy = actorUserId
        };

        // Kunci yang sama dengan PatientMembershipService, sehingga pendaftaran, pembatalan, proses harian, dan
        // konfirmasi pelunasan satu pasien tidak pernah berjalan bersamaan.
        private async Task AcquirePatientLockAsync(Guid patientId, CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsNpgsql())
                return;

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [$"PAT_MEMBERSHIP_{patientId:N}"],
                cancellationToken);
        }
    }
}
