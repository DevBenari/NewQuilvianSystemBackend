using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using P = QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Constants.RehabilitationPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Controllers
{
    /// <summary>
    /// Pilihan untuk form rehab — <c>BE-RHB-006</c>, <c>contracts/api-contract.md</c> v1 grup Rehab
    /// Lookup. Seluruhnya <c>GET</c> baca saja di bawah satu hak akses <c>RehabLookup : Read</c>.
    /// </summary>
    /// <remarks>
    /// Petugas rehab memilih kunjungan dan pasien lewat endpoint ini, bukan lewat modul pendaftaran
    /// (RHB-DEC-016). Endpoint baca tidak dicatat ke logger (<c>permission-audit-matrix.md</c>
    /// bagian 6). Controller ini tidak menyentuh <c>ApplicationDbContext</c> (QBE-SVC-001).
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/rehabilitation-management/rehab-lookups")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Rehab Lookup",
        AreaName = P.AreaName,
        ControllerName = P.Lookup.Resource,
        Description = "Pilihan pasien, kunjungan, rujukan, dokter, layanan, tindakan, dan instrumen untuk form Rehab Medik",
        SortOrder = 2
    )]
    [Tags("Health Services / Rehabilitation Management / Rehab Lookup")]
    public class RehabLookupController : ControllerBase
    {
        private const string ReadName = "Read Rehab Lookup";

        private readonly RhbLookupService _service;

        public RehabLookupController(RhbLookupService service)
        {
            _service = service;
        }

        [HttpGet("patients")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabPatientOption>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Read, ReadName, Description = "Mencari pasien untuk form rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Lookup.Resource, P.Read)]
        public async Task<IActionResult> GetPatients([FromQuery] RehabPatientLookupQuery query, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.SearchPatientsAsync(query, cancellationToken), "Pilihan pasien berhasil diambil.");

        [HttpGet("encounters")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabEncounterOption>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Read, ReadName, Description = "Melihat kunjungan terbuka milik pasien yang dipilih", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Lookup.Resource, P.Read)]
        public async Task<IActionResult> GetEncounters(
            [FromQuery] Guid? patientId,
            [FromQuery] EncounterType? encounterType,
            CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetOpenEncountersAsync(patientId, encounterType, cancellationToken), "Pilihan kunjungan berhasil diambil.");

        [HttpGet("internal-referral-letters")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabReferralLetterOption>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(P.Read, ReadName, Description = "Melihat surat rujukan internal milik pasien yang dipilih", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Lookup.Resource, P.Read)]
        public async Task<IActionResult> GetInternalReferralLetters([FromQuery] Guid? patientId, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetInternalReferralLettersAsync(patientId, cancellationToken), "Pilihan surat rujukan berhasil diambil.");

        [HttpGet("doctors")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabDoctorOption>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, ReadName, Description = "Melihat pilihan dokter aktif", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Lookup.Resource, P.Read)]
        public async Task<IActionResult> GetDoctors([FromQuery] string? search, CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<List<RehabDoctorOption>>.Ok(
                await _service.GetDoctorsAsync(search, cancellationToken), "Pilihan dokter berhasil diambil."));

        [HttpGet("disciplines")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabDisciplineLookupOption>>), StatusCodes.Status200OK)]
        [AccessAction(P.Read, ReadName, Description = "Melihat layanan rehab yang aktif", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Lookup.Resource, P.Read)]
        public async Task<IActionResult> GetDisciplines(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<List<RehabDisciplineLookupOption>>.Ok(
                await _service.GetDisciplinesAsync(cancellationToken), "Pilihan layanan rehab berhasil diambil."));

        [HttpGet("disciplines/{disciplineId:guid}/procedures")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabProcedureOption>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = "Melihat tindakan yang boleh dipakai layanan rehab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Lookup.Resource, P.Read)]
        public async Task<IActionResult> GetDisciplineProcedures(Guid disciplineId, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetDisciplineProceduresAsync(disciplineId, cancellationToken), "Pilihan tindakan berhasil diambil.");

        [HttpGet("instruments")]
        [ProducesResponseType(typeof(ApiResponse<List<RehabResolvedInstrument>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Read, ReadName, Description = "Melihat instrumen asesmen yang berlaku bagi pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission(P.Lookup.Resource, P.Read)]
        public async Task<IActionResult> GetInstruments([FromQuery] RehabInstrumentLookupQuery query, CancellationToken cancellationToken = default) =>
            RhbHttp.Map(this, await _service.GetInstrumentsAsync(query, cancellationToken), "Pilihan instrumen berhasil diambil.");
    }
}
