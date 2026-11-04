using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    public class HrdEmployee360Service : IHrdEmployee360Service
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly AccessPermissionService _permissionService;

        public HrdEmployee360Service(
            ApplicationDbContext dbContext,
            AccessPermissionService permissionService)
        {
            _dbContext = dbContext;
            _permissionService = permissionService;
        }

        public async Task<Employee360ServiceResult> GetEmployee360Async(
            Guid workforceProfileId,
            ClaimsPrincipal userPrincipal,
            Guid callerUserId,
            DateTime? asOfDate = null,
            CancellationToken ct = default)
        {
            var caller = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == callerUserId, ct);

            if (caller == null)
            {
                return Employee360ServiceResult.Forbidden("Identitas pengguna tidak valid.");
            }

            var today = (asOfDate?.Date ?? DateTime.UtcNow.Date);

            // Check if caller has HR permission
            var isHr = await _permissionService.HasAccessAsync(userPrincipal, "HrdEmployee360", "ReadFull")
                       || userPrincipal.IsInRole("SuperAdmin")
                       || userPrincipal.IsInRole("Admin")
                       || await _permissionService.HasAccessAsync(userPrincipal, "HrdPersonnelActionOrder", "Read");

            // Check if caller is active direct manager of target employee
            var isDirectManager = false;
            if (!isHr && caller.WorkforceProfileId.HasValue && caller.WorkforceProfileId.Value != Guid.Empty)
            {
                isDirectManager = await _dbContext.WfpManagerAssignments
                    .AsNoTracking()
                    .AnyAsync(x => x.WorkforceProfileId == workforceProfileId &&
                                   x.ManagerWorkforceProfileId == caller.WorkforceProfileId.Value &&
                                   x.IsPrimaryManager &&
                                   x.IsActive &&
                                   !x.IsDelete &&
                                   x.EffectiveStartDate <= today &&
                                   (x.EffectiveEndDate == null || x.EffectiveEndDate >= today), ct);
            }

            if (!isHr && !isDirectManager)
            {
                return Employee360ServiceResult.Forbidden(
                    "Akses ditolak. Profil 360 hanya dapat diakses oleh HR Administrator atau Atasan Langsung aktif.");
            }

            var profile = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete, ct);

            if (profile == null)
            {
                return Employee360ServiceResult.NotFound("Profil pegawai tidak ditemukan.");
            }

            var employee = await _dbContext.MstEmployees
                .Include(x => x.EmploymentStatus)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete, ct);

            var activeOrg = await _dbContext.WfpOrganizationAssignments
                .Include(x => x.OrganizationUnit)
                .Include(x => x.Department)
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId &&
                            x.IsActive &&
                            !x.IsDelete &&
                            x.EffectiveStartDate <= today &&
                            (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .FirstOrDefaultAsync(ct);

            var activePos = await _dbContext.WfpPositionAssignments
                .Include(x => x.Position)
                    .ThenInclude(p => p.Department)
                .Include(x => x.OrganizationUnit)
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId &&
                            x.IsActive &&
                            !x.IsDelete &&
                            x.EffectiveStartDate <= today &&
                            (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .FirstOrDefaultAsync(ct);

            var activeMgr = await _dbContext.WfpManagerAssignments
                .Include(x => x.ManagerWorkforceProfile)
                .Include(x => x.Department)
                .Include(x => x.ManagerPosition)
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId &&
                            x.IsActive &&
                            !x.IsDelete &&
                            x.EffectiveStartDate <= today &&
                            (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
                .OrderByDescending(x => x.IsPrimaryManager)
                .ThenByDescending(x => x.EffectiveStartDate)
                .FirstOrDefaultAsync(ct);

            var orders = await _dbContext.HrdPersonnelActionOrders
                .Include(x => x.TargetOrganizationUnit)
                .Include(x => x.TargetDepartment)
                .Include(x => x.TargetPosition)
                .Include(x => x.TargetManagerProfile)
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.EffectiveDate)
                .Select(x => new PersonnelActionOrderListResponse
                {
                    Id = x.Id,
                    ActionOrderNumber = x.ActionOrderNumber,
                    WorkforceProfileId = x.WorkforceProfileId,
                    EmployeeName = profile.DisplayName,
                    ProfileCode = profile.ProfileCode,
                    ActionCategory = x.ActionCategory,
                    EffectiveDate = x.EffectiveDate,
                    PostingDate = x.PostingDate,
                    OrderStatus = x.OrderStatus,
                    ReasonText = x.ReasonText,
                    HasSignedDocument = !string.IsNullOrEmpty(x.SignedDocumentPath),
                    SignedDocumentFileName = x.SignedDocumentFileName,
                    TargetOrganizationUnitName = x.TargetOrganizationUnit != null ? x.TargetOrganizationUnit.UnitName : null,
                    TargetDepartmentName = x.TargetDepartment != null ? x.TargetDepartment.DepartmentName : null,
                    TargetPositionName = x.TargetPosition != null ? x.TargetPosition.PositionName : null,
                    TargetManagerName = x.TargetManagerProfile != null ? x.TargetManagerProfile.DisplayName : null,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(ct);

            var orgHistories = await _dbContext.WfpOrganizationAssignments
                .Include(x => x.OrganizationUnit)
                .Include(x => x.PersonnelActionOrder)
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.EffectiveStartDate)
                .Select(x => new EmploymentAssignmentHistoryDto
                {
                    Id = x.Id,
                    Type = "Organization",
                    Detail = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : "Unit Kerja",
                    StartDate = x.EffectiveStartDate,
                    EndDate = x.EffectiveEndDate,
                    IsPrimary = x.IsPrimary,
                    ActionOrderNumber = x.PersonnelActionOrder != null ? x.PersonnelActionOrder.ActionOrderNumber : null
                })
                .ToListAsync(ct);

            var posHistories = await _dbContext.WfpPositionAssignments
                .Include(x => x.Position)
                .Include(x => x.PersonnelActionOrder)
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.EffectiveStartDate)
                .Select(x => new EmploymentAssignmentHistoryDto
                {
                    Id = x.Id,
                    Type = "Position",
                    Detail = x.Position != null ? x.Position.PositionName : "Jabatan",
                    StartDate = x.EffectiveStartDate,
                    EndDate = x.EffectiveEndDate,
                    IsPrimary = x.IsPrimary,
                    ActionOrderNumber = x.PersonnelActionOrder != null ? x.PersonnelActionOrder.ActionOrderNumber : null
                })
                .ToListAsync(ct);

            var mgrHistories = await _dbContext.WfpManagerAssignments
                .Include(x => x.ManagerWorkforceProfile)
                .Include(x => x.PersonnelActionOrder)
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.EffectiveStartDate)
                .Select(x => new EmploymentAssignmentHistoryDto
                {
                    Id = x.Id,
                    Type = "Manager",
                    Detail = x.ManagerWorkforceProfile != null ? x.ManagerWorkforceProfile.DisplayName : "Atasan",
                    StartDate = x.EffectiveStartDate,
                    EndDate = x.EffectiveEndDate,
                    IsPrimary = x.IsPrimaryManager,
                    ActionOrderNumber = x.PersonnelActionOrder != null ? x.PersonnelActionOrder.ActionOrderNumber : null
                })
                .ToListAsync(ct);

            var allHistories = orgHistories
                .Concat(posHistories)
                .Concat(mgrHistories)
                .OrderByDescending(x => x.StartDate)
                .ToList();

            var certs = await _dbContext.WfpCertifications
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.IssueDate)
                .Select(x => new EmployeeCompetencyDto
                {
                    Title = x.CertificationName,
                    Issuer = x.Issuer,
                    IssueDate = x.IssueDate,
                    ExpiryDate = x.ExpiredDate,
                    CertificateNumber = x.CertificateNumber
                })
                .ToListAsync(ct);

            string? mgrPositionName = activeMgr?.ManagerPosition?.PositionName;
            string? mgrDepartmentName = activeMgr?.Department?.DepartmentName;
            if (activeMgr != null && (mgrPositionName == null || mgrDepartmentName == null))
            {
                var mgrPos = await _dbContext.WfpPositionAssignments
                    .Include(p => p.Position)
                        .ThenInclude(p => p.Department)
                    .AsNoTracking()
                    .Where(p => p.WorkforceProfileId == activeMgr.ManagerWorkforceProfileId &&
                                p.IsActive && !p.IsDelete &&
                                p.EffectiveStartDate <= today &&
                                (p.EffectiveEndDate == null || p.EffectiveEndDate >= today))
                    .OrderByDescending(p => p.IsPrimary)
                    .ThenByDescending(p => p.EffectiveStartDate)
                    .FirstOrDefaultAsync(ct);

                mgrPositionName ??= mgrPos?.Position?.PositionName;
                mgrDepartmentName ??= mgrPos?.Position?.Department?.DepartmentName;
            }

            var response = new Employee360Response
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                UserType = profile.UserType.ToString(),
                Email = profile.Email,
                PhoneNumber = profile.PhoneNumber,
                WhatsAppNumber = profile.WhatsAppNumber,
                IsActive = profile.IsActive,
                ViewMode = isHr ? "HR" : "DirectManager",
                CurrentEmployment = new CurrentEmploymentInfoDto
                {
                    LegalEntityId = activeOrg?.LegalEntityId ?? activePos?.LegalEntityId,
                    HospitalSiteId = activeOrg?.HospitalSiteId ?? activePos?.HospitalSiteId,
                    OrganizationUnitId = activeOrg?.OrganizationUnitId ?? activePos?.OrganizationUnitId,
                    OrganizationUnitName = activeOrg?.OrganizationUnit?.UnitName ?? activePos?.OrganizationUnit?.UnitName,
                    DepartmentId = activeOrg?.DepartmentId ?? activePos?.DepartmentId ?? activePos?.Position?.DepartmentId,
                    DepartmentName = activeOrg?.Department?.DepartmentName ?? activePos?.Position?.Department?.DepartmentName,
                    PositionId = activePos?.PositionId ?? activeOrg?.PositionId,
                    PositionName = activePos?.Position?.PositionName ?? activeOrg?.Position?.PositionName,
                    AssignmentType = activeOrg?.AssignmentType ?? activePos?.AssignmentType,
                    EffectiveStartDate = activeOrg?.EffectiveStartDate ?? activePos?.EffectiveStartDate,
                    EmployeeCode = employee?.EmployeeCode ?? profile.ProfileCode,
                    EmploymentStatusName = employee?.EmploymentStatus?.EmploymentStatusName ?? (profile.IsActive ? "Aktif" : "Nonaktif")
                },
                DirectManager = activeMgr?.ManagerWorkforceProfile != null
                    ? new DirectManagerInfoDto
                    {
                        ManagerProfileId = activeMgr.ManagerWorkforceProfile.Id,
                        ManagerName = activeMgr.ManagerWorkforceProfile.DisplayName,
                        ManagerPositionName = mgrPositionName,
                        ManagerDepartmentName = mgrDepartmentName
                    }
                    : null,
                PersonnelActionOrders = orders,
                AssignmentHistories = allHistories,
                Competencies = certs
            };

            if (isHr)
            {
                // Confidential Financial Profile
                var salary = await _dbContext.WfpSalaryAssignments
                    .Include(x => x.SalaryGrade)
                    .AsNoTracking()
                    .Where(x => x.WorkforceProfileId == workforceProfileId &&
                                x.IsActive &&
                                !x.IsDelete &&
                                x.EffectiveStartDate <= today &&
                                (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenByDescending(x => x.EffectiveStartDate)
                    .FirstOrDefaultAsync(ct);

                var bank = await _dbContext.WfpBankAccounts
                    .Include(x => x.Bank)
                    .AsNoTracking()
                    .Where(x => x.WorkforceProfileId == workforceProfileId &&
                                x.IsActive &&
                                !x.IsDelete)
                    .OrderByDescending(x => x.IsPrimary)
                    .FirstOrDefaultAsync(ct);

                response.ConfidentialFinancialProfile = new ConfidentialFinancialProfileDto
                {
                    BaseSalary = salary?.BaseSalary,
                    CurrencyCode = salary?.CurrencyCode ?? "IDR",
                    SalaryGradeName = salary?.SalaryGrade?.SalaryGradeName,
                    BankName = bank?.Bank?.BankName ?? bank?.BankName,
                    BankAccountNumber = bank?.AccountNumber,
                    BankAccountHolderName = bank?.AccountHolderName
                };

                // Confidential Personal & Discipline Profile
                var familyMembers = await _dbContext.WfpFamilyMembers
                    .AsNoTracking()
                    .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                    .Select(x => $"{x.FullName} ({x.Relationship})")
                    .ToListAsync(ct);

                var sanctions = await _dbContext.WfpDisciplinaryActions
                    .AsNoTracking()
                    .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                    .Select(x => $"{x.ActionType}: {x.Subject} ({x.ActionDate:yyyy-MM-dd})")
                    .ToListAsync(ct);

                response.ConfidentialPersonalAndDiscipline = new ConfidentialPersonalAndDisciplineDto
                {
                    NationalIdentificationNumber = employee?.IdentityNumber,
                    TaxIdentificationNumber = null,
                    DateOfBirth = employee?.BirthDate,
                    BloodType = employee?.BloodType.ToString(),
                    FamilyMembersSummary = familyMembers,
                    DisciplinarySanctions = sanctions
                };
            }
            else
            {
                response.ConfidentialFinancialProfile = null;
                response.ConfidentialPersonalAndDiscipline = null;
            }

            return Employee360ServiceResult.Ok(response);
        }
    }
}
