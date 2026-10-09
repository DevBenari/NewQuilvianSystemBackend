using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BloodBankManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

/// <summary>Memeriksa konteks bangsal; mutation dan transaksi tetap milik Gizi/Bank Darah.</summary>
public sealed class InpAncillaryOrderAdapter
{
    private readonly ApplicationDbContext _dbContext;
    private readonly InpatientClinicalContextService _clinicalContext;
    private readonly InsuranceCoverageService _coverage;
    private readonly AccessPermissionService _permissions;
    private readonly NutritionOrderService _nutrition;
    private readonly BbkBloodOrderService _blood;
    private readonly ILogger<InpAncillaryOrderAdapter> _logger;

    public InpAncillaryOrderAdapter(ApplicationDbContext dbContext,
        InpatientClinicalContextService clinicalContext, InsuranceCoverageService coverage,
        AccessPermissionService permissions, NutritionOrderService nutrition,
        BbkBloodOrderService blood, ILogger<InpAncillaryOrderAdapter> logger)
    {
        _dbContext = dbContext; _clinicalContext = clinicalContext; _coverage = coverage;
        _permissions = permissions; _nutrition = nutrition; _blood = blood; _logger = logger;
    }

    public async Task<List<CoverageStatusItem>> GetCoverageStatusAsync(Guid episodeId,
        AncillaryCoverageQuery request, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        if (request.ItemIds == null || request.ItemIds.Count == 0 || request.ItemIds.Any(x => x == Guid.Empty))
            throw new InpAncillaryOrderException(400, "Pilih sekurang-kurangnya satu pemeriksaan.", "VAL-RWF-66");
        if (!Enum.IsDefined(request.ItemType))
            throw new InpAncillaryOrderException(400, "Jenis pemeriksaan tidak valid.");
        var episode = await LoadEpisodeAsync(episodeId, false, cancellationToken);
        var canCreate = await _permissions.HasAccessAsync(user, PermissionResource(request.ItemType), "Create");
        var items = new List<CoverageStatusItem>();
        foreach (var itemId in request.ItemIds)
        {
            var item = new CoverageStatusItem
            {
                ItemId = itemId, Label = "Tarif belum tersedia",
                PriceStatus = canCreate ? "NOT_ESTIMABLE" : "NOT_PERMITTED"
            };
            if (request.ItemType is AncillaryOrderItemType.Nutrition or AncillaryOrderItemType.Blood)
            {
                items.Add(item);
                continue;
            }
            try
            {
                var result = await _coverage.ResolveProcedureAsync(episode.EncounterId, itemId,
                    1, DateTime.UtcNow, cancellationToken);
                if (result.IsValid && result.TariffId.HasValue)
                {
                    item.IsCovered = result.IsCovered;
                    var baseLabel = result.IsCovered ? "Ditanggung" : "Tidak Di-cover";
                    if (result.IsNeedGuarantorApproval || (!string.IsNullOrWhiteSpace(result.CoverageNote) && result.CoverageNote.Contains("Perlu persetujuan")))
                    {
                        baseLabel += " · Perlu persetujuan penjamin";
                    }
                    item.Label = baseLabel;
                    if (canCreate)
                    {
                        item.PriceStatus = "AVAILABLE";
                        item.EstimatedUnitPrice = result.UnitPrice;
                        item.PriceLabel = "perkiraan — tagihan final di kasir";
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Resolusi perkiraan tarif gagal untuk item {ItemId}.", itemId);
            }
            items.Add(item);
        }
        return items;
    }

    public async Task<GziOrderDetailResponse> CreateNutritionConsultationAsync(Guid episodeId,
        CreateInpatientNutritionConsultationRequest request, ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        var context = await ResolveOrderContextAsync(episodeId, request.RequesterDoctorId, user, cancellationToken);
        try
        {
            return await _nutrition.CreateAsync(new CreateGzOrderRequest
            {
                PatientId = context.Episode.PatientId, EncounterId = context.Episode.EncounterId,
                RequesterDoctorId = context.DoctorId, Priority = request.Priority,
                ReasonForReferral = request.ReasonForReferral, IdempotencyKey = request.IdempotencyKey
            }, cancellationToken, context.IsActorDoctor ? GziInstructionVerificationStatus.NotRequired
                : GziInstructionVerificationStatus.Pending);
        }
        catch (NutritionForbiddenException ex) { throw new InpAncillaryOrderException(403, ex.Message); }
        catch (NutritionConflictException ex) { throw new InpAncillaryOrderException(409, ex.Message, ex.Code); }
        catch (NutritionUnprocessableException ex) { throw new InpAncillaryOrderException(422, ex.Message, ex.Code); }
    }

    public async Task<BloodOrderDetailDto> CreateBloodOrderAsync(Guid episodeId,
        CreateInpatientBloodOrderRequest request, ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        var context = await ResolveOrderContextAsync(episodeId, request.RequestingDoctorId, user, cancellationToken);
        var status = context.IsActorDoctor ? BbkInstructionVerificationStatus.NotRequired
            : BbkInstructionVerificationStatus.Pending;
        var result = await _blood.CreateAsync(new CreateBloodOrderRequest
        {
            PatientId = context.Episode.PatientId, EncounterId = context.Episode.EncounterId,
            ServiceUnitId = context.Episode.ServiceUnitId, RequestingDoctorId = context.DoctorId,
            RequestedBloodGroup = request.RequestedBloodGroup, Lines = request.Lines,
            ClinicalNote = request.ClinicalNote, IdempotencyKey = request.IdempotencyKey
        }, context.ActorUserId, cancellationToken, status);
        return await BloodDetailAsync(result, context.ActorUserId, cancellationToken);
    }

    public async Task<BloodOrderDetailDto> ConfirmDuplicateBloodOrderAsync(Guid episodeId,
        ConfirmInpatientDuplicateBloodOrderRequest request, ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        var context = await ResolveOrderContextAsync(episodeId, request.RequestingDoctorId, user, cancellationToken);
        var result = await _blood.ConfirmDuplicateAsync(new ConfirmDuplicateOrderRequest
        {
            PatientId = context.Episode.PatientId, EncounterId = context.Episode.EncounterId,
            ServiceUnitId = context.Episode.ServiceUnitId, RequestingDoctorId = context.DoctorId,
            RequestedBloodGroup = request.RequestedBloodGroup, Lines = request.Lines,
            ClinicalNote = request.ClinicalNote, IdempotencyKey = request.IdempotencyKey,
            DuplicateOverrideReason = request.DuplicateOverrideReason, IsManual = false
        }, context.ActorUserId, cancellationToken,
            context.IsActorDoctor ? BbkInstructionVerificationStatus.NotRequired : BbkInstructionVerificationStatus.Pending);
        return await BloodDetailAsync(result, context.ActorUserId, cancellationToken);
    }

    private async Task<BloodOrderDetailDto> BloodDetailAsync(BloodOrderResult result, Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (result.Outcome != BloodOrderOutcome.Success)
        {
            var status = result.Outcome switch
            {
                BloodOrderOutcome.NotFound => 404,
                BloodOrderOutcome.UnitNotAuthorized or BloodOrderOutcome.Forbidden => 403,
                BloodOrderOutcome.VersionConflict => 409,
                BloodOrderOutcome.DuplicateOrder or BloodOrderOutcome.NotAllowedByState => 422,
                _ => 400
            };
            throw new InpAncillaryOrderException(status, result.Message,
                result.Outcome == BloodOrderOutcome.DuplicateOrder ? "VAL-BD-001" : null,
                result.Outcome == BloodOrderOutcome.DuplicateOrder ? result.DuplicateComponentIds : null);
        }
        return await _blood.GetDetailAsync(result.Entity!.Id, actorUserId, cancellationToken)
            ?? BbkBloodOrderService.ToDetail(result.Entity!);
    }

    private async Task<(InpEpisode Episode, Guid DoctorId, Guid ActorUserId, bool IsActorDoctor)>
        ResolveOrderContextAsync(Guid episodeId, Guid? selectedDoctorId, ClaimsPrincipal user,
            CancellationToken cancellationToken)
    {
        var actorText = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("user_id");
        if (!Guid.TryParse(actorText, out var actorUserId) || actorUserId == Guid.Empty)
            throw new InpAncillaryOrderException(403, "Akun pengguna tidak dikenali.");
        var episode = await LoadEpisodeAsync(episodeId, true, cancellationToken);
        var actorDoctor = await _clinicalContext.ResolveActorDoctorIdAsync(user, actorUserId, cancellationToken);
        var doctorId = actorDoctor ?? selectedDoctorId;
        if (!doctorId.HasValue || doctorId == Guid.Empty)
            throw new InpAncillaryOrderException(400, "Dokter pemberi instruksi wajib dipilih.", "VAL-RWF-60");
        bool assigned;
        try
        {
            assigned = await _clinicalContext.IsDoctorAssignedAsync(episodeId, doctorId.Value,
                DateTime.UtcNow, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Konteks penugasan dokter episode {EpisodeId} tidak dapat dibaca.", episodeId);
            throw new InpAncillaryOrderException(503,
                "Penugasan dokter belum dapat diverifikasi. Ulangi setelah layanan tersedia.", "INT-RWF-16");
        }
        if (!assigned)
            throw new InpAncillaryOrderException(403,
                "Dokter yang dipilih tidak sedang menangani pasien ini.", "VAL-RWF-61");
        return (episode, doctorId.Value, actorUserId, actorDoctor.HasValue);
    }

    private async Task<InpEpisode> LoadEpisodeAsync(Guid id, bool requireOpen, CancellationToken cancellationToken)
    {
        var episode = await _dbContext.Set<InpEpisode>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete && !x.IsCancel, cancellationToken)
            ?? throw new InpAncillaryOrderException(404, "Episode rawat inap tidak ditemukan.");
        if (requireOpen && episode.EpisodeStatus is not (InpEpisodeStatus.Admitted or InpEpisodeStatus.DischargePending))
            throw new InpAncillaryOrderException(409, "Episode rawat inap ini tidak dapat menerima pesanan baru.");
        return episode;
    }

    private static void EnsureIdempotencyKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 100)
            throw new InpAncillaryOrderException(400, "IdempotencyKey wajib diisi, maksimal 100 karakter.");
    }

    private static string PermissionResource(AncillaryOrderItemType type) => type switch
    {
        AncillaryOrderItemType.Laboratory => "LabOrder", AncillaryOrderItemType.Radiology => "RadOrder",
        AncillaryOrderItemType.Procedure => "PatientProcedure", AncillaryOrderItemType.Nutrition => "NutritionOrder",
        AncillaryOrderItemType.Blood => "BloodOrder", _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}

public sealed class InpAncillaryOrderException : Exception
{
    public int StatusCode { get; }
    public object? Errors { get; }
    public InpAncillaryOrderException(int statusCode, string message, string? code = null,
        IReadOnlyList<Guid>? duplicateComponentIds = null) : base(message)
    {
        StatusCode = statusCode;
        Errors = code == null ? null : duplicateComponentIds == null
            ? new { code } : (object)new { code, duplicateComponentIds };
    }
}
