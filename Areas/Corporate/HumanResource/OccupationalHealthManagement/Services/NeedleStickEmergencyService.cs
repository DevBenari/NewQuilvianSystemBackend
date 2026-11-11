using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Models;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OccupationalHealthManagement.Services
{
    /// <summary>
    /// Implementasi Layanan Tanggap Darurat Tertusuk Jarum Cito & Tombol Panik ESS (BE-HRD-294, VR-OHS-08, INT-HR-91)
    /// </summary>
    public class NeedleStickEmergencyService : INeedleStickEmergencyService
    {
        private readonly IOccupationalHealthUnitOfWork _uow;

        public NeedleStickEmergencyService(IOccupationalHealthUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<NeedleStickReceiptDto> ReportCitoAsync(Guid employeeId, ReportNeedleStickCitoRequestDto request, CancellationToken cancellationToken = default)
        {
            var employee = await _uow.Employees
                .FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken);
            if (employee == null)
                throw new KeyNotFoundException($"Pegawai dengan ID {employeeId} tidak ditemukan.");

            var incidentNumber = $"NS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

            // Buat parent TrxOccupationalExposure terlebih dahulu
            var exposure = new TrxOccupationalExposure
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = employee.WorkforceProfileId,
                EmployeeId = employee.Id,
                ExposureNumber = $"EXP-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                ExposureDateTime = request.IncidentTimestamp,
                ExposureType = "NeedleStickSharpInjury",
                ExposureLocation = request.Location,
                SourceCategory = request.IsSourceKnown ? $"Pasien MRN: {request.SourcePatientMrn ?? "Teridentifikasi"}" : "Sumber Tidak Diketahui",
                ExposureRoute = "Percutaneous",
                BodySite = request.InjuryBodySite ?? "Jari Tangan",
                RiskLevel = "HighRisk",
                ExposureStatus = "PendingPEP",
                ClinicalNotesRestricted = AesClinicalEncryptionHelper.Encrypt($"Tindakan: {request.ProcedurePerformed}. Device: {request.DeviceType}. Pertolongan Pertama: {request.ImmediateFirstAid}"),
                FollowUpRequired = true,
                FollowUpDate = DateTime.UtcNow.AddMonths(1),
                IsReportableIncident = true,
                IsActive = true,
                CreateBy = employee.Id,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = employee.Id,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(exposure, cancellationToken);

            var incident = new TrxNeedleStickIncident
            {
                Id = Guid.NewGuid(),
                OccupationalExposureId = exposure.Id,
                WorkforceProfileId = employee.WorkforceProfileId,
                EmployeeId = employee.Id,
                IncidentNumber = incidentNumber,
                IncidentDateTime = request.IncidentTimestamp,
                DeviceType = request.DeviceType,
                ProcedurePerformed = request.ProcedurePerformed,
                IsHollowBoreNeedle = request.IsHollowBoreNeedle,
                WasVisibleBloodPresent = request.WasVisibleBloodPresent,
                IsSourceKnown = request.IsSourceKnown,
                SourceRiskCategory = request.IsSourceKnown ? "KnownSource" : "UnknownSource",
                InjuryBodySite = request.InjuryBodySite,
                ImmediateTreatment = request.ImmediateFirstAid ?? "Cuci luka dengan air mengalir dan sabun antiseptik.",
                PostExposureProphylaxisRecommended = true,
                PostExposureProphylaxisStarted = false,
                FollowUpDate = DateTime.UtcNow.AddMonths(1),
                IncidentStatus = "CitoReported",
                ConfidentialClinicalNotes = AesClinicalEncryptionHelper.Encrypt($"Laporan Tombol Panik ESS Tertusuk Jarum. Lokasi: {request.Location}"),
                IsActive = true,
                CreateBy = employee.Id,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = employee.Id,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(incident, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            // Hitung sisa countdown SLA 4 jam (14400 detik)
            var elapsed = DateTime.UtcNow - request.IncidentTimestamp;
            var remainingSeconds = Math.Max(0, (int)(14400 - elapsed.TotalSeconds));

            return new NeedleStickReceiptDto
            {
                IncidentId = incident.Id,
                IncidentNumber = incident.IncidentNumber,
                IncidentTimestamp = incident.IncidentDateTime,
                Status = "CitoReported",
                SlaPepCountdownSeconds = remainingSeconds,
                ActionDirective = "Peringatan darurat telah dikirim ke Tim K3RS & PPI. Segera laporkan fisik ke IGD/Depo Farmasi untuk pengambilan starter pack ARV/HBIG < 4 jam.",
                EmergencyContactPhone = "Ext-777 (Posko IGD K3RS RS MMC)"
            };
        }

        public async Task<PagedResult<NeedleStickIncidentDto>> GetNeedleStickQueueAsync(string? status, bool? isPepAdministered, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var query = _uow.NeedleStickIncidents.AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(x => x.IncidentStatus == status);

            if (isPepAdministered.HasValue)
                query = query.Where(x => x.PostExposureProphylaxisStarted == isPepAdministered.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.IncidentDateTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var empIds = items.Select(x => x.EmployeeId).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var employees = await _uow.Employees.Where(x => empIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);

            var dtos = items.Select(x => new NeedleStickIncidentDto
            {
                Id = x.Id,
                EmployeeId = x.EmployeeId ?? Guid.Empty,
                EmployeeName = x.EmployeeId.HasValue && employees.TryGetValue(x.EmployeeId.Value, out var emp)
                    ? emp.FullName
                    : "Pegawai RS",
                IncidentNumber = x.IncidentNumber,
                IncidentDateTime = x.IncidentDateTime,
                DeviceType = x.DeviceType,
                InjuryBodySite = x.InjuryBodySite,
                PostExposureProphylaxisRecommended = x.PostExposureProphylaxisRecommended,
                PostExposureProphylaxisStarted = x.PostExposureProphylaxisStarted,
                PepStartedAt = x.PostExposureProphylaxisStarted ? x.UpdateDateTime : null,
                IncidentStatus = x.IncidentStatus,
                FollowUpDate = x.FollowUpDate
            }).ToList();

            var totalPage = (int)Math.Ceiling((double)totalCount / pageSize);

            return new PagedResult<NeedleStickIncidentDto>
            {
                PageNumber = page,
                PageSize = pageSize,
                TotalData = totalCount,
                TotalPage = totalPage,
                Items = dtos
            };
        }

        public async Task<NeedleStickIncidentDto> AdministerPepAsync(Guid incidentId, AdministerPepRequestDto request, Guid doctorUserId, CancellationToken cancellationToken = default)
        {
            var incident = await _uow.NeedleStickIncidents
                .FirstOrDefaultAsync(x => x.Id == incidentId, cancellationToken);
            if (incident == null)
                throw new KeyNotFoundException($"Insiden tertusuk jarum dengan ID {incidentId} tidak ditemukan.");

            // Validasi Golden Hour < 4 jam (VR-OHS-09)
            var elapsed = request.PepAdministeredTime - incident.IncidentDateTime;
            if (elapsed.TotalHours > 72)
            {
                throw new InvalidOperationException("Pemberian profilaksis pasca pajanan (PEP) telah melewati batas mutlak efektivitas 72 jam.");
            }

            incident.PostExposureProphylaxisStarted = true;
            incident.IncidentStatus = "PepAdministered";
            incident.UpdateDateTime = DateTime.UtcNow;
            incident.UpdateBy = doctorUserId;
            incident.ConfidentialClinicalNotes = AesClinicalEncryptionHelper.Encrypt($"PEP: {request.PepRegimenDescription}. Catatan Dokter: {request.DoctorNotes}. Sampel Darah Baseline: {request.BaselineBloodSampleTaken}");
            _uow.Update(incident);

            // Update parent exposure juga
            var exposure = await _uow.OccupationalExposures
                .FirstOrDefaultAsync(x => x.Id == incident.OccupationalExposureId, cancellationToken);
            if (exposure != null)
            {
                exposure.ExposureStatus = "UnderSurveillance";
                exposure.UpdateDateTime = DateTime.UtcNow;
                exposure.UpdateBy = doctorUserId;
                _uow.Update(exposure);
            }

            await _uow.SaveChangesAsync(cancellationToken);

            var emp = incident.EmployeeId.HasValue 
                ? await _uow.Employees.FirstOrDefaultAsync(x => x.Id == incident.EmployeeId.Value, cancellationToken)
                : null;

            return new NeedleStickIncidentDto
            {
                Id = incident.Id,
                EmployeeId = incident.EmployeeId ?? Guid.Empty,
                EmployeeName = emp != null ? emp.FullName : "Pegawai RS",
                IncidentNumber = incident.IncidentNumber,
                IncidentDateTime = incident.IncidentDateTime,
                DeviceType = incident.DeviceType,
                InjuryBodySite = incident.InjuryBodySite,
                PostExposureProphylaxisRecommended = incident.PostExposureProphylaxisRecommended,
                PostExposureProphylaxisStarted = incident.PostExposureProphylaxisStarted,
                PepStartedAt = request.PepAdministeredTime,
                IncidentStatus = incident.IncidentStatus,
                FollowUpDate = incident.FollowUpDate
            };
        }
    }
}
