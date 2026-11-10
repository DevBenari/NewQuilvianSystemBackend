using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using System.Security.Claims;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/hrd/services")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_SERVICE_MANAGEMENT",
        moduleName: "Human Resource Service Management",
        displayName: "Katalog Layanan SDM",
        AreaName = "Corporate",
        ControllerName = "HrServiceCatalog",
        Description = "Kategori layanan, tipe layanan dan SLA, serta master jenis dokumen",
        SortOrder = 11)]
    [Tags("HR Services - Catalog & SLA")]
    [Authorize]
    public class HrServiceCatalogController : ControllerBase
    {
        private readonly IHrServiceCatalogService _catalogService;

        public HrServiceCatalogController(IHrServiceCatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        #region Kategori Layanan

        /// <summary>
        /// Mengambil seluruh kategori layanan SDM yang aktif (FR-HRS-01)
        /// </summary>
        [HttpGet("categories")]
        [AccessAction("Read", "Melihat Katalog Layanan SDM", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrServiceCatalog", "Read")]
        public async Task<IActionResult> GetCategories([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            var categories = await _catalogService.GetActiveCategoriesAsync(includeInactive, cancellationToken);
            return Ok(categories);
        }

        /// <summary>
        /// Mengambil detail satu kategori layanan SDM berdasarkan ID
        /// </summary>
        [HttpGet("categories/{id:guid}")]
        [AccessAction("Read", "Melihat Katalog Layanan SDM", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrServiceCatalog", "Read")]
        public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken cancellationToken = default)
        {
            var cat = await _catalogService.GetCategoryByIdAsync(id, cancellationToken);
            if (cat == null) return NotFound(new { message = "Kategori layanan tidak ditemukan" });
            return Ok(cat);
        }

        /// <summary>
        /// Membuat master kategori layanan SDM baru (VR-HRS-03)
        /// </summary>
        [HttpPost("categories")]
        [AccessAction("Create", "Membuat Katalog Layanan SDM", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("HrServiceCatalog", "Create")]
        [Authorize(Roles = "SuperAdmin,HRAdmin,HRManager")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateHrServiceCategoryRequestDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
            try
            {
                var created = await _catalogService.CreateCategoryAsync(request, currentUserId, cancellationToken);
                return CreatedAtAction(nameof(GetCategoryById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Memperbarui konfigurasi kategori layanan SDM
        /// </summary>
        [HttpPut("categories/{id:guid}")]
        [AccessAction("Update", "Mengubah Katalog Layanan SDM", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("HrServiceCatalog", "Update")]
        [Authorize(Roles = "SuperAdmin,HRAdmin,HRManager")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateHrServiceCategoryRequestDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
            try
            {
                var updated = await _catalogService.UpdateCategoryAsync(id, request, currentUserId, cancellationToken);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Menghapus kategori layanan SDM (soft-delete)
        /// </summary>
        [HttpDelete("categories/{id:guid}")]
        [AccessAction("Delete", "Menghapus Katalog Layanan SDM", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("HrServiceCatalog", "Delete")]
        [Authorize(Roles = "SuperAdmin,HRManager")]
        public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken = default)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
            try
            {
                var deleted = await _catalogService.DeleteCategoryAsync(id, currentUserId, cancellationToken);
                if (!deleted) return NotFound(new { message = "Kategori layanan tidak ditemukan" });
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Tipe Layanan

        /// <summary>
        /// Mengambil daftar tipe layanan SDM dengan filter kategori
        /// </summary>
        [HttpGet("types")]
        [AccessAction("Read", "Melihat Katalog Layanan SDM", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrServiceCatalog", "Read")]
        public async Task<IActionResult> GetServiceTypes([FromQuery] Guid? categoryId = null, [FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
        {
            var types = await _catalogService.GetServiceTypesAsync(categoryId, activeOnly, cancellationToken);
            return Ok(types);
        }

        /// <summary>
        /// Mengambil detail satu tipe layanan SDM berdasarkan ID
        /// </summary>
        [HttpGet("types/{id:guid}")]
        [AccessAction("Read", "Melihat Katalog Layanan SDM", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrServiceCatalog", "Read")]
        public async Task<IActionResult> GetServiceTypeById(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _catalogService.GetServiceTypeByIdAsync(id, cancellationToken);
            if (item == null) return NotFound(new { message = "Tipe layanan tidak ditemukan" });
            return Ok(item);
        }

        /// <summary>
        /// Membuat konfigurasi tipe layanan SDM baru lengkap dengan target durasi SLA
        /// </summary>
        [HttpPost("types")]
        [AccessAction("Create", "Membuat Katalog Layanan SDM", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("HrServiceCatalog", "Create")]
        [Authorize(Roles = "SuperAdmin,HRAdmin,HRManager")]
        public async Task<IActionResult> CreateServiceType([FromBody] CreateHrServiceTypeRequestDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
            try
            {
                var created = await _catalogService.CreateServiceTypeAsync(request, currentUserId, cancellationToken);
                return CreatedAtAction(nameof(GetServiceTypeById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Memperbarui konfigurasi tipe layanan SDM
        /// </summary>
        [HttpPut("types/{id:guid}")]
        [AccessAction("Update", "Mengubah Katalog Layanan SDM", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("HrServiceCatalog", "Update")]
        [Authorize(Roles = "SuperAdmin,HRAdmin,HRManager")]
        public async Task<IActionResult> UpdateServiceType(Guid id, [FromBody] UpdateHrServiceTypeRequestDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
            try
            {
                var updated = await _catalogService.UpdateServiceTypeAsync(id, request, currentUserId, cancellationToken);
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Menghapus tipe layanan SDM (soft-delete)
        /// </summary>
        [HttpDelete("types/{id:guid}")]
        [AccessAction("Delete", "Menghapus Katalog Layanan SDM", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("HrServiceCatalog", "Delete")]
        [Authorize(Roles = "SuperAdmin,HRManager")]
        public async Task<IActionResult> DeleteServiceType(Guid id, CancellationToken cancellationToken = default)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
            try
            {
                var deleted = await _catalogService.DeleteServiceTypeAsync(id, currentUserId, cancellationToken);
                if (!deleted) return NotFound(new { message = "Tipe layanan tidak ditemukan" });
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion

        #region Master Template Dokumen Resmi

        /// <summary>
        /// Mengambil daftar master template naskah dinas resmi rumah sakit
        /// </summary>
        [HttpGet("document-types")]
        [AccessAction("Read", "Melihat Katalog Layanan SDM", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrServiceCatalog", "Read")]
        public async Task<IActionResult> GetDocumentTypes([FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
        {
            var types = await _catalogService.GetDocumentTypesAsync(activeOnly, cancellationToken);
            return Ok(types);
        }

        /// <summary>
        /// Mengambil detail master template dokumen resmi berdasarkan ID
        /// </summary>
        [HttpGet("document-types/{id:guid}")]
        [AccessAction("Read", "Melihat Katalog Layanan SDM", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HrServiceCatalog", "Read")]
        public async Task<IActionResult> GetDocumentTypeById(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _catalogService.GetDocumentTypeByIdAsync(id, cancellationToken);
            if (item == null) return NotFound(new { message = "Template dokumen tidak ditemukan" });
            return Ok(item);
        }

        /// <summary>
        /// Mendaftarkan master template naskah dinas resmi baru
        /// </summary>
        [HttpPost("document-types")]
        [AccessAction("Create", "Membuat Katalog Layanan SDM", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("HrServiceCatalog", "Create")]
        [Authorize(Roles = "SuperAdmin,HRAdmin,HRManager")]
        public async Task<IActionResult> CreateDocumentType([FromBody] CreateEmployeeDocumentTypeRequestDto request, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "SYSTEM";
            try
            {
                var created = await _catalogService.CreateDocumentTypeAsync(request, currentUserId, cancellationToken);
                return CreatedAtAction(nameof(GetDocumentTypeById), new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        #endregion
    }
}
