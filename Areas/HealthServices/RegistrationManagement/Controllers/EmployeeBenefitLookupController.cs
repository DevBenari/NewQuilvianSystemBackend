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
        private readonly IPatientWorkforceIdentityResolver _workforceResolver;

        public EmployeeBenefitLookupController(
            RegistrationEmployeeBenefitLookupService service,
            IPatientWorkforceIdentityResolver workforceResolver)
        {
            _service = service;
            _workforceResolver = workforceResolver;
        }

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

        /// <summary>
        /// Mencari pemilik benefit gabungan: karyawan dan dokter tetap aktif berdasarkan nama, NIP/nomor dokter,
        /// atau NIK. Berbasis WorkforceProfileId. Minimal 3 karakter; paling banyak 20 hasil; NIK tersamar.
        /// Endpoint <c>employees</c> tetap berfungsi untuk kompatibilitas.
        /// </summary>
        [HttpGet("workforce-profiles")]
        [AccessAction("Read", "Read Employee Benefit Lookup", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitLookup", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<WorkforceProfileLookupResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> SearchWorkforceProfiles([FromQuery] string? search, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(search)
                || search.Trim().Length < EmployeeLookupLimits.MinimumKeywordLength)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, TooShortKeywordMessage));
            }

            return await RunAsync(async () =>
            {
                var (items, isTruncated) = await _service.SearchWorkforceAsync(search, cancellationToken);
                return Ok(ApiResponse<List<WorkforceProfileLookupResponse>>.Ok(
                    items, isTruncated ? TooManyResultsMessage : "Pencarian pemilik benefit berhasil."));
            });
        }

        /// <summary>Anggota keluarga aktif satu pemilik benefit (karyawan atau dokter tetap) berdasarkan WorkforceProfileId.</summary>
        [HttpGet("workforce-profiles/{workforceProfileId:guid}/family-members")]
        [AccessAction("Read", "Read Employee Benefit Lookup", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeBenefitLookup", "Read")]
        [ProducesResponseType(typeof(ApiResponse<List<FamilyMemberLookupResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        public Task<IActionResult> GetFamilyMembersByWorkforce(Guid workforceProfileId, CancellationToken cancellationToken) =>
            RunAsync(async () => Ok(ApiResponse<List<FamilyMemberLookupResponse>>.Ok(
                await _service.GetFamilyMembersByWorkforceAsync(workforceProfileId, cancellationToken),
                "Daftar anggota keluarga berhasil diambil.")));

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
            [FromQuery] Guid? workforceProfileId,
            CancellationToken cancellationToken)
        {
            // workforceProfileId adalah identitas utama (karyawan atau dokter tetap); employeeId tetap diterima
            // untuk kompatibilitas. Salah satu wajib diisi.
            var hasWorkforce = workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty;
            if (employeeId == Guid.Empty && !hasWorkforce)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "workforceProfileId atau employeeId wajib diisi."));
            if (!serviceDate.HasValue || serviceDate.Value.Year < 2000)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "serviceDate wajib diisi dengan tanggal yang valid."));

            var result = await _service.CheckEligibilityAsync(
                hasWorkforce ? workforceProfileId : null,
                employeeId == Guid.Empty ? null : employeeId,
                familyMemberId == Guid.Empty ? null : familyMemberId,
                serviceDate.Value,
                cancellationToken);

            if (result.ReasonCode == BenefitEligibilityReasonCodes.HrUnavailable)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<BenefitEligibilityResponse>.Ok(result, result.Message ?? string.Empty));
            }

            return Ok(ApiResponse<BenefitEligibilityResponse>.Ok(
                result, result.IsEligible ? "Layak memakai penjamin internal." : (result.Message ?? "Tidak layak memakai penjamin internal.")));
        }

        /// <summary>
        /// Mendeteksi otomatis status karyawan/dokter dan kelayakan benefit berdasarkan No. RM / PatientId.
        /// Read-only: tidak memodifikasi pasien, benefit, atau membuat kartu penjamin/kunjungan.
        /// </summary>
        [HttpGet("patients/{patientId:guid}/workforce-status")]
        [HttpGet("/api/v1/health-services/registration-management/patients/{patientId:guid}/workforce-status")]
        [AccessAction("Read", "Read Patient Workforce Status", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("EmployeeBenefitLookup", "Read")]
        [ProducesResponseType(typeof(ApiResponse<PatientWorkforceStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        public Task<IActionResult> GetPatientWorkforceStatus(Guid patientId, CancellationToken cancellationToken) =>
            RunAsync(async () =>
            {
                var result = await _workforceResolver.ResolveStatusAsync(patientId, cancellationToken);
                return Ok(ApiResponse<PatientWorkforceStatusResponse>.Ok(
                    result, "Status identitas dan benefit pasien berhasil diverifikasi."));
            });

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
