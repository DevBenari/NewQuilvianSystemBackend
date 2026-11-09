using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Controllers
{
    [ApiController]
    [Route("api/hr-deductions/settings")]
    [Tags("HR Deductions - Settings")]
    [Authorize]
    public class RestitutionSettingController : ControllerBase
    {
        private readonly IRestitutionSettingService _settingService;
        private readonly ILogger<RestitutionSettingController> _logger;

        public RestitutionSettingController(
            IRestitutionSettingService settingService,
            ILogger<RestitutionSettingController> logger)
        {
            _settingService = settingService;
            _logger = logger;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var uid) ? uid : Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        /// <summary>
        /// Mengambil seluruh konfigurasi kebijakan plafon tahunan dan batasan pemotongan upah.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllSettings([FromQuery] RestitutionSettingFilterDto filter, CancellationToken cancellationToken)
        {
            var result = await _settingService.GetAllSettingsAsync(filter, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Mengambil detail satu kebijakan berdasarkan ID.
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetSettingById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _settingService.GetSettingByIdAsync(id, cancellationToken);
            if (result == null) return NotFound(new { message = "Konfigurasi kebijakan tidak ditemukan." });
            return Ok(result);
        }

        /// <summary>
        /// Mengambil kebijakan aktif untuk grade dan kategori manfaat tertentu.
        /// </summary>
        [HttpGet("policy")]
        public async Task<IActionResult> GetPolicy([FromQuery] string gradeLevelCode, [FromQuery] string benefitCategory, CancellationToken cancellationToken)
        {
            var result = await _settingService.GetPolicyForGradeAndCategoryAsync(gradeLevelCode, benefitCategory, cancellationToken);
            if (result == null) return NotFound(new { message = "Kebijakan untuk kombinasi grade dan benefit tersebut tidak ditemukan." });
            return Ok(result);
        }

        /// <summary>
        /// Membuat atau memperbarui konfigurasi kebijakan plafon dan batas pemotongan upah.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateOrUpdateSetting([FromBody] CreateRestitutionSettingRequestDto dto, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _settingService.CreateOrUpdateSettingAsync(dto, GetCurrentUserId(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Menyemai pengaturan default rumah sakit untuk seluruh grade (Grade-All s.d. Grade-IV).
        /// </summary>
        [HttpPost("seed-defaults")]
        public async Task<IActionResult> SeedDefaultSettings(CancellationToken cancellationToken)
        {
            var inserted = await _settingService.SeedDefaultHospitalSettingsAsync(GetCurrentUserId(), cancellationToken);
            return Ok(new { message = $"Berhasil menyemai {inserted} pengaturan default rumah sakit." });
        }
    }
}
