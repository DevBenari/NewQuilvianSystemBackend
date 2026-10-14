using System.Globalization;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services
{
    /// <summary>Hasil satu perintah membership: kode HTTP, membership yang terdampak, dan pesan bagi pengguna.</summary>
    public sealed record PatientMembershipCommandResult(int StatusCode, Guid? PatientMembershipId, string Message)
    {
        public bool IsSuccess => StatusCode == StatusCodes.Status200OK;
    }

    /// <summary>Ringkasan satu putaran pekerjaan harian membership (BE-CRM-020).</summary>
    public sealed class MembershipLifecycleRunResult
    {
        public int Expired { get; set; }
        public int ScheduledStarted { get; set; }
        public int UnpaidCancelled { get; set; }
        public int ProjectionsSynced { get; set; }
        public int Failures { get; set; }
    }

    /// <summary>
    /// Pemilik seluruh perintah yang mengubah membership (crm-membership `02-backend-architecture.md` §4.5).
    /// Setiap perintah adalah satu transaksi di bawah kunci per pasien, sehingga invariant
    /// "paling banyak satu Aktif, satu Menunggu Pembayaran, satu Terjadwal" (INV-01..03) tidak
    /// bisa dilanggar dua petugas yang menyimpan bersamaan. Index unik per status menjadi pengaman kedua.
    /// </summary>
    public class PatientMembershipService
    {
        public const string PaymentDueDaysConfigKey = "PatientMembership:PaymentDueDays";
        public const string RenewalWindowDaysConfigKey = "PatientMembership:RenewalWindowDays";

        private const int DefaultPaymentDueDays = 7;
        private const int DefaultRenewalWindowDays = 30;
        private const string LogCategory = "HealthServices.PatientManagement.Membership";

        // Deret nomor member milik modul ini (QBE-CODE-005); deret baru memakai NEVER (DEC-PLT-004).
        private const string MemberNumberSequenceKey = "PAT_MEMBERSHIP_NUMBER";
        private const string MemberNumberPrefix = "MBR";
        private const int MemberNumberDigits = 6;

        private static readonly MembershipTierType[] RegistrableTierTypes =
        [
            MembershipTierType.Silver,
            MembershipTierType.Gold,
            MembershipTierType.Platinum,
            MembershipTierType.Executive
        ];

        private static readonly CultureInfo Indonesia = CultureInfo.GetCultureInfo("id-ID");

        private readonly ApplicationDbContext _dbContext;
        private readonly NumberSeriesAllocator _numberSeriesAllocator;
        private readonly PatientMembershipStatusResolver _statusResolver;
        private readonly LoggerService _loggerService;
        private readonly TimeProvider _timeProvider;
        private readonly int _paymentDueDays;
        private readonly int _renewalWindowDays;

        public PatientMembershipService(
            ApplicationDbContext dbContext,
            NumberSeriesAllocator numberSeriesAllocator,
            PatientMembershipStatusResolver statusResolver,
            LoggerService loggerService,
            IConfiguration configuration,
            TimeProvider? timeProvider = null)
        {
            _dbContext = dbContext;
            _numberSeriesAllocator = numberSeriesAllocator;
            _statusResolver = statusResolver;
            _loggerService = loggerService;
            _timeProvider = timeProvider ?? TimeProvider.System;
            _paymentDueDays = int.TryParse(configuration[PaymentDueDaysConfigKey], out var days) && days > 0
                ? days
                : DefaultPaymentDueDays;
            _renewalWindowDays = int.TryParse(configuration[RenewalWindowDaysConfigKey], out var window) && window > 0
                ? window
                : DefaultRenewalWindowDays;
        }

        /// <summary>
        /// Mendaftarkan membership baru berstatus Menunggu Pembayaran (CRM-DEC-003, 004, 006, 023).
        /// Membership baru aktif hanya lewat kabar iuran lunas dari billing (EPIC CRM-08).
        /// </summary>
        public async Task<PatientMembershipCommandResult> RegisterAsync(
            RegisterPatientMembershipRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request.PatientId == Guid.Empty)
                return Fail(StatusCodes.Status400BadRequest, "Pasien wajib dipilih.");

            if (request.MembershipTierId == Guid.Empty)
                return Fail(StatusCodes.Status400BadRequest, "Tingkat membership wajib dipilih.");

            if (!TryNormalizeNotes(request.Notes, out var notes, out var notesError))
                return notesError!;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await AcquirePatientLockAsync(request.PatientId, cancellationToken);

            // VAL-01 — membership selalu menempel pada pasien ber-No. RM yang aktif.
            var patient = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => x.Id == request.PatientId && x.IsActive && !x.IsDelete)
                .Select(x => new { x.Id, x.MedicalRecordNumber })
                .FirstOrDefaultAsync(cancellationToken);

            if (patient == null || string.IsNullOrWhiteSpace(patient.MedicalRecordNumber))
                return Fail(StatusCodes.Status404NotFound, "Pasien tidak ditemukan atau tidak aktif. Daftarkan pasien lebih dulu.");

            var (tier, tierError) = await LoadRegistrableTierAsync(request.MembershipTierId, cancellationToken);
            if (tierError != null)
                return tierError;

            // VAL-04 — daftar baru tidak boleh menumpuk di atas membership Aktif.
            var active = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x => x.PatientId == request.PatientId && !x.IsDelete && x.MembershipStatus == MembershipStatus.Active)
                .Select(x => new { TierName = x.MembershipTier!.TierName, x.ExpiredDate })
                .FirstOrDefaultAsync(cancellationToken);

            if (active != null)
            {
                var sampai = active.ExpiredDate.HasValue
                    ? $" sampai {FormatDate(active.ExpiredDate.Value)}"
                    : string.Empty;
                return Fail(StatusCodes.Status409Conflict,
                    $"Pasien sudah member {active.TierName}{sampai}. Gunakan perpanjangan atau upgrade.");
            }

            var pendingError = await CheckNoPendingAsync(request.PatientId, cancellationToken);
            if (pendingError != null)
                return pendingError;

            return await CreatePendingAsync(
                transaction, request.PatientId, tier!, MembershipRegistrationType.New, null, notes, actorUserId,
                "PatientMembership.Register", "Pendaftaran membership tersimpan dan menunggu pembayaran iuran.",
                cancellationToken);
        }

        /// <summary>
        /// Mendaftarkan perpanjangan untuk membership Aktif <paramref name="activeMembershipId"/>
        /// (CRM-DEC-024). Tingkatnya sama dengan yang Aktif. Perpanjangan dibuka paling cepat
        /// <c>PatientMembership:RenewalWindowDays</c> (bawaan 30) hari sebelum tanggal akhir. Setelah
        /// lunas statusnya Terjadwal dan menyambung periode lama — bagian itu milik EPIC CRM-08.
        /// </summary>
        public async Task<PatientMembershipCommandResult> RenewAsync(
            Guid activeMembershipId,
            RenewPatientMembershipRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (!TryNormalizeNotes(request.Notes, out var notes, out var notesError))
                return notesError!;

            var patientId = await FindPatientIdAsync(activeMembershipId, cancellationToken);
            if (patientId == null)
                return Fail(StatusCodes.Status404NotFound, "Membership tidak ditemukan.");

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await AcquirePatientLockAsync(patientId.Value, cancellationToken);

            var (active, activeError) = await LoadActiveForChangeAsync(activeMembershipId, request.ExpectedRowVersion, "Perpanjangan", cancellationToken);
            if (activeError != null)
                return activeError;

            // VAL-07 — jendela perpanjangan dihitung dengan kalender WIB.
            var today = _statusResolver.GetTodayWib();
            var opensOn = active!.ExpiredDate!.Value.AddDays(-_renewalWindowDays);
            if (today < opensOn)
                return Fail(StatusCodes.Status409Conflict, $"Perpanjangan baru bisa mulai {FormatDate(opensOn)}.");

            // VAL-08 — paling banyak satu perpanjangan Terjadwal (INV-02).
            var hasScheduled = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .AnyAsync(x => x.PatientId == active.PatientId && !x.IsDelete && x.MembershipStatus == MembershipStatus.Scheduled, cancellationToken);
            if (hasScheduled)
                return Fail(StatusCodes.Status409Conflict, "Perpanjangan untuk periode berikutnya sudah dibayar.");

            var pendingError = await CheckNoPendingAsync(active.PatientId, cancellationToken);
            if (pendingError != null)
                return pendingError;

            var (tier, tierError) = await LoadRegistrableTierAsync(active.MembershipTierId, cancellationToken);
            if (tierError != null)
                return tierError;

            return await CreatePendingAsync(
                transaction, active.PatientId, tier!, MembershipRegistrationType.Renewal, active.Id, notes, actorUserId,
                "PatientMembership.Renew", "Perpanjangan membership tersimpan dan menunggu pembayaran iuran.",
                cancellationToken);
        }

        /// <summary>
        /// Mendaftarkan upgrade dari membership Aktif <paramref name="activeMembershipId"/> ke
        /// tingkat yang lebih tinggi (CRM-DEC-012, CRM-DEC-029: urutan mengikuti
        /// <c>PriorityLevel</c>). Membership lama baru ditutup saat iuran upgrade lunas (EPIC CRM-08).
        /// </summary>
        public async Task<PatientMembershipCommandResult> UpgradeAsync(
            Guid activeMembershipId,
            UpgradePatientMembershipRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (request.MembershipTierId == Guid.Empty)
                return Fail(StatusCodes.Status400BadRequest, "Tingkat tujuan upgrade wajib dipilih.");

            if (!TryNormalizeNotes(request.Notes, out var notes, out var notesError))
                return notesError!;

            var patientId = await FindPatientIdAsync(activeMembershipId, cancellationToken);
            if (patientId == null)
                return Fail(StatusCodes.Status404NotFound, "Membership tidak ditemukan.");

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await AcquirePatientLockAsync(patientId.Value, cancellationToken);

            var (active, activeError) = await LoadActiveForChangeAsync(activeMembershipId, request.ExpectedRowVersion, "Upgrade", cancellationToken);
            if (activeError != null)
                return activeError;

            var (tier, tierError) = await LoadRegistrableTierAsync(request.MembershipTierId, cancellationToken);
            if (tierError != null)
                return tierError;

            // VAL-09 — hanya ke tingkat yang lebih tinggi; turun tingkat tidak tersedia di MVP.
            if (tier!.PriorityLevel <= active!.TierPriorityLevel)
                return Fail(StatusCodes.Status409Conflict, $"Upgrade hanya ke tingkat yang lebih tinggi dari {active.TierName}.");

            var pendingError = await CheckNoPendingAsync(active.PatientId, cancellationToken);
            if (pendingError != null)
                return pendingError;

            return await CreatePendingAsync(
                transaction, active.PatientId, tier, MembershipRegistrationType.Upgrade, active.Id, notes, actorUserId,
                "PatientMembership.Upgrade", "Upgrade membership tersimpan dan menunggu pembayaran iuran.",
                cancellationToken);
        }

        /// <summary>
        /// Membatalkan membership (CRM-DEC-013, CRM-DEC-025, CRM-DEC-029). Alasan wajib dan riwayat
        /// tidak pernah dihapus.
        /// <list type="bullet">
        /// <item>Menunggu Pembayaran → Dibatalkan; pembatalan tagihan iuran dicatat menunggu billing.</item>
        /// <item>Aktif + permintaan pasien → Dibatalkan tanpa refund; Terjadwal milik pasien ikut
        /// dibatalkan dengan permintaan refund penuh.</item>
        /// <item>Aktif + salah input → ditolak (fail-closed) sampai pemakaian manfaat dan refund
        /// dapat diperiksa lewat billing (INT-07, DEC-CRM-001).</item>
        /// <item>Terjadwal → Dibatalkan dengan permintaan refund penuh.</item>
        /// </list>
        /// </summary>
        public async Task<PatientMembershipCommandResult> CancelAsync(
            Guid membershipId,
            CancelPatientMembershipRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            // VAL-11 — petugas hanya memilih permintaan pasien atau salah input.
            if (request.Category is not (MembershipCancellationCategory.PatientRequest or MembershipCancellationCategory.InputError))
                return Fail(StatusCodes.Status400BadRequest, "Pilih alasan: permintaan pasien atau salah input.");

            if (!TryNormalizeReason(request.Reason, required: true, out var reason, out var reasonError))
                return reasonError!;

            var patientId = await FindPatientIdAsync(membershipId, cancellationToken);
            if (patientId == null)
                return Fail(StatusCodes.Status404NotFound, "Membership tidak ditemukan.");

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await AcquirePatientLockAsync(patientId.Value, cancellationToken);

            var (membership, loadError) = await LoadForChangeAsync(membershipId, request.ExpectedRowVersion, cancellationToken);
            if (loadError != null)
                return loadError;

            var now = _timeProvider.GetUtcNow();
            var fromStatus = membership!.MembershipStatus;

            switch (fromStatus)
            {
                case MembershipStatus.NeedsVerification:
                    return Fail(StatusCodes.Status409Conflict, "Data lama ditolak lewat verifikasi, bukan pembatalan.");

                case MembershipStatus.Active when request.Category == MembershipCancellationCategory.InputError:
                    return Fail(StatusCodes.Status409Conflict,
                        "Pembatalan karena salah input belum bisa diproses: pemeriksaan pemakaian diskon dan refund iuran menunggu modul kasir.");

                case MembershipStatus.PendingPayment:
                    MarkCancelled(membership, request.Category, reason, actorUserId, now);
                    AddHistory(membership.Id, fromStatus, MembershipStatus.Cancelled, "CANCELLED", actorUserId, now, reason);
                    AddHistory(membership.Id, MembershipStatus.Cancelled, MembershipStatus.Cancelled, FeeChargeCancelRequestedAction, actorUserId, now, PendingBillingNote);
                    break;

                case MembershipStatus.Scheduled:
                    MarkCancelled(membership, request.Category, reason, actorUserId, now);
                    AddHistory(membership.Id, fromStatus, MembershipStatus.Cancelled, "CANCELLED", actorUserId, now, reason);
                    AddHistory(membership.Id, MembershipStatus.Cancelled, MembershipStatus.Cancelled, RefundRequestedAction, actorUserId, now, PendingBillingNote);
                    break;

                case MembershipStatus.Active:
                    MarkCancelled(membership, request.Category, reason, actorUserId, now);
                    AddHistory(membership.Id, fromStatus, MembershipStatus.Cancelled, "CANCELLED", actorUserId, now, reason);

                    var scheduled = await _dbContext.Set<MstPatientMembership>()
                        .Where(x => x.PatientId == membership.PatientId && !x.IsDelete && x.MembershipStatus == MembershipStatus.Scheduled)
                        .ToListAsync(cancellationToken);
                    foreach (var next in scheduled)
                    {
                        MarkCancelled(next, request.Category, $"Ikut dibatalkan bersama {membership.MemberNumber}: {reason}", actorUserId, now);
                        AddHistory(next.Id, MembershipStatus.Scheduled, MembershipStatus.Cancelled, "CANCELLED", actorUserId, now, next.CancellationReason);
                        AddHistory(next.Id, MembershipStatus.Cancelled, MembershipStatus.Cancelled, RefundRequestedAction, actorUserId, now, PendingBillingNote);
                    }
                    break;

                default:
                    return Fail(StatusCodes.Status409Conflict, "Membership yang sudah berakhir tidak bisa dibatalkan.");
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _statusResolver.SyncPatientProjectionAsync(membership.PatientId, actorUserId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientMembership.Cancel",
                $"Membership {membership.Id} dibatalkan dari {fromStatus} ({request.Category}).");

            return new PatientMembershipCommandResult(StatusCodes.Status200OK, membership.Id, "Membership dibatalkan.");
        }

        /// <summary>
        /// Memutuskan membership lama berstatus Perlu Verifikasi (CRM-DEC-021, CRM-DEC-029).
        /// Diakui: periode sah wajib diisi; tanggal akhir sudah lewat → Kedaluwarsa, selain itu Aktif
        /// (tanpa bertabrakan dengan Aktif lain). Ditolak: Dibatalkan dengan alasan.
        /// </summary>
        public async Task<PatientMembershipCommandResult> VerifyLegacyAsync(
            Guid membershipId,
            VerifyLegacyPatientMembershipRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var isAccept = request.Decision == MembershipVerificationDecision.Accept;
            if (!isAccept && request.Decision != MembershipVerificationDecision.Reject)
                return Fail(StatusCodes.Status400BadRequest, "Pilih keputusan: akui atau tolak.");

            if (!TryNormalizeReason(request.Reason, required: !isAccept, out var reason, out var reasonError))
                return reasonError!;

            var patientId = await FindPatientIdAsync(membershipId, cancellationToken);
            if (patientId == null)
                return Fail(StatusCodes.Status404NotFound, "Membership tidak ditemukan.");

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            await AcquirePatientLockAsync(patientId.Value, cancellationToken);

            var (membership, loadError) = await LoadForChangeAsync(membershipId, request.ExpectedRowVersion, cancellationToken);
            if (loadError != null)
                return loadError;

            // VAL-13
            if (membership!.MembershipStatus != MembershipStatus.NeedsVerification)
                return Fail(StatusCodes.Status409Conflict, "Membership ini tidak sedang menunggu verifikasi.");

            var now = _timeProvider.GetUtcNow();
            MembershipStatus toStatus;
            string actionCode;

            if (isAccept)
            {
                // VAL-14 — periode sah: mulai ≤ akhir dan paling panjang satu masa berlaku tingkat.
                var validityMonths = await _dbContext.Set<MstMembershipTier>()
                    .AsNoTracking()
                    .Where(x => x.Id == membership.MembershipTierId)
                    .Select(x => x.ValidityMonths)
                    .FirstOrDefaultAsync(cancellationToken);
                if (validityMonths <= 0)
                    validityMonths = 12;

                var start = request.PeriodStartDate?.Date;
                var end = request.PeriodEndDate?.Date;
                if (start == null || end == null || start > end || end > start.Value.AddMonths(validityMonths).AddDays(-1))
                    return Fail(StatusCodes.Status400BadRequest,
                        $"Periode yang diakui tidak sah. Tanggal mulai harus sebelum tanggal akhir dan maksimal {validityMonths} bulan.");

                toStatus = end < _statusResolver.GetTodayWib() ? MembershipStatus.Expired : MembershipStatus.Active;

                // VAL-15 — tidak boleh bertabrakan dengan membership Aktif lain (INV-01).
                if (toStatus == MembershipStatus.Active)
                {
                    var hasActive = await _dbContext.Set<MstPatientMembership>()
                        .AsNoTracking()
                        .AnyAsync(x => x.PatientId == membership.PatientId && x.Id != membership.Id && !x.IsDelete
                            && x.MembershipStatus == MembershipStatus.Active, cancellationToken);
                    if (hasActive)
                        return Fail(StatusCodes.Status409Conflict,
                            "Pasien sudah punya membership aktif. Tolak data lama ini atau batalkan yang aktif lebih dulu.");
                }

                membership.PeriodStartDate = DateTime.SpecifyKind(start.Value, DateTimeKind.Unspecified);
                membership.ExpiredDate = DateTime.SpecifyKind(end.Value, DateTimeKind.Unspecified);
                membership.MembershipStatus = toStatus;
                membership.IsActive = toStatus == MembershipStatus.Active;
                membership.IsPrimary = toStatus == MembershipStatus.Active;
                if (toStatus == MembershipStatus.Active)
                    membership.ActivatedAt = now;
                Touch(membership, actorUserId, now);
                actionCode = "LEGACY_ACCEPTED";
            }
            else
            {
                MarkCancelled(membership, null, reason, actorUserId, now);
                toStatus = MembershipStatus.Cancelled;
                actionCode = "LEGACY_REJECTED";
            }

            AddHistory(membership.Id, MembershipStatus.NeedsVerification, toStatus, actionCode, actorUserId, now, reason);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                await transaction.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
                return Fail(StatusCodes.Status409Conflict,
                    "Pasien sudah punya membership aktif. Tolak data lama ini atau batalkan yang aktif lebih dulu.");
            }

            await _statusResolver.SyncPatientProjectionAsync(membership.PatientId, actorUserId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "PatientMembership.VerifyLegacy",
                $"Membership lama {membership.Id} diverifikasi: {actionCode}, status {toStatus}.");

            return new PatientMembershipCommandResult(StatusCodes.Status200OK, membership.Id,
                isAccept ? "Data lama diakui." : "Data lama ditolak.");
        }

        /// <summary>
        /// Mengubah catatan — satu-satunya ubah yang diizinkan setelah membership tersimpan (CRM-DEC-022).
        /// </summary>
        public async Task<PatientMembershipCommandResult> UpdateNotesAsync(
            Guid membershipId,
            UpdatePatientMembershipNotesRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (!TryNormalizeNotes(request.Notes, out var notes, out var notesError))
                return notesError!;

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var (membership, loadError) = await LoadForChangeAsync(membershipId, request.ExpectedRowVersion, cancellationToken);
            if (loadError != null)
                return loadError;

            // VAL-16 — status akhir tidak bisa diubah.
            if (membership!.MembershipStatus is MembershipStatus.Expired or MembershipStatus.Cancelled or MembershipStatus.Upgraded)
                return Fail(StatusCodes.Status409Conflict, "Membership yang sudah berakhir tidak bisa diubah.");

            if (membership.Notes == notes)
                return new PatientMembershipCommandResult(StatusCodes.Status200OK, membership.Id, "Catatan tidak berubah.");

            var now = _timeProvider.GetUtcNow();
            var history = NewHistory(membership.Id, membership.MembershipStatus, membership.MembershipStatus, "NOTES_UPDATED", actorUserId, now);
            history.OldNotes = membership.Notes;
            history.NewNotes = notes;
            _dbContext.Set<PatMembershipStatusHistory>().Add(history);

            membership.Notes = notes;
            Touch(membership, actorUserId, now);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "PatientMembership.UpdateNotes", $"Catatan membership {membership.Id} diubah.");

            return new PatientMembershipCommandResult(StatusCodes.Status200OK, membership.Id, "Catatan membership diperbarui.");
        }

        /// <summary>Riwayat status satu membership, terbaru di atas; <c>null</c> bila membership tidak ada.</summary>
        public async Task<List<PatientMembershipStatusHistoryResponse>?> GetStatusHistoriesAsync(
            Guid membershipId,
            CancellationToken cancellationToken = default)
        {
            var exists = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == membershipId && !x.IsDelete, cancellationToken);
            if (!exists)
                return null;

            var rows = await _dbContext.Set<PatMembershipStatusHistory>()
                .AsNoTracking()
                .Where(x => x.PatientMembershipId == membershipId && !x.IsDelete)
                .ToListAsync(cancellationToken);

            var actorIds = rows.Where(x => x.ActorUserId.HasValue).Select(x => x.ActorUserId!.Value).Distinct().ToList();
            var actorNames = actorIds.Count == 0
                ? new Dictionary<Guid, string?>()
                : await _dbContext.Users
                    .AsNoTracking()
                    .Where(x => actorIds.Contains(x.Id))
                    .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                    .ToDictionaryAsync(x => x.Id, x => (string?)x.Name, cancellationToken);

            // Urutan memakai OccurredAt lalu Id, tanpa nomor urut tersimpan (QBE-CODE-003).
            return rows
                .OrderByDescending(x => x.OccurredAt)
                .ThenByDescending(x => x.Id)
                .Select(x => new PatientMembershipStatusHistoryResponse
                {
                    Id = x.Id,
                    FromStatus = x.FromStatus,
                    ToStatus = x.ToStatus,
                    ActionCode = x.ActionCode,
                    Source = x.Source,
                    ActorUserId = x.ActorUserId,
                    ActorName = x.ActorUserId.HasValue ? actorNames.GetValueOrDefault(x.ActorUserId.Value) : null,
                    OccurredAt = x.OccurredAt,
                    Reason = x.Reason,
                    ReferenceId = x.ReferenceId,
                    OldNotes = x.OldNotes,
                    NewNotes = x.NewNotes
                })
                .ToList();
        }

        // Riwayat penanda bahwa tindakan uang menunggu billing; dibaca rekonsiliasi saat EPIC CRM-08 tersedia.
        private const string RefundRequestedAction = "REFUND_REQUESTED";
        private const string FeeChargeCancelRequestedAction = "FEE_CHARGE_CANCEL_REQUESTED";
        private const string PendingBillingNote = "Menunggu modul kasir (DEC-CRM-001).";

        // Membership yang akan diubah: ada, belum dihapus, dan belum diubah petugas lain (VAL-17).
        private async Task<(MstPatientMembership? Membership, PatientMembershipCommandResult? Error)> LoadForChangeAsync(
            Guid membershipId,
            Guid expectedRowVersion,
            CancellationToken cancellationToken)
        {
            var membership = await _dbContext.Set<MstPatientMembership>()
                .FirstOrDefaultAsync(x => x.Id == membershipId && !x.IsDelete, cancellationToken);

            if (membership == null)
                return (null, Fail(StatusCodes.Status404NotFound, "Membership tidak ditemukan."));

            if (membership.RowVersion != expectedRowVersion)
                return (null, Fail(StatusCodes.Status409Conflict, "Data ini baru saja diubah petugas lain. Muat ulang lalu coba lagi."));

            return (membership, null);
        }

        private static void MarkCancelled(
            MstPatientMembership membership,
            MembershipCancellationCategory? category,
            string? reason,
            Guid actorUserId,
            DateTimeOffset now)
        {
            membership.MembershipStatus = MembershipStatus.Cancelled;
            membership.CancellationCategory = category;
            membership.CancellationReason = reason is { Length: > 500 } ? reason[..500] : reason;
            membership.IsActive = false;
            membership.IsPrimary = false;
            membership.IsCancel = true;
            membership.CancelDateTime = now.UtcDateTime;
            membership.CancelBy = actorUserId;
            Touch(membership, actorUserId, now);
        }

        private static void Touch(MstPatientMembership membership, Guid actorUserId, DateTimeOffset now)
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
            string? reason)
        {
            var history = NewHistory(membershipId, fromStatus, toStatus, actionCode, actorUserId, occurredAt);
            history.Reason = reason;
            _dbContext.Set<PatMembershipStatusHistory>().Add(history);
        }

        // VAL-10 — alasan 10–500 karakter bila wajib; bila opsional tetap maksimal 500.
        private static bool TryNormalizeReason(string? value, bool required, out string? reason, out PatientMembershipCommandResult? error)
        {
            reason = NormalizeNullableString(value);
            error = null;

            if (required && (reason == null || reason.Length < 10))
                error = Fail(StatusCodes.Status400BadRequest, "Tulis alasan minimal 10 karakter.");
            else if (reason is { Length: > 500 })
                error = Fail(StatusCodes.Status400BadRequest, "Alasan maksimal 500 karakter.");

            return error == null;
        }

        /// <summary>
        /// Mencari pasien aktif lewat No. RM, NIK, atau No. Telp beserta status member masing-masing
        /// (CRM-DEC-010). Semua yang cocok ditampilkan; tidak ada pilihan otomatis. NIK dan No. Telp
        /// dikirim tersamar. Mengembalikan <c>null</c> bila kata kunci kurang dari 3 karakter (VAL-22).
        /// </summary>
        public async Task<PagedResult<PatientMembershipLookupResponse>?> LookupPatientsAsync(
            PatientMembershipLookupQuery query,
            CancellationToken cancellationToken = default)
        {
            var keyword = query.Keyword?.Trim();
            if (string.IsNullOrEmpty(keyword) || keyword.Length < 3)
                return null;

            var pageSize = Math.Clamp(query.PageSize <= 0 ? 25 : query.PageSize, 1, 100);
            var pageNumber = Math.Max(query.PageNumber, 1);
            var lowered = keyword.ToLower();

            var patients = _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .Where(x =>
                    x.MedicalRecordNumber.ToLower().Contains(lowered) ||
                    (x.IdentityNumber != null && x.IdentityNumber.ToLower().Contains(lowered)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(lowered)));

            var totalData = await patients.CountAsync(cancellationToken);

            var rows = await patients
                .OrderBy(x => x.FullName)
                .ThenBy(x => x.MedicalRecordNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new { x.Id, x.MedicalRecordNumber, x.FullName, x.BirthDate, x.IdentityNumber, x.PhoneNumber })
                .ToListAsync(cancellationToken);

            var ids = rows.Select(x => x.Id).ToList();
            var active = await _statusResolver.GetActiveForPatientsAsync(ids, null, cancellationToken);
            var pending = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x => ids.Contains(x.PatientId) && !x.IsDelete && x.MembershipStatus == MembershipStatus.PendingPayment)
                .Select(x => new { x.PatientId, x.MemberNumber, x.PaymentDueAt })
                .ToListAsync(cancellationToken);

            return new PagedResult<PatientMembershipLookupResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = rows.Select(x =>
                {
                    var aktif = active.GetValueOrDefault(x.Id);
                    var menunggu = pending.FirstOrDefault(p => p.PatientId == x.Id);
                    return new PatientMembershipLookupResponse
                    {
                        PatientId = x.Id,
                        MedicalRecordNumber = x.MedicalRecordNumber,
                        FullName = x.FullName,
                        BirthDate = x.BirthDate,
                        IdentityNumberMasked = Mask(x.IdentityNumber, 4, 4),
                        PhoneNumberMasked = Mask(x.PhoneNumber, 4, 3),
                        IsMember = aktif != null,
                        ActiveMembership = aktif == null ? null : ToActiveResponse(aktif),
                        PendingMemberNumber = menunggu?.MemberNumber,
                        PendingPaymentDueAt = menunggu?.PaymentDueAt
                    };
                }).ToList()
            };
        }

        /// <summary>
        /// Status member pasien pada tanggal WIB tertentu (bawaan hari ini) menurut aturan tunggal J1.
        /// Mengembalikan <c>null</c> bila pasien tidak ditemukan.
        /// </summary>
        public async Task<PatientActiveMembershipResponse?> GetPatientActiveMembershipAsync(
            Guid patientId,
            DateTime? dateWib,
            CancellationToken cancellationToken = default)
        {
            var exists = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == patientId && !x.IsDelete, cancellationToken);
            if (!exists)
                return null;

            var date = DateTime.SpecifyKind((dateWib ?? _statusResolver.GetTodayWib()).Date, DateTimeKind.Unspecified);
            var aktif = await _statusResolver.GetActiveAsync(patientId, date, cancellationToken);

            return new PatientActiveMembershipResponse
            {
                PatientId = patientId,
                Date = date,
                IsMember = aktif != null,
                ActiveMembership = aktif == null ? null : ToActiveResponse(aktif)
            };
        }

        private static PatientMembershipActiveSummaryResponse ToActiveResponse(PatientActiveMembership aktif) => new()
        {
            PatientMembershipId = aktif.PatientMembershipId,
            MemberNumber = aktif.MemberNumber,
            MembershipTierId = aktif.MembershipTierId,
            TierCode = aktif.TierCode,
            TierName = aktif.TierName,
            TierType = aktif.TierType,
            PeriodStartDate = aktif.PeriodStartDate,
            PeriodEndDate = aktif.PeriodEndDate
        };

        // Contoh: Mask("3174010101010001", 4, 4) → "3174********0001"; nilai pendek disamarkan seluruhnya.
        private static string? Mask(string? value, int keepStart, int keepEnd)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var text = value.Trim();
            if (text.Length <= keepStart + keepEnd)
                return new string('*', text.Length);

            return text[..keepStart] + new string('*', text.Length - keepStart - keepEnd) + text[^keepEnd..];
        }

        /// <summary>
        /// Pekerjaan harian sistem (crm-membership `BE-CRM-020`, INT-08), aman dijalankan ulang dan
        /// mengejar hari yang terlewat:
        /// <list type="number">
        /// <item>Aktif yang tanggal akhirnya sebelum hari ini (WIB) menjadi Kedaluwarsa (CRM-DEC-005).</item>
        /// <item>Terjadwal yang tanggal mulainya tiba menjadi Aktif, dalam transaksi yang sama dengan
        /// kedaluwarsanya periode lama (CRM-DEC-024).</item>
        /// <item>Menunggu Pembayaran yang lewat batas bayar menjadi Dibatalkan (tidak dibayar, CRM-DEC-014).</item>
        /// <item>Proyeksi status member di pasien diselaraskan dengan aturan tunggal J1.</item>
        /// </list>
        /// Satu pasien satu transaksi, sehingga satu kegagalan tidak menahan yang lain.
        /// </summary>
        public async Task<MembershipLifecycleRunResult> RunDailyLifecycleAsync(CancellationToken cancellationToken = default)
        {
            var result = new MembershipLifecycleRunResult();
            var today = _statusResolver.GetTodayWib();
            var now = _timeProvider.GetUtcNow();

            var periodPatients = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                    ((x.MembershipStatus == MembershipStatus.Active && x.ExpiredDate != null && x.ExpiredDate < today) ||
                     (x.MembershipStatus == MembershipStatus.Scheduled && x.PeriodStartDate != null && x.PeriodStartDate <= today)))
                .Select(x => x.PatientId)
                .Distinct()
                .ToListAsync(cancellationToken);

            foreach (var patientId in periodPatients)
            {
                await RunGuardedAsync(result, patientId, () => AdvancePeriodsAsync(patientId, today, now, result, cancellationToken));
            }

            // Batas bayar dibandingkan di memori: jumlahnya kecil dan perbandingan DateTimeOffset
            // tidak seragam antar-provider.
            var pending = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.MembershipStatus == MembershipStatus.PendingPayment && x.PaymentDueAt != null)
                .Select(x => new { x.Id, x.PatientId, x.PaymentDueAt })
                .ToListAsync(cancellationToken);

            foreach (var item in pending.Where(x => x.PaymentDueAt <= now))
            {
                await RunGuardedAsync(result, item.PatientId, () => CancelUnpaidAsync(item.Id, item.PatientId, now, result, cancellationToken));
            }

            await SweepPatientProjectionsAsync(result, cancellationToken);

            return result;
        }

        private async Task AdvancePeriodsAsync(
            Guid patientId,
            DateTime today,
            DateTimeOffset now,
            MembershipLifecycleRunResult result,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await AcquirePatientLockAsync(patientId, cancellationToken);

            var rows = await _dbContext.Set<MstPatientMembership>()
                .Where(x => x.PatientId == patientId && !x.IsDelete &&
                    (x.MembershipStatus == MembershipStatus.Active || x.MembershipStatus == MembershipStatus.Scheduled))
                .ToListAsync(cancellationToken);

            var expired = 0;
            foreach (var active in rows.Where(x => x.MembershipStatus == MembershipStatus.Active && x.ExpiredDate < today))
            {
                active.MembershipStatus = MembershipStatus.Expired;
                active.IsActive = false;
                active.IsPrimary = false;
                Touch(active, Guid.Empty, now);
                AddSystemHistory(active.Id, MembershipStatus.Active, MembershipStatus.Expired, "EXPIRED", now, null);
                expired++;
            }

            // Kedaluwarsa disimpan lebih dulu: index unik "satu Aktif per pasien" diperiksa per baris,
            // jadi periode lama harus berhenti Aktif sebelum Terjadwal menjadi Aktif.
            if (expired > 0)
                await _dbContext.SaveChangesAsync(cancellationToken);

            var started = 0;
            var stillActive = rows.Any(x => x.MembershipStatus == MembershipStatus.Active);
            var due = rows
                .Where(x => x.MembershipStatus == MembershipStatus.Scheduled && x.PeriodStartDate <= today)
                .OrderBy(x => x.PeriodStartDate)
                .FirstOrDefault();

            if (due != null && !stillActive)
            {
                due.MembershipStatus = MembershipStatus.Active;
                due.IsActive = true;
                due.IsPrimary = true;
                due.ActivatedAt = now;
                Touch(due, Guid.Empty, now);
                AddSystemHistory(due.Id, MembershipStatus.Scheduled, MembershipStatus.Active, "SCHEDULE_STARTED", now, null);
                started++;
            }

            if (expired == 0 && started == 0)
                return;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _statusResolver.SyncPatientProjectionAsync(patientId, Guid.Empty, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            result.Expired += expired;
            result.ScheduledStarted += started;
        }

        private async Task CancelUnpaidAsync(
            Guid membershipId,
            Guid patientId,
            DateTimeOffset now,
            MembershipLifecycleRunResult result,
            CancellationToken cancellationToken)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            await AcquirePatientLockAsync(patientId, cancellationToken);

            var membership = await _dbContext.Set<MstPatientMembership>()
                .FirstOrDefaultAsync(x => x.Id == membershipId && !x.IsDelete, cancellationToken);

            // Diperiksa ulang di bawah kunci: bisa saja sudah lunas atau dibatalkan petugas.
            if (membership == null || membership.MembershipStatus != MembershipStatus.PendingPayment || membership.PaymentDueAt > now)
                return;

            MarkCancelled(membership, MembershipCancellationCategory.Unpaid, "Iuran tidak dibayar sampai batas waktu.", Guid.Empty, now);
            AddSystemHistory(membership.Id, MembershipStatus.PendingPayment, MembershipStatus.Cancelled, "UNPAID_CANCELLED", now, membership.CancellationReason);
            AddSystemHistory(membership.Id, MembershipStatus.Cancelled, MembershipStatus.Cancelled, FeeChargeCancelRequestedAction, now, PendingBillingNote);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            result.UnpaidCancelled++;
        }

        // Pasien yang proyeksinya menyatakan member, atau yang punya membership Aktif, diselaraskan
        // dengan aturan tunggal. Termasuk pasien lama ber-IsMember = true tanpa membership apa pun.
        private async Task SweepPatientProjectionsAsync(MembershipLifecycleRunResult result, CancellationToken cancellationToken)
        {
            var withActive = _dbContext.Set<MstPatientMembership>()
                .Where(x => !x.IsDelete && x.MembershipStatus == MembershipStatus.Active)
                .Select(x => x.PatientId);

            var candidates = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => !x.IsDelete &&
                    (x.IsMember || x.ActivePatientMembershipId != null || x.DefaultMembershipTierId != null || withActive.Contains(x.Id)))
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);

            foreach (var batch in candidates.Chunk(100))
            {
                try
                {
                    var changed = 0;
                    foreach (var patientId in batch)
                    {
                        if (await _statusResolver.SyncPatientProjectionAsync(patientId, Guid.Empty, cancellationToken))
                            changed++;
                    }

                    if (changed > 0)
                        await _dbContext.SaveChangesAsync(cancellationToken);

                    result.ProjectionsSynced += changed;
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _dbContext.ChangeTracker.Clear();
                    result.Failures++;
                    await _loggerService.ErrorAsync(LogCategory, "PatientMembership.SweepProjection", "Gagal menyelaraskan proyeksi status member satu kelompok pasien.", ex);
                }
            }
        }

        private async Task RunGuardedAsync(MembershipLifecycleRunResult result, Guid patientId, Func<Task> work)
        {
            try
            {
                await work();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _dbContext.ChangeTracker.Clear();
                result.Failures++;
                await _loggerService.ErrorAsync(LogCategory, "PatientMembership.DailyLifecycle", $"Gagal memproses siklus hidup membership pasien {patientId}.", ex);
            }
        }

        private void AddSystemHistory(
            Guid membershipId,
            MembershipStatus? fromStatus,
            MembershipStatus toStatus,
            string actionCode,
            DateTimeOffset occurredAt,
            string? reason)
        {
            var history = NewHistory(membershipId, fromStatus, toStatus, actionCode, Guid.Empty, occurredAt);
            history.Source = MembershipChangeSource.System;
            history.Reason = reason;
            _dbContext.Set<PatMembershipStatusHistory>().Add(history);
        }

        private sealed record RegistrableTier(Guid Id, string TierName, MembershipTierType TierType, decimal AnnualFeeAmount, int PriorityLevel);

        private sealed record ActiveMembership(
            Guid Id, Guid PatientId, Guid MembershipTierId, string TierName, int TierPriorityLevel, DateTime? ExpiredDate);

        // VAL-02 / VAL-03 — hanya tingkat berbayar yang aktif dan beriuran.
        private async Task<(RegistrableTier? Tier, PatientMembershipCommandResult? Error)> LoadRegistrableTierAsync(
            Guid tierId,
            CancellationToken cancellationToken)
        {
            var tier = await _dbContext.Set<MstMembershipTier>()
                .AsNoTracking()
                .Where(x => x.Id == tierId && x.IsActive && !x.IsDelete)
                .Select(x => new RegistrableTier(x.Id, x.TierName, x.TierType, x.AnnualFeeAmount, x.PriorityLevel))
                .FirstOrDefaultAsync(cancellationToken);

            if (tier == null || !RegistrableTierTypes.Contains(tier.TierType))
                return (null, Fail(StatusCodes.Status400BadRequest, "Tingkat ini tidak bisa dipilih untuk pendaftaran membership."));

            if (tier.AnnualFeeAmount <= 0)
                return (null, Fail(StatusCodes.Status409Conflict, $"Nominal iuran tingkat {tier.TierName} belum diisi. Hubungi admin master data."));

            return (tier, null);
        }

        // Membership sumber perpanjangan/upgrade wajib masih Aktif, punya periode, dan belum diubah petugas lain (VAL-17).
        private async Task<(ActiveMembership? Active, PatientMembershipCommandResult? Error)> LoadActiveForChangeAsync(
            Guid membershipId,
            Guid expectedRowVersion,
            string actionLabel,
            CancellationToken cancellationToken)
        {
            var row = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x => x.Id == membershipId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.PatientId,
                    x.MembershipTierId,
                    x.MembershipStatus,
                    x.ExpiredDate,
                    x.RowVersion,
                    TierName = x.MembershipTier!.TierName,
                    TierPriorityLevel = x.MembershipTier.PriorityLevel
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (row == null)
                return (null, Fail(StatusCodes.Status404NotFound, "Membership tidak ditemukan."));

            if (row.RowVersion != expectedRowVersion)
                return (null, Fail(StatusCodes.Status409Conflict, "Data ini baru saja diubah petugas lain. Muat ulang lalu coba lagi."));

            if (row.MembershipStatus != MembershipStatus.Active || row.ExpiredDate == null)
                return (null, Fail(StatusCodes.Status409Conflict, $"{actionLabel} hanya untuk membership yang sedang aktif."));

            return (new ActiveMembership(row.Id, row.PatientId, row.MembershipTierId, row.TierName, row.TierPriorityLevel, row.ExpiredDate), null);
        }

        private async Task<Guid?> FindPatientIdAsync(Guid membershipId, CancellationToken cancellationToken) =>
            await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x => x.Id == membershipId && !x.IsDelete)
                .Select(x => (Guid?)x.PatientId)
                .FirstOrDefaultAsync(cancellationToken);

        // VAL-05 — paling banyak satu pendaftaran Menunggu Pembayaran per pasien (INV-03).
        private async Task<PatientMembershipCommandResult?> CheckNoPendingAsync(Guid patientId, CancellationToken cancellationToken)
        {
            var pendingNumber = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x => x.PatientId == patientId && !x.IsDelete && x.MembershipStatus == MembershipStatus.PendingPayment)
                .Select(x => x.MemberNumber)
                .FirstOrDefaultAsync(cancellationToken);

            return pendingNumber == null ? null : PendingConflict(pendingNumber);
        }

        private async Task<PatientMembershipCommandResult> CreatePendingAsync(
            Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction,
            Guid patientId,
            RegistrableTier tier,
            MembershipRegistrationType registrationType,
            Guid? previousMembershipId,
            string? notes,
            Guid actorUserId,
            string logAction,
            string successMessage,
            CancellationToken cancellationToken)
        {
            var now = _timeProvider.GetUtcNow();

            // VAL-06 — nomor member dari deret milik modul, bukan dari request. Nomor yang sudah
            // terbit tetap hangus bila transaksi ini gagal (DEC-PLT-008).
            var memberNumber = await _numberSeriesAllocator.AllocateAsync(
                new NumberAllocationRequest(
                    SequenceKey: MemberNumberSequenceKey,
                    Prefix: MemberNumberPrefix,
                    ResetPolicy: NumberSeriesResetPolicies.Never,
                    SequenceDigits: MemberNumberDigits,
                    ActorUserId: actorUserId,
                    Instant: now),
                cancellationToken);

            var membership = new MstPatientMembership
            {
                Id = Guid.NewGuid(),
                PatientId = patientId,
                MembershipTierId = tier.Id,
                MemberNumber = memberNumber,
                MembershipStatus = MembershipStatus.PendingPayment,
                RegistrationType = registrationType,
                PreviousPatientMembershipId = previousMembershipId,
                JoinDate = _statusResolver.ToWibDate(now),
                PaymentDueAt = now.AddDays(_paymentDueDays),
                FeeAmountSnapshot = tier.AnnualFeeAmount,
                // Proyeksi kompatibilitas: true hanya saat Aktif.
                IsActive = false,
                IsPrimary = false,
                IsCreatedByMarketing = true,
                Notes = notes,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = now.UtcDateTime,
                CreateBy = actorUserId
            };

            _dbContext.Set<MstPatientMembership>().Add(membership);
            _dbContext.Set<PatMembershipStatusHistory>().Add(NewHistory(
                membership.Id, null, MembershipStatus.PendingPayment, "REGISTERED", actorUserId, now));

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // Pengaman kedua: kunci per pasien terlewati, index unik per status menolak.
                await transaction.RollbackAsync(cancellationToken);
                _dbContext.ChangeTracker.Clear();
                return Fail(StatusCodes.Status409Conflict,
                    "Pasien ini sudah punya pendaftaran yang menunggu pembayaran. Muat ulang data lalu periksa kembali.");
            }

            await _loggerService.InfoAsync(
                LogCategory,
                logAction,
                $"Membership {membership.Id} ({registrationType}) tersimpan berstatus {MembershipStatus.PendingPayment}.");

            return new PatientMembershipCommandResult(StatusCodes.Status200OK, membership.Id, successMessage);
        }

        private static bool TryNormalizeNotes(string? value, out string? notes, out PatientMembershipCommandResult? error)
        {
            notes = NormalizeNullableString(value);
            error = notes is { Length: > 250 }
                ? Fail(StatusCodes.Status400BadRequest, "Catatan maksimal 250 karakter.")
                : null;
            return error == null;
        }

        private static string FormatDate(DateTime date) => date.ToString("d MMM yyyy", Indonesia);

        private async Task AcquirePatientLockAsync(Guid patientId, CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsNpgsql())
                return;

            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [$"PAT_MEMBERSHIP_{patientId:N}"],
                cancellationToken);
        }

        private static PatMembershipStatusHistory NewHistory(
            Guid membershipId,
            MembershipStatus? fromStatus,
            MembershipStatus toStatus,
            string actionCode,
            Guid actorUserId,
            DateTimeOffset occurredAt) => new()
        {
            Id = Guid.NewGuid(),
            PatientMembershipId = membershipId,
            FromStatus = fromStatus,
            ToStatus = toStatus,
            ActionCode = actionCode,
            Source = MembershipChangeSource.Staff,
            ActorUserId = actorUserId == Guid.Empty ? null : actorUserId,
            OccurredAt = occurredAt,
            CreateDateTime = occurredAt.UtcDateTime,
            CreateBy = actorUserId
        };

        private static PatientMembershipCommandResult PendingConflict(string memberNumber) =>
            Fail(StatusCodes.Status409Conflict,
                $"Pasien ini sudah punya pendaftaran yang menunggu pembayaran ({memberNumber}).");

        private static PatientMembershipCommandResult Fail(int statusCode, string message) =>
            new(statusCode, null, message);

        private static bool IsUniqueViolation(DbUpdateException ex) =>
            ex.InnerException is Npgsql.PostgresException { SqlState: "23505" };

        private static string? NormalizeNullableString(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
