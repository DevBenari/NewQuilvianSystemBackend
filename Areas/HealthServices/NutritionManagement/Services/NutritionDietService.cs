using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;

/// <summary>
/// Diet pasien, rekap produksi makanan, dan distribusi makanan ke pasien.
/// </summary>
public sealed class NutritionDietService
{
    private const string LogCategory = "NutritionManagement";

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;

    public NutritionDietService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
    }

    // ============================================================== diet pasien

    public async Task<List<GzPatientDietResponse>> GetDietsByOrderAsync(Guid orderId,
        CancellationToken cancellationToken = default) =>
        await BuildDietQuery()
            .Where(x => x.NutritionOrderId == orderId)
            .OrderByDescending(x => x.StartAt)
            .Select(x => MapDiet(x))
            .ToListAsync(cancellationToken);

    /// <summary>Seluruh diet yang sedang berlaku; inilah yang dibaca dapur.</summary>
    public async Task<List<GzPatientDietResponse>> GetActiveDietsAsync(
        CancellationToken cancellationToken = default) =>
        await BuildDietQuery()
            .Where(x => x.Status == GzPatientDietStatus.Active)
            .OrderBy(x => x.Patient!.FullName)
            .Select(x => MapDiet(x))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Menetapkan diet baru. Diet yang sedang aktif dihentikan lebih dulu, bukan ditimpa,
    /// sehingga pertanyaan "diet apa yang berlaku tanggal sekian" tetap terjawab.
    /// </summary>
    public async Task<GzPatientDietResponse> PrescribeAsync(PrescribeGzDietRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        var actorUserId = GetCurrentUserId();

        var deterministicId = DeterministicId(request.IdempotencyKey);
        var existing = await BuildDietQuery().FirstOrDefaultAsync(x => x.Id == deterministicId,
            cancellationToken);
        if (existing != null) return MapDiet(existing);

        var order = await _dbContext.GzNutritionOrders.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.NutritionOrderId && !x.IsDelete,
                cancellationToken)
            ?? throw new KeyNotFoundException("Order konsultasi gizi tidak ditemukan.");

        if (order.Status is not (GzOrderStatus.Requested or GzOrderStatus.InProgress))
            throw new NutritionConflictException("GIZ004",
                "Diet hanya dapat ditetapkan pada order yang masih berjalan.");

        await EnsureMasterActiveAsync(request.DietTypeId, request.FoodFormId, cancellationToken);

        var now = DateTime.UtcNow;

        var current = await _dbContext.GzPatientDiets
            .FirstOrDefaultAsync(x => x.PatientId == order.PatientId && !x.IsDelete &&
                                      x.Status == GzPatientDietStatus.Active, cancellationToken);

        if (current != null)
        {
            if (string.IsNullOrWhiteSpace(request.ChangeReason))
                throw new NutritionUnprocessableException("GIZ010",
                    "Pasien sudah punya diet aktif. Alasan perubahan wajib diisi.");

            current.Status = GzPatientDietStatus.Changed;
            current.EndAt = now;
            current.ChangeReason = request.ChangeReason.Trim();
            current.Version++;
            current.UpdateDateTime = now;
            current.UpdateBy = actorUserId;
        }

        var diet = new GzPatientDiet
        {
            Id = deterministicId,
            NutritionOrderId = order.Id,
            PatientId = order.PatientId,
            DietTypeId = request.DietTypeId,
            FoodFormId = request.FoodFormId,
            EnergyRequirementKcal = request.EnergyRequirementKcal,
            Instruction = Normalize(request.Instruction),
            Status = GzPatientDietStatus.Active,
            StartAt = now,
            PrescribedByWorkforceId = request.PrescribedByWorkforceId,
            Version = 0,
            CreateDateTime = now,
            CreateBy = actorUserId
        };

        _dbContext.GzPatientDiets.Add(diet);
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "NutritionDiet.Prescribe",
            "Menetapkan diet pasien.",
            new { diet.Id, diet.PatientId, ActorUserId = actorUserId });

        return MapDiet(await BuildDietQuery().FirstAsync(x => x.Id == diet.Id, cancellationToken));
    }

    public async Task<GzPatientDietResponse> StopAsync(Guid dietId, StopGzDietRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new NutritionUnprocessableException("GIZ011",
                "Alasan penghentian diet wajib diisi.");

        var actorUserId = GetCurrentUserId();
        var diet = await _dbContext.GzPatientDiets
            .FirstOrDefaultAsync(x => x.Id == dietId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Diet pasien tidak ditemukan.");

        if (diet.Status != GzPatientDietStatus.Active)
            throw new NutritionConflictException("GIZ004",
                "Diet ini sudah tidak berlaku dan tidak dapat dihentikan lagi.");

        if (diet.Version != request.ExpectedVersion)
            throw new NutritionConflictException("GIZ012",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");

        var now = DateTime.UtcNow;
        diet.Status = GzPatientDietStatus.Stopped;
        diet.EndAt = now;
        diet.ChangeReason = request.Reason.Trim();
        diet.Version++;
        diet.UpdateDateTime = now;
        diet.UpdateBy = actorUserId;

        await SaveAsync(cancellationToken);
        return MapDiet(await BuildDietQuery().FirstAsync(x => x.Id == dietId, cancellationToken));
    }

    // ============================================================== produksi

    /// <summary>
    /// Menghitung kebutuhan produksi untuk satu tanggal, dikelompokkan per jadwal makan
    /// lalu per jenis diet dan bentuk makanan.
    /// </summary>
    /// <remarks>
    /// Tidak ada data produksi yang disimpan. Angkanya selalu dihitung ulang dari diet yang
    /// sedang aktif, sehingga dapur tidak pernah memasak berdasarkan angka basi.
    /// </remarks>
    public async Task<List<GzProductionSummaryResponse>> GetProductionSummaryAsync(
        DateOnly? serviceDate, CancellationToken cancellationToken = default)
    {
        var date = serviceDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var schedules = await _dbContext.GzMealSchedules.AsNoTracking()
            .Where(x => x.IsActive && !x.IsDelete)
            .OrderBy(x => x.ServingTime).ThenBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        var diets = await _dbContext.GzPatientDiets.AsNoTracking()
            .Include(x => x.DietType)
            .Include(x => x.FoodForm)
            .Where(x => !x.IsDelete && x.Status == GzPatientDietStatus.Active)
            .ToListAsync(cancellationToken);

        var breakdown = diets
            .GroupBy(x => new { x.DietTypeId, x.FoodFormId })
            .Select(group => new GzProductionBreakdownResponse
            {
                DietTypeId = group.Key.DietTypeId,
                DietTypeName = group.First().DietType?.DietTypeName ?? string.Empty,
                FoodFormId = group.Key.FoodFormId,
                FoodFormName = group.First().FoodForm?.FoodFormName ?? string.Empty,
                IsSpecialDiet = group.First().DietType?.IsSpecialDiet ?? false,
                Portion = group.Count()
            })
            .OrderByDescending(x => x.IsSpecialDiet)
            .ThenBy(x => x.DietTypeName)
            .ToList();

        // Setiap jadwal makan memakai rekap yang sama, karena diet berlaku sepanjang hari.
        // Bila kelak diet dibedakan per waktu makan, di sinilah pembedaannya ditambahkan.
        return [.. schedules.Select(schedule => new GzProductionSummaryResponse
        {
            ServiceDate = date,
            MealScheduleId = schedule.Id,
            MealScheduleName = schedule.MealScheduleName,
            ServingTime = schedule.ServingTime,
            TotalPortion = diets.Count,
            Breakdown = breakdown
        })];
    }

    // ============================================================== distribusi

    /// <summary>
    /// Daftar distribusi satu jadwal makan pada satu tanggal: seluruh pasien berdiet aktif,
    /// beserta keterangan apakah makanannya sudah diserahkan.
    /// </summary>
    public async Task<List<GzMealDistributionRowResponse>> GetDistributionAsync(
        Guid mealScheduleId, DateOnly? serviceDate, CancellationToken cancellationToken = default)
    {
        var date = serviceDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        return await _dbContext.GzPatientDiets.AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.DietType)
            .Include(x => x.FoodForm)
            .Where(x => !x.IsDelete && x.Status == GzPatientDietStatus.Active)
            .OrderBy(x => x.Patient!.FullName)
            .Select(x => new GzMealDistributionRowResponse
            {
                PatientDietId = x.Id,
                PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
                DietTypeName = x.DietType != null ? x.DietType.DietTypeName : string.Empty,
                FoodFormName = x.FoodForm != null ? x.FoodForm.FoodFormName : string.Empty,
                EnergyRequirementKcal = x.EnergyRequirementKcal,
                Instruction = x.Instruction,

                DeliveryId = x.Deliveries
                    .Where(d => !d.IsDelete && d.MealScheduleId == mealScheduleId &&
                                d.ServiceDate == date)
                    .Select(d => (Guid?)d.Id).FirstOrDefault(),
                DeliveryStatus = x.Deliveries
                    .Where(d => !d.IsDelete && d.MealScheduleId == mealScheduleId &&
                                d.ServiceDate == date)
                    .Select(d => (GzMealDeliveryStatus?)d.Status).FirstOrDefault(),
                DeliveredAt = x.Deliveries
                    .Where(d => !d.IsDelete && d.MealScheduleId == mealScheduleId &&
                                d.ServiceDate == date)
                    .Select(d => d.DeliveredAt).FirstOrDefault(),
                LeftoverPercent = x.Deliveries
                    .Where(d => !d.IsDelete && d.MealScheduleId == mealScheduleId &&
                                d.ServiceDate == date)
                    .Select(d => d.LeftoverPercent).FirstOrDefault(),
                DeliveryNote = x.Deliveries
                    .Where(d => !d.IsDelete && d.MealScheduleId == mealScheduleId &&
                                d.ServiceDate == date)
                    .Select(d => d.Note).FirstOrDefault()
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<GzMealDistributionRowResponse> RecordDeliveryAsync(
        RecordGzMealDeliveryRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        var actorUserId = GetCurrentUserId();
        var date = request.ServiceDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var diet = await _dbContext.GzPatientDiets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.PatientDietId && !x.IsDelete,
                cancellationToken)
            ?? throw new KeyNotFoundException("Diet pasien tidak ditemukan.");

        if (diet.Status != GzPatientDietStatus.Active)
            throw new NutritionConflictException("GIZ004",
                "Makanan hanya dapat dicatat untuk diet yang sedang berlaku.");

        var existing = await _dbContext.GzMealDeliveries
            .FirstOrDefaultAsync(x => x.PatientDietId == request.PatientDietId &&
                                      x.MealScheduleId == request.MealScheduleId &&
                                      x.ServiceDate == date && !x.IsDelete, cancellationToken);

        var now = DateTime.UtcNow;

        if (existing != null)
        {
            // Pencatatan ulang pada jadwal yang sama memperbarui baris yang ada, bukan
            // menambah baris baru, supaya rekap sisa makanan tidak terhitung dua kali.
            existing.Status = request.Status;
            existing.DeliveredAt = request.Status == GzMealDeliveryStatus.Delivered ? now : null;
            existing.DeliveredByWorkforceId = request.DeliveredByWorkforceId;
            existing.LeftoverPercent = request.LeftoverPercent;
            existing.Note = Normalize(request.Note);
            existing.UpdateDateTime = now;
            existing.UpdateBy = actorUserId;
        }
        else
        {
            _dbContext.GzMealDeliveries.Add(new GzMealDelivery
            {
                Id = DeterministicId(request.IdempotencyKey),
                PatientDietId = request.PatientDietId,
                MealScheduleId = request.MealScheduleId,
                ServiceDate = date,
                Status = request.Status,
                DeliveredAt = request.Status == GzMealDeliveryStatus.Delivered ? now : null,
                DeliveredByWorkforceId = request.DeliveredByWorkforceId,
                LeftoverPercent = request.LeftoverPercent,
                Note = Normalize(request.Note),
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }

        await SaveAsync(cancellationToken);

        var rows = await GetDistributionAsync(request.MealScheduleId, date, cancellationToken);
        return rows.First(x => x.PatientDietId == request.PatientDietId);
    }

    // ============================================================== penolong

    private IQueryable<GzPatientDiet> BuildDietQuery() =>
        _dbContext.GzPatientDiets.AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.DietType)
            .Include(x => x.FoodForm)
            .Include(x => x.PrescribedByWorkforce)
            .Where(x => !x.IsDelete);

    private async Task EnsureMasterActiveAsync(Guid dietTypeId, Guid foodFormId,
        CancellationToken cancellationToken)
    {
        var dietValid = await _dbContext.GzDietTypes.AsNoTracking()
            .AnyAsync(x => x.Id == dietTypeId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!dietValid)
            throw new NutritionUnprocessableException("GIZ014",
                "Jenis diet tidak ditemukan atau tidak aktif.");

        var formValid = await _dbContext.GzFoodForms.AsNoTracking()
            .AnyAsync(x => x.Id == foodFormId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!formValid)
            throw new NutritionUnprocessableException("GIZ014",
                "Bentuk makanan tidak ditemukan atau tidak aktif.");
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new NutritionConflictException("GIZ012",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        }
    }

    private Guid GetCurrentUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("user_id");
        if (!Guid.TryParse(value, out var id) || id == Guid.Empty)
            throw new NutritionForbiddenException("Identitas pengguna tidak valid.");
        return id;
    }

    private static void EnsureIdempotencyKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Idempotency key wajib diisi.");
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Guid DeterministicId(string key) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"GzDiet:{key.Trim()}"))[..16]);

    private static GzPatientDietResponse MapDiet(GzPatientDiet x) => new()
    {
        Id = x.Id,
        NutritionOrderId = x.NutritionOrderId,
        PatientId = x.PatientId,
        PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
        MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
        DietTypeId = x.DietTypeId,
        DietTypeName = x.DietType != null ? x.DietType.DietTypeName : string.Empty,
        FoodFormId = x.FoodFormId,
        FoodFormName = x.FoodForm != null ? x.FoodForm.FoodFormName : string.Empty,
        EnergyRequirementKcal = x.EnergyRequirementKcal,
        Instruction = x.Instruction,
        Status = x.Status,
        StartAt = x.StartAt,
        EndAt = x.EndAt,
        ChangeReason = x.ChangeReason,
        PrescribedByName = x.PrescribedByWorkforce != null
            ? x.PrescribedByWorkforce.DisplayName : string.Empty,
        Version = x.Version
    };
}
