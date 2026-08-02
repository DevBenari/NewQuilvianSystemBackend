using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services
{
    public class LeaveAccrualPolicyResolution
    {
        public bool Success { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public MstLeavePolicy? LeavePolicy { get; set; }
        public MstLeaveEntitlementPolicy? EntitlementPolicy { get; set; }
    }

    public class LeaveAccrualPolicyContext
    {
        public MstWorkforceProfile WorkforceProfile { get; set; } = null!;
        public MstEmployee Employee { get; set; } = null!;
        public WfpOrganizationAssignment? OrganizationAssignment { get; set; }
        public Guid LeaveTypeId { get; set; }
        public DateOnly EvaluationDate { get; set; }
        public Guid? RequestedEntitlementPolicyId { get; set; }
    }

    public class LeaveAccrualPolicyResolverService
    {
        private readonly ApplicationDbContext _dbContext;

        public LeaveAccrualPolicyResolverService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<MstLeavePolicy>> LoadCandidatePoliciesAsync(
            Guid leaveTypeId,
            DateOnly evaluationDate,
            CancellationToken cancellationToken = default)
        {
            var startOfDay = evaluationDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var endOfDay = evaluationDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

            return await _dbContext.Set<MstLeavePolicy>()
                .AsNoTracking()
                .Include(x => x.LeaveType)
                .Include(x => x.EntitlementPolicies)
                .Where(x =>
                    x.LeaveTypeId == leaveTypeId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= endOfDay) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= startOfDay))
                .ToListAsync(cancellationToken);
        }

        public LeaveAccrualPolicyResolution Resolve(
            LeaveAccrualPolicyContext context,
            IReadOnlyCollection<MstLeavePolicy> candidatePolicies)
        {
            var employee = context.Employee;
            var assignment = context.OrganizationAssignment;
            var evaluationDateTime = context.EvaluationDate
                .ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

            var rankedPolicies = candidatePolicies
                .Where(policy => IsPolicyMatch(policy, employee, assignment))
                .Select(policy => new
                {
                    Policy = policy,
                    Score = CalculateSpecificityScore(policy)
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Policy.Priority)
                .ThenByDescending(x => x.Policy.IsDefault)
                .ThenBy(x => x.Policy.LeavePolicyCode)
                .ToList();

            if (rankedPolicies.Count == 0)
            {
                return new LeaveAccrualPolicyResolution
                {
                    Success = false,
                    Code = "LEAVE_POLICY_NOT_FOUND",
                    Message = "Leave policy yang sesuai dengan profil dan scope organisasi tidak ditemukan."
                };
            }

            var leavePolicy = rankedPolicies[0].Policy;
            var entitlementPolicies = leavePolicy.EntitlementPolicies
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value <= evaluationDateTime) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value >= evaluationDateTime))
                .ToList();

            MstLeaveEntitlementPolicy? entitlementPolicy;

            if (context.RequestedEntitlementPolicyId.HasValue)
            {
                entitlementPolicy = entitlementPolicies.FirstOrDefault(
                    x => x.Id == context.RequestedEntitlementPolicyId.Value);

                if (entitlementPolicy == null)
                {
                    return new LeaveAccrualPolicyResolution
                    {
                        Success = false,
                        Code = "ENTITLEMENT_POLICY_NOT_APPLICABLE",
                        Message = "Entitlement policy yang dipilih tidak aktif atau tidak berasal dari leave policy yang sesuai."
                    };
                }
            }
            else
            {
                entitlementPolicy = entitlementPolicies
                    .OrderByDescending(x => x.IsDefault)
                    .ThenBy(x => x.EntitlementPolicyCode)
                    .FirstOrDefault();
            }

            if (entitlementPolicy == null)
            {
                return new LeaveAccrualPolicyResolution
                {
                    Success = false,
                    Code = "ENTITLEMENT_POLICY_NOT_FOUND",
                    Message = "Entitlement policy aktif tidak ditemukan pada leave policy terpilih."
                };
            }

            var serviceMonths = GetCompletedServiceMonths(
                employee.JoinDate,
                context.EvaluationDate.ToDateTime(TimeOnly.MinValue));

            var minimumServiceMonths = Math.Max(
                leavePolicy.MinimumServiceMonths,
                entitlementPolicy.MinimumServiceMonths);

            if (serviceMonths < minimumServiceMonths)
            {
                return new LeaveAccrualPolicyResolution
                {
                    Success = false,
                    Code = "MINIMUM_SERVICE_NOT_MET",
                    Message = $"Masa kerja belum memenuhi minimum {minimumServiceMonths} bulan.",
                    LeavePolicy = leavePolicy,
                    EntitlementPolicy = entitlementPolicy
                };
            }

            return new LeaveAccrualPolicyResolution
            {
                Success = true,
                Code = "RESOLVED",
                Message = "Leave policy dan entitlement policy berhasil di-resolve.",
                LeavePolicy = leavePolicy,
                EntitlementPolicy = entitlementPolicy
            };
        }

        private static bool IsPolicyMatch(
            MstLeavePolicy policy,
            MstEmployee employee,
            WfpOrganizationAssignment? assignment)
        {
            if (policy.LegalEntityId.HasValue &&
                policy.LegalEntityId != assignment?.LegalEntityId)
            {
                return false;
            }

            if (policy.HospitalSiteId.HasValue &&
                policy.HospitalSiteId != assignment?.HospitalSiteId)
            {
                return false;
            }

            if (policy.OrganizationUnitId.HasValue &&
                policy.OrganizationUnitId != assignment?.OrganizationUnitId)
            {
                return false;
            }

            if (policy.DepartmentId.HasValue &&
                policy.DepartmentId != assignment?.DepartmentId)
            {
                return false;
            }

            if (policy.PositionId.HasValue &&
                policy.PositionId != assignment?.PositionId)
            {
                return false;
            }

            if (policy.WorkLocationId.HasValue &&
                policy.WorkLocationId != assignment?.WorkLocationId)
            {
                return false;
            }

            if (policy.WorkforceTypeId.HasValue &&
                policy.WorkforceTypeId != employee.WorkforceTypeId)
            {
                return false;
            }

            if (policy.EmployeeCategoryId.HasValue &&
                policy.EmployeeCategoryId != employee.EmployeeCategoryId)
            {
                return false;
            }

            if (policy.EmploymentTypeId.HasValue &&
                policy.EmploymentTypeId != employee.EmploymentTypeId)
            {
                return false;
            }

            if (policy.EmploymentStatusId.HasValue &&
                policy.EmploymentStatusId != employee.EmploymentStatusId)
            {
                return false;
            }

            if (policy.ContractTypeId.HasValue &&
                policy.ContractTypeId != employee.ContractTypeId)
            {
                return false;
            }

            return true;
        }

        private static int CalculateSpecificityScore(MstLeavePolicy policy)
        {
            var score = 0;
            score += policy.LegalEntityId.HasValue ? 10 : 0;
            score += policy.HospitalSiteId.HasValue ? 20 : 0;
            score += policy.OrganizationUnitId.HasValue ? 30 : 0;
            score += policy.DepartmentId.HasValue ? 40 : 0;
            score += policy.PositionId.HasValue ? 50 : 0;
            score += policy.WorkLocationId.HasValue ? 15 : 0;
            score += policy.WorkforceTypeId.HasValue ? 10 : 0;
            score += policy.EmployeeCategoryId.HasValue ? 10 : 0;
            score += policy.EmploymentTypeId.HasValue ? 10 : 0;
            score += policy.EmploymentStatusId.HasValue ? 10 : 0;
            score += policy.ContractTypeId.HasValue ? 10 : 0;
            score += policy.IsFallback ? -1000 : 0;
            return score;
        }

        public static int GetCompletedServiceMonths(DateTime joinDate, DateTime evaluationDate)
        {
            if (evaluationDate.Date < joinDate.Date)
            {
                return 0;
            }

            var months = (evaluationDate.Year - joinDate.Year) * 12 +
                         evaluationDate.Month - joinDate.Month;

            if (evaluationDate.Day < joinDate.Day)
            {
                months--;
            }

            return Math.Max(0, months);
        }
    }
}
