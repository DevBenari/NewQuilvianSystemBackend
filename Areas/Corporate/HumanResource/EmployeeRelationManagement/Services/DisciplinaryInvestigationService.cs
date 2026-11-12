using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Services
{
    public class DisciplinaryInvestigationService : IDisciplinaryInvestigationService
    {
        private readonly IEmployeeRelationUnitOfWork _uow;
        private readonly NumberSeriesAllocator _numberAllocator;

        public DisciplinaryInvestigationService(IEmployeeRelationUnitOfWork uow, NumberSeriesAllocator numberAllocator)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _numberAllocator = numberAllocator ?? throw new ArgumentNullException(nameof(numberAllocator));
        }

        public async Task<InvestigationRecordResponseDto> InitiateInvestigationAsync(InitiateInvestigationRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var incident = await _uow.IncidentReports.FirstOrDefaultAsync(x => x.IncidentReportId == request.IncidentReportId, cancellationToken);
            if (incident == null)
            {
                throw new KeyNotFoundException($"Laporan insiden dengan ID {request.IncidentReportId} tidak ditemukan.");
            }

            // VR-EMP-04: Tim pemeriksa BAP minimal 2 orang dan independen
            var investigators = request.InvestigatorNames.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (investigators.Length < 2)
            {
                throw new InvalidOperationException("Tim pemeriksa BAP minimal terdiri dari 2 orang independen tanpa konflik kepentingan sesuai aturan VR-EMP-04.");
            }

            // Nomor urut tahunan dari provider bersama, bukan angka acak (QBE-CODE-003). Bentuk: BAP-HRD-2026-0001
            var bapNumber = await _numberAllocator.AllocateAsync(new NumberAllocationRequest(
                SequenceKey: EmployeeRelationNumberSeries.Keys.InvestigationBap,
                Prefix: EmployeeRelationNumberSeries.Prefixes.InvestigationBap,
                ResetPolicy: EmployeeRelationNumberSeries.ResetPolicies.InvestigationBap,
                SequenceDigits: EmployeeRelationNumberSeries.SequenceDigits,
                ActorUserId: await ResolveActorUserIdAsync(actorEmail, cancellationToken),
                Instant: DateTimeOffset.UtcNow), cancellationToken);

            var entity = new HrdDisciplinaryInvestigationRecord
            {
                InvestigationId = Guid.NewGuid(),
                IncidentReportId = request.IncidentReportId,
                BapNumber = bapNumber,
                HearingDate = request.HearingDate,
                InvestigatorNames = request.InvestigatorNames.Trim(),
                WitnessStatements = request.WitnessStatements?.Trim(),
                EvidenceDocumentUrls = request.EvidenceDocumentUrls?.Trim(),
                InvestigationFindings = "Dalam proses pemeriksaan.",
                RecommendedAction = "PendingFindings",
                Status = "Draft",
                CreateDateTime = DateTime.UtcNow
            };

            incident.Status = "Investigating";

            await _uow.AddInvestigationRecordAsync(entity, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return MapToDto(entity);
        }

        public async Task<InvestigationRecordResponseDto> RecordFindingsAsync(Guid investigationId, RecordBapFindingsRequestDto request, string actorEmail, CancellationToken cancellationToken = default)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var record = await _uow.InvestigationRecords
                .Include(x => x.IncidentReport)
                .FirstOrDefaultAsync(x => x.InvestigationId == investigationId, cancellationToken);

            if (record == null)
            {
                throw new KeyNotFoundException($"Berkas BAP dengan ID {investigationId} tidak ditemukan.");
            }

            // VR-EMP-05: BAP tidak dapat disahkan jika employee_defense_statement kosong
            if (string.IsNullOrWhiteSpace(request.EmployeeDefense) || request.EmployeeDefense.Trim().Length < 10)
            {
                throw new InvalidOperationException("Berita Acara Pemeriksaan wajib memuat lembar pembelaan tertulis pegawai terperiksa sesuai aturan VR-EMP-05.");
            }

            record.EmployeeDefenseStatement = request.EmployeeDefense.Trim();
            record.InvestigationFindings = request.Findings.Trim();
            record.RecommendedAction = request.RecommendedAction.Trim();
            record.Status = "ApprovedByHrDirector";
            record.UpdateDateTime = DateTime.UtcNow;

            if (record.IncidentReport != null)
            {
                if (request.RecommendedAction.Contains("SP") || request.RecommendedAction == "Suspension" || request.RecommendedAction == "Termination")
                {
                    record.IncidentReport.Status = "SanctionRecommended";
                }
                else
                {
                    record.IncidentReport.Status = "ResolvedNoSanction";
                }
                record.IncidentReport.UpdateDateTime = DateTime.UtcNow;
            }

            await _uow.SaveChangesAsync(cancellationToken);
            return MapToDto(record);
        }

        public async Task<InvestigationRecordResponseDto?> GetInvestigationByIdAsync(Guid investigationId, CancellationToken cancellationToken = default)
        {
            var record = await _uow.InvestigationRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.InvestigationId == investigationId, cancellationToken);

            return record == null ? null : MapToDto(record);
        }

        private static InvestigationRecordResponseDto MapToDto(HrdDisciplinaryInvestigationRecord entity)
        {
            return new InvestigationRecordResponseDto
            {
                InvestigationId = entity.InvestigationId,
                IncidentReportId = entity.IncidentReportId,
                BapNumber = entity.BapNumber,
                HearingDate = entity.HearingDate,
                InvestigatorNames = entity.InvestigatorNames,
                WitnessStatements = entity.WitnessStatements,
                EvidenceDocumentUrls = entity.EvidenceDocumentUrls,
                EmployeeDefense = entity.EmployeeDefenseStatement,
                Findings = entity.InvestigationFindings,
                RecommendedAction = entity.RecommendedAction,
                Status = entity.Status,
                CreatedAt = entity.CreateDateTime
            };
        }

        private async Task<Guid> ResolveActorUserIdAsync(string actorEmail, CancellationToken cancellationToken)
        {
            var userId = await _uow.FindUserIdByEmailAsync(actorEmail, cancellationToken);
            return userId ?? EmployeeRelationNumberSeries.SystemFallbackUserId;
        }
    }
}
