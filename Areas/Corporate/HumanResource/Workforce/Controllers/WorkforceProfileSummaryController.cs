using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profile-summary")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Profile Summary",
        AreaName = "Corporate",
        ControllerName = "WorkforceProfileSummary",
        Description = "Corporate human resource workforce profile summary",
        SortOrder = 1
    )]
    [Tags("Corporate / Human Resource / Workforce / Profile Summary")]
    public class WorkforceProfileSummaryController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.ProfileSummary";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceProfileSummaryController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("{workforceProfileId:guid}/summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceProfileSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Profile Summary",
            Description = "Melihat ringkasan workforce profile summary",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceProfileSummary", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;

            var profile = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => x.Id == workforceProfileId && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.ProfileCode,
                    x.DisplayName,
                    x.UserType,
                    x.Email,
                    x.PhoneNumber,
                    x.WhatsAppNumber,
                    x.PrimaryDepartmentId,
                    PrimaryDepartmentCode = x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentCode : null,
                    PrimaryDepartmentName = x.PrimaryDepartment != null ? x.PrimaryDepartment.DepartmentName : null,
                    x.PrimaryPositionId,
                    PrimaryPositionCode = x.PrimaryPosition != null ? x.PrimaryPosition.PositionCode : null,
                    PrimaryPositionName = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : null,
                    x.IsActive
                })
                .FirstOrDefaultAsync();

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var result = new WorkforceProfileSummaryResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                UserType = profile.UserType,
                UserTypeName = profile.UserType.ToString(),
                Email = profile.Email,
                PhoneNumber = profile.PhoneNumber,
                WhatsAppNumber = profile.WhatsAppNumber,
                PrimaryDepartmentId = profile.PrimaryDepartmentId,
                PrimaryDepartmentCode = profile.PrimaryDepartmentCode,
                PrimaryDepartmentName = profile.PrimaryDepartmentName,
                PrimaryPositionId = profile.PrimaryPositionId,
                PrimaryPositionCode = profile.PrimaryPositionCode,
                PrimaryPositionName = profile.PrimaryPositionName,
                IsActive = profile.IsActive
            };

            await FillMainIdentitySummaryAsync(result, workforceProfileId, profile.UserType);
            await FillUserAccountSummaryAsync(result, workforceProfileId, now);
            await FillOrganizationAssignmentSummaryAsync(result, workforceProfileId, today);
            await FillDocumentComplianceSummaryAsync(result, workforceProfileId, today);
            await FillHealthSummaryAsync(result, workforceProfileId, today);
            await FillBankInsuranceSummaryAsync(result, workforceProfileId, today);
            await FillCompetencySummaryAsync(result, workforceProfileId, today);
            await FillEmploymentHistorySummaryAsync(result, workforceProfileId);
            FillCompletionSummary(result);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceProfileSummary.GetSummary",
                "Mengambil summary workforce profile.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    result.ProfileCode,
                    result.DisplayName,
                    result.UserType
                }
            );

            return Ok(ApiResponse<WorkforceProfileSummaryResponse>.Ok(
                result,
                "Summary workforce profile berhasil diambil."
            ));
        }

        private async Task FillMainIdentitySummaryAsync(
            WorkforceProfileSummaryResponse result,
            Guid workforceProfileId,
            UserType userType)
        {
            if (userType == UserType.Employee)
            {
                var employee = await _dbContext.MstEmployees
                    .AsNoTracking()
                    .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                    .Select(x => new
                    {
                        x.Id,
                        x.EmployeeCode,
                        x.EmployeeNumber,
                        x.EmployeeStatus,
                        x.ProfessionType,
                        x.EmploymentType,
                        x.WorkLocation,
                        x.JoinDate,
                        x.ContractStartDate,
                        x.ContractEndDate,
                        x.ResignDate
                    })
                    .FirstOrDefaultAsync();

                if (employee == null)
                    return;

                result.MainIdentity.EmployeeId = employee.Id;
                result.MainIdentity.Code = employee.EmployeeCode;
                result.MainIdentity.Number = employee.EmployeeNumber;
                result.MainIdentity.Status = employee.EmployeeStatus.ToString();
                result.MainIdentity.Type = employee.ProfessionType.ToString();
                result.MainIdentity.EmploymentType = employee.EmploymentType.ToString();
                result.MainIdentity.WorkLocation = employee.WorkLocation;
                result.MainIdentity.JoinDate = employee.JoinDate;
                result.MainIdentity.ContractStartDate = employee.ContractStartDate;
                result.MainIdentity.ContractEndDate = employee.ContractEndDate;
                result.MainIdentity.ResignDate = employee.ResignDate;

                return;
            }

            if (userType == UserType.PermanentDoctor ||
                userType == UserType.GuestDoctor)
            {
                var doctor = await _dbContext.MstDoctors
                    .AsNoTracking()
                    .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                    .Select(x => new
                    {
                        x.Id,
                        x.DoctorCode,
                        x.DoctorNumber,
                        x.DoctorStatus,
                        x.DoctorType,
                        x.EmploymentType,
                        x.WorkLocation,
                        x.JoinDate,
                        x.ContractStartDate,
                        x.ContractEndDate,
                        x.ResignDate
                    })
                    .FirstOrDefaultAsync();

                if (doctor == null)
                    return;

                result.MainIdentity.DoctorId = doctor.Id;
                result.MainIdentity.Code = doctor.DoctorCode;
                result.MainIdentity.Number = doctor.DoctorNumber;
                result.MainIdentity.Status = doctor.DoctorStatus.ToString();
                result.MainIdentity.Type = doctor.DoctorType.ToString();
                result.MainIdentity.EmploymentType = doctor.EmploymentType.ToString();
                result.MainIdentity.WorkLocation = doctor.WorkLocation;
                result.MainIdentity.JoinDate = doctor.JoinDate;
                result.MainIdentity.ContractStartDate = doctor.ContractStartDate;
                result.MainIdentity.ContractEndDate = doctor.ContractEndDate;
                result.MainIdentity.ResignDate = doctor.ResignDate;

                return;
            }

            if (userType == UserType.ExternalUser)
            {
                var externalUser = await _dbContext.MstExternalUsers
                    .AsNoTracking()
                    .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                    .Select(x => new
                    {
                        x.Id,
                        x.ExternalCode,
                        x.ExternalUserStatus,
                        x.ExternalUserType,
                        x.EngagementType,
                        x.WorkLocation,
                        x.ContractStartDate,
                        x.ContractEndDate,
                        x.AccessStartDate,
                        x.AccessEndDate
                    })
                    .FirstOrDefaultAsync();

                if (externalUser == null)
                    return;

                result.MainIdentity.ExternalUserId = externalUser.Id;
                result.MainIdentity.Code = externalUser.ExternalCode;
                result.MainIdentity.Number = null;
                result.MainIdentity.Status = externalUser.ExternalUserStatus.ToString();
                result.MainIdentity.Type = externalUser.ExternalUserType.ToString();
                result.MainIdentity.EmploymentType = externalUser.EngagementType.ToString();
                result.MainIdentity.WorkLocation = externalUser.WorkLocation;
                result.MainIdentity.ContractStartDate = externalUser.ContractStartDate;
                result.MainIdentity.ContractEndDate = externalUser.ContractEndDate;
                result.MainIdentity.AccessStartDate = externalUser.AccessStartDate;
                result.MainIdentity.AccessEndDate = externalUser.AccessEndDate;
            }
        }

        private async Task FillUserAccountSummaryAsync(
            WorkforceProfileSummaryResponse result,
            Guid workforceProfileId,
            DateTime now)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId)
                .Select(x => new
                {
                    x.Id,
                    x.UserCode,
                    x.UserName,
                    x.Email,
                    x.IsActive,
                    x.MustChangePassword,
                    x.LastLoginAt,
                    x.AccessValidUntil,
                    x.IsFingerprintRegistrationEnabled
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                result.UserAccount.HasUserAccount = false;
                return;
            }

            result.UserAccount.HasUserAccount = true;
            result.UserAccount.UserId = user.Id;
            result.UserAccount.UserCode = user.UserCode;
            result.UserAccount.UserName = user.UserName;
            result.UserAccount.Email = user.Email;
            result.UserAccount.IsActive = user.IsActive;
            result.UserAccount.MustChangePassword = user.MustChangePassword;
            result.UserAccount.LastLoginAt = user.LastLoginAt;
            result.UserAccount.AccessValidUntil = user.AccessValidUntil;
            result.UserAccount.IsAccessExpired = user.AccessValidUntil.HasValue &&
                                                 user.AccessValidUntil.Value < now;
            result.UserAccount.IsFingerprintRegistrationEnabled = user.IsFingerprintRegistrationEnabled;

            result.UserAccount.FingerprintCredentialCount =
                await _dbContext.ApplicationUserFingerprintCredentials
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.UserId == user.Id &&
                        x.IsActive &&
                        !x.IsDelete);
        }

        private async Task FillOrganizationAssignmentSummaryAsync(
            WorkforceProfileSummaryResponse result,
            Guid workforceProfileId,
            DateTime today)
        {
            var query = _dbContext.Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            result.OrganizationAssignment.TotalAssignment =
                await query.CountAsync();

            result.OrganizationAssignment.ActiveAssignment =
                await query.CountAsync(x => x.IsActive);

            result.OrganizationAssignment.InactiveAssignment =
                await query.CountAsync(x => !x.IsActive);

            result.OrganizationAssignment.PrimaryAssignment =
                await query.CountAsync(x => x.IsPrimary && x.IsActive);

            result.OrganizationAssignment.CurrentlyValidAssignment =
                await query.CountAsync(x =>
                    x.IsActive &&
                    x.EffectiveStartDate.Date <= today &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today));

            result.OrganizationAssignment.ExpiredAssignment =
                await query.CountAsync(x =>
                    x.EffectiveEndDate.HasValue &&
                    x.EffectiveEndDate.Value.Date < today);

            var primary = await query
                .Where(x => x.IsPrimary && x.IsActive)
                .Select(x => new
                {
                    x.DepartmentId,
                    DepartmentCode = x.Department != null ? x.Department.DepartmentCode : null,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    x.PositionId,
                    PositionCode = x.Position != null ? x.Position.PositionCode : null,
                    PositionName = x.Position != null ? x.Position.PositionName : null
                })
                .FirstOrDefaultAsync();

            result.OrganizationAssignment.HasPrimaryAssignment = primary != null;

            if (primary == null)
                return;

            result.OrganizationAssignment.PrimaryDepartmentId = primary.DepartmentId;
            result.OrganizationAssignment.PrimaryDepartmentCode = primary.DepartmentCode;
            result.OrganizationAssignment.PrimaryDepartmentName = primary.DepartmentName;
            result.OrganizationAssignment.PrimaryPositionId = primary.PositionId;
            result.OrganizationAssignment.PrimaryPositionCode = primary.PositionCode;
            result.OrganizationAssignment.PrimaryPositionName = primary.PositionName;
        }

        private async Task FillDocumentComplianceSummaryAsync(
            WorkforceProfileSummaryResponse result,
            Guid workforceProfileId,
            DateTime today)
        {
            var documentQuery = _dbContext.Set<WfpDocument>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            result.DocumentCompliance.DocumentTotal = await documentQuery.CountAsync();
            result.DocumentCompliance.DocumentVerified = await documentQuery.CountAsync(x => x.IsVerified);
            result.DocumentCompliance.DocumentUnverified = await documentQuery.CountAsync(x => !x.IsVerified);
            result.DocumentCompliance.DocumentExpired = await documentQuery.CountAsync(x =>
                x.ExpiredDate.HasValue &&
                x.ExpiredDate.Value.Date < today);
            result.DocumentCompliance.DocumentWithFile = await documentQuery.CountAsync(x =>
                !string.IsNullOrWhiteSpace(x.FilePath));

            var educationQuery = _dbContext.Set<WfpEducation>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            result.DocumentCompliance.EducationTotal = await educationQuery.CountAsync();
            result.DocumentCompliance.EducationVerified = await educationQuery.CountAsync(x => x.IsVerified);
            result.DocumentCompliance.EducationUnverified = await educationQuery.CountAsync(x => !x.IsVerified);
            result.DocumentCompliance.EducationWithFile = await educationQuery.CountAsync(x =>
                !string.IsNullOrWhiteSpace(x.FilePath));

            var trainingQuery = _dbContext.Set<WfpTrainingRecord>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            result.DocumentCompliance.TrainingTotal = await trainingQuery.CountAsync();
            result.DocumentCompliance.TrainingVerified = await trainingQuery.CountAsync(x => x.IsVerified);
            result.DocumentCompliance.TrainingUnverified = await trainingQuery.CountAsync(x => !x.IsVerified);
            result.DocumentCompliance.TrainingWithFile = await trainingQuery.CountAsync(x =>
                !string.IsNullOrWhiteSpace(x.FilePath));
            result.DocumentCompliance.TrainingTotalCreditPoint = await trainingQuery
                .Where(x => x.IsActive)
                .SumAsync(x => x.CreditPoint);

            var certificationQuery = _dbContext.Set<WfpCertification>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            result.DocumentCompliance.CertificationTotal = await certificationQuery.CountAsync();
            result.DocumentCompliance.CertificationVerified = await certificationQuery.CountAsync(x => x.IsVerified);
            result.DocumentCompliance.CertificationUnverified = await certificationQuery.CountAsync(x => !x.IsVerified);
            result.DocumentCompliance.CertificationExpired = await certificationQuery.CountAsync(x =>
                !x.IsLifetime &&
                x.ExpiredDate.HasValue &&
                x.ExpiredDate.Value.Date < today);
            result.DocumentCompliance.CertificationWithFile = await certificationQuery.CountAsync(x =>
                !string.IsNullOrWhiteSpace(x.FilePath));

            var credentialQuery = _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            result.DocumentCompliance.CredentialLicenseTotal = await credentialQuery.CountAsync();
            result.DocumentCompliance.CredentialLicenseVerified = await credentialQuery.CountAsync(x => x.IsVerified);
            result.DocumentCompliance.CredentialLicenseUnverified = await credentialQuery.CountAsync(x => !x.IsVerified);
            result.DocumentCompliance.CredentialLicenseExpired = await credentialQuery.CountAsync(x =>
                x.ExpiredDate.Date < today);
            result.DocumentCompliance.CredentialLicenseCurrentlyValid = await credentialQuery.CountAsync(x =>
                x.IsActive &&
                x.IsVerified &&
                x.VerificationStatus == CredentialVerificationStatus.Verified &&
                x.ExpiredDate.Date >= today);
            result.DocumentCompliance.CredentialLicenseWithFile = await credentialQuery.CountAsync(x =>
                !string.IsNullOrWhiteSpace(x.FilePath));

            var clinicalPrivilegeQuery = _dbContext.Set<WfpClinicalPrivilege>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            result.DocumentCompliance.ClinicalPrivilegeTotal = await clinicalPrivilegeQuery.CountAsync();
            result.DocumentCompliance.ClinicalPrivilegeActive = await clinicalPrivilegeQuery.CountAsync(x => x.IsActive);
            result.DocumentCompliance.ClinicalPrivilegeCurrentlyValid = await clinicalPrivilegeQuery.CountAsync(x =>
                x.IsActive &&
                x.PrivilegeStatus == ClinicalPrivilegeStatus.Active &&
                (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today));
            result.DocumentCompliance.ClinicalPrivilegeExpired = await clinicalPrivilegeQuery.CountAsync(x =>
                x.EffectiveEndDate.HasValue &&
                x.EffectiveEndDate.Value.Date < today);
            result.DocumentCompliance.ClinicalPrivilegeWithFile = await clinicalPrivilegeQuery.CountAsync(x =>
                !string.IsNullOrWhiteSpace(x.SupportingFilePath));

            result.DocumentCompliance.HasExpiredCompliance =
                result.DocumentCompliance.DocumentExpired > 0 ||
                result.DocumentCompliance.CertificationExpired > 0 ||
                result.DocumentCompliance.CredentialLicenseExpired > 0 ||
                result.DocumentCompliance.ClinicalPrivilegeExpired > 0;

            result.DocumentCompliance.HasPendingVerification =
                result.DocumentCompliance.DocumentUnverified > 0 ||
                result.DocumentCompliance.EducationUnverified > 0 ||
                result.DocumentCompliance.TrainingUnverified > 0 ||
                result.DocumentCompliance.CertificationUnverified > 0 ||
                result.DocumentCompliance.CredentialLicenseUnverified > 0;
        }

        private async Task FillHealthSummaryAsync(
            WorkforceProfileSummaryResponse result,
            Guid workforceProfileId,
            DateTime today)
        {
            var query = _dbContext.Set<WfpHealthRecord>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            result.Health.HealthRecordTotal = await query.CountAsync();
            result.Health.HealthRecordVerified = await query.CountAsync(x => x.IsVerified);
            result.Health.HealthRecordUnverified = await query.CountAsync(x => !x.IsVerified);
            result.Health.HealthRecordExpired = await query.CountAsync(x =>
                x.ExpiredDate.HasValue &&
                x.ExpiredDate.Value.Date < today);
            result.Health.HealthRecordCurrentlyValid = await query.CountAsync(x =>
                x.IsActive &&
                x.IsVerified &&
                (!x.ExpiredDate.HasValue || x.ExpiredDate.Value.Date >= today));
            result.Health.HealthRecordCompliantForWork = await query.CountAsync(x =>
                x.IsActive &&
                x.IsVerified &&
                (!x.ExpiredDate.HasValue || x.ExpiredDate.Value.Date >= today) &&
                (!x.IsFitToWork.HasValue || x.IsFitToWork == true));
            result.Health.HealthRecordWithFile = await query.CountAsync(x =>
                !string.IsNullOrWhiteSpace(x.FilePath));

            var latestHealth = await query
                .OrderByDescending(x => x.RecordDate)
                .Select(x => new
                {
                    x.RecordDate,
                    x.IsFitToWork
                })
                .FirstOrDefaultAsync();

            if (latestHealth != null)
            {
                result.Health.LatestHealthRecordDate = latestHealth.RecordDate;
                result.Health.LatestFitToWork = latestHealth.IsFitToWork;
            }

            result.Health.NearestHealthRecordExpiredDate =
                await query
                    .Where(x =>
                        x.ExpiredDate.HasValue &&
                        x.ExpiredDate.Value.Date >= today)
                    .OrderBy(x => x.ExpiredDate)
                    .Select(x => x.ExpiredDate)
                    .FirstOrDefaultAsync();
        }

        private async Task FillBankInsuranceSummaryAsync(
            WorkforceProfileSummaryResponse result,
            Guid workforceProfileId,
            DateTime today)
        {
            var bankQuery = _dbContext.Set<WfpBankAccount>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            result.BankInsurance.BankAccountTotal = await bankQuery.CountAsync();
            result.BankInsurance.ActiveBankAccount = await bankQuery.CountAsync(x => x.IsActive);
            result.BankInsurance.HasPrimaryBankAccount = await bankQuery.AnyAsync(x =>
                x.IsPrimary &&
                x.IsActive);

            var primaryBank = await bankQuery
                .Where(x => x.IsPrimary && x.IsActive)
                .Select(x => new
                {
                    x.BankName,
                    x.AccountNumber
                })
                .FirstOrDefaultAsync();

            if (primaryBank != null)
            {
                result.BankInsurance.PrimaryBankName = primaryBank.BankName;
                result.BankInsurance.PrimaryAccountNumberMasked = MaskNumber(primaryBank.AccountNumber);
            }

            var insuranceQuery = _dbContext.Set<WfpInsurance>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            result.BankInsurance.InsuranceTotal = await insuranceQuery.CountAsync();
            result.BankInsurance.ActiveInsurance = await insuranceQuery.CountAsync(x => x.IsActive);
            result.BankInsurance.HasInsuranceProfile = await insuranceQuery.AnyAsync();
            result.BankInsurance.HasBpjsKesehatan = await insuranceQuery.AnyAsync(x =>
                x.IsActive &&
                x.IsBpjsKesehatanEnabled);
            result.BankInsurance.HasBpjsKetenagakerjaan = await insuranceQuery.AnyAsync(x =>
                x.IsActive &&
                x.IsBpjsKetenagakerjaanEnabled);
            result.BankInsurance.HasPrivateInsurance = await insuranceQuery.AnyAsync(x =>
                x.IsActive &&
                x.IsPrivateInsuranceEnabled);
            result.BankInsurance.HasCurrentlyValidInsurance = await insuranceQuery.AnyAsync(x =>
                x.IsActive &&
                (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today));
        }

        private async Task FillCompetencySummaryAsync(
            WorkforceProfileSummaryResponse result,
            Guid workforceProfileId,
            DateTime today)
        {
            var items = await _dbContext.Set<WfpCompetencyAssessment>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .Select(x => new
                {
                    x.AssessmentDate,
                    x.ExpiredDate,
                    x.IsVerified,
                    x.ResultStatus
                })
                .ToListAsync();

            result.Competency.AssessmentTotal = items.Count;
            result.Competency.AssessmentVerified = items.Count(x => x.IsVerified);
            result.Competency.AssessmentUnverified = items.Count(x => !x.IsVerified);
            result.Competency.AssessmentExpired = items.Count(x =>
                x.ExpiredDate.HasValue &&
                x.ExpiredDate.Value.Date < today);
            result.Competency.PassedAssessment = items.Count(x =>
                string.Equals(x.ResultStatus.ToString(), "Passed", StringComparison.OrdinalIgnoreCase));
            result.Competency.FailedAssessment = items.Count(x =>
                string.Equals(x.ResultStatus.ToString(), "Failed", StringComparison.OrdinalIgnoreCase));
            result.Competency.NeedTrainingAssessment = items.Count(x =>
                string.Equals(x.ResultStatus.ToString(), "NeedTraining", StringComparison.OrdinalIgnoreCase));
            result.Competency.LatestAssessmentDate = items
                .OrderByDescending(x => x.AssessmentDate)
                .Select(x => (DateTime?)x.AssessmentDate)
                .FirstOrDefault();
        }

        private async Task FillEmploymentHistorySummaryAsync(
            WorkforceProfileSummaryResponse result,
            Guid workforceProfileId)
        {
            var items = await _dbContext.Set<WfpEmploymentHistory>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.EffectiveDate)
                .Select(x => new
                {
                    x.HistoryType,
                    x.OldStatus,
                    x.NewStatus,
                    x.EffectiveDate,
                    x.IsActive
                })
                .ToListAsync();

            result.EmploymentHistory.HistoryTotal = items.Count;
            result.EmploymentHistory.ActiveHistory = items.Count(x => x.IsActive);

            var latest = items.FirstOrDefault();

            if (latest == null)
                return;

            result.EmploymentHistory.LatestEffectiveDate = latest.EffectiveDate;
            result.EmploymentHistory.LatestHistoryType = latest.HistoryType.ToString();
            result.EmploymentHistory.LatestOldStatus = latest.OldStatus;
            result.EmploymentHistory.LatestNewStatus = latest.NewStatus;
        }

        private static void FillCompletionSummary(WorkforceProfileSummaryResponse result)
        {
            var sections = new Dictionary<string, bool>
            {
                ["MainIdentity"] = result.MainIdentity.EmployeeId.HasValue ||
                                   result.MainIdentity.DoctorId.HasValue ||
                                   result.MainIdentity.ExternalUserId.HasValue,

                ["UserAccount"] = result.UserAccount.HasUserAccount,

                ["OrganizationAssignment"] = result.OrganizationAssignment.HasPrimaryAssignment,

                ["Document"] = result.DocumentCompliance.DocumentTotal > 0,

                ["Education"] = result.DocumentCompliance.EducationTotal > 0,

                ["BankAccount"] = result.BankInsurance.HasPrimaryBankAccount,

                ["Insurance"] = result.BankInsurance.HasInsuranceProfile
            };

            result.Completion.TotalRequiredSection = sections.Count;
            result.Completion.CompletedSection = sections.Count(x => x.Value);
            result.Completion.CompletionPercentage = sections.Count == 0
                ? 0
                : Math.Round(result.Completion.CompletedSection / (decimal)sections.Count * 100m, 2);

            result.Completion.MissingSections = sections
                .Where(x => !x.Value)
                .Select(x => x.Key)
                .ToList();

            if (result.DocumentCompliance.HasExpiredCompliance)
                result.Completion.WarningSections.Add("ExpiredCompliance");

            if (result.DocumentCompliance.HasPendingVerification)
                result.Completion.WarningSections.Add("PendingVerification");

            if (result.Health.HealthRecordExpired > 0)
                result.Completion.WarningSections.Add("ExpiredHealthRecord");

            if (result.UserAccount.HasUserAccount && result.UserAccount.IsAccessExpired)
                result.Completion.WarningSections.Add("ExpiredUserAccess");
        }

        private static string? MaskNumber(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var text = value.Trim();

            if (text.Length <= 4)
                return new string('*', text.Length);

            return $"{new string('*', Math.Max(0, text.Length - 4))}{text[^4..]}";
        }
    }
}