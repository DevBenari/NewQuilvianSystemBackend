using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-orders")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Order",
        AreaName = "HealthServices",
        ControllerName = "LabOrder",
        Description = "Pencatatan order pemeriksaan laboratorium",
        SortOrder = 1
    )]
    [Tags("Health Services / Laboratory Management / Lab Order")]
    public class LabOrderController : ControllerBase
    {
        private readonly LabOrderService _labOrderService;

        public LabOrderController(LabOrderService labOrderService)
        {
            _labOrderService = labOrderService;
        }

        // Keterangan bentuk layar daftar pesanan: pilihan status, disiplin, urutan, dan ukuran
        // halaman. Menyatakan terbuka bahwa daftar pesanan belum menyaring di sisi server.
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Order", Description = "Melihat daftar pilihan penyaring pesanan laboratorium", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrder", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = _labOrderService.GetFilterMetadata();

            return Ok(ApiResponse<LabOrderFilterMetadataResponse>.Ok(
                result,
                "Metadata penyaring pesanan laboratorium berhasil diambil."));
        }

        // Rekap pesanan pada satu rentang waktu. Bila rentangnya tidak dikirim, dipakai 30 hari
        // terakhir.
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Lab Order", Description = "Melihat rekap pesanan laboratorium", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrder", "Read")]
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

            var result = await _labOrderService.GetSummaryAsync(awal, akhir, cancellationToken);

            return Ok(ApiResponse<LabOrderSummaryResponse>.Ok(
                result,
                "Rekap pesanan laboratorium berhasil diambil."));
        }

        // Daftar pesanan dengan penyaring, pengurutan, dan pagination di sisi server.
        //
        // Penyaring encounterId membuat pemanggil yang hanya butuh pesanan satu pasien tidak
        // perlu lagi menarik seluruh tabel lalu menyaringnya sendiri di browser (IGD-DEC-105).
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabOrderListResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Lab Order", Description = "Melihat daftar order laboratorium", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrder", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabOrderPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            if (query.StartDate.HasValue && query.EndDate.HasValue &&
                query.StartDate.Value > query.EndDate.Value)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal awal tidak boleh melewati tanggal akhir."));
            }

            var result = await _labOrderService.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabOrderListResponse>>.Ok(
                result,
                "Daftar order laboratorium berhasil diambil."));
        }

        // Daftar pesanan satu disiplin. Disiplin datang dari jalur, bukan dari penyaring,
        // sehingga jalur Mikrobiologi tidak pernah dapat mengembalikan pesanan Patologi Klinik.
        //
        // Nilai disiplin yang tidak dikenal ditolak 400, bukan dikembalikan menjadi daftar
        // kosong — daftar kosong akan terbaca sebagai "belum ada pekerjaan", padahal yang
        // terjadi adalah salah ketik.
        [HttpGet("by-discipline/{discipline}")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabOrderListResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Lab Order", Description = "Melihat daftar order laboratorium per disiplin", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrder", "Read")]
        public async Task<IActionResult> GetByDiscipline(
            string discipline,
            [FromQuery] LabOrderPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            if (!TryParseDiscipline(discipline, out var nilai))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Disiplin laboratorium tidak dikenal. Pilihannya: clinical-pathology, anatomical-pathology, atau microbiology."));
            }

            if (query.StartDate.HasValue && query.EndDate.HasValue &&
                query.StartDate.Value > query.EndDate.Value)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal awal tidak boleh melewati tanggal akhir."));
            }

            query.Discipline = nilai;

            var result = await _labOrderService.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabOrderListResponse>>.Ok(
                result,
                "Daftar order laboratorium per disiplin berhasil diambil."));
        }

        /// <summary>
        /// Menerima nama enum maupun bentuk bertanda hubung yang dipakai jalur monitoring,
        /// supaya kedua grup dapat dipanggil dengan istilah yang sama.
        /// </summary>
        private static bool TryParseDiscipline(string? nilai, out LabDiscipline discipline)
        {
            discipline = default;

            if (string.IsNullOrWhiteSpace(nilai))
                return false;

            var bersih = nilai.Trim().Replace("-", string.Empty).Replace("_", string.Empty);

            return Enum.TryParse(bersih, ignoreCase: true, out discipline) &&
                   Enum.IsDefined(typeof(LabDiscipline), discipline);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Order", Description = "Melihat detail order laboratorium", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrder", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _labOrderService.GetDetailAsync(id, cancellationToken);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Order laboratorium tidak ditemukan."));
            }

            return Ok(ApiResponse<LabOrderDetailResponse>.Ok(
                result,
                "Detail order laboratorium berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabOrderDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Lab Order", Description = "Membuat order pemeriksaan laboratorium", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabOrder", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateLabOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labOrderService.CreateAsync(request, cancellationToken);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Id },
                    ApiResponse<LabOrderDetailResponse>.Ok(
                        result,
                        "Order laboratorium berhasil dibuat."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }
        }

        // Menandai pesanan mulai dikerjakan laboratorium.
        [HttpPut("{id:guid}/start-process")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Process Lab Order", Description = "Menandai order mulai dikerjakan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabOrder", "Process")]
        public Task<IActionResult> StartProcess(Guid id, CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labOrderService.StartProcessAsync(id, cancellationToken),
                "Order laboratorium mulai dikerjakan.");

        // Menandai pekerjaan laboratorium selesai. Tidak menerbitkan fakta tagihan; kelayakan
        // tagih sudah terbentuk pada saat sampel dinyatakan layak.
        [HttpPut("{id:guid}/complete")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Process Lab Order", Description = "Menandai order selesai dikerjakan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LabOrder", "Process")]
        public Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labOrderService.CompleteAsync(id, cancellationToken),
                "Order laboratorium selesai dikerjakan.");

        [HttpPut("{id:guid}/hold")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Hold Lab Order", Description = "Menahan order laboratorium", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LabOrder", "Hold")]
        public Task<IActionResult> Hold(
            Guid id,
            [FromBody] HoldLabRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labOrderService.HoldAsync(id, request, cancellationToken),
                "Order laboratorium ditahan.");

        [HttpPut("{id:guid}/resume")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Hold Lab Order", Description = "Melanjutkan order laboratorium yang ditahan", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LabOrder", "Hold")]
        public Task<IActionResult> Resume(
            Guid id,
            [FromBody] ResumeLabRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labOrderService.ResumeAsync(id, request, cancellationToken),
                "Order laboratorium dilanjutkan.");

        // Membatalkan order laboratorium beserta sampel yang masih berjalan.
        //
        // Pembatalan ini bersifat klinis. Untuk sampel yang sebelumnya sudah dinyatakan layak,
        // diterbitkan fakta pembatalan sebagai revisi baru sehingga tagihan lama tetap utuh
        // dan Billing yang menentukan koreksinya. Laboratorium tidak menghapus tagihan.
        [HttpPut("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderCancellationResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Cancel Lab Order", Description = "Membatalkan order laboratorium", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabOrder", "Update")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelLabSpecimenRequest? request = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labOrderService.CancelAsync(id, request, cancellationToken);

                return Ok(ApiResponse<LabOrderCancellationResult>.Ok(
                    result,
                    "Order laboratorium berhasil dibatalkan secara klinis."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    ex.Message));
            }
            catch (LabConcurrencyException ex)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }
        }

        /// <summary>
        /// Menjalankan satu perpindahan status dan menerjemahkan kegagalannya menjadi status
        /// HTTP yang tepat, tanpa membocorkan detail exception ke pemanggil.
        /// </summary>
        private async Task<IActionResult> ExecuteAsync(
            Func<Task<LabOrderDetailResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabOrderDetailResponse>.Ok(result, successMessage));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    ex.Message));
            }
            catch (LabConcurrencyException ex)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict,
                    ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }
        }
    }
}
