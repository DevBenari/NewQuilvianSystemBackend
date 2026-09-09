using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;

/// <summary>
/// Alur makanan pasien rawat inap: daftar pasien, diet, produksi, dan distribusi.
/// </summary>
public sealed class NutritionDietService
{
    private const string LogCategory = "NutritionManagement";

    /// <summary>
    /// Episode yang dianggap masih dirawat. Pasien yang sudah pulang atau batal tidak
    /// pernah masuk daftar gizi maupun perhitungan produksi.
    /// </summary>
    private static readonly InpEpisodeStatus[] ActiveEpisodeStatuses =
        [InpEpisodeStatus.Admitted, InpEpisodeStatus.DischargePending];

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

    // ==================================================== 1. daftar pasien gizi

    /// <summary>
    /// Seluruh pasien rawat inap yang masih dirawat, beserta diet aktifnya bila ada.
    /// </summary>
    /// <remarks>
    /// Daftar ini BUKAN daftar order konsultasi gizi. Ia mencakup semua pasien rawat inap
    /// karena setiap pasien yang dirawat perlu makan, sementara order konsultasi hanya
    /// untuk pasien yang secara khusus dirujuk ke ahli gizi.
    /// </remarks>
    public async Task<PagedResult<GzNutritionPatientResponse>> GetNutritionPatientsAsync(
        GzNutritionPatientQuery request, CancellationToken cancellationToken = default)
    {
        var query =
            from episode in _dbContext.Set<InpEpisode>().AsNoTracking()
            where !episode.IsDelete && ActiveEpisodeStatuses.Contains(episode.EpisodeStatus)
            select episode;

        if (request.ServiceUnitId.HasValue)
            query = query.Where(x => x.ServiceUnitId == request.ServiceUnitId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Patient != null &&
                (x.Patient.FullName.ToLower().Contains(keyword) ||
                 x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)));
        }

        var projected = query.Select(episode => new GzNutritionPatientResponse
        {
            PatientId = episode.PatientId,
            EncounterId = episode.EncounterId,
            EpisodeId = episode.Id,
            EpisodeNumber = episode.EpisodeNumber,
            MedicalRecordNumber = episode.Patient != null ? episode.Patient.MedicalRecordNumber : string.Empty,
            PatientName = episode.Patient != null ? episode.Patient.FullName : string.Empty,
            EpisodeStatus = episode.EpisodeStatus.ToString(),
            AdmittedAt = episode.AdmittedAt,

            // Penempatan dan DPJP yang sedang berjalan dibaca dari modul rawat inap,
            // tidak disalin ke tabel Gizi.
            RoomName = _dbContext.Set<InpBedPlacement>()
                .Where(p => p.EpisodeId == episode.Id && !p.IsDelete && p.EndDateTime == null)
                .OrderByDescending(p => p.SequenceNumber)
                .Select(p => p.Room != null ? p.Room.RoomName : null).FirstOrDefault(),
            BedName = _dbContext.Set<InpBedPlacement>()
                .Where(p => p.EpisodeId == episode.Id && !p.IsDelete && p.EndDateTime == null)
                .OrderByDescending(p => p.SequenceNumber)
                .Select(p => p.Bed != null ? p.Bed.BedName : null).FirstOrDefault(),
            DoctorName = _dbContext.Set<InpDoctorAssignment>()
                .Where(d => d.EpisodeId == episode.Id && !d.IsDelete && d.EndDateTime == null)
                .OrderByDescending(d => d.SequenceNumber)
                .Select(d => d.Doctor != null ? d.Doctor.FullName : null).FirstOrDefault(),

            PatientDietId = _dbContext.GzPatientDiets
                .Where(d => d.EncounterId == episode.EncounterId && !d.IsDelete &&
                            d.Status == GzPatientDietStatus.Active)
                .Select(d => (Guid?)d.Id).FirstOrDefault(),
            DietTypeName = _dbContext.GzPatientDiets
                .Where(d => d.EncounterId == episode.EncounterId && !d.IsDelete &&
                            d.Status == GzPatientDietStatus.Active)
                .Select(d => d.DietType != null ? d.DietType.DietTypeName : null).FirstOrDefault(),
            FoodFormName = _dbContext.GzPatientDiets
                .Where(d => d.EncounterId == episode.EncounterId && !d.IsDelete &&
                            d.Status == GzPatientDietStatus.Active)
                .Select(d => d.FoodForm != null ? d.FoodForm.FoodFormName : null).FirstOrDefault(),
            EnergyRequirementKcal = _dbContext.GzPatientDiets
                .Where(d => d.EncounterId == episode.EncounterId && !d.IsDelete &&
                            d.Status == GzPatientDietStatus.Active)
                .Select(d => d.EnergyRequirementKcal).FirstOrDefault(),
            DietInstruction = _dbContext.GzPatientDiets
                .Where(d => d.EncounterId == episode.EncounterId && !d.IsDelete &&
                            d.Status == GzPatientDietStatus.Active)
                .Select(d => d.Instruction).FirstOrDefault(),
            DietStatus = _dbContext.GzPatientDiets
                .Where(d => d.EncounterId == episode.EncounterId && !d.IsDelete &&
                            d.Status == GzPatientDietStatus.Active)
                .Select(d => (GzPatientDietStatus?)d.Status).FirstOrDefault(),
            DietVersion = _dbContext.GzPatientDiets
                .Where(d => d.EncounterId == episode.EncounterId && !d.IsDelete &&
                            d.Status == GzPatientDietStatus.Active)
                .Select(d => (int?)d.Version).FirstOrDefault()
        });

        if (request.WithoutDiet == true)
            projected = projected.Where(x => x.PatientDietId == null);

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 25 : request.PageSize;

        var total = await projected.CountAsync(cancellationToken);
        var items = await projected
            .OrderBy(x => x.RoomName).ThenBy(x => x.BedName).ThenBy(x => x.PatientName)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<GzNutritionPatientResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    // ========================================================== 2. diet pasien

    /// <summary>Riwayat diet satu kunjungan, terbaru lebih dulu.</summary>
    public async Task<List<GzPatientDietResponse>> GetDietHistoryAsync(Guid encounterId,
        CancellationToken cancellationToken = default) =>
        await BuildDietQuery()
            .Where(x => x.EncounterId == encounterId)
            .OrderByDescending(x => x.StartAt)
            .Select(x => MapDiet(x))
            .ToListAsync(cancellationToken);

    public async Task<GzPatientDietResponse> PrescribeAsync(PrescribeGzDietRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        var actorUserId = GetCurrentUserId();

        var deterministicId = DeterministicId(request.IdempotencyKey);
        var replay = await BuildDietQuery().FirstOrDefaultAsync(x => x.Id == deterministicId,
            cancellationToken);
        if (replay != null) return MapDiet(replay);

        await EnsureActiveInpatientAsync(request.PatientId, request.EncounterId, cancellationToken);
        await EnsureMasterActiveAsync(request.DietTypeId, request.FoodFormId, cancellationToken);

        var now = DateTime.UtcNow;
        var startAt = request.EffectiveStartAt?.ToUniversalTime() ?? now;

        var current = await _dbContext.GzPatientDiets
            .FirstOrDefaultAsync(x => x.EncounterId == request.EncounterId && !x.IsDelete &&
                                      x.Status == GzPatientDietStatus.Active, cancellationToken);

        if (current != null)
        {
            if (string.IsNullOrWhiteSpace(request.ChangeReason))
                throw new NutritionUnprocessableException("GIZ010",
                    "Kunjungan ini sudah punya diet aktif. Alasan perubahan wajib diisi.");

            // Diet lama TIDAK ditimpa. Ia ditutup dan tetap tersimpan sebagai riwayat,
            // sehingga urutan Diet Biasa -> Diabetes -> Lunak -> Puasa tetap dapat dibaca
            // seluruhnya di kemudian hari.
            current.Status = GzPatientDietStatus.Changed;
            current.EndAt = startAt;
            current.ChangeReason = request.ChangeReason.Trim();
            current.Version++;
            current.UpdateDateTime = now;
            current.UpdateBy = actorUserId;
        }

        var diet = new GzPatientDiet
        {
            Id = deterministicId,
            NutritionOrderId = request.NutritionOrderId,
            PatientId = request.PatientId,
            EncounterId = request.EncounterId,
            DietTypeId = request.DietTypeId,
            FoodFormId = request.FoodFormId,
            EnergyRequirementKcal = request.EnergyRequirementKcal,
            Instruction = Normalize(request.Instruction),
            Status = GzPatientDietStatus.Active,
            StartAt = startAt,
            PrescribedByWorkforceId = request.PrescribedByWorkforceId,
            Version = 0,
            CreateDateTime = now,
            CreateBy = actorUserId
        };

        _dbContext.GzPatientDiets.Add(diet);
        await SaveAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "NutritionDiet.Prescribe",
            "Menetapkan diet pasien.",
            new { diet.Id, diet.EncounterId, ActorUserId = actorUserId });

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

    // =========================================================== 3. produksi

    public async Task<List<GzProductionBatchSummaryResponse>> GetBatchesAsync(
        DateOnly? serviceDate, CancellationToken cancellationToken = default)
    {
        var date = serviceDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var batches = await _dbContext.GzProductionBatches.AsNoTracking()
            .Include(x => x.MealSchedule)
            .Where(x => !x.IsDelete && x.ServiceDate == date)
            .OrderBy(x => x.MealSchedule!.ServingTime)
            .ToListAsync(cancellationToken);

        var result = new List<GzProductionBatchSummaryResponse>();
        foreach (var batch in batches)
        {
            result.Add(new GzProductionBatchSummaryResponse
            {
                Id = batch.Id,
                BatchNumber = batch.BatchNumber,
                ServiceDate = batch.ServiceDate,
                MealScheduleId = batch.MealScheduleId,
                MealScheduleName = batch.MealSchedule?.MealScheduleName ?? string.Empty,
                Status = batch.Status,
                TotalPortion = batch.TotalPortion,
                ConfirmedAt = batch.ConfirmedAt,
                ReadyAt = batch.ReadyAt,
                CompletedAt = batch.CompletedAt,
                Version = batch.Version,
                DietChangedCount = await CountDietChangedAsync(batch.Id, cancellationToken)
            });
        }

        return result;
    }

    /// <summary>
    /// Membuat batch produksi beserta salinan keadaan setiap pasien saat ini.
    /// </summary>
    /// <remarks>
    /// Snapshot diambil di sini, sekali. Setelah batch ada, perubahan diet pasien tidak
    /// lagi mengubah isinya — karena dapur sudah bekerja berdasarkan angka ini.
    /// </remarks>
    public async Task<GzProductionBatchDetailResponse> CreateBatchAsync(
        CreateGzProductionBatchRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        var actorUserId = GetCurrentUserId();
        var date = request.ServiceDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var batchId = DeterministicId(request.IdempotencyKey);
        var replay = await _dbContext.GzProductionBatches.AsNoTracking()
            .AnyAsync(x => x.Id == batchId, cancellationToken);
        if (replay) return (await GetBatchDetailAsync(batchId, cancellationToken))!;

        var scheduleValid = await _dbContext.GzMealSchedules.AsNoTracking()
            .AnyAsync(x => x.Id == request.MealScheduleId && x.IsActive && !x.IsDelete,
                cancellationToken);
        if (!scheduleValid)
            throw new NutritionUnprocessableException("GIZ014",
                "Jadwal makan tidak ditemukan atau tidak aktif.");

        var duplicate = await _dbContext.GzProductionBatches.AsNoTracking()
            .AnyAsync(x => x.ServiceDate == date && x.MealScheduleId == request.MealScheduleId &&
                           x.Status != GzProductionBatchStatus.Cancelled && !x.IsDelete,
                cancellationToken);
        if (duplicate)
            throw new NutritionConflictException("GIZ015",
                "Sudah ada batch produksi untuk tanggal dan jadwal makan ini.");

        var candidates = await GetNutritionPatientsAsync(
            new GzNutritionPatientQuery { PageNumber = 1, PageSize = 200 }, cancellationToken);

        var withDiet = candidates.Items.Where(x => x.PatientDietId.HasValue).ToList();
        if (withDiet.Count == 0)
            throw new NutritionUnprocessableException("GIZ016",
                "Tidak ada pasien rawat inap dengan diet aktif. Tetapkan diet lebih dulu.");

        var now = DateTime.UtcNow;
        var batch = new GzProductionBatch
        {
            Id = batchId,
            BatchNumber = $"PRD-{date:yyyyMMdd}-{batchId.ToString("N")[..6].ToUpperInvariant()}",
            ServiceDate = date,
            MealScheduleId = request.MealScheduleId,
            Status = GzProductionBatchStatus.Draft,
            TotalPortion = withDiet.Count,
            Note = Normalize(request.Note),
            Version = 0,
            CreateDateTime = now,
            CreateBy = actorUserId
        };

        _dbContext.GzProductionBatches.Add(batch);

        foreach (var patient in withDiet)
        {
            _dbContext.GzProductionBatchDetails.Add(new GzProductionBatchDetail
            {
                ProductionBatchId = batch.Id,
                PatientId = patient.PatientId,
                EncounterId = patient.EncounterId,
                PatientDietId = patient.PatientDietId!.Value,
                PatientNameSnapshot = patient.PatientName,
                MedicalRecordNumberSnapshot = patient.MedicalRecordNumber,
                RoomNameSnapshot = patient.RoomName,
                BedNameSnapshot = patient.BedName,
                DoctorNameSnapshot = patient.DoctorName,
                DietTypeNameSnapshot = patient.DietTypeName ?? string.Empty,
                FoodFormNameSnapshot = patient.FoodFormName ?? string.Empty,
                EnergyRequirementKcalSnapshot = patient.EnergyRequirementKcal,
                InstructionSnapshot = patient.DietInstruction,
                Portion = 1,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "NutritionProduction.CreateBatch",
            "Membuat batch produksi makanan.",
            new { batch.Id, batch.BatchNumber, batch.TotalPortion, ActorUserId = actorUserId });

        return (await GetBatchDetailAsync(batch.Id, cancellationToken))!;
    }

    /// <summary>Memindahkan batch ke status berikutnya sesuai daur hidup yang sah.</summary>
    public async Task<GzProductionBatchDetailResponse> ChangeBatchStatusAsync(Guid batchId,
        ChangeGzBatchStatusRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        var actorUserId = GetCurrentUserId();

        var batch = await _dbContext.GzProductionBatches
            .FirstOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Batch produksi tidak ditemukan.");

        if (batch.Version != request.ExpectedVersion)
            throw new NutritionConflictException("GIZ012",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");

        EnsureTransitionAllowed(batch.Status, request.Status);

        if (request.Status == GzProductionBatchStatus.Cancelled &&
            string.IsNullOrWhiteSpace(request.Reason))
            throw new NutritionUnprocessableException("GIZ017",
                "Alasan pembatalan batch wajib diisi.");

        var now = DateTime.UtcNow;
        batch.Status = request.Status;
        batch.Version++;
        batch.UpdateDateTime = now;
        batch.UpdateBy = actorUserId;

        if (request.Status == GzProductionBatchStatus.Confirmed) batch.ConfirmedAt = now;
        if (request.Status == GzProductionBatchStatus.ReadyForDistribution) batch.ReadyAt = now;
        if (request.Status == GzProductionBatchStatus.Completed) batch.CompletedAt = now;
        if (request.Status == GzProductionBatchStatus.Cancelled)
            batch.CancelReason = request.Reason!.Trim();

        await SaveAsync(cancellationToken);
        return (await GetBatchDetailAsync(batchId, cancellationToken))!;
    }

    public async Task<GzProductionBatchDetailResponse?> GetBatchDetailAsync(Guid batchId,
        CancellationToken cancellationToken = default)
    {
        var batch = await _dbContext.GzProductionBatches.AsNoTracking()
            .Include(x => x.MealSchedule)
            .Include(x => x.Details.Where(d => !d.IsDelete))
                .ThenInclude(d => d.Deliveries.Where(v => !v.IsDelete))
            .FirstOrDefaultAsync(x => x.Id == batchId && !x.IsDelete, cancellationToken);

        if (batch == null) return null;

        // Diet yang sedang berlaku dibaca terpisah, lalu dibandingkan dengan snapshot.
        // Snapshot tidak pernah diubah; yang dilaporkan hanyalah selisihnya.
        var encounterIds = batch.Details.Select(x => x.EncounterId).ToList();
        var currentDiets = await _dbContext.GzPatientDiets.AsNoTracking()
            .Include(x => x.DietType)
            .Where(x => encounterIds.Contains(x.EncounterId) && !x.IsDelete &&
                        x.Status == GzPatientDietStatus.Active)
            .ToDictionaryAsync(x => x.EncounterId, cancellationToken);

        var portions = batch.Details.Select(detail =>
        {
            currentDiets.TryGetValue(detail.EncounterId, out var currentDiet);
            var delivery = detail.Deliveries.FirstOrDefault();
            var changed = currentDiet != null && currentDiet.Id != detail.PatientDietId;

            return new GzProductionPortionResponse
            {
                Id = detail.Id,
                PatientId = detail.PatientId,
                EncounterId = detail.EncounterId,
                PatientDietId = detail.PatientDietId,
                PatientName = detail.PatientNameSnapshot,
                MedicalRecordNumber = detail.MedicalRecordNumberSnapshot,
                RoomName = detail.RoomNameSnapshot,
                BedName = detail.BedNameSnapshot,
                DoctorName = detail.DoctorNameSnapshot,
                DietTypeName = detail.DietTypeNameSnapshot,
                FoodFormName = detail.FoodFormNameSnapshot,
                EnergyRequirementKcal = detail.EnergyRequirementKcalSnapshot,
                Instruction = detail.InstructionSnapshot,
                Portion = detail.Portion,
                IsDietChangedAfterProduction = changed,
                CurrentDietTypeName = changed ? currentDiet!.DietType?.DietTypeName : null,
                DeliveryId = delivery?.Id,
                DeliveryStatus = delivery?.Status,
                DeliveredAt = delivery?.DeliveredAt,
                LeftoverPercent = delivery?.LeftoverPercent,
                DeliveryNote = delivery?.Note
            };
        })
        .OrderBy(x => x.RoomName).ThenBy(x => x.BedName).ThenBy(x => x.PatientName)
        .ToList();

        return new GzProductionBatchDetailResponse
        {
            Id = batch.Id,
            BatchNumber = batch.BatchNumber,
            ServiceDate = batch.ServiceDate,
            MealScheduleId = batch.MealScheduleId,
            MealScheduleName = batch.MealSchedule?.MealScheduleName ?? string.Empty,
            Status = batch.Status,
            TotalPortion = batch.TotalPortion,
            ConfirmedAt = batch.ConfirmedAt,
            ReadyAt = batch.ReadyAt,
            CompletedAt = batch.CompletedAt,
            Version = batch.Version,
            Note = batch.Note,
            CancelReason = batch.CancelReason,
            DietChangedCount = portions.Count(x => x.IsDietChangedAfterProduction),
            Portions = portions,
            Groups = [.. portions
                .GroupBy(x => new { x.DietTypeName, x.FoodFormName })
                .Select(g => new GzProductionGroupResponse
                {
                    DietTypeName = g.Key.DietTypeName,
                    FoodFormName = g.Key.FoodFormName,
                    Portion = g.Sum(x => x.Portion)
                })
                .OrderBy(x => x.DietTypeName)]
        };
    }

    // ========================================================= 4. distribusi

    public async Task<GzProductionBatchDetailResponse> RecordDeliveryAsync(
        RecordGzMealDeliveryRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        var actorUserId = GetCurrentUserId();

        var detail = await _dbContext.GzProductionBatchDetails.AsNoTracking()
            .Include(x => x.ProductionBatch)
            .FirstOrDefaultAsync(x => x.Id == request.ProductionBatchDetailId && !x.IsDelete,
                cancellationToken)
            ?? throw new KeyNotFoundException("Porsi produksi tidak ditemukan.");

        var batchStatus = detail.ProductionBatch?.Status;
        if (batchStatus is not (GzProductionBatchStatus.ReadyForDistribution
            or GzProductionBatchStatus.Completed))
            throw new NutritionConflictException("GIZ018",
                "Makanan hanya dapat didistribusikan setelah batch siap distribusi.");

        var now = DateTime.UtcNow;
        var existing = await _dbContext.GzMealDeliveries
            .FirstOrDefaultAsync(x => x.ProductionBatchDetailId == request.ProductionBatchDetailId &&
                                      !x.IsDelete, cancellationToken);

        if (existing != null)
        {
            // Pencatatan ulang memperbarui baris yang ada, bukan menambah baris baru,
            // supaya rekap sisa makanan tidak terhitung dua kali.
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
                ProductionBatchDetailId = request.ProductionBatchDetailId,
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
        return (await GetBatchDetailAsync(detail.ProductionBatchId, cancellationToken))!;
    }

    // ============================================================== penolong

    private Task<int> CountDietChangedAsync(Guid batchId, CancellationToken cancellationToken) =>
        _dbContext.GzProductionBatchDetails.AsNoTracking()
            .Where(d => d.ProductionBatchId == batchId && !d.IsDelete)
            .CountAsync(d => _dbContext.GzPatientDiets.Any(cur =>
                cur.EncounterId == d.EncounterId && !cur.IsDelete &&
                cur.Status == GzPatientDietStatus.Active && cur.Id != d.PatientDietId),
                cancellationToken);

    /// <summary>
    /// Transisi status batch yang sah. Selain yang tercantum di sini, ditolak — supaya
    /// batch yang sudah selesai tidak dapat dikembalikan menjadi draft dan menghapus jejak.
    /// </summary>
    private static void EnsureTransitionAllowed(GzProductionBatchStatus from,
        GzProductionBatchStatus to)
    {
        var allowed = from switch
        {
            GzProductionBatchStatus.Draft =>
                new[] { GzProductionBatchStatus.Confirmed, GzProductionBatchStatus.Cancelled },
            GzProductionBatchStatus.Confirmed =>
                [GzProductionBatchStatus.InProduction, GzProductionBatchStatus.Cancelled],
            GzProductionBatchStatus.InProduction =>
                [GzProductionBatchStatus.ReadyForDistribution, GzProductionBatchStatus.Cancelled],
            GzProductionBatchStatus.ReadyForDistribution =>
                [GzProductionBatchStatus.Completed],
            _ => []
        };

        if (!allowed.Contains(to))
            throw new NutritionConflictException("GIZ019",
                $"Batch berstatus {from} tidak dapat diubah menjadi {to}.");
    }

    private async Task EnsureActiveInpatientAsync(Guid patientId, Guid encounterId,
        CancellationToken cancellationToken)
    {
        var valid = await _dbContext.Set<InpEpisode>().AsNoTracking()
            .AnyAsync(x => x.EncounterId == encounterId && x.PatientId == patientId &&
                           !x.IsDelete && ActiveEpisodeStatuses.Contains(x.EpisodeStatus),
                cancellationToken);

        if (!valid)
            throw new NutritionUnprocessableException("GIZ001",
                "Kunjungan rawat inap tidak ditemukan, tidak sesuai pasien, atau sudah tidak aktif.");
    }

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

    private IQueryable<GzPatientDiet> BuildDietQuery() =>
        _dbContext.GzPatientDiets.AsNoTracking()
            .Include(x => x.Patient)
            .Include(x => x.DietType)
            .Include(x => x.FoodForm)
            .Include(x => x.PrescribedByWorkforce)
            .Where(x => !x.IsDelete);

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
        EncounterId = x.EncounterId,
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
