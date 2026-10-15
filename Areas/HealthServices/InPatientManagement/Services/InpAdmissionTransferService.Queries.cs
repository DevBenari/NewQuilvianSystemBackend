using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public partial class InpAdmissionTransferService
{
    private IQueryable<InpatientAdmissionTransferItemResponse> TransferQuery(string? sourceType)
    {
        var emergency = _dbContext.Set<EmgDisposition>().AsNoTracking()
            .Where(x => !x.IsDelete && !x.IsCancel && x.EmergencyVisit != null && !x.EmergencyVisit.IsDelete
                && !x.EmergencyVisit.IsCancel && x.EmergencyVisit.PatientId != null && x.DispositionType != null && !x.DispositionType.IsDelete
                && x.DispositionType.IsActive
                && (x.DispositionType.RequiresAdmissionRequest
                    ? _dbContext.Set<EmgAdmissionRequest>().Any(f => f.DispositionId == x.Id && !f.IsDelete
                        && f.Status == EmergencyAdmissionRequestStatus.Signed)
                    : x.DispositionType.Code == "RANAP")
                && x.DestinationServiceUnit != null && !x.DestinationServiceUnit.IsDelete
                && x.DestinationServiceUnit.ServiceUnitType == ServiceUnitType.Inpatient)
            .Select(x => new InpatientAdmissionTransferItemResponse
            {
                TransferId = x.Id, SourceType = "Emergency", EmergencyVisitId = x.EmergencyVisit!.Id,
                EmergencyVisitNumber = x.EmergencyVisit.EmergencyVisitNumber,
                SourceEncounterId = x.EmergencyVisit.EncounterId,
                SourceEncounterNumber = _dbContext.RegPatientEncounters.Where(e => e.Id == x.EmergencyVisit.EncounterId).Select(e => e.EncounterNumber).FirstOrDefault(),
                PatientId = x.EmergencyVisit.PatientId!.Value,
                PatientName = x.EmergencyVisit.Patient != null ? x.EmergencyVisit.Patient.FullName : "",
                MedicalRecordNumber = x.EmergencyVisit.Patient != null ? x.EmergencyVisit.Patient.MedicalRecordNumber : "",
                BirthDate = x.EmergencyVisit.Patient != null ? x.EmergencyVisit.Patient.BirthDate : null,
                DecidedByDoctorId = x.DecidedByDoctor != null && x.DecidedByDoctor.IsActive && !x.DecidedByDoctor.IsDelete ? x.DecidedByDoctorId : null,
                DecidedByDoctorName = x.DecidedByDoctor != null ? x.DecidedByDoctor.FullName : null,
                DispositionReason = x.DispositionReason, PatientCondition = x.PatientConditionAtDisposition,
                FollowUpInstruction = x.FollowUpInstruction, RecommendedServiceUnitId = x.DestinationServiceUnitId,
                RecommendedServiceUnitName = x.DestinationServiceUnit.ServiceUnitName, DecidedAt = x.DecidedAt,
                DispositionStatus = x.DispositionStatus == EmergencyDispositionStatus.Confirmed && x.ExecutedAt == null ? "Confirmed"
                    : x.DispositionStatus == EmergencyDispositionStatus.Executed || x.ExecutedAt != null ? "Executed"
                    : x.DispositionStatus == EmergencyDispositionStatus.Cancelled ? "Cancelled" : "Draft",
                ResultEpisodeId = _dbContext.Set<InpEpisode>().Where(e => e.Encounter != null
                    && e.Encounter.AdmissionTransferId == x.Id && e.Encounter.AdmissionTransferSourceType == "Emergency")
                    .Select(e => (Guid?)e.Id).FirstOrDefault()
            });

        var outpatient = from c in _dbContext.Set<CliDoctorCertificate>().AsNoTracking()
            join e in _dbContext.RegPatientEncounters on c.EncounterId equals e.Id
            join p in _dbContext.Set<MstPatient>() on c.PatientId equals p.Id
            where !c.IsDelete && c.CertificateType == DoctorCertificateType.InpatientReferral
                && e.PatientId == c.PatientId && !e.IsDelete && !e.IsCancel
                && e.EncounterType == EncounterType.Outpatient
                && e.EncounterStatus != EncounterStatus.Cancelled && e.EncounterStatus != EncounterStatus.NoShow
                && !p.IsDelete && !_dbContext.Set<EmgVisit>().Any(v => v.EncounterId == e.Id && !v.IsDelete)
            select new InpatientAdmissionTransferItemResponse
            {
                TransferId = c.Id, SourceType = "Outpatient", EmergencyVisitId = Guid.Empty,
                EmergencyVisitNumber = "", SourceEncounterId = e.Id, SourceEncounterNumber = e.EncounterNumber,
                PatientId = p.Id, PatientName = p.FullName, MedicalRecordNumber = p.MedicalRecordNumber,
                BirthDate = p.BirthDate,
                DecidedByDoctorId = _dbContext.Set<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models.MstDoctor>()
                    .Where(d => d.Id == c.DoctorId && d.IsActive && !d.IsDelete).Select(d => (Guid?)d.Id).FirstOrDefault(),
                DecidedByDoctorName = c.DoctorNameSnapshot,
                DispositionReason = c.ReferralReason, PatientCondition = c.ClinicalSummary,
                FollowUpInstruction = c.SpecialInstruction, RecommendedServiceUnitId = c.ReferralTargetServiceUnitId,
                RecommendedServiceUnitName = c.ReferralTargetNameSnapshot, DecidedAt = c.IssuedDate,
                DispositionStatus = c.CertificateStatus != DoctorCertificateStatus.Issued || c.IsCancel || !c.IsActive ? "Cancelled"
                    : _dbContext.RegPatientEncounters.IgnoreQueryFilters().Any(t => t.AdmissionTransferId == c.Id
                        && t.AdmissionTransferSourceType == "Outpatient") ? "Executed" : "Confirmed",
                ResultEpisodeId = _dbContext.Set<InpEpisode>().Where(t => t.Encounter != null
                    && t.Encounter.AdmissionTransferId == c.Id && t.Encounter.AdmissionTransferSourceType == "Outpatient")
                    .Select(t => (Guid?)t.Id).FirstOrDefault()
            };
        return sourceType == "Emergency" ? emergency : sourceType == "Outpatient" ? outpatient : emergency.Concat(outpatient);
    }
}
