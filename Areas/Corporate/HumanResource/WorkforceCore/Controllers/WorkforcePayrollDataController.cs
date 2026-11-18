using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    /// <summary>
    /// Data penggajian karyawan, perawat, dan dokter yang dikelola HRD: kelengkapan penetapan gaji utama dan rekening payroll, serta
    /// ubah per orang. Daftar bersumber dari Master Karyawan dan Master Dokter yang aktif. Modul Payroll hanya membaca data ini.
    /// Detail memuat gaji pokok dan nomor rekening lengkap, sehingga hanya pemegang hak ubah yang dapat memuatnya. Menyetujui
    /// penetapan gaji dan memverifikasi rekening lewat endpoint ini hanya diterima bila pemanggil juga berwenang atas aksi itu pada
    /// endpoint HRD (WfpSalaryAssignment dan WorkforceBankAccount : Update). Seluruh akses data lewat
    /// <see cref="WorkforcePayrollDataService"/>.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-core/payroll-data")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Payroll Data",
        AreaName = "Corporate",
        ControllerName = "WorkforcePayrollData",
        Description = "Kelengkapan dan perubahan penetapan gaji utama serta rekening payroll karyawan, perawat, dan dokter",
        SortOrder = 26)]
    [Tags("Corporate / Human Resource / Workforce Core / Payroll Data")]
    public class WorkforcePayrollDataController : ControllerBase
    {
        private const string BankAccountResource = "WorkforceBankAccount";
        private const string SalaryAssignmentResource = "WfpSalaryAssignment";

        private readonly WorkforcePayrollDataService _service;
        private readonly AccessPermissionService _accessPermissionService;

        public WorkforcePayrollDataController(WorkforcePayrollDataService service, AccessPermissionService accessPermissionService)
        {
            _service = service;
            _accessPermissionService = accessPermissionService;
        }

        private Guid CurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePayrollDataSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Payroll Data", Description = "Melihat kelengkapan penetapan gaji utama dan rekening payroll karyawan, perawat, dan dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforcePayrollData", "Read")]
        public async Task<IActionResult> Summary(CancellationToken cancellationToken)
        {
            var result = await _service.SummaryAsync(cancellationToken);
            return Ok(ApiResponse<WorkforcePayrollDataSummaryResponse>.Ok(result, "Ringkasan data penggajian berhasil dimuat."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WorkforcePayrollDataItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Payroll Data", Description = "Melihat kelengkapan penetapan gaji utama dan rekening payroll karyawan, perawat, dan dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforcePayrollData", "Read")]
        public async Task<IActionResult> List([FromQuery] WorkforcePayrollDataQuery query, CancellationToken cancellationToken)
        {
            var result = await _service.ListAsync(query, cancellationToken);
            return Ok(ApiResponse<PagedResult<WorkforcePayrollDataItemResponse>>.Ok(result, "Data penggajian berhasil dimuat."));
        }

        [HttpGet("{workforceProfileId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePayrollDataDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Payroll Data", Description = "Memuat dan mengubah penetapan gaji utama dan rekening payroll satu orang", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("WorkforcePayrollData", "Update")]
        public async Task<IActionResult> Detail(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.GetDetailAsync(workforceProfileId, cancellationToken);
                return Ok(ApiResponse<WorkforcePayrollDataDetailResponse>.Ok(result, "Detail data penggajian berhasil dimuat."));
            }
            catch (WorkforcePayrollDataException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message));
            }
        }

        [HttpPut("{workforceProfileId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePayrollDataSaveResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Payroll Data", Description = "Memuat dan mengubah penetapan gaji utama dan rekening payroll satu orang", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("WorkforcePayrollData", "Update")]
        public async Task<IActionResult> Save(
            Guid workforceProfileId,
            [FromBody] UpdateWorkforcePayrollDataRequest request,
            CancellationToken cancellationToken)
        {
            if (request.MarkBankAccountVerified && !await _accessPermissionService.HasAccessAsync(User, BankAccountResource, "Update"))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, "Anda tidak berwenang memverifikasi rekening bank."));
            }

            if (request.ApproveSalaryAssignment && !await _accessPermissionService.HasAccessAsync(User, SalaryAssignmentResource, "Update"))
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, "Anda tidak berwenang menyetujui penetapan gaji."));
            }

            try
            {
                var result = await _service.SaveAsync(workforceProfileId, request, CurrentUserId(), cancellationToken);
                return Ok(ApiResponse<WorkforcePayrollDataSaveResponse>.Ok(result, "Data penggajian berhasil disimpan."));
            }
            catch (WorkforcePayrollDataException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message));
            }
        }
    }
}
