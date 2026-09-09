using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;
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
    [ProducesResponseType(typeof(ApiResponse<List<GzMasterOptionResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Master",
        Description = "Melihat master jenis diet", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("NutritionMaster", "Read")]
    public async Task<IActionResult> GetDietTypes([FromQuery] bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var data = await _dbContext.GzDietTypes.AsNoTracking()
            .Where(x => !x.IsDelete && (!onlyActive || x.IsActive))
            .OrderBy(x => x.SortOrder).ThenBy(x => x.DietTypeName)
            .Select(x => new GzMasterOptionResponse
            {
                Id = x.Id, Code = x.DietTypeCode, Name = x.DietTypeName,
                Description = x.Description, IsSpecialDiet = x.IsSpecialDiet, IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<GzMasterOptionResponse>>.Ok(data,
            "Master jenis diet berhasil diambil."));
    }

    [HttpPost("diet-types")]
    [ProducesResponseType(typeof(ApiResponse<GzMasterOptionResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Nutrition Master",
        Description = "Menambah jenis diet", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("NutritionMaster", "Update")]
    public async Task<IActionResult> CreateDietType([FromBody] SaveGzMasterRequest request,
        CancellationToken cancellationToken)
    {
        var duplicate = await _dbContext.GzDietTypes.AsNoTracking()
            .AnyAsync(x => x.DietTypeCode == request.Code.Trim() && !x.IsDelete, cancellationToken);
        if (duplicate)
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict,
                "Kode jenis diet sudah dipakai."));

        var entity = new GzDietType
        {
            DietTypeCode = request.Code.Trim(),
            DietTypeName = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsSpecialDiet = request.IsSpecialDiet,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow
        };

        _dbContext.GzDietTypes.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<GzMasterOptionResponse>.Ok(new GzMasterOptionResponse
        {
            Id = entity.Id, Code = entity.DietTypeCode, Name = entity.DietTypeName,
            Description = entity.Description, IsSpecialDiet = entity.IsSpecialDiet,
            IsActive = entity.IsActive
        }, "Jenis diet berhasil ditambahkan."));
    }

    // --------------------------------------------------------- bentuk makanan

    [HttpGet("food-forms")]
    [ProducesResponseType(typeof(ApiResponse<List<GzMasterOptionResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Master",
        Description = "Melihat master bentuk makanan", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("NutritionMaster", "Read")]
    public async Task<IActionResult> GetFoodForms([FromQuery] bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var data = await _dbContext.GzFoodForms.AsNoTracking()
            .Where(x => !x.IsDelete && (!onlyActive || x.IsActive))
            .OrderBy(x => x.SortOrder).ThenBy(x => x.FoodFormName)
            .Select(x => new GzMasterOptionResponse
            {
                Id = x.Id, Code = x.FoodFormCode, Name = x.FoodFormName,
                Description = x.Description, IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<GzMasterOptionResponse>>.Ok(data,
            "Master bentuk makanan berhasil diambil."));
    }

    [HttpPost("food-forms")]
    [ProducesResponseType(typeof(ApiResponse<GzMasterOptionResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Nutrition Master",
        Description = "Menambah bentuk makanan", AccessType = AccessTypes.Create, SortOrder = 4)]
    [AccessPermission("NutritionMaster", "Update")]
    public async Task<IActionResult> CreateFoodForm([FromBody] SaveGzMasterRequest request,
        CancellationToken cancellationToken)
    {
        var duplicate = await _dbContext.GzFoodForms.AsNoTracking()
            .AnyAsync(x => x.FoodFormCode == request.Code.Trim() && !x.IsDelete, cancellationToken);
        if (duplicate)
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict,
                "Kode bentuk makanan sudah dipakai."));

        var entity = new GzFoodForm
        {
            FoodFormCode = request.Code.Trim(),
            FoodFormName = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow
        };

        _dbContext.GzFoodForms.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<GzMasterOptionResponse>.Ok(new GzMasterOptionResponse
        {
            Id = entity.Id, Code = entity.FoodFormCode, Name = entity.FoodFormName,
            Description = entity.Description, IsActive = entity.IsActive
        }, "Bentuk makanan berhasil ditambahkan."));
    }

    // ----------------------------------------------------------- jadwal makan

    [HttpGet("meal-schedules")]
    [ProducesResponseType(typeof(ApiResponse<List<GzMasterOptionResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Master",
        Description = "Melihat master jadwal makan", AccessType = AccessTypes.Read, SortOrder = 5)]
    [AccessPermission("NutritionMaster", "Read")]
    public async Task<IActionResult> GetMealSchedules([FromQuery] bool onlyActive = true,
        CancellationToken cancellationToken = default)
    {
        var data = await _dbContext.GzMealSchedules.AsNoTracking()
            .Where(x => !x.IsDelete && (!onlyActive || x.IsActive))
            .OrderBy(x => x.ServingTime).ThenBy(x => x.SortOrder)
            .Select(x => new GzMasterOptionResponse
            {
                Id = x.Id, Code = x.MealScheduleCode, Name = x.MealScheduleName,
                ServingTime = x.ServingTime, IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(ApiResponse<List<GzMasterOptionResponse>>.Ok(data,
            "Master jadwal makan berhasil diambil."));
    }

    [HttpPost("meal-schedules")]
    [ProducesResponseType(typeof(ApiResponse<GzMasterOptionResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Nutrition Master",
        Description = "Menambah jadwal makan", AccessType = AccessTypes.Create, SortOrder = 6)]
    [AccessPermission("NutritionMaster", "Update")]
    public async Task<IActionResult> CreateMealSchedule([FromBody] SaveGzMasterRequest request,
        CancellationToken cancellationToken)
    {
        var duplicate = await _dbContext.GzMealSchedules.AsNoTracking()
            .AnyAsync(x => x.MealScheduleCode == request.Code.Trim() && !x.IsDelete, cancellationToken);
        if (duplicate)
            return Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict,
                "Kode jadwal makan sudah dipakai."));

        var entity = new GzMealSchedule
        {
            MealScheduleCode = request.Code.Trim(),
            MealScheduleName = request.Name.Trim(),
            ServingTime = request.ServingTime ?? new TimeOnly(7, 0),
            IsMainMeal = request.IsMainMeal,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreateDateTime = DateTime.UtcNow
        };

        _dbContext.GzMealSchedules.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ApiResponse<GzMasterOptionResponse>.Ok(new GzMasterOptionResponse
        {
            Id = entity.Id, Code = entity.MealScheduleCode, Name = entity.MealScheduleName,
            ServingTime = entity.ServingTime, IsActive = entity.IsActive
        }, "Jadwal makan berhasil ditambahkan."));
    }
}
