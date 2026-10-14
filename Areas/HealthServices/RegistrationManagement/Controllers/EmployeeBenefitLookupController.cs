using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Controllers
{
    /// <summary>
    /// Pencarian pegawai dan pemeriksaan kelayakan penjamin internal dari data HR (BE-FIN-106;
    /// FIN-API-1.10 R.3; INT-REG-HR-001). Read-only (transaction-endpoint-standard.md 2.4): tidak ada
    /// POST, PUT, PATCH, DELETE, maupun GET /options. Hak akses: EmployeeBenefitLookup : Read.
    /// Saldo plafon tidak pernah dikembalikan ke Registrasi.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/registration-management/employee-benefit-lookups")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_REGISTRATION_MANAGEMENT",
        moduleName: "Health Service Registration Management",
        displayName: "Employee Benefit Lookup",
        AreaName = "HealthServices",
        ControllerName = "EmployeeBenefitLookup",
        Description = "Pencarian pegawai dan pemeriksaan kelayakan penjamin internal dari data HR",
        SortOrder = 8
    )]
    [Tags("Health Services / Registration Management / Employee Benefit Lookup")]
    public sealed class EmployeeBenefitLookupController : ControllerBase
    {
        private const string TooShortKeywordMessage = "Ketik minimal 3 karakter untuk mencari pegawai.";
        private const string TooManyResultsMessage = "Hasil terlalu banyak. Ketik nama atau NIP yang lebih spesifik.";

        private readonly RegistrationEmployeeBenefitLookupService _service;

        public EmployeeBenefitLookupController(RegistrationEmployeeBenefitLookupService service) => _service = service;

        /// <summary>
        /// Mencari pegawai aktif dengan NIP, NIK, atau nama. Minimal 3 karakter (FIN-VAL-268); paling banyak
        /// 20 hasil, dan bila lebih, pesan respons meminta petugas mengetik lebih spesifik (FIN-VAL-269).
        /// </summary>
        [HttpGet("employees")]
        [AccessAction("Read", "Read Employee Benefit Lookup", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitLookup", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<EmployeeLookupResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> SearchEmployees([FromQuery] string? search, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(search)
                || search.Trim().Length < EmployeeLookupLimits.MinimumKeywordLength)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, TooShortKeywordMessage));
            }

            return await RunAsync(async () =>
            {
                var (items, isTruncated) = await _service.SearchEmployeesAsync(search, cancellationToken);
                return Ok(ApiResponse<List<EmployeeLookupResponse>>.Ok(
                    items, isTruncated ? TooManyResultsMessage : "Pencarian pegawai berhasil."));
            });
        }

        /// <summary>Anggota keluarga aktif satu pegawai beserta status verifikasi tanggungannya.</summary>
        [HttpGet("employees/{employeeId:guid}/family-members")]
        [AccessAction("Read", "Read Employee Benefit Lookup", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitLookup", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<FamilyMemberLookupResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        public Task<IActionResult> GetFamilyMembers(Guid employeeId, CancellationToken cancellationToken) =>
            RunAsync(async () => Ok(ApiResponse<List<FamilyMemberLookupResponse>>.Ok(
                await _service.GetFamilyMembersAsync(employeeId, cancellationToken),
                "Daftar anggota keluarga berhasil diambil.")));

        /// <summary>
        /// Memeriksa apakah orang itu layak memakai penjamin internal pada tanggal layanan. Hasil "tidak layak"
        /// tetap 200; alasannya ada pada ReasonCode dan Message. Tanpa saldo.
        /// </summary>
        [HttpGet("eligibility")]
        [AccessAction("Read", "Read Employee Benefit Lookup", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitLookup", "Read")]
        [ProducesResponseType(typeof(ApiResponse<BenefitEligibilityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<BenefitEligibilityResponse>), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> CheckEligibility(
            [FromQuery] Guid employeeId, [FromQuery] Guid? familyMemberId, [FromQuery] DateTime? serviceDate,
            CancellationToken cancellationToken)
        {
            if (employeeId == Guid.Empty)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "employeeId wajib diisi."));
            if (!serviceDate.HasValue || serviceDate.Value.Year < 2000)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "serviceDate wajib diisi dengan tanggal yang valid."));

            var result = await _service.CheckEligibilityAsync(
                employeeId, familyMemberId == Guid.Empty ? null : familyMemberId, serviceDate.Value, cancellationToken);

            if (result.ReasonCode == BenefitEligibilityReasonCodes.HrUnavailable)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<BenefitEligibilityResponse>.Ok(result, result.Message ?? string.Empty));
            }

            return Ok(ApiResponse<BenefitEligibilityResponse>.Ok(
                result, result.IsEligible ? "Layak memakai penjamin internal." : (result.Message ?? "Tidak layak memakai penjamin internal.")));
        }

        private async Task<IActionResult> RunAsync(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
            }
            catch (EmployeeBenefitHrUnavailableException exception)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<object>.Fail(StatusCodes.Status503ServiceUnavailable, exception.Message));
            }
        }
    }
}
