using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Controllers
{
    /// <summary>
    /// REST API Controller untuk Program Pembinaan Formal PIP 60 Hari & Evaluasi Jenjang Karier Klinis Nakes PK I-V (Kontrak v21 - Tag 27.4)
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/hrd/performance")]
    [Tags("Corporate - HRD Performance Improvement Plan & Clinical Ladder")]
    public class PerformancePipAndCareerController : ControllerBase
    {
        private readonly IPerformanceImprovementPlanService _pipService;
        private readonly IClinicalCareerLadderService _careerService;

        public PerformancePipAndCareerController(
            IPerformanceImprovementPlanService pipService,
            IClinicalCareerLadderService careerService)
        {
            _pipService = pipService ?? throw new ArgumentNullException(nameof(pipService));
            _careerService = careerService ?? throw new ArgumentNullException(nameof(careerService));
        }

        #region PIP
        /// <summary>
        /// Menampilkan daftar pegawai yang sedang menjalani Program Peningkatan Kinerja (skor komposit < 65.00, VR-PRF-08)
        /// </summary>
        [HttpGet("pip/active-list")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<PipActiveSummaryDto>>), 200)]
        public async Task<IActionResult> GetActivePipList(CancellationToken cancellationToken = default)
        {
            var result = await _pipService.GetActivePipListAsync(cancellationToken);
            return Ok(ApiResponse<List<PipActiveSummaryDto>>.Ok(result, "Daftar pegawai berstatus pembinaan PIP aktif berhasil diambil."));
        }

        /// <summary>
        /// Menerbitkan kontrak pembinaan formal PIP 60 hari kalender, sasaran perbaikan, dan penugasan mentor (VR-PRF-08, PRF-B7)
        /// </summary>
        [HttpPost("pip/issue-contract")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PipActiveSummaryDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> IssuePipContract(
            [FromBody] IssuePipContractRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _pipService.IssueContractAsync(dto, Guid.Empty, cancellationToken);
                return StatusCode(201, ApiResponse<PipActiveSummaryDto>.Ok(result, "Surat kontrak pembinaan formal PIP 60 hari berhasil diterbitkan."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Menginput catatan observasi monitoring mingguan (Minggu 1 s.d. 8) oleh mentor dan atasan (VR-PRF-09, VR-PRF-15)
        /// </summary>
        [HttpPost("pip/{id}/weekly-logs")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> RecordPipWeeklyLog(
            [FromRoute] Guid id,
            [FromQuery] Guid? mentorId = null,
            [FromBody] RecordPipWeeklyLogRequestDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var mId = mentorId ?? Guid.Empty;
                var result = await _pipService.RecordWeeklyLogAsync(id, dto, mId, cancellationToken);
                return Ok(ApiResponse<bool>.Ok(result, "Logbook pemantauan mingguan mentor PIP berhasil dicatat."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Menerbitkan putusan akhir sidang PIP (Lulus, Perpanjang 30 Hari, atau Eskalasi HR Legal, VR-PRF-10)
        /// </summary>
        [HttpPost("pip/{id}/final-verdict")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 422)]
        public async Task<IActionResult> DeliverPipFinalVerdict(
            [FromRoute] Guid id,
            [FromQuery] Guid? approverId = null,
            [FromBody] VerdictPipRequestDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var aId = approverId ?? Guid.Empty;
                var result = await _pipService.DeliverFinalVerdictAsync(id, dto, aId, cancellationToken);
                return Ok(ApiResponse<bool>.Ok(result, "Putusan sidang akhir evaluasi hari ke-60 PIP berhasil ditetapkan."));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(422, ApiResponse<string>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }
        #endregion

        #region CLINICAL LADDER
        /// <summary>
        /// Pengajuan kenaikan jenjang klinis nakes (PK I-V) via ESS dengan validasi gate KPI >= 75 (VR-PRF-14, PRF-B8)
        /// </summary>
        [HttpPost("clinical-ladder/apply")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ClinicalCareerLadderDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 422)]
        public async Task<IActionResult> ApplyClinicalLadder(
            [FromQuery] Guid? workforceProfileId = null,
            [FromBody] ApplyClinicalLadderRequestDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var staffId = workforceProfileId ?? Guid.Empty;
                var result = await _careerService.ApplyClinicalLadderAsync(staffId, dto, cancellationToken);
                return StatusCode(201, ApiResponse<ClinicalCareerLadderDto>.Ok(result, "Permohonan asesmen kenaikan jenjang klinis nakes berhasil diajukan ke Komite Keperawatan."));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(422, ApiResponse<string>.Fail(422, ex.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Pengesahan rekomendasi sidang Komite Keperawatan dan sinkronisasi SK jenjang baru ke Sub-02/14 (INT-HR-83)
        /// </summary>
        [HttpPost("clinical-ladder/{id}/verdict")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ClinicalCareerLadderDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> DeliverClinicalLadderVerdict(
            [FromRoute] Guid id,
            [FromQuery] Guid? assessorId = null,
            [FromBody] VerdictClinicalLadderRequestDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var aId = assessorId ?? Guid.Empty;
                var result = await _careerService.DeliverVerdictAsync(id, dto, aId, cancellationToken);
                return Ok(ApiResponse<ClinicalCareerLadderDto>.Ok(result, "Putusan sidang komite dan penetapan SK Direktur Utama jenjang klinis baru berhasil diproses."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }
        #endregion
    }
}
