using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Controllers
{
    /// <summary>
    /// REST API Controller untuk Perlindungan Hukum Hak Istimewa Telaah Sejawat (Peer Review Privilege) & Read-Audit Logging
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/hrd/clinical-performance/peer-review-audit")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PERFORMANCE",
        moduleName: "Human Resource Performance",
        displayName: "Audit Akses Peer Review Klinis",
        AreaName = "Corporate",
        ControllerName = "ClinicalPeerReviewAudit",
        Description = "Pencatatan dan peninjauan log akses berkas peer review rahasia",
        SortOrder = 10)]
    [Tags("Corporate - HRD Peer Review Confidentiality & Audit Log")]
    public class ClinicalPeerReviewAuditController : ControllerBase
    {
        private readonly IClinicalPeerReviewAuditService _auditService;

        public ClinicalPeerReviewAuditController(IClinicalPeerReviewAuditService auditService)
        {
            _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        }

        /// <summary>
        /// Mencatat jejak audit permanen setiap kali dokumen telaah sejawat / performa klinis dibuka (VR-OPP-23)
        /// </summary>
        [HttpPost("log-access")]
        [AccessAction("LogAccess", "Mencatat Akses Berkas Peer Review", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("ClinicalPeerReviewAudit", "LogAccess")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ClinicalPeerReviewAuditLogDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> LogAccess(
            [FromBody] LogPeerReviewAccessRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Data permintaan audit log tidak valid."));
            }

            try
            {
                var result = await _auditService.LogPeerReviewAccessAsync(request, cancellationToken);
                return Ok(ApiResponse<ClinicalPeerReviewAuditLogDto>.Ok(result, "Jejak audit akses konfidensial berhasil dicatat (Tamper-Proof Read-Audit Log)."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Menampilkan log jejak audit akses dokumen konfidensial untuk kepatuhan hukum & komite etik
        /// </summary>
        [HttpGet("logs")]
        [AccessAction("Read", "Melihat Log Akses Peer Review", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("ClinicalPeerReviewAudit", "Read")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<ClinicalPeerReviewAuditLogDto>>), 200)]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] Guid? workforceProfileId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            CancellationToken cancellationToken = default)
        {
            var logs = await _auditService.GetAuditLogsAsync(workforceProfileId, fromDate, toDate, cancellationToken);
            return Ok(ApiResponse<List<ClinicalPeerReviewAuditLogDto>>.Ok(logs, "Data log audit kerahasiaan telaah sejawat berhasil diambil."));
        }
    }
}
