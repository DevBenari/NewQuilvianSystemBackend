using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
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
            var query = _context.MstDepartments.AsNoTracking().Where(d => d.IsActive);
            if (departmentId.HasValue)
            {
                query = query.Where(d => d.Id == departmentId.Value);
            }

            var departments = await query.Take(10).ToListAsync();
            var result = new List<WorkflowMatrixResponseDto>();

            var defaultTypes = new[] { "Leave", "Overtime", "ShiftSwap", "MedicalClaim", "BusinessTravel", "Resignation" };
            if (!string.IsNullOrWhiteSpace(requestType))
            {
                defaultTypes = new[] { requestType.Trim() };
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
