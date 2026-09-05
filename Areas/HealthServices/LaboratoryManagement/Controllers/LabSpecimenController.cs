using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers
{
    /// <summary>
    /// Alur operasional sampel laboratorium.
    ///
    /// Setiap langkah memakai permission tersendiri sesuai <c>RJ-BIL-GATE-DEC-003</c>: hak
    /// mengambil sampel tidak otomatis memberi hak menyatakan sampel layak periksa, dan jabatan
    /// organisasi tidak memberi kewenangan apa pun dengan sendirinya. Permission yang belum
    /// diberikan kepada role akan ditolak dengan <c>403</c>, bukan diloloskan.
    ///
    /// Tidak ada satu pun endpoint finansial di sini. Laboratorium tidak dapat menyatakan
    /// sesuatu lunas, dibatalkan secara finansial, disetujui penjamin, di-void, dikembalikan,
    /// maupun dibalik.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-specimens")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Specimen",
        AreaName = "HealthServices",
        ControllerName = "LabSpecimen",
        Description = "Alur sampel laboratorium sampai penetapan kelayakan pemeriksaan",
        SortOrder = 2
    )]
    [Tags("Health Services / Laboratory Management / Lab Specimen")]
    public class LabSpecimenController : ControllerBase
    {
        private readonly LabSpecimenService _labSpecimenService;

        public LabSpecimenController(LabSpecimenService labSpecimenService)
        {
            _labSpecimenService = labSpecimenService;
        }

        // Keterangan bentuk layar wadah: pilihan status, sebab ambil ulang, urutan, dan ukuran
        // halaman.
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Specimen", Description = "Melihat daftar pilihan penyaring wadah sampel", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabSpecimen", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = _labSpecimenService.GetFilterMetadata();

            return Ok(ApiResponse<LabSpecimenFilterMetadataResponse>.Ok(
                result,
                "Metadata penyaring wadah sampel berhasil diambil."));
        }

        // Rekap wadah pada satu rentang waktu, termasuk pencacahan sebab pengambilan ulang.
        // Bila rentangnya tidak dikirim, dipakai 30 hari terakhir.
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Lab Specimen", Description = "Melihat rekap wadah sampel", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabSpecimen", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            CancellationToken cancellationToken = default)
        {
            var akhir = endDate ?? DateTime.UtcNow;
            var awal = startDate ?? akhir.AddDays(-30);

            if (awal > akhir)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal awal tidak boleh melewati tanggal akhir."));
            }

            var result = await _labSpecimenService.GetSummaryAsync(awal, akhir, cancellationToken);

            return Ok(ApiResponse<LabSpecimenSummaryResponse>.Ok(
                result,
                "Rekap wadah sampel berhasil diambil."));
        }

        [HttpGet("rejection-reasons")]
        [ProducesResponseType(typeof(ApiResponse<List<LabRejectionReasonResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Specimen", Description = "Melihat katalog alasan penolakan sampel", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabSpecimen", "Read")]
        public async Task<IActionResult> GetRejectionReasons(CancellationToken cancellationToken = default)
        {
            var result = await _labSpecimenService.GetRejectionReasonsAsync(cancellationToken);

            return Ok(ApiResponse<List<LabRejectionReasonResponse>>.Ok(
                result,
                "Katalog alasan penolakan sampel berhasil diambil."));
        }

        [HttpGet("by-order/{labOrderId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<LabSpecimenResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Specimen", Description = "Melihat sampel pada satu order laboratorium", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabSpecimen", "Read")]
        public async Task<IActionResult> GetByOrder(Guid labOrderId, CancellationToken cancellationToken = default)
        {
            var result = await _labSpecimenService.GetByOrderAsync(labOrderId, cancellationToken);

            return Ok(ApiResponse<List<LabSpecimenResponse>>.Ok(
                result,
                "Daftar sampel laboratorium berhasil diambil."));
        }

        [HttpGet("by-order/{labOrderId:guid}/history")]
        [ProducesResponseType(typeof(ApiResponse<List<LabTransitionHistoryResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Specimen", Description = "Melihat riwayat perpindahan status order dan sampel", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabSpecimen", "Read")]
        public async Task<IActionResult> GetHistory(Guid labOrderId, CancellationToken cancellationToken = default)
        {
            var result = await _labSpecimenService.GetHistoryAsync(labOrderId, cancellationToken);

            return Ok(ApiResponse<List<LabTransitionHistoryResponse>>.Ok(
                result,
                "Riwayat laboratorium berhasil diambil."));
        }

        [HttpPost("by-order/{labOrderId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Plan Lab Specimen", Description = "Merencanakan sampel dan komponen pemeriksaan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabSpecimen", "Plan")]
        public Task<IActionResult> Plan(
            Guid labOrderId,
            [FromBody] PlanLabSpecimenRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labSpecimenService.PlanAsync(labOrderId, request, cancellationToken),
                "Sampel laboratorium berhasil direncanakan.",
                StatusCodes.Status201Created);

        [HttpPost("{id:guid}/collect")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Collect Lab Specimen", Description = "Mencatat pengambilan sampel", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabSpecimen", "Collect")]
        public Task<IActionResult> Collect(
            Guid id,
            [FromBody] CollectLabSpecimenRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labSpecimenService.CollectAsync(id, request, cancellationToken),
                "Pengambilan sampel berhasil dicatat.");

        [HttpPost("{id:guid}/receive")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Receive Lab Specimen", Description = "Mencatat sampel tiba di laboratorium", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabSpecimen", "Receive")]
        public Task<IActionResult> Receive(
            Guid id,
            [FromBody] ReceiveLabSpecimenRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labSpecimenService.ReceiveAsync(id, request, cancellationToken),
                "Penerimaan sampel berhasil dicatat.");

        // Menyatakan sampel layak periksa. Satu-satunya endpoint Laboratorium yang menerbitkan
        // fakta kelayakan tagih ke Billing.
        [HttpPost("{id:guid}/accept")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Accept Lab Specimen", Description = "Menyatakan sampel layak periksa", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LabSpecimen", "Accept")]
        public Task<IActionResult> Accept(
            Guid id,
            [FromBody] AcceptLabSpecimenRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labSpecimenService.AcceptAsync(id, request, cancellationToken),
                "Sampel dinyatakan layak periksa.");

        [HttpPost("{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Accept Lab Specimen", Description = "Menolak sampel dengan alasan terkendali", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LabSpecimen", "Accept")]
        public Task<IActionResult> Reject(
            Guid id,
            [FromBody] RejectLabSpecimenRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labSpecimenService.RejectAsync(id, request, cancellationToken),
                "Sampel ditolak dan tidak menghasilkan tagihan pemeriksaan.");

        [HttpPost("{id:guid}/request-recollection")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Accept Lab Specimen", Description = "Meminta pengambilan ulang sampel", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LabSpecimen", "Accept")]
        public Task<IActionResult> RequestRecollection(
            Guid id,
            [FromBody] RequestLabRecollectionRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labSpecimenService.RequestRecollectionAsync(id, request, cancellationToken),
                "Pengambilan ulang diminta dan sampel pengganti dibuat.",
                StatusCodes.Status201Created);

        [HttpPost("{id:guid}/hold")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Hold Lab Specimen", Description = "Menahan sampel sementara", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("LabSpecimen", "Hold")]
        public Task<IActionResult> Hold(
            Guid id,
            [FromBody] HoldLabRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labSpecimenService.HoldAsync(id, request, cancellationToken),
                "Sampel ditahan.");

        [HttpPost("{id:guid}/resume")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Hold Lab Specimen", Description = "Melanjutkan sampel yang ditahan", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("LabSpecimen", "Hold")]
        public Task<IActionResult> Resume(
            Guid id,
            [FromBody] ResumeLabRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labSpecimenService.ResumeAsync(id, request, cancellationToken),
                "Sampel dilanjutkan.");

        // Membatalkan sampel secara klinis. Pembatalan klinis bukan pembatalan finansial:
        // tagihan yang sudah terbentuk tidak dihapus, dan Billing yang menentukan koreksinya.
        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Cancel Lab Specimen", Description = "Membatalkan sampel secara klinis", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("LabSpecimen", "Cancel")]
        public Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelLabSpecimenRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labSpecimenService.CancelAsync(id, request, cancellationToken),
                "Sampel dibatalkan secara klinis.");

        /// <summary>
        /// Menjalankan satu tindakan operasional dan menerjemahkan kegagalannya menjadi status
        /// HTTP yang tepat.
        ///
        /// Pesan yang dikembalikan hanya pesan yang memang disusun untuk pengguna. Stack trace
        /// dan detail exception tidak pernah ikut ke response.
        /// </summary>
        private async Task<IActionResult> ExecuteAsync(
            Func<Task<LabSpecimenActionResult>> action,
            string successMessage,
            int successStatusCode = StatusCodes.Status200OK)
        {
            try
            {
                var result = await action();
                var payload = MapResponse(result);

                return StatusCode(
                    successStatusCode,
                    ApiResponse<LabSpecimenResponse>.Ok(payload, successMessage));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    exception.Message));
            }
            catch (LabConcurrencyException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    exception.Message));
            }
            catch (LabSpecimenForbiddenException exception)
            {
                return StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));
            }
            catch (LabSpecimenConflictException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    exception.Message));
            }
            catch (LabSpecimenValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    exception.Message));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    exception.Message));
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    exception.Message));
            }
        }

        private static LabSpecimenResponse MapResponse(LabSpecimenActionResult result)
        {
            var specimen = result.Specimen;

            return new LabSpecimenResponse
            {
                Id = specimen.Id,
                LabOrderId = specimen.LabOrderId,
                SpecimenBarcode = specimen.SpecimenBarcode,
                SpecimenSequence = specimen.SpecimenSequence,
                SpecimenDescription = specimen.SpecimenDescription,
                SpecimenStatus = specimen.SpecimenStatus.ToString(),
                CollectedAt = specimen.CollectedAt,
                ReceivedAt = specimen.ReceivedAt,
                DecidedAt = specimen.DecidedAt,
                RejectionReasonCode = specimen.RejectionReasonCode,
                RejectionNote = specimen.RejectionNote,
                SupersededSpecimenId = specimen.SupersededSpecimenId,
                RecollectionCause = specimen.RecollectionCause?.ToString(),
                Version = specimen.Version,
                BillingHandoff = result.Handoff == null
                    ? null
                    : LabOrderService.MapHandoff(result.Handoff)
            };
        }
    }
}
