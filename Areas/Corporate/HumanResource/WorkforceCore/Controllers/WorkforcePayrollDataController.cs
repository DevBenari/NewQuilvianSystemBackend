using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    /// <summary>
    /// Monitoring READ-ONLY Data Penggajian Pegawai (HRD): menampilkan kelengkapan penetapan gaji utama dan rekening payroll
    /// karyawan, perawat, dan dokter aktif yang bersumber dari Master Karyawan dan Master Dokter.
    ///
    /// Controller ini tidak menyediakan operasi Create, Update, atau Delete. Pengelolaan penetapan gaji, persetujuan gaji,
    /// pendaftaran rekening, dan verifikasi rekening hanya dilakukan melalui endpoint master HRD (WfpSalaryAssignmentController
    /// dan WfpBankAccountController).
    ///
    /// Nominal gaji pokok hanya dikembalikan kepada pemegang izin ViewAmount; pengguna dengan izin Read saja tetap dapat memantau
    /// status dan kelengkapan dengan nominal bernilai null. Nomor rekening selalu disamarkan.
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
        Description = "Monitoring read-only penetapan gaji utama dan rekening payroll karyawan, perawat, dan dokter dari Master HRD",
        SortOrder = 26)]
    [Tags("Corporate / Human Resource / Workforce Core / Payroll Data")]
    public class WorkforcePayrollDataController : ControllerBase
    {
        private const string Resource = "WorkforcePayrollData";

        private readonly WorkforcePayrollDataService _service;
        private readonly AccessPermissionService _accessPermissionService;

        public WorkforcePayrollDataController(WorkforcePayrollDataService service, AccessPermissionService accessPermissionService)
        {
            _service = service;
            _accessPermissionService = accessPermissionService;
        }

        private Task<bool> CanViewAmountAsync() => _accessPermissionService.HasAccessAsync(User, Resource, "ViewAmount");

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WorkforcePayrollDataItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Payroll Data", Description = "Melihat monitoring penetapan gaji utama dan rekening payroll karyawan, perawat, dan dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforcePayrollData", "Read")]
        public async Task<IActionResult> List([FromQuery] WorkforcePayrollDataQuery query, CancellationToken cancellationToken)
        {
            var includeAmounts = await CanViewAmountAsync();
            var result = await _service.ListAsync(query, includeAmounts, cancellationToken);
            return Ok(ApiResponse<PagedResult<WorkforcePayrollDataItemResponse>>.Ok(result, "Data penggajian berhasil dimuat."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePayrollDataSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Payroll Data", Description = "Melihat ringkasan monitoring data penggajian karyawan, perawat, dan dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforcePayrollData", "Read")]
        public async Task<IActionResult> Summary(CancellationToken cancellationToken)
        {
            var result = await _service.SummaryAsync(cancellationToken);
            return Ok(ApiResponse<WorkforcePayrollDataSummaryResponse>.Ok(result, "Ringkasan data penggajian berhasil dimuat."));
        }

        /// <summary>
        /// Rincian read-only data penggajian satu orang. Nominal gaji hanya dikirim bagi pemegang izin ViewAmount; nomor rekening
        /// selalu disamarkan.
        /// </summary>
        [HttpGet("{workforceProfileId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePayrollDataDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Payroll Data", Description = "Melihat rincian read-only data penggajian satu orang", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforcePayrollData", "Read")]
        public async Task<IActionResult> Detail(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            try
            {
                var includeAmounts = await CanViewAmountAsync();
                var result = await _service.GetDetailAsync(workforceProfileId, includeAmounts, cancellationToken);
                return Ok(ApiResponse<WorkforcePayrollDataDetailResponse>.Ok(result, "Detail data penggajian berhasil dimuat."));
            }
            catch (WorkforcePayrollDataException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message));
            }
        }

        /// <summary>
        /// Pintu izin nominal (pola ViewAmount existing): mendaftarkan aksi WorkforcePayrollData : ViewAmount pada layar Akses Role.
        /// Pemegang izin ini melihat nominal gaji pokok pada daftar, detail, dan endpoint ini; tanpa izin, nominal bernilai null.
        /// </summary>
        [HttpGet("{workforceProfileId:guid}/amounts")]
        [ProducesResponseType(typeof(ApiResponse<WorkforcePayrollDataDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("ViewAmount", "View Workforce Payroll Data Amount", Description = "Melihat nominal gaji pokok pada monitoring data penggajian pegawai", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("WorkforcePayrollData", "ViewAmount")]
        public async Task<IActionResult> Amounts(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _service.GetDetailAsync(workforceProfileId, includeAmounts: true, cancellationToken);
                return Ok(ApiResponse<WorkforcePayrollDataDetailResponse>.Ok(result, "Nominal data penggajian berhasil dimuat."));
            }
            catch (WorkforcePayrollDataException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message));
            }
        }
    }
}
