using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using static QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services.OperatingRoomCommandSupport;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;

public sealed class OperatingRoomCaseService
{

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;

    private readonly OperatingRoomRuleRelaxation _relaxation;

    public OperatingRoomCaseService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor,
        LoggerService loggerService, OperatingRoomRuleRelaxation relaxation)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
        _relaxation = relaxation;
    }

    public async Task<PagedResult<OprCaseSummaryResponse>> GetPagedAsync(OprCasePagedQuery request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.OprCases.AsNoTracking().Where(x => !x.IsDelete);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.PatientId.HasValue) query = query.Where(x => x.PatientId == request.PatientId);
        if (request.EncounterId.HasValue) query = query.Where(x => x.EncounterId == request.EncounterId);
        if (request.RequestedFrom.HasValue) query = query.Where(x => x.RequestedAt >= request.RequestedFrom.Value);
        if (request.RequestedTo.HasValue) query = query.Where(x => x.RequestedAt <= request.RequestedTo.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(x => x.CaseNumber.ToLower().Contains(search) ||
                (x.Patient != null && x.Patient.FullName.ToLower().Contains(search)));
        }

        var totalData = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.RequestedAt).ThenByDescending(x => x.Id)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new OprCaseSummaryResponse
            {
                Id = x.Id, CaseNumber = x.CaseNumber, PatientId = x.PatientId,
                PatientName = x.Patient != null ? x.Patient.FullName : string.Empty,
                EncounterId = x.EncounterId, CaseType = x.CaseType, Priority = x.Priority,
                Status = x.Status, RequestedAt = x.RequestedAt, Version = x.Version,
                PrimaryProcedureName = x.Procedures.Where(p => p.IsPrimary && !p.IsDelete)
                    .Select(p => p.PatientProcedure != null ? p.PatientProcedure.ProcedureNameSnapshot : string.Empty)
                    .FirstOrDefault() ?? string.Empty
            }).ToListAsync(cancellationToken);

        return new PagedResult<OprCaseSummaryResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = totalData,
            TotalPage = (int)Math.Ceiling(totalData / (double)request.PageSize), Items = items
        };
    }

    public async Task<OprCaseDetailResponse?> GetDetailAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await LoadCaseAsync(id, false, cancellationToken);
        return entity == null ? null : MapDetail(entity);
    }

    public async Task<OprCaseDetailResponse> CreateAsync(CreateOprCaseRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Procedures, request.Indication, request.IdempotencyKey);
        var actorUserId = GetCurrentUserId();
        if (!_relaxation.IsRelaxed)
            EnsureDoctorActor(GetCurrentDoctorId(), request.RequesterDoctorId);
        var fingerprint = BuildFingerprint(request);

        var prior = await FindIdempotentCaseAsync("Request", request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(prior.OprCaseId, cancellationToken))!;
        }

        await ValidateReferencesAsync(request.PatientId, request.EncounterId, request.RequesterDoctorId,
            request.PrimarySurgeonId, request.Procedures, null, cancellationToken);

        var now = DateTime.UtcNow;
        var entity = new OprCase
        {
            Id = CreateDeterministicId(request.IdempotencyKey),
            PatientId = request.PatientId, EncounterId = request.EncounterId,
            RequesterDoctorId = request.RequesterDoctorId, PrimarySurgeonId = request.PrimarySurgeonId,
            CaseType = request.CaseType, Priority = request.Priority, Status = OprCaseStatus.Requested,
            Indication = request.Indication.Trim(), Laterality = Normalize(request.Laterality),
            EstimatedMinutes = request.EstimatedMinutes, RequestedAt = now,
            PreferredAt = request.PreferredAt?.ToUniversalTime(), Version = 0,
            CreateDateTime = now, CreateBy = actorUserId
        };
        entity.CaseNumber = $"OPR-{entity.Id:N}";
        AddProcedures(entity, request.Procedures, actorUserId, now);
        entity.StatusHistories.Add(NewHistory(entity.Id, entity.Status, null, "Request", request.IdempotencyKey,
            fingerprint, actorUserId, now));

        _dbContext.OprCases.Add(entity);
        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException)
        {
            _dbContext.ChangeTracker.Clear();
            var concurrent = await FindIdempotentCaseAsync("Request", request.IdempotencyKey, cancellationToken);
            if (concurrent == null)
                throw new OperatingRoomConflictException("OPR002", "Tindakan sudah diproses pada kasus operasi lain.");
            EnsureSameFingerprint(concurrent.Source, fingerprint);
            return (await GetDetailAsync(concurrent.OprCaseId, cancellationToken))!;
        }
        await _loggerService.AuditAsync(LogCategory, "OperatingRoomCase.Create", "Membuat permintaan kasus operasi.",
            new { entity.Id, entity.CaseNumber, ActorUserId = actorUserId, Status = entity.Status.ToString() });
        return (await GetDetailAsync(entity.Id, cancellationToken))!;
    }

    public async Task<OprCaseDetailResponse> UpdateAsync(Guid id, UpdateOprCaseRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Procedures, request.Indication, request.IdempotencyKey);
        var actorUserId = GetCurrentUserId();
        var actorDoctorId = _relaxation.IsRelaxed ? Guid.Empty : GetCurrentDoctorId();
        var fingerprint = BuildFingerprint(request);
        var prior = await FindIdempotentCaseAsync("UpdateRequest", request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            if (prior.OprCaseId != id)
                throw new OperatingRoomConflictException("OPR013", "Idempotency key sudah digunakan untuk kasus lain.");
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadCaseAsync(id, true, cancellationToken)
            ?? throw new KeyNotFoundException("Kasus operasi tidak ditemukan.");
        if (entity.Status != OprCaseStatus.Requested)
            throw new OperatingRoomConflictException("InvalidStateTransition", "Permintaan hanya dapat diubah pada status Requested.");
        if (entity.Version != request.ExpectedVersion)
            throw new OperatingRoomConflictException("OPR012", "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        if (!_relaxation.IsRelaxed &&
            actorDoctorId != entity.RequesterDoctorId && actorDoctorId != entity.PrimarySurgeonId)
            throw new OperatingRoomForbiddenException("Hanya dokter pemohon atau dokter bedah utama yang boleh mengubah permintaan.");

        await ValidateReferencesAsync(entity.PatientId, entity.EncounterId, request.RequesterDoctorId,
            request.PrimarySurgeonId, request.Procedures, entity.Id, cancellationToken);
        var now = DateTime.UtcNow;
        entity.RequesterDoctorId = request.RequesterDoctorId;
        entity.PrimarySurgeonId = request.PrimarySurgeonId;
        entity.CaseType = request.CaseType;
        entity.Priority = request.Priority;
        entity.Indication = request.Indication.Trim();
        entity.Laterality = Normalize(request.Laterality);
        entity.EstimatedMinutes = request.EstimatedMinutes;
        entity.PreferredAt = request.PreferredAt?.ToUniversalTime();
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;
        _dbContext.OprCaseProcedures.RemoveRange(entity.Procedures);
        entity.Procedures.Clear();
        AddProcedures(entity, request.Procedures, actorUserId, now);
        // Kasus sudah dilacak sebagai Modified. Anak baru didaftarkan lewat DbSet agar pasti
        // berstatus Added; menambahkannya lewat navigasi membuat EF mengira barisnya sudah ada.
        _dbContext.OprCaseProcedures.AddRange(entity.Procedures);
        _dbContext.OprStatusHistories.Add(NewHistory(entity.Id, entity.Status, entity.Status, "UpdateRequest",
            request.IdempotencyKey, fingerprint, actorUserId, now));

        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException)
        {
            _dbContext.ChangeTracker.Clear();
            var concurrent = await FindIdempotentCaseAsync("UpdateRequest", request.IdempotencyKey, cancellationToken);
            if (concurrent != null && concurrent.OprCaseId == id)
            {
                EnsureSameFingerprint(concurrent.Source, fingerprint);
                return (await GetDetailAsync(id, cancellationToken))!;
            }
            throw new OperatingRoomConflictException("OPR012", "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        }

        await _loggerService.AuditAsync(LogCategory, "OperatingRoomCase.Update", "Memperbarui permintaan kasus operasi.",
            new { entity.Id, entity.CaseNumber, ActorUserId = actorUserId, entity.Version });
        return (await GetDetailAsync(entity.Id, cancellationToken))!;
    }

    private async Task ValidateReferencesAsync(Guid patientId, Guid encounterId, Guid requesterDoctorId,
        Guid primarySurgeonId, IReadOnlyCollection<OprCaseProcedureRequest> procedures, Guid? currentCaseId,
        CancellationToken cancellationToken)
    {
        var encounterValid = await _dbContext.Set<TrxPatientEncounter>().AsNoTracking()
            .AnyAsync(x => x.Id == encounterId && x.PatientId == patientId && !x.IsDelete, cancellationToken);
        if (!encounterValid) throw new ArgumentException("Encounter tidak ditemukan atau tidak sesuai dengan pasien.");

        var doctorIds = new[] { requesterDoctorId, primarySurgeonId }.Distinct().ToList();
        var validDoctors = await _dbContext.Set<MstDoctor>().AsNoTracking()
            .CountAsync(x => doctorIds.Contains(x.Id) && x.IsActive && !x.IsDelete, cancellationToken);
        if (validDoctors != doctorIds.Count) throw new ArgumentException("Dokter pemohon atau dokter bedah utama tidak aktif/tidak ditemukan.");

        var procedureIds = procedures.Select(x => x.PatientProcedureId).Distinct().ToList();
        var validProcedures = await _dbContext.Set<TrxPatientProcedure>().AsNoTracking()
            .CountAsync(x => procedureIds.Contains(x.Id) && x.EncounterId == encounterId && x.PatientId == patientId &&
                x.IsSurgeryRelated && x.IsActive && !x.IsDelete && !x.IsCancel, cancellationToken);
        if (validProcedures != procedureIds.Count)
            throw new ArgumentException("Tindakan tidak ditemukan, tidak aktif, atau bukan tindakan operasi.");

        var duplicateExists = await _dbContext.OprCaseProcedures.AsNoTracking()
            .AnyAsync(x => procedureIds.Contains(x.PatientProcedureId) && !x.IsDelete &&
                (!currentCaseId.HasValue || x.OprCaseId != currentCaseId.Value) && x.OprCase != null && !x.OprCase.IsDelete &&
                x.OprCase.Status != OprCaseStatus.Completed && x.OprCase.Status != OprCaseStatus.Cancelled, cancellationToken);
        if (duplicateExists)
            throw new OperatingRoomConflictException("OPR002", "Tindakan sudah diproses pada kasus operasi lain.");
    }

    private static void ValidateRequest(IReadOnlyCollection<OprCaseProcedureRequest> procedures, string indication, string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(indication)) throw new ArgumentException("Indikasi operasi wajib diisi.");
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("Idempotency key wajib diisi.");
        if (procedures.Count == 0 || procedures.Count(x => x.IsPrimary) != 1)
            throw new ArgumentException("Pilih satu tindakan utama.");
        if (procedures.Any(x => x.PatientProcedureId == Guid.Empty) ||
            procedures.Select(x => x.PatientProcedureId).Distinct().Count() != procedures.Count)
            throw new ArgumentException("Daftar tindakan tidak valid atau memiliki data ganda.");
    }

    private static void AddProcedures(OprCase entity, IReadOnlyCollection<OprCaseProcedureRequest> procedures, Guid actorUserId, DateTime now)
    {
        var sequence = 1;
        foreach (var procedure in procedures.OrderByDescending(x => x.IsPrimary))
            entity.Procedures.Add(new OprCaseProcedure { OprCaseId = entity.Id,
                PatientProcedureId = procedure.PatientProcedureId,
                IsPrimary = procedure.IsPrimary, Sequence = sequence++, CreateDateTime = now, CreateBy = actorUserId });
    }

    private static OprStatusHistory NewHistory(Guid caseId, OprCaseStatus to, OprCaseStatus? from, string action,
        string idempotencyKey, string fingerprint, Guid actorUserId, DateTime now) => new()
    {
        OprCaseId = caseId, FromStatus = from, ToStatus = to, Action = action, ActorUserId = actorUserId,
        OccurredAt = now, Source = BuildSource(fingerprint), CorrelationId = idempotencyKey.Trim(),
        CreateDateTime = now, CreateBy = actorUserId
    };

    private Task<OprStatusHistory?> FindIdempotentCaseAsync(string action, string idempotencyKey, CancellationToken cancellationToken) =>
        _dbContext.OprStatusHistories.AsNoTracking().FirstOrDefaultAsync(x => x.Action == action &&
            x.CorrelationId == idempotencyKey.Trim() && !x.IsDelete, cancellationToken);

    private async Task<OprCase?> LoadCaseAsync(Guid id, bool tracking, CancellationToken cancellationToken)
    {
        var query = _dbContext.OprCases.Include(x => x.Patient).Include(x => x.RequesterDoctor)
            .Include(x => x.PrimarySurgeon).Include(x => x.Procedures).ThenInclude(x => x.PatientProcedure)
            .Include(x => x.StatusHistories).Where(x => x.Id == id && !x.IsDelete);
        if (!tracking) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private Guid GetCurrentUserId() => GetRequiredClaim(ClaimTypes.NameIdentifier, "user_id", "Identitas pengguna tidak valid.");
    private Guid GetCurrentDoctorId() => GetRequiredClaim("doctor_id", "DoctorId", "Akun pengguna tidak terhubung dengan dokter.");

    private Guid GetRequiredClaim(string primary, string secondary, string message)
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(primary) ??
            _httpContextAccessor.HttpContext?.User.FindFirstValue(secondary);
        if (!Guid.TryParse(value, out var id) || id == Guid.Empty) throw new OperatingRoomForbiddenException(message);
        return id;
    }

    private static void EnsureDoctorActor(Guid actorDoctorId, Guid requesterDoctorId)
    {
        if (actorDoctorId != requesterDoctorId)
            throw new OperatingRoomForbiddenException("Dokter pemohon harus sesuai dengan pengguna yang sedang login.");
    }

    private static void EnsureSameFingerprint(string source, string fingerprint)
    {
        if (!string.Equals(source, BuildSource(fingerprint), StringComparison.Ordinal))
            throw new OperatingRoomConflictException("OPR013", "Idempotency key digunakan dengan isi permintaan yang berbeda.");
    }

    private static string BuildFingerprint(CreateOprCaseRequest r) => Hash(string.Join('|', r.PatientId, r.EncounterId,
        r.RequesterDoctorId, r.PrimarySurgeonId, r.CaseType, r.Priority, r.Indication.Trim(), Normalize(r.Laterality),
        r.EstimatedMinutes, r.PreferredAt?.ToUniversalTime().Ticks, ProcedureFingerprint(r.Procedures)));
    private static string BuildFingerprint(UpdateOprCaseRequest r) => Hash(string.Join('|', r.RequesterDoctorId,
        r.PrimarySurgeonId, r.CaseType, r.Priority, r.Indication.Trim(), Normalize(r.Laterality), r.EstimatedMinutes,
        r.PreferredAt?.ToUniversalTime().Ticks, ProcedureFingerprint(r.Procedures)));
    private static string ProcedureFingerprint(IEnumerable<OprCaseProcedureRequest> values) => string.Join(',',
        values.OrderBy(x => x.PatientProcedureId).Select(x => $"{x.PatientProcedureId:N}:{x.IsPrimary}"));
    private static Guid CreateDeterministicId(string idempotencyKey) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"OperatingRoomCase:{idempotencyKey.Trim()}"))[..16]);

    private static OprCaseDetailResponse MapDetail(OprCase entity)
    {
        var primary = entity.Procedures.FirstOrDefault(x => x.IsPrimary && !x.IsDelete);
        return new OprCaseDetailResponse
        {
            Id = entity.Id, CaseNumber = entity.CaseNumber, PatientId = entity.PatientId,
            PatientName = entity.Patient?.FullName ?? string.Empty, EncounterId = entity.EncounterId,
            RequesterDoctorId = entity.RequesterDoctorId, RequesterDoctorName = entity.RequesterDoctor?.FullName ?? string.Empty,
            PrimarySurgeonId = entity.PrimarySurgeonId, PrimarySurgeonName = entity.PrimarySurgeon?.FullName ?? string.Empty,
            CaseType = entity.CaseType, Priority = entity.Priority, Status = entity.Status, Outcome = entity.Outcome,
            Indication = entity.Indication, Laterality = entity.Laterality, EstimatedMinutes = entity.EstimatedMinutes,
            RequestedAt = entity.RequestedAt, PreferredAt = entity.PreferredAt, Version = entity.Version,
            PrimaryProcedureName = primary?.PatientProcedure?.ProcedureNameSnapshot ?? string.Empty,
            Procedures = entity.Procedures.Where(x => !x.IsDelete).OrderBy(x => x.Sequence).Select(x => new OprCaseProcedureResponse
            {
                PatientProcedureId = x.PatientProcedureId, ProcedureCode = x.PatientProcedure?.ProcedureCodeSnapshot ?? string.Empty,
                ProcedureName = x.PatientProcedure?.ProcedureNameSnapshot ?? string.Empty, IsPrimary = x.IsPrimary, Sequence = x.Sequence
            }).ToList(),
            AvailableActions = AvailableActions(entity.Status)
        };
    }
}
