using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/radiology-management/rad-studies")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_RADIOLOGY_MANAGEMENT",
        moduleName: "Health Service Radiology Management",
        displayName: "Rad Study",
        AreaName = "HealthServices",
        ControllerName = "RadStudy",
        Description = "Pencatatan study, gerbang keselamatan, dan acquisition radiologi",
        SortOrder = 2
    )]
    [Tags("Health Services / Radiology Management / Rad Study")]
    public class RadStudyController : ControllerBase
    {
        private readonly RadStudyService _radStudyService;

        public RadStudyController(RadStudyService radStudyService)
        {
            _radStudyService = radStudyService;
        }

        /* ================================================================ *
         * Referensi
         * ================================================================ */

        /// <summary>
        /// Daftar modalitas beserta penanda apakah aturan keselamatannya sudah ditetapkan.
        ///
        /// Penanda itu penting untuk ditampilkan lebih dulu: modalitas tanpa aturan aktif akan
        /// menolak setiap acquisition, dan petugas berhak tahu itu sebelum pasien dipanggil,
        /// bukan setelah pasien sudah di ruangan.
        /// </summary>
        [HttpGet("modalities")]
        [ProducesResponseType(typeof(ApiResponse<List<RadModalityResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Study", Description = "Melihat daftar modalitas radiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadStudy", "Read")]
        public async Task<IActionResult> GetModalities(CancellationToken cancellationToken = default)
        {
            var result = await _radStudyService.GetModalitiesAsync(cancellationToken);

            return Ok(ApiResponse<List<RadModalityResponse>>.Ok(
                result, "Daftar modalitas radiologi berhasil diambil."));
        }

        [HttpGet("safety-requirements")]
        [ProducesResponseType(typeof(ApiResponse<List<RadSafetyRequirementResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Study", Description = "Melihat katalog butir keselamatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadStudy", "Read")]
        public async Task<IActionResult> GetSafetyRequirements(CancellationToken cancellationToken = default)
        {
            var result = await _radStudyService.GetSafetyRequirementsAsync(cancellationToken);

            return Ok(ApiResponse<List<RadSafetyRequirementResponse>>.Ok(
                result, "Katalog butir keselamatan berhasil diambil."));
        }

        /* ================================================================ *
         * Study
         * ================================================================ */

        [HttpGet("by-order/{radOrderId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<RadStudyResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Study", Description = "Melihat study pada satu order", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadStudy", "Read")]
        public async Task<IActionResult> GetByOrder(Guid radOrderId, CancellationToken cancellationToken = default)
        {
            var result = await _radStudyService.GetByOrderAsync(radOrderId, cancellationToken);

            return Ok(ApiResponse<List<RadStudyResponse>>.Ok(
                result, "Daftar study radiologi berhasil diambil."));
        }

        [HttpGet("by-order/{radOrderId:guid}/history")]
        [ProducesResponseType(typeof(ApiResponse<List<RadTransitionHistoryResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Study", Description = "Melihat riwayat perpindahan status", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadStudy", "Read")]
        public async Task<IActionResult> GetHistory(Guid radOrderId, CancellationToken cancellationToken = default)
        {
            var result = await _radStudyService.GetHistoryAsync(radOrderId, cancellationToken);

            return Ok(ApiResponse<List<RadTransitionHistoryResponse>>.Ok(
                result, "Riwayat radiologi berhasil diambil."));
        }

        [HttpPost("by-order/{radOrderId:guid}")]
        [AccessAction("Create", "Create Rad Study", Description = "Membuat study radiologi", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RadStudy", "Create")]
        public Task<IActionResult> Create(
            Guid radOrderId, [FromBody] CreateRadStudyRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radStudyService.CreateStudyAsync(radOrderId, request, cancellationToken),
                "Study radiologi berhasil dibuat.");

        [HttpPost("{id:guid}/verify-patient")]
        [AccessAction("Verify", "Verify Rad Study", Description = "Memverifikasi identitas sebelum acquisition", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadStudy", "Verify")]
        public Task<IActionResult> VerifyPatient(Guid id, CancellationToken cancellationToken = default) =>
            Execute(() => _radStudyService.VerifyPatientAsync(id, cancellationToken),
                "Identitas pasien berhasil diverifikasi.");

        [HttpPost("{id:guid}/safety-checks")]
        [AccessAction("Safety", "Decide Rad Safety Check", Description = "Menjawab butir gerbang keselamatan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("RadStudy", "Safety")]
        public Task<IActionResult> DecideSafetyCheck(
            Guid id, [FromBody] RadSafetyCheckDecisionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radStudyService.DecideSafetyCheckAsync(id, request, cancellationToken),
                "Butir keselamatan berhasil dijawab.");

        [HttpPost("{id:guid}/clear-safety")]
        [AccessAction("Safety", "Clear Rad Safety Gate", Description = "Menyatakan gerbang keselamatan tuntas", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("RadStudy", "Safety")]
        public Task<IActionResult> ClearSafety(Guid id, CancellationToken cancellationToken = default) =>
            Execute(() => _radStudyService.ClearSafetyAsync(id, cancellationToken),
                "Gerbang keselamatan dinyatakan tuntas.");

        [HttpPost("{id:guid}/start-acquisition")]
        [ProducesResponseType(typeof(ApiResponse<RadStudyResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Acquire", "Acquire Rad Study", Description = "Memulai acquisition radiologi", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("RadStudy", "Acquire")]
        public Task<IActionResult> StartAcquisition(Guid id, CancellationToken cancellationToken = default) =>
            Execute(() => _radStudyService.StartAcquisitionAsync(id, cancellationToken),
                "Acquisition dimulai.");

        [HttpPost("{id:guid}/complete-acquisition")]
        [AccessAction("Acquire", "Acquire Rad Study", Description = "Menyelesaikan acquisition radiologi", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("RadStudy", "Acquire")]
        public Task<IActionResult> CompleteAcquisition(Guid id, CancellationToken cancellationToken = default) =>
            Execute(() => _radStudyService.CompleteAcquisitionAsync(id, cancellationToken),
                "Acquisition selesai dikerjakan.");

        [HttpPost("{id:guid}/abort-acquisition")]
        [AccessAction("Acquire", "Acquire Rad Study", Description = "Menghentikan acquisition radiologi", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("RadStudy", "Acquire")]
        public Task<IActionResult> AbortAcquisition(
            Guid id, [FromBody] RadAbortAcquisitionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radStudyService.AbortAcquisitionAsync(id, request, cancellationToken),
                "Acquisition dihentikan dan sebabnya tercatat.");

        /// <summary>
        /// Menilai kualitas citra. Inilah satu-satunya titik yang menerbitkan kelayakan tagih.
        /// </summary>
        [HttpPost("{id:guid}/decide-quality")]
        [ProducesResponseType(typeof(ApiResponse<RadStudyActionResult>), StatusCodes.Status200OK)]
        [AccessAction("Quality", "Decide Rad Study Quality", Description = "Menilai kualitas citra", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("RadStudy", "Quality")]
        public Task<IActionResult> DecideQuality(
            Guid id, [FromBody] RadAcquisitionQualityRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radStudyService.DecideQualityAsync(id, request, cancellationToken),
                "Penilaian kualitas tersimpan.");

        [HttpPost("{id:guid}/repeat")]
        [AccessAction("Repeat", "Repeat Rad Study", Description = "Mengulang study radiologi", AccessType = AccessTypes.Create, SortOrder = 7)]
        [AccessPermission("RadStudy", "Repeat")]
        public Task<IActionResult> Repeat(
            Guid id, [FromBody] RadRepeatStudyRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radStudyService.RepeatStudyAsync(id, request, cancellationToken),
                "Study pengulangan berhasil dibuat; study asli tetap utuh.");

        [HttpPost("{id:guid}/consumptions")]
        [AccessAction("Consumption", "Record Rad Consumption", Description = "Mencatat konsumsi bahan acquisition", AccessType = AccessTypes.Create, SortOrder = 8)]
        [AccessPermission("RadStudy", "Consumption")]
        public Task<IActionResult> RecordConsumption(
            Guid id, [FromBody] RadConsumptionRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radStudyService.RecordConsumptionAsync(id, request, cancellationToken),
                "Konsumsi bahan berhasil dicatat.");

        /// <summary>
        /// Memetakan hasil tindakan menjadi status HTTP.
        ///
        /// `SafetyBlocked` dan `PolicyNotConfigured` menjadi <c>422</c>, bukan <c>400</c>:
        /// permintaannya sendiri sah, yang belum terpenuhi adalah prasyaratnya. Kode galatnya
        /// tetap berbeda supaya layar dapat menuntun ke tindakan yang benar — menyelesaikan
        /// gerbangnya, atau meminta admin menetapkan aturannya.
        /// </summary>
        private async Task<IActionResult> Execute<T>(
            Func<Task<RadOperationResult<T>>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return result.Kind switch
                {
                    RadOperationResultKind.Success =>
                        Ok(ApiResponse<T>.Ok(result.Value, successMessage)),

                    RadOperationResultKind.NotFound =>
                        NotFound(ApiResponse<object>.Fail(
                            StatusCodes.Status404NotFound,
                            result.ErrorMessage ?? "Data tidak ditemukan.",
                            new { Code = result.ErrorCode })),

                    RadOperationResultKind.Conflict =>
                        Conflict(ApiResponse<object>.Fail(
                            StatusCodes.Status409Conflict,
                            result.ErrorMessage ?? "Terjadi konflik.",
                            new { Code = result.ErrorCode })),

                    RadOperationResultKind.SafetyBlocked or
                    RadOperationResultKind.PolicyNotConfigured =>
                        UnprocessableEntity(ApiResponse<object>.Fail(
                            StatusCodes.Status422UnprocessableEntity,
                            result.ErrorMessage ?? "Prasyarat belum terpenuhi.",
                            new { Code = result.ErrorCode })),

                    _ => BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        result.ErrorMessage ?? "Permintaan tidak valid.",
                        new { Code = result.ErrorCode })),
                };
            }
            catch (RadConcurrencyException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    exception.Message,
                    new { Code = RadErrorCodes.ConcurrencyConflict }));
            }
        }
    }
}
