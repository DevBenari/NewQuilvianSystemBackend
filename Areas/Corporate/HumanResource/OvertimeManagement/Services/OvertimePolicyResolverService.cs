using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    public class OvertimePolicyResolverService
    {
        private readonly ApplicationDbContext _dbContext;

        public OvertimePolicyResolverService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OvertimePolicyResolutionResponse> ResolveAsync(
            OvertimePolicyResolveRequest request,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await BuildContextAsync(request, cancellationToken);
            if (!contextResult.IsValid)
            {
                return new OvertimePolicyResolutionResponse
                {
                    IsResolved = false,
                    IsAmbiguous = false,
                    Message = contextResult.ErrorMessage ?? "Konteks resolusi overtime policy tidak valid.",
                    Context = contextResult.Context
                };
            }

            var context = contextResult.Context;
            var effectiveDate = context.EffectiveDate.Date;

            var entities = await _dbContext.MstOvertimePolicies
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= effectiveDate) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= effectiveDate) &&
                    (!x.LegalEntityId.HasValue || x.LegalEntityId == context.LegalEntityId) &&
                    (!x.HospitalSiteId.HasValue || x.HospitalSiteId == context.HospitalSiteId) &&
                    (!x.OrganizationUnitId.HasValue || x.OrganizationUnitId == context.OrganizationUnitId) &&
                    (!x.EmployeeCategoryId.HasValue || x.EmployeeCategoryId == context.EmployeeCategoryId) &&
                    (!x.EmploymentTypeId.HasValue || x.EmploymentTypeId == context.EmploymentTypeId))
                .ToListAsync(cancellationToken);

            if (entities.Count == 0)
            {
                return new OvertimePolicyResolutionResponse
                {
                    IsResolved = false,
                    IsAmbiguous = false,
                    Message = "Tidak ada overtime policy aktif yang sesuai dengan konteks workforce dan tanggal efektif.",
                    Context = context
                };
            }

            var specific = entities.Where(x => !x.IsFallback).ToList();
            var selectedPool = specific.Count > 0
                ? specific
                : entities.Where(x => x.IsFallback).ToList();

            if (selectedPool.Count == 0)
            {
                return new OvertimePolicyResolutionResponse
                {
                    IsResolved = false,
                    IsAmbiguous = false,
                    Message = "Tidak ada overtime policy spesifik maupun fallback yang sesuai.",
                    Context = context
                };
            }

            var ordered = selectedPool
                .Select(MapCandidate)
                .OrderByDescending(x => x.SpecificityScore)
                .ThenByDescending(x => x.Priority)
                .ThenByDescending(x => x.IsDefault)
                .ThenByDescending(x => x.EffectiveStartDate ?? DateTime.MinValue)
                .ThenBy(x => x.OvertimePolicyCode)
                .ToList();

            var selected = ordered[0];
            var isAmbiguous = ordered.Skip(1).Any(x => HasSameRank(selected, x));

            return new OvertimePolicyResolutionResponse
            {
                IsResolved = true,
                IsAmbiguous = isAmbiguous,
                ResolutionSource = selected.IsFallback
                    ? OvertimeValueConstants.ResolutionSource.Fallback
                    : OvertimeValueConstants.ResolutionSource.Specific,
                Message = isAmbiguous
                    ? "Overtime policy berhasil dipilih secara deterministik, tetapi terdapat kandidat lain dengan ranking yang sama. Perbaiki priority atau periode efektif sebelum policy digunakan untuk transaksi."
                    : "Overtime policy berhasil di-resolve.",
                Context = context,
                SelectedPolicy = selected,
                Candidates = ordered.Take(20).ToList()
            };
        }

        public async Task<OvertimePolicyOverlapResponse> CheckAmbiguousOverlapAsync(
            Guid? excludeId,
            OvertimePolicyDefinitionInput input,
            CancellationToken cancellationToken = default)
        {
            if (!input.IsActive)
            {
                return new OvertimePolicyOverlapResponse();
            }

            var legalEntityId = NormalizeGuid(input.LegalEntityId);
            var hospitalSiteId = NormalizeGuid(input.HospitalSiteId);
            var organizationUnitId = NormalizeGuid(input.OrganizationUnitId);
            var employeeCategoryId = NormalizeGuid(input.EmployeeCategoryId);
            var employmentTypeId = NormalizeGuid(input.EmploymentTypeId);

            var query = _dbContext.MstOvertimePolicies
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.IsActive &&
                    x.Priority == input.Priority &&
                    x.IsFallback == input.IsFallback &&
                    x.LegalEntityId == legalEntityId &&
                    x.HospitalSiteId == hospitalSiteId &&
                    x.OrganizationUnitId == organizationUnitId &&
                    x.EmployeeCategoryId == employeeCategoryId &&
                    x.EmploymentTypeId == employmentTypeId);

            if (excludeId.HasValue && excludeId.Value != Guid.Empty)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            var candidates = await query.ToListAsync(cancellationToken);
            var conflict = candidates.FirstOrDefault(x => DateRangesOverlap(
                x.EffectiveStartDate,
                x.EffectiveEndDate,
                input.EffectiveStartDate,
                input.EffectiveEndDate));

            return conflict == null
                ? new OvertimePolicyOverlapResponse()
                : new OvertimePolicyOverlapResponse
                {
                    HasAmbiguousOverlap = true,
                    ConflictingPolicyId = conflict.Id,
                    ConflictingPolicyCode = conflict.OvertimePolicyCode,
                    ConflictingPolicyName = conflict.OvertimePolicyName
                };
        }

        private async Task<ContextBuildResult> BuildContextAsync(
            OvertimePolicyResolveRequest request,
            CancellationToken cancellationToken)
        {
            var effectiveDate = (request.EffectiveDate ?? DateTime.UtcNow).Date;
            var context = new OvertimePolicyResolutionContextResponse
            {
                WorkforceProfileId = NormalizeGuid(request.WorkforceProfileId),
                LegalEntityId = NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId),
                EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId),
                EffectiveDate = effectiveDate
            };

            if (!context.WorkforceProfileId.HasValue)
            {
                return ContextBuildResult.Success(context);
            }

            var profileExists = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == context.WorkforceProfileId.Value &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel,
                    cancellationToken);

            if (!profileExists)
            {
                return ContextBuildResult.Fail(
                    context,
                    "Workforce profile tidak ditemukan atau tidak aktif.");
            }

            var employee = await _dbContext.MstEmployees
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == context.WorkforceProfileId.Value &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel)
                .Select(x => new
                {
                    x.EmployeeCategoryId,
                    x.EmploymentTypeId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (employee != null)
            {
                context.EmployeeCategoryId = employee.EmployeeCategoryId;
                context.EmploymentTypeId = employee.EmploymentTypeId;
            }

            var assignment = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == context.WorkforceProfileId.Value &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.EffectiveStartDate.Date <= effectiveDate &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= effectiveDate))
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .Select(x => new
                {
                    x.LegalEntityId,
                    x.HospitalSiteId,
                    x.OrganizationUnitId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (assignment != null)
            {
                context.LegalEntityId = assignment.LegalEntityId;
                context.HospitalSiteId = assignment.HospitalSiteId;
                context.OrganizationUnitId = assignment.OrganizationUnitId;
            }

            return ContextBuildResult.Success(context);
        }

        private static OvertimePolicyResolutionCandidateResponse MapCandidate(
            MstOvertimePolicy entity) => new()
        {
            Id = entity.Id,
            OvertimePolicyCode = entity.OvertimePolicyCode,
            OvertimePolicyName = entity.OvertimePolicyName,
            Priority = entity.Priority,
            IsFallback = entity.IsFallback,
            IsDefault = entity.IsDefault,
            SpecificityScore = GetSpecificityScore(entity),
            EffectiveStartDate = entity.EffectiveStartDate,
            EffectiveEndDate = entity.EffectiveEndDate
        };

        private static int GetSpecificityScore(MstOvertimePolicy entity)
        {
            var score = 0;
            if (entity.LegalEntityId.HasValue) score++;
            if (entity.HospitalSiteId.HasValue) score++;
            if (entity.OrganizationUnitId.HasValue) score++;
            if (entity.EmployeeCategoryId.HasValue) score++;
            if (entity.EmploymentTypeId.HasValue) score++;
            return score;
        }

        private static bool HasSameRank(
            OvertimePolicyResolutionCandidateResponse left,
            OvertimePolicyResolutionCandidateResponse right) =>
            left.SpecificityScore == right.SpecificityScore &&
            left.Priority == right.Priority &&
            left.IsDefault == right.IsDefault &&
            (left.EffectiveStartDate ?? DateTime.MinValue).Date ==
            (right.EffectiveStartDate ?? DateTime.MinValue).Date;

        private static bool DateRangesOverlap(
            DateTime? leftStart,
            DateTime? leftEnd,
            DateTime? rightStart,
            DateTime? rightEnd)
        {
            var leftStartValue = leftStart?.Date ?? DateTime.MinValue.Date;
            var leftEndValue = leftEnd?.Date ?? DateTime.MaxValue.Date;
            var rightStartValue = rightStart?.Date ?? DateTime.MinValue.Date;
            var rightEndValue = rightEnd?.Date ?? DateTime.MaxValue.Date;

            return leftStartValue <= rightEndValue && rightStartValue <= leftEndValue;
        }

        private static Guid? NormalizeGuid(Guid? value) =>
            !value.HasValue || value.Value == Guid.Empty ? null : value.Value;

        private sealed class ContextBuildResult
        {
            public bool IsValid { get; private set; }
            public string? ErrorMessage { get; private set; }
            public OvertimePolicyResolutionContextResponse Context { get; private set; } = new();

            public static ContextBuildResult Success(
                OvertimePolicyResolutionContextResponse context) => new()
            {
                IsValid = true,
                Context = context
            };

            public static ContextBuildResult Fail(
                OvertimePolicyResolutionContextResponse context,
                string errorMessage) => new()
            {
                IsValid = false,
                ErrorMessage = errorMessage,
                Context = context
            };
        }
    }
}
