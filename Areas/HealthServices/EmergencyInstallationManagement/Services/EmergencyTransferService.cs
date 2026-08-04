using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Services
{
    /// <summary>
    /// Validasi tujuan transfer, konsistensi unit asal/tujuan, dan transisi status transfer.
    /// </summary>
    public class EmergencyTransferService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly EmergencyDocumentNumberService _documentNumberService;

        public EmergencyTransferService(
            ApplicationDbContext dbContext,
            EmergencyDocumentNumberService documentNumberService)
        {
            _dbContext = dbContext;
            _documentNumberService = documentNumberService;
        }

        public async Task<string?> ValidateRequestAsync(
            CreateEmergencyTransferRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.EmergencyVisitId == Guid.Empty)
                return "EmergencyVisitId wajib diisi.";

            if (request.ToServiceUnitId == Guid.Empty)
                return "ToServiceUnitId wajib diisi.";

            if (!Enum.IsDefined(typeof(EmergencyTransferStatus), request.TransferStatus))
                return "Nilai TransferStatus tidak valid.";

            if (request.FromServiceUnitId.HasValue &&
                request.FromServiceUnitId.Value == request.ToServiceUnitId)
                return "Unit tujuan transfer harus berbeda dengan unit asal.";

            var visitExists = await _dbContext.Set<TrxEmergencyVisit>()
                .AsNoTracking()
                .AnyAsync(
                    x => x.Id == request.EmergencyVisitId &&
                         !x.IsDelete &&
                         x.VisitStatus != EmergencyVisitStatus.Disposed &&
                         x.VisitStatus != EmergencyVisitStatus.Cancelled,
                    cancellationToken);

            if (!visitExists)
                return "EmergencyVisitId tidak ditemukan atau kunjungan sudah ditutup.";

            if (request.FromServiceUnitId.HasValue &&
                request.FromServiceUnitId.Value != Guid.Empty &&
                !await _dbContext.Set<MstServiceUnit>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.FromServiceUnitId.Value && !x.IsDelete, cancellationToken))
                return "FromServiceUnitId tidak ditemukan.";

            if (!await _dbContext.Set<MstServiceUnit>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.ToServiceUnitId && !x.IsDelete, cancellationToken))
                return "ToServiceUnitId tidak ditemukan.";

            return null;
        }

        public Task<string?> ValidateRequestAsync(
            UpdateEmergencyTransferRequest request,
            CancellationToken cancellationToken = default)
            => ValidateRequestAsync((CreateEmergencyTransferRequest)request, cancellationToken);

        public bool CanTransition(EmergencyTransferStatus current, EmergencyTransferStatus target)
        {
            if (current == target)
                return true;

            return current switch
            {
                EmergencyTransferStatus.Requested => target is EmergencyTransferStatus.Accepted
                    or EmergencyTransferStatus.Rejected
                    or EmergencyTransferStatus.Cancelled,
                EmergencyTransferStatus.Accepted => target is EmergencyTransferStatus.InTransit
                    or EmergencyTransferStatus.Rejected
                    or EmergencyTransferStatus.Cancelled,
                EmergencyTransferStatus.InTransit => target is EmergencyTransferStatus.Completed
                    or EmergencyTransferStatus.Cancelled,
                _ => false
            };
        }

        public string GenerateNumber(DateTime now)
            => _documentNumberService.Generate("TRF", now);
    }
}
