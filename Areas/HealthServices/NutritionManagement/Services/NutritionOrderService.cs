using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;

/// <summary>
/// Asuhan gizi pasien rawat inap: order konsultasi, kunjungan ahli gizi, dan penutupan.
/// </summary>
/// <remarks>
/// Seluruh perintah yang mengubah data bersifat idempoten lewat <c>IdempotencyKey</c>, dan
/// memakai <c>ExpectedVersion</c> sebagai penjaga agar dua petugas tidak saling menimpa
/// tanpa sadar.
/// </remarks>
public sealed class NutritionOrderService
{
    private const string LogCategory = "NutritionManagement";
    private const string DiagnosisTypeNutrition = "NUTRITION";

    private const string CreateAction = "CreateOrder";
    private const string UpdateAction = "UpdateOrder";
    private const string CloseAction = "CloseOrder";
    private const string CancelAction = "CancelOrder";
    private const string RecordAction = "SaveCareRecord";

    private static readonly GzOrderStatus[] OpenStatuses =
        [GzOrderStatus.Requested, GzOrderStatus.InProgress];

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;

    public NutritionOrderService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
    }

    // ================================================================== pembacaan

    public async Task<PagedResult<GzOrderSummaryResponse>> GetPagedAsync(
        GzOrderPagedQuery request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.GzNutritionOrders.AsNoTracking().Where(x => !x.IsDelete);

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.PatientId.HasValue) query = query.Where(x => x.PatientId == request.PatientId);
        if (request.AssignedWorkforceId.HasValue)
            query = query.Where(x => x.AssignedWorkforceId == request.AssignedWorkforceId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.OrderNumber.ToLower().Contains(keyword) ||
                (x.Patient != null && x.Patient.FullName.ToLower().Contains(keyword)) ||
                (x.Patient != null && x.Patient.MedicalRecordNumber.ToLower().Contains(keyword)));
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.RequestedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => MapSummaryExpression(x))
            .ToListAsync(cancellationToken);

        return new PagedResult<GzOrderSummaryResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<GzOrderDetailResponse?> GetDetailAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadOrderAsync(id, tracking: false, cancellationToken);
        return entity == null ? null : MapDetail(entity);
    }

    /// <summary>
    /// Pasien rawat inap yang skrining gizinya menunjukkan risiko tetapi belum punya order.
    /// </summary>
    /// <remarks>
    /// Inilah yang membuat order lahir dari hasil skrining, bukan dari ingatan petugas
    /// (`GIZ-DEC-003`). Modul Gizi hanya MEMBACA asesmen; pengisiannya milik keperawatan.
    /// </remarks>
    public async Task<List<GzScreeningCandidateResponse>> GetScreeningCandidatesAsync(
        CancellationToken cancellationToken = default)
    {
        var atRisk = new[]
        {
            ClinicalManagement.Enums.NutritionRiskStatus.LowRisk,
            ClinicalManagement.Enums.NutritionRiskStatus.MediumRisk,
            ClinicalManagement.Enums.NutritionRiskStatus.HighRisk
        };

        return await _dbContext.Set<TrxPatientAssessment>().AsNoTracking()
            .Where(a => !a.IsDelete && atRisk.Contains(a.NutritionRiskStatus))
            .Where(a => !_dbContext.GzNutritionOrders
                .Any(o => o.EncounterId == a.EncounterId && !o.IsDelete &&
                          OpenStatuses.Contains(o.Status)))
            .OrderByDescending(a => a.CreateDateTime)
            .Take(100)
            .Select(a => new GzScreeningCandidateResponse
            {
                PatientId = a.PatientId,
                PatientName = a.Patient != null ? a.Patient.FullName : string.Empty,
                MedicalRecordNumber = a.Patient != null ? a.Patient.MedicalRecordNumber : string.Empty,
                EncounterId = a.EncounterId,
                EncounterNumber = a.Encounter != null ? a.Encounter.EncounterNumber : string.Empty,
                RiskStatus = a.NutritionRiskStatus,
                RiskScore = a.NutritionRiskScore,
                AssessedAt = a.CreateDateTime
            })
            .ToListAsync(cancellationToken);
    }

    // ================================================================== perintah

    public async Task<GzOrderDetailResponse> CreateAsync(CreateGzOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        if (string.IsNullOrWhiteSpace(request.ReasonForReferral))
            throw new NutritionUnprocessableException("GIZ003", "Alasan rujukan wajib diisi.");

        var actorUserId = GetCurrentUserId();
        var fingerprint = Hash(string.Join('|', request.PatientId, request.EncounterId,
            request.RequesterDoctorId, request.AssignedWorkforceId, (int)request.Priority,
            request.ReasonForReferral.Trim()));

        var prior = await FindIdempotentAsync(CreateAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(prior.NutritionOrderId, cancellationToken))!;
        }

        await ValidateReferencesAsync(request.PatientId, request.EncounterId,
            request.RequesterDoctorId, request.AssignedWorkforceId, cancellationToken);

        // `GIZ002`. Diperiksa di sini agar pesannya jelas, dan ditegakkan sekali lagi oleh
        // indeks unik tersaring di basis data agar dua permintaan bersamaan tidak lolos.
        var alreadyOpen = await _dbContext.GzNutritionOrders.AsNoTracking()
            .AnyAsync(x => x.EncounterId == request.EncounterId && !x.IsDelete &&
                           OpenStatuses.Contains(x.Status), cancellationToken);
        if (alreadyOpen)
            throw new NutritionConflictException("GIZ002",
                "Episode rawat inap ini sudah punya order konsultasi gizi yang masih berjalan.");

        var screening = await ReadLatestScreeningAsync(request.EncounterId, cancellationToken);
        var now = DateTime.UtcNow;
        var id = DeterministicId(request.IdempotencyKey);

        var entity = new GzNutritionOrder
        {
            Id = id,
            OrderNumber = $"GZ-{id:N}"[..20],
            PatientId = request.PatientId,
            EncounterId = request.EncounterId,
            RequesterDoctorId = request.RequesterDoctorId,
            AssignedWorkforceId = request.AssignedWorkforceId,
            Status = GzOrderStatus.Requested,
            Priority = request.Priority,
            ReasonForReferral = request.ReasonForReferral.Trim(),
            ScreeningRiskStatus = screening.RiskStatus,
            ScreeningScore = screening.Score,
            RequestedAt = now,
            Version = 0,
            CreateDateTime = now,
            CreateBy = actorUserId
        };

        _dbContext.GzNutritionOrders.Add(entity);
        _dbContext.GzNutritionOrderHistories.Add(NewHistory(entity.Id, GzOrderStatus.Requested,
            null, CreateAction, null, request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "NutritionOrder.Create",
            "Membuat order konsultasi gizi.",
            new { entity.Id, entity.OrderNumber, ActorUserId = actorUserId });

        return (await GetDetailAsync(entity.Id, cancellationToken))!;
    }

    public async Task<GzOrderDetailResponse> UpdateAsync(Guid id, UpdateGzOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        if (string.IsNullOrWhiteSpace(request.ReasonForReferral))
            throw new NutritionUnprocessableException("GIZ003", "Alasan rujukan wajib diisi.");

        var actorUserId = GetCurrentUserId();
        var fingerprint = Hash(string.Join('|', request.AssignedWorkforceId,
            (int)request.Priority, request.ReasonForReferral.Trim()));

        var prior = await FindIdempotentAsync(UpdateAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            if (prior.NutritionOrderId != id)
                throw new NutritionConflictException("GIZ013",
                    "Idempotency key sudah dipakai untuk order lain.");
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadOrderAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Order konsultasi gizi tidak ditemukan.");
        EnsureOpen(entity);
        EnsureVersion(entity.Version, request.ExpectedVersion);

        if (request.AssignedWorkforceId.HasValue)
            await EnsureWorkforceActiveAsync(request.AssignedWorkforceId.Value, cancellationToken);

        var now = DateTime.UtcNow;
        entity.AssignedWorkforceId = request.AssignedWorkforceId;
        entity.Priority = request.Priority;
        entity.ReasonForReferral = request.ReasonForReferral.Trim();
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.GzNutritionOrderHistories.Add(NewHistory(entity.Id, entity.Status,
            entity.Status, UpdateAction, null, request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        return (await GetDetailAsync(id, cancellationToken))!;
    }

    public async Task<GzOrderDetailResponse> CloseAsync(Guid id, CloseGzOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        if (string.IsNullOrWhiteSpace(request.ClosingNote))
            throw new NutritionUnprocessableException("GIZ008",
                "Catatan penutup wajib diisi saat menutup asuhan gizi.");

        var actorUserId = GetCurrentUserId();
        var note = request.ClosingNote.Trim();
        var fingerprint = Hash(note);

        var prior = await FindIdempotentAsync(CloseAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadOrderAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Order konsultasi gizi tidak ditemukan.");
        EnsureOpen(entity);
        EnsureVersion(entity.Version, request.ExpectedVersion);

        var now = DateTime.UtcNow;
        var from = entity.Status;
        entity.Status = GzOrderStatus.Closed;
        entity.ClosedAt = now;
        entity.ClosingNote = note;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.GzNutritionOrderHistories.Add(NewHistory(entity.Id, GzOrderStatus.Closed,
            from, CloseAction, note, request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "NutritionOrder.Close",
            "Menutup asuhan gizi.", new { entity.Id, entity.OrderNumber, ActorUserId = actorUserId });

        return (await GetDetailAsync(id, cancellationToken))!;
    }

    public async Task<GzOrderDetailResponse> CancelAsync(Guid id, CancelGzOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new NutritionUnprocessableException("GIZ009",
                "Alasan pembatalan wajib diisi.");

        var actorUserId = GetCurrentUserId();
        var reason = request.Reason.Trim();
        var fingerprint = Hash(reason);

        var prior = await FindIdempotentAsync(CancelAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadOrderAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Order konsultasi gizi tidak ditemukan.");
        EnsureOpen(entity);
        EnsureVersion(entity.Version, request.ExpectedVersion);

        var now = DateTime.UtcNow;
        var from = entity.Status;
        entity.Status = GzOrderStatus.Cancelled;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.GzNutritionOrderHistories.Add(NewHistory(entity.Id, GzOrderStatus.Cancelled,
            from, CancelAction, reason, request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        return (await GetDetailAsync(id, cancellationToken))!;
    }

    /// <summary>
    /// Mencatat satu kunjungan ahli gizi. Kunjungan pertama menaikkan status order menjadi
    /// <c>InProgress</c> secara otomatis, bukan lewat tombol tersendiri.
    /// </summary>
    public async Task<GzCareRecordResponse> SaveCareRecordAsync(Guid orderId,
        SaveGzCareRecordRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        var actorUserId = GetCurrentUserId();
        var fingerprint = Hash(string.Join('|', request.RecordedByWorkforceId, request.Weight,
            request.Height, request.NutritionDiagnosisId, request.EnergyRequirementKcal,
            request.IntakePercent, Normalize(request.AssessmentNote),
            Normalize(request.InterventionNote), Normalize(request.DietPrescription),
            Normalize(request.IntakeRecallNote), Normalize(request.EvaluationNote)));

        var prior = await FindIdempotentAsync(RecordAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            var existing = await _dbContext.GzNutritionCareRecords.AsNoTracking()
                .Where(x => x.NutritionOrderId == orderId && !x.IsDelete)
                .OrderByDescending(x => x.VisitSequence)
                .FirstAsync(cancellationToken);
            return MapRecord(existing);
        }

        var entity = await LoadOrderAsync(orderId, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Order konsultasi gizi tidak ditemukan.");

        if (!OpenStatuses.Contains(entity.Status))
            throw new NutritionConflictException("GIZ004",
                "Kunjungan hanya dapat dicatat pada order yang masih berjalan.");

        await EnsureWorkforceActiveAsync(request.RecordedByWorkforceId, cancellationToken);
        if (request.NutritionDiagnosisId.HasValue)
            await EnsureNutritionDiagnosisAsync(request.NutritionDiagnosisId.Value, cancellationToken);

        var now = DateTime.UtcNow;
        var sequence = entity.CareRecords.Count(x => !x.IsDelete) + 1;

        var record = new GzNutritionCareRecord
        {
            Id = DeterministicId(request.IdempotencyKey),
            NutritionOrderId = entity.Id,
            VisitSequence = sequence,
            VisitAt = request.VisitAt?.ToUniversalTime() ?? now,
            RecordedByWorkforceId = request.RecordedByWorkforceId,
            RecordType = sequence == 1 ? GzCareRecordType.Initial : GzCareRecordType.FollowUp,
            Weight = request.Weight,
            Height = request.Height,
            Bmi = ComputeBmi(request.Weight, request.Height),
            AssessmentNote = Normalize(request.AssessmentNote),
            NutritionDiagnosisId = request.NutritionDiagnosisId,
            DiagnosisNote = Normalize(request.DiagnosisNote),
            InterventionNote = Normalize(request.InterventionNote),
            DietPrescription = Normalize(request.DietPrescription),
            EnergyRequirementKcal = request.EnergyRequirementKcal,
            IntakeRecallNote = Normalize(request.IntakeRecallNote),
            IntakePercent = request.IntakePercent,
            EvaluationNote = Normalize(request.EvaluationNote),
            Version = 0,
            CreateDateTime = now,
            CreateBy = actorUserId
        };

        // Ditambahkan lewat DbSet, bukan lewat navigasi induk yang sudah dilacak, agar
        // entity baru pasti berstatus Added walaupun kuncinya diisi dari sisi aplikasi.
        _dbContext.GzNutritionCareRecords.Add(record);

        var from = entity.Status;
        if (entity.Status == GzOrderStatus.Requested)
        {
            entity.Status = GzOrderStatus.InProgress;
        }

        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.GzNutritionOrderHistories.Add(NewHistory(entity.Id, entity.Status, from,
            RecordAction, $"Kunjungan ke-{sequence}", request.IdempotencyKey, fingerprint,
            actorUserId, now));

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "NutritionOrder.SaveCareRecord",
            "Mencatat kunjungan ahli gizi.",
            new { entity.Id, entity.OrderNumber, record.VisitSequence, ActorUserId = actorUserId });

        return MapRecord(record);
    }

    // ================================================================== penolong

    private async Task<GzNutritionOrder?> LoadOrderAsync(Guid id, bool tracking,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.GzNutritionOrders
            .Include(x => x.Patient)
            .Include(x => x.RequesterDoctor)
            .Include(x => x.AssignedWorkforce)
            .Include(x => x.CareRecords.Where(r => !r.IsDelete))
                .ThenInclude(r => r.NutritionDiagnosis)
            .Include(x => x.CareRecords.Where(r => !r.IsDelete))
                .ThenInclude(r => r.RecordedByWorkforce)
            .Include(x => x.Histories.Where(h => !h.IsDelete))
            .Where(x => x.Id == id && !x.IsDelete);

        if (!tracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private async Task ValidateReferencesAsync(Guid patientId, Guid encounterId,
        Guid doctorId, Guid? workforceId, CancellationToken cancellationToken)
    {
        var encounterValid = await _dbContext.Set<TrxPatientEncounter>().AsNoTracking()
            .AnyAsync(x => x.Id == encounterId && x.PatientId == patientId && !x.IsDelete,
                cancellationToken);
        if (!encounterValid)
            throw new NutritionUnprocessableException("GIZ001",
                "Kunjungan tidak ditemukan atau tidak sesuai dengan pasien.");

        var doctorValid = await _dbContext.MstDoctors.AsNoTracking()
            .AnyAsync(x => x.Id == doctorId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!doctorValid)
            throw new NutritionUnprocessableException("GIZ001",
                "Dokter pemohon tidak ditemukan atau tidak aktif.");

        if (workforceId.HasValue)
            await EnsureWorkforceActiveAsync(workforceId.Value, cancellationToken);
    }

    private async Task EnsureWorkforceActiveAsync(Guid workforceId, CancellationToken cancellationToken)
    {
        var valid = await _dbContext.MstWorkforceProfiles.AsNoTracking()
            .AnyAsync(x => x.Id == workforceId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!valid)
            throw new NutritionUnprocessableException("GIZ001",
                "Ahli gizi tidak ditemukan atau tidak aktif.");
    }

    /// <summary>
    /// Diagnosis gizi harus berasal dari master bertipe <c>NUTRITION</c> (`GIZ-DEC-009`).
    /// </summary>
    private async Task EnsureNutritionDiagnosisAsync(Guid diagnosisId,
        CancellationToken cancellationToken)
    {
        var valid = await _dbContext.Set<MstDiagnosis>().AsNoTracking()
            .AnyAsync(x => x.Id == diagnosisId && !x.IsDelete &&
                           x.DiagnosisType == DiagnosisTypeNutrition, cancellationToken);
        if (!valid)
            throw new NutritionUnprocessableException("GIZ005",
                "Diagnosis yang dipilih bukan diagnosis gizi.");
    }

    private async Task<(ClinicalManagement.Enums.NutritionRiskStatus? RiskStatus, int? Score)>
        ReadLatestScreeningAsync(Guid encounterId, CancellationToken cancellationToken)
    {
        var assessment = await _dbContext.Set<TrxPatientAssessment>().AsNoTracking()
            .Where(x => x.EncounterId == encounterId && !x.IsDelete)
            .OrderByDescending(x => x.CreateDateTime)
            .Select(x => new { x.NutritionRiskStatus, x.NutritionRiskScore })
            .FirstOrDefaultAsync(cancellationToken);

        return assessment == null
            ? (null, null)
            : (assessment.NutritionRiskStatus, assessment.NutritionRiskScore);
    }

    private static void EnsureOpen(GzNutritionOrder entity)
    {
        if (!OpenStatuses.Contains(entity.Status))
            throw new NutritionConflictException("GIZ004",
                "Order yang sudah ditutup atau dibatalkan tidak dapat diubah lagi.");
    }

    private static void EnsureVersion(int current, int expected)
    {
        if (current != expected)
            throw new NutritionConflictException("GIZ012",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
    }

    private static void EnsureIdempotencyKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Idempotency key wajib diisi.");
    }

    private static void EnsureSameFingerprint(string source, string fingerprint)
    {
        if (!string.Equals(source, BuildSource(fingerprint), StringComparison.Ordinal))
            throw new NutritionConflictException("GIZ013",
                "Idempotency key dipakai dengan isi permintaan yang berbeda.");
    }

    private Task<GzNutritionOrderHistory?> FindIdempotentAsync(string action, string key,
        CancellationToken cancellationToken) =>
        _dbContext.GzNutritionOrderHistories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Action == action && x.CorrelationId == key.Trim() &&
                                      !x.IsDelete, cancellationToken);

    private static GzNutritionOrderHistory NewHistory(Guid orderId, GzOrderStatus to,
        GzOrderStatus? from, string action, string? reason, string idempotencyKey,
        string fingerprint, Guid actorUserId, DateTime now) => new()
        {
            NutritionOrderId = orderId,
            FromStatus = from,
            ToStatus = to,
            Action = action,
            Reason = reason,
            ActorUserId = actorUserId,
            OccurredAt = now,
            Source = BuildSource(fingerprint),
            CorrelationId = idempotencyKey.Trim(),
            CreateDateTime = now,
            CreateBy = actorUserId
        };

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

    /// <summary>
    /// BMI dihitung karena rumusnya universal dan tidak berbeda antar rumah sakit — berbeda
    /// dengan kebutuhan energi, yang sengaja diketik ahli gizi (`GIZ-DEC-012`).
    /// </summary>
    private static decimal? ComputeBmi(decimal? weightKg, decimal? heightCm)
    {
        if (!weightKg.HasValue || !heightCm.HasValue || heightCm.Value <= 0) return null;
        var meters = heightCm.Value / 100m;
        return Math.Round(weightKg.Value / (meters * meters), 2);
    }

    private static string BuildSource(string fingerprint) => $"API:{fingerprint[..46]}";
    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static Guid DeterministicId(string key) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"GzNutrition:{key.Trim()}"))[..16]);

    private static GzOrderSummaryResponse MapSummaryExpression(GzNutritionOrder x) => new()
    {
        Id = x.Id,
        OrderNumber = x.OrderNumber,
        PatientId = x.PatientId,
        PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
        MedicalRecordNumber = x.Patient != null ? x.Patient.MedicalRecordNumber : string.Empty,
        EncounterId = x.EncounterId,
        RequesterDoctorName = x.RequesterDoctor != null ? x.RequesterDoctor.FullName : string.Empty,
        AssignedWorkforceName = x.AssignedWorkforce != null ? x.AssignedWorkforce.DisplayName : null,
        Status = x.Status,
        Priority = x.Priority,
        ScreeningRiskStatus = x.ScreeningRiskStatus,
        RequestedAt = x.RequestedAt,
        VisitCount = x.CareRecords.Count(r => !r.IsDelete),
        LastVisitAt = x.CareRecords.Where(r => !r.IsDelete)
            .OrderByDescending(r => r.VisitAt).Select(r => (DateTime?)r.VisitAt).FirstOrDefault(),
        Version = x.Version
    };

    private static GzOrderDetailResponse MapDetail(GzNutritionOrder x) => new()
    {
        Id = x.Id,
        OrderNumber = x.OrderNumber,
        PatientId = x.PatientId,
        PatientName = x.Patient?.FullName ?? string.Empty,
        MedicalRecordNumber = x.Patient?.MedicalRecordNumber ?? string.Empty,
        EncounterId = x.EncounterId,
        RequesterDoctorId = x.RequesterDoctorId,
        RequesterDoctorName = x.RequesterDoctor?.FullName ?? string.Empty,
        AssignedWorkforceId = x.AssignedWorkforceId,
        AssignedWorkforceName = x.AssignedWorkforce?.DisplayName,
        Status = x.Status,
        Priority = x.Priority,
        ReasonForReferral = x.ReasonForReferral,
        ScreeningRiskStatus = x.ScreeningRiskStatus,
        ScreeningScore = x.ScreeningScore,
        RequestedAt = x.RequestedAt,
        ClosedAt = x.ClosedAt,
        ClosingNote = x.ClosingNote,
        VisitCount = x.CareRecords.Count(r => !r.IsDelete),
        LastVisitAt = x.CareRecords.Where(r => !r.IsDelete)
            .OrderByDescending(r => r.VisitAt).Select(r => (DateTime?)r.VisitAt).FirstOrDefault(),
        Version = x.Version,
        CareRecords = [.. x.CareRecords.Where(r => !r.IsDelete)
            .OrderBy(r => r.VisitSequence).Select(MapRecord)],
        Histories = [.. x.Histories.Where(h => !h.IsDelete)
            .OrderByDescending(h => h.OccurredAt)
            .Select(h => new GzOrderHistoryResponse
            {
                Id = h.Id, FromStatus = h.FromStatus, ToStatus = h.ToStatus,
                Action = h.Action, Reason = h.Reason, OccurredAt = h.OccurredAt
            })]
    };

    private static GzCareRecordResponse MapRecord(GzNutritionCareRecord r) => new()
    {
        Id = r.Id,
        NutritionOrderId = r.NutritionOrderId,
        VisitSequence = r.VisitSequence,
        VisitAt = r.VisitAt,
        RecordedByWorkforceId = r.RecordedByWorkforceId,
        RecordedByName = r.RecordedByWorkforce?.DisplayName ?? string.Empty,
        RecordType = r.RecordType,
        Weight = r.Weight,
        Height = r.Height,
        Bmi = r.Bmi,
        AssessmentNote = r.AssessmentNote,
        NutritionDiagnosisId = r.NutritionDiagnosisId,
        NutritionDiagnosisName = r.NutritionDiagnosis?.DiagnosisName,
        DiagnosisNote = r.DiagnosisNote,
        InterventionNote = r.InterventionNote,
        DietPrescription = r.DietPrescription,
        EnergyRequirementKcal = r.EnergyRequirementKcal,
        IntakeRecallNote = r.IntakeRecallNote,
        IntakePercent = r.IntakePercent,
        EvaluationNote = r.EvaluationNote,
        ProgressNoteId = r.ProgressNoteId,
        Version = r.Version
    };
}
