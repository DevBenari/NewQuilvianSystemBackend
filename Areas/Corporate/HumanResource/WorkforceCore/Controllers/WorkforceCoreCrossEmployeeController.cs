using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-core")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Core Cross-Employee Administration",
        AreaName = "Corporate",
        ControllerName = "WorkforceCoreCrossEmployee",
        Description = "Cross-employee administration listings for workforce assignments and histories",
        SortOrder = 25)]
    [Tags("Corporate / Human Resource / Workforce Core / Cross-Employee Administration")]
    public class WorkforceCoreCrossEmployeeController : ControllerBase
    {
        private readonly WorkforceCoreCrossEmployeeQueryService _queryService;

        public WorkforceCoreCrossEmployeeController(WorkforceCoreCrossEmployeeQueryService queryService)
        {
            _queryService = queryService;
        }

        /// <summary>
        /// Mengambil daftar rekapitulasi penempatan organisasi lintas seluruh pegawai.
        /// [DECISION] HRD-DEC-012.
        /// </summary>
        [HttpGet("organization-assignments")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpOrganizationAssignmentResponse>>), StatusCodes.Status200OK)]
        [AccessAction("ReadAll", "Read All Workforce Organization Assignments", Description = "Melihat rekapitulasi penempatan organisasi lintas seluruh pegawai", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WfpOrganizationAssignment", "ReadAll")]
        public async Task<IActionResult> GetOrganizationAssignments(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] Guid? organizationUnitId,
            [FromQuery] Guid? workforceProfileId,
            [FromQuery] string? assignmentType,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isManagerialAssignment,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "effectiveStartDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken ct = default)
        {
            var result = await _queryService.GetOrganizationAssignmentsAsync(
                startDate, endDate, customPeriod, departmentId, positionId, organizationUnitId, workforceProfileId,
                assignmentType, isPrimary, isManagerialAssignment, isActive, search, sortBy, sortDirection, pageNumber, pageSize, ct);

            return Ok(ApiResponse<PagedResult<WfpOrganizationAssignmentResponse>>.Ok(result, "Data rekapitulasi penempatan organisasi berhasil diambil."));
        }

        /// <summary>
        /// Mengambil daftar rekapitulasi penempatan jabatan lintas seluruh pegawai.
        /// [DECISION] HRD-DEC-012.
        /// </summary>
        [HttpGet("position-assignments")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpPositionAssignmentResponse>>), StatusCodes.Status200OK)]
        [AccessAction("ReadAll", "Read All Workforce Position Assignments", Description = "Melihat rekapitulasi penempatan jabatan lintas seluruh pegawai", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("WfpPositionAssignment", "ReadAll")]
        public async Task<IActionResult> GetPositionAssignments(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? positionId,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? jobLevelId,
            [FromQuery] Guid? workforceProfileId,
            [FromQuery] string? assignmentType,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isActing,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "effectiveStartDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken ct = default)
        {
            var result = await _queryService.GetPositionAssignmentsAsync(
                startDate, endDate, customPeriod, positionId, departmentId, jobLevelId, workforceProfileId,
                assignmentType, isPrimary, isActing, isActive, search, sortBy, sortDirection, pageNumber, pageSize, ct);

            return Ok(ApiResponse<PagedResult<WfpPositionAssignmentResponse>>.Ok(result, "Data rekapitulasi penempatan jabatan berhasil diambil."));
        }

        /// <summary>
        /// Mengambil daftar rekapitulasi relasi atasan lintas seluruh pegawai.
        /// [DECISION] HRD-DEC-012.
        /// </summary>
        [HttpGet("manager-assignments")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpManagerAssignmentResponse>>), StatusCodes.Status200OK)]
        [AccessAction("ReadAll", "Read All Workforce Manager Assignments", Description = "Melihat rekapitulasi relasi atasan lintas seluruh pegawai", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("WfpManagerAssignment", "ReadAll")]
        public async Task<IActionResult> GetManagerAssignments(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? managerProfileId,
            [FromQuery] Guid? workforceProfileId,
            [FromQuery] Guid? departmentId,
            [FromQuery] string? managerType,
            [FromQuery] bool? isPrimaryManager,
            [FromQuery] bool? canApproveRequests,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "effectiveStartDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken ct = default)
        {
            var result = await _queryService.GetManagerAssignmentsAsync(
                startDate, endDate, customPeriod, managerProfileId, workforceProfileId, departmentId,
                managerType, isPrimaryManager, canApproveRequests, isActive, search, sortBy, sortDirection, pageNumber, pageSize, ct);

            return Ok(ApiResponse<PagedResult<WfpManagerAssignmentResponse>>.Ok(result, "Data rekapitulasi relasi atasan berhasil diambil."));
        }

        /// <summary>
        /// Mengambil daftar rekapitulasi riwayat kepegawaian lintas seluruh pegawai.
        /// [DECISION] HRD-DEC-012.
        /// </summary>
        [HttpGet("employment-histories")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpEmploymentHistoryResponse>>), StatusCodes.Status200OK)]
        [AccessAction("ReadAll", "Read All Workforce Employment Histories", Description = "Melihat rekapitulasi riwayat kepegawaian lintas seluruh pegawai", AccessType = AccessTypes.Read, SortOrder = 4)]
        [AccessPermission("WfpEmploymentHistory", "ReadAll")]
        public async Task<IActionResult> GetEmploymentHistories(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? workforceProfileId,
            [FromQuery] string? historyType,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "effectiveDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken ct = default)
        {
            var result = await _queryService.GetEmploymentHistoriesAsync(
                startDate, endDate, customPeriod, workforceProfileId, historyType, departmentId, positionId,
                search, sortBy, sortDirection, pageNumber, pageSize, ct);

            return Ok(ApiResponse<PagedResult<WfpEmploymentHistoryResponse>>.Ok(result, "Data rekapitulasi riwayat kepegawaian berhasil diambil."));
        }

        /// <summary>
        /// Mengambil daftar rekapitulasi penetapan gaji lintas seluruh pegawai.
        /// [DECISION] HRD-DEC-012 & HRD-DEC-033: Nominal gaji (BaseSalary) sengaja disembunyikan/tidak disertakan pada response ini.
        /// </summary>
        [HttpGet("salary-assignments")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpSalaryAssignmentCrossEmployeeResponse>>), StatusCodes.Status200OK)]
        [AccessAction("ReadAll", "Read All Workforce Salary Assignments", Description = "Melihat rekapitulasi penetapan gaji lintas seluruh pegawai tanpa nominal (HRD-DEC-033)", AccessType = AccessTypes.Read, SortOrder = 5)]
        [AccessPermission("WfpSalaryAssignment", "ReadAll")]
        public async Task<IActionResult> GetSalaryAssignments(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? workforceProfileId,
            [FromQuery] Guid? salaryStructureId,
            [FromQuery] Guid? salaryGradeId,
            [FromQuery] string? paymentFrequency,
            [FromQuery] string? currencyCode,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isApproved,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "effectiveStartDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken ct = default)
        {
            var result = await _queryService.GetSalaryAssignmentsAsync(
                startDate, endDate, customPeriod, workforceProfileId, salaryStructureId, salaryGradeId,
                paymentFrequency, currencyCode, isPrimary, isApproved, isActive, search, sortBy, sortDirection, pageNumber, pageSize, ct);

            return Ok(ApiResponse<PagedResult<WfpSalaryAssignmentCrossEmployeeResponse>>.Ok(result, "Data rekapitulasi penetapan gaji berhasil diambil."));
        }

        /// <summary>
        /// Mengambil nominal penetapan gaji spesifik satu record (hanya bagi pengguna dengan wewenang sensitif ViewAmount).
        /// [DECISION] HRD-DEC-033.
        /// </summary>
        [HttpGet("salary-assignments/{id:guid}/amount")]
        [ProducesResponseType(typeof(ApiResponse<WfpSalaryAmountResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("ViewAmount", "View Salary Assignment Amount", Description = "Melihat nominal gaji spesifik satu penetapan (butir sensitif sesuai HRD-DEC-033)", AccessType = AccessTypes.Read, SortOrder = 6)]
        [AccessPermission("WfpSalaryAssignment", "ViewAmount")]
        public async Task<IActionResult> GetSalaryAmount(Guid id, CancellationToken ct)
        {
            var result = await _queryService.GetSalaryAmountByIdAsync(id, ct);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(404, "Data penetapan gaji tidak ditemukan."));
            }

            return Ok(ApiResponse<WfpSalaryAmountResponse>.Ok(result, "Nominal penetapan gaji berhasil diambil."));
        }
    }
}
