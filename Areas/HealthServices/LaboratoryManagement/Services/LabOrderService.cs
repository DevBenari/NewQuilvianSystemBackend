using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    public class LabOrderService
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoggerService _loggerService;

        public LabOrderService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _loggerService = loggerService;
        }

        public async Task<List<LabOrderListResponse>> GetListAsync(
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.LabOrders
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .OrderByDescending(x => x.CreateDateTime)
                .Select(x => new LabOrderListResponse
                {
                    Id = x.Id,
                    EncounterId = x.EncounterId,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : string.Empty,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : string.Empty,
                    IsCancel = x.IsCancel,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);
        }

        public async Task<LabOrderDetailResponse?> GetDetailAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _dbContext.LabOrders
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new LabOrderDetailResponse
                {
                    Id = x.Id,
                    EncounterId = x.EncounterId,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : string.Empty,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : string.Empty,
                    IsCancel = x.IsCancel,
                    CreateDateTime = x.CreateDateTime,
                    CancelDateTime = x.CancelDateTime,
                    CancelBy = x.CancelBy == Guid.Empty ? null : x.CancelBy
                })
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<LabOrderDetailResponse> CreateAsync(
            CreateLabOrderRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.EncounterId == Guid.Empty)
                throw new ArgumentException("EncounterId wajib diisi.");

            if (request.ProcedureId == Guid.Empty)
                throw new ArgumentException("ProcedureId wajib diisi.");

            var encounterExists = await _dbContext.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.EncounterId && !x.IsDelete, cancellationToken);

            if (!encounterExists)
                throw new KeyNotFoundException("Encounter tidak ditemukan.");

            var procedure = await _dbContext.Set<MstProcedure>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.ProcedureId &&
                    x.IsLaboratory &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (procedure == null)
                throw new ArgumentException("Procedure tidak ditemukan, tidak aktif, atau bukan procedure laboratorium.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var entity = new LabOrder
            {
                EncounterId = request.EncounterId,
                ProcedureId = request.ProcedureId,
                CreateDateTime = now,
                CreateBy = actorUserId
            };

            _dbContext.LabOrders.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabOrder.Create",
                "Membuat order laboratorium.",
                new { entity.Id, entity.EncounterId, entity.ProcedureId, ActorUserId = actorUserId });

            return MapDetailResponse(entity, procedure);
        }

        public async Task<LabOrderDetailResponse> CancelAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.LabOrders
                .Include(x => x.Procedure)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                throw new KeyNotFoundException("Order laboratorium tidak ditemukan.");

            if (entity.IsCancel)
                throw new InvalidOperationException("Order laboratorium sudah dibatalkan.");

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsCancel = true;
            entity.CancelDateTime = now;
            entity.CancelBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "LabOrder.Cancel",
                "Membatalkan order laboratorium.",
                new { entity.Id, entity.EncounterId, entity.ProcedureId, ActorUserId = actorUserId });

            return MapDetailResponse(entity, entity.Procedure);
        }

        private Guid GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var value = user?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        user?.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }

        private static LabOrderDetailResponse MapDetailResponse(LabOrder entity, MstProcedure? procedure)
        {
            return new LabOrderDetailResponse
            {
                Id = entity.Id,
                EncounterId = entity.EncounterId,
                ProcedureId = entity.ProcedureId,
                ProcedureCode = procedure?.ProcedureCode ?? string.Empty,
                ProcedureName = procedure?.ProcedureName ?? string.Empty,
                IsCancel = entity.IsCancel,
                CreateDateTime = entity.CreateDateTime,
                CancelDateTime = entity.CancelDateTime,
                CancelBy = entity.CancelBy == Guid.Empty ? null : entity.CancelBy
            };
        }
    }
}
