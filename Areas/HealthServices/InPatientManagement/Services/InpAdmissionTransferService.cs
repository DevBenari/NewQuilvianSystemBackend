using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

/// <summary>Satu transaksi sumber, kunjungan, penjamin, episode, bed dan deposit.</summary>
public partial class InpAdmissionTransferService(
    ApplicationDbContext dbContext, InpEpisodeService episodeService,
    InpBedOccupancyService bedOccupancyService, LoggerService loggerService,
    InpBedMutationCoordinator bedCoordinator, InpBedAccessService bedAccess,
    EncounterPaymentSourceService paymentSources, BillingDepositService deposits,
    BillingNumberSeriesService? billingNumberSeries = null)
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly InpBedMutationCoordinator _bedCoordinator = bedCoordinator;
    private readonly InpBedAccessService _bedAccess = bedAccess;

    public async Task<PagedResult<InpatientAdmissionTransferItemResponse>> GetPagedTransfersAsync(
        AdmissionTransferQuery query, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var itemsQuery = TransferQuery(query.SourceType);
        var status = (query.Status ?? EmergencyDispositionStatus.Confirmed).ToString();
        itemsQuery = itemsQuery.Where(x => x.DispositionStatus == status);
        if (query.OverdueOnly == true)
            itemsQuery = itemsQuery.Where(x => x.DecidedAt < now.AddMinutes(-60));
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            itemsQuery = itemsQuery.Where(x => x.PatientName.ToLower().Contains(term)
                || x.MedicalRecordNumber.ToLower().Contains(term)
                || (x.SourceEncounterNumber != null && x.SourceEncounterNumber.ToLower().Contains(term))
                || x.EmergencyVisitNumber.ToLower().Contains(term));
        }
        var count = await itemsQuery.CountAsync(cancellationToken);
        var items = await itemsQuery.OrderBy(x => x.DecidedAt).ThenBy(x => x.TransferId)
            .Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize)
            .ToListAsync(cancellationToken);
        foreach (var item in items) ApplyWaiting(item, now);
        return new PagedResult<InpatientAdmissionTransferItemResponse>
        {
            PageNumber = query.PageNumber, PageSize = query.PageSize,
            TotalData = count, TotalPage = (int)Math.Ceiling(count / (double)query.PageSize), Items = items
        };
    }

    public async Task<InpatientAdmissionTransferItemResponse?> GetTransferByIdAsync(
        Guid transferId, CancellationToken cancellationToken = default, string? sourceType = null)
    {
        var item = await TransferQuery(sourceType).FirstOrDefaultAsync(x => x.TransferId == transferId, cancellationToken);
        if (item != null) ApplyWaiting(item, DateTime.UtcNow);
        return item;
    }

    private static void ApplyWaiting(InpatientAdmissionTransferItemResponse item, DateTime now)
    {
        item.WaitingMinutes = (int)Math.Max(0, (now - item.DecidedAt).TotalMinutes);
        item.IsOverdue = item.DecidedAt < now.AddMinutes(-60);
    }

    private async Task<InpEpisodeOperationResult> ExecuteTransferAdmissionCoreAsync(
        OpenAdmissionFromTransferRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (request.SourceType is not ("Emergency" or "Outpatient"))
            throw new InpBedOperationException(400, "INVALID_SOURCE_TYPE", "Jenis sumber transfer tidak dikenali.");
        if (!Enum.IsDefined(request.PaymentType) || (request.DepositAmount ?? 0) < 0
            || request.DepositAmount > 9999999999999999.99m
            || (request.DepositAmount > 0 && request.PaymentType != EncounterPaymentType.Cash))
            throw new InpBedOperationException(422, "INVALID_PAYMENT", "Jenis pembayaran atau nominal deposit tidak sesuai.");
        if (!string.IsNullOrWhiteSpace(request.PaymentScheme)
            && !string.Equals(request.PaymentScheme, request.PaymentType.ToString(), StringComparison.OrdinalIgnoreCase))
            throw new InpBedOperationException(422, "INVALID_PAYMENT_SCHEME", "PaymentScheme harus sesuai PaymentType dan identitas penjamin.");

        if (request.SourceType == "Emergency")
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM public.\"EmgDisposition\" WHERE \"Id\" = {request.TransferDispositionId} FOR UPDATE", cancellationToken);
        else
            await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM public.\"CliDoctorCertificate\" WHERE \"Id\" = {request.TransferDispositionId} FOR UPDATE", cancellationToken);

        var source = await GetTransferByIdAsync(request.TransferDispositionId, cancellationToken, request.SourceType);
        if (source == null) return InpEpisodeOperationResult.NotFound("Permintaan rawat inap yang sah tidak ditemukan.");
        if (source.DispositionStatus != "Confirmed" || source.ResultEpisodeId.HasValue)
            throw new InpBedOperationException(409, "TRANSFER_ALREADY_PROCESSED", "Permintaan sudah diproses atau dibatalkan. Muat ulang sumber.");
        if (source.PatientId != request.PatientId || source.SourceEncounterId == null)
            throw new InpBedOperationException(422, "TRANSFER_PATIENT_MISMATCH", "Pasien atau kunjungan asal tidak sesuai permintaan.");

        await _dbContext.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM public.\"RegPatientEncounter\" WHERE \"Id\" = {source.SourceEncounterId.Value} FOR UPDATE", cancellationToken);
        var origin = await _dbContext.RegPatientEncounters.AsNoTracking().FirstOrDefaultAsync(
            x => x.Id == source.SourceEncounterId && x.PatientId == request.PatientId && !x.IsDelete && !x.IsCancel
                && x.EncounterStatus != EncounterStatus.Cancelled && x.EncounterStatus != EncounterStatus.NoShow,
            cancellationToken);
        if (origin == null)
            throw new InpBedOperationException(422, "INVALID_SOURCE_ENCOUNTER", "Kunjungan asal tidak aktif atau dibatalkan.");
        if (await _dbContext.RegPatientEncounters.IgnoreQueryFilters().AnyAsync(
            x => x.AdmissionTransferId == request.TransferDispositionId && x.AdmissionTransferSourceType == request.SourceType,
            cancellationToken))
            throw new InpBedOperationException(409, "TRANSFER_ALREADY_PROCESSED", "Permintaan ini sudah memiliki hasil admisi.");

        if (!await _dbContext.Set<QuilvianSystemBackend.Areas.HealthServices.MasterData.Models.MstBed>()
            .AnyAsync(x => x.Id == request.BedId && x.Room != null
                && x.Room.ServiceUnitId == request.ServiceUnitId && x.Room.PatientClassId == request.PatientClassId, cancellationToken))
            throw new InpBedOperationException(422, "BED_CONTEXT_MISMATCH", "Unit dan kelas admisi harus sesuai tempat tidur yang dipilih.");

        var result = await episodeService.OpenAdmissionAsync(new OpenAdmissionRequest
        {
            PatientId = request.PatientId, ServiceUnitId = request.ServiceUnitId,
            PatientClassId = request.PatientClassId, DoctorId = request.DoctorId, Notes = request.Notes
        }, actorUserId, cancellationToken);
        if (result.Status != InpEpisodeOperationStatus.Success || result.Episode == null) return result;
        var episode = result.Episode;

        try
        {
            var payer = await paymentSources.InitializeAdmissionAsync(new EncounterPaymentSourceSwitchCommand(
                episode.EncounterId, request.PaymentType, request.PaymentMethodId, request.PatientInsuranceId,
                request.PatientCompanyGuarantorId, DateTime.UtcNow, "Admisi transfer", actorUserId),
                source.SourceEncounterId.Value, request.TransferDispositionId, request.SourceType, cancellationToken);
            if (payer is { Success: false })
                throw new InpBedOperationException(payer.StatusCode, payer.ErrorCode!, payer.ErrorMessage!);
        }
        catch (ArgumentException ex) { throw new InpBedOperationException(422, "INVALID_PAYMENT", ex.Message); }

        if (request.RequiresIsolation)
        {
            var isolation = await episodeService.SetIsolationRequirementAsync(episode.Id,
                new SetIsolationRequirementRequest { RequiresIsolation = true, IsolationNote = request.IsolationNote },
                actorUserId, null, cancellationToken);
            if (isolation.Status != InpEpisodeOperationStatus.Success) return isolation;
        }
        var bedResult = await bedOccupancyService.ReserveBedCoreAsync(new ReserveBedRequest
        {
            EpisodeId = episode.Id, BedId = request.BedId, ExpectedBedVersion = request.ExpectedBedVersion
        }, actorUserId, cancellationToken);
        if (bedResult.Status != InpEpisodeOperationStatus.Success)
            throw new InpBedOperationException(bedResult.Status == InpEpisodeOperationStatus.Conflict ? 409 : 422,
                "BED_NOT_READY", bedResult.Message);

        if (request.DepositAmount > 0)
        {
            if (request.DepositPaymentMethodId.HasValue)
            {
                // Deposit Dibayar Langsung Saat Admisi Transfer (Jalur penerimaan uang tunai/kartu)
                try
                {
                    await deposits.TopUpAsync(episode.EncounterId, new DepositTopUpRequest
                    {
                        Amount = request.DepositAmount.Value, PaymentMethodId = request.DepositPaymentMethodId.Value,
                        Reason = "Deposit admisi transfer", CorrelationId = episode.Id, CausationId = request.TransferDispositionId
                    }, episode.Id, actorUserId, cancellationToken, deferAudit: true);
                }
                catch (BillingDepositValidationException ex) { throw new InpBedOperationException(422, "DEPOSIT_REJECTED", ex.Message); }
                catch (BillingDepositConflictException ex) { throw new InpBedOperationException(409, "DEPOSIT_CONFLICT", ex.Message); }
                catch (KeyNotFoundException ex) { throw new InpBedOperationException(422, "DEPOSIT_REFERENCE_INVALID", ex.Message); }
            }
            else
            {
                // Deposit Diminta (Permintaan Pembayaran - Belum Bayar) -> Terbitkan BilDepositPaymentOrder (UNPAID) untuk Kasir
                string orderNumber;
                if (billingNumberSeries != null)
                {
                    orderNumber = await billingNumberSeries.AllocateDepositOrderNumberAsync(actorUserId, DateTime.UtcNow, cancellationToken);
                }
                else
                {
                    orderNumber = $"DPO-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}";
                }

                var depositOrder = new BilDepositPaymentOrder
                {
                    Id = Guid.NewGuid(),
                    OrderNumber = orderNumber,
                    EpisodeId = episode.Id,
                    EncounterId = episode.EncounterId,
                    PatientId = episode.PatientId,
                    OrderType = DepositOrderTypes.InitialDeposit,
                    SourceType = DepositOrderSourceTypes.InpatientAdmission,
                    SourceReferenceId = request.TransferDispositionId,
                    RequestedAmount = request.DepositAmount.Value,
                    PaidAmount = 0m,
                    OutstandingAmount = request.DepositAmount.Value,
                    Status = DepositOrderStatuses.Unpaid,
                    DueDate = DateTime.UtcNow.Date.AddDays(3),
                    Remarks = "Order deposit awal admisi transfer",
                    CorrelationId = episode.Id,
                    CausationId = request.TransferDispositionId,
                    CreateBy = actorUserId,
                    CreateDateTime = DateTime.UtcNow
                };

                _dbContext.Set<BilDepositPaymentOrder>().Add(depositOrder);
            }
        }
        if (request.SourceType == "Emergency")
        {
            var disposition = await _dbContext.Set<EmgDisposition>().SingleAsync(x => x.Id == request.TransferDispositionId, cancellationToken);
            disposition.DispositionStatus = EmergencyDispositionStatus.Executed;
            disposition.ExecutedAt = DateTime.UtcNow;
            disposition.UpdateBy = actorUserId;
            disposition.UpdateDateTime = DateTime.UtcNow;
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        return InpEpisodeOperationResult.Success(episode, "Admisi dan reservasi transfer tersimpan. Lanjutkan persetujuan pasien.");
    }
}
