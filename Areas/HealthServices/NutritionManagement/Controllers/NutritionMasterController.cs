using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Controllers;

/// <summary>
/// Master gizi: jenis diet, bentuk makanan, dan jadwal makan.
/// </summary>
/// <remarks>
/// Ketiganya sengaja dibuat KOSONG dan diisi admin lewat layar ini. Nama diet, bentuk
/// makanan, dan jam makan berbeda antar rumah sakit; mengisinya dengan daftar karangan
/// menghasilkan master yang terlihat resmi padahal tidak pernah disahkan siapa pun.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/nutrition-management/masters")]
[Tags("Health Services / Nutrition Management / Master")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_NUTRITION_MANAGEMENT",
    moduleName: "Health Service Nutrition Management",
    displayName: "Nutrition Master",
    AreaName = "HealthServices",
    ControllerName = "NutritionMaster",
    Description = "Master jenis diet, bentuk makanan, dan jadwal makan",
    SortOrder = 3)]
public class NutritionMasterController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public NutritionMasterController(ApplicationDbContext dbContext) => _dbContext = dbContext;

    // ------------------------------------------------------------- jenis diet

    [HttpGet("diet-types")]
    [ProducesResponseType(typeof(ApiResponse<List<GziMasterOptionResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Master",
        Description = "Melihat master jenis diet", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("NutritionMaster", "Read")]
    public async Task<IActionResult> GetDietTypes([FromQuery] bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var data = await _dbContext.GziDietTypes.AsNoTracking()
            .Where(x => !x.IsDelete && (!onlyActive || x.IsActive))
            .OrderBy(x => x.SortOrder).ThenBy(x => x.DietTypeName)
            .Select(x => new GziMasterOptionResponse
            {
                Id = x.Id, Code = x.DietTypeCode, Name = x.DietTypeName,
                Description = x.Description, IsSpecialDiet = x.IsSpecialDiet, IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<GziMasterOptionResponse>>.Ok(data,
            "Master jenis diet berhasil diambil."));
    }

    [HttpPost("diet-types")]
    [ProducesResponseType(typeof(ApiResponse<GziMasterOptionResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Nutrition Master",
        Description = "Menambah jenis diet", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("NutritionMaster", "Update")]
    public async Task<IActionResult> CreateDietType([FromBody] SaveGzMasterRequest request,
        CancellationToken cancellationToken)
    {
        var duplicate = await _dbContext.GziDietTypes.AsNoTracking()
            .AnyAsync(x => x.DietTypeCode == request.Code.Trim() && !x.IsDelete, cancellationToken);
        if (duplicate)
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict,
                "Kode jenis diet sudah dipakai."));

        var entity = new GziDietType
        {
            DietTypeCode = request.Code.Trim(),
            DietTypeName = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsSpecialDiet = request.IsSpecialDiet,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow
        };

        _dbContext.GziDietTypes.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<GziMasterOptionResponse>.Ok(new GziMasterOptionResponse
        {
            Id = entity.Id, Code = entity.DietTypeCode, Name = entity.DietTypeName,
            Description = entity.Description, IsSpecialDiet = entity.IsSpecialDiet,
            IsActive = entity.IsActive
        }, "Jenis diet berhasil ditambahkan."));
    }

    // --------------------------------------------------------- bentuk makanan

    [HttpGet("food-forms")]
    [ProducesResponseType(typeof(ApiResponse<List<GziMasterOptionResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Master",
        Description = "Melihat master bentuk makanan", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("NutritionMaster", "Read")]
    public async Task<IActionResult> GetFoodForms([FromQuery] bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var data = await _dbContext.GziFoodForms.AsNoTracking()
            .Where(x => !x.IsDelete && (!onlyActive || x.IsActive))
            .OrderBy(x => x.SortOrder).ThenBy(x => x.FoodFormName)
            .Select(x => new GziMasterOptionResponse
            {
                Id = x.Id, Code = x.FoodFormCode, Name = x.FoodFormName,
                Description = x.Description, IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<GziMasterOptionResponse>>.Ok(data,
            "Master bentuk makanan berhasil diambil."));
    }

    [HttpPost("food-forms")]
    [ProducesResponseType(typeof(ApiResponse<GziMasterOptionResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Nutrition Master",
        Description = "Menambah bentuk makanan", AccessType = AccessTypes.Create, SortOrder = 4)]
    [AccessPermission("NutritionMaster", "Update")]
    public async Task<IActionResult> CreateFoodForm([FromBody] SaveGzMasterRequest request,
        CancellationToken cancellationToken)
    {
        var duplicate = await _dbContext.GziFoodForms.AsNoTracking()
            .AnyAsync(x => x.FoodFormCode == request.Code.Trim() && !x.IsDelete, cancellationToken);
        if (duplicate)
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict,
                "Kode bentuk makanan sudah dipakai."));

        var entity = new GziFoodForm
        {
            FoodFormCode = request.Code.Trim(),
            FoodFormName = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow
        };

        _dbContext.GziFoodForms.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<GziMasterOptionResponse>.Ok(new GziMasterOptionResponse
        {
            Id = entity.Id, Code = entity.FoodFormCode, Name = entity.FoodFormName,
            Description = entity.Description, IsActive = entity.IsActive
        }, "Bentuk makanan berhasil ditambahkan."));
    }

    // ----------------------------------------------------------- jadwal makan

    [HttpGet("meal-schedules")]
    [ProducesResponseType(typeof(ApiResponse<List<GziMasterOptionResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Master",
        Description = "Melihat master jadwal makan", AccessType = AccessTypes.Read, SortOrder = 5)]
    [AccessPermission("NutritionMaster", "Read")]
    public async Task<IActionResult> GetMealSchedules([FromQuery] bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var data = await _dbContext.GziMealSchedules.AsNoTracking()
            .Where(x => !x.IsDelete && (!onlyActive || x.IsActive))
            .OrderBy(x => x.ServingTime).ThenBy(x => x.SortOrder)
            .Select(x => new GziMasterOptionResponse
            {
                Id = x.Id, Code = x.MealScheduleCode, Name = x.MealScheduleName,
                ServingTime = x.ServingTime, IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<GziMasterOptionResponse>>.Ok(data,
            "Master jadwal makan berhasil diambil."));
    }

    [HttpPost("meal-schedules")]
    [ProducesResponseType(typeof(ApiResponse<GziMasterOptionResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Nutrition Master",
        Description = "Menambah jadwal makan", AccessType = AccessTypes.Create, SortOrder = 6)]
    [AccessPermission("NutritionMaster", "Update")]
    public async Task<IActionResult> CreateMealSchedule([FromBody] SaveGzMasterRequest request,
        CancellationToken cancellationToken)
    {
        var duplicate = await _dbContext.GziMealSchedules.AsNoTracking()
            .AnyAsync(x => x.MealScheduleCode == request.Code.Trim() && !x.IsDelete, cancellationToken);
        if (duplicate)
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict,
                "Kode jadwal makan sudah dipakai."));

        var entity = new GziMealSchedule
        {
            MealScheduleCode = request.Code.Trim(),
            MealScheduleName = request.Name.Trim(),
            ServingTime = request.ServingTime ?? new TimeOnly(7, 0),
            IsMainMeal = request.IsMainMeal,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow
        };

        _dbContext.GziMealSchedules.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<GziMasterOptionResponse>.Ok(new GziMasterOptionResponse
        {
            Id = entity.Id, Code = entity.MealScheduleCode, Name = entity.MealScheduleName,
            ServingTime = entity.ServingTime, IsActive = entity.IsActive
        }, "Jadwal makan berhasil ditambahkan."));
    }

    // =========================================== master diagnosis gizi (GIZ-DEC-011)

    [HttpGet("nutrition-diagnosis-domains")]
    [ProducesResponseType(typeof(ApiResponse<List<GziDiagnosisDomainResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Master",
        Description = "Melihat domain diagnosis gizi", AccessType = AccessTypes.Read, SortOrder = 7)]
    [AccessPermission("NutritionMaster", "Read")]
    public async Task<IActionResult> GetDiagnosisDomains([FromQuery] bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var data = await _dbContext.GziNutritionDiagnosisDomains.AsNoTracking()
            .Where(x => !x.IsDelete && (!onlyActive || x.IsActive))
            .OrderBy(x => x.SortOrder).ThenBy(x => x.DomainCode)
            .Select(x => new GziDiagnosisDomainResponse
            {
                Id = x.Id, DomainCode = x.DomainCode, DomainName = x.DomainName,
                Description = x.Description, SortOrder = x.SortOrder, IsActive = x.IsActive,
                DiagnosisCount = x.Diagnoses.Count(d => !d.IsDelete && d.IsActive)
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<GziDiagnosisDomainResponse>>.Ok(data,
            "Domain diagnosis gizi berhasil diambil."));
    }

    [HttpPost("nutrition-diagnosis-domains")]
    [ProducesResponseType(typeof(ApiResponse<GziDiagnosisDomainResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Nutrition Master",
        Description = "Menambah domain diagnosis gizi", AccessType = AccessTypes.Create, SortOrder = 8)]
    [AccessPermission("NutritionMaster", "Update")]
    public async Task<IActionResult> CreateDiagnosisDomain(
        [FromBody] SaveGzDiagnosisDomainRequest request, CancellationToken cancellationToken)
    {
        var code = request.DomainCode.Trim().ToUpperInvariant();
        var duplicate = await _dbContext.GziNutritionDiagnosisDomains.AsNoTracking()
            .AnyAsync(x => x.DomainCode == code && !x.IsDelete, cancellationToken);
        if (duplicate)
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict,
                "Kode domain diagnosis sudah dipakai."));

        var entity = new GziNutritionDiagnosisDomain
        {
            DomainCode = code,
            DomainName = request.DomainName.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow
        };

        _dbContext.GziNutritionDiagnosisDomains.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<GziDiagnosisDomainResponse>.Ok(new GziDiagnosisDomainResponse
        {
            Id = entity.Id, DomainCode = entity.DomainCode, DomainName = entity.DomainName,
            Description = entity.Description, SortOrder = entity.SortOrder,
            IsActive = entity.IsActive, DiagnosisCount = 0
        }, "Domain diagnosis gizi berhasil ditambahkan."));
    }

    /// <summary>
    /// Daftar diagnosis gizi. Dipaginasi karena daftar IDNT berjumlah ratusan baris, dan layar
    /// yang memuat semuanya sekaligus membuat pencarian menjadi satu-satunya cara memakainya.
    /// </summary>
    [HttpGet("nutrition-diagnoses")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<GziNutritionDiagnosisResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Master",
        Description = "Melihat master diagnosis gizi", AccessType = AccessTypes.Read, SortOrder = 9)]
    [AccessPermission("NutritionMaster", "Read")]
    public async Task<IActionResult> GetNutritionDiagnoses([FromQuery] GziDiagnosisQuery query,
        CancellationToken cancellationToken)
    {
        var source = _dbContext.GziNutritionDiagnoses.AsNoTracking().Where(x => !x.IsDelete);

        if (query.OnlyActive) source = source.Where(x => x.IsActive);
        if (query.OnlySelectable) source = source.Where(x => x.IsSelectable);
        if (query.DiagnosisDomainId.HasValue)
            source = source.Where(x => x.DiagnosisDomainId == query.DiagnosisDomainId);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var keyword = query.Search.Trim().ToLower();
            source = source.Where(x => x.DiagnosisCode.ToLower().Contains(keyword) ||
                                       x.DiagnosisName.ToLower().Contains(keyword));
        }

        var pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
        var pageSize = query.PageSize is < 1 or > 200 ? 25 : query.PageSize;

        var total = await source.CountAsync(cancellationToken);
        var items = await source
            .OrderBy(x => x.SortOrder).ThenBy(x => x.DiagnosisCode)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new GziNutritionDiagnosisResponse
            {
                Id = x.Id,
                DiagnosisDomainId = x.DiagnosisDomainId,
                DomainCode = x.DiagnosisDomain!.DomainCode,
                DomainName = x.DiagnosisDomain.DomainName,
                ParentDiagnosisId = x.ParentDiagnosisId,
                ParentDiagnosisCode = x.ParentDiagnosis != null ? x.ParentDiagnosis.DiagnosisCode : null,
                DiagnosisCode = x.DiagnosisCode,
                DiagnosisName = x.DiagnosisName,
                Description = x.Description,
                Standard = x.Standard,
                StandardVersion = x.StandardVersion,
                IsSelectable = x.IsSelectable,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<PagedResult<GziNutritionDiagnosisResponse>>.Ok(
            new PagedResult<GziNutritionDiagnosisResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = total,
                TotalPage = (int)Math.Ceiling(total / (double)pageSize),
                Items = items
            }, "Master diagnosis gizi berhasil diambil."));
    }

    [HttpPost("nutrition-diagnoses")]
    [ProducesResponseType(typeof(ApiResponse<GziNutritionDiagnosisResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Nutrition Master",
        Description = "Menambah diagnosis gizi", AccessType = AccessTypes.Create, SortOrder = 10)]
    [AccessPermission("NutritionMaster", "Update")]
    public async Task<IActionResult> CreateNutritionDiagnosis(
        [FromBody] SaveGzNutritionDiagnosisRequest request, CancellationToken cancellationToken)
    {
        var code = request.DiagnosisCode.Trim().ToUpperInvariant();
        var duplicate = await _dbContext.GziNutritionDiagnoses.AsNoTracking()
            .AnyAsync(x => x.DiagnosisCode == code && !x.IsDelete, cancellationToken);
        if (duplicate)
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict,
                "Kode diagnosis gizi sudah dipakai."));

        var domainExists = await _dbContext.GziNutritionDiagnosisDomains.AsNoTracking()
            .AnyAsync(x => x.Id == request.DiagnosisDomainId && !x.IsDelete, cancellationToken);
        if (!domainExists)
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity,
                "Domain diagnosis yang dipilih tidak ditemukan."));

        if (request.ParentDiagnosisId.HasValue)
        {
            var parentExists = await _dbContext.GziNutritionDiagnoses.AsNoTracking()
                .AnyAsync(x => x.Id == request.ParentDiagnosisId && !x.IsDelete, cancellationToken);
            if (!parentExists)
                return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity,
                    "Diagnosis induk yang dipilih tidak ditemukan."));
        }

        var entity = new GziNutritionDiagnosis
        {
            DiagnosisDomainId = request.DiagnosisDomainId,
            ParentDiagnosisId = request.ParentDiagnosisId,
            DiagnosisCode = code,
            DiagnosisName = request.DiagnosisName.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            Standard = string.IsNullOrWhiteSpace(request.Standard) ? "IDNT" : request.Standard.Trim(),
            StandardVersion = string.IsNullOrWhiteSpace(request.StandardVersion) ? null : request.StandardVersion.Trim(),
            IsSelectable = request.IsSelectable,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow
        };

        _dbContext.GziNutritionDiagnoses.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var domain = await _dbContext.GziNutritionDiagnosisDomains.AsNoTracking()
            .FirstAsync(x => x.Id == entity.DiagnosisDomainId, cancellationToken);

        return Ok(ApiResponse<GziNutritionDiagnosisResponse>.Ok(new GziNutritionDiagnosisResponse
        {
            Id = entity.Id,
            DiagnosisDomainId = entity.DiagnosisDomainId,
            DomainCode = domain.DomainCode,
            DomainName = domain.DomainName,
            ParentDiagnosisId = entity.ParentDiagnosisId,
            DiagnosisCode = entity.DiagnosisCode,
            DiagnosisName = entity.DiagnosisName,
            Description = entity.Description,
            Standard = entity.Standard,
            StandardVersion = entity.StandardVersion,
            IsSelectable = entity.IsSelectable,
            SortOrder = entity.SortOrder,
            IsActive = entity.IsActive
        }, "Diagnosis gizi berhasil ditambahkan."));
    }

    // ========================================== master parameter nutrisi (GIZ-DEC-012)

    [HttpGet("nutrition-parameters")]
    [ProducesResponseType(typeof(ApiResponse<List<GziNutritionParameterResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Master",
        Description = "Melihat master parameter nutrisi", AccessType = AccessTypes.Read, SortOrder = 11)]
    [AccessPermission("NutritionMaster", "Read")]
    public async Task<IActionResult> GetNutritionParameters([FromQuery] bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var data = await _dbContext.GziNutritionParameters.AsNoTracking()
            .Where(x => !x.IsDelete && (!onlyActive || x.IsActive))
            .OrderBy(x => x.SortOrder).ThenBy(x => x.ParameterName)
            .Select(x => new GziNutritionParameterResponse
            {
                Id = x.Id, ParameterCode = x.ParameterCode, ParameterName = x.ParameterName,
                UnitCode = x.UnitCode, ValueScale = x.ValueScale,
                MinValue = x.MinValue, MaxValue = x.MaxValue,
                SortOrder = x.SortOrder, IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<GziNutritionParameterResponse>>.Ok(data,
            "Master parameter nutrisi berhasil diambil."));
    }

    [HttpPost("nutrition-parameters")]
    [ProducesResponseType(typeof(ApiResponse<GziNutritionParameterResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Nutrition Master",
        Description = "Menambah parameter nutrisi", AccessType = AccessTypes.Create, SortOrder = 12)]
    [AccessPermission("NutritionMaster", "Update")]
    public async Task<IActionResult> CreateNutritionParameter(
        [FromBody] SaveGzNutritionParameterRequest request, CancellationToken cancellationToken)
    {
        var code = request.ParameterCode.Trim().ToUpperInvariant();
        var duplicate = await _dbContext.GziNutritionParameters.AsNoTracking()
            .AnyAsync(x => x.ParameterCode == code && !x.IsDelete, cancellationToken);
        if (duplicate)
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict,
                "Kode parameter nutrisi sudah dipakai."));

        if (request.MinValue.HasValue && request.MaxValue.HasValue &&
            request.MinValue.Value > request.MaxValue.Value)
            return UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity,
                "Batas bawah tidak boleh melebihi batas atas."));

        var entity = new GziNutritionParameter
        {
            ParameterCode = code,
            ParameterName = request.ParameterName.Trim(),
            UnitCode = request.UnitCode.Trim(),
            ValueScale = request.ValueScale,
            MinValue = request.MinValue,
            MaxValue = request.MaxValue,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow
        };

        _dbContext.GziNutritionParameters.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<GziNutritionParameterResponse>.Ok(new GziNutritionParameterResponse
        {
            Id = entity.Id, ParameterCode = entity.ParameterCode, ParameterName = entity.ParameterName,
            UnitCode = entity.UnitCode, ValueScale = entity.ValueScale,
            MinValue = entity.MinValue, MaxValue = entity.MaxValue,
            SortOrder = entity.SortOrder, IsActive = entity.IsActive
        }, "Parameter nutrisi berhasil ditambahkan."));
    }

    // =================================================== registry rumus (GIZ-OQ-007)

    /// <summary>
    /// Registry rumus kebutuhan nutrisi.
    /// </summary>
    /// <remarks>
    /// Daftarnya kosong pada V1, dan itu memang keputusan pemilik proses — rumus tidak dibuat
    /// sampai rumus resmi beserta sumbernya diserahkan. <c>IsImplemented</c> memperlihatkan
    /// apakah sebuah baris benar-benar punya kelas perhitungan, sehingga baris yang didaftarkan
    /// tanpa implementasi tidak diam-diam tampak siap pakai.
    /// </remarks>
    [HttpGet("nutrition-formulas")]
    [ProducesResponseType(typeof(ApiResponse<List<GziNutritionFormulaResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Master",
        Description = "Melihat registry rumus nutrisi", AccessType = AccessTypes.Read, SortOrder = 13)]
    [AccessPermission("NutritionMaster", "Read")]
    public async Task<IActionResult> GetNutritionFormulas(
        [FromServices] NutritionRequirementCalculator calculator,
        [FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.GziNutritionFormulas.AsNoTracking()
            .Where(x => !x.IsDelete && (!onlyActive || x.IsActive))
            .OrderBy(x => x.FormulaCode).ThenBy(x => x.FormulaVersion)
            .ToListAsync(cancellationToken);

        var data = rows.Select(x => new GziNutritionFormulaResponse
        {
            Id = x.Id, FormulaCode = x.FormulaCode, FormulaName = x.FormulaName,
            FormulaVersion = x.FormulaVersion, Description = x.Description,
            SourceReference = x.SourceReference, ImplementationKey = x.ImplementationKey,
            IsActive = x.IsActive,
            IsImplemented = calculator.Resolve(x.ImplementationKey) != null
        }).ToList();

        return Ok(ApiResponse<List<GziNutritionFormulaResponse>>.Ok(data,
            data.Count == 0
                ? "Belum ada rumus terdaftar. Kebutuhan nutrisi diisi ahli gizi."
                : "Registry rumus nutrisi berhasil diambil."));
    }

    [HttpPost("nutrition-formulas")]
    [ProducesResponseType(typeof(ApiResponse<GziNutritionFormulaResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Nutrition Master",
        Description = "Mendaftarkan rumus nutrisi", AccessType = AccessTypes.Create, SortOrder = 14)]
    [AccessPermission("NutritionMaster", "Update")]
    public async Task<IActionResult> CreateNutritionFormula(
        [FromServices] NutritionRequirementCalculator calculator,
        [FromBody] SaveGzNutritionFormulaRequest request, CancellationToken cancellationToken)
    {
        var code = request.FormulaCode.Trim().ToUpperInvariant();
        var version = request.FormulaVersion.Trim();

        var duplicate = await _dbContext.GziNutritionFormulas.AsNoTracking()
            .AnyAsync(x => x.FormulaCode == code && x.FormulaVersion == version && !x.IsDelete,
                cancellationToken);
        if (duplicate)
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict,
                "Rumus dengan kode dan versi tersebut sudah terdaftar."));

        var entity = new GziNutritionFormula
        {
            FormulaCode = code,
            FormulaName = request.FormulaName.Trim(),
            FormulaVersion = version,
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            SourceReference = string.IsNullOrWhiteSpace(request.SourceReference) ? null : request.SourceReference.Trim(),
            ImplementationKey = request.ImplementationKey.Trim(),
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow
        };

        _dbContext.GziNutritionFormulas.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var implemented = calculator.Resolve(entity.ImplementationKey) != null;

        return Ok(ApiResponse<GziNutritionFormulaResponse>.Ok(new GziNutritionFormulaResponse
        {
            Id = entity.Id, FormulaCode = entity.FormulaCode, FormulaName = entity.FormulaName,
            FormulaVersion = entity.FormulaVersion, Description = entity.Description,
            SourceReference = entity.SourceReference, ImplementationKey = entity.ImplementationKey,
            IsActive = entity.IsActive, IsImplemented = implemented
        }, implemented
            ? "Rumus nutrisi berhasil didaftarkan."
            : "Rumus terdaftar, tetapi belum ada kelas perhitungan untuk kunci implementasi ini."));
    }
}
