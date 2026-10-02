using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers
{
    /// <summary>
    /// MAR rawat inap — <c>BE-RWI-114</c> s.d. <c>BE-RWI-116</c>, api-contract <c>keperawatan</c> 0.5.0 bagian 7.11.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Arketipe: aggregate ber-lifecycle</b> per dosis (<c>Due</c> → <c>Administered</c>/<c>Held</c>/<c>Refused</c>/
    /// <c>Missed</c>, atau <c>Cancelled</c> oleh sistem). Tidak ada <c>DELETE</c>: dosis adalah rekam medis pemberian obat.
    /// </para>
    /// <para>
    /// <b>Hak akses.</b> <c>MedicationAdministration : Read</c>, <c>: Create</c> (mencatat), <c>: Update</c> (koreksi,
    /// evaluasi PRN), <c>: DoubleCheck</c> (perawat kedua). Seluruhnya dicentang di layar Akses Role; service menambah
    /// penjaga yang melekat pada data — episode berjalan, penempatan unit, dan pemeriksa kedua bukan pencatat.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/pharmacy-management/medication-administrations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PHARMACY",
        moduleName: "Health Service Pharmacy",
        displayName: "Medication Administration",
        AreaName = "HealthServices",
        ControllerName = "MedicationAdministration",
        Description = "Catatan pemberian obat rawat inap (MAR)",
        SortOrder = 14)]
    [Tags("Health Services / Pharmacy Management / Medication Administration")]
    public class MedicationAdministrationController : ControllerBase
    {
        private readonly MedicationAdministrationService _service;

        public MedicationAdministrationController(MedicationAdministrationService service)
        {
            _service = service;
        }

        /// <summary>
        /// MAR satu hari. Memanggil pembentukan dosis sebelum membaca; butir resep aktif sebagai baris, dosis sebagai sel.
        /// </summary>
        /// <remarks>
        /// <c>date</c> = tanggal jam dinding rumah sakit (<c>yyyy-MM-dd</c>, bawaan hari ini). <c>includeStopped=false</c>
        /// menyembunyikan butir yang sudah dihentikan sebelum hari itu; butir yang dihentikan pada hari itu tetap tampil.
        /// </remarks>
        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MedicationAdministrationChartResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Medication Administration", Description = "Melihat MAR dan detail dosis obat pasien rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicationAdministration", "Read")]
        public async Task<IActionResult> GetChart(
            Guid episodeId,
            [FromQuery] DateOnly? date,
            [FromQuery] bool includeStopped = false,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.GetChartAsync(episodeId, date, includeStopped, this.CurrentUserId(), cancellationToken));

        /// <summary>Dosis menunggu cek ganda di satu unit.</summary>
        [HttpGet("double-check-worklist")]
        [ProducesResponseType(typeof(ApiResponse<List<MedicationAdministrationListItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("DoubleCheck", "Double Check Medication Administration", Description = "Perawat kedua memeriksa dosis obat high-alert", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("MedicationAdministration", "DoubleCheck")]
        public async Task<IActionResult> GetDoubleCheckWorklist(
            [FromQuery] Guid? serviceUnitId,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.GetDoubleCheckWorklistAsync(serviceUnitId, User, this.CurrentUserId(), cancellationToken));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MedicationAdministrationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Medication Administration", Description = "Melihat MAR dan detail dosis obat pasien rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicationAdministration", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.GetAsync(id, this.CurrentUserId(), cancellationToken));

        [HttpGet("{id:guid}/revisions")]
        [ProducesResponseType(typeof(ApiResponse<List<MedicationAdministrationRevisionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medication Administration", Description = "Melihat MAR dan detail dosis obat pasien rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicationAdministration", "Read")]
        public async Task<IActionResult> GetRevisions(Guid id, CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.GetRevisionsAsync(id, cancellationToken));

        /// <summary>Mencatat hasil dosis <c>Due</c>: diberikan, ditahan, ditolak pasien, atau terlewat.</summary>
        /// <remarks>
        /// <c>400</c> isian atau alasan kosong, catatan penyimpangan kosong; <c>403</c> tidak ditempatkan di unit;
        /// <c>409</c> dosis sudah dicatat, butir dihentikan, data basi, butir sliding scale; <c>422</c> perawatan ditutup.
        /// Obat high-alert: dosis tetap <c>Due</c> dengan cek ganda <c>Pending</c>.
        /// </remarks>
        [HttpPatch("{id:guid}/record")]
        [ProducesResponseType(typeof(ApiResponse<MedicationAdministrationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Medication Administration", Description = "Mencatat pemberian obat pasien rawat inap", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MedicationAdministration", "Create")]
        public async Task<IActionResult> Record(
            Guid id,
            [FromBody] RecordMedicationAdministrationRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.RecordAsync(id, request, this.IdempotencyKey(request.IdempotencyKey), User, this.CurrentUserId(), cancellationToken));

        /// <summary>Mencatat pemberian obat sesuai kebutuhan (PRN); indikasi wajib.</summary>
        [HttpPost("as-needed")]
        [ProducesResponseType(typeof(ApiResponse<MedicationAdministrationResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Medication Administration", Description = "Mencatat pemberian obat pasien rawat inap", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MedicationAdministration", "Create")]
        public async Task<IActionResult> RecordAsNeeded(
            [FromBody] RecordAsNeededAdministrationRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.RecordAsNeededAsync(request, this.IdempotencyKey(request.IdempotencyKey), User, this.CurrentUserId(), cancellationToken));

        /// <summary>Mencatat pemberian butir berfrekuensi yang jadwalnya belum dikonfigurasi; alasan wajib.</summary>
        [HttpPost("unscheduled")]
        [ProducesResponseType(typeof(ApiResponse<MedicationAdministrationResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Medication Administration", Description = "Mencatat pemberian obat pasien rawat inap", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MedicationAdministration", "Create")]
        public async Task<IActionResult> RecordUnscheduled(
            [FromBody] RecordUnscheduledAdministrationRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.RecordUnscheduledAsync(request, this.IdempotencyKey(request.IdempotencyKey), User, this.CurrentUserId(), cancellationToken));

        /// <summary>Perawat kedua mengonfirmasi atau menolak dosis high-alert.</summary>
        [HttpPatch("{id:guid}/double-check")]
        [ProducesResponseType(typeof(ApiResponse<MedicationAdministrationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("DoubleCheck", "Double Check Medication Administration", Description = "Perawat kedua memeriksa dosis obat high-alert", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("MedicationAdministration", "DoubleCheck")]
        public async Task<IActionResult> DoubleCheck(
            Guid id,
            [FromBody] DoubleCheckRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.DoubleCheckAsync(id, request, User, this.CurrentUserId(), cancellationToken));

        /// <summary>Koreksi hasil yang sudah tercatat; alasan wajib, nilai lama tersimpan sebagai revisi.</summary>
        [HttpPut("{id:guid}/correct")]
        [ProducesResponseType(typeof(ApiResponse<MedicationAdministrationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Medication Administration", Description = "Mengoreksi pemberian obat dan mencatat evaluasi PRN", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicationAdministration", "Update")]
        public async Task<IActionResult> Correct(
            Guid id,
            [FromBody] CorrectMedicationAdministrationRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.CorrectAsync(id, request, User, this.CurrentUserId(), cancellationToken));

        [HttpPatch("{id:guid}/prn-evaluation")]
        [ProducesResponseType(typeof(ApiResponse<MedicationAdministrationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Medication Administration", Description = "Mengoreksi pemberian obat dan mencatat evaluasi PRN", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MedicationAdministration", "Update")]
        public async Task<IActionResult> RecordPrnEvaluation(
            Guid id,
            [FromBody] PrnEvaluationRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.RecordPrnEvaluationAsync(id, request, User, this.CurrentUserId(), cancellationToken));
    }
}
