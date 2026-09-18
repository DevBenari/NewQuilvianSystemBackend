using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers
{
    /// <summary>
    /// Pengelolaan data induk jenis specimen (<c>LAB-DEC-040</c>, <c>BR-35</c>).
    ///
    /// Dua audiens dilayani dari base URL yang sama, dan pemisahannya disengaja.
    /// <c>GET /options</c> melayani petugas penerimaan: ia <b>hanya</b> mengembalikan jenis
    /// yang aktif, sehingga petugas tidak pernah dapat memilih jenis yang sudah ditarik dari
    /// peredaran. <c>GET /</c> melayani kepala instalasi: ia memuat yang nonaktif juga, karena
    /// tanpa itu jenis yang dinonaktifkan tidak akan pernah dapat diaktifkan kembali.
    ///
    /// Tidak ada penghapusan di sini. Jenis yang pernah menempel pada wadah adalah bagian
    /// riwayat penerimaan; ia dinonaktifkan lewat <c>PUT /{id}/activation</c>, bukan dihapus.
    ///
    /// Satu baris punya perlakuan khusus: jenis berpenanda <c>Lainnya</c>. Ia adalah jalan
    /// keluar ketika jenis sampel yang datang belum terdaftar, sehingga hanya satu yang boleh
    /// aktif (<c>VAL-62</c>) dan ia tidak boleh dinonaktifkan selama satu-satunya
    /// (<c>VAL-63</c>).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-specimen-types")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Specimen Type",
        AreaName = "HealthServices",
        ControllerName = "LabSpecimenType",
        Description = "Pengelolaan data induk jenis specimen laboratorium",
        SortOrder = 11
    )]
    [Tags("Health Services / Laboratory Management / Lab Specimen Type")]
    public class LabSpecimenTypeController : ControllerBase
    {
        private readonly LabSpecimenTypeService _labSpecimenTypeService;

        public LabSpecimenTypeController(LabSpecimenTypeService labSpecimenTypeService)
        {
            _labSpecimenTypeService = labSpecimenTypeService;
        }

        // Keterangan bentuk layar pengelolaan: pilihan urutan, ukuran halaman, dan penanda
        // bahwa jenis specimen tidak dapat dihapus maupun disetel penanda Lainnya-nya, sehingga
        // layar dapat menampilkan keduanya bergembok sejak awal.
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenTypeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Specimen Type", Description = "Melihat daftar pilihan penyaring jenis specimen", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabSpecimenType", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = _labSpecimenTypeService.GetFilterMetadata();

            return Ok(ApiResponse<LabSpecimenTypeFilterMetadataResponse>.Ok(
                result,
                "Metadata penyaring jenis specimen berhasil diambil."));
        }

        // Rekap jenis specimen. Tanpa rentang waktu; ini data induk, bukan catatan kejadian.
        //
        // Angka JalanKeluarLainnyaAktif sengaja ikut ditampilkan: nilai 0 berarti sampel yang
        // jenisnya belum terdaftar akan tertahan di meja penerimaan.
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenTypeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Specimen Type", Description = "Melihat rekap jenis specimen", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabSpecimenType", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var result = await _labSpecimenTypeService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<LabSpecimenTypeSummaryResponse>.Ok(
                result,
                "Rekap jenis specimen berhasil diambil."));
        }

        // Daftar jenis specimen untuk layar pengelolaan kepala instalasi. Memuat yang nonaktif
        // juga, karena tanpa itu jenis yang dinonaktifkan tidak akan pernah dapat diaktifkan
        // kembali.
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabSpecimenTypeResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Specimen Type", Description = "Melihat daftar jenis specimen", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabSpecimenType", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabSpecimenTypePagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _labSpecimenTypeService.GetListAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabSpecimenTypeResponse>>.Ok(
                result,
                "Daftar jenis specimen berhasil diambil."));
        }

        // Daftar pilihan untuk layar penerimaan sampel. Hanya jenis aktif yang dikembalikan
        // (VAL-55), dan bentuknya ringan karena dipakai sebagai isi kotak pilihan.
        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabSpecimenTypeOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Specimen Type", Description = "Melihat daftar pilihan jenis specimen", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabSpecimenType", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] LabSpecimenTypeOptionQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _labSpecimenTypeService.GetOptionsAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabSpecimenTypeOptionResponse>>.Ok(
                result,
                "Daftar pilihan jenis specimen berhasil diambil."));
        }

        // Daftar pantau pemakaian Lainnya: keterangan apa saja yang ditulis petugas, berapa kali
        // masing-masing muncul, dan kapan terakhir dipakai. Dari sini kepala instalasi menaikkan
        // keterangan yang sering berulang menjadi jenis specimen tetap lewat POST /.
        //
        // Tidak ada tabel ringkasan di belakangnya; rekapnya diturunkan langsung dari wadah,
        // sehingga angkanya berubah seketika begitu wadah baru dicatat.
        [HttpGet("other-usage")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabSpecimenOtherUsageResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Lab Specimen Type", Description = "Melihat daftar pantau pemakaian jenis Lainnya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabSpecimenType", "Read")]
        public async Task<IActionResult> GetOtherUsage(
            [FromQuery] LabSpecimenOtherUsageQuery query,
            CancellationToken cancellationToken = default)
        {
            // Rentang disiapkan sebelum apa pun yang lain — lihat LabQueryDateRange.
            (query.StartDate, query.EndDate) =
                LabQueryDateRange.Normalize(query.StartDate, query.EndDate);

            if (query.StartDate.HasValue && query.EndDate.HasValue && query.StartDate > query.EndDate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal awal tidak boleh melewati tanggal akhir."));
            }

            var result = await _labSpecimenTypeService.GetOtherUsageAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabSpecimenOtherUsageResponse>>.Ok(
                result,
                "Daftar pantau pemakaian jenis Lainnya berhasil diambil."));
        }

        // Detail satu jenis specimen. Jalur ini yang dipakai formulir ubah saat dibuka lewat
        // tautan langsung atau sesudah halaman disegarkan, sehingga formulirnya tidak terbuka
        // kosong tanpa pesan.
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Specimen Type", Description = "Melihat detail satu jenis specimen", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabSpecimenType", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labSpecimenTypeService.GetByIdAsync(id, cancellationToken);

                return Ok(ApiResponse<LabSpecimenTypeResponse>.Ok(
                    result,
                    "Detail jenis specimen berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
        }

        // Menambah jenis specimen baru. Kode dinormalkan menjadi huruf kapital dan wajib unik
        // (VAL-61).
        //
        // Penanda Lainnya TIDAK dapat diisi dari sini: jenis baru selalu lahir sebagai jenis
        // biasa, karena hanya satu baris Lainnya yang boleh aktif (VAL-62).
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Lab Specimen Type", Description = "Menambah jenis specimen", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LabSpecimenType", "Create")]
        public Task<IActionResult> Create(
            [FromBody] CreateLabSpecimenTypeRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labSpecimenTypeService.CreateAsync(request, cancellationToken),
                "Jenis specimen berhasil ditambahkan.");

        // Mengubah nama, keterangan, dan urutan tampil.
        //
        // Kode jenis tidak ikut berubah karena ia menjadi penanda yang dirujuk seeder dan
        // pemanggil lain. Penanda Lainnya yang disertakan dan berbeda dari nilai tersimpan
        // ditolak 422 (VAL-62), bukan diabaikan diam-diam.
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Lab Specimen Type", Description = "Mengubah jenis specimen", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabSpecimenType", "Update")]
        public Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateLabSpecimenTypeRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labSpecimenTypeService.UpdateAsync(id, request, cancellationToken),
                "Jenis specimen berhasil diubah.");

        // Mengaktifkan atau menonaktifkan satu jenis specimen. Jenis Lainnya yang aktif tidak
        // dapat dinonaktifkan selama ia satu-satunya (VAL-63), karena ia jalan keluar ketika
        // jenis sampel yang datang belum terdaftar.
        [HttpPut("{id:guid}/activation")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenTypeResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Lab Specimen Type", Description = "Mengaktifkan atau menonaktifkan jenis specimen", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LabSpecimenType", "Update")]
        public Task<IActionResult> SetActivation(
            Guid id,
            [FromBody] SetLabSpecimenTypeActivationRequest request,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(
                () => _labSpecimenTypeService.SetActivationAsync(id, request, cancellationToken),
                request.IsActive
                    ? "Jenis specimen berhasil diaktifkan."
                    : "Jenis specimen berhasil dinonaktifkan.");

        /// <summary>
        /// Menjalankan satu perubahan dan menerjemahkan kegagalannya menjadi status HTTP yang
        /// tepat, tanpa membocorkan detail exception ke pemanggil.
        /// </summary>
        private async Task<IActionResult> ExecuteAsync(
            Func<Task<LabSpecimenTypeResponse>> action,
            string successMessage)
        {
            try
            {
                var result = await action();

                return Ok(ApiResponse<LabSpecimenTypeResponse>.Ok(result, successMessage));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabSpecimenTypeConflictException exception)
            {
                return Conflict(ApiResponse<object>.Fail(
                    StatusCodes.Status409Conflict, exception.Message));
            }
            catch (LabSpecimenTypeValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
            catch (ArgumentException exception)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, exception.Message));
            }
        }
    }
}
