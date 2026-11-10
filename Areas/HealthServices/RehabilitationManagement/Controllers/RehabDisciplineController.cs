using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants.RehabilitationPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Controllers
{
    /// <summary>
    /// Konfigurasi layanan rehab: disiplin, profesinya, tindakan yang boleh dipakai, dan jenis
    /// instrumen asesmen — <c>BE-RHB-005</c>, <c>contracts/api-contract.md</c> v1 grup Rehab
    /// Discipline.
    /// </summary>
    /// <remarks>
    /// Sembilan endpoint baseline master data ditambah sub-resource <c>/{id}/procedures</c>,
    /// <c>/{id}/instruments</c>, dan <c>/medical-instruments</c> (instrumen asesmen medis
    /// Sp.KFR, tanpa disiplin). Seluruh CRUD dan aturan berada di
    /// <see cref="RhbConfigurationService"/>; controller ini tidak menyentuh
    /// <c>ApplicationDbContext</c> (QBE-SVC-001).
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/rehabilitation-management/rehab-disciplines")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Rehab Configuration",
        AreaName = P.AreaName,
        ControllerName = P.Configuration.Resource,
        Description = "Konfigurasi layanan Rehab Medik: disiplin, profesi, tindakan, dan instrumen",
        SortOrder = 1
    )]
    [Tags("Health Services / Rehabilitation Management / Rehab Discipline")]
    public class RehabDisciplineController : ControllerBase
    {
        private readonly RhbConfigurationService _service;
        private readonly LoggerService _logger;

        public RehabDisciplineController(RhbConfigurationService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<RehabDisciplineFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Rehab Configuration", Description = "Melihat konfigurasi penyaring layanan rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Configuration.Resource, P.Read)]
        public IActionResult GetFilterMetadata() =>
            Ok(ApiResponse<RehabDisciplineFilterMetadataResponse>.Ok(
                RhbConfigurationService.BuildFilterMetadata(), "Metadata filter layanan rehab berhasil diambil."));

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<RehabDisciplineSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Rehab Configuration", Description = "Melihat ringkasan layanan rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Configuration.Resource, P.Read)]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<RehabDisciplineSummaryResponse>.Ok(
                await _service.GetSummaryAsync(cancellationToken), "Ringkasan layanan rehab berhasil diambil."));

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RehabDisciplineListItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Read, "Read Rehab Configuration", Description = "Melihat daftar layanan rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Configuration.Resource, P.Read)]
        public async Task<IActionResult> GetList([FromQuery] RehabDisciplineQuery query, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetListAsync(query, cancellationToken), "Daftar layanan rehab berhasil diambil.");

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabDisciplineOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Rehab Configuration", Description = "Melihat pilihan layanan rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Configuration.Resource, P.Read)]
        public async Task<IActionResult> GetOptions([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<List<RehabDisciplineOptionResponse>>.Ok(
                await _service.GetOptionsAsync(onlyActive, cancellationToken), "Pilihan layanan rehab berhasil diambil."));

        [HttpGet("medical-instruments")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabDisciplineInstrumentResponse>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, "Read Rehab Configuration", Description = "Melihat jenis instrumen asesmen medis Sp.KFR", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Configuration.Resource, P.Read)]
        public async Task<IActionResult> GetMedicalInstruments(CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetInstrumentsAsync(null, cancellationToken), "Instrumen asesmen medis berhasil diambil.");

        [HttpPut("medical-instruments")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabDisciplineInstrumentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Update, "Update Rehab Configuration", Description = "Mengubah layanan rehab beserta tindakan dan instrumennya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Configuration.Resource, P.Update)]
        public async Task<IActionResult> SaveMedicalInstruments([FromBody] SaveRehabDisciplineInstrumentsRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.SaveInstrumentsAsync(null, request, RhbHttp.ActorId(User), cancellationToken),
                "Instrumen asesmen medis berhasil disimpan.", _logger, P.Configuration.Resource, P.Update,
                x => new { Scope = "Medical", ActiveCount = x.Count(i => i.IsActive) });

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabDisciplineDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Rehab Configuration", Description = "Melihat rincian layanan rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Configuration.Resource, P.Read)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetDetailAsync(id, cancellationToken);
            return result == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Layanan rehab tidak ditemukan."))
                : Ok(ApiResponse<RehabDisciplineDetailResponse>.Ok(result, "Rincian layanan rehab berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<RehabDisciplineDetailResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Create, "Create Rehab Configuration", Description = "Menambah layanan rehab", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission(P.Configuration.Resource, P.Create)]
        public async Task<IActionResult> Create([FromBody] CreateRehabDisciplineRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.CreateAsync(request, RhbHttp.ActorId(User), cancellationToken),
                "Layanan rehab berhasil ditambahkan.", _logger, P.Configuration.Resource, P.Create,
                x => new { x.Id, x.DisciplineCode, x.IsActive });

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabDisciplineDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, "Update Rehab Configuration", Description = "Mengubah layanan rehab beserta tindakan dan instrumennya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Configuration.Resource, P.Update)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRehabDisciplineRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.UpdateAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Layanan rehab berhasil diperbarui.", _logger, P.Configuration.Resource, P.Update,
                x => new { x.Id, x.DisciplineCode, x.ProfessionId });

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<RehabDisciplineDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, "Update Rehab Configuration", Description = "Mengubah layanan rehab beserta tindakan dan instrumennya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Configuration.Resource, P.Update)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateRehabDisciplineStatusRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.SetStatusAsync(id, request.IsActive, RhbHttp.ActorId(User), cancellationToken),
                "Status layanan rehab berhasil diperbarui.", _logger, P.Configuration.Resource, P.Update,
                x => new { x.Id, x.DisciplineCode, x.IsActive });

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RehabDisciplineDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Delete, "Delete Rehab Configuration", Description = "Menghapus layanan rehab yang belum pernah dipakai", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission(P.Configuration.Resource, P.Delete)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.DeleteAsync(id, RhbHttp.ActorId(User), cancellationToken),
                "Layanan rehab berhasil dihapus.", _logger, P.Configuration.Resource, P.Delete,
                x => new { x.Id, x.DisciplineCode });

        [HttpGet("{id:guid}/procedures")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabDisciplineProcedureResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Rehab Configuration", Description = "Melihat tindakan yang boleh dipakai layanan rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Configuration.Resource, P.Read)]
        public async Task<IActionResult> GetProcedures(Guid id, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetProceduresAsync(id, cancellationToken), "Tindakan layanan rehab berhasil diambil.");

        [HttpPut("{id:guid}/procedures")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabDisciplineProcedureResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction(P.Update, "Update Rehab Configuration", Description = "Mengubah layanan rehab beserta tindakan dan instrumennya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Configuration.Resource, P.Update)]
        public async Task<IActionResult> SaveProcedures(Guid id, [FromBody] SaveRehabDisciplineProceduresRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.SaveProceduresAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Tindakan layanan rehab berhasil disimpan.", _logger, P.Configuration.Resource, P.Update,
                x => new { DisciplineId = id, ActiveCount = x.Count(p => p.IsActive) });

        [HttpGet("{id:guid}/instruments")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabDisciplineInstrumentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, "Read Rehab Configuration", Description = "Melihat jenis instrumen asesmen layanan rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Configuration.Resource, P.Read)]
        public async Task<IActionResult> GetInstruments(Guid id, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetInstrumentsAsync(id, cancellationToken), "Instrumen layanan rehab berhasil diambil.");

        [HttpPut("{id:guid}/instruments")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabDisciplineInstrumentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Update, "Update Rehab Configuration", Description = "Mengubah layanan rehab beserta tindakan dan instrumennya", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission(P.Configuration.Resource, P.Update)]
        public async Task<IActionResult> SaveInstruments(Guid id, [FromBody] SaveRehabDisciplineInstrumentsRequest request, CancellationToken cancellationToken = default) =>
            await RhbHttp.RespondAsync(this,
                await _service.SaveInstrumentsAsync(id, request, RhbHttp.ActorId(User), cancellationToken),
                "Instrumen layanan rehab berhasil disimpan.", _logger, P.Configuration.Resource, P.Update,
                x => new { DisciplineId = id, ActiveCount = x.Count(i => i.IsActive) });
    }
}
