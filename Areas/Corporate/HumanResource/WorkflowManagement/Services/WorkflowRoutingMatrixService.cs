using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforcePlanning.Constants;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services
{
    public interface IWorkflowRoutingMatrixService
    {
        Task<List<WorkflowMatrixStepConfigDto>> ResolveApprovalStepsAsync(string requestType, Guid departmentId, decimal? amount = null, Guid? requesterEmployeeId = null);
        Task<List<WorkflowMatrixResponseDto>> GetMatrixConfigurationsAsync(Guid? departmentId, string? requestType);
        Task<WorkflowMatrixResponseDto> ConfigureMatrixAsync(ConfigureWorkflowMatrixRequestDto request);
    }

    public class WorkflowRoutingMatrixService : IWorkflowRoutingMatrixService
    {
        private readonly ApplicationDbContext _context;

        public WorkflowRoutingMatrixService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<WorkflowMatrixStepConfigDto>> ResolveApprovalStepsAsync(
            string requestType, 
            Guid departmentId, 
            decimal? amount = null, 
            Guid? requesterEmployeeId = null)
        {
            var steps = new List<WorkflowMatrixStepConfigDto>();
            var normalizedType = requestType.Trim().ToUpperInvariant();

            // Aturan SLA Baku Rumah Sakit (HRD-DEC-185, VR-WF-04):
            // - Shift Swap / Tukar Shift: 24 Jam
            // - Leave / Cuti: 48 Jam
            // - Overtime / Lembur: 24 Jam
            // - Medical Claim / Restitusi: 72 Jam
            // - Business Travel / SPPD: 48 Jam
            // - Resignation / Pengunduran Diri: 72 Jam
            // - Training / Diklat: 48 Jam

            // Step 1: Atasan Langsung (Karu / Kepala Unit / Supervisor)
            steps.Add(new WorkflowMatrixStepConfigDto
            {
                StepOrder = 1,
                StepName = "Persetujuan Atasan Langsung (Karu / Supervisor)",
                ApproverRole = "DirectSupervisor",
                SlaHours = normalizedType.Contains("SHIFT") || normalizedType.Contains("OVERTIME") ? 24 : 48,
                IsRequired = true
            });

            // Step 2: Kepala Bidang / Head of Department (Untuk jenis tertentu atau threshold finansial)
            bool requiresDepartmentHead = normalizedType.Contains("RESIGN") ||
                                          normalizedType.Contains("TRAVEL") ||
                                          normalizedType.Contains("LOAN") ||
                                          (amount.HasValue && amount.Value > 5000000m);

            if (requiresDepartmentHead)
            {
                steps.Add(new WorkflowMatrixStepConfigDto
                {
                    StepOrder = steps.Count + 1,
                    StepName = "Persetujuan Kepala Bidang / Departemen (Kabid)",
                    ApproverRole = "HeadOfDepartment",
                    SlaHours = 48,
                    IsRequired = true
                });
            }

            // Step 3: Verifikasi HRD / SDM (Untuk cuti tahunan panjang, resign, mutasi, diklat)
            bool requiresHrd = normalizedType.Contains("RESIGN") ||
                               normalizedType.Contains("TRAINING") ||
                               normalizedType.Contains("LEAVE") ||
                               normalizedType.Contains("INCIDENT");

            if (requiresHrd)
            {
                steps.Add(new WorkflowMatrixStepConfigDto
                {
                    StepOrder = steps.Count + 1,
                    StepName = "Verifikasi & Validasi Bagian SDM / HRD",
                    ApproverRole = "HRDirector",
                    SlaHours = 48,
                    IsRequired = true
                });
            }

            // Step 4: Direksi / Keuangan (Untuk nominal besar > 25 juta atau pinjaman)
            if (amount.HasValue && amount.Value > 25000000m || normalizedType.Contains("LOAN"))
            {
                steps.Add(new WorkflowMatrixStepConfigDto
                {
                    StepOrder = steps.Count + 1,
                    StepName = "Pengesahan Direksi Keuangan / Direktur Utama",
                    ApproverRole = "FinanceDirector",
                    SlaHours = 72,
                    IsRequired = true
                });
            }

            return await Task.FromResult(steps);
        }

        public async Task<List<WorkflowMatrixResponseDto>> GetMatrixConfigurationsAsync(Guid? departmentId, string? requestType)
        {
            var result = new List<WorkflowMatrixResponseDto>();
            var normalizedRequestFilter = string.IsNullOrWhiteSpace(requestType)
                ? null
                : requestType.Trim();

            // 1. Baca konfigurasi definisi workflow & step aktif dari database (termasuk T10 Batch Formasi Awal / Rencana Tahunan).
            var definitions = await _context.Set<MstWorkflowDefinition>()
                .AsNoTracking()
                .Include(d => d.HospitalSite)
                .Include(d => d.WorkflowSteps.Where(s => s.IsActive && !s.IsDelete))
                    .ThenInclude(s => s.ApproverPosition)
                .Where(d => d.IsActive &&
                            !d.IsDelete &&
                            d.WorkflowStatus == "Active" &&
                            d.WorkflowSteps.Any(s => s.IsActive && !s.IsDelete))
                .OrderBy(d => d.WorkflowCode)
                .ThenByDescending(d => d.Version)
                .ToListAsync();

            foreach (var def in definitions)
            {
                var mappedSteps = def.WorkflowSteps
                    .Where(s => s.IsActive && !s.IsDelete)
                    .OrderBy(s => s.StepOrder)
                    .Select(s => new WorkflowMatrixStepConfigDto
                    {
                        StepOrder = s.StepOrder,
                        StepName = s.StepName,
                        ApproverRole = !string.IsNullOrWhiteSpace(s.ApproverRoleCode)
                            ? s.ApproverRoleCode
                            : (s.ApproverPosition?.PositionName ?? s.ApproverSourceType),
                        SlaHours = s.EscalationAfterHours ?? s.ReminderAfterHours ?? 48,
                        IsRequired = s.IsRequired
                    })
                    .ToList();

                var exposedRequestTypes = new List<string> { def.RequestType };
                if (string.Equals(def.WorkflowCode, WorkforcePlanningValueConstants.WorkflowDefinitionCodes.ManpowerPlanAndInitialFormation, StringComparison.OrdinalIgnoreCase))
                {
                    exposedRequestTypes = new List<string>
                    {
                        WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceInitialFormationBatch,
                        WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceManpowerPlan
                    };
                }
                else if (string.Equals(def.WorkflowCode, WorkforcePlanningValueConstants.WorkflowDefinitionCodes.HeadcountWithinFormation, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(def.WorkflowCode, WorkforcePlanningValueConstants.WorkflowDefinitionCodes.HeadcountBeyondFormation, StringComparison.OrdinalIgnoreCase))
                {
                    if (!exposedRequestTypes.Contains(WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceHeadcountRequest, StringComparer.OrdinalIgnoreCase))
                    {
                        exposedRequestTypes.Add(WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceHeadcountRequest);
                    }
                }

                var scopeLabel = def.HospitalSite != null
                    ? $"{def.WorkflowCode} — {def.HospitalSite.SiteName}"
                    : $"{def.WorkflowCode} — Seluruh Unit / Global";

                foreach (var exposedType in exposedRequestTypes.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrWhiteSpace(normalizedRequestFilter) &&
                        !string.Equals(normalizedRequestFilter, exposedType, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    result.Add(new WorkflowMatrixResponseDto
                    {
                        Id = def.Id,
                        DepartmentId = def.OrganizationUnitId,
                        DepartmentName = scopeLabel,
                        RequestType = exposedType,
                        MinAmount = null,
                        MaxAmount = null,
                        Steps = mappedSteps,
                        IsActive = def.IsActive
                    });
                }
            }

            // 2. Sertakan konfigurasi per departemen untuk jenis permohonan operasional umum.
            var query = _context.MstDepartments.AsNoTracking().Where(d => d.IsActive);
            if (departmentId.HasValue)
            {
                query = query.Where(d => d.Id == departmentId.Value);
            }

            var departments = await query.Take(10).ToListAsync();

            var defaultTypes = new[] { "Leave", "Overtime", "ShiftSwap", "MedicalClaim", "BusinessTravel", "Resignation" };
            if (!string.IsNullOrWhiteSpace(normalizedRequestFilter))
            {
                var isWorkforcePlanningFilter =
                    string.Equals(normalizedRequestFilter, WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceInitialFormationBatch, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(normalizedRequestFilter, WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceManpowerPlan, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(normalizedRequestFilter, WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceStaffingStandard, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(normalizedRequestFilter, WorkforcePlanningValueConstants.WorkflowReferenceTypes.WorkforceHeadcountRequest, StringComparison.OrdinalIgnoreCase);

                defaultTypes = isWorkforcePlanningFilter ? Array.Empty<string>() : new[] { normalizedRequestFilter };
            }

            foreach (var dept in departments)
            {
                foreach (var type in defaultTypes)
                {
                    var sampleSteps = await ResolveApprovalStepsAsync(type, dept.Id, 1000000m);
                    result.Add(new WorkflowMatrixResponseDto
                    {
                        Id = Guid.NewGuid(),
                        DepartmentId = dept.Id,
                        DepartmentName = dept.DepartmentName,
                        RequestType = type,
                        MinAmount = 0,
                        MaxAmount = 50000000,
                        Steps = sampleSteps,
                        IsActive = true
                    });
                }
            }

            return result;
        }

        public async Task<WorkflowMatrixResponseDto> ConfigureMatrixAsync(ConfigureWorkflowMatrixRequestDto request)
        {
            string deptName = "Seluruh Unit / Global";
            if (request.DepartmentId.HasValue)
            {
                var dept = await _context.MstDepartments.FindAsync(request.DepartmentId.Value);
                if (dept != null)
                {
                    deptName = dept.DepartmentName;
                }
            }

            return new WorkflowMatrixResponseDto
            {
                Id = Guid.NewGuid(),
                DepartmentId = request.DepartmentId,
                DepartmentName = deptName,
                RequestType = request.RequestType,
                MinAmount = request.MinAmount,
                MaxAmount = request.MaxAmount,
                Steps = request.Steps,
                IsActive = true
            };
        }
    }
}
