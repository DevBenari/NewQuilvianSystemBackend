using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Controllers
{
    /// <remarks>
    /// Indikator ketepatan tanda tangan ringkasan pulang Rawat Inap per dokter, untuk OPPE/FPPE HR
    /// (`BE-49`, kontrak API 0.4.0 C1, `INT-RM-20`). Hanya angka; tidak pernah identitas pasien,
    /// nomor rekam medis, kunjungan, maupun isi ringkasan (`RM-DEC-101`).
    ///
    /// Izin `DischargeSummaryTimeliness : Read` sengaja terpisah dari `MedicalRecordCompleteness : Read`
    /// dan tidak dipasangkan ke jabatan mana pun secara bawaan (`DS-R6-05`).
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-record-management/discharge-summary-timeliness")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MEDICAL_RECORD",
        moduleName: "Health Service Medical Record",
        displayName: "Discharge Summary Timeliness",
        AreaName = "HealthServices",
        ControllerName = "DischargeSummaryTimeliness",
        Description = "Indikator ketepatan tanda tangan ringkasan pulang Rawat Inap per dokter untuk OPPE",
        SortOrder = 8
    )]
    [Tags("Health Services / Medical Record Management / Discharge Summary Timeliness")]
    public class DischargeSummaryTimelinessController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MedicalRecord";

        private readonly DischargeSummaryTimelinessService _timelinessService;
        private readonly LoggerService _loggerService;

        public DischargeSummaryTimelinessController(
            DischargeSummaryTimelinessService timelinessService,
            LoggerService loggerService)
        {
            _timelinessService = timelinessService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DischargeSummaryTimelinessFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Discharge Summary Timeliness", Description = "Melihat definisi kolom indikator ketepatan ringkasan pulang", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DischargeSummaryTimeliness", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<DischargeSummaryTimelinessFilterMetadataResponse>.Ok(
                DischargeSummaryTimelinessService.BuildFilterMetadata(),
                "Definisi indikator ketepatan ringkasan pulang berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<DischargeSummaryTimelinessPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction("Read", "Read Discharge Summary Timeliness", Description = "Melihat angka ketepatan tanda tangan ringkasan pulang per dokter per periode", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DischargeSummaryTimeliness", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] DischargeSummaryTimelinessPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var galat = DischargeSummaryTimelinessService.ValidateQuery(query);
            if (galat != null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, galat));

            var hasil = await _timelinessService.GetPagedAsync(query, cancellationToken);
            if (hasil == null)
            {
                // Integration 0.4.0 "Kegagalan": tidak pernah angka sebagian, dan tidak ada audit.
                await _loggerService.ErrorAsync(
                    LogCategory,
                    "DischargeSummaryTimeliness.Read",
                    $"Indikator ketepatan ringkasan pulang periode {query.PeriodFrom:yyyy-MM-dd} s.d. {query.PeriodTo:yyyy-MM-dd} gagal: sumber Rawat Inap tidak dapat dibaca.");

                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status503ServiceUnavailable,
                        DischargeSummaryTimelinessService.SourceUnavailableMessage));
            }

            // Permission/audit 0.4.0: isi audit di teks pesan (AuditAsync mengabaikan objek data);
            // hanya periode, saringan, jumlah item, dan waktu tarik — tanpa angka per dokter.
            await _loggerService.AuditAsync(
                LogCategory,
                "DischargeSummaryTimeliness.Read",
                $"Menarik indikator ketepatan ringkasan pulang periode {hasil.PeriodFrom:yyyy-MM-dd} s.d. {hasil.PeriodTo:yyyy-MM-dd}, " +
                $"saringan dokter {(query.DoctorId?.ToString() ?? "-")}, profil HR {(query.WorkforceProfileId?.ToString() ?? "-")}, " +
                $"{hasil.TotalData} baris dokter, ditarik {hasil.GeneratedAt:yyyy-MM-ddTHH:mm:ssZ}.");

            return Ok(ApiResponse<DischargeSummaryTimelinessPagedResult>.Ok(
                hasil, "Indikator ketepatan ringkasan pulang berhasil diambil."));
        }
    }
}
