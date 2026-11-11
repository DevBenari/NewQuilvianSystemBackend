using QuilvianSystemBackend.Responses;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public class DisciplinaryIncidentService : IDisciplinaryIncidentService
    {
        private readonly IEmployeeRelationUnitOfWork _uow;
        private readonly NumberSeriesAllocator _numberAllocator;

        public DisciplinaryIncidentService(IEmployeeRelationUnitOfWork uow, NumberSeriesAllocator numberAllocator)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _numberAllocator = numberAllocator ?? throw new ArgumentNullException(nameof(numberAllocator));
        }

        public async Task<IncidentReportResponseDto> ReportIncidentAsync(CreateIncidentReportRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            // VR-EMP-01: Incident date tidak boleh di masa depan & deskripsi >= 50 karakter
            if (request.IncidentDate > DateTime.UtcNow.AddMinutes(5))
            {
                throw new InvalidOperationException("Tanggal insiden tidak boleh di masa depan sesuai aturan VR-EMP-01.");
            }

            if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Trim().Length < 50)
            {
                throw new InvalidOperationException("Deskripsi kronologi kejadian minimal 50 karakter sesuai aturan VR-EMP-01.");
            }

            // VR-EMP-02: Jika category ClinicalSafety, wajib ada nomor audit KMKP (INT-HR-86)
            if (string.Equals(request.Category, "ClinicalSafety", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(request.KmkpAuditRef))
                {
                    throw new InvalidOperationException("Insiden yang melibatkan keselamatan pasien wajib menyertakan nomor registrasi audit keselamatan pasien KMKP sesuai aturan VR-EMP-02.");
                }
            }

            // Nomor urut dari provider bersama, bukan angka acak (QBE-CODE-003). Bentuk: INC-202611-0001
            var incidentNumber = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                SequenceKey: EmployeeRelationNumberSeries.Keys.Incident,
                Prefix: EmployeeRelationNumberSeries.Prefixes.Incident,
                ResetPolicy: EmployeeRelationNumberSeries.ResetPolicies.Incident,
                SequenceDigits: EmployeeRelationNumberSeries.SequenceDigits,
                ActorUserId: await ResolveActorUserIdAsync(actorEmail, cancellationToken),
                Instant: DateTimeOffset.UtcNow), cancellationToken);

            var entity = new TrxDisciplinaryIncidentReport
            {
                IncidentReportId = Guid.NewGuid(),
                IncidentNumber = incidentNumber,
                EmployeeId = request.EmployeeId,
                IncidentDate = request.IncidentDate,
                IncidentLocation = request.Location.Trim(),
                Category = request.Category.Trim(),
                JustCultureClassification = request.JustCultureClassification.Trim(),
                Description = request.Description.Trim(),
                ReporterType = string.IsNullOrWhiteSpace(request.ReporterType) ? "Superior" : request.ReporterType.Trim(),
                ReporterEmployeeId = request.ReporterEmployeeId,
                KmkpAuditReferenceNumber = request.KmkpAuditRef?.Trim(),
                Status = "Reported",
                CreateDateTime = DateTime.UtcNow
            };

            await _uow.AddIncidentReportAsync(entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new IncidentReportResponseDto
            {
                IncidentReportId = entity.IncidentReportId,
                IncidentNumber = entity.IncidentNumber,
                EmployeeId = entity.EmployeeId,
                IncidentDate = entity.IncidentDate,
                Location = entity.IncidentLocation,
                Category = entity.Category,
                JustCultureClassification = entity.JustCultureClassification,
                Description = entity.Description,
                Status = entity.Status,
                KmkpAuditRef = entity.KmkpAuditReferenceNumber,
                CreatedAt = entity.CreateDateTime
            };
        }

        public async Task<PagedResult<IncidentReportResponseDto>> GetIncidentsAsync(Guid? employeeId, string? justCultureClassification, string? status, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _uow.IncidentReports.AsNoTracking();

            if (employeeId.HasValue && employeeId.Value != Guid.Empty)
            {
                query = query.Where(x => x.EmployeeId == employeeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(justCultureClassification))
            {
                query = query.Where(x => x.JustCultureClassification == justCultureClassification.Trim());
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(x => x.Status == status.Trim());
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var safePage = Math.Max(1, page);
            var safePageSize = Math.Clamp(pageSize, 1, 100);

            var items = await query
                .OrderByDescending(x => x.IncidentDate)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .Select(x => new IncidentReportResponseDto
                {
                    IncidentReportId = x.IncidentReportId,
                    IncidentNumber = x.IncidentNumber,
                    EmployeeId = x.EmployeeId,
                    IncidentDate = x.IncidentDate,
                    Location = x.IncidentLocation,
                    Category = x.Category,
                    JustCultureClassification = x.JustCultureClassification,
                    Description = x.Description,
                    Status = x.Status,
                    KmkpAuditRef = x.KmkpAuditReferenceNumber,
                    CreatedAt = x.CreateDateTime
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<IncidentReportResponseDto>
            {
                Items = items,
                TotalData = totalCount,
                PageNumber = safePage,
                PageSize = safePageSize,
                TotalPage = (int)Math.Ceiling((double)totalCount / Math.Max(1, safePageSize))
            };
        }

        public async Task<IncidentReportDetailDto?> GetIncidentByIdAsync(Guid incidentId, CancellationToken cancellationToken = default)
        {
            var entity = await _uow.IncidentReports
                .AsNoTracking()
                .Include(x => x.InvestigationRecords)
                .FirstOrDefaultAsync(x => x.IncidentReportId == incidentId, cancellationToken);

            if (entity == null) return null;

            return new IncidentReportDetailDto
            {
                IncidentReportId = entity.IncidentReportId,
                IncidentNumber = entity.IncidentNumber,
                EmployeeId = entity.EmployeeId,
                IncidentDate = entity.IncidentDate,
                Location = entity.IncidentLocation,
                Category = entity.Category,
                JustCultureClassification = entity.JustCultureClassification,
                Description = entity.Description,
                Status = entity.Status,
                KmkpAuditRef = entity.KmkpAuditReferenceNumber,
                CreatedAt = entity.CreateDateTime,
                ReporterType = entity.ReporterType,
                ReporterEmployeeId = entity.ReporterEmployeeId,
                Investigations = entity.InvestigationRecords.Select(i => new InvestigationRecordResponseDto
                {
                    InvestigationId = i.InvestigationId,
                    IncidentReportId = i.IncidentReportId,
                    BapNumber = i.BapNumber,
                    HearingDate = i.HearingDate,
                    InvestigatorNames = i.InvestigatorNames,
                    WitnessStatements = i.WitnessStatements,
                    EvidenceDocumentUrls = i.EvidenceDocumentUrls,
                    EmployeeDefense = i.EmployeeDefenseStatement,
                    Findings = i.InvestigationFindings,
                    RecommendedAction = i.RecommendedAction,
                    Status = i.Status,
                    CreatedAt = i.CreateDateTime
                }).ToList()
            };
        }

        private async Task<Guid> ResolveActorUserIdAsync(string actorEmail, CancellationToken cancellationToken)
        {
            var userId = await _uow.FindUserIdByEmailAsync(actorEmail, cancellationToken);
            return userId ?? EmployeeRelationNumberSeries.SystemFallbackUserId;
        }
    }
}
