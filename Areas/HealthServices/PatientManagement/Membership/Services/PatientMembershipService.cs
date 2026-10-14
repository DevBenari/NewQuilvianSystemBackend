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
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services
{
    /// <summary>Hasil satu perintah membership: kode HTTP, membership yang terdampak, dan pesan bagi pengguna.</summary>
    public sealed record PatientMembershipCommandResult(int StatusCode, Guid? PatientMembershipId, string Message)
    {
        public bool IsSuccess => StatusCode == StatusCodes.Status200OK;
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

        private const int DefaultPaymentDueDays = 7;
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

            var notes = NormalizeNullableString(request.Notes);
            if (notes is { Length: > 250 })
                return Fail(StatusCodes.Status400BadRequest, "Catatan maksimal 250 karakter.");

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

            // VAL-02 / VAL-03 — hanya tingkat berbayar yang aktif dan beriuran.
            var tier = await _dbContext.Set<MstMembershipTier>()
                .AsNoTracking()
                .Where(x => x.Id == request.MembershipTierId && x.IsActive && !x.IsDelete)
                .Select(x => new { x.Id, x.TierName, x.TierType, x.AnnualFeeAmount })
                .FirstOrDefaultAsync(cancellationToken);

            if (tier == null || !RegistrableTierTypes.Contains(tier.TierType))
                return Fail(StatusCodes.Status400BadRequest, "Tingkat ini tidak bisa dipilih untuk pendaftaran membership.");

            if (tier.AnnualFeeAmount <= 0)
                return Fail(StatusCodes.Status409Conflict, $"Nominal iuran tingkat {tier.TierName} belum diisi. Hubungi admin master data.");

            // VAL-04 — daftar baru tidak boleh menumpuk di atas membership Aktif.
            var active = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x => x.PatientId == request.PatientId && !x.IsDelete && x.MembershipStatus == MembershipStatus.Active)
                .Select(x => new { TierName = x.MembershipTier!.TierName, x.ExpiredDate })
                .FirstOrDefaultAsync(cancellationToken);

            if (active != null)
            {
                var sampai = active.ExpiredDate.HasValue
                    ? $" sampai {active.ExpiredDate.Value.ToString("d MMM yyyy", Indonesia)}"
                    : string.Empty;
                return Fail(StatusCodes.Status409Conflict,
                    $"Pasien sudah member {active.TierName}{sampai}. Gunakan perpanjangan atau upgrade.");
            }

            // VAL-05 — paling banyak satu pendaftaran Menunggu Pembayaran.
            var pendingNumber = await _dbContext.Set<MstPatientMembership>()
                .AsNoTracking()
                .Where(x => x.PatientId == request.PatientId && !x.IsDelete && x.MembershipStatus == MembershipStatus.PendingPayment)
                .Select(x => x.MemberNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (pendingNumber != null)
                return PendingConflict(pendingNumber);

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
                PatientId = request.PatientId,
                MembershipTierId = tier.Id,
                MemberNumber = memberNumber,
                MembershipStatus = MembershipStatus.PendingPayment,
                RegistrationType = MembershipRegistrationType.New,
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
                "PatientMembership.Register",
                $"Mendaftarkan membership {membership.Id} berstatus {MembershipStatus.PendingPayment}.");

            return new PatientMembershipCommandResult(StatusCodes.Status200OK, membership.Id, "Pendaftaran membership tersimpan dan menunggu pembayaran iuran.");
        }

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
