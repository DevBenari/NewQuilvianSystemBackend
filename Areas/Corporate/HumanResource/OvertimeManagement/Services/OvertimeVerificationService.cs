using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Data;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimeVerificationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly OvertimePeriodGuardService _periodGuard;

        public OvertimeVerificationService(
            ApplicationDbContext dbContext,
            OvertimePeriodGuardService periodGuard)
        {
            _dbContext = dbContext;
            _periodGuard = periodGuard;
        }

        public async Task<OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>> StartAsync(
            Guid overtimeRealizationId,
            StartOvertimeVerificationRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized();
            }

            request ??= new StartOvertimeVerificationRequest();
            var verificationType = NormalizeVerificationType(request.VerificationType);
            if (verificationType == null)
            {
                return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Verification type tidak valid.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var realization = await LoadRealizationAsync(overtimeRealizationId, cancellationToken);
                if (realization?.OvertimeRequest == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return NotFound();
                }

                var periodGuard = await CheckPeriodAsync(realization.OvertimeRequest, cancellationToken);
                if (!periodGuard.IsWritable)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        periodGuard.Message);
                }

                var readiness = ValidateWaitingVerification(realization);
                if (readiness != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return readiness;
                }

                var current = GetCurrentVerification(realization);
                if (current != null)
                {
                    if (IsSameIdempotencyKey(current, request.IdempotencyKey))
                    {
                        await transaction.CommitAsync(cancellationToken);
                        return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Ok(
                            MapMutation(current, realization, true),
                            "Proses verifikasi dengan idempotency key yang sama sudah tersedia.");
                    }

                    if (!string.Equals(
                            current.VerificationStatus,
                            OvertimeValueConstants.VerificationStatus.Pending,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                            StatusCodes.Status409Conflict,
                            "Realization sudah memiliki hasil verifikasi dan tidak dapat dimulai ulang.");
                    }

                    if (current.VerifierUserId.HasValue && current.VerifierUserId != actorUserId)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                            StatusCodes.Status409Conflict,
                            "Realization sedang direview oleh verifier lain.");
                    }

                    current.VerifierUserId = actorUserId;
                    current.VerifierWorkforceProfileId ??= await ResolveVerifierWorkforceProfileIdAsync(
                        actorUserId,
                        cancellationToken);
                    current.VerificationType = verificationType;
                    current.Comments = AppendText(current.Comments, request.Comments, 2000);
                    current.VerificationResultJson = BuildVerificationResultJson(
                        OvertimeValueConstants.VerificationAction.Start,
                        request.IdempotencyKey,
                        realization,
                        current,
                        actorUserId,
                        null,
                        null);
                    current.UpdateDateTime = DateTime.UtcNow;
                    current.UpdateBy = actorUserId;

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Ok(
                        MapMutation(current, realization, false),
                        "Overtime verification sudah berstatus Pending dan berhasil diambil alih oleh verifier yang sama.");
                }

                var now = DateTime.UtcNow;
                var verification = new TrxOvertimeVerification
                {
                    Id = Guid.NewGuid(),
                    OvertimeRealizationId = realization.Id,
                    VerificationOrder = 1,
                    VerificationType = verificationType,
                    VerifierUserId = actorUserId,
                    VerifierWorkforceProfileId = await ResolveVerifierWorkforceProfileIdAsync(
                        actorUserId,
                        cancellationToken),
                    VerificationStatus = OvertimeValueConstants.VerificationStatus.Pending,
                    SubmittedMinutes = realization.EligibleMinutes,
                    EligibleMinutes = realization.EligibleMinutes,
                    VerifiedMinutes = 0,
                    VerifiedAmount = 0,
                    IsAttendanceMatched = HasAttendanceEvidence(realization),
                    IsPolicyCompliant = IsPolicyCompliant(realization),
                    HasVariance = realization.VarianceMinutes != 0,
                    RequiresRevision = false,
                    IsFinalVerification = false,
                    Comments = NormalizeNullable(request.Comments, 2000),
                    IsActive = true,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                verification.VerificationResultJson = BuildVerificationResultJson(
                    OvertimeValueConstants.VerificationAction.Start,
                    request.IdempotencyKey,
                    realization,
                    verification,
                    actorUserId,
                    null,
                    null);

                _dbContext.TrxOvertimeVerifications.Add(verification);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Ok(
                    MapMutation(verification, realization, false),
                    "Overtime verification berhasil dimulai.",
                    StatusCodes.Status201Created);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>> ApproveAsync(
            Guid overtimeRealizationId,
            ApproveOvertimeVerificationRequest? request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized();
            }

            request ??= new ApproveOvertimeVerificationRequest();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var realization = await LoadRealizationAsync(overtimeRealizationId, cancellationToken);
                if (realization?.OvertimeRequest == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return NotFound();
                }

                var periodGuard = await CheckPeriodAsync(realization.OvertimeRequest, cancellationToken);
                if (!periodGuard.IsWritable)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        periodGuard.Message);
                }

                var current = GetCurrentVerification(realization);
                if (current != null &&
                    string.Equals(
                        current.VerificationStatus,
                        OvertimeValueConstants.VerificationStatus.Approved,
                        StringComparison.OrdinalIgnoreCase) &&
                    IsSameIdempotencyKey(current, request.IdempotencyKey))
                {
                    await transaction.CommitAsync(cancellationToken);
                    return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Ok(
                        MapMutation(current, realization, true),
                        "Final approval dengan idempotency key yang sama sudah diproses.");
                }

                var readiness = ValidateWaitingVerification(realization);
                if (readiness != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return readiness;
                }

                var verifierResult = await EnsurePendingVerificationAsync(
                    realization,
                    actorUserId,
                    OvertimeValueConstants.VerificationType.HR,
                    cancellationToken);

                if (!verifierResult.Success || verifierResult.Data == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                        verifierResult.StatusCode,
                        verifierResult.Message);
                }

                current = verifierResult.Data;

                var adjustmentMap = new Dictionary<Guid, OvertimeVerificationDetailAdjustmentRequest>();
                foreach (var item in request.DetailAdjustments ?? new List<OvertimeVerificationDetailAdjustmentRequest>())
                {
                    if (item.RealizationDetailId == Guid.Empty)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            "Realization detail ID pada adjustment wajib diisi.");
                    }

                    if (!adjustmentMap.TryAdd(item.RealizationDetailId, item))
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            "Realization detail yang sama tidak boleh dikirim lebih dari satu kali.");
                    }
                }

                var validDetailIds = realization.Details.Select(x => x.Id).ToHashSet();
                var unknownIds = adjustmentMap.Keys.Where(x => !validDetailIds.Contains(x)).ToList();
                if (unknownIds.Count > 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Terdapat detail adjustment yang bukan bagian dari overtime realization.");
                }

                var hasAdjustment = false;
                foreach (var detail in realization.Details)
                {
                    var verifiedMinutes = adjustmentMap.TryGetValue(detail.Id, out var adjustment)
                        ? adjustment.VerifiedMinutes
                        : detail.EligibleMinutes;

                    if (verifiedMinutes < 0 || verifiedMinutes > detail.EligibleMinutes)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                            StatusCodes.Status400BadRequest,
                            $"Verified minutes detail sequence {detail.SequenceNumber} harus berada antara 0 dan eligible minutes {detail.EligibleMinutes}.");
                    }

                    if (verifiedMinutes != detail.EligibleMinutes)
                    {
                        hasAdjustment = true;
                        if (adjustment == null || string.IsNullOrWhiteSpace(adjustment.Reason))
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                                StatusCodes.Status400BadRequest,
                                $"Alasan adjustment wajib diisi untuk detail sequence {detail.SequenceNumber}.");
                        }
                    }
                }

                if (hasAdjustment && string.IsNullOrWhiteSpace(request.AdjustmentReason))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Adjustment reason header wajib diisi ketika verified minutes berbeda dari eligible minutes.");
                }

                var now = DateTime.UtcNow;
                var verifiedTotal = 0;

                foreach (var detail in realization.Details)
                {
                    var adjustment = adjustmentMap.GetValueOrDefault(detail.Id);
                    detail.VerifiedMinutes = adjustment?.VerifiedMinutes ?? detail.EligibleMinutes;
                    detail.VerifiedAmount = 0;
                    detail.DetailStatus = OvertimeValueConstants.RealizationDetailStatus.Verified;
                    detail.Notes = AppendText(
                        detail.Notes,
                        adjustment?.Reason,
                        1000);
                    detail.UpdateDateTime = now;
                    detail.UpdateBy = actorUserId;
                    verifiedTotal += detail.VerifiedMinutes;
                }

                current.VerifierUserId = actorUserId;
                current.VerifierWorkforceProfileId ??= await ResolveVerifierWorkforceProfileIdAsync(
                    actorUserId,
                    cancellationToken);
                current.VerificationStatus = OvertimeValueConstants.VerificationStatus.Approved;
                current.ActionAt = now;
                current.SubmittedMinutes = realization.EligibleMinutes;
                current.EligibleMinutes = realization.Details.Sum(x => x.EligibleMinutes);
                current.VerifiedMinutes = verifiedTotal;
                current.VerifiedAmount = 0;
                current.IsAttendanceMatched = HasAttendanceEvidence(realization);
                current.IsPolicyCompliant = IsPolicyCompliant(realization);
                current.HasVariance = realization.VarianceMinutes != 0 || verifiedTotal != current.EligibleMinutes;
                current.RequiresRevision = false;
                current.IsFinalVerification = true;
                current.Comments = AppendText(current.Comments, request.Comments, 2000);
                current.VerificationResultJson = BuildVerificationResultJson(
                    OvertimeValueConstants.VerificationAction.Approve,
                    request.IdempotencyKey,
                    realization,
                    current,
                    actorUserId,
                    request.AdjustmentReason,
                    adjustmentMap);
                current.UpdateDateTime = now;
                current.UpdateBy = actorUserId;

                realization.VerifiedMinutes = verifiedTotal;
                realization.VerifiedAmount = 0;
                realization.VerifiedAt = now;
                realization.VerifiedByUserId = actorUserId;
                realization.RealizationStatus = OvertimeValueConstants.RealizationStatus.Verified;
                realization.RealizationNotes = AppendText(
                    realization.RealizationNotes,
                    BuildActionNote("Verified", request.Comments, request.AdjustmentReason),
                    2000);
                realization.UpdateDateTime = now;
                realization.UpdateBy = actorUserId;

                realization.OvertimeRequest.OvertimeRequestStatus =
                    OvertimeValueConstants.RequestStatus.Realized;
                realization.OvertimeRequest.RealizedAt = now;
                realization.OvertimeRequest.UpdateDateTime = now;
                realization.OvertimeRequest.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Ok(
                    MapMutation(current, realization, false),
                    hasAdjustment
                        ? "Overtime realization berhasil diverifikasi dan diselesaikan dengan adjustment menit."
                        : "Overtime realization berhasil diverifikasi dan diselesaikan.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>> RequestRevisionAsync(
            Guid overtimeRealizationId,
            RequestOvertimeVerificationRevisionRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan need revision wajib diisi.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var realization = await LoadRealizationAsync(overtimeRealizationId, cancellationToken);
                if (realization?.OvertimeRequest == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return NotFound();
                }

                var periodGuard = await CheckPeriodAsync(realization.OvertimeRequest, cancellationToken);
                if (!periodGuard.IsWritable)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        periodGuard.Message);
                }

                var current = GetCurrentVerification(realization);
                if (current != null &&
                    string.Equals(
                        current.VerificationStatus,
                        OvertimeValueConstants.VerificationStatus.NeedRevision,
                        StringComparison.OrdinalIgnoreCase) &&
                    IsSameIdempotencyKey(current, request.IdempotencyKey))
                {
                    await transaction.CommitAsync(cancellationToken);
                    return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Ok(
                        MapMutation(current, realization, true),
                        "Need revision dengan idempotency key yang sama sudah diproses.");
                }

                var readiness = ValidateWaitingVerification(realization);
                if (readiness != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return readiness;
                }

                var verifierResult = await EnsurePendingVerificationAsync(
                    realization,
                    actorUserId,
                    OvertimeValueConstants.VerificationType.HR,
                    cancellationToken);

                if (!verifierResult.Success || verifierResult.Data == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                        verifierResult.StatusCode,
                        verifierResult.Message);
                }

                current = verifierResult.Data;
                var now = DateTime.UtcNow;

                current.VerificationStatus = OvertimeValueConstants.VerificationStatus.NeedRevision;
                current.ActionAt = now;
                current.SubmittedMinutes = realization.EligibleMinutes;
                current.EligibleMinutes = realization.EligibleMinutes;
                current.VerifiedMinutes = 0;
                current.VerifiedAmount = 0;
                current.IsAttendanceMatched = HasAttendanceEvidence(realization);
                current.IsPolicyCompliant = false;
                current.HasVariance = true;
                current.RequiresRevision = true;
                current.IsFinalVerification = false;
                current.Comments = AppendText(
                    current.Comments,
                    BuildActionNote("NeedRevision", request.Reason, request.Comments),
                    2000);
                current.VerificationResultJson = BuildVerificationResultJson(
                    OvertimeValueConstants.VerificationAction.NeedRevision,
                    request.IdempotencyKey,
                    realization,
                    current,
                    actorUserId,
                    request.Reason,
                    null);
                current.UpdateDateTime = now;
                current.UpdateBy = actorUserId;

                realization.RealizationStatus = OvertimeValueConstants.RealizationStatus.NeedRevision;
                realization.VerifiedMinutes = 0;
                realization.VerifiedAmount = 0;
                realization.VerifiedAt = null;
                realization.VerifiedByUserId = null;
                realization.RealizationNotes = AppendText(
                    realization.RealizationNotes,
                    BuildActionNote("NeedRevision", request.Reason, request.Comments),
                    2000);
                realization.UpdateDateTime = now;
                realization.UpdateBy = actorUserId;

                foreach (var detail in realization.Details)
                {
                    detail.VerifiedMinutes = 0;
                    detail.VerifiedAmount = 0;
                    detail.DetailStatus = OvertimeValueConstants.RealizationDetailStatus.NeedRevision;
                    detail.UpdateDateTime = now;
                    detail.UpdateBy = actorUserId;
                }

                realization.OvertimeRequest.OvertimeRequestStatus =
                    OvertimeValueConstants.RequestStatus.WaitingVerification;
                realization.OvertimeRequest.RealizedAt = null;
                realization.OvertimeRequest.UpdateDateTime = now;
                realization.OvertimeRequest.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Ok(
                    MapMutation(current, realization, false),
                    "Overtime realization dikembalikan untuk perbaikan atau recalculation.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>> RejectAsync(
            Guid overtimeRealizationId,
            RejectOvertimeVerificationRequest request,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized();
            }

            if (request.RejectionReasonId == Guid.Empty || string.IsNullOrWhiteSpace(request.Comments))
            {
                return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Rejection reason dan komentar wajib diisi.");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

            try
            {
                var realization = await LoadRealizationAsync(overtimeRealizationId, cancellationToken);
                if (realization?.OvertimeRequest == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return NotFound();
                }

                var periodGuard = await CheckPeriodAsync(realization.OvertimeRequest, cancellationToken);
                if (!periodGuard.IsWritable)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                        StatusCodes.Status409Conflict,
                        periodGuard.Message);
                }

                var current = GetCurrentVerification(realization);
                if (current != null &&
                    string.Equals(
                        current.VerificationStatus,
                        OvertimeValueConstants.VerificationStatus.Rejected,
                        StringComparison.OrdinalIgnoreCase) &&
                    IsSameIdempotencyKey(current, request.IdempotencyKey))
                {
                    await transaction.CommitAsync(cancellationToken);
                    return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Ok(
                        MapMutation(current, realization, true),
                        "Rejection dengan idempotency key yang sama sudah diproses.");
                }

                var readiness = ValidateWaitingVerification(realization);
                if (readiness != null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return readiness;
                }

                var now = DateTime.UtcNow;
                var rejectionReason = await _dbContext.MstRejectionReasons
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.RejectionReasonId &&
                        !x.IsDelete &&
                        !x.IsCancel &&
                        x.IsActive &&
                        x.RequestType == OvertimeValueConstants.Workflow.RequestType &&
                        (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= now) &&
                        (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= now),
                        cancellationToken);

                if (rejectionReason == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Rejection reason OvertimeRequest tidak ditemukan, tidak aktif, atau berada di luar periode efektif.");
                }

                var verifierResult = await EnsurePendingVerificationAsync(
                    realization,
                    actorUserId,
                    OvertimeValueConstants.VerificationType.HR,
                    cancellationToken);

                if (!verifierResult.Success || verifierResult.Data == null)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                        verifierResult.StatusCode,
                        verifierResult.Message);
                }

                current = verifierResult.Data;
                current.VerificationStatus = OvertimeValueConstants.VerificationStatus.Rejected;
                current.RejectionReasonId = rejectionReason.Id;
                current.ActionAt = now;
                current.SubmittedMinutes = realization.EligibleMinutes;
                current.EligibleMinutes = realization.EligibleMinutes;
                current.VerifiedMinutes = 0;
                current.VerifiedAmount = 0;
                current.IsAttendanceMatched = HasAttendanceEvidence(realization);
                current.IsPolicyCompliant = false;
                current.HasVariance = realization.EligibleMinutes != 0 || realization.VarianceMinutes != 0;
                current.RequiresRevision = false;
                current.IsFinalVerification = true;
                current.Comments = AppendText(current.Comments, request.Comments, 2000);
                current.VerificationResultJson = BuildVerificationResultJson(
                    OvertimeValueConstants.VerificationAction.Reject,
                    request.IdempotencyKey,
                    realization,
                    current,
                    actorUserId,
                    rejectionReason.ReasonCode + " - " + rejectionReason.ReasonName,
                    null);
                current.UpdateDateTime = now;
                current.UpdateBy = actorUserId;

                realization.RealizationStatus = OvertimeValueConstants.RealizationStatus.Rejected;
                realization.VerifiedMinutes = 0;
                realization.VerifiedAmount = 0;
                realization.VerifiedAt = now;
                realization.VerifiedByUserId = actorUserId;
                realization.RealizationNotes = AppendText(
                    realization.RealizationNotes,
                    BuildActionNote(
                        "Rejected",
                        rejectionReason.ReasonCode + " - " + rejectionReason.ReasonName,
                        request.Comments),
                    2000);
                realization.UpdateDateTime = now;
                realization.UpdateBy = actorUserId;

                foreach (var detail in realization.Details)
                {
                    detail.VerifiedMinutes = 0;
                    detail.VerifiedAmount = 0;
                    detail.DetailStatus = OvertimeValueConstants.RealizationDetailStatus.Rejected;
                    detail.UpdateDateTime = now;
                    detail.UpdateBy = actorUserId;
                }

                realization.OvertimeRequest.OvertimeRequestStatus =
                    OvertimeValueConstants.RequestStatus.Rejected;
                realization.OvertimeRequest.RejectionReasonId = rejectionReason.Id;
                realization.OvertimeRequest.RejectedAt = now;
                realization.OvertimeRequest.RejectedByUserId = actorUserId;
                realization.OvertimeRequest.RealizedAt = null;
                realization.OvertimeRequest.UpdateDateTime = now;
                realization.OvertimeRequest.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Ok(
                    MapMutation(current, realization, false),
                    "Overtime realization berhasil ditolak pada final verification.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        private Task<OvertimePeriodGuardResult> CheckPeriodAsync(
            WfpOvertimeRequest request,
            CancellationToken cancellationToken) =>
            _periodGuard.CheckDateAsync(
                request.OvertimeDate,
                null,
                request.HospitalSiteId,
                request.OrganizationUnitId,
                request.DepartmentId,
                cancellationToken);

        private async Task<TrxOvertimeRealization?> LoadRealizationAsync(
            Guid overtimeRealizationId,
            CancellationToken cancellationToken) =>
            await _dbContext.TrxOvertimeRealizations
                .Include(x => x.OvertimeRequest)
                .Include(x => x.Details.Where(d => !d.IsDelete && !d.IsCancel && d.IsActive))
                .Include(x => x.Verifications.Where(v => !v.IsDelete && !v.IsCancel && v.IsActive))
                .FirstOrDefaultAsync(x =>
                    x.Id == overtimeRealizationId &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive,
                    cancellationToken);

        private async Task<OvertimeVerificationServiceResult<TrxOvertimeVerification>> EnsurePendingVerificationAsync(
            TrxOvertimeRealization realization,
            Guid actorUserId,
            string verificationType,
            CancellationToken cancellationToken)
        {
            var current = GetCurrentVerification(realization);
            if (current != null)
            {
                if (!string.Equals(
                        current.VerificationStatus,
                        OvertimeValueConstants.VerificationStatus.Pending,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return OvertimeVerificationServiceResult<TrxOvertimeVerification>.Fail(
                        StatusCodes.Status409Conflict,
                        "Verification record sudah memiliki action final atau need revision.");
                }

                if (current.VerifierUserId.HasValue && current.VerifierUserId != actorUserId)
                {
                    return OvertimeVerificationServiceResult<TrxOvertimeVerification>.Fail(
                        StatusCodes.Status409Conflict,
                        "Realization sedang direview oleh verifier lain.");
                }

                current.VerifierUserId = actorUserId;
                current.VerifierWorkforceProfileId ??= await ResolveVerifierWorkforceProfileIdAsync(
                    actorUserId,
                    cancellationToken);
                return OvertimeVerificationServiceResult<TrxOvertimeVerification>.Ok(
                    current,
                    "Pending verification ditemukan.");
            }

            var now = DateTime.UtcNow;
            current = new TrxOvertimeVerification
            {
                Id = Guid.NewGuid(),
                OvertimeRealizationId = realization.Id,
                VerificationOrder = 1,
                VerificationType = verificationType,
                VerifierUserId = actorUserId,
                VerifierWorkforceProfileId = await ResolveVerifierWorkforceProfileIdAsync(
                    actorUserId,
                    cancellationToken),
                VerificationStatus = OvertimeValueConstants.VerificationStatus.Pending,
                SubmittedMinutes = realization.EligibleMinutes,
                EligibleMinutes = realization.EligibleMinutes,
                VerifiedMinutes = 0,
                VerifiedAmount = 0,
                IsAttendanceMatched = HasAttendanceEvidence(realization),
                IsPolicyCompliant = IsPolicyCompliant(realization),
                HasVariance = realization.VarianceMinutes != 0,
                RequiresRevision = false,
                IsFinalVerification = false,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.TrxOvertimeVerifications.Add(current);
            realization.Verifications.Add(current);

            return OvertimeVerificationServiceResult<TrxOvertimeVerification>.Ok(
                current,
                "Pending verification dibuat.",
                StatusCodes.Status201Created);
        }

        private OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>? ValidateWaitingVerification(
            TrxOvertimeRealization realization)
        {
            if (!string.Equals(
                    realization.RealizationStatus,
                    OvertimeValueConstants.RealizationStatus.WaitingVerification,
                    StringComparison.OrdinalIgnoreCase))
            {
                return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Hanya overtime realization berstatus WaitingVerification yang dapat diproses pada verification queue.");
            }

            if (realization.IsPayrollPosted ||
                string.Equals(
                    realization.RealizationStatus,
                    OvertimeValueConstants.RealizationStatus.PostedToPayroll,
                    StringComparison.OrdinalIgnoreCase))
            {
                return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Realization yang sudah diposting ke payroll tidak dapat diverifikasi ulang.");
            }

            if (realization.Details.Count == 0)
            {
                return OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                    StatusCodes.Status409Conflict,
                    "Overtime realization belum memiliki calculation detail.");
            }

            return null;
        }

        private async Task<Guid?> ResolveVerifierWorkforceProfileIdAsync(
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var email = await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == actorUserId)
                .Select(x => x.Email)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(email))
            {
                return null;
            }

            var normalizedEmail = email.Trim().ToLower();
            return await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.Email != null &&
                    x.Email.ToLower() == normalizedEmail)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static TrxOvertimeVerification? GetCurrentVerification(
            TrxOvertimeRealization realization) =>
            realization.Verifications
                .Where(x => !x.IsDelete && !x.IsCancel && x.IsActive)
                .OrderByDescending(x => x.VerificationOrder)
                .ThenByDescending(x => x.CreateDateTime)
                .FirstOrDefault();

        private static bool HasAttendanceEvidence(TrxOvertimeRealization realization) =>
            realization.ActualStartAt.HasValue &&
            realization.ActualEndAt.HasValue &&
            realization.ActualEndAt.Value > realization.ActualStartAt.Value &&
            realization.ActualMinutes > 0 &&
            !string.IsNullOrWhiteSpace(realization.EvidenceSummaryJson);

        private static bool IsPolicyCompliant(TrxOvertimeRealization realization) =>
            realization.EligibleMinutes >= 0 &&
            realization.Details.All(x =>
                x.EligibleMinutes >= 0 &&
                x.ActualEndAt > x.ActualStartAt);

        private static string? NormalizeVerificationType(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return OvertimeValueConstants.VerificationType.HR;
            }

            return OvertimeValueConstants.VerificationType.All.FirstOrDefault(x =>
                string.Equals(x, value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsSameIdempotencyKey(
            TrxOvertimeVerification verification,
            string? idempotencyKey)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey) ||
                string.IsNullOrWhiteSpace(verification.VerificationResultJson))
            {
                return false;
            }

            try
            {
                using var document = JsonDocument.Parse(verification.VerificationResultJson);
                var property = document.RootElement
                    .EnumerateObject()
                    .FirstOrDefault(x => string.Equals(
                        x.Name,
                        "IdempotencyKey",
                        StringComparison.OrdinalIgnoreCase));

                return property.Value.ValueKind == JsonValueKind.String &&
                       string.Equals(
                           property.Value.GetString(),
                           idempotencyKey.Trim(),
                           StringComparison.Ordinal);
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private static string BuildVerificationResultJson(
            string action,
            string? idempotencyKey,
            TrxOvertimeRealization realization,
            TrxOvertimeVerification verification,
            Guid actorUserId,
            string? actionReason,
            IReadOnlyDictionary<Guid, OvertimeVerificationDetailAdjustmentRequest>? adjustments) =>
            JsonSerializer.Serialize(new
            {
                SchemaVersion = "4F.1",
                Action = action,
                IdempotencyKey = NormalizeNullable(idempotencyKey, 120),
                ActionAt = DateTime.UtcNow,
                ActorUserId = actorUserId,
                Reason = NormalizeNullable(actionReason, 2000),
                Realization = new
                {
                    realization.Id,
                    realization.RealizationNumber,
                    realization.RealizationVersion,
                    realization.ActualMinutes,
                    realization.ActualBreakMinutes,
                    realization.EligibleMinutes,
                    realization.VarianceMinutes,
                    realization.RealizationStatus
                },
                Verification = new
                {
                    verification.Id,
                    verification.VerificationOrder,
                    verification.VerificationType,
                    verification.VerificationStatus,
                    verification.SubmittedMinutes,
                    verification.EligibleMinutes,
                    verification.VerifiedMinutes,
                    verification.IsAttendanceMatched,
                    verification.IsPolicyCompliant,
                    verification.HasVariance,
                    verification.RequiresRevision,
                    verification.IsFinalVerification
                },
                Details = realization.Details
                    .OrderBy(x => x.SequenceNumber)
                    .Select(x => new
                    {
                        x.Id,
                        x.SequenceNumber,
                        x.OvertimeDate,
                        x.ActualMinutes,
                        x.BreakMinutes,
                        x.EligibleMinutes,
                        VerifiedMinutes = adjustments != null && adjustments.TryGetValue(x.Id, out var adjustment)
                            ? adjustment.VerifiedMinutes
                            : x.VerifiedMinutes,
                        AdjustmentReason = adjustments != null && adjustments.TryGetValue(x.Id, out var adjustmentReasonItem)
                            ? NormalizeNullable(adjustmentReasonItem.Reason, 1000)
                            : null
                    })
            });

        private static OvertimeVerificationMutationResponse MapMutation(
            TrxOvertimeVerification verification,
            TrxOvertimeRealization realization,
            bool isIdempotent) => new()
        {
            OvertimeVerificationId = verification.Id,
            OvertimeRealizationId = realization.Id,
            RealizationNumber = realization.RealizationNumber,
            RealizationVersion = realization.RealizationVersion,
            RealizationStatus = realization.RealizationStatus,
            OvertimeRequestId = realization.OvertimeRequestId,
            RequestNumber = realization.OvertimeRequest?.RequestNumber ?? string.Empty,
            RequestStatus = realization.OvertimeRequest?.OvertimeRequestStatus ?? string.Empty,
            VerificationType = verification.VerificationType,
            VerificationStatus = verification.VerificationStatus,
            SubmittedMinutes = verification.SubmittedMinutes,
            EligibleMinutes = verification.EligibleMinutes,
            VerifiedMinutes = verification.VerifiedMinutes,
            AdjustmentMinutes = verification.VerifiedMinutes - verification.EligibleMinutes,
            HasVariance = verification.HasVariance,
            RequiresRevision = verification.RequiresRevision,
            IsFinalVerification = verification.IsFinalVerification,
            IsIdempotentResult = isIdempotent,
            ActionAt = verification.ActionAt
        };

        private static OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse> Unauthorized() =>
            OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                StatusCodes.Status401Unauthorized,
                "Identitas user login tidak valid.");

        private static OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse> NotFound() =>
            OvertimeVerificationServiceResult<OvertimeVerificationMutationResponse>.Fail(
                StatusCodes.Status404NotFound,
                "Overtime realization tidak ditemukan.");

        private static string? NormalizeNullable(string? value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var normalized = value.Trim();
            return normalized.Length <= maxLength
                ? normalized
                : normalized[..maxLength];
        }

        private static string? AppendText(
            string? existing,
            string? addition,
            int maxLength)
        {
            var normalizedAddition = NormalizeNullable(addition, maxLength);
            if (normalizedAddition == null)
            {
                return NormalizeNullable(existing, maxLength);
            }

            var combined = string.IsNullOrWhiteSpace(existing)
                ? normalizedAddition
                : existing.Trim() + Environment.NewLine + normalizedAddition;

            return combined.Length <= maxLength
                ? combined
                : combined[^maxLength..];
        }

        private static string BuildActionNote(
            string action,
            string? primary,
            string? secondary)
        {
            var parts = new List<string> { "[4F " + action + "]" };
            if (!string.IsNullOrWhiteSpace(primary))
            {
                parts.Add(primary.Trim());
            }

            if (!string.IsNullOrWhiteSpace(secondary))
            {
                parts.Add(secondary.Trim());
            }

            return string.Join(" ", parts);
        }
    }
}
