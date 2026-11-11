using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Controllers
{
    /// <summary>
    /// Controller Integrasi Real-Time Loket Registrasi & Kasir SIMRS (Slice S-D3, BE-HRD-186 & 187).
    /// Tag Swagger: 'HR Benefits - In-House Healthcare & Balance'.
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/hrd/benefits/inhouse")]
    [Route("api/hr-benefits/inhouse")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_BENEFIT_MANAGEMENT",
        moduleName: "Human Resource Benefit Management",
        displayName: "Plafon Berobat di RS (In-House)",
        AreaName = "Corporate",
        ControllerName = "InHouseHealthcare",
        Description = "Cek eligibilitas dan potong saldo plafon berobat staf oleh SIMRS, buku besar, dan koreksi",
        SortOrder = 23)]
    [Tags("HR Benefits - In-House Healthcare & Balance")]
    public class InHouseHealthcareController : ControllerBase
    {
        private readonly IBenefitInHouseService _inHouseService;

        public InHouseHealthcareController(IBenefitInHouseService inHouseService)
        {
            _inHouseService = inHouseService;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(claim, out var uid) && uid != Guid.Empty)
            {
                return uid;
            }

            // Tidak memakai aktor cadangan: jejak audit tanpa identitas asli lebih berbahaya
            // daripada permintaan yang ditolak.
            throw new UnauthorizedAccessException("Identitas user pada token tidak valid.");
        }

        /// <summary>
        /// Validasi hak berobat staf/keluarga secara online oleh petugas admisi loket SIMRS (BE-HRD-186, INT-HR-61).
        /// </summary>
        [HttpGet("coverage-inquiry")]
        [AccessAction("Inquiry", "Mengecek Eligibilitas Plafon Staf", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InHouseHealthcare", "Inquiry")]
        [ProducesResponseType(typeof(InHouseCoverageInquiryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> InquireCoverage(
            [FromQuery] InHouseCoverageInquiryQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _inHouseService.InquireCoverageAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Mengurangi saldo plafon subsidi nakes saat kasir SIMRS menutup kuitansi tindakan (BE-HRD-186, BEN-B4, VR-BEN-14..15).
        /// </summary>
        [HttpPost("deduct-balance")]
        [AccessAction("Deduct", "Memotong Saldo Plafon Staf", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("InHouseHealthcare", "Deduct")]
        [ProducesResponseType(typeof(BenefitBalanceDeductionResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeductBalance(
            [FromBody] DeductInHouseBalanceRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _inHouseService.DeductBalanceAsync(
                    request, GetCurrentUserId(), cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengambil histori mutasi buku besar plafon tahunan nakes (BE-HRD-187, HrdBenefitBalanceMovement).
        /// </summary>
        [HttpGet("ledger/{employeeId:guid}")]
        [AccessAction("Read", "Melihat Buku Besar Plafon Staf", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("InHouseHealthcare", "Read")]
        [ProducesResponseType(typeof(List<BenefitBalanceMovementDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLedger(
            Guid employeeId,
            [FromQuery] int? year,
            CancellationToken cancellationToken)
        {
            var result = await _inHouseService.GetLedgerMovementsAsync(employeeId, year, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Penyesuaian manual saldo plafon nakes oleh HR Manajer (BE-HRD-187).
        /// </summary>
        [HttpPost("ledger/adjust")]
        [AccessAction("Adjust", "Mengoreksi Saldo Plafon Staf", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("InHouseHealthcare", "Adjust")]
        [ProducesResponseType(typeof(BenefitBalanceMovementDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AdjustLedger(
            [FromBody] AdjustBalanceManualRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _inHouseService.AdjustLedgerBalanceAsync(
                    request, GetCurrentUserId(), cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
