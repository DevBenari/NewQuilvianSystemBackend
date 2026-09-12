using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.DTOs;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

using ResponseNumberSeriesPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.DTOs.NumberSeriesResponse>;

namespace QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Controllers
{
    /// <summary>
    /// Layar pemantauan deret nomor bisnis (<c>PLT-BE-005</c>, <c>FR-PLT-012</c>,
    /// <c>FR-PLT-013</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Keempat endpoint di sini hanya membaca, dan tidak akan pernah bertambah menjadi lima.</b>
    /// Ketiadaan <c>POST</c>, <c>PUT</c>, <c>PATCH</c>, dan <c>DELETE</c> adalah keputusan kontrak
    /// (<c>api-contract.md</c> §2), bukan pekerjaan yang belum sempat dibuat:
    /// </para>
    /// <list type="bullet">
    /// <item><description>
    /// <c>POST /</c> tidak ada karena deret lahir sendiri pada alokasi pertama.
    /// </description></item>
    /// <item><description>
    /// <c>PUT</c> dan <c>PATCH</c> tidak ada karena menyunting pencacah berarti menerbitkan ulang
    /// nomor yang sudah menempel pada catatan lain — <c>INV-PLT-001</c>.
    /// </description></item>
    /// <item><description>
    /// <c>DELETE /{id}</c> tidak ada karena menghapus baris deret menghilangkan nilai tertinggi
    /// yang pernah terbit, sehingga alokasi berikutnya mengulang dari nol.
    /// </description></item>
    /// <item><description>
    /// <c>POST /{id}/reset</c> tidak ada karena deret berlubang adalah keadaan sah
    /// (<c>INV-PLT-002</c>); menyetel ulang justru merusaknya.
    /// </description></item>
    /// <item><description>
    /// <c>GET /options</c> tidak ada karena deret bukan isi kotak pilihan — ia bukan master data.
    /// </description></item>
    /// </list>
    /// <para>
    /// <b>Alokasi nomor sengaja tidak dipaparkan lewat HTTP sama sekali.</b> Nomor yang dapat
    /// diminta lewat HTTP dapat terbit tanpa catatan yang menempel padanya. Alokasi karena itu
    /// tetap berupa panggilan dalam proses lewat <see cref="NumberSeriesAllocator"/>, dijaga butir
    /// hak akses milik pekerjaan yang membuat catatannya — bukan butir tersendiri.
    /// </para>
    /// <para>
    /// <b>Satu butir hak akses untuk keempatnya: <c>NumberSeries : Read</c>.</b> Tidak ada butir
    /// <c>Create</c>, <c>Update</c>, maupun <c>Delete</c>, karena tidak ada endpoint yang menulis;
    /// mendaftarkan butir yang tidak menjaga apa pun hanya melahirkan baris yatim yang dapat
    /// dicentang admin tetapi tidak berpengaruh.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/platform/number-series-management/number-series")]
    [AccessController(
        moduleCode: "PLATFORM_NUMBER_SERIES_MANAGEMENT",
        moduleName: "Platform Number Series Management",
        displayName: "Number Series",
        AreaName = "Platform",
        ControllerName = "NumberSeries",
        Description = "Pemantauan deret nomor bisnis bersama",
        SortOrder = 1
    )]
    [Tags("Platform / Number Series Management / Number Series")]
    public class NumberSeriesController : ControllerBase
    {
        private readonly NumberSeriesQueryService _queryService;

        public NumberSeriesController(NumberSeriesQueryService queryService)
        {
            _queryService = queryService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<NumberSeriesFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Number Series",
            Description = "Melihat metadata filter deret nomor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("NumberSeries", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = _queryService.GetFilterMetadata();

            return Ok(ApiResponse<NumberSeriesFilterMetadataResponse>.Ok(
                result,
                "Metadata filter deret nomor berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<NumberSeriesSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Number Series",
            Description = "Melihat ringkasan deret nomor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("NumberSeries", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var result = await _queryService.GetSummaryAsync(cancellationToken);

            return Ok(ApiResponse<NumberSeriesSummaryResponse>.Ok(
                result,
                "Ringkasan deret nomor berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseNumberSeriesPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Number Series",
            Description = "Melihat daftar deret nomor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("NumberSeries", "Read")]
        public async Task<IActionResult> GetNumberSeries(
            [FromQuery] string? search,
            [FromQuery] string? sequenceKey,
            [FromQuery] string? scopeKey,
            [FromQuery] string? resetPolicy,
            [FromQuery] string? sortBy = "sequenceKey",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _queryService.GetPagedAsync(
                new NumberSeriesPagedQuery
                {
                    Search = search,
                    SequenceKey = sequenceKey,
                    ScopeKey = scopeKey,
                    ResetPolicy = resetPolicy,
                    SortBy = sortBy,
                    SortDirection = sortDirection,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                },
                cancellationToken);

            return Ok(ApiResponse<ResponseNumberSeriesPagedResult>.Ok(
                result,
                "Data deret nomor berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NumberSeriesResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Number Series",
            Description = "Melihat detail deret nomor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("NumberSeries", "Read")]
        public async Task<IActionResult> GetNumberSeriesById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _queryService.GetByIdAsync(id, cancellationToken);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Deret nomor tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<NumberSeriesResponse>.Ok(
                result,
                "Detail deret nomor berhasil diambil."
            ));
        }
    }
}
