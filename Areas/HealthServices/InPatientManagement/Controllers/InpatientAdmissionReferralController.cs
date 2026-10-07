using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers
{
    /// <summary>
    /// Daftar permintaan admisi dari kamar pulih untuk petugas admisi (<c>BE-RWI-181</c>, API 11.6).
    /// </summary>
    /// <remarks>
    /// Membuat dan membatalkan permintaan <b>tidak</b> punya endpoint: keduanya dipanggil service
    /// Kamar Operasi dalam proses yang sama saat keputusan kamar pulih disimpan (<c>RWI-DEC-201</c>).
    /// Admisi dari permintaan memakai <c>POST episodes</c> dengan <c>AdmissionReferralId</c>.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/inpatient-management/admission-referrals")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_INPATIENT",
        moduleName: "Health Service Inpatient",
        displayName: "Inpatient Admission Referral",
        AreaName = "HealthServices",
        ControllerName = "InpatientAdmissionReferral",
        Description = "Permintaan admisi rawat inap dari keputusan kamar pulih Kamar Operasi",
        SortOrder = 15
    )]
    [Tags("Health Services / Inpatient Management / Inpatient Admission Referral")]
    public class InpatientAdmissionReferralController : ControllerBase
    {
        private readonly InpAdmissionReferralService _service;

        public InpatientAdmissionReferralController(InpAdmissionReferralService service)
        {
            _service = service;
        }

        /// <summary>Daftar permintaan admisi; bawaan status <c>Pending</c>, terlama lebih dulu.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<AdmissionReferralResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Admission Referral", Description = "Melihat permintaan admisi dari kamar pulih", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientAdmissionReferral", "Read")]
        public async Task<IActionResult> GetPaged([FromQuery] AdmissionReferralQuery request,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<PagedResult<AdmissionReferralResponse>>.Ok(result,
                "Daftar permintaan admisi berhasil diambil."));
        }

        /// <summary>Detail satu permintaan, untuk mengisi awal admisi berlangkah.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AdmissionReferralResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Inpatient Admission Referral", Description = "Melihat detail permintaan admisi dari kamar pulih", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientAdmissionReferral", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Permintaan admisi tidak ditemukan."))
                : Ok(ApiResponse<AdmissionReferralResponse>.Ok(result, "Detail permintaan admisi berhasil diambil."));
        }
    }
}
