using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/diagnosis-recommendations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Diagnosis Recommendation Resolver",
        AreaName = "HealthServices",
        ControllerName = "DiagnosisRecommendationResolver",
        Description = "Resolver rekomendasi diagnosis aktif untuk SOAP dokter",
        SortOrder = 13
    )]
    [Tags("Health Services / Clinical Management / Diagnosis Recommendation Resolver")]
    public class DiagnosisRecommendationResolverController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";
        private const string ReviewStatusActiveForSoap = "ActiveForSoap";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DiagnosisRecommendationResolverController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpPost("resolve")]
        [ProducesResponseType(typeof(ApiResponse<ResolveDiagnosisRecommendationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Diagnosis Recommendation", Description = "Mengambil rekomendasi aktif untuk SOAP dokter berdasarkan diagnosis ICD-10", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DiagnosisRecommendationResolver", "Read")]
        public async Task<IActionResult> Resolve([FromBody] ResolveDiagnosisRecommendationRequest request)
        {
            if (request == null || request.DiagnosisIds == null || request.DiagnosisIds.Count == 0)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "DiagnosisIds wajib diisi."
                ));
            }

            var diagnosisIds = request.DiagnosisIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .Take(20)
                .ToList();

            if (!diagnosisIds.Any())
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "DiagnosisIds tidak valid."
                ));
            }

            var validDiagnosisIds = await _dbContext.Set<MstDiagnosis>()
                .AsNoTracking()
                .Where(x =>
                    diagnosisIds.Contains(x.Id) &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.IcdVersion == "ICD-10" &&
                    x.DiagnosisType == "ICD10")
                .Select(x => x.Id)
                .ToListAsync();

            if (!validDiagnosisIds.Any())
            {
                return Ok(ApiResponse<ResolveDiagnosisRecommendationResponse>.Ok(
                    new ResolveDiagnosisRecommendationResponse { HasRecommendation = false },
                    "Tidak ada diagnosis ICD-10 valid untuk resolver rekomendasi."
                ));
            }

            var drugRecommendations = await _dbContext.Set<MstDiagnosisDrugRecommendation>()
                .AsNoTracking()
                .Include(x => x.Diagnosis)
                .Where(x =>
                    validDiagnosisIds.Contains(x.DiagnosisId) &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.ReviewStatus == ReviewStatusActiveForSoap)
                .OrderBy(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty)
                .ThenBy(x => x.RecommendationType)
                .Select(x => new ResolvedDiagnosisDrugRecommendationResponse
                {
                    Id = x.Id,
                    DiagnosisId = x.DiagnosisId,
                    DiagnosisCode = x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty,
                    DiagnosisName = x.Diagnosis != null ? x.Diagnosis.DiagnosisName : string.Empty,
                    DrugId = x.DrugId,
                    RecommendationType = x.RecommendationType,
                    RecommendationTypeName = BuildLabel(x.RecommendationType),
                    IndicationText = x.IndicationText,
                    DoseText = x.DoseText,
                    Route = x.Route,
                    Frequency = x.Frequency,
                    DurationText = x.DurationText,
                    CautionNote = x.CautionNote,
                    SourceType = x.SourceType,
                    SourceTitle = x.SourceTitle,
                    SourceYear = x.SourceYear
                })
                .ToListAsync();

            var procedureRecommendations = await _dbContext.Set<MstDiagnosisProcedureRecommendation>()
                .AsNoTracking()
                .Include(x => x.Diagnosis)
                .Where(x =>
                    validDiagnosisIds.Contains(x.DiagnosisId) &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.ReviewStatus == ReviewStatusActiveForSoap)
                .OrderBy(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty)
                .ThenBy(x => x.RecommendationType)
                .ThenBy(x => x.RecommendationName)
                .Select(x => new ResolvedDiagnosisProcedureRecommendationResponse
                {
                    Id = x.Id,
                    DiagnosisId = x.DiagnosisId,
                    DiagnosisCode = x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty,
                    DiagnosisName = x.Diagnosis != null ? x.Diagnosis.DiagnosisName : string.Empty,
                    ProcedureId = x.ProcedureId,
                    RecommendationType = x.RecommendationType,
                    RecommendationTypeName = BuildLabel(x.RecommendationType),
                    RecommendationName = x.RecommendationName,
                    InstructionText = x.InstructionText,
                    SourceType = x.SourceType,
                    SourceTitle = x.SourceTitle,
                    SourceYear = x.SourceYear
                })
                .ToListAsync();

            var educationRecommendations = await _dbContext.Set<MstDiagnosisEducationRecommendation>()
                .AsNoTracking()
                .Include(x => x.Diagnosis)
                .Where(x =>
                    validDiagnosisIds.Contains(x.DiagnosisId) &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.ReviewStatus == ReviewStatusActiveForSoap)
                .OrderBy(x => x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty)
                .ThenBy(x => x.EducationType)
                .ThenBy(x => x.EducationTitle)
                .Select(x => new ResolvedDiagnosisEducationRecommendationResponse
                {
                    Id = x.Id,
                    DiagnosisId = x.DiagnosisId,
                    DiagnosisCode = x.Diagnosis != null ? x.Diagnosis.DiagnosisCode : string.Empty,
                    DiagnosisName = x.Diagnosis != null ? x.Diagnosis.DiagnosisName : string.Empty,
                    EducationType = x.EducationType,
                    EducationTypeName = BuildLabel(x.EducationType),
                    EducationTitle = x.EducationTitle,
                    EducationText = x.EducationText,
                    SourceType = x.SourceType,
                    SourceTitle = x.SourceTitle,
                    SourceYear = x.SourceYear
                })
                .ToListAsync();

            var result = new ResolveDiagnosisRecommendationResponse
            {
                DrugRecommendations = drugRecommendations,
                ProcedureRecommendations = procedureRecommendations,
                EducationRecommendations = educationRecommendations
            };

            result.HasRecommendation =
                result.DrugRecommendations.Any() ||
                result.ProcedureRecommendations.Any() ||
                result.EducationRecommendations.Any();

            await _loggerService.InfoAsync(
                LogCategory,
                "DiagnosisRecommendationResolver.Resolve",
                "Mengambil rekomendasi diagnosis aktif untuk SOAP dokter.",
                new
                {
                    DiagnosisCount = validDiagnosisIds.Count,
                    DrugCount = result.DrugRecommendations.Count,
                    ProcedureCount = result.ProcedureRecommendations.Count,
                    EducationCount = result.EducationRecommendations.Count
                });

            return Ok(ApiResponse<ResolveDiagnosisRecommendationResponse>.Ok(
                result,
                result.HasRecommendation
                    ? "Rekomendasi diagnosis aktif berhasil diambil."
                    : "Tidak ada rekomendasi aktif untuk diagnosis yang dipilih."
            ));
        }

        private static string BuildLabel(string value)
        {
            return value switch
            {
                "FirstLine" => "First Line",
                "Alternative" => "Alternative",
                "Symptomatic" => "Symptomatic",
                "Supportive" => "Supportive",
                "Conditional" => "Conditional",
                "Procedure" => "Procedure",
                "Lab" => "Laboratorium",
                "Radiology" => "Radiologi",
                "Monitoring" => "Monitoring",
                "Referral" => "Rujukan",
                "FollowUp" => "Follow Up",
                "General" => "Umum",
                "Diet" => "Diet",
                "Activity" => "Aktivitas",
                "MedicationUse" => "Penggunaan Obat",
                "WarningSign" => "Tanda Bahaya",
                "Prevention" => "Pencegahan",
                _ => value
            };
        }
    }
}
