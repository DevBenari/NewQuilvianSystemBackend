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
    /// Implementasi Layanan Pembatasan Tugas Medis (Work Restriction) & Hard-Stop Roster Sub-06 (BE-HRD-291, VR-OHS-04..06, INT-HR-89)
    /// </summary>
    public class WorkRestrictionService : IWorkRestrictionService
    {
        private readonly IOccupationalHealthUnitOfWork _uow;

        public WorkRestrictionService(IOccupationalHealthUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<WorkRestrictionDto> IssueRestrictionAsync(IssueWorkRestrictionRequestDto request, Guid issuedByUserId, CancellationToken cancellationToken = default)
        {
            var employee = await _uow.Employees
                .FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken);
            if (employee == null)
                throw new KeyNotFoundException($"Pegawai dengan ID {request.EmployeeId} tidak ditemukan.");

            // Aturan VR-OHS-04: Masa berlaku surat pembatasan tugas kerja maksimal 180 hari kalender
            var durationDays = (request.EndDate.Date - request.StartDate.Date).TotalDays;
            if (durationDays <= 0)
                throw new ArgumentException("Tanggal akhir restriksi harus lebih besar dari tanggal mulai.");

            if (durationDays > 180)
                throw new InvalidOperationException($"Durasi surat pembatasan tugas ({durationDays} hari) melebihi batas maksimal regulasi 180 hari kalender (VR-OHS-04). Rekomendasikan evaluasi kelaikan permanen.");

            // Pastikan ada fitness to work record
            var fitnessId = request.FitnessToWorkId;
            if (!fitnessId.HasValue || fitnessId.Value == Guid.Empty)
            {
                var latestFitness = await _uow.FitnessToWorks
                    .Where(x => x.EmployeeId == request.EmployeeId)
                    .OrderByDescending(x => x.AssessmentDate)
                    .FirstOrDefaultAsync(cancellationToken);

                if (latestFitness != null)
                {
                    fitnessId = latestFitness.Id;
                }
                else
                {
                    // Buat baseline fitness to work fit with restriction
                    var newFt = new TrxEmployeeFitnessToWork
                    {
                        Id = Guid.NewGuid(),
                        WorkforceProfileId = employee.WorkforceProfileId,
                        EmployeeId = employee.Id,
                        AssessmentNumber = $"FTW-RES-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                        AssessmentDate = DateTime.UtcNow,
                        FitnessStatus = "FitWithRestriction",
                        EffectiveStartDate = request.StartDate,
                        EffectiveEndDate = request.EndDate,
                        WorkRestrictionRequired = true,
                        AdministrativeRestrictionSummary = request.Directives,
                        IsSchedulingAllowed = true,
                        IsClinicalDutyAllowed = false,
                        AssessedByProvider = !string.IsNullOrWhiteSpace(request.DoctorHiperkesRegNumber) 
                            ? $"Dokter Hiperkes RS (No. Reg: {request.DoctorHiperkesRegNumber})" 
                            : "Dokter Hiperkes RS",
                        ApprovedByUserId = issuedByUserId,
                        ApprovedAt = DateTime.UtcNow,
                        IsActive = true,
                        CreateBy = issuedByUserId,
                        CreateDateTime = DateTime.UtcNow,
                        UpdateBy = issuedByUserId,
                        UpdateDateTime = DateTime.UtcNow
                    };
                    await _uow.AddAsync(newFt, cancellationToken);
                    fitnessId = newFt.Id;
                }
            }

            var restriction = new TrxWorkRestriction
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = employee.WorkforceProfileId,
                EmployeeId = employee.Id,
                FitnessToWorkId = fitnessId.Value,
                RestrictionNumber = $"RST-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}",
                RestrictionType = request.RestrictionType,
                EffectiveStartDate = request.StartDate,
                EffectiveEndDate = request.EndDate,
                RestrictionStatus = "Active",
                AdministrativeInstruction = request.Directives,
                RestrictedActivity = $"Larangan: {(request.DisallowNightShift ? "[Shift Malam] " : "")}{(request.DisallowOnCall ? "[On-Call] " : "")}{(request.DisallowHeavyLifting ? "[Angkat Beban Berat] " : "")}{(request.DisallowHighRiskZone ? "[Zona Risiko Tinggi/Radiasi] " : "")}",
                AllowedActivity = "Tugas administratif, poliklinik rawat jalan, atau tugas medis ringan.",
                MaximumWorkHoursPerDay = request.MaxWorkHoursPerDay,
                NoNightShift = request.DisallowNightShift,
                NoOnCall = request.DisallowOnCall,
                NoHeavyLifting = request.DisallowHeavyLifting,
                RequiresWorkplaceAdjustment = request.DisallowHighRiskZone,
                IsSchedulingBlocked = request.DisallowNightShift || request.DisallowOnCall,
                IsClinicalServiceBlocked = request.DisallowHighRiskZone,
                ApprovedByUserId = issuedByUserId,
                ApprovedAt = DateTime.UtcNow,
                IsActive = true,
                CreateBy = issuedByUserId,
                CreateDateTime = DateTime.UtcNow,
                UpdateBy = issuedByUserId,
                UpdateDateTime = DateTime.UtcNow
            };

            await _uow.AddAsync(restriction, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            return new WorkRestrictionDto
            {
                Id = restriction.Id,
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                RestrictionNumber = restriction.RestrictionNumber,
                RestrictionType = restriction.RestrictionType,
                EffectiveStartDate = restriction.EffectiveStartDate,
                EffectiveEndDate = restriction.EffectiveEndDate,
                RestrictionStatus = restriction.RestrictionStatus,
                AdministrativeInstruction = restriction.AdministrativeInstruction,
                NoNightShift = restriction.NoNightShift,
                NoOnCall = restriction.NoOnCall,
                NoHeavyLifting = restriction.NoHeavyLifting,
                IsSchedulingBlocked = restriction.IsSchedulingBlocked
            };
        }

        public async Task<ScheduleValidationResultDto> ValidateScheduleAsync(Guid employeeId, DateTime shiftDate, string shiftType, Guid? unitId, CancellationToken cancellationToken = default)
        {
            var activeRestriction = await _uow.WorkRestrictions
                .Where(x => x.EmployeeId == employeeId && x.RestrictionStatus == "Active" && x.EffectiveStartDate <= shiftDate && x.EffectiveEndDate >= shiftDate)
                .OrderByDescending(x => x.EffectiveEndDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (activeRestriction == null)
            {
                return new ScheduleValidationResultDto
                {
                    EmployeeId = employeeId,
                    IsAllowed = true,
                    RejectionReason = null,
                    ActiveRestrictionNumber = null,
                    RestrictionEndDate = null
                };
            }

            // Validasi Hard-stop Sub-06 (VR-OHS-06, INT-HR-89)
            var isNightShift = shiftType.Contains("Malam", StringComparison.OrdinalIgnoreCase) || 
                               shiftType.Contains("Night", StringComparison.OrdinalIgnoreCase);

            var isOnCall = shiftType.Contains("OnCall", StringComparison.OrdinalIgnoreCase) || 
                           shiftType.Contains("Panggilan", StringComparison.OrdinalIgnoreCase);

            if (activeRestriction.NoNightShift && isNightShift)
            {
                return new ScheduleValidationResultDto
                {
                    EmployeeId = employeeId,
                    IsAllowed = false,
                    RejectionReason = $"[HARD-STOP K3RS INT-HR-89]: Staf memiliki surat pembatasan tugas aktif ({activeRestriction.RestrictionNumber}) yang melarang penugasan shift malam sampai {activeRestriction.EffectiveEndDate:dd/MM/yyyy}.",
                    ActiveRestrictionNumber = activeRestriction.RestrictionNumber,
                    RestrictionEndDate = activeRestriction.EffectiveEndDate
                };
            }

            if (activeRestriction.NoOnCall && isOnCall)
            {
                return new ScheduleValidationResultDto
                {
                    EmployeeId = employeeId,
                    IsAllowed = false,
                    RejectionReason = $"[HARD-STOP K3RS INT-HR-89]: Staf memiliki surat pembatasan tugas aktif ({activeRestriction.RestrictionNumber}) yang melarang penugasan on-call darurat sampai {activeRestriction.EffectiveEndDate:dd/MM/yyyy}.",
                    ActiveRestrictionNumber = activeRestriction.RestrictionNumber,
                    RestrictionEndDate = activeRestriction.EffectiveEndDate
                };
            }

            return new ScheduleValidationResultDto
            {
                EmployeeId = employeeId,
                IsAllowed = true,
                RejectionReason = null,
                ActiveRestrictionNumber = activeRestriction.RestrictionNumber,
                RestrictionEndDate = activeRestriction.EffectiveEndDate
            };
        }

        public async Task<List<WorkRestrictionDto>> GetActiveRestrictionsAsync(CancellationToken cancellationToken = default)
        {
            var restrictions = await _uow.WorkRestrictions
                .Where(x => x.RestrictionStatus == "Active" && x.EffectiveEndDate >= DateTime.UtcNow)
                .OrderBy(x => x.EffectiveEndDate)
                .ToListAsync(cancellationToken);

            var employeeIds = restrictions.Select(x => x.EmployeeId).Where(x => x.HasValue).Select(x => x!.Value).Distinct().ToList();
            var employees = await _uow.Employees.Where(x => employeeIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id, cancellationToken);

            return restrictions.Select(r => new WorkRestrictionDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId ?? Guid.Empty,
                EmployeeName = r.EmployeeId.HasValue && employees.TryGetValue(r.EmployeeId.Value, out var emp)
                    ? emp.FullName
                    : "Pegawai RS",
                RestrictionNumber = r.RestrictionNumber,
                RestrictionType = r.RestrictionType,
                EffectiveStartDate = r.EffectiveStartDate,
                EffectiveEndDate = r.EffectiveEndDate,
                RestrictionStatus = r.RestrictionStatus,
                AdministrativeInstruction = r.AdministrativeInstruction,
                NoNightShift = r.NoNightShift,
                NoOnCall = r.NoOnCall,
                NoHeavyLifting = r.NoHeavyLifting,
                IsSchedulingBlocked = r.IsSchedulingBlocked
            }).ToList();
        }

        public async Task<WorkRestrictionDto?> GetActiveRestrictionByEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
        {
            var r = await _uow.WorkRestrictions
                .Where(x => x.EmployeeId == employeeId && x.RestrictionStatus == "Active" && x.EffectiveEndDate >= DateTime.UtcNow)
                .OrderByDescending(x => x.EffectiveEndDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (r == null) return null;

            var emp = await _uow.Employees.FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken);

            return new WorkRestrictionDto
            {
                Id = r.Id,
                EmployeeId = employeeId,
                EmployeeName = emp != null ? emp.FullName : string.Empty,
                RestrictionNumber = r.RestrictionNumber,
                RestrictionType = r.RestrictionType,
                EffectiveStartDate = r.EffectiveStartDate,
                EffectiveEndDate = r.EffectiveEndDate,
                RestrictionStatus = r.RestrictionStatus,
                AdministrativeInstruction = r.AdministrativeInstruction,
                NoNightShift = r.NoNightShift,
                NoOnCall = r.NoOnCall,
                NoHeavyLifting = r.NoHeavyLifting,
                IsSchedulingBlocked = r.IsSchedulingBlocked
            };
        }
    }
}
