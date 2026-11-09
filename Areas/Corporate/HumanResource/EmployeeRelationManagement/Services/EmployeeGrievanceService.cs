using QuilvianSystemBackend.Responses;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public class EmployeeGrievanceService : IEmployeeGrievanceService
    {
        private readonly IEmployeeRelationUnitOfWork _uow;

        public EmployeeGrievanceService(IEmployeeRelationUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<GrievanceTicketResponseDto> SubmitGrievanceAsync(SubmitGrievanceRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // VR-EMP-14: SLA penyelesaian target resolusi maksimal 14 hari kerja
            var targetResolutionDate = DateTime.UtcNow.AddDays(14);
            var ticketNumber = $"GRV-{DateTime.UtcNow:yyyyMM}-{Random.Shared.Next(1000, 9999)}";

            var entity = new TrxEmployeeGrievance
            {
                Id = Guid.NewGuid(),
                GrievanceNumber = ticketNumber,
                ComplainantEmployeeId = request.ComplainantEmployeeId,
                ComplainantWorkforceProfileId = request.ComplainantEmployeeId ?? Guid.Empty,
                GrievanceType = request.Category.Trim(),
                Category = request.Category.Trim(),
                Subject = $"Pengaduan Kategori {request.Category}",
                GrievanceSummary = request.ReportDescription.Trim(),
                TargetUnit = request.TargetUnit?.Trim(),
                SeverityLevel = string.IsNullOrWhiteSpace(request.SeverityLevel) ? "Medium" : request.SeverityLevel.Trim(),
                TargetResolutionDate = targetResolutionDate,
                GrievanceDate = DateTime.UtcNow,
                GrievanceStatus = "Submitted",
                IsWhistleblowerCase = false,
                IsAnonymous = false,
                CreateDateTime = DateTime.UtcNow
            };

            await _uow.AddGrievanceAsync(entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return MapToDto(entity);
        }

        public async Task<PagedResult<GrievanceTicketSummaryDto>> GetMyTicketsAsync(Guid employeeId, string? status, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _uow.Grievances
                .AsNoTracking()
                .Where(x => x.ComplainantEmployeeId == employeeId && !x.IsWhistleblowerCase);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.GrievanceStatus == status.Trim());
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var safePage = Math.Max(1, page);
            var safePageSize = Math.Clamp(pageSize, 1, 100);

            var items = await query
                .OrderByDescending(x => x.GrievanceDate)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(x => new GrievanceTicketSummaryDto
                {
                    GrievanceId = x.Id,
                    TicketNumber = x.GrievanceNumber,
                    Category = x.Category ?? x.GrievanceType,
                    SeverityLevel = x.SeverityLevel,
                    TargetResolutionDate = x.TargetResolutionDate ?? x.GrievanceDate.AddDays(14),
                    Status = x.GrievanceStatus,
                    CreatedAt = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<GrievanceTicketSummaryDto>
            {
                Items = items,
                TotalData = totalCount,
                PageNumber = safePage,
                PageSize = safePageSize,
                TotalPage = (int)Math.Ceiling((double)totalCount / Math.Max(1, safePageSize))
            };
        }

        public async Task<GrievanceTicketResponseDto> RecordMediationResolutionAsync(Guid grievanceId, RecordMediationResolutionRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var grievance = await _uow.Grievances.FirstOrDefaultAsync(x => x.Id == grievanceId, cancellationToken);
            if (grievance == null)
            {
                throw new KeyNotFoundException($"Tiket pengaduan dengan ID {grievanceId} tidak ditemukan.");
            }

            grievance.ResolutionNotes = request.ResolutionNotes.Trim();
            grievance.GrievanceStatus = request.IsResolved ? "Resolved" : "MediationScheduled";
            grievance.ResolvedAt = request.IsResolved ? DateTime.UtcNow : null;
            grievance.UpdateDateTime = DateTime.UtcNow;

            await _uow.SaveChangesAsync(cancellationToken);
            return MapToDto(grievance);
        }

        private static GrievanceTicketResponseDto MapToDto(TrxEmployeeGrievance entity)
        {
            return new GrievanceTicketResponseDto
            {
                GrievanceId = entity.Id,
                TicketNumber = entity.GrievanceNumber,
                ComplainantEmployeeId = entity.ComplainantEmployeeId,
                IsWhistleblowerCase = entity.IsWhistleblowerCase,
                IsAnonymous = entity.IsAnonymous,
                Category = entity.Category ?? entity.GrievanceType,
                TargetUnit = entity.TargetUnit,
                SeverityLevel = entity.SeverityLevel,
                TargetResolutionDate = entity.TargetResolutionDate ?? entity.GrievanceDate.AddDays(14),
                Status = entity.GrievanceStatus,
                ResolutionNotes = entity.ResolutionNotes,
                CreatedAt = entity.CreateDateTime
            };
        }
    }
}
