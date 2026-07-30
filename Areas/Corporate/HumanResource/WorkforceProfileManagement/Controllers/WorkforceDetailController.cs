using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceProfileManagement.DTOs;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceProfileManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/overview")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_PROFILE_MANAGEMENT",
        moduleName: "Human Resource Workforce Profile Management",
        displayName: "Workforce Profile Overview",
        AreaName = "Corporate",
        ControllerName = "WorkforceProfileOverview",
        Description = "Aggregated workforce profile overview, section availability, and summary counts",
        SortOrder = 1)]
    [Tags("Corporate / Human Resource / Workforce Profile Management / Workforce Profile Overview")]
    public class WorkforceDetailController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public WorkforceDetailController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [AccessAction(
            "Read",
            "Read Workforce Profile Overview",
            Description = "Melihat overview, context section, dan summary count profil workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("WorkforceProfileOverview", "Read")]
        public async Task<IActionResult> GetWorkforceOverview(Guid workforceProfileId)
        {
            if (workforceProfileId == Guid.Empty)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "WorkforceProfileId tidak valid."));
            }

            var workforceProfile = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == workforceProfileId &&
                    !x.IsDelete);

            if (workforceProfile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."));
            }

            var employee = await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.EmployeeCode,
                    x.EmployeeNumber,
                    x.IsActive
                })
                .FirstOrDefaultAsync();

            var doctor = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.DoctorCode,
                    x.DoctorNumber,
                    x.IsActive,
                    x.ProfessionId,
                    x.SpecializationId
                })
                .FirstOrDefaultAsync();

            var externalUser = await _dbContext.Set<MstExternalUser>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.ExternalCode,
                    x.IsActive
                })
                .FirstOrDefaultAsync();

            var now = DateTime.UtcNow;

            var summary = await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new WorkforceDetailSummaryResponse
                {
                    ProfileSummaryCount = 1,

                    AddressCount = _dbContext.Set<WfpAddress>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    BankAccountCount = _dbContext.Set<WfpBankAccount>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    EducationCount = _dbContext.Set<WfpEducation>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    DocumentCount = _dbContext.Set<WfpDocument>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    EmergencyContactCount = _dbContext.Set<WfpEmergencyContact>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    FamilyMemberCount = _dbContext.Set<WfpFamilyMember>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    DependentCount = _dbContext.Set<WfpDependent>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    ContractHistoryCount = _dbContext.Set<WfpContractHistory>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    EmploymentHistoryCount = _dbContext.Set<WfpEmploymentHistory>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    OrganizationAssignmentCount = _dbContext.Set<WfpOrganizationAssignment>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    PositionAssignmentCount = _dbContext.Set<WfpPositionAssignment>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    ManagerAssignmentCount = _dbContext.Set<WfpManagerAssignment>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    SalaryAssignmentCount = _dbContext.Set<WfpSalaryAssignment>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    WorkScheduleAssignmentCount = _dbContext.Set<WfpWorkScheduleAssignment>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    TrainingRecordCount = _dbContext.Set<WfpTrainingRecord>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    CompetencyAssessmentCount = _dbContext.Set<WfpCompetencyAssessment>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    VerifiedCompetencyAssessmentCount = _dbContext.Set<WfpCompetencyAssessment>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete &&
                            y.IsVerified),

                    ExpiredCompetencyAssessmentCount = _dbContext.Set<WfpCompetencyAssessment>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete &&
                            y.ExpiredDate.HasValue &&
                            y.ExpiredDate.Value < now),

                    CertificationCount = _dbContext.Set<WfpCertification>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    CredentialLicenseCount = _dbContext.Set<WfpCredentialLicense>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    ClinicalPrivilegeCount = _dbContext.Set<WfpClinicalPrivilege>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    HealthRecordCount = _dbContext.Set<WfpHealthRecord>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    PerformanceReviewCount = _dbContext.Set<WfpPerformanceReview>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    DisciplinaryActionCount = _dbContext.Set<WfpDisciplinaryAction>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    HasPayroll = _dbContext.Set<WfpPayroll>()
                        .Any(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    HasTax = _dbContext.Set<WfpTax>()
                        .Any(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    HasInsurance = _dbContext.Set<WfpInsurance>()
                        .Any(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    HasTransportAllowance = _dbContext.Set<WfpTransportAllowance>()
                        .Any(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    ComplianceAlertCount = _dbContext.Set<WfpComplianceAlert>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete),

                    OpenComplianceAlertCount = _dbContext.Set<WfpComplianceAlert>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete &&
                            !y.IsResolved &&
                            (y.AlertStatus == ComplianceAlertStatus.Open ||
                             y.AlertStatus == ComplianceAlertStatus.InProgress)),

                    CriticalComplianceAlertCount = _dbContext.Set<WfpComplianceAlert>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete &&
                            !y.IsResolved &&
                            y.SeverityLevel == ComplianceAlertSeverityLevel.Critical),

                    OverdueComplianceAlertCount = _dbContext.Set<WfpComplianceAlert>()
                        .Count(y =>
                            y.WorkforceProfileId == workforceProfileId &&
                            !y.IsDelete &&
                            !y.IsResolved &&
                            y.DueDate < now)
                })
                .FirstAsync();

            var userType = workforceProfile.UserType.ToString();
            var isEmployee = employee != null ||
                userType.Equals("Employee", StringComparison.OrdinalIgnoreCase);
            var isDoctor = doctor != null ||
                userType.Equals("Doctor", StringComparison.OrdinalIgnoreCase);
            var isExternalUser = externalUser != null ||
                userType.Equals("ExternalUser", StringComparison.OrdinalIgnoreCase) ||
                userType.Equals("External", StringComparison.OrdinalIgnoreCase);

            var hasActiveOrganizationAssignment = await _dbContext
                .Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.EffectiveStartDate <= now &&
                    (!x.EffectiveEndDate.HasValue ||
                     x.EffectiveEndDate.Value >= now));

            var hasActiveWorkSchedule = await _dbContext
                .Set<WfpWorkScheduleAssignment>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete);

            var isPayrollEligible = await _dbContext.Set<WfpPayroll>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsPayrollEligible &&
                    x.IsActive &&
                    !x.IsDelete);

            var availableSections = BuildAvailableSections(
                isEmployee,
                isDoctor,
                isExternalUser,
                summary);

            var restrictedSections = BuildRestrictedSections(
                availableSections);

            var result = new WorkforceDetailResponse
            {
                Profile = new WorkforceDetailProfileResponse
                {
                    WorkforceProfileId = workforceProfile.Id,
                    ProfileCode = workforceProfile.ProfileCode,
                    DisplayName = workforceProfile.DisplayName,
                    UserType = userType,
                    Email = workforceProfile.Email,
                    PhoneNumber = workforceProfile.PhoneNumber,
                    WhatsAppNumber = workforceProfile.WhatsAppNumber,
                    PrimaryDepartmentId = workforceProfile.PrimaryDepartmentId,
                    PrimaryPositionId = workforceProfile.PrimaryPositionId,
                    IsActive = workforceProfile.IsActive,

                    EmployeeId = employee?.Id,
                    EmployeeCode = employee?.EmployeeCode,
                    EmployeeNumber = employee?.EmployeeNumber,

                    DoctorId = doctor?.Id,
                    DoctorCode = doctor?.DoctorCode,
                    DoctorNumber = doctor?.DoctorNumber,

                    ExternalUserId = externalUser?.Id,
                    ExternalCode = externalUser?.ExternalCode
                },
                Context = new WorkforceDetailContextResponse
                {
                    IsEmployee = isEmployee,
                    IsDoctor = isDoctor,
                    IsExternalUser = isExternalUser,
                    IsClinicalWorkforce = isDoctor ||
                        summary.ClinicalPrivilegeCount > 0 ||
                        summary.CredentialLicenseCount > 0,
                    IsPayrollEligible = isPayrollEligible,
                    HasActiveOrganizationAssignment = hasActiveOrganizationAssignment,
                    HasActiveWorkSchedule = hasActiveWorkSchedule,
                    AvailableSections = availableSections,
                    RestrictedSections = restrictedSections
                },
                Summary = summary
            };

            return Ok(ApiResponse<WorkforceDetailResponse>.Ok(
                result,
                "Workforce profile overview berhasil diambil."));
        }

        private static List<string> BuildAvailableSections(
            bool isEmployee,
            bool isDoctor,
            bool isExternalUser,
            WorkforceDetailSummaryResponse summary)
        {
            var sections = new List<string>
            {
                "ProfileSummary",
                "Address",
                "BankAccount",
                "Education",
                "Document",
                "EmergencyContact",
                "ContractHistory",
                "EmploymentHistory",
                "OrganizationAssignment",
                "PositionAssignment",
                "ManagerAssignment",
                "WorkSchedule",
                "TrainingRecord",
                "CompetencyAssessment",
                "Certification",
                "CredentialLicense",
                "PerformanceReview",
                "ComplianceAlert"
            };

            if (isEmployee || isDoctor)
            {
                sections.Add("FamilyMember");
                sections.Add("Dependent");
                sections.Add("HealthRecord");
                sections.Add("DisciplinaryAction");
            }

            if (isDoctor || summary.ClinicalPrivilegeCount > 0)
                sections.Add("ClinicalPrivilege");

            if (summary.SalaryAssignmentCount > 0 ||
                summary.HasPayroll ||
                isEmployee ||
                isDoctor)
            {
                sections.Add("SalaryAssignment");
                sections.Add("Payroll");
                sections.Add("Tax");
                sections.Add("Insurance");
                sections.Add("TransportAllowance");
            }

            if (isExternalUser)
            {
                RemoveWhenEmpty(sections, "BankAccount", summary.BankAccountCount);
                RemoveWhenEmpty(sections, "Education", summary.EducationCount);
                RemoveWhenEmpty(sections, "EmergencyContact", summary.EmergencyContactCount);
                RemoveWhenEmpty(sections, "EmploymentHistory", summary.EmploymentHistoryCount);
                RemoveWhenEmpty(sections, "ManagerAssignment", summary.ManagerAssignmentCount);
                RemoveWhenEmpty(sections, "TrainingRecord", summary.TrainingRecordCount);
                RemoveWhenEmpty(sections, "CompetencyAssessment", summary.CompetencyAssessmentCount);
                RemoveWhenEmpty(sections, "Certification", summary.CertificationCount);
                RemoveWhenEmpty(sections, "CredentialLicense", summary.CredentialLicenseCount);
                RemoveWhenEmpty(sections, "PerformanceReview", summary.PerformanceReviewCount);
            }

            return sections
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<string> BuildRestrictedSections(
            IReadOnlyCollection<string> availableSections)
        {
            var confidentialSections = new[]
            {
                "BankAccount",
                "SalaryAssignment",
                "Payroll",
                "Tax",
                "Insurance",
                "HealthRecord",
                "DisciplinaryAction"
            };

            return confidentialSections
                .Where(x => availableSections.Contains(
                    x,
                    StringComparer.OrdinalIgnoreCase))
                .ToList();
        }

        private static void RemoveWhenEmpty(
            ICollection<string> sections,
            string section,
            int count)
        {
            if (count > 0)
                return;

            var item = sections.FirstOrDefault(x =>
                x.Equals(section, StringComparison.OrdinalIgnoreCase));

            if (item != null)
                sections.Remove(item);
        }
    }
}
