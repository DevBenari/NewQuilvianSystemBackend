using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
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
    [Route("api/v1/corporate/human-resource/workforce-profiles")]
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
        private const string LogCategory = "Corporate.HumanResource.Workforce";

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
            Description = "Melihat ringkasan workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceProfileSummary", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId)
        {
            var now = DateTime.UtcNow;
            var today = now.Date;

            var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd = monthStart.AddMonths(1);

            var monthStartDateOnly = DateOnly.FromDateTime(monthStart);
            var monthEndDateOnly = DateOnly.FromDateTime(monthEnd);

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
            await FillOrganizationSummaryAsync(result, workforceProfileId);
            await FillDocumentComplianceSummaryAsync(result, workforceProfileId, today);
            await FillHealthSummaryAsync(result, workforceProfileId, today);
            await FillScheduleAttendanceSummaryAsync(
                result,
                workforceProfileId,
                monthStartDateOnly,
                monthEndDateOnly
            );
            await FillPayrollBenefitSummaryAsync(result, workforceProfileId);
            await FillLeaveOvertimeSummaryAsync(result, workforceProfileId);

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
                {
                    return;
                }

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
                {
                    return;
                }

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
                {
                    return;
                }

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

        private async Task FillOrganizationSummaryAsync(
            WorkforceProfileSummaryResponse result,
            Guid workforceProfileId)
        {
            result.Organization.TotalAssignment =
                await _dbContext.WfpOrganizationAssignments
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

            result.Organization.ActiveAssignment =
                await _dbContext.WfpOrganizationAssignments
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsActive &&
                        !x.IsDelete);

            var primary = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsPrimary &&
                    x.IsActive &&
                    !x.IsDelete)
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

            result.Organization.HasPrimaryAssignment = primary != null;

            if (primary == null)
            {
                return;
            }

            result.Organization.PrimaryDepartmentId = primary.DepartmentId;
            result.Organization.PrimaryDepartmentCode = primary.DepartmentCode;
            result.Organization.PrimaryDepartmentName = primary.DepartmentName;
            result.Organization.PrimaryPositionId = primary.PositionId;
            result.Organization.PrimaryPositionCode = primary.PositionCode;
            result.Organization.PrimaryPositionName = primary.PositionName;
        }

        private async Task FillDocumentComplianceSummaryAsync(
            WorkforceProfileSummaryResponse result,
            Guid workforceProfileId,
            DateTime today)
        {
            result.DocumentCompliance.DocumentTotal =
                await _dbContext.WfpDocuments.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

            result.DocumentCompliance.DocumentVerified =
                await _dbContext.WfpDocuments.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsVerified &&
                        !x.IsDelete);

            result.DocumentCompliance.DocumentUnverified =
                await _dbContext.WfpDocuments.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsVerified &&
                        !x.IsDelete);

            result.DocumentCompliance.DocumentExpired =
                await _dbContext.WfpDocuments.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.ExpiredDate.HasValue &&
                        x.ExpiredDate.Value.Date < today &&
                        !x.IsDelete);

            result.DocumentCompliance.EducationTotal =
                await _dbContext.WfpEducations.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

            result.DocumentCompliance.EducationVerified =
                await _dbContext.WfpEducations.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsVerified &&
                        !x.IsDelete);

            result.DocumentCompliance.TrainingTotal =
                await _dbContext.WfpTrainingRecords.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

            result.DocumentCompliance.TrainingVerified =
                await _dbContext.WfpTrainingRecords.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsVerified &&
                        !x.IsDelete);

            result.DocumentCompliance.CertificationTotal =
                await _dbContext.WfpCertifications.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

            result.DocumentCompliance.CertificationVerified =
                await _dbContext.WfpCertifications.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsVerified &&
                        !x.IsDelete);

            result.DocumentCompliance.CertificationExpired =
                await _dbContext.WfpCertifications.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsLifetime &&
                        x.ExpiredDate.HasValue &&
                        x.ExpiredDate.Value.Date < today &&
                        !x.IsDelete);

            result.DocumentCompliance.CredentialLicenseTotal =
                await _dbContext.WfpCredentialLicenses.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

            result.DocumentCompliance.CredentialLicenseVerified =
                await _dbContext.WfpCredentialLicenses.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsVerified &&
                        !x.IsDelete);

            result.DocumentCompliance.CredentialLicenseExpired =
                await _dbContext.WfpCredentialLicenses.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.ExpiredDate.Date < today &&
                        !x.IsDelete);

            result.DocumentCompliance.ClinicalPrivilegeTotal =
                await _dbContext.WfpClinicalPrivileges.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

            result.DocumentCompliance.ClinicalPrivilegeActive =
                await _dbContext.WfpClinicalPrivileges.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsActive &&
                        !x.IsDelete);

            result.DocumentCompliance.HasExpiredCompliance =
                result.DocumentCompliance.DocumentExpired > 0 ||
                result.DocumentCompliance.CertificationExpired > 0 ||
                result.DocumentCompliance.CredentialLicenseExpired > 0;

            result.DocumentCompliance.HasPendingVerification =
                result.DocumentCompliance.DocumentUnverified > 0 ||
                result.DocumentCompliance.DocumentVerified < result.DocumentCompliance.DocumentTotal ||
                result.DocumentCompliance.EducationVerified < result.DocumentCompliance.EducationTotal ||
                result.DocumentCompliance.TrainingVerified < result.DocumentCompliance.TrainingTotal ||
                result.DocumentCompliance.CertificationVerified < result.DocumentCompliance.CertificationTotal ||
                result.DocumentCompliance.CredentialLicenseVerified < result.DocumentCompliance.CredentialLicenseTotal;
        }

        private async Task FillHealthSummaryAsync(
            WorkforceProfileSummaryResponse result,
            Guid workforceProfileId,
            DateTime today)
        {
            result.Health.HealthRecordTotal =
                await _dbContext.WfpHealthRecords.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

            result.Health.HealthRecordVerified =
                await _dbContext.WfpHealthRecords.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsVerified &&
                        !x.IsDelete);

            result.Health.HealthRecordExpired =
                await _dbContext.WfpHealthRecords.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.ExpiredDate.HasValue &&
                        x.ExpiredDate.Value.Date < today &&
                        !x.IsDelete);

            var latestHealth = await _dbContext.WfpHealthRecords.AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
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
                await _dbContext.WfpHealthRecords.AsNoTracking()
                    .Where(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.ExpiredDate.HasValue &&
                        x.ExpiredDate.Value.Date >= today &&
                        !x.IsDelete)
                    .OrderBy(x => x.ExpiredDate)
                    .Select(x => x.ExpiredDate)
                    .FirstOrDefaultAsync();
        }

        private async Task FillScheduleAttendanceSummaryAsync(
            WorkforceProfileSummaryResponse result,
            Guid workforceProfileId,
            DateOnly monthStart,
            DateOnly monthEnd)
        {
            result.ScheduleAttendance.WorkScheduleAssignmentTotal =
                await _dbContext.WfpWorkScheduleAssignments.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

            result.ScheduleAttendance.ActiveWorkScheduleAssignment =
                await _dbContext.WfpWorkScheduleAssignments.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsActive &&
                        !x.IsDelete);

            result.ScheduleAttendance.LatestScheduleDate =
                await _dbContext.WfpWorkScheduleAssignments.AsNoTracking()
                    .Where(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete)
                    .OrderByDescending(x => x.ScheduleDate)
                    .Select(x => (DateTime?)x.ScheduleDate.ToDateTime(TimeOnly.MinValue))
                    .FirstOrDefaultAsync();

            result.ScheduleAttendance.AttendanceThisMonth =
                await _dbContext.EmpAttendances.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.AttendanceDate >= monthStart &&
                        x.AttendanceDate < monthEnd &&
                        !x.IsDelete);

            result.ScheduleAttendance.LateThisMonth =
                await _dbContext.EmpAttendances.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.AttendanceDate >= monthStart &&
                        x.AttendanceDate < monthEnd &&
                        x.IsLate &&
                        !x.IsDelete);

            result.ScheduleAttendance.AttendanceAllTime =
                await _dbContext.EmpAttendances.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);
        }

        private async Task FillPayrollBenefitSummaryAsync(
            WorkforceProfileSummaryResponse result,
            Guid workforceProfileId)
        {
            result.PayrollBenefit.BankAccountTotal =
                await _dbContext.WfpBankAccounts.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

            result.PayrollBenefit.HasPrimaryBankAccount =
                await _dbContext.WfpBankAccounts.AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.IsPrimary &&
                        x.IsActive &&
                        !x.IsDelete);

            var payroll = await _dbContext.WfpPayrolls.AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new
                {
                    x.IsPayrollActive
                })
                .FirstOrDefaultAsync();

            result.PayrollBenefit.HasPayrollProfile = payroll != null;
            result.PayrollBenefit.IsPayrollActive = payroll?.IsPayrollActive ?? false;

            result.PayrollBenefit.HasTaxProfile =
                await _dbContext.WfpTaxes.AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

            result.PayrollBenefit.HasInsuranceProfile =
                await _dbContext.WfpInsurances.AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

            var transport = await _dbContext.WfpTransportAllowances.AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new
                {
                    x.IsEligible,
                    x.IsNightTransportEligible
                })
                .FirstOrDefaultAsync();

            result.PayrollBenefit.HasTransportAllowanceProfile = transport != null;
            result.PayrollBenefit.IsTransportEligible = transport?.IsEligible ?? false;
            result.PayrollBenefit.IsNightTransportEligible = transport?.IsNightTransportEligible ?? false;
        }

        private async Task FillLeaveOvertimeSummaryAsync(
            WorkforceProfileSummaryResponse result,
            Guid workforceProfileId)
        {
            result.LeaveOvertime.LeaveBalanceTotal =
                await _dbContext.WfpLeaveBalances.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

            var remainingLeaveItems = await _dbContext.WfpLeaveBalances.AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete)
                .Select(x => x.RemainingDays)
                .ToListAsync();

            result.LeaveOvertime.TotalRemainingLeave = remainingLeaveItems.Sum();

            result.LeaveOvertime.LeaveRequestTotal =
                await _dbContext.WfpLeaveRequests.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

            result.LeaveOvertime.PendingLeaveRequest =
                await _dbContext.WfpLeaveRequests.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.ApprovalStatus == LeaveApprovalStatus.PendingApproval &&
                        !x.IsDelete);

            result.LeaveOvertime.ApprovedLeaveRequest =
                await _dbContext.WfpLeaveRequests.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.ApprovalStatus == LeaveApprovalStatus.Approved &&
                        !x.IsDelete);

            result.LeaveOvertime.OvertimeRequestTotal =
                await _dbContext.WfpOvertimeRequests.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        !x.IsDelete);

            result.LeaveOvertime.PendingOvertimeRequest =
                await _dbContext.WfpOvertimeRequests.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.ApprovalStatus == OvertimeApprovalStatus.PendingApproval &&
                        !x.IsDelete);

            result.LeaveOvertime.ApprovedOvertimeRequest =
                await _dbContext.WfpOvertimeRequests.AsNoTracking()
                    .CountAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.ApprovalStatus == OvertimeApprovalStatus.Approved &&
                        !x.IsDelete);

            var approvedOvertimeMinutes = await _dbContext.WfpOvertimeRequests.AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.ApprovalStatus == OvertimeApprovalStatus.Approved &&
                    !x.IsDelete)
                .Select(x => x.TotalMinutes)
                .ToListAsync();

            result.LeaveOvertime.ApprovedOvertimeMinutes = approvedOvertimeMinutes.Sum();
            result.LeaveOvertime.ApprovedOvertimeHours =
                Math.Round(result.LeaveOvertime.ApprovedOvertimeMinutes / 60m, 2);
        }
    }
}