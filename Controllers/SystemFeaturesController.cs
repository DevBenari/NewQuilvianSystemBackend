using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.DTOs.System;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Features;

namespace QuilvianSystemBackend.Controllers
{
    /// <summary>
    /// Status fitur yang dikendalikan Backend (saat ini: BPJS). Frontend membacanya untuk menyembunyikan konfigurasi dan jalur BPJS
    /// yang dinonaktifkan sementara, tanpa menyediakan pengaturan BPJS baru. Hanya perlu login: nilainya bukan data sensitif dan
    /// semua modul (Payroll, HRD, K3) membutuhkannya, jadi tidak diikat ke izin modul tertentu. Murni baca.
    /// </summary>
    [ApiController]
    [Authorize]
    [Tags("02-System Features")]
    public class SystemFeaturesController : ControllerBase
    {
        private readonly IBpjsFeatureGate _bpjsGate;

        public SystemFeaturesController(IBpjsFeatureGate bpjsGate)
        {
            _bpjsGate = bpjsGate;
        }

        [HttpGet("/api/v1/system/features")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(ApiResponse<SystemFeaturesResponse>), StatusCodes.Status200OK)]
        public IActionResult GetFeatures()
        {
            var response = new SystemFeaturesResponse
            {
                BpjsEnabled = _bpjsGate.IsEnabled,
                BpjsDisabledMessage = _bpjsGate.IsEnabled ? null : _bpjsGate.DisabledMessage
            };

            return Ok(ApiResponse<SystemFeaturesResponse>.Ok(response, "Status fitur berhasil diambil."));
        }
    }
}
