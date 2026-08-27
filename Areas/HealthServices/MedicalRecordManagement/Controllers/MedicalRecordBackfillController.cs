using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Controllers
{
    /// <summary>
    /// Menjalankan pengisian status keutuhan untuk catatan klinis yang sudah tersimpan
    /// sebelum modul rekam medis ada.
    ///
    /// Dipisahkan sebagai endpoint tersendiri, bukan dijalankan otomatis saat aplikasi naik,
    /// supaya waktunya dapat dipilih dan hasilnya dapat ditelaah lebih dulu.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-record-management/backfill")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MEDICAL_RECORD",
        moduleName: "Health Service Medical Record",
        displayName: "Medical Record Backfill",
        AreaName = "HealthServices",
        ControllerName = "MedicalRecordBackfill",
        Description = "Pengisian status keutuhan untuk catatan klinis lama",
        SortOrder = 6
    )]
    [Tags("Health Services / Medical Record Management / Medical Record Backfill")]
    public class MedicalRecordBackfillController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MedicalRecord";

        private readonly LoggerService _loggerService;
        private readonly MedicalRecordBackfillService _backfillService;

        public MedicalRecordBackfillController(
            LoggerService loggerService,
            MedicalRecordBackfillService backfillService)
        {
            _loggerService = loggerService;
            _backfillService = backfillService;
        }

        [HttpGet("survey")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordBackfillSurveyResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Survey Medical Record Backfill", Description = "Menelaah catatan lama sebelum pengisian dijalankan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordBackfill", "Read")]
        public async Task<IActionResult> Survey([FromQuery] int batchSize = 500)
        {
            var hasil = await _backfillService.SurveyAsync(batchSize);

            return Ok(ApiResponse<MedicalRecordBackfillSurveyResponse>.Ok(
                hasil, "Penelaahan catatan lama berhasil."));
        }

        [HttpPost("run-batch")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordBackfillRunResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Run Medical Record Backfill", Description = "Menjalankan satu potongan pengisian catatan lama", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("MedicalRecordBackfill", "Update")]
        public async Task<IActionResult> RunBatch(
            [FromQuery] int batchSize = 500,
            [FromQuery] bool isDryRun = true)
        {
            var actorUserId = GetCurrentUserId();

            var hasil = await _backfillService.ExecuteBatchAsync(
                actorUserId, DateTime.UtcNow, batchSize, isDryRun);

            // Penjalanan sungguhan selalu dicatat, percobaan tidak. Yang perlu ditelusuri
            // kemudian adalah tindakan yang benar-benar mengubah data.
            if (!isDryRun)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "MedicalRecordBackfill.RunBatch",
                    "Satu potongan pengisian catatan lama dijalankan.",
                    new
                    {
                        hasil.JumlahDiproses,
                        hasil.JumlahTerkunciTanpaTandaTangan,
                        hasil.JumlahTetapDraf,
                        hasil.JumlahPenulisTidakDiketahui,
                        hasil.PerkiraanSisa
                    });
            }

            var pesan = isDryRun
                ? "Percobaan pengisian selesai. Tidak ada data yang diubah."
                : "Satu potongan pengisian berhasil dijalankan.";

            return Ok(ApiResponse<MedicalRecordBackfillRunResponse>.Ok(hasil, pesan));
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}
