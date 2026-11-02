using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
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
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;

public partial class InpAdmissionTransferService
{
    private IQueryable<InpatientAdmissionTransferItemResponse> TransferQuery(string? sourceType)
    {
        var today = DateTime.UtcNow.Date;

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
                TransferId = x.Id, SourceType = "Emergency", EmergencyVisitId = (Guid?)x.EmergencyVisit!.Id,
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
                    .Select(e => (Guid?)e.Id).FirstOrDefault(),

                // Kontrak 0.17.0 (RWI-DEC-338 s.d. 341):
                IsCito = _dbContext.Set<EmgAdmissionRequest>().Any(f =>
                    f.DispositionId == x.Id && !f.IsDelete
                    && f.Status == EmergencyAdmissionRequestStatus.Signed
                    && f.IsCito),
                IsPriorityMember = _dbContext.MstPatientMemberships.Any(m =>
                    m.PatientId == x.EmergencyVisit.PatientId && !m.IsDelete && m.IsActive
                    && m.MembershipStatus == MembershipStatus.Active
                    && (m.PeriodStartDate == null || m.PeriodStartDate <= today)
                    && (m.ExpiredDate == null || m.ExpiredDate >= today)
                    && m.MembershipTier != null && !m.MembershipTier.IsDelete && m.MembershipTier.IsActive
                    && m.MembershipTier.PriorityQueue),
                MemberTierName = _dbContext.MstPatientMemberships
                    .Where(m => m.PatientId == x.EmergencyVisit.PatientId && !m.IsDelete && m.IsActive
                        && m.MembershipStatus == MembershipStatus.Active
                        && (m.PeriodStartDate == null || m.PeriodStartDate <= today)
                        && (m.ExpiredDate == null || m.ExpiredDate >= today)
                        && m.MembershipTier != null && !m.MembershipTier.IsDelete && m.MembershipTier.IsActive
                        && m.MembershipTier.PriorityQueue)
                    .Select(m => m.MembershipTier!.TierName)
                    .FirstOrDefault(),
                MemberPriorityLevel = _dbContext.MstPatientMemberships
                    .Where(m => m.PatientId == x.EmergencyVisit.PatientId && !m.IsDelete && m.IsActive
                        && m.MembershipStatus == MembershipStatus.Active
                        && (m.PeriodStartDate == null || m.PeriodStartDate <= today)
                        && (m.ExpiredDate == null || m.ExpiredDate >= today)
                        && m.MembershipTier != null && !m.MembershipTier.IsDelete && m.MembershipTier.IsActive
                        && m.MembershipTier.PriorityQueue)
                    .Select(m => (int?)m.MembershipTier!.PriorityLevel)
                    .FirstOrDefault(),

                // PLAN-REPAIR-EPS-041 FIX-01: identitas, kebutuhan klinis, dan DPJP usulan dari permintaan
                // rawat inap IGD yang ditandatangani. Urutan anggota sama dengan proyeksi Poliklinik di bawah
                // karena keduanya digabung dengan Concat.
                Gender = x.EmergencyVisit.Patient == null ? null
                    : x.EmergencyVisit.Patient.Gender == Gender.Male ? "Laki-laki"
                    : x.EmergencyVisit.Patient.Gender == Gender.Female ? "Perempuan" : null,
                NationalId = x.EmergencyVisit.Patient != null ? x.EmergencyVisit.Patient.IdentityNumber : null,
                PhoneNumber = x.EmergencyVisit.Patient != null ? x.EmergencyVisit.Patient.PhoneNumber : null,
                NeedsIsolation = _dbContext.Set<EmgAdmissionRequest>().Any(f => f.DispositionId == x.Id && !f.IsDelete
                    && f.Status == EmergencyAdmissionRequestStatus.Signed && f.NeedsIsolation),
                NeedsIntensiveCare = _dbContext.Set<EmgAdmissionRequest>().Any(f => f.DispositionId == x.Id && !f.IsDelete
                    && f.Status == EmergencyAdmissionRequestStatus.Signed && f.NeedsIntensiveCare),
                NeedsOxygenSupport = _dbContext.Set<EmgAdmissionRequest>().Any(f => f.DispositionId == x.Id && !f.IsDelete
                    && f.Status == EmergencyAdmissionRequestStatus.Signed && f.NeedsOxygenSupport),
                NeedsSpecialMonitoring = _dbContext.Set<EmgAdmissionRequest>().Any(f => f.DispositionId == x.Id && !f.IsDelete
                    && f.Status == EmergencyAdmissionRequestStatus.Signed && f.NeedsSpecialMonitoring),
                EstimatedLengthOfStayDays = _dbContext.Set<EmgAdmissionRequest>()
                    .Where(f => f.DispositionId == x.Id && !f.IsDelete && f.Status == EmergencyAdmissionRequestStatus.Signed)
                    .OrderByDescending(f => f.SignedAt)
                    .Select(f => (int?)f.EstimatedLengthOfStayDays)
                    .FirstOrDefault(),
                ProposedInpatientDoctorId = _dbContext.Set<EmgAdmissionRequest>()
                    .Where(f => f.DispositionId == x.Id && !f.IsDelete && f.Status == EmergencyAdmissionRequestStatus.Signed
                        && f.ProposedInpatientDoctor != null && f.ProposedInpatientDoctor.IsActive && !f.ProposedInpatientDoctor.IsDelete)
                    .OrderByDescending(f => f.SignedAt)
                    .Select(f => f.ProposedInpatientDoctorId)
                    .FirstOrDefault(),
                ProposedInpatientDoctorName = _dbContext.Set<EmgAdmissionRequest>()
                    .Where(f => f.DispositionId == x.Id && !f.IsDelete && f.Status == EmergencyAdmissionRequestStatus.Signed
                        && f.ProposedInpatientDoctor != null)
                    .OrderByDescending(f => f.SignedAt)
                    .Select(f => f.ProposedInpatientDoctor!.FullName)
                    .FirstOrDefault(),
                Diagnosis = null,
                RequestedRoomClass = null
            });

        // Sumber Poliklinik hanya Surat Pengantar Rawat Inap; Surat Rujukan biasa bukan pengajuan admisi.
        // DecidedAt memakai waktu terbit sebenarnya (CreateDateTime), bukan tanggal kalender IssuedDate.
        var outpatient = from c in _dbContext.Set<CliDoctorCertificate>().AsNoTracking()
            join e in _dbContext.RegPatientEncounters on c.EncounterId equals e.Id
            join p in _dbContext.Set<MstPatient>() on c.PatientId equals p.Id
            where !c.IsDelete && c.CertificateType == DoctorCertificateType.InpatientAdmissionLetter
                && e.PatientId == c.PatientId && !e.IsDelete && !e.IsCancel
                && e.EncounterType == EncounterType.Outpatient
                && e.EncounterStatus != EncounterStatus.Cancelled && e.EncounterStatus != EncounterStatus.NoShow
                && !p.IsDelete && !_dbContext.Set<EmgVisit>().Any(v => v.EncounterId == e.Id && !v.IsDelete)
            select new InpatientAdmissionTransferItemResponse
            {
                TransferId = c.Id, SourceType = "Outpatient", EmergencyVisitId = (Guid?)null,
                EmergencyVisitNumber = "", SourceEncounterId = e.Id, SourceEncounterNumber = e.EncounterNumber,
                PatientId = p.Id, PatientName = p.FullName, MedicalRecordNumber = p.MedicalRecordNumber,
                BirthDate = p.BirthDate,
                DecidedByDoctorId = _dbContext.Set<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models.MstDoctor>()
                    .Where(d => d.Id == c.DoctorId && d.IsActive && !d.IsDelete).Select(d => (Guid?)d.Id).FirstOrDefault(),
                DecidedByDoctorName = c.DoctorNameSnapshot,
                DispositionReason = c.ReferralReason, PatientCondition = c.ClinicalSummary,
                FollowUpInstruction = c.SpecialInstruction, RecommendedServiceUnitId = c.ReferralTargetServiceUnitId,
                RecommendedServiceUnitName = c.ReferralTargetNameSnapshot, DecidedAt = c.CreateDateTime,
                DispositionStatus = c.CertificateStatus != DoctorCertificateStatus.Issued || c.IsCancel || !c.IsActive ? "Cancelled"
                    : _dbContext.RegPatientEncounters.IgnoreQueryFilters().Any(t => t.AdmissionTransferId == c.Id
                        && t.AdmissionTransferSourceType == "Outpatient") ? "Executed" : "Confirmed",
                ResultEpisodeId = _dbContext.Set<InpEpisode>().Where(t => t.Encounter != null
                    && t.Encounter.AdmissionTransferId == c.Id && t.Encounter.AdmissionTransferSourceType == "Outpatient")
                    .Select(t => (Guid?)t.Id).FirstOrDefault(),

                // Kontrak 0.17.0 (RWI-DEC-338 s.d. 341):
                IsCito = false,
                IsPriorityMember = _dbContext.MstPatientMemberships.Any(m =>
                    m.PatientId == p.Id && !m.IsDelete && m.IsActive
                    && m.MembershipStatus == MembershipStatus.Active
                    && (m.PeriodStartDate == null || m.PeriodStartDate <= today)
                    && (m.ExpiredDate == null || m.ExpiredDate >= today)
                    && m.MembershipTier != null && !m.MembershipTier.IsDelete && m.MembershipTier.IsActive
                    && m.MembershipTier.PriorityQueue),
                MemberTierName = _dbContext.MstPatientMemberships
                    .Where(m => m.PatientId == p.Id && !m.IsDelete && m.IsActive
                        && m.MembershipStatus == MembershipStatus.Active
                        && (m.PeriodStartDate == null || m.PeriodStartDate <= today)
                        && (m.ExpiredDate == null || m.ExpiredDate >= today)
                        && m.MembershipTier != null && !m.MembershipTier.IsDelete && m.MembershipTier.IsActive
                        && m.MembershipTier.PriorityQueue)
                    .Select(m => m.MembershipTier!.TierName)
                    .FirstOrDefault(),
                MemberPriorityLevel = _dbContext.MstPatientMemberships
                    .Where(m => m.PatientId == p.Id && !m.IsDelete && m.IsActive
                        && m.MembershipStatus == MembershipStatus.Active
                        && (m.PeriodStartDate == null || m.PeriodStartDate <= today)
                        && (m.ExpiredDate == null || m.ExpiredDate >= today)
                        && m.MembershipTier != null && !m.MembershipTier.IsDelete && m.MembershipTier.IsActive
                        && m.MembershipTier.PriorityQueue)
                    .Select(m => (int?)m.MembershipTier!.PriorityLevel)
                    .FirstOrDefault(),

                // PLAN-REPAIR-EPS-041 FIX-01: urutan anggota sama dengan proyeksi IGD. Surat pengantar
                // tidak memuat kebutuhan klinis IGD; yang dibawa adalah diagnosis dan kelas yang diminta.
                Gender = p.Gender == Gender.Male ? "Laki-laki" : p.Gender == Gender.Female ? "Perempuan" : null,
                NationalId = p.IdentityNumber,
                PhoneNumber = p.PhoneNumber,
                NeedsIsolation = false,
                NeedsIntensiveCare = false,
                NeedsOxygenSupport = false,
                NeedsSpecialMonitoring = false,
                EstimatedLengthOfStayDays = null,
                ProposedInpatientDoctorId = null,
                ProposedInpatientDoctorName = null,
                Diagnosis = c.ReferralDiagnosis ?? c.Diagnosis,
                RequestedRoomClass = c.RequestedRoomClass
            };
        return sourceType == "Emergency" ? emergency : sourceType == "Outpatient" ? outpatient : emergency.Concat(outpatient);
    }
}
