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
    /// Katalog pemeriksaan, harga, dan cakupan penjamin.
    ///
    /// <b>Seluruh grup ini baca saja.</b> Tidak ada <c>POST</c>, <c>PUT</c>, maupun
    /// <c>DELETE</c> — dan ketiadaan itu disengaja, bukan kebetulan. Tarif tetap milik Master
    /// Data (<c>LAB-DEC-033</c>); mengubahnya lewat modul Laboratorium tidak mungkin karena
    /// jalurnya memang tidak pernah dibuat (<c>VAL-50</c>, <c>AC-48</c>).
    ///
    /// Laboratorium juga tidak memiliki tabel tarif sendiri. Harga yang tampil di sini selalu
    /// berasal dari <c>MstTariff</c> dan <c>MstInsuranceTariff</c> (<c>AC-47</c>).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-catalog")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Catalog",
        AreaName = "HealthServices",
        ControllerName = "LabCatalog",
        Description = "Katalog pemeriksaan, harga, dan cakupan penjamin laboratorium",
        SortOrder = 9
    )]
    [Tags("Health Services / Laboratory Management / Lab Catalog")]
    public class LabCatalogController : ControllerBase
    {
        private readonly LabCatalogService _labCatalogService;

        public LabCatalogController(LabCatalogService labCatalogService)
        {
            _labCatalogService = labCatalogService;
        }

        // Daftar pemeriksaan yang dapat dipesan, disaring per disiplin.
        //
        // Pemeriksaan yang belum digolongkan disiplinnya tetap tampil bila penyaring disiplin
        // tidak dikirim — menyembunyikannya membuat katalog tampak kosong pada rumah sakit yang
        // penggolongannya belum diisi.
        [HttpGet("examinations")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabCatalogItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Catalog", Description = "Melihat katalog pemeriksaan laboratorium", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabCatalog", "Read")]
        public async Task<IActionResult> GetExaminations(
            [FromQuery] LabCatalogQuery query,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _labCatalogService.GetExaminationsAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabCatalogItemResponse>>.Ok(
                hasil, "Katalog pemeriksaan laboratorium berhasil diambil."));
        }

        // Harga berlaku dan status cakupan penjamin untuk satu pemeriksaan.
        //
        // Membacanya tidak membentuk baris tagihan apa pun (AC-43): melihat harga bukan
        // memesan, dan memesan bukan menagih.
        [HttpGet("examinations/{procedureId:guid}/price")]
        [ProducesResponseType(typeof(ApiResponse<LabPriceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Read", "Read Lab Catalog", Description = "Melihat harga dan cakupan penjamin satu pemeriksaan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabCatalog", "Read")]
        public async Task<IActionResult> GetPrice(
            Guid procedureId,
            [FromQuery] LabPriceQuery query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var hasil = await _labCatalogService.GetPriceAsync(procedureId, query, cancellationToken);

                return Ok(ApiResponse<LabPriceResponse>.Ok(hasil, "Harga pemeriksaan berhasil diambil."));
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, exception.Message));
            }
            catch (LabCatalogValidationException exception)
            {
                return UnprocessableEntity(ApiResponse<object>.Fail(
                    StatusCodes.Status422UnprocessableEntity, exception.Message));
            }
        }

        // Tampilan tersaring daftar tarif pemeriksaan laboratorium — baca saja.
        [HttpGet("tariffs")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabTariffViewResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Catalog", Description = "Melihat tarif pemeriksaan laboratorium", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabCatalog", "Read")]
        public async Task<IActionResult> GetTariffs(
            [FromQuery] LabTariffQuery query,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _labCatalogService.GetTariffsAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabTariffViewResponse>>.Ok(
                hasil, "Daftar tarif pemeriksaan laboratorium berhasil diambil."));
        }
    }
}
