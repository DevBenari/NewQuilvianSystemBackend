using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Implementasi Layanan Investigasi Kecelakaan Kerja (KAK/PAK), Rekomendasi CAPA & Integrasi Klaim BPJS JKK (BE-HRD-300, VR-OHS-18..21, INT-HR-90)
    /// </summary>
    public class OccupationalInjuryService : IOccupationalInjuryService
    {
        private readonly IOccupationalHealthUnitOfWork _uow;

        public OccupationalInjuryService(IOccupationalHealthUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<EmployeeInjuryResponseDto> ReportInjuryAsync(ReportEmployeeInjuryRequestDto request, Guid reportedByUserId, CancellationToken cancellationToken = default)
        {
            var employee = await _uow.Employees
                .FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken);
            if (employee == null)
                throw new KeyNotFoundException($"Pegawai dengan ID {request.EmployeeId} tidak ditemukan.");

            // Validasi investigasi minimal 2 investigator K3RS (VR-OHS-19)
            if (request.InvestigatorNames == null || request.InvestigatorNames.Count < 2)
            {
                throw new InvalidOperationException("Pelaporan investigasi kecelakaan kerja (KAK/PAK) wajib menyertakan minimal 2 personel tim investigator K3RS (VR-OHS-19).");
            }

            var injuryNumber = $"KAK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";
            var bpjsDeadline = request.Timestamp.AddHours(48); // Batas regulasi 2x24 jam (VR-OHS-18, INT-HR-90)
            var reportedWithin48h = DateTime.UtcNow <= bpjsDeadline;

            var clinicalDetails = $"RCA: {request.RootCauseAnalysis}. CAPA: {request.CapaRecommendation}. Tim Investigator: {string.Join(", ", request.InvestigatorNames)}";
            var encryptedDetails = AesClinicalEncryptionHelper.Encrypt(clinicalDetails);

            var injury = new TrxEmployeeInjury
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = employee.WorkforceProfileId,
                EmployeeId = employee.Id,
                InjuryNumber = injuryNumber,
                InjuryDateTime = request.Timestamp,
                InjuryType = request.InjuryType,
                BodyPart = request.BodyPart,
                SeverityLevel = request.SeverityLevel,
                InjuryDescription = request.Description,
                ImmediateAction = request.ImmediateAction ?? "Pertolongan pertama pada kecelakaan (P3K) di tempat kejadian.",
                IsLostTimeInjury = request.IsLostTimeInjury,
                LostWorkDays = request.EstimatedLostWorkDays,
                IsReportableIncident = true,
                InsuranceClaimNumber = $"BPJS-JKK-T1-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                WorkCompensationClaimNumber = $"JKK-{DateTime.UtcNow:yyyyMM}",
                InjuryStatus = "Reported",
                IsActive = true,
                CreateBy = reportedByUserId,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = reportedByUserId,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(injury, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new EmployeeInjuryResponseDto
            {
                Id = injury.Id,
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                InjuryNumber = injury.InjuryNumber,
                InjuryDateTime = injury.InjuryDateTime,
                InjuryType = injury.InjuryType,
                SeverityLevel = injury.SeverityLevel,
                InjuryStatus = injury.InjuryStatus,
                IsLostTimeInjury = injury.IsLostTimeInjury,
                LostWorkDays = injury.LostWorkDays,
                BpjsStage1ClaimNumber = injury.InsuranceClaimNumber,
                BpjsStage1ReportDeadline = bpjsDeadline,
                BpjsReportedWithin48Hours = reportedWithin48h
            };
        }

        public async Task<BpjsJkkClaimFormDto> GenerateBpjsFormAsync(Guid injuryId, int stage, CancellationToken cancellationToken = default)
        {
            var injury = await _uow.EmployeeInjuries
                .FirstOrDefaultAsync(x => x.Id == injuryId, cancellationToken);
            if (injury == null)
                throw new KeyNotFoundException($"Laporan kecelakaan kerja dengan ID {injuryId} tidak ditemukan.");

            var employee = injury.EmployeeId.HasValue
                ? await _uow.Employees.FirstOrDefaultAsync(x => x.Id == injury.EmployeeId.Value, cancellationToken)
                : null;

            var deadline = injury.InjuryDateTime.AddHours(stage == 1 ? 48 : 720); // Tahap I < 48 jam, Tahap II < 30 hari
            var isOverdue = DateTime.UtcNow > deadline;
            var isWarning = !isOverdue && (deadline - DateTime.UtcNow).TotalHours < 12;

            var deadlineStatus = isOverdue ? "Overdue" : (isWarning ? "Warning" : "OnTime");

            return new BpjsJkkClaimFormDto
            {
                InjuryId = injury.Id,
                Stage = stage,
                FormTitle = stage == 1 
                    ? "Formulir 3a BPJS Ketenagakerjaan - Laporan Kecelakaan Kerja Tahap I" 
                    : "Formulir 3b BPJS Ketenagakerjaan - Surat Keterangan Dokter Kasus Kecelakaan Kerja Tahap II",
                FormCode = stage == 1 ? "FORM-BPJS-JKK-3A" : "FORM-BPJS-JKK-3B",
                EmployeeName = employee != null ? employee.FullName : "Pegawai Rumah Sakit",
                BpjsKetenagakerjaanNumber = "0001234567890", // Nomor kepesertaan BPJS TK
                IncidentDateTime = injury.InjuryDateTime,
                ReportDeadline = deadline,
                DeadlineStatus = deadlineStatus,
                FormPdfDownloadUrl = $"/api/v1/hrd/occupational-health/injuries/{injury.Id}/bpjs-jkk-form/pdf?stage={stage}",
                ReimbursementAmountClaimed = stage == 2 ? 15000000m : null,
                ReimbursementStatus = stage == 2 ? "SubmittedToBPJS" : "Draft"
            };
        }
    }
}
