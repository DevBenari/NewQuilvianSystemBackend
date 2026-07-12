using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services
{
    public class ConsultationValidationService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly PrescriptionValidationService _prescriptionValidationService;

        public ConsultationValidationService(ApplicationDbContext dbContext, PrescriptionValidationService prescriptionValidationService)
        {
            _dbContext = dbContext;
            _prescriptionValidationService = prescriptionValidationService;
        }

        public async Task<ConsultationFinalizationValidationResponse> ValidateAsync(Guid consultationId, CancellationToken cancellationToken = default)
        {
            var consultation = await _dbContext.Set<TrxDoctorConsultation>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == consultationId && !x.IsDelete, cancellationToken);

            var issues = new List<ConsultationFinalizationIssueResponse>();

            if (consultation == null)
            {
                issues.Add(Issue("CONSULTATION_NOT_FOUND", ConsultationValidationSeverity.Error, "Konsultasi dokter tidak ditemukan.", "Consultation", "soap"));
                return Build(consultationId, issues);
            }

            if (consultation.ConsultationStatus == DoctorConsultationStatus.Completed)
                issues.Add(Issue("CONSULTATION_ALREADY_COMPLETED", ConsultationValidationSeverity.Error, "Konsultasi dokter sudah diselesaikan.", "Consultation", "soap"));
            if (consultation.ConsultationStatus == DoctorConsultationStatus.Cancelled)
                issues.Add(Issue("CONSULTATION_CANCELLED", ConsultationValidationSeverity.Error, "Konsultasi dokter sudah dibatalkan.", "Consultation", "soap"));

            ValidateSoap(consultation, issues);
            ValidateDiagnosis(consultation, issues);
            issues.AddRange(await _prescriptionValidationService.ValidateForConsultationAsync(consultationId, cancellationToken));
            await ValidateProceduresAsync(consultationId, issues, cancellationToken);

            return Build(consultationId, issues);
        }

        private static void ValidateSoap(TrxDoctorConsultation c, List<ConsultationFinalizationIssueResponse> issues)
        {
            if (string.IsNullOrWhiteSpace(c.Subjective) && string.IsNullOrWhiteSpace(c.ChiefComplaint))
                issues.Add(Issue("MISSING_SUBJECTIVE", ConsultationValidationSeverity.Error, "Subjective atau keluhan utama wajib diisi.", "SOAP", "soap", "Subjective"));
            if (string.IsNullOrWhiteSpace(c.Objective) && string.IsNullOrWhiteSpace(c.PhysicalExamination))
                issues.Add(Issue("MISSING_OBJECTIVE", ConsultationValidationSeverity.Error, "Objective atau pemeriksaan fisik wajib diisi.", "SOAP", "soap", "Objective"));
            if (string.IsNullOrWhiteSpace(c.Assessment))
                issues.Add(Issue("MISSING_ASSESSMENT", ConsultationValidationSeverity.Error, "Assessment wajib diisi.", "SOAP", "soap", "Assessment"));
            if (string.IsNullOrWhiteSpace(c.Plan))
                issues.Add(Issue("MISSING_PLAN", ConsultationValidationSeverity.Error, "Plan wajib diisi.", "SOAP", "soap", "Plan"));
        }

        private static void ValidateDiagnosis(TrxDoctorConsultation c, List<ConsultationFinalizationIssueResponse> issues)
        {
            if (!c.HasPrimaryDiagnosis || c.DiagnosisCount <= 0)
                issues.Add(Issue("MISSING_PRIMARY_DIAGNOSIS", ConsultationValidationSeverity.Error, "Diagnosis utama wajib tersedia sebelum konsultasi diselesaikan.", "Diagnosis", "diagnosis"));
        }

        private async Task ValidateProceduresAsync(Guid consultationId, List<ConsultationFinalizationIssueResponse> issues, CancellationToken cancellationToken)
        {
            var procedures = await _dbContext.Set<TrxPatientProcedure>()
                .AsNoTracking()
                .Where(x => x.ConsultationId == consultationId && !x.IsDelete && !x.IsCancel && x.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var item in procedures)
            {
                if (item.Quantity <= 0)
                    issues.Add(Issue("INVALID_PROCEDURE_QUANTITY", ConsultationValidationSeverity.Error, $"Jumlah tindakan {item.ProcedureNameSnapshot} harus lebih dari 0.", "Procedure", "procedure", "Quantity", "PatientProcedure", item.Id));
                if (item.IsBillable && !item.IsFreeOfCharge && !item.TariffId.HasValue)
                    issues.Add(Issue("MISSING_PROCEDURE_TARIFF", ConsultationValidationSeverity.Error, $"Tarif tindakan {item.ProcedureNameSnapshot} belum tersedia.", "Procedure", "procedure", "TariffId", "PatientProcedure", item.Id));
                if (item.IsNeedApproval && !item.IsApproved)
                    issues.Add(Issue("UNAPPROVED_PROCEDURE", ConsultationValidationSeverity.Error, $"Tindakan {item.ProcedureNameSnapshot} membutuhkan approval.", "Procedure", "procedure", null, "PatientProcedure", item.Id));
            }
        }

        private static ConsultationFinalizationValidationResponse Build(Guid consultationId, List<ConsultationFinalizationIssueResponse> issues)
        {
            var sections = issues
                .GroupBy(x => new { x.Section, x.TabKey })
                .Select(g => new ConsultationFinalizationSectionResponse
                {
                    Section = g.Key.Section,
                    TabKey = g.Key.TabKey,
                    ErrorCount = g.Count(x => x.Severity == ConsultationValidationSeverity.Error),
                    WarningCount = g.Count(x => x.Severity == ConsultationValidationSeverity.Warning),
                    InformationCount = g.Count(x => x.Severity == ConsultationValidationSeverity.Information),
                    Issues = g.ToList()
                })
                .ToList();

            var errors = issues.Count(x => x.Severity == ConsultationValidationSeverity.Error);
            var warnings = issues.Count(x => x.Severity == ConsultationValidationSeverity.Warning);

            return new ConsultationFinalizationValidationResponse
            {
                ConsultationId = consultationId,
                CanFinalize = errors == 0,
                RequiresWarningAcknowledgement = warnings > 0,
                ErrorCount = errors,
                WarningCount = warnings,
                InformationCount = issues.Count(x => x.Severity == ConsultationValidationSeverity.Information),
                Sections = sections
            };
        }

        private static ConsultationFinalizationIssueResponse Issue(string code, ConsultationValidationSeverity severity, string message, string section, string tabKey, string? field = null, string? entityType = null, Guid? entityId = null)
        {
            return new ConsultationFinalizationIssueResponse
            {
                Code = code,
                Severity = severity,
                Message = message,
                Section = section,
                TabKey = tabKey,
                Field = field,
                EntityType = entityType,
                EntityId = entityId,
                IssueKey = $"{code}:{entityType ?? section}:{entityId?.ToString() ?? "general"}"
            };
        }
    }
}
