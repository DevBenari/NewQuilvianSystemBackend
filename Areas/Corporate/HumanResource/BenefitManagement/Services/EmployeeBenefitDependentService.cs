using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services
{
    /// <summary>
    /// Tanggungan benefit pegawai (BE-FIN-103; FIN-API-1.10 R.2; FIN-STATE-1.9 R.2).
    /// Draft -> Requested -> Verified / Rejected; Verified -> Ended. Hanya Verified yang membuat
    /// seseorang layak memakai penjamin internal (FIN-DEC-204). Saldo per orang ditetapkan saat
    /// Verify (FIN-DEC-207). Kode hubungan diturunkan dari WfpFamilyMember, tidak dari klien.
    ///
    /// MaximumDependents = 0 dibaca "tidak ada tanggungan diizinkan" (fail-closed), selaras dengan
    /// BenefitTypeController yang memaksa 0 bila jenis benefit tidak mengizinkan tanggungan.
    /// Tafsiran ini dicatat pada laporan task untuk dikonfirmasi pemilik HR.
    /// </summary>
    public sealed class EmployeeBenefitDependentService
    {
        private const string LogCategory = "Corporate.HumanResource.BenefitManagement.Dependent";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public EmployeeBenefitDependentService(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        public async Task<List<EmployeeBenefitDependentResponse>> GetListAsync(Guid enrollmentId, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.TrxEmployeeBenefitEnrollments.AsNoTracking()
                .AnyAsync(x => x.Id == enrollmentId && !x.IsDelete, cancellationToken);
            if (!exists) throw new KeyNotFoundException("Enrollment benefit tidak ditemukan.");

            var rows = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                .Where(x => !x.IsDelete && x.EmployeeBenefitEnrollmentId == enrollmentId)
                .OrderBy(x => x.CreateDateTime)
                .ToListAsync(cancellationToken);
            return rows.Select(Map).ToList();
        }

        /// <summary>FIN-VAL-251, 252, 253.</summary>
        public async Task<EmployeeBenefitDependentResponse> CreateAsync(
            Guid enrollmentId, CreateEmployeeBenefitDependentRequest request, Guid actorUserId, CancellationToken cancellationToken)
        {
            IDbContextTransaction? transaction = null;
            try
            {
                transaction = await BeginTransactionAsync(cancellationToken);
                await AcquireLockAsync($"HRD_BENEFIT_ENROLLMENT_{enrollmentId:N}", cancellationToken);

                var enrollment = await _dbContext.TrxEmployeeBenefitEnrollments
                    .SingleOrDefaultAsync(x => x.Id == enrollmentId && !x.IsDelete, cancellationToken)
                    ?? throw new KeyNotFoundException("Enrollment benefit tidak ditemukan.");
                RequireOpenEnrollment(enrollment);

                var member = await _dbContext.WfpFamilyMembers.AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == request.FamilyMemberId && !x.IsDelete && x.IsActive, cancellationToken);
                if (member is null || member.WorkforceProfileId != enrollment.WorkforceProfileId)
                    throw new EmployeeBenefitValidationException("Anggota keluarga tidak terdaftar pada pegawai ini.");

                var relationship = BenefitRelationshipCodes.FromFamilyRelationship(member.Relationship);
                if (!BenefitCoverageTypes.AllowsRelationship(enrollment.CoverageLevel, relationship))
                {
                    var planName = await _dbContext.MstBenefitPlans.AsNoTracking()
                        .Where(x => x.Id == enrollment.BenefitPlanId).Select(x => x.BenefitPlanName)
                        .FirstOrDefaultAsync(cancellationToken);
                    throw new EmployeeBenefitValidationException($"Jenis hubungan {relationship} tidak ditanggung oleh plan {planName}.");
                }

                var openCount = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                    .CountAsync(x => !x.IsDelete && x.EmployeeBenefitEnrollmentId == enrollment.Id
                        && EmployeeBenefitDependentStatuses.Open.Contains(x.DependentStatus), cancellationToken);
                if (openCount >= enrollment.MaximumDependents)
                    throw new EmployeeBenefitValidationException($"Jumlah tanggungan melebihi batas {enrollment.MaximumDependents} pada plan ini.");

                var duplicate = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                    .AnyAsync(x => !x.IsDelete && x.EmployeeBenefitEnrollmentId == enrollment.Id && x.FamilyMemberId == member.Id
                        && EmployeeBenefitDependentStatuses.Open.Contains(x.DependentStatus), cancellationToken);
                if (duplicate)
                    throw new EmployeeBenefitConflictException("Anggota keluarga ini sudah terdaftar sebagai tanggungan pada enrollment ini.");

                var dependent = new TrxEmployeeBenefitDependent
                {
                    EmployeeBenefitEnrollmentId = enrollment.Id,
                    WorkforceProfileId = enrollment.WorkforceProfileId,
                    FamilyMemberId = member.Id,
                    DependentName = member.FullName,
                    RelationshipType = relationship,
                    IdentityNumber = member.IdentityNumber,
                    BirthDate = member.BirthDate,
                    GenderSnapshot = member.Gender?.ToString(),
                    DependentStatus = EmployeeBenefitDependentStatuses.Draft,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.TrxEmployeeBenefitDependents.Add(dependent);
                await SaveChangesAsync("Anggota keluarga ini sudah terdaftar sebagai tanggungan pada enrollment ini.", cancellationToken);
                await CommitAsync(transaction, cancellationToken);
                await AuditAsync("Dependent.Created", dependent.Id, actorUserId, dependent.DependentStatus);
                return Map(dependent);
            }
            catch
            {
                await RollbackAsync(transaction);
                throw;
            }
            finally
            {
                if (transaction is not null) await transaction.DisposeAsync();
            }
        }

        public Task<EmployeeBenefitDependentResponse> RequestVerificationAsync(
            Guid enrollmentId, Guid id, DependentTransitionRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
            TransitionAsync(enrollmentId, id, request.RowVersion, actorUserId, "Dependent.Requested", (d, e) =>
            {
                RequireStatus(d, "diajukan untuk verifikasi", EmployeeBenefitDependentStatuses.Draft);
                RequireOpenEnrollment(e);
                d.DependentStatus = EmployeeBenefitDependentStatuses.Requested;
                d.RequestedAt = DateTime.UtcNow;
                d.RequestedByUserId = actorUserId;
            }, cancellationToken);

        /// <summary>FIN-VAL-254: hanya dari Requested; masa berlaku dan batas plafon lebih dari nol wajib.</summary>
        public Task<EmployeeBenefitDependentResponse> VerifyAsync(
            Guid enrollmentId, Guid id, VerifyEmployeeBenefitDependentRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
            TransitionAsync(enrollmentId, id, request.RowVersion, actorUserId, "Dependent.Verified", (d, e) =>
            {
                RequireStatus(d, "diverifikasi", EmployeeBenefitDependentStatuses.Requested);
                RequireOpenEnrollment(e);
                if (request.CoverageLimitAmount <= 0)
                    throw new EmployeeBenefitValidationException("Batas plafon tanggungan wajib lebih dari nol.");
                if (request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Date)
                    throw new EmployeeBenefitValidationException("Tanggal akhir berlaku tidak boleh sebelum tanggal mulai berlaku.");

                d.DependentStatus = EmployeeBenefitDependentStatuses.Verified;
                d.EffectiveStartDate = AppDateTimeHelper.OperationalDateToUtc(request.EffectiveStartDate);
                d.EffectiveEndDate = EmployeeBenefitEnrollmentService.ToUtcDate(request.EffectiveEndDate);
                d.CoverageLimitAmount = request.CoverageLimitAmount;
                d.UsedAmount = 0m;
                d.RemainingAmount = request.CoverageLimitAmount;
                d.IsEligible = true;
                d.IsCovered = true;
                d.EligibilityReason = null;
                d.VerifiedAt = DateTime.UtcNow;
                d.VerifiedByUserId = actorUserId;
                d.VerificationNotes = request.VerificationNotes;
            }, cancellationToken, recountEnrollment: true);

        public Task<EmployeeBenefitDependentResponse> RejectAsync(
            Guid enrollmentId, Guid id, RejectEmployeeBenefitDependentRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
            TransitionAsync(enrollmentId, id, request.RowVersion, actorUserId, "Dependent.Rejected", (d, _) =>
            {
                RequireStatus(d, "ditolak", EmployeeBenefitDependentStatuses.Requested);
                if (string.IsNullOrWhiteSpace(request.EligibilityReason))
                    throw new EmployeeBenefitValidationException("Alasan penolakan wajib diisi.");
                d.DependentStatus = EmployeeBenefitDependentStatuses.Rejected;
                d.IsEligible = false;
                d.IsCovered = false;
                d.EligibilityReason = request.EligibilityReason.Trim();
            }, cancellationToken);

        public Task<EmployeeBenefitDependentResponse> EndAsync(
            Guid enrollmentId, Guid id, EndEmployeeBenefitDependentRequest request, Guid actorUserId, CancellationToken cancellationToken) =>
            TransitionAsync(enrollmentId, id, request.RowVersion, actorUserId, "Dependent.Ended", (d, _) =>
            {
                RequireStatus(d, "diakhiri", EmployeeBenefitDependentStatuses.Verified);
                if (string.IsNullOrWhiteSpace(request.Reason))
                    throw new EmployeeBenefitValidationException("Alasan mengakhiri tanggungan wajib diisi.");
                var end = AppDateTimeHelper.OperationalDateToUtc(request.EffectiveEndDate);
                if (d.EffectiveStartDate.HasValue && end < d.EffectiveStartDate.Value)
                    throw new EmployeeBenefitValidationException("Tanggal akhir berlaku tidak boleh sebelum tanggal mulai berlaku.");
                d.DependentStatus = EmployeeBenefitDependentStatuses.Ended;
                d.EffectiveEndDate = end;
                d.IsCovered = false;
                d.EligibilityReason = request.Reason.Trim();
            }, cancellationToken, recountEnrollment: true);

        // ------------------------------------------------------------------------------------
        // Bantuan
        // ------------------------------------------------------------------------------------

        private async Task<EmployeeBenefitDependentResponse> TransitionAsync(
            Guid enrollmentId, Guid id, Guid rowVersion, Guid actorUserId, string auditAction,
            Action<TrxEmployeeBenefitDependent, TrxEmployeeBenefitEnrollment> apply,
            CancellationToken cancellationToken, bool recountEnrollment = false)
        {
            IDbContextTransaction? transaction = null;
            try
            {
                transaction = await BeginTransactionAsync(cancellationToken);
                await AcquireLockAsync($"HRD_BENEFIT_ENROLLMENT_{enrollmentId:N}", cancellationToken);

                var enrollment = await _dbContext.TrxEmployeeBenefitEnrollments
                    .SingleOrDefaultAsync(x => x.Id == enrollmentId && !x.IsDelete, cancellationToken)
                    ?? throw new KeyNotFoundException("Enrollment benefit tidak ditemukan.");
                var dependent = await _dbContext.TrxEmployeeBenefitDependents
                    .SingleOrDefaultAsync(x => x.Id == id && x.EmployeeBenefitEnrollmentId == enrollmentId && !x.IsDelete, cancellationToken)
                    ?? throw new KeyNotFoundException("Tanggungan benefit tidak ditemukan.");
                if (dependent.RowVersion != rowVersion)
                    throw new EmployeeBenefitConflictException("Data sudah diperbarui petugas lain. Muat ulang lalu coba lagi.");

                apply(dependent, enrollment);
                dependent.RowVersion = Guid.NewGuid();
                dependent.UpdateDateTime = DateTime.UtcNow;
                dependent.UpdateBy = actorUserId;

                if (recountEnrollment)
                {
                    var verifiedOthers = await _dbContext.TrxEmployeeBenefitDependents.AsNoTracking()
                        .CountAsync(x => !x.IsDelete && x.EmployeeBenefitEnrollmentId == enrollmentId && x.Id != dependent.Id
                            && x.DependentStatus == EmployeeBenefitDependentStatuses.Verified, cancellationToken);
                    enrollment.ActiveDependentCount = verifiedOthers
                        + (dependent.DependentStatus == EmployeeBenefitDependentStatuses.Verified ? 1 : 0);
                    enrollment.RowVersion = Guid.NewGuid();
                    enrollment.UpdateDateTime = DateTime.UtcNow;
                    enrollment.UpdateBy = actorUserId;
                }

                await SaveChangesAsync("Data sudah diperbarui petugas lain. Muat ulang lalu coba lagi.", cancellationToken);
                await CommitAsync(transaction, cancellationToken);
                await AuditAsync(auditAction, dependent.Id, actorUserId, dependent.DependentStatus);
                return Map(dependent);
            }
            catch
            {
                await RollbackAsync(transaction);
                throw;
            }
            finally
            {
                if (transaction is not null) await transaction.DisposeAsync();
            }
        }

        public static EmployeeBenefitDependentResponse Map(TrxEmployeeBenefitDependent d) => new()
        {
            Id = d.Id,
            EmployeeBenefitEnrollmentId = d.EmployeeBenefitEnrollmentId,
            FamilyMemberId = d.FamilyMemberId,
            FullName = d.DependentName,
            RelationshipCode = d.RelationshipType,
            DependentStatus = d.DependentStatus,
            IsEligible = d.IsEligible,
            IsCovered = d.IsCovered,
            EffectiveStartDate = d.EffectiveStartDate,
            EffectiveEndDate = d.EffectiveEndDate,
            LimitAmount = d.CoverageLimitAmount,
            UsedAmount = d.UsedAmount,
            RemainingAmount = d.RemainingAmount,
            EligibilityReason = d.EligibilityReason,
            RequestedAt = d.RequestedAt,
            VerifiedAt = d.VerifiedAt,
            VerificationNotes = d.VerificationNotes,
            RowVersion = d.RowVersion,
            AvailableActions = d.DependentStatus switch
            {
                EmployeeBenefitDependentStatuses.Draft => new List<string> { EmployeeBenefitActions.Request },
                EmployeeBenefitDependentStatuses.Requested => new List<string> { EmployeeBenefitActions.Verify, EmployeeBenefitActions.Reject },
                EmployeeBenefitDependentStatuses.Verified => new List<string> { EmployeeBenefitActions.End },
                _ => new List<string>()
            }
        };

        private static void RequireStatus(TrxEmployeeBenefitDependent d, string action, params string[] allowed)
        {
            if (!allowed.Contains(d.DependentStatus))
                throw new EmployeeBenefitTransitionException($"Tanggungan berstatus {d.DependentStatus} sehingga tidak dapat {action}.");
        }

        private static void RequireOpenEnrollment(TrxEmployeeBenefitEnrollment e)
        {
            if (EmployeeBenefitEnrollmentStatuses.Terminal.Contains(e.EnrollmentStatus))
                throw new EmployeeBenefitTransitionException($"Enrollment berstatus {e.EnrollmentStatus} sehingga tanggungannya tidak dapat diubah.");
        }

        private async Task SaveChangesAsync(string conflictMessage, CancellationToken cancellationToken)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
            {
                throw new EmployeeBenefitConflictException(conflictMessage, exception);
            }
        }

        private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
        {
            if (!_dbContext.Database.IsRelational()) return null;
            return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        }

        private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
            _dbContext.Database.IsRelational()
                ? _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
                : Task.CompletedTask;

        private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
            transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

        private static Task RollbackAsync(IDbContextTransaction? transaction) =>
            transaction is null ? Task.CompletedTask : transaction.RollbackAsync(CancellationToken.None);

        // QBE-LOG-001: aktor dicatat; payload tanpa nama, NIK, atau tanggal lahir (kolom sensitif).
        private Task AuditAsync(string action, Guid dependentId, Guid actorUserId, string status) =>
            _loggerService.AuditAsync(LogCategory, action, $"{action} DependentId={dependentId}",
                new { DependentId = dependentId, Status = status, ActorUserId = actorUserId });
    }
}
