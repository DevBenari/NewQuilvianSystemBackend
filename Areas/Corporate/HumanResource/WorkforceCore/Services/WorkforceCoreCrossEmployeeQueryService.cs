using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services
{
    /// <summary>
    /// Service penyedia query daftar lintas seluruh pegawai untuk Administrasi Kepegawaian (Workforce Core).
    /// Mengimplementasikan keputusan arsitektur [DECISION] HRD-DEC-012 dan kebijakan kerahasiaan nominal gaji HRD-DEC-033.
    /// </summary>
    public class WorkforceCoreCrossEmployeeQueryService
    {
        private const string LogCategory = "Corporate.HumanResource.WorkforceCore.CrossEmployee";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceCoreCrossEmployeeQueryService(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        #region 1. Organization Assignments (Penempatan Organisasi)

        public async Task<PagedResult<WfpOrganizationAssignmentResponse>> GetOrganizationAssignmentsAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod,
            Guid? departmentId,
            Guid? positionId,
            Guid? organizationUnitId,
            Guid? workforceProfileId,
            string? assignmentType,
            bool? isPrimary,
            bool? isManagerialAssignment,
            bool? isActive,
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            var query = _dbContext.Set<WfpOrganizationAssignment>().AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Where(x => !x.IsDelete);

            if (dateRange.Start.HasValue) query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            if (dateRange.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty) query = query.Where(x => x.WorkforceProfileId == workforceProfileId.Value);
            if (departmentId.HasValue && departmentId.Value != Guid.Empty) query = query.Where(x => x.DepartmentId == departmentId.Value);
            if (positionId.HasValue && positionId.Value != Guid.Empty) query = query.Where(x => x.PositionId == positionId.Value);
            if (organizationUnitId.HasValue && organizationUnitId.Value != Guid.Empty) query = query.Where(x => x.OrganizationUnitId == organizationUnitId.Value);
            if (!string.IsNullOrWhiteSpace(assignmentType)) query = query.Where(x => x.AssignmentType.ToLower() == assignmentType.Trim().ToLower());
            if (isPrimary.HasValue) query = query.Where(x => x.IsPrimary == isPrimary.Value);
            if (isManagerialAssignment.HasValue) query = query.Where(x => x.IsManagerialAssignment == isManagerialAssignment.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.AssignmentNumber != null && x.AssignmentNumber.ToLower().Contains(keyword)) ||
                    (x.AssignmentType != null && x.AssignmentType.ToLower().Contains(keyword)) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.Position != null && x.Position.PositionName.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && (x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword) || x.WorkforceProfile.DisplayName.ToLower().Contains(keyword))));
            }

            var totalData = await query.CountAsync(ct);
            var sortedQuery = ApplyOrganizationSorting(query, sortBy, sortDirection);
            var entities = await sortedQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            var actorIds = entities.SelectMany(x => new[] { x.CreateBy, x.UpdateBy }).Where(id => id != Guid.Empty).Distinct().ToList();
            var actorNames = await GetActorNamesAsync(actorIds, ct);

            var items = entities.Select(x => new WfpOrganizationAssignmentResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                LegalEntityId = x.LegalEntityId,
                HospitalSiteId = x.HospitalSiteId,
                OrganizationUnitId = x.OrganizationUnitId,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department?.DepartmentName ?? string.Empty,
                PositionId = x.PositionId,
                PositionName = x.Position?.PositionName ?? string.Empty,
                CostCenterId = x.CostCenterId,
                WorkLocationId = x.WorkLocationId,
                EmployeeGradeId = x.EmployeeGradeId,
                AssignmentType = x.AssignmentType,
                IsPrimary = x.IsPrimary,
                IsManagerialAssignment = x.IsManagerialAssignment,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                AssignmentNumber = x.AssignmentNumber,
                Description = x.Description,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetActorName(actorNames, x.CreateBy),
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                UpdateByName = GetActorName(actorNames, x.UpdateBy)
            }).ToList();

            return new PagedResult<WfpOrganizationAssignmentResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        private static IOrderedQueryable<WfpOrganizationAssignment> ApplyOrganizationSorting(
            IQueryable<WfpOrganizationAssignment> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "effectiveStartDate").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "assignmenttype" => desc ? query.OrderByDescending(x => x.AssignmentType) : query.OrderBy(x => x.AssignmentType),
                "isprimary" => desc ? query.OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate),
                _ => desc ? query.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.EffectiveStartDate).ThenBy(x => x.CreateDateTime)
            };
        }

        #endregion

        #region 2. Position Assignments (Penempatan Jabatan)

        public async Task<PagedResult<WfpPositionAssignmentResponse>> GetPositionAssignmentsAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod,
            Guid? positionId,
            Guid? departmentId,
            Guid? jobLevelId,
            Guid? workforceProfileId,
            string? assignmentType,
            bool? isPrimary,
            bool? isActing,
            bool? isActive,
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            var query = _dbContext.Set<WfpPositionAssignment>().AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Position)
                .Include(x => x.Department)
                .Where(x => !x.IsDelete);

            if (dateRange.Start.HasValue) query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            if (dateRange.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty) query = query.Where(x => x.WorkforceProfileId == workforceProfileId.Value);
            if (positionId.HasValue && positionId.Value != Guid.Empty) query = query.Where(x => x.PositionId == positionId.Value);
            if (departmentId.HasValue && departmentId.Value != Guid.Empty) query = query.Where(x => x.DepartmentId == departmentId.Value);
            if (jobLevelId.HasValue && jobLevelId.Value != Guid.Empty) query = query.Where(x => x.JobLevelId == jobLevelId.Value);
            if (!string.IsNullOrWhiteSpace(assignmentType)) query = query.Where(x => x.AssignmentType.ToLower() == assignmentType.Trim().ToLower());
            if (isPrimary.HasValue) query = query.Where(x => x.IsPrimary == isPrimary.Value);
            if (isActing.HasValue) query = query.Where(x => x.IsActing == isActing.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.AssignmentType != null && x.AssignmentType.ToLower().Contains(keyword)) ||
                    (x.Position != null && x.Position.PositionName.ToLower().Contains(keyword)) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && (x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword) || x.WorkforceProfile.DisplayName.ToLower().Contains(keyword))));
            }

            var totalData = await query.CountAsync(ct);
            var sortedQuery = ApplyPositionSorting(query, sortBy, sortDirection);
            var entities = await sortedQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            var actorIds = entities.SelectMany(x => new[] { x.CreateBy, x.UpdateBy }).Where(id => id != Guid.Empty).Distinct().ToList();
            var actorNames = await GetActorNamesAsync(actorIds, ct);

            var items = entities.Select(x => new WfpPositionAssignmentResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                OrganizationAssignmentId = x.OrganizationAssignmentId,
                LegalEntityId = x.LegalEntityId,
                HospitalSiteId = x.HospitalSiteId,
                OrganizationUnitId = x.OrganizationUnitId,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department?.DepartmentName,
                PositionId = x.PositionId,
                PositionName = x.Position?.PositionName ?? string.Empty,
                JobFamilyId = x.JobFamilyId,
                JobLevelId = x.JobLevelId,
                EmployeeGradeId = x.EmployeeGradeId,
                AssignmentType = x.AssignmentType,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                IsPrimary = x.IsPrimary,
                IsActing = x.IsActing,
                IsActive = x.IsActive,
                Description = x.Description,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetActorName(actorNames, x.CreateBy),
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                UpdateByName = GetActorName(actorNames, x.UpdateBy)
            }).ToList();

            return new PagedResult<WfpPositionAssignmentResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        private static IOrderedQueryable<WfpPositionAssignment> ApplyPositionSorting(
            IQueryable<WfpPositionAssignment> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "effectiveStartDate").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "assignmenttype" => desc ? query.OrderByDescending(x => x.AssignmentType) : query.OrderBy(x => x.AssignmentType),
                "isprimary" => desc ? query.OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate),
                "isacting" => desc ? query.OrderByDescending(x => x.IsActing).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsActing).ThenByDescending(x => x.EffectiveStartDate),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate),
                _ => desc ? query.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.EffectiveStartDate).ThenBy(x => x.CreateDateTime)
            };
        }

        #endregion

        #region 3. Manager Assignments (Relasi Atasan)

        public async Task<PagedResult<WfpManagerAssignmentResponse>> GetManagerAssignmentsAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod,
            Guid? managerProfileId,
            Guid? workforceProfileId,
            Guid? departmentId,
            string? managerType,
            bool? isPrimaryManager,
            bool? canApproveRequests,
            bool? isActive,
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            var query = _dbContext.Set<WfpManagerAssignment>().AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.ManagerWorkforceProfile)
                .Include(x => x.Department)
                .Include(x => x.ManagerPosition)
                .Where(x => !x.IsDelete);

            if (dateRange.Start.HasValue) query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            if (dateRange.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            if (managerProfileId.HasValue && managerProfileId.Value != Guid.Empty) query = query.Where(x => x.ManagerWorkforceProfileId == managerProfileId.Value);
            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty) query = query.Where(x => x.WorkforceProfileId == workforceProfileId.Value);
            if (departmentId.HasValue && departmentId.Value != Guid.Empty) query = query.Where(x => x.DepartmentId == departmentId.Value);
            if (!string.IsNullOrWhiteSpace(managerType)) query = query.Where(x => x.ManagerType.ToLower() == managerType.Trim().ToLower());
            if (isPrimaryManager.HasValue) query = query.Where(x => x.IsPrimaryManager == isPrimaryManager.Value);
            if (canApproveRequests.HasValue) query = query.Where(x => x.CanApproveRequests == canApproveRequests.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.ManagerType != null && x.ManagerType.ToLower().Contains(keyword)) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.ManagerPosition != null && x.ManagerPosition.PositionName.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && (x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword) || x.WorkforceProfile.DisplayName.ToLower().Contains(keyword))) ||
                    (x.ManagerWorkforceProfile != null && (x.ManagerWorkforceProfile.ProfileCode.ToLower().Contains(keyword) || x.ManagerWorkforceProfile.DisplayName.ToLower().Contains(keyword))));
            }

            var totalData = await query.CountAsync(ct);
            var sortedQuery = ApplyManagerSorting(query, sortBy, sortDirection);
            var entities = await sortedQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            var actorIds = entities.SelectMany(x => new[] { x.CreateBy, x.UpdateBy }).Where(id => id != Guid.Empty).Distinct().ToList();
            var actorNames = await GetActorNamesAsync(actorIds, ct);

            var items = entities.Select(x => new WfpManagerAssignmentResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                ManagerWorkforceProfileId = x.ManagerWorkforceProfileId,
                ManagerProfileCode = x.ManagerWorkforceProfile?.ProfileCode ?? string.Empty,
                ManagerDisplayName = x.ManagerWorkforceProfile?.DisplayName ?? string.Empty,
                OrganizationUnitId = x.OrganizationUnitId,
                DepartmentId = x.DepartmentId,
                DepartmentName = x.Department?.DepartmentName,
                ManagerPositionId = x.ManagerPositionId,
                ManagerPositionName = x.ManagerPosition?.PositionName,
                ManagerType = x.ManagerType,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                IsPrimaryManager = x.IsPrimaryManager,
                CanApproveRequests = x.CanApproveRequests,
                IsActive = x.IsActive,
                Description = x.Description,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetActorName(actorNames, x.CreateBy),
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                UpdateByName = GetActorName(actorNames, x.UpdateBy)
            }).ToList();

            return new PagedResult<WfpManagerAssignmentResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        private static IOrderedQueryable<WfpManagerAssignment> ApplyManagerSorting(
            IQueryable<WfpManagerAssignment> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "effectiveStartDate").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "managertype" => desc ? query.OrderByDescending(x => x.ManagerType) : query.OrderBy(x => x.ManagerType),
                "isprimarymanager" => desc ? query.OrderByDescending(x => x.IsPrimaryManager).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsPrimaryManager).ThenByDescending(x => x.EffectiveStartDate),
                "canapproverequests" => desc ? query.OrderByDescending(x => x.CanApproveRequests).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.CanApproveRequests).ThenByDescending(x => x.EffectiveStartDate),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate),
                _ => desc ? query.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.EffectiveStartDate).ThenBy(x => x.CreateDateTime)
            };
        }

        #endregion

        #region 4. Employment Histories (Riwayat Kepegawaian)

        public async Task<PagedResult<WfpEmploymentHistoryResponse>> GetEmploymentHistoriesAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod,
            Guid? workforceProfileId,
            string? historyType,
            Guid? departmentId,
            Guid? positionId,
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            var query = _dbContext.Set<WfpEmploymentHistory>().AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.OldDepartment)
                .Include(x => x.NewDepartment)
                .Include(x => x.OldPosition)
                .Include(x => x.NewPosition)
                .Include(x => x.OldEmploymentStatus)
                .Include(x => x.NewEmploymentStatus)
                .Include(x => x.OldEmploymentType)
                .Include(x => x.NewEmploymentType)
                .Where(x => !x.IsDelete);

            if (dateRange.Start.HasValue) query = query.Where(x => x.EffectiveDate >= dateRange.Start.Value);
            if (dateRange.EndExclusive.HasValue) query = query.Where(x => x.EffectiveDate < dateRange.EndExclusive.Value);
            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty) query = query.Where(x => x.WorkforceProfileId == workforceProfileId.Value);
            if (!string.IsNullOrWhiteSpace(historyType)) query = query.Where(x => x.HistoryType.ToLower() == historyType.Trim().ToLower());
            if (departmentId.HasValue && departmentId.Value != Guid.Empty) query = query.Where(x => x.OldDepartmentId == departmentId.Value || x.NewDepartmentId == departmentId.Value);
            if (positionId.HasValue && positionId.Value != Guid.Empty) query = query.Where(x => x.OldPositionId == positionId.Value || x.NewPositionId == positionId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.HistoryType != null && x.HistoryType.ToLower().Contains(keyword)) ||
                    (x.Reason != null && x.Reason.ToLower().Contains(keyword)) ||
                    (x.OldStatus != null && x.OldStatus.ToLower().Contains(keyword)) ||
                    (x.NewStatus != null && x.NewStatus.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && (x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword) || x.WorkforceProfile.DisplayName.ToLower().Contains(keyword))));
            }

            var totalData = await query.CountAsync(ct);
            var sortedQuery = ApplyEmploymentHistorySorting(query, sortBy, sortDirection);
            var entities = await sortedQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            var actorIds = entities.SelectMany(x => new[] { x.CreateBy, x.UpdateBy, x.ApprovedByUserId ?? Guid.Empty }).Where(id => id != Guid.Empty).Distinct().ToList();
            var actorNames = await GetActorNamesAsync(actorIds, ct);

            var items = entities.Select(x => new WfpEmploymentHistoryResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                HistoryType = x.HistoryType,
                OldStatus = x.OldStatus,
                NewStatus = x.NewStatus,
                OldEmploymentStatusId = x.OldEmploymentStatusId,
                OldEmploymentStatusName = x.OldEmploymentStatus?.EmploymentStatusName,
                NewEmploymentStatusId = x.NewEmploymentStatusId,
                NewEmploymentStatusName = x.NewEmploymentStatus?.EmploymentStatusName,
                OldEmploymentTypeId = x.OldEmploymentTypeId,
                OldEmploymentTypeName = x.OldEmploymentType?.EmploymentTypeName,
                NewEmploymentTypeId = x.NewEmploymentTypeId,
                NewEmploymentTypeName = x.NewEmploymentType?.EmploymentTypeName,
                OldDepartmentId = x.OldDepartmentId,
                OldDepartmentName = x.OldDepartment?.DepartmentName,
                NewDepartmentId = x.NewDepartmentId,
                NewDepartmentName = x.NewDepartment?.DepartmentName,
                OldPositionId = x.OldPositionId,
                OldPositionName = x.OldPosition?.PositionName,
                NewPositionId = x.NewPositionId,
                NewPositionName = x.NewPosition?.PositionName,
                OldOrganizationUnitId = x.OldOrganizationUnitId,
                NewOrganizationUnitId = x.NewOrganizationUnitId,
                OldEmployeeGradeId = x.OldEmployeeGradeId,
                NewEmployeeGradeId = x.NewEmployeeGradeId,
                EffectiveDate = x.EffectiveDate,
                EndDate = x.EndDate,
                Reason = x.Reason,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedByUserName = x.ApprovedByUserId.HasValue ? GetActorName(actorNames, x.ApprovedByUserId.Value) : null,
                ApprovedAt = x.ApprovedAt,
                IsActive = x.IsActive,
                Description = x.Description,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetActorName(actorNames, x.CreateBy),
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                UpdateByName = GetActorName(actorNames, x.UpdateBy)
            }).ToList();

            return new PagedResult<WfpEmploymentHistoryResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        private static IOrderedQueryable<WfpEmploymentHistory> ApplyEmploymentHistorySorting(
            IQueryable<WfpEmploymentHistory> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "effectiveDate").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "historytype" => desc ? query.OrderByDescending(x => x.HistoryType) : query.OrderBy(x => x.HistoryType),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.EffectiveDate) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.EffectiveDate),
                _ => desc ? query.OrderByDescending(x => x.EffectiveDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.EffectiveDate).ThenBy(x => x.CreateDateTime)
            };
        }

        #endregion

        #region 5. Salary Assignments (Penetapan Gaji Lintas Seluruh Pegawai - Nominal Dirahasiakan)

        /// <summary>
        /// Mengambil rekapitulasi penetapan gaji lintas seluruh pegawai.
        /// [DECISION] HRD-DEC-033: Nominal BaseSalary sengaja DITIADAKAN dari response ini demi keamanan dan kepatuhan privasi data gaji.
        /// </summary>
        public async Task<PagedResult<WfpSalaryAssignmentCrossEmployeeResponse>> GetSalaryAssignmentsAsync(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod,
            Guid? workforceProfileId,
            Guid? salaryStructureId,
            Guid? salaryGradeId,
            string? paymentFrequency,
            string? currencyCode,
            bool? isPrimary,
            bool? isApproved,
            bool? isActive,
            string? search,
            string? sortBy,
            string? sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            var query = _dbContext.Set<WfpSalaryAssignment>().AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.SalaryStructure)
                .Include(x => x.SalaryGrade)
                .Include(x => x.EmployeeGrade)
                .Include(x => x.PayrollPeriod)
                .Where(x => !x.IsDelete);

            if (dateRange.Start.HasValue) query = query.Where(x => x.EffectiveStartDate >= dateRange.Start.Value);
            if (dateRange.EndExclusive.HasValue) query = query.Where(x => x.EffectiveStartDate < dateRange.EndExclusive.Value);
            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty) query = query.Where(x => x.WorkforceProfileId == workforceProfileId.Value);
            if (salaryStructureId.HasValue && salaryStructureId.Value != Guid.Empty) query = query.Where(x => x.SalaryStructureId == salaryStructureId.Value);
            if (salaryGradeId.HasValue && salaryGradeId.Value != Guid.Empty) query = query.Where(x => x.SalaryGradeId == salaryGradeId.Value);
            if (!string.IsNullOrWhiteSpace(paymentFrequency)) query = query.Where(x => x.PaymentFrequency.ToLower() == paymentFrequency.Trim().ToLower());
            if (!string.IsNullOrWhiteSpace(currencyCode)) query = query.Where(x => x.CurrencyCode.ToUpper() == currencyCode.Trim().ToUpper());
            if (isPrimary.HasValue) query = query.Where(x => x.IsPrimary == isPrimary.Value);
            if (isApproved.HasValue)
            {
                query = isApproved.Value
                    ? query.Where(x => x.ApprovedAt.HasValue && x.ApprovedByUserId.HasValue)
                    : query.Where(x => !x.ApprovedAt.HasValue || !x.ApprovedByUserId.HasValue);
            }
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.CurrencyCode.ToLower().Contains(keyword) ||
                    x.PaymentFrequency.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.SalaryStructure != null && x.SalaryStructure.SalaryStructureName.ToLower().Contains(keyword)) ||
                    (x.SalaryGrade != null && x.SalaryGrade.SalaryGradeName.ToLower().Contains(keyword)) ||
                    (x.WorkforceProfile != null && (x.WorkforceProfile.ProfileCode.ToLower().Contains(keyword) || x.WorkforceProfile.DisplayName.ToLower().Contains(keyword))));
            }

            var totalData = await query.CountAsync(ct);
            var sortedQuery = ApplySalarySorting(query, sortBy, sortDirection);
            var entities = await sortedQuery.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            var actorIds = entities.SelectMany(x => new[] { x.CreateBy, x.UpdateBy, x.ApprovedByUserId ?? Guid.Empty }).Where(id => id != Guid.Empty).Distinct().ToList();
            var actorNames = await GetActorNamesAsync(actorIds, ct);

            // Mappings explicitly OMIT BaseSalary (HRD-DEC-033)
            var items = entities.Select(x => new WfpSalaryAssignmentCrossEmployeeResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                SalaryStructureId = x.SalaryStructureId,
                SalaryStructureName = x.SalaryStructure?.SalaryStructureName ?? string.Empty,
                SalaryGradeId = x.SalaryGradeId,
                SalaryGradeName = x.SalaryGrade?.SalaryGradeName ?? string.Empty,
                EmployeeGradeId = x.EmployeeGradeId,
                EmployeeGradeName = x.EmployeeGrade?.GradeName,
                PayrollPeriodId = x.PayrollPeriodId,
                PayrollPeriodName = x.PayrollPeriod?.PayrollPeriodName,
                CurrencyCode = x.CurrencyCode,
                PaymentFrequency = x.PaymentFrequency,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                IsPrimary = x.IsPrimary,
                IsConfidential = x.IsConfidential,
                IsActive = x.IsActive,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedByUserName = x.ApprovedByUserId.HasValue ? GetActorName(actorNames, x.ApprovedByUserId.Value) : null,
                ApprovedAt = x.ApprovedAt,
                IsApproved = x.ApprovedAt.HasValue && x.ApprovedByUserId.HasValue,
                Description = x.Description,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetActorName(actorNames, x.CreateBy),
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                UpdateByName = GetActorName(actorNames, x.UpdateBy)
            }).ToList();

            return new PagedResult<WfpSalaryAssignmentCrossEmployeeResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };
        }

        private static IOrderedQueryable<WfpSalaryAssignment> ApplySalarySorting(
            IQueryable<WfpSalaryAssignment> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "effectiveStartDate").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "paymentfrequency" => desc ? query.OrderByDescending(x => x.PaymentFrequency) : query.OrderBy(x => x.PaymentFrequency),
                "isprimary" => desc ? query.OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate),
                "approvedat" => desc ? query.OrderByDescending(x => x.ApprovedAt) : query.OrderBy(x => x.ApprovedAt),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate),
                _ => desc ? query.OrderByDescending(x => x.EffectiveStartDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.EffectiveStartDate).ThenBy(x => x.CreateDateTime)
            };
        }

        #endregion

        #region 6. Specific Salary Amount (ViewAmount Authorized Only)

        /// <summary>
        /// Mengambil nominal penetapan gaji spesifik untuk satu record.
        /// [DECISION] HRD-DEC-033: Memerlukan wewenang sensitif WfpSalaryAssignment:ViewAmount.
        /// </summary>
        public async Task<WfpSalaryAmountResponse?> GetSalaryAmountByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _dbContext.Set<WfpSalaryAssignment>().AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, ct);

            if (entity == null) return null;

            return new WfpSalaryAmountResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                WorkforceProfileCode = entity.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = entity.WorkforceProfile?.DisplayName ?? string.Empty,
                BaseSalary = entity.BaseSalary,
                CurrencyCode = entity.CurrencyCode,
                PaymentFrequency = entity.PaymentFrequency,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate
            };
        }

        #endregion

        #region Helper Methods

        private static void NormalizePaging(ref int pageNumber, ref int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100);
        }

        private static (DateTime? Start, DateTime? EndExclusive) ResolveDateRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            if (!string.IsNullOrWhiteSpace(customPeriod) && !customPeriod.Equals("custom", StringComparison.OrdinalIgnoreCase))
            {
                var today = DateTime.UtcNow.Date;
                return customPeriod.Trim().ToLowerInvariant() switch
                {
                    "today" => (today, today.AddDays(1)),
                    "last7days" => (today.AddDays(-6), today.AddDays(1)),
                    "last30days" => (today.AddDays(-29), today.AddDays(1)),
                    "thismonth" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                    _ => (null, null)
                };
            }
            DateTime? start = startDate.HasValue ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc) : null;
            DateTime? endExclusive = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc) : null;
            return (start, endExclusive);
        }

        private async Task<Dictionary<Guid, string?>> GetActorNamesAsync(IEnumerable<Guid> ids, CancellationToken ct)
        {
            var actorIds = ids.Where(x => x != Guid.Empty).Distinct().ToList();
            if (!actorIds.Any()) return new Dictionary<Guid, string?>();
            return await _dbContext.Users.AsNoTracking().Where(x => actorIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => (string?)x.Name, ct);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> map, Guid id) =>
            id == Guid.Empty ? null : map.TryGetValue(id, out var name) ? name : null;

        #endregion
    }
}
